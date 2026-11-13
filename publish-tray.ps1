<#
.SYNOPSIS
  LOCAL ONLY (developer's PC): publish, start and stop KBot.Tray (the signer edition, notification area) for tests.

.DESCRIPTION
  Slice TRAY-02: the tray is a program of its own. Its folder holds KBot.Tray.exe and the KBot.App.dll files it opens the signing
  windows from, and it replaces its own files when it updates, so it is tested from a published folder (F5 from Visual Studio does
  not do that). The default folder is under src\KBot.Tray\bin\<Configuration>\..., so the tray treats it as a developer's build:
  it never writes the Windows Run key and never updates itself.
  Nothing here is part of any release: push-tray.ps1 builds and publishes the package.

  -Action Run      stop a running tray, publish, start it            (default)
  -Action Publish  stop a running tray, publish only
  -Action Start    start the published tray (does not publish)
  -Action Stop     stop the running tray

.PARAMETER Configuration
  Debug (default) or Release.

.PARAMETER OutputDir
  Where the tray folder lands / is started from. Default: src\KBot.Tray\bin\<Configuration>\net8.0-windows\local.

.EXAMPLE
  .\publish-tray.ps1              # publish + start
  .\publish-tray.ps1 -Action Stop
#>
[CmdletBinding()]
param(
    [ValidateSet('Run', 'Publish', 'Start', 'Stop')]
    [string]$Action = 'Run',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [string]$OutputDir
)

$ErrorActionPreference = 'Stop'
$SolutionRoot = $PSScriptRoot
$Rid = 'win-x64'

if (-not $OutputDir) {
    $OutputDir = Join-Path $SolutionRoot "src\KBot.Tray\bin\$Configuration\net8.0-windows\local"
}
$TrayExe = Join-Path $OutputDir 'KBot.Tray.exe'

function Stop-Tray {
    # A running tray holds its own files; a new one would find the single-instance lock taken and exit without doing anything.
    $running = @(Get-Process -Name 'KBot.Tray' -ErrorAction SilentlyContinue)
    if ($running.Count -eq 0) {
        Write-Host 'No tray is running.' -ForegroundColor DarkGray
        return
    }
    Write-Host "Stopping the tray ($($running.Count) process) ..." -ForegroundColor Yellow
    $running | Stop-Process -Force
    $running | Wait-Process -Timeout 10 -ErrorAction SilentlyContinue
    Write-Host 'Stopped.' -ForegroundColor Green
}

function Publish-Tray {
    $proj = Join-Path $SolutionRoot 'src\KBot.Tray\KBot.Tray.vbproj'
    if (-not (Test-Path -LiteralPath $proj)) { throw "Project not found: $proj" }
    Write-Host "Publish KBot.Tray ($Configuration, framework-dependent folder) -> $OutputDir" -ForegroundColor Cyan
    & dotnet publish $proj `
        -c $Configuration `
        -r $Rid `
        --self-contained false `
        -o $OutputDir
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish KBot.Tray failed (ExitCode=$LASTEXITCODE)." }
    if (-not (Test-Path -LiteralPath $TrayExe)) { throw "KBot.Tray.exe missing from $OutputDir after publish." }
    Write-Host "Tray OK: $TrayExe" -ForegroundColor Green
}

function Start-Tray {
    if (-not (Test-Path -LiteralPath $TrayExe)) { throw "KBot.Tray.exe not found: $TrayExe. Run with -Action Publish first." }
    Start-Process -FilePath $TrayExe -WorkingDirectory $OutputDir
    Write-Host 'Started. The icon appears next to the clock (maybe under the ^ arrow); right click -> Inchide to close it.' -ForegroundColor Cyan
}

switch ($Action) {
    'Stop'    { Stop-Tray }
    'Start'   { Start-Tray }
    'Publish' { Stop-Tray; Publish-Tray }
    'Run'     { Stop-Tray; Publish-Tray; Start-Tray }
}
