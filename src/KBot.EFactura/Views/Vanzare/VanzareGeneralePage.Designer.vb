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
        components = New System.ComponentModel.Container()
        tips = New KBotToolTip(components)
        tlyGenerale = New KBotTableLayoutPanel()
        lblNumarT = New System.Windows.Forms.Label()
        lblNumar = New System.Windows.Forms.Label()
        lblDataT = New System.Windows.Forms.Label()
        dtpData = New KBotDatePicker()
        lblTipT = New System.Windows.Forms.Label()
        lblTip = New System.Windows.Forms.Label()
        lblStareT = New System.Windows.Forms.Label()
        lblStareFactura = New System.Windows.Forms.Label()
        lblComentariiT = New System.Windows.Forms.Label()
        txtComentarii = New KBotTextBox()
        lblRefT = New System.Windows.Forms.Label()
        txtRef = New KBotTextField()
        lblContPlataT = New System.Windows.Forms.Label()
        cmbContPlata = New KBotComboBox()
        lblTotalT = New System.Windows.Forms.Label()
        lblTotal = New System.Windows.Forms.Label()
        lblInfoFactura = New System.Windows.Forms.Label()
        tlyGenerale.SuspendLayout()
        SuspendLayout()
        '
        ' tlyGenerale
        '
        tlyGenerale.ColumnCount = 2
        tlyGenerale.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170.0!))
        tlyGenerale.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
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
        tlyGenerale.Location = New System.Drawing.Point(3, 3)
        tlyGenerale.Margin = New System.Windows.Forms.Padding(0)
        tlyGenerale.Name = "tlyGenerale"
        tlyGenerale.Padding = New System.Windows.Forms.Padding(10, 8, 10, 8)
        tlyGenerale.RowCount = 10
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38.0!))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100.0!))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80.0!))
        tlyGenerale.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyGenerale.Size = New System.Drawing.Size(743, 556)
        tlyGenerale.TabIndex = 0
        '
        ' lblNumarT
        '
        lblNumarT.Dock = System.Windows.Forms.DockStyle.Fill
        lblNumarT.Margin = New System.Windows.Forms.Padding(0)
        lblNumarT.Name = "lblNumarT"
        lblNumarT.TabIndex = 0
        lblNumarT.Text = "Număr factură"
        lblNumarT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' lblNumar
        '
        lblNumar.Dock = System.Windows.Forms.DockStyle.Fill
        lblNumar.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        lblNumar.Margin = New System.Windows.Forms.Padding(0)
        lblNumar.Name = "lblNumar"
        lblNumar.TabIndex = 1
        lblNumar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' lblDataT
        '
        lblDataT.Dock = System.Windows.Forms.DockStyle.Fill
        lblDataT.Margin = New System.Windows.Forms.Padding(0)
        lblDataT.Name = "lblDataT"
        lblDataT.TabIndex = 2
        lblDataT.Text = "Data facturii"
        lblDataT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' dtpData
        '
        dtpData.Dock = System.Windows.Forms.DockStyle.Left
        dtpData.Margin = New System.Windows.Forms.Padding(0, 3, 0, 3)
        dtpData.Name = "dtpData"
        dtpData.Size = New System.Drawing.Size(170, 32)
        dtpData.TabIndex = 3
        tips.SetToolTipText(dtpData, "Data facturii. Scrieți-o sau alegeți-o din calendar.")
        '
        ' lblTipT
        '
        lblTipT.Dock = System.Windows.Forms.DockStyle.Fill
        lblTipT.Margin = New System.Windows.Forms.Padding(0)
        lblTipT.Name = "lblTipT"
        lblTipT.TabIndex = 4
        lblTipT.Text = "Tip factură"
        lblTipT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' lblTip
        '
        lblTip.Dock = System.Windows.Forms.DockStyle.Fill
        lblTip.Margin = New System.Windows.Forms.Padding(0)
        lblTip.Name = "lblTip"
        lblTip.TabIndex = 5
        lblTip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' lblStareT
        '
        lblStareT.Dock = System.Windows.Forms.DockStyle.Fill
        lblStareT.Margin = New System.Windows.Forms.Padding(0)
        lblStareT.Name = "lblStareT"
        lblStareT.TabIndex = 6
        lblStareT.Text = "Stare"
        lblStareT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' lblStareFactura
        '
        lblStareFactura.Dock = System.Windows.Forms.DockStyle.Fill
        lblStareFactura.Margin = New System.Windows.Forms.Padding(0)
        lblStareFactura.Name = "lblStareFactura"
        lblStareFactura.TabIndex = 7
        lblStareFactura.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' lblComentariiT
        '
        lblComentariiT.Dock = System.Windows.Forms.DockStyle.Fill
        lblComentariiT.Margin = New System.Windows.Forms.Padding(0)
        lblComentariiT.Name = "lblComentariiT"
        lblComentariiT.TabIndex = 8
        lblComentariiT.Text = "Comentarii factură"
        lblComentariiT.TextAlign = System.Drawing.ContentAlignment.TopLeft
        lblComentariiT.Padding = New System.Windows.Forms.Padding(0, 8, 0, 0)
        '
        ' txtComentarii
        '
        txtComentarii.Dock = System.Windows.Forms.DockStyle.Fill
        txtComentarii.MaxLength = 255
        txtComentarii.Margin = New System.Windows.Forms.Padding(0, 3, 0, 3)
        txtComentarii.Name = "txtComentarii"
        txtComentarii.Size = New System.Drawing.Size(553, 94)
        txtComentarii.TabIndex = 9
        tips.SetToolTipHeader(txtComentarii, "Comentarii factură")
        tips.SetToolTipText(txtComentarii, "Text liber, cel mult 255 de caractere; se trimite la ANAF împreună cu factura.")
        '
        ' lblRefT
        '
        lblRefT.Dock = System.Windows.Forms.DockStyle.Fill
        lblRefT.Margin = New System.Windows.Forms.Padding(0)
        lblRefT.Name = "lblRefT"
        lblRefT.TabIndex = 10
        lblRefT.Text = "Ref. comandă (BT-13)"
        lblRefT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtRef
        '
        txtRef.BackColor = System.Drawing.Color.Transparent
        txtRef.Dock = System.Windows.Forms.DockStyle.Fill
        txtRef.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtRef.MaxLength = 30
        txtRef.Name = "txtRef"
        txtRef.Size = New System.Drawing.Size(553, 32)
        txtRef.TabIndex = 10
        tips.SetToolTipText(txtRef, "Numărul comenzii la care se referă factura (cel mult 30 de caractere). Poate rămâne gol.")
        '
        ' lblContPlataT
        '
        lblContPlataT.Dock = System.Windows.Forms.DockStyle.Fill
        lblContPlataT.Margin = New System.Windows.Forms.Padding(0)
        lblContPlataT.Name = "lblContPlataT"
        lblContPlataT.TabIndex = 11
        lblContPlataT.Text = "Cont emitent (IBAN) *"
        lblContPlataT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' cmbContPlata
        '
        cmbContPlata.Dock = System.Windows.Forms.DockStyle.Fill
        cmbContPlata.Editable = True
        cmbContPlata.LimitToList = False
        cmbContPlata.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        cmbContPlata.Name = "cmbContPlata"
        cmbContPlata.Size = New System.Drawing.Size(553, 32)
        cmbContPlata.TabIndex = 12
        tips.SetToolTipHeader(cmbContPlata, "Cont emitent")
        tips.SetToolTipText(cmbContPlata, "IBAN-ul unității în care se plătește factura. Alegeți unul folosit pe facturile anterioare sau scrieți altul; contul este al facturii, nu al unității.")
        '
        ' lblTotalT
        '
        lblTotalT.Dock = System.Windows.Forms.DockStyle.Fill
        lblTotalT.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        lblTotalT.Margin = New System.Windows.Forms.Padding(0)
        lblTotalT.Name = "lblTotalT"
        lblTotalT.TabIndex = 13
        lblTotalT.Text = "TOTAL FACTURĂ"
        lblTotalT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' lblTotal
        '
        lblTotal.Dock = System.Windows.Forms.DockStyle.Fill
        lblTotal.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        lblTotal.Margin = New System.Windows.Forms.Padding(0)
        lblTotal.Name = "lblTotal"
        lblTotal.TabIndex = 14
        lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' lblInfoFactura
        '
        tlyGenerale.SetColumnSpan(lblInfoFactura, 2)
        lblInfoFactura.Dock = System.Windows.Forms.DockStyle.Fill
        lblInfoFactura.Margin = New System.Windows.Forms.Padding(0)
        lblInfoFactura.Name = "lblInfoFactura"
        lblInfoFactura.TabIndex = 15
        lblInfoFactura.TextAlign = System.Drawing.ContentAlignment.TopLeft
        '
        ' VanzareGeneralePage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyGenerale)
        Name = "VanzareGeneralePage"
        Padding = New System.Windows.Forms.Padding(3)
        Size = New System.Drawing.Size(757, 526)
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
