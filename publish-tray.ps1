<#
.SYNOPSIS
  LOCAL ONLY (developer's PC): publish, start and stop KBot.Tray (the notification-area application) for tests.

.DESCRIPTION
  The tray runs only as a published single file (it copies itself to %LOCALAPPDATA% at start), so F5 from Visual Studio
  does not work for it. Run from a build folder (...\bin\Debug\...) it never writes the Windows Run key.
  Nothing here is part of the release: publish-release.ps1 builds KBot.Tray.exe for the package on its own (step 4c3).

  -Action Run      stop a running tray, publish, start it            (default)
  -Action Publish  stop a running tray, publish only
  -Action Start    start the published tray (does not publish)
  -Action Stop     stop the running tray

.PARAMETER Configuration
  Debug (default) or Release.

.PARAMETER OutputDir
  Where KBot.Tray.exe lands / is started from. Default: the build folder of KBot.App for the chosen configuration, so the
  tray finds KBot.App.exe beside it (a click on a document starts K-BOT from there).

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
    $OutputDir = Join-Path $SolutionRoot "src\KBot.App\bin\$Configuration\net8.0-windows"
}
$TrayExe = Join-Path $OutputDir 'KBot.Tray.exe'

function Stop-Tray {
    # A running tray holds its copy in %LOCALAPPDATA%; a new launcher would find it and exit without doing anything.
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
    if (-not (Test-Path -LiteralPath $OutputDir)) {
        throw "Folder not found: $OutputDir. Build KBot.App ($Configuration) first, or pass -OutputDir."
    }
    Write-Host "Publish KBot.Tray ($Configuration, single-file) -> $OutputDir" -ForegroundColor Cyan
    & dotnet publish $proj `
        -c $Configuration `
        -r $Rid `
        --self-contained false `
        -p:PublishSingleFile=true `
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
