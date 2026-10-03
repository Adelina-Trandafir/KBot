Imports System.Collections.Generic
Imports System.Threading
Imports KBot.Domain     ' CelulaTabel / RandTabel / TabelRezultat (decizia D-N).

Namespace KBot.Forexe
    Public Class JobRequest
        Public Property WorkflowName As String = String.Empty
        Public Property WflPath As String = String.Empty
        Public Property Parameters As New Dictionary(Of String, String)
        ' Slice 0081-07, the dry run: every Click marked commits="true" is skipped and the run
        ' stops right before it (WorkflowExecutor.StopBeforeCommit). Nothing is saved in FOREXE.
        Public Property StopBeforeSave As Boolean

        ' There is no ShowBrowser switch any more (slice 0070). KBOT_IPC had one
        ' (`isStealth = Not jobToRun.ShowBrowser`) and a job could ask for a visible Chromium
        ' window; here the window is ALWAYS born hidden, and the operator sees the page only
        ' docked into a K-BOT form (the console's show-browser button). A free window has a
        ' close button, and closing it kills the session.
    End Class

    Public Class JobResult
        Public Property Success As Boolean
        Public Property Message As String = String.Empty
        ' Slice 0081-07: the run was a dry run and stopped before a step that saves.
        Public Property StoppedBeforeSave As Boolean
        ' The executor's flat variables at the end of the job (name -> value).
        ' Existing consumers (flat dictionary) are left untouched.
        Public Property Data As New Dictionary(Of String, String)
        ' Additive enrichment: the tabular results (e.g. ScrapeTable) broken out per
        ' variable -> list of rows (column -> cell). Filled by RunJobAsync for every
        ' variable that holds a JSON array of objects.
        '
        ' THE CELL IS `CelulaTabel`, NOT `String`, SINCE 26.08.2026 (decision D-N). A
        ' `ForEachVar` whose `collectFields` names a field an inner `ScrapeTable` writes with
        ' `saveTo` produces a NESTED cell, and the executor keeps it as it is
        ' (`BuildCollectedRow` does `JToken.Parse`). Here it used to be flattened back to text
        ' with `.ToString()`, and the server had to keep a second read path alive for it. The
        ' structure travels; nothing is flattened.
        Public Property Tables As New Dictionary(Of String, TabelRezultat)
    End Class

    ''' <summary>
    ''' Slice 0100: what one job of a multi-thread run brought back, kept in the common result list
    ''' the moment its tab finished. A failure, a stop or a cancel is an outcome too (Success =
    ''' False, the reason in Message): one tab giving up never takes the others down with it.
    ''' </summary>
    Public Class ParallelJobOutcome
        Public Property Job As JobRequest
        Public Property Result As JobResult
        ''' <summary>The worker (1..N) whose tab ran the job; 0 = the job never started (the run was cancelled first).</summary>
        Public Property Worker As Integer
        Public Property StartedAt As Date
        Public Property FinishedAt As Date
        ''' <summary>
        ''' Slice 0100-03: the operator stopped THIS download (the X of its row): it never started, or its
        ''' tab was closed under it. Not a failure -- the caller reports nothing for it.
        ''' </summary>
        Public Property StoppedByOperator As Boolean
    End Class

    ''' <summary>
    ''' Slice 0100-03: the two-way line between a multi-thread run and whoever shows it. Out: a job
    ''' started on a tab (<see cref="JobStarted"/>) and how far its main flow got
    ''' (<see cref="JobProgress"/>, step / steps) -- both called from the worker's thread. In:
    ''' <see cref="StopJob"/> stops ONE job, running or still waiting, and leaves the others alone.
    ''' </summary>
    ''' <remarks>
    ''' A job that has not started is simply skipped when its turn comes. A running one has its tab
    ''' CLOSED (the runner does it): a robot step that waits on the page is cut short that way,
    ''' where a cancel token is only looked at between steps.
    ''' </remarks>
    Public NotInheritable Class ParallelRunHooks
        Private ReadOnly _gate As New Object()
        Private ReadOnly _stopped As New Dictionary(Of JobRequest, CancellationTokenSource)()

        ''' <summary>(job, worker number) -- the job took a tab and began.</summary>
        Public Property JobStarted As Action(Of JobRequest, Integer)

        ''' <summary>(job, step, steps) -- progress of the job's main flow.</summary>
        Public Property JobProgress As Action(Of JobRequest, Integer, Integer)

        ''' <summary>Stops <paramref name="k_job"/>. False when it is unknown (already gone).</summary>
        Public Function StopJob(k_job As JobRequest) As Boolean
            ArgumentNullException.ThrowIfNull(k_job)
            Dim k_source As CancellationTokenSource = Nothing
            SyncLock _gate
                If Not _stopped.TryGetValue(k_job, k_source) Then
                    k_source = New CancellationTokenSource()
                    _stopped(k_job) = k_source
                End If
            End SyncLock
            k_source.Cancel()
            Return True
        End Function

        ''' <summary>True once the operator asked to stop <paramref name="k_job"/>.</summary>
        Public Function IsStopped(k_job As JobRequest) As Boolean
            SyncLock _gate
                Dim k_source As CancellationTokenSource = Nothing
                Return _stopped.TryGetValue(k_job, k_source) AndAlso k_source.IsCancellationRequested
            End SyncLock
        End Function

        ''' <summary>The token that fires when the operator stops <paramref name="k_job"/>.</summary>
        Public Function StopTokenOf(k_job As JobRequest) As CancellationToken
            SyncLock _gate
                Dim k_source As CancellationTokenSource = Nothing
                If Not _stopped.TryGetValue(k_job, k_source) Then
                    k_source = New CancellationTokenSource()
                    _stopped(k_job) = k_source
                End If
                Return k_source.Token
            End SyncLock
        End Function
    End Class
End Namespace
