Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

' The SMALL watch (slice 0078-15, operator 07.10.2026): only what the two fixes need -- the size re-send
' (AcroPdfViewer.SizeNudge.vb) and Read Mode (AcroPdfViewer.ReadMode.vb). It is interchangeable with the big watch
' (AcroPdfViewer.Watch.vb, ACTIVEX-CHECK): DetailedWatch picks which one runs; both feed the fixes through the same
' three calls (NotePageView, NudgeIfAdobeWindowEmpty, ReadModeOnEvent). The big one stays in the code, unused, for a
' future investigation.
'
' Two out-of-context WinEvent hooks (object events CREATE..LOCATIONCHANGE, and the foreground change), from the load
' until the fixes are done (page laid out, Read Mode on or given up) or Clear. Nothing is written anywhere.
Partial Public NotInheritable Class AcroPdfViewer

    ''' <summary>
    ''' True = the big watch (ACTIVEX-CHECK: every window event, tree dumps, release snapshots, all written to
    ''' <c>Logs\activex_check.log</c>); False (default) = the small one, which only drives the fixes and writes nothing.
    ''' Read at each load.
    ''' </summary>
    Public Property DetailedWatch As Boolean

    Private _lightObject As IntPtr
    Private _lightForeground As IntPtr
    ' Kept in a field: the delegate must outlive the hooks (a collected callback crashes the process).
    Private _lightProc As AdobeNativeMethods.WinEventProc

    ' The watch DetailedWatch picks. Called by ShowDocument before every load.
    Private Sub StartWatching(k_path As String)
        StopWatching()
        If DetailedWatch Then
            StartWatch(k_path)                                                          ' ACTIVEX-CHECK
        Else
            StartLightWatch()
        End If
    End Sub

    ' Both, whichever runs (each is a no-op when it is not running).
    Private Sub StopWatching()
        StopWatch()                                                                     ' ACTIVEX-CHECK
        StopLightWatch()
    End Sub

    ' k_keepPageSeen: started again for a later key (Ctrl+S after a signature) -- the page is already laid out.
    Private Sub StartLightWatch(Optional k_keepPageSeen As Boolean = False)
        If Not k_keepPageSeen Then _seenPageView = False
        If _lightProc Is Nothing Then _lightProc = AddressOf OnLightWinEvent
        _lightObject = AdobeNativeMethods.SetWinEventHook(
            AdobeNativeMethods.EVENT_OBJECT_CREATE, AdobeNativeMethods.EVENT_OBJECT_LOCATIONCHANGE,
            IntPtr.Zero, _lightProc, 0UI, 0UI, AdobeNativeMethods.WINEVENT_OUTOFCONTEXT)
        _lightForeground = AdobeNativeMethods.SetWinEventHook(
            EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
            IntPtr.Zero, _lightProc, 0UI, 0UI, AdobeNativeMethods.WINEVENT_OUTOFCONTEXT)
    End Sub

    Private Sub StopLightWatch()
        If _lightObject <> IntPtr.Zero Then AdobeNativeMethods.UnhookWinEvent(_lightObject)
        If _lightForeground <> IntPtr.Zero Then AdobeNativeMethods.UnhookWinEvent(_lightForeground)
        _lightObject = IntPtr.Zero
        _lightForeground = IntPtr.Zero
    End Sub

    ' The fixes are done: the small watch has nothing left to do (the big one keeps watching -- that is its job).
    ' Called from posted code, never inside the WinEvent callback.
    Private Sub FixesDone()
        If Not DetailedWatch Then StopLightWatch()
    End Sub

    ' WinEvent callback (UI thread). Boundary: log and swallow.
    Private Sub OnLightWinEvent(hook As IntPtr, eventType As UInteger, hwnd As IntPtr,
                                idObject As Integer, idChild As Integer,
                                threadId As UInteger, timestamp As UInteger)
        Try
            If hwnd = IntPtr.Zero OrElse _host Is Nothing OrElse Not _host.IsHandleCreated Then Return
            If eventType = EVENT_SYSTEM_FOREGROUND Then
                ReadModeOnEvent()
                Return
            End If
            If idObject <> AdobeNativeMethods.OBJID_WINDOW OrElse idChild <> 0 Then Return

            Dim k_form As Form = _panel.FindForm()
            If k_form IsNot Nothing AndAlso k_form.IsHandleCreated AndAlso hwnd = k_form.Handle Then
                ' Enabled again after a modal box.
                If eventType = AdobeNativeMethods.EVENT_OBJECT_STATECHANGE Then ReadModeOnEvent()
                Return
            End If

            If eventType = AdobeNativeMethods.EVENT_OBJECT_DESTROY Then Return
            If hwnd <> _host.Handle AndAlso Not AdobeNativeMethods.IsChild(_host.Handle, hwnd) Then Return
            NotePageView(hwnd)
            NudgeIfAdobeWindowEmpty()
            ReadModeOnEvent()
        Catch ex As Exception
            GlobalErrorLog.Write("AcroPdfViewer.OnLightWinEvent", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Adobe's page view visible with a size: the moment Read Mode may go. Shared by both watches; the big one also
    ''' writes its MILESTONE line (that is why this returns True the first time).
    ''' </summary>
    Private Function NotePageView(hwnd As IntPtr) As Boolean
        If _seenPageView Then Return False
        If Not String.Equals(AdobeNativeMethods.GetTitle(hwnd), PageViewTitle, StringComparison.Ordinal) Then Return False
        If Not AdobeNativeMethods.IsWindowVisible(hwnd) Then Return False
        Dim k_rect As Rectangle = AdobeNativeMethods.RectInParent(hwnd)
        If k_rect.Width <= 0 OrElse k_rect.Height <= 0 Then Return False
        _seenPageView = True
        Return True
    End Function

End Class
