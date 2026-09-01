Imports KBot.Controls

' Pagina «Documente justificative» a editorului de ordonantare (felia 0049) — portul lui
' `frmFX_ORD_DOC` plus cele trei subformulare ale lui:
'   stanga      = `frmFX_ORD_DOC_BENE`, selectorul de beneficiar, cu randul sintetic
'                 «< TOTI BENEFICIARII >» (care in Access inseamna randurile fara legatura de
'                 beneficiar — documente ale INTREGII ordonantari, nu ale unuia);
'   mijloc      = `frmFX_ORD_DOC_TXT`, randurile TEXT (`DocJust`);
'   dreapta     = `frmFX_ORD_DOC_ATT`, randurile cu FISIER (`NumeDoc` + `TipDoc`).
'
' `btnSav` de pe popup-ul Access DISPARE (D2): aici exista o singura salvare, a formularului,
' pentru tot graful.
'
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md).
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OrdDocumentePage
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
        tips = New KBotToolTip(components)
        dlgFisiere = New OpenFileDialog()
        split = New SplitContainer()
        grdBene = New KBotDataView()
        splitDreapta = New SplitContainer()
        tlyText = New TableLayoutPanel()
        lblText = New Label()
        grdText = New KBotDataView()
        tlyButoaneText = New FlowLayoutPanel()
        btnAdaugaText = New Button()
        btnStergeText = New Button()
        tlyFisiere = New TableLayoutPanel()
        lblFisiere = New Label()
        grdFisiere = New KBotDataView()
        tlyButoaneFisiere = New FlowLayoutPanel()
        btnAdaugaFisier = New Button()
        btnStergeFisier = New Button()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        CType(grdBene, ComponentModel.ISupportInitialize).BeginInit()
        CType(splitDreapta, ComponentModel.ISupportInitialize).BeginInit()
        splitDreapta.Panel1.SuspendLayout()
        splitDreapta.Panel2.SuspendLayout()
        splitDreapta.SuspendLayout()
        tlyText.SuspendLayout()
        CType(grdText, ComponentModel.ISupportInitialize).BeginInit()
        tlyButoaneText.SuspendLayout()
        tlyFisiere.SuspendLayout()
        CType(grdFisiere, ComponentModel.ISupportInitialize).BeginInit()
        tlyButoaneFisiere.SuspendLayout()
        SuspendLayout()
        '
        ' dlgFisiere
        '
        dlgFisiere.Filter = "Toate fișierele|*.*"
        dlgFisiere.Multiselect = True
        dlgFisiere.Title = "Selectează fișiere"
        '
        ' split
        '
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Name = "split"
        '
        ' split.Panel1
        '
        split.Panel1.Controls.Add(grdBene)
        split.Panel1MinSize = 180
        '
        ' split.Panel2
        '
        split.Panel2.Controls.Add(splitDreapta)
        split.Panel2MinSize = 380
        split.Size = New Size(980, 520)
        split.SplitterDistance = 260
        split.SplitterWidth = 6
        split.TabIndex = 0
        '
        ' grdBene
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
        grdBene.Columns.Add(KBotDataColumn1)
        grdBene.ColumnFillMode = KBotFillMode.SpecificColumn
        grdBene.Dock = DockStyle.Fill
        grdBene.FillColumnKey = "eticheta"
        grdBene.FooterVisible = False
        grdBene.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdBene.Location = New Point(0, 0)
        grdBene.Margin = New Padding(0)
        grdBene.Name = "grdBene"
        grdBene.ReadOnlyGrid = True
        grdBene.RowHeight = 22
        grdBene.Size = New Size(260, 520)
        grdBene.TabIndex = 0
        '
        ' splitDreapta
        '
        splitDreapta.Dock = DockStyle.Fill
        splitDreapta.Location = New Point(0, 0)
        splitDreapta.Name = "splitDreapta"
        '
        ' splitDreapta.Panel1
        '
        splitDreapta.Panel1.Controls.Add(tlyText)
        splitDreapta.Panel1MinSize = 200
        '
        ' splitDreapta.Panel2
        '
        splitDreapta.Panel2.Controls.Add(tlyFisiere)
        splitDreapta.Panel2MinSize = 180
        splitDreapta.Size = New Size(714, 520)
        splitDreapta.SplitterDistance = 420
        splitDreapta.SplitterWidth = 6
        splitDreapta.TabIndex = 0
        '
        ' tlyText
        '
        tlyText.ColumnCount = 1
        tlyText.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyText.Controls.Add(grdText, 0, 1)
        tlyText.Controls.Add(lblText, 0, 0)
        tlyText.Controls.Add(tlyButoaneText, 0, 2)
        tlyText.Dock = DockStyle.Fill
        tlyText.Location = New Point(0, 0)
        tlyText.Margin = New Padding(0)
        tlyText.Name = "tlyText"
        tlyText.RowCount = 3
        tlyText.RowStyles.Add(New RowStyle(SizeType.Absolute, 26F))
        tlyText.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyText.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyText.Size = New Size(420, 520)
        tlyText.TabIndex = 0
        '
        ' lblText
        '
        lblText.AutoSize = True
        lblText.Dock = DockStyle.Fill
        lblText.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblText.Location = New Point(3, 0)
        lblText.Name = "lblText"
        lblText.Size = New Size(414, 26)
        lblText.TabIndex = 0
        lblText.Text = "Documente justificative (text)"
        lblText.TextAlign = ContentAlignment.MiddleLeft
        '
        ' grdText
        '
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn2.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.HeaderText = "Document justificativ"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Key = "doc_just"
        KBotDataColumn2.MinWidth = 120
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Width = 300
        grdText.Columns.Add(KBotDataColumn2)
        grdText.ColumnFillMode = KBotFillMode.SpecificColumn
        grdText.Dock = DockStyle.Fill
        grdText.FillColumnKey = "doc_just"
        grdText.FooterVisible = False
        grdText.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdText.Location = New Point(0, 26)
        grdText.Margin = New Padding(0)
        grdText.Name = "grdText"
        grdText.ReadOnlyGrid = False
        grdText.RowHeight = 22
        grdText.Size = New Size(420, 454)
        grdText.TabIndex = 1
        '
        ' tlyButoaneText
        '
        tlyButoaneText.Controls.Add(btnAdaugaText)
        tlyButoaneText.Controls.Add(btnStergeText)
        tlyButoaneText.Dock = DockStyle.Fill
        tlyButoaneText.Location = New Point(0, 480)
        tlyButoaneText.Margin = New Padding(0)
        tlyButoaneText.Name = "tlyButoaneText"
        tlyButoaneText.Padding = New Padding(0, 5, 0, 5)
        tlyButoaneText.Size = New Size(420, 40)
        tlyButoaneText.TabIndex = 2
        '
        ' btnAdaugaText
        '
        btnAdaugaText.AutoSize = True
        btnAdaugaText.Location = New Point(3, 8)
        btnAdaugaText.Name = "btnAdaugaText"
        btnAdaugaText.Padding = New Padding(8, 2, 8, 2)
        btnAdaugaText.Size = New Size(120, 28)
        btnAdaugaText.TabIndex = 0
        btnAdaugaText.Text = "Adaugă rând"
        btnAdaugaText.UseVisualStyleBackColor = True
        '
        ' btnStergeText
        '
        btnStergeText.AutoSize = True
        btnStergeText.Location = New Point(129, 8)
        btnStergeText.Name = "btnStergeText"
        btnStergeText.Padding = New Padding(8, 2, 8, 2)
        btnStergeText.Size = New Size(120, 28)
        btnStergeText.TabIndex = 1
        btnStergeText.Text = "Șterge rândul"
        btnStergeText.UseVisualStyleBackColor = True
        '
        ' tlyFisiere
        '
        tlyFisiere.ColumnCount = 1
        tlyFisiere.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFisiere.Controls.Add(grdFisiere, 0, 1)
        tlyFisiere.Controls.Add(lblFisiere, 0, 0)
        tlyFisiere.Controls.Add(tlyButoaneFisiere, 0, 2)
        tlyFisiere.Dock = DockStyle.Fill
        tlyFisiere.Location = New Point(0, 0)
        tlyFisiere.Margin = New Padding(0)
        tlyFisiere.Name = "tlyFisiere"
        tlyFisiere.RowCount = 3
        tlyFisiere.RowStyles.Add(New RowStyle(SizeType.Absolute, 26F))
        tlyFisiere.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFisiere.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyFisiere.Size = New Size(288, 520)
        tlyFisiere.TabIndex = 0
        '
        ' lblFisiere
        '
        lblFisiere.AutoSize = True
        lblFisiere.Dock = DockStyle.Fill
        lblFisiere.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblFisiere.Location = New Point(3, 0)
        lblFisiere.Name = "lblFisiere"
        lblFisiere.Size = New Size(282, 26)
        lblFisiere.TabIndex = 0
        lblFisiere.Text = "Fișiere anexate"
        lblFisiere.TextAlign = ContentAlignment.MiddleLeft
        '
        ' grdFisiere
        '
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn3.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.HeaderText = "Nume fișier"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn3.Key = "nume_doc"
        KBotDataColumn3.MinWidth = 100
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn3.Width = 200
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn4.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn4.HeaderText = "Tip"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "tip_doc"
        KBotDataColumn4.MinWidth = 40
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 60
        grdFisiere.Columns.Add(KBotDataColumn3)
        grdFisiere.Columns.Add(KBotDataColumn4)
        grdFisiere.ColumnFillMode = KBotFillMode.SpecificColumn
        grdFisiere.Dock = DockStyle.Fill
        grdFisiere.FillColumnKey = "nume_doc"
        grdFisiere.FooterVisible = False
        grdFisiere.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdFisiere.Location = New Point(0, 26)
        grdFisiere.Margin = New Padding(0)
        grdFisiere.Name = "grdFisiere"
        grdFisiere.ReadOnlyGrid = True
        grdFisiere.RowHeight = 22
        grdFisiere.Size = New Size(288, 454)
        grdFisiere.TabIndex = 1
        '
        ' tlyButoaneFisiere
        '
        tlyButoaneFisiere.Controls.Add(btnAdaugaFisier)
        tlyButoaneFisiere.Controls.Add(btnStergeFisier)
        tlyButoaneFisiere.Dock = DockStyle.Fill
        tlyButoaneFisiere.Location = New Point(0, 480)
        tlyButoaneFisiere.Margin = New Padding(0)
        tlyButoaneFisiere.Name = "tlyButoaneFisiere"
        tlyButoaneFisiere.Padding = New Padding(0, 5, 0, 5)
        tlyButoaneFisiere.Size = New Size(288, 40)
        tlyButoaneFisiere.TabIndex = 2
        '
        ' btnAdaugaFisier
        '
        btnAdaugaFisier.AutoSize = True
        btnAdaugaFisier.Location = New Point(3, 8)
        btnAdaugaFisier.Name = "btnAdaugaFisier"
        btnAdaugaFisier.Padding = New Padding(8, 2, 8, 2)
        btnAdaugaFisier.Size = New Size(120, 28)
        btnAdaugaFisier.TabIndex = 0
        btnAdaugaFisier.Text = "Adaugă fișier"
        btnAdaugaFisier.UseVisualStyleBackColor = True
        '
        ' btnStergeFisier
        '
        btnStergeFisier.AutoSize = True
        btnStergeFisier.Location = New Point(129, 8)
        btnStergeFisier.Name = "btnStergeFisier"
        btnStergeFisier.Padding = New Padding(8, 2, 8, 2)
        btnStergeFisier.Size = New Size(120, 28)
        btnStergeFisier.TabIndex = 1
        btnStergeFisier.Text = "Șterge fișierul"
        btnStergeFisier.UseVisualStyleBackColor = True
        '
        ' OrdDocumentePage
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Name = "OrdDocumentePage"
        Size = New Size(980, 520)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        CType(grdBene, ComponentModel.ISupportInitialize).EndInit()
        splitDreapta.Panel1.ResumeLayout(False)
        splitDreapta.Panel2.ResumeLayout(False)
        CType(splitDreapta, ComponentModel.ISupportInitialize).EndInit()
        splitDreapta.ResumeLayout(False)
        tlyText.ResumeLayout(False)
        tlyText.PerformLayout()
        CType(grdText, ComponentModel.ISupportInitialize).EndInit()
        tlyButoaneText.ResumeLayout(False)
        tlyButoaneText.PerformLayout()
        tlyFisiere.ResumeLayout(False)
        tlyFisiere.PerformLayout()
        CType(grdFisiere, ComponentModel.ISupportInitialize).EndInit()
        tlyButoaneFisiere.ResumeLayout(False)
        tlyButoaneFisiere.PerformLayout()
        tips.SetToolTipHeader(grdBene, "Beneficiarul")
        tips.SetToolTipText(grdBene, "Primul rând, «< TOȚI BENEFICIARII >», ține documentele întregii ordonanțări." & vbLf & "Un beneficiar anume arată și documentele lui, și pe cele comune.")
        tips.SetToolTipHeader(btnAdaugaText, "Adaugă rând")
        tips.SetToolTipText(btnAdaugaText, "Un rând text nou, pe beneficiarul selectat." & vbLf & "Cel puțin un rând text trebuie să existe ca ordonanțarea să se poată salva.")
        tips.SetToolTipHeader(btnAdaugaFisier, "Adaugă fișier")
        tips.SetToolTipText(btnAdaugaFisier, "Anexează unul sau mai multe fișiere la beneficiarul selectat." & vbLf & "Se păstrează în document, codificate, ca în vechea machetă.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents dlgFisiere As OpenFileDialog
    Friend WithEvents split As SplitContainer
    Friend WithEvents grdBene As KBot.Controls.KBotDataView
    Friend WithEvents splitDreapta As SplitContainer
    Friend WithEvents tlyText As TableLayoutPanel
    Friend WithEvents lblText As Label
    Friend WithEvents grdText As KBot.Controls.KBotDataView
    Friend WithEvents tlyButoaneText As FlowLayoutPanel
    Friend WithEvents btnAdaugaText As Button
    Friend WithEvents btnStergeText As Button
    Friend WithEvents tlyFisiere As TableLayoutPanel
    Friend WithEvents lblFisiere As Label
    Friend WithEvents grdFisiere As KBot.Controls.KBotDataView
    Friend WithEvents tlyButoaneFisiere As FlowLayoutPanel
    Friend WithEvents btnAdaugaFisier As Button
    Friend WithEvents btnStergeFisier As Button
End Class
