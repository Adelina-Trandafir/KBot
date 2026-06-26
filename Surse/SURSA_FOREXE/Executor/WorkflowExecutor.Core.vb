Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports Microsoft.Playwright
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports WorkflowModels

Partial Public Class WorkflowExecutor
    Implements IAsyncDisposable

    ' --- EVENIMENTE ---
    Public Event OnStatusUpdate(status As String)
    Public Event OnActionStart(action As IWorkflowAction)
    Public Event OnLogMessage(message As String)
    Public Event OnBrowserClosed(message As String)
    ' -----------------------------------------------

    Private ReadOnly _logger As RichTextBoxLogger
    Private ReadOnly _certificate As X509Certificate2
    Private ReadOnly _headless As Boolean
    Private ReadOnly _stealthMode As Boolean

    Private _cancellationToken As CancellationToken
    'Private Property _pin As String

    Private _playwright As IPlaywright
    Private _browser As IBrowser
    Private _context As IBrowserContext
    Private _page As IPage
    Private _stepByStep As Boolean
    Private _stepOnlyCheckpoints As Boolean
    Private _useSnapAssist As Boolean

    Private _confirmStep As Func(Of String, StepResult)
    Private _workflowPath As String = Nothing


    Private ReadOnly _windowsSecurityAutomation As WindowsSecurityAutomation
    Private ReadOnly _variables As New Dictionary(Of String, List(Of String))
    Private ReadOnly _isDebug As Boolean
    Private ReadOnly _progressCallback As Action(Of Integer, Integer)
    Private ReadOnly _showTechnicalMessage As Boolean = False

    Private _throttleSettings As ThrottleSettings = ThrottleSettings.None

    ' Clasă internă pentru a ține minte starea fiecărei bucle active
    Public Class LoopContext
        Public Property ActionType As String     ' "While", "ForEach", "Repeat"
        Public Property RuntimeIndex As Integer  ' 1, 2, 3...
        Public Property IndexVariableName As String ' "angIdx", "pageIdx", etc.
    End Class

    ' Asta ține minte ierarhia curentă (Cine e părintele, cine e copilul)
    Private ReadOnly _executionStack As New Stack(Of LoopContext)

    ' Enum extins cu Previous
    Public Enum StepResult
        [Continue]
        Skip
        [Stop]
        Previous
    End Enum

    Private _lastCheckpointIndex As Integer = -1
    Private _currentWorkflow As Workflow
    Private _currentActionIndex As Integer = 0

    Public IsReloaded As Boolean = False

    Public ReadOnly Property CurrentPage As IPage
        Get
            Return _page
        End Get
    End Property

    Public ReadOnly Property CurrentWorkflow As Workflow
        Get
            Return _currentWorkflow
        End Get
    End Property

    Public ReadOnly Property CurrentURL As String
        Get
            If _page IsNot Nothing Then
                Return _page.Url
            Else
                Return String.Empty
            End If
        End Get
    End Property

    Public ReadOnly Property LastCheckpointIndex As Integer
        Get
            Return _lastCheckpointIndex
        End Get
    End Property

    Public ReadOnly Property CanResumeFromCheckpoint As Boolean
        Get
            Return _lastCheckpointIndex >= 0 AndAlso _page IsNot Nothing
        End Get
    End Property

    Public ReadOnly Property IsBrowserOpen As Boolean
        Get
            Return _page IsNot Nothing AndAlso Not _page.IsClosed
        End Get
    End Property

#Disable Warning CA1068 ' CancellationToken parameters must come last
    Public Sub New(logger As RichTextBoxLogger,
                   certificate As X509Certificate2,
                   stealthMode As Boolean,
                   Optional stepByStep As Boolean = False,
                   Optional confirmStep As Func(Of String, StepResult) = Nothing,
                   Optional stepOnlyCheckpoints As Boolean = False,
                   Optional progressCallback As Action(Of Integer, Integer) = Nothing,
                   Optional useSnapAssist As Boolean = False,
                   Optional cancellationToken As CancellationToken = Nothing,
                   Optional showTechnicalMessage As Boolean = False)
        Try
#Enable Warning CA1068 ' CancellationToken parameters must come last
            _logger = logger
            _certificate = certificate
            _headless = False
            _stealthMode = stealthMode
            _cancellationToken = cancellationToken
            _stepByStep = stepByStep
            _confirmStep = confirmStep
            _progressCallback = progressCallback
            _stepOnlyCheckpoints = stepOnlyCheckpoints
            _useSnapAssist = useSnapAssist
            _showTechnicalMessage = showTechnicalMessage

#If DEBUG Then
            _isDebug = True
#Else
        _isDebug = False
