' The «Evoluția schemei» page: what the schema sync changed in a database over time (table schema_diff_log, read from the COMUN dump): a lane per
' table, a dot per DDL statement. All controls are declared HERE; coordinates are in the 96 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SchemaHistoryView
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
        lblDb = New Label()
        cmbDb = New KBotComboBox()
        lblFamily = New Label()
        cmbFamily = New KBotComboBox()
        lblKind = New Label()
        cmbKind = New KBotComboBox()
        btnReload = New KBotButton()
        laneView = New KBotLaneView()
        lblDetail = New Label()
        CType(laneView, ComponentModel.ISupportInitialize).BeginInit()
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
        tlyMain.Controls.Add(laneView, 0, 1)
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
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 70F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 210F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 72F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 64F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 250F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(lblDb, 0, 0)
        tlyBar.Controls.Add(cmbDb, 1, 0)
        tlyBar.Controls.Add(lblFamily, 2, 0)
        tlyBar.Controls.Add(cmbFamily, 3, 0)
        tlyBar.Controls.Add(lblKind, 4, 0)
        tlyBar.Controls.Add(cmbKind, 5, 0)
        tlyBar.Controls.Add(btnReload, 6, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' lblDb
        '
        tips.SetToolTipHeader(lblDb, "Baza")
        tips.SetToolTipText(lblDb, "Baza de date ale cărei modificări de schemă se arată. Implicit 000_DEMO.")
        lblDb.Dock = DockStyle.Fill
        lblDb.Location = New Point(0, 0)
        lblDb.Margin = New Padding(0)
        lblDb.Name = "lblDb"
        lblDb.Size = New Size(70, 44)
        lblDb.TabIndex = 0
        lblDb.Text = "Baza:"
        lblDb.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbDb
        '
        cmbDb.Dock = DockStyle.Fill
        cmbDb.Location = New Point(70, 0)
        cmbDb.Margin = New Padding(0, 4, 8, 8)
        cmbDb.Name = "cmbDb"
        cmbDb.Size = New Size(202, 32)
        cmbDb.TabIndex = 1
        tips.SetToolTipHeader(cmbDb, "Baza")
        tips.SetToolTipText(cmbDb, "Alege baza; lângă nume e numărul de instrucțiuni DDL din jurnal.")
        '
        ' lblFamily
        '
        tips.SetToolTipHeader(lblFamily, "Familie")
        tips.SetToolTipText(lblFamily, "Familia tabelelor: prefixul numelui (AD, FX, TZ ...). Arată doar modificările tabelelor unei familii.")
        lblFamily.Dock = DockStyle.Fill
        lblFamily.Location = New Point(280, 0)
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
        cmbFamily.Location = New Point(352, 0)
        cmbFamily.Margin = New Padding(0, 4, 8, 8)
        cmbFamily.Name = "cmbFamily"
        cmbFamily.Size = New Size(192, 32)
        cmbFamily.TabIndex = 3
        tips.SetToolTipHeader(cmbFamily, "Familie")
        tips.SetToolTipText(cmbFamily, "Arată doar tabelele cu prefixul ales; lângă prefix e numărul de instrucțiuni.")
        '
        ' lblKind
        '
        tips.SetToolTipHeader(lblKind, "Arată")
        tips.SetToolTipText(lblKind, "Ce fel de obiecte ale schemei se arată: tabele, coloane, indexuri și chei.")
        lblKind.Dock = DockStyle.Fill
        lblKind.Location = New Point(280, 0)
        lblKind.Margin = New Padding(0)
        lblKind.Name = "lblKind"
        lblKind.Size = New Size(64, 44)
        lblKind.TabIndex = 4
        lblKind.Text = "Arată:"
        lblKind.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbKind
        '
        cmbKind.Dock = DockStyle.Fill
        cmbKind.Location = New Point(344, 0)
        cmbKind.Margin = New Padding(0, 4, 8, 8)
        cmbKind.Name = "cmbKind"
        cmbKind.Size = New Size(242, 32)
        cmbKind.TabIndex = 5
        tips.SetToolTipHeader(cmbKind, "Arată")
        tips.SetToolTipText(cmbKind, "Filtrează după felul obiectului schimbat.")
        '
        ' btnReload
        '
        tips.SetToolTipHeader(btnReload, "Citește din nou")
        tips.SetToolTipText(btnReload, "Citește din nou jurnalul schema_diff_log din dump-ul COMUN.")
        btnReload.Dock = DockStyle.Left
        btnReload.Location = New Point(594, 2)
        btnReload.Margin = New Padding(0, 2, 6, 6)
        btnReload.Name = "btnReload"
        btnReload.Size = New Size(164, 36)
        btnReload.TabIndex = 6
        btnReload.Text = "Citește din nou"
        '
        ' laneView
        '
        laneView.AxisVisible = True
        laneView.Dock = DockStyle.Fill
        laneView.EmptyText = "Nicio modificare de schemă pentru filtrul ales."
        laneView.EnlargeButtonVisible = False
        laneView.HeaderVisible = False
        laneView.LaneCaptionsVisible = True
        laneView.LaneCaptionWidth = 190
        laneView.LaneHeight = 18
        laneView.Location = New Point(8, 52)
        laneView.Margin = New Padding(0)
        laneView.MarkerSize = 8
        laneView.Name = "laneView"
        laneView.Size = New Size(884, 450)
        laneView.TabIndex = 1
        laneView.TrailingSpace = 40
        '
        ' lblDetail
        '
        tips.SetToolTipHeader(lblDetail, "Modificarea aleasă")
        tips.SetToolTipText(lblDetail, "Instrucțiunea de sub cursor: ce a făcut, în ce bază, când și dacă a reușit.")
        lblDetail.AutoEllipsis = True
        lblDetail.Dock = DockStyle.Fill
        lblDetail.Location = New Point(8, 502)
        lblDetail.Margin = New Padding(0)
        lblDetail.Name = "lblDetail"
        lblDetail.Size = New Size(884, 50)
        lblDetail.TabIndex = 2
        lblDetail.TextAlign = ContentAlignment.MiddleLeft
        '
        ' SchemaHistoryView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "SchemaHistoryView"
        Size = New Size(900, 560)
        CType(laneView, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents lblDb As Label
    Friend WithEvents cmbDb As KBotComboBox
    Friend WithEvents lblFamily As Label
    Friend WithEvents cmbFamily As KBotComboBox
    Friend WithEvents lblKind As Label
    Friend WithEvents cmbKind As KBotComboBox
    Friend WithEvents btnReload As KBotButton
    Friend WithEvents laneView As KBotLaneView
    Friend WithEvents lblDetail As Label
End Class
