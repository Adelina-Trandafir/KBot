' The VALUE CORRECTION window of one snapshot (slice 0111).
'
' Every control is declared HERE, like every form in K-BOT (docs/kbot-forms-ui-convention.md):
' the designer draws them, the code behind builds none. The children of pnlCard are added in
' REVERSE dock order -- body (Fill) first, then the bottom bar, then the title bar -- because
' WinForms puts the last-added closest to the edge. Same in pnlJos: the last added (the
' primary button) is the closest to the right edge.
'
' Drawn at 96 dpi (AutoScaleDimensions = 96); AutoScaleMode.Dpi scales it to the real screen.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CorectieValoareForm
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
        Dim KBotDataColumn1 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn2 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn3 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        tips = New KBot.Controls.KBotToolTip(components)
        pnlCard = New Panel()
        tlyCorp = New KBot.Controls.KBotTableLayoutPanel()
        lblAntet = New Label()
        grdValori = New Controls.KBotDataView()
        lblSume = New Label()
        lblMotivCaption = New Label()
        txtMotiv = New KBot.Controls.KBotTextField()
        pnlJos = New Panel()
        btnRevino = New Button()
        lblSpatiu1 = New Label()
        btnRenunta = New Button()
        lblSpatiu2 = New Label()
        btnSalveaza = New Button()
        capBar = New Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlyCorp.SuspendLayout()
        CType(grdValori, ComponentModel.ISupportInitialize).BeginInit()
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
        pnlCard.Margin = New Padding(3)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(636, 516)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' tlyCorp
        '
        tlyCorp.ColumnCount = 1
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCorp.Controls.Add(lblAntet, 0, 0)
        tlyCorp.Controls.Add(grdValori, 0, 1)
        tlyCorp.Controls.Add(lblSume, 0, 2)
        tlyCorp.Controls.Add(lblMotivCaption, 0, 3)
        tlyCorp.Controls.Add(txtMotiv, 0, 4)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 40)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(12, 8, 12, 4)
        tlyCorp.RowCount = 5
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 96F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 24F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyCorp.Size = New Size(636, 420)
        tlyCorp.TabIndex = 1
        '
        ' lblAntet
        '
        lblAntet.Dock = DockStyle.Fill
        lblAntet.Location = New Point(15, 8)
        lblAntet.Margin = New Padding(3, 0, 3, 0)
        lblAntet.Name = "lblAntet"
        lblAntet.Size = New Size(606, 96)
        lblAntet.TabIndex = 0
        lblAntet.Text = "FOREXE a scris în istoric o valoare care nu e cea reală. Totalul propus este suma liniilor pe indicatori; modificați totalul sau liniile dacă e nevoie. Cifrele din FOREXE se păstrează și se pot readuce oricând. Totalul trebuie să fie egal cu suma liniilor."
        lblAntet.TextAlign = ContentAlignment.MiddleLeft
        '
        ' grdValori
        '
        grdValori.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        grdValori.BackColor = SystemColors.Window
        grdValori.ColumnFillMode = KBot.Controls.KBotFillMode.FirstColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Rând"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "nume"
        KBotDataColumn1.MinWidth = 120
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 220
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn2.DecimalPlaces = 2
        KBotDataColumn2.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Din FOREXE"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "forexe"
        KBotDataColumn2.MinWidth = 90
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn2.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn2.Width = 130
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Valoare corectă *"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "valoare"
        KBotDataColumn3.MinWidth = 90
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn3.Width = 150
        grdValori.Columns.Add(KBotDataColumn1)
        grdValori.Columns.Add(KBotDataColumn2)
        grdValori.Columns.Add(KBotDataColumn3)
        grdValori.Dock = DockStyle.Fill
        grdValori.EnterKeyMode = KBot.Controls.KBotEnterKeyMode.NextRow
        grdValori.Location = New Point(15, 104)
        grdValori.Margin = New Padding(3, 4, 3, 4)
        grdValori.Name = "grdValori"
        grdValori.ShrinkColumnsToFit = False
        grdValori.Size = New Size(606, 204)
        grdValori.TabIndex = 1
        tips.SetToolTipHeader(grdValori, "Valorile instantaneului")
        tips.SetToolTipText(grdValori, "Prima linie e totalul recepției la acel moment, apoi câte o linie pe indicator." & vbLf & "Totalul propus e suma liniilor și o urmărește cât timp modificați liniile; dacă îl tastați chiar dumneavoastră, rămâne cum l-ați pus. Se tastează doar în coloana «Valoare corectă». Totalul trebuie să fie egal cu suma liniilor.")
        '
        ' lblSume
        '
        lblSume.Dock = DockStyle.Fill
        lblSume.Location = New Point(15, 312)
        lblSume.Margin = New Padding(3, 0, 3, 0)
        lblSume.Name = "lblSume"
        lblSume.Size = New Size(606, 52)
        lblSume.TabIndex = 2
        lblSume.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblMotivCaption
        '
        lblMotivCaption.Dock = DockStyle.Fill
        lblMotivCaption.Location = New Point(15, 364)
        lblMotivCaption.Margin = New Padding(3, 0, 3, 0)
        lblMotivCaption.Name = "lblMotivCaption"
        lblMotivCaption.Size = New Size(606, 24)
        lblMotivCaption.TabIndex = 3
        lblMotivCaption.Text = "Motivul corecției *"
        lblMotivCaption.TextAlign = ContentAlignment.BottomLeft
        '
        ' txtMotiv
        '
        txtMotiv.BackColor = Color.Transparent
        txtMotiv.Dock = DockStyle.Fill
        txtMotiv.Location = New Point(15, 391)
        txtMotiv.Margin = New Padding(3, 2, 3, 2)
        txtMotiv.Name = "txtMotiv"
        txtMotiv.Size = New Size(606, 36)
        txtMotiv.TabIndex = 4
        txtMotiv.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtMotiv, "Motivul corecției")
        tips.SetToolTipText(txtMotiv, "Obligatoriu, cel mult 500 de caractere." & vbLf & "Se păstrează lângă instantaneu, cu numele dumneavoastră și data.")
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnRevino)
        pnlJos.Controls.Add(lblSpatiu1)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Controls.Add(lblSpatiu2)
        pnlJos.Controls.Add(btnSalveaza)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 460)
        pnlJos.Margin = New Padding(3)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(12, 8, 12, 14)
        pnlJos.Size = New Size(636, 56)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        '
        ' btnRevino
        '
        btnRevino.Dock = DockStyle.Left
        btnRevino.FlatStyle = FlatStyle.Flat
        btnRevino.Location = New Point(12, 8)
        btnRevino.Margin = New Padding(0)
        btnRevino.Name = "btnRevino"
        btnRevino.Size = New Size(210, 34)
        btnRevino.TabIndex = 2
        btnRevino.Text = "Revino la valorile din FOREXE"
        tips.SetToolTipHeader(btnRevino, "Revino la valorile din FOREXE")
        tips.SetToolTipText(btnRevino, "Pune în coloana «Valoare corectă» valorile pe care le-a dat FOREXE." & vbLf & "Nu se scrie nimic până nu apăsați «Salvează».")
        btnRevino.UseVisualStyleBackColor = True
        '
        ' lblSpatiu1
        '
        lblSpatiu1.Dock = DockStyle.Right
        lblSpatiu1.Location = New Point(404, 8)
        lblSpatiu1.Margin = New Padding(3, 0, 3, 0)
        lblSpatiu1.Name = "lblSpatiu1"
        lblSpatiu1.Size = New Size(8, 34)
        lblSpatiu1.TabIndex = 3
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Right
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(412, 8)
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(104, 34)
        btnRenunta.TabIndex = 1
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să schimbe nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' lblSpatiu2
        '
        lblSpatiu2.Dock = DockStyle.Right
        lblSpatiu2.Location = New Point(516, 8)
        lblSpatiu2.Margin = New Padding(3, 0, 3, 0)
        lblSpatiu2.Name = "lblSpatiu2"
        lblSpatiu2.Size = New Size(8, 34)
        lblSpatiu2.TabIndex = 4
        '
        ' btnSalveaza
        '
        btnSalveaza.Dock = DockStyle.Right
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(524, 8)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New Size(100, 34)
        btnSalveaza.TabIndex = 0
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează corecția")
        tips.SetToolTipText(btnSalveaza, "Scrie totalul și liniile corectate, cu motivul, numele dumneavoastră și data." & vbLf & "Valorile din FOREXE rămân păstrate.")
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(3)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New Size(636, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Corectează valoarea"
        '
        ' CorectieValoareForm
        '
        AcceptButton = btnSalveaza
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(640, 520)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(3)
        MaximizeBox = False
        MinimizeBox = False
        Name = "CorectieValoareForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Corectează valoarea"
        pnlCard.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        CType(grdValori, ComponentModel.ISupportInitialize).EndInit()
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents tlyCorp As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblAntet As Label
    Friend WithEvents grdValori As Global.KBot.Controls.KBotDataView
    Friend WithEvents lblSume As Label
    Friend WithEvents lblMotivCaption As Label
    Friend WithEvents txtMotiv As Global.KBot.Controls.KBotTextField
    Friend WithEvents pnlJos As Panel
    Friend WithEvents btnRevino As Button
    Friend WithEvents lblSpatiu1 As Label
    Friend WithEvents btnRenunta As Button
    Friend WithEvents lblSpatiu2 As Label
    Friend WithEvents btnSalveaza As Button
End Class
