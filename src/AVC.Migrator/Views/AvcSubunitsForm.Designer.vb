<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AvcSubunitsForm
    Inherits KBot.Theming.KBotThemedForm

    ' Every control is declared HERE so the form renders in the VS designer (docs/kbot-forms-ui-convention.md).
    ' Pixel values are logical, saved at 96 dpi.
    Private components As System.ComponentModel.IContainer

    Friend WithEvents tlpRoot As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblIntro As System.Windows.Forms.Label
    Friend WithEvents dgvSubunitati As KBot.Controls.KBotDataView
    Friend WithEvents tlpButoane As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents btnInchide As System.Windows.Forms.Button
    Friend WithEvents btnPorteaza As System.Windows.Forms.Button
    Friend WithEvents tipSub As KBot.Controls.KBotToolTip

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        Dim colBifa As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colId As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colDenumire As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colCapitol As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colSursa As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colZec As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colParinte As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colStare As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        tlpRoot = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblIntro = New System.Windows.Forms.Label()
        dgvSubunitati = New KBot.Controls.KBotDataView()
        tlpButoane = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblStare = New System.Windows.Forms.Label()
        btnInchide = New System.Windows.Forms.Button()
        btnPorteaza = New System.Windows.Forms.Button()
        tipSub = New KBot.Controls.KBotToolTip(components)
        CType(dgvSubunitati, System.ComponentModel.ISupportInitialize).BeginInit()
        tlpRoot.SuspendLayout()
        tlpButoane.SuspendLayout()
        SuspendLayout()
        '
        ' tlpRoot
        '
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpRoot.Controls.Add(lblIntro, 0, 0)
        tlpRoot.Controls.Add(dgvSubunitati, 0, 1)
        tlpRoot.Controls.Add(tlpButoane, 0, 2)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Name = "tlpRoot"
        tlpRoot.Padding = New Padding(8)
        tlpRoot.RowCount = 3
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 72.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 44.0F))
        tlpRoot.TabIndex = 0
        '
        ' lblIntro
        '
        lblIntro.Dock = DockStyle.Fill
        lblIntro.Name = "lblIntro"
        lblIntro.TabIndex = 0
        lblIntro.Text = "Fiecare unitate din registru devine o subunitate, iar planul ei de conturi intră în ea. Denumirea vine din «AlteDetalii», capitolul din sursă (01A = 65.01, 02A = 65.02, 02E = 65.10); le puteți corecta în grilă. O unitate cu părinte citește clasificațiile și bugetul părintelui: portați-l întâi sau odată cu ea."
        lblIntro.TextAlign = ContentAlignment.MiddleLeft
        '
        ' dgvSubunitati
        '
        dgvSubunitati.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        dgvSubunitati.BackColor = SystemColors.Window
        colBifa.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        colBifa.HeaderText = "Portează"
        colBifa.Key = "bifa"
        colBifa.Width = 72
        colId.HeaderText = "IdUnitate"
        colId.Key = "id"
        colId.ReadOnly = True
        colId.Width = 76
        colDenumire.HeaderText = "Denumire (AlteDetalii)"
        colDenumire.Key = "denumire"
        colDenumire.Width = 300
        colCapitol.HeaderText = "Capitol"
        colCapitol.Key = "capitol"
        colCapitol.Width = 76
        colSursa.HeaderText = "Sursă"
        colSursa.Key = "sursa"
        colSursa.ReadOnly = True
        colSursa.Width = 64
        colZec.HeaderText = "Zecimale salarii"
        colZec.Key = "zec"
        colZec.Width = 110
        colParinte.HeaderText = "Părinte"
        colParinte.Key = "parinte"
        colParinte.ReadOnly = True
        colParinte.Width = 72
        colStare.HeaderText = "Stare"
        colStare.Key = "stare"
        colStare.ReadOnly = True
        colStare.Width = 150
        dgvSubunitati.Columns.Add(colBifa)
        dgvSubunitati.Columns.Add(colId)
        dgvSubunitati.Columns.Add(colDenumire)
        dgvSubunitati.Columns.Add(colCapitol)
        dgvSubunitati.Columns.Add(colSursa)
        dgvSubunitati.Columns.Add(colZec)
        dgvSubunitati.Columns.Add(colParinte)
        dgvSubunitati.Columns.Add(colStare)
        dgvSubunitati.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        dgvSubunitati.FillColumnKey = "denumire"
        dgvSubunitati.Dock = DockStyle.Fill
        dgvSubunitati.HeaderHeight = 26
        dgvSubunitati.Name = "dgvSubunitati"
        dgvSubunitati.RowHeight = 24
        dgvSubunitati.TabIndex = 1
        '
        ' tlpButoane
        '
        tlpButoane.ColumnCount = 3
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170.0F))
        tlpButoane.Controls.Add(lblStare, 0, 0)
        tlpButoane.Controls.Add(btnInchide, 1, 0)
        tlpButoane.Controls.Add(btnPorteaza, 2, 0)
        tlpButoane.Dock = DockStyle.Fill
        tlpButoane.Name = "tlpButoane"
        tlpButoane.RowCount = 1
        tlpButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpButoane.TabIndex = 2
        '
        ' lblStare
        '
        lblStare.Dock = DockStyle.Fill
        lblStare.Name = "lblStare"
        lblStare.TabIndex = 0
        lblStare.Text = ""
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnInchide
        '
        btnInchide.Dock = DockStyle.Fill
        btnInchide.Name = "btnInchide"
        btnInchide.TabIndex = 1
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' btnPorteaza
        '
        btnPorteaza.Dock = DockStyle.Fill
        btnPorteaza.Name = "btnPorteaza"
        btnPorteaza.TabIndex = 2
        btnPorteaza.Text = "Portează bifatele"
        btnPorteaza.UseVisualStyleBackColor = True
        '
        ' tipSub
        '
        tipSub.SetToolTipHeader(dgvSubunitati, "Subunități")
        tipSub.SetToolTipText(dgvSubunitati, "Câte o linie pe unitate din registru. Bifați unitățile de portat; denumirea, capitolul și zecimalele salariilor se corectează direct în grilă. Unitățile deja portate nu se mai schimbă aici.")
        tipSub.SetToolTipHeader(btnPorteaza, "Portează bifatele")
        tipSub.SetToolTipText(btnPorteaza, "Scrie în baza unității câte o subunitate pentru fiecare unitate bifată, toate odată sau niciuna. Prima subunitate a bazei devine cea implicită.")
        '
        ' AvcSubunitsForm
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(940, 480)
        Controls.Add(tlpRoot)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "AvcSubunitsForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Subunități"
        CType(dgvSubunitati, System.ComponentModel.ISupportInitialize).EndInit()
        tlpRoot.ResumeLayout(False)
        tlpButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
