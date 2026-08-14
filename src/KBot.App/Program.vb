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
            ' PerMonitorV2 (0025-05), nu SystemAware. SystemAware citește DPI-ul monitorului
            ' PRINCIPAL o singură dată, la pornire, și nu-l mai actualizează: mutată pe un monitor
            ' cu altă scalare, fereastra e ÎNTINSĂ ca bitmap de Windows — exact aspectul „textul e
            ' mai gros și nu mai încape". PerMonitorV2 redimensionează și re-scalează real la
            ' fiecare schimbare de DPI.
            '
            ' Controalele K-BOT desenate de noi sunt pregătite: toate trec prin
            ' ThemeShapes.ScaleDpi(Me, …), care citește DeviceDpi la fiecare pictare, deci urmăresc
            ' DPI-ul nou fără cod în plus. NEVERIFICAT PE ECRAN: AdvancedTreeControl (netematizat,
            ' cu metrici proprii) și ferestrele fără chenar din KBotShellForm (WM_NCHITTEST /
            ' WM_GETMINMAXINFO) nu au fost privite la o schimbare de monitor.
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)

            ' Tema: încarcă schema persistată (implicit Classic) ÎNAINTE de primul formular,
            ' apoi conectează subsistemele Forexe (RichTextBoxLogger) la ThemeManager.ThemeChanged.
            ThemeManager.Initialize()
            KBotTheme.WireSubsystems()

            Dim services As New ServiceCollection()
            ConfigureServices(services)

            Using provider As ServiceProvider = services.BuildServiceProvider()
#If DEBUG Then
                ' Pe Debug, dezvoltatorul alege fereastra de pornire dintr-o listă tematizată
                ' (StartupLauncherForm). Apare DOAR în build-ul Debug; Release-ul nu are nicio
                ' alegere de făcut.
                RunLauncher(provider)
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

#If DEBUG Then
    ''' <summary>
    ''' Fereastra de pornire (Debug) și dispecerul ei. Launcher-ul e MODAL și se închide COMPLET
    ''' înainte să pornească ceva — bucla de mesaje a ferestrei alese rulează abia după, exact ca
    ''' la LoginForm.
    '''
    ''' O cheie necunoscută ARUNCĂ: dacă cineva adaugă o pornire în <c>StartupLauncherForm.PORNIRI</c>
    ''' și uită dispecerul, trebuie să afle imediat, nu să vadă un proces care iese tăcut.
    ''' </summary>
    Private Sub RunLauncher(provider As ServiceProvider)
        Dim alegere As String
        Using launcher As New StartupLauncherForm()
            If launcher.ShowDialog() <> DialogResult.OK Then Return   ' renunțare -> ieșim curat
            alegere = launcher.Alegere
        End Using

        Select Case alegere
            Case StartupLauncherForm.KEY_APLICATIE
                RunShellWithLogin(provider)
            Case StartupLauncherForm.KEY_BANC
                RunHarness(provider)
            Case StartupLauncherForm.KEY_JURNALE
                ' Jurnalele, singure: fără autentificare (citesc fișiere locale) și fără shell.
                ' Grupul de jurnale de server rămâne acolo, dar fără sesiune apelul lui va pica —
                ' și o spune în notificarea proprie, ca orice altă cădere de server.
                Application.Run(provider.GetRequiredService(Of LogViewerForm)())
            Case Else
                Throw New ArgumentException("Pornire necunoscută în launcher: «" & If(alegere, "<nimic>") & "».")
        End Select
    End Sub

    ' Calea "Banc de probă" (Debug): fereastra-rădăcină a buclei de mesaje este harness-ul.
    ' MainForm-ul se deschide din harness la cerere (un singur exemplar, re-deschis dacă a
    ' fost închis). La ÎNCHIDEREA harness-ului (pagina de teste) închidem și MainForm-ul, ca
    ' Application.Run să se termine curat și procesul să revină în VB.NET — fără fereastră
    ' orfană sau dispose în ordine greșită a serviciilor DI.
    Private Sub RunHarness(provider As ServiceProvider)
        Try
            Dim harness As KBot.DevHarness.DevHarnessForm = provider.GetRequiredService(Of KBot.DevHarness.DevHarnessForm)()

            Dim harnessMain As MainForm = Nothing
            harness.OpenMainFormAction =
                Sub()
                    If harnessMain Is Nothing OrElse harnessMain.IsDisposed Then
                        harnessMain = provider.GetRequiredService(Of MainForm)()
                        AddHandler harnessMain.FormClosed, Sub() harnessMain = Nothing
                    End If
                    harnessMain.Show()
                    harnessMain.BringToFront()
                End Sub

            ' Vizualizatorul de jurnale, deschis nemodal din butonul «Jurnale» al bancului — o
            ' singură fereastră, re-deschisă dacă a fost închisă, exact ca shell-ul de mai sus.
            Dim harnessLogs As LogViewerForm = Nothing
            harness.OpenLogViewerAction =
                Sub()
                    If harnessLogs Is Nothing OrElse harnessLogs.IsDisposed Then
                        harnessLogs = provider.GetRequiredService(Of LogViewerForm)()
                        AddHandler harnessLogs.FormClosed, Sub() harnessLogs = Nothing
                    End If
                    harnessLogs.Show()
                    harnessLogs.BringToFront()
                End Sub

            AddHandler harness.FormClosed,
                Sub()
                    If harnessMain IsNot Nothing AndAlso Not harnessMain.IsDisposed Then harnessMain.Close()
                    If harnessLogs IsNot Nothing AndAlso Not harnessLogs.IsDisposed Then harnessLogs.Close()
                End Sub

            Application.Run(harness)   ' se termină când harness-ul (pagina de teste) se închide
        Catch ex As Exception
            GlobalErrorLog.Write("Program.RunHarness", ex)
            Throw
        End Try
    End Sub
