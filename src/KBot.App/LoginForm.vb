Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
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
    Private ReadOnly _updates As AppUpdateService

    ' Slice 0104: one e-mail, as the server's address check would take it (the server checks again).
    Private Shared ReadOnly EmailShape As New Regex("^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)

    ' Pastrate in memorie DOAR pe durata fluxului in doua faze; sterse la inchidere.
    Private _username As String
    Private _password As String

    ' Who logged in last on this Windows account (slice 0063): user name pre-filled at
    ' Load, unit pre-selected at phase 2. Read once; written only after a login SUCCEEDED.
    ' Never holds the password -- see LastLoginStore.
    Private _lastLogin As LastLoginStore

    ''' <summary>
    ''' Slice 0104: the startup update check has not run yet because the client's kind (with / without Access)
    ''' was not known when the application started -- no e-mail remembered. It runs here, once, as soon as the
    ''' server has said the kind for the e-mail the operator types.
    ''' </summary>
    Public Property UpdateCheckPending As Boolean

    ''' <summary>Slice 0104: that check started the updater (or a mandatory update was refused): the window closed
    ''' with Cancel and the application must exit, not go on without a login.</summary>
    Public Property ExitForUpdate As Boolean

    ''' <summary>Slice 0081-06: the unit to pre-select at phase 2, over the remembered one -- the
    ''' director's window asks for a login on the unit of the document to sign. Nothing = as before.</summary>
    Public Property PreferredDc As String

    Public Sub New(authApi As IAuthApi, session As SessionContext, updates As AppUpdateService)
        ArgumentNullException.ThrowIfNull(authApi)
        ArgumentNullException.ThrowIfNull(session)
        ArgumentNullException.ThrowIfNull(updates)
        _authApi = authApi
        _session = session
        _updates = updates
        InitializeComponent()
    End Sub

    Private Sub LoginForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            ' Tematizarea structurala o face KBotThemedForm (base OnLoad -> ThemeManager.Apply);
            ' accentele/eroarea le pune OnThemeChanged (ruleaza dupa Apply si la comutare live).
            picLogo.Image = My.Resources.kbot_64
            capBar.IconImage = My.Resources.kbot_64
#If DEBUG Then
            txtUser.Text = "scavatarsoft@gmail.com"
            txtPass.Text = "Par0laN0u@"
#End If
            ' The last user who got in wins over the Debug default: that is the name the
            ' operator would otherwise type again. Load never throws (missing file = nothing).
            _lastLogin = LastLoginStore.Load()
            ' ...unless the operator switched the memory off («Setari» -> Autentificare, slice 0072).
            If AppSettings.Current.RememberLastLogin AndAlso Not String.IsNullOrWhiteSpace(_lastLogin.Username) Then
                txtUser.Text = _lastLogin.Username
            End If
            ConfigureRememberPassword()
            Me.KeyPreview = True                ' Escape inchide (nu mai exista X nativ)
            ShowPhaseCreds()
        Catch ex As Exception
            ' Boundary UI (Load): logam si inghitim.
            GlobalErrorLog.Write("LoginForm.LoginForm_Load", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0097 -- «Tine minte parola pana la repornirea calculatorului». The box is on the
    ''' window only with the advanced options on and its switch in «Setari → Autentificare»
    ''' (<see cref="AppSettings.RememberPasswordInEffect"/>); the window grows by its row only then.
    ''' A pair stored for this Windows session (<see cref="SessionCredentials"/>) fills both fields
    ''' and ticks the box. Without the box, a stored pair is not used at all.
    ''' </summary>
    Private Sub ConfigureRememberPassword()
        Dim shown As Boolean = AppSettings.Current.RememberPasswordInEffect
        chkRememberPassword.Visible = shown
        If Not shown Then Return
        Height += chkRememberPassword.Height + chkRememberPassword.Margin.Vertical

        Dim user As String = Nothing, pass As String = Nothing
        If SessionCredentials.TryLoadForWindowsSession(user, pass) Then
            txtUser.Text = user
            txtPass.Text = pass
            chkRememberPassword.Checked = True
        End If
    End Sub

    ' Slice 0097: after a login the server accepted. The password is kept in this process for the
    ' silent re-login (KbotForm.WithReauth), and for the Windows session when the box is ticked --
    ' unticked, a pair stored earlier is erased. Never fails the login it follows.
    Private Sub RememberCredentials()
        Try
            SessionCredentials.Remember(_username, _password, interactive:=True)
            If chkRememberPassword.Visible Then
                If chkRememberPassword.Checked Then
                    SessionCredentials.SaveForWindowsSession(_username, _password)
                Else
                    SessionCredentials.ForgetWindowsSession()
                End If
            End If
        Catch ex As Exception
            ' Already logged by SessionCredentials. A convenience that cannot be stored is not a
            ' reason to fail a login that just succeeded.
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
            cboUnit.Items.Clear()
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

    ' ---------------- slice 0104: ce fel de client este e-mailul ----------------
    ' Each change of the e-mail box restarts a short timer; when typing stops, a non-empty address is sent
    ' to the server, which says whether that client also has the Access application. The answer picks
    ' the update package (and, on a first start, lets the postponed update check run).
    Private Sub TxtUser_TextChanged(sender As Object, e As EventArgs) Handles txtUser.TextChanged
        Try
            tmrAccess.Stop()
            If EmailShape.IsMatch(txtUser.Text.Trim()) Then tmrAccess.Start()
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.TxtUser_TextChanged", ex)
        End Try
    End Sub

    Private Async Sub TmrAccess_Tick(sender As Object, e As EventArgs) Handles tmrAccess.Tick
        Try
            tmrAccess.Stop()
            Dim email As String = txtUser.Text.Trim()
            If Not EmailShape.IsMatch(email) Then Return
            Await ResolveKindAsync(email)
        Catch ex As Exception
            ' UI boundary (timer, async Sub): log and swallow.
            GlobalErrorLog.Write("LoginForm.TmrAccess_Tick", ex)
        End Try
    End Sub

    ' Asks the server (once per e-mail), then runs the postponed update check. Never throws: a server that
    ' does not answer leaves the kind unknown and the operator logs in as usual.
    Private Async Function ResolveKindAsync(email As String) As Task
        Try
            If Not Await _updates.ResolveAccessAsync(email, CancellationToken.None) Then Return
            ' The box moved on while the server was answering: the answer is for an e-mail no longer there.
            If Not ClientProfile.IsResolvedFor(txtUser.Text) Then Return
            If Not UpdateCheckPending OrElse Not ClientProfile.AccessKnown Then Return
            UpdateCheckPending = False
            If Await _updates.RunStartupCheckAsync(Me) Then
                ExitForUpdate = True
                DialogResult = DialogResult.Cancel
                Close()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.ResolveKindAsync", ex)
        End Try
    End Function

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
            ' Slice 0104: Enter / Continue before the timer fired -- ask now, so the update check has run.
            tmrAccess.Stop()
            If EmailShape.IsMatch(user) Then Await ResolveKindAsync(user)
            If ExitForUpdate Then Return

            Dim units = Await _authApi.GetUnitsAsync(user, pass, CancellationToken.None)
            If units Is Nothing OrElse units.Count = 0 Then
                ShowError("Nu aveți nicio unitate accesibilă.")
                Return
            End If

            _username = user
            _password = pass

            ' The combo shows the unit's name, never the raw DC; the item itself stays the UnitInfo.
            cboUnit.CaptionSelector = Function(o) DirectCast(o, UnitInfo).Display
            cboUnit.Items.Clear()
            cboUnit.Items.AddRange(units)
            cboUnit.SelectedIndex = 0    ' caz mono-unitate: pre-selectat, un click de confirmat
            ' Same user as last time and their unit is still on the list -> pre-select it.
            ' Another user, or a unit gone from the list -> the first one, as before.
            If AppSettings.Current.RememberLastUnit AndAlso _lastLogin IsNot Nothing AndAlso
               String.Equals(_lastLogin.Username, user, StringComparison.OrdinalIgnoreCase) AndAlso
               Not String.IsNullOrWhiteSpace(_lastLogin.UnitDc) Then
                For i As Integer = 0 To units.Count - 1
                    If String.Equals(units(i).DC, _lastLogin.UnitDc, StringComparison.Ordinal) Then
                        cboUnit.SelectedIndex = i
                        Exit For
                    End If
                Next
            End If

            If Not String.IsNullOrWhiteSpace(PreferredDc) Then
                For i As Integer = 0 To units.Count - 1
                    If String.Equals(units(i).DC, PreferredDc, StringComparison.Ordinal) Then
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
            RememberCredentials()

            ' Remember the pair for next time -- only now, after the server said yes, and only
            ' while the operator wants it remembered (slice 0072). With the memory off the file
            ' is not written at all: an unread file is still a file with the e-mail in it.
            Try
                If AppSettings.Current.RememberLastLogin Then
                    LastLoginStore.Save(_username, If(AppSettings.Current.RememberLastUnit, selected.DC, Nothing))
                End If
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
            tmrAccess.Stop()
            ' Nu lasa credentialele in memorie dupa ce dialogul se incheie.
            _password = Nothing
            _username = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("LoginForm.LoginForm_FormClosed", ex)
        End Try
    End Sub
End Class
