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
                ' Pe Debug, dezvoltatorul alege fereastra de pornire: Autentificare (Login)
                ' sau Banc de probă (Harness). Dialogul apare DOAR în build-ul Debug.
                Dim choice As DialogResult = MessageBox.Show(
                    "Alegeți fereastra de pornire:" & Environment.NewLine & Environment.NewLine &
                    "Da = Autentificare (Login)" & Environment.NewLine &
                    "Nu = Banc de probă (Harness)",
                    "K-BOT — Mod de pornire (Debug)",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question)

                If choice = DialogResult.Yes Then
                    RunShellWithLogin(provider)
                Else
                    ' Fereastra de start e bancul de probă; deschide shell-ul real la cerere.
                    Dim harness As KBot.DevHarness.DevHarnessForm = provider.GetRequiredService(Of KBot.DevHarness.DevHarnessForm)()
                    harness.OpenMainFormAction = Sub() provider.GetRequiredService(Of MainForm)().Show()
                    Application.Run(harness)
                End If
#Else
                ' Pe Release, singura cale este poarta de login înaintea shell-ului.
                RunShellWithLogin(provider)
#End If
            End Using

        Catch ex As Exception
            ' Erori la pornire (DI / construcție formă), în afara message loop-ului.
            GlobalErrorLog.Write("Main.Startup", ex)
            ShowFatal(ex)
        End Try
    End Sub

    ' Poarta de login -> shell (MainForm) -> logout best-effort la închidere.
    ' Folosită de calea Release și de opțiunea "Login" din dialogul de start Debug.
    Private Sub RunShellWithLogin(provider As ServiceProvider)
        Using login As LoginForm = provider.GetRequiredService(Of LoginForm)()
            If login.ShowDialog() <> DialogResult.OK Then
                Return   ' anulat -> ieșim fără a lansa shell-ul
            End If
        End Using

        Dim session As SessionContext = provider.GetRequiredService(Of SessionContext)()
        Dim authApi As IAuthApi = provider.GetRequiredService(Of IAuthApi)()

        Application.Run(provider.GetRequiredService(Of MainForm)())

        ' --- logout best-effort la închidere (sink terminal; NU rearunca la ieșire). ---
        If session.IsAuthenticated AndAlso session.SessionId > 0 Then
            Try
                Using cts As New CancellationTokenSource(TimeSpan.FromSeconds(5))
                    authApi.LogoutAsync(session.SessionId, cts.Token).GetAwaiter().GetResult()
                End Using
            Catch ex As Exception
                GlobalErrorLog.Write("Logout la închidere", ex)
            End Try
        End If
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
        ' Context de sesiune (înlocuiește glob*). DEMO SEED (throwaway): login (felia
        ' 1) nu e implementat, deci seedăm 000_DEMO ca să putem închide round-trip-ul
        ' ListaAngajamente. ȘTERGE SeedDemoSession când login populează SessionContext.
        services.AddSingleton(Of SessionContext)(Function(sp) SeedDemoSession())

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

        ' Client de login (felia login). Fără stare — refolosește HttpClient + ApiOptions.
        services.AddSingleton(Of IAuthApi, AuthApi)()

        ' Stocare temporară (SQLite in-memory).
        services.AddSingleton(Of ITempStore, SqliteTempStore)()

        ' Executor FOREXE (in-process).
        services.AddSingleton(Of IForexeRunner, ForexeRunner)()

        ' Forms.
        services.AddTransient(Of MainForm)()
        services.AddTransient(Of LoginForm)()

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

    ' DEMO SEED — throwaway until login (felia 1) populates SessionContext.
    ' Defaults target 000_DEMO with an unfiltered scrape (empty COD_PROGRAM/SURSA →
    ' the .wfl scrapes everything the connected FOREXE unit shows); An defaults to the
    ' current year. Each field is env-overridable so a live operator can match their
    ' FOREXE unit/year WITHOUT a rebuild. Remove this whole method when login lands.
    Private Function SeedDemoSession() As SessionContext
        Dim anRaw As String = Environment.GetEnvironmentVariable("KBOT_DEMO_AN")
        Dim an As Integer
        If Not Integer.TryParse(anRaw, an) Then an = DateTime.Now.Year

        Return New SessionContext With {
            .DbName = If(Environment.GetEnvironmentVariable("KBOT_DEMO_DBNAME"), "000_DEMO"),
            .An = an,
            .CodProgram = If(Environment.GetEnvironmentVariable("KBOT_DEMO_COD_PROGRAM"), String.Empty),
            .SectorSursa = If(Environment.GetEnvironmentVariable("KBOT_DEMO_SURSA"), String.Empty),
            .CF = "000_DEMO",
            .NumeUnitate = "DEMO",
            .OperatorName = "demo-seed"
        }
    End Function

End Module
