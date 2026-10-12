<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VrcPartnersForm
    Inherits KBot.Theming.KBotThemedForm

    ' Every control is declared HERE so the form renders in the VS designer (docs/kbot-forms-ui-convention.md).
    ' Pixel values are logical, saved at 96 dpi. The checkbox columns «one per sector-source of the DC» depend on the data
    ' and are added in code (Load), after the fixed columns declared below.
    Private components As System.ComponentModel.IContainer

    Friend WithEvents tlpRoot As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents dgvFurnizori As KBot.Controls.KBotDataView
    Friend WithEvents tlpButoane As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnInchide As System.Windows.Forms.Button
    Friend WithEvents btnGoleste As System.Windows.Forms.Button
    Friend WithEvents btnAnaf As System.Windows.Forms.Button
    Friend WithEvents btnAdauga As System.Windows.Forms.Button
    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents tipParteneri As KBot.Controls.KBotToolTip

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(VrcPartnersForm))
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn7 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn8 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        tlpRoot = New KBot.Controls.KBotTableLayoutPanel()
        lblInfo = New Label()
        dgvFurnizori = New KBot.Controls.KBotDataView()
        tlpButoane = New KBot.Controls.KBotTableLayoutPanel()
        btnInchide = New Button()
        btnGoleste = New Button()
        btnAnaf = New Button()
        btnAdauga = New Button()
        lblStare = New Label()
        tipParteneri = New KBot.Controls.KBotToolTip(components)
        tlpRoot.SuspendLayout()
        CType(dgvFurnizori, ComponentModel.ISupportInitialize).BeginInit()
        tlpButoane.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlpRoot
        ' 
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpRoot.Controls.Add(lblInfo, 0, 0)
        tlpRoot.Controls.Add(dgvFurnizori, 0, 1)
        tlpRoot.Controls.Add(tlpButoane, 0, 2)
        tlpRoot.Controls.Add(lblStare, 0, 3)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Location = New Point(0, 0)
        tlpRoot.Margin = New Padding(4)
        tlpRoot.Name = "tlpRoot"
        tlpRoot.Padding = New Padding(8)
        tlpRoot.RowCount = 4
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 180F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 69F))
        tlpRoot.Size = New Size(1393, 780)
        tlpRoot.TabIndex = 0
        ' 
        ' lblInfo
        ' 
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Location = New Point(12, 8)
        lblInfo.Margin = New Padding(4, 0, 4, 0)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(1369, 180)
        lblInfo.TabIndex = 0
        lblInfo.Text = resources.GetString("lblInfo.Text")
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' dgvFurnizori
        ' 
        dgvFurnizori.BackColor = SystemColors.Window
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Cod fiscal"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn5.Key = "cf"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 90
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Denumire (ANAF)"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn6.Key = "denumire"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 200
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Adresă (ANAF)"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn7.Key = "adresa"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Width = 300
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Stare"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn8.Key = "stare"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        dgvFurnizori.Columns.Add(KBotDataColumn5)
        dgvFurnizori.Columns.Add(KBotDataColumn6)
        dgvFurnizori.Columns.Add(KBotDataColumn7)
        dgvFurnizori.Columns.Add(KBotDataColumn8)
        dgvFurnizori.Dock = DockStyle.Fill
        dgvFurnizori.HeaderHeight = 26
        dgvFurnizori.Location = New Point(12, 192)
        dgvFurnizori.Margin = New Padding(4)
        dgvFurnizori.Name = "dgvFurnizori"
        dgvFurnizori.RowHeight = 24
        dgvFurnizori.Size = New Size(1369, 447)
        dgvFurnizori.TabIndex = 1
        ' 
        ' tlpButoane
        ' 
        tlpButoane.ColumnCount = 5
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 151F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 171F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlpButoane.Controls.Add(btnInchide, 0, 0)
        tlpButoane.Controls.Add(btnGoleste, 2, 0)
        tlpButoane.Controls.Add(btnAnaf, 3, 0)
        tlpButoane.Controls.Add(btnAdauga, 4, 0)
        tlpButoane.Dock = DockStyle.Fill
        tlpButoane.Location = New Point(12, 647)
        tlpButoane.Margin = New Padding(4)
        tlpButoane.Name = "tlpButoane"
        tlpButoane.RowCount = 1
        tlpButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpButoane.Size = New Size(1369, 52)
        tlpButoane.TabIndex = 2
        ' 
        ' btnInchide
        ' 
        btnInchide.Dock = DockStyle.Fill
        btnInchide.Location = New Point(4, 4)
        btnInchide.Margin = New Padding(4)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New Size(112, 44)
        btnInchide.TabIndex = 0
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        ' 
        ' btnGoleste
        ' 
        btnGoleste.Dock = DockStyle.Fill
        btnGoleste.Location = New Point(881, 4)
        btnGoleste.Margin = New Padding(4)
        btnGoleste.Name = "btnGoleste"
        btnGoleste.Size = New Size(143, 44)
        btnGoleste.TabIndex = 3
        btnGoleste.Text = "Anulează"
        tipParteneri.SetToolTipHeader(btnGoleste, "Anulează pregătirea")
        tipParteneri.SetToolTipText(btnGoleste, "Șterge lista de parteneri pregătiți pentru transfer. Nu s-a scris nimic în bază.")
        btnGoleste.UseVisualStyleBackColor = True
        ' 
        ' btnAnaf
        ' 
        btnAnaf.Dock = DockStyle.Fill
        btnAnaf.Location = New Point(1032, 4)
        btnAnaf.Margin = New Padding(4)
        btnAnaf.Name = "btnAnaf"
        btnAnaf.Size = New Size(163, 44)
        btnAnaf.TabIndex = 1
        btnAnaf.Text = "ANAF"
        tipParteneri.SetToolTipHeader(btnAnaf, "Interogare ANAF")
        tipParteneri.SetToolTipText(btnAnaf, "Cere de la ANAF denumirea și adresa pentru toate codurile fiscale din listă (nu scrie nimic în baza de date).")
        btnAnaf.UseVisualStyleBackColor = True
        ' 
        ' btnAdauga
        ' 
        btnAdauga.Dock = DockStyle.Fill
        btnAdauga.Location = New Point(1203, 4)
        btnAdauga.Margin = New Padding(4)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Size = New Size(162, 44)
        btnAdauga.TabIndex = 2
        btnAdauga.Text = "Pregătește"
        tipParteneri.SetToolTipHeader(btnAdauga, "Pregătește pentru transfer")
        tipParteneri.SetToolTipText(btnAdauga, resources.GetString("btnAdauga.ToolTipText"))
        btnAdauga.UseVisualStyleBackColor = True
        ' 
        ' lblStare
        ' 
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(12, 703)
        lblStare.Margin = New Padding(4, 0, 4, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(1369, 69)
        lblStare.TabIndex = 3
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' VrcPartnersForm
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CenterOnScreen = False
        ClientSize = New Size(1393, 780)
        Controls.Add(tlpRoot)
        Margin = New Padding(4)
        MinimumSize = New Size(619, 542)
        Name = "VrcPartnersForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Furnizori fără partener — din ANAF"
        tlpRoot.ResumeLayout(False)
        CType(dgvFurnizori, ComponentModel.ISupportInitialize).EndInit()
        tlpButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
