Imports System.IO
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
'Ver: 2.0.0

Module Program
    <DllImport("user32.dll")>
    Private Function SetProcessDPIAware() As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Function IsWindow(ByVal hWnd As IntPtr) As Boolean
    End Function

    <STAThread()>
    Sub Main()
        SetProcessDPIAware()

        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        ' Verificăm argumentele liniei de comandă
        Dim args As String() = Environment.GetCommandLineArgs()
        Dim jobFilePath As String = String.Empty
        Dim parentHwnd As IntPtr = IntPtr.Zero

        ' args(0) este calea executabilului, args(1) este primul parametru real
#If DEBUG Then
        If MessageBox.Show("Doriți să rulați în modul de testare IPC?", "Testare IPC", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
            ' MOD MANUAL (interactiv) -> confirmăm instalarea Playwright cu MsgBox
            PlaywrightBootstrap.EnsureChromiumInstalled(interactive:=True)
            Application.Run(New KBOT_STANDALONE())
            Return
        End If
        '###################################################
        parentHwnd = 198986
        jobFilePath = "C:\Avacont\FB_JOBS\FX_CONNECT.JSON"
        '###################################################

        If parentHwnd = IntPtr.Zero OrElse Not IsWindow(parentHwnd) Then
            MessageBox.Show("Nu am găsit fereastra Access pentru testare IPC!", "Eroare Test IPC", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Environment.Exit(-1)
        End If

        ' MOD IPC (nesupravegheat) -> instalare automată, fără dialoguri
        PlaywrightBootstrap.EnsureChromiumInstalled(interactive:=False)
        Application.Run(New KBOT_IPC(jobFilePath, parentHwnd))
#Else
        If args.Length > 2 Then
            jobFilePath = args(1)
            parentHwnd = CType(Convert.ToInt64(args(2)), IntPtr)

            If parentHwnd = IntPtr.Zero Then
                MessageBox.Show("Handle-ul ferestrei părinte este invalid.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
            If Not IsWindow(parentHwnd) Then
                MessageBox.Show("Handle-ul ferestrei părinte nu este valid.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ' MOD IPC cu parent Access (nesupravegheat) -> instalare automată, fără dialoguri
            PlaywrightBootstrap.EnsureChromiumInstalled(interactive:=False)
            Application.Run(New KBOT_IPC(jobFilePath, parentHwnd))

        ElseIf args.Length > 1 AndAlso File.Exists(args(1)) Then
            ' MOD AUTOMAT (BATCH) -> Lansăm KBOT_IPC cu calea fișierului JSON
            jobFilePath = args(1)
            ' Nesupravegheat -> instalare automată, fără dialoguri
            PlaywrightBootstrap.EnsureChromiumInstalled(interactive:=False)
            Application.Run(New KBOT_IPC(jobFilePath))
        Else
            ' MOD MANUAL -> Lansăm aplicația normală
            ' Interactiv -> confirmăm instalarea Playwright cu MsgBox
            PlaywrightBootstrap.EnsureChromiumInstalled(interactive:=True)
            Application.Run(New KBOT_STANDALONE())
        End If
#End If
    End Sub

End Module