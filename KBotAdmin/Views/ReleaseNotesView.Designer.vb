<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ReleaseNotesView
    Inherits Global.KBot.Theming.KBotThemedUserControl

    Private Sub InitializeComponent()
        Dim tlyMain As New KBotTableLayoutPanel()
        Dim tlyMove As New KBotTableLayoutPanel()
        Dim lblHelp As New Label()
        Dim lblTarget As New Label()
        Dim colVersion As New KBotDataColumn("version", "Versiune", KBotColumnType.Text, 120)
        Dim colOrder As New KBotDataColumn("order", String.Empty, KBotColumnType.Text, 40)
        Dim colNote As New KBotDataColumn("note", "Noutate", KBotColumnType.Text, 500)
        Dim groupVersion As New KBotGroupLevel("version", KBotSortDirection.Descending)
        gridNotes = New AdminDataView()
        cmbTarget = New KBotComboBox()
        btnMove = New KBotButton()
        lblStatus = New Label()

        SuspendLayout()
        tlyMain.SuspendLayout()
        tlyMove.SuspendLayout()

        lblHelp.Dock = DockStyle.Fill
        lblHelp.TextAlign = ContentAlignment.MiddleLeft
        lblHelp.Text = "Deschideți o versiune și scrieți pe rândul gol. Apăsați Enter pentru a adăuga noutatea."

        colVersion.Visible = KBotColumnVisibility.Hidden
        colOrder.Visible = KBotColumnVisibility.Hidden
        colOrder.ValueType = KBotValueType.Number
        colNote.ColumnFont = New Font("Calibri", 11.0F)
        colNote.Resizable = False
        groupVersion.HeaderCaptionFormat = "{1}"
        groupVersion.HeaderFont = New Font("Calibri", 11.0F, FontStyle.Bold)
        groupVersion.ShowFooter = False
        groupVersion.Collapsible = True
        groupVersion.CollapsedByDefault = True
        gridNotes.Columns.Add(colVersion)
        gridNotes.Columns.Add(colOrder)
        gridNotes.Columns.Add(colNote)
        gridNotes.Groups.Add(groupVersion)
        gridNotes.CheckBoxes = KBotCheckBoxMode.All
        gridNotes.FillColumnKey = "note"
        gridNotes.ColumnFillMode = KBotFillMode.SpecificColumn
        gridNotes.Dock = DockStyle.Fill
        gridNotes.TabIndex = 0

        lblTarget.Dock = DockStyle.Fill
        lblTarget.TextAlign = ContentAlignment.MiddleLeft
        lblTarget.Text = "Mută în versiunea"
        cmbTarget.Dock = DockStyle.Fill
        cmbTarget.Editable = False
        cmbTarget.PlaceholderText = "Alegeți versiunea"
        cmbTarget.AccessibleName = "Versiunea de destinație"
        cmbTarget.TabIndex = 0
        btnMove.Dock = DockStyle.Fill
        btnMove.Enabled = False
        btnMove.Text = "Mută noutățile bifate"
        btnMove.TabIndex = 1

        tlyMove.Dock = DockStyle.Fill
        tlyMove.Margin = New Padding(0)
        tlyMove.AutoFitToTheme = False
        tlyMove.ColumnCount = 4
        tlyMove.RowCount = 1
        tlyMove.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        tlyMove.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 155))
        tlyMove.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190))
        tlyMove.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        tlyMove.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        lblTarget.AutoSize = True
        tlyMove.Controls.Add(lblTarget, 0, 0)
        tlyMove.Controls.Add(cmbTarget, 1, 0)
        tlyMove.Controls.Add(btnMove, 2, 0)
        tlyMove.TabIndex = 1

        lblStatus.Dock = DockStyle.Fill
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        lblStatus.AutoEllipsis = True

        tlyMain.Dock = DockStyle.Fill
        tlyMain.Padding = New Padding(8)
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.RowCount = 4
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 36))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 30))
        tlyMain.Controls.Add(lblHelp, 0, 0)
        tlyMain.Controls.Add(gridNotes, 0, 1)
        tlyMain.Controls.Add(tlyMove, 0, 2)
        tlyMain.Controls.Add(lblStatus, 0, 3)

        AutoScaleDimensions = New SizeF(96, 96)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "ReleaseNotesView"
        Size = New Size(900, 600)
        tlyMove.ResumeLayout(False)
        tlyMain.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents gridNotes As AdminDataView
    Friend WithEvents cmbTarget As KBotComboBox
    Friend WithEvents btnMove As KBotButton
    Friend lblStatus As Label
End Class
