Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' Slices KE01..KE07 -- the links of the shown invoice with receptions: the view «Receptii», the windows that link to an existing reception, make a new one or change one. The rules (amounts with VAT, what is left, who may write)
' are the server's; the robot that writes in FOREXE is the shell's (<see cref="KeRobot"/>). Plan: docs/KE_PLAN.md.
Partial Public Class PrimiteView

    Private Const ViewReceptii As String = "receptii"

    Private Shared ReadOnly _markerPattern As New Regex("\(IDRH:\s*(\d+);\s*IDR:\s*(\d+)\)", RegexOptions.CultureInvariant)

    Private _ke As EFacturaKeStare
    Private _keSeq As Integer
    Private _keCod As String = String.Empty
    ' Slice KE12-04: the reception form of the shown invoice (the embedded variant of KeLegaForm), made when the first invoice is shown.
    Private _keForm As KeLegaForm
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
            GlobalErrorLog.Write("PrimiteView.LoadKeAsync", ex)
            If Not IsDisposed Then ShowKeProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.LoadKeAsync", ex)
            If Not IsDisposed Then ShowKeProblem("Legăturile cu recepțiile nu au putut fi citite. Detalii în jurnalul de erori.")
        End Try
    End Function

    Private Sub ShowKeProblem(k_text As String)
        _ke = Nothing
        navDetaliu.SetItemVisible(ViewReceptii, True)
        pgReceptii.ShowProblem(k_text)
        _keForm?.ClearInvoice()
    End Sub

    Private Sub ApplyKe(k_state As EFacturaKeStare)
        _ke = k_state
        ' An invoice that is already linked goes on with the angajament of its first link.
        If String.IsNullOrEmpty(_keCod) AndAlso k_state.Legaturi.Count > 0 Then _keCod = k_state.Legaturi(0).CodAngajament
        navDetaliu.SetItemVisible(ViewReceptii, True)
        pgReceptii.ShowState(k_state, KeRobot.Current IsNot Nothing)
        FillKeLines(k_state)
        ShowKeForm(k_state)
    End Sub

    ' The reception form works on the shown invoice (and on the angajament of the view, when there is one).
    Private Sub ShowKeForm(k_state As EFacturaKeStare)
        Dim k_invoice As EFacturaPrimita = If(_detail IsNot Nothing AndAlso _detail.Factura.IdPrimita = _shownId, _detail.Factura, _items.FirstOrDefault(Function(k_p) k_p.IdPrimita = _shownId))
        If k_invoice Is Nothing Then Return
        If _keForm Is Nothing Then
            _keForm = New KeLegaForm(_api, _gate, String.Empty, _year)
            _keForm.Embed()
            AddHandler _keForm.Committed, AddressOf KeForm_Committed
            pgReceptii.Embed(_keForm)
        End If
        _keForm.ShowInvoice(k_invoice, k_state, _angajamentCode)
    End Sub

    ' UI boundary (async Sub): a reception was made or linked: the list and the invoice are read again, and the form is given the invoice back.
    Private Async Sub KeForm_Committed(sender As Object, e As EventArgs)
        Try
            Dim k_id As Integer = _shownId
            If k_id = 0 Then Return
            Await ReloadKeepingSelectionAsync(k_id).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.KeForm_Committed", ex)
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
        pgReceptii.Clear()
        _keForm?.ClearInvoice()
        navDetaliu.SetItemVisible(ViewReceptii, False)
    End Sub

    Private Sub Notify(k_text As String, k_icon As MessageBoxIcon)
        KBotMessage.Show(FindForm(), k_text, "Recepții", MessageBoxButtons.OK, k_icon)
    End Sub

End Class
