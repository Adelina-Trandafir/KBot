Option Strict On
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

''' <summary>
''' A diagnostic log for the close-into-the-tray path (slice ME01-06): <c>&lt;AppDir&gt;\Logs\minimizare_tray.log</c>. It exists because a picture
''' of the window stayed on the screen after the first close and could not be explained by reading the code. Per close it writes the state
''' of the window before and after each step, the window messages the main window gets while it hides (only the ones that show, hide, move,
''' paint, create or destroy it), and the list of every window of this program (the picture might be a second window: a tooltip, a fly-out)
''' right after, 0.4 s later and 2 s later. Nothing here changes what the window does.
''' </summary>
Public Module TrayLog

    Private ReadOnly _lock As New Object()
    Private _traceUntil As DateTime = DateTime.MinValue
    Private _closeCount As Integer

    Private Delegate Function EnumWindowsProc(k_handle As IntPtr, k_param As IntPtr) As Boolean

    <DllImport("user32.dll")>
    Private Function EnumWindows(k_callback As EnumWindowsProc, k_param As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Function GetWindowThreadProcessId(k_handle As IntPtr, ByRef k_processId As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Function IsWindowVisible(k_handle As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Function GetWindowRect(k_handle As IntPtr, ByRef k_rect As NativeRect) As Boolean
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Function GetClassName(k_handle As IntPtr, k_text As StringBuilder, k_max As Integer) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Function GetWindowText(k_handle As IntPtr, k_text As StringBuilder, k_max As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Function GetWindowLong(k_handle As IntPtr, k_index As Integer) As Integer
    End Function

    <DllImport("dwmapi.dll")>
    Private Function DwmIsCompositionEnabled(ByRef k_enabled As Boolean) As Integer
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure NativeRect
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure WindowPos
        Public Handle As IntPtr
        Public After As IntPtr
        Public X As Integer
        Public Y As Integer
        Public Cx As Integer
        Public Cy As Integer
        Public Flags As Integer
    End Structure

    Private ReadOnly MessageNames As New Dictionary(Of Integer, String) From {
        {&H1, "WM_CREATE"}, {&H2, "WM_DESTROY"}, {&H5, "WM_SIZE"}, {&H6, "WM_ACTIVATE"}, {&HF, "WM_PAINT"}, {&H10, "WM_CLOSE"},
        {&H14, "WM_ERASEBKGND"}, {&H18, "WM_SHOWWINDOW"}, {&H1C, "WM_ACTIVATEAPP"}, {&H46, "WM_WINDOWPOSCHANGING"},
        {&H47, "WM_WINDOWPOSCHANGED"}, {&H7D, "WM_STYLECHANGED"}, {&H82, "WM_NCDESTROY"}, {&H85, "WM_NCPAINT"}, {&H86, "WM_NCACTIVATE"},
        {&H112, "WM_SYSCOMMAND"}, {&H215, "WM_CAPTURECHANGED"}, {&H31E, "WM_DWMCOMPOSITIONCHANGED"}}

    Public ReadOnly Property LogPath As String
        Get
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "minimizare_tray.log")
        End Get
    End Property

    ''' <summary>One line, with the time to the millisecond. Never throws: a diagnostic must not break what it observes.</summary>
    Public Sub Write(k_text As String)
        Try
            SyncLock _lock
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath))
                File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", Globalization.CultureInfo.InvariantCulture) & "  " & k_text & Environment.NewLine, Encoding.UTF8)
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("TrayLog.Write", ex)
        End Try
    End Sub

    ''' <summary>A close into the tray starts: numbers it, logs the state of the form and starts the 2 s window during which messages are traced.</summary>
    Public Function BeginClose(k_form As Form, k_step As String) As Integer
        _closeCount += 1
        _traceUntil = DateTime.UtcNow.AddSeconds(3)
        Dim k_enabled As Boolean
        Try
            DwmIsCompositionEnabled(k_enabled)
        Catch ex As Exception
            GlobalErrorLog.Write("TrayLog.BeginClose", ex)
        End Try
        Write("===== CLOSE #" & _closeCount.ToString(Globalization.CultureInfo.InvariantCulture) & "  dwmComposition=" & k_enabled.ToString() &
              "  screens=" & Screen.AllScreens.Length.ToString(Globalization.CultureInfo.InvariantCulture) & "  os=" & Environment.OSVersion.Version.ToString())
        Snapshot(k_form, k_step)
        Return _closeCount
    End Function

    ''' <summary>The state of the form now.</summary>
    Public Sub Snapshot(k_form As Form, k_step As String)
        Try
            Dim k_handle As String = If(k_form.IsHandleCreated, "0x" & k_form.Handle.ToInt64().ToString("X"), "none")
            Dim k_style As String = If(k_form.IsHandleCreated, "style=0x" & GetWindowLong(k_form.Handle, -16).ToString("X8") & " exstyle=0x" & GetWindowLong(k_form.Handle, -20).ToString("X8"), "")
            Write("[" & k_step & "] handle=" & k_handle & " visible=" & k_form.Visible.ToString() & " state=" & k_form.WindowState.ToString() &
                  " bounds=" & k_form.Bounds.ToString() & " showInTaskbar=" & k_form.ShowInTaskbar.ToString() & " opacity=" & k_form.Opacity.ToString("0.##", Globalization.CultureInfo.InvariantCulture) &
                  " dpi=" & k_form.DeviceDpi.ToString(Globalization.CultureInfo.InvariantCulture) & " active=" & Form.ActiveForm?.Name & " " & k_style)
        Catch ex As Exception
            GlobalErrorLog.Write("TrayLog.Snapshot", ex)
        End Try
    End Sub

    ''' <summary>Every top-level window of this program, and every form WinForms knows of: a leftover picture might be one of THEM.</summary>
    Public Sub SnapshotWindows(k_step As String)
        Try
            Dim k_pid As Integer = Environment.ProcessId
            Dim k_lines As New List(Of String)()
            EnumWindows(Function(k_hwnd, k_param)
                            Dim k_owner As Integer = 0
                            GetWindowThreadProcessId(k_hwnd, k_owner)
                            If k_owner <> k_pid Then Return True
                            Dim k_class As New StringBuilder(128)
                            Dim k_title As New StringBuilder(128)
                            GetClassName(k_hwnd, k_class, k_class.Capacity)
                            GetWindowText(k_hwnd, k_title, k_title.Capacity)
                            Dim k_rect As NativeRect
                            GetWindowRect(k_hwnd, k_rect)
                            k_lines.Add("    hwnd=0x" & k_hwnd.ToInt64().ToString("X") & " visible=" & IsWindowVisible(k_hwnd).ToString() & " class=" & k_class.ToString() &
                                        " title=""" & k_title.ToString() & """ rect=" & k_rect.Left & "," & k_rect.Top & "," & k_rect.Right & "," & k_rect.Bottom)
                            Return True
                        End Function, IntPtr.Zero)
            Write("[" & k_step & "] windows of this program: " & k_lines.Count.ToString(Globalization.CultureInfo.InvariantCulture))
            For Each k_line As String In k_lines
                Write(k_line)
            Next
            Dim k_forms As New List(Of Form)()
            For Each k_form As Form In Application.OpenForms
                k_forms.Add(k_form)
            Next
            Write("[" & k_step & "] open forms: " & k_forms.Count.ToString(Globalization.CultureInfo.InvariantCulture))
            For Each k_form As Form In k_forms
                Write("    " & k_form.GetType().Name & " name=" & k_form.Name & " visible=" & k_form.Visible.ToString() & " bounds=" & k_form.Bounds.ToString() &
                      " handle=" & If(k_form.IsHandleCreated, "0x" & k_form.Handle.ToInt64().ToString("X"), "none"))
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("TrayLog.SnapshotWindows", ex)
        End Try
    End Sub

    ''' <summary>Logs a window message of the main window while a close is being traced (the messages that show, hide, move, paint, create or destroy it).</summary>
    Public Sub Trace(ByRef k_message As Message)
        Try
            If DateTime.UtcNow > _traceUntil Then Return
            Dim k_name As String = Nothing
            If Not MessageNames.TryGetValue(k_message.Msg, k_name) Then Return
            Dim k_text As String = "  msg " & k_name & " w=0x" & k_message.WParam.ToInt64().ToString("X") & " l=0x" & k_message.LParam.ToInt64().ToString("X")
            If (k_message.Msg = &H46 OrElse k_message.Msg = &H47) AndAlso k_message.LParam <> IntPtr.Zero Then
                Dim k_pos As WindowPos = Marshal.PtrToStructure(Of WindowPos)(k_message.LParam)
                k_text &= " pos=" & k_pos.X & "," & k_pos.Y & " " & k_pos.Cx & "x" & k_pos.Cy & " flags=0x" & k_pos.Flags.ToString("X")
            End If
            Write(k_text)
        Catch ex As Exception
            GlobalErrorLog.Write("TrayLog.Trace", ex)
        End Try
    End Sub

End Module
