<#
================================================================================
  publish-debug.ps1
  Publica KBot.App in DEBUG si produce un .zip de copiat pe alt PC.

  DEBUG => Program.vb intra pe ramura #If DEBUG:
           - porneste in DevHarnessForm (bancul de teste);
           - butonul "Deschide MainForm" deschide shell-ul real la cerere;
           - KBot.DevHarness este INCLUS (referinta conditionata pe Debug in .vbproj).

  Parametri publish (conform deciziilor confirmate):
           - self-contained (nu cere .NET instalat pe PC-ul userului)
           - RID win-x64
           - FARA PublishSingleFile (Playwright are nevoie de .playwright\ pe disc)

  Cerinte pe PC-ul de BUILD: .NET 8 SDK in PATH (dotnet).
  Plasare: pune acest script in radacina solutiei (langa KBot.sln) sau sub ea.
================================================================================
#>

[CmdletBinding()]
param(
    [string] $Rid = 'win-x64'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$Configuration = 'Debug'

# --- 0. Gaseste radacina solutiei (urca pana la KBot.sln) -----------------------
function Find-SolutionRoot {
    param([string] $Start)
    $dir = $Start
    for ($i = 0; $i -lt 4; $i++) {
        if (Test-Path (Join-Path $dir 'KBot.sln')) { return $dir }
        $parent = Split-Path $dir -Parent
        if ([string]::IsNullOrEmpty($parent) -or $parent -eq $dir) { break }
        $dir = $parent
    }
    return $null
}

$startDir = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }
$SolutionRoot = Find-SolutionRoot -Start $startDir
if (-not $SolutionRoot) {
    throw "Nu gasesc KBot.sln pornind din '$startDir' (am urcat 4 niveluri). Pune scriptul in/sub radacina solutiei."
}

$ProjectFile = Join-Path $SolutionRoot 'src\KBot.App\KBot.App.vbproj'
if (-not (Test-Path $ProjectFile)) {
    throw "Nu gasesc proiectul: $ProjectFile"
}

# --- 1. Verifica dotnet SDK -----------------------------------------------------
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet SDK nu este in PATH pe acest PC (de build). Instaleaza .NET 8 SDK."
}

# --- 2. Nume / cai --------------------------------------------------------------
$Stamp         = Get-Date -Format 'yyyyMMdd_HHmmss'
$AppFolderName = "KBot_${Configuration}_$Stamp"          # folderul de la radacina zip-ului
$ArtifactsDir  = Join-Path $SolutionRoot 'artifacts'
$PublishDir    = Join-Path $ArtifactsDir $AppFolderName  # staging
$ZipPath       = Join-Path $ArtifactsDir "$AppFolderName.zip"

# --- 3. Curata staging anterior (livrabil curat de fiecare data) ----------------
if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }
if (Test-Path $ZipPath)    { Remove-Item $ZipPath -Force }
New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null

# --- 4. Publish -----------------------------------------------------------------
Write-Host "Publish $Configuration ($Rid, self-contained)..." -ForegroundColor Cyan
& dotnet publish $ProjectFile `
    -c $Configuration `
    -r $Rid `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o $PublishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish a esuat (ExitCode=$LASTEXITCODE)."
}

# --- 5. Folder Workflows (gol) — .wfl se pune MANUAL pe PC-ul userului -----------
$WorkflowsDir = Join-Path $PublishDir 'Workflows'
New-Item -ItemType Directory -Path $WorkflowsDir -Force | Out-Null
@"
Pune aici fisierele workflow (.wfl) pe PC-ul userului.
Necesar pentru conectare: adlop - Conectare.wfl
"@ | Set-Content -Path (Join-Path $WorkflowsDir '_CITESTE_pune_wfl_aici.txt') -Encoding UTF8

# --- 6. Folder Logs (gol) — defensiv: garanteaza primul write de log ------------
$LogsDir = Join-Path $PublishDir 'Logs'
New-Item -ItemType Directory -Path $LogsDir -Force | Out-Null
'Folder de loguri (creat la publish).' | Set-Content -Path (Join-Path $LogsDir '_keep.txt') -Encoding UTF8

# --- 7. Arhivare → un singur .zip (cu folder-radacina cu nume descriptiv) -------
if (-not ('System.IO.Compression.ZipFile' -as [type])) {
    Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
}
Write-Host "Arhivez -> $ZipPath" -ForegroundColor Cyan
[System.IO.Compression.ZipFile]::CreateFromDirectory(
    $PublishDir, $ZipPath,
    [System.IO.Compression.CompressionLevel]::Optimal,
    $true)   # includeBaseDirectory = True => in zip exista folderul $AppFolderName

# --- 8. Curata staging-ul (ramane doar .zip-ul) ---------------------------------
Remove-Item $PublishDir -Recurse -Force

# --- 9. Raport ------------------------------------------------------------------
$ZipSizeMB = [Math]::Round((Get-Item $ZipPath).Length / 1MB, 1)
Write-Host ""
Write-Host "GATA (DEBUG)." -ForegroundColor Green
Write-Host "  Comportament : porneste in DevHarnessForm (bancul de teste)."
Write-Host "  Livrabil     : $ZipPath  ($ZipSizeMB MB)"
Write-Host "  Pe alt PC    : dezarhiveaza => folderul '$AppFolderName' => ruleaza KBot.App.exe"
Write-Host "  Workflows    : pune .wfl manual in '$AppFolderName\Workflows\' (langa .exe)"