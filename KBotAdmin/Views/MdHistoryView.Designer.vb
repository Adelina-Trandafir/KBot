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
        lblMode = New Label()
        cmbMode = New KBotComboBox()
        lblFamily = New Label()
        cmbFamily = New KBotComboBox()
        chkEdits = New KBotCheckBox()
        btnReload = New KBotButton()
        scrollTime = New KBotScrollBar()
        lblCursor = New Label()
        lblDetail = New Label()
        btnOpen = New KBotButton()
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        txtFileSearch = New KBotTextBox()
        pnlContent = New Panel()
        laneView = New KBotLaneView()
        graphView = New KBotGraphView()
        tlyCursor = New KBotTableLayoutPanel()
        tlyDetail = New KBotTableLayoutPanel()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        pnlContent.SuspendLayout()
        CType(laneView, ComponentModel.ISupportInitialize).BeginInit()
        tlyCursor.SuspendLayout()
        tlyDetail.SuspendLayout()
        SuspendLayout()
        ' 
        ' lblMode
        ' 
        lblMode.Dock = DockStyle.Fill
        lblMode.Location = New Point(0, 0)
        lblMode.Margin = New Padding(0)
        lblMode.Name = "lblMode"
        lblMode.Size = New Size(96, 66)
        lblMode.TabIndex = 0
        lblMode.Text = "Vedere:"
        lblMode.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblMode, "Vedere")
        tips.SetToolTipText(lblMode, "Cronologie pe felii: un rând pe felie, un punct pe fișier. Graf în timp: fișierele și legăturile dintre ele, cu un cursor de timp.")
        ' 
        ' cmbMode
        ' 
        cmbMode.Dock = DockStyle.Fill
        cmbMode.Location = New Point(96, 6)
        cmbMode.Margin = New Padding(0, 6, 12, 12)
        cmbMode.Name = "cmbMode"
        cmbMode.Size = New Size(288, 37)
        cmbMode.TabIndex = 1
        tips.SetToolTipHeader(cmbMode, "Vedere")
        tips.SetToolTipText(cmbMode, "Alege între cronologia pe felii și graful în timp.")
        ' 
        ' lblFamily
        ' 
        lblFamily.Dock = DockStyle.Fill
        lblFamily.Location = New Point(396, 0)
        lblFamily.Margin = New Padding(0)
        lblFamily.Name = "lblFamily"
        lblFamily.Size = New Size(108, 66)
        lblFamily.TabIndex = 2
        lblFamily.Text = "Familie:"
        lblFamily.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblFamily, "Familie")
        tips.SetToolTipText(lblFamily, "Arată doar fișierele unei familii: K-BOT, VERCON, ADECHIT, AVACONT, Ajutor ...")
        ' 
        ' cmbFamily
        ' 
        cmbFamily.Dock = DockStyle.Fill
        cmbFamily.Location = New Point(504, 6)
        cmbFamily.Margin = New Padding(0, 6, 12, 12)
        cmbFamily.Name = "cmbFamily"
        cmbFamily.Size = New Size(288, 37)
        cmbFamily.TabIndex = 3
        tips.SetToolTipHeader(cmbFamily, "Familie")
        tips.SetToolTipText(cmbFamily, "Arată doar fișierele unei familii.")
        ' 
        ' chkEdits
        ' 
        chkEdits.BackColor = Color.Transparent
        chkEdits.Dock = DockStyle.Fill
        chkEdits.Location = New Point(804, 0)
        chkEdits.Margin = New Padding(0)
        chkEdits.Name = "chkEdits"
        chkEdits.Size = New Size(275, 66)
        chkEdits.TabIndex = 4
        chkEdits.Text = "Arată și modificările"
        tips.SetToolTipHeader(chkEdits, "Modificări")
        tips.SetToolTipText(chkEdits, "Bifat: pe cronologie, fiecare zi în care un fișier a fost modificat primește un punct gol, pe lângă punctul plin al apariției lui.")
        chkEdits.UseVisualStyleBackColor = True
        ' 
        ' btnReload
        ' 
        btnReload.AccessibleRole = AccessibleRole.PushButton
        btnReload.Dock = DockStyle.Fill
        btnReload.Location = New Point(1079, 3)
        btnReload.Margin = New Padding(0, 3, 9, 9)
        btnReload.Name = "btnReload"
        btnReload.Size = New Size(246, 54)
        btnReload.TabIndex = 5
        btnReload.Text = "Citește din nou"
        tips.SetToolTipHeader(btnReload, "Citește din nou")
        tips.SetToolTipText(btnReload, "Citește din nou fișierele md și istoricul lor din git.")
        ' 
        ' scrollTime
        ' 
        scrollTime.Dock = DockStyle.Fill
        scrollTime.LargeChange = 7
        scrollTime.Location = New Point(0, 12)
        scrollTime.Margin = New Padding(0, 12, 18, 12)
        scrollTime.Name = "scrollTime"
        scrollTime.Orientation = Orientation.Horizontal
        scrollTime.Size = New Size(686, 30)
        scrollTime.TabIndex = 0
        scrollTime.TabStop = False
        tips.SetToolTipHeader(scrollTime, "Cursor de timp")
        tips.SetToolTipText(scrollTime, "Trage ca să vezi proiectul la o altă dată: fișierele apărute după ea dispar din graf, iar cele modificate în ultima săptămână au un inel.")
        ' 
        ' lblCursor
        ' 
        lblCursor.AutoEllipsis = True
        lblCursor.Dock = DockStyle.Fill
        lblCursor.Location = New Point(704, 0)
        lblCursor.Margin = New Padding(0)
        lblCursor.Name = "lblCursor"
        lblCursor.Size = New Size(630, 54)
        lblCursor.TabIndex = 1
        lblCursor.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblCursor, "Data cursorului")
        tips.SetToolTipText(lblCursor, "Data aleasă cu cursorul, câte fișiere existau până atunci și câte au fost atinse în ultima săptămână.")
        ' 
        ' lblDetail
        ' 
        lblDetail.AutoEllipsis = True
        lblDetail.Dock = DockStyle.Fill
        lblDetail.Location = New Point(0, 0)
        lblDetail.Margin = New Padding(0)
        lblDetail.Name = "lblDetail"
        lblDetail.Size = New Size(1079, 87)
        lblDetail.TabIndex = 0
        lblDetail.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblDetail, "Fișierul ales")
        tips.SetToolTipText(lblDetail, "Fișierul de sub cursor sau ales cu un clic: felia, datele și legăturile lui.")
        ' 
        ' btnOpen
        ' 
        btnOpen.AccessibleRole = AccessibleRole.PushButton
        btnOpen.Dock = DockStyle.Fill
        btnOpen.Enabled = False
        btnOpen.Location = New Point(1088, 15)
        btnOpen.Margin = New Padding(9, 15, 0, 15)
        btnOpen.Name = "btnOpen"
        btnOpen.Size = New Size(246, 57)
        btnOpen.TabIndex = 1
        btnOpen.Text = "Deschide fișierul"
        tips.SetToolTipHeader(btnOpen, "Deschide fișierul")
        tips.SetToolTipText(btnOpen, "Deschide fișierul md ales în programul care îl citește pe acest calculator.")
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
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 120F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 54F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 87F))
        tlyMain.Size = New Size(1350, 840)
        tlyMain.TabIndex = 0
        ' 
        ' tlyBar
        ' 
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 6
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 96F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 300F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 108F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 300F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 255F))
        tlyBar.Controls.Add(lblMode, 0, 0)
        tlyBar.Controls.Add(cmbMode, 1, 0)
        tlyBar.Controls.Add(lblFamily, 2, 0)
        tlyBar.Controls.Add(cmbFamily, 3, 0)
        tlyBar.Controls.Add(chkEdits, 4, 0)
        tlyBar.Controls.Add(btnReload, 5, 0)
        tlyBar.Controls.Add(txtFileSearch, 0, 1)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 2
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Absolute, 66F))
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(1334, 120)
        tlyBar.TabIndex = 0
        ' 
        ' txtFileSearch
        ' 
        txtFileSearch.AccessibleName = "Caută după numele fișierului"
        tlyBar.SetColumnSpan(txtFileSearch, 6)
        txtFileSearch.Dock = DockStyle.Fill
        txtFileSearch.Location = New Point(0, 66)
        txtFileSearch.Margin = New Padding(0, 0, 9, 6)
        txtFileSearch.Multiline = False
        txtFileSearch.Name = "txtFileSearch"
        txtFileSearch.PlaceholderText = "Caută după numele fișierului"
        txtFileSearch.Size = New Size(1325, 48)
        txtFileSearch.TabIndex = 6
        txtFileSearch.TextPadding = New Padding(0)
        ' 
        ' pnlContent
        ' 
        pnlContent.Controls.Add(laneView)
        pnlContent.Controls.Add(graphView)
        pnlContent.Dock = DockStyle.Fill
        pnlContent.Location = New Point(8, 128)
        pnlContent.Margin = New Padding(0)
        pnlContent.Name = "pnlContent"
        pnlContent.Size = New Size(1334, 563)
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
        laneView.Size = New Size(1334, 563)
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
        graphView.Size = New Size(1334, 563)
        graphView.TabIndex = 1
        graphView.Visible = False
        ' 
        ' tlyCursor
        ' 
        tlyCursor.AutoFitToTheme = False
        tlyCursor.ColumnCount = 2
        tlyCursor.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCursor.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 630F))
        tlyCursor.Controls.Add(scrollTime, 0, 0)
        tlyCursor.Controls.Add(lblCursor, 1, 0)
        tlyCursor.Dock = DockStyle.Fill
        tlyCursor.Location = New Point(8, 691)
        tlyCursor.Margin = New Padding(0)
        tlyCursor.Name = "tlyCursor"
        tlyCursor.RowCount = 1
        tlyCursor.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCursor.Size = New Size(1334, 54)
        tlyCursor.TabIndex = 2
        tlyCursor.Visible = False
        ' 
        ' tlyDetail
        ' 
        tlyDetail.AutoFitToTheme = False
        tlyDetail.ColumnCount = 2
        tlyDetail.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyDetail.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 255F))
        tlyDetail.Controls.Add(lblDetail, 0, 0)
        tlyDetail.Controls.Add(btnOpen, 1, 0)
        tlyDetail.Dock = DockStyle.Fill
        tlyDetail.Location = New Point(8, 745)
        tlyDetail.Margin = New Padding(0)
        tlyDetail.Name = "tlyDetail"
        tlyDetail.RowCount = 1
        tlyDetail.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyDetail.Size = New Size(1334, 87)
        tlyDetail.TabIndex = 3
        ' 
        ' MdHistoryView
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Margin = New Padding(4)
        Name = "MdHistoryView"
        Size = New Size(1350, 840)
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        pnlContent.ResumeLayout(False)
        CType(laneView, ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents txtFileSearch As KBotTextBox
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
