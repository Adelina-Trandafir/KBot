Imports KBot.Controls

' Slice 00EF-48 -- the page «Trimite la Vercon» of the E-Factura ribbon. All the controls are declared HERE (docs/kbot-forms-ui-convention.md).
' Left: the TREE of the received invoices of the year (by month and supplier, or by supplier), a tick on every row, a tick on a month or a supplier ticks all its invoices.
' Right: the lines of the ticked invoices, the account of VERCON proposed on each (or once on the header of an invoice), the note for the Administrator and the buttons.
' Children are added in reverse dock order (Fill first, then the edges). The text of the labels and of the note is 2 pt larger than the base font (operator, 21.10.2026).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VerconForm
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
        components = New System.ComponentModel.Container()
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotGroupLevel1 As KBotGroupLevel = New KBotGroupLevel()
        tips = New KBotToolTip(components)
        capBar = New KBotCaptionBar()
        pnlCard = New System.Windows.Forms.Panel()
        pnlStanga = New System.Windows.Forms.Panel()
        tree = New AdvancedTreeControl()
        lblStatus = New System.Windows.Forms.Label()
        pnlDreapta = New System.Windows.Forms.Panel()
        gridLinii = New KBotDataView()
        lblRezumat = New System.Windows.Forms.Label()
        ntfMesaj = New KBotNotice()
        pnlJos = New System.Windows.Forms.Panel()
        btnRetrage = New KBotButton()
        pnlGap = New System.Windows.Forms.Panel()
        btnTrimite = New KBotButton()
        pnlNota = New System.Windows.Forms.Panel()
        txtNota = New KBotTextBox()
        lblNota = New System.Windows.Forms.Label()
        lblAntet = New System.Windows.Forms.Label()
        CType(gridLinii, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlCard.SuspendLayout()
        pnlStanga.SuspendLayout()
        pnlDreapta.SuspendLayout()
        pnlJos.SuspendLayout()
        pnlNota.SuspendLayout()
        SuspendLayout()
        '
        ' capBar
        '
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_e_64
        capBar.Location = New System.Drawing.Point(3, 3)
        capBar.Margin = New System.Windows.Forms.Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowOptionsButton = False
        capBar.ShowTextScaleSlider = False
        capBar.Size = New System.Drawing.Size(1194, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Trimite la Vercon"
        '
        ' tree: the received invoices (month > supplier > invoice, or supplier > invoice); a tick on every row; the options button of the header switches the grouping
        '
        tree.BorderColor = Drawing.SystemColors.ActiveBorder
        tree.CheckBoxes = True
        tree.Dock = System.Windows.Forms.DockStyle.Fill
        tree.ExpanderSize = 10
        tree.Font = New System.Drawing.Font("Calibri", 9.0!, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        tree.FooterBackColor = Drawing.SystemColors.Control
        tree.FooterCaption = "TOTAL"
        tree.FooterCaptionBackColor = Drawing.SystemColors.Control
        tree.FooterCaptionFont = New System.Drawing.Font("Calibri", 9.0!, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        tree.FooterCaptionForeColor = Drawing.SystemColors.ActiveCaptionText
        tree.FooterHeight = 30
        tree.FooterSeparatorColor = Drawing.Color.Gainsboro
        tree.FooterSeparatorWidth = 2
        tree.FooterTextAlign = Drawing.ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HasNodeIcons = False
        tree.HeaderBackColor = Drawing.SystemColors.Control
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " FACTURI PRIMITE"
        tree.HeaderFont = New System.Drawing.Font("Calibri", 9.0!, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        tree.HeaderForeColor = Drawing.Color.Black
        tree.HeaderHeight = 30
        tree.HeaderIconSize = New System.Drawing.Size(18, 18)
        tree.HeaderRightIcon = My.Resources.Resources.settings
        tree.HeaderRightIconTooltip = "Opțiunile arborelui: grupare după lună sau după furnizor."
        tree.HeaderSearchIcon = My.Resources.Resources.search
        tree.HeaderSearchIconTooltip = "Deschide banda de căutare peste arbore." & vbLf & "ESC golește căutarea și o închide."
        tree.HeaderSeparatorColor = Drawing.Color.Gainsboro
        tree.HeaderSeparatorWidth = 2
        tree.HeaderVisible = True
        tree.Indent = 8
        tree.ItemHeight = 24
        tree.Location = New System.Drawing.Point(0, 0)
        tree.Margin = New System.Windows.Forms.Padding(0)
        tree.Name = "tree"
        tree.PaddingExpanderGap = 8
        tree.PaddingIconGap = 8
        tree.PaddingTreeEnd = 8
        tree.PaddingTreeStart = 8
        tree.RightTextWidth = 90
        tree.SearchBarFont = New System.Drawing.Font("Calibri", 9.0!)
        tree.SearchBoxBackColor = Drawing.SystemColors.Control
        tree.SearchClearButton = True
        tree.SearchClearButtonHoverColor = Drawing.SystemColors.Control
        tree.SearchDefaultText = "... furnizor sau număr factură, minim 3 caractere ..."
        tree.SearchSeparatorColor = Drawing.SystemColors.ActiveBorder
        tree.SearchSeparatorWidth = 2
        tree.Size = New System.Drawing.Size(600, 640)
        tree.TabIndex = 0
        '
        ' lblStatus
        '
        lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom
        lblStatus.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblStatus.Location = New System.Drawing.Point(0, 640)
        lblStatus.Margin = New System.Windows.Forms.Padding(0)
        lblStatus.Name = "lblStatus"
        lblStatus.Padding = New System.Windows.Forms.Padding(4, 0, 0, 0)
        lblStatus.Size = New System.Drawing.Size(600, 30)
        lblStatus.TabIndex = 1
        lblStatus.Text = "Nimic bifat."
        lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' pnlStanga
        '
        pnlStanga.Controls.Add(tree)
        pnlStanga.Controls.Add(lblStatus)
        pnlStanga.Dock = System.Windows.Forms.DockStyle.Left
        pnlStanga.Location = New System.Drawing.Point(12, 9)
        pnlStanga.Margin = New System.Windows.Forms.Padding(0)
        pnlStanga.Name = "pnlStanga"
        pnlStanga.Padding = New System.Windows.Forms.Padding(0, 0, 8, 0)
        pnlStanga.Size = New System.Drawing.Size(608, 670)
        pnlStanga.TabIndex = 1
        '
        ' lblAntet
        '
        lblAntet.Dock = System.Windows.Forms.DockStyle.Top
        lblAntet.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblAntet.Location = New System.Drawing.Point(0, 0)
        lblAntet.Margin = New System.Windows.Forms.Padding(0)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New System.Windows.Forms.Padding(8, 4, 8, 6)
        lblAntet.Size = New System.Drawing.Size(562, 96)
        lblAntet.TabIndex = 0
        '
        ' lblNota
        '
        lblNota.Dock = System.Windows.Forms.DockStyle.Left
        lblNota.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblNota.Location = New System.Drawing.Point(8, 4)
        lblNota.Margin = New System.Windows.Forms.Padding(0)
        lblNota.Name = "lblNota"
        lblNota.Size = New System.Drawing.Size(190, 36)
        lblNota.TabIndex = 0
        lblNota.Text = "Notă pentru Administrator"
        lblNota.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtNota
        '
        txtNota.Dock = System.Windows.Forms.DockStyle.Fill
        txtNota.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        txtNota.Location = New System.Drawing.Point(198, 4)
        txtNota.Margin = New System.Windows.Forms.Padding(0)
        txtNota.MaxLength = 500
        txtNota.Name = "txtNota"
        txtNota.Size = New System.Drawing.Size(356, 36)
        txtNota.TabIndex = 1
        tips.SetToolTipHeader(txtNota, "Notă pentru Administrator")
        tips.SetToolTipText(txtNota, "Un text scurt pe care Administratorul îl vede când preia factura (cel mult 500 de caractere). Se salvează o singură dată, la trimitere.")
        '
        ' pnlNota
        '
        pnlNota.Controls.Add(txtNota)
        pnlNota.Controls.Add(lblNota)
        pnlNota.Dock = System.Windows.Forms.DockStyle.Top
        pnlNota.Location = New System.Drawing.Point(0, 96)
        pnlNota.Margin = New System.Windows.Forms.Padding(0)
        pnlNota.Name = "pnlNota"
        pnlNota.Padding = New System.Windows.Forms.Padding(8, 4, 8, 4)
        pnlNota.Size = New System.Drawing.Size(562, 44)
        pnlNota.TabIndex = 1
        '
        ' gridLinii: one row per line of the ticked invoices, grouped by invoice; the account of VERCON is chosen from the tree (gestiune -> accounts), on the line or once on the header of the invoice
        '
        gridLinii.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridLinii.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Factura"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "factura"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn1.Width = 220
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Nr."
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "nr"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 48
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Denumire"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "denumire"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 260
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Cantitate"
        KBotDataColumn4.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "cant"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn4.Width = 100
        KBotDataColumn5.Aggregate = KBotAggregate.Sum
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Valoare cu TVA"
        KBotDataColumn5.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "cuTva"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 120
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.ColumnType = KBotColumnType.Combo
        KBotDataColumn6.CommitOnSelect = True
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.GroupHeaderCombo = True
        KBotDataColumn6.HeaderText = "Cont Vercon"
        KBotDataColumn6.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "cont"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.Width = 300
        gridLinii.Columns.Add(KBotDataColumn1)
        gridLinii.Columns.Add(KBotDataColumn2)
        gridLinii.Columns.Add(KBotDataColumn3)
        gridLinii.Columns.Add(KBotDataColumn4)
        gridLinii.Columns.Add(KBotDataColumn5)
        gridLinii.Columns.Add(KBotDataColumn6)
        gridLinii.Dock = System.Windows.Forms.DockStyle.Fill
        gridLinii.FillColumnKey = "denumire"
        gridLinii.FooterCaption = "TOTAL"
        gridLinii.FooterVisible = True
        KBotGroupLevel1.ColumnKey = "factura"
        KBotGroupLevel1.FooterCaptionFormat = "{1}"
        KBotGroupLevel1.HeaderBorder = True
        KBotGroupLevel1.HeaderCaptionFormat = "{1}"
        KBotGroupLevel1.ShowFooter = False
        KBotGroupLevel1.ShowFooterAggregates = False
        KBotGroupLevel1.ShowHeaderAggregates = True
        KBotGroupLevel1.SortDirection = KBotSortDirection.Ascending
        gridLinii.Groups.Add(KBotGroupLevel1)
        gridLinii.Location = New System.Drawing.Point(0, 140)
        gridLinii.Margin = New System.Windows.Forms.Padding(0)
        gridLinii.Name = "gridLinii"
        gridLinii.Size = New System.Drawing.Size(562, 394)
        gridLinii.TabIndex = 2
        '
        ' lblRezumat
        '
        lblRezumat.Dock = System.Windows.Forms.DockStyle.Bottom
        lblRezumat.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblRezumat.Location = New System.Drawing.Point(0, 534)
        lblRezumat.Margin = New System.Windows.Forms.Padding(0)
        lblRezumat.Name = "lblRezumat"
        lblRezumat.Padding = New System.Windows.Forms.Padding(8, 4, 8, 4)
        lblRezumat.Size = New System.Drawing.Size(562, 52)
        lblRezumat.TabIndex = 3
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = System.Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 586)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(0)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(562, 30)
        ntfMesaj.TabIndex = 4
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' btnRetrage: takes back a sending that still waits (shown only while one waiting invoice is ticked)
        '
        btnRetrage.Dock = System.Windows.Forms.DockStyle.Left
        btnRetrage.Location = New System.Drawing.Point(0, 8)
        btnRetrage.Margin = New System.Windows.Forms.Padding(0)
        btnRetrage.Name = "btnRetrage"
        btnRetrage.Size = New System.Drawing.Size(200, 50)
        btnRetrage.TabIndex = 0
        btnRetrage.Text = "Retrage de la Vercon"
        btnRetrage.Visible = False
        tips.SetToolTipHeader(btnRetrage, "Retrage de la Vercon")
        tips.SetToolTipText(btnRetrage, "Ia factura înapoi cât timp Administratorul nu a preluat-o: dispare din lista lui și o poți trimite din nou oricând.")
        '
        ' pnlGap
        '
        pnlGap.Dock = System.Windows.Forms.DockStyle.Right
        pnlGap.Location = New System.Drawing.Point(342, 8)
        pnlGap.Margin = New System.Windows.Forms.Padding(0)
        pnlGap.Name = "pnlGap"
        pnlGap.Size = New System.Drawing.Size(10, 50)
        pnlGap.TabIndex = 1
        '
        ' btnTrimite
        '
        btnTrimite.Dock = System.Windows.Forms.DockStyle.Right
        btnTrimite.Enabled = False
        btnTrimite.Location = New System.Drawing.Point(352, 8)
        btnTrimite.Margin = New System.Windows.Forms.Padding(0)
        btnTrimite.Name = "btnTrimite"
        btnTrimite.Size = New System.Drawing.Size(210, 50)
        btnTrimite.TabIndex = 2
        btnTrimite.Text = "Trimite la Vercon"
        tips.SetToolTipHeader(btnTrimite, "Trimite la Vercon")
        tips.SetToolTipText(btnTrimite, "Trimite facturile <b>bifate</b> cu TOATE liniile lor și conturile alese (o linie fără cont rămâne pentru Administrator). Administratorul le preia într-un NIR.")
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnRetrage)
        pnlJos.Controls.Add(pnlGap)
        pnlJos.Controls.Add(btnTrimite)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 616)
        pnlJos.Margin = New System.Windows.Forms.Padding(0)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(0, 8, 0, 4)
        pnlJos.Size = New System.Drawing.Size(562, 58)
        pnlJos.TabIndex = 5
        '
        ' pnlDreapta
        '
        pnlDreapta.Controls.Add(gridLinii)
        pnlDreapta.Controls.Add(lblRezumat)
        pnlDreapta.Controls.Add(ntfMesaj)
        pnlDreapta.Controls.Add(pnlJos)
        pnlDreapta.Controls.Add(pnlNota)
        pnlDreapta.Controls.Add(lblAntet)
        pnlDreapta.Dock = System.Windows.Forms.DockStyle.Fill
        pnlDreapta.Location = New System.Drawing.Point(620, 9)
        pnlDreapta.Margin = New System.Windows.Forms.Padding(0)
        pnlDreapta.Name = "pnlDreapta"
        pnlDreapta.Size = New System.Drawing.Size(562, 670)
        pnlDreapta.TabIndex = 0
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(pnlDreapta)
        pnlCard.Controls.Add(pnlStanga)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(3, 69)
        pnlCard.Margin = New System.Windows.Forms.Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New System.Windows.Forms.Padding(12, 9, 12, 9)
        pnlCard.Size = New System.Drawing.Size(1194, 688)
        pnlCard.TabIndex = 1
        pnlCard.Tag = "Card"
        '
        ' VerconForm
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        ClientSize = New System.Drawing.Size(1200, 760)
        Controls.Add(pnlCard)
        Controls.Add(capBar)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(1100, 700)
        Name = "VerconForm"
        Padding = New System.Windows.Forms.Padding(3)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "E-Factura — trimite la Vercon"
        CType(gridLinii, System.ComponentModel.ISupportInitialize).EndInit()
        pnlCard.ResumeLayout(False)
        pnlStanga.ResumeLayout(False)
        pnlDreapta.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        pnlNota.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents pnlStanga As System.Windows.Forms.Panel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents pnlDreapta As System.Windows.Forms.Panel
    Friend WithEvents lblAntet As System.Windows.Forms.Label
    Friend WithEvents pnlNota As System.Windows.Forms.Panel
    Friend WithEvents lblNota As System.Windows.Forms.Label
    Friend WithEvents txtNota As KBotTextBox
    Friend WithEvents gridLinii As KBotDataView
    Friend WithEvents lblRezumat As System.Windows.Forms.Label
    Friend WithEvents ntfMesaj As KBotNotice
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnRetrage As KBotButton
    Friend WithEvents pnlGap As System.Windows.Forms.Panel
    Friend WithEvents btnTrimite As KBotButton
End Class
