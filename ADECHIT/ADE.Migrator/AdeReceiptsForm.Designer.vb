<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdeReceiptsForm
    Inherits KBot.Theming.KBotThemedForm

    ' Every control is declared HERE so the form renders in the VS designer (docs/kbot-forms-ui-convention.md).
    ' Pixel values are logical, saved at 96 dpi.
    Private components As System.ComponentModel.IContainer

    Friend WithEvents tlpRoot As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblIntro As System.Windows.Forms.Label
    Friend WithEvents dgvChitante As KBot.Controls.KBotDataView
    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents tlpButoane As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnRegula As System.Windows.Forms.Button
    Friend WithEvents btnReset As System.Windows.Forms.Button
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
    Friend WithEvents btnGata As System.Windows.Forms.Button
    Friend WithEvents tipChitante As KBot.Controls.KBotToolTip

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        Dim colIdc As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colIdpl As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colData As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colSerie As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colNumar As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colAnulata As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colStare As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        tlpRoot = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblIntro = New System.Windows.Forms.Label()
        dgvChitante = New KBot.Controls.KBotDataView()
        lblStare = New System.Windows.Forms.Label()
        tlpButoane = New Global.KBot.Controls.KBotTableLayoutPanel()
        btnRegula = New System.Windows.Forms.Button()
        btnReset = New System.Windows.Forms.Button()
        btnRenunta = New System.Windows.Forms.Button()
        btnGata = New System.Windows.Forms.Button()
        tipChitante = New KBot.Controls.KBotToolTip(components)
        CType(dgvChitante, System.ComponentModel.ISupportInitialize).BeginInit()
        tlpRoot.SuspendLayout()
        tlpButoane.SuspendLayout()
        SuspendLayout()
        '
        ' tlpRoot
        '
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpRoot.Controls.Add(lblIntro, 0, 0)
        tlpRoot.Controls.Add(dgvChitante, 0, 1)
        tlpRoot.Controls.Add(lblStare, 0, 2)
        tlpRoot.Controls.Add(tlpButoane, 0, 3)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Location = New Point(0, 0)
        tlpRoot.Name = "tlpRoot"
        tlpRoot.Padding = New Padding(8)
        tlpRoot.RowCount = 4
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 64.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 44.0F))
        tlpRoot.Size = New Size(960, 600)
        tlpRoot.TabIndex = 0
        '
        ' lblIntro
        '
        lblIntro.Dock = DockStyle.Fill
        lblIntro.Name = "lblIntro"
        lblIntro.TabIndex = 0
        lblIntro.Text = "Chitanțele de mai jos au aceeași serie și același număr. Baza de date primește fiecare serie și număr o singură dată, deci schimbați numărul la cele în plus: dați dublu clic pe coloana «Număr nou». După fiecare schimbare lista se verifică din nou."
        lblIntro.TextAlign = ContentAlignment.MiddleLeft
        '
        ' dgvChitante
        '
        dgvChitante.BackColor = SystemColors.Window
        colIdc.HeaderText = "IDC"
        colIdc.Key = "idc"
        colIdc.ReadOnly = True
        colIdc.TextAlign = ContentAlignment.MiddleRight
        colIdc.Width = 70
        colIdpl.HeaderText = "IDPL"
        colIdpl.Key = "idpl"
        colIdpl.ReadOnly = True
        colIdpl.TextAlign = ContentAlignment.MiddleRight
        colIdpl.Width = 70
        colData.HeaderText = "Data"
        colData.Key = "data"
        colData.ReadOnly = True
        colData.Width = 100
        colSerie.HeaderText = "Serie"
        colSerie.Key = "serie"
        colSerie.ReadOnly = True
        colSerie.Width = 90
        colNumar.HeaderText = "Număr nou"
        colNumar.Key = "numar"
        colNumar.TextAlign = ContentAlignment.MiddleRight
        colNumar.Width = 110
        colAnulata.HeaderText = "Anulată"
        colAnulata.Key = "anulata"
        colAnulata.ReadOnly = True
        colAnulata.Width = 70
        colStare.HeaderText = "Stare după schimbări"
        colStare.Key = "stare"
        colStare.ReadOnly = True
        colStare.Width = 380
        dgvChitante.Columns.Add(colIdc)
        dgvChitante.Columns.Add(colIdpl)
        dgvChitante.Columns.Add(colData)
        dgvChitante.Columns.Add(colSerie)
        dgvChitante.Columns.Add(colNumar)
        dgvChitante.Columns.Add(colAnulata)
        dgvChitante.Columns.Add(colStare)
        dgvChitante.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        dgvChitante.Dock = DockStyle.Fill
        dgvChitante.FillColumnKey = "stare"
        dgvChitante.HeaderHeight = 26
        dgvChitante.Name = "dgvChitante"
        dgvChitante.RowHeight = 24
        dgvChitante.TabIndex = 1
        '
        ' lblStare
        '
        lblStare.Dock = DockStyle.Fill
        lblStare.Name = "lblStare"
        lblStare.TabIndex = 2
        lblStare.Text = ""
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlpButoane
        '
        tlpButoane.ColumnCount = 5
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 330.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110.0F))
        tlpButoane.Controls.Add(btnRegula, 0, 0)
        tlpButoane.Controls.Add(btnReset, 1, 0)
        tlpButoane.Controls.Add(btnRenunta, 3, 0)
        tlpButoane.Controls.Add(btnGata, 4, 0)
        tlpButoane.Dock = DockStyle.Fill
        tlpButoane.Name = "tlpButoane"
        tlpButoane.RowCount = 1
        tlpButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpButoane.TabIndex = 3
        '
        ' btnRegula
        '
        btnRegula.Dock = DockStyle.Fill
        btnRegula.Name = "btnRegula"
        btnRegula.TabIndex = 0
        btnRegula.Text = "Taie cifra miilor la cele mai vechi"
        btnRegula.UseVisualStyleBackColor = True
        '
        ' btnReset
        '
        btnReset.Dock = DockStyle.Fill
        btnReset.Name = "btnReset"
        btnReset.TabIndex = 1
        btnReset.Text = "Anulează schimbările"
        btnReset.UseVisualStyleBackColor = True
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Name = "btnRenunta"
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnGata
        '
        btnGata.DialogResult = DialogResult.OK
        btnGata.Dock = DockStyle.Fill
        btnGata.Name = "btnGata"
        btnGata.TabIndex = 3
        btnGata.Text = "Gata"
        btnGata.UseVisualStyleBackColor = True
        '
        ' tipChitante
        '
        tipChitante.SetToolTipHeader(btnRegula, "Regula pentru dubluri")
        tipChitante.SetToolTipText(btnRegula, "În fiecare grup de chitanțe cu același număr, toate în afară de cea mai nouă pierd cifra miilor (1234 devine 234), dacă numărul nou este liber în serie. Cele care nu se pot schimba așa rămân neschimbate.")
        tipChitante.SetToolTipHeader(btnReset, "Anulează schimbările")
        tipChitante.SetToolTipText(btnReset, "Aduce înapoi numerele din Access pentru toate chitanțele din listă.")
        tipChitante.SetToolTipHeader(btnGata, "Gata")
        tipChitante.SetToolTipText(btnGata, "Păstrează numerele schimbate și reface planul de migrare. Access-ul nu este modificat.")
        '
        ' AdeReceiptsForm
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(960, 600)
        Controls.Add(tlpRoot)
        MinimumSize = New Size(860, 480)
        Name = "AdeReceiptsForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Chitanțe dublate — schimbați numerele"
        CType(dgvChitante, System.ComponentModel.ISupportInitialize).EndInit()
        tlpRoot.ResumeLayout(False)
        tlpButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
