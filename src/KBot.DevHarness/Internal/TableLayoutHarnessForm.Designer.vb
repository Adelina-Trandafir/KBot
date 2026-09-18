<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class TableLayoutHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    ' The bench of slice 0066: a KBotTableLayoutPanel whose fixed rows, fixed columns and padding
    ' are authored in LOGICAL pixels, next to an AdvancedTreeControl whose ItemHeight is the same
    ' logical number -- so the operator can see the two scale by the same factor. The scaling
    ' mode (Automatic / Fixed 100% / Manual), the text size and the scheme are switchable from
    ' the bench; the table's own switches and its runtime API (collapse, SetRowHeight, Padding)
    ' have a button each. Controls declared here (house rule).

    Friend WithEvents pnlTop As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnClassic As System.Windows.Forms.Button
    Friend WithEvents btnDark As System.Windows.Forms.Button
    Friend WithEvents btnModern As System.Windows.Forms.Button
    Friend WithEvents btnColorful As System.Windows.Forms.Button
    Friend WithEvents lblActive As System.Windows.Forms.Label

    Friend WithEvents pnlScale As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblMode As System.Windows.Forms.Label
    Friend WithEvents rdoAuto As System.Windows.Forms.RadioButton
    Friend WithEvents rdoFixed As System.Windows.Forms.RadioButton
    Friend WithEvents rdoManual As System.Windows.Forms.RadioButton
    Friend WithEvents numFactor As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnApplyMode As System.Windows.Forms.Button
    Friend WithEvents lblText As System.Windows.Forms.Label
    Friend WithEvents trkText As System.Windows.Forms.TrackBar
    Friend WithEvents lblTextValue As System.Windows.Forms.Label

    Friend WithEvents pnlTableOptions As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents chkThemedBorder As System.Windows.Forms.CheckBox
    Friend WithEvents chkScaleStyles As System.Windows.Forms.CheckBox
    Friend WithEvents chkAutoFit As System.Windows.Forms.CheckBox
    Friend WithEvents btnCollapseRow As System.Windows.Forms.Button
    Friend WithEvents btnExpandRow As System.Windows.Forms.Button
    Friend WithEvents btnCollapseCol As System.Windows.Forms.Button
    Friend WithEvents btnExpandCol As System.Windows.Forms.Button
    Friend WithEvents btnRowTall As System.Windows.Forms.Button
    Friend WithEvents btnRowBack As System.Windows.Forms.Button
    Friend WithEvents btnPadWide As System.Windows.Forms.Button
    Friend WithEvents btnPadBack As System.Windows.Forms.Button
    Friend WithEvents btnReadout As System.Windows.Forms.Button

    Friend WithEvents tlyHost As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents tlyProbe As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblNume As System.Windows.Forms.Label
    Friend WithEvents txtNume As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblData As System.Windows.Forms.Label
    Friend WithEvents txtData As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblCod As System.Windows.Forms.Label
    Friend WithEvents txtCod As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblSuma As System.Windows.Forms.Label
    Friend WithEvents txtSuma As Global.KBot.Controls.KBotTextField
    Friend WithEvents btnSalveaza As System.Windows.Forms.Button
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
    Friend WithEvents lblBanda As System.Windows.Forms.Label
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents treeProbe As Global.KBot.Controls.AdvancedTreeControl

    Friend WithEvents lstLog As System.Windows.Forms.ListBox

    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnFail As System.Windows.Forms.Button
    Friend WithEvents btnPass As System.Windows.Forms.Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlTop = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnClassic = New System.Windows.Forms.Button()
        Me.btnDark = New System.Windows.Forms.Button()
        Me.btnModern = New System.Windows.Forms.Button()
        Me.btnColorful = New System.Windows.Forms.Button()
        Me.lblActive = New System.Windows.Forms.Label()
        Me.pnlScale = New System.Windows.Forms.FlowLayoutPanel()
        Me.lblMode = New System.Windows.Forms.Label()
        Me.rdoAuto = New System.Windows.Forms.RadioButton()
        Me.rdoFixed = New System.Windows.Forms.RadioButton()
        Me.rdoManual = New System.Windows.Forms.RadioButton()
        Me.numFactor = New System.Windows.Forms.NumericUpDown()
        Me.btnApplyMode = New System.Windows.Forms.Button()
        Me.lblText = New System.Windows.Forms.Label()
        Me.trkText = New System.Windows.Forms.TrackBar()
        Me.lblTextValue = New System.Windows.Forms.Label()
        Me.pnlTableOptions = New System.Windows.Forms.FlowLayoutPanel()
        Me.chkThemedBorder = New System.Windows.Forms.CheckBox()
        Me.chkScaleStyles = New System.Windows.Forms.CheckBox()
        Me.chkAutoFit = New System.Windows.Forms.CheckBox()
        Me.btnCollapseRow = New System.Windows.Forms.Button()
        Me.btnExpandRow = New System.Windows.Forms.Button()
        Me.btnCollapseCol = New System.Windows.Forms.Button()
        Me.btnExpandCol = New System.Windows.Forms.Button()
        Me.btnRowTall = New System.Windows.Forms.Button()
        Me.btnRowBack = New System.Windows.Forms.Button()
        Me.btnPadWide = New System.Windows.Forms.Button()
        Me.btnPadBack = New System.Windows.Forms.Button()
        Me.btnReadout = New System.Windows.Forms.Button()
        Me.tlyHost = New Global.KBot.Controls.KBotTableLayoutPanel()
        Me.tlyProbe = New Global.KBot.Controls.KBotTableLayoutPanel()
        Me.lblNume = New System.Windows.Forms.Label()
        Me.txtNume = New Global.KBot.Controls.KBotTextField()
        Me.lblData = New System.Windows.Forms.Label()
        Me.txtData = New Global.KBot.Controls.KBotTextField()
        Me.lblCod = New System.Windows.Forms.Label()
        Me.txtCod = New Global.KBot.Controls.KBotTextField()
        Me.lblSuma = New System.Windows.Forms.Label()
        Me.txtSuma = New Global.KBot.Controls.KBotTextField()
        Me.btnSalveaza = New System.Windows.Forms.Button()
        Me.btnRenunta = New System.Windows.Forms.Button()
        Me.lblBanda = New System.Windows.Forms.Label()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.treeProbe = New Global.KBot.Controls.AdvancedTreeControl()
        Me.lstLog = New System.Windows.Forms.ListBox()
        Me.pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.btnPass = New System.Windows.Forms.Button()
        Me.pnlTop.SuspendLayout()
        Me.pnlScale.SuspendLayout()
        CType(Me.numFactor, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trkText, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlTableOptions.SuspendLayout()
        Me.tlyHost.SuspendLayout()
        Me.tlyProbe.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlTop
        '
        Me.pnlTop.Controls.Add(Me.btnClassic)
        Me.pnlTop.Controls.Add(Me.btnDark)
        Me.pnlTop.Controls.Add(Me.btnModern)
        Me.pnlTop.Controls.Add(Me.btnColorful)
        Me.pnlTop.Controls.Add(Me.lblActive)
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Height = 44
        Me.pnlTop.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlTop.Name = "pnlTop"
        '
        'btnClassic
        '
        Me.btnClassic.AutoSize = True
        Me.btnClassic.Text = "Classic"
        Me.btnClassic.UseVisualStyleBackColor = True
        Me.btnClassic.Name = "btnClassic"
        '
        'btnDark
        '
        Me.btnDark.AutoSize = True
        Me.btnDark.Text = "Dark"
        Me.btnDark.UseVisualStyleBackColor = True
        Me.btnDark.Name = "btnDark"
        '
        'btnModern
        '
        Me.btnModern.AutoSize = True
        Me.btnModern.Text = "Modern"
        Me.btnModern.UseVisualStyleBackColor = True
        Me.btnModern.Name = "btnModern"
        '
        'btnColorful
        '
        Me.btnColorful.AutoSize = True
        Me.btnColorful.Text = "Colorat"
        Me.btnColorful.UseVisualStyleBackColor = True
        Me.btnColorful.Name = "btnColorful"
        '
        'lblActive
        '
        Me.lblActive.AutoSize = True
        Me.lblActive.Margin = New System.Windows.Forms.Padding(12, 9, 3, 0)
        Me.lblActive.Text = "activ: —"
        Me.lblActive.Name = "lblActive"
        '
        'pnlScale
        '
        Me.pnlScale.Controls.Add(Me.lblMode)
        Me.pnlScale.Controls.Add(Me.rdoAuto)
        Me.pnlScale.Controls.Add(Me.rdoFixed)
        Me.pnlScale.Controls.Add(Me.rdoManual)
        Me.pnlScale.Controls.Add(Me.numFactor)
        Me.pnlScale.Controls.Add(Me.btnApplyMode)
        Me.pnlScale.Controls.Add(Me.lblText)
        Me.pnlScale.Controls.Add(Me.trkText)
        Me.pnlScale.Controls.Add(Me.lblTextValue)
        Me.pnlScale.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlScale.Height = 46
        Me.pnlScale.Padding = New System.Windows.Forms.Padding(6, 4, 6, 4)
        Me.pnlScale.Name = "pnlScale"
        '
        'lblMode
        '
        Me.lblMode.AutoSize = True
        Me.lblMode.Margin = New System.Windows.Forms.Padding(3, 9, 3, 0)
        Me.lblMode.Text = "Scara K-BOT:"
        Me.lblMode.Name = "lblMode"
        '
        'rdoAuto
        '
        Me.rdoAuto.AutoSize = True
        Me.rdoAuto.Checked = True
        Me.rdoAuto.Margin = New System.Windows.Forms.Padding(3, 6, 3, 3)
        Me.rdoAuto.TabStop = True
        Me.rdoAuto.Text = "automată (DPI)"
        Me.rdoAuto.UseVisualStyleBackColor = True
        Me.rdoAuto.Name = "rdoAuto"
        '
        'rdoFixed
        '
        Me.rdoFixed.AutoSize = True
        Me.rdoFixed.Margin = New System.Windows.Forms.Padding(3, 6, 3, 3)
        Me.rdoFixed.Text = "fix 100%"
        Me.rdoFixed.UseVisualStyleBackColor = True
        Me.rdoFixed.Name = "rdoFixed"
        '
        'rdoManual
        '
        Me.rdoManual.AutoSize = True
        Me.rdoManual.Margin = New System.Windows.Forms.Padding(3, 6, 3, 3)
        Me.rdoManual.Text = "manual ×"
        Me.rdoManual.UseVisualStyleBackColor = True
        Me.rdoManual.Name = "rdoManual"
        '
        'numFactor
        '
        Me.numFactor.DecimalPlaces = 2
        Me.numFactor.Increment = New Decimal(New Integer() {25, 0, 0, 131072})
        Me.numFactor.Margin = New System.Windows.Forms.Padding(0, 5, 3, 3)
        Me.numFactor.Maximum = New Decimal(New Integer() {4, 0, 0, 0})
        Me.numFactor.Minimum = New Decimal(New Integer() {5, 0, 0, 65536})
        Me.numFactor.Size = New System.Drawing.Size(64, 23)
        Me.numFactor.Value = New Decimal(New Integer() {15, 0, 0, 65536})
        Me.numFactor.Name = "numFactor"
        '
        'btnApplyMode
        '
        Me.btnApplyMode.AutoSize = True
        Me.btnApplyMode.Margin = New System.Windows.Forms.Padding(3, 3, 12, 3)
        Me.btnApplyMode.Text = "Aplică scara"
        Me.btnApplyMode.UseVisualStyleBackColor = True
        Me.btnApplyMode.Name = "btnApplyMode"
        '
        'lblText
        '
        Me.lblText.AutoSize = True
        Me.lblText.Margin = New System.Windows.Forms.Padding(3, 9, 3, 0)
        Me.lblText.Text = "Mărime text:"
        Me.lblText.Name = "lblText"
        '
        'trkText
        '
        Me.trkText.AutoSize = False
        Me.trkText.LargeChange = 25
        Me.trkText.Maximum = 200
        Me.trkText.Minimum = 75
        Me.trkText.Size = New System.Drawing.Size(180, 30)
        Me.trkText.SmallChange = 5
        Me.trkText.TickFrequency = 25
        Me.trkText.TickStyle = System.Windows.Forms.TickStyle.BottomRight
        Me.trkText.Value = 100
        Me.trkText.Name = "trkText"
        '
        'lblTextValue
        '
        Me.lblTextValue.AutoSize = True
        Me.lblTextValue.Margin = New System.Windows.Forms.Padding(3, 9, 3, 0)
        Me.lblTextValue.Text = "100%"
        Me.lblTextValue.Name = "lblTextValue"
        '
        'pnlTableOptions
        '
        Me.pnlTableOptions.Controls.Add(Me.chkThemedBorder)
        Me.pnlTableOptions.Controls.Add(Me.chkScaleStyles)
        Me.pnlTableOptions.Controls.Add(Me.chkAutoFit)
        Me.pnlTableOptions.Controls.Add(Me.btnCollapseRow)
        Me.pnlTableOptions.Controls.Add(Me.btnExpandRow)
        Me.pnlTableOptions.Controls.Add(Me.btnCollapseCol)
        Me.pnlTableOptions.Controls.Add(Me.btnExpandCol)
        Me.pnlTableOptions.Controls.Add(Me.btnRowTall)
        Me.pnlTableOptions.Controls.Add(Me.btnRowBack)
        Me.pnlTableOptions.Controls.Add(Me.btnPadWide)
        Me.pnlTableOptions.Controls.Add(Me.btnPadBack)
        Me.pnlTableOptions.Controls.Add(Me.btnReadout)
        Me.pnlTableOptions.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTableOptions.Height = 74
        Me.pnlTableOptions.Padding = New System.Windows.Forms.Padding(6, 4, 6, 4)
        Me.pnlTableOptions.Name = "pnlTableOptions"
        '
        'chkThemedBorder
        '
        Me.chkThemedBorder.AutoSize = True
        Me.chkThemedBorder.Checked = True
        Me.chkThemedBorder.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkThemedBorder.Margin = New System.Windows.Forms.Padding(3, 6, 12, 3)
        Me.chkThemedBorder.Text = "linii de celulă din temă"
        Me.chkThemedBorder.UseVisualStyleBackColor = True
        Me.chkThemedBorder.Name = "chkThemedBorder"
        '
        'chkScaleStyles
        '
        Me.chkScaleStyles.AutoSize = True
        Me.chkScaleStyles.Checked = True
        Me.chkScaleStyles.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkScaleStyles.Margin = New System.Windows.Forms.Padding(3, 6, 12, 3)
        Me.chkScaleStyles.Text = "măsurile fixe + marginea la scara K-BOT"
        Me.chkScaleStyles.UseVisualStyleBackColor = True
        Me.chkScaleStyles.Name = "chkScaleStyles"
        '
        'chkAutoFit
        '
        Me.chkAutoFit.AutoSize = True
        Me.chkAutoFit.Checked = True
        Me.chkAutoFit.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAutoFit.Margin = New System.Windows.Forms.Padding(3, 6, 12, 3)
        Me.chkAutoFit.Text = "rândurile fixe cresc la conținut"
        Me.chkAutoFit.UseVisualStyleBackColor = True
        Me.chkAutoFit.Name = "chkAutoFit"
        '
        'btnCollapseRow
        '
        Me.btnCollapseRow.AutoSize = True
        Me.btnCollapseRow.Text = "Strânge banda (rând 3)"
        Me.btnCollapseRow.UseVisualStyleBackColor = True
        Me.btnCollapseRow.Name = "btnCollapseRow"
        '
        'btnExpandRow
        '
        Me.btnExpandRow.AutoSize = True
        Me.btnExpandRow.Text = "Desface banda"
        Me.btnExpandRow.UseVisualStyleBackColor = True
        Me.btnExpandRow.Name = "btnExpandRow"
        '
        'btnCollapseCol
        '
        Me.btnCollapseCol.AutoSize = True
        Me.btnCollapseCol.Text = "Strânge coloana 3"
        Me.btnCollapseCol.UseVisualStyleBackColor = True
        Me.btnCollapseCol.Name = "btnCollapseCol"
        '
        'btnExpandCol
        '
        Me.btnExpandCol.AutoSize = True
        Me.btnExpandCol.Text = "Desface coloana 3"
        Me.btnExpandCol.UseVisualStyleBackColor = True
        Me.btnExpandCol.Name = "btnExpandCol"
        '
        'btnRowTall
        '
        Me.btnRowTall.AutoSize = True
        Me.btnRowTall.Text = "Rândul 0 → 60 logic"
        Me.btnRowTall.UseVisualStyleBackColor = True
        Me.btnRowTall.Name = "btnRowTall"
        '
        'btnRowBack
        '
        Me.btnRowBack.AutoSize = True
        Me.btnRowBack.Text = "Rândul 0 → 32 logic"
        Me.btnRowBack.UseVisualStyleBackColor = True
        Me.btnRowBack.Name = "btnRowBack"
        '
        'btnPadWide
        '
        Me.btnPadWide.AutoSize = True
        Me.btnPadWide.Text = "Padding → 24 logic"
        Me.btnPadWide.UseVisualStyleBackColor = True
        Me.btnPadWide.Name = "btnPadWide"
        '
        'btnPadBack
        '
        Me.btnPadBack.AutoSize = True
        Me.btnPadBack.Text = "Padding → 8 logic"
        Me.btnPadBack.UseVisualStyleBackColor = True
        Me.btnPadBack.Name = "btnPadBack"
        '
        'btnReadout
        '
        Me.btnReadout.AutoSize = True
        Me.btnReadout.Text = "Citește măsurile"
        Me.btnReadout.UseVisualStyleBackColor = True
        Me.btnReadout.Name = "btnReadout"
        '
        'tlyHost
        '
        ' Two columns: the probe table (Percent) and the tree (fixed 300 LOGICAL px). A table
        ' inside a table: the host is a KBotTableLayoutPanel too, so its fixed column must scale
        ' by the same factor as the probe's.
        Me.tlyHost.ColumnCount = 2
        Me.tlyHost.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        Me.tlyHost.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300.0F))
        Me.tlyHost.Controls.Add(Me.tlyProbe, 0, 0)
        Me.tlyHost.Controls.Add(Me.treeProbe, 1, 0)
        Me.tlyHost.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlyHost.Padding = New System.Windows.Forms.Padding(8)
        Me.tlyHost.RowCount = 1
        Me.tlyHost.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        Me.tlyHost.Name = "tlyHost"
        '
        'tlyProbe
        '
        ' Every number here is LOGICAL (96 dpi): 90/180/12/110 columns, 32/32/40/24 rows, padding
        ' 8. At 150% the operator must read 135/270/18/165, 48/48/60/36 and 12 in the journal.
        Me.tlyProbe.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single
        Me.tlyProbe.ColumnCount = 5
        Me.tlyProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90.0F))
        Me.tlyProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180.0F))
        Me.tlyProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12.0F))
        Me.tlyProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110.0F))
        Me.tlyProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        Me.tlyProbe.Controls.Add(Me.lblNume, 0, 0)
        Me.tlyProbe.Controls.Add(Me.txtNume, 1, 0)
        Me.tlyProbe.Controls.Add(Me.lblData, 3, 0)
        Me.tlyProbe.Controls.Add(Me.txtData, 4, 0)
        Me.tlyProbe.Controls.Add(Me.lblCod, 0, 1)
        Me.tlyProbe.Controls.Add(Me.txtCod, 1, 1)
        Me.tlyProbe.Controls.Add(Me.lblSuma, 3, 1)
        Me.tlyProbe.Controls.Add(Me.txtSuma, 4, 1)
        Me.tlyProbe.Controls.Add(Me.btnSalveaza, 1, 2)
        Me.tlyProbe.Controls.Add(Me.btnRenunta, 3, 2)
        Me.tlyProbe.Controls.Add(Me.lblBanda, 0, 3)
        Me.tlyProbe.Controls.Add(Me.lblInfo, 0, 4)
        Me.tlyProbe.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlyProbe.Margin = New System.Windows.Forms.Padding(0, 0, 8, 0)
        Me.tlyProbe.Padding = New System.Windows.Forms.Padding(8)
        Me.tlyProbe.RowCount = 5
        Me.tlyProbe.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32.0F))
        Me.tlyProbe.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32.0F))
        Me.tlyProbe.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0F))
        Me.tlyProbe.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24.0F))
        Me.tlyProbe.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        Me.tlyProbe.SetColumnSpan(Me.lblBanda, 5)
        Me.tlyProbe.SetColumnSpan(Me.lblInfo, 5)
        Me.tlyProbe.ThemedCellBorder = True
        Me.tlyProbe.Name = "tlyProbe"
        '
        'lblNume
        '
        Me.lblNume.AutoSize = True
        Me.lblNume.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblNume.Text = "Nume:"
        Me.lblNume.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblNume.Name = "lblNume"
        '
        'txtNume
        '
        Me.txtNume.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtNume.Margin = New System.Windows.Forms.Padding(0)
        Me.txtNume.PlaceholderText = "un nume"
        Me.txtNume.Name = "txtNume"
        '
        'lblData
        '
        Me.lblData.AutoSize = True
        Me.lblData.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblData.Text = "Data:"
        Me.lblData.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblData.Name = "lblData"
        '
        'txtData
        '
        Me.txtData.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtData.Margin = New System.Windows.Forms.Padding(0)
        Me.txtData.PlaceholderText = "zz.ll.aaaa"
        Me.txtData.Name = "txtData"
        '
        'lblCod
        '
        Me.lblCod.AutoSize = True
        Me.lblCod.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblCod.Text = "Cod:"
        Me.lblCod.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblCod.Name = "lblCod"
        '
        'txtCod
        '
        Me.txtCod.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtCod.Margin = New System.Windows.Forms.Padding(0)
        Me.txtCod.PlaceholderText = "un cod"
        Me.txtCod.Name = "txtCod"
        '
        'lblSuma
        '
        Me.lblSuma.AutoSize = True
        Me.lblSuma.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblSuma.Text = "Suma:"
        Me.lblSuma.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblSuma.Name = "lblSuma"
        '
        'txtSuma
        '
        Me.txtSuma.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtSuma.Margin = New System.Windows.Forms.Padding(0)
        Me.txtSuma.PlaceholderText = "0,00"
        Me.txtSuma.Name = "txtSuma"
        '
        'btnSalveaza
        '
        Me.btnSalveaza.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnSalveaza.Margin = New System.Windows.Forms.Padding(0)
        Me.btnSalveaza.Text = "Salvează"
        Me.btnSalveaza.UseVisualStyleBackColor = True
        Me.btnSalveaza.Name = "btnSalveaza"
        '
        'btnRenunta
        '
        Me.btnRenunta.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnRenunta.Margin = New System.Windows.Forms.Padding(0)
        Me.btnRenunta.Text = "Renunță"
        Me.btnRenunta.UseVisualStyleBackColor = True
        Me.btnRenunta.Name = "btnRenunta"
        '
        'lblBanda
        '
        Me.lblBanda.AutoSize = True
        Me.lblBanda.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblBanda.Text = "banda de 24px logic — se strânge la 0 prin SetRowCollapsed"
        Me.lblBanda.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblBanda.Name = "lblBanda"
        '
        'lblInfo
        '
        Me.lblInfo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblInfo.Text = "măsuri: —"
        Me.lblInfo.TextAlign = System.Drawing.ContentAlignment.TopLeft
        Me.lblInfo.Name = "lblInfo"
        '
        'treeProbe
        '
        ' ItemHeight = 32 LOGICAL, the same number as the first two rows of the table: at any
        ' scale the tree row and the table row must come out the same height.
        Me.treeProbe.Dock = System.Windows.Forms.DockStyle.Fill
        Me.treeProbe.HeaderVisible = True
        Me.treeProbe.HeaderCaption = "Arbore — ItemHeight 32 logic"
        Me.treeProbe.ItemHeight = 32
        Me.treeProbe.Margin = New System.Windows.Forms.Padding(0)
        Me.treeProbe.Name = "treeProbe"
        '
        'lstLog
        '
        Me.lstLog.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.lstLog.Height = 150
        Me.lstLog.HorizontalScrollbar = True
        Me.lstLog.IntegralHeight = False
        Me.lstLog.Name = "lstLog"
        '
        'pnlButtons
        '
        Me.pnlButtons.Controls.Add(Me.btnFail)
        Me.pnlButtons.Controls.Add(Me.btnPass)
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlButtons.Height = 44
        Me.pnlButtons.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlButtons.Name = "pnlButtons"
        '
        'btnFail
        '
        Me.btnFail.AutoSize = True
        Me.btnFail.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnFail.Text = "Fail"
        Me.btnFail.UseVisualStyleBackColor = True
        Me.btnFail.Name = "btnFail"
        '
        'btnPass
        '
        Me.btnPass.AutoSize = True
        Me.btnPass.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnPass.Text = "Pass"
        Me.btnPass.UseVisualStyleBackColor = True
        Me.btnPass.Name = "btnPass"
        '
        'TableLayoutHarnessForm
        '
        Me.CancelButton = Me.btnFail
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0F, 15.0F)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1180, 700)
        ' Reverse dock order (house rule): Fill first, then the Bottom bands, then the Top bands
        ' from the lowest to the highest -- the last Top added ends up on top.
        Me.Controls.Add(Me.tlyHost)
        Me.Controls.Add(Me.lstLog)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.pnlTableOptions)
        Me.Controls.Add(Me.pnlScale)
        Me.Controls.Add(Me.pnlTop)
        Me.MinimumSize = New System.Drawing.Size(900, 560)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "KBotTableLayoutPanel la DPI — 0066 (banc de probă)"
        Me.Name = "TableLayoutHarnessForm"
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        Me.pnlScale.ResumeLayout(False)
        Me.pnlScale.PerformLayout()
        CType(Me.numFactor, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trkText, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlTableOptions.ResumeLayout(False)
        Me.pnlTableOptions.PerformLayout()
        Me.tlyHost.ResumeLayout(False)
        Me.tlyProbe.ResumeLayout(False)
        Me.tlyProbe.PerformLayout()
        Me.pnlButtons.ResumeLayout(False)
        Me.pnlButtons.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

End Class
