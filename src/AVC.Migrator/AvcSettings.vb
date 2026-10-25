Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports KBot.Common

''' <summary>
''' What the operator typed last time, so the next launch starts where this one stopped. Registry path, DC, host, port, user and
''' the common database name - never a password.
''' </summary>
Friend NotInheritable Class AvcSettings

    Private Const FileName As String = "avc-migrator-settings.json"

    Public Property RegistryPath As String = "C:\AVACONT\cale.accdb"
    Public Property Dc As String = String.Empty
    Public Property Host As String = "89.33.25.34"
    Public Property Port As Integer = 3306
    Public Property User As String = "Admin"
    Public Property CommonDatabase As String = "AVACONT_COMUN"

    Private Shared Function FilePath() As String
        Return Path.Combine(AppContext.BaseDirectory, FileName)
    End Function

    ''' <summary>Loads the settings; a missing or damaged file gives the defaults (a damaged one is reported).</summary>
    Public Shared Function Load() As AvcSettings
        Try
            If Not File.Exists(FilePath()) Then Return New AvcSettings()
            Return If(JsonSerializer.Deserialize(Of AvcSettings)(File.ReadAllText(FilePath(), Encoding.UTF8)), New AvcSettings())
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSettings.Load", ex)
            KBotMessage.Show("Setările nu au putut fi citite. Se folosesc valorile implicite." & Environment.NewLine & ex.Message,
                             "Migrare Avacont", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return New AvcSettings()
        End Try
    End Function

    ''' <summary>Saves the settings. I/O: the caller (a UI handler) records and shows a failure.</summary>
    Public Sub Save()
        Try
            File.WriteAllText(FilePath(), JsonSerializer.Serialize(Me), Encoding.UTF8)
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSettings.Save", ex)
            Throw
        End Try
    End Sub

End Class
