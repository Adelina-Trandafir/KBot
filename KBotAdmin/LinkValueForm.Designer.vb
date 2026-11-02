' The value picker of a linked-table jump on a composite key (page «Tabele», ME01-08): a combo the operator types in (the values come from
' the server after the first character), limited to the list. All controls are declared HERE; coordinates are in the 96 dpi the form was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LinkValueForm
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
        lblPrompt = New Label()
        cmbValue = New KBotComboBox()
        lblHint = New Label()
        tlyButtons = New KBotTableLayoutPanel()
        btnCancel = New KBotButton()
        btnOk = New KBotButton()
        tlyMain.SuspendLayout()
        tlyButtons.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(lblPrompt, 0, 0)
        tlyMain.Controls.Add(cmbValue, 0, 1)
        tlyMain.Controls.Add(lblHint, 0, 2)
        tlyMain.Controls.Add(tlyButtons, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(16, 16, 16, 12)
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(520, 220)
        tlyMain.TabIndex = 0
        '
        ' lblPrompt
        '
        lblPrompt.Dock = DockStyle.Fill
        lblPrompt.Location = New Point(16, 16)
        lblPrompt.Margin = New Padding(0)
        lblPrompt.Name = "lblPrompt"
        lblPrompt.Size = New Size(488, 52)
        lblPrompt.TabIndex = 0
        lblPrompt.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbValue: typed in, the matching values come from the server after the first character; only a value of the list is accepted
        '
        cmbValue.Dock = DockStyle.Fill
        cmbValue.Editable = True
        cmbValue.FindAfterNChars = 1
        cmbValue.FindAsYouType = True
        cmbValue.LimitToList = True
        cmbValue.Location = New Point(16, 68)
        cmbValue.Margin = New Padding(0, 0, 0, 4)
        cmbValue.MaxDropDownItems = 10
        cmbValue.Name = "cmbValue"
        cmbValue.Size = New Size(488, 32)
        cmbValue.TabIndex = 1
        tips.SetToolTipHeader(cmbValue, "Valoarea")
        tips.SetToolTipText(cmbValue, "Scrieți primul caracter: din tabel se aduc valorile care îl conțin (cele care încep cu el, primele)." & vbLf & "Se acceptă doar o valoare din listă; ce nu e în listă nu rămâne în câmp.")
        '
        ' lblHint
        '
        lblHint.Dock = DockStyle.Fill
        lblHint.Location = New Point(16, 104)
        lblHint.Margin = New Padding(0)
        lblHint.Name = "lblHint"
        lblHint.Size = New Size(488, 48)
        lblHint.TabIndex = 2
        lblHint.TextAlign = ContentAlignment.TopLeft
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
        tlyButtons.Location = New Point(16, 152)
        tlyButtons.Margin = New Padding(0)
        tlyButtons.Name = "tlyButtons"
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButtons.Size = New Size(488, 52)
        tlyButtons.TabIndex = 3
        '
        ' btnCancel
        '
        tips.SetToolTipHeader(btnCancel, "Renunță")
        tips.SetToolTipText(btnCancel, "Închide fereastra: tabelul legat nu se deschide.")
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
        tips.SetToolTipHeader(btnOk, "Alege")
        tips.SetToolTipText(btnOk, "Folosește valoarea scrisă (trebuie să fie una din listă) și continuă.")
        btnOk.Dock = DockStyle.Fill
        btnOk.Location = New Point(348, 0)
        btnOk.Margin = New Padding(6, 6, 0, 6)
        btnOk.Name = "btnOk"
        btnOk.Primary = True
        btnOk.Size = New Size(140, 40)
        btnOk.TabIndex = 1
        btnOk.Text = "Alege"
        '
        ' LinkValueForm
        '
        AcceptButton = btnOk
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnCancel
        ClientSize = New Size(520, 220)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "LinkValueForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Tabele legate"
        tlyMain.ResumeLayout(False)
        tlyButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents lblPrompt As Label
    Friend WithEvents cmbValue As KBotComboBox
    Friend WithEvents lblHint As Label
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents btnCancel As KBotButton
    Friend WithEvents btnOk As KBotButton
End Class
