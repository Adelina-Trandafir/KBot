Imports System.Globalization
Imports System.Text
Imports Renci.SshNet
Imports Renci.SshNet.Common

' What one remote command produced. ExitStatus is -1 when the server sent none.
Public NotInheritable Class SshResult
    Public Property ExitStatus As Integer = -1
    Public Property StdOut As String = ""
    Public Property StdErr As String = ""
End Class

' Wraps a single SshClient to run commands on the server: the service restart
' sequence, and the schema_sync runs. Same host-key pinning as the SFTP service
' (the server is already trusted from the scan, but verified again).
'
' Commands run WITHOUT a terminal and with no stdin: anything that would prompt
' reads end-of-file instead. Whatever needs answering is answered on the command
' line, or piped in (see SchemaSyncService).
Public NotInheritable Class SshCommandService
    Implements IDisposable

    Private ReadOnly _settings As PushSettings
    Private _client As SshClient
    Private _observedFingerprint As String = ""
    Private _hostKeyError As String = ""

    Public Sub New(settings As PushSettings)
        _settings = settings
    End Sub

    Public Sub Connect()
        _client = New SshClient(SshAuth.Build(_settings))
        AddHandler _client.HostKeyReceived, AddressOf OnHostKeyReceived
        _hostKeyError = ""
        Try
            _client.Connect()
        Catch ex As SshConnectionException
            If _hostKeyError <> "" Then
                Throw New ApplicationException(_hostKeyError, ex)
            End If
            Throw New ApplicationException("Conectare eșuată la server (SSH). Verificați rețeaua și portul.", ex)
        Catch ex As SshAuthenticationException
            Throw New ApplicationException("Autentificare eșuată (SSH). Verificați utilizatorul, cheia SSH din ~/.ssh sau parola.", ex)
        End Try
    End Sub

    Private Sub OnHostKeyReceived(sender As Object, e As HostKeyEventArgs)
        _observedFingerprint = FormatFingerprint(e.FingerPrint)
        Dim expected = If(_settings.HostKeyFingerprint, "").Trim()

        If expected = "" Then
            e.CanTrust = True
        ElseIf String.Equals(expected, _observedFingerprint, StringComparison.OrdinalIgnoreCase) Then
            e.CanTrust = True
        Else
            e.CanTrust = False
            _hostKeyError = $"Amprenta cheii serverului s-a schimbat! Așteptat: {expected}  Primit: {_observedFingerprint}. Conexiune refuzată (posibil atac MITM)."
        End If
    End Sub

    ' Runs one command; returns True when the exit status is 0.
    Public Function RunCommand(commandText As String, ByRef exitStatus As Integer, ByRef stdOut As String, ByRef stdErr As String) As Boolean
        Using cmd = _client.CreateCommand(commandText)
            stdOut = cmd.Execute()
            stdErr = cmd.Error
            ' ExitStatus is Integer? in current SSH.NET; treat a missing status as -1.
            exitStatus = If(cmd.ExitStatus, -1)
        End Using
        Return exitStatus = 0
    End Function

    ' Same as RunCommand, packed into one object. Used where the caller wants to
    ' keep the whole exchange (a schema sync run) rather than three variables.
    Public Function Run(commandText As String) As SshResult
        Dim exitStatus As Integer = -1
        Dim stdOut As String = ""
        Dim stdErr As String = ""
        RunCommand(commandText, exitStatus, stdOut, stdErr)
        Return New SshResult With {
            .ExitStatus = exitStatus,
            .StdOut = If(stdOut, ""),
            .StdErr = If(stdErr, "")
        }
    End Function

    ' Runs one command and hands every complete output line to onLine WHILE it runs (stdout as is, stderr
    ' prefixed "[err] "). The whole stdout/stderr is still returned, so callers that parse it work unchanged.
    ' onLine runs on the calling (worker) thread; the caller marshals to the UI.
    Public Function RunStreaming(commandText As String, onLine As Action(Of String)) As SshResult
        Using cmd = _client.CreateCommand(commandText)
            Dim pending = cmd.BeginExecute()
            Dim utf8 = New UTF8Encoding(False)
            Dim outDecoder = utf8.GetDecoder()
            Dim errDecoder = utf8.GetDecoder()
            Dim outAll As New StringBuilder()
            Dim errAll As New StringBuilder()
            Dim outLine As New StringBuilder()
            Dim errLine As New StringBuilder()
            Do
                Dim moved = Pump(cmd.OutputStream, outDecoder, outAll, outLine, "", onLine)
                moved = Pump(cmd.ExtendedOutputStream, errDecoder, errAll, errLine, "[err] ", onLine) OrElse moved
                If pending.IsCompleted AndAlso Not moved Then Exit Do
                If Not moved Then Threading.Thread.Sleep(80)
            Loop
            cmd.EndExecute(pending)
            ' Whatever the last line was, even without a trailing newline.
            If outLine.Length > 0 Then onLine?.Invoke(outLine.ToString())
            If errLine.Length > 0 Then onLine?.Invoke("[err] " & errLine.ToString())
            Return New SshResult With {
                .ExitStatus = If(cmd.ExitStatus, -1),
                .StdOut = outAll.ToString(),
                .StdErr = errAll.ToString()
            }
        End Using
    End Function

    ' Moves what is available in the channel stream into the totals and emits finished lines. True if bytes moved.
    Private Shared Function Pump(stream As IO.Stream, decoder As Decoder, all As StringBuilder, line As StringBuilder,
                                 prefix As String, onLine As Action(Of String)) As Boolean
        Dim moved = False
        Dim buffer(8191) As Byte
        Do While stream.Length > 0
            Dim count = stream.Read(buffer, 0, buffer.Length)
            If count <= 0 Then Exit Do
            moved = True
            Dim chars(decoder.GetCharCount(buffer, 0, count) - 1) As Char
            Dim written = decoder.GetChars(buffer, 0, count, chars, 0)
            For i = 0 To written - 1
                all.Append(chars(i))
                If chars(i) = ControlChars.Lf Then
                    onLine?.Invoke(prefix & line.ToString().TrimEnd(ControlChars.Cr))
                    line.Clear()
                Else
                    line.Append(chars(i))
                End If
            Next
        Loop
        Return moved
    End Function

    Private Shared Function FormatFingerprint(bytes As Byte()) As String
        If bytes Is Nothing OrElse bytes.Length = 0 Then Return ""
        Dim sb As New StringBuilder(bytes.Length * 3)
        For i As Integer = 0 To bytes.Length - 1
            If i > 0 Then sb.Append(":"c)
            sb.Append(bytes(i).ToString("x2", CultureInfo.InvariantCulture))
        Next
        Return sb.ToString()
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _client IsNot Nothing Then
            Try
                If _client.IsConnected Then _client.Disconnect()
            Finally
                _client.Dispose()
                _client = Nothing
            End Try
        End If
    End Sub

End Class
