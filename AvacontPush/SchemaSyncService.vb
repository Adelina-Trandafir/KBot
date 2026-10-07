Imports System.Collections.Generic
Imports System.Text

' Builds the remote schema_sync command lines and reads back what the listing
' prints. Runs nothing itself - SshCommandService does that.
'
' The module is invoked from inside the pushed tree (RemoteRoot) because it does
' "from config import DB_CONFIG", and config.py is host-only: it is not in the
' repository and never travels with a push. That is also why the list of unit
' databases can only come from the server.
Public NotInheritable Class SchemaSyncService

    ' Marker starting every data line of --list-targets. Anything else on
    ' stdout (a log line, a warning) is ignored by the parser, so a database
    ' name can never be confused with a message.
    Private Const TargetLinePrefix As String = "DB" & vbTab

    ' Exit code the tool returns when a run would execute destructive DDL and
    ' --allow-destructive was not given. NOTHING has been executed at that point:
    ' the refusal is total, not selective.
    Public Const ExitDestructiveRefused As Integer = 2

    Private ReadOnly _settings As PushSettings

    Public Sub New(settings As PushSettings)
        _settings = settings
    End Sub

    ' "cd '<RemoteRoot>' && " - everything runs from the pushed tree, and a
    ' failed cd stops the chain instead of running the module from elsewhere.
    Private Function CdPrefix() As String
        Return $"cd {Quote(_settings.RemoteRoot)} && "
    End Function

    ' Without PYTHONIOENCODING a server under the C locale raises
    ' UnicodeEncodeError on the first Romanian message with diacritics, because
    ' here stdout is a pipe rather than a terminal and Python takes its encoding
    ' from the locale.
    Private Function Invocation() As String
        Return $"PYTHONIOENCODING=utf-8 {Quote(_settings.RemotePython)} " &
               "-m routes.schema_sync.schema_sync"
    End Function

    Public Function ListCommand() As String
        Return CdPrefix() & Invocation() & " --list-targets"
    End Function

    ' view:=True generates the statements and shows them without executing.
    '
    ' allowDestructive also pipes the typed DA the tool asks for before
    ' destructive DDL. stdin has no terminal here, so the question would
    ' otherwise read end-of-file and cancel the run; the operator has already
    ' answered exactly that question in a dialog. The pipe sits INSIDE the cd
    ' chain - "a | b && c" would otherwise feed the DA to cd, not to Python.
    Public Function SyncCommand(mode As String, targets As IEnumerable(Of String),
                                view As Boolean, allowDestructive As Boolean) As String
        Dim sb As New StringBuilder()
        sb.Append(CdPrefix())
        If allowDestructive Then sb.Append("printf 'DA\n' | ")
        sb.Append(Invocation())
        sb.Append(" --mode ").Append(mode)
        sb.Append(" --targets ").Append(Quote(String.Join(",", targets)))
        ' Never the default interactive mode: "Executați acum?" would read
        ' end-of-file, take the last answer (nu) and exit 0 - a cancelled run
        ' that looks exactly like a clean one.
        sb.Append(If(view, " --view", " --run"))
        If allowDestructive Then sb.Append(" --allow-destructive")
        Return sb.ToString()
    End Function

    ' Reads the "DB<tab>name<tab>CAI|-<tab>EXISTS|MISSING" lines of
    ' --list-targets. A line without the fourth field is taken as EXISTS, so an
    ' older server still produces a usable list rather than one where nothing
    ' can be ticked.
    Public Shared Function ParseTargets(stdOut As String) As List(Of SchemaTarget)
        Dim list As New List(Of SchemaTarget)()
        If String.IsNullOrEmpty(stdOut) Then Return list

        For Each line In stdOut.Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)
            If Not line.StartsWith(TargetLinePrefix, StringComparison.Ordinal) Then Continue For

            Dim parts = line.Split(CChar(vbTab))
            If parts.Length < 2 Then Continue For

            Dim name = parts(1).Trim()
            If name = "" Then Continue For

            list.Add(New SchemaTarget With {
                .Name = name,
                .InCai = parts.Length > 2 AndAlso
                         String.Equals(parts(2).Trim(), "CAI", StringComparison.Ordinal),
                .Exists = parts.Length < 4 OrElse
                          Not String.Equals(parts(3).Trim(), "MISSING", StringComparison.Ordinal)
            })
        Next

        Return list
    End Function

    ' POSIX single-quoting: everything inside single quotes is literal, and an
    ' embedded quote is closed, escaped, reopened.
    Private Shared Function Quote(value As String) As String
        Return "'" & If(value, "").Replace("'", "'\''") & "'"
    End Function

End Class
