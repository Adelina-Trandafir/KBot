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

    ''' <summary>The Python interpreter that runs schema_sync.</summary>
    ''' <remarks>
    ''' Not "python3": that is a Linux spelling, and the interpreter that carries the
    ''' dependencies on this estate is the repository venv.
    ''' </remarks>
    <JsonPropertyName("pythonExecutable")>
    Public Property PythonExecutable As String = String.Empty

    ''' <summary>
    ''' The folder schema_sync is launched from - the repository's <c>PYTHON</c> folder.
    ''' </summary>
    ''' <remarks>
    ''' The module path <c>routes.schema_sync.schema_sync</c> only resolves from there.
    ''' </remarks>
    <JsonPropertyName("pythonWorkingFolder")>
    Public Property PythonWorkingFolder As String = String.Empty

    ''' <summary>
    ''' The schema_sync argument template. <c>{dc}</c> is replaced with the chosen DC.
    ''' </summary>
    ''' <remarks>
    ''' <c>--run</c> is deliberate and must stay: without it the script asks
    ''' «Executați acum?» on stdin, which cannot be answered from a form.
    ''' </remarks>
    <JsonPropertyName("schemaSyncArguments")>
    Public Property SchemaSyncArguments As String =
        "-m routes.schema_sync.schema_sync --mode FORCE --targets {dc} --run"

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
        settings.PythonWorkingFolder = GuessPythonFolder()
        settings.PythonExecutable = GuessPythonExecutable(settings.PythonWorkingFolder)
        Return settings
    End Function

    ''' <summary>
    ''' Walks up from the executable looking for the repository's <c>PYTHON</c> folder.
    ''' </summary>
    ''' <remarks>
    ''' A guess, and only a starting value - the operator can point it anywhere. It walks up
    ''' rather than assuming a fixed depth because the app runs from bin\Debug\net8.0-windows
    ''' during development and from the install folder afterwards.
    ''' </remarks>
    Private Shared Function GuessPythonFolder() As String
        Try
            Dim folder As New DirectoryInfo(AppContext.BaseDirectory)
            For depth = 0 To 7
                If folder Is Nothing Then Exit For
                Dim candidate = Path.Combine(folder.FullName, "PYTHON")
                If Directory.Exists(Path.Combine(candidate, "routes", "schema_sync")) Then Return candidate
                folder = folder.Parent
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorSettings.GuessPythonFolder", ex)
        End Try
        Return String.Empty
    End Function

    Private Shared Function GuessPythonExecutable(pythonFolder As String) As String
        Try
            If pythonFolder.Length > 0 Then
                Dim venv = Path.Combine(pythonFolder, ".venv", "Scripts", "python.exe")
                If File.Exists(venv) Then Return venv
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MigratorSettings.GuessPythonExecutable", ex)
        End Try
        Return String.Empty
    End Function

    Private Shared Function SettingsPath() As String
        Return Path.Combine(AppContext.BaseDirectory, FileName)
    End Function

End Class
