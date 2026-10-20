' The «Clonare DEMO» page (slice ME01-04): empties 000_DEMO and clones the database chosen in the tree into it.
' All controls are declared HERE; coordinates are in the 96 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DemoCloneView
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
        btnPlan = New KBotButton()
        txtConfirm = New KBotTextBox()
        btnClone = New KBotButton()
        btnCopy = New KBotButton()
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
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 5
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(btnPlan, 0, 0)
        tlyBar.Controls.Add(txtConfirm, 1, 0)
        tlyBar.Controls.Add(btnClone, 2, 0)
        tlyBar.Controls.Add(btnCopy, 3, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' btnPlan
        '
        btnPlan.Dock = DockStyle.Fill
        btnPlan.Location = New Point(0, 0)
        btnPlan.Margin = New Padding(0, 2, 6, 6)
        btnPlan.Name = "btnPlan"
        btnPlan.Size = New Size(144, 36)
        btnPlan.TabIndex = 0
        btnPlan.Text = "Previzualizare"
        tips.SetToolTipHeader(btnPlan, "Previzualizare")
        tips.SetToolTipText(btnPlan, "Arată ce s-ar întâmpla cu baza aleasă în arbore, fără să scrie nimic: tabelele, numărul de rânduri, coloanele care se rescriu (DC și codul fiscal) și avertismentele.")
        '
        ' txtConfirm
        '
        txtConfirm.Dock = DockStyle.Fill
        txtConfirm.Location = New Point(150, 0)
        txtConfirm.Margin = New Padding(0, 4, 8, 8)
        txtConfirm.MaxLength = 20
        txtConfirm.Name = "txtConfirm"
        txtConfirm.PlaceholderText = "Scrieți 000_DEMO ca să confirmați"
        txtConfirm.Size = New Size(252, 32)
        txtConfirm.TabIndex = 1
        tips.SetToolTipHeader(txtConfirm, "Confirmare")
        tips.SetToolTipText(txtConfirm, "Golirea lui 000_DEMO nu se poate anula decât din copia de siguranță. Clonarea merge doar după ce scrieți aici numele bazei care se golește.")
        '
        ' btnClone
        '
        btnClone.Dock = DockStyle.Fill
        btnClone.Location = New Point(410, 0)
        btnClone.Margin = New Padding(0, 2, 6, 6)
        btnClone.Name = "btnClone"
        btnClone.Primary = True
        btnClone.Size = New Size(194, 36)
        btnClone.TabIndex = 2
        btnClone.Text = "Golește și clonează"
        tips.SetToolTipHeader(btnClone, "Golește și clonează")
        tips.SetToolTipText(btnClone, "Face întâi o copie de siguranță a lui 000_DEMO, apoi îl golește și copiază în el baza aleasă în arbore." & vbLf & "DC devine 000_DEMO peste tot, iar codul fiscal al bazei copiate devine codul fiscal al lui 000_DEMO. Dacă ceva eșuează, 000_DEMO rămâne neschimbat.")
        '
        ' btnCopy
        '
        btnCopy.Dock = DockStyle.Fill
        btnCopy.Location = New Point(610, 0)
        btnCopy.Margin = New Padding(0, 2, 6, 6)
        btnCopy.Name = "btnCopy"
        btnCopy.Size = New Size(114, 36)
        btnCopy.TabIndex = 3
        btnCopy.Text = "Copiază"
        tips.SetToolTipHeader(btnCopy, "Copiază")
        tips.SetToolTipText(btnCopy, "Pune rezultatul în clipboard.")
        '
        ' lblDesc
        '
        tips.SetToolTipHeader(lblDesc, "Ce se întâmplă")
        tips.SetToolTipText(lblDesc, "Sursa este baza aleasă în arborele din stânga; destinația este mereu 000_DEMO.")
        lblDesc.AutoSize = False
        lblDesc.Dock = DockStyle.Fill
        lblDesc.Location = New Point(8, 52)
        lblDesc.Margin = New Padding(0)
        lblDesc.Name = "lblDesc"
        lblDesc.Size = New Size(884, 52)
        lblDesc.TabIndex = 1
        lblDesc.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtOutput
        '
        txtOutput.CornerRadius = 0
        txtOutput.Dock = DockStyle.Fill
        txtOutput.Font = New Font("Consolas", 9.75F)
        txtOutput.Location = New Point(8, 104)
        txtOutput.Margin = New Padding(0)
        txtOutput.MaxLength = 1000000
        txtOutput.Name = "txtOutput"
        txtOutput.ReadOnly = True
        txtOutput.ScrollBars = ScrollBars.Both
        txtOutput.Size = New Size(884, 420)
        txtOutput.TabIndex = 2
        tips.SetToolTipHeader(txtOutput, "Rezultat")
        tips.SetToolTipText(txtOutput, "Planul sau rezultatul clonării: tabelele, rândurile, coloanele rescrise.")
        txtOutput.WordWrap = False
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Ce s-a rulat ultima dată și cât a durat.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 524)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 28)
        lblState.TabIndex = 3
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' DemoCloneView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "DemoCloneView"
        Size = New Size(900, 560)
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents btnPlan As KBotButton
    Friend WithEvents txtConfirm As KBotTextBox
    Friend WithEvents btnClone As KBotButton
    Friend WithEvents btnCopy As KBotButton
    Friend WithEvents lblDesc As Label
    Friend WithEvents txtOutput As KBotTextBox
    Friend WithEvents lblState As Label
End Class
