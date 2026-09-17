# Rebuild missing FX_Receptii_H / FX_Receptii rows from FX_Istoric through curl (slice 0062).
#
# Usage (PowerShell):
#   ADMIN (no login; the server key + the database name):
#   .\refacere.ps1 -Db 000_DEMO                                       # whole DB, DRY RUN
#   .\refacere.ps1 -Db 000_DEMO -Apply                                # whole DB, WRITE
#   .\refacere.ps1 -Db 000_DEMO -Cod AAB37CNBK95                      # one angajament
#   (the key is asked at a masked prompt, or pass -ApiKey; env KBOT_API_KEY is also read)
#
#   OPERATOR (login on a unit, bearer token):
#   .\refacere.ps1 -User you@example.com -Dc 1234                    # whole DB, DRY RUN
#   .\refacere.ps1 -User you@example.com -Dc 1234 -Apply             # whole DB, WRITE
#   .\refacere.ps1 -User you@example.com -Dc 1234 -Cod AAB37CNBK95   # one angajament, dry run
#   .\refacere.ps1 -User you@example.com -Dc 1234 -ListUnits         # just list the DCs you can open
#   add -Host http://127.0.0.1:5000 for a local Flask.
#
# The password is asked at a prompt (masked) and never written anywhere. The response JSON
# is printed pretty and also saved next to this script as refacere_<timestamp>.json.
param(
    [string] $User,
    [string] $Dc,
    [string] $Db,
    [string] $ApiKey,
    [string] $Cod,
    [switch] $Apply,
    [switch] $ListUnits,
    [string] $Host = "https://kbot.avatarsoft.ro"
)

$ErrorActionPreference = "Stop"

function Read-Secret([string] $Prompt) {
    $secure = Read-Host -Prompt $Prompt -AsSecureString
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
}

function Invoke-Json([string] $Url, [string] $Body, [string] $Token, [string] $Key) {
    # Body goes through a temp file: no quoting fights with curl.exe on Windows.
    $tmp = [IO.Path]::GetTempFileName()
    try {
        [IO.File]::WriteAllText($tmp, $Body, (New-Object Text.UTF8Encoding($false)))
        $args = @("-s", "-S", "-X", "POST", $Url, "-H", "Content-Type: application/json",
                  "--data-binary", "@$tmp")
        if ($Token) { $args += @("-H", "Authorization: Bearer $Token") }
        if ($Key)   { $args += @("-H", "X-Api-Key: $Key") }
        $raw = & curl.exe @args
        if ($LASTEXITCODE -ne 0) { throw "curl a ieșit cu codul $LASTEXITCODE" }
        return $raw
    } finally { Remove-Item $tmp -Force -ErrorAction SilentlyContinue }
}

$token = $null
$key = $null

if ($Db) {
    # ---- ADMIN path: X-Api-Key + db_name, no login ----
    $key = if ($ApiKey) { $ApiKey } elseif ($env:KBOT_API_KEY) { $env:KBOT_API_KEY } else { Read-Secret "API_KEY (config.py)" }
    if ($Cod) { $req = @{ db_name = $Db; cod = $Cod; aplica = [bool]$Apply } }
    else      { $req = @{ db_name = $Db; toate = $true; aplica = [bool]$Apply } }
    $url = "$Host/api/admin/receptii/refacere"
    $baza = $Db
} else {
    # ---- OPERATOR path: login on a unit -> bearer token ----
    if (-not $User) { throw "Lipsește -Db (admin) sau -User (operator)." }
    $password = Read-Secret "Parola pentru $User"
    if ($ListUnits) {
        $body = @{ username = $User; password = $password } | ConvertTo-Json -Compress
        Invoke-Json "$Host/api/auth/units" $body $null $null | ConvertFrom-Json | ConvertTo-Json -Depth 5
        exit 0
    }
    if (-not $Dc) { throw "Lipsește -Dc (baza pe care se lucrează). Vezi -ListUnits." }

    $loginBody = @{ username = $User; password = $password; db_name = $Dc; machine = $env:COMPUTERNAME } |
        ConvertTo-Json -Compress
    $login = Invoke-Json "$Host/api/auth/login" $loginBody $null $null | ConvertFrom-Json
    if (-not $login.Token) { throw "Login refuzat: $($login.error)" }
    $token = $login.Token
    $password = $null

    if ($Cod) { $req = @{ cod = $Cod; aplica = [bool]$Apply } }
    else      { $req = @{ toate = $true; aplica = [bool]$Apply } }
    $url = "$Host/api/forexe/receptii/refacere"
    $baza = $Dc
}

$mode = if ($Apply) { "APLICARE (scrie)" } else { "PROBA (nu scrie nimic)" }
$scope = if ($Cod) { "angajamentul $Cod din $baza" } else { "TOATA baza $baza" }
Write-Host "Refacere: $scope - $mode" -ForegroundColor Cyan

$raw = Invoke-Json $url ($req | ConvertTo-Json -Compress) $token $key
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$out = Join-Path $PSScriptRoot "refacere_$stamp.json"
[IO.File]::WriteAllText($out, $raw, (New-Object Text.UTF8Encoding($false)))

$json = $raw | ConvertFrom-Json
if ($json.error) { Write-Host "EROARE: $($json.error)" -ForegroundColor Red; exit 1 }

if ($json.toate) {
    Write-Host ("Angajamente parcurse: {0}   cu lipsuri: {1}   erori: {2}" -f
        $json.angajamente, $json.cu_lipsuri, $json.erori.Count)
    Write-Host ("Totaluri: " + ($json.totaluri | ConvertTo-Json -Compress))
    foreach ($d in $json.detalii) {
        Write-Host ("  {0}: antete lipsa {1}, linii lipsa {2}, orfane {3} | scrise H {4}, linii {5}, relegate {6}" -f
            $d.cod, $d.antete_lipsa, $d.linii_lipsa, $d.linii_orfane,
            $d.antete_scrise, $d.linii_scrise, $d.linii_relegate)
        foreach ($a in $d.avertismente) { Write-Host "      ! $a" -ForegroundColor Yellow }
    }
    foreach ($e in $json.erori) { Write-Host ("  {0}: EROARE {1}" -f $e.cod, $e.eroare) -ForegroundColor Red }
} else {
    $json | ConvertTo-Json -Depth 5
}
Write-Host "Raspunsul complet: $out"

# logout (operator path only, best effort)
if ($token) { & curl.exe -s -o NUL -X POST "$Host/api/auth/logout" -H "Authorization: Bearer $token" }
