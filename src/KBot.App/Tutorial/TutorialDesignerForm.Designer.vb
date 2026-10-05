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
        btnSterge = New Button()
        btnTesteaza = New Button()
        btnHtmlBold = New Button()
        btnHtmlItalic = New Button()
        btnHtmlUnderline = New Button()
        cmbHtmlColor = New ComboBox()
        btnHtmlMark = New Button()
        btnHtmlBreak = New Button()
        btnHtmlList = New Button()
        btnHtmlHeading = New Button()
        lblPreview = New KBotHtmlLabel()
        pnlCard = New Panel()
        tlpBody = New KBotTableLayoutPanel()
        pnlLeft = New Panel()
        tlpStepButtons = New KBotTableLayoutPanel()
        pgFlow = New PropertyGrid()
        pnlRight = New Panel()
        pgStep = New PropertyGrid()
        pnlHtml = New Panel()
        tlpHtmlEditor = New KBotTableLayoutPanel()
        tlpHtmlTools = New KBotTableLayoutPanel()
        lblText = New Label()
        tlpButtons = New KBotTableLayoutPanel()
        btnInchide = New Button()
        capBar = New KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlpBody.SuspendLayout()
        pnlLeft.SuspendLayout()
        tlpStepButtons.SuspendLayout()
        pnlRight.SuspendLayout()
        pnlHtml.SuspendLayout()
        tlpHtmlEditor.SuspendLayout()
        tlpHtmlTools.SuspendLayout()
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
        lstSteps.Margin = New Padding(4)
        lstSteps.Name = "lstSteps"
        lstSteps.Size = New Size(498, 353)
        lstSteps.TabIndex = 2
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
        btnDup.UseVisualStyleBackColor = True
        ' 
        ' cmbFlows
        ' 
        cmbFlows.Dock = DockStyle.Top
        cmbFlows.DropDownStyle = ComboBoxStyle.DropDownList
        cmbFlows.FormattingEnabled = True
        cmbFlows.Location = New Point(0, 0)
        cmbFlows.Margin = New Padding(4)
        cmbFlows.Name = "cmbFlows"
        cmbFlows.Size = New Size(498, 30)
        cmbFlows.TabIndex = 0
        ' 
        ' txtText
        ' 
        txtText.AcceptsReturn = True
        txtText.Dock = DockStyle.Fill
        txtText.Font = New Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtText.Location = New Point(0, 6)
        txtText.Margin = New Padding(0, 0, 6, 0)
        txtText.Multiline = True
        txtText.Name = "txtText"
        txtText.ScrollBars = ScrollBars.Vertical
        txtText.Size = New Size(475, 327)
        txtText.TabIndex = 0
        ' 
        ' btnPick
        ' 
        btnPick.Dock = DockStyle.Top
        btnPick.FlatStyle = FlatStyle.Flat
        btnPick.Font = New Font("Segoe UI Semibold", 9F)
        btnPick.Location = New Point(0, 0)
        btnPick.Margin = New Padding(4)
        btnPick.Name = "btnPick"
        btnPick.Size = New Size(962, 54)
        btnPick.TabIndex = 0
        btnPick.Text = "Alege pe ecran..."
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
        btnInregistreaza.UseVisualStyleBackColor = True
        ' 
        ' btnSterge
        ' 
        btnSterge.Dock = DockStyle.Fill
        btnSterge.FlatStyle = FlatStyle.Flat
        btnSterge.Location = New Point(572, 6)
        btnSterge.Margin = New Padding(0, 6, 9, 0)
        btnSterge.Name = "btnSterge"
        btnSterge.Size = New Size(216, 60)
        btnSterge.TabIndex = 4
        btnSterge.Text = "Șterge tutorialul"
        btnSterge.UseVisualStyleBackColor = True
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
        btnTesteaza.UseVisualStyleBackColor = True
        ' 
        ' btnHtmlBold
        ' 
        btnHtmlBold.Dock = DockStyle.Fill
        btnHtmlBold.FlatStyle = FlatStyle.Flat
        btnHtmlBold.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        btnHtmlBold.Location = New Point(0, 0)
        btnHtmlBold.Margin = New Padding(0, 0, 4, 0)
        btnHtmlBold.Name = "btnHtmlBold"
        btnHtmlBold.Size = New Size(56, 54)
        btnHtmlBold.TabIndex = 0
        btnHtmlBold.Text = "B"
        btnHtmlBold.UseVisualStyleBackColor = True
        ' 
        ' btnHtmlItalic
        ' 
        btnHtmlItalic.Dock = DockStyle.Fill
        btnHtmlItalic.FlatStyle = FlatStyle.Flat
        btnHtmlItalic.Font = New Font("Segoe UI", 9F, FontStyle.Italic)
        btnHtmlItalic.Location = New Point(60, 0)
        btnHtmlItalic.Margin = New Padding(0, 0, 4, 0)
        btnHtmlItalic.Name = "btnHtmlItalic"
        btnHtmlItalic.Size = New Size(56, 54)
        btnHtmlItalic.TabIndex = 1
        btnHtmlItalic.Text = "I"
        btnHtmlItalic.UseVisualStyleBackColor = True
        ' 
        ' btnHtmlUnderline
        ' 
        btnHtmlUnderline.Dock = DockStyle.Fill
        btnHtmlUnderline.FlatStyle = FlatStyle.Flat
        btnHtmlUnderline.Font = New Font("Segoe UI", 9F, FontStyle.Underline)
        btnHtmlUnderline.Location = New Point(120, 0)
        btnHtmlUnderline.Margin = New Padding(0, 0, 4, 0)
        btnHtmlUnderline.Name = "btnHtmlUnderline"
        btnHtmlUnderline.Size = New Size(56, 54)
        btnHtmlUnderline.TabIndex = 2
        btnHtmlUnderline.Text = "U"
        btnHtmlUnderline.UseVisualStyleBackColor = True
        ' 
        ' cmbHtmlColor
        ' 
        cmbHtmlColor.Dock = DockStyle.Fill
        cmbHtmlColor.DropDownStyle = ComboBoxStyle.DropDownList
        cmbHtmlColor.FormattingEnabled = True
        cmbHtmlColor.Items.AddRange(New Object() {"Culoare...", "Accent (din temă)", "Estompat (din temă)", "Avertisment (din temă)", "Eroare (din temă)", "Succes (din temă)", "Roșu", "Verde", "Albastru", "Portocaliu"})
        cmbHtmlColor.Location = New Point(180, 0)
        cmbHtmlColor.Margin = New Padding(0, 0, 4, 0)
        cmbHtmlColor.Name = "cmbHtmlColor"
        cmbHtmlColor.Size = New Size(296, 30)
        cmbHtmlColor.TabIndex = 3
        ' 
        ' btnHtmlMark
        ' 
        btnHtmlMark.Dock = DockStyle.Fill
        btnHtmlMark.FlatStyle = FlatStyle.Flat
        btnHtmlMark.Location = New Point(480, 0)
        btnHtmlMark.Margin = New Padding(0, 0, 4, 0)
        btnHtmlMark.Name = "btnHtmlMark"
        btnHtmlMark.Size = New Size(146, 54)
        btnHtmlMark.TabIndex = 4
        btnHtmlMark.Text = "Marcaj"
        btnHtmlMark.UseVisualStyleBackColor = True
        ' 
        ' btnHtmlBreak
        ' 
        btnHtmlBreak.Dock = DockStyle.Fill
        btnHtmlBreak.FlatStyle = FlatStyle.Flat
        btnHtmlBreak.Location = New Point(630, 0)
        btnHtmlBreak.Margin = New Padding(0, 0, 4, 0)
        btnHtmlBreak.Name = "btnHtmlBreak"
        btnHtmlBreak.Size = New Size(146, 54)
        btnHtmlBreak.TabIndex = 5
        btnHtmlBreak.Text = "↵ Rând"
        btnHtmlBreak.UseVisualStyleBackColor = True
        ' 
        ' btnHtmlList
        ' 
        btnHtmlList.Dock = DockStyle.Fill
        btnHtmlList.FlatStyle = FlatStyle.Flat
        btnHtmlList.Location = New Point(780, 0)
        btnHtmlList.Margin = New Padding(0, 0, 4, 0)
        btnHtmlList.Name = "btnHtmlList"
        btnHtmlList.Size = New Size(146, 54)
        btnHtmlList.TabIndex = 6
        btnHtmlList.Text = "• Listă"
        btnHtmlList.UseVisualStyleBackColor = True
        ' 
        ' btnHtmlHeading
        ' 
        btnHtmlHeading.Dock = DockStyle.Fill
        btnHtmlHeading.FlatStyle = FlatStyle.Flat
        btnHtmlHeading.Location = New Point(930, 0)
        btnHtmlHeading.Margin = New Padding(0, 0, 4, 0)
        btnHtmlHeading.Name = "btnHtmlHeading"
        btnHtmlHeading.Size = New Size(126, 54)
        btnHtmlHeading.TabIndex = 7
        btnHtmlHeading.Text = "Titlu"
        btnHtmlHeading.UseVisualStyleBackColor = True
        ' 
        ' lblPreview
        ' 
        lblPreview.Dock = DockStyle.Fill
        lblPreview.Font = New Font("Segoe UI", 10.5F)
        lblPreview.Location = New Point(487, 6)
        lblPreview.Margin = New Padding(6, 0, 0, 0)
        lblPreview.Name = "lblPreview"
        lblPreview.Padding = New Padding(12, 10, 12, 10)
        lblPreview.Size = New Size(475, 327)
        lblPreview.TabIndex = 1
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlpBody)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 4)
        pnlCard.Margin = New Padding(4)
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
        tlpBody.Margin = New Padding(4)
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
        tlpStepButtons.Margin = New Padding(4)
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
        pgFlow.Margin = New Padding(6)
        pgFlow.Name = "pgFlow"
        pgFlow.Size = New Size(498, 450)
        pgFlow.TabIndex = 1
        pgFlow.ToolbarVisible = False
        ' 
        ' pnlRight
        ' 
        pnlRight.Controls.Add(pgStep)
        pnlRight.Controls.Add(pnlHtml)
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
        pgStep.Margin = New Padding(6)
        pgStep.Name = "pgStep"
        pgStep.Size = New Size(962, 410)
        pgStep.TabIndex = 1
        pgStep.ToolbarVisible = False
        ' 
        ' pnlHtml
        ' 
        pnlHtml.Controls.Add(tlpHtmlEditor)
        pnlHtml.Controls.Add(tlpHtmlTools)
        pnlHtml.Controls.Add(lblText)
        pnlHtml.Dock = DockStyle.Bottom
        pnlHtml.Location = New Point(0, 464)
        pnlHtml.Margin = New Padding(0)
        pnlHtml.Name = "pnlHtml"
        pnlHtml.Size = New Size(962, 420)
        pnlHtml.TabIndex = 2
        ' 
        ' tlpHtmlEditor
        ' 
        tlpHtmlEditor.ColumnCount = 2
        tlpHtmlEditor.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlpHtmlEditor.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlpHtmlEditor.Controls.Add(txtText, 0, 0)
        tlpHtmlEditor.Controls.Add(lblPreview, 1, 0)
        tlpHtmlEditor.Dock = DockStyle.Fill
        tlpHtmlEditor.Location = New Point(0, 87)
        tlpHtmlEditor.Margin = New Padding(0)
        tlpHtmlEditor.Name = "tlpHtmlEditor"
        tlpHtmlEditor.Padding = New Padding(0, 6, 0, 0)
        tlpHtmlEditor.RowCount = 1
        tlpHtmlEditor.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpHtmlEditor.Size = New Size(962, 333)
        tlpHtmlEditor.TabIndex = 2
        tlpHtmlEditor.Tag = "Card"
        ' 
        ' tlpHtmlTools
        ' 
        tlpHtmlTools.ColumnCount = 9
        tlpHtmlTools.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60F))
        tlpHtmlTools.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60F))
        tlpHtmlTools.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60F))
        tlpHtmlTools.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 300F))
        tlpHtmlTools.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpHtmlTools.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpHtmlTools.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpHtmlTools.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlpHtmlTools.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpHtmlTools.Controls.Add(btnHtmlBold, 0, 0)
        tlpHtmlTools.Controls.Add(btnHtmlItalic, 1, 0)
        tlpHtmlTools.Controls.Add(btnHtmlUnderline, 2, 0)
        tlpHtmlTools.Controls.Add(cmbHtmlColor, 3, 0)
        tlpHtmlTools.Controls.Add(btnHtmlMark, 4, 0)
        tlpHtmlTools.Controls.Add(btnHtmlBreak, 5, 0)
        tlpHtmlTools.Controls.Add(btnHtmlList, 6, 0)
        tlpHtmlTools.Controls.Add(btnHtmlHeading, 7, 0)
        tlpHtmlTools.Dock = DockStyle.Top
        tlpHtmlTools.Location = New Point(0, 33)
        tlpHtmlTools.Margin = New Padding(0)
        tlpHtmlTools.Name = "tlpHtmlTools"
        tlpHtmlTools.RowCount = 1
        tlpHtmlTools.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpHtmlTools.Size = New Size(962, 54)
        tlpHtmlTools.TabIndex = 1
        tlpHtmlTools.Tag = "Card"
        ' 
        ' lblText
        ' 
        lblText.Dock = DockStyle.Top
        lblText.Location = New Point(0, 0)
        lblText.Margin = New Padding(4, 0, 4, 0)
        lblText.Name = "lblText"
        lblText.Size = New Size(962, 33)
        lblText.TabIndex = 0
        lblText.Text = "Textul din bulă — stânga: ce scrii (text simplu sau HTML) · dreapta: cum se vede"
        lblText.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' tlpButtons
        ' 
        tlpButtons.ColumnCount = 6
        tlpBody.SetColumnSpan(tlpButtons, 2)
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 225F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 225F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 225F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 285F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 165F))
        tlpButtons.Controls.Add(btnSterge, 1, 0)
        tlpButtons.Controls.Add(btnInregistreaza, 2, 0)
        tlpButtons.Controls.Add(btnSalveaza, 3, 0)
        tlpButtons.Controls.Add(btnTesteaza, 4, 0)
        tlpButtons.Controls.Add(btnInchide, 5, 0)
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
        capBar.Margin = New Padding(4)
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
        Margin = New Padding(4)
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
        pnlHtml.ResumeLayout(False)
        tlpHtmlEditor.ResumeLayout(False)
        tlpHtmlEditor.PerformLayout()
        tlpHtmlTools.ResumeLayout(False)
        tlpButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

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
    Friend WithEvents pnlHtml As Panel
    Friend WithEvents tlpHtmlTools As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnHtmlBold As Button
    Friend WithEvents btnHtmlItalic As Button
    Friend WithEvents btnHtmlUnderline As Button
    Friend WithEvents cmbHtmlColor As ComboBox
    Friend WithEvents btnHtmlMark As Button
    Friend WithEvents btnHtmlBreak As Button
    Friend WithEvents btnHtmlList As Button
    Friend WithEvents btnHtmlHeading As Button
    Friend WithEvents tlpHtmlEditor As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblPreview As Global.KBot.Controls.KBotHtmlLabel
    Friend WithEvents tlpButtons As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnInregistreaza As Button
    Friend WithEvents btnSalveaza As Button
    Friend WithEvents btnSterge As Button
    Friend WithEvents btnTesteaza As Button
    Friend WithEvents btnInchide As Button
End Class
