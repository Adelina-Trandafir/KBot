Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Entry point of VRC.Migrator. Same global safety nets as KBot.Migrator / ADE.Migrator: no exception is lost, all of them
''' reach <c>&lt;AppDir&gt;\Logs\harness_errors.log</c>. Besides the window there is a headless read-only mode:
''' <c>VRC.Migrator.exe --profile report.md file1.accdb file2.accdb ...</c> writes the profile report and exits.
''' </summary>
Friend Module Program

    <STAThread>
    Friend Function Main(k_args As String()) As Integer
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, AddressOf OnThreadException
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnUnhandledException
        AddHandler TaskScheduler.UnobservedTaskException, AddressOf OnUnobservedTaskException

        Dim k_exitCode As Integer = 0
        Try
            StartErrorLog()
            If k_args.Length >= 3 AndAlso String.Equals(k_args(0), "--profile", StringComparison.OrdinalIgnoreCase) Then
                k_exitCode = RunHeadless(k_args)
                Return k_exitCode
            End If
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            ThemeStore.LoadScaling()
            If AppScaling.DpiUnaware Then
                Application.SetHighDpiMode(HighDpiMode.DpiUnaware)
            Else
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)
            End If
            ThemeManager.Initialize()
            Application.Run(New VrcMigratorForm())
        Catch ex As Exception
            GlobalErrorLog.Write("Program.Main", ex)
            ShowFatal(ex)
            k_exitCode = 1
        Finally
            Try
                OleDbConnection.ReleaseObjectPool()
            Catch poolEx As Exception
                GlobalErrorLog.Write("Program.ReleaseObjectPool", poolEx)
            End Try
            ' The Office Access driver crashes while the process unloads it (see KBot.Migrator Program, slice 0104-02).
            FastExit.TerminateIfOfficeDriverLoaded()
        End Try
        Return k_exitCode
    End Function

    ''' <summary>Read-only profile of the given files into a Markdown report; no window.</summary>
    Private Function RunHeadless(k_args As String()) As Integer
        Dim k_out As String = k_args(1)
        Dim k_files = k_args.Skip(2).ToList()
        Dim k_profiles = k_files.Select(Function(k_f) VrcProfiler.Profile(k_f, Nothing)).ToList()
        File.WriteAllText(k_out, VrcReport.ToMarkdown(k_profiles), New UTF8Encoding(False))
        Return If(k_profiles.Any(Function(k_p) k_p.ReadError.Length > 0), 2, 0)
    End Function

    ''' <summary>Archive the previous error file and keep the shared logger's current path for this run.</summary>
    Private Sub StartErrorLog()
        Try
            LogPaths.EnsureLogsDirectory()
            Dim k_logPath = LogPaths.Combine(GlobalErrorLog.FileNameOnly)
            If File.Exists(k_logPath) Then
                Dim k_archive = Path.Combine(Path.GetDirectoryName(k_logPath),
                    $"harness_errors_VRC_{DateTime.Now:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}.log")
                File.Move(k_logPath, k_archive)
            End If
            Using k_stream As New FileStream(k_logPath, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite)
                ' The common logger appends only errors from this launch to the new empty file.
            End Using
        Catch ex As Exception
            Throw New IOException("Jurnalul nou de erori nu a putut fi pregătit. Jurnalul anterior nu este șters.", ex)
        End Try
    End Sub

    Private Sub OnThreadException(sender As Object, e As Threading.ThreadExceptionEventArgs)
        GlobalErrorLog.Write("Application.ThreadException", e.Exception)
        ShowFatal(e.Exception)
    End Sub

    Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim k_ex As Exception = TryCast(e.ExceptionObject, Exception)
        GlobalErrorLog.Write("AppDomain.UnhandledException", k_ex)
        ShowFatal(k_ex)
    End Sub

    Private Sub OnUnobservedTaskException(sender As Object, e As UnobservedTaskExceptionEventArgs)
        GlobalErrorLog.Write("TaskScheduler.UnobservedTaskException", e.Exception)
        e.SetObserved()
    End Sub

    Private Sub ShowFatal(k_ex As Exception)
        Try
            KBotMessage.Show(
                "A apărut o eroare neașteptată." & Environment.NewLine & Environment.NewLine &
                If(k_ex IsNot Nothing, k_ex.Message, "Eroare necunoscută.") & Environment.NewLine & Environment.NewLine &
                "Detaliul complet e în " & LogPaths.Combine(GlobalErrorLog.FileNameOnly) & ".",
                "VRC.Migrator", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch showEx As Exception
            GlobalErrorLog.Write("Program.ShowFatal", showEx)
        End Try
    End Sub

End Module
