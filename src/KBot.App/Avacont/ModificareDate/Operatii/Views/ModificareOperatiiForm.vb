Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Modificare operatii» (slice AV02-05; the Access form ModificareNC1280 with its ribbon «MNC»): every posted operation of the subunit and year
''' in force, one row each, with the column filters of the grid (several at once), the sort remembered per user and
''' deletion of operations or documents. «Modifică operație» opens the shared accounting editor (SLICE-AV02-07).
''' The subunit and the year follow the main window's title bar.
''' Partials: .Ribbon (the view group), .Stergere (delete), .Lista (filling, icons, keys, sort, Excel).
''' </summary>
Public Class ModificareOperatiiForm
    Implements IViewRibbon

    Friend Const Caption As String = "Modificare operații"

    Private ReadOnly _api As IModificareOperatiiApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _session As SessionContext
    Private ReadOnly _host As IPlanContHost
    ' The subunit and year of the list on screen (Nothing / 0 = nothing loaded).
    Private _idSubunitate As Integer?
    Private _an As Integer
    Private _busy As Boolean
    ' Bumped by every load: a load that finds a newer one started leaves the screen to it.
    Private _loadSeq As Integer

    Public Event RibbonStateChanged As EventHandler Implements IViewRibbon.RibbonStateChanged

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <param name="k_host">The main window: the subunit in force and the operator's picks. Nothing = the page runs without a subunit.</param>
    Public Sub New(k_api As IModificareOperatiiApi, k_gate As ReauthGate, k_session As SessionContext, k_host As IPlanContHost)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_session)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _session = k_session
        _host = k_host
        SetCaption()
    End Sub

    Private Sub SetCaption()
        Dim k_an As Integer = If(_session Is Nothing, 0, _session.An)
        capBar.Text = $"K-BOT — {Caption} · {k_an.ToString(CultureInfo.InvariantCulture)}"
        Text = capBar.Text
    End Sub

    Private Sub ModificareOperatiiForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _api Is Nothing Then Return
            AddHandler _session.PeriodChanged, AddressOf SessionPeriodChanged
            If _host IsNot Nothing Then AddHandler _host.SubunitPicked, AddressOf HostSubunitPicked
            LoadList()
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.ModificareOperatiiForm_Load", ex)
        End Try
    End Sub

    ' The subunit selector is the application's: when the page comes back on screen it takes the subunit in force.
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        Try
            MyBase.OnVisibleChanged(e)
            If Visible AndAlso _api IsNot Nothing AndAlso _host IsNot Nothing AndAlso
               (Not Nullable.Equals(_host.CurrentSubunitId, _idSubunitate) OrElse _session.An <> _an) Then LoadList()
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.OnVisibleChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            If _session IsNot Nothing Then RemoveHandler _session.PeriodChanged, AddressOf SessionPeriodChanged
            If _host IsNot Nothing Then RemoveHandler _host.SubunitPicked, AddressOf HostSubunitPicked
            MyBase.OnFormClosed(e)
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.OnFormClosed", ex)
        End Try
    End Sub

    ' The title bar changed the year or the source: the subunit in force may be another one.
    Private Sub SessionPeriodChanged(sender As Object, e As PeriodChangedEventArgs)
        Try
            If IsDisposed Then Return
            SetCaption()
            If Visible Then LoadList()
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.SessionPeriodChanged", ex)
        End Try
    End Sub

    Private Sub HostSubunitPicked(k_key As String)
        Try
            If IsDisposed OrElse Not Visible Then Return
            LoadList()
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.HostSubunitPicked", ex)
        End Try
    End Sub

    ' ── Loading ─────────────────────────────────────────────────────────────────

    ''' <summary>Reads the list of the subunit and year in force again, keeping the operation on screen selected when it is still there
    ''' (or <paramref name="k_select"/>, the operation just saved, slice AV08-05).</summary>
    Friend Async Sub LoadList(Optional k_select As Integer = 0)
        Dim k_seq As Integer = Interlocked.Increment(_loadSeq)
        Try
            Dim k_sub As Integer? = If(_host Is Nothing, Nothing, _host.CurrentSubunitId)
            Dim k_an As Integer = _session.An
            Dim k_keep As Integer = If(k_select > 0, k_select, SelectedIdOper())
            _idSubunitate = k_sub
            _an = k_an
            If Not k_sub.HasValue OrElse k_an = 0 Then
                FillGrid(Array.Empty(Of OperatieContabila)(), 0)
                SetStatus("Alegeți în bara de titlu sursa (și subunitatea) ale căror operații vreți să le vedeți. Unitatea nu are o subunitate pentru sursa aleasă.",
                          NoticeKind.Warning)
                Return
            End If
            SetBusy(True)
            Dim k_list As OperatiiContabileLista = Await _gate.RunAsync(
                Function() _api.GetOperatiiContabileAsync(k_sub.Value, k_an, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed OrElse k_seq <> _loadSeq Then Return
            FillGrid(k_list.Operatii, k_keep)
            SetStatus(String.Empty, NoticeKind.Warning)
        Catch ex As ApiException
            GlobalErrorLog.Write("ModificareOperatiiForm.LoadList", ex)
            If Not IsDisposed AndAlso k_seq = _loadSeq Then SetStatus(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.LoadList", ex)
            If Not IsDisposed AndAlso k_seq = _loadSeq Then SetStatus("Lista operațiilor nu a putut fi încărcată. Detalii în jurnalul de erori.", NoticeKind.Error)
        Finally
            If Not IsDisposed AndAlso k_seq = _loadSeq Then SetBusy(False)
        End Try
    End Sub

    ' ── State ───────────────────────────────────────────────────────────────────

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        UseWaitCursor = k_busy
        RefreshButtons()
    End Sub

    Private Sub RefreshButtons()
        RaiseEvent RibbonStateChanged(Me, EventArgs.Empty)
    End Sub

    ' Same system as «Plan de conturi»: the notice shows with a text and hides when there is none.
    Private Sub SetStatus(k_text As String, k_kind As NoticeKind)
        If String.IsNullOrEmpty(k_text) Then
            ntfMesaj.Clear()
        Else
            ntfMesaj.Show(k_text, k_kind)
        End If
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            ' The form background IS the 1px outline of the window (Padding(2)).
            BackColor = k_scheme.Palette.BorderColor
            For Each k_c As Control In {tlyMain, pnlCard, tlyBody}
                k_c.BackColor = k_scheme.PanelBack()
            Next
            gridOperatii.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
