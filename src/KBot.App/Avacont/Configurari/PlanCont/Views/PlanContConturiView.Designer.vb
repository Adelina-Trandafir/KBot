Imports KBot.Controls

' «Conturi» view of the «Plan de conturi» window (slice AV01-04): the tree of accounts and the fields of the selected one.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PlanContConturiView
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
        tlyRoot = New KBotTableLayoutPanel()
        tlyCont = New KBotTableLayoutPanel()
        tree = New AdvancedTreeControl()
        tlyDetalii = New KBotTableLayoutPanel()
        lblCont = New Label()
        cboCont = New KBotComboBox()
        lblSector = New Label()
        cboSector = New KBotComboBox()
        lblSursa = New Label()
        cboSursa = New KBotComboBox()
        chkClsfF = New CheckBox()
        cboClsfF = New KBotComboBox()
        chkClsfE = New CheckBox()
        cboClsfE = New KBotComboBox()
        lblAnalitic = New Label()
        txtAnalitic = New KBotTextField()
        lblFel = New Label()
        cboFel = New KBotComboBox()
        lblDenumire = New Label()
        txtDenumire = New KBotTextBox()
        lblSoldD = New Label()
        txtSoldD = New KBotTextField()
        lblSoldC = New Label()
        txtSoldC = New KBotTextField()
        lblRulajD = New Label()
        txtRulajD = New KBotTextField()
        lblRulajC = New Label()
        txtRulajC = New KBotTextField()
        tlyButoane = New KBotTableLayoutPanel()
        lblSimbol = New Label()
        btnSterge = New Button()
        btnRenunta = New Button()
        btnModifica = New Button()
        btnAdauga = New Button()
        btnSalveaza = New Button()
        tlyRoot.SuspendLayout()
        tlyCont.SuspendLayout()
        tlyDetalii.SuspendLayout()
        tlyButoane.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlyRoot
        ' 
        tlyRoot.ColumnCount = 1
        tlyRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRoot.Controls.Add(tlyCont, 0, 0)
        tlyRoot.Controls.Add(tlyButoane, 0, 1)
        tlyRoot.Dock = DockStyle.Fill
        tlyRoot.Location = New Point(0, 0)
        tlyRoot.Margin = New Padding(0)
        tlyRoot.Name = "tlyRoot"
        tlyRoot.RowCount = 2
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 78F))
        tlyRoot.Size = New Size(1412, 760)
        tlyRoot.TabIndex = 0
        ' 
        ' tlyCont
        ' 
        tlyCont.ColumnCount = 2
        tlyCont.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 55F))
        tlyCont.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 45F))
        tlyCont.Controls.Add(tree, 0, 0)
        tlyCont.Controls.Add(tlyDetalii, 1, 0)
        tlyCont.Dock = DockStyle.Fill
        tlyCont.Location = New Point(0, 0)
        tlyCont.Margin = New Padding(0)
        tlyCont.Name = "tlyCont"
        tlyCont.RowCount = 1
        tlyCont.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCont.Size = New Size(1412, 682)
        tlyCont.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.BackColor = SystemColors.Window
        tree.BorderColor = SystemColors.ActiveBorder
        tree.Dock = DockStyle.Fill
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderCaption = " CONTURI"
        tree.HeaderHeight = 24
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderRightIcon = My.Resources.Resources.settings__1_
        tree.HeaderRightIconTooltip = "Opțiunile arborelui:" & vbLf & "arată și conturile inactive."
        tree.HeaderVisible = True
        tree.Indent = 14
        tree.LeftIconSize = New Size(16, 16)
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(0, 0, 15, 0)
        tree.Name = "tree"
        tree.RightTextColumn = 240
        tree.SearchBackColor = SystemColors.Control
        tree.SearchDefaultText = "… tastați o parte din simbol sau din denumire …"
        tree.SearchSeparatorColor = SystemColors.ActiveBorder
        tree.SearchSeparatorWidth = 2
        tree.SearchShow = True
        tree.Size = New Size(762, 682)
        tree.TabIndex = 0
        ' 
        ' tlyDetalii
        ' 
        tlyDetalii.ColumnCount = 2
        tlyDetalii.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 270F))
        tlyDetalii.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyDetalii.Controls.Add(lblCont, 0, 0)
        tlyDetalii.Controls.Add(cboCont, 1, 0)
        tlyDetalii.Controls.Add(lblSector, 0, 1)
        tlyDetalii.Controls.Add(cboSector, 1, 1)
        tlyDetalii.Controls.Add(lblSursa, 0, 2)
        tlyDetalii.Controls.Add(cboSursa, 1, 2)
        tlyDetalii.Controls.Add(chkClsfF, 0, 3)
        tlyDetalii.Controls.Add(cboClsfF, 1, 3)
        tlyDetalii.Controls.Add(chkClsfE, 0, 4)
        tlyDetalii.Controls.Add(cboClsfE, 1, 4)
        tlyDetalii.Controls.Add(lblAnalitic, 0, 5)
        tlyDetalii.Controls.Add(txtAnalitic, 1, 5)
        tlyDetalii.Controls.Add(lblFel, 0, 6)
        tlyDetalii.Controls.Add(cboFel, 1, 6)
        tlyDetalii.Controls.Add(lblDenumire, 0, 7)
        tlyDetalii.Controls.Add(txtDenumire, 0, 8)
        tlyDetalii.Controls.Add(lblSoldD, 0, 9)
        tlyDetalii.Controls.Add(txtSoldD, 1, 9)
        tlyDetalii.Controls.Add(lblSoldC, 0, 10)
        tlyDetalii.Controls.Add(txtSoldC, 1, 10)
        tlyDetalii.Controls.Add(lblRulajD, 0, 11)
        tlyDetalii.Controls.Add(txtRulajD, 1, 11)
        tlyDetalii.Controls.Add(lblRulajC, 0, 12)
        tlyDetalii.Controls.Add(txtRulajC, 1, 12)
        tlyDetalii.Dock = DockStyle.Fill
        tlyDetalii.Location = New Point(777, 0)
        tlyDetalii.Margin = New Padding(0)
        tlyDetalii.Name = "tlyDetalii"
        tlyDetalii.RowCount = 13
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 32F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 46F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 46F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 46F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 46F))
        tlyDetalii.Size = New Size(635, 682)
        tlyDetalii.TabIndex = 1
        ' 
        ' lblCont
        ' 
        lblCont.Dock = DockStyle.Fill
        lblCont.Margin = New Padding(0)
        lblCont.Name = "lblCont"
        lblCont.TabIndex = 0
        lblCont.Text = "Cont de bază"
        lblCont.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboCont
        ' 
        cboCont.Dock = DockStyle.Fill
        cboCont.Editable = True
        cboCont.FindAsYouType = True
        cboCont.LimitToList = True
        cboCont.Margin = New Padding(0, 4, 0, 4)
        cboCont.Name = "cboCont"
        cboCont.TabIndex = 1
        tips.SetToolTipHeader(cboCont, "Cont de bază")
        tips.SetToolTipText(cboCont, "Contul din nomenclatorul comun (7 cifre). El spune dacă contul poate avea clasificație funcțională și / sau economică.")
        ' 
        ' lblSector
        ' 
        lblSector.Dock = DockStyle.Fill
        lblSector.Margin = New Padding(0)
        lblSector.Name = "lblSector"
        lblSector.TabIndex = 2
        lblSector.Text = "Sector"
        lblSector.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboSector
        '
        cboSector.Dock = DockStyle.Left
        cboSector.Editable = True
        cboSector.FindAsYouType = True
        cboSector.LimitToList = True
        cboSector.Margin = New Padding(0, 4, 0, 4)
        cboSector.Name = "cboSector"
        cboSector.Size = New Size(150, 42)
        cboSector.TabIndex = 3
        tips.SetToolTipHeader(cboSector, "Sector")
        tips.SetToolTipText(cboSector, "Urmează alegerea din bara de titlu. Doar când bara de titlu este pe «Toate» îl alegeți aici.")
        ' 
        ' lblSursa
        ' 
        lblSursa.Dock = DockStyle.Fill
        lblSursa.Margin = New Padding(0)
        lblSursa.Name = "lblSursa"
        lblSursa.TabIndex = 4
        lblSursa.Text = "Sursă"
        lblSursa.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboSursa
        '
        cboSursa.Dock = DockStyle.Left
        cboSursa.Editable = True
        cboSursa.FindAsYouType = True
        cboSursa.LimitToList = True
        cboSursa.Margin = New Padding(0, 4, 0, 4)
        cboSursa.Name = "cboSursa"
        cboSursa.Size = New Size(150, 42)
        cboSursa.TabIndex = 5
        tips.SetToolTipHeader(cboSursa, "Sursă")
        tips.SetToolTipText(cboSursa, "Urmează alegerea din bara de titlu. Doar când bara de titlu este pe «Toate» o alegeți aici.")
        ' 
        ' chkClsfF
        ' 
        chkClsfF.Dock = DockStyle.Fill
        chkClsfF.FlatStyle = FlatStyle.Flat
        chkClsfF.Margin = New Padding(0)
        chkClsfF.Name = "chkClsfF"
        chkClsfF.TabIndex = 6
        chkClsfF.Text = "Clsf. funcțională"
        tips.SetToolTipText(chkClsfF, "Bifați ca să dați contului o clasificație funcțională (opțională); apoi o alegeți în câmpul alăturat. Debifată, clasificația se șterge.")
        ' 
        ' cboClsfF
        ' 
        cboClsfF.Dock = DockStyle.Fill
        cboClsfF.Editable = True
        cboClsfF.FindAsYouType = True
        cboClsfF.LimitToList = True
        cboClsfF.Margin = New Padding(0, 4, 0, 4)
        cboClsfF.Name = "cboClsfF"
        cboClsfF.TabIndex = 7
        ' 
        ' chkClsfE
        ' 
        chkClsfE.Dock = DockStyle.Fill
        chkClsfE.FlatStyle = FlatStyle.Flat
        chkClsfE.Margin = New Padding(0)
        chkClsfE.Name = "chkClsfE"
        chkClsfE.TabIndex = 8
        chkClsfE.Text = "Clsf. economică"
        tips.SetToolTipText(chkClsfE, "Bifați ca să dați contului o clasificație economică (opțională; doar împreună cu cea funcțională); apoi o alegeți în câmpul alăturat. Debifată, clasificația se șterge.")
        ' 
        ' cboClsfE
        ' 
        cboClsfE.Dock = DockStyle.Fill
        cboClsfE.Editable = True
        cboClsfE.FindAsYouType = True
        cboClsfE.LimitToList = True
        cboClsfE.Margin = New Padding(0, 4, 0, 4)
        cboClsfE.Name = "cboClsfE"
        cboClsfE.TabIndex = 9
        ' 
        ' lblAnalitic
        ' 
        lblAnalitic.Dock = DockStyle.Fill
        lblAnalitic.Margin = New Padding(0)
        lblAnalitic.Name = "lblAnalitic"
        lblAnalitic.TabIndex = 10
        lblAnalitic.Text = "Analitic"
        lblAnalitic.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtAnalitic
        ' 
        txtAnalitic.BackColor = Color.Transparent
        txtAnalitic.Dock = DockStyle.Fill
        txtAnalitic.Margin = New Padding(0, 4, 0, 4)
        txtAnalitic.MaxLength = 10
        txtAnalitic.Name = "txtAnalitic"
        txtAnalitic.TabIndex = 11
        txtAnalitic.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtAnalitic, "Analitic")
        tips.SetToolTipText(txtAnalitic, "Cifre, litere mari și «_», cel mult 10 caractere. Gol = contul sintetic.")
        ' 
        ' lblFel
        ' 
        lblFel.Dock = DockStyle.Fill
        lblFel.Margin = New Padding(0)
        lblFel.Name = "lblFel"
        lblFel.TabIndex = 12
        lblFel.Text = "Fel cont"
        lblFel.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboFel
        ' 
        cboFel.Dock = DockStyle.Fill
        cboFel.Margin = New Padding(0, 4, 0, 4)
        cboFel.Name = "cboFel"
        cboFel.TabIndex = 13
        ' 
        ' lblDenumire
        ' 
        tlyDetalii.SetColumnSpan(lblDenumire, 2)
        lblDenumire.Dock = DockStyle.Fill
        lblDenumire.Margin = New Padding(0)
        lblDenumire.Name = "lblDenumire"
        lblDenumire.TabIndex = 14
        lblDenumire.Text = "Denumire cont"
        lblDenumire.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' txtDenumire
        ' 
        tlyDetalii.SetColumnSpan(txtDenumire, 2)
        txtDenumire.Dock = DockStyle.Fill
        txtDenumire.Margin = New Padding(0, 2, 0, 6)
        txtDenumire.MaxLength = 255
        txtDenumire.Multiline = True
        txtDenumire.Name = "txtDenumire"
        txtDenumire.TabIndex = 15
        ' 
        ' lblSoldD
        ' 
        lblSoldD.Dock = DockStyle.Fill
        lblSoldD.Margin = New Padding(0)
        lblSoldD.Name = "lblSoldD"
        lblSoldD.TabIndex = 16
        lblSoldD.Text = "Sold inițial debitor"
        lblSoldD.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtSoldD
        ' 
        txtSoldD.BackColor = Color.Transparent
        txtSoldD.Dock = DockStyle.Fill
        txtSoldD.Margin = New Padding(0, 3, 0, 3)
        txtSoldD.Name = "txtSoldD"
        txtSoldD.TabIndex = 17
        txtSoldD.TextAlign = HorizontalAlignment.Right
        txtSoldD.TextPadding = New Padding(8, 0, 8, 0)
        ' 
        ' lblSoldC
        ' 
        lblSoldC.Dock = DockStyle.Fill
        lblSoldC.Margin = New Padding(0)
        lblSoldC.Name = "lblSoldC"
        lblSoldC.TabIndex = 18
        lblSoldC.Text = "Sold inițial creditor"
        lblSoldC.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtSoldC
        ' 
        txtSoldC.BackColor = Color.Transparent
        txtSoldC.Dock = DockStyle.Fill
        txtSoldC.Margin = New Padding(0, 3, 0, 3)
        txtSoldC.Name = "txtSoldC"
        txtSoldC.TabIndex = 19
        txtSoldC.TextAlign = HorizontalAlignment.Right
        txtSoldC.TextPadding = New Padding(8, 0, 8, 0)
        ' 
        ' lblRulajD
        ' 
        lblRulajD.Dock = DockStyle.Fill
        lblRulajD.Margin = New Padding(0)
        lblRulajD.Name = "lblRulajD"
        lblRulajD.TabIndex = 20
        lblRulajD.Text = "Rulaj cumul. debitor"
        lblRulajD.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtRulajD
        ' 
        txtRulajD.BackColor = Color.Transparent
        txtRulajD.Dock = DockStyle.Fill
        txtRulajD.Margin = New Padding(0, 3, 0, 3)
        txtRulajD.Name = "txtRulajD"
        txtRulajD.TabIndex = 21
        txtRulajD.TextAlign = HorizontalAlignment.Right
        txtRulajD.TextPadding = New Padding(8, 0, 8, 0)
        ' 
        ' lblRulajC
        ' 
        lblRulajC.Dock = DockStyle.Fill
        lblRulajC.Margin = New Padding(0)
        lblRulajC.Name = "lblRulajC"
        lblRulajC.TabIndex = 22
        lblRulajC.Text = "Rulaj cumul. creditor"
        lblRulajC.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtRulajC
        ' 
        txtRulajC.BackColor = Color.Transparent
        txtRulajC.Dock = DockStyle.Fill
        txtRulajC.Margin = New Padding(0, 3, 0, 3)
        txtRulajC.Name = "txtRulajC"
        txtRulajC.TabIndex = 23
        txtRulajC.TextAlign = HorizontalAlignment.Right
        txtRulajC.TextPadding = New Padding(8, 0, 8, 0)
        ' 
        ' tlyButoane
        ' 
        tlyButoane.AutoFitToTheme = False
        tlyButoane.ColumnCount = 6
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.Controls.Add(lblSimbol, 0, 0)
        tlyButoane.Controls.Add(btnSterge, 1, 0)
        tlyButoane.Controls.Add(btnRenunta, 2, 0)
        tlyButoane.Controls.Add(btnModifica, 3, 0)
        tlyButoane.Controls.Add(btnAdauga, 4, 0)
        tlyButoane.Controls.Add(btnSalveaza, 5, 0)
        tlyButoane.Dock = DockStyle.Fill
        tlyButoane.Location = New Point(0, 682)
        tlyButoane.Margin = New Padding(0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.Padding = New Padding(8, 6, 8, 6)
        tlyButoane.RowCount = 1
        tlyButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButoane.Size = New Size(1412, 78)
        tlyButoane.TabIndex = 1
        ' 
        ' lblSimbol
        ' 
        lblSimbol.AutoEllipsis = True
        lblSimbol.Dock = DockStyle.Fill
        lblSimbol.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lblSimbol.Margin = New Padding(0, 0, 12, 0)
        lblSimbol.Name = "lblSimbol"
        lblSimbol.TabIndex = 0
        lblSimbol.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblSimbol, "Simbol cont")
        tips.SetToolTipText(lblSimbol, "Cont + sector-sursa + clasificații + analitic. Îl compune serverul la salvare; aici e doar previzualizarea.")
        ' 
        ' btnSterge
        ' 
        btnSterge.AutoSize = True
        btnSterge.Enabled = False
        btnSterge.FlatStyle = FlatStyle.Flat
        btnSterge.Margin = New Padding(0, 0, 12, 0)
        btnSterge.Name = "btnSterge"
        btnSterge.Padding = New Padding(26, 4, 26, 4)
        btnSterge.TabIndex = 1
        btnSterge.Text = "Ștergere"
        tips.SetToolTipText(btnSterge, "Șterge contul ales. Un cont cu operații, în șabloane sau cu sold inițial nu se șterge: îl marcați inactiv.")
        btnSterge.UseVisualStyleBackColor = True
        ' 
        ' btnRenunta
        ' 
        btnRenunta.AutoSize = True
        btnRenunta.Enabled = False
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Margin = New Padding(0, 0, 12, 0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New Padding(26, 4, 26, 4)
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        tips.SetToolTipText(btnRenunta, "Renunță la modificările nesalvate ale contului.")
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' btnModifica
        '
        btnModifica.AutoSize = True
        btnModifica.Enabled = False
        btnModifica.FlatStyle = FlatStyle.Flat
        btnModifica.Margin = New Padding(0, 0, 12, 0)
        btnModifica.Name = "btnModifica"
        btnModifica.Padding = New Padding(26, 4, 26, 4)
        btnModifica.TabIndex = 3
        btnModifica.Text = "Modificare"
        tips.SetToolTipText(btnModifica, "Câmpurile contului se pot schimba doar după ce apăsați «Modificare». Apoi «Salvare» sau «Renunță».")
        btnModifica.UseVisualStyleBackColor = True
        '
        ' btnAdauga
        '
        btnAdauga.AutoSize = True
        btnAdauga.FlatStyle = FlatStyle.Flat
        btnAdauga.Margin = New Padding(0, 0, 12, 0)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Padding = New Padding(26, 4, 26, 4)
        btnAdauga.TabIndex = 4
        btnAdauga.Text = "Adăugare"
        tips.SetToolTipText(btnAdauga, "Golește câmpurile pentru un cont nou în subunitatea aleasă.")
        btnAdauga.UseVisualStyleBackColor = True
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.AutoSize = True
        btnSalveaza.Enabled = False
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(26, 4, 26, 4)
        btnSalveaza.TabIndex = 5
        btnSalveaza.Text = "Salvare"
        tips.SetToolTipText(btnSalveaza, "Scrie contul, cu soldurile lui, în planul de conturi al subunității.")
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' PlanContConturiView
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyRoot)
        Margin = New Padding(0)
        Name = "PlanContConturiView"
        Size = New Size(1412, 760)
        tlyRoot.ResumeLayout(False)
        tlyCont.ResumeLayout(False)
        tlyDetalii.ResumeLayout(False)
        tlyDetalii.PerformLayout()
        tlyButoane.ResumeLayout(False)
        tlyButoane.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyRoot As KBotTableLayoutPanel
    Friend WithEvents tlyCont As KBotTableLayoutPanel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents tlyDetalii As KBotTableLayoutPanel
    Friend WithEvents lblCont As Label
    Friend WithEvents cboCont As KBotComboBox
    Friend WithEvents lblSector As Label
    Friend WithEvents cboSector As KBotComboBox
    Friend WithEvents lblSursa As Label
    Friend WithEvents cboSursa As KBotComboBox
    Friend WithEvents chkClsfF As CheckBox
    Friend WithEvents cboClsfF As KBotComboBox
    Friend WithEvents chkClsfE As CheckBox
    Friend WithEvents cboClsfE As KBotComboBox
    Friend WithEvents lblAnalitic As Label
    Friend WithEvents txtAnalitic As KBotTextField
    Friend WithEvents lblFel As Label
    Friend WithEvents cboFel As KBotComboBox
    Friend WithEvents lblDenumire As Label
    Friend WithEvents txtDenumire As KBotTextBox
    Friend WithEvents lblSoldD As Label
    Friend WithEvents txtSoldD As KBotTextField
    Friend WithEvents lblSoldC As Label
    Friend WithEvents txtSoldC As KBotTextField
    Friend WithEvents lblRulajD As Label
    Friend WithEvents txtRulajD As KBotTextField
    Friend WithEvents lblRulajC As Label
    Friend WithEvents txtRulajC As KBotTextField
    Friend WithEvents tlyButoane As KBotTableLayoutPanel
    Friend WithEvents lblSimbol As Label
    Friend WithEvents btnSterge As Button
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnModifica As Button
    Friend WithEvents btnAdauga As Button
    Friend WithEvents btnSalveaza As Button
End Class
