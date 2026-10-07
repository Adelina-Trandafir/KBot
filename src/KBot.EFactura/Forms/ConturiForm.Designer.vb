Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ConturiForm
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        tips = New Global.KBot.Controls.KBotToolTip(components)
        pnlCard = New System.Windows.Forms.Panel()
        gridConturi = New KBotDataView()
        ntfMesaj = New Global.KBot.Controls.KBotNotice()
        lblInfo = New System.Windows.Forms.Label()
        pnlJos = New System.Windows.Forms.Panel()
        btnNou = New System.Windows.Forms.Button()
        btnSalveaza = New System.Windows.Forms.Button()
        lblSep1 = New System.Windows.Forms.Label()
        btnInchide = New System.Windows.Forms.Button()
        busy = New Global.KBot.Controls.KBotBusyBar()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        CType(gridConturi, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(gridConturi)
        pnlCard.Controls.Add(ntfMesaj)
        pnlCard.Controls.Add(lblInfo)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(busy)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(2, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(716, 456)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' gridConturi
        '
        gridConturi.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridConturi.ColumnFillMode = KBotFillMode.SpecificColumn
        gridConturi.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Cont (IBAN)"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "cont"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Width = 300
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Banca (dedusă din cont)"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "banca"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 300
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.ColumnType = KBotColumnType.Button
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = ""
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleLeft
        KBotDataColumn3.Key = "sterge"
        KBotDataColumn3.MinWidth = 34
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.Resizable = False
        KBotDataColumn3.Width = 34
        gridConturi.Columns.Add(KBotDataColumn1)
        gridConturi.Columns.Add(KBotDataColumn2)
        gridConturi.Columns.Add(KBotDataColumn3)
        gridConturi.Dock = System.Windows.Forms.DockStyle.Fill
        gridConturi.FillColumnKey = "banca"
        gridConturi.Location = New System.Drawing.Point(0, 130)
        gridConturi.Margin = New System.Windows.Forms.Padding(0)
        gridConturi.Name = "gridConturi"
        gridConturi.Size = New System.Drawing.Size(716, 220)
        gridConturi.TabIndex = 3
        tips.SetToolTipHeader(gridConturi, "Conturile unității")
        tips.SetToolTipText(gridConturi, "Scrieți IBAN-ul direct în tabel; banca se deduce din cont la salvare. «✕» șterge rândul. Totul se scrie la «Salvează».")
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = System.Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Top
        ntfMesaj.Location = New System.Drawing.Point(0, 86)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(716, 44)
        ntfMesaj.TabIndex = 2
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' lblInfo
        '
        lblInfo.Dock = System.Windows.Forms.DockStyle.Top
        lblInfo.Location = New System.Drawing.Point(0, 46)
        lblInfo.Name = "lblInfo"
        lblInfo.Padding = New System.Windows.Forms.Padding(16, 8, 16, 8)
        lblInfo.Size = New System.Drawing.Size(716, 40)
        lblInfo.TabIndex = 1
        lblInfo.Text = "Conturile unității care emite facturile (nu ale partenerilor). Pe factură se alege unul dintre ele, în fila «Generale»."
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnNou)
        pnlJos.Controls.Add(btnSalveaza)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Controls.Add(btnInchide)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 398)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(716, 58)
        pnlJos.TabIndex = 4
        pnlJos.Tag = "Card"
        '
        ' btnNou
        '
        btnNou.Dock = System.Windows.Forms.DockStyle.Left
        btnNou.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnNou.Location = New System.Drawing.Point(8, 8)
        btnNou.Name = "btnNou"
        btnNou.Size = New System.Drawing.Size(140, 42)
        btnNou.TabIndex = 0
        btnNou.Text = "Cont nou"
        tips.SetToolTipHeader(btnNou, "Cont nou")
        tips.SetToolTipText(btnNou, "Adaugă un rând gol la sfârșitul listei; IBAN-ul se scrie direct în tabel.")
        btnNou.UseVisualStyleBackColor = True
        '
        ' btnSalveaza
        '
        btnSalveaza.Dock = System.Windows.Forms.DockStyle.Right
        btnSalveaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnSalveaza.Location = New System.Drawing.Point(388, 8)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New System.Drawing.Size(168, 42)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Verifică conturile și le scrie pe server; banca fiecărui cont se deduce din codul băncii din IBAN.")
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' lblSep1
        '
        lblSep1.Dock = System.Windows.Forms.DockStyle.Right
        lblSep1.Location = New System.Drawing.Point(556, 8)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(12, 42)
        lblSep1.TabIndex = 2
        '
        ' btnInchide
        '
        btnInchide.Dock = System.Windows.Forms.DockStyle.Right
        btnInchide.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnInchide.Location = New System.Drawing.Point(568, 8)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New System.Drawing.Size(140, 42)
        btnInchide.TabIndex = 3
        btnInchide.Text = "Închide"
        tips.SetToolTipHeader(btnInchide, "Închide")
        tips.SetToolTipText(btnInchide, "Închide fereastra; dacă sunt modificări nesalvate, întreabă mai întâi.")
        btnInchide.UseVisualStyleBackColor = True
        '
        ' busy
        '
        busy.Dock = System.Windows.Forms.DockStyle.Top
        busy.Location = New System.Drawing.Point(0, 40)
        busy.Name = "busy"
        busy.Size = New System.Drawing.Size(716, 6)
        busy.TabIndex = 1
        '
        ' capBar
        '
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New System.Drawing.Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New System.Drawing.Size(716, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "E-Factura — conturile unității"
        '
        ' ConturiForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnInchide
        ClientSize = New System.Drawing.Size(720, 460)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "ConturiForm"
        Padding = New System.Windows.Forms.Padding(2)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "E-Factura — conturile unității"
        pnlCard.ResumeLayout(False)
        CType(gridConturi, System.ComponentModel.ISupportInitialize).EndInit()
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents busy As Global.KBot.Controls.KBotBusyBar
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents ntfMesaj As Global.KBot.Controls.KBotNotice
    Friend WithEvents gridConturi As KBotDataView
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnNou As System.Windows.Forms.Button
    Friend WithEvents btnSalveaza As System.Windows.Forms.Button
    Friend WithEvents lblSep1 As System.Windows.Forms.Label
    Friend WithEvents btnInchide As System.Windows.Forms.Button
End Class
