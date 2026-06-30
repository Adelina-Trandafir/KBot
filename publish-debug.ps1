<#
================================================================================
  publish-debug.ps1
  Publica KBot.App in DEBUG si produce un .zip de copiat pe alt PC.

  DEBUG => Program.vb intra pe ramura #If DEBUG:
           - porneste in DevHarnessForm (bancul de teste);
           - butonul "Deschide MainForm" deschide shell-ul real la cerere;
           - KBot.DevHarness este INCLUS (referinta conditionata pe Debug in .vbproj).

  Parametri publish:
           - framework-dependent (clientul are deja .NET Desktop Runtime 8 win-x64)
           - RID win-x64
           - FARA PublishSingleFile (single-file goleste Assembly.Location =>
             Playwright nu mai gaseste .playwright\ => NRE la GetExecutablePath)

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

# --- 0b. Generator manifest update (manifest.xml la radacina output-ului de publish) ---
#  Listeaza DOAR assembly-urile KBot.* (DLL + EXE) cu FileVersion, dimensiune si SHA-256.
#  Autoritatea de comparatie pentru clientul de update (viitor) este FileVersion.
#  Erorile sunt HARD (throw): publish-ul pica zgomotos, nu emite manifest partial.
function New-KBotManifest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [string] $PublishDir,
        [Parameter(Mandatory)] [string] $Configuration,
        [Parameter(Mandatory)] [string] $Runtime,
        [string] $Product = 'K-BOT',
        [int]    $SchemaVersion = 1
    )

    if (-not (Test-Path -LiteralPath $PublishDir -PathType Container)) {
        throw "Publish directory not found: $PublishDir"
    }

    # Doar assembly-urile livrate KBot.* (DLL + EXE) de la radacina output-ului.
    $files = Get-ChildItem -LiteralPath $PublishDir -File |
        Where-Object { $_.Name -like 'KBot.*.dll' -or $_.Name -like 'KBot.*.exe' } |
        Sort-Object Name

    if ($files.Count -eq 0) {
        throw "No KBot.* assemblies found in $PublishDir"
    }

    $generatedUtc = [DateTime]::UtcNow.ToString('o')

    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
    [void]$sb.AppendLine(
        ('<KBotManifest schemaVersion="{0}" product="{1}" generatedUtc="{2}" configuration="{3}" runtime="{4}">' -f `
            $SchemaVersion, $Product, $generatedUtc, $Configuration, $Runtime))

    foreach ($f in $files) {
        $vi  = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($f.FullName)
        $ver = $vi.FileVersion
        if ([string]::IsNullOrWhiteSpace($ver)) {
            # Eroare dura: fiecare assembly KBot.* trebuie sa poarte <FileVersion>.
            throw "Missing FileVersion on $($f.Name) — set <FileVersion> in its .vbproj."
        }
        $hash = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash
        [void]$sb.AppendLine(
            ('  <Component file="{0}" version="{1}" size="{2}" sha256="{3}" />' -f `
                $f.Name, $ver, $f.Length, $hash))
    }

    [void]$sb.AppendLine('</KBotManifest>')

    $manifestPath = Join-Path $PublishDir 'manifest.xml'
    # UTF-8 fara BOM, continut determinist.
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText($manifestPath, $sb.ToString(), $utf8NoBom)

    Write-Host "manifest.xml written: $manifestPath ($($files.Count) components)"
    return $manifestPath
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

