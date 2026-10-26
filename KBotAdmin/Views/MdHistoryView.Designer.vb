' The «Istoric md» page: the project's markdown files over time (a lane per slice, and a graph with a time cursor). All controls are
' declared HERE; coordinates are in the 96 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MdHistoryView
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
        lblMode = New Label()
        cmbMode = New KBotComboBox()
        lblFamily = New Label()
        cmbFamily = New KBotComboBox()
        chkEdits = New KBotCheckBox()
        btnReload = New KBotButton()
        pnlContent = New Panel()
        laneView = New KBotLaneView()
        graphView = New KBotGraphView()
        tlyCursor = New KBotTableLayoutPanel()
        scrollTime = New KBotScrollBar()
        lblCursor = New Label()
        tlyDetail = New KBotTableLayoutPanel()
        lblDetail = New Label()
        btnOpen = New KBotButton()
        CType(laneView, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        pnlContent.SuspendLayout()
        tlyCursor.SuspendLayout()
        tlyDetail.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(pnlContent, 0, 1)
        tlyMain.Controls.Add(tlyCursor, 0, 2)
        tlyMain.Controls.Add(tlyDetail, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 58F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 7
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 64F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 72F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(lblMode, 0, 0)
        tlyBar.Controls.Add(cmbMode, 1, 0)
        tlyBar.Controls.Add(lblFamily, 2, 0)
        tlyBar.Controls.Add(cmbFamily, 3, 0)
        tlyBar.Controls.Add(chkEdits, 4, 0)
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
        ' lblMode
        '
        tips.SetToolTipHeader(lblMode, "Vedere")
        tips.SetToolTipText(lblMode, "Cronologie pe felii: un rând pe felie, un punct pe fișier. Graf în timp: fișierele și legăturile dintre ele, cu un cursor de timp.")
        lblMode.Dock = DockStyle.Fill
        lblMode.Location = New Point(0, 0)
        lblMode.Margin = New Padding(0)
        lblMode.Name = "lblMode"
        lblMode.Size = New Size(64, 44)
        lblMode.TabIndex = 0
        lblMode.Text = "Vedere:"
        lblMode.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbMode
        '
        cmbMode.Dock = DockStyle.Fill
        cmbMode.Location = New Point(64, 0)
        cmbMode.Margin = New Padding(0, 4, 8, 8)
        cmbMode.Name = "cmbMode"
        cmbMode.Size = New Size(192, 32)
        cmbMode.TabIndex = 1
        tips.SetToolTipHeader(cmbMode, "Vedere")
        tips.SetToolTipText(cmbMode, "Alege între cronologia pe felii și graful în timp.")
        '
        ' lblFamily
        '
        tips.SetToolTipHeader(lblFamily, "Familie")
        tips.SetToolTipText(lblFamily, "Arată doar fișierele unei familii: K-BOT, VERCON, ADECHIT, AVACONT, Ajutor ...")
        lblFamily.Dock = DockStyle.Fill
        lblFamily.Location = New Point(264, 0)
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
        cmbFamily.Location = New Point(336, 0)
        cmbFamily.Margin = New Padding(0, 4, 8, 8)
        cmbFamily.Name = "cmbFamily"
        cmbFamily.Size = New Size(192, 32)
        cmbFamily.TabIndex = 3
        tips.SetToolTipHeader(cmbFamily, "Familie")
        tips.SetToolTipText(cmbFamily, "Arată doar fișierele unei familii.")
        '
        ' chkEdits
        '
        chkEdits.Dock = DockStyle.Fill
        chkEdits.Location = New Point(544, 0)
        chkEdits.Margin = New Padding(0)
        chkEdits.Name = "chkEdits"
        chkEdits.Size = New Size(220, 44)
        chkEdits.TabIndex = 4
        chkEdits.Text = "Arată și modificările"
        tips.SetToolTipHeader(chkEdits, "Modificări")
        tips.SetToolTipText(chkEdits, "Bifat: pe cronologie, fiecare zi în care un fișier a fost modificat primește un punct gol, pe lângă punctul plin al apariției lui.")
        chkEdits.UseVisualStyleBackColor = True
        '
        ' btnReload
        '
        tips.SetToolTipHeader(btnReload, "Citește din nou")
        tips.SetToolTipText(btnReload, "Citește din nou fișierele md și istoricul lor din git.")
        btnReload.Dock = DockStyle.Fill
        btnReload.Location = New Point(764, 0)
        btnReload.Margin = New Padding(0, 2, 6, 6)
        btnReload.Name = "btnReload"
        btnReload.Size = New Size(164, 36)
        btnReload.TabIndex = 5
        btnReload.Text = "Citește din nou"
        '
        ' pnlContent (both views fill it; one is visible at a time)
        '
        pnlContent.Controls.Add(laneView)
        pnlContent.Controls.Add(graphView)
        pnlContent.Dock = DockStyle.Fill
        pnlContent.Location = New Point(8, 52)
        pnlContent.Margin = New Padding(0)
        pnlContent.Name = "pnlContent"
        pnlContent.Size = New Size(884, 406)
        pnlContent.TabIndex = 1
        '
        ' laneView
        '
        laneView.AxisVisible = True
        laneView.Dock = DockStyle.Fill
        laneView.EmptyText = "Nu există fișiere md pentru filtrul ales."
        laneView.EnlargeButtonVisible = False
        laneView.HeaderVisible = False
        laneView.LaneCaptionsVisible = True
        laneView.LaneCaptionWidth = 96
        laneView.LaneHeight = 18
        laneView.Location = New Point(0, 0)
        laneView.Margin = New Padding(0)
        laneView.MarkerSize = 8
        laneView.Name = "laneView"
        laneView.Size = New Size(884, 406)
        laneView.TabIndex = 0
        laneView.TrailingSpace = 40
        '
        ' graphView
        '
        graphView.Dock = DockStyle.Fill
        graphView.EmptyText = "Niciun fișier până la data aleasă."
        graphView.Location = New Point(0, 0)
        graphView.Margin = New Padding(0)
        graphView.Name = "graphView"
        graphView.Size = New Size(884, 406)
        graphView.TabIndex = 1
        graphView.Visible = False
        '
        ' tlyCursor
        '
        tlyCursor.AutoFitToTheme = False
        tlyCursor.ColumnCount = 2
        tlyCursor.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCursor.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 420F))
        tlyCursor.Controls.Add(scrollTime, 0, 0)
        tlyCursor.Controls.Add(lblCursor, 1, 0)
        tlyCursor.Dock = DockStyle.Fill
        tlyCursor.Location = New Point(8, 458)
        tlyCursor.Margin = New Padding(0)
        tlyCursor.Name = "tlyCursor"
        tlyCursor.RowCount = 1
        tlyCursor.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCursor.Size = New Size(884, 36)
        tlyCursor.TabIndex = 2
        tlyCursor.Visible = False
        '
        ' scrollTime
        '
        tips.SetToolTipHeader(scrollTime, "Cursor de timp")
        tips.SetToolTipText(scrollTime, "Trage ca să vezi proiectul la o altă dată: fișierele apărute după ea dispar din graf, iar cele modificate în ultima săptămână au un inel.")
        scrollTime.Dock = DockStyle.Fill
        scrollTime.LargeChange = 7
        scrollTime.Location = New Point(0, 0)
        scrollTime.Margin = New Padding(0, 8, 12, 8)
        scrollTime.Name = "scrollTime"
        scrollTime.Orientation = Orientation.Horizontal
        scrollTime.Size = New Size(452, 20)
        scrollTime.TabIndex = 0
        '
        ' lblCursor
        '
        tips.SetToolTipHeader(lblCursor, "Data cursorului")
        tips.SetToolTipText(lblCursor, "Data aleasă cu cursorul, câte fișiere existau până atunci și câte au fost atinse în ultima săptămână.")
        lblCursor.AutoEllipsis = True
        lblCursor.Dock = DockStyle.Fill
        lblCursor.Location = New Point(464, 0)
        lblCursor.Margin = New Padding(0)
        lblCursor.Name = "lblCursor"
        lblCursor.Size = New Size(420, 36)
        lblCursor.TabIndex = 1
        lblCursor.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyDetail
        '
        tlyDetail.AutoFitToTheme = False
        tlyDetail.ColumnCount = 2
        tlyDetail.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyDetail.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyDetail.Controls.Add(lblDetail, 0, 0)
        tlyDetail.Controls.Add(btnOpen, 1, 0)
        tlyDetail.Dock = DockStyle.Fill
        tlyDetail.Location = New Point(8, 494)
        tlyDetail.Margin = New Padding(0)
        tlyDetail.Name = "tlyDetail"
        tlyDetail.RowCount = 1
        tlyDetail.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyDetail.Size = New Size(884, 58)
        tlyDetail.TabIndex = 3
        '
        ' lblDetail
        '
        tips.SetToolTipHeader(lblDetail, "Fișierul ales")
        tips.SetToolTipText(lblDetail, "Fișierul de sub cursor sau ales cu un clic: felia, datele și legăturile lui.")
        lblDetail.AutoEllipsis = True
        lblDetail.Dock = DockStyle.Fill
        lblDetail.Location = New Point(0, 0)
        lblDetail.Margin = New Padding(0)
        lblDetail.Name = "lblDetail"
        lblDetail.Size = New Size(714, 58)
        lblDetail.TabIndex = 0
        lblDetail.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnOpen
        '
        tips.SetToolTipHeader(btnOpen, "Deschide fișierul")
        tips.SetToolTipText(btnOpen, "Deschide fișierul md ales în programul care îl citește pe acest calculator.")
        btnOpen.Dock = DockStyle.Fill
        btnOpen.Enabled = False
        btnOpen.Location = New Point(714, 0)
        btnOpen.Margin = New Padding(6, 10, 0, 10)
        btnOpen.Name = "btnOpen"
        btnOpen.Size = New Size(164, 38)
        btnOpen.TabIndex = 1
        btnOpen.Text = "Deschide fișierul"
        '
        ' MdHistoryView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "MdHistoryView"
        Size = New Size(900, 560)
        CType(laneView, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        pnlContent.ResumeLayout(False)
        tlyCursor.ResumeLayout(False)
        tlyDetail.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents lblMode As Label
    Friend WithEvents cmbMode As KBotComboBox
    Friend WithEvents lblFamily As Label
    Friend WithEvents cmbFamily As KBotComboBox
    Friend WithEvents chkEdits As KBotCheckBox
    Friend WithEvents btnReload As KBotButton
    Friend WithEvents pnlContent As Panel
    Friend WithEvents laneView As KBotLaneView
    Friend WithEvents graphView As KBotGraphView
    Friend WithEvents tlyCursor As KBotTableLayoutPanel
    Friend WithEvents scrollTime As KBotScrollBar
    Friend WithEvents lblCursor As Label
    Friend WithEvents tlyDetail As KBotTableLayoutPanel
    Friend WithEvents lblDetail As Label
    Friend WithEvents btnOpen As KBotButton
End Class
