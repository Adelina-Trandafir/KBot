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
Imports KBot.Theming
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

            ' Tema: încarcă schema persistată (implicit Classic) ÎNAINTE de primul formular,
            ' apoi conectează subsistemele Forexe (RichTextBoxLogger) la ThemeManager.ThemeChanged.
            ThemeManager.Initialize()
            KBotTheme.WireSubsystems()

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
        ' Citește token-ul curent din sesiune — cel post-reauth, dacă a existat un re-login.
        If session.IsAuthenticated AndAlso Not String.IsNullOrEmpty(session.Token) Then
            Try
                Using cts As New CancellationTokenSource(TimeSpan.FromSeconds(5))
                    authApi.LogoutAsync(session.Token, cts.Token).GetAwaiter().GetResult()
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
        ' Context de sesiune (înlocuiește glob*): singleton gol, populat de login
        ' (LoginForm -> Populate). ApiClient citește token-ul din aceeași instanță.
        services.AddSingleton(Of SessionContext)()

        ' Adresa serverului e o constantă în ApiOptions (hostname public, ne-secret).
        ' Nimic despre adresă nu se mai citește din mediu / config de pe PC-ul clientului.
        services.AddSingleton(New ApiOptions())

        ' HttpClient tipat: BaseAddress + Timeout din ApiOptions (token-ul bearer merge
        ' per-request din ApiClient/AuthApi). Gardă https: refuzăm orice adresă ne-https,
        ' ca un token să nu plece niciodată necriptat. Prinde doar o editare greșită
        ' viitoare a constantei — aruncă la pornire, prins de plasele globale -> ShowFatal.
        services.AddSingleton(Of HttpClient)(
            Function(sp)
                Dim opt As ApiOptions = sp.GetRequiredService(Of ApiOptions)()
                If Not opt.BaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidOperationException(
                        "Adresa serverului trebuie să folosească https. Valoare: " & opt.BaseUrl)
                End If
                Dim client As New HttpClient() With {.BaseAddress = New Uri(opt.BaseUrl)}
                client.Timeout = TimeSpan.FromSeconds(opt.TimeoutSeconds)
                Return client
            End Function)
        services.AddSingleton(Of IApiClient, ApiClient)()

        ' Client de login (felia login). Fără stare — refolosește HttpClient + ApiOptions.
        services.AddSingleton(Of IAuthApi, AuthApi)()

        ' Stocare temporară (SQLite in-memory).
        services.AddSingleton(Of ITempStore, SqliteTempStore)()

        ' Procesorul Excel pentru workflow-urile FOREXE: un mic pod către ApiClient, ca
        ' tot HTTP-ul să stea într-un singur loc. FOREXE nu depinde de KBot.Api — primește
        ' doar acest Func (ExcelJob din KBot.Common, văzut de ambele straturi).
        services.AddSingleton(Of Func(Of ExcelJob, CancellationToken, Task(Of String)))(
            Function(sp)
                Return Function(job As ExcelJob, ct As CancellationToken)
                           Return sp.GetRequiredService(Of IApiClient)().ProcessExcelAsync(job, ct)
                       End Function
            End Function)

        ' Executor FOREXE (in-process).
        services.AddSingleton(Of IForexeRunner, ForexeRunner)()

        ' Forms.
        services.AddTransient(Of MainForm)()
        services.AddTransient(Of LoginForm)()

        ' Fabrică de LoginForm pentru re-login la 401 (MainForm.WithReauth) — fără
        ' service-locator în MainForm.
        services.AddSingleton(Of Func(Of LoginForm))(
            Function(sp) Function() sp.GetRequiredService(Of LoginForm)())

#If DEBUG Then
        ' Banc de probă (Dev Harness) — doar pe Debug.
        services.AddTransient(Of KBot.DevHarness.DevHarnessForm)()
#End If
    End Sub

End Module
