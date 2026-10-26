' The «Schema bazelor» page: tables of the server schemas (SURSA and COMUN, read from the dumps in MariaDB_Schema) as a graph, foreign keys as
' lines. All controls are declared HERE; coordinates are in the 96 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SchemaGraphView
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
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        lblSchema = New Label()
        cmbSchema = New KBotComboBox()
        lblFamily = New Label()
        cmbFamily = New KBotComboBox()
        chkLinked = New KBotCheckBox()
        btnReload = New KBotButton()
        graphView = New KBotGraphView()
        lblDetail = New Label()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(graphView, 0, 1)
        tlyMain.Controls.Add(lblDetail, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 58F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 7
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 72F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 180F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 72F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 250F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(lblSchema, 0, 0)
        tlyBar.Controls.Add(cmbSchema, 1, 0)
        tlyBar.Controls.Add(lblFamily, 2, 0)
        tlyBar.Controls.Add(cmbFamily, 3, 0)
        tlyBar.Controls.Add(chkLinked, 4, 0)
        tlyBar.Controls.Add(btnReload, 5, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' lblSchema
        '
        tips.SetToolTipHeader(lblSchema, "Schema")
        tips.SetToolTipText(lblSchema, "SURSA = AVACONT_SURSA, COMUN = AVACONT_COMUN, citite din dump-urile din MariaDB_Schema.")
        lblSchema.Dock = DockStyle.Fill
        lblSchema.Location = New Point(0, 0)
        lblSchema.Margin = New Padding(0)
        lblSchema.Name = "lblSchema"
        lblSchema.Size = New Size(72, 44)
        lblSchema.TabIndex = 0
        lblSchema.Text = "Schema:"
        lblSchema.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbSchema
        '
        cmbSchema.Dock = DockStyle.Fill
        cmbSchema.Location = New Point(72, 0)
        cmbSchema.Margin = New Padding(0, 4, 8, 8)
        cmbSchema.Name = "cmbSchema"
        cmbSchema.Size = New Size(172, 32)
        cmbSchema.TabIndex = 1
        tips.SetToolTipHeader(cmbSchema, "Schema")
        tips.SetToolTipText(cmbSchema, "Arată tabelele ambelor scheme sau doar ale uneia.")
        '
        ' lblFamily
        '
        tips.SetToolTipHeader(lblFamily, "Familie")
        tips.SetToolTipText(lblFamily, "Familia tabelelor: prefixul numelui (AD, FX, TZ ...). Arată doar tabelele unei familii.")
        lblFamily.Dock = DockStyle.Fill
        lblFamily.Location = New Point(252, 0)
        lblFamily.Margin = New Padding(0)
        lblFamily.Name = "lblFamily"
        lblFamily.Size = New Size(72, 44)
        lblFamily.TabIndex = 2
        lblFamily.Text = "Familie:"
        lblFamily.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbFamily
        '
        cmbFamily.Dock = DockStyle.Fill
        cmbFamily.Location = New Point(324, 0)
        cmbFamily.Margin = New Padding(0, 4, 8, 8)
        cmbFamily.Name = "cmbFamily"
        cmbFamily.Size = New Size(192, 32)
        cmbFamily.TabIndex = 3
        tips.SetToolTipHeader(cmbFamily, "Familie")
        tips.SetToolTipText(cmbFamily, "Arată doar tabelele cu prefixul ales; lângă prefix e numărul de tabele.")
        '
        ' chkLinked
        '
        chkLinked.Dock = DockStyle.Fill
        chkLinked.Location = New Point(252, 0)
        chkLinked.Margin = New Padding(0)
        chkLinked.Name = "chkLinked"
        chkLinked.Size = New Size(280, 44)
        chkLinked.TabIndex = 4
        chkLinked.Text = "Doar tabele cu legături"
        tips.SetToolTipHeader(chkLinked, "Doar tabele cu legături")
        tips.SetToolTipText(chkLinked, "Bifat: tabelele fără nicio cheie străină, nici spre ele, nici de la ele, nu se mai arată.")
        chkLinked.UseVisualStyleBackColor = True
        '
        ' btnReload
        '
        tips.SetToolTipHeader(btnReload, "Citește din nou")
        tips.SetToolTipText(btnReload, "Citește din nou dump-urile din MariaDB_Schema.")
        btnReload.Dock = DockStyle.Fill
        btnReload.Location = New Point(532, 0)
        btnReload.Margin = New Padding(0, 2, 6, 6)
        btnReload.Name = "btnReload"
        btnReload.Size = New Size(164, 36)
        btnReload.TabIndex = 5
        btnReload.Text = "Citește din nou"
        '
        ' graphView
        '
        graphView.Dock = DockStyle.Fill
        graphView.EmptyText = "Niciun tabel pentru filtrul ales."
        graphView.Location = New Point(8, 52)
        graphView.Margin = New Padding(0)
        graphView.Name = "graphView"
        graphView.Size = New Size(884, 450)
        graphView.TabIndex = 1
        '
        ' lblDetail
        '
        tips.SetToolTipHeader(lblDetail, "Tabelul ales")
        tips.SetToolTipText(lblDetail, "Tabelul de sub cursor sau ales cu un clic: coloane, cheie primară și legături.")
        lblDetail.AutoEllipsis = True
        lblDetail.Dock = DockStyle.Fill
        lblDetail.Location = New Point(8, 502)
        lblDetail.Margin = New Padding(0)
        lblDetail.Name = "lblDetail"
        lblDetail.Size = New Size(884, 50)
        lblDetail.TabIndex = 2
        lblDetail.TextAlign = ContentAlignment.MiddleLeft
        '
        ' SchemaGraphView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "SchemaGraphView"
        Size = New Size(900, 560)
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents lblSchema As Label
    Friend WithEvents cmbSchema As KBotComboBox
    Friend WithEvents lblFamily As Label
    Friend WithEvents cmbFamily As KBotComboBox
    Friend WithEvents chkLinked As KBotCheckBox
    Friend WithEvents btnReload As KBotButton
    Friend WithEvents graphView As KBotGraphView
    Friend WithEvents lblDetail As Label
End Class
