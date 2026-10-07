Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions

' Builds the remote command lines of the one-time query runner
' (routes/one_time/runner.py, slice 0103). Runs nothing itself - SshCommandService does that.
'
' The query text travels on the command line as base64 (UTF-8), so no file has to be uploaded and no
' quoting of the SQL can go wrong. The module is invoked from inside the pushed tree (RemoteRoot)
' because it does "from config import ...", and config.py is host-only: it never travels with a push.
Public NotInheritable Class OneTimeService

    ' Exit codes of the runner. 2 = refused, nothing executed (bad input, or the same name already
    ' used by a different text); 1 = at least one database failed.
    Public Const ExitFailed As Integer = 1
    Public Const ExitRefused As Integer = 2

    ' A command line rides in ONE SSH packet (SSH.NET: ~68 KB). Base64 adds a third, so the text
    ' stays well below that.
    Public Const MaxSqlBytes As Integer = 30000

    Private Shared ReadOnly NamePattern As New Regex("^[A-Za-z0-9_.\-]{3,100}$", RegexOptions.Compiled)

    Private ReadOnly _settings As PushSettings

    Public Sub New(settings As PushSettings)
        _settings = settings
    End Sub

    ' Empty = the name is usable; otherwise what is wrong with it, in the operator's words.
    Public Shared Function CheckName(name As String) As String
        If String.IsNullOrWhiteSpace(name) Then Return "Dați un nume interogării."
        If Not NamePattern.IsMatch(name.Trim()) Then
            Return "Numele are 3-100 de caractere: doar litere, cifre, _ . -"
        End If
        Return ""
    End Function

    ' Empty = the text fits on a command line; otherwise why it does not.
    Public Shared Function CheckSize(sql As String) As String
        Dim size = Encoding.UTF8.GetByteCount(If(sql, ""))
        If size = 0 Then Return "Scrieți sau încărcați interogarea."
        If size > MaxSqlBytes Then
            Return $"Interogarea are {size} octeți; limita este {MaxSqlBytes}. Împărțiți-o în mai multe interogări."
        End If
        Return ""
    End Function

    Private Function CdPrefix() As String
        Return $"cd {Quote(_settings.RemoteRoot)} && "
    End Function

    ' PYTHONIOENCODING: stdout is a pipe here, so Romanian messages need an explicit encoding.
    Private Function Invocation() As String
        Return $"PYTHONIOENCODING=utf-8 {Quote(_settings.RemotePython)} -m routes.one_time.runner"
    End Function

    ' view:=True shows what would run on each database and executes nothing (not even the ledger
    ' table is created). view:=False executes. Never an interactive mode: there is no terminal.
    ' targets: the unit databases the operator ticked (AVACONT_SURSA is added by the runner, always
    ' first).
    Public Function RunCommand(name As String, sql As String, view As Boolean,
                               targets As IEnumerable(Of String)) As String
        Dim problem = CheckName(name)
        If problem <> "" Then Throw New ApplicationException(problem)
        problem = CheckSize(sql)
        If problem <> "" Then Throw New ApplicationException(problem)
        If targets Is Nothing OrElse Not targets.Any() Then
            Throw New ApplicationException("Bifați cel puțin o bază (după «Citește bazele»).")
        End If

        Dim b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(sql))
        Return CdPrefix() & Invocation() &
               " --name " & Quote(name.Trim()) &
               " --sql-b64 " & Quote(b64) &
               " --targets " & Quote(String.Join(",", targets)) &
               If(view, " --view", " --run")
    End Function

    ' What each database's ledger holds.
    Public Function StatusCommand() As String
        Return CdPrefix() & Invocation() & " --status"
    End Function

    ' The command as the output pane and the run log show it: without the base64 blob.
    Public Shared Function Describe(name As String, view As Boolean) As String
        Return $"interogare unică «{name.Trim()}» ({If(view, "vezi", "execută")})"
    End Function

    ' POSIX single-quoting: everything inside single quotes is literal, and an embedded quote is
    ' closed, escaped, reopened.
    Private Shared Function Quote(value As String) As String
        Return "'" & If(value, "").Replace("'", "'\''") & "'"
    End Function

End Class
