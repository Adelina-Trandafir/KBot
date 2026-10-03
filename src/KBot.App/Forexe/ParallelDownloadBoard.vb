Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Common
Imports KBot.Forexe

''' <summary>
''' Slice 0100-03: what a multi-thread download looks like from outside -- the angajamente that
''' RUN right now (one row each, with how far its main flow got) and how many still WAIT for a free
''' tab. The queue window draws it; the X of a row stops that one download only.
''' </summary>
''' <remarks>
''' <para>One board per <see cref="ForexeController"/>. It is filled by the controller while a
''' multi-thread run goes (<see cref="Begin"/> ... <see cref="Finish"/>) and is read by the window.
''' Every change comes from a worker's thread, so every member takes the lock; the events are raised
''' OUTSIDE the lock and the window marshals to the UI thread.</para>
''' <para>A run of ONE angajament is kept here too (the stop and the progress work the same) but is
''' not <see cref="IsMulti"/>: the window shows nothing for it (operator, 03.10.2026).</para>
''' </remarks>
Public NotInheritable Class ParallelDownloadBoard

    ''' <summary>One running download, as the grid shows it.</summary>
    Public NotInheritable Class Entry
        Public Property Cod As String = String.Empty
        ''' <summary>0..100, by the main flow's steps; never goes backwards.</summary>
        Public Property Percent As Integer
        ''' <summary>The X was pressed: the tab is closing.</summary>
        Public Property Stopping As Boolean
        Friend Property Job As JobRequest
    End Class

    ''' <summary>A frozen copy of the board, so the window never reads a list that is changing.</summary>
    Public NotInheritable Class Snapshot
        Public Property Rows As IReadOnlyList(Of Entry) = New List(Of Entry)()
        Public Property Waiting As Integer
        ''' <summary>The codes of the angajamente still waiting for a tab, in the order they will start.</summary>
        Public Property WaitingCodes As IReadOnlyList(Of String) = New List(Of String)()
        Public Property Total As Integer
        ''' <summary>The most rows that can run together: the window sizes its grid for it.</summary>
        Public Property MaxRows As Integer
        Public Property IsMulti As Boolean
    End Class

    Private ReadOnly _gate As New Object()
    Private ReadOnly _running As New List(Of Entry)()
    Private ReadOnly _pending As New List(Of JobRequest)()
    Private _hooks As ParallelRunHooks
    Private _total As Integer
    Private _maxRows As Integer
    Private _active As Boolean

    ''' <summary>Anything changed (a row came, moved, or went; the waiting count). May come from any thread.</summary>
    Public Event Changed As EventHandler

    ''' <summary>A run of two or more angajamente began: the shell opens the queue window. May come from any thread.</summary>
    Public Event Started As EventHandler

    ''' <summary>True while a run of two or more angajamente goes -- the window shows the grid only then.</summary>
    Public ReadOnly Property IsMulti As Boolean
        Get
            SyncLock _gate
                Return _active AndAlso _total >= 2
            End SyncLock
        End Get
    End Property

    Public Function GetSnapshot() As Snapshot
        SyncLock _gate
            Return New Snapshot With {
                .Rows = _running.Select(Function(k_e) New Entry With {
                    .Cod = k_e.Cod, .Percent = k_e.Percent, .Stopping = k_e.Stopping, .Job = k_e.Job}).ToList(),
                .Waiting = _pending.Count,
                .WaitingCodes = _pending.Select(Function(k_j) CodOf(k_j)).ToList(),
                .Total = _total,
                .MaxRows = _maxRows,
                .IsMulti = _active AndAlso _total >= 2}
        End SyncLock
    End Function

    ''' <summary>
    ''' A run starts. Returns the hooks to hand to the robot; the board listens to the robot through
    ''' them. <paramref name="maxThreads"/> only sizes the grid (the runner decides the real number).
    ''' </summary>
    Friend Function Begin(k_jobs As IReadOnlyList(Of JobRequest), maxThreads As Integer) As ParallelRunHooks
        ArgumentNullException.ThrowIfNull(k_jobs)
        Dim k_hooks As New ParallelRunHooks With {
            .JobStarted = AddressOf OnJobStarted,
            .JobProgress = AddressOf OnJobProgress}
        Dim k_multi As Boolean
        SyncLock _gate
            _running.Clear()
            _pending.Clear()
            _pending.AddRange(k_jobs)
            _hooks = k_hooks
            _total = k_jobs.Count
            _maxRows = Math.Max(1, Math.Min(maxThreads, k_jobs.Count))
            _active = True
            k_multi = _total >= 2
        End SyncLock
        If k_multi Then RaiseEvent Started(Me, EventArgs.Empty)
        RaiseEvent Changed(Me, EventArgs.Empty)
        Return k_hooks
    End Function

    ''' <summary>The run is over (finished, failed or cancelled): the grid goes away.</summary>
    Friend Sub Finish()
        SyncLock _gate
            _running.Clear()
            _pending.Clear()
            _hooks = Nothing
            _total = 0
            _maxRows = 0
            _active = False
        End SyncLock
        RaiseEvent Changed(Me, EventArgs.Empty)
    End Sub

    ''' <summary>One download ended (any way): its row goes, and the next waiting one will take its place.</summary>
    Friend Sub JobEnded(k_job As JobRequest)
        SyncLock _gate
            _running.RemoveAll(Function(k_e) ReferenceEquals(k_e.Job, k_job))
            _pending.Remove(k_job)
        End SyncLock
        RaiseEvent Changed(Me, EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' The X of the row of <paramref name="k_cod"/>: stops that download only. Quiet when the row is
    ''' already gone (it ended while the operator was clicking).
    ''' </summary>
    Public Sub StopRow(k_cod As String)
        Dim k_hooks As ParallelRunHooks
        Dim k_job As JobRequest = Nothing
        SyncLock _gate
            k_hooks = _hooks
            Dim k_entry As Entry = _running.FirstOrDefault(Function(k_e) String.Equals(k_e.Cod, k_cod, StringComparison.OrdinalIgnoreCase))
            If k_entry IsNot Nothing Then
                k_entry.Stopping = True
                k_job = k_entry.Job
            End If
        End SyncLock
        If k_hooks Is Nothing OrElse k_job Is Nothing Then Return
        RaiseEvent Changed(Me, EventArgs.Empty)
        k_hooks.StopJob(k_job)
    End Sub

    ''' <summary>
    ''' The X of a WAITING angajament: it leaves the queue at once and will never get a tab. Quiet when it
    ''' already started or ended meanwhile.
    ''' </summary>
    Public Sub RemoveWaiting(k_cod As String)
        Dim k_hooks As ParallelRunHooks
        Dim k_job As JobRequest
        SyncLock _gate
            k_hooks = _hooks
            k_job = _pending.FirstOrDefault(Function(k_j) String.Equals(CodOf(k_j), k_cod, StringComparison.OrdinalIgnoreCase))
            If k_job Is Nothing Then Return
            _pending.Remove(k_job)
        End SyncLock
        ' The runner skips it when its turn comes (it is still in the robot's own line).
        If k_hooks IsNot Nothing Then k_hooks.StopJob(k_job)
        RaiseEvent Changed(Me, EventArgs.Empty)
    End Sub

    Private Shared Function CodOf(k_job As JobRequest) As String
        Dim k_cod As String = String.Empty
        If k_job.Parameters IsNot Nothing Then k_job.Parameters.TryGetValue(WorkflowCatalog.VarCodAngajament, k_cod)
        Return If(k_cod, String.Empty)
    End Function

    ' From the worker's thread: the job took a tab.
    Private Sub OnJobStarted(k_job As JobRequest, k_worker As Integer)
        SyncLock _gate
            If Not _active Then Return
            _pending.Remove(k_job)
            _running.Add(New Entry With {.Cod = CodOf(k_job), .Job = k_job})
        End SyncLock
        RaiseEvent Changed(Me, EventArgs.Empty)
    End Sub

    ' From the worker's thread: the main flow is at step k_step of k_steps.
    Private Sub OnJobProgress(k_job As JobRequest, k_step As Integer, k_steps As Integer)
        If k_steps <= 0 Then Return
        Dim k_percent As Integer = CInt(Math.Max(0L, Math.Min(100L, 100L * k_step \ k_steps)))
        SyncLock _gate
            Dim k_entry As Entry = _running.FirstOrDefault(Function(k_e) ReferenceEquals(k_e.Job, k_job))
            If k_entry Is Nothing OrElse k_percent <= k_entry.Percent Then Return
            k_entry.Percent = k_percent
        End SyncLock
        RaiseEvent Changed(Me, EventArgs.Empty)
    End Sub
End Class
