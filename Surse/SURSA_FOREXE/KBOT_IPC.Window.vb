Imports System.Text
Imports System.Threading

Partial Public Class KBOT_IPC
    ' =============================================================
    '  FUNCTIA DE CAUTARE (PUBLIC)
    ' =============================================================
    Public Function FindWindowByTitleInsideAccess(parentAccessHwnd As IntPtr, partOfTitle As String) As IntPtr
        Dim foundHwnd As IntPtr = IntPtr.Zero

        ' Verificare de siguranta
        If parentAccessHwnd = IntPtr.Zero Then Return IntPtr.Zero

        ' Definim Callback-ul (Lambda Expression)
        ' Aceasta functie mica va fi apelata de Windows pentru FIECARE fereastra copil gasita
        Dim callback As EnumWindowsProc = Function(hWnd, lParam)
                                              ' 1. Aflam lungimea titlului
                                              Dim length As Integer = GetWindowTextLength(hWnd)
                                              If length = 0 Then Return True ' Nu are titlu, trecem la urmatoarea

                                              ' 2. Citim titlul
                                              Dim sb As New StringBuilder(length + 1)
                                              GetWindowText(hWnd, sb, sb.Capacity)
                                              Dim currentTitle As String = sb.ToString()

                                              ' 3. Verificam daca contine textul cautat (Case Insensitive)
                                              If currentTitle.IndexOf(partOfTitle, StringComparison.OrdinalIgnoreCase) >= 0 Then
                                                  foundHwnd = hWnd
                                                  Return False ' STOP! Am gasit-o, nu mai cautam.
                                              End If

                                              Return True ' Continua cautarea
                                          End Function

        ' Pornim cautarea recursiva
        EnumChildWindows(parentAccessHwnd, callback, IntPtr.Zero)

        Return foundHwnd
    End Function

    ''' <summary>
    ''' Pornește un Task de fundal care scanează periodic apariția ferestrelor de PIN/Securitate.
    ''' </summary>
    Private Sub StartUiGuardian(cancelToken As CancellationToken)
        Task.Run(Sub()
                     While Not cancelToken.IsCancellationRequested
                         Try
                             ' Rulăm logica de scanare doar dacă nu monitorizăm deja o fereastră activă
                             If Not _isPinWatcherActive Then
                                 ScanForSecurityWindow()
                             End If
                         Catch ex As Exception
                             ' Ignorăm erorile punctuale din bucla de scanare
                         End Try

                         ' Scanăm o dată pe secundă pentru a nu încărca CPU-ul
                         Thread.Sleep(1000)
                     End While
                 End Sub, cancelToken)
    End Sub

    ''' <summary>
    ''' Logica efectivă de căutare a ferestrei (separată pentru claritate).
    ''' </summary>
    Private Sub ScanForSecurityWindow()
        Dim root As System.Windows.Automation.AutomationElement = System.Windows.Automation.AutomationElement.RootElement
        Dim winCondition As New System.Windows.Automation.PropertyCondition(System.Windows.Automation.AutomationElement.ClassNameProperty, "#32770")

        ' Căutăm doar copiii direcți ai Desktop-ului (ferestre top-level)
        Dim foundWins As System.Windows.Automation.AutomationElementCollection = root.FindAll(System.Windows.Automation.TreeScope.Children, winCondition)

        For Each window As System.Windows.Automation.AutomationElement In foundWins
            Dim title As String = ""
            Try
                title = window.Current.Name.ToLower()
            Catch
                Continue For
            End Try

            ' Cuvinte cheie pentru ferestrele de autentificare
            If title.Contains("pin") OrElse title.Contains("smart card") OrElse title.Contains("token") OrElse title.Contains("securitate") OrElse title.Contains("security") Then

                ' 1. Obținem HWND-ul pentru monitorizare eficientă
                Dim hwndInt As Integer = window.Current.NativeWindowHandle
                Dim hwndPtr As New IntPtr(hwndInt)

                ' 2. Ascundem KBOT_STANDALONE (TopMost = False) pentru a lăsa PIN-ul să se vadă
                Me.Invoke(Sub()
                              If Me.TopMost Then
                                  Me.TopMost = False
                                  Me.SendToBack()
                                  Try : _logger.LogInfo("[UI-GUARD] Fereastră PIN detectată. Dezactivez TopMost.") : Catch : End Try
                              End If
                          End Sub)

                ' 3. Pornim Watcher-ul care va aștepta închiderea ferestrei
                _isPinWatcherActive = True
                Task.Run(Sub() WatchPinWindow(hwndPtr))

                ' Ne oprim din scanat această iterație, am găsit ce căutam
                Exit For
            End If
        Next
    End Sub

    ''' <summary>
    ''' Monitorizează un HWND specific folosind API-ul IsWindow. 
    ''' Când fereastra dispare, reactivează TopMost.
    ''' </summary>
    Private Sub WatchPinWindow(targetHwnd As IntPtr)
        Try
            Dim sw As Stopwatch = Stopwatch.StartNew()
            Dim timeOutSeconds As Integer = 120 ' Timeout de siguranță (2 min)

            While sw.Elapsed.TotalSeconds < timeOutSeconds
                ' Verificare ultra-rapidă dacă fereastra mai există
                If Not IsWindow(targetHwnd) Then
                    Exit While ' Fereastra s-a închis (Userul a dat OK/Cancel)
                End If
                Thread.Sleep(200)
            End While

        Catch ex As Exception
            ' Erori de sistem (ignorate safe)
        Finally
            ' INDIFERENT de motivul ieșirii (Succes sau Timeout), repunem TopMost
            Me.Invoke(Sub()
                          Me.TopMost = True
                          Me.BringToFront()
                          Try : _logger.LogInfo("[UI-GUARD] Fereastra PIN s-a închis. TopMost reactivat.") : Catch : End Try
                      End Sub)

            ' Eliberăm flag-ul pentru a permite scanarea viitoarelor ferestre
            _isPinWatcherActive = False
        End Try
    End Sub
End Class
