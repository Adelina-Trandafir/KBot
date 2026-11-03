Option Strict On
Imports System.Runtime.InteropServices
Imports System.Threading

''' <summary>
''' Single-instance guard: the first KBotAdmin owns a named mutex for its whole life. A second launch
''' finds it taken, asks the running one to show itself and quits. The running window is usually hidden
''' in the tray (no window handle to activate), so the request is a registered window message that
''' MainForm / ConnectionForm answer in WndProc.
''' </summary>
Friend Module SingleInstance

    Private Const MUTEX_NAME As String = "Local\KBotAdmin_SingleInstance_5B3E7D12"
    Private Const MESSAGE_NAME As String = "KBotAdmin_ShowYourself_5B3E7D12"
    Private ReadOnly HWND_BROADCAST As New IntPtr(&HFFFF)

    Private _mutex As Mutex
    Private _showMessage As Integer

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Function RegisterWindowMessage(k_name As String) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Function PostMessage(k_hwnd As IntPtr, k_msg As Integer, k_wParam As IntPtr, k_lParam As IntPtr) As Boolean
    End Function

    ''' <summary>The window message a second launch sends to the running instance.</summary>
    Friend ReadOnly Property ShowMessage As Integer
        Get
            If _showMessage = 0 Then _showMessage = RegisterWindowMessage(MESSAGE_NAME)
            Return _showMessage
        End Get
    End Property

    ''' <summary>
    ''' True when this process is the only KBotAdmin. False means another one is running (and was asked
    ''' to show itself): the caller must quit without starting anything.
    ''' </summary>
    Friend Function TryAcquire() As Boolean
        Try
            Dim k_createdNew As Boolean
            _mutex = New Mutex(True, MUTEX_NAME, k_createdNew)
            If k_createdNew Then Return True

            _mutex.Dispose()
            _mutex = Nothing
            PostMessage(HWND_BROADCAST, ShowMessage, IntPtr.Zero, IntPtr.Zero)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("SingleInstance.TryAcquire", ex)
            Return True   ' never block the launch over a failed guard
        End Try
    End Function

    ''' <summary>Releases the mutex at shutdown. Safe to call when it was never taken.</summary>
    Friend Sub Release()
        Try
            If _mutex Is Nothing Then Return
            _mutex.ReleaseMutex()
            _mutex.Dispose()
            _mutex = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("SingleInstance.Release", ex)
        End Try
    End Sub

End Module
