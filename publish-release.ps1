<#
================================================================================
  publish-release.ps1
  Publishes KBot.App (+ KBot.Migrator under Migrare\) in RELEASE and produces
  a .zip plus an Inno Setup installer (KBot_Setup_<stamp>.exe) with uninstaller.

  RELEASE => Program.vb does NOT enter the #If DEBUG branch:
             - starts directly in MainForm (the real shell);
             - KBot.DevHarness is NOT included (Debug-conditional reference);
             - no test harness, no "Open MainForm" button.

  Publish parameters (same as debug -- these are runtime constraints):
             - framework-dependent (client already has .NET Desktop Runtime 8 win-x64)
             - RID win-x64
             - NO PublishSingleFile (single-file clears Assembly.Location =>
               Playwright cannot find .playwright\ => NRE at GetExecutablePath)

  Build PC requirements: .NET 8 SDK in PATH (dotnet), Inno Setup 6 (ISCC.exe).

  Questions before building (each skippable by parameter):
    -Sign    sign the exes, Setup and uninstaller? (Y)es / (N)o, Enter = Y. "No" =
             no signing step runs, the SimplySign token is never asked.
    -Bump    which part of KBot.App's FileVersion to bump: (M)ajor, m(i)nor, (b)uild,
             (r)evision, (N)one, Enter = N. Minor caps at 9, Build at 99, Revision at 9:
             one past the cap carries into the part above and resets.
             -Bump <part> skips the question.

  Digital signing (optional, never fatal):
             - Uses Certum SimplySign token via certificate thumbprint.
             - SimplySign Desktop must be running and logged in before the build.
             - If no thumbprint is provided or signing fails, the build continues
               with a warning. Unsigned artifacts are still produced.

  Placement: put this script at the solution root (next to KBot.sln) or below it.
================================================================================
#>

