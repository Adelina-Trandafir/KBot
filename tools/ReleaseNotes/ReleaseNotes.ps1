<#
================================================================================
  ReleaseNotes.ps1  (slice 0067-02 -- what changed, written at every release)

  Every release gets a short Romanian text saying what changed since the version
  before it. The text is written by an AI assistant (GitHub Copilot Chat in
  Visual Studio, or Claude) from the worklogs, never by this script: neither
  assistant can be started from a script, so the script prepares the request,
  puts it on the clipboard and waits for the section to appear in

      docs\release-notes\NOUTATI.md      one "## <version>" section per release

  Writing rules for the assistant: docs\release-notes\README.md.

  Actions
    Request  work out what changed since the previous version, build the request,
             save it to artifacts\release-notes-request.txt and copy it to the
             clipboard. Does nothing when the version already has its section.
    Wait     wait (console) until the section for -Version is written, stamp it.
             Output: $true = the section is there, $false = skipped.
    Text     output the notes as plain text for latest.json ("" when none). With
             -Since, every section above that version and up to -Version goes in:
             a client that jumps several versions is told about all of them.
    Record   the operator typed the notes by hand (push-update.ps1 -Notes): store
             them as this version's section when it has none.

  "Since when" = the newest section BELOW -Version that carries a
  <!-- release: utc=... --> marker. What changed = the worklogs
  (docs\worklog\SLICE-*.md) written or changed after that moment.

  Called by publish-release.ps1 (Request after the version question, Wait at the
  end) and by push-update.ps1 (Text; Request + Wait under -SkipBuild; Record).
  By hand:  .\tools\ReleaseNotes\ReleaseNotes.ps1 -Action Request -Version 1.1.0.7
            (-NoClipboard = only save the request file)
================================================================================
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Request', 'Wait', 'Text', 'Record', 'Installer')]
    [string] $Action,

    [Parameter(Mandatory = $true)]
    [string] $Version,

    # Text only: the version the clients have now (the server's "version").
    [string] $Since = '',

    # Record only: the text typed by the operator, one change per line.
    [string] $NotesText = '',

    # Request only: leave the clipboard alone (the request is still saved to the file).
    [switch] $NoClipboard,

    # Installer only: the Inno Setup include file to write, and how many versions it lists.
    [string] $OutFile = '',
    [int] $Count = 3
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# The update box is a message box: a long text pushes its buttons off the screen.
$MaxBullets      = 12
$MaxBulletLength = 160
$MaxCommits      = 40

$BulletMark = [string][char]0x2022
$Utf8NoBom  = New-Object System.Text.UTF8Encoding($false)
$Invariant  = [System.Globalization.CultureInfo]::InvariantCulture

# ==============================================================================
#  HELPERS
# ==============================================================================

function Write-NotesInfo { param([string]$m) Write-Host "[notes] $m" -ForegroundColor Cyan }
function Write-NotesOk   { param([string]$m) Write-Host "[notes] $m" -ForegroundColor Green }
function Write-NotesWarn { param([string]$m) Write-Host "[notes] $m" -ForegroundColor Yellow }

function Find-SolutionRoot {
    $dir = $PSScriptRoot
    while ($dir -and -not (Test-Path (Join-Path $dir 'KBot.sln'))) {
        $dir = Split-Path $dir -Parent
    }
    if (-not $dir) { throw "KBot.sln not found above $PSScriptRoot." }
    return $dir
}

function ConvertTo-KBotVersion {
    # Four parts, the missing ones 0 (as UpdatePolicy.Normalize): "1.1" = 1.1.0.0.
    param([string]$Text)
    $v = [version]$Text
    return [version]::new([Math]::Max($v.Major, 0), [Math]::Max($v.Minor, 0),
                          [Math]::Max($v.Build, 0), [Math]::Max($v.Revision, 0))
}

function Format-UtcStamp {
    param([datetime]$Utc)
    return $Utc.ToString('yyyy-MM-ddTHH:mm:ssZ', $Invariant)
}

function Format-LocalTime {
    param([datetime]$Utc)
    return $Utc.ToLocalTime().ToString('dd.MM.yyyy HH:mm', $Invariant)
}

