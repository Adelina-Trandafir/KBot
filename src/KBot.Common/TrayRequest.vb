Option Strict On
Imports System.IO
Imports System.Text
Imports System.Text.Json

''' <summary>
''' What the tray application (KBot.Tray) asks K-BOT to open: the director clicked a document in the tray's list
''' (<see cref="ActionSign"/>, with the unit and the revision) or the tray's list itself (the same action with no
''' revision), or just K-BOT's main window (<see cref="ActionMain"/>).
'''
''' <para>The tray writes this file and then starts <c>KBot.App.exe</c>. A K-BOT that is not running reads it once its
''' window is up; one that is running is only brought forward by its single-instance guard and finds the file through
''' its watcher (<c>TrayRequestHub</c>). Either way the file is the one message, so nothing about the tray's command line
''' has to be parsed by the application.</para>
'''
''' <para>Where: <c>%APPDATA%\AVACONT\KBot\tray_request.json</c>, next to the other per-user files. Plain identifiers only,
''' no secret: the unit, the revision and the moment.</para>
''' </summary>
Public NotInheritable Class TrayRequest

    ''' <summary>File name inside <see cref="SetariFoldere.DirectorSetari"/>.</summary>
    Public Const FileName As String = "tray_request.json"

    ''' <summary>Open the list of documents waiting for the signature (and the revision, when <see cref="Idrev"/> is set).</summary>
    Public Const ActionSign As String = "sign"

    ''' <summary>Open the main window, nothing more.</summary>
    Public Const ActionMain As String = "main"

    ''' <summary>
    ''' Name of the event the tray waits on besides its own menu: K-BOT sets it before an update so the tray ends and the new
    ''' version of it can start afterwards (the update must not wait for a restart of Windows).
    ''' </summary>
    Public Const ExitEventName As String = "Local\KBOT_Tray_Exit_5C1E7A90"

    Public Property Action As String = ActionSign

    ''' <summary>The unit database of the revision (<c>DdfDeSemnat.DbName</c>); empty = no unit asked for.</summary>
    Public Property Dc As String = String.Empty

    ''' <summary>The revision to open; 0 = none, the list opens with no selection.</summary>
    Public Property Idrev As Integer

    Public Property CreatedUtc As DateTime

    Private Shared ReadOnly _json As New JsonSerializerOptions With {.WriteIndented = True}

    Public Shared Function FilePath(Optional k_dir As String = Nothing) As String
        Return Path.Combine(If(String.IsNullOrEmpty(k_dir), SetariFoldere.DirectorSetari(), k_dir), FileName)
    End Function

    ''' <summary>
    ''' Writes the request, whole or not at all (a temporary file moved over the target), so a watcher never reads half of it.
    ''' I/O boundary: logs and rethrows.
    ''' </summary>
    Public Shared Sub Write(k_request As TrayRequest, Optional k_dir As String = Nothing)
        Try
            ArgumentNullException.ThrowIfNull(k_request)
            k_request.CreatedUtc = DateTime.UtcNow
            Dim k_target As String = FilePath(k_dir)
            Directory.CreateDirectory(Path.GetDirectoryName(k_target))
            Dim k_temp As String = k_target & ".tmp"
            File.WriteAllText(k_temp, JsonSerializer.Serialize(k_request, _json), New UTF8Encoding(False))
            File.Move(k_temp, k_target, overwrite:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("TrayRequest.Write", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Reads the request and deletes the file. Nothing when there is no file, it is older than
    ''' <paramref name="k_maxAge"/> (a request nobody answered long ago must not open a window by surprise) or it cannot be
    ''' read yet (the writer still holds it -- the watcher fires again). Never throws; a real failure is logged.
    ''' </summary>
    Public Shared Function Consume(k_maxAge As TimeSpan, Optional k_dir As String = Nothing) As TrayRequest
        Try
            Dim k_path As String = FilePath(k_dir)
            If Not File.Exists(k_path) Then Return Nothing
            Dim k_text As String = File.ReadAllText(k_path, Encoding.UTF8)
            File.Delete(k_path)
            If String.IsNullOrWhiteSpace(k_text) Then Return Nothing
            Dim k_request As TrayRequest = JsonSerializer.Deserialize(Of TrayRequest)(k_text)
            If k_request Is Nothing Then Return Nothing
            If DateTime.UtcNow - k_request.CreatedUtc > k_maxAge Then Return Nothing
            Return k_request
        Catch ex As IOException
            ' Still being written or held by the other process: the next change of the file brings us back.
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("TrayRequest.Consume", ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>The pending request, left on disk (the login window reads the unit before the shell exists). Never throws.</summary>
    Public Shared Function Peek(k_maxAge As TimeSpan, Optional k_dir As String = Nothing) As TrayRequest
        Try
            Dim k_path As String = FilePath(k_dir)
            If Not File.Exists(k_path) Then Return Nothing
            Dim k_request As TrayRequest = JsonSerializer.Deserialize(Of TrayRequest)(File.ReadAllText(k_path, Encoding.UTF8))
            If k_request Is Nothing OrElse DateTime.UtcNow - k_request.CreatedUtc > k_maxAge Then Return Nothing
            Return k_request
        Catch ex As IOException
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("TrayRequest.Peek", ex)
            Return Nothing
        End Try
    End Function
End Class
