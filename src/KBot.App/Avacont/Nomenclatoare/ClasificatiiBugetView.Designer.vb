Imports KBot.Controls

' «Buget» view of the «Clasificatii» window (slice AV01-11): the expenses budget, the former content of the window.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ClasificatiiBugetView
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
        tips =New KBotToolTip(components)
        gridBuget = New KBotDataView()
        gridRectificari = New KBotDataView()
        gridTotal = New KBotDataView()
        gridAngajamente = New KBotDataView()
        tlyMain = New KBotTableLayoutPanel()
        pnlCard = New Panel()
        tlyBody = New KBotTableLayoutPanel()
        tree = New AdvancedTreeControl()
        tlyRight = New KBotTableLayoutPanel()
        CType(gridBuget, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridRectificari, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridTotal, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridAngajamente, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        tlyRight.SuspendLayout()
        SuspendLayout()
        ' 
        ' gridBuget
        ' 
        gridBuget.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridBuget.ColumnFillMode = KBotFillMode.SpecificColumn
        gridBuget.FillColumnKey = "clsf"
        KBotDataColumn17.AggregateFormatString = Nothing
        KBotDataColumn17.FormatString = Nothing
        KBotDataColumn17.HeaderText = "Buget"
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
        KBotDataColumn5.ButtonKind = KBotButtonKind.Delete
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
        gridBuget.FooterCaption = "Versiuni de buget"
        gridBuget.FooterVisible = True
        gridBuget.Location = New Point(0, 45)
        gridBuget.Margin = New Padding(0)
        gridBuget.Name = "gridBuget"
        gridBuget.RowHeight = 24
        gridBuget.Size = New Size(955, 200)
        gridBuget.TabIndex = 1
        ' tips.SetToolTipHeader(gridBuget, "Bugetul clasificatiei, pe versiuni")
        ' tips.SetToolTipText(gridBuget, "Fiecare rand este bugetul de la data din «Inceput» incolo (trimestrele 1 - 4); documentul de fundamentare citeste bugetul de la data revizuirii." & vbLf & "«+» din subsol adauga o versiune; «✕» o sterge. Totul se scrie la «Salveaza».")
        ' 
        ' gridRectificari
        ' 
        gridRectificari.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridRectificari.ColumnFillMode = KBotFillMode.SpecificColumn
        gridRectificari.FillColumnKey = "document"
        KBotDataColumn18.AggregateFormatString = Nothing
        KBotDataColumn18.FormatString = Nothing
        KBotDataColumn18.HeaderText = "Rectificări bugetare"
        KBotDataColumn18.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn18.Key = "clsf"
        KBotDataColumn18.MinWidth = 150
        KBotDataColumn18.OptionGroup = Nothing
        KBotDataColumn18.ReadOnly = True
        KBotDataColumn18.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn18.Width = 150
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Rectificări bugetare — Nr. doc."
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
        KBotDataColumn13.ButtonKind = KBotButtonKind.Delete
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
        gridRectificari.FooterCaption = "Total"
        gridRectificari.FooterVisible = True
        gridRectificari.Location = New Point(0, 192)
        gridRectificari.Margin = New Padding(0, 12, 0, 0)
        gridRectificari.Name = "gridRectificari"
        gridRectificari.Size = New Size(955, 422)
        gridRectificari.TabIndex = 3
        ' tips.SetToolTipHeader(gridRectificari, "Rectificarile bugetare ale anului")
        ' tips.SetToolTipText(gridRectificari, "Nr. doc. si data sunt obligatorii; data trebuie sa fie in anul de lucru." & vbLf & "«+» din subsol adauga un rand; «✕» il sterge. Totul se scrie la «Salveaza».")
        '
        ' gridTotal
        '
        gridTotal.AutoSizeColumnsMode = KBotAutoSizeMode.None
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
        gridTotal.Font = New Font("Calibri", 9F, FontStyle.Bold)
        gridTotal.Location = New Point(0, 604)
        gridTotal.Margin = New Padding(0, 12, 0, 0)
        gridTotal.Name = "gridTotal"
        gridTotal.ReadOnlyGrid = True
        gridTotal.Selectable = False
        gridTotal.RowHeight = 24
        gridTotal.ShowHeader = False
        gridTotal.Size = New Size(955, 38)
        gridTotal.TabIndex = 4
        ' tips.SetToolTipHeader(gridTotal, "Bugetul in vigoare, cu rectificari")
        ' tips.SetToolTipText(gridTotal, "Ultimul buget (cel cu data cea mai noua) plus TOATE rectificarile de mai sus, pe trimestre." & vbLf & "La un nod, insumeaza toate clasificatiile de sub el.")
        '
        ' gridAngajamente
        '
        gridAngajamente.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridAngajamente.ColumnFillMode = KBotFillMode.SpecificColumn
        gridAngajamente.FillColumnKey = "angajament"
        KBotDataColumn28.AggregateFormatString = Nothing
        KBotDataColumn28.FormatString = Nothing
        KBotDataColumn28.HeaderText = "Angajamente"
        KBotDataColumn28.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn28.Key = "angajament"
        KBotDataColumn28.MinWidth = 150
        KBotDataColumn28.OptionGroup = Nothing
        KBotDataColumn28.ReadOnly = True
        KBotDataColumn28.Width = 150
        KBotDataColumn29.AggregateFormatString = Nothing
        KBotDataColumn29.FormatString = Nothing
        KBotDataColumn29.HeaderText = "Cod indicator"
        KBotDataColumn29.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn29.Key = "indicator"
        KBotDataColumn29.OptionGroup = Nothing
        KBotDataColumn29.ReadOnly = True
        KBotDataColumn29.Width = 130
        KBotDataColumn30.Aggregate = KBotAggregate.Sum
        KBotDataColumn30.AggregateFormatString = Nothing
        KBotDataColumn30.DecimalPlaces = 2
        KBotDataColumn30.Format = KBotFormat.Standard
        KBotDataColumn30.FormatString = Nothing
        KBotDataColumn30.HeaderText = "Total rezervări"
        KBotDataColumn30.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn30.Key = "rezervari"
        KBotDataColumn30.OptionGroup = Nothing
        KBotDataColumn30.ReadOnly = True
        KBotDataColumn30.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn30.ValueType = KBotValueType.Number
        KBotDataColumn30.Width = 130
        KBotDataColumn31.Aggregate = KBotAggregate.Sum
        KBotDataColumn31.AggregateFormatString = Nothing
        KBotDataColumn31.DecimalPlaces = 2
        KBotDataColumn31.Format = KBotFormat.Standard
        KBotDataColumn31.FormatString = Nothing
        KBotDataColumn31.HeaderText = "Total recepții"
        KBotDataColumn31.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn31.Key = "receptii"
        KBotDataColumn31.OptionGroup = Nothing
        KBotDataColumn31.ReadOnly = True
        KBotDataColumn31.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn31.ValueType = KBotValueType.Number
        KBotDataColumn31.Width = 130
        KBotDataColumn32.Aggregate = KBotAggregate.Sum
        KBotDataColumn32.AggregateFormatString = Nothing
        KBotDataColumn32.DecimalPlaces = 2
        KBotDataColumn32.Format = KBotFormat.Standard
        KBotDataColumn32.FormatString = Nothing
        KBotDataColumn32.HeaderText = "Total plăți"
        KBotDataColumn32.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn32.Key = "plati"
        KBotDataColumn32.OptionGroup = Nothing
        KBotDataColumn32.ReadOnly = True
        KBotDataColumn32.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn32.ValueType = KBotValueType.Number
        KBotDataColumn32.Width = 130
        gridAngajamente.Columns.Add(KBotDataColumn28)
        gridAngajamente.Columns.Add(KBotDataColumn29)
        gridAngajamente.Columns.Add(KBotDataColumn30)
        gridAngajamente.Columns.Add(KBotDataColumn31)
        gridAngajamente.Columns.Add(KBotDataColumn32)
        gridAngajamente.Dock = DockStyle.Fill
        gridAngajamente.FooterCaption = "Total"
        gridAngajamente.FooterVisible = True
        gridAngajamente.Location = New Point(0, 760)
        gridAngajamente.Margin = New Padding(0, 12, 0, 0)
        gridAngajamente.Name = "gridAngajamente"
        gridAngajamente.ReadOnlyGrid = True
        gridAngajamente.RowHeight = 24
        gridAngajamente.Size = New Size(955, 180)
        gridAngajamente.TabIndex = 6
        '
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(pnlCard, 0, 0)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 1
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.Size = New Size(1599, 1044)
        tlyMain.TabIndex = 0
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlyBody)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 0)
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
        tlyBody.Controls.Add(tlyRight, 1, 0)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(12, 9)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.Size = New Size(1575, 614)
        tlyBody.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.Dock = DockStyle.Fill
        tree.FooterCaptionStandalone = True
        tree.FooterVisible = True
        tree.HeaderCaption = " CLASIFICAȚII"
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        ' Slice 0126-04: the two filters of the tree are in the popup of this icon (they were two tick boxes under the tree).
        tree.HeaderRightIcon = My.Resources.Resources.settings__1_
        tree.HeaderRightIconTooltip = "Opțiunile arborelui: ce clasificații se arată." & vbLf & "«Arată toate clasificațiile» și «Arată DOAR clasificațiile folosite în FOREXE»."
        tree.HeaderVisible = True
        tree.Indent = 14
        tree.LeftIconSize = New Size(16, 16)
        tree.LeftTextWidth = 80
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(0, 0, 12, 0)
        tree.Name = "tree"
        tree.RightTextColumn = 150
        tree.SearchDefaultText = "… tastați o parte din cod sau din denumire …"
        tree.SearchShow = True
        tree.Size = New Size(608, 614)
        tree.TabIndex = 0
        ' 
        ' tlyRight
        ' 
        tlyRight.ColumnCount = 1
        tlyRight.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRight.Controls.Add(gridBuget, 0, 0)
        tlyRight.Controls.Add(gridRectificari, 0, 1)
        tlyRight.Controls.Add(gridTotal, 0, 2)
        tlyRight.Controls.Add(gridAngajamente, 0, 3)
        tlyRight.Dock = DockStyle.Fill
        tlyRight.Location = New Point(620, 0)
        tlyRight.Margin = New Padding(0)
        tlyRight.Name = "tlyRight"
        tlyRight.RowCount = 4
        ' Slice 0126-06: the titles that stood in labels above the grids are the header of the first column of each grid. The heights of the
        ' two grids (rows 0 and 1) are set in code to fit their rows (row 1 + the 12 of the gap above the grid); these are the same figures
        ' at the designer dpi.
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 222F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 216F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRight.Size = New Size(955, 614)
        tlyRight.TabIndex = 1
        ' 
        ' ClasificatiiBugetView
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Margin = New Padding(0)
        Name = "ClasificatiiBugetView"
        Size = New Size(1599, 1044)
        CType(gridBuget, ComponentModel.ISupportInitialize).EndInit()
        CType(gridRectificari, ComponentModel.ISupportInitialize).EndInit()
        CType(gridTotal, ComponentModel.ISupportInitialize).EndInit()
        CType(gridAngajamente, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyBody.ResumeLayout(False)
        tlyRight.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents tlyRight As KBotTableLayoutPanel
    Friend WithEvents gridBuget As KBotDataView
    Friend WithEvents gridRectificari As KBotDataView
    Friend WithEvents gridTotal As KBotDataView
    Friend WithEvents gridAngajamente As KBotDataView
End Class
