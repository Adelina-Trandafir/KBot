Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Threading
Imports KBot.Common

''' <summary>
''' What one schema-sync run produced.
''' </summary>
Public NotInheritable Class SchemaSyncResult

    Public Sub New(exitCode As Integer, output As String, commandLine As String)
        Me.ExitCode = exitCode
        Me.Output = output
        Me.CommandLine = commandLine
    End Sub

    Public ReadOnly Property ExitCode As Integer
    ''' <summary>Everything the script wrote, stdout and stderr interleaved.</summary>
    Public ReadOnly Property Output As String
    ''' <summary>The command as launched, for the log. Carries no credential.</summary>
    Public ReadOnly Property CommandLine As String

    Public ReadOnly Property Succeeded As Boolean
        Get
            Return ExitCode = 0
        End Get
    End Property

End Class

''' <summary>
''' Runs <c>routes.schema_sync.schema_sync</c> to build a target database's structure.
''' </summary>
''' <remarks>
''' <para>
''' Used for one case only: the target database EXISTS but holds no tables. Creating it
''' from <c>AVACONT_SURSA</c> (plan §4) is the migrator's own path; this is the other one,
''' for a database somebody already created empty. The operator asked for it explicitly.
''' </para>
''' <para>
''' <b>Two things about the command line are worth knowing before changing it.</b>
''' </para>
''' <para>
''' First, <c>--run</c> is REQUIRED and is not in the command as it was originally handed
''' over. Without it, <c>schema_sync.py</c> reaches <c>_ask("Executați acum?")</c> and reads
''' from stdin. Launched from a form with redirected streams that read hits EOF, so the
''' script would either abort or sit there forever while the form waits on a process that
''' is waiting on a human. With <c>--run</c> it is non-interactive, which is the only shape
''' that can work from here.
''' </para>
''' <para>
''' Second, <c>python3</c> is a Linux spelling. On this estate the interpreter that has the
''' dependencies is the repository venv, <c>PYTHON\.venv\Scripts\python.exe</c>, and the
''' module path <c>routes.schema_sync.schema_sync</c> only resolves with the working
''' directory set to <c>PYTHON\</c>. Both are settings rather than constants, so an
''' operator whose layout differs can point them somewhere else without a rebuild.
''' </para>
''' <para>
''' Every line the script writes goes to the job log, stdout and stderr alike. The script
''' talks to the server through the Python side's own configuration, so no credential from
''' this form is passed to it and none appears on the command line.
''' </para>
''' </remarks>
Public NotInheritable Class SchemaSyncRunner

    ''' <summary>The token replaced with the chosen DC in the argument template.</summary>
    Public Const DcToken As String = "{dc}"

    Private ReadOnly _pythonExecutable As String
    Private ReadOnly _workingDirectory As String
    Private ReadOnly _argumentTemplate As String
    Private ReadOnly _log As Action(Of String)

    Public Sub New(pythonExecutable As String, workingDirectory As String,
                   argumentTemplate As String, log As Action(Of String))
        _pythonExecutable = pythonExecutable
        _workingDirectory = workingDirectory
        _argumentTemplate = argumentTemplate
        _log = log
    End Sub

    ''' <summary>
    ''' Checks the interpreter and the working directory exist, so a misconfiguration is
    ''' reported in Romanian instead of as a Win32 error.
    ''' </summary>
    Public Function Validate(ByRef reason As String) As Boolean
        reason = String.Empty

        If String.IsNullOrWhiteSpace(_pythonExecutable) Then
            reason = "Calea către interpretorul Python nu este configurată."
            Return False
        End If
        If Not File.Exists(_pythonExecutable) Then
            reason = $"Interpretorul Python «{_pythonExecutable}» nu există."
            Return False
        End If
        If String.IsNullOrWhiteSpace(_workingDirectory) OrElse Not Directory.Exists(_workingDirectory) Then
            reason = $"Dosarul «{_workingDirectory}» nu există. El trebuie să fie dosarul " &
                     "«PYTHON» al depozitului, altfel modulul «routes.schema_sync.schema_sync» " &
                     "nu se rezolvă."
            Return False
        End If
        If String.IsNullOrWhiteSpace(_argumentTemplate) Then
            reason = "Argumentele pentru schema_sync nu sunt configurate."
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Runs the script for one DC, streaming its output into the log.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">The configuration is unusable.</exception>
    Public Function Run(dc As String, cancel As CancellationToken) As SchemaSyncResult
        Dim reason As String = Nothing
        If Not Validate(reason) Then Throw New InvalidOperationException(reason)

        Dim arguments = _argumentTemplate.Replace(DcToken, dc)
        Dim commandLine = $"{_pythonExecutable} {arguments}"

        Try
            Say($"Se pornește schema_sync pentru «{dc}».")
            Say($"   dosar de lucru: {_workingDirectory}")
            Say($"   comandă: {commandLine}")

            Dim startInfo As New ProcessStartInfo(_pythonExecutable, arguments) With {
                .WorkingDirectory = _workingDirectory,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .RedirectStandardInput = True,
                .CreateNoWindow = True,
                .StandardOutputEncoding = Encoding.UTF8,
                .StandardErrorEncoding = Encoding.UTF8
            }
            ' The Python side writes Romanian with diacritics; without this it would arrive
            ' as mojibake in the job log on a machine whose console codepage is not UTF-8.
            startInfo.EnvironmentVariables("PYTHONIOENCODING") = "utf-8"

            Dim collected As New StringBuilder()

            Using process As New Process()
                process.StartInfo = startInfo
                process.EnableRaisingEvents = True

                AddHandler process.OutputDataReceived,
                    Sub(sender As Object, e As DataReceivedEventArgs)
                        If e.Data Is Nothing Then Return
                        collected.AppendLine(e.Data)
                        Say("   | " & e.Data)
                    End Sub

                AddHandler process.ErrorDataReceived,
                    Sub(sender As Object, e As DataReceivedEventArgs)
                        If e.Data Is Nothing Then Return
                        collected.AppendLine(e.Data)
                        Say("   ! " & e.Data)
                    End Sub

                process.Start()
                process.BeginOutputReadLine()
                process.BeginErrorReadLine()

                ' Close stdin at once. The script should never ask anything with --run, and
                ' if a future version does, an EOF fails it fast instead of hanging the form.
                Try
                    process.StandardInput.Close()
                Catch ex As Exception
                    GlobalErrorLog.Write("SchemaSyncRunner.CloseStdin", ex)
                End Try

                While Not process.WaitForExit(250)
                    If Not cancel.IsCancellationRequested Then Continue While
                    Try
                        Say("Oprire cerută — se închide schema_sync.")
                        process.Kill(entireProcessTree:=True)
                    Catch ex As Exception
                        GlobalErrorLog.Write("SchemaSyncRunner.Kill", ex)
                    End Try
                    cancel.ThrowIfCancellationRequested()
                End While

                ' Lets the two async readers drain what is still buffered.
                process.WaitForExit()

                Dim result As New SchemaSyncResult(process.ExitCode, collected.ToString(), commandLine)
                If result.Succeeded Then
                    Say($"schema_sync s-a încheiat cu succes (cod {result.ExitCode}).")
                Else
                    Say($"schema_sync a eșuat, cod {result.ExitCode}. Citiți liniile de mai sus.")
                End If
                Return result
            End Using

        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaSyncRunner.Run", ex)
            Throw
        End Try
    End Function

    Private Sub Say(message As String)
        _log?.Invoke(message)
    End Sub

End Class
