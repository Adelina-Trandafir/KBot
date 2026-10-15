param([int]$Port = 5053)
$ErrorActionPreference = 'Stop'
$taskLauncher = Join-Path $PSScriptRoot 'local_ssh.py'
$taskPidFile = Join-Path $PSScriptRoot ".local\localhost-$Port.pid"
if (-not (Test-Path -LiteralPath $taskPidFile)) { Write-Host 'Nu exista o pornire salvata pentru acest port.'; return }
$taskProcessId = [int](Get-Content -LiteralPath $taskPidFile -Raw)
$taskProcess = Get-CimInstance Win32_Process -Filter "ProcessId = $taskProcessId" -ErrorAction SilentlyContinue
# A saved PID whose process is not this launcher (gone, reused by another program, or unreadable) is stale: forget it, never kill it.
if ($taskProcess -and -not ([string]$taskProcess.CommandLine).Contains($taskLauncher)) {
    Remove-Item -LiteralPath $taskPidFile
    Write-Host 'PID-ul salvat nu mai apartine VERCON; l-am sters fara sa opresc nimic.'
    return
}
if ($taskProcess) {
    # The Windows venv launcher can spawn another Python process; include its SSH tunnel.
    function Stop-LocalChild([int]$taskParentId) {
        $taskChildren = @(Get-CimInstance Win32_Process -Filter "ParentProcessId = $taskParentId")
        foreach ($taskChild in $taskChildren) {
            if ($taskChild.Name -notin @('python.exe', 'pythonw.exe', 'ssh.exe')) { continue }
            Stop-LocalChild -taskParentId $taskChild.ProcessId
            Stop-Process -Id $taskChild.ProcessId -Force -ErrorAction SilentlyContinue
        }
    }
    Stop-LocalChild -taskParentId $taskProcessId
    Stop-Process -Id $taskProcessId -Force -ErrorAction SilentlyContinue
}
Remove-Item -LiteralPath $taskPidFile
Write-Host "VERCON localhost:$Port oprit. Datele 000_DEMO au fost pastrate."
