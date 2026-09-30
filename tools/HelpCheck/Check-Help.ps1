<#
.SYNOPSIS
    Static check of the K-BOT help content (slice 0000). Reads src\KBot.App\HelpContent, never
    runs the app, never builds.

.DESCRIPTION
    Errors (exit code 1):
      - a topic header without id / title / part, an unknown header key, a bad part
      - duplicate topic ids, a parent or a [x](topic:id) link that does not exist
      - a capture tag with a bad or duplicate id, or an unknown goto prefix
      - a tour whose topic does not exist, a tour step goto with an unknown prefix
      - a screens: key or a tour target: whose type (or control name) is not in src\
    Report (no error):
      - -Coverage: every Form / UserControl in KBot.App that no topic lists in screens:
        (minus the known inner pages and Debug windows in $CoverageSkip)
      - -Map: the topic tree per part, in reading order, with file names and capture counts

.EXAMPLE
    powershell -NoProfile -ExecutionPolicy Bypass -File tools\HelpCheck\Check-Help.ps1 -Coverage -Map
#>
[CmdletBinding()]
param(
    [switch]$Coverage,
    [switch]$Map
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$content = Join-Path $repo 'src\KBot.App\HelpContent'
$src = Join-Path $repo 'src'
$appDir = Join-Path $repo 'src\KBot.App'

# Pages that live INSIDE a window that has a topic (F1 walks up to it), Debug-only windows,
# the help's own windows. Add here only with that reason; a real operator window needs a topic.
$CoverageSkip = @(
    'CabNoteReceiptPage', 'DdfFileBrowser', 'DdfFisierPreview', 'DdfValoriPage', 'ReaderHostPreview',
    'XfaXmlPreview', 'RegulaPaginaEditor', 'OrdDocumentePage', 'OrdAtasamentePage', 'OrdBeneficiariPage',
    'StartupLauncherForm', 'PlaceholderView',
    'HelpForm', 'HelpCaptureForm', 'HelpCapturePromptForm', 'HelpCaptureOverlay', 'HelpTourBubble', 'HelpTourFrame'
)
$HeaderKeys = @('id', 'title', 'part', 'order', 'parent', 'screens', 'keywords')
$TourKeys = @('id', 'title', 'part', 'topic')
$Parts = @('contabil', 'avansat', 'director')
$GotoPrefix = '^(view:[a-z0-9_]+|menu:[a-z0-9_]+|setari:[a-z0-9_]+|help|help:[a-z0-9._-]+)$'

$errors = New-Object System.Collections.Generic.List[string]
function Add-Err([string]$msg) { $script:errors.Add($msg) }
function Rel([string]$p) { $p.Substring($content.Length + 1) }

function Read-Header([string]$path, [string[]]$allowed) {
    $lines = Get-Content -LiteralPath $path -Encoding UTF8
    $h = @{}
    if ($lines.Count -eq 0 -or $lines[0].Trim() -ne '---') { Add-Err "$(Rel $path): no header block"; return $null }
    $i = 1
    while ($i -lt $lines.Count -and $lines[$i].Trim() -ne '---') {
        $line = $lines[$i]
        $c = $line.IndexOf(':')
        if ($c -gt 0) {
            $k = $line.Substring(0, $c).Trim().ToLowerInvariant()
            $v = $line.Substring($c + 1).Trim()
            if ($allowed -notcontains $k) { Add-Err "$(Rel $path): unknown header key '$k'" }
            $h[$k] = $v
        }
        $i++
    }
    $h['__body'] = ($lines | Select-Object -Skip ($i + 1)) -join "`n"
    $h['__file'] = Rel $path
    return $h
}

# --- Topics
$topicFiles = Get-ChildItem -LiteralPath $content -Recurse -Filter *.md |
    Where-Object { $_.Name -ne 'README.md' -and $_.FullName -notlike "*\tours\*" }
$topics = @()
foreach ($f in $topicFiles) {
    $h = Read-Header $f.FullName $HeaderKeys
    if ($null -eq $h) { continue }
    foreach ($k in 'id', 'title', 'part') { if (-not $h[$k]) { Add-Err "$($h.__file): missing '$k'" } }
    if ($h.part -and $Parts -notcontains $h.part) { Add-Err "$($h.__file): part '$($h.part)' is not one of $($Parts -join ', ')" }
    $topics += $h
}
$ids = @{}
foreach ($t in $topics) {
    if ($ids.ContainsKey($t.id)) { Add-Err "duplicate id '$($t.id)' in $($t.__file) and $($ids[$t.id].__file)" } else { $ids[$t.id] = $t }
}
$captureIds = @{}
foreach ($t in $topics) {
    if ($t.parent -and -not $ids.ContainsKey($t.parent)) { Add-Err "$($t.__file): parent '$($t.parent)' does not exist" }
    foreach ($m in [regex]::Matches($t.__body, '\(topic:([^)\s]+)\)')) {
        if (-not $ids.ContainsKey($m.Groups[1].Value)) { Add-Err "$($t.__file): link to missing topic '$($m.Groups[1].Value)'" }
    }
    foreach ($m in [regex]::Matches($t.__body, '<!--\s*capture:(.*?)-->')) {
        $segments = $m.Groups[1].Value.Split('|')
        $cid = $segments[0].Trim()
        if ($cid -notmatch '^[a-z0-9-]+$') { Add-Err "$($t.__file): capture id '$cid' is not lower-case ASCII / digits / dashes" }
        if ($captureIds.ContainsKey($cid)) { Add-Err "$($t.__file): capture id '$cid' also in $($captureIds[$cid])" } else { $captureIds[$cid] = $t.__file }
        foreach ($p in $segments | Select-Object -Skip 1) {
            $c = $p.IndexOf(':')
            if ($c -lt 0) { Add-Err "$($t.__file): capture '$cid' has a part without 'key:'"; continue }
            $k = $p.Substring(0, $c).Trim(); $v = $p.Substring($c + 1).Trim()
            if (@('caption', 'goto', 'prepare') -notcontains $k) { Add-Err "$($t.__file): capture '$cid' unknown key '$k'" }
            if ($k -eq 'goto' -and $v -notmatch $GotoPrefix) { Add-Err "$($t.__file): capture '$cid' goto '$v' has an unknown form" }
        }
    }
}

# --- Tours
$tours = @()
$tourDir = Join-Path $content 'tours'
if (Test-Path $tourDir) {
    foreach ($f in Get-ChildItem -LiteralPath $tourDir -Filter *.md) {
        $h = Read-Header $f.FullName $TourKeys
        if ($null -eq $h) { continue }
        if ($h.topic -and -not $ids.ContainsKey($h.topic)) { Add-Err "$($h.__file): topic '$($h.topic)' does not exist" }
        if ($h.part -and $Parts -notcontains $h.part) { Add-Err "$($h.__file): part '$($h.part)' is not valid" }
        foreach ($m in [regex]::Matches($h.__body, '(?m)^goto:\s*(.+)$')) {
            $v = $m.Groups[1].Value.Trim()
            if ($v -notmatch $GotoPrefix) { Add-Err "$($h.__file): goto '$v' has an unknown form" }
        }
        $tours += $h
    }
}

# --- Screen keys and tour targets against the code
$vbFiles = Get-ChildItem -LiteralPath $src -Recurse -Filter *.vb |
    Where-Object { $_.FullName -notmatch '\\(obj|bin|_reference)\\' }
$typeFile = @{}
foreach ($f in $vbFiles) {
    foreach ($m in [regex]::Matches((Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8), '(?m)^\s*(?:Partial\s+)?(?:Public\s+|Friend\s+)?(?:Partial\s+)?(?:NotInheritable\s+|MustInherit\s+)?Class\s+([A-Za-z0-9_]+)')) {
        $n = $m.Groups[1].Value
        if (-not $typeFile.ContainsKey($n)) { $typeFile[$n] = New-Object System.Collections.Generic.List[string] }
        $typeFile[$n].Add($f.FullName)
    }
}
function Test-Key([string]$key, [string]$where) {
    $dot = $key.IndexOf('.')
    $type = if ($dot -lt 0) { $key } else { $key.Substring(0, $dot) }
    if (-not $typeFile.ContainsKey($type)) { Add-Err "${where}: type '$type' not found in src\"; return }
    if ($dot -lt 0) { return }
    $ctl = $key.Substring($dot + 1)
    $found = $false
    foreach ($p in $typeFile[$type]) {
        if ((Get-Content -LiteralPath $p -Raw -Encoding UTF8) -match "\b$([regex]::Escape($ctl))\.Name\s*=\s*""$([regex]::Escape($ctl))""") { $found = $true; break }
    }
    if (-not $found) { Add-Err "${where}: control '$ctl' not found in $type's files" }
}
$listed = @{}
foreach ($t in $topics) {
    if (-not $t.screens) { continue }
    foreach ($k in $t.screens.Split(',')) {
        $k = $k.Trim(); if (-not $k) { continue }
        Test-Key $k $t.__file
        $listed[$k.Split('.')[0]] = $true
    }
}
foreach ($tr in $tours) {
    foreach ($m in [regex]::Matches($tr.__body, '(?m)^target:\s*(.+)$')) { Test-Key $m.Groups[1].Value.Trim() $tr.__file }
}

# --- Source tags (slice 0000-13): <!-- slice: 0072, 0097 --> right after the header block of a
# topic (covers the text before the first ##) and right under every ## heading (topics and tour
# steps). Each id is a slice of the index in KBOT_STATUS.md (0048 or 0048-04) or with a worklog, a help sub-slice
# with a worklog (0000-13), or 'fara-felie' (work recorded in KBOT_STATUS_SLICELESS.md).
$statusIndex = Get-Content -LiteralPath (Join-Path $repo 'docs\worklog\KBOT_STATUS.md') -Raw -Encoding UTF8
$worklogDir = Join-Path $repo 'docs\worklog'
$knownSlice = @{}
function Test-SliceId([string]$id, [string]$where) {
    if ($script:knownSlice.ContainsKey($id)) { return }
    $ok = $false
    if ($id -eq 'fara-felie') { $ok = $true }
    elseif ($id -match '^0000-\d{2}$') { $ok = [bool](Get-ChildItem -LiteralPath $worklogDir -Filter "SLICE-$id-*.md") }
    elseif ($id -match '^(\d{4})(-\d{2})?$') {
        $ok = ($statusIndex -match "(?m)^\|\s*$($Matches[1])[\s|/-]") -or
              [bool](Get-ChildItem -LiteralPath $worklogDir -Filter "SLICE-$id*.md")
    }
    if ($ok) { $script:knownSlice[$id] = $true } else { Add-Err "${where}: slice '$id' is not in KBOT_STATUS.md (nor has a worklog)" }
}
function Test-SourceTags([string]$body, [string]$file, [bool]$needIntro) {
    $lines = $body -split "`r?`n"
    if ($needIntro) {
        $first = $lines | Where-Object { $_.Trim() } | Select-Object -First 1
        if ($first -notmatch '^\s*<!--\s*slice:') { Add-Err "${file}: no <!-- slice: ... --> right after the header" }
    }
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^## ') {
            $next = if ($i + 1 -lt $lines.Count) { $lines[$i + 1] } else { '' }
            if ($next -notmatch '^\s*<!--\s*slice:') { Add-Err "${file}: section '$($lines[$i].Substring(3).Trim())' has no <!-- slice: ... --> under it" }
        }
    }
    foreach ($m in [regex]::Matches($body, '<!--\s*slice:([^\r\n]*?)-->')) {
        $ids = $m.Groups[1].Value.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ }
        if (-not $ids) { Add-Err "${file}: empty slice tag" }
        foreach ($id in $ids) { Test-SliceId $id $file }
    }
}
foreach ($t in $topics) { Test-SourceTags $t.__body $t.__file $true }
foreach ($tr in $tours) { Test-SourceTags $tr.__body $tr.__file $false }

