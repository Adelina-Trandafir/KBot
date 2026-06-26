Imports System.Runtime
Imports System.Text
Imports System.Threading
Imports System.Windows.Automation

''' <summary>
''' Automatizare robustă folosind UI Automation (System.Windows.Automation)
''' Aceasta vede ferestrele pe care EnumWindows le ratează.
''' </summary>
Public Class WindowsSecurityAutomation
    ' Funcție care returnează True dacă browserul a ajuns deja pe pagina următoare
    Public Property IsLoginCompleteCheck As Func(Of Boolean)

    Private ReadOnly _logger As RichTextBoxLogger
    Private ReadOnly _certificateName As String
    Private ReadOnly _pin As String
    Private ReadOnly _cancellationToken As CancellationToken

    Private _isRunning As Boolean = False
    Private Property _certificateSelected As Boolean = False
    Private _pinEntered As Boolean = False
    Private _manualPinMode As Boolean = False

    Public Sub New(logger As RichTextBoxLogger, certificateName As String, pin As String, cancellationToken As CancellationToken)
        Try
            _logger = logger
            _certificateName = certificateName
            _pin = pin
            _cancellationToken = cancellationToken

        Catch ex As Exception
            _logger.LogError($"[WinSec] Eroare la inițializare: {ex.Message}")
        End Try
    End Sub

    Public Sub StartMonitoring()
        Try
            If _isRunning Then Return
            _isRunning = True
            _certificateSelected = False
            _pinEntered = False

            Dim monitorThread As New Thread(AddressOf MonitoringLoop)
            monitorThread.IsBackground = True
            monitorThread.SetApartmentState(ApartmentState.STA) ' CRITIC pentru UI Automation
            monitorThread.Start()

            _logger.LogInfo($"[WinSec] Pornit monitorizare UIA pentru cert: {_certificateName}")
        Catch ex As Exception
            _logger.LogError($"[WinSec] Eroare la pornirea monitorizării: {ex.Message}")
        End Try

    End Sub

    ' Constructor actualizat sau proprietate
    Public Sub SetManualPinMode(enabled As Boolean)
        _manualPinMode = enabled
    End Sub

    Public Sub StopMonitoring()
        _isRunning = False
    End Sub

    Public ReadOnly Property IsComplete As Boolean
        Get
            If _manualPinMode Then
                ' În mod manual, considerăm partea de UI Automation gata doar după ce s-a selectat certificatul.
                ' Restul (PIN) e treaba utilizatorului.
                Return _certificateSelected
            Else
                Return _certificateSelected AndAlso _pinEntered
            End If
        End Get
    End Property

    Private Sub MonitoringLoop()
        Dim root As AutomationElement = AutomationElement.RootElement

        ' Condiții pentru găsirea ferestrelor principale
        Dim chromeCondition As New PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_WidgetWin_1")
        Dim pinWindowCondition As New PropertyCondition(AutomationElement.ClassNameProperty, "#32770") ' Dialog sistem

        While _isRunning AndAlso Not _cancellationToken.IsCancellationRequested
            Try
                ' =========================================================================
                ' LOGICA 2: SELECTARE CERTIFICAT
                ' SE RULEAZA DOAR DACA 2 = FALSE (Certificatul nu a fost selectat încă)
                ' =========================================================================
                If Not _certificateSelected Then
                    ' 1. Găsim fereastra principală Chrome
                    Dim chromeWindow As AutomationElement = root.FindFirst(TreeScope.Children, chromeCondition)

                    'poate certificatul a fost autoselectat deja prin chrome://policy
                    'verific daca exista un anume element in pagina urmatoare

                    If chromeWindow IsNot Nothing Then
                        ' 2. Căutăm ancora (Select a certificate) STRICT în interiorul Chrome
                        ' Folosim funcția optimizată FindAnchorSmart pe care o ai deja
                        Dim anchor As AutomationElement = FindAnchorSmart(chromeWindow)

                        If anchor IsNot Nothing Then
                            ' 3. Dacă am găsit ancora, gestionăm certificatul
                            If HandleCertificate(anchor) Then
                                _certificateSelected = True
                                _logger.LogInfo("[WinSec] Certificat selectat. Trec la etapa următoare.")
                            End If
                        End If
                    End If
                End If

                If _certificateSelected Then
                    If _manualPinMode Then
                        _logger.LogInfo("[WinSec] Certificat selectat. Aștept introducerea manuală a PIN-ului...")

                        ' Ieșim din bucla de automation pentru că nu mai avem ce apăsa
                        Exit While
                    Else
                        ' =========================================================================
                        ' LOGICA 1 și 3: CACHE sau PIN
                        ' SE RULEAZA CAND 2 = TRUE (Certificat selectat) SI 3 = FALSE (PIN neintrodus)
                        ' =========================================================================
                        If Not _pinEntered Then
                            ' --- PASUL 1: VERIFIC CACHE (Logica 1) ---
                            ' Verificăm dacă browserul a încărcat deja următorul element
                            If IsLoginCompleteCheck IsNot Nothing Then
                                Try
                                    If IsLoginCompleteCheck.Invoke() Then
                                        _logger.LogSuccess("[WinSec] CACHE DETECTAT: Elementul următor s-a încărcat. PIN-ul a fost sărit.")
                                        _pinEntered = True ' Considerăm PIN-ul rezolvat
                                    End If
                                Catch
                                    ' Ignorăm erori de comunicare cu browserul
                                End Try
                            End If

                            ' --- PASUL 3: VERIFIC FEREASTRA PIN (Logica 3) ---
                            ' Doar dacă Cache-ul nu a confirmat deja succesul
                            If Not _pinEntered Then
                                ' Căutăm ferestre de tip Dialog (#32770) pe Desktop
                                Dim dialogs As AutomationElementCollection = root.FindAll(TreeScope.Children, pinWindowCondition)

                                For Each window As AutomationElement In dialogs
                                    Dim title As String = ""
                                    Try
                                        title = window.Current.Name
                                    Catch
                                        Continue For
                                    End Try

                                    ' Verificăm dacă este fereastra de PIN
                                    If IsPinWindow(title) Then
                                        If HandlePin(window) Then
                                            _pinEntered = True
                                            _logger.LogSuccess("[WinSec] PIN introdus manual.")
                                            Exit For
                                        End If
                                    End If
                                Next
                            End If
                        End If

                        ' =========================================================================
                        ' IEȘIRE: CAND 2=TRUE si 3=TRUE -> iese din while
                        ' =========================================================================
                        If _certificateSelected AndAlso _pinEntered Then
                            _logger.LogSuccess("[WinSec] Autentificare completă (Certificat + PIN/Cache).")
                            Exit While
                        End If
                    End If
                End If

            Catch ex As Exception
                _logger.LogDebug($"[WinSec] Eroare în buclă: {ex.Message}")
            End Try

            ' Pauză scurtă pentru a nu bloca procesorul
            Thread.Sleep(500)
        End While

        _isRunning = False
    End Sub

    ' --- DETECȚIE TITLURI ---
    Private Function IsCertificateWindow(title As String) As Boolean
        Dim t = title.ToLower()
        Return t.Contains("select a certificate") OrElse
               t.Contains("selectați un certificat") OrElse
               t.Contains("securitate windows") OrElse
               t.Contains("windows security") OrElse
               t.Contains("certificate")
    End Function

    Private Function IsPinWindow(title As String) As Boolean
        Dim t = title.ToLower()
        Return t.Contains("pin") OrElse
               t.Contains("smart card") OrElse
               t.Contains("token") OrElse
               t.Contains("securitate windows") OrElse
               t.Contains("windows security")
    End Function

    Private Function FindAnchorSmart(window As AutomationElement) As AutomationElement
        ' Folosim un cronometru pentru siguranță, nu un contor fix
        Dim sw As Stopwatch = Stopwatch.StartNew()
        Dim timeLimitMs As Long = 3000 ' 3 secunde maxim de căutare

        Dim walker As TreeWalker = TreeWalker.ControlViewWalker
        Dim child As AutomationElement = walker.GetFirstChild(window)

        While child IsNot Nothing
            ' 1. PROTECȚIE TIMP: Dacă au trecut 3 secunde și n-am găsit, ieșim ca să nu blocăm aplicația
            If sw.ElapsedMilliseconds > timeLimitMs Then
                _logger.LogDebug("[WinSec] Timeout la căutarea ancorei. Prea multe elemente.")
                Exit While
            End If

            Try
                ' 2. OPTIMIZARE MAJORĂ: Ignorăm explicit conținutul paginii web
                ' Dacă elementul e de tip "Document" sau "Group", e probabil pagina web uriașă. O sărim.
                ' Asta ne lasă să scanăm restul interfeței foarte repede.
                Dim chType As ControlType = child.Current.ControlType

                If chType Is ControlType.Document OrElse chType Is ControlType.Group Then
                    ' Trecem rapid la următorul frate fără să verificăm altceva
                    child = walker.GetNextSibling(child)
                    Continue While
                End If

                ' 3. Verificarea numelui
                If child.Current.Name = "Select a certificate" Then
                    _logger.LogInfo("[WinSec] Anchor găsit!")
                    Return child
                End If

                ' (Opțional) Dacă e un "Pane" simplu, s-ar putea ca popup-ul să fie imediat sub el
                ' Aici ai putea face un mic "peek" înăuntru, dar de obicei popup-urile sunt la nivel înalt.

            Catch ex As Exception
                ' Elementele UI pot dispărea în timp ce le scanăm, ignorăm eroarea
            End Try

            ' Trecem la următorul
            child = walker.GetNextSibling(child)
        End While

        Return Nothing
    End Function

    Private Function HandleCertificate(anchor As AutomationElement) As Boolean
        Try
            ' 1. Căutăm UN SINGUR container (List/Table/DataGrid) în interiorul Ancorei
            ' Folosim FindFirst pentru viteză. De obicei există o singură listă în acest popup.
            Dim listCondition As New OrCondition(
            New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.List),
            New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Table),
            New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataGrid)
        )

            Dim listElement As AutomationElement = anchor.FindFirst(TreeScope.Descendants, listCondition)

            If listElement Is Nothing Then
                _logger.LogError("[WinSec] Popup-ul a fost găsit, dar nu conține nicio listă/tabel.")
                Return False
            End If

            ' 2. Iterăm rândurile folosind TreeWalker (mult mai rapid decât FindAll)
            Dim walker As TreeWalker = TreeWalker.ControlViewWalker
            Dim child As AutomationElement = walker.GetFirstChild(listElement)
            Dim searchParts As String() = _certificateName.Split(" "c, StringSplitOptions.RemoveEmptyEntries)

            Dim targetItem As AutomationElement = Nothing

            While child IsNot Nothing
                ' a) Obținem textul rândului
                Dim rowText As String = child.Current.Name

                ' b) Dacă rândul nu are nume (se întâmplă la tabele complexe), concatenăm textul copiilor
                If String.IsNullOrWhiteSpace(rowText) Then
                    Dim subChildren As AutomationElementCollection = child.FindAll(TreeScope.Children, Condition.TrueCondition)
                    For Each subChild As AutomationElement In subChildren
                        rowText &= subChild.Current.Name & " "
                    Next
                End If

                ' c) Verificăm dacă nu cumva e Header-ul tabelului (ca să nu dăm click pe el)
                ' Logica simplificată: Dacă conține "Subject" și "Issuer", e probabil antetul
                If rowText.IndexOf("Subject", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso
               rowText.IndexOf("Issuer", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    ' E header, trecem mai departe
                    child = walker.GetNextSibling(child)
                    Continue While
                End If

                ' d) Verificăm potrivirea cu numele certificatului căutat
                Dim isMatch As Boolean = True
                If Not String.IsNullOrWhiteSpace(rowText) Then
                    For Each part In searchParts
                        If rowText.IndexOf(part, StringComparison.OrdinalIgnoreCase) < 0 Then
                            isMatch = False
                            Exit For
                        End If
                    Next
                Else
                    isMatch = False
                End If

                ' Dacă am găsit certificatul
                If isMatch Then
                    targetItem = child
                    _logger.LogInfo($"[WinSec] Certificat identificat: {rowText}")
                    Exit While ' Ieșim din buclă instant
                End If

                ' Trecem la următorul rând
                child = walker.GetNextSibling(child)
            End While

            ' 3. ACȚIUNEA (SmartSelect + Click OK)
            If targetItem IsNot Nothing Then
                ' Apelăm funcția ta specială
                If SmartSelect(targetItem) Then
                    System.Threading.Thread.Sleep(200) ' Scurtă pauză pentru ca UI să proceseze selecția

                    ' Click pe butonul OK din Ancoră
                    Return ClickButton(anchor, "OK")
                Else
                    _logger.LogError("[WinSec] SmartSelect a eșuat pe rândul identificat.")
                    Return False
                End If
            Else
                _logger.LogError($"[WinSec] Certificatul '{_certificateName}' nu a fost găsit în listă.")
                Return False
            End If

        Catch ex As Exception
            _logger.LogError($"[WinSec] Eroare în HandleCertificate: {ex.Message}")
            Return False
        End Try
    End Function

    Private Function HandleCertificate_B(anchor As AutomationElement) As Boolean
        Try
            ' 1. Căutăm containerele (Custom, List, Table, DataGrid)
            ' Folosim o condiție OR pentru a prinde toate variațiile posibile de randare
            Dim listCondition As New OrCondition(
                New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom),
                New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.List),
                New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Table),
                New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataGrid)
            )

            ' Obținem toate elementele posibile
            Dim collections As AutomationElementCollection = anchor.FindAll(TreeScope.Descendants, listCondition)

            _logger.LogDebug($"[WinSec] Scanare rapidă: {collections.Count} elemente potențiale.")

            Dim validCertRows As New Dictionary(Of String, AutomationElement)
            Dim searchParts As String() = _certificateName.Split(" "c, StringSplitOptions.RemoveEmptyEntries)
            Dim foundHeader As Boolean = False
            Dim isHeader As Boolean = False

            For i As Integer = 0 To collections.Count - 1 ' To 0 Step -1
                Dim item As AutomationElement = collections(i)
                Dim children As AutomationElementCollection = item.FindAll(TreeScope.Children, Condition.TrueCondition)
                If children.Count = 3 Then
                    Dim col1 As String = children(0).Current.Name.ToLower()
                    Dim col2 As String = children(1).Current.Name.ToLower()
                    Dim col3 As String = children(2).Current.Name.ToLower()

                    ' Verificare rapidă dacă e null
                    If String.IsNullOrWhiteSpace(col1) Or String.IsNullOrWhiteSpace(col2) Or String.IsNullOrWhiteSpace(col2) Then Continue For

                    Dim col1Potential As Boolean = col1.Contains("subject") OrElse col1.Contains("subiect") OrElse col1.Contains("issuer") OrElse col1.Contains("emitent")
                    Dim col2Potential As Boolean = col2.Contains("issuer", StringComparison.CurrentCultureIgnoreCase)
                    Dim col3Potential As Boolean = col3.Contains("serial", StringComparison.CurrentCultureIgnoreCase)

                    If col1Potential AndAlso col2Potential AndAlso col3Potential Then
                        foundHeader = True
                        Continue For
                    ElseIf foundHeader Then
                        validCertRows.TryAdd(col1, item)
                    End If
                End If
            Next

            ' ==============================================================================
            ' 5. LOGICA DE DECIZIE (Trust the Count)
            ' ==============================================================================
            Dim targetItem As AutomationElement = Nothing
            Dim foundMatch As Boolean = False

            If validCertRows.Count = 0 Then
                _logger.LogError("[WinSec] EROARE: Nu am găsit niciun rând care să semene a certificat (3 coloane).")
                Return False
            End If

            ' Spargem numele căutat (din ComboBox) în cuvinte pentru a face potrivirea
            searchParts = _certificateName.Split(" "c, StringSplitOptions.RemoveEmptyEntries)

            For Each kvp In validCertRows
                Dim rowSubjectName As String = kvp.Key ' Cheia este textul din prima coloană (Numele)
                Dim rowElement As AutomationElement = kvp.Value

                ' Construim textul complet al rândului (Nume + Emitent + etc.) pentru verificare
                Dim rowFullText As String = ""
                Dim children = rowElement.FindAll(TreeScope.Children, Condition.TrueCondition)
                For Each child In children
                    rowFullText &= child.Current.Name & " "
                Next

                ' VERIFICARE: Toate părțile din numele selectat în ComboBox trebuie să existe în rândul curent
                Dim isMatch As Boolean = True
                For Each part In searchParts
                    ' Folosim IndexOf pentru a fi case-insensitive
                    If rowFullText.IndexOf(part, StringComparison.OrdinalIgnoreCase) < 0 Then
                        isMatch = False
                        Exit For
                    End If
                Next

                If isMatch Then
                    targetItem = rowElement
                    foundMatch = True
                    _logger.LogInfo($"[WinSec] Identificat certificat corect: {rowSubjectName}")
                    Exit For ' Ieșim din buclă, l-am găsit
                End If
            Next

            'ElseIf validCertRows.Count > 1 Then
            '    _logger.LogError($"[WinSec] EROARE DE SECURITATE: Am găsit {validCertRows.Count} certificate în listă!")
            '    _logger.LogError("Pentru a evita blocarea token-ului, vă rugăm să lăsați conectat DOAR stick-ul corect.")
            '    Return False

            'Else
            '    ' --- AVEM EXACT 1 CERTIFICAT ---
            '    targetItem = validCertRows(0)
            'End If

            ' Dacă după ce am verificat toate rândurile nu am găsit unul potrivit
            If Not foundMatch Then
                _logger.LogError($"[WinSec] EROARE: Certificatul selectat '{_certificateName}' nu a fost găsit în lista ferestrei.")
                _logger.LogDebug("Certificate disponibile în fereastră: " & String.Join(", ", validCertRows.Keys))
                Return False
            End If

            ' 6. ACȚIUNEA (Selectare și Click OK)
            _logger.LogSuccess($"[WinSec] Selectez certificatul...")

            If targetItem IsNot Nothing Then
                If SmartSelect(targetItem) Then
                    _logger.LogInfo("[WinSec] Element selectat cu succes.")
                    System.Threading.Thread.Sleep(200)
                    Return ClickButton(anchor, "OK")
                Else
                    _logger.LogError("[WinSec] Nu s-a putut selecta rândul nici după navigarea la părinți.")
                    Return False
                End If
            End If

        Catch ex As Exception
            _logger.LogError($"[WinSec] Eroare: {ex.Message}")
        End Try

        Return False
    End Function

    ''' <summary>
    ''' Încearcă să selecteze elementul. Dacă nu suportă, urcă la părinte și încearcă din nou.
    ''' </summary>
    Private Function SmartSelect(element As AutomationElement, Optional depth As Integer = 0) As Boolean
        ' Nu urcăm mai mult de 3 nivele pentru a nu selecta containerul întreg
        If element Is Nothing OrElse depth > 3 Then Return False

        Dim success As Boolean = False

        ' 1. Încercăm SelectionItemPattern (Ideal pentru liste - echivalentul la a da click pentru a selecta)
        Try
            Dim selPattern As SelectionItemPattern = TryCast(element.GetCurrentPattern(SelectionItemPattern.Pattern), SelectionItemPattern)
            If selPattern IsNot Nothing Then
                selPattern.Select()
                _logger.LogDebug($"[WinSec] Selectat via SelectionItemPattern la nivelul {depth}")
                Return True
            End If
        Catch
        End Try

        ' 2. Încercăm InvokePattern (Echivalentul unui Click pe buton/item)
        ' Asta înlocuiește LegacyIAccessiblePattern care îți dădea eroare
        Try
            Dim invPattern As InvokePattern = TryCast(element.GetCurrentPattern(InvokePattern.Pattern), InvokePattern)
            If invPattern IsNot Nothing Then
                invPattern.Invoke()
                _logger.LogDebug($"[WinSec] Activat via InvokePattern la nivelul {depth}")
                Return True
            End If
        Catch
        End Try

        ' 3. Încercăm SetFocus simplu (uneori e suficient ca să se albăstrească rândul)
        Try
            If element.Current.IsKeyboardFocusable Then
                element.SetFocus()
                _logger.LogDebug($"[WinSec] Selectat via SetFocus la nivelul {depth}")
                Return True
            End If
        Catch
        End Try

        ' DACA NIMIC NU A MERS, URCĂM LA PĂRINTE
        ' (Dacă am găsit TEXTUL "Popescu", dar textul nu e clickabil, urcăm la Rândul care îl conține)
        If Not success Then
            Try
                Dim walker As TreeWalker = TreeWalker.ControlViewWalker
                Dim parent As AutomationElement = walker.GetParent(element)

                If parent IsNot Nothing AndAlso Not parent.Equals(AutomationElement.RootElement) Then
                    _logger.LogDebug($"[WinSec] Nivelul {depth} nu suportă interacțiune. Urc la părinte: {parent.Current.ControlType.ProgrammaticName}")
                    Return SmartSelect(parent, depth + 1)
                End If
            Catch ex As Exception
                _logger.LogDebug($"[WinSec] Eroare la obținerea părintelui: {ex.Message}")
            End Try
        End If

        Return False
    End Function

    Private Sub DebugElement(element As AutomationElement, index As Integer)
        Try
            _logger.LogInfo($"--- INSPECTARE ELEMENT #{index} ---")

            ' 1. Proprietăți de bază
            _logger.LogInfo($"   Type: {element.Current.ControlType.ProgrammaticName}")
            _logger.LogInfo($"   Name: '{element.Current.Name}'")
            _logger.LogInfo($"   AutomationId: '{element.Current.AutomationId}'")
            _logger.LogInfo($"   ClassName: '{element.Current.ClassName}'")

            ' 2. Verificăm dacă are copii (adesea textul e ascuns aici)
            Dim children As AutomationElementCollection = element.FindAll(TreeScope.Children, Condition.TrueCondition)

            If children.Count > 0 Then
                _logger.LogInfo($"   Are {children.Count} elemente copil:")
                For i As Integer = 0 To children.Count - 1
                    Dim child = children(i)
                    _logger.LogInfo($"      [Child {i}] Type: {child.Current.ControlType.ProgrammaticName}, Name: '{child.Current.Name}'")
                Next
            Else
                _logger.LogInfo("   Nu are elemente copil.")
            End If

        Catch ex As Exception
            _logger.LogDebug($"   Eroare la inspectare: {ex.Message}")
        End Try
    End Sub

    Private Function HandlePin(window As AutomationElement) As Boolean
        Try
            ' PASUL 1: Focus pe FEREASTRA principală (nu pe controlul din ea)
            ' Asta aduce dialogul în față
            Try
                window.SetFocus()
            Catch
                ' Ignorăm dacă nu putem pune focus pe fereastră (uneori e deja active)
            End Try

            Thread.Sleep(200)

            ' Căutăm câmpul de editare doar pentru a confirma că suntem în fereastra bună
            Dim editBox As AutomationElement = window.FindFirst(TreeScope.Descendants, New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit))

            If editBox IsNot Nothing Then
                ' Încercăm focus explicit, dar cu plasă de siguranță
                Try
                    If editBox.Current.IsEnabled Then
                        editBox.SetFocus()
                    End If
                Catch ex As Exception
                    ' AICI E FIX-UL: Ignorăm eroarea "Cannot receive focus".
                    ' De obicei cursorul e deja acolo implicit.
                    _logger.LogDebug($"[WinSec] Focus direct pe edit a eșuat ({ex.Message}), dar continui...")
                End Try

                Thread.Sleep(100)

                ' Trimitem PIN-ul (Blind typing)
                _logger.LogInfo("[WinSec] Trimit PIN-ul...")
                System.Windows.Forms.SendKeys.SendWait(_pin)
                Thread.Sleep(500)

                ' Apăsăm OK
                Return ClickButton(window, "OK")
            Else
                _logger.LogWarning("[WinSec] Nu am găsit câmpul PIN explicit. Încerc Enter blind.")
                System.Windows.Forms.SendKeys.SendWait(_pin)
                System.Windows.Forms.SendKeys.SendWait("{ENTER}")
                Return True
            End If
        Catch ex As Exception
            _logger.LogError($"[WinSec] Eroare manipulare PIN: {ex.Message}")
        End Try
        Return False
    End Function

    Private Function ClickButton(window As AutomationElement, buttonName As String) As Boolean
        Try
            Dim btn As AutomationElement = window.FindFirst(TreeScope.Descendants,
                New AndCondition(
                    New PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                    New PropertyCondition(AutomationElement.NameProperty, buttonName)
                ))

            If btn IsNot Nothing Then
                Dim invokePat As InvokePattern = DirectCast(btn.GetCurrentPattern(InvokePattern.Pattern), InvokePattern)
                invokePat.Invoke()
                Return True
            Else
                ' Fallback la Enter
                System.Windows.Forms.SendKeys.SendWait("{ENTER}")
                Return True
            End If
        Catch
            Return False
        End Try
    End Function

End Class