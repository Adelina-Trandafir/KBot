Imports KBot.Controls

' The director's K-BOT (slice 0081-06): the DDF revisions waiting for the Ordonator's signature,
' across every unit of the director, on the left; the document of the selected one on the right
' (the DDF view, created at run time in pnlDocument like the shell's views -- its constructor
' takes the API client). Nothing else: no tree, no FOREXE, no editing.
'
' Inherits KBotShellForm (resizable, maximizable). All controls are declared HERE
' (docs/kbot-forms-ui-convention.md). Coordinates at 96 dpi, AutoScaleMode.Dpi (slice 0066-02).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DirectorForm
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
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        lblTitlu = New Label()
        split = New SplitContainer()
        lstDocumente = New ListView()
        colUnitate = New ColumnHeader()
        colCod = New ColumnHeader()
        colObiect = New ColumnHeader()
        colRevizie = New ColumnHeader()
        colData = New ColumnHeader()
        colTotal = New ColumnHeader()
        pnlDocument = New Panel()
        lblDocument = New Label()
        tlySubsol = New KBotTableLayoutPanel()
        btnReincarca = New Button()
        lblStare = New Label()
        btnInchide = New Button()
        tlyMain.SuspendLayout()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        pnlDocument.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(lblTitlu, 0, 1)
        tlyMain.Controls.Add(split, 0, 2)
        tlyMain.Controls.Add(tlySubsol, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 1)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(1398, 818)
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
        capBar.Size = New Size(1398, 44)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Documente de fundamentare de semnat"
        '
        ' lblTitlu
        '
        lblTitlu.AutoEllipsis = True
        lblTitlu.Dock = DockStyle.Fill
        lblTitlu.Font = New Font("Segoe UI Semibold", 11F)
        lblTitlu.Location = New Point(0, 44)
        lblTitlu.Margin = New Padding(0)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.Padding = New Padding(12, 0, 12, 0)
        lblTitlu.Size = New Size(1398, 40)
        lblTitlu.TabIndex = 1
        lblTitlu.Text = "Se încarcă documentele de semnat…"
        lblTitlu.TextAlign = ContentAlignment.MiddleLeft
        '
        ' split
        '
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 84)
        split.Margin = New Padding(0)
        split.Name = "split"
        '
        ' split.Panel1
        '
        split.Panel1.Controls.Add(lstDocumente)
        split.Panel1.Padding = New Padding(8, 0, 0, 6)
        split.Panel1MinSize = 300
        '
        ' split.Panel2
        '
        split.Panel2.Controls.Add(pnlDocument)
        split.Panel2.Padding = New Padding(0, 0, 8, 6)
        split.Panel2MinSize = 400
        split.Size = New Size(1398, 682)
        split.SplitterDistance = 520
        split.SplitterWidth = 6
        split.TabIndex = 2
        '
        ' lstDocumente
        '
        lstDocumente.BorderStyle = BorderStyle.None
        lstDocumente.Columns.AddRange(New ColumnHeader() {colUnitate, colCod, colObiect, colRevizie, colData, colTotal})
        lstDocumente.Dock = DockStyle.Fill
        lstDocumente.FullRowSelect = True
        lstDocumente.HideSelection = False
        lstDocumente.Location = New Point(8, 0)
        lstDocumente.Margin = New Padding(0)
        lstDocumente.MultiSelect = False
        lstDocumente.Name = "lstDocumente"
        lstDocumente.Size = New Size(512, 676)
        lstDocumente.TabIndex = 0
        lstDocumente.UseCompatibleStateImageBehavior = False
        lstDocumente.View = View.Details
        tips.SetToolTipHeader(lstDocumente, "Documente de semnat")
        tips.SetToolTipText(lstDocumente, "Reviziile semnate A și B care așteaptă semnătura directorului, din toate unitățile." & vbLf & "Selectați una ca să o deschideți în dreapta.")
        '
        ' colUnitate
        '
        colUnitate.Text = "Unitate"
        colUnitate.Width = 120
        '
        ' colCod
        '
        colCod.Text = "Angajament"
        colCod.Width = 110
        '
        ' colObiect
        '
        colObiect.Text = "Obiect"
        colObiect.Width = 150
        '
        ' colRevizie
        '
        colRevizie.Text = "Rev."
        colRevizie.TextAlign = HorizontalAlignment.Right
        colRevizie.Width = 40
        '
        ' colData
        '
        colData.Text = "Data"
        colData.Width = 80
        '
        ' colTotal
        '
        colTotal.Text = "Total"
        colTotal.TextAlign = HorizontalAlignment.Right
        colTotal.Width = 90
        '
        ' pnlDocument
        '
        pnlDocument.Controls.Add(lblDocument)
        pnlDocument.Dock = DockStyle.Fill
        pnlDocument.Location = New Point(0, 0)
        pnlDocument.Margin = New Padding(0)
        pnlDocument.Name = "pnlDocument"
        pnlDocument.Size = New Size(864, 676)
        pnlDocument.TabIndex = 0
        pnlDocument.Tag = "Card"
        '
        ' lblDocument
        '
        lblDocument.Dock = DockStyle.Fill
        lblDocument.Font = New Font("Segoe UI", 10F)
        lblDocument.Location = New Point(0, 0)
        lblDocument.Margin = New Padding(0)
        lblDocument.Name = "lblDocument"
        lblDocument.Size = New Size(864, 676)
        lblDocument.TabIndex = 0
        lblDocument.Text = "Selectați un document din listă."
        lblDocument.TextAlign = ContentAlignment.MiddleCenter
        '
        ' tlySubsol
        '
        tlySubsol.AutoFitToTheme = False
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(btnReincarca, 0, 0)
        tlySubsol.Controls.Add(lblStare, 1, 0)
        tlySubsol.Controls.Add(btnInchide, 2, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 766)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(8, 6, 8, 6)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(1398, 52)
        tlySubsol.TabIndex = 3
        '
        ' btnReincarca
        '
        btnReincarca.AutoSize = True
        btnReincarca.FlatStyle = FlatStyle.Flat
        btnReincarca.Location = New Point(8, 6)
        btnReincarca.Margin = New Padding(0)
        btnReincarca.Name = "btnReincarca"
        btnReincarca.Padding = New Padding(8, 3, 8, 3)
        btnReincarca.Size = New Size(150, 40)
        btnReincarca.TabIndex = 0
        btnReincarca.Text = "Reîncarcă lista"
        tips.SetToolTipHeader(btnReincarca, "Reîncarcă lista")
        tips.SetToolTipText(btnReincarca, "Citește din nou documentele care așteaptă semnătura directorului." & vbLf & "Un document semnat dispare din listă.")
        btnReincarca.UseVisualStyleBackColor = True
        '
        ' lblStare
        '
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(166, 6)
        lblStare.Margin = New Padding(8, 0, 8, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(1086, 40)
        lblStare.TabIndex = 1
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnInchide
        '
        btnInchide.AutoSize = True
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Location = New Point(1260, 6)
        btnInchide.Margin = New Padding(0)
        btnInchide.Name = "btnInchide"
        btnInchide.Padding = New Padding(17, 3, 17, 3)
        btnInchide.Size = New Size(130, 40)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' DirectorForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1400, 820)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        MinimumSize = New Size(900, 560)
        Name = "DirectorForm"
        Padding = New Padding(1)
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Documente de fundamentare de semnat"
        tlyMain.ResumeLayout(False)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        pnlDocument.ResumeLayout(False)
        tlySubsol.ResumeLayout(False)
        tlySubsol.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents lblTitlu As Label
    Friend WithEvents split As SplitContainer
    Friend WithEvents lstDocumente As ListView
    Friend WithEvents colUnitate As ColumnHeader
    Friend WithEvents colCod As ColumnHeader
    Friend WithEvents colObiect As ColumnHeader
    Friend WithEvents colRevizie As ColumnHeader
    Friend WithEvents colData As ColumnHeader
    Friend WithEvents colTotal As ColumnHeader
    Friend WithEvents pnlDocument As Panel
    Friend WithEvents lblDocument As Label
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents btnReincarca As Button
    Friend WithEvents lblStare As Label
    Friend WithEvents btnInchide As Button
End Class