[CmdletBinding()]
param(
    [string] $Rid = 'win-x64',

    # Certum SimplySign certificate thumbprint (SHA1, no spaces).
    # If empty, tries env var KBOT_SIGN_THUMBPRINT, then skips signing.
    [string] $SignThumbprint = '2F0E82DB6781F3DC17D43166A3420B7CBE380A68',

    # Timestamp server. Certum's RFC3161 endpoint.
    [string] $TimestampUrl = 'http://time.certum.pl',

    # Which part of KBot.App's FileVersion to bump BEFORE building. 'Ask' (default)
    # prompts on the console: "(M)ajor, m(i)nor, (b)uild, (r)evision, (N)one", Enter = N.
    # Without an interactive console 'Ask' behaves as 'None'.
    [ValidateSet('Ask', 'None', 'Major', 'Minor', 'Build', 'Revision')]
    [string] $Bump = 'Ask',

    # Sign the exes, Setup and the uninstaller? 'Ask' (default) puts one question on
    # the console BEFORE the version one: "(Y)es / (N)o", Enter = Y. 'No' means no
    # SimplySign confirmation is ever requested: every signing step is skipped.
    # Without an interactive console 'Ask' behaves as 'Yes' (the old behaviour).
    [ValidateSet('Ask', 'Yes', 'No')]
    [string] $Sign = 'Ask'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$Configuration = 'Release'

# ==============================================================================
#  VERSION BUMP
#  KBot.App's FileVersion is THE product version: the installer's AppVersion, the
#  update server's "version", the number every installed client compares itself
#  against. It is bumped here, before the build, so the package carries it.
# ==============================================================================

$VersionParts = @('Major', 'Minor', 'Build', 'Revision')
# Highest value a part may hold; one past it resets the part to 0 and carries into
# the part above (1.9.99.9 + Revision -> 2.0.0.0). Major never carries.
$VersionCaps  = @{ Minor = 9; Build = 99; Revision = 9 }

function Step-KBotVersion {
    # $Part + 1; everything below $Part resets to 0; carries per $VersionCaps.
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
    # Rewrites only the <FileVersion> text; keeps the file's encoding (BOM or not).
    param([string]$Project, [version]$NewVersion)
    $bytes = [System.IO.File]::ReadAllBytes($Project)
    $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $text = [System.IO.File]::ReadAllText($Project)
    $rx = [regex]'<FileVersion>\s*[0-9.]+\s*</FileVersion>'
    if (-not $rx.IsMatch($text)) { throw "No <FileVersion> element in $Project." }
    $text = $rx.Replace($text, "<FileVersion>$NewVersion</FileVersion>", 1)
    [System.IO.File]::WriteAllText($Project, $text, (New-Object System.Text.UTF8Encoding($hasBom)))
}

function Read-KBotBumpChoice {
    # One question on the console; Enter = None. Letters as the operator asked for
    # them: M(ajor), i (m-i-nor), b(uild), r(evision), N(one); full words work too.
    param([version]$Current)
    if (-not [Environment]::UserInteractive -or [Console]::IsInputRedirected) {
        Write-Host "Version bump: no interactive console -> None (pass -Bump to choose)." -ForegroundColor Yellow
        return 'None'
    }
    $map = @{ ''  = 'None'; 'N' = 'None'; 'NONE' = 'None'
              'M' = 'Major'; 'MAJOR' = 'Major'
              'I' = 'Minor'; 'MINOR' = 'Minor'
              'B' = 'Build'; 'BUILD' = 'Build'
              'R' = 'Revision'; 'REVISION' = 'Revision' }
    while ($true) {
        $answer = Read-Host "Bump FileVersion $Current ? (M)ajor, m(i)nor, (b)uild, (r)evision, (N)one [N]"
        $key = ($answer -replace '\s', '').ToUpperInvariant()
        if ($map.ContainsKey($key)) { return $map[$key] }
        Write-Host "  Type M, i, b, r or N (Enter = N)." -ForegroundColor Yellow
    }
}

# ==============================================================================
#  SIGNING HELPERS
# ==============================================================================

function Write-SignInfo { param([string]$m) Write-Host "[sign] $m" -ForegroundColor Cyan }
function Write-SignWarn { param([string]$m) Write-Warning "[sign] $m" }
function Write-SignOk   { param([string]$m) Write-Host "[sign] $m" -ForegroundColor Green }

# Set once in step 1a. $false = every signing step (exes, Setup, uninstaller) is
# skipped, so the SimplySign token is never asked for a confirmation.
$script:SigningEnabled = $true

function Read-KBotSignChoice {
    # One question on the console; Enter = Yes.
    if (-not [Environment]::UserInteractive -or [Console]::IsInputRedirected) {
        Write-Host "Signing: no interactive console -> Yes (pass -Sign No to skip)." -ForegroundColor Yellow
        return $true
    }
    while ($true) {
        $answer = Read-Host "Sign the app, Setup and uninstaller (SimplySign confirmations)? (Y)es / (N)o [Y]"
        $key = ($answer -replace '\s', '').ToUpperInvariant()
        if ($key -in @('', 'Y', 'YES')) { return $true }
        if ($key -in @('N', 'NO')) { return $false }
        Write-Host "  Type Y or N (Enter = Y)." -ForegroundColor Yellow
    }
}

function Resolve-SignThumbprint {
    # Priority: signing switched off (step 1a) > explicit parameter > environment variable > none (skip).
    if (-not $script:SigningEnabled) { return $null }
    if ($SignThumbprint) { return ($SignThumbprint -replace '\s', '').ToUpperInvariant() }
    if ($env:KBOT_SIGN_THUMBPRINT) { return ($env:KBOT_SIGN_THUMBPRINT -replace '\s', '').ToUpperInvariant() }
    return $null
}

function Resolve-SignTool {
    # Locate signtool.exe from PATH or the Windows SDK.
    $cmd = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $roots = @(
        "${env:ProgramFiles(x86)}\Windows Kits\10\bin",
        "${env:ProgramFiles}\Windows Kits\10\bin"
    ) | Where-Object { $_ -and (Test-Path $_) }

    foreach ($root in $roots) {
        $candidate = Get-ChildItem -Path $root -Recurse -Filter 'signtool.exe' -File -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match '\\x64\\' } |
            Sort-Object FullName -Descending |
            Select-Object -First 1
        if ($candidate) { return $candidate.FullName }
    }
    return $null
}

