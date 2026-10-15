param([int]$Port = 5053, [string]$Actor = 'localhost-ssh@local.test')
# Restarts VERCON on localhost: stops the server that holds the port (started by Start-Localhost.ps1 or by hand with
# preview.py / local_ssh.py), then starts it again in the background. Needed after any change to Python code.
$ErrorActionPreference = 'Stop'
$taskLauncher = Join-Path $PSScriptRoot 'local_ssh.py'
$taskPreview = Join-Path $PSScriptRoot 'preview.py'

# 1. The normal case: a start saved by Start-Localhost.ps1.
& (Join-Path $PSScriptRoot 'Stop-Localhost.ps1') -Port $Port

# 2. Whatever still holds the port: only a VERCON launcher of this folder is stopped, never a foreign process.
$taskListener = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
if ($taskListener) {
    $taskOwner = Get-CimInstance Win32_Process -Filter "ProcessId = $($taskListener.OwningProcess)" -ErrorAction SilentlyContinue
    $taskOwnerLine = [string]$taskOwner.CommandLine
    if (-not $taskOwner -or -not ($taskOwnerLine.Contains($taskLauncher) -or $taskOwnerLine.Contains($taskPreview))) {
        throw "Portul $Port este ocupat de alt program. Nu il opresc."
    }
    function Stop-Tree([int]$taskParentId) {
        foreach ($taskChild in @(Get-CimInstance Win32_Process -Filter "ParentProcessId = $taskParentId")) {
            if ($taskChild.Name -notin @('python.exe', 'pythonw.exe', 'ssh.exe')) { continue }
            Stop-Tree -taskParentId $taskChild.ProcessId
            Stop-Process -Id $taskChild.ProcessId -Force -ErrorAction SilentlyContinue
        }
    }
    # The Windows venv launcher can sit above the process that owns the port: stop that parent too.
    $taskParent = Get-CimInstance Win32_Process -Filter "ProcessId = $($taskOwner.ParentProcessId)" -ErrorAction SilentlyContinue
    $taskParentLine = [string]$taskParent.CommandLine
    $taskTop = if ($taskParent -and $taskParent.Name -eq 'python.exe' -and ($taskParentLine.Contains($taskLauncher) -or $taskParentLine.Contains($taskPreview))) { $taskParent } else { $taskOwner }
    Stop-Tree -taskParentId $taskTop.ProcessId
    Stop-Process -Id $taskTop.ProcessId -Force -ErrorAction SilentlyContinue
    for ($taskAttempt = 0; $taskAttempt -lt 20 -and (Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue); $taskAttempt++) { Start-Sleep -Milliseconds 250 }
    Write-Host "Procesul de pe portul $Port a fost oprit."
}

# 3. Start again.
& (Join-Path $PSScriptRoot 'Start-Localhost.ps1') -Port $Port -Actor $Actor
