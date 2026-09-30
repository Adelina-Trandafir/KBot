Option Strict On
Imports System.Drawing
Imports System.Runtime.InteropServices

''' <summary>Win32 calls the help capture overlay needs to know which window is where (slice 0000-02).</summary>
Friend NotInheritable Class HelpCaptureNative

    Private Sub New()
    End Sub

    <StructLayout(LayoutKind.Sequential)>
    Private Structure NativeRect
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure NativePoint
        Public X As Integer
        Public Y As Integer
    End Structure

    Private Delegate Function EnumWindowsProc(hWnd As IntPtr, lParam As IntPtr) As Boolean

    <DllImport("user32.dll")>
    Private Shared Function EnumWindows(cb As EnumWindowsProc, lParam As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function IsWindowVisible(hWnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function IsIconic(hWnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetWindowRect(hWnd As IntPtr, ByRef r As NativeRect) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ScreenToClient(hWnd As IntPtr, ByRef p As NativePoint) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ChildWindowFromPointEx(parent As IntPtr, p As NativePoint, flags As UInteger) As IntPtr
    End Function

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmGetWindowAttribute(hWnd As IntPtr, attr As Integer, ByRef r As NativeRect, size As Integer) As Integer
    End Function

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmGetWindowAttribute(hWnd As IntPtr, attr As Integer, ByRef value As Integer, size As Integer) As Integer
    End Function

    Private Const DWMWA_CLOAKED As Integer = 14
    Private Const DWMWA_EXTENDED_FRAME_BOUNDS As Integer = 9
    Private Const CWP_SKIPINVISIBLE As UInteger = 1
    Private Const CWP_SKIPTRANSPARENT As UInteger = 4

    ''' <summary>A visible top-level window: its handle and its bounds on screen (no drop shadow).</summary>
    Friend Structure WindowBox
        Public Handle As IntPtr
        Public Bounds As Rectangle
    End Structure

    ''' <summary>
    ''' Visible, not minimized, not cloaked top-level windows, topmost first (EnumWindows order),
    ''' except the handles in <paramref name="skip"/>.
    ''' </summary>
    Friend Shared Function TopLevelWindows(skip As ICollection(Of IntPtr)) As List(Of WindowBox)
        Dim result As New List(Of WindowBox)()
        EnumWindows(Function(h, l)
                        If skip.Contains(h) OrElse Not IsWindowVisible(h) OrElse IsIconic(h) Then Return True
                        Dim cloaked As Integer = 0
                        DwmGetWindowAttribute(h, DWMWA_CLOAKED, cloaked, 4)
                        If cloaked <> 0 Then Return True
                        Dim b As Rectangle = FrameBounds(h)
                        If b.Width > 1 AndAlso b.Height > 1 Then result.Add(New WindowBox With {.Handle = h, .Bounds = b})
                        Return True
                    End Function, IntPtr.Zero)
        Return result
    End Function

    ''' <summary>The window's visible frame (DWM), falling back to GetWindowRect.</summary>
    Friend Shared Function FrameBounds(h As IntPtr) As Rectangle
        Dim r As NativeRect
        If DwmGetWindowAttribute(h, DWMWA_EXTENDED_FRAME_BOUNDS, r, Marshal.SizeOf(Of NativeRect)()) <> 0 Then
            If Not GetWindowRect(h, r) Then Return Rectangle.Empty
        End If
        Return Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom)
    End Function

    ''' <summary>The deepest visible child window of <paramref name="top"/> under a screen point, bounds on screen.</summary>
    Friend Shared Function DeepestChildBounds(top As IntPtr, screenPoint As Point) As Rectangle
        Dim current As IntPtr = top
        For depth As Integer = 0 To 64
            Dim p As New NativePoint With {.X = screenPoint.X, .Y = screenPoint.Y}
            ScreenToClient(current, p)
            Dim child As IntPtr = ChildWindowFromPointEx(current, p, CWP_SKIPINVISIBLE Or CWP_SKIPTRANSPARENT)
            If child = IntPtr.Zero OrElse child = current Then Exit For
            current = child
        Next
        Dim r As NativeRect
        If Not GetWindowRect(current, r) Then Return Rectangle.Empty
        Return Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom)
    End Function

End Class
