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
' Coordonatele sunt scrise la 144 dpi -- fisierul a fost salvat din designer pe un ecran la
' 150%, iar Visual Studio rescrie ATUNCI si coordonatele, si perechea. AutoScaleDimensions
' le insoteste: Calibri 9 se masoara (9, 22) acolo (felia 0052). Cele doua se schimba
' INTOTDEAUNA impreuna; o pereche luata de la alt font sau de la alt dpi turteste fereastra
' la deschidere, fara ca nimic din designer s-o arate.
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
        grdBene = New KBotDataView()
        btnAdaugaText = New Button()
        btnAdaugaFisier = New Button()
        dlgFisiere = New OpenFileDialog()
        split = New SplitContainer()
        splitDreapta = New SplitContainer()
        tlyText = New TableLayoutPanel()
        btnStergeText = New Button()
        grdText = New KBotDataView()
        tlyFisiere = New TableLayoutPanel()
        btnStergeFisier = New Button()
        grdFisiere = New KBotDataView()
        CType(grdBene, ComponentModel.ISupportInitialize).BeginInit()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        CType(splitDreapta, ComponentModel.ISupportInitialize).BeginInit()
        splitDreapta.Panel1.SuspendLayout()
        splitDreapta.Panel2.SuspendLayout()
        splitDreapta.SuspendLayout()
        tlyText.SuspendLayout()
        CType(grdText, ComponentModel.ISupportInitialize).BeginInit()
        tlyFisiere.SuspendLayout()
        CType(grdFisiere, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' grdBene
        ' 
        grdBene.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grdBene.BackColor = SystemColors.Window
        grdBene.ColumnFillMode = KBotFillMode.FirstColumn
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
        KBotDataColumn1.Width = 240
        grdBene.Columns.Add(KBotDataColumn1)
        grdBene.Dock = DockStyle.Fill
        grdBene.FillColumnKey = "eticheta"
        grdBene.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdBene.HeaderSeparatorColor = SystemColors.ActiveBorder
        grdBene.Location = New Point(10, 0)
        grdBene.Margin = New Padding(0)
        grdBene.Name = "grdBene"
        grdBene.ReadOnlyGrid = True
        grdBene.RowHeight = 22
        grdBene.Size = New Size(294, 566)
        grdBene.TabIndex = 0
        tips.SetToolTipHeader(grdBene, "Beneficiarul")
        tips.SetToolTipText(grdBene, "Primul rând, «< TOȚI BENEFICIARII >», ține documentele întregii ordonanțări." & vbLf & "Un beneficiar anume arată și documentele lui, și pe cele comune." & vbLf & "Când ordonanțarea are un singur beneficiar, rândul acela lipsește: n-ar spune nimic în plus.")
        ' 
        ' btnAdaugaText
        ' 
        btnAdaugaText.AutoSize = True
        btnAdaugaText.Dock = DockStyle.Left
        btnAdaugaText.Location = New Point(0, 516)
        btnAdaugaText.Margin = New Padding(0)
        btnAdaugaText.Name = "btnAdaugaText"
        btnAdaugaText.Size = New Size(153, 50)
        btnAdaugaText.TabIndex = 0
        btnAdaugaText.Text = "Adaugă rând"
        tips.SetToolTipHeader(btnAdaugaText, "Adaugă rând")
        tips.SetToolTipText(btnAdaugaText, "Un rând text nou, pe beneficiarul selectat." & vbLf & "Cel puțin un rând text trebuie să existe ca ordonanțarea să se poată salva.")
        btnAdaugaText.UseVisualStyleBackColor = True
        ' 
        ' btnAdaugaFisier
        ' 
        btnAdaugaFisier.AutoSize = True
        btnAdaugaFisier.Dock = DockStyle.Left
        btnAdaugaFisier.Location = New Point(0, 516)
        btnAdaugaFisier.Margin = New Padding(0)
        btnAdaugaFisier.Name = "btnAdaugaFisier"
        btnAdaugaFisier.Size = New Size(153, 50)
        btnAdaugaFisier.TabIndex = 0
        btnAdaugaFisier.Text = "Adaugă fișier"
        tips.SetToolTipHeader(btnAdaugaFisier, "Adaugă fișier")
        tips.SetToolTipText(btnAdaugaFisier, "Anexează unul sau mai multe fișiere la beneficiarul selectat." & vbLf & "Se păstrează în document, codificate, ca în vechea machetă.")
        btnAdaugaFisier.UseVisualStyleBackColor = True
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
        split.Margin = New Padding(4, 5, 4, 5)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(grdBene)
        split.Panel1.Padding = New Padding(10, 0, 0, 10)
        split.Panel1MinSize = 180
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(splitDreapta)
        split.Panel2.Padding = New Padding(0, 0, 10, 0)
        split.Panel2MinSize = 380
        split.Size = New Size(1147, 576)
        split.SplitterDistance = 304
        split.SplitterWidth = 9
        split.TabIndex = 0
        ' 
        ' splitDreapta
        ' 
        splitDreapta.Dock = DockStyle.Fill
        splitDreapta.Location = New Point(0, 0)
        splitDreapta.Margin = New Padding(4, 5, 4, 5)
        splitDreapta.Name = "splitDreapta"
        ' 
        ' splitDreapta.Panel1
        ' 
        splitDreapta.Panel1.Controls.Add(tlyText)
        splitDreapta.Panel1.Padding = New Padding(0, 0, 0, 10)
        splitDreapta.Panel1MinSize = 200
        ' 
        ' splitDreapta.Panel2
        ' 
        splitDreapta.Panel2.Controls.Add(tlyFisiere)
        splitDreapta.Panel2.Padding = New Padding(0, 0, 0, 10)
        splitDreapta.Panel2MinSize = 180
        splitDreapta.Size = New Size(824, 576)
        splitDreapta.SplitterDistance = 483
        splitDreapta.SplitterWidth = 9
        splitDreapta.TabIndex = 0
        ' 
        ' tlyText
        ' 
        tlyText.ColumnCount = 2
        tlyText.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyText.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyText.Controls.Add(btnStergeText, 1, 1)
        tlyText.Controls.Add(grdText, 0, 0)
        tlyText.Controls.Add(btnAdaugaText, 0, 1)
        tlyText.Dock = DockStyle.Fill
        tlyText.Location = New Point(0, 0)
        tlyText.Margin = New Padding(0)
        tlyText.Name = "tlyText"
        tlyText.RowCount = 2
        tlyText.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyText.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyText.Size = New Size(483, 566)
        tlyText.TabIndex = 0
        ' 
        ' btnStergeText
        ' 
        btnStergeText.AutoSize = True
        btnStergeText.Dock = DockStyle.Right
        btnStergeText.Location = New Point(330, 516)
        btnStergeText.Margin = New Padding(0)
        btnStergeText.Name = "btnStergeText"
        btnStergeText.Padding = New Padding(11, 3, 11, 3)
        btnStergeText.Size = New Size(153, 50)
        btnStergeText.TabIndex = 2
        btnStergeText.Text = "Șterge rândul"
        btnStergeText.UseVisualStyleBackColor = True
        ' 
        ' grdText
        ' 
        grdText.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grdText.BackColor = SystemColors.Window
        grdText.ColumnFillMode = KBotFillMode.FirstColumn
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
        KBotDataColumn2.Width = 300
        grdText.Columns.Add(KBotDataColumn2)
        tlyText.SetColumnSpan(grdText, 2)
        grdText.Dock = DockStyle.Fill
        grdText.FillColumnKey = "doc_just"
        grdText.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdText.HeaderSeparatorColor = SystemColors.ActiveBorder
        grdText.Location = New Point(0, 0)
        grdText.Margin = New Padding(0)
        grdText.Name = "grdText"
        grdText.RowHeight = 22
        grdText.Size = New Size(483, 516)
        grdText.TabIndex = 1
        ' 
        ' tlyFisiere
        ' 
        tlyFisiere.ColumnCount = 2
        tlyFisiere.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyFisiere.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyFisiere.Controls.Add(btnStergeFisier, 1, 1)
        tlyFisiere.Controls.Add(grdFisiere, 0, 0)
        tlyFisiere.Controls.Add(btnAdaugaFisier, 0, 1)
        tlyFisiere.Dock = DockStyle.Fill
        tlyFisiere.Location = New Point(0, 0)
        tlyFisiere.Margin = New Padding(0)
        tlyFisiere.Name = "tlyFisiere"
        tlyFisiere.RowCount = 2
        tlyFisiere.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFisiere.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyFisiere.Size = New Size(332, 566)
        tlyFisiere.TabIndex = 0
        ' 
        ' btnStergeFisier
        ' 
        btnStergeFisier.AutoSize = True
        btnStergeFisier.Dock = DockStyle.Right
        btnStergeFisier.Location = New Point(179, 516)
        btnStergeFisier.Margin = New Padding(0)
        btnStergeFisier.Name = "btnStergeFisier"
        btnStergeFisier.Padding = New Padding(11, 3, 11, 3)
        btnStergeFisier.Size = New Size(153, 50)
        btnStergeFisier.TabIndex = 2
        btnStergeFisier.Text = "Șterge fișierul"
        btnStergeFisier.UseVisualStyleBackColor = True
        ' 
        ' grdFisiere
        ' 
        grdFisiere.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grdFisiere.BackColor = SystemColors.Window
        grdFisiere.ColumnFillMode = KBotFillMode.FirstColumn
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
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 60
        grdFisiere.Columns.Add(KBotDataColumn3)
        grdFisiere.Columns.Add(KBotDataColumn4)
        tlyFisiere.SetColumnSpan(grdFisiere, 2)
        grdFisiere.Dock = DockStyle.Fill
        grdFisiere.FillColumnKey = "nume_doc"
        grdFisiere.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdFisiere.HeaderSeparatorColor = SystemColors.ActiveBorder
        grdFisiere.Location = New Point(0, 0)
        grdFisiere.Margin = New Padding(0)
        grdFisiere.Name = "grdFisiere"
        grdFisiere.ReadOnlyGrid = True
        grdFisiere.RowHeight = 22
        grdFisiere.Size = New Size(332, 516)
        grdFisiere.TabIndex = 1
        ' 
        ' OrdDocumentePage
        ' 
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Margin = New Padding(4, 5, 4, 5)
        Name = "OrdDocumentePage"
        Size = New Size(1147, 576)
        CType(grdBene, ComponentModel.ISupportInitialize).EndInit()
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        splitDreapta.Panel1.ResumeLayout(False)
        splitDreapta.Panel2.ResumeLayout(False)
        CType(splitDreapta, ComponentModel.ISupportInitialize).EndInit()
        splitDreapta.ResumeLayout(False)
        tlyText.ResumeLayout(False)
        tlyText.PerformLayout()
        CType(grdText, ComponentModel.ISupportInitialize).EndInit()
        tlyFisiere.ResumeLayout(False)
        tlyFisiere.PerformLayout()
        CType(grdFisiere, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents dlgFisiere As OpenFileDialog
    Friend WithEvents split As SplitContainer
    Friend WithEvents grdBene As Global.KBot.Controls.KBotDataView
    Friend WithEvents splitDreapta As SplitContainer
    Friend WithEvents tlyText As TableLayoutPanel
    Friend WithEvents grdText As Global.KBot.Controls.KBotDataView
    Friend WithEvents btnAdaugaText As Button
    Friend WithEvents tlyFisiere As TableLayoutPanel
    Friend WithEvents grdFisiere As Global.KBot.Controls.KBotDataView
    Friend WithEvents btnAdaugaFisier As Button
    Friend WithEvents btnStergeText As Button
    Friend WithEvents btnStergeFisier As Button
End Class
