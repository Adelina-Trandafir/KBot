Imports KBot.Controls

' «Sectiunea A» of the DDF editor (slice 0051) -- the port of `frmFX_DDF_REV_SECT_A`.
' The only grid in the editor that can be edited at all -- and even here, ONLY when the
' document was NOT generated from FX_Rezervari. A DDF that comes from reservations is read
' back exactly as the reservations made it; the grid unlocks for a manually built document,
' which is a path that does not exist yet. `AplicaModulDeEditare` is where that is decided.
'
' Columns, left to right as in Access. They are declared HERE, not built in code:
'   Clasificatie          combo over GET /api/forexe/ddf/clasificatii      EDITABLE
'   Clsf                  follows the combo                                read-only
'   Element fundamentare                                                   EDITABLE
'   Parametrii                                                             EDITABLE
'   Partener              only when the document has a partener            EDITABLE (gated)
'   Buget                 display only -- NO column on FX_DDF_REV_SA       read-only
'   Val. receptii         display only -- NO column on FX_DDF_REV_SA       read-only
'   Val. precedenta                                                        read-only
'   Val. curenta                                                           EDITABLE
'   Val. totala           computed                                          read-only
'
' `Buget` and `ValRec` exist on Access's `tmpFX_DDF_REV_SA` but NOT on `FX_DDF_REV_SA`. They
' ride on the draft for display and are dropped at the wire; no column was added for them.
'
' READ-ONLY IS `ReadOnly`, NEVER `Enabled = False`. `KBotDataColumn.Enabled = False` draws the
' whole column GREYED OUT -- it is the "this control is off" look, and a grid full of it reads
' as a disabled grid, which is not what a document whose lines merely cannot be retyped should
' look like. Every column here used to carry it, which greyed the entire grid AND made even the
' five editable columns inert, so `AplicaModulDeEditare`'s unlock could never actually unlock
' anything. The derived columns say `ReadOnly = True` instead, and the whole-grid lock for a
' document generated from reservations is `grd.ReadOnlyGrid`: both refuse the edit and neither
' touches the paint. No grid in `OrdEditForm` uses `Enabled = False` either.
'
' There is NO `btnClsf`. Access had one, but its only appearance outside the designer is in a
' function whose first statement is `Exit Function` -- a button that does nothing is exactly
' the silent no-op the house rules forbid, and it was already refused once, in slice 0049.
' The tree picker arrives by replacing the body of `AlegeClasificatie`, and nothing else.
'
' All controls are declared HERE (docs/kbot-forms-ui-convention.md).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfEditSectiuneaAPage
    Inherits KBot.Theming.KBotThemedUserControl

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
        Dim KBotDataColumn9 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn10 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        btnAdauga = New Button()
        btnSterge = New Button()
        tlyRoot = New TableLayoutPanel()
        grd = New KBotDataView()
        tlyButoane = New TableLayoutPanel()
        lblStare = New Label()
        tlyRoot.SuspendLayout()
        CType(grd, ComponentModel.ISupportInitialize).BeginInit()
        tlyButoane.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnAdauga
        ' 
        btnAdauga.Dock = DockStyle.Fill
        btnAdauga.Location = New Point(11, 8)
        btnAdauga.Margin = New Padding(11, 8, 6, 8)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Size = New Size(269, 47)
        btnAdauga.TabIndex = 0
        btnAdauga.Text = "Adaugă rând"
        tips.SetToolTipHeader(btnAdauga, "Adaugă un rând")
        tips.SetToolTipText(btnAdauga, "Adaugă o linie nouă și deschide lista de clasificații." & vbLf & "Clasificațiile deja folosite nu apar în listă.")
        btnAdauga.UseVisualStyleBackColor = True
        ' 
        ' btnSterge
        ' 
        btnSterge.Dock = DockStyle.Fill
        btnSterge.Location = New Point(292, 8)
        btnSterge.Margin = New Padding(6, 8, 6, 8)
        btnSterge.Name = "btnSterge"
        btnSterge.Size = New Size(274, 47)
        btnSterge.TabIndex = 1
        btnSterge.Text = "Șterge rândul"
        tips.SetToolTipHeader(btnSterge, "Șterge rândul")
        tips.SetToolTipText(btnSterge, "Scoate linia selectată." & vbLf & "Rândul pereche din secțiunea B dispare odată cu ea.")
        btnSterge.UseVisualStyleBackColor = True
        ' 
        ' tlyRoot
        ' 
        tlyRoot.ColumnCount = 1
        tlyRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRoot.Controls.Add(grd, 0, 0)
        tlyRoot.Controls.Add(tlyButoane, 0, 1)
        tlyRoot.Dock = DockStyle.Fill
        tlyRoot.Location = New Point(0, 0)
        tlyRoot.Margin = New Padding(0)
        tlyRoot.Name = "tlyRoot"
        tlyRoot.RowCount = 2
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 63F))
        tlyRoot.Size = New Size(980, 603)
        tlyRoot.TabIndex = 0
        ' 
        ' grd
        ' 
        grd.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grd.BackColor = SystemColors.Window
        grd.BorderColor = SystemColors.ActiveBorder
        grd.ColumnFillMode = KBotFillMode.FirstColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn1.ColumnType = KBotColumnType.Combo
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn1.HeaderText = "Clasificație"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clasificatie"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Visible = False
        KBotDataColumn1.Width = 300
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn2.HeaderText = "Clsf"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "clsf"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 170
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn3.HeaderText = "Element fundamentare"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "element_fund"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.Width = 190
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn4.HeaderText = "Parametrii"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "parametrii_fund"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn5.HeaderText = "Partener"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "cod_partener"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Visible = False
        KBotDataColumn5.Width = 150
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn6.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn6.HeaderText = "Buget"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "buget"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn7.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn7.HeaderText = "Val. recepții"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "val_rec"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn8.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn8.DecimalPlaces = 2
        KBotDataColumn8.Format = KBotFormat.Standard
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn8.HeaderText = "Val. precedentă"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "val_prec"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn8.ValueType = KBotValueType.Number
        KBotDataColumn9.Aggregate = KBotAggregate.Sum
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn9.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn9.HeaderText = "Val. curentă"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "val_cur"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBotValueType.Number
        KBotDataColumn10.Aggregate = KBotAggregate.Sum
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn10.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn10.DecimalPlaces = 2
        KBotDataColumn10.Format = KBotFormat.Standard
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn10.HeaderText = "Val. totală"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "val_tot"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn10.ValueType = KBotValueType.Number
        grd.Columns.Add(KBotDataColumn1)
        grd.Columns.Add(KBotDataColumn2)
        grd.Columns.Add(KBotDataColumn3)
        grd.Columns.Add(KBotDataColumn4)
        grd.Columns.Add(KBotDataColumn5)
        grd.Columns.Add(KBotDataColumn6)
        grd.Columns.Add(KBotDataColumn7)
        grd.Columns.Add(KBotDataColumn8)
        grd.Columns.Add(KBotDataColumn9)
        grd.Columns.Add(KBotDataColumn10)
        grd.Dock = DockStyle.Fill
        grd.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        grd.FooterBackColor = SystemColors.Control
        grd.FooterFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grd.FooterSeparatorColor = SystemColors.ActiveBorder
        grd.FooterVisible = True
        grd.HeaderBackColor = SystemColors.Control
        grd.HeaderSeparatorColor = SystemColors.ActiveBorder
        grd.Location = New Point(6, 7)
        grd.Margin = New Padding(6, 7, 6, 7)
        grd.Name = "grd"
        grd.ShrinkColumnsToFit = False
        grd.Size = New Size(968, 526)
        grd.TabIndex = 0
        ' 
        ' tlyButoane
        ' 
        tlyButoane.ColumnCount = 3
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 286F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 286F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButoane.Controls.Add(btnAdauga, 0, 0)
        tlyButoane.Controls.Add(btnSterge, 1, 0)
        tlyButoane.Controls.Add(lblStare, 2, 0)
        tlyButoane.Dock = DockStyle.Fill
        tlyButoane.Location = New Point(0, 540)
        tlyButoane.Margin = New Padding(0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.RowCount = 1
        tlyButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButoane.Size = New Size(980, 63)
        tlyButoane.TabIndex = 1
        ' 
        ' lblStare
        ' 
        lblStare.AutoSize = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Font = New Font("Calibri", 9F)
        lblStare.Location = New Point(578, 0)
        lblStare.Margin = New Padding(6, 0, 11, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(391, 63)
        lblStare.TabIndex = 2
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' DdfEditSectiuneaAPage
        ' 
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(tlyRoot)
        Margin = New Padding(0)
        Name = "DdfEditSectiuneaAPage"
        Size = New Size(980, 603)
        tlyRoot.ResumeLayout(False)
        CType(grd, ComponentModel.ISupportInitialize).EndInit()
        tlyButoane.ResumeLayout(False)
        tlyButoane.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents tlyRoot As TableLayoutPanel
    Friend WithEvents grd As KBot.Controls.KBotDataView
    Friend WithEvents tlyButoane As TableLayoutPanel
    Friend WithEvents btnAdauga As Button
    Friend WithEvents btnSterge As Button
    Friend WithEvents lblStare As Label
End Class
