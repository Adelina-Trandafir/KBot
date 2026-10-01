Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class HelpCaptureForm
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
        tips = New KBotToolTip(components)
        pnlRoot = New Panel()
        grid = New KBotDataView()
        tlyBara = New KBotTableLayoutPanel()
        lblSumar = New Label()
        chkDoarLipsa = New CheckBox()
        btnReincarca = New Button()
        btnDosar = New Button()
        lblDosar = New Label()
        capBar = New KBotCaptionBar()
        tlyDetaliu = New KBotTableLayoutPanel()
        lblImagineTitlu = New Label()
        lblPregatireTitlu = New Label()
        lblPreviewTitlu = New Label()
        txtImagine = New TextBox()
        txtPregatire = New TextBox()
        picPreview = New PictureBox()
        pnlRoot.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        tlyBara.SuspendLayout()
        tlyDetaliu.SuspendLayout()
        CType(picPreview, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pnlRoot
        '
        pnlRoot.Controls.Add(grid)
        pnlRoot.Controls.Add(tlyDetaliu)
        pnlRoot.Controls.Add(lblDosar)
        pnlRoot.Controls.Add(tlyBara)
        pnlRoot.Controls.Add(capBar)
        pnlRoot.Dock = DockStyle.Fill
        pnlRoot.Location = New Point(1, 1)
        pnlRoot.Margin = New Padding(0)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Padding = New Padding(10, 0, 10, 0)
        pnlRoot.Size = New Size(1098, 618)
        pnlRoot.TabIndex = 0
        pnlRoot.Tag = "Card"
        '
        ' grid
        '
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Parte"
        KBotDataColumn1.Key = "parte"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 90
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Pagina de ajutor"
        KBotDataColumn2.Key = "pagina"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 190
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Imagine"
        KBotDataColumn3.Key = "imagine"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 260
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Ce pregătești"
        KBotDataColumn4.Key = "pregatire"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 300
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Stare"
        KBotDataColumn5.Key = "stare"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 120
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.ColumnType = KBotColumnType.Button
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = ""
        KBotDataColumn6.Key = "poza"
        KBotDataColumn6.MinWidth = 90
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.Resizable = False
        KBotDataColumn6.Width = 90
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.ColumnType = KBotColumnType.Button
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = ""
        KBotDataColumn7.Key = "incarca"
        KBotDataColumn7.MinWidth = 90
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.Resizable = False
        KBotDataColumn7.Width = 90
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.ColumnType = KBotColumnType.Button
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = ""
        KBotDataColumn8.Key = "vezi"
        KBotDataColumn8.MinWidth = 90
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.Resizable = False
        KBotDataColumn8.Width = 90
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Columns.Add(KBotDataColumn6)
        grid.Columns.Add(KBotDataColumn8)
        grid.Columns.Add(KBotDataColumn7)
        grid.Dock = DockStyle.Fill
        grid.FillColumnKey = "imagine"
        grid.Location = New Point(10, 78)
        grid.Margin = New Padding(0)
        grid.Name = "grid"
        grid.ReadOnlyGrid = False
        grid.Size = New Size(1078, 512)
        grid.TabIndex = 2
        '
        ' tlyBara
        '
        tlyBara.ColumnCount = 4
        tlyBara.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBara.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyBara.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyBara.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBara.Controls.Add(lblSumar, 0, 0)
        tlyBara.Controls.Add(chkDoarLipsa, 1, 0)
        tlyBara.Controls.Add(btnReincarca, 2, 0)
        tlyBara.Controls.Add(btnDosar, 3, 0)
        tlyBara.Dock = DockStyle.Top
        tlyBara.Location = New Point(10, 34)
        tlyBara.Margin = New Padding(0)
        tlyBara.Name = "tlyBara"
        tlyBara.Padding = New Padding(0, 6, 0, 6)
        tlyBara.RowCount = 1
        tlyBara.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBara.Size = New Size(1078, 44)
        tlyBara.TabIndex = 1
        '
        ' lblSumar
        '
        lblSumar.AutoEllipsis = True
        lblSumar.Dock = DockStyle.Fill
        lblSumar.Font = New Font("Segoe UI Semibold", 10F)
        lblSumar.Margin = New Padding(0)
        lblSumar.Name = "lblSumar"
        lblSumar.TabIndex = 0
        lblSumar.TextAlign = ContentAlignment.MiddleLeft
        '
        ' chkDoarLipsa
        '
        chkDoarLipsa.Checked = True
        chkDoarLipsa.CheckState = CheckState.Checked
        chkDoarLipsa.Dock = DockStyle.Fill
        chkDoarLipsa.Margin = New Padding(0)
        chkDoarLipsa.Name = "chkDoarLipsa"
        chkDoarLipsa.TabIndex = 1
        chkDoarLipsa.Text = "Doar cele care lipsesc"
        tips.SetToolTipHeader(chkDoarLipsa, "Doar cele care lipsesc")
        tips.SetToolTipText(chkDoarLipsa, "Debifat: apar și imaginile deja făcute, cu butonul «Refă».")
        chkDoarLipsa.UseVisualStyleBackColor = True
        '
        ' btnReincarca
        '
        btnReincarca.Dock = DockStyle.Fill
        btnReincarca.FlatStyle = FlatStyle.Flat
        btnReincarca.Margin = New Padding(0, 0, 6, 0)
        btnReincarca.Name = "btnReincarca"
        btnReincarca.TabIndex = 2
        btnReincarca.Text = "Reîncarcă"
        tips.SetToolTipHeader(btnReincarca, "Reîncarcă")
        tips.SetToolTipText(btnReincarca, "Citește din nou paginile de ajutor (după ce s-au adăugat etichete noi).")
        btnReincarca.UseVisualStyleBackColor = True
        '
        ' btnDosar
        '
        btnDosar.Dock = DockStyle.Fill
        btnDosar.FlatStyle = FlatStyle.Flat
        btnDosar.Margin = New Padding(0)
        btnDosar.Name = "btnDosar"
        btnDosar.TabIndex = 3
        btnDosar.Text = "Deschide dosarul"
        tips.SetToolTipHeader(btnDosar, "Dosarul imaginilor")
        tips.SetToolTipText(btnDosar, "Deschide dosarul în care se salvează pozele.")
        btnDosar.UseVisualStyleBackColor = True
        '
        ' tlyDetaliu
        '
        tlyDetaliu.ColumnCount = 3
        tlyDetaliu.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30F))
        tlyDetaliu.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30F))
        tlyDetaliu.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 40F))
        tlyDetaliu.Controls.Add(lblImagineTitlu, 0, 0)
        tlyDetaliu.Controls.Add(lblPregatireTitlu, 1, 0)
        tlyDetaliu.Controls.Add(lblPreviewTitlu, 2, 0)
        tlyDetaliu.Controls.Add(txtImagine, 0, 1)
        tlyDetaliu.Controls.Add(txtPregatire, 1, 1)
        tlyDetaliu.Controls.Add(picPreview, 2, 1)
        tlyDetaliu.Dock = DockStyle.Bottom
        tlyDetaliu.Location = New Point(10, 440)
        tlyDetaliu.Margin = New Padding(0)
        tlyDetaliu.Name = "tlyDetaliu"
        tlyDetaliu.RowCount = 2
        tlyDetaliu.RowStyles.Add(New RowStyle(SizeType.Absolute, 22F))
        tlyDetaliu.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyDetaliu.Size = New Size(1078, 150)
        tlyDetaliu.TabIndex = 4
        '
        ' lblImagineTitlu
        '
        lblImagineTitlu.Dock = DockStyle.Fill
        lblImagineTitlu.Margin = New Padding(0)
        lblImagineTitlu.Name = "lblImagineTitlu"
        lblImagineTitlu.TabIndex = 0
        lblImagineTitlu.Text = "Imagine"
        lblImagineTitlu.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblPregatireTitlu
        '
        lblPregatireTitlu.Dock = DockStyle.Fill
        lblPregatireTitlu.Margin = New Padding(0)
        lblPregatireTitlu.Name = "lblPregatireTitlu"
        lblPregatireTitlu.TabIndex = 1
        lblPregatireTitlu.Text = "Ce pregătești"
        lblPregatireTitlu.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblPreviewTitlu
        '
        lblPreviewTitlu.Dock = DockStyle.Fill
        lblPreviewTitlu.Margin = New Padding(0)
        lblPreviewTitlu.Name = "lblPreviewTitlu"
        lblPreviewTitlu.TabIndex = 2
        lblPreviewTitlu.Text = "Imaginea făcută"
        lblPreviewTitlu.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtImagine
        '
        txtImagine.Dock = DockStyle.Fill
        txtImagine.Margin = New Padding(0, 0, 6, 0)
        txtImagine.Multiline = True
        txtImagine.Name = "txtImagine"
        txtImagine.ReadOnly = True
        txtImagine.ScrollBars = ScrollBars.Vertical
        txtImagine.TabIndex = 3
        '
        ' txtPregatire
        '
        txtPregatire.Dock = DockStyle.Fill
        txtPregatire.Margin = New Padding(0, 0, 6, 0)
        txtPregatire.Multiline = True
        txtPregatire.Name = "txtPregatire"
        txtPregatire.ReadOnly = True
        txtPregatire.ScrollBars = ScrollBars.Vertical
        txtPregatire.TabIndex = 4
        '
        ' picPreview
        '
        picPreview.BorderStyle = BorderStyle.FixedSingle
        picPreview.Dock = DockStyle.Fill
        picPreview.Margin = New Padding(0)
        picPreview.Name = "picPreview"
        picPreview.SizeMode = PictureBoxSizeMode.Zoom
        picPreview.TabIndex = 5
        picPreview.TabStop = False
        '
        ' lblDosar
        '
        lblDosar.AutoEllipsis = True
        lblDosar.Dock = DockStyle.Bottom
        lblDosar.Location = New Point(10, 590)
        lblDosar.Margin = New Padding(0)
        lblDosar.Name = "lblDosar"
        lblDosar.Size = New Size(1078, 28)
        lblDosar.TabIndex = 3
        lblDosar.TextAlign = ContentAlignment.MiddleLeft
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(10, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowHelpButton = False
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.Size = New Size(1078, 34)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Capturi pentru ajutor"
        '
        ' HelpCaptureForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1100, 700)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        MinimumSize = New Size(700, 360)
        Name = "HelpCaptureForm"
        Padding = New Padding(1)
        StartPosition = FormStartPosition.CenterScreen
        Text = "Capturi pentru ajutor"
        pnlRoot.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyBara.ResumeLayout(False)
        tlyDetaliu.ResumeLayout(False)
        tlyDetaliu.PerformLayout()
        CType(picPreview, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlRoot As Panel
    Friend WithEvents grid As KBotDataView
    Friend WithEvents tlyBara As KBotTableLayoutPanel
    Friend WithEvents lblSumar As Label
    Friend WithEvents chkDoarLipsa As CheckBox
    Friend WithEvents btnReincarca As Button
    Friend WithEvents btnDosar As Button
    Friend WithEvents lblDosar As Label
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents tlyDetaliu As KBotTableLayoutPanel
    Friend WithEvents lblImagineTitlu As Label
    Friend WithEvents lblPregatireTitlu As Label
    Friend WithEvents lblPreviewTitlu As Label
    Friend WithEvents txtImagine As TextBox
    Friend WithEvents txtPregatire As TextBox
    Friend WithEvents picPreview As PictureBox
End Class
