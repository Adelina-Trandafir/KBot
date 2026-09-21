Imports KBot.Common
Imports Newtonsoft.Json.Linq

' =============================================================================
'  Watch - the JS to .NET pipe of the floating K-BOT menu (slice 0073).
'
'  JS: Services/JavaScripts/ForexeWatch.js (Build Action = Embedded Resource)
'
'  Same two facts as the recorder shape this file:
'
'  1. ExposeFunctionAsync cannot be undone and throws on a second registration
'     of the same name, so the callback is installed once per executor
'     (_watchInstalled) and "stop" only flips a flag on both sides.
'
'  2. AddInitScriptAsync reaches FUTURE navigations only; the page already on
'     screen is injected with EvaluateAsync. The guard in the script makes the
'     second install harmless.
'
'  While a robot job drives the page the watcher is SUSPENDED: the robot's own
'  clicks would arm operations («Angajament nou» is the first thing Creare
'  Angajament.wfl presses), and a menu sitting over a target would make a
'  Playwright click fail with «element intercepts pointer events».
' =============================================================================
Partial Public Class WorkflowExecutor

    Private _watchInstalled As Boolean = False
    Private _watchActive As Boolean = False
    Private _watchSuspended As Boolean = False

    ''' <summary>Raised on the Playwright callback thread - marshal before touching UI.</summary>
    Public Event OnWatchEvent(ev As ForexeWatchEvent)

    ''' <summary>True while the floating menu is installed and its events are forwarded.</summary>
    Public ReadOnly Property WatchingActive As Boolean
        Get
            Return _watchActive
        End Get
    End Property

    ' =========================================================================
    '  StartWatchingAsync - installs the menu in the page (once) and opens the pipe
    ' =========================================================================
    Public Async Function StartWatchingAsync() As Task
        If _page Is Nothing OrElse _page.IsClosed Then Throw New InvalidOperationException(
            "Browserul nu este pornit. Nu pot instala meniul K-BOT în pagină.")

        If Not _watchInstalled Then
            Dim initEx As Exception = Nothing
            Try
                Await _page.ExposeFunctionAsync(Of String)(
                    "_kbotWatchCallback", AddressOf HandleWatchPayload)
                Await _page.AddInitScriptAsync(GetEmbeddedJs("ForexeWatch.js"))
                ' The page already on screen needs the script now. Wrapped in an arrow
                ' function so Playwright treats it as a function to call.
                Await _page.EvaluateAsync(Of Object)(
                    "() => { " & GetEmbeddedJs("ForexeWatch.js") & " }")
            Catch ex As Exception
                initEx = ex
            End Try

            If initEx IsNot Nothing Then
                GlobalErrorLog.Write("WorkflowExecutor.StartWatchingAsync", initEx)
                _logger.LogError($"[Urmărire] Eroare la instalarea meniului K-BOT: {initEx.Message}")
                Throw New InvalidOperationException(
                    "Nu am putut instala meniul K-BOT în pagină: " & initEx.Message, initEx)
            End If

            _watchInstalled = True
        End If

        _watchActive = True
        ' A stale page-side suspension (a job that died mid-way) is lifted here - but NOT
        ' one a job holds right now (slice 0074: the shell's «Browser» view docks the browser
        ' while a job may be running, and waking the watcher under the robot would arm
        ' operations on its clicks). RunJobAsync lifts its own hold when it is done.
        If Not _watchSuspended Then Await SetWatchSuspendedAsync(False)
        _logger.LogInfo("[Urmărire] Meniul K-BOT e în pagină; urmăresc operațiunile FOREXE.")
    End Function

    ''' <summary>
    ''' The angajament code the page shows RIGHT NOW (its header), or an empty string when
    ''' there is none, the menu is not installed, or the page cannot answer (mid-navigation).
    ''' Slice 0074: the shell's view asks this once it has docked, before deciding whether the
    ''' selected node still has to be opened - the "page" event of the resume would arrive
    ''' too late for that decision.
    ''' </summary>
    Public Async Function ReadPageAngajamentAsync() As Task(Of String)
        If Not _watchInstalled Then Return String.Empty
        If _page Is Nothing OrElse _page.IsClosed Then Return String.Empty
        Try
            Dim cod As String = Await _page.EvaluateAsync(Of String)(
                "() => (window._kbotWatch && window._kbotWatch.getCod) ? (window._kbotWatch.getCod() || '') : ''")
            Return If(cod, String.Empty).Trim()
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.ReadPageAngajamentAsync", ex)
            _logger.LogDebug($"[Urmărire] Nu am putut citi codul din pagină: {ex.Message}")
            Return String.Empty
        End Try
    End Function

    ''' <summary>Stops forwarding events. The page side script stays installed.</summary>
    Public Sub StopWatching()
        _watchActive = False
        _logger.LogInfo("[Urmărire] Urmărirea operațiunilor FOREXE oprită.")
    End Sub

    ' =========================================================================
    '  SetWatchSuspendedAsync - hides the menu and mutes the watcher while a job runs
    ' =========================================================================
    ''' <summary>
    ''' Suspends (True) or resumes (False) the in-page watcher. Safe to call at any time:
    ''' when the script is not installed or the page is mid-navigation it only records the
    ''' flag, which the callback reads too, so nothing the robot does is ever forwarded.
    ''' </summary>
    Public Async Function SetWatchSuspendedAsync(suspended As Boolean) As Task
        _watchSuspended = suspended
        If Not _watchInstalled Then Return
        If _page Is Nothing OrElse _page.IsClosed Then Return
        Try
            Await _page.EvaluateAsync(Of Object)(
                "(s) => { if (window._kbotWatch) { window._kbotWatch.setSuspended(s); } }",
                suspended)
        Catch ex As Exception
            ' The page may be navigating right now; the flag above still guards the
            ' callback, and the script re-reads sessionStorage on the next page anyway.
            GlobalErrorLog.Write("WorkflowExecutor.SetWatchSuspendedAsync", ex)
            _logger.LogDebug($"[Urmărire] Nu am putut comuta suspendarea în pagină: {ex.Message}")
        End Try
    End Function

    ' =========================================================================
    '  HandleWatchPayload - callback boundary: log and swallow, never rethrow
    ' =========================================================================
    Private Sub HandleWatchPayload(jsonArgs As String)
        If Not _watchActive OrElse _watchSuspended Then Return

        Dim ev As ForexeWatchEvent = Nothing
        Dim parseEx As Exception = Nothing
        Try
            ev = BuildWatchEvent(JObject.Parse(jsonArgs))
        Catch ex As Exception
            parseEx = ex
        End Try

        If parseEx IsNot Nothing Then
            GlobalErrorLog.Write("WorkflowExecutor.HandleWatchPayload", parseEx)
            _logger.LogWarning($"[Urmărire] Mesaj ignorat (JSON invalid): {parseEx.Message}")
            Return
        End If
        If ev Is Nothing Then Return

        Select Case ev.Kind
            Case ForexeWatchEventKind.Started
                _logger.LogAction($"[Urmărire] A început «{ev.Label}»" &
                                  If(String.IsNullOrEmpty(ev.CodLaStart), "", $" pe «{ev.CodLaStart}»") & ".")
            Case ForexeWatchEventKind.Finished
                _logger.LogSuccess($"[Urmărire] «{ev.Label}» s-a salvat" &
                                   If(String.IsNullOrEmpty(ev.CodEfectiv), "", $" — angajament «{ev.CodEfectiv}»") &
                                   If(String.IsNullOrEmpty(ev.Message), "", $" ({ev.Message})") & ".")
            Case ForexeWatchEventKind.Cancelled
                _logger.LogWarning($"[Urmărire] «{ev.Label}» abandonată" &
                                   If(String.IsNullOrEmpty(ev.Message), "", $" ({ev.Message})") & ".")
            Case ForexeWatchEventKind.PageOpened
                ' Every navigation says this; Debug keeps the console readable.
                _logger.LogDebug("[Urmărire] Pagina arată " &
                                 If(String.IsNullOrEmpty(ev.CodAngajament), "niciun angajament.",
                                    $"angajamentul «{ev.CodAngajament}»."))
            Case Else
                If Not String.IsNullOrEmpty(ev.Message) Then _logger.LogDebug($"[Urmărire] {ev.Message}")
        End Select

        RaiseEvent OnWatchEvent(ev)
    End Sub

    Private Shared Function BuildWatchEvent(obj As JObject) As ForexeWatchEvent
        Dim opName As String = TextOf(obj, "op")
        Return New ForexeWatchEvent With {
            .Kind = ForexeWatchEvent.ParseKind(TextOf(obj, "event")),
            .Operation = ForexeWatchEvent.ParseOperation(opName),
            .OperationName = opName,
            .Label = TextOf(obj, "label"),
            .CodAngajament = TextOf(obj, "cod"),
            .CodLaStart = TextOf(obj, "codAtStart"),
            .StartedAt = DateOf(obj, "startedAt"),
            .FinishedAt = DateOf(obj, "finishedAt"),
            .Url = TextOf(obj, "url"),
            .Message = TextOf(obj, "message")
        }
    End Function

    ''' <summary>ISO-8601 from the page (UTC, «Z»), turned into LOCAL time; Nothing when absent.</summary>
    Private Shared Function DateOf(obj As JObject, name As String) As Date?
        Dim text As String = TextOf(obj, name)
        If String.IsNullOrWhiteSpace(text) Then Return Nothing
        Dim parsed As DateTimeOffset
        If DateTimeOffset.TryParse(text, Globalization.CultureInfo.InvariantCulture,
                                   Globalization.DateTimeStyles.AssumeUniversal, parsed) Then
            Return parsed.LocalDateTime
        End If
        Return Nothing
    End Function

End Class
