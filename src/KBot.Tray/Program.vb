Option Strict On
Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Entry point of KBot.Tray.exe, the signer edition (slice TRAY-02). One instance per Windows user. It runs from its own folder and
''' replaces its own files when a newer tray is published (<see cref="TrayUpdater"/>).
''' </summary>
Friend Module Program

    Private Const MutexName As String = "Local\KBOT_Tray_5C1E7A90"

    <STAThread>
    Friend Sub Main(k_args As String())
        ' Started by the previous tray after an update: that one still owns the lock below until it has ended.
        TrayUpdater.WaitForPreviousInstance(k_args)

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, AddressOf OnThreadException
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnUnhandledException
        AddHandler TaskScheduler.UnobservedTaskException, AddressOf OnUnobservedTaskException

        Dim k_mutex As Mutex = Nothing
        Try
            Dim k_created As Boolean
            k_mutex = New Mutex(True, MutexName, k_created)
            If Not k_created Then
                ' A tray is already running: nothing to do (a second icon would only confuse).
                k_mutex.Dispose()
                k_mutex = Nothing
                Return
            End If

            Dim k_appDir As String = Path.GetDirectoryName(Environment.ProcessPath)
            TrayLog.Write("start", $"KBot.Tray {Diagnostics.FileVersionInfo.GetVersionInfo(Environment.ProcessPath).FileVersion} from {k_appDir}; developer's build folder={Autostart.IsDevelopmentFolder(k_appDir)}")

            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)
            ThemeManager.Initialize()
            ' The message boxes of the signing windows (KBotMessage.Show) are the K-BOT window, as in K-BOT.
            KBot.Controls.KBotMessageBox.Install()

            Using k_context As New TrayContext(k_appDir)
                Application.Run(k_context)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("Tray.Main", ex)
        Finally
            If k_mutex IsNot Nothing Then
                Try
                    k_mutex.ReleaseMutex()
                Catch ex As ApplicationException
                    ' Not owned by this thread any more: the process is ending anyway.
                End Try
                k_mutex.Dispose()
            End If
        End Try
    End Sub

    Private Sub OnThreadException(k_sender As Object, e As ThreadExceptionEventArgs)
        GlobalErrorLog.Write("Tray.ThreadException", e.Exception)
    End Sub

    Private Sub OnUnhandledException(k_sender As Object, e As UnhandledExceptionEventArgs)
        GlobalErrorLog.Write("Tray.UnhandledException", TryCast(e.ExceptionObject, Exception))
    End Sub

    Private Sub OnUnobservedTaskException(k_sender As Object, e As UnobservedTaskExceptionEventArgs)
        GlobalErrorLog.Write("Tray.UnobservedTaskException", e.Exception)
        e.SetObserved()
    End Sub
End Module
