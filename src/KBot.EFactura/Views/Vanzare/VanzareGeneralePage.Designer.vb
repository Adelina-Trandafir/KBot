Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VanzareGeneralePage
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
        tips = New KBotToolTip(components)
        dtpData = New KBotDatePicker()
        txtComentarii = New KBotTextBox()
        txtRef = New KBotTextField()
        cmbContPlata = New KBotComboBox()
        tlyGenerale = New KBotTableLayoutPanel()
        lblNumarT = New System.Windows.Forms.Label()
        lblNumar = New System.Windows.Forms.Label()
        lblDataT = New System.Windows.Forms.Label()
        lblTipT = New System.Windows.Forms.Label()
        lblTip = New System.Windows.Forms.Label()
        lblStareT = New System.Windows.Forms.Label()
        lblStareFactura = New System.Windows.Forms.Label()
        lblComentariiT = New System.Windows.Forms.Label()
        lblRefT = New System.Windows.Forms.Label()
        lblContPlataT = New System.Windows.Forms.Label()
        lblTotalT = New System.Windows.Forms.Label()
        lblTotal = New System.Windows.Forms.Label()
        lblInfoFactura = New System.Windows.Forms.Label()
        tlyGenerale.SuspendLayout()
        SuspendLayout()
        ' 
        ' dtpData
        ' 
        dtpData.Dock = System.Windows.Forms.DockStyle.Left
        dtpData.Location = New System.Drawing.Point(265, 63)
        dtpData.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        dtpData.Name = "dtpData"
        dtpData.Size = New System.Drawing.Size(255, 49)
        dtpData.TabIndex = 3
        tips.SetToolTipText(dtpData, "Data facturii. Scrieți-o sau alegeți-o din calendar.")
        ' 
        ' txtComentarii
        ' 
        txtComentarii.Dock = System.Windows.Forms.DockStyle.Fill
        txtComentarii.Location = New System.Drawing.Point(265, 222)
        txtComentarii.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtComentarii.MaxLength = 255
        txtComentarii.Name = "txtComentarii"
        txtComentarii.Size = New System.Drawing.Size(813, 142)
        txtComentarii.TabIndex = 9
        tips.SetToolTipHeader(txtComentarii, "Comentarii factură")
        tips.SetToolTipText(txtComentarii, "Text liber, cel mult 255 de caractere; se trimite la ANAF împreună cu factura.")
        ' 
        ' txtRef
        ' 
        txtRef.BackColor = Drawing.Color.Transparent
        txtRef.Dock = System.Windows.Forms.DockStyle.Fill
        txtRef.Location = New System.Drawing.Point(265, 374)
        txtRef.Margin = New System.Windows.Forms.Padding(0, 6, 0, 6)
        txtRef.MaxLength = 30
        txtRef.Name = "txtRef"
        txtRef.Size = New System.Drawing.Size(813, 48)
        txtRef.TabIndex = 10
        tips.SetToolTipText(txtRef, "Numărul comenzii la care se referă factura (cel mult 30 de caractere). Poate rămâne gol.")
        ' 
        ' cmbContPlata
        ' 
        cmbContPlata.Dock = System.Windows.Forms.DockStyle.Fill
        cmbContPlata.Editable = True
        cmbContPlata.LimitToList = False
        cmbContPlata.Location = New System.Drawing.Point(265, 434)
        cmbContPlata.Margin = New System.Windows.Forms.Padding(0, 6, 0, 6)
        cmbContPlata.Name = "cmbContPlata"
        cmbContPlata.Size = New System.Drawing.Size(813, 37)
        cmbContPlata.TabIndex = 12
        tips.SetToolTipHeader(cmbContPlata, "Cont emitent")
        tips.SetToolTipText(cmbContPlata, "IBAN-ul unității în care se plătește factura. Alegeți unul folosit pe facturile anterioare sau scrieți altul; contul este al facturii, nu al unității.")
        ' 
        ' tlyGenerale
        ' 
        tlyGenerale.ColumnCount = 2
        tlyGenerale.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 255F))
        tlyGenerale.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyGenerale.Controls.Add(lblNumarT, 0, 0)
        tlyGenerale.Controls.Add(lblNumar, 1, 0)
        tlyGenerale.Controls.Add(lblDataT, 0, 1)
        tlyGenerale.Controls.Add(dtpData, 1, 1)
        tlyGenerale.Controls.Add(lblTipT, 0, 2)
        tlyGenerale.Controls.Add(lblTip, 1, 2)
        tlyGenerale.Controls.Add(lblStareT, 0, 3)
        tlyGenerale.Controls.Add(lblStareFactura, 1, 3)
        tlyGenerale.Controls.Add(lblComentariiT, 0, 4)
        tlyGenerale.Controls.Add(txtComentarii, 1, 4)
        tlyGenerale.Controls.Add(lblRefT, 0, 5)
        tlyGenerale.Controls.Add(txtRef, 1, 5)
        tlyGenerale.Controls.Add(lblContPlataT, 0, 6)
        tlyGenerale.Controls.Add(cmbContPlata, 1, 6)
        tlyGenerale.Controls.Add(lblTotalT, 0, 7)
        tlyGenerale.Controls.Add(lblTotal, 1, 7)
        tlyGenerale.Controls.Add(lblInfoFactura, 0, 8)
        tlyGenerale.Dock = System.Windows.Forms.DockStyle.Fill
        tlyGenerale.Location = New System.Drawing.Point(4, 4)
        tlyGenerale.Margin = New System.Windows.Forms.Padding(0)
        tlyGenerale.Name = "tlyGenerale"
        tlyGenerale.Padding = New System.Windows.Forms.Padding(10, 8, 10, 8)
        tlyGenerale.RowCount = 10
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyGenerale.Size = New System.Drawing.Size(1088, 670)
        tlyGenerale.TabIndex = 0
        ' 
        ' lblNumarT
        ' 
        lblNumarT.Dock = System.Windows.Forms.DockStyle.Fill
        lblNumarT.Location = New System.Drawing.Point(10, 8)
        lblNumarT.Margin = New System.Windows.Forms.Padding(0)
        lblNumarT.Name = "lblNumarT"
        lblNumarT.Size = New System.Drawing.Size(255, 51)
        lblNumarT.TabIndex = 0
        lblNumarT.Text = "Număr factură"
        lblNumarT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblNumar
        ' 
        lblNumar.Dock = System.Windows.Forms.DockStyle.Fill
        lblNumar.Font = New System.Drawing.Font("Segoe UI", 10F, Drawing.FontStyle.Bold)
        lblNumar.Location = New System.Drawing.Point(265, 8)
        lblNumar.Margin = New System.Windows.Forms.Padding(0)
        lblNumar.Name = "lblNumar"
        lblNumar.Size = New System.Drawing.Size(813, 51)
        lblNumar.TabIndex = 1
        lblNumar.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblDataT
        ' 
        lblDataT.Dock = System.Windows.Forms.DockStyle.Fill
        lblDataT.Location = New System.Drawing.Point(10, 59)
        lblDataT.Margin = New System.Windows.Forms.Padding(0)
        lblDataT.Name = "lblDataT"
        lblDataT.Size = New System.Drawing.Size(255, 57)
        lblDataT.TabIndex = 2
        lblDataT.Text = "Data facturii"
        lblDataT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblTipT
        ' 
        lblTipT.Dock = System.Windows.Forms.DockStyle.Fill
        lblTipT.Location = New System.Drawing.Point(10, 116)
        lblTipT.Margin = New System.Windows.Forms.Padding(0)
        lblTipT.Name = "lblTipT"
        lblTipT.Size = New System.Drawing.Size(255, 51)
        lblTipT.TabIndex = 4
        lblTipT.Text = "Tip factură"
        lblTipT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblTip
        ' 
        lblTip.Dock = System.Windows.Forms.DockStyle.Fill
        lblTip.Location = New System.Drawing.Point(265, 116)
        lblTip.Margin = New System.Windows.Forms.Padding(0)
        lblTip.Name = "lblTip"
        lblTip.Size = New System.Drawing.Size(813, 51)
        lblTip.TabIndex = 5
        lblTip.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblStareT
        ' 
        lblStareT.Dock = System.Windows.Forms.DockStyle.Fill
        lblStareT.Location = New System.Drawing.Point(10, 167)
        lblStareT.Margin = New System.Windows.Forms.Padding(0)
        lblStareT.Name = "lblStareT"
        lblStareT.Size = New System.Drawing.Size(255, 51)
        lblStareT.TabIndex = 6
        lblStareT.Text = "Stare"
        lblStareT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblStareFactura
        ' 
        lblStareFactura.Dock = System.Windows.Forms.DockStyle.Fill
        lblStareFactura.Location = New System.Drawing.Point(265, 167)
        lblStareFactura.Margin = New System.Windows.Forms.Padding(0)
        lblStareFactura.Name = "lblStareFactura"
        lblStareFactura.Size = New System.Drawing.Size(813, 51)
        lblStareFactura.TabIndex = 7
        lblStareFactura.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblComentariiT
        ' 
        lblComentariiT.Dock = System.Windows.Forms.DockStyle.Fill
        lblComentariiT.Location = New System.Drawing.Point(10, 218)
        lblComentariiT.Margin = New System.Windows.Forms.Padding(0)
        lblComentariiT.Name = "lblComentariiT"
        lblComentariiT.Padding = New System.Windows.Forms.Padding(0, 12, 0, 0)
        lblComentariiT.Size = New System.Drawing.Size(255, 150)
        lblComentariiT.TabIndex = 8
        lblComentariiT.Text = "Comentarii factură"
        ' 
        ' lblRefT
        ' 
        lblRefT.Dock = System.Windows.Forms.DockStyle.Fill
        lblRefT.Location = New System.Drawing.Point(10, 368)
        lblRefT.Margin = New System.Windows.Forms.Padding(0)
        lblRefT.Name = "lblRefT"
        lblRefT.Size = New System.Drawing.Size(255, 60)
        lblRefT.TabIndex = 10
        lblRefT.Text = "Ref. comandă (BT-13)"
        lblRefT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblContPlataT
        ' 
        lblContPlataT.Dock = System.Windows.Forms.DockStyle.Fill
        lblContPlataT.Location = New System.Drawing.Point(10, 428)
        lblContPlataT.Margin = New System.Windows.Forms.Padding(0)
        lblContPlataT.Name = "lblContPlataT"
        lblContPlataT.Size = New System.Drawing.Size(255, 60)
        lblContPlataT.TabIndex = 11
        lblContPlataT.Text = "Cont emitent (IBAN) *"
        lblContPlataT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblTotalT
        ' 
        lblTotalT.Dock = System.Windows.Forms.DockStyle.Fill
        lblTotalT.Font = New System.Drawing.Font("Segoe UI", 10F, Drawing.FontStyle.Bold)
        lblTotalT.Location = New System.Drawing.Point(10, 488)
        lblTotalT.Margin = New System.Windows.Forms.Padding(0)
        lblTotalT.Name = "lblTotalT"
        lblTotalT.Size = New System.Drawing.Size(255, 60)
        lblTotalT.TabIndex = 13
        lblTotalT.Text = "TOTAL FACTURĂ"
        lblTotalT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblTotal
        ' 
        lblTotal.Dock = System.Windows.Forms.DockStyle.Fill
        lblTotal.Font = New System.Drawing.Font("Segoe UI", 12F, Drawing.FontStyle.Bold)
        lblTotal.Location = New System.Drawing.Point(265, 488)
        lblTotal.Margin = New System.Windows.Forms.Padding(0)
        lblTotal.Name = "lblTotal"
        lblTotal.Size = New System.Drawing.Size(813, 60)
        lblTotal.TabIndex = 14
        lblTotal.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblInfoFactura
        ' 
        tlyGenerale.SetColumnSpan(lblInfoFactura, 2)
        lblInfoFactura.Dock = System.Windows.Forms.DockStyle.Fill
        lblInfoFactura.Location = New System.Drawing.Point(10, 548)
        lblInfoFactura.Margin = New System.Windows.Forms.Padding(0)
        lblInfoFactura.Name = "lblInfoFactura"
        lblInfoFactura.Size = New System.Drawing.Size(1068, 120)
        lblInfoFactura.TabIndex = 15
        ' 
        ' VanzareGeneralePage
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyGenerale)
        Margin = New System.Windows.Forms.Padding(4)
        Name = "VanzareGeneralePage"
        Padding = New System.Windows.Forms.Padding(4)
        Size = New System.Drawing.Size(1096, 678)
        tlyGenerale.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyGenerale As KBotTableLayoutPanel
    Friend WithEvents lblNumarT As System.Windows.Forms.Label
    Friend WithEvents lblNumar As System.Windows.Forms.Label
    Friend WithEvents lblDataT As System.Windows.Forms.Label
    Friend WithEvents dtpData As KBotDatePicker
    Friend WithEvents lblTipT As System.Windows.Forms.Label
    Friend WithEvents lblTip As System.Windows.Forms.Label
    Friend WithEvents lblStareT As System.Windows.Forms.Label
    Friend WithEvents lblStareFactura As System.Windows.Forms.Label
    Friend WithEvents lblComentariiT As System.Windows.Forms.Label
    Friend WithEvents txtComentarii As KBotTextBox
    Friend WithEvents lblRefT As System.Windows.Forms.Label
    Friend WithEvents txtRef As KBotTextField
    Friend WithEvents lblContPlataT As System.Windows.Forms.Label
    Friend WithEvents cmbContPlata As KBotComboBox
    Friend WithEvents lblTotalT As System.Windows.Forms.Label
    Friend WithEvents lblTotal As System.Windows.Forms.Label
    Friend WithEvents lblInfoFactura As System.Windows.Forms.Label
End Class