function Invoke-KBotSign {
    <#
      Sign files with the Certum SimplySign token.
      -SignFile  : single file path (optional)
      -SignDir   : directory whose KBot.*.exe/dll will be signed (optional)
      Never throws. Missing cert / missing signtool => warning, then return.
    #>
    [CmdletBinding()]
    param(
        [string] $SignFile,
        [string] $SignDir,
        [string[]] $Include = @('KBot.*.exe', 'KBot.*.dll')
    )

    # 1. Resolve thumbprint. Signing off / no thumbprint => skip cleanly.
    if (-not $script:SigningEnabled) {
        Write-SignInfo "Signing switched off for this build. Skipping."
        return
    }
    $tp = Resolve-SignThumbprint
    if (-not $tp) {
        Write-SignWarn "No signing thumbprint configured (KBOT_SIGN_THUMBPRINT). Skipping signing."
        return
    }

    # 2. Resolve signtool. Missing => skip cleanly.
    $signtool = Resolve-SignTool
    if (-not $signtool) {
        Write-SignWarn "signtool.exe not found (Windows SDK missing?). Skipping signing."
        return
    }

    # 3. Build the target list. @() forces an array even for a single element,
    #    which is required under Set-StrictMode -Version Latest.
    $targets = @()
    if ($SignFile) {
        if (-not (Test-Path -LiteralPath $SignFile)) {
            Write-SignWarn "SignFile not found: $SignFile. Skipping."
            return
        }
        $targets += (Resolve-Path -LiteralPath $SignFile).Path
    }
    if ($SignDir) {
        if (-not (Test-Path -LiteralPath $SignDir -PathType Container)) {
            Write-SignWarn "SignDir not found: $SignDir. Skipping."
            return
        }
        foreach ($pattern in $Include) {
            $found = @(Get-ChildItem -LiteralPath $SignDir -File -Filter $pattern -ErrorAction SilentlyContinue |
                ForEach-Object { $_.FullName })
            if ($found.Count -gt 0) {
                $targets += $found
            }
        }
    }
    # Belt and braces: re-wrap and dedupe.
    $targets = @($targets | Sort-Object -Unique)

    if ($targets.Count -eq 0) {
        Write-SignInfo "Nothing to sign (no matching files)."
        return
    }

    # 4. Verify SimplySign is exposing the certificate.
    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
        Where-Object { $_.Thumbprint -eq $tp } |
        Select-Object -First 1

    if (-not $cert) {
        Write-SignWarn "Certificate with thumbprint $tp not found in CurrentUser\My."
        Write-SignWarn "Ensure SimplySign Desktop is running and logged in, then retry."
        return
    }

    Write-SignInfo "signtool  : $signtool"
    Write-SignInfo "thumbprint: $tp"
    Write-SignInfo "subject   : $($cert.Subject)"

    # 5. Sign each target. Failures are warnings only.
    $okCount   = 0
    $failCount = 0

    foreach ($t in $targets) {
        Write-SignInfo "Signing: $t"
        try {
            & $signtool sign /fd SHA256 /tr $TimestampUrl /td SHA256 /sha1 $tp /v $t 2>&1 |
                ForEach-Object { Write-Verbose $_ }

            if ($LASTEXITCODE -ne 0) {
                Write-SignWarn "signtool returned $LASTEXITCODE for $t. Continuing."
                $failCount++
            } else {
                $okCount++
            }
        } catch {
            Write-SignWarn "Signing failed for ${t}: $($_.Exception.Message). Continuing."
            $failCount++
        }
    }

    if ($failCount -eq 0) {
        Write-SignOk "Signed $okCount file(s)."
    } else {
        Write-SignWarn "Signed $okCount file(s), $failCount failure(s). Build continues."
    }
}