# --- 4. Publish (framework-dependent: NU livram runtime-ul .NET) -----------------
#  Clientul TREBUIE sa aiba ".NET Desktop Runtime 8" (win-x64) — app-ul e WinForms+WPF.
#  --self-contained false => fara cele cateva sute de DLL-uri de runtime in output.
#  PublishSingleFile RAMANE false: single-file goleste Assembly.Location, iar
#  Playwright (Driver.GetExecutablePath) nu mai gaseste .playwright\ => exact NRE-ul
#  pe care l-am vanat. Deci NU porni single-file aici.
Write-Host "Publish $Configuration ($Rid, framework-dependent)..." -ForegroundColor Cyan
& dotnet publish $ProjectFile `
    -c $Configuration `
    -r $Rid `
    --self-contained false `
    -p:PublishSingleFile=false `
    -o $PublishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish a esuat (ExitCode=$LASTEXITCODE)."
}

# --- 5. Folder Workflows — copiaza .wfl din src\KBot.Forexe\Workflows in output ---
#  Sursa de adevar pentru workflow-uri. Eroare DURA daca lipsesc (app-ul nu se conecteaza).
$WorkflowsDir    = Join-Path $PublishDir 'Workflows'
$WorkflowsSource = Join-Path $SolutionRoot 'src\KBot.Forexe\Workflows'
New-Item -ItemType Directory -Path $WorkflowsDir -Force | Out-Null

if (-not (Test-Path -LiteralPath $WorkflowsSource -PathType Container)) {
    throw "Folderul sursa de workflow-uri lipseste: $WorkflowsSource"
}
$wfls = @(Get-ChildItem -LiteralPath $WorkflowsSource -Filter '*.wfl' -File)
if ($wfls.Count -eq 0) {
    throw "Niciun .wfl in $WorkflowsSource (necesar cel putin 'adlop - Conectare.wfl')."
}
foreach ($w in $wfls) {
    Copy-Item -LiteralPath $w.FullName -Destination $WorkflowsDir -Force
}
if (-not (Test-Path -LiteralPath (Join-Path $WorkflowsDir 'adlop - Conectare.wfl'))) {
    throw "Lipseste 'adlop - Conectare.wfl' dupa copiere — conectarea nu va functiona."
}
Write-Host "Workflows copiate: $($wfls.Count) fisier(e) .wfl -> $WorkflowsDir" -ForegroundColor Cyan

# --- 6. Folder Logs (gol) — defensiv: garanteaza primul write de log ------------
$LogsDir = Join-Path $PublishDir 'Logs'
New-Item -ItemType Directory -Path $LogsDir -Force | Out-Null
'Folder de loguri (creat la publish).' | Set-Content -Path (Join-Path $LogsDir '_keep.txt') -Encoding UTF8

# --- 6b. Manifest update (manifest.xml la radacina output-ului, inainte de zip) --
#  Trebuie sa fie INAINTE de arhivare ca sa intre in .zip langa DLL-urile KBot.*.
New-KBotManifest -PublishDir $PublishDir -Configuration $Configuration -Runtime $Rid | Out-Null

# --- 7. Arhivare → un singur .zip (cu folder-radacina cu nume descriptiv) -------
if (-not ('System.IO.Compression.ZipFile' -as [type])) {
    Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
}
Write-Host "Arhivez -> $ZipPath" -ForegroundColor Cyan
[System.IO.Compression.ZipFile]::CreateFromDirectory(
    $PublishDir, $ZipPath,
    [System.IO.Compression.CompressionLevel]::Optimal,
    $true)   # includeBaseDirectory = True => in zip exista folderul $AppFolderName

# --- 7b. Self-extractor EXE (KBot_Setup_<stamp>.exe) -----------------------------
#  Stub VB.NET (tools\KBotSetup) publicat single-file + payload (.zip cu CONTINUTUL
#  publish-ului, FARA folder-radacina) atasat la final + footer (Int64 lungime + magic).
#  La dublu-click stub-ul se auto-dezarhiveaza in C:\KBOT, suprascrie tot, fara prompturi.
$SetupStubProj = Join-Path $SolutionRoot 'tools\KBotSetup\KBotSetup.vbproj'
if (-not (Test-Path $SetupStubProj)) { throw "Lipseste proiectul stub: $SetupStubProj" }

$StubOutDir = Join-Path $ArtifactsDir "_setupstub_$Stamp"
Write-Host "Build self-extractor stub..." -ForegroundColor Cyan
& dotnet publish $SetupStubProj -c Release -r $Rid --self-contained false -p:PublishSingleFile=true -o $StubOutDir
if ($LASTEXITCODE -ne 0) { throw "Build stub setup a esuat (ExitCode=$LASTEXITCODE)." }
$StubExe = Join-Path $StubOutDir 'KBotSetup.exe'
if (-not (Test-Path $StubExe)) { throw "Nu gasesc KBotSetup.exe in $StubOutDir." }

# Payload = continutul publish-ului ca .zip temporar (includeBaseDirectory = False).
$PayloadZip = Join-Path $ArtifactsDir "_payload_$Stamp.zip"
if (Test-Path $PayloadZip) { Remove-Item $PayloadZip -Force }
[System.IO.Compression.ZipFile]::CreateFromDirectory(
    $PublishDir, $PayloadZip,
    [System.IO.Compression.CompressionLevel]::Optimal,
    $false)   # FARA folder-radacina => extragere directa in C:\KBOT

# Asambleaza: stub + payload + footer [Int64 lungime][magic 'KBOTSFX1'].
$SetupExe = Join-Path $ArtifactsDir "KBot_Setup_$Stamp.exe"
if (Test-Path $SetupExe) { Remove-Item $SetupExe -Force }
$payloadBytes = [System.IO.File]::ReadAllBytes($PayloadZip)
$magicBytes   = [System.Text.Encoding]::ASCII.GetBytes('KBOTSFX1')          # 8 octeti
$lenBytes     = [System.BitConverter]::GetBytes([Int64]$payloadBytes.Length) # 8 octeti, little-endian
$out = [System.IO.File]::Create($SetupExe)
try {
    $stubBytes = [System.IO.File]::ReadAllBytes($StubExe)
    $out.Write($stubBytes,   0, $stubBytes.Length)
    $out.Write($payloadBytes,0, $payloadBytes.Length)
    $out.Write($lenBytes,    0, $lenBytes.Length)
    $out.Write($magicBytes,  0, $magicBytes.Length)
} finally {
    $out.Close()
}
Remove-Item $PayloadZip -Force
Remove-Item $StubOutDir -Recurse -Force
$SetupSizeMB = [Math]::Round((Get-Item $SetupExe).Length / 1MB, 1)
Write-Host "Self-extractor -> $SetupExe ($SetupSizeMB MB)" -ForegroundColor Green

# --- 8. Curata staging-ul (ramane .zip-ul + .exe-ul self-extractor) -------------
Remove-Item $PublishDir -Recurse -Force

# --- 9. Raport ------------------------------------------------------------------
$ZipSizeMB = [Math]::Round((Get-Item $ZipPath).Length / 1MB, 1)
Write-Host ""
Write-Host "GATA (DEBUG)." -ForegroundColor Green
Write-Host "  Comportament : porneste in DevHarnessForm (bancul de teste)."
Write-Host "  Installer    : $SetupExe  ($SetupSizeMB MB)"
Write-Host "                 -> dublu-click => se instaleaza SILENTIOS in C:\KBOT (suprascrie tot)."
Write-Host "                 -> optional alt folder: KBot_Setup_$Stamp.exe `"D:\AltCale`""
Write-Host "  Zip (manual) : $ZipPath  ($ZipSizeMB MB)  [fallback de dezarhivare manuala]"
Write-Host "  Necesita     : .NET Desktop Runtime 8 (win-x64) instalat pe PC-ul clientului."
Write-Host "  Workflows    : $($wfls.Count) fisier(e) .wfl incluse deja in 'Workflows\'."
Write-Host "  Browser      : prima rulare pe un PC nou cere '.\playwright.ps1 install chromium' (per-user)."