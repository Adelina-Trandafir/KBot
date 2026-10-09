Option Strict On
Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MessageCatalogForm
    Inherits KBot.Theming.KBotThemedForm

    ' Debug bench for the message boxes: the grid on the left lists EVERY message box of the
    ' application (Config\mesaje_catalog.json); the panel on the right edits the selected one.
    ' House rule: every WinForms control is declared here, in .Designer.vb.

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
        Dim KBotGroupLevel1 As KBotGroupLevel = New KBotGroupLevel()
        pnlList = New Panel()
        grd = New KBotDataView()
        pnlRight = New Panel()
        tlyEdit = New KBotTableLayoutPanel()
        lblFnCap = New Label()
        lblFn = New Label()
        lblTypeCap = New Label()
        cboType = New KBotComboBox()
        lblButtonsCap = New Label()
        cboButtons = New KBotComboBox()
        lblExtraCap = New Label()
        cboExtra = New KBotComboBox()
        lblCaptionCap = New Label()
        txtCaption = New KBotTextField()
        lblHeaderCap = New Label()
        txtHeader = New KBotTextField()
        lblCloseCap = New Label()
        cboClose = New KBotComboBox()
        lblTextCap = New Label()
        rtbText = New KBotRichTextEditor()
        lblNote = New Label()
        pnlBottom = New Panel()
        lblStatus = New Label()
        btnPreview = New Button()
        btnReset = New Button()
        btnScan = New Button()
        btnSave = New Button()
        btnClose = New Button()
        pnlList.SuspendLayout()
        CType(grd, ComponentModel.ISupportInitialize).BeginInit()
        pnlRight.SuspendLayout()
        tlyEdit.SuspendLayout()
        pnlBottom.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlList
        ' 
        pnlList.Controls.Add(grd)
        pnlList.Dock = DockStyle.Fill
        pnlList.Location = New Point(0, 0)
        pnlList.Margin = New Padding(0)
        pnlList.Name = "pnlList"
        pnlList.Padding = New Padding(12, 12, 6, 12)
        pnlList.Size = New Size(985, 751)
        pnlList.TabIndex = 0
        ' 
        ' grd
        ' 
        grd.BackColor = SystemColors.Window
        grd.ColumnFillMode = KBotFillMode.LastColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Stare"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn1.Key = "stare"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Tip"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Key = "tip"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.ShowColumnFilter = True
        KBotDataColumn2.Width = 80
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Butoane"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn3.Key = "butoane"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 120
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Buton extra"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn4.Key = "extra"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn4.Width = 110
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Titlu"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn5.Key = "titlu"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 170
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Mesaj"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn6.Key = "mesaj"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn6.Width = 420
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Funcția"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn7.Key = "functia"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        grd.Columns.Add(KBotDataColumn1)
        grd.Columns.Add(KBotDataColumn2)
        grd.Columns.Add(KBotDataColumn3)
        grd.Columns.Add(KBotDataColumn4)
        grd.Columns.Add(KBotDataColumn5)
        grd.Columns.Add(KBotDataColumn6)
        grd.Columns.Add(KBotDataColumn7)
        grd.Dock = DockStyle.Fill
        grd.EnableGrouping = True
        KBotGroupLevel1.ColumnKey = "tip"
        KBotGroupLevel1.KeyPattern = Nothing
        KBotGroupLevel1.ShowFooter = False
        KBotGroupLevel1.ShowFooterAggregates = False
        grd.Groups.Add(KBotGroupLevel1)
        grd.Location = New Point(12, 12)
        grd.Margin = New Padding(0)
        grd.Name = "grd"
        grd.ReadOnlyGrid = True
        grd.Size = New Size(967, 727)
        grd.TabIndex = 0
        ' 
        ' pnlRight
        ' 
        pnlRight.Controls.Add(tlyEdit)
        pnlRight.Dock = DockStyle.Right
        pnlRight.Location = New Point(985, 0)
        pnlRight.Margin = New Padding(0)
        pnlRight.Name = "pnlRight"
        pnlRight.Size = New Size(482, 751)
        pnlRight.TabIndex = 1
        ' 
        ' tlyEdit
        ' 
        tlyEdit.ColumnCount = 2
        tlyEdit.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyEdit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyEdit.Controls.Add(lblFnCap, 0, 0)
        tlyEdit.Controls.Add(lblFn, 1, 0)
        tlyEdit.Controls.Add(lblTypeCap, 0, 1)
        tlyEdit.Controls.Add(cboType, 1, 1)
        tlyEdit.Controls.Add(lblButtonsCap, 0, 2)
        tlyEdit.Controls.Add(cboButtons, 1, 2)
        tlyEdit.Controls.Add(lblExtraCap, 0, 3)
        tlyEdit.Controls.Add(cboExtra, 1, 3)
        tlyEdit.Controls.Add(lblCaptionCap, 0, 4)
        tlyEdit.Controls.Add(txtCaption, 1, 4)
        tlyEdit.Controls.Add(lblHeaderCap, 0, 5)
        tlyEdit.Controls.Add(txtHeader, 1, 5)
        tlyEdit.Controls.Add(lblCloseCap, 0, 6)
        tlyEdit.Controls.Add(cboClose, 1, 6)
        tlyEdit.Controls.Add(lblTextCap, 0, 7)
        tlyEdit.Controls.Add(rtbText, 1, 7)
        tlyEdit.Controls.Add(lblNote, 0, 8)
        tlyEdit.Dock = DockStyle.Fill
        tlyEdit.Location = New Point(0, 0)
        tlyEdit.Margin = New Padding(0)
        tlyEdit.Name = "tlyEdit"
        tlyEdit.Padding = New Padding(8, 8, 8, 4)
        tlyEdit.RowCount = 9
        tlyEdit.RowStyles.Add(New RowStyle(SizeType.Absolute, 84F))
        tlyEdit.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyEdit.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyEdit.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyEdit.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyEdit.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyEdit.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyEdit.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyEdit.RowStyles.Add(New RowStyle(SizeType.Absolute, 66F))
        tlyEdit.Size = New Size(482, 751)
        tlyEdit.TabIndex = 0
        ' 
        ' lblFnCap
        ' 
        lblFnCap.Dock = DockStyle.Fill
        lblFnCap.Location = New Point(12, 8)
        lblFnCap.Margin = New Padding(4, 0, 4, 0)
        lblFnCap.Name = "lblFnCap"
        lblFnCap.Size = New Size(142, 84)
        lblFnCap.TabIndex = 0
        lblFnCap.Text = "Funcția"
        ' 
        ' lblFn
        ' 
        lblFn.Dock = DockStyle.Fill
        lblFn.Location = New Point(162, 8)
        lblFn.Margin = New Padding(4, 0, 4, 0)
        lblFn.Name = "lblFn"
        lblFn.Size = New Size(308, 84)
        lblFn.TabIndex = 1
        lblFn.Text = "-"
        ' 
        ' lblTypeCap
        ' 
        lblTypeCap.Dock = DockStyle.Fill
        lblTypeCap.Location = New Point(12, 92)
        lblTypeCap.Margin = New Padding(4, 0, 4, 0)
        lblTypeCap.Name = "lblTypeCap"
        lblTypeCap.Size = New Size(142, 60)
        lblTypeCap.TabIndex = 2
        lblTypeCap.Text = "Tip"
        lblTypeCap.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboType
        ' 
        cboType.Dock = DockStyle.Fill
        cboType.Location = New Point(162, 96)
        cboType.Margin = New Padding(4)
        cboType.Name = "cboType"
        cboType.Size = New Size(308, 37)
        cboType.TabIndex = 0
        ' 
        ' lblButtonsCap
        ' 
        lblButtonsCap.Dock = DockStyle.Fill
        lblButtonsCap.Location = New Point(12, 152)
        lblButtonsCap.Margin = New Padding(4, 0, 4, 0)
        lblButtonsCap.Name = "lblButtonsCap"
        lblButtonsCap.Size = New Size(142, 60)
        lblButtonsCap.TabIndex = 3
        lblButtonsCap.Text = "Butoane"
        lblButtonsCap.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboButtons
        ' 
        cboButtons.Dock = DockStyle.Fill
        cboButtons.Location = New Point(162, 156)
        cboButtons.Margin = New Padding(4)
        cboButtons.Name = "cboButtons"
        cboButtons.Size = New Size(308, 37)
        cboButtons.TabIndex = 1
        ' 
        ' lblExtraCap
        ' 
        lblExtraCap.Dock = DockStyle.Fill
        lblExtraCap.Location = New Point(12, 212)
        lblExtraCap.Margin = New Padding(4, 0, 4, 0)
        lblExtraCap.Name = "lblExtraCap"
        lblExtraCap.Size = New Size(142, 60)
        lblExtraCap.TabIndex = 4
        lblExtraCap.Text = "Buton extra"
        lblExtraCap.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboExtra
        ' 
        cboExtra.Dock = DockStyle.Fill
        cboExtra.Editable = True
        cboExtra.LimitToList = False
        cboExtra.Location = New Point(162, 216)
        cboExtra.Margin = New Padding(4)
        cboExtra.Name = "cboExtra"
        cboExtra.Size = New Size(308, 37)
        cboExtra.TabIndex = 2
        ' 
        ' lblCaptionCap
        ' 
        lblCaptionCap.Dock = DockStyle.Fill
        lblCaptionCap.Location = New Point(12, 272)
        lblCaptionCap.Margin = New Padding(4, 0, 4, 0)
        lblCaptionCap.Name = "lblCaptionCap"
        lblCaptionCap.Size = New Size(142, 60)
        lblCaptionCap.TabIndex = 5
        lblCaptionCap.Text = "Titlu"
        lblCaptionCap.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtCaption
        ' 
        txtCaption.Dock = DockStyle.Fill
        txtCaption.Location = New Point(162, 276)
        txtCaption.Margin = New Padding(4)
        txtCaption.Name = "txtCaption"
        txtCaption.Size = New Size(308, 52)
        txtCaption.TabIndex = 3
        ' 
        ' lblHeaderCap
        ' 
        lblHeaderCap.Dock = DockStyle.Fill
        lblHeaderCap.Location = New Point(12, 332)
        lblHeaderCap.Margin = New Padding(4, 0, 4, 0)
        lblHeaderCap.Name = "lblHeaderCap"
        lblHeaderCap.Size = New Size(142, 60)
        lblHeaderCap.TabIndex = 6
        lblHeaderCap.Text = "Antet"
        lblHeaderCap.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtHeader
        ' 
        txtHeader.Dock = DockStyle.Fill
        txtHeader.Location = New Point(162, 336)
        txtHeader.Margin = New Padding(4)
        txtHeader.Name = "txtHeader"
        txtHeader.Size = New Size(308, 52)
        txtHeader.TabIndex = 4
        ' 
        ' lblCloseCap
        ' 
        lblCloseCap.Dock = DockStyle.Fill
        lblCloseCap.Location = New Point(12, 392)
        lblCloseCap.Margin = New Padding(4, 0, 4, 0)
        lblCloseCap.Name = "lblCloseCap"
        lblCloseCap.Size = New Size(142, 60)
        lblCloseCap.TabIndex = 7
        lblCloseCap.Text = "Butonul X"
        lblCloseCap.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboClose
        ' 
        cboClose.Dock = DockStyle.Fill
        cboClose.Location = New Point(162, 396)
        cboClose.Margin = New Padding(4)
        cboClose.Name = "cboClose"
        cboClose.Size = New Size(308, 37)
        cboClose.TabIndex = 5
        ' 
        ' lblTextCap
        ' 
        lblTextCap.Dock = DockStyle.Fill
        lblTextCap.Location = New Point(12, 452)
        lblTextCap.Margin = New Padding(4, 0, 4, 0)
        lblTextCap.Name = "lblTextCap"
        lblTextCap.Size = New Size(142, 229)
        lblTextCap.TabIndex = 8
        lblTextCap.Text = "Mesaj"
        ' 
        ' rtbText
        ' 
        rtbText.Dock = DockStyle.Fill
        rtbText.Font = New Font("Calibri", 9F)
        rtbText.HeaderVisible = False
        rtbText.Location = New Point(158, 452)
        rtbText.Margin = New Padding(0)
        rtbText.Name = "rtbText"
        rtbText.Size = New Size(316, 229)
        rtbText.TabIndex = 4
        ' 
        ' lblNote
        ' 
        tlyEdit.SetColumnSpan(lblNote, 2)
        lblNote.Dock = DockStyle.Fill
        lblNote.Location = New Point(12, 681)
        lblNote.Margin = New Padding(4, 0, 4, 0)
        lblNote.Name = "lblNote"
        lblNote.Size = New Size(458, 66)
        lblNote.TabIndex = 9
        lblNote.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlBottom
        ' 
        pnlBottom.Controls.Add(lblStatus)
        pnlBottom.Controls.Add(btnPreview)
        pnlBottom.Controls.Add(btnReset)
        pnlBottom.Controls.Add(btnScan)
        pnlBottom.Controls.Add(btnSave)
        pnlBottom.Controls.Add(btnClose)
        pnlBottom.Dock = DockStyle.Bottom
        pnlBottom.Location = New Point(0, 751)
        pnlBottom.Margin = New Padding(0)
        pnlBottom.Name = "pnlBottom"
        pnlBottom.Padding = New Padding(6)
        pnlBottom.Size = New Size(1467, 84)
        pnlBottom.TabIndex = 2
        ' 
        ' lblStatus
        ' 
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Location = New Point(6, 6)
        lblStatus.Margin = New Padding(4, 0, 4, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Padding = New Padding(15, 0, 0, 0)
        lblStatus.Size = New Size(375, 72)
        lblStatus.TabIndex = 0
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnPreview
        ' 
        btnPreview.Dock = DockStyle.Right
        btnPreview.FlatStyle = FlatStyle.Flat
        btnPreview.Location = New Point(381, 6)
        btnPreview.Margin = New Padding(4)
        btnPreview.Name = "btnPreview"
        btnPreview.Size = New Size(225, 72)
        btnPreview.TabIndex = 1
        btnPreview.Text = "Previzualizează"
        btnPreview.UseVisualStyleBackColor = True
        ' 
        ' btnReset
        ' 
        btnReset.Dock = DockStyle.Right
        btnReset.FlatStyle = FlatStyle.Flat
        btnReset.Location = New Point(606, 6)
        btnReset.Margin = New Padding(4)
        btnReset.Name = "btnReset"
        btnReset.Size = New Size(225, 72)
        btnReset.TabIndex = 4
        btnReset.Text = "Revino la cod"
        btnReset.UseVisualStyleBackColor = True
        ' 
        ' btnScan
        ' 
        btnScan.Dock = DockStyle.Right
        btnScan.FlatStyle = FlatStyle.Flat
        btnScan.Location = New Point(831, 6)
        btnScan.Margin = New Padding(4)
        btnScan.Name = "btnScan"
        btnScan.Size = New Size(225, 72)
        btnScan.TabIndex = 5
        btnScan.Text = "Actualizează"
        btnScan.UseVisualStyleBackColor = True
        ' 
        ' btnSave
        ' 
        btnSave.Dock = DockStyle.Right
        btnSave.FlatStyle = FlatStyle.Flat
        btnSave.Location = New Point(1056, 6)
        btnSave.Margin = New Padding(4)
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(225, 72)
        btnSave.TabIndex = 2
        btnSave.Text = "Salvează în JSON"
        btnSave.UseVisualStyleBackColor = True
        ' 
        ' btnClose
        ' 
        btnClose.DialogResult = DialogResult.OK
        btnClose.Dock = DockStyle.Right
        btnClose.FlatStyle = FlatStyle.Flat
        btnClose.Location = New Point(1281, 6)
        btnClose.Margin = New Padding(4)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(180, 72)
        btnClose.TabIndex = 3
        btnClose.Text = "Închide"
        btnClose.UseVisualStyleBackColor = True
        ' 
        ' MessageCatalogForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnClose
        ClientSize = New Size(1467, 835)
        Controls.Add(pnlList)
        Controls.Add(pnlRight)
        Controls.Add(pnlBottom)
        Margin = New Padding(4)
        MinimumSize = New Size(1489, 812)
        Name = "MessageCatalogForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Mesaje (debug) - toate casetele de mesaj ale aplicatiei"
        pnlList.ResumeLayout(False)
        CType(grd, ComponentModel.ISupportInitialize).EndInit()
        pnlRight.ResumeLayout(False)
        tlyEdit.ResumeLayout(False)
        pnlBottom.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlList As Panel
    Friend WithEvents grd As KBotDataView
    Friend WithEvents pnlRight As Panel
    Friend WithEvents tlyEdit As KBotTableLayoutPanel
    Friend WithEvents lblFnCap As Label
    Friend WithEvents lblFn As Label
    Friend WithEvents lblTypeCap As Label
    Friend WithEvents cboType As KBotComboBox
    Friend WithEvents lblButtonsCap As Label
    Friend WithEvents cboButtons As KBotComboBox
    Friend WithEvents lblExtraCap As Label
    Friend WithEvents cboExtra As KBotComboBox
    Friend WithEvents lblCaptionCap As Label
    Friend WithEvents txtCaption As KBotTextField
    Friend WithEvents lblHeaderCap As Label
    Friend WithEvents txtHeader As KBotTextField
    Friend WithEvents lblCloseCap As Label
    Friend WithEvents cboClose As KBotComboBox
    Friend WithEvents lblTextCap As Label
    Friend WithEvents rtbText As KBotRichTextEditor
    Friend WithEvents lblNote As Label
    Friend WithEvents pnlBottom As Panel
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnPreview As Button
    Friend WithEvents btnSave As Button
    Friend WithEvents btnScan As Button
    Friend WithEvents btnReset As Button
    Friend WithEvents btnClose As Button

End Class
