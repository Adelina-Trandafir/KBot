Option Strict On
Imports System.Globalization
Imports System.Linq
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>Which statements the panel shows, and so which column layouts and detail rows it uses.</summary>
Public Enum ExtrasePanelMode
    ''' <summary>One angajament's statements -- the Extrase view.</summary>
    Angajament = 0
    ''' <summary>Every statement of the database -- the «Extrase de cont» window.</summary>
    Toate = 1
End Enum

''' <summary>What the right-hand side shows for the selected node (slice 0096, tree header menu).</summary>
Public Enum ExtraseDisplayMode
    ''' <summary>Headers on top, the selected header's operations below; a day shows operations + detail.</summary>
    AntetOperatii = 0
    ''' <summary>Every node shows its period's operations on top and the selected one in full below.</summary>
    OperatiiDetalii = 1
End Enum

''' <summary>
''' The body of the Extrase view and of the «Extrase de cont» window (slice 0080-02 / 0080-03).
''' It does not talk to the API: the host loads an <see cref="ExtraseInfo"/> and hands it over
''' through <see cref="SetData"/>, so the view (one angajament) and the window (everything)
''' share one tree, one set of grids and one detail panel.
'''
''' <para><b>The tree</b> (operator, 24.09.2026): «Toate extrasele» -> month -> day, the days
''' being the <c>FX_Extrase.DataBanca</c> of the operations. A header (FX_Extrase_H) sits under
''' every day on which one of its operations has a DataBanca; a header with no operation (only
''' in the window) sits on its statement's <c>DataExtras</c>.</para>
'''
''' <para><b>Toate / month selected</b>: the top grid lists the headers, each once, with its
''' statement date in «Data»; the lower grid lists the operations of the selected header.
''' <b>Day selected</b>: the top grid lists that day's operations; the bottom shows the selected
''' one in full, like the Plăți detail panel. The window adds four rows to it.</para>
'''
''' <para><b>Display menu</b> (slice 0096, right icon of the tree header): «antet + operații» is
''' the arrangement above; «operații + detalii» shows, for EVERY node, the operations of its
''' period on top and the selected one in full below (see <see cref="ApplyDisplayColumns"/>).</para>
'''
''' <para><b>Columns</b>: every column of <see cref="ExtraseColumns"/> is authored in the
''' designer, defaults visible; the operator's choice from «Setări → Extrase» is applied over
''' them (<see cref="ApplyColumnLayouts"/>) and again whenever the settings are saved.</para>
''' </summary>
Public Class ExtrasePanel
    Implements IThemedControl

    Friend Const ROOT_KEY As String = "all"

    ' Row keys of the display menu (right icon of the tree header).
    Private Const MENU_ANTET_OPERATII As String = "antet-operatii"
    Private Const MENU_OPERATII_DETALII As String = "operatii-detalii"

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    ''' <summary>What one tree node stands for. POCO.</summary>
    Private NotInheritable Class NodeData
        Public Property IsDay As Boolean
        Public Property Antete As New List(Of ExtrasAntet)()
        Public Property Operatiuni As New List(Of ExtrasOperatiune)()
    End Class

    Private _mode As ExtrasePanelMode = ExtrasePanelMode.Angajament
    Private _info As ExtraseInfo
    Private ReadOnly _antetById As New Dictionary(Of Integer, ExtrasAntet)()
    Private ReadOnly _opsByAntet As New Dictionary(Of Integer, List(Of ExtrasOperatiune))()
    Private _current As NodeData
    Private _display As ExtraseDisplayMode = ExtraseDisplayMode.AntetOperatii

    ' Designer values of the gridZi columns the display mode switches (filter / grouping). POCO.
    Private NotInheritable Class ColumnCaps
        Public Property ShowColumnFilter As Boolean
        Public Property AllowGrouping As Boolean
        Public Property ColumnFilterIcon As Image
    End Class
    Private ReadOnly _designCaps As New Dictionary(Of String, ColumnCaps)(StringComparer.Ordinal)

    Private _splitterDistanceDesfasurat As Integer
    Private _panel1MinSizeDesfasurat As Integer

    ''' <summary>The right icon of the tree footer was pressed: the host downloads the statements.</summary>
    Public Event DescarcaCerut As EventHandler

    Public Sub New()
        InitializeComponent()
        For Each key As String In New String() {ExtraseColumns.ODataBanca, ExtraseColumns.OPlatitor, ExtraseColumns.OCui}
            Dim c As KBotDataColumn = gridZi.Column(key)
            _designCaps(key) = New ColumnCaps() With {.ShowColumnFilter = c.ShowColumnFilter,
                                                      .AllowGrouping = c.AllowGrouping,
                                                      .ColumnFilterIcon = c.ColumnFilterIcon}
        Next
        ApplyMode()
        ApplyDisplayColumns()
    End Sub

    ''' <summary>Angajament (the view) or Toate (the window). Chooses the layouts and the detail rows.</summary>
    <ComponentModel.DefaultValue(GetType(ExtrasePanelMode), "Angajament")>
    Public Property Mode As ExtrasePanelMode
        Get
            Return _mode
        End Get
        Set(value As ExtrasePanelMode)
            If _mode = value Then Return
            _mode = value
            ApplyMode()
        End Set
    End Property

    ''' <summary>The download icon in the tree footer. Off where the host offers a real button instead.</summary>
    <ComponentModel.DefaultValue(True)>
    Public Property ShowDownloadIcon As Boolean
        Get
            Return tree.FooterRightIcon IsNot Nothing
        End Get
        Set(value As Boolean)
            If value Then
                tree.FooterRightIcon = My.Resources.Resources.Jonas_Rask_Danish_Royalty_Free_Refresh_32
                tree.FooterRightIconTooltip = "Descarcă extrasele de cont (SNM) din FOREXE." & vbLf &
                                              "Se conectează întâi, dacă nu există sesiune."
            Else
                tree.FooterRightIcon = Nothing
                tree.FooterRightIconTooltip = String.Empty
            End If
        End Set
    End Property

    Private Sub ApplyMode()
        Try
            Dim extra As Boolean = (_mode = ExtrasePanelMode.Toate)
            For Each lbl As Label In New Label() {capCodAngajament, valCodAngajament, capIndicator, valIndicator,
                                                  capReferintaDest, valReferintaDest, capCodProgram, valCodProgram}
                lbl.Visible = extra
            Next
            ApplyColumnLayouts()
        Catch ex As Exception
            GlobalErrorLog.Write("ExtrasePanel.ApplyMode", ex)
            Throw
        End Try
    End Sub

    Private ReadOnly Property HeaderGrid As ExtraseGrid
        Get
            Return If(_mode = ExtrasePanelMode.Toate, ExtraseGrid.WindowHeaders, ExtraseGrid.ViewHeaders)
        End Get
    End Property

    Private ReadOnly Property OperationGrid As ExtraseGrid
        Get
            Return If(_mode = ExtrasePanelMode.Toate, ExtraseGrid.WindowOperations, ExtraseGrid.ViewOperations)
        End Get
    End Property

    ' ── Settings ────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Applies the operator's column choice to the three grids and repaints what is selected.
    ''' Skipped in the designer: there the authored defaults are what should be seen.
    ''' </summary>
    Public Sub ApplyColumnLayouts()
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            Dim s As AppSettings = AppSettings.Current
            ExtraseLayout.Apply(gridAntete, s.ExtraseColumnsFor(HeaderGrid))
            Dim ops As List(Of String) = s.ExtraseColumnsFor(OperationGrid)
            ExtraseLayout.Apply(gridZi, ops)
            ExtraseLayout.Apply(gridOperatiuni, ops)
            If _current IsNot Nothing Then ShowNode(_current)
        Catch ex As Exception
            GlobalErrorLog.Write("ExtrasePanel.ApplyColumnLayouts", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        If Not KBotDesignTime.IsDesignTime(Me) Then AddHandler AppSettings.Changed, AddressOf AppSettings_Changed
    End Sub

    ' AppSettings.Changed is static: the subscription ends with the handle.
    Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
        RemoveHandler AppSettings.Changed, AddressOf AppSettings_Changed
        MyBase.OnHandleDestroyed(e)
    End Sub

    Private Sub AppSettings_Changed(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Not IsHandleCreated Then Return
            If InvokeRequired Then
                BeginInvoke(New Action(AddressOf ApplyColumnLayouts))
            Else
                ApplyColumnLayouts()
            End If
        Catch ex As Exception
            ' UI boundary (static event from another window): log and swallow.
            GlobalErrorLog.Write("ExtrasePanel.AppSettings_Changed", ex)
        End Try
    End Sub

    ' ── Data ────────────────────────────────────────────────────────────────

    ''' <summary>Shows <paramref name="info"/>. The root starts selected, the headers listed.</summary>
    Public Sub SetData(info As ExtraseInfo)
        Try
            _info = If(info, New ExtraseInfo())
            _antetById.Clear()
            _opsByAntet.Clear()
            For Each a As ExtrasAntet In _info.Antete
                _antetById(a.IdExh) = a
            Next
            For Each o As ExtrasOperatiune In _info.Operatiuni
                If Not o.IdFxh.HasValue Then Continue For
                Dim list As List(Of ExtrasOperatiune) = Nothing
                If Not _opsByAntet.TryGetValue(o.IdFxh.Value, list) Then
                    list = New List(Of ExtrasOperatiune)()
                    _opsByAntet(o.IdFxh.Value) = list
                End If
                list.Add(o)
            Next
            Dim root As AdvancedTreeControl.TreeItem = BuildTree()
            tree.SelectedNode = root
            ShowNode(TryCast(root.Tag, NodeData))
        Catch ex As Exception
            GlobalErrorLog.Write("ExtrasePanel.SetData", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Empties the tree and the grids.</summary>
    Public Sub ClearData()
        Try
            _info = Nothing
            _current = Nothing
            _antetById.Clear()
            _opsByAntet.Clear()
            tree.Clear()
            gridAntete.ClearRows()
            gridZi.ClearRows()
            gridOperatiuni.ClearRows()
            ShowDetailMessage("Selectați o operațiune.")
        Catch ex As Exception
            GlobalErrorLog.Write("ExtrasePanel.ClearData", ex)
            Throw
        End Try
    End Sub

    ''' <summary>True when the last <see cref="SetData"/> had neither headers nor operations.</summary>
    Public ReadOnly Property IsEmpty As Boolean
        Get
            Return _info Is Nothing OrElse (_info.Antete.Count = 0 AndAlso _info.Operatiuni.Count = 0)
        End Get
    End Property

    ' The days a header sits on: its operations' DataBanca; none -> its statement date.
    Private Function DaysOf(a As ExtrasAntet) As List(Of Date)
        Dim ops As List(Of ExtrasOperatiune) = Nothing
        Dim days As New List(Of Date)()
        If _opsByAntet.TryGetValue(a.IdExh, ops) Then
            days.AddRange(ops.Where(Function(o) o.DataBanca.HasValue).
                              Select(Function(o) o.DataBanca.Value.Date).Distinct())
        End If
        If days.Count = 0 AndAlso a.DataExtras.HasValue Then days.Add(a.DataExtras.Value.Date)
        Return days
    End Function

    Private Function BuildTree() As AdvancedTreeControl.TreeItem
        tree.Clear()
        Dim icoLuna As Image = My.Resources.Resources.calendar

        ' day -> headers placed there + that day's operations
        Dim antetePeZi As New SortedDictionary(Of Date, List(Of ExtrasAntet))()
        For Each a As ExtrasAntet In OrderedAntete(_info.Antete)
            For Each d As Date In DaysOf(a)
                Dim list As List(Of ExtrasAntet) = Nothing
                If Not antetePeZi.TryGetValue(d, list) Then
                    list = New List(Of ExtrasAntet)()
                    antetePeZi(d) = list
                End If
                list.Add(a)
            Next
        Next
        Dim opsPeZi As Dictionary(Of Date, List(Of ExtrasOperatiune)) =
            _info.Operatiuni.Where(Function(o) o.DataBanca.HasValue).
                             GroupBy(Function(o) o.DataBanca.Value.Date).
                             ToDictionary(Function(g) g.Key, Function(g) g.OrderBy(Function(o) o.IdFxe).ToList())
        ' A day may have operations whose header is absent (orphans): it still gets a node.
        For Each d As Date In opsPeZi.Keys
            If Not antetePeZi.ContainsKey(d) Then antetePeZi(d) = New List(Of ExtrasAntet)()
        Next

        Dim rootData As New NodeData() With {
            .Antete = OrderedAntete(_info.Antete),
            .Operatiuni = OrderedOperatiuni(_info.Operatiuni)}
        Dim rootItem As AdvancedTreeControl.TreeItem =
            tree.AddItem(ROOT_KEY, "Toate extrasele", pLeftIconClosed:=icoLuna, pLeftIconOpen:=icoLuna,
                         pExpanded:=True)
        rootItem.Tag = rootData
        rootItem.Bold = True

        Dim multiYear As Boolean = antetePeZi.Keys.Select(Function(d) d.Year).Distinct().Count() > 1
        For Each luna In antetePeZi.Keys.GroupBy(Function(d) New With {Key .Y = d.Year, Key .M = d.Month})
            Dim zile As List(Of Date) = luna.OrderBy(Function(d) d).ToList()
            Dim monthData As New NodeData()
            monthData.Antete = OrderedAntete(zile.SelectMany(Function(d) antetePeZi(d)).Distinct())
            Dim opsLuna As New List(Of ExtrasOperatiune)()
            For Each d As Date In zile
                Dim opsZi As List(Of ExtrasOperatiune) = Nothing
                If opsPeZi.TryGetValue(d, opsZi) Then opsLuna.AddRange(opsZi)
            Next
            monthData.Operatiuni = OrderedOperatiuni(opsLuna)
            Dim caption As String = MonthLabel(luna.Key.M) & If(multiYear, " " & luna.Key.Y.ToString(CultureInfo.InvariantCulture), String.Empty)
            Dim monthItem As AdvancedTreeControl.TreeItem =
                tree.AddItem($"L_{luna.Key.Y}_{luna.Key.M}", caption, rootItem,
                             pLeftIconClosed:=icoLuna, pLeftIconOpen:=icoLuna)
            monthItem.Tag = monthData
            monthItem.Bold = True

            For Each d As Date In zile
                Dim ops As List(Of ExtrasOperatiune) = Nothing
                If Not opsPeZi.TryGetValue(d, ops) Then ops = New List(Of ExtrasOperatiune)()
                Dim dayData As New NodeData() With {.IsDay = True, .Operatiuni = ops}
                Dim dayItem As AdvancedTreeControl.TreeItem =
                    tree.AddItem($"Z_{d:yyyyMMdd}", d.ToString("dd.MM.yyyy", _roCulture), monthItem)
                dayItem.Tag = dayData
            Next
        Next
        tree.Invalidate()
        Return rootItem
    End Function

    Private Shared Function OrderedAntete(source As IEnumerable(Of ExtrasAntet)) As List(Of ExtrasAntet)
        Return source.OrderBy(Function(a) If(a.DataExtras, Date.MaxValue)).
                      ThenBy(Function(a) a.Clsf, StringComparer.Ordinal).
                      ThenBy(Function(a) a.IdExh).ToList()
    End Function

    ' Operations of a period: by bank date (none last), then by id.
    Private Shared Function OrderedOperatiuni(source As IEnumerable(Of ExtrasOperatiune)) As List(Of ExtrasOperatiune)
        Return source.OrderBy(Function(o) If(o.DataBanca, Date.MaxValue)).
                      ThenBy(Function(o) o.IdFxe).ToList()
    End Function

    ' ── Tree -> grids ───────────────────────────────────────────────────────

    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing Then Return
            Dim data As NodeData = TryCast(pNode.Tag, NodeData)
            If data Is Nothing Then Return
            ShowNode(data)
        Catch ex As Exception
            GlobalErrorLog.Write("ExtrasePanel.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    Private Sub ShowNode(data As NodeData)
        If data Is Nothing Then Return
        _current = data
        If data.IsDay OrElse _display = ExtraseDisplayMode.OperatiiDetalii Then
            gridAntete.Visible = False
            gridZi.Visible = True
            gridOperatiuni.Visible = False
            detailPane.Visible = True
            FillOperatiuni(gridZi, data.Operatiuni)
            If gridZi.RowCount > 0 Then gridZi.CurrentRowIndex = 0
            UpdateDetail(CurrentOperatiune())
        Else
            gridZi.Visible = False
            gridAntete.Visible = True
            detailPane.Visible = False
            gridOperatiuni.Visible = True
            FillAntete(data.Antete)
            ' The first header starts selected, so the lower grid is never empty for no reason.
            If gridAntete.RowCount > 0 Then gridAntete.CurrentRowIndex = 0
            FillOperatiuniOfSelectedAntet()
        End If
    End Sub

    Private Sub FillAntete(rows As List(Of ExtrasAntet))
        gridAntete.BeginUpdate()
        Try
            gridAntete.ClearRows()
            For Each a As ExtrasAntet In rows
                Dim row As KBotDataRow = gridAntete.AddRow()
                row.Tag = a
                row(ExtraseColumns.HData) = If(a.DataExtras.HasValue, CObj(a.DataExtras.Value), Nothing)
                row(ExtraseColumns.HNumar) = a.NumarExtras
                row(ExtraseColumns.HClsf) = a.Clsf
                row(ExtraseColumns.HDenumire) = a.Denumire
                row(ExtraseColumns.HCont) = a.Cont
                row(ExtraseColumns.HIban) = a.CodIban
                row(ExtraseColumns.HSid) = a.Sid
                row(ExtraseColumns.HSic) = a.Sic
                row(ExtraseColumns.HRpd) = a.Rpd
                row(ExtraseColumns.HRpc) = a.Rpc
                row(ExtraseColumns.HTsd) = a.Tsd
                row(ExtraseColumns.HTsc) = a.Tsc
                row(ExtraseColumns.HSfd) = a.Sfd
                row(ExtraseColumns.HSfc) = a.Sfc
            Next
        Finally
            gridAntete.EndUpdate()
        End Try
    End Sub

    Private Sub FillOperatiuni(grid As KBotDataView, rows As List(Of ExtrasOperatiune))
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            If rows Is Nothing Then Return
            For Each o As ExtrasOperatiune In rows
                Dim row As KBotDataRow = grid.AddRow()
                row.Tag = o
                Dim antet As ExtrasAntet = Nothing
                If o.IdFxh.HasValue Then _antetById.TryGetValue(o.IdFxh.Value, antet)
                row(ExtraseColumns.ODataBanca) = If(o.DataBanca.HasValue, CObj(o.DataBanca.Value), Nothing)
                row(ExtraseColumns.ODataDoc) = If(o.DataDoc.HasValue, CObj(o.DataDoc.Value), Nothing)
                row(ExtraseColumns.OClsf) = If(antet Is Nothing, String.Empty, antet.Clsf)
                row(ExtraseColumns.ONrDoc) = o.NrDoc
                row(ExtraseColumns.OReferinta) = o.Referinta
                row(ExtraseColumns.OReferintaDest) = o.ReferintaDest
                row(ExtraseColumns.OPlatitor) = o.PlatitorNume
                row(ExtraseColumns.OCui) = o.PlatitorCui
                row(ExtraseColumns.OIban) = o.PlatitorIban
                row(ExtraseColumns.ODebit) = o.SumaDebit
                row(ExtraseColumns.OCredit) = o.SumaCredit
                row(ExtraseColumns.OCodAngajament) = o.CodContract
                row(ExtraseColumns.OIndicator) = o.RandContract
                row(ExtraseColumns.OCodProgram) = o.CodProgram
                row(ExtraseColumns.OCodAi) = o.CodAi
                row(ExtraseColumns.OExplicatii) = o.Explicatii
            Next
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    Private Sub FillOperatiuniOfSelectedAntet()
        Dim cur As KBotDataRow = gridAntete.CurrentRow
        Dim a As ExtrasAntet = If(cur Is Nothing, Nothing, TryCast(cur.Tag, ExtrasAntet))
        Dim ops As List(Of ExtrasOperatiune) = Nothing
        If a IsNot Nothing Then _opsByAntet.TryGetValue(a.IdExh, ops)
        FillOperatiuni(gridOperatiuni, If(ops, New List(Of ExtrasOperatiune)()))
    End Sub

    Private Sub GridAntete_SelectionChanged(sender As Object, e As EventArgs) Handles gridAntete.SelectionChanged
        Try
            FillOperatiuniOfSelectedAntet()
        Catch ex As Exception
            GlobalErrorLog.Write("ExtrasePanel.GridAntete_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub GridZi_SelectionChanged(sender As Object, e As EventArgs) Handles gridZi.SelectionChanged
        Try
            UpdateDetail(CurrentOperatiune())
        Catch ex As Exception
            GlobalErrorLog.Write("ExtrasePanel.GridZi_SelectionChanged", ex)
        End Try
    End Sub

    Private Function CurrentOperatiune() As ExtrasOperatiune
        Dim cur As KBotDataRow = gridZi.CurrentRow
        Return If(cur Is Nothing, Nothing, TryCast(cur.Tag, ExtrasOperatiune))
    End Function

    ' ── Detail ──────────────────────────────────────────────────────────────

    Private Sub UpdateDetail(o As ExtrasOperatiune)
        If o Is Nothing Then
            ShowDetailMessage("Selectați o operațiune.")
            Return
        End If
        valNrDoc.Text = o.NrDoc
        valDataBanca.Text = ShortDate(o.DataBanca)
        valDataDoc.Text = ShortDate(o.DataDoc)
        valReferinta.Text = o.Referinta
        valPlatitor.Text = o.PlatitorNume
        valCui.Text = o.PlatitorCui
        valIban.Text = o.PlatitorIban
        valDebit.Text = Money(o.SumaDebit)
        valCredit.Text = Money(o.SumaCredit)
        valCodAngajament.Text = o.CodContract
        valIndicator.Text = o.RandContract
        valReferintaDest.Text = o.ReferintaDest
        valCodProgram.Text = o.CodProgram
        valExplicatii.Text = o.Explicatii
        lblDetailMessage.Visible = False
        detailTable.Visible = True
    End Sub

    Private Sub ShowDetailMessage(message As String)
        lblDetailMessage.Text = message
        detailTable.Visible = False
        lblDetailMessage.Visible = True
    End Sub

    ' ── Display mode (tree header menu, slice 0096) ─────────────────────────

    ''' <summary>The right icon of the tree header opens the display menu under it.</summary>
    Private Sub Tree_HeaderRightIconClicked(e As MouseEventArgs) Handles tree.HeaderRightIconClicked
        Try
            ' A second press on the icon CLOSES the menu (same as the main tree).
            If CustomPopup.ClosedJustNow Then Return
            Dim ancora As Rectangle = tree.HeaderRightIconRect
            If ancora.IsEmpty Then Return
            Dim rows As New List(Of CustomPopupItem) From {
                New CustomPopupItem(MENU_ANTET_OPERATII, "Arată &antet + operații", My.Resources.Resources.cells) With {
                    .Checked = _display = ExtraseDisplayMode.AntetOperatii},
                New CustomPopupItem(MENU_OPERATII_DETALII, "Arată &operații + detalii",
                                    My.Resources.Resources.Wefunction_Woofunction_Window_app_list_info_32) With {
                    .Checked = _display = ExtraseDisplayMode.OperatiiDetalii}
            }
            ' NOT in a «Using»: shown modeless, the popup disposes itself when it closes.
            Dim menu As New CustomPopup(rows)
            AddHandler menu.ItemClicked, AddressOf DisplayMenu_ItemClicked
            menu.ShowBelow(tree, ancora)
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("ExtrasePanel.Tree_HeaderRightIconClicked", ex)
        End Try
    End Sub

    Private Sub DisplayMenu_ItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Try
            Select Case e.Item.Key
                Case MENU_ANTET_OPERATII
                    DisplayMode = ExtraseDisplayMode.AntetOperatii
                Case MENU_OPERATII_DETALII
                    DisplayMode = ExtraseDisplayMode.OperatiiDetalii
                Case Else
                    ' No silent no-ops: a row added to the menu and forgotten here must show.
                    Throw New ArgumentException("Rând necunoscut în meniul extraselor: «" & e.Item.Key & "».")
            End Select
        Catch ex As Exception
            ' UI boundary (popup event): log and swallow.
            GlobalErrorLog.Write("ExtrasePanel.DisplayMenu_ItemClicked", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Headers + operations (as before) or operations + detail for every node. The selected
    ''' node is shown again in the new mode.
    ''' </summary>
    <ComponentModel.Browsable(False)>
    <ComponentModel.DesignerSerializationVisibility(ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property DisplayMode As ExtraseDisplayMode
        Get
            Return _display
        End Get
        Set(value As ExtraseDisplayMode)
            If _display = value Then Return
            _display = value
            ApplyDisplayColumns()
            If _current IsNot Nothing Then ShowNode(_current)
        End Set
    End Property

    ''' <summary>
    ''' The operations grid on top offers more in «operații + detalii»: «Data bancă» gets the
    ''' grouping tab (only in this mode), «Plătitor» and «CUI» get filtering and grouping. Back in
    ''' the other mode the designer values return and whatever those extras set is lifted.
    ''' The menu lives behind the filter icon, so a hidden column offers nothing by itself.
    ''' </summary>
    Private Sub ApplyDisplayColumns()
        Try
            Dim detalii As Boolean = (_display = ExtraseDisplayMode.OperatiiDetalii)
            Dim filterIcon As Image = _designCaps(ExtraseColumns.OPlatitor).ColumnFilterIcon
            For Each pair As KeyValuePair(Of String, ColumnCaps) In _designCaps
                Dim c As KBotDataColumn = gridZi.Column(pair.Key)
                If detalii Then
                    c.ShowColumnFilter = True
                    c.AllowGrouping = True
                    If c.ColumnFilterIcon Is Nothing Then c.ColumnFilterIcon = filterIcon
                Else
                    If Not pair.Value.ShowColumnFilter Then gridZi.ClearColumnFilter(pair.Key)
                    c.ShowColumnFilter = pair.Value.ShowColumnFilter
                    c.ColumnFilterIcon = pair.Value.ColumnFilterIcon
                    c.AllowGrouping = pair.Value.AllowGrouping AndAlso
                                      Not String.Equals(pair.Key, ExtraseColumns.ODataBanca, StringComparison.Ordinal)
                End If
            Next
            If Not detalii Then gridZi.ClearGrouping()
            gridZi.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("ExtrasePanel.ApplyDisplayColumns", ex)
            Throw
        End Try
    End Sub

    ' ── Footer, collapse ────────────────────────────────────────────────────

    Private Sub Tree_FooterRightIconClicked(e As MouseEventArgs) Handles tree.FooterRightIconClicked
        Try
            RaiseEvent DescarcaCerut(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("ExtrasePanel.Tree_FooterRightIconClicked", ex)
        End Try
    End Sub

    ''' <summary>Same arrangement as the other views: the tree flips state, the host moves the splitter.</summary>
    Private Sub Tree_CollapsedChanged(collapsed As Boolean) Handles tree.CollapsedChanged
        Try
            Dim padStanga As Integer = split.Panel1.Padding.Left
            If collapsed Then
                _splitterDistanceDesfasurat = split.SplitterDistance
                _panel1MinSizeDesfasurat = split.Panel1MinSize
                Dim tinta As Integer = tree.MinimumCollapsedWidth + padStanga
                split.Panel1MinSize = Math.Min(_panel1MinSizeDesfasurat, tinta)
                split.SplitterDistance = ClampSplitter(tinta)
                split.IsSplitterFixed = True
            Else
                split.IsSplitterFixed = False
                If _panel1MinSizeDesfasurat > 0 Then split.Panel1MinSize = _panel1MinSizeDesfasurat
                Dim tinta As Integer = If(_splitterDistanceDesfasurat > 0,
                                          _splitterDistanceDesfasurat,
                                          tree.ExpandedWidth + padStanga)
                split.SplitterDistance = ClampSplitter(tinta)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ExtrasePanel.Tree_CollapsedChanged", ex)
        End Try
    End Sub

    Private Function ClampSplitter(dorit As Integer) As Integer
        Dim maxim As Integer = split.Width - split.Panel2MinSize - split.SplitterWidth
        If maxim < split.Panel1MinSize Then Return split.Panel1MinSize
        Return Math.Max(split.Panel1MinSize, Math.Min(dorit, maxim))
    End Function

    ' ── Formatting ──────────────────────────────────────────────────────────

    Private Shared Function Money(value As Double) As String
        Return value.ToString("N2", _roCulture)
    End Function

    Private Shared Function ShortDate(value As Date?) As String
        If Not value.HasValue Then Return String.Empty
        Return value.Value.ToString("dd.MM.yyyy", _roCulture)
    End Function

    Private Shared Function MonthLabel(month As Integer) As String
        If month < 1 OrElse month > 12 Then Return CStr(month)
        Dim name As String = _roCulture.DateTimeFormat.GetMonthName(month)
        If String.IsNullOrEmpty(name) Then Return CStr(month)
        Return Char.ToUpper(name(0), _roCulture) & name.Substring(1)
    End Function

    ' ── Theme ───────────────────────────────────────────────────────────────

    ''' <summary>
    ''' The panel owns child controls, so it themes them itself (house rule): surfaces, the
    ''' detail labels. The tree and the grids are IThemedControl and take the palette alone.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            For Each c As Control In New Control() {split, split.Panel1, split.Panel2, innerSplit,
                                                    innerSplit.Panel1, innerSplit.Panel2, detailPane, detailTable}
                c.BackColor = p.SurfaceAltColor
            Next
            tree.ApplyTheme(scheme)
            gridAntete.ApplyTheme(scheme)
            gridZi.ApplyTheme(scheme)
            gridOperatiuni.ApplyTheme(scheme)
            For Each c As Control In detailTable.Controls
                Dim lbl As Label = TryCast(c, Label)
                If lbl Is Nothing Then Continue For
                lbl.BackColor = Color.Transparent
                lbl.ForeColor = If(lbl.Name.StartsWith("cap", StringComparison.Ordinal), p.TextDimColor, p.TextColor)
            Next
            lblDetailMessage.ForeColor = p.TextDimColor
            lblDetailMessage.BackColor = p.SurfaceAltColor
        Catch ex As Exception
            ' UI boundary (theme cascade): log and swallow.
            GlobalErrorLog.Write("ExtrasePanel.ApplyTheme", ex)
        End Try
    End Sub

End Class
