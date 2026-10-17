param(
    [ValidateRange(1024, 65535)]
    [int]$Port = 5053,
    [switch]$Background
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$previewPython = Join-Path $repo 'PYTHON\.venv\Scripts\python.exe'
$previewServer = Join-Path $PSScriptRoot 'preview_server.py'

if (-not (Test-Path -LiteralPath $previewPython)) {
    throw "Lipseste mediul Python: $previewPython. Vezi README.md din acest director."
}

if ($Background) {
    $listener = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    if ($listener) { throw "Portul $Port este deja folosit. Alege alt port sau opreste preview-ul existent." }
    $previewArguments = '"{0}" --port {1}' -f $previewServer, $Port
    $previewProcess = Start-Process -FilePath $previewPython -ArgumentList $previewArguments `
        -WorkingDirectory $PSScriptRoot -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $PSScriptRoot 'preview.stdout.log') `
        -RedirectStandardError (Join-Path $PSScriptRoot 'preview.stderr.log')
    Set-Content -LiteralPath (Join-Path $PSScriptRoot 'preview.pid') -Value $previewProcess.Id
    Write-Host "VERCON demo: http://localhost:$Port/ (PID $($previewProcess.Id))"
} else {
    Write-Host "VERCON demo: http://localhost:$Port/ (Ctrl+C opreste serverul)"
    & $previewPython $previewServer --port $Port
}