function Read-NotesFile {
    # The file as lines; remembers its line ending so a rewrite keeps it.
    if (-not (Test-Path -LiteralPath $NotesPath)) {
        throw "Release notes file not found: $NotesPath"
    }
    $raw = [System.IO.File]::ReadAllText($NotesPath, [System.Text.Encoding]::UTF8)
    $script:NewLine = if ($raw.Contains("`r`n")) { "`r`n" } else { "`n" }
    return , [string[]]($raw -split "\r?\n")
}

function Write-NotesFile {
    param([string[]]$Lines)
    [System.IO.File]::WriteAllText($NotesPath, ($Lines -join $script:NewLine), $Utf8NoBom)
}

function New-NotesSection {
    # Lines $Start (the heading) .. $End - 1 of one "## <version>" section.
    param([string[]]$Lines, [int]$Start, [int]$End, [version]$SectionVersion)
    $utc        = $null
    $isBaseline = $false
    $ids        = New-Object System.Collections.Generic.List[string]
    $bullets    = New-Object System.Collections.Generic.List[string]
    $open       = $false   # the previous line was part of a bullet: an indented line continues it
    for ($i = $Start + 1; $i -lt $End; $i++) {
        $line = $Lines[$i]
        if ($line -match '^\s*<!--') {
            $open = $false
            $m = [regex]::Match($line, 'release:[^>]*?utc=([0-9T:\-]+Z)')
            if ($m.Success) {
                $utc = [datetime]::Parse($m.Groups[1].Value, $Invariant,
                    [System.Globalization.DateTimeStyles]'AssumeUniversal, AdjustToUniversal')
                if ($line -match 'release:[^>]*\bbaseline\b') { $isBaseline = $true }
            }
            $m = [regex]::Match($line, 'felii:\s*(.*?)\s*-->')
            if ($m.Success) {
                foreach ($id in ($m.Groups[1].Value -split '[,;\s]+')) {
                    if ($id -match '^\d{4}(-\d{2})?$') { $ids.Add($id) }
                }
            }
            continue
        }
        $m = [regex]::Match($line, '^\s*[-*]\s+(\S.*)$')
        if ($m.Success) {
            $bullets.Add($m.Groups[1].Value.Trim())
            $open = $true
        } elseif ($open -and $line -match '^\s+\S') {
            $bullets[$bullets.Count - 1] = $bullets[$bullets.Count - 1] + ' ' + $line.Trim()
        } else {
            $open = $false
        }
    }
    return [pscustomobject]@{
        Version    = $SectionVersion
        Heading    = ($Lines[$Start] -replace '^##s*', '').Trim()
        Start      = $Start
        End        = $End
        Utc        = $utc
        IsBaseline = $isBaseline
        Ids        = $ids.ToArray()
        Bullets    = $bullets.ToArray()
    }
}

function Get-NotesSections {
    # One object per "## <version>" heading, in file order. A "## " heading that is
    # not a version only closes the section above it.
    param([string[]]$Lines)
    $sections = New-Object System.Collections.Generic.List[object]
    $start = -1
    $ver   = $null
    for ($i = 0; $i -le $Lines.Length; $i++) {
        $isEnd     = ($i -eq $Lines.Length)
        $isHeading = (-not $isEnd) -and ($Lines[$i] -match '^##(?!#)')
        if (($isEnd -or $isHeading) -and $start -ge 0) {
            $sections.Add((New-NotesSection -Lines $Lines -Start $start -End $i -SectionVersion $ver))
            $start = -1
        }
        if ($isHeading) {
            $m = [regex]::Match($Lines[$i], '^##\s+(\d+(?:\.\d+){1,3})(?![\d.])')
            if ($m.Success) {
                $start = $i
                $ver   = ConvertTo-KBotVersion $m.Groups[1].Value
            }
        }
    }
    return , $sections
}

function Find-NotesSection {
    param($Sections, [version]$Wanted)
    foreach ($s in $Sections) {
        if ($s.Version -eq $Wanted) { return $s }
    }
    return $null
}

