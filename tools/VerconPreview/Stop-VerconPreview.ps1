$ErrorActionPreference = 'Stop'
$previewPidFile = Join-Path $PSScriptRoot 'preview.pid'
if (-not (Test-Path -LiteralPath $previewPidFile)) {
    Write-Host 'Nu exista un preview pornit in fundal cu acest launcher.'
    return
}
$previewProcessId = [int](Get-Content -LiteralPath $previewPidFile -Raw).Trim()
$previewProcess = Get-CimInstance Win32_Process -Filter "ProcessId = $previewProcessId"
$previewServer = Join-Path $PSScriptRoot 'preview_server.py'
if ($previewProcess -and $previewProcess.CommandLine -and
    $previewProcess.CommandLine.Contains($previewServer)) {
    Stop-Process -Id $previewProcessId
    Write-Host 'Preview VERCON oprit.'
} elseif ($previewProcess) {
    throw 'PID-ul apartine altui proces. Nu il opresc.'
}
Remove-Item -LiteralPath $previewPidFile