function Get-KBotInnoSignCommand {
    <#
      Sign command handed to Inno Setup (ISCC /S<name>=<cmd>) so it signs Setup.exe
      and the uninstaller itself. Returns $null when signing is not possible, so the
      compile runs unsigned instead of failing -- same never-fatal policy as
      Invoke-KBotSign. $q / $f are Inno placeholders (quote / quoted file name).
    #>
    $tp = Resolve-SignThumbprint
    if (-not $tp) { return $null }
    $signtool = Resolve-SignTool
    if (-not $signtool) { return $null }
    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
        Where-Object { $_.Thumbprint -eq $tp } | Select-Object -First 1
    if (-not $cert) { return $null }
    return ('$q{0}$q sign /fd SHA256 /tr {1} /td SHA256 /sha1 {2} $f' -f $signtool, $TimestampUrl, $tp)
}

function Resolve-InnoCompiler {
    # ISCC.exe from PATH or the default Inno Setup 6 install folder.
    $cmd = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
    )
    foreach ($c in $candidates) {
        if ($c -and (Test-Path -LiteralPath $c)) { return $c }
    }
    return $null
}

# ==============================================================================
#  SOLUTION ROOT DISCOVERY
# ==============================================================================

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

# ==============================================================================
#  MANIFEST GENERATOR
# ==============================================================================

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

    # @() is required: Get-ChildItem can return 0, 1, or N items, and under
    # StrictMode .Count only exists on real arrays.
    $files = @(Get-ChildItem -LiteralPath $PublishDir -File |
        Where-Object { $_.Name -like 'KBot.*.dll' -or $_.Name -like 'KBot.*.exe' } |
        Sort-Object Name)

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
            throw "Missing FileVersion on $($f.Name) -- set <FileVersion> in its .vbproj."
        }
        $hash = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash
        [void]$sb.AppendLine(
            ('  <Component file="{0}" version="{1}" size="{2}" sha256="{3}" />' -f `
                $f.Name, $ver, $f.Length, $hash))
    }

    [void]$sb.AppendLine('</KBotManifest>')

    $manifestPath = Join-Path $PublishDir 'manifest.xml'
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText($manifestPath, $sb.ToString(), $utf8NoBom)

    Write-Host "manifest.xml written: $manifestPath ($($files.Count) components)"
    return $manifestPath
}

# ==============================================================================
#  MAIN
# ==============================================================================

$startDir = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }
$SolutionRoot = Find-SolutionRoot -Start $startDir
if (-not $SolutionRoot) {
    throw "Cannot find KBot.sln starting from '$startDir' (searched 4 levels up). Put the script at/below the solution root."
}

$ProjectFile = Join-Path $SolutionRoot 'src\KBot.App\KBot.App.vbproj'
if (-not (Test-Path $ProjectFile)) {
    throw "Project not found: $ProjectFile"
}

# --- 1. Check dotnet SDK -------------------------------------------------------
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet SDK is not in PATH on this (build) PC. Install .NET 8 SDK."
}

# --- 1a. Sign or not (asked on the console unless -Sign says) -------------------
#  Decided first, before anything is built, so a "No" never reaches the SimplySign
#  token: Invoke-KBotSign and the Inno sign command both read $script:SigningEnabled.
$script:SigningEnabled = switch ($Sign) {
    'Yes' { $true }
    'No'  { $false }
    default { Read-KBotSignChoice }
}
if ($script:SigningEnabled) {
    Write-Host "Signing: ON (exes, Setup, uninstaller)." -ForegroundColor Cyan
} else {
    Write-Host "Signing: OFF -- no SimplySign confirmation will be requested; artifacts stay unsigned." -ForegroundColor Yellow
}

# --- 1b. Version bump (asked on the console unless -Bump says) ------------------
#  Writes the new number into KBot.App.vbproj BEFORE dotnet publish, so the exe, the
#  installer and the update package all carry it. The edit is NOT committed here.
$CurrentVersion = Get-KBotFileVersion -Project $ProjectFile
$bumpPart = if ($Bump -eq 'Ask') { Read-KBotBumpChoice -Current $CurrentVersion } else { $Bump }
if ($bumpPart -ne 'None') {
    $NewVersion = Step-KBotVersion -Version $CurrentVersion -Part $bumpPart
    Set-KBotFileVersion -Project $ProjectFile -NewVersion $NewVersion
    Write-Host "FileVersion $CurrentVersion -> $NewVersion ($bumpPart) written to KBot.App.vbproj (not committed)." -ForegroundColor Green
} else {
    Write-Host "FileVersion stays $CurrentVersion." -ForegroundColor Cyan
}

# --- 2. Names / paths ----------------------------------------------------------
$Stamp         = Get-Date -Format 'yyyyMMdd_HHmmss'
$AppFolderName = "KBot_${Configuration}_$Stamp"
$ArtifactsDir  = Join-Path $SolutionRoot 'artifacts'
$PublishDir    = Join-Path $ArtifactsDir $AppFolderName
$ZipPath       = Join-Path $ArtifactsDir "$AppFolderName.zip"

# --- 3. Clean previous staging -------------------------------------------------
if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }
if (Test-Path $ZipPath)    { Remove-Item $ZipPath -Force }
New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null

# --- 4. Publish (framework-dependent) ------------------------------------------
Write-Host "Publish $Configuration ($Rid, framework-dependent)..." -ForegroundColor Cyan
& dotnet publish $ProjectFile `
    -c $Configuration `
    -r $Rid `
    --self-contained false `
    -p:PublishSingleFile=false `
    -o $PublishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed (ExitCode=$LASTEXITCODE)."
}

# --- 4b. RELEASE guard: DevHarness must NOT appear in output --------------------
$harnessLeak = @(Get-ChildItem -LiteralPath $PublishDir -File -Filter 'KBot.DevHarness.*' -ErrorAction SilentlyContinue)
if ($harnessLeak.Count -gt 0) {
    throw ("Release publish contains KBot.DevHarness: {0}. " +
           "Check <ProjectReference ... Condition=`"'`$(Configuration)'=='Debug'`"> in KBot.App.vbproj.") -f `
          (($harnessLeak | ForEach-Object Name) -join ', ')
}
Write-Host "Guard OK: no KBot.DevHarness.* in output." -ForegroundColor Cyan

# --- 4c. Migrare -- KBot.Migrator (Access -> MariaDB utility) under 'Migrare\' --
#  Separate EXE (not referenced by KBot.App), so it gets its own publish. It lives in
#  a subfolder so its own .deps.json / .runtimeconfig.json do not mix with the app's.
#  Same parameters: framework-dependent, no single-file.
$MigratorProj = Join-Path $SolutionRoot 'src\KBot.Migrator\KBot.Migrator.vbproj'
if (-not (Test-Path $MigratorProj)) { throw "Project not found: $MigratorProj" }
$MigrareDir = Join-Path $PublishDir 'Migrare'
Write-Host "Publish KBot.Migrator -> Migrare\ ..." -ForegroundColor Cyan
& dotnet publish $MigratorProj `
    -c $Configuration `
    -r $Rid `
    --self-contained false `
    -p:PublishSingleFile=false `
    -o $MigrareDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish KBot.Migrator failed (ExitCode=$LASTEXITCODE)."
}
if (-not (Test-Path -LiteralPath (Join-Path $MigrareDir 'KBot.Migrator.exe'))) {
    throw "KBot.Migrator.exe missing from $MigrareDir after publish."
}
Write-Host "Migrare OK: KBot.Migrator.exe in $MigrareDir" -ForegroundColor Cyan

# --- 4c2. KBot.Updater (slice 0067) -> single KBot.Updater.exe next to the app ----
#  The in-place update helper. Published SINGLE-FILE (framework-dependent) so the app
#  has ONE file to copy to %TEMP% before it exits; it references no KBot.* assembly and
#  no Playwright, so single-file is safe here (the app itself cannot be single-file).
#  Its own .deps/.runtimeconfig live inside the bundle -- nothing else lands in the
#  app folder. push-update.ps1 refuses a package without it.
$UpdaterProj = Join-Path $SolutionRoot 'src\KBot.Updater\KBot.Updater.vbproj'
if (-not (Test-Path $UpdaterProj)) { throw "Project not found: $UpdaterProj" }
$UpdaterStage = Join-Path $ArtifactsDir "_updater_$Stamp"
if (Test-Path $UpdaterStage) { Remove-Item $UpdaterStage -Recurse -Force }
Write-Host "Publish KBot.Updater (single-file) ..." -ForegroundColor Cyan
& dotnet publish $UpdaterProj `
    -c $Configuration `
    -r $Rid `
    --self-contained false `
    -p:PublishSingleFile=true `
    -o $UpdaterStage
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish KBot.Updater failed (ExitCode=$LASTEXITCODE)."
}
$UpdaterExe = Join-Path $UpdaterStage 'KBot.Updater.exe'
if (-not (Test-Path -LiteralPath $UpdaterExe)) { throw "KBot.Updater.exe missing from $UpdaterStage after publish." }
Copy-Item -LiteralPath $UpdaterExe -Destination (Join-Path $PublishDir 'KBot.Updater.exe') -Force
Remove-Item $UpdaterStage -Recurse -Force
Write-Host "Updater OK: KBot.Updater.exe in $PublishDir" -ForegroundColor Cyan

# --- 4d. Sign the three EXEs only (optional, non-fatal) ------------------------
#  Windows checks Authenticode on what the operator launches, never on DLLs a
#  desktop app loads, so the KBot.*.dll files stay unsigned on purpose: each
#  signature is one SimplySign confirmation. Setup.exe and the uninstaller are
#  signed by Inno Setup itself (step 7b). Total: 5 confirmations per build.
#  The updater relaunches itself through UAC on machines where C:\KBOT is not
#  writable; unsigned, that prompt would be the yellow one.
Invoke-KBotSign -SignFile (Join-Path $PublishDir 'KBot.App.exe')
Invoke-KBotSign -SignFile (Join-Path $MigrareDir 'KBot.Migrator.exe')
Invoke-KBotSign -SignFile (Join-Path $PublishDir 'KBot.Updater.exe')

# --- 5. Workflows folder -------------------------------------------------------
$WorkflowsDir    = Join-Path $PublishDir 'Workflows'
$WorkflowsSource = Join-Path $SolutionRoot 'src\KBot.Forexe\Workflows'
New-Item -ItemType Directory -Path $WorkflowsDir -Force | Out-Null

if (-not (Test-Path -LiteralPath $WorkflowsSource -PathType Container)) {
    throw "Workflows source folder missing: $WorkflowsSource"
}
$wfls = @(Get-ChildItem -LiteralPath $WorkflowsSource -Filter '*.wfl' -File)
if ($wfls.Count -eq 0) {
    throw "No .wfl files in $WorkflowsSource (at least 'adlop - Conectare.wfl' required)."
}
foreach ($w in $wfls) {
    Copy-Item -LiteralPath $w.FullName -Destination $WorkflowsDir -Force
}
if (-not (Test-Path -LiteralPath (Join-Path $WorkflowsDir 'adlop - Conectare.wfl'))) {
    throw "'adlop - Conectare.wfl' missing after copy -- connection will not work."
}
Write-Host "Workflows copied: $($wfls.Count) .wfl file(s) -> $WorkflowsDir" -ForegroundColor Cyan

# --- 6. Logs folder (empty) ----------------------------------------------------
$LogsDir = Join-Path $PublishDir 'Logs'
New-Item -ItemType Directory -Path $LogsDir -Force | Out-Null
'Log folder (created at publish).' | Set-Content -Path (Join-Path $LogsDir '_keep.txt') -Encoding UTF8

# --- 6b. Manifest --------------------------------------------------------------
New-KBotManifest -PublishDir $PublishDir -Configuration $Configuration -Runtime $Rid | Out-Null

# --- 7. Archive -> single .zip -------------------------------------------------
if (-not ('System.IO.Compression.ZipFile' -as [type])) {
    Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
}
if (-not ('System.IO.Compression.ZipArchive' -as [type])) {
    Add-Type -AssemblyName 'System.IO.Compression'
}
Write-Host "Archiving -> $ZipPath" -ForegroundColor Cyan
#  Written entry by entry, NOT with ZipFile::CreateFromDirectory: under Windows
#  PowerShell 5.1 (.NET Framework) that writes the entry names with BACKSLASHES
#  ("KBot_Release_x\KBot.App.exe"), which is not a folder separator for zip readers.
#  KBot.Updater strips the top folder by "/" and push-update.ps1 looks for the exes by
#  "/" -- with "\" the update would land in C:\KBOT\KBot_Release_<stamp>\ (seen
#  20.09.2026). Every name below is built with "/" by hand.
$zipStream = [System.IO.File]::Open($ZipPath, [System.IO.FileMode]::CreateNew)
try {
    $archive = New-Object System.IO.Compression.ZipArchive($zipStream, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        # -Name yields paths RELATIVE to $PublishDir (no prefix arithmetic, no
        # short-vs-long path surprises).
        foreach ($rel in Get-ChildItem -LiteralPath $PublishDir -Recurse -File -Name) {
            $file = Get-Item -LiteralPath (Join-Path $PublishDir $rel)
            $entryName = "$AppFolderName/" + $rel.Replace('\', '/')
            $entry = $archive.CreateEntry($entryName, [System.IO.Compression.CompressionLevel]::Optimal)
            $entry.LastWriteTime = $file.LastWriteTime
            $out = $entry.Open()
            try {
                $in = [System.IO.File]::OpenRead($file.FullName)
                try { $in.CopyTo($out) } finally { $in.Dispose() }
            } finally { $out.Dispose() }
        }
    } finally { $archive.Dispose() }
} finally { $zipStream.Dispose() }

# Guard: no entry may carry a backslash, and the two exes must sit right under the top folder.
$check = [System.IO.Compression.ZipFile]::OpenRead($ZipPath)
try {
    $bad = @($check.Entries | Where-Object { $_.FullName.Contains('\') })
    if ($bad.Count -gt 0) { throw "Zip entries with backslashes: $($bad[0].FullName) (+$($bad.Count - 1))." }
    foreach ($must in @("$AppFolderName/KBot.App.exe", "$AppFolderName/KBot.Updater.exe")) {
        if (-not ($check.Entries | Where-Object { $_.FullName -eq $must })) { throw "Zip is missing $must." }
    }
    Write-Host "Zip OK: $($check.Entries.Count) entries under '$AppFolderName/'." -ForegroundColor Cyan
} finally { $check.Dispose() }

# --- 7b. Installer (Inno Setup) -> KBot_Setup_<stamp>.exe -----------------------
#  tools\KBotInstaller\KBot.iss: publisher, logo, Romanian wizard text, components
#  (app + Migrare), Start menu / desktop shortcuts, .NET Desktop Runtime 8 check,
#  optional Chromium download, and a registered uninstaller. When a signing cert is
#  available, Inno signs Setup.exe and the uninstaller itself (2 confirmations).
$IssFile = Join-Path $SolutionRoot 'tools\KBotInstaller\KBot.iss'
if (-not (Test-Path -LiteralPath $IssFile)) { throw "Installer script missing: $IssFile" }
$Iscc = Resolve-InnoCompiler
if (-not $Iscc) { throw "ISCC.exe not found. Install Inno Setup 6 (https://jrsoftware.org/isdl.php) on this (build) PC." }

$AppExe = Join-Path $PublishDir 'KBot.App.exe'
$AppVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($AppExe).FileVersion
if ([string]::IsNullOrWhiteSpace($AppVersion)) { throw "Missing FileVersion on KBot.App.exe." }

$SetupBaseName = "KBot_Setup_$Stamp"
$SetupExe      = Join-Path $ArtifactsDir "$SetupBaseName.exe"
if (Test-Path -LiteralPath $SetupExe) { Remove-Item -LiteralPath $SetupExe -Force }

$isccArgs = @(
    "/DSourceDir=$PublishDir",
    "/DAppVersion=$AppVersion",
    "/DOutputDir=$ArtifactsDir",
    "/DOutputBaseFilename=$SetupBaseName",
    '/Qp'
)
$innoSign = Get-KBotInnoSignCommand
if ($innoSign) {
    $isccArgs += '/DSIGN=1'
    $isccArgs += "/Skbotsign=$innoSign"
    Write-SignInfo "Inno Setup will sign Setup.exe + uninstaller."
} elseif (-not $script:SigningEnabled) {
    Write-SignInfo "Signing switched off -- installer and uninstaller stay UNSIGNED."
} else {
    Write-SignWarn "No usable signing setup -- installer and uninstaller will be UNSIGNED."
}
$isccArgs += $IssFile

Write-Host "Compiling installer (Inno Setup) -> $SetupExe" -ForegroundColor Cyan
& $Iscc @isccArgs
if ($LASTEXITCODE -ne 0) { throw "ISCC failed (ExitCode=$LASTEXITCODE)." }
if (-not (Test-Path -LiteralPath $SetupExe)) { throw "ISCC reported success but $SetupExe is missing." }
$SetupSizeMB = [Math]::Round((Get-Item -LiteralPath $SetupExe).Length / 1MB, 1)
Write-Host "Installer -> $SetupExe ($SetupSizeMB MB)" -ForegroundColor Green

# --- 8. Clean staging ----------------------------------------------------------
Remove-Item $PublishDir -Recurse -Force

# --- 9. Report -----------------------------------------------------------------
$ZipSizeMB = [Math]::Round((Get-Item $ZipPath).Length / 1MB, 1)
Write-Host ""
Write-Host "DONE (RELEASE)." -ForegroundColor Green
Write-Host "  Behavior : starts DIRECTLY in MainForm (real shell, no DevHarness)."
Write-Host "  Installer: $SetupExe  ($SetupSizeMB MB)"
Write-Host "             -> wizard (Inno Setup): default C:\KBOT, registers an uninstaller."
Write-Host "             -> silent: KBot_Setup_$Stamp.exe /VERYSILENT /NORESTART [/DIR=`"D:\AltPath`"]"
Write-Host "  Zip (manual): $ZipPath  ($ZipSizeMB MB)  [fallback manual extraction]"
Write-Host "  Requires : .NET Desktop Runtime 8 (win-x64) on the client PC."
Write-Host "  Workflows: $($wfls.Count) .wfl file(s) included under 'Workflows\'."
Write-Host "  Migrare  : KBot.Migrator.exe (Access -> MariaDB) included under 'Migrare\'."
Write-Host "  Updater  : KBot.Updater.exe next to the app (slice 0067). Publish it: .\push-update.ps1 [-Mandatory] [-Notes '...']"
Write-Host "  Browser  : installer offers the Chromium download as a task (else '.\playwright.ps1 install chromium')."

# --- 10. Signing status summary ------------------------------------------------
$tp = Resolve-SignThumbprint
if (-not $script:SigningEnabled) {
    Write-Host "  Signing  : OFF for this build (operator's choice) -- ALL artifacts are UNSIGNED." -ForegroundColor Yellow
} elseif ($tp) {
    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
        Where-Object { $_.Thumbprint -eq $tp } | Select-Object -First 1
    if ($cert) {
        Write-Host "  Signing  : thumbprint $tp  (expires $($cert.NotAfter.ToString('yyyy-MM-dd')))" -ForegroundColor Green
    } else {
        Write-Host "  Signing  : thumbprint $tp was set, but cert NOT found -- artifacts are UNSIGNED." -ForegroundColor Yellow
    }
} else {
    Write-Host "  Signing  : disabled (no thumbprint provided). Artifacts are UNSIGNED." -ForegroundColor Yellow
}
