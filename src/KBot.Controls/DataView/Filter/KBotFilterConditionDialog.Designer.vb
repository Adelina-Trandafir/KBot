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

    Friend WithEvents lblPrompt As Label
    Friend WithEvents lblOperand1 As Label
    Friend WithEvents txtOperand1 As TextBox
    Friend WithEvents lblOperand2 As Label
    Friend WithEvents txtOperand2 As TextBox
    Friend WithEvents btnOk As Button
    Friend WithEvents btnCancel As Button

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private Sub InitializeComponent()
        lblPrompt = New Label()
        lblOperand1 = New Label()
        txtOperand1 = New TextBox()
        lblOperand2 = New Label()
        txtOperand2 = New TextBox()
        btnOk = New Button()
        btnCancel = New Button()
        SuspendLayout()
        '
        ' lblPrompt — întrebarea («Rândurile în care Data FX este…»)
        '
        lblPrompt.AutoSize = False
        lblPrompt.Location = New Drawing.Point(16, 14)
        lblPrompt.Size = New Drawing.Size(316, 34)
        lblPrompt.Name = "lblPrompt"
        '
        ' lblOperand1
        '
        lblOperand1.AutoSize = True
        lblOperand1.Location = New Drawing.Point(16, 58)
        lblOperand1.Name = "lblOperand1"
        lblOperand1.Text = "Valoare:"
        '
        ' txtOperand1
        '
        txtOperand1.Location = New Drawing.Point(16, 78)
        txtOperand1.Size = New Drawing.Size(316, 23)
        txtOperand1.Name = "txtOperand1"
        '
        ' lblOperand2 — doar pentru «Între…»
        '
        lblOperand2.AutoSize = True
        lblOperand2.Location = New Drawing.Point(16, 110)
        lblOperand2.Name = "lblOperand2"
        lblOperand2.Text = "și:"
        '
        ' txtOperand2
        '
        txtOperand2.Location = New Drawing.Point(16, 130)
        txtOperand2.Size = New Drawing.Size(316, 23)
        txtOperand2.Name = "txtOperand2"
        '
        ' btnOk
        '
        btnOk.DialogResult = DialogResult.OK
        btnOk.Location = New Drawing.Point(176, 168)
        btnOk.Size = New Drawing.Size(75, 27)
        btnOk.Name = "btnOk"
        btnOk.Text = "OK"
        '
        ' btnCancel
        '
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Location = New Drawing.Point(257, 168)
        btnCancel.Size = New Drawing.Size(75, 27)
        btnCancel.Name = "btnCancel"
        btnCancel.Text = "Anulează"
        '
        ' KBotFilterConditionDialog
        '
        AcceptButton = btnOk
        CancelButton = btnCancel
        ClientSize = New Drawing.Size(348, 207)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Name = "KBotFilterConditionDialog"
        Text = "Filtru personalizat"
        Controls.Add(lblPrompt)
        Controls.Add(lblOperand1)
        Controls.Add(txtOperand1)
        Controls.Add(lblOperand2)
        Controls.Add(txtOperand2)
        Controls.Add(btnOk)
        Controls.Add(btnCancel)
        ResumeLayout(False)
        PerformLayout()
    End Sub

End Class
