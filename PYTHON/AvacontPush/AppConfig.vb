Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Text.Json.Serialization

' Plaintext configuration (strategy C, chosen by the user).
' The file push_settings.json lives next to the EXE and is git-ignored.
Public Class PushSettings
    Public Property Host As String = "89.33.25.34"
    Public Property Port As Integer = 27015
    Public Property User As String = "root"

    ' Never persisted. The password is typed every run and lives only in memory,
    ' so it is never written to push_settings.json nor read back from it.
    <JsonIgnore>
    Public Property Password As String = ""

    Public Property LocalRoot As String = ""
    Public Property RemoteRoot As String = "/root/AVACONT"
    Public Property PreserveMTime As Boolean = True
    Public Property HostKeyFingerprint As String = ""

    ' Folders/patterns skipped by the scan. Editable in the config file.
    ' Entries without a leading "*." match any path segment (folder or file name).
    ' Entries like "*.pyc" match by file extension. Build/IDE output is skipped so
    ' the app never lists its own compiled files (bin, obj, .vs).
    Public Property IgnorePatterns As List(Of String) = New List(Of String) From {
        ".git", ".venv", "venv", "__pycache__", "*.pyc", ".vscode", ".vs", "bin", "obj"
    }

    ' Allow-list of file extensions to push (without the leading dot). Only these are
    ' scanned/pushed - the source is Python plus its related config/asset files, never
    ' build output or binaries. Editable in the config file.
    Public Property IncludeExtensions As List(Of String) = New List(Of String) From {
        "py", "json", "xml", "yaml", "yml", "ini", "cfg", "toml", "txt", "sql", "html", "css", "js", "md"
    }
End Class

Public Module AppConfigStore

    Private Const ConfigFileName As String = "push_settings.json"

    Public Function ConfigPath() As String
        Return Path.Combine(AppContext.BaseDirectory, ConfigFileName)
    End Function

    ' Returns defaults when the file is absent. Throws (with a Romanian message)
    ' when the file exists but cannot be read or parsed - never swallowed.
    Public Function Load() As PushSettings
        Dim fullPath = ConfigPath()
        If Not File.Exists(fullPath) Then
            Return New PushSettings()
        End If

        Dim json As String
        Try
            json = File.ReadAllText(fullPath)
        Catch ex As IOException
            Throw New ApplicationException("Nu s-a putut citi fișierul de configurare push_settings.json.", ex)
        End Try

        Dim opts As New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True}
        Dim result As PushSettings
        Try
            result = JsonSerializer.Deserialize(Of PushSettings)(json, opts)
        Catch ex As JsonException
            Throw New ApplicationException("Fișierul de configurare push_settings.json nu este valid (JSON incorect).", ex)
        End Try

        If result Is Nothing Then
            Return New PushSettings()
        End If

        Normalize(result)
        Return result
    End Function

    ' Backfills lists that an older/partial config file may be missing, so a config
    ' written before these fields existed still skips build output and honors the
    ' extension allow-list. The union is persisted on the next Save.
    Private Sub Normalize(settings As PushSettings)
        Dim defaults As New PushSettings()

        If settings.IncludeExtensions Is Nothing OrElse settings.IncludeExtensions.Count = 0 Then
            settings.IncludeExtensions = defaults.IncludeExtensions
        End If

        If settings.IgnorePatterns Is Nothing Then
            settings.IgnorePatterns = New List(Of String)()
        End If
        For Each d In defaults.IgnorePatterns
            If Not settings.IgnorePatterns.Any(Function(x) String.Equals(If(x, "").Trim(), d, StringComparison.OrdinalIgnoreCase)) Then
                settings.IgnorePatterns.Add(d)
            End If
        Next
    End Sub

    ' Writes the config back as indented plaintext JSON.
    Public Sub Save(settings As PushSettings)
        Dim opts As New JsonSerializerOptions With {.WriteIndented = True}
        Dim json = JsonSerializer.Serialize(settings, opts)
        Try
            File.WriteAllText(ConfigPath(), json)
        Catch ex As IOException
            Throw New ApplicationException("Nu s-a putut salva fișierul de configurare push_settings.json.", ex)
        End Try
    End Sub

End Module
