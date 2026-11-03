Option Strict On
Imports System.Globalization

''' <summary>
''' The first window (slice ME01): where to connect and how to log in. The settings are remembered in
''' admin_settings.json (never the password). On a successful connect the dialog closes with OK and the
''' connected client is in <see cref="Client"/>.
''' </summary>
Public Class ConnectionForm

    Private ReadOnly _settings As AdminSettings
    Private _client As AdminClient
    Private _connecting As Boolean

    Public Sub New(k_settings As AdminSettings)
        InitializeComponent()
        Icon = AdminIcon.Value
        _settings = k_settings
        txtHost.Text = k_settings.Host
        txtPort.Text = k_settings.Port.ToString(CultureInfo.InvariantCulture)
        txtUser.Text = k_settings.User
        txtKey.Text = k_settings.PrivateKeyPath
        lblInfo.Text = SshAuth.Describe(k_settings)
    End Sub

    ''' <summary>The connected client (valid after OK). The caller owns and disposes it.</summary>
    Public ReadOnly Property Client As AdminClient
        Get
            Return _client
        End Get
    End Property

    Private Const WM_ACTIVATE As Integer = &H6
    Private Const WM_ACTIVATEAPP As Integer = &H1C
    Private Const WM_NCACTIVATE As Integer = &H86

    ''' <summary>Coming back to the program (taskbar, Alt+Tab) must show an open message box, not hide it behind this window.</summary>
    Protected Overrides Sub WndProc(ByRef m As Message)
        MyBase.WndProc(m)
        If m.Msg = SingleInstance.ShowMessage AndAlso IsHandleCreated Then
            BeginInvoke(New Action(Sub() Activate()))
            Return
        End If
        If (m.Msg = WM_ACTIVATEAPP OrElse m.Msg = WM_ACTIVATE OrElse m.Msg = WM_NCACTIVATE) AndAlso IsHandleCreated Then
            BeginInvoke(New Action(AddressOf ModalGuard.RaiseModal))
        End If
    End Sub

    Private Async Sub BtnConnect_Click(k_sender As Object, e As EventArgs) Handles btnConnect.Click
        If _connecting Then Return
        Try
            Dim k_port As Integer
            If Not Integer.TryParse(txtPort.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, k_port) OrElse
               k_port < 1 OrElse k_port > 65535 Then
                ShowProblem("Portul trebuie să fie un număr între 1 și 65535.")
                Return
            End If
            If txtHost.Text.Trim() = String.Empty OrElse txtUser.Text.Trim() = String.Empty Then
                ShowProblem("Serverul și utilizatorul sunt obligatorii.")
                Return
            End If

            _settings.Host = txtHost.Text.Trim()
            _settings.Port = k_port
            _settings.User = txtUser.Text.Trim()
            _settings.PrivateKeyPath = txtKey.Text.Trim()
            _settings.Password = txtPass.Text

            SetConnecting(True)
            Dim k_candidate As New AdminClient(_settings)
            Try
                Await k_candidate.ConnectAsync()
            Catch ex As Exception
                k_candidate.Dispose()
                ShowProblem(ex.Message)
                Return
            Finally
                SetConnecting(False)
            End Try

            If String.IsNullOrWhiteSpace(_settings.HostKeyFingerprint) AndAlso k_candidate.ObservedFingerprint <> String.Empty Then
                If UiRun.Confirm("Prima conectare la acest server." & vbLf & "Amprenta cheii lui: " & k_candidate.ObservedFingerprint & vbLf & vbLf &
                                 "O păstrez și o verific la fiecare conectare?", "KBotAdmin") Then
                    _settings.HostKeyFingerprint = k_candidate.ObservedFingerprint
                End If
            End If
            AdminSettingsStore.Save(_settings)
            _client = k_candidate
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            ' UI boundary (click handler): shown to the operator, logged, never rethrown.
            GlobalErrorLog.Write("ConnectionForm.BtnConnect_Click", ex)
            ShowProblem(ex.Message)
        End Try
    End Sub

    Private Sub SetConnecting(k_on As Boolean)
        _connecting = k_on
        busy.Running = k_on
        btnConnect.Enabled = Not k_on
        txtPass.Enabled = Not k_on
        If k_on Then
            lblInfo.ForeColor = ThemeManager.Current.Palette.TextColor
            lblInfo.Text = "Se conectează..."
        End If
    End Sub

    Private Sub ShowProblem(k_text As String)
        lblInfo.ForeColor = ThemeManager.Current.Palette.ErrorColor
        lblInfo.Text = k_text
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            lblInfo.ForeColor = ThemeManager.Current.Palette.TextColor
        Catch ex As Exception
            GlobalErrorLog.Write("ConnectionForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
