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
' Ctrl+2 (page width): once Read Mode is on, when FitWidthAfterReadMode is True (a setting, operator 07.10.2026).
' Driven by either watch (AcroPdfViewer.LightWatch.vb, or the ACTIVEX-CHECK AcroPdfViewer.Watch.vb).
Partial Public NotInheritable Class AcroPdfViewer

    Private Const VK_H As UShort = &H48US
    Private Const VK_2 As UShort = &H32US
    Private Const MaxReadModeSends As Integer = 2
    Private Const RetryAfterMs As Long = 700
    Private Const TaskPaneTitle As String = "AVTaskPaneHostView"
    Private Const TabStripTitle As String = "AVDockableTabStripView"
    Private Const HeaderTitle As String = "AVDocumentHeaderView"

    Private _readModePending As Boolean
    Private _fitWidthPending As Boolean
    Private _readModePosted As Boolean
    Private _readModeWait As String
    Private _readModeSends As Integer
    Private ReadOnly _readModeClock As New Diagnostics.Stopwatch()

    ''' <summary>True = Ctrl+2 (page width) once Read Mode is on. Read when Read Mode is seen on.</summary>
    Public Property FitWidthAfterReadMode As Boolean

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
        _fitWidthPending = False
        _readModePosted = False
        _readModeWait = Nothing
        _readModeSends = 0
        _readModeClock.Reset()
    End Sub

    ' Called from the WinEvent callback of either watch (wrapped there). Only flags; the attempt is posted, so it runs
    ' after the callback has returned.
    Private Sub ReadModeOnEvent()
        If Not KeysPending() OrElse _readModePosted OrElse Not _seenPageView Then Return
        If _host Is Nothing OrElse Not _host.IsHandleCreated OrElse Not _panel.IsHandleCreated Then Return
        _readModePosted = True
        _panel.BeginInvoke(New MethodInvoker(AddressOf TrySendReadMode))
    End Sub

    ' Posted from the WinEvent callback: log and swallow.
    Private Sub TrySendReadMode()
        Try
            _readModePosted = False
            If Not KeysPending() OrElse _host Is Nothing OrElse Not _host.IsHandleCreated Then Return
            Dim k_layout As AdobeLayout = ReadLayout()

            ' Ctrl+S after a signature (AcroPdfViewer.SaveKeys.vb) does not wait for Read Mode.
            If _savePending Then SendSaveStep(k_layout)

            If _readModePending Then
                If Not k_layout.ReadModeOn Then
                    SendReadModeStep(k_layout)
                    If Not KeysPending() Then FixesDone()
                    Return
                End If
                _readModePending = False
                Check($"+{Elapsed()} ms >>>>> CTRL+H: read mode is ON (right pane and tab strip hidden) after {_readModeSends} send(s)" &   ' ACTIVEX-CHECK
                      If(_readModeSends > 0, $", {_readModeClock.ElapsedMilliseconds} ms after the last one", "") & " <<<<<")
                If _readModeSends > 0 Then Report("AcroPDF: mod citire activ.")
                ' Ctrl+2 (page width) after Read Mode, when the setting asks for it (operator, 07.10.2026).
                _fitWidthPending = FitWidthAfterReadMode
            End If

            If _fitWidthPending Then SendFitWidthStep(k_layout)
            If Not KeysPending() Then FixesDone()
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.TrySendReadMode", ex)
        End Try
    End Sub

    ' Something still to type into the document: Ctrl+H, Ctrl+2 or Ctrl+S.
    Private Function KeysPending() As Boolean
        Return _readModePending OrElse _fitWidthPending OrElse _savePending
    End Function

    ' Read Mode still off: wait, send Ctrl+H (again), or give up.
    Private Sub SendReadModeStep(k_layout As AdobeLayout)
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

        Dim k_focus As IntPtr = FocusForKeys(k_layout)
        If k_focus = IntPtr.Zero Then Return
        Dim k_error As Integer = 0
        Dim k_ok As Boolean = SendCtrlChord(VK_H, k_error)
        _readModeSends += 1
        _readModeClock.Restart()
        _readModeWait = Nothing
        Check($"+{Elapsed()} ms >>>>> CTRL+H sent ({_readModeSends}/{MaxReadModeSends}): ok={k_ok}" &   ' ACTIVEX-CHECK
              If(k_error <> 0, $", Win32 error {k_error}", "") &
              $"; page view {HexOf(k_layout.Page)} {AdobeNativeMethods.RectInParent(k_layout.Page)}, " &
              $"top bar height {k_layout.HeaderHeight}, focus {Describe(k_focus)} <<<<<")
        DumpTree($"right after Ctrl+H {_readModeSends}")                               ' ACTIVEX-CHECK
        If Not k_ok Then
            _readModePending = False
            Report($"AcroPDF: ATENȚIE — Ctrl+H nu a putut fi trimis (eroarea {k_error}).")
        End If
    End Sub

    ' Ctrl+2 once, under the same conditions as Ctrl+H; it is not a toggle, so it is not verified.
    Private Sub SendFitWidthStep(k_layout As AdobeLayout)
        Dim k_focus As IntPtr = FocusForKeys(k_layout)
        If k_focus = IntPtr.Zero Then Return
        Dim k_error As Integer = 0
        Dim k_ok As Boolean = SendCtrlChord(VK_2, k_error)
        _fitWidthPending = False
        Check($"+{Elapsed()} ms >>>>> CTRL+2 sent: ok={k_ok}" & If(k_error <> 0, $", Win32 error {k_error}", "") &   ' ACTIVEX-CHECK
              $"; focus {Describe(k_focus)} <<<<<")
        If Not k_ok Then Report($"AcroPDF: ATENȚIE — Ctrl+2 nu a putut fi trimis (eroarea {k_error}).")
    End Sub

    ' The conditions for typing into the document, then the keyboard focus on the page view. Returns the focus when it
    ' is really inside the control, otherwise Zero (the reason goes to WaitReason).
    Private Function FocusForKeys(k_layout As AdobeLayout) As IntPtr
        Dim k_wait As String = ReadModeBlocker(k_layout)
        If k_wait IsNot Nothing Then
            WaitReason(k_wait)
            Return IntPtr.Zero
        End If
        AdobeNativeMethods.SetFocus(k_layout.Page)
        Dim k_focus As IntPtr = AdobeNativeMethods.GetFocus()
        If k_focus = _host.Handle OrElse AdobeNativeMethods.IsChild(_host.Handle, k_focus) Then Return k_focus
        WaitReason($"keyboard focus not inside the control after SetFocus (focus {Describe(k_focus)})")
        Return IntPtr.Zero
    End Function

    ' Ctrl + one key, in one SendInput call. True when all four key events went out.
    Private Shared Function SendCtrlChord(k_vk As UShort, ByRef k_error As Integer) As Boolean
        Dim k_keys As AdobeNativeMethods.INPUT() = {
            AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, False),
            AdobeNativeMethods.KeyInput(k_vk, False),
            AdobeNativeMethods.KeyInput(k_vk, True),
            AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, True)}
        Dim k_sent As UInteger = AdobeNativeMethods.SendInput(CUInt(k_keys.Length), k_keys,
            Runtime.InteropServices.Marshal.SizeOf(GetType(AdobeNativeMethods.INPUT)))
        k_error = If(k_sent = k_keys.Length, 0, Runtime.InteropServices.Marshal.GetLastWin32Error())
        Return k_sent = k_keys.Length
    End Function

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
        ' Keys sent while script alerts come and go did not hold (old surface, 24.09.2026): wait for the burst to end
        ' (the trap's ScriptBurstEnded re-checks).
        If _saveTrap IsNot Nothing AndAlso _saveTrap.InScriptBurst Then Return $"script alerts within the last {ActiveXBurstQuietMs} ms"
        Dim k_form As Form = _panel.FindForm()
        If k_form Is Nothing OrElse Not k_form.IsHandleCreated Then Return "no form"
        If Not AdobeNativeMethods.IsWindowEnabled(k_form.Handle) Then Return "form disabled (a modal box is up)"
        Dim k_foreground As IntPtr = AdobeNativeMethods.GetForegroundWindow()
        If k_foreground <> k_form.Handle Then Return $"K-BOT is not the foreground window (foreground {Describe(k_foreground)})"
        Return Nothing
    End Function

End Class
