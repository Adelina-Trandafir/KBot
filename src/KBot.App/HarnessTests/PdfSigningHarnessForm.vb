#If DEBUG Then
Option Strict On
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming
Imports KBot.Xfa

''' <summary>
''' The bench for slice 0078: the whole signing round trip, on the REAL pieces the DDF / ORD pages
''' use -- <see cref="ReaderHostPreview"/> (with whichever engine the operator chose in «Setări»;
''' the bench shows it and never changes it), <see cref="PdfSigningSession"/> for the upload and
''' the server routes for the read-back.
'''
''' <para><b>What to watch.</b> The log on the right mirrors <c>adobe_preview.log</c> live: every
''' dialog the Save As trap sees, what it wrote into the file-name box, whether it pressed Save and
''' answered «replace?». After each save the bench reads the file itself and lists the signed
''' fields, the roles and the sha -- independent of the upload, so a trap that works and an upload
''' that fails are told apart.</para>
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
        lblMotor.Text = "Motor: " & AdobeViewerSettings.EngineLabel(preview.Engine) & " (din «Setări»)"
        UpdateSessionLabel()
        Write("Banc pornit. Motorul Adobe e cel din «Setări»; bancul nu-l schimbă.")
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

            Dim id As Integer = 0
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
            preview.ShowDocument(path, exists:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.OpenAsync", ex)
            Write("Deschiderea a eșuat: " & ex.Message)
        End Try
    End Sub

    ''' <summary>Adobe holds the file open: the preview lets go before the copy is overwritten.</summary>
    Private Sub ReleaseDocument()
        EndSigning()
        preview.Signing = Nothing
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
    ' A trapped save finished: the bench reads the file ITSELF, whatever the session decides.
    Private Async Sub OnDocumentSaved(path As String)
        Try
            Write("SALVAT de capcană: " & path)
            Dim bytes As Byte() = Await SignedPdfFiles.ReadWhenSettledAsync(path).ConfigureAwait(True)
            If bytes Is Nothing Then
                Write("Fișierul salvat nu s-a stabilizat în 10 s — nu a putut fi citit.")
                Return
            End If
            LogSignatures("După salvare", bytes)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.OnDocumentSaved", ex)
            Write("Citirea după salvare a eșuat: " & ex.Message)
        End Try
    End Sub

    Private Sub OnSaveCancelled(reason As String)
        Write("SALVARE ANULATĂ de capcană: " & reason)
    End Sub

    ' The session already showed its message to the operator; the bench adds it to the log.
    Private Sub OnSigningCompleted(session As PdfSigningSession, outcome As PdfSigningOutcome)
        Try
            If outcome Is Nothing Then Return
            Write($"ÎNCĂRCARE: {outcome.Status} — roluri «{outcome.Semnatura}», sha {ShortSha(outcome.NewSha)}. " &
                  outcome.Message)
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
            Dim info As PdfSignatureInfo = PdfSignatures.Read(bytes, Kind())
            If Not info.IsSigned Then
                Write($"{label}: nicio semnătură ({bytes.Length:N0} octeți, sha {ShortSha(PdfHash.Compute(bytes))}).")
                Return
            End If
            Write($"{label}: {info.FieldNames.Count} câmpuri semnate [{String.Join(", ", info.FieldNames)}] " &
                  $"-> Semnatura «{info.ToSemnatura()}», sha {ShortSha(PdfHash.Compute(bytes))}.")
            If info.Unclassified.Count > 0 Then
                Write($"{label}: câmpuri FĂRĂ rol recunoscut: {String.Join(", ", info.Unclassified)}.")
            End If
            ' Slice 0079: what the signature log receives for each field.
            For Each d As PdfSignatureDetail In info.Details
                Write($"{label}:   {d.FieldName} — rol «{d.Role}», semnatar «{d.Signer}», " &
                      If(d.SignedAt.HasValue, $"semnat la {d.SignedAt.Value:yyyy-MM-dd HH:mm:ss zzz}", "fără dată"))
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
            preview.Signing = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessForm.Inchide", ex)
        End Try
    End Sub

End Class
#End If
