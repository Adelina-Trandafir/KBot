Imports Newtonsoft.Json.Linq

' =============================================================================
'  Recorder - the JS to .NET pipe for the K-BOT recorder.
'
'  JS: Services/JavaScripts/Recorder.js (Build Action = Embedded Resource)
'
'  Two facts drive the shape of this file:
'
'  1. ExposeFunctionAsync cannot be undone and throws when the same name is
'     registered twice on a context. The callback is therefore installed once per
'     executor, guarded by _recorderInstalled, and StopRecording only flips a
'     boolean - it never uninstalls anything.
'
'  2. AddInitScriptAsync only reaches FUTURE navigations. The page that is already
'     loaded when recording starts is injected separately with EvaluateAsync,
'     otherwise the very first click is never recorded.
'
'  AJAX correlation reuses the Wicket monitor: #animlogo leaving display:none right
'  after a step means that step triggered a round trip; the following IDLE entry
'  measures how long it took. #statlogo is unreliable and is ignored on purpose.
' =============================================================================
Partial Public Class WorkflowExecutor

    ''' <summary>An #animlogo change later than this is no longer credited to the last step.</summary>
    Private Const RecorderAjaxWindowMs As Integer = 1500

    Private _recorderInstalled As Boolean = False
    Private _recorderIdleHooked As Boolean = False
    Private _recordingActive As Boolean = False
    Private ReadOnly _recorderSteps As New List(Of RecordedStep)
    Private ReadOnly _recorderLock As New Object()
    Private _lastStep As RecordedStep = Nothing
    Private ReadOnly _lastStepWatch As New Stopwatch()

    ''' <summary>Raised on the Playwright callback thread - marshal before touching UI.</summary>
    Public Event OnRecordedStep(recordedStep As RecordedStep)

    Public ReadOnly Property RecordingActive As Boolean
        Get
            Return _recordingActive
        End Get
    End Property

    Public ReadOnly Property RecordedSteps As IReadOnlyList(Of RecordedStep)
        Get
            SyncLock _recorderLock
                Return _recorderSteps.ToList()
            End SyncLock
        End Get
    End Property

    ' =========================================================================
    '  StartRecordingAsync
    ' =========================================================================
    Public Async Function StartRecordingAsync() As Task
        If _page Is Nothing OrElse _page.IsClosed Then Throw New InvalidOperationException(
            "Browserul nu este pornit. Nu pot începe înregistrarea.")

        ' The AJAX correlation depends on the Wicket monitor being alive.
        If Not WicketMonitoringActive Then
            Await StartWicketMonitoringAsync()
        End If

        If Not _recorderIdleHooked Then
            AddHandler OnWicketStateChange, AddressOf HandleRecorderWicketEntry
            _recorderIdleHooked = True
        End If

        If Not _recorderInstalled Then
            Dim initEx As Exception = Nothing
            Try
                Await _page.ExposeFunctionAsync(Of String)(
                    "_kbotRecorderCallback", AddressOf HandleRecorderPayload)
                Await _page.AddInitScriptAsync(GetEmbeddedJs("Recorder.js"))
                ' The page already on screen needs the script now; the guard inside
                ' Recorder.js makes this second injection harmless. Wrapped in an arrow
                ' function so Playwright treats it as a function to call, not as an
                ' expression to evaluate.
                Await _page.EvaluateAsync(Of Object)(
                    "() => { " & GetEmbeddedJs("Recorder.js") & " }")
            Catch ex As Exception
                initEx = ex
            End Try

            If initEx IsNot Nothing Then
                _logger.LogError($"[Recorder] Eroare la instalare: {initEx.Message}")
                Throw New InvalidOperationException(
                    "Nu am putut instala recorderul în pagină: " & initEx.Message, initEx)
            End If

            _recorderInstalled = True
        End If

        _recordingActive = True
        _logger.LogInfo("[Recorder] Înregistrare pornită.")
    End Function

    ''' <summary>Stops feeding the trail. The page side script stays installed.</summary>
    Public Sub StopRecording()
        _recordingActive = False
        _logger.LogInfo("[Recorder] Înregistrare oprită.")
    End Sub

    Public Sub ClearRecordedSteps()
        SyncLock _recorderLock
            _recorderSteps.Clear()
            _lastStep = Nothing
            _lastStepWatch.Reset()
        End SyncLock
    End Sub

    ' =========================================================================
    '  HandleRecorderPayload - callback boundary: log and swallow, never rethrow
    ' =========================================================================
    Private Sub HandleRecorderPayload(jsonArgs As String)
        If Not _recordingActive Then Return

        Dim recordedStep As RecordedStep = Nothing
        Dim parseEx As Exception = Nothing
        Try
            recordedStep = BuildRecordedStep(JObject.Parse(jsonArgs))
        Catch ex As Exception
            parseEx = ex
        End Try

        If parseEx IsNot Nothing Then
            _logger.LogWarning($"[Recorder] Eveniment ignorat (JSON invalid): {parseEx.Message}")
            Return
        End If
        If recordedStep Is Nothing Then Return

        SyncLock _recorderLock
            recordedStep.Index = _recorderSteps.Count
            _recorderSteps.Add(recordedStep)
            _lastStep = recordedStep
            _lastStepWatch.Restart()
        End SyncLock

        RaiseEvent OnRecordedStep(recordedStep)
    End Sub

    Private Shared Function BuildRecordedStep(obj As JObject) As RecordedStep
        Dim result As New RecordedStep With {
            .Kind = TextOf(obj, "kind"),
            .KeyName = TextOf(obj, "keyName"),
            .Timestamp = DateTime.Now,
            .Url = TextOf(obj, "url"),
            .Tag = TextOf(obj, "tag"),
            .TypeAttr = TextOf(obj, "type"),
            .NameAttr = TextOf(obj, "nameAttr"),
            .Value = TextOf(obj, "value"),
            .Text = TextOf(obj, "text"),
            .Widget = If(String.IsNullOrEmpty(TextOf(obj, "widget")), "none", TextOf(obj, "widget"))
        }

        Dim candidates As JArray = TryCast(obj("candidates"), JArray)
        If candidates IsNot Nothing Then
            For Each token As JToken In candidates
                Dim c As JObject = TryCast(token, JObject)
                If c Is Nothing Then Continue For
                result.Candidates.Add(New SelectorCandidate With {
                    .Selector = TextOf(c, "selector"),
                    .Strategy = TextOf(c, "strategy"),
                    .Score = NumberOf(c, "score"),
                    .MatchCount = NumberOf(c, "matchCount"),
                    .Fragile = FlagOf(c, "fragile")
                })
            Next
        End If

        Dim path As JArray = TryCast(obj("path"), JArray)
        If path IsNot Nothing Then
            For Each token As JToken In path
                Dim p As JObject = TryCast(token, JObject)
                If p Is Nothing Then Continue For
                result.Path.Add(BuildRecordedAncestor(p))
            Next
        End If

        Return result
    End Function

    Private Shared Function BuildRecordedAncestor(p As JObject) As RecordedAncestor
        Dim ancestor As New RecordedAncestor With {
            .Tag = TextOf(p, "tag"),
            .NameAttr = TextOf(p, "name"),
            .Text = TextOf(p, "text"),
            .IndexInParent = NumberOf(p, "indexInParent")
        }

        Dim classes As JArray = TryCast(p("classes"), JArray)
        If classes IsNot Nothing Then
            For Each c As JToken In classes
                Dim name As String = c.ToString()
                If Not String.IsNullOrEmpty(name) Then ancestor.Classes.Add(name)
            Next
        End If

        Dim attrs As JObject = TryCast(p("dataAttrs"), JObject)
        If attrs IsNot Nothing Then
            For Each prop As JProperty In attrs.Properties()
                ancestor.DataAttrs(prop.Name) = prop.Value.ToString()
            Next
        End If

        Return ancestor
    End Function

    Private Shared Function TextOf(obj As JObject, name As String) As String
        Dim token As JToken = obj(name)
        If token Is Nothing OrElse token.Type = JTokenType.Null Then Return String.Empty
        Return token.ToString()
    End Function

    Private Shared Function NumberOf(obj As JObject, name As String) As Integer
        Dim token As JToken = obj(name)
        If token Is Nothing OrElse token.Type = JTokenType.Null Then Return 0
        Dim parsed As Integer
        If Integer.TryParse(token.ToString(), parsed) Then Return parsed
        Return 0
    End Function

    Private Shared Function FlagOf(obj As JObject, name As String) As Boolean
        Dim token As JToken = obj(name)
        If token Is Nothing OrElse token.Type = JTokenType.Null Then Return False
        Return String.Equals(token.ToString(), "True", StringComparison.OrdinalIgnoreCase)
    End Function

    ' =========================================================================
    '  HandleRecorderWicketEntry - marks the last step as AJAX and times the idle
    ' =========================================================================
    Private Sub HandleRecorderWicketEntry(entry As WicketMonitorEntry)
        If Not _recordingActive Then Return
        If entry Is Nothing Then Return

        Dim target As RecordedStep
        Dim elapsed As Long
        SyncLock _recorderLock
            target = _lastStep
            elapsed = _lastStepWatch.ElapsedMilliseconds
        End SyncLock
        If target Is Nothing Then Return

        If String.Equals(entry.Source, "WICKET", StringComparison.Ordinal) Then
            ' #statlogo is unreliable; only #animlogo leaving "none" means a round trip.
            If String.Equals(entry.Element, "#animlogo", StringComparison.Ordinal) AndAlso
               Not String.Equals(entry.State, "none", StringComparison.OrdinalIgnoreCase) AndAlso
               elapsed < RecorderAjaxWindowMs Then
                target.TriggeredAjax = True
            End If

        ElseIf String.Equals(entry.Source, "IDLE", StringComparison.Ordinal) Then
            If target.TriggeredAjax AndAlso target.IdleAfterMs = 0 Then
                target.IdleAfterMs = CInt(Math.Min(elapsed, CLng(Integer.MaxValue)))
            End If
        End If
    End Sub

End Class
