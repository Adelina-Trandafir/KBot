# Builds and publishes ONLY AVC.Migrator (src\AVC.Migrator) as ONE small file: <repo root>\_AvcMigrator\AVC.Migrator.exe
# (single-file, framework-dependent, x64, no .pdb). Copy that .exe by hand to the user's PC and run it there --
# nothing to install besides what the PC needs anyway:
#   - the .NET 8 Desktop Runtime (x64)            (use -SelfContained if a PC does not have it: bigger exe, ~70 MB, compressed)
#   - the Microsoft Access Database Engine (ACE), 64-bit, to read the .accdb
# The exe writes its own settings (avc-migrator-settings.json), Logs\ and Jurnale\ next to itself.
# The folder is git-ignored and is NOT part of any K-BOT delivery.
# (VRC.Migrator and ADE.Migrator have their own script: publish-vrc-migrator.ps1.)
#
# Signing (slice ME01-35, optional, never fatal): the finished exe is signed with the Certum SimplySign certificate, the same signtool call as
# publish-release.ps1. -Sign Ask (default) puts ONE question on the console, Yes signs, No skips (the SimplySign token is never asked).
# SimplySign Desktop must be running and logged in. No certificate / no signtool / a signtool failure = a warning and the unsigned exe stays.
#
# Usage:  .\publish-avc-migrator.ps1 [-Configuration Release|Debug] [-Output <folder>] [-Clean] [-SelfContained] [-Sign Ask|Yes|No] [-SignThumbprint <sha1>]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',
    [string]$Output,
    [switch]$Clean,
    [switch]$SelfContained,
    [ValidateSet('Ask', 'Yes', 'No')]
    [string]$Sign = 'Ask',
    # Certum SimplySign certificate thumbprint (SHA1, no spaces); empty = env KBOT_SIGN_THUMBPRINT, then no signing. Same default as publish-release.ps1.
    [string]$SignThumbprint = '2F0E82DB6781F3DC17D43166A3420B7CBE380A68',
    [string]$TimestampUrl = 'http://time.certum.pl'
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$out = if ($Output) { $Output } else { Join-Path $root '_AvcMigrator' }
$proj = Join-Path $root 'src\AVC.Migrator\AVC.Migrator.vbproj'
if (-not (Test-Path $proj)) { throw "Project not found: $proj" }

if (Get-Process -Name 'AVC.Migrator' -ErrorAction SilentlyContinue) { throw 'AVC.Migrator.exe is running - close it before publishing.' }

# keep the operator's own settings and logs across publishes (all live next to the exe)
$keepDir = Join-Path ([IO.Path]::GetTempPath()) ('avc-migrator-keep-' + [guid]::NewGuid().ToString('N'))
$keepItems = 'avc-migrator-settings.json', 'Logs', 'Jurnale'
if (Test-Path $out) {
    New-Item -ItemType Directory -Path $keepDir | Out-Null
    foreach ($k in $keepItems) {
        $src = Join-Path $out $k
        if (Test-Path $src) { Copy-Item $src (Join-Path $keepDir $k) -Recurse }
    }
    Remove-Item $out -Recurse -Force
}

try {
    if ($Clean) {
        dotnet clean $proj -c $Configuration --nologo -v q
        if ($LASTEXITCODE -ne 0) { throw "dotnet clean failed ($LASTEXITCODE)" }
    }
    # publish builds the project and its ProjectReferences first
    # one file: managed DLLs bundled into the exe; embedded debug info keeps line numbers in the error log without a .pdb
    # (the single-file settings are in AVC.Migrator.vbproj, switched on by AvcSingleFile)
    $pubArgs = @('-p:AvcSingleFile=true')
    if ($SelfContained) { $pubArgs += '-p:AvcSelfContained=true' }
    dotnet publish $proj -c $Configuration -o $out --nologo @pubArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $proj ($LASTEXITCODE)" }

    # KBot.Migrator is referenced as a library only (bundled into the exe): its own exe/config files are not part of the package
    foreach ($f in 'KBot.Migrator.exe', 'KBot.Migrator.deps.json', 'KBot.Migrator.runtimeconfig.json', 'KBot.Migrator.pdb') {
        $p = Join-Path $out $f
        if (Test-Path $p) { Remove-Item $p -Force }
    }
    # the referenced projects' .pdb files are not bundled and not needed (AVC.Migrator's own is embedded)
    Get-ChildItem $out -File -Filter '*.pdb' | Remove-Item -Force
}
finally {
    if (Test-Path $keepDir) {
        # a failed publish leaves no output folder: recreate it so the settings and logs are not lost
        if (-not (Test-Path $out)) { New-Item -ItemType Directory -Path $out | Out-Null }
        if (Test-Path $out) {
            foreach ($k in $keepItems) {
                $src = Join-Path $keepDir $k
                if (Test-Path $src) { Copy-Item $src (Join-Path $out $k) -Recurse -Force }
            }
        }
        Remove-Item $keepDir -Recurse -Force
    }
}

