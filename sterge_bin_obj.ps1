$root = (Get-Location).Path   # sau: $root = "C:\calea\ta\folder"

$dirs = Get-ChildItem -Path $root -Recurse -Directory -Force |
    Where-Object { $_.Name -in 'bin','obj' } |
    # sare peste bin/obj aflate în interiorul altui bin/obj (deja acoperite de părinte)
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }

if (-not $dirs) { Write-Host "Nu am găsit niciun folder bin/obj."; return }

"`nFoldere găsite ($($dirs.Count)):"
$dirs | ForEach-Object { "  " + $_.FullName }

$ans = Read-Host "`nȘterg toate aceste foldere? (da/nu)"
if ($ans -eq 'da') {
    $dirs | ForEach-Object {
        Remove-Item -LiteralPath $_.FullName -Recurse -Force
        Write-Host "Șters: $($_.FullName)"
    }
    Write-Host "Gata."
} else {
    Write-Host "Anulat, nu s-a șters nimic."
}