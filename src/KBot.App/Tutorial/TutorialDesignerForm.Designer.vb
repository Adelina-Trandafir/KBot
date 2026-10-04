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
        tips = New KBotToolTip(components)
        pnlCard = New Panel()
        tlpBody = New KBotTableLayoutPanel()
        pnlLeft = New Panel()
        lstSteps = New ListBox()
        tlpStepButtons = New KBotTableLayoutPanel()
        btnAdd = New Button()
        btnDel = New Button()
        btnUp = New Button()
        btnDown = New Button()
        btnDup = New Button()
        pgFlow = New PropertyGrid()
        cmbFlows = New ComboBox()
        pnlRight = New Panel()
        pgStep = New PropertyGrid()
        txtText = New TextBox()
        lblText = New Label()
        btnPick = New Button()
        tlpButtons = New KBotTableLayoutPanel()
        btnInregistreaza = New Button()
        btnSalveaza = New Button()
        btnTesteaza = New Button()
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
        ' pnlCard
        '
        pnlCard.Controls.Add(tlpBody)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 3)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(998, 694)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' tlpBody
        '
        tlpBody.ColumnCount = 2
        tlpBody.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 340F))
        tlpBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpBody.Controls.Add(pnlLeft, 0, 0)
        tlpBody.Controls.Add(pnlRight, 1, 0)
        tlpBody.Controls.Add(tlpButtons, 0, 1)
        tlpBody.SetColumnSpan(tlpButtons, 2)
        tlpBody.Dock = DockStyle.Fill
        tlpBody.Location = New Point(0, 40)
        tlpBody.Name = "tlpBody"
        tlpBody.Padding = New Padding(12, 8, 12, 12)
        tlpBody.RowCount = 2
        tlpBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.Size = New Size(998, 654)
        tlpBody.TabIndex = 0
        tlpBody.Tag = "Card"
        '
        ' pnlLeft  (Fill first, then Bottom, then the Tops)
        '
        pnlLeft.Controls.Add(lstSteps)
        pnlLeft.Controls.Add(tlpStepButtons)
        pnlLeft.Controls.Add(pgFlow)
        pnlLeft.Controls.Add(cmbFlows)
        pnlLeft.Dock = DockStyle.Fill
        pnlLeft.Margin = New Padding(0, 0, 8, 8)
        pnlLeft.Name = "pnlLeft"
        pnlLeft.TabIndex = 0
        '
        ' lstSteps
        '
        lstSteps.Dock = DockStyle.Fill
        lstSteps.FormattingEnabled = True
        lstSteps.IntegralHeight = False
        lstSteps.Name = "lstSteps"
        lstSteps.TabIndex = 2
        tips.SetToolTipHeader(lstSteps, "Pașii tutorialului")
        tips.SetToolTipText(lstSteps, "Alege un pas ca să-l editezi în dreapta. «(opțional)» = se poate sări peste el.")
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
        tlpStepButtons.Name = "tlpStepButtons"
        tlpStepButtons.RowCount = 1
        tlpStepButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpStepButtons.Size = New Size(332, 34)
        tlpStepButtons.TabIndex = 3
        tlpStepButtons.Tag = "Card"
        '
        ' btnAdd
        '
        btnAdd.Dock = DockStyle.Fill
        btnAdd.FlatStyle = FlatStyle.Flat
        btnAdd.Margin = New Padding(0, 4, 3, 0)
        btnAdd.Name = "btnAdd"
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
        btnDel.Margin = New Padding(0, 4, 3, 0)
        btnDel.Name = "btnDel"
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
        btnUp.Margin = New Padding(0, 4, 3, 0)
        btnUp.Name = "btnUp"
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
        btnDown.Margin = New Padding(0, 4, 3, 0)
        btnDown.Name = "btnDown"
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
        btnDup.Margin = New Padding(0, 4, 0, 0)
        btnDup.Name = "btnDup"
        btnDup.TabIndex = 4
        btnDup.Text = "Copie"
        tips.SetToolTipHeader(btnDup, "Copiază pasul")
        tips.SetToolTipText(btnDup, "Pune o copie a pasului chiar după el.")
        btnDup.UseVisualStyleBackColor = True
        '
        ' pgFlow
        '
        pgFlow.Dock = DockStyle.Top
        pgFlow.HelpVisible = False
        pgFlow.Name = "pgFlow"
        pgFlow.Size = New Size(332, 200)
        pgFlow.TabIndex = 1
        pgFlow.ToolbarVisible = False
        '
        ' cmbFlows
        '
        cmbFlows.Dock = DockStyle.Top
        cmbFlows.DropDownStyle = ComboBoxStyle.DropDownList
        cmbFlows.FormattingEnabled = True
        cmbFlows.Name = "cmbFlows"
        cmbFlows.TabIndex = 0
        tips.SetToolTipHeader(cmbFlows, "Tutorialul")
        tips.SetToolTipText(cmbFlows, "Alege un tutorial salvat ca să-l modifici, sau «(tutorial nou)».")
        '
        ' pnlRight  (Fill first, then Bottom, then Top)
        '
        pnlRight.Controls.Add(pgStep)
        pnlRight.Controls.Add(lblText)
        pnlRight.Controls.Add(txtText)
        pnlRight.Controls.Add(btnPick)
        pnlRight.Dock = DockStyle.Fill
        pnlRight.Margin = New Padding(0, 0, 0, 8)
        pnlRight.Name = "pnlRight"
        pnlRight.TabIndex = 1
        '
        ' pgStep
        '
        pgStep.Dock = DockStyle.Fill
        pgStep.Name = "pgStep"
        pgStep.TabIndex = 1
        pgStep.ToolbarVisible = False
        '
        ' txtText
        '
        txtText.AcceptsReturn = True
        txtText.Dock = DockStyle.Bottom
        txtText.Multiline = True
        txtText.Name = "txtText"
        txtText.ScrollBars = ScrollBars.Vertical
        txtText.Size = New Size(610, 130)
        txtText.TabIndex = 2
        tips.SetToolTipHeader(txtText, "Textul din bulă")
        tips.SetToolTipText(txtText, "Ce citește utilizatorul la acest pas. Un rând gol începe un paragraf; rândurile care încep cu «- » devin liste.")
        '
        ' lblText
        '
        lblText.Dock = DockStyle.Bottom
        lblText.Name = "lblText"
        lblText.Size = New Size(610, 22)
        lblText.TabIndex = 3
        lblText.Text = "Textul din bulă"
        lblText.TextAlign = ContentAlignment.BottomLeft
        '
        ' btnPick
        '
        btnPick.Dock = DockStyle.Top
        btnPick.FlatStyle = FlatStyle.Flat
        btnPick.Font = New Font("Segoe UI Semibold", 9.0F)
        btnPick.Name = "btnPick"
        btnPick.Size = New Size(610, 36)
        btnPick.TabIndex = 0
        btnPick.Text = "Alege pe ecran..."
        tips.SetToolTipHeader(btnPick, "Alege pe ecran")
        tips.SetToolTipText(btnPick, "Ascunde fereastra aceasta; treci cu mouse-ul peste K-BOT, apare un cadru pe ce găsește, iar un clic îl trece în pas (țintă, piesă, ce așteaptă). Esc sau clic dreapta renunță.")
        btnPick.UseVisualStyleBackColor = True
        '
        ' tlpButtons
        '
        tlpButtons.ColumnCount = 5
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlpButtons.Controls.Add(btnInregistreaza, 1, 0)
        tlpButtons.Controls.Add(btnSalveaza, 2, 0)
        tlpButtons.Controls.Add(btnTesteaza, 3, 0)
        tlpButtons.Controls.Add(btnInchide, 4, 0)
        tlpButtons.Dock = DockStyle.Fill
        tlpButtons.Margin = New Padding(0)
        tlpButtons.Name = "tlpButtons"
        tlpButtons.RowCount = 1
        tlpButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpButtons.Size = New Size(974, 44)
        tlpButtons.TabIndex = 2
        tlpButtons.Tag = "Card"
        '
        ' btnInregistreaza
        '
        btnInregistreaza.Dock = DockStyle.Fill
        btnInregistreaza.FlatStyle = FlatStyle.Flat
        btnInregistreaza.Margin = New Padding(0, 4, 6, 0)
        btnInregistreaza.Name = "btnInregistreaza"
        btnInregistreaza.TabIndex = 3
        btnInregistreaza.Text = "Înregistrează"
        tips.SetToolTipHeader(btnInregistreaza, "Înregistrează pașii")
        tips.SetToolTipText(btnInregistreaza, "Fereastra aceasta se micșorează; lucrezi în K-BOT ca de obicei (apeși, alegi, completezi, deschizi ferestre) și fiecare gest devine un pas. «Oprește» din bara mică aduce pașii aici, după pasul ales. Din ce scrii se păstrează doar că s-a schimbat ceva, nu valoarea.")
        btnInregistreaza.UseVisualStyleBackColor = True
        '
        ' btnSalveaza
        '
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Font = New Font("Segoe UI Semibold", 9.0F)
        btnSalveaza.Margin = New Padding(0, 4, 6, 0)
        btnSalveaza.Name = "btnSalveaza"
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
        btnTesteaza.Margin = New Padding(0, 4, 6, 0)
        btnTesteaza.Name = "btnTesteaza"
        btnTesteaza.TabIndex = 1
        btnTesteaza.Text = "Testează de la pasul"
        tips.SetToolTipHeader(btnTesteaza, "Testează de la pasul ales")
        tips.SetToolTipText(btnTesteaza, "Pornește tutorialul (așa cum e acum, nesalvat) de la pasul ales; fereastra aceasta se micșorează și revine la sfârșit.")
        btnTesteaza.UseVisualStyleBackColor = True
        '
        ' btnInchide
        '
        btnInchide.Dock = DockStyle.Fill
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Margin = New Padding(0, 4, 0, 0)
        btnInchide.Name = "btnInchide"
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(998, 40)
        capBar.TabIndex = 1
        capBar.TabStop = False
        capBar.Text = "K-BOT — Designer tutoriale"
        '
        ' TutorialDesignerForm
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1000, 700)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "TutorialDesignerForm"
        Padding = New Padding(1, 3, 1, 3)
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
