Option Strict On

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ThemeOptionsForm
    Inherits KBotThemedForm

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        pnlTop = New Panel()
        lblScheme = New Label()
        cboScheme = New KBotComboBox()
        lblSchemeState = New Label()
        grid = New PropertyGrid()
        pnlScaling = New Panel()
        lblScalingTitle = New Label()
        lblScalingMode = New Label()
        cboScalingMode = New KBotComboBox()
        lblScalingFactor = New Label()
        numScalingFactor = New NumericUpDown()
        chkDpiUnaware = New CheckBox()
        lblTextScale = New Label()
        trkTextScale = New TrackBar()
        lblTextScaleValue = New Label()
        lblScalingHint = New Label()
        pnlBottom = New Panel()
        lblStatus = New Label()
        btnReset = New Button()
        btnSave = New Button()
        btnClose = New Button()
        tips = New KBotToolTip(components)
        pnlTop.SuspendLayout()
        pnlScaling.SuspendLayout()
        CType(numScalingFactor, ComponentModel.ISupportInitialize).BeginInit()
        CType(trkTextScale, ComponentModel.ISupportInitialize).BeginInit()
        pnlBottom.SuspendLayout()
        SuspendLayout()
        '
        ' lblScheme
        '
        lblScheme.AutoSize = True
        lblScheme.Location = New Point(12, 17)
        lblScheme.Name = "lblScheme"
        lblScheme.Size = New Size(53, 15)
        lblScheme.TabIndex = 0
        lblScheme.Text = "Schemă:"
        '
        ' cboScheme
        '
        cboScheme.Location = New Point(90, 12)
        cboScheme.Name = "cboScheme"
        cboScheme.Size = New Size(220, 26)
        cboScheme.TabIndex = 1
        tips.SetToolTipHeader(cboScheme, "Schema editată")
        tips.SetToolTipText(cboScheme, "Alegerea schimbă ȘI tema activă a aplicației." & Global.Microsoft.VisualBasic.ChrW(10) &
                                       "Așa vezi pe loc, pe ferestrele din spate, ce editezi.")
        '
        ' lblSchemeState
        '
        lblSchemeState.AutoEllipsis = True
        lblSchemeState.Location = New Point(320, 16)
        lblSchemeState.Name = "lblSchemeState"
        lblSchemeState.Size = New Size(250, 18)
        lblSchemeState.TabIndex = 2
        lblSchemeState.Text = ""
        '
        ' pnlTop
        '
        pnlTop.Controls.Add(lblSchemeState)
        pnlTop.Controls.Add(cboScheme)
        pnlTop.Controls.Add(lblScheme)
        pnlTop.Dock = DockStyle.Top
        pnlTop.Location = New Point(0, 0)
        pnlTop.Name = "pnlTop"
        pnlTop.Size = New Size(584, 50)
        pnlTop.TabIndex = 0
        '
        ' grid
        '
        grid.Dock = DockStyle.Fill
        grid.Location = New Point(0, 50)
        grid.Name = "grid"
        grid.PropertySort = PropertySort.Categorized
        grid.Size = New Size(584, 260)
        grid.TabIndex = 1
        grid.ToolbarVisible = False
        '
        ' lblScalingTitle
        '
        lblScalingTitle.AutoSize = True
        lblScalingTitle.Location = New Point(12, 10)
        lblScalingTitle.Name = "lblScalingTitle"
        lblScalingTitle.Size = New Size(220, 15)
        lblScalingTitle.TabIndex = 0
        lblScalingTitle.Text = "Scalare (pentru toată aplicația, nu pe schemă)"
        '
        ' lblScalingMode
        '
        lblScalingMode.AutoSize = True
        lblScalingMode.Location = New Point(12, 40)
        lblScalingMode.Name = "lblScalingMode"
        lblScalingMode.Size = New Size(38, 15)
        lblScalingMode.TabIndex = 1
        lblScalingMode.Text = "Mod:"
        '
        ' cboScalingMode
        '
        cboScalingMode.Location = New Point(90, 35)
        cboScalingMode.Name = "cboScalingMode"
        cboScalingMode.Size = New Size(220, 26)
        cboScalingMode.TabIndex = 2
        tips.SetToolTipHeader(cboScalingMode, "Cum se scalează măsurile K-BOT")
        tips.SetToolTipText(cboScalingMode, "Automat — după DPI-ul ecranului (dintotdeauna)." & Global.Microsoft.VisualBasic.ChrW(10) &
                                            "Fix 100% — geometria desenată rămâne cea din designer." & Global.Microsoft.VisualBasic.ChrW(10) &
                                            "Manual — factorul de alături, pe orice ecran.")
        '
        ' lblScalingFactor
        '
        lblScalingFactor.AutoSize = True
        lblScalingFactor.Location = New Point(324, 40)
        lblScalingFactor.Name = "lblScalingFactor"
        lblScalingFactor.Size = New Size(45, 15)
        lblScalingFactor.TabIndex = 3
        lblScalingFactor.Text = "Factor:"
        '
        ' numScalingFactor
        '
        numScalingFactor.DecimalPlaces = 2
        numScalingFactor.Increment = New Decimal(New Integer() {5, 0, 0, 131072})
        numScalingFactor.Location = New Point(400, 36)
        numScalingFactor.Maximum = New Decimal(New Integer() {4, 0, 0, 0})
        numScalingFactor.Minimum = New Decimal(New Integer() {5, 0, 0, 65536})
        numScalingFactor.Name = "numScalingFactor"
        numScalingFactor.Size = New Size(80, 23)
        numScalingFactor.TabIndex = 4
        numScalingFactor.Value = New Decimal(New Integer() {1, 0, 0, 0})
        tips.SetToolTipHeader(numScalingFactor, "Factorul manual")
        tips.SetToolTipText(numScalingFactor, "1,00 = măsurile de la 96 dpi. Are efect doar pe modul «Manual».")
        '
        ' chkDpiUnaware
        '
        chkDpiUnaware.AutoSize = True
        chkDpiUnaware.Location = New Point(90, 70)
        chkDpiUnaware.Name = "chkDpiUnaware"
        chkDpiUnaware.Size = New Size(300, 19)
        chkDpiUnaware.TabIndex = 5
        chkDpiUnaware.Text = "Windows întinde fereastra (necesită repornire)"
        chkDpiUnaware.UseVisualStyleBackColor = True
        tips.SetToolTipHeader(chkDpiUnaware, "Proporții identice cu proiectarea")
        tips.SetToolTipText(chkDpiUnaware, "Aplicația devine surdă la DPI, iar Windows îi întinde fereastra ca pe o poză." & Global.Microsoft.VisualBasic.ChrW(10) &
                                           "Ce vezi are EXACT proporțiile de la 100%, dar textul iese mai moale." & Global.Microsoft.VisualBasic.ChrW(10) &
                                           "Modul DPI al unui proces nu se schimbă din mers — de aici repornirea.")
        '
        ' lblTextScale
        '
        lblTextScale.AutoSize = True
        lblTextScale.Location = New Point(12, 102)
        lblTextScale.Name = "lblTextScale"
        lblTextScale.Size = New Size(76, 15)
        lblTextScale.TabIndex = 6
        lblTextScale.Text = "Mărime text:"
        '
        ' trkTextScale
        '
        trkTextScale.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        trkTextScale.AutoSize = False
        trkTextScale.LargeChange = 10
        trkTextScale.Location = New Point(90, 96)
        trkTextScale.Maximum = 200
        trkTextScale.Minimum = 75
        trkTextScale.Name = "trkTextScale"
        trkTextScale.Size = New Size(400, 34)
        trkTextScale.SmallChange = 5
        trkTextScale.TabIndex = 7
        trkTextScale.TickFrequency = 25
        trkTextScale.TickStyle = TickStyle.BottomRight
        trkTextScale.Value = 100
        tips.SetToolTipHeader(trkTextScale, "Mărimea textului și a controalelor")
        tips.SetToolTipText(trkTextScale, "Mărește literele ȘI controalele din jurul lor." & Global.Microsoft.VisualBasic.ChrW(10) &
                                          "Același cursor stă și în meniul butonului de temă.")
        '
        ' lblTextScaleValue
        '
        lblTextScaleValue.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblTextScaleValue.Location = New Point(496, 102)
        lblTextScaleValue.Name = "lblTextScaleValue"
        lblTextScaleValue.Size = New Size(60, 20)
        lblTextScaleValue.TabIndex = 8
        lblTextScaleValue.Text = "100%"
        '
        ' lblScalingHint
        '
        lblScalingHint.Location = New Point(12, 136)
        lblScalingHint.Name = "lblScalingHint"
        lblScalingHint.Size = New Size(560, 32)
        lblScalingHint.TabIndex = 9
        lblScalingHint.Text = "«Fix 100%» oprește scalarea măsurilor NOASTRE; fonturile le scalează în continuare Windows, deci la 150% textul rămâne mai mare decât geometria din jur. Pentru proporții identice cu proiectarea, bifează întinderea de mai sus."
        '
        ' pnlScaling
        '
        pnlScaling.Controls.Add(lblScalingHint)
        pnlScaling.Controls.Add(lblTextScaleValue)
        pnlScaling.Controls.Add(trkTextScale)
        pnlScaling.Controls.Add(lblTextScale)
        pnlScaling.Controls.Add(chkDpiUnaware)
        pnlScaling.Controls.Add(numScalingFactor)
        pnlScaling.Controls.Add(lblScalingFactor)
        pnlScaling.Controls.Add(cboScalingMode)
        pnlScaling.Controls.Add(lblScalingMode)
        pnlScaling.Controls.Add(lblScalingTitle)
        pnlScaling.Dock = DockStyle.Bottom
        pnlScaling.Location = New Point(0, 310)
        pnlScaling.Name = "pnlScaling"
        pnlScaling.Size = New Size(584, 176)
        pnlScaling.TabIndex = 2
        '
        ' lblStatus
        '
        lblStatus.AutoEllipsis = True
        lblStatus.Location = New Point(12, 18)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(230, 20)
        lblStatus.TabIndex = 0
        lblStatus.Text = ""
        '
        ' btnReset
        '
        btnReset.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnReset.Location = New Point(248, 13)
        btnReset.Name = "btnReset"
        btnReset.Size = New Size(150, 28)
        btnReset.TabIndex = 1
        btnReset.Text = "Restaurează implicit"
        btnReset.UseVisualStyleBackColor = True
        tips.SetToolTipHeader(btnReset, "Înapoi la schema din program")
        tips.SetToolTipText(btnReset, "Șterge fișierul de personalizare al schemei și repune valorile compilate.")
        '
        ' btnSave
        '
        btnSave.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnSave.Location = New Point(404, 13)
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(84, 28)
        btnSave.TabIndex = 2
        btnSave.Text = "Salvează"
        btnSave.UseVisualStyleBackColor = True
        tips.SetToolTipHeader(btnSave, "Păstrează modificările")
        tips.SetToolTipText(btnSave, "Scrie schema în AppData, ca s-o găsești și după repornire.")
        '
        ' btnClose
        '
        btnClose.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnClose.Location = New Point(494, 13)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(78, 28)
        btnClose.TabIndex = 3
        btnClose.Text = "Închide"
        btnClose.UseVisualStyleBackColor = True
        '
        ' pnlBottom
        '
        pnlBottom.Controls.Add(btnClose)
        pnlBottom.Controls.Add(btnSave)
        pnlBottom.Controls.Add(btnReset)
        pnlBottom.Controls.Add(lblStatus)
        pnlBottom.Dock = DockStyle.Bottom
        pnlBottom.Location = New Point(0, 446)
        pnlBottom.Name = "pnlBottom"
        pnlBottom.Size = New Size(584, 54)
        pnlBottom.TabIndex = 3
        '
        ' ThemeOptionsForm
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(584, 540)
        ' Ordine INVERSĂ de andocare (regula casei): Fill primul, apoi Bottom-urile — cel adăugat
        ' mai devreme rămâne mai jos — și Top la urmă.
        Controls.Add(grid)
        Controls.Add(pnlBottom)
        Controls.Add(pnlScaling)
        Controls.Add(pnlTop)
        FormBorderStyle = FormBorderStyle.SizableToolWindow
        MinimizeBox = False
        MinimumSize = New Size(560, 500)
        Name = "ThemeOptionsForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Opțiuni de temă"
        pnlTop.ResumeLayout(False)
        pnlTop.PerformLayout()
        pnlScaling.ResumeLayout(False)
        pnlScaling.PerformLayout()
        CType(numScalingFactor, ComponentModel.ISupportInitialize).EndInit()
        CType(trkTextScale, ComponentModel.ISupportInitialize).EndInit()
        pnlBottom.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlTop As Panel
    Friend WithEvents lblScheme As Label
    Friend WithEvents cboScheme As KBotComboBox
    Friend WithEvents lblSchemeState As Label
    Friend WithEvents grid As PropertyGrid
    Friend WithEvents pnlScaling As Panel
    Friend WithEvents lblScalingTitle As Label
    Friend WithEvents lblScalingMode As Label
    Friend WithEvents cboScalingMode As KBotComboBox
    Friend WithEvents lblScalingFactor As Label
    Friend WithEvents numScalingFactor As NumericUpDown
    Friend WithEvents chkDpiUnaware As CheckBox
    Friend WithEvents lblTextScale As Label
    Friend WithEvents trkTextScale As TrackBar
    Friend WithEvents lblTextScaleValue As Label
    Friend WithEvents lblScalingHint As Label
    Friend WithEvents pnlBottom As Panel
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnReset As Button
    Friend WithEvents btnSave As Button
    Friend WithEvents btnClose As Button
    Friend WithEvents tips As KBotToolTip
End Class
