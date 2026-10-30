Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>How a check ended.</summary>
Friend Enum TrayStatus
    ''' <summary>No pair in the «Ține minte parola» key: K-BOT was not signed in on this Windows session (yet).</summary>
    NoCredentials
    ''' <summary>The server refused the pair (wrong password, account closed): it is not tried again until another pair is given.</summary>
    Rejected
    ''' <summary>The account must change its password first: only K-BOT can do that.</summary>
    PasswordChangeRequired
    ''' <summary>The user has no unit at all, so there is no list to keep.</summary>
    NoList
    ''' <summary>The list was read (it may be empty).</summary>
    Listed
    ''' <summary>The server could not be reached or refused; the next check tries again.</summary>
    Failed
End Enum

Friend NotInheritable Class TrayCheckResult
    Friend ReadOnly Property Status As TrayStatus
    Friend ReadOnly Property Documents As IReadOnlyList(Of DdfDeSemnat)
    ''' <summary>Units whose list could not be read on the server (the list is then incomplete).</summary>
    Friend ReadOnly Property UnreadUnits As IReadOnlyList(Of String)
    ''' <summary>For <see cref="TrayStatus.Rejected"/>: what the server said, in the words the user reads.</summary>
    Friend Property Message As String = String.Empty

    Friend Shared Function RejectedWith(k_message As String) As TrayCheckResult
        Return New TrayCheckResult(TrayStatus.Rejected) With {.Message = If(k_message, String.Empty)}
    End Function

    Friend Sub New(k_status As TrayStatus)
        Me.New(k_status, Nothing, Nothing)
    End Sub

    Friend Sub New(k_status As TrayStatus, k_documents As IEnumerable(Of DdfDeSemnat), k_unread As IEnumerable(Of String))
        Me.Status = k_status
        Me.Documents = If(k_documents IsNot Nothing, k_documents.ToList(), New List(Of DdfDeSemnat)())
        Me.UnreadUnits = If(k_unread IsNot Nothing, k_unread.ToList(), New List(Of String)())
    End Sub
End Class

