Option Strict On
Imports System
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Downloads the update package while the operator watches (slice 0067). Modal; the
''' download starts when the window is shown and the result is the dialog result:
''' <c>OK</c> = the file is on disk and its SHA-256 matched, <c>Cancel</c> = the operator
''' stopped it (button, caption close, Esc), <c>Abort</c> = it failed and the failure was
''' already shown.
'''
''' <para>The form owns nothing but the window: the bytes go through
''' <see cref="IUpdateApi.DownloadAsync"/>, which also verifies the hash and deletes a bad
''' file, so a package that reaches <c>OK</c> is one the updater can trust.</para>
''' </summary>
Public Class UpdateProgressForm

    Private ReadOnly _api As IUpdateApi
    Private ReadOnly _info As UpdateInfo
    Private ReadOnly _destination As String
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _running As Boolean
    Private _finished As Boolean

    Public Sub New(api As IUpdateApi, info As UpdateInfo, destinationPath As String)
        If api Is Nothing Then Throw New ArgumentNullException(NameOf(api))
        If info Is Nothing Then Throw New ArgumentNullException(NameOf(info))
        If String.IsNullOrWhiteSpace(destinationPath) Then Throw New ArgumentException("Calea de destinație lipsește.", NameOf(destinationPath))
        _api = api
        _info = info
        _destination = destinationPath
        InitializeComponent()
        Try
            capBar.IconImage = My.Resources.kbot_64
        Catch ex As Exception
            ' The icon is cosmetic; its absence must not stop the update.
            GlobalErrorLog.Write("UpdateProgressForm.New", ex)
        End Try
        lblAntet.Text = "Se descarcă versiunea " & _info.Version & "…"
        lblDetaliu.Text = FormatBytes(0) & " din " & FormatBytes(_info.Size)
        bara.Maximum = 1000
        bara.Value = 0
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        If _running Then Return
        _running = True
        Try
            Dim ignored As Task = DownloadAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("UpdateProgressForm.OnShown", ex)
            Finish(DialogResult.Abort)
        End Try
    End Sub

    Private Async Function DownloadAsync() As Task
        Try
            Dim progress As New Progress(Of Long)(AddressOf OnProgress)
            Await _api.DownloadAsync(_destination, _info.Sha256, progress, _cts.Token)
            Finish(DialogResult.OK)
        Catch ex As OperationCanceledException
            DeleteQuietly()
            Finish(DialogResult.Cancel)
        Catch ex As ApiException
            ' Typed, already in the operator's language.
            DeleteQuietly()
            KBotMessage.Show(Me, "Descărcarea actualizării a eșuat:" & Environment.NewLine & ex.Message,
                             "Actualizare K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finish(DialogResult.Abort)
        Catch ex As Exception
            GlobalErrorLog.Write("UpdateProgressForm.DownloadAsync", ex)
            DeleteQuietly()
            KBotMessage.Show(Me, "Descărcarea actualizării a eșuat:" & Environment.NewLine & ex.Message,
                             "Actualizare K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finish(DialogResult.Abort)
        End Try
    End Function

    Private Sub OnProgress(bytes As Long)
        Try
            If IsDisposed Then Return
            If _info.Size > 0 Then
                bara.Value = CInt(Math.Min(1000L, bytes * 1000L \ _info.Size))
            End If
            lblDetaliu.Text = FormatBytes(bytes) & " din " & FormatBytes(_info.Size)
        Catch ex As Exception
            GlobalErrorLog.Write("UpdateProgressForm.OnProgress", ex)
        End Try
    End Sub

    Private Sub btnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Try
            Renunta()
        Catch ex As Exception
            GlobalErrorLog.Write("UpdateProgressForm.btnRenunta_Click", ex)
        End Try
    End Sub

    ' The caption bar's X and Esc both arrive here. While the download runs, closing means
    ' cancelling: veto the close, cancel the token, and let DownloadAsync finish the dialog.
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If _running AndAlso Not _finished Then
                e.Cancel = True
                Renunta()
                Return
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("UpdateProgressForm.OnFormClosing", ex)
        End Try
        MyBase.OnFormClosing(e)
    End Sub

    Private Sub Renunta()
        If _finished Then Return
        btnRenunta.Enabled = False
        lblAntet.Text = "Se oprește descărcarea…"
        _cts.Cancel()
    End Sub

    Private Sub Finish(result As DialogResult)
        If _finished Then Return
        _finished = True
        DialogResult = result
        Close()
    End Sub

    Private Sub DeleteQuietly()
        Try
            If File.Exists(_destination) Then File.Delete(_destination)
        Catch ex As Exception
            GlobalErrorLog.Write("UpdateProgressForm.DeleteQuietly", ex)
        End Try
    End Sub

    Friend Shared Function FormatBytes(bytes As Long) As String
        If bytes >= 1024L * 1024L Then Return (bytes / (1024.0 * 1024.0)).ToString("0.0") & " MB"
        If bytes >= 1024L Then Return (bytes / 1024.0).ToString("0") & " KB"
        Return bytes.ToString() & " B"
    End Function

    ' Dispose is the Designer's; the token source goes when the window has closed.
    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        Try
            _cts.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("UpdateProgressForm.OnFormClosed", ex)
        End Try
    End Sub
End Class
