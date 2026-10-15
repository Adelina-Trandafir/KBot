<#
.SYNOPSIS
  Sends ONLY the changed PYTHON\ files to the server (one SSH connection, one tar), then restarts
  the service when a .py file was among them.

.DESCRIPTION
  What "changed" means: the file differs from what THIS script sent last time (SHA256 kept in
  VERCON	ools.localpush-state.json). Git is not used at all. The very first run has no memory,
  so it would send every file of PYTHON: use -Baseline once to just record the current state
  (when the server is already up to date), then every run sends only what you edited since.

  Server, port, user, key and remote root are read from _Admin\admin_settings.json (the same
  settings KBotAdmin / AvacontPush use): PYTHON\ -> RemoteRoot (/root/AVACONT).

  Run from a command prompt:
    powershell -ExecutionPolicy Bypass -File tools\Push-Changed.ps1
    powershell -ExecutionPolicy Bypass -File tools\Push-Changed.ps1 -DryRun

.PARAMETER DryRun     Only lists what would be sent.
.PARAMETER Baseline  Records the current files as already on the server, sends nothing.
.PARAMETER Restart    Restart the service even when no .py file changed.
.PARAMETER NoRestart  Never restart the service.
.PARAMETER Forget     Delete the remembered state (the next run sends everything it considers).
#>
param(
    [switch]$DryRun,
    [switch]$Baseline,
    [switch]$Restart,
    [switch]$NoRestart,
    [switch]$Forget,
    [string]$Service = 'avacont'
)

$ErrorActionPreference = 'Stop'
$root      = Split-Path $PSScriptRoot -Parent
$pythonDir = Join-Path $root 'PYTHON'
$settingsFile = Join-Path $root '_Admin\admin_settings.json'
$stateDir  = Join-Path $root 'VERCON\tools\.local'
$stateFile = Join-Path $stateDir 'push-state.json'

if ($Forget) { Remove-Item $stateFile -ErrorAction SilentlyContinue; Write-Host 'Starea a fost stearsa.'; return }
if (-not (Test-Path $settingsFile)) { throw "Lipseste $settingsFile (setarile serverului)." }
foreach ($tool in 'ssh', 'tar') { if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { throw "Nu gasesc '$tool' in PATH." } }

$cfg = Get-Content $settingsFile -Raw | ConvertFrom-Json
$remoteRoot = ([string]$cfg.RemoteRoot).TrimEnd('/')
if (-not $cfg.Host -or -not $remoteRoot) { throw 'Setarile serverului sunt incomplete (Host / RemoteRoot).' }
$extensions = @($cfg.IncludeExtensions | ForEach-Object { '.' + $_.ToLowerInvariant() })
$ignore = @($cfg.IgnorePatterns) + @('.pytest_cache')

# ==============================================================================================================================
# RULE 00 -- config.py IS NEVER TOUCHED. Any possible change of config.py (or of another file that holds the server's secrets) ON THE
# SERVER IS STRICTLY FORBIDDEN. The config.py of this PC is a MOCK; the real one lives only on the server. This is not a setting and
# no switch lifts it. It is enforced three times: (1) such a file is never a candidate, (2) the list is checked again right before the
# archive is made and the script STOPS if one slipped in, (3) the unpacking on the server is told to skip it.
# ==============================================================================================================================
$protectedNames = @('config.py', '*.env', 'efactura.env', 'admin_settings.json', '*secret*', '*.pem', '*.key')
function Test-Protected([string]$rel) {
    $name = [IO.Path]::GetFileName($rel)
    foreach ($pattern in $protectedNames) { if ($name -like $pattern) { return $true } }
    return $false
}

function Test-Wanted([string]$rel) {
    if (Test-Protected $rel) { return $false }   # RULE 00
    if ($extensions -notcontains ([IO.Path]::GetExtension($rel).ToLowerInvariant())) { return $false }
    foreach ($segment in $rel.Split('/')) { foreach ($pattern in $ignore) { if ($segment -like $pattern) { return $false } } }
    return $true
}

# --- candidates: every wanted file under PYTHON (ignored folders such as .venv are not even entered) -------------
$candidates = New-Object System.Collections.Generic.List[string]
function Walk([string]$dir, [string]$prefix) {
    foreach ($item in Get-ChildItem -LiteralPath $dir -Force) {
        $rel = $prefix + $item.Name
        $skip = $false
        foreach ($pattern in $ignore) { if ($item.Name -like $pattern) { $skip = $true; break } }
        if ($skip) { continue }
        if ($item.PSIsContainer) { Walk $item.FullName ($rel + '/') }
        elseif (Test-Wanted $rel) { $candidates.Add($rel) }
    }
}
Walk $pythonDir ''

# --- what differs from the last push -------------------------------------------------------------------------
$state = @{}
if (Test-Path $stateFile) {
    $saved = Get-Content $stateFile -Raw | ConvertFrom-Json
    $mine = $saved.PSObject.Properties[$remoteRoot]
    if ($mine) { $mine.Value.PSObject.Properties | ForEach-Object { $state[$_.Name] = [string]$_.Value } }
}
$changed = @(); $hashes = @{}
foreach ($rel in ($candidates | Sort-Object -Unique)) {
    $hash = (Get-FileHash (Join-Path $pythonDir $rel) -Algorithm SHA256).Hash
    $hashes[$rel] = $hash
    if ($state[$rel] -ne $hash) { $changed += $rel }
}

