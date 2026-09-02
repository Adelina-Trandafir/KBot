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
        btnAdauga = New Button()
        btnSterge = New Button()
        btnLipeste = New Button()
        dlgImagine = New OpenFileDialog()
        split = New SplitContainer()
        grdBene = New KBotDataView()
        splitDreapta = New SplitContainer()
        tlyLista = New TableLayoutPanel()
        grdAtasamente = New KBotDataView()
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
        pnlPreview.SuspendLayout()
        CType(picPreview, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' btnAdauga
        ' 
        btnAdauga.AutoSize = True
        btnAdauga.Dock = DockStyle.Left
        btnAdauga.Location = New Point(0, 500)
        btnAdauga.Margin = New Padding(0)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Padding = New Padding(11, 3, 11, 3)
        btnAdauga.Size = New Size(136, 50)
        btnAdauga.TabIndex = 0
        btnAdauga.Text = "Adaugă"
        tips.SetToolTipHeader(btnAdauga, "Adaugă imagine")
        tips.SetToolTipText(btnAdauga, "Alege una sau mai multe imagini de pe disc." & vbLf & "Se încarcă pe server după salvarea ordonanțării.")
        btnAdauga.UseVisualStyleBackColor = True
        ' 
        ' btnSterge
        ' 
        btnSterge.AutoSize = True
        btnSterge.Dock = DockStyle.Right
        btnSterge.Location = New Point(136, 500)
        btnSterge.Margin = New Padding(0)
        btnSterge.Name = "btnSterge"
        btnSterge.Padding = New Padding(11, 3, 11, 3)
        btnSterge.Size = New Size(136, 50)
        btnSterge.TabIndex = 3
        btnSterge.Text = "Șterge"
        tips.SetToolTipHeader(btnSterge, "Șterge imaginea")
        tips.SetToolTipText(btnSterge, "Scoate imaginea din ordonanțare." & vbLf & "Dispare de pe server la următoarea salvare.")
        btnSterge.UseVisualStyleBackColor = True
        ' 
        ' btnLipeste
        ' 
        btnLipeste.AutoSize = True
        btnLipeste.Dock = DockStyle.Right
        btnLipeste.Location = New Point(272, 500)
        btnLipeste.Margin = New Padding(0)
        btnLipeste.Name = "btnLipeste"
        btnLipeste.Padding = New Padding(11, 3, 11, 3)
        btnLipeste.Size = New Size(138, 50)
        btnLipeste.TabIndex = 4
        btnLipeste.Text = "Lipește"
        tips.SetToolTipHeader(btnLipeste, "Șterge imaginea")
        tips.SetToolTipText(btnLipeste, "Scoate imaginea din ordonanțare." & vbLf & "Dispare de pe server la următoarea salvare.")
        btnLipeste.UseVisualStyleBackColor = True
        ' 
        ' dlgImagine
        ' 
        dlgImagine.Filter = "Imagini|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff|Toate fișierele|*.*"
        dlgImagine.Multiselect = True
        dlgImagine.Title = "Selectează imagine"
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
        split.Panel2.Padding = New Padding(0, 0, 10, 10)
        split.Panel2MinSize = 380
        split.Size = New Size(1187, 560)
        split.SplitterDistance = 314
        split.SplitterWidth = 9
        split.TabIndex = 0
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
        grdBene.HeaderBackColor = SystemColors.Control
        grdBene.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdBene.HeaderSeparatorColor = SystemColors.ActiveBorder
        grdBene.Location = New Point(10, 0)
        grdBene.Margin = New Padding(0)
        grdBene.Name = "grdBene"
        grdBene.ReadOnlyGrid = True
        grdBene.RowHeight = 22
        grdBene.Size = New Size(304, 550)
        grdBene.TabIndex = 0
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
        splitDreapta.Panel1.Controls.Add(tlyLista)
        splitDreapta.Panel1MinSize = 180
        ' 
        ' splitDreapta.Panel2
        ' 
        splitDreapta.Panel2.Controls.Add(pnlPreview)
        splitDreapta.Panel2MinSize = 200
        splitDreapta.Size = New Size(854, 550)
        splitDreapta.SplitterDistance = 410
        splitDreapta.SplitterWidth = 9
        splitDreapta.TabIndex = 0
        ' 
        ' tlyLista
        ' 
        tlyLista.ColumnCount = 3
        tlyLista.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.3333321F))
        tlyLista.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.3333359F))
        tlyLista.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.3333359F))
        tlyLista.Controls.Add(btnLipeste, 2, 1)
        tlyLista.Controls.Add(btnSterge, 1, 1)
        tlyLista.Controls.Add(grdAtasamente, 0, 0)
        tlyLista.Controls.Add(btnAdauga, 0, 1)
        tlyLista.Dock = DockStyle.Fill
        tlyLista.Location = New Point(0, 0)
        tlyLista.Margin = New Padding(0)
        tlyLista.Name = "tlyLista"
        tlyLista.RowCount = 2
        tlyLista.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyLista.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyLista.Size = New Size(410, 550)
        tlyLista.TabIndex = 0
        ' 
        ' grdAtasamente
        ' 
        grdAtasamente.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grdAtasamente.BackColor = SystemColors.Window
        grdAtasamente.ColumnFillMode = KBotFillMode.FirstColumn
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
        tlyLista.SetColumnSpan(grdAtasamente, 3)
        grdAtasamente.Dock = DockStyle.Fill
        grdAtasamente.FillColumnKey = "nume_fisier"
        grdAtasamente.HeaderBackColor = SystemColors.Control
        grdAtasamente.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdAtasamente.HeaderSeparatorColor = SystemColors.ActiveBorder
        grdAtasamente.Location = New Point(0, 0)
        grdAtasamente.Margin = New Padding(0)
        grdAtasamente.Name = "grdAtasamente"
        grdAtasamente.RowHeight = 22
        grdAtasamente.Size = New Size(410, 500)
        grdAtasamente.TabIndex = 1
        ' 
        ' pnlPreview
        ' 
        pnlPreview.Controls.Add(picPreview)
        pnlPreview.Controls.Add(lblPreviewGol)
        pnlPreview.Dock = DockStyle.Fill
        pnlPreview.Location = New Point(0, 0)
        pnlPreview.Margin = New Padding(0)
        pnlPreview.Name = "pnlPreview"
        pnlPreview.Size = New Size(435, 550)
        pnlPreview.TabIndex = 0
        ' 
        ' picPreview
        ' 
        picPreview.Dock = DockStyle.Fill
        picPreview.Location = New Point(0, 0)
        picPreview.Margin = New Padding(4, 5, 4, 5)
        picPreview.Name = "picPreview"
        picPreview.Size = New Size(435, 550)
        picPreview.SizeMode = PictureBoxSizeMode.Zoom
        picPreview.TabIndex = 0
        picPreview.TabStop = False
        ' 
        ' lblPreviewGol
        ' 
        lblPreviewGol.Dock = DockStyle.Fill
        lblPreviewGol.Font = New Font("Segoe UI", 10F)
        lblPreviewGol.Location = New Point(0, 0)
        lblPreviewGol.Margin = New Padding(4, 0, 4, 0)
        lblPreviewGol.Name = "lblPreviewGol"
        lblPreviewGol.Size = New Size(435, 550)
        lblPreviewGol.TabIndex = 1
        lblPreviewGol.Text = "Selectați o imagine din listă."
        lblPreviewGol.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' OrdAtasamentePage
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Margin = New Padding(4, 5, 4, 5)
        Name = "OrdAtasamentePage"
        Size = New Size(1187, 560)
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
        pnlPreview.ResumeLayout(False)
        CType(picPreview, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents dlgImagine As OpenFileDialog
    Friend WithEvents split As SplitContainer
    Friend WithEvents grdBene As KBot.Controls.KBotDataView
    Friend WithEvents splitDreapta As SplitContainer
    Friend WithEvents tlyLista As TableLayoutPanel
    Friend WithEvents grdAtasamente As KBot.Controls.KBotDataView
    Friend WithEvents btnAdauga As Button
    Friend WithEvents pnlPreview As Panel
    Friend WithEvents picPreview As PictureBox
    Friend WithEvents lblPreviewGol As Label
    Friend WithEvents btnSterge As Button
    Friend WithEvents btnLipeste As Button
End Class
