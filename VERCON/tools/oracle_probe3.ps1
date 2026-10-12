# VC00-03: the 114 Alimente lines where ValoareCUTVA <> Valoare + TVA (aggregates only)
$eng = New-Object -ComObject DAO.DBEngine.120
$db = $eng.OpenDatabase((Join-Path (Split-Path $PSScriptRoot) 'VERCON_ACCESS\Databases\tz_Alimente.accdb'), $false, $true)
try {
 foreach ($sql in @(
  "SELECT IDTVA, Count(*), Max(Abs(ValoareCUTVA-(Valoare+TVA))), Min(Abs(ValoareCUTVA-(Valoare+TVA))) FROM Intrari WHERE Abs(ValoareCUTVA-(Valoare+TVA)) > 0.005 GROUP BY IDTVA",
  "SELECT Count(*) FROM Intrari WHERE Abs(ValoareCUTVA-(Valoare+TVA)) > 0.05",
  "SELECT Count(*) FROM Intrari AS I INNER JOIN TVA AS T ON I.IDTVA=T.IdTVA WHERE Abs(I.ValoareCUTVA - I.Valoare*(1+T.TVA)) > 0.005",
  "SELECT Count(*) FROM Intrari AS I INNER JOIN TVA AS T ON I.IDTVA=T.IdTVA WHERE Abs(Round(I.Valoare*(1+T.TVA),2) - Round(I.ValoareCUTVA,2)) > 0.011")) {
  $rs=$db.OpenRecordset($sql,4); $o=@(); while(-not $rs.EOF){ $r=@(); for($i=0;$i -lt $rs.Fields.Count;$i++){$r+=[string]$rs.Fields($i).Value}; $o+=($r -join ','); $rs.MoveNext() }; $rs.Close(); ($o -join ' | ')
 }
} finally { $db.Close() }
