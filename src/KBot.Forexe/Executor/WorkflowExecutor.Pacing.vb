Imports Newtonsoft.Json.Linq
Imports WorkflowModels

''' <summary>
''' Slice 0091 (operator, 29.09.2026): two knobs for slow connections, both set by
''' <c>ForexeRunner</c> from «Setări» → FOREXE before every job.
''' <list type="bullet">
''' <item><see cref="TimeoutMultiplier"/> stretches every wait written in the WFL files (the
''' actions' <c>timeout</c>, the <c>Wait</c> pauses) and the robot's own Ajax wait, without
''' touching the files.</item>
''' <item><see cref="ValidateReadsTwice"/> makes <c>ScrapeTable</c> read every page twice and
''' keep reading until two readings agree. Born from a reception detail read while FOREXE was
''' still sending the page (017_SCNB, AAB2DH3X6SK): the table stopped mid-row, even mid-cell.</item>
''' </list>
''' </summary>
Partial Public Class WorkflowExecutor

    ''' <summary>How many readings of one page before giving up on agreement.</summary>
    Private Const MaxReadsPerPage As Integer = 4

    ''' <summary>The pause between two readings of the same page, before the multiplier.</summary>
    Private Const ReReadPauseMs As Integer = 750

    Private _timeoutMultiplier As Double = 1.0

    ' The actions already stretched: the same object runs many times (loops, ForEachVar), and
    ' it must be multiplied once. By reference -- two identical actions are two actions.
    Private ReadOnly _stretched As New HashSet(Of IWorkflowAction)(ReferenceEqualityComparer.Instance)

    ''' <summary>
    ''' Multiplier for every wait of the flow; 1 = the WFL files as written. Setting it starts
    ''' a fresh count of stretched actions (a new job parses new action objects anyway).
    ''' </summary>
    Public Property TimeoutMultiplier As Double
        Get
            Return _timeoutMultiplier
        End Get
        Set(value As Double)
            If value < 1.0 OrElse value > 5.0 Then
                Throw New ArgumentException($"Multiplicatorul timpilor trebuie să fie între 1 și 5 (primit {value}).", NameOf(value))
            End If
            _timeoutMultiplier = value
            _stretched.Clear()
        End Set
    End Property

    ''' <summary>Every <c>ScrapeTable</c> page is read until two readings agree.</summary>
    Public Property ValidateReadsTwice As Boolean

    ''' <summary>A millisecond wait of the robot itself, stretched by the multiplier.</summary>
    Private Function Stretched(ms As Integer) As Integer
        Return CInt(Math.Ceiling(ms * _timeoutMultiplier))
    End Function

    ''' <summary>
    ''' Stretches the waits of <paramref name="action"/> once, the first time it runs. Mutating
    ''' the parsed action (not the file) keeps every one of the ~60 places that read
    ''' <c>action.Timeout</c> unchanged.
    ''' </summary>
    Private Sub StretchWaitsOnce(action As IWorkflowAction)
        If action Is Nothing OrElse _timeoutMultiplier = 1.0 Then Return
        If Not _stretched.Add(action) Then Return
        action.Timeout = CInt(Math.Ceiling(action.Timeout * _timeoutMultiplier))
        Dim pauza As WaitAction = TryCast(action, WaitAction)
        If pauza IsNot Nothing Then pauza.Seconds *= _timeoutMultiplier
    End Sub

    ''' <summary>
    ''' One page of a <c>ScrapeTable</c>, read once -- or, with <see cref="ValidateReadsTwice"/>,
    ''' read again after the Ajax wait and a pause until two consecutive readings agree (at most
    ''' <see cref="MaxReadsPerPage"/>). The last reading is kept either way; a page that never
    ''' settles is said on the console, and the server's own check of the detail still stands.
    ''' </summary>
    Private Async Function ExtractRowsCheckedAsync(parsedSelector As String,
                                                   action As ScrapeTableAction) As Task(Of List(Of Object))
        Dim rows As List(Of Object) = Await ExtractRawRowsAsync(parsedSelector, action)
        If Not ValidateReadsTwice Then Return rows

        Dim before As String = Amprenta(rows)
        For citirea As Integer = 2 To MaxReadsPerPage
            Await WaitForWicketIdleAsync()
            Await Task.Delay(Stretched(ReReadPauseMs))
            Dim again As List(Of Object) = Await ExtractRawRowsAsync(parsedSelector, action)
            Dim now As String = Amprenta(again)
            If now = before Then
                If citirea > 2 Then
                    _logger.LogInfo($"[ScrapeTable] Tabelul s-a stabilizat la citirea {citirea}.")
                End If
                Return again
            End If
            _logger.LogWarning($"[ScrapeTable] Citirea {citirea} diferă de cea dinainte " &
                               $"({rows.Count} → {again.Count} rânduri): pagina încă se încărca. Mai citesc o dată.")
            rows = again
            before = now
        Next
        _logger.LogWarning($"[ScrapeTable] Tabelul nu s-a stabilizat după {MaxReadsPerPage} citiri; merg mai departe cu ultima.")
        Return rows
    End Function

    ' The same conversion SaveScrapeResults goes through, so two readings compare exactly on
    ' what would be saved.
    Private Shared Function Amprenta(rows As List(Of Object)) As String
        Dim arr As New JArray()
        For Each r As Object In rows
            arr.Add(If(r Is Nothing, JValue.CreateNull(), JToken.FromObject(r)))
        Next
        Return arr.ToString(Newtonsoft.Json.Formatting.None)
    End Function

End Class
