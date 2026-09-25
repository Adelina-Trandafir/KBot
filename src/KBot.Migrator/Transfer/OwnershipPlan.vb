Imports System.Data.OleDb
Imports System.Globalization
Imports KBot.Common

''' <summary>Which subtree an FX_* row hangs from, if any.</summary>
''' <remarks>
''' Only these five families need a subtree at all. Every other FX_* table either carries
''' its own <c>IdUnitate</c> or reaches its unit through the foreign keys
''' <see cref="TransferRunner"/> already follows.
''' </remarks>
Public Enum Subtree
    ''' <summary>Not part of a routed family - the generic rule answers for it.</summary>
    None = 0
    ''' <summary>The FX_DDF tree, keyed on IDDF.</summary>
    Ddf = 1
    ''' <summary>The FX_ORD tree, keyed on IDORD.</summary>
    Ord = 2
    ''' <summary>An account statement FILE, keyed on IDEXF.</summary>
    ExtrasFile = 3
    ''' <summary>One statement HEADER inside a file, keyed on IDEXH.</summary>
    ExtrasHeader = 4
    ''' <summary>
    ''' An angajament and its history, keyed on CodAngajament; its units come from
    ''' <c>FX_Indicatori.IdUnitate</c> (slice 0080-01, operator decision 24.09.2026).
    ''' </summary>
    Angajament = 5
End Enum

''' <summary>What happens to one Access row.</summary>
''' <remarks>
''' The last two are DIFFERENT facts and the operator has to be able to tell them apart:
''' "it belongs to another unit" is the normal shape of a shared file, while "its document
''' stayed behind" says a whole subtree was held back and names why elsewhere. Counting
''' them together is what made <c>FX_DDF</c> report "altă unitate 14" for rows that were
''' really being dropped on a relic column.
''' </remarks>
Public Enum RowDisposition
    Travels = 0
    ''' <summary>Its unit is not one of the selected ones. Silent, by design.</summary>
    OtherUnit = 1
    ''' <summary>Its document, order or statement file did not travel.</summary>
    SubtreeStayedBehind = 2
End Enum

''' <summary>What the plan says about one row: whether it travels, and whose it is.</summary>
Public NotInheritable Class RowVerdict

    Private Sub New(disposition As RowDisposition, scope As UnitScope,
                    hasUnit As Boolean, idUnitate As Integer)
        Me.Disposition = disposition
        Me.Scope = scope
        Me.HasUnit = hasUnit
        Me.IdUnitate = idUnitate
    End Sub

    Public ReadOnly Property Disposition As RowDisposition
    Public ReadOnly Property Scope As UnitScope
    ''' <summary>True when exactly one unit owns this row, so a nomenclator can be picked.</summary>
    Public ReadOnly Property HasUnit As Boolean
    ''' <summary>Meaningful only while <see cref="HasUnit"/> is True.</summary>
    Public ReadOnly Property IdUnitate As Integer

    Public Shared Function Named(idUnitate As Integer) As RowVerdict
        Return New RowVerdict(RowDisposition.Travels, UnitScope.Named, True, idUnitate)
    End Function

    ''' <summary>Travels, and serves several units at once - a DDF or an ORD header.</summary>
    Public Shared Function Shared1() As RowVerdict
        Return New RowVerdict(RowDisposition.Travels, UnitScope.SharedByMany, False, 0)
    End Function

    ''' <summary>Travels; the table has no unit column and the parents answer for it.</summary>
    Public Shared Function ParentScoped() As RowVerdict
        Return New RowVerdict(RowDisposition.Travels, UnitScope.ParentScoped, False, 0)
    End Function

    Public Shared Function OtherUnit() As RowVerdict
        Return New RowVerdict(RowDisposition.OtherUnit, UnitScope.Named, False, 0)
    End Function

    Public Shared Function SubtreeStayedBehind() As RowVerdict
        Return New RowVerdict(RowDisposition.SubtreeStayedBehind, UnitScope.Named, False, 0)
    End Function

End Class