#End If

    ' Poarta de login -> shell (MainForm) -> logout best-effort la închidere.
    ' Folosită de calea Release și de opțiunea "Login" din dialogul de start Debug.
    ' LoginForm e MODAL (ShowDialog): se închide COMPLET înainte ca MainForm să se deschidă
    ' (Application.Run(MainForm) rulează abia după). La închiderea MainForm-ului bucla se
    ' termină și procesul revine în VB.NET; dacă login-ul e anulat, ieșim fără shell.
    Private Sub RunShellWithLogin(provider As ServiceProvider)
        Try
            Using login As LoginForm = provider.GetRequiredService(Of LoginForm)()
                If login.ShowDialog() <> DialogResult.OK Then
                    Return   ' anulat -> ieșim fără a lansa shell-ul
                End If
                login.Dispose()   ' nu mai avem nevoie de login, eliberăm resursele
            End Using

            Dim session As SessionContext = provider.GetRequiredService(Of SessionContext)()
            Dim authApi As IAuthApi = provider.GetRequiredService(Of IAuthApi)()

            Application.Run(provider.GetRequiredService(Of MainForm)())

            'trebuie sa aduca in prim plan fereastra main, daca loginul a fost facut cu succes si s-a inchis formularul login

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
        Catch ex As Exception
            GlobalErrorLog.Write("Program.RunShellWithLogin", ex)
            Throw
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

    ' Nu are Try/Catch propriu: ruleaza EXCLUSIV in interiorul Try-ului din Main
    ' (compozitia DI la pornire), deci orice esec e deja prins + logat acolo ->
    ' ShowFatal. Un wrapper aici ar dubla doar logarea.
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
                opt.EnsureHttpsBaseUrl()
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
        ' Vizualizatorul de jurnale (felia 0031-04). Transient: se deschide nemodal din meniul
        ' butonului de opțiuni al shell-ului și modal din bancul de probă — două vieți diferite,
        ' deci două instanțe. Primește IApiClient, deci vede și grupul de jurnale de SERVER.
        services.AddTransient(Of LogViewerForm)(
            Function(sp) New LogViewerForm(sp.GetRequiredService(Of IApiClient)()))

        ' Fabrică de LoginForm pentru re-login la 401 (MainForm.WithReauth) — fără
        ' service-locator în MainForm.
        services.AddSingleton(Of Func(Of LoginForm))(
            Function(sp) Function() sp.GetRequiredService(Of LoginForm)())

#If DEBUG Then
        ' Banc de probă (Dev Harness) — doar pe Debug.
        services.AddTransient(Of KBot.DevHarness.DevHarnessForm)()
        ' Puntea prin care proba vizuală a jurnalelor deschide fereastra din KBot.App fără ca
        ' bancul să refere KBot.App (vezi ILogViewerLauncher).
        services.AddSingleton(Of KBot.DevHarness.ILogViewerLauncher)(
            Function(sp) New LogViewerLauncher(sp))
#End If
    End Sub

End Module
