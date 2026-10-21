# Publishes KBotAdmin (the owner's private administration program, slice ME01) to <repo root>\_Admin so it
# can be run on its own: _Admin\KBotAdmin.exe. The folder is git-ignored. This is NOT part of any K-BOT
# delivery (publish-release.ps1 / push-update.ps1 refuse a package that holds KBotAdmin files).
# Needs the .NET 8 Desktop Runtime on the PC that runs it.
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$out = Join-Path $root '_Admin'
if (Test-Path $out) {
    # keep the owner's own settings (server, key path, pinned fingerprint) across publishes
    $keep = Join-Path $out 'admin_settings.json'
    $saved = if (Test-Path $keep) { Get-Content $keep -Raw } else { $null }
    # ME02: the key of the local backup (protected by this Windows account) lives next to the exe; a publish must not lose it
    $keys = @(Get-ChildItem -Path $out -Filter 'backup_key_*.dat' -ErrorAction SilentlyContinue | ForEach-Object { [pscustomobject]@{ Name = $_.Name; Bytes = [IO.File]::ReadAllBytes($_.FullName) } })
    Remove-Item $out -Recurse -Force
}
dotnet publish (Join-Path $root 'KBotAdmin\KBotAdmin.vbproj') -c Release -o $out --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }
if ($saved) { Set-Content -Path (Join-Path $out 'admin_settings.json') -Value $saved -NoNewline -Encoding utf8 }
foreach ($k in $keys) { [IO.File]::WriteAllBytes((Join-Path $out $k.Name), $k.Bytes) }
Write-Host "KBotAdmin published to $out"
