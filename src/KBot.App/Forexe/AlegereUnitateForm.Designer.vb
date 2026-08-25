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
        Dim colUnitate As KBotDataColumn = New KBotDataColumn()
        Dim colSursa As KBotDataColumn = New KBotDataColumn()
        Dim colProgram As KBotDataColumn = New KBotDataColumn()
        Dim colCod As KBotDataColumn = New KBotDataColumn()
        components = New ComponentModel.Container()
        tips = New Controls.KBotToolTip(components)
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
        grid = New Controls.KBotDataView()
        chkRetine = New CheckBox()
        ntfError = New Controls.KBotNotice()
        tlpButtons = New TableLayoutPanel()
        btnRenunta = New Button()
        btnAlege = New Button()
        capBar = New Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlpBody.SuspendLayout()
        tlpInfo.SuspendLayout()
        tlpButtons.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlpBody)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(618, 576)
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
        tlpBody.Location = New Point(0, 46)
        tlpBody.Name = "tlpBody"
        tlpBody.Padding = New Padding(20, 8, 20, 12)
        tlpBody.RowCount = 7
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.Size = New Size(618, 530)
        tlpBody.TabIndex = 0
        tlpBody.Tag = "Card"
        '
        ' lblTitle
        '
        lblTitle.AutoSize = True
        lblTitle.Dock = DockStyle.Top
        lblTitle.Font = New Font("Segoe UI", 13F, FontStyle.Bold)
        lblTitle.Location = New Point(23, 8)
        lblTitle.Margin = New Padding(3, 0, 3, 2)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(572, 25)
        lblTitle.TabIndex = 0
        lblTitle.Text = "Alegeți unitatea"
        '
        ' lblIntro
        '
        lblIntro.AutoSize = True
        lblIntro.Dock = DockStyle.Top
        lblIntro.Location = New Point(23, 35)
        lblIntro.Margin = New Padding(3, 0, 3, 10)
        lblIntro.Name = "lblIntro"
        lblIntro.Size = New Size(572, 30)
        lblIntro.TabIndex = 1
        lblIntro.Text = "Clasificația de mai jos aparține mai multor unități, așa că salvarea s-a oprit și nu s-a scris nimic. Alegeți unitatea potrivită."
        '
        ' tlpInfo
        '
        tlpInfo.AutoSize = True
        tlpInfo.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpInfo.ColumnCount = 2
        tlpInfo.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlpInfo.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpInfo.Controls.Add(lblCapAngajament, 0, 0)
        tlpInfo.Controls.Add(lblAngajament, 1, 0)
        tlpInfo.Controls.Add(lblCapIndicator, 0, 1)
        tlpInfo.Controls.Add(lblIndicator, 1, 1)
        tlpInfo.Controls.Add(lblCapClsf, 0, 2)
        tlpInfo.Controls.Add(lblClsf, 1, 2)
        tlpInfo.Dock = DockStyle.Top
        tlpInfo.Location = New Point(23, 75)
        tlpInfo.Margin = New Padding(3, 0, 3, 10)
        tlpInfo.Name = "tlpInfo"
        tlpInfo.RowCount = 3
        tlpInfo.RowStyles.Add(New RowStyle())
        tlpInfo.RowStyles.Add(New RowStyle())
        tlpInfo.RowStyles.Add(New RowStyle())
        tlpInfo.Size = New Size(572, 60)
        tlpInfo.TabIndex = 2
        tlpInfo.Tag = "Card"
        '
        ' lblCapAngajament
        '
        lblCapAngajament.AutoSize = True
        lblCapAngajament.Dock = DockStyle.Fill
        lblCapAngajament.Location = New Point(3, 0)
        lblCapAngajament.Name = "lblCapAngajament"
        lblCapAngajament.Size = New Size(104, 20)
        lblCapAngajament.TabIndex = 0
        lblCapAngajament.Text = "Angajament"
        lblCapAngajament.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblAngajament
        '
        lblAngajament.AutoSize = True
        lblAngajament.Dock = DockStyle.Fill
        lblAngajament.Font = New Font("Segoe UI Semibold", 9F)
        lblAngajament.Location = New Point(113, 0)
        lblAngajament.Name = "lblAngajament"
        lblAngajament.Size = New Size(456, 20)
        lblAngajament.TabIndex = 1
        lblAngajament.Text = "—"
        lblAngajament.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblCapIndicator
        '
        lblCapIndicator.AutoSize = True
        lblCapIndicator.Dock = DockStyle.Fill
        lblCapIndicator.Location = New Point(3, 20)
        lblCapIndicator.Name = "lblCapIndicator"
        lblCapIndicator.Size = New Size(104, 20)
        lblCapIndicator.TabIndex = 2
        lblCapIndicator.Text = "Indicator"
        lblCapIndicator.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblIndicator
        '
        lblIndicator.AutoSize = True
        lblIndicator.Dock = DockStyle.Fill
        lblIndicator.Font = New Font("Segoe UI Semibold", 9F)
        lblIndicator.Location = New Point(113, 20)
        lblIndicator.Name = "lblIndicator"
        lblIndicator.Size = New Size(456, 20)
        lblIndicator.TabIndex = 3
        lblIndicator.Text = "—"
        lblIndicator.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblCapClsf
        '
        lblCapClsf.AutoSize = True
        lblCapClsf.Dock = DockStyle.Fill
        lblCapClsf.Location = New Point(3, 40)
        lblCapClsf.Name = "lblCapClsf"
        lblCapClsf.Size = New Size(104, 20)
        lblCapClsf.TabIndex = 4
        lblCapClsf.Text = "Clasificație"
        lblCapClsf.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblClsf
        '
        lblClsf.AutoSize = True
        lblClsf.Dock = DockStyle.Fill
        lblClsf.Font = New Font("Segoe UI Semibold", 9F)
        lblClsf.Location = New Point(113, 40)
        lblClsf.Name = "lblClsf"
        lblClsf.Size = New Size(456, 20)
        lblClsf.TabIndex = 5
        lblClsf.Text = "—"
        lblClsf.TextAlign = ContentAlignment.MiddleLeft
        '
        ' grid
        '
        grid.AlternatingRows = True
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.CellTooltip.Enabled = False
        grid.ColumnFillMode = KBotFillMode.FirstColumn
        colUnitate.HeaderText = "Unitate"
        colUnitate.HeaderTextAlign = ContentAlignment.MiddleLeft
        colUnitate.Key = "unitate"
        colUnitate.MinWidth = 120
        colUnitate.[ReadOnly] = True
        colUnitate.ShowColumnFilter = False
        colUnitate.TextAlign = ContentAlignment.MiddleLeft
        colUnitate.Width = 250
        colSursa.HeaderText = "Sursă / Sector"
        colSursa.HeaderTextAlign = ContentAlignment.MiddleCenter
        colSursa.Key = "sursa"
        colSursa.MinWidth = 60
        colSursa.[ReadOnly] = True
        colSursa.ShowColumnFilter = False
        colSursa.TextAlign = ContentAlignment.MiddleCenter
        colSursa.Width = 110
        colProgram.HeaderText = "Program"
        colProgram.HeaderTextAlign = ContentAlignment.MiddleLeft
        colProgram.Key = "program"
        colProgram.MinWidth = 60
        colProgram.[ReadOnly] = True
        colProgram.ShowColumnFilter = False
        colProgram.TextAlign = ContentAlignment.MiddleLeft
        colProgram.Width = 130
        colCod.HeaderText = "Cod"
        colCod.HeaderTextAlign = ContentAlignment.MiddleCenter
        colCod.Key = "cod"
        colCod.MinWidth = 50
        colCod.[ReadOnly] = True
        colCod.ShowColumnFilter = False
        colCod.TextAlign = ContentAlignment.MiddleCenter
        colCod.Width = 70
        grid.Columns.Add(colUnitate)
        grid.Columns.Add(colSursa)
        grid.Columns.Add(colProgram)
        grid.Columns.Add(colCod)
        grid.Dock = DockStyle.Fill
        grid.FooterVisible = False
        grid.Location = New Point(23, 145)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 26
        grid.Size = New Size(572, 260)
        grid.TabIndex = 3
        '
        ' chkRetine
        '
        chkRetine.AutoSize = True
        chkRetine.Dock = DockStyle.Top
        chkRetine.Location = New Point(23, 413)
        chkRetine.Margin = New Padding(3, 8, 3, 6)
        chkRetine.Name = "chkRetine"
        chkRetine.Size = New Size(572, 24)
        chkRetine.TabIndex = 4
        chkRetine.Text = "Nu mă mai întreba pentru această combinație"
        chkRetine.UseVisualStyleBackColor = True
        '
        ' ntfError
        '
        ntfError.BackColor = Color.Transparent
        ntfError.Dock = DockStyle.Top
        ntfError.Location = New Point(23, 443)
        ntfError.Margin = New Padding(3, 0, 3, 6)
        ntfError.Name = "ntfError"
        ntfError.Size = New Size(572, 40)
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
        tlpButtons.Location = New Point(20, 489)
        tlpButtons.Margin = New Padding(0)
        tlpButtons.Name = "tlpButtons"
        tlpButtons.RowCount = 1
        tlpButtons.RowStyles.Add(New RowStyle())
        tlpButtons.Size = New Size(578, 41)
        tlpButtons.TabIndex = 6
        tlpButtons.Tag = "Card"
        '
        ' btnRenunta
        '
        btnRenunta.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(3, 3)
        btnRenunta.Margin = New Padding(3, 3, 6, 3)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(280, 35)
        btnRenunta.TabIndex = 1
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnAlege
        '
        btnAlege.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnAlege.FlatStyle = FlatStyle.Flat
        btnAlege.Font = New Font("Segoe UI Semibold", 9F)
        btnAlege.Location = New Point(295, 3)
        btnAlege.Margin = New Padding(6, 3, 3, 3)
        btnAlege.Name = "btnAlege"
        btnAlege.Size = New Size(280, 35)
        btnAlege.TabIndex = 0
        btnAlege.Text = "Alege unitatea"
        btnAlege.UseVisualStyleBackColor = True
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.Location = New Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = False
        capBar.ShowMinimize = False
        capBar.Size = New Size(618, 46)
        capBar.TabIndex = 1
        capBar.TabStop = False
        capBar.Text = "K-BOT — Alegerea unității"
        '
        ' AlegereUnitateForm
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(620, 580)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "AlegereUnitateForm"
        Padding = New Padding(1, 2, 1, 2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Alegerea unității"
        pnlCard.ResumeLayout(False)
        tlpBody.ResumeLayout(False)
        tlpBody.PerformLayout()
        tlpInfo.ResumeLayout(False)
        tlpInfo.PerformLayout()
        tlpButtons.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        '
        ' tips — etichetele de survolare (felia 0035). Toate în română.
        '
        tips.SetToolTipHeader(grid, "Unități posibile")
        tips.SetToolTipText(grid, "Fiecare rând este o unitate căreia îi poate aparține clasificația." & vbLf &
                                  "Alegeți-o pe cea potrivită; dublu-click confirmă direct.")
        tips.SetToolTipHeader(chkRetine, "Nu mă mai întreba")
        tips.SetToolTipText(chkRetine, "Ține minte răspunsul pentru ACEASTĂ combinație (sursă + clasificație)." & vbLf &
                                       "Data viitoare se aplică singur, fără să te mai întrebe." & vbLf &
                                       "O combinație nouă se întreabă oricum din nou.")
        tips.SetToolTipHeader(btnAlege, "Alege unitatea")
        tips.SetToolTipText(btnAlege, "Trimite din nou aceleași date, cu unitatea aleasă atașată.")
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Oprește salvarea. Nu s-a scris nimic și nimic nu se va scrie.")
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
