Imports System
Imports System.IO
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Forexe
' RichTextBoxLogger și CertificateSelectionForm sunt în namespace global (din KBot.Forexe).

Public Class MainForm

    Private ReadOnly _forexeRunner As IForexeRunner
    Private _logger As RichTextBoxLogger
    Private _cts As CancellationTokenSource

    Public Sub New(forexeRunner As IForexeRunner)
        InitializeComponent()
        _forexeRunner = forexeRunner
        Me.Text = "K-BOT"
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Logger FOREXE legat de panoul de log + fișier în <AppDir>\Logs
        _logger = New RichTextBoxLogger(rtbLog)
        Dim logDir As String = Path.Combine(AppContext.BaseDirectory, "Logs")
        Directory.CreateDirectory(logDir)
        _logger.LogFilePath = Path.Combine(logDir, $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt")

        ' Atașează logger-ul FOREXE la runner (aceeași instanță singleton)
        DirectCast(_forexeRunner, ForexeRunner).AttachLogger(_logger)
    End Sub

    Private Async Sub btnConnect_Click(sender As Object, e As EventArgs) Handles btnConnect.Click
        Dim cert As X509Certificate2 = SelectCertificate()
        If cert Is Nothing Then Return   ' anulat / fără certificat

        btnConnect.Enabled = False
        pbProgress.Maximum = 100
        pbProgress.Value = 0
        _cts = New CancellationTokenSource()

        Dim job As New JobRequest With {
            .WorkflowName = "Conectare",
            .WflPath = Path.Combine(AppContext.BaseDirectory, "Workflows", "adlop - Conectare.wfl")
        }

        Dim progress As New Progress(Of Integer)(Sub(p) pbProgress.Value = Math.Min(p, pbProgress.Maximum))

        Try
            Dim result As JobResult = Await _forexeRunner.RunAsync(job, cert, progress, _cts.Token)
            If result.Success Then
                btnConnect.Text = "Conectat"
            Else
                btnConnect.Enabled = True
            End If
        Catch ex As Exception
            _logger.LogException(ex, "Eroare conectare (UI)")
            btnConnect.Enabled = True
        End Try
    End Sub

    ''' <summary>Picker de certificat în mod manual de PIN (utilizatorul tastează PIN-ul în dialogul Windows).</summary>
    Private Function SelectCertificate() As X509Certificate2
        Using dlg As New CertificateSelectionForm(manualPin:=True)
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                Return dlg.SelectedCertificate
            End If
        End Using
        Return Nothing
    End Function

End Class
