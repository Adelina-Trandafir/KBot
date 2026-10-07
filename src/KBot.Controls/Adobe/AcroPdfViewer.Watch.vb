Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports KBot.Common

' ACTIVEX-CHECK (slice 0078-15): the temporary watch of Adobe's windows and of the operator's clicks.
' Part of AcroPdfViewer; the whole file goes when the operator says the investigation is over.
Partial Public NotInheritable Class AcroPdfViewer

    ' ══ ACTIVEX-CHECK: temporary window watch (slice 0078-15) ══════════════════════════════════
    '
    ' Two out-of-context WinEvent hooks (object events 0x8000..0x800B, and the foreground change),
    ' from the load until Clear. Out-of-context = the callback runs on OUR UI thread, through its
    ' message queue. Every line says:
    '   +N ms   -- time since the load started;
    '   lag N   -- how long the event waited in our queue before we saw it. A large lag means
    '              K-BOT's UI thread was not running for that long (blocked), whoever blocked it.
    ' What is logged: windows inside the control; top-level windows of Adobe processes (its boxes);
    ' our own form (enabled / disabled -- a modal box disables it); the keyboard focus; the
    ' foreground window. LOCATIONCHANGE only when the rectangle really changed.
    ' Milestones (MILESTONE lines, each with a tree dump): first window inside the control; page
    ' view («AVPageView») visible with a size; first Adobe box; first focus inside the control.

    Private Const CheckTag As String = "[ACTIVEX-CHECK] "
    Private Const EVENT_SYSTEM_FOREGROUND As UInteger = &H3UI
    Private Const EVENT_OBJECT_HIDE As UInteger = &H8003UI
    Private Const EVENT_OBJECT_FOCUS As UInteger = &H8005UI
    Private Const PageViewTitle As String = "AVPageView"
    Private Shared ReadOnly AdobeProcessNames As String() = {"Acrobat", "AcroRd32"}

    Private _clock As Diagnostics.Stopwatch
    Private _hookObject As IntPtr
    Private _hookForeground As IntPtr
    ' Kept in a field: the delegate must outlive the hooks (a collected callback crashes the process).
    Private _eventProc As AdobeNativeMethods.WinEventProc
    ' Low-level mouse hook: the time of every click the operator makes on the control (same rule for the delegate).
    Private _hookMouse As IntPtr
    Private _mouseProc As AdobeNativeMethods.LowLevelMouseProc
    ' Windows seen inside the control / Adobe top-level windows: hwnd -> description (DESTROY
    ' arrives when the window is already gone and can no longer be read).
    Private ReadOnly _controlWindows As New Dictionary(Of IntPtr, String)()
    Private ReadOnly _adobeTopWindows As New Dictionary(Of IntPtr, String)()
    Private ReadOnly _lastRect As New Dictionary(Of IntPtr, Rectangle)()
    Private ReadOnly _pidIsAdobe As New Dictionary(Of Integer, Boolean)()
    Private _formEnabled As Boolean = True
    Private _seenFirstWindow As Boolean
    Private _seenPageView As Boolean
    Private _seenAdobeBox As Boolean
    Private _seenFocusInside As Boolean
    ' Reactivation (see ReactivateIfAdobeTookForeground): was our form active when the load started; done once already.
    Private _formForegroundAtLoad As Boolean
    Private _reactivated As Boolean

    Private Sub StartWatch(k_path As String)
        StopWatch()
        _controlWindows.Clear()
        _adobeTopWindows.Clear()
        _lastRect.Clear()
        _seenFirstWindow = False
        _seenPageView = False
        _seenAdobeBox = False
        _seenFocusInside = False
        Dim k_form As Form = _panel.FindForm()
        _formEnabled = k_form Is Nothing OrElse Not k_form.IsHandleCreated OrElse AdobeNativeMethods.IsWindowEnabled(k_form.Handle)
        _formForegroundAtLoad = k_form IsNot Nothing AndAlso k_form.IsHandleCreated AndAlso
                                AdobeNativeMethods.GetForegroundWindow() = k_form.Handle
        _reactivated = False
        _clock = Diagnostics.Stopwatch.StartNew()
        If _eventProc Is Nothing Then _eventProc = AddressOf OnWinEvent
        _hookObject = AdobeNativeMethods.SetWinEventHook(
            AdobeNativeMethods.EVENT_OBJECT_CREATE, AdobeNativeMethods.EVENT_OBJECT_LOCATIONCHANGE,
            IntPtr.Zero, _eventProc, 0UI, 0UI, AdobeNativeMethods.WINEVENT_OUTOFCONTEXT)
        _hookForeground = AdobeNativeMethods.SetWinEventHook(
            EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
            IntPtr.Zero, _eventProc, 0UI, 0UI, AdobeNativeMethods.WINEVENT_OUTOFCONTEXT)
        ' Mouse hook switched OFF (operator, 07.10.2026: src only for now). Kept for later.
        'If _mouseProc Is Nothing Then _mouseProc = AddressOf OnMouseLowLevel
        '_hookMouse = AdobeNativeMethods.SetWindowsHookEx(
        '    AdobeNativeMethods.WH_MOUSE_LL, _mouseProc, AdobeNativeMethods.GetModuleHandle(Nothing), 0UI)
        'Check($"mouse hook {HexOf(_hookMouse)}")
        Dim k_tag As String = ViewerTag()
        Threading.ThreadPool.QueueUserWorkItem(Sub(k_state) Check("processes at load: " & AdobeProcesses(), k_tag))
        Check($"===== LOAD «{k_path}» ({If(File.Exists(k_path), New FileInfo(k_path).Length.ToString() & " bytes", "missing")}, " &
              $"written {If(File.Exists(k_path), File.GetLastWriteTime(k_path).ToString("HH:mm:ss.fff"), "-")}) " &
              $"control={HexOf(_host.Handle)} {DescribePanel()} form={DescribeForm()} foreground={Describe(AdobeNativeMethods.GetForegroundWindow())} " &
              $"hooks object={HexOf(_hookObject)} foreground={HexOf(_hookForeground)} pid={Environment.ProcessId}")
    End Sub

    Private Sub StopWatch()
        If _hookObject <> IntPtr.Zero Then AdobeNativeMethods.UnhookWinEvent(_hookObject)
        If _hookForeground <> IntPtr.Zero Then AdobeNativeMethods.UnhookWinEvent(_hookForeground)
        If _hookMouse <> IntPtr.Zero Then AdobeNativeMethods.UnhookWindowsHookEx(_hookMouse)
        If _hookObject <> IntPtr.Zero OrElse _hookForeground <> IntPtr.Zero Then Check("watch stopped")
        _hookObject = IntPtr.Zero
        _hookForeground = IntPtr.Zero
        _hookMouse = IntPtr.Zero
    End Sub

    ' Low-level mouse hook callback: the time of every click the operator makes on the control. Adobe's windows belong to
    ' Adobe's process, so their clicks never reach a WinForms event; the AcroPDF control has no click event either (only
    ' OnError / OnMessage). Runs on the UI thread for EVERY click on the desktop, so it stays short: no window text (that
    ' would send a message to Adobe's process). Boundary: log and swallow; the click is always passed on.
    Private Function OnMouseLowLevel(nCode As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
        Try
            Dim k_msg As Long = wParam.ToInt64()
            If nCode >= 0 AndAlso (k_msg = AdobeNativeMethods.WM_LBUTTONDOWN OrElse k_msg = AdobeNativeMethods.WM_RBUTTONDOWN) AndAlso
               _host IsNot Nothing AndAlso _host.IsHandleCreated Then
                Dim k_info As AdobeNativeMethods.MSLLHOOKSTRUCT =
                    System.Runtime.InteropServices.Marshal.PtrToStructure(Of AdobeNativeMethods.MSLLHOOKSTRUCT)(lParam)
                Dim k_rect As Rectangle = AdobeNativeMethods.RectOnScreen(_host.Handle)
                If k_rect.Contains(k_info.X, k_info.Y) Then
                    Dim k_hit As IntPtr = AdobeNativeMethods.WindowFromPoint(
                        New AdobeNativeMethods.POINTSTRUCT With {.X = k_info.X, .Y = k_info.Y})
                    Dim k_button As String = If(k_msg = AdobeNativeMethods.WM_LBUTTONDOWN, "left", "right")
                    Check($"{Stamp(LagMs(k_info.Time))} >>>>> CLICK ({k_button}) on the control at {k_info.X - k_rect.X},{k_info.Y - k_rect.Y} " &
                          $"-> {HexOf(k_hit)} class={AdobeNativeMethods.GetClass(k_hit)} pid={AdobeNativeMethods.OwnerPid(k_hit)} <<<<<")
                    ' Also in adobe_preview.log and the bench's live box -- posted, so the hook returns at once.
                    Dim k_time As String = DateTime.Now.ToString("HH:mm:ss.fff")
                    _panel.BeginInvoke(New Action(Sub() Report($"Operatorul: click în documentul Adobe la {k_time}.")))
                End If
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.OnMouseLowLevel", ex)
        End Try
        Return AdobeNativeMethods.CallNextHookEx(_hookMouse, nCode, wParam, lParam)
    End Function

    ' WinEvent callback (UI thread). Boundary: log and swallow.
    Private Sub OnWinEvent(hook As IntPtr, eventType As UInteger, hwnd As IntPtr,
                           idObject As Integer, idChild As Integer,
                           threadId As UInteger, timestamp As UInteger)
        Try
            If hwnd = IntPtr.Zero OrElse _host Is Nothing OrElse Not _host.IsHandleCreated Then Return
            Dim k_lag As Long = LagMs(timestamp)

            If eventType = EVENT_SYSTEM_FOREGROUND Then
                Check($"{Stamp(k_lag)} FOREGROUND {Describe(hwnd)}")
                ReadModeOnEvent()   ' Ctrl+H (AcroPdfViewer.ReadMode.vb), not check-only
                'ReactivateIfAdobeTookForeground(hwnd)   ' switched off (operator, 07.10.2026: src only for now)
                Return
            End If
            ' Only the events EventName knows (no reorder / selection noise).
            If EventName(eventType).StartsWith("0x", StringComparison.Ordinal) Then Return
            ' Focus arrives with the client object id; everything else only for the window itself.
            If eventType <> EVENT_OBJECT_FOCUS AndAlso (idObject <> AdobeNativeMethods.OBJID_WINDOW OrElse idChild <> 0) Then Return

            Dim k_form As Form = _panel.FindForm()
            If k_form IsNot Nothing AndAlso k_form.IsHandleCreated AndAlso hwnd = k_form.Handle Then
                OnFormEvent(eventType, k_form, k_lag)
                ReadModeOnEvent()   ' Ctrl+H (AcroPdfViewer.ReadMode.vb), not check-only
                Return
            End If

            Dim k_inside As Boolean = hwnd = _host.Handle OrElse AdobeNativeMethods.IsChild(_host.Handle, hwnd)
            If k_inside OrElse (eventType = AdobeNativeMethods.EVENT_OBJECT_DESTROY AndAlso _controlWindows.ContainsKey(hwnd)) Then
                OnControlEvent(eventType, hwnd, k_lag)
                NudgeIfAdobeWindowEmpty()   ' the fix under test (AcroPdfViewer.SizeNudge.vb), not check-only
                ReadModeOnEvent()   ' Ctrl+H (AcroPdfViewer.ReadMode.vb), not check-only
                Return
            End If

            If _adobeTopWindows.ContainsKey(hwnd) OrElse IsAdobeTopLevel(hwnd) Then OnAdobeTopEvent(eventType, hwnd, k_lag)
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.OnWinEvent", ex)
        End Try
    End Sub

    Private Sub OnFormEvent(eventType As UInteger, k_form As Form, k_lag As Long)
        If eventType = AdobeNativeMethods.EVENT_OBJECT_STATECHANGE Then
            Dim k_enabled As Boolean = AdobeNativeMethods.IsWindowEnabled(k_form.Handle)
            If k_enabled = _formEnabled Then Return
            _formEnabled = k_enabled
            Check($"{Stamp(k_lag)} FORM {If(k_enabled, "ENABLED again", "DISABLED (a modal box is up)")}")
        ElseIf eventType = EVENT_OBJECT_FOCUS Then
            Check($"{Stamp(k_lag)} FOCUS on the K-BOT form itself")
        End If
    End Sub

    Private Sub OnControlEvent(eventType As UInteger, hwnd As IntPtr, k_lag As Long)
        If eventType = AdobeNativeMethods.EVENT_OBJECT_DESTROY Then
            Dim k_was As String = Nothing
            _controlWindows.TryGetValue(hwnd, k_was)
            _controlWindows.Remove(hwnd)
            _lastRect.Remove(hwnd)
            Check($"{Stamp(k_lag)} DESTROY in control {HexOf(hwnd)} {k_was}")
            Return
        End If

        Dim k_rect As Rectangle = AdobeNativeMethods.RectInParent(hwnd)
        If eventType = AdobeNativeMethods.EVENT_OBJECT_LOCATIONCHANGE Then
            Dim k_old As Rectangle
            If _lastRect.TryGetValue(hwnd, k_old) AndAlso k_old = k_rect Then Return
        End If
        _lastRect(hwnd) = k_rect
        Dim k_text As String = DescribeChild(hwnd, k_rect)
        _controlWindows(hwnd) = k_text
        Check($"{Stamp(k_lag)} {EventName(eventType)} in control {k_text}" &
              If(hwnd = _host.Handle, " " & DescribePanel(), ""))

        If Not _seenFirstWindow AndAlso hwnd <> _host.Handle Then
            _seenFirstWindow = True
            Milestone("first window inside the control")
            ' The primer has attached: the document is loaded, posted so LoadFile is not called inside the hook.
            If _primerPending IsNot Nothing AndAlso _panel.IsHandleCreated Then _panel.BeginInvoke(New MethodInvoker(AddressOf OnPrimerAttached))
        End If
        If Not _seenPageView AndAlso String.Equals(AdobeNativeMethods.GetTitle(hwnd), PageViewTitle, StringComparison.Ordinal) AndAlso
           AdobeNativeMethods.IsWindowVisible(hwnd) AndAlso k_rect.Width > 0 AndAlso k_rect.Height > 0 Then
            _seenPageView = True
            Milestone($"page view «{PageViewTitle}» visible with a size {k_rect.Width}x{k_rect.Height}")
        End If
        If eventType = EVENT_OBJECT_FOCUS AndAlso Not _seenFocusInside Then
            _seenFocusInside = True
            Milestone("first keyboard focus inside the control")
        End If
    End Sub

    Private Sub OnAdobeTopEvent(eventType As UInteger, hwnd As IntPtr, k_lag As Long)
        If eventType = AdobeNativeMethods.EVENT_OBJECT_DESTROY Then
            Dim k_was As String = Nothing
            _adobeTopWindows.TryGetValue(hwnd, k_was)
            _adobeTopWindows.Remove(hwnd)
            _lastRect.Remove(hwnd)
            Check($"{Stamp(k_lag)} DESTROY Adobe window {HexOf(hwnd)} {k_was}")
            Return
        End If

        Dim k_rect As Rectangle = AdobeNativeMethods.RectOnScreen(hwnd)
        If eventType = AdobeNativeMethods.EVENT_OBJECT_LOCATIONCHANGE Then
            Dim k_old As Rectangle
            If _lastRect.TryGetValue(hwnd, k_old) AndAlso k_old = k_rect Then Return
        End If
        _lastRect(hwnd) = k_rect
        Dim k_owner As IntPtr = AdobeNativeMethods.GetWindow(hwnd, AdobeNativeMethods.GW_OWNER)
        Dim k_form As Form = _panel.FindForm()
        Dim k_ownerText As String = If(k_owner = IntPtr.Zero, "none",
                                       If(k_form IsNot Nothing AndAlso k_form.IsHandleCreated AndAlso k_owner = k_form.Handle,
                                          "K-BOT form", HexOf(k_owner)))
        Dim k_text As String = $"{HexOf(hwnd)} class={AdobeNativeMethods.GetClass(hwnd)} title=«{AdobeNativeMethods.GetTitle(hwnd)}» " &
                               $"rect={k_rect} visible={AdobeNativeMethods.IsWindowVisible(hwnd)} owner={k_ownerText} pid={AdobeNativeMethods.OwnerPid(hwnd)}"
        _adobeTopWindows(hwnd) = k_text
        Check($"{Stamp(k_lag)} {EventName(eventType)} Adobe window {k_text}")
        ' An Adobe box (a script's «Warning: JavaScript Window», the «JavaScript Debugger» console): its text, when it
        ' appears and again when it goes (the console has grown by then).
        If (eventType = AdobeNativeMethods.EVENT_OBJECT_SHOW OrElse eventType = EVENT_OBJECT_HIDE) AndAlso
           AdobeNativeMethods.GetClass(hwnd) = "#32770" Then
            ReadBoxTextLater(hwnd, EventName(eventType))
        End If

        If eventType = AdobeNativeMethods.EVENT_OBJECT_SHOW AndAlso Not _seenAdobeBox Then
            _seenAdobeBox = True
            Milestone("first Adobe top-level window shown: " & AdobeNativeMethods.GetTitle(hwnd))
            DumpBox(hwnd)
        End If
    End Sub

    ' Reads every text inside an Adobe box on a background thread (reading a foreign window sends it a message; the UI
    ' thread must not wait on Adobe) and writes one BOX TEXT line. Background thread: must not throw.
    Private Sub ReadBoxTextLater(hwnd As IntPtr, k_when As String)
        Dim k_tag As String = ViewerTag()
        Dim k_at As Long = Elapsed()
        Threading.ThreadPool.QueueUserWorkItem(
            Sub(k_state)
                Try
                    Dim k_parts As New List(Of String)()
                    For Each k_child As IntPtr In AdobeNativeMethods.Descendants(hwnd)
                        Dim k_class As String = AdobeNativeMethods.GetClass(k_child)
                        Dim k_value As String = AdobeNativeMethods.ReadAllText(k_child)
                        If String.IsNullOrWhiteSpace(k_value) Then Continue For
                        k_parts.Add($"{k_class}: «{k_value.Replace(vbCrLf, " / ").Replace(vbLf, " / ").Replace(vbCr, " / ")}»")
                    Next
                    Check($"+{k_at} ms BOX TEXT ({k_when}) {HexOf(hwnd)} «{AdobeNativeMethods.GetTitle(hwnd)}»: " &
                          If(k_parts.Count = 0, "(no text)", String.Join(" | ", k_parts)), k_tag)
                Catch ex As Exception
                    GlobalErrorLog.Write("AcroPdfViewer.ReadBoxTextLater", ex)
                End Try
            End Sub)
    End Sub

    ' ── Reactivation: the fix under test, NOT check-only (operator, 06.10.2026) ──────────────────────────────────────────
    ' Measured on 7 runs: when Adobe makes one of its own hidden windows (an «Edit» at -16384,-16384) the foreground window
    ' and keeps it, the page is not laid out until the operator clicks -- which makes our form active again; when our form
    ' becomes active again by itself, the page is laid out at that moment. So: once per load, while the page is not laid
    ' out yet, if our form was the active window when the load started and an Adobe process takes the foreground, make the
    ' form active again. Driven by the FOREGROUND event, no timer. It lives on the watch hooks for now; when the check
    ' code goes, this moves with the hook it needs.

    Private Sub ReactivateIfAdobeTookForeground(hwnd As IntPtr)
        If _reactivated OrElse _seenPageView OrElse Not _formForegroundAtLoad Then Return
        If Not IsAdobePid(AdobeNativeMethods.OwnerPid(hwnd)) Then Return
        _reactivated = True
        Dim k_took As String = Describe(hwnd)
        ' Posted: it runs after the WinEvent callback has returned.
        _panel.BeginInvoke(New Action(Sub() Reactivate(k_took)))
    End Sub

    ' Posted from the WinEvent callback: log and swallow.
    Private Sub Reactivate(k_took As String)
        Try
            Dim k_form As Form = _panel.FindForm()
            If k_form Is Nothing OrElse _host Is Nothing OrElse Not _host.IsHandleCreated Then Return
            k_form.Activate()
            _host.Focus()
            Check($"+{Elapsed()} ms >>>>> REACTIVATE: Adobe took the foreground ({k_took}) -> form.Activate + control focus; " &   ' ACTIVEX-CHECK
                  $"foreground now {Describe(AdobeNativeMethods.GetForegroundWindow())}, focus {Describe(AdobeNativeMethods.GetFocus())} <<<<<")
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.Reactivate", ex)
        End Try
    End Sub

    ' A top-level window (not a child) owned by an Adobe process.
    Private Function IsAdobeTopLevel(hwnd As IntPtr) As Boolean
        If AdobeNativeMethods.GetAncestor(hwnd, AdobeNativeMethods.GA_ROOT) <> hwnd Then Return False
        Return IsAdobePid(AdobeNativeMethods.OwnerPid(hwnd))
    End Function

    ' A process of Adobe's (Acrobat / AcroRd32). Cached per pid.
    Private Function IsAdobePid(k_pid As Integer) As Boolean
        If k_pid <= 0 OrElse k_pid = Environment.ProcessId Then Return False
        Dim k_known As Boolean
        If _pidIsAdobe.TryGetValue(k_pid, k_known) Then Return k_known
        Dim k_isAdobe As Boolean = False
        Try
            Using k_proc As Diagnostics.Process = Diagnostics.Process.GetProcessById(k_pid)
                For Each k_name As String In AdobeProcessNames
                    If String.Equals(k_proc.ProcessName, k_name, StringComparison.OrdinalIgnoreCase) Then k_isAdobe = True
                Next
            End Using
        Catch ex As ArgumentException
            ' The process is already gone: not one to watch.
        Catch ex As InvalidOperationException
            ' Exited while being read: same.
        End Try
        _pidIsAdobe(k_pid) = k_isAdobe
        Return k_isAdobe
    End Function

End Class
