Option Strict On
Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization

''' <summary>
''' How KBotAdmin reaches the server (slice ME01). The file <c>admin_settings.json</c> lives next
''' to the exe and is git-ignored. The password (or the key's passphrase) is typed every run and is
''' never written anywhere.
''' </summary>
Public Class AdminSettings
    Public Property Host As String = "89.33.25.34"
    Public Property Port As Integer = 27015
    Public Property User As String = "root"

    ''' <summary>Never persisted: the SSH password, or the passphrase of the key.</summary>
    <JsonIgnore>
    Public Property Password As String = String.Empty

    ''' <summary>Empty = ~/.ssh/config, then ~/.ssh/id_ed25519, id_ecdsa, id_rsa (see SshAuth).</summary>
    Public Property PrivateKeyPath As String = String.Empty

    Public Property RemoteRoot As String = "/root/AVACONT"
    Public Property RemotePython As String = "/root/AVACONT/.venv/bin/python"
    Public Property HostKeyFingerprint As String = String.Empty

    ''' <summary>The folder pushed to the server. Empty = the PYTHON folder next to KBot.sln (see <see cref="EffectiveLocalRoot"/>).</summary>
    Public Property LocalRoot As String = String.Empty

    ''' <summary>After an upload the remote time is set to the local one, so a new scan sees the file as identical.</summary>
    Public Property PreserveMTime As Boolean = True

    ''' <summary>Flask address for the admin routes of the Login page. https only.</summary>
    Public Property ApiBaseUrl As String = "https://kbot.avatarsoft.ro"

    ''' <summary>Folders and patterns the scan skips (a plain name = any path segment, *.ext = by extension).</summary>
    Public Property IgnorePatterns As List(Of String) = New List(Of String) From {
        ".git", ".venv", "venv", "__pycache__", "*.pyc", ".vscode", ".vs", "bin", "obj"
    }

    ''' <summary>The only extensions that are pushed (no leading dot).</summary>
    Public Property IncludeExtensions As List(Of String) = New List(Of String) From {
        "py", "json", "xml", "yaml", "yml", "ini", "cfg", "toml", "txt", "sql", "html", "css", "js", "md", "png", "svg", "jpg", "webp", "ico"
    }

    ''' <summary>The folder to push: the setting, else the PYTHON folder of the repository this exe was built in.</summary>
    Public Function EffectiveLocalRoot() As String
        If Not String.IsNullOrWhiteSpace(LocalRoot) Then Return LocalRoot.Trim()
        Dim k_dir As DirectoryInfo = New DirectoryInfo(AppContext.BaseDirectory)
        Do While k_dir IsNot Nothing
            If File.Exists(Path.Combine(k_dir.FullName, "KBot.sln")) Then Return Path.Combine(k_dir.FullName, "PYTHON")
            k_dir = k_dir.Parent
        Loop
        Return String.Empty
    End Function
End Class

Public Module AdminSettingsStore

    Private Const FileNameOnly As String = "admin_settings.json"

    Public Function SettingsPath() As String
        Return Path.Combine(AppContext.BaseDirectory, FileNameOnly)
    End Function

    ''' <summary>Defaults when the file is absent. A file that exists but cannot be read throws.</summary>
    Public Function Load() As AdminSettings
        Try
            Dim k_path As String = SettingsPath()
            If Not File.Exists(k_path) Then Return New AdminSettings()
            Dim k_options As New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True}
            Dim k_result As AdminSettings = JsonSerializer.Deserialize(Of AdminSettings)(File.ReadAllText(k_path), k_options)
            If k_result Is Nothing Then Return New AdminSettings()
            Dim k_defaults As New AdminSettings()
            If String.IsNullOrWhiteSpace(k_result.RemoteRoot) Then k_result.RemoteRoot = k_defaults.RemoteRoot
            If String.IsNullOrWhiteSpace(k_result.RemotePython) Then k_result.RemotePython = k_defaults.RemotePython
            If k_result.IgnorePatterns Is Nothing OrElse k_result.IgnorePatterns.Count = 0 Then k_result.IgnorePatterns = k_defaults.IgnorePatterns
            If k_result.IncludeExtensions Is Nothing OrElse k_result.IncludeExtensions.Count = 0 Then k_result.IncludeExtensions = k_defaults.IncludeExtensions
            If String.IsNullOrWhiteSpace(k_result.ApiBaseUrl) Then k_result.ApiBaseUrl = k_defaults.ApiBaseUrl
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AdminSettingsStore.Load", ex)
            Throw
        End Try
    End Function

    Public Sub Save(k_settings As AdminSettings)
        Try
            Dim k_options As New JsonSerializerOptions With {.WriteIndented = True}
            File.WriteAllText(SettingsPath(), JsonSerializer.Serialize(k_settings, k_options))
        Catch ex As Exception
            GlobalErrorLog.Write("AdminSettingsStore.Save", ex)
            Throw
        End Try
    End Sub

End Module
