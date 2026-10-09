<#
================================================================================
  push-last-zip.ps1

  Pushes the NEWEST zip in artifacts\ as the update, WITHOUT building anything.
  The update version is the current <FileVersion> of src\KBot.App\KBot.App.vbproj.

  Same server layout and the same latest.json as push-update.ps1 (both kinds of
  package, only this kind's block is rewritten, package first and latest.json
  last, one sftp session, one password prompt).

  Differences from push-update.ps1:
    - no build, no signing question, no version bump;
    - the kind of package (access / non-access) comes from the zip's name
      (*_noaccess.zip = non-access) and is checked against its content;
    - a version the server already has is not an error: you are asked whether
      to overwrite it (-Force answers Yes).

  Parameters
    -Mandatory      raise `minimum` of this kind to the pushed version.
    -Notes "<text>" your own text for the update box (else NOUTATI.md is used).
    -Force          overwrite without asking when the server has this version or newer.
    -SettingsPath   alternative push_settings.json.
    -ApiBaseUrl     where /api/update/latest is read from (default: production).
================================================================================
#>

[CmdletBinding()]
param(
    [switch] $Mandatory,
    [string] $Notes = '',
    [switch] $Force,
    [string] $SettingsPath = '',
    [string] $ApiBaseUrl = 'https://kbot.avatarsoft.ro'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Write-Step { param([string]$m) Write-Host "[update] $m" -ForegroundColor Cyan }
function Write-Ok   { param([string]$m) Write-Host "[update] $m" -ForegroundColor Green }

function Read-YesNo {
    param([string]$Question)
    if (-not [Environment]::UserInteractive -or [Console]::IsInputRedirected) { return $false }
    $answer = Read-Host "$Question (y/N)"
    return ($answer.Trim().ToUpperInvariant() -in @('Y', 'YES', 'DA'))
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
        throw "push_settings.json not found: $Path  (copy AvacontPush\push_settings.example.json and fill Host/Port/User/RemoteRoot)."
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

# The version the project says it is now (src\KBot.App\KBot.App.vbproj <FileVersion>).
function Get-ProjectVersion {
    param([string]$Root)
    $proj = Join-Path $Root 'src\KBot.App\KBot.App.vbproj'
    if (-not (Test-Path -LiteralPath $proj)) { throw "Project file not found: $proj" }
    $m = [regex]::Match((Get-Content -LiteralPath $proj -Raw -Encoding UTF8), '<FileVersion>\s*([0-9.]+)\s*</FileVersion>')
    if (-not $m.Success) { throw "<FileVersion> not found in $proj." }
    return [version]$m.Groups[1].Value
}

# Checks the zip is a usable package of the given kind and reads the KBot.App
# FileVersion from inside it (reported, compared with the project's).
function Get-PackageInfo {
    param([string]$ZipPath, [bool]$IncludeAccess)
    if (-not ('System.IO.Compression.ZipFile' -as [type])) {
        Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
    }
    $zipFile = [System.IO.Compression.ZipFile]::OpenRead($ZipPath)
    try {
        $entries = @($zipFile.Entries)
        $backslashed = @($entries | Where-Object { $_.FullName.Contains('\') })
        if ($backslashed.Count -gt 0) {
            throw ("Package entries use backslashes ({0}); rebuild it with the current publish-release.ps1 -- " +
                   "KBot.Updater would unpack it into a subfolder.") -f $backslashed[0].FullName
        }
        $app = $entries | Where-Object { $_.FullName -match '(^|/)KBot\.App\.exe$' } | Select-Object -First 1
        if (-not $app) { throw "KBot.App.exe not found inside $ZipPath." }
        $upd = $entries | Where-Object { $_.FullName -match '(^|/)KBot\.Updater\.exe$' } | Select-Object -First 1
        if (-not $upd) { throw "KBot.Updater.exe not found inside $ZipPath." }

        $migrator  = @($entries | Where-Object { $_.FullName -match '(^|/)Migrare/KBot\.Migrator\.exe$' })
        $accessDll = @($entries | Where-Object { $_.FullName -match '(^|/)KBot\.Access\.dll$' })
        if ($IncludeAccess) {
            if ($migrator.Count -eq 0 -or $accessDll.Count -eq 0) {
                throw "$ZipPath is not an ACCESS package (Migrare\KBot.Migrator.exe and KBot.Access.dll expected)."
            }
        } else {
            if ($migrator.Count -gt 0 -or $accessDll.Count -gt 0) {
                throw "$ZipPath carries Access components -- it cannot go out as the NON-ACCESS package."
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
        return [pscustomobject]@{ Version = [version]$ver; Entries = $entries.Count }
    } finally {
        $zipFile.Dispose()
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
if ([string]::IsNullOrWhiteSpace($SettingsPath)) {
    $SettingsPath = Join-Path $SolutionRoot '_push\push_settings.json'
}

# --- 1. Prerequisites ----------------------------------------------------------
$settings  = Read-PushSettings -Path $SettingsPath
$sftp      = Resolve-Sftp
$RemoteDir = "$($settings.RemoteRoot)/updates"
Write-Step "Target: $($settings.User)@$($settings.Host):$($settings.Port)  $RemoteDir"

# --- 2. The newest zip, whatever its kind --------------------------------------
$zips = @(Get-ChildItem -LiteralPath $ArtifactsDir -Filter 'KBot_Release_*.zip' -File -ErrorAction SilentlyContinue |
          Sort-Object LastWriteTime -Descending)
if ($zips.Count -eq 0) { throw "No KBot_Release_*.zip in $ArtifactsDir." }
$zip            = $zips[0]
$IncludeAccess  = -not ($zip.BaseName -like '*_noaccess')
$KindName       = if ($IncludeAccess) { 'access' } else { 'non-access' }
$ZipSuffix      = if ($IncludeAccess) { '' } else { '_noaccess' }
Write-Step "Package: $($zip.FullName) ($([Math]::Round($zip.Length / 1MB, 1)) MB, $($zip.LastWriteTime), $($KindName.ToUpper()))"

# --- 3. Version: the project's, checked against the package --------------------
$info         = Get-PackageInfo -ZipPath $zip.FullName -IncludeAccess $IncludeAccess
$localVersion = Get-ProjectVersion -Root $SolutionRoot
$sha          = (Get-FileHash -LiteralPath $zip.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Step "Update version (project FileVersion): $localVersion"
Write-Step "KBot.App FileVersion inside the package: $($info.Version)  (entries: $($info.Entries))"
Write-Step "sha256: $sha"
if ($info.Version -ne $localVersion) {
    Write-Warning "[update] The package holds $($info.Version) but the project says $localVersion. Clients compare their own version with the one published, so this package would be announced as $localVersion."
    if (-not $Force -and -not (Read-YesNo "Push it as $localVersion anyway?")) {
        throw "Aborted: package version $($info.Version) differs from the project version $localVersion."
    }
}

# --- 4. What the server has: a prompt, not an error ----------------------------
$server      = Get-ServerLatest -BaseUrl $ApiBaseUrl -Access $IncludeAccess
$otherServer = Get-ServerLatest -BaseUrl $ApiBaseUrl -Access (-not $IncludeAccess)
$serverMinimum = [version]'0.0.0.0'
if ($server) {
    $serverVersion = [version]$server.version
    $serverMinimum = [version]$server.minimum
    Write-Step "Server has ($KindName): version $serverVersion, minimum $serverMinimum"
    if ($localVersion -le $serverVersion) {
        $what = if ($localVersion -eq $serverVersion) { "already has $serverVersion" } else { "has $serverVersion, NEWER than $localVersion" }
        Write-Warning "[update] The server $what for the $KindName package."
        if (-not $Force -and -not (Read-YesNo "Overwrite it with $($localVersion)?")) {
            Write-Host "[update] Nothing pushed." -ForegroundColor Yellow
            return
        }
    }
} else {
    Write-Step "Server has no published $KindName update yet (404)."
}
if ($otherServer) {
    Write-Step "The other kind stays as it is: version $($otherServer.version), minimum $($otherServer.minimum)."
}

# --- 4b. Notes for the update box ----------------------------------------------
$notesTool = Join-Path $SolutionRoot 'tools\ReleaseNotes\ReleaseNotes.ps1'
if (-not (Test-Path -LiteralPath $notesTool)) { throw "ReleaseNotes.ps1 not found at $notesTool." }
if (-not [string]::IsNullOrWhiteSpace($Notes)) {
    & $notesTool -Action Record -Version $localVersion.ToString() -NotesText $Notes
} else {
    $sinceVersion = ''
    if ($server -and ([version]$server.version) -lt $localVersion) { $sinceVersion = ([version]$server.version).ToString() }
    $Notes = [string](& $notesTool -Action Text -Version $localVersion.ToString() -Since $sinceVersion)
    if ([string]::IsNullOrWhiteSpace($Notes)) {
        Write-Warning "[update] No release notes for $localVersion in docs\release-notes\NOUTATI.md -- the update box will say nothing about what changed."
    } else {
        Write-Step "Notes for the update box (from NOUTATI.md):"
        foreach ($noteLine in ($Notes -split "\r?\n")) { Write-Host "    $noteLine" }
    }
}

$minimum = $serverMinimum
if ($Mandatory) { $minimum = $localVersion }
if ($minimum -gt $localVersion) { $minimum = $localVersion }

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
if (-not $after) { throw "Upload finished but GET /api/update/latest?access=$(if ($IncludeAccess) { 1 } else { 0 }) still returns 404. Is UPDATE_DIR on the server $RemoteDir ?" }
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
Write-Host "  Clients read: $($ApiBaseUrl.TrimEnd('/'))/api/update/latest?access=$(if ($IncludeAccess) { 1 } else { 0 })"
