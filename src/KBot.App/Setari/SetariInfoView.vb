Option Strict On
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Informații» (slice 0072): who is logged in, what this installation is, which version
''' runs (+ «Caută actualizări»), and the password change.
'''
''' <para><b>Password change in two steps, two factors.</b> The operator types the current
''' password and asks for a code; the server checks that password (first factor) and
''' e-mails a 6-digit code to the operator's address -- the user name IS the e-mail
''' (second factor). Only then do the code and the new-password fields open. Nothing about
''' the passwords is kept on this machine: the fields are cleared after every outcome.</para>
'''
''' <para><b>Licence.</b> There is no licensing model on the server yet, so «Tip instalare»
''' reads what the session knows (the unit) and says so; the row exists so the commercial
''' information (demo / registered) has its place when it arrives.</para>
''' </summary>
Public Class SetariInfoView
    Implements ISetariView, IThemedContainer

    Private ReadOnly _session As SessionContext
    Private ReadOnly _authApi As IAuthApi
    Private ReadOnly _updates As AppUpdateService
    Private ReadOnly _exitApp As Action

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    ' The password of the FIRST step, held only between «Trimite codul» and «Schimbă parola»
    ' (the change call sends it again), then cleared.
    Private _currentPassword As String

    Public Sub New(session As SessionContext, authApi As IAuthApi, updates As AppUpdateService, exitApp As Action)
        ArgumentNullException.ThrowIfNull(session)
        ArgumentNullException.ThrowIfNull(authApi)
        ArgumentNullException.ThrowIfNull(updates)
        ArgumentNullException.ThrowIfNull(exitApp)
        InitializeComponent()
        _session = session
        _authApi = authApi
        _updates = updates
        _exitApp = exitApp
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "info"
        End Get
    End Property

    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Return True
    End Function

    Public Sub Activated() Implements ISetariView.Activated
        Try
            lblOperator.Text = If(String.IsNullOrEmpty(_session.OperatorName), "—", _session.OperatorName)
            lblUnitate.Text = If(String.IsNullOrEmpty(_session.NumeUnitate), "—", _session.NumeUnitate)
            lblRol.Text = If(String.IsNullOrEmpty(_session.Role), "—", _session.Role)
            lblBaza.Text = DescriePerioada()
            lblTip.Text = If(_session.IsAuthenticated,
                             "Instalare înregistrată pe unitatea «" & _session.NumeUnitate & "» (datele comerciale — în lucru)",
                             "Neautentificat (banc de probă)")
            lblVersiune.Text = VersiuneaCurenta()
            ' A fresh look at the page starts the password flow from the beginning.
            ReseteazaParola()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariInfoView.Activated", ex)
        End Try
    End Sub

    Private Function DescriePerioada() As String
        If String.IsNullOrEmpty(_session.DbName) Then Return "—"
        Dim text As String = _session.DbName
        If _session.An > 0 Then text &= " · " & _session.An.ToString(Globalization.CultureInfo.InvariantCulture)
        If Not String.IsNullOrEmpty(_session.SectorSursa) Then text &= " / " & _session.SectorSursa
        If Not String.IsNullOrEmpty(_session.CodProgram) Then text &= " · " & _session.CodProgram
        Return text
    End Function

    Private Shared Function VersiuneaCurenta() As String
        Try
            Return AppUpdateService.CurrentVersion.ToString()
        Catch ex As Exception
            ' Already logged by the service; the page says it could not read it.
            Return "necunoscută"
        End Try
    End Function

    ' ---------------- updates ----------------

    Private Async Sub BtnActualizari_Click(sender As Object, e As EventArgs) Handles btnActualizari.Click
        Try
            btnActualizari.Enabled = False
            RaiseEvent BusyChanged(True)
            RaiseEvent StatusChanged("Se verifică actualizările...")
            Dim mustExit As Boolean = Await _updates.RunManualCheckAsync(FindForm())
            RaiseEvent StatusChanged(String.Empty)
            If mustExit Then _exitApp()
        Catch ex As Exception
            ' UI boundary (async Sub): log and tell.
            GlobalErrorLog.Write("SetariInfoView.BtnActualizari_Click", ex)
            RaiseEvent StatusChanged("Verificarea actualizărilor a eșuat: " & ex.Message)
        Finally
            RaiseEvent BusyChanged(False)
            btnActualizari.Enabled = True
        End Try
    End Sub

    ' ---------------- password ----------------

    Private Sub ReseteazaParola()
        _currentPassword = Nothing
        txtParolaActuala.Text = String.Empty
        txtCod.Text = String.Empty
        txtParolaNoua.Text = String.Empty
        txtParolaConfirmare.Text = String.Empty
        txtParolaActuala.Enabled = True
        btnTrimiteCod.Enabled = True
        txtCod.Enabled = False
        txtParolaNoua.Enabled = False
        txtParolaConfirmare.Enabled = False
        btnSchimbaParola.Enabled = False
        ntfParola.Clear()
    End Sub

    Private Sub SeteazaOcupat(busy As Boolean)
        RaiseEvent BusyChanged(busy)
        tlyParola.Enabled = Not busy
    End Sub

    Private Async Sub BtnTrimiteCod_Click(sender As Object, e As EventArgs) Handles btnTrimiteCod.Click
        Dim parola As String = txtParolaActuala.Text
        If parola.Length = 0 Then
            ntfParola.Show("Introduceți parola actuală.", NoticeKind.Error)
            Return
        End If
        If Not _session.IsAuthenticated Then
            ntfParola.Show("Schimbarea parolei cere o sesiune autentificată.", NoticeKind.Error)
            Return
        End If

        ntfParola.Clear()
        SeteazaOcupat(True)
        Try
            Dim info As PasswordCodeInfo = Await _authApi.RequestPasswordCodeAsync(_session.Token, parola, CancellationToken.None)
            _currentPassword = parola
            ' Step two opens; the first step locks so the code stays tied to that password.
            txtParolaActuala.Enabled = False
            btnTrimiteCod.Enabled = False
            txtCod.Enabled = True
            txtParolaNoua.Enabled = True
            txtParolaConfirmare.Enabled = True
            btnSchimbaParola.Enabled = True
            Dim minute As Integer = Math.Max(1, info.ExpiresInSeconds \ 60)
            ntfParola.Show($"Codul a fost trimis la {info.EmailMasked}. Este valabil {minute} minute.", NoticeKind.Success)
            RaiseEvent StatusChanged("Cod de confirmare trimis pe e-mail.")
            txtCod.FocusInput()
        Catch ex As ApiException
            ntfParola.Show(ex.Message, NoticeKind.Error)   ' the server's Romanian sentence
        Catch ex As Exception
            GlobalErrorLog.Write("SetariInfoView.BtnTrimiteCod_Click", ex)
            ntfParola.Show("Codul nu a putut fi cerut. Verificați rețeaua.", NoticeKind.Error)
        Finally
            SeteazaOcupat(False)
        End Try
    End Sub

    Private Async Sub BtnSchimbaParola_Click(sender As Object, e As EventArgs) Handles btnSchimbaParola.Click
        Dim cod As String = txtCod.Text.Trim()
        Dim noua As String = txtParolaNoua.Text
        Dim confirmare As String = txtParolaConfirmare.Text

        If cod.Length = 0 Then
            ntfParola.Show("Introduceți codul primit pe e-mail.", NoticeKind.Error)
            Return
        End If
        If noua.Length < 8 Then
            ntfParola.Show("Parola nouă trebuie să aibă cel puțin 8 caractere.", NoticeKind.Error)
            Return
        End If
        If Not String.Equals(noua, confirmare, StringComparison.Ordinal) Then
            ntfParola.Show("Parola nouă și confirmarea ei nu coincid.", NoticeKind.Error)
            Return
        End If
        If String.Equals(noua, _currentPassword, StringComparison.Ordinal) Then
            ntfParola.Show("Parola nouă trebuie să difere de cea actuală.", NoticeKind.Error)
            Return
        End If

        ntfParola.Clear()
        SeteazaOcupat(True)
        Try
            Dim rezultat As PasswordChangeResult = Await _authApi.ChangePasswordAsync(
                _session.Token, _currentPassword, cod, noua, CancellationToken.None)
            ReseteazaParola()
            If rezultat.LegacyUpdated Then
                ntfParola.Show("Parola a fost schimbată. De la următoarea autentificare se folosește parola nouă.", NoticeKind.Success)
            Else
                ntfParola.Show("Parola a fost schimbată pe serverul K-BOT. Serverul vechi (Access) NU a putut fi actualizat — " &
                               "acolo rămâne parola veche până o aliniază un administrator.", NoticeKind.Warning)
            End If
            RaiseEvent StatusChanged("Parola a fost schimbată.")
        Catch ex As ApiException
            ntfParola.Show(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariInfoView.BtnSchimbaParola_Click", ex)
            ntfParola.Show("Parola nu a putut fi schimbată. Verificați rețeaua.", NoticeKind.Error)
        Finally
            SeteazaOcupat(False)
        End Try
    End Sub

    ' ---------------- theme ----------------

    ''' <summary>
    ''' Themed CONTAINER: the labels, buttons and text fields are the page's own and take the
    ''' generic rules first; this runs after them and only sets what those rules leave alone
    ''' (the surface the page sits on, the dim captions, the button styles).
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            tlyCont.BackColor = p.SurfaceAltColor
            tlyLicenta.BackColor = p.SurfaceAltColor
            tlyParola.BackColor = p.SurfaceAltColor
            For Each caption As Label In New Label() {lblOperatorCaption, lblUnitateCaption, lblRolCaption,
                                                      lblBazaCaption, lblTipCaption, lblVersiuneCaption,
                                                      lblParolaActuala, lblCod, lblParolaNoua, lblParolaConfirmare}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplySecondary(btnActualizari, scheme)
            ButtonStyles.ApplySecondary(btnTrimiteCod, scheme)
            ButtonStyles.ApplyPrimary(btnSchimbaParola, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariInfoView.ApplyTheme", ex)
        End Try
    End Sub

End Class
