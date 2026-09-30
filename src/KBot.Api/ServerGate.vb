Option Strict On
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Slice 0098: while the FOREXE robot runs a workflow, nothing is WRITTEN on the K-BOT server --
''' the operator's rule, so that a download in progress and the database can never meet halfway.
''' Reads go through (operator, 30.09.2026: the views must not freeze for the length of a download).
''' </summary>
''' <remarks>
''' <para><b>How.</b> Every request of the shared <c>HttpClient</c> passes through
''' <see cref="ServerGateHandler"/>. A read (<see cref="IsRead"/>: GET / HEAD / OPTIONS) always passes.
''' While the gate is closed a new WRITE waits at the gate (it
''' is not refused: the view that asked simply shows its busy bar a little longer), and it leaves
''' the moment the robot is done. Closing the gate also waits for the writes already on the wire to
''' come back, so the robot never starts with a write half done.</para>
''' <para><b>A POST is a write</b>, even the few that only read (the ingest proposal, which the
''' server rolls back): the method is the only thing the gate can see, and holding a read is
''' harmless where letting a write through is not.</para>
''' <para><b>Who closes it.</b> Only <c>ForexeController</c>, around the runner call itself (the
''' workflow run, the statements walk, a document upload / receipt search) -- not around the
''' whole operation, because the history read that decides forward / REVERSE is a server read
''' made right before the run.</para>
''' <para><b>What passes a closed gate</b> (<see cref="Bypass"/>): the requests the robot itself
''' makes DURING its run and waits for -- the Excel processing of a workflow step and the id
''' markers the FOREXE page asks for on a save. Holding them would hold the robot forever. The
''' update channel passes too: it never touches the database.</para>
''' </remarks>
Public NotInheritable Class ServerGate

    ''' <summary>Set to True on a request that must pass a closed gate (see the remarks).</summary>
    Public Shared ReadOnly Bypass As New HttpRequestOptionsKey(Of Boolean)("kbot.gate_bypass")

    ''' <summary>GET, HEAD and OPTIONS: they change nothing on the server, so they are never held.</summary>
    Public Shared Function IsRead(method As HttpMethod) As Boolean
        Return method = HttpMethod.Get OrElse method = HttpMethod.Head OrElse method = HttpMethod.Options
    End Function

    Private ReadOnly _sync As New Object()
    Private _closedCount As Integer
    Private _open As TaskCompletionSource(Of Boolean) = NewOpenSignal(alreadyOpen:=True)
    Private _inFlight As Integer
    Private _drained As TaskCompletionSource(Of Boolean)

    ''' <summary>Raised (on any thread) when the gate closes or opens.</summary>
    Public Event StateChanged As EventHandler

    ''' <summary>True while the robot runs and server writes are held.</summary>
    Public ReadOnly Property IsClosed As Boolean
        Get
            SyncLock _sync
                Return _closedCount > 0
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Closes the gate and waits until the writes already on the wire have come back. Every
    ''' call is paired with one <see cref="Open"/> (in a Finally).
    ''' </summary>
    Public Async Function CloseAsync() As Task
        Dim drained As Task
        Dim changed As Boolean
        SyncLock _sync
            _closedCount += 1
            If _closedCount = 1 Then
                _open = NewOpenSignal(alreadyOpen:=False)
                changed = True
            End If
            If _inFlight = 0 Then
                drained = Task.CompletedTask
            Else
                If _drained Is Nothing Then _drained = New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
                drained = _drained.Task
            End If
        End SyncLock
        If changed Then RaiseEvent StateChanged(Me, EventArgs.Empty)
        Await drained.ConfigureAwait(True)
    End Function

    ''' <summary>Opens the gate; every held request leaves.</summary>
    Public Sub Open()
        Dim release As TaskCompletionSource(Of Boolean) = Nothing
        SyncLock _sync
            ' An Open without its Close is a programming defect, not a silent no-op.
            If _closedCount = 0 Then Throw New InvalidOperationException("ServerGate.Open without a matching CloseAsync.")
            _closedCount -= 1
            If _closedCount = 0 Then release = _open
        End SyncLock
        If release IsNot Nothing Then
            release.TrySetResult(True)
            RaiseEvent StateChanged(Me, EventArgs.Empty)
        End If
    End Sub

    ''' <summary>Waits while the gate is closed, then counts the request as on the wire.</summary>
    Friend Async Function EnterAsync(ct As CancellationToken) As Task
        Do
            Dim wait As Task
            SyncLock _sync
                If _closedCount = 0 Then
                    _inFlight += 1
                    Return
                End If
                wait = _open.Task
            End SyncLock
            Await wait.WaitAsync(ct).ConfigureAwait(False)
        Loop
    End Function

    ''' <summary>The request came back (answer read whole): no longer on the wire.</summary>
    Friend Sub Leave()
        Dim release As TaskCompletionSource(Of Boolean) = Nothing
        SyncLock _sync
            _inFlight = Math.Max(0, _inFlight - 1)
            If _inFlight = 0 AndAlso _drained IsNot Nothing Then
                release = _drained
                _drained = Nothing
            End If
        End SyncLock
        release?.TrySetResult(True)
    End Sub

    Private Shared Function NewOpenSignal(alreadyOpen As Boolean) As TaskCompletionSource(Of Boolean)
        Dim signal As New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
        If alreadyOpen Then signal.SetResult(True)
        Return signal
    End Function
End Class
