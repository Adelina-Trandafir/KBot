Imports KBot.Controls

' Pagina «Atasamente» a editorului de ordonantare (felia 0049) — portul lui
' `frmFX_ORD_PRTSCR` + `_BENE` + `_S`:
'   stanga  = selectorul de beneficiar, cu acelasi rand sintetic «< TOTI BENEFICIARII >»;
'   mijloc  = randurile de atasament, plus alegerea fisierului / lipirea din clipboard /
'             stergerea;
'   dreapta = previzualizarea imaginii.
'
' CE NU S-A PORTAT, si de ce: campurile `hwndAccess` / `hwndForm` si controlul `WebBrowser0`
' din `frmFX_ORD_PRTSCR` sunt instalatie de gazduire a ferestrelor Access — Access reparenta
' un WebBrowser prin `SetParent` ca sa poata afisa o imagine base64 cu zoom si panoramare.
' In WinForms, previzualizarea e un `PictureBox` cu `SizeMode = Zoom`; nu exista nimic de
' reparentat, deci cele trei nu au succesor. (Consemnat si in worklog, ca sa nu se intrebe
' cineva unde s-au dus.)
'
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md).
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OrdAtasamentePage
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
        tips = New KBotToolTip(components)
        dlgImagine = New OpenFileDialog()
        split = New SplitContainer()
        grdBene = New KBotDataView()
        splitDreapta = New SplitContainer()
        tlyLista = New TableLayoutPanel()
        lblLista = New Label()
        grdAtasamente = New KBotDataView()
        tlyButoane = New FlowLayoutPanel()
        btnAdauga = New Button()
        btnLipeste = New Button()
        btnSterge = New Button()
        pnlPreview = New Panel()
        picPreview = New PictureBox()
        lblPreviewGol = New Label()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        CType(grdBene, ComponentModel.ISupportInitialize).BeginInit()
        CType(splitDreapta, ComponentModel.ISupportInitialize).BeginInit()
        splitDreapta.Panel1.SuspendLayout()
        splitDreapta.Panel2.SuspendLayout()
        splitDreapta.SuspendLayout()
        tlyLista.SuspendLayout()
        CType(grdAtasamente, ComponentModel.ISupportInitialize).BeginInit()
        tlyButoane.SuspendLayout()
        pnlPreview.SuspendLayout()
        CType(picPreview, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' dlgImagine
        '
        ' Aceleasi filtre ca `SelectFile` din frmFX_ORD_PRTSCR_S.
        dlgImagine.Filter = "Imagini|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff|Toate fișierele|*.*"
        dlgImagine.Multiselect = True
        dlgImagine.Title = "Selectează imagine"
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
        splitDreapta.Panel1.Controls.Add(tlyLista)
        splitDreapta.Panel1MinSize = 180
        '
        ' splitDreapta.Panel2
        '
        splitDreapta.Panel2.Controls.Add(pnlPreview)
        splitDreapta.Panel2MinSize = 200
        splitDreapta.Size = New Size(714, 520)
        splitDreapta.SplitterDistance = 300
        splitDreapta.SplitterWidth = 6
        splitDreapta.TabIndex = 0
        '
        ' tlyLista
        '
        tlyLista.ColumnCount = 1
        tlyLista.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyLista.Controls.Add(grdAtasamente, 0, 1)
        tlyLista.Controls.Add(lblLista, 0, 0)
        tlyLista.Controls.Add(tlyButoane, 0, 2)
        tlyLista.Dock = DockStyle.Fill
        tlyLista.Location = New Point(0, 0)
        tlyLista.Margin = New Padding(0)
        tlyLista.Name = "tlyLista"
        tlyLista.RowCount = 3
        tlyLista.RowStyles.Add(New RowStyle(SizeType.Absolute, 26F))
        tlyLista.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyLista.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyLista.Size = New Size(300, 520)
        tlyLista.TabIndex = 0
        '
        ' lblLista
        '
        lblLista.AutoSize = True
        lblLista.Dock = DockStyle.Fill
        lblLista.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblLista.Location = New Point(3, 0)
        lblLista.Name = "lblLista"
        lblLista.Size = New Size(294, 26)
        lblLista.TabIndex = 0
        lblLista.Text = "Imagini atașate"
        lblLista.TextAlign = ContentAlignment.MiddleLeft
        '
        ' grdAtasamente
        '
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn2.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.HeaderText = "Nume fișier"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Key = "nume_fisier"
        KBotDataColumn2.MinWidth = 100
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Width = 200
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn3.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.HeaderText = "Stare"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "stare"
        KBotDataColumn3.MinWidth = 60
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 90
        grdAtasamente.Columns.Add(KBotDataColumn2)
        grdAtasamente.Columns.Add(KBotDataColumn3)
        grdAtasamente.ColumnFillMode = KBotFillMode.SpecificColumn
        grdAtasamente.Dock = DockStyle.Fill
        grdAtasamente.FillColumnKey = "nume_fisier"
        grdAtasamente.FooterVisible = False
        grdAtasamente.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdAtasamente.Location = New Point(0, 26)
        grdAtasamente.Margin = New Padding(0)
        grdAtasamente.Name = "grdAtasamente"
        grdAtasamente.ReadOnlyGrid = False
        grdAtasamente.RowHeight = 22
        grdAtasamente.Size = New Size(300, 454)
        grdAtasamente.TabIndex = 1
        '
        ' tlyButoane
        '
        tlyButoane.Controls.Add(btnAdauga)
        tlyButoane.Controls.Add(btnLipeste)
        tlyButoane.Controls.Add(btnSterge)
        tlyButoane.Dock = DockStyle.Fill
        tlyButoane.Location = New Point(0, 480)
        tlyButoane.Margin = New Padding(0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.Padding = New Padding(0, 5, 0, 5)
        tlyButoane.Size = New Size(300, 40)
        tlyButoane.TabIndex = 2
        '
        ' btnAdauga
        '
        btnAdauga.AutoSize = True
        btnAdauga.Location = New Point(3, 8)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Padding = New Padding(8, 2, 8, 2)
        btnAdauga.Size = New Size(100, 28)
        btnAdauga.TabIndex = 0
        btnAdauga.Text = "Adaugă"
        btnAdauga.UseVisualStyleBackColor = True
        '
        ' btnLipeste
        '
        btnLipeste.AutoSize = True
        btnLipeste.Location = New Point(109, 8)
        btnLipeste.Name = "btnLipeste"
        btnLipeste.Padding = New Padding(8, 2, 8, 2)
        btnLipeste.Size = New Size(100, 28)
        btnLipeste.TabIndex = 1
        btnLipeste.Text = "Lipește"
        btnLipeste.UseVisualStyleBackColor = True
        '
        ' btnSterge
        '
        btnSterge.AutoSize = True
        btnSterge.Location = New Point(215, 8)
        btnSterge.Name = "btnSterge"
        btnSterge.Padding = New Padding(8, 2, 8, 2)
        btnSterge.Size = New Size(100, 28)
        btnSterge.TabIndex = 2
        btnSterge.Text = "Șterge"
        btnSterge.UseVisualStyleBackColor = True
        '
        ' pnlPreview
        '
        pnlPreview.Controls.Add(picPreview)
        pnlPreview.Controls.Add(lblPreviewGol)
        pnlPreview.Dock = DockStyle.Fill
        pnlPreview.Location = New Point(0, 0)
        pnlPreview.Margin = New Padding(0)
        pnlPreview.Name = "pnlPreview"
        pnlPreview.Size = New Size(408, 520)
        pnlPreview.TabIndex = 0
        '
        ' picPreview
        '
        picPreview.Dock = DockStyle.Fill
        picPreview.Location = New Point(0, 0)
        picPreview.Name = "picPreview"
        picPreview.Size = New Size(408, 520)
        picPreview.SizeMode = PictureBoxSizeMode.Zoom
        picPreview.TabIndex = 0
        picPreview.TabStop = False
        '
        ' lblPreviewGol
        '
        lblPreviewGol.Dock = DockStyle.Fill
        lblPreviewGol.Font = New Font("Segoe UI", 10F)
        lblPreviewGol.Location = New Point(0, 0)
        lblPreviewGol.Name = "lblPreviewGol"
        lblPreviewGol.Size = New Size(408, 520)
        lblPreviewGol.TabIndex = 1
        lblPreviewGol.Text = "Selectați o imagine din listă."
        lblPreviewGol.TextAlign = ContentAlignment.MiddleCenter
        '
        ' OrdAtasamentePage
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Name = "OrdAtasamentePage"
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
        tlyLista.ResumeLayout(False)
        tlyLista.PerformLayout()
        CType(grdAtasamente, ComponentModel.ISupportInitialize).EndInit()
        tlyButoane.ResumeLayout(False)
        tlyButoane.PerformLayout()
        pnlPreview.ResumeLayout(False)
        CType(picPreview, ComponentModel.ISupportInitialize).EndInit()
        tips.SetToolTipHeader(btnAdauga, "Adaugă imagine")
        tips.SetToolTipText(btnAdauga, "Alege una sau mai multe imagini de pe disc." & vbLf & "Se încarcă pe server după salvarea ordonanțării.")
        tips.SetToolTipHeader(btnLipeste, "Lipește din clipboard")
        tips.SetToolTipText(btnLipeste, "Ia captura de ecran din memoria temporară a Windows-ului." & vbLf & "Se salvează ca PNG.")
        tips.SetToolTipHeader(btnSterge, "Șterge imaginea")
        tips.SetToolTipText(btnSterge, "Scoate imaginea din ordonanțare." & vbLf & "Dispare de pe server la următoarea salvare.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents dlgImagine As OpenFileDialog
    Friend WithEvents split As SplitContainer
    Friend WithEvents grdBene As KBot.Controls.KBotDataView
    Friend WithEvents splitDreapta As SplitContainer
    Friend WithEvents tlyLista As TableLayoutPanel
    Friend WithEvents lblLista As Label
    Friend WithEvents grdAtasamente As KBot.Controls.KBotDataView
    Friend WithEvents tlyButoane As FlowLayoutPanel
    Friend WithEvents btnAdauga As Button
    Friend WithEvents btnLipeste As Button
    Friend WithEvents btnSterge As Button
    Friend WithEvents pnlPreview As Panel
    Friend WithEvents picPreview As PictureBox
    Friend WithEvents lblPreviewGol As Label
End Class