function Find-PreviousSection {
    # The newest version below $Wanted that says WHEN it was released.
    param($Sections, [version]$Wanted)
    $best = $null
    foreach ($s in $Sections) {
        if ($s.Version -lt $Wanted -and $s.Utc -and (-not $best -or $s.Version -gt $best.Version)) {
            $best = $s
        }
    }
    return $best
}

function Test-SectionWritten {
    # The starting point of the journal has no changes to list; any other section
    # counts as written only once it has at least one line.
    param($Section)
    return ($Section.IsBaseline -or @($Section.Bullets).Count -gt 0)
}

function Get-ChangedWorklogs {
    param([datetime]$SinceUtc)
    $dir = Join-Path $SolutionRoot 'docs\worklog'
    if (-not (Test-Path -LiteralPath $dir)) { return @() }
    return @(Get-ChildItem -LiteralPath $dir -Filter 'SLICE-*.md' -File |
             Where-Object { $_.LastWriteTimeUtc -gt $SinceUtc } |
             Sort-Object LastWriteTimeUtc)
}

function Get-GitLines {
    # Read-only git; no git or no repository = no lines, said on the console.
    param([string[]]$GitArgs)
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        Write-NotesWarn "git is not in PATH: the request goes without the commit list."
        return @()
    }
    # The calling scripts read $LASTEXITCODE after their own tools (dotnet, ISCC, sftp):
    # git's exit code must not be the one they find. (Get-Variable: under StrictMode
    # the variable itself does not exist before the session's first native command.)
    $callerExitCode = Get-Variable -Name LASTEXITCODE -Scope Global -ValueOnly -ErrorAction SilentlyContinue
    try {
        $out = & git -C $SolutionRoot @GitArgs
        if ($LASTEXITCODE -ne 0) {
            Write-NotesWarn "git $($GitArgs[0]) returned ${LASTEXITCODE}: the request goes without the commit list."
            return @()
        }
        return @($out | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    } catch {
        Write-NotesWarn "git $($GitArgs[0]) failed ($($_.Exception.Message)): the request goes without the commit list."
        return @()
    } finally {
        $global:LASTEXITCODE = $callerExitCode
    }
}

function Copy-RequestToClipboard {
    # Never fatal: without a clipboard (a remote or service session) the saved file is the way.
    param([string]$Text)
    if ($NoClipboard) {
        Write-NotesOk "The request is saved: $RequestPath (-NoClipboard: the clipboard was left alone)."
        return
    }
    try {
        Set-Clipboard -Value $Text
        Write-NotesOk "The request is ON THE CLIPBOARD (also saved: $RequestPath)."
    } catch {
        Write-NotesWarn "Clipboard not available ($($_.Exception.Message)). Open $RequestPath and copy it from there."
    }
}

function Show-Notes {
    param($Section)
    $bullets = @($Section.Bullets)
    Write-NotesOk "Notes for $($Section.Version) ($($bullets.Count) line(s)):"
    foreach ($b in $bullets) { Write-Host "    - $b" }
    if ($bullets.Count -gt $MaxBullets) {
        Write-NotesWarn "$($bullets.Count) lines; the update box is comfortable up to $MaxBullets."
    }
    $long = @($bullets | Where-Object { $_.Length -gt $MaxBulletLength })
    if ($long.Count -gt 0) {
        Write-NotesWarn "$($long.Count) line(s) longer than $MaxBulletLength characters."
    }
    $marked = @($bullets | Where-Object { $_.Contains('**') -or $_.Contains('`') -or $_.Contains('](') })
    if ($marked.Count -gt 0) {
        Write-NotesWarn "$($marked.Count) line(s) carry Markdown (**, backticks or links); the update box shows plain text."
    }
}

# ==============================================================================
#  ACTIONS
# ==============================================================================

