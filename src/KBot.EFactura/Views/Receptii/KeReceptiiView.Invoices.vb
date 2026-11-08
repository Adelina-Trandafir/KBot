Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' SLICE-KE21-02: KeReceptiiView.Invoices - one responsibility of KeReceptiiView.
Partial Public Class KeReceptiiView

    ' An invoice that has nothing left to receive: its value is 0, or the receptions it is linked to already cover its whole total (the links of all of them added up).
    Friend Shared Function IsCovered(k_p As EFacturaPrimita) As Boolean
        If Math.Abs(k_p.Total) < 0.01D Then Return True   ' nothing to receive
        If k_p.Receptii.Count = 0 Then Return False
        Dim k_sum As Decimal = k_p.Receptii.Sum(Function(k_r) k_r.Suma)
        Return Math.Abs(k_sum) >= Math.Abs(k_p.Total) - 0.01D
    End Function

    Friend Shared Function SupplierKey(k_p As EFacturaPrimita) As String
        If Not String.IsNullOrWhiteSpace(k_p.CuiNormalizat) Then Return k_p.CuiNormalizat.Trim()
        Return "N" & k_p.Furnizor.Trim().ToUpperInvariant()
    End Function

    Friend Shared Function DateText(k_p As EFacturaPrimita) As String
        Return If(k_p.DataFact.HasValue, k_p.DataFact.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "fără dată")
    End Function

    ' The suppliers the angajamente table is about now: those of the ticked invoices, else the one pressed in the tree ("" when none).
    Private Function ContextKeys() As List(Of String)
        Dim k_keys As New List(Of String)()
        For Each k_id As Integer In _checked
            Dim k_inv As EFacturaPrimita = Nothing
            If _invoices.TryGetValue(k_id, k_inv) AndAlso Not k_keys.Contains(SupplierKey(k_inv)) Then k_keys.Add(SupplierKey(k_inv))
        Next
        If k_keys.Count = 0 Then k_keys.Add(String.Empty)
        Return k_keys
    End Function

    ' The tax code of the supplier a key stands for; empty when its invoices carry none (the key is then built from the name).
    Private Function SupplierCui(k_key As String) As String
        For Each k_inv As EFacturaPrimita In _invoices.Values
            If Not String.IsNullOrWhiteSpace(k_inv.CuiNormalizat) AndAlso String.Equals(SupplierKey(k_inv), k_key, StringComparison.Ordinal) Then
                Return k_inv.CuiNormalizat.Trim()
            End If
        Next
        Return String.Empty
    End Function

    ' The state of the invoices the angajamente table is about (ticked, else the one pressed) is read again, after the supplier's links changed.
    Private Async Function RefreshContextStatesAsync() As Task
        ' The states kept for every invoice of those suppliers are stale now (their candidates); the ones on screen are read again.
        Dim k_keys As List(Of String) = ContextKeys()
        For Each k_inv As EFacturaPrimita In _invoices.Values
            If k_keys.Contains(SupplierKey(k_inv)) Then _states.Remove(k_inv.IdPrimita)
        Next
        For Each k_id As Integer In _checked.ToList()
            Dim k_idNow As Integer = k_id
            _states(k_idNow) = Await _gate.RunAsync(Function() _api.GetKeAsync(k_idNow, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
        Next
    End Function

    Friend Sub ApplyChecked()
        _filling = True
        gridLinii.BeginUpdate()
        Try
            Dim k_keep As New HashSet(Of Integer)(_checked)
            For k_i As Integer = gridLinii.RowCount - 1 To 0 Step -1
                Dim k_tag As LineTag = TryCast(gridLinii.Rows(k_i).Tag, LineTag)
                If k_tag Is Nothing OrElse Not k_keep.Contains(k_tag.Invoice.IdPrimita) Then gridLinii.RemoveRowAt(k_i)
            Next
            Dim k_have As New HashSet(Of Integer)(gridLinii.Rows.Select(Function(k_r) CType(k_r.Tag, LineTag).Invoice.IdPrimita))
            ' Slice KE20-04: a newly ticked invoice gets ONE row for the classification its lines already carry (when they agree); the empty row follows (ReshapeInvoices).
            For Each k_id As Integer In _checked
                If k_have.Contains(k_id) Then Continue For
                Dim k_stored As Integer? = StoredClass(k_id)
                If k_stored.HasValue Then AddAllocRow(k_id, k_stored)
            Next
        Finally
            gridLinii.EndUpdate()
            _filling = False
        End Try
        UpdateAssociatedTab()
        ApplyIndicators()
        ApplySingleIndicator(False)
        ReshapeInvoices()
        FillAngajamente()
        ApplyFieldDefaults()
        RecomputeDefaults()
        FillAlocari()
        UpdateTitles()
        ' Slice KE20-02: the ticks decide the day the receptions are measured on.
        If _mode = EFacturaKeTip.Existenta AndAlso _data IsNot Nothing AndAlso Not String.Equals(CurrentRecKey(), _recShownKey, StringComparison.OrdinalIgnoreCase) Then
            FillReceptii()
            BeginLoadReceptiiAt()
        End If
    End Sub

    ' Slice 02EF-07: the tabs follow the ticked invoices. «Asociate» is there only while one of them has receptions linked. «Adaugă» is there only while NONE of them is associated
    ' in full (a new reception for what is already covered makes no sense). When the tab that is on goes away, the page goes to the first one left; while it stays on, its grid follows the ticks.
    Private Sub UpdateAssociatedTab()
        Dim k_has As Boolean = _checked.Any(Function(k_id) _states.ContainsKey(k_id) AndAlso _states(k_id).Legaturi.Count > 0)
        Dim k_full As Boolean = _checked.Any(Function(k_id) _invoices.ContainsKey(k_id) AndAlso IsCovered(_invoices(k_id)))
        navMod.SetItemVisible(TabAsociate, k_has)
        navMod.SetItemVisible(TabNoua, Not k_full)
        Dim k_key As String = navMod.SelectedKey
        Dim k_gone As Boolean = (String.Equals(k_key, TabAsociate, StringComparison.Ordinal) AndAlso Not k_has) OrElse
                                (String.Equals(k_key, TabNoua, StringComparison.Ordinal) AndAlso k_full)
        If Not k_gone Then
            If _assocTab Then FillReceptii()
            Return
        End If
        SelectTab(If(k_has, TabAsociate, If(k_full, TabExistente, TabNoua)))
    End Sub

    ' Puts the page on a tab without going through the whole of ChangeMode (the caller, a refresh of the ticks, goes on to refill everything else).
    Private Sub SelectTab(k_key As String)
        _loadingMode = True
        Try
            navMod.SelectedKey = k_key
        Finally
            _loadingMode = False
        End Try
        Dim k_existing As Boolean = String.Equals(k_key, TabExistente, StringComparison.Ordinal)
        _assocTab = String.Equals(k_key, TabAsociate, StringComparison.Ordinal)
        _mode = If(k_existing, _actionKeys(Math.Max(0, pgExistente.cmbMod.SelectedIndex)), EFacturaKeTip.Noua)
        ApplyMode()
        FillReceptii()
        BeginLoadReceptiiAt()
        BeginRefreshDispo()
        RaiseEvent ActionStateChanged(Me, EventArgs.Empty)
    End Sub

    ' The classification ALL the classified lines of the invoice carry (they agree), else Nothing: the one it was last given.
    Private Function StoredClass(k_id As Integer) As Integer?
        Dim k_ids As List(Of Integer) = _states(k_id).Linii.Where(Function(k_l) k_l.IdClsf.HasValue).Select(Function(k_l) k_l.IdClsf.Value).Distinct().ToList()
        Return If(k_ids.Count = 1, CType(k_ids(0), Integer?), Nothing)
    End Function

    ' What a row shows for a classification: the indicator's label, else the code the lines carry.
    Private Function LabelFor(k_idClsf As Integer, k_id As Integer) As String
        If _labelOf.ContainsKey(k_idClsf) Then Return _labelOf(k_idClsf)
        Dim k_line As EFacturaKeLinie = _states(k_id).Linii.FirstOrDefault(Function(k_l) k_l.IdClsf.HasValue AndAlso k_l.IdClsf.Value = k_idClsf)
        Return If(k_line IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(k_line.Clsf), k_line.Clsf, NeAleasa)
    End Function

    ' A row of the invoice: with a classification (its sum comes from RecomputeDefaults) or, with Nothing, the empty one.
    Private Function AddAllocRow(k_id As Integer, k_cls As Integer?) As KBotDataRow
        Dim k_row As KBotDataRow = gridLinii.AddRow()
        k_row.Tag = New LineTag() With {.Invoice = _invoices(k_id), .InitClsf = k_cls}
        If k_cls.HasValue Then
            k_row(ColLClsf) = LabelFor(k_cls.Value, k_id)
            k_row(ColLRec) = 0.0R
        Else
            k_row(ColLClsf) = NeAleasa
            k_row(ColLRec) = Nothing
        End If
        Return k_row
    End Function

    ' What is left of the invoice to allocate: its total less what the links it already has take (the server's figure, with the sign of the invoice).
    Private Function InvoiceRest(k_id As Integer) As Decimal
        Return _states(k_id).Rest
    End Function

    ' The sum of the amounts of the classified rows of the invoice, the row k_except left out.
    Private Function AllocatedOf(k_id As Integer, Optional k_except As KBotDataRow = Nothing) As Decimal
        Dim k_sum As Decimal = 0D
        For Each k_row As KBotDataRow In RowsOf(k_id)
            If k_except IsNot Nothing AndAlso ReferenceEquals(k_row, k_except) Then Continue For
            If Not ChosenClass(k_row).HasValue Then Continue For
            Dim k_number As Decimal = 0D
            If ReadNumber(k_row(ColLRec), k_number) Then k_sum += k_number
        Next
        Return k_sum
    End Function

    ' The shape of the rows of every ticked invoice (see above): the amounts are worked out first, the empty rows taken away, and ONE empty row put back where there is
    ' still something to allocate. Changes the rows, so it never runs inside the edit of one of them (QueueReshape).
    Private Sub ReshapeInvoices()
        If _viewing IsNot Nothing Then Return
        RecomputeDefaults()
        _filling = True
        gridLinii.BeginUpdate()
        Try
            For Each k_id As Integer In _checked
                For k_i As Integer = gridLinii.RowCount - 1 To 0 Step -1
                    Dim k_tag As LineTag = TryCast(gridLinii.Rows(k_i).Tag, LineTag)
                    If k_tag IsNot Nothing AndAlso k_tag.Invoice.IdPrimita = k_id AndAlso Not ChosenClass(gridLinii.Rows(k_i)).HasValue Then gridLinii.RemoveRowAt(k_i)
                Next
                Dim k_rest As Decimal = InvoiceRest(k_id)
                Dim k_left As Decimal = k_rest - AllocatedOf(k_id)
                If Math.Abs(k_left) >= Tolerance AndAlso Math.Sign(k_left) = Math.Sign(k_rest) Then
                    Dim k_empty As KBotDataRow = AddAllocRow(k_id, Nothing)
                    k_empty(ColLDenumire) = $"Rest de alocat: {Fmt(k_left)}"
                End If
            Next
            AssignGroupKeys()
        Finally
            gridLinii.EndUpdate()
            _filling = False
        End Try
        gridLinii.Invalidate()
    End Sub

    ' The same, a moment later: from the event of a cell or of a header combo the rows must not change under the editor that raised it.
    Private _reshapeQueued As Boolean

    Private Sub QueueReshape()
        If _reshapeQueued Then Return
        _reshapeQueued = True
        If IsHandleCreated Then
            BeginInvoke(New Action(AddressOf RunReshape))
        Else
            RunReshape()
        End If
    End Sub

    ' UI boundary (queued): logs and swallows.
    Private Sub RunReshape()
        _reshapeQueued = False
        Try
            If IsDisposed OrElse _viewing IsNot Nothing Then Return
            ReshapeInvoices()
            FillAlocari()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.RunReshape", ex)
        End Try
    End Sub

    ' The group of an invoice is its «number / date», its total and what is left; two ticked invoices that would read the same get the supplier added, so they stay two groups.
    Private Sub AssignGroupKeys()
        Dim k_base As New Dictionary(Of Integer, String)()
        For Each k_id As Integer In _checked
            Dim k_p As EFacturaPrimita = _invoices(k_id)
            Dim k_state As EFacturaKeStare = _states(k_id)
            k_base(k_id) = $"{If(k_p.Tip = "NC", "NC ", String.Empty)}{k_p.NrFact} / {DateText(k_p)} — total {Fmt(k_state.Total)}" &
                           If(Math.Abs(k_state.Alocat) >= 0.01D, $", rest {Fmt(k_state.Rest)}", String.Empty)
        Next
        Dim k_count As Dictionary(Of String, Integer) = k_base.Values.GroupBy(Function(k_s) k_s).ToDictionary(Function(k_g) k_g.Key, Function(k_g) k_g.Count())
        For Each k_row As KBotDataRow In gridLinii.Rows
            Dim k_tag As LineTag = CType(k_row.Tag, LineTag)
            Dim k_text As String = k_base(k_tag.Invoice.IdPrimita)
            If k_count(k_text) > 1 Then k_text &= " · " & k_tag.Invoice.Furnizor
            k_row(ColFactura) = k_text
        Next
    End Sub

    ' The date and the description of a new reception follow the ticked invoices, until the accountant writes his own.
    Private Sub ApplyFieldDefaults()
        If _mode <> EFacturaKeTip.Noua OrElse _checked.Count = 0 Then Return
        Dim k_list As List(Of EFacturaPrimita) = _checked.Select(Function(k_id) _invoices(k_id)).ToList()
        Dim k_newDate As Date = k_list.Select(Function(k_p) If(k_p.DataFact, Date.Today)).Max()
        If _dataAuto = Date.MinValue OrElse pgNoua.dtpData.Value.Date = _dataAuto.Date Then
            pgNoua.dtpData.Value = k_newDate
            _dataAuto = k_newDate
        End If
        Dim k_text As String = "plata " & If(k_list.Count = 1, "factura ", "facturi ") & String.Join(", ", k_list.Select(Function(k_p) DescriptionPart(k_p)))
        If String.Equals(pgNoua.txtDescriere.Text, _descrAuto, StringComparison.Ordinal) Then
            pgNoua.txtDescriere.Text = k_text
            _descrAuto = k_text
        End If
    End Sub

    Private Shared Function DescriptionPart(k_p As EFacturaPrimita) As String
        Return k_p.NrFact & If(k_p.DataFact.HasValue, "/" & k_p.DataFact.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
    End Function

    ' Slice KE21: the line over the lines says what the invoices in discussion are and how they stand with the receptions, read from their states: one invoice (its total,
    ' what the receptions take of it, what is left, the state), several ticked (the sums), or none.
    Private Sub UpdateTitles()
        lblDetalii.Text = DetailsText()
    End Sub

    Private Function DetailsText() As String
        Dim k_ids As List(Of Integer) = If(_viewing IsNot Nothing, New List(Of Integer) From {_viewing.IdPrimita}, _checked)
        If k_ids.Count = 0 Then Return "Nicio factură bifată"
        Dim k_total As Decimal, k_allocated As Decimal, k_rest As Decimal
        Dim k_last As EFacturaKeStare = Nothing
        For Each k_id As Integer In k_ids
            Dim k_state As EFacturaKeStare = Nothing
            If Not _states.TryGetValue(k_id, k_state) Then Return DetailsWithoutState(k_ids)
            k_total += k_state.Total
            k_allocated += k_state.Alocat
            k_rest += k_state.Rest
            k_last = k_state
        Next
        Dim k_figures As String = $"{Fmt(k_total)}   ·   alocat pe recepții: {Fmt(k_allocated)}   ·   rest: {Fmt(k_rest)}"
        If k_ids.Count = 1 Then
            Return $"Total factură {InvoiceCaption(InvoiceOf(k_ids(0)))}: {k_figures}   ·   {StateWords(k_last.Stare)}"
        End If
        Return $"{k_ids.Count} facturi bifate: total {k_figures}"
    End Function

    ' The states are still on their way: only the invoice(s) are named.
    Private Function DetailsWithoutState(k_ids As List(Of Integer)) As String
        If k_ids.Count = 1 Then Return $"Factura {InvoiceCaption(InvoiceOf(k_ids(0)))}"
        Return $"{k_ids.Count} facturi bifate"
    End Function

    Private Function InvoiceOf(k_id As Integer) As EFacturaPrimita
        If _viewing IsNot Nothing AndAlso _viewing.IdPrimita = k_id Then Return _viewing
        Dim k_invoice As EFacturaPrimita = Nothing
        Return If(_invoices.TryGetValue(k_id, k_invoice), k_invoice, Nothing)
    End Function

    ' «number / date» of the invoice (a credit note says so); «(cu TVA)» when the invoice is not known.
    Private Shared Function InvoiceCaption(k_invoice As EFacturaPrimita) As String
        If k_invoice Is Nothing OrElse String.IsNullOrWhiteSpace(k_invoice.NrFact) Then Return "(cu TVA)"
        Dim k_date As String = If(k_invoice.DataFact.HasValue, k_invoice.DataFact.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "fără dată")
        Return $"{If(k_invoice.Tip = "NC", "NC ", String.Empty)}{k_invoice.NrFact.Trim()} / {k_date}"
    End Function

    Private Shared Function StateWords(k_state As String) As String
        Select Case k_state
            Case "acoperita" : Return "acoperită"
            Case "partiala" : Return "acoperită parțial"
            Case "depasita" : Return "alocarea depășește factura"
            Case Else : Return "nelegată de recepții"
        End Select
    End Function

    ' The selected reception view and the action button follow the existing mode rules.
    Private Sub ApplyMode()
        Dim k_page As Control = If(_viewing IsNot Nothing OrElse _assocTab,
                                   DirectCast(pgAsociate, Control),
                                   If(_mode = EFacturaKeTip.Noua, DirectCast(pgNoua, Control), DirectCast(pgExistente, Control)))
        editorReceptii.ShowPage(k_page)
        pgAsociate.gridReceptii.Column(FacturiCumparareReceptiiAsociateView.ColElimina).Visible =
            If(_viewing IsNot Nothing, KBotColumnVisibility.Visible, KBotColumnVisibility.Hidden)
        Select Case _mode
            Case EFacturaKeTip.Existenta
                btnOk.Text = "Leagă"
                tips.SetToolTipText(btnOk, "Leagă facturile bifate de recepția aleasă, pe sumele din «Recepționată». Recepția nu se schimbă.")
            Case EFacturaKeTip.Noua
                btnOk.Text = "Creează recepția"
                tips.SetToolTipText(btnOk, "Creează în FOREXE o recepție cu sumele de mai jos (a tuturor facturilor bifate) și le leagă de ea.")
            Case Else
                btnOk.Text = "Modifică recepția"
                tips.SetToolTipText(btnOk, "Mărește în FOREXE recepția aleasă cu sumele de mai jos și leagă facturile bifate de ea.")
        End Select
        gridAloc.Column(ColARestRecep).Visible = If(_mode = EFacturaKeTip.Existenta, KBotColumnVisibility.Visible, KBotColumnVisibility.Hidden)
        btnOk.Enabled = False
        UpdateTitles()
    End Sub

    ''' <summary>
    ''' The host gives the view the invoices it works on (the ticked ones, or the one pressed) and, when <paramref name="k_angajament"/> is not empty (the view of an angajament), THAT angajament.
    ''' <paramref name="k_states"/> are the states of the invoices the host already read. Everything kept about the former invoices is dropped. UI boundary: logs and swallows.
    ''' </summary>
    Public Sub ShowInvoices(k_invoices As IReadOnlyList(Of EFacturaPrimita), k_states As IReadOnlyDictionary(Of Integer, EFacturaKeStare), k_angajament As String)
        Try
            ArgumentNullException.ThrowIfNull(k_invoices)
            ArgumentNullException.ThrowIfNull(k_states)
            _embedded = True
            If _viewing IsNot Nothing Then LeaveViewing()
            _fixedAng = If(k_angajament, String.Empty).Trim()
            angajamente.FixedAngajament = _fixedAng
            angajamente.ShortLabels = True
            ResetWork()
            _invoices.Clear()
            For Each k_invoice As EFacturaPrimita In k_invoices
                _invoices(k_invoice.IdPrimita) = k_invoice
                _states(k_invoice.IdPrimita) = k_states(k_invoice.IdPrimita)
            Next
            _checked = k_invoices.Select(Function(k_p) k_p.IdPrimita).ToList()
            ApplyChecked()
            FillReceptii()
            If _fixedAng.Length = 0 Then angajamente.BeginLoadAll()
            ShowPendingNotice()
            If k_invoices.Any(Function(k_p) Not _states(k_p.IdPrimita).PoateScrie) AndAlso Not ntfMesaj.Visible Then
                ShowNotice("Numai un utilizator cu rolul «Contabil» poate lega facturile de recepții.", NoticeKind.Warning)
            End If
            RelayoutSoon()
            RaiseEvent ActionStateChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.ShowInvoices", ex)
            ShowNotice("Recepțiile facturilor nu au putut fi afișate. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ''' <summary>The view of an invoice shows nothing (the host has no invoice to show). UI boundary: logs and swallows.</summary>
    Public Sub ClearInvoice()
        Try
            ResetWork()
            _invoices.Clear()
            _checked = New List(Of Integer)()
            ApplyChecked()
            FillReceptii()
            ntfMesaj.Visible = False
            RaiseEvent ActionStateChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.ClearInvoice", ex)
        End Try
    End Sub

    ''' <summary>The message of success a reception left (the host read the invoices again): shown once.</summary>
    Friend Sub ShowPendingNotice()
        If _pendingNotice.Length = 0 Then Return
        ShowNotice(_pendingNotice, NoticeKind.Success)
        _pendingNotice = String.Empty
    End Sub


    ' What was kept about the former invoice / angajament.
    Private Sub ResetWork()
        Interlocked.Increment(_syncSeq)
        _states.Clear()
        angajamente.ResetCache()
        _dispoCache.Clear()
        _dispoKey = String.Empty
        _dispo.Clear()
        _extra.Clear()
        ForgetReceptiiAt()
        _data = Nothing
        _angCod = String.Empty
        pgNoua.txtDescriere.Text = String.Empty
        _descrAuto = String.Empty
        _dataAuto = Date.MinValue
        If _pendingNotice.Length = 0 Then ntfMesaj.Visible = False
    End Sub

    ' UI boundary: the date of a new reception changed -> the budget and the receptions before it are asked again.
    Private Sub DtpData_ValueChanged(sender As Object, e As EventArgs) Handles pgNoua.ReceptionDateChanged
        Try
            If _mode = EFacturaKeTip.Noua Then BeginRefreshDispo()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.DtpData_ValueChanged", ex)
        End Try
    End Sub

    ' UI boundary: the navbar (new reception / existing receptions) changed.
    Private Sub NavMod_SelectionChanged(k_key As String) Handles navMod.SelectionChanged
        Try
            ChangeMode()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.NavMod_SelectionChanged", ex)
        End Try
    End Sub

    ' UI boundary: what is done with an existing reception (link / modify) was chosen.
    Private Sub CmbMod_SelectedIndexChanged(sender As Object, e As EventArgs) Handles pgExistente.ActionChanged
        Try
            ChangeMode()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.CmbMod_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' The mode is the navbar (new / existing) and, on «Recepții existente», the action chosen below it (link / modify).
    Private Sub ChangeMode()
        Try
            If _loadingMode OrElse pgExistente Is Nothing OrElse pgExistente.cmbMod.SelectedIndex < 0 Then Return
            Dim k_new As Boolean = Not String.Equals(navMod.SelectedKey, TabExistente, StringComparison.Ordinal)
            ' Slice 02EF-07: «Asociate» works like «Adaugă» as to the amounts (nothing is made there); only the grid and the button differ.
            _assocTab = String.Equals(navMod.SelectedKey, TabAsociate, StringComparison.Ordinal)
            _mode = If(k_new, EFacturaKeTip.Noua, _actionKeys(pgExistente.cmbMod.SelectedIndex))
            ' Slice KE20-01: a link to an existing reception takes no value above the invoices.
            If _mode = EFacturaKeTip.Existenta Then _extra.Clear()
            ApplyMode()
            FillReceptii()
            BeginLoadReceptiiAt()
            ApplyFieldDefaults()
            RecomputeDefaults()
            FillAlocari()
            UpdateTitles()
            BeginRefreshDispo()
            RaiseEvent ActionStateChanged(Me, EventArgs.Empty)   ' the ribbon button is not offered on «Asociate»
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.ChangeMode", ex)
        End Try
    End Sub

End Class
