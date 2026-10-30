Option Strict On
Imports System.Globalization
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Sabloane» window (slice AV01-14): the shell. A vertical bar like the one of the main window chooses the page, one at a time: «Operații cu clasificație»
''' (type M, the default: <see cref="SablonOperatiiView"/> with the operations hung under the classification) and «Operații diverse» (type A, the same page
''' with one flat list; read the first time it is shown) and «Salarii» (<see cref="SablonSalariiView"/>, the salary templates, slice AV01-15). Before the other page takes over, and before
''' the window closes, the page in front settles what it has pending (the question of unsaved changes); Cancel keeps the operator where they are.
''' </summary>
Public Class SabloaneForm

    Private Const KeyOperatiiM As String = "op_m"
    Private Const KeyOperatiiA As String = "op_a"
    Private Const KeySalarii As String = "salarii"

    Private _api As ISablonOperatiiApi
    Private _salariiApi As ISablonSalariiApi
    Private _gate As ReauthGate
    Private _session As SessionContext
    Private _activeKey As String = KeyOperatiiM
    Private _boundA As Boolean
    Private _boundSalarii As Boolean
    Private _switching As Boolean
    Private _closeAfterCommit As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_salariiApi">The salary templates («Salarii» page). Nothing = the page is not offered (the API client lacks it).</param>
    ''' <param name="k_session">The session: the sector+source of the title bar decides which templates show.</param>
    Public Sub New(k_api As ISablonOperatiiApi, k_salariiApi As ISablonSalariiApi, k_gate As ReauthGate, k_session As SessionContext)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_session)
        InitializeComponent()
        _api = k_api
        _salariiApi = k_salariiApi
        _gate = k_gate
        _session = k_session
        SetCaption()
    End Sub

    ' The default page (type M) reads its templates once the window is up.
    Private Sub SabloaneForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _api Is Nothing Then Return
            viewOperatiiM.Bind(_api, _gate, _session, "M")
        Catch ex As Exception
            GlobalErrorLog.Write("SabloaneForm.SabloaneForm_Load", ex)
        End Try
    End Sub

    Private ReadOnly Property ActivePage As ISablonPagina
        Get
            Select Case _activeKey
                Case KeyOperatiiA
                    Return viewOperatiiA
                Case KeySalarii
                    Return viewSalarii
                Case Else
                    Return viewOperatiiM
            End Select
        End Get
    End Property

    Private Sub SetCaption()
        ' The window is modal: the sector+source and the year are the ones of the title bar as it was when the window opened.
        Dim k_year As String = If(_session Is Nothing, String.Empty, " · " & _session.An.ToString(CultureInfo.InvariantCulture))
        Dim k_ss As String = String.Empty
        If _session IsNot Nothing Then
            k_ss = If(_session.ToateSursele OrElse String.IsNullOrEmpty(_session.SectorSursa), " · toate sursele", " · " & _session.SectorSursa)
        End If
        Dim k_page As String = "operații cu clasificație"
        If _activeKey = KeyOperatiiA Then k_page = "operații diverse"
        If _activeKey = KeySalarii Then k_page = "salarii"
        capBar.Text = "K-BOT — Șabloane · " & k_page & k_ss & k_year
        Text = capBar.Text
    End Sub

    ' ── The vertical bar ────────────────────────────────────────────────────────

    ' UI boundary (event handler): the page in front settles its pending changes first; on Cancel the bar goes back to it.
    Private Async Sub NavSabloane_SelectionChanged(k_key As String) Handles navSabloane.SelectionChanged
        Try
            If _session Is Nothing OrElse _switching OrElse String.Equals(k_key, _activeKey, StringComparison.Ordinal) Then Return
            _switching = True
            Try
                If Not Await ActivePage.CommitAsync().ConfigureAwait(True) Then
                    navSabloane.SelectedKey = _activeKey
                    Return
                End If
                If IsDisposed Then Return
                If String.Equals(k_key, KeySalarii, StringComparison.Ordinal) AndAlso _salariiApi Is Nothing Then
                    navSabloane.SelectedKey = _activeKey
                    KBotMessage.Show(Me, "Această instalare nu poate citi șabloanele de salarii.", "Șabloane", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return
                End If
                ShowPage(k_key)
            Finally
                _switching = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("SabloaneForm.NavSabloane_SelectionChanged", ex)
        End Try
    End Sub

    ' Brings one page in front. The second page reads its templates the first time it is shown.
    Private Sub ShowPage(k_key As String)
        _activeKey = k_key
        Dim k_isA As Boolean = String.Equals(k_key, KeyOperatiiA, StringComparison.Ordinal)
        Dim k_isSalarii As Boolean = String.Equals(k_key, KeySalarii, StringComparison.Ordinal)
        If k_isA AndAlso Not _boundA Then
            _boundA = True
            viewOperatiiA.Bind(_api, _gate, _session, "A")
        End If
        If k_isSalarii AndAlso Not _boundSalarii Then
            _boundSalarii = True
            viewSalarii.Bind(_salariiApi, _gate, _session)
        End If
        viewOperatiiM.Visible = Not k_isA AndAlso Not k_isSalarii
        viewOperatiiA.Visible = k_isA
        viewSalarii.Visible = k_isSalarii
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
            GlobalErrorLog.Write("SabloaneForm.OnFormClosing", ex)
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
            GlobalErrorLog.Write("SabloaneForm.CommitThenClose", ex)
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
            GlobalErrorLog.Write("SabloaneForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
