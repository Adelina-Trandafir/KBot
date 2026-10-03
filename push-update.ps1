<#
================================================================================
  push-update.ps1  (slice 0067 -- application updates)

  Builds a RELEASE package (publish-release.ps1: publish, sign, zip, installer)
  and publishes it as THE update on the K-BOT server:

      <RemoteRoot>/updates/KBot_<version>.zip             the package (the Release zip)
      <RemoteRoot>/updates/KBot_<version>_noaccess.zip    ... for clients WITHOUT Access
      <RemoteRoot>/updates/latest.json                    what the server tells clients

  TWO KINDS OF PACKAGE (slice 0104). The Migrator and the Access code are only for clients
  that also run the Access application, so the server keeps two packages and latest.json has
  two blocks:   { "access": {...}, "non_access": {...} }
  The FIRST question of this script (before the build, before the signing one) says which kind
  this push is for; only that block is rewritten, the other one stays as the server has it
  (read back through the API). A client asks /api/update/latest?access=1|0 for its own kind.

  The client (KBot.App, Release build only) asks GET /api/update/latest at
  startup and from the "Cauta actualizari" buttons, compares its own
  KBot.App FileVersion with `version` / `minimum`, downloads /api/update/download
  and hands the zip to KBot.Updater.exe.

  Run it when YOU decide an update is viable, AFTER bumping <FileVersion> in
  src\KBot.App\KBot.App.vbproj by hand. The script refuses to push a version that
  is not newer than the one already on the server (override with -Force).

  Transport: Windows OpenSSH sftp.exe in batch mode, one session, ONE password
  prompt (typed at the OpenSSH prompt, never stored). Host / Port / User /
  RemoteRoot come from PYTHON\_push\push_settings.json, the same file
  AvacontPush uses. The package goes up as *.part and is renamed into place, and
  latest.json is written LAST, so a client can never read a version whose file
  is not fully there.

  What changed (slice 0067-02): the `notes` of latest.json -- the text the client
  reads in the update box -- come from docs\release-notes\NOUTATI.md, where an AI
  assistant (Copilot Chat in Visual Studio, or Claude) writes one Romanian section
  per version. publish-release.ps1 puts the request on the clipboard and waits for
  the section; this script then sends every section ABOVE the server's version and
  up to the new one, so a client that jumps several versions reads all of them.
  Rules and procedure: docs\release-notes\README.md.

  Parameters
    -Audience         Access / NonAccess / Ask (default: asks FIRST, before the signing
                      question): which kind of package this is. Forwarded to publish-release.ps1.
                      With -SkipBuild it picks which newest zip is pushed.
    -Mandatory        raise `minimum` of THIS kind to this version: every client of this kind
                      below it is forced to update (no "Mai tarziu"). Without it, `minimum`
                      stays what the server already had for it (or 0.0.0.0).
    -Notes "<text>"   your own text for the update dialog, instead of the
                      assistant's (one change per line). It is also stored as the
                      version's section in NOUTATI.md when that has none.
    -SkipBuild        do not build; push the newest artifacts\KBot_Release_*.zip.
    -Force            push even if the server already has this version or newer.
    -SignThumbprint   forwarded to publish-release.ps1.
    -Bump             forwarded to publish-release.ps1: Ask (default, one console
                      question), None, Major, Minor, Build, Revision.
    -Sign             forwarded to publish-release.ps1: Ask (default, one console
                      question before the version one), Yes, No (nothing signed,
                      no SimplySign confirmation).
    -SettingsPath     alternative push_settings.json.
    -ApiBaseUrl       where /api/update/latest is read from (default: production).
================================================================================
#>

