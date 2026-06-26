Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.Playwright

''' <summary>
''' Verifică la pornire dacă browserul Chromium folosit de Playwright este instalat
''' și, dacă lipsește, îl instalează rulând "playwright.ps1 install chromium" din
''' folderul aplicației.
'''
''' Strategie:
'''   1. Probă rapidă pe filesystem (cache user %LocalAppData%\ms-playwright).
'''   2. Dacă lipsește, confirmare cu probă reală headless (CreateAsync + LaunchAsync).
'''   3. Instalare via powershell.exe -ExecutionPolicy Bypass -File playwright.ps1 install chromium.
'''      - Elevare (runas) DOAR dacă folderul țintă nu e scriibil de userul curent.
'''   4. Re-verificare pe filesystem.
'''
''' Notă: în configurația curentă (fără PLAYWRIGHT_BROWSERS_PATH, app în folder scriibil)
'''       ramura de elevare nu se declanșează — browserele merg în cache-ul userului,
'''       care e mereu scriibil fără admin. Ramura runas rămâne ca fallback corect
'''       pentru cazul în care folderul țintă chiar nu e scriibil.
''' </summary>
Module PlaywrightBootstrap

    ' =========================================================================
    '  PUNCT DE INTRARE (sincron, apelat din Program.Main înainte de Application.Run)
    ' =========================================================================

    ''' <summary>
    ''' Asigură prezența Chromium. Nu aruncă niciodată — în caz de eșec doar
    ''' loghează (și informează prin MsgBox dacă interactive=True), lăsând
    ''' handler-ul lazy din LaunchAndPositionBrowserAsync ca plasă de siguranță.
    ''' </summary>
    ''' <param name="interactive">
    ''' True = mod manual (STANDALONE): cere confirmare + afișează rezultat prin MsgBox.
    ''' False = mod IPC/batch (lansat din Access): instalează automat, fără dialoguri.
    ''' </param>
    Public Sub EnsureChromiumInstalled(interactive As Boolean)
        Dim bootEx As Exception = Nothing
        Try
            EnsureChromiumInstalledAsync(interactive).GetAwaiter().GetResult()
        Catch ex As Exception
            bootEx = ex
        End Try

        If bootEx IsNot Nothing Then
            ' Niciodată nu blocăm pornirea aplicației din cauza bootstrap-ului.
            LogLine("[Bootstrap] Eroare neașteptată: " & bootEx.Message)
            LogLine(bootEx.StackTrace)
        End If
    End Sub

    ' =========================================================================
    '  FLUX PRINCIPAL
    ' =========================================================================

    Private Async Function EnsureChromiumInstalledAsync(interactive As Boolean) As Task
        ' --- 1. PROBĂ RAPIDĂ PE FILESYSTEM ---
        If IsChromiumPresentOnDisk() Then
            LogLine("[Bootstrap] Chromium prezent pe disc — OK.")
            Return
        End If

        ' --- 2. CONFIRMARE CU PROBĂ REALĂ (headless) ---
        LogLine("[Bootstrap] Chromium negăsit pe disc — confirm cu probă reală...")
        Dim reallyPresent As Boolean = Await RealProbeAsync()
        If reallyPresent Then
            LogLine("[Bootstrap] Proba reală a reușit — Chromium funcțional. OK.")
            Return
        End If

        LogLine("[Bootstrap] Confirmat: Chromium NU este instalat.")

        ' --- 3. LOCALIZARE playwright.ps1 ---
        Dim baseDir As String = AppContext.BaseDirectory
        Dim ps1Path As String = Path.Combine(baseDir, "playwright.ps1")

        If Not File.Exists(ps1Path) Then
            LogLine("[Bootstrap] LIPSĂ scriptul de instalare: " & ps1Path)
            If interactive Then
                MessageBox.Show(
                    "Playwright (Chromium) nu este instalat și nu găsesc scriptul de instalare:" & vbCrLf &
                    ps1Path & vbCrLf & vbCrLf &
                    "Instalează manual: deschide PowerShell în folderul aplicației și rulează:" & vbCrLf &
                    "    .\playwright.ps1 install chromium",
                    "ForexeBot — Playwright lipsă",
                    MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return
        End If

        ' --- 4. CONFIRMARE UTILIZATOR (doar mod manual) ---
        If interactive Then
            Dim ans As DialogResult = MessageBox.Show(
                "Componentele Playwright (Chromium) nu sunt instalate pe acest PC." & vbCrLf & vbCrLf &
                "Aplicația le poate instala acum (descărcare ~150 MB, poate dura câteva minute)." & vbCrLf & vbCrLf &
                "Pornesc instalarea?",
                "ForexeBot — Instalare Playwright",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question)

            If ans <> DialogResult.Yes Then
                LogLine("[Bootstrap] Utilizatorul a refuzat instalarea.")
                MessageBox.Show(
                    "Instalarea a fost anulată." & vbCrLf &
                    "Aplicația nu va putea deschide browserul până când Playwright nu este instalat.",
                    "ForexeBot", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
        End If

        ' --- 5. DECIDEM DACĂ E NEVOIE DE ELEVARE ---
        Dim writable As Boolean = IsTargetWritable()
        Dim needsElevation As Boolean = Not writable
        LogLine($"[Bootstrap] Folder țintă scriibil: {writable}. Elevare necesară: {needsElevation}.")

        ' --- 6. RULĂM INSTALAREA ---
        Dim exitCode As Integer = -1
        Dim runEx As Exception = Nothing
        Try
            If needsElevation Then
                LogLine("[Bootstrap] Pornesc instalarea cu elevare (runas)...")
                exitCode = RunInstallElevated(baseDir, ps1Path)
            Else
                LogLine("[Bootstrap] Pornesc instalarea ca utilizator curent...")
                exitCode = RunInstallNormal(baseDir, ps1Path)
            End If
        Catch ex As Exception
            runEx = ex
        End Try

        If runEx IsNot Nothing Then
            LogLine("[Bootstrap] Eroare la lansarea instalării: " & runEx.Message)
            If interactive Then
                MessageBox.Show(
                    "Eroare la pornirea instalării Playwright:" & vbCrLf & runEx.Message,
                    "ForexeBot", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return
        End If

        LogLine($"[Bootstrap] Instalare terminată. ExitCode={exitCode}")

        ' --- 7. RE-VERIFICARE ---
        If IsChromiumPresentOnDisk() Then
            LogLine("[Bootstrap] Verificare OK — Chromium instalat.")
            If interactive Then
                MessageBox.Show(
                    "Playwright (Chromium) a fost instalat cu succes.",
                    "ForexeBot", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Else
            LogLine("[Bootstrap] Verificare EȘUATĂ — Chromium tot lipsește după instalare.")
            If interactive Then
                MessageBox.Show(
                    $"Instalarea s-a încheiat (ExitCode={exitCode}) dar Chromium tot nu este detectat." & vbCrLf &
                    "Verifică log-ul: " & LogPath(),
                    "ForexeBot", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End If
    End Function

    ' =========================================================================
    '  DETECȚIE
    ' =========================================================================

    ''' <summary>
    ''' Caută chrome.exe în cache-ul user implicit %LocalAppData%\ms-playwright\chromium-*\chrome-win\.
    ''' (Fără PLAYWRIGHT_BROWSERS_PATH setat, asta este locația deterministă.)
    ''' Atenție: nu validează revizia exactă — un build vechi tot ar trece de probă;
    ''' confirmarea fină se face de proba reală (apelată doar când aici nu găsim nimic).
    ''' </summary>
    Private Function IsChromiumPresentOnDisk() As Boolean
        Dim findEx As Exception = Nothing
        Try
            Dim cacheRoot As String = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ms-playwright")

            If Not Directory.Exists(cacheRoot) Then Return False

            ' "chromium-*" prinde build-ul complet (headed), NU "chromium_headless_shell-*".
            For Each dir As String In Directory.GetDirectories(cacheRoot, "chromium-*")
                Dim chromeExe As String = Path.Combine(dir, "chrome-win", "chrome.exe")
                If File.Exists(chromeExe) Then Return True
            Next

            Return False
        Catch ex As Exception
            findEx = ex
        End Try

        ' Orice eroare la citire o tratăm ca "nedetectat" (forțează proba reală).
        LogLine("[Bootstrap] Eroare la proba pe disc: " & findEx.Message)
        Return False
    End Function

    ''' <summary>
    ''' Probă reală: pornește Chromium headless prin Playwright și îl închide imediat.
    ''' Returnează False DOAR pentru semnătura clară "executabil lipsă"; pentru orice
    ''' altă eroare returnează True (nu vrem să declanșăm o descărcare de 150 MB
    ''' pentru o eroare nelegată de lipsa browserului).
    ''' </summary>
    Private Async Function RealProbeAsync() As Task(Of Boolean)
        Dim pw As IPlaywright = Nothing
        Dim br As IBrowser = Nothing
        Dim probeEx As Exception = Nothing

        Try
            pw = Await Playwright.CreateAsync()
            br = Await pw.Chromium.LaunchAsync(New BrowserTypeLaunchOptions With {
                .Headless = True,
                .Channel = "chromium"
            })
        Catch ex As Exception
            probeEx = ex
        End Try

        ' Curățare în flux normal (fără Await în Catch).
        If br IsNot Nothing Then
            Dim closeEx As Exception = Nothing
            Try
                Await br.CloseAsync()
            Catch ex As Exception
                closeEx = ex
            End Try
            If closeEx IsNot Nothing Then LogLine("[Bootstrap] Avertisment închidere probă: " & closeEx.Message)
        End If
        If pw IsNot Nothing Then pw.Dispose()

        If probeEx Is Nothing Then Return True ' s-a lansat → instalat

        Dim msg As String = probeEx.Message
        If msg.Contains("Executable doesn't exist") OrElse msg.Contains("playwright install") Then
            Return False ' lipsă confirmată
        End If

        LogLine("[Bootstrap] Probă reală — eroare neclasificată: " & msg)
        Return True
    End Function

    ' =========================================================================
    '  SCRIIBILITATE ȚINTĂ (decide elevarea)
    ' =========================================================================

    Private Function IsTargetWritable() As Boolean
        Dim writeEx As Exception = Nothing
        Try
            Dim target As String = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ms-playwright")

            If Not Directory.Exists(target) Then Directory.CreateDirectory(target)

            Dim testFile As String = Path.Combine(target, ".fb_write_test_" & Guid.NewGuid().ToString("N"))
            File.WriteAllText(testFile, "x")
            File.Delete(testFile)
            Return True
        Catch ex As Exception
            writeEx = ex
        End Try

        LogLine("[Bootstrap] Folder țintă NEscriibil: " & writeEx.Message)
        Return False
    End Function

    ' =========================================================================
    '  INSTALARE
    ' =========================================================================

    ''' <summary>Instalare ca utilizator curent — captează stdout/stderr în log.</summary>
    Private Function RunInstallNormal(baseDir As String, ps1Path As String) As Integer
        Dim psi As New ProcessStartInfo With {
            .FileName = "powershell.exe",
            .Arguments = $"-NoProfile -ExecutionPolicy Bypass -File ""{ps1Path}"" install chromium",
            .WorkingDirectory = baseDir,
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .CreateNoWindow = True
        }

        Dim sb As New StringBuilder()
        Using p As New Process()
            p.StartInfo = psi
            AddHandler p.OutputDataReceived, Sub(s, e) If e.Data IsNot Nothing Then sb.AppendLine(e.Data)
            AddHandler p.ErrorDataReceived, Sub(s, e) If e.Data IsNot Nothing Then sb.AppendLine("ERR: " & e.Data)

            p.Start()
            p.BeginOutputReadLine()
            p.BeginErrorReadLine()
            p.WaitForExit()

            LogLine("[Install/output] " & vbCrLf & sb.ToString())
            Return p.ExitCode
        End Using
    End Function

    ''' <summary>
    ''' Instalare cu elevare (runas) — același tipar ca ConfigureAutoSelectCertificatePolicy.
    ''' Nu se poate captura output-ul când UseShellExecute=True; afișăm fereastra ca
    ''' utilizatorul să vadă progresul descărcării.
    ''' </summary>
    Private Function RunInstallElevated(baseDir As String, ps1Path As String) As Integer
        Dim psi As New ProcessStartInfo With {
            .FileName = "powershell.exe",
            .Arguments = $"-NoProfile -ExecutionPolicy Bypass -File ""{ps1Path}"" install chromium",
            .WorkingDirectory = baseDir,
            .UseShellExecute = True,
            .Verb = "runas",
            .WindowStyle = ProcessWindowStyle.Normal
        }

        Using p As Process = Process.Start(psi)
            If p Is Nothing Then
                LogLine("[Bootstrap] Process.Start a returnat Nothing la instalarea elevată.")
                Return -1
            End If
            p.WaitForExit()
            Return p.ExitCode
        End Using
    End Function

    ' =========================================================================
    '  LOGGING (fișier lângă .exe, fallback Temp)
    ' =========================================================================

    Private Function LogPath() As String
        Dim pathEx As Exception = Nothing
        Try
            Return Path.Combine(AppContext.BaseDirectory, "ForexeBot_PlaywrightInstall.log")
        Catch ex As Exception
            pathEx = ex
        End Try
        ' Fallback dacă nu putem compune calea lângă exe.
        Return Path.Combine(Path.GetTempPath(), "ForexeBot_PlaywrightInstall.log")
    End Function

    Private Sub LogLine(text As String)
        Dim writeEx As Exception = Nothing
        Dim line As String = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}{Environment.NewLine}"
        Try
            File.AppendAllText(LogPath(), line)
        Catch ex As Exception
            writeEx = ex
        End Try

        If writeEx IsNot Nothing Then
            ' Ultim resort: Temp. Dacă nici asta nu merge, renunțăm tăcut (e doar log).
            Try
                File.AppendAllText(Path.Combine(Path.GetTempPath(), "ForexeBot_PlaywrightInstall.log"), line)
            Catch
            End Try
        End If
    End Sub

End Module