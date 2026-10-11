Option Strict On
Imports System.Runtime.InteropServices

''' <summary>
''' Keeps a modal window in front (slice ME01). A message box that stays open while the operator switches to
''' another program and comes back through the taskbar can end up BEHIND the main window: the main window is
''' disabled by the modal loop, so the program looks frozen. A window of this program calls
''' <see cref="RaiseModal"/> when it is activated (see MainForm.WndProc) and the open modal window is brought
''' back on top, but only when the program itself is the foreground one, so nothing is ever stolen from
''' another program.
''' </summary>
Public Module ModalGuard

    <DllImport("user32.dll")>
    Private Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Function GetWindowThreadProcessId(k_hWnd As IntPtr, ByRef k_processId As Integer) As Integer
    End Function

    ''' <summary>Brings the topmost open modal form to the front, when this program is the foreground one.</summary>
    Public Sub RaiseModal()
        Try
            Dim k_foreground As IntPtr = GetForegroundWindow()
            If k_foreground = IntPtr.Zero Then Return
            Dim k_pid As Integer
            GetWindowThreadProcessId(k_foreground, k_pid)
            If k_pid <> Environment.ProcessId Then Return

            Dim k_modal As Form = Nothing
            For Each k_form As Form In Application.OpenForms
                If k_form.Visible AndAlso k_form.Modal Then k_modal = k_form
            Next
            If k_modal Is Nothing OrElse Form.ActiveForm Is k_modal Then Return
            If k_modal.WindowState = FormWindowState.Minimized Then k_modal.WindowState = FormWindowState.Normal
            k_modal.Activate()
            k_modal.BringToFront()
        Catch ex As Exception
            GlobalErrorLog.Write("ModalGuard.RaiseModal", ex)
        End Try
    End Sub

End Module
