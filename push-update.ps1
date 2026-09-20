<#
================================================================================
  push-update.ps1  (slice 0067 -- application updates)

  Builds a RELEASE package (publish-release.ps1: publish, sign, zip, installer)
  and publishes it as THE update on the K-BOT server:

      <RemoteRoot>/updates/KBot_<version>.zip     the package (the Release zip)
      <RemoteRoot>/updates/latest.json            what the server tells clients

  The client (KBot.App, Release build only) asks GET /api/update/latest at
  startup and from the "Caută actualizări" buttons, compares its own
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

  Parameters
    -Mandatory        raise `minimum` to this version: every client below it is
                      forced to update (no "Mai târziu"). Without it, `minimum`
                      stays what the server already had (or 0.0.0.0).
    -Notes "<text>"   free text shown to the operator in the update dialog.
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
    param([string]$ZipPath)
    if (-not ('System.IO.Compression.ZipFile' -as [type])) {
        Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
    }
    $zip = [System.IO.Compression.ZipFile]::OpenRead($ZipPath)
    try {
        $entries = @($zip.Entries)
        $app = $entries | Where-Object { $_.FullName -match '(^|/)KBot\.App\.exe$' } | Select-Object -First 1
        if (-not $app) { throw "KBot.App.exe not found inside $ZipPath." }
        $upd = $entries | Where-Object { $_.FullName -match '(^|/)KBot\.Updater\.exe$' } | Select-Object -First 1
        if (-not $upd) { throw "KBot.Updater.exe not found inside $ZipPath -- publish-release.ps1 must include it (step 4e)." }

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

# GET /api/update/latest. Returns $null when nothing is published (404).
function Get-ServerLatest {
    param([string]$BaseUrl)
    $url = $BaseUrl.TrimEnd('/') + '/api/update/latest'
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
    if ([string]::IsNullOrWhiteSpace($SignThumbprint)) {
        & $publish -Bump $Bump -Sign $Sign
    } else {
        & $publish -SignThumbprint $SignThumbprint -Bump $Bump -Sign $Sign
    }
    if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw "publish-release.ps1 failed (ExitCode=$LASTEXITCODE)." }
}

$zips = @(Get-ChildItem -LiteralPath $ArtifactsDir -Filter 'KBot_Release_*.zip' -File -ErrorAction SilentlyContinue |
          Sort-Object LastWriteTime -Descending)
if ($zips.Count -eq 0) { throw "No KBot_Release_*.zip in $ArtifactsDir." }
$zip = $zips[0]
if (-not $SkipBuild -and $zip.LastWriteTime -lt $buildStart) {
    throw "The newest zip ($($zip.Name)) predates this build -- publish-release.ps1 produced nothing."
}
Write-Step "Package: $($zip.FullName) ($([Math]::Round($zip.Length / 1MB, 1)) MB)"

# --- 3. What is in it ----------------------------------------------------------
$info = Get-PackageInfo -ZipPath $zip.FullName
$localVersion = $info.Version
$sha = (Get-FileHash -LiteralPath $zip.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Step "KBot.App FileVersion in package: $localVersion  (entries: $($info.Entries), top folder: '$($info.TopFolder)')"
Write-Step "sha256: $sha"

# --- 4. What the server has ----------------------------------------------------
$server = Get-ServerLatest -BaseUrl $ApiBaseUrl
$serverMinimum = [version]'0.0.0.0'
if ($server) {
    $serverVersion = [version]$server.version
    $serverMinimum = [version]$server.minimum
    Write-Step "Server has: version $serverVersion, minimum $serverMinimum"
    if ($localVersion -le $serverVersion -and -not $Force) {
        throw "Server already has $serverVersion; local package is $localVersion. Bump <FileVersion> in src\KBot.App\KBot.App.vbproj, or use -Force."
    }
} else {
    Write-Step "Server has no published update yet (404)."
}

$minimum = $serverMinimum
if ($Mandatory) { $minimum = $localVersion }
if ($minimum -gt $localVersion) {
    # Can happen only with -Force on an older package: never publish a minimum
    # the published version itself does not satisfy.
    $minimum = $localVersion
}

# --- 5. latest.json ------------------------------------------------------------
$remoteFile = "KBot_$($localVersion).zip"
$latest = [ordered]@{
    version       = $localVersion.ToString()
    minimum       = $minimum.ToString()
    file          = $remoteFile
    size          = [long]$zip.Length
    sha256        = $sha
    published_utc = [DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')
    notes         = $Notes
}
$stage = Join-Path ([System.IO.Path]::GetTempPath()) ("kbot_push_" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage -Force | Out-Null
$latestPath = Join-Path $stage 'latest.json'
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($latestPath, ($latest | ConvertTo-Json -Depth 3), $utf8NoBom)
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
$after = Get-ServerLatest -BaseUrl $ApiBaseUrl
if (-not $after) { throw "Upload finished but GET /api/update/latest still returns 404. Is UPDATE_DIR on the server $RemoteDir ?" }
if ([version]$after.version -ne $localVersion) {
    throw "Upload finished but the server reports version $($after.version), expected $localVersion."
}
if ($after.sha256 -ne $sha) { throw "Server sha256 differs from the local package." }

Remove-Item -LiteralPath $stage -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Ok "PUBLISHED  version $localVersion   minimum $minimum   ($remoteFile, $([Math]::Round($zip.Length / 1MB, 1)) MB)"
if ($Mandatory) {
    Write-Ok "Mandatory: every client below $localVersion must update before it can log in."
} else {
    Write-Ok "Optional for clients at or above $minimum; mandatory below it."
}
Write-Host "  Clients read: $($ApiBaseUrl.TrimEnd('/'))/api/update/latest"
