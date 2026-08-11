Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Partea „designer” a <see cref="KBotFilterConditionDialog"/>. Conform regulii casei, TOATE
''' controalele copil se declară aici, nu se construiesc în cod la nevoie.
'''
''' Cele două casete de operand sunt declarate AMÂNDOUĂ, mereu: a doua se ascunde pentru condițiile
''' cu un singur operand. Un control creat la nevoie n-ar exista pe suprafața de proiectare, iar
''' rostul acestui fișier e tocmai ca formularul să se poată deschide și citi acolo.
''' </summary>
Partial Class KBotFilterConditionDialog
    Inherits KBot.Theming.KBotThemedForm

    Private components As System.ComponentModel.IContainer

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(KBotFilterConditionDialog))
        tlyMAIN = New TableLayoutPanel()
        btnCancel = New Button()
        btnOk = New Button()
        txtOperand2 = New TextBox()
        lblOperand2 = New Label()
        txtOperand1 = New TextBox()
        lblOperand1 = New Label()
        lblPrompt = New Label()
        tlyMAIN.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlyMAIN
        ' 
        tlyMAIN.ColumnCount = 2
        tlyMAIN.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyMAIN.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyMAIN.Controls.Add(btnCancel, 0, 5)
        tlyMAIN.Controls.Add(btnOk, 1, 5)
        tlyMAIN.Controls.Add(txtOperand2, 0, 4)
        tlyMAIN.Controls.Add(lblOperand2, 0, 3)
        tlyMAIN.Controls.Add(txtOperand1, 0, 2)
        tlyMAIN.Controls.Add(lblOperand1, 0, 1)
        tlyMAIN.Controls.Add(lblPrompt, 0, 0)
        tlyMAIN.Dock = DockStyle.Fill
        tlyMAIN.Location = New Point(0, 0)
        tlyMAIN.Name = "tlyMAIN"
        tlyMAIN.RowCount = 6
        tlyMAIN.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMAIN.RowStyles.Add(New RowStyle(SizeType.Absolute, 32F))
        tlyMAIN.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMAIN.RowStyles.Add(New RowStyle(SizeType.Absolute, 32F))
        tlyMAIN.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMAIN.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyMAIN.Size = New Size(338, 335)
        tlyMAIN.TabIndex = 7
        ' 
        ' btnCancel
        ' 
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Dock = DockStyle.Fill
        btnCancel.Location = New Point(3, 282)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(163, 50)
        btnCancel.TabIndex = 7
        btnCancel.Text = "Anulează"
        ' 
        ' btnOk
        ' 
        btnOk.DialogResult = DialogResult.OK
        btnOk.Dock = DockStyle.Fill
        btnOk.Location = New Point(172, 282)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(163, 50)
        btnOk.TabIndex = 6
        btnOk.Text = "OK"
        ' 
        ' txtOperand2
        ' 
        tlyMAIN.SetColumnSpan(txtOperand2, 2)
        txtOperand2.Dock = DockStyle.Fill
        txtOperand2.Location = New Point(3, 242)
        txtOperand2.Name = "txtOperand2"
        txtOperand2.Size = New Size(332, 31)
        txtOperand2.TabIndex = 5
        ' 
        ' lblOperand2
        ' 
        lblOperand2.AutoSize = True
        tlyMAIN.SetColumnSpan(lblOperand2, 2)
        lblOperand2.Dock = DockStyle.Fill
        lblOperand2.Location = New Point(3, 207)
        lblOperand2.Name = "lblOperand2"
        lblOperand2.Size = New Size(332, 32)
        lblOperand2.TabIndex = 4
        lblOperand2.Text = "și:"
        ' 
        ' txtOperand1
        ' 
        tlyMAIN.SetColumnSpan(txtOperand1, 2)
        txtOperand1.Dock = DockStyle.Fill
        txtOperand1.Location = New Point(3, 170)
        txtOperand1.Name = "txtOperand1"
        txtOperand1.Size = New Size(332, 31)
        txtOperand1.TabIndex = 3
        ' 
        ' lblOperand1
        ' 
        lblOperand1.AutoSize = True
        tlyMAIN.SetColumnSpan(lblOperand1, 2)
        lblOperand1.Dock = DockStyle.Fill
        lblOperand1.Location = New Point(3, 135)
        lblOperand1.Name = "lblOperand1"
        lblOperand1.Size = New Size(332, 32)
        lblOperand1.TabIndex = 2
        lblOperand1.Text = "Valoare:"
        ' 
        ' lblPrompt
        ' 
        tlyMAIN.SetColumnSpan(lblPrompt, 2)
        lblPrompt.Dock = DockStyle.Fill
        lblPrompt.Location = New Point(3, 0)
        lblPrompt.Name = "lblPrompt"
        lblPrompt.Size = New Size(332, 135)
        lblPrompt.TabIndex = 1
        ' 
        ' KBotFilterConditionDialog
        ' 
        ClientSize = New Size(338, 335)
        Controls.Add(tlyMAIN)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        MinimizeBox = False
        Name = "KBotFilterConditionDialog"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Show
        StartPosition = FormStartPosition.CenterParent
        Text = "Filtru personalizat"
        TopMost = True
        tlyMAIN.ResumeLayout(False)
        tlyMAIN.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tlyMAIN As TableLayoutPanel
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnOk As Button
    Friend WithEvents txtOperand2 As TextBox
    Friend WithEvents lblOperand2 As Label
    Friend WithEvents txtOperand1 As TextBox
    Friend WithEvents lblOperand1 As Label
    Friend WithEvents lblPrompt As Label

End Class
