Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class HelpForm
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
        tips = New KBotToolTip(components)
        btnInapoi = New Button()
        btnInainte = New Button()
        btnIstoric = New Button()
        btnTextMic = New Button()
        btnTextMare = New Button()
        btnManual = New Button()
        btnPrint = New Button()
        mnuIstoric = New KBotDropDownMenu(components)
        mnuPrint = New KBotDropDownMenu(components)
        mnuExport = New KBotDropDownMenu(components)
        tmrModal = New Timer(components)
        tmrScroll = New Timer(components)
        pnlRoot = New Panel()
        split = New SplitContainer()
        tvCuprins = New TreeView()
        pnlCautare = New KBotHelpSearchPanel()
        web = New WebBrowser()
        tlyBara = New KBotTableLayoutPanel()
        lblContext = New Label()
        capBar = New KBotCaptionBar()
        pnlRoot.SuspendLayout()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        tlyBara.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnInapoi
        ' 
        btnInapoi.Dock = DockStyle.Fill
        btnInapoi.FlatStyle = FlatStyle.Flat
        btnInapoi.Image = My.Resources.Resources.left_32
        btnInapoi.Location = New Point(0, 6)
        btnInapoi.Margin = New Padding(0, 0, 6, 0)
        btnInapoi.Name = "btnInapoi"
        btnInapoi.Size = New Size(48, 48)
        btnInapoi.TabIndex = 0
        tips.SetToolTipHeader(btnInapoi, "Înapoi")
        tips.SetToolTipText(btnInapoi, "Pagina de ajutor văzută înainte.")
        btnInapoi.UseVisualStyleBackColor = True
        ' 
        ' btnInainte
        ' 
        btnInainte.Dock = DockStyle.Fill
        btnInainte.FlatStyle = FlatStyle.Flat
        btnInainte.Image = My.Resources.Resources.right_32
        btnInainte.Location = New Point(54, 6)
        btnInainte.Margin = New Padding(0, 0, 6, 0)
        btnInainte.Name = "btnInainte"
        btnInainte.Size = New Size(48, 48)
        btnInainte.TabIndex = 1
        tips.SetToolTipHeader(btnInainte, "Înainte")
        tips.SetToolTipText(btnInainte, "Pagina din care te-ai întors cu «Înapoi».")
        btnInainte.UseVisualStyleBackColor = True
        ' 
        ' btnIstoric
        ' 
        btnIstoric.Dock = DockStyle.Fill
        btnIstoric.FlatStyle = FlatStyle.Flat
        btnIstoric.Image = My.Resources.Resources.istoric_32
        btnIstoric.Location = New Point(108, 6)
        btnIstoric.Margin = New Padding(0, 0, 6, 0)
        btnIstoric.Name = "btnIstoric"
        btnIstoric.Size = New Size(48, 48)
        btnIstoric.TabIndex = 2
        tips.SetToolTipHeader(btnIstoric, "Istoric")
        tips.SetToolTipText(btnIstoric, "Paginile de ajutor deschise de când ai pornit K-BOT, cea mai nouă sus. Un clic pe una o deschide din nou.")
        btnIstoric.UseVisualStyleBackColor = True
        ' 
        ' btnTextMic
        ' 
        btnTextMic.Dock = DockStyle.Fill
        btnTextMic.FlatStyle = FlatStyle.Flat
        btnTextMic.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Magnifier_zoom_out_32
        btnTextMic.Location = New Point(827, 6)
        btnTextMic.Margin = New Padding(0, 0, 6, 0)
        btnTextMic.Name = "btnTextMic"
        btnTextMic.Size = New Size(54, 48)
        btnTextMic.TabIndex = 4
        tips.SetToolTipHeader(btnTextMic, "Text mai mic")
        tips.SetToolTipText(btnTextMic, "Micșorează textul paginilor de ajutor. Mărimea aleasă se ține minte.")
        btnTextMic.UseVisualStyleBackColor = True
        ' 
        ' btnTextMare
        ' 
        btnTextMare.Dock = DockStyle.Fill
        btnTextMare.FlatStyle = FlatStyle.Flat
        btnTextMare.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Magnifier_zoom_in_32
        btnTextMare.Location = New Point(887, 6)
        btnTextMare.Margin = New Padding(0, 0, 12, 0)
        btnTextMare.Name = "btnTextMare"
        btnTextMare.Size = New Size(54, 48)
        btnTextMare.TabIndex = 5
        tips.SetToolTipHeader(btnTextMare, "Text mai mare")
        tips.SetToolTipText(btnTextMare, "Mărește textul paginilor de ajutor. Mărimea aleasă se ține minte.")
        btnTextMare.UseVisualStyleBackColor = True
        ' 
        ' btnManual
        ' 
        btnManual.Dock = DockStyle.Fill
        btnManual.FlatStyle = FlatStyle.Flat
        btnManual.Image = My.Resources.Resources.export
        btnManual.Location = New Point(1007, 6)
        btnManual.Margin = New Padding(0)
        btnManual.Name = "btnManual"
        btnManual.Size = New Size(48, 48)
        btnManual.TabIndex = 7
        tips.SetToolTipHeader(btnManual, "Exportă")
        tips.SetToolTipText(btnManual, "Salvează într-un singur fișier, care se deschide în browser: subiectul curent, capitolul lui sau tot ajutorul (manualul). Alegi dintr-un meniu.")
        btnManual.UseVisualStyleBackColor = True
        ' 
        ' btnPrint
        ' 
        btnPrint.Dock = DockStyle.Fill
        btnPrint.FlatStyle = FlatStyle.Flat
        btnPrint.Image = My.Resources.Resources.print
        btnPrint.Location = New Point(953, 6)
        btnPrint.Margin = New Padding(0, 0, 6, 0)
        btnPrint.Name = "btnPrint"
        btnPrint.Size = New Size(48, 48)
        btnPrint.TabIndex = 6
        tips.SetToolTipHeader(btnPrint, "Imprimă")
        tips.SetToolTipText(btnPrint, "Trimite la imprimantă, cu textul și imaginile de pe ecran: subiectul curent, capitolul lui sau tot ajutorul. Alegi dintr-un meniu.")
        btnPrint.UseVisualStyleBackColor = True
        '
        ' mnuIstoric
        '
        '
        ' mnuPrint
        '
        '
        ' mnuExport
        '
        '
        ' tmrModal
        '
        tmrModal.Interval = 150
        '
        ' tmrScroll
        '
        tmrScroll.Interval = 250
        ' 
        ' pnlRoot
        ' 
        pnlRoot.Controls.Add(split)
        pnlRoot.Controls.Add(capBar)
        pnlRoot.Dock = DockStyle.Fill
        pnlRoot.Location = New Point(2, 2)
        pnlRoot.Margin = New Padding(0)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Size = New Size(1496, 1016)
        pnlRoot.TabIndex = 0
        pnlRoot.Tag = "Card"
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.FixedPanel = FixedPanel.Panel1
        split.Location = New Point(0, 57)
        split.Margin = New Padding(0)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(tvCuprins)
        split.Panel1.Controls.Add(pnlCautare)
        split.Panel1.Padding = New Padding(12, 12, 0, 12)
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(web)
        split.Panel2.Controls.Add(tlyBara)
        split.Panel2.Padding = New Padding(0, 0, 12, 12)
        split.Size = New Size(1496, 959)
        split.SplitterDistance = 420
        split.SplitterWidth = 9
        split.TabIndex = 1
        ' 
        ' tvCuprins
        ' 
        tvCuprins.BorderStyle = BorderStyle.FixedSingle
        tvCuprins.Dock = DockStyle.Fill
        tvCuprins.FullRowSelect = True
        tvCuprins.HideSelection = False
        tvCuprins.ItemHeight = 22
        tvCuprins.Location = New Point(12, 66)
        tvCuprins.Margin = New Padding(0)
        tvCuprins.Name = "tvCuprins"
        tvCuprins.ShowLines = False
        tvCuprins.Size = New Size(408, 881)
        tvCuprins.TabIndex = 1
        ' 
        ' pnlCautare
        ' 
        pnlCautare.Dock = DockStyle.Top
        pnlCautare.Font = New Font("Calibri", 9F)
        pnlCautare.Location = New Point(12, 12)
        pnlCautare.Margin = New Padding(0)
        pnlCautare.Name = "pnlCautare"
        pnlCautare.Padding = New Padding(0, 0, 0, 9)
        pnlCautare.Size = New Size(408, 54)
        pnlCautare.TabIndex = 0
        ' 
        ' web
        ' 
        web.AllowWebBrowserDrop = False
        web.Dock = DockStyle.Fill
        web.IsWebBrowserContextMenuEnabled = False
        web.Location = New Point(0, 60)
        web.Margin = New Padding(0)
        web.MinimumSize = New Size(30, 30)
        web.Name = "web"
        web.ScriptErrorsSuppressed = True
        web.Size = New Size(1055, 887)
        web.TabIndex = 1
        ' 
        ' tlyBara
        ' 
        tlyBara.ColumnCount = 8
        tlyBara.ColumnStyles.Add(New ColumnStyle())
        tlyBara.ColumnStyles.Add(New ColumnStyle())
        tlyBara.ColumnStyles.Add(New ColumnStyle())
        tlyBara.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBara.ColumnStyles.Add(New ColumnStyle())
        tlyBara.ColumnStyles.Add(New ColumnStyle())
        tlyBara.ColumnStyles.Add(New ColumnStyle())
        tlyBara.ColumnStyles.Add(New ColumnStyle())
        tlyBara.Controls.Add(btnInapoi, 0, 0)
        tlyBara.Controls.Add(btnInainte, 1, 0)
        tlyBara.Controls.Add(btnIstoric, 2, 0)
        tlyBara.Controls.Add(lblContext, 3, 0)
        tlyBara.Controls.Add(btnTextMic, 4, 0)
        tlyBara.Controls.Add(btnTextMare, 5, 0)
        tlyBara.Controls.Add(btnPrint, 6, 0)
        tlyBara.Controls.Add(btnManual, 7, 0)
        tlyBara.Dock = DockStyle.Top
        tlyBara.Location = New Point(0, 0)
        tlyBara.Margin = New Padding(0)
        tlyBara.Name = "tlyBara"
        tlyBara.Padding = New Padding(0, 6, 0, 6)
        tlyBara.RowCount = 1
        tlyBara.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBara.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tlyBara.Size = New Size(1055, 60)
        tlyBara.TabIndex = 0
        ' 
        ' lblContext
        ' 
        lblContext.AutoEllipsis = True
        lblContext.Dock = DockStyle.Fill
        lblContext.Location = New Point(174, 6)
        lblContext.Margin = New Padding(12, 0, 12, 0)
        lblContext.Name = "lblContext"
        lblContext.Size = New Size(641, 48)
        lblContext.TabIndex = 3
        lblContext.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowHelpButton = False
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.Size = New Size(1496, 57)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Ajutor"
        ' 
        ' HelpForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1500, 1020)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MinimumSize = New Size(960, 630)
        Name = "HelpForm"
        Padding = New Padding(2)
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Ajutor"
        TopMost = True
        pnlRoot.ResumeLayout(False)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        tlyBara.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlRoot As Panel
    Friend WithEvents split As SplitContainer
    Friend WithEvents tvCuprins As TreeView
    Friend WithEvents pnlCautare As KBotHelpSearchPanel
    Friend WithEvents web As WebBrowser
    Friend WithEvents tlyBara As KBotTableLayoutPanel
    Friend WithEvents btnInapoi As Button
    Friend WithEvents btnInainte As Button
    Friend WithEvents btnIstoric As Button
    Friend WithEvents lblContext As Label
    Friend WithEvents btnTextMic As Button
    Friend WithEvents btnTextMare As Button
    Friend WithEvents btnManual As Button
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents mnuIstoric As KBotDropDownMenu
    Friend WithEvents tmrModal As Timer
    Friend WithEvents btnPrint As Button
    Friend WithEvents mnuPrint As KBotDropDownMenu
    Friend WithEvents mnuExport As KBotDropDownMenu
    Friend WithEvents tmrScroll As Timer
End Class