''' <summary>
''' One check: signs the user in with the pair K-BOT keeps for this Windows session (<see cref="SessionCredentials"/>), finds
''' the role, and for a director reads the documents waiting for the signature (the same route as the director's window of
''' K-BOT). Keeps its own session between checks and signs in again, once, when the server says it expired.
'''
''' <para>The role is the one of the user's units (<c>/api/auth/my-units</c>), not of the unit signed in on: a director
''' whose last unit is one where they are an accountant is a director for the tray, so the session moves to the unit where the
''' role is «Director» (the list route needs it and then spans all the director's units).</para>
''' </summary>
Friend NotInheritable Class TrayChecker
    Implements IDisposable

    Private Const RoleDirector As String = "Director"

    Private ReadOnly _http As HttpClient
    Private ReadOnly _auth As AuthApi
    Private ReadOnly _session As New SessionContext()
    Private ReadOnly _api As ApiClient
    Private _sessionUser As String = String.Empty
    ' Slice 0124-06: the session is on a unit where the user is the director (the older director list is read too).
    Private _isDirector As Boolean
    Private ReadOnly _rejected As New HashSet(Of String)()

    ' A user with no list is not asked again on every check: the units of an account change rarely.
    Private _noListUser As String = String.Empty
    Private _noListUntilUtc As DateTime = DateTime.MinValue
    Private Shared ReadOnly NoListFor As TimeSpan = TimeSpan.FromHours(6)

    Friend Sub New()
        Dim k_options As New ApiOptions()
        k_options.EnsureHttpsBaseUrl()
        _http = New HttpClient() With {.BaseAddress = New Uri(k_options.BaseUrl), .Timeout = TimeSpan.FromSeconds(30)}
        _auth = New AuthApi(_http)
        _api = New ApiClient(_http, k_options, _session)
    End Sub

    ''' <summary>Runs one check. Never throws: a failure is logged and comes back as <see cref="TrayStatus.Failed"/>.</summary>
    Friend Async Function CheckAsync(k_ct As CancellationToken) As Task(Of TrayCheckResult)
        Try
            Dim k_user As String = Nothing, k_pass As String = Nothing
            If Not LoadCredentials(k_user, k_pass) Then
                Await LogoutAsync().ConfigureAwait(True)
                Return New TrayCheckResult(TrayStatus.NoCredentials)
            End If
            If Not String.Equals(k_user, _sessionUser, StringComparison.OrdinalIgnoreCase) Then
                Await LogoutAsync().ConfigureAwait(True)
            End If
            If String.Equals(k_user, _noListUser, StringComparison.OrdinalIgnoreCase) AndAlso DateTime.UtcNow < _noListUntilUtc Then
                Return New TrayCheckResult(TrayStatus.NoList)
            End If

            Try
                Return Await ReadAsync(k_user, k_pass, k_ct).ConfigureAwait(True)
            Catch ex As ApiException When ex.StatusCode.HasValue AndAlso ex.StatusCode.Value = 401 AndAlso _session.IsAuthenticated
                ' The server dropped the session (idle too long, restart): sign in again, once (below, outside the Catch).
                _session.Clear()
            End Try
            Return Await ReadAsync(k_user, k_pass, k_ct).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            Return New TrayCheckResult(TrayStatus.Failed)
        Catch ex As Exception
            GlobalErrorLog.Write("TrayChecker.CheckAsync", ex)
            Return New TrayCheckResult(TrayStatus.Failed)
        End Try
    End Function

    Private Async Function ReadAsync(k_user As String, k_pass As String, k_ct As CancellationToken) As Task(Of TrayCheckResult)
        If Not _session.IsAuthenticated Then
            Dim k_status As TrayStatus
            Try
                k_status = Await SignInAsync(k_user, k_pass, k_ct).ConfigureAwait(True)
            Catch ex As ApiException When IsRefusal(ex)
                _rejected.Add(Fingerprint(k_user, k_pass))
                SessionCredentials.ForgetInProcess()
                Return TrayCheckResult.RejectedWith(ex.Message)
            End Try
            If k_status <> TrayStatus.Listed Then Return New TrayCheckResult(k_status)
        End If
        ' Slice 0124-06: every role reads the flow's list; a director also reads the older one (A and B signed), the two joined.
        Dim k_list As DdfDeSemnatLista = Await _api.GetDdfDeSemnatAsync(k_ct).ConfigureAwait(True)
        If _isDirector Then
            Dim k_old As DdfDeSemnatLista = Await _api.GetDdfDeSemnatDirectorAsync(k_ct).ConfigureAwait(True)
            k_list = k_old.Uneste(k_list)
        End If
        Return New TrayCheckResult(TrayStatus.Listed, k_list.Revizii, k_list.UnitatiNecitite)
    End Function

    ''' <summary>
    ''' Signs in on the last unit used, learns the role from the user's units and, for a director, moves to the unit where the
    ''' role is «Director». <see cref="TrayStatus.Listed"/> = signed in as a director, ready to read.
    ''' </summary>
    Private Async Function SignInAsync(k_user As String, k_pass As String, k_ct As CancellationToken) As Task(Of TrayStatus)
        Dim k_access As UnitsOutcome = Await _auth.CheckAccessAsync(k_user, k_pass, k_ct).ConfigureAwait(True)
        If k_access.PasswordChangeRequired Then Return TrayStatus.PasswordChangeRequired
        If k_access.Units.Count = 0 Then Return MarkNoList(k_user)

        Dim k_last As String = LastLoginStore.Load().UnitDc
        Dim k_unit As UnitInfo = k_access.Units.FirstOrDefault(Function(k_u) String.Equals(k_u.DC, k_last, StringComparison.Ordinal))
        If k_unit Is Nothing Then k_unit = k_access.Units(0)

        Dim k_login As LoginResult = Await _auth.LoginAsync(k_user, k_pass, k_unit.DC, Environment.MachineName, k_ct).ConfigureAwait(True)
        _session.Populate(k_user, k_login.Token, k_login.SessionContext)
        _sessionUser = k_user

        If Not String.Equals(_session.Role, RoleDirector, StringComparison.OrdinalIgnoreCase) Then
            Dim k_mine As IReadOnlyList(Of UnitInfo) = Await _auth.GetMyUnitsAsync(_session.Token, k_ct).ConfigureAwait(True)
            Dim k_director As UnitInfo = k_mine.FirstOrDefault(
                Function(k_u) String.Equals(k_u.Rol, RoleDirector, StringComparison.OrdinalIgnoreCase))
            If k_director IsNot Nothing Then
                Dim k_switched As LoginResult = Await _auth.SwitchUnitAsync(_session.Token, k_director.DC, Environment.MachineName, k_ct).ConfigureAwait(True)
                _session.Populate(k_user, k_switched.Token, k_switched.SessionContext)
            End If
        End If
        _isDirector = String.Equals(_session.Role, RoleDirector, StringComparison.OrdinalIgnoreCase)
        Return TrayStatus.Listed
    End Function

    ' The pair K-BOT keeps for this Windows session ("Tine minte parola") first, then the one given to the tray itself (kept in
    ' this process only, encrypted with DPAPI). A pair the server refused is skipped until a different one appears.
    Private Function LoadCredentials(ByRef k_user As String, ByRef k_pass As String) As Boolean
        If SessionCredentials.TryLoadForWindowsSession(k_user, k_pass) AndAlso Not _rejected.Contains(Fingerprint(k_user, k_pass)) Then Return True
        If SessionCredentials.TryRecall(k_user, k_pass) AndAlso Not _rejected.Contains(Fingerprint(k_user, k_pass)) Then Return True
        k_user = Nothing
        k_pass = Nothing
        Return False
    End Function

    Private Shared Function IsRefusal(k_ex As ApiException) As Boolean
        Return k_ex.StatusCode.HasValue AndAlso (k_ex.StatusCode.Value = 400 OrElse k_ex.StatusCode.Value = 401 OrElse k_ex.StatusCode.Value = 403)
    End Function

    ' Only a hash of the refused pair is kept, never the password.
    Private Shared Function Fingerprint(k_user As String, k_pass As String) As String
        Return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(k_user & vbLf & k_pass)))
    End Function

    Private Function MarkNoList(k_user As String) As TrayStatus
        _noListUser = k_user
        _noListUntilUtc = DateTime.UtcNow + NoListFor
        Return TrayStatus.NoList
    End Function

    ''' <summary>Ends the server session, if there is one. Best effort: a failure is logged only.</summary>
    Friend Async Function LogoutAsync() As Task
        Dim k_token As String = _session.Token
        _session.Clear()
        _sessionUser = String.Empty
        _isDirector = False
        If String.IsNullOrEmpty(k_token) Then Return
        Try
            Using k_cts As New CancellationTokenSource(TimeSpan.FromSeconds(5))
                Await _auth.LogoutAsync(k_token, k_cts.Token).ConfigureAwait(True)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("TrayChecker.LogoutAsync", ex)
        End Try
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        _http.Dispose()
    End Sub
End Class
