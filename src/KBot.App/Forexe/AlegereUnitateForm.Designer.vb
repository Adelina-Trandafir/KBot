Imports KBot.Controls

' Portul modern al formularului Access `FX_Unitate` (felia 0048-02).
'
' Formularul Access NU este in FX_System_Export — nici sub numele asta, nici sub altul —
' asa ca aspectul de aici este PROIECTAT, nu portat. Ce se stie despre el se stie doar din
' apelul care il deschidea, in `Obtine_IdUnitate_Din`:
'
'     DoCmd.OpenForm "FX_Unitate", acNormal, , , , acDialog, SS & "|" & ClsfE
'
' adica: modal, si primea perechea intrebata ca argument. Cate coloane arata si cum arata
' raspunsul nu se poate sti. Consemnat in worklog ca NEVERIFICAT.
'
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md): formularul trebuie
' sa se randeze in designerul Visual Studio, nu sa se construiasca la rulare.
'
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste (7, 15) — perechea
' care corespunde fontului implicit Segoe UI 9pt nesetat. Alte formulare din depozit sunt
' autorizate la 150% (10, 25); AutoScaleMode.Font le face echivalente la rulare, dar cele
' doua NU se pot amesteca in acelasi fisier.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AlegereUnitateForm
    Inherits KBot.Theming.KBotThemedForm

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        grid = New KBotDataView()
        chkRetine = New CheckBox()
        btnAlege = New Button()
        btnRenunta = New Button()
        pnlCard = New Panel()
        tlpBody = New TableLayoutPanel()
        lblTitle = New Label()
        lblIntro = New Label()
        tlpInfo = New TableLayoutPanel()
        lblCapAngajament = New Label()
        lblAngajament = New Label()
        lblCapIndicator = New Label()
        lblIndicator = New Label()
        lblCapClsf = New Label()
        lblClsf = New Label()
        ntfError = New KBotNotice()
        tlpButtons = New TableLayoutPanel()
        capBar = New KBotCaptionBar()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        pnlCard.SuspendLayout()
        tlpBody.SuspendLayout()
        tlpInfo.SuspendLayout()
        tlpButtons.SuspendLayout()
        SuspendLayout()
        ' 
        ' grid
        ' 
        grid.AlternatingRows = False
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.BackColor = SystemColors.Window
        grid.CellTooltip.Enabled = False
        grid.ColumnFillMode = KBotFillMode.FirstColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.HeaderText = "Unitate"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "unitate"
        KBotDataColumn1.MinWidth = 120
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 250
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.HeaderText = "Sursă / Sector"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "sursa"
        KBotDataColumn2.MinWidth = 60
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 150
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.HeaderText = "Program"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "program"
        KBotDataColumn3.MinWidth = 60
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 150
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn4.HeaderText = "Cod"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "cod"
        KBotDataColumn4.MinWidth = 50
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 150
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Dock = DockStyle.Fill
        grid.Location = New Point(33, 216)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 26
        grid.ShrinkColumnsToFit = False
        grid.Size = New Size(818, 224)
        grid.TabIndex = 3
        tips.SetToolTipHeader(grid, "Unități posibile")
        tips.SetToolTipText(grid, "Fiecare rând este o unitate căreia îi poate aparține clasificația." & vbLf & "Alegeți-o pe cea potrivită; dublu-click confirmă direct.")
        ' 
        ' chkRetine
        ' 
        chkRetine.AutoSize = True
        chkRetine.Dock = DockStyle.Top
        chkRetine.Location = New Point(33, 458)
        chkRetine.Margin = New Padding(4, 13, 4, 10)
        chkRetine.Name = "chkRetine"
        chkRetine.Size = New Size(818, 29)
        chkRetine.TabIndex = 4
        chkRetine.Text = "Nu mă mai întreba pentru această combinație"
        tips.SetToolTipHeader(chkRetine, "Nu mă mai întreba")
        tips.SetToolTipText(chkRetine, "Ține minte răspunsul pentru ACEASTĂ combinație (sursă + clasificație)." & vbLf & "Data viitoare se aplică singur, fără să te mai întrebe." & vbLf & "O combinație nouă se întreabă oricum din nou.")
        chkRetine.UseVisualStyleBackColor = True
        ' 
        ' btnAlege
        ' 
        btnAlege.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnAlege.FlatStyle = FlatStyle.Flat
        btnAlege.Font = New Font("Segoe UI Semibold", 9F)
        btnAlege.Location = New Point(422, 5)
        btnAlege.Margin = New Padding(9, 5, 4, 5)
        btnAlege.Name = "btnAlege"
        btnAlege.Size = New Size(400, 58)
        btnAlege.TabIndex = 0
        btnAlege.Text = "Alege unitatea"
        tips.SetToolTipHeader(btnAlege, "Alege unitatea")
        tips.SetToolTipText(btnAlege, "Trimite din nou aceleași date, cu unitatea aleasă atașată.")
        btnAlege.UseVisualStyleBackColor = True
        ' 
        ' btnRenunta
        ' 
        btnRenunta.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(4, 5)
        btnRenunta.Margin = New Padding(4, 5, 9, 5)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(400, 58)
        btnRenunta.TabIndex = 1
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Oprește salvarea. Nu s-a scris nimic și nimic nu se va scrie.")
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlpBody)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 3)
        pnlCard.Margin = New Padding(4, 5, 4, 5)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(884, 729)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' tlpBody
        ' 
        tlpBody.ColumnCount = 1
        tlpBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpBody.Controls.Add(lblTitle, 0, 0)
        tlpBody.Controls.Add(lblIntro, 0, 1)
        tlpBody.Controls.Add(tlpInfo, 0, 2)
        tlpBody.Controls.Add(grid, 0, 3)
        tlpBody.Controls.Add(chkRetine, 0, 4)
        tlpBody.Controls.Add(ntfError, 0, 5)
        tlpBody.Controls.Add(tlpButtons, 0, 6)
        tlpBody.Dock = DockStyle.Fill
        tlpBody.Location = New Point(0, 67)
        tlpBody.Margin = New Padding(4, 5, 4, 5)
        tlpBody.Name = "tlpBody"
        tlpBody.Padding = New Padding(29, 13, 29, 20)
        tlpBody.RowCount = 7
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.Size = New Size(884, 662)
        tlpBody.TabIndex = 0
        tlpBody.Tag = "Card"
        ' 
        ' lblTitle
        ' 
        lblTitle.AutoSize = True
        lblTitle.Dock = DockStyle.Top
        lblTitle.Font = New Font("Segoe UI", 13F, FontStyle.Bold)
        lblTitle.Location = New Point(33, 13)
        lblTitle.Margin = New Padding(4, 0, 4, 3)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(818, 36)
        lblTitle.TabIndex = 0
        lblTitle.Text = "Alegeți unitatea"
        ' 
        ' lblIntro
        ' 
        lblIntro.AutoSize = True
        lblIntro.Dock = DockStyle.Top
        lblIntro.Location = New Point(33, 52)
        lblIntro.Margin = New Padding(4, 0, 4, 17)
        lblIntro.Name = "lblIntro"
        lblIntro.Size = New Size(818, 50)
        lblIntro.TabIndex = 1
        lblIntro.Text = "Clasificația de mai jos aparține mai multor unități, așa că salvarea s-a oprit și nu s-a scris nimic. Alegeți unitatea potrivită."
        ' 
        ' tlpInfo
        ' 
        tlpInfo.AutoSize = True
        tlpInfo.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpInfo.ColumnCount = 2
        tlpInfo.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 157F))
        tlpInfo.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpInfo.Controls.Add(lblCapAngajament, 0, 0)
        tlpInfo.Controls.Add(lblAngajament, 1, 0)
        tlpInfo.Controls.Add(lblCapIndicator, 0, 1)
        tlpInfo.Controls.Add(lblIndicator, 1, 1)
        tlpInfo.Controls.Add(lblCapClsf, 0, 2)
        tlpInfo.Controls.Add(lblClsf, 1, 2)
        tlpInfo.Dock = DockStyle.Top
        tlpInfo.Location = New Point(33, 119)
        tlpInfo.Margin = New Padding(4, 0, 4, 17)
        tlpInfo.Name = "tlpInfo"
        tlpInfo.RowCount = 3
        tlpInfo.RowStyles.Add(New RowStyle())
        tlpInfo.RowStyles.Add(New RowStyle())
        tlpInfo.RowStyles.Add(New RowStyle())
        tlpInfo.Size = New Size(818, 75)
        tlpInfo.TabIndex = 2
        tlpInfo.Tag = "Card"
        ' 
        ' lblCapAngajament
        ' 
        lblCapAngajament.AutoSize = True
        lblCapAngajament.Dock = DockStyle.Fill
        lblCapAngajament.Location = New Point(4, 0)
        lblCapAngajament.Margin = New Padding(4, 0, 4, 0)
        lblCapAngajament.Name = "lblCapAngajament"
        lblCapAngajament.Size = New Size(149, 25)
        lblCapAngajament.TabIndex = 0
        lblCapAngajament.Text = "Angajament"
        lblCapAngajament.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblAngajament
        ' 
        lblAngajament.AutoSize = True
        lblAngajament.Dock = DockStyle.Fill
        lblAngajament.Font = New Font("Segoe UI Semibold", 9F)
        lblAngajament.Location = New Point(161, 0)
        lblAngajament.Margin = New Padding(4, 0, 4, 0)
        lblAngajament.Name = "lblAngajament"
        lblAngajament.Size = New Size(653, 25)
        lblAngajament.TabIndex = 1
        lblAngajament.Text = "—"
        lblAngajament.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCapIndicator
        ' 
        lblCapIndicator.AutoSize = True
        lblCapIndicator.Dock = DockStyle.Fill
        lblCapIndicator.Location = New Point(4, 25)
        lblCapIndicator.Margin = New Padding(4, 0, 4, 0)
        lblCapIndicator.Name = "lblCapIndicator"
        lblCapIndicator.Size = New Size(149, 25)
        lblCapIndicator.TabIndex = 2
        lblCapIndicator.Text = "Indicator"
        lblCapIndicator.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblIndicator
        ' 
        lblIndicator.AutoSize = True
        lblIndicator.Dock = DockStyle.Fill
        lblIndicator.Font = New Font("Segoe UI Semibold", 9F)
        lblIndicator.Location = New Point(161, 25)
        lblIndicator.Margin = New Padding(4, 0, 4, 0)
        lblIndicator.Name = "lblIndicator"
        lblIndicator.Size = New Size(653, 25)
        lblIndicator.TabIndex = 3
        lblIndicator.Text = "—"
        lblIndicator.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCapClsf
        ' 
        lblCapClsf.AutoSize = True
        lblCapClsf.Dock = DockStyle.Fill
        lblCapClsf.Location = New Point(4, 50)
        lblCapClsf.Margin = New Padding(4, 0, 4, 0)
        lblCapClsf.Name = "lblCapClsf"
        lblCapClsf.Size = New Size(149, 25)
        lblCapClsf.TabIndex = 4
        lblCapClsf.Text = "Clasificație"
        lblCapClsf.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblClsf
        ' 
        lblClsf.AutoSize = True
        lblClsf.Dock = DockStyle.Fill
        lblClsf.Font = New Font("Segoe UI Semibold", 9F)
        lblClsf.Location = New Point(161, 50)
        lblClsf.Margin = New Padding(4, 0, 4, 0)
        lblClsf.Name = "lblClsf"
        lblClsf.Size = New Size(653, 25)
        lblClsf.TabIndex = 5
        lblClsf.Text = "—"
        lblClsf.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ntfError
        ' 
        ntfError.BackColor = Color.Transparent
        ntfError.Dock = DockStyle.Top
        ntfError.Location = New Point(33, 497)
        ntfError.Margin = New Padding(4, 0, 4, 10)
        ntfError.Name = "ntfError"
        ntfError.Size = New Size(818, 67)
        ntfError.TabIndex = 5
        ntfError.TabStop = False
        ntfError.Visible = False
        ' 
        ' tlpButtons
        ' 
        tlpButtons.AutoSize = True
        tlpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpButtons.ColumnCount = 2
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlpButtons.Controls.Add(btnRenunta, 0, 0)
        tlpButtons.Controls.Add(btnAlege, 1, 0)
        tlpButtons.Dock = DockStyle.Top
        tlpButtons.Location = New Point(29, 574)
        tlpButtons.Margin = New Padding(0)
        tlpButtons.Name = "tlpButtons"
        tlpButtons.RowCount = 1
        tlpButtons.RowStyles.Add(New RowStyle())
        tlpButtons.Size = New Size(826, 68)
        tlpButtons.TabIndex = 6
        tlpButtons.Tag = "Card"
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4, 5, 4, 5)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.ShowThemeEditor = False
        capBar.ShowThemeOptions = False
        capBar.Size = New Size(884, 67)
        capBar.TabIndex = 1
        capBar.TabStop = False
        capBar.Text = "K-BOT — Alegerea unității"
        ' 
        ' AlegereUnitateForm
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(886, 735)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 5, 4, 5)
        MaximizeBox = False
        MinimizeBox = False
        Name = "AlegereUnitateForm"
        Padding = New Padding(1, 3, 1, 3)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Alegerea unității"
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        pnlCard.ResumeLayout(False)
        tlpBody.ResumeLayout(False)
        tlpBody.PerformLayout()
        tlpInfo.ResumeLayout(False)
        tlpInfo.PerformLayout()
        tlpButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBot.Controls.KBotCaptionBar
    Friend WithEvents tlpBody As TableLayoutPanel
    Friend WithEvents lblTitle As Label
    Friend WithEvents lblIntro As Label
    Friend WithEvents tlpInfo As TableLayoutPanel
    Friend WithEvents lblCapAngajament As Label
    Friend WithEvents lblAngajament As Label
    Friend WithEvents lblCapIndicator As Label
    Friend WithEvents lblIndicator As Label
    Friend WithEvents lblCapClsf As Label
    Friend WithEvents lblClsf As Label
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents chkRetine As CheckBox
    Friend WithEvents ntfError As KBot.Controls.KBotNotice
    Friend WithEvents tlpButtons As TableLayoutPanel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnAlege As Button
End Class
