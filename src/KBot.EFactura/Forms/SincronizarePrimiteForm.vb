Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' Slice 00EF-18 -- «Sincronizează facturile primite»: the operator picks the period (7 to 60 days; ANAF lists at most 60) and the window
''' brings the new received invoices from ANAF into the database, a batch per call, with a progress line, until the server says nothing
''' is left. Nothing is doubled (the server skips what it already has), so it can be run again and again. No box with figures on
''' success: the line in the window says it is done; only problems become a notice. The server does everything; this window only repeats the call.
''' </summary>
Public Class SincronizarePrimiteForm

    Private Const Batch As Integer = 20
    Private Shared ReadOnly _days As Integer() = {7, 15, 30, 45, 60}

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _running As Boolean

    ''' <summary>True when at least one invoice was added: the list behind the window should be read again.</summary>
    Public ReadOnly Property AdaugatCeva As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        cmbZile.SelectedIndex = _days.Length - 1
    End Sub

    Private Sub SincronizarePrimiteForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            _cts.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("SincronizarePrimiteForm.FormClosing", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): logs and swallows; the failures show as a notice.
    Private Async Sub BtnPorneste_Click(sender As Object, e As EventArgs) Handles btnPorneste.Click
        Try
            If _running OrElse _api Is Nothing Then Return
            Dim k_days As Integer = _days(Math.Max(0, Math.Min(_days.Length - 1, cmbZile.SelectedIndex)))
            SetRunning(True)
            ntfMesaj.Clear()
            ntfMesaj.Visible = False
            Dim k_problems As New List(Of String)()
            Dim k_done As Integer
            Dim k_total As Integer
            Do
                Dim k_answer As EFacturaSincronizare = Await _gate.RunAsync(
                    Function() _api.SyncPrimiteAsync(k_days, Batch, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                If k_total = 0 Then k_total = k_answer.Adaugate + k_answer.Ramase + k_answer.Erori.Count
                k_done += k_answer.Adaugate + k_answer.Erori.Count
                If k_answer.Adaugate > 0 Then _AdaugatCeva = True
                k_problems.AddRange(k_answer.Erori)
                If k_total > 0 Then
                    lblProgres.Text = $"Se aduc facturile de la ANAF… {Math.Min(k_done, k_total).ToString(CultureInfo.CurrentCulture)} din {k_total.ToString(CultureInfo.CurrentCulture)}"
                End If
                ' Stop when nothing is left, or when a whole batch brought nothing (the same messages would come back forever).
                If k_answer.Ramase = 0 OrElse k_answer.Adaugate = 0 Then Exit Do
            Loop
            lblProgres.Text = If(k_problems.Count = 0, "Facturile primite sunt la zi.", "Sincronizarea s-a încheiat, dar unele mesaje nu au putut fi citite.")
            If k_problems.Count > 0 Then
                ntfMesaj.Show("Nu s-au putut citi:" & vbLf & String.Join(vbLf, k_problems.Take(4)) &
                              If(k_problems.Count > 4, vbLf & "…", String.Empty), NoticeKind.Warning)
                ntfMesaj.Visible = True
            End If
        Catch ex As OperationCanceledException
            ' The window was closed while the sync ran.
        Catch ex As ApiException
            GlobalErrorLog.Write("SincronizarePrimiteForm.BtnPorneste_Click", ex)
            lblProgres.Text = "Sincronizarea s-a oprit."
            ntfMesaj.Show(ex.Message, NoticeKind.Error)
            ntfMesaj.Visible = True
        Catch ex As Exception
            GlobalErrorLog.Write("SincronizarePrimiteForm.BtnPorneste_Click", ex)
            lblProgres.Text = "Sincronizarea s-a oprit."
            ntfMesaj.Show("Sincronizarea nu a putut fi dusă la capăt. Detalii în jurnalul de erori.", NoticeKind.Error)
            ntfMesaj.Visible = True
        Finally
            If Not IsDisposed Then SetRunning(False)
        End Try
    End Sub

    Private Sub SetRunning(k_running As Boolean)
        _running = k_running
        btnPorneste.Enabled = Not k_running
        cmbZile.Enabled = Not k_running
        barProgres.Running = k_running
        If k_running Then lblProgres.Text = "Se citește lista de mesaje de la ANAF…"
    End Sub

End Class
