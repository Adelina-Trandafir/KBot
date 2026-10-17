' The «Loguri» page (slice ME01): the server's log files for the chosen database, in a coloured rich text
' box (time in the accent colour, errors dark red). All controls are declared HERE; coordinates are in the
' 96 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LogsView
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
        cmbFile = New KBotComboBox()
        cmbLevel = New KBotComboBox()
        cmbGens = New KBotComboBox()
        txtSearch = New KBotTextField()
        chkFollow = New CheckBox()
        btnRefresh = New KBotButton()
        btnCopy = New KBotButton()
        btnClear = New KBotButton()
        logBox = New KBotLogBox()
        lblState = New Label()
        tmrFollow = New Timer(components)
        tmrSearch = New Timer(components)
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
        tlyMain.Controls.Add(logBox, 0, 1)
        tlyMain.Controls.Add(lblState, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 8
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 96F))
        tlyBar.Controls.Add(cmbFile, 0, 0)
        tlyBar.Controls.Add(cmbLevel, 1, 0)
        tlyBar.Controls.Add(cmbGens, 2, 0)
        tlyBar.Controls.Add(txtSearch, 3, 0)
        tlyBar.Controls.Add(chkFollow, 4, 0)
        tlyBar.Controls.Add(btnRefresh, 5, 0)
        tlyBar.Controls.Add(btnCopy, 6, 0)
        tlyBar.Controls.Add(btnClear, 7, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' cmbFile
        '
        cmbFile.Dock = DockStyle.Fill
        cmbFile.Location = New Point(0, 0)
        cmbFile.Margin = New Padding(0, 4, 8, 8)
        cmbFile.Name = "cmbFile"
        cmbFile.Size = New Size(152, 32)
        cmbFile.TabIndex = 0
        tips.SetToolTipHeader(cmbFile, "Fișierul")
        tips.SetToolTipText(cmbFile, "Toate fișierele de jurnal ale serverului, sau doar unul." & vbLf & "Se filtrează după baza aleasă în arbore.")
        '
        ' cmbLevel
        '
        cmbLevel.Dock = DockStyle.Fill
        cmbLevel.Location = New Point(160, 0)
        cmbLevel.Margin = New Padding(0, 4, 8, 8)
        cmbLevel.Name = "cmbLevel"
        cmbLevel.Size = New Size(132, 32)
        cmbLevel.TabIndex = 1
        tips.SetToolTipHeader(cmbLevel, "Nivelul")
        tips.SetToolTipText(cmbLevel, "Toate liniile, doar avertismentele și erorile, sau doar erorile.")
        '
        ' cmbGens
        '
        cmbGens.Dock = DockStyle.Fill
        cmbGens.Location = New Point(300, 0)
        cmbGens.Margin = New Padding(0, 4, 8, 8)
        cmbGens.Name = "cmbGens"
        cmbGens.Size = New Size(142, 32)
        cmbGens.TabIndex = 2
        tips.SetToolTipHeader(cmbGens, "Cât de departe")
        tips.SetToolTipText(cmbGens, "Fișierul curent, sau și copiile lui rotite (mai vechi).")
        '
        ' txtSearch
        '
        txtSearch.BackColor = Color.Transparent
        txtSearch.Dock = DockStyle.Fill
        txtSearch.Location = New Point(450, 0)
        txtSearch.Margin = New Padding(0, 4, 8, 8)
        txtSearch.Name = "txtSearch"
        txtSearch.PlaceholderText = "caută în mesaje"
        txtSearch.Size = New Size(104, 32)
        txtSearch.TabIndex = 3
        txtSearch.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtSearch, "Caută")
        tips.SetToolTipText(txtSearch, "Doar liniile care conțin acest text (în mesaj, cont, bază sau detaliu).")
        '
        ' chkFollow
        '
        chkFollow.Dock = DockStyle.Fill
        chkFollow.Location = New Point(562, 0)
        chkFollow.Margin = New Padding(0)
        chkFollow.Name = "chkFollow"
        chkFollow.Size = New Size(110, 44)
        chkFollow.TabIndex = 4
        chkFollow.Text = "Urmărește"
        tips.SetToolTipHeader(chkFollow, "Urmărește")
        tips.SetToolTipText(chkFollow, "Reciteste jurnalul la fiecare 5 secunde și rămâne jos, ca un tail.")
        chkFollow.UseVisualStyleBackColor = True
        '
        ' btnRefresh
        '
        tips.SetToolTipHeader(btnRefresh, "Citește")
        tips.SetToolTipText(btnRefresh, "Citește din nou fișierul ales cu filtrele curente.")
        btnRefresh.Dock = DockStyle.Fill
        btnRefresh.Location = New Point(672, 0)
        btnRefresh.Margin = New Padding(0, 2, 6, 6)
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Primary = True
        btnRefresh.Size = New Size(104, 36)
        btnRefresh.TabIndex = 5
        btnRefresh.Text = "Citește"
        '
        ' btnCopy
        '
        btnCopy.Dock = DockStyle.Fill
        btnCopy.Location = New Point(782, 0)
        btnCopy.Margin = New Padding(0, 2, 6, 6)
        btnCopy.Name = "btnCopy"
        btnCopy.Size = New Size(90, 36)
        btnCopy.TabIndex = 6
        btnCopy.Text = "Copiază"
        tips.SetToolTipHeader(btnCopy, "Copiază")
        tips.SetToolTipText(btnCopy, "Pune în clipboard liniile afișate acum, ca text simplu.")
        '
        ' btnClear
        '
        btnClear.Dock = DockStyle.Fill
        btnClear.Location = New Point(878, 0)
        btnClear.Margin = New Padding(0, 2, 0, 6)
        btnClear.Name = "btnClear"
        btnClear.Size = New Size(90, 36)
        btnClear.TabIndex = 7
        btnClear.Text = "Golește"
        tips.SetToolTipHeader(btnClear, "Golește")
        tips.SetToolTipText(btnClear, "Șterge din fișierele de jurnal de pe server: tot, sau păstrează ultima zi ori ultima săptămână." & vbLf & "Se aplică fișierului ales în lista din stânga (sau tuturor).")
        '
        ' logBox
        '
        logBox.Dock = DockStyle.Fill
        logBox.Location = New Point(8, 52)
        logBox.Margin = New Padding(0)
        logBox.Name = "logBox"
        logBox.Size = New Size(884, 472)
        logBox.TabIndex = 1
        logBox.Text = ""
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Câte intrări se văd din câte s-au citit, cu numărătoarea pe niveluri.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 524)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 28)
        lblState.TabIndex = 2
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tmrFollow
        '
        tmrFollow.Interval = 5000
        '
        ' tmrSearch
        '
        tmrSearch.Interval = 300
        '
        ' LogsView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "LogsView"
        Size = New Size(900, 560)
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents cmbFile As KBotComboBox
    Friend WithEvents cmbLevel As KBotComboBox
    Friend WithEvents cmbGens As KBotComboBox
    Friend WithEvents txtSearch As KBotTextField
    Friend WithEvents chkFollow As CheckBox
    Friend WithEvents btnRefresh As KBotButton
    Friend WithEvents btnCopy As KBotButton
    Friend WithEvents btnClear As KBotButton
    Friend WithEvents logBox As KBotLogBox
    Friend WithEvents lblState As Label
    Friend WithEvents tmrFollow As Timer
    Friend WithEvents tmrSearch As Timer
End Class
