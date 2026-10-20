
Imports KBot.Common
Imports MySqlConnector

''' <summary>One indicator of a supplier's angajament, as it will be written.</summary>
Friend NotInheritable Class PartnerLinkIndicator

    Public Sub New(k_codAI As String, k_codInd As String, k_contBancar As String)
        CodAI = k_codAI
        CodInd = k_codInd
        ContBancar = k_contBancar
    End Sub

    ''' <summary><c>CodAng-CodInd</c>, the key of <c>FX_Indicatori</c>.</summary>
    Public ReadOnly Property CodAI As String
    Public ReadOnly Property CodInd As String
    ''' <summary>Empty = none.</summary>
    Public Property ContBancar As String

End Class

''' <summary>One supplier &lt;-&gt; angajament link: the pair <c>(CodFiscal, CodAng)</c> and its indicators.</summary>
Friend NotInheritable Class PartnerLink

    Private ReadOnly _byCodAI As New Dictionary(Of String, PartnerLinkIndicator)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _indicators As New List(Of PartnerLinkIndicator)()

    Public Sub New(k_codFiscal As String, k_codAng As String)
        CodFiscal = k_codFiscal
        CodAng = k_codAng
    End Sub

    ''' <summary>The normalized CUI (<see cref="EfCui.Normalize"/>).</summary>
    Public ReadOnly Property CodFiscal As String
    Public ReadOnly Property CodAng As String

    Public ReadOnly Property Indicators As IReadOnlyList(Of PartnerLinkIndicator)
        Get
            Return _indicators
        End Get
    End Property

    Public Function Find(k_codAI As String) As PartnerLinkIndicator
        Dim k_found As PartnerLinkIndicator = Nothing
        _byCodAI.TryGetValue(k_codAI, k_found)
        Return k_found
    End Function

    Public Sub AddIndicator(k_indicator As PartnerLinkIndicator)
        _byCodAI(k_indicator.CodAI) = k_indicator
        _indicators.Add(k_indicator)
    End Sub

End Class

''' <summary>A partner code, in one unit, with the number of <c>ParteneriAng</c> rows hanging on it.</summary>
Friend NotInheritable Class PartnerRowCount

    Public Sub New(k_idUnitate As Integer, k_codPartener As String, k_denumire As String)
        IdUnitate = k_idUnitate
        CodPartener = k_codPartener
        Denumire = k_denumire
    End Sub

    Public ReadOnly Property IdUnitate As Integer
    Public ReadOnly Property CodPartener As String
    Public ReadOnly Property Denumire As String
    Public Property Rows As Integer

    Public Overrides Function ToString() As String
        Dim k_name = If(Denumire.Length = 0, String.Empty, " " & Denumire)
        Return $"unitatea {IdUnitate}, «{CodPartener}»{k_name} ({Rows} rânduri)"
    End Function

End Class

