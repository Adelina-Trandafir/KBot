<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RobotQueueForm
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
        components = New ComponentModel.Container()
        Dim KBotDataColumn1 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn2 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn3 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn4 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        tips = New KBot.Controls.KBotToolTip(components)
        btnPauza = New Button()
        btnGoleste = New Button()
        btnOpreste = New Button()
        pnlCard = New Panel()
        pnlDescarcari = New Panel()
        gridAsteapta = New Controls.KBotDataView()
        lblInCoada = New Label()
        gridDescarcari = New Controls.KBotDataView()
        pnlFoot = New Panel()
        lblCurent = New Label()
        capBar = New Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlDescarcari.SuspendLayout()
        CType(gridAsteapta, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridDescarcari, ComponentModel.ISupportInitialize).BeginInit()
        pnlFoot.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnPauza
        ' 
        btnPauza.Dock = DockStyle.Left
        btnPauza.FlatStyle = FlatStyle.Flat
        btnPauza.Image = My.Resources.Resources.pause
        btnPauza.Location = New Point(40, 0)
        btnPauza.Margin = New Padding(4)
        btnPauza.Name = "btnPauza"
        btnPauza.Size = New Size(40, 40)
        btnPauza.TabIndex = 0
        tips.SetToolTipHeader(btnPauza, "Pauză / Continuă")
        tips.SetToolTipText(btnPauza, "Pauză: sarcina în lucru se termină, următoarele nu mai pornesc." & vbLf & "Continuă: coada pornește din nou, în aceeași ordine.")
        btnPauza.UseVisualStyleBackColor = True
        ' 
        ' btnGoleste
        ' 
        btnGoleste.Dock = DockStyle.Right
        btnGoleste.FlatStyle = FlatStyle.Flat
        btnGoleste.Image = My.Resources.Resources.stop_round
        btnGoleste.Location = New Point(493, 0)
        btnGoleste.Margin = New Padding(4)
        btnGoleste.Name = "btnGoleste"
        btnGoleste.Size = New Size(40, 40)
        btnGoleste.TabIndex = 2
        tips.SetToolTipHeader(btnGoleste, "Golește coada")
        tips.SetToolTipText(btnGoleste, "Scoate toate sarcinile care așteaptă." & vbLf & "Sarcina în lucru se termină normal.")
        btnGoleste.UseVisualStyleBackColor = True
        ' 
        ' btnOpreste
        ' 
        btnOpreste.Dock = DockStyle.Left
        btnOpreste.FlatStyle = FlatStyle.Flat
        btnOpreste.Image = My.Resources.Resources._stop
        btnOpreste.Location = New Point(0, 0)
        btnOpreste.Margin = New Padding(4)
        btnOpreste.Name = "btnOpreste"
        btnOpreste.Size = New Size(40, 40)
        btnOpreste.TabIndex = 3
        tips.SetToolTipHeader(btnOpreste, "Oprește curenta")
        tips.SetToolTipText(btnOpreste, "Oprește robotul din sarcina în lucru (ca «Anulează» din consolă)." & vbLf & "Se poate doar cât robotul lucrează în FOREXE; ce s-a salvat deja rămâne.")
        btnOpreste.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(pnlDescarcari)
        pnlCard.Controls.Add(pnlFoot)
        pnlCard.Controls.Add(lblCurent)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(533, 423)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' pnlDescarcari
        ' 
        pnlDescarcari.Controls.Add(gridAsteapta)
        pnlDescarcari.Controls.Add(lblInCoada)
        pnlDescarcari.Controls.Add(gridDescarcari)
        pnlDescarcari.Dock = DockStyle.Fill
        pnlDescarcari.Location = New Point(0, 100)
        pnlDescarcari.Margin = New Padding(4)
        pnlDescarcari.Name = "pnlDescarcari"
        pnlDescarcari.Padding = New Padding(18, 0, 18, 0)
        pnlDescarcari.Size = New Size(533, 283)
        pnlDescarcari.TabIndex = 4
        pnlDescarcari.Tag = "Card"
        ' 
        ' gridAsteapta
        ' 
        gridAsteapta.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        gridAsteapta.BackColor = SystemColors.Window
        gridAsteapta.CellTooltip.Enabled = False
        gridAsteapta.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Angajament"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn1.Key = "cod"
        KBotDataColumn1.MinWidth = 100
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 291
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ButtonImage = My.Resources.Resources.minus_red
        KBotDataColumn2.ButtonMargin = New Padding(0)
        KBotDataColumn2.ColumnType = KBot.Controls.KBotColumnType.Button
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = ""
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Key = "scoate"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Resizable = False
        KBotDataColumn2.Width = 40
        gridAsteapta.Columns.Add(KBotDataColumn1)
        gridAsteapta.Columns.Add(KBotDataColumn2)
        gridAsteapta.Dock = DockStyle.Fill
        gridAsteapta.FillColumnKey = "cod"
        gridAsteapta.Location = New Point(18, 163)
        gridAsteapta.Margin = New Padding(4)
        gridAsteapta.Name = "gridAsteapta"
        gridAsteapta.ReadOnlyGrid = True
        gridAsteapta.Selectable = False
        gridAsteapta.ShowHeader = False
        gridAsteapta.Size = New Size(497, 120)
        gridAsteapta.TabIndex = 2
        ' 
        ' lblInCoada
        ' 
        lblInCoada.Dock = DockStyle.Top
        lblInCoada.Location = New Point(18, 118)
        lblInCoada.Margin = New Padding(4, 0, 4, 0)
        lblInCoada.Name = "lblInCoada"
        lblInCoada.Size = New Size(497, 45)
        lblInCoada.TabIndex = 1
        lblInCoada.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' gridDescarcari
        ' 
        gridDescarcari.AlternatingRows = False
        gridDescarcari.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        gridDescarcari.BackColor = SystemColors.Window
        gridDescarcari.BorderWidth = 0
        gridDescarcari.CellTooltip.Enabled = False
        gridDescarcari.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.CellBorders = KBot.Controls.KBotBorderSides.None
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Angajament"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn3.Key = "cod"
        KBotDataColumn3.MinWidth = 100
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 113
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.CellBorders = KBot.Controls.KBotBorderSides.None
        KBotDataColumn4.ColumnType = KBot.Controls.KBotColumnType.ProgressBar
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Progres"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn4.Key = "prog"
        KBotDataColumn4.MinWidth = 100
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 178
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ButtonImage = My.Resources.Resources.minus_red
        KBotDataColumn5.ButtonMargin = New Padding(0)
        KBotDataColumn5.CellBorders = KBot.Controls.KBotBorderSides.All
        KBotDataColumn5.ColumnType = KBot.Controls.KBotColumnType.Button
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = ""
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn5.Key = "opreste"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Resizable = False
        KBotDataColumn5.Width = 40
        gridDescarcari.Columns.Add(KBotDataColumn3)
        gridDescarcari.Columns.Add(KBotDataColumn4)
        gridDescarcari.Columns.Add(KBotDataColumn5)
        gridDescarcari.Dock = DockStyle.Top
        gridDescarcari.FillColumnKey = "cod"
        gridDescarcari.Location = New Point(18, 0)
        gridDescarcari.Margin = New Padding(4)
        gridDescarcari.Name = "gridDescarcari"
        gridDescarcari.ReadOnlyGrid = True
        gridDescarcari.Selectable = False
        gridDescarcari.ShowHeader = False
        gridDescarcari.Size = New Size(497, 118)
        gridDescarcari.TabIndex = 0
        ' 
        ' pnlFoot
        ' 
        pnlFoot.Controls.Add(btnPauza)
        pnlFoot.Controls.Add(btnGoleste)
        pnlFoot.Controls.Add(btnOpreste)
        pnlFoot.Dock = DockStyle.Bottom
        pnlFoot.Location = New Point(0, 383)
        pnlFoot.Margin = New Padding(4)
        pnlFoot.Name = "pnlFoot"
        pnlFoot.Size = New Size(533, 40)
        pnlFoot.TabIndex = 2
        pnlFoot.Tag = "Card"
        ' 
        ' lblCurent
        ' 
        lblCurent.Dock = DockStyle.Top
        lblCurent.Font = New Font("Segoe UI Semibold", 9.75F)
        lblCurent.Location = New Point(0, 40)
        lblCurent.Margin = New Padding(4, 0, 4, 0)
        lblCurent.Name = "lblCurent"
        lblCurent.Padding = New Padding(18, 4, 18, 4)
        lblCurent.Size = New Size(533, 60)
        lblCurent.TabIndex = 0
        lblCurent.Text = "Nicio sarcină în lucru."
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New Size(533, 40)
        capBar.TabIndex = 3
        capBar.TabStop = False
        capBar.Text = "Coada robotului"
        ' 
        ' RobotQueueForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(537, 427)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(400, 300)
        Name = "RobotQueueForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Coada robotului"
        pnlCard.ResumeLayout(False)
        pnlDescarcari.ResumeLayout(False)
        CType(gridAsteapta, ComponentModel.ISupportInitialize).EndInit()
        CType(gridDescarcari, ComponentModel.ISupportInitialize).EndInit()
        pnlFoot.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblCurent As Label
    Friend WithEvents pnlDescarcari As Panel
    Friend WithEvents gridDescarcari As Global.KBot.Controls.KBotDataView
    Friend WithEvents lblInCoada As Label
    Friend WithEvents gridAsteapta As Global.KBot.Controls.KBotDataView
    Friend WithEvents pnlFoot As Panel
    Friend WithEvents btnPauza As Button
    Friend WithEvents btnGoleste As Button
    Friend WithEvents btnOpreste As Button
End Class