[CmdletBinding()]
param(
    [ValidateSet('Ask', 'Access', 'NonAccess')]
    [string] $Audience = 'Ask',
    [switch] $Mandatory,
    [string] $Notes = '',
    [switch] $SkipBuild,
    [switch] $Force,
    [string] $SignThumbprint = '',
    [ValidateSet('Ask', 'None', 'Major', 'Minor', 'Build', 'Revision')]
    [string] $Bump = 'Ask',
    [ValidateSet('Ask', 'Yes', 'No')]
    [string] $Sign = 'Ask',
    [string] $SettingsPath = '',
    [string] $ApiBaseUrl = 'https://kbot.avatarsoft.ro'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# ==============================================================================
#  HELPERS
# ==============================================================================

function Write-Step { param([string]$m) Write-Host "[update] $m" -ForegroundColor Cyan }
function Write-Ok   { param([string]$m) Write-Host "[update] $m" -ForegroundColor Green }

# The first question (slice 0104), same wording as publish-release.ps1. Enter = Access.
function Read-AudienceChoice {
    if (-not [Environment]::UserInteractive -or [Console]::IsInputRedirected) {
        Write-Host "Audience: no interactive console -> Access (pass -Audience NonAccess for the other package)." -ForegroundColor Yellow
        return 'Access'
    }
    while ($true) {
        $answer = Read-Host "For whom is this push? (A)ccess = with Migrator + Access code / (N)on-access = without [A]"
        $key = ($answer -replace '[\s-]', '').ToUpperInvariant()
        if ($key -in @('', 'A', 'ACCESS')) { return 'Access' }
        if ($key -in @('N', 'NONACCESS')) { return 'NonAccess' }
        Write-Host "  Type A (access) or N (non-access); Enter = A." -ForegroundColor Yellow
    }
}

function Find-SolutionRoot {
    $dir = $PSScriptRoot
    while ($dir -and -not (Test-Path (Join-Path $dir 'KBot.sln'))) {
        $dir = Split-Path $dir -Parent
    }
    if (-not $dir) { throw "KBot.sln not found above $PSScriptRoot." }
    return $dir
}

function Read-PushSettings {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "push_settings.json not found: $Path  (copy PYTHON\AvacontPush\push_settings.example.json and fill Host/Port/User/RemoteRoot)."
    }
    $s = Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($k in 'Host', 'User', 'RemoteRoot') {
        if (-not ($s.PSObject.Properties.Name -contains $k) -or [string]::IsNullOrWhiteSpace([string]$s.$k)) {
            throw "push_settings.json: '$k' is missing or empty."
        }
    }
    $port = 22
    if ($s.PSObject.Properties.Name -contains 'Port' -and $s.Port) { $port = [int]$s.Port }
    return [pscustomobject]@{
        Host       = [string]$s.Host
        Port       = $port
        User       = [string]$s.User
        RemoteRoot = ([string]$s.RemoteRoot).TrimEnd('/')
    }
}

function Resolve-Sftp {
    $cmd = Get-Command sftp.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $fallback = Join-Path $env:SystemRoot 'System32\OpenSSH\sftp.exe'
    if (Test-Path -LiteralPath $fallback) { return $fallback }
    throw "sftp.exe not found. Enable the Windows optional feature 'OpenSSH Client'."
}