''' <summary>
''' What the Access table <c>ParteneriAng</c> becomes (slice 0121-02): the links keyed by
''' the supplier's normalized CodFiscal, plus everything the report has to say about the
''' rows that did not make it, or were merged.
''' </summary>
''' <remarks>
''' Built by <see cref="PartnerLinkReader.Read"/>, used twice and never edited in between: the
''' verifier measures it, <see cref="TransferRunner"/> writes it. Nothing is lost silently -
''' every Access row ends in exactly one of: a link or indicator written, merged into one
''' already there, or one of the skipped lists below.
''' </remarks>
Friend NotInheritable Class PartnerLinkSet

    Private ReadOnly _byKey As New Dictionary(Of String, PartnerLink)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _links As New List(Of PartnerLink)()

    ''' <summary>The links in the order they were first met.</summary>
    Public ReadOnly Property Links As IReadOnlyList(Of PartnerLink)
        Get
            Return _links
        End Get
    End Property

    Public Function IndicatorCount() As Integer
        Dim k_total = 0
        For Each k_link In _links
            k_total += k_link.Indicators.Count
        Next
        Return k_total
    End Function

    ''' <summary>The link for the pair, created when it is new.</summary>
    Public Function GetOrAdd(k_codFiscal As String, k_codAng As String, ByRef k_isNew As Boolean) As PartnerLink
        Dim k_key = k_codFiscal & "|" & k_codAng
        Dim k_found As PartnerLink = Nothing
        If _byKey.TryGetValue(k_key, k_found) Then
            k_isNew = False
            Return k_found
        End If
        k_found = New PartnerLink(k_codFiscal, k_codAng)
        _byKey(k_key) = k_found
        _links.Add(k_found)
        k_isNew = True
        Return k_found
    End Function

    ' ---- counts ------------------------------------------------------------------

    ''' <summary>Rows read from <c>ParteneriAng</c>, over all the ticked units.</summary>
    Public Property RowsRead As Long
    ''' <summary>Units whose file has no <c>ParteneriAng</c> table at all.</summary>
    Public ReadOnly Property UnitsWithoutTable As New List(Of Integer)()

    ' ---- rows that did not travel (each is listed, none is lost) -------------------

    ''' <summary>Partners with no usable CodFiscal (empty, or nothing left after normalization).</summary>
    Public ReadOnly Property PartnersWithoutCodFiscal As New List(Of PartnerRowCount)()
    ''' <summary>Partner codes of <c>ParteneriAng</c> that are not in the unit's own <c>Parteneri</c>.</summary>
    Public ReadOnly Property PartnersNotFound As New List(Of PartnerRowCount)()
    ''' <summary>Rows with a partner that has a CodFiscal but no <c>CodAng</c>: no link can be made.</summary>
    Public ReadOnly Property RowsWithoutCodAng As New List(Of PartnerRowCount)()

    ' ---- rows that travelled in a special way ----------------------------------------

    ''' <summary>Rows with a <c>CodAng</c> but no <c>CodInd</c>: the link is written, no indicator.</summary>
    Public Property RowsWithoutCodInd As Long
    ''' <summary>Rows that repeated a pair (link, <c>CodAI</c>) already read: merged into it.</summary>
    Public Property DuplicatesMerged As Long
    ''' <summary>Same link and <c>CodAI</c>, two different non-empty bank accounts. The first one stays.</summary>
    Public ReadOnly Property BankAccountConflicts As New List(Of String)()
    ''' <summary>Normalized CodFiscal values that are not all digits (CNP, foreign VAT ids): raw ▸ normalized.</summary>
    Public ReadOnly Property NonNumericCodFiscal As New SortedDictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    ''' <summary>
    ''' Same CodFiscal and same classification text in two or more units with different <c>CodAng</c>
    ''' (the operator believes it never happens; the report shows whether it does).
    ''' </summary>
    Public ReadOnly Property CrossUnitConflicts As New List(Of String)()

End Class

