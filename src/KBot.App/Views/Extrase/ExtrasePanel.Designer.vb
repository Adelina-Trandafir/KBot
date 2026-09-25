Imports KBot.Controls

' The body shared by the Extrase view and the «Extrase de cont» window (slice 0080-02 / 0080-03):
' the tree of statements on the left (Toate -> month -> day, days from FX_Extrase.DataBanca),
' and on the right two stacked areas that change with the level of the selected node:
'   Toate / month : top = the headers (FX_Extrase_H), bottom = the operations of the selected header;
'   day           : top = the operations of that day (FX_Extrase), bottom = the selected one in full.
' Every column of the catalogue (KBot.Common.ExtraseColumns) is declared HERE; the defaults are
' visible, the rest Hidden, so the designer shows exactly what a new operator sees. What the
' operator picked in «Setări → Extrase» is applied over them at runtime.
' All controls are declared here (docs/kbot-forms-ui-convention.md); coordinates are in 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ExtrasePanel
    Inherits Global.KBot.Theming.KBotThemedUserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn9 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn10 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn11 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn12 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn13 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn14 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn15 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn16 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn17 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn18 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn19 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn20 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn21 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn22 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn23 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn24 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn25 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn26 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn27 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn28 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn29 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn30 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn31 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn32 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn33 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn34 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn35 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn36 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn37 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn38 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn39 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn40 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn41 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn42 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn43 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn44 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn45 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn46 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        split = New SplitContainer()
        tree = New AdvancedTreeControl()
        innerSplit = New SplitContainer()
        gridAntete = New KBotDataView()
        gridZi = New KBotDataView()
        gridOperatiuni = New KBotDataView()
        detailPane = New Panel()
        detailTable = New KBotTableLayoutPanel()
        lblDetailMessage = New Label()
        capNrDoc = New Label()
        valNrDoc = New Label()
        capDataBanca = New Label()
        valDataBanca = New Label()
        capDataDoc = New Label()
        valDataDoc = New Label()
        capReferinta = New Label()
        valReferinta = New Label()
        capPlatitor = New Label()
        valPlatitor = New Label()
        capCui = New Label()
        valCui = New Label()
        capIban = New Label()
        valIban = New Label()
        capDebit = New Label()
        valDebit = New Label()
        capCredit = New Label()
        valCredit = New Label()
        capCodAngajament = New Label()
        valCodAngajament = New Label()
        capIndicator = New Label()
        valIndicator = New Label()
        capReferintaDest = New Label()
        valReferintaDest = New Label()
        capCodProgram = New Label()
        valCodProgram = New Label()
        capExplicatii = New Label()
        valExplicatii = New Label()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        CType(innerSplit, ComponentModel.ISupportInitialize).BeginInit()
        innerSplit.Panel1.SuspendLayout()
        innerSplit.Panel2.SuspendLayout()
        innerSplit.SuspendLayout()
        CType(gridAntete, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridZi, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridOperatiuni, ComponentModel.ISupportInitialize).BeginInit()
        detailPane.SuspendLayout()
        detailTable.SuspendLayout()
        SuspendLayout()
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Margin = New Padding(4, 5, 4, 5)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(tree)
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(innerSplit)
        split.Size = New Size(986, 568)
        split.SplitterDistance = 280
        split.SplitterWidth = 9
        split.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.CollapseButtonTooltip = "Strânge arborele la o bandă îngustă." & vbLf & "Rândurile se citesc atunci prin eticheta care iese la survolare."
        tree.Dock = DockStyle.Fill
        tree.DynamicColumns = False
        tree.ExpandButtonTooltip = "Desfă arborele la loc, pe toată lățimea lui."
        tree.ExpanderSize = 10
        tree.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tree.FooterCaption = "Actualizează"
        tree.FooterCaptionFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.FooterCollapseButtonPosition = AdvancedTreeControl.En_FooterButtonPosition.Left
        tree.FooterCollapseCollapsedImage = My.Resources.Resources.expand_24
        tree.FooterCollapseExpandedImage = My.Resources.Resources.collapse_24
        tree.FooterHeight = 30
        tree.FooterIconSize = New Size(18, 18)
        tree.FooterRightIcon = My.Resources.Resources.Jonas_Rask_Danish_Royalty_Free_Refresh_32
        tree.FooterRightIconTooltip = "Descarcă extrasele de cont (SNM) din FOREXE." & vbLf & "Se conectează întâi, dacă nu există sesiune."
        tree.FooterSeparatorWidth = 2
        tree.FooterTextAlign = ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " EXTRASE DE CONT"
        tree.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.HeaderHeight = 30
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderSearchIconTooltip = "Caută în arbore." & vbLf & "ESC golește căutarea și închide banda."
        tree.HeaderSeparatorWidth = 2
        tree.HeaderVisible = True
        tree.Indent = 8
        tree.LeftIconSize = New Size(14, 14)
        tree.LeftTextWidth = 100
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(2, 4, 2, 4)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.PaddingExpanderGap = 10
        tree.PaddingIconGap = 10
        tree.PaddingTreeStart = 8
        tree.Size = New Size(280, 568)
        tree.TabIndex = 0
        ' 
        ' innerSplit
        ' 
        innerSplit.Dock = DockStyle.Fill
        innerSplit.Location = New Point(0, 0)
        innerSplit.Margin = New Padding(2)
        innerSplit.Name = "innerSplit"
        innerSplit.Orientation = Orientation.Horizontal
        ' 
        ' innerSplit.Panel1
        ' 
        innerSplit.Panel1.Controls.Add(gridZi)
        innerSplit.Panel1.Controls.Add(gridAntete)
        ' 
        ' innerSplit.Panel2
        ' 
        innerSplit.Panel2.Controls.Add(detailPane)
        innerSplit.Panel2.Controls.Add(gridOperatiuni)
        innerSplit.Size = New Size(697, 568)
        innerSplit.SplitterDistance = 300
        innerSplit.SplitterWidth = 9
        innerSplit.TabIndex = 0
        ' 
        ' gridAntete
        ' 
        gridAntete.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridAntete.AutoSizeHeaderHeight = False
        gridAntete.ColumnFillMode = KBotFillMode.LastColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn1.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn1.Format = KBotFormat.ShortDate
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Data"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "h_data"
        KBotDataColumn1.MinWidth = 40
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.ValueType = KBotValueType.DateTime
        KBotDataColumn1.Width = 100
        gridAntete.Columns.Add(KBotDataColumn1)
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn2.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Nr. extras"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "h_numar"
        KBotDataColumn2.MinWidth = 40
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn2.Width = 70
        gridAntete.Columns.Add(KBotDataColumn2)
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn3.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn3.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Clasificație"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "h_clsf"
        KBotDataColumn3.MinWidth = 40
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.ShowColumnFilter = True
        KBotDataColumn3.Width = 150
        gridAntete.Columns.Add(KBotDataColumn3)
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn4.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Denumire clasificație"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "h_denumire"
        KBotDataColumn4.MinWidth = 40
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn4.Width = 200
        gridAntete.Columns.Add(KBotDataColumn4)
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn5.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn5.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Cont"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "h_cont"
        KBotDataColumn5.MinWidth = 40
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn5.Width = 150
        gridAntete.Columns.Add(KBotDataColumn5)
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn6.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn6.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "IBAN"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "h_iban"
        KBotDataColumn6.MinWidth = 40
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn6.Width = 190
        gridAntete.Columns.Add(KBotDataColumn6)
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn7.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn7.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "SID"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "h_sid"
        KBotDataColumn7.MinWidth = 40
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn7.Width = 95
        gridAntete.Columns.Add(KBotDataColumn7)
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn8.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn8.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn8.DecimalPlaces = 2
        KBotDataColumn8.Format = KBotFormat.Standard
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "SIC"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "h_sic"
        KBotDataColumn8.MinWidth = 40
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn8.ValueType = KBotValueType.Number
        KBotDataColumn8.Width = 95
        gridAntete.Columns.Add(KBotDataColumn8)
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn9.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn9.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "RPD"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "h_rpd"
        KBotDataColumn9.MinWidth = 40
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBotValueType.Number
        KBotDataColumn9.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn9.Width = 95
        gridAntete.Columns.Add(KBotDataColumn9)
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn10.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn10.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn10.DecimalPlaces = 2
        KBotDataColumn10.Format = KBotFormat.Standard
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "RPC"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "h_rpc"
        KBotDataColumn10.MinWidth = 40
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn10.ValueType = KBotValueType.Number
        KBotDataColumn10.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn10.Width = 95
        gridAntete.Columns.Add(KBotDataColumn10)
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn11.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn11.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn11.DecimalPlaces = 2
        KBotDataColumn11.Format = KBotFormat.Standard
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "TSD"
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "h_tsd"
        KBotDataColumn11.MinWidth = 40
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.ReadOnly = True
        KBotDataColumn11.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn11.ValueType = KBotValueType.Number
        KBotDataColumn11.Width = 95
        gridAntete.Columns.Add(KBotDataColumn11)
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn12.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn12.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn12.DecimalPlaces = 2
        KBotDataColumn12.Format = KBotFormat.Standard
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "TSC"
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "h_tsc"
        KBotDataColumn12.MinWidth = 40
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.ReadOnly = True
        KBotDataColumn12.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn12.ValueType = KBotValueType.Number
        KBotDataColumn12.Width = 95
        gridAntete.Columns.Add(KBotDataColumn12)
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn13.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn13.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn13.DecimalPlaces = 2
        KBotDataColumn13.Format = KBotFormat.Standard
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = "SFD"
        KBotDataColumn13.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn13.Key = "h_sfd"
        KBotDataColumn13.MinWidth = 40
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.ReadOnly = True
        KBotDataColumn13.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn13.ValueType = KBotValueType.Number
        KBotDataColumn13.Width = 95
        gridAntete.Columns.Add(KBotDataColumn13)
        KBotDataColumn14.AggregateFormatString = Nothing
        KBotDataColumn14.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn14.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn14.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn14.DecimalPlaces = 2
        KBotDataColumn14.Format = KBotFormat.Standard
        KBotDataColumn14.FormatString = Nothing
        KBotDataColumn14.HeaderText = "SFC"
        KBotDataColumn14.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn14.Key = "h_sfc"
        KBotDataColumn14.MinWidth = 40
        KBotDataColumn14.OptionGroup = Nothing
        KBotDataColumn14.ReadOnly = True
        KBotDataColumn14.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn14.ValueType = KBotValueType.Number
        KBotDataColumn14.Width = 95
        gridAntete.Columns.Add(KBotDataColumn14)
        gridAntete.Dock = DockStyle.Fill
        gridAntete.EnableGrouping = True
        gridAntete.FilterIconSize = New Size(14, 14)
        gridAntete.FrozenColumnCount = 1
        gridAntete.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        gridAntete.Location = New Point(0, 0)
        gridAntete.Margin = New Padding(4, 5, 4, 5)
        gridAntete.Name = "gridAntete"
        gridAntete.ReadOnlyGrid = True
        gridAntete.RowHeight = 22
        gridAntete.ScrollByColumn = True
        gridAntete.ShrinkColumnsToFit = False
        gridAntete.Size = New Size(659, 300)
        gridAntete.TabIndex = 0
        tips.SetToolTipHeader(gridAntete, "Antetele extraselor")
        tips.SetToolTipText(gridAntete, "Un rând pentru fiecare cont din extras (FX_Extrase_H), cu soldurile lui." & vbLf & "Rândul selectat își arată operațiunile în grila de dedesubt.")
        ' 
        ' gridZi
        ' 
        gridZi.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridZi.AutoSizeHeaderHeight = False
        gridZi.ColumnFillMode = KBotFillMode.LastColumn
        KBotDataColumn15.AggregateFormatString = Nothing
        KBotDataColumn15.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn15.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn15.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn15.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn15.Format = KBotFormat.ShortDate
        KBotDataColumn15.FormatString = Nothing
        KBotDataColumn15.HeaderText = "Data bancă"
        KBotDataColumn15.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.Key = "o_data_banca"
        KBotDataColumn15.MinWidth = 40
        KBotDataColumn15.OptionGroup = Nothing
        KBotDataColumn15.ReadOnly = True
        KBotDataColumn15.ShowColumnFilter = True
        KBotDataColumn15.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.ValueType = KBotValueType.DateTime
        KBotDataColumn15.Width = 100
        gridZi.Columns.Add(KBotDataColumn15)
        KBotDataColumn16.AggregateFormatString = Nothing
        KBotDataColumn16.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn16.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn16.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn16.Format = KBotFormat.ShortDate
        KBotDataColumn16.FormatString = Nothing
        KBotDataColumn16.HeaderText = "Data document"
        KBotDataColumn16.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn16.Key = "o_data_doc"
        KBotDataColumn16.MinWidth = 40
        KBotDataColumn16.OptionGroup = Nothing
        KBotDataColumn16.ReadOnly = True
        KBotDataColumn16.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn16.ValueType = KBotValueType.DateTime
        KBotDataColumn16.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn16.Width = 100
        gridZi.Columns.Add(KBotDataColumn16)
        KBotDataColumn17.AggregateFormatString = Nothing
        KBotDataColumn17.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn17.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn17.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn17.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn17.FormatString = Nothing
        KBotDataColumn17.HeaderText = "Clasificație"
        KBotDataColumn17.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn17.Key = "o_clsf"
        KBotDataColumn17.MinWidth = 40
        KBotDataColumn17.OptionGroup = Nothing
        KBotDataColumn17.ReadOnly = True
        KBotDataColumn17.ShowColumnFilter = True
        KBotDataColumn17.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn17.Width = 150
        gridZi.Columns.Add(KBotDataColumn17)
        KBotDataColumn18.AggregateFormatString = Nothing
        KBotDataColumn18.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn18.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn18.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn18.FormatString = Nothing
        KBotDataColumn18.HeaderText = "Nr. document"
        KBotDataColumn18.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn18.Key = "o_nr_doc"
        KBotDataColumn18.MinWidth = 40
        KBotDataColumn18.OptionGroup = Nothing
        KBotDataColumn18.ReadOnly = True
        KBotDataColumn18.Width = 90
        gridZi.Columns.Add(KBotDataColumn18)
        KBotDataColumn19.AggregateFormatString = Nothing
        KBotDataColumn19.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn19.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn19.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn19.FormatString = Nothing
        KBotDataColumn19.HeaderText = "Referință"
        KBotDataColumn19.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn19.Key = "o_referinta"
        KBotDataColumn19.MinWidth = 40
        KBotDataColumn19.OptionGroup = Nothing
        KBotDataColumn19.ReadOnly = True
        KBotDataColumn19.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn19.Width = 120
        gridZi.Columns.Add(KBotDataColumn19)
        KBotDataColumn20.AggregateFormatString = Nothing
        KBotDataColumn20.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn20.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn20.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn20.FormatString = Nothing
        KBotDataColumn20.HeaderText = "Referință destinatar"
        KBotDataColumn20.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn20.Key = "o_referinta_dest"
        KBotDataColumn20.MinWidth = 40
        KBotDataColumn20.OptionGroup = Nothing
        KBotDataColumn20.ReadOnly = True
        KBotDataColumn20.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn20.Width = 120
        gridZi.Columns.Add(KBotDataColumn20)
        KBotDataColumn21.AggregateFormatString = Nothing
        KBotDataColumn21.AllowGrouping = False
        KBotDataColumn21.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn21.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn21.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn21.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn21.FormatString = Nothing
        KBotDataColumn21.HeaderText = "Plătitor"
        KBotDataColumn21.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn21.Key = "o_platitor"
        KBotDataColumn21.MinWidth = 40
        KBotDataColumn21.OptionGroup = Nothing
        KBotDataColumn21.ReadOnly = True
        KBotDataColumn21.ShowColumnFilter = True
        KBotDataColumn21.Width = 200
        gridZi.Columns.Add(KBotDataColumn21)
        KBotDataColumn22.AggregateFormatString = Nothing
        KBotDataColumn22.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn22.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn22.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn22.FormatString = Nothing
        KBotDataColumn22.HeaderText = "CUI"
        KBotDataColumn22.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn22.Key = "o_cui"
        KBotDataColumn22.MinWidth = 40
        KBotDataColumn22.OptionGroup = Nothing
        KBotDataColumn22.ReadOnly = True
        KBotDataColumn22.Width = 90
        gridZi.Columns.Add(KBotDataColumn22)
        KBotDataColumn23.AggregateFormatString = Nothing
        KBotDataColumn23.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn23.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn23.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn23.FormatString = Nothing
        KBotDataColumn23.HeaderText = "IBAN"
        KBotDataColumn23.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn23.Key = "o_iban"
        KBotDataColumn23.MinWidth = 40
        KBotDataColumn23.OptionGroup = Nothing
        KBotDataColumn23.ReadOnly = True
        KBotDataColumn23.Width = 190
        gridZi.Columns.Add(KBotDataColumn23)
        KBotDataColumn24.Aggregate = KBotAggregate.Sum
        KBotDataColumn24.AggregateFormatString = Nothing
        KBotDataColumn24.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn24.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn24.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn24.DecimalPlaces = 2
        KBotDataColumn24.Format = KBotFormat.Standard
        KBotDataColumn24.FormatString = Nothing
        KBotDataColumn24.HeaderText = "Debit"
        KBotDataColumn24.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn24.Key = "o_debit"
        KBotDataColumn24.MinWidth = 40
        KBotDataColumn24.OptionGroup = Nothing
        KBotDataColumn24.ReadOnly = True
        KBotDataColumn24.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn24.ValueType = KBotValueType.Number
        KBotDataColumn24.Width = 95
        gridZi.Columns.Add(KBotDataColumn24)
        KBotDataColumn25.Aggregate = KBotAggregate.Sum
        KBotDataColumn25.AggregateFormatString = Nothing
        KBotDataColumn25.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn25.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn25.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn25.DecimalPlaces = 2
        KBotDataColumn25.Format = KBotFormat.Standard
        KBotDataColumn25.FormatString = Nothing
        KBotDataColumn25.HeaderText = "Credit"
        KBotDataColumn25.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn25.Key = "o_credit"
        KBotDataColumn25.MinWidth = 40
        KBotDataColumn25.OptionGroup = Nothing
        KBotDataColumn25.ReadOnly = True
        KBotDataColumn25.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn25.ValueType = KBotValueType.Number
        KBotDataColumn25.Width = 95
        gridZi.Columns.Add(KBotDataColumn25)
        KBotDataColumn26.AggregateFormatString = Nothing
        KBotDataColumn26.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn26.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn26.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn26.FormatString = Nothing
        KBotDataColumn26.HeaderText = "Cod angajament"
        KBotDataColumn26.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn26.Key = "o_cod_angajament"
        KBotDataColumn26.MinWidth = 40
        KBotDataColumn26.OptionGroup = Nothing
        KBotDataColumn26.ReadOnly = True
        KBotDataColumn26.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn26.Width = 110
        gridZi.Columns.Add(KBotDataColumn26)
        KBotDataColumn27.AggregateFormatString = Nothing
        KBotDataColumn27.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn27.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn27.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn27.FormatString = Nothing
        KBotDataColumn27.HeaderText = "Indicator"
        KBotDataColumn27.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn27.Key = "o_indicator"
        KBotDataColumn27.MinWidth = 40
        KBotDataColumn27.OptionGroup = Nothing
        KBotDataColumn27.ReadOnly = True
        KBotDataColumn27.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn27.Width = 80
        gridZi.Columns.Add(KBotDataColumn27)
        KBotDataColumn28.AggregateFormatString = Nothing
        KBotDataColumn28.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn28.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn28.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn28.FormatString = Nothing
        KBotDataColumn28.HeaderText = "Cod program"
        KBotDataColumn28.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn28.Key = "o_cod_program"
        KBotDataColumn28.MinWidth = 40
        KBotDataColumn28.OptionGroup = Nothing
        KBotDataColumn28.ReadOnly = True
        KBotDataColumn28.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn28.Width = 90
        gridZi.Columns.Add(KBotDataColumn28)
        KBotDataColumn29.AggregateFormatString = Nothing
        KBotDataColumn29.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn29.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn29.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn29.FormatString = Nothing
        KBotDataColumn29.HeaderText = "CodAI"
        KBotDataColumn29.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn29.Key = "o_cod_ai"
        KBotDataColumn29.MinWidth = 40
        KBotDataColumn29.OptionGroup = Nothing
        KBotDataColumn29.ReadOnly = True
        KBotDataColumn29.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn29.Width = 120
        gridZi.Columns.Add(KBotDataColumn29)
        KBotDataColumn30.AggregateFormatString = Nothing
        KBotDataColumn30.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn30.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn30.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn30.FormatString = Nothing
        KBotDataColumn30.HeaderText = "Explicații"
        KBotDataColumn30.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn30.Key = "o_explicatii"
        KBotDataColumn30.MinWidth = 40
        KBotDataColumn30.OptionGroup = Nothing
        KBotDataColumn30.ReadOnly = True
        KBotDataColumn30.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn30.Width = 300
        gridZi.Columns.Add(KBotDataColumn30)
        gridZi.Dock = DockStyle.Fill
        gridZi.EnableGrouping = True
        gridZi.FilterIconSize = New Size(14, 14)
        gridZi.FooterCaption = "TOTALURI"
        gridZi.FooterFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        gridZi.FooterHeight = 30
        gridZi.FooterIconSize = New Size(14, 14)
        gridZi.FooterVisible = True
        gridZi.FrozenColumnCount = 1
        gridZi.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        gridZi.Location = New Point(0, 0)
        gridZi.Margin = New Padding(4, 5, 4, 5)
        gridZi.Name = "gridZi"
        gridZi.ReadOnlyGrid = True
        gridZi.RowHeight = 22
        gridZi.ScrollByColumn = True
        gridZi.ShrinkColumnsToFit = False
        gridZi.Size = New Size(659, 300)
        gridZi.TabIndex = 1
        tips.SetToolTipHeader(gridZi, "Operațiunile zilei")
        tips.SetToolTipText(gridZi, "Operațiunile din extras cu data băncii în ziua aleasă." & vbLf & "Rândul selectat se vede în întregime dedesubt.")
        gridZi.Visible = False
        ' 
        ' gridOperatiuni
        ' 
        gridOperatiuni.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridOperatiuni.AutoSizeHeaderHeight = False
        gridOperatiuni.ColumnFillMode = KBotFillMode.LastColumn
        KBotDataColumn31.AggregateFormatString = Nothing
        KBotDataColumn31.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn31.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn31.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn31.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn31.Format = KBotFormat.ShortDate
        KBotDataColumn31.FormatString = Nothing
        KBotDataColumn31.HeaderText = "Data bancă"
        KBotDataColumn31.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn31.Key = "o_data_banca"
        KBotDataColumn31.MinWidth = 40
        KBotDataColumn31.OptionGroup = Nothing
        KBotDataColumn31.ReadOnly = True
        KBotDataColumn31.ShowColumnFilter = True
        KBotDataColumn31.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn31.ValueType = KBotValueType.DateTime
        KBotDataColumn31.Width = 100
        gridOperatiuni.Columns.Add(KBotDataColumn31)
        KBotDataColumn32.AggregateFormatString = Nothing
        KBotDataColumn32.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn32.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn32.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn32.Format = KBotFormat.ShortDate
        KBotDataColumn32.FormatString = Nothing
        KBotDataColumn32.HeaderText = "Data document"
        KBotDataColumn32.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn32.Key = "o_data_doc"
        KBotDataColumn32.MinWidth = 40
        KBotDataColumn32.OptionGroup = Nothing
        KBotDataColumn32.ReadOnly = True
        KBotDataColumn32.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn32.ValueType = KBotValueType.DateTime
        KBotDataColumn32.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn32.Width = 100
        gridOperatiuni.Columns.Add(KBotDataColumn32)
        KBotDataColumn33.AggregateFormatString = Nothing
        KBotDataColumn33.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn33.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn33.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn33.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn33.FormatString = Nothing
        KBotDataColumn33.HeaderText = "Clasificație"
        KBotDataColumn33.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn33.Key = "o_clsf"
        KBotDataColumn33.MinWidth = 40
        KBotDataColumn33.OptionGroup = Nothing
        KBotDataColumn33.ReadOnly = True
        KBotDataColumn33.ShowColumnFilter = True
        KBotDataColumn33.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn33.Width = 150
        gridOperatiuni.Columns.Add(KBotDataColumn33)
        KBotDataColumn34.AggregateFormatString = Nothing
        KBotDataColumn34.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn34.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn34.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn34.FormatString = Nothing
        KBotDataColumn34.HeaderText = "Nr. document"
        KBotDataColumn34.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn34.Key = "o_nr_doc"
        KBotDataColumn34.MinWidth = 40
        KBotDataColumn34.OptionGroup = Nothing
        KBotDataColumn34.ReadOnly = True
        KBotDataColumn34.Width = 90
        gridOperatiuni.Columns.Add(KBotDataColumn34)
        KBotDataColumn35.AggregateFormatString = Nothing
        KBotDataColumn35.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn35.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn35.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn35.FormatString = Nothing
        KBotDataColumn35.HeaderText = "Referință"
        KBotDataColumn35.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn35.Key = "o_referinta"
        KBotDataColumn35.MinWidth = 40
        KBotDataColumn35.OptionGroup = Nothing
        KBotDataColumn35.ReadOnly = True
        KBotDataColumn35.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn35.Width = 120
        gridOperatiuni.Columns.Add(KBotDataColumn35)
        KBotDataColumn36.AggregateFormatString = Nothing
        KBotDataColumn36.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn36.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn36.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn36.FormatString = Nothing
        KBotDataColumn36.HeaderText = "Referință destinatar"
        KBotDataColumn36.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn36.Key = "o_referinta_dest"
        KBotDataColumn36.MinWidth = 40
        KBotDataColumn36.OptionGroup = Nothing
        KBotDataColumn36.ReadOnly = True
        KBotDataColumn36.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn36.Width = 120
        gridOperatiuni.Columns.Add(KBotDataColumn36)
        KBotDataColumn37.AggregateFormatString = Nothing
        KBotDataColumn37.AllowGrouping = False
        KBotDataColumn37.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn37.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn37.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn37.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn37.FormatString = Nothing
        KBotDataColumn37.HeaderText = "Plătitor"
        KBotDataColumn37.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn37.Key = "o_platitor"
        KBotDataColumn37.MinWidth = 40
        KBotDataColumn37.OptionGroup = Nothing
        KBotDataColumn37.ReadOnly = True
        KBotDataColumn37.ShowColumnFilter = True
        KBotDataColumn37.Width = 200
        gridOperatiuni.Columns.Add(KBotDataColumn37)
        KBotDataColumn38.AggregateFormatString = Nothing
        KBotDataColumn38.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn38.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn38.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn38.FormatString = Nothing
        KBotDataColumn38.HeaderText = "CUI"
        KBotDataColumn38.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn38.Key = "o_cui"
        KBotDataColumn38.MinWidth = 40
        KBotDataColumn38.OptionGroup = Nothing
        KBotDataColumn38.ReadOnly = True
        KBotDataColumn38.Width = 90
        gridOperatiuni.Columns.Add(KBotDataColumn38)
        KBotDataColumn39.AggregateFormatString = Nothing
        KBotDataColumn39.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn39.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn39.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn39.FormatString = Nothing
        KBotDataColumn39.HeaderText = "IBAN"
        KBotDataColumn39.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn39.Key = "o_iban"
        KBotDataColumn39.MinWidth = 40
        KBotDataColumn39.OptionGroup = Nothing
        KBotDataColumn39.ReadOnly = True
        KBotDataColumn39.Width = 190
        gridOperatiuni.Columns.Add(KBotDataColumn39)
        KBotDataColumn40.Aggregate = KBotAggregate.Sum
        KBotDataColumn40.AggregateFormatString = Nothing
        KBotDataColumn40.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn40.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn40.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn40.DecimalPlaces = 2
        KBotDataColumn40.Format = KBotFormat.Standard
        KBotDataColumn40.FormatString = Nothing
        KBotDataColumn40.HeaderText = "Debit"
        KBotDataColumn40.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn40.Key = "o_debit"
        KBotDataColumn40.MinWidth = 40
        KBotDataColumn40.OptionGroup = Nothing
        KBotDataColumn40.ReadOnly = True
        KBotDataColumn40.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn40.ValueType = KBotValueType.Number
        KBotDataColumn40.Width = 95
        gridOperatiuni.Columns.Add(KBotDataColumn40)
        KBotDataColumn41.Aggregate = KBotAggregate.Sum
        KBotDataColumn41.AggregateFormatString = Nothing
        KBotDataColumn41.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn41.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn41.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn41.DecimalPlaces = 2
        KBotDataColumn41.Format = KBotFormat.Standard
        KBotDataColumn41.FormatString = Nothing
        KBotDataColumn41.HeaderText = "Credit"
        KBotDataColumn41.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn41.Key = "o_credit"
        KBotDataColumn41.MinWidth = 40
        KBotDataColumn41.OptionGroup = Nothing
        KBotDataColumn41.ReadOnly = True
        KBotDataColumn41.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn41.ValueType = KBotValueType.Number
        KBotDataColumn41.Width = 95
        gridOperatiuni.Columns.Add(KBotDataColumn41)
        KBotDataColumn42.AggregateFormatString = Nothing
        KBotDataColumn42.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn42.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn42.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn42.FormatString = Nothing
        KBotDataColumn42.HeaderText = "Cod angajament"
        KBotDataColumn42.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn42.Key = "o_cod_angajament"
        KBotDataColumn42.MinWidth = 40
        KBotDataColumn42.OptionGroup = Nothing
        KBotDataColumn42.ReadOnly = True
        KBotDataColumn42.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn42.Width = 110
        gridOperatiuni.Columns.Add(KBotDataColumn42)
        KBotDataColumn43.AggregateFormatString = Nothing
        KBotDataColumn43.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn43.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn43.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn43.FormatString = Nothing
        KBotDataColumn43.HeaderText = "Indicator"
        KBotDataColumn43.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn43.Key = "o_indicator"
        KBotDataColumn43.MinWidth = 40
        KBotDataColumn43.OptionGroup = Nothing
        KBotDataColumn43.ReadOnly = True
        KBotDataColumn43.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn43.Width = 80
        gridOperatiuni.Columns.Add(KBotDataColumn43)
        KBotDataColumn44.AggregateFormatString = Nothing
        KBotDataColumn44.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn44.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn44.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn44.FormatString = Nothing
        KBotDataColumn44.HeaderText = "Cod program"
        KBotDataColumn44.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn44.Key = "o_cod_program"
        KBotDataColumn44.MinWidth = 40
        KBotDataColumn44.OptionGroup = Nothing
        KBotDataColumn44.ReadOnly = True
        KBotDataColumn44.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn44.Width = 90
        gridOperatiuni.Columns.Add(KBotDataColumn44)
        KBotDataColumn45.AggregateFormatString = Nothing
        KBotDataColumn45.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn45.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn45.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn45.FormatString = Nothing
        KBotDataColumn45.HeaderText = "CodAI"
        KBotDataColumn45.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn45.Key = "o_cod_ai"
        KBotDataColumn45.MinWidth = 40
        KBotDataColumn45.OptionGroup = Nothing
        KBotDataColumn45.ReadOnly = True
        KBotDataColumn45.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn45.Width = 120
        gridOperatiuni.Columns.Add(KBotDataColumn45)
        KBotDataColumn46.AggregateFormatString = Nothing
        KBotDataColumn46.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn46.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn46.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn46.FormatString = Nothing
        KBotDataColumn46.HeaderText = "Explicații"
        KBotDataColumn46.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn46.Key = "o_explicatii"
        KBotDataColumn46.MinWidth = 40
        KBotDataColumn46.OptionGroup = Nothing
        KBotDataColumn46.ReadOnly = True
        KBotDataColumn46.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn46.Width = 300
        gridOperatiuni.Columns.Add(KBotDataColumn46)
        gridOperatiuni.Dock = DockStyle.Fill
        gridOperatiuni.EnableGrouping = True
        gridOperatiuni.FilterIconSize = New Size(14, 14)
        gridOperatiuni.FooterCaption = "TOTALURI"
        gridOperatiuni.FooterFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        gridOperatiuni.FooterHeight = 30
        gridOperatiuni.FooterIconSize = New Size(14, 14)
        gridOperatiuni.FooterVisible = True
        gridOperatiuni.FrozenColumnCount = 1
        gridOperatiuni.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        gridOperatiuni.Location = New Point(0, 0)
        gridOperatiuni.Margin = New Padding(4, 5, 4, 5)
        gridOperatiuni.Name = "gridOperatiuni"
        gridOperatiuni.ReadOnlyGrid = True
        gridOperatiuni.RowHeight = 22
        gridOperatiuni.ScrollByColumn = True
        gridOperatiuni.ShrinkColumnsToFit = False
        gridOperatiuni.Size = New Size(659, 300)
        gridOperatiuni.TabIndex = 0
        tips.SetToolTipHeader(gridOperatiuni, "Operațiunile antetului")
        tips.SetToolTipText(gridOperatiuni, "Operațiunile din extras ale rândului selectat sus.")
        ' 
        ' detailPane
        ' 
        detailPane.Controls.Add(detailTable)
        detailPane.Controls.Add(lblDetailMessage)
        detailPane.Dock = DockStyle.Fill
        detailPane.Location = New Point(0, 0)
        detailPane.Margin = New Padding(0)
        detailPane.Name = "detailPane"
        detailPane.Size = New Size(697, 259)
        detailPane.TabIndex = 1
        detailPane.Visible = False
        ' 
        ' detailTable
        ' 
        detailTable.AutoScroll = True
        detailTable.ColumnCount = 2
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 236F))
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        detailTable.Controls.Add(capNrDoc, 0, 0)
        detailTable.Controls.Add(valNrDoc, 1, 0)
        detailTable.Controls.Add(capDataBanca, 0, 1)
        detailTable.Controls.Add(valDataBanca, 1, 1)
        detailTable.Controls.Add(capDataDoc, 0, 2)
        detailTable.Controls.Add(valDataDoc, 1, 2)
        detailTable.Controls.Add(capReferinta, 0, 3)
        detailTable.Controls.Add(valReferinta, 1, 3)
        detailTable.Controls.Add(capPlatitor, 0, 4)
        detailTable.Controls.Add(valPlatitor, 1, 4)
        detailTable.Controls.Add(capCui, 0, 5)
        detailTable.Controls.Add(valCui, 1, 5)
        detailTable.Controls.Add(capIban, 0, 6)
        detailTable.Controls.Add(valIban, 1, 6)
        detailTable.Controls.Add(capDebit, 0, 7)
        detailTable.Controls.Add(valDebit, 1, 7)
        detailTable.Controls.Add(capCredit, 0, 8)
        detailTable.Controls.Add(valCredit, 1, 8)
        detailTable.Controls.Add(capCodAngajament, 0, 9)
        detailTable.Controls.Add(valCodAngajament, 1, 9)
        detailTable.Controls.Add(capIndicator, 0, 10)
        detailTable.Controls.Add(valIndicator, 1, 10)
        detailTable.Controls.Add(capReferintaDest, 0, 11)
        detailTable.Controls.Add(valReferintaDest, 1, 11)
        detailTable.Controls.Add(capCodProgram, 0, 12)
        detailTable.Controls.Add(valCodProgram, 1, 12)
        detailTable.Controls.Add(capExplicatii, 0, 13)
        detailTable.Controls.Add(valExplicatii, 1, 13)
        detailTable.Dock = DockStyle.Fill
        detailTable.Location = New Point(0, 0)
        detailTable.Margin = New Padding(0)
        detailTable.Name = "detailTable"
        detailTable.RowCount = 14
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        detailTable.Size = New Size(697, 259)
        detailTable.TabIndex = 0
        detailTable.Visible = False
        ' 
        ' capNrDoc
        ' 
        capNrDoc.AutoSize = True
        capNrDoc.Margin = New Padding(3, 4, 8, 4)
        capNrDoc.Name = "capNrDoc"
        capNrDoc.TabIndex = 0
        capNrDoc.Text = "Nr. document"
        ' 
        ' valNrDoc
        ' 
        valNrDoc.AutoSize = True
        valNrDoc.Margin = New Padding(3, 4, 3, 4)
        valNrDoc.Name = "valNrDoc"
        valNrDoc.TabIndex = 1
        valNrDoc.Text = ""
        ' 
        ' capDataBanca
        ' 
        capDataBanca.AutoSize = True
        capDataBanca.Margin = New Padding(3, 4, 8, 4)
        capDataBanca.Name = "capDataBanca"
        capDataBanca.TabIndex = 2
        capDataBanca.Text = "Data bancă"
        ' 
        ' valDataBanca
        ' 
        valDataBanca.AutoSize = True
        valDataBanca.Margin = New Padding(3, 4, 3, 4)
        valDataBanca.Name = "valDataBanca"
        valDataBanca.TabIndex = 3
        valDataBanca.Text = ""
        ' 
        ' capDataDoc
        ' 
        capDataDoc.AutoSize = True
        capDataDoc.Margin = New Padding(3, 4, 8, 4)
        capDataDoc.Name = "capDataDoc"
        capDataDoc.TabIndex = 4
        capDataDoc.Text = "Data document"
        ' 
        ' valDataDoc
        ' 
        valDataDoc.AutoSize = True
        valDataDoc.Margin = New Padding(3, 4, 3, 4)
        valDataDoc.Name = "valDataDoc"
        valDataDoc.TabIndex = 5
        valDataDoc.Text = ""
        ' 
        ' capReferinta
        ' 
        capReferinta.AutoSize = True
        capReferinta.Margin = New Padding(3, 4, 8, 4)
        capReferinta.Name = "capReferinta"
        capReferinta.TabIndex = 6
        capReferinta.Text = "Referință"
        ' 
        ' valReferinta
        ' 
        valReferinta.AutoSize = True
        valReferinta.Margin = New Padding(3, 4, 3, 4)
        valReferinta.Name = "valReferinta"
        valReferinta.TabIndex = 7
        valReferinta.Text = ""
        ' 
        ' capPlatitor
        ' 
        capPlatitor.AutoSize = True
        capPlatitor.Margin = New Padding(3, 4, 8, 4)
        capPlatitor.Name = "capPlatitor"
        capPlatitor.TabIndex = 8
        capPlatitor.Text = "Plătitor"
        ' 
        ' valPlatitor
        ' 
        valPlatitor.AutoSize = True
        valPlatitor.Margin = New Padding(3, 4, 3, 4)
        valPlatitor.Name = "valPlatitor"
        valPlatitor.TabIndex = 9
        valPlatitor.Text = ""
        ' 
        ' capCui
        ' 
        capCui.AutoSize = True
        capCui.Margin = New Padding(3, 4, 8, 4)
        capCui.Name = "capCui"
        capCui.TabIndex = 10
        capCui.Text = "CUI"
        ' 
        ' valCui
        ' 
        valCui.AutoSize = True
        valCui.Margin = New Padding(3, 4, 3, 4)
        valCui.Name = "valCui"
        valCui.TabIndex = 11
        valCui.Text = ""
        ' 
        ' capIban
        ' 
        capIban.AutoSize = True
        capIban.Margin = New Padding(3, 4, 8, 4)
        capIban.Name = "capIban"
        capIban.TabIndex = 12
        capIban.Text = "IBAN"
        ' 
        ' valIban
        ' 
        valIban.AutoSize = True
        valIban.Margin = New Padding(3, 4, 3, 4)
        valIban.Name = "valIban"
        valIban.TabIndex = 13
        valIban.Text = ""
        ' 
        ' capDebit
        ' 
        capDebit.AutoSize = True
        capDebit.Margin = New Padding(3, 4, 8, 4)
        capDebit.Name = "capDebit"
        capDebit.TabIndex = 14
        capDebit.Text = "Sumă debit"
        ' 
        ' valDebit
        ' 
        valDebit.AutoSize = True
        valDebit.Margin = New Padding(3, 4, 3, 4)
        valDebit.Name = "valDebit"
        valDebit.TabIndex = 15
        valDebit.Text = ""
        ' 
        ' capCredit
        ' 
        capCredit.AutoSize = True
        capCredit.Margin = New Padding(3, 4, 8, 4)
        capCredit.Name = "capCredit"
        capCredit.TabIndex = 16
        capCredit.Text = "Sumă credit"
        ' 
        ' valCredit
        ' 
        valCredit.AutoSize = True
        valCredit.Margin = New Padding(3, 4, 3, 4)
        valCredit.Name = "valCredit"
        valCredit.TabIndex = 17
        valCredit.Text = ""
        ' 
        ' capCodAngajament
        ' 
        capCodAngajament.AutoSize = True
        capCodAngajament.Margin = New Padding(3, 4, 8, 4)
        capCodAngajament.Name = "capCodAngajament"
        capCodAngajament.TabIndex = 18
        capCodAngajament.Text = "Cod angajament"
        ' 
        ' valCodAngajament
        ' 
        valCodAngajament.AutoSize = True
        valCodAngajament.Margin = New Padding(3, 4, 3, 4)
        valCodAngajament.Name = "valCodAngajament"
        valCodAngajament.TabIndex = 19
        valCodAngajament.Text = ""
        ' 
        ' capIndicator
        ' 
        capIndicator.AutoSize = True
        capIndicator.Margin = New Padding(3, 4, 8, 4)
        capIndicator.Name = "capIndicator"
        capIndicator.TabIndex = 20
        capIndicator.Text = "Indicator"
        ' 
        ' valIndicator
        ' 
        valIndicator.AutoSize = True
        valIndicator.Margin = New Padding(3, 4, 3, 4)
        valIndicator.Name = "valIndicator"
        valIndicator.TabIndex = 21
        valIndicator.Text = ""
        ' 
        ' capReferintaDest
        ' 
        capReferintaDest.AutoSize = True
        capReferintaDest.Margin = New Padding(3, 4, 8, 4)
        capReferintaDest.Name = "capReferintaDest"
        capReferintaDest.TabIndex = 22
        capReferintaDest.Text = "Referință destinatar"
        ' 
        ' valReferintaDest
        ' 
        valReferintaDest.AutoSize = True
        valReferintaDest.Margin = New Padding(3, 4, 3, 4)
        valReferintaDest.Name = "valReferintaDest"
        valReferintaDest.TabIndex = 23
        valReferintaDest.Text = ""
        ' 
        ' capCodProgram
        ' 
        capCodProgram.AutoSize = True
        capCodProgram.Margin = New Padding(3, 4, 8, 4)
        capCodProgram.Name = "capCodProgram"
        capCodProgram.TabIndex = 24
        capCodProgram.Text = "Cod program"
        ' 
        ' valCodProgram
        ' 
        valCodProgram.AutoSize = True
        valCodProgram.Margin = New Padding(3, 4, 3, 4)
        valCodProgram.Name = "valCodProgram"
        valCodProgram.TabIndex = 25
        valCodProgram.Text = ""
        ' 
        ' capExplicatii
        ' 
        capExplicatii.AutoSize = True
        capExplicatii.Margin = New Padding(3, 4, 8, 4)
        capExplicatii.Name = "capExplicatii"
        capExplicatii.TabIndex = 26
        capExplicatii.Text = "Explicații"
        ' 
        ' valExplicatii
        ' 
        valExplicatii.AutoSize = True
        valExplicatii.Dock = DockStyle.Fill
        valExplicatii.Margin = New Padding(3, 4, 3, 4)
        valExplicatii.Name = "valExplicatii"
        valExplicatii.TabIndex = 27
        valExplicatii.Text = ""
        ' 
        ' lblDetailMessage
        ' 
        lblDetailMessage.Dock = DockStyle.Fill
        lblDetailMessage.Font = New Font("Segoe UI", 10F)
        lblDetailMessage.Location = New Point(0, 0)
        lblDetailMessage.Margin = New Padding(2, 0, 2, 0)
        lblDetailMessage.Name = "lblDetailMessage"
        lblDetailMessage.Size = New Size(697, 259)
        lblDetailMessage.TabIndex = 1
        lblDetailMessage.Text = "Selectați o operațiune."
        lblDetailMessage.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ExtrasePanel
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(split)
        Margin = New Padding(4, 5, 4, 5)
        Name = "ExtrasePanel"
        Size = New Size(986, 568)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        innerSplit.Panel1.ResumeLayout(False)
        innerSplit.Panel2.ResumeLayout(False)
        CType(innerSplit, ComponentModel.ISupportInitialize).EndInit()
        innerSplit.ResumeLayout(False)
        CType(gridAntete, ComponentModel.ISupportInitialize).EndInit()
        CType(gridZi, ComponentModel.ISupportInitialize).EndInit()
        CType(gridOperatiuni, ComponentModel.ISupportInitialize).EndInit()
        detailTable.ResumeLayout(False)
        detailTable.PerformLayout()
        detailPane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents split As SplitContainer
    Friend WithEvents tree As Global.KBot.Controls.AdvancedTreeControl
    Friend WithEvents innerSplit As SplitContainer
    Friend WithEvents gridAntete As Global.KBot.Controls.KBotDataView
    Friend WithEvents gridZi As Global.KBot.Controls.KBotDataView
    Friend WithEvents gridOperatiuni As Global.KBot.Controls.KBotDataView
    Friend WithEvents detailPane As Panel
    Friend WithEvents detailTable As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblDetailMessage As Label
    Friend WithEvents capNrDoc As Label
    Friend WithEvents valNrDoc As Label
    Friend WithEvents capDataBanca As Label
    Friend WithEvents valDataBanca As Label
    Friend WithEvents capDataDoc As Label
    Friend WithEvents valDataDoc As Label
    Friend WithEvents capReferinta As Label
    Friend WithEvents valReferinta As Label
    Friend WithEvents capPlatitor As Label
    Friend WithEvents valPlatitor As Label
    Friend WithEvents capCui As Label
    Friend WithEvents valCui As Label
    Friend WithEvents capIban As Label
    Friend WithEvents valIban As Label
    Friend WithEvents capDebit As Label
    Friend WithEvents valDebit As Label
    Friend WithEvents capCredit As Label
    Friend WithEvents valCredit As Label
    Friend WithEvents capCodAngajament As Label
    Friend WithEvents valCodAngajament As Label
    Friend WithEvents capIndicator As Label
    Friend WithEvents valIndicator As Label
    Friend WithEvents capReferintaDest As Label
    Friend WithEvents valReferintaDest As Label
    Friend WithEvents capCodProgram As Label
    Friend WithEvents valCodProgram As Label
    Friend WithEvents capExplicatii As Label
    Friend WithEvents valExplicatii As Label
End Class