# --- Output
"Help check: $($topics.Count) topics, $($tours.Count) tours, $($captureIds.Count) capture tags."

if ($Coverage) {
    ''
    'Windows / user controls in KBot.App without a topic (screens:):'
    $appTypes = Get-ChildItem -LiteralPath $appDir -Recurse -Filter *.vb |
        Where-Object { $_.FullName -notmatch '\\(obj|bin|HarnessTests)\\' } |
        ForEach-Object {
            $raw = Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8
            if ($raw -match '(?m)^\s*Inherits\s+[A-Za-z0-9_.]*(Form|UserControl)\s*$') {
                $m = [regex]::Match($raw, '(?m)^\s*(?:Partial\s+)?(?:Public\s+|Friend\s+)?(?:Partial\s+)?Class\s+([A-Za-z0-9_]+)')
                if ($m.Success) { $m.Groups[1].Value }
            }
        } | Sort-Object -Unique
    $missing = $appTypes | Where-Object { -not $listed.ContainsKey($_) -and $CoverageSkip -notcontains $_ }
    if ($missing) { $missing | ForEach-Object { "  - $_" } } else { '  (none)' }
}

if ($Map) {
    foreach ($part in $Parts) {
        ''
        "== $part"
        $byParent = $topics | Where-Object { $_.part -eq $part } | Group-Object { $_.parent } -AsHashTable -AsString
        function Show-Level([string]$parent, [int]$depth) {
            $key = if ($parent) { $parent } else { '' }
            if (-not $byParent -or -not $byParent.ContainsKey($key)) { return }
            foreach ($t in $byParent[$key] | Sort-Object { [int]("0" + $_.order) }, { $_.title }) {
                $caps = [regex]::Matches($t.__body, '<!--\s*capture:').Count
                ('  ' * ($depth + 1)) + "$($t.id)  '$($t.title)'  [$($t.__file)]" + $(if ($caps) { "  $caps capt." } else { '' })
                Show-Level $t.id ($depth + 1)
            }
        }
        Show-Level '' 0
        $tours | Where-Object { $_.part -eq $part } | ForEach-Object { "  tour $($_.id) -> $($_.topic)  [$($_.__file)]" }
    }
}

''
if ($errors.Count -gt 0) {
    "ERRORS ($($errors.Count)):"
    $errors | ForEach-Object { "  $_" }
    exit 1
}
'No errors.'
exit 0
