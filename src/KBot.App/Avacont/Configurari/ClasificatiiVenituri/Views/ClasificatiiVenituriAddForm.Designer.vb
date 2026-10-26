Imports KBot.Controls

' «Adauga clasificatie de venituri» (slice AV01-11): the tree of the revenue codes of the nomenclator; the operator picks one and adds it.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ClasificatiiVenituriAddForm
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
        lblHint = New Label()
        tree = New AdvancedTreeControl()
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
        btnAdauga.Location = New Point(745, 12)
        btnAdauga.Margin = New Padding(0)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Size = New Size(255, 60)
        btnAdauga.TabIndex = 1
        btnAdauga.Text = "Adaugă clasificația"
        tips.SetToolTipHeader(btnAdauga, "Adaugă clasificația")
        tips.SetToolTipText(btnAdauga, "Adaugă în baza de date clasificația aleasă din arbore, cu denumirea din nomenclator.")
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
        pnlCard.Size = New Size(1196, 896)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' tlyCorp
        '
        tlyCorp.ColumnCount = 1
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCorp.Controls.Add(lblHint, 0, 0)
        tlyCorp.Controls.Add(tree, 0, 1)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 66)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(14, 8, 14, 8)
        tlyCorp.RowCount = 2
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 54F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.Size = New Size(1196, 746)
        tlyCorp.TabIndex = 1
        '
        ' lblHint
        '
        lblHint.Dock = DockStyle.Fill
        lblHint.Location = New Point(14, 8)
        lblHint.Margin = New Padding(0)
        lblHint.Name = "lblHint"
        lblHint.Size = New Size(1168, 54)
        lblHint.TabIndex = 0
        lblHint.Text = "Alegeți din arbore codul clasificației de venituri (capitol, subcapitol, paragraf). Cele care există deja nu se pot alege."
        lblHint.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tree
        '
        tree.BackColor = SystemColors.Window
        tree.BorderColor = SystemColors.ActiveBorder
        tree.ClickOnKeyNavigation = True
        tree.Dock = DockStyle.Fill
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderCaption = " NOMENCLATOR VENITURI"
        tree.HeaderHeight = 24
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderVisible = True
        tree.Indent = 14
        tree.LeftIconSize = New Size(16, 16)
        tree.LeftTextWidth = 80
        tree.Location = New Point(14, 62)
        tree.Margin = New Padding(0)
        tree.Name = "tree"
        tree.RightTextColumn = 150
        tree.SearchBackColor = SystemColors.Control
        tree.SearchDefaultText = "… tastați o parte din cod sau din denumire …"
        tree.SearchSeparatorColor = SystemColors.ActiveBorder
        tree.SearchSeparatorWidth = 2
        tree.SearchShow = True
        tree.Size = New Size(1168, 676)
        tree.TabIndex = 1
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(lblStare)
        pnlJos.Controls.Add(btnAdauga)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 812)
        pnlJos.Margin = New Padding(0)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(21, 12, 21, 12)
        pnlJos.Size = New Size(1196, 84)
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
        lblStare.Size = New Size(724, 60)
        lblStare.TabIndex = 0
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Right
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(1000, 12)
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(175, 60)
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
        capBar.Size = New Size(1196, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Adaugă clasificație de venituri"
        '
        ' ClasificatiiVenituriAddForm
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(1200, 900)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 4, 4, 4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "ClasificatiiVenituriAddForm"
        Padding = New Padding(2, 2, 2, 2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Adaugă clasificație de venituri"
        pnlCard.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyCorp As KBotTableLayoutPanel
    Friend WithEvents lblHint As Label
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnAdauga As Button
    Friend WithEvents btnRenunta As Button
    Friend WithEvents capBar As KBotCaptionBar
End Class
