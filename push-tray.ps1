<#
================================================================================
  push-tray.ps1  (slice TRAY-02 -- the signer edition, "tray only" push)

  Builds ONLY the tray program (src\KBot.Tray) -- the small program of the people who
  only sign: the documents waiting for their signature, viewing and signing them -- signs
  ONLY KBot.Tray.exe and puts it on the K-BOT server as its own package:

      <RemoteRoot>/updates/KBot_Tray_<version>.zip     the package
      <RemoteRoot>/updates/tray.json                   what the server tells the tray

  It has nothing to do with push-update.ps1: other version number (the FileVersion of
  src\KBot.Tray\KBot.Tray.vbproj), other file (tray.json, never latest.json), other
  clients (the signers; the full K-BOT of the accountants does not carry the tray any
  more). Running it never touches the K-BOT packages on the server, and the other way round.

  What is in the package: the tray's folder from `dotnet publish` (it uses the DLLs of
  KBot.App, so those are in it) -- but NO other .exe. KBot.App.exe, the updater, the
  migrators and the Playwright driver (node.exe) are left out on purpose: a signer has
  nothing to double-click except KBot.Tray.exe. The DLLs stay unsigned, like in the
  K-BOT package (Windows checks the signature of what is started, not of DLLs).

  Run it when YOU decide a new tray is ready. The version is the FileVersion of the tray
  project; this script can bump it (one console question, like publish-release.ps1).
  If the server already has this version or a newer one, it asks before overwriting
  (-Force answers Yes).

  Transport: Windows OpenSSH sftp.exe in batch mode, one session, ONE password prompt
  (typed at the OpenSSH prompt, never stored). Host / Port / User / RemoteRoot come from
  _push\push_settings.json, the same file push-update.ps1 uses. The package goes up as
  *.part and is renamed into place, and tray.json is written LAST, so a client can never
  read a version whose file is not fully there.

  Parameters
    -Bump             Ask (default, one console question) / None / Major / Minor / Build /
                      Revision: which part of the tray's FileVersion to raise before building.
    -Sign             Ask (default, one console question) / Yes / No. No = nothing signed,
                      no SimplySign confirmation. Yes/Ask = ONE confirmation (KBot.Tray.exe).
    -SignThumbprint   Certum SimplySign certificate (default: the same as publish-release.ps1).
    -Installer        Yes (default) / No: also build the installer for NEW signers,
                      artifacts\KBot_Tray_Setup_<version>.exe (Inno Setup, tools\KBotTrayInstaller\KBotTray.iss): per-user,
                      no administrator, installs to %LOCALAPPDATA%\AVACONT\KBotSemnare. It is NOT uploaded to the server:
                      you hand it to the signers. With signing on it costs 2 more SimplySign confirmations (Setup + uninstaller).
                      Installed tray programs update themselves from the package; the installer is only for the first install.
    -Notes "<text>"   free text stored in tray.json (not shown anywhere yet).
    -Force            push without asking even if the server already has this version or newer.
    -SettingsPath     alternative push_settings.json.
    -ApiBaseUrl       where /api/update/tray/latest is read from (default: production).
================================================================================
#>

