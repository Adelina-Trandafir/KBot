# VC00: suppliers (Furnizori) vs documents. Aggregates only; supplier names are matched by pattern, never printed.
$eng = New-Object -ComObject DAO.DBEngine.120
$dbDir = Join-Path (Split-Path $PSScriptRoot) 'VERCON_ACCESS\Databases'
$q = [ordered]@{
 'furnizori_total'        = "SELECT Count(*) FROM Furnizori"
 'furnizori_fara_NIR'     = "SELECT Count(*) FROM Furnizori WHERE IDF NOT IN (SELECT IDF FROM NIR WHERE IDF Is Not Null)"
 'furnizori_cf_gol'       = "SELECT Count(*) FROM Furnizori WHERE CF Is Null OR Trim(CF)=''"
 'furnizori_cf_duplicat'  = "SELECT Count(*) FROM (SELECT CF FROM Furnizori WHERE CF Is Not Null AND Trim(CF)<>'' GROUP BY CF HAVING Count(*)>1)"
 'furnizori_pattern_SI'   = "SELECT Count(*) FROM Furnizori WHERE Denumire LIKE '*stoc*ini*' OR Denumire LIKE '*stoc*ini*' OR Denumire LIKE '*S.I*' OR Denumire LIKE 'SI *' OR Denumire LIKE '*sold*ini*'"
 'NIR_pe_furnizori_SI'    = "SELECT Count(*) FROM NIR WHERE IDF IN (SELECT IDF FROM Furnizori WHERE Denumire LIKE '*stoc*ini*' OR Denumire LIKE '*S.I*' OR Denumire LIKE 'SI *' OR Denumire LIKE '*sold*ini*')"
 'linii_pe_furnizori_SI'  = "SELECT Count(*) FROM Intrari WHERE IdIntrare IN (SELECT IdIntrare FROM NIR WHERE IDF IN (SELECT IDF FROM Furnizori WHERE Denumire LIKE '*stoc*ini*' OR Denumire LIKE '*S.I*' OR Denumire LIKE 'SI *' OR Denumire LIKE '*sold*ini*'))"
 'NIR_total'              = "SELECT Count(*) FROM NIR"
 'NIR_IDF_null_sau_0'     = "SELECT Count(*) FROM NIR WHERE IDF Is Null OR IDF=0"
 'NIR_IDFV_nenul'         = "SELECT Count(*) FROM NIR WHERE IDFV Is Not Null AND IDFV<>0"
 'NIR_FurnizorV_nenul'    = "SELECT Count(*) FROM NIR WHERE FurnizorV Is Not Null AND Trim(FurnizorV)<>''"
 'CF_forma_RO_prefix'     = "SELECT Count(*) FROM Furnizori WHERE CF LIKE 'RO*'"
 'clienti_total'          = "SELECT Count(*) FROM Clienti"
 'clienti_CF_P'           = "SELECT Count(*) FROM Clienti WHERE CF='P'"
}
foreach ($f in 'tz_Alimente','tz_Mijloace_Fixe','tz_Obiecte_Inventar') {
  $db = $eng.OpenDatabase((Join-Path $dbDir "$f.accdb"), $false, $true)
  try { "### $f"
    foreach ($k in $q.Keys) { try { $rs=$db.OpenRecordset($q[$k],4); "$k = " + $rs.Fields(0).Value; $rs.Close() } catch { "$k = ERR " + $_.Exception.Message.Split("`n")[0] } }
  } finally { $db.Close() }
}
