Imports System.Globalization
Imports System.Text
Imports Renci.SshNet
Imports Renci.SshNet.Common

' Wraps a single SshClient to run the restart sequence. Same host-key pinning
' as the SFTP service (the server is already trusted from the scan, but verified again).
Public NotInheritable Class SshRestartService
    Implements IDisposable

    Private ReadOnly _settings As PushSettings
    Private _client As SshClient
    Private _observedFingerprint As String = ""
    Private _hostKeyError As String = ""

    Public Sub New(settings As PushSettings)
        _settings = settings
    End Sub

    Public Sub Connect()
        _client = New SshClient(_settings.Host, _settings.Port, _settings.User, _settings.Password)
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
            Throw New ApplicationException("Autentificare eșuată (SSH). Verificați utilizatorul și parola.", ex)
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
