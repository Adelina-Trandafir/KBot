Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' P/Invoke pentru bara de titlu (DWM) și tema scrollbar-urilor native (uxtheme).
''' Contract „zero excepții înghițite fără urmă”: pe eșec logăm O SINGURĂ DATĂ prin
''' GlobalErrorLog (guard static), apoi suprimăm — altfel am spama log-ul la fiecare
''' formular ne-tematizat pe Windows vechi unde atributul nu există.
''' Modulul e Public ca să rămână vizibili DOI membri: <see cref="DragMove"/>, cerut de
''' KBotCaptionBar, și <see cref="ApplyWindowTheme"/>, cerut de KBotComboBox — amândouă
''' controale care stau acum în KBot.Controls (toate controalele K-BOT trăiesc acolo).
''' Restul membrilor sunt consumați numai din KBot.Theming și rămân Friend.
''' </summary>
Public Module NativeMethods

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

    <DllImport("user32.dll")>
    Private Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As IntPtr, X As Integer, Y As Integer,
                                  cx As Integer, cy As Integer, uFlags As UInteger) As Boolean
    End Function

    Private Const SWP_NOZORDER As UInteger = &H4
    Private Const SWP_NOACTIVATE As UInteger = &H10

    ' -- The edit box of an editable combo (KBotComboBox.Editable) --------------
    ' A DropDown-style combo creates an EDIT child of its own, with its own HWND. We do not
    ' draw it, but we have to know WHERE it is in order to line its text up with the text we
    ' do draw, on the closed face and on the list rows.
    Private Const EM_SETMARGINS As Integer = &HD3
    Private Const EC_LEFTMARGIN As Integer = 1
    Private Const EC_RIGHTMARGIN As Integer = 2

    ' A single-line EDIT does NOT centre its line vertically: it draws it at the top of its own
    ' client rectangle, so where the glyphs land is decided by the EDIT's TOP alone -- its height
    ' plays no part. Centring the text therefore needs two numbers Windows only gives when asked:
    ' the font's line height (GetTextMetrics on the EDIT's own DC, with the EDIT's own font) and
    ' the EDIT's internal top offset (EM_POSFROMCHAR on character 0).
    Private Const WM_GETFONT As Integer = &H31
    Private Const WM_GETTEXTLENGTH As Integer = &HE
    Private Const EM_POSFROMCHAR As Integer = &HD6

    ' The W variant, so the four character fields marshal as Char and not as Byte. Only tmHeight is
    ' read, but the whole layout still has to be right or the marshaller reads garbage into it.
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Private Structure TEXTMETRICW
        Public tmHeight As Integer
        Public tmAscent As Integer
        Public tmDescent As Integer
        Public tmInternalLeading As Integer
        Public tmExternalLeading As Integer
        Public tmAveCharWidth As Integer
        Public tmMaxCharWidth As Integer
        Public tmWeight As Integer
        Public tmOverhang As Integer
        Public tmDigitizedAspectX As Integer
        Public tmDigitizedAspectY As Integer
        Public tmFirstChar As Char
        Public tmLastChar As Char
        Public tmDefaultChar As Char
        Public tmBreakChar As Char
        Public tmItalic As Byte
        Public tmUnderlined As Byte
        Public tmStruckOut As Byte
        Public tmPitchAndFamily As Byte
        Public tmCharSet As Byte
    End Structure

    <DllImport("user32.dll")>
    Private Function GetDC(hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Function ReleaseDC(hWnd As IntPtr, hDC As IntPtr) As Integer
    End Function

    <DllImport("gdi32.dll")>
    Private Function SelectObject(hdc As IntPtr, hObj As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll", CharSet:=CharSet.Unicode, EntryPoint:="GetTextMetricsW")>
    Private Function GetTextMetrics(hdc As IntPtr, ByRef tm As TEXTMETRICW) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure NativeRect
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure COMBOBOXINFO
        Public cbSize As Integer
        Public rcItem As NativeRect
        Public rcButton As NativeRect
        Public stateButton As Integer
        Public hwndCombo As IntPtr
        Public hwndItem As IntPtr
        Public hwndList As IntPtr
    End Structure

    <DllImport("user32.dll")>
    Private Function GetComboBoxInfo(hWnd As IntPtr, ByRef pcbi As COMBOBOXINFO) As Boolean
    End Function

    <DllImport("gdi32.dll")>
    Private Function SetTextColor(hdc As IntPtr, crColor As Integer) As Integer
    End Function

    <DllImport("gdi32.dll")>
    Private Function SetBkColor(hdc As IntPtr, crColor As Integer) As Integer
    End Function

    <DllImport("gdi32.dll")>
    Private Function CreateSolidBrush(crColor As Integer) As IntPtr
    End Function

    Private _comboInfoLogged As Boolean = False

    ' One brush per colour, kept for the life of the process. A WM_CTLCOLOR* answer must return a
    ' brush the caller does NOT own, so it cannot be deleted after the message; caching it is the
    ' only way to answer without leaking one brush per repaint. The set is bounded by the number of
    ' distinct input colours across the schemes — single digits.
    Private ReadOnly _solidBrushes As New Dictionary(Of Integer, IntPtr)()

    ''' <summary>
    ''' Answers a <c>WM_CTLCOLOR*</c> for a native child that we do not paint (the EDIT of an
    ''' editable combo): puts our colours on its DC and hands back the matching background brush.
    ''' <c>IntPtr.Zero</c> = it could not be done, and the caller must fall back to the default
    ''' handling — which is exactly what leaves a white box on a dark scheme.
    ''' </summary>
    Public Function ApplyControlColors(hdc As IntPtr, back As Color, fore As Color) As IntPtr
        If hdc = IntPtr.Zero Then Return IntPtr.Zero
        Try
            Dim backRef As Integer = ColorTranslator.ToWin32(back)
            SetTextColor(hdc, ColorTranslator.ToWin32(fore))
            SetBkColor(hdc, backRef)

            Dim brush As IntPtr
            If Not _solidBrushes.TryGetValue(backRef, brush) Then
                brush = CreateSolidBrush(backRef)
                If brush = IntPtr.Zero Then Return IntPtr.Zero
                _solidBrushes(backRef) = brush
            End If
            Return brush
        Catch ex As Exception
            If Not _comboInfoLogged Then
                _comboInfoLogged = True
                GlobalErrorLog.Write("NativeMethods.ApplyControlColors", ex)
            End If
            Return IntPtr.Zero
        End Try
    End Function

    ''' <summary>
    ''' The rectangle of an editable combo's edit box, in the combo's own client coordinates.
    ''' <c>Rectangle.Empty</c> = there is none (DropDownList style) or the call failed.
    ''' PUBLIC for the same reason as <see cref="ApplyWindowTheme"/>: KBotComboBox needs it, and
    ''' it lives in KBot.Controls.
    ''' </summary>
    Public Function GetComboEditBounds(combo As Control) As Rectangle
        If combo Is Nothing OrElse Not combo.IsHandleCreated Then Return Rectangle.Empty
        Try
            Dim info As New COMBOBOXINFO()
            info.cbSize = Marshal.SizeOf(Of COMBOBOXINFO)()
            If Not GetComboBoxInfo(combo.Handle, info) Then Return Rectangle.Empty
            If info.hwndItem = IntPtr.Zero Then Return Rectangle.Empty
            Return Rectangle.FromLTRB(info.rcItem.Left, info.rcItem.Top,
                                      info.rcItem.Right, info.rcItem.Bottom)
        Catch ex As Exception
            If Not _comboInfoLogged Then
                _comboInfoLogged = True
                GlobalErrorLog.Write("NativeMethods.GetComboEditBounds", ex)
            End If
            Return Rectangle.Empty
        End Try
    End Function

    ''' <summary>
    ''' The inner margins of an editable combo's edit box, in device pixels. Without them the
    ''' typed text would start ~3 px in, while we draw the closed face and the list rows with a
    ''' different padding — the jog would be visible.
    ''' </summary>
    Public Sub SetComboEditMargins(combo As Control, leftPx As Integer, rightPx As Integer)
        If combo Is Nothing OrElse Not combo.IsHandleCreated Then Return
        Try
            Dim info As New COMBOBOXINFO()
            info.cbSize = Marshal.SizeOf(Of COMBOBOXINFO)()
            If Not GetComboBoxInfo(combo.Handle, info) Then Return
            If info.hwndItem = IntPtr.Zero Then Return
            Dim left As Integer = Math.Max(0, Math.Min(leftPx, Short.MaxValue))
            Dim right As Integer = Math.Max(0, Math.Min(rightPx, Short.MaxValue))
            Dim packed As Integer = (right << 16) Or (left And &HFFFF)
            SendMessage(info.hwndItem, EM_SETMARGINS,
                        New IntPtr(EC_LEFTMARGIN Or EC_RIGHTMARGIN), New IntPtr(packed))
        Catch ex As Exception
            If Not _comboInfoLogged Then
                _comboInfoLogged = True
                GlobalErrorLog.Write("NativeMethods.SetComboEditMargins", ex)
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Moves/resizes the EDIT child of an editable combo, in the combo's own client
    ''' coordinates (the same space <see cref="GetComboEditBounds"/> reads). Backs the vertical
    ''' centring done in <c>KBotComboBox.AlignEditText</c>: a vertical position is not something
    ''' <c>EM_SETMARGINS</c> can express (it is horizontal-only), so the child window has to move.
    ''' </summary>
    Public Sub SetComboEditBounds(combo As Control, bounds As Rectangle)
        If combo Is Nothing OrElse Not combo.IsHandleCreated Then Return
        Try
            Dim info As New COMBOBOXINFO()
            info.cbSize = Marshal.SizeOf(Of COMBOBOXINFO)()
            If Not GetComboBoxInfo(combo.Handle, info) Then Return
            If info.hwndItem = IntPtr.Zero Then Return
            SetWindowPos(info.hwndItem, IntPtr.Zero, bounds.Left, bounds.Top, bounds.Width, bounds.Height,
                        SWP_NOZORDER Or SWP_NOACTIVATE)
        Catch ex As Exception
            If Not _comboInfoLogged Then
                _comboInfoLogged = True
                GlobalErrorLog.Write("NativeMethods.SetComboEditBounds", ex)
            End If
        End Try
    End Sub

    ''' <summary>
    ''' The line height of an editable combo's EDIT child, in DEVICE pixels -- measured on the
    ''' EDIT's own DC with the EDIT's own font, which is the only number that says how tall the
    ''' text really is. <c>0</c> = there is no EDIT (DropDownList style), or the call failed.
    ''' </summary>
    Public Function GetComboEditLineHeight(combo As Control) As Integer
        If combo Is Nothing OrElse Not combo.IsHandleCreated Then Return 0
        Try
            Dim info As New COMBOBOXINFO()
            info.cbSize = Marshal.SizeOf(Of COMBOBOXINFO)()
            If Not GetComboBoxInfo(combo.Handle, info) Then Return 0
            If info.hwndItem = IntPtr.Zero Then Return 0

            ' IntPtr.Zero is NOT an error here: it means the EDIT uses the stock system font, which
            ' is already the DC's default -- in that case there is simply nothing to select in.
            Dim hFont As IntPtr = SendMessage(info.hwndItem, WM_GETFONT, IntPtr.Zero, IntPtr.Zero)

            Dim hdc As IntPtr = GetDC(info.hwndItem)
            If hdc = IntPtr.Zero Then Return 0
            Dim oldFont As IntPtr = IntPtr.Zero
            Try
                If hFont <> IntPtr.Zero Then oldFont = SelectObject(hdc, hFont)
                Dim tm As New TEXTMETRICW()
                If Not GetTextMetrics(hdc, tm) Then Return 0
                Return tm.tmHeight
            Finally
                If oldFont <> IntPtr.Zero Then SelectObject(hdc, oldFont)
                ReleaseDC(info.hwndItem, hdc)
            End Try
        Catch ex As Exception
            If Not _comboInfoLogged Then
                _comboInfoLogged = True
                GlobalErrorLog.Write("NativeMethods.GetComboEditLineHeight", ex)
            End If
            Return 0
        End Try
    End Function

    ''' <summary>
    ''' The y of character 0 inside the EDIT's OWN client area -- the internal top offset the EDIT
    ''' adds on top of its window position (normally 0 or 1 px). <c>Integer.MinValue</c> = it could
    ''' not be measured (no EDIT, an empty box, or <c>EM_POSFROMCHAR</c> refused); that is the
    ''' sentinel, because <c>0</c> is a perfectly legal answer.
    ''' </summary>
    Public Function GetComboEditTextTop(combo As Control) As Integer
        If combo Is Nothing OrElse Not combo.IsHandleCreated Then Return Integer.MinValue
        Try
            Dim info As New COMBOBOXINFO()
            info.cbSize = Marshal.SizeOf(Of COMBOBOXINFO)()
            If Not GetComboBoxInfo(combo.Handle, info) Then Return Integer.MinValue
            If info.hwndItem = IntPtr.Zero Then Return Integer.MinValue

            ' No character in the box, nothing to ask about.
            If SendMessage(info.hwndItem, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero) = IntPtr.Zero Then
                Return Integer.MinValue
            End If

            Dim r As IntPtr = SendMessage(info.hwndItem, EM_POSFROMCHAR, New IntPtr(0), IntPtr.Zero)
            ' ToInt32 would overflow on x64 -- take the low 32 bits explicitly.
            Dim v As Integer = CInt(r.ToInt64() And &HFFFFFFFFL)
            If v = -1 Then Return Integer.MinValue

            ' HIWORD, signed.
            Dim y As Integer = (v >> 16) And &HFFFF
            If y > &H7FFF Then y -= &H10000
            Return y
        Catch ex As Exception
            If Not _comboInfoLogged Then
                _comboInfoLogged = True
                GlobalErrorLog.Write("NativeMethods.GetComboEditTextTop", ex)
            End If
            Return Integer.MinValue
        End Try
    End Function

    ' Guard-uri „loghează o singură dată” — pe versiuni de OS nesuportate eșecul e
    ' cronic și previzibil; nu vrem un log per formular.
    Private _dwmLogged As Boolean = False
    Private _uxLogged As Boolean = False
    Private _cornerLogged As Boolean = False

    ''' <summary>Setează bara de titlu dark/light pentru un formular.</summary>
    Friend Sub SetTitleBarDark(f As Form, dark As Boolean)
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
    Friend Sub SetRoundedCorners(f As Form, rounded As Boolean)
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
    Friend Sub ApplyMinMaxInfo(lParam As IntPtr, f As Form)
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
    '''
    ''' PUBLIC de la felia 0028, al doilea membru vizibil după <see cref="DragMove"/>, din același
    ''' motiv: <c>KBotComboBox</c> trăiește în KBot.Controls și are nevoie de el pentru fereastra
    ''' NATIVĂ de listă derulantă — acel HWND nu e al controlului, deci nici pictura proprie, nici
    ''' traversarea temei nu ajung la el.
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
