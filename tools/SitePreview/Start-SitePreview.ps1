<#
.SYNOPSIS
  Starts the local PREVIEW of the public K-BOT site (made-up data, nothing stored or mailed).

.DESCRIPTION
  Lets you look at the presentation page, «Cere mai multe detalii» and the web area for
  registered users in a browser BEFORE anything is uploaded to the real server.
  See tools\SitePreview\preview_server.py for what is real and what is made up.

  -Lan   also listen on the local network, so a phone on the same Wi-Fi can open it.
         The address to type on the phone is printed. Windows may ask to allow Python through
         the firewall (allow it for private networks only).
  -Port  default 5050.

  Stop with Ctrl+C. HTML, CSS, JS and content.json changes show on a reload; a Python change needs a restart.
#>
param(
    [switch]$Lan,
    [int]$Port = 5050
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$python = Join-Path $repo 'PYTHON\.venv\Scripts\python.exe'
$server = Join-Path $PSScriptRoot 'preview_server.py'

if (-not (Test-Path $python)) {
    throw "Nu gasesc PYTHON\.venv (mediul Python al serverului): $python"
}

$arguments = @($server, '--port', $Port)
if ($Lan) {
    $arguments += '--lan'
    $addresses = Get-NetIPAddress -AddressFamily IPv4 |
        Where-Object { $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*' -and $_.PrefixOrigin -ne 'WellKnown' } |
        Select-Object -ExpandProperty IPAddress
    Write-Host ''
    Write-Host 'Pe telefon (aceeasi retea Wi-Fi) deschide una dintre adrese:' -ForegroundColor Cyan
    $addresses | ForEach-Object { Write-Host "  http://${_}:$Port/demo" }
}

Write-Host ''
Write-Host "Previzualizare K-BOT: http://localhost:$Port/demo   (Ctrl+C opreste)" -ForegroundColor Green
& $python @arguments
