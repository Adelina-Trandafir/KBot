Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

' Read Mode (Ctrl+H), the fix under test, NOT check-only (slice 0078-15, operator 07.10.2026).
'
' Sent as soon as Adobe's page view («AVPageView») is visible WITH A SIZE -- not after the page has settled (that takes
' long). No timer: every window event inside the control, every foreground change and our form becoming enabled again
' re-check, while it is pending. It goes out only when:
'  1. the page view is visible and has a size;
'  2. our form is enabled (no modal box up) and holds the foreground -- SendInput types into whatever window has the
'     keyboard;
'  3. the keyboard focus is then really inside the control (SetFocus on the page view first) -- Ctrl+H in a K-BOT text
'     box would be a backspace.
' A condition that fails leaves it pending (the reason is written to the check log once) for the next event.
'
' VERIFIED, NOT ASSUMED (operator 07.10.2026). Measured at 10:18: with Adobe already running, Ctrl+H at +922 ms took
' effect ~220 ms later; with Adobe starting from zero, Ctrl+H at +2087 ms did nothing -- Adobe was still building its
' interface (its top bar got a height only at +3427 ms). Read Mode ON is seen in the window tree: Adobe HIDES its
' right-hand pane («AVTaskPaneHostView») AND its left tab strip («AVDockableTabStripView»). So:
'  - before every send: already on -> nothing is sent (Ctrl+H toggles; a reused control may keep it);
'  - after a send, every event re-reads the tree: on -> done;
'  - still off at least RetryAfterMs after the send AND Adobe's top bar («AVDocumentHeaderView») has a height (its
'    interface is built) -> sent once more; after MaxSends it gives up and tells the operator to press Ctrl+H.
' Limit: an Adobe whose right pane and tab strip are both hidden by the operator's own settings reads as «on».
' It lives on the watch hooks for now and moves with the hook it needs when the check code goes.
Partial Public NotInheritable Class AcroPdfViewer

    Private Const VK_H As UShort = &H48US
    Private Const MaxReadModeSends As Integer = 2
    Private Const RetryAfterMs As Long = 700
    Private Const TaskPaneTitle As String = "AVTaskPaneHostView"
    Private Const TabStripTitle As String = "AVDockableTabStripView"
    Private Const HeaderTitle As String = "AVDocumentHeaderView"

    Private _readModePending As Boolean
    Private _readModePosted As Boolean
    Private _readModeWait As String
    Private _readModeSends As Integer
    Private ReadOnly _readModeClock As New Diagnostics.Stopwatch()

    ''' <summary>What of Adobe's layout Read Mode looks at, read in one walk of the control's windows.</summary>
    Private NotInheritable Class AdobeLayout
        Public Property Page As IntPtr
        Public Property TaskPane As IntPtr
        Public Property TabStrip As IntPtr
        Public Property HeaderHeight As Integer

        ''' <summary>
        ''' Both the right pane and the left tab strip exist and are hidden THEMSELVES (their own visible bit: a viewer that
        ''' is off screen hides them through its ancestors, which is not Read Mode).
        ''' </summary>
        Public ReadOnly Property ReadModeOn As Boolean
            Get
                Return TaskPane <> IntPtr.Zero AndAlso TabStrip <> IntPtr.Zero AndAlso
                       Not AdobeNativeMethods.IsVisibleStyleSet(TaskPane) AndAlso Not AdobeNativeMethods.IsVisibleStyleSet(TabStrip)
            End Get
        End Property
    End Class

    ' Called by ShowDocument before every load.
    Private Sub ArmReadMode()
        _readModePending = True
        _readModePosted = False
        _readModeWait = Nothing
        _readModeSends = 0
        _readModeClock.Reset()
    End Sub

    ' Called from the WinEvent callback (wrapped there). Only flags; the attempt is posted, so it runs after the callback
    ' has returned.
    Private Sub ReadModeOnEvent()
        If Not _readModePending OrElse _readModePosted OrElse Not _seenPageView Then Return
        If _host Is Nothing OrElse Not _host.IsHandleCreated OrElse Not _panel.IsHandleCreated Then Return
        _readModePosted = True
        _panel.BeginInvoke(New MethodInvoker(AddressOf TrySendReadMode))
    End Sub

    ' Posted from the WinEvent callback: log and swallow.
    Private Sub TrySendReadMode()
        Try
            _readModePosted = False
            If Not _readModePending OrElse _host Is Nothing OrElse Not _host.IsHandleCreated Then Return
            Dim k_layout As AdobeLayout = ReadLayout()

            If k_layout.ReadModeOn Then
                _readModePending = False
                Check($"+{Elapsed()} ms >>>>> CTRL+H: read mode is ON (right pane and tab strip hidden) after {_readModeSends} send(s)" &   ' ACTIVEX-CHECK
                      If(_readModeSends > 0, $", {_readModeClock.ElapsedMilliseconds} ms after the last one", "") & " <<<<<")
                If _readModeSends > 0 Then Report("AcroPDF: mod citire activ.")
                Return
            End If

            If _readModeSends > 0 Then
                If _readModeClock.ElapsedMilliseconds < RetryAfterMs Then Return   ' its effect may still be on the way
                If _readModeSends >= MaxReadModeSends Then
                    _readModePending = False
                    Check($"+{Elapsed()} ms >>>>> CTRL+H GIVEN UP: read mode still off {_readModeClock.ElapsedMilliseconds} ms after send {_readModeSends} <<<<<")   ' ACTIVEX-CHECK
                    Report("AcroPDF: ATENȚIE — modul de citire nu s-a activat. Apăsați Ctrl+H în document pentru a ascunde barele.")
                    Return
                End If
                If k_layout.HeaderHeight <= 0 Then
                    WaitReason("read mode still off; Adobe's top bar has no height yet (its interface is not built)")
                    Return
                End If
            End If

            Dim k_wait As String = ReadModeBlocker(k_layout)
            If k_wait IsNot Nothing Then
                WaitReason(k_wait)
                Return
            End If

            AdobeNativeMethods.SetFocus(k_layout.Page)
            Dim k_focus As IntPtr = AdobeNativeMethods.GetFocus()
            Dim k_inside As Boolean = k_focus = _host.Handle OrElse AdobeNativeMethods.IsChild(_host.Handle, k_focus)
            If Not k_inside Then
                WaitReason($"keyboard focus not inside the control after SetFocus (focus {Describe(k_focus)})")
                Return
            End If

            Dim k_keys As AdobeNativeMethods.INPUT() = {
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, False),
                AdobeNativeMethods.KeyInput(VK_H, False),
                AdobeNativeMethods.KeyInput(VK_H, True),
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, True)}
            Dim k_sent As UInteger = AdobeNativeMethods.SendInput(CUInt(k_keys.Length), k_keys,
                Runtime.InteropServices.Marshal.SizeOf(GetType(AdobeNativeMethods.INPUT)))
            Dim k_error As Integer = If(k_sent = k_keys.Length, 0, Runtime.InteropServices.Marshal.GetLastWin32Error())
            _readModeSends += 1
            _readModeClock.Restart()
            _readModeWait = Nothing
            Check($"+{Elapsed()} ms >>>>> CTRL+H sent ({_readModeSends}/{MaxReadModeSends}): {k_sent}/{k_keys.Length} key event(s)" &   ' ACTIVEX-CHECK
                  If(k_error <> 0, $", Win32 error {k_error}", "") &
                  $"; page view {HexOf(k_layout.Page)} {AdobeNativeMethods.RectInParent(k_layout.Page)}, " &
                  $"top bar height {k_layout.HeaderHeight}, focus {Describe(k_focus)} <<<<<")
            DumpTree($"right after Ctrl+H {_readModeSends}")                           ' ACTIVEX-CHECK
            If k_sent <> k_keys.Length Then
                _readModePending = False
                Report($"AcroPDF: ATENȚIE — Ctrl+H nu a putut fi trimis ({k_sent}/{k_keys.Length} taste, eroarea {k_error}).")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.TrySendReadMode", ex)
        End Try
    End Sub

    ' The reason it waits, in the check log once per change of reason.
    Private Sub WaitReason(k_why As String)
        If k_why = _readModeWait Then Return
        _readModeWait = k_why
        Check($"+{Elapsed()} ms CTRL+H waiting: {k_why}")                              ' ACTIVEX-CHECK
    End Sub

    ' One walk of the control's windows: the sized page view, the right pane, the tab strip, the top bar's height.
    Private Function ReadLayout() As AdobeLayout
        Dim k_layout As New AdobeLayout()
        For Each k_h As IntPtr In AdobeNativeMethods.Descendants(_host.Handle)
            Select Case AdobeNativeMethods.GetTitle(k_h)
                Case PageViewTitle
                    If k_layout.Page = IntPtr.Zero AndAlso AdobeNativeMethods.IsWindowVisible(k_h) Then
                        Dim k_rect As Rectangle = AdobeNativeMethods.RectInParent(k_h)
                        If k_rect.Width > 0 AndAlso k_rect.Height > 0 Then k_layout.Page = k_h
                    End If
                Case TaskPaneTitle
                    If k_layout.TaskPane = IntPtr.Zero Then k_layout.TaskPane = k_h
                Case TabStripTitle
                    If k_layout.TabStrip = IntPtr.Zero Then k_layout.TabStrip = k_h
                Case HeaderTitle
                    If AdobeNativeMethods.IsWindowVisible(k_h) Then
                        k_layout.HeaderHeight = Math.Max(k_layout.HeaderHeight, AdobeNativeMethods.RectInParent(k_h).Height)
                    End If
            End Select
        Next
        Return k_layout
    End Function

    ' Nothing when Ctrl+H may go now; otherwise why not.
    Private Function ReadModeBlocker(k_layout As AdobeLayout) As String
        If Not _host.Visible OrElse Not _panel.Visible Then Return "viewer not on screen"
        If k_layout.Page = IntPtr.Zero Then Return "page view not visible with a size"
        Dim k_form As Form = _panel.FindForm()
        If k_form Is Nothing OrElse Not k_form.IsHandleCreated Then Return "no form"
        If Not AdobeNativeMethods.IsWindowEnabled(k_form.Handle) Then Return "form disabled (a modal box is up)"
        Dim k_foreground As IntPtr = AdobeNativeMethods.GetForegroundWindow()
        If k_foreground <> k_form.Handle Then Return $"K-BOT is not the foreground window (foreground {Describe(k_foreground)})"
        Return Nothing
    End Function

End Class
