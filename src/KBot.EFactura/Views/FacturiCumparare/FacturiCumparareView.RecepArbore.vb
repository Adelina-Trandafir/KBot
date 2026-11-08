Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' Slice KE21: the tree of the invoices while the view «Recepții» is on screen -- tick boxes (several invoices on ONE reception), the invoices the view works on, an invoice a
' reception already covers (only looked at), the button of proposals of a supplier and the filter of the header menu. The work of the reception itself is the view's
' (<see cref="KeReceptiiView"/>): here only the tree tells it which invoices are in discussion.
Partial Public Class FacturiCumparareView

    Private Const KeFilterFree As String = "ke_fara"
    Private Const KeFilterAssociated As String = "ke_asociate"
    Private Const KeFilterAll As String = "ke_toate"

    Private ReadOnly _keStates As New Dictionary(Of Integer, EFacturaKeStare)()
    Private _keFilter As String = KeFilterFree
    Private _keSetSeq As Integer

    ' ── What the tree lists ──────────────────────────────────────────────────────

    ' While the view «Recepții» is on screen the tree lists the invoices by the filter of the header menu (in the view of an angajament, which has no menu, all of them).
    Private Function ReceptiiListed(k_p As EFacturaPrimita) As Boolean
        If Math.Abs(k_p.Total) < 0.01D Then Return False   ' nothing to receive
        If _angajamentView Then Return True
        Select Case _keFilter
            Case KeFilterAll : Return True
            Case KeFilterAssociated : Return k_p.Receptii.Count > 0
            Case Else : Return Not KeReceptiiView.IsCovered(k_p)   ' not associated IN FULL: no reception, or receptions that do not cover the whole total yet
        End Select
    End Function

    ' Slice KE13: an invoice a reception already covers is ticked and locked (it is not received again; a press shows it, read-only, with its reception), written in the dim text
    ' colour; the tooltip says by which receptions. Any other invoice is a plain row with an empty tick.
    Private Sub StyleReceptiiLeaf(k_leaf As AdvancedTreeControl.TreeItem, k_p As EFacturaPrimita)
        Dim k_locked As Boolean = KeReceptiiView.IsCovered(k_p) AndAlso Math.Abs(k_p.Total) >= 0.01D
        k_leaf.CheckLocked = k_locked
        k_leaf.NodeForeColor = If(k_locked, ThemeManager.Current.Palette.TextDimColor, System.Drawing.Color.Empty)
        Dim k_tip As String = AssociationTip(k_p)
        If k_tip.Length > 0 Then k_leaf.Tooltip = k_tip
        tree.SetItemCheckState(k_leaf, If(k_locked, AdvancedTreeControl.TreeCheckState.Checked, AdvancedTreeControl.TreeCheckState.Unchecked))
    End Sub

    Private Shared Function AssociationTip(k_p As EFacturaPrimita) As String
        If k_p.Receptii.Count = 0 Then Return String.Empty
        Dim k_sum As Decimal = k_p.Receptii.Sum(Function(k_r) k_r.Suma)
        Dim k_head As String = If(KeReceptiiView.IsCovered(k_p), "Recepționată integral.", $"Recepționată parțial: {k_sum.ToString("N2", _ro)} din {k_p.Total.ToString("N2", _ro)}.")
        Dim k_lines As IEnumerable(Of String) = k_p.Receptii.Select(
            Function(k_r) $"{If(k_r.DataR.HasValue, k_r.DataR.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "recepție încă nedescărcată")} · {k_r.Suma.ToString("N2", _ro)}")
        Return k_head & vbLf & String.Join(vbLf, k_lines) & vbLf & "Un clic pe factură arată recepția ei și clasificațiile pe care e alocată."
    End Function

    ' ── The invoices the view works on ───────────────────────────────────────────

    ' The invoices ticked (the ones already covered are ticked and locked: they are not among them).
    Private Function TickedInvoices() As List(Of EFacturaPrimita)
        Return _nodes.Values.Where(Function(k_n) k_n.CheckState = AdvancedTreeControl.TreeCheckState.Checked AndAlso Not k_n.CheckLocked).
            Select(Function(k_n) TryCast(k_n.Tag, EFacturaPrimita)).Where(Function(k_p) k_p IsNot Nothing).
            OrderBy(Function(k_p) k_p.DataFact).ThenBy(Function(k_p) k_p.IdPrimita).ToList()
    End Function

    ' UI boundary (Handles the tree): a tick changed while the view «Recepții» is on screen.
    Private Async Sub ReceptiiTickChanged()
        Try
            Await ApplyReceptiiSetAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.ReceptiiTickChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Gives the view the invoices it works on: the ticked ones; with none ticked, the one pressed (an invoice a reception already covers is then only looked at, read-only). The state of
    ''' each (its angajamente, links, lines) is read once and kept until the list is read again. A failure becomes a line in the notice of the view. Never throws.
    ''' </summary>
    Private Async Function ApplyReceptiiSetAsync() As Task
        Try
            Dim k_mySeq As Integer = Interlocked.Increment(_keSetSeq)
            Dim k_invoices As List(Of EFacturaPrimita) = TickedInvoices()
            Dim k_viewOnly As Boolean
            If k_invoices.Count = 0 Then
                Dim k_shown As EFacturaPrimita = ShownInvoice()
                If k_shown Is Nothing Then
                    receptii.ClearInvoice()
                    RaiseEvent ReceptiiActionChanged(Me, EventArgs.Empty)
                    Return
                End If
                k_invoices.Add(k_shown)
                k_viewOnly = KeReceptiiView.IsCovered(k_shown) AndAlso Math.Abs(k_shown.Total) >= 0.01D
            End If
            For Each k_p As EFacturaPrimita In k_invoices
                If _keStates.ContainsKey(k_p.IdPrimita) Then Continue For
                Dim k_id As Integer = k_p.IdPrimita
                Dim k_state As EFacturaKeStare = Await _gate.RunAsync(Function() _api.GetKeAsync(k_id, _cts.Token)).ConfigureAwait(True)
                If IsDisposed OrElse k_mySeq <> _keSetSeq Then Return
                _keStates(k_id) = k_state
            Next
            If IsDisposed OrElse k_mySeq <> _keSetSeq Then Return
            If k_viewOnly Then
                receptii.EnterViewing(k_invoices(0), _keStates(k_invoices(0).IdPrimita))
            Else
                receptii.ShowInvoices(k_invoices, _keStates, _angajamentCode)
            End If
            RaiseEvent ReceptiiActionChanged(Me, EventArgs.Empty)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiCumparareView.ApplyReceptiiSetAsync", ex)
            KeNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.ApplyReceptiiSetAsync", ex)
            KeNotice("Recepțiile facturilor nu au putut fi afișate. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Function

    ' The message goes in the notice of the view (the detail, with its own notice, is hidden while the view «Recepții» is on screen).
    Private Sub KeNotice(k_text As String, k_kind As NoticeKind)
        DirectCast(receptii, IKeAngajamenteHost).ShowNotice(k_text, k_kind)
    End Sub

    ' ── The button of proposals of a supplier ───────────────────────────────────

    ' Slice KE16: the invoices of a supplier that have no reception linked at all.
    Private Shared Function FreeInvoices(k_sup As AdvancedTreeControl.TreeItem) As List(Of EFacturaPrimita)
        Return k_sup.Children.Select(Function(k_c) TryCast(k_c.Tag, EFacturaPrimita)).
            Where(Function(k_p) k_p IsNot Nothing AndAlso k_p.Receptii.Count = 0 AndAlso Math.Abs(k_p.Total) >= 0.01D).ToList()
    End Function

    ' Slice KE16: a supplier that has invoices without a reception gets, while the mouse is over its row, the button that opens the window of proposals (bulk link, 1 invoice = 1
    ' reception). Only while the view «Recepții» is on screen.
    Private Sub StyleReceptiiSupplier(k_sup As AdvancedTreeControl.TreeItem)
        Dim k_free As Integer = FreeInvoices(k_sup).Count
        If k_free > 0 Then
            k_sup.RightIcon = FacturaIcons.Link(ThemeManager.Current.Palette, tree.RightIconSize.Width)
            k_sup.ShowRightIconOnHover = True
            k_sup.Tooltip = $"{k_free} {If(k_free = 1, "factură fără recepție", "facturi fără recepție")}. Butonul din dreapta rândului propune legarea lor de recepții."
        End If
    End Sub

    ' UI boundary (async Sub): the button of a supplier. The window only PROPOSES (nothing is saved until the accountant presses «Leagă» in it); when it wrote links the list is read again.
    Private Async Sub Tree_RightIconClicked(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            If Not _receptiiWide OrElse _api Is Nothing OrElse k_node Is Nothing OrElse receptii.Busy Then Return
            Dim k_free As List(Of EFacturaPrimita) = FreeInvoices(k_node)
            If k_free.Count = 0 Then Return
            Dim k_written As Integer
            ' The supplier's angajamente are saved on the server (slice 0121-05): the window needs none sent.
            Using k_dialog As New KePropuneriForm(_api, _gate, SupplierName(k_free), k_free.Select(Function(k_p) k_p.IdPrimita), New List(Of String)())
                If k_dialog.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                k_written = k_dialog.Legate
            End Using
            If IsDisposed Then Return
            _keStates.Clear()
            Await LoadAsync(_idDdf).ConfigureAwait(True)
            If IsDisposed Then Return
            KeNotice($"{k_written} {If(k_written = 1, "factură legată", "facturi legate")} de recepții.", NoticeKind.Success)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiCumparareView.Tree_RightIconClicked", ex)
            KeNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.Tree_RightIconClicked", ex)
            KeNotice("Propunerile nu au putut fi făcute. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' ── The filter of the header menu ───────────────────────────────────────────

    ' Slice KE13 / 02EF-06: which invoices the tree lists while the view «Recepții» is on screen: «fără recepții» (the way it opens), «asociate» (ONLY the ones a reception is linked to) or all.
    Private Function ReceptiiFilterItems() As List(Of CustomPopupItem)
        Return New List(Of CustomPopupItem) From {
            New CustomPopupItem(KeFilterFree, "Arată facturile fără &recepții") With {.Checked = String.Equals(_keFilter, KeFilterFree, StringComparison.Ordinal)},
            New CustomPopupItem(KeFilterAssociated, "Arată facturile &asociate") With {.Checked = String.Equals(_keFilter, KeFilterAssociated, StringComparison.Ordinal)},
            New CustomPopupItem(KeFilterAll, "Arată &toate facturile") With {.Checked = String.Equals(_keFilter, KeFilterAll, StringComparison.Ordinal)},
            CustomPopupItem.Separator()}
    End Function

    ' UI boundary (called from OptionChosen): a row of the filter. True = it was one of the filter's rows (handled here).
    Private Function ChooseReceptiiFilter(k_key As String) As Boolean
        If Not (String.Equals(k_key, KeFilterFree, StringComparison.Ordinal) OrElse String.Equals(k_key, KeFilterAssociated, StringComparison.Ordinal) OrElse
                String.Equals(k_key, KeFilterAll, StringComparison.Ordinal)) Then Return False
        If String.Equals(k_key, _keFilter, StringComparison.Ordinal) Then Return True
        _keFilter = k_key
        BuildTree(If(_shownId > 0, CType(_shownId, Integer?), Nothing))
        Dim k_ignored As Task = ApplyReceptiiSetAsync()
        Return True
    End Function

    ' ── An invoice a reception covers: the link is taken off ────────────────────

    ' SLICE-KE20-06: the standard delete button of a reception row of an invoice only looked at. A row can represent several links, one per classification of the same reception.
    Private Async Sub Receptii_UnlinkRequested(k_invoice As EFacturaPrimita, k_links As EFacturaKeLegatura()) Handles receptii.UnlinkRequested
        Try
            If receptii.Busy OrElse IsDisposed Then Return
            Dim k_reception As EFacturaKeLegatura = k_links(0)
            Dim k_label As String = k_reception.DescriereR
            If k_reception.DataR.HasValue Then k_label = $"{k_label} ({k_reception.DataR.Value:dd.MM.yyyy})".Trim()
            Dim k_receptionText As String = If(String.IsNullOrWhiteSpace(k_label), "recepția selectată", $"recepția «{k_label}»")
            If KBotMessage.Show(FindForm(), $"Eliminați din K-BOT asocierea facturii {k_invoice.NrFact} cu {k_receptionText}?",
                                "Recepții", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            receptii.SetBusy(True)
            Try
                Dim k_id As Integer = k_invoice.IdPrimita
                Dim k_state As EFacturaKeStare = Await _gate.RunAsync(Function() _api.GetKeAsync(k_id, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                If Not k_state.PoateScrie Then
                    KeNotice("Numai un utilizator cu rolul «Contabil» poate elimina asocierea facturilor cu recepțiile.", NoticeKind.Warning)
                    Return
                End If
                Dim k_rowIds As New HashSet(Of Integer)(k_links.Select(Function(k_link) k_link.IdLegatura))
                Dim k_currentLinks As List(Of EFacturaKeLegatura) = k_state.Legaturi.Where(Function(k_link) k_rowIds.Contains(k_link.IdLegatura)).ToList()
                Dim k_removalError As Exception = Nothing
                Try
                    For Each k_link As EFacturaKeLegatura In k_currentLinks
                        Dim k_idLink As Integer = k_link.IdLegatura
                        Await _gate.RunAsync(Function() _api.RemoveKeLegaturaAsync(k_id, k_idLink, _cts.Token)).ConfigureAwait(True)
                        If IsDisposed Then Return
                    Next
                Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
                    k_removalError = ex
                    GlobalErrorLog.Write("FacturiCumparareView.Receptii_UnlinkRequested", ex)
                End Try
                _keStates.Clear()
                Await LoadAsync(_idDdf).ConfigureAwait(True)
                If IsDisposed Then Return
                If k_removalError IsNot Nothing Then
                    KeNotice(If(TypeOf k_removalError Is ApiException, k_removalError.Message,
                                "Eliminarea asocierii nu s-a încheiat. Detalii în jurnalul de erori."), NoticeKind.Error)
                ElseIf k_currentLinks.Count > 0 Then
                    KeNotice($"Asocierea facturii {k_invoice.NrFact} cu {k_receptionText} a fost eliminată.", NoticeKind.Success)
                End If
            Finally
                If Not IsDisposed Then receptii.SetBusy(False)
            End Try
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiCumparareView.Receptii_UnlinkRequested", ex)
            KeNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.Receptii_UnlinkRequested", ex)
            KeNotice("Asocierea nu a putut fi eliminată. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

End Class
