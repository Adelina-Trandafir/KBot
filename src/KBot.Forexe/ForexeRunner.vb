Imports System
Imports System.IO
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
Imports GeneralClasses   ' JobHistoryManager (istoricul lucrărilor FOREXE).
Imports KBot.Common      ' ExcelJob (payload-ul parseExcel dat procesorului).
Imports KBot.Domain      ' CelulaTabel / RandTabel / TabelRezultat (decizia D-N).
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
        ' Gardianul ferestrei de PIN (port din KBOT_IPC): cedează prim-planul cât e PIN-ul pe
        ' ecran și îl repune după. Trăiește cât sesiunea; se oprește singur la anularea token-ului.
        Private _guardian As PinWindowGuardian

        ' Procesorul Excel (bridge DI către ApiClient.ProcessExcelAsync): executorul
        ' primește prin el conversia Excel->JSON fără ca FOREXE să depindă de KBot.Api.
        ' Token-ul bearer și adresa stau în ApiClient; re-login-ul e transparent aici.
        Private ReadOnly _excelProcessor As Func(Of ExcelJob, CancellationToken, Task(Of String))

        Public Sub New(excelProcessor As Func(Of ExcelJob, CancellationToken, Task(Of String)))
            _excelProcessor = excelProcessor
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

        ''' <summary>
        ''' LINIA DE STARE, retransmisă gazdelor — echivalentul exact al lui <c>lblStatus</c> din
        ''' <c>KBOT_IPC</c> (felia 0040).
        '''
        ''' <para>Ce trece prin ea, la fel ca acolo: frazele de FAZĂ ale robotului («Lansare
        ''' browser...», «Execut: X.wfl...», «În așteptare...») și mesajele <c>OnLogMessage</c> ale
        ''' executorului — adică pașii <c>Log</c> scriși în workflow și rândurile de progres ale
        ''' scrapingului. Astea sunt propoziții pentru operator.</para>
        '''
        ''' <para>Ce NU mai trece: <c>OnStatusUpdate</c>, adică «[MAIN] Pasul 7/23: Click [#btn]».
        ''' Aceea e trasarea pas cu pas a motorului, iar în KBOT_IPC nu a ajuns niciodată în
        ''' <c>lblStatus</c> — rămâne unde îi e locul, în jurnalul consolei.</para>
        ''' </summary>
        Public Event StatusUpdated As EventHandler(Of String) Implements IForexeRunner.StatusUpdated

        ''' <summary>True dacă există o sesiune (executor cu browser deschis).</summary>
        Public ReadOnly Property HasLiveSession As Boolean Implements IForexeRunner.HasLiveSession
            Get
                Return _executor IsNot Nothing AndAlso _executor.IsBrowserOpen
            End Get
        End Property

        ''' <summary>
        ''' Aduce fereastra browserului în față. Fără sesiune vie nu e un no-op tăcut:
        ''' apelantul (butonul «Arată browser») trebuie să afle de ce nu s-a întâmplat nimic.
        ''' </summary>
        Public Async Function ShowBrowserAsync() As Task Implements IForexeRunner.ShowBrowserAsync
            If _executor Is Nothing OrElse Not _executor.IsBrowserOpen Then
                Throw New InvalidOperationException("Nicio sesiune activă — nu există browser de arătat.")
            End If
            Try
                Await _executor.ShowBrowserWindowAsync()
            Catch ex As Exception
                _logger?.LogException(ex, "Eroare la aducerea browserului în față")
                Throw
            End Try
        End Function

        ''' <summary>Ascunde la loc fereastra browserului (perechea lui ShowBrowserAsync).</summary>
        Public Async Function HideBrowserAsync() As Task Implements IForexeRunner.HideBrowserAsync
            If _executor Is Nothing OrElse Not _executor.IsBrowserOpen Then
                Throw New InvalidOperationException("Nicio sesiune activă — nu există browser de ascuns.")
            End If
            Try
                Await _executor.HideBrowserWindowAsync()
            Catch ex As Exception
                _logger?.LogException(ex, "Eroare la ascunderea browserului")
                Throw
            End Try
        End Function

        ''' <summary>Browserul e la vedere acum? False și când nu există sesiune.</summary>
        Public ReadOnly Property IsBrowserVisible As Boolean Implements IForexeRunner.IsBrowserVisible
            Get
                If _executor Is Nothing OrElse Not _executor.IsBrowserOpen Then Return False
                Return _executor.IsBrowserVisible
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

            ' Istoricul se DESCHIDE înainte de orice altceva: de aici încolo fiecare linie scrisă
            ' în jurnal intră și în intrarea acestei lucrări (RichTextBoxLogger → AppendLog), deci
            ' și un eșec de la prima verificare are unde să se vadă.
            JobHistoryManager.StartJob(job.WorkflowName, DescrieCererea(job))

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

                ' Stealth = INVERSUL lui job.ShowBrowser, exact ca în KBOT_IPC
                ' (`isStealth = Not jobToRun.ShowBrowser`). Implicit ascuns: fereastra pleacă
                ' off-screen și iese din Taskbar/Alt-Tab. Operatorul o poate aduce oricând la
                ' vedere din consolă — vezi ShowBrowserAsync / HideBrowserAsync.
                _executor = New WorkflowExecutor(
                    logger:=_logger,
                    certificate:=certificate,
                    stealthMode:=Not job.ShowBrowser,
                    stepByStep:=False,
                    confirmStep:=Nothing,
                    stepOnlyCheckpoints:=False,
                    progressCallback:=progressAction,
                    useSnapAssist:=False,
                    cancellationToken:=ct)

                ' PIN MANUAL (decizie A3): utilizatorul tastează PIN-ul în dialogul Windows.
                ' Niciun SendKeys de PIN.
                _executor.ManualPinMode = True

                ' Procesorul Excel pentru apelurile parseExcel din workflow. Tot HTTP-ul
                ' (adresă + token bearer + POST) stă în ApiClient; re-login-ul e transparent.
                _executor.SetExcelProcessor(_excelProcessor)

                AddHandler _executor.OnStatusUpdate, AddressOf OnExecutorStatus
                AddHandler _executor.OnLogMessage, AddressOf OnExecutorLogMessage
                AddHandler _executor.OnBrowserClosed, AddressOf OnExecutorBrowserClosed

                ' Gardianul ferestrei de PIN, pornit ÎNAINTE de lansarea browserului — ca în
                ' KBOT_IPC, unde StartUiGuardian merge imediat după inițializarea executorului.
                ' Cu browserul ascuns, dialogul de PIN e singurul lucru vizibil din tot fluxul:
                ' dacă rămâne acoperit, conectarea pare blocată.
                _guardian = New PinWindowGuardian(_logger)
                _guardian.Start()

                ' Frazele de fază — aceleași ca în KBOT_IPC, care le punea direct în lblStatus.
                Anunta("Lansare browser...")
                Await Task.Run(Function() _executor.LaunchAndPositionBrowserAsync())

                Anunta("Autentificare...")
                Dim xml As String = File.ReadAllText(job.WflPath)
                WorkflowParser.Logger = _logger
                Dim workflow As Workflow = WorkflowParser.Parse(xml, job.WflPath)
                _executor.SetWorkflowPath(job.WflPath)

                Anunta($"Execut: {Path.GetFileName(job.WflPath)}...")
                Await Task.Run(Function() _executor.ExecuteAsync(workflow))

                _logger.LogSuccess("Conectare reușită!")
                RidicaStare("Conectat. În așteptare...")
                JobHistoryManager.FinishJob("Succes")
                Return New JobResult With {.Success = True, .Message = "Conectare reușită."}

            Catch ex As OperationCanceledException
                _logger.LogWarning("Conectare anulată.")
                RidicaStare("Operație anulată.")
                ' Anulat NU e eroare: închidem intrarea cu statusul corect ÎNAINTE de Failed
                ' (care ar fi scris «Eroare»; pe o intrare deja închisă nu mai are efect).
                JobHistoryManager.FinishJob("Anulat")
                Return Failed("Conectare anulată.")

            Catch ex As Exception
                _logger.LogException(ex, "Eroare conectare")
                RidicaStare("Eroare!")
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

            ' O intrare de istoric per lucrare, deschisă înaintea primei verificări — vezi RunAsync.
            JobHistoryManager.StartJob(job.WorkflowName, DescrieCererea(job))

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
                ' Linia de stare arată FIȘIERUL, ca «Execut: ...» din KBOT_IPC; numele de workflow
                ' rămâne în jurnal, unde se poate citi pe îndelete.
                RidicaStare($"Execut: {Path.GetFileName(job.WflPath)}...")
                Await Task.Run(Function() _executor.ExecuteAsync(workflow))

                Dim result As New JobResult With {.Success = True, .Message = $"'{job.WorkflowName}' rulat."}
                ' Perechea lui «Salvare raport final...» din KBOT_IPC. Acolo se scria fișierul de
                ' output; aici se strâng variabilele executorului, iar salvarea locală o face
                ' coordonatorul mai încolo — deci fraza spune ce se întâmplă ACUM, nu ce se scria.
                RidicaStare("Colectare rezultate...")
                PopulateResult(result)
                InregistreazaRezultat(result)
                progress?.Report(100)
                RidicaStare("În așteptare...")
                JobHistoryManager.FinishJob("Succes")
                Return result

            Catch ex As OperationCanceledException
                _logger.LogWarning($"'{job.WorkflowName}' anulat.")
                RidicaStare("Operație anulată.")
                JobHistoryManager.FinishJob("Anulat")
                Return Failed($"'{job.WorkflowName}' anulat.")

            Catch ex As Exception
                _logger.LogException(ex, $"Eroare rulare '{job.WorkflowName}'")
                RidicaStare("Eroare!")
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
                Dim table As TabelRezultat = TryParseTable(kvp.Value)
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
        ''' <remarks>
        ''' STRUCTURA SE PĂSTREAZĂ (decizia D-N, 26.08.2026). Până atunci fiecare celulă
        ''' trecea prin <c>prop.Value.ToString()</c>. Pentru un scalar e fără pierdere;
        ''' pentru o celulă imbricată — <c>ListaReceptii.Detaliu</c>,
        ''' <c>TabelIndicatori.BugetIndicator</c> — dădea textul JSON, adică o listă
        ''' deghizată în text, pe care serverul trebuia apoi să o parseze a doua oară.
        ''' Executorul păstrează deja structura (<c>BuildCollectedRow</c> face
        ''' <c>JToken.Parse</c>); aici se pierdea, și aici s-a oprit.
        ''' </remarks>
        Public Shared Function TryParseTable(value As String) As TabelRezultat
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

            Dim rows As New TabelRezultat()
            For Each item In arr
                Dim obj As JObject = TryCast(item, JObject)
                If obj Is Nothing Then Return Nothing   ' array de valori, nu de obiecte
                Dim row As New RandTabel()
                For Each prop In obj.Properties()
                    row.Pune(prop.Name, DinJToken(prop.Value))
                Next
                rows.Adauga(row)
            Next
            Return rows
        End Function

        ''' <summary>
        ''' Un <c>JToken</c> al lui Newtonsoft ▸ o <see cref="CelulaTabel"/>, recursiv.
        ''' Singurul loc din soluție în care cele două reprezentări se ating: executorul
        ''' lucrează cu Newtonsoft, restul lanțului cu <c>CelulaTabel</c> și
        ''' <c>System.Text.Json</c>, iar <c>KBot.Domain</c> nu referă Newtonsoft.
        ''' </summary>
        ''' <remarks>
        ''' Numerele și valorile logice devin TEXT, fiindcă exact asta produce scraperul
        ''' (celulele vin din HTML) și exact asta parsează fiecare consumator
        ''' (<c>parse_amount</c> și frații lui). O celulă numerică inventată aici ar da
        ''' aceleiași coloane două forme — chiar lucrul de care scăpăm.
        ''' </remarks>
        Friend Shared Function DinJToken(token As JToken) As CelulaTabel
            If token Is Nothing Then Return CelulaTabel.Gol

            Select Case token.Type
                Case JTokenType.Array
                    Dim elemente As New List(Of CelulaTabel)()
                    For Each element As JToken In CType(token, JArray)
                        elemente.Add(DinJToken(element))
                    Next
                    Return CelulaTabel.DinLista(elemente)

                Case JTokenType.Object
                    Dim membri As New List(Of KeyValuePair(Of String, CelulaTabel))()
                    For Each prop In CType(token, JObject).Properties()
                        membri.Add(New KeyValuePair(Of String, CelulaTabel)(
                            prop.Name, DinJToken(prop.Value)))
                    Next
                    Return CelulaTabel.DinObiect(membri)

                Case JTokenType.Null, JTokenType.Undefined
                    Return CelulaTabel.Gol

                Case Else
                    Return CelulaTabel.DinText(If(token.ToString(), String.Empty))
            End Select
        End Function

        ''' <summary>
        ''' Trasarea pas cu pas a motorului («[MAIN] Pasul 7/23: Click [#btn]») — DOAR în jurnal.
        ''' Până în felia 0040 urca și în linia de stare, care astfel clipea de zeci de ori pe
        ''' secundă cu selectoare; în KBOT_IPC <c>OnStatusUpdate</c> nu era legat de
        ''' <c>lblStatus</c> deloc. Vezi <see cref="StatusUpdated"/>.
        ''' </summary>
        Private Sub OnExecutorStatus(status As String)
            _logger.LogInfo(status)
        End Sub

        ''' <summary>
        ''' Mesajele <c>OnLogMessage</c> ale executorului — pașii <c>Log</c> din workflow și
        ''' rândurile de progres ale scrapingului. ASTA e sursa lui <c>lblStatus</c> din
        ''' <c>KBOT_IPC</c> (<c>WireLogMessage</c>). Nu le mai scriem în jurnal: acțiunea
        ''' <c>Log</c> a scris deja acolo, pe nivelul ei (vezi <c>ExecuteLog</c>), iar un al doilea
        ''' rând ar dubla fiecare mesaj al workflow-ului.
        ''' </summary>
        Private Sub OnExecutorLogMessage(message As String)
            RidicaStare(message)
        End Sub

        ''' <summary>Frază de fază: intră ȘI în jurnal, ȘI în linia de stare.</summary>
        Private Sub Anunta(mesaj As String)
            _logger.LogInfo(mesaj)
            RidicaStare(mesaj)
        End Sub

        ' Un abonat care aruncă nu are voie să oprească robotul.
        Private Sub RidicaStare(mesaj As String)
            Try
                RaiseEvent StatusUpdated(Me, mesaj)
            Catch ex As Exception
                _logger.LogWarning($"Un abonat la StatusUpdated a aruncat: {ex.Message}")
            End Try
        End Sub

        Private Sub OnExecutorBrowserClosed(message As String)
            _logger.LogWarning($"Browser închis: {message}")
        End Sub

        ''' <summary>
        ''' Eșecul, într-un singur loc — inclusiv închiderea intrării din istoric. Așa nicio cale
        ''' de ieșire nu poate lăsa în urmă o lucrare veșnic «În execuție...»: toate trec pe aici.
        ''' <c>FailJob</c> nu face nimic dacă nu există lucrare deschisă.
        ''' </summary>
        Private Shared Function Failed(message As String) As JobResult
            JobHistoryManager.FailJob(message)
            Return New JobResult With {.Success = False, .Message = message}
        End Function

        ''' <summary>
        ''' Ce s-a cerut robotului, pentru coloana «intrare» a istoricului: numele workflow-ului,
        ''' fișierul .wfl și parametrii injectați. Valorile lungi se scurtează — un JSON de câteva
        ''' sute de kilobiți nu e o descriere, e o arhivă (aceea stă în WorkflowResultStore).
        ''' </summary>
        Private Shared Function DescrieCererea(job As JobRequest) As String
            Dim sb As New Text.StringBuilder()
            sb.AppendLine($"Workflow: {job.WorkflowName}")
            sb.AppendLine($"Fișier:   {job.WflPath}")
            If job.Parameters IsNot Nothing AndAlso job.Parameters.Count > 0 Then
                sb.AppendLine("Parametri:")
                For Each kvp In job.Parameters
                    sb.AppendLine($"  {kvp.Key} = {Scurteaza(kvp.Value, 500)}")
                Next
            End If
            Return sb.ToString()
        End Function

        ''' <summary>Taie un text la o lungime maximă și SPUNE că l-a tăiat.</summary>
        Private Shared Function Scurteaza(text As String, maxim As Integer) As String
            If String.IsNullOrEmpty(text) OrElse text.Length <= maxim Then Return If(text, String.Empty)
            Return text.Substring(0, maxim) & $"… (+{text.Length - maxim} caractere)"
        End Function

        ''' <summary>
        ''' Rezultatele lucrării, în forma în care le arată istoricul: scalarii ca valoare
        ''' (scurtată), iar tabelele ca dimensiune + capul de tabel. Conținutul integral nu se
        ''' dublează aici — el e deja salvat de <c>WorkflowResultStore</c>, pe disc.
        ''' </summary>
        Private Shared Sub InregistreazaRezultat(result As JobResult)
            Dim rezumat As New Dictionary(Of String, String)
            For Each kvp In result.Data
                Dim tabel As TabelRezultat = Nothing
                If result.Tables.TryGetValue(kvp.Key, tabel) Then
                    Dim coloane As String = If(tabel.Count > 0, String.Join(", ", tabel(0).Keys), "—")
                    ' Coloanele IMBRICATE se numesc pe litere. Istoricul lucrărilor e primul
                    ' loc în care cineva se uită după o descărcare, iar «Detaliu e o listă»
                    ' e chiar lucrul care se pierdea tăcut înainte de decizia D-N.
                    Dim imbricate As IReadOnlyList(Of String) = tabel.ColoaneImbricate()
                    rezumat(kvp.Key) = $"{tabel.Count} rânduri × {If(tabel.Count > 0, tabel(0).Count, 0)} coloane" &
                                       Environment.NewLine & "Coloane: " & coloane &
                                       If(imbricate.Count = 0, String.Empty,
                                          Environment.NewLine & "Imbricate: " & String.Join(", ", imbricate))
                Else
                    rezumat(kvp.Key) = Scurteaza(kvp.Value, 2000)
                End If
            Next
            JobHistoryManager.SaveOutputVariables(rezumat)
        End Sub

        Private Async Function DisposeExecutorAsync() As Task
            ' Gardianul trăiește cât sesiunea: se oprește ODATĂ cu executorul, altfel ar
            ' rămâne să scaneze la nesfârșit, câte un fir în plus la fiecare reconectare.
            _guardian?.Stop()
            _guardian = Nothing

            If _executor Is Nothing Then Return
            Try
                RemoveHandler _executor.OnStatusUpdate, AddressOf OnExecutorStatus
                RemoveHandler _executor.OnLogMessage, AddressOf OnExecutorLogMessage
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
