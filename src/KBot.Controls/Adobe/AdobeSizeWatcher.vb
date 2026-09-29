Option Strict On
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Slice 0078-07: tells the host when the HOSTED Adobe window moved or changed size on its own.
'''
''' Seen on a client PC 29.09.2026: K-BOT placed the window at the panel's size (log: «NESCHIMBAT»
''' on the second pass), yet a moment later it was 1919x1079 inside an 807x772 panel -- clipped, its
''' right side and bottom out of sight. Nothing in K-BOT noticed, because the panel is only re-filled
''' when the PANEL changes size.
'''
''' A WinEvent hook (out of context: no DLL goes into Adobe; the callback runs on the UI thread that
''' installed it) on EVENT_OBJECT_LOCATIONCHANGE of Adobe's process, filtered to the hosted window
''' itself. A burst of events is folded into one <see cref="Changed"/>, raised 100 ms after the last
''' one -- the host's own MoveWindow raises events too, and the host only acts when the rectangle is
''' not the panel's.
''' </summary>
Friend NotInheritable Class AdobeSizeWatcher
    Implements IDisposable

    Private Const SettleMs As Integer = 100

    Private _hook As IntPtr = IntPtr.Zero
    Private _window As IntPtr = IntPtr.Zero
    ' Kept in a field: the native side holds only a pointer, so the delegate must not be collected.
    Private ReadOnly _proc As AdobeNativeMethods.WinEventProc
    Private ReadOnly _settle As New Timer()

    ''' <summary>The hosted window moved or was resized by someone (UI thread, once per burst).</summary>
    Public Event Changed As Action

    Public Sub New()
        _proc = AddressOf OnWinEvent
        _settle.Interval = SettleMs
        AddHandler _settle.Tick, AddressOf OnSettled
    End Sub

    ''' <summary>
    ''' Starts watching <paramref name="window"/> (owned by process <paramref name="pid"/>). Install
    ''' from the UI thread only. Returns False when Windows refused the hook. Risky boundary (Win32):
    ''' logs and rethrows.
    ''' </summary>
    Public Function Start(window As IntPtr, pid As Integer) As Boolean
        Try
            [Stop]()
            If window = IntPtr.Zero OrElse pid <= 0 Then Return False
            _window = window
            _hook = AdobeNativeMethods.SetWinEventHook(
                AdobeNativeMethods.EVENT_OBJECT_LOCATIONCHANGE, AdobeNativeMethods.EVENT_OBJECT_LOCATIONCHANGE,
                IntPtr.Zero, _proc, CUInt(pid), 0UI, AdobeNativeMethods.WINEVENT_OUTOFCONTEXT)
            Return _hook <> IntPtr.Zero
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSizeWatcher.Start", ex)
            Throw
        End Try
    End Function

    ''' <summary>Stops watching. Safe when nothing is watched.</summary>
    Public Sub [Stop]()
        Try
            _settle.Stop()
            If _hook <> IntPtr.Zero Then AdobeNativeMethods.UnhookWinEvent(_hook)
            _hook = IntPtr.Zero
            _window = IntPtr.Zero
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSizeWatcher.Stop", ex)
        End Try
    End Sub

    ' Native callback (UI thread): only the hosted window itself, never its inner views.
    Private Sub OnWinEvent(hook As IntPtr, eventType As UInteger, hWnd As IntPtr,
                           idObject As Integer, idChild As Integer, threadId As UInteger, timestamp As UInteger)
        Try
            If hook <> _hook OrElse hWnd <> _window OrElse idObject <> AdobeNativeMethods.OBJID_WINDOW OrElse idChild <> 0 Then Return
            _settle.Stop()
            _settle.Start()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSizeWatcher.OnWinEvent", ex)
        End Try
    End Sub

    Private Sub OnSettled(sender As Object, e As EventArgs)
        Try
            _settle.Stop()
            If _window = IntPtr.Zero Then Return
            RaiseEvent Changed()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeSizeWatcher.OnSettled", ex)
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        [Stop]()
        RemoveHandler _settle.Tick, AddressOf OnSettled
        _settle.Dispose()
    End Sub

End Class
