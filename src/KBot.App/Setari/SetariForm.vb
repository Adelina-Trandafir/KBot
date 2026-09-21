Option Strict On
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The settings window (slice 0072): the same shape as the shell -- caption bar, a
''' <see cref="KBotNavList"/> on the left, one page at a time on the right, a status band
''' below. Six pages, created lazily at first activation like the shell's views:
''' «Informații» (operator, licence, updates, password), «Aplicație» (switches, documents,
''' folders), «FOREXE», «Temă», «Autentificare» and -- pinned at the bottom of the list --
''' «Jurnal», the log viewer (slice 0072-01; it used to be a window of its own).
'''
''' <para><b>Modeless, one instance.</b> Opened from the shell's options menu and owned by
''' it; a second request brings the open window to the front (see <see cref="ShowFor"/>),
''' the same rule as <c>ThemeOptionsForm</c> -- two windows editing the same file would
''' contradict each other.</para>
'''
''' <para><b>Every setting saves as it changes.</b> There is no «Salvează» for the window
''' as a whole; each page writes its own store the moment a value is confirmed, and says so
''' on the status band. «Închide» just closes.</para>
''' </summary>
Public Class SetariForm

    Private ReadOnly _session As SessionContext
    Private ReadOnly _authApi As IAuthApi
    Private ReadOnly _updates As AppUpdateService
    Private ReadOnly _controller As ForexeController
    Private ReadOnly _apiOptions As ApiOptions
    Private ReadOnly _apiClient As IApiClient

    ' Pages created lazily (key -> instance); one is visible.
    Private ReadOnly _views As New Dictionary(Of String, ISetariView)()
    Private _activeView As ISetariView

    Public Sub New(session As SessionContext, authApi As IAuthApi, updates As AppUpdateService,
                   controller As ForexeController, apiOptions As ApiOptions, apiClient As IApiClient)
        ArgumentNullException.ThrowIfNull(session)
        ArgumentNullException.ThrowIfNull(authApi)
        ArgumentNullException.ThrowIfNull(updates)
        ArgumentNullException.ThrowIfNull(controller)
        ArgumentNullException.ThrowIfNull(apiOptions)
        ArgumentNullException.ThrowIfNull(apiClient)
        InitializeComponent()
        _session = session
        _authApi = authApi
        _updates = updates
        _controller = controller
        _apiOptions = apiOptions
        _apiClient = apiClient
    End Sub

    ''' <summary>
    ''' Shows the window modeless, owned by <paramref name="host"/>; if one is already open
    ''' for that host it is brought to the front instead.
    ''' </summary>
    Public Shared Function ShowFor(host As Form, factory As Func(Of SetariForm)) As SetariForm
        ArgumentNullException.ThrowIfNull(host)
        ArgumentNullException.ThrowIfNull(factory)
        Try
            For Each f As Form In host.OwnedForms
                Dim existing As SetariForm = TryCast(f, SetariForm)
                If existing IsNot Nothing Then
                    existing.BringToFront()
                    existing.Activate()
                    Return existing
                End If
            Next
            Dim window As SetariForm = factory()
            window.Show(host)
            Return window
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForm.ShowFor", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Brings a page to the front by its nav key ("jurnal" from the shell's «Arată jurnal»
    ''' row). Unknown key -> ArgumentException, from the nav list itself: a row added to the
    ''' menu and forgotten here must be seen, not swallowed.
    ''' </summary>
    Public Sub ShowPage(key As String)
        Try
            If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Cheia paginii lipsește.", NameOf(key))
            navViews.SelectedKey = key
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForm.ShowPage", ex)
            Throw
        End Try
    End Sub

    Private Sub SetariForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.KeyPreview = True   ' Escape closes: there is no native X on a borderless window
            ' The first page. Assigning here (not in the designer) is what raises
            ' SelectionChanged and therefore creates it -- same reasoning as the shell.
            navViews.SelectedKey = "info"
        Catch ex As Exception
            ' UI boundary (Load): log and swallow.
            GlobalErrorLog.Write("SetariForm.SetariForm_Load", ex)
        End Try
    End Sub

    ' Every page created so far gets a say: the theme page may hold unsaved scheme edits.
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            MyBase.OnFormClosing(e)
            If e.Cancel Then Return
            For Each view As ISetariView In _views.Values
                If Not view.CanClose() Then
                    e.Cancel = True
                    Return
                End If
            Next
        Catch ex As Exception
            ' UI boundary: a throw from closing would leave the window stuck.
            GlobalErrorLog.Write("SetariForm.OnFormClosing", ex)
        End Try
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Try
            MyBase.OnKeyDown(e)
            If e.KeyCode = Keys.Escape Then Close()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForm.OnKeyDown", ex)
        End Try
    End Sub

    ' ---------------- pages ----------------

    Private Sub NavViews_SelectionChanged(key As String) Handles navViews.SelectionChanged
        Try
            ActivateView(key)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForm.NavViews_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub ActivateView(key As String)
        Try
            Dim view As ISetariView = Nothing
            If Not _views.TryGetValue(key, view) Then
                view = CreateView(key)
                Dim ctrl As Control = DirectCast(view, Control)
                ctrl.Dock = DockStyle.Fill
                ctrl.Visible = False
                viewHost.Controls.Add(ctrl)
                ThemeManager.Apply(ctrl)
                AddHandler view.StatusChanged, AddressOf View_StatusChanged
                AddHandler view.BusyChanged, AddressOf View_BusyChanged
                _views(key) = view
            End If

            Dim previous As ISetariView = _activeView
            _activeView = view
            DirectCast(view, Control).Visible = True
            If previous IsNot Nothing AndAlso Not ReferenceEquals(previous, view) Then
                DirectCast(previous, Control).Visible = False
            End If
            SetStatus(String.Empty)
            view.Activated()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForm.ActivateView", ex)
            Throw
        End Try
    End Sub

    Private Function CreateView(key As String) As ISetariView
        Try
            Select Case key
                Case "info" : Return New SetariInfoView(_session, _authApi, _updates, AddressOf InchideAplicatia)
                Case "aplicatie" : Return New SetariAplicatieView()
                Case "forexe" : Return New SetariForexeView(_controller)
                Case "pagina" : Return New SetariPaginaView(_controller)
                Case "tema" : Return New SetariTemaView()
                Case "autentificare" : Return New SetariAutentificareView(_apiOptions)
                Case "jurnal" : Return New SetariJurnalView() With {.ApiClient = _apiClient}
                Case "foldere" : Return New SetariFolder()
                Case Else
                    Throw New ArgumentException($"Pagină de setări necunoscută: '{key}'.", NameOf(key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForm.CreateView", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The update flow said the process must exit now (updater started, or a mandatory
    ''' update refused). The window closes and the application follows -- the same handover
    ''' the startup path does by returning from Main.
    ''' </summary>
    Private Sub InchideAplicatia()
        Try
            Close()
            Application.Exit()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForm.InchideAplicatia", ex)
        End Try
    End Sub

    ' ---------------- status band ----------------

    Private Sub View_StatusChanged(text As String)
        Try
            SetStatus(text)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForm.View_StatusChanged", ex)
        End Try
    End Sub

    Private Sub View_BusyChanged(busy As Boolean)
        Try
            busyBar.Running = busy
            Me.UseWaitCursor = busy
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForm.View_BusyChanged", ex)
        End Try
    End Sub

    Private Sub SetStatus(text As String)
        lblStatus.Text = If(text, String.Empty)
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Close()
    End Sub

    ' ---------------- theme ----------------

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p As ThemePalette = ThemeManager.Current.Palette
            ' The form background IS the 1px outline of the window (Padding(1, 2, 1, 2)).
            BackColor = p.BorderColor
            lblStatus.ForeColor = p.TextDimColor
            ButtonStyles.ApplySecondary(btnClose, ThemeManager.Current)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' The two cards (root and status) draw the same 1px top line the shell's cards draw.
    Private Sub PnlStatus_Paint(sender As Object, e As PaintEventArgs) Handles pnlStatus.Paint
        Try
            Using pen As New Pen(ThemeManager.Current.Palette.BorderColor)
                e.Graphics.DrawLine(pen, 0, 0, pnlStatus.Width, 0)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("SetariForm.PnlStatus_Paint", ex)
        End Try
    End Sub

End Class
