Imports KBot.Controls

' Pagina «Beneficiari» a editorului de ordonantare (felia 0049) — portul lui
' `frmFX_ORD_PART` + subformularul continuu `frmFX_ORD_TBL`.
'
' Cele doua formulare Access devin trei zone:
'   stanga    = lista beneficiarilor (Access: `lstDenBene`), cu randul sintetic
'               «< TOTI BENEFICIARII >» si cu bifa `chkClsf` deasupra;
'   dreapta sus = campurile beneficiarului selectat (DenBene / CodFiscal / ContIBAN / Banca)
'               plus alegatorul de partener (Access: combo-ul `CodPartener`);
'   dreapta jos = grila liniilor de plata (Access: `frmFX_ORD_TBL`), cu rand de totaluri.
'
' `btnClsf` din `frmFX_ORD_TBL` NU se porteaza: in tot exportul Access nu are niciun
' `btnClsf_Click`, iar singura lui aparitie e in `PositionElements`, o functie care incepe cu
' `Exit Function`. Un buton fara comportament ar fi un no-op tacut (interzis de regulile casei).
'
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md): pagina trebuie sa se
' randeze in designerul Visual Studio, nu sa se construiasca la rulare.
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OrdBeneficiariPage
    Inherits System.Windows.Forms.UserControl

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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        split = New SplitContainer()
        tlyStanga = New TableLayoutPanel()
        chkClsf = New CheckBox()
        grdStanga = New KBotDataView()
        tlyDreapta = New TableLayoutPanel()
        tlyAntetBene = New TableLayoutPanel()
        lblDenBene = New Label()
        txtDenBene = New KBotTextField()
        lblCodPartener = New Label()
        cboCodPartener = New KBotComboBox()
        lblCodFiscal = New Label()
        txtCodFiscal = New KBotTextField()
        lblContIban = New Label()
        txtContIban = New KBotTextField()
        lblBanca = New Label()
        txtBanca = New KBotTextField()
        grdLinii = New KBotDataView()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        tlyStanga.SuspendLayout()
        CType(grdStanga, ComponentModel.ISupportInitialize).BeginInit()
        tlyDreapta.SuspendLayout()
        tlyAntetBene.SuspendLayout()
        CType(grdLinii, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' split
        '
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Name = "split"
        '
        ' split.Panel1
        '
        split.Panel1.Controls.Add(tlyStanga)
        split.Panel1MinSize = 180
        '
        ' split.Panel2
        '
        split.Panel2.Controls.Add(tlyDreapta)
        split.Panel2MinSize = 380
        split.Size = New Size(980, 520)
        split.SplitterDistance = 260
        split.SplitterWidth = 6
        split.TabIndex = 0
        '
        ' tlyStanga
        '
        tlyStanga.ColumnCount = 1
        tlyStanga.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyStanga.Controls.Add(grdStanga, 0, 1)
        tlyStanga.Controls.Add(chkClsf, 0, 0)
        tlyStanga.Dock = DockStyle.Fill
        tlyStanga.Location = New Point(0, 0)
        tlyStanga.Margin = New Padding(0)
        tlyStanga.Name = "tlyStanga"
        tlyStanga.RowCount = 2
        tlyStanga.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tlyStanga.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyStanga.Size = New Size(260, 520)
        tlyStanga.TabIndex = 0
        '
        ' chkClsf
        '
        chkClsf.AutoSize = True
        chkClsf.Dock = DockStyle.Fill
        chkClsf.Location = New Point(6, 4)
        chkClsf.Margin = New Padding(6, 4, 6, 2)
        chkClsf.Name = "chkClsf"
        chkClsf.Size = New Size(248, 24)
        chkClsf.TabIndex = 0
        chkClsf.Text = "Grupează pe clasificații"
        chkClsf.UseVisualStyleBackColor = True
        '
        ' grdStanga
        '
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn1.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.HeaderText = "Beneficiari"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn1.Key = "eticheta"
        KBotDataColumn1.MinWidth = 80
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn1.Width = 240
        grdStanga.Columns.Add(KBotDataColumn1)
        grdStanga.ColumnFillMode = KBotFillMode.SpecificColumn
        grdStanga.Dock = DockStyle.Fill
        grdStanga.FillColumnKey = "eticheta"
        grdStanga.FooterVisible = False
        grdStanga.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdStanga.Location = New Point(0, 30)
        grdStanga.Margin = New Padding(0)
        grdStanga.Name = "grdStanga"
        grdStanga.ReadOnlyGrid = True
        grdStanga.RowHeight = 22
        grdStanga.Size = New Size(260, 490)
        grdStanga.TabIndex = 1
        '
        ' tlyDreapta
        '
        tlyDreapta.ColumnCount = 1
        tlyDreapta.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyDreapta.Controls.Add(grdLinii, 0, 1)
        tlyDreapta.Controls.Add(tlyAntetBene, 0, 0)
        tlyDreapta.Dock = DockStyle.Fill
        tlyDreapta.Location = New Point(0, 0)
        tlyDreapta.Margin = New Padding(0)
        tlyDreapta.Name = "tlyDreapta"
        tlyDreapta.RowCount = 2
        tlyDreapta.RowStyles.Add(New RowStyle(SizeType.Absolute, 116F))
        tlyDreapta.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyDreapta.Size = New Size(714, 520)
        tlyDreapta.TabIndex = 0
        '
        ' tlyAntetBene
        '
        tlyAntetBene.ColumnCount = 4
        tlyAntetBene.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlyAntetBene.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 55F))
        tlyAntetBene.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyAntetBene.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 45F))
        tlyAntetBene.Controls.Add(lblDenBene, 0, 0)
        tlyAntetBene.Controls.Add(txtDenBene, 1, 0)
        tlyAntetBene.Controls.Add(lblCodPartener, 2, 0)
        tlyAntetBene.Controls.Add(cboCodPartener, 3, 0)
        tlyAntetBene.Controls.Add(lblCodFiscal, 0, 1)
        tlyAntetBene.Controls.Add(txtCodFiscal, 1, 1)
        tlyAntetBene.Controls.Add(lblContIban, 2, 1)
        tlyAntetBene.Controls.Add(txtContIban, 3, 1)
        tlyAntetBene.Controls.Add(lblBanca, 0, 2)
        tlyAntetBene.Controls.Add(txtBanca, 1, 2)
        tlyAntetBene.Dock = DockStyle.Fill
        tlyAntetBene.Location = New Point(3, 3)
        tlyAntetBene.Name = "tlyAntetBene"
        tlyAntetBene.RowCount = 3
        tlyAntetBene.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyAntetBene.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyAntetBene.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyAntetBene.Size = New Size(708, 110)
        tlyAntetBene.TabIndex = 0
        '
        ' lblDenBene
        '
        lblDenBene.AutoSize = True
        lblDenBene.Dock = DockStyle.Fill
        lblDenBene.Location = New Point(3, 0)
        lblDenBene.Name = "lblDenBene"
        lblDenBene.Size = New Size(94, 34)
        lblDenBene.TabIndex = 0
        lblDenBene.Text = "Beneficiar"
        lblDenBene.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtDenBene
        '
        txtDenBene.Dock = DockStyle.Fill
        txtDenBene.Location = New Point(103, 3)
        txtDenBene.Name = "txtDenBene"
        txtDenBene.Size = New Size(228, 28)
        txtDenBene.TabIndex = 1
        '
        ' lblCodPartener
        '
        lblCodPartener.AutoSize = True
        lblCodPartener.Dock = DockStyle.Fill
        lblCodPartener.Location = New Point(337, 0)
        lblCodPartener.Name = "lblCodPartener"
        lblCodPartener.Size = New Size(74, 34)
        lblCodPartener.TabIndex = 2
        lblCodPartener.Text = "Partener"
        lblCodPartener.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboCodPartener
        '
        cboCodPartener.Dock = DockStyle.Fill
        cboCodPartener.DropDownStyle = ComboBoxStyle.DropDownList
        cboCodPartener.Location = New Point(417, 3)
        cboCodPartener.Name = "cboCodPartener"
        cboCodPartener.Size = New Size(288, 28)
        cboCodPartener.TabIndex = 3
        '
        ' lblCodFiscal
        '
        lblCodFiscal.AutoSize = True
        lblCodFiscal.Dock = DockStyle.Fill
        lblCodFiscal.Location = New Point(3, 34)
        lblCodFiscal.Name = "lblCodFiscal"
        lblCodFiscal.Size = New Size(94, 34)
        lblCodFiscal.TabIndex = 4
        lblCodFiscal.Text = "Cod fiscal"
        lblCodFiscal.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtCodFiscal
        '
        txtCodFiscal.Dock = DockStyle.Fill
        txtCodFiscal.Location = New Point(103, 37)
        txtCodFiscal.Name = "txtCodFiscal"
        txtCodFiscal.Size = New Size(228, 28)
        txtCodFiscal.TabIndex = 5
        '
        ' lblContIban
        '
        lblContIban.AutoSize = True
        lblContIban.Dock = DockStyle.Fill
        lblContIban.Location = New Point(337, 34)
        lblContIban.Name = "lblContIban"
        lblContIban.Size = New Size(74, 34)
        lblContIban.TabIndex = 6
        lblContIban.Text = "Cont IBAN"
        lblContIban.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtContIban
        '
        txtContIban.Dock = DockStyle.Fill
        txtContIban.Location = New Point(417, 37)
        txtContIban.Name = "txtContIban"
        txtContIban.Size = New Size(288, 28)
        txtContIban.TabIndex = 7
        '
        ' lblBanca
        '
        lblBanca.AutoSize = True
        lblBanca.Dock = DockStyle.Fill
        lblBanca.Location = New Point(3, 68)
        lblBanca.Name = "lblBanca"
        lblBanca.Size = New Size(94, 34)
        lblBanca.TabIndex = 8
        lblBanca.Text = "Banca"
        lblBanca.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtBanca
        '
        tlyAntetBene.SetColumnSpan(txtBanca, 3)
        txtBanca.Dock = DockStyle.Fill
        txtBanca.Location = New Point(103, 71)
        txtBanca.Name = "txtBanca"
        txtBanca.Size = New Size(602, 28)
        txtBanca.TabIndex = 9
        '
        ' grdLinii
        '
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn2.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.HeaderText = "Clasificație"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "clsf"
        KBotDataColumn2.MinWidth = 60
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Width = 110
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn3.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.HeaderText = "Cod SSI"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "ssi_iban"
        KBotDataColumn3.MinWidth = 80
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn3.Width = 170
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn4.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn4.HeaderText = "Recepții"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "total_receptii"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 90
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn5.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn5.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn5.HeaderText = "Plăți ant."
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "plati_ant"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 90
        KBotDataColumn6.Aggregate = KBotAggregate.Sum
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn6.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn6.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn6.HeaderText = "Valoare"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "valoare"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 90
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn7.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn7.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn7.HeaderText = "Rămas"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "ramas"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn7.Width = 90
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn8.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn8.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn8.HeaderText = "Explicație"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "explicatie"
        KBotDataColumn8.MinWidth = 100
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn8.Width = 220
        grdLinii.Columns.Add(KBotDataColumn2)
        grdLinii.Columns.Add(KBotDataColumn3)
        grdLinii.Columns.Add(KBotDataColumn8)
        grdLinii.Columns.Add(KBotDataColumn4)
        grdLinii.Columns.Add(KBotDataColumn5)
        grdLinii.Columns.Add(KBotDataColumn6)
        grdLinii.Columns.Add(KBotDataColumn7)
        grdLinii.ColumnFillMode = KBotFillMode.SpecificColumn
        grdLinii.Dock = DockStyle.Fill
        grdLinii.FillColumnKey = "explicatie"
        grdLinii.FooterCaption = "TOTAL"
        grdLinii.FooterHeight = 28
        grdLinii.FooterVisible = True
        grdLinii.FrozenColumnCount = 1
        grdLinii.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdLinii.Location = New Point(3, 119)
        grdLinii.Name = "grdLinii"
        grdLinii.ReadOnlyGrid = False
        grdLinii.RowHeight = 22
        grdLinii.Size = New Size(708, 398)
        grdLinii.TabIndex = 1
        '
        ' OrdBeneficiariPage
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Name = "OrdBeneficiariPage"
        Size = New Size(980, 520)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        tlyStanga.ResumeLayout(False)
        tlyStanga.PerformLayout()
        CType(grdStanga, ComponentModel.ISupportInitialize).EndInit()
        tlyDreapta.ResumeLayout(False)
        tlyAntetBene.ResumeLayout(False)
        tlyAntetBene.PerformLayout()
        CType(grdLinii, ComponentModel.ISupportInitialize).EndInit()
        tips.SetToolTipHeader(chkClsf, "Grupează pe clasificații")
        tips.SetToolTipText(chkClsf, "Bifat: lista din stânga arată clasificațiile, iar coloana a doua a grilei arată contul IBAN." & vbLf & "Nebifat: lista arată beneficiarii, iar coloana arată codul SSI.")
        tips.SetToolTipHeader(cboCodPartener, "Partener")
        tips.SetToolTipText(cboCodPartener, "Partenerul din nomenclator care corespunde beneficiarului." & vbLf & "Completează codul fiscal și contul, dacă sunt goale.")
        tips.SetToolTipHeader(grdLinii, "Rândurile de plată")
        tips.SetToolTipText(grdLinii, "Se editează «Valoare» și «Explicație»." & vbLf & "«Rămas» se recalculează singur: recepții − plăți anterioare − valoare.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents split As SplitContainer
    Friend WithEvents tlyStanga As TableLayoutPanel
    Friend WithEvents chkClsf As CheckBox
    Friend WithEvents grdStanga As KBot.Controls.KBotDataView
    Friend WithEvents tlyDreapta As TableLayoutPanel
    Friend WithEvents tlyAntetBene As TableLayoutPanel
    Friend WithEvents lblDenBene As Label
    Friend WithEvents txtDenBene As KBot.Controls.KBotTextField
    Friend WithEvents lblCodPartener As Label
    Friend WithEvents cboCodPartener As KBot.Controls.KBotComboBox
    Friend WithEvents lblCodFiscal As Label
    Friend WithEvents txtCodFiscal As KBot.Controls.KBotTextField
    Friend WithEvents lblContIban As Label
    Friend WithEvents txtContIban As KBot.Controls.KBotTextField
    Friend WithEvents lblBanca As Label
    Friend WithEvents txtBanca As KBot.Controls.KBotTextField
    Friend WithEvents grdLinii As KBot.Controls.KBotDataView
End Class
