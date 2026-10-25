Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Plan de conturi» (slice AV01-04): the shell of the window. It loads what both views share (<see cref="PlanContContext"/>)
''' and shows ONE view at a time: <see cref="PlanContConturiView"/> (tree + fields of the account) or, after «Afiseaza solduri
''' initiale» (a button of the ribbon's view group, slice AV01-06), <see cref="PlanContSolduriView"/> (a grid of opening
''' balances; the tree and the fields are hidden then). The subunit is not chosen here any more: the chart follows the
''' sector/source of the main window's title bar, and when several subunits have that source a second selector beside it names
''' them (<see cref="IPlanContHost"/>). Before the view, the subunit, the source or the window changes, the view in front
''' settles what it has pending.
''' </summary>
Public Class PlanContForm
    Implements IViewRibbon

    Private Const Caption As String = "Plan de conturi"
    Private Const SoldTextOff As String = "Afișează solduri inițiale"
    Private Const SoldTextOn As String = "Ascunde soldurile inițiale"
    Private Const KeySolduri As String = "plan_cont_solduri"
    Private Const KeyExcel As String = "plan_cont_excel"

    Private ReadOnly _ctx As PlanContContext
    Private ReadOnly _host As IPlanContHost
    Private _soldMode As Boolean
    Private _busy As Boolean
    Private _closeAfterCommit As Boolean
    ' True once the subunits were read the first time: a change of the title bar before that is picked up by that first load itself.
    Private _subunitsLoaded As Boolean
    ' Bumped by every load of the chart: a load that finds a newer one started leaves the screen to it.
    Private _loadSeq As Integer

    Public Event RibbonStateChanged As EventHandler Implements IViewRibbon.RibbonStateChanged

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <param name="k_session">The session: the working year (the year of the opening balances) and the sector/source of the title bar.</param>
    ''' <param name="k_host">The main window: its title bar holds the subunit selector. Nothing = the window runs without it.</param>
    Public Sub New(k_api As IConturiApi, k_gate As ReauthGate, k_session As SessionContext, k_host As IPlanContHost)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_session)
        InitializeComponent()
        _ctx = New PlanContContext(k_api, k_gate, k_session)
        _host = k_host
        SetCaption()
    End Sub

    Private Sub SetCaption()
        capBar.Text = $"K-BOT — Plan de conturi · {_ctx.An.ToString(CultureInfo.InvariantCulture)}"
        Text = capBar.Text
    End Sub

    Private ReadOnly Property ActiveView As IPlanContView
        Get
            Return If(_soldMode, CType(viewSolduri, IPlanContView), CType(viewConturi, IPlanContView))
        End Get
    End Property

    Private Sub PlanContForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _ctx Is Nothing Then Return
            AddHandler viewConturi.StatusMessage, AddressOf SetStatus
            AddHandler viewSolduri.StatusMessage, AddressOf SetStatus
            AddHandler viewConturi.BusyChanged, AddressOf ViewBusyChanged
            AddHandler viewSolduri.BusyChanged, AddressOf ViewBusyChanged
            AddHandler viewConturi.PlanChanged, AddressOf ViewPlanChanged
            AddHandler viewSolduri.PlanChanged, AddressOf ViewPlanChanged
            AddHandler _ctx.Session.PeriodChanged, AddressOf SessionPeriodChanged
            If _host IsNot Nothing Then AddHandler _host.SubunitPicked, AddressOf HostSubunitPicked
            viewConturi.Bind(_ctx, Nothing)
            LoadAll()
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.PlanContForm_Load", ex)
        End Try
    End Sub

    ' The subunit selector of the title bar belongs to this window only while it is on screen (a page put away is hidden).
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        Try
            MyBase.OnVisibleChanged(e)
            If _host IsNot Nothing Then _host.SetSubunitShown(Visible)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.OnVisibleChanged", ex)
        End Try
    End Sub

    ' The title bar changed its year, its sector/source or went to / left «Toate»: the account fields follow it (SLICE-0097-06)
    ' and the chart of the subunit of that source is read again (slice AV01-06). UI boundary: started without await.
    Private Async Sub SessionPeriodChanged(sender As Object, e As PeriodChangedEventArgs)
        Try
            If IsDisposed OrElse _ctx Is Nothing Then Return
            viewConturi.RefreshSs()
            If Not _subunitsLoaded Then Return
            SetCaption()
            If Not Await ActiveView.CommitAsync().ConfigureAwait(True) Then
                SetStatus("Modificările nesalvate au rămas: planul de conturi afișat nu s-a schimbat.")
                Return
            End If
            If IsDisposed Then Return
            ApplySource(Nothing)
            Await ReloadAndBindAsync(Nothing).ConfigureAwait(True)
        Catch ex As ApiException
            GlobalErrorLog.Write("PlanContForm.SessionPeriodChanged", ex)
            If Not IsDisposed Then SetStatus(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.SessionPeriodChanged", ex)
            If Not IsDisposed Then SetStatus("Planul de conturi nu a putut fi încărcat. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            If _ctx IsNot Nothing Then RemoveHandler _ctx.Session.PeriodChanged, AddressOf SessionPeriodChanged
            If _host IsNot Nothing Then
                RemoveHandler _host.SubunitPicked, AddressOf HostSubunitPicked
                _host.SetSubunitChoices(Nothing, Nothing)
            End If
            MyBase.OnFormClosed(e)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.OnFormClosed", ex)
        End Try
    End Sub

    ' The name the title bar shows for a subunit.
    Private Shared Function SubunitateCaption(k_sub As SubunitateInfo) As String
        Return k_sub.Denumire & If(k_sub.Activ, String.Empty, " · inactivă")
    End Function

    ' ── Loading ─────────────────────────────────────────────────────────────────

    ' UI boundary: logs and shows the error; started without await. Reads the nomenclator once, the subunits, the chart.
    Private Async Sub LoadAll()
        Try
            SetBusy(True, Nothing)
            If _ctx.Nomenclator Is Nothing Then
                _ctx.SetNomenclator(Await _ctx.Gate.RunAsync(
                    Function() _ctx.Api.GetNomenclatorConturiAsync(CancellationToken.None)).ConfigureAwait(True))
                If IsDisposed Then Return
            End If
            _ctx.Subunitati = Await _ctx.Gate.RunAsync(Function() _ctx.Api.GetSubunitatiAsync(CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _subunitsLoaded = True
            ApplySource(Nothing)
            Dim k_seq As Integer = Interlocked.Increment(_loadSeq)
            Await _ctx.ReloadPlanAsync().ConfigureAwait(True)
            If IsDisposed OrElse k_seq <> _loadSeq Then Return
            ActiveView.Bind(_ctx, Nothing)
            SetChartStatus()
        Catch ex As ApiException
            GlobalErrorLog.Write("PlanContForm.LoadAll", ex)
            If Not IsDisposed Then SetStatus(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.LoadAll", ex)
            If Not IsDisposed Then SetStatus("Planul de conturi nu a putut fi încărcat. Detalii în jurnalul de erori.", NoticeKind.Error)
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    ' The subunit on screen follows the source of the title bar: the subunits of that source are the choices of the selector in
    ' the title bar (drawn only when there are two or more), and the one shown is the asked one, the one already on screen, the
    ' default one or the first. None = the source has no subunit (the chart is empty).
    Private Sub ApplySource(k_select As Integer?)
        Dim k_choices As List(Of SubunitateInfo) = _ctx.SubunitatiForSource()
        Dim k_keep As Integer? = If(k_select, _ctx.Subunitate?.IdSubunitate)
        Dim k_pick As SubunitateInfo = k_choices.FirstOrDefault(Function(k_s) k_keep.HasValue AndAlso k_s.IdSubunitate = k_keep.Value)
        If k_pick Is Nothing Then k_pick = k_choices.FirstOrDefault(Function(k_s) k_s.Implicita)
        If k_pick Is Nothing Then k_pick = k_choices.FirstOrDefault()
        _ctx.Subunitate = k_pick
        If _host IsNot Nothing Then
            _host.SetSubunitChoices(
                k_choices.Select(Function(k_s) New KeyValuePair(Of String, String)(KeyOf(k_s), SubunitateCaption(k_s))),
                If(k_pick Is Nothing, Nothing, KeyOf(k_pick)))
        End If
        RefreshButtons()
    End Sub

    Private Shared Function KeyOf(k_sub As SubunitateInfo) As String
        Return k_sub.IdSubunitate.ToString(CultureInfo.InvariantCulture)
    End Function

    ' Reads the chart of the subunit on screen and shows it. UI boundary callers catch.
    Private Async Function ReloadAndBindAsync(k_selectCont As Integer?) As Task
        SetBusy(True, Nothing)
        Try
            Dim k_seq As Integer = Interlocked.Increment(_loadSeq)
            Await _ctx.ReloadPlanAsync().ConfigureAwait(True)
            If IsDisposed OrElse k_seq <> _loadSeq Then Return
            ActiveView.Bind(_ctx, k_selectCont)
            SetChartStatus()
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    Private Function ChartStatus() As String
        If _ctx.Subunitati.Count = 0 Then Return "Unitatea nu are nicio subunitate."
        If _ctx.Subunitate Is Nothing Then Return $"Nicio subunitate nu are sursa/sectorul {_ctx.Session.SectorSursa}."
        Return $"{_ctx.Plan.Conturi.Count} conturi în {_ctx.Subunitate.Denumire}."
    End Function

    ' ── Events of the views ─────────────────────────────────────────────────────

    Private Sub ViewBusyChanged(k_busy As Boolean)
        SetBusy(k_busy, Nothing)
    End Sub

    ' The view saved or deleted an account: the chart is read again and the view shows it (with the account selected).
    Private Async Sub ViewPlanChanged(k_selectCont As Integer?)
        Try
            Dim k_seq As Integer = Interlocked.Increment(_loadSeq)
            Await _ctx.ReloadPlanAsync().ConfigureAwait(True)
            If IsDisposed OrElse k_seq <> _loadSeq Then Return
            ActiveView.Bind(_ctx, k_selectCont)
            SetChartStatus()
        Catch ex As ApiException
            GlobalErrorLog.Write("PlanContForm.ViewPlanChanged", ex)
            If Not IsDisposed Then SetStatus(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.ViewPlanChanged", ex)
            If Not IsDisposed Then SetStatus("Planul de conturi nu a putut fi reîncărcat. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' ── Subunit ─────────────────────────────────────────────────────────────────

    ' The operator picked a subunit in the title bar (several subunits share the source). The selector does not move by itself:
    ' it moves here, once the view in front has settled what it had pending. UI boundary: started without await.
    Private Async Sub HostSubunitPicked(k_key As String)
        Try
            If IsDisposed OrElse _busy OrElse _ctx Is Nothing Then Return
            Dim k_id As Integer
            If Not Integer.TryParse(k_key, NumberStyles.Integer, CultureInfo.InvariantCulture, k_id) Then Return
            Dim k_new As SubunitateInfo = _ctx.Subunitati.FirstOrDefault(Function(k_s) k_s.IdSubunitate = k_id)
            If k_new Is Nothing OrElse (_ctx.Subunitate IsNot Nothing AndAlso k_new.IdSubunitate = _ctx.Subunitate.IdSubunitate) Then Return
            If Not Await ActiveView.CommitAsync().ConfigureAwait(True) Then Return
            If IsDisposed Then Return
            _ctx.Subunitate = k_new
            _host.SetSubunitKey(k_key)
            RefreshButtons()
            Await ReloadAndBindAsync(Nothing).ConfigureAwait(True)
        Catch ex As ApiException
            GlobalErrorLog.Write("PlanContForm.HostSubunitPicked", ex)
            If Not IsDisposed Then SetStatus(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.HostSubunitPicked", ex)
            If Not IsDisposed Then SetStatus("Planul de conturi nu a putut fi încărcat. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' ── The two views ───────────────────────────────────────────────────────────

    ' «Afiseaza solduri initiale» / «Ascunde soldurile initiale»: the view in front settles what it has pending, then the other
    ' one takes its place. The tree and the fields of the account are hidden while the balances grid is in front.
    Private Async Sub ToggleSolduri()
        Try
            If _busy OrElse _ctx.Subunitate Is Nothing Then Return
            Dim k_cont As Integer? = ActiveView.CurrentContId
            If Not Await ActiveView.CommitAsync().ConfigureAwait(True) Then Return
            _soldMode = Not _soldMode
            viewConturi.Visible = Not _soldMode
            viewSolduri.Visible = _soldMode
            RaiseEvent RibbonStateChanged(Me, EventArgs.Empty)
            ActiveView.Bind(_ctx, k_cont)
            If Not _soldMode Then SetChartStatus()
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.ToggleSolduri", ex)
            If Not IsDisposed Then SetStatus("Nu s-a putut schimba vederea. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' «Exporta in Excel» (a button of the ribbon's view group): the chart of the subunit on screen with the opening balances and turnovers of the working year (what
    ' is saved: the view in front settles its pending changes first). Inactive accounts only when the box shows them.
    Private Async Sub ExportExcel()
        Try
            If _busy OrElse _ctx.Subunitate Is Nothing Then Return
            If Not Await ActiveView.CommitAsync().ConfigureAwait(True) Then Return
            Using k_dialog As New SaveFileDialog()
                k_dialog.Title = "Salvează planul de conturi ca Excel"
                k_dialog.Filter = "Excel (*.xlsx)|*.xlsx"
                k_dialog.FileName = $"Plan_conturi_{PlanContExcel.SafeName(_ctx.Subunitate.Capitol)}_{_ctx.An.ToString(CultureInfo.InvariantCulture)}.xlsx"
                k_dialog.OverwritePrompt = True
                If k_dialog.ShowDialog(Me) <> DialogResult.OK Then Return
                Try
                    System.IO.File.WriteAllBytes(k_dialog.FileName, PlanContExcel.Build(_ctx))
                Catch ex As System.IO.IOException
                    GlobalErrorLog.Write("PlanContForm.ExportExcel", ex)
                    KBotMessage.Show(Me, "Fișierul nu a putut fi scris. Este deschis în alt program sau locul nu permite scrierea.",
                                     Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End Try
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.ExportExcel", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Excelul nu a putut fi salvat. Detalii în jurnalul de erori.",
                                 Caption, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    ' The balances view saves its edited rows by itself and the accounts view asks about unsaved changes; the save cannot be
    ' awaited inside FormClosing, so the window cancels the closing, settles the view, and closes after it.
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If _ctx IsNot Nothing AndAlso Not _closeAfterCommit AndAlso e.CloseReason = CloseReason.UserClosing AndAlso
               (_soldMode OrElse ActiveView.HasPending) Then
                e.Cancel = True
                CommitThenClose()
            End If
            MyBase.OnFormClosing(e)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.OnFormClosing", ex)
        End Try
    End Sub

    ' UI boundary: started without await.
    Private Async Sub CommitThenClose()
        Try
            If Await ActiveView.CommitAsync().ConfigureAwait(True) AndAlso Not IsDisposed Then
                _closeAfterCommit = True
                Close()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.CommitThenClose", ex)
        End Try
    End Sub

    ' ── Ribbon: the button of the view group (slice AV01-06) ────────────────────

    Public ReadOnly Property RibbonCaption As String Implements IViewRibbon.RibbonCaption
        Get
            Return Caption
        End Get
    End Property

    Public ReadOnly Property RibbonKeys As IReadOnlyList(Of String) Implements IViewRibbon.RibbonKeys
        Get
            Return {KeySolduri, KeyExcel}
        End Get
    End Property

    Public Function RibbonButtonState(k_key As String) As ViewButtonState Implements IViewRibbon.RibbonButtonState
        If Not String.Equals(k_key, KeySolduri, StringComparison.Ordinal) AndAlso Not String.Equals(k_key, KeyExcel, StringComparison.Ordinal) Then Return ViewButtonState.Hidden
        Return If(_ctx IsNot Nothing AndAlso _ctx.Subunitate IsNot Nothing AndAlso Not _busy, ViewButtonState.Enabled, ViewButtonState.Dimmed)
    End Function

    Public Function RibbonButtonText(k_key As String) As String Implements IViewRibbon.RibbonButtonText
        If Not String.Equals(k_key, KeySolduri, StringComparison.Ordinal) Then Return String.Empty
        Return If(_soldMode, SoldTextOn, SoldTextOff)
    End Function

    Public Sub RunRibbonButton(k_key As String) Implements IViewRibbon.RunRibbonButton
        If String.Equals(k_key, KeySolduri, StringComparison.Ordinal) Then
            ToggleSolduri()
        ElseIf String.Equals(k_key, KeyExcel, StringComparison.Ordinal) Then
            ExportExcel()
        End If
    End Sub

    ' ── State ───────────────────────────────────────────────────────────────────

    Private Sub SetBusy(k_busy As Boolean, k_status As String)
        _busy = k_busy
        UseWaitCursor = k_busy
        RefreshButtons()
        If k_status IsNot Nothing Then SetStatus(k_status)
    End Sub

    Private Sub RefreshButtons()
        RaiseEvent RibbonStateChanged(Me, EventArgs.Empty)
    End Sub

    ' The messages of the views (hints about what to do next) are warnings; failures pass Error, results Success.
    Private Sub SetStatus(k_text As String)
        SetStatus(k_text, NoticeKind.Warning)
    End Sub

    ' Same system as OrdEditForm: the notice shows with the text and hides (no row height, the tlyMain row is AutoSize) when there is none.
    Private Sub SetStatus(k_text As String, k_kind As NoticeKind)
        If String.IsNullOrEmpty(k_text) Then
            ntfMesaj.Clear()
        Else
            ntfMesaj.Show(k_text, k_kind)
        End If
    End Sub

    ' Nothing is said while the chart is fine (no count); only the missing-subunit cases show.
    Private Sub SetChartStatus()
        If _ctx.Subunitate IsNot Nothing Then
            SetStatus(String.Empty)
        Else
            SetStatus(ChartStatus(), NoticeKind.Warning)
        End If
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            Dim k_p As ThemePalette = k_scheme.Palette
            ' The form background IS the 1px outline of the window (Padding(2)).
            BackColor = k_p.BorderColor
            For Each k_c As Control In {tlyMain, pnlCard, tlyBody, pnlHost}
                k_c.BackColor = k_p.SurfaceAltColor
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
