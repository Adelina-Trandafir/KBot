Option Strict On
Imports System.Globalization
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Clasificatii» window (slice 0087-01; the vertical bar and the revenue page came with slice AV01-11): the shell. A vertical bar like the one of the main
''' window chooses the page, one at a time: «Buget» (the default; <see cref="ClasificatiiBugetView"/>, the classifications of expenses with their budget,
''' what this window was before) and «Venituri» (<see cref="ClasificatiiVenituriView"/>, the revenue classifications, read the first time the page is
''' shown). Before the other page takes over, and before the window closes, the page in front settles what it has pending (the question of unsaved
''' changes); Cancel keeps the operator where they are.
''' </summary>
Public Class ClasificatiiForm

    Private Const KeyBuget As String = "buget"
    Private Const KeyVenituri As String = "venituri"

    Private _session As SessionContext
    Private _api As INomenclatoareApi
    Private _gate As ReauthGate
    Private _venituriApi As IClasificatiiVenituriApi
    Private _an As Integer
    Private _activeKey As String = KeyBuget
    Private _venituriBound As Boolean
    Private _switching As Boolean
    Private _closeAfterCommit As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_api">The expenses side («Buget» page).</param>
    ''' <param name="k_venituriApi">The revenue side («Venituri» page). Nothing = the page is not offered (the API client lacks it).</param>
    ''' <param name="k_session">The session: the working year (the year opened) and the sector+source of the title bar, which the revenue page follows.</param>
    Public Sub New(k_api As INomenclatoareApi, k_venituriApi As IClasificatiiVenituriApi, k_gate As ReauthGate, k_session As SessionContext)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_session)
        InitializeComponent()
        _gate = k_gate
        _session = k_session
        _venituriApi = k_venituriApi
        _api = k_api
        _an = k_session.An
        SetCaption()
    End Sub

    ' The default page («Buget») loads its tree once the window is up.
    Private Sub ClasificatiiForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _api Is Nothing Then Return
            viewBuget.Bind(_api, _gate, _an, _session)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.ClasificatiiForm_Load", ex)
        End Try
    End Sub

    Private ReadOnly Property ActivePage As IClasificatiiPage
        Get
            Return If(_activeKey = KeyVenituri, CType(viewVenituri, IClasificatiiPage), CType(viewBuget, IClasificatiiPage))
        End Get
    End Property

    Private Sub SetCaption()
        capBar.Text = If(_activeKey = KeyVenituri,
                         $"K-BOT — Clasificații venituri {_an.ToString(CultureInfo.InvariantCulture)}",
                         $"K-BOT — Clasificații cheltuieli {_an.ToString(CultureInfo.InvariantCulture)}")
        Text = capBar.Text
    End Sub

    ' ── The vertical bar ────────────────────────────────────────────────────────

    ' UI boundary (event handler): the page in front settles its pending changes first; on Cancel the bar goes back to it.
    Private Async Sub NavClasificatii_SelectionChanged(k_key As String) Handles navClasificatii.SelectionChanged
        Try
            If _session Is Nothing OrElse _switching OrElse String.Equals(k_key, _activeKey, StringComparison.Ordinal) Then Return
            _switching = True
            Try
                If Not Await ActivePage.CommitAsync().ConfigureAwait(True) Then
                    navClasificatii.SelectedKey = _activeKey
                    Return
                End If
                If IsDisposed Then Return
                If String.Equals(k_key, KeyVenituri, StringComparison.Ordinal) AndAlso _venituriApi Is Nothing Then
                    navClasificatii.SelectedKey = _activeKey
                    KBotMessage.Show(Me, "Această instalare nu poate citi clasificațiile de venituri.",
                                     "Clasificații", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return
                End If
                ShowPage(k_key)
            Finally
                _switching = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.NavClasificatii_SelectionChanged", ex)
        End Try
    End Sub

    ' Brings one page in front. The revenue page reads its data the first time it is shown.
    Private Sub ShowPage(k_key As String)
        _activeKey = k_key
        Dim k_venituri As Boolean = String.Equals(k_key, KeyVenituri, StringComparison.Ordinal)
        If k_venituri AndAlso Not _venituriBound Then
            _venituriBound = True
            viewVenituri.Bind(_venituriApi, _gate, _session)
        End If
        viewBuget.Visible = Not k_venituri
        viewVenituri.Visible = k_venituri
        SetCaption()
    End Sub

    ' ── Closing ─────────────────────────────────────────────────────────────────

    ' The save cannot be awaited inside FormClosing: the window cancels the closing, settles the page in front, and closes after it.
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If _session IsNot Nothing AndAlso Not _closeAfterCommit AndAlso e.CloseReason = CloseReason.UserClosing AndAlso ActivePage.HasPending Then
                e.Cancel = True
                CommitThenClose()
            End If
            MyBase.OnFormClosing(e)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.OnFormClosing", ex)
        End Try
    End Sub

    ' UI boundary: started without await.
    Private Async Sub CommitThenClose()
        Try
            If Await ActivePage.CommitAsync().ConfigureAwait(True) AndAlso Not IsDisposed Then
                _closeAfterCommit = True
                Close()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.CommitThenClose", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_p As ThemePalette = ThemeManager.Current.Palette
            ' The form background IS the 1px outline of the window (Padding(2)).
            BackColor = k_p.BorderColor
            For Each k_c As Control In {tlyMain, pnlWork, pnlHost}
                k_c.BackColor = k_p.SurfaceAltColor
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
