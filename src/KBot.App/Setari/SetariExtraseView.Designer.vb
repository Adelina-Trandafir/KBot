Imports KBot.Controls

' The «Extrase» page of the settings window (slice 0080-02): which columns the four statement
' grids show, and in what order. A combo picks the grid; the list below it is that grid's
' catalogue, ticked = shown, top to bottom = left to right. All controls are declared HERE
' (docs/kbot-forms-ui-convention.md). Coordinates are in the 144 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariExtraseView
    Inherits Global.KBot.Theming.KBotThemedUserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyBody = New KBotTableLayoutPanel()
        lblTitlu = New Label()
        lblHint = New Label()
        tlyGrila = New KBotTableLayoutPanel()
        lblGrila = New Label()
        cboGrila = New KBotComboBox()
        grila = New KBotDataView()
        tlyButoane = New KBotTableLayoutPanel()
        btnSus = New Button()
        btnJos = New Button()
        btnImplicite = New Button()
        btnSalveaza = New Button()
        tlyBody.SuspendLayout()
        tlyGrila.SuspendLayout()
        CType(grila, ComponentModel.ISupportInitialize).BeginInit()
        tlyButoane.SuspendLayout()
        SuspendLayout()
        '
        ' tlyBody
        '
        tlyBody.ColumnCount = 1
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBody.Controls.Add(lblTitlu, 0, 0)
        tlyBody.Controls.Add(lblHint, 0, 1)
        tlyBody.Controls.Add(tlyGrila, 0, 2)
        tlyBody.Controls.Add(grila, 0, 3)
        tlyBody.Controls.Add(tlyButoane, 0, 4)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(0, 0)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.Padding = New Padding(24, 18, 24, 18)
        tlyBody.RowCount = 5
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.Size = New Size(1400, 840)
        tlyBody.TabIndex = 0
        '
        ' lblTitlu
        '
        lblTitlu.AutoSize = True
        lblTitlu.Font = New Font("Segoe UI Semibold", 12F)
        lblTitlu.Location = New Point(28, 18)
        lblTitlu.Margin = New Padding(4, 0, 4, 8)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.Size = New Size(245, 32)
        lblTitlu.TabIndex = 0
        lblTitlu.Text = "Coloanele extraselor de cont"
        '
        ' lblHint
        '
        lblHint.AutoSize = True
        lblHint.Dock = DockStyle.Fill
        lblHint.Location = New Point(28, 58)
        lblHint.Margin = New Padding(4, 0, 4, 12)
        lblHint.Name = "lblHint"
        lblHint.Size = New Size(1336, 52)
        lblHint.TabIndex = 1
        lblHint.Text = "Alegeți grila, bifați coloanele de afișat și ordonați-le cu «Sus» / «Jos» (de sus în jos = de la stânga la dreapta). Vederea Extrase și fereastra «Extrase de cont» au fiecare coloanele ei. Nimic nu se aplică până la «Salvează»."
        '
        ' tlyGrila
        '
        tlyGrila.AutoFitToTheme = False
        tlyGrila.AutoSize = True
        tlyGrila.ColumnCount = 2
        tlyGrila.ColumnStyles.Add(New ColumnStyle())
        tlyGrila.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 520F))
        tlyGrila.Controls.Add(lblGrila, 0, 0)
        tlyGrila.Controls.Add(cboGrila, 1, 0)
        tlyGrila.Dock = DockStyle.Top
        tlyGrila.Location = New Point(24, 122)
        tlyGrila.Margin = New Padding(0, 0, 0, 12)
        tlyGrila.Name = "tlyGrila"
        tlyGrila.RowCount = 1
        tlyGrila.RowStyles.Add(New RowStyle())
        tlyGrila.Size = New Size(1352, 44)
        tlyGrila.TabIndex = 2
        '
        ' lblGrila
        '
        lblGrila.Anchor = AnchorStyles.Left
        lblGrila.AutoSize = True
        lblGrila.Location = New Point(4, 8)
        lblGrila.Margin = New Padding(4, 0, 12, 0)
        lblGrila.Name = "lblGrila"
        lblGrila.Size = New Size(52, 25)
        lblGrila.TabIndex = 0
        lblGrila.Text = "Grila"
        '
        ' cboGrila
        '
        cboGrila.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboGrila.CornerRadius = 4
        cboGrila.DrawMode = DrawMode.OwnerDrawFixed
        cboGrila.DropDownStyle = ComboBoxStyle.DropDownList
        cboGrila.FlatStyle = FlatStyle.Flat
        cboGrila.ItemHeight = 31
        cboGrila.Location = New Point(72, 2)
        cboGrila.Margin = New Padding(4, 0, 4, 0)
        cboGrila.Name = "cboGrila"
        cboGrila.Size = New Size(512, 37)
        cboGrila.TabIndex = 1
        tips.SetToolTipHeader(cboGrila, "Grila")
        tips.SetToolTipText(cboGrila, "Care dintre cele patru grile de extrase se editează acum." & vbLf & "Modificările fiecăreia se păstrează până la «Salvează».")
        '
        ' grila
        '
        grila.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grila.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Afișată"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "afisata"
        KBotDataColumn1.MinWidth = 40
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 70
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Coloana"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "coloana"
        KBotDataColumn2.MinWidth = 100
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 300
        grila.Columns.Add(KBotDataColumn1)
        grila.Columns.Add(KBotDataColumn2)
        grila.Dock = DockStyle.Fill
        grila.FillColumnKey = "coloana"
        grila.HeaderHeight = 24
        grila.Location = New Point(28, 178)
        grila.Margin = New Padding(4, 0, 4, 12)
        grila.Name = "grila"
        grila.Size = New Size(1344, 580)
        grila.TabIndex = 3
        tips.SetToolTipHeader(grila, "Coloanele grilei")
        tips.SetToolTipText(grila, "Bifă = coloana se vede. Ordinea de aici e ordinea din grilă, de la stânga la dreapta." & vbLf & "Cel puțin o coloană rămâne bifată.")
        '
        ' tlyButoane
        '
        tlyButoane.AutoFitToTheme = False
        tlyButoane.AutoSize = True
        tlyButoane.ColumnCount = 5
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.Controls.Add(btnSus, 0, 0)
        tlyButoane.Controls.Add(btnJos, 1, 0)
        tlyButoane.Controls.Add(btnImplicite, 2, 0)
        tlyButoane.Controls.Add(btnSalveaza, 4, 0)
        tlyButoane.Dock = DockStyle.Top
        tlyButoane.Location = New Point(28, 770)
        tlyButoane.Margin = New Padding(4, 0, 4, 0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.RowCount = 1
        tlyButoane.RowStyles.Add(New RowStyle())
        tlyButoane.Size = New Size(1344, 52)
        tlyButoane.TabIndex = 4
        '
        ' btnSus
        '
        btnSus.AutoSize = True
        btnSus.FlatStyle = FlatStyle.Flat
        btnSus.Location = New Point(0, 0)
        btnSus.Margin = New Padding(0, 0, 6, 0)
        btnSus.Name = "btnSus"
        btnSus.Padding = New Padding(6, 3, 6, 3)
        btnSus.Size = New Size(110, 46)
        btnSus.TabIndex = 0
        btnSus.Text = "Sus"
        tips.SetToolTipHeader(btnSus, "Mută în sus")
        tips.SetToolTipText(btnSus, "Coloana selectată urcă un loc, adică se mută spre stânga în grilă.")
        btnSus.UseVisualStyleBackColor = True
        '
        ' btnJos
        '
        btnJos.AutoSize = True
        btnJos.FlatStyle = FlatStyle.Flat
        btnJos.Location = New Point(116, 0)
        btnJos.Margin = New Padding(0, 0, 6, 0)
        btnJos.Name = "btnJos"
        btnJos.Padding = New Padding(6, 3, 6, 3)
        btnJos.Size = New Size(110, 46)
        btnJos.TabIndex = 1
        btnJos.Text = "Jos"
        tips.SetToolTipHeader(btnJos, "Mută în jos")
        tips.SetToolTipText(btnJos, "Coloana selectată coboară un loc, adică se mută spre dreapta în grilă.")
        btnJos.UseVisualStyleBackColor = True
        '
        ' btnImplicite
        '
        btnImplicite.AutoSize = True
        btnImplicite.FlatStyle = FlatStyle.Flat
        btnImplicite.Location = New Point(232, 0)
        btnImplicite.Margin = New Padding(0, 0, 6, 0)
        btnImplicite.Name = "btnImplicite"
        btnImplicite.Padding = New Padding(6, 3, 6, 3)
        btnImplicite.Size = New Size(170, 46)
        btnImplicite.TabIndex = 2
        btnImplicite.Text = "Revino la implicit"
        tips.SetToolTipHeader(btnImplicite, "Coloanele implicite")
        tips.SetToolTipText(btnImplicite, "Pune înapoi coloanele de la început ale grilei alese, în ordinea lor." & vbLf & "Nimic nu se salvează până la «Salvează».")
        btnImplicite.UseVisualStyleBackColor = True
        '
        ' btnSalveaza
        '
        btnSalveaza.AutoSize = True
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(1188, 0)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(8, 3, 8, 3)
        btnSalveaza.Size = New Size(156, 46)
        btnSalveaza.TabIndex = 3
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Scrie alegerea pentru toate cele patru grile." & vbLf & "Vederea Extrase și fereastra «Extrase de cont» o preiau pe loc.")
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' SetariExtraseView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyBody)
        Name = "SetariExtraseView"
        Size = New Size(1400, 840)
        tlyBody.ResumeLayout(False)
        tlyBody.PerformLayout()
        tlyGrila.ResumeLayout(False)
        tlyGrila.PerformLayout()
        CType(grila, ComponentModel.ISupportInitialize).EndInit()
        tlyButoane.ResumeLayout(False)
        tlyButoane.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents lblTitlu As Label
    Friend WithEvents lblHint As Label
    Friend WithEvents tlyGrila As KBotTableLayoutPanel
    Friend WithEvents lblGrila As Label
    Friend WithEvents cboGrila As KBotComboBox
    Friend WithEvents grila As KBotDataView
    Friend WithEvents tlyButoane As KBotTableLayoutPanel
    Friend WithEvents btnSus As Button
    Friend WithEvents btnJos As Button
    Friend WithEvents btnImplicite As Button
    Friend WithEvents btnSalveaza As Button
End Class
