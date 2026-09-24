Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Takes Adobe's «Save As» away from the operator (slice 0078-02).
'''
''' WHY: signing a field in Adobe forces a save, and Adobe asks WHERE with the standard file
''' dialog. The signed PDF must overwrite exactly the file K-BOT opened -- it is the file K-BOT
''' reads back, hashes and uploads. Letting the operator pick a folder would put the signature in a
''' file nobody looks at. So the dialog is moved off screen the moment it appears, the target path
''' is written into its file-name box, READ BACK, and Save is pressed. The «replace?» prompt that
''' follows (owned by that dialog, and only that one) is answered «Yes».
'''
''' ANY DOUBT CANCELS. If the box is not found, or the text read back is not exactly the target,
''' the dialog is cancelled (Adobe then does not apply the signature) and <see cref="Failed"/> is
''' raised so the caller can tell the operator. The dialog is NEVER left for the operator to use.
'''
''' TWO TRIGGERS. A per-process out-of-context WinEvent hook (EVENT_OBJECT_SHOW) reacts as soon as
''' Windows reports the dialog, and a timer sweep catches anything the hook missed and drives the
''' after-Save state (confirm prompt, dialog gone). Both run on the UI thread -- the hook because it
''' is out-of-context, the timer because it is a WinForms timer.
'''
''' CLICKS ARE POSTED, NEVER SENT. Pressing Save makes the dialog show the modal «replace?» prompt;
''' a SendMessage would not return until that prompt closed, and we are the ones who must close it.
'''
''' ONE DIALOG, ONE OWNER (measured 23.09.2026 in the app, not on the bench). The DDF and the ORD
''' view each keep a viewer, so two traps run at once, and every AcroPDF control of the process is
''' served by the SAME Acrobat broker: both traps saw the same Save As and took turns writing their
''' own path into it. The signed DDF landed in the ORD's file, was uploaded as that ORD, and the DDF
''' itself was never saved. So a Save As is handed to exactly one trap (<see cref="Claims"/>): the one
''' whose document Adobe offers as the file name; failing that, the one whose viewer is on screen;
''' failing that, nobody -- the dialog is cancelled.
'''
''' SCRIPT NOISE. The DDF / ORD forms raise script errors on open that change nothing in the file
''' (the operator: «even with them, the file works normally»). Adobe shows them as «Warning:
''' JavaScript Window» boxes and opens its «JavaScript Debugger» console. The operator's order
''' (23.09.2026): press OK with a MESSAGE, never keystrokes; what has no OK (the console) is
''' hidden. The text is logged. See <see cref="DismissScriptNoise"/>.
''' </summary>
Public NotInheritable Class AdobeSaveTrap
    Implements IDisposable

    Public Const SweepIntervalMs As Integer = 200

    ' ── SCRIPT-ERROR BURST — the values to change for a manual test (operator, 23.09.2026) ──
    ''' <summary>
    ''' Sweep interval from the moment a script error box is seen: the boxes come in a burst (5-8 in
    ''' a row on opening a DDF), so the sweep speeds up to catch each one as it shows.
    ''' </summary>
    Public Const ScriptBurstIntervalMs As Integer = 100
    ''' <summary>After this long with no new script box the sweep goes back to <see cref="SweepIntervalMs"/>.</summary>
    Public Const ScriptBurstQuietMs As Integer = 3000
    ''' <summary>How long after our Save click the dialog may take to disappear.</summary>
    Public Const SaveTimeoutMs As Integer = 20000
    ''' <summary>
    ''' A pressed Save As that stays HIDDEN this long (without closing) counts as done. MEASURED:
    ''' the dialog hides for a moment while it handles the click and may show itself again (then
    ''' Save is pressed again) -- treating the first blink as the end raised «saved» before Adobe
    ''' had written anything.
    ''' </summary>
    Public Const HiddenDoneMs As Integer = 1000
    ''' <summary>How many times one dialog may be pressed before it is cancelled.</summary>
    Public Const MaxPresses As Integer = 3

    ''' <summary>Title prefix of the alert boxes raised by the form's own scripts.</summary>
    Public Const ScriptAlertTitle As String = "Warning: JavaScript Window"
    ''' <summary>Title of Adobe's script console, which opens itself on a script error.</summary>
    Public Const ScriptConsoleTitle As String = "JavaScript Debugger"

    ' Every running trap of the process, and which trap a Save As was handed to. All traps live on
    ' the UI thread (WinForms timer + out-of-context hook), so no lock is needed.
    Private Shared ReadOnly Running As New List(Of AdobeSaveTrap)()
    Private Shared ReadOnly Claims As New Dictionary(Of IntPtr, AdobeSaveTrap)()
    ' Script windows already reported as hidden, and the OK clicks per alert -- process-wide, so two
    ' traps never both press one box.
    Private Shared ReadOnly Dismissed As New HashSet(Of IntPtr)()
    Private Shared ReadOnly ScriptClicks As New Dictionary(Of IntPtr, DateTime)()
    Private Shared ReadOnly ScriptAttempts As New Dictionary(Of IntPtr, Integer)()
    ' When a script ERROR box was last seen, process-wide: a console showing near it opened itself.
    Private Shared _lastScriptAlert As DateTime = DateTime.MinValue
    ''' <summary>
    ''' A script console that shows up within this long of a K-BOT load opened itself on the form's
    ''' errors and is hidden. Outside that window (and with no error box in the last
    ''' <see cref="ScriptBurstQuietMs"/>) the console was opened by the operator and is LEFT ALONE --
    ''' measured 23.09.2026: an Acrobat opened by hand joins K-BOT's Acrobat process, and its
    ''' console was hidden too.
    ''' </summary>
    Public Const ConsoleAfterLoadMs As Integer = 10000
    ''' <summary>A clicked alert still showing after this long is clicked again.</summary>
    Public Const ScriptClickRetryMs As Integer = 700
    ''' <summary>OK clicks on one alert before it is hidden instead.</summary>
    Public Const MaxScriptClicks As Integer = 3

    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _timer As New Timer()
    ' Held for the lifetime of the hooks -- a collected delegate crashes inside user32.
    Private ReadOnly _callback As AdobeNativeMethods.WinEventProc
    Private ReadOnly _hooks As New List(Of IntPtr)()
    ' «Other» dialogs are logged once per handle.
    Private ReadOnly _seen As New HashSet(Of IntPtr)()

    Private _targetPath As String
    Private _pids As New List(Of Integer)()
    ' The Save As we pressed and are waiting on (IntPtr.Zero when idle).
    Private _pending As IntPtr = IntPtr.Zero
    Private _pendingSince As DateTime
    Private _confirmed As Boolean
    ' When the pressed dialog was last seen hidden (Nothing = visible), and how often it was pressed.
    Private _hiddenSince As DateTime?
    Private _presses As Integer
    Private _paused As Boolean
    ' When this trap was started, i.e. when its document was loaded.
    Private _startedAt As DateTime = DateTime.MinValue
    ' End of the current script-error burst (the sweep runs at ScriptBurstIntervalMs until then).
    Private _burstUntil As DateTime = DateTime.MinValue
    Private _lastDialog As IntPtr = IntPtr.Zero
    ' Confirm prompts already answered (answered once, not on every sweep).
    Private ReadOnly _answered As New HashSet(Of IntPtr)()

    ''' <summary>Raised on the UI thread after a trapped Save As closed. Argument = the target path.</summary>
    Public Event Saved As Action(Of String)
    ''' <summary>Raised on the UI thread when a Save As had to be cancelled. Argument = Romanian reason.</summary>
    Public Event Failed As Action(Of String)
    ''' <summary>
    ''' Raised on the UI thread when a burst of script alerts is over (<see cref="ScriptBurstQuietMs"/>
    ''' without a new one). The ActiveX viewer's Read Mode waits for it: Ctrl+H sent while Adobe is
    ''' still recovering from the alerts did not always hold (operator, 24.09.2026).
    ''' </summary>
    Public Event ScriptBurstEnded As Action

    ' Trace identity: two traps (DDF + ORD) run at once, and their lines must be told apart.
    Private Shared _nextId As Integer
    Private ReadOnly _id As Integer

    Public Sub New(log As Action(Of String))
        _log = log
        _callback = AddressOf OnWinEvent
        _timer.Interval = SweepIntervalMs
        AddHandler _timer.Tick, AddressOf OnTick
        _nextId += 1
        _id = _nextId
    End Sub

    ''' <summary>
    ''' When True, every step of this trap is also written to <see cref="AcroPdfTraceLog"/> (while
    ''' the operator has that trace switched on). Set by <see cref="AcroPdfSurface"/> only -- the
    ''' hosted-window engine's trap stays out of the ActiveX trace.
    ''' </summary>
    Public Property Traced As Boolean

    ' True when this trap writes trace lines right now. Checked before building a costly line.
    Private ReadOnly Property Tracing As Boolean
        Get
            Return Traced AndAlso AcroPdfTraceLog.Enabled
        End Get
    End Property

    ' Last traced tick state / sweep list / dialogs: repeats are not written again.
    Private _lastTickTrace As String
    Private _lastSweepTrace As String
    Private ReadOnly _tracedDialogs As New HashSet(Of String)()

    Private Sub Tr(line As String)
        If Tracing Then AcroPdfTraceLog.Write($"SaveTrap#{_id}", line)
    End Sub

    ' The state of this trap in one line, for the trace.
    Private Function StateText() As String
        Return $"target={If(_targetPath, "(none)")} paused={_paused} timer={_timer.Enabled}/{_timer.Interval}ms " &
               $"pids=[{String.Join(",", _pids)}] hooks={_hooks.Count} pending={AcroPdfTraceLog.Hex(_pending)} " &
               $"presses={_presses} confirmed={_confirmed} hiddenSince={If(_hiddenSince.HasValue, _hiddenSince.Value.ToString("HH:mm:ss.fff"), "-")} " &
               $"running={Running.Count} claims={Claims.Count}"
    End Function

    ''' <summary>
    ''' Optional: re-asked on every sweep for the processes whose dialogs are trapped. Used by the
    ''' ActiveX engine, where nobody knows Adobe's process up front: AcroPDF draws the document with
    ''' windows owned by whichever Adobe process serves it, and that process can appear (or change)
    ''' after the load. New pids get their own hook; pids are never dropped while running.
    ''' </summary>
    Public Property PidSource As Func(Of IEnumerable(Of Integer))

    ''' <summary>
    ''' Optional: True when this trap's viewer is on screen. Decides who owns a Save As when the
    ''' file name Adobe offers matches no running trap (or several).
    ''' </summary>
    Public Property IsOnScreen As Func(Of Boolean)

    ''' <summary>The path this trap forces (Nothing when stopped).</summary>
    Public ReadOnly Property TargetPath As String
        Get
            Return _targetPath
        End Get
    End Property

    ''' <summary>True while a Save we pressed has not finished (the file may be half written).</summary>
    Public ReadOnly Property IsBusy As Boolean
        Get
            Return _pending <> IntPtr.Zero
        End Get
    End Property

    ''' <summary>Started and not paused: this trap is watching right now.</summary>
    Public ReadOnly Property IsRunning As Boolean
        Get
            Return _targetPath IsNot Nothing AndAlso Not _paused
        End Get
    End Property

    ''' <summary>True between <see cref="Pause"/> and <see cref="[Resume]"/>.</summary>
    Public ReadOnly Property IsPaused As Boolean
        Get
            Return _paused
        End Get
    End Property

    ''' <summary>
    ''' Stops the timer and the hooks while the viewer is off screen (operator, 23.09.2026: no timer
    ''' runs for a viewer nobody sees). Target and processes are kept for <see cref="[Resume]"/>.
    ''' A Save already pressed is followed to its end first -- the timer stops when it is done.
    ''' A paused trap is never chosen as the owner of a Save As.
    ''' </summary>
    Public Sub Pause()
        Try
            Tr("Pause() called; " & StateText())
            If _paused Then Return
            _paused = True
            If _targetPath Is Nothing Then Return
            If IsBusy Then
                Report("Capcana «Salvare ca»: vizualizatorul a ieșit de pe ecran — mă opresc după salvarea în curs.")
                Return
            End If
            HaltWatching()
            Report("Capcana «Salvare ca» suspendată (vizualizatorul nu e pe ecran).")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSaveTrap.Pause", ex)
        End Try
    End Sub

    ''' <summary>Restarts the timer and the hooks when the viewer is back on screen.</summary>
    Public Sub [Resume]()
        Try
            Tr("Resume() called; " & StateText())
            If Not _paused Then Return
            _paused = False
            If _targetPath Is Nothing Then Return
            If Not _timer.Enabled Then
                For Each pid As Integer In _pids
                    Hook(pid)
                Next
                _timer.Start()
                Report("Capcana «Salvare ca» repornită (vizualizatorul e din nou pe ecran).")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSaveTrap.Resume", ex)
        End Try
    End Sub

    ' Timer off, hooks off; target, pids and claims untouched.
    Private Sub HaltWatching()
        _timer.Stop()
        For Each h As IntPtr In _hooks
            Dim ok As Boolean = AdobeNativeMethods.UnhookWinEvent(h)
            Tr($"HaltWatching: UnhookWinEvent({AcroPdfTraceLog.Hex(h)}) -> {ok}")
        Next
        _hooks.Clear()
        Tr("HaltWatching done; " & StateText())
    End Sub

    ''' <summary>
    ''' Starts trapping the file dialogs of <paramref name="adobePids"/>, forcing
    ''' <paramref name="targetPath"/>. Call from the UI thread (the hooks need its message pump).
    ''' </summary>
    Public Sub Start(targetPath As String, adobePids As IEnumerable(Of Integer))
        Try
            Tr($"Start(target={targetPath}, pids=[{If(adobePids Is Nothing, "", String.Join(",", adobePids))}]) " &
               $"pidSource={PidSource IsNot Nothing} isOnScreen={IsOnScreen IsNot Nothing}")
            [Stop]()
            If String.IsNullOrWhiteSpace(targetPath) Then
                Tr("Start: empty target -> not started")
                Return
            End If
            _targetPath = targetPath
            _startedAt = DateTime.UtcNow
            _pids = If(adobePids Is Nothing, New List(Of Integer)(), New List(Of Integer)(adobePids))
            _pids.RemoveAll(Function(p) p <= 0)
            If _pids.Count = 0 AndAlso PidSource Is Nothing Then
                Report("Capcana «Salvare ca» NU a pornit: niciun proces Adobe identificat.")
                Return
            End If
            For Each pid As Integer In _pids
                Hook(pid)
            Next
            If Not Running.Contains(Me) Then Running.Add(Me)
            If _paused Then
                ' Loaded while off screen: armed, but nothing runs until Resume.
                HaltWatching()
                Report($"Capcana «Salvare ca» pregătită, suspendată (vizualizatorul nu e pe ecran). Țintă: {_targetPath}")
                Return
            End If
            _timer.Start()
            Tr("Start done; " & StateText())
            Report($"Capcana «Salvare ca» pornită pentru {String.Join(", ", _pids)} " &
                   $"({_hooks.Count} cârlige, verificare la {SweepIntervalMs} ms). Țintă: {_targetPath}")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSaveTrap.Start", ex)
            Throw
        End Try
    End Sub

    ' Reached from Start / RefreshPids, both wrapped -- transitive coverage.
    Private Sub Hook(pid As Integer)
        Dim h As IntPtr = AdobeNativeMethods.SetWinEventHook(
            AdobeNativeMethods.EVENT_OBJECT_SHOW, AdobeNativeMethods.EVENT_OBJECT_SHOW,
            IntPtr.Zero, _callback, CUInt(pid), 0UI, AdobeNativeMethods.WINEVENT_OUTOFCONTEXT)
        ' Read before anything else can overwrite it.
        Dim lastError As Integer = Runtime.InteropServices.Marshal.GetLastWin32Error()
        If h <> IntPtr.Zero Then _hooks.Add(h)
        If Tracing Then
            Tr($"Hook(pid={pid} «{ProcessName(pid)}») SetWinEventHook -> {AcroPdfTraceLog.Hex(h)}" &
               If(h = IntPtr.Zero, $" FAILED (Win32 error {lastError})", ""))
        End If
    End Sub

    ' Adds the pids PidSource reports now and did not report before. Reached from Sweep (wrapped).
    Private Sub RefreshPids()
        If PidSource Is Nothing OrElse _paused Then Return
        Dim current As IEnumerable(Of Integer) = PidSource.Invoke()
        If current Is Nothing Then Return
        For Each pid As Integer In current
            If pid <= 0 OrElse _pids.Contains(pid) Then Continue For
            _pids.Add(pid)
            Hook(pid)
            Report($"Capcana «Salvare ca»: proces Adobe nou urmărit {pid} (proces «{ProcessName(pid)}»).")
        Next
    End Sub

    Private Shared Function IsAdobeProcess(pid As Integer) As Boolean
        If pid <= 0 Then Return False
        Return ProcessName(pid).StartsWith("Acro", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function ProcessName(pid As Integer) As String
        Try
            Using p As Diagnostics.Process = Diagnostics.Process.GetProcessById(pid)
                Return p.ProcessName
            End Using
        Catch ex As Exception
            ' A process that ended between the two calls is not an error worth more than a log.
            GlobalErrorLog.Write("AdobeSaveTrap.ProcessName", ex)
            Return "?"
        End Try
    End Function

    ''' <summary>Stops trapping. Safe to call twice and when never started.</summary>
    Public Sub [Stop]()
        Try
            Tr("Stop() called; " & StateText())
            _timer.Stop()
            For Each h As IntPtr In _hooks
                AdobeNativeMethods.UnhookWinEvent(h)
            Next
            If _hooks.Count > 0 OrElse _targetPath IsNot Nothing Then Report("Capcana «Salvare ca» oprită.")
            _hooks.Clear()
            _seen.Clear()
            _answered.Clear()
            _pending = IntPtr.Zero
            _hiddenSince = Nothing
            _presses = 0
            _targetPath = Nothing
            _pids.Clear()
            Running.Remove(Me)
            For Each h As IntPtr In Claims.Where(Function(kv) kv.Value Is Me).Select(Function(kv) kv.Key).ToList()
                Claims.Remove(h)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSaveTrap.Stop", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Keeps the UI pumping until a pressed Save has finished, or <paramref name="timeoutMs"/>
    ''' passes. Used before Adobe is torn down, so a kill never cuts a signed file in half.
    ''' </summary>
    Public Sub WaitWhileBusy(timeoutMs As Integer)
        Try
            Tr($"WaitWhileBusy({timeoutMs} ms) begin; " & StateText())
            Dim limit As DateTime = DateTime.UtcNow.AddMilliseconds(timeoutMs)
            Dim rounds As Integer = 0
            While IsBusy AndAlso DateTime.UtcNow < limit
                rounds += 1
                Sweep()
                Application.DoEvents()
                Threading.Thread.Sleep(50)
            End While
            Tr($"WaitWhileBusy end after {rounds} rounds, busy={IsBusy}")
            If IsBusy Then Report("ATENȚIE: salvarea Adobe nu s-a încheiat la timp; închid oricum.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSaveTrap.WaitWhileBusy", ex)
        End Try
    End Sub

    ' Callback boundary (user32 across a process boundary): it must never throw.
    Private Sub OnWinEvent(hook As IntPtr, eventType As UInteger, hwnd As IntPtr,
                           idObject As Integer, idChild As Integer,
                           threadId As UInteger, timestamp As UInteger)
        Try
            If Tracing Then
                Tr($"OnWinEvent hook={AcroPdfTraceLog.Hex(hook)} event=0x{eventType:X} hwnd={AcroPdfTraceLog.Hex(hwnd)} " &
                   $"obj={idObject} child={idChild} thread={threadId} class={AdobeNativeMethods.GetClass(hwnd)} " &
                   $"title=«{AdobeNativeMethods.GetTitle(hwnd)}» pid={AdobeNativeMethods.OwnerPid(hwnd)} paused={_paused}")
            End If
            If hwnd = IntPtr.Zero OrElse _paused Then Return
            If idObject <> AdobeNativeMethods.OBJID_WINDOW OrElse idChild <> 0 Then Return
            ' Top-level only. Tested on WS_CHILD, NOT GetParent: for an owned popup (every modal
            ' dialog) GetParent returns the OWNER, so a GetParent test would skip the very dialog.
            If (AdobeNativeMethods.GetWindowLongPtrSafe(hwnd, AdobeNativeMethods.GWL_STYLE).ToInt64() And
                AdobeNativeMethods.WS_CHILD) <> 0 Then
                Tr($"OnWinEvent {AcroPdfTraceLog.Hex(hwnd)}: WS_CHILD -> ignored")
                Return
            End If
            If Not String.Equals(AdobeNativeMethods.GetClass(hwnd), AdobeSaveDialogFilter.DialogClass,
                                 StringComparison.Ordinal) Then
                Tr($"OnWinEvent {AcroPdfTraceLog.Hex(hwnd)}: class is not {AdobeSaveDialogFilter.DialogClass} -> ignored")
                Return
            End If
            Tr($"OnWinEvent {AcroPdfTraceLog.Hex(hwnd)}: dialog class -> Handle")
            Handle(hwnd)
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSaveTrap.OnWinEvent", ex)
        End Try
    End Sub

    ' Timer tick = UI boundary: log and swallow.
    Private Sub OnTick(sender As Object, e As EventArgs)
        Try
            ' Only when the state changed since the last traced tick.
            If Tracing Then
                Dim state As String = StateText()
                If state <> _lastTickTrace Then
                    _lastTickTrace = state
                    Tr("OnTick, state changed; " & state)
                End If
            End If
            If _timer.Interval <> SweepIntervalMs AndAlso DateTime.UtcNow > _burstUntil Then
                _timer.Interval = SweepIntervalMs
                Report($"Capcana: fără mesaje de script de {ScriptBurstQuietMs} ms — verificare din nou la {SweepIntervalMs} ms.")
                RaiseEvent ScriptBurstEnded()
            End If
            Sweep()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSaveTrap.OnTick", ex)
        End Try
    End Sub

    ''' <summary>One pass over the top-level dialogs, plus the after-Save bookkeeping.</summary>
    Public Sub Sweep()
        Try
            If _targetPath Is Nothing Then
                Tr("Sweep: no target -> skipped")
                Return
            End If
            RefreshPids()
            If _pids.Count = 0 Then
                Tr("Sweep: no Adobe pid known yet -> skipped")
                Return
            End If
            Dim dialogs As New List(Of IntPtr)()
            AdobeNativeMethods.EnumWindows(
                Function(h, l)
                    If String.Equals(AdobeNativeMethods.GetClass(h), AdobeSaveDialogFilter.DialogClass,
                                     StringComparison.Ordinal) Then dialogs.Add(h)
                    Return True
                End Function, IntPtr.Zero)
            If Tracing Then
                ' Only the dialogs of the processes we watch, and only when that list CHANGED:
                ' the 24.09.2026 trace listed every #32770 of the machine every 200 ms.
                Dim ours As List(Of String) = dialogs.
                    Where(Function(h) _pids.Contains(AdobeNativeMethods.OwnerPid(h))).
                    Select(Function(h) $"{AcroPdfTraceLog.Hex(h)} pid={AdobeNativeMethods.OwnerPid(h)} " &
                                       $"visible={AdobeNativeMethods.IsWindowVisible(h)} " &
                                       $"rect={AdobeNativeMethods.RectOnScreen(h)} " &
                                       $"owner={AcroPdfTraceLog.Hex(AdobeNativeMethods.GetWindow(h, AdobeNativeMethods.GW_OWNER))} " &
                                       $"title=«{AdobeNativeMethods.GetTitle(h)}»").ToList()
                Dim signature As String = String.Join("|", ours)
                If signature <> _lastSweepTrace Then
                    _lastSweepTrace = signature
                    AcroPdfTraceLog.WriteBlock($"SaveTrap#{_id}",
                        $"Sweep: dialogs of watched processes changed -- {ours.Count} now ({dialogs.Count - ours.Count} of other processes not listed)",
                        ours)
                End If
            End If
            PruneShared()
            For Each h As IntPtr In dialogs
                Handle(h)
            Next
            CheckPending()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSaveTrap.Sweep", ex)
            Throw
        End Try
    End Sub

    ' Reached from OnWinEvent / Sweep, both wrapped -- transitive coverage.
    Private Sub Handle(hwnd As IntPtr)
        If _targetPath Is Nothing OrElse Not AdobeNativeMethods.IsWindow(hwnd) Then
            Tr($"Handle {AcroPdfTraceLog.Hex(hwnd)}: no target or window gone -> skipped")
            Return
        End If
        If hwnd = _pending Then
            Tr($"Handle {AcroPdfTraceLog.Hex(hwnd)}: this is the pending Save As -> left to CheckPending")
            Return
        End If

        ' Handed to another running trap: its business, not ours.
        Dim owner As AdobeSaveTrap = Nothing
        If Claims.TryGetValue(hwnd, owner) AndAlso owner IsNot Me AndAlso (owner.IsRunning OrElse owner.IsBusy) Then
            Tr($"Handle {AcroPdfTraceLog.Hex(hwnd)}: claimed by SaveTrap#{owner._id} ({owner._targetPath}) -> skipped")
            Return
        End If

        Dim facts As AdobeDialogFacts = GatherFacts(hwnd)
        Dim kind As AdobeDialogKind = AdobeSaveDialogFilter.Classify(facts, _pids, _pending)
        ' Dialogs of processes we do not watch are not traced (the Report below covers the Adobe
        ' ones once); ours are traced once per handle and kind.
        If kind <> AdobeDialogKind.NotOurs AndAlso _tracedDialogs.Add($"{hwnd.ToInt64()}:{kind}") Then
            Tr($"Handle {AcroPdfTraceLog.Hex(hwnd)}: kind={kind} claimedByMe={owner Is Me} facts: {facts.Describe()} " &
               $"owner={AcroPdfTraceLog.Hex(facts.OwnerWindow)}")
        End If
        Select Case kind
            Case AdobeDialogKind.SaveAs
                If owner Is Me Then
                    HandleSaveAs(hwnd, facts)
                Else
                    Arbitrate(hwnd, facts)
                End If
            Case AdobeDialogKind.ConfirmOverwrite
                HandleConfirm(hwnd, facts)
            Case AdobeDialogKind.Other
                If AdobeSaveDialogFilter.IsScriptNoise(facts.Title, ScriptAlertTitle, ScriptConsoleTitle) Then
                    DismissScriptNoise(hwnd, facts)
                ElseIf _seen.Add(hwnd) Then
                    Report($"Dialog Adobe {AdobeSaveDialogFilter.Label(kind)}: 0x{hwnd.ToInt64():X} {facts.Describe()}")
                End If
            Case AdobeDialogKind.NotOurs
                ' An Adobe dialog from a process we do NOT watch is logged once: that is exactly
                ' how the broker/renderer split (23.09.2026) stayed invisible on the first run.
                If String.Equals(facts.ClassName, AdobeSaveDialogFilter.DialogClass, StringComparison.Ordinal) AndAlso
                   _seen.Add(hwnd) AndAlso IsAdobeProcess(facts.OwnerPid) Then
                    Report($"Dialog al unui proces Adobe NEURMĂRIT (lăsat în pace): 0x{hwnd.ToInt64():X} {facts.Describe()}")
                End If
        End Select
    End Sub

    ' First sight of a Save As: decide which running trap it belongs to, BEFORE anything is written
    ' into it -- the name Adobe offers is the only trace of the document it is saving.
    Private Sub Arbitrate(hwnd As IntPtr, facts As AdobeDialogFacts)
        MoveOffScreen(hwnd)
        Dim offered As String = AdobeNativeMethods.ReadText(FindFileNameEdit(hwnd))
        Dim how As String = Nothing
        If Tracing Then
            AcroPdfTraceLog.WriteBlock($"SaveTrap#{_id}", $"Arbitrate {AcroPdfTraceLog.Hex(hwnd)}: offered name «{offered}», dialog pid {facts.OwnerPid}; running traps:",
                Running.Select(Function(t) $"SaveTrap#{t._id} running={t.IsRunning} busy={t.IsBusy} watchesPid={t._pids.Contains(facts.OwnerPid)} " &
                                           $"nameMatches={AdobeSaveDialogFilter.NameMatches(offered, t._targetPath)} onScreen={t.OnScreen()} target={t._targetPath}"))
        End If
        Dim chosen As AdobeSaveTrap = ChooseOwner(offered, facts.OwnerPid, how)
        Tr($"Arbitrate {AcroPdfTraceLog.Hex(hwnd)}: chosen={If(chosen Is Nothing, "(nobody)", "SaveTrap#" & chosen._id)} how={how}")
        If chosen Is Nothing Then
            Claims(hwnd) = Me
            Report($"Dialog «Salvare ca» 0x{hwnd.ToInt64():X}: {how}")
            Cancel(hwnd, $"Nu se poate stabili cărui document îi aparține salvarea (nume propus de Adobe: «{offered}»).")
            Return
        End If
        Claims(hwnd) = chosen
        Report($"Dialog «Salvare ca» 0x{hwnd.ToInt64():X} (nume propus «{offered}») dat capcanei pentru " &
               $"«{Path.GetFileName(chosen._targetPath)}» — {how}.")
        chosen.HandleSaveAs(hwnd, GatherFacts(hwnd))
    End Sub

    ' Who owns a Save As, among the traps watching its process. Nothing = nobody can be sure.
    Private Shared Function ChooseOwner(offered As String, ownerPid As Integer, ByRef how As String) As AdobeSaveTrap
        Dim candidates As List(Of AdobeSaveTrap) =
            Running.Where(Function(t) t.IsRunning AndAlso t._pids.Contains(ownerPid)).ToList()
        If candidates.Count = 1 Then
            how = "singura capcană care urmărește procesul"
            Return candidates(0)
        End If
        Dim byName As List(Of AdobeSaveTrap) =
            candidates.Where(Function(t) AdobeSaveDialogFilter.NameMatches(offered, t._targetPath)).ToList()
        If byName.Count = 1 Then
            how = "numele propus de Adobe este al acestui document"
            Return byName(0)
        End If
        Dim pool As List(Of AdobeSaveTrap) = If(byName.Count > 1, byName, candidates)
        Dim shown As List(Of AdobeSaveTrap) = pool.Where(Function(t) t.OnScreen()).ToList()
        If shown.Count = 1 Then
            how = "vizualizatorul acestui document este cel de pe ecran"
            Return shown(0)
        End If
        how = $"ANULAT — {candidates.Count} capcane urmăresc procesul, {byName.Count} după nume, {shown.Count} pe ecran."
        Return Nothing
    End Function

    Private Function OnScreen() As Boolean
        Try
            Return IsOnScreen IsNot Nothing AndAlso IsOnScreen.Invoke()
        Catch ex As Exception
            ' A viewer being torn down is simply «not on screen».
            GlobalErrorLog.Write("AdobeSaveTrap.OnScreen", ex)
            Return False
        End Try
    End Function

    ' Claims and dismissed boxes of windows that no longer exist.
    Private Shared Sub PruneShared()
        For Each h As IntPtr In Claims.Keys.Where(Function(k) Not AdobeNativeMethods.IsWindow(k)).ToList()
            Claims.Remove(h)
        Next
        Dismissed.RemoveWhere(Function(k) Not AdobeNativeMethods.IsWindow(k))
        For Each h As IntPtr In ScriptClicks.Keys.Where(Function(k) Not AdobeNativeMethods.IsWindow(k)).ToList()
            ScriptClicks.Remove(h)
            ScriptAttempts.Remove(h)
        Next
    End Sub

    Private Sub HandleSaveAs(hwnd As IntPtr, facts As AdobeDialogFacts)
        ' Off screen FIRST: the operator must not get a chance to type a folder.
        MoveOffScreen(hwnd)
        If hwnd <> _lastDialog Then _presses = 0
        _lastDialog = hwnd
        _presses += 1
        Report($"Dialog «Salvare ca» prins: 0x{hwnd.ToInt64():X} {facts.Describe()} (apăsarea {_presses})")
        If _presses > MaxPresses Then
            _pending = IntPtr.Zero
            Cancel(hwnd, $"Dialogul «Salvare ca» a reapărut după {MaxPresses} apăsări pe «Salvare».")
            Return
        End If

        Dim edit As IntPtr = FindFileNameEdit(hwnd)
        Tr($"HandleSaveAs {AcroPdfTraceLog.Hex(hwnd)}: moved off screen, now at {AdobeNativeMethods.RectOnScreen(hwnd)}; " &
           $"file-name edit={AcroPdfTraceLog.Hex(edit)}" &
           If(edit = IntPtr.Zero, "", $" id={AdobeNativeMethods.GetDlgCtrlID(edit)} parent={AdobeNativeMethods.GetClass(AdobeNativeMethods.GetParent(edit))} text before=«{AdobeNativeMethods.ReadText(edit)}»"))
        If edit = IntPtr.Zero Then
            If Tracing Then TraceChildren(hwnd, "HandleSaveAs: no file-name edit; dialog children")
            _pending = IntPtr.Zero
            Cancel(hwnd, "Caseta cu numele fișierului nu a fost găsită în dialogul «Salvare ca».")
            Return
        End If
        Dim wrote As Boolean = AdobeNativeMethods.WriteText(edit, _targetPath)
        Dim readBack As String = AdobeNativeMethods.ReadText(edit)
        Tr($"HandleSaveAs {AcroPdfTraceLog.Hex(hwnd)}: WriteText -> {wrote}, read back «{readBack}», " &
           $"same path={AdobeSaveDialogFilter.SamePath(readBack, _targetPath)}")
        If Not AdobeSaveDialogFilter.SamePath(readBack, _targetPath) Then
            _pending = IntPtr.Zero
            Cancel(hwnd, $"Numele din dialog («{readBack}») nu a putut fi fixat pe «{_targetPath}».")
            Return
        End If

        Claims(hwnd) = Me
        _pending = hwnd
        _pendingSince = DateTime.UtcNow
        _hiddenSince = Nothing
        _confirmed = False
        Dim posted As Boolean = AdobeNativeMethods.PostMessage(hwnd, AdobeNativeMethods.WM_COMMAND,
                                                               New IntPtr(AdobeNativeMethods.IDOK), IntPtr.Zero)
        Tr($"HandleSaveAs {AcroPdfTraceLog.Hex(hwnd)}: PostMessage(WM_COMMAND, IDOK) -> {posted}; " & StateText())
        Report($"«Salvare» apăsat pe {_targetPath}.")
    End Sub

    Private Sub HandleConfirm(hwnd As IntPtr, facts As AdobeDialogFacts)
        MoveOffScreen(hwnd)
        _confirmed = True
        ' Answered ONCE: the prompt takes a moment to close, and every sweep would press it again.
        If Not _answered.Add(hwnd) Then
            Tr($"HandleConfirm {AcroPdfTraceLog.Hex(hwnd)}: already answered -> waiting for it to close")
            Return
        End If
        Dim posted As Boolean
        If facts.HasYesButton Then
            posted = AdobeNativeMethods.PostMessage(hwnd, AdobeNativeMethods.WM_COMMAND,
                                                    New IntPtr(AdobeNativeMethods.IDYES), IntPtr.Zero)
        Else
            posted = AdobeNativeMethods.PostMessage(hwnd, AdobeNativeMethods.TDM_CLICK_BUTTON,
                                                    New IntPtr(AdobeNativeMethods.IDYES), IntPtr.Zero)
        End If
        Tr($"HandleConfirm {AcroPdfTraceLog.Hex(hwnd)}: {If(facts.HasYesButton, "WM_COMMAND IDYES", "TDM_CLICK_BUTTON IDYES")} posted -> {posted}")
        Report($"Confirmarea de suprascriere acceptată: 0x{hwnd.ToInt64():X} {facts.Describe()}")
    End Sub

    ' The pressed Save either closes (done), stays hidden long enough (done), shows itself again
    ' (pressed again -- measured: it can blink while handling the click), or outlives the timeout
    ' (cancelled -- an invisible modal dialog left alive would freeze Adobe for good).
    Private Sub CheckPending()
        If _pending = IntPtr.Zero Then Return
        Dim gone As Boolean = Not AdobeNativeMethods.IsWindow(_pending)
        Tr($"CheckPending {AcroPdfTraceLog.Hex(_pending)}: exists={Not gone} " &
           $"visible={Not gone AndAlso AdobeNativeMethods.IsWindowVisible(_pending)} " &
           $"elapsed={(DateTime.UtcNow - _pendingSince).TotalMilliseconds:0} ms; " & StateText())
        If Not gone AndAlso Not AdobeNativeMethods.IsWindowVisible(_pending) Then
            If Not _hiddenSince.HasValue Then _hiddenSince = DateTime.UtcNow
            gone = (DateTime.UtcNow - _hiddenSince.Value).TotalMilliseconds >= HiddenDoneMs
        ElseIf Not gone Then
            If _hiddenSince.HasValue AndAlso Not _confirmed Then
                ' Hidden, then back without asking «replace?»: the click did not take. Again.
                _hiddenSince = Nothing
                Report($"Dialogul «Salvare ca» 0x{_pending.ToInt64():X} a reapărut — apăs din nou.")
                HandleSaveAs(_pending, GatherFacts(_pending))
                Return
            End If
            _hiddenSince = Nothing
        End If
        If gone Then
            Dim path As String = _targetPath
            _pending = IntPtr.Zero
            _hiddenSince = Nothing
            Report($"«Salvare ca» încheiat ({If(_confirmed, "cu", "fără")} confirmare de suprascriere): {path}")
            ' Paused while the save ran: now that it is done, the timer stops too.
            If _paused Then HaltWatching()
            RaiseEvent Saved(path)
            Return
        End If
        If (DateTime.UtcNow - _pendingSince).TotalMilliseconds > SaveTimeoutMs Then
            Dim hwnd As IntPtr = _pending
            _pending = IntPtr.Zero
            Cancel(hwnd, "Dialogul «Salvare ca» nu s-a închis după apăsarea butonului «Salvare».")
        End If
    End Sub

    ' A script alert or the script console of our Adobe, re-checked on EVERY sweep (Adobe shows the
    ' console again on the next error). MEASURED 23.09.2026 (Window Detective + log):
    '   * the alert holds, inside a GroupBox, a check box, OK, an unnamed button and Cancel -- ALL
    '     with control id 0, so a WM_COMMAND by id presses nothing. The OK button is found by its
    '     TEXT and pressed by messages, never keystrokes -- three different ways, see PressOk;
    '   * the console and the button-less warning frame have no OK: they are hidden (SW_HIDE) --
    '     the console ONLY when it opened itself (near an error box or a load, ConsoleAfterLoadMs);
    ' An alert WITH an OK that is still there after MaxScriptClicks presses is NO LONGER hidden
    ' (24.09.2026): it is modal to Adobe's view, and hidden it kept the ActiveX control blank for
    ' good. It stays on screen for the operator, with a warning in the log.
    Private Sub DismissScriptNoise(hwnd As IntPtr, facts As AdobeDialogFacts)
        If Not AdobeNativeMethods.IsWindowVisible(hwnd) Then
            Return
        End If
        Dim isConsole As Boolean = facts.Title.StartsWith(ScriptConsoleTitle, StringComparison.OrdinalIgnoreCase)
        Dim now As DateTime = DateTime.UtcNow
        Tr($"DismissScriptNoise {AcroPdfTraceLog.Hex(hwnd)} «{facts.Title}»: console={isConsole} " &
           $"sinceLastAlert={(now - _lastScriptAlert).TotalMilliseconds:0} ms sinceStart={(now - _startedAt).TotalMilliseconds:0} ms")
        If isConsole Then
            Dim afterAlert As Boolean = (now - _lastScriptAlert).TotalMilliseconds <= ScriptBurstQuietMs
            Dim afterLoad As Boolean = (now - _startedAt).TotalMilliseconds <= ConsoleAfterLoadMs
            If Not afterAlert AndAlso Not afterLoad Then
                ' Opened by the operator: theirs. Logged once per showing.
                If _seen.Add(hwnd) Then Report($"Consola de script Adobe 0x{hwnd.ToInt64():X} deschisă fără eroare — lăsată în pace.")
                Return
            End If
            _seen.Remove(hwnd)
        Else
            _lastScriptAlert = now
        End If
        EnterScriptBurst()
        Dim clickedAt As DateTime
        If ScriptClicks.TryGetValue(hwnd, clickedAt) AndAlso
           (DateTime.UtcNow - clickedAt).TotalMilliseconds < ScriptClickRetryMs Then
            Return
        End If

        Dim okButton As IntPtr = IntPtr.Zero
        Dim texts As New List(Of String)()
        For Each c As IntPtr In AdobeNativeMethods.Descendants(hwnd)
            Dim cls As String = AdobeNativeMethods.GetClass(c)
            If String.Equals(cls, "Button", StringComparison.OrdinalIgnoreCase) Then
                If okButton = IntPtr.Zero AndAlso AdobeNativeMethods.IsVisibleStyleSet(c) AndAlso
                   AdobeSaveDialogFilter.IsOkCaption(AdobeNativeMethods.ReadText(c)) Then okButton = c
            ElseIf cls.StartsWith("Static", StringComparison.OrdinalIgnoreCase) Then
                Dim t As String = AdobeNativeMethods.ReadText(c).Trim()
                If t.Length > 0 Then texts.Add(t.Replace(vbCr, " ").Replace(vbLf, " "))
            End If
        Next
        Dim text As String = If(texts.Count = 0, "(fără text citibil)", String.Join(" | ", texts))
        If text.Length > 500 Then text = text.Substring(0, 500) & "…"
        Dim what As String = If(isConsole, "Consola de script", "Mesajul de script")

        Dim attempts As Integer = 0
        ScriptAttempts.TryGetValue(hwnd, attempts)
        If Not isConsole AndAlso okButton <> IntPtr.Zero AndAlso attempts < MaxScriptClicks Then
            ScriptAttempts(hwnd) = attempts + 1
            ScriptClicks(hwnd) = DateTime.UtcNow
            Dim how As String = PressOk(hwnd, okButton, attempts)
            Report($"{what} Adobe 0x{hwnd.ToInt64():X}: apăsat «OK» (încercarea {attempts + 1}, {how}): {text}")
            Return
        End If

        If Not isConsole AndAlso okButton <> IntPtr.Zero Then
            ' MEASURED 24.09.2026 (acropdf_trace.log): the alert is MODAL to Adobe's view, and while it
            ' is up Adobe builds nothing in the control. Hiding it (the old last resort) left the
            ' control blank for good, with no way out for the operator. So an alert that survived
            ' every way of pressing OK is LEFT ON SCREEN: the operator can press OK by hand. Said
            ' once per alert; the trace recording ends here, this is the blocking error.
            If Dismissed.Add(hwnd) Then
                Report($"ATENȚIE: {what} Adobe 0x{hwnd.ToInt64():X} nu s-a închis după {attempts} apăsări pe «OK» — " &
                       $"îl las pe ecran (ascuns ar bloca Adobe): {text}")
                If Traced Then AcroPdfTraceLog.EndSession($"SaveTrap#{_id}", $"BLOCKING: script alert {AcroPdfTraceLog.Hex(hwnd)} did not close after {attempts} OK presses: {text}")
            End If
            Return
        End If

        Dim hid As Boolean = AdobeNativeMethods.ShowWindow(hwnd, AdobeNativeMethods.SW_HIDE)
        Tr($"DismissScriptNoise {AcroPdfTraceLog.Hex(hwnd)}: ShowWindow(SW_HIDE) -> {hid} (okButton={AcroPdfTraceLog.Hex(okButton)}, attempts={attempts}); text: {text}")
        If Dismissed.Add(hwnd) Then
            Dim why As String = If(isConsole, "consola nu are «OK»",
                                   If(okButton = IntPtr.Zero, "fără buton «OK»", $"«OK» apăsat de {attempts} ori fără efect"))
            Report($"{what} Adobe 0x{hwnd.ToInt64():X} «{facts.Title}» ascuns ({why}): {text}")
        End If
    End Sub

    ' One press of a script alert's OK, a DIFFERENT way per attempt. MEASURED 24.09.2026: BM_CLICK
    ' returned True three times and the alert stayed -- the alert is owned by K-BOT's main form,
    ' which held the foreground, and BM_CLICK is documented to fail when the dialog is not active.
    '   0: tell the dialog its OK was clicked (WM_COMMAND / BN_CLICKED with the button's handle,
    '      posted) -- needs no activation;
    '   1: activate the dialog, then BM_CLICK (the documented way);
    '   2: WM_CLOSE to the dialog (what the title bar's X does).
    ' Messages only, never keystrokes (operator, 23.09.2026). Returns what was done, for the log.
    Private Function PressOk(dialog As IntPtr, okButton As IntPtr, attempt As Integer) As String
        Select Case attempt
            Case 0
                Dim wParam As New IntPtr((AdobeNativeMethods.BN_CLICKED << 16) Or (AdobeNativeMethods.GetDlgCtrlID(okButton) And &HFFFF))
                Dim posted As Boolean = AdobeNativeMethods.PostMessage(dialog, AdobeNativeMethods.WM_COMMAND, wParam, okButton)
                Tr($"PressOk {AcroPdfTraceLog.Hex(dialog)}: WM_COMMAND BN_CLICKED (wParam=0x{wParam.ToInt64():X}, button {AcroPdfTraceLog.Hex(okButton)}) posted -> {posted}")
                Return "WM_COMMAND"
            Case 1
                Dim front As Boolean = AdobeNativeMethods.SetForegroundWindow(dialog)
                Dim sent As Boolean = AdobeNativeMethods.SendWithTimeout(okButton, AdobeNativeMethods.BM_CLICK, IntPtr.Zero, IntPtr.Zero)
                Tr($"PressOk {AcroPdfTraceLog.Hex(dialog)}: SetForegroundWindow -> {front} (foreground now {AcroPdfTraceLog.Hex(AdobeNativeMethods.GetForegroundWindow())}), BM_CLICK -> {sent}")
                Return "activare + BM_CLICK"
            Case Else
                Dim posted As Boolean = AdobeNativeMethods.PostMessage(dialog, AdobeNativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero)
                Tr($"PressOk {AcroPdfTraceLog.Hex(dialog)}: WM_CLOSE posted -> {posted}")
                Return "WM_CLOSE"
        End Select
    End Function

    ''' <summary>
    ''' True while Adobe's form scripts are raising alerts (a burst is running). The ActiveX viewer
    ''' does not count its layout attempts meanwhile: the view is built only after the alerts close.
    ''' </summary>
    Public ReadOnly Property InScriptBurst As Boolean
        Get
            Return DateTime.UtcNow <= _burstUntil
        End Get
    End Property

    ' A script box is showing: sweep every ScriptBurstIntervalMs until ScriptBurstQuietMs pass
    ' without another one.
    Private Sub EnterScriptBurst()
        _burstUntil = DateTime.UtcNow.AddMilliseconds(ScriptBurstQuietMs)
        If _timer.Interval <> ScriptBurstIntervalMs Then
            _timer.Interval = ScriptBurstIntervalMs
            Report($"Capcana: mesaj de script detectat — verificare la {ScriptBurstIntervalMs} ms.")
        End If
    End Sub

    Private Sub Cancel(hwnd As IntPtr, reason As String)
        Dim posted As Boolean = AdobeNativeMethods.PostMessage(hwnd, AdobeNativeMethods.WM_COMMAND,
                                                               New IntPtr(AdobeNativeMethods.IDCANCEL), IntPtr.Zero)
        Tr($"Cancel {AcroPdfTraceLog.Hex(hwnd)}: PostMessage(WM_COMMAND, IDCANCEL) -> {posted}; reason: {reason}")
        Report("ANULAT: " & reason)
        If _paused AndAlso Not IsBusy Then HaltWatching()
        RaiseEvent Failed(reason)
    End Sub

    Private Shared Sub MoveOffScreen(hwnd As IntPtr)
        AdobeNativeMethods.SetWindowPos(hwnd, IntPtr.Zero, -32000, -32000, 0, 0,
                                        AdobeNativeMethods.SWP_NOSIZE Or AdobeNativeMethods.SWP_NOZORDER Or
                                        AdobeNativeMethods.SWP_NOACTIVATE)
    End Sub

    Private Shared Function GatherFacts(hwnd As IntPtr) As AdobeDialogFacts
        Dim facts As New AdobeDialogFacts With {
            .ClassName = AdobeNativeMethods.GetClass(hwnd),
            .OwnerPid = AdobeNativeMethods.OwnerPid(hwnd),
            .OwnerWindow = AdobeNativeMethods.GetWindow(hwnd, AdobeNativeMethods.GW_OWNER),
            .Title = AdobeNativeMethods.GetTitle(hwnd)}
        Dim hasDirectUi As Boolean = False
        For Each c As IntPtr In AdobeNativeMethods.Descendants(hwnd)
            Dim cls As String = AdobeNativeMethods.GetClass(c)
            If String.Equals(cls, "Button", StringComparison.OrdinalIgnoreCase) AndAlso
               AdobeNativeMethods.GetParent(c) = hwnd Then
                Dim id As Integer = AdobeNativeMethods.GetDlgCtrlID(c)
                If id = AdobeNativeMethods.IDOK Then facts.HasOkButton = True
                If id = AdobeNativeMethods.IDYES Then facts.HasYesButton = True
            ElseIf String.Equals(cls, "DirectUIHWND", StringComparison.OrdinalIgnoreCase) Then
                hasDirectUi = True
            End If
        Next
        facts.HasFileNameEdit = FindFileNameEdit(hwnd) <> IntPtr.Zero
        facts.IsTaskDialog = hasDirectUi AndAlso Not facts.HasFileNameEdit
        Return facts
    End Function

    ' The file-name box: an Edit whose parent or grandparent is the combo with id 1148 / 1001
    ' (classic cmb13 -> ComboBox -> Edit), or the old edt1 (1152), or -- MEASURED 23.09.2026 on
    ' Windows 11, the dialog Acrobat actually shows -- an Edit with id 1001 ITSELF inside a ComboBox
    ' whose own id is 0 (DirectUIHWND > FloatNotifySink > ComboBox(0) > Edit(1001)). The address
    ' bar's Edit sits in a ComboBox too, but carries id 41477, so it never matches.
    Private Shared Function FindFileNameEdit(dialog As IntPtr) As IntPtr
        For Each c As IntPtr In AdobeNativeMethods.Descendants(dialog)
            If Not String.Equals(AdobeNativeMethods.GetClass(c), "Edit", StringComparison.OrdinalIgnoreCase) Then Continue For
            Dim id As Integer = AdobeNativeMethods.GetDlgCtrlID(c)
            If id = AdobeSaveDialogFilter.FileNameEditId AndAlso
               AdobeNativeMethods.GetParent(c) = dialog Then Return c
            If AdobeSaveDialogFilter.FileNameComboIds.Contains(id) AndAlso
               AdobeNativeMethods.GetClass(AdobeNativeMethods.GetParent(c)).StartsWith("ComboBox", StringComparison.OrdinalIgnoreCase) Then Return c
            Dim p As IntPtr = AdobeNativeMethods.GetParent(c)
            Dim gp As IntPtr = If(p = IntPtr.Zero, IntPtr.Zero, AdobeNativeMethods.GetParent(p))
            If IsFileNameCombo(p) OrElse IsFileNameCombo(gp) Then Return c
        Next
        Return IntPtr.Zero
    End Function

    Private Shared Function IsFileNameCombo(h As IntPtr) As Boolean
        If h = IntPtr.Zero Then Return False
        Dim cls As String = AdobeNativeMethods.GetClass(h)
        If Not cls.StartsWith("ComboBox", StringComparison.OrdinalIgnoreCase) Then Return False
        Return AdobeSaveDialogFilter.FileNameComboIds.Contains(AdobeNativeMethods.GetDlgCtrlID(h))
    End Function

    Private Sub Report(line As String)
        _log?.Invoke(line)
        Tr("REPORT: " & line)
    End Sub

    ' Every descendant of a dialog with class, id, text and visibility -- for a dialog whose shape
    ' surprised the trap. Reached from HandleSaveAs (transitively wrapped).
    Private Sub TraceChildren(hwnd As IntPtr, header As String)
        AcroPdfTraceLog.WriteBlock($"SaveTrap#{_id}", $"{header} ({AcroPdfTraceLog.Hex(hwnd)}):",
            AdobeNativeMethods.Descendants(hwnd).Select(
                Function(c) $"{AcroPdfTraceLog.Hex(c)} class={AdobeNativeMethods.GetClass(c)} id={AdobeNativeMethods.GetDlgCtrlID(c)} " &
                            $"parent={AcroPdfTraceLog.Hex(AdobeNativeMethods.GetParent(c))} visible={AdobeNativeMethods.IsVisibleStyleSet(c)} " &
                            $"text=«{AdobeNativeMethods.ReadText(c)}»"))
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            [Stop]()
            RemoveHandler _timer.Tick, AddressOf OnTick
            _timer.Dispose()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSaveTrap.Dispose", ex)
        End Try
    End Sub

End Class
