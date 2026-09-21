Imports KBot.Controls

' The «Regulă nouă» window of the «Pagina FOREXE» settings page (operator, 21.09.2026): the
' open page's elements as a tree on the left, the three fields of the rule on the right,
' «Adaugă regula» / «Renunță» below. All controls are declared HERE
' (docs/kbot-forms-ui-convention.md). Coordinates are in the 144 dpi the form was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RegulaPaginaForm
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
        tlyCorp = New KBotTableLayoutPanel()
        pnlArbore = New Panel()
        arbore = New AdvancedTreeControl()
        lblFaraPagina = New Label()
        editor = New RegulaPaginaEditor()
        pnlJos = New Panel()
        btnRenunta = New Button()
        btnOk = New Button()
        lblStare = New Label()
        capBar = New KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlyCorp.SuspendLayout()
        pnlArbore.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlyCorp)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(1396, 896)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' tlyCorp
        '
        tlyCorp.ColumnCount = 2
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 56F))
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 44F))
        tlyCorp.Controls.Add(pnlArbore, 0, 0)
        tlyCorp.Controls.Add(editor, 1, 0)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 60)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(18, 14, 18, 6)
        tlyCorp.RowCount = 1
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.Size = New Size(1396, 760)
        tlyCorp.TabIndex = 1
        '
        ' pnlArbore
        '
        pnlArbore.Controls.Add(lblFaraPagina)
        pnlArbore.Controls.Add(arbore)
        pnlArbore.Dock = DockStyle.Fill
        pnlArbore.Location = New Point(18, 14)
        pnlArbore.Margin = New Padding(0, 0, 14, 0)
        pnlArbore.Name = "pnlArbore"
        pnlArbore.Size = New Size(747, 740)
        pnlArbore.TabIndex = 0
        '
        ' arbore
        '
        arbore.BorderColor = SystemColors.ActiveBorder
        arbore.Dock = DockStyle.Fill
        arbore.DynamicColumns = False
        arbore.ExpanderSize = 10
        arbore.Font = New Font("Consolas", 9.75F)
        arbore.HasNodeIcons = False
        arbore.HeaderBackColor = SystemColors.Control
        arbore.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        arbore.HeaderCaption = " ELEMENTELE PAGINII FOREXE"
        arbore.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        arbore.HeaderForeColor = Color.Black
        arbore.HeaderHeight = 30
        arbore.HeaderSeparatorColor = Color.Gainsboro
        arbore.HeaderSeparatorWidth = 2
        arbore.HeaderTextAlign = ContentAlignment.MiddleLeft
        arbore.HeaderVisible = True
        arbore.Indent = 10
        arbore.Location = New Point(0, 0)
        arbore.Margin = New Padding(0)
        arbore.Name = "arbore"
        arbore.SearchBarFont = New Font("Calibri", 9F)
        arbore.SearchClearButton = True
        arbore.SearchClearButtonImage = My.Resources.Resources.Everaldo_Crystal_Clear_Action_cancel_128_resized
        arbore.SearchDefaultText = "caută: tag, clasă, id, name sau text (minim 3 caractere)"
        arbore.SearchIn = AdvancedTreeControl.En_Tree_SearchIn.SearchIn_Both
        arbore.SearchShow = True
        arbore.Size = New Size(747, 740)
        arbore.TabIndex = 0
        tips.SetToolTipHeader(arbore, "Elementele paginii")
        tips.SetToolTipText(arbore, "Un clic pe un element îi pune selectorul și stilul de acum în câmpurile din dreapta și îl încadrează în pagina FOREXE." & vbLf & "Dublu clic = «Adaugă regula».")
        '
        ' lblFaraPagina
        '
        lblFaraPagina.Dock = DockStyle.Fill
        lblFaraPagina.Location = New Point(0, 0)
        lblFaraPagina.Margin = New Padding(4, 0, 4, 0)
        lblFaraPagina.Name = "lblFaraPagina"
        lblFaraPagina.Padding = New Padding(24)
        lblFaraPagina.Size = New Size(747, 740)
        lblFaraPagina.TabIndex = 1
        lblFaraPagina.Text = "Nu există o pagină FOREXE deschisă din care să citesc elementele." & vbLf & vbLf &
            "Regula se poate scrie și de mână, în câmpurile din dreapta."
        lblFaraPagina.TextAlign = ContentAlignment.MiddleCenter
        lblFaraPagina.Visible = False
        '
        ' editor
        '
        editor.Dock = DockStyle.Fill
        editor.Location = New Point(779, 14)
        editor.Margin = New Padding(0)
        editor.Name = "editor"
        editor.Size = New Size(599, 740)
        editor.TabIndex = 1
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Controls.Add(btnOk)
        pnlJos.Controls.Add(lblStare)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 820)
        pnlJos.Margin = New Padding(4)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(18, 10, 18, 20)
        pnlJos.Size = New Size(1396, 76)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Right
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(1062, 10)
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(147, 46)
        btnRenunta.TabIndex = 1
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnOk
        '
        btnOk.Dock = DockStyle.Right
        btnOk.FlatStyle = FlatStyle.Flat
        btnOk.Location = New Point(1209, 10)
        btnOk.Margin = New Padding(0)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(169, 46)
        btnOk.TabIndex = 2
        btnOk.Text = "Adaugă regula"
        tips.SetToolTipHeader(btnOk, "Adaugă regula")
        tips.SetToolTipText(btnOk, "Pune regula în listă. În pagină ajunge la «Salvează și aplică».")
        btnOk.UseVisualStyleBackColor = True
        '
        ' lblStare
        '
        lblStare.AutoSize = True
        lblStare.Location = New Point(18, 22)
        lblStare.Margin = New Padding(4, 0, 4, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(0, 22)
        lblStare.TabIndex = 0
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New Size(1396, 60)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Regulă nouă pentru pagina FOREXE"
        '
        ' RegulaPaginaForm
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(1400, 900)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "RegulaPaginaForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Regulă nouă pentru pagina FOREXE"
        pnlCard.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        pnlArbore.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        pnlJos.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents tlyCorp As KBotTableLayoutPanel
    Friend WithEvents pnlArbore As Panel
    Friend WithEvents arbore As AdvancedTreeControl
    Friend WithEvents lblFaraPagina As Label
    Friend WithEvents editor As RegulaPaginaEditor
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnOk As Button
    Friend WithEvents btnRenunta As Button
End Class
