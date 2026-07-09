Imports System
Imports System.IO
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common      ' SessionContext (token-ul bearer al sesiunii).
Imports Newtonsoft.Json.Linq
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

        ' Sesiunea K-BOT (singleton DI): sursa token-ului bearer pe care executorul
        ' îl trimite la API (parseExcel). Se citește lazy, per-cerere, ca să
        ' urmărească re-login-ul.
        Private ReadOnly _session As SessionContext

        Public Sub New(session As SessionContext)
            _session = session
        End Sub

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
                    stealthMode:=False,   ' Browser ON-screen și mereu vizibil (fără off-screen -3000, fără ascundere din taskbar).
                    stepByStep:=False,
                    confirmStep:=Nothing,
                    stepOnlyCheckpoints:=False,
                    progressCallback:=progressAction,
                    useSnapAssist:=False,
                    cancellationToken:=ct)

                ' PIN MANUAL (decizie A3): utilizatorul tastează PIN-ul în dialogul Windows.
                ' Niciun SendKeys de PIN.
                _executor.ManualPinMode = True

                ' Token-ul bearer al sesiunii K-BOT pentru apelurile API din workflow
                ' (parseExcel). Provider, nu valoare: un re-login în timpul sesiunii
                ' de browser furnizează automat token-ul nou.
                _executor.SetSessionTokenProvider(Function() _session.Token)

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
                ' DIAGNOSTIC TEMPORAR: stack trace complet în log (LogException scrie doar Message).
                _logger.LogError("[DIAG] " & ex.GetType().FullName & ": " & ex.Message)
                _logger.LogError("[DIAG][STACK] " & ex.ToString())
                ' Browserul rămâne deschis pentru investigație (decizie A3).
                Return Failed(ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' Rulează un workflow pe sesiunea EXISTENTĂ (fără relansare de browser).
        ''' Injectează job.Parameters (JSON -> SetVariable, plate -> ApplyVariables,
        ''' ca în KBOT_IPC.WorkFlow), execută .wfl-ul și întoarce variabilele
        ''' executorului în JobResult (Data plat + Tables tabelar).
        ''' </summary>
        Public Async Function RunJobAsync(job As JobRequest,
                                          progress As IProgress(Of Integer),
                                          ct As CancellationToken) As Task(Of JobResult) Implements IForexeRunner.RunJobAsync

            If _logger Is Nothing Then
                Throw New InvalidOperationException("Logger neatașat — apelează AttachLogger înainte de RunJobAsync.")
            End If
            If _executor Is Nothing OrElse Not _executor.IsBrowserOpen Then
                Throw New InvalidOperationException("Nicio sesiune activă — rulează Conectare (RunAsync) înainte de RunJobAsync.")
            End If

            Try
                If String.IsNullOrEmpty(job.WflPath) OrElse Not File.Exists(job.WflPath) Then
                    Return Failed($"Fișierul workflow lipsește: {job.WflPath}")
                End If

                ct.ThrowIfCancellationRequested()

                Dim xml As String = File.ReadAllText(job.WflPath)

                ' Injectare variabile — separat pe tip (ca în KBOT_IPC.WorkFlow):
                ' JSON -> executor (SetVariable), plate -> substituție în XML (ApplyVariables).
                If job.Parameters IsNot Nothing AndAlso job.Parameters.Count > 0 Then
                    Dim varMeta As Dictionary(Of String, WorkflowVariable) = WorkflowParser.ExtractVariablesDetailed(xml)
                    Dim flatVars As New Dictionary(Of String, String)
                    For Each kvp In job.Parameters
                        Dim meta As WorkflowVariable = Nothing
                        Dim isJson As Boolean = varMeta.TryGetValue(kvp.Key, meta) AndAlso
                                                meta.VarType.Equals("JSON", StringComparison.OrdinalIgnoreCase)
                        If isJson Then
                            _executor.SetVariable(kvp.Key, kvp.Value)
                        Else
                            flatVars(kvp.Key) = kvp.Value
                        End If
                    Next
                    If flatVars.Count > 0 Then xml = WorkflowParser.ApplyVariables(xml, flatVars)
                End If

                WorkflowParser.Logger = _logger
                Dim workflow As Workflow = WorkflowParser.Parse(xml, job.WflPath)
                _executor.SetWorkflowPath(job.WflPath)

                _logger.LogInfo($"Rulez workflow-ul '{job.WorkflowName}' pe sesiunea existentă...")
                Await Task.Run(Function() _executor.ExecuteAsync(workflow))

                Dim result As New JobResult With {.Success = True, .Message = $"'{job.WorkflowName}' rulat."}
                PopulateResult(result)
                progress?.Report(100)
                Return result

            Catch ex As OperationCanceledException
                _logger.LogWarning($"'{job.WorkflowName}' anulat.")
                Return Failed($"'{job.WorkflowName}' anulat.")

            Catch ex As Exception
                _logger.LogException(ex, $"Eroare rulare '{job.WorkflowName}'")
                _logger.LogError("[DIAG] " & ex.GetType().FullName & ": " & ex.Message)
                _logger.LogError("[DIAG][STACK] " & ex.ToString())
                Return Failed(ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' Copiază variabilele executorului în JobResult: toate ca Data plat, iar
        ''' cele care conțin un JSON array de obiecte și în Tables (rând = coloană->valoare).
        ''' </summary>
        Private Sub PopulateResult(result As JobResult)
            Dim vars As Dictionary(Of String, String) = _executor.GetAllVariables()
            For Each kvp In vars
                result.Data(kvp.Key) = kvp.Value
                Dim table As List(Of Dictionary(Of String, String)) = TryParseTable(kvp.Value)
                If table IsNot Nothing Then result.Tables(kvp.Key) = table
            Next
        End Sub

        ''' <summary>
        ''' Parsează un JSON array de obiecte în listă de rânduri. Întoarce Nothing
        ''' dacă valoarea nu e un array de obiecte (clasificare, nu eroare — la fel ca
        ''' detecția JSON din WorkflowExecutor.GetAllVariables).
        ''' Public: this is the single raw-JSON -> Tables() parsing seam; the harness
        ''' test ListaAngajamenteEnrichmentTest exercises it directly.
        ''' </summary>
        Public Shared Function TryParseTable(value As String) As List(Of Dictionary(Of String, String))
            If String.IsNullOrWhiteSpace(value) Then Return Nothing
            Dim trimmed As String = value.Trim()
            If Not trimmed.StartsWith("["c) Then Return Nothing

            Dim token As JToken
            Try
                token = JToken.Parse(trimmed)
            Catch
                Return Nothing   ' nu e JSON valid -> nu e tabel
            End Try

            Dim arr As JArray = TryCast(token, JArray)
            If arr Is Nothing OrElse arr.Count = 0 Then Return Nothing

            Dim rows As New List(Of Dictionary(Of String, String))
            For Each item In arr
                Dim obj As JObject = TryCast(item, JObject)
                If obj Is Nothing Then Return Nothing   ' array de valori, nu de obiecte
                Dim row As New Dictionary(Of String, String)
                For Each prop In obj.Properties()
                    row(prop.Name) = If(prop.Value?.ToString(), String.Empty)
                Next
                rows.Add(row)
            Next
            Return rows
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
