# Builds and publishes ONLY AVC.Migrator (src\AVC.Migrator), with all its parts, into <repo root>\_AvcMigrator
# -> AVC.Migrator.exe. The folder is git-ignored and is NOT part of any K-BOT delivery.
# Needs the .NET 8 Desktop Runtime on the PC that runs it (framework-dependent, x64).
# (VRC.Migrator and ADE.Migrator have their own script: publish-vrc-migrator.ps1.)
#
# Usage:  .\publish-avc-migrator.ps1 [-Configuration Release|Debug] [-Output <folder>] [-Clean]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',
    [string]$Output,
    [switch]$Clean
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
    dotnet publish $proj -c $Configuration -o $out --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $proj ($LASTEXITCODE)" }

    # KBot.Migrator is referenced as a library only: its DLL stays, its own exe/config files are not part of the package
    foreach ($f in 'KBot.Migrator.exe', 'KBot.Migrator.deps.json', 'KBot.Migrator.runtimeconfig.json') {
        $p = Join-Path $out $f
        if (Test-Path $p) { Remove-Item $p -Force }
    }
}
finally {
    if (Test-Path $keepDir) {
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
Write-Host "AVC.Migrator $((Get-Item $exe).VersionInfo.FileVersion) ($Configuration) -> $exe"
