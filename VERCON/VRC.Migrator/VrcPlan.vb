Imports System.Globalization
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports KBot.Common

''' <summary>One Access file placed in the plan: which gestiune and exercise it becomes (SLICE-VC05-02).</summary>
Public NotInheritable Class VrcPlanSource

    Public Property Source As VrcSourceFile
    Public Property Profile As VrcFileProfile
    ''' <summary>Name of the target gestiune; several files of one gestiune (the yearly bases) share it.</summary>
    Public Property GestiuneName As String = String.Empty
    ''' <summary>MATERIALE, ALIMENTE, OI or MF.</summary>
    Public Property GestiuneType As String = String.Empty
    ''' <summary>Exercise year of the target. For a tz_&lt;year&gt; file it is the year of the file; for tz_2100 (not a year) the operator chooses.</summary>
    Public Property Year As Integer

    ''' <summary>Which rows of the file this source takes: "A" (Alimente accounts), "M" (Materiale accounts) or "*" (the whole special base).</summary>
    Public ReadOnly Property Part As String
        Get
            Select Case GestiuneType
                Case "ALIMENTE" : Return VrcPartition.Alimente
                Case "MATERIALE" : Return VrcPartition.Materiale
                Case Else : Return VrcPartition.Shared_
            End Select
        End Get
    End Property

End Class

''' <summary>
''' The complete association plan of ONE subunit (SLICE-VC05-02): every source file, the gestiune/type/exercise it becomes, the
''' common precision, and the reasons why the transfer cannot start. The file name only PROPOSES; nothing is approved
''' automatically. The plan has a hash that covers the transformations and the context, not only the files.
''' </summary>
Public NotInheritable Class VrcPlan

    Public Const PlanVersion As String = "1"
    Public Const RuleVersion As String = "VC05-MEAL-TYPES-2026-10-15"

    Public Shared ReadOnly GestiuneTypes As String() = {"MATERIALE", "ALIMENTE", "OI", "MF"}
    Private Const DecimalLimit As Double = 10000000000000000.0

    Public Property Dc As String = String.Empty
    ''' <summary>The base the operator typed as destination; empty = the DC of the files (UNIT.DC). A different one is a deliberate choice (a test base).</summary>
    Public Property TargetDc As String = String.Empty
    Public Property SubunitName As String = String.Empty
    ''' <summary>Partners prepared from ANAF (window «Parteneri ANAF»): inserted into Parteneri INSIDE the transfer transaction, so a failed transfer leaves none of them. Not part of <see cref="Hash"/>.</summary>
    Public Property NewPartners As New List(Of VrcNewPartner)()
    Public Property Zec As Integer = 4
    Public Property Scale As Integer = 4
    Public Property Sources As New List(Of VrcPlanSource)()
    ''' <summary>Reasons the transfer cannot start. Empty = the plan is complete.</summary>
    Public Property Blockers As New List(Of String)()
    Public Property Warnings As New List(Of String)()

    Public ReadOnly Property IsComplete As Boolean
        Get
            Return Blockers.Count = 0 AndAlso Sources.Count > 0
        End Get
    End Property

    ' ---- proposals from the name and the flags (never decisions) ----------------------------------------------------------
    Public Shared Function SuggestType(k_profile As VrcFileProfile) As String
        If Flag(k_profile, "Setari.OBINV") Then Return "OI"
        If Flag(k_profile, "Setari.MF") Then Return "MF"
        If k_profile.Kind = VrcFileKind.Year Then
            ' A year file holds Materiale or Alimente: the presence of food lists says Alimente, but the operator decides.
            Dim k_lists = k_profile.Tables.FirstOrDefault(Function(k_t) String.Equals(k_t.Name, "LABC", StringComparison.OrdinalIgnoreCase))
            Return If(k_lists IsNot Nothing AndAlso k_lists.Rows > 0, "ALIMENTE", "MATERIALE")
        End If
        Return String.Empty
    End Function

    ''' <summary>The plan rows a file proposes: a year file gives one row per kind of account it holds (A = Alimente, the rest = Materiale); a special base gives one.</summary>
    Public Shared Function SuggestTypes(k_profile As VrcFileProfile) As List(Of String)
        Dim k_result As New List(Of String)()
        If k_profile.Kind <> VrcFileKind.Year OrElse k_profile.Split Is Nothing Then
            k_result.Add(SuggestType(k_profile))
            Return k_result
        End If
        If k_profile.Split.HasAlimente Then k_result.Add("ALIMENTE")
        If k_profile.Split.HasMateriale Then k_result.Add("MATERIALE")
        Return k_result
    End Function

    ''' <summary>The exercise year proposed for a special base: the smallest year of its NIR dates (decided 12.10.2026); the current year when it has none.</summary>
    Public Shared Function SuggestYear(k_profile As VrcFileProfile, k_source As VrcSourceFile) As Integer
        If k_source.Kind = VrcFileKind.Year Then Return k_source.Year
        Return If(k_profile.MinNirYear > 0, k_profile.MinNirYear, Date.Now.Year)
    End Function

    ''' <summary>UNIT.NumeUnitate of the profiles, in the order year files (oldest first) then the others; empty names are left out.</summary>
    Public Shared Function UnitNames(k_profiles As IEnumerable(Of VrcFileProfile)) As List(Of String)
        Dim k_ordered = k_profiles.OrderBy(Function(k_p) If(k_p.Kind = VrcFileKind.Year, 0, 1)).ThenBy(Function(k_p) k_p.Year).ToList()
        Dim k_result As New List(Of String)()
        For Each k_p In k_ordered
            Dim k_name As String = Nothing
            If k_p.Config.TryGetValue("UNIT.NumeUnitate", k_name) AndAlso k_name <> "NULL" AndAlso k_name.Trim().Length > 0 Then k_result.Add(k_name.Trim())
        Next
        Return k_result
    End Function

    Public Shared Function SuggestName(k_type As String) As String
        Select Case k_type
            Case "MATERIALE" : Return "Materiale"
            Case "ALIMENTE" : Return "Alimente"
            Case "OI" : Return "Obiecte de inventar"
            Case "MF" : Return "Mijloace fixe"
            Case Else : Return String.Empty
        End Select
    End Function

    Private Shared Function Flag(k_profile As VrcFileProfile, k_key As String) As Boolean
        Dim k_raw As String = Nothing
        Dim k_value As Double
        Return k_profile.Config.TryGetValue(k_key, k_raw) AndAlso Double.TryParse(k_raw, NumberStyles.Float, CultureInfo.InvariantCulture, k_value) AndAlso k_value <> 0
    End Function

    ' ---- validation --------------------------------------------------------------------------------------------------------
    ''' <summary>Recomputes precision, blockers and warnings. Call after every change of the association.</summary>
    Public Sub Evaluate()
        Try
            Blockers.Clear()
            Warnings.Clear()
            If Sources.Count = 0 Then
                Blockers.Add("Planul nu are nicio sursă. Citiți fișierele și asociați-le.")
                Return
            End If
            If SubunitName Is Nothing OrElse SubunitName.Trim().Length = 0 Then
                Blockers.Add("Completați denumirea subunității.")
            ElseIf SubunitName.Trim().Length > 100 Then
                Blockers.Add("Denumirea subunității poate avea cel mult 100 de caractere.")
            End If

            CheckFiles()
            CheckAssociation()
            CheckPrecision()
            CheckData()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcPlan.Evaluate", ex)
            Throw
        End Try
    End Sub

    ''' <summary>One source per file (a year file can feed two plan rows).</summary>
    Private Function DistinctSources() As List(Of VrcPlanSource)
        Return Sources.GroupBy(Function(k_s) k_s.Source.Path, StringComparer.OrdinalIgnoreCase).Select(Function(k_g) k_g.First()).ToList()
    End Function

    Private Sub CheckFiles()
        Dim k_dcs As New SortedSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each k_s In DistinctSources()
            Dim k_p = k_s.Profile
            Dim k_name = k_p.FileName
            If k_p.ReadError.Length > 0 Then
                Blockers.Add($"{k_name}: nu a putut fi citit ({k_p.ReadError}).")
                Continue For
            End If
            If Not k_p.Stable Then Blockers.Add($"{k_name}: fișierul s-a schimbat în timpul citirii; recitiți-l.")
            If k_p.LinkedTables.Count > 0 Then Blockers.Add($"{k_name}: tabele legate ({String.Join(", ", k_p.LinkedTables)}); datele lor nu sunt în fișier.")
            If k_p.MissingTables.Count > 0 Then Warnings.Add($"{k_name}: lipsesc tabelele {String.Join(", ", k_p.MissingTables)} (se consideră goale).")
            For Each k_t In k_p.Tables.Where(Function(k_x) k_x.Absent.Count > 0)
                ' ValoareLunara exists only in the MF base (dictionary section 6): absent in a year file or in OI is expected
                Dim k_absent = k_t.Absent.Where(Function(k_a) Not (k_p.Kind = VrcFileKind.Year AndAlso String.Equals(k_a, "ValoareLunara", StringComparison.OrdinalIgnoreCase))).ToList()
                If k_absent.Count > 0 Then Warnings.Add($"{k_name}: tabelul {k_t.Name} nu are coloanele {String.Join(", ", k_absent)} (absente, nu NULL).")
            Next
            Dim k_dc As String = Nothing
            If k_p.Config.TryGetValue("UNIT.DC", k_dc) AndAlso k_dc <> "NULL" AndAlso k_dc.Trim().Length > 0 Then
                k_dcs.Add(k_dc.Trim())
            Else
                If TargetDc Is Nothing OrElse TargetDc.Trim().Length = 0 Then Blockers.Add($"{k_name}: UNIT.DC lipsește (nu se știe baza DC destinație); completați câmpul DC.")
            End If
        Next
        If k_dcs.Count > 1 AndAlso (TargetDc Is Nothing OrElse TargetDc.Trim().Length = 0) Then
            Blockers.Add($"Fișierele indică DC-uri diferite ({String.Join(", ", k_dcs)}); o subunitate are un singur DC.")
        ElseIf k_dcs.Count = 1 Then
            Dc = k_dcs.First()
        End If
        If TargetDc IsNot Nothing AndAlso TargetDc.Trim().Length > 0 Then
            If Dc.Length > 0 AndAlso Not String.Equals(Dc, TargetDc.Trim(), StringComparison.OrdinalIgnoreCase) Then
                Warnings.Add($"Baza destinație «{TargetDc.Trim()}» diferă de UNIT.DC din fișiere («{Dc}»): transferul se scrie în «{TargetDc.Trim()}».")
            End If
            Dc = TargetDc.Trim()
        End If
    End Sub

    Private Sub CheckAssociation()
        Dim k_types As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Dim k_years As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each k_file In Sources.GroupBy(Function(k_s) k_s.Source.Path, StringComparer.OrdinalIgnoreCase)
            Dim k_first = k_file.First()
            If k_first.Source.Kind <> VrcFileKind.Year Then Continue For
            Dim k_name = k_first.Profile.FileName
            For Each k_type In {"ALIMENTE", "MATERIALE"}
                If k_file.Where(Function(k_s) k_s.GestiuneType = k_type).Count() > 1 Then Blockers.Add($"{k_name}: tipul {k_type} apare de două ori; fiecare parte a fișierului se duce într-o singură gestiune.")
            Next
            Dim k_split = k_first.Profile.Split
            If k_split Is Nothing Then Continue For
            If k_split.HasAlimente AndAlso k_file.All(Function(k_s) k_s.GestiuneType <> "ALIMENTE") Then Blockers.Add($"{k_name}: fișierul are conturi de tip A / liste de alimente, dar planul nu are un rând ALIMENTE pentru el.")
            If k_split.HasMateriale AndAlso k_file.All(Function(k_s) k_s.GestiuneType <> "MATERIALE") Then Blockers.Add($"{k_name}: fișierul are conturi Materiale (M sau fără tip), dar planul nu are un rând MATERIALE pentru el.")
            If Not k_split.HasAlimente AndAlso k_file.Any(Function(k_s) k_s.GestiuneType = "ALIMENTE") Then Blockers.Add($"{k_name}: planul are un rând ALIMENTE, dar fișierul nu are conturi de tip A sau liste.")
        Next
        For Each k_s In Sources
            Dim k_name = k_s.Profile.FileName
            If k_s.GestiuneName Is Nothing OrElse k_s.GestiuneName.Trim().Length = 0 Then
                Blockers.Add($"{k_name}: alegeți gestiunea (denumirea).")
                Continue For
            End If
            If k_s.GestiuneName.Trim().Length > 100 Then Blockers.Add($"{k_name}: denumirea gestiunii poate avea cel mult 100 de caractere.")
            If Not GestiuneTypes.Contains(k_s.GestiuneType) Then
                Blockers.Add($"{k_name}: alegeți tipul gestiunii (MATERIALE, ALIMENTE, OI sau MF).")
                Continue For
            End If
            Dim k_isSpecial As Boolean = k_s.Source.Kind = VrcFileKind.Special
            If k_isSpecial AndAlso (k_s.GestiuneType = "MATERIALE" OrElse k_s.GestiuneType = "ALIMENTE") Then
                Blockers.Add($"{k_name}: baza specială tz_2100 este OI sau MF, nu {k_s.GestiuneType}.")
            End If
            If Not k_isSpecial AndAlso (k_s.GestiuneType = "OI" OrElse k_s.GestiuneType = "MF") Then
                Blockers.Add($"{k_name}: fișierul de an este Materiale sau Alimente, nu {k_s.GestiuneType}.")
            End If
            Dim k_flagType As String = If(Flag(k_s.Profile, "Setari.OBINV"), "OI", If(Flag(k_s.Profile, "Setari.MF"), "MF", String.Empty))
            If k_flagType.Length > 0 AndAlso k_flagType <> k_s.GestiuneType Then
                Blockers.Add($"{k_name}: steagurile din fișier spun {k_flagType}, dar planul spune {k_s.GestiuneType}.")
            ElseIf k_flagType.Length = 0 AndAlso (k_s.GestiuneType = "OI" OrElse k_s.GestiuneType = "MF") Then
                Blockers.Add($"{k_name}: planul spune {k_s.GestiuneType}, dar fișierul nu are steagul OBINV/MF.")
            End If
            If k_s.Year < 1990 OrElse k_s.Year > 2099 Then
                Blockers.Add($"{k_name}: alegeți anul exercițiului (între 1990 și 2099).")
            ElseIf k_s.Source.Kind = VrcFileKind.Year AndAlso k_s.Year <> k_s.Source.Year Then
                Warnings.Add($"{k_name}: anul exercițiului ({k_s.Year}) diferă de anul din numele fișierului ({k_s.Source.Year}).")
            End If

            Dim k_known As String = Nothing
            If k_types.TryGetValue(k_s.GestiuneName.Trim(), k_known) AndAlso k_known <> k_s.GestiuneType Then
                Blockers.Add($"Gestiunea «{k_s.GestiuneName.Trim()}» are tipuri diferite ({k_known} și {k_s.GestiuneType}).")
            End If
            k_types(k_s.GestiuneName.Trim()) = k_s.GestiuneType
            If Not k_years.Add($"{k_s.GestiuneName.Trim()}|{k_s.Year}") Then
                Blockers.Add($"Gestiunea «{k_s.GestiuneName.Trim()}» are două surse pentru anul {k_s.Year}.")
            End If
            If k_s.GestiuneType = "OI" OrElse k_s.GestiuneType = "MF" Then
                Dim k_same = Sources.Where(Function(k_o) String.Equals(k_o.GestiuneName, k_s.GestiuneName, StringComparison.OrdinalIgnoreCase)).Count()
                If k_same > 1 Then Blockers.Add($"Gestiunea «{k_s.GestiuneName.Trim()}» ({k_s.GestiuneType}) are o singură bază multianuală; nu se împarte pe ani.")
            End If
        Next
    End Sub

    ''' <summary>ZEC retains calculation/display precision; storage is always direct DECIMAL(20,4).</summary>
    Private Sub CheckPrecision()
        Dim k_zecs As New SortedSet(Of Integer)()
        Dim k_needed As Integer = 0
        For Each k_s In DistinctSources().Where(Function(k_x) k_x.Profile.ReadError.Length = 0)
            Dim k_zec As Integer = 4
            Dim k_raw As String = Nothing
            If k_s.Profile.Config.TryGetValue("NR.ZEC", k_raw) Then Integer.TryParse(k_raw, NumberStyles.Integer, CultureInfo.InvariantCulture, k_zec)
            k_zecs.Add(k_zec)
            For Each k_t In k_s.Profile.Tables
                For Each k_f In k_t.Fields.Where(Function(k_x) k_x.FieldClass = "VALOARE-INT" AndAlso k_x.NonNull > 0)
                    If k_f.MaxDecimals > 4 Then
                        Blockers.Add($"{k_s.Profile.FileName}: {k_t.Name}.{k_f.Name} are valori cu mai mult de 4 zecimale; scara maximă este 4.")
                    Else
                        k_needed = Math.Max(k_needed, k_f.MaxDecimals)
                    End If
                Next
            Next
        Next
        Zec = If(k_zecs.Count > 0, k_zecs.Max, 4)
        If k_zecs.Count > 1 Then Warnings.Add($"ZEC diferă între surse ({String.Join(", ", k_zecs)}); se alege cel mai mare ({Zec}) pentru toată subunitatea.")
        Scale = 4 ' Fixed database precision; independent of calculation/display ZEC.
        If Zec > 4 Then Blockers.Add($"ZEC {Zec} depășește scara maximă 4.")
        For Each k_s In DistinctSources().Where(Function(k_x) k_x.Profile.ReadError.Length = 0)
            For Each k_t In k_s.Profile.Tables
                For Each k_f In k_t.Fields.Where(Function(k_x) k_x.FieldClass = "VALOARE-INT" AndAlso k_x.NonNull > 0)
                    Dim k_max As Double = Math.Max(Math.Abs(k_f.Minimum), Math.Abs(k_f.Maximum))
                    If k_max >= DecimalLimit Then
                        Blockers.Add($"{k_s.Profile.FileName}: {k_t.Name}.{k_f.Name} (maxim {k_max.ToString("0.####", CultureInfo.InvariantCulture)}) nu încape în DECIMAL(20,4).")
                    End If
                Next
            Next
        Next
    End Sub

    ''' <summary>Findings of the reader that the decided rules turn into blockers (the ones that are only warnings are listed too).</summary>
    Private Sub CheckData()
        For Each k_s In DistinctSources().Where(Function(k_x) k_x.Profile.ReadError.Length = 0)
            Dim k_name = k_s.Profile.FileName
            Dim k_nir = k_s.Profile.Tables.FirstOrDefault(Function(k_t) String.Equals(k_t.Name, "NIR", StringComparison.OrdinalIgnoreCase))
            Dim k_number = k_nir?.Fields.FirstOrDefault(Function(k_f) k_f.Name = "Numar")
            If k_number IsNot Nothing AndAlso k_number.MaxDecimals > 2 Then Blockers.Add($"{k_name}: NIR.Numar are valori cu mai mult de 2 zecimale; NumarSub păstrează cel mult 2.")
            If k_number IsNot Nothing AndAlso k_number.ByDecimals.Skip(1).Sum() > 0 Then Warnings.Add($"{k_name}: {k_number.ByDecimals.Skip(1).Sum():N0} numere NIR au zecimale (document introdus între două numere): partea întreagă în Numar, fracția în NumarSub.")
            If k_s.Profile.Split IsNot Nothing Then
                If k_s.Profile.Split.Conflicts > 0 Then Blockers.Add($"{k_name}: {k_s.Profile.Split.Conflicts:N0} documente au linii și din conturi Alimente, și din conturi Materiale; corectați-le în sursă.")
                If k_s.Profile.Split.Unclassified > 0 Then Warnings.Add($"{k_name}: {k_s.Profile.Split.Unclassified:N0} documente fără cont și fără linii care să le plaseze; merg la Materiale.")
            End If
            For Each k_l In k_s.Profile.LineSigns
                If k_l.BothNegative > 0 Then Warnings.Add($"{k_name}: {k_l.BothNegative:N0} linii din {k_l.Table} au și cantitatea, și prețul negative; ambele se păstrează ca în Access.")
                If k_l.NegativePriceOnly > 0 Then Warnings.Add($"{k_name}: {k_l.NegativePriceOnly:N0} linii din {k_l.Table} au preț negativ; prețul și cantitatea se păstrează ca în Access, fără schimbarea semnelor.")
                If k_l.NegativeQuantityOnly > 0 Then Warnings.Add($"{k_name}: {k_l.NegativeQuantityOnly:N0} linii din {k_l.Table} au cantitate negativă (se păstrează).")
            Next
            If k_s.Profile.ProductCheck IsNot Nothing Then
                Dim k_c = k_s.Profile.ProductCheck
                If k_c.NegativeUsed > 0 Then Warnings.Add($"{k_name}: {k_c.NegativeUsed:N0} rânduri ProduseP cu PUTVA negativ sunt folosite în documente; valoarea se păstrează ca în Access.")
                If k_c.Total > k_c.Used Then Warnings.Add($"{k_name}: {(k_c.Total - k_c.Used):N0} din {k_c.Total:N0} rânduri ProduseP nu au folosire și nu se transferă.")
            End If
            For Each k_h In k_s.Profile.HeaderChecks
                If k_h.HeaderZero > 0 Then Warnings.Add($"{k_name}: {k_h.HeaderZero:N0} antete {k_h.HeaderTable} cu total 0; se păstrează 0, fără completare din linii.")
                If k_h.DiffOverOneLeu > 0 Then Warnings.Add($"{k_name}: {k_h.DiffOverOneLeu:N0} antete {k_h.HeaderTable} diferă cu mai mult de 1 leu de suma liniilor; se păstrează totalul antetului din Access, fără recalculare.")
                If k_h.HeaderNull > 0 Then Warnings.Add($"{k_name}: {k_h.HeaderNull:N0} antete {k_h.HeaderTable} cu total NULL; se păstrează NULL.")
            Next
        Next
    End Sub

    ' ---- hash and export ----------------------------------------------------------------------------------------------------
    ''' <summary>Canonical text of everything the transfer depends on: context, precision, rules and every source with its hash.</summary>
    Private Function Canonical() As String
        Dim k_sb As New StringBuilder()
        k_sb.Append("plan=").Append(PlanVersion).Append(";rules=").Append(RuleVersion).Append(";dc=").Append(Dc).Append(";subunit=").Append(SubunitName.Trim())
        k_sb.Append(";zec=").Append(Zec).Append(";scale=").Append(Scale).AppendLine()
        For Each k_s In Sources.OrderBy(Function(k_x) k_x.GestiuneName, StringComparer.OrdinalIgnoreCase).ThenBy(Function(k_x) k_x.Year).ThenBy(Function(k_x) k_x.Profile.FileName, StringComparer.OrdinalIgnoreCase)
            k_sb.Append(k_s.Profile.FileName).Append("|"c).Append(k_s.Profile.HashBefore).Append("|"c).Append(k_s.GestiuneName.Trim()).Append("|"c).Append(k_s.GestiuneType).Append("|"c).Append(k_s.Year).AppendLine()
        Next
        Return k_sb.ToString()
    End Function

    Public Function Hash() As String
        Using k_sha = SHA256.Create()
            Return Convert.ToHexString(k_sha.ComputeHash(Encoding.UTF8.GetBytes(Canonical())))
        End Using
    End Function

    ''' <summary>Exportable plan (no passwords, no personal data, file names only).</summary>
    Public Function ToJson() As String
        Try
            Dim k_sources As New List(Of Object)()
            For Each k_s In Sources
                k_sources.Add(New With {.file = k_s.Profile.FileName, .sha256 = k_s.Profile.HashBefore, .sizeBytes = k_s.Profile.SizeBytes,
                                        .gestiune = k_s.GestiuneName.Trim(), .type = k_s.GestiuneType, .year = k_s.Year})
            Next
            Dim k_doc = New With {
                .planVersion = PlanVersion, .ruleVersion = RuleVersion, .dc = Dc, .subunit = SubunitName.Trim(), .zec = Zec, .scale = Scale,
                .planHash = Hash(), .complete = IsComplete, .blockers = Blockers, .warnings = Warnings, .sources = k_sources}
            Return JsonSerializer.Serialize(k_doc, New JsonSerializerOptions() With {.WriteIndented = True, .Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping})
        Catch ex As Exception
            GlobalErrorLog.Write("VrcPlan.ToJson", ex)
            Throw
        End Try
    End Function

End Class
