Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ClasificatiiForm
    Inherits Global.KBot.Theming.KBotShellForm

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
        tips =New KBotToolTip(components)
        gridBuget = New KBotDataView()
        gridRectificari = New KBotDataView()
        gridTotal = New KBotDataView()
        btnSalveaza = New Button()
        btnVerifica = New Button()
        btnTrimiteAccess = New Button()
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        pnlCard = New Panel()
        tlyBody = New KBotTableLayoutPanel()
        tree = New AdvancedTreeControl()
        tlyBife = New KBotTableLayoutPanel()
        chkToate = New CheckBox()
        chkForexe = New CheckBox()
        tlyRight = New KBotTableLayoutPanel()
        lblBuget = New Label()
        lblRectificari = New Label()
        tlySubsol = New KBotTableLayoutPanel()
        lblStare = New Label()
        btnInchide = New Button()
        CType(gridBuget, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridRectificari, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridTotal, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        tlyBife.SuspendLayout()
        tlyRight.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        ' 
        ' gridBuget
        ' 
        gridBuget.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridBuget.BackColor = SystemColors.Window
        gridBuget.ColumnFillMode = KBotFillMode.SpecificColumn
        gridBuget.FillColumnKey = "clsf"
        KBotDataColumn17.AggregateFormatString = Nothing
        KBotDataColumn17.FormatString = Nothing
        KBotDataColumn17.HeaderText = "Clsf"
        KBotDataColumn17.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn17.Key = "clsf"
        KBotDataColumn17.MinWidth = 150
        KBotDataColumn17.OptionGroup = Nothing
        KBotDataColumn17.ReadOnly = True
        KBotDataColumn17.Width = 150
        KBotDataColumn15.AggregateFormatString = Nothing
        KBotDataColumn15.Format = KBotFormat.ShortDate
        KBotDataColumn15.FormatString = Nothing
        KBotDataColumn15.HeaderText = "Început"
        KBotDataColumn15.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.Key = "inceput"
        KBotDataColumn15.OptionGroup = Nothing
        KBotDataColumn15.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.ValueType = KBotValueType.DateTime
        KBotDataColumn15.Width = 110
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.DecimalPlaces = 2
        KBotDataColumn1.Format = KBotFormat.Standard
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Trim. 1"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "trim1"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn1.ValueType = KBotValueType.Number
        KBotDataColumn1.Width = 110
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.DecimalPlaces = 2
        KBotDataColumn2.Format = KBotFormat.Standard
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Trim. 2"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "trim2"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn2.ValueType = KBotValueType.Number
        KBotDataColumn2.Width = 110
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Trim. 3"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "trim3"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBotValueType.Number
        KBotDataColumn3.Width = 110
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Trim. 4"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "trim4"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 110
        KBotDataColumn19.AggregateFormatString = Nothing
        KBotDataColumn19.DecimalPlaces = 2
        KBotDataColumn19.Format = KBotFormat.Standard
        KBotDataColumn19.FormatString = Nothing
        KBotDataColumn19.HeaderText = "Total"
        KBotDataColumn19.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn19.Key = "total"
        KBotDataColumn19.OptionGroup = Nothing
        KBotDataColumn19.ReadOnly = True
        KBotDataColumn19.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn19.ValueType = KBotValueType.Number
        KBotDataColumn19.Width = 110
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBotColumnType.Button
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = ""
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn5.Key = "sterge"
        KBotDataColumn5.MinWidth = 34
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Resizable = False
        KBotDataColumn5.Width = 34
        KBotDataColumn16.AggregateFormatString = Nothing
        KBotDataColumn16.FormatString = Nothing
        KBotDataColumn16.HeaderText = "Id"
        KBotDataColumn16.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn16.Key = "id"
        KBotDataColumn16.OptionGroup = Nothing
        KBotDataColumn16.ReadOnly = True
        KBotDataColumn16.Visible = KBotColumnVisibility.Hidden
        gridBuget.Columns.Add(KBotDataColumn17)
        gridBuget.Columns.Add(KBotDataColumn15)
        gridBuget.Columns.Add(KBotDataColumn1)
        gridBuget.Columns.Add(KBotDataColumn2)
        gridBuget.Columns.Add(KBotDataColumn3)
        gridBuget.Columns.Add(KBotDataColumn4)
        gridBuget.Columns.Add(KBotDataColumn19)
        gridBuget.Columns.Add(KBotDataColumn5)
        gridBuget.Columns.Add(KBotDataColumn16)
        gridBuget.Dock = DockStyle.Fill
        gridBuget.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridBuget.FooterBackColor = SystemColors.Control
        gridBuget.FooterCaption = "Versiuni de buget"
        gridBuget.FooterRightIcon = My.Resources.Resources.plus_green
        gridBuget.FooterRightIconTooltip = "Adaugă o versiune de buget" & vbLf & "Rândul nou se completează direct în tabel."
        gridBuget.FooterSeparatorColor = SystemColors.ActiveBorder
        gridBuget.FooterVisible = True
        gridBuget.HeaderBackColor = SystemColors.Control
        gridBuget.HeaderHeight = 24
        gridBuget.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridBuget.Location = New Point(0, 45)
        gridBuget.Margin = New Padding(0)
        gridBuget.Name = "gridBuget"
        gridBuget.RowHeight = 24
        gridBuget.Size = New Size(955, 200)
        gridBuget.TabIndex = 1
        tips.SetToolTipHeader(gridBuget, "Bugetul clasificației, pe versiuni")
        tips.SetToolTipText(gridBuget, "Fiecare rând este bugetul de la data din «Început» încolo (trimestrele 1 - 4); documentul de fundamentare citește bugetul de la data revizuirii." & vbLf & "«+» din subsol adaugă o versiune; «✕» o șterge. Totul se scrie la «Salvează».")
        ' 
        ' gridRectificari
        ' 
        gridRectificari.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridRectificari.BackColor = SystemColors.Window
        gridRectificari.ColumnFillMode = KBotFillMode.SpecificColumn
        gridRectificari.FillColumnKey = "document"
        KBotDataColumn18.AggregateFormatString = Nothing
        KBotDataColumn18.FormatString = Nothing
        KBotDataColumn18.HeaderText = "Clsf"
        KBotDataColumn18.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn18.Key = "clsf"
        KBotDataColumn18.MinWidth = 150
        KBotDataColumn18.OptionGroup = Nothing
        KBotDataColumn18.ReadOnly = True
        KBotDataColumn18.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn18.Width = 150
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Nr. doc."
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "document"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.MinWidth = 150
        KBotDataColumn6.Width = 150
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.Format = KBotFormat.ShortDate
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Data"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "data"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.ValueType = KBotValueType.DateTime
        KBotDataColumn7.Width = 110
        KBotDataColumn8.Aggregate = KBotAggregate.Sum
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.DecimalPlaces = 2
        KBotDataColumn8.Format = KBotFormat.Standard
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Trim. 1"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "trim1"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn8.ValueType = KBotValueType.Number
        KBotDataColumn8.Width = 110
        KBotDataColumn9.Aggregate = KBotAggregate.Sum
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Trim. 2"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "trim2"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBotValueType.Number
        KBotDataColumn9.Width = 110
        KBotDataColumn10.Aggregate = KBotAggregate.Sum
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.DecimalPlaces = 2
        KBotDataColumn10.Format = KBotFormat.Standard
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Trim. 3"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "trim3"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn10.ValueType = KBotValueType.Number
        KBotDataColumn10.Width = 110
        KBotDataColumn11.Aggregate = KBotAggregate.Sum
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.DecimalPlaces = 2
        KBotDataColumn11.Format = KBotFormat.Standard
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "Trim. 4"
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "trim4"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn11.ValueType = KBotValueType.Number
        KBotDataColumn11.Width = 110
        KBotDataColumn12.Aggregate = KBotAggregate.Sum
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.DecimalPlaces = 2
        KBotDataColumn12.Format = KBotFormat.Standard
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "Total"
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "total"
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.ReadOnly = True
        KBotDataColumn12.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn12.ValueType = KBotValueType.Number
        KBotDataColumn12.Width = 110
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.ColumnType = KBotColumnType.Button
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = ""
        KBotDataColumn13.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn13.Key = "sterge"
        KBotDataColumn13.MinWidth = 34
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.Resizable = False
        KBotDataColumn13.Width = 34
        KBotDataColumn14.AggregateFormatString = Nothing
        KBotDataColumn14.FormatString = Nothing
        KBotDataColumn14.HeaderText = "Id"
        KBotDataColumn14.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn14.Key = "id"
        KBotDataColumn14.OptionGroup = Nothing
        KBotDataColumn14.ReadOnly = True
        KBotDataColumn14.Visible = KBotColumnVisibility.Hidden
        gridRectificari.Columns.Add(KBotDataColumn18)
        gridRectificari.Columns.Add(KBotDataColumn6)
        gridRectificari.Columns.Add(KBotDataColumn7)
        gridRectificari.Columns.Add(KBotDataColumn8)
        gridRectificari.Columns.Add(KBotDataColumn9)
        gridRectificari.Columns.Add(KBotDataColumn10)
        gridRectificari.Columns.Add(KBotDataColumn11)
        gridRectificari.Columns.Add(KBotDataColumn12)
        gridRectificari.Columns.Add(KBotDataColumn13)
        gridRectificari.Columns.Add(KBotDataColumn14)
        gridRectificari.Dock = DockStyle.Fill
        gridRectificari.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridRectificari.FooterBackColor = SystemColors.Control
        gridRectificari.FooterCaption = "Total"
        gridRectificari.FooterRightIcon = My.Resources.Resources.plus_green
        gridRectificari.FooterRightIconTooltip = "Adaugă o rectificare" & vbLf & "Rândul nou se completează direct în tabel."
        gridRectificari.FooterSeparatorColor = SystemColors.ActiveBorder
        gridRectificari.FooterVisible = True
        gridRectificari.HeaderHeight = 24
        gridRectificari.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridRectificari.Location = New Point(0, 192)
        gridRectificari.Margin = New Padding(0)
        gridRectificari.Name = "gridRectificari"
        gridRectificari.Size = New Size(955, 422)
        gridRectificari.TabIndex = 3
        tips.SetToolTipHeader(gridRectificari, "Rectificările bugetare ale anului")
        tips.SetToolTipText(gridRectificari, "Nr. doc. și data sunt obligatorii; data trebuie să fie în anul de lucru." & vbLf & "«+» din subsol adaugă un rând; «✕» îl șterge. Totul se scrie la «Salvează».")
        '
        ' gridTotal
        '
        gridTotal.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridTotal.BackColor = SystemColors.Window
        gridTotal.ColumnFillMode = KBotFillMode.SpecificColumn
        gridTotal.FillColumnKey = "clsf"
        KBotDataColumn20.AggregateFormatString = Nothing
        KBotDataColumn20.FormatString = Nothing
        KBotDataColumn20.HeaderText = "Clsf"
        KBotDataColumn20.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn20.Key = "clsf"
        KBotDataColumn20.MinWidth = 150
        KBotDataColumn20.OptionGroup = Nothing
        KBotDataColumn20.ReadOnly = True
        KBotDataColumn20.Width = 150
        KBotDataColumn21.AggregateFormatString = Nothing
        KBotDataColumn21.Format = KBotFormat.ShortDate
        KBotDataColumn21.FormatString = Nothing
        KBotDataColumn21.HeaderText = "Început"
        KBotDataColumn21.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn21.Key = "inceput"
        KBotDataColumn21.OptionGroup = Nothing
        KBotDataColumn21.ReadOnly = True
        KBotDataColumn21.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn21.ValueType = KBotValueType.DateTime
        KBotDataColumn21.Width = 110
        KBotDataColumn22.AggregateFormatString = Nothing
        KBotDataColumn22.DecimalPlaces = 2
        KBotDataColumn22.Format = KBotFormat.Standard
        KBotDataColumn22.FormatString = Nothing
        KBotDataColumn22.HeaderText = "Trim. 1"
        KBotDataColumn22.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn22.Key = "trim1"
        KBotDataColumn22.OptionGroup = Nothing
        KBotDataColumn22.ReadOnly = True
        KBotDataColumn22.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn22.ValueType = KBotValueType.Number
        KBotDataColumn22.Width = 110
        KBotDataColumn23.AggregateFormatString = Nothing
        KBotDataColumn23.DecimalPlaces = 2
        KBotDataColumn23.Format = KBotFormat.Standard
        KBotDataColumn23.FormatString = Nothing
        KBotDataColumn23.HeaderText = "Trim. 2"
        KBotDataColumn23.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn23.Key = "trim2"
        KBotDataColumn23.OptionGroup = Nothing
        KBotDataColumn23.ReadOnly = True
        KBotDataColumn23.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn23.ValueType = KBotValueType.Number
        KBotDataColumn23.Width = 110
        KBotDataColumn24.AggregateFormatString = Nothing
        KBotDataColumn24.DecimalPlaces = 2
        KBotDataColumn24.Format = KBotFormat.Standard
        KBotDataColumn24.FormatString = Nothing
        KBotDataColumn24.HeaderText = "Trim. 3"
        KBotDataColumn24.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn24.Key = "trim3"
        KBotDataColumn24.OptionGroup = Nothing
        KBotDataColumn24.ReadOnly = True
        KBotDataColumn24.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn24.ValueType = KBotValueType.Number
        KBotDataColumn24.Width = 110
        KBotDataColumn25.AggregateFormatString = Nothing
        KBotDataColumn25.DecimalPlaces = 2
        KBotDataColumn25.Format = KBotFormat.Standard
        KBotDataColumn25.FormatString = Nothing
        KBotDataColumn25.HeaderText = "Trim. 4"
        KBotDataColumn25.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn25.Key = "trim4"
        KBotDataColumn25.OptionGroup = Nothing
        KBotDataColumn25.ReadOnly = True
        KBotDataColumn25.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn25.ValueType = KBotValueType.Number
        KBotDataColumn25.Width = 110
        KBotDataColumn26.AggregateFormatString = Nothing
        KBotDataColumn26.DecimalPlaces = 2
        KBotDataColumn26.Format = KBotFormat.Standard
        KBotDataColumn26.FormatString = Nothing
        KBotDataColumn26.HeaderText = "Total"
        KBotDataColumn26.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn26.Key = "total"
        KBotDataColumn26.OptionGroup = Nothing
        KBotDataColumn26.ReadOnly = True
        KBotDataColumn26.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn26.ValueType = KBotValueType.Number
        KBotDataColumn26.Width = 110
        KBotDataColumn27.AggregateFormatString = Nothing
        KBotDataColumn27.FormatString = Nothing
        KBotDataColumn27.HeaderText = ""
        KBotDataColumn27.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn27.Key = "sterge"
        KBotDataColumn27.MinWidth = 34
        KBotDataColumn27.OptionGroup = Nothing
        KBotDataColumn27.ReadOnly = True
        KBotDataColumn27.Resizable = False
        KBotDataColumn27.Width = 34
        gridTotal.Columns.Add(KBotDataColumn20)
        gridTotal.Columns.Add(KBotDataColumn21)
        gridTotal.Columns.Add(KBotDataColumn22)
        gridTotal.Columns.Add(KBotDataColumn23)
        gridTotal.Columns.Add(KBotDataColumn24)
        gridTotal.Columns.Add(KBotDataColumn25)
        gridTotal.Columns.Add(KBotDataColumn26)
        gridTotal.Columns.Add(KBotDataColumn27)
        gridTotal.Dock = DockStyle.Fill
        gridTotal.HeaderHeight = 24
        gridTotal.Location = New Point(0, 604)
        gridTotal.Margin = New Padding(0, 12, 0, 0)
        gridTotal.Name = "gridTotal"
        gridTotal.ReadOnlyGrid = True
        gridTotal.Selectable = False
        gridTotal.RowHeight = 24
        gridTotal.ShowHeader = False
        gridTotal.Size = New Size(955, 38)
        gridTotal.TabIndex = 4
        tips.SetToolTipHeader(gridTotal, "Bugetul în vigoare, cu rectificări")
        tips.SetToolTipText(gridTotal, "Ultimul buget (cel cu data cea mai nouă) plus TOATE rectificările de mai sus, pe trimestre." & vbLf & "La un nod, însumează toate clasificațiile de sub el.")
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.AutoSize = True
        btnSalveaza.Enabled = False
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(1186, 6)
        btnSalveaza.Margin = New Padding(0, 0, 12, 0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(26, 4, 26, 4)
        btnSalveaza.Size = New Size(198, 60)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Scrie în baza de date versiunile de buget și rectificările clasificației alese.")
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' btnVerifica
        ' 
        btnVerifica.AutoSize = True
        btnVerifica.FlatStyle = FlatStyle.Flat
        btnVerifica.Location = New Point(600, 6)
        btnVerifica.Margin = New Padding(0, 0, 12, 0)
        btnVerifica.Name = "btnVerifica"
        btnVerifica.Padding = New Padding(26, 4, 26, 4)
        btnVerifica.Size = New Size(198, 60)
        btnVerifica.TabIndex = 3
        btnVerifica.Text = "Verifică bugetul"
        tips.SetToolTipHeader(btnVerifica, "Verifică bugetul față de FOREXE")
        tips.SetToolTipText(btnVerifica, "Compară bugetul + rectificările de azi cu creditul bugetar descărcat din FOREXE, pe fiecare clasificație." & vbLf & "Arată fiecare clasificație cu cele două valori și diferența.")
        btnVerifica.UseVisualStyleBackColor = True
        ' 
        ' btnTrimiteAccess
        ' 
        btnTrimiteAccess.AutoSize = True
        btnTrimiteAccess.Enabled = False
        btnTrimiteAccess.FlatStyle = FlatStyle.Flat
        btnTrimiteAccess.Location = New Point(800, 6)
        btnTrimiteAccess.Margin = New Padding(0, 0, 12, 0)
        btnTrimiteAccess.Name = "btnTrimiteAccess"
        btnTrimiteAccess.Padding = New Padding(26, 4, 26, 4)
        btnTrimiteAccess.Size = New Size(198, 60)
        btnTrimiteAccess.TabIndex = 4
        btnTrimiteAccess.Text = "Trimite în Access"
        tips.SetToolTipHeader(btnTrimiteAccess, "Trimite în Access")
        tips.SetToolTipText(btnTrimiteAccess, "Scrie în baza Access a unității (din registrul AVACONT) bugetul în vigoare azi și rectificările anului clasificației alese." & vbLf & "Se activează după salvare.")
        btnTrimiteAccess.UseVisualStyleBackColor = True
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(pnlCard, 0, 1)
        tlyMain.Controls.Add(tlySubsol, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(2, 2)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 78F))
        tlyMain.Size = New Size(1599, 776)
        tlyMain.TabIndex = 0
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.cells
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(1599, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Clasificații bugetare"
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlyBody)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 66)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(12, 9, 12, 9)
        pnlCard.Size = New Size(1599, 632)
        pnlCard.TabIndex = 1
        pnlCard.Tag = "Card"
        ' 
        ' tlyBody
        ' 
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 39.42857F))
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 60.57143F))
        tlyBody.Controls.Add(tree, 0, 0)
        tlyBody.Controls.Add(tlyBife, 0, 1)
        tlyBody.Controls.Add(tlyRight, 1, 0)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(12, 9)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 2
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.SetRowSpan(tlyRight, 2)
        tlyBody.Size = New Size(1575, 614)
        tlyBody.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.BackColor = SystemColors.Window
        tree.BorderColor = SystemColors.ActiveBorder
        tree.Dock = DockStyle.Fill
        tree.FooterCaption = "Adaugă clasificații"
        tree.FooterHeight = 30
        tree.FooterIconSize = New Size(18, 18)
        tree.FooterRightIcon = My.Resources.Resources.plus_green
        tree.FooterRightIconTooltip = "Adaugă clasificații" & vbLf & "Alegeți sursa, clasificațiile funcționale și pe cele economice," & vbLf & "ca la înregistrarea unității."
        tree.FooterTextAlign = ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderCaption = " CLASIFICAȚII"
        tree.HeaderHeight = 24
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderVisible = True
        tree.Indent = 14
        tree.LeftIconSize = New Size(16, 16)
        tree.LeftTextWidth = 80
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(0, 0, 12, 0)
        tree.Name = "tree"
        tree.RightTextColumn = 150
        tree.SearchBackColor = SystemColors.Control
        tree.SearchDefaultText = "… tastați o parte din cod sau din denumire …"
        tree.SearchSeparatorColor = SystemColors.ActiveBorder
        tree.SearchSeparatorWidth = 2
        tree.SearchShow = True
        tree.Size = New Size(608, 614)
        tree.TabIndex = 0
        ' 
        ' tlyBife
        ' 
        tlyBife.AutoFitToTheme = False
        tlyBife.AutoSize = True
        tlyBife.ColumnCount = 2
        tlyBife.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBife.ColumnStyles.Add(New ColumnStyle())
        tlyBife.Controls.Add(chkToate, 0, 0)
        tlyBife.Controls.Add(chkForexe, 1, 0)
        tlyBife.Dock = DockStyle.Fill
        tlyBife.Location = New Point(0, 614)
        tlyBife.Margin = New Padding(0, 0, 12, 0)
        tlyBife.Name = "tlyBife"
        tlyBife.RowCount = 1
        tlyBife.RowStyles.Add(New RowStyle())
        tlyBife.Size = New Size(608, 39)
        tlyBife.TabIndex = 2
        ' 
        ' chkForexe
        ' 
        chkForexe.AutoSize = True
        chkForexe.Anchor = AnchorStyles.Right
        chkForexe.FlatStyle = FlatStyle.Flat
        chkForexe.Location = New Point(340, 9)
        chkForexe.Margin = New Padding(12, 9, 0, 0)
        chkForexe.Name = "chkForexe"
        chkForexe.Size = New Size(268, 30)
        chkForexe.TabIndex = 1
        chkForexe.Text = "Arată DOAR clasificațiile folosite în FOREXE"
        tips.SetToolTipHeader(chkForexe, "Doar clasificațiile folosite în FOREXE")
        tips.SetToolTipText(chkForexe, "Bifat: arborele arată doar clasificațiile pentru care FOREXE a raportat un credit bugetar." & vbLf & "Are prioritate față de «Arată toate clasificațiile».")
        chkForexe.UseVisualStyleBackColor = True
        ' 
        ' chkToate
        ' 
        chkToate.AutoSize = True
        chkToate.FlatStyle = FlatStyle.Flat
        chkToate.Location = New Point(0, 9)
        chkToate.Margin = New Padding(0, 9, 0, 0)
        chkToate.Name = "chkToate"
        chkToate.Size = New Size(260, 30)
        chkToate.TabIndex = 0
        chkToate.Text = "Arată toate clasificațiile"
        tips.SetToolTipHeader(chkToate, "Arată toate clasificațiile")
        tips.SetToolTipText(chkToate, "Debifat: arborele arată doar clasificațiile cu mișcare în an (vreun trimestru de buget sau de rectificare diferit de zero)." & vbLf & "Bifat: toate clasificațiile configurate.")
        chkToate.UseVisualStyleBackColor = True
        ' 
        ' tlyRight
        ' 
        tlyRight.ColumnCount = 1
        tlyRight.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRight.Controls.Add(lblBuget, 0, 0)
        tlyRight.Controls.Add(gridBuget, 0, 1)
        tlyRight.Controls.Add(lblRectificari, 0, 2)
        tlyRight.Controls.Add(gridRectificari, 0, 3)
        tlyRight.Controls.Add(gridTotal, 0, 4)
        tlyRight.Dock = DockStyle.Fill
        tlyRight.Location = New Point(620, 0)
        tlyRight.Margin = New Padding(0)
        tlyRight.Name = "tlyRight"
        tlyRight.RowCount = 5
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 45F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Percent, 40F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 57F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Percent, 60F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyRight.Size = New Size(955, 614)
        tlyRight.TabIndex = 1
        ' 
        ' lblBuget
        ' 
        lblBuget.AutoEllipsis = True
        lblBuget.Dock = DockStyle.Fill
        lblBuget.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lblBuget.Location = New Point(0, 0)
        lblBuget.Margin = New Padding(0)
        lblBuget.Name = "lblBuget"
        lblBuget.Size = New Size(955, 45)
        lblBuget.TabIndex = 0
        lblBuget.Text = "Buget"
        lblBuget.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblRectificari
        ' 
        lblRectificari.AutoEllipsis = True
        lblRectificari.Dock = DockStyle.Fill
        lblRectificari.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lblRectificari.Location = New Point(0, 147)
        lblRectificari.Margin = New Padding(0, 12, 0, 0)
        lblRectificari.Name = "lblRectificari"
        lblRectificari.Size = New Size(955, 45)
        lblRectificari.TabIndex = 2
        lblRectificari.Text = "Rectificări bugetare"
        lblRectificari.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' tlySubsol
        ' 
        tlySubsol.AutoFitToTheme = False
        tlySubsol.ColumnCount = 5
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(lblStare, 0, 0)
        tlySubsol.Controls.Add(btnVerifica, 1, 0)
        tlySubsol.Controls.Add(btnTrimiteAccess, 2, 0)
        tlySubsol.Controls.Add(btnSalveaza, 3, 0)
        tlySubsol.Controls.Add(btnInchide, 4, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 698)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(8, 6, 8, 6)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(1599, 78)
        tlySubsol.TabIndex = 2
        ' 
        ' lblStare
        ' 
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(8, 6)
        lblStare.Margin = New Padding(0, 0, 12, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(1166, 66)
        lblStare.TabIndex = 0
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnInchide
        ' 
        btnInchide.AutoSize = True
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Location = New Point(1396, 6)
        btnInchide.Margin = New Padding(0)
        btnInchide.Name = "btnInchide"
        btnInchide.Padding = New Padding(26, 4, 26, 4)
        btnInchide.Size = New Size(195, 60)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        ' 
        ' ClasificatiiForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1603, 780)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MinimizeBox = False
        MinimumSize = New Size(1350, 780)
        Name = "ClasificatiiForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Clasificații bugetare"
        CType(gridBuget, ComponentModel.ISupportInitialize).EndInit()
        CType(gridRectificari, ComponentModel.ISupportInitialize).EndInit()
        CType(gridTotal, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyBody.ResumeLayout(False)
        tlyBife.ResumeLayout(False)
        tlyBife.PerformLayout()
        tlyRight.ResumeLayout(False)
        tlySubsol.ResumeLayout(False)
        tlySubsol.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents tlyBife As KBotTableLayoutPanel
    Friend WithEvents chkToate As CheckBox
    Friend WithEvents chkForexe As CheckBox
    Friend WithEvents tlyRight As KBotTableLayoutPanel
    Friend WithEvents lblBuget As Label
    Friend WithEvents gridBuget As KBotDataView
    Friend WithEvents lblRectificari As Label
    Friend WithEvents gridRectificari As KBotDataView
    Friend WithEvents gridTotal As KBotDataView
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnSalveaza As Button
    Friend WithEvents btnVerifica As Button
    Friend WithEvents btnTrimiteAccess As Button
    Friend WithEvents btnInchide As Button
End Class
