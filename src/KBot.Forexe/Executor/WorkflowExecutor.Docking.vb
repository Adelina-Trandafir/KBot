Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports KBot.Common

' =============================================================================
'  Docking - the Chromium window becomes a child of a host panel.
'
'  This is REAL docking: SetParent reparents the browser frame window into the
'  panel and the frame decorations are stripped, so from that moment the browser
'  behaves like any other control on the form - it moves with the form, clips to
'  the panel, and can no longer fall behind it. There is no z-order to maintain
'  and no coordinate conversion: a child window is positioned in its parent's
'  CLIENT pixels, which is exactly what WinForms reports.
'
'  The earlier "false" docking - window left top level, kept over the panel with
'  CDP Browser.setWindowBounds plus a SetWindowPos above the owner - is gone. It
'  depended on two things that kept breaking: finding the window by a title that
'  navigation had already replaced, and winning a z-order race against every
'  activation of the host form.
'
'  Cost of reparenting, stated out loud: the browser window now belongs to the
'  panel. Disposing the panel or closing the host form without undocking first
'  destroys the Chromium window and with it the session, so every host must undock
'  on FormClosing.
'
'  THE FRAME IS REPARENTED WITHOUT WS_CHILD - a top level window with a parent, the
'  shape every "put notepad in a panel" sample uses - and that is what makes the KEYBOARD
'  work. Found on screen (slice 0070): docked as a real WS_CHILD the page took the mouse
'  but not one typed character. Windows never activates a child window, it activates the
'  top level ancestor - our form - and Chromium routes typed characters through its input
'  method only while its own window is the active one (WM_ACTIVATE). That message cannot be
'  faked either: Chromium throws away a WM_ACTIVATE addressed to a window that has WS_CHILD.
'  Without WS_CHILD the window still hangs off the panel - positioned in the panel's client
'  pixels, clipped by it, moved with the form - but a click on the page activates the
'  browser window itself, exactly as if it stood alone, and typing lands in the page. The
'  visible side effect is honest: while the operator types in the page the host form paints
'  its caption as inactive, the way any form does when a different window is active.
'
'  Two consequences of that shape. GetParent is useless for a window that is not WS_CHILD
'  (it reports the OWNER, which is nobody), so the parent is read with GetAncestor. And
'  activating a window does not raise its top level ancestor, so the host form is raised by
'  hand when the browser becomes the active window (see OnHostFormDeactivate). The input
'  queues are still attached (AttachBrowserInput): SetFocus and the shared focus state
'  behave better with them, and it costs nothing.
'
'  THE BROWSER IS NEVER A WINDOW OF ITS OWN (slice 0070). It has exactly two states:
'  docked inside a K-BOT form, or hidden off screen. A free standing Chromium window
'  has a close button, and one click on it ends the FOREXE session together with every
'  job that was going to run on it. So undocking does not hand the window back to the
'  desktop: UndockBrowserAsync puts it straight into the same stealth state the launch
'  uses (off screen, out of the taskbar), and the only way to see the page again is to
'  dock it into a form. Whoever docked it must undock on FormClosing, and that undock
'  hides it - closing the host form is the same gesture as «Ascunde browserul».
'
'  While docked ExecuteMinimizeAsync returns early, otherwise a workflow could throw the
'  browser off screen while the operator is looking at it. HideBrowserWindowAsync does
'  the opposite: docked, it undocks - which is the hide.
'
'  Hiding the browser's own toolbar (tab strip, address bar, bookmarks) is done by
'  PLACEMENT, not by a Chromium flag. The window is given a negative top - the toolbar
'  band sits above the host panel's top edge - and a height larger by the same amount,
'  so the page area lands exactly on the panel. A child window is clipped by its parent,
'  so the band is neither drawn nor clickable. Nothing is stripped from the browser: it
'  is the same window, and undocking or HideChromeWhenDocked = False shows it again.
'
'  The band is MEASURED, never assumed: the page is drawn in a child window of class
'  Chrome_RenderWidgetHostHWND, and the distance from the frame to that child is the
'  toolbar height whatever the DPI, the theme or a visible bookmarks bar make it.
'
'  Only the TOP is compensated. The same measurement also reports a gap on the left, the
'  right and the bottom, and compensating those cost the operator the page SCROLLBARS: the
'  render widget window does not cover them, so widening the window by that gap pushed both
'  scrollbars past the edges of the panel. Any real frame border left over on the sides is a
'  couple of dead pixels; a missing scrollbar is a page that cannot be read.
' =============================================================================
Partial Public Class WorkflowExecutor

    ' --- Window styles and flags used by docking. The Dock prefix keeps them apart from
    '     locals of the same meaning inside WorkflowExecutor.Browser.vb. ---
    Private Const DockGwlStyle As Integer = -16
    Private Const DockWsChild As Integer = &H40000000           ' never SET - see the header
    Private Const DockWsPopup As Integer = Integer.MinValue     ' 0x80000000
    Private Const DockWsVisible As Integer = &H10000000
    Private Const DockWsCaption As Integer = &HC00000           ' WS_BORDER | WS_DLGFRAME
    Private Const DockWsThickFrame As Integer = &H40000
    Private Const DockWsMinimizeBox As Integer = &H20000
    Private Const DockWsMaximizeBox As Integer = &H10000
    Private Const DockWsSysMenu As Integer = &H80000

    Private Const DockSwpNoSize As UInteger = &H1
    Private Const DockSwpNoMove As UInteger = &H2
    Private Const DockSwpNoZOrder As UInteger = &H4
    Private Const DockSwpNoActivate As UInteger = &H10
    Private Const DockSwpFrameChanged As UInteger = &H20
    Private Const DockSwpShowWindow As UInteger = &H40

    ''' <summary>GetAncestor flag: the real parent from the window tree, owner excluded.</summary>
    Private Const GaParent As UInteger = 1

    ''' <summary>SetWindowPos insert-after: top of the z-order (below topmost windows).</summary>
    Private Shared ReadOnly HwndTop As IntPtr = IntPtr.Zero

    ' --- PInvoke used only by docking ---
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetParent(hWndChild As IntPtr, hWndNewParent As IntPtr) As IntPtr
    End Function

    ' GetParent is NOT used on purpose: for a window without WS_CHILD it answers with the
    ' owner, not the parent, and the docked browser has no owner. GetAncestor(GA_PARENT)
    ' reads the parent out of the window tree whatever the style says.
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetAncestor(hWnd As IntPtr, gaFlags As UInteger) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetDesktopWindow() As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function MoveWindow(hWnd As IntPtr, x As Integer, y As Integer,
                                       nWidth As Integer, nHeight As Integer,
                                       bRepaint As Boolean) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowRect(hWnd As IntPtr, ByRef lpRect As RECT) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowThreadProcessId(hWnd As IntPtr, ByRef lpdwProcessId As UInteger) As UInteger
    End Function

    <DllImport("user32.dll")>
    Private Shared Function AttachThreadInput(idAttach As UInteger, idAttachTo As UInteger,
                                              fAttach As Boolean) As Boolean
    End Function

    <DllImport("kernel32.dll")>
    Private Shared Function GetCurrentThreadId() As UInteger
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function EnumChildWindows(hWndParent As IntPtr, callback As EnumWindowsProc,
                                             lParam As IntPtr) As Boolean
    End Function

    ''' <summary>Class name of the Chromium child window the page itself is drawn into.</summary>
    Private Const ChromeRenderWidgetClass As String = "Chrome_RenderWidgetHostHWND"

    ''' <summary>Largest toolbar band still believed to be real, in device pixels.</summary>
    Private Const MaxChromeBandPx As Integer = 400

    ' --- Docking state ---
    Private _isDocked As Boolean = False
    Private _dockHost As Control = Nothing
    Private _dockedHwnd As IntPtr = IntPtr.Zero

    ' The window styles exactly as they were before docking, so undocking can give the
    ' frame back. The bounds are NOT kept: an undocked browser goes off screen, never back
    ' to where it was.
    Private _preDockStyle As Integer = 0
    Private _preDockExStyle As Integer = 0
    Private _preDockParent As IntPtr = IntPtr.Zero

    ''' <summary>
    ''' Raised after the browser was docked into a host (True) or undocked and hidden (False).
    ''' Raised on whatever thread finished the operation - marshal before touching UI. Lets a
    ''' host form refresh its buttons when somebody ELSE undocked the browser, for instance
    ''' «Ascunde browserul» from the console while the recorder is open.
    ''' </summary>
    Public Event OnDockStateChanged(docked As Boolean)

    ' Input queue plumbing. The UI thread id is captured on the UI thread at dock time.
    Private _dockUiThreadId As UInteger = 0
    Private _inputAttachedTo As UInteger = 0

    ' The form the host panel sits on, held only to hear its Deactivate while docked - see
    ' OnHostFormDeactivate. Cleared on undock.
    Private _hostForm As Form = Nothing

    ' Toolbar hiding. _chromeBandPx is the last measurement that made sense, kept so a single
    ' failed measurement does not make the toolbar flash back into the panel.
    Private _hideChromeWhenDocked As Boolean = True
    Private _chromeBandPx As Integer = 0
    Private _chromeMeasureWarned As Boolean = False
    Private _chromeBandLogged As Integer = 0

    ''' <summary>True while the browser window is a child of a host panel.</summary>
    Public ReadOnly Property IsDocked As Boolean
        Get
            Return _isDocked
        End Get
    End Property

    ''' <summary>
    ''' The panel the browser is docked into right now, or Nothing. Two hosts exist since
    ''' slice 0074 (the recorder's panel and the shell's «Browser» view), and each needs to
    ''' know whether the browser is ITS before it resyncs, releases or takes it over.
    ''' </summary>
    Public ReadOnly Property DockHost As Control
        Get
            Return If(_isDocked, _dockHost, Nothing)
        End Get
    End Property

    ''' <summary>
    ''' While docked, keeps the browser's own toolbar - tab strip, address bar, bookmarks -
    ''' outside the host panel, so the operator sees only the page. On by default. Setting it
    ''' takes effect at once when the browser is already docked.
    ''' </summary>
    Public Property HideChromeWhenDocked As Boolean
        Get
            Return _hideChromeWhenDocked
        End Get
        Set(value As Boolean)
            If _hideChromeWhenDocked = value Then Return
            _hideChromeWhenDocked = value
            If Not _isDocked Then Return
            Try
                ApplyDockedBounds()
            Catch ex As Exception
                GlobalErrorLog.Write("WorkflowExecutor.HideChromeWhenDocked", ex)
                _logger.LogWarning("[Andocare] Bara browserului nu a putut fi comutată: " & ex.Message)
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Makes the browser window a child of <paramref name="host"/>. Fails fast: a half
    ''' docked browser is worse than a browser that never docked, so anything that goes
    ''' wrong puts the window styles back before throwing.
    ''' </summary>
    Public Async Function DockBrowserToAsync(host As Control) As Task
        If host Is Nothing Then Throw New ArgumentNullException(NameOf(host))
        If host.IsDisposed Then Throw New InvalidOperationException(
            "Panoul gazdă a fost deja eliberat. Nu pot andoca browserul.")
        If _page Is Nothing OrElse _page.IsClosed Then Throw New InvalidOperationException(
            "Browserul nu este pornit. Pornește sesiunea înainte de andocare.")
        If _isDocked Then Return

        ' Everything that must be read on the UI thread is read HERE, before the first
        ' Await: after it the continuation may be on any thread, and Control.Handle is
        ' not free to touch from just anywhere.
        Dim hostHandle As IntPtr = ReadHostHandle(host)
        If hostHandle = IntPtr.Zero Then Throw New InvalidOperationException(
            "Panoul gazdă nu are încă handle. Afișează formularul înainte de andocare.")
        Dim hostForm As Form = ReadHostForm(host)
        _dockUiThreadId = GetCurrentThreadId()

        Dim hwnd As IntPtr = Await GetOrRefreshBrowserHwndAsync()
        If hwnd = IntPtr.Zero Then Throw New InvalidOperationException(
            "Nu am găsit fereastra browserului. Andocarea nu poate continua.")

        _preDockStyle = GetWindowLong(hwnd, DockGwlStyle)
        _preDockExStyle = GetWindowLong(hwnd, GWL_EXSTYLE)
        _preDockParent = ParentOf(hwnd)

        ' Out of any maximized or minimized state first: a maximized window keeps fighting
        ' the size it is about to be given.
        ShowWindow(hwnd, SW_RESTORE)

        ' The frame loses its decorations - no caption, no resize grip, no system menu - but
        ' it is NOT made a WS_CHILD: a child window is never activated by Windows, and a
        ' Chromium window that is never activated drops every typed character (see the
        ' header). It stays a top level window in style, hung off the panel by SetParent.
        Dim style As Integer = _preDockStyle
        style = style And Not (DockWsCaption Or DockWsThickFrame Or
                               DockWsMinimizeBox Or DockWsMaximizeBox Or DockWsSysMenu)
        style = style And Not DockWsChild
        style = style Or DockWsVisible
        SetWindowLong(hwnd, DockGwlStyle, style)

        ' Tool window, not app window: a window that is top level in style could otherwise
        ' earn a taskbar button of its own while it sits inside the panel.
        Dim exStyle As Integer = (_preDockExStyle Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle)

        ' SetParent returns the PREVIOUS parent, which is null for a top level window and
        ' also null on failure - so its result says nothing. Ask who the parent is now.
        SetParent(hwnd, hostHandle)
        If ParentOf(hwnd) <> hostHandle Then
            Dim err As Integer = Marshal.GetLastWin32Error()
            SetWindowLong(hwnd, DockGwlStyle, _preDockStyle)
            SetWindowLong(hwnd, GWL_EXSTYLE, _preDockExStyle)
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                         DockSwpNoMove Or DockSwpNoSize Or DockSwpNoZOrder Or DockSwpFrameChanged)
            Throw New InvalidOperationException(
                $"Reparentarea ferestrei browserului a eșuat (cod {err}). Andocarea nu poate continua.")
        End If

        _dockedHwnd = hwnd
        _dockHost = host
        _isDocked = True
        _isBrowserVisible = True
        _chromeBandPx = 0
        _chromeMeasureWarned = False
        _chromeBandLogged = 0

        AttachBrowserInput(hwnd, True)
        ListenToHostForm(hostForm)

        Await SyncDockedBoundsAsync()

        ' Playwright draws the page into an EMULATED viewport (1280x720 unless the context
        ' asked for another one) and that viewport does NOT follow the window. Docked, the
        ' window is resized to the host panel, so without this the operator would keep
        ' seeing a page of the old size letterboxed inside the panel.
        Await ClearEmulatedViewportAsync()

        _logger.LogInfo("[Andocare] Browserul este copilul panoului gazdă.")
        RaiseEvent OnDockStateChanged(True)
    End Function

    ''' <summary>
    ''' Re-applies the host rectangle to the browser window. Called after every resize or
    ''' splitter drag of the host form. Not asynchronous in fact - a child window is moved
    ''' with one synchronous call - but the name and the Task keep every caller unchanged.
    ''' </summary>
    Public Function SyncDockedBoundsAsync() As Task
        ApplyDockedBounds()
        Return Task.CompletedTask
    End Function

    ''' <summary>
    ''' Gives the browser window the rectangle that puts its PAGE over the whole host panel.
    ''' With the toolbar hidden that rectangle starts above and left of the panel, by exactly
    ''' the measured toolbar and border widths, and is larger by the same amount.
    ''' </summary>
    Private Sub ApplyDockedBounds()
        If Not _isDocked Then Return

        Dim host As Control = _dockHost
        If host Is Nothing OrElse host.IsDisposed Then Throw New InvalidOperationException(
            "Panoul gazdă nu mai există. Detașează browserul înainte de a continua.")
        If _page Is Nothing OrElse _page.IsClosed Then Throw New InvalidOperationException(
            "Browserul s-a închis. Detașează-l înainte de a continua.")

        Dim hwnd As IntPtr = _dockedHwnd
        If hwnd = IntPtr.Zero OrElse Not IsWindow(hwnd) Then Throw New InvalidOperationException(
            "Fereastra browserului nu mai poate fi găsită.")

        Dim size As Drawing.Size = ReadHostClientSize(host)
        If size.Width < 40 OrElse size.Height < 40 Then Return

        ' Child window coordinates are the parent's CLIENT pixels - the same physical pixels
        ' WinForms reports - so there is no DPI conversion here and no CDP round trip.
        Dim band As Integer = CurrentChromeBand(hwnd)
        PlaceDockedWindow(hwnd, size, band)

        ' Chromium lays its toolbar out for the size the window has, so a measurement taken
        ' right after reparenting can still describe the window as it was. Measuring again
        ' once the new rectangle is in place either confirms it or corrects it.
        If _hideChromeWhenDocked Then
            Dim settled As Integer = CurrentChromeBand(hwnd)
            If settled <> band Then PlaceDockedWindow(hwnd, size, settled)
        End If
    End Sub

    ''' <summary>
    ''' Width stays the panel's width and the left edge stays at zero: the page keeps its own
    ''' right hand scrollbar inside the panel. Only the top is pulled up, by the toolbar band.
    ''' </summary>
    Private Shared Sub PlaceDockedWindow(hwnd As IntPtr, size As Drawing.Size, band As Integer)
        MoveWindow(hwnd, 0, -band, size.Width, size.Height + band, True)
    End Sub

    ''' <summary>
    ''' Height of the band the browser draws above the page right now - tab strip, address bar
    ''' and bookmarks together. Zero when the toolbar is meant to stay visible, or when no
    ''' measurement made sense.
    ''' </summary>
    Private Function CurrentChromeBand(hwnd As IntPtr) As Integer
        If Not _hideChromeWhenDocked Then Return 0

        Dim measured As Integer = MeasureChromeBandPx(hwnd)
        If measured > 0 Then
            _chromeBandPx = measured

            ' The figure itself, once per value: if the band is ever measured wrong, the log
            ' says by how much instead of leaving the operator to guess from the picture.
            If _chromeBandLogged <> measured Then
                _chromeBandLogged = measured
                _logger.LogInfo($"[Andocare] Bara browserului ascunsă: {measured} px.")
            End If
            Return measured
        End If

        ' A measurement can fail while Chromium is between layouts. The last good one is a
        ' far better answer than zero, which would drop the toolbar back into the panel.
        If _chromeBandPx > 0 Then Return _chromeBandPx

        ' Said once per docking: this runs on every resize, and a repeated line would bury
        ' the rest of the log under a splitter drag.
        If Not _chromeMeasureWarned Then
            _chromeMeasureWarned = True
            _logger.LogWarning(
                "[Andocare] Nu am putut măsura bara browserului: rămâne vizibilă în panou.")
        End If
        Return 0
    End Function

    ''' <summary>
    ''' Distance from the top of the frame window to the top of the child window the page is
    ''' drawn into. Returns zero when that child cannot be found or the number is not
    ''' believable - the caller must not move the window by a figure it does not trust.
    ''' </summary>
    Private Shared Function MeasureChromeBandPx(hwnd As IntPtr) As Integer
        Dim page As IntPtr = FindLargestRenderWidget(hwnd)
        If page = IntPtr.Zero Then Return 0

        Dim frameRect As RECT
        Dim pageRect As RECT
        If Not GetWindowRect(hwnd, frameRect) Then Return 0
        If Not GetWindowRect(page, pageRect) Then Return 0

        Dim band As Integer = pageRect.Top - frameRect.Top
        If band < 1 OrElse band > MaxChromeBandPx Then Return 0
        Return band
    End Function

    ''' <summary>
    ''' The render widget of the page. Chromium can keep several - a dropdown, a tab about to
    ''' close - so the biggest one is taken: that is the tab the operator is looking at.
    ''' </summary>
    Private Shared Function FindLargestRenderWidget(parent As IntPtr) As IntPtr
        Dim best As IntPtr = IntPtr.Zero
        Dim bestArea As Long = 0
        Dim classBuf As New System.Text.StringBuilder(256)

        EnumChildWindows(parent,
            Function(child As IntPtr, lParam As IntPtr) As Boolean
                classBuf.Clear()
                GetClassName(child, classBuf, classBuf.Capacity)
                If Not String.Equals(classBuf.ToString(), ChromeRenderWidgetClass,
                                     StringComparison.Ordinal) Then Return True

                Dim r As RECT
                If Not GetWindowRect(child, r) Then Return True

                Dim area As Long = CLng(r.Width) * r.Height
                If area > bestArea Then
                    bestArea = area
                    best = child
                End If
                Return True
            End Function, IntPtr.Zero)

        Return best
    End Function

    ''' <summary>
    ''' Takes the window out of the host panel AND hides it: top level again, with the
    ''' frame styles it had, but parked off screen and out of the taskbar - the same
    ''' stealth state the launch puts it in. It is NOT shown on the desktop, ever: a free
    ''' Chromium window can be closed by the operator, and that closes the session.
    '''
    ''' <para>Completes synchronously - a child window is reparented and moved with a few
    ''' direct calls - so a host that is being disposed may call it without awaiting.</para>
    ''' </summary>
    Public Function UndockBrowserAsync() As Task
        If Not _isDocked Then Return Task.CompletedTask

        Dim hwnd As IntPtr = _dockedHwnd
        _isDocked = False
        _dockHost = Nothing
        _dockedHwnd = IntPtr.Zero
        _chromeBandPx = 0
        _chromeMeasureWarned = False
        _chromeBandLogged = 0
        ListenToHostForm(Nothing)

        If hwnd = IntPtr.Zero OrElse Not IsWindow(hwnd) Then
            _logger.LogWarning("[Andocare] Fereastra browserului nu a fost găsită la detașare.")
            _isBrowserVisible = False
            RaiseEvent OnDockStateChanged(False)
            Return Task.CompletedTask
        End If

        AttachBrowserInput(hwnd, False)

        ' Back to the desktop (a null parent) and the styles it had. Never WS_CHILD in
        ' either direction, so the order of the two calls does not matter.
        SetParent(hwnd, _preDockParent)
        SetWindowLong(hwnd, DockGwlStyle, _preDockStyle)

        ' Stealth, not the pre-dock ex-style: whatever the window was before docking, after
        ' undocking it is a hidden window - tool window (no taskbar button, no Alt-Tab entry).
        SetWindowLong(hwnd, GWL_EXSTYLE, (_preDockExStyle Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW)

        ' Off screen, the same parking spot HideBrowserWindowAsync uses. Still SHOWN in the
        ' Win32 sense (SWP_SHOWWINDOW, never SW_HIDE): a hidden window cannot host the
        ' certificate picker, see ApplyStealthWindowStyle.
        SetWindowPos(hwnd, IntPtr.Zero, StealthLeft, StealthTop, StealthWidth, StealthHeight,
                     DockSwpFrameChanged Or DockSwpNoZOrder Or DockSwpNoActivate Or DockSwpShowWindow)

        _isBrowserVisible = False
        _logger.LogInfo("[Andocare] Browser detașat și ascuns.")
        RaiseEvent OnDockStateChanged(False)
        Return Task.CompletedTask
    End Function

    ''' <summary>
    ''' Drops the emulated viewport so the page follows the real window size. A LAST RESORT,
    ''' and normally a no-op: the context is created with ViewportSize.NoViewport (see
    ''' LaunchAndPositionBrowserAsync), so there is nothing to clear. It stays for a session
    ''' someone else built with an emulated viewport, and it only half works there - Playwright
    ''' pushes its own override back on, so the fix belongs at context creation, not here.
    '''
    ''' <para>The override is deliberately NOT put back on undock: a page that follows its
    ''' own window is right in both states, while restoring the fixed size would letterbox
    ''' the undocked window instead.</para>
    ''' </summary>
    Private Async Function ClearEmulatedViewportAsync() As Task
        If _page Is Nothing OrElse _page.IsClosed Then Return
        If _page.ViewportSize Is Nothing Then Return   ' already following the window

        Try
            Dim cdp = Await _page.Context.NewCDPSessionAsync(_page)
            Await cdp.SendAsync("Emulation.clearDeviceMetricsOverride")
            _logger.LogInfo("[Andocare] Viewport emulat eliminat: pagina urmează fereastra.")
        Catch ex As Exception
            ' Cosmetic: a page still drawn at the old size is ugly, not a reason to fail the
            ' docking the operator just asked for. Said out loud in the log, not swallowed.
            _logger.LogWarning("[Andocare] Viewport-ul emulat nu a putut fi eliminat: " & ex.Message)
        End Try
    End Function

    ' RaiseBrowserAboveAsync (keeping a free browser above the recorder in the z-order) is
    ' gone with slice 0070: there is no free browser to keep in place any more.

    ' =========================================================================
    '  Input queues
    ' =========================================================================

    ''' <summary>
    ''' Attaches this thread's input queue to the browser window's thread, or detaches it.
    ''' Without it a reparented Chromium child takes the mouse but not the keyboard: focus
    ''' follows the foreground window, which belongs to the host form's thread, so typing
    ''' would land in the form instead of the page.
    '''
    ''' <para>Best effort. Awkward typing is not a reason to refuse the docking the operator
    ''' asked for, so a failure is logged and the docking continues.</para>
    ''' </summary>
    Private Sub AttachBrowserInput(hwnd As IntPtr, attach As Boolean)
        Try
            If _dockUiThreadId = 0 Then Return

            Dim pid As UInteger = 0
            Dim browserThread As UInteger = GetWindowThreadProcessId(hwnd, pid)
            If browserThread = 0 OrElse browserThread = _dockUiThreadId Then Return

            If attach Then
                If _inputAttachedTo <> 0 Then Return
                If AttachThreadInput(_dockUiThreadId, browserThread, True) Then
                    _inputAttachedTo = browserThread
                Else
                    _logger.LogWarning(
                        "[Andocare] Firele de input nu au putut fi legate: tastarea în pagină poate fi capricioasă.")
                End If
            Else
                If _inputAttachedTo = 0 Then Return
                AttachThreadInput(_dockUiThreadId, _inputAttachedTo, False)
                _inputAttachedTo = 0
            End If
        Catch ex As Exception
            _logger.LogWarning("[Andocare] Eroare la legarea firelor de input: " & ex.Message)
        End Try
    End Sub

    ' =========================================================================
    '  Keeping the host form in front of the browser it hosts
    ' =========================================================================

    ''' <summary>
    ''' Subscribes to (or, with Nothing, unsubscribes from) the host form's Deactivate.
    ''' Best effort: a host without a form - the dev harness bench inside another
    ''' container, a test - simply does not get raised.
    ''' </summary>
    Private Sub ListenToHostForm(form As Form)
        Try
            If _hostForm IsNot Nothing Then
                RemoveHandler _hostForm.Deactivate, AddressOf OnHostFormDeactivate
                _hostForm = Nothing
            End If
            If form Is Nothing OrElse form.IsDisposed Then Return
            AddHandler form.Deactivate, AddressOf OnHostFormDeactivate
            _hostForm = form
        Catch ex As Exception
            _logger.LogWarning("[Andocare] Nu pot urmări activarea formularului gazdă: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' A click on the page activates the browser window, not the form around it, and
    ''' activating a window does not raise its top level ancestor. So when the form loses
    ''' activation TO ITS OWN BROWSER it raises itself, without taking the activation back -
    ''' otherwise a form half covered by another program would stay half covered while the
    ''' operator types in the part they can see. Any other loss of activation (the operator
    ''' went to another program) is left alone.
    ''' </summary>
    Private Sub OnHostFormDeactivate(sender As Object, e As EventArgs)
        Try
            If Not _isDocked OrElse _hostForm Is Nothing OrElse _hostForm.IsDisposed Then Return
            Dim active As IntPtr = GetForegroundWindow()
            If active = IntPtr.Zero OrElse active <> _dockedHwnd Then Return
            SetWindowPos(_hostForm.Handle, HwndTop, 0, 0, 0, 0,
                         DockSwpNoMove Or DockSwpNoSize Or DockSwpNoActivate)
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.OnHostFormDeactivate", ex)
        End Try
    End Sub

    ' =========================================================================
    '  Host reads, marshalled to the UI thread
    ' =========================================================================

    ''' <summary>
    ''' The parent from the window tree. Null for a top level window - GetAncestor answers
    ''' with the desktop window for those, which nobody wants to SetParent back to.
    ''' </summary>
    Private Shared Function ParentOf(hwnd As IntPtr) As IntPtr
        Dim parent As IntPtr = GetAncestor(hwnd, GaParent)
        If parent = GetDesktopWindow() Then Return IntPtr.Zero
        Return parent
    End Function

    Private Shared Function ReadHostHandle(host As Control) As IntPtr
        If host.InvokeRequired Then
            Return CType(host.Invoke(Function() ReadHostHandle(host)), IntPtr)
        End If
        If Not host.IsHandleCreated Then Return IntPtr.Zero
        Return host.Handle
    End Function

    Private Shared Function ReadHostForm(host As Control) As Form
        If host.InvokeRequired Then
            Return CType(host.Invoke(Function() ReadHostForm(host)), Form)
        End If
        Return host.FindForm()
    End Function

    Private Shared Function ReadHostClientSize(host As Control) As Drawing.Size
        If host.InvokeRequired Then
            Return CType(host.Invoke(Function() ReadHostClientSize(host)), Drawing.Size)
        End If
        Return host.ClientSize
    End Function

End Class
