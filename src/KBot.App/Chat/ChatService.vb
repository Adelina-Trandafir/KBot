Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>How a batch of messages relates to what the bubble already shows.</summary>
Public Enum ChatLoadMode
    ''' <summary>The latest messages: the bubble forgets what it had and shows these.</summary>
    Latest = 0
    ''' <summary>Messages newer than the newest shown: they go at the end.</summary>
    Newer = 1
    ''' <summary>Messages older than the oldest shown: they go at the start.</summary>
    Older = 2
End Enum

Public NotInheritable Class ChatMessagesEventArgs
    Inherits EventArgs

    Public ReadOnly Property Messages As IReadOnlyList(Of ChatMessage)
    Public ReadOnly Property Mode As ChatLoadMode
    ''' <summary>For <see cref="ChatLoadMode.Latest"/> and <see cref="ChatLoadMode.Older"/>: older messages remain on the server.</summary>
    Public ReadOnly Property HasMore As Boolean

    Public Sub New(k_messages As IReadOnlyList(Of ChatMessage), k_mode As ChatLoadMode, k_hasMore As Boolean)
        Messages = k_messages
        Mode = k_mode
        HasMore = k_hasMore
    End Sub
End Class

''' <summary>
''' The shared chat as K-BOT.app lives it (slice CHAT-08): one object for the whole run that asks the server for news,
''' keeps what it needs to ask only for what is new, and tells the opening page by events (always on the UI thread).
''' The page never calls the server; it asks this service.
''' </summary>
''' <remarks>
''' <para><b>Polling.</b> One WinForms timer: every <see cref="ClosedSeconds"/> s while the bubble is closed (only the unread
''' count is asked), every <see cref="OpenSeconds"/> s while it is open (the messages after the newest one known). A
''' poll that finds the previous call still running is skipped. A poll that fails stays silent -- the next one
''' tries again; only what the operator did (open, send) shows an error. The server does not extend the K-BOT session
''' for these calls, so the chat never keeps an idle session alive.</para>
''' <para><b>Read marks.</b> A message counts as read when the bubble is open and shows it.</para>
''' </remarks>
Public NotInheritable Class ChatService
    Implements IDisposable

    Private Const ClosedSeconds As Integer = 60
    Private Const OpenSeconds As Integer = 5
    Private Const PageSize As Integer = 50
    Private Const CatchUpBatches As Integer = 5
    ''' <summary>The code this application sends its messages under (V = VERCON, A = ADECHIT, K = K-BOT).</summary>
    Public Const ApplicationCode As String = "K"

    Private ReadOnly _api As IChatApi
    Private ReadOnly _session As SessionContext
    Private ReadOnly _timer As New System.Windows.Forms.Timer()
    Private ReadOnly _gate As New SemaphoreSlim(1, 1)
    Private ReadOnly _cts As New CancellationTokenSource()
    Private ReadOnly _newestByApp As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Private _state As ChatState
    Private _newest As String
    Private _oldest As String
    Private _loaded As Boolean
    Private _filter As String = String.Empty
    Private _open As Boolean
    Private _started As Boolean
    Private _disposed As Boolean
    Private _retrySignature As String
    Private _retryKey As String
    Private _tokenSeen As String = String.Empty

    ''' <summary>The state changed: unread count, identity or preferences.</summary>
    Public Event StateChanged As EventHandler(Of ChatState)
    ''' <summary>Messages to show.</summary>
    Public Event MessagesReceived As EventHandler(Of ChatMessagesEventArgs)
    ''' <summary>Something the operator asked for did not work (the text is for them, in Romanian).</summary>
    Public Event Failed As EventHandler(Of String)
    ''' <summary>A call the operator asked for starts (True) or ends (False).</summary>
    Public Event BusyChanged As EventHandler(Of Boolean)

    Public Sub New(k_api As IChatApi, k_session As SessionContext)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_session)
        _api = k_api
        _session = k_session
        _timer.Interval = ClosedSeconds * 1000
        AddHandler _timer.Tick, AddressOf Timer_Tick
    End Sub

    ''' <summary>The last state the server gave, or Nothing before the first answer.</summary>
    Public ReadOnly Property State As ChatState
        Get
            Return _state
        End Get
    End Property

    ''' <summary>Starts asking the server for news (once; a second call does nothing). Needs a logged-in session.</summary>
    Public Sub Start()
        If _started OrElse _disposed Then Return
        _started = True
        _timer.Start()
        ' The first answer should not wait a minute.
        Dim k_ignored As Task = PollAsync()
    End Sub

    ''' <summary>The page tells the service whether the bubble is open: it decides how often to ask and what.</summary>
    Public Sub SetOpen(k_open As Boolean)
        _open = k_open
        _timer.Interval = If(k_open, OpenSeconds, ClosedSeconds) * 1000
    End Sub

    ' ---- What the operator asks ----------------------------------------------------------------

    ''' <summary>The bubble opened: show the history (first time or after the filter changed) or the news since it closed.</summary>
    Public Async Function OpenAsync() As Task
        Await RunAsync(Async Function(k_ct As CancellationToken)
                           If _loaded Then
                               Await PullNewerAsync(k_ct)
                           Else
                               Await LoadLatestAsync(k_ct)
                           End If
                           Await MarkReadAsync(k_ct)
                       End Function, True)
    End Function

    ''' <summary>The messages before the oldest one shown.</summary>
    Public Async Function OlderAsync() As Task
        Await RunAsync(Async Function(k_ct As CancellationToken)
                           If _oldest Is Nothing Then Return
                           Dim k_page As ChatMessagePage = Await _api.GetChatMessagesAsync(_oldest, Nothing, PageSize, k_ct)
                           Absorb(k_page.state)
                           Deliver(k_page.messages, ChatLoadMode.Older, k_page.has_more)
                       End Function, True)
    End Function

    ''' <summary>Sends a message. True = the server has it (the page then clears the draft).</summary>
    Public Async Function SendAsync(k_text As String, k_anonymous As Boolean, k_displayName As String,
                                    k_replyTo As String) As Task(Of Boolean)
        Dim k_ok As Boolean = False
        Await RunAsync(Async Function(k_ct As CancellationToken)
                           ' The same message sent again after a failure keeps its key, so the server cannot store it twice.
                           Dim k_sig As String = k_text & vbLf & k_anonymous.ToString() & vbLf & k_displayName & vbLf & k_replyTo
                           If _retrySignature <> k_sig Then
                               _retrySignature = k_sig
                               _retryKey = Guid.NewGuid().ToString()
                           End If
                           Dim k_req As New ChatSendRequest() With {
                               .text = k_text, .application = ApplicationCode, .anonymous = k_anonymous,
                               .display_name = k_displayName, .client_key = _retryKey, .reply_to = k_replyTo}
                           Dim k_result As ChatSendResult = Await _api.SendChatMessageAsync(k_req, k_ct)
                           _retrySignature = Nothing
                           _retryKey = Nothing
                           Absorb(k_result.state)
                           k_ok = True
                           If _loaded Then Await PullNewerAsync(k_ct) Else Await LoadLatestAsync(k_ct)
                           Await MarkReadAsync(k_ct)
                       End Function, True)
        Return k_ok
    End Function

    ''' <summary>Saves whether new messages go out under the pseudonym.</summary>
    Public Async Function SaveAnonymousAsync(k_anonymous As Boolean) As Task
        Await RunAsync(Async Function(k_ct As CancellationToken)
                           Absorb(Await _api.SaveChatPreferencesAsync(Nothing, k_anonymous, Nothing, k_ct))
                       End Function, False)
    End Function

    ''' <summary>Saves which applications messages are shown (V, A, K; at least one) and loads the list again for them.</summary>
    Public Async Function SaveApplicationsAsync(k_applications As IReadOnlyList(Of String)) As Task
        Await RunAsync(Async Function(k_ct As CancellationToken)
                           Absorb(Await _api.SaveChatPreferencesAsync(Nothing, Nothing, k_applications, k_ct))
                           Await LoadLatestAsync(k_ct)
                           Await MarkReadAsync(k_ct)
                       End Function, True)
    End Function

    ' ---- Polling -------------------------------------------------------------------------------

    Private Async Sub Timer_Tick(sender As Object, e As EventArgs)
        Try
            Await PollAsync()
        Catch ex As Exception
            ' UI boundary (timer, async void): log and swallow.
            GlobalErrorLog.Write("ChatService.Timer_Tick", ex)
        End Try
    End Sub

    Private Async Function PollAsync() As Task
        If _disposed OrElse Not _session.IsAuthenticated Then Return
        If Not Await _gate.WaitAsync(0) Then Return   ' a call is running: skip this poll
        Try
            ForgetIfNewSession()
            If _open Then
                If _loaded Then Await PullNewerAsync(_cts.Token) Else Await LoadLatestAsync(_cts.Token)
                Await MarkReadAsync(_cts.Token)
            Else
                Absorb(Await _api.GetChatStateAsync(_cts.Token))
            End If
        Catch ex As OperationCanceledException
            ' Closing down.
        Catch ex As ApiException
            ' Quiet: the next poll tries again (a dead session is found by the calls the operator makes).
        Catch ex As Exception
            GlobalErrorLog.Write("ChatService.PollAsync", ex)
        Finally
            If Not _disposed Then _gate.Release()
        End Try
    End Function

    ' ---- Calls ---------------------------------------------------------------------------------

    ' Runs one operator-asked call alone (the polls wait their turn by skipping), reports a failure to the page.
    Private Async Function RunAsync(k_work As Func(Of CancellationToken, Task), k_showBusy As Boolean) As Task
        If _disposed Then Return
        Await _gate.WaitAsync(_cts.Token)
        If k_showBusy Then RaiseEvent BusyChanged(Me, True)
        Try
            ForgetIfNewSession()
            Await k_work(_cts.Token)
        Catch ex As OperationCanceledException
            ' Closing down.
        Catch ex As ApiException
            RaiseEvent Failed(Me, ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ChatService.RunAsync", ex)
            RaiseEvent Failed(Me, "Chatul nu este momentan disponibil. Reîncercați.")
        Finally
            If Not _disposed Then _gate.Release()
            If k_showBusy Then RaiseEvent BusyChanged(Me, False)
        End Try
    End Function

    Private Async Function LoadLatestAsync(k_ct As CancellationToken) As Task
        Dim k_page As ChatMessagePage = Await _api.GetChatMessagesAsync(Nothing, Nothing, PageSize, k_ct)
        Absorb(k_page.state)
        _loaded = True
        _newest = Nothing
        _oldest = Nothing
        _newestByApp.Clear()
        Deliver(k_page.messages, ChatLoadMode.Latest, k_page.has_more)
    End Function

    Private Async Function PullNewerAsync(k_ct As CancellationToken) As Task
        For k_batch As Integer = 1 To CatchUpBatches
            Dim k_page As ChatMessagePage = Await _api.GetChatMessagesAsync(Nothing, If(_newest, "0"), PageSize, k_ct)
            ' The filter changed elsewhere (the web page): the list on screen is no longer the right one.
            If FilterOf(k_page.state) <> _filter Then
                Absorb(k_page.state)
                Await LoadLatestAsync(k_ct)
                Return
            End If
            Absorb(k_page.state)
            Deliver(k_page.messages, ChatLoadMode.Newer, k_page.has_more)
            If Not k_page.has_more Then Return
        Next
    End Function

    Private Async Function MarkReadAsync(k_ct As CancellationToken) As Task
        If _state Is Nothing Then Return
        For Each k_pair As KeyValuePair(Of String, String) In New List(Of KeyValuePair(Of String, String))(_newestByApp)
            Dim k_read As String = Nothing
            _state.read_by_application.TryGetValue(k_pair.Key, k_read)
            If k_read Is Nothing OrElse IsGreater(k_pair.Value, k_read) Then
                Absorb(Await _api.MarkChatReadAsync(k_pair.Key, k_pair.Value, k_ct))
            End If
        Next
    End Function

    Private Sub Deliver(k_messages As List(Of ChatMessage), k_mode As ChatLoadMode, k_hasMore As Boolean)
        For Each k_m As ChatMessage In k_messages
            If _newest Is Nothing OrElse IsGreater(k_m.id, _newest) Then _newest = k_m.id
            If _oldest Is Nothing OrElse IsGreater(_oldest, k_m.id) Then _oldest = k_m.id
            Dim k_prev As String = Nothing
            _newestByApp.TryGetValue(k_m.application, k_prev)
            If k_prev Is Nothing OrElse IsGreater(k_m.id, k_prev) Then _newestByApp(k_m.application) = k_m.id
        Next
        ' Nothing new on a "newer" pull is not news for the page.
        If k_mode = ChatLoadMode.Newer AndAlso k_messages.Count = 0 Then Return
        RaiseEvent MessagesReceived(Me, New ChatMessagesEventArgs(k_messages, k_mode, k_hasMore))
    End Sub

    ' The service lives as long as the program; the person behind the session may change (log out, log in as another).
    ' What it remembered about the first is of no use to the second.
    Private Sub ForgetIfNewSession()
        If String.Equals(_tokenSeen, _session.Token, StringComparison.Ordinal) Then Return
        _tokenSeen = _session.Token
        _state = Nothing
        _loaded = False
        _newest = Nothing
        _oldest = Nothing
        _filter = String.Empty
        _newestByApp.Clear()
        _retrySignature = Nothing
        _retryKey = Nothing
    End Sub

    Private Sub Absorb(k_state As ChatState)
        If k_state Is Nothing Then Return
        Dim k_filter As String = FilterOf(k_state)
        If _state IsNot Nothing AndAlso _filter <> k_filter Then _loaded = False   ' another set of applications: load again
        _filter = k_filter
        _state = k_state
        RaiseEvent StateChanged(Me, k_state)
    End Sub

    Private Shared Function FilterOf(k_state As ChatState) As String
        Return String.Concat(k_state.preferences.applications)
    End Function

    ' Message ids are decimal strings: compare by length, then by text.
    Private Shared Function IsGreater(k_a As String, k_b As String) As Boolean
        If k_a.Length <> k_b.Length Then Return k_a.Length > k_b.Length
        Return String.CompareOrdinal(k_a, k_b) > 0
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        _timer.Stop()
        _timer.Dispose()
        _cts.Cancel()
        _cts.Dispose()
        _gate.Dispose()
    End Sub

End Class
