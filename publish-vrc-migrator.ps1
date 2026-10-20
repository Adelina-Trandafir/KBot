# Builds and publishes VRC.Migrator (VERCON\VRC.Migrator) and ADE.Migrator (ADECHIT\ADE.Migrator), with all their
# parts, into ONE folder: <repo root>\_VrcMigrator  ->  VRC.Migrator.exe and ADE.Migrator.exe side by side.
# The folder is git-ignored and is NOT part of any K-BOT delivery.
# Needs the .NET 8 Desktop Runtime on the PC that runs them (framework-dependent, x64).
#
# Usage:  .\publish-vrc-migrator.ps1 [-Configuration Release|Debug] [-Output <folder>] [-Clean]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',
    [string]$Output,
    [switch]$Clean
)
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$out = if ($Output) { $Output } else { Join-Path $root '_VrcMigrator' }
$projects = @(
    (Join-Path $root 'VERCON\VRC.Migrator\VRC.Migrator.vbproj'),
    (Join-Path $root 'ADECHIT\ADE.Migrator\ADE.Migrator.vbproj')
)
foreach ($p in $projects) { if (-not (Test-Path $p)) { throw "Project not found: $p" } }

foreach ($n in 'VRC.Migrator', 'ADE.Migrator') {
    if (Get-Process -Name $n -ErrorAction SilentlyContinue) { throw "$n.exe is running - close it before publishing." }
}

# keep the operator's own settings and logs across publishes (all live next to the exes)
$keepDir = Join-Path ([IO.Path]::GetTempPath()) ('migrators-keep-' + [guid]::NewGuid().ToString('N'))
$keepItems = 'vrc-migrator-settings.json', 'ade-migrator-settings.json', 'Logs', 'Jurnale'
if (Test-Path $out) {
    New-Item -ItemType Directory -Path $keepDir | Out-Null
    foreach ($k in $keepItems) {
        $src = Join-Path $out $k
        if (Test-Path $src) { Copy-Item $src (Join-Path $keepDir $k) -Recurse }
    }
    Remove-Item $out -Recurse -Force
}

try {
    foreach ($proj in $projects) {
        if ($Clean) {
            dotnet clean $proj -c $Configuration --nologo -v q
            if ($LASTEXITCODE -ne 0) { throw "dotnet clean failed ($LASTEXITCODE)" }
        }
        # publish builds the project and its ProjectReferences first; the shared DLLs overwrite each other (same build)
        dotnet publish $proj -c $Configuration -o $out --nologo
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $proj ($LASTEXITCODE)" }
    }

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

foreach ($n in 'VRC.Migrator', 'ADE.Migrator') {
    $exe = Join-Path $out "$n.exe"
    if (-not (Test-Path $exe)) { throw "Publish finished but $exe is missing." }
    Write-Host "$n $((Get-Item $exe).VersionInfo.FileVersion) ($Configuration) -> $exe"
}
