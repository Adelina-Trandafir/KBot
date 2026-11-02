# SLICE-ME01-07. Run once the task's final file contents have been written.
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][ValidatePattern('^SLICE-[A-Za-z0-9]{4}-[0-9]{2}$')][string]$Slice,
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Reason,
    [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string[]]$Files
)
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$repoPrefix = $repoRoot.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
$pendingRoot = Join-Path $repoRoot 'docs\deployment\pending'
$reasonText = $Reason.Trim()
if (-not $reasonText -or $reasonText.Contains([char]0)) { throw 'Motivul trebuie completat.' }
$records = @()
foreach ($itemPath in $Files) {
    if ([IO.Path]::IsPathRooted($itemPath)) { throw "Folositi o cale relativa la proiect: $itemPath" }
    $fullPath = [IO.Path]::GetFullPath((Join-Path $repoRoot $itemPath))
    if (-not $fullPath.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) { throw "Fisier in afara proiectului: $itemPath" }
    $relativePath = $fullPath.Substring($repoPrefix.Length).Replace('\', '/')
    if ($relativePath.Split('/') -icontains '.git') { throw 'Nu se inregistreaza fisierele interne Git.' }
    if ($relativePath.StartsWith('docs/deployment/pending/', [StringComparison]::OrdinalIgnoreCase)) { throw 'Registrul nu se inregistreaza pe el insusi.' }
    if (-not [IO.File]::Exists($fullPath)) { throw "Fisierul nu exista: $relativePath" }
    $walkPath = $fullPath
    while ($walkPath.Length -gt $repoRoot.Length) {
        if ((Get-Item -LiteralPath $walkPath -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Legatura simbolica neacceptata: $relativePath" }
        $walkPath = Split-Path -Parent $walkPath
    }
    $records += [ordered]@{
        id = [Guid]::NewGuid().ToString('N')
        file = $relativePath
        slice = $Slice.ToUpperInvariant()
        reason = $reasonText
        sha256 = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash.ToLowerInvariant()
        created_utc = [DateTime]::UtcNow.ToString('o')
    }
}
[IO.Directory]::CreateDirectory($pendingRoot) | Out-Null
foreach ($record in $records) {
    $recordPath = Join-Path $pendingRoot ($record.id + '.json')
    $tempPath = $recordPath + '.tmp'
    try {
        [IO.File]::WriteAllText($tempPath, ($record | ConvertTo-Json -Depth 4), [Text.UTF8Encoding]::new($false))
        [IO.File]::Move($tempPath, $recordPath)
    } finally {
        if ([IO.File]::Exists($tempPath)) { [IO.File]::Delete($tempPath) }
    }
}
Write-Output ("Inregistrate {0} fisiere pentru {1}." -f $records.Count, $Slice.ToUpperInvariant())