[CmdletBinding()]
param(
    [ValidateSet('Ask', 'None', 'Major', 'Minor', 'Build', 'Revision')]
    [string] $Bump = 'Ask',
    [ValidateSet('Ask', 'Yes', 'No')]
    [string] $Sign = 'Ask',
    [ValidateSet('Yes', 'No')]
    [string] $Installer = 'Yes',
    [string] $SignThumbprint = '2F0E82DB6781F3DC17D43166A3420B7CBE380A68',
    [string] $TimestampUrl = 'http://time.certum.pl',
    [string] $Notes = '',
    [switch] $Force,
    [string] $SettingsPath = '',
    [string] $ApiBaseUrl = 'https://kbot.avatarsoft.ro'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$Configuration = 'Release'
$Rid = 'win-x64'

function Write-Step { param([string]$m) Write-Host "[tray] $m" -ForegroundColor Cyan }
function Write-Ok   { param([string]$m) Write-Host "[tray] $m" -ForegroundColor Green }

function Find-SolutionRoot {
    $dir = $PSScriptRoot
    while ($dir -and -not (Test-Path (Join-Path $dir 'KBot.sln'))) { $dir = Split-Path $dir -Parent }
    if (-not $dir) { throw "KBot.sln not found above $PSScriptRoot." }
    return $dir
}

function Read-InteractiveChoice {
    param([string]$Question, [string]$NoConsoleMessage, $NoConsoleValue, [hashtable]$Map, [string]$Hint)
    if (-not [Environment]::UserInteractive -or [Console]::IsInputRedirected) {
        Write-Host $NoConsoleMessage -ForegroundColor Yellow
        return $NoConsoleValue
    }
    while ($true) {
        $answer = Read-Host $Question
        $key = ($answer -replace '\s', '').ToUpperInvariant()
        if ($Map.ContainsKey($key)) { return $Map[$key] }
        Write-Host "  $Hint" -ForegroundColor Yellow
    }
}

function Read-YesNo {
    param([string]$Question)
    if (-not [Environment]::UserInteractive -or [Console]::IsInputRedirected) { return $false }
    $answer = Read-Host "$Question (y/N)"
    return ($answer.Trim().ToUpperInvariant() -in @('Y', 'YES', 'DA'))
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

# ---- version (same rules as publish-release.ps1: 1.9.99.9 + Revision -> 2.0.0.0) ----------
$VersionParts = @('Major', 'Minor', 'Build', 'Revision')
$VersionCaps  = @{ Minor = 9; Build = 99; Revision = 9 }

function Step-KBotVersion {
    param([version]$Version, [string]$Part)
    $n = @([Math]::Max($Version.Major, 0), [Math]::Max($Version.Minor, 0),
           [Math]::Max($Version.Build, 0), [Math]::Max($Version.Revision, 0))
    $i = [Array]::IndexOf($VersionParts, $Part)
    if ($i -lt 0) { throw "Unknown version part '$Part'." }
    for ($j = $i + 1; $j -lt 4; $j++) { $n[$j] = 0 }
    while ($true) {
        $n[$i]++
        if ($i -eq 0 -or $n[$i] -le $VersionCaps[$VersionParts[$i]]) { break }
        $n[$i] = 0
        $i--
    }
    return [version]::new($n[0], $n[1], $n[2], $n[3])
}

function Get-KBotFileVersion {
    param([string]$Project)
    $text = [System.IO.File]::ReadAllText($Project)
    $m = [regex]::Match($text, '<FileVersion>\s*([0-9.]+)\s*</FileVersion>')
    if (-not $m.Success) { throw "No <FileVersion> element in $Project." }
    return [version]$m.Groups[1].Value
}

function Set-KBotFileVersion {
    param([string]$Project, [version]$NewVersion)
    $bytes = [System.IO.File]::ReadAllBytes($Project)
    $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $text = [System.IO.File]::ReadAllText($Project)
    $rx = [regex]'<FileVersion>\s*[0-9.]+\s*</FileVersion>'
    if (-not $rx.IsMatch($text)) { throw "No <FileVersion> element in $Project." }
    $text = $rx.Replace($text, "<FileVersion>$NewVersion</FileVersion>", 1)
    [System.IO.File]::WriteAllText($Project, $text, (New-Object System.Text.UTF8Encoding($hasBom)))
}

# ---- signing (one file: KBot.Tray.exe) --------------------------------------------------------
function Resolve-SignTool {
    $cmd = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $roots = @("${env:ProgramFiles(x86)}\Windows Kits\10\bin", "${env:ProgramFiles}\Windows Kits\10\bin") |
        Where-Object { $_ -and (Test-Path $_) }
    foreach ($root in $roots) {
        $candidate = Get-ChildItem -Path $root -Recurse -Filter 'signtool.exe' -File -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match '\\x64\\' } | Sort-Object FullName -Descending | Select-Object -First 1
        if ($candidate) { return $candidate.FullName }
    }
    return $null
}

# Unlike publish-release.ps1 (where an unsigned build can still be useful) the question was answered
# explicitly here: when the operator said Yes and the signing cannot be done, the push STOPS.
function Invoke-TraySign {
    param([string]$File)
    $tp = ($SignThumbprint -replace '\s', '').ToUpperInvariant()
    if (-not $tp -and $env:KBOT_SIGN_THUMBPRINT) { $tp = ($env:KBOT_SIGN_THUMBPRINT -replace '\s', '').ToUpperInvariant() }
    if (-not $tp) { throw "No signing thumbprint (-SignThumbprint or KBOT_SIGN_THUMBPRINT). Use -Sign No to push unsigned." }
    $signtool = Resolve-SignTool
    if (-not $signtool) { throw "signtool.exe not found (Windows SDK missing?). Use -Sign No to push unsigned." }
    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
        Where-Object { $_.Thumbprint -eq $tp } | Select-Object -First 1
    if (-not $cert) {
        throw "Certificate $tp not found in CurrentUser\My. Is SimplySign Desktop running and logged in? (-Sign No pushes unsigned.)"
    }
    Write-Step "Signing $File with $($cert.Subject) (one SimplySign confirmation) ..."
    & $signtool sign /fd SHA256 /tr $TimestampUrl /td SHA256 /sha1 $tp /v $File 2>&1 | ForEach-Object { Write-Verbose $_ }
    if ($LASTEXITCODE -ne 0) { throw "signtool returned $LASTEXITCODE for $File." }
    Write-Ok "Signed."
}

function Resolve-InnoCompiler {
    $cmd = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    foreach ($c in @("${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe", "${env:ProgramFiles}\Inno Setup 6\ISCC.exe")) {
        if ($c -and (Test-Path -LiteralPath $c)) { return $c }
    }
    return $null
}

# The sign command Inno Setup runs on Setup.exe and the uninstaller ($q / $f are Inno's quote / file placeholders).
function Get-InnoSignCommand {
    $tp = ($SignThumbprint -replace '\s', '').ToUpperInvariant()
    $signtool = Resolve-SignTool
    if (-not $tp -or -not $signtool) { throw "Cannot sign the installer: no thumbprint or no signtool.exe. Use -Sign No." }
    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
        Where-Object { $_.Thumbprint -eq $tp } | Select-Object -First 1
    if (-not $cert) { throw "Certificate $tp not found in CurrentUser\My (SimplySign Desktop running and logged in?). Use -Sign No." }
    return ('$q{0}$q sign /fd SHA256 /tr {1} /td SHA256 /sha1 {2} $f' -f $signtool, $TimestampUrl, $tp)
}

# GET /api/update/tray/latest; $null when nothing is published (404).
function Get-ServerTray {
    param([string]$BaseUrl)
    $url = $BaseUrl.TrimEnd('/') + '/api/update/tray/latest'
    try {
        $resp = Invoke-WebRequest -Uri $url -Method Get -TimeoutSec 30 -UseBasicParsing
        $bytes = $resp.RawContentStream.ToArray()
        return ([System.Text.Encoding]::UTF8.GetString($bytes) | ConvertFrom-Json)
    } catch {
        $r = $null
        if ($_.Exception.PSObject.Properties.Name -contains 'Response') { $r = $_.Exception.Response }
        if ($r -and [int]$r.StatusCode -eq 404) { return $null }
        throw "GET $url failed: $($_.Exception.Message)"
    }
}

# A zip with FORWARD slashes in the entry names (ZipFile::CreateFromDirectory of Windows PowerShell 5.1
# writes backslashes), no top folder: the tray unpacks it straight into its own folder.
function New-TrayZip {
    param([string]$SourceDir, [string]$ZipPath)
    Add-Type -AssemblyName 'System.IO.Compression'
    Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
    if (Test-Path -LiteralPath $ZipPath) { Remove-Item -LiteralPath $ZipPath -Force }
    $root = (Resolve-Path -LiteralPath $SourceDir).Path.TrimEnd('\')
    $fs = [System.IO.File]::Open($ZipPath, [System.IO.FileMode]::CreateNew)
    try {
        $zip = New-Object System.IO.Compression.ZipArchive($fs, [System.IO.Compression.ZipArchiveMode]::Create)
        try {
            foreach ($f in (Get-ChildItem -LiteralPath $root -Recurse -File)) {
                $name = $f.FullName.Substring($root.Length + 1).Replace('\', '/')
                [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $f.FullName, $name, [System.IO.Compression.CompressionLevel]::Optimal)
            }
        } finally { $zip.Dispose() }
    } finally { $fs.Dispose() }
}

# ==============================================================================
#  MAIN
# ==============================================================================

$SolutionRoot = Find-SolutionRoot
$ArtifactsDir = Join-Path $SolutionRoot 'artifacts'
$TrayProj     = Join-Path $SolutionRoot 'src\KBot.Tray\KBot.Tray.vbproj'
if (-not (Test-Path -LiteralPath $TrayProj)) { throw "Project not found: $TrayProj" }
if ([string]::IsNullOrWhiteSpace($SettingsPath)) { $SettingsPath = Join-Path $SolutionRoot '_push\push_settings.json' }

# --- 1. Prerequisites first: fail before the build, not after ------------------
$settings  = Read-PushSettings -Path $SettingsPath
$sftp      = Resolve-Sftp
$RemoteDir = "$($settings.RemoteRoot)/updates"
Write-Step "Target: $($settings.User)@$($settings.Host):$($settings.Port)  $RemoteDir"

$iscc = $null
if ($Installer -eq 'Yes') {
    $iscc = Resolve-InnoCompiler
    if (-not $iscc) { throw 'ISCC.exe (Inno Setup 6) not found. Install it, or pass -Installer No.' }
}

# --- 2. Questions: sign? bump? --------------------------------------------------
$signNow = switch ($Sign) {
    'Yes' { $true }
    'No'  { $false }
    default {
        Read-InteractiveChoice -Question 'Sign KBot.Tray.exe (1 SimplySign confirmation, +2 for the installer)? (Y)es / (N)o [Y]' `
            -NoConsoleMessage 'Signing: no interactive console -> Yes (pass -Sign No to skip).' -NoConsoleValue $true `
            -Map @{ '' = $true; 'Y' = $true; 'YES' = $true; 'N' = $false; 'NO' = $false } -Hint 'Type Y or N (Enter = Y).'
    }
}
$current  = Get-KBotFileVersion -Project $TrayProj
$bumpPart = if ($Bump -ne 'Ask') { $Bump } else {
    Read-InteractiveChoice -Question "Bump the tray FileVersion $current ? (M)ajor, m(i)nor, (b)uild, (r)evision, (N)one [N]" `
        -NoConsoleMessage 'Version bump: no interactive console -> None (pass -Bump to choose).' -NoConsoleValue 'None' `
        -Map @{ '' = 'None'; 'N' = 'None'; 'NONE' = 'None'; 'M' = 'Major'; 'MAJOR' = 'Major'; 'I' = 'Minor'; 'MINOR' = 'Minor';
                'B' = 'Build'; 'BUILD' = 'Build'; 'R' = 'Revision'; 'REVISION' = 'Revision' } `
        -Hint 'Type M, i, b, r or N (Enter = N).'
}
if ($bumpPart -ne 'None') {
    $new = Step-KBotVersion -Version $current -Part $bumpPart
    Set-KBotFileVersion -Project $TrayProj -NewVersion $new
    Write-Step "FileVersion of KBot.Tray: $current -> $new  (KBot.Tray.vbproj)"
    $current = $new
}
$version = $current

# --- 3. What the server has ----------------------------------------------------
$server = Get-ServerTray -BaseUrl $ApiBaseUrl
if ($server) {
    $serverVersion = [version]$server.version
    Write-Step "Server has the tray: version $serverVersion"
    if ($version -le $serverVersion -and -not $Force) {
        $what = if ($version -eq $serverVersion) { "already has $serverVersion" } else { "has $serverVersion, NEWER than $version" }
        Write-Warning "[tray] The server $what."
        if (-not (Read-YesNo "Overwrite it with $version?")) {
            Write-Host "[tray] Nothing pushed." -ForegroundColor Yellow
            return
        }
    }
} else {
    Write-Step 'Server has no published tray yet (404).'
}

# --- 4. Build: publish the tray folder, keep only KBot.Tray.exe as an exe -------
$Stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$Stage = Join-Path $ArtifactsDir "_tray_$Stamp"
if (Test-Path -LiteralPath $Stage) { Remove-Item -LiteralPath $Stage -Recurse -Force }
New-Item -ItemType Directory -Path $ArtifactsDir -Force | Out-Null
Write-Step "Publish KBot.Tray ($Configuration, $Rid, framework-dependent) ..."
& dotnet publish $TrayProj -c $Configuration -r $Rid --self-contained false -o $Stage
if ($LASTEXITCODE -ne 0) { throw "dotnet publish KBot.Tray failed (ExitCode=$LASTEXITCODE)." }

$trayExe = Join-Path $Stage 'KBot.Tray.exe'
if (-not (Test-Path -LiteralPath $trayExe)) { throw "KBot.Tray.exe missing from $Stage after publish." }

# No other program in the package: a signer must have nothing else to double-click.
$driver = Join-Path $Stage '.playwright'
if (Test-Path -LiteralPath $driver) {
    Remove-Item -LiteralPath $driver -Recurse -Force
    Write-Step 'Left out the Playwright driver (.playwright): the signer edition does not run the robot.'
}
$others = @(Get-ChildItem -LiteralPath $Stage -Recurse -File -Filter '*.exe' |
            Where-Object { $_.FullName -ne (Resolve-Path -LiteralPath $trayExe).Path })
foreach ($o in $others) {
    Write-Step "Left out the program $($o.FullName.Substring($Stage.Length + 1))"
    Remove-Item -LiteralPath $o.FullName -Force
}
$leftover = @(Get-ChildItem -LiteralPath $Stage -Recurse -File -Filter '*.exe')
if ($leftover.Count -ne 1) { throw "The package must hold exactly one .exe (KBot.Tray.exe); found $($leftover.Count)." }

# The exe's own FileVersion is the version the server will announce: they must agree.
$exeVersion = [version][System.Diagnostics.FileVersionInfo]::GetVersionInfo($trayExe).FileVersion
if ($exeVersion -ne $version) { throw "KBot.Tray.exe says $exeVersion but the project says $version." }

# --- 5. Sign ONLY KBot.Tray.exe -------------------------------------------------
if ($signNow) { Invoke-TraySign -File $trayExe }
else { Write-Host '[tray] Signing: OFF -- KBot.Tray.exe stays UNSIGNED.' -ForegroundColor Yellow }

# --- 5b. The installer for NEW signers (per-user, no administrator) -----------------
$setupExe = $null
if ($Installer -eq 'Yes') {
    $setupName = "KBot_Tray_Setup_$version"
    $iss = Join-Path $SolutionRoot 'tools\KBotTrayInstaller\KBotTray.iss'
    if (-not (Test-Path -LiteralPath $iss)) { throw "Installer script not found: $iss" }
    $isccArgs = @("/DSourceDir=$Stage", "/DAppVersion=$version", "/DOutputDir=$ArtifactsDir", "/DOutputBaseFilename=$setupName", '/Qp')
    if ($signNow) {
        $isccArgs += '/DSIGN=1'
        $isccArgs += "/Skbotsign=$(Get-InnoSignCommand)"
        Write-Step 'Inno Setup will sign Setup.exe + the uninstaller (2 more SimplySign confirmations).'
    }
    $isccArgs += $iss
    Write-Step "Compiling the installer -> $setupName.exe"
    & $iscc @isccArgs
    if ($LASTEXITCODE -ne 0) { throw "ISCC failed (ExitCode=$LASTEXITCODE)." }
    $setupExe = Join-Path $ArtifactsDir "$setupName.exe"
    if (-not (Test-Path -LiteralPath $setupExe)) { throw "ISCC reported success but $setupExe is missing." }
    Write-Ok "Installer: $setupExe ($([Math]::Round((Get-Item -LiteralPath $setupExe).Length / 1MB, 1)) MB)"
}

# --- 6. Zip + tray.json ----------------------------------------------------------
$remoteFile = "KBot_Tray_$version.zip"
$zipPath    = Join-Path $ArtifactsDir $remoteFile
New-TrayZip -SourceDir $Stage -ZipPath $zipPath
Remove-Item -LiteralPath $Stage -Recurse -Force
$zipInfo = Get-Item -LiteralPath $zipPath
$sha = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Step "Package: $zipPath ($([Math]::Round($zipInfo.Length / 1MB, 1)) MB)"
Write-Step "sha256: $sha"

$block = [ordered]@{
    version       = $version.ToString()
    file          = $remoteFile
    size          = [long]$zipInfo.Length
    sha256        = $sha
    published_utc = [DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')
    notes         = $Notes
}
$tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("kbot_tray_push_" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tmp -Force | Out-Null
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$jsonPath = Join-Path $tmp 'tray.json'
[System.IO.File]::WriteAllText($jsonPath, ($block | ConvertTo-Json -Depth 3), $utf8NoBom)
Write-Step "tray.json:`n$(Get-Content -LiteralPath $jsonPath -Raw)"

# --- 7. Upload: one sftp session, package first, tray.json LAST ------------------
#  '-' prefix = ignore that command's failure (folder already there, no old file).
$batchPath = Join-Path $tmp 'batch.sftp'
$batch = @(
    "-mkdir $RemoteDir",
    "cd $RemoteDir",
    "put ""$zipPath"" $remoteFile.part",
    "-rm $remoteFile",
    "rename $remoteFile.part $remoteFile",
    "put ""$jsonPath"" tray.json.part",
    "-rm tray.json",
    "rename tray.json.part tray.json",
    "bye"
)
[System.IO.File]::WriteAllText($batchPath, ($batch -join "`n") + "`n", $utf8NoBom)
Write-Step 'Uploading over sftp (type the password at the OpenSSH prompt) ...'
& $sftp -P $settings.Port -b $batchPath "$($settings.User)@$($settings.Host)"
if ($LASTEXITCODE -ne 0) { throw "sftp failed (ExitCode=$LASTEXITCODE). Nothing may be assumed about the server state -- check $RemoteDir." }

# --- 8. Verify through the API, the way the tray will see it ---------------------
$after = Get-ServerTray -BaseUrl $ApiBaseUrl
if (-not $after) { throw "Upload finished but GET /api/update/tray/latest still returns 404. Is the new routes\update.py (tray channel) on the server and restarted?" }
if ([version]$after.version -ne $version) { throw "Upload finished but the server reports version $($after.version), expected $version." }
if ($after.sha256 -ne $sha) { throw 'Server sha256 differs from the local package.' }

Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue
Write-Host ''
Write-Ok "PUBLISHED (tray)  version $version   ($remoteFile, $([Math]::Round($zipInfo.Length / 1MB, 1)) MB)$(if (-not $signNow) { '  UNSIGNED' })"
if ($setupExe) { Write-Ok "Installer for new signers (hand it out; NOT on the server): $setupExe" }
Write-Host "  The tray reads: $($ApiBaseUrl.TrimEnd('/'))/api/update/tray/latest"
