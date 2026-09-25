Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>How an attempt to show a document in the ActiveX control ended.</summary>
Public Enum AcroPdfStatus
    ''' <summary>Loaded, laid out, and the panes are collapsed.</summary>
    Shown = 0
    ''' <summary>The AcroPDF control is not registered on this machine.</summary>
    NotRegistered = 1
    ''' <summary>The file is not on disk.</summary>
    FileMissing = 2
    ''' <summary>The control refused the document, or could not be created.</summary>
    Failed = 3
End Enum

''' <summary>The status, the Romanian sentence for the operator, and what the collapse achieved.</summary>
Public NotInheritable Class AcroPdfResult

    Public ReadOnly Property Status As AcroPdfStatus
    Public ReadOnly Property Message As String
    ''' <summary>True when the document view ended up filling the panel (panes collapsed).</summary>
    Public ReadOnly Property Collapsed As Boolean

    Public Sub New(status As AcroPdfStatus, message As String, Optional collapsed As Boolean = False)
        Me.Status = status
        Me.Message = If(message, "")
        Me.Collapsed = collapsed
    End Sub

    Public ReadOnly Property Succeeded As Boolean
        Get
            Return Status = AcroPdfStatus.Shown
        End Get
    End Property

End Class

''' <summary>
''' Shows a PDF in the AcroPDF ActiveX control, and puts it into the state the operator actually
''' wants: document filling the panel, Adobe's panes collapsed.
'''
''' WHY THIS EXISTS AS A SHARED CLASS. Everything here was learned on the bench between 05 and
''' 06.08.2026, and every step is a measurement rather than a guess. Re-deriving it in KBot.App
''' would mean a second copy of hard-won behaviour that would drift — the mistake slice 0024-01 was
''' written to stop. The bench and the DDF preview drive THIS class.
'''
''' THE SEQUENCE, AND WHY EACH STEP IS THERE:
'''
'''  1. <b>Load.</b> <c>LoadFile</c> by reflection — <c>Option Strict On</c> forbids late binding,
'''     and there is deliberately no interop assembly (see <see cref="AcroPdfHost"/>).
'''  2. <b>Wake.</b> Adobe postpones its FIRST layout until the control receives input: right after
'''     the first load, 26 of 28 child windows are zero-sized and the panel is grey until someone
'''     clicks in it. A focus call plus a one-pixel resize fixes it — and the SIZE CHANGE is the part
'''     that works («Adobe recomputes its layout on a size CHANGE, not on a repaint»).
'''  3. <b>Collapse.</b> Pressing Adobe's OWN collapse button. Hiding the panes with
'''     <c>ShowWindow</c> does NOT work: hiding leaves the WIDTH, so Adobe never re-lays-out and the
'''     document stays inset by 67px. Adobe's own collapse sets the width to zero AND moves the
'''     siblings. And because the button toggles, an already-collapsed strip is toggled TWICE — the
'''     floating bar follows the collapse ACTION, not the collapsed STATE.
'''
''' WHAT IS NOT CLAIMED: the chrome API (<c>setShowToolbar</c> and friends) does nothing on this
''' build — all five calls return OK and the bars stay — so it is not used here at all.
'''
''' SIGNING (slice 0078): the same Save As trap as <see cref="AdobeReaderHost"/>. With AcroPDF
''' nobody hands us Adobe's process id, so the trap asks <see cref="OwnerPids"/> on every sweep.
''' MEASURED 23.09.2026 on the bench: Acrobat serves the control with TWO processes -- a BROKER
''' that K-BOT's own process starts (<c>/o /eo /l /b /ac /id &lt;our pid&gt;</c>) and that owns the
''' Save As dialog, and its RENDERER child (<c>--type=renderer</c>) that owns the <c>AVL_*</c> view
''' windows inside the control. Watching only the renderer let the Save As through unseen. The
''' family is: the renderers seen in the control, their parents, every Adobe process K-BOT
''' started, and every child of those.
''' </summary>
Public NotInheritable Class AcroPdfSurface
    Implements IDisposable

    ' Deep enough for Adobe's pane tree, which was measured at 11 levels.
    Private Const ProbeDepth As Integer = 14
    ' Adobe re-lays-out asynchronously; these are the waits that let it.
    Private Const CollapseSettleMs As Integer = 600

    ''' <summary>
    ''' How long to keep waiting for Adobe to BUILD its pane tree after a load.
    '''
    ''' THE FIRST VERSION OF THIS CLASS TRIED ONCE, 430 ms after LoadFile, AND FAILED — the log read
    ''' «the layout stayed degenerate» and then «the tab strip does not exist in the tree». The tree simply
    ''' did not exist yet. On the bench the operator presses those buttons seconds after loading, so
    ''' the panes were always already there and the one-shot looked fine.
    '''
    ''' This is the same lesson slice 0023 wrote down for `hideChildren`: «Adobe creates the task
    ''' pane host AFTER the main view, so a single attempt right after embed often finds nothing —
    ''' hence the retry knobs». Same numbers as that config, for the same reason.
    ''' </summary>
    Private Const MaxLayoutAttempts As Integer = 10
    Private Const AttemptIntervalMs As Integer = 400
    ' How long Acrobat may take to put its FIRST window in the control (cold start of its renderer,
    ' measured on a client 24.09.2026 as more than 4 s), and the cap on the whole wait.
    Private Const ColdStartMaxMs As Integer = 20000
    Private Const LayoutMaxMs As Integer = 40000

    ''' <summary>
    ''' Panes hidden AFTER the collapse. <c>AVExpandCollapseButtonView</c> is deliberately ABSENT:
    ''' hiding it removes the strip the floating bar hovers out of, and the bar never appears again
    ''' (measured 06.08.2026).
    ''' </summary>
    Private Shared ReadOnly ChromeToHide As String() = {TabStripText, TaskPaneText}

    Private Const TabStripText As String = "AVDockableTabStripView"
    Private Const CollapseButtonText As String = "AVExpandCollapseButtonView"
    Private Const PageViewText As String = "AVSplitationPageView"
    Private Const TaskPaneText As String = "AVTaskPaneHostView"
    ' The strip above the page (Adobe's document header, measured 23.09.2026: 1067x102 at the top
    ' of the view). Hidden, and its height given to the window right below it (operator's order).
    Private Const DocumentHeaderText As String = "AVDocumentHeaderView"
    ' Adobe re-lays-out after a resize; the header fix is re-applied once it has settled.
    Private Const RefitDelayMs As Integer = 400
    ' How many refit ticks look for the header after a load (it may appear only once the page has
    ' rendered), and after a resize / return on screen.
    Private Const HeaderSearchTicksAfterLoad As Integer = 15
    Private Const HeaderSearchTicksAfterResize As Integer = 3

    ' Adobe's own view windows start with this class prefix (AVL_AVView, AVL_AVFrame, ...).
    Private Const AdobeViewClassPrefix As String = "AVL_"
    Private Shared ReadOnly AdobeProcessNames As String() = {"Acrobat", "AcroRd32"}

    Private ReadOnly _panel As Control
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _saveTrap As AdobeSaveTrap
    Private _host As AcroPdfHost
    Private _clsid As String
    Private _loadedPath As String
    Private _saveTrapEnabled As Boolean
    Private ReadOnly _refitTimer As New Timer() With {.Interval = RefitDelayMs}
    ' The form whose minimise also takes the viewer off screen (found lazily: at construction the
    ' panel may not be on a form yet).
    Private _form As Form
    ' Refit ticks left before the header search gives up (and says so in the log).
    Private _headerTicksLeft As Integer

    ''' <summary>
    ''' The Read Mode path (engine «ActiveX -- mod citire», operator 24.09.2026): after a load
    ''' NOTHING here walks, collapses, hides or refits Adobe's windows, and no timer of this class
    ''' runs. Adobe's own Read Mode (Ctrl+H) hides the toolbars, sent once, when a WinEvent says the
    ''' page view has been laid out (see <see cref="ArmReadMode"/>). The script-alert clicker and the
    ''' Save As trap (<see cref="AdobeSaveTrap"/>) run exactly as on the old path. False = the old
    ''' path, kept whole for future problems. Read at every load.
    ''' </summary>
    Public Property ReadMode As Boolean

    ''' <summary>Slice 0078: raised on the UI thread after a trapped save. Argument = the document path.</summary>
    Public Event DocumentSaved As Action(Of String)
    ''' <summary>Slice 0078: raised on the UI thread when a save had to be cancelled. Argument = Romanian reason.</summary>
    Public Event SaveTrapFailed As Action(Of String)

    Public Sub New(hostPanel As Control, log As Action(Of String))
        If hostPanel Is Nothing Then Throw New ArgumentNullException(NameOf(hostPanel))
        _panel = hostPanel
        _log = log
        _clsid = AcroPdfDetector.NormaliseClsid(AcroPdfDetector.ResolveClsid())
        _saveTrap = New AdobeSaveTrap(AddressOf Report) With {
            .PidSource = AddressOf OwnerPids, .IsOnScreen = AddressOf IsOnScreen, .Traced = True}
        AddHandler _saveTrap.Saved, AddressOf OnTrapSaved
        AddHandler _saveTrap.Failed, AddressOf OnTrapFailed
        AddHandler _saveTrap.ScriptBurstEnded, AddressOf OnScriptBurstEnded
        AddHandler _refitTimer.Tick, AddressOf OnRefitTick
        AddHandler _deadCheckTimer.Tick, AddressOf OnDeadCheckTick
        AddHandler _panel.SizeChanged, AddressOf OnPanelSizeChanged
        AddHandler _panel.VisibleChanged, AddressOf OnScreenStateChanged
        AddHandler _panel.ParentChanged, AddressOf OnScreenStateChanged
    End Sub

    ' ── On screen / off screen (operator, 23.09.2026) ───────────────────────────
    '
    ' Every timer of this viewer (the trap's sweep, the header refit) stops while the viewer is not
    ' on screen and starts again when it is back. Off screen = the panel or any parent hidden (the
    ' other view is showing, another page of this view), or the form minimised.
    Private Function ViewerShown() As Boolean
        If Not _panel.Visible Then Return False
        Return _form Is Nothing OrElse _form.WindowState <> FormWindowState.Minimized
    End Function

    Private Sub OnScreenStateChanged(sender As Object, e As EventArgs)
        Try
            Tr($"OnScreenStateChanged from {If(sender Is Nothing, "(null)", sender.GetType().Name)}: " & ScreenText())
            AttachForm()
            ApplyScreenState()
        Catch ex As Exception
            Tr("OnScreenStateChanged EXCEPTION: " & ex.ToString())
            GlobalErrorLog.Write("AcroPdfSurface.OnScreenStateChanged", ex)
        End Try
    End Sub

    Private Sub AttachForm()
        Dim f As Form = _panel.FindForm()
        If f Is _form Then Return
        Tr($"AttachForm: {If(_form Is Nothing, "(none)", _form.Name)} -> {If(f Is Nothing, "(none)", f.Name)}")
        If _form IsNot Nothing Then
            RemoveHandler _form.Resize, AddressOf OnScreenStateChanged
            RemoveHandler _form.Activated, AddressOf OnFormActivated
        End If
        _form = f
        If _form IsNot Nothing Then
            AddHandler _form.Resize, AddressOf OnScreenStateChanged
            AddHandler _form.Activated, AddressOf OnFormActivated
        End If
    End Sub

    ' Pauses or resumes the trap and the refit timer to match ViewerShown. Reached from wrapped
    ' handlers and from StartSaveTrap (wrapped).
    Private Sub ApplyScreenState()
        If ViewerShown() Then
            Tr($"ApplyScreenState: shown, trap paused={_saveTrap.IsPaused}" &
               If(_saveTrap.IsPaused, " -> resume trap + header search", " -> nothing to change"))
            If _saveTrap.IsPaused Then
                _saveTrap.Resume()
                ' Adobe may have laid out again while hidden: re-apply the header fix.
                If _host IsNot Nothing AndAlso Not ReadMode Then ScheduleHeaderSearch(HeaderSearchTicksAfterResize)
            End If
            If _readModePending Then
                ' The empty-control check waits while the viewer is off screen; restarted here.
                If _host IsNot Nothing AndAlso _host.IsHandleCreated AndAlso
                   AdobeNativeMethods.Descendants(_host.Handle).Count = 0 Then _deadCheckTimer.Start()
                TrySendReadMode("viewer back on screen")
            End If
        Else
            Tr("ApplyScreenState: NOT shown -> refit timer stopped, trap paused")
            _refitTimer.Stop()
            _deadCheckTimer.Stop()
            _saveTrap.Pause()
            ' The header search that would have ended the recording stopped with the timer.
            If _loadedPath IsNot Nothing AndAlso _headerTicksLeft > 0 Then EndRecording("viewer left the screen while opening")
        End If
    End Sub

    ' A resize makes Adobe lay out again, header included: re-apply after it settles.
    Private Sub OnPanelSizeChanged(sender As Object, e As EventArgs)
        Try
            Tr($"OnPanelSizeChanged: size={_panel.ClientSize} host={_host IsNot Nothing}; " & ScreenText())
            If _host Is Nothing OrElse Not ViewerShown() Then Return
            ' Read Mode survives a resize: Adobe lays it out itself.
            If ReadMode Then Return
            ScheduleHeaderSearch(HeaderSearchTicksAfterResize)
        Catch ex As Exception
            Tr("OnPanelSizeChanged EXCEPTION: " & ex.ToString())
            GlobalErrorLog.Write("AcroPdfSurface.OnPanelSizeChanged", ex)
        End Try
    End Sub

    Private Sub OnRefitTick(sender As Object, e As EventArgs)
        Try
            _refitTimer.Stop()
            Tr($"OnRefitTick: ticksLeft={_headerTicksLeft} host={_host IsNot Nothing}; " & ScreenText())
            If Not ViewerShown() OrElse _host Is Nothing Then
                Tr("OnRefitTick: not shown or no control -> stop")
                Return
            End If
            If HideDocumentHeader() Then
                Tr("OnRefitTick: header found and handled -> stop")
                EndRecording("document open (header handled)")
                Return
            End If
            _headerTicksLeft -= 1
            If _headerTicksLeft > 0 Then
                Tr($"OnRefitTick: header not found, {_headerTicksLeft} tick(s) left -> timer restarted")
                _refitTimer.Start()
            Else
                If Tracing Then TraceTree("OnRefitTick: header search gave up; full tree")
                Report($"AcroPDF: «{DocumentHeaderText}» nu a fost găsit în control ({DescribeTree()}).")
                EndRecording("document open (header not found)")
            End If
        Catch ex As Exception
            Tr("OnRefitTick EXCEPTION: " & ex.ToString())
            GlobalErrorLog.Write("AcroPdfSurface.OnRefitTick", ex)
        End Try
    End Sub

    ' (Re)starts the header search: first tick after RefitDelayMs, then one per tick until found.
    Private Sub ScheduleHeaderSearch(ticks As Integer)
        _headerTicksLeft = Math.Max(_headerTicksLeft, ticks)
        _refitTimer.Stop()
        If ViewerShown() Then _refitTimer.Start()
        Tr($"ScheduleHeaderSearch({ticks}): ticksLeft={_headerTicksLeft} timerStarted={_refitTimer.Enabled} interval={_refitTimer.Interval} ms")
    End Sub

    ''' <summary>
    ''' Slice 0078: when True, every Save As of the loaded document is forced back onto its own
    ''' path, off screen (see <see cref="AdobeSaveTrap"/>). Takes effect on the document on screen.
    ''' </summary>
    Public Property SaveTrapEnabled As Boolean
        Get
            Return _saveTrapEnabled
        End Get
        Set(value As Boolean)
            Tr($"SaveTrapEnabled = {value}")
            _saveTrapEnabled = value
            If value Then
                StartSaveTrap()
            Else
                _saveTrap.Stop()
            End If
        End Set
    End Property

    ''' <summary>The document last loaded, or Nothing.</summary>
    Public ReadOnly Property LoadedPath As String
        Get
            Return _loadedPath
        End Get
    End Property

    ''' <summary>
    ''' The processes that own Adobe's view windows inside this control -- the ones whose
    ''' Save As belongs to our document. Empty before Adobe has built its windows.
    ''' </summary>
    Public Function OwnerPids() As IEnumerable(Of Integer)
        Dim pids As New List(Of Integer)()
        Try
            If _host Is Nothing OrElse Not _host.IsHandleCreated Then Return pids
            Dim renderers As New HashSet(Of Integer)()
            For Each h As IntPtr In AdobeNativeMethods.Descendants(_host.Handle)
                If Not AdobeNativeMethods.GetClass(h).StartsWith(AdobeViewClassPrefix, StringComparison.Ordinal) Then Continue For
                Dim pid As Integer = AdobeNativeMethods.OwnerPid(h)
                If pid > 0 Then renderers.Add(pid)
            Next
            pids.AddRange(renderers)

            ' The rest of the family, among the Adobe processes only (name + parent).
            Dim self As Integer = Environment.ProcessId
            Dim parents As New Dictionary(Of Integer, Integer)()
            For Each name As String In AdobeProcessNames
                For Each p As Diagnostics.Process In Diagnostics.Process.GetProcessesByName(name)
                    Using p
                        parents(p.Id) = AdobeNativeMethods.ParentPid(p.Id)
                    End Using
                Next
            Next
            Dim brokers As New HashSet(Of Integer)()
            For Each kv As KeyValuePair(Of Integer, Integer) In parents
                If kv.Value = self Then brokers.Add(kv.Key)                      ' started by K-BOT
                If renderers.Contains(kv.Key) AndAlso parents.ContainsKey(kv.Value) Then brokers.Add(kv.Value)
            Next
            For Each kv As KeyValuePair(Of Integer, Integer) In parents
                If brokers.Contains(kv.Key) OrElse brokers.Contains(kv.Value) Then
                    If Not pids.Contains(kv.Key) Then pids.Add(kv.Key)
                End If
            Next
            ' Asked on every sweep: traced only when the answer changed.
            If Tracing Then
                Dim line As String = $"renderers=[{String.Join(",", renderers)}] " &
                    $"adobe processes=[{String.Join(", ", parents.Select(Function(kv) $"{kv.Key}<-parent {kv.Value}"))}] " &
                    $"brokers=[{String.Join(",", brokers)}] self={self} -> [{String.Join(",", pids)}]"
                If line <> _lastOwnerPidsTrace Then
                    _lastOwnerPidsTrace = line
                    Tr("OwnerPids changed: " & line)
                End If
            End If
        Catch ex As Exception
            Tr("OwnerPids EXCEPTION: " & ex.ToString())
            ' Called from the trap's timer: a failed walk only means «no pids this time».
            GlobalErrorLog.Write("AcroPdfSurface.OwnerPids", ex)
        End Try
        Return pids
    End Function

    ''' <summary>
    ''' True when this viewer is on screen. Two viewers (DDF and ORD) share one Acrobat, so a Save As
    ''' whose offered name matches neither document goes to the one the operator is looking at.
    ''' </summary>
    Public Function IsOnScreen() As Boolean
        If _host Is Nothing OrElse Not _host.IsHandleCreated OrElse Not ViewerShown() Then Return False
        Return AdobeNativeMethods.IsWindowVisible(_host.Handle)
    End Function

    ' Wrapped: called from a setter and after a load.
    Private Sub StartSaveTrap()
        Try
            Tr($"StartSaveTrap: enabled={_saveTrapEnabled} host={_host IsNot Nothing} loaded={If(_loadedPath, "(none)")}; " & ScreenText())
            If Not _saveTrapEnabled OrElse _host Is Nothing OrElse String.IsNullOrEmpty(_loadedPath) Then Return
            AttachForm()
            If ViewerShown() Then _saveTrap.Resume() Else _saveTrap.Pause()
            _saveTrap.Start(_loadedPath, OwnerPids())
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfSurface.StartSaveTrap", ex)
        End Try
    End Sub

    ' Trap events arrive on the UI thread (hook + WinForms timer); forwarded as they are.
    Private Sub OnTrapSaved(path As String)
        Try
            Tr($"Trap reported a save of «{path}» -> raising DocumentSaved")
            RaiseEvent DocumentSaved(path)
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfSurface.OnTrapSaved", ex)
        End Try
    End Sub

    Private Sub OnTrapFailed(reason As String)
        Try
            Tr($"Trap reported a cancelled save: {reason} -> raising SaveTrapFailed")
            RaiseEvent SaveTrapFailed(reason)
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfSurface.OnTrapFailed", ex)
        End Try
    End Sub

    ''' <summary>False when AcroPDF is not registered — the caller must then fall back or say so.</summary>
    Public ReadOnly Property IsAvailable As Boolean
        Get
            Return Not String.IsNullOrEmpty(_clsid)
        End Get
    End Property

    ''' <summary>The control's own version string, or Nothing. Available only after a first load.</summary>
    Public Function TryReadVersion() As String
        Return If(_host Is Nothing, Nothing, _host.TryReadVersion())
    End Function

    ''' <summary>
    ''' Loads the document and leaves it in the wanted state. Never throws — every failure path
    ''' returns a Romanian sentence the caller can put on screen.
    ''' </summary>
    Public Async Function ShowDocumentAsync(pdfPath As String) As Task(Of AcroPdfResult)
        Dim clock As Diagnostics.Stopwatch = Diagnostics.Stopwatch.StartNew()
        Try
            AcroPdfTraceLog.BeginSession("AcroPdfSurface", $"load of «{pdfPath}»")
            Tr($"Environment: clsid={If(_clsid, "(not registered)")} pid={Environment.ProcessId} " &
               $"os={Environment.OSVersion} 64bit={Environment.Is64BitProcess} dpi={_panel.DeviceDpi}")
            Tr($"ShowDocumentAsync BEGIN «{pdfPath}» exists={Not String.IsNullOrWhiteSpace(pdfPath) AndAlso File.Exists(pdfPath)} " &
               $"size={If(Not String.IsNullOrWhiteSpace(pdfPath) AndAlso File.Exists(pdfPath), New FileInfo(pdfPath).Length.ToString() & " bytes", "-")} " &
               $"available={IsAvailable} host={_host IsNot Nothing} previous={If(_loadedPath, "(none)")}; " & ScreenText())
            If Not IsAvailable Then
                EndRecording("BLOCKING: AcroPDF is not registered")
                Return New AcroPdfResult(AcroPdfStatus.NotRegistered,
                                         "Controlul Adobe (AcroPDF) nu este înregistrat pe această mașină.")
            End If
            If String.IsNullOrWhiteSpace(pdfPath) OrElse Not File.Exists(pdfPath) Then
                EndRecording("BLOCKING: the file is not on disk")
                Return New AcroPdfResult(AcroPdfStatus.FileMissing, "Documentul nu există pe disc.")
            End If

            Dim host As AcroPdfHost = EnsureHost()
            If host Is Nothing Then
                EndRecording("BLOCKING: the control could not be created")
                Return New AcroPdfResult(AcroPdfStatus.Failed,
                                         "Controlul Adobe nu a putut fi creat. Detalii în jurnalul de erori.")
            End If

            Report($"AcroPDF: încarc «{Path.GetFileName(pdfPath)}».")
            ' The previous document's header search / Read Mode wait belongs to that document (the
            ' 24.09.2026 trace: a search left running ended the NEXT load's recording as «open»).
            _refitTimer.Stop()
            _headerTicksLeft = 0
            DisarmReadMode()
            ' A save of the previous document must finish before it is replaced.
            If _saveTrap.IsBusy Then _saveTrap.WaitWhileBusy(5000)
            _saveTrap.Stop()
            TracedLoad(host, pdfPath, clock)
            _loadedPath = pdfPath
            ' Started right after the load, NOT after the pane wait: the operator may sign while
            ' Adobe is still laying out, and the pids are re-read on every sweep anyway.
            StartSaveTrap()

            If ReadMode Then
                ' No pane wait, no collapse, no hiding, no header timer: one Ctrl+H when Adobe
                ' says the page is laid out (only on a control that has not had it). The
                ' recording ends when it is sent.
                _deadRetryDone = False
                ArmReadMode()
                Tr($"ShowDocumentAsync END (read mode armed) after {clock.ElapsedMilliseconds} ms")
                Return New AcroPdfResult(AcroPdfStatus.Shown, "", collapsed:=True)
            End If

            If Not Await WaitForPaneTreeAsync().ConfigureAwait(True) Then
                ' MEASURED 23.09.2026: now and then Adobe builds NOTHING in the control (the panel
                ' stays empty) and the same file opens fine on the next try. One fresh control and
                ' one more load before giving up; the trap follows the new load.
                Report($"AcroPDF: controlul nu are vederi Adobe ({DescribeTree()}) — îl recreez și reîncarc o dată.")
                Clear()
                host = EnsureHost()
                If host Is Nothing Then
                    EndRecording("BLOCKING: the control could not be recreated")
                    Return New AcroPdfResult(AcroPdfStatus.Failed,
                                             "Controlul Adobe nu a putut fi recreat. Detalii în jurnalul de erori.")
                End If
                TracedLoad(host, pdfPath, clock)
                _loadedPath = pdfPath
                StartSaveTrap()
                If Not Await WaitForPaneTreeAsync().ConfigureAwait(True) Then
                    Report($"AcroPDF: nici a doua încărcare nu a construit vederile ({DescribeTree()}).")
                    If Tracing Then TraceTree("ShowDocumentAsync: tree when giving up")
                    EndRecording($"BLOCKING: second load built no Adobe views after {clock.ElapsedMilliseconds} ms")
                    ' Said as what it is: Adobe did not SHOW the document (the old note blamed the
                    ' collapse, which never ran).
                    Return New AcroPdfResult(AcroPdfStatus.Shown,
                                             "Adobe nu a afișat documentul — vezi jurnalul.", collapsed:=False)
                End If
                Report("AcroPDF: a doua încărcare a reușit.")
            End If
            Tr($"ShowDocumentAsync: pane tree ready at {clock.ElapsedMilliseconds} ms -> collapse")
            Dim collapsed As Boolean = Await CollapsePanesAsync().ConfigureAwait(True)
            Tr($"ShowDocumentAsync: collapse -> {collapsed} at {clock.ElapsedMilliseconds} ms -> hide chrome")
            HideChrome()
            ' The header can appear only after the page has rendered: searched on the refit timer.
            _headerTicksLeft = 0
            ScheduleHeaderSearch(HeaderSearchTicksAfterLoad)
            If Tracing Then
                Tr($"ShowDocumentAsync: control version «{If(TryReadVersion(), "(none)")}»")
                TraceTree("ShowDocumentAsync: tree at the end of the load")
            End If

            ' The recording ends when the header search ends (OnRefitTick): that is the last step
            ' of opening the document.
            Tr($"ShowDocumentAsync END shown, collapsed={collapsed}, after {clock.ElapsedMilliseconds} ms")
            Return New AcroPdfResult(AcroPdfStatus.Shown, "", collapsed)
        Catch ex As Exception
            Tr($"ShowDocumentAsync EXCEPTION after {clock.ElapsedMilliseconds} ms: " & ex.ToString())
            EndRecording("BLOCKING: exception " & ex.GetType().Name & ": " & ex.Message)
            GlobalErrorLog.Write("AcroPdfSurface.ShowDocumentAsync", ex)
            Return New AcroPdfResult(AcroPdfStatus.Failed,
                                     "Documentul nu a putut fi afișat. Detalii în jurnalul de erori.")
        End Try
    End Function

    ''' <summary>
    ''' Empties the surface by DESTROYING the control; the next load recreates it. Setting <c>src</c>
    ''' to an empty string returns success and changes nothing on this build.
    ''' </summary>
    Public Sub Clear()
        Try
            Tr($"Clear: host={_host IsNot Nothing} loaded={If(_loadedPath, "(none)")} trapBusy={_saveTrap.IsBusy}")
            ' A pressed Save still writing the file must not lose its control halfway.
            If _saveTrap.IsBusy Then _saveTrap.WaitWhileBusy(5000)
            _saveTrap.Stop()
            DisarmReadMode()
            _loadedPath = Nothing
            Dim host As AcroPdfHost = _host
            _host = Nothing
            If host Is Nothing Then Return
            Dim handle As String = If(host.IsHandleCreated, AcroPdfTraceLog.Hex(host.Handle), "(no handle)")
            _panel.Controls.Remove(host)
            host.Dispose()
            Tr($"Clear: control {handle} removed and disposed")
        Catch ex As Exception
            Tr("Clear EXCEPTION: " & ex.ToString())
            GlobalErrorLog.Write("AcroPdfSurface.Clear", ex)
        End Try
    End Sub

    ' Creates the control on first use. AxHost needs a handle before GetOcx() returns anything.
    Private Function EnsureHost() As AcroPdfHost
        If _host IsNot Nothing Then Return _host
        Try
            Tr($"EnsureHost: creating AxHost for clsid {_clsid}")
            Dim host As New AcroPdfHost(_clsid) With {.Dock = DockStyle.Fill, .Name = "axAcroPdf"}
            _panel.Controls.Add(host)
            Dim unused As IntPtr = host.Handle
            _host = host
            Tr($"EnsureHost: control created, handle={AcroPdfTraceLog.Hex(unused)} bounds={host.Bounds} " &
               $"ocx={host.GetOcx() IsNot Nothing} children={AdobeNativeMethods.Descendants(unused).Count}")
            Return host
        Catch ex As Exception
            Tr("EnsureHost EXCEPTION: " & ex.ToString())
            GlobalErrorLog.Write("AcroPdfSurface.EnsureHost", ex)
            Return Nothing
        End Try
    End Function

    ' ── Step 2: wait for Adobe to build AND lay out its panes ───────────────────
    '
    ' Waits for BOTH conditions, because they fail separately: the document view can have size while
    ' the tab strip does not exist yet, which is precisely what the 10:48 log caught.
    '
    ' TWO PHASES (24.09.2026, from the client trace):
    '  1. Adobe STARTING. On the first load of a session the control held ZERO windows for the whole
    '     4 s the old single phase waited, and the control was then destroyed -- in the middle of
    '     Acrobat's cold start. The second control got its first window 24 ms after LoadFile. So
    '     first wait (up to ColdStartMaxMs) for the control to hold ANY window.
    '  2. Adobe LAYING OUT: the old 10 x 400 ms. Attempts made while the form's script alerts are
    '     up do NOT count, and nothing is nudged then: the alerts are modal to Adobe's view, which
    '     is built only after they close (and a focus call would pull the foreground off them).
    '     Capped by LayoutMaxMs so an alert nobody closes cannot hold the load forever.
    Private Async Function WaitForPaneTreeAsync() As Task(Of Boolean)
        Dim clock As Diagnostics.Stopwatch = Diagnostics.Stopwatch.StartNew()

        ' Phase 1.
        Dim windows As Integer = AdobeNativeMethods.Descendants(_host.Handle).Count
        If windows = 0 Then Tr($"WaitForPaneTree: control empty, waiting up to {ColdStartMaxMs} ms for Adobe to start")
        While windows = 0 AndAlso clock.ElapsedMilliseconds < ColdStartMaxMs
            If Not _saveTrap.InScriptBurst Then Nudge()
            Await Task.Delay(AttemptIntervalMs).ConfigureAwait(True)
            If _host Is Nothing Then Return False
            windows = AdobeNativeMethods.Descendants(_host.Handle).Count
        End While
        If windows = 0 Then
            Report($"AcroPDF: ATENȚIE — după {ColdStartMaxMs} ms controlul tot nu are nicio fereastră Adobe.")
            Return False
        End If
        Tr($"WaitForPaneTree: control has {windows} window(s) at {clock.ElapsedMilliseconds} ms")

        ' Phase 2.
        Dim attempt As Integer = 0
        Dim lastTree As String = Nothing
        Dim waitingOnScript As Boolean = False
        While attempt < MaxLayoutAttempts AndAlso clock.ElapsedMilliseconds < LayoutMaxMs
            Dim nodes As List(Of AdobeWindowNode) = Walk()
            Dim page As AdobeWindowNode = FindNode(nodes, PageViewText)
            Dim strip As AdobeWindowNode = FindNode(nodes, TabStripText)
            Dim laidOut As Boolean = page IsNot Nothing AndAlso page.Width > 0 AndAlso page.Height > 0
            Dim built As Boolean = strip IsNot Nothing AndAlso strip.Height > 0
            If Tracing Then
                ' The tree only when it changed since the last look.
                Dim tree As String = String.Join("|", nodes.Select(Function(n) NodeText(n)))
                If tree <> lastTree Then
                    lastTree = tree
                    Tr($"WaitForPaneTree at {clock.ElapsedMilliseconds} ms (attempt {attempt}/{MaxLayoutAttempts}): " &
                       $"page={NodeText(page)} strip={NodeText(strip)} laidOut={laidOut} built={built} panel={_panel.ClientSize}")
                    TraceNodes("WaitForPaneTree: tree changed", nodes)
                End If
            End If

            If laidOut AndAlso built Then
                Report($"AcroPDF: arborele de panouri a apărut după {clock.ElapsedMilliseconds} ms.")
                Return True
            End If

            If _saveTrap.InScriptBurst Then
                If Not waitingOnScript Then Tr("WaitForPaneTree: Adobe script alerts are up -- waiting, attempts not counted")
                waitingOnScript = True
            Else
                If waitingOnScript Then Tr("WaitForPaneTree: script alerts over -- attempts count again")
                waitingOnScript = False
                attempt += 1
                Nudge()
            End If
            Await Task.Delay(AttemptIntervalMs).ConfigureAwait(True)
            If _host Is Nothing Then Return False
        End While

        Report($"AcroPDF: ATENȚIE — după {clock.ElapsedMilliseconds} ms ({attempt} încercări) Adobe tot nu " &
               "are un arbore de panouri utilizabil. Nu colapsez și nu ascund nimic — o acțiune pe un " &
               "arbore incomplet ar lăsa suprafața într-o stare pe care nimeni nu a cerut-o.")
        Return False
    End Function

    ' Focus first, then a SIZE CHANGE — Adobe recomputes its layout on a size change, not on a
    ' repaint. Reached from WaitForPaneTreeAsync (wrapped by ShowDocumentAsync).
    Private Sub Nudge()
        AdobeWindowHosting.FocusWindow(_host.Handle)
        AdobeWindowHosting.NudgeRedraw(_host.Handle,
                                       New Rectangle(0, 0, _panel.ClientSize.Width, _panel.ClientSize.Height))
    End Sub

    ' ── Step 3: Adobe's own collapse ────────────────────────────────────────────
    Private Async Function CollapsePanesAsync() As Task(Of Boolean)
        Dim nodes As List(Of AdobeWindowNode) = Walk()
        Dim strip As AdobeWindowNode = FindNode(nodes, TabStripText)
        Tr($"CollapsePanes: strip={NodeText(strip)} -> {If(strip Is Nothing, "missing", If(strip.Width > 0, "open, one click", "already collapsed, two clicks"))}")
        ' WaitForPaneTreeAsync has already guaranteed both of these; the guards stay because Adobe
        ' destroys and recreates these windows and the tree can change under us.
        If strip Is Nothing OrElse strip.Height <= 0 Then
            Report("AcroPDF: banda de file a dispărut din arbore între verificare și colapsare.")
            Return False
        End If

        If strip.Width > 0 Then
            If Not Await ClickCollapseAsync(nodes, strip).ConfigureAwait(True) Then Return False
        Else
            ' Already collapsed. The floating bar follows the collapse ACTION, not the STATE, so
            ' re-open and close again to make Adobe perform it in this session.
            Report("AcroPDF: deja colapsat — comut de două ori, ca acțiunea să se producă acum.")
            If Not Await ClickCollapseAsync(nodes, strip).ConfigureAwait(True) Then Return False
            Dim reopened As List(Of AdobeWindowNode) = Walk()
            Dim s2 As AdobeWindowNode = FindNode(reopened, TabStripText)
            If s2 Is Nothing Then Return False
            If Not Await ClickCollapseAsync(reopened, s2).ConfigureAwait(True) Then Return False
        End If

        ' The objective signal: the document view reaches the left edge and fills the panel.
        Dim after As List(Of AdobeWindowNode) = Walk()
        Dim page As AdobeWindowNode = FindNode(after, PageViewText)
        Dim ok As Boolean = page IsNot Nothing AndAlso page.Bounds.X = 0 AndAlso page.Width > 0
        If Tracing Then
            Tr($"CollapsePanes: after -> page={NodeText(page)} strip={NodeText(FindNode(after, TabStripText))} ok={ok}")
            TraceNodes("CollapsePanes: tree after the collapse", after)
        End If
        If ok Then
            Report($"AcroPDF: panouri colapsate — documentul ocupă {page.Width}x{page.Height} de la x=0.")
        Else
            Report("AcroPDF: ATENȚIE — după colapsare documentul NU a ajuns la marginea stângă. " &
                   "Un click sintetic peste graniță de proces nu e garantat onorat.")
        End If
        Return ok
    End Function

    Private Async Function ClickCollapseAsync(nodes As List(Of AdobeWindowNode),
                                              strip As AdobeWindowNode) As Task(Of Boolean)
        Dim button As AdobeWindowNode = nodes.
            Where(Function(n) String.Equals(n.Text, CollapseButtonText, StringComparison.OrdinalIgnoreCase)).
            OrderBy(Function(n) Math.Abs(n.Bounds.X - (strip.Bounds.X + strip.Width))).
            FirstOrDefault()
        If Tracing Then
            Dim buttons As List(Of AdobeWindowNode) = nodes.
                Where(Function(n) String.Equals(n.Text, CollapseButtonText, StringComparison.OrdinalIgnoreCase)).ToList()
            AcroPdfTraceLog.WriteBlock("AcroPdfSurface", $"ClickCollapse: {buttons.Count} candidate button(s), chosen {NodeText(button)}",
                                       buttons.Select(Function(b) NodeText(b)))
        End If
        If button Is Nothing Then
            Report("AcroPDF: butonul de colapsare nu există în arbore.")
            Return False
        End If
        AdobeWindowHosting.ClickCentre(button.Hwnd)
        Tr($"ClickCollapse: clicked centre of {AcroPdfTraceLog.Hex(button.Hwnd)} (alive={AdobeWindowHosting.IsAlive(button.Hwnd)}); settling {CollapseSettleMs} ms")
        Await Task.Delay(CollapseSettleMs).ConfigureAwait(True)
        Return True
    End Function

    ' ── Step 4: hide what the collapse leaves behind ────────────────────────────
    '
    ' The collapse takes the panes to zero width and reflows the document, but the strip windows are
    ' still THERE and can still paint. Hiding them is what the bench's «Ascunde chrome-ul» does, and
    ' it is the second half of the state the operator asked for.
    '
    ' Hiding alone would NOT be enough — it leaves the width, so Adobe never reflows and the document
    ' stays inset. Collapse first, hide second; the order is the whole point.
    Private Sub HideChrome()
        Try
            Dim nodes As List(Of AdobeWindowNode) = Walk()
            For Each text As String In ChromeToHide
                For Each n As AdobeWindowNode In nodes.
                    Where(Function(x) String.Equals(x.Text, text, StringComparison.OrdinalIgnoreCase))
                    If Not AdobeWindowHosting.IsAlive(n.Hwnd) Then Continue For
                    ' Classify BEFORE touching it: afterwards everything looks hidden, which is how a
                    ' no-op gets logged as a success (the lesson of slice 0023 pass 4).
                    Dim outcome As HideOutcome = HideOutcomeClassifier.Classify(
                        found:=True, visible:=AdobeWindowHosting.IsVisible(n.Hwnd),
                        width:=n.Width, height:=n.Height)
                    Tr($"HideChrome: {NodeText(n)} -> {outcome}")
                    If outcome = HideOutcome.Hidden Then
                        AdobeWindowHosting.Hide(n.Hwnd)
                        Report($"AcroPDF: ascuns «{n.Text}» {n.Width}x{n.Height}.")
                    End If
                Next
            Next
        Catch ex As Exception
            Tr("HideChrome EXCEPTION: " & ex.ToString())
            GlobalErrorLog.Write("AcroPdfSurface.HideChrome", ex)
        End Try
    End Sub

    ' ── Step 5: the document header strip ───────────────────────────────────────
    '
    ' Hidden, and the window directly below it (same parent, top edge on the header's bottom edge,
    ' overlapping horizontally) moved up and made taller by the header's height, so the document
    ' takes the whole panel. The header's own rectangle is kept by Adobe even when hidden, so a
    ' re-run after a resize finds the view again where Adobe put it back, and moves it again.
    ' Returns True when the header was found. Searched over the WHOLE tree (EnumChildWindows), not
    ' the depth-limited Walk: the first version used Walk and never logged a thing (23.09.2026).
    Private Function HideDocumentHeader() As Boolean
        Dim found As Boolean = False
        Try
            If _host Is Nothing OrElse Not _host.IsHandleCreated Then Return False
            Dim all As List(Of IntPtr) = AdobeNativeMethods.Descendants(_host.Handle)
            Tr($"HideDocumentHeader: scanning {all.Count} descendant(s) of {AcroPdfTraceLog.Hex(_host.Handle)}")
            For Each hwnd As IntPtr In all
                If Not String.Equals(AdobeNativeMethods.GetTitle(hwnd), DocumentHeaderText, StringComparison.OrdinalIgnoreCase) Then Continue For
                Dim alive As Boolean = AdobeWindowHosting.IsAlive(hwnd)
                Dim header As Rectangle = AdobeNativeMethods.RectInParent(hwnd)
                Tr($"HideDocumentHeader: candidate {AcroPdfTraceLog.Hex(hwnd)} alive={alive} rect={header} " &
                   $"visibleStyle={AdobeNativeMethods.IsVisibleStyleSet(hwnd)} parent={AcroPdfTraceLog.Hex(AdobeNativeMethods.GetParent(hwnd))}")
                If Not alive Then Continue For
                If header.Height <= 0 OrElse header.Width <= 0 Then Continue For
                found = True
                If AdobeNativeMethods.IsVisibleStyleSet(hwnd) Then
                    AdobeWindowHosting.Hide(hwnd)
                    Report($"AcroPDF: ascuns «{DocumentHeaderText}» {header.Width}x{header.Height}.")
                End If
                Dim parent As IntPtr = AdobeNativeMethods.GetParent(hwnd)
                Dim sib As IntPtr = AdobeNativeMethods.GetWindow(parent, AdobeNativeMethods.GW_CHILD)
                While sib <> IntPtr.Zero
                    If sib <> hwnd AndAlso AdobeNativeMethods.IsVisibleStyleSet(sib) Then
                        Dim r As Rectangle = AdobeNativeMethods.RectInParent(sib)
                        Dim below As Boolean = Math.Abs(r.Top - header.Bottom) <= 2 AndAlso r.Right > header.Left AndAlso r.Left < header.Right
                        Tr($"HideDocumentHeader: sibling {AcroPdfTraceLog.Hex(sib)} «{AdobeNativeMethods.GetTitle(sib)}» " &
                           $"class={AdobeNativeMethods.GetClass(sib)} rect={r} directlyBelow={below}")
                        If below Then
                            Dim moved As Boolean = AdobeNativeMethods.MoveWindow(sib, r.X, header.Top, r.Width, r.Height + (r.Top - header.Top), True)
                            Tr($"HideDocumentHeader: MoveWindow({AcroPdfTraceLog.Hex(sib)}) -> {moved}, now {AdobeNativeMethods.RectInParent(sib)}")
                            Report($"AcroPDF: vederea de sub antet («{AdobeNativeMethods.GetTitle(sib)}») urcată la y={header.Top}, " &
                                   $"înălțime {r.Height} -> {r.Height + (r.Top - header.Top)}.")
                        End If
                    End If
                    sib = AdobeNativeMethods.GetWindow(sib, AdobeNativeMethods.GW_HWNDNEXT)
                End While
            Next
        Catch ex As Exception
            Tr("HideDocumentHeader EXCEPTION: " & ex.ToString())
            GlobalErrorLog.Write("AcroPdfSurface.HideDocumentHeader", ex)
        End Try
        Return found
    End Function

    ' ── Read Mode path (Ctrl+H), engine «ActiveX -- mod citire» ─────────────────
    '
    ' NO TIMER and no window fixing. One out-of-context WinEvent hook (show / state / location
    ' changes) lives from the load until Ctrl+H is sent. Each event about a window inside the
    ' control, or about our own form, re-checks the conditions below; the first time they all hold,
    ' Ctrl+H is sent once and the hook is removed:
    '  1. the viewer is on screen;
    '  2. the page view («AVPageView») is visible and has a size -- Adobe has laid the document out;
    '  3. our form is ENABLED -- a modal script alert (owned by our form) is not up. The clicker
    '     closes those, and the form being enabled again is itself an event here;
    '  4. our form holds the foreground -- SendInput types into whatever window has the keyboard, so
    '     otherwise the keys could land in another program. Form.Activated re-checks.
    ' Then the keyboard focus must actually be inside the control, or nothing is sent: Ctrl+H in a
    ' K-BOT text box would be a backspace.
    ' Adobe postpones its first layout until the control gets input (see the class notes): the first
    ' event about a window in the control wakes it ONCE (focus + size change), never while script
    ' alerts are up.

    '
    ' AFTER SCRIPT ALERTS (operator, 24.09.2026): on documents without alerts Ctrl+H holds every
    ' time; on documents whose form scripts raise alerts it held only sometimes. In the client
    ' trace («acropdf_trace (2).log», #1 and #5) the keys went out ~0.5 s after the clicker closed
    ' the last alert, and the tree at that moment was not Adobe's finished layout (no toolbar row,
    ' tab strip zero-wide and hidden). So Ctrl+H also waits for the alert burst to be OVER
    ' (AdobeSaveTrap.InScriptBurst false, i.e. ScriptBurstQuietMs without a new alert); the trap's
    ' ScriptBurstEnded event re-checks, since Adobe may raise no window event after that.
    '
    ' THE EMPTY CONTROL (same trace, #3 and #4): after the generate-document button a freshly created
    ' control got NO window at all from Adobe -- not even «Acrobat External Window», which normally
    ' appears within 0.5 s -- for 17 s, and a second document loaded into it stayed empty too.
    ' Destroying it and creating a new one (what the operator's click on another revision did,
    ' #5) worked at once. The cause is not known. So ONE one-shot check, DeadControlMs after the
    ' load: a control still holding no window at all is replaced and the document loaded again,
    ' once. This is the only timer of this path, and it never touches Adobe's windows.

    Private Const PageViewTitle As String = "AVPageView"
    Private Const ExternalWindowTitle As String = "Acrobat External Window"
    Private Const VK_H As UShort = &H48US
    ' Longer than Acrobat's cold start seen on the client first (> 4 s would have been cut), far
    ' longer than a warm load (first window within 0.5 s).
    Private Const DeadControlMs As Integer = 5000
    Private ReadOnly _deadCheckTimer As New Timer() With {.Interval = DeadControlMs}
    ' The empty-control replacement already happened for this load.
    Private _deadRetryDone As Boolean
    Private _readModePending As Boolean
    Private _readModeHook As IntPtr
    Private _readModeWoken As Boolean
    Private _inReadModeCheck As Boolean
    ' The last reason for waiting, traced only when it changes.
    Private _readModeWait As String
    ' Kept in a field: the delegate must outlive the hook (a collected callback crashes the process).
    Private _readModeProc As AdobeNativeMethods.WinEventProc

    ' Reached from ShowDocumentAsync (wrapped).
    Private Sub ArmReadMode()
        DisarmReadMode()
        _readModePending = True
        _readModeWoken = False
        _readModeWait = Nothing
        If ViewerShown() Then _deadCheckTimer.Start()
        If _readModeProc Is Nothing Then _readModeProc = AddressOf OnReadModeEvent
        _readModeHook = AdobeNativeMethods.SetWinEventHook(
            AdobeNativeMethods.EVENT_OBJECT_SHOW, AdobeNativeMethods.EVENT_OBJECT_LOCATIONCHANGE,
            IntPtr.Zero, _readModeProc, 0UI, 0UI, AdobeNativeMethods.WINEVENT_OUTOFCONTEXT)
        Dim err As Integer = Runtime.InteropServices.Marshal.GetLastWin32Error()
        Tr($"ReadMode: armed, hook={AcroPdfTraceLog.Hex(_readModeHook)}" & If(_readModeHook = IntPtr.Zero, $" (Win32 error {err})", ""))
        If _readModeHook = IntPtr.Zero Then
            Report($"AcroPDF: ATENȚIE — evenimentele Adobe nu pot fi urmărite (eroarea {err}); " &
                   "modul citire se trimite doar la reactivarea ferestrei K-BOT.")
        End If
        AttachForm()
        TrySendReadMode("armed")
    End Sub

    Private Sub DisarmReadMode()
        _readModePending = False
        _deadCheckTimer.Stop()
        If _readModeHook <> IntPtr.Zero Then
            AdobeNativeMethods.UnhookWinEvent(_readModeHook)
            Tr($"ReadMode: hook {AcroPdfTraceLog.Hex(_readModeHook)} removed")
            _readModeHook = IntPtr.Zero
        End If
    End Sub

    ' WinEvent callback (UI thread, out of context). Boundary: log and swallow.
    Private Sub OnReadModeEvent(hook As IntPtr, eventType As UInteger, hwnd As IntPtr,
                                idObject As Integer, idChild As Integer,
                                threadId As UInteger, timestamp As UInteger)
        Try
            ' Global hook: mouse moves, carets and every other program arrive here too. Cheap
            ' filters first.
            If Not _readModePending OrElse idObject <> AdobeNativeMethods.OBJID_WINDOW OrElse hwnd = IntPtr.Zero Then Return
            If _host Is Nothing OrElse Not _host.IsHandleCreated Then Return
            Dim isForm As Boolean = _form IsNot Nothing AndAlso _form.IsHandleCreated AndAlso hwnd = _form.Handle
            If Not isForm AndAlso Not AdobeNativeMethods.IsChild(_host.Handle, hwnd) Then
                ' For the empty-control case: Adobe's first window showing up OUTSIDE our control.
                If eventType = AdobeNativeMethods.EVENT_OBJECT_SHOW AndAlso Tracing AndAlso
                   String.Equals(AdobeNativeMethods.GetTitle(hwnd), ExternalWindowTitle, StringComparison.Ordinal) Then
                    Tr($"ReadMode: «{ExternalWindowTitle}» {AcroPdfTraceLog.Hex(hwnd)} shown OUTSIDE the control: " &
                       $"parent={AcroPdfTraceLog.Hex(AdobeNativeMethods.GetParent(hwnd))} pid={AdobeNativeMethods.OwnerPid(hwnd)} " &
                       $"rect={AdobeNativeMethods.RectOnScreen(hwnd)}")
                End If
                Return
            End If
            ' Adobe put a window in the control: it is not the empty-control case.
            If Not isForm AndAlso _deadCheckTimer.Enabled Then
                _deadCheckTimer.Stop()
                Tr("ReadMode: Adobe is building in the control -> empty-control check cancelled")
            End If

            If Not isForm AndAlso Not _readModeWoken AndAlso Not _saveTrap.InScriptBurst AndAlso ViewerShown() Then
                _readModeWoken = True
                Tr($"ReadMode: first event inside the control ({EventName(eventType)} {AcroPdfTraceLog.Hex(hwnd)} " &
                   $"«{AdobeNativeMethods.GetTitle(hwnd)}») -> wake Adobe once (focus + size change)")
                Nudge()
            End If
            TrySendReadMode($"{EventName(eventType)} {AcroPdfTraceLog.Hex(hwnd)} «{AdobeNativeMethods.GetTitle(hwnd)}»")
        Catch ex As Exception
            Tr("OnReadModeEvent EXCEPTION: " & ex.ToString())
            GlobalErrorLog.Write("AcroPdfSurface.OnReadModeEvent", ex)
        End Try
    End Sub

    ' The one-shot empty-control check (see the notes above PageViewTitle). Timer: log and swallow.
    Private Sub OnDeadCheckTick(sender As Object, e As EventArgs)
        Try
            _deadCheckTimer.Stop()
            If Not _readModePending OrElse _host Is Nothing OrElse Not _host.IsHandleCreated Then Return
            ' Off screen: ApplyScreenState starts the check again when the viewer is back.
            If Not ViewerShown() Then Return
            Dim windows As Integer = AdobeNativeMethods.Descendants(_host.Handle).Count
            Tr($"EmptyControlCheck: {windows} window(s) in {AcroPdfTraceLog.Hex(_host.Handle)} {DeadControlMs} ms after the load; replaced already={_deadRetryDone}")
            If windows > 0 Then Return
            ' A script alert is up (one left for the operator stays up until they press OK) and
            ' Adobe builds nothing while it is: the control is waiting, not dead. Check again later.
            If _saveTrap IsNot Nothing AndAlso _saveTrap.InScriptBurst Then
                Tr("EmptyControlCheck: script alert(s) showing -> postponed")
                _deadCheckTimer.Start()
                Return
            End If

            Dim path As String = _loadedPath
            If _deadRetryDone OrElse String.IsNullOrEmpty(path) Then
                Report($"AcroPDF: ATENȚIE — nici controlul nou nu a primit nimic de la Adobe în {DeadControlMs} ms.")
                DisarmReadMode()
                EndRecording("BLOCKING: the replacement control stayed empty too")
                Return
            End If

            Report($"AcroPDF: controlul a rămas gol {DeadControlMs} ms după încărcare — îl recreez și reîncarc documentul o dată.")
            Clear()
            Dim host As AcroPdfHost = EnsureHost()
            If host Is Nothing Then
                EndRecording("BLOCKING: the empty control could not be recreated")
                Return
            End If
            _deadRetryDone = True
            TracedLoad(host, path, Diagnostics.Stopwatch.StartNew())
            _loadedPath = path
            StartSaveTrap()
            ArmReadMode()
        Catch ex As Exception
            Tr("OnDeadCheckTick EXCEPTION: " & ex.ToString())
            GlobalErrorLog.Write("AcroPdfSurface.OnDeadCheckTick", ex)
        End Try
    End Sub

    ' Trap event, UI thread. Boundary: log and swallow.
    Private Sub OnScriptBurstEnded()
        Try
            If _readModePending Then TrySendReadMode("script alerts over")
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfSurface.OnScriptBurstEnded", ex)
        End Try
    End Sub

    Private Sub OnFormActivated(sender As Object, e As EventArgs)
        Try
            If _readModePending Then TrySendReadMode("form activated")
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfSurface.OnFormActivated", ex)
        End Try
    End Sub

    ' Sends Ctrl+H once, when every condition holds. Never throws.
    Private Sub TrySendReadMode(trigger As String)
        If Not _readModePending OrElse _inReadModeCheck Then Return
        _inReadModeCheck = True
        Try
            Dim page As IntPtr
            Dim wait As String = ReadModeBlocker(page)
            If wait IsNot Nothing Then
                If wait <> _readModeWait Then
                    _readModeWait = wait
                    Tr($"ReadMode: waiting -- {wait} (trigger: {trigger})")
                End If
                Return
            End If

            Dim before As IntPtr = AdobeNativeMethods.SetFocus(page)
            Dim focus As IntPtr = AdobeNativeMethods.GetFocus()
            Dim focusInside As Boolean = focus = page OrElse AdobeNativeMethods.IsChild(_host.Handle, focus)
            Tr($"ReadMode: conditions met (trigger: {trigger}); page={AcroPdfTraceLog.Hex(page)} " &
               $"{AdobeNativeMethods.RectInParent(page)}; SetFocus returned {AcroPdfTraceLog.Hex(before)}, " &
               $"focus now {AcroPdfTraceLog.Hex(focus)} «{AdobeNativeMethods.GetTitle(focus)}» inside={focusInside}")
            If Tracing Then TraceTree("ReadMode: tree before Ctrl+H")
            DisarmReadMode()

            If Not focusInside Then
                Report("AcroPDF: ATENȚIE — documentul nu a primit focusul tastaturii; Ctrl+H NU a fost trimis " &
                       "(ar fi ajuns în K-BOT). Apăsați Ctrl+H în document pentru a ascunde barele.")
                EndRecording("BLOCKING: keyboard focus did not reach the control, Ctrl+H not sent")
                Return
            End If

            Dim keys As AdobeNativeMethods.INPUT() = {
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, False),
                AdobeNativeMethods.KeyInput(VK_H, False),
                AdobeNativeMethods.KeyInput(VK_H, True),
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, True)}
            Dim sent As UInteger = AdobeNativeMethods.SendInput(CUInt(keys.Length), keys,
                Runtime.InteropServices.Marshal.SizeOf(GetType(AdobeNativeMethods.INPUT)))
            Dim err As Integer = If(sent = keys.Length, 0, Runtime.InteropServices.Marshal.GetLastWin32Error())
            Tr($"ReadMode: SendInput Ctrl+H -> {sent}/{keys.Length} event(s)" & If(err <> 0, $", Win32 error {err}", ""))
            If sent = keys.Length Then
                Report("AcroPDF: mod citire — Ctrl+H trimis documentului.")
                EndRecording("document open, Ctrl+H sent")
            Else
                Report($"AcroPDF: ATENȚIE — Ctrl+H nu a putut fi trimis ({sent}/{keys.Length} taste, eroarea {err}).")
                EndRecording($"BLOCKING: SendInput sent {sent} of {keys.Length} key events, error {err}")
            End If
        Catch ex As Exception
            Tr("TrySendReadMode EXCEPTION: " & ex.ToString())
            GlobalErrorLog.Write("AcroPdfSurface.TrySendReadMode", ex)
        Finally
            _inReadModeCheck = False
        End Try
    End Sub

    ' Nothing when Ctrl+H may be sent now; otherwise why not (for the trace). page = the laid-out
    ' page view when found.
    Private Function ReadModeBlocker(ByRef page As IntPtr) As String
        page = IntPtr.Zero
        If _host Is Nothing OrElse Not _host.IsHandleCreated Then Return "no control"
        If Not ViewerShown() Then Return "viewer not on screen"
        For Each h As IntPtr In AdobeNativeMethods.Descendants(_host.Handle)
            If Not String.Equals(AdobeNativeMethods.GetTitle(h), PageViewTitle, StringComparison.Ordinal) Then Continue For
            If Not AdobeNativeMethods.IsWindowVisible(h) Then Continue For
            Dim r As Rectangle = AdobeNativeMethods.RectInParent(h)
            If r.Width > 0 AndAlso r.Height > 0 Then
                page = h
                Exit For
            End If
        Next
        If page = IntPtr.Zero Then Return "page view not laid out yet"
        If _saveTrap.InScriptBurst Then Return $"script alerts within the last {AdobeSaveTrap.ScriptBurstQuietMs} ms"
        If _form Is Nothing OrElse Not _form.IsHandleCreated Then Return "no form"
        If Not AdobeNativeMethods.IsWindowEnabled(_form.Handle) Then Return "form disabled (a modal alert is up)"
        Dim fg As IntPtr = AdobeNativeMethods.GetForegroundWindow()
        If fg <> _form.Handle Then Return $"K-BOT is not the foreground window (foreground {AcroPdfTraceLog.Hex(fg)} «{AdobeNativeMethods.GetTitle(fg)}»)"
        Return Nothing
    End Function

    Private Shared Function EventName(eventType As UInteger) As String
        Select Case eventType
            Case AdobeNativeMethods.EVENT_OBJECT_SHOW : Return "SHOW"
            Case AdobeNativeMethods.EVENT_OBJECT_STATECHANGE : Return "STATECHANGE"
            Case AdobeNativeMethods.EVENT_OBJECT_LOCATIONCHANGE : Return "LOCATIONCHANGE"
            Case Else : Return "0x" & eventType.ToString("X")
        End Select
    End Function

    ' ── Helpers ─────────────────────────────────────────────────────────────────
    ' For the log: how many windows the control holds, and how many are Adobe's own views.
    Private Function DescribeTree() As String
        If _host Is Nothing OrElse Not _host.IsHandleCreated Then Return "fără control"
        Dim all As List(Of IntPtr) = AdobeNativeMethods.Descendants(_host.Handle)
        Dim views As Integer = all.Where(Function(h) AdobeNativeMethods.GetClass(h).StartsWith(AdobeViewClassPrefix, StringComparison.Ordinal)).Count()
        Return $"{all.Count} ferestre, {views} vederi Adobe"
    End Function

    Private Function Walk() As List(Of AdobeWindowNode)
        If _host Is Nothing OrElse Not _host.IsHandleCreated Then Return New List(Of AdobeWindowNode)()
        Return AdobeWindowProbe.Walk(_host.Handle, _panel.Handle, ProbeDepth)
    End Function

    Private Shared Function FindNode(nodes As List(Of AdobeWindowNode), text As String) As AdobeWindowNode
        Return nodes.FirstOrDefault(Function(n) String.Equals(n.Text, text, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Sub Report(line As String)
        _log?.Invoke(line)
        Tr("REPORT: " & line)
    End Sub

    ' ── Trace (AcroPdfTraceLog, switched on in the settings window) ────────────────────────
    ' Everything below writes nothing while the trace is off; the costly ones are guarded by
    ' Tracing at the call site so the tree is not even walked.

    Private Shared ReadOnly Property Tracing As Boolean
        Get
            Return AcroPdfTraceLog.Enabled
        End Get
    End Property

    Private Shared Sub Tr(line As String)
        AcroPdfTraceLog.Write("AcroPdfSurface", line)
    End Sub

    Private Shared Sub EndRecording(reason As String)
        AcroPdfTraceLog.EndSession("AcroPdfSurface", reason)
    End Sub

    ' Last traced OwnerPids answer (see OwnerPids).
    Private _lastOwnerPidsTrace As String

    ' The inputs of ViewerShown in one line.
    Private Function ScreenText() As String
        Return $"panel.Visible={_panel.Visible} form={If(_form Is Nothing, "(none)", _form.Name & "/" & _form.WindowState.ToString())} " &
               $"shown={ViewerShown()} trapPaused={_saveTrap.IsPaused} trapRunning={_saveTrap.IsRunning} " &
               $"refitTimer={_refitTimer.Enabled} headerTicksLeft={_headerTicksLeft}"
    End Function

    ' Loads through the control and traces what it answered and how long it took.
    Private Sub TracedLoad(host As AcroPdfHost, pdfPath As String, clock As Diagnostics.Stopwatch)
        Tr($"LoadFile «{pdfPath}» on {AcroPdfTraceLog.Hex(host.Handle)} at {clock.ElapsedMilliseconds} ms")
        Dim before As Long = clock.ElapsedMilliseconds
        Dim answer As Boolean = host.LoadFile(pdfPath)
        Tr($"LoadFile returned {answer} in {clock.ElapsedMilliseconds - before} ms; {DescribeTree()}")
    End Sub

    Private Shared Function NodeText(n As AdobeWindowNode) As String
        If n Is Nothing Then Return "(none)"
        Return $"{AcroPdfTraceLog.Hex(n.Hwnd)} «{n.Text}» {n.ClassName} {n.Bounds} visible={n.Visible} depth={n.Depth}"
    End Function

    Private Shared Sub TraceNodes(header As String, nodes As List(Of AdobeWindowNode))
        AcroPdfTraceLog.WriteBlock("AcroPdfSurface", $"{header} ({nodes.Count} node(s)):",
            nodes.Select(Function(n) New String(" "c, n.Depth * 2) & NodeText(n) &
                                     $" pid={AdobeNativeMethods.OwnerPid(n.Hwnd)}"))
    End Sub

    ' The WHOLE tree under the control (EnumChildWindows, no depth limit), with owner processes.
    Private Sub TraceTree(header As String)
        If _host Is Nothing OrElse Not _host.IsHandleCreated Then
            Tr(header & ": no control")
            Return
        End If
        AcroPdfTraceLog.WriteBlock("AcroPdfSurface", $"{header} under {AcroPdfTraceLog.Hex(_host.Handle)}:",
            AdobeNativeMethods.Descendants(_host.Handle).Select(
                Function(h) $"{AcroPdfTraceLog.Hex(h)} parent={AcroPdfTraceLog.Hex(AdobeNativeMethods.GetParent(h))} " &
                            $"class={AdobeNativeMethods.GetClass(h)} title=«{AdobeNativeMethods.GetTitle(h)}» " &
                            $"rect={AdobeNativeMethods.RectInParent(h)} visibleStyle={AdobeNativeMethods.IsVisibleStyleSet(h)} " &
                            $"pid={AdobeNativeMethods.OwnerPid(h)}"))
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Tr("Dispose")
        Clear()
        Try
            RemoveHandler _panel.SizeChanged, AddressOf OnPanelSizeChanged
            RemoveHandler _panel.VisibleChanged, AddressOf OnScreenStateChanged
            RemoveHandler _panel.ParentChanged, AddressOf OnScreenStateChanged
            If _form IsNot Nothing Then
                RemoveHandler _form.Resize, AddressOf OnScreenStateChanged
                RemoveHandler _form.Activated, AddressOf OnFormActivated
            End If
            _form = Nothing
            RemoveHandler _refitTimer.Tick, AddressOf OnRefitTick
            _refitTimer.Dispose()
            RemoveHandler _deadCheckTimer.Tick, AddressOf OnDeadCheckTick
            _deadCheckTimer.Dispose()
            RemoveHandler _saveTrap.Saved, AddressOf OnTrapSaved
            RemoveHandler _saveTrap.Failed, AddressOf OnTrapFailed
            RemoveHandler _saveTrap.ScriptBurstEnded, AddressOf OnScriptBurstEnded
            _saveTrap.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfSurface.Dispose", ex)
        End Try
    End Sub

End Class
