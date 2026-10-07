#If DEBUG Then
Option Strict On
Imports System.IO
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The bench for slice 0078-15: ONLY the AcroPDF ActiveX control (<see cref="AcroPdfViewer"/>, the engine the DDF / ORD
''' pages use when «ActiveX» is chosen) loading the PDF the operator picks -- no signing session, no server, no settings
''' preface. «Semnare și salvare» loads a COPY with the Save As trap on, as the app does, and reads the copy back when
''' Adobe writes it. «Urmărire detaliată» writes every window event to <c>Logs\activex_check.log</c>; «Jurnalul ActiveX…»
''' opens them in <see cref="ActivexLogViewerForm"/>.
''' </summary>
Public NotInheritable Class ActiveXPdfHarnessForm

    Private ReadOnly _viewer As AcroPdfViewer
    ' The copy loaded while «Semnare și salvare» is checked (Nothing otherwise), and its SHA-256 at load time.
    Private _copyPath As String
    Private _copyHashAtLoad As String
    ' After a trapped save: the file being waited on, since when, and its last size / write time seen.
    Private Const SavedSettleMaxMs As Integer = 30000
    Private _savedPath As String
    Private _savedSince As DateTime
    Private _savedLastLength As Long = -1
    Private _savedLastWrite As DateTime

    ' How the saved file is read back (operator, 07.10.2026: as few timers as possible). True = a FileSystemWatcher on the
    ' copy: every change re-reads it, the last change gives the final state -- no timer. False = the tmrSaved path
    ' (wait until size and write time hold for two ticks), kept in case the watcher misses the end of a save.
    Private Const SavedCheckByWatcher As Boolean = True
    ' ONE watcher for the bench's whole life, re-pointed at each new copy and switched off between documents -- never one
    ' per document. Created on first use, disposed with the bench.
    Private _copyWatcher As FileSystemWatcher
    ' The sha last reported for the copy: a change event that leaves the content as it was writes nothing.
    Private _lastSeenHash As String
    ' Set when the trap reports the signature's Save As; the first write of the copy after it sends Ctrl+S (chkAutoSave),
    ' as the app's signing session asks once the signed file is on disk. Cleared at once, so the save that Ctrl+S
    ' itself causes does not ask again.
    Private _saveAfterWrite As Boolean

    Public Sub New()
        InitializeComponent()
        _viewer = New AcroPdfViewer(pnlHost, AddressOf AdobeHostLog.Write) With {
            .PrimerPath = Path.Combine(AppContext.BaseDirectory, "empty_pdf.pdf")}
        If Not _viewer.IsAvailable Then lblFile.Text = "Controlul Adobe (AcroPDF) nu este înregistrat pe această mașină."
        ' The same two events the app's ReaderHostPreview listens to.
        AddHandler _viewer.DocumentSaved, AddressOf OnDocumentSaved
        AddHandler _viewer.SaveTrapFailed, AddressOf OnSaveTrapFailed
        AddHandler _viewer.SaveKeysSent, AddressOf OnSaveKeysSent
        AddHandler _viewer.SaveNotSent, AddressOf OnSaveNotSent
    End Sub

    ' ── Buttons (UI boundaries: log and swallow) ────────────────────────────────
    Private Async Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        Try
            If dlgOpen.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim k_path As String = dlgOpen.FileName
            ' Signing and saving as in the app: the trap writes the signed PDF over the file it loaded, so a copy is loaded.
            _copyPath = Nothing
            _copyHashAtLoad = Nothing
            ' The previous document's read-back is over (either path).
            StopCopyWatch()
            tmrSaved.Stop()
            _savedPath = Nothing
            _saveAfterWrite = False
            If chkSaveTrap.Checked Then
                _copyPath = MakeCopy(k_path)
                _copyHashAtLoad = PdfHash.ComputeFile(_copyPath)
                AdobeHostLog.Write($"Banc ActiveX: copie pentru semnare «{_copyPath}» (din «{k_path}»).")
                k_path = _copyPath
                _lastSeenHash = _copyHashAtLoad
                If SavedCheckByWatcher Then WatchCopy(_copyPath)
            End If
            btnCopyFolder.Enabled = _copyPath IsNot Nothing
            _viewer.SaveTrapEnabled = chkSaveTrap.Checked
            _viewer.LoadThroughSrc = chkSrc.Checked
            _viewer.DetailedWatch = chkDetailed.Checked
            _viewer.FitWidthAfterReadMode = AppSettings.Current.AcroPdfFitWidth
            lblFile.Text = "Se încarcă «" & Path.GetFileName(k_path) & "»" & If(chkSrc.Checked, " prin src", " prin LoadFile") &
                           If(_copyPath IsNot Nothing, ", copie cu capcana «Salvare ca»", "") & "…"
            Dim k_result As AcroPdfResult = Await _viewer.ShowDocumentAsync(k_path)
            lblFile.Text = If(k_result.Succeeded, k_path, k_result.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.btnOpen_Click", ex)
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Try
            _viewer.Clear()
            StopCopyWatch()
            lblFile.Text = "Niciun document"
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.btnClose_Click", ex)
        End Try
    End Sub

    Private Sub btnSeen_Click(sender As Object, e As EventArgs) Handles btnSeen.Click
        Try
            _viewer.MarkOperatorSeen()
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.btnSeen_Click", ex)
        End Try
    End Sub

    ' The copy: <AppDir>\TempPdf\<name>_banc_<HHmmss>.pdf (the app's unsigned work area, wiped at every start).
    ' Reached from btnOpen_Click (wrapped).
    Private Shared Function MakeCopy(k_source As String) As String
        Dim k_target As String = Path.Combine(TempPdfStore.EnsureRoot(),
            $"{Path.GetFileNameWithoutExtension(k_source)}_banc_{DateTime.Now:HHmmss}.pdf")
        File.Copy(k_source, k_target, overwrite:=True)
        Return k_target
    End Function

    ' The trap reports the Save As dialog closed -- Adobe writes the file AFTER that (measured 07.10.2026: read at once,
    ' the copy still had its old size and sha). So, like the app's PdfSigningSession, the file is read only once it has
    ' stopped changing: same size and write time on two ticks in a row, and readable.
    Private Sub OnDocumentSaved(k_path As String)
        Try
            If SavedCheckByWatcher Then
                ' The watcher reads the file when Adobe writes it; the trap's event only says the dialog closed.
                lblFile.Text = $"Salvare raportată la {DateTime.Now:HH:mm:ss}; fișierul se citește când Adobe îl scrie…"
                _saveAfterWrite = chkAutoSave.Checked
                Return
            End If
            _savedPath = k_path
            _savedSince = DateTime.UtcNow
            _savedLastLength = -1
            lblFile.Text = $"Salvare raportată la {DateTime.Now:HH:mm:ss}; aștept ca Adobe să termine de scris fișierul…"
            tmrSaved.Start()
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.OnDocumentSaved", ex)
        End Try
    End Sub

    ' Timer: log and swallow.
    Private Sub tmrSaved_Tick(sender As Object, e As EventArgs) Handles tmrSaved.Tick
        Try
            If _savedPath Is Nothing Then
                tmrSaved.Stop()
                Return
            End If
            Dim k_info As New FileInfo(_savedPath)
            Dim k_stable As Boolean = k_info.Exists AndAlso k_info.Length = _savedLastLength AndAlso k_info.LastWriteTime = _savedLastWrite
            _savedLastLength = If(k_info.Exists, k_info.Length, -1)
            _savedLastWrite = If(k_info.Exists, k_info.LastWriteTime, DateTime.MinValue)
            Dim k_timedOut As Boolean = (DateTime.UtcNow - _savedSince).TotalMilliseconds > SavedSettleMaxMs
            If Not k_stable AndAlso Not k_timedOut Then Return
            tmrSaved.Stop()
            Dim k_path As String = _savedPath
            _savedPath = Nothing
            ReportSaved(k_path, k_info, k_timedOut AndAlso Not k_stable)
        Catch ex As IOException
            ' Adobe still holds the file: the next tick tries again.
        Catch ex As Exception
            tmrSaved.Stop()
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.tmrSaved_Tick", ex)
        End Try
    End Sub

    ' What the app would read back: size, write time, SHA-256 against the copy at load. Reached from the tick (wrapped).
    Private Sub ReportSaved(k_path As String, k_info As FileInfo, k_unsettled As Boolean)
        Try
            Dim k_hash As String = PdfHash.ComputeFile(k_path)
            Dim k_changed As Boolean = Not PdfHash.AreEqual(k_hash, _copyHashAtLoad)
            lblFile.Text = $"Salvat la {DateTime.Now:HH:mm:ss}: «{k_info.Name}», {k_info.Length:N0} octeți, " &
                           If(k_changed, "conținut schimbat față de încărcare.", "conținut IDENTIC cu cel încărcat!")
            AdobeHostLog.Write($"Banc ActiveX: documentul salvat de capcană «{k_path}», {k_info.Length} octeți, " &
                               $"scris la {k_info.LastWriteTime:HH:mm:ss.fff}, SHA-256 {k_hash} (la încărcare {_copyHashAtLoad})" &
                               If(k_unsettled, $" — fișierul încă se schimba după {SavedSettleMaxMs / 1000} s.", "."))
        Catch ex As IOException
            lblFile.Text = "Fișierul salvat nu poate fi citit (Adobe îl ține deschis): " & ex.Message
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.ReportSaved", ex)
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.ReportSaved", ex)
        End Try
    End Sub

    ' ── Read-back by FileSystemWatcher (SavedCheckByWatcher) ────────────────────

    ' Points the bench's one watcher at the copy (creates it the first time). Reached from btnOpen_Click (wrapped).
    Private Sub WatchCopy(k_copy As String)
        If _copyWatcher Is Nothing Then
            ' Events come on the UI thread (SynchronizingObject). Adobe may write the file in place or replace it
            ' (temporary file + rename): Changed, Created and Renamed all lead to a re-read.
            _copyWatcher = New FileSystemWatcher() With {
                .NotifyFilter = NotifyFilters.FileName Or NotifyFilters.Size Or NotifyFilters.LastWrite,
                .IncludeSubdirectories = False,
                .SynchronizingObject = Me}
            AddHandler _copyWatcher.Changed, AddressOf OnCopyChanged
            AddHandler _copyWatcher.Created, AddressOf OnCopyChanged
            AddHandler _copyWatcher.Renamed, AddressOf OnCopyChanged
        End If
        _copyWatcher.EnableRaisingEvents = False
        _copyWatcher.Path = Path.GetDirectoryName(k_copy)
        _copyWatcher.Filter = Path.GetFileName(k_copy)
        _copyWatcher.EnableRaisingEvents = True
    End Sub

    ' Off between documents; the same watcher is pointed at the next copy.
    Private Sub StopCopyWatch()
        If _copyWatcher IsNot Nothing Then _copyWatcher.EnableRaisingEvents = False
    End Sub

    ' Watcher event (UI thread): log and swallow.
    Private Sub OnCopyChanged(sender As Object, e As FileSystemEventArgs)
        Try
            If _copyPath Is Nothing OrElse Not String.Equals(e.FullPath, _copyPath, StringComparison.OrdinalIgnoreCase) Then Return
            ReadCopyBack()
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.OnCopyChanged", ex)
        End Try
    End Sub

    ' Re-reads the copy after a change. A read that fails while Adobe writes is skipped -- the end of the write raises
    ' another event -- but said in the bar, in case it was the last one. Reached from OnCopyChanged (wrapped).
    Private Sub ReadCopyBack()
        Dim k_info As New FileInfo(_copyPath)
        If Not k_info.Exists Then Return
        Dim k_hash As String
        Try
            k_hash = PdfHash.Compute(ReadSharedQuiet(_copyPath))
        Catch ex As IOException
            lblFile.Text = $"{DateTime.Now:HH:mm:ss}: fișierul se schimbă, citirea a picat (Adobe încă scrie?) — aștept următoarea schimbare."
            Return
        End Try
        If PdfHash.AreEqual(k_hash, _lastSeenHash) Then Return
        _lastSeenHash = k_hash
        Dim k_changed As Boolean = Not PdfHash.AreEqual(k_hash, _copyHashAtLoad)
        lblFile.Text = $"Scris la {k_info.LastWriteTime:HH:mm:ss}: «{k_info.Name}», {k_info.Length:N0} octeți, " &
                       If(k_changed, "conținut schimbat față de încărcare.", "conținut IDENTIC cu cel încărcat!")
        AdobeHostLog.Write($"Banc ActiveX: copia s-a schimbat pe disc «{_copyPath}», {k_info.Length} octeți, " &
                           $"scrisă la {k_info.LastWriteTime:HH:mm:ss.fff}, SHA-256 {k_hash} (la încărcare {_copyHashAtLoad}).")
        If _saveAfterWrite Then
            _saveAfterWrite = False
            AdobeHostLog.Write("Banc ActiveX: semnătura e pe disc — cer Ctrl+S, ca sesiunea de semnare din aplicație.")
            _viewer.SaveAfterSignatureEnabled = True
            _viewer.RequestSave()
        End If
    End Sub

    ' Reads the copy WITHOUT ever blocking Adobe (FileShare.ReadWrite|Delete, as the app's SignedPdfFiles.ReadShared) and
    ' without an error-log entry: a lock while Adobe writes is expected here (PdfHash.ComputeFile opened it with
    ' FileShare.Read -- for those milliseconds Adobe could not have written -- and logged every miss). Throws IOException.
    Private Shared Function ReadSharedQuiet(k_path As String) As Byte()
        Using k_stream As New FileStream(k_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Dim k_bytes(CInt(k_stream.Length) - 1) As Byte
            Dim k_read As Integer = 0
            While k_read < k_bytes.Length
                Dim k_n As Integer = k_stream.Read(k_bytes, k_read, k_bytes.Length - k_read)
                If k_n <= 0 Then Throw New IOException($"Short read on {k_path}: {k_read}/{k_bytes.Length}.")
                k_read += k_n
            End While
            Return k_bytes
        End Using
    End Function

    ' What the app's signing session asks after a signature (option «salvare automată după semnătură»), by hand.
    Private Sub btnSaveNow_Click(sender As Object, e As EventArgs) Handles btnSaveNow.Click
        Try
            _viewer.SaveAfterSignatureEnabled = True
            lblFile.Text = If(_viewer.RequestSave(), "Ctrl+S cerut…", "Ctrl+S nu se poate cere: niciun document încărcat.")
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.btnSaveNow_Click", ex)
        End Try
    End Sub

    Private Sub OnSaveKeysSent()
        Try
            lblFile.Text = $"Ctrl+S trimis la {DateTime.Now:HH:mm:ss}; fișierul se citește când Adobe îl scrie…"
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.OnSaveKeysSent", ex)
        End Try
    End Sub

    Private Sub OnSaveNotSent(k_message As String)
        Try
            KBotMessage.Show(Me, k_message, "Salvare după semnătură", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.OnSaveNotSent", ex)
        End Try
    End Sub

    ' The same box the app shows when no signing session is running.
    Private Sub OnSaveTrapFailed(k_reason As String)
        Try
            lblFile.Text = "Salvare oprită de K-BOT: " & k_reason
            KBotMessage.Show(Me,
                             "Salvarea documentului a fost oprită de K-BOT, ca fișierul să nu ajungă în alt loc." &
                             Environment.NewLine & k_reason,
                             "Salvare oprită", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.OnSaveTrapFailed", ex)
        End Try
    End Sub

    Private Sub btnCopyFolder_Click(sender As Object, e As EventArgs) Handles btnCopyFolder.Click
        Try
            If _copyPath Is Nothing OrElse Not File.Exists(_copyPath) Then Return
            Process.Start("explorer.exe", $"/select,""{_copyPath}""")?.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.btnCopyFolder_Click", ex)
        End Try
    End Sub

    ' Not modal: the viewer stays open next to the bench and is reloaded by hand.
    Private Sub btnLogViewer_Click(sender As Object, e As EventArgs) Handles btnLogViewer.Click
        Try
            Dim k_form As New ActivexLogViewerForm()
            k_form.Show(Me)
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.btnLogViewer_Click", ex)
        End Try
    End Sub

    ' The newest load of Logsactivex_check.log as lanes (a root of the viewer's tree is a lane, a leaf a change), not modal.
    Private Sub btnLanes_Click(sender As Object, e As EventArgs) Handles btnLanes.Click
        Try
            Dim k_path As String = LogPaths.Combine("activex_check.log")
            If Not File.Exists(k_path) Then
                lblFile.Text = $"Nu există «{k_path}»."
                Return
            End If
            Dim k_caption As String = ""
            Dim k_lanes As List(Of ActivexLaneSpec) = ActivexLaneForm.SpecsFromLog(k_path, k_caption)
            Dim k_form As New ActivexLaneForm($"Jurnalul ActiveX — pe culoare — {k_caption}", k_lanes)
            k_form.Show(Me)
        Catch ex As Exception
            lblFile.Text = "Nu pot deschide culoarele: " & ex.Message
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.btnLanes_Click", ex)
        End Try
    End Sub

    ' Called from Dispose (see Designer). Must not throw there.
    Private Sub ShutDownBench()
        Try
            tmrSaved.Stop()
            If _copyWatcher IsNot Nothing Then
                _copyWatcher.EnableRaisingEvents = False
                RemoveHandler _copyWatcher.Changed, AddressOf OnCopyChanged
                RemoveHandler _copyWatcher.Created, AddressOf OnCopyChanged
                RemoveHandler _copyWatcher.Renamed, AddressOf OnCopyChanged
                _copyWatcher.Dispose()
                _copyWatcher = Nothing
            End If
            If _viewer Is Nothing Then Return
            RemoveHandler _viewer.DocumentSaved, AddressOf OnDocumentSaved
            RemoveHandler _viewer.SaveTrapFailed, AddressOf OnSaveTrapFailed
            RemoveHandler _viewer.SaveKeysSent, AddressOf OnSaveKeysSent
            RemoveHandler _viewer.SaveNotSent, AddressOf OnSaveNotSent
            _viewer.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessForm.ShutDownBench", ex)
        End Try
    End Sub

End Class
#End If
