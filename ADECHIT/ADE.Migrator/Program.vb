Imports System.Data.OleDb
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Entry point of ADE.Migrator. Same global safety nets as KBot.Migrator: no exception is lost, all of them reach
''' <c>&lt;AppDir&gt;\Logs\harness_errors.log</c>.
''' </summary>
Friend Module Program

    <STAThread>
    Friend Sub Main()
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, AddressOf OnThreadException
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnUnhandledException
        AddHandler TaskScheduler.UnobservedTaskException, AddressOf OnUnobservedTaskException

        Try
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            ThemeStore.LoadScaling()
            If AppScaling.DpiUnaware Then
                Application.SetHighDpiMode(HighDpiMode.DpiUnaware)
            Else
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)
            End If
            ThemeManager.Initialize()
            Application.Run(New AdeMigratorForm())
        Catch ex As Exception
            GlobalErrorLog.Write("Program.Main", ex)
            ShowFatal(ex)
        Finally
            Try
                OleDbConnection.ReleaseObjectPool()
            Catch poolEx As Exception
                GlobalErrorLog.Write("Program.ReleaseObjectPool", poolEx)
            End Try
            ' The Office Access driver crashes while the process unloads it (see KBot.Migrator Program, slice 0104-02).
            FastExit.TerminateIfOfficeDriverLoaded()
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
                "Migrare ADE", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch showEx As Exception
            GlobalErrorLog.Write("Program.ShowFatal", showEx)
        End Try
    End Sub

End Module
