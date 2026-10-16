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
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        cmbCommand = New KBotComboBox()
        btnRun = New KBotButton()
        btnCopy = New KBotButton()
        txtParam = New KBotTextBox()
        lblDesc = New Label()
        txtOutput = New KBotTextBox()
        lblState = New Label()
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
        tlyMain.Controls.Add(lblDesc, 0, 1)
        tlyMain.Controls.Add(txtOutput, 0, 2)
        tlyMain.Controls.Add(lblState, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 4
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 320F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(cmbCommand, 0, 0)
        tlyBar.Controls.Add(btnRun, 1, 0)
        tlyBar.Controls.Add(btnCopy, 2, 0)
        tlyBar.Controls.Add(txtParam, 3, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' cmbCommand
        '
        cmbCommand.Dock = DockStyle.Fill
        cmbCommand.Location = New Point(0, 0)
        cmbCommand.Margin = New Padding(0, 4, 8, 8)
        cmbCommand.Name = "cmbCommand"
        cmbCommand.Size = New Size(312, 32)
        cmbCommand.TabIndex = 0
        tips.SetToolTipHeader(cmbCommand, "Comanda")
        tips.SetToolTipText(cmbCommand, "Lista este fixă, hotărâtă pe server: nu se poate rula alt SQL de aici.")
        '
        ' btnRun
        '
        btnRun.Dock = DockStyle.Fill
        btnRun.Location = New Point(320, 0)
        btnRun.Margin = New Padding(0, 2, 6, 6)
        btnRun.Name = "btnRun"
        btnRun.Primary = True
        btnRun.Size = New Size(134, 36)
        btnRun.TabIndex = 1
        btnRun.Text = "Rulează"
        tips.SetToolTipHeader(btnRun, "Rulează")
        tips.SetToolTipText(btnRun, "Rulează comanda pe baza aleasă în arbore, sau pe toate dacă e aleasă <TOATE BAZELE>." & vbLf & "Comenzile care scriu cer confirmare.")
        '
        ' btnCopy
        '
        btnCopy.Dock = DockStyle.Fill
        btnCopy.Location = New Point(460, 0)
        btnCopy.Margin = New Padding(0, 2, 6, 6)
        btnCopy.Name = "btnCopy"
        btnCopy.Size = New Size(124, 36)
        btnCopy.TabIndex = 2
        btnCopy.Text = "Copiază"
        tips.SetToolTipHeader(btnCopy, "Copiază")
        tips.SetToolTipText(btnCopy, "Pune rezultatul în clipboard.")
        '
        ' txtParam (shown only for a command that needs a typed value, e.g. the code of an angajament)
        '
        txtParam.Dock = DockStyle.Left
        txtParam.Location = New Point(590, 0)
        txtParam.Margin = New Padding(0, 4, 8, 8)
        txtParam.MaxLength = 60
        txtParam.Name = "txtParam"
        txtParam.PlaceholderText = "Cod angajament"
        txtParam.Size = New Size(240, 32)
        txtParam.TabIndex = 3
        tips.SetToolTipHeader(txtParam, "Valoarea comenzii")
        tips.SetToolTipText(txtParam, "Codul angajamentului. Apare doar la comenzile care au nevoie de el și dispare când alegi altă comandă." & vbLf & "Comanda cere o bază anume aleasă în arbore, nu <TOATE BAZELE>.")
        txtParam.Visible = False
        '
        ' lblDesc
        '
        tips.SetToolTipHeader(lblDesc, "Ce face comanda")
        tips.SetToolTipText(lblDesc, "Descrierea comenzii alese și dacă scrie ceva în baza de date.")
        lblDesc.AutoEllipsis = True
        lblDesc.Dock = DockStyle.Fill
        lblDesc.Location = New Point(8, 52)
        lblDesc.Margin = New Padding(0)
        lblDesc.Name = "lblDesc"
        lblDesc.Size = New Size(884, 28)
        lblDesc.TabIndex = 1
        lblDesc.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtOutput
        '
        txtOutput.CornerRadius = 0
        txtOutput.Dock = DockStyle.Fill
        txtOutput.Font = New Font("Consolas", 9.75F)
        txtOutput.Location = New Point(8, 80)
        txtOutput.Margin = New Padding(0)
        txtOutput.MaxLength = 1000000
        txtOutput.Name = "txtOutput"
        txtOutput.ReadOnly = True
        txtOutput.ScrollBars = ScrollBars.Both
        txtOutput.Size = New Size(884, 444)
        txtOutput.TabIndex = 2
        tips.SetToolTipHeader(txtOutput, "Rezultat")
        tips.SetToolTipText(txtOutput, "Rezultatul comenzii, câte un bloc pe fiecare bază.")
        txtOutput.WordWrap = False
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Pe ce baze s-a rulat și cât a durat.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 524)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 28)
        lblState.TabIndex = 3
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' CommandsView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "CommandsView"
        Size = New Size(900, 560)
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
    Friend WithEvents txtParam As KBotTextBox
    Friend WithEvents lblDesc As Label
    Friend WithEvents txtOutput As KBotTextBox
    Friend WithEvents lblState As Label
End Class
