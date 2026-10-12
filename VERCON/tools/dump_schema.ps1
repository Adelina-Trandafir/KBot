# VC00-02: read-only DAO dump of schema (fields, indexes, relations, row counts). No row data is read.
param([string]$Out = (Join-Path $PSScriptRoot 'out'))
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Force $Out | Out-Null
$eng = New-Object -ComObject DAO.DBEngine.120
$dbDir = Join-Path (Split-Path $PSScriptRoot) 'VERCON_ACCESS\Databases'
foreach ($f in 'cale','tz_Alimente','tz_Mijloace_Fixe','tz_Obiecte_Inventar') {
  $db = $null
  try {
    $db = $eng.OpenDatabase((Join-Path $dbDir "$f.accdb"), $false, $true)
    $res = [ordered]@{ db = $f; tables = @(); relations = @() }
    foreach ($t in $db.TableDefs) {
      if ($t.Name -like 'MSys*' -or $t.Name -like '~*' -or ($t.Attributes -band 0x80000000)) { continue }
      $fl = @(); foreach ($c in $t.Fields) { $fl += [ordered]@{ n = $c.Name; t = [int]$c.Type; sz = [int]$c.Size; req = [bool]$c.Required; ai = [bool](($c.Attributes -band 16) -ne 0); } }
      $ix = @(); foreach ($i in $t.Indexes) { $cols = @(); foreach ($c in $i.Fields) { $cols += $c.Name }; $ix += [ordered]@{ n = $i.Name; cols = $cols; pk = [bool]$i.Primary; uq = [bool]$i.Unique; fk = [bool]$i.Foreign } }
      $rs = $db.OpenRecordset("SELECT Count(*) AS N FROM [$($t.Name)]", 4)
      $cnt = $rs.Fields(0).Value; $rs.Close()
      $res.tables += [ordered]@{ name = $t.Name; fields = $fl; indexes = $ix; rows = [int]$cnt }
    }
    foreach ($r in $db.Relations) { $fl = @(); foreach ($c in $r.Fields) { $fl += "$($c.Name)->$($c.ForeignName)" }; $res.relations += [ordered]@{ n = $r.Name; parent = $r.Table; child = $r.ForeignTable; attr = [int]$r.Attributes; fields = $fl } }
    $res | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $Out "schema_$f.json") -Encoding UTF8
    "$f ok tables=$($res.tables.Count) rel=$($res.relations.Count)"
  } finally { if ($db) { $db.Close() } }
}
