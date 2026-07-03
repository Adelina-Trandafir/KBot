Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Renci.SshNet
Imports Renci.SshNet.Common

' Wraps a single SftpClient for one operation. Verifies the server host key
' against the pinned fingerprint; refuses connection on mismatch (MITM guard).
Public NotInheritable Class SftpService
    Implements IDisposable

    Private ReadOnly _settings As PushSettings
    Private ReadOnly _log As RunLogger
    Private _client As SftpClient
    Private _observedFingerprint As String = ""
    Private _hostKeyError As String = ""

    ' Fingerprint seen during the handshake (used to pin on first run).
    Public ReadOnly Property ObservedFingerprint As String
        Get
            Return _observedFingerprint
        End Get
    End Property

    Public Sub New(settings As PushSettings, log As RunLogger)
        _settings = settings
        _log = log
    End Sub

    Public Sub Connect()
        _client = New SftpClient(_settings.Host, _settings.Port, _settings.User, _settings.Password)
        AddHandler _client.HostKeyReceived, AddressOf OnHostKeyReceived
        _hostKeyError = ""
        Try
            _client.Connect()
        Catch ex As SshConnectionException
            ' A host-key mismatch fails the connect; surface the specific reason.
            If _hostKeyError <> "" Then
                Throw New ApplicationException(_hostKeyError, ex)
            End If
            Throw New ApplicationException("Conectare eșuată la server (SFTP). Verificați rețeaua și portul.", ex)
        Catch ex As SshAuthenticationException
            Throw New ApplicationException("Autentificare eșuată (SFTP). Verificați utilizatorul și parola.", ex)
        End Try
    End Sub

    Private Sub OnHostKeyReceived(sender As Object, e As HostKeyEventArgs)
        _observedFingerprint = FormatFingerprint(e.FingerPrint)
        Dim expected = If(_settings.HostKeyFingerprint, "").Trim()

        If expected = "" Then
            ' First run: accept now; the caller pins it after a successful connect.
            e.CanTrust = True
        ElseIf String.Equals(expected, _observedFingerprint, StringComparison.OrdinalIgnoreCase) Then
            e.CanTrust = True
        Else
            e.CanTrust = False
            _hostKeyError = $"Amprenta cheii serverului s-a schimbat! Așteptat: {expected}  Primit: {_observedFingerprint}. Conexiune refuzată (posibil atac MITM)."
        End If
    End Sub

    ' Returns True and the remote UTC mtime if the file exists; False otherwise.
    Public Function TryGetRemoteMtimeUtc(remotePath As String, ByRef mtimeUtc As DateTime) As Boolean
        If Not _client.Exists(remotePath) Then
            mtimeUtc = DateTime.MinValue
            Return False
        End If
        Dim attrs = _client.GetAttributes(remotePath)
        mtimeUtc = attrs.LastWriteTimeUtc
        Return True
    End Function

    ' Uploads one file, creating any missing remote directories first.
    ' When preserveMTime is True, the remote mtime is set to match the local file
    ' so a later scan reports the file as IDENTIC (avoids phantom re-pushes).
    Public Sub UploadFile(localPath As String, remotePath As String, preserveMTime As Boolean, createdDirs As HashSet(Of String))
        EnsureRemoteDirectory(RemoteDirName(remotePath), createdDirs)

        Using fs = File.OpenRead(localPath)
            _client.UploadFile(fs, remotePath, canOverride:=True)
        End Using

        If preserveMTime Then
            Dim attrs = _client.GetAttributes(remotePath)
            attrs.LastWriteTime = File.GetLastWriteTime(localPath)
            _client.SetAttributes(remotePath, attrs)
        End If
    End Sub

    ' Creates each missing segment of an absolute remote directory path.
    ' SFTP has no "mkdir -p", so we walk from root down.
    Private Sub EnsureRemoteDirectory(remoteDir As String, createdDirs As HashSet(Of String))
        Dim normalized = NormalizeRemote(remoteDir)
        If normalized = "" OrElse normalized = "/" Then Return
        If createdDirs.Contains(normalized) Then Return

        Dim segments = normalized.TrimStart("/"c).Split("/"c)
        Dim current = ""
        For Each seg In segments
            If seg = "" Then Continue For
            current &= "/" & seg
            If createdDirs.Contains(current) Then Continue For
            If Not _client.Exists(current) Then
                _client.CreateDirectory(current)
            End If
            createdDirs.Add(current)
        Next
    End Sub

    Private Shared Function NormalizeRemote(p As String) As String
        Return p.Replace("\"c, "/"c)
    End Function

    Private Shared Function RemoteDirName(remotePath As String) As String
        Dim p = NormalizeRemote(remotePath)
        Dim idx = p.LastIndexOf("/"c)
        If idx <= 0 Then Return "/"
        Return p.Substring(0, idx)
    End Function

    ' Hex, colon-separated fingerprint of the host key (MD5 bytes from SSH.NET).
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
