Option Strict On
Imports System.Runtime.InteropServices
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The floating bar of a help capture (slice 0000-02): which picture, what to set up by hand, and
''' what K-BOT could not do on its own. It stays on top, bottom-right of the working area, while the
''' operator uses K-BOT underneath; «Capturează» or «Renunță» ends it (<see cref="Finished"/>).
''' </summary>
Public Class HelpCapturePromptForm

    ''' <summary>Raised once: True = «Capturează», False = «Renunță» / the bar was closed.</summary>
    Public Event Finished(capture As Boolean)

    Private _done As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(capture As HelpCapture, note As String)
        Me.New()
        lblImagine.Text = capture.Caption
        lblPregatire.Text = If(String.IsNullOrWhiteSpace(capture.Prepare),
                               "Nimic de pregătit: ecranul e cel potrivit. Apasă «Capturează» sau Ctrl + `.",
                               capture.Prepare & vbCrLf & "(Meniurile și ferestrele mici deschise rămân deschise cât ține captura; Ctrl + ` face la fel ca «Capturează».)")
        lblNota.Text = If(note, String.Empty)
        lblNota.Visible = Not String.IsNullOrEmpty(note)
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            ' Bottom-right of the screen the cursor is on, out of the way of what gets photographed.
            Dim area As Rectangle = Screen.FromPoint(Cursor.Position).WorkingArea
            Location = New Point(area.Right - Width - 16, area.Bottom - Height - 16)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCapturePromptForm.OnLoad", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p As ThemePalette = ThemeManager.Current.Palette
            BackColor = p.BorderColor
            lblNota.ForeColor = p.WarningColor
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCapturePromptForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' Ctrl + ` : a system-wide hotkey, so a popup menu set up by hand stays open (pressing the button
    ' would take the focus and close it). WM_HOTKEY does not activate this window.
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function RegisterHotKey(hWnd As IntPtr, id As Integer, modifiers As UInteger, vk As UInteger) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function UnregisterHotKey(hWnd As IntPtr, id As Integer) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    Private Const HotkeyId As Integer = &H4B42
    Private Const WM_HOTKEY As Integer = &H312
    Private Const MOD_CONTROL As UInteger = &H2UI
    Private Const MOD_NOREPEAT As UInteger = &H4000UI
    Private Const VK_OEM_3 As UInteger = &HC0UI
    Private _hotkeyOn As Boolean

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Try
            _hotkeyOn = RegisterHotKey(Handle, HotkeyId, MOD_CONTROL Or MOD_NOREPEAT, VK_OEM_3)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCapturePromptForm.OnHandleCreated", ex)
        End Try
    End Sub

    Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
        Try
            If _hotkeyOn Then
                UnregisterHotKey(Handle, HotkeyId)
                _hotkeyOn = False
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCapturePromptForm.OnHandleDestroyed", ex)
        End Try
        MyBase.OnHandleDestroyed(e)
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = WM_HOTKEY AndAlso m.WParam.ToInt32() = HotkeyId Then
            Finish(True)
            Return
        End If
        MyBase.WndProc(m)
    End Sub

    Private Sub BtnCaptureaza_Click(sender As Object, e As EventArgs) Handles btnCaptureaza.Click
        Finish(True)
    End Sub

    Private Sub BtnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Finish(False)
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        Finish(False)   ' the caption bar's close = give up
    End Sub

    Private Sub Finish(capture As Boolean)
        Try
            If _done Then Return
            _done = True
            RaiseEvent Finished(capture)
            If Not IsDisposed Then Close()
        Catch ex As Exception
            ' UI boundary (click / close handlers).
            GlobalErrorLog.Write("HelpCapturePromptForm.Finish", ex)
        End Try
    End Sub

End Class
