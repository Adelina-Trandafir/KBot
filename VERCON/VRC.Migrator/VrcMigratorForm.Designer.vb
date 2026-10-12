<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VrcMigratorForm
    Inherits KBot.Theming.KBotThemedForm

    ' Every control is declared HERE so the form renders in the VS designer (docs/kbot-forms-ui-convention.md).
    ' Pixel values are logical, saved at 96 dpi.
    Private components As System.ComponentModel.IContainer

    Friend WithEvents tlpRoot As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents grpSurse As System.Windows.Forms.GroupBox
    Friend WithEvents tlpSurse As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lstFisiere As System.Windows.Forms.ListBox
    Friend WithEvents btnAdauga As System.Windows.Forms.Button
    Friend WithEvents btnElimina As System.Windows.Forms.Button
    Friend WithEvents btnCiteste As System.Windows.Forms.Button
    Friend WithEvents btnSalveaza As System.Windows.Forms.Button
    Friend WithEvents grpRaport As System.Windows.Forms.GroupBox
    Friend WithEvents rtbRaport As System.Windows.Forms.RichTextBox
    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents ofdBaze As System.Windows.Forms.OpenFileDialog
    Friend WithEvents sfdRaport As System.Windows.Forms.SaveFileDialog
    Friend WithEvents tipVrc As KBot.Controls.KBotToolTip

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        tlpRoot = New Global.KBot.Controls.KBotTableLayoutPanel()
        grpSurse = New System.Windows.Forms.GroupBox()
        tlpSurse = New Global.KBot.Controls.KBotTableLayoutPanel()
        lstFisiere = New System.Windows.Forms.ListBox()
        btnAdauga = New System.Windows.Forms.Button()
        btnElimina = New System.Windows.Forms.Button()
        btnCiteste = New System.Windows.Forms.Button()
        btnSalveaza = New System.Windows.Forms.Button()
        grpRaport = New System.Windows.Forms.GroupBox()
        rtbRaport = New System.Windows.Forms.RichTextBox()
        lblStare = New System.Windows.Forms.Label()
        ofdBaze = New System.Windows.Forms.OpenFileDialog()
        sfdRaport = New System.Windows.Forms.SaveFileDialog()
        tipVrc = New KBot.Controls.KBotToolTip(components)
        tlpRoot.SuspendLayout()
        grpSurse.SuspendLayout()
        tlpSurse.SuspendLayout()
        grpRaport.SuspendLayout()
        SuspendLayout()
        '
        ' tlpRoot
        '
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpRoot.Controls.Add(grpSurse, 0, 0)
        tlpRoot.Controls.Add(grpRaport, 0, 1)
        tlpRoot.Controls.Add(lblStare, 0, 2)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Location = New Point(0, 0)
        tlpRoot.Name = "tlpRoot"
        tlpRoot.Padding = New Padding(8)
        tlpRoot.RowCount = 3
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 170.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 26.0F))
        tlpRoot.Size = New Size(1100, 700)
        tlpRoot.TabIndex = 0
        '
        ' grpSurse
        '
        grpSurse.Controls.Add(tlpSurse)
        grpSurse.Dock = DockStyle.Fill
        grpSurse.Name = "grpSurse"
        grpSurse.TabIndex = 0
        grpSurse.TabStop = False
        grpSurse.Text = "Bazele Access ale subunității (toate: MF, OI și toate tz_aaaa)"
        '
        ' tlpSurse
        '
        tlpSurse.ColumnCount = 2
        tlpSurse.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpSurse.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150.0F))
        tlpSurse.Controls.Add(lstFisiere, 0, 0)
        tlpSurse.SetRowSpan(lstFisiere, 4)
        tlpSurse.Controls.Add(btnAdauga, 1, 0)
        tlpSurse.Controls.Add(btnElimina, 1, 1)
        tlpSurse.Controls.Add(btnCiteste, 1, 2)
        tlpSurse.Controls.Add(btnSalveaza, 1, 3)
        tlpSurse.Dock = DockStyle.Fill
        tlpSurse.Name = "tlpSurse"
        tlpSurse.RowCount = 4
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Percent, 25.0F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Percent, 25.0F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Percent, 25.0F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Percent, 25.0F))
        tlpSurse.TabIndex = 0
        '
        ' lstFisiere
        '
        lstFisiere.Dock = DockStyle.Fill
        lstFisiere.HorizontalScrollbar = True
        lstFisiere.Name = "lstFisiere"
        lstFisiere.SelectionMode = SelectionMode.MultiExtended
        lstFisiere.TabIndex = 0
        '
        ' btnAdauga
        '
        btnAdauga.Dock = DockStyle.Fill
        btnAdauga.Name = "btnAdauga"
        btnAdauga.TabIndex = 1
        btnAdauga.Text = "Adaugă baze…"
        btnAdauga.UseVisualStyleBackColor = True
        '
        ' btnElimina
        '
        btnElimina.Dock = DockStyle.Fill
        btnElimina.Name = "btnElimina"
        btnElimina.TabIndex = 2
        btnElimina.Text = "Elimină"
        btnElimina.UseVisualStyleBackColor = True
        '
        ' btnCiteste
        '
        btnCiteste.Dock = DockStyle.Fill
        btnCiteste.Name = "btnCiteste"
        btnCiteste.TabIndex = 3
        btnCiteste.Text = "Citește"
        btnCiteste.UseVisualStyleBackColor = True
        '
        ' btnSalveaza
        '
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.TabIndex = 4
        btnSalveaza.Text = "Salvează raportul…"
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' grpRaport
        '
        grpRaport.Controls.Add(rtbRaport)
        grpRaport.Dock = DockStyle.Fill
        grpRaport.Name = "grpRaport"
        grpRaport.TabIndex = 1
        grpRaport.TabStop = False
        grpRaport.Text = "Raport de profilare"
        '
        ' rtbRaport
        '
        rtbRaport.Dock = DockStyle.Fill
        rtbRaport.Font = New Font("Consolas", 9.0F)
        rtbRaport.Name = "rtbRaport"
        rtbRaport.ReadOnly = True
        rtbRaport.TabIndex = 0
        rtbRaport.Text = ""
        rtbRaport.WordWrap = False
        '
        ' lblStare
        '
        lblStare.Dock = DockStyle.Fill
        lblStare.Name = "lblStare"
        lblStare.TabIndex = 2
        lblStare.Text = ""
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' ofdBaze
        '
        ofdBaze.Filter = "Baze Access (*.accdb;*.mdb)|*.accdb;*.mdb|Toate fișierele|*.*"
        ofdBaze.Multiselect = True
        ofdBaze.Title = "Alegeți bazele Access ale subunității"
        '
        ' sfdRaport
        '
        sfdRaport.DefaultExt = "md"
        sfdRaport.Filter = "Markdown (*.md)|*.md"
        sfdRaport.Title = "Salvați raportul de profilare"
        '
        ' tipVrc
        '
        tipVrc.SetToolTipHeader(btnAdauga, "Adaugă baze")
        tipVrc.SetToolTipText(btnAdauga, "Alegeți TOATE bazele subunității: baza MF, baza OI și toate tz_aaaa de la Materiale/Alimente. Nu se începe cu o gestiune urmând ca celelalte să fie adăugate după.")
        tipVrc.SetToolTipHeader(btnCiteste, "Citește")
        tipVrc.SetToolTipText(btnCiteste, "Citește bazele în mod doar-citire și face raportul. Nu scrie nimic în Access și nu atinge serverul.")
        tipVrc.SetToolTipHeader(btnSalveaza, "Salvează raportul")
        tipVrc.SetToolTipText(btnSalveaza, "Salvează raportul ca fișier Markdown. Conține numere și intervale, nu rânduri de date.")
        '
        ' VrcMigratorForm
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1100, 700)
        Controls.Add(tlpRoot)
        MinimumSize = New Size(900, 560)
        Name = "VrcMigratorForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "VRC.Migrator — Access VERCON ▸ MariaDB (profilare)"
        tlpRoot.ResumeLayout(False)
        grpSurse.ResumeLayout(False)
        tlpSurse.ResumeLayout(False)
        grpRaport.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
