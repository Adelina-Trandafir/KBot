Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The AcroPDF ActiveX viewer, rebuilt from zero (slice 0078-15, operator 06-07.10.2026). It replaced the old viewer
''' (AcroPdfSurface), removed from the project on 07.10.2026 -- what it did is in docs/worklog/SLICE-0078-15-*.md.
'''
''' Built step by step, each step checked on the operator's PC before the next one is added. Now (07.10.2026):
''' the document goes in through <c>src</c> (<see cref="LoadThroughSrc"/>); Adobe's window born 0x0 gets the control's
''' size again (AcroPdfViewer.SizeNudge.vb); Read Mode (Ctrl+H, then optionally Ctrl+2) is sent as soon as the page is
''' laid out and verified (AcroPdfViewer.ReadMode.vb); the Save As trap, the «save changes?» answer and the trapped
''' script alerts (AcroPdfViewer.SaveTrap.vb); Ctrl+S after a signature, optional (AcroPdfViewer.SaveKeys.vb).
''' The primer (<see cref="PrimerPath"/>) is switched off.
'''
''' Two interchangeable watches drive the fixes (<see cref="DetailedWatch"/>): the small one (default,
''' AcroPdfViewer.LightWatch.vb) writes nothing; the big one (ACTIVEX-CHECK, kept for future investigations) also
''' writes every window event to <c>&lt;AppDir&gt;\Logs\activex_check.log</c>.
''' </summary>
Partial Public NotInheritable Class AcroPdfViewer
    Implements IDisposable

    Private ReadOnly _panel As Control
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _clsid As String
    Private _host As AcroPdfHost
    Private _loadedPath As String

    Public Sub New(hostPanel As Control, log As Action(Of String))
        If hostPanel Is Nothing Then Throw New ArgumentNullException(NameOf(hostPanel))
        _panel = hostPanel
        _log = log
        _clsid = AcroPdfDetector.NormaliseClsid(AcroPdfDetector.ResolveClsid())
        InitSaveTrap()
    End Sub

    ''' <summary>False when AcroPDF is not registered -- the caller must then say so.</summary>
    Public ReadOnly Property IsAvailable As Boolean
        Get
            Return Not String.IsNullOrEmpty(_clsid)
        End Get
    End Property

    ''' <summary>The document last loaded, or Nothing.</summary>
    Public ReadOnly Property LoadedPath As String
        Get
            Return _loadedPath
        End Get
    End Property

    ''' <summary>
    ''' Loads the document into the control. Never throws -- every failure path returns a Romanian
    ''' sentence the caller can put on screen. Synchronous; returned as a task for the caller's shape.
    ''' </summary>
    Public Function ShowDocumentAsync(pdfPath As String) As Task(Of AcroPdfResult)
        Return Task.FromResult(ShowDocument(pdfPath))
    End Function

    Private Function ShowDocument(pdfPath As String) As AcroPdfResult
        Try
            If Not IsAvailable Then
                Return New AcroPdfResult(AcroPdfStatus.NotRegistered,
                                         "Controlul Adobe (AcroPDF) nu este înregistrat pe această mașină.")
            End If
            If String.IsNullOrWhiteSpace(pdfPath) OrElse Not File.Exists(pdfPath) Then
                Return New AcroPdfResult(AcroPdfStatus.FileMissing, "Documentul nu există pe disc.")
            End If

            Dim k_isNew As Boolean = _host Is Nothing
            Dim k_host As AcroPdfHost = EnsureHost()
            If k_host Is Nothing Then
                Return New AcroPdfResult(AcroPdfStatus.Failed,
                                         "Controlul Adobe nu a putut fi creat. Detalii în jurnalul de erori.")
            End If

            Report($"AcroPDF: încarc «{Path.GetFileName(pdfPath)}».")
            StartWatching(pdfPath)   ' the big watch (ACTIVEX-CHECK) or the small one: DetailedWatch
            CancelPrimer()
            ResetSizeNudge()
            ArmReadMode()
            Dim k_replacing As Boolean = BeforeLoadSaveTrap()
            _loadedPath = pdfPath

            ' PRIMER switched off (operator, 07.10.2026: src only for now): the document goes in directly.
            'If k_isNew Then
            '    If String.IsNullOrEmpty(PrimerPath) OrElse Not File.Exists(PrimerPath) Then
            '        Check($"PRIMER: file missing «{PrimerPath}» -> the document is loaded directly")   ' ACTIVEX-CHECK
            '    Else
            '        _primerPending = pdfPath
            '        '_primerTimer.Start()   ' timer switched off (operator, 06.10.2026): the event alone starts the document
            '        LoadTraced(k_host, PrimerPath, "PRIMER")
            '        Return New AcroPdfResult(AcroPdfStatus.Shown, "", collapsed:=True)
            '    End If
            'End If

            LoadTraced(k_host, pdfPath, "DOCUMENT")
            AfterLoadSaveTrap(k_replacing)
            Return New AcroPdfResult(AcroPdfStatus.Shown, "", collapsed:=True)
        Catch ex As Exception
            Check("ShowDocument EXCEPTION: " & ex.Message)                          ' ACTIVEX-CHECK
            GlobalErrorLog.Write("AcroPdfViewer.ShowDocument", ex)
            Return New AcroPdfResult(AcroPdfStatus.Failed,
                                     "Documentul nu a putut fi afișat. Detalii în jurnalul de erori.")
        End Try
    End Function

    ''' <summary>Empties the viewer by DESTROYING the control; the next load creates a new one.</summary>
    Public Sub Clear()
        Try
            StopWatching()
            CancelPrimer()
            _readModePending = False
            _fitWidthPending = False
            _savePending = False
            Dim k_hadDocument As Boolean = BeforeClearSaveTrap()
            _loadedPath = Nothing
            Dim k_host As AcroPdfHost = _host
            _host = Nothing
            If k_host Is Nothing Then
                _saveTrap.Stop()
                Return
            End If
            Dim k_release As ReleaseState = If(DetailedWatch, BeforeRelease(k_host), Nothing)   ' ACTIVEX-CHECK
            Dim k_clock As Diagnostics.Stopwatch = Diagnostics.Stopwatch.StartNew()
            _panel.Controls.Remove(k_host)
            k_host.Dispose()
            Check($"Clear: control removed and disposed in {k_clock.ElapsedMilliseconds} ms, IsDisposed={k_host.IsDisposed}")   ' ACTIVEX-CHECK
            AfterClearSaveTrap(k_hadDocument)
            If k_release IsNot Nothing Then StartReleaseWatch(k_release)            ' ACTIVEX-CHECK
        Catch ex As Exception
            _saveTrap.Stop()
            GlobalErrorLog.Write("AcroPdfViewer.Clear", ex)
        End Try
    End Sub

    ' Creates the control on first use. AxHost needs a handle before GetOcx() returns anything.
    Private Function EnsureHost() As AcroPdfHost
        If _host IsNot Nothing Then Return _host
        Try
            Dim k_clock As Diagnostics.Stopwatch = Diagnostics.Stopwatch.StartNew()
            Dim k_host As New AcroPdfHost(_clsid) With {.Dock = DockStyle.Fill, .Name = "axAcroPdf"}
            _panel.Controls.Add(k_host)
            Dim k_handle As IntPtr = k_host.Handle
            _host = k_host
            ' Release on FormClosed back ON for testing (operator, 07.10.2026, afternoon): the control goes while its
            ' windows still exist, so the trap can answer «save changes?» and Adobe sees a clean close.
            WatchFormClose()
            Check($"EnsureHost: control {HexOf(k_handle)} created in {k_clock.ElapsedMilliseconds} ms, " &   ' ACTIVEX-CHECK
                  $"bounds={k_host.Bounds}, {AdobeNativeMethods.Descendants(k_handle).Count} window(s) inside")
            Return k_host
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.EnsureHost", ex)
            Return Nothing
        End Try
    End Function

    ' The form the control lives on. When it closes, the control is released while its windows still exist. Measured
    ' 06.10.2026: released only from Dispose, Windows had already destroyed the form and the control window with it
    ' (Clear found control 0x0, form none), and Adobe's two processes stayed alive 10-30 s more.
    Private _form As Form

    Private Sub WatchFormClose()
        Dim k_form As Form = _panel.FindForm()
        If k_form Is _form Then Return
        If _form IsNot Nothing Then RemoveHandler _form.FormClosed, AddressOf OnFormClosed
        _form = k_form
        If _form IsNot Nothing Then AddHandler _form.FormClosed, AddressOf OnFormClosed
    End Sub

    ' FormClosed comes before Windows destroys the form's windows. UI boundary: log and swallow.
    Private Sub OnFormClosed(sender As Object, e As FormClosedEventArgs)
        Try
            Check($"form closed ({e.CloseReason}) -> releasing the control while its windows still exist")   ' ACTIVEX-CHECK
            Clear()
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.OnFormClosed", ex)
        End Try
    End Sub

    ' ── Primer (slice 0078-15 test, operator 06.10.2026) ──────────────────────────────────────────
    ' A new control first loads a plain PDF (no form, no scripts); the real document is loaded once Adobe has put its
    ' first window into the control (OnControlEvent).
    ' The 5 s fallback timer is switched off (operator, 06.10.2026: no timers, events only) -- kept commented. Without it,
    ' a primer that never attaches leaves the document unloaded (the check log then shows no «PRIMER: ... -> loading»).

    'Private Const PrimerMaxWaitMs As Integer = 5000

    ''' <summary>The plain PDF loaded into every new control before the document. Nothing / missing = no primer.</summary>
    Public Property PrimerPath As String

    ' The document waiting for the primer to attach (Nothing = none).
    Private _primerPending As String
    'Private ReadOnly _primerTimer As New Timer() With {.Interval = PrimerMaxWaitMs}
    'Private _primerTimerWired As Boolean

    Private Sub CancelPrimer()
        'If Not _primerTimerWired Then
        '    AddHandler _primerTimer.Tick, AddressOf OnPrimerTimeout
        '    _primerTimerWired = True
        'End If
        '_primerTimer.Stop()
        If _primerPending IsNot Nothing Then Check($"PRIMER: pending document «{_primerPending}» dropped")   ' ACTIVEX-CHECK
        _primerPending = Nothing
    End Sub

    '' Timer: log and swallow.
    'Private Sub OnPrimerTimeout(sender As Object, e As EventArgs)
    '    Try
    '        LoadPendingDocument($"the primer did NOT attach within {PrimerMaxWaitMs} ms")
    '    Catch ex As Exception
    '        GlobalErrorLog.Write("AcroPdfViewer.OnPrimerTimeout", ex)
    '    End Try
    'End Sub

    ' Posted from the WinEvent callback (not called inside it): log and swallow.
    Private Sub OnPrimerAttached()
        Try
            LoadPendingDocument("the primer attached (first window inside the control)")
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.OnPrimerAttached", ex)
        End Try
    End Sub

    Private Sub LoadPendingDocument(k_reason As String)
        '_primerTimer.Stop()
        Dim k_path As String = _primerPending
        _primerPending = Nothing
        If k_path Is Nothing OrElse _host Is Nothing OrElse Not _host.IsHandleCreated Then Return
        Check($"+{Elapsed()} ms PRIMER: {k_reason} -> loading the document")    ' ACTIVEX-CHECK
        ' The real document's page view is a new milestone.
        _seenPageView = False
        LoadTraced(_host, k_path, "DOCUMENT")
    End Sub

    ''' <summary>
    ''' True (the default since 07.10.2026) = the document goes in through the control's <c>src</c> property
    ''' (<c>file:///…</c>): on the bench, every load through src showed the page without a click. False =
    ''' <c>LoadFile</c>, kept in case src does not work somewhere. Read at each load.
    ''' </summary>
    Public Property LoadThroughSrc As Boolean = True

    ' src or LoadFile (see LoadThroughSrc), with the answer and timing in the check log. Callers are wrapped.
    Private Sub LoadTraced(k_host As AcroPdfHost, k_path As String, k_what As String)
        Dim k_before As Long = Elapsed()
        If LoadThroughSrc Then
            Dim k_address As String = k_host.LoadThroughSrc(k_path)
            Check($"+{Elapsed()} ms {k_what}: src=«{k_address}» set in {Elapsed() - k_before} ms")   ' ACTIVEX-CHECK
            DumpTree($"right after src ({k_what})")                                 ' ACTIVEX-CHECK
            Return
        End If
        Dim k_answer As Boolean = k_host.LoadFile(k_path)
        Check($"+{Elapsed()} ms {k_what}: LoadFile «{Path.GetFileName(k_path)}» returned {k_answer} in {Elapsed() - k_before} ms")   ' ACTIVEX-CHECK
        DumpTree($"right after LoadFile ({k_what})")                                ' ACTIVEX-CHECK
    End Sub

    Private Sub Report(k_line As String)
        _log?.Invoke(k_line)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Clear()
        Try
            DisposeSaveTrap()
            If _form IsNot Nothing Then RemoveHandler _form.FormClosed, AddressOf OnFormClosed
            _form = Nothing
            'If _primerTimerWired Then RemoveHandler _primerTimer.Tick, AddressOf OnPrimerTimeout
            '_primerTimer.Dispose()
            ' The writer finishes the lines already queued, then ends -- but only after the release watch (ACTIVEX-CHECK),
            ' whose lines keep coming for 30 s after the bench is closed.
            Dim k_watch As Threading.Thread = _releaseWatch
            If k_watch Is Nothing OrElse Not k_watch.IsAlive Then
                CompleteChecks()
            Else
                Dim k_closer As New Threading.Thread(
                    Sub()
                        k_watch.Join()
                        CompleteChecks()
                    End Sub) With {.IsBackground = True, .Name = "AcroPdfViewer check log closer"}
                k_closer.Start()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.Dispose", ex)
        End Try
    End Sub

End Class
