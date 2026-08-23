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
''' <b>Never a password.</b> Not the two Access ones, not the MariaDB one. They live in
''' memory for the duration of a run and nowhere else - not here, not in the log, not in
''' the SQL journal. That is why this type has no password field at all rather than a
''' field someone could later decide to fill.
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

    ' ---- schema_sync, which runs ON THE SERVER over SSH -----------------------------------
    '
    ' The Python project lives on the VPS and the deployed copy there is the one allowed to
    ' alter the databases, so none of these describe anything on the operator's machine except
    ' the SSH client itself. Authentication is by KEY - see SchemaSyncRunner for why a password
    ' cannot work here even if someone wanted one.

    ''' <summary>The SSH client. A bare name is resolved from PATH.</summary>
    <JsonPropertyName("sshExecutable")>
    Public Property SshExecutable As String = String.Empty

    ''' <summary>The server, written as <c>user@host</c>.</summary>
    ''' <remarks>
    ''' Empty by default and deliberately NOT guessed: the address of the production server is
    ''' not written down anywhere in this repository, and inventing a plausible one is how a
    ''' tool ends up pointed at the wrong machine.
    ''' </remarks>
    <JsonPropertyName("sshTarget")>
    Public Property SshTarget As String = String.Empty

    ''' <summary>The private key file. Empty means "whatever ssh would use by default".</summary>
    <JsonPropertyName("sshKeyFile")>
    Public Property SshKeyFile As String = String.Empty

    <JsonPropertyName("sshPort")>
    Public Property SshPort As Integer = 22

    ''' <summary>
    ''' The command run on the SERVER. <c>{dc}</c> is replaced with the chosen DC.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The folder in the default is a PLACEHOLDER - only the operator knows where the project
    ''' sits on the server, it cannot be checked from here, and a wrong one shows up as
    ''' «No such file or directory» on the first run. The <c>cd</c> itself is not optional:
    ''' the module path <c>routes.schema_sync.schema_sync</c> only resolves from the project
    ''' root.
    ''' </para>
    ''' <para>
    ''' <c>--run</c> is deliberate and must stay: without it the script asks
    ''' «Executați acum?» on stdin, which cannot be answered over a non-interactive channel.
    ''' </para>
    ''' </remarks>
    <JsonPropertyName("schemaSyncRemoteCommand")>
    Public Property SchemaSyncRemoteCommand As String =
        "cd /var/www/AVACONT-PY && python3 -m routes.schema_sync.schema_sync " &
        "--mode FORCE --targets {dc} --run"

    ''' <summary>Reads the settings file, or returns defaults when there is none.</summary>
    Public Shared Function Load() As MigratorSettings
        Try
            Dim path = SettingsPath()
            If Not File.Exists(path) Then Return Defaults()
            Dim json = File.ReadAllText(path, Encoding.UTF8)
            Dim loaded = JsonSerializer.Deserialize(Of MigratorSettings)(json)
            Return If(loaded, Defaults())
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
        settings.SshExecutable = GuessSshExecutable()
        Return settings
    End Function

    ''' <summary>
    ''' Finds the SSH client that ships with Windows.
    ''' </summary>
    ''' <remarks>
    ''' Only the CLIENT is guessed - it is a fixed part of the operating system since Windows
    ''' 10, so there is a right answer. The server, the key and the remote folder are not
    ''' guessed at all: none of them is recorded anywhere in this repository, and a plausible
    ''' invention would be worse than an empty field, which at least says so.
    ''' </remarks>
    Private Shared Function GuessSshExecutable() As String
        Try
            Dim shipped = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System), "OpenSSH", "ssh.exe")
            If File.Exists(shipped) Then Return shipped
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorSettings.GuessSshExecutable", ex)
        End Try
        ' Left to PATH. Validate() accepts a bare name for exactly this case.
        Return "ssh.exe"
    End Function

    Private Shared Function SettingsPath() As String
        Return Path.Combine(AppContext.BaseDirectory, FileName)
    End Function

End Class
