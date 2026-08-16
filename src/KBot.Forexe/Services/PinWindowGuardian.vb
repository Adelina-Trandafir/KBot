Option Strict On
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Automation
Imports System.Windows.Forms

''' <summary>
''' Gardianul ferestrei de PIN — PORT EXACT al lui <c>KBOT_IPC.Window.vb</c>
''' (<c>StartUiGuardian</c> / <c>ScanForSecurityWindow</c> / <c>WatchPinWindow</c>).
'''
''' <para>Scanează o dată pe secundă ferestrele de dialog ale sistemului (clasa «#32770») și,
''' când găsește una de autentificare (PIN / smart card / token / securitate), CEDEAZĂ
''' prim-planul: orice fereastră a aplicației care e <c>TopMost</c> îl pierde și e trimisă în
''' spate, ca dialogul de PIN să nu rămână acoperit. Apoi urmărește acel HWND cu
''' <c>IsWindow</c> și, la dispariția lui — sau după 120 de secunde, oricare vine prima —
''' repune exact ferestrele pe care le-a coborât.</para>
'''
''' <para>De ce contează ACUM: de la felia 0034-02 browserul pornește ascuns (stealth), deci
''' dialogul de PIN e singurul lucru pe care operatorul îl vede din tot fluxul de conectare.
''' Dacă rămâne în spatele unei ferestre, conectarea pare pur și simplu blocată.</para>
'''
''' <para>NU introduce PIN-ul și nu atinge certificatul — acelea rămân la
''' <see cref="WindowsSecurityAutomation"/>, care rulează în executor. Gardianul se ocupă
''' DOAR de vizibilitate.</para>
''' </summary>
Public NotInheritable Class PinWindowGuardian

    ' Ritmul de scanare și plasa de siguranță — valorile din KBOT_IPC.
    Private Const SCAN_INTERVAL_MS As Integer = 1000
    Private Const WATCH_POLL_MS As Integer = 200
    Private Const WATCH_TIMEOUT_SECONDS As Integer = 120

    Private ReadOnly _logger As RichTextBoxLogger
    ' Cât timp urmărim o fereastră de PIN, nu mai pornim un al doilea urmăritor.
    Private _isPinWatcherActive As Boolean

    Public Sub New(logger As RichTextBoxLogger)
        _logger = logger
    End Sub

    ' Ciclul de viață al gardianului e cel al SESIUNII (browser deschis), nu al unei operații.
    ' De aceea își ține PROPRIUL CancellationTokenSource: token-ul unei operații e eliberat de
    ' apelant la sfârșitul ei — de regulă FĂRĂ să fie anulat — iar o buclă legată de el ar
    ' rămâne să scaneze la nesfârșit, câte un fir scurs la fiecare conectare.
    Private _cts As CancellationTokenSource

    ''' <summary>
    ''' Pornește bucla de scanare pe un fir de fundal. Idempotent: a doua chemare nu pornește
    ''' un al doilea scaner. Se oprește din <see cref="[Stop]"/>, la închiderea sesiunii.
    ''' </summary>
    Public Sub Start()
        Try
            If _cts IsNot Nothing Then Return          ' deja pornit
            _cts = New CancellationTokenSource()
            Dim ct As CancellationToken = _cts.Token

            Task.Run(Sub()
                         While Not ct.IsCancellationRequested
                             Try
                                 ' Scanăm doar dacă nu urmărim deja o fereastră activă.
                                 If Not _isPinWatcherActive Then ScanForSecurityWindow()
                             Catch
                                 ' Erorile punctuale din bucla de scanare se ignoră (ca în IPC):
                                 ' UI Automation aruncă des pe ferestre care dispar între apeluri.
                             End Try
                             Thread.Sleep(SCAN_INTERVAL_MS)
                         End While
                     End Sub, ct)
        Catch ex As Exception
            ' Pornirea gardianului nu are voie să oprească job-ul: fără el conectarea merge,
            ' doar că dialogul de PIN poate rămâne acoperit.
            _logger?.LogWarning($"[UI-GUARD] Nu am putut porni gardianul de PIN: {ex.Message}")
        End Try
    End Sub

    ''' <summary>Oprește scanarea (la închiderea sesiunii). Sigur de chemat de mai multe ori.</summary>
    Public Sub [Stop]()
        Try
            If _cts Is Nothing Then Return
            _cts.Cancel()
            _cts.Dispose()
            _cts = Nothing
        Catch ex As Exception
            _logger?.LogWarning($"[UI-GUARD] Nu am putut opri gardianul de PIN: {ex.Message}")
        End Try
    End Sub

    ''' <summary>Căutarea efectivă a ferestrei de autentificare.</summary>
    Private Sub ScanForSecurityWindow()
        Dim root As AutomationElement = AutomationElement.RootElement
        If root Is Nothing Then Return
        Dim winCondition As New PropertyCondition(AutomationElement.ClassNameProperty, "#32770")

        ' Doar copiii direcți ai Desktop-ului (ferestre top-level).
        Dim foundWins As AutomationElementCollection = root.FindAll(TreeScope.Children, winCondition)

        For Each window As AutomationElement In foundWins
            Dim title As String
            Try
                title = window.Current.Name.ToLowerInvariant()
            Catch
                Continue For   ' fereastra a dispărut între enumerare și citire
            End Try

            If Not EsteFereastraDeAutentificare(title) Then Continue For

            Dim hwndPtr As New IntPtr(window.Current.NativeWindowHandle)
            If hwndPtr = IntPtr.Zero Then Continue For

            ' Cedăm prim-planul ferestrelor noastre TopMost și pornim urmăritorul.
            Dim coborate As List(Of Form) = CedeazaPrimPlanul()
            _isPinWatcherActive = True
            Task.Run(Sub() WatchPinWindow(hwndPtr, coborate))
            Exit For
        Next
    End Sub

    ' Cuvintele-cheie din KBOT_IPC.ScanForSecurityWindow, neschimbate.
    Private Shared Function EsteFereastraDeAutentificare(titluMic As String) As Boolean
        If String.IsNullOrEmpty(titluMic) Then Return False
        Return titluMic.Contains("pin") OrElse
               titluMic.Contains("smart card") OrElse
               titluMic.Contains("token") OrElse
               titluMic.Contains("securitate") OrElse
               titluMic.Contains("security")
    End Function

    ''' <summary>
    ''' Coboară ferestrele TopMost ale aplicației și le întoarce, ca să știm exact pe care le
    ''' repunem. În KBOT_IPC era o singură fereastră (consola, mereu TopMost); aici sunt
    ''' oricâte, fiindcă shell-ul K-BOT are mai multe ferestre nemodale.
    ''' </summary>
    Private Function CedeazaPrimPlanul() As List(Of Form)
        Dim coborate As New List(Of Form)()
        Try
            For Each f As Form In Application.OpenForms.Cast(Of Form)().ToList()
                Dim tinta As Form = f
                If tinta.IsDisposed OrElse Not tinta.IsHandleCreated Then Continue For
                Try
                    tinta.Invoke(Sub()
                                     If tinta.TopMost Then
                                         tinta.TopMost = False
                                         tinta.SendToBack()
                                         coborate.Add(tinta)
                                     End If
                                 End Sub)
                Catch
                    ' Fereastra s-a închis între timp — nu e o eroare de raportat.
                End Try
            Next
            If coborate.Count > 0 Then
                _logger?.LogInfo($"[UI-GUARD] Fereastră PIN detectată. Cedez prim-planul ({coborate.Count} fereastră/ferestre).")
            Else
                _logger?.LogInfo("[UI-GUARD] Fereastră PIN detectată.")
            End If
        Catch ex As Exception
            _logger?.LogWarning($"[UI-GUARD] Nu am putut ceda prim-planul: {ex.Message}")
        End Try
        Return coborate
    End Function

    ''' <summary>
    ''' Urmărește HWND-ul cu <c>IsWindow</c>. Când dispare — sau la timeout — repune TopMost
    ''' pe EXACT ferestrele coborâte. Structura (Try / Finally care restaurează întotdeauna)
    ''' e cea din KBOT_IPC.WatchPinWindow.
    ''' </summary>
    Private Sub WatchPinWindow(targetHwnd As IntPtr, coborate As List(Of Form))
        Try
            Dim sw As Stopwatch = Stopwatch.StartNew()
            While sw.Elapsed.TotalSeconds < WATCH_TIMEOUT_SECONDS
                If Not IsWindow(targetHwnd) Then Exit While   ' operatorul a dat OK / Anulare
                Thread.Sleep(WATCH_POLL_MS)
            End While
        Catch
            ' Erori de sistem — ignorate, ca în IPC. Restaurarea se face oricum în Finally.
        Finally
            ' INDIFERENT de motivul ieșirii (închidere sau timeout), repunem ce am coborât.
            Try
                For Each f As Form In coborate
                    Dim tinta As Form = f
                    If tinta.IsDisposed OrElse Not tinta.IsHandleCreated Then Continue For
                    Try
                        tinta.Invoke(Sub()
                                         tinta.TopMost = True
                                         tinta.BringToFront()
                                     End Sub)
                    Catch
                        ' Fereastra s-a închis cât era PIN-ul pe ecran.
                    End Try
                Next
                _logger?.LogInfo("[UI-GUARD] Fereastra PIN s-a închis. Prim-plan restaurat.")
            Catch ex As Exception
                _logger?.LogWarning($"[UI-GUARD] Nu am putut restaura prim-planul: {ex.Message}")
            End Try

            ' Eliberăm steagul ca scanarea să poată prinde ferestrele următoare.
            _isPinWatcherActive = False
        End Try
    End Sub

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function IsWindow(hWnd As IntPtr) As Boolean
    End Function

End Class
