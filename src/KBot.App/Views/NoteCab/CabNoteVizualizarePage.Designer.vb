Imports KBot.Controls

' «Vizualizare» of the «Note corecție» view (slice 0088): the rows of the selected note(s) as the
' F1135 form prints them, and one line of state (signed / sent into CAB) above them.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CabNoteVizualizarePage
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
        Dim KBotDataColumn10 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn11 As KBotDataColumn = New KBotDataColumn()
        grdRanduri = New KBotDataView()
        lblStare = New Label()
        lblEmpty = New Label()
        CType(grdRanduri, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' grdRanduri
        '
        grdRanduri.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grdRanduri.BackColor = SystemColors.Window
        grdRanduri.ColumnFillMode = KBotFillMode.LastColumn
        KBotDataColumn1.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn1.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn1.HeaderText = "Nota"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "nota"
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 60
        KBotDataColumn2.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn2.HeaderText = "Nr. rd."
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "rand"
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 50
        KBotDataColumn3.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn3.HeaderText = "Simbol cont"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "simbol"
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 130
        KBotDataColumn4.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn4.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn4.HeaderText = "Cod program"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "program"
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 100
        KBotDataColumn5.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn5.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn5.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn5.HeaderText = "Cod angajament"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "angajament"
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 110
        KBotDataColumn6.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn6.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn6.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn6.HeaderText = "Indicator"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "indicator"
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Width = 70
        KBotDataColumn7.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn7.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn7.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn7.HeaderText = "Nr. ref. oper. inițială"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "referinta"
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Width = 130
        KBotDataColumn8.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn8.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn8.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn8.HeaderText = "Data oper. inițială"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "data"
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Width = 90
        KBotDataColumn9.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn9.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = KBotFormat.Standard
        KBotDataColumn9.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn9.HeaderText = "Suma debit"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "debit"
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBotValueType.Number
        KBotDataColumn9.Width = 90
        KBotDataColumn10.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn10.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn10.DecimalPlaces = 2
        KBotDataColumn10.Format = KBotFormat.Standard
        KBotDataColumn10.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn10.HeaderText = "Suma credit"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "credit"
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn10.ValueType = KBotValueType.Number
        KBotDataColumn10.Width = 90
        KBotDataColumn11.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn11.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn11.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn11.HeaderText = "Explicații"
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "explicatii"
        KBotDataColumn11.ReadOnly = True
        KBotDataColumn11.Width = 220
        grdRanduri.Columns.Add(KBotDataColumn1)
        grdRanduri.Columns.Add(KBotDataColumn2)
        grdRanduri.Columns.Add(KBotDataColumn3)
        grdRanduri.Columns.Add(KBotDataColumn4)
        grdRanduri.Columns.Add(KBotDataColumn5)
        grdRanduri.Columns.Add(KBotDataColumn6)
        grdRanduri.Columns.Add(KBotDataColumn7)
        grdRanduri.Columns.Add(KBotDataColumn8)
        grdRanduri.Columns.Add(KBotDataColumn9)
        grdRanduri.Columns.Add(KBotDataColumn10)
        grdRanduri.Columns.Add(KBotDataColumn11)
        grdRanduri.Dock = DockStyle.Fill
        grdRanduri.Location = New Point(0, 44)
        grdRanduri.Margin = New Padding(0)
        grdRanduri.Name = "grdRanduri"
        grdRanduri.ReadOnlyGrid = True
        grdRanduri.ShrinkColumnsToFit = False
        grdRanduri.Size = New Size(849, 444)
        grdRanduri.TabIndex = 1
        '
        ' lblStare
        '
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Top
        lblStare.Location = New Point(0, 0)
        lblStare.Margin = New Padding(4, 0, 4, 0)
        lblStare.Name = "lblStare"
        lblStare.Padding = New Padding(8, 0, 8, 0)
        lblStare.Size = New Size(849, 44)
        lblStare.TabIndex = 0
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblEmpty
        '
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Margin = New Padding(4, 0, 4, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(849, 488)
        lblEmpty.TabIndex = 2
        lblEmpty.Text = "Selectați o notă de corecție din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        lblEmpty.Visible = False
        '
        ' CabNoteVizualizarePage
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(grdRanduri)
        Controls.Add(lblStare)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "CabNoteVizualizarePage"
        Size = New Size(849, 488)
        CType(grdRanduri, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents grdRanduri As KBotDataView
    Friend WithEvents lblStare As Label
    Friend WithEvents lblEmpty As Label
End Class
