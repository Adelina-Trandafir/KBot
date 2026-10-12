Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports KBot.Common

''' <summary>Host, port and user of the last run, next to the executable. Never a password. Same defaults as ADE.Migrator.</summary>
Public NotInheritable Class VrcSettings

    Private Const FileName As String = "vrc-migrator-settings.json"

    Public Property Host As String = "89.33.25.34"
    Public Property Port As Integer = 3306
    Public Property User As String = "Admin"

    Private Shared Function FilePath() As String
        Return Path.Combine(AppContext.BaseDirectory, FileName)
    End Function

    Public Shared Function Load() As VrcSettings
        Try
            If Not File.Exists(FilePath()) Then Return New VrcSettings()
            Return If(JsonSerializer.Deserialize(Of VrcSettings)(File.ReadAllText(FilePath(), Encoding.UTF8)), New VrcSettings())
        Catch ex As Exception
            GlobalErrorLog.Write("VrcSettings.Load", ex)   ' a damaged file must not stop the tool: start from the defaults
            Return New VrcSettings()
        End Try
    End Function

    Public Sub Save()
        Try
            File.WriteAllText(FilePath(), JsonSerializer.Serialize(Me), Encoding.UTF8)
        Catch ex As Exception
            GlobalErrorLog.Write("VrcSettings.Save", ex)   ' remembering the server is a convenience; the run goes on
        End Try
    End Sub

End Class
