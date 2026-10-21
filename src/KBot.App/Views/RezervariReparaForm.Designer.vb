' The REPAIR PREVIEW window of the reservations of one angajament (slice 0114-25).
'
' Every control is declared HERE, like every form in K-BOT (docs/kbot-forms-ui-convention.md):
' the designer draws them, the code behind builds none. The children of pnlCard are added in
' REVERSE dock order -- body (Fill) first, then the bottom bar, then the title bar -- because
' WinForms puts the last-added closest to the edge. Same in pnlJos: the primary button (right)
' is added after the cancel button (left).
'
' Drawn at 96 dpi (AutoScaleDimensions = 96); AutoScaleMode.Dpi scales it to the real screen.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RezervariReparaForm
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
        Dim KBotDataColumn4 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn7 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn8 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn9 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn10 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        tips = New KBot.Controls.KBotToolTip(components)
        pnlCard = New Panel()
        tlyCorp = New KBot.Controls.KBotTableLayoutPanel()
        lblConstatare = New Label()
        lblGreseliCaption = New Label()
        grdGreseli = New Controls.KBotDataView()
        lblPlanCaption = New Label()
        lblPlan = New Label()
        grdPlan = New Controls.KBotDataView()
        txtNote = New TextBox()
        pnlJos = New Panel()
        btnRenunta = New Button()
        btnRepara = New Button()
        capBar = New Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlyCorp.SuspendLayout()
        CType(grdGreseli, ComponentModel.ISupportInitialize).BeginInit()
        CType(grdPlan, ComponentModel.ISupportInitialize).BeginInit()
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
        pnlCard.Size = New Size(776, 676)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' tlyCorp
        '
        tlyCorp.ColumnCount = 1
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCorp.Controls.Add(lblConstatare, 0, 0)
        tlyCorp.Controls.Add(lblGreseliCaption, 0, 1)
        tlyCorp.Controls.Add(grdGreseli, 0, 2)
        tlyCorp.Controls.Add(lblPlanCaption, 0, 3)
        tlyCorp.Controls.Add(lblPlan, 0, 4)
        tlyCorp.Controls.Add(grdPlan, 0, 5)
        tlyCorp.Controls.Add(txtNote, 0, 6)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 40)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(12, 8, 12, 4)
        tlyCorp.RowCount = 7
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 26F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 40F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 26F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 60F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 72F))
        tlyCorp.Size = New Size(776, 580)
        tlyCorp.TabIndex = 1
        '
        ' lblConstatare
        '
        lblConstatare.Dock = DockStyle.Fill
        lblConstatare.Location = New Point(15, 8)
        lblConstatare.Margin = New Padding(3, 0, 3, 0)
        lblConstatare.Name = "lblConstatare"
        lblConstatare.Size = New Size(746, 56)
        lblConstatare.TabIndex = 0
        lblConstatare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblGreseliCaption
        '
        lblGreseliCaption.Dock = DockStyle.Fill
        lblGreseliCaption.Location = New Point(15, 64)
        lblGreseliCaption.Margin = New Padding(3, 0, 3, 0)
        lblGreseliCaption.Name = "lblGreseliCaption"
        lblGreseliCaption.Size = New Size(746, 26)
        lblGreseliCaption.TabIndex = 1
        lblGreseliCaption.Text = "Ce s-a detectat — rezervările care nu pornesc de la valoarea la care s-a oprit cea dinainte"
        lblGreseliCaption.TextAlign = ContentAlignment.BottomLeft
        '
        ' grdGreseli
        '
        grdGreseli.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        grdGreseli.BackColor = SystemColors.Window
        grdGreseli.ColumnFillMode = KBot.Controls.KBotFillMode.FirstColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Ziua rezervării"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "data"
        KBotDataColumn1.MinWidth = 100
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 160
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Indicator"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "indicator"
        KBotDataColumn2.MinWidth = 80
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 120
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Anterioară acum"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "anterioara"
        KBotDataColumn3.MinWidth = 100
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn3.Width = 150
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Ar trebui să fie"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "asteptata"
        KBotDataColumn4.MinWidth = 100
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn4.Width = 150
        grdGreseli.Columns.Add(KBotDataColumn1)
        grdGreseli.Columns.Add(KBotDataColumn2)
        grdGreseli.Columns.Add(KBotDataColumn3)
        grdGreseli.Columns.Add(KBotDataColumn4)
        grdGreseli.Dock = DockStyle.Fill
        grdGreseli.Location = New Point(15, 90)
        grdGreseli.Margin = New Padding(3, 4, 3, 4)
        grdGreseli.Name = "grdGreseli"
        grdGreseli.ShrinkColumnsToFit = False
        grdGreseli.Size = New Size(746, 145)
        grdGreseli.TabIndex = 2
        ' tips.SetToolTipHeader(grdGreseli, "Rezervările greșite")
        ' tips.SetToolTipText(grdGreseli, "Aceleași rânduri care fac angajamentul roșu în lista principală." & vbLf & "«Anterioară acum» e ce ține rezervarea; «Ar trebui să fie» e valoarea la care s-a oprit rezervarea dinainte, pe același indicator.")
        '
        ' lblPlanCaption
        '
        lblPlanCaption.Dock = DockStyle.Fill
        lblPlanCaption.Location = New Point(15, 239)
        lblPlanCaption.Margin = New Padding(3, 0, 3, 0)
        lblPlanCaption.Name = "lblPlanCaption"
        lblPlanCaption.Size = New Size(746, 26)
        lblPlanCaption.TabIndex = 3
        lblPlanCaption.Text = "Ce se va face — rezervările care își schimbă valoarea"
        lblPlanCaption.TextAlign = ContentAlignment.BottomLeft
        '
        ' lblPlan
        '
        lblPlan.Dock = DockStyle.Fill
        lblPlan.Location = New Point(15, 265)
        lblPlan.Margin = New Padding(3, 0, 3, 0)
        lblPlan.Name = "lblPlan"
        lblPlan.Size = New Size(746, 56)
        lblPlan.TabIndex = 4
        lblPlan.TextAlign = ContentAlignment.MiddleLeft
        '
        ' grdPlan
        '
        grdPlan.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        grdPlan.BackColor = SystemColors.Window
        grdPlan.ColumnFillMode = KBot.Controls.KBotFillMode.FirstColumn
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Ziua"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "data"
        KBotDataColumn5.MinWidth = 90
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 110
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Indicator"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "indicator"
        KBotDataColumn6.MinWidth = 70
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 100
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Valoare acum"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "valoare_veche"
        KBotDataColumn7.MinWidth = 90
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn7.Width = 130
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn8.DecimalPlaces = 2
        KBotDataColumn8.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Valoare nouă"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "valoare_noua"
        KBotDataColumn8.MinWidth = 90
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn8.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn8.Width = 130
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Anterioară acum"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "anterioara_veche"
        KBotDataColumn9.MinWidth = 90
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn9.Width = 130
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn10.DecimalPlaces = 2
        KBotDataColumn10.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Anterioară nouă"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "anterioara_noua"
        KBotDataColumn10.MinWidth = 90
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn10.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn10.Width = 130
        grdPlan.Columns.Add(KBotDataColumn5)
        grdPlan.Columns.Add(KBotDataColumn6)
        grdPlan.Columns.Add(KBotDataColumn7)
        grdPlan.Columns.Add(KBotDataColumn8)
        grdPlan.Columns.Add(KBotDataColumn9)
        grdPlan.Columns.Add(KBotDataColumn10)
        grdPlan.Dock = DockStyle.Fill
        grdPlan.Location = New Point(15, 325)
        grdPlan.Margin = New Padding(3, 4, 3, 4)
        grdPlan.Name = "grdPlan"
        grdPlan.ShrinkColumnsToFit = False
        grdPlan.Size = New Size(746, 217)
        grdPlan.TabIndex = 5
        ' tips.SetToolTipHeader(grdPlan, "Ce se schimbă")
        ' tips.SetToolTipText(grdPlan, "Fiecare rând e o rezervare care se rescrie: valoarea ei și valoarea anterioară, din vechi în nou." & vbLf & "Nimic nu se scrie până nu apăsați «Repară».")
        '
        ' txtNote
        '
        txtNote.BorderStyle = BorderStyle.FixedSingle
        txtNote.Dock = DockStyle.Fill
        txtNote.Location = New Point(15, 546)
        txtNote.Margin = New Padding(3, 2, 3, 2)
        txtNote.Multiline = True
        txtNote.Name = "txtNote"
        txtNote.ReadOnly = True
        txtNote.ScrollBars = ScrollBars.Vertical
        txtNote.Size = New Size(746, 68)
        txtNote.TabIndex = 6
        txtNote.TabStop = False
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Controls.Add(btnRepara)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 620)
        pnlJos.Margin = New Padding(3)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(12, 8, 12, 14)
        pnlJos.Size = New Size(776, 56)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Left
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(12, 8)
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(104, 34)
        btnRenunta.TabIndex = 1
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să schimbe nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnRepara
        '
        btnRepara.DialogResult = DialogResult.OK
        btnRepara.Dock = DockStyle.Right
        btnRepara.FlatStyle = FlatStyle.Flat
        btnRepara.Location = New Point(664, 8)
        btnRepara.Margin = New Padding(0)
        btnRepara.Name = "btnRepara"
        btnRepara.Size = New Size(100, 34)
        btnRepara.TabIndex = 0
        btnRepara.Text = "Repară"
        tips.SetToolTipHeader(btnRepara, "Repară rezervările")
        tips.SetToolTipText(btnRepara, "Scrie corecturile arătate mai sus, într-o singură operație." & vbLf & "Nu se descarcă nimic din FOREXE.")
        btnRepara.UseVisualStyleBackColor = True
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
        capBar.Size = New Size(776, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Repară rezervările"
        '
        ' RezervariReparaForm
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(780, 680)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(3)
        MaximizeBox = False
        MinimizeBox = False
        Name = "RezervariReparaForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Repară rezervările"
        pnlCard.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        tlyCorp.PerformLayout()
        CType(grdGreseli, ComponentModel.ISupportInitialize).EndInit()
        CType(grdPlan, ComponentModel.ISupportInitialize).EndInit()
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents tlyCorp As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblConstatare As Label
    Friend WithEvents lblGreseliCaption As Label
    Friend WithEvents grdGreseli As Global.KBot.Controls.KBotDataView
    Friend WithEvents lblPlanCaption As Label
    Friend WithEvents lblPlan As Label
    Friend WithEvents grdPlan As Global.KBot.Controls.KBotDataView
    Friend WithEvents txtNote As TextBox
    Friend WithEvents pnlJos As Panel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnRepara As Button
End Class
