Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe
' RichTextBoxLogger și CertificateSelectionForm sunt în namespace global (din KBot.Forexe).

Public Class MainForm

    Private ReadOnly _forexeRunner As IForexeRunner
    Private ReadOnly _session As SessionContext
    Private ReadOnly _apiClient As IApiClient
    Private _logger As RichTextBoxLogger
    Private _cts As CancellationTokenSource

    Public Sub New(forexeRunner As IForexeRunner, session As SessionContext, apiClient As IApiClient)
        InitializeComponent()
        _forexeRunner = forexeRunner
        _session = session
        _apiClient = apiClient
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
                ' Sesiune vie => activăm job-urile pe sesiunea existentă.
                btnListaAngajamente.Enabled = True
            Else
                btnConnect.Enabled = True
            End If
        Catch ex As Exception
            _logger.LogException(ex, "Eroare conectare (UI)")
            btnConnect.Enabled = True
        End Try
    End Sub

    ''' <summary>
    ''' Rulează ListaAngajamente pe sesiunea existentă, mapează rezultatul tabelar
    ''' și îl trimite la /api/forexe/angajamente/upsert. Fără sesiune vie e no-op;
    ''' fără DbName setat (login încă neimplementat) se oprește după mapare.
    ''' </summary>
    Private Async Sub btnListaAngajamente_Click(sender As Object, e As EventArgs) Handles btnListaAngajamente.Click
        If Not DirectCast(_forexeRunner, ForexeRunner).HasLiveSession Then
            _logger.LogWarning("Nicio sesiune activă — apasă întâi 'Conectare'.")
            Return
        End If

        btnListaAngajamente.Enabled = False
        pbProgress.Value = 0
        _cts = New CancellationTokenSource()

        Dim progress As New Progress(Of Integer)(Sub(p) pbProgress.Value = Math.Min(p, pbProgress.Maximum))

        Try
            Dim job As JobRequest = JobBuilder.BuildListaAngajamente(_session)
            Dim result As JobResult = Await _forexeRunner.RunJobAsync(job, progress, _cts.Token)
            If Not result.Success Then
                _logger.LogError($"ListaAngajamente eșuat: {result.Message}")
                Return
            End If

            Dim rows As List(Of Dictionary(Of String, String)) = Nothing
            If Not result.Tables.TryGetValue(WorkflowCatalog.ListaAngajamenteTable, rows) Then
                _logger.LogWarning($"Nu s-a găsit tabelul '{WorkflowCatalog.ListaAngajamenteTable}' în rezultat (0 rânduri scrape).")
                Return
            End If

            Dim mapped As List(Of Angajament) = AngajamentMapper.FromListaAngajamenteResult(rows)
            _logger.LogInfo($"ListaAngajamente: {mapped.Count} rânduri mapate (din {rows.Count} brute).")

            ' Guard: fără DbName (populat la login) nu putem ținti baza unității.
            If String.IsNullOrEmpty(_session.DbName) Then
                _logger.LogWarning("DbName nesetat pe sesiune — sar peste upsert (login încă neimplementat).")
                Return
            End If

            Dim resp As String = Await _apiClient.UpsertAngajamenteAsync(_session.DbName, mapped, _cts.Token)
            _logger.LogSuccess($"Upsert reușit: {mapped.Count} angajamente în '{_session.DbName}'. Răspuns server: {resp}")

        Catch ex As Exception
            _logger.LogException(ex, "Eroare ListaAngajamente (UI)")
        Finally
            btnListaAngajamente.Enabled = True
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
