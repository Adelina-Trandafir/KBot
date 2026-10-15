Imports System.Globalization
Imports System.Text

''' <summary>
''' Readable profile report (Markdown) of the files read. Counts and ranges only: no row of data and no name of a person is
''' copied into it. "Propunere" lines are suggestions for the association plan (VC05-02), never decisions.
''' </summary>
Public NotInheritable Class VrcReport

    Private Sub New()
    End Sub

    Private Const DecimalLimit As Double = 10000000000000000.0

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
        k_sb.AppendLine($"- Rol după nume: {KindText(k_p)}.")
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

        AppendUsage(k_sb, k_p)

        If k_p.HeaderChecks.Count > 0 Then
            k_sb.AppendLine()
            k_sb.AppendLine("### Antete față de suma liniilor — doar informativ, valorile sursei se păstrează")
            k_sb.AppendLine()
            k_sb.AppendLine("| Antet | Antete | Antet 0 | Antet NULL | Diferență > 1 leu | Diferență maximă | Fără linii |")
            k_sb.AppendLine("|---|---:|---:|---:|---:|---:|---:|")
            For Each k_h In k_p.HeaderChecks
                k_sb.AppendLine($"| {k_h.HeaderTable} | {k_h.Headers:N0} | {k_h.HeaderZero:N0} | {k_h.HeaderNull:N0} | {k_h.DiffOverOneLeu:N0} | {k_h.MaxAbsDiff.ToString("N2", CultureInfo.InvariantCulture)} | {k_h.WithoutLines:N0} |")
            Next
        End If
    End Sub

    Private Shared Sub AppendUsage(k_sb As StringBuilder, k_p As VrcFileProfile)
        If k_p.Split IsNot Nothing Then
            Dim k_s = k_p.Split
            k_sb.AppendLine()
            k_sb.AppendLine("### Alimente / Materiale după tipul contului (Masini.TIP: A = Alimente, M sau fără tip = Materiale)")
            k_sb.AppendLine()
            k_sb.AppendLine($"- Conturi: {k_s.AccountsAlimente:N0} Alimente, {k_s.AccountsMateriale:N0} Materiale. NIR: {k_s.NirAlimente:N0} / {k_s.NirMateriale:N0}. Bonuri de consum: {k_s.BonAlimente:N0} / {k_s.BonMateriale:N0}. Liste de alimente: {k_s.Lists:N0}.")
            If k_s.Unclassified > 0 Then k_sb.AppendLine($"- {k_s.Unclassified:N0} documente fără cont și fără linii care să le plaseze: merg la Materiale.")
            If k_s.Conflicts > 0 Then k_sb.AppendLine($"- **{k_s.Conflicts:N0} documente au linii din ambele părți (BLOCANT).**")
        End If
        If k_p.MinNirYear > 0 Then k_sb.AppendLine($"- Cel mai mic an din NIR.Data: {k_p.MinNirYear} (anul propus pentru o bază specială).")
        If k_p.ProductCheck IsNot Nothing Then
            Dim k_c = k_p.ProductCheck
            k_sb.AppendLine()
            k_sb.AppendLine("### ProduseP — rânduri cu folosire (se migrează doar acestea)")
            k_sb.AppendLine()
            k_sb.AppendLine($"- Rânduri în ProduseP: {k_c.Total:N0}; **cu folosire în documente: {k_c.Used:N0}** (se migrează); fără folosire: {(k_c.Total - k_c.Used):N0} (nu se transferă). Căutat în: {String.Join(", ", k_c.SearchedIn)}.")
            k_sb.AppendLine($"- PUTVA negativ: {k_c.NegativeUsed:N0} cu folosire (avertisment), {k_c.NegativeUnused:N0} fără folosire (nu se transferă).")
        End If
        If k_p.LineSigns.Count > 0 Then
            k_sb.AppendLine()
            k_sb.AppendLine("### Semne cantitate / preț pe liniile documentelor")
            k_sb.AppendLine()
            k_sb.AppendLine("| Tabel | Linii | Preț negativ (se păstrează) | Cantitate negativă (se păstrează) | AMBELE negative (se păstrează) |")
            k_sb.AppendLine("|---|---:|---:|---:|---:|")
            For Each k_l In k_p.LineSigns
                k_sb.AppendLine($"| {k_l.Table} | {k_l.Lines:N0} | {k_l.NegativePriceOnly:N0} | {k_l.NegativeQuantityOnly:N0} | {If(k_l.BothNegative > 0, "**" & k_l.BothNegative.ToString("N0", CultureInfo.InvariantCulture) & "**", "0")} |")
            Next
        End If
    End Sub

    ''' <summary>Suggestions for the plan: type, calculation/display ZEC and DECIMAL(20,4) capacity.</summary>
    Private Shared Sub AppendSuggestions(k_sb As StringBuilder, k_p As VrcFileProfile)
        k_sb.AppendLine()
        k_sb.AppendLine("### Propuneri pentru plan (nu sunt decizii)")
        k_sb.AppendLine()
        Dim k_obinv As Boolean = Flag(k_p, "Setari.OBINV")
        Dim k_mf As Boolean = Flag(k_p, "Setari.MF")
        Dim k_type As String = If(k_obinv, "OI", If(k_mf, "MF", "MATERIALE sau ALIMENTE (nu există steag: alege operatorul)"))
        k_sb.AppendLine($"- Tip propus din steaguri (OBINV={Flag01(k_p, "Setari.OBINV")}, MF={Flag01(k_p, "Setari.MF")}): **{k_type}**.")
        If k_p.Kind = VrcFileKind.Special AndAlso Not (k_obinv OrElse k_mf) Then
            k_sb.AppendLine("- **ATENȚIE:** fișierul este bază specială (tz_2100) dar nu are steagul OBINV sau MF: tipul trebuie stabilit de operator.")
        ElseIf k_p.Kind = VrcFileKind.Year AndAlso (k_obinv OrElse k_mf) Then
            k_sb.AppendLine("- **ATENȚIE:** fișier de an (nu tz_2100) cu steag OBINV/MF: neașteptat; se verifică înainte de plan.")
        End If
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
            k_sb.AppendLine("- Stocare: **DECIMAL(20,4)**, valori directe cu maximum 4 zecimale, independent de ZEC.")
            k_sb.AppendLine($"- Cea mai mare valoare pe coloană: {Num(k_maxAbs)}; în DECIMAL(20,4): {If(k_maxAbs < DecimalLimit, "încape", "**NU ÎNCAPE**")}.")
            k_sb.AppendLine("- Capacitatea totalurilor (sume pe document/gestiune) se verifică la construirea planului (VC05-02).")
        End If
    End Sub

    Private Shared Function KindText(k_p As VrcFileProfile) As String
        Select Case k_p.Kind
            Case VrcFileKind.Special
                Return "bază specială (tz_2100: Obiecte de inventar sau Mijloace fixe; 2100 nu este un an)"
            Case VrcFileKind.Year
                Return $"exercițiul {k_p.Year} (Materiale sau Alimente)"
            Case Else
                Return "nume neobișnuit, selectat manual"
        End Select
    End Function

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
