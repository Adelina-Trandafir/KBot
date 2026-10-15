param([int]$Port = 5053, [string]$Actor = 'localhost-ssh@local.test', [switch]$Foreground)
$ErrorActionPreference = 'Stop'
# Windows PowerShell 5.1 does not load this assembly by itself (PowerShell 7 does).
Add-Type -AssemblyName System.Net.Http
$taskRepo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$taskPython = Join-Path $taskRepo 'PYTHON\.venv\Scripts\python.exe'
$taskLauncher = Join-Path $PSScriptRoot 'local_ssh.py'
$taskLocal = Join-Path $PSScriptRoot '.local'
if ($Port -lt 1024 -or $Port -gt 65535) { throw 'Portul trebuie sa fie intre 1024 si 65535.' }
if ($Actor.Contains('"')) { throw 'Identitatea pentru jurnal nu poate contine ghilimele.' }
if (-not (Test-Path -LiteralPath $taskPython)) { throw 'Lipseste mediul PYTHON\.venv.' }
$taskHandler = [System.Net.Http.HttpClientHandler]::new()
$taskHandler.UseProxy = $false
$taskHttp = [System.Net.Http.HttpClient]::new($taskHandler)
$taskHttp.Timeout = [TimeSpan]::FromSeconds(1)
function Read-LocalStatus {
    $taskResponse = $taskHttp.GetStringAsync("http://127.0.0.1:$Port/__vercon_local_status").GetAwaiter().GetResult()
    return ($taskResponse | ConvertFrom-Json)
}
if ($Foreground) {
    & $taskPython -u $taskLauncher --port $Port --actor $Actor
    return
}
$taskPidFile = Join-Path $taskLocal "localhost-$Port.pid"
if (Test-Path -LiteralPath $taskPidFile) {
    $taskExistingId = [int](Get-Content -LiteralPath $taskPidFile -Raw)
    $taskExisting = Get-CimInstance Win32_Process -Filter "ProcessId = $taskExistingId" -ErrorAction SilentlyContinue
    # CommandLine is empty for a process that is gone or that cannot be read: then the saved PID is stale and nothing is running.
    if ($taskExisting -and ([string]$taskExisting.CommandLine).Contains($taskLauncher)) {
        try {
            $taskStatus = Read-LocalStatus
            if ($taskStatus.application -eq 'vercon_local_ssh' -and $taskStatus.database -eq '000_DEMO' -and $taskStatus.ready) {
                Write-Host "VERCON este deja pornit: http://localhost:$Port/vercon (000_DEMO)"
                return
            }
        } catch { }
        throw 'Exista un proces vechi VERCON fara server disponibil. Opriti-l cu Stop-Localhost.ps1, apoi reporniti.'
    }
}
New-Item -ItemType Directory -Path $taskLocal -Force | Out-Null
$taskOut = Join-Path $taskLocal "localhost-$Port.out.log"
$taskErr = Join-Path $taskLocal "localhost-$Port.err.log"
$taskArguments = '-u "{0}" --port {1} --actor "{2}"' -f $taskLauncher, $Port, $Actor
$taskProcess = Start-Process -FilePath $taskPython -ArgumentList $taskArguments -WorkingDirectory $taskRepo -WindowStyle Hidden -PassThru -RedirectStandardOutput $taskOut -RedirectStandardError $taskErr
Set-Content -LiteralPath $taskPidFile -Value $taskProcess.Id -Encoding ascii
$taskReady = $false
for ($taskAttempt = 0; $taskAttempt -lt 40; $taskAttempt++) {
    $taskProcess.Refresh()
    if ($taskProcess.HasExited) { break }
    try {
        $taskStatus = Read-LocalStatus
        if ($taskStatus.application -eq 'vercon_local_ssh' -and $taskStatus.database -eq '000_DEMO' -and $taskStatus.ready) { $taskReady = $true; break }
    } catch { }
    Start-Sleep -Milliseconds 500
}
if (-not $taskReady) {
    if (Test-Path -LiteralPath $taskErr) { Get-Content -LiteralPath $taskErr -Tail 12 | Write-Host }
    throw "VERCON nu a devenit disponibil. Vedeti $taskErr. Nu este confirmata pornirea."
}
Write-Host "VERCON pornit: http://localhost:$Port/vercon (000_DEMO prin SSH)"
Write-Host "Jurnal pornire: $taskOut"
Write-Host "Erori pornire: $taskErr"
Write-Host "Oprire: .\VERCON\tools\Stop-Localhost.ps1 -Port $Port"
