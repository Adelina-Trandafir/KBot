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

' SLICE-KE21-02: KeReceptiiView.Viewing - one responsibility of KeReceptiiView.
Partial Public Class KeReceptiiView

    ' The right side shows only what the invoice already has: its products (nothing can be changed), the angajament and the reception it is linked to, the amounts by
    ' classification. Each reception row offers removal of its links to this invoice.
    Friend Sub EnterViewing(k_invoice As EFacturaPrimita, k_state As EFacturaKeStare)
        angajamente.CancelPending()
        _viewing = k_invoice
        _viewState = k_state
        _data = Nothing
        _angCod = String.Empty
        ntfMesaj.Visible = False
        pgAsociate.gridReceptii.Column(ColRTotal).HeaderText = "Valoare factură"
        gridLinii.ReadOnlyGrid = True
        angajamente.Grid.ReadOnlyGrid = True
        navMod.Visible = False
        editorReceptii.ShowPage(pgAsociate)
        gridAloc.Column(ColARestRecep).Visible = KBotColumnVisibility.Hidden
        pgAsociate.gridReceptii.Column(FacturiCumparareReceptiiAsociateView.ColElimina).Visible = KBotColumnVisibility.Visible
        FillViewedLines(k_invoice, k_state)
        FillAngajamente()
        FillReceptii()
        _viewDispo.Clear()
        FillViewedAlocari(k_state)
        BeginViewedDispo(k_state, _viewSeq)
        UpdateTitles()
        RefreshOk()
        If Not k_state.PoateScrie Then ShowNotice("Numai un utilizator cu rolul «Contabil» poate elimina asocierea facturilor cu recepțiile.", NoticeKind.Warning)
        RaiseEvent ActionStateChanged(Me, EventArgs.Empty)
    End Sub

    ' Back to the page as it works: the tabs, the form of a new reception, the lines of the ticked invoices (none, or what the ticks say).
    Friend Sub LeaveViewing()
        If _viewing Is Nothing Then Return
        _viewing = Nothing
        _viewState = Nothing
        Interlocked.Increment(_viewSeq)
        gridLinii.ReadOnlyGrid = False
        angajamente.Grid.ReadOnlyGrid = False
        pgAsociate.gridReceptii.Column(ColRTotal).HeaderText = "Valoare"
        navMod.Visible = True
        ApplyMode()
        ApplyChecked()
        FillReceptii()
        RaiseEvent ActionStateChanged(Me, EventArgs.Empty)
    End Sub

    Private Sub FillViewedLines(k_invoice As EFacturaPrimita, k_state As EFacturaKeStare)
        _filling = True
        gridLinii.BeginUpdate()
        Try
            gridLinii.ClearRows()
            ' Slice KE20-04: an invoice a reception covers shows what it is allocated on (one row per classification of its links), not its products.
            Dim k_group As String = $"{If(k_invoice.Tip = "NC", "NC ", String.Empty)}{k_invoice.NrFact} / {DateText(k_invoice)} — total {Fmt(k_state.Total)}"
            For Each k_alloc As IGrouping(Of Integer, EFacturaKeLegatura) In k_state.Legaturi.GroupBy(Function(k_l) k_l.IdClsf).OrderBy(
                    Function(k_g) k_g.First().Clsf, StringComparer.CurrentCultureIgnoreCase)
                Dim k_row As KBotDataRow = gridLinii.AddRow()
                k_row.Tag = New LineTag() With {.Invoice = k_invoice, .InitClsf = k_alloc.Key}
                k_row(ColFactura) = k_group
                k_row(ColLClsf) = k_alloc.First().Clsf
                k_row(ColLRec) = CDbl(k_alloc.Sum(Function(k_l) k_l.Suma))
            Next
        Finally
            gridLinii.EndUpdate()
            _filling = False
        End Try
    End Sub

    ' ONLY the receptions the invoice is linked to, one row each (what the invoice takes of it).
    Private Sub FillViewedReceptii()
        If _viewState Is Nothing Then
            pgAsociate.gridReceptii.ClearRows()
            Return
        End If
        FillLinkedReceptii(_viewState.Legaturi)
    End Sub

    ' Slice 02EF-07: the tab «Asociate»: the receptions the TICKED invoices are linked to, one row each; «Valoare» = what those invoices take of it.
    Private Sub FillAssociatedReceptii()
        Dim k_links As New List(Of EFacturaKeLegatura)()
        For Each k_id As Integer In _checked
            Dim k_state As EFacturaKeStare = Nothing
            If _states.TryGetValue(k_id, k_state) Then k_links.AddRange(k_state.Legaturi)
        Next
        FillLinkedReceptii(k_links)
    End Sub

    ' One row per reception of the links: its date (or «în așteptare»), its description and the sum the links take of it.
    Private Sub FillLinkedReceptii(k_links As IEnumerable(Of EFacturaKeLegatura))
        pgAsociate.gridReceptii.BeginUpdate()
        Try
            pgAsociate.gridReceptii.ClearRows()
            Dim k_groups As IEnumerable(Of IGrouping(Of String, EFacturaKeLegatura)) = k_links.GroupBy(
                Function(k_l) If(k_l.Idrr.HasValue, "R" & k_l.Idrr.Value.ToString(CultureInfo.InvariantCulture),
                                 "H" & k_l.Idrh.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) & "/" & k_l.Idr.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)))
            For Each k_group As IGrouping(Of String, EFacturaKeLegatura) In k_groups.OrderBy(Function(k_g) k_g.First().DataR).ThenBy(Function(k_g) k_g.First().DescriereR, StringComparer.Create(_ro, True))
                Dim k_first As EFacturaKeLegatura = k_group.First()
                Dim k_row As KBotDataRow = pgAsociate.gridReceptii.AddRow()
                k_row.Tag = k_group.ToArray()
                k_row(ColRData) = If(k_first.DataR.HasValue, CObj(k_first.DataR.Value),
                                     If(k_first.AsteaptaDescarcarea, "în așteptare", String.Empty))
                k_row(ColRDescriere) = k_first.DescriereR
                k_row(ColRTotal) = CDbl(k_group.Sum(Function(k_l) k_l.Suma))
            Next
            If pgAsociate.gridReceptii.RowCount > 0 Then pgAsociate.gridReceptii.CurrentRowIndex = 0
        Finally
            pgAsociate.gridReceptii.EndUpdate()
        End Try
    End Sub

    ' The key of the budget answer of one angajament on the day of a reception.
    Private Shared Function ViewKey(k_link As EFacturaKeLegatura) As String
        Dim k_day As Date = If(k_link.DataR.HasValue, k_link.DataR.Value.Date, Date.Today)
        Return k_link.CodAngajament & "|" & k_day.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
    End Function

    ' The amounts of the links by classification and angajament; Buget / Receptii / Disponibil / Ramas as in the form of a reception, on the day of the reception
    ' (the answers come in with BeginViewedDispo and the table is laid out again each time).
    Private Sub FillViewedAlocari(k_state As EFacturaKeStare)
        _filling = True
        gridAloc.BeginUpdate()
        Try
            gridAloc.ClearRows()
            _alocDisponibil.Clear()
            For Each k_group As IGrouping(Of String, EFacturaKeLegatura) In k_state.Legaturi.GroupBy(
                    Function(k_l) k_l.CodAngajament & "|" & k_l.IdClsf.ToString(CultureInfo.InvariantCulture)).OrderBy(
                    Function(k_g) k_g.First().Clsf, StringComparer.CurrentCultureIgnoreCase)
                Dim k_first As EFacturaKeLegatura = k_group.First()
                Dim k_row As KBotDataRow = gridAloc.AddRow()
                Dim k_value As Decimal = k_group.Sum(Function(k_l) k_l.Suma)
                k_row(ColAClsf) = k_first.Clsf
                k_row(ColAValRecep) = CDbl(k_value)
                Dim k_answer As EFacturaKeDisponibilZi = Nothing
                If _viewDispo.TryGetValue(ViewKey(k_first), k_answer) Then
                    Dim k_d As EFacturaKeDisponibil = k_answer.Indicatori.FirstOrDefault(
                        Function(k_x) k_x.IdClsf.HasValue AndAlso k_x.IdClsf.Value = k_first.IdClsf)
                    If k_d IsNot Nothing Then
                        _alocDisponibil(k_row) = k_d
                        k_row(ColAReceptii) = CDbl(k_d.Receptii)
                        If k_d.Buget.HasValue Then
                            Dim k_available As Decimal = k_d.Buget.Value - k_d.Receptii
                            k_row(ColABuget) = CDbl(k_d.Buget.Value)
                            k_row(ColADisponibil) = CDbl(k_available)
                            k_row(ColARamas) = CDbl(k_available - k_value)
                        End If
                    End If
                End If
            Next
        Finally
            gridAloc.EndUpdate()
            _filling = False
        End Try
    End Sub

    ' UI boundary (async Sub): asks the server, once for each angajament and reception day of the links, for the budget and the receptions before that day.
    Private Async Sub BeginViewedDispo(k_state As EFacturaKeStare, k_seq As Integer)
        Try
            If _api Is Nothing Then Return
            For Each k_link As EFacturaKeLegatura In k_state.Legaturi
                Dim k_key As String = ViewKey(k_link)
                If _viewDispo.ContainsKey(k_key) Then Continue For
                Dim k_cod As String = k_link.CodAngajament
                Dim k_day As Date = If(k_link.DataR.HasValue, k_link.DataR.Value.Date, Date.Today)
                Dim k_answer As EFacturaKeDisponibilZi = Nothing
                If Not _dispoCache.TryGetValue(k_key, k_answer) Then
                    k_answer = Await _gate.RunAsync(Function() _api.GetKeDisponibilAsync(k_cod, k_day, _cts.Token)).ConfigureAwait(True)
                    If IsDisposed OrElse k_seq <> _viewSeq Then Return
                    _dispoCache(k_key) = k_answer
                End If
                If IsDisposed OrElse k_seq <> _viewSeq Then Return
                _viewDispo(k_key) = k_answer
                FillViewedAlocari(k_state)
            Next
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeReceptiiView.BeginViewedDispo", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.BeginViewedDispo", ex)
            ShowNotice("Bugetul și recepțiile pe indicatori nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' SLICE-KE20-06: the standard delete button acts on the clicked reception row.
    Private Sub RemoveAssociationRequested(k_links As EFacturaKeLegatura()) Handles pgAsociate.RemoveAssociationRequested
        Try
            If _viewing Is Nothing OrElse _viewState Is Nothing OrElse Not _viewState.PoateScrie Then Return
            RaiseEvent UnlinkRequested(_viewing, k_links)
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.RemoveAssociationRequested", ex)
        End Try
    End Sub

End Class
