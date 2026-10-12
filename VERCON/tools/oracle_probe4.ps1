# VC00-03: effect of rounding to NR.ZEC on the real data (aggregates only). Half-up estimate via Int(x*10^z+0.5).
$eng = New-Object -ComObject DAO.DBEngine.120
$dbDir = Join-Path (Split-Path $PSScriptRoot) 'VERCON_ACCESS\Databases'
$cols = @(@('Intrari','Cant'),@('Intrari','Pret'),@('Intrari','PUTVA'),@('Intrari','Valoare'),@('Intrari','TVA'),@('Intrari','ValoareCUTVA'),@('Iesiri','Cant'),@('Iesiri','PUTVA'),@('Iesiri','ValoareCUTVA'),@('ProduseP','PUTVA'),@('Iesiri_H','ValoareBC'),@('NIR','Valoare'))
foreach ($f in 'tz_Alimente','tz_Mijloace_Fixe','tz_Obiecte_Inventar') {
  $db = $eng.OpenDatabase((Join-Path $dbDir "$f.accdb"), $false, $true)
  try {
    $rs = $db.OpenRecordset('SELECT ZEC FROM NR', 4); $z = [int]$rs.Fields(0).Value; $rs.Close(); $p = [math]::Pow(10,$z)
    "### $f  NR.ZEC=$z"
    foreach ($c in $cols) {
      $t=$c[0]; $k=$c[1]
      $sql = "SELECT Count(*), Max(Abs([$k] - Int([$k]*$p+0.5)/$p)), Sum([$k]), Sum(Int([$k]*$p+0.5)/$p) FROM [$t] WHERE [$k] Is Not Null AND Abs([$k]*$p - Int([$k]*$p+0.5)) > 0.0005"
      $rs = $db.OpenRecordset($sql, 4)
      "{0}.{1}: rows_with_more_than_ZEC_decimals={2} max_abs_diff={3} sumdiff={4}" -f $t,$k,$rs.Fields(0).Value,$rs.Fields(1).Value,$(if ($rs.Fields(0).Value -eq 0) { 0 } else { $rs.Fields(2).Value - $rs.Fields(3).Value })
      $rs.Close()
    }
    foreach ($q in @("SELECT Count(*) FROM ListaAlimente","SELECT Count(*) FROM LABC")) { $rs=$db.OpenRecordset($q,4); "$q = " + $rs.Fields(0).Value; $rs.Close() }
  } finally { $db.Close() }
}
