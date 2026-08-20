Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Punctul de intrare al utilitarului de migrare. Aceleași plase globale ca în KBot.App:
''' nicio excepție ne-tratată nu se pierde, totul ajunge în
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

            ' Tema, înaintea primului formular. MigratorForm moștenește KBotThemedForm, deci
            ' nu-și aplică singur nicio culoare.
            ThemeManager.Initialize()

            ' Formularul de pornire (felia 0044): adresa serverului + cheia API, si
            ' proba ca amandoua sunt bune -- lista bazelor de pe MariaDB. Fara ea nu
            ' se merge mai departe: ecranul urmator alege chiar din lista aceea.
            Using conectare As New ConnectForm()
                If conectare.ShowDialog() <> DialogResult.OK Then Return
                ' Clientul trece mai departe si e eliberat de MigratorForm.
                Application.Run(New MigratorForm(conectare.Client, conectare.Baze))
            End Using

        Catch ex As Exception
            GlobalErrorLog.Write("Program.Main", ex)
            ShowFatal(ex)
        End Try
    End Sub

    Private Sub OnThreadException(sender As Object, e As Threading.ThreadExceptionEventArgs)
        GlobalErrorLog.Write("Application.ThreadException", e.Exception)
        ShowFatal(e.Exception)
    End Sub

    Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex As Exception = TryCast(e.ExceptionObject, Exception)
        GlobalErrorLog.Write("AppDomain.UnhandledException", ex)
        ShowFatal(ex)
    End Sub

    Private Sub OnUnobservedTaskException(sender As Object, e As UnobservedTaskExceptionEventArgs)
        GlobalErrorLog.Write("TaskScheduler.UnobservedTaskException", e.Exception)
        e.SetObserved()
    End Sub

    ''' <summary>
    ''' Ultima plasă vizibilă. Nu re-aruncăm de aici: procesul e deja pe drumul spre ieșire,
    ''' iar detaliul complet e deja în jurnal.
    ''' </summary>
    Private Sub ShowFatal(ex As Exception)
        Try
            MessageBox.Show(
                "A apărut o eroare neașteptată." & Environment.NewLine & Environment.NewLine &
                If(ex IsNot Nothing, ex.Message, "Eroare necunoscută.") & Environment.NewLine & Environment.NewLine &
                "Detaliul complet e în " & LogPaths.Combine(GlobalErrorLog.FileNameOnly) & ".",
                "Migrare FX", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch showEx As Exception
            GlobalErrorLog.Write("Program.ShowFatal", showEx)
        End Try
    End Sub

End Module
