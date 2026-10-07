Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming
Imports KBot.Xfa

''' <summary>
''' The bench for slice 0078: the whole signing round trip, on the REAL pieces the DDF / ORD pages
''' use -- <see cref="ReaderHostPreview"/> (the operator's engine, or the hosted window when
''' «Forțează fereastra găzduită» is ticked; the setting itself is never changed),
''' <see cref="PdfSigningSession"/> for the upload and
''' the server routes for the read-back.
'''
''' <para><b>What to watch.</b> The log on the right mirrors <c>adobe_preview.log</c> live: every
''' dialog the Save As trap sees, what it wrote into the file-name box, whether it pressed Save and
''' answered «replace?». After each save the bench reads the file itself and lists the signed
''' fields, the roles and the sha -- independent of the upload, so a trap that works and an upload
''' that fails are told apart.</para>
'''
''' <para><b>The order of a save (slice 0078-05).</b> In the hosted Adobe window the «Save As»
''' comes first and the signed file is written only after it (after the token's PIN). The
''' «[Disc]» lines (<see cref="PdfSaveTimeline"/>) show every file event and size change counted
''' from the dialog's close, and the signatures once the file stops changing; the upload line says
''' how long after the dialog the session sent it. «Doar simulează încărcarea» runs the same session
''' on <see cref="RecordingPdfApi"/>: nothing reaches the server. «Forțează fereastra găzduită»
''' (on by default) makes this bench's viewer use the hosted window whatever «Setări» says.</para>
'''
''' <para><b>Printing (slice 0099).</b> Every document opened here is watched in the print queue
''' like on the real pages: print it from Adobe (Ctrl+P) and the journal shows the print lines of
''' <c>adobe_preview.log</c>, marked «[Adobe]» (the Print window seen, the job found, the jobs that
''' were NOT taken for this document), and one line of the bench per detected print. With an id
''' and a login -- and not in the simulated-upload mode -- the print is also counted on the server,
''' on the REAL row of that id (<c>PrintCount</c>), and the server's answer comes back as an
''' «[Adobe]» line.</para>
'''
''' <para><b>Always on a copy.</b> A local PDF is copied into <c>TempPdf\Banc\</c> first (wiped at
''' every start); the original is never touched. «Deschide copia de pe server» downloads into the
''' same folder -- that is the «retrieve» half of the round trip.</para>
''' </summary>
Public NotInheritable Class PdfSigningHarnessForm

    Private Const BenchFolder As String = "Banc"

    Private ReadOnly _api As IApiClient
    Private ReadOnly _session As SessionContext
    Private ReadOnly _loginFactory As Func(Of LoginForm)
    Private ReadOnly _log As Action(Of String)
    Private _signing As PdfSigningSession
    Private _timeline As PdfSaveTimeline
    Private _path As String
    Private _logHooked As Boolean

    Public Sub New(api As IApiClient, session As SessionContext, loginFactory As Func(Of LoginForm),
                   log As Action(Of String))
        InitializeComponent()
        _api = api
        _session = session
        _loginFactory = loginFactory
        _log = log
        cmbTip.SelectedIndex = 0
        AddHandler AdobeHostLog.LineWritten, AddressOf OnAdobeLogLine
        _logHooked = True
        AddHandler preview.DocumentSaved, AddressOf OnDocumentSaved
        AddHandler preview.SaveCancelled, AddressOf OnSaveCancelled
        AddHandler preview.DocumentPrinted, AddressOf OnDocumentPrinted
        ApplyEngineChoice()
        UpdateSessionLabel()
        Write("Banc pornit. Setarea motorului Adobe din «Setări» nu se schimbă; bancul poate doar să-și " &
              "forțeze propriul vizualizator pe fereastra găzduită.")
    End Sub

    ' ── Buttons (UI boundaries: log and swallow) ────────────────────────────────
    Private Sub btnAutentificare_Click(sender As Object, e As EventArgs) Handles btnAutentificare.Click
        Try
            Using login As LoginForm = _loginFactory.Invoke()
                If login.ShowDialog(Me) = DialogResult.OK Then Write("Autentificat.")
            End Using
            UpdateSessionLabel()
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.btnAutentificare_Click", ex)
            Write("Autentificarea a eșuat: " & ex.Message)
        End Try
    End Sub

    Private Sub btnAlege_Click(sender As Object, e As EventArgs) Handles btnAlege.Click
        Try
            If dlgAlege.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim target As String = BenchPath()
            ReleaseDocument()
            File.Copy(dlgAlege.FileName, target, overwrite:=True)
            Write($"Copiat «{dlgAlege.FileName}» în «{target}» (originalul nu se atinge).")
            OpenAsync(target, knownServerSha:=Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.btnAlege_Click", ex)
            Write("Nu s-a putut deschide: " & ex.Message)
        End Try
    End Sub

    Private Async Sub btnDinServer_Click(sender As Object, e As EventArgs) Handles btnDinServer.Click
        Try
            Dim id As Integer
            If Not TryReadId(id) OrElse Not RequireLogin() Then Return
            Dim result As PdfDownloadResult = Await DownloadAsync(id).ConfigureAwait(True)
            If result.Status <> PdfDownloadStatus.Content Then
                Write($"Serverul nu are un PDF pentru {Kind()} {id}.")
                Return
            End If
            Dim target As String = BenchPath()
            ReleaseDocument()
            File.WriteAllBytes(target, result.Bytes)
            Write($"Descărcat de pe server: {result.Bytes.Length:N0} octeți, sha {ShortSha(result.Sha256)}.")
            LogSignatures("Server", result.Bytes)
            OpenAsync(target, knownServerSha:=result.Sha256)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.btnDinServer_Click", ex)
            Write("Descărcarea a eșuat: " & ex.Message)
        End Try
    End Sub

    Private Async Sub btnCompara_Click(sender As Object, e As EventArgs) Handles btnCompara.Click
        Try
            Dim id As Integer
            If Not TryReadId(id) OrElse Not RequireLogin() Then Return
            If String.IsNullOrEmpty(_path) OrElse Not File.Exists(_path) Then
                Write("Nu e niciun document local deschis.")
                Return
            End If
            Dim local As Byte() = SignedPdfFiles.ReadShared(_path)
            Dim localSha As String = PdfHash.Compute(local)
            Dim result As PdfDownloadResult = Await DownloadAsync(id).ConfigureAwait(True)
            If result.Status <> PdfDownloadStatus.Content Then
                Write($"Serverul nu are un PDF pentru {Kind()} {id}. Local: sha {ShortSha(localSha)}.")
                Return
            End If
            LogSignatures("Local", local)
            LogSignatures("Server", result.Bytes)
            If PdfHash.AreEqual(localSha, result.Sha256) Then
                Write($"IDENTIC: copia locală și cea de pe server au aceeași sumă ({ShortSha(localSha)}), " &
                      $"{local.Length:N0} octeți.")
            Else
                Write($"DIFERIT: local {ShortSha(localSha)} ({local.Length:N0} octeți), " &
                      $"server {ShortSha(result.Sha256)} ({result.Bytes.Length:N0} octeți).")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.btnCompara_Click", ex)
            Write("Comparația a eșuat: " & ex.Message)
        End Try
    End Sub

    Private Sub btnJurnal_Click(sender As Object, e As EventArgs) Handles btnJurnal.Click
        txtJurnal.Clear()
    End Sub

    ' Slice 0078-15 check (temporary, ACTIVEX-CHECK): the operator sees the document loaded -- the viewer logs the moment.
    Private Sub btnVad_Click(sender As Object, e As EventArgs) Handles btnVad.Click
        Try
            Write("Operatorul: documentul se vede încărcat la " & DateTime.Now.ToString("HH:mm:ss.fff") & ".")
            preview.MarkOperatorSeen()
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.btnVad_Click", ex)
        End Try
    End Sub

    Private Sub chkGazduita_CheckedChanged(sender As Object, e As EventArgs) Handles chkGazduita.CheckedChanged
        Try
            ApplyEngineChoice()
            Write("Motorul se schimbă de la documentul următor: " & lblMotor.Text)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.chkGazduita_CheckedChanged", ex)
        End Try
    End Sub

    ' The bench's own viewer only: nothing is written to the settings.
    Private Sub ApplyEngineChoice()
        preview.ForcedEngine = If(chkGazduita.Checked, AdobePreviewEngine.WindowHost, CType(Nothing, AdobePreviewEngine?))
        lblMotor.Text = "Motor: " & AdobeViewerSettings.EngineLabel(preview.Engine) &
                        If(chkGazduita.Checked, " (forțat de banc)", " (din «Setări»)")
    End Sub

    ' ── Opening a document ──────────────────────────────────────────────────────
    ''' <summary>
    ''' Shows the copy and, when uploading is on, starts a real signing session against the id.
    ''' The server's current sha is the upload precedent: taken from the download when there was
    ''' one, asked for otherwise (404 = no row yet).
    ''' </summary>
    Private Async Sub OpenAsync(path As String, knownServerSha As String)
        Try
            _path = path
            lblFisier.Text = System.IO.Path.GetFileName(path)
            LogSignatures("Local", SignedPdfFiles.ReadShared(path))
            _timeline = New PdfSaveTimeline(path, Kind(), AddressOf Write)
            _timeline.Start()

            Dim id As Integer = 0
            If chkSimuleaza.Checked Then
                ' A real session on a recording client: no server, no login, nothing written.
                If Not Integer.TryParse(txtId.Text.Trim(), id) OrElse id <= 0 Then id = 1
                _signing = New PdfSigningSession(KindEnum(), id, path, path, String.Empty,
                                                 RecordingPdfApi.ForUploads(AddressOf OnSimulatedUpload))
                AddHandler _signing.Completed, AddressOf OnSigningCompleted
                _signing.Begin()
                Write($"Sesiune de semnare SIMULATĂ: {Kind()} {id}, nimic nu ajunge pe server.")
                preview.Signing = _signing
                preview.PrintTarget = BenchPrintTarget()
                preview.ShowDocument(path, exists:=True)
                Return
            End If
            If chkIncarca.Checked Then
                If Not TryReadId(id) OrElse Not RequireLogin() Then
                    Write("Încărcarea e bifată, dar lipsește id-ul sau autentificarea: documentul se " &
                          "deschide DOAR cu capcana, fără încărcare.")
                    id = 0
                End If
            End If

            If id > 0 Then
                Dim serverSha As String = knownServerSha
                If serverSha Is Nothing Then
                    Dim result As PdfDownloadResult = Await DownloadAsync(id).ConfigureAwait(True)
                    serverSha = If(result.Status = PdfDownloadStatus.Content, result.Sha256, String.Empty)
                End If
                ' Same shape as the views: displayed path = cache path, so nothing is copied twice.
                _signing = New PdfSigningSession(KindEnum(), id, path, path, serverSha, _api)
                AddHandler _signing.Completed, AddressOf OnSigningCompleted
                _signing.Begin()
                Write($"Sesiune de semnare: {Kind()} {id}, precedent " &
                      If(String.IsNullOrEmpty(serverSha), "«fără rând pe server»", ShortSha(serverSha)) & ".")
            Else
                Write("Fără sesiune de încărcare: se testează doar capcana «Salvare ca».")
            End If

            preview.Signing = _signing
            preview.PrintTarget = BenchPrintTarget()
            preview.ShowDocument(path, exists:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.OpenAsync", ex)
            Write("Deschiderea a eșuat: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0099: where a print of the document about to be shown is counted. Nothing = the bench
    ''' only DETECTS the print (simulation, no id, no login). Independent of the upload tick: a
    ''' document opened only for the trap is counted too when the id and the login are there.
    ''' </summary>
    Private Function BenchPrintTarget() As PdfPrintTarget
        If chkSimuleaza.Checked Then
            Write("Tipărire: simulare — o tipărire se DETECTEAZĂ, dar nu se numără pe server.")
            Return Nothing
        End If
        Dim id As Integer
        If Not Integer.TryParse(txtId.Text.Trim(), id) OrElse id <= 0 OrElse
           _session Is Nothing OrElse Not _session.IsAuthenticated Then
            Write("Tipărire: fără id sau fără autentificare, o tipărire se DETECTEAZĂ, dar nu se numără pe server.")
            Return Nothing
        End If
        Dim printed As PrintedDocumentKind = If(KindEnum() = PdfDocKind.Ord, PrintedDocumentKind.Ord, PrintedDocumentKind.Ddf)
        Dim target As PdfPrintTarget = PdfPrintTarget.Create(printed, id, _api)
        If target Is Nothing Then
            Write("Tipărire: clientul API al bancului nu are ruta de numărare — o tipărire doar se DETECTEAZĂ.")
        Else
            Write($"Tipărire: fiecare tipărire a acestui document se numără pe server, pe rândul REAL «{target.Describe()}» (PrintCount).")
        End If
        Return target
    End Function

    ''' <summary>Adobe holds the file open: the preview lets go before the copy is overwritten.</summary>
    Private Sub ReleaseDocument()
        If preview.IsSaving Then Write("ATENȚIE: Adobe încă salvează documentul — eliberarea așteaptă salvarea (max. 5 s).")
        EndSigning()
        _timeline?.Dispose()
        _timeline = Nothing
        preview.Signing = Nothing
        preview.PrintTarget = Nothing
        preview.Clear()
        _path = Nothing
        lblFisier.Text = "Niciun document"
    End Sub

    Private Sub EndSigning()
        If _signing Is Nothing Then Return
        RemoveHandler _signing.Completed, AddressOf OnSigningCompleted
        _signing.Dispose()
        _signing = Nothing
    End Sub

    ' ── Events ──────────────────────────────────────────────────────────────────
    ' A trapped «Save As» CLOSED -- which is not yet «the file is written» (slice 0078-05). The
    ' timeline reads the file ITSELF as it changes, whatever the session decides.
    Private Sub OnDocumentSaved(path As String)
        Try
            Write("SALVAT de capcană (dialogul s-a închis): " & path)
            _timeline?.MarkDialogClosed()
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.OnDocumentSaved", ex)
            Write("Cronologia salvării a eșuat: " & ex.Message)
        End Try
    End Sub

    ' The recording client got an upload from the session (UI thread). What would have been sent.
    Private Sub OnSimulatedUpload(docType As String, id As Integer, bytes As Byte(), semnatura As String,
                                  records As IReadOnlyList(Of PdfSignatureRecord))
        Try
            Dim fields As String = If(records Is Nothing OrElse records.Count = 0, "—",
                                      String.Join(", ", records.Select(Function(r) $"{r.camp} ({r.rol}, {r.semnatar})")))
            Write($"ÎNCĂRCARE SIMULATĂ {docType} {id}: {bytes.Length:N0} octeți, roluri «{semnatura}», " &
                  $"semnături noi: {fields}" & If(_timeline Is Nothing, "", _timeline.SinceDialog()) & ".")
            LogSignatures("  Trimis", bytes)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.OnSimulatedUpload", ex)
        End Try
    End Sub

    Private Sub OnSaveCancelled(reason As String)
        Write("SALVARE ANULATĂ de capcană: " & reason)
    End Sub

    ' Slice 0099: the print queue showed a job of the document on screen (UI thread). The server's
    ' answer, when the print is counted, follows as an «[Adobe]» line (PdfPrintTarget writes it).
    Private Sub OnDocumentPrinted(job As AdobePrintJob)
        Try
            Write($"TIPĂRIRE detectată: imprimanta «{job.Printer}», lucrarea {job.JobId} «{job.JobName}»" &
                  If(preview.PrintTarget Is Nothing, " — nenumărată pe server (vezi mai sus de ce).",
                     " — trimisă la server pentru numărare."))
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.OnDocumentPrinted", ex)
        End Try
    End Sub

    ' The session already showed its message to the operator; the bench adds it to the log.
    Private Sub OnSigningCompleted(session As PdfSigningSession, outcome As PdfSigningOutcome)
        Try
            If outcome Is Nothing Then Return
            Write($"ÎNCĂRCARE: {outcome.Status} — roluri «{outcome.Semnatura}», sha {ShortSha(outcome.NewSha)}" &
                  If(_timeline Is Nothing, "", _timeline.SinceDialog()) & ". " & outcome.Message)
            SigningMessages.ShowOutcome(Me, outcome)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.OnSigningCompleted", ex)
        End Try
    End Sub

    ' Raised on the writer's thread (the trap and the session write from the UI thread, but not
    ' every writer does). Never throws: AdobeHostLog swallows a failing listener anyway.
    Private Sub OnAdobeLogLine(line As String)
        If IsDisposed OrElse Not IsHandleCreated Then Return
        If InvokeRequired Then
            BeginInvoke(New Action(Of String)(AddressOf AppendLine), "[Adobe] " & line)
        Else
            AppendLine("[Adobe] " & line)
        End If
    End Sub

    ' ── Helpers ─────────────────────────────────────────────────────────────────
    Private Sub LogSignatures(label As String, bytes As Byte())
        Try
            ' Slice 0078-04: shared with the section B bench (adds the integrity lines).
            For Each line As String In PdfSignatureReport.Lines(label, bytes, Kind())
                Write(line)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.LogSignatures", ex)
            Write($"{label}: semnăturile nu au putut fi citite: {ex.Message}")
        End Try
    End Sub

    Private Function DownloadAsync(id As Integer) As Task(Of PdfDownloadResult)
        If KindEnum() = PdfDocKind.Ddf Then Return _api.DownloadDdfPdfAsync(id, Nothing, CancellationToken.None)
        Return _api.DownloadOrdPdfAsync(id, Nothing, CancellationToken.None)
    End Function

    Private Function BenchPath() As String
        Dim folder As String = Path.Combine(TempPdfStore.EnsureRoot(), BenchFolder)
        Directory.CreateDirectory(folder)
        Dim id As String = If(String.IsNullOrWhiteSpace(txtId.Text), "0", txtId.Text.Trim())
        Return Path.Combine(folder, $"BANC_{Kind()}_{id}.PDF")
    End Function

    Private Function Kind() As String
        Return If(cmbTip.SelectedIndex = 1, "ORD", "DDF")
    End Function

    Private Function KindEnum() As PdfDocKind
        Return If(cmbTip.SelectedIndex = 1, PdfDocKind.Ord, PdfDocKind.Ddf)
    End Function

    Private Function TryReadId(ByRef id As Integer) As Boolean
        If Integer.TryParse(txtId.Text.Trim(), id) AndAlso id > 0 Then Return True
        Write("Scrieți id-ul documentului (IDREV pentru DDF, IDORDP pentru ORD).")
        Return False
    End Function

    Private Function RequireLogin() As Boolean
        If _session IsNot Nothing AndAlso _session.IsAuthenticated Then Return True
        Write("Nu sunteți autentificat — apăsați «Autentificare…».")
        Return False
    End Function

    Private Sub UpdateSessionLabel()
        If _session IsNot Nothing AndAlso _session.IsAuthenticated Then
            lblSesiune.Text = $"Autentificat: {_session.OperatorName} @ {_session.DbName}"
        Else
            lblSesiune.Text = "Neautentificat"
        End If
    End Sub

    Private Shared Function ShortSha(sha As String) As String
        If String.IsNullOrEmpty(sha) Then Return "—"
        Return If(sha.Length > 12, sha.Substring(0, 12), sha)
    End Function

    Private Sub Write(line As String)
        AppendLine(line)
        _log?.Invoke(line)
    End Sub

    Private Sub AppendLine(line As String)
        If IsDisposed Then Return
        txtJurnal.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") & "  " & line & Environment.NewLine)
    End Sub

    ' Called from Dispose (see Designer). Must not throw there.
    Private Sub Inchide()
        Try
            If _logHooked Then
                RemoveHandler AdobeHostLog.LineWritten, AddressOf OnAdobeLogLine
                _logHooked = False
            End If
            EndSigning()
            _timeline?.Dispose()
            _timeline = Nothing
            preview.Signing = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.Inchide", ex)
        End Try
    End Sub

End Class
