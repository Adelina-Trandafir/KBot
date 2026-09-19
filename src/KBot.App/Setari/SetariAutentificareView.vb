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

    ' ---------------- theme ----------------

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            tlyMemorie.BackColor = p.SurfaceAltColor
            tlyServer.BackColor = p.SurfaceAltColor
            For Each caption As Label In New Label() {lblUtilizatorCaption, lblUnitateCaption, lblServerCaption,
                                                      lblTimeoutCaption, lblServerHint}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplySecondary(btnUita, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAutentificareView.ApplyTheme", ex)
        End Try
    End Sub

End Class
