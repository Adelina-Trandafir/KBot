Option Strict On
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Text

' The Win32 surface the Adobe hosting code needs, in ONE place (slice 0024).
'
' WHY THIS FILE EXISTS: the same eighteen declarations were duplicated in AdobeReaderHarnessForm
' (KBot.DevHarness) and ReaderHostPreview (KBot.App). Two copies of a P/Invoke set is two places to
' get a signature subtly wrong, and the harness copy had already diverged (it strips WS_SYSMENU /
' WS_MINIMIZEBOX / WS_MAXIMIZEBOX, the preview copy did not). One declaration set, one behaviour.
'
' Friend, not Public: nothing outside KBot.Controls should be calling user32 directly — callers go
' through AdobeReaderHost.
Friend NotInheritable Class AdobeNativeMethods

    Private Sub New()
    End Sub

    ' ── Window styles ───────────────────────────────────────────────────────────
    Public Const GWL_STYLE As Integer = -16
    Public Const GWL_EXSTYLE As Integer = -20
    Public Const WS_CHILD As Long = &H40000000L
    Public Const WS_POPUP As Long = &H80000000L
    Public Const WS_CAPTION As Long = &HC00000L
    Public Const WS_THICKFRAME As Long = &H40000L
    Public Const WS_SYSMENU As Long = &H80000L
    Public Const WS_MINIMIZEBOX As Long = &H20000L
    Public Const WS_MAXIMIZEBOX As Long = &H10000L

    ' The styles a top-level window must LOSE to behave as a hosted child.
    Public Const StandaloneStyles As Long =
        WS_CAPTION Or WS_THICKFRAME Or WS_POPUP Or WS_MINIMIZEBOX Or WS_MAXIMIZEBOX Or WS_SYSMENU

    Public Const SWP_NOZORDER As UInteger = &H4UI
    Public Const SWP_NOACTIVATE As UInteger = &H10UI
    Public Const SWP_FRAMECHANGED As UInteger = &H20UI
    Public Const SWP_SHOWWINDOW As UInteger = &H40UI

    Public Const RDW_INVALIDATE As UInteger = &H1UI
    Public Const RDW_UPDATENOW As UInteger = &H100UI
    Public Const RDW_ALLCHILDREN As UInteger = &H80UI
    Public Const RDW_FRAME As UInteger = &H400UI

    Public Const SW_HIDE As Integer = 0
    Public Const SW_SHOW As Integer = 5
    Public Const GW_HWNDNEXT As UInteger = 2UI
    Public Const GW_CHILD As UInteger = 5UI

    ' Detach mode B (slice 0024-03): ask ONE window to close, leaving the process alive for the next
    ' document. Posted, never sent — a foreign UI thread that is busy must not block ours.
    Public Const WM_CLOSE As UInteger = &H10UI

    ' Synthetic click on one of Adobe's own child windows (slice 0024-03).
    ' WHY A CLICK AND NOT A RESIZE: hiding or zero-sizing Adobe's panes from outside does NOT make it
    ' re-lay-out — measured 05.08.2026, the document view stayed inset by 67px. Adobe's own collapse
    ' button sets the width to zero AND reflows the siblings. Letting Adobe do it is the only way to
    ' get its layout to agree with the result.
    Public Const WM_LBUTTONDOWN As UInteger = &H201UI
    Public Const WM_LBUTTONUP As UInteger = &H202UI

    ''' <summary>Packs client-area coordinates into an lParam for the mouse messages.</summary>
    Public Shared Function MakeLParam(x As Integer, y As Integer) As IntPtr
        Return New IntPtr((y << 16) Or (x And &HFFFF))
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer

        Public Function ToRectangle() As Rectangle
            Return New Rectangle(Left, Top, Right - Left, Bottom - Top)
        End Function
    End Structure

    Public Delegate Function EnumWindowsProc(hWnd As IntPtr, lParam As IntPtr) As Boolean

    <DllImport("user32.dll")>
    Public Shared Function EnumWindows(callback As EnumWindowsProc, extra As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function IsWindowVisible(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function IsWindow(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function SetParent(hWndChild As IntPtr, hWndNewParent As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Public Shared Function GetParent(hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function MoveWindow(hWnd As IntPtr, x As Integer, y As Integer,
                                      w As Integer, h As Integer, repaint As Boolean) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As IntPtr,
                                        x As Integer, y As Integer, cx As Integer, cy As Integer,
                                        uFlags As UInteger) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function RedrawWindow(hWnd As IntPtr, lprcUpdate As IntPtr,
                                        hrgnUpdate As IntPtr, flags As UInteger) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function GetWindowThreadProcessId(hWnd As IntPtr, ByRef lpdwProcessId As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Public Shared Function GetWindow(hWnd As IntPtr, uCmd As UInteger) As IntPtr
    End Function

    ' cPoints = 2 when the "points" are the two corners of a RECT. Converts screen -> parent client,
    ' which is the coordinate space SetWindowPos wants for a child window. Mixing the two spaces is
    ' the one way this code produces plausible numbers that mean nothing.
    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function MapWindowPoints(hWndFrom As IntPtr, hWndTo As IntPtr,
                                           ByRef lpPoints As RECT, cPoints As UInteger) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function GetWindowRect(hWnd As IntPtr, ByRef lpRect As RECT) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function SetFocus(hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function PostMessage(hWnd As IntPtr, msg As UInteger,
                                       wParam As IntPtr, lParam As IntPtr) As Boolean
    End Function

    ' ── Creation hook (optional early catch, slice 0024-03 §4) ──────────────────
    Public Const EVENT_OBJECT_CREATE As UInteger = &H8000UI
    Public Const EVENT_OBJECT_SHOW As UInteger = &H8002UI
    ' Out-of-context: the callback runs on OUR thread, so it needs a message pump — install from the
    ' UI thread only. In-context would inject a DLL into Adobe, which is out of the question.
    Public Const WINEVENT_OUTOFCONTEXT As UInteger = &H0UI
    ' The event is about the window itself, not one of its accessibility children.
    Public Const OBJID_WINDOW As Integer = 0

    Public Delegate Sub WinEventProc(hook As IntPtr, eventType As UInteger, hWnd As IntPtr,
                                     idObject As Integer, idChild As Integer,
                                     threadId As UInteger, timestamp As UInteger)

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function SetWinEventHook(eventMin As UInteger, eventMax As UInteger,
                                           hmodWinEventProc As IntPtr, lpfnWinEventProc As WinEventProc,
                                           idProcess As UInteger, idThread As UInteger,
                                           dwFlags As UInteger) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function UnhookWinEvent(hWinEventHook As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function GetClassName(hWnd As IntPtr, lpClassName As StringBuilder, nMaxCount As Integer) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function GetWindowText(hWnd As IntPtr, lpString As StringBuilder, nMaxCount As Integer) As Integer
    End Function

    ' GetWindowLongPtr/SetWindowLongPtr do not exist on 32-bit; pick the right one at run time.
    <DllImport("user32.dll", EntryPoint:="GetWindowLongPtrW")>
    Private Shared Function GetWindowLongPtr64(hWnd As IntPtr, nIndex As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="GetWindowLongW")>
    Private Shared Function GetWindowLong32(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    <DllImport("user32.dll", EntryPoint:="SetWindowLongPtrW")>
    Private Shared Function SetWindowLongPtr64(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="SetWindowLongW")>
    Private Shared Function SetWindowLong32(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    Public Shared Function GetWindowLongPtrSafe(hWnd As IntPtr, nIndex As Integer) As IntPtr
        If IntPtr.Size = 8 Then Return GetWindowLongPtr64(hWnd, nIndex)
        Return New IntPtr(GetWindowLong32(hWnd, nIndex))
    End Function

    Public Shared Function SetWindowLongPtrSafe(hWnd As IntPtr, nIndex As Integer, val As IntPtr) As IntPtr
        If IntPtr.Size = 8 Then Return SetWindowLongPtr64(hWnd, nIndex, val)
        Return New IntPtr(SetWindowLong32(hWnd, nIndex, val.ToInt32()))
    End Function

    Public Shared Function GetClass(hWnd As IntPtr) As String
        Dim sb As New StringBuilder(256)
        GetClassName(hWnd, sb, sb.Capacity)
        Return sb.ToString()
    End Function

    Public Shared Function GetTitle(hWnd As IntPtr) As String
        Dim sb As New StringBuilder(512)
        GetWindowText(hWnd, sb, sb.Capacity)
        Return sb.ToString()
    End Function

    ''' <summary>The owning process id of a window, or 0 when it cannot be read.</summary>
    Public Shared Function OwnerPid(hWnd As IntPtr) As Integer
        Dim pid As Integer = 0
        GetWindowThreadProcessId(hWnd, pid)
        Return pid
    End Function

    ''' <summary>
    ''' A window's rectangle in its PARENT'S client coordinates. GetWindowRect returns SCREEN
    ''' coordinates; the conversion happens once, here.
    ''' </summary>
    Public Shared Function RectInParent(hWnd As IntPtr) As Rectangle
        If hWnd = IntPtr.Zero OrElse Not IsWindow(hWnd) Then Return Rectangle.Empty
        Dim r As RECT
        If Not GetWindowRect(hWnd, r) Then Return Rectangle.Empty
        Dim parent As IntPtr = GetParent(hWnd)
        If parent = IntPtr.Zero Then Return r.ToRectangle()
        MapWindowPoints(IntPtr.Zero, parent, r, 2)
        Return r.ToRectangle()
    End Function

    ''' <summary>A window's rectangle in SCREEN coordinates (Empty when it cannot be read).</summary>
    Public Shared Function RectOnScreen(hWnd As IntPtr) As Rectangle
        If hWnd = IntPtr.Zero OrElse Not IsWindow(hWnd) Then Return Rectangle.Empty
        Dim r As RECT
        If Not GetWindowRect(hWnd, r) Then Return Rectangle.Empty
        Return r.ToRectangle()
    End Function

End Class