''' <summary>
''' Which rows travel, decided BEFORE the first row is written.
''' </summary>
''' <remarks>
''' <para>
''' <b>Why it cannot be answered as the run goes.</b> A DDF's unit lives in
''' <c>FX_DDF_REV_SA</c> and an ORD's in <c>FX_ORD_TBL</c> - tables written AFTER their
''' head, because the foreign key points that way. <see cref="WrittenKeys"/> fills as the
''' pass runs and therefore cannot answer "does this document travel?" at the moment
''' <c>FX_DDF</c> is being written. So the selection is resolved once, in a separate read
''' of the Access file, and reused unchanged - the same reason the Python side builds a
''' routing plan before it writes. A selection that could shift between measuring and
''' writing is the defect this shape exists to prevent.
''' </para>
''' <para>
''' <b>The ownership arrow points UP from the children.</b> Slice 0045-07 had it the other
''' way: SA asked FX_DDF who it belonged to. That reading died with decision D1 -
''' <c>FX_DDF.IdUnitate</c> is a relic, never read, and one IDDF can serve many units. The
''' authority is now <c>FX_DDF_REV_SA.IdUnitate</c> and nothing else (D2), and
''' <c>FX_ORD_TBL.IdUnitate</c> and nothing else (D3).
''' </para>
''' <para>
''' <b>Measured on the operator's live file, 24.08.</b> Nine DDF rows, of which five carry
''' a unit through their section A (30, 31, 32 and 64 ▸ unit 75; 33 ▸ unit 76) and four
''' carry none at all (73, 77, 79, 80 - their single SA row has <c>IdUnitate</c> NULL).
''' Under D5 those four keep their whole subtree behind. Seventeen ORD rows, every one of
''' them unit 76. Seventy-two statement files, every one carrying exactly one all-digit
''' name segment, <c>2842919</c>, which is the CodFiscal the registry holds for
''' <c>000_DEMO</c>.
''' </para>
''' </remarks>
Public NotInheritable Class OwnershipPlan

    ''' <summary>The only table that says which units a DDF serves (D2).</summary>
    Public Const DdfAuthorityTable As String = "FX_DDF_REV_SA"

    ''' <summary>The only table that says which units an ORD serves (D3).</summary>
    Public Const OrdAuthorityTable As String = "FX_ORD_TBL"

    ''' <summary>
    ''' The only table that says which units an angajament serves (slice 0080-01).
    ''' </summary>
    ''' <remarks>
    ''' Operator, 24.09.2026: <c>FX_Angajamente.IdUnitate</c> is NOT usable, and one
    ''' <c>FX_2026.accdb</c> can hold two DCs. <c>FX_Istoric</c> and <c>FX_Rezervari</c> have
    ''' no unit column at all, so until this they travelled whole - the other DC's history
    ''' included, with <c>CodAI</c> / <c>CodAngajament</c> blanked because their parents
    ''' stayed behind (014_SCSV run: 704 + 68 such rows).
    ''' </remarks>
    Public Const AngajamentAuthorityTable As String = "FX_Indicatori"

    ''' <summary>
    ''' <c>FX_Angajamente.DC</c>: the database the angajament was downloaded for. The
    ''' tiebreaker for an angajament with no indicator unit.
    ''' </summary>
    Public Const DcColumn As String = "DC"

    Private ReadOnly _selected As HashSet(Of Integer)
    Private ReadOnly _knownInCai As HashSet(Of Integer)
    Private ReadOnly _codFiscal As String
    Private ReadOnly _findings As New List(Of Finding)()

    ' key ▸ the units that key serves. Keys are normalised text so an Integer 30 and a
    ' Long 30 are one key, exactly as WrittenKeys does it.
    Private ReadOnly _ddfUnits As New Dictionary(Of String, HashSet(Of Integer))(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _ordUnits As New Dictionary(Of String, HashSet(Of Integer))(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _travellingExtraseF As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _travellingExtraseH As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _angajamentUnits As New Dictionary(Of String, HashSet(Of Integer))(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _indicatorUnit As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    ' Angajamente with no indicator unit whose FX_Angajamente.DC is the target database:
    ' not fully downloaded, so they exist only in FX_Angajamente. They travel into that
    ' table alone (operator, 25.09.2026).
    Private ReadOnly _dcOnlyAngajamente As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _targetDatabase As String

    Private _ddfRead As Boolean
    Private _ordRead As Boolean
    Private _extraseRead As Boolean
    Private _angajamentRead As Boolean

    Private Sub New(selected As HashSet(Of Integer), knownInCai As HashSet(Of Integer),
                    codFiscal As String, targetDatabase As String)
        _selected = selected
        _knownInCai = knownInCai
        _codFiscal = codFiscal
        _targetDatabase = If(targetDatabase, String.Empty).Trim()
    End Sub

    ''' <summary>Findings raised while the plan was built. Raised ONCE, never per row.</summary>
    Public ReadOnly Property Findings As IReadOnlyList(Of Finding)
        Get
            Return _findings
        End Get
    End Property

    ''' <summary>The CodFiscal the statement files are matched against.</summary>
    Public ReadOnly Property CodFiscal As String
        Get
            Return _codFiscal
        End Get
    End Property

    ' Where(...).Count() rather than Count(...): on a Dictionary's ValueCollection, VB binds
    ' the bare Count to the COLLECTION's own property and then fails to index it, which is
    ' a confusing error for what looks like an ordinary LINQ call.
    Public ReadOnly Property TravellingDdfCount As Integer
        Get
            Return _ddfUnits.Values.Where(Function(u) u.Overlaps(_selected)).Count()
        End Get
    End Property

    Public ReadOnly Property TravellingOrdCount As Integer
        Get
            Return _ordUnits.Values.Where(Function(u) u.Overlaps(_selected)).Count()
        End Get
    End Property

    Public ReadOnly Property TravellingAngajamentCount As Integer
        Get
            Return _angajamentUnits.Values.Where(Function(u) u.Overlaps(_selected)).Count() +
                   _dcOnlyAngajamente.Count
        End Get
    End Property

    Public ReadOnly Property TravellingExtraseFileCount As Integer
        Get
            Return _travellingExtraseF.Count
        End Get
    End Property

    ''' <summary>
    ''' Reads the three authority tables of every Forexe file in the request and resolves
    ''' the whole selection.
    ''' </summary>
    ''' <remarks>
    ''' Built ONCE per run and handed to both <see cref="Verifier"/> and
    ''' <see cref="TransferRunner"/>. Two construction sites would be the 0045-07 mistake
    ''' again: one rule asked in two places answers differently sooner or later.
    ''' </remarks>
    Public Shared Function Build(request As TransferRequest, log As Action(Of String)) As OwnershipPlan
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))

        Try
            Dim selected As New HashSet(Of Integer)(request.Units.Select(Function(u) u.IdUnitate))
            Dim known As New HashSet(Of Integer)(request.KnownUnitIds())
            Dim codFiscal = request.ResolvedCodFiscal()

            Dim plan As New OwnershipPlan(selected, known, codFiscal, request.TargetDatabase)

            If codFiscal.Length = 0 Then
                ' D15: silence here would look exactly like "this file holds no statements
                ' of ours", which is the wrong answer arrived at without a word.
                plan._findings.Add(New Finding(
                    Finding.COD_FISCAL_LIPSA, FindingClass.Blocant, "FX_Extrase_F", "NumeFisier",
                    $"Codul fiscal al DC-ului «{request.TargetDatabase}» nu a fost găsit în " &
                    "registrul Windows («HKCU\Software\VB and VBA Program Settings\AVACONT\" &
                    $"{request.TargetDatabase}\CodFiscal»), iar caseta de suprascriere e goală. " &
                    "Extrasele se aleg după codul fiscal din numele fișierului, deci fără el " &
                    "nu s-ar potrivi niciunul — și asta ar arăta exact ca «fișierul nu are " &
                    "extrase pentru noi»."))
            End If

            For Each file In DistinctForexeFiles(request)
                Using cn = AccessProvider.Open(file, request.ForexeFilePassword)
                    plan.ReadDdfAuthority(cn)
                    plan.ReadOrdAuthority(cn)
                    plan.ReadExtrase(cn)
                    plan.ReadAngajamentAuthority(cn)
                End Using
            Next

            plan.Say(log)
            Return plan

        Catch ex As Exception
            GlobalErrorLog.Write("OwnershipPlan.Build", ex)
            Throw
        End Try
    End Function

    ''' <summary>Every distinct Forexe file the selected units point at.</summary>
    ''' <remarks>
    ''' Eleven of the thirteen <c>cai</c> rows name the SAME file, so this is one pass in
    ''' practice - but the registry allows several and the plan must not assume one.
    ''' </remarks>
    Private Shared Function DistinctForexeFiles(request As TransferRequest) As List(Of String)
        Dim files As New List(Of String)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each unit In request.Units
            If Not unit.HasForexeFile Then Continue For
            If seen.Add(unit.ForexeFilePath) Then files.Add(unit.ForexeFilePath)
        Next
        Return files
    End Function

    ' ---- reading the authorities -------------------------------------------------------

    ''' <summary>
    ''' <c>FX_DDF_REV_SA</c> ▸ which units each IDDF serves, plus the two data conditions.
    ''' </summary>
    Private Sub ReadDdfAuthority(cn As OleDbConnection)
        Dim realName = AccessSchema.ResolveTableName(cn, DdfAuthorityTable)
        If realName Is Nothing Then Return
        _ddfRead = True

        Dim nullUnit As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim unknownUnits As New HashSet(Of Integer)()

        Using reader = AccessSchema.OpenKeyReader(cn, realName, {"IDDF", UnitOwnership.UnitColumn})
            While reader.Read()
                Dim key = Normalise(reader.ValueOrMissing("IDDF"))
                If key.Length = 0 Then Continue While

                Dim owner = Verifier.AsInteger(reader.ValueOrMissing(UnitOwnership.UnitColumn))
                If Not owner.HasValue Then
                    nullUnit.Add(key)
                    Continue While
                End If

                If Not _knownInCai.Contains(owner.Value) Then unknownUnits.Add(owner.Value)
                Units(_ddfUnits, key).Add(owner.Value)
            End While
        End Using

        RaiseUnknownUnits(unknownUnits, DdfAuthorityTable)

        ' D5: a document whose section A names no unit stays behind, and so does everything
        ' under it. Only reported for the documents that have NO named unit at all - a
        ' document with one NULL line and one named line still travels on the named one.
        Dim stranded = nullUnit.Where(Function(k) Not _ddfUnits.ContainsKey(k)).OrderBy(Function(k) k).ToList()
        If stranded.Count > 0 Then
            _findings.Add(New Finding(
                Finding.UNITATE_NEDETERMINATA, FindingClass.Atentie, "FX_DDF", UnitOwnership.UnitColumn,
                $"{stranded.Count} documente au «{DdfAuthorityTable}.IdUnitate» gol pe toate " &
                $"liniile lor de secțiune A (IDDF: {String.Join(", ", stranded.Take(20))}), deci " &
                "nu se poate spune cărei unități aparțin. Documentul și tot ce atârnă de el " &
                "— revizii, secțiuni A și B, ordonanțări — rămân în Access. Datele din Access " &
                "trebuie completate; unealta nu ghicește unitatea.", stranded.Count))
        End If

        ' D6: impossible by the operator's account. An impossible case that passes quietly
        ' is how bad data arrives.
        Dim headRealName = AccessSchema.ResolveTableName(cn, "FX_DDF")
        If headRealName Is Nothing Then Return

        Dim without As New List(Of String)()
        Using reader = AccessSchema.OpenKeyReader(cn, headRealName, {"IDDF"})
            While reader.Read()
                Dim key = Normalise(reader.ValueOrMissing("IDDF"))
                If key.Length = 0 Then Continue While
                If _ddfUnits.ContainsKey(key) OrElse nullUnit.Contains(key) Then Continue While
                without.Add(key)
            End While
        End Using

        If without.Count > 0 Then
            _findings.Add(New Finding(
                Finding.DDF_FARA_SECTIUNE_A, FindingClass.Blocant, "FX_DDF", "IDDF",
                $"{without.Count} documente nu au NICIO linie în «{DdfAuthorityTable}» " &
                $"(IDDF: {String.Join(", ", without.Take(20))}). Operatorul spune că e " &
                "imposibil, deci fișierul Access nu arată cum ar trebui și rularea se " &
                "oprește aici, în loc să treacă tăcut peste el.", without.Count))
        End If
    End Sub

    ''' <summary><c>FX_ORD_TBL</c> ▸ which units each IDORD serves.</summary>
    Private Sub ReadOrdAuthority(cn As OleDbConnection)
        Dim realName = AccessSchema.ResolveTableName(cn, OrdAuthorityTable)
        If realName Is Nothing Then Return
        _ordRead = True

        Dim nullUnit As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim unknownUnits As New HashSet(Of Integer)()

        Using reader = AccessSchema.OpenKeyReader(cn, realName, {"IDORD", UnitOwnership.UnitColumn})
            While reader.Read()
                Dim key = Normalise(reader.ValueOrMissing("IDORD"))
                If key.Length = 0 Then Continue While

                Dim owner = Verifier.AsInteger(reader.ValueOrMissing(UnitOwnership.UnitColumn))
                If Not owner.HasValue Then
                    nullUnit.Add(key)
                    Continue While
                End If

                If Not _knownInCai.Contains(owner.Value) Then unknownUnits.Add(owner.Value)
                Units(_ordUnits, key).Add(owner.Value)
            End While
        End Using

        RaiseUnknownUnits(unknownUnits, OrdAuthorityTable)

        Dim stranded = nullUnit.Where(Function(k) Not _ordUnits.ContainsKey(k)).OrderBy(Function(k) k).ToList()
        If stranded.Count = 0 Then Return

        _findings.Add(New Finding(
            Finding.UNITATE_NEDETERMINATA, FindingClass.Atentie, "FX_ORD", UnitOwnership.UnitColumn,
            $"{stranded.Count} ordonanțări au «{OrdAuthorityTable}.IdUnitate» gol pe toate " &
            $"liniile lor (IDORD: {String.Join(", ", stranded.Take(20))}), deci nu se poate " &
            "spune cărei unități aparțin. Ordonanțarea și liniile ei rămân în Access.",
            stranded.Count))
    End Sub

    ''' <summary>
    ''' <c>FX_Indicatori</c> ▸ which units each CodAngajament serves, plus the data
    ''' conditions: an angajament with no indicator unit, and history with no angajament.
    ''' </summary>
    Private Sub ReadAngajamentAuthority(cn As OleDbConnection)
        Dim realName = AccessSchema.ResolveTableName(cn, AngajamentAuthorityTable)
        If realName Is Nothing Then Return
        _angajamentRead = True

        Using reader = AccessSchema.OpenKeyReader(cn, realName, {"CodAI", "CodAngajament", UnitOwnership.UnitColumn})
            While reader.Read()
                Dim owner = Verifier.AsInteger(reader.ValueOrMissing(UnitOwnership.UnitColumn))
                If Not owner.HasValue Then Continue While
                Dim codAi = Normalise(reader.ValueOrMissing("CodAI")).Trim()
                If codAi.Length > 0 Then _indicatorUnit(codAi) = owner.Value
                Dim key = Normalise(reader.ValueOrMissing("CodAngajament")).Trim()
                If key.Length = 0 Then Continue While
                Units(_angajamentUnits, key).Add(owner.Value)
            End While
        End Using

        ' An angajament whose indicators name no unit was not fully downloaded: it exists
        ' only in FX_Angajamente. The DC column breaks the tie (operator, 25.09.2026): when
        ' it names the target database the angajament travels, into FX_Angajamente only;
        ' another DC's stays behind as that DC's. With no DC it cannot be placed - said
        ' once, with the codes.
        Dim headName = AccessSchema.ResolveTableName(cn, "FX_Angajamente")
        If headName IsNot Nothing Then
            Dim hasDc = AccessSchema.Columns(cn, headName).Any(
                Function(c) String.Equals(c.Name, DcColumn, StringComparison.OrdinalIgnoreCase))
            Dim columns = If(hasDc, {"CodAngajament", DcColumn}, {"CodAngajament"})
            Dim stranded As New List(Of String)()
            Using reader = AccessSchema.OpenKeyReader(cn, headName, columns)
                While reader.Read()
                    Dim key = Normalise(reader.ValueOrMissing("CodAngajament")).Trim()
                    If key.Length = 0 OrElse _angajamentUnits.ContainsKey(key) Then Continue While
                    Dim dc = If(hasDc, Normalise(reader.ValueOrMissing(DcColumn)).Trim(), String.Empty)
                    If dc.Length = 0 Then
                        stranded.Add(key)
                    ElseIf _targetDatabase.Length > 0 AndAlso
                           String.Equals(dc, _targetDatabase, StringComparison.OrdinalIgnoreCase) Then
                        _dcOnlyAngajamente.Add(key)
                    End If
                End While
            End Using
            If stranded.Count > 0 Then
                _findings.Add(New Finding(
                    Finding.UNITATE_NEDETERMINATA, FindingClass.Atentie, "FX_Angajamente", "CodAngajament",
                    $"{stranded.Count} angajamente nu au niciun indicator cu «IdUnitate» în " &
                    $"«{AngajamentAuthorityTable}» și nici «{DcColumn}» completat " &
                    $"({String.Join(", ", stranded.Take(20))}), deci nu se poate spune cărei " &
                    "unități aparțin. Angajamentul, istoricul și rezervările lui rămân în Access.",
                    stranded.Count))
            End If
        End If

        ' An angajament placed only by its DC goes into FX_Angajamente alone. History or a
        ' reservation hanging from one is not expected; if there is any, it stays behind
        ' and is said, never dropped quietly.
        If _dcOnlyAngajamente.Count > 0 Then
            For Each table In {"FX_Istoric", "FX_Rezervari"}
                Dim left = CountHangingFrom(cn, table, _dcOnlyAngajamente)
                If left = 0 Then Continue For
                _findings.Add(New Finding(
                    Finding.UNITATE_NEDETERMINATA, FindingClass.Atentie, table, "CodAngajament",
                    $"{left} rânduri din «{table}» aparțin unor angajamente fără indicatori, " &
                    $"trimise doar în «FX_Angajamente» după «{DcColumn}». Rândurile acestea " &
                    "rămân în Access.", left))
            Next
        End If

        ' Operator, 24.09.2026: history or a reservation without CodAngajament is impossible -
        ' a critical rule. So it blocks, rather than the row staying behind in silence.
        For Each table In {"FX_Istoric", "FX_Rezervari"}
            Dim missing = CountWithoutAngajament(cn, table)
            If missing = 0 Then Continue For
            _findings.Add(New Finding(
                Finding.ANGAJAMENT_LIPSA, FindingClass.Blocant, table, "CodAngajament",
                $"{missing} rânduri din «{table}» nu au «CodAngajament». Regula spune că " &
                "nu pot exista; fișierul Access trebuie corectat înainte de transfer.", missing))
        Next
    End Sub

    Private Shared Function CountHangingFrom(cn As OleDbConnection, table As String,
                                             keys As HashSet(Of String)) As Integer
        Dim realName = AccessSchema.ResolveTableName(cn, table)
        If realName Is Nothing Then Return 0
        Dim count = 0
        Using reader = AccessSchema.OpenKeyReader(cn, realName, {"CodAngajament"})
            While reader.Read()
                If keys.Contains(Normalise(reader.ValueOrMissing("CodAngajament")).Trim()) Then count += 1
            End While
        End Using
        Return count
    End Function

    Private Shared Function CountWithoutAngajament(cn As OleDbConnection, table As String) As Integer
        Dim realName = AccessSchema.ResolveTableName(cn, table)
        If realName Is Nothing Then Return 0
        Dim count = 0
        Using reader = AccessSchema.OpenKeyReader(cn, realName, {"CodAngajament"})
            While reader.Read()
                If Normalise(reader.ValueOrMissing("CodAngajament")).Trim().Length = 0 Then count += 1
            End While
        End Using
        Return count
    End Function

    ''' <summary>
    ''' The statement files whose name carries the DC's CodFiscal, and their headers.
    ''' </summary>
    ''' <remarks>
    ''' D8: split the name on '_' and take the file when ANY segment that is entirely
    ''' digits equals the CodFiscal. No positional rule and no digging inside a segment -
    ''' a name with an extra piece in front still works, and the trailing
    ''' <c>03062026h1717.pdf</c> cannot collide because it is not all digits.
    ''' D11: a file that travels takes its headers and their lines with it, untested
    ''' further. <c>FX_Extrase_H.IdUnitate</c> is NOT consulted (D10) - it travels for the
    ''' operator's information and decides nothing, which is why unit 77 in it raises
    ''' nothing any more.
    ''' </remarks>
    Private Sub ReadExtrase(cn As OleDbConnection)
        If _codFiscal.Length = 0 Then Return

        Dim fileTable = AccessSchema.ResolveTableName(cn, "FX_Extrase_F")
        If fileTable Is Nothing Then Return
        _extraseRead = True

        Dim seen = 0
        Using reader = AccessSchema.OpenKeyReader(cn, fileTable, {"IDEXF", "NumeFisier"})
            While reader.Read()
                Dim key = Normalise(reader.ValueOrMissing("IDEXF"))
                If key.Length = 0 Then Continue While
                seen += 1
                If Not NameCarriesCodFiscal(Verifier.AsText(reader.ValueOrMissing("NumeFisier")), _codFiscal) Then Continue While
                _travellingExtraseF.Add(key)
            End While
        End Using

        ' The same reasoning as D15, one step further along: a CodFiscal that matches NOTHING
        ' looks exactly like "this file has no extrase for us", and the operator cannot tell
        ' the two apart from an empty result. Atenție rather than Blocant - a file genuinely
        ' holding another unit's statements is a real shape, and the operator may have typed
        ' the override precisely to exclude them.
        If seen > 0 AndAlso _travellingExtraseF.Count = 0 Then
            _findings.Add(New Finding(
                Finding.COD_FISCAL_LIPSA, FindingClass.Atentie, "FX_Extrase_F", "NumeFisier",
                $"Niciunul dintre cele {seen} fișiere de extras nu poartă codul fiscal " &
                $"«{_codFiscal}» în nume, deci nu pleacă niciun extras. Dacă unitatea " &
                "chiar are extrase, codul fiscal folosit e greșit — verificați caseta " &
                "«Cod fiscal» și valoarea din registry.", seen))
        End If

        Dim headerTable = AccessSchema.ResolveTableName(cn, "FX_Extrase_H")
        If headerTable Is Nothing Then Return

        Using reader = AccessSchema.OpenKeyReader(cn, headerTable, {"IDEXH", "IDEXF"})
            While reader.Read()
                Dim parent = Normalise(reader.ValueOrMissing("IDEXF"))
                If Not _travellingExtraseF.Contains(parent) Then Continue While
                Dim key = Normalise(reader.ValueOrMissing("IDEXH"))
                If key.Length > 0 Then _travellingExtraseH.Add(key)
            End While
        End Using
    End Sub

    ''' <summary>
    ''' True when any all-digit segment of the file name equals the CodFiscal (D8).
    ''' </summary>
    ''' <remarks>
    ''' Public because it is the whole of decision D8 in one pure function, with no state
    ''' and no file behind it - the one piece of this class that can be checked by itself,
    ''' and the first thing a test project should pin down. Equality, not "contains": a
    ''' code with a digit appended or a leading zero is a DIFFERENT unit, not a match.
    ''' </remarks>
    Public Shared Function NameCarriesCodFiscal(fileName As String, codFiscal As String) As Boolean
        If String.IsNullOrEmpty(fileName) OrElse String.IsNullOrEmpty(codFiscal) Then Return False
        For Each segment In fileName.Split("_"c)
            If segment.Length = 0 Then Continue For
            If Not segment.All(AddressOf Char.IsDigit) Then Continue For
            If String.Equals(segment, codFiscal, StringComparison.Ordinal) Then Return True
        Next
        Return False
    End Function

    Private Sub RaiseUnknownUnits(units As HashSet(Of Integer), authorityTable As String)
        If units.Count = 0 Then Return
        ' D7 case C: the file and the registry disagree, and neither can be guessed past.
        ' Case B - a unit of ANOTHER DC, present in cai - is the normal shape of a shared
        ' file and is skipped silently, which is why this asks the whole registry rather
        ' than the selected units.
        _findings.Add(New Finding(
            Finding.UNITATE_NECUNOSCUTA, FindingClass.Blocant, authorityTable, UnitOwnership.UnitColumn,
            $"«{authorityTable}» conține unitățile {String.Join(", ", units.OrderBy(Function(u) u))}, " &
            "care nu apar în NICIUN rând din registrul «cai» — nici la acest DC, nici la altul. " &
            "Fișierul și registrul se contrazic, iar unealta nu are cum să aleagă între ele.",
            units.Count))
    End Sub

    ' ---- the one question both the verifier and the writer ask ---------------------------

    ''' <summary>
    ''' Does this row travel, and whose is it?
    ''' </summary>
    ''' <param name="passUnits">
    ''' The units sharing the Access file being read. Exactly one for a nomenclator file,
    ''' where the rows carry no <c>IdUnitate</c> of their own and the FILE is the unit.
    ''' </param>
    ''' <remarks>
    ''' Asked identically by <see cref="Verifier"/> and <see cref="TransferRunner"/>. Two
    ''' copies of this rule drifting apart is exactly the defect slice 0045-07 was written
    ''' to remove, and it stays removed only while there is one function.
    ''' </remarks>
    Public Function Decide(map As TableMap, reader As AccessTableReader,
                           passUnits As IReadOnlyList(Of CaiUnit)) As RowVerdict
        If map Is Nothing Then Throw New ArgumentNullException(NameOf(map))
        If reader Is Nothing Then Throw New ArgumentNullException(NameOf(reader))

        Dim route = RouteFor(map.AccessTable)
        If route IsNot Nothing Then Return DecideRouted(route, reader)

        ' --- the generic rule ------------------------------------------------------------
        Dim rowUnit As Integer
        Select Case UnitOwnership.Resolve(reader, rowUnit)
            Case UnitScope.Named
                If Not _selected.Contains(rowUnit) Then Return RowVerdict.OtherUnit()
                Return RowVerdict.Named(rowUnit)

            Case UnitScope.ParentScoped
                ' No IdUnitate column at all, so the unit can only come from the FILE - and
                ' only a NOMENCLATOR file has one. Access Clasificatii and Parteneri carry
                ' no IdUnitate column because each unit has its own baza2026.accdb; there,
                ' the file genuinely is the unit.
                '
                ' The Forexe file is NOT, no matter how many units happen to be ticked.
                ' Keying this on passUnits.Count alone (as this first did) made 3.246
                ' FX_Istoric rows read as unit 76's whenever 76 was the only tick, and
                ' correctly refuse when two units were ticked - an answer that changes with
                ' the tick list is the 0045-07 defect wearing a different hat. Nothing
                ' consumes it today, because every table in this state travels by name
                ' match; the day one gains a resolved IdClsf it would have resolved 3.246
                ' rows against one unit's nomenclator without a word.
                If map.Source = SourceFile.UnitFile AndAlso
                   passUnits IsNot Nothing AndAlso passUnits.Count = 1 Then
                    Return RowVerdict.Named(passUnits(0).IdUnitate)
                End If
                Return RowVerdict.ParentScoped()

            Case Else
                Throw New TransferException(
                    $"«{map.TargetTable}»: un rând are «IdUnitate» gol și nicio autoritate " &
                    "care să spună cărei unități îi aparține. Un fișier FOREXE ține " &
                    "rândurile tuturor unităților din DC, deci rândul NU poate fi atribuit " &
                    "unei unități alese la întâmplare.")
        End Select
    End Function

    ''' <summary>
    ''' The unit whose nomenclator a row's classification is resolved on (slice 0080-01), or
    ''' Nothing when none can be named.
    ''' </summary>
    ''' <remarks>
    ''' The same order as <c>PYTHON/utils/clsf_pair.py</c>: the row's own <c>IdUnitate</c>
    ''' (for <c>FX_Extrase_H</c> that column is the operator's information for ownership,
    ''' D10, but it IS the unit of the header's account); else the unit the ownership plan
    ''' gave the row (<c>FX_Istoric</c> / <c>FX_Rezervari</c>: their angajament's); else the
    ''' unit of the row's indicator (<c>CodAI</c> in Access <c>FX_Indicatori</c>). Asked by
    ''' the verifier's dry run and by the writer alike.
    ''' </remarks>
    Public Function ClassificationUnit(reader As AccessTableReader, verdict As RowVerdict) As Integer?
        If reader Is Nothing Then Throw New ArgumentNullException(NameOf(reader))
        If reader.HasColumn(UnitOwnership.UnitColumn) Then
            Dim own = Verifier.AsInteger(reader.ValueOrMissing(UnitOwnership.UnitColumn))
            If own.HasValue AndAlso own.Value <> 0 Then Return own
        End If
        If verdict IsNot Nothing AndAlso verdict.HasUnit Then Return verdict.IdUnitate
        If reader.HasColumn("CodAI") Then
            Dim codAi = Normalise(reader.ValueOrMissing("CodAI")).Trim()
            Dim unit As Integer
            If codAi.Length > 0 AndAlso _indicatorUnit.TryGetValue(codAi, unit) Then Return unit
        End If
        Return Nothing
    End Function

    Private Function DecideRouted(route As SubtreeRoute, reader As AccessTableReader) As RowVerdict
        Dim key = Normalise(reader.ValueOrMissing(route.KeyColumn))
        If route.Subtree = Subtree.Angajament Then Return DecideAngajament(route.AccessTable, key.Trim())
        If Not SubtreeTravels(route.Subtree, key) Then Return RowVerdict.SubtreeStayedBehind()

        ' The head and its descriptive tables serve every unit the subtree serves; only the
        ' money lines name one (D12). SA and SB write their OWN IdUnitate now - the pass has
        ' no unit to lend them any more, and would have lent the wrong one anyway.
        If String.IsNullOrEmpty(route.OwnUnitColumn) Then Return RowVerdict.Shared1()

        Dim own = Verifier.AsInteger(reader.ValueOrMissing(route.OwnUnitColumn))
        If Not own.HasValue Then Return RowVerdict.SubtreeStayedBehind()
        If Not _selected.Contains(own.Value) Then Return RowVerdict.OtherUnit()
        Return RowVerdict.Named(own.Value)
    End Function

    ''' <summary>
    ''' An angajament row, or a history / reservation row hanging from one: it travels when
    ''' one of its indicators is in a ticked unit, and it is that unit's when there is one.
    ''' </summary>
    ''' <remarks>
    ''' An angajament whose indicators are all in unticked units is another unit's (or the
    ''' other DC's) and is counted as such. One with no indicator unit travels into
    ''' <c>FX_Angajamente</c> alone when its <c>DC</c> is the target database; otherwise it
    ''' stays behind. <see cref="ReadAngajamentAuthority"/> has already said so.
    ''' </remarks>
    Private Function DecideAngajament(accessTable As String, key As String) As RowVerdict
        If Not _angajamentRead Then
            Throw New TransferException(
                $"«{AngajamentAuthorityTable}» lipsește din fișierul FOREXE, deci nu se poate " &
                "spune cărei unități aparține un angajament. Unealta nu ghicește.")
        End If
        If key.Length = 0 Then Return RowVerdict.SubtreeStayedBehind()
        Dim units As HashSet(Of Integer) = Nothing
        If Not _angajamentUnits.TryGetValue(key, units) Then
            If _dcOnlyAngajamente.Contains(key) AndAlso
               String.Equals(accessTable, "FX_Angajamente", StringComparison.OrdinalIgnoreCase) Then
                Return RowVerdict.Shared1()
            End If
            Return RowVerdict.SubtreeStayedBehind()
        End If
        Dim ours = units.Where(Function(u) _selected.Contains(u)).ToList()
        If ours.Count = 0 Then Return RowVerdict.OtherUnit()
        If ours.Count = 1 Then Return RowVerdict.Named(ours(0))
        Return RowVerdict.Shared1()
    End Function

    Private Function SubtreeTravels(subtree1 As Subtree, key As String) As Boolean
        If key.Length = 0 Then Return False
        Select Case subtree1
            Case Subtree.Ddf
                ' A family never read - no such table in this file - must not silently hold
                ' back every row of it. The generic rule cannot answer either, so the rows
                ' travel and ParentsTravelled remains the backstop.
                If Not _ddfRead Then Return True
                Dim units As HashSet(Of Integer) = Nothing
                Return _ddfUnits.TryGetValue(key, units) AndAlso units.Overlaps(_selected)
            Case Subtree.Ord
                If Not _ordRead Then Return True
                Dim units As HashSet(Of Integer) = Nothing
                Return _ordUnits.TryGetValue(key, units) AndAlso units.Overlaps(_selected)
            Case Subtree.ExtrasFile
                If Not _extraseRead Then Return _codFiscal.Length = 0
                Return _travellingExtraseF.Contains(key)
            Case Subtree.ExtrasHeader
                If Not _extraseRead Then Return _codFiscal.Length = 0
                Return _travellingExtraseH.Contains(key)
            Case Else
                Return True
        End Select
    End Function

    ' ---- the routed families ------------------------------------------------------------

    ''' <summary>
    ''' The three routed families, table by table.
    ''' </summary>
    ''' <remarks>
    ''' A short explicit table rather than something inferred from the foreign keys: which
    ''' column names the subtree is a fact about the ACCESS file, and the target's keys have
    ''' been renamed underneath it (IDORD ▸ IDORDP). Everything not named here falls to the
    ''' generic rule, which is the right answer for the other seventeen FX_* tables.
    ''' </remarks>
    Private Shared ReadOnly Routes As IReadOnlyList(Of SubtreeRoute) = New SubtreeRoute() {
        New SubtreeRoute("FX_DDF", Subtree.Ddf, "IDDF", Nothing),
        New SubtreeRoute("FX_DDF_REV", Subtree.Ddf, "IDDF", Nothing),
        New SubtreeRoute("FX_DDF_REV_SA", Subtree.Ddf, "IDDF", UnitOwnership.UnitColumn),
        New SubtreeRoute("FX_DDF_REV_SB", Subtree.Ddf, "IDDF", UnitOwnership.UnitColumn),
        New SubtreeRoute("FX_ORD", Subtree.Ord, "IDORD", Nothing),
        New SubtreeRoute("FX_ORD_PART", Subtree.Ord, "IDORD", Nothing),
        New SubtreeRoute("FX_ORD_DOC", Subtree.Ord, "IDORD", Nothing),
        New SubtreeRoute("FX_ORD_TBL", Subtree.Ord, "IDORD", UnitOwnership.UnitColumn),
        New SubtreeRoute("FX_Extrase_F", Subtree.ExtrasFile, "IDEXF", Nothing),
        New SubtreeRoute("FX_Extrase_H", Subtree.ExtrasFile, "IDEXF", Nothing),
        New SubtreeRoute("FX_Extrase", Subtree.ExtrasHeader, "IDFXH", Nothing),
        New SubtreeRoute("FX_Angajamente", Subtree.Angajament, "CodAngajament", Nothing),
        New SubtreeRoute("FX_Istoric", Subtree.Angajament, "CodAngajament", Nothing),
        New SubtreeRoute("FX_Rezervari", Subtree.Angajament, "CodAngajament", Nothing)
    }

    Private Shared Function RouteFor(accessTable As String) As SubtreeRoute
        If String.IsNullOrEmpty(accessTable) Then Return Nothing
        Return Routes.FirstOrDefault(
            Function(r) String.Equals(r.AccessTable, accessTable, StringComparison.OrdinalIgnoreCase))
    End Function

    ' ---- helpers --------------------------------------------------------------------------

    Private Shared Function Units(map As Dictionary(Of String, HashSet(Of Integer)),
                                  key As String) As HashSet(Of Integer)
        Dim set1 As HashSet(Of Integer) = Nothing
        If Not map.TryGetValue(key, set1) Then
            set1 = New HashSet(Of Integer)()
            map(key) = set1
        End If
        Return set1
    End Function

    ''' <summary>One canonical text form per key, so an Integer 30 and a Long 30 are one.</summary>
    Private Shared Function Normalise(value As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then Return String.Empty
        Dim formattable = TryCast(value, IFormattable)
        If formattable IsNot Nothing Then Return formattable.ToString(Nothing, CultureInfo.InvariantCulture)
        Return Convert.ToString(value, CultureInfo.InvariantCulture)
    End Function

    Private Sub Say(log As Action(Of String))
        If log Is Nothing Then Return
        log($"Proprietatea rândurilor, hotărâtă înainte de prima scriere: " &
            $"{TravellingDdfCount} din {_ddfUnits.Count} documente cu unitate cunoscută pleacă, " &
            $"{TravellingOrdCount} din {_ordUnits.Count} ordonanțări, " &
            $"{TravellingAngajamentCount} din {_angajamentUnits.Count} angajamente, " &
            $"{TravellingExtraseFileCount} fișiere de extras (cod fiscal " &
            $"«{If(_codFiscal.Length = 0, "lipsă", _codFiscal)}»).")
    End Sub

End Class

''' <summary>One FX_* table, and the key that says which subtree its rows hang from.</summary>
Friend NotInheritable Class SubtreeRoute

    Public Sub New(accessTable As String, subtree1 As Subtree, keyColumn As String,
                   ownUnitColumn As String)
        Me.AccessTable = accessTable
        Me.Subtree = subtree1
        Me.KeyColumn = keyColumn
        Me.OwnUnitColumn = ownUnitColumn
    End Sub

    Public ReadOnly Property AccessTable As String
    Public ReadOnly Property Subtree As Subtree
    ''' <summary>The Access column carrying the subtree key on THIS table.</summary>
    Public ReadOnly Property KeyColumn As String
    ''' <summary>
    ''' The column naming this row's own unit, or Nothing when the row serves every unit
    ''' its subtree serves.
    ''' </summary>
    Public ReadOnly Property OwnUnitColumn As String

End Class
