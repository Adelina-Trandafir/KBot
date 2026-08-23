Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
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
''' Runs <c>routes.schema_sync.schema_sync</c> ON THE SERVER, over SSH, to build a target
''' database's structure.
''' </summary>
''' <remarks>
''' <para>
''' Used for one case only: the target database EXISTS but holds no tables. Creating it
''' from <c>AVACONT_SURSA</c> (plan §4) is the migrator's own path; this is the other one,
''' for a database somebody already created empty.
''' </para>
''' <para>
''' <b>The script runs on the VPS, never on the operator's machine.</b> That is the whole
''' point of this class: the migrator is a Windows form, the Python project lives on a Linux
''' server, and the deployed copy there - with its own <c>config.py</c> and its own
''' interpreter - is the one allowed to alter the databases. Driving a local checkout instead
''' would be a different codebase pointed at the same server.
''' </para>
''' <para>
''' <b>Four things about the command line are worth knowing before changing it.</b>
''' </para>
''' <para>
''' First, <c>--run</c> is REQUIRED. Without it, <c>schema_sync.py</c> reaches
''' <c>_ask("Executați acum?")</c> and reads from stdin. Over a non-interactive SSH channel
''' that read hits EOF, so the script would either abort or sit there forever while the form
''' waits on a process that is waiting on a human.
''' </para>
''' <para>
''' Second, <c>BatchMode=yes</c> is the SSH half of the same rule: it makes the client fail
''' instead of prompting for a password or a key passphrase. A prompt cannot be answered from
''' a form, so a hang is the only other outcome. It also means <b>authentication is by KEY</b> -
''' no password is asked for, typed, stored or passed anywhere by this tool.
''' </para>
''' <para>
''' Third, <c>StrictHostKeyChecking</c> is deliberately left at its default rather than set to
''' <c>accept-new</c>. A migration tool must not be the thing that silently decides to trust a
''' server it has never seen. With <c>BatchMode=yes</c> an unknown host fails fast, and
''' <see cref="HostKeyHint"/> tells the operator to connect once by hand and accept the
''' fingerprint themselves.
''' </para>
''' <para>
''' Fourth, the remote command is a SETTING, not a constant, because only the operator knows
''' where the project sits on the server. It carries the <c>cd</c> - the module path
''' <c>routes.schema_sync.schema_sync</c> only resolves from the project root - and it carries
''' <c>python3</c>, which is the correct spelling on the far side.
''' </para>
''' <para>
''' Every line the script writes goes to the job log, stdout and stderr alike. The script talks
''' to the databases through the SERVER's own configuration, so no credential from this form is
''' passed to it and none appears on the command line.
''' </para>
''' </remarks>
Public NotInheritable Class SchemaSyncRunner

    ''' <summary>The token replaced with the chosen DC in the remote command.</summary>
    Public Const DcToken As String = "{dc}"

    ''' <summary>
    ''' What a DC name is allowed to look like before it is put into a remote shell command.
    ''' </summary>
    ''' <remarks>
    ''' This is the one place where data read out of an Access file crosses into a shell on
    ''' another machine. A DC comes from <c>cai.DC</c> - operator data, not a constant - so it
    ''' is checked against a whitelist rather than escaped: a name carrying a backtick, a
    ''' semicolon or a dollar-parenthesis would otherwise RUN on the server. Every real DC
    ''' looks like <c>000_DEMO</c>, so nothing legitimate is refused.
    ''' </remarks>
    Private Shared ReadOnly SafeDc As New Regex("^[A-Za-z0-9_]+$", RegexOptions.Compiled)

    Private ReadOnly _sshExecutable As String
    Private ReadOnly _target As String
    Private ReadOnly _keyFile As String
    Private ReadOnly _port As Integer
    Private ReadOnly _remoteCommand As String
    Private ReadOnly _log As Action(Of String)

    Public Sub New(sshExecutable As String, target As String, keyFile As String, port As Integer,
                   remoteCommand As String, log As Action(Of String))
        _sshExecutable = If(sshExecutable, String.Empty).Trim()
        _target = If(target, String.Empty).Trim()
        _keyFile = If(keyFile, String.Empty).Trim()
        _port = port
        _remoteCommand = If(remoteCommand, String.Empty).Trim()
        _log = log
    End Sub

    ''' <summary>
    ''' What to tell an operator whose host key has never been accepted.
    ''' </summary>
    Public Function HostKeyHint() As String
        Return "Dacă mesajul de mai sus vorbește despre «Host key verification failed», " &
               "serverul nu este încă în «known_hosts». Deschideți o dată o consolă, rulați " &
               $"«ssh {_target}» de mână și acceptați amprenta, apoi reveniți aici. Unealta nu " &
               "acceptă singură amprenta unui server pe care nu l-a mai văzut."
    End Function

    ''' <summary>
    ''' Checks the client and the settings, so a misconfiguration is reported in Romanian
    ''' instead of as a Win32 error or a remote shell error.
    ''' </summary>
    ''' <remarks>
    ''' The REMOTE folder cannot be checked from here - a wrong <c>cd</c> shows up as
    ''' «No such file or directory» in the log, immediately and loudly, on the first run.
    ''' </remarks>
    Public Function Validate(ByRef reason As String) As Boolean
        reason = String.Empty

        If _sshExecutable.Length = 0 Then
            reason = "Calea către «ssh.exe» nu este configurată."
            Return False
        End If
        ' A bare name is left to PATH on purpose; anything with a separator must really exist.
        If _sshExecutable.IndexOfAny(New Char() {"\"c, "/"c}) >= 0 AndAlso Not File.Exists(_sshExecutable) Then
            reason = $"Clientul SSH «{_sshExecutable}» nu există."
            Return False
        End If
        If _target.Length = 0 Then
            reason = "Serverul nu este configurat. Se scrie ca «utilizator@gazdă» în " &
                     "«sshTarget», în «migrator-settings.json»."
            Return False
        End If
        If _keyFile.Length > 0 AndAlso Not File.Exists(_keyFile) Then
            reason = $"Cheia SSH «{_keyFile}» nu există."
            Return False
        End If
        If _port <= 0 OrElse _port > 65535 Then
            reason = $"Portul SSH «{_port}» nu este valid."
            Return False
        End If
        If _remoteCommand.Length = 0 Then
            reason = "Comanda care se rulează pe server nu este configurată " &
                     "(«schemaSyncRemoteCommand»)."
            Return False
        End If
        If Not _remoteCommand.Contains(DcToken) Then
            reason = $"Comanda pentru server nu conține «{DcToken}», deci nu ar ști pe care " &
                     "bază să lucreze."
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Runs the script for one DC on the server, streaming its output into the log.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">
    ''' The configuration is unusable, or the DC name is not safe to send.
    ''' </exception>
    Public Function Run(dc As String, cancel As CancellationToken) As SchemaSyncResult
        Dim reason As String = Nothing
        If Not Validate(reason) Then Throw New InvalidOperationException(reason)

        If String.IsNullOrWhiteSpace(dc) OrElse Not SafeDc.IsMatch(dc) Then
            Throw New InvalidOperationException(
                $"Numele bazei «{dc}» conține caractere care nu pot fi trimise într-o comandă " &
                "pe server. Sunt permise doar litere, cifre și liniuța de subliniere.")
        End If

        ' PYTHONIOENCODING is set on the FAR side: the Python code writes Romanian with
        ' diacritics, and without it the output arrives as mojibake whenever the server's
        ' locale is not UTF-8. Prepended here rather than left inside the setting, so an
        ' operator editing the command cannot drop it by accident.
        Dim remote = "export PYTHONIOENCODING=utf-8; " & _remoteCommand.Replace(DcToken, dc)
        Dim commandLine = $"ssh {_target} «{remote}»"

        Try
            Say($"Se pornește schema_sync PE SERVER, pentru «{dc}».")
            Say($"   server: {_target}   port: {_port}")
            If _keyFile.Length > 0 Then Say($"   cheie: {_keyFile}")
            Say($"   comandă: {remote}")

            Dim startInfo As New ProcessStartInfo(_sshExecutable) With {
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .RedirectStandardInput = True,
                .CreateNoWindow = True,
                .StandardOutputEncoding = Encoding.UTF8,
                .StandardErrorEncoding = Encoding.UTF8
            }

            ' ArgumentList, not one concatenated string: it quotes each argument the way the
            ' process expects, so the remote command travels as ONE argument even though it
            ' carries spaces, semicolons and quotes of its own.
            With startInfo.ArgumentList
                .Add("-n")                                  ' never read our stdin
                .Add("-o")
                .Add("BatchMode=yes")                       ' fail, never prompt
                .Add("-p")
                .Add(_port.ToString(CultureInfo.InvariantCulture))
                If _keyFile.Length > 0 Then
                    .Add("-i")
                    .Add(_keyFile)
                    ' Only this key: otherwise an agent may offer others first and the server
                    ' can drop the connection for too many failed attempts.
                    .Add("-o")
                    .Add("IdentitiesOnly=yes")
                End If
                .Add(_target)
                .Add(remote)
            End With

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

                ' Close stdin at once. Nothing should ever ask us anything - BatchMode on the
                ' client, --run on the script - and if something does, an EOF fails it fast
                ' instead of hanging the form.
                Try
                    process.StandardInput.Close()
                Catch ex As Exception
                    GlobalErrorLog.Write("SchemaSyncRunner.CloseStdin", ex)
                End Try

                While Not process.WaitForExit(250)
                    If Not cancel.IsCancellationRequested Then Continue While
                    Try
                        ' Killing the client ends the channel; whether the remote script stops
                        ' with it is the server's business, so the message says «conexiunea»,
                        ' not «schema_sync». A half-applied schema is exactly what the
                        ' re-verify that follows is there to find.
                        Say("Oprire cerută — se închide conexiunea SSH.")
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
                    ' 255 is the SSH client's OWN failure code - the connection or the
                    ' authentication, not the script. Worth naming, because the operator would
                    ' otherwise go looking on the server for a script that never ran.
                    If result.ExitCode = 255 Then
                        Say("Codul 255 vine de la clientul SSH, nu de la script: conexiunea " &
                            "sau autentificarea a eșuat, deci scriptul nu a pornit.")
                        Say(HostKeyHint())
                    End If
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
