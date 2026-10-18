<#
================================================================================
  rollback-update.ps1  (slice 0067 -- application updates)

  Takes the LAST update back: makes an OLDER version THE update again on the K-BOT
  server, with the same layout, transport and safety order as push-update.ps1:

      <RemoteRoot>/updates/KBot_<version>.zip             the package (the Release zip)
      <RemoteRoot>/updates/KBot_<version>_noaccess.zip    ... for clients WITHOUT Access
      <RemoteRoot>/updates/latest.json                    what the server tells clients

  What it does
    - asks FIRST for whom the rollback is (Access / Non-access), like push-update.ps1;
      only that kind's block of latest.json is rewritten, the other kind and the
      Playwright driver block stay exactly as the server has them (read back through
      the API);
    - looks for the older package in artifacts\KBot_Release_*.zip of that kind (the
      version is read from INSIDE the zip) and proposes the newest one that is OLDER
      than what the server publishes now (-ToVersion picks another);
    - uploads that zip again (a *.part, renamed into place) so the file on the server
      is certainly the one whose sha256 goes into latest.json; -SkipUpload leaves the
      server's own copy of it (it must already be there);
    - writes latest.json LAST, so a client never reads a version whose file is not there;
    - reads it back through the API, checks the other kind and the driver did not move
      and that /api/update/download really serves the file.
    - keeps a copy of what the server told before in artifacts\latest_before_rollback_<stamp>.json.

  What it CANNOT do
    A client that already updated to the bad version stays on it: the updater never
    goes down. Those clients get the next real version. Everyone else (not yet updated)
    simply sees the old version again and is not asked to update.

  `minimum` of the kind is lowered to the rolled-back version when it is above it (a
  minimum higher than the published version would lock every client out).

  The version in src\KBot.App\KBot.App.vbproj is NOT touched: the next push needs a
  <FileVersion> higher than the bad one anyway.

  Parameters
    -Audience         Access / NonAccess / Ask (default: asks FIRST).
    -ToVersion        the version to go back to (e.g. 1.1.2.3). Default: the newest local
                      package older than the server's current version.
    -Notes "<text>"   text for the update box; default: that version's own section of
                      docs\release-notes\NOUTATI.md (empty if it has none).
    -SkipUpload       do not upload the zip again; trust the copy on the server.
    -Force            do not ask for the final confirmation.
    -SettingsPath     alternative push_settings.json.
    -ApiBaseUrl       where /api/update/latest is read from (default: production).
================================================================================
#>

