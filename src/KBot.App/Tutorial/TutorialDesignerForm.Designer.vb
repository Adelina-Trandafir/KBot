Imports KBot.Controls

' Slice 000T-03 -- the tutorial designer (operator tool, shown only in capture mode).
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md). Coordonatele sunt la 96 dpi,
' cu AutoScaleDimensions (96, 96) in AutoScaleMode.Dpi (felia 0066-02).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class TutorialDesignerForm
    Inherits Global.KBot.Theming.KBotThemedForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(TutorialDesignerForm))
        tips = New KBotToolTip(components)
        lstSteps = New ListBox()
        btnAdd = New Button()
        btnDel = New Button()
        btnUp = New Button()
        btnDown = New Button()
        btnDup = New Button()
        cmbFlows = New ComboBox()
        txtText = New TextBox()
        btnPick = New Button()
        btnInregistreaza = New Button()
        btnSalveaza = New Button()
        btnTesteaza = New Button()
        pnlCard = New Panel()
        tlpBody = New KBotTableLayoutPanel()
        pnlLeft = New Panel()
        tlpStepButtons = New KBotTableLayoutPanel()
        pgFlow = New PropertyGrid()
        pnlRight = New Panel()
        pgStep = New PropertyGrid()
        lblText = New Label()
        tlpButtons = New KBotTableLayoutPanel()
        btnInchide = New Button()
        capBar = New KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlpBody.SuspendLayout()
        pnlLeft.SuspendLayout()
        tlpStepButtons.SuspendLayout()
        pnlRight.SuspendLayout()
        tlpButtons.SuspendLayout()
        SuspendLayout()
        ' 
        ' lstSteps
        ' 
        lstSteps.Dock = DockStyle.Fill
        lstSteps.Font = New Font("Calibri", 10F)
        lstSteps.FormattingEnabled = True
        lstSteps.IntegralHeight = False
        lstSteps.Location = New Point(0, 480)
        lstSteps.Margin = New Padding(4, 4, 4, 4)
        lstSteps.Name = "lstSteps"
        lstSteps.Size = New Size(498, 353)
        lstSteps.TabIndex = 2
        tips.SetToolTipHeader(lstSteps, "Pașii tutorialului")
        tips.SetToolTipText(lstSteps, "Alege un pas ca să-l editezi în dreapta. «(opțional)» = se poate sări peste el.")
        ' 
        ' btnAdd
        ' 
        btnAdd.Dock = DockStyle.Fill
        btnAdd.FlatStyle = FlatStyle.Flat
        btnAdd.Location = New Point(0, 6)
        btnAdd.Margin = New Padding(0, 6, 4, 0)
        btnAdd.Name = "btnAdd"
        btnAdd.Size = New Size(115, 45)
        btnAdd.TabIndex = 0
        btnAdd.Text = "+ Pas"
        tips.SetToolTipHeader(btnAdd, "Pas nou")
        tips.SetToolTipText(btnAdd, "Adaugă un pas după cel ales.")
        btnAdd.UseVisualStyleBackColor = True
        ' 
        ' btnDel
        ' 
        btnDel.Dock = DockStyle.Fill
        btnDel.FlatStyle = FlatStyle.Flat
        btnDel.Location = New Point(119, 6)
        btnDel.Margin = New Padding(0, 6, 4, 0)
        btnDel.Name = "btnDel"
        btnDel.Size = New Size(115, 45)
        btnDel.TabIndex = 1
        btnDel.Text = "Șterge"
        tips.SetToolTipHeader(btnDel, "Șterge pasul")
        tips.SetToolTipText(btnDel, "Scoate pasul ales din tutorial.")
        btnDel.UseVisualStyleBackColor = True
        ' 
        ' btnUp
        ' 
        btnUp.Dock = DockStyle.Fill
        btnUp.FlatStyle = FlatStyle.Flat
        btnUp.Location = New Point(238, 6)
        btnUp.Margin = New Padding(0, 6, 4, 0)
        btnUp.Name = "btnUp"
        btnUp.Size = New Size(65, 45)
        btnUp.TabIndex = 2
        btnUp.Text = "▲"
        tips.SetToolTipHeader(btnUp, "Mai sus")
        tips.SetToolTipText(btnUp, "Mută pasul înainte cu o poziție.")
        btnUp.UseVisualStyleBackColor = True
        ' 
        ' btnDown
        ' 
        btnDown.Dock = DockStyle.Fill
        btnDown.FlatStyle = FlatStyle.Flat
        btnDown.Location = New Point(307, 6)
        btnDown.Margin = New Padding(0, 6, 4, 0)
        btnDown.Name = "btnDown"
        btnDown.Size = New Size(65, 45)
        btnDown.TabIndex = 3
        btnDown.Text = "▼"
        tips.SetToolTipHeader(btnDown, "Mai jos")
        tips.SetToolTipText(btnDown, "Mută pasul după următorul.")
        btnDown.UseVisualStyleBackColor = True
        ' 
        ' btnDup
        ' 
        btnDup.Dock = DockStyle.Fill
        btnDup.FlatStyle = FlatStyle.Flat
        btnDup.Location = New Point(376, 6)
        btnDup.Margin = New Padding(0, 6, 0, 0)
        btnDup.Name = "btnDup"
        btnDup.Size = New Size(122, 45)
        btnDup.TabIndex = 4
        btnDup.Text = "Copie"
        tips.SetToolTipHeader(btnDup, "Copiază pasul")
        tips.SetToolTipText(btnDup, "Pune o copie a pasului chiar după el.")
        btnDup.UseVisualStyleBackColor = True
        ' 
        ' cmbFlows
        ' 
        cmbFlows.Dock = DockStyle.Top
        cmbFlows.DropDownStyle = ComboBoxStyle.DropDownList
        cmbFlows.FormattingEnabled = True
        cmbFlows.Location = New Point(0, 0)
        cmbFlows.Margin = New Padding(4, 4, 4, 4)
        cmbFlows.Name = "cmbFlows"
        cmbFlows.Size = New Size(498, 30)
        cmbFlows.TabIndex = 0
        tips.SetToolTipHeader(cmbFlows, "Tutorialul")
        tips.SetToolTipText(cmbFlows, "Alege un tutorial salvat ca să-l modifici, sau «(tutorial nou)».")
        ' 
        ' txtText
        ' 
        txtText.AcceptsReturn = True
        txtText.Dock = DockStyle.Bottom
        txtText.Font = New Font("Calibri", 11F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtText.Location = New Point(0, 691)
        txtText.Margin = New Padding(4, 4, 4, 4)
        txtText.Multiline = True
        txtText.Name = "txtText"
        txtText.ScrollBars = ScrollBars.Vertical
        txtText.Size = New Size(962, 193)
        txtText.TabIndex = 2
        tips.SetToolTipHeader(txtText, "Textul din bulă")
        tips.SetToolTipText(txtText, "Ce citește utilizatorul la acest pas. Un rând gol începe un paragraf; rândurile care încep cu «- » devin liste.")
        ' 
        ' btnPick
        ' 
        btnPick.Dock = DockStyle.Top
        btnPick.FlatStyle = FlatStyle.Flat
        btnPick.Font = New Font("Segoe UI Semibold", 9F)
        btnPick.Location = New Point(0, 0)
        btnPick.Margin = New Padding(4, 4, 4, 4)
        btnPick.Name = "btnPick"
        btnPick.Size = New Size(962, 54)
        btnPick.TabIndex = 0
        btnPick.Text = "Alege pe ecran..."
        tips.SetToolTipHeader(btnPick, "Alege pe ecran")
        tips.SetToolTipText(btnPick, "Ascunde fereastra aceasta; treci cu mouse-ul peste K-BOT, apare un cadru pe ce găsește, iar un clic îl trece în pas (țintă, piesă, ce așteaptă). Esc sau clic dreapta renunță.")
        btnPick.UseVisualStyleBackColor = True
        ' 
        ' btnInregistreaza
        ' 
        btnInregistreaza.Dock = DockStyle.Fill
        btnInregistreaza.FlatStyle = FlatStyle.Flat
        btnInregistreaza.Location = New Point(572, 6)
        btnInregistreaza.Margin = New Padding(0, 6, 9, 0)
        btnInregistreaza.Name = "btnInregistreaza"
        btnInregistreaza.Size = New Size(216, 60)
        btnInregistreaza.TabIndex = 3
        btnInregistreaza.Text = "Înregistrează"
        tips.SetToolTipHeader(btnInregistreaza, "Înregistrează pașii")
        tips.SetToolTipText(btnInregistreaza, resources.GetString("btnInregistreaza.ToolTipText"))
        btnInregistreaza.UseVisualStyleBackColor = True
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Font = New Font("Segoe UI Semibold", 9F)
        btnSalveaza.Location = New Point(797, 6)
        btnSalveaza.Margin = New Padding(0, 6, 9, 0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New Size(216, 60)
        btnSalveaza.TabIndex = 0
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează tutorialul")
        tips.SetToolTipText(btnSalveaza, "Scrie fișierul în dosarul Help\tutorials (și în sursa din repo, când K-BOT rulează de acolo) și reîncarcă ajutorul.")
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' btnTesteaza
        ' 
        btnTesteaza.Dock = DockStyle.Fill
        btnTesteaza.FlatStyle = FlatStyle.Flat
        btnTesteaza.Location = New Point(1022, 6)
        btnTesteaza.Margin = New Padding(0, 6, 9, 0)
        btnTesteaza.Name = "btnTesteaza"
        btnTesteaza.Size = New Size(276, 60)
        btnTesteaza.TabIndex = 1
        btnTesteaza.Text = "Testează de la pasul"
        tips.SetToolTipHeader(btnTesteaza, "Testează de la pasul ales")
        tips.SetToolTipText(btnTesteaza, "Pornește tutorialul (așa cum e acum, nesalvat) de la pasul ales; fereastra aceasta se micșorează și revine la sfârșit.")
        btnTesteaza.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlpBody)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 4)
        pnlCard.Margin = New Padding(4, 4, 4, 4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(1496, 1042)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' tlpBody
        ' 
        tlpBody.ColumnCount = 2
        tlpBody.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 510F))
        tlpBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpBody.Controls.Add(pnlLeft, 0, 0)
        tlpBody.Controls.Add(pnlRight, 1, 0)
        tlpBody.Controls.Add(tlpButtons, 0, 1)
        tlpBody.Dock = DockStyle.Fill
        tlpBody.Location = New Point(0, 60)
        tlpBody.Margin = New Padding(4, 4, 4, 4)
        tlpBody.Name = "tlpBody"
        tlpBody.Padding = New Padding(12, 8, 12, 12)
        tlpBody.RowCount = 2
        tlpBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.Size = New Size(1496, 982)
        tlpBody.TabIndex = 0
        tlpBody.Tag = "Card"
        ' 
        ' pnlLeft
        ' 
        pnlLeft.Controls.Add(lstSteps)
        pnlLeft.Controls.Add(tlpStepButtons)
        pnlLeft.Controls.Add(pgFlow)
        pnlLeft.Controls.Add(cmbFlows)
        pnlLeft.Dock = DockStyle.Fill
        pnlLeft.Location = New Point(12, 8)
        pnlLeft.Margin = New Padding(0, 0, 12, 12)
        pnlLeft.Name = "pnlLeft"
        pnlLeft.Size = New Size(498, 884)
        pnlLeft.TabIndex = 0
        ' 
        ' tlpStepButtons
        ' 
        tlpStepButtons.ColumnCount = 5
        tlpStepButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 24F))
        tlpStepButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 24F))
        tlpStepButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 14F))
        tlpStepButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 14F))
        tlpStepButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 24F))
        tlpStepButtons.Controls.Add(btnAdd, 0, 0)
        tlpStepButtons.Controls.Add(btnDel, 1, 0)
        tlpStepButtons.Controls.Add(btnUp, 2, 0)
        tlpStepButtons.Controls.Add(btnDown, 3, 0)
        tlpStepButtons.Controls.Add(btnDup, 4, 0)
        tlpStepButtons.Dock = DockStyle.Bottom
        tlpStepButtons.Location = New Point(0, 833)
        tlpStepButtons.Margin = New Padding(4, 4, 4, 4)
        tlpStepButtons.Name = "tlpStepButtons"
        tlpStepButtons.RowCount = 1
        tlpStepButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpStepButtons.Size = New Size(498, 51)
        tlpStepButtons.TabIndex = 3
        tlpStepButtons.Tag = "Card"
        ' 
        ' pgFlow
        ' 
        pgFlow.Dock = DockStyle.Top
        pgFlow.Font = New Font("Calibri", 10F)
        pgFlow.HelpVisible = False
        pgFlow.Location = New Point(0, 30)
        pgFlow.Margin = New Padding(6, 6, 6, 6)
        pgFlow.Name = "pgFlow"
        pgFlow.Size = New Size(498, 450)
        pgFlow.TabIndex = 1
        pgFlow.ToolbarVisible = False
        ' 
        ' pnlRight
        ' 
        pnlRight.Controls.Add(pgStep)
        pnlRight.Controls.Add(lblText)
        pnlRight.Controls.Add(txtText)
        pnlRight.Controls.Add(btnPick)
        pnlRight.Dock = DockStyle.Fill
        pnlRight.Location = New Point(522, 8)
        pnlRight.Margin = New Padding(0, 0, 0, 12)
        pnlRight.Name = "pnlRight"
        pnlRight.Size = New Size(962, 884)
        pnlRight.TabIndex = 1
        ' 
        ' pgStep
        ' 
        pgStep.Dock = DockStyle.Fill
        pgStep.Font = New Font("Calibri", 10F)
        pgStep.Location = New Point(0, 54)
        pgStep.Margin = New Padding(6, 6, 6, 6)
        pgStep.Name = "pgStep"
        pgStep.Size = New Size(962, 604)
        pgStep.TabIndex = 1
        pgStep.ToolbarVisible = False
        ' 
        ' lblText
        ' 
        lblText.Dock = DockStyle.Bottom
        lblText.Location = New Point(0, 658)
        lblText.Margin = New Padding(4, 0, 4, 0)
        lblText.Name = "lblText"
        lblText.Size = New Size(962, 33)
        lblText.TabIndex = 3
        lblText.Text = "Textul din bulă"
        lblText.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' tlpButtons
        ' 
        tlpButtons.ColumnCount = 5
        tlpBody.SetColumnSpan(tlpButtons, 2)
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 225F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 225F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 285F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 165F))
        tlpButtons.Controls.Add(btnInregistreaza, 1, 0)
        tlpButtons.Controls.Add(btnSalveaza, 2, 0)
        tlpButtons.Controls.Add(btnTesteaza, 3, 0)
        tlpButtons.Controls.Add(btnInchide, 4, 0)
        tlpButtons.Dock = DockStyle.Fill
        tlpButtons.Location = New Point(12, 904)
        tlpButtons.Margin = New Padding(0)
        tlpButtons.Name = "tlpButtons"
        tlpButtons.RowCount = 1
        tlpButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpButtons.Size = New Size(1472, 66)
        tlpButtons.TabIndex = 2
        tlpButtons.Tag = "Card"
        ' 
        ' btnInchide
        ' 
        btnInchide.Dock = DockStyle.Fill
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Location = New Point(1307, 6)
        btnInchide.Margin = New Padding(0, 6, 0, 0)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New Size(165, 60)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4, 4, 4, 4)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(1496, 60)
        capBar.TabIndex = 1
        capBar.TabStop = False
        capBar.Text = "K-BOT — Designer tutoriale"
        ' 
        ' TutorialDesignerForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1500, 1050)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 4, 4, 4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "TutorialDesignerForm"
        Padding = New Padding(2, 4, 2, 4)
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Designer tutoriale"
        pnlCard.ResumeLayout(False)
        tlpBody.ResumeLayout(False)
        pnlLeft.ResumeLayout(False)
        tlpStepButtons.ResumeLayout(False)
        pnlRight.ResumeLayout(False)
        pnlRight.PerformLayout()
        tlpButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents tlpBody As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents pnlLeft As Panel
    Friend WithEvents cmbFlows As ComboBox
    Friend WithEvents pgFlow As PropertyGrid
    Friend WithEvents lstSteps As ListBox
    Friend WithEvents tlpStepButtons As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnAdd As Button
    Friend WithEvents btnDel As Button
    Friend WithEvents btnUp As Button
    Friend WithEvents btnDown As Button
    Friend WithEvents btnDup As Button
    Friend WithEvents pnlRight As Panel
    Friend WithEvents btnPick As Button
    Friend WithEvents pgStep As PropertyGrid
    Friend WithEvents lblText As Label
    Friend WithEvents txtText As TextBox
    Friend WithEvents tlpButtons As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnInregistreaza As Button
    Friend WithEvents btnSalveaza As Button
    Friend WithEvents btnTesteaza As Button
    Friend WithEvents btnInchide As Button
End Class
