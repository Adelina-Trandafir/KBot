#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Domain
Imports KBot.Forexe

''' <summary>
''' Slice 0098 bench: a FOREXE robot with no browser and no FOREXE. Every job "runs" for
''' <see cref="RunSeconds"/>, reports progress and status lines like the real executor, and hands
''' back a JobResult shaped like a real «Prelucrare Completa» answer: the flat variables plus the
''' TabelIstoric / TabelIndicatori_results / ListaReceptii_results tables, with made-up rows.
''' </summary>
''' <remarks>
''' Halfway through a run it calls <see cref="MidRunCall"/> when set: the bench uses it to send a
''' request marked <c>ServerGate.Bypass</c>, the way a real workflow's Excel step does, to show it
''' passing the closed gate. The browser / page members are quiet no-ops: the queue never needs them.
''' </remarks>
Public NotInheritable Class FakeForexeRunner
    Implements IForexeRunner

    Private ReadOnly _rng As New Random()

    ''' <summary>How long one job takes, in seconds.</summary>
    Public Property RunSeconds As Double = 4
    ''' <summary>True = every job ends with Success = False, like a workflow that stopped.</summary>
    Public Property FailRuns As Boolean
    ''' <summary>Called once, halfway through each job (a robot-own server request).</summary>
    Public Property MidRunCall As Func(Of Task)
    ''' <summary>The session the controller asks about; the bench starts "connected".</summary>
    Public Property Connected As Boolean = True

    Public Event StatusUpdated As EventHandler(Of String) Implements IForexeRunner.StatusUpdated
    Public Event BrowserVisibilityChanged As EventHandler Implements IForexeRunner.BrowserVisibilityChanged
    Public Event OperationCaptured As EventHandler(Of ForexeWatchEvent) Implements IForexeRunner.OperationCaptured

    Public ReadOnly Property HasLiveSession As Boolean Implements IForexeRunner.HasLiveSession
        Get
            Return Connected
        End Get
    End Property

    Public Async Function RunAsync(job As JobRequest, certificate As X509Certificate2,
                                   progress As IProgress(Of Integer), ct As CancellationToken) As Task(Of JobResult) _
        Implements IForexeRunner.RunAsync
        ' «Conectare»: a shorter run, then the session is live.
        Dim result As JobResult = Await SimulateAsync(job, progress, ct, RunSeconds / 2).ConfigureAwait(True)
        If result.Success Then Connected = True
        Return result
    End Function

    Public Async Function RunJobAsync(job As JobRequest, progress As IProgress(Of Integer),
                                      ct As CancellationToken) As Task(Of JobResult) _
        Implements IForexeRunner.RunJobAsync
        Return Await SimulateAsync(job, progress, ct, RunSeconds).ConfigureAwait(True)
    End Function

    ''' <summary>
    ''' Slice 0100 bench: the same contract as the real runner -- at most min(maxThreads, 10) fake
    ''' runs together, a FIFO queue behind them, each answer added to the list when it ends.
    ''' </summary>
    Public Async Function RunJobsParallelAsync(jobs As IReadOnlyList(Of JobRequest), maxThreads As Integer,
                                               jobFinished As Action(Of ParallelJobOutcome),
                                               hooks As ParallelRunHooks,
                                               ct As CancellationToken) As Task(Of List(Of ParallelJobOutcome)) _
        Implements IForexeRunner.RunJobsParallelAsync
        Dim outcomes As New List(Of ParallelJobOutcome)()
        Dim fifo As New Queue(Of JobRequest)(jobs)
        Dim threads As Integer = Math.Min(Math.Min(Math.Max(1, maxThreads), 10), Math.Max(1, jobs.Count))
        Dim workers As New List(Of Task)()
        For n As Integer = 1 To threads
            Dim workerNo As Integer = n
            workers.Add(Task.Run(
                Async Function()
                    Do
                        Dim job As JobRequest = Nothing
                        SyncLock fifo
                            If fifo.Count = 0 OrElse ct.IsCancellationRequested Then Exit Do
                            job = fifo.Dequeue()
                        End SyncLock
                        Dim started As Date = Date.Now
                        Dim outcome As ParallelJobOutcome
                        If hooks IsNot Nothing AndAlso hooks.IsStopped(job) Then
                            outcome = New ParallelJobOutcome With {
                                .Job = job, .Worker = 0, .StartedAt = started, .FinishedAt = started, .StoppedByOperator = True,
                                .Result = New JobResult With {.Success = False, .Message = "Oprit de operator (simulat)."}}
                        Else
                            hooks?.JobStarted?.Invoke(job, workerNo)
                            Dim k_stop As CancellationToken = If(hooks IsNot Nothing, hooks.StopTokenOf(job), CancellationToken.None)
                            Dim k_report As Action(Of Integer, Integer) = Nothing
                            If hooks IsNot Nothing Then k_report = Sub(k_step As Integer, k_steps As Integer) hooks.JobProgress?.Invoke(job, k_step, k_steps)
                            Dim result As JobResult = Await SimulateAsync(job, Nothing, ct, RunSeconds, k_stop, k_report).ConfigureAwait(False)
                            If k_stop.IsCancellationRequested Then
                                result = New JobResult With {.Success = False, .Message = "Oprit de operator (simulat)."}
                            End If
                            outcome = New ParallelJobOutcome With {.Job = job, .Result = result, .Worker = workerNo,
                                                                   .StartedAt = started, .FinishedAt = Date.Now,
                                                                   .StoppedByOperator = k_stop.IsCancellationRequested}
                        End If
                        SyncLock outcomes
                            outcomes.Add(outcome)
                        End SyncLock
                        jobFinished?.Invoke(outcome)
                    Loop
                End Function))
        Next
        Await Task.WhenAll(workers).ConfigureAwait(True)
        Return outcomes
    End Function

    Public Async Function DescarcaExtraseAsync(folderDescarcare As String, dataDeLa As Date?,
                                               progres As Action(Of Integer, Integer, String),
                                               ct As CancellationToken) As Task(Of List(Of ExtrasDescarcat)) _
        Implements IForexeRunner.DescarcaExtraseAsync
        Await SimulateAsync(New JobRequest With {.WorkflowName = "Extrase"}, Nothing, ct, RunSeconds).ConfigureAwait(True)
        Return New List(Of ExtrasDescarcat)()
    End Function

    ' One fake run: ten steps over `seconds`, the mid-run call at step five, a cancel honoured
    ' between steps (answered as a stopped workflow, not thrown).
    Private Async Function SimulateAsync(job As JobRequest, progress As IProgress(Of Integer),
                                         ct As CancellationToken, seconds As Double,
                                         Optional stopToken As CancellationToken = Nothing,
                                         Optional stepReport As Action(Of Integer, Integer) = Nothing) As Task(Of JobResult)
        Dim name As String = If(job?.WorkflowName, "Job")
        Dim cod As String = String.Empty
        If job IsNot Nothing Then job.Parameters.TryGetValue(WorkflowCatalog.VarCodAngajament, cod)
        Dim steps As Integer = 10
        Dim pause As Integer = CInt(Math.Max(50, seconds * 1000 / steps))
        RaiseEvent StatusUpdated(Me, $"[simulat] {name} pornește...")
        For i As Integer = 1 To steps
            If stopToken.IsCancellationRequested Then
                Return New JobResult With {.Success = False, .Message = "Oprit de operator (simulat)."}
            End If
            If ct.IsCancellationRequested Then
                RaiseEvent StatusUpdated(Me, $"[simulat] {name} anulat la pasul {i}.")
                Return New JobResult With {.Success = False, .Message = "Anulat de operator (simulat)."}
            End If
            stepReport?.Invoke(i - 1, steps)
            Try
                Await Task.Delay(pause, ct).ConfigureAwait(True)
            Catch ex As OperationCanceledException
                Continue For   ' answered at the top of the next step
            End Try
            progress?.Report(i * 100 \ steps)
            RaiseEvent StatusUpdated(Me, $"[simulat] {name}: pasul {i}/{steps}")
            If i = steps \ 2 AndAlso MidRunCall IsNot Nothing Then Await MidRunCall.Invoke().ConfigureAwait(True)
        Next
        stepReport?.Invoke(steps, steps)
        If FailRuns Then
            Return New JobResult With {.Success = False, .Message = "Workflow-ul simulat s-a oprit (eșec cerut din banc)."}
        End If
        Return AnswerFor(name, cod)
    End Function

    ' A «Prelucrare Completa»-shaped answer: the scalars and the three tables the ingest reads.
    Private Function AnswerFor(workflow As String, cod As String) As JobResult
        Dim result As New JobResult With {.Success = True, .Message = workflow}
        result.Data("CodAngajament") = cod
        result.Data("Stare") = "In derulare"
        result.Data("Descriere") = $"ANGAJAMENT DE PROBĂ {cod}"

        Dim istoric As New List(Of RandTabel)()
        Dim moment As Date = Date.Now.AddDays(-_rng.Next(5, 40))
        For i As Integer = 1 To _rng.Next(3, 7)
            moment = moment.AddHours(_rng.Next(2, 30))
            istoric.Add(Rand(("Timp", moment.ToString("dd.MM.yyyy HH:mm:ss")),
                             ("Utilizator", "operator.proba"),
                             ("Descriere", If(i = 1, "Creare angajament", "Modificare rezervare")),
                             ("Observatii", $"rând simulat {i}")))
        Next

        Dim indicatori As New List(Of RandTabel)()
        For i As Integer = 1 To _rng.Next(1, 4)
            indicatori.Add(Rand(("CodAI", $"10.01.{i:00}"),
                                ("Denumire", $"Indicator de probă {i}"),
                                ("Suma", (_rng.Next(1000, 90000) + 0.5).ToString("0.00", Globalization.CultureInfo.InvariantCulture))))
        Next

        Dim receptii As New List(Of RandTabel)()
        For i As Integer = 1 To _rng.Next(0, 3)
            receptii.Add(Rand(("DataR", Date.Today.AddDays(-i * 7).ToString("dd.MM.yyyy")),
                              ("Val_Receptie", (_rng.Next(100, 20000) + 0.25).ToString("0.00", Globalization.CultureInfo.InvariantCulture)),
                              ("Descriere", $"Recepție simulată {i}")))
        Next

        result.Tables("TabelIstoric") = New TabelRezultat(istoric)
        result.Tables("TabelIndicatori_results") = New TabelRezultat(indicatori)
        result.Tables("ListaReceptii_results") = New TabelRezultat(receptii)
        For Each t In result.Tables
            result.Data(t.Key) = $"[{t.Value.Count} rânduri]"
        Next
        Return result
    End Function

    Private Shared Function Rand(ParamArray cells As (Key As String, Value As String)()) As RandTabel
        Dim pairs As New List(Of KeyValuePair(Of String, CelulaTabel))()
        For Each c In cells
            pairs.Add(New KeyValuePair(Of String, CelulaTabel)(c.Key, CelulaTabel.DinText(c.Value)))
        Next
        Return New RandTabel(pairs)
    End Function

    ' ── Browser / page members: quiet, the queue never needs them ─────────────

    Public Function ShowBrowserAsync(owner As IWin32Window) As Task Implements IForexeRunner.ShowBrowserAsync
        Return Task.CompletedTask
    End Function

    Public Function HideBrowserAsync() As Task Implements IForexeRunner.HideBrowserAsync
        Return Task.CompletedTask
    End Function

    Public ReadOnly Property IsBrowserVisible As Boolean Implements IForexeRunner.IsBrowserVisible
        Get
            Return False
        End Get
    End Property

    Public Function DockBrowserAsync(host As Control) As Task Implements IForexeRunner.DockBrowserAsync
        Return Task.CompletedTask
    End Function

    Public Function ReleaseBrowserAsync(host As Control) As Task Implements IForexeRunner.ReleaseBrowserAsync
        Return Task.CompletedTask
    End Function

    Public Function SyncBrowserBoundsAsync() As Task Implements IForexeRunner.SyncBrowserBoundsAsync
        Return Task.CompletedTask
    End Function

    Public ReadOnly Property BrowserHost As Control Implements IForexeRunner.BrowserHost
        Get
            Return Nothing
        End Get
    End Property

    Public Function ReadPageAngajamentAsync() As Task(Of String) Implements IForexeRunner.ReadPageAngajamentAsync
        Return Task.FromResult(String.Empty)
    End Function

    Public Function ReadUncorrectedOperationsAsync() As Task(Of String) Implements IForexeRunner.ReadUncorrectedOperationsAsync
        Return Task.FromResult("{""found"":false,""pages"":0,""rows"":[]}")
    End Function

    Public Function ApplyPageConfigAsync() As Task Implements IForexeRunner.ApplyPageConfigAsync
        Return Task.CompletedTask
    End Function

    Public Function ReadPageElementsAsync() As Task(Of String) Implements IForexeRunner.ReadPageElementsAsync
        Return Task.FromResult("[]")
    End Function

    Public Function HighlightPageElementAsync(index As Integer) As Task(Of String) Implements IForexeRunner.HighlightPageElementAsync
        Return Task.FromResult(String.Empty)
    End Function

    Public Function CapturePaginaAsync(paginaOriginala As Boolean) As Task(Of Byte()) Implements IForexeRunner.CapturePaginaAsync
        Return Task.FromResult(Of Byte())(Nothing)
    End Function

    Public Function CaptureInfoCompleteAsync(paginaOriginala As Boolean) As Task(Of Byte()) Implements IForexeRunner.CaptureInfoCompleteAsync
        Return Task.FromResult(Of Byte())(Nothing)
    End Function

    Public Sub SetCapturaProvider(provider As Func(Of String, CancellationToken, Task(Of Boolean))) Implements IForexeRunner.SetCapturaProvider
    End Sub

    Public Function ResetShotSessionAsync() As Task Implements IForexeRunner.ResetShotSessionAsync
        Return Task.CompletedTask
    End Function

    Public Sub ShowRecorder(owner As IWin32Window) Implements IForexeRunner.ShowRecorder
    End Sub
End Class
#End If