$exe = Join-Path $out 'AVC.Migrator.exe'
if (-not (Test-Path $exe)) { throw "Publish finished but $exe is missing." }

# ---- signing (never fatal) ----
function Read-SignChoice {
    # One question on the console; Enter = Yes.
    if (-not [Environment]::UserInteractive -or [Console]::IsInputRedirected) {
        Write-Host 'Signing: no interactive console -> Yes (pass -Sign No to skip).' -ForegroundColor Yellow
        return $true
    }
    while ($true) {
        $answer = Read-Host 'Sign AVC.Migrator.exe (SimplySign confirmation)? (Y)es / (N)o [Y]'
        $key = ($answer -replace '\s', '').ToUpperInvariant()
        if ($key -in @('', 'Y', 'YES')) { return $true }
        if ($key -in @('N', 'NO')) { return $false }
        Write-Host '  Type Y or N (Enter = Y).' -ForegroundColor Yellow
    }
}

function Resolve-SignTool {
    $cmd = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $roots = @("${env:ProgramFiles(x86)}\Windows Kits\10\bin", "${env:ProgramFiles}\Windows Kits\10\bin") | Where-Object { $_ -and (Test-Path $_) }
    foreach ($r in $roots) {
        $found = Get-ChildItem -Path $r -Recurse -Filter 'signtool.exe' -File -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match '\\x64\\' } | Sort-Object FullName -Descending | Select-Object -First 1
        if ($found) { return $found.FullName }
    }
    return $null
}

function Invoke-AvcSign {
    param([string]$File)
    $tp = if ($SignThumbprint) { $SignThumbprint } elseif ($env:KBOT_SIGN_THUMBPRINT) { $env:KBOT_SIGN_THUMBPRINT } else { '' }
    $tp = ($tp -replace '\s', '').ToUpperInvariant()
    if (-not $tp) { Write-Warning '[sign] No signing thumbprint configured (KBOT_SIGN_THUMBPRINT). The exe stays unsigned.'; return $false }
    $signtool = Resolve-SignTool
    if (-not $signtool) { Write-Warning '[sign] signtool.exe not found (Windows SDK missing?). The exe stays unsigned.'; return $false }
    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue | Where-Object { $_.Thumbprint -eq $tp } | Select-Object -First 1
    if (-not $cert) {
        Write-Warning "[sign] Certificate $tp not found in CurrentUser\My. Start SimplySign Desktop, log in, then run again. The exe stays unsigned."
        return $false
    }
    Write-Host "[sign] signtool: $signtool" -ForegroundColor Cyan
    Write-Host "[sign] subject : $($cert.Subject)" -ForegroundColor Cyan
    try {
        & $signtool sign /fd SHA256 /tr $TimestampUrl /td SHA256 /sha1 $tp /v $File 2>&1 | ForEach-Object { Write-Verbose $_ }
        if ($LASTEXITCODE -ne 0) { Write-Warning "[sign] signtool returned $LASTEXITCODE. The exe stays unsigned."; return $false }
    }
    catch {
        Write-Warning "[sign] Signing failed: $($_.Exception.Message). The exe stays unsigned."
        return $false
    }
    return $true
}

$doSign = switch ($Sign) { 'Yes' { $true } 'No' { $false } default { Read-SignChoice } }
$signedNote = 'not signed'
if ($doSign) {
    if (Invoke-AvcSign -File $exe) {
        $sig = Get-AuthenticodeSignature -FilePath $exe
        $signedNote = if ($sig.Status -eq 'Valid') { 'signed' } else { "signed, but Windows says: $($sig.Status)" }
    }
}
# anything else left next to the exe is not needed: the exe is the whole delivery
$extra = Get-ChildItem $out -File | Where-Object { $_.Name -ne 'AVC.Migrator.exe' -and $_.Name -ne 'avc-migrator-settings.json' }
foreach ($e in $extra) { Write-Warning "Extra file next to the exe (not needed): $($e.Name)" }
$mb = [math]::Round((Get-Item $exe).Length / 1MB, 1)
Write-Host "AVC.Migrator $((Get-Item $exe).VersionInfo.FileVersion) ($Configuration, $(if ($SelfContained) { 'self-contained' } else { 'needs .NET 8 Desktop Runtime' }), $mb MB, $signedNote) -> $exe"
