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

' Slices KE01..KE07, KE21 -- the links of the shown invoice with receptions: the view «Recepții». Since KE21 the work is done by ONE view (<see cref="KeReceptiiView"/>), the same one the page
' «Recepții <> E-Factura» holds, and the table of angajamente (<see cref="KeAngajamenteView"/>) stands under the tree, as there. This page only tells them which invoice is shown. The rules
' (amounts with VAT, what is left, who may write) are the server's; the robot that writes in FOREXE is the shell's (<see cref="KeRobot"/>). Plan: docs/KE_PLAN.md.
Partial Public Class FacturiCumparareView

    Private _ke As EFacturaKeStare
    Private _keSeq As Integer
    Private _keCod As String = String.Empty
    Private _angajamentCode As String = String.Empty

    ''' <summary>
    ''' Slice KE12-04: the angajament the view is for (the shell's «E-Factura» view of an angajament), or empty (the window with all the received invoices). When it is given, the view «Recepții» of an
    ''' invoice does not offer the choice of the angajament.
    ''' </summary>
    Public Property AngajamentCode As String
        Get
            Return _angajamentCode
        End Get
        Set(k_value As String)
            _angajamentCode = If(k_value, String.Empty).Trim()
            If _receptiiWide Then ApplyReceptiiJoker()
        End Set
    End Property

    ' ── Reading ─────────────────────────────────────────────────────────────────

    ' Reads the state of the links of the shown invoice and puts it on screen. A failure becomes a line in the view; nothing is thrown.
    Private Async Function LoadKeAsync(k_id As Integer) As Task
        Try
            Dim k_mySeq As Integer = Interlocked.Increment(_keSeq)
            Dim k_state As EFacturaKeStare = Await _gate.RunAsync(Function() _api.GetKeAsync(k_id, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _keSeq OrElse k_id <> _shownId Then Return
            ApplyKe(k_state)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiCumparareView.LoadKeAsync", ex)
            If Not IsDisposed Then ShowKeProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.LoadKeAsync", ex)
            If Not IsDisposed Then ShowKeProblem("Legăturile cu recepțiile nu au putut fi citite. Detalii în jurnalul de erori.")
        End Try
    End Function

    Private Sub ShowKeProblem(k_text As String)
        _ke = Nothing
        detalii.navDetaliu.SetItemVisible(FacturiCumparareDetaliiView.ViewReceptii, True)
        receptii.ClearInvoice()
        DirectCast(receptii, IKeAngajamenteHost).ShowNotice(k_text, NoticeKind.Error)
    End Sub

    Private Sub ApplyKe(k_state As EFacturaKeStare)
        _ke = k_state
        ' An invoice that is already linked goes on with the angajament of its first link.
        If String.IsNullOrEmpty(_keCod) AndAlso k_state.Legaturi.Count > 0 Then _keCod = k_state.Legaturi(0).CodAngajament
        detalii.navDetaliu.SetItemVisible(FacturiCumparareDetaliiView.ViewReceptii, True)
        Dim k_invoice As EFacturaPrimita = ShownInvoice()
        If k_invoice Is Nothing Then Return
        FillKeLines(k_state)
        _keStates(_shownId) = k_state
        Dim k_ignored As Task = ApplyReceptiiSetAsync()
    End Sub

    ' The invoice on screen (the detail when it is the one, else the row of the list); Nothing when there is none.
    Private Function ShownInvoice() As EFacturaPrimita
        Return If(_detail IsNot Nothing AndAlso _detail.Factura.IdPrimita = _shownId, _detail.Factura, _items.FirstOrDefault(Function(k_p) k_p.IdPrimita = _shownId))
    End Function

    ' ── The shape of the view «Recepții» ────────────────────────────────────────

    ' The reception work needs the width: while the view is on screen the tree is only as high as the top band, the table of angajamente stands under it, and the view of the
    ' reception takes the whole right side (the bar of the pages stands over it). Every other view keeps the tall tree and the detail.
    Private _receptiiWide As Boolean

    ' While the view «Recepții» is on screen the whole page is the layout of the view (KeReceptiiView): the tree of the invoices, the bar of the pages and the table of angajamente
    ' stand in its places (left, top, under the tree), the grid of classifications runs under them. Every other page keeps the tall tree and the detail.
    Private _treeBoxesBefore As Boolean

    Private Sub ApplyReceptiiShape(k_wide As Boolean)
        If k_wide = _receptiiWide Then Return
        _receptiiWide = k_wide
        If k_wide Then
            _treeBoxesBefore = tree.CheckBoxes
            tree.CheckBoxes = True   ' the tick boxes of the invoices (several on one reception)
            tlyBody.Visible = False
            pnlArbore.Controls.Remove(tree)
            receptii.TreePane.Controls.Add(tree)
            tree.Dock = DockStyle.Fill
            detalii.HostNavBar(receptii.NavPane)
            ApplyReceptiiJoker()
            receptii.Visible = True
        Else
            receptii.Visible = False
            receptii.TreePane.Controls.Remove(tree)
            pnlArbore.Controls.Add(tree)
            tree.Dock = DockStyle.Fill
            detalii.TakeNavBack()
            tlyBody.Visible = True
            tree.CheckBoxes = _treeBoxesBefore
        End If
        ' The tick boxes, the filter and the buttons of the tree are those of the page now on screen.
        If _items.Count > 0 Then BuildTree(If(_shownId > 0, CType(_shownId, Integer?), Nothing))
        Dim k_ignored As Task = ApplyReceptiiSetAsync()
    End Sub

    ' The free place under the tree: the table of angajamente, or, in the view of an angajament (its angajament is the one chosen in the shell), the grid of classifications.
    Private Sub ApplyReceptiiJoker()
        If _angajamentCode.Length > 0 Then
            angajamente.Visible = False
            receptii.JokerUse = KeJokerUse.GridLeft
        Else
            receptii.JokerUse = KeJokerUse.Table
            angajamente.Visible = True
        End If
    End Sub

    ' ── The button of the work, in the ribbon (the view has none of its own in this page) ──────────────────

    ''' <summary>The ribbon button of the reception work changed (shown / hidden, enabled, its text): the window asks <see cref="ReceptiiActionVisible"/> and the others again.</summary>
    Public Event ReceptiiActionChanged As EventHandler

    ''' <summary>The view «Recepții» is the one on screen and its view offers the work (not while an invoice already covered is only looked at).</summary>
    Public ReadOnly Property ReceptiiActionVisible As Boolean
        Get
            Return String.Equals(detalii.navDetaliu.SelectedKey, FacturiCumparareDetaliiView.ViewReceptii, StringComparison.Ordinal) AndAlso receptii.ActionAvailable
        End Get
    End Property

    ''' <summary>The work can be done now (everything it needs is chosen).</summary>
    Public ReadOnly Property ReceptiiActionEnabled As Boolean
        Get
            Return receptii.ActionEnabled
        End Get
    End Property

    ''' <summary>«Leagă» / «Creează recepția» / «Modifică recepția», as the mode of the view says.</summary>
    Public ReadOnly Property ReceptiiActionText As String
        Get
            Return receptii.ActionText
        End Get
    End Property

    ''' <summary>The ribbon button was pressed. UI boundary: logs and swallows.</summary>
    Public Sub RunReceptiiAction()
        Try
            receptii.RunAction()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.RunReceptiiAction", ex)
        End Try
    End Sub

    ''' <summary>Slice 02EF-11 / KE21: «Genereaza ordonantare» is offered: the view «Recepții» is on screen and it saved a reception that nothing has changed since.</summary>
    Public ReadOnly Property OrdDisponibil As Boolean
        Get
            Return ReceptiiViewShown() AndAlso receptii.OrdDisponibil
        End Get
    End Property

    Public ReadOnly Property OrdCod As String
        Get
            Return receptii.OrdCod
        End Get
    End Property

    Public ReadOnly Property OrdIdrh As Integer
        Get
            Return receptii.OrdIdrh
        End Get
    End Property

    ''' <summary>Slice 02EF-12 / KE21: «Creeaza rezervare» is offered (a new or a modified reception on a chosen angajament).</summary>
    Public ReadOnly Property RezervareAvailable As Boolean
        Get
            Return ReceptiiViewShown() AndAlso receptii.RezervareAvailable
        End Get
    End Property

    Public ReadOnly Property RezervareEnabled As Boolean
        Get
            Return receptii.RezervareEnabled
        End Get
    End Property

    ''' <summary>The ribbon button «Creeaza rezervare» was pressed. UI boundary: logs and swallows.</summary>
    Public Sub RunRezervare()
        Try
            receptii.RunRezervare()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.RunRezervare", ex)
        End Try
    End Sub

    Private Function ReceptiiViewShown() As Boolean
        Return String.Equals(detalii.navDetaliu.SelectedKey, FacturiCumparareDetaliiView.ViewReceptii, StringComparison.Ordinal)
    End Function

    Private Sub KeForm_ActionStateChanged(sender As Object, e As EventArgs)
        Try
            RaiseEvent ReceptiiActionChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.KeForm_ActionStateChanged", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): a reception was made or linked: the list and the invoice are read again, and the view is given the invoice back.
    Private Async Sub KeForm_Committed(sender As Object, e As EventArgs)
        Try
            Dim k_id As Integer = _shownId
            If k_id = 0 Then Return
            _keStates.Clear()
            Await ReloadKeepingSelectionAsync(k_id).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareView.KeForm_Committed", ex)
        End Try
    End Sub

    ' The lines of the first view get their value with VAT and their classification (the server answers them in the same order).
    Private Sub FillKeLines(k_state As EFacturaKeStare)
        Dim k_rows As IReadOnlyList(Of KBotDataRow) = gridLinii().Rows
        If k_rows.Count <> k_state.Linii.Count Then Return
        For k_i As Integer = 0 To k_rows.Count - 1
            k_rows(k_i)("cuTva") = CDbl(k_state.Linii(k_i).ValoareCuTva)
            k_rows(k_i)("clsf") = k_state.Linii(k_i).Clsf
        Next
        gridLinii().Invalidate()
    End Sub

    Private Sub ClearKe()
        Interlocked.Increment(_keSeq)
        _ke = Nothing
        ' The angajament chosen last belongs to the invoice it was chosen on: another invoice starts without one.
        _keCod = String.Empty
        receptii.ClearInvoice()
        detalii.navDetaliu.SetItemVisible(FacturiCumparareDetaliiView.ViewReceptii, False)
        RaiseEvent ReceptiiActionChanged(Me, EventArgs.Empty)
    End Sub

    Private Sub Notify(k_text As String, k_icon As MessageBoxIcon)
        KBotMessage.Show(FindForm(), k_text, "Recepții", MessageBoxButtons.OK, k_icon)
    End Sub

End Class
