Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' Slice 00EF-20 -- «Leagă de un DDF»: the list of the unit's fundamentări (at most the newest 300) with a search box; the operator picks one.
''' The window only chooses (<see cref="Ales"/>); the link itself is written by <see cref="PrimiteView"/>. The search is the SERVER's: a pause
''' after the last key (no timer: a short wait that a newer key cancels), then the server looks in ALL the fundamentari, not only in the newest 300.
''' </summary>
Public Class AlegeDdfForm

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _all As New List(Of EFacturaDdfAlegere)()
    Private _searchSeq As Integer

    ''' <summary>The fundamentare picked (valid when the window closed with OK).</summary>
    Public ReadOnly Property Ales As EFacturaDdfAlegere

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_invoiceLabel">The invoice being linked, shown in the header (number and supplier).</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate, k_invoiceLabel As String)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        lblAntet.Text = $"Alege fundamentarea de care se leagă factura {k_invoiceLabel}."
        AddHandler gridDdf.CellDoubleClick, AddressOf GridDdf_CellDoubleClick
        AddHandler gridDdf.SelectionChanged, AddressOf GridDdf_SelectionChanged
    End Sub

    ' UI boundary (Load, async Sub): logs and swallows; a failure becomes a notice.
    Private Async Sub AlegeDdfForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            _all = Await _gate.RunAsync(Function() _api.GetDdfAlegereAsync(Nothing, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            FillGrid()
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("AlegeDdfForm.AlegeDdfForm_Load", ex)
            ntfMesaj.Show(ex.Message, NoticeKind.Error)
            ntfMesaj.Visible = True
        Catch ex As Exception
            GlobalErrorLog.Write("AlegeDdfForm.AlegeDdfForm_Load", ex)
            ntfMesaj.Show("Lista fundamentărilor nu a putut fi citită. Detalii în jurnalul de erori.", NoticeKind.Error)
            ntfMesaj.Visible = True
        End Try
    End Sub

    Private Sub AlegeDdfForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            _cts.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("AlegeDdfForm.AlegeDdfForm_FormClosing", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): logs and swallows; a failure becomes a notice. A newer key cancels the older wait and the older answer.
    Private Async Sub TxtCauta_TextChanged(sender As Object, e As EventArgs) Handles txtCauta.TextChanged
        Try
            If _api Is Nothing Then Return
            Dim k_mySeq As Integer = Interlocked.Increment(_searchSeq)
            Await Task.Delay(350, _cts.Token).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _searchSeq Then Return
            Dim k_text As String = txtCauta.Text.Trim()
            Dim k_list As List(Of EFacturaDdfAlegere) = Await _gate.RunAsync(
                Function() _api.GetDdfAlegereAsync(If(k_text.Length = 0, Nothing, k_text), _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _searchSeq Then Return
            _all = k_list
            ntfMesaj.Visible = False
            FillGrid()
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("AlegeDdfForm.TxtCauta_TextChanged", ex)
            ntfMesaj.Show(ex.Message, NoticeKind.Error)
            ntfMesaj.Visible = True
        Catch ex As Exception
            GlobalErrorLog.Write("AlegeDdfForm.TxtCauta_TextChanged", ex)
            ntfMesaj.Show("Căutarea nu a putut fi făcută. Detalii în jurnalul de erori.", NoticeKind.Error)
            ntfMesaj.Visible = True
        End Try
    End Sub

    Private Sub FillGrid()
        gridDdf.BeginUpdate()
        Try
            gridDdf.ClearRows()
            For Each k_d As EFacturaDdfAlegere In _all
                Dim k_row As KBotDataRow = gridDdf.AddRow()
                k_row.Tag = k_d
                k_row("cod") = k_d.CodAngajament
                k_row("obiect") = k_d.Obiect
                k_row("partener") = k_d.Partener
                k_row("cf") = k_d.CodFiscal
            Next
        Finally
            gridDdf.EndUpdate()
        End Try
        btnLeaga.Enabled = False
    End Sub

    Private Sub GridDdf_SelectionChanged(sender As Object, e As EventArgs)
        Try
            btnLeaga.Enabled = TryCast(gridDdf.CurrentRow?.Tag, EFacturaDdfAlegere) IsNot Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("AlegeDdfForm.GridDdf_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub GridDdf_CellDoubleClick(sender As Object, e As KBotCellEventArgs)
        Try
            If TryCast(gridDdf.CurrentRow?.Tag, EFacturaDdfAlegere) IsNot Nothing Then BtnLeaga_Click(sender, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("AlegeDdfForm.GridDdf_CellDoubleClick", ex)
        End Try
    End Sub

    Private Sub BtnLeaga_Click(sender As Object, e As EventArgs) Handles btnLeaga.Click
        Try
            Dim k_picked As EFacturaDdfAlegere = TryCast(gridDdf.CurrentRow?.Tag, EFacturaDdfAlegere)
            If k_picked Is Nothing Then Return
            _Ales = k_picked
            DialogResult = DialogResult.OK
        Catch ex As Exception
            GlobalErrorLog.Write("AlegeDdfForm.BtnLeaga_Click", ex)
        End Try
    End Sub

End Class
