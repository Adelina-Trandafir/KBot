Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

' Formularul de login al aplicatiei K-BOT: doua faze (credentiale -> unitate).
' Tema: KBotTheme.ApplyTheme + constantele confirmate. Rolul nu e afisat/enforced.
' Parola traieste in memorie doar cat dureaza fluxul si e stearsa la inchidere.
Public NotInheritable Class LoginForm

    Private ReadOnly _authApi As IAuthApi
    Private ReadOnly _session As SessionContext

    ' Pastrate in memorie DOAR pe durata fluxului in doua faze; sterse la inchidere.
    Private _username As String
    Private _password As String

    ' Who logged in last on this Windows account (slice 0063): user name pre-filled at
    ' Load, unit pre-selected at phase 2. Read once; written only after a login SUCCEEDED.
    ' Never holds the password -- see LastLoginStore.
    Private _lastLogin As LastLoginStore

    Public Sub New(authApi As IAuthApi, session As SessionContext)
        ArgumentNullException.ThrowIfNull(authApi)
        ArgumentNullException.ThrowIfNull(session)
        _authApi = authApi
        _session = session
        InitializeComponent()
    End Sub

    Private Sub LoginForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' Tematizarea structurala o face KBotThemedForm (base OnLoad -> ThemeManager.Apply);
            ' accentele/eroarea le pune OnThemeChanged (ruleaza dupa Apply si la comutare live).
            picLogo.Image = My.Resources.kbot_64
            capBar.IconImage = My.Resources.kbot_64
            '#If DEBUG Then
            '            txtUser.Text = "scavatarsoft@gmail.com"
            '            txtPass.Text = "Par0laN0u@"
            '#End If
            ' The last user who got in wins over the Debug default: that is the name the
            ' operator would otherwise type again. Load never throws (missing file = nothing).
            _lastLogin = LastLoginStore.Load()
            If Not String.IsNullOrWhiteSpace(_lastLogin.Username) Then
                txtUser.Text = _lastLogin.Username
            End If
            Me.KeyPreview = True                ' Escape inchide (nu mai exista X nativ)
            ShowPhaseCreds()
        Catch ex As Exception
            ' Boundary UI (Load): logam si inghitim.
            GlobalErrorLog.Write("LoginForm.LoginForm_Load", ex)
        End Try
    End Sub

    ' Fara chenar nativ => fara buton X. Escape inchide dialogul cu Cancel.
    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Try
            MyBase.OnKeyDown(e)
            If e.KeyCode = Keys.Escape Then
                DialogResult = DialogResult.Cancel
                Close()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.OnKeyDown", ex)
        End Try
    End Sub

    ' Culorile theme-aware. Ruleaza DUPA structura temei (base OnLoad cheama
    ' OnThemeChanged dupa Apply) si la fiecare comutare de schema.
    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p = ThemeManager.Current.Palette

            ' Fundalul formularului ESTE conturul de 1px al ferestrei: se vede prin Padding(1)
            ' de jur imprejurul cardului.
            BackColor = p.BorderColor

            ' Etichetele de camp / subtitlul sunt secundare -> text dim (ThemeManager le pune
            ' pe TextColor plin; titlul ramane full TextColor).
            For Each l As Label In {lblUser, lblPass, lblUnit, lblSubtitle}
                l.ForeColor = p.TextDimColor
            Next

            ApplyPrimaryButtons()
            ApplySecondaryButton()
        Catch ex As Exception
            ' Boundary UI (cascada de tema): logam si inghitim.
            GlobalErrorLog.Write("LoginForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' Stilurile de buton au fost extrase in KBot.Theming.ButtonStyles (refolosite de
    ' MainForm); aici raman doar apelurile.
    Private Sub ApplyPrimaryButtons()
        Try
            For Each b As Button In {btnContinue, btnLogin}
                StylePrimaryButton(b)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.ApplyPrimaryButtons", ex)
            Throw
        End Try
    End Sub

    ' A flat button keeps painting its accent BackColor when disabled, so the phase that
    ' is NOT active would still look clickable. Disabled = flat surface + dim text.
    Private Sub StylePrimaryButton(b As Button)
        Dim scheme = ThemeManager.Current
        ButtonStyles.ApplyPrimary(b, scheme)
        If Not b.Enabled Then
            Dim p = scheme.Palette
            b.BackColor = p.SurfaceAltColor
            b.ForeColor = p.DisabledTextColor
            b.FlatAppearance.BorderColor = p.BorderColor
        End If
    End Sub

    Private Sub PrimaryButton_EnabledChanged(sender As Object, e As EventArgs) Handles btnContinue.EnabledChanged, btnLogin.EnabledChanged
        Try
            StylePrimaryButton(CType(sender, Button))
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.PrimaryButton_EnabledChanged", ex)
        End Try
    End Sub

    Private Sub ApplySecondaryButton()
        Try
            ButtonStyles.ApplySecondary(btnBack, ThemeManager.Current)
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.ApplySecondaryButton", ex)
            Throw
        End Try
    End Sub

    ' ---------------- comutare faze ----------------
    ' The form keeps ONE height: every control is always on screen and the phases only
    ' flip Enabled. Phase 1: credentials + Continua live; the unit combo is empty and
    ' disabled, Inapoi / Autentificare disabled.
    Private Sub ShowPhaseCreds()
        Try
            cboUnit.DataSource = Nothing
            cboUnit.Enabled = False
            btnBack.Enabled = False
            btnLogin.Enabled = False
            txtUser.Enabled = True
            txtPass.Enabled = True
            btnContinue.Enabled = True
            Me.AcceptButton = btnContinue
            ClearError()
            ' With the name already filled in (remembered or typed before «Inapoi»), the
            ' next thing to type is the password.
            If String.IsNullOrWhiteSpace(txtUser.Text) Then
                txtUser.FocusInput()
            Else
                txtPass.FocusInput()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.ShowPhaseCreds", ex)
            Throw
        End Try
    End Sub

    ' Phase 2: credentials are locked (the units were fetched with them); the combo was
    ' populated by the caller and becomes live together with Inapoi / Autentificare.
    Private Sub ShowPhaseUnit()
        Try
            txtUser.Enabled = False
            txtPass.Enabled = False
            btnContinue.Enabled = False
            cboUnit.Enabled = True
            btnBack.Enabled = True
            btnLogin.Enabled = True
            Me.AcceptButton = btnLogin
            ClearError()
            cboUnit.Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.ShowPhaseUnit", ex)
            Throw
        End Try
    End Sub

    ' ---------------- helpers ----------------
    Private Sub ShowError(message As String)
        ntfError.Show(message, Global.KBot.Controls.NoticeKind.Error)
    End Sub

    Private Sub ClearError()
        ntfError.Clear()
    End Sub

    ' Cosmetic; chemata si din Finally-ul handler-elor de login => NU rearunca
    ' (un throw din Finally ar scapa din async Sub si ar darama procesul).
    Private Sub SetBusy(busy As Boolean)
        Try
            busyBar.Running = busy
            tlpBody.Enabled = Not busy        ' every control sits in tlpBody => all covered
            Me.UseWaitCursor = busy
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.SetBusy", ex)
        End Try
    End Sub

    ' ---------------- faza 1: obtinere unitati ----------------
    Private Async Sub BtnContinue_Click(sender As Object, e As EventArgs) Handles btnContinue.Click
        Dim user = txtUser.Text.Trim
        Dim pass = txtPass.Text
        If user.Length = 0 OrElse pass.Length = 0 Then
            ShowError("Introduceți utilizatorul și parola.")
            Return
        End If

        ClearError()
        SetBusy(True)
        Try
            Dim units = Await _authApi.GetUnitsAsync(user, pass, CancellationToken.None)
            If units Is Nothing OrElse units.Count = 0 Then
                ShowError("Nu aveți nicio unitate accesibilă.")
                Return
            End If

            _username = user
            _password = pass

            cboUnit.DataSource = New List(Of UnitInfo)(units)
            cboUnit.DisplayMember = NameOf(UnitInfo.Display)   ' arata NumeUnitate
            cboUnit.ValueMember = NameOf(UnitInfo.DC)          ' valoarea din spate e DC
            cboUnit.SelectedIndex = 0    ' caz mono-unitate: pre-selectat, un click de confirmat
            ' Same user as last time and their unit is still on the list -> pre-select it.
            ' Another user, or a unit gone from the list -> the first one, as before.
            If _lastLogin IsNot Nothing AndAlso
               String.Equals(_lastLogin.Username, user, StringComparison.OrdinalIgnoreCase) AndAlso
               Not String.IsNullOrWhiteSpace(_lastLogin.UnitDc) Then
                For i As Integer = 0 To units.Count - 1
                    If String.Equals(units(i).DC, _lastLogin.UnitDc, StringComparison.Ordinal) Then
                        cboUnit.SelectedIndex = i
                        Exit For
                    End If
                Next
            End If

            ShowPhaseUnit()

        Catch ex As ApiException
            ShowError(ex.Message)                              ' mesajul roman al serverului
        Catch ex As Exception
            ShowError("Eroare la conectare. Verificați rețeaua.")
            GlobalErrorLog.Write("LoginForm.GetUnits", ex)     ' log detaliul complet; nu inghitim
        Finally
            SetBusy(False)
        End Try
    End Sub

    ' ---------------- faza 2: login ----------------
    Private Async Sub BtnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim selected = TryCast(cboUnit.SelectedItem, UnitInfo)
        If selected Is Nothing Then
            ShowError("Selectați o unitate.")
            Return
        End If

        ClearError()
        SetBusy(True)
        Try
            Dim result = Await _authApi.LoginAsync(
                _username, _password, selected.DC,
                Environment.MachineName, CancellationToken.None)

            _session.Populate(_username, result.Token, result.SessionContext)   ' OperatorName = e-mail
            _session.LastSS = result.LastSS                                     ' hint pentru MainForm

            ' Remember the pair for next time -- only now, after the server said yes.
            Try
                LastLoginStore.Save(_username, selected.DC)
            Catch ex As Exception
                ' Already logged by Save. A convenience file that cannot be written is not a
                ' reason to fail a login that just succeeded.
            End Try

            DialogResult = DialogResult.OK
            Close()

        Catch ex As ApiException
            ShowError(ex.Message)
            SetBusy(False)
        Catch ex As Exception
            ShowError("Eroare la autentificare. Verificați rețeaua.")
            GlobalErrorLog.Write("LoginForm.Login", ex)
            SetBusy(False)
        End Try
    End Sub

    Private Sub BtnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Try
            _password = Nothing
            ShowPhaseCreds()
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.BtnBack_Click", ex)
        End Try
    End Sub

    Private Sub LoginForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            ' Nu lasa credentialele in memorie dupa ce dialogul se incheie.
            _password = Nothing
            _username = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.LoginForm_FormClosed", ex)
        End Try
    End Sub
End Class
