Imports KBot.Controls

' «Fisiere» of the DDF editor (slice 0051) -- the port of `frmFX_DDF_ATT`.
'
' THE GRID IS READ-ONLY, and that is not a restriction on the operator: the name and the size
' come from the file that was chosen, never from the keyboard. Adding, removing and saving to
' disk are the three buttons underneath.
'
' PRINT SCREENS ARE SHOWN. Access filtered them out (`WHERE PrtScr = False` on the form's
' record source); that filter is DELIBERATELY NOT PORTED. Everything this slice creates is
' `PrtScr = 0`; rows with `PrtScr = 1` arrive only from the future FOREXE workflow, when a
' manually created angajament is pushed and the workflow returns a print screen. They are
' visible, NOT editable, NOT deletable -- and they CAN be saved to disk, which is the whole
' reason to show them at all.
'
' Columns, left to right. They are declared HERE, not built in code -- the same rule the two
' section pages and every page of `OrdEditForm` follow. This page used to add them from a
' `ConstruiesteColoanele` called by the constructor, which is the one thing the convention
' forbids: the grid then shows nothing at design time.
'   Nume fisier   from the chosen file
'   Dimensiune    formatted, right-aligned
'   Sursa         «Atasat» or «FOREXE (print screen)»
'   Cale          the fill column
'
' The file picker's filters come from `bChoose_Click`:
'   Imagini    *.bmp;*.jpg;*.png;*.ico
'   Documente  *.doc;*.docx;*.pdf
'   Tabele     *.xls;*.xlsx
'
' THE SECOND COLUMN IS THE PREVIEW. `tlyRoot` is 65/35: the grid and the three buttons on the left,
' `prv` (cell 1;0) and `lblStare` (cell 1;1) on the right. `DdfFisierPreview` shows images, Word and
' Excel documents and PDFs; every other type gets a sentence saying so. Selecting a row is the only
' thing that drives it -- nothing about the preview can change the draft.
'
' All controls are declared HERE (docs/kbot-forms-ui-convention.md).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfEditFisierePage
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        btnAdauga = New Button()
        btnSterge = New Button()
        btnSalveazaPeDisc = New Button()
        dlgAlege = New OpenFileDialog()
        dlgSalveaza = New SaveFileDialog()
        tlyRoot = New TableLayoutPanel()
        lblStare = New Label()
        grd = New KBotDataView()
        tlyButoane = New TableLayoutPanel()
        prv = New DdfFisierPreview()
        tlyRoot.SuspendLayout()
        CType(grd, ComponentModel.ISupportInitialize).BeginInit()
        tlyButoane.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnAdauga
        ' 
        btnAdauga.BackgroundImageLayout = ImageLayout.Stretch
        btnAdauga.Dock = DockStyle.Fill
        btnAdauga.Location = New Point(11, 8)
        btnAdauga.Margin = New Padding(11, 8, 6, 8)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Size = New Size(203, 67)
        btnAdauga.TabIndex = 0
        btnAdauga.Text = "Atașează fișier"
        tips.SetToolTipHeader(btnAdauga, "Atașează un fișier")
        tips.SetToolTipText(btnAdauga, "Imagini, documente sau tabele." & vbLf & "Se încarcă pe server abia după salvarea documentului.")
        btnAdauga.UseVisualStyleBackColor = True
        ' 
        ' btnSterge
        ' 
        btnSterge.Dock = DockStyle.Fill
        btnSterge.Location = New Point(226, 8)
        btnSterge.Margin = New Padding(6, 8, 6, 8)
        btnSterge.Name = "btnSterge"
        btnSterge.Size = New Size(208, 67)
        btnSterge.TabIndex = 1
        btnSterge.Text = "Șterge fișierul"
        tips.SetToolTipHeader(btnSterge, "Șterge fișierul")
        tips.SetToolTipText(btnSterge, "Scoate fișierul din document." & vbLf & "Print screen-urile venite din FOREXE nu se pot șterge de aici.")
        btnSterge.UseVisualStyleBackColor = True
        ' 
        ' btnSalveazaPeDisc
        ' 
        btnSalveazaPeDisc.Dock = DockStyle.Fill
        btnSalveazaPeDisc.Location = New Point(446, 8)
        btnSalveazaPeDisc.Margin = New Padding(6, 8, 6, 8)
        btnSalveazaPeDisc.Name = "btnSalveazaPeDisc"
        btnSalveazaPeDisc.Size = New Size(209, 67)
        btnSalveazaPeDisc.TabIndex = 2
        btnSalveazaPeDisc.Text = "Salvează pe disc"
        tips.SetToolTipHeader(btnSalveazaPeDisc, "Salvează pe disc")
        tips.SetToolTipText(btnSalveazaPeDisc, "Scrie fișierul selectat într-un folder ales de tine." & vbLf & "Merge și pentru print screen-urile venite din FOREXE.")
        btnSalveazaPeDisc.UseVisualStyleBackColor = True
        ' 
        ' dlgAlege
        ' 
        dlgAlege.Filter = "Imagini|*.bmp;*.jpg;*.jpeg;*.png;*.ico|Documente|*.doc;*.docx;*.pdf|Tabele|*.xls;*.xlsx|Toate fișierele|*.*"
        dlgAlege.Title = "Alege fișierul de atașat"
        ' 
        ' dlgSalveaza
        ' 
        dlgSalveaza.Title = "Salvează fișierul pe disc"
        ' 
        ' tlyRoot
        ' 
        tlyRoot.ColumnCount = 2
        tlyRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 65F))
        tlyRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 35F))
        tlyRoot.Controls.Add(lblStare, 1, 1)
        tlyRoot.Controls.Add(grd, 0, 0)
        tlyRoot.Controls.Add(tlyButoane, 0, 1)
        tlyRoot.Controls.Add(prv, 1, 0)
        tlyRoot.Dock = DockStyle.Fill
        tlyRoot.Location = New Point(0, 0)
        tlyRoot.Margin = New Padding(0)
        tlyRoot.Name = "tlyRoot"
        tlyRoot.RowCount = 2
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 83F))
        tlyRoot.Size = New Size(1018, 609)
        tlyRoot.TabIndex = 0
        ' 
        ' lblStare
        ' 
        lblStare.AutoSize = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Font = New Font("Calibri", 9F)
        lblStare.Location = New Point(667, 526)
        lblStare.Margin = New Padding(6, 0, 11, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(340, 83)
        lblStare.TabIndex = 4
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' grd
        ' 
        grd.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grd.BackColor = SystemColors.Window
        grd.BorderColor = SystemColors.ActiveBorder
        grd.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn1.HeaderText = "Nume fișier"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "nume_fisier"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 320
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn2.HeaderText = "Dimensiune"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "dimensiune"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn2.Width = 140
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn3.HeaderText = "Sursă"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "sursa"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 180
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn4.HeaderText = "Cale"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "cale_fisier"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Visible = False
        KBotDataColumn4.Width = 460
        grd.Columns.Add(KBotDataColumn1)
        grd.Columns.Add(KBotDataColumn2)
        grd.Columns.Add(KBotDataColumn3)
        grd.Columns.Add(KBotDataColumn4)
        grd.Dock = DockStyle.Fill
        grd.FillColumnKey = "cale_fisier"
        grd.HeaderBackColor = SystemColors.Control
        grd.HeaderSeparatorColor = SystemColors.ActiveBorder
        grd.Location = New Point(6, 7)
        grd.Margin = New Padding(6, 7, 6, 7)
        grd.Name = "grd"
        grd.ReadOnlyGrid = True
        grd.ShrinkColumnsToFit = False
        grd.Size = New Size(649, 512)
        grd.TabIndex = 0
        ' 
        ' tlyButoane
        ' 
        tlyButoane.ColumnCount = 3
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.3333321F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.3333321F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.3333321F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 20F))
        tlyButoane.Controls.Add(btnAdauga, 0, 0)
        tlyButoane.Controls.Add(btnSterge, 1, 0)
        tlyButoane.Controls.Add(btnSalveazaPeDisc, 2, 0)
        tlyButoane.Dock = DockStyle.Fill
        tlyButoane.Location = New Point(0, 526)
        tlyButoane.Margin = New Padding(0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.RowCount = 1
        tlyButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButoane.Size = New Size(661, 83)
        tlyButoane.TabIndex = 1
        ' 
        ' prv
        ' 
        prv.Dock = DockStyle.Fill
        prv.Font = New Font("Calibri", 9F)
        prv.Location = New Point(667, 7)
        prv.Margin = New Padding(6, 7, 11, 7)
        prv.Name = "prv"
        prv.Size = New Size(340, 512)
        prv.TabIndex = 3
        ' 
        ' DdfEditFisierePage
        ' 
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(tlyRoot)
        Margin = New Padding(0)
        Name = "DdfEditFisierePage"
        Size = New Size(1018, 609)
        tlyRoot.ResumeLayout(False)
        tlyRoot.PerformLayout()
        CType(grd, ComponentModel.ISupportInitialize).EndInit()
        tlyButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents dlgAlege As OpenFileDialog
    Friend WithEvents dlgSalveaza As SaveFileDialog
    Friend WithEvents tlyRoot As TableLayoutPanel
    Friend WithEvents grd As Global.KBot.Controls.KBotDataView
    Friend WithEvents tlyButoane As TableLayoutPanel
    Friend WithEvents btnAdauga As Button
    Friend WithEvents btnSterge As Button
    Friend WithEvents btnSalveazaPeDisc As Button
    Friend WithEvents lblStare As Label
    Friend WithEvents prv As DdfFisierPreview
End Class
