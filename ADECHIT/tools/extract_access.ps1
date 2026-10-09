param(
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][string]$Output
)

$ErrorActionPreference = 'Stop'
$resolvedSource = (Resolve-Path -LiteralPath $Source).Path
$resolvedOutput = [System.IO.Path]::GetFullPath($Output)
$tables = @(
    'Grupe', 'ValoriTaxe', 'Platitori', 'Platitori_sub', 'Delegati', 'LunaD',
    'Prezenta', 'Prezenta_sub', 'Plati', 'Chitante', 'AlteDoc', 'Retur',
    'SS_Buget', 'MutaCopil'
)

function Convert-Value($value) {
    if ($value -is [System.DBNull]) { return $null }
    if ($value -is [datetime]) { return $value.ToString('yyyy-MM-ddTHH:mm:ss.fffffff') }
    return $value
}

function Read-Rows($connection, [string]$sql) {
    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    $reader = $command.ExecuteReader()
    try {
        $rows = @()
        while ($reader.Read()) {
            $row = [ordered]@{}
            for ($index = 0; $index -lt $reader.FieldCount; $index++) {
                $row[$reader.GetName($index)] = Convert-Value $reader.GetValue($index)
            }
            $rows += [pscustomobject]$row
        }
        return $rows
    }
    finally {
        $reader.Close()
        $command.Dispose()
    }
}

$connection = New-Object System.Data.OleDb.OleDbConnection(
    "Provider=Microsoft.ACE.OLEDB.16.0;Data Source=$resolvedSource;Mode=Read;Persist Security Info=False;"
)
$connection.Open()
try {
    $result = [ordered]@{
        format = 'adechit-access-v1'
        source = [System.IO.Path]::GetFileName($resolvedSource)
        extracted_at = [datetime]::UtcNow.ToString('o')
        tables = [ordered]@{}
    }
    foreach ($table in $tables) {
        try {
            $result.tables[$table] = @(Read-Rows $connection "SELECT * FROM [$table]")
        }
        catch {
            throw "Read-only extraction failed for table '$table': $($_.Exception.Message)"
        }
    }
    $configRows = @(Read-Rows $connection "SELECT [CFG], [VL] FROM [CFGs] WHERE [f]='CH'")
    if ($configRows.Count -ne 0) {
        $values = @{}
        foreach ($item in $configRows) { $values[[string]$item.CFG] = $item.VL }
        $result.receipt_config = [ordered]@{
            Serie = [string]$values['SERIE']
            Numar = [int]$values['NUMAR']
            Explicatie = [string]$values['EXPLICATIE']
        }
    }
    $directory = [System.IO.Path]::GetDirectoryName($resolvedOutput)
    if ($directory) { [System.IO.Directory]::CreateDirectory($directory) | Out-Null }
    $json = $result | ConvertTo-Json -Depth 8 -Compress
    [System.IO.File]::WriteAllText($resolvedOutput, $json, (New-Object System.Text.UTF8Encoding($false)))
    Write-Output "Extracted $($tables.Count) tables to $resolvedOutput"
}
finally {
    $connection.Close()
    $connection.Dispose()
}
