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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(DemoCloneView))
        tips = New KBotToolTip(components)
        btnPlan = New KBotButton()
        txtConfirm = New KBotTextBox()
        btnClone = New KBotButton()
        btnCopy = New KBotButton()
        lblDesc = New Label()
        txtOutput = New KBotTextBox()
        lblState = New Label()
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnPlan
        ' 
        btnPlan.AccessibleRole = AccessibleRole.PushButton
        btnPlan.Dock = DockStyle.Left
        btnPlan.Location = New Point(8, 8)
        btnPlan.Margin = New Padding(0)
        btnPlan.Name = "btnPlan"
        btnPlan.Size = New Size(159, 50)
        btnPlan.TabIndex = 0
        btnPlan.Text = "Calculeaza"
        tips.SetToolTipHeader(btnPlan, "Previzualizare")
        tips.SetToolTipText(btnPlan, "Arată ce s-ar întâmpla cu baza aleasă în arbore, fără să scrie nimic: tabelele, numărul de rânduri, coloanele care se rescriu (DC și codul fiscal) și avertismentele.")
        ' 
        ' txtConfirm
        ' 
        txtConfirm.Dock = DockStyle.Fill
        txtConfirm.Location = New Point(233, 8)
        txtConfirm.Margin = New Padding(0)
        txtConfirm.MaxLength = 20
        txtConfirm.Name = "txtConfirm"
        txtConfirm.PlaceholderText = "Scrieți 000_DEMO ca să confirmați"
        txtConfirm.Size = New Size(390, 50)
        txtConfirm.TabIndex = 1
        tips.SetToolTipHeader(txtConfirm, "Confirmare")
        tips.SetToolTipText(txtConfirm, "Golirea lui 000_DEMO nu se poate anula decât din copia de siguranță. Clonarea merge doar după ce scrieți aici numele bazei care se golește.")
        ' 
        ' btnClone
        ' 
        btnClone.AccessibleRole = AccessibleRole.PushButton
        btnClone.Dock = DockStyle.Right
        btnClone.Location = New Point(1134, 8)
        btnClone.Margin = New Padding(0)
        btnClone.Name = "btnClone"
        btnClone.Primary = True
        btnClone.Size = New Size(192, 50)
        btnClone.TabIndex = 2
        btnClone.Text = "Golește și clonează"
        tips.SetToolTipHeader(btnClone, "Golește și clonează")
        tips.SetToolTipText(btnClone, resources.GetString("btnClone.ToolTipText"))
        ' 
        ' btnCopy
        ' 
        btnCopy.AccessibleRole = AccessibleRole.PushButton
        btnCopy.Dock = DockStyle.Left
        btnCopy.Location = New Point(623, 8)
        btnCopy.Margin = New Padding(0)
        btnCopy.Name = "btnCopy"
        btnCopy.Size = New Size(114, 50)
        btnCopy.TabIndex = 3
        btnCopy.Text = "Copiază"
        tips.SetToolTipHeader(btnCopy, "Copiază")
        tips.SetToolTipText(btnCopy, "Pune rezultatul în clipboard.")
        ' 
        ' lblDesc
        ' 
        lblDesc.Dock = DockStyle.Fill
        lblDesc.Location = New Point(8, 74)
        lblDesc.Margin = New Padding(0)
        lblDesc.Name = "lblDesc"
        lblDesc.Size = New Size(1334, 78)
        lblDesc.TabIndex = 1
        lblDesc.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblDesc, "Ce se întâmplă")
        tips.SetToolTipText(lblDesc, "Sursa este baza aleasă în arborele din stânga; destinația este mereu 000_DEMO.")
        ' 
        ' txtOutput
        ' 
        txtOutput.CornerRadius = 0
        txtOutput.Dock = DockStyle.Fill
        txtOutput.Font = New Font("Consolas", 9.75F)
        txtOutput.Location = New Point(8, 152)
        txtOutput.Margin = New Padding(0)
        txtOutput.MaxLength = 1000000
        txtOutput.Name = "txtOutput"
        txtOutput.ReadOnly = True
        txtOutput.ScrollBars = ScrollBars.Both
        txtOutput.Size = New Size(1334, 638)
        txtOutput.TabIndex = 2
        tips.SetToolTipHeader(txtOutput, "Rezultat")
        tips.SetToolTipText(txtOutput, "Planul sau rezultatul clonării: tabelele, rândurile, coloanele rescrise.")
        txtOutput.WordWrap = False
        ' 
        ' lblState
        ' 
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 790)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(1334, 42)
        lblState.TabIndex = 3
        lblState.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Ce s-a rulat ultima dată și cât a durat.")
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
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 66F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 78F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyMain.Size = New Size(1350, 840)
        tlyMain.TabIndex = 0
        ' 
        ' tlyBar
        ' 
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 5
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 225F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 390F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 300F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 180F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(btnPlan, 0, 0)
        tlyBar.Controls.Add(txtConfirm, 1, 0)
        tlyBar.Controls.Add(btnClone, 4, 0)
        tlyBar.Controls.Add(btnCopy, 2, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.Padding = New Padding(8)
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(1334, 66)
        tlyBar.TabIndex = 0
        ' 
        ' DemoCloneView
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Margin = New Padding(4)
        Name = "DemoCloneView"
        Size = New Size(1350, 840)
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
