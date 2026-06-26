Imports System
Imports System.IO
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
Imports WorkflowModels   ' Workflow (modelele). WorkflowExecutor e în namespace global.

Namespace KBot.Forexe

    ''' <summary>
    ''' Rulează workflow-uri .wfl in-process prin WorkflowExecutor.
    ''' Singleton cu stare: după o conectare reușită ține executorul (deci sesiunea
    ''' browser/autentificarea) viu pentru job-urile următoare. Conectarea NU închide browserul.
    ''' </summary>
    Public Class ForexeRunner
        Implements IForexeRunner

        Private _logger As RichTextBoxLogger
        Private _executor As WorkflowExecutor

        ''' <summary>
        ''' Leagă logger-ul FOREXE (panoul de log K-BOT). Apelat o singură dată, după ce panoul există.
        ''' RichTextBoxLogger cere RichTextBox-ul la construcție, deci nu poate fi injectat în ctor
        ''' (runner-ul e singleton creat înainte de MainForm). Aici e seam-ul pentru viitorul
        ''' logger-fișier centralizat (se va atașa o variantă compusă).
        ''' </summary>
        Public Sub AttachLogger(logger As RichTextBoxLogger)
            _logger = logger
        End Sub

        ''' <summary>True dacă există o sesiune (executor cu browser deschis).</summary>
        Public ReadOnly Property HasLiveSession As Boolean
            Get
                Return _executor IsNot Nothing AndAlso _executor.IsBrowserOpen
            End Get
        End Property

        Public Async Function RunAsync(job As JobRequest,
                                       certificate As X509Certificate2,
                                       progress As IProgress(Of Integer),
                                       ct As CancellationToken) As Task(Of JobResult) Implements IForexeRunner.RunAsync

            If _logger Is Nothing Then
                Throw New InvalidOperationException("Logger neatașat — apelează AttachLogger înainte de RunAsync.")
            End If

            ' Conectarea forțează întotdeauna o sesiune nouă.
            Await DisposeExecutorAsync()

            Try
                If String.IsNullOrEmpty(job.WflPath) OrElse Not File.Exists(job.WflPath) Then
                    Return Failed($"Fișierul workflow lipsește: {job.WflPath}")
                End If

                ' Bridge progres: (currentStep, totalSteps) -> procent 0..100
                Dim progressAction As Action(Of Integer, Integer) =
                    Sub(currentStep As Integer, totalSteps As Integer)
                        If progress Is Nothing Then Return
                        Dim pct As Integer = If(totalSteps > 0, CInt(Math.Min(100, currentStep / totalSteps * 100)), 0)
                        progress.Report(pct)
                    End Sub

                _executor = New WorkflowExecutor(
                    logger:=_logger,
                    certificate:=certificate,
                    stealthMode:=True,
                    stepByStep:=False,
                    confirmStep:=Nothing,
                    stepOnlyCheckpoints:=False,
                    progressCallback:=progressAction,
                    useSnapAssist:=False,
                    cancellationToken:=ct)

                ' PIN MANUAL (decizie A3): utilizatorul tastează PIN-ul în dialogul Windows.
                ' Niciun SendKeys de PIN.
                _executor.ManualPinMode = True

                AddHandler _executor.OnStatusUpdate, AddressOf OnExecutorStatus
                AddHandler _executor.OnBrowserClosed, AddressOf OnExecutorBrowserClosed

                _logger.LogInfo("Lansare browser...")
                Await Task.Run(Function() _executor.LaunchAndPositionBrowserAsync())

                _logger.LogInfo("Autentificare...")
                Dim xml As String = File.ReadAllText(job.WflPath)
                WorkflowParser.Logger = _logger
                Dim workflow As Workflow = WorkflowParser.Parse(xml, job.WflPath)
                _executor.SetWorkflowPath(job.WflPath)

                Await Task.Run(Function() _executor.ExecuteAsync(workflow))

                _logger.LogSuccess("Conectare reușită!")
                Return New JobResult With {.Success = True, .Message = "Conectare reușită."}

            Catch ex As OperationCanceledException
                _logger.LogWarning("Conectare anulată.")
                Return Failed("Conectare anulată.")

            Catch ex As Exception
                _logger.LogException(ex, "Eroare conectare")
                ' Browserul rămâne deschis pentru investigație (decizie A3).
                Return Failed(ex.Message)
            End Try
        End Function

        Private Sub OnExecutorStatus(status As String)
            _logger.LogInfo(status)
        End Sub

        Private Sub OnExecutorBrowserClosed(message As String)
            _logger.LogWarning($"Browser închis: {message}")
        End Sub

        Private Shared Function Failed(message As String) As JobResult
            Return New JobResult With {.Success = False, .Message = message}
        End Function

        Private Async Function DisposeExecutorAsync() As Task
            If _executor Is Nothing Then Return
            Try
                RemoveHandler _executor.OnStatusUpdate, AddressOf OnExecutorStatus
                RemoveHandler _executor.OnBrowserClosed, AddressOf OnExecutorBrowserClosed
                Await _executor.CloseAsync()
            Catch
                ' ignorăm erorile de cleanup
            Finally
                _executor = Nothing
            End Try
        End Function

    End Class

End Namespace
