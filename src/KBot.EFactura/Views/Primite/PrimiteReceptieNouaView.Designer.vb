<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteReceptieNouaView
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
        components = New ComponentModel.Container()
        tips = New KBot.Controls.KBotToolTip(components)
        cmbTip = New Controls.KBotComboBox()
        dtpData = New Controls.KBotDatePicker()
        txtDescriere = New Controls.KBotTextBox()
        tlyNoua = New Controls.KBotTableLayoutPanel()
        lblTip = New Controls.KBotLabel()
        lblData = New Controls.KBotLabel()
        lblDescriere = New Controls.KBotLabel()
        tlyNoua.SuspendLayout()
        SuspendLayout()
        ' 
        ' cmbTip
        ' 
        cmbTip.Dock = System.Windows.Forms.DockStyle.Fill
        cmbTip.Location = New System.Drawing.Point(0, 30)
        cmbTip.Margin = New System.Windows.Forms.Padding(0, 0, 0, 6)
        cmbTip.Name = "cmbTip"
        cmbTip.Size = New System.Drawing.Size(283, 37)
        cmbTip.TabIndex = 1
        cmbTip.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        tips.SetToolTipHeader(cmbTip, "Tipul recepției")
        tips.SetToolTipText(cmbTip, "Avans, Partial sau Final, ca în FOREXE. Implicit «Final», ca în FOREXE.")
        ' 
        ' dtpData
        ' 
        dtpData.Dock = System.Windows.Forms.DockStyle.Fill
        dtpData.Location = New System.Drawing.Point(0, 109)
        dtpData.Margin = New System.Windows.Forms.Padding(0, 0, 0, 6)
        dtpData.Name = "dtpData"
        dtpData.Size = New System.Drawing.Size(283, 48)
        dtpData.TabIndex = 3
        tips.SetToolTipHeader(dtpData, "Data recepției")
        tips.SetToolTipText(dtpData, "Data scrisă în recepția din FOREXE. Implicit, data celei mai noi facturi bifate.")
        ' 
        ' txtDescriere
        ' 
        txtDescriere.Dock = System.Windows.Forms.DockStyle.Fill
        txtDescriere.Location = New System.Drawing.Point(0, 193)
        txtDescriere.Margin = New System.Windows.Forms.Padding(0)
        txtDescriere.MaxLength = 200
        txtDescriere.Name = "txtDescriere"
        txtDescriere.Size = New System.Drawing.Size(283, 11)
        txtDescriere.TabIndex = 5
        tips.SetToolTipHeader(txtDescriere, "Descrierea recepției")
        tips.SetToolTipText(txtDescriere, "Textul scris în recepția din FOREXE, de obicei «plata factura număr/data». K-BOT adaugă la sfârșit marcajul lui.")
        ' 
        ' tlyNoua
        ' 
        tlyNoua.ColumnCount = 1
        tlyNoua.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyNoua.Controls.Add(lblTip, 0, 0)
        tlyNoua.Controls.Add(cmbTip, 0, 1)
        tlyNoua.Controls.Add(lblData, 0, 2)
        tlyNoua.Controls.Add(dtpData, 0, 3)
        tlyNoua.Controls.Add(lblDescriere, 0, 4)
        tlyNoua.Controls.Add(txtDescriere, 0, 5)
        tlyNoua.Dock = System.Windows.Forms.DockStyle.Fill
        tlyNoua.Location = New System.Drawing.Point(4, 4)
        tlyNoua.Margin = New System.Windows.Forms.Padding(0)
        tlyNoua.Name = "tlyNoua"
        tlyNoua.RowCount = 6
        tlyNoua.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F))
        tlyNoua.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F))
        tlyNoua.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F))
        tlyNoua.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F))
        tlyNoua.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F))
        tlyNoua.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyNoua.Size = New System.Drawing.Size(283, 204)
        tlyNoua.TabIndex = 0
        ' 
        ' lblTip
        ' 
        lblTip.BorderWidth = 0
        lblTip.Dock = System.Windows.Forms.DockStyle.Fill
        lblTip.Location = New System.Drawing.Point(0, 0)
        lblTip.Margin = New System.Windows.Forms.Padding(0)
        lblTip.Name = "lblTip"
        lblTip.Size = New System.Drawing.Size(283, 30)
        lblTip.TabIndex = 0
        lblTip.Text = "Tip:"
        lblTip.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblData
        ' 
        lblData.BorderWidth = 0
        lblData.Dock = System.Windows.Forms.DockStyle.Fill
        lblData.Location = New System.Drawing.Point(0, 79)
        lblData.Margin = New System.Windows.Forms.Padding(0)
        lblData.Name = "lblData"
        lblData.Size = New System.Drawing.Size(283, 30)
        lblData.TabIndex = 2
        lblData.Text = "Data:"
        lblData.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblDescriere
        ' 
        lblDescriere.BorderWidth = 0
        lblDescriere.Dock = System.Windows.Forms.DockStyle.Fill
        lblDescriere.Location = New System.Drawing.Point(0, 163)
        lblDescriere.Margin = New System.Windows.Forms.Padding(0)
        lblDescriere.Name = "lblDescriere"
        lblDescriere.Size = New System.Drawing.Size(283, 30)
        lblDescriere.TabIndex = 4
        lblDescriere.Text = "Descriere:"
        lblDescriere.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' PrimiteReceptieNouaView
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyNoua)
        Margin = New System.Windows.Forms.Padding(0)
        Name = "PrimiteReceptieNouaView"
        Padding = New System.Windows.Forms.Padding(4)
        Size = New System.Drawing.Size(291, 212)
        tlyNoua.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents tlyNoua As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblTip As Global.KBot.Controls.KBotLabel
    Friend WithEvents cmbTip As Global.KBot.Controls.KBotComboBox
    Friend WithEvents lblData As Global.KBot.Controls.KBotLabel
    Friend WithEvents dtpData As Global.KBot.Controls.KBotDatePicker
    Friend WithEvents lblDescriere As Global.KBot.Controls.KBotLabel
    Friend WithEvents txtDescriere As Global.KBot.Controls.KBotTextBox
End Class
