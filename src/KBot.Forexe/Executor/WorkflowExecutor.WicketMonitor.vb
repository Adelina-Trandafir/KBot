Imports Newtonsoft.Json.Linq

' =============================================================================
'  Date returnate de browser / de timer la fiecare schimbare sau idle confirmat
' =============================================================================
Public Class WicketMonitorEntry
    Public Property Timestamp As DateTime
    Public Property Source As String   ' "WICKET" | "WFL" | "IDLE" | "CLICK" | "KEY"
    Public Property Element As String
    Public Property State As String
    Public Property PageUrl As String
End Class

' =============================================================================
Partial Public Class WorkflowExecutor
    Public Const WicketIdleTimeoutMs As Integer = 5000
    Public Const WicketIdleThresholdMs As Integer = 300

    ' ── Câmpuri monitor ──────────────────────────────────────────────────────
    Private _wicketMonitoringActive As Boolean = False
    Private _statlogoNone As Boolean = False
    Private _animlogoNone As Boolean = False
    Private _idleTimer As System.Threading.Timer = Nothing
    Private ReadOnly _idleLock As New Object()

    ' ── Câmpuri WaitForWicketIdleAsync ───────────────────────────────────────
    Private _idleTcs As TaskCompletionSource(Of Boolean) = Nothing
    Private ReadOnly _idleTcsLock As New Object()

    ' ── Event ────────────────────────────────────────────────────────────────
    Public Event OnWicketStateChange(entry As WicketMonitorEntry)

    Public ReadOnly Property WicketMonitoringActive As Boolean
        Get
            Return _wicketMonitoringActive
        End Get
    End Property

    ' =========================================================================
    '  StartWicketMonitoringAsync
    ' =========================================================================
    Public Async Function StartWicketMonitoringAsync() As Task
        If _wicketMonitoringActive Then Return
        If _page Is Nothing Then Return

        Dim initEx As Exception = Nothing
        Try
            ' The timer first: a callback may arrive the moment the function is exposed.
            _idleTimer?.Dispose()
            _idleTimer = New System.Threading.Timer(
                AddressOf OnIdleTimerFired, Nothing,
                System.Threading.Timeout.Infinite,
                System.Threading.Timeout.Infinite)

            ' The delegate goes through MonitorTarget: a page adopted by the primary executor keeps
            ' calling the delegate its worker registered, and the answer must reach the new owner.
            Await _page.ExposeFunctionAsync(Of String)(
                "_wicketMonitorCallback",
                Sub(jsonArgs As String) MonitorTarget.HandleWicketPayload(jsonArgs))

            ' ── JS din cache ──────────────────────────────────────────────────
            Await _page.AddInitScriptAsync(GetEmbeddedJs("WicketMonitor.js"))

            ' ── Click monitor — instalat alături, același ciclu de viață ─────
            Await StartClickMonitoringAsync()

            _wicketMonitoringActive = True

        Catch ex As Exception
            initEx = ex
        End Try

        If initEx IsNot Nothing Then
            _logger.LogError($"[WicketMonitor] Eroare la activare: {initEx.Message}")
        End If
    End Function

    ' =========================================================================
    '  Adopted tabs (slice 0100)
    ' =========================================================================

    ' Set on a worker whose tab the primary executor adopted: the callbacks registered in that
    ' page (ExposeFunctionAsync cannot be undone) still run the worker's delegates, and these
    ' hand every call on to the primary.
    Private _monitorForward As WorkflowExecutor = Nothing

    Private ReadOnly Property MonitorTarget As WorkflowExecutor
        Get
            Return If(_monitorForward, Me)
        End Get
    End Property

    Friend Sub RaiseWicketEntry(entry As WicketMonitorEntry)
        RaiseEvent OnWicketStateChange(entry)
    End Sub

    ''' <summary>One «#statlogo» / «#animlogo» report from the page. Runs on the Playwright callback thread.</summary>
    Friend Sub HandleWicketPayload(jsonArgs As String)
        Dim entry As WicketMonitorEntry = Nothing
        Dim parseEx As Exception = Nothing
        Try
            Dim obj As JObject = JObject.Parse(jsonArgs)
            entry = New WicketMonitorEntry With {
                .Timestamp = DateTime.Now,
                .Source = "WICKET",
                .Element = obj.Value(Of String)("element"),
                .State = obj.Value(Of String)("state"),
                .PageUrl = obj.Value(Of String)("url")
            }
        Catch ex As Exception
            parseEx = ex
        End Try

        If parseEx IsNot Nothing OrElse entry Is Nothing Then Return

        RaiseEvent OnWicketStateChange(entry)

        Dim isNone As Boolean = String.Equals(entry.State, "none",
                                              StringComparison.OrdinalIgnoreCase)
        SyncLock _idleLock
            If entry.Element = "#statlogo" Then _statlogoNone = isNone
            If entry.Element = "#animlogo" Then _animlogoNone = isNone
            If _idleTimer Is Nothing Then Return

            If _animlogoNone Then
                _idleTimer.Change(WicketIdleThresholdMs,
                                  System.Threading.Timeout.Infinite)
            Else
                _idleTimer.Change(System.Threading.Timeout.Infinite,
                                  System.Threading.Timeout.Infinite)
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Slice 0100: this executor now owns the page of <paramref name="worker"/>. The Wicket / click /
    ''' key monitors the worker installed in it are still there and cannot be installed again, so they
    ''' are taken over instead: the worker forwards to this executor, which gets its own idle timer and
    ''' the last state the page reported. A worker that never started the monitors leaves this executor
    ''' inactive, and the next wait installs them on the (clean) page as usual.
    ''' </summary>
    Friend Sub TakeOverWicketMonitoring(worker As WorkflowExecutor)
        StopWicketMonitoring()   ' the old page's monitor went with the old page
        If Not worker._wicketMonitoringActive Then Return

        Dim statNone As Boolean
        Dim animNone As Boolean
        SyncLock worker._idleLock
            statNone = worker._statlogoNone
            animNone = worker._animlogoNone
        End SyncLock

        SyncLock _idleLock
            _statlogoNone = statNone
            _animlogoNone = animNone
            _idleTimer = New System.Threading.Timer(
                AddressOf OnIdleTimerFired, Nothing,
                System.Threading.Timeout.Infinite,
                System.Threading.Timeout.Infinite)
        End SyncLock
        _wicketMonitoringActive = True

        worker._monitorForward = Me
        worker.StopWicketMonitoring()   ' the worker's own timer and waiters
    End Sub

    ' =========================================================================
    '  OnIdleTimerFired
    ' =========================================================================
    Private Sub OnIdleTimerFired(state As Object)
        Dim shouldRaise As Boolean = False
        SyncLock _idleLock
            shouldRaise = _animlogoNone
        End SyncLock

        If Not shouldRaise Then Return

        RaiseEvent OnWicketStateChange(New WicketMonitorEntry With {
            .Timestamp = DateTime.Now,
            .Source = "IDLE",
            .Element = "WICKET IDLE",
            .State = $"({WicketIdleThresholdMs}ms)",
            .PageUrl = ""
        })

        SyncLock _idleTcsLock
            If _idleTcs IsNot Nothing Then
                _idleTcs.TrySetResult(True)
                _idleTcs = Nothing
            End If
        End SyncLock
    End Sub

    ' =========================================================================
    '  WaitForWicketIdleAsync
    ' =========================================================================
    Public Async Function WaitForWicketIdleAsync() As Task
        If Not _wicketMonitoringActive Then
            Await StartWicketMonitoringAsync()
        End If

        Dim tcs As New TaskCompletionSource(Of Boolean)()
        SyncLock _idleTcsLock
            _idleTcs = tcs
        End SyncLock

        Dim alreadyIdle As Boolean
        SyncLock _idleLock
            alreadyIdle = _animlogoNone
        End SyncLock
        If alreadyIdle Then tcs.TrySetResult(True)

        Try
            ' Slice 0091: the cap stretches with the operator's multiplier for the waits.
            Await Task.WhenAny(tcs.Task, Task.Delay(Stretched(WicketIdleTimeoutMs)))
        Finally
            SyncLock _idleTcsLock
                If _idleTcs Is tcs Then _idleTcs = Nothing
            End SyncLock
        End Try
    End Function

    ' =========================================================================
    '  StopWicketMonitoring
    ' =========================================================================
    Public Sub StopWicketMonitoring()
        SyncLock _idleTcsLock
            If _idleTcs IsNot Nothing Then
                _idleTcs.TrySetResult(False)
                _idleTcs = Nothing
            End If
        End SyncLock

        SyncLock _idleLock
            If _idleTimer IsNot Nothing Then
                _idleTimer.Change(System.Threading.Timeout.Infinite,
                                  System.Threading.Timeout.Infinite)
                _idleTimer.Dispose()
                _idleTimer = Nothing
            End If
            _statlogoNone = False
            _animlogoNone = False
            _wicketMonitoringActive = False
        End SyncLock
    End Sub

End Class