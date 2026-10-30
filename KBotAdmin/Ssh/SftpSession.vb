Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Renci.SshNet
Imports Renci.SshNet.Common

''' <summary>
''' One SftpClient to the server (slice ME01), for the file push. Same login and the same host-key
''' pinning as <see cref="SshSession"/>; a fingerprint that differs from the pinned one refuses the
''' connection.
''' </summary>
Public NotInheritable Class SftpSession
    Implements IDisposable

    Private ReadOnly _settings As AdminSettings
    Private _client As SftpClient
    Private _hostKeyError As String = String.Empty

    Public Sub New(k_settings As AdminSettings)
        _settings = k_settings
    End Sub

    ''' <summary>Connects; a server that does not answer in time is tried again (a lost first packet is common), then reported.</summary>
    Public Sub Connect()
        Try
            For k_attempt As Integer = 1 To SshAuth.ConnectAttempts
                Try
                    ConnectOnce()
                    Return
                Catch ex As Exception When k_attempt < SshAuth.ConnectAttempts AndAlso SshAuth.IsTransientConnectFailure(ex, _hostKeyError <> String.Empty)
                    GlobalErrorLog.Write("SftpSession.Connect (incercarea " & k_attempt.ToString(CultureInfo.InvariantCulture) & ")", ex)
                    DropClient()
                    Threading.Thread.Sleep(1000)
                End Try
            Next
        Catch ex As Exception When TypeOf ex Is Net.Sockets.SocketException OrElse TypeOf ex Is Renci.SshNet.Common.SshOperationTimeoutException
            GlobalErrorLog.Write("SftpSession.Connect", ex)
            Throw New ApplicationException("Serverul nu răspunde (SFTP), după " & SshAuth.ConnectAttempts.ToString(CultureInfo.InvariantCulture) &
                                           " încercări. Verificați rețeaua, VPN-ul și firewall-ul.", ex)
        Catch ex As Exception
            GlobalErrorLog.Write("SftpSession.Connect", ex)
            Throw
        End Try
    End Sub

    Private Sub ConnectOnce()
        _client = New SftpClient(SshAuth.Build(_settings))
        AddHandler _client.HostKeyReceived, AddressOf OnHostKeyReceived
        _hostKeyError = String.Empty
        Try
            _client.Connect()
        Catch ex As SshConnectionException
            If _hostKeyError <> String.Empty Then Throw New ApplicationException(_hostKeyError, ex)
            Throw New ApplicationException("Conectare eșuată la server (SFTP). Verificați rețeaua și portul.", ex)
        Catch ex As SshAuthenticationException
            Throw New ApplicationException("Autentificare eșuată (SFTP). Verificați utilizatorul, cheia SSH din ~/.ssh sau parola.", ex)
        End Try
    End Sub

    Private Sub DropClient()
        If _client Is Nothing Then Return
        _client.Dispose()
        _client = Nothing
    End Sub

    Private Sub OnHostKeyReceived(k_sender As Object, e As HostKeyEventArgs)
        Dim k_seen As String = FormatFingerprint(e.FingerPrint)
        Dim k_expected As String = If(_settings.HostKeyFingerprint, String.Empty).Trim()
        If k_expected = String.Empty OrElse String.Equals(k_expected, k_seen, StringComparison.OrdinalIgnoreCase) Then
            e.CanTrust = True
        Else
            e.CanTrust = False
            _hostKeyError = $"Amprenta cheii serverului s-a schimbat! Așteptat: {k_expected}  Primit: {k_seen}. Conexiune refuzată (posibil atac MITM)."
        End If
    End Sub

    ''' <summary>True and the remote UTC time and size when the file exists.</summary>
    Public Function TryGetRemote(k_remotePath As String, ByRef k_mtimeUtc As DateTime, ByRef k_size As Long) As Boolean
        If Not _client.Exists(k_remotePath) Then
            k_mtimeUtc = DateTime.MinValue
            k_size = 0
            Return False
        End If
        Dim k_attrs As Sftp.ISftpFile = _client.Get(k_remotePath)
        k_mtimeUtc = k_attrs.LastWriteTimeUtc
        k_size = k_attrs.Length
        Return True
    End Function

    ''' <summary>Uploads one file, creating the missing remote folders first.</summary>
    Public Sub UploadFile(k_localPath As String, k_remotePath As String, k_preserveMTime As Boolean, k_createdDirs As HashSet(Of String))
        EnsureRemoteDirectory(RemoteDirName(k_remotePath), k_createdDirs)
        Using k_stream As FileStream = File.OpenRead(k_localPath)
            _client.UploadFile(k_stream, k_remotePath, True)
        End Using
        If k_preserveMTime Then
            Dim k_attrs As Sftp.SftpFileAttributes = _client.GetAttributes(k_remotePath)
            k_attrs.LastWriteTime = File.GetLastWriteTime(k_localPath)
            _client.SetAttributes(k_remotePath, k_attrs)
        End If
    End Sub

    ''' <summary>Uploads bytes as a file readable by its owner only (the state file of the local backup, ME02).</summary>
    Public Sub UploadBytes(k_bytes As Byte(), k_remotePath As String)
        Try
            EnsureRemoteDirectory(RemoteDirName(k_remotePath), New HashSet(Of String)())
            Using k_stream As New MemoryStream(k_bytes)
                _client.UploadFile(k_stream, k_remotePath, True)
            End Using
            _client.ChangePermissions(k_remotePath, 600S)       ' SSH.NET reads the digits as octal: 600 = rw------- (384 is refused: «Parameter 'mode'»)
        Catch ex As Exception
            GlobalErrorLog.Write("SftpSession.UploadBytes", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Deletes one remote file when it exists.</summary>
    Public Sub DeleteRemote(k_remotePath As String)
        Try
            If _client.Exists(k_remotePath) Then _client.DeleteFile(k_remotePath)
        Catch ex As Exception
            GlobalErrorLog.Write("SftpSession.DeleteRemote", ex)
            Throw
        End Try
    End Sub

    ' SFTP has no "mkdir -p": walk from the root down.
    Private Sub EnsureRemoteDirectory(k_remoteDir As String, k_createdDirs As HashSet(Of String))
        Dim k_normalized As String = k_remoteDir.Replace("\"c, "/"c)
        If k_normalized = String.Empty OrElse k_normalized = "/" Then Return
        If k_createdDirs.Contains(k_normalized) Then Return
        Dim k_current As String = String.Empty
        For Each k_segment As String In k_normalized.TrimStart("/"c).Split("/"c)
            If k_segment = String.Empty Then Continue For
            k_current &= "/" & k_segment
            If k_createdDirs.Contains(k_current) Then Continue For
            If Not _client.Exists(k_current) Then _client.CreateDirectory(k_current)
            k_createdDirs.Add(k_current)
        Next
    End Sub

    Private Shared Function RemoteDirName(k_remotePath As String) As String
        Dim k_path As String = k_remotePath.Replace("\"c, "/"c)
        Dim k_index As Integer = k_path.LastIndexOf("/"c)
        If k_index <= 0 Then Return "/"
        Return k_path.Substring(0, k_index)
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
