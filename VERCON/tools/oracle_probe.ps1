# VC00-03: read-only aggregate probes (counts/min/max only, no row output) used to document formulas.
$ErrorActionPreference = 'Stop'
$eng = New-Object -ComObject DAO.DBEngine.120
$dbDir = Join-Path (Split-Path $PSScriptRoot) 'VERCON_ACCESS\Databases'
$q = [ordered]@{
 'tva_distinct'   = "SELECT TVA, Count(*) AS N FROM Intrari GROUP BY TVA ORDER BY TVA"
 'tvatbl'         = "SELECT * FROM TVA"
 'val_eq_cant_pret_r2' = "SELECT Count(*) FROM Intrari WHERE Abs(Valoare - Cant*Pret) <= 0.005"
 'val_ne_cant_pret' = "SELECT Count(*) FROM Intrari WHERE Abs(Valoare - Cant*Pret) > 0.005"
 'cutva_eq_val_tva' = "SELECT Count(*) FROM Intrari WHERE Abs(ValoareCUTVA - Valoare*(1+TVA/100)) <= 0.005"
 'cutva_ne_val_tva' = "SELECT Count(*) FROM Intrari WHERE Abs(ValoareCUTVA - Valoare*(1+TVA/100)) > 0.005"
 'cutva_eq_cant_putva' = "SELECT Count(*) FROM Intrari WHERE Abs(ValoareCUTVA - Cant*PUTVA) <= 0.005"
 'cutva_ne_cant_putva' = "SELECT Count(*) FROM Intrari WHERE Abs(ValoareCUTVA - Cant*PUTVA) > 0.005"
 'putva_eq_pret_tva' = "SELECT Count(*) FROM Intrari WHERE Abs(PUTVA - Pret*(1+TVA/100)) <= 0.00005"
 'putva_ne_pret_tva' = "SELECT Count(*) FROM Intrari WHERE Abs(PUTVA - Pret*(1+TVA/100)) > 0.00005"
 'iesiri_val_eq' = "SELECT Count(*) FROM Iesiri WHERE Abs(ValoareCUTVA - Cant*PUTVA) <= 0.005"
 'iesiri_val_ne' = "SELECT Count(*) FROM Iesiri WHERE Abs(ValoareCUTVA - Cant*PUTVA) > 0.005"
 'intrari_minmax' = "SELECT Min(Cant), Max(Cant), Min(Pret), Max(Pret), Min(PUTVA), Max(PUTVA), Min(ValoareCUTVA), Max(ValoareCUTVA) FROM Intrari"
 'iesiri_minmax' = "SELECT Min(Cant), Max(Cant), Min(PUTVA), Max(PUTVA), Min(ValoareCUTVA), Max(ValoareCUTVA) FROM Iesiri"
 'nir_minmax'    = "SELECT Min(Valoare), Max(Valoare), Count(*) FROM NIR"
 'bc_minmax'     = "SELECT Min(ValoareBC), Max(ValoareBC) FROM Iesiri_H"
 'neg_price_in'  = "SELECT Count(*) FROM Intrari WHERE Pret < 0 OR PUTVA < 0"
 'neg_price_out' = "SELECT Count(*) FROM Iesiri WHERE PUTVA < 0"
 'neg_putva_prodP' = "SELECT Count(*) FROM ProduseP WHERE PUTVA < 0"
 'nr_row'        = "SELECT ZEC, SI_LA FROM NR"
 'setari'        = "SELECT S, V FROM Setari"
 'nir_h0'        = "SELECT Count(*) FROM NIR WHERE Valoare = 0"
 'nir_hnull'     = "SELECT Count(*) FROM NIR WHERE Valoare Is Null"
 'maxdec_cant'   = "SELECT Count(*) FROM Intrari WHERE Abs(Cant*10000 - Int(Cant*10000+0.5)) > 0.0001"
 'maxdec_pret'   = "SELECT Count(*) FROM Intrari WHERE Abs(Pret*10000 - Int(Pret*10000+0.5)) > 0.0001"
 'maxdec_putva'  = "SELECT Count(*) FROM Intrari WHERE Abs(PUTVA*10000 - Int(PUTVA*10000+0.5)) > 0.0001"
}
foreach ($f in 'tz_Alimente','tz_Mijloace_Fixe','tz_Obiecte_Inventar') {
  $db = $eng.OpenDatabase((Join-Path $dbDir "$f.accdb"), $false, $true)
  try {
    "### $f"
    foreach ($k in $q.Keys) {
      try {
        $rs = $db.OpenRecordset($q[$k], 4); $out = @()
        while (-not $rs.EOF) { $row = @(); for ($i=0; $i -lt $rs.Fields.Count; $i++) { $row += [string]$rs.Fields($i).Value }; $out += ($row -join ','); $rs.MoveNext() }
        $rs.Close(); "$k = " + ($out -join ' | ')
      } catch { "$k = ERR " + $_.Exception.Message.Split("`n")[0] }
    }
  } finally { $db.Close() }
}
