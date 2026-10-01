Option Strict On
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports KBot.Common
Imports Microsoft.Win32.SafeHandles

''' <summary>A print of a watched document, as the print queue showed it (slice 0099). POCO.</summary>
Public NotInheritable Class AdobePrintJob

    ''' <summary>The watched file the job was recognised as.</summary>
    Public ReadOnly Property DocumentPath As String
    Public ReadOnly Property Printer As String
    Public ReadOnly Property JobId As Integer
    ''' <summary>The job's name in the queue, as the printing application gave it.</summary>
    Public ReadOnly Property JobName As String
    ''' <summary>Pages spooled when the job was seen -- often 0, the job is still being written.</summary>
    Public ReadOnly Property TotalPages As Integer

    Public Sub New(documentPath As String, printer As String, jobId As Integer, jobName As String, totalPages As Integer)
        Me.DocumentPath = If(documentPath, "")
        Me.Printer = If(printer, "")
        Me.JobId = jobId
        Me.JobName = If(jobName, "")
        Me.TotalPages = totalPages
    End Sub

End Class

''' <summary>
''' Notices that a document shown in K-BOT's Adobe pane was sent to a printer (slice 0099).
'''
''' WHY THE PRINT QUEUE. K-BOT has no print command: the operator prints from inside Adobe (Ctrl+P,
''' Adobe's button, the right-click menu). Adobe's messages cannot be read from another process
''' without loading code into Adobe, which K-BOT never does (see
''' <see cref="AdobeNativeMethods.WINEVENT_OUTOFCONTEXT"/>). What every print leaves behind, however
''' it was started, is a JOB in a Windows print queue, named after the file. So the queues are
''' watched and a new job carrying the name of a watched file is one print. A print dialog that was
''' cancelled leaves no job and is not counted. «Microsoft Print to PDF» is a queue like any other.
'''
''' ONE WATCH FOR THE PROCESS. Every viewer registers the file it shows (<see cref="Watch"/>); one
''' background thread reads the queues for all of them. It exists only while a document is shown,
''' and never runs on the UI thread: a queue on a print server that does not answer can hold a
''' spooler call for seconds.
'''
''' WHEN THE QUEUES ARE READ:
'''  * at once when Windows says a job was added to a queue of this computer;
'''  * every <see cref="HotScanMs"/> for <see cref="HotAfterDialogMs"/> after Adobe's own Print
'''    window was seen (<see cref="NotePrintDialog"/>, called by <see cref="AdobeSaveTrap"/>) -- this
'''    is what catches a short job on a network printer, which Windows does not announce;
'''  * otherwise every <see cref="IdleScanMs"/>.
''' A pass that takes long stretches both intervals, so a slow print server is never asked without
''' pause.
'''
''' WHAT IS NOT COUNTED: jobs already in a queue when its watching started, jobs of another user of
''' the computer, jobs whose name is not a watched file's. Every job is counted once.
'''
''' NOT YET SEEN ON A PRINTER (01.10.2026): the name Adobe gives its jobs and the title of its Print
''' window are assumptions. Every decision is written to <c>adobe_preview.log</c>, so the first print
''' on a client PC shows whether they hold.
''' </summary>
Public NotInheritable Class AdobePrintWatcher

    Private Sub New()
    End Sub

    ''' <summary>How often the queues are read when nothing announces a print.</summary>
    Public Const IdleScanMs As Integer = 2000
    ''' <summary>How often they are read after Adobe's Print window was seen.</summary>
    Public Const HotScanMs As Integer = 250
    ''' <summary>For how long after the last sight of Adobe's Print window.</summary>
    Public Const HotAfterDialogMs As Integer = 60000
    ''' <summary>
    ''' A file is still watched this long after its viewer let it go: the job of a large document
    ''' can reach the queue after the operator has already clicked another row.
    ''' </summary>
    Public Const ReleaseGraceMs As Integer = 15000
    ' After a wake-up (a job announced, a kick), before the queues are read: gives the spooler time
    ' to fill the job in, and bounds the loop if a wake-up handle ever stayed signalled.
    Private Const WakeSettleMs As Integer = 100

    ' One registered file.
    Private NotInheritable Class PrintWatch
        Implements IDisposable

        Public ReadOnly FilePath As String
        Public ReadOnly Stem As String
        Public ReadOnly Callback As Action(Of AdobePrintJob)
        ' The thread the callback is posted to (Nothing = called on the watching thread).
        Public ReadOnly Ui As SynchronizationContext
        Public ReadOnly Order As Integer
        ' DateTime.MaxValue while the viewer shows the file; set when it lets go.
        Public RetireAtUtc As DateTime = DateTime.MaxValue

        Public Sub New(filePath As String, callback As Action(Of AdobePrintJob), ui As SynchronizationContext, order As Integer)
            Me.FilePath = filePath
            Me.Stem = Path.GetFileNameWithoutExtension(filePath)
            Me.Callback = callback
            Me.Ui = ui
            Me.Order = order
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Release(Me)
        End Sub
    End Class

    ' What the watching thread remembers about one queue.
    Private NotInheritable Class QueueState
        ' Jobs never looked at again: in the queue before the watch saw it, or already counted.
        Public ReadOnly Settled As New HashSet(Of Integer)()
        ' Jobs that are nobody's so far and were already written to the log.
        Public ReadOnly Noted As New HashSet(Of Integer)()
    End Class

    Private Shared ReadOnly _lock As New Object()
    Private Shared ReadOnly _watches As New List(Of PrintWatch)()
    Private Shared ReadOnly _kick As New AutoResetEvent(False)
    Private Shared _thread As Thread
    Private Shared _nextOrder As Integer
    ' UTC ticks until which the queues are read every HotScanMs.
    Private Shared _hotUntilTicks As Long
    ' Print windows already reported. UI thread only (the traps' sweeps).
    Private Shared ReadOnly _dialogs As New HashSet(Of IntPtr)()

    ''' <summary>
    ''' Starts watching for prints of <paramref name="pdfPath"/>. <paramref name="onPrinted"/> is
    ''' called once per print job, on the thread that called Watch when that thread has a
    ''' synchronization context (the UI thread), otherwise on the watching thread. Dispose the result
    ''' when the file is no longer shown; prints seen within <see cref="ReleaseGraceMs"/> after that
    ''' are still reported. Boundary (thread start): log and rethrow.
    ''' </summary>
    Public Shared Function Watch(pdfPath As String, onPrinted As Action(Of AdobePrintJob)) As IDisposable
        Try
            If String.IsNullOrWhiteSpace(pdfPath) Then Throw New ArgumentException("Empty path.", NameOf(pdfPath))
            ArgumentNullException.ThrowIfNull(onPrinted)
            Dim entry As PrintWatch
            SyncLock _lock
                _nextOrder += 1
                entry = New PrintWatch(pdfPath, onPrinted, SynchronizationContext.Current, _nextOrder)
                If String.IsNullOrEmpty(entry.Stem) Then Throw New ArgumentException("The path has no file name.", NameOf(pdfPath))
                _watches.Add(entry)
                If _thread Is Nothing Then
                    _thread = New Thread(AddressOf Run) With {.IsBackground = True, .Name = "KBot print queue watch"}
                    _thread.Start()
                End If
            End SyncLock
            Return entry
        Catch ex As Exception
            GlobalErrorLog.Write("AdobePrintWatcher.Watch", ex)
            Throw
        End Try
    End Function

    ' The viewer let the file go: watched for the grace period, then dropped by the thread.
    Private Shared Sub Release(entry As PrintWatch)
        SyncLock _lock
            If entry.RetireAtUtc = DateTime.MaxValue Then entry.RetireAtUtc = DateTime.UtcNow.AddMilliseconds(ReleaseGraceMs)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Adobe's own Print window is on screen (<see cref="AdobePrintJobFilter.IsPrintDialogTitle"/>):
    ''' from now and for <see cref="HotAfterDialogMs"/> after its last sight the queues are read every
    ''' <see cref="HotScanMs"/>. Called by the traps on every sweep that sees the window; UI thread.
    ''' The window says a print MAY follow -- only a job in a queue is counted. Never throws: it is
    ''' called from inside a trap's sweep, which must go on.
    ''' </summary>
    Public Shared Sub NotePrintDialog(hwnd As IntPtr, title As String)
        Try
            Interlocked.Exchange(_hotUntilTicks, DateTime.UtcNow.AddMilliseconds(HotAfterDialogMs).Ticks)
            _dialogs.RemoveWhere(Function(h) Not AdobeNativeMethods.IsWindow(h))
            If Not _dialogs.Add(hwnd) Then Return
            AdobeHostLog.Write($"Tipărire: fereastra de tipărire Adobe e deschisă (0x{hwnd.ToInt64():X}, «{title}») — " &
                               $"coada de tipărire se citește la {HotScanMs} ms.")
            _kick.Set()
        Catch ex As Exception
            GlobalErrorLog.Write("AdobePrintWatcher.NotePrintDialog", ex)
        End Try
    End Sub

    ' ── The watching thread ─────────────────────────────────────────────────────

    ' Thread boundary: log and swallow. A later Watch starts a new thread.
    Private Shared Sub Run()
        Dim server As IntPtr = IntPtr.Zero
        Dim change As IntPtr = IntPtr.Zero
        Dim changeEvent As ManualResetEvent = Nothing
        Try
            Dim openError As Integer
            change = PrintSpooler.OpenAddJobNotification(server, openError)
            If change = IntPtr.Zero Then
                Log($"Tipărire: Windows nu anunță lucrările noi (eroarea {openError}); cozile se citesc la {IdleScanMs} ms.")
            Else
                ' The spooler's handle is waited on like any other; it is closed by PrintSpooler.
                changeEvent = New ManualResetEvent(False)
                changeEvent.SafeWaitHandle = New SafeWaitHandle(change, ownsHandle:=False)
            End If
            Log("Tipărire: urmărirea cozilor de tipărire a pornit.")

            Dim queues As New Dictionary(Of String, QueueState)(StringComparer.OrdinalIgnoreCase)
            ' Queues whose failure was already written to the log (said once, again after a recovery).
            Dim failing As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim listFailing As Boolean = False
            Dim slowSaid As Boolean = False

            Do
                If Not AnyWatchLeft() Then Exit Do

                Dim clock As Stopwatch = Stopwatch.StartNew()
                Dim scan As SpoolerScan = PrintSpooler.Scan()
                Dim scanMs As Integer = CInt(Math.Min(clock.ElapsedMilliseconds, 60000L))
                If scanMs > 1000 AndAlso Not slowSaid Then
                    slowSaid = True
                    Log($"Tipărire: citirea cozilor a durat {scanMs} ms (o imprimantă de rețea răspunde greu); " &
                        "citirile se răresc pe măsură.")
                End If

                Dim hot As Boolean = DateTime.UtcNow.Ticks < Interlocked.Read(_hotUntilTicks)
                Apply(scan, queues, failing, listFailing, hot)

                Dim pause As Integer = If(hot, Math.Max(HotScanMs, scanMs * 2), Math.Max(IdleScanMs, scanMs * 10))
                Dim waitOn As WaitHandle() = If(changeEvent Is Nothing,
                                                New WaitHandle() {_kick},
                                                New WaitHandle() {_kick, changeEvent})
                Dim signalled As Integer = WaitHandle.WaitAny(waitOn, pause)
                If signalled = WaitHandle.WaitTimeout Then Continue Do

                If signalled = 1 Then
                    Dim ackError As Integer
                    If Not PrintSpooler.AcknowledgeChange(change, ackError) Then
                        Log($"Tipărire: Windows nu mai anunță lucrările noi (eroarea {ackError}); cozile se citesc la {IdleScanMs} ms.")
                        changeEvent.Dispose()
                        changeEvent = Nothing
                        PrintSpooler.CloseAddJobNotification(change, server)
                        change = IntPtr.Zero
                        server = IntPtr.Zero
                    End If
                End If
                Thread.Sleep(WakeSettleMs)
            Loop
            Log("Tipărire: niciun document urmărit — urmărirea cozilor de tipărire s-a oprit.")
        Catch ex As Exception
            GlobalErrorLog.Write("AdobePrintWatcher.Run", ex)
            Log("ATENȚIE: urmărirea cozilor de tipărire s-a oprit pe o eroare (vezi jurnalul de erori); " &
                "repornește la următorul document deschis. Tipăririle de până atunci NU se numără.")
        Finally
            Try
                changeEvent?.Dispose()
                PrintSpooler.CloseAddJobNotification(change, server)
            Catch ex As Exception
                GlobalErrorLog.Write("AdobePrintWatcher.Run.Close", ex)
            End Try
            SyncLock _lock
                If _thread Is Thread.CurrentThread Then _thread = Nothing
            End SyncLock
        End Try
    End Sub

    ' Drops the files released long enough ago. False = nothing left to watch: the thread ends, and
    ' says so under the same lock Watch takes, so a file registered now gets a new thread.
    Private Shared Function AnyWatchLeft() As Boolean
        SyncLock _lock
            Dim now As DateTime = DateTime.UtcNow
            _watches.RemoveAll(Function(w) w.RetireAtUtc <= now)
            If _watches.Count > 0 Then Return True
            If _thread Is Thread.CurrentThread Then _thread = Nothing
            Return False
        End SyncLock
    End Function

    ' One pass over what the spooler showed. Reached from Run (wrapped).
    Private Shared Sub Apply(scan As SpoolerScan, queues As Dictionary(Of String, QueueState),
                             failing As HashSet(Of String), ByRef listFailing As Boolean, hot As Boolean)
        If Not scan.PrintersListed Then
            If Not listFailing Then
                listFailing = True
                Log($"ATENȚIE: imprimantele acestui calculator nu pot fi enumerate (eroarea {scan.ListError}); " &
                    "tipăririle nu se pot număra până nu răspunde serviciul de tipărire.")
            End If
            Return
        End If
        listFailing = False

        For Each bad As KeyValuePair(Of String, Integer) In scan.Failed
            If failing.Add(bad.Key) Then
                Log($"Tipărire: coada imprimantei «{bad.Key}» nu poate fi citită (eroarea {bad.Value}); " &
                    "tipăririle pe ea nu se văd până nu răspunde.")
            End If
        Next

        ' A printer that is gone is forgotten; one whose queue merely failed keeps what was known.
        For Each gone As String In queues.Keys.Where(
            Function(k) Not scan.Queues.ContainsKey(k) AndAlso Not scan.Failed.ContainsKey(k)).ToList()
            queues.Remove(gone)
        Next

        For Each queue As KeyValuePair(Of String, List(Of SpoolerJob)) In scan.Queues
            failing.Remove(queue.Key)
            Dim current As New HashSet(Of Integer)(queue.Value.Select(Function(j) j.JobId))
            Dim state As QueueState = Nothing
            If Not queues.TryGetValue(queue.Key, state) Then
                ' First sight of this queue: what is already in it was not printed under this watch.
                state = New QueueState()
                state.Settled.UnionWith(current)
                queues(queue.Key) = state
                Continue For
            End If
            For Each job As SpoolerJob In queue.Value
                If state.Settled.Contains(job.JobId) Then Continue For
                ' A job nobody owns is asked about again on the next pass (its name can be filled
                ' in late), but written to the log only once.
                If Deliver(job) Then
                    state.Settled.Add(job.JobId)
                ElseIf state.Noted.Add(job.JobId) Then
                    NoteUnmatched(job, hot)
                End If
            Next
            state.Settled.IntersectWith(current)
            state.Noted.IntersectWith(current)
        Next
    End Sub

    ' True when the job is a print of a watched file: its viewer is told (once). The file with the
    ' longest matching name wins, then the one still on screen, then the newest.
    Private Shared Function Deliver(job As SpoolerJob) As Boolean
        If Not AdobePrintJobFilter.SameUser(job.User, Environment.UserName) Then Return False
        Dim target As PrintWatch
        SyncLock _lock
            target = _watches.
                Where(Function(w) AdobePrintJobFilter.JobMatches(job.Document, w.FilePath)).
                OrderByDescending(Function(w) w.Stem.Length).
                ThenByDescending(Function(w) w.RetireAtUtc).
                ThenByDescending(Function(w) w.Order).
                FirstOrDefault()
        End SyncLock
        If target Is Nothing Then Return False

        Log($"Tipărire detectată: «{Path.GetFileName(target.FilePath)}» — imprimanta «{job.Printer}», " &
            $"lucrarea {job.JobId} «{job.Document}».")
        Dim seen As New AdobePrintJob(target.FilePath, job.Printer, job.JobId, job.Document, job.TotalPages)
        Dim tell As SendOrPostCallback =
            Sub(unused As Object)
                Try
                    target.Callback.Invoke(seen)
                Catch ex As Exception
                    GlobalErrorLog.Write("AdobePrintWatcher.Callback", ex)
                End Try
            End Sub
        Try
            If target.Ui Is Nothing Then
                tell(Nothing)
            Else
                target.Ui.Post(tell, Nothing)
            End If
        Catch ex As Exception
            ' The viewer's thread is gone (the window closed): the job stays settled, not retried.
            GlobalErrorLog.Write("AdobePrintWatcher.Deliver", ex)
        End Try
        Return True
    End Function

    ' A new job that is no watched file's. Written only when it is likely Adobe's -- right after its
    ' Print window, or a PDF by name -- so the log shows what Adobe calls its jobs without listing
    ' everything else the operator prints.
    Private Shared Sub NoteUnmatched(job As SpoolerJob, hot As Boolean)
        If Not hot AndAlso job.Document.IndexOf(".pdf", StringComparison.OrdinalIgnoreCase) < 0 Then Return
        Dim mine As Boolean = AdobePrintJobFilter.SameUser(job.User, Environment.UserName)
        Log($"Tipărire: lucrare nouă pe «{job.Printer}» — {job.JobId} «{job.Document}»" &
            If(mine, "", $" (a utilizatorului {job.User})") &
            ": nu poartă numele unui document deschis în K-BOT, nu se numără.")
    End Sub

    Private Shared Sub Log(line As String)
        AdobeHostLog.Write(line)
    End Sub

End Class
