Imports System.Runtime.InteropServices

Public Class WindowSnapper
    <DllImport("user32.dll")>
    Private Shared Sub keybd_event(bVk As Byte, bScan As Byte, dwFlags As UInteger, dwExtraInfo As UIntPtr)
    End Sub

    <DllImport("user32.dll")>
    Private Shared Function SetForegroundWindow(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

    Private Const VK_LWIN As Byte = &H5B
    Private Const VK_LEFT As Byte = &H25
    Private Const VK_RIGHT As Byte = &H27
    Private Const VK_UP As Byte = &H26
    Private Const KEYEVENTF_KEYUP As UInteger = &H2

    Public Shared Sub SnapLeft(hWnd As IntPtr)
        PerformSnap(hWnd, VK_LEFT)
    End Sub

    Public Shared Sub SnapRight(hWnd As IntPtr)
        PerformSnap(hWnd, VK_RIGHT)
    End Sub

    Private Shared Sub PerformSnap(hWnd As IntPtr, arrowKey As Byte)
        ' 1. Aducem fereastra în prim-plan și ne asigurăm că e normală (nu minimizată)
        ShowWindow(hWnd, 9) ' SW_RESTORE
        SetForegroundWindow(hWnd)

        ' Mică pauză să prindă focusul
        System.Threading.Thread.Sleep(100)

        ' 2. Apăsăm WIN
        keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero)

        ' 3. Apăsăm Săgeata (Stânga sau Dreapta)
        keybd_event(arrowKey, 0, 0, UIntPtr.Zero)

        ' 4. Ridicăm Săgeata
        keybd_event(arrowKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero)

        ' 5. Ridicăm WIN
        keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero)
    End Sub
End Class