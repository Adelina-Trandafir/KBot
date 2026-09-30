Option Strict On
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Watches the windows of the hosted Adobe process (slices 0078-07 and 0078-08). Two signals:
'''
''' <list type="bullet">
''' <item><see cref="HostedWindowMoved"/> -- the hosted window itself moved or changed size (seen on a
''' client PC 29.09.2026: placed at the panel's size, then back at screen size, clipped). The host
''' puts it back to the panel.</item>
''' <item><see cref="Quiet"/> -- Adobe's windows stopped changing: no window of the process was shown,
''' hidden or moved for <see cref="QuietMs"/>. The host uses it to decide that a document has finished
''' opening (0078-08: the trees stay locked until then).</item>
''' </list>
'''
''' A WinEvent hook, out of context (no DLL goes into Adobe; the callback runs on the UI thread that
''' installed it), on EVENT_OBJECT_SHOW .. EVENT_OBJECT_LOCATIONCHANGE of Adobe's process, only for
''' whole windows (OBJID_WINDOW). A burst of events is folded into one signal, raised when the burst
''' is over; the host's own moves raise events too, and the host only acts on what it finds.
''' </summary>
Friend NotInheritable Class AdobeWindowWatcher
    Implements IDisposable

    ''' <summary>How long Adobe's windows must stay unchanged to count as «quiet».</summary>
    Public Const QuietMs As Integer = 250

    Private _hook As IntPtr = IntPtr.Zero
    Private _window As IntPtr = IntPtr.Zero
    ' Kept in a field: the native side holds only a pointer, so the delegate must not be collected.
    Private ReadOnly _proc As AdobeNativeMethods.WinEventProc
    ' Folds a burst of events into one signal (restarted by every event).
    Private ReadOnly _settle As New Timer()
    Private _movedPending As Boolean

    ''' <summary>The hosted window moved or was resized (UI thread, once per burst).</summary>
    Public Event HostedWindowMoved As Action
    ''' <summary>No window of Adobe's process changed for <see cref="QuietMs"/> (UI thread).</summary>
    Public Event Quiet As Action

    Public Sub New()
        _proc = AddressOf OnWinEvent
        _settle.Interval = QuietMs
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
                AdobeNativeMethods.EVENT_OBJECT_SHOW, AdobeNativeMethods.EVENT_OBJECT_LOCATIONCHANGE,
                IntPtr.Zero, _proc, CUInt(pid), 0UI, AdobeNativeMethods.WINEVENT_OUTOFCONTEXT)
            Return _hook <> IntPtr.Zero
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowWatcher.Start", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Restarts the quiet wait as if an event had just arrived: <see cref="Quiet"/> follows after
    ''' <see cref="QuietMs"/> unless Adobe changes something first. Used right after hosting (Adobe may
    ''' already be done, and then no event would ever come) and after K-BOT sent keys to Adobe.
    ''' </summary>
    Public Sub Poke()
        If _window = IntPtr.Zero Then Return
        _settle.Stop()
        _settle.Start()
    End Sub

    ''' <summary>Stops watching. Safe when nothing is watched.</summary>
    Public Sub [Stop]()
        Try
            _settle.Stop()
            _movedPending = False
            If _hook <> IntPtr.Zero Then AdobeNativeMethods.UnhookWinEvent(_hook)
            _hook = IntPtr.Zero
            _window = IntPtr.Zero
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowWatcher.Stop", ex)
        End Try
    End Sub

    ' Native callback (UI thread): whole windows only (no caret, cursor or accessibility children).
    Private Sub OnWinEvent(hook As IntPtr, eventType As UInteger, hWnd As IntPtr,
                           idObject As Integer, idChild As Integer, threadId As UInteger, timestamp As UInteger)
        Try
            If hook <> _hook OrElse idObject <> AdobeNativeMethods.OBJID_WINDOW OrElse idChild <> 0 Then Return
            If eventType = AdobeNativeMethods.EVENT_OBJECT_LOCATIONCHANGE AndAlso hWnd = _window Then _movedPending = True
            _settle.Stop()
            _settle.Start()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowWatcher.OnWinEvent", ex)
        End Try
    End Sub

    Private Sub OnSettled(sender As Object, e As EventArgs)
        Try
            _settle.Stop()
            If _window = IntPtr.Zero Then Return
            If _movedPending Then
                _movedPending = False
                RaiseEvent HostedWindowMoved()
            End If
            ' Moving the window back makes new events; Quiet then comes after THAT burst.
            If Not _settle.Enabled Then RaiseEvent Quiet()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeWindowWatcher.OnSettled", ex)
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        [Stop]()
        RemoveHandler _settle.Tick, AddressOf OnSettled
        _settle.Dispose()
    End Sub

End Class