# Reads the KBot.App FileVersion from INSIDE the zip -- the package is the truth,
# not the .vbproj on disk. Also checks the updater is in there: without it the
# client has nothing to apply the package with.
function Get-PackageInfo {
    param([string]$ZipPath, [bool]$IncludeAccess)
    if (-not ('System.IO.Compression.ZipFile' -as [type])) {
        Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
    }
    $zip = [System.IO.Compression.ZipFile]::OpenRead($ZipPath)
    try {
        $entries = @($zip.Entries)
        # A zip written under Windows PowerShell 5.1 by ZipFile::CreateFromDirectory
        # carries BACKSLASHES in its entry names. publish-release.ps1 no longer does
        # that (20.09.2026), and such a package is refused here: the client's updater
        # strips the top folder by "/" and would unpack it into a subfolder of C:\KBOT.
        $backslashed = @($entries | Where-Object { $_.FullName.Contains('\') })
        if ($backslashed.Count -gt 0) {
            throw ("Package entries use backslashes ({0}); rebuild it with the current publish-release.ps1 -- " +
                   "KBot.Updater would unpack it into a subfolder.") -f $backslashed[0].FullName
        }
        $app = $entries | Where-Object { $_.FullName -match '(^|/)KBot\.App\.exe$' } | Select-Object -First 1
        if (-not $app) { throw "KBot.App.exe not found inside $ZipPath." }
        $upd = $entries | Where-Object { $_.FullName -match '(^|/)KBot\.Updater\.exe$' } | Select-Object -First 1
        if (-not $upd) { throw "KBot.Updater.exe not found inside $ZipPath -- publish-release.ps1 must include it (step 4e)." }

        # Slice 0104: the package must be the kind it is pushed as. A non-access package under the
        # "access" block would strip the Migrator from nobody, but an access package under
        # "non_access" would hand the Access code to clients who must not have it.
        $migrator   = @($entries | Where-Object { $_.FullName -match '(^|/)Migrare/KBot\.Migrator\.exe$' })
        $accessDll  = @($entries | Where-Object { $_.FullName -match '(^|/)KBot\.Access\.dll$' })
        if ($IncludeAccess) {
            if ($migrator.Count -eq 0 -or $accessDll.Count -eq 0) {
                throw "$ZipPath is not an ACCESS package (Migrare\KBot.Migrator.exe and KBot.Access.dll expected). Build with -Audience Access."
            }
        } else {
            if ($migrator.Count -gt 0 -or $accessDll.Count -gt 0) {
                throw "$ZipPath carries Access components -- it cannot go out as the NON-ACCESS package. Build with -Audience NonAccess."
            }
        }

        $tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("kbot_app_" + [guid]::NewGuid().ToString('N') + '.exe')
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($app, $tmp, $true)
        try {
            $ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($tmp).FileVersion
        } finally {
            Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
        }
        if ([string]::IsNullOrWhiteSpace($ver)) { throw "KBot.App.exe inside the package has no FileVersion." }

        # The Release zip is created with includeBaseDirectory=true, so every entry
        # starts with 'KBot_Release_<stamp>/'. The updater strips one common top
        # folder; report it so the operator sees what the client will see.
        $top = ($entries | ForEach-Object { ($_.FullName -split '/')[0] } | Select-Object -Unique)
        $topFolder = ''
        if (@($top).Count -eq 1 -and $entries[0].FullName.Contains('/')) { $topFolder = $top }

        return [pscustomobject]@{
            Version   = [version]$ver
            TopFolder = $topFolder
            Entries   = $entries.Count
        }
    } finally {
        $zip.Dispose()
    }
}

# GET /api/update/latest?access=1|0. Returns $null when nothing is published for that kind (404).
function Get-ServerLatest {
    param([string]$BaseUrl, [bool]$Access)
    $url = $BaseUrl.TrimEnd('/') + '/api/update/latest?access=' + $(if ($Access) { '1' } else { '0' })
    try {
        return Invoke-RestMethod -Uri $url -Method Get -TimeoutSec 30
    } catch {
        $resp = $null
        if ($_.Exception.PSObject.Properties.Name -contains 'Response') { $resp = $_.Exception.Response }
        if ($resp -and [int]$resp.StatusCode -eq 404) { return $null }
        throw "GET $url failed: $($_.Exception.Message)"
    }
}

# ==============================================================================
#  MAIN
# ==============================================================================

$SolutionRoot = Find-SolutionRoot
$ArtifactsDir = Join-Path $SolutionRoot 'artifacts'

# --- 0. For whom (the FIRST question, before the build and its signing question) ----
$AudienceChoice = if ($Audience -eq 'Ask') { Read-AudienceChoice } else { $Audience }
$IncludeAccess  = ($AudienceChoice -eq 'Access')
$KindName       = if ($IncludeAccess) { 'access' } else { 'non-access' }
$ZipSuffix      = if ($IncludeAccess) { '' } else { '_noaccess' }
Write-Step "Audience: $($KindName.ToUpper()) package."
if ([string]::IsNullOrWhiteSpace($SettingsPath)) {
    $SettingsPath = Join-Path $SolutionRoot 'PYTHON\_push\push_settings.json'
}

# --- 1. Prerequisites first: fail before the (long) build, not after -----------
$settings = Read-PushSettings -Path $SettingsPath
$sftp     = Resolve-Sftp
$RemoteDir = "$($settings.RemoteRoot)/updates"
Write-Step "Target: $($settings.User)@$($settings.Host):$($settings.Port)  $RemoteDir"

# --- 2. Build (publish-release.ps1) or pick the newest existing zip -------------
$buildStart = Get-Date
if (-not $SkipBuild) {
    $publish = Join-Path $SolutionRoot 'publish-release.ps1'
    if (-not (Test-Path -LiteralPath $publish)) { throw "publish-release.ps1 not found at $publish." }
    Write-Step "Building RELEASE via publish-release.ps1 ..."
    $publishArgs = @{ Bump = $Bump; Sign = $Sign; Audience = $AudienceChoice }
    if (-not [string]::IsNullOrWhiteSpace($SignThumbprint)) { $publishArgs['SignThumbprint'] = $SignThumbprint }
    # Notes typed by hand (-Notes): the AI assistant is not asked for them (step 4b records them).
    if (-not [string]::IsNullOrWhiteSpace($Notes)) { $publishArgs['ReleaseNotes'] = 'Skip' }
    & $publish @publishArgs
    if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw "publish-release.ps1 failed (ExitCode=$LASTEXITCODE)." }
}

$zips = @(Get-ChildItem -LiteralPath $ArtifactsDir -Filter 'KBot_Release_*.zip' -File -ErrorAction SilentlyContinue |
          Where-Object { ($_.BaseName -like '*_noaccess') -eq (-not $IncludeAccess) } |
          Sort-Object LastWriteTime -Descending)
if ($zips.Count -eq 0) { throw "No $KindName KBot_Release_*$ZipSuffix.zip in $ArtifactsDir." }
$zip = $zips[0]
if (-not $SkipBuild -and $zip.LastWriteTime -lt $buildStart) {
    throw "The newest zip ($($zip.Name)) predates this build -- publish-release.ps1 produced nothing."
}
Write-Step "Package: $($zip.FullName) ($([Math]::Round($zip.Length / 1MB, 1)) MB)"

# --- 3. What is in it ----------------------------------------------------------
$info = Get-PackageInfo -ZipPath $zip.FullName -IncludeAccess $IncludeAccess
$localVersion = $info.Version
$sha = (Get-FileHash -LiteralPath $zip.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Step "KBot.App FileVersion in package: $localVersion  (entries: $($info.Entries), top folder: '$($info.TopFolder)')"
Write-Step "sha256: $sha"

# --- 4. What the server has ----------------------------------------------------
$server = Get-ServerLatest -BaseUrl $ApiBaseUrl -Access $IncludeAccess
# The OTHER kind stays exactly as the server has it: latest.json is rewritten whole, so its block is
# read back here and written again below.
$otherServer = Get-ServerLatest -BaseUrl $ApiBaseUrl -Access (-not $IncludeAccess)
$serverMinimum = [version]'0.0.0.0'
if ($server) {
    $serverVersion = [version]$server.version
    $serverMinimum = [version]$server.minimum
    Write-Step "Server has ($KindName): version $serverVersion, minimum $serverMinimum"
    if ($localVersion -le $serverVersion -and -not $Force) {
        throw "Server already has $serverVersion for the $KindName package; local package is $localVersion. Bump <FileVersion> in src\KBot.App\KBot.App.vbproj, or use -Force."
    }
} else {
    Write-Step "Server has no published $KindName update yet (404)."
}
if ($otherServer) {
    Write-Step "The other kind stays as it is: version $($otherServer.version), minimum $($otherServer.minimum)."
}

# --- 4b. What changed: the notes for the update box (slice 0067-02) -------------
#  Typed by hand (-Notes) = sent as they are and recorded. Otherwise they are read
#  from docs\release-notes\NOUTATI.md: every section above the server's version, up
#  to this one. publish-release.ps1 already asked for and waited on this version's
#  section; under -SkipBuild nobody did, so it is asked for here.
#  An error here stops the push: nothing has been uploaded yet.
$notesTool = Join-Path $SolutionRoot 'tools\ReleaseNotes\ReleaseNotes.ps1'
if (-not (Test-Path -LiteralPath $notesTool)) { throw "ReleaseNotes.ps1 not found at $notesTool." }
if (-not [string]::IsNullOrWhiteSpace($Notes)) {
    & $notesTool -Action Record -Version $localVersion.ToString() -NotesText $Notes
} else {
    if ($SkipBuild) {
        & $notesTool -Action Request -Version $localVersion.ToString()
        $null = & $notesTool -Action Wait -Version $localVersion.ToString()
    }
    $sinceVersion = ''
    if ($server) { $sinceVersion = ([version]$server.version).ToString() }
    $Notes = [string](& $notesTool -Action Text -Version $localVersion.ToString() -Since $sinceVersion)
    if ([string]::IsNullOrWhiteSpace($Notes)) {
        Write-Warning "[update] No release notes for $localVersion in docs\release-notes\NOUTATI.md -- the update box will say nothing about what changed."
    } else {
        Write-Step "Notes for the update box (from NOUTATI.md). To change them: stop at the password prompt (Ctrl+C), edit NOUTATI.md, run again with -SkipBuild."
        foreach ($noteLine in ($Notes -split "\r?\n")) { Write-Host "    $noteLine" }
    }
}

$minimum = $serverMinimum
if ($Mandatory) { $minimum = $localVersion }
if ($minimum -gt $localVersion) {
    # Can happen only with -Force on an older package: never publish a minimum
    # the published version itself does not satisfy.
    $minimum = $localVersion
}

# --- 5. latest.json ------------------------------------------------------------
$remoteFile = "KBot_$($localVersion)$ZipSuffix.zip"
$block = [ordered]@{
    version       = $localVersion.ToString()
    minimum       = $minimum.ToString()
    file          = $remoteFile
    size          = [long]$zip.Length
    sha256        = $sha
    published_utc = [DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')
    notes         = $Notes
}
# What the server answered for the other kind is the block as latest.json holds it (the API only
# trims nothing and adds nothing), so it goes back unchanged.
$otherBlock = $null
if ($otherServer) {
    $otherBlock = [ordered]@{
        version       = [string]$otherServer.version
        minimum       = [string]$otherServer.minimum
        file          = [string]$otherServer.file
        size          = [long]$otherServer.size
        sha256        = [string]$otherServer.sha256
        published_utc = [string]$otherServer.published_utc
        notes         = [string]$otherServer.notes
    }
}
$latest = [ordered]@{}
if ($IncludeAccess) {
    $latest['access'] = $block
    if ($otherBlock) { $latest['non_access'] = $otherBlock }
} else {
    if ($otherBlock) { $latest['access'] = $otherBlock }
    $latest['non_access'] = $block
}
$stage = Join-Path ([System.IO.Path]::GetTempPath()) ("kbot_push_" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage -Force | Out-Null
$latestPath = Join-Path $stage 'latest.json'
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($latestPath, ($latest | ConvertTo-Json -Depth 4), $utf8NoBom)
Write-Step "latest.json:`n$(Get-Content -LiteralPath $latestPath -Raw)"

# --- 6. Upload: one sftp session, package first, latest.json LAST ---------------
#  '-' prefix = ignore that command's failure (folder already there, no old file).
#  rename over an existing name is refused by some servers, so the old name is
#  removed first; both removals are harmless when nothing is there.
$batchPath = Join-Path $stage 'batch.sftp'
$batch = @(
    "-mkdir $RemoteDir",
    "cd $RemoteDir",
    "put ""$($zip.FullName)"" $remoteFile.part",
    "-rm $remoteFile",
    "rename $remoteFile.part $remoteFile",
    "put ""$latestPath"" latest.json.part",
    "-rm latest.json",
    "rename latest.json.part latest.json",
    "bye"
)
[System.IO.File]::WriteAllText($batchPath, ($batch -join "`n") + "`n", $utf8NoBom)

Write-Step "Uploading over sftp (type the password at the OpenSSH prompt) ..."
& $sftp -P $settings.Port -b $batchPath "$($settings.User)@$($settings.Host)"
if ($LASTEXITCODE -ne 0) { throw "sftp failed (ExitCode=$LASTEXITCODE). Nothing may be assumed about the server state -- check $RemoteDir." }

# --- 7. Verify through the API, the way clients will see it --------------------
$after = Get-ServerLatest -BaseUrl $ApiBaseUrl -Access $IncludeAccess
if (-not $after) { throw "Upload finished but GET /api/update/latest?access=$(if ($IncludeAccess) { 1 } else { 0 }) still returns 404. Is UPDATE_DIR on the server $RemoteDir ? (an old server without slice 0104 does not know the access parameter)" }
if ([version]$after.version -ne $localVersion) {
    throw "Upload finished but the server reports version $($after.version), expected $localVersion."
}
if ($after.sha256 -ne $sha) { throw "Server sha256 differs from the local package." }
if ($otherServer) {
    $otherAfter = Get-ServerLatest -BaseUrl $ApiBaseUrl -Access (-not $IncludeAccess)
    if (-not $otherAfter -or $otherAfter.sha256 -ne $otherServer.sha256) {
        throw "The OTHER kind of package changed or vanished during the upload. Check latest.json in $RemoteDir."
    }
}

Remove-Item -LiteralPath $stage -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Ok "PUBLISHED ($($KindName.ToUpper()))  version $localVersion   minimum $minimum   ($remoteFile, $([Math]::Round($zip.Length / 1MB, 1)) MB)"
if ($Mandatory) {
    Write-Ok "Mandatory: every client below $localVersion must update before it can log in."
} else {
    Write-Ok "Optional for clients at or above $minimum; mandatory below it."
}
Write-Host "  Clients read: $($ApiBaseUrl.TrimEnd('/'))/api/update/latest?access=$(if ($IncludeAccess) { 1 } else { 0 })"
