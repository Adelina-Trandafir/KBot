Imports System.Globalization
Imports KBot.Common
Imports MySqlConnector

''' <summary>A received invoice that travels in this run, as the linking step needs it.</summary>
Friend NotInheritable Class EfLinkInvoice

    Public Property IdPrimita As Integer
    Public Property NrFact As String = String.Empty
    ''' <summary>Tax code of the supplier, already normalised (<see cref="EfCui"/>).</summary>
    Public Property Cui As String = String.Empty
    ''' <summary>The payable total WITH the invoice sign (a credit note is negative), the number the manual screen calls «Total».</summary>
    Public Property Total As Decimal
    Public Property DataFact As DateTime?

End Class

''' <summary>One automatic link: the whole invoice on one classification of one existing reception.</summary>
Friend NotInheritable Class EfLinkProposal

    Public Property IdPrimita As Integer
    Public Property NrFact As String = String.Empty
    Public Property CodAngajament As String = String.Empty
    Public Property IdClsf As Integer
    Public Property IdReceptie As Integer
    Public Property Suma As Decimal

End Class

''' <summary>
''' Slice 00EF-38. Links the received invoices that travel in the E-Factura import to the RECEPTIONS that already exist in the unit
''' database, the way the accountant does by hand on the invoice's «Recepții» view (slices KE01..KE04), but only when the match is certain.
''' </summary>
''' <remarks>
''' <para>
''' What the manual system writes, and so what this writes: <c>EF_PrimiteLinii.IdClsf</c> (the classification of every line) and one row of
''' <c>EF_PrimiteReceptii</c> (type «existenta») naming the angajament, the classification, the reception and the amount WITH VAT.
''' </para>
''' <para>
''' The angajamente an invoice may be linked to are those the manual screen offers first: the angajamente of the DDFs whose partner has the
''' supplier's tax code (<c>FX_DDF_Parteneri.CodFiscal</c>, and the header partner <c>FX_DDF.CodFiscal</c>, which the DDF editor also keeps as a row
''' of that table), both compared by <see cref="EfCui"/>. An invoice whose supplier has no DDF is never linked.
''' </para>
''' <para>
''' Certainty, in this order: (1) the TOTAL: a reception of those angajamente whose uncovered amount (value of its last counting snapshot
''' minus what other links already allocate) equals the invoice total, with the same sign; exactly one such reception = linked.
''' (2) Several with the same amount: the MONTH of the invoice date against the month of the reception date; exactly one in that month = linked.
''' Anything else stays for the accountant: no reception, several, a reception with more than one classification (the invoice lines carry none, so
''' the split would be a guess), or two invoices that want the same reception (both left alone).
''' </para>
''' <para>
''' Only invoices that THIS run adds are looked at, so an invoice that was already in the target (and may have been linked or unlinked by hand since)
''' is never touched, and a repeated import changes nothing.
''' </para>
''' </remarks>
Friend NotInheritable Class EfReceptionLinker

    Private Const Tolerance As Decimal = 0.005D
    Private Const Area As String = "EF_PrimiteReceptii"
    Private Const LinkedBy As String = "migrare"

    Private Shared ReadOnly RequiredTables As String() = {
        "EF_Primite", "EF_PrimiteLinii", "EF_PrimiteReceptii", "FX_DDF", "FX_Receptii_R", "FX_Receptii_H", "FX_Receptii", "FX_Indicatori"}

    Private ReadOnly _cn As MySqlConnection
    Private ReadOnly _tx As MySqlTransaction
    Private ReadOnly _say As Action(Of String)

    Public Sub New(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_say As Action(Of String))
        If k_cn Is Nothing Then Throw New ArgumentNullException(NameOf(k_cn))
        _cn = k_cn
        _tx = k_tx
        _say = If(k_say, Sub(k_text As String)
                         End Sub)
    End Sub

    ' ---- one reception, as the matching sees it ---------------------------------------------------------------------

    Private NotInheritable Class ReceptionInfo
        Public Property IdReceptie As Integer
        Public Property Cod As String = String.Empty
        Public Property DataR As DateTime?
        ''' <summary>Value of the last counting snapshot per classification (IdClsf).</summary>
        Public ReadOnly Property Values As New Dictionary(Of Integer, Decimal)()
        ''' <summary>What is still uncovered per classification (value minus the amounts other links allocate); only the non-zero ones.</summary>
        Public ReadOnly Property Rest As New Dictionary(Of Integer, Decimal)()

        Public ReadOnly Property RestTotal As Decimal
            Get
                Return Rest.Values.Sum()
            End Get
        End Property
    End Class

    ' ---- reading and deciding (writes nothing) -----------------------------------------------------------------------

    ''' <summary>Decides which invoices can be linked, and to what. Reads the receptions, writes nothing.</summary>
    Public Function Propose(k_invoices As List(Of EfLinkInvoice), k_report As EfImportReport, k_plan As EfTablePlan) As List(Of EfLinkProposal)
        Dim k_proposals As New List(Of EfLinkProposal)()
        Try
            k_plan.RowsRead = k_invoices.Count
            If k_invoices.Count = 0 Then Return k_proposals

            Dim k_missing = RequiredTables.Where(Function(k_table) Not TableExists(k_table)).ToList()
            If k_missing.Count = 0 AndAlso Not ColumnExists("EF_PrimiteLinii", "IdClsf") Then k_missing.Add("EF_PrimiteLinii.IdClsf")
            If k_missing.Count > 0 Then
                k_report.Add(EfSeverity.Warning, Area,
                    $"Asocierea automată cu recepțiile este omisă: în baza «{CurrentDatabase()}» lipsesc {String.Join(", ", k_missing)}. " &
                    "Aplicați sql/KE_01_primite_receptii.sql pe sursă și transferați recepțiile, apoi legați facturile din K-BOT.")
                _say("EF_PrimiteReceptii: asocierea automată omisă (lipsesc tabele sau coloane).")
                Return k_proposals
            End If

            Dim k_suppliers = LoadSupplierAngajamente()
            Dim k_byAngajament As Dictionary(Of String, List(Of ReceptionInfo)) = LoadReceptionsByAngajament()

            Dim k_noSupplier As Integer = 0
            Dim k_noMatch As Integer = 0
            Dim k_ambiguous As Integer = 0
            Dim k_multiClass As Integer = 0
            Dim k_chosen As New List(Of (Invoice As EfLinkInvoice, Reception As ReceptionInfo))()

            For Each k_invoice In k_invoices.OrderBy(Function(k_i) k_i.DataFact).ThenBy(Function(k_i) k_i.IdPrimita)
                Dim k_cods As HashSet(Of String) = Nothing
                If k_invoice.Total = 0D OrElse Not k_suppliers.TryGetValue(k_invoice.Cui, k_cods) Then
                    k_noSupplier += 1
                    Continue For
                End If

                ' (1) the total
                Dim k_candidates As New List(Of ReceptionInfo)()
                For Each k_cod In k_cods
                    Dim k_list As List(Of ReceptionInfo) = Nothing
                    If Not k_byAngajament.TryGetValue(k_cod, k_list) Then Continue For
                    For Each k_reception In k_list
                        If k_reception.Rest.Count > 0 AndAlso
                           Math.Sign(k_reception.RestTotal) = Math.Sign(k_invoice.Total) AndAlso
                           Math.Abs(k_reception.RestTotal - k_invoice.Total) <= Tolerance Then k_candidates.Add(k_reception)
                    Next
                Next
                If k_candidates.Count = 0 Then
                    k_noMatch += 1
                    Continue For
                End If

                ' (2) several with the same amount: the month of the invoice
                Dim k_pick As ReceptionInfo = Nothing
                If k_candidates.Count = 1 Then
                    k_pick = k_candidates(0)
                ElseIf k_invoice.DataFact.HasValue Then
                    Dim k_month = k_invoice.DataFact.Value
                    Dim k_sameMonth = k_candidates.Where(Function(k_r) k_r.DataR.HasValue AndAlso
                                                                         k_r.DataR.Value.Year = k_month.Year AndAlso
                                                                         k_r.DataR.Value.Month = k_month.Month).ToList()
                    If k_sameMonth.Count = 1 Then k_pick = k_sameMonth(0)
                End If
                If k_pick Is Nothing Then
                    k_ambiguous += 1
                    k_report.AddLimited(EfSeverity.Warning, Area, "recepție nesigură",
                        $"factura {Describe(k_invoice)}: {k_candidates.Count} recepții cu aceeași valoare ({Money(k_invoice.Total)}), " &
                        "fără una singură în luna facturii. Nu s-a legat; legați din K-BOT.")
                    Continue For
                End If
                If k_pick.Rest.Count <> 1 Then
                    k_multiClass += 1
                    k_report.AddLimited(EfSeverity.Warning, Area, "recepție cu mai multe clasificații",
                        $"factura {Describe(k_invoice)}: recepția {k_pick.IdReceptie} (angajamentul {k_pick.Cod}) are valoarea potrivită, dar pe " &
                        $"{k_pick.Rest.Count} clasificații. Nu s-a legat; legați din K-BOT.")
                    Continue For
                End If
                k_chosen.Add((k_invoice, k_pick))
            Next

            ' two invoices that want the same reception: neither is certain
            Dim k_conflicts As Integer = 0
            For Each k_group In k_chosen.GroupBy(Function(k_c) k_c.Reception.IdReceptie)
                If k_group.Count() > 1 Then
                    k_conflicts += k_group.Count()
                    k_report.AddLimited(EfSeverity.Warning, Area, "recepție dorită de mai multe facturi",
                        $"recepția {k_group.Key}: o vor {k_group.Count()} facturi ({String.Join(", ", k_group.Select(Function(k_c) Describe(k_c.Invoice)))}). " &
                        "Nu s-a legat niciuna; legați din K-BOT.")
                    Continue For
                End If
                Dim k_one = k_group.First()
                k_proposals.Add(New EfLinkProposal With {
                    .IdPrimita = k_one.Invoice.IdPrimita, .NrFact = k_one.Invoice.NrFact, .CodAngajament = k_one.Reception.Cod,
                    .IdClsf = k_one.Reception.Rest.Keys.First(), .IdReceptie = k_one.Reception.IdReceptie, .Suma = k_one.Invoice.Total})
                k_report.AddLimited(EfSeverity.Info, Area, "legat automat",
                    $"factura {Describe(k_one.Invoice)} -> recepția {k_one.Reception.IdReceptie}, angajamentul {k_one.Reception.Cod}, " &
                    $"clasificația {k_one.Reception.Rest.Keys.First()}, suma {Money(k_one.Invoice.Total)}.")
            Next

            k_plan.RowsSelected = k_proposals.Count
            k_plan.RowsLeftOut = k_invoices.Count - k_proposals.Count
            _say($"EF_PrimiteReceptii: din {k_invoices.Count} facturi primite noi, {k_proposals.Count} legate automat de o recepție existentă; " &
                 $"{k_noSupplier} fără angajament al furnizorului, {k_noMatch} fără recepție de aceeași valoare, {k_ambiguous} nesigure, " &
                 $"{k_multiClass} cu recepție pe mai multe clasificații, {k_conflicts} cu recepție dorită și de altă factură.")
            Return k_proposals
        Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
            GlobalErrorLog.Write("EfReceptionLinker.Propose", ex)
            Throw
        End Try
    End Function

    ''' <summary>Angajamente per supplier tax code (normalised): the DDF partners and the DDF header partner.</summary>
    Private Function LoadSupplierAngajamente() As Dictionary(Of String, HashSet(Of String))
        Dim k_result As New Dictionary(Of String, HashSet(Of String))(StringComparer.Ordinal)
        Dim k_sql As String = "SELECT d.CodFiscal, d.CodAngajament FROM FX_DDF d WHERE d.CodFiscal IS NOT NULL"
        If TableExists("FX_DDF_Parteneri") Then
            k_sql &= " UNION SELECT p.CodFiscal, d.CodAngajament FROM FX_DDF_Parteneri p JOIN FX_DDF d ON d.IDDF = p.IDDF"
        End If
        Using k_cmd = _cn.CreateCommand()
            k_cmd.Transaction = _tx
            k_cmd.CommandText = k_sql
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    If k_reader.IsDBNull(0) OrElse k_reader.IsDBNull(1) Then Continue While
                    Dim k_cui = EfCui.Normalize(k_reader.GetString(0))
                    If k_cui.Length = 0 Then Continue While
                    Dim k_set As HashSet(Of String) = Nothing
                    If Not k_result.TryGetValue(k_cui, k_set) Then
                        k_set = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                        k_result(k_cui) = k_set
                    End If
                    k_set.Add(k_reader.GetString(1))
                End While
            End Using
        End Using
        Return k_result
    End Function

    ''' <summary>
    ''' The live receptions with their value per classification: the sum of the lines of the LAST counting snapshot, the rule of the receptions
    ''' view and of the manual linking (a deleted reception has none), minus what existing links already allocate.
    ''' </summary>
    Private Function LoadReceptionsByAngajament() As Dictionary(Of String, List(Of ReceptionInfo))
        Dim k_receptions As New Dictionary(Of Integer, ReceptionInfo)()
        Using k_cmd = _cn.CreateCommand()
            k_cmd.Transaction = _tx
            k_cmd.CommandTimeout = 300
            k_cmd.CommandText =
                "SELECT R.IDRR, R.CodAngajament, R.DataR, I.IdClsf, SUM(Rc.Valoare) AS Valoare " &
                "FROM FX_Receptii_R R " &
                "JOIN FX_Receptii_H H ON H.IDRH = (" &
                "  SELECT H2.IDRH FROM FX_Receptii_H H2 WHERE H2.IDRR = R.IDRR AND COALESCE(H2.Sters, 0) = 0 " &
                "  AND (COALESCE(H2.EsteStergere, 0) <> 0 OR EXISTS (SELECT 1 FROM FX_Receptii L WHERE L.IDRH = H2.IDRH)) " &
                "  ORDER BY H2.DataH DESC, H2.NrCrt DESC, H2.IDRH DESC LIMIT 1) " &
                "LEFT JOIN FX_Receptii Rc ON Rc.IDRH = H.IDRH " &
                "LEFT JOIN FX_Indicatori I ON I.CodAI = Rc.CodAI " &
                "WHERE COALESCE(R.Sters, 0) = 0 AND COALESCE(H.EsteStergere, 0) = 0 " &
                "GROUP BY R.IDRR, R.CodAngajament, R.DataR, I.IdClsf"
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    Dim k_id As Integer = k_reader.GetInt32(0)
                    Dim k_info As ReceptionInfo = Nothing
                    If Not k_receptions.TryGetValue(k_id, k_info) Then
                        k_info = New ReceptionInfo With {
                            .IdReceptie = k_id,
                            .Cod = If(k_reader.IsDBNull(1), String.Empty, k_reader.GetString(1))}
                        If Not k_reader.IsDBNull(2) Then k_info.DataR = k_reader.GetDateTime(2)
                        k_receptions(k_id) = k_info
                    End If
                    If k_reader.IsDBNull(3) OrElse k_reader.IsDBNull(4) Then Continue While
                    Dim k_clsf As Integer = k_reader.GetInt32(3)
                    Dim k_value As Decimal = Math.Round(Convert.ToDecimal(k_reader.GetValue(4), CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero)
                    Dim k_before As Decimal = 0D
                    k_info.Values.TryGetValue(k_clsf, k_before)
                    k_info.Values(k_clsf) = k_before + k_value
                End While
            End Using
        End Using

        Dim k_allocated As New Dictionary(Of (Integer, Integer), Decimal)()
        Using k_cmd = _cn.CreateCommand()
            k_cmd.Transaction = _tx
            k_cmd.CommandText =
                "SELECT COALESCE(l.IDRR, (SELECT H.IDRR FROM FX_Receptii_H H WHERE H.IDRH = l.IDRH LIMIT 1)) AS IdReceptie, " &
                "l.IdClsf, SUM(l.Suma) AS Suma FROM EF_PrimiteReceptii l GROUP BY IdReceptie, l.IdClsf"
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    If k_reader.IsDBNull(0) OrElse k_reader.IsDBNull(2) Then Continue While
                    k_allocated((k_reader.GetInt32(0), k_reader.GetInt32(1))) = Convert.ToDecimal(k_reader.GetValue(2), CultureInfo.InvariantCulture)
                End While
            End Using
        End Using

        Dim k_byAngajament As New Dictionary(Of String, List(Of ReceptionInfo))(StringComparer.OrdinalIgnoreCase)
        For Each k_info In k_receptions.Values
            If k_info.Cod.Length = 0 Then Continue For
            For Each k_pair In k_info.Values
                Dim k_done As Decimal = 0D
                k_allocated.TryGetValue((k_info.IdReceptie, k_pair.Key), k_done)
                Dim k_rest = k_pair.Value - k_done
                If Math.Abs(k_rest) > Tolerance Then k_info.Rest(k_pair.Key) = k_rest
            Next
            Dim k_list As List(Of ReceptionInfo) = Nothing
            If Not k_byAngajament.TryGetValue(k_info.Cod, k_list) Then
                k_list = New List(Of ReceptionInfo)()
                k_byAngajament(k_info.Cod) = k_list
            End If
            k_list.Add(k_info)
        Next
        Return k_byAngajament
    End Function

    ' ---- writing ---------------------------------------------------------------------------------------------------------

    ''' <summary>
    ''' Writes the decided links in the caller's transaction: the classification on every line of the invoice that has none, and the link row.
    ''' An invoice that did not reach EF_Primite (the writer skips a row that collides on another unique key) is skipped here, so the foreign key
    ''' can never fail the whole import.
    ''' </summary>
    Public Sub Write(k_proposals As List(Of EfLinkProposal), k_report As EfImportReport, k_plan As EfTablePlan)
        Try
            k_plan.RowsBefore = CountLinks()
            Dim k_present As New HashSet(Of Integer)()
            Using k_cmd = _cn.CreateCommand()
                k_cmd.Transaction = _tx
                k_cmd.CommandText = "SELECT `IdPrimita` FROM `EF_Primite`"
                Using k_reader = k_cmd.ExecuteReader()
                    While k_reader.Read()
                        k_present.Add(k_reader.GetInt32(0))
                    End While
                End Using
            End Using

            Dim k_skipped As Integer = 0
            For Each k_proposal In k_proposals
                If Not k_present.Contains(k_proposal.IdPrimita) Then
                    k_skipped += 1
                    Continue For
                End If
                Using k_cmd = _cn.CreateCommand()
                    k_cmd.Transaction = _tx
                    k_cmd.CommandText = "UPDATE `EF_PrimiteLinii` SET `IdClsf` = @c WHERE `IdPrimita` = @i AND `IdClsf` IS NULL"
                    k_cmd.Parameters.AddWithValue("@c", k_proposal.IdClsf)
                    k_cmd.Parameters.AddWithValue("@i", k_proposal.IdPrimita)
                    k_cmd.ExecuteNonQuery()
                End Using
                Using k_cmd = _cn.CreateCommand()
                    k_cmd.Transaction = _tx
                    k_cmd.CommandText = "INSERT INTO `EF_PrimiteReceptii` (`IdPrimita`, `CodAngajament`, `IdClsf`, `IDRR`, `Suma`, `Tip`, `UN`) " &
                                        "VALUES (@i, @a, @c, @r, @s, 'existenta', @u)"
                    k_cmd.Parameters.AddWithValue("@i", k_proposal.IdPrimita)
                    k_cmd.Parameters.AddWithValue("@a", k_proposal.CodAngajament)
                    k_cmd.Parameters.AddWithValue("@c", k_proposal.IdClsf)
                    k_cmd.Parameters.AddWithValue("@r", k_proposal.IdReceptie)
                    k_cmd.Parameters.AddWithValue("@s", k_proposal.Suma)
                    k_cmd.Parameters.AddWithValue("@u", LinkedBy)
                    k_cmd.ExecuteNonQuery()
                End Using
            Next
            If k_skipped > 0 Then
                k_report.Add(EfSeverity.Warning, Area, $"{k_skipped} facturi propuse pentru legare nu au ajuns în EF_Primite (rămân nelegate).")
            End If
            k_plan.RowsAdded = CountLinks() - k_plan.RowsBefore
            k_plan.RowsKept = 0
            _say($"EF_PrimiteReceptii: {k_plan.RowsAdded} legături scrise.")
        Catch ex As Exception
            GlobalErrorLog.Write("EfReceptionLinker.Write", ex)
            Throw
        End Try
    End Sub

    ' ---- helpers ---------------------------------------------------------------------------------------------------------

    Private Shared Function Describe(k_invoice As EfLinkInvoice) As String
        Return $"«{k_invoice.NrFact}» (Id {k_invoice.IdPrimita})"
    End Function

    Private Shared Function Money(k_amount As Decimal) As String
        Return k_amount.ToString("0.00", CultureInfo.InvariantCulture)
    End Function

    Private Function CurrentDatabase() As String
        Return _cn.Database
    End Function

    Private Function CountLinks() As Long
        Using k_cmd = _cn.CreateCommand()
            k_cmd.Transaction = _tx
            k_cmd.CommandText = "SELECT COUNT(*) FROM `EF_PrimiteReceptii`"
            Return Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
        End Using
    End Function

    Private Function TableExists(k_table As String) As Boolean
        Using k_cmd = _cn.CreateCommand()
            k_cmd.Transaction = _tx
            k_cmd.CommandText = "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @t"
            k_cmd.Parameters.AddWithValue("@t", k_table)
            Return Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    Private Function ColumnExists(k_table As String, k_column As String) As Boolean
        Using k_cmd = _cn.CreateCommand()
            k_cmd.Transaction = _tx
            k_cmd.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @t AND COLUMN_NAME = @c"
            k_cmd.Parameters.AddWithValue("@t", k_table)
            k_cmd.Parameters.AddWithValue("@c", k_column)
            Return Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

End Class
