Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VanzareAtasamentePage
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
        components = New System.ComponentModel.Container()
        tips = New KBotToolTip(components)
        tlyAtasamente = New KBotTableLayoutPanel()
        chkAtasament = New System.Windows.Forms.CheckBox()
        lblAtasHint = New System.Windows.Forms.Label()
        tlyAtasamente.SuspendLayout()
        SuspendLayout()
        '
        ' tlyAtasamente
        '
        tlyAtasamente.ColumnCount = 1
        tlyAtasamente.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyAtasamente.Controls.Add(chkAtasament, 0, 0)
        tlyAtasamente.Controls.Add(lblAtasHint, 0, 1)
        tlyAtasamente.Dock = System.Windows.Forms.DockStyle.Fill
        tlyAtasamente.Location = New System.Drawing.Point(3, 3)
        tlyAtasamente.Margin = New System.Windows.Forms.Padding(0)
        tlyAtasamente.Name = "tlyAtasamente"
        tlyAtasamente.Padding = New System.Windows.Forms.Padding(10, 12, 10, 8)
        tlyAtasamente.RowCount = 3
        tlyAtasamente.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        tlyAtasamente.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60.0!))
        tlyAtasamente.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyAtasamente.Size = New System.Drawing.Size(743, 556)
        tlyAtasamente.TabIndex = 0
        '
        ' chkAtasament
        '
        chkAtasament.AutoSize = True
        chkAtasament.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        chkAtasament.Margin = New System.Windows.Forms.Padding(0)
        chkAtasament.Name = "chkAtasament"
        chkAtasament.TabIndex = 0
        chkAtasament.Text = "Atașează factura originală"
        tips.SetToolTipText(chkAtasament, "Alegerea se păstrează pe factură.")
        '
        ' lblAtasHint
        '
        lblAtasHint.Dock = System.Windows.Forms.DockStyle.Fill
        lblAtasHint.Margin = New System.Windows.Forms.Padding(0)
        lblAtasHint.Name = "lblAtasHint"
        lblAtasHint.TabIndex = 1
        lblAtasHint.Text = "Alegerea se păstrează pe factură. Se modifică doar cât timp factura este o ciornă (netrimisă)."
        lblAtasHint.TextAlign = System.Drawing.ContentAlignment.TopLeft
        '
        ' VanzareAtasamentePage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyAtasamente)
        Name = "VanzareAtasamentePage"
        Padding = New System.Windows.Forms.Padding(3)
        Size = New System.Drawing.Size(757, 526)
        tlyAtasamente.ResumeLayout(False)
        tlyAtasamente.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyAtasamente As KBotTableLayoutPanel
    Friend WithEvents chkAtasament As System.Windows.Forms.CheckBox
    Friend WithEvents lblAtasHint As System.Windows.Forms.Label
End Class
