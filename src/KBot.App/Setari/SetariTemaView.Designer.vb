Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariTemaView
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        tlyTop = New KBotTableLayoutPanel()
        lblScheme = New Label()
        cboScheme = New KBotComboBox()
        lblSchemeState = New Label()
        btnReset = New Button()
        btnSave = New Button()
        grid = New PropertyGrid()
        tlyScaling = New KBotTableLayoutPanel()
        lblScalingTitle = New Label()
        lblScalingMode = New Label()
        cboScalingMode = New KBotComboBox()
        lblScalingFactor = New Label()
        numScalingFactor = New NumericUpDown()
        chkDpiUnaware = New CheckBox()
        lblTextScale = New Label()
        trkTextScale = New TrackBar()
        lblTextScaleValue = New Label()
        chkThemeFont = New CheckBox()
        lblFitBaseline = New Label()
        rdoFitScaled = New RadioButton()
        rdoFitRaw = New RadioButton()
        lblScalingHint = New Label()
        tlyTop.SuspendLayout()
        tlyScaling.SuspendLayout()
        CType(numScalingFactor, ComponentModel.ISupportInitialize).BeginInit()
        CType(trkTextScale, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' tlyTop
        '
        tlyTop.AutoFitToTheme = False
        tlyTop.AutoSize = True
        tlyTop.ColumnCount = 5
        tlyTop.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyTop.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 300F))
        tlyTop.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyTop.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyTop.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyTop.Controls.Add(lblScheme, 0, 0)
        tlyTop.Controls.Add(cboScheme, 1, 0)
        tlyTop.Controls.Add(lblSchemeState, 2, 0)
        tlyTop.Controls.Add(btnReset, 3, 0)
        tlyTop.Controls.Add(btnSave, 4, 0)
        tlyTop.Dock = DockStyle.Top
        tlyTop.Location = New Point(0, 0)
        tlyTop.Margin = New Padding(0)
        tlyTop.Name = "tlyTop"
        tlyTop.Padding = New Padding(24, 18, 24, 10)
        tlyTop.RowCount = 1
        tlyTop.RowStyles.Add(New RowStyle())
        tlyTop.Size = New Size(960, 78)
        tlyTop.TabIndex = 0
        '
        ' lblScheme
        '
        lblScheme.AutoSize = True
        lblScheme.Dock = DockStyle.Fill
        lblScheme.Location = New Point(28, 18)
        lblScheme.Margin = New Padding(4, 0, 4, 0)
        lblScheme.Name = "lblScheme"
        lblScheme.Size = New Size(102, 50)
        lblScheme.TabIndex = 0
        lblScheme.Text = "Schemă:"
        lblScheme.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboScheme
        '
        cboScheme.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboScheme.CornerRadius = 4
        cboScheme.DrawMode = DrawMode.OwnerDrawFixed
        cboScheme.DropDownStyle = ComboBoxStyle.DropDownList
        cboScheme.FlatStyle = FlatStyle.Flat
        cboScheme.ItemHeight = 30
        cboScheme.Location = New Point(138, 25)
        cboScheme.Margin = New Padding(4, 0, 4, 0)
        cboScheme.Name = "cboScheme"
        cboScheme.Size = New Size(292, 36)
        cboScheme.TabIndex = 1
        tips.SetToolTipHeader(cboScheme, "Schema editată")
        tips.SetToolTipText(cboScheme, "Alegerea schimbă ȘI tema activă a aplicației." & vbLf & "Așa vezi pe loc, pe ferestrele din spate, ce editezi.")
        '
        ' lblSchemeState
        '
        lblSchemeState.AutoEllipsis = True
        lblSchemeState.Dock = DockStyle.Fill
        lblSchemeState.Location = New Point(438, 18)
        lblSchemeState.Margin = New Padding(4, 0, 4, 0)
        lblSchemeState.Name = "lblSchemeState"
        lblSchemeState.Size = New Size(140, 50)
        lblSchemeState.TabIndex = 2
        lblSchemeState.Text = ""
        lblSchemeState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnReset
        '
        btnReset.Dock = DockStyle.Fill
        btnReset.FlatStyle = FlatStyle.Flat
        btnReset.Font = New Font("Segoe UI", 9F)
        btnReset.Location = New Point(590, 18)
        btnReset.Margin = New Padding(4, 0, 4, 0)
        btnReset.Name = "btnReset"
        btnReset.Size = New Size(212, 50)
        btnReset.TabIndex = 3
        btnReset.Text = "Restaurează implicit"
        tips.SetToolTipHeader(btnReset, "Înapoi la schema din program")
        tips.SetToolTipText(btnReset, "Șterge fișierul de personalizare al schemei și repune valorile compilate.")
        btnReset.UseVisualStyleBackColor = True
        '
        ' btnSave
        '
        btnSave.Dock = DockStyle.Fill
        btnSave.FlatStyle = FlatStyle.Flat
        btnSave.Font = New Font("Segoe UI Semibold", 9F)
        btnSave.Location = New Point(810, 18)
        btnSave.Margin = New Padding(4, 0, 4, 0)
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(122, 50)
        btnSave.TabIndex = 4
        btnSave.Text = "Salvează"
        tips.SetToolTipHeader(btnSave, "Păstrează modificările")
        tips.SetToolTipText(btnSave, "Scrie schema în AppData, ca s-o găsești și după repornire.")
        btnSave.UseVisualStyleBackColor = True
        '
        ' grid
        '
        grid.Dock = DockStyle.Fill
        grid.Location = New Point(0, 78)
        grid.Margin = New Padding(24, 0, 24, 0)
        grid.Name = "grid"
        grid.PropertySort = PropertySort.Categorized
        grid.Size = New Size(960, 420)
        grid.TabIndex = 1
        grid.ToolbarVisible = False
        '
        ' tlyScaling
        '
        tlyScaling.AutoFitToTheme = False
        tlyScaling.AutoSize = True
        tlyScaling.ColumnCount = 4
        tlyScaling.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyScaling.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 320F))
        tlyScaling.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90F))
        tlyScaling.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyScaling.Controls.Add(lblScalingTitle, 0, 0)
        tlyScaling.Controls.Add(lblScalingMode, 0, 1)
        tlyScaling.Controls.Add(cboScalingMode, 1, 1)
        tlyScaling.Controls.Add(lblScalingFactor, 2, 1)
        tlyScaling.Controls.Add(numScalingFactor, 3, 1)
        tlyScaling.Controls.Add(chkDpiUnaware, 1, 2)
        tlyScaling.Controls.Add(lblTextScale, 0, 3)
        tlyScaling.Controls.Add(trkTextScale, 1, 3)
        tlyScaling.Controls.Add(lblTextScaleValue, 2, 3)
        tlyScaling.Controls.Add(chkThemeFont, 3, 3)
        tlyScaling.Controls.Add(lblFitBaseline, 0, 4)
        tlyScaling.Controls.Add(rdoFitScaled, 1, 4)
        tlyScaling.Controls.Add(rdoFitRaw, 1, 5)
        tlyScaling.Controls.Add(lblScalingHint, 0, 6)
        tlyScaling.Dock = DockStyle.Bottom
        tlyScaling.Location = New Point(0, 498)
        tlyScaling.Margin = New Padding(0)
        tlyScaling.Name = "tlyScaling"
        tlyScaling.Padding = New Padding(24, 12, 24, 18)
        tlyScaling.RowCount = 7
        tlyScaling.RowStyles.Add(New RowStyle())
        tlyScaling.RowStyles.Add(New RowStyle())
        tlyScaling.RowStyles.Add(New RowStyle())
        tlyScaling.RowStyles.Add(New RowStyle())
        tlyScaling.RowStyles.Add(New RowStyle())
        tlyScaling.RowStyles.Add(New RowStyle())
        tlyScaling.RowStyles.Add(New RowStyle())
        tlyScaling.Size = New Size(960, 342)
        tlyScaling.TabIndex = 2
        '
        ' lblScalingTitle
        '
        lblScalingTitle.AutoSize = True
        tlyScaling.SetColumnSpan(lblScalingTitle, 4)
        lblScalingTitle.Font = New Font("Segoe UI Semibold", 12F)
        lblScalingTitle.Location = New Point(28, 12)
        lblScalingTitle.Margin = New Padding(4, 0, 4, 8)
        lblScalingTitle.Name = "lblScalingTitle"
        lblScalingTitle.Size = New Size(420, 32)
        lblScalingTitle.TabIndex = 0
        lblScalingTitle.Text = "Scalare (pentru toată aplicația, nu pe schemă)"
        '
        ' lblScalingMode
        '
        lblScalingMode.AutoSize = True
        lblScalingMode.Dock = DockStyle.Fill
        lblScalingMode.Location = New Point(28, 52)
        lblScalingMode.Margin = New Padding(4, 0, 4, 8)
        lblScalingMode.Name = "lblScalingMode"
        lblScalingMode.Size = New Size(122, 36)
        lblScalingMode.TabIndex = 1
        lblScalingMode.Text = "Mod:"
        lblScalingMode.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboScalingMode
        '
        cboScalingMode.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboScalingMode.CornerRadius = 4
        cboScalingMode.DrawMode = DrawMode.OwnerDrawFixed
        cboScalingMode.DropDownStyle = ComboBoxStyle.DropDownList
        cboScalingMode.FlatStyle = FlatStyle.Flat
        cboScalingMode.ItemHeight = 30
        cboScalingMode.Location = New Point(158, 52)
        cboScalingMode.Margin = New Padding(4, 0, 4, 8)
        cboScalingMode.Name = "cboScalingMode"
        cboScalingMode.Size = New Size(312, 36)
        cboScalingMode.TabIndex = 2
        tips.SetToolTipHeader(cboScalingMode, "Cum se scalează măsurile K-BOT")
        tips.SetToolTipText(cboScalingMode, "Automat — după DPI-ul ecranului (dintotdeauna)." & vbLf & "Fix 100% — geometria desenată rămâne cea din designer." & vbLf & "Manual — factorul de alături, pe orice ecran.")
        '
        ' lblScalingFactor
        '
        lblScalingFactor.AutoSize = True
        lblScalingFactor.Dock = DockStyle.Fill
        lblScalingFactor.Location = New Point(478, 52)
        lblScalingFactor.Margin = New Padding(4, 0, 4, 8)
        lblScalingFactor.Name = "lblScalingFactor"
        lblScalingFactor.Size = New Size(82, 36)
        lblScalingFactor.TabIndex = 3
        lblScalingFactor.Text = "Factor:"
        lblScalingFactor.TextAlign = ContentAlignment.MiddleRight
        '
        ' numScalingFactor
        '
        numScalingFactor.Anchor = AnchorStyles.Left
        numScalingFactor.DecimalPlaces = 2
        numScalingFactor.Increment = New Decimal(New Integer() {5, 0, 0, 131072})
        numScalingFactor.Location = New Point(568, 52)
        numScalingFactor.Margin = New Padding(4, 0, 4, 8)
        numScalingFactor.Maximum = New Decimal(New Integer() {4, 0, 0, 0})
        numScalingFactor.Minimum = New Decimal(New Integer() {5, 0, 0, 65536})
        numScalingFactor.Name = "numScalingFactor"
        numScalingFactor.Size = New Size(110, 31)
        numScalingFactor.TabIndex = 4
        numScalingFactor.Value = New Decimal(New Integer() {1, 0, 0, 0})
        tips.SetToolTipHeader(numScalingFactor, "Factorul manual")
        tips.SetToolTipText(numScalingFactor, "1,00 = măsurile de la 96 dpi. Are efect doar pe modul «Manual».")
        '
        ' chkDpiUnaware
        '
        chkDpiUnaware.AutoSize = True
        tlyScaling.SetColumnSpan(chkDpiUnaware, 3)
        chkDpiUnaware.Location = New Point(158, 96)
        chkDpiUnaware.Margin = New Padding(4, 0, 4, 8)
        chkDpiUnaware.Name = "chkDpiUnaware"
        chkDpiUnaware.Size = New Size(420, 29)
        chkDpiUnaware.TabIndex = 5
        chkDpiUnaware.Text = "Windows întinde fereastra (necesită repornire)"
        chkDpiUnaware.UseVisualStyleBackColor = True
        tips.SetToolTipHeader(chkDpiUnaware, "Proporții identice cu proiectarea")
        tips.SetToolTipText(chkDpiUnaware, "Aplicația devine surdă la DPI, iar Windows îi întinde fereastra ca pe o poză." & vbLf & "Ce vezi are EXACT proporțiile de la 100%, dar textul iese mai moale." & vbLf & "Modul DPI al unui proces nu se schimbă din mers — de aici repornirea.")
        '
        ' lblTextScale
        '
        lblTextScale.AutoSize = True
        lblTextScale.Dock = DockStyle.Fill
        lblTextScale.Location = New Point(28, 133)
        lblTextScale.Margin = New Padding(4, 0, 4, 8)
        lblTextScale.Name = "lblTextScale"
        lblTextScale.Size = New Size(122, 52)
        lblTextScale.TabIndex = 6
        lblTextScale.Text = "Mărime text:"
        lblTextScale.TextAlign = ContentAlignment.MiddleLeft
        '
        ' trkTextScale
        '
        trkTextScale.AutoSize = False
        trkTextScale.Dock = DockStyle.Fill
        trkTextScale.LargeChange = 10
        trkTextScale.Location = New Point(158, 133)
        trkTextScale.Margin = New Padding(4, 0, 4, 8)
        trkTextScale.Maximum = 200
        trkTextScale.Minimum = 75
        trkTextScale.Name = "trkTextScale"
        trkTextScale.Size = New Size(312, 52)
        trkTextScale.SmallChange = 5
        trkTextScale.TabIndex = 7
        trkTextScale.TickFrequency = 25
        trkTextScale.TickStyle = TickStyle.BottomRight
        trkTextScale.Value = 100
        tips.SetToolTipHeader(trkTextScale, "Mărimea textului și a controalelor")
        tips.SetToolTipText(trkTextScale, "Mărește literele ȘI controalele din jurul lor." & vbLf & "Cursorul se lipește la 100 %, 110 % și 125 %; același cursor stă și în meniul butonului de temă.")
        '
        ' lblTextScaleValue
        '
        lblTextScaleValue.Dock = DockStyle.Fill
        lblTextScaleValue.Location = New Point(478, 133)
        lblTextScaleValue.Margin = New Padding(4, 0, 4, 8)
        lblTextScaleValue.Name = "lblTextScaleValue"
        lblTextScaleValue.Size = New Size(82, 52)
        lblTextScaleValue.TabIndex = 8
        lblTextScaleValue.Text = "100%"
        lblTextScaleValue.TextAlign = ContentAlignment.MiddleRight
        '
        ' chkThemeFont
        '
        chkThemeFont.Anchor = AnchorStyles.Left
        chkThemeFont.AutoSize = True
        chkThemeFont.Checked = True
        chkThemeFont.CheckState = CheckState.Checked
        chkThemeFont.Location = New Point(568, 144)
        chkThemeFont.Margin = New Padding(12, 0, 4, 8)
        chkThemeFont.Name = "chkThemeFont"
        chkThemeFont.Size = New Size(180, 29)
        chkThemeFont.TabIndex = 13
        chkThemeFont.Text = "Font din temă"
        chkThemeFont.UseVisualStyleBackColor = True
        tips.SetToolTipHeader(chkThemeFont, "Font din temă")
        tips.SetToolTipText(chkThemeFont, "Bifat: tema scrie fontul ei de bază pe fiecare fereastră, iar cursorul de mărime are efect." & vbLf & "Debifat: ferestrele rămân cu fontul din designer, iar cursorul dispare din meniul butonului de temă.")
        '
        ' lblFitBaseline
        '
        lblFitBaseline.AutoSize = True
        lblFitBaseline.Dock = DockStyle.Fill
        lblFitBaseline.Location = New Point(28, 193)
        lblFitBaseline.Margin = New Padding(4, 0, 4, 0)
        lblFitBaseline.Name = "lblFitBaseline"
        lblFitBaseline.Size = New Size(122, 29)
        lblFitBaseline.TabIndex = 9
        lblFitBaseline.Text = "Baza ferestrei:"
        lblFitBaseline.TextAlign = ContentAlignment.MiddleLeft
        '
        ' rdoFitScaled
        '
        rdoFitScaled.AutoSize = True
        rdoFitScaled.Checked = True
        tlyScaling.SetColumnSpan(rdoFitScaled, 3)
        rdoFitScaled.Location = New Point(158, 193)
        rdoFitScaled.Margin = New Padding(4, 0, 4, 0)
        rdoFitScaled.Name = "rdoFitScaled"
        rdoFitScaled.Size = New Size(420, 29)
        rdoFitScaled.TabIndex = 10
        rdoFitScaled.TabStop = True
        rdoFitScaled.Text = "urmează scalarea (mărimea de pornire crește cu textul)"
        rdoFitScaled.UseVisualStyleBackColor = True
        tips.SetToolTipHeader(rdoFitScaled, "Baza urmează cursorul de scalare")
        tips.SetToolTipText(rdoFitScaled, "Fereastra nu scade niciodată sub mărimea ei de pornire, iar acea mărime crește și scade odată cu textul și cu ecranul.")
        '
        ' rdoFitRaw
        '
        rdoFitRaw.AutoSize = True
        tlyScaling.SetColumnSpan(rdoFitRaw, 3)
        rdoFitRaw.Location = New Point(158, 222)
        rdoFitRaw.Margin = New Padding(4, 0, 4, 8)
        rdoFitRaw.Name = "rdoFitRaw"
        rdoFitRaw.Size = New Size(420, 29)
        rdoFitRaw.TabIndex = 11
        rdoFitRaw.Text = "rămâne cea din designer (pixeli bruți)"
        rdoFitRaw.UseVisualStyleBackColor = True
        tips.SetToolTipHeader(rdoFitRaw, "Baza înghețată în pixeli")
        tips.SetToolTipText(rdoFitRaw, "Mărimea de pornire rămâne cea de la deschidere, oricât s-ar schimba scalarea; fereastra crește doar cât îi cere conținutul.")
        '
        ' lblScalingHint
        '
        lblScalingHint.AutoSize = True
        tlyScaling.SetColumnSpan(lblScalingHint, 4)
        lblScalingHint.Dock = DockStyle.Fill
        lblScalingHint.Location = New Point(28, 259)
        lblScalingHint.Margin = New Padding(4, 0, 4, 0)
        lblScalingHint.Name = "lblScalingHint"
        lblScalingHint.Size = New Size(904, 50)
        lblScalingHint.TabIndex = 12
        lblScalingHint.Text = "«Fix 100%» oprește scalarea măsurilor NOASTRE; fonturile le scalează în continuare Windows, deci la 150% textul rămâne mai mare decât geometria din jur. Pentru proporții identice cu proiectarea, bifează întinderea de mai sus."
        '
        ' SetariTemaView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(grid)
        Controls.Add(tlyScaling)
        Controls.Add(tlyTop)
        Name = "SetariTemaView"
        Size = New Size(960, 840)
        tlyTop.ResumeLayout(False)
        tlyTop.PerformLayout()
        tlyScaling.ResumeLayout(False)
        tlyScaling.PerformLayout()
        CType(numScalingFactor, ComponentModel.ISupportInitialize).EndInit()
        CType(trkTextScale, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyTop As KBotTableLayoutPanel
    Friend WithEvents lblScheme As Label
    Friend WithEvents cboScheme As KBotComboBox
    Friend WithEvents lblSchemeState As Label
    Friend WithEvents btnReset As Button
    Friend WithEvents btnSave As Button
    Friend WithEvents grid As PropertyGrid
    Friend WithEvents tlyScaling As KBotTableLayoutPanel
    Friend WithEvents lblScalingTitle As Label
    Friend WithEvents lblScalingMode As Label
    Friend WithEvents cboScalingMode As KBotComboBox
    Friend WithEvents lblScalingFactor As Label
    Friend WithEvents numScalingFactor As NumericUpDown
    Friend WithEvents chkDpiUnaware As CheckBox
    Friend WithEvents lblTextScale As Label
    Friend WithEvents trkTextScale As TrackBar
    Friend WithEvents lblTextScaleValue As Label
    Friend WithEvents chkThemeFont As CheckBox
    Friend WithEvents lblFitBaseline As Label
    Friend WithEvents rdoFitScaled As RadioButton
    Friend WithEvents rdoFitRaw As RadioButton
    Friend WithEvents lblScalingHint As Label
End Class
