Option Strict On
Imports System.Runtime.InteropServices

''' <summary>
''' Win32 calls the help window needs to tell whose modal dialog disabled it (slice 0000-23): its
''' own (the manual's «Save as», the capture tool) or another window's, which closes it.
''' </summary>
Friend NotInheritable Class HelpWindowNative

    Private Sub New()
    End Sub

    Private Const GW_OWNER As UInteger = 4
    Private Const MaxOwnerDepth As Integer = 16

    Private Delegate Function EnumThreadWndProc(hWnd As IntPtr, lParam As IntPtr) As Boolean

    <DllImport("user32.dll")>
    Private Shared Function EnumThreadWindows(threadId As UInteger, cb As EnumThreadWndProc, lParam As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("kernel32.dll")>
    Private Shared Function GetCurrentThreadId() As UInteger
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetWindow(hWnd As IntPtr, cmd As UInteger) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function IsWindowVisible(hWnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll", EntryPoint:="IsWindowEnabled")>
    Private Shared Function NativeIsWindowEnabled(hWnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    ''' <summary>True when Windows lets <paramref name="hWnd"/> take input (no modal dialog disabled it).</summary>
    Public Shared Function IsEnabled(hWnd As IntPtr) As Boolean
        Return NativeIsWindowEnabled(hWnd)
    End Function

    ''' <summary>
    ''' True when a visible, enabled top-level window of this thread is owned -- directly or through
    ''' its owners -- by <paramref name="owner"/>: a modal dialog opened from that window.
    ''' </summary>
    Public Shared Function OwnsEnabledWindow(owner As IntPtr) As Boolean
        Dim found As Boolean = False
        Dim cb As EnumThreadWndProc =
            Function(h As IntPtr, l As IntPtr) As Boolean
                If h = owner OrElse Not IsWindowVisible(h) OrElse Not NativeIsWindowEnabled(h) Then Return True
                Dim o As IntPtr = GetWindow(h, GW_OWNER)
                Dim depth As Integer = 0
                While o <> IntPtr.Zero AndAlso depth < MaxOwnerDepth
                    If o = owner Then
                        found = True
                        Return False
                    End If
                    o = GetWindow(o, GW_OWNER)
                    depth += 1
                End While
                Return True
            End Function
        EnumThreadWindows(GetCurrentThreadId(), cb, IntPtr.Zero)
        GC.KeepAlive(cb)
        Return found
    End Function

    ''' <summary>
    ''' Slice 0000-32: true when this thread has a visible, enabled top-level window other than
    ''' <paramref name="except"/>. While a dialog is being opened every window of the thread is
    ''' disabled and the dialog itself is not on screen yet, so this is False until it shows.
    ''' </summary>
    Public Shared Function AnyEnabledWindow(except As IntPtr) As Boolean
        Dim found As Boolean = False
        Dim cb As EnumThreadWndProc =
            Function(h As IntPtr, l As IntPtr) As Boolean
                If h = except OrElse Not IsWindowVisible(h) OrElse Not NativeIsWindowEnabled(h) Then Return True
                found = True
                Return False
            End Function
        EnumThreadWindows(GetCurrentThreadId(), cb, IntPtr.Zero)
        GC.KeepAlive(cb)
        Return found
    End Function

    ' ── Slice 0000-24: no Windows 11 frame around a shaped window ─────────────────────

    Private Const DWMWA_WINDOW_CORNER_PREFERENCE As Integer = 33
    Private Const DWMWA_BORDER_COLOR As Integer = 34
    Private Const DWMWCP_DONOTROUND As Integer = 1
    Private Const DWMWA_COLOR_NONE As Integer = &HFFFFFFFE

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(hWnd As IntPtr, attr As Integer, ByRef value As Integer, size As Integer) As Integer
    End Function

    ''' <summary>
    ''' Asks DWM not to round <paramref name="hWnd"/> and not to draw its border. Windows 11 draws
    ''' both around the whole window rectangle, ignoring the window's Region, so a callout (a body
    ''' plus a triangle) would show a grey outline around the strip beside its triangle. Windows 10
    ''' does not know the attributes: the calls fail there with an HRESULT and change nothing.
    ''' </summary>
    Public Shared Sub PlainFrame(hWnd As IntPtr)
        If hWnd = IntPtr.Zero Then Return
        Dim corner As Integer = DWMWCP_DONOTROUND
        DwmSetWindowAttribute(hWnd, DWMWA_WINDOW_CORNER_PREFERENCE, corner, 4)
        Dim border As Integer = DWMWA_COLOR_NONE
        DwmSetWindowAttribute(hWnd, DWMWA_BORDER_COLOR, border, 4)
    End Sub

End Class