function New-RequestText {
    param($Previous, $Sections)

    # Worklog id -> the version whose notes already list it (file order = newest first).
    $covered = @{}
    foreach ($s in $Sections) {
        if ($s.Version -lt $Target) {
            foreach ($id in $s.Ids) {
                if (-not $covered.ContainsKey($id)) { $covered[$id] = $s.Version.ToString() }
            }
        }
    }

    $logs    = @(Get-ChangedWorklogs -SinceUtc $Previous.Utc)
    $commits = @(Get-GitLines -GitArgs @('log', "--since=$(Format-UtcStamp $Previous.Utc)", '-n', "$MaxCommits", '--pretty=format:%h %s'))
    $prevText = "$($Previous.Version) ($(Format-LocalTime $Previous.Utc))"

    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine("Write the K-BOT release notes for version $Target, in Romanian.")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("1. Read docs/release-notes/README.md first: who reads the text, what is left out, the format.")
    [void]$sb.AppendLine("2. Read the worklogs listed below. They are the source. Describe ONLY what changed after version $prevText.")
    [void]$sb.AppendLine("3. Put this section in docs/release-notes/NOUTATI.md, above the first ""## "" heading, in exactly this shape")
    [void]$sb.AppendLine("   (if a ""## $Target"" heading is already there, replace that section):")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("## $Target ($((Get-Date).ToString('dd.MM.yyyy', $Invariant)))")
    [void]$sb.AppendLine("<!-- release: utc=$(Format-UtcStamp ([DateTime]::UtcNow)) -->")
    [void]$sb.AppendLine("<!-- felii: the worklog ids you used, comma separated (0097-02, 0000-25) -->")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("- one change per line, Romanian, plain words, WITHOUT diacritics (a b c only: the Setup/update window shows them as '?')")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("4. Edit ONLY docs/release-notes/NOUTATI.md. Do not build, run, test or commit anything.")
    [void]$sb.AppendLine()

    if ($logs.Count -eq 0) {
        [void]$sb.AppendLine("No worklog was written or changed since $prevText. Look at the commits below;")
        [void]$sb.AppendLine("if nothing the user can see changed, write the single line the README gives for that case.")
    } else {
        [void]$sb.AppendLine("Worklogs written or changed since $prevText -- $($logs.Count):")
        foreach ($f in $logs) {
            $line = "  docs/worklog/$($f.Name)   ($($f.LastWriteTime.ToString('dd.MM.yyyy HH:mm', $Invariant)))"
            if ($f.Name -match '^SLICE-(\d{4}(?:-\d{2})?)' -and $covered.ContainsKey($Matches[1])) {
                $line += "   [already listed under $($covered[$Matches[1]]): only what is new in it]"
            }
            [void]$sb.AppendLine($line)
        }
    }
    [void]$sb.AppendLine()
    if ($commits.Count -gt 0) {
        [void]$sb.AppendLine("Commits since then (newest first, at most $MaxCommits) -- only to cross-check the worklogs:")
        foreach ($c in $commits) { [void]$sb.AppendLine("  $c") }
    } else {
        [void]$sb.AppendLine("No commits since then (the work may still be uncommitted; the worklogs are the source).")
    }

    return [pscustomobject]@{ Text = $sb.ToString(); Worklogs = $logs.Count; Previous = $prevText }
}

function Invoke-NotesRequest {
    $lines    = Read-NotesFile
    $sections = Get-NotesSections -Lines $lines
    $own      = Find-NotesSection -Sections $sections -Wanted $Target
    if ($own -and (Test-SectionWritten $own)) {
        Write-NotesInfo "Version $Target already has its section in NOUTATI.md -- nothing to write."
        Write-NotesInfo "(To have it written again: delete that section, then run this build again.)"
        return
    }

    $previous = Find-PreviousSection -Sections $sections -Wanted $Target
    if (-not $previous) {
        throw ("NOUTATI.md has no section below $Target with a <!-- release: utc=... --> marker, " +
               "so 'since when' is unknown. See docs\release-notes\README.md.")
    }

    $request = New-RequestText -Previous $previous -Sections $sections
    New-Item -ItemType Directory -Path (Split-Path $RequestPath -Parent) -Force | Out-Null
    [System.IO.File]::WriteAllText($RequestPath, $request.Text, $Utf8NoBom)

    Write-NotesInfo "Release notes for ${Target}: $($request.Worklogs) worklog(s) changed since $($request.Previous)."
    Copy-RequestToClipboard -Text $request.Text
    Write-NotesInfo "Paste it NOW into Copilot Chat in Visual Studio (agent mode) or into Claude."
    Write-NotesInfo "The assistant writes docs\release-notes\NOUTATI.md while the build runs."
}

