Option Strict On
Imports System.Collections.Generic

''' <summary>
''' The window operations the Adobe capture and teardown code performs, behind an interface so both
''' can run WITHOUT a real Adobe, a real window or even a message pump.
'''
''' WHY THIS EXISTS (slice 0024-03). The two defects this pass fixes — the window that survives
''' teardown and keeps its taskbar button, and the window seen on screen before it is embedded —
''' are both defects of ORDER: which call happens before which, and which call must never happen at
''' all. Neither is observable from a unit test while the calls go straight to user32. With the seam
''' the whole sequence is recorded and asserted, including the negative assertions that matter most:
''' teardown must never call <see cref="SetParent"/> back to the original parent, and never restore
''' the original style. Those two calls WERE the taskbar defect.
'''
''' Deliberately NOT a general user32 wrapper. It carries exactly the calls the capture/teardown
''' path makes and nothing else; <see cref="AdobeNativeMethods"/> stays the single P/Invoke set, and
''' <see cref="Win32Windows"/> is the only implementation that reaches it.
''' </summary>
Public Interface INativeWindows

    ''' <summary>
    ''' Every top-level window, newest first is NOT guaranteed — the caller filters. Returned as a
    ''' list rather than a callback so a fake can hand back a fixed set without an enumeration
    ''' contract to imitate.
    ''' </summary>
    Function EnumTopLevelWindows() As IReadOnlyList(Of IntPtr)

    ''' <summary>The process that owns a window, or 0 when it cannot be read.</summary>
    Function OwnerPid(hwnd As IntPtr) As Integer

    Function GetClass(hwnd As IntPtr) As String
    Function GetTitle(hwnd As IntPtr) As String
    Function IsWindow(hwnd As IntPtr) As Boolean
    Function IsWindowVisible(hwnd As IntPtr) As Boolean

    Sub ShowWindow(hwnd As IntPtr, command As Integer)
    Function GetParent(hwnd As IntPtr) As IntPtr
    Function SetParent(child As IntPtr, newParent As IntPtr) As IntPtr

    Function GetWindowLongPtr(hwnd As IntPtr, index As Integer) As IntPtr
    Function SetWindowLongPtr(hwnd As IntPtr, index As Integer, value As IntPtr) As IntPtr

    Sub MoveWindow(hwnd As IntPtr, x As Integer, y As Integer, w As Integer, h As Integer, repaint As Boolean)
    Sub SetWindowPos(hwnd As IntPtr, insertAfter As IntPtr, x As Integer, y As Integer,
                     cx As Integer, cy As Integer, flags As UInteger)

    ''' <summary>Posts (never sends) a message. Used for WM_CLOSE in detach mode B.</summary>
    Function PostMessage(hwnd As IntPtr, msg As UInteger, wParam As IntPtr, lParam As IntPtr) As Boolean

End Interface

''' <summary>
''' The real implementation: a straight delegation to <see cref="AdobeNativeMethods"/>. Stateless,
''' so <see cref="Instance"/> is shared by every caller.
''' </summary>
Public NotInheritable Class Win32Windows
    Implements INativeWindows

    Private Shared ReadOnly _instance As New Win32Windows()

    ''' <summary>The shared instance — this class holds no state.</summary>
    Public Shared ReadOnly Property Instance As INativeWindows
        Get
            Return _instance
        End Get
    End Property

    Public Function EnumTopLevelWindows() As IReadOnlyList(Of IntPtr) Implements INativeWindows.EnumTopLevelWindows
        Dim all As New List(Of IntPtr)()
        ' The callback cannot throw across the interop boundary, so it only ever appends.
        AdobeNativeMethods.EnumWindows(
            Function(h, l)
                all.Add(h)
                Return True
            End Function, IntPtr.Zero)
        Return all
    End Function

    Public Function OwnerPid(hwnd As IntPtr) As Integer Implements INativeWindows.OwnerPid
        Return AdobeNativeMethods.OwnerPid(hwnd)
    End Function

    Public Function GetClass(hwnd As IntPtr) As String Implements INativeWindows.GetClass
        Return AdobeNativeMethods.GetClass(hwnd)
    End Function

    Public Function GetTitle(hwnd As IntPtr) As String Implements INativeWindows.GetTitle
        Return AdobeNativeMethods.GetTitle(hwnd)
    End Function

    Public Function IsWindow(hwnd As IntPtr) As Boolean Implements INativeWindows.IsWindow
        Return AdobeNativeMethods.IsWindow(hwnd)
    End Function

    Public Function IsWindowVisible(hwnd As IntPtr) As Boolean Implements INativeWindows.IsWindowVisible
        Return AdobeNativeMethods.IsWindowVisible(hwnd)
    End Function

    Public Sub ShowWindow(hwnd As IntPtr, command As Integer) Implements INativeWindows.ShowWindow
        AdobeNativeMethods.ShowWindow(hwnd, command)
    End Sub

    Public Function GetParent(hwnd As IntPtr) As IntPtr Implements INativeWindows.GetParent
        Return AdobeNativeMethods.GetParent(hwnd)
    End Function

    Public Function SetParent(child As IntPtr, newParent As IntPtr) As IntPtr Implements INativeWindows.SetParent
        Return AdobeNativeMethods.SetParent(child, newParent)
    End Function

    Public Function GetWindowLongPtr(hwnd As IntPtr, index As Integer) As IntPtr Implements INativeWindows.GetWindowLongPtr
        Return AdobeNativeMethods.GetWindowLongPtrSafe(hwnd, index)
    End Function

    Public Function SetWindowLongPtr(hwnd As IntPtr, index As Integer, value As IntPtr) As IntPtr Implements INativeWindows.SetWindowLongPtr
        Return AdobeNativeMethods.SetWindowLongPtrSafe(hwnd, index, value)
    End Function

    Public Sub MoveWindow(hwnd As IntPtr, x As Integer, y As Integer, w As Integer, h As Integer,
                          repaint As Boolean) Implements INativeWindows.MoveWindow
        AdobeNativeMethods.MoveWindow(hwnd, x, y, w, h, repaint)
    End Sub

    Public Sub SetWindowPos(hwnd As IntPtr, insertAfter As IntPtr, x As Integer, y As Integer,
                            cx As Integer, cy As Integer, flags As UInteger) Implements INativeWindows.SetWindowPos
        AdobeNativeMethods.SetWindowPos(hwnd, insertAfter, x, y, cx, cy, flags)
    End Sub

    Public Function PostMessage(hwnd As IntPtr, msg As UInteger, wParam As IntPtr,
                                lParam As IntPtr) As Boolean Implements INativeWindows.PostMessage
        Return AdobeNativeMethods.PostMessage(hwnd, msg, wParam, lParam)
    End Function

End Class