''' <summary>
''' Reads <c>ParteneriAng</c> (with <c>Parteneri</c>, for the CodFiscal) out of the ticked units' Access
''' files and shapes it into a <see cref="PartnerLinkSet"/>; also reads which <c>CodAI</c> exist.
''' </summary>
''' <remarks>
''' The supplier's CodFiscal is read from the unit's own Access <c>Parteneri</c> (by <c>CodPartener</c>),
''' NOT from the target: the partner does not have to exist in the unit on the server (slice 0121).
''' </remarks>
Friend NotInheritable Class PartnerLinkReader

    Private Sub New()
    End Sub

    Public Const AccessTable As String = "ParteneriAng"

    ''' <summary>Reads every ticked unit. Access I/O: logs and rethrows.</summary>
    Public Shared Function Read(k_request As TransferRequest) As PartnerLinkSet
        Try
            Dim k_result As New PartnerLinkSet()
            ' CodFiscal ▸ classification text ▸ unit ▸ CodAng set, for the cross-unit report.
            Dim k_seen As New Dictionary(Of String, Dictionary(Of Integer, HashSet(Of String)))(StringComparer.OrdinalIgnoreCase)

            For Each k_unit In k_request.Units
                If Not k_unit.HasUnitFile Then Continue For
                ReadUnit(k_request, k_unit, k_result, k_seen)
            Next

            For Each k_entry In k_seen.OrderBy(Function(e) e.Key, StringComparer.OrdinalIgnoreCase)
                Dim k_allCodes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                For Each k_unitCodes In k_entry.Value.Values
                    k_allCodes.UnionWith(k_unitCodes)
                Next
                If k_entry.Value.Count < 2 OrElse k_allCodes.Count < 2 Then Continue For

                Dim k_cut = k_entry.Key.IndexOf("|"c)
                Dim k_parts = String.Join("; ", k_entry.Value.OrderBy(Function(u) u.Key).
                    Select(Function(u) $"unitatea {u.Key}: {String.Join(", ", u.Value.OrderBy(Function(c) c, StringComparer.OrdinalIgnoreCase))}"))
                k_result.CrossUnitConflicts.Add(
                    $"CodFiscal {k_entry.Key.Substring(0, k_cut)}, clasificația «{k_entry.Key.Substring(k_cut + 1)}» — {k_parts}")
            Next

            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("PartnerLinkReader.Read", ex)
            Throw
        End Try
    End Function

    Private Shared Sub ReadUnit(k_request As TransferRequest, k_unit As CaiUnit, k_result As PartnerLinkSet,
                                k_seen As Dictionary(Of String, Dictionary(Of Integer, HashSet(Of String))))
        Using k_cn = AccessProvider.Open(k_unit.UnitFilePath, k_request.UnitFilePassword)
            Dim k_partnerTable = AccessSchema.ResolveTableName(k_cn, "Parteneri")
            Dim k_linkTable = AccessSchema.ResolveTableName(k_cn, AccessTable)
            If k_linkTable Is Nothing Then
                k_result.UnitsWithoutTable.Add(k_unit.IdUnitate)
                Return
            End If

            ' CodPartener ▸ (raw CodFiscal, name), of THIS unit's file.
            Dim k_partners As New Dictionary(Of String, (CodFiscal As String, Denumire As String))(StringComparer.OrdinalIgnoreCase)
            If k_partnerTable IsNot Nothing Then
                Using k_reader = AccessSchema.OpenReader(k_cn, k_partnerTable)
                    While k_reader.Read()
                        Dim k_code = Verifier.AsText(k_reader.ValueOrMissing("CodPartener")).Trim()
                        If k_code.Length = 0 Then Continue While
                        Dim k_name = Verifier.AsText(k_reader.ValueOrMissing("DenumirePartener")).Trim()
                        If k_name.Length = 0 Then k_name = Verifier.AsText(k_reader.ValueOrMissing("NumePartener")).Trim()
                        k_partners(k_code) = (Verifier.AsText(k_reader.ValueOrMissing("CodFiscal")), k_name)
                    End While
                End Using
            End If

            Dim k_noCui As New Dictionary(Of String, PartnerRowCount)(StringComparer.OrdinalIgnoreCase)
            Dim k_notFound As New Dictionary(Of String, PartnerRowCount)(StringComparer.OrdinalIgnoreCase)
            Dim k_noAng As New Dictionary(Of String, PartnerRowCount)(StringComparer.OrdinalIgnoreCase)

            Using k_reader = AccessSchema.OpenReader(k_cn, k_linkTable)
                While k_reader.Read()
                    k_result.RowsRead += 1

                    Dim k_code = Verifier.AsText(k_reader.ValueOrMissing("CodPartener")).Trim()
                    Dim k_codAng = Verifier.AsText(k_reader.ValueOrMissing("CodAng")).Trim()
                    Dim k_codInd = Verifier.AsText(k_reader.ValueOrMissing("CodInd")).Trim()
                    Dim k_bank = Verifier.AsText(k_reader.ValueOrMissing("ContBanca")).Trim()
                    Dim k_clsf = Verifier.AsText(k_reader.ValueOrMissing("Clsf")).Trim()

                    Dim k_partner As (CodFiscal As String, Denumire As String) = Nothing
                    If k_code.Length = 0 OrElse Not k_partners.TryGetValue(k_code, k_partner) Then
                        Tally(k_notFound, k_unit.IdUnitate, k_code, String.Empty)
                        Continue While
                    End If

                    Dim k_cui = EfCui.Normalize(k_partner.CodFiscal)
                    If k_cui.Length = 0 Then
                        Tally(k_noCui, k_unit.IdUnitate, k_code, k_partner.Denumire)
                        Continue While
                    End If

                    If k_codAng.Length = 0 Then
                        Tally(k_noAng, k_unit.IdUnitate, k_code, k_partner.Denumire)
                        Continue While
                    End If

                    If Not k_cui.All(Function(c) Char.IsDigit(c)) AndAlso Not k_result.NonNumericCodFiscal.ContainsKey(k_cui) Then
                        k_result.NonNumericCodFiscal(k_cui) = k_partner.CodFiscal.Trim()
                    End If

                    Dim k_isNew As Boolean
                    Dim k_link = k_result.GetOrAdd(k_cui, k_codAng, k_isNew)

                    If k_clsf.Length > 0 Then
                        Dim k_seenKey = k_cui & "|" & k_clsf
                        Dim k_units As Dictionary(Of Integer, HashSet(Of String)) = Nothing
                        If Not k_seen.TryGetValue(k_seenKey, k_units) Then
                            k_units = New Dictionary(Of Integer, HashSet(Of String))()
                            k_seen(k_seenKey) = k_units
                        End If
                        Dim k_codes As HashSet(Of String) = Nothing
                        If Not k_units.TryGetValue(k_unit.IdUnitate, k_codes) Then
                            k_codes = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                            k_units(k_unit.IdUnitate) = k_codes
                        End If
                        k_codes.Add(k_codAng)
                    End If

                    If k_codInd.Length = 0 Then
                        k_result.RowsWithoutCodInd += 1
                        If Not k_isNew Then k_result.DuplicatesMerged += 1
                        Continue While
                    End If

                    Dim k_codAI = k_codAng & "-" & k_codInd
                    Dim k_existing = k_link.Find(k_codAI)
                    If k_existing Is Nothing Then
                        k_link.AddIndicator(New PartnerLinkIndicator(k_codAI, k_codInd, k_bank))
                        Continue While
                    End If

                    k_result.DuplicatesMerged += 1
                    If k_bank.Length = 0 Then Continue While
                    If k_existing.ContBancar.Length = 0 Then
                        k_existing.ContBancar = k_bank
                    ElseIf Not String.Equals(k_existing.ContBancar, k_bank, StringComparison.OrdinalIgnoreCase) Then
                        k_result.BankAccountConflicts.Add(
                            $"CodFiscal {k_cui}, {k_codAI}: «{k_existing.ContBancar}» rămâne, «{k_bank}» (unitatea {k_unit.IdUnitate}) nu se scrie")
                    End If
                End While
            End Using

            k_result.PartnersWithoutCodFiscal.AddRange(k_noCui.Values)
            k_result.PartnersNotFound.AddRange(k_notFound.Values)
            k_result.RowsWithoutCodAng.AddRange(k_noAng.Values)
        End Using
    End Sub

    Private Shared Sub Tally(k_target As Dictionary(Of String, PartnerRowCount), k_idUnitate As Integer,
                             k_code As String, k_denumire As String)
        Dim k_found As PartnerRowCount = Nothing
        If Not k_target.TryGetValue(k_code, k_found) Then
            k_found = New PartnerRowCount(k_idUnitate, k_code, k_denumire)
            k_target(k_code) = k_found
        End If
        k_found.Rows += 1
    End Sub

    ''' <summary>
    ''' Every <c>CodAI</c> the run can know: the ones already in the target's <c>FX_Indicatori</c> plus the ones in
    ''' the ticked units' FOREXE files (the same run writes them). Empty when neither source exists.
    ''' </summary>
    Public Shared Function ReadKnownIndicators(k_request As TransferRequest, k_cn As MySqlConnection,
                                               k_schema As TargetSchema) As HashSet(Of String)
        Try
            Dim k_known As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            If k_schema.HasTable("FX_Indicatori") Then
                Using k_cmd As New MySqlCommand("SELECT `CodAI` FROM `FX_Indicatori`", k_cn)
                    Using k_reader = k_cmd.ExecuteReader()
                        While k_reader.Read()
                            If Not k_reader.IsDBNull(0) Then k_known.Add(k_reader.GetString(0).Trim())
                        End While
                    End Using
                End Using
            End If

            Dim k_done As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each k_unit In k_request.Units
                Dim k_path = k_unit.ForexeFilePath
                If String.IsNullOrEmpty(k_path) OrElse Not IO.File.Exists(k_path) Then Continue For
                If Not k_done.Add(k_path) Then Continue For

                Using k_access = AccessProvider.Open(k_path, k_request.ForexeFilePassword)
                    Dim k_table = AccessSchema.ResolveTableName(k_access, "FX_Indicatori")
                    If k_table Is Nothing Then Continue For
                    Using k_reader = AccessSchema.OpenReader(k_access, k_table)
                        While k_reader.Read()
                            Dim k_codAI = Verifier.AsText(k_reader.ValueOrMissing("CodAI")).Trim()
                            If k_codAI.Length > 0 Then k_known.Add(k_codAI)
                        End While
                    End Using
                End Using
            Next

            Return k_known
        Catch ex As Exception
            GlobalErrorLog.Write("PartnerLinkReader.ReadKnownIndicators", ex)
            Throw
        End Try
    End Function

End Class
