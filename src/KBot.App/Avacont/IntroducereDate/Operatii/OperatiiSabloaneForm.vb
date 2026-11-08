Option Strict On
Imports System.Globalization
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «OperatiiSabloane» window (slices AV02-03 / AV02-04): the shell. A vertical bar like the one of «Sabloane» chooses the page, one at a time: «Operatii diverse»
''' (<see cref="OperatiiDiversePage"/>, the Access form OP9) and «Facturi si plati» (<see cref="FacturiPlatiPage"/>, OPuri2017; read the first time it is shown).
''' Before the other page takes over, and before the window closes, the page in front settles what it has pending (the question of unsaved work); Cancel keeps the
''' operator where they are. The window follows the year and the subunit of the title bar as they were when it opened (it is modal). «Salarii» comes as a third page.
''' </summary>
Public Class OperatiiSabloaneForm

    Private Const KeyDiverse As String = "diverse"
    Private Const KeyFacturi As String = "facturi"

    Private _api As IOperatiiApi
    Private _sablonApi As ISablonOperatiiApi
    Private _gate As ReauthGate
    Private _session As SessionContext
    Private _idSubunitate As Integer?
    Private _subunitSs As String = String.Empty
    Private _activeKey As String = KeyDiverse
    Private _boundFacturi As Boolean
    Private _switching As Boolean
    Private _closeAfterCommit As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_api">The posting API of the two pages.</param>
    ''' <param name="k_sablonApi">The operation templates (the page «Operatii diverse» lists the ones of type A).</param>
    ''' <param name="k_session">The session: the year of the title bar.</param>
    ''' <param name="k_idSubunitate">The subunit in force; Nothing = none (the pages stay locked and say so).</param>
    ''' <param name="k_subunitSs">The sector+source of that subunit (which templates show).</param>
    Public Sub New(k_api As IOperatiiApi, k_sablonApi As ISablonOperatiiApi, k_gate As ReauthGate, k_session As SessionContext,
                   Optional k_idSubunitate As Integer? = Nothing, Optional k_subunitSs As String = Nothing)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_sablonApi)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_session)
        InitializeComponent()
        _api = k_api
        _sablonApi = k_sablonApi
        _gate = k_gate
        _session = k_session
        _idSubunitate = k_idSubunitate
        _subunitSs = If(k_subunitSs, String.Empty)
        SetCaption()
    End Sub

    ' The first page reads its data once the window is up.
    Private Sub OperatiiSabloaneForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _api Is Nothing Then Return
            pageDiverse.Bind(_api, _sablonApi, _gate, _session, _idSubunitate, _subunitSs)
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiSabloaneForm.OperatiiSabloaneForm_Load", ex)
        End Try
    End Sub

    Private ReadOnly Property ActivePage As ISablonPagina
        Get
            Return If(String.Equals(_activeKey, KeyFacturi, StringComparison.Ordinal), DirectCast(pageFacturi, ISablonPagina), DirectCast(pageDiverse, ISablonPagina))
        End Get
    End Property

    Private Sub SetCaption()
        Dim k_year As String = If(_session Is Nothing, String.Empty, " · " & _session.An.ToString(CultureInfo.InvariantCulture))
        Dim k_ss As String = String.Empty
        If _session IsNot Nothing Then
            k_ss = If(_session.ToateSursele OrElse String.IsNullOrEmpty(_session.SectorSursa), " · toate sursele", " · " & _session.SectorSursa)
        End If
        Dim k_page As String = If(String.Equals(_activeKey, KeyFacturi, StringComparison.Ordinal), "facturi și plăți", "operații diverse")
        capBar.Text = "K-BOT — Operații contabile · " & k_page & k_ss & k_year
        Text = capBar.Text
    End Sub

    ' ── The vertical bar ────────────────────────────────────────────────────────

    ' UI boundary (event handler): the page in front settles its pending work first; on Cancel the bar goes back to it.
    Private Async Sub NavOperatii_SelectionChanged(k_key As String) Handles navOperatii.SelectionChanged
        Try
            If _session Is Nothing OrElse _switching OrElse String.Equals(k_key, _activeKey, StringComparison.Ordinal) Then Return
            _switching = True
            Try
                If Not Await ActivePage.CommitAsync().ConfigureAwait(True) Then
                    navOperatii.SelectedKey = _activeKey
                    Return
                End If
                If IsDisposed Then Return
                ShowPage(k_key)
            Finally
                _switching = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiSabloaneForm.NavOperatii_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub ShowPage(k_key As String)
        _activeKey = k_key
        Dim k_isFacturi As Boolean = String.Equals(k_key, KeyFacturi, StringComparison.Ordinal)
        If k_isFacturi AndAlso Not _boundFacturi Then
            _boundFacturi = True
            pageFacturi.Bind(_api, _gate, _session, _idSubunitate)
        End If
        pageDiverse.Visible = Not k_isFacturi
        pageFacturi.Visible = k_isFacturi
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
            GlobalErrorLog.Write("OperatiiSabloaneForm.OnFormClosing", ex)
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
            GlobalErrorLog.Write("OperatiiSabloaneForm.CommitThenClose", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_p As ThemePalette = ThemeManager.Current.Palette
            ' The form background IS the 1px outline of the window (Padding(2)).
            BackColor = k_p.BorderColor
            For Each k_c As Control In {tlyMain, pnlWork, pnlHost}
                k_c.BackColor = ThemeManager.Current.PanelBack()
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("OperatiiSabloaneForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
