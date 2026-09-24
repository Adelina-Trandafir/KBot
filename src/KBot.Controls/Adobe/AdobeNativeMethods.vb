Option Strict On
Imports System.Collections.Generic
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
    ' A window's OWN visibility bit, as opposed to IsWindowVisible, which is only true when every
    ' ancestor is showing too. See AdobeNativeMethods.IsVisibleStyleSet.
    Public Const WS_VISIBLE As Long = &H10000000L

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
    ''' <summary>Presses a button as a click would (the button then notifies its own parent).</summary>
    Public Const BM_CLICK As UInteger = &HF5UI
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

    ' Sent by Windows to tell a window that its APPLICATION became active or inactive. A hosted
    ' window never gets it again — its top-level ancestor now belongs to us — and Office decides
    ' from it whether to accept mouse buttons at all. See OfficeDocumentHost.PulseActivation.
    Public Const WM_ACTIVATEAPP As UInteger = &H1CUI

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

    ' The CLIENT rectangle: always at 0,0, so only its size means anything. Needed to fill a
    ' foreign window's own children -- GetWindowRect would include whatever border the window has,
    ' and a child placed against that is a few pixels wrong on every edge.
    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function GetClientRect(hWnd As IntPtr, ByRef lpRect As RECT) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function SetFocus(hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Public Shared Function GetForegroundWindow() As IntPtr
    End Function

    ' BM_CLICK on a button of a dialog that is NOT active may do nothing (documented for BM_CLICK);
    ' the dialog is activated first. Allowed only while our process owns the foreground.
    <DllImport("user32.dll")>
    Public Shared Function SetForegroundWindow(hWnd As IntPtr) As Boolean
    End Function

    ''' <summary>Notification code a button sends its dialog in WM_COMMAND when clicked.</summary>
    Public Const BN_CLICKED As Integer = 0

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

    ' ── Read mode (Ctrl+H) path of the ActiveX surface ─────────────────────────
    Public Const EVENT_OBJECT_STATECHANGE As UInteger = &H800AUI
    Public Const EVENT_OBJECT_LOCATIONCHANGE As UInteger = &H800BUI

    <DllImport("user32.dll")>
    Public Shared Function IsChild(hWndParent As IntPtr, hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function IsWindowEnabled(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function GetFocus() As IntPtr
    End Function

    Public Const INPUT_KEYBOARD As Integer = 1
    Public Const KEYEVENTF_KEYUP As UInteger = &H2UI
    Public Const VK_CONTROL As UShort = &H11US

    <StructLayout(LayoutKind.Sequential)>
    Public Structure MOUSEINPUT
        Public dx As Integer
        Public dy As Integer
        Public mouseData As UInteger
        Public dwFlags As UInteger
        Public time As UInteger
        Public dwExtraInfo As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure KEYBDINPUT
        Public wVk As UShort
        Public wScan As UShort
        Public dwFlags As UInteger
        Public time As UInteger
        Public dwExtraInfo As IntPtr
    End Structure

    ' The union is sized by its largest member (MOUSEINPUT), so INPUT has the size SendInput
    ' checks on both 32 and 64 bit.
    <StructLayout(LayoutKind.Explicit)>
    Public Structure InputUnion
        <FieldOffset(0)> Public mi As MOUSEINPUT
        <FieldOffset(0)> Public ki As KEYBDINPUT
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure INPUT
        Public type As Integer
        Public u As InputUnion
    End Structure

    <DllImport("user32.dll", SetLastError:=True)>
    Public Shared Function SendInput(nInputs As UInteger, pInputs As INPUT(), cbSize As Integer) As UInteger
    End Function

    ''' <summary>One key event for <see cref="SendInput"/>.</summary>
    Public Shared Function KeyInput(vk As UShort, up As Boolean) As INPUT
        Dim i As New INPUT With {.type = INPUT_KEYBOARD}
        i.u.ki.wVk = vk
        i.u.ki.dwFlags = If(up, KEYEVENTF_KEYUP, 0UI)
        Return i
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

    ' ── Save As trap (slice 0078-02) ────────────────────────────────────────────
    ' The owner of a top-level window (a dialog's owner is the window it is modal to).
    Public Const GW_OWNER As UInteger = 4UI
    Public Const SWP_NOSIZE As UInteger = &H1UI
    Public Const WM_SETTEXT As UInteger = &HCUI
    Public Const WM_GETTEXT As UInteger = &HDUI
    Public Const WM_COMMAND As UInteger = &H111UI
    ' TaskDialog-only message: presses one of its buttons by id. The Vista-style «Confirm Save As»
    ' prompt is a TaskDialog, whose buttons are not child windows and cannot be clicked any other way.
    Public Const TDM_CLICK_BUTTON As UInteger = &H466UI
    Public Const IDOK As Integer = 1
    Public Const IDCANCEL As Integer = 2
    Public Const IDYES As Integer = 6
    ' SendMessageTimeout: return even if the foreign thread is hung, never block our UI for long.
    Public Const SMTO_ABORTIFHUNG As UInteger = &H2UI

    <DllImport("user32.dll")>
    Public Shared Function EnumChildWindows(hWndParent As IntPtr, callback As EnumWindowsProc,
                                            extra As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function GetDlgCtrlID(hWnd As IntPtr) As Integer
    End Function

    ' WM_SETTEXT with a string: the system marshals the string across the process boundary.
    <DllImport("user32.dll", CharSet:=CharSet.Unicode, EntryPoint:="SendMessageTimeoutW", SetLastError:=True)>
    Public Shared Function SendMessageTimeoutText(hWnd As IntPtr, msg As UInteger, wParam As IntPtr,
                                                  lParam As String, flags As UInteger, timeoutMs As UInteger,
                                                  ByRef result As IntPtr) As IntPtr
    End Function

    ' WM_GETTEXT into a buffer: also marshalled by the system.
    <DllImport("user32.dll", CharSet:=CharSet.Unicode, EntryPoint:="SendMessageTimeoutW", SetLastError:=True)>
    Public Shared Function SendMessageTimeoutBuffer(hWnd As IntPtr, msg As UInteger, wParam As IntPtr,
                                                    lParam As StringBuilder, flags As UInteger, timeoutMs As UInteger,
                                                    ByRef result As IntPtr) As IntPtr
    End Function

    ' A plain message with a timeout: SendMessage semantics, but a hung Adobe cannot freeze K-BOT.
    <DllImport("user32.dll", EntryPoint:="SendMessageTimeoutW", SetLastError:=True)>
    Private Shared Function SendMessageTimeoutPtr(hWnd As IntPtr, msg As UInteger, wParam As IntPtr,
                                                  lParam As IntPtr, flags As UInteger, timeoutMs As UInteger,
                                                  ByRef result As IntPtr) As IntPtr
    End Function

    ''' <summary>SendMessage with a 2 s timeout (abort if hung). True when the window processed it.</summary>
    Public Shared Function SendWithTimeout(hWnd As IntPtr, msg As UInteger, wParam As IntPtr, lParam As IntPtr) As Boolean
        If hWnd = IntPtr.Zero Then Return False
        Dim result As IntPtr
        Return SendMessageTimeoutPtr(hWnd, msg, wParam, lParam, SMTO_ABORTIFHUNG, 2000UI, result) <> IntPtr.Zero
    End Function

    ''' <summary>The window's text read with a timeout (works across processes); "" on failure.</summary>
    Public Shared Function ReadText(hWnd As IntPtr) As String
        If hWnd = IntPtr.Zero Then Return ""
        Dim sb As New StringBuilder(1024)
        Dim result As IntPtr
        Dim ok As IntPtr = SendMessageTimeoutBuffer(hWnd, WM_GETTEXT, New IntPtr(sb.Capacity), sb,
                                                    SMTO_ABORTIFHUNG, 2000UI, result)
        If ok = IntPtr.Zero Then Return ""
        Return sb.ToString()
    End Function

    ''' <summary>Sets a window's text with a timeout (works across processes). True on delivery.</summary>
    Public Shared Function WriteText(hWnd As IntPtr, text As String) As Boolean
        If hWnd = IntPtr.Zero Then Return False
        Dim result As IntPtr
        Return SendMessageTimeoutText(hWnd, WM_SETTEXT, IntPtr.Zero, text,
                                      SMTO_ABORTIFHUNG, 2000UI, result) <> IntPtr.Zero
    End Function

    ''' <summary>Every descendant of <paramref name="hWnd"/> (EnumChildWindows walks the whole tree).</summary>
    Public Shared Function Descendants(hWnd As IntPtr) As List(Of IntPtr)
        Dim all As New List(Of IntPtr)()
        If hWnd = IntPtr.Zero Then Return all
        EnumChildWindows(hWnd, Function(h, l)
                                   all.Add(h)
                                   Return True
                               End Function, IntPtr.Zero)
        Return all
    End Function

    ''' <summary>
    ''' Whether a window carries <c>WS_VISIBLE</c> ITSELF, without asking about its ancestors.
    '''
    ''' <para><b>This is not the same question as <see cref="IsWindowVisible"/>, and the difference
    ''' matters for every window we have reparented.</b> IsWindowVisible walks the whole parent chain
    ''' and answers False if ANY link in it is hidden. Once an Office frame is a child of one of our
    ''' panels, that chain runs through our own controls — so a panel that is momentarily not showing
    ''' (a pane displaying «Se deschide documentul…» while the document opens) makes every window
    ''' inside the hosted frame report itself invisible, including the ones that are perfectly real
    ''' and about to be on screen.</para>
    '''
    ''' <para>Use this wherever the question is «did Office lay this window out, or is it a scrap it
    ''' has parked», and keep IsWindowVisible for top-level windows, where the two agree.</para>
    ''' </summary>
    Public Shared Function IsVisibleStyleSet(hWnd As IntPtr) As Boolean
        If hWnd = IntPtr.Zero Then Return False
        Return (GetWindowLongPtrSafe(hWnd, GWL_STYLE).ToInt64() And WS_VISIBLE) <> 0
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

    Private Const PROCESS_QUERY_LIMITED_INFORMATION As Integer = &H1000

    <StructLayout(LayoutKind.Sequential)>
    Private Structure PROCESS_BASIC_INFORMATION
        Public ExitStatus As IntPtr
        Public PebBaseAddress As IntPtr
        Public AffinityMask As IntPtr
        Public BasePriority As IntPtr
        Public UniqueProcessId As IntPtr
        Public InheritedFromUniqueProcessId As IntPtr
    End Structure

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function OpenProcess(access As Integer, inherit As Boolean, pid As Integer) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function CloseHandle(h As IntPtr) As Boolean
    End Function

    <DllImport("ntdll.dll")>
    Private Shared Function NtQueryInformationProcess(h As IntPtr, infoClass As Integer,
                                                      ByRef info As PROCESS_BASIC_INFORMATION,
                                                      size As Integer, ByRef returned As Integer) As Integer
    End Function

    ''' <summary>
    ''' The parent process id of <paramref name="pid"/>, or 0 when it cannot be read (process gone,
    ''' no access). Slice 0078: Acrobat serves an embedded document with a BROKER (started by
    ''' K-BOT, owns the Save As dialog) and a RENDERER child (owns the view windows).
    ''' </summary>
    Public Shared Function ParentPid(pid As Integer) As Integer
        Dim h As IntPtr = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, False, pid)
        If h = IntPtr.Zero Then Return 0
        Try
            Dim info As New PROCESS_BASIC_INFORMATION()
            Dim returned As Integer
            If NtQueryInformationProcess(h, 0, info, Marshal.SizeOf(info), returned) <> 0 Then Return 0
            Return CInt(info.InheritedFromUniqueProcessId.ToInt64())
        Finally
            CloseHandle(h)
        End Try
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

    ''' <summary>The size of a window's client area (Empty when it cannot be read).</summary>
    Public Shared Function ClientSize(hWnd As IntPtr) As Size
        If hWnd = IntPtr.Zero OrElse Not IsWindow(hWnd) Then Return Size.Empty
        Dim r As RECT
        If Not GetClientRect(hWnd, r) Then Return Size.Empty
        Return New Size(r.Right - r.Left, r.Bottom - r.Top)
    End Function

    ''' <summary>A window's rectangle in SCREEN coordinates (Empty when it cannot be read).</summary>
    Public Shared Function RectOnScreen(hWnd As IntPtr) As Rectangle
        If hWnd = IntPtr.Zero OrElse Not IsWindow(hWnd) Then Return Rectangle.Empty
        Dim r As RECT
        If Not GetWindowRect(hWnd, r) Then Return Rectangle.Empty
        Return r.ToRectangle()
    End Function

End Class
