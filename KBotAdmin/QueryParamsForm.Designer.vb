' The parameter window of a catalog query (tree group «_Interogari» of the page «Tabele»): what the query does, on which database it runs, and one row per parameter
' (label, field, explanation). There is room for 6 parameters; the rows a query does not use are collapsed by the code. All controls are declared HERE;
' coordinates are in the 96 dpi the form was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class QueryParamsForm
    Inherits Global.KBot.Theming.KBotThemedForm

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
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        lblDescription = New Label()
        lblDatabase = New Label()
        tlyParams = New KBotTableLayoutPanel()
        lblP1 = New Label()
        txtP1 = New KBotTextBox()
        lblE1 = New Label()
        lblP2 = New Label()
        txtP2 = New KBotTextBox()
        lblE2 = New Label()
        lblP3 = New Label()
        txtP3 = New KBotTextBox()
        lblE3 = New Label()
        lblP4 = New Label()
        txtP4 = New KBotTextBox()
        lblE4 = New Label()
        lblP5 = New Label()
        txtP5 = New KBotTextBox()
        lblE5 = New Label()
        lblP6 = New Label()
        txtP6 = New KBotTextBox()
        lblE6 = New Label()
        tlyButtons = New KBotTableLayoutPanel()
        btnCancel = New KBotButton()
        btnOk = New KBotButton()
        tlyMain.SuspendLayout()
        tlyParams.SuspendLayout()
        tlyButtons.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(lblDescription, 0, 0)
        tlyMain.Controls.Add(lblDatabase, 0, 1)
        tlyMain.Controls.Add(tlyParams, 0, 2)
        tlyMain.Controls.Add(tlyButtons, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(16, 16, 16, 12)
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 84F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(780, 528)
        tlyMain.TabIndex = 0
        '
        ' lblDescription: what the query does (filled by the code)
        '
        lblDescription.Dock = DockStyle.Fill
        lblDescription.Location = New Point(16, 16)
        lblDescription.Margin = New Padding(0)
        lblDescription.Name = "lblDescription"
        lblDescription.Size = New Size(748, 84)
        lblDescription.TabIndex = 0
        lblDescription.TextAlign = ContentAlignment.TopLeft
        '
        ' lblDatabase: the database it runs on (filled by the code)
        '
        lblDatabase.Dock = DockStyle.Fill
        lblDatabase.Location = New Point(16, 100)
        lblDatabase.Margin = New Padding(0)
        lblDatabase.Name = "lblDatabase"
        lblDatabase.Size = New Size(748, 28)
        lblDatabase.TabIndex = 1
        lblDatabase.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyParams
        '
        tlyParams.AutoFitToTheme = False
        tlyParams.ColumnCount = 3
        tlyParams.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyParams.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyParams.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyParams.Controls.Add(lblP1, 0, 0)
        tlyParams.Controls.Add(txtP1, 1, 0)
        tlyParams.Controls.Add(lblE1, 2, 0)
        tlyParams.Controls.Add(lblP2, 0, 1)
        tlyParams.Controls.Add(txtP2, 1, 1)
        tlyParams.Controls.Add(lblE2, 2, 1)
        tlyParams.Controls.Add(lblP3, 0, 2)
        tlyParams.Controls.Add(txtP3, 1, 2)
        tlyParams.Controls.Add(lblE3, 2, 2)
        tlyParams.Controls.Add(lblP4, 0, 3)
        tlyParams.Controls.Add(txtP4, 1, 3)
        tlyParams.Controls.Add(lblE4, 2, 3)
        tlyParams.Controls.Add(lblP5, 0, 4)
        tlyParams.Controls.Add(txtP5, 1, 4)
        tlyParams.Controls.Add(lblE5, 2, 4)
        tlyParams.Controls.Add(lblP6, 0, 5)
        tlyParams.Controls.Add(txtP6, 1, 5)
        tlyParams.Controls.Add(lblE6, 2, 5)
        tlyParams.Dock = DockStyle.Fill
        tlyParams.Location = New Point(16, 128)
        tlyParams.Margin = New Padding(0)
        tlyParams.Name = "tlyParams"
        tlyParams.RowCount = 6
        tlyParams.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyParams.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyParams.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyParams.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyParams.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyParams.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyParams.Size = New Size(748, 336)
        tlyParams.TabIndex = 2
        '
        ' parameter 1: label, field, explanation
        '
        lblP1.Dock = DockStyle.Fill
        lblP1.Location = New Point(0, 0)
        lblP1.Margin = New Padding(0, 0, 8, 0)
        lblP1.Name = "lblP1"
        lblP1.Size = New Size(162, 56)
        lblP1.TabIndex = 101
        lblP1.TextAlign = ContentAlignment.MiddleLeft
        txtP1.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtP1.Location = New Point(170, 12)
        txtP1.Margin = New Padding(0, 0, 12, 0)
        txtP1.Name = "txtP1"
        txtP1.Size = New Size(218, 32)
        txtP1.TabIndex = 1
        lblE1.Dock = DockStyle.Fill
        lblE1.Location = New Point(400, 0)
        lblE1.Margin = New Padding(0)
        lblE1.Name = "lblE1"
        lblE1.Size = New Size(348, 56)
        lblE1.TabIndex = 201
        lblE1.TextAlign = ContentAlignment.MiddleLeft
        '
        ' parameter 2: label, field, explanation
        '
        lblP2.Dock = DockStyle.Fill
        lblP2.Location = New Point(0, 56)
        lblP2.Margin = New Padding(0, 0, 8, 0)
        lblP2.Name = "lblP2"
        lblP2.Size = New Size(162, 56)
        lblP2.TabIndex = 102
        lblP2.TextAlign = ContentAlignment.MiddleLeft
        txtP2.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtP2.Location = New Point(170, 68)
        txtP2.Margin = New Padding(0, 0, 12, 0)
        txtP2.Name = "txtP2"
        txtP2.Size = New Size(218, 32)
        txtP2.TabIndex = 2
        lblE2.Dock = DockStyle.Fill
        lblE2.Location = New Point(400, 56)
        lblE2.Margin = New Padding(0)
        lblE2.Name = "lblE2"
        lblE2.Size = New Size(348, 56)
        lblE2.TabIndex = 202
        lblE2.TextAlign = ContentAlignment.MiddleLeft
        '
        ' parameter 3: label, field, explanation
        '
        lblP3.Dock = DockStyle.Fill
        lblP3.Location = New Point(0, 112)
        lblP3.Margin = New Padding(0, 0, 8, 0)
        lblP3.Name = "lblP3"
        lblP3.Size = New Size(162, 56)
        lblP3.TabIndex = 103
        lblP3.TextAlign = ContentAlignment.MiddleLeft
        txtP3.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtP3.Location = New Point(170, 124)
        txtP3.Margin = New Padding(0, 0, 12, 0)
        txtP3.Name = "txtP3"
        txtP3.Size = New Size(218, 32)
        txtP3.TabIndex = 3
        lblE3.Dock = DockStyle.Fill
        lblE3.Location = New Point(400, 112)
        lblE3.Margin = New Padding(0)
        lblE3.Name = "lblE3"
        lblE3.Size = New Size(348, 56)
        lblE3.TabIndex = 203
        lblE3.TextAlign = ContentAlignment.MiddleLeft
        '
        ' parameter 4: label, field, explanation
        '
        lblP4.Dock = DockStyle.Fill
        lblP4.Location = New Point(0, 168)
        lblP4.Margin = New Padding(0, 0, 8, 0)
        lblP4.Name = "lblP4"
        lblP4.Size = New Size(162, 56)
        lblP4.TabIndex = 104
        lblP4.TextAlign = ContentAlignment.MiddleLeft
        txtP4.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtP4.Location = New Point(170, 180)
        txtP4.Margin = New Padding(0, 0, 12, 0)
        txtP4.Name = "txtP4"
        txtP4.Size = New Size(218, 32)
        txtP4.TabIndex = 4
        lblE4.Dock = DockStyle.Fill
        lblE4.Location = New Point(400, 168)
        lblE4.Margin = New Padding(0)
        lblE4.Name = "lblE4"
        lblE4.Size = New Size(348, 56)
        lblE4.TabIndex = 204
        lblE4.TextAlign = ContentAlignment.MiddleLeft
        '
        ' parameter 5: label, field, explanation
        '
        lblP5.Dock = DockStyle.Fill
        lblP5.Location = New Point(0, 224)
        lblP5.Margin = New Padding(0, 0, 8, 0)
        lblP5.Name = "lblP5"
        lblP5.Size = New Size(162, 56)
        lblP5.TabIndex = 105
        lblP5.TextAlign = ContentAlignment.MiddleLeft
        txtP5.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtP5.Location = New Point(170, 236)
        txtP5.Margin = New Padding(0, 0, 12, 0)
        txtP5.Name = "txtP5"
        txtP5.Size = New Size(218, 32)
        txtP5.TabIndex = 5
        lblE5.Dock = DockStyle.Fill
        lblE5.Location = New Point(400, 224)
        lblE5.Margin = New Padding(0)
        lblE5.Name = "lblE5"
        lblE5.Size = New Size(348, 56)
        lblE5.TabIndex = 205
        lblE5.TextAlign = ContentAlignment.MiddleLeft
        '
        ' parameter 6: label, field, explanation
        '
        lblP6.Dock = DockStyle.Fill
        lblP6.Location = New Point(0, 280)
        lblP6.Margin = New Padding(0, 0, 8, 0)
        lblP6.Name = "lblP6"
        lblP6.Size = New Size(162, 56)
        lblP6.TabIndex = 106
        lblP6.TextAlign = ContentAlignment.MiddleLeft
        txtP6.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtP6.Location = New Point(170, 292)
        txtP6.Margin = New Padding(0, 0, 12, 0)
        txtP6.Name = "txtP6"
        txtP6.Size = New Size(218, 32)
        txtP6.TabIndex = 6
        lblE6.Dock = DockStyle.Fill
        lblE6.Location = New Point(400, 280)
        lblE6.Margin = New Padding(0)
        lblE6.Name = "lblE6"
        lblE6.Size = New Size(348, 56)
        lblE6.TabIndex = 206
        lblE6.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyButtons
        '
        tlyButtons.AutoFitToTheme = False
        tlyButtons.ColumnCount = 3
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlyButtons.Controls.Add(btnCancel, 0, 0)
        tlyButtons.Controls.Add(btnOk, 2, 0)
        tlyButtons.Dock = DockStyle.Fill
        tlyButtons.Location = New Point(16, 464)
        tlyButtons.Margin = New Padding(0)
        tlyButtons.Name = "tlyButtons"
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButtons.Size = New Size(748, 52)
        tlyButtons.TabIndex = 3
        '
        ' btnCancel
        '
        tips.SetToolTipHeader(btnCancel, "Renunță")
        tips.SetToolTipText(btnCancel, "Închide fereastra: interogarea nu rulează.")
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Dock = DockStyle.Fill
        btnCancel.Location = New Point(0, 0)
        btnCancel.Margin = New Padding(0, 6, 6, 6)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(114, 40)
        btnCancel.TabIndex = 0
        btnCancel.Text = "Renunță"
        '
        ' btnOk
        '
        tips.SetToolTipHeader(btnOk, "Rulează")
        tips.SetToolTipText(btnOk, "Verifică valorile scrise și rulează interogarea pe baza aleasă. Interogarea doar citește; rezultatul apare în grila din dreapta.")
        btnOk.Dock = DockStyle.Fill
        btnOk.Location = New Point(608, 0)
        btnOk.Margin = New Padding(6, 6, 0, 6)
        btnOk.Name = "btnOk"
        btnOk.Primary = True
        btnOk.Size = New Size(140, 40)
        btnOk.TabIndex = 1
        btnOk.Text = "Rulează"
        '
        ' QueryParamsForm
        '
        AcceptButton = btnOk
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnCancel
        ClientSize = New Size(780, 528)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "QueryParamsForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Interogare"
        tlyMain.ResumeLayout(False)
        tlyParams.ResumeLayout(False)
        tlyButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents lblDescription As Label
    Friend WithEvents lblDatabase As Label
    Friend WithEvents tlyParams As KBotTableLayoutPanel
    Friend WithEvents lblP1 As Label
    Friend WithEvents txtP1 As KBotTextBox
    Friend WithEvents lblE1 As Label
    Friend WithEvents lblP2 As Label
    Friend WithEvents txtP2 As KBotTextBox
    Friend WithEvents lblE2 As Label
    Friend WithEvents lblP3 As Label
    Friend WithEvents txtP3 As KBotTextBox
    Friend WithEvents lblE3 As Label
    Friend WithEvents lblP4 As Label
    Friend WithEvents txtP4 As KBotTextBox
    Friend WithEvents lblE4 As Label
    Friend WithEvents lblP5 As Label
    Friend WithEvents txtP5 As KBotTextBox
    Friend WithEvents lblE5 As Label
    Friend WithEvents lblP6 As Label
    Friend WithEvents txtP6 As KBotTextBox
    Friend WithEvents lblE6 As Label
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents btnCancel As KBotButton
    Friend WithEvents btnOk As KBotButton
End Class
