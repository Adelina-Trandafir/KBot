Option Strict On
Imports System.Threading

''' <summary>
''' KBotAdmin entry point (slice ME01). Explicit Sub Main: no VB application framework. The scaling is the
''' one K-BOT uses; the theme is fixed to Classic, the message boxes are
''' K-BOT's own, and every unhandled error goes to the same error log (<AppDir>\Logs\harness_errors.log).
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
            ' KBotAdmin is ALWAYS Classic, whatever theme K-BOT has saved; nothing is written to the theme settings.
            ThemeManager.InitializeFixed(BuiltInSchemes.Classic())
            KBotMessageBox.Install()

            Dim k_client As AdminClient = Nothing
            Using k_connect As New ConnectionForm(AdminSettingsStore.Load())
                AppScreen.SetReference(k_connect)
                If k_connect.ShowDialog() <> DialogResult.OK Then Return
                k_client = k_connect.Client
            End Using

            Using k_client
                Using k_main As New MainForm(k_client)
                    AppScreen.SetReference(k_main)
                    Application.Run(k_main)
                End Using
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotAdmin.Main", ex)
            KBotMessage.Show("KBotAdmin nu a putut porni: " & ex.Message, "KBotAdmin", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub OnThreadException(k_sender As Object, e As ThreadExceptionEventArgs)
        GlobalErrorLog.Write("KBotAdmin.ThreadException", e.Exception)
        KBotMessage.Show(e.Exception.Message, "KBotAdmin", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    Private Sub OnUnhandledException(k_sender As Object, e As UnhandledExceptionEventArgs)
        GlobalErrorLog.Write("KBotAdmin.UnhandledException", TryCast(e.ExceptionObject, Exception))
    End Sub

    Private Sub OnUnobservedTaskException(k_sender As Object, e As UnobservedTaskExceptionEventArgs)
        GlobalErrorLog.Write("KBotAdmin.UnobservedTask", e.Exception)
        e.SetObserved()
    End Sub

End Module
