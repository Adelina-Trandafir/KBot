Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ClasificatiiAddForm
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
        btnAdauga = New Button()
        pnlCard = New Panel()
        tlyCorp = New KBotTableLayoutPanel()
        lblPasSurse = New Label()
        lblHintSurse = New Label()
        flowSurse = New FlowLayoutPanel()
        lblPasF = New Label()
        lblPasE = New Label()
        lblHintF = New Label()
        lblHintE = New Label()
        treeF = New AdvancedTreeControl()
        treeE = New AdvancedTreeControl()
        lblCountF = New Label()
        lblCountE = New Label()
        pnlJos = New Panel()
        lblStare = New Label()
        btnRenunta = New Button()
        capBar = New KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlyCorp.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnAdauga
        ' 
        btnAdauga.Dock = DockStyle.Right
        btnAdauga.Enabled = False
        btnAdauga.FlatStyle = FlatStyle.Flat
        btnAdauga.Location = New Point(1160, 12)
        btnAdauga.Margin = New Padding(0)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Size = New Size(255, 60)
        btnAdauga.TabIndex = 1
        btnAdauga.Text = "Adaugă clasificațiile"
        tips.SetToolTipHeader(btnAdauga, "Adaugă clasificațiile")
        tips.SetToolTipText(btnAdauga, "Se adaugă câte o clasificație pentru fiecare sursă × funcțională × economică bifată." & vbLf & "Cele care există deja se sar.")
        btnAdauga.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlyCorp)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(1616, 1076)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' tlyCorp
        ' 
        tlyCorp.ColumnCount = 2
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyCorp.Controls.Add(lblPasSurse, 0, 0)
        tlyCorp.Controls.Add(lblHintSurse, 0, 1)
        tlyCorp.Controls.Add(flowSurse, 0, 2)
        tlyCorp.Controls.Add(lblPasF, 0, 3)
        tlyCorp.Controls.Add(lblPasE, 1, 3)
        tlyCorp.Controls.Add(lblHintF, 0, 4)
        tlyCorp.Controls.Add(lblHintE, 1, 4)
        tlyCorp.Controls.Add(treeF, 0, 5)
        tlyCorp.Controls.Add(treeE, 1, 5)
        tlyCorp.Controls.Add(lblCountF, 0, 6)
        tlyCorp.Controls.Add(lblCountE, 1, 6)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 66)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(14, 8, 14, 4)
        tlyCorp.RowCount = 7
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 45F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 39F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 54F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 39F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyCorp.Size = New Size(1616, 926)
        tlyCorp.TabIndex = 1
        ' 
        ' lblPasSurse
        ' 
        tlyCorp.SetColumnSpan(lblPasSurse, 2)
        lblPasSurse.Dock = DockStyle.Fill
        lblPasSurse.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        lblPasSurse.Location = New Point(14, 8)
        lblPasSurse.Margin = New Padding(0)
        lblPasSurse.Name = "lblPasSurse"
        lblPasSurse.Size = New Size(1588, 45)
        lblPasSurse.TabIndex = 0
        lblPasSurse.Text = "1. Sursa și sectorul"
        lblPasSurse.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblHintSurse
        ' 
        tlyCorp.SetColumnSpan(lblHintSurse, 2)
        lblHintSurse.Dock = DockStyle.Fill
        lblHintSurse.Location = New Point(14, 53)
        lblHintSurse.Margin = New Padding(0)
        lblHintSurse.Name = "lblHintSurse"
        lblHintSurse.Size = New Size(1588, 39)
        lblHintSurse.TabIndex = 1
        lblHintSurse.Text = "Bifați sursele-sector pentru care se adaugă clasificațiile. Se pot alege doar sursele unității."
        lblHintSurse.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' flowSurse
        ' 
        tlyCorp.SetColumnSpan(flowSurse, 2)
        flowSurse.Dock = DockStyle.Fill
        flowSurse.Location = New Point(14, 92)
        flowSurse.Margin = New Padding(0)
        flowSurse.Name = "flowSurse"
        flowSurse.Size = New Size(1588, 60)
        flowSurse.TabIndex = 2
        flowSurse.WrapContents = False
        ' 
        ' lblPasF
        ' 
        lblPasF.Dock = DockStyle.Fill
        lblPasF.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        lblPasF.Location = New Point(14, 152)
        lblPasF.Margin = New Padding(0)
        lblPasF.Name = "lblPasF"
        lblPasF.Size = New Size(794, 54)
        lblPasF.TabIndex = 3
        lblPasF.Text = "2. Clasificația funcțională"
        lblPasF.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' lblPasE
        ' 
        lblPasE.Dock = DockStyle.Fill
        lblPasE.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        lblPasE.Location = New Point(808, 152)
        lblPasE.Margin = New Padding(0)
        lblPasE.Name = "lblPasE"
        lblPasE.Size = New Size(794, 54)
        lblPasE.TabIndex = 4
        lblPasE.Text = "3. Clasificația economică"
        lblPasE.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' lblHintF
        ' 
        lblHintF.Dock = DockStyle.Fill
        lblHintF.Location = New Point(14, 206)
        lblHintF.Margin = New Padding(0)
        lblHintF.Name = "lblHintF"
        lblHintF.Size = New Size(794, 39)
        lblHintF.TabIndex = 5
        lblHintF.Text = "Se bifează pozițiile de pe ultimul nivel."
        lblHintF.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblHintE
        ' 
        lblHintE.Dock = DockStyle.Fill
        lblHintE.Location = New Point(808, 206)
        lblHintE.Margin = New Padding(0)
        lblHintE.Name = "lblHintE"
        lblHintE.Size = New Size(794, 39)
        lblHintE.TabIndex = 6
        lblHintE.Text = "Bifarea unui articol bifează tot ce este sub el."
        lblHintE.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' treeF
        ' 
        treeF.CheckBoxes = True
        treeF.Dock = DockStyle.Fill
        treeF.Location = New Point(14, 245)
        treeF.Margin = New Padding(0, 0, 12, 0)
        treeF.Name = "treeF"
        treeF.SearchDefaultText = "… caută în clasificația funcțională …"
        treeF.SearchShow = True
        treeF.Size = New Size(782, 635)
        treeF.TabIndex = 7
        ' 
        ' treeE
        ' 
        treeE.CheckBoxes = True
        treeE.Dock = DockStyle.Fill
        treeE.Location = New Point(820, 245)
        treeE.Margin = New Padding(12, 0, 0, 0)
        treeE.Name = "treeE"
        treeE.SearchDefaultText = "… caută în clasificația economică …"
        treeE.SearchShow = True
        treeE.Size = New Size(782, 635)
        treeE.TabIndex = 8
        ' 
        ' lblCountF
        ' 
        lblCountF.Dock = DockStyle.Fill
        lblCountF.Location = New Point(14, 880)
        lblCountF.Margin = New Padding(0)
        lblCountF.Name = "lblCountF"
        lblCountF.Size = New Size(794, 42)
        lblCountF.TabIndex = 9
        lblCountF.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCountE
        ' 
        lblCountE.Dock = DockStyle.Fill
        lblCountE.Location = New Point(808, 880)
        lblCountE.Margin = New Padding(0)
        lblCountE.Name = "lblCountE"
        lblCountE.Size = New Size(794, 42)
        lblCountE.TabIndex = 10
        lblCountE.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlJos
        ' 
        pnlJos.Controls.Add(lblStare)
        pnlJos.Controls.Add(btnAdauga)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 992)
        pnlJos.Margin = New Padding(0)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(21, 12, 21, 12)
        pnlJos.Size = New Size(1616, 84)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        ' 
        ' lblStare
        ' 
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(21, 12)
        lblStare.Margin = New Padding(0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(1139, 60)
        lblStare.TabIndex = 0
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnRenunta
        ' 
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Right
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(1415, 12)
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(180, 60)
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.plus_green
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(1616, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Adaugă clasificații"
        ' 
        ' ClasificatiiAddForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(1620, 1080)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 4, 4, 4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "ClasificatiiAddForm"
        Padding = New Padding(2, 2, 2, 2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "Adaugă clasificații"
        pnlCard.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyCorp As KBotTableLayoutPanel
    Friend WithEvents lblPasSurse As Label
    Friend WithEvents lblHintSurse As Label
    Friend WithEvents flowSurse As FlowLayoutPanel
    Friend WithEvents lblPasF As Label
    Friend WithEvents lblPasE As Label
    Friend WithEvents lblHintF As Label
    Friend WithEvents lblHintE As Label
    Friend WithEvents treeF As AdvancedTreeControl
    Friend WithEvents treeE As AdvancedTreeControl
    Friend WithEvents lblCountF As Label
    Friend WithEvents lblCountE As Label
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnAdauga As Button
    Friend WithEvents btnRenunta As Button
    Friend WithEvents capBar As KBotCaptionBar
End Class
