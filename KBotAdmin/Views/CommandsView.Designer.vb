' The «Comenzi» page (slice ME01): ready-made server commands run on the chosen database (or on all of
' them). All controls are declared HERE; coordinates are in the 96 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CommandsView
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
        cmbCommand = New KBotComboBox()
        btnRun = New KBotButton()
        btnCopy = New KBotButton()
        btnRepair = New KBotButton()
        txtParam = New KBotTextBox()
        txtOutput = New KBotTextBox()
        txtExplain = New KBotTextBox()
        lblState = New Label()
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        SuspendLayout()
        ' 
        ' cmbCommand
        ' 
        cmbCommand.Dock = DockStyle.Fill
        cmbCommand.Location = New Point(0, 6)
        cmbCommand.Margin = New Padding(0, 6, 12, 12)
        cmbCommand.Name = "cmbCommand"
        cmbCommand.Size = New Size(468, 37)
        cmbCommand.TabIndex = 0
        tips.SetToolTipHeader(cmbCommand, "Comanda")
        tips.SetToolTipText(cmbCommand, "Lista este fixă, hotărâtă pe server: nu se poate rula alt SQL de aici.")
        ' 
        ' btnRun
        ' 
        btnRun.AccessibleRole = AccessibleRole.PushButton
        btnRun.Dock = DockStyle.Fill
        btnRun.Location = New Point(480, 3)
        btnRun.Margin = New Padding(0, 3, 9, 9)
        btnRun.Name = "btnRun"
        btnRun.Primary = True
        btnRun.Size = New Size(201, 54)
        btnRun.TabIndex = 1
        btnRun.Text = "Rulează"
        tips.SetToolTipHeader(btnRun, "Rulează")
        tips.SetToolTipText(btnRun, "Rulează comanda pe baza aleasă în arbore, sau pe toate dacă e aleasă <TOATE BAZELE>." & vbLf & "Comenzile care scriu cer confirmare.")
        ' 
        ' btnCopy
        ' 
        btnCopy.AccessibleRole = AccessibleRole.PushButton
        btnCopy.Dock = DockStyle.Fill
        btnCopy.Location = New Point(690, 3)
        btnCopy.Margin = New Padding(0, 3, 9, 9)
        btnCopy.Name = "btnCopy"
        btnCopy.Size = New Size(186, 54)
        btnCopy.TabIndex = 2
        btnCopy.Text = "Copiază"
        tips.SetToolTipHeader(btnCopy, "Copiază")
        tips.SetToolTipText(btnCopy, "Pune rezultatul în clipboard.")
        '
        ' btnRepair
        '
        btnRepair.AccessibleRole = AccessibleRole.PushButton
        btnRepair.Dock = DockStyle.Fill
        btnRepair.Location = New Point(1125, 3)
        btnRepair.Margin = New Padding(0, 3, 0, 9)
        btnRepair.Name = "btnRepair"
        btnRepair.Size = New Size(210, 54)
        btnRepair.TabIndex = 4
        btnRepair.Text = "Repară"
        tips.SetToolTipHeader(btnRepair, "Repară")
        tips.SetToolTipText(btnRepair, "Apare doar după o comandă care a găsit ceva de reparat, și doar când e aleasă o singură bază." & vbLf & "Scrie în baza aleasă (cere confirmare), notează în Jurnal și arată apoi ce a mai rămas.")
        btnRepair.Visible = False
        '
        ' txtParam
        ' 
        txtParam.Dock = DockStyle.Left
        txtParam.HorizontalScrollBarPadding = New Padding(0)
        txtParam.Location = New Point(885, 6)
        txtParam.Margin = New Padding(0, 6, 12, 12)
        txtParam.MaxLength = 60
        txtParam.Name = "txtParam"
        txtParam.PlaceholderText = "Cod angajament"
        txtParam.Size = New Size(360, 48)
        txtParam.TabIndex = 3
        txtParam.TextAlign = HorizontalAlignment.Center
        txtParam.TextPadding = New Padding(0)
        tips.SetToolTipHeader(txtParam, "Valoarea comenzii")
        tips.SetToolTipText(txtParam, "Codul angajamentului. Apare doar la comenzile care au nevoie de el și dispare când alegi altă comandă." & vbLf & "Comanda cere o bază anume aleasă în arbore, nu <TOATE BAZELE>.")
        txtParam.VerticalScrollBarPadding = New Padding(0)
        txtParam.Visible = False
        ' 
        ' txtOutput
        ' 
        txtOutput.CornerRadius = 0
        txtOutput.Dock = DockStyle.Fill
        txtOutput.Font = New Font("Consolas", 9.75F)
        txtOutput.Location = New Point(8, 74)
        txtOutput.Margin = New Padding(0)
        txtOutput.MaxLength = 1000000
        txtOutput.Name = "txtOutput"
        txtOutput.ReadOnly = True
        txtOutput.ScrollBars = ScrollBars.Both
        txtOutput.Size = New Size(1334, 356)
        txtOutput.TabIndex = 2
        tips.SetToolTipHeader(txtOutput, "Rezultat")
        tips.SetToolTipText(txtOutput, "Rezultatul comenzii, câte un bloc pe fiecare bază.")
        txtOutput.WordWrap = False
        ' 
        ' txtExplain
        ' 
        txtExplain.CornerRadius = 0
        txtExplain.Dock = DockStyle.Fill
        txtExplain.Font = New Font("Segoe UI", 11F)
        txtExplain.Location = New Point(8, 439)
        txtExplain.Margin = New Padding(0, 9, 0, 0)
        txtExplain.Name = "txtExplain"
        txtExplain.ReadOnly = True
        txtExplain.Size = New Size(1334, 351)
        txtExplain.TabIndex = 3
        tips.SetToolTipHeader(txtExplain, "Ce face și ce te aștepți")
        tips.SetToolTipText(txtExplain, "Explicația comenzii alese și ce rezultat e normal. Se schimbă odată cu comanda din listă.")
        ' 
        ' lblState
        ' 
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 790)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(1334, 42)
        lblState.TabIndex = 4
        lblState.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Pe ce baze s-a rulat și cât a durat.")
        ' 
        ' tlyMain
        ' 
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(txtOutput, 0, 1)
        tlyMain.Controls.Add(txtExplain, 0, 2)
        tlyMain.Controls.Add(lblState, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 66F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 360F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tlyMain.Size = New Size(1350, 840)
        tlyMain.TabIndex = 0
        ' 
        ' tlyBar
        ' 
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 5
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 480F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 210F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 195F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 210F))
        tlyBar.Controls.Add(cmbCommand, 0, 0)
        tlyBar.Controls.Add(btnRun, 1, 0)
        tlyBar.Controls.Add(btnCopy, 2, 0)
        tlyBar.Controls.Add(txtParam, 3, 0)
        tlyBar.Controls.Add(btnRepair, 4, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.ScaleAbsoluteStyles = False
        tlyBar.Size = New Size(1334, 66)
        tlyBar.TabIndex = 0
        ' 
        ' CommandsView
        ' 
        AutoScaleMode = AutoScaleMode.None
        Controls.Add(tlyMain)
        Margin = New Padding(4)
        Name = "CommandsView"
        Size = New Size(1350, 840)
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents cmbCommand As KBotComboBox
    Friend WithEvents btnRun As KBotButton
    Friend WithEvents btnCopy As KBotButton
    Friend WithEvents btnRepair As KBotButton
    Friend WithEvents txtParam As KBotTextBox
    Friend WithEvents txtOutput As KBotTextBox
    Friend WithEvents txtExplain As KBotTextBox
    Friend WithEvents lblState As Label
End Class
