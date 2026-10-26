' The «Sincronizare» page (slice ME01): runs the server's schema_sync on the chosen database or on all of
' them, as AvacontPush does, and shows its progress line by line. Two buttons: «Verifică diferențe» (preview) and «Sincronizează». All controls are declared HERE;
' coordinates are in the 96 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SyncView
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
        btnCheck = New KBotButton()
        btnRun = New KBotButton()
        lblTarget = New Label()
        logBox = New KBotLogBox()
        tlyFoot = New KBotTableLayoutPanel()
        lblState = New Label()
        btnCopy = New KBotButton()
        btnClear = New KBotButton()
        tmrFlush = New Timer(components)
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        tlyFoot.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(lblTarget, 0, 1)
        tlyMain.Controls.Add(logBox, 0, 2)
        tlyMain.Controls.Add(tlyFoot, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 3
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(btnCheck, 0, 0)
        tlyBar.Controls.Add(btnRun, 1, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' btnCheck
        '
        btnCheck.Dock = DockStyle.Fill
        btnCheck.Location = New Point(0, 0)
        btnCheck.Margin = New Padding(0, 2, 8, 6)
        btnCheck.Name = "btnCheck"
        btnCheck.Size = New Size(182, 36)
        btnCheck.TabIndex = 0
        btnCheck.Text = "Verifică diferențe"
        tips.SetToolTipHeader(btnCheck, "Verifică diferențe")
        tips.SetToolTipText(btnCheck, "Calculează și arată planul complet (adăugări, modificări, ștergeri), dar nu execută nimic.")
        '
        ' btnRun
        '
        btnRun.Dock = DockStyle.Fill
        btnRun.Location = New Point(190, 0)
        btnRun.Margin = New Padding(0, 2, 6, 6)
        btnRun.Name = "btnRun"
        btnRun.Primary = True
        btnRun.Size = New Size(164, 36)
        btnRun.TabIndex = 1
        btnRun.Text = "Sincronizează"
        tips.SetToolTipHeader(btnRun, "Sincronizează")
        tips.SetToolTipText(btnRun, "Execută sincronizarea pe baza aleasă sau pe toate." & vbLf & "Dacă planul conține operații distructive, se cere o a doua confirmare." & vbLf & "O singură sincronizare rulează odată.")
        '
        ' lblTarget
        '
        tips.SetToolTipHeader(lblTarget, "Ținta")
        tips.SetToolTipText(lblTarget, "Baza (sau toate bazele) pe care se face sincronizarea.")
        lblTarget.AutoEllipsis = True
        lblTarget.Dock = DockStyle.Fill
        lblTarget.Location = New Point(8, 52)
        lblTarget.Margin = New Padding(0)
        lblTarget.Name = "lblTarget"
        lblTarget.Size = New Size(884, 28)
        lblTarget.TabIndex = 1
        lblTarget.TextAlign = ContentAlignment.MiddleLeft
        '
        ' logBox
        '
        logBox.Dock = DockStyle.Fill
        logBox.Location = New Point(8, 80)
        logBox.Margin = New Padding(0, 0, 0, 6)
        logBox.Name = "logBox"
        logBox.Size = New Size(884, 392)
        logBox.TabIndex = 2
        logBox.Text = ""
        '
        ' tlyFoot
        '
        tlyFoot.AutoFitToTheme = False
        tlyFoot.ColumnCount = 3
        tlyFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyFoot.Controls.Add(lblState, 0, 0)
        tlyFoot.Controls.Add(btnClear, 1, 0)
        tlyFoot.Controls.Add(btnCopy, 2, 0)
        tlyFoot.Dock = DockStyle.Fill
        tlyFoot.Location = New Point(8, 478)
        tlyFoot.Margin = New Padding(0)
        tlyFoot.Name = "tlyFoot"
        tlyFoot.RowCount = 1
        tlyFoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFoot.Size = New Size(884, 44)
        tlyFoot.TabIndex = 3
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Codul de ieșire al ultimei rulări: 0 reușit, 2 cu avertismente, 3 cu erori.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(0, 0)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(644, 44)
        lblState.TabIndex = 0
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnClear
        '
        tips.SetToolTipHeader(btnClear, "Golește")
        tips.SetToolTipText(btnClear, "Șterge textul din fereastra de rezultat. Nu schimbă nimic pe server.")
        btnClear.Dock = DockStyle.Fill
        btnClear.Location = New Point(644, 0)
        btnClear.Margin = New Padding(0, 4, 6, 4)
        btnClear.Name = "btnClear"
        btnClear.Size = New Size(114, 36)
        btnClear.TabIndex = 1
        btnClear.Text = "Golește"
        '
        ' btnCopy
        '
        tips.SetToolTipHeader(btnCopy, "Copiază")
        tips.SetToolTipText(btnCopy, "Copiază tot rezultatul în clipboard.")
        btnCopy.Dock = DockStyle.Fill
        btnCopy.Location = New Point(764, 0)
        btnCopy.Margin = New Padding(0, 4, 0, 4)
        btnCopy.Name = "btnCopy"
        btnCopy.Size = New Size(120, 36)
        btnCopy.TabIndex = 2
        btnCopy.Text = "Copiază"
        '
        ' tmrFlush
        '
        tmrFlush.Interval = 300
        '
        ' SyncView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "SyncView"
        Size = New Size(900, 560)
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        tlyFoot.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents btnCheck As KBotButton
    Friend WithEvents btnRun As KBotButton
    Friend WithEvents lblTarget As Label
    Friend WithEvents logBox As KBotLogBox
    Friend WithEvents tlyFoot As KBotTableLayoutPanel
    Friend WithEvents lblState As Label
    Friend WithEvents btnClear As KBotButton
    Friend WithEvents btnCopy As KBotButton
    Friend WithEvents tmrFlush As Timer
End Class
