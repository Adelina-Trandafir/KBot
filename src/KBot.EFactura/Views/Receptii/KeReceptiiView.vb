Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>What the free place under the tree of <see cref="KeReceptiiView"/> is used for.</summary>
Public Enum KeJokerUse
    ''' <summary>The host's table of angajamente; the tree stops over it.</summary>
    Table = 0

    ''' <summary>The tree runs down over it.</summary>
    TreeDown = 1

    ''' <summary>The grid of classifications runs under the tree, to the left edge.</summary>
    GridLeft = 2
End Enum

Public Class KeReceptiiView
    Implements IKeAngajamenteHost

    Private Shared ReadOnly _ro As CultureInfo = DdfSendInputs.CulturaSite
    Private Const ColFactura As String = "factura"
    Private Const ColLDenumire As String = "denumire"
    Private Const ColLRec As String = "rec"
    Private Const ColLClsf As String = "clsf"
    Private Const ColAClsf As String = "clsf"
    Private Const ColABuget As String = "buget"
    Private Const ColADisponibil As String = "disponibil"
    Private Const ColAReceptii As String = "receptii"
    Private Const ColARamas As String = "ramas"

    ' Slice 02EF-02: the three columns that are per ANGAJAMENT (the angajament in use), next to the ones per classification.
    Private Const ColAAngRezervari As String = "angRezervari"
    Private Const ColAAngReceptii As String = "angReceptii"
    Private Const ColAAngDisponibil As String = "angDisponibil"
    Private Const ColAValRecep As String = "valRecep"
    Private Const ColARestRecep As String = "restRecep"
    Private Const ColRData As String = "data"
    Private Const ColRDescriere As String = "descriere"
    Private Const ColRTotal As String = "total"
    Private Const ColRRest As String = "rest"
    Private Const NeAleasa As String = "(nealeasă)"
    Private Const Tolerance As Decimal = 0.005D

    ' What a row of the table of invoices stands for. Slice KE20-04: an ALLOCATION of the invoice (a classification and a sum), no longer a line of it; the invoice is
    ' covered on its total. A row without a classification is the empty one at the end, where a new classification and sum can be chosen.
    Private NotInheritable Class LineTag
        Public Property Invoice As EFacturaPrimita

        ' The classification the invoice's lines had stored when the row was made (used while no indicator list exists yet to name it).
        Public Property InitClsf As Integer?

        ' The accountant typed the amount of this row: it is no longer proposed.
        Public Property Edited As Boolean
    End Class

    Private _api As IEFacturaApi
    Private _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _mode As String = EFacturaKeTip.Noua
    Private _year As Integer
    Private Const TabNoua As String = "noua"            ' «Adaugă»
    Private Const TabAsociate As String = "asociate"    ' «Asociate»: only while a ticked invoice has receptions linked
    Private Const TabExistente As String = "existente"  ' «Existente»

    ' Slice 02EF-07: the tab «Asociate» is on (the grid lists the receptions the ticked invoices are linked to; nothing is done there).
    Private _assocTab As Boolean
    Private ReadOnly _actionKeys As String() = {EFacturaKeTip.Existenta, EFacturaKeTip.Modificata}
    Private ReadOnly _actionLabels As String() = {"Leagă facturile de recepție", "Modifică recepția (valorile cresc)"}
    Private WithEvents navMod As KBotNavList
    Private WithEvents pgNoua As PrimiteReceptieNouaView
    Private WithEvents pgExistente As PrimiteReceptiiExistenteView
    Private WithEvents pgAsociate As PrimiteReceptiiAsociateView
    Private _loadingMode As Boolean
    Private _busy As Boolean
    Private ReadOnly _tipuri As String() = {"Avans", "Partial", "Final"}
    Private ReadOnly _byLabel As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _labelOf As New Dictionary(Of Integer, String)()
    Private ReadOnly _states As New Dictionary(Of Integer, EFacturaKeStare)()
    Private ReadOnly _invoices As New Dictionary(Of Integer, EFacturaPrimita)()

    Private _checked As New List(Of Integer)()
    Private _syncSeq As Integer

    ' Slice KE12-04: the embedded variant (inside a view of the invoices): ONE invoice and, in the view of an angajament, ONE angajament are given; their choosers are not shown.
    Private _embedded As Boolean
    Private _fixedAng As String = String.Empty
    Private _pendingNotice As String = String.Empty

    Private _filling As Boolean
    Private _fitShown As Boolean
    Private _dataAuto As Date = Date.MinValue

    ' Slice KE20-01: per classification, the value a NEW / MODIFIED reception takes ABOVE the lines of the ticked invoices (always > 0 here). It is typed in «Valoare» of the grid of
    ' classifications, never written back on the lines; «Valoare» there = the lines + this, so when a line goes down by 50 the value goes down by 50 and the surplus stays.
    Private ReadOnly _extra As New Dictionary(Of Integer, Decimal)()

    ' Slice KE20-02: «Recepții existente» with «Leagă» lists the receptions as they stood on the date of the (oldest) ticked invoice: _recAt is that answer, _recAtKey the angajament and
    ' day it belongs to, _recShownKey what the grid of receptions shows now. Nothing here for the other modes (they work on the receptions as they are).
    Private _recAt As EFacturaKeReceptiiAngajament
    Private _recAtKey As String = String.Empty
    Private _recShownKey As String = String.Empty
    Private _recAtSeq As Integer
    Private ReadOnly _recAtCache As New Dictionary(Of String, EFacturaKeReceptiiAngajament)(StringComparer.OrdinalIgnoreCase)

    ' Slice KE12-03: the budget of each indicator on the day of the reception and the receptions before that day (by classification), for the angajament and day in _dispoKey.
    Private ReadOnly _dispo As New Dictionary(Of Integer, EFacturaKeDisponibil)()
    Private ReadOnly _alocDisponibil As New Dictionary(Of KBotDataRow, EFacturaKeDisponibil)()
    Private ReadOnly _dispoCache As New Dictionary(Of String, EFacturaKeDisponibilZi)(StringComparer.OrdinalIgnoreCase)
    Private _dispoKey As String = String.Empty
    Private _dispoWarning As String = String.Empty
    Private _dispoSeq As Integer

    ' Slice 02EF-11: the reception this page saved last (angajament + snapshot); the ribbon button «Genereaza ordonantare» is built on it. Nothing = no reception saved
    ' since the operator last ticked an invoice.
    Private _ordCod As String = String.Empty
    Private _ordIdrh As Integer
    Private _descrAuto As String = String.Empty

    ' Slice KE14: the invoice on screen is one a reception already covers (pressed in the tree): its products, the reception it is linked to, nothing can be changed, «Elimină asociere».
    Private _viewing As EFacturaPrimita
    Private _viewState As EFacturaKeStare
    Private _viewSeq As Integer
    Private ReadOnly _viewDispo As New Dictionary(Of String, EFacturaKeDisponibilZi)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Embedded variant only: a reception was made or linked; the host reads the invoice again and gives it back through <see cref="ShowInvoice"/>.</summary>
    Public Event Committed As EventHandler

    Private Sub BindEditor()
        navMod = editorReceptii.navMod
        pgNoua = editorReceptii.pgNoua
        pgExistente = editorReceptii.pgExistente
        pgAsociate = editorReceptii.pgAsociate
    End Sub

    ' Columns that must never be cut: the date of a reception, the amounts (up to 9.999.999,99), with the padding of the cell and of the header. The width is
    ' measured with the font of the grid and kept in LOGICAL pixels (the grids scale it with the screen); what is left over goes to the fill column. The
    ' columns that say WhenRoom are shown only when the others leave this much, so no column is ever squeezed below it.
    Private Sub ReserveColumnWidths()
        Try
            ReserveWidth(pgExistente.gridReceptii, ColRData, "31.12.2026")
            ReserveWidth(pgExistente.gridReceptii, ColRTotal, "9.999.999,99")
            ReserveWidth(pgExistente.gridReceptii, ColRRest, "9.999.999,99")
            ReserveWidth(pgAsociate.gridReceptii, ColRData, "31.12.2026")
            ReserveWidth(pgAsociate.gridReceptii, ColRTotal, "9.999.999,99")
            pgAsociate.gridReceptii.Column(ColRDescriere).MinWidth = 60
            For Each k_key As String In New String() {ColABuget, ColAReceptii, ColADisponibil, ColAAngRezervari, ColAAngReceptii, ColAAngDisponibil, ColAValRecep, ColARamas, ColARestRecep}
                ReserveWidth(gridAloc, k_key, "-9.999.999,99")
            Next
            gridAloc.Column(ColAClsf).MinWidth = 120
            gridAloc.Column(ColAClsf).Width = 150
            pgExistente.gridReceptii.Column(ColRDescriere).MinWidth = 60
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.ReserveColumnWidths", ex)
        End Try
    End Sub

    Private Shared Sub ReserveWidth(k_grid As KBotDataView, k_key As String, k_sample As String)
        Dim k_col As KBotDataColumn = k_grid.Column(k_key)
        Dim k_scale As Single = Math.Max(1.0F, k_grid.DeviceDpi / 96.0F)
        Dim k_text As Integer = TextRenderer.MeasureText(k_sample, k_grid.Font).Width + CInt(Math.Ceiling((k_col.CellPadding.Left + k_col.CellPadding.Right) * k_scale))
        Dim k_head As Integer
        Using k_bold As New System.Drawing.Font(k_grid.Font, System.Drawing.FontStyle.Bold)
            k_head = TextRenderer.MeasureText(k_col.HeaderText, k_bold).Width + CInt(Math.Ceiling(2 * 8 * k_scale))
        End Using
        ' Back to logical pixels, with a little air for the rounding of the two conversions.
        Dim k_need As Integer = CInt(Math.Ceiling(Math.Max(k_text, k_head) / k_scale)) + 4
        k_col.MinWidth = Math.Max(k_col.MinWidth, k_need)
        k_col.Width = Math.Max(k_col.Width, k_need)
    End Sub

    Private WithEvents angajamente As KeAngajamenteView

    ''' <summary>A work is running (nothing can be changed, nothing is sent twice): the host can lock what it holds around the view.</summary>
    Public Event BusyChanged As EventHandler

    ''' <summary>The standard delete button of a reception row of an invoice only looked at: the host asks, and takes the links off.</summary>
    Public Event UnlinkRequested(k_invoice As EFacturaPrimita, k_links As EFacturaKeLegatura())

    Friend ReadOnly Property Busy As Boolean
        Get
            Return _busy
        End Get
    End Property

    ''' <summary>Designer only (the hosts call <see cref="Initialize"/>).</summary>
    Public Sub New()
        InitializeComponent()
        BindEditor()
        _gate = ReauthGate.Direct
    End Sub

    ''' <summary>Once, before use: the server, the gate and the year chosen in K-BOT (0 = all years).</summary>
    Public Sub Initialize(k_api As IEFacturaApi, k_gate As ReauthGate, k_year As Integer)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        _api = k_api
        _gate = k_gate
        _year = k_year
        gridAloc.CellTooltipTableProvider = AddressOf AvailabilityTooltip
        pgNoua.cmbTip.Items.AddRange(_tipuri.Cast(Of Object)())
        pgNoua.cmbTip.SelectedIndex = Array.IndexOf(_tipuri, "Final")
        _loadingMode = True
        pgExistente.cmbMod.Items.AddRange(_actionLabels.Cast(Of Object)())
        pgExistente.cmbMod.SelectedIndex = 0
        navMod.SelectedKey = TabNoua
        _loadingMode = False
        ReserveColumnWidths()
        UpdateAssociatedTab()
        ApplyMode()
    End Sub

    ''' <summary>
    ''' The table of angajamente the host holds under its tree. The view gets its angajament in use from it (the code and the data of the receptions and indicators) and the table is told
    ''' which invoices are in discussion; the host does the placing of both.
    ''' </summary>
    Public Sub AttachAngajamente(k_table As KeAngajamenteView)
        ArgumentNullException.ThrowIfNull(k_table)
        angajamente = k_table
        angajamente.Initialize(_api, _gate, _cts, Me)
        FillAngajamente()
    End Sub

    Private _joker As KeJokerUse = KeJokerUse.Table

    ''' <summary>
    ''' What the free place under the tree (the «joker») is used for: the host's table of angajamente (<see cref="KeJokerUse.Table"/>, the tree stops over it), the tree itself
    ''' (<see cref="KeJokerUse.TreeDown"/>) or the grid of classifications, which then runs under the tree, to the left edge (<see cref="KeJokerUse.GridLeft"/>).
    ''' </summary>
    Public Property JokerUse As KeJokerUse
        Get
            Return _joker
        End Get
        Set(k_value As KeJokerUse)
            If k_value = _joker Then Return
            _joker = k_value
            ApplyJoker()
        End Set
    End Property

    ''' <summary>The place of the host's tree (left, over the three upper rows; four with <see cref="KeJokerUse.TreeDown"/>).</summary>
    Friend ReadOnly Property TreePane As Panel
        Get
            Return pnlTree
        End Get
    End Property

    ''' <summary>The place of the host's bar of pages (top, over the details and the data of the reception).</summary>
    Friend ReadOnly Property NavPane As Panel
        Get
            Return pnlNav
        End Get
    End Property

    ''' <summary>The joker place (left, under the tree): the table of angajamente goes here.</summary>
    Friend ReadOnly Property JokerPane As Panel
        Get
            Return pnlJoker
        End Get
    End Property

    Private Sub ApplyJoker()
        pnlJoker.Visible = _joker = KeJokerUse.Table
        tlyRec.SetRowSpan(pnlTree, If(_joker = KeJokerUse.TreeDown, 4, 3))
        If _joker = KeJokerUse.GridLeft Then
            tlyRec.SetCellPosition(gridAloc, New TableLayoutPanelCellPosition(0, 3))
            tlyRec.SetColumnSpan(gridAloc, 3)
        Else
            tlyRec.SetColumnSpan(gridAloc, 2)
            tlyRec.SetCellPosition(gridAloc, New TableLayoutPanelCellPosition(1, 3))
        End If
    End Sub

    ' The static event of the robot holds the view: it is taken off again when the view goes (HandleDestroyed / Shutdown).
    Private Sub KeReceptiiView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            RemoveHandler KeRobot.DateAngajamentSchimbate, AddressOf KeRobot_DateAngajamentSchimbate
            AddHandler KeRobot.DateAngajamentSchimbate, AddressOf KeRobot_DateAngajamentSchimbate
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.KeReceptiiView_Load", ex)
        End Try
    End Sub

    ''' <summary>The host is closing: the answers on their way are dropped.</summary>
    Public Sub Shutdown()
        Try
            RemoveHandler KeRobot.DateAngajamentSchimbate, AddressOf KeRobot_DateAngajamentSchimbate
            _cts.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.Shutdown", ex)
        End Try
    End Sub

    ' The view is built while the page that holds it is not on screen: the first time it is really shown (and after an invoice is put in it) the layout of the grids and of the bar
    ' «Adaugă / Asociate / Existente» is worked out again, so nothing stays as it was measured in the hidden page.
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        MyBase.OnVisibleChanged(e)
        Try
            If Visible Then RelayoutSoon()
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.OnVisibleChanged", ex)
        End Try
    End Sub

    Friend Sub RelayoutSoon()
        If IsDisposed Then Return
        If IsHandleCreated Then BeginInvoke(New Action(AddressOf Relayout)) Else Relayout()
    End Sub

    ' UI boundary (posted to the message loop): logs and swallows.
    Private Sub Relayout()
        Try
            If IsDisposed Then Return
            PerformLayout()
            pnlEditor.PerformLayout()   ' the message box under the data of the reception: its height is known only now
            For Each k_grid As KBotDataView In New KBotDataView() {gridLinii, pgExistente.gridReceptii, pgAsociate.gridReceptii, gridAloc}
                k_grid.AutoSizeColumns()
            Next
            navMod.Invalidate()
            Invalidate(True)
        Catch ex As Exception
            GlobalErrorLog.Write("KeReceptiiView.Relayout", ex)
        End Try
    End Sub

    Private Sub KeReceptiiView_HandleDestroyed(sender As Object, e As EventArgs) Handles MyBase.HandleDestroyed
        Shutdown()
    End Sub

    Private Sub ShowNotice(k_text As String, k_kind As NoticeKind) Implements IKeAngajamenteHost.ShowNotice
        ntfMesaj.Show(k_text, k_kind)
        ntfMesaj.Visible = True
        pnlEditor.PerformLayout()   ' the box stands under the data of the reception and takes its height from them
    End Sub

End Class
