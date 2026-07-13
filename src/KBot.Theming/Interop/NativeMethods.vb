Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' P/Invoke pentru bara de titlu (DWM) și tema scrollbar-urilor native (uxtheme).
''' Contract „zero excepții înghițite fără urmă”: pe eșec logăm O SINGURĂ DATĂ prin
''' GlobalErrorLog (guard static), apoi suprimăm — altfel am spama log-ul la fiecare
''' formular ne-tematizat pe Windows vechi unde atributul nu există.
''' </summary>
Friend Module NativeMethods

    ' DWMWA_USE_IMMERSIVE_DARK_MODE = 20 (Windows 10 v2004+ / Windows 11).
    Private Const DWMWA_USE_IMMERSIVE_DARK_MODE As Integer = 20

    ' Rotunjirea colțurilor ferestrei (DWM, Windows 11+). Pe Windows 10 atributul nu
    ' există: apelul eșuează, se loghează O DATĂ și fereastra rămâne pătrată — outcome
    ' acceptat (NU cădem pe Form.Region, care ar da margini crenelate, ne-antialiate).
    Private Const DWMWA_WINDOW_CORNER_PREFERENCE As Integer = 33
    Private Const DWMWCP_DEFAULT As Integer = 0
    Private Const DWMWCP_ROUND As Integer = 2

    ' Tragerea unei ferestre fără chenar de pe o zonă client (via mesaj non-client).
    Private Const WM_NCLBUTTONDOWN As Integer = &HA1
    Private Const HTCAPTION As Integer = 2

    ' ── Redimensionare / maximizare fereastră fără chenar (KBotShellForm) ──────
    ' Mesaje și coduri de hit-test folosite de shell-urile borderless redimensionabile.
    Friend Const WM_GETMINMAXINFO As Integer = &H24
    Friend Const WM_NCHITTEST As Integer = &H84
    Friend Const HTTRANSPARENT As Integer = -1
    Friend Const HTCLIENT As Integer = 1
    Friend Const HTLEFT As Integer = 10
    Friend Const HTRIGHT As Integer = 11
    Friend Const HTTOP As Integer = 12
    Friend Const HTTOPLEFT As Integer = 13
    Friend Const HTTOPRIGHT As Integer = 14
    Friend Const HTBOTTOM As Integer = 15
    Friend Const HTBOTTOMLEFT As Integer = 16
    Friend Const HTBOTTOMRIGHT As Integer = 17

    ' POINT / MINMAXINFO — definite O SINGURĂ DATĂ aici; consumate prin ApplyMinMaxInfo.
    <StructLayout(LayoutKind.Sequential)>
    Friend Structure NativePoint
        Public X As Integer
        Public Y As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure MINMAXINFO
        Public ptReserved As NativePoint
        Public ptMaxSize As NativePoint
        Public ptMaxPosition As NativePoint
        Public ptMinTrackSize As NativePoint
        Public ptMaxTrackSize As NativePoint
    End Structure

    <DllImport("dwmapi.dll", PreserveSig:=True)>
    Private Function DwmSetWindowAttribute(hwnd As IntPtr, dwAttribute As Integer,
                                           ByRef pvAttribute As Integer, cbAttribute As Integer) As Integer
    End Function

    <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)>
    Private Function SetWindowTheme(hWnd As IntPtr, pszSubAppName As String, pszSubIdList As String) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Function ReleaseCapture() As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    ' Guard-uri „loghează o singură dată” — pe versiuni de OS nesuportate eșecul e
    ' cronic și previzibil; nu vrem un log per formular.
    Private _dwmLogged As Boolean = False
    Private _uxLogged As Boolean = False
    Private _cornerLogged As Boolean = False

    ''' <summary>Setează bara de titlu dark/light pentru un formular.</summary>
    Public Sub SetTitleBarDark(f As Form, dark As Boolean)
        If f Is Nothing OrElse Not f.IsHandleCreated Then Return
        Try
            Dim value As Integer = If(dark, 1, 0)
            DwmSetWindowAttribute(f.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, value, 4)
        Catch ex As Exception
            If Not _dwmLogged Then
                _dwmLogged = True
                GlobalErrorLog.Write("NativeMethods.SetTitleBarDark (OS nesuportat?)", ex)
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Rotunjește colțurile unei ferestre fără chenar (DWM attr 33, Windows 11+). Pe
    ''' Windows 10 atributul nu există; apelul eșuează, se loghează o singură dată, iar
    ''' fereastra rămâne pătrată — outcome documentat și acceptat.
    ''' </summary>
    Public Sub SetRoundedCorners(f As Form, rounded As Boolean)
        If f Is Nothing OrElse Not f.IsHandleCreated Then Return
        Try
            Dim pref As Integer = If(rounded, DWMWCP_ROUND, DWMWCP_DEFAULT)
            DwmSetWindowAttribute(f.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, pref, 4)
        Catch ex As Exception
            If Not _cornerLogged Then
                _cornerLogged = True
                GlobalErrorLog.Write("NativeMethods.SetRoundedCorners (OS nesuportat?)", ex)
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Pornește tragerea ferestrei fără chenar: eliberează captura mouse-ului, apoi
    ''' trimite WM_NCLBUTTONDOWN/HTCAPTION ca și cum s-ar fi apăsat pe bara de titlu.
    ''' </summary>
    Public Sub DragMove(f As Form)
        If f Is Nothing OrElse Not f.IsHandleCreated Then Return
        Try
            ReleaseCapture()
            SendMessage(f.Handle, WM_NCLBUTTONDOWN, New IntPtr(HTCAPTION), IntPtr.Zero)
        Catch ex As Exception
            GlobalErrorLog.Write("NativeMethods.DragMove", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Completează MINMAXINFO (WM_GETMINMAXINFO) cu zona de lucru a monitorului
    ''' curent, ca o fereastră fără chenar maximizată să NU acopere taskbar-ul.
    ''' Se apelează DUPĂ MyBase.WndProc, ca ptMinTrackSize (MinimumSize) pus de
    ''' WinForms să rămână neatins — se suprascriu doar câmpurile de maximizare.
    ''' </summary>
    Public Sub ApplyMinMaxInfo(lParam As IntPtr, f As Form)
        If f Is Nothing OrElse Not f.IsHandleCreated Then Return
        Try
            Dim mmi As MINMAXINFO = Marshal.PtrToStructure(Of MINMAXINFO)(lParam)
            Dim scr As Screen = Screen.FromHandle(f.Handle)
            ' ptMaxPosition e relativ la originea monitorului, nu la ecranul virtual.
            mmi.ptMaxPosition.X = scr.WorkingArea.Left - scr.Bounds.Left
            mmi.ptMaxPosition.Y = scr.WorkingArea.Top - scr.Bounds.Top
            mmi.ptMaxSize.X = scr.WorkingArea.Width
            mmi.ptMaxSize.Y = scr.WorkingArea.Height
            Marshal.StructureToPtr(mmi, lParam, False)
        Catch ex As Exception
            GlobalErrorLog.Write(“NativeMethods.ApplyMinMaxInfo”, ex)
        End Try
    End Sub

    ''' <summary>
    ''' Aplică o temă vizuală uxtheme (ex. „DarkMode_Explorer”, „Explorer”) pe
    ''' scrollbar-urile native ale unui control. Erorile se loghează o singură dată.
    ''' </summary>
    Public Sub ApplyWindowTheme(ctrl As Control, theme As String)
        If ctrl Is Nothing OrElse Not ctrl.IsHandleCreated Then Return
        Try
            SetWindowTheme(ctrl.Handle, theme, Nothing)
        Catch ex As Exception
            If Not _uxLogged Then
                _uxLogged = True
                GlobalErrorLog.Write("NativeMethods.ApplyWindowTheme", ex)
            End If
        End Try
    End Sub

End Module
