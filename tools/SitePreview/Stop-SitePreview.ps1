<#
.SYNOPSIS
  Stops the local site PREVIEW (see Start-SitePreview.ps1) and prints a short summary of the session.

.DESCRIPTION
  Finds whatever listens on the port, shows what it was (process, started, ran for, memory,
  connections), then stops it. Nothing listening is not an error.

  -Port  default 5050.
  -InfoOnly  only show the info, do not stop anything.
#>
param(
    [int]$Port = 5050,
    [switch]$InfoOnly
)

$ErrorActionPreference = 'Stop'

$listeners = @(Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue)
if ($listeners.Count -eq 0) {
    Write-Host "Nimic nu asculta pe portul $Port." -ForegroundColor Yellow
    return
}

$connections = @(Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue |
    Where-Object { $_.State -eq 'Established' })

foreach ($processId in ($listeners | Select-Object -ExpandProperty OwningProcess -Unique)) {
    $proc = Get-Process -Id $processId -ErrorAction SilentlyContinue
    $cim = Get-CimInstance Win32_Process -Filter "ProcessId = $processId" -ErrorAction SilentlyContinue
    $bind = ($listeners | Where-Object { $_.OwningProcess -eq $processId } |
        ForEach-Object { "$($_.LocalAddress):$($_.LocalPort)" }) -join ', '

    Write-Host ''
    Write-Host "Sesiune previzualizare K-BOT (port $Port)" -ForegroundColor Cyan
    Write-Host "  Proces      : $($proc.ProcessName) (PID $processId)"
    Write-Host "  Asculta pe  : $bind"
    if ($proc) {
        $ran = (Get-Date) - $proc.StartTime
        Write-Host "  Pornit la   : $($proc.StartTime.ToString('dd.MM.yyyy HH:mm:ss'))"
        Write-Host ("  A rulat     : {0:00}:{1:00}:{2:00}" -f [int][math]::Floor($ran.TotalHours), $ran.Minutes, $ran.Seconds)
        Write-Host ("  Memorie     : {0:N1} MB" -f ($proc.WorkingSet64 / 1MB))
        Write-Host ("  CPU total   : {0:N1} s" -f $proc.CPU)
    }
    if ($cim -and $cim.CommandLine) {
        Write-Host "  Comanda     : $($cim.CommandLine)"
    }
    Write-Host "  Conexiuni   : $($connections.Count) active acum"
    if ($cim -and $cim.CommandLine -match '--lan') {
        Write-Host '  Mod         : LAN (accesibil si din retea)'
    }
    else {
        Write-Host '  Mod         : doar acest calculator'
    }

    if ($InfoOnly) { continue }

    Stop-Process -Id $processId -Force
    Write-Host "  Oprit." -ForegroundColor Green
}