function Invoke-NotesWait {
    $interactive = [Environment]::UserInteractive -and -not [Console]::IsInputRedirected
    while ($true) {
        $lines    = Read-NotesFile
        $sections = Get-NotesSections -Lines $lines
        $own      = Find-NotesSection -Sections $sections -Wanted $Target
        if ($own -and (Test-SectionWritten $own)) {
            if (-not $own.Utc) {
                # Written without the marker: the next release needs to know "since when".
                $stamped = New-Object System.Collections.Generic.List[string]
                $stamped.AddRange([string[]]@($lines[0..$own.Start]))
                $stamped.Add("<!-- release: utc=$(Format-UtcStamp ([DateTime]::UtcNow)) -->")
                if ($own.Start + 1 -lt $lines.Length) {
                    $stamped.AddRange([string[]]@($lines[($own.Start + 1)..($lines.Length - 1)]))
                }
                Write-NotesFile -Lines $stamped.ToArray()
                Write-NotesInfo "Release marker added to the section of $Target."
            }
            if (-not $own.IsBaseline) { Show-Notes -Section $own }
            return $true
        }

        if (-not $interactive) {
            Write-NotesWarn "No notes for $Target in NOUTATI.md and no interactive console: not waiting."
            return $false
        }
        Write-NotesWarn "No notes for $Target in docs\release-notes\NOUTATI.md yet."
        $answer = Read-Host "[notes] Enter = look again, P = copy the request again, S = skip (release WITHOUT notes)"
        $key = ($answer -replace '\s', '').ToUpperInvariant()
        if ($key -eq 'S') {
            Write-NotesWarn "Skipped: version $Target has no notes."
            return $false
        }
        if ($key -eq 'P') {
            if (Test-Path -LiteralPath $RequestPath) {
                Copy-RequestToClipboard -Text ([System.IO.File]::ReadAllText($RequestPath, [System.Text.Encoding]::UTF8))
            } else {
                Invoke-NotesRequest
            }
        }
    }
}

function Get-NotesText {
    $lines    = Read-NotesFile
    $sections = Get-NotesSections -Lines $lines

    $floor = $null
    if (-not [string]::IsNullOrWhiteSpace($Since)) {
        $floor = ConvertTo-KBotVersion $Since
        # A forced re-push of the same or an older version: that version's own notes only.
        if ($floor -ge $Target) { $floor = $null }
    }
    $picked = @($sections | Where-Object {
            if ($floor) { $_.Version -gt $floor -and $_.Version -le $Target } else { $_.Version -eq $Target }
        } | Sort-Object Version -Descending)

    $out = New-Object System.Collections.Generic.List[string]
    foreach ($s in $picked) {
        foreach ($b in $s.Bullets) { $out.Add("$BulletMark $b") }
    }
    if ($out.Count -gt 2 * $MaxBullets) {
        Write-NotesWarn "$($out.Count) lines for the update box (several versions at once) -- consider shortening NOUTATI.md."
    }
    return ($out.ToArray() -join "`r`n")
}

