Option Strict On
Imports System.Globalization
Imports System.Text
Imports Renci.SshNet
Imports Renci.SshNet.Common

''' <summary>What one remote command produced. ExitStatus is -1 when the server sent none.</summary>
Public NotInheritable Class SshResult
    Public Property ExitStatus As Integer = -1
    Public Property StdOut As String = String.Empty
    Public Property StdErr As String = String.Empty
End Class

''' <summary>
''' One SshClient to the server (slice ME01). Commands run WITHOUT a terminal and with no stdin, so
''' anything that would prompt reads end-of-file instead. The server's host key is pinned by
''' fingerprint when admin_settings.json holds one; an empty fingerprint trusts the first key seen
''' and <see cref="ObservedFingerprint"/> tells the caller what to pin.
''' </summary>
Public NotInheritable Class SshSession
    Implements IDisposable

    Private ReadOnly _settings As AdminSettings
    Private _client As SshClient
    Private _hostKeyError As String = String.Empty

    Public Sub New(k_settings As AdminSettings)
        _settings = k_settings
    End Sub

    ''' <summary>The fingerprint the server presented on the last connect.</summary>
    Public ReadOnly Property ObservedFingerprint As String = String.Empty

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _client IsNot Nothing AndAlso _client.IsConnected
        End Get
    End Property

    Public Sub Connect()
        Try
            _client = New SshClient(SshAuth.Build(_settings))
            _client.KeepAliveInterval = TimeSpan.FromSeconds(30)   ' an idle session is not dropped by the server or a firewall
            AddHandler _client.HostKeyReceived, AddressOf OnHostKeyReceived
            _hostKeyError = String.Empty
            Try
                _client.Connect()
            Catch ex As SshConnectionException
                If _hostKeyError <> String.Empty Then Throw New ApplicationException(_hostKeyError, ex)
                Throw New ApplicationException("Conectare eșuată la server (SSH). Verificați rețeaua și portul.", ex)
            Catch ex As SshAuthenticationException
                Throw New ApplicationException("Autentificare eșuată (SSH). Verificați utilizatorul, cheia SSH din ~/.ssh sau parola.", ex)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("SshSession.Connect", ex)
            Throw
        End Try
    End Sub

    Private Sub OnHostKeyReceived(k_sender As Object, e As HostKeyEventArgs)
        _ObservedFingerprint = FormatFingerprint(e.FingerPrint)
        Dim k_expected As String = If(_settings.HostKeyFingerprint, String.Empty).Trim()
        If k_expected = String.Empty OrElse String.Equals(k_expected, ObservedFingerprint, StringComparison.OrdinalIgnoreCase) Then
            e.CanTrust = True
        Else
            e.CanTrust = False
            _hostKeyError = $"Amprenta cheii serverului s-a schimbat! Așteptat: {k_expected}  Primit: {ObservedFingerprint}. Conexiune refuzată (posibil atac MITM)."
        End If
    End Sub

    ''' <summary>Runs one command and returns everything it printed.</summary>
    Public Function Run(k_commandText As String) As SshResult
        Try
            EnsureConnected()
            Using k_cmd As SshCommand = _client.CreateCommand(k_commandText)
                Dim k_out As String = k_cmd.Execute()
                Return New SshResult With {
                    .ExitStatus = If(k_cmd.ExitStatus, -1),
                    .StdOut = If(k_out, String.Empty),
                    .StdErr = If(k_cmd.Error, String.Empty)
                }
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("SshSession.Run", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Runs one command and hands every complete output line to <paramref name="k_onLine"/> WHILE it runs
    ''' (stdout as is, stderr prefixed "[err] "). The whole output is still returned. k_onLine runs on the
    ''' calling (worker) thread; the caller marshals to the UI.
    ''' </summary>
    Public Function RunStreaming(k_commandText As String, k_onLine As Action(Of String)) As SshResult
        Try
            EnsureConnected()
            Using k_cmd As SshCommand = _client.CreateCommand(k_commandText)
                Dim k_pending As IAsyncResult = k_cmd.BeginExecute()
                Dim k_utf8 As New UTF8Encoding(False)
                Dim k_outDecoder As Decoder = k_utf8.GetDecoder()
                Dim k_errDecoder As Decoder = k_utf8.GetDecoder()
                Dim k_outAll As New StringBuilder()
                Dim k_errAll As New StringBuilder()
                Dim k_outLine As New StringBuilder()
                Dim k_errLine As New StringBuilder()
                Do
                    Dim k_moved As Boolean = Pump(k_cmd.OutputStream, k_outDecoder, k_outAll, k_outLine, String.Empty, k_onLine)
                    k_moved = Pump(k_cmd.ExtendedOutputStream, k_errDecoder, k_errAll, k_errLine, "[err] ", k_onLine) OrElse k_moved
                    If k_pending.IsCompleted AndAlso Not k_moved Then Exit Do
                    If Not k_moved Then Threading.Thread.Sleep(80)
                Loop
                k_cmd.EndExecute(k_pending)
                If k_outLine.Length > 0 Then k_onLine?.Invoke(k_outLine.ToString())
                If k_errLine.Length > 0 Then k_onLine?.Invoke("[err] " & k_errLine.ToString())
                Return New SshResult With {
                    .ExitStatus = If(k_cmd.ExitStatus, -1),
                    .StdOut = k_outAll.ToString(),
                    .StdErr = k_errAll.ToString()
                }
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("SshSession.RunStreaming", ex)
            Throw
        End Try
    End Function

    Private Sub EnsureConnected()
        If _client Is Nothing OrElse Not _client.IsConnected Then
            Throw New ApplicationException("Nu există conexiune la server. Conectați-vă mai întâi.")
        End If
    End Sub

    ' Moves what is available in the channel stream into the totals and emits finished lines. True if bytes moved.
    Private Shared Function Pump(k_stream As IO.Stream, k_decoder As Decoder, k_all As StringBuilder, k_line As StringBuilder,
                                 k_prefix As String, k_onLine As Action(Of String)) As Boolean
        Dim k_moved As Boolean = False
        Dim k_buffer(8191) As Byte
        Do While k_stream.Length > 0
            Dim k_count As Integer = k_stream.Read(k_buffer, 0, k_buffer.Length)
            If k_count <= 0 Then Exit Do
            k_moved = True
            Dim k_chars(k_decoder.GetCharCount(k_buffer, 0, k_count) - 1) As Char
            Dim k_written As Integer = k_decoder.GetChars(k_buffer, 0, k_count, k_chars, 0)
            For k_i As Integer = 0 To k_written - 1
                k_all.Append(k_chars(k_i))
                If k_chars(k_i) = ControlChars.Lf Then
                    k_onLine?.Invoke(k_prefix & k_line.ToString().TrimEnd(ControlChars.Cr))
                    k_line.Clear()
                Else
                    k_line.Append(k_chars(k_i))
                End If
            Next
        Loop
        Return k_moved
    End Function

    Private Shared Function FormatFingerprint(k_bytes As Byte()) As String
        If k_bytes Is Nothing OrElse k_bytes.Length = 0 Then Return String.Empty
        Dim k_sb As New StringBuilder(k_bytes.Length * 3)
        For k_i As Integer = 0 To k_bytes.Length - 1
            If k_i > 0 Then k_sb.Append(":"c)
            k_sb.Append(k_bytes(k_i).ToString("x2", CultureInfo.InvariantCulture))
        Next
        Return k_sb.ToString()
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
