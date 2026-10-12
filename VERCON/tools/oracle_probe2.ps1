# VC00-03: second probe (TVA semantics, effective decimals per column, header vs lines). Aggregates only.
$ErrorActionPreference = 'Stop'
$eng = New-Object -ComObject DAO.DBEngine.120
$dbDir = Join-Path (Split-Path $PSScriptRoot) 'VERCON_ACCESS\Databases'
$cols = @{ Intrari = 'Cant','Pret','TVA','Valoare','PUTVA','ValoareCUTVA'; Iesiri = 'Cant','PUTVA','ValoareCUTVA'; NIR = 'Valoare','Numar'; Iesiri_H = 'ValoareBC'; ProduseP = 'PUTVA' }
$q = [ordered]@{
 'cutva_eq_val_plus_tva' = "SELECT Count(*) FROM Intrari WHERE Abs(ValoareCUTVA - (Valoare + TVA)) <= 0.005"
 'tva_eq_val_x_rate'     = "SELECT Count(*) FROM Intrari AS I INNER JOIN TVA AS T ON I.IDTVA = T.IdTVA WHERE Abs(I.TVA - I.Valoare*T.TVA) <= 0.005"
 'tva_ne_val_x_rate'     = "SELECT Count(*) FROM Intrari AS I INNER JOIN TVA AS T ON I.IDTVA = T.IdTVA WHERE Abs(I.TVA - I.Valoare*T.TVA) > 0.005"
 'putva_eq_pret_x_1prate'= "SELECT Count(*) FROM Intrari AS I INNER JOIN TVA AS T ON I.IDTVA = T.IdTVA WHERE Abs(I.PUTVA - I.Pret*(1+T.TVA)) <= 0.00005"
 'idtva_dist'            = "SELECT IDTVA, Count(*) FROM Intrari GROUP BY IDTVA"
 'iesiri_mismatch_rows'  = "SELECT Count(*) FROM Iesiri WHERE Abs(ValoareCUTVA - Cant*PUTVA) > 0.005"
 'hdr_gt1_diff'          = "SELECT Count(*) FROM NIR AS H INNER JOIN (SELECT IdIntrare, Sum(ValoareCUTVA) AS S FROM Intrari GROUP BY IdIntrare) AS T ON H.IdIntrare = T.IdIntrare WHERE H.Valoare = 0 OR Abs(H.Valoare - T.S) > 1"
 'hdr_eq1'               = "SELECT Count(*) FROM NIR AS H INNER JOIN (SELECT IdIntrare, Sum(ValoareCUTVA) AS S FROM Intrari GROUP BY IdIntrare) AS T ON H.IdIntrare = T.IdIntrare WHERE H.Valoare <> 0 AND Abs(H.Valoare - T.S) > 0.01 AND Abs(H.Valoare - T.S) <= 1"
 'bc_hdr_gt1'            = "SELECT Count(*) FROM Iesiri_H AS H INNER JOIN (SELECT IdBonConsum, Sum(ValoareCUTVA) AS S FROM Iesiri GROUP BY IdBonConsum) AS T ON H.IdBonConsum = T.IdBonConsum WHERE H.Retur = False AND (H.ValoareBC = 0 OR Abs(H.ValoareBC - T.S) > 1)"
 'bc_hdr_le1_gt001'      = "SELECT Count(*) FROM Iesiri_H AS H INNER JOIN (SELECT IdBonConsum, Sum(ValoareCUTVA) AS S FROM Iesiri GROUP BY IdBonConsum) AS T ON H.IdBonConsum = T.IdBonConsum WHERE H.Retur = False AND H.ValoareBC <> 0 AND Abs(H.ValoareBC - T.S) > 0.01 AND Abs(H.ValoareBC - T.S) <= 1"
 'nir_max_sum'           = "SELECT Max(S) FROM (SELECT IdIntrare, Sum(ValoareCUTVA) AS S FROM Intrari GROUP BY IdIntrare)"
 'bc_max_sum'            = "SELECT Max(S) FROM (SELECT IdBonConsum, Sum(ValoareCUTVA) AS S FROM Iesiri GROUP BY IdBonConsum)"
}
foreach ($f in 'tz_Alimente','tz_Mijloace_Fixe','tz_Obiecte_Inventar') {
  $db = $eng.OpenDatabase((Join-Path $dbDir "$f.accdb"), $false, $true)
  try {
    "### $f"
    foreach ($k in $q.Keys) { try { $rs=$db.OpenRecordset($q[$k],4); $o=@(); while(-not $rs.EOF){ $r=@(); for($i=0;$i -lt $rs.Fields.Count;$i++){ $r+=[string]$rs.Fields($i).Value }; $o+=($r -join ','); $rs.MoveNext() }; $rs.Close(); "$k = " + ($o -join ' | ') } catch { "$k = ERR " + $_.Exception.Message.Split("`n")[0] } }
    foreach ($t in $cols.Keys) { foreach ($c in $cols[$t]) {
      $found = -1
      for ($k = 0; $k -le 6; $k++) {
        $p = [math]::Pow(10, $k)
        $sql = "SELECT Count(*) FROM [$t] WHERE [$c] Is Not Null AND Abs([$c]*$p - Int([$c]*$p + 0.5)) > 0.0005"
        try { $rs = $db.OpenRecordset($sql, 4); $n = [int]$rs.Fields(0).Value; $rs.Close() } catch { $n = -1; break }
        if ($n -eq 0) { $found = $k; break }
      }
      $rs = $db.OpenRecordset("SELECT Min([$c]), Max([$c]) FROM [$t]", 4); $mm = "$($rs.Fields(0).Value)..$($rs.Fields(1).Value)"; $rs.Close()
      "decimals $t.$c = $found  range $mm"
    } }
  } finally { $db.Close() }
}