if ($Baseline) {
    $all0 = @{}
    if (Test-Path $stateFile) { (Get-Content $stateFile -Raw | ConvertFrom-Json).PSObject.Properties | ForEach-Object { $all0[$_.Name] = $_.Value } }
    $all0[$remoteRoot] = $hashes
    New-Item -ItemType Directory -Force -Path $stateDir | Out-Null
    $all0 | ConvertTo-Json -Depth 5 | Set-Content $stateFile -Encoding UTF8
    Write-Host ("Stare inregistrata pentru {0} fisiere (nimic trimis)." -f $hashes.Count); return
}
if (-not $changed.Count) { Write-Host 'Nimic de trimis: nicio schimbare fata de ultima trimitere.'; return }
Write-Host ("De trimis la {0}@{1}:{2} ({3} fisiere):" -f $cfg.User, $cfg.Host, $remoteRoot, $changed.Count)
$changed | ForEach-Object { Write-Host "  $_" }
if ($DryRun) { Write-Host '(DryRun: nu s-a trimis nimic)'; return }

# RULE 00, second check: nothing protected may be in the list that is about to be sent
$blocked = @($changed | Where-Object { Test-Protected $_ })
if ($blocked.Count) { throw ("REGULA 00: fisier protejat in lista de trimis ({0}). Nu s-a trimis nimic." -f ($blocked -join ', ')) }

# --- one tar, one ssh ----------------------------------------------------------------------------------------
$tmp = Join-Path ([IO.Path]::GetTempPath()) ('kbot_push_' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tmp | Out-Null
try {
    $ErrorActionPreference = 'Continue'   # ssh/scp write warnings to stderr; success is judged by the exit codes below
    $list = Join-Path $tmp 'files.txt'; $tar = Join-Path $tmp 'push.tar'
    [IO.File]::WriteAllText($list, (($changed -join "`n") + "`n"), (New-Object Text.UTF8Encoding($false)))
    & tar --format=ustar -cf $tar -C $pythonDir -T $list
    if ($LASTEXITCODE -ne 0) { throw 'tar (local) a esuat.' }

    $sshOptions = @('-p', [string]$cfg.Port, '-o', 'ConnectTimeout=20', '-o', 'ServerAliveInterval=15')
    $sshKey = @(); if ($cfg.PrivateKeyPath) { $sshKey = @('-i', [string]$cfg.PrivateKeyPath); $sshOptions += $sshKey }
    $target = '{0}@{1}' -f $cfg.User, $cfg.Host

    # the archive goes up with scp (a binary file, no pipe: PowerShell 5.1 pipes would corrupt it), then one ssh unpacks it
    $remoteTar = "/tmp/kbot_push_$PID.tar"
    $watch = [Diagnostics.Stopwatch]::StartNew()
    Push-Location $tmp
    try { & scp -P ([string]$cfg.Port) -o ConnectTimeout=20 @($sshKey) push.tar ("{0}:{1}" -f $target, $remoteTar) } finally { Pop-Location }
    if ($LASTEXITCODE -ne 0) { throw "scp a esuat (cod $LASTEXITCODE). Nu s-a retinut nimic." }
    & ssh @sshOptions $target "tar -xf $remoteTar -C $remoteRoot --no-same-owner --exclude=config.py --exclude='*/config.py' && rm -f $remoteTar"
    if ($LASTEXITCODE -ne 0) { throw "dezarhivarea pe server a esuat (cod $LASTEXITCODE). Nu s-a retinut nimic." }
    Write-Host ("Trimis in {0:N1} s." -f $watch.Elapsed.TotalSeconds)

    # remember only after a successful send
    foreach ($rel in $changed) { $state[$rel] = $hashes[$rel] }
    New-Item -ItemType Directory -Force -Path $stateDir | Out-Null
    $all = @{}
    if (Test-Path $stateFile) { (Get-Content $stateFile -Raw | ConvertFrom-Json).PSObject.Properties | ForEach-Object { $all[$_.Name] = $_.Value } }
    $all[$remoteRoot] = $state
    $all | ConvertTo-Json -Depth 5 | Set-Content $stateFile -Encoding UTF8

    # python code needs the service restarted; static files are read from disk on every request
    $needRestart = (-not $NoRestart) -and ($Restart -or ($changed | Where-Object { $_ -like '*.py' }))
    if ($needRestart) {
        Write-Host "Repornesc serviciul '$Service'..."
        & ssh @sshOptions $target "systemctl restart $Service && sleep 2 && systemctl is-active $Service"
        if ($LASTEXITCODE -ne 0) { throw "Repornirea serviciului '$Service' a esuat (cod $LASTEXITCODE)." }
    } else {
        Write-Host 'Fara repornire (doar fisiere statice, sau -NoRestart).'
    }
    Write-Host 'Gata.'
} finally {
    Remove-Item $tmp -Recurse -Force -ErrorAction SilentlyContinue
}