function Invoke-NotesInstaller {
    # The "what is new" page of the Setup wizard (KBot.iss, NewsText): the newest $Count
    # versions up to $Target, as an Inno Setup include file (UTF-8 with BOM, so the
    # diacritics survive). No sections at all -> nothing is written and Setup has no page.
    if ([string]::IsNullOrWhiteSpace($OutFile)) { throw "-Action Installer needs -OutFile." }
    $lines    = Read-NotesFile
    $sections = Get-NotesSections -Lines $lines
    $picked   = @($sections | Where-Object { $_.Version -le $Target -and @($_.Bullets).Count -gt 0 } |
                  Sort-Object Version -Descending | Select-Object -First $Count)
    if ($picked.Count -eq 0) {
        if (Test-Path -LiteralPath $OutFile) { Remove-Item -LiteralPath $OutFile -Force }
        Write-NotesWarn "No notes to show in the installer: Setup will have no news page."
        return
    }
    $q  = [string][char]39
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine('// Generated by toolsReleaseNotesReleaseNotes.ps1 -Action Installer. Do not edit.')
    [void]$sb.AppendLine('function NewsText: String;')
    [void]$sb.AppendLine('begin')
    [void]$sb.AppendLine('  Result :=')
    $parts = New-Object System.Collections.Generic.List[string]
    foreach ($s in $picked) {
        $parts.Add("Versiunea $($s.Heading)")
        foreach ($b in $s.Bullets) { $parts.Add("$BulletMark $b") }
        $parts.Add('')
    }
    while ($parts.Count -gt 0 -and $parts[$parts.Count - 1] -eq '') { $parts.RemoveAt($parts.Count - 1) }
    for ($i = 0; $i -lt $parts.Count; $i++) {
        $text = $parts[$i].Replace($q, $q + $q)
        $glue = if ($i -lt $parts.Count - 1) { " + #13#10 +" } else { ';' }
        [void]$sb.AppendLine("    $q$text$q$glue")
    }
    [void]$sb.AppendLine('end;')
    $dir = Split-Path -Parent $OutFile
    if ($dir -and -not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    [System.IO.File]::WriteAllText($OutFile, $sb.ToString(), (New-Object System.Text.UTF8Encoding($true)))
    Write-NotesOk "Installer news page: $($picked.Count) version(s) -> $OutFile"
}

function Invoke-NotesRecord {
    if ([string]::IsNullOrWhiteSpace($NotesText)) { throw "-Action Record needs -NotesText." }
    $lines    = Read-NotesFile
    $sections = Get-NotesSections -Lines $lines
    $own      = Find-NotesSection -Sections $sections -Wanted $Target
    if ($own) {
        Write-NotesInfo "NOUTATI.md already has a section for $Target; it stays as it is (the typed text goes to latest.json only)."
        return
    }

    $bullets = @($NotesText -split "\r?\n" |
                 ForEach-Object { ($_ -replace '^\s*(?:[-*]|\u2022)\s*', '').Trim() } |
                 Where-Object { $_ })
    $block = New-Object System.Collections.Generic.List[string]
    $block.Add("## $Target ($((Get-Date).ToString('dd.MM.yyyy', $Invariant)))")
    $block.Add("<!-- release: utc=$(Format-UtcStamp ([DateTime]::UtcNow)) -->")
    $block.Add('<!-- felii: -->')
    $block.Add('')
    foreach ($b in $bullets) { $block.Add("- $b") }
    $block.Add('')

    # Newest first: above the first "## " heading; a file without one gets it at the end.
    $at = -1
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match '^##(?!#)') { $at = $i; break }
    }
    $result = New-Object System.Collections.Generic.List[string]
    if ($at -lt 0) {
        $result.AddRange($lines)
        $result.Add('')
        $result.AddRange($block)
    } else {
        if ($at -gt 0) { $result.AddRange([string[]]@($lines[0..($at - 1)])) }
        $result.AddRange($block)
        $result.AddRange([string[]]@($lines[$at..($lines.Length - 1)]))
    }
    Write-NotesFile -Lines $result.ToArray()
    Write-NotesOk "The typed notes were stored as the section of $Target in NOUTATI.md ($($bullets.Count) line(s))."
}

# ==============================================================================
#  MAIN
# ==============================================================================

$SolutionRoot = Find-SolutionRoot
$NotesPath    = Join-Path $SolutionRoot 'docs\release-notes\NOUTATI.md'
$RequestPath  = Join-Path $SolutionRoot 'artifacts\release-notes-request.txt'
$Target       = ConvertTo-KBotVersion $Version
$NewLine      = "`n"

switch ($Action) {
    'Request' { Invoke-NotesRequest }
    'Wait'    { Invoke-NotesWait }
    'Text'    { Get-NotesText }
    'Record'  { Invoke-NotesRecord }
    'Installer' { Invoke-NotesInstaller }
}
