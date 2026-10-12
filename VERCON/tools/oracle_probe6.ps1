# VC00: suppliers that matter for migration (have NIR) and CF quality. Aggregates only.
$eng = New-Object -ComObject DAO.DBEngine.120
$dbDir = Join-Path (Split-Path $PSScriptRoot) 'VERCON_ACCESS\Databases'
$si = "(Denumire LIKE '*stoc*ini*' OR Denumire LIKE '*S.I*' OR Denumire LIKE 'SI *' OR Denumire LIKE '*sold*ini*')"
$q = [ordered]@{
 'furnizori_cu_NIR'                    = "SELECT Count(*) FROM Furnizori WHERE IDF IN (SELECT IDF FROM NIR)"
 'furnizori_cu_NIR_dar_CF_gol'         = "SELECT Count(*) FROM Furnizori WHERE IDF IN (SELECT IDF FROM NIR) AND (CF Is Null OR Trim(CF)='')"
 'furnizori_cu_NIR_CF_gol_fara_SI'     = "SELECT Count(*) FROM Furnizori WHERE IDF IN (SELECT IDF FROM NIR) AND (CF Is Null OR Trim(CF)='') AND NOT $si"
 'furnizori_cu_NIR_si_pattern_SI'      = "SELECT Count(*) FROM Furnizori WHERE IDF IN (SELECT IDF FROM NIR) AND $si"
 'furnizori_SI_fara_NIR'               = "SELECT Count(*) FROM Furnizori WHERE IDF NOT IN (SELECT IDF FROM NIR) AND $si"
 'NIR_cu_IDF_diferit_de_IDFV'          = "SELECT Count(*) FROM NIR WHERE IDF <> IDFV"
 'NIR_storno_si_IDF'                   = "SELECT Count(*) FROM NIR WHERE Storno = True"
}
foreach ($f in 'tz_Alimente','tz_Mijloace_Fixe','tz_Obiecte_Inventar') {
  $db = $eng.OpenDatabase((Join-Path $dbDir "$f.accdb"), $false, $true)
  try { "### $f"
    foreach ($k in $q.Keys) { try { $rs=$db.OpenRecordset($q[$k],4); "$k = " + $rs.Fields(0).Value; $rs.Close() } catch { "$k = ERR " + $_.Exception.Message.Split("`n")[0] } }
  } finally { $db.Close() }
}
