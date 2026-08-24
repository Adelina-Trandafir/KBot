Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports KBot.Common

''' <summary>
''' The few things worth remembering between runs.
''' </summary>
''' <remarks>
''' <para>
''' <b>Never a secret.</b> Not the two Access passwords, not the MariaDB one, and not the
''' server's API key. They live in memory for the duration of a run and nowhere else - not
''' here, not in the log, not in the SQL journal. That is why this type has no such field at
''' all rather than a field someone could later decide to fill.
''' </para>
''' <para>
''' The plan asked for persistence "through the existing KBot.LocalStore mechanism".
''' There is no such mechanism: <c>KBot.LocalStore</c> is <c>ITempStore</c> plus
''' <c>SqliteTempStore</c>, a scratch store with Open/Reset/Dispose and no settings API at
''' all. Rather than invent one inside a migration slice, this writes a small JSON file
''' next to the executable. If a real settings store lands later, this is one class to
''' move.
''' </para>
''' </remarks>
Public NotInheritable Class MigratorSettings

    Private Const FileName As String = "migrator-settings.json"

    <JsonPropertyName("registryPath")>
    Public Property RegistryPath As String = String.Empty

    <JsonPropertyName("journalFolder")>
    Public Property JournalFolder As String = String.Empty

    <JsonPropertyName("host")>
    Public Property Host As String = "localhost"

    <JsonPropertyName("port")>
    Public Property Port As Integer = 3306

    <JsonPropertyName("user")>
    Public Property User As String = String.Empty

    <JsonPropertyName("dc")>
    Public Property Dc As String = String.Empty

    <JsonPropertyName("commonDatabase")>
    Public Property CommonDatabase As String = "AVACONT_COMUN"

    <JsonPropertyName("templateDatabase")>
    Public Property TemplateDatabase As String = "AVACONT_SURSA"

    ' ---- schema_sync, which runs ON THE SERVER over the migration API ----------------------
    '
    ' The Python project lives on the VPS and the deployed copy there is the one allowed to
    ' alter the databases. Starting it used to mean an SSH session, which in turn meant a shell
    ' account and a private key file on every operator's machine, for one step run rarely. The
    ' server exposes it as a route now, so what is left of that whole apparatus is ONE address.
    '
    ' There is no second setting beside it, and in particular no key: the route is guarded by
    ' the same X-Api-Key as the rest of the migration API, and a key is a secret, so it is
    ' typed into the form for the run and kept nowhere - like the two passwords, and for the
    ' same reason. See the note at the top of this class.

    ''' <summary>
    ''' The API server, written as <c>https://server.exemplu.ro</c>. Edited in the first box
    ''' of the server group on the form.
    ''' </summary>
    ''' <remarks>
    ''' Empty by default and deliberately NOT guessed: the address of the production server is
    ''' not written down anywhere in this repository, and inventing a plausible one is how a
    ''' tool ends up pointed at the wrong machine.
    ''' </remarks>
    <JsonPropertyName("serverUrl")>
    Public Property ServerUrl As String = String.Empty

    ''' <summary>Reads the settings file, or returns defaults when there is none.</summary>
    Public Shared Function Load() As MigratorSettings
        Try
            Dim path = SettingsPath()
            If Not File.Exists(path) Then Return Defaults()
            Dim json = File.ReadAllText(path, Encoding.UTF8)
            Dim loaded = JsonSerializer.Deserialize(Of MigratorSettings)(json)
            If loaded Is Nothing Then Return Defaults()
            ' A settings file written by the SSH-era build carries «sshTarget» and friends.
            ' They are simply ignored - System.Text.Json drops properties it does not know -
            ' and the file loses them the next time it is written. Nothing to migrate: an
            ' SSH target is not an address this build could use anyway.
            Return loaded
        Catch ex As Exception
            ' A corrupt settings file must never stop the tool starting.
            GlobalErrorLog.Write("MigratorSettings.Load", ex)
            Return Defaults()
        End Try
    End Function

    ''' <summary>Writes the settings file. Failure is logged, never surfaced.</summary>
    Public Sub Save()
        Try
            Dim options As New JsonSerializerOptions With {.WriteIndented = True}
            File.WriteAllText(SettingsPath(), JsonSerializer.Serialize(Me, options), New UTF8Encoding(False))
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorSettings.Save", ex)
        End Try
    End Sub

    Private Shared Function Defaults() As MigratorSettings
        Dim settings As New MigratorSettings()
        settings.RegistryPath = "C:\AVACONT\cale.accdb"
        settings.JournalFolder = LogPaths.Combine("Migrare")
        Return settings
    End Function

    Private Shared Function SettingsPath() As String
        Return Path.Combine(AppContext.BaseDirectory, FileName)
    End Function

End Class
