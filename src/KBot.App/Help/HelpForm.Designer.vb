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
        pnlRoot = New Panel()
        split = New SplitContainer()
        tvCuprins = New TreeView()
        pnlCautare = New KBotHelpSearchPanel()
        web = New WebBrowser()
        tlyBara = New KBotTableLayoutPanel()
        btnInapoi = New Button()
        btnInainte = New Button()
        lblContext = New Label()
        btnCapturi = New Button()
        btnManual = New Button()
        capBar = New KBotCaptionBar()
        pnlRoot.SuspendLayout()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        tlyBara.SuspendLayout()
        SuspendLayout()
        '
        ' pnlRoot
        '
        pnlRoot.Controls.Add(split)
        pnlRoot.Controls.Add(capBar)
        pnlRoot.Dock = DockStyle.Fill
        pnlRoot.Location = New Point(1, 1)
        pnlRoot.Margin = New Padding(0)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Size = New Size(998, 678)
        pnlRoot.TabIndex = 0
        pnlRoot.Tag = "Card"
        '
        ' split
        '
        split.Dock = DockStyle.Fill
        split.FixedPanel = FixedPanel.Panel1
        split.Location = New Point(0, 38)
        split.Margin = New Padding(0)
        split.Name = "split"
        split.Panel1.Controls.Add(tvCuprins)
        split.Panel1.Controls.Add(pnlCautare)
        split.Panel1.Padding = New Padding(8, 8, 0, 8)
        split.Panel2.Controls.Add(web)
        split.Panel2.Controls.Add(tlyBara)
        split.Panel2.Padding = New Padding(0, 0, 8, 8)
        split.Size = New Size(998, 640)
        split.SplitterDistance = 280
        split.SplitterWidth = 6
        split.TabIndex = 1
        '
        ' tvCuprins
        '
        tvCuprins.BorderStyle = BorderStyle.FixedSingle
        tvCuprins.Dock = DockStyle.Fill
        tvCuprins.FullRowSelect = True
        tvCuprins.HideSelection = False
        tvCuprins.ItemHeight = 22
        tvCuprins.Location = New Point(8, 44)
        tvCuprins.Margin = New Padding(0)
        tvCuprins.Name = "tvCuprins"
        tvCuprins.ShowLines = False
        tvCuprins.Size = New Size(272, 588)
        tvCuprins.TabIndex = 1
        '
        ' pnlCautare
        '
        pnlCautare.Dock = DockStyle.Top
        pnlCautare.Location = New Point(8, 8)
        pnlCautare.Margin = New Padding(0)
        pnlCautare.Name = "pnlCautare"
        pnlCautare.Padding = New Padding(0, 0, 0, 6)
        pnlCautare.Size = New Size(272, 36)
        pnlCautare.TabIndex = 0
        '
        ' web
        '
        web.AllowWebBrowserDrop = False
        web.Dock = DockStyle.Fill
        web.IsWebBrowserContextMenuEnabled = False
        web.Location = New Point(0, 40)
        web.Margin = New Padding(0)
        web.MinimumSize = New Size(20, 20)
        web.Name = "web"
        web.ScriptErrorsSuppressed = True
        web.Size = New Size(704, 592)
        web.TabIndex = 1
        '
        ' tlyBara
        '
        tlyBara.ColumnCount = 5
        tlyBara.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90F))
        tlyBara.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90F))
        tlyBara.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBara.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyBara.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyBara.Controls.Add(btnInapoi, 0, 0)
        tlyBara.Controls.Add(btnInainte, 1, 0)
        tlyBara.Controls.Add(lblContext, 2, 0)
        tlyBara.Controls.Add(btnCapturi, 3, 0)
        tlyBara.Controls.Add(btnManual, 4, 0)
        tlyBara.Dock = DockStyle.Top
        tlyBara.Location = New Point(0, 0)
        tlyBara.Margin = New Padding(0)
        tlyBara.Name = "tlyBara"
        tlyBara.Padding = New Padding(0, 6, 0, 6)
        tlyBara.RowCount = 1
        tlyBara.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBara.Size = New Size(704, 40)
        tlyBara.TabIndex = 0
        '
        ' btnInapoi
        '
        btnInapoi.Dock = DockStyle.Fill
        btnInapoi.FlatStyle = FlatStyle.Flat
        btnInapoi.Margin = New Padding(0, 0, 4, 0)
        btnInapoi.Name = "btnInapoi"
        btnInapoi.TabIndex = 0
        btnInapoi.Text = "◄ Înapoi"
        tips.SetToolTipHeader(btnInapoi, "Înapoi")
        tips.SetToolTipText(btnInapoi, "Pagina de ajutor văzută înainte.")
        btnInapoi.UseVisualStyleBackColor = True
        '
        ' btnInainte
        '
        btnInainte.Dock = DockStyle.Fill
        btnInainte.FlatStyle = FlatStyle.Flat
        btnInainte.Margin = New Padding(0, 0, 4, 0)
        btnInainte.Name = "btnInainte"
        btnInainte.TabIndex = 1
        btnInainte.Text = "Înainte ►"
        tips.SetToolTipHeader(btnInainte, "Înainte")
        tips.SetToolTipText(btnInainte, "Pagina din care te-ai întors cu «Înapoi».")
        btnInainte.UseVisualStyleBackColor = True
        '
        ' lblContext
        '
        lblContext.AutoEllipsis = True
        lblContext.Dock = DockStyle.Fill
        lblContext.Margin = New Padding(8, 0, 8, 0)
        lblContext.Name = "lblContext"
        lblContext.TabIndex = 2
        lblContext.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnCapturi
        '
        btnCapturi.Dock = DockStyle.Fill
        btnCapturi.FlatStyle = FlatStyle.Flat
        btnCapturi.Margin = New Padding(0, 0, 4, 0)
        btnCapturi.Name = "btnCapturi"
        btnCapturi.TabIndex = 3
        btnCapturi.Text = "Capturi..."
        btnCapturi.Visible = False
        tips.SetToolTipHeader(btnCapturi, "Capturi pentru ajutor")
        tips.SetToolTipText(btnCapturi, "Lista imaginilor cerute de paginile de ajutor, fiecare cu «Fă poza».")
        btnCapturi.UseVisualStyleBackColor = True
        '
        ' btnManual
        '
        btnManual.Dock = DockStyle.Fill
        btnManual.FlatStyle = FlatStyle.Flat
        btnManual.Margin = New Padding(0)
        btnManual.Name = "btnManual"
        btnManual.TabIndex = 4
        btnManual.Text = "Exportă manualul..."
        tips.SetToolTipHeader(btnManual, "Exportă manualul")
        tips.SetToolTipText(btnManual, "Salvează tot ajutorul într-un singur fișier, care se deschide în browser. De acolo se tipărește sau se salvează ca PDF.")
        btnManual.UseVisualStyleBackColor = True
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
        capBar.Size = New Size(998, 38)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Ajutor"
        '
        ' HelpForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1000, 680)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        MinimumSize = New Size(640, 420)
        Name = "HelpForm"
        Padding = New Padding(1)
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Ajutor"
        pnlRoot.ResumeLayout(False)
        split.Panel1.ResumeLayout(False)
        split.Panel1.PerformLayout()
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
    Friend WithEvents lblContext As Label
    Friend WithEvents btnCapturi As Button
    Friend WithEvents btnManual As Button
    Friend WithEvents capBar As KBotCaptionBar
End Class
