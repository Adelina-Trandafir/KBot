Option Strict On
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The tray's own sign-in window, for a director who does not tick «Ține minte parola» in K-BOT. The pair given here is kept
''' by the caller in this process only (<see cref="SessionCredentials.Remember"/>, encrypted with DPAPI): nothing is written
''' to the disk or the registry, and the next start of Windows asks again.
'''
''' <para>The pair is checked with the server BEFORE the window closes: a wrong password (or an account with no unit) is shown in
''' the window, in the server's own words, and the window stays open for another try -- as K-BOT's sign-in window does.</para>
''' </summary>
Friend Class TrayLoginForm

    Private ReadOnly _auth As AuthApi
    Private _checking As Boolean
    Private _cancelled As Boolean

    Friend Sub New(k_user As String, k_error As String, k_auth As AuthApi)
        ArgumentNullException.ThrowIfNull(k_auth)
        InitializeComponent()
        _auth = k_auth
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

    ' UI boundary (async Sub): logs and shows the problem in the window; it never throws.
    Private Async Sub BtnLogin_Click(k_sender As Object, e As EventArgs) Handles btnLogin.Click
        If _checking Then Return
        Try
            If UserName.Length = 0 OrElse txtPass.Text.Length = 0 Then
                lblError.Text = "Scrieți e-mailul și parola."
                Return
            End If

            _checking = True
            SetChecking(True)
            lblError.Text = String.Empty
            Dim k_outcome As UnitsOutcome = Await _auth.CheckAccessAsync(UserName, txtPass.Text, CancellationToken.None)
            If _cancelled OrElse IsDisposed Then Return

            If k_outcome.PasswordChangeRequired Then
                TrayLog.Write("signin window", "the account must change its password first")
                lblError.Text = "Parola trebuie schimbată înainte de autentificare. Contactați administratorul."
                Return
            End If
            If k_outcome.Units Is Nothing OrElse k_outcome.Units.Count = 0 Then
                TrayLog.Write("signin window", "the account has no unit")
                lblError.Text = "Nu aveți nicio unitate accesibilă."
                Return
            End If
            TrayLog.Write("signin window", "the server accepted the pair")
            DialogResult = DialogResult.OK
        Catch ex As ApiException
            TrayLog.Write("signin window", "the server refused the pair: HTTP " & ex.StatusCode.GetValueOrDefault().ToString() & ", " & ex.Message)
            If _cancelled OrElse IsDisposed Then Return
            lblError.Text = ex.Message
            txtPass.Clear()
            txtPass.Select()
        Catch ex As Exception
            GlobalErrorLog.Write("TrayLoginForm.BtnLogin_Click", ex)
            If _cancelled OrElse IsDisposed Then Return
            lblError.Text = "Serverul nu a putut fi contactat. Verificați conexiunea la internet și încercați din nou."
        Finally
            _checking = False
            If Not IsDisposed Then SetChecking(False)
        End Try
    End Sub

    ' While the server is asked the fields and the button are frozen; the Cancel button stays live.
    Private Sub SetChecking(k_busy As Boolean)
        btnLogin.Enabled = Not k_busy
        btnLogin.Text = If(k_busy, "Se verifică...", "Conectare")
        txtUser.ReadOnly = k_busy
        txtPass.ReadOnly = k_busy
        UseWaitCursor = k_busy
    End Sub

    Private Sub BtnCancel_Click(k_sender As Object, e As EventArgs) Handles btnCancel.Click
        _cancelled = True
        DialogResult = DialogResult.Cancel
    End Sub
End Class
