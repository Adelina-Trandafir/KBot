Option Strict On
Imports System.Text
Imports System.Text.Json
Imports System.Threading

''' <summary>A refusal or a failure reported by the server half (admin_cli.py), already readable.</summary>
Public Class AdminException
    Inherits Exception

    Public Sub New(k_message As String)
        MyBase.New(k_message)
    End Sub

    Public Sub New(k_message As String, k_inner As Exception)
        MyBase.New(k_message, k_inner)
    End Sub
End Class

''' <summary>
''' The one door to the server (slice ME01): every call runs <c>routes.admin_cli.admin_cli</c> over SSH
''' and reads back the single JSON object it prints. Calls are serialized (one command at a time on
''' the one session), which also keeps the schema sync from overlapping with a refresh.
''' </summary>
Public NotInheritable Class AdminClient
    Implements IDisposable

    Private Shared ReadOnly JsonOptions As New JsonSerializerOptions With {
        .PropertyNameCaseInsensitive = True,
        .PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    }

    Private ReadOnly _settings As AdminSettings
    Private ReadOnly _gate As New SemaphoreSlim(1, 1)
    Private _ssh As SshSession

    Public Sub New(k_settings As AdminSettings)
        _settings = k_settings
    End Sub

    Public ReadOnly Property Settings As AdminSettings
        Get
            Return _settings
        End Get
    End Property

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _ssh IsNot Nothing AndAlso _ssh.IsConnected
        End Get
    End Property

    ''' <summary>The fingerprint the server showed on the last connect (for the operator to pin).</summary>
    Public ReadOnly Property ObservedFingerprint As String
        Get
            Return If(_ssh Is Nothing, String.Empty, _ssh.ObservedFingerprint)
        End Get
    End Property

    Public Async Function ConnectAsync() As Task
        Try
            Await _gate.WaitAsync().ConfigureAwait(True)
            Try
                If _ssh IsNot Nothing Then _ssh.Dispose()
                _ssh = New SshSession(_settings)
                Dim k_session As SshSession = _ssh
                Await Task.Run(Sub() k_session.Connect()).ConfigureAwait(True)
            Finally
                _gate.Release()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("AdminClient.ConnectAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The session, connected again when the server (or a firewall) dropped it while the program sat idle. Runs on a
    ''' worker thread inside the gate. Before the first connect there is nothing to repeat, so that is an error.
    ''' </summary>
    Private Function LiveSession() As SshSession
        If _ssh Is Nothing Then Throw New AdminException("Nu există conexiune la server. Conectați-vă mai întâi.")
        If _ssh.IsConnected Then Return _ssh
        Dim k_fresh As New SshSession(_settings)
        Try
            k_fresh.Connect()
        Catch ex As Exception
            k_fresh.Dispose()
            Throw
        End Try
        _ssh.Dispose()
        _ssh = k_fresh
        Return _ssh
    End Function

    ' ------------------------------------------------------------------
    ' Commands
    ' ------------------------------------------------------------------

    ''' <summary>Single-quotes a value for the remote shell.</summary>
    Public Shared Function Quote(k_value As String) As String
        Return "'" & If(k_value, String.Empty).Replace("'", "'\''") & "'"
    End Function

    Private Function CliCommand(k_verb As String, k_args As IEnumerable(Of String)) As String
        Dim k_sb As New StringBuilder()
        k_sb.Append("cd ").Append(Quote(_settings.RemoteRoot))
        k_sb.Append(" && PYTHONIOENCODING=utf-8 PYTHONUNBUFFERED=1 ").Append(Quote(_settings.RemotePython))
        k_sb.Append(" -m routes.admin_cli.admin_cli ").Append(k_verb)
        For Each k_arg As String In k_args
            k_sb.Append(" ").Append(Quote(k_arg))
        Next
        Return k_sb.ToString()
    End Function

    ''' <summary>Runs one admin_cli verb and returns its <c>data</c> element, or throws AdminException.</summary>
    Public Async Function CallRawAsync(k_verb As String, ParamArray k_args As String()) As Task(Of JsonElement)
        Try
            Dim k_command As String = CliCommand(k_verb, k_args)
            Await _gate.WaitAsync().ConfigureAwait(True)
            Dim k_result As SshResult
            Try
                k_result = Await Task.Run(Function() LiveSession().Run(k_command)).ConfigureAwait(True)
            Finally
                _gate.Release()
            End Try
            Return ParseEnvelope(k_result)
        Catch ex As AdminException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AdminClient.CallRawAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>Runs one admin_cli verb and reads its data as <typeparamref name="T"/>.</summary>
    Public Async Function CallAsync(Of T)(k_verb As String, ParamArray k_args As String()) As Task(Of T)
        Dim k_data As JsonElement = Await CallRawAsync(k_verb, k_args).ConfigureAwait(True)
        Try
            Return JsonSerializer.Deserialize(Of T)(k_data.GetRawText(), JsonOptions)
        Catch ex As JsonException
            GlobalErrorLog.Write("AdminClient.CallAsync", ex)
            Throw New AdminException("Răspunsul serverului nu a putut fi citit (" & k_verb & ").", ex)
        End Try
    End Function

    ''' <summary>
    ''' Runs an admin_cli verb that prints progress lines (<c>LINE&lt;TAB&gt;text</c>) before its JSON answer and
    ''' hands each progress text to <paramref name="k_onLine"/> as it arrives (on a worker thread).
    ''' </summary>
    Public Async Function CallStreamAsync(Of T)(k_verb As String, k_onLine As Action(Of String), ParamArray k_args As String()) As Task(Of T)
        Try
            Dim k_command As String = CliCommand(k_verb, k_args)
            Dim k_result As SshResult = Await StreamAsync(k_command,
                Sub(k_line)
                    If k_line.StartsWith("LINE" & vbTab, StringComparison.Ordinal) Then k_onLine?.Invoke(k_line.Substring(5))
                End Sub).ConfigureAwait(True)
            Dim k_data As JsonElement = ParseEnvelope(k_result)
            Return JsonSerializer.Deserialize(Of T)(k_data.GetRawText(), JsonOptions)
        Catch ex As AdminException
            Throw
        Catch ex As JsonException
            GlobalErrorLog.Write("AdminClient.CallStreamAsync", ex)
            Throw New AdminException("Răspunsul serverului nu a putut fi citit (" & k_verb & ").", ex)
        Catch ex As Exception
            GlobalErrorLog.Write("AdminClient.CallStreamAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Runs an arbitrary command line on the server and streams its lines (the schema sync). The line
    ''' handler runs on a worker thread.
    ''' </summary>
    Public Async Function StreamAsync(k_commandLine As String, k_onLine As Action(Of String)) As Task(Of SshResult)
        Try
            Await _gate.WaitAsync().ConfigureAwait(True)
            Try
                Return Await Task.Run(Function() LiveSession().RunStreaming(k_commandLine, k_onLine)).ConfigureAwait(True)
            Finally
                _gate.Release()
            End Try
        Catch ex As AdminException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AdminClient.StreamAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The schema_sync command line, as AvacontPush builds it. Targets Nothing/empty = every unit
    ''' database (the server discovers them). <paramref name="k_allowDestructive"/> pipes the typed «DA»
    ''' the command line asks for; the caller has already asked the operator.
    ''' </summary>
    Public Function SyncCommand(k_mode As String, k_targets As String, k_viewOnly As Boolean, k_allowDestructive As Boolean) As String
        If k_mode <> "SAFE" AndAlso k_mode <> "FORCE" Then Throw New ArgumentException("Mod necunoscut: " & k_mode, NameOf(k_mode))
        Dim k_sb As New StringBuilder()
        k_sb.Append("cd ").Append(Quote(_settings.RemoteRoot)).Append(" && ")
        If k_allowDestructive AndAlso Not k_viewOnly Then k_sb.Append("printf 'DA\n' | ")
        k_sb.Append("PYTHONIOENCODING=utf-8 PYTHONUNBUFFERED=1 ").Append(Quote(_settings.RemotePython))
        k_sb.Append(" -m routes.schema_sync.schema_sync --mode ").Append(k_mode)
        If Not String.IsNullOrWhiteSpace(k_targets) Then k_sb.Append(" --targets ").Append(Quote(k_targets))
        k_sb.Append(If(k_viewOnly, " --view", " --run"))
        If k_allowDestructive AndAlso Not k_viewOnly Then k_sb.Append(" --allow-destructive")
        Return k_sb.ToString()
    End Function

    ' ------------------------------------------------------------------

    Private Shared Function ParseEnvelope(k_result As SshResult) As JsonElement
        ' The JSON is the last line that starts with "{": anything the interpreter printed before it
        ' (a warning) must not break the read.
        Dim k_json As String = String.Empty
        For Each k_line As String In k_result.StdOut.Split({ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
            Dim k_trim As String = k_line.Trim()
            If k_trim.StartsWith("{", StringComparison.Ordinal) Then k_json = k_trim
        Next
        If k_json = String.Empty Then
            Dim k_err As String = k_result.StdErr.Trim()
            If k_err.Length > 600 Then k_err = k_err.Substring(k_err.Length - 600)
            Throw New AdminException("Serverul nu a răspuns cu date (cod " & k_result.ExitStatus & "). " & k_err)
        End If
        Try
            Using k_doc As JsonDocument = JsonDocument.Parse(k_json)
                Dim k_root As JsonElement = k_doc.RootElement
                Dim k_ok As JsonElement
                If Not k_root.TryGetProperty("ok", k_ok) Then Throw New AdminException("Răspuns necunoscut de la server.")
                If k_ok.ValueKind <> JsonValueKind.True Then
                    Dim k_message As JsonElement
                    Dim k_text As String = If(k_root.TryGetProperty("error", k_message), k_message.GetString(), "Eroare necunoscută.")
                    Throw New AdminException(k_text)
                End If
                Dim k_data As JsonElement
                If Not k_root.TryGetProperty("data", k_data) Then Throw New AdminException("Răspunsul serverului nu are date.")
                Return k_data.Clone()
            End Using
        Catch ex As JsonException
            Throw New AdminException("Răspunsul serverului nu este JSON valid.", ex)
        End Try
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _ssh IsNot Nothing Then _ssh.Dispose()
        _gate.Dispose()
    End Sub

End Class
