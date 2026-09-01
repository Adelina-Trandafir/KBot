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
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OrdEditForm
    Inherits KBot.Theming.KBotShellForm

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
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        tlyMain = New TableLayoutPanel()
        capBar = New KBotCaptionBar()
        busyBar = New KBotBusyBar()
        tlyAntet = New TableLayoutPanel()
        lblCodCaption = New Label()
        lblCod = New Label()
        lblNrOrdCaption = New Label()
        lblNrOrd = New Label()
        lblDataCaption = New Label()
        dtpData = New DateTimePicker()
        lblTotalCaption = New Label()
        lblTotal = New Label()
        lblObiectCaption = New Label()
        lblObiect = New Label()
        ntfMesaj = New KBotNotice()
        navSub = New KBotNavList()
        pnlPages = New Panel()
        tlySubsol = New TableLayoutPanel()
        btnRenunta = New Button()
        btnSalveaza = New Button()
        tlyMain.SuspendLayout()
        tlyAntet.SuspendLayout()
        CType(navSub, ComponentModel.ISupportInitialize).BeginInit()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(busyBar, 0, 1)
        tlyMain.Controls.Add(tlyAntet, 0, 2)
        tlyMain.Controls.Add(ntfMesaj, 0, 3)
        tlyMain.Controls.Add(navSub, 0, 4)
        tlyMain.Controls.Add(pnlPages, 0, 5)
        tlyMain.Controls.Add(tlySubsol, 0, 6)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 1)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 7
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 46F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 4F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 76F))
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(1100, 720)
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
        capBar.ShowTextScaleSlider = False
        capBar.ShowThemeEditor = False
        capBar.ShowThemeOptions = False
        capBar.Size = New Size(1100, 46)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Ordonanțare de plată"
        '
        ' busyBar
        '
        busyBar.Dock = DockStyle.Fill
        busyBar.Location = New Point(0, 46)
        busyBar.Margin = New Padding(0)
        busyBar.Name = "busyBar"
        busyBar.Size = New Size(1100, 4)
        busyBar.TabIndex = 1
        busyBar.TabStop = False
        '
        ' tlyAntet
        '
        tlyAntet.ColumnCount = 8
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 70F))
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
        tlyAntet.Location = New Point(3, 53)
        tlyAntet.Name = "tlyAntet"
        tlyAntet.RowCount = 2
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 32F))
        tlyAntet.Size = New Size(1094, 70)
        tlyAntet.TabIndex = 2
        '
        ' lblCodCaption
        '
        lblCodCaption.AutoSize = True
        lblCodCaption.Dock = DockStyle.Fill
        lblCodCaption.Location = New Point(3, 0)
        lblCodCaption.Name = "lblCodCaption"
        lblCodCaption.Size = New Size(114, 36)
        lblCodCaption.TabIndex = 0
        lblCodCaption.Text = "Cod angajament"
        lblCodCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblCod
        '
        lblCod.AutoSize = True
        lblCod.Dock = DockStyle.Fill
        lblCod.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblCod.Location = New Point(123, 0)
        lblCod.Name = "lblCod"
        lblCod.Size = New Size(200, 36)
        lblCod.TabIndex = 1
        lblCod.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblNrOrdCaption
        '
        lblNrOrdCaption.AutoSize = True
        lblNrOrdCaption.Dock = DockStyle.Fill
        lblNrOrdCaption.Location = New Point(329, 0)
        lblNrOrdCaption.Name = "lblNrOrdCaption"
        lblNrOrdCaption.Size = New Size(84, 36)
        lblNrOrdCaption.TabIndex = 2
        lblNrOrdCaption.Text = "Număr"
        lblNrOrdCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblNrOrd
        '
        lblNrOrd.AutoSize = True
        lblNrOrd.Dock = DockStyle.Fill
        lblNrOrd.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblNrOrd.Location = New Point(419, 0)
        lblNrOrd.Name = "lblNrOrd"
        lblNrOrd.Size = New Size(114, 36)
        lblNrOrd.TabIndex = 3
        lblNrOrd.Text = "se alocă la salvare"
        lblNrOrd.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblDataCaption
        '
        lblDataCaption.AutoSize = True
        lblDataCaption.Dock = DockStyle.Fill
        lblDataCaption.Location = New Point(539, 0)
        lblDataCaption.Name = "lblDataCaption"
        lblDataCaption.Size = New Size(54, 36)
        lblDataCaption.TabIndex = 4
        lblDataCaption.Text = "Data"
        lblDataCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' dtpData
        '
        dtpData.Dock = DockStyle.Fill
        dtpData.Format = DateTimePickerFormat.Custom
        dtpData.CustomFormat = "dd.MM.yyyy"
        dtpData.Location = New Point(599, 6)
        dtpData.Margin = New Padding(3, 6, 3, 6)
        dtpData.Name = "dtpData"
        dtpData.Size = New Size(134, 23)
        dtpData.TabIndex = 5
        '
        ' lblTotalCaption
        '
        lblTotalCaption.AutoSize = True
        lblTotalCaption.Dock = DockStyle.Fill
        lblTotalCaption.Location = New Point(739, 0)
        lblTotalCaption.Name = "lblTotalCaption"
        lblTotalCaption.Size = New Size(54, 36)
        lblTotalCaption.TabIndex = 6
        lblTotalCaption.Text = "Total"
        lblTotalCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblTotal
        '
        lblTotal.AutoSize = True
        lblTotal.Dock = DockStyle.Fill
        lblTotal.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        lblTotal.Location = New Point(799, 0)
        lblTotal.Name = "lblTotal"
        lblTotal.Size = New Size(292, 36)
        lblTotal.TabIndex = 7
        lblTotal.Text = "0,00"
        lblTotal.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblObiectCaption
        '
        lblObiectCaption.AutoSize = True
        lblObiectCaption.Dock = DockStyle.Fill
        lblObiectCaption.Location = New Point(3, 36)
        lblObiectCaption.Name = "lblObiectCaption"
        lblObiectCaption.Size = New Size(114, 32)
        lblObiectCaption.TabIndex = 8
        lblObiectCaption.Text = "Obiect DDF"
        lblObiectCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblObiect
        '
        tlyAntet.SetColumnSpan(lblObiect, 7)
        lblObiect.AutoEllipsis = True
        lblObiect.Dock = DockStyle.Fill
        lblObiect.Location = New Point(123, 36)
        lblObiect.Name = "lblObiect"
        lblObiect.Size = New Size(968, 32)
        lblObiect.TabIndex = 9
        lblObiect.TextAlign = ContentAlignment.MiddleLeft
        '
        ' ntfMesaj
        '
        ntfMesaj.Dock = DockStyle.Fill
        ntfMesaj.Location = New Point(3, 126)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New Size(1094, 1)
        ntfMesaj.TabIndex = 3
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' navSub
        '
        navSub.Dock = DockStyle.Fill
        navSub.IconSize = 16
        navSub.ItemCornerRadius = 2
        navSub.ItemPadding = New Padding(3)
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
        navSub.Location = New Point(0, 129)
        navSub.Margin = New Padding(0)
        navSub.Name = "navSub"
        navSub.Orientation = KBotNavOrientation.Horizontal
        navSub.SelectedKey = Nothing
        navSub.Size = New Size(1100, 40)
        navSub.TabIndex = 4
        '
        ' pnlPages
        '
        pnlPages.Dock = DockStyle.Fill
        pnlPages.Location = New Point(0, 169)
        pnlPages.Margin = New Padding(0)
        pnlPages.Name = "pnlPages"
        pnlPages.Size = New Size(1100, 499)
        pnlPages.TabIndex = 5
        '
        ' tlySubsol
        '
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlySubsol.Controls.Add(btnRenunta, 1, 0)
        tlySubsol.Controls.Add(btnSalveaza, 2, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 668)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(1100, 52)
        tlySubsol.TabIndex = 6
        '
        ' btnRenunta
        '
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Location = New Point(743, 6)
        btnRenunta.Margin = New Padding(3, 6, 3, 6)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New Padding(10, 4, 10, 4)
        btnRenunta.Size = New Size(154, 40)
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnSalveaza
        '
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.Location = New Point(903, 6)
        btnSalveaza.Margin = New Padding(3, 6, 3, 6)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(10, 4, 10, 4)
        btnSalveaza.Size = New Size(194, 40)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează ordonanțarea"
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' OrdEditForm
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1102, 722)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(900, 600)
        Name = "OrdEditForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Ordonanțare de plată"
        tlyMain.ResumeLayout(False)
        tlyAntet.ResumeLayout(False)
        tlyAntet.PerformLayout()
        CType(navSub, ComponentModel.ISupportInitialize).EndInit()
        tlySubsol.ResumeLayout(False)
        tips.SetToolTipHeader(dtpData, "Data ordonanțării")
        tips.SetToolTipText(dtpData, "Data care se scrie în document." & vbLf & "Plățile propuse rămân cele ale zilei pentru care s-a generat ordonanțarea.")
        tips.SetToolTipHeader(lblNrOrd, "Numărul ordonanțării")
        tips.SetToolTipText(lblNrOrd, "Se alocă de server, în tranzacția de salvare." & vbLf & "Așa nu pot primi două salvări simultane același număr.")
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Trimite tot documentul într-o singură tranzacție." & vbLf & "Imaginile atașate se încarcă imediat după, când rândurile lor au chei.")
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fără să salveze nimic.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents tlyMain As TableLayoutPanel
    Friend WithEvents capBar As KBot.Controls.KBotCaptionBar
    Friend WithEvents busyBar As KBot.Controls.KBotBusyBar
    Friend WithEvents tlyAntet As TableLayoutPanel
    Friend WithEvents lblCodCaption As Label
    Friend WithEvents lblCod As Label
    Friend WithEvents lblNrOrdCaption As Label
    Friend WithEvents lblNrOrd As Label
    Friend WithEvents lblDataCaption As Label
    Friend WithEvents dtpData As DateTimePicker
    Friend WithEvents lblTotalCaption As Label
    Friend WithEvents lblTotal As Label
    Friend WithEvents lblObiectCaption As Label
    Friend WithEvents lblObiect As Label
    Friend WithEvents ntfMesaj As KBot.Controls.KBotNotice
    Friend WithEvents navSub As KBot.Controls.KBotNavList
    Friend WithEvents pnlPages As Panel
    Friend WithEvents tlySubsol As TableLayoutPanel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnSalveaza As Button
End Class
