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

    ' Doua inaltimi ale ferestrei: compacta (doar credentiale) si extinsa (+ selector
    ' unitate). Capturate la Load; formularul creste la faza 2, revine la Inapoi.
    Private _collapsedHeight As Integer
    Private _expandedHeight As Integer

    ' --- culoare eroare theme-aware: slotul Error din paleta schemei active ---
    Private ReadOnly Property ClrError As Color
        Get
            Return ThemeManager.Current.Palette.ErrorColor
        End Get
    End Property

    Public Sub New(authApi As IAuthApi, session As SessionContext)
        ArgumentNullException.ThrowIfNull(authApi)
        ArgumentNullException.ThrowIfNull(session)
        _authApi = authApi
        _session = session
        InitializeComponent()
    End Sub

    Private Sub LoginForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Tematizarea structurala o face KBotThemedForm (base OnLoad -> ThemeManager.Apply);
        ' accentele/eroarea le pune OnThemeChanged (ruleaza dupa Apply si la comutare live).
        CaptureFormHeights()
        ShowPhaseCreds()
    End Sub

    ' Culorile theme-aware ale showcase-ului. Ruleaza DUPA structura temei (base OnLoad
    ' cheama OnThemeChanged dupa Apply) si la fiecare comutare de schema.
    Protected Overrides Sub OnThemeChanged()
        MyBase.OnThemeChanged()
        ApplyAccentColors()
        lblError.ForeColor = ClrError
    End Sub

    ' Butoanele primare = accent (din paleta activa); rotunjite cand schema e Modern.
    Private Sub ApplyAccentColors()
        Dim scheme = ThemeManager.Current
        Dim p = scheme.Palette
        Dim modern As Boolean = (scheme.Style.ButtonRender = ButtonRenderStyle.ModernOwnerDrawn)

        For Each b As Button In {btnContinue, btnLogin}
            If modern Then
                ModernRenderer.ApplyButton(b, scheme)   ' colturi rotunjite + handlere
            Else
                ModernRenderer.DetachButton(b)          ' scoate rotunjirea daca venim din Modern
                b.FlatStyle = FlatStyle.Flat
                b.FlatAppearance.BorderSize = 1
            End If
            ' Peste randarea de baza, pictam accentul (buton primar).
            b.BackColor = p.AccentColor
            b.ForeColor = p.AccentTextColor
            b.FlatAppearance.BorderColor = p.AccentColor
            b.FlatAppearance.MouseOverBackColor = p.AccentHoverColor
            b.FlatAppearance.MouseDownBackColor = p.AccentColor
            b.UseVisualStyleBackColor = False
        Next

        ' Butonul secundar „Inapoi” — buton neutru din paleta.
        ModernRenderer.DetachButton(btnBack)
        btnBack.FlatStyle = FlatStyle.Flat
        btnBack.FlatAppearance.BorderColor = p.ButtonBorderColor
        btnBack.BackColor = p.ButtonBackColor
        btnBack.ForeColor = p.ButtonTextColor
        btnBack.UseVisualStyleBackColor = False
    End Sub

    ' Inaltimea din Designer include selectorul (pnlUnit) vizibil => e cea extinsa.
    ' Cea compacta scade spatiul ocupat de selector. Capturat o singura data, la Load.
    Private Sub CaptureFormHeights()
        _expandedHeight = Me.Height
        Dim unitSpace As Integer = If(pnlUnit.Height > 0, pnlUnit.Height, 150)
        _collapsedHeight = _expandedHeight - unitSpace
    End Sub

    ' ---------------- comutare faze ----------------
    ' Faza 1: doar credentialele. Formularul e compact; selectorul (pnlUnit, imbricat in
    ' randul elastic al pnlCreds) e ascuns.
    Private Sub ShowPhaseCreds()
        pnlUnit.Visible = False
        Me.Height = _collapsedHeight
        Me.AcceptButton = btnContinue
        ClearError()
        txtUser.Focus()
    End Sub

    ' Faza 2: formularul creste si arata selectorul unitatii sub credentiale.
    Private Sub ShowPhaseUnit()
        pnlUnit.Visible = True
        Me.Height = _expandedHeight
        Me.AcceptButton = btnLogin
        ClearError()
        cboUnit.Focus()
    End Sub

    ' ---------------- helpers ----------------
    Private Sub ShowError(message As String)
        lblError.Text = message
        lblError.Visible = True
    End Sub

    Private Sub ClearError()
        lblError.Visible = False
        lblError.Text = String.Empty
    End Sub

    Private Sub SetBusy(busy As Boolean)
        pbBusy.Visible = busy
        pnlCreds.Enabled = Not busy
        pnlUnit.Enabled = Not busy
        Me.UseWaitCursor = busy
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
            Dim units = Await _authApi.GetUnitsAsync(user, pass, Nothing, CancellationToken.None)
            If units Is Nothing OrElse units.Count = 0 Then
                ShowError("Nu aveți nicio unitate accesibilă.")
                Return
            End If

            _username = user
            _password = pass

            cboUnit.DataSource = New List(Of UnitInfo)(units)
            cboUnit.DisplayMember = NameOf(UnitInfo.Display)
            cboUnit.ValueMember = NameOf(UnitInfo.IdUnitate)
            cboUnit.SelectedIndex = 0    ' caz mono-unitate: pre-selectat, un click de confirmat

            ShowPhaseUnit()

        Catch ex As ApiException
            ShowError(ex.Message)                              ' mesajul roman al serverului
        Catch ex As Exception
            ShowError("Eroare la conectare. Verificați rețeaua.")
            Write("LoginForm.GetUnits", ex)     ' log detaliul complet; nu inghitim
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
                _username, _password, selected.IdUnitate,
                Environment.MachineName, CancellationToken.None)

            _session.Populate(_username, result.Token, result.SessionContext)

            DialogResult = DialogResult.OK
            Close()

        Catch ex As ApiException
            ShowError(ex.Message)
        Catch ex As Exception
            ShowError("Eroare la autentificare. Verificați rețeaua.")
            Write("LoginForm.Login", ex)
        Finally
            SetBusy(False)
        End Try
    End Sub

    Private Sub BtnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        _password = Nothing
        ShowPhaseCreds()
    End Sub

    Private Sub LoginForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        ' Nu lasa credentialele in memorie dupa ce dialogul se incheie.
        _password = Nothing
        _username = Nothing
    End Sub
End Class
