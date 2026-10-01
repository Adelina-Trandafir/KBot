Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe

''' <summary>One angajament of a multi-thread download, and the receptions the operator chose to skip (slice 0100).</summary>
Public NotInheritable Class ParallelNodeRequest
    Public Property Cod As String = String.Empty
    ''' <summary>Nothing / empty = every reception is read.</summary>
    Public Property Sarite As ReceptiiSarite
End Class

''' <summary>
''' What one angajament of a multi-thread download came to (slice 0100). <see cref="Pachet"/> is
''' Nothing when the download failed -- <see cref="Failure"/> then says why, in Romanian.
''' </summary>
Public NotInheritable Class ParallelNodeResult
    Public Property Cod As String = String.Empty
    Public Property Pachet As PrelucrareRezultat
    Public Property Failure As String = String.Empty
End Class

''' <summary>
''' Slice 0100: downloading several angajamente at once. The robot part -- one FOREXE tab per
''' running download, a FIFO queue behind them -- is <c>IForexeRunner.RunJobsParallelAsync</c>; this
''' is the coordinator's half: the jobs go in, and when EVERY download has ended the answers come
''' out as packages saved on disk, in the order they finished. The ingest of those packages is the
''' shell's, one at a time (KbotForm.Download.vb).
''' </summary>
Partial Public NotInheritable Class ForexeController

    ''' <summary>
    ''' Downloads <paramref name="requests"/> concurrently (at most <paramref name="maxThreads"/>
    ''' tabs, never more than 10) and returns one result per request in the order they FINISHED, or
    ''' Nothing when nothing could start (busy, no session, the operator cancelled the certificate):
    ''' <see cref="LastFailure"/> says why. Writes nothing to the server; a failed or timed-out
    ''' download is its own result and never stops the others.
    ''' </summary>
    ''' <param name="citesteIstoric">
    ''' The LOCAL history read that decides forward / REVERSE for each angajament (see
    ''' <see cref="DownloadNodeAsync"/>); done here, before the robot starts, because the server
    ''' gate is closed while it runs.
    ''' </param>
    Public Async Function DownloadNodesParallelAsync(
            requests As IReadOnlyList(Of ParallelNodeRequest),
            maxThreads As Integer,
            citesteIstoric As Func(Of String, CancellationToken, Task(Of IstoricInfo))) As Task(Of List(Of ParallelNodeResult))
        Try
            ArgumentNullException.ThrowIfNull(requests)
            _ultimulEsec = String.Empty
            If requests.Count = 0 Then Return New List(Of ParallelNodeResult)()
            If _busy Then
                RaporteazaEsec("Rulează deja o operație FOREXE — descărcarea multiplă a fost ignorată.")
                Return Nothing
            End If

            ' Replay answers one file at a time, through the operator's dialogs: no tabs to share out.
            If _replayMode Then Return Await DownloadNodesOneByOneAsync(requests, citesteIstoric)

            If Not Await AsiguraSesiuneAsync() Then Return Nothing

            IntraInLucru()
            Try
                Dim sarite As New Dictionary(Of String, ReceptiiSarite)(StringComparer.OrdinalIgnoreCase)
                Dim jobs As New List(Of JobRequest)()
                For Each rq As ParallelNodeRequest In requests
                    If String.IsNullOrWhiteSpace(rq.Cod) OrElse sarite.ContainsKey(rq.Cod) Then Continue For
                    sarite(rq.Cod) = rq.Sarite
                    Dim ultimaData As Date? = Await UltimaDataIstoric(rq.Cod, citesteIstoric)
                    Dim job As JobRequest =
                        If(ultimaData.HasValue,
                           JobBuilder.BuildPrelucrareCompletaReverse(rq.Cod, ultimaData.Value, rq.Sarite?.Zile),
                           JobBuilder.BuildPrelucrareCompleta(rq.Cod, rq.Sarite?.Zile))
                    job.StopBeforeSave = _dryRunMode
                    jobs.Add(job)
                Next

                Dim fire As Integer = Math.Min(Math.Max(1, maxThreads), AppSettings.DownloadThreadsMax)
                RaporteazaStare($"Descarc {jobs.Count} angajamente, cel mult {fire} deodată (câte un tab FOREXE)...")

                Dim raportProgres As IProgress(Of Integer) = Progres()
                Dim gata As Integer = 0
                Dim total As Integer = jobs.Count
                Dim terminat As Action(Of ParallelJobOutcome) =
                    Sub(o)
                        Dim n As Integer = Interlocked.Increment(gata)
                        raportProgres.Report(CInt(Math.Min(100L, 100L * n \ total)))
                    End Sub

                Dim outcomes As List(Of ParallelJobOutcome) =
                    Await RunGatedAsync(Function() _runner.RunJobsParallelAsync(jobs, fire, terminat, _cts.Token))

                ' ALL downloads have ended. Only now are the answers worked through, one at a time.
                Dim rezultate As New List(Of ParallelNodeResult)()
                For Each o As ParallelJobOutcome In outcomes
                    rezultate.Add(ProceseazaRaspunsul(o, sarite))
                Next
                Dim bune As Integer = rezultate.Where(Function(r) r.Pachet IsNot Nothing).Count()
                RaporteazaStare($"Descărcarea multiplă s-a încheiat: {bune} reușite din {rezultate.Count}; urmează ingestia, una câte una.")
                Return rezultate
            Finally
                IesDinLucru()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeController.DownloadNodesParallelAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' One finished download -> its black box on disk, its kept answer, and (when it succeeded) its
    ''' package saved locally. Never throws: a problem here is that angajament's failure.
    ''' </summary>
    Private Function ProceseazaRaspunsul(o As ParallelJobOutcome,
                                         sarite As Dictionary(Of String, ReceptiiSarite)) As ParallelNodeResult
        Dim cod As String = String.Empty
        If o.Job IsNot Nothing Then o.Job.Parameters.TryGetValue(WorkflowCatalog.VarCodAngajament, cod)
        Dim rezultat As New ParallelNodeResult With {.Cod = cod}
        Dim jurnal As New ForexeRunDump("PrelucrareCompleta", cod, _session)
        Try
            jurnal.NoteRequest(o.Job)
            jurnal.Note("tab_forexe", o.Worker.ToString(Globalization.CultureInfo.InvariantCulture))
            jurnal.Note("durata", (o.FinishedAt - o.StartedAt).ToString("hh\:mm\:ss"))
            Dim job As JobResult = o.Result

            ' The answer is kept on disk whatever the outcome, like every other run (ForexeAnswerStore).
            If job IsNot Nothing AndAlso o.Worker > 0 Then
                Dim filePath As String = ForexeAnswerStore.Save(o.Job, cod, _session, job)
                If String.IsNullOrEmpty(filePath) Then
                    RaporteazaStare($"«{cod}»: răspunsul FOREXE nu s-a putut păstra în «Rezultate_Forexe» — vezi Logs\harness_errors.log.")
                End If
            End If

            If job Is Nothing OrElse Not job.Success Then
                Dim motiv As String = If(job Is Nothing, "fără răspuns", job.Message)
                ScrieJurnal(jurnal, "esuat", job)
                rezultat.Failure = $"Prelucrarea lui «{cod}» a eșuat: " & motiv
                RaporteazaStare(rezultat.Failure)
                Return rezultat
            End If

            Dim skip As ReceptiiSarite = Nothing
            sarite.TryGetValue(cod, skip)
            Dim pachet As PrelucrareRezultat =
                WorkflowResultStore.FaraReceptiileSarite(WorkflowResultStore.DinJobResult(cod, job), skip)
            Dim cale As String = _store.SalveazaNod(cod, pachet)
            Dim totalRanduri As Integer = pachet.Tabele.Values.Sum(Function(t) t.Count)
            ScrieJurnal(jurnal, "ok", job, pachet)
            RaporteazaStare($"«{cod}»: {pachet.Tabele.Count} tabele, {totalRanduri} rânduri → {Path.GetFileName(cale)}")
            rezultat.Pachet = pachet
            Return rezultat
        Catch ex As Exception
            jurnal.Note("exceptie", ex.ToString())
            ScrieJurnal(jurnal, "exceptie", Nothing)
            GlobalErrorLog.Write("ForexeController.ProceseazaRaspunsul", ex)
            rezultat.Pachet = Nothing
            rezultat.Failure = $"Prelucrarea lui «{cod}» a eșuat: " & ex.Message
            RaporteazaStare(rezultat.Failure)
            Return rezultat
        End Try
    End Function

    ''' <summary>The replay mode's road: the ordinary one-at-a-time download for each request.</summary>
    Private Async Function DownloadNodesOneByOneAsync(
            requests As IReadOnlyList(Of ParallelNodeRequest),
            citesteIstoric As Func(Of String, CancellationToken, Task(Of IstoricInfo))) As Task(Of List(Of ParallelNodeResult))
        Dim rezultate As New List(Of ParallelNodeResult)()
        For Each rq As ParallelNodeRequest In requests
            Dim pachet As PrelucrareRezultat = Await DownloadNodeAsync(rq.Cod, citesteIstoric, rq.Sarite)
            rezultate.Add(New ParallelNodeResult With {
                .Cod = rq.Cod, .Pachet = pachet,
                .Failure = If(pachet Is Nothing, If(String.IsNullOrEmpty(_ultimulEsec),
                                                    $"«{rq.Cod}» nu s-a descărcat.", _ultimulEsec), String.Empty)})
        Next
        Return rezultate
    End Function

End Class
