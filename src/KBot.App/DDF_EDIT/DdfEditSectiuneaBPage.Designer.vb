Imports KBot.Controls

' «Sectiunea B» of the DDF editor (slice 0051) -- the port of `frmFX_DDF_REV_SECT_B`.
'
' READ-ONLY, and that is a decision, not an omission (D8). In Access the grid was editable in
' principle, through `Inf1_AfterUpdate` / `Inf2_BeforeUpdate`, but in practice it was not
' used: every value here is derived from section A, and an override would put two numbers
' that must agree in a position to disagree. So section B is RECOMPUTED IN FULL from section
' A on every change, the server writes what it receives, and any stored override is replaced.
' The two manual-override handlers are therefore not ported.
'
' The totals row sums `Inf1` and `Inf2`, which is what Access's `=Sum([Inf1])` /
' `=Sum([Inf2])` footers showed.
'
' THE GRID IS DRESSED LIKE SECTION A'S, on purpose: same column and header fonts, same band
' colours, same money format (`Standard` -- thousand separators, as in every grid of
' `OrdEditForm`), same fill behaviour. The two sit one nav click apart and show the same
' document; a different font or a number written two ways between them reads as two
' applications. `AutoScaleDimensions` is (9, 22) for the same reason -- it was 7x15 here and
' 10x25 on the other three pages of this form, so this one alone came out scaled up beside
' its siblings. All four now carry the Calibri 9 pair at 144 dpi (slice 0052).
'
' All controls are declared HERE (docs/kbot-forms-ui-convention.md).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfEditSectiuneaBPage
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn9 As KBotDataColumn = New KBotDataColumn()
        components = New ComponentModel.Container()
        tips = New KBotToolTip(components)
        tlyRoot = New TableLayoutPanel()
        grd = New KBotDataView()
        lblNota = New Label()
        tlyRoot.SuspendLayout()
        CType(grd, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' tlyRoot
        '
        tlyRoot.ColumnCount = 1
        tlyRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        ' REVERSE dock order: the Fill row first, then the fixed band under it.
        tlyRoot.Controls.Add(grd, 0, 0)
        tlyRoot.Controls.Add(lblNota, 0, 1)
        tlyRoot.Dock = DockStyle.Fill
        tlyRoot.Location = New Point(0, 0)
        tlyRoot.Margin = New Padding(0)
        tlyRoot.Name = "tlyRoot"
        tlyRoot.RowCount = 2
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyRoot.Size = New Size(1196, 620)
        tlyRoot.TabIndex = 0
        '
        ' grd
        '
        grd.AlternatingRows = True
        grd.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grd.BackColor = SystemColors.Window
        grd.BorderColor = SystemColors.ActiveBorder
        grd.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn1.HeaderText = "Cod angajament"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "cod_angajament"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Width = 180
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn2.HeaderText = "Cod indicator"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "cod_indicator"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Width = 140
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn3.HeaderText = "Cod SSI"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "cod_ssi"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.Width = 170
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn4.HeaderText = "C.A. anterior"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "ca_anterior"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 130
        KBotDataColumn5.Aggregate = KBotAggregate.Sum
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn5.HeaderText = "Influențe C.A."
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "inf1"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 130
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn6.HeaderText = "C.A. curent"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "ca_curent"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 130
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn7.HeaderText = "C.B. anterior"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "cb_anterior"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn7.Width = 130
        KBotDataColumn8.Aggregate = KBotAggregate.Sum
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn8.DecimalPlaces = 2
        KBotDataColumn8.Format = KBotFormat.Standard
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn8.HeaderText = "Influențe C.B."
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "inf2"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn8.ValueType = KBotValueType.Number
        KBotDataColumn8.Width = 130
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn9.HeaderText = "C.B. curent"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "cb_curent"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBotValueType.Number
        KBotDataColumn9.Width = 130
        grd.Columns.Add(KBotDataColumn1)
        grd.Columns.Add(KBotDataColumn2)
        grd.Columns.Add(KBotDataColumn3)
        grd.Columns.Add(KBotDataColumn4)
        grd.Columns.Add(KBotDataColumn5)
        grd.Columns.Add(KBotDataColumn6)
        grd.Columns.Add(KBotDataColumn7)
        grd.Columns.Add(KBotDataColumn8)
        grd.Columns.Add(KBotDataColumn9)
        grd.Dock = DockStyle.Fill
        grd.FillColumnKey = "cod_ssi"
        grd.FooterBackColor = SystemColors.Control
        grd.FooterFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grd.FooterSeparatorColor = SystemColors.ActiveBorder
        grd.FooterVisible = True
        grd.HeaderBackColor = SystemColors.Control
        grd.HeaderSeparatorColor = SystemColors.ActiveBorder
        grd.Location = New Point(4, 4)
        grd.Margin = New Padding(4)
        grd.Name = "grd"
        grd.ReadOnlyGrid = True
        grd.RowHeight = 28
        grd.ShrinkColumnsToFit = False
        grd.Size = New Size(1188, 578)
        tips.SetToolTipHeader(grd, "Secțiunea B")
        tips.SetToolTipText(grd, "Se recalculează întreagă din secțiunea A la fiecare modificare." & vbLf & "Ce se vede aici este ce se va salva.")
        grd.TabIndex = 0
        '
        ' lblNota
        '
        lblNota.AutoSize = True
        lblNota.Dock = DockStyle.Fill
        lblNota.Font = New Font("Calibri", 9F)
        lblNota.Location = New Point(8, 586)
        lblNota.Margin = New Padding(8, 0, 8, 0)
        lblNota.Name = "lblNota"
        lblNota.Size = New Size(1180, 34)
        lblNota.TabIndex = 1
        lblNota.Text = "Secțiunea B se calculează din secțiunea A și nu se editează."
        lblNota.TextAlign = ContentAlignment.MiddleLeft
        '
        ' DdfEditSectiuneaBPage
        '
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(tlyRoot)
        Margin = New Padding(0)
        Name = "DdfEditSectiuneaBPage"
        Size = New Size(1196, 620)
        tlyRoot.ResumeLayout(False)
        tlyRoot.PerformLayout()
        CType(grd, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents tlyRoot As TableLayoutPanel
    Friend WithEvents grd As Global.KBot.Controls.KBotDataView
    Friend WithEvents lblNota As Label
End Class
