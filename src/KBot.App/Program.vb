Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Forexe
Imports KBot.LocalStore
Imports Microsoft.Extensions.DependencyInjection

Friend Module Program

    <STAThread>
    Friend Sub Main()
        ' Plase globale ÎNAINTE de orice form / message loop: nicio excepție ne-tratată
        ' din TOATĂ soluția (inclusiv codul importat FOREXE/Controls) nu se pierde.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, AddressOf OnThreadException
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnUnhandledException
        AddHandler TaskScheduler.UnobservedTaskException, AddressOf OnUnobservedTaskException

        Try
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.SetHighDpiMode(HighDpiMode.SystemAware)

            Dim services As New ServiceCollection()
            ConfigureServices(services)

            Using provider As ServiceProvider = services.BuildServiceProvider()
#If DEBUG Then
                ' Pe Debug, fereastra de start e bancul de probă; deschide shell-ul real la cerere.
                Dim harness As KBot.DevHarness.DevHarnessForm = provider.GetRequiredService(Of KBot.DevHarness.DevHarnessForm)()
                harness.OpenMainFormAction = Sub() provider.GetRequiredService(Of MainForm)().Show()
                Application.Run(harness)
#Else
                Application.Run(provider.GetRequiredService(Of MainForm)())
#End If
            End Using

        Catch ex As Exception
            ' Erori la pornire (DI / construcție formă), în afara message loop-ului.
            GlobalErrorLog.Write("Main.Startup", ex)
            ShowFatal(ex)
        End Try
    End Sub

    ' ---------- plase globale de erori + dialog fatal ----------
    Private Sub OnThreadException(sender As Object, e As ThreadExceptionEventArgs)
        GlobalErrorLog.Write("Application.ThreadException", e.Exception)
        ShowFatal(e.Exception)
    End Sub

    Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex As Exception = TryCast(e.ExceptionObject, Exception)
        GlobalErrorLog.Write("AppDomain.UnhandledException (terminating=" & e.IsTerminating.ToString() & ")", ex)
        ShowFatal(ex)
    End Sub

    Private Sub OnUnobservedTaskException(sender As Object, e As UnobservedTaskExceptionEventArgs)
        GlobalErrorLog.Write("TaskScheduler.UnobservedTaskException", e.Exception)
        e.SetObserved()   ' marcat observat DUPĂ logare (deja suprafațat)
    End Sub

    Private Sub ShowFatal(ex As Exception)
        Try
            Dim logFile As String = Path.Combine(AppContext.BaseDirectory, "Logs", "harness_errors.log")
            Dim msg As String = "Eroare neașteptată. Detalii complete în:" & Environment.NewLine & logFile &
                                Environment.NewLine & Environment.NewLine & If(ex IsNot Nothing, ex.Message, "<necunoscut>")
            MessageBox.Show(msg, "K-BOT — eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch dialogEx As Exception
            ' SINK TERMINAL: dialogul nu poate fi afișat (ex. fără UI pe firul curent).
            ' Eroarea principală e deja în harness_errors.log; suprafațăm pe Trace, NU rearuncăm.
            Trace.WriteLine("ShowFatal dialog failure: " & dialogEx.Message)
        End Try
    End Sub

    Private Sub ConfigureServices(services As IServiceCollection)
        ' Context de sesiune (înlocuiește glob*), încărcat după login în felia 1.
        services.AddSingleton(Of SessionContext)()

        ' Config (BaseUrl / ApiKey) din mediu; ApiOptions e ce injectăm efectiv.
        Dim cfg As AppConfig = LoadAppConfig()
        services.AddSingleton(cfg)
        services.AddSingleton(New ApiOptions With {.BaseUrl = cfg.ApiBaseUrl, .ApiKey = cfg.ApiKey})

        ' HttpClient tipat: BaseAddress + Timeout din ApiOptions (X-Api-Key merge
        ' per-request din ApiClient). În felia 1 se poate trece pe IHttpClientFactory.
        services.AddSingleton(Of HttpClient)(
            Function(sp)
                Dim opt As ApiOptions = sp.GetRequiredService(Of ApiOptions)()
                Dim client As New HttpClient()
                If Not String.IsNullOrWhiteSpace(opt.BaseUrl) Then
                    client.BaseAddress = New Uri(opt.BaseUrl)
                End If
                client.Timeout = TimeSpan.FromSeconds(opt.TimeoutSeconds)
                Return client
            End Function)
        services.AddSingleton(Of IApiClient, ApiClient)()

        ' Stocare temporară (SQLite in-memory).
        services.AddSingleton(Of ITempStore, SqliteTempStore)()

        ' Executor FOREXE (in-process).
        services.AddSingleton(Of IForexeRunner, ForexeRunner)()

        ' Forms.
        services.AddTransient(Of MainForm)()

#If DEBUG Then
        ' Banc de probă (Dev Harness) — doar pe Debug.
        services.AddTransient(Of KBot.DevHarness.DevHarnessForm)()
#End If
    End Sub

    ' Sursă de config pentru felia curentă: variabile de mediu. Lipsa lor lasă
    ' câmpurile goale — ApiClient hard-fail-uiește la primul apel cu mesaj clar.
    ' (Login-ul / un appsettings dedicat vor înlocui asta ulterior.)
    Private Function LoadAppConfig() As AppConfig
        Return New AppConfig With {
            .ApiBaseUrl = If(Environment.GetEnvironmentVariable("KBOT_API_BASE_URL"), String.Empty),
            .ApiKey = If(Environment.GetEnvironmentVariable("KBOT_API_KEY"), String.Empty)
        }
    End Function

End Module
