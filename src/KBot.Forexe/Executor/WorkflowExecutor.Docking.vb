Imports System.Windows.Forms

' =============================================================================
'  Docking - "false" docking of the Playwright browser window.
'
'  The browser window is NEVER reparented (no SetParent). Reparenting a Chromium
'  top level window breaks its input queue and its certificate dialogs, so the
'  window is only kept positioned over a host panel and just above the host form
'  in the z-order. Everything else about the window stays untouched.
'
'  Coordinate systems:
'    - WinForms reports screen coordinates in physical pixels (the app runs
'      PerMonitorV2, see KBot.App Program.Main).
'    - CDP Browser.setWindowBounds expects device independent pixels.
'    Conversion is therefore physical * 96 / hostDpi.
'
'  While docked, the window moving helpers are disabled: HideBrowserWindowAsync,
'  ShowBrowserWindowAsync and ExecuteMinimizeAsync all return early, otherwise a
'  workflow could throw the browser off screen while the operator is recording.
' =============================================================================
Partial Public Class WorkflowExecutor

    Private Const DockSwpNoSize As UInteger = &H1
    Private Const DockSwpNoMove As UInteger = &H2
    Private Const DockSwpNoActivate As UInteger = &H10

    Private _isDocked As Boolean = False
    Private _dockHost As Control = Nothing

    ''' <summary>True while the browser window is being kept over a host panel.</summary>
    Public ReadOnly Property IsDocked As Boolean
        Get
            Return _isDocked
        End Get
    End Property

    ''' <summary>
    ''' Starts keeping the browser window over <paramref name="host"/>. Fails fast:
    ''' a half docked browser is worse than a browser that never docked.
    ''' </summary>
    Public Async Function DockBrowserToAsync(host As Control) As Task
        If host Is Nothing Then Throw New ArgumentNullException(NameOf(host))
        If host.IsDisposed Then Throw New InvalidOperationException(
            "Panoul gazdă a fost deja eliberat. Nu pot andoca browserul.")
        If _page Is Nothing OrElse _page.IsClosed Then Throw New InvalidOperationException(
            "Browserul nu este pornit. Pornește sesiunea înainte de andocare.")

        Dim hwnd As IntPtr = Await GetOrRefreshBrowserHwndAsync()
        If hwnd = IntPtr.Zero Then Throw New InvalidOperationException(
            "Nu am găsit fereastra browserului. Andocarea nu poate continua.")

        ' Give the window back its normal application style: while stealth mode is on
        ' the browser is a tool window parked off screen, which cannot be shown.
        Dim exStyle As Integer = GetWindowLong(hwnd, GWL_EXSTYLE)
        exStyle = (exStyle And Not WS_EX_TOOLWINDOW) Or WS_EX_APPWINDOW
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle)
        ShowWindow(hwnd, SW_RESTORE)

        _dockHost = host
        _isDocked = True
        _isBrowserVisible = True

        Await SyncDockedBoundsAsync()
        _logger.LogInfo("[Andocare] Browserul urmărește panoul gazdă.")
    End Function

    ''' <summary>
    ''' Re-applies the host rectangle to the browser window. Called after every move,
    ''' resize, splitter drag or activation of the host form.
    ''' </summary>
    Public Async Function SyncDockedBoundsAsync() As Task
        If Not _isDocked Then Return

        Dim host As Control = _dockHost
        If host Is Nothing OrElse host.IsDisposed Then Throw New InvalidOperationException(
            "Panoul gazdă nu mai există. Detașează browserul înainte de a continua.")
        If _page Is Nothing OrElse _page.IsClosed Then Throw New InvalidOperationException(
            "Browserul s-a închis. Detașează-l înainte de a continua.")

        Dim hwnd As IntPtr = Await GetOrRefreshBrowserHwndAsync()
        If hwnd = IntPtr.Zero Then Throw New InvalidOperationException(
            "Fereastra browserului nu mai poate fi găsită.")

        Dim geometry As DockGeometry = ReadHostGeometry(host)
        If geometry.WidthPx < 80 OrElse geometry.HeightPx < 80 Then Return

        Dim scale As Double = 96.0R / geometry.Dpi
        Dim boundsLeft As Integer = CInt(Math.Round(geometry.LeftPx * scale))
        Dim boundsTop As Integer = CInt(Math.Round(geometry.TopPx * scale))
        Dim boundsWidth As Integer = CInt(Math.Round(geometry.WidthPx * scale))
        Dim boundsHeight As Integer = CInt(Math.Round(geometry.HeightPx * scale))

        Dim cdp = Await _page.Context.NewCDPSessionAsync(_page)
        Dim windowId As Integer = Await GetChromeWindowIdAsync()

        Dim param As New Dictionary(Of String, Object) From {
            {"windowId", windowId},
            {"bounds", CreateBounds(boundsLeft, boundsTop, boundsWidth, boundsHeight)}
        }
        Await cdp.SendAsync("Browser.setWindowBounds", param)

        ' Sit immediately above the host form without stealing the focus. The form is
        ' deliberately not TopMost: a TopMost form would cover the docked browser.
        If geometry.OwnerHandle <> IntPtr.Zero Then
            SetWindowPos(hwnd, geometry.OwnerHandle, 0, 0, 0, 0,
                         DockSwpNoMove Or DockSwpNoSize Or DockSwpNoActivate)
        End If
    End Function

    ''' <summary>Stops following the host panel and parks the browser as a normal window.</summary>
    Public Async Function UndockBrowserAsync() As Task
        If Not _isDocked Then Return

        _isDocked = False
        _dockHost = Nothing

        If _page Is Nothing OrElse _page.IsClosed Then Return

        Dim hwnd As IntPtr = Await GetOrRefreshBrowserHwndAsync()
        If hwnd = IntPtr.Zero Then
            _logger.LogWarning("[Andocare] Fereastra browserului nu a fost găsită la detașare.")
            Return
        End If

        Dim cdp = Await _page.Context.NewCDPSessionAsync(_page)
        Dim windowId As Integer = Await GetChromeWindowIdAsync()
        Dim param As New Dictionary(Of String, Object) From {
            {"windowId", windowId},
            {"bounds", CreateBounds(100, 100, 1400, 900)}
        }
        Await cdp.SendAsync("Browser.setWindowBounds", param)

        _isBrowserVisible = True
        _logger.LogInfo("[Andocare] Browser detașat.")
    End Function

    ' =========================================================================
    '  Host geometry, read on the UI thread
    ' =========================================================================
    Private Structure DockGeometry
        Public LeftPx As Integer
        Public TopPx As Integer
        Public WidthPx As Integer
        Public HeightPx As Integer
        Public Dpi As Integer
        Public OwnerHandle As IntPtr
    End Structure

    Private Shared Function ReadHostGeometry(host As Control) As DockGeometry
        If host.InvokeRequired Then
            Return CType(host.Invoke(Function() ReadHostGeometry(host)), DockGeometry)
        End If

        Dim topLeft As System.Drawing.Point = host.PointToScreen(System.Drawing.Point.Empty)
        Dim owner As Form = host.FindForm()

        Return New DockGeometry With {
            .LeftPx = topLeft.X,
            .TopPx = topLeft.Y,
            .WidthPx = host.ClientSize.Width,
            .HeightPx = host.ClientSize.Height,
            .Dpi = If(host.DeviceDpi > 0, host.DeviceDpi, 96),
            .OwnerHandle = If(owner Is Nothing OrElse Not owner.IsHandleCreated,
                              IntPtr.Zero, owner.Handle)
        }
    End Function

End Class
