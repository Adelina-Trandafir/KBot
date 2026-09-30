Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' «Autentificare» (slice 0072): what the login form remembers between sessions (slice
''' 0063's <see cref="LastLoginStore"/>) -- the two switches that allow it, what is remembered
''' right now, and the button that forgets it -- plus the server the form talks to, read-only
''' (the address is a constant in <see cref="ApiOptions"/>, https only, by decision).
''' </summary>
Public Class SetariAutentificareView
    Implements ISetariView, IThemedContainer

    Private ReadOnly _apiOptions As ApiOptions

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    Private _suppress As Boolean

    Public Sub New(apiOptions As ApiOptions)
        ArgumentNullException.ThrowIfNull(apiOptions)
        InitializeComponent()
        _apiOptions = apiOptions
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "autentificare"
        End Get
    End Property

    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Return True
    End Function

    Public Sub Activated() Implements ISetariView.Activated
        Try
            _suppress = True
            Try
                chkRememberLogin.Checked = AppSettings.Current.RememberLastLogin
                chkRememberUnit.Checked = AppSettings.Current.RememberLastUnit
                ArataReautentificarea()
            Finally
                _suppress = False
            End Try
            ActualizeazaDisponibilitatea()
            ActualizeazaMemoria()
            lblServer.Text = _apiOptions.BaseUrl
            lblTimeout.Text = _apiOptions.TimeoutSeconds.ToString(Globalization.CultureInfo.InvariantCulture) & " secunde per cerere"
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAutentificareView.Activated", ex)
        End Try
    End Sub

    ' What last_login.json holds now. Load never throws (missing file = nothing remembered).
    Private Sub ActualizeazaMemoria()
        Dim memorat As LastLoginStore = LastLoginStore.Load()
        Dim areCeva As Boolean = Not String.IsNullOrWhiteSpace(memorat.Username)
        lblUtilizator.Text = If(areCeva, memorat.Username, "— (nimic memorat)")
        lblUnitate.Text = If(String.IsNullOrWhiteSpace(memorat.UnitDc), "—", memorat.UnitDc)
        btnUita.Enabled = areCeva
    End Sub

    ' The unit switch means nothing without the user switch: greyed out, not silently ignored.
    Private Sub ActualizeazaDisponibilitatea()
        chkRememberUnit.Enabled = chkRememberLogin.Checked
    End Sub

    Private Sub SalveazaComutator(aplica As Action(Of AppSettings), mesaj As String)
        Try
            If _suppress Then Return
            Dim copie As AppSettings = AppSettings.Current.Clone()
            aplica(copie)
            copie.Save()
            RaiseEvent StatusChanged(mesaj)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAutentificareView.SalveazaComutator", ex)
            RaiseEvent StatusChanged("Setarea nu a putut fi salvată: " & ex.Message)
        End Try
    End Sub

    Private Sub ChkRememberLogin_CheckedChanged(sender As Object, e As EventArgs) Handles chkRememberLogin.CheckedChanged
        ActualizeazaDisponibilitatea()
        SalveazaComutator(Sub(s) s.RememberLastLogin = chkRememberLogin.Checked,
                          If(chkRememberLogin.Checked,
                             "Ultimul utilizator se ține minte.",
                             "Ultimul utilizator nu se mai ține minte de la următoarea autentificare."))
    End Sub

    Private Sub ChkRememberUnit_CheckedChanged(sender As Object, e As EventArgs) Handles chkRememberUnit.CheckedChanged
        SalveazaComutator(Sub(s) s.RememberLastUnit = chkRememberUnit.Checked,
                          If(chkRememberUnit.Checked, "Unitatea aleasă se ține minte.", "Unitatea aleasă nu se mai ține minte."))
    End Sub

    Private Sub BtnUita_Click(sender As Object, e As EventArgs) Handles btnUita.Click
        Try
            If KBotMessage.Show(FindForm(),
                    "Datele memorate (e-mailul și unitatea) se șterg de pe acest calculator. Continui?",
                    "Uită datele memorate", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            Dim sters As Boolean = LastLoginStore.Forget()
            ActualizeazaMemoria()
            RaiseEvent StatusChanged(If(sters, "Datele memorate au fost șterse.", "Nu era nimic memorat."))
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAutentificareView.BtnUita_Click", ex)
            RaiseEvent StatusChanged("Datele memorate nu au putut fi șterse: " & ex.Message)
        End Try
    End Sub

    ' ---------------- during work (slice 0097) ----------------

    ' The minutes offered in the combo, in its order.
    Private _minuteOferite As IReadOnlyList(Of Integer) = New List(Of Integer)()

    ''' <summary>
    ''' The «In timpul lucrului» section as the settings and the advanced options stand now.
    ''' Without the advanced options the window cannot be switched off (the box is shown ticked and
    ''' greyed out), the interval is 10..60 minutes, and the «Tine minte parola» switch is hidden.
    ''' Called with <c>_suppress</c> raised.
    ''' </summary>
    Private Sub ArataReautentificarea()
        Dim s As AppSettings = AppSettings.Current
        Dim avansat As Boolean = s.AdvancedOptions

        chkRelogin.Checked = s.ReloginPromptInEffect
        chkRelogin.Enabled = avansat

        _minuteOferite = If(avansat, AppSettings.ReloginMinuteChoicesAdvanced, AppSettings.ReloginMinuteChoices)
        cmbInterval.Items.Clear()
        For Each m As Integer In _minuteOferite
            cmbInterval.Items.Add(EtichetaMinute(m))
        Next
        ' The closest offered value: a hand-edited file may hold one the list does not have.
        Dim curent As Integer = s.ReloginMinutesInEffect
        Dim cel As Integer = 0
        For i As Integer = 1 To _minuteOferite.Count - 1
            If Math.Abs(_minuteOferite(i) - curent) < Math.Abs(_minuteOferite(cel) - curent) Then cel = i
        Next
        cmbInterval.SelectedIndex = cel
        ActualizeazaIntervalul()

        chkRememberPasswordOption.Visible = avansat
        chkRememberPasswordOption.Checked = s.RememberPasswordOption
        btnUitaParola.Visible = avansat
        btnUitaParola.Enabled = SessionCredentials.HasWindowsSessionPassword()
    End Sub

    ' The interval means nothing when the window is off: greyed out, not silently ignored.
    Private Sub ActualizeazaIntervalul()
        cmbInterval.Enabled = chkRelogin.Checked
        lblIntervalCaption.Enabled = chkRelogin.Checked
    End Sub

    Private Shared Function EtichetaMinute(m As Integer) As String
        If m = 60 Then Return "60 de minute (o dată pe oră)"
        If m > 60 AndAlso m Mod 60 = 0 Then Return $"{m \ 60} ore"
        If m < 20 Then Return $"{m} minute"
        Return $"{m} de minute"
    End Function

    Private Sub ChkRelogin_CheckedChanged(sender As Object, e As EventArgs) Handles chkRelogin.CheckedChanged
        ActualizeazaIntervalul()
        SalveazaComutator(Sub(s) s.ReloginPrompt = chkRelogin.Checked,
                          If(chkRelogin.Checked,
                             "Fereastra de autentificare se reafișează când expiră sesiunea.",
                             "K-BOT se reautentifică singur când expiră sesiunea, fără fereastră."))
    End Sub

    Private Sub CmbInterval_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbInterval.SelectedIndexChanged
        If cmbInterval.SelectedIndex < 0 OrElse cmbInterval.SelectedIndex >= _minuteOferite.Count Then Return
        Dim m As Integer = _minuteOferite(cmbInterval.SelectedIndex)
        SalveazaComutator(Sub(s) s.ReloginMinutes = m,
                          $"Fereastra de autentificare se reafișează cel mult o dată la {EtichetaMinute(m)}.")
    End Sub

    Private Sub ChkRememberPasswordOption_CheckedChanged(sender As Object, e As EventArgs) Handles chkRememberPasswordOption.CheckedChanged
        SalveazaComutator(Sub(s) s.RememberPasswordOption = chkRememberPasswordOption.Checked,
                          If(chkRememberPasswordOption.Checked,
                             "Bifa «Ține minte parola» apare în fereastra de autentificare.",
                             "Bifa «Ține minte parola» nu mai apare; o parolă memorată nu se mai folosește."))
    End Sub

    Private Sub BtnUitaParola_Click(sender As Object, e As EventArgs) Handles btnUitaParola.Click
        Try
            Dim sters As Boolean = SessionCredentials.ForgetWindowsSession()
            btnUitaParola.Enabled = False
            RaiseEvent StatusChanged(If(sters, "Parola memorată a fost ștearsă.", "Nu era nicio parolă memorată."))
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAutentificareView.BtnUitaParola_Click", ex)
            RaiseEvent StatusChanged("Parola memorată nu a putut fi ștearsă: " & ex.Message)
        End Try
    End Sub

    ' ---------------- theme ----------------

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            tlyMemorie.BackColor = p.SurfaceAltColor
            tlyLucru.BackColor = p.SurfaceAltColor
            tlyServer.BackColor = p.SurfaceAltColor
            For Each caption As Label In New Label() {lblUtilizatorCaption, lblUnitateCaption, lblServerCaption,
                                                      lblTimeoutCaption, lblServerHint, lblIntervalCaption,
                                                      lblReloginHint}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplySecondary(btnUita, scheme)
            ButtonStyles.ApplySecondary(btnUitaParola, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAutentificareView.ApplyTheme", ex)
        End Try
    End Sub

End Class
