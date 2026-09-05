Imports KBot.Controls

' Formularul de EDITARE a ordonantarii (felia 0049) — portul lui `frmFX_ORD`.
'
' DE CE E UN FORMULAR SEPARAT, nu o pagina a lui `OrdView` (D1): Access deschidea `frmFX_ORD`
' ca popup, cu semantica de salveaza/renunta. `OrdView` ramane read-only si primeste doar
' punctele de intrare; editarea are propriul formular, modal peste `MainForm`.
'
' CELE TREI POPUP-URI ALE ACCESS-ULUI AU DEVENIT TREI PAGINI (D2), in spatele unui
' `KBotNavList` orizontal — aceeasi forma pe care o folosesc deja `OrdView` si `DdfView`:
'   «Beneficiari»              = frmFX_ORD_PART + frmFX_ORD_TBL
'   «Documente justificative»  = frmFX_ORD_DOC + _BENE + _TXT + _ATT
'   «Atasamente»               = frmFX_ORD_PRTSCR + _BENE + _S
' Consecinta: `btnSav` de pe frmFX_ORD_DOC dispare — exista O SINGURA salvare, aici.
'
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md).
' Coordonatele sunt scrise la 144 dpi -- fisierul a fost salvat din designer pe un ecran la
' 150%, iar Visual Studio rescrie ATUNCI si coordonatele, si perechea. AutoScaleDimensions
' le insoteste: Calibri 9 se masoara (9, 22) acolo (felia 0052). Cele doua se schimba
' INTOTDEAUNA impreuna; o pereche luata de la alt font sau de la alt dpi turteste fereastra
' la deschidere, fara ca nimic din designer s-o arate.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OrdEditForm
    Inherits Global.KBot.Theming.KBotShellForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(OrdEditForm))
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        dtpData = New KBotDatePicker()
        lblNrOrd = New Label()
        btnSalveaza = New Button()
        btnRenunta = New Button()
        tlyMain = New TableLayoutPanel()
        capBar = New KBotCaptionBar()
        busyBar = New KBotBusyBar()
        tlyAntet = New TableLayoutPanel()
        lblCodCaption = New Label()
        lblCod = New Label()
        lblNrOrdCaption = New Label()
        lblDataCaption = New Label()
        lblTotalCaption = New Label()
        lblTotal = New Label()
        lblObiectCaption = New Label()
        lblObiect = New Label()
        ntfMesaj = New KBotNotice()
        navSub = New KBotNavList()
        pnlPages = New Panel()
        tlySubsol = New TableLayoutPanel()
        tlyMain.SuspendLayout()
        tlyAntet.SuspendLayout()
        CType(navSub, ComponentModel.ISupportInitialize).BeginInit()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        ' 
        ' dtpData
        ' 
        dtpData.ButtonPadding = New Padding(0, 0, 4, 0)
        dtpData.ButtonWidth = 16
        dtpData.Dock = DockStyle.Fill
        dtpData.FocusBorderColor = SystemColors.Highlight
        dtpData.ForeColor = SystemColors.ActiveCaptionText
        dtpData.GlyphColor = SystemColors.ActiveCaptionText
        dtpData.GlyphSize = 16
        dtpData.Location = New Point(694, 4)
        dtpData.Margin = New Padding(4, 4, 4, 10)
        dtpData.Name = "dtpData"
        dtpData.Size = New Size(181, 26)
        dtpData.TabIndex = 5
        dtpData.TabStop = False
        tips.SetToolTipHeader(dtpData, "Data ordonanțării")
        tips.SetToolTipText(dtpData, "Data care se scrie în document." & vbLf & "Plățile propuse rămân cele ale zilei pentru care s-a generat ordonanțarea.")
        ' 
        ' lblNrOrd
        ' 
        lblNrOrd.AutoSize = True
        lblNrOrd.Cursor = Cursors.Hand
        lblNrOrd.Dock = DockStyle.Fill
        lblNrOrd.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblNrOrd.Location = New Point(424, 0)
        lblNrOrd.Margin = New Padding(4, 0, 4, 0)
        lblNrOrd.Name = "lblNrOrd"
        lblNrOrd.Size = New Size(142, 40)
        lblNrOrd.TabIndex = 3
        lblNrOrd.Text = "se alocă la salvare"
        lblNrOrd.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblNrOrd, "Numărul ordonanțării")
        tips.SetToolTipText(lblNrOrd, resources.GetString("lblNrOrd.ToolTipText"))
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.Location = New Point(1012, 4)
        btnSalveaza.Margin = New Padding(0, 4, 10, 4)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(14, 7, 14, 7)
        btnSalveaza.Size = New Size(276, 50)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează ordonanțarea"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Trimite tot documentul într-o singură tranzacție." & vbLf & "Imaginile atașate se încarcă imediat după, când rândurile lor au chei.")
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' btnRenunta
        ' 
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Location = New Point(10, 4)
        btnRenunta.Margin = New Padding(10, 4, 0, 4)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New Padding(14, 7, 14, 7)
        btnRenunta.Size = New Size(219, 50)
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fără să salveze nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(busyBar, 0, 1)
        tlyMain.Controls.Add(tlyAntet, 0, 2)
        tlyMain.Controls.Add(ntfMesaj, 0, 4)
        tlyMain.Controls.Add(navSub, 0, 4)
        tlyMain.Controls.Add(pnlPages, 0, 5)
        tlyMain.Controls.Add(tlySubsol, 0, 6)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 2)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 8
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 57F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 7F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 97F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 7F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 39F))
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.Size = New Size(1298, 996)
        tlyMain.TabIndex = 0
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowTextScaleSlider = False
        capBar.ShowThemeEditor = False
        capBar.ShowThemeOptions = False
        capBar.Size = New Size(1298, 57)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Ordonanțare de plată"
        ' 
        ' busyBar
        ' 
        busyBar.Dock = DockStyle.Fill
        busyBar.Location = New Point(0, 57)
        busyBar.Margin = New Padding(0)
        busyBar.Name = "busyBar"
        busyBar.Size = New Size(1298, 7)
        busyBar.TabIndex = 1
        busyBar.TabStop = False
        ' 
        ' tlyAntet
        ' 
        tlyAntet.ColumnCount = 8
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 189F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 118F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyAntet.Controls.Add(lblCodCaption, 0, 0)
        tlyAntet.Controls.Add(lblCod, 1, 0)
        tlyAntet.Controls.Add(lblNrOrdCaption, 2, 0)
        tlyAntet.Controls.Add(lblNrOrd, 3, 0)
        tlyAntet.Controls.Add(lblDataCaption, 4, 0)
        tlyAntet.Controls.Add(dtpData, 5, 0)
        tlyAntet.Controls.Add(lblTotalCaption, 6, 0)
        tlyAntet.Controls.Add(lblTotal, 7, 0)
        tlyAntet.Controls.Add(lblObiectCaption, 0, 1)
        tlyAntet.Controls.Add(lblObiect, 1, 1)
        tlyAntet.Dock = DockStyle.Fill
        tlyAntet.Location = New Point(4, 69)
        tlyAntet.Margin = New Padding(4, 5, 4, 5)
        tlyAntet.Name = "tlyAntet"
        tlyAntet.RowCount = 3
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyAntet.Size = New Size(1290, 87)
        tlyAntet.TabIndex = 2
        ' 
        ' lblCodCaption
        ' 
        lblCodCaption.AutoSize = True
        lblCodCaption.Dock = DockStyle.Fill
        lblCodCaption.Font = New Font("Calibri", 9F)
        lblCodCaption.Location = New Point(4, 0)
        lblCodCaption.Margin = New Padding(4, 0, 4, 0)
        lblCodCaption.Name = "lblCodCaption"
        lblCodCaption.Size = New Size(142, 40)
        lblCodCaption.TabIndex = 0
        lblCodCaption.Text = "Cod angajament"
        lblCodCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCod
        ' 
        lblCod.AutoSize = True
        lblCod.Dock = DockStyle.Fill
        lblCod.Font = New Font("Calibri", 12F, FontStyle.Bold)
        lblCod.Location = New Point(154, 0)
        lblCod.Margin = New Padding(4, 0, 4, 0)
        lblCod.Name = "lblCod"
        lblCod.Size = New Size(142, 40)
        lblCod.TabIndex = 1
        lblCod.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblNrOrdCaption
        ' 
        lblNrOrdCaption.AutoSize = True
        lblNrOrdCaption.Dock = DockStyle.Fill
        lblNrOrdCaption.Font = New Font("Calibri", 9F)
        lblNrOrdCaption.Location = New Point(304, 0)
        lblNrOrdCaption.Margin = New Padding(4, 0, 4, 0)
        lblNrOrdCaption.Name = "lblNrOrdCaption"
        lblNrOrdCaption.Size = New Size(112, 40)
        lblNrOrdCaption.TabIndex = 2
        lblNrOrdCaption.Text = "Număr"
        lblNrOrdCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataCaption
        ' 
        lblDataCaption.AutoSize = True
        lblDataCaption.Dock = DockStyle.Fill
        lblDataCaption.Font = New Font("Calibri", 9F)
        lblDataCaption.Location = New Point(574, 0)
        lblDataCaption.Margin = New Padding(4, 0, 4, 0)
        lblDataCaption.Name = "lblDataCaption"
        lblDataCaption.Size = New Size(112, 40)
        lblDataCaption.TabIndex = 4
        lblDataCaption.Text = "Data"
        lblDataCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblTotalCaption
        ' 
        lblTotalCaption.AutoSize = True
        lblTotalCaption.Dock = DockStyle.Fill
        lblTotalCaption.Font = New Font("Calibri", 9F)
        lblTotalCaption.Location = New Point(883, 0)
        lblTotalCaption.Margin = New Padding(4, 0, 4, 0)
        lblTotalCaption.Name = "lblTotalCaption"
        lblTotalCaption.Size = New Size(110, 40)
        lblTotalCaption.TabIndex = 6
        lblTotalCaption.Text = "Total"
        lblTotalCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblTotal
        ' 
        lblTotal.AutoSize = True
        lblTotal.Dock = DockStyle.Fill
        lblTotal.Font = New Font("Calibri", 12F, FontStyle.Bold)
        lblTotal.Location = New Point(1001, 0)
        lblTotal.Margin = New Padding(4, 0, 4, 0)
        lblTotal.Name = "lblTotal"
        lblTotal.Size = New Size(285, 40)
        lblTotal.TabIndex = 7
        lblTotal.Text = "0,00"
        lblTotal.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblObiectCaption
        ' 
        lblObiectCaption.AutoSize = True
        lblObiectCaption.Dock = DockStyle.Fill
        lblObiectCaption.Font = New Font("Calibri", 9F)
        lblObiectCaption.Location = New Point(4, 40)
        lblObiectCaption.Margin = New Padding(4, 0, 4, 0)
        lblObiectCaption.Name = "lblObiectCaption"
        lblObiectCaption.Size = New Size(142, 40)
        lblObiectCaption.TabIndex = 8
        lblObiectCaption.Text = "Obiect DDF"
        lblObiectCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblObiect
        ' 
        lblObiect.AutoEllipsis = True
        tlyAntet.SetColumnSpan(lblObiect, 7)
        lblObiect.Dock = DockStyle.Fill
        lblObiect.Font = New Font("Calibri", 12F, FontStyle.Bold)
        lblObiect.Location = New Point(154, 40)
        lblObiect.Margin = New Padding(4, 0, 4, 0)
        lblObiect.Name = "lblObiect"
        lblObiect.Size = New Size(1132, 40)
        lblObiect.TabIndex = 9
        lblObiect.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ntfMesaj
        ' 
        ntfMesaj.BackColor = Color.Transparent
        ntfMesaj.Dock = DockStyle.Fill
        ntfMesaj.Location = New Point(4, 212)
        ntfMesaj.Margin = New Padding(4, 5, 4, 5)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New Size(1290, 10)
        ntfMesaj.TabIndex = 3
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        ' 
        ' navSub
        ' 
        navSub.Dock = DockStyle.Fill
        navSub.IconSize = 16
        navSub.ItemCornerRadius = 0
        navSub.ItemPadding = New Padding(3, 0, 3, 0)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Image = My.Resources.Resources.vertical
        KBotNavItem1.Key = "beneficiari"
        KBotNavItem1.Text = "Beneficiari"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Image = My.Resources.Resources.binvoice
        KBotNavItem2.Key = "documente"
        KBotNavItem2.Text = "Documente justificative"
        KBotNavItem3.AutoSize = True
        KBotNavItem3.Image = My.Resources.Resources.cells
        KBotNavItem3.Key = "atasamente"
        KBotNavItem3.Text = "Atașamente"
        navSub.Items.Add(KBotNavItem1)
        navSub.Items.Add(KBotNavItem2)
        navSub.Items.Add(KBotNavItem3)
        navSub.Location = New Point(0, 168)
        navSub.Margin = New Padding(0)
        navSub.Name = "navSub"
        navSub.Orientation = KBotNavOrientation.Horizontal
        navSub.SelectedKey = Nothing
        navSub.Size = New Size(1298, 39)
        navSub.TabIndex = 4
        ' 
        ' pnlPages
        ' 
        pnlPages.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlPages.Dock = DockStyle.Fill
        pnlPages.Location = New Point(0, 227)
        pnlPages.Margin = New Padding(0)
        pnlPages.Name = "pnlPages"
        pnlPages.Size = New Size(1298, 711)
        pnlPages.TabIndex = 5
        ' 
        ' tlySubsol
        ' 
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 229F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 286F))
        tlySubsol.Controls.Add(btnRenunta, 0, 0)
        tlySubsol.Controls.Add(btnSalveaza, 2, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 938)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(1298, 58)
        tlySubsol.TabIndex = 6
        ' 
        ' OrdEditForm
        ' 
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1641, 1000)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 5, 4, 5)
        MaximizeBox = False
        MinimizeBox = False
        Name = "OrdEditForm"
        Padding = New Padding(1, 2, 1, 2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Ordonanțare de plată"
        tlyMain.ResumeLayout(False)
        tlyAntet.ResumeLayout(False)
        tlyAntet.PerformLayout()
        CType(navSub, ComponentModel.ISupportInitialize).EndInit()
        tlySubsol.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents tlyMain As TableLayoutPanel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents busyBar As Global.KBot.Controls.KBotBusyBar
    Friend WithEvents tlyAntet As TableLayoutPanel
    Friend WithEvents lblCodCaption As Label
    Friend WithEvents lblCod As Label
    Friend WithEvents lblNrOrdCaption As Label
    Friend WithEvents lblNrOrd As Label
    Friend WithEvents lblDataCaption As Label
    Friend WithEvents dtpData As KBotDatePicker
    Friend WithEvents lblTotalCaption As Label
    Friend WithEvents lblTotal As Label
    Friend WithEvents lblObiectCaption As Label
    Friend WithEvents lblObiect As Label
    Friend WithEvents navSub As Global.KBot.Controls.KBotNavList
    Friend WithEvents tlySubsol As TableLayoutPanel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnSalveaza As Button
    Friend WithEvents ntfMesaj As KBotNotice
    Friend WithEvents pnlPages As Panel
End Class
