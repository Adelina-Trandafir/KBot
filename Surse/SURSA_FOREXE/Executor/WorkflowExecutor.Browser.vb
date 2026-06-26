Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography.X509Certificates
Imports System.Windows
Imports Microsoft.Playwright
Imports Microsoft.Win32
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    ' --- PInvoke ---
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As IntPtr, X As Integer, Y As Integer, cx As Integer, cy As Integer, uFlags As UInteger) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function
    <DllImport("user32.dll")>
    Private Shared Function SetForegroundWindow(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function IsWindow(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetSystemMenu(hWnd As IntPtr, bRevert As Boolean) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function EnableMenuItem(hMenu As IntPtr, uIDEnableItem As UInteger, uEnable As UInteger) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetClientRect(hWnd As IntPtr, ByRef lpRect As RECT) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
        Public ReadOnly Property Width As Integer
            Get
                Return Right - Left
            End Get
        End Property
        Public ReadOnly Property Height As Integer
            Get
                Return Bottom - Top
            End Get
        End Property
    End Structure

    Private Const SC_CLOSE As UInteger = &HF060
    Private Const MF_GRAYED As UInteger = &H1

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const SW_RESTORE As Integer = 9
    Private Const SW_MAXIMIZE As Integer = 3
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    ' Variabilă internă de stare
    Private _isBrowserVisible As Boolean = False
    Private ReadOnly _browserWindowTitle As String = "WF_BROWSER_" & Guid.NewGuid().ToString("N")
    Private _browserHwnd As IntPtr = IntPtr.Zero


    ' Proprietate Publică pentru a fi citită din KBOT_STANDALONE
    Public ReadOnly Property IsBrowserVisible As Boolean
        Get
            Return _isBrowserVisible
        End Get
    End Property

    Public Async Function HideBrowserWindowAsync() As Task
        If _page Is Nothing Then Return

        Dim hwnd = Await GetOrRefreshBrowserHwndAsync()
        If hwnd = IntPtr.Zero Then Return

        Dim cdp = Await _page.Context.NewCDPSessionAsync(_page)
        Dim windowId = Await GetChromeWindowIdAsync()

        Dim bounds = CreateBounds(-3000, 0, 1200, 800)

        Dim param = New Dictionary(Of String, Object) From {
            {"windowId", windowId},
            {"bounds", bounds}
        }

        Await cdp.SendAsync("Browser.setWindowBounds", param)

        ' Taskbar OFF
        Dim exStyle = GetWindowLong(hwnd, GWL_EXSTYLE)
        exStyle = (exStyle Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW
        Dim v1 = SetWindowLong(hwnd, GWL_EXSTYLE, exStyle)
        If v1 = 0 Then
            Dim err = Marshal.GetLastWin32Error()
            _logger.LogError($"Eroare SetWindowLong pentru Stealth: {err}")
        End If

        _isBrowserVisible = False
    End Function


    Public Async Function SetBrowserTopMostAsync(enable As Boolean) As Task
        Dim hwnd = Await GetOrRefreshBrowserHwndAsync()
        If hwnd = IntPtr.Zero Then
            _logger.LogWarning("Nu pot seta TopMost: Fereastra browserului nu a fost găsită.")
            Return
        End If

        ' Constante pentru SetWindowPos
        Dim HWND_TOPMOST As New IntPtr(-1)
        Dim HWND_NOTOPMOST As New IntPtr(-2)
        Const SWP_NOMOVE As UInteger = &H2
        Const SWP_NOSIZE As UInteger = &H1
        Const SWP_SHOWWINDOW As UInteger = &H40

        Dim insertAfter As IntPtr = If(enable, HWND_TOPMOST, HWND_NOTOPMOST)
        Dim flags As UInteger = SWP_NOMOVE Or SWP_NOSIZE Or SWP_SHOWWINDOW

        Try
            SetWindowPos(hwnd, insertAfter, 0, 0, 0, 0, flags)
            Dim stare As String = If(enable, "ACTIVA", "DEZACTIVA")
            _logger.LogInfo($"Browser TopMost: {stare}")
        Catch ex As Exception
            _logger.LogError($"Eroare setare Browser TopMost: {ex.Message}")
        End Try
    End Function

    Public Async Function ShowBrowserWindowAsync() As Task
        If _page Is Nothing Then Return

        Dim hwnd = Await GetOrRefreshBrowserHwndAsync()
        If hwnd = IntPtr.Zero Then Return

        ' Taskbar ON
        Dim exStyle = GetWindowLong(hwnd, GWL_EXSTYLE)
        exStyle = (exStyle And Not WS_EX_TOOLWINDOW) Or WS_EX_APPWINDOW
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle)

        Dim cdp = Await _page.Context.NewCDPSessionAsync(_page)
        Dim windowId = Await GetChromeWindowIdAsync()

        Dim bounds = CreateBounds(100, 100, 1400, 900)

        Dim param = New Dictionary(Of String, Object) From {
            {"windowId", windowId},
            {"bounds", bounds}
        }

        Await cdp.SendAsync("Browser.setWindowBounds", param)

        _isBrowserVisible = True
    End Function

    ''' <summary>
    ''' Lansează browserul GOL (about:blank), îl poziționează și așteaptă.
    ''' </summary>
    Public Async Function LaunchAndPositionBrowserAsync() As Task
        If IsBrowserOpen Then
            _logger.LogInfo("Browserul este deja deschis. Refolosesc sesiunea existentă.")
            Return
        End If

        _logger.LogInfo("Lansare browser (Blank)...")

        Try
            ' --- PAS NOU: Configurare Politică Certificate ---
            ConfigureAutoSelectCertificatePolicy()
        Catch ex As Exception
            _logger.LogError($"Eroare configurare politică certificate: {ex.Message}")
            Throw New Exception($"Eroare configurare politică certificate: {ex.Message}")
        End Try

        Try
            _playwright = Await Playwright.CreateAsync()

        Catch ex As Microsoft.Playwright.PlaywrightException
            ' 3. AICI PRINDEM EROAREA CĂ NU E INSTALAT
            If ex.Message.Contains("Executable doesn't exist") OrElse ex.Message.Contains("playwright install") Then

                _logger.LogError("--------------------------------------------------------------")
                _logger.LogError("CRITIC: Componentele Playwright nu sunt instalate pe acest PC!")
                _logger.LogError("-----------------------------------------------------------")

                ' Mesaj clar cu instrucțiuni
                _logger.LogInfo("PASUL 1: Deschide folderul unde este executabilul aplicației.")
                _logger.LogInfo("PASUL 2: Caută folderul '.playwright/node/...' sau rulează scriptul 'playwright.ps1'.")

                ' Le dăm și link-ul oficial dacă vor să citească manual
                _logger.LogInfo("LINK INSTALARE MANUALĂ: https://playwright.dev/dotnet/docs/intro#installing-browsers")

                ' Opțional: Putem deschide automat pagina de ajutor în browserul default al sistemului
                Try
                    Process.Start(New ProcessStartInfo("https://playwright.dev/dotnet/docs/intro#installing-browsers") With {.UseShellExecute = True})
                Catch
                End Try

                ' Oprim execuția pentru că nu putem continua fără browser
                Throw New Exception("Playwright necesită instalare. Verifică log-ul pentru detalii.")
            Else
                ' Dacă e altă eroare, o aruncăm mai departe
                Throw
            End If

        End Try

        Dim browserArgs As New List(Of String)
        ' Dacă e Stealth și NU folosim Snap, îl ascundem off-screen
        If _stealthMode AndAlso Not _useSnapAssist Then
            browserArgs.Add("--window-position=-3000,0")

        End If

        browserArgs.Add("--window-size=1368,768") ' Forțăm o rezoluție de start

        ' Spune browserului să NU încetinească execuția când nu e vizibil
        browserArgs.Add("--disable-background-timer-throttling")
        browserArgs.Add("--disable-backgrounding-occluded-windows")
        browserArgs.Add("--disable-renderer-backgrounding")

        ' ADAUGĂ ACEST ARGUMENT pentru a te asigura că politicile sunt citite corect
        ' În unele medii, Chromium izolat poate ignora politicile dacă nu este marcat ca "managed"
        browserArgs.Add("--enable-logging")
        browserArgs.Add("--log-level=0")

        ' 🔥 fără infobars / chrome UI
        browserArgs.Add("--disable-infobars")
        browserArgs.Add("--disable-features=TranslateUI")

        _browser = Await _playwright.Chromium.LaunchAsync(New BrowserTypeLaunchOptions() With {
            .Headless = False,
            .Args = browserArgs,
            .Channel = "chromium"
        })
        '            .SlowMo = 100,

        _context = Await _browser.NewContextAsync(New BrowserNewContextOptions() With {
            .IgnoreHTTPSErrors = True,
            .AcceptDownloads = True
        })

        '_context = Await _browser.NewContextAsync(New BrowserNewContextOptions() With {
        '    .IgnoreHTTPSErrors = True,
        '    .AcceptDownloads = True,
        '    .ViewportSize = New ViewportSize With {.Width = 1920, .Height = 1080}
        '})

        ' Deschide pagină goală (about:blank)
        _page = Await _context.NewPageAsync()
        Await _page.EvaluateAsync($"document.title = '{_browserWindowTitle}';")

        ' Acum facem Snap pe pagina goală
        If _useSnapAssist Then
            _logger.LogInfo("Aplic Windows Snap pe Browser...")
            'Await Task.Delay(500)
            'Await SnapBrowserRight()
            'Await Task.Delay(500)
        ElseIf _stealthMode Then
            Await ApplyStealthWindowStyle()
        End If

        _logger.LogSuccess("Browser poziționat (Ready).")


        AddHandler _page.Close, Sub()
                                    _logger.LogWarning($"Pagina {_page.Url} a fost închisă de utilizator. Se revine la starea initiala")

                                    Task.Run(Sub()
                                                 RaiseEvent OnBrowserClosed("Browser-ul a fost inchis. Aplicatia se va inchide automat!")
                                             End Sub)
                                End Sub
    End Function

    Private Async Function ApplyStealthWindowStyle() As Task
        Try
            Dim hwnd = Await GetOrRefreshBrowserHwndAsync()
            If hwnd = IntPtr.Zero Then
                _logger.LogWarning("Nu am găsit fereastra pentru Stealth.")
                Return
            End If

            ' Scoatem din Taskbar / Alt-Tab
            Dim exStyle = GetWindowLong(hwnd, GWL_EXSTYLE)
            exStyle = (exStyle Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW
            Dim v = SetWindowLong(hwnd, GWL_EXSTYLE, exStyle)

            If v = 0 Then
                Dim err = Marshal.GetLastWin32Error()
                _logger.LogError($"Eroare SetWindowLong pentru Stealth: {err}")
            End If

            ' SUB NICIO FORMA NU FOLOSESC SW_HIDE, sau nu o sa mai pot selecta
            ' certificatul din lista browserului
            'ShowWindow(hwnd, 0) ' SW_HIDE

            _logger.LogInfo("Stealth aplicat cu succes.")

        Catch ex As Exception
            _logger.LogDebug($"Eroare Stealth: {ex.Message}")
        End Try
    End Function


    Private Async Function GetBrowserHwndAsync() As Task(Of IntPtr)
        Dim hwnd As IntPtr = IntPtr.Zero

        For i = 1 To 20
            Await Task.Delay(200)
            For Each p In Process.GetProcesses()
                Try
                    If p.MainWindowHandle <> IntPtr.Zero AndAlso
                   p.MainWindowTitle = _browserWindowTitle Then
                        Return p.MainWindowHandle
                    End If
                Catch
                End Try
            Next
        Next

        Return IntPtr.Zero
    End Function

    Private Async Function GetChromeWindowIdAsync() As Task(Of Integer)
        Dim cdp = Await _page.Context.NewCDPSessionAsync(_page)

        Dim resultObj = Await cdp.SendAsync("Browser.getWindowForTarget")

        Dim json = CType(resultObj, System.Text.Json.JsonElement)

        Dim windowId = json.GetProperty("windowId").GetInt32()

        Return windowId
    End Function

    Private Function CreateBounds(left As Integer, top As Integer, width As Integer, height As Integer) As Dictionary(Of String, Object)
        Return New Dictionary(Of String, Object) From {
        {"left", left},
        {"top", top},
        {"width", width},
        {"height", height},
        {"windowState", "normal"}
    }
    End Function

    Private Async Function GetOrRefreshBrowserHwndAsync() As Task(Of IntPtr)

        ' 1. Dacă avem deja HWND și e valid → îl folosim
        If _browserHwnd <> IntPtr.Zero AndAlso IsWindow(_browserHwnd) Then
            Return _browserHwnd
        End If

        ' 2. Altfel, căutăm din nou după titlul unic

        Dim hwnd As IntPtr = IntPtr.Zero
        Dim found As Boolean = False
        Dim attempts As Integer = 0

        While Not found AndAlso attempts < 20
            Await Task.Delay(250)
            attempts += 1
            Dim processes = Process.GetProcesses()
            For Each p In processes
                Try
                    If Not String.IsNullOrEmpty(p.MainWindowTitle) AndAlso p.MainWindowTitle.Contains(_browserWindowTitle) Then
                        hwnd = p.MainWindowHandle
                        _browserHwnd = hwnd
                        Return hwnd
                    End If
                Catch
                End Try
            Next
        End While

        ' 3. Nu am găsit nimic
        _browserHwnd = IntPtr.Zero
        Return IntPtr.Zero
    End Function

    Private Sub ConfigureAutoSelectCertificatePolicy()
        If _certificate Is Nothing Then Return

        Dim subjectCN As String = _certificate.GetNameInfo(X509NameType.SimpleName, False)
        Dim issuerCN As String = _certificate.GetNameInfo(X509NameType.SimpleName, True)
        Dim patternUrl As String = "https://forexe.mfinante.gov.ro:443"

        ' JSON-ul politicii
        Dim jsonPolicy As String = $"{{""pattern"":""{patternUrl}"",""filter"":{{""ISSUER"":{{""CN"":""{issuerCN}""}},""SUBJECT"":{{""CN"":""{subjectCN}""}}}}}}"

        Dim keyPath As String = "Software\Policies\Chromium\AutoSelectCertificateForUrls"
        Dim fullKeyPath As String = "HKEY_CURRENT_USER\" & keyPath

        Try
            ' PASUL 1: VERIFICĂM DACĂ EXISTĂ DEJA (Citirea e permisă oricând)
            ' ===============================================================
            Try
                Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(keyPath, False) ' False = ReadOnly
                    If key IsNot Nothing Then
                        Dim existingNames As String() = key.GetValueNames()
                        For Each name As String In existingNames
                            Dim val = key.GetValue(name)?.ToString()
                            If val IsNot Nothing AndAlso val.Contains(subjectCN) AndAlso val.Contains(patternUrl) Then
                                _logger.LogInfo($"[Policy] Regula este deja configurată corect. Nu sunt necesare modificări.")
                                Return ' Ieșim, nu facem nimic
                            End If
                        Next
                    End If
                End Using
            Catch ex As Exception
                Throw New Exception($"[Policy] Eroare la verificarea politicii existente: {ex.Message}")
            End Try

            ' ==============================================================
            ' PASUL 2: ELEVARE
            ' ==============================================================
            ' Creăm un fișier .reg temporar (e cel mai sigur mod de a trece JSON cu ghilimele)
            Dim result = MessageBox.Show(
                                        "Aplicația va solicita permisiuni de Administrator pentru a configura politica de selecție automată a certificatului." & vbCrLf & vbCrLf &
                                        "Acest lucru este NECESAR pentru ca browserul să selecteze automat certificatul tău." & vbCrLf & vbCrLf &
                                        "Te rog să accepți promptul UAC când apare." & vbCrLf & vbCrLf &
                                        "Continui?",
                                        "Permisiuni Administrator Necesare",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Information)

            If result <> MessageBoxResult.Yes Then
                Throw New Exception("Utilizatorul a refuzat elevarea UAC. Nu pot continua fără politica de certificate.")
            End If

            _logger.LogInfo($"[Policy] Subject CN: '{subjectCN}'")
            _logger.LogInfo($"[Policy] Issuer CN: '{issuerCN}'")

            Dim tempRegFile As String = Path.Combine(Path.GetTempPath(), "ForexeBot_CertPolicy.reg")

            ' Formatul fișierului .reg. Atenție: Ghilimelele din JSON trebuie escaped cu \" în fișierul .reg
            Dim escapedJson As String = jsonPolicy.Replace("""", "\""")

            ' Scriem conținutul .reg
            Using sw As New StreamWriter(tempRegFile)
                sw.WriteLine("Windows Registry Editor Version 5.00")
                sw.WriteLine()
                sw.WriteLine($"[{fullKeyPath}]")
                ' "1" este ID-ul. Dacă vrei logică de next ID prin .reg e greu, așa că suprascriem "1" sau folosim un ID mare fix gen "99"
                sw.WriteLine($"""1""=""{escapedJson}""")
            End Using

            Try
                ' Lansăm reg.exe cu drepturi de administrator (runas)
                Dim procInfo As New ProcessStartInfo With {
                    .FileName = "reg.exe",
                    .Arguments = $"import ""{tempRegFile}""",
                    .Verb = "runas", ' Asta declanșează promptul UAC
                    .UseShellExecute = True,
                    .WindowStyle = ProcessWindowStyle.Hidden
                }

                Dim proc = Process.Start(procInfo)
                proc.WaitForExit()

                If proc.ExitCode = 0 Then
                    _logger.LogSuccess("[Policy] reg.exe finalizat cu succes.")

                    ' VERIFICARE: Citim înapoi ce s-a scris
                    Try
                        Using verifyKey As RegistryKey = Registry.CurrentUser.OpenSubKey(fullKeyPath, False)
                            If verifyKey IsNot Nothing Then
                                _logger.LogInfo("[Policy] Verificare post-import:")
                                For Each valueName In verifyKey.GetValueNames()
                                    _logger.LogInfo($"  [{valueName}] = {verifyKey.GetValue(valueName)}")
                                Next
                            End If
                        End Using
                    Catch ex As Exception
                        _logger.LogError($"[Policy] Eroare la verificarea post-import: {ex.Message}")
                    End Try

                Else
                    _logger.LogError($"[Policy] Elevarea a eșuat sau utilizatorul a dat Cancel. ExitCode: {proc.ExitCode}")
                End If

                ' Curățăm fișierul temporar
                Try
                    File.Delete(tempRegFile)
                Catch
                End Try

            Catch ex As Exception
                Throw New Exception($"[Policy] Eroare la elevarea UAC sau aplicarea politicii: {ex.Message}")
            End Try

        Catch ex As Exception
            _logger.LogError($"[Policy] Eroare fatală la configurare: {ex.Message}")
        End Try
    End Sub

    Private Async Function ApplyThrottleAsync() As Task
        If _page Is Nothing Then Return

        Try
            Dim cdp = Await _page.Context.NewCDPSessionAsync(_page)

            If _throttleSettings IsNot Nothing AndAlso _throttleSettings.Enabled Then
                Await cdp.SendAsync("Network.enable")
                Await cdp.SendAsync("Network.emulateNetworkConditions", _throttleSettings.ToCDPParams())
                _logger.LogInfo($"[Throttle] Aplicat: {_throttleSettings.Label} " &
                             $"(DL: {_throttleSettings.DownloadThroughput / 1000:F0} KB/s, " &
                             $"UL: {_throttleSettings.UploadThroughput / 1000:F0} KB/s, " &
                             $"Lat: {_throttleSettings.Latency} ms)")
            Else
                ' Reset - dezactivăm throttle-ul dacă era activ
                Await cdp.SendAsync("Network.enable")
                Await cdp.SendAsync("Network.emulateNetworkConditions", ThrottleSettings.DisableParams())
                _logger.LogInfo("[Throttle] Dezactivat.")
            End If
        Catch ex As Exception
            _logger.LogWarning($"[Throttle] Nu am putut aplica throttle: {ex.Message}")
        End Try
    End Function
End Class