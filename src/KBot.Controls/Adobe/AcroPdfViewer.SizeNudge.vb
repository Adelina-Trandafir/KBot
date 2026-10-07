Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

' The fix under test, NOT check-only (slice 0078-15, operator 07.10.2026): Adobe's window inside the control born 0x0.
'
' Measured 07.10.2026 (load 09:35:38 against 09:35:54 and 09:36:38): when Adobe's process starts WHILE the document is
' being loaded, the window Adobe puts inside the control («Acrobat External Window», later titled with the document) is
' created 0x0, every view of the document under it is built 0x0, and nothing is shown until the control's rectangle is
' sent again -- the operator's click did it (the control's own LOCATIONCHANGE, then everything laid out within 30 ms).
' When Adobe was already running, that window had the control's size from the start.
' So: while the page view is not laid out yet, when a window directly inside the control is 0x0 and the control is not,
' re-send the control's rectangle (AcroPdfHost.ResendRectangle: geometry only, the document is NOT loaded again). At most
' MaxNudges times per load, not twice within NudgeGapMs. Driven by the watch's WinEvents, no timer; it lives on the
' watch hooks for now and moves with the hook it needs when the check code goes.
Partial Public NotInheritable Class AcroPdfViewer

    Private Const MaxNudges As Integer = 3
    Private Const NudgeGapMs As Long = 250

    Private _nudges As Integer
    Private _nudgePending As Boolean
    Private ReadOnly _nudgeClock As New Diagnostics.Stopwatch()

    ' Called by ShowDocument before every load.
    Private Sub ResetSizeNudge()
        _nudges = 0
        _nudgePending = False
        _nudgeClock.Reset()
    End Sub

    ' Called from the WinEvent callback for every event inside the control (wrapped there). Only reads our own windows'
    ' rectangles; the nudge itself is posted, so it runs after the callback has returned.
    Private Sub NudgeIfAdobeWindowEmpty()
        If _seenPageView OrElse _nudgePending OrElse _nudges >= MaxNudges Then Return
        If _nudgeClock.IsRunning AndAlso _nudgeClock.ElapsedMilliseconds < NudgeGapMs Then Return
        If _host Is Nothing OrElse Not _host.IsHandleCreated OrElse _host.Width <= 1 OrElse _host.Height <= 1 Then Return
        Dim k_empty As IntPtr = EmptyChildOfControl()
        If k_empty = IntPtr.Zero Then Return
        _nudgePending = True
        _panel.BeginInvoke(New Action(Sub() Nudge(k_empty)))
    End Sub

    ' A window directly inside the control with no width or no height, or Zero.
    Private Function EmptyChildOfControl() As IntPtr
        Dim k_child As IntPtr = AdobeNativeMethods.GetWindow(_host.Handle, AdobeNativeMethods.GW_CHILD)
        While k_child <> IntPtr.Zero
            Dim k_rect As Rectangle = AdobeNativeMethods.RectInParent(k_child)
            If k_rect.Width <= 0 OrElse k_rect.Height <= 0 Then Return k_child
            k_child = AdobeNativeMethods.GetWindow(k_child, AdobeNativeMethods.GW_HWNDNEXT)
        End While
        Return IntPtr.Zero
    End Function

    ' Posted from the WinEvent callback: log and swallow.
    Private Sub Nudge(k_empty As IntPtr)
        Try
            _nudgePending = False
            If _seenPageView OrElse _host Is Nothing OrElse Not _host.IsHandleCreated Then Return
            _nudges += 1
            _nudgeClock.Restart()
            Dim k_answer As String = _host.ResendRectangle()
            Check($"+{Elapsed()} ms >>>>> SIZE NUDGE {_nudges}/{MaxNudges}: window {HexOf(k_empty)} inside the control was 0x0 " &   ' ACTIVEX-CHECK
                  $"-> control rectangle {_host.Bounds} sent again ({k_answer}); that window is now {AdobeNativeMethods.RectInParent(k_empty)} <<<<<")
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.Nudge", ex)
        End Try
    End Sub

End Class
