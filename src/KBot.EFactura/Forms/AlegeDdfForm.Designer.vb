<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AlegeDdfForm
    Inherits Global.KBot.Theming.KBotThemedForm

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
        components = New System.ComponentModel.Container()
        Dim KBotDataColumn1 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn2 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn3 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn4 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        tips = New Global.KBot.Controls.KBotToolTip(components)
        pnlCard = New System.Windows.Forms.Panel()
        gridDdf = New Global.KBot.Controls.KBotDataView()
        pnlJos = New System.Windows.Forms.Panel()
        btnLeaga = New System.Windows.Forms.Button()
        lblSep1 = New System.Windows.Forms.Label()
        btnRenunta = New System.Windows.Forms.Button()
        ntfMesaj = New Global.KBot.Controls.KBotNotice()
        txtCauta = New Global.KBot.Controls.KBotTextField()
        lblAntet = New System.Windows.Forms.Label()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlJos.SuspendLayout()
        CType(gridDdf, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(gridDdf)
        pnlCard.Controls.Add(ntfMesaj)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(txtCauta)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(2, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(796, 546)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' gridDdf
        '
        gridDdf.AutoSizeColumnsMode = Global.KBot.Controls.KBotAutoSizeMode.None
        gridDdf.ColumnFillMode = Global.KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Angajament"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "cod"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 150
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Obiectul fundamentării"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "obiect"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 300
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Partener"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "partener"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 200
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Cod fiscal"
        KBotDataColumn4.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "cf"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 110
        gridDdf.Columns.Add(KBotDataColumn1)
        gridDdf.Columns.Add(KBotDataColumn2)
        gridDdf.Columns.Add(KBotDataColumn3)
        gridDdf.Columns.Add(KBotDataColumn4)
        gridDdf.Dock = System.Windows.Forms.DockStyle.Fill
        gridDdf.FillColumnKey = "obiect"
        gridDdf.Location = New System.Drawing.Point(0, 128)
        gridDdf.Margin = New System.Windows.Forms.Padding(0)
        gridDdf.Name = "gridDdf"
        gridDdf.ReadOnlyGrid = True
        gridDdf.Size = New System.Drawing.Size(796, 300)
        gridDdf.TabIndex = 3
        tips.SetToolTipHeader(gridDdf, "Fundamentările")
        tips.SetToolTipText(gridDdf, "Alege fundamentarea de care se leagă factura. Dublu clic o alege direct.")
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = System.Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 428)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(4)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(796, 60)
        ntfMesaj.TabIndex = 7
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnLeaga)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 488)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(796, 58)
        pnlJos.TabIndex = 6
        pnlJos.Tag = "Card"
        '
        ' btnLeaga
        '
        btnLeaga.Dock = System.Windows.Forms.DockStyle.Right
        btnLeaga.Enabled = False
        btnLeaga.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnLeaga.Location = New System.Drawing.Point(628, 8)
        btnLeaga.Name = "btnLeaga"
        btnLeaga.Size = New System.Drawing.Size(160, 42)
        btnLeaga.TabIndex = 0
        btnLeaga.Text = "Leagă"
        tips.SetToolTipHeader(btnLeaga, "Leagă")
        tips.SetToolTipText(btnLeaga, "Leagă factura de fundamentarea aleasă.")
        btnLeaga.UseVisualStyleBackColor = True
        '
        ' lblSep1
        '
        lblSep1.Dock = System.Windows.Forms.DockStyle.Right
        lblSep1.Location = New System.Drawing.Point(616, 8)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(12, 42)
        lblSep1.TabIndex = 1
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = System.Windows.Forms.DialogResult.Cancel
        btnRenunta.Dock = System.Windows.Forms.DockStyle.Left
        btnRenunta.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnRenunta.Location = New System.Drawing.Point(8, 8)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New System.Drawing.Size(117, 42)
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să lege nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' txtCauta
        '
        txtCauta.BackColor = System.Drawing.Color.Transparent
        txtCauta.Dock = System.Windows.Forms.DockStyle.Top
        txtCauta.Location = New System.Drawing.Point(0, 80)
        txtCauta.MaxLength = 100
        txtCauta.Name = "txtCauta"
        txtCauta.PlaceholderText = "Caută după angajament, obiect, partener sau cod fiscal…"
        txtCauta.Size = New System.Drawing.Size(796, 48)
        txtCauta.TabIndex = 2
        tips.SetToolTipText(txtCauta, "Arată doar fundamentările care conțin textul scris.")
        '
        ' lblAntet
        '
        lblAntet.Dock = System.Windows.Forms.DockStyle.Top
        lblAntet.Location = New System.Drawing.Point(0, 40)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New System.Windows.Forms.Padding(14, 10, 14, 6)
        lblAntet.Size = New System.Drawing.Size(796, 40)
        lblAntet.TabIndex = 1
        '
        ' capBar
        '
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New System.Drawing.Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New System.Drawing.Size(796, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "E-Factura — legarea unei facturi primite"
        '
        ' AlegeDdfForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New System.Drawing.Size(800, 550)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(700, 450)
        Name = "AlegeDdfForm"
        Padding = New System.Windows.Forms.Padding(2)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "E-Factura — legarea unei facturi primite"
        pnlCard.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        CType(gridDdf, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As System.Windows.Forms.Label
    Friend WithEvents txtCauta As Global.KBot.Controls.KBotTextField
    Friend WithEvents gridDdf As Global.KBot.Controls.KBotDataView
    Friend WithEvents ntfMesaj As Global.KBot.Controls.KBotNotice
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnLeaga As System.Windows.Forms.Button
    Friend WithEvents lblSep1 As System.Windows.Forms.Label
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
End Class
