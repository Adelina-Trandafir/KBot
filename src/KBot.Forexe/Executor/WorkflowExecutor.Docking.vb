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
'  on FormClosing. Keyboard input needs the two input queues attached, which is
'  what AttachBrowserInput does.
'
'  While docked the window moving helpers are disabled: HideBrowserWindowAsync,
'  ShowBrowserWindowAsync and ExecuteMinimizeAsync all return early, otherwise a
'  workflow could throw the browser off screen while the operator is recording.
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
    Private Const DockWsChild As Integer = &H40000000
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

    Private Const DockSwShow As Integer = 5

    ' --- PInvoke used only by docking ---
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetParent(hWndChild As IntPtr, hWndNewParent As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetParent(hWnd As IntPtr) As IntPtr
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

    ' The window exactly as it was before docking, so undocking can hand it back unchanged.
    Private _preDockStyle As Integer = 0
    Private _preDockExStyle As Integer = 0
    Private _preDockParent As IntPtr = IntPtr.Zero
    Private _preDockBounds As RECT

    ' Input queue plumbing. The UI thread id is captured on the UI thread at dock time.
    Private _dockUiThreadId As UInteger = 0
    Private _inputAttachedTo As UInteger = 0

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
        _dockUiThreadId = GetCurrentThreadId()

        Dim hwnd As IntPtr = Await GetOrRefreshBrowserHwndAsync()
        If hwnd = IntPtr.Zero Then Throw New InvalidOperationException(
            "Nu am găsit fereastra browserului. Andocarea nu poate continua.")

        _preDockStyle = GetWindowLong(hwnd, DockGwlStyle)
        _preDockExStyle = GetWindowLong(hwnd, GWL_EXSTYLE)
        _preDockParent = GetParent(hwnd)
        GetWindowRect(hwnd, _preDockBounds)

        ' Out of any maximized or minimized state first: a maximized window keeps fighting
        ' the size it is about to be given.
        ShowWindow(hwnd, SW_RESTORE)

        ' A frame window becomes a plain child: no caption, no resize grip, no system menu,
        ' and out of the taskbar - it is a control now, not a window of its own.
        Dim style As Integer = _preDockStyle
        style = style And Not (DockWsCaption Or DockWsThickFrame Or
                               DockWsMinimizeBox Or DockWsMaximizeBox Or DockWsSysMenu)
        style = style And Not DockWsPopup
        style = style Or DockWsChild Or DockWsVisible
        SetWindowLong(hwnd, DockGwlStyle, style)

        Dim exStyle As Integer = (_preDockExStyle And Not WS_EX_APPWINDOW) And Not WS_EX_TOOLWINDOW
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle)

        ' SetParent returns the PREVIOUS parent, which is null for a top level window and
        ' also null on failure - so its result says nothing. Ask who the parent is now.
        SetParent(hwnd, hostHandle)
        If GetParent(hwnd) <> hostHandle Then
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

        Await SyncDockedBoundsAsync()

        ' Playwright draws the page into an EMULATED viewport (1280x720 unless the context
        ' asked for another one) and that viewport does NOT follow the window. Docked, the
        ' window is resized to the host panel, so without this the operator would keep
        ' seeing a page of the old size letterboxed inside the panel.
        Await ClearEmulatedViewportAsync()

        _logger.LogInfo("[Andocare] Browserul este copilul panoului gazdă.")
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

    ''' <summary>Hands the window back: top level again, with the styles and bounds it had.</summary>
    Public Function UndockBrowserAsync() As Task
        If Not _isDocked Then Return Task.CompletedTask

        Dim hwnd As IntPtr = _dockedHwnd
        _isDocked = False
        _dockHost = Nothing
        _dockedHwnd = IntPtr.Zero
        _chromeBandPx = 0
        _chromeMeasureWarned = False
        _chromeBandLogged = 0

        If hwnd = IntPtr.Zero OrElse Not IsWindow(hwnd) Then
            _logger.LogWarning("[Andocare] Fereastra browserului nu a fost găsită la detașare.")
            Return Task.CompletedTask
        End If

        AttachBrowserInput(hwnd, False)

        SetParent(hwnd, _preDockParent)
        SetWindowLong(hwnd, DockGwlStyle, _preDockStyle)
        SetWindowLong(hwnd, GWL_EXSTYLE, _preDockExStyle Or WS_EX_APPWINDOW)

        ' The pre-dock rectangle, unless it was never usable - a browser docked straight
        ' after launch may have been parked off screen by stealth mode.
        Dim left As Integer = _preDockBounds.Left
        Dim top As Integer = _preDockBounds.Top
        Dim width As Integer = _preDockBounds.Width
        Dim height As Integer = _preDockBounds.Height
        If width < 300 OrElse height < 200 OrElse left < 0 OrElse top < 0 Then
            left = 100
            top = 100
            width = 1400
            height = 900
        End If

        SetWindowPos(hwnd, IntPtr.Zero, left, top, width, height,
                     DockSwpFrameChanged Or DockSwpNoZOrder Or DockSwpShowWindow)
        ShowWindow(hwnd, DockSwShow)

        _isBrowserVisible = True
        _logger.LogInfo("[Andocare] Browser detașat.")
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

    ''' <summary>
    ''' Puts the browser window immediately above <paramref name="owner"/> in the z-order
    ''' without activating it. Only for the UNDOCKED browser: clicking the host form
    ''' activates it, and without this the browser the operator is driving would drop behind
    ''' the very window that is supposed to sit next to it. Docked there is nothing to do -
    ''' a child window cannot fall behind its own parent.
    ''' </summary>
    Public Async Function RaiseBrowserAboveAsync(owner As IWin32Window) As Task
        If owner Is Nothing Then Throw New ArgumentNullException(NameOf(owner))
        If _isDocked Then Return
        If _page Is Nothing OrElse _page.IsClosed Then Return

        ' Read on the CALLING thread: after the await the continuation may not be on the UI
        ' thread any more, and Control.Handle is not free to touch from anywhere.
        Dim ownerHandle As IntPtr = owner.Handle
        If ownerHandle = IntPtr.Zero Then Return

        Dim hwnd As IntPtr = Await GetOrRefreshBrowserHwndAsync()
        If hwnd = IntPtr.Zero Then Return

        SetWindowPos(hwnd, ownerHandle, 0, 0, 0, 0,
                     DockSwpNoMove Or DockSwpNoSize Or DockSwpNoActivate)
    End Function

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
    '  Host reads, marshalled to the UI thread
    ' =========================================================================
    Private Shared Function ReadHostHandle(host As Control) As IntPtr
        If host.InvokeRequired Then
            Return CType(host.Invoke(Function() ReadHostHandle(host)), IntPtr)
        End If
        If Not host.IsHandleCreated Then Return IntPtr.Zero
        Return host.Handle
    End Function

    Private Shared Function ReadHostClientSize(host As Control) As Drawing.Size
        If host.InvokeRequired Then
            Return CType(host.Invoke(Function() ReadHostClientSize(host)), Drawing.Size)
        End If
        Return host.ClientSize
    End Function

End Class
