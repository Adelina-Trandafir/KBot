Option Strict On
Imports System
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms

''' <summary>
''' Entry point of KBot.Updater.exe (slice 0067). KBot.App copies this exe to %TEMP%, starts
''' it with the package path and its own process id, and exits; this process waits, writes
''' the package over the installation folder and starts the app again.
''' </summary>
Friend Module Program

    <STAThread>
    Friend Function Main(argv As String()) As Integer
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Dim args As UpdaterArgs
        Try
            args = UpdaterArgs.Parse(If(argv, Array.Empty(Of String)()))
        Catch ex As ArgumentException
            MessageBox.Show(
                "KBot.Updater se pornește doar din aplicația K-BOT, nu direct." & Environment.NewLine & Environment.NewLine & ex.Message,
                "Actualizare K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return 2
        End Try

        Dim log As New UpdaterLog(args.TargetDir)
        Try
            Using form As New UpdaterForm(args, log)
                Application.Run(form)
                Return form.ExitCode
            End Using
        Catch ex As Exception
            ' Outside the form's own net (construction, message loop): last resort.
            log.Write("Program.Main", ex)
            MessageBox.Show("Actualizarea K-BOT nu s-a putut face." & Environment.NewLine & ex.Message &
                            Environment.NewLine & "Detalii în: " & log.Path,
                            "Actualizare K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return 1
        End Try
    End Function

    ''' <summary>
    ''' Starts a second copy of this exe with the same arguments plus <c>--elevated</c>, through
    ''' the UAC prompt. The caller exits right after. A declined prompt is an error the operator
    ''' reads, not a silent no-op.
    ''' </summary>
    Friend Sub RelaunchElevated(args As UpdaterArgs)
        Dim self As String = Environment.ProcessPath
        If String.IsNullOrEmpty(self) Then Throw New InvalidOperationException("Nu pot determina calea propriului executabil.")
        Dim psi As New ProcessStartInfo(self) With {
            .UseShellExecute = True,
            .Verb = "runas",
            .WorkingDirectory = Path.GetDirectoryName(self)
        }
        For Each a As String In args.ToArgumentList(markElevated:=True)
            psi.ArgumentList.Add(a)
        Next
        Try
            Process.Start(psi)
        Catch ex As Win32Exception When ex.NativeErrorCode = 1223
            Throw New OperationCanceledException("Cererea de drepturi de administrator a fost refuzată; actualizarea nu s-a făcut.", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Starts the application after the update. When this process is elevated (the UAC
    ''' path), starting the app directly would run it as administrator for the whole session;
    ''' going through the shell's own <c>explorer.exe</c> launches it as the logged-on user.
    ''' </summary>
    Friend Sub StartApplication(exePath As String)
        If Not File.Exists(exePath) Then Throw New FileNotFoundException("Executabilul aplicației lipsește după actualizare.", exePath)
        Dim workDir As String = Path.GetDirectoryName(exePath)
        If Environment.IsPrivilegedProcess Then
            Dim explorer As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe")
            Dim psi As New ProcessStartInfo(explorer) With {.UseShellExecute = False, .WorkingDirectory = workDir}
            psi.ArgumentList.Add(exePath)
            Process.Start(psi)
        Else
            Process.Start(New ProcessStartInfo(exePath) With {.UseShellExecute = True, .WorkingDirectory = workDir})
        End If
    End Sub
End Module
