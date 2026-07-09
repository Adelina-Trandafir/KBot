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

    <DllImport("dwmapi.dll", PreserveSig:=True)>
    Private Function DwmSetWindowAttribute(hwnd As IntPtr, dwAttribute As Integer,
                                           ByRef pvAttribute As Integer, cbAttribute As Integer) As Integer
    End Function

    <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)>
    Private Function SetWindowTheme(hWnd As IntPtr, pszSubAppName As String, pszSubIdList As String) As Integer
    End Function

    ' Guard-uri „loghează o singură dată” — pe versiuni de OS nesuportate eșecul e
    ' cronic și previzibil; nu vrem un log per formular.
    Private _dwmLogged As Boolean = False
    Private _uxLogged As Boolean = False

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
