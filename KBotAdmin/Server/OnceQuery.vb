Option Strict On
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' Builds the command lines of the one-time query runner (<c>routes/one_time/runner.py</c>), the same ones
''' AvacontPush builds (slice ME01). The query text travels on the command line as base64 (UTF-8), so
''' nothing has to be uploaded and the quoting of the SQL cannot go wrong. Runs nothing itself.
''' </summary>
Public NotInheritable Class OnceQuery

    ''' <summary>Runner exit codes: 2 = refused (nothing executed), 1 = at least one database failed.</summary>
    Public Const ExitFailed As Integer = 1
    Public Const ExitRefused As Integer = 2

    ''' <summary>One SSH packet carries the command line; base64 adds a third.</summary>
    Public Const MaxSqlBytes As Integer = 30000

    Private Shared ReadOnly NamePattern As New Regex("^[A-Za-z0-9_.\-]{3,100}$", RegexOptions.Compiled)

    Private Sub New()
    End Sub

    ''' <summary>Empty = the name is usable; otherwise what is wrong with it.</summary>
    Public Shared Function CheckName(k_name As String) As String
        If String.IsNullOrWhiteSpace(k_name) Then Return "Dați un nume interogării."
        If Not NamePattern.IsMatch(k_name.Trim()) Then Return "Numele are 3-100 de caractere: doar litere, cifre, _ . -"
        Return String.Empty
    End Function

    ''' <summary>Empty = the text fits on a command line; otherwise why it does not.</summary>
    Public Shared Function CheckSize(k_sql As String) As String
        Dim k_size As Integer = Encoding.UTF8.GetByteCount(If(k_sql, String.Empty))
        If k_size = 0 Then Return "Scrieți sau încărcați interogarea."
        If k_size > MaxSqlBytes Then
            Return $"Interogarea are {k_size} octeți; limita este {MaxSqlBytes}. Împărțiți-o în mai multe interogări."
        End If
        Return String.Empty
    End Function

    Private Shared Function Prefix(k_settings As AdminSettings) As String
        Return "cd " & AdminClient.Quote(k_settings.RemoteRoot) & " && PYTHONIOENCODING=utf-8 PYTHONUNBUFFERED=1 " &
               AdminClient.Quote(k_settings.RemotePython) & " -m routes.one_time.runner"
    End Function

    ''' <summary>View = show what would run on each database and execute nothing; otherwise execute.</summary>
    Public Shared Function RunCommand(k_settings As AdminSettings, k_name As String, k_sql As String,
                                      k_view As Boolean, k_targets As IEnumerable(Of String)) As String
        Dim k_problem As String = CheckName(k_name)
        If k_problem = String.Empty Then k_problem = CheckSize(k_sql)
        If k_problem <> String.Empty Then Throw New ApplicationException(k_problem)
        If k_targets Is Nothing OrElse Not k_targets.Any() Then Throw New ApplicationException("Bifați cel puțin o bază.")
        Dim k_b64 As String = Convert.ToBase64String(Encoding.UTF8.GetBytes(k_sql))
        Return Prefix(k_settings) & " --name " & AdminClient.Quote(k_name.Trim()) & " --sql-b64 " & AdminClient.Quote(k_b64) &
               " --targets " & AdminClient.Quote(String.Join(",", k_targets)) & If(k_view, " --view", " --run")
    End Function

    ''' <summary>
    ''' «Interogare pe SURSA»: the text runs on AVACONT_SURSA alone (<c>--only-source</c>); the units are not touched.
    ''' View = show what would run and execute nothing.
    ''' </summary>
    Public Shared Function SourceCommand(k_settings As AdminSettings, k_name As String, k_sql As String, k_view As Boolean) As String
        Dim k_problem As String = CheckName(k_name)
        If k_problem = String.Empty Then k_problem = CheckSize(k_sql)
        If k_problem <> String.Empty Then Throw New ApplicationException(k_problem)
        Dim k_b64 As String = Convert.ToBase64String(Encoding.UTF8.GetBytes(k_sql))
        Return Prefix(k_settings) & " --name " & AdminClient.Quote(k_name.Trim()) & " --sql-b64 " & AdminClient.Quote(k_b64) &
               " --only-source" & If(k_view, " --view", " --run")
    End Function

    ''' <summary>
    ''' Writes the ledger row of a query that ran on AVACONT_SURSA into the units, without executing it (<c>--mark</c>): run after the
    ''' schema sync carried the structure, so the units do not run the text a second time.
    ''' </summary>
    Public Shared Function MarkCommand(k_settings As AdminSettings, k_name As String) As String
        Dim k_problem As String = CheckName(k_name)
        If k_problem <> String.Empty Then Throw New ApplicationException(k_problem)
        Return Prefix(k_settings) & " --name " & AdminClient.Quote(k_name.Trim()) & " --mark"
    End Function

    ''' <summary>What every database's ledger holds.</summary>
    Public Shared Function StatusCommand(k_settings As AdminSettings) As String
        Return Prefix(k_settings) & " --status"
    End Function

    ''' <summary>The runner's exit codes in plain words.</summary>
    Public Shared Function Describe(k_exitStatus As Integer, k_view As Boolean) As String
        Select Case k_exitStatus
            Case 0
                Return If(k_view, "Verificare completă. Nu s-a executat nimic.", "Interogare unică încheiată.")
            Case ExitRefused
                Return "Refuzat. Nimic nu s-a executat. Citiți rezultatul."
            Case ExitFailed
                Return "Terminat cu erori pe unele baze (rămân nemarcate). Citiți rezultatul."
            Case Else
                Return "Interogarea s-a încheiat cu codul " & k_exitStatus.ToString(Globalization.CultureInfo.InvariantCulture) & ". Citiți rezultatul."
        End Select
    End Function

End Class
