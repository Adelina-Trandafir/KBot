Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

' Formularul de login al aplicatiei K-BOT: doua faze (credentiale -> unitate).
' Tema: KBotTheme.ApplyTheme + constantele confirmate. Rolul nu e afisat/enforced.
' Parola traieste in memorie doar cat dureaza fluxul si e stearsa la inchidere.
Public NotInheritable Class LoginForm

    Private ReadOnly _authApi As IAuthApi
    Private ReadOnly _session As SessionContext

    ' Pastrate in memorie DOAR pe durata fluxului in doua faze; sterse la inchidere.
    Private _username As String
    Private _password As String

    ' Inaltimile originale ale randurilor pnlCreds, capturate la Load; folosite ca sa
    ' colapsam randurile de credentiale in faza 2 (doar selectorul unitatii ramane).
    Private _credRowHeights As Single()

    ' --- culori accent theme-aware (doar unde nu exista o constanta KBotTheme reala) ---
    Private ReadOnly Property ClrError As Color
        Get
            Return If(KBotTheme.IsDark,
                      Color.FromArgb(240, 120, 120),
                      Color.FromArgb(190, 30, 30))
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
        ' Tematizare structurala intai; preferam constantele KBotTheme reale unde exista.
        KBotTheme.ApplyTheme(Me)
        ApplyAccentColors()
        lblError.ForeColor = ClrError
        CaptureCredRowHeights()
        ShowPhaseCreds()
    End Sub

    Private Sub ApplyAccentColors()
        ' Butoanele primare folosesc constantele confirmate din KBotTheme.
        For Each b As Button In {btnContinue, btnLogin}
            b.FlatAppearance.BorderColor = KBotTheme.CLR_TAB_ACCENT
            b.FlatAppearance.BorderSize = 1
            b.BackColor = KBotTheme.CLR_BTN
            b.UseVisualStyleBackColor = False
        Next
        btnBack.FlatAppearance.BorderColor = KBotTheme.CLR_BTN_BORDER
        btnBack.BackColor = KBotTheme.CLR_BG
        btnBack.UseVisualStyleBackColor = False
    End Sub

    ' Capteaza inaltimile randurilor pnlCreds inainte de orice colaps (o singura data, la Load).
    Private Sub CaptureCredRowHeights()
        _credRowHeights = New Single(pnlCreds.RowStyles.Count - 1) {}
        For i As Integer = 0 To pnlCreds.RowStyles.Count - 1
            _credRowHeights(i) = pnlCreds.RowStyles(i).Height
        Next
    End Sub

    ' ---------------- comutare faze ----------------
    ' pnlUnit e imbricat in randul elastic (row5) al pnlCreds. Comutarea NU ascunde
    ' pnlCreds (ar ascunde si pnlUnit); in schimb aratam/ascundem controalele de
    ' credentiale si colapsam randurile lor, lasand selectorul sa umple cardul in faza 2.
    Private Sub ShowPhaseCreds()
        SetCredControlsVisible(True)
        SetCredRowsCollapsed(False)
        pnlUnit.Visible = False
        Me.AcceptButton = btnContinue
        ClearError()
        txtUser.Focus()
    End Sub

    Private Sub ShowPhaseUnit()
        pnlUnit.Visible = True
        SetCredControlsVisible(False)
        SetCredRowsCollapsed(True)
        Me.AcceptButton = btnLogin
        ClearError()
        cboUnit.Focus()
    End Sub

    ' Vizibilitatea controalelor fazei 1 (utilizator / parola / Continua).
    Private Sub SetCredControlsVisible(visible As Boolean)
        lblUser.Visible = visible
        txtUser.Visible = visible
        lblPass.Visible = visible
        txtPass.Visible = visible
        btnContinue.Visible = visible
    End Sub

    ' Colapseaza randurile Absolute ale pnlCreds (credentiale + spatiere) la 0 in faza 2,
    ' ca randul Percent (pnlUnit) sa ocupe tot cardul; le reface din inaltimile capturate.
    Private Sub SetCredRowsCollapsed(collapsed As Boolean)
        pnlCreds.SuspendLayout()
        For i As Integer = 0 To pnlCreds.RowStyles.Count - 1
            Dim rs As RowStyle = pnlCreds.RowStyles(i)
            If rs.SizeType = SizeType.Absolute Then
                rs.Height = If(collapsed, 0F, _credRowHeights(i))
            End If
        Next
        pnlCreds.ResumeLayout()
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

            _session.Populate(_username, result.SessionId, result.SessionContext)

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
