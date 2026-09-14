Imports System
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Threading
Imports KBot.Common

''' <summary>
''' Single-instance guard: the first process owns a named mutex for its whole life.
''' A second launch finds the mutex taken, brings the running window to the front and quits.
''' </summary>
Friend Module SingleInstance

    Private Const MUTEX_NAME As String = "Local\KBOT_SingleInstance_9F2A6C41"

    Private _mutex As Mutex

    Private Const SW_RESTORE As Integer = 9

    <DllImport("user32.dll")>
    Private Function SetForegroundWindow(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Function ShowWindowAsync(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Function IsIconic(hWnd As IntPtr) As Boolean
    End Function

    ''' <summary>
    ''' True when this process is the only one running. False means another instance was
    ''' already up (and has been activated) — the caller must exit without starting anything.
    ''' </summary>
    Friend Function TryAcquire() As Boolean
        Try
            Dim createdNew As Boolean
            _mutex = New Mutex(True, MUTEX_NAME, createdNew)
            If createdNew Then Return True

            _mutex.Dispose()
            _mutex = Nothing
            ActivateRunningInstance()
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

    Private Sub ActivateRunningInstance()
        Try
            Dim current As Process = Process.GetCurrentProcess()
            For Each other As Process In Process.GetProcessesByName(current.ProcessName)
                Using other
                    If other.Id = current.Id Then Continue For
                    Dim h As IntPtr = other.MainWindowHandle
                    If h = IntPtr.Zero Then Continue For
                    If IsIconic(h) Then ShowWindowAsync(h, SW_RESTORE)
                    SetForegroundWindow(h)
                    Return
                End Using
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("SingleInstance.ActivateRunningInstance", ex)
        End Try
    End Sub

End Module
