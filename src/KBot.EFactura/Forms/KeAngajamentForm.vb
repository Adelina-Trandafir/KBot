Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' Slice KE02 -- «Alege angajamentul»: the list of the unit's angajamente (at most the newest 300) with a search box; the accountant picks one.
''' Used when the supplier of the invoice has no DDF (the invoice is not tied to any angajament yet) or when it must go to another one. The window only
''' chooses (<see cref="Ales"/>); what is done with the choice is the caller's. The search is the SERVER's: a pause after the last key (no timer: a short
''' wait that a newer key cancels), then the server looks in ALL the angajamente, not only in the newest 300.
''' </summary>
Public Class KeAngajamentForm

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _all As New List(Of EFacturaKeAngajamentAlegere)()
    Private _searchSeq As Integer

    ''' <summary>The angajament picked (valid when the window closed with OK).</summary>
    Public ReadOnly Property Ales As EFacturaKeAngajamentAlegere

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
        lblAntet.Text = $"Alege angajamentul pe care se face recepția facturii {k_invoiceLabel}."
        AddHandler gridAng.CellDoubleClick, AddressOf GridAng_CellDoubleClick
        AddHandler gridAng.SelectionChanged, AddressOf GridAng_SelectionChanged
    End Sub

    ' UI boundary (Load, async Sub): logs and swallows; a failure becomes a notice.
    Private Async Sub KeAngajamentForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            _all = Await _gate.RunAsync(Function() _api.GetKeAngajamenteAsync(Nothing, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            FillGrid()
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeAngajamentForm.KeAngajamentForm_Load", ex)
            ntfMesaj.Show(ex.Message, NoticeKind.Error)
            ntfMesaj.Visible = True
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamentForm.KeAngajamentForm_Load", ex)
            ntfMesaj.Show("Lista angajamentelor nu a putut fi citită. Detalii în jurnalul de erori.", NoticeKind.Error)
            ntfMesaj.Visible = True
        End Try
    End Sub

    Private Sub KeAngajamentForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            _cts.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamentForm.KeAngajamentForm_FormClosing", ex)
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
            Dim k_list As List(Of EFacturaKeAngajamentAlegere) = Await _gate.RunAsync(
                Function() _api.GetKeAngajamenteAsync(If(k_text.Length = 0, Nothing, k_text), _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _searchSeq Then Return
            _all = k_list
            ntfMesaj.Visible = False
            FillGrid()
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeAngajamentForm.TxtCauta_TextChanged", ex)
            ntfMesaj.Show(ex.Message, NoticeKind.Error)
            ntfMesaj.Visible = True
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamentForm.TxtCauta_TextChanged", ex)
            ntfMesaj.Show("Căutarea nu a putut fi făcută. Detalii în jurnalul de erori.", NoticeKind.Error)
            ntfMesaj.Visible = True
        End Try
    End Sub

    Private Sub FillGrid()
        gridAng.BeginUpdate()
        Try
            gridAng.ClearRows()
            For Each k_a As EFacturaKeAngajamentAlegere In _all
                Dim k_row As KBotDataRow = gridAng.AddRow()
                k_row.Tag = k_a
                k_row("cod") = k_a.CodAngajament
                k_row("descriere") = k_a.Eticheta
                k_row("stare") = k_a.Stare
                k_row("partener") = k_a.Parteneri
                k_row("data") = If(k_a.DataCreare.HasValue, k_a.DataCreare.Value.ToString("dd.MM.yyyy", _ro), String.Empty)
            Next
        Finally
            gridAng.EndUpdate()
        End Try
        btnAlege.Enabled = False
    End Sub

    Private Sub GridAng_SelectionChanged(sender As Object, e As EventArgs)
        Try
            btnAlege.Enabled = TryCast(gridAng.CurrentRow?.Tag, EFacturaKeAngajamentAlegere) IsNot Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamentForm.GridAng_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub GridAng_CellDoubleClick(sender As Object, e As KBotCellEventArgs)
        Try
            If TryCast(gridAng.CurrentRow?.Tag, EFacturaKeAngajamentAlegere) IsNot Nothing Then BtnAlege_Click(sender, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamentForm.GridAng_CellDoubleClick", ex)
        End Try
    End Sub

    Private Sub BtnAlege_Click(sender As Object, e As EventArgs) Handles btnAlege.Click
        Try
            Dim k_picked As EFacturaKeAngajamentAlegere = TryCast(gridAng.CurrentRow?.Tag, EFacturaKeAngajamentAlegere)
            If k_picked Is Nothing Then Return
            _Ales = k_picked
            DialogResult = DialogResult.OK
        Catch ex As Exception
            GlobalErrorLog.Write("KeAngajamentForm.BtnAlege_Click", ex)
        End Try
    End Sub

End Class
