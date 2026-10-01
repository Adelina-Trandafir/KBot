Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

''' <summary>
''' Slice 0098: the ONE line of work for the FOREXE robot. Every operation the operator starts
''' that drives the robot (a node refresh, the Recepții / Rezervări refresh, the list, the
''' statements, a DDF send, a CAB note upload, an operation captured in the FOREXE page) is put
''' here and run in the exact order it was asked for, one at a time.
''' </summary>
''' <remarks>
''' <para><b>One task = the whole operation</b>, not only the robot part: questions, download,
''' the two-phase ingest (the «Asociere» window included) and the tree reload. So the next robot
''' run cannot start while an «Asociere» window waits for the operator -- there is never more
''' than one of them -- and the robot never runs while its previous package is being written
''' (slice 0098 rule 1, see <c>ServerGate</c>).</para>
''' <para><b>UI thread only.</b> Every member is called from the UI thread and every task runs
''' on it (they are the shell's own async flows); no lock is needed and none is taken.</para>
''' <para><b>Pause</b> stops the queue from starting the NEXT task; the running one finishes.
''' <b>Cancel</b> takes a waiting task out; the running one is stopped through the robot's own
''' cancel (<c>ForexeController.Cancel</c>), which the queue window calls.</para>
''' <para><b>A task that queues another task</b> (a flow reused inside a queued flow) runs it
''' in place: waiting behind itself would wait forever. Told apart with an AsyncLocal that
''' flows through the awaits of the running task.</para>
''' </remarks>
Public NotInheritable Class RobotQueue

    ''' <summary>One task, as the queue window shows it.</summary>
    Public NotInheritable Class RobotTask
        Public Property Id As Integer
        ''' <summary>Duplicate guard: two tasks with the same non-empty key cannot wait together.</summary>
        Public Property Key As String
        ''' <summary>What the operator reads in the queue window (Romanian).</summary>
        Public Property Label As String
        Public Property QueuedAt As Date
        Friend Property Run As Func(Of Task)
        Friend Property Drop As Action
    End Class

    Private ReadOnly _waiting As New List(Of RobotTask)()
    Private ReadOnly _insideTask As New AsyncLocal(Of Boolean)()
    Private ReadOnly _waitRobotIdle As Func(Of Task)
    Private ReadOnly _say As Action(Of String)
    Private _current As RobotTask
    Private _paused As Boolean
    Private _pumping As Boolean
    Private _nextId As Integer
    Private _note As String = String.Empty

    ''' <summary>Raised on the UI thread whenever the list, the running task, the pause or the note changes.</summary>
    Public Event Changed As EventHandler

    ''' <param name="waitRobotIdle">Awaited before each task: an operation started outside the
    ''' queue (the Conectare button, the Browser view) finishes first.</param>
    ''' <param name="say">Writes a line on the FOREXE console.</param>
    Public Sub New(waitRobotIdle As Func(Of Task), say As Action(Of String))
        ArgumentNullException.ThrowIfNull(waitRobotIdle)
        ArgumentNullException.ThrowIfNull(say)
        _waitRobotIdle = waitRobotIdle
        _say = say
    End Sub

    ''' <summary>The running task, or Nothing.</summary>
    Public ReadOnly Property Current As RobotTask
        Get
            Return _current
        End Get
    End Property

    ''' <summary>The tasks waiting, in the order they will run.</summary>
    Public ReadOnly Property Waiting As IReadOnlyList(Of RobotTask)
        Get
            Return _waiting.ToList()
        End Get
    End Property

    ''' <summary>
    ''' How many FOREXE actions there are right now: the waiting tasks, plus the running one --
    ''' or, when no queued task runs, a robot operation started outside the queue
    ''' (<paramref name="robotBusy"/> = <c>ForexeController.IsBusy</c>). The queue window opens by
    ''' itself only above ONE (operator, 30.09.2026).
    ''' </summary>
    ''' <remarks>A single click can sit in the waiting list for a moment (the queue has not picked
    ''' it up yet, or it waits for an operation outside the queue) -- counting only the waiting
    ''' list opened the window for one action.</remarks>
    Public Function ActionCount(robotBusy As Boolean) As Integer
        Return _waiting.Count + If(_current IsNot Nothing OrElse robotBusy, 1, 0)
    End Function

    Public ReadOnly Property IsPaused As Boolean
        Get
            Return _paused
        End Get
    End Property

    ''' <summary>What the running task waits for right now (e.g. the «Asociere» window); empty = nothing.</summary>
    Public ReadOnly Property Note As String
        Get
            Return _note
        End Get
    End Property

    ''' <summary>
    ''' Queues <paramref name="work"/> and completes when it has run (with its result or its
    ''' exception). Throws <see cref="RobotTaskDroppedException"/> when the task never ran: taken
    ''' out by the operator, or refused as a duplicate of one already queued / running.
    ''' </summary>
    ''' <param name="key">Duplicate guard; Nothing = no guard (tasks whose caller waits for a result).</param>
    ''' <param name="label">The operator's name of the task (Romanian).</param>
    ''' <param name="inFront">True = run right after the current task, before the waiting ones
    ''' (an operation captured in the FOREXE page: its pictures cannot wait for the page to move).</param>
    Public Function RunWithResultAsync(Of T)(key As String, label As String, work As Func(Of Task(Of T)),
                                   Optional inFront As Boolean = False) As Task(Of T)
        Try
            ArgumentNullException.ThrowIfNull(work)
            If _insideTask.Value Then Return work()

            If Not String.IsNullOrEmpty(key) AndAlso
               ((_current IsNot Nothing AndAlso String.Equals(_current.Key, key, StringComparison.OrdinalIgnoreCase)) OrElse
                _waiting.Any(Function(w) String.Equals(w.Key, key, StringComparison.OrdinalIgnoreCase))) Then
                _say($"«{label}» este deja în coada robotului — cererea nouă nu s-a mai adăugat.")
                Return Task.FromException(Of T)(New RobotTaskDroppedException($"«{label}» este deja în coada robotului."))
            End If

            Dim done As New TaskCompletionSource(Of T)(TaskCreationOptions.RunContinuationsAsynchronously)
            _nextId += 1
            Dim item As New RobotTask With {
                .Id = _nextId,
                .Key = key,
                .Label = label,
                .QueuedAt = Date.Now
            }
            item.Run = Async Function()
                           _insideTask.Value = True
                           Try
                               done.TrySetResult(Await work().ConfigureAwait(True))
                           Catch ex As Exception
                               done.TrySetException(ex)
                           End Try
                       End Function
            item.Drop = Sub() done.TrySetException(New RobotTaskDroppedException($"«{label}» a fost scoasă din coada robotului."))

            If inFront Then _waiting.Insert(0, item) Else _waiting.Add(item)
            If _current IsNot Nothing OrElse _paused Then
                _say($"«{label}» a intrat în coada robotului (poziția {_waiting.IndexOf(item) + 1}).")
            End If
            RaiseChanged()
            Pump()
            Return done.Task
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueue.RunWithResultAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>The same, for a task with no result.</summary>
    Public Function RunAsync(key As String, label As String, work As Func(Of Task),
                             Optional inFront As Boolean = False) As Task
        ArgumentNullException.ThrowIfNull(work)
        Return RunWithResultAsync(Of Boolean)(key, label,
                                    Async Function()
                                        Await work().ConfigureAwait(True)
                                        Return True
                                    End Function, inFront)
    End Function

    ''' <summary>Pause = start no new task; resume = carry on from the first waiting one.</summary>
    Public Sub SetPaused(value As Boolean)
        Try
            If _paused = value Then Return
            _paused = value
            _say(If(value, "Coada robotului e în pauză: sarcina curentă se termină, următoarele așteaptă.",
                           "Coada robotului pornește din nou."))
            RaiseChanged()
            If Not value Then Pump()
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueue.SetPaused", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Takes one waiting task out. False when it already started or finished.</summary>
    Public Function Cancel(id As Integer) As Boolean
        Try
            Dim item As RobotTask = _waiting.FirstOrDefault(Function(w) w.Id = id)
            If item Is Nothing Then Return False
            _waiting.Remove(item)
            item.Drop.Invoke()
            _say($"«{item.Label}» a fost scoasă din coada robotului.")
            RaiseChanged()
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueue.Cancel", ex)
            Throw
        End Try
    End Function

    ''' <summary>Takes every waiting task out; the running one is left alone.</summary>
    Public Function CancelAll() As Integer
        Try
            Dim removed As List(Of RobotTask) = _waiting.ToList()
            _waiting.Clear()
            For Each item As RobotTask In removed
                item.Drop.Invoke()
            Next
            If removed.Count > 0 Then
                _say($"{removed.Count} {If(removed.Count = 1, "sarcină a fost scoasă", "sarcini au fost scoase")} din coada robotului.")
            End If
            RaiseChanged()
            Return removed.Count
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueue.CancelAll", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The running task says what it waits for (the «Asociere» window); the queue window shows it.
    ''' Empty = it waits for nothing.
    ''' </summary>
    Public Sub SetNote(text As String)
        _note = If(text, String.Empty)
        RaiseChanged()
    End Sub

    ' The single worker. Re-entrant calls return at once; the loop that runs picks up the new task.
    Private Async Sub Pump()
        If _pumping Then Return
        _pumping = True
        Try
            Do While Not _paused AndAlso _waiting.Count > 0
                ' An operation started outside the queue finishes first; the queue can be paused
                ' or emptied meanwhile, so both are checked again after the wait.
                Await _waitRobotIdle().ConfigureAwait(True)
                If _paused OrElse _waiting.Count = 0 Then Exit Do

                Dim item As RobotTask = _waiting(0)
                _waiting.RemoveAt(0)
                _current = item
                _note = String.Empty
                RaiseChanged()
                Try
                    Await item.Run.Invoke().ConfigureAwait(True)   ' never throws: the result goes to the caller
                Finally
                    _current = Nothing
                    _note = String.Empty
                    RaiseChanged()
                End Try
            Loop
        Catch ex As Exception
            ' UI boundary (async Sub): log and swallow; the tasks already queued stay queued and the
            ' next RunAsync / RunWithResultAsync / resume starts the loop again.
            GlobalErrorLog.Write("RobotQueue.Pump", ex)
        Finally
            _pumping = False
        End Try
    End Sub

    Private Sub RaiseChanged()
        Try
            RaiseEvent Changed(Me, EventArgs.Empty)
        Catch ex As Exception
            ' Event boundary: a subscriber that throws must not stop the queue.
            GlobalErrorLog.Write("RobotQueue.RaiseChanged", ex)
        End Try
    End Sub
End Class

''' <summary>
''' Slice 0098: a queued robot task that never ran -- the operator took it out of the queue, or it
''' was a duplicate. The message is Romanian (it may reach a caller's box); callers that started
''' the task from a click swallow it, the console already said it.
''' </summary>
Public NotInheritable Class RobotTaskDroppedException
    Inherits OperationCanceledException

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub
End Class