#End If
        Catch ex As Exception
            _logger.LogError($"[WorkflowExecutor] Eroare la inițializare: {ex.Message}")
        End Try
    End Sub

    Public Sub SetWorkflowPath(path As String)
        _workflowPath = path
    End Sub

    Public Sub SetVariable(name As String, value As String)
        If String.IsNullOrEmpty(name) Then Return

        Dim valueList As List(Of String) = Nothing
        If Not _variables.TryGetValue(name, valueList) Then
            valueList = New List(Of String)()
            _variables(name) = valueList
        End If

        valueList.Add(value)
    End Sub

    Public Function GetVariable(name As String) As String
        Dim list As List(Of String) = Nothing
        If _variables.TryGetValue(name, list) AndAlso list.Count > 0 Then
            Return list.Last()
        End If
        Return ""
    End Function

    Public Sub DumpVariablesToLog()
        Dim vars = GetAllVariables()
        If vars.Count = 0 Then
            _logger.LogInfo("[VARS] Nicio variabilă în memorie.")
            Return
        End If

        _logger.Separator()
        _logger.LogInfo($"[VARS] {vars.Count} variabile în memorie la momentul opririi:")

        For Each kvp In vars.OrderBy(Function(k) k.Key)
            Dim val = kvp.Value
            If String.IsNullOrEmpty(val) Then
                _logger.LogInfo($"  [[{kvp.Key}]] = (gol)")
            ElseIf val.Length > 300 Then
                _logger.LogInfo($"  [[{kvp.Key}]] = {val.Substring(0, 300)}...")
            Else
                _logger.LogInfo($"  [[{kvp.Key}]] = {val}")
            End If
        Next

        _logger.Separator()
    End Sub

    Public Function GetAllVariables() As Dictionary(Of String, String)
        Dim result As New Dictionary(Of String, String)

        For Each kvp In _variables
            Dim key = kvp.Key
            Dim values = kvp.Value

            If values.Count = 1 Then
                ' Cazul simplu: O singură valoare -> String simplu (păstrăm compatibilitatea)
                result(key) = values(0)
            Else
                ' --- MODIFICARE PENTRU VBA / JSON COMPLEX ---
                ' Dacă avem mai multe valori (ex: ScrapeTable rulat de 2 ori),
                ' le combinăm într-un JArray real, nu Array de String-uri.

                Dim mergedArray As New JArray()

                For Each val As String In values
                    Try
                        ' Încercăm să detectăm dacă valoarea este un JSON valid (Array sau Object)
                        If val.Trim().StartsWith("["c) OrElse val.Trim().StartsWith("{"c) Then
                            Dim token As JToken = JToken.Parse(val)
                            mergedArray.Add(token)
                        Else
                            ' Dacă e text simplu, îl adăugăm ca atare
                            mergedArray.Add(val)
                        End If
                    Catch ex As Exception
                        ' Fallback: Dacă parsing-ul eșuează, îl punem ca string
                        mergedArray.Add(val)
                    End Try
                Next

                ' Serializăm rezultatul ca un JSON Array curat
                ' Rezultat: [ [{col:val}, {col:val}], [{col:val}] ]
                result(key) = mergedArray.ToString(Formatting.None)
            End If
        Next

        Return result
    End Function

    Public Sub ResetVariables()
        _variables.Clear()
    End Sub

    Public Sub ClearVariable(name As String)
        _variables.Remove(name)
    End Sub

    Public Sub ClearAllVariables()
        _variables.Clear()
    End Sub

    Private Sub StopAuthMonitoring()
        _windowsSecurityAutomation?.StopMonitoring()
    End Sub

    Public Sub UpdateThrottleSettings(settings As ThrottleSettings)
        _throttleSettings = If(settings, ThrottleSettings.None)
        _logger.LogInfo($"[EXECUTOR] Throttle actualizat: {_throttleSettings.Label}")
    End Sub

    Public Async Function CloseAsync(Optional keepOpen As Boolean = False) As Task
        StopAuthMonitoring()
        If keepOpen AndAlso _isDebug Then Return
        Try
            If _page IsNot Nothing Then Await _page.CloseAsync()
            If _context IsNot Nothing Then Await _context.CloseAsync()
            If _browser IsNot Nothing Then Await _browser.CloseAsync()
            _playwright?.Dispose()
        Catch
        End Try
    End Function

    Public Function DisposeAsync() As ValueTask Implements IAsyncDisposable.DisposeAsync
        Return New ValueTask(CloseAsync(False))
    End Function

    ' Funcție ajutătoare pentru afișarea mesajelor prietenoase
    Private Sub LogStep(action As IWorkflowAction, defaultTechnicalMessage As String)
        If Not String.IsNullOrEmpty(action.LogValue) Then
            If action.LogValue.Contains("_"c) Then Return
            ' Afișăm textul frumos din XML (ex: "Salvez factura...")
            _logger.LogNormal(action.LogValue)
        Else
            'If Not _showTechnicalMessage Then Return
            ' Afișăm detaliul tehnic (ex: "Click pe #btnSave")
            _logger.LogAction(defaultTechnicalMessage)
        End If
    End Sub

End Class