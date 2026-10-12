Imports System.Globalization
Imports System.Text

''' <summary>
''' Readable profile report (Markdown) of the files read. Counts and ranges only: no row of data and no name of a person is
''' copied into it. "Propunere" lines are suggestions for the association plan (VC05-02), never decisions.
''' </summary>
Public NotInheritable Class VrcReport

    Private Sub New()
    End Sub

    Private Const IntMax As Double = 2147483647.0

    Public Shared Function ToMarkdown(k_profiles As IEnumerable(Of VrcFileProfile)) As String
        Dim k_sb As New StringBuilder()
        k_sb.AppendLine("# VRC.Migrator — raport de profilare")
        k_sb.AppendLine()
        k_sb.AppendLine($"Generat: {DateTime.Now:yyyy-MM-dd HH:mm}. Citire DOAR din fișiere (mod citire); nimic nu a fost scris în Access sau în MariaDB.")
        For Each k_profile In k_profiles
            AppendFile(k_sb, k_profile)
        Next
        Return k_sb.ToString()
    End Function

    Private Shared Sub AppendFile(k_sb As StringBuilder, k_p As VrcFileProfile)
        k_sb.AppendLine()
        k_sb.AppendLine($"## {k_p.FileName}")
        k_sb.AppendLine()
        If k_p.ReadError.Length > 0 Then
            k_sb.AppendLine($"**NECITIT:** {k_p.ReadError}")
            Return
        End If
        k_sb.AppendLine($"- Dimensiune {k_p.SizeBytes:N0} octeți; SHA-256 `{k_p.HashBefore}`; furnizor de date {k_p.Provider}.")
        k_sb.AppendLine($"- Stabilitate: {If(k_p.Stable, "hash identic înainte și după citire", "**FIȘIERUL S-A SCHIMBAT ÎN TIMPUL CITIRII** (hash diferit): se recitește pe o copie")}.")
        k_sb.AppendLine($"- Tabel `Ver`: {If(k_p.HasVerTable, "există (citit ca diagnostic, nemodificat)", "nu există (nu se creează)")}.")
        If k_p.LinkedTables.Count > 0 Then k_sb.AppendLine($"- **Tabele legate (datele nu sunt în acest fișier):** {String.Join(", ", k_p.LinkedTables)}.")
        If k_p.MissingTables.Count > 0 Then k_sb.AppendLine($"- Tabele așteptate care lipsesc din fișier: {String.Join(", ", k_p.MissingTables)}.")
        If k_p.OtherTables.Count > 0 Then k_sb.AppendLine($"- Alte tabele (neprevăzute în schema așteptată): {String.Join(", ", k_p.OtherTables)}.")

        k_sb.AppendLine()
        k_sb.AppendLine("### Configurare citită")
        k_sb.AppendLine()
        For Each k_pair In k_p.Config
            k_sb.AppendLine($"- `{k_pair.Key}` = {k_pair.Value}")
        Next
        AppendSuggestions(k_sb, k_p)

        k_sb.AppendLine()
        k_sb.AppendLine("### Tabele")
        k_sb.AppendLine()
        k_sb.AppendLine("| Tabel | Rânduri | Coloane lipsă (ABSENTE, ≠ NULL) | Coloane în plus | Observație |")
        k_sb.AppendLine("|---|---:|---|---|---|")
        For Each k_t In k_p.Tables
            k_sb.AppendLine($"| {k_t.Name} | {k_t.Rows:N0} | {Join(k_t.Absent)} | {Join(k_t.Extra)} | {k_t.Problem} |")
        Next

        k_sb.AppendLine()
        k_sb.AppendLine("### Coloane numerice (doar cele cu date)")
        k_sb.AppendLine()
        k_sb.AppendLine("| Tabel.coloană | Clasă | Valori | NULL | Zero | Negative | Minim | Maxim | Zecimale necesare | Distribuție 0/1/2/3/4/>4 |")
        k_sb.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|---:|---|")
        For Each k_t In k_p.Tables
            For Each k_f In k_t.Fields.Where(Function(k_x) k_x.NonNull > 0 OrElse k_x.Nulls > 0)
                k_sb.AppendLine($"| {k_t.Name}.{k_f.Name} | {k_f.FieldClass} | {k_f.NonNull:N0} | {k_f.Nulls:N0} | {k_f.Zeros:N0} | {k_f.Negatives:N0} | {Num(k_f.Minimum)} | {Num(k_f.Maximum)} | {If(k_f.MaxDecimals < 0, "—", If(k_f.MaxDecimals > 4, ">4", k_f.MaxDecimals.ToString(CultureInfo.InvariantCulture)))} | {String.Join("/", k_f.ByDecimals)} |")
            Next
        Next

        If k_p.HeaderChecks.Count > 0 Then
            k_sb.AppendLine()
            k_sb.AppendLine("### Antete față de suma liniilor (N01/N02) — doar numărate")
            k_sb.AppendLine()
            k_sb.AppendLine("| Antet | Antete | Antet 0 | Antet NULL | Diferență > 1 leu | Diferență maximă | Fără linii |")
            k_sb.AppendLine("|---|---:|---:|---:|---:|---:|---:|")
            For Each k_h In k_p.HeaderChecks
                k_sb.AppendLine($"| {k_h.HeaderTable} | {k_h.Headers:N0} | {k_h.HeaderZero:N0} | {k_h.HeaderNull:N0} | {k_h.DiffOverOneLeu:N0} | {k_h.MaxAbsDiff.ToString("N2", CultureInfo.InvariantCulture)} | {k_h.WithoutLines:N0} |")
            Next
        End If
    End Sub

    ''' <summary>Suggestions for the plan: type from the flags, ZEC, the storage scale that holds every value exactly, INT capacity.</summary>
    Private Shared Sub AppendSuggestions(k_sb As StringBuilder, k_p As VrcFileProfile)
        k_sb.AppendLine()
        k_sb.AppendLine("### Propuneri pentru plan (nu sunt decizii)")
        k_sb.AppendLine()
        Dim k_obinv As Boolean = Flag(k_p, "Setari.OBINV")
        Dim k_mf As Boolean = Flag(k_p, "Setari.MF")
        Dim k_type As String = If(k_obinv, "OI", If(k_mf, "MF", "MATERIALE sau ALIMENTE (nu există steag: alege operatorul)"))
        k_sb.AppendLine($"- Tip propus din steaguri (OBINV={Flag01(k_p, "Setari.OBINV")}, MF={Flag01(k_p, "Setari.MF")}): **{k_type}**.")
        Dim k_zec As Integer = 4
        Dim k_zecText As String = "lipsește (se folosește 4)"
        Dim k_zecRaw As String = Nothing
        If k_p.Config.TryGetValue("NR.ZEC", k_zecRaw) AndAlso Integer.TryParse(k_zecRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, k_zec) Then k_zecText = k_zec.ToString(CultureInfo.InvariantCulture)
        k_sb.AppendLine($"- ZEC (NR.ZEC): {k_zecText}.")
        Dim k_needed As Integer = k_zec
        Dim k_maxAbs As Double = 0
        Dim k_valueColumns As Integer = 0
        For Each k_t In k_p.Tables
            For Each k_f In k_t.Fields.Where(Function(k_x) k_x.FieldClass = "VALOARE-INT" AndAlso k_x.NonNull > 0)
                k_valueColumns += 1
                k_needed = Math.Max(k_needed, k_f.MaxDecimals)
                k_maxAbs = Math.Max(k_maxAbs, Math.Max(Math.Abs(k_f.Minimum), Math.Abs(k_f.Maximum)))
            Next
        Next
        If k_valueColumns = 0 Then
            k_sb.AppendLine("- Nu există coloane de valoare cu date: scara nu se poate propune.")
            Return
        End If
        If k_needed > 4 Then
            k_sb.AppendLine("- **BLOCAJ:** există valori cu mai mult de 4 zecimale; scara maximă permisă este 4.")
        Else
            k_sb.AppendLine($"- Scara de stocare propusă: **{k_needed}** (cea mai mică scară ≥ ZEC care reprezintă exact toate valorile citite).")
            Dim k_scaled As Double = k_maxAbs * Math.Pow(10, k_needed)
            k_sb.AppendLine($"- Cea mai mare valoare pe coloană: {Num(k_maxAbs)} → {k_scaled:N0} la scara {k_needed} față de limita INT {IntMax:N0}: {If(k_scaled <= IntMax, "încape", "**NU ÎNCAPE**")}.")
            k_sb.AppendLine("- Capacitatea totalurilor (sume pe document/gestiune) se verifică la construirea planului (VC05-02).")
        End If
    End Sub

    Private Shared Function Flag(k_p As VrcFileProfile, k_key As String) As Boolean
        Dim k_raw As String = Nothing
        Dim k_value As Double
        Return k_p.Config.TryGetValue(k_key, k_raw) AndAlso Double.TryParse(k_raw, NumberStyles.Float, CultureInfo.InvariantCulture, k_value) AndAlso k_value <> 0
    End Function

    Private Shared Function Flag01(k_p As VrcFileProfile, k_key As String) As String
        Dim k_raw As String = Nothing
        Return If(k_p.Config.TryGetValue(k_key, k_raw), k_raw, "absent")
    End Function

    Private Shared Function Join(k_items As List(Of String)) As String
        Return If(k_items.Count = 0, "—", String.Join(", ", k_items))
    End Function

    Private Shared Function Num(k_value As Double) As String
        Return If(Double.IsNaN(k_value), "—", k_value.ToString("0.####", CultureInfo.InvariantCulture))
    End Function

End Class