[CmdletBinding()]
param(
    [ValidateSet('Ask', 'Access', 'NonAccess')]
    [string] $Audience = 'Ask',
    [string] $ToVersion = '',
    [string] $Notes = '',
    [switch] $SkipUpload,
    [switch] $Force,
    [string] $SettingsPath = '',
    [string] $ApiBaseUrl = 'https://kbot.avatarsoft.ro'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# ==============================================================================
#  HELPERS
# ==============================================================================

function Write-Step { param([string]$m) Write-Host "[rollback] $m" -ForegroundColor Cyan }
function Write-Ok   { param([string]$m) Write-Host "[rollback] $m" -ForegroundColor Green }

# The first question, same wording as push-update.ps1. Enter = Access.
function Read-AudienceChoice {
    if (-not [Environment]::UserInteractive -or [Console]::IsInputRedirected) {
        Write-Host "Audience: no interactive console -> Access (pass -Audience NonAccess for the other package)." -ForegroundColor Yellow
        return 'Access'
    }
    while ($true) {
        $answer = Read-Host "For whom is this rollback? (A)ccess = with Migrator + Access code / (N)on-access = without [A]"
        $key = ($answer -replace '[\s-]', '').ToUpperInvariant()
        if ($key -in @('', 'A', 'ACCESS')) { return 'Access' }
        if ($key -in @('N', 'NONACCESS')) { return 'NonAccess' }
        Write-Host "  Type A (access) or N (non-access); Enter = A." -ForegroundColor Yellow
    }
}

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

# Reads the KBot.App FileVersion from INSIDE the zip -- the package is the truth, not its name.
# Same checks as push-update.ps1: a package that would not be accepted as an update is not
# accepted as a rollback target either.
function Get-PackageInfo {
    param([string]$ZipPath, [bool]$IncludeAccess)
    if (-not ('System.IO.Compression.ZipFile' -as [type])) {
        Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
    }
    $zip = [System.IO.Compression.ZipFile]::OpenRead($ZipPath)
    try {
        $entries = @($zip.Entries)
        $adminInside = @($entries | Where-Object { $_.FullName -match '(^|/)(KBotAdmin[^/]*|admin_settings[^/]*|Renci\.SshNet[^/]*)$' })
        if ($adminInside.Count -gt 0) {
            throw ("Package contains KBotAdmin files ({0}); KBotAdmin must never be delivered to clients." -f $adminInside[0].FullName)
        }
        $backslashed = @($entries | Where-Object { $_.FullName.Contains('\') })
        if ($backslashed.Count -gt 0) {
            throw ("Package entries use backslashes ({0}); KBot.Updater would unpack it into a subfolder." -f $backslashed[0].FullName)
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

        return [pscustomobject]@{
            Version = [version]$ver
            Entries = $entries.Count
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
        # The server's JSON has no charset in its Content-Type: decode the bytes as UTF-8 ourselves
        # (Windows PowerShell 5.1 would read Latin-1 and the notes would come back as mojibake).
        $resp = Invoke-WebRequest -Uri $url -Method Get -TimeoutSec 30 -UseBasicParsing
        $bytes = $resp.RawContentStream.ToArray()
        return ([System.Text.Encoding]::UTF8.GetString($bytes) | ConvertFrom-Json)
    } catch {
        $resp = $null
        if ($_.Exception.PSObject.Properties.Name -contains 'Response') { $resp = $_.Exception.Response }
        if ($resp -and [int]$resp.StatusCode -eq 404) { return $null }
        throw "GET $url failed: $($_.Exception.Message)"
    }
}

# A block as latest.json holds it, from what the API answered.
function ConvertTo-Block {
    param($Api)
    return [ordered]@{
        version       = [string]$Api.version
        minimum       = [string]$Api.minimum
        file          = [string]$Api.file
        size          = [long]$Api.size
        sha256        = [string]$Api.sha256
        published_utc = [string]$Api.published_utc
        notes         = [string]$Api.notes
    }
}

# ==============================================================================
#  MAIN
# ==============================================================================

$SolutionRoot = Find-SolutionRoot
$ArtifactsDir = Join-Path $SolutionRoot 'artifacts'

# --- 0. For whom (the FIRST question) -------------------------------------------
$AudienceChoice = if ($Audience -eq 'Ask') { Read-AudienceChoice } else { $Audience }
$IncludeAccess  = ($AudienceChoice -eq 'Access')
$KindName       = if ($IncludeAccess) { 'access' } else { 'non-access' }
$ZipSuffix      = if ($IncludeAccess) { '' } else { '_noaccess' }
Write-Step "Audience: $($KindName.ToUpper()) package."
if ([string]::IsNullOrWhiteSpace($SettingsPath)) {
    $SettingsPath = Join-Path $SolutionRoot '_push\push_settings.json'
}

# --- 1. Prerequisites first ------------------------------------------------------
$settings = Read-PushSettings -Path $SettingsPath
$sftp     = Resolve-Sftp
$RemoteDir = "$($settings.RemoteRoot)/updates"
Write-Step "Target: $($settings.User)@$($settings.Host):$($settings.Port)  $RemoteDir"

# --- 2. What the server publishes now ---------------------------------------------
$server = Get-ServerLatest -BaseUrl $ApiBaseUrl -Access $IncludeAccess
if (-not $server) { throw "The server has no published $KindName update -- there is nothing to roll back." }
$otherServer = Get-ServerLatest -BaseUrl $ApiBaseUrl -Access (-not $IncludeAccess)
$serverVersion = [version]$server.version
$serverMinimum = [version]$server.minimum
Write-Step "Server publishes ($KindName): version $serverVersion, minimum $serverMinimum  ($($server.file))"
if ($otherServer) {
    Write-Step "The other kind stays as it is: version $($otherServer.version), minimum $($otherServer.minimum)."
}

# The driver block is shared: either answer carries it, and it goes back unchanged.
$driverBlock = $null
$driverSource = $null
if ($server.PSObject.Properties.Name -contains 'driver' -and $server.driver) { $driverSource = $server.driver }
elseif ($otherServer -and $otherServer.PSObject.Properties.Name -contains 'driver' -and $otherServer.driver) { $driverSource = $otherServer.driver }
if ($driverSource) {
    $driverBlock = [ordered]@{ version = [string]$driverSource.version; file = [string]$driverSource.file; size = [long]$driverSource.size; sha256 = [string]$driverSource.sha256 }
}

# --- 3. The older packages we still have here -------------------------------------
$zips = @(Get-ChildItem -LiteralPath $ArtifactsDir -Filter 'KBot_Release_*.zip' -File -ErrorAction SilentlyContinue |
          Where-Object { ($_.BaseName -like '*_noaccess') -eq (-not $IncludeAccess) } |
          Sort-Object LastWriteTime -Descending | Select-Object -First 15)
if ($zips.Count -eq 0) { throw "No $KindName KBot_Release_*$ZipSuffix.zip in $ArtifactsDir -- nothing to roll back to." }

Write-Step "Reading the version of $($zips.Count) local package(s) ..."
$found = New-Object System.Collections.Generic.List[object]
foreach ($z in $zips) {
    try {
        $pi = Get-PackageInfo -ZipPath $z.FullName -IncludeAccess $IncludeAccess
        $found.Add([pscustomobject]@{ Version = $pi.Version; Zip = $z; Entries = $pi.Entries })
    } catch {
        Write-Host "[rollback]   skipped $($z.Name): $($_.Exception.Message)" -ForegroundColor DarkYellow
    }
}
# One entry per version: the newest zip of it.
$candidates = @($found | Sort-Object @{ Expression = { $_.Version }; Descending = $true }, @{ Expression = { $_.Zip.LastWriteTime }; Descending = $true } |
                Group-Object { $_.Version.ToString() } | ForEach-Object { $_.Group[0] })
if ($candidates.Count -eq 0) { throw "None of the local packages could be used." }

$target = $null
if (-not [string]::IsNullOrWhiteSpace($ToVersion)) {
    $want = [version]$ToVersion
    $target = $candidates | Where-Object { $_.Version -eq $want } | Select-Object -First 1
    if (-not $target) {
        throw "No local $KindName package with version $want. Local versions: $((($candidates | ForEach-Object { $_.Version.ToString() }) -join ', '))."
    }
    if ($want -ge $serverVersion) {
        Write-Warning "[rollback] $want is not older than the server's $serverVersion -- this is not a rollback."
    }
} else {
    $older = @($candidates | Where-Object { $_.Version -lt $serverVersion })
    if ($older.Count -eq 0) {
        throw "No local $KindName package is older than the server's $serverVersion. Local versions: $((($candidates | ForEach-Object { $_.Version.ToString() }) -join ', ')). Use -ToVersion for another one."
    }
    $older = @($older | Select-Object -First 8)
    Write-Step "Older local packages ($KindName):"
    for ($i = 0; $i -lt $older.Count; $i++) {
        Write-Host ("    [{0}] {1}   {2}   ({3:yyyy-MM-dd HH:mm})" -f ($i + 1), $older[$i].Version, $older[$i].Zip.Name, $older[$i].Zip.LastWriteTime)
    }
    $pick = 1
    if ([Environment]::UserInteractive -and -not [Console]::IsInputRedirected -and $older.Count -gt 1) {
        $answer = Read-Host "Roll back to which one? [1]"
        if (-not [string]::IsNullOrWhiteSpace($answer)) {
            $n = 0
            if (-not [int]::TryParse($answer.Trim(), [ref]$n) -or $n -lt 1 -or $n -gt $older.Count) { throw "Invalid choice: $answer" }
            $pick = $n
        }
    }
    $target = $older[$pick - 1]
}

$zip = $target.Zip
$targetVersion = $target.Version
$sha = (Get-FileHash -LiteralPath $zip.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
$remoteFile = "KBot_$($targetVersion)$ZipSuffix.zip"
Write-Step "Rolling back to: $targetVersion  ($($zip.Name), $([Math]::Round($zip.Length / 1MB, 1)) MB)"
Write-Step "sha256: $sha"

# --- 4. minimum and notes ---------------------------------------------------------
$minimum = $serverMinimum
if ($minimum -gt $targetVersion) {
    Write-Step "minimum $minimum is above $targetVersion -> lowered to $targetVersion."
    $minimum = $targetVersion
}

if ([string]::IsNullOrWhiteSpace($Notes)) {
    $notesTool = Join-Path $SolutionRoot 'tools\ReleaseNotes\ReleaseNotes.ps1'
    if (Test-Path -LiteralPath $notesTool) {
        try { $Notes = [string](& $notesTool -Action Text -Version $targetVersion.ToString()) }
        catch { Write-Warning "[rollback] Could not read the notes of ${targetVersion}: $($_.Exception.Message)"; $Notes = '' }
    }
}
if ([string]::IsNullOrWhiteSpace($Notes)) {
    $Notes = ''
    Write-Step "No release notes for $targetVersion -- the update box will say nothing about what changed."
}

# --- 5. Confirm --------------------------------------------------------------------
Write-Host ""
Write-Host "  $($KindName.ToUpper()) clients will be offered: $targetVersion   (now: $serverVersion)" -ForegroundColor Yellow
Write-Host "  Clients that already updated to $serverVersion stay on it." -ForegroundColor Yellow
if (-not $Force) {
    if (-not (Read-YesNo "Make $targetVersion the published $KindName update?")) {
        Write-Host "[rollback] Nothing changed." -ForegroundColor Yellow
        return
    }
}

# --- 6. latest.json ----------------------------------------------------------------
# A copy of what the server says now, so this rollback can itself be undone.
$stage = Join-Path ([System.IO.Path]::GetTempPath()) ("kbot_rollback_" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage -Force | Out-Null
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)

$before = [ordered]@{}
if ($IncludeAccess) {
    $before['access'] = ConvertTo-Block $server
    if ($otherServer) { $before['non_access'] = ConvertTo-Block $otherServer }
} else {
    if ($otherServer) { $before['access'] = ConvertTo-Block $otherServer }
    $before['non_access'] = ConvertTo-Block $server
}
if ($driverBlock) { $before['driver'] = $driverBlock }
$beforePath = Join-Path $ArtifactsDir ("latest_before_rollback_{0:yyyyMMdd_HHmmss}.json" -f (Get-Date))
[System.IO.File]::WriteAllText($beforePath, ($before | ConvertTo-Json -Depth 4), $utf8NoBom)
Write-Step "Copy of the current latest.json: $beforePath"

$block = [ordered]@{
    version       = $targetVersion.ToString()
    minimum       = $minimum.ToString()
    file          = $remoteFile
    size          = [long]$zip.Length
    sha256        = $sha
    published_utc = [DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')
    notes         = $Notes
}
$otherBlock = $null
if ($otherServer) { $otherBlock = ConvertTo-Block $otherServer }

$latest = [ordered]@{}
if ($IncludeAccess) {
    $latest['access'] = $block
    if ($otherBlock) { $latest['non_access'] = $otherBlock }
} else {
    if ($otherBlock) { $latest['access'] = $otherBlock }
    $latest['non_access'] = $block
}
if ($driverBlock) { $latest['driver'] = $driverBlock }
$latestPath = Join-Path $stage 'latest.json'
[System.IO.File]::WriteAllText($latestPath, ($latest | ConvertTo-Json -Depth 4), $utf8NoBom)
Write-Step "latest.json:`n$(Get-Content -LiteralPath $latestPath -Raw)"

# --- 7. Upload: one sftp session, package first, latest.json LAST --------------------
#  '-' prefix = ignore that command's failure (folder already there, no old file).
$batchPath = Join-Path $stage 'batch.sftp'
$batch = @(
    "-mkdir $RemoteDir",
    "cd $RemoteDir"
)
if (-not $SkipUpload) {
    $batch += @(
        "put ""$($zip.FullName)"" $remoteFile.part",
        "-rm $remoteFile",
        "rename $remoteFile.part $remoteFile"
    )
}
$batch += @(
    "put ""$latestPath"" latest.json.part",
    "-rm latest.json",
    "rename latest.json.part latest.json",
    "bye"
)
[System.IO.File]::WriteAllText($batchPath, ($batch -join "`n") + "`n", $utf8NoBom)

Write-Step "Uploading over sftp (type the password at the OpenSSH prompt) ..."
& $sftp -P $settings.Port -b $batchPath "$($settings.User)@$($settings.Host)"
if ($LASTEXITCODE -ne 0) { throw "sftp failed (ExitCode=$LASTEXITCODE). Nothing may be assumed about the server state -- check $RemoteDir. The previous latest.json is in $beforePath." }

# --- 8. Verify through the API, the way clients will see it ------------------------
$after = Get-ServerLatest -BaseUrl $ApiBaseUrl -Access $IncludeAccess
if (-not $after) { throw "Upload finished but GET /api/update/latest?access=$(if ($IncludeAccess) { 1 } else { 0 }) returns 404." }
if ([version]$after.version -ne $targetVersion) {
    throw "Upload finished but the server reports version $($after.version), expected $targetVersion."
}
if ($after.sha256 -ne $sha) { throw "Server sha256 differs from the local package." }
if ($driverBlock) {
    if (-not ($after.PSObject.Properties.Name -contains 'driver') -or -not $after.driver -or $after.driver.sha256 -ne $driverBlock.sha256) {
        throw "Upload finished but GET /api/update/latest does not report the driver block as before. Compare with $beforePath."
    }
}
if ($otherServer) {
    $otherAfter = Get-ServerLatest -BaseUrl $ApiBaseUrl -Access (-not $IncludeAccess)
    if (-not $otherAfter -or $otherAfter.sha256 -ne $otherServer.sha256) {
        throw "The OTHER kind of package changed or vanished during the upload. Compare latest.json in $RemoteDir with $beforePath."
    }
}

# The file itself: ask for its first byte (the download route answers 500 PACKAGE_MISSING when it is not there).
$dlUrl = $ApiBaseUrl.TrimEnd('/') + '/api/update/download?access=' + $(if ($IncludeAccess) { '1' } else { '0' })
#  Windows PowerShell 5.1 refuses a 'Range' entry in -Headers, so the request is built by hand (AddRange).
try {
    $req = [System.Net.HttpWebRequest]::Create($dlUrl)
    $req.Method = 'GET'
    $req.Timeout = 60000
    $req.AddRange(0, 0)
    $probe = $req.GetResponse()
    try {
        $probeStatus = [int]$probe.StatusCode
    } finally {
        $probe.Close()
    }
    if ($probeStatus -ne 200 -and $probeStatus -ne 206) { throw "HTTP $probeStatus" }
} catch {
    throw "latest.json now names $remoteFile but GET $dlUrl fails: $($_.Exception.Message). If the package is missing on the server, run again WITHOUT -SkipUpload. The previous latest.json is in $beforePath."
}

Remove-Item -LiteralPath $stage -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Ok "ROLLED BACK ($($KindName.ToUpper()))  published version $targetVersion   minimum $minimum   ($remoteFile, $([Math]::Round($zip.Length / 1MB, 1)) MB)"
Write-Ok "Clients on $targetVersion or older are no longer offered $serverVersion. Clients already on $serverVersion keep it."
Write-Host "  Clients read: $($ApiBaseUrl.TrimEnd('/'))/api/update/latest?access=$(if ($IncludeAccess) { 1 } else { 0 })"
Write-Host "  Remember: <FileVersion> in src\KBot.App\KBot.App.vbproj is still $serverVersion or higher -- the next push must be a NEW higher version." -ForegroundColor Yellow
