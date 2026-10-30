Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The tray's own sign-in window, for a director who does not tick «Ține minte parola» in K-BOT. The pair given here is kept
''' by the caller in this process only (<see cref="SessionCredentials.Remember"/>, encrypted with DPAPI): nothing is written
''' to the disk or the registry, and the next start of Windows asks again.
''' </summary>
Friend Class TrayLoginForm

    Friend Sub New(k_user As String, k_error As String)
        InitializeComponent()
        txtUser.Text = If(k_user, String.Empty)
        lblError.Text = If(k_error, String.Empty)
    End Sub

    ''' <summary>The e-mail typed (valid when the window closed with OK).</summary>
    Friend ReadOnly Property UserName As String
        Get
            Return txtUser.Text.Trim()
        End Get
    End Property

    ''' <summary>The password typed (valid when the window closed with OK). The caller clears nothing: the window is disposed.</summary>
    Friend ReadOnly Property Password As String
        Get
            Return txtPass.Text
        End Get
    End Property

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        If txtUser.Text.Length = 0 Then txtUser.Select() Else txtPass.Select()
        Activate()
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            lblError.ForeColor = k_scheme.Palette.ErrorColor
            lblInfo.ForeColor = k_scheme.Palette.TextDimColor
            ButtonStyles.ApplyPrimary(btnLogin, k_scheme)
            ButtonStyles.ApplySecondary(btnCancel, k_scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("TrayLoginForm.OnThemeChanged", ex)
        End Try
    End Sub

    Private Sub BtnLogin_Click(k_sender As Object, e As EventArgs) Handles btnLogin.Click
        Try
            If UserName.Length = 0 OrElse txtPass.Text.Length = 0 Then
                lblError.Text = "Scrieți e-mailul și parola."
                Return
            End If
            DialogResult = DialogResult.OK
        Catch ex As Exception
            GlobalErrorLog.Write("TrayLoginForm.BtnLogin_Click", ex)
        End Try
    End Sub

    Private Sub BtnCancel_Click(k_sender As Object, e As EventArgs) Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
    End Sub
End Class
