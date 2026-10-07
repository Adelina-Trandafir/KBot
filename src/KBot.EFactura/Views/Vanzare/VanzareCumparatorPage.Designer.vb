Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VanzareCumparatorPage
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
        tlyClient = New KBotTableLayoutPanel()
        lblClientT = New System.Windows.Forms.Label()
        cmbClient = New KBotComboBox()
        lblCnpT = New System.Windows.Forms.Label()
        chkCnp = New System.Windows.Forms.CheckBox()
        lblIndT = New System.Windows.Forms.Label()
        txtClientInd = New KBotTextField()
        lblCfT = New System.Windows.Forms.Label()
        txtClientCf = New KBotTextField()
        lblDenClientT = New System.Windows.Forms.Label()
        txtClientDen = New KBotTextField()
        lblJudClientT = New System.Windows.Forms.Label()
        cmbClientJud = New KBotComboBox()
        lblOrasClientT = New System.Windows.Forms.Label()
        txtClientOras = New KBotTextField()
        lblSectorT = New System.Windows.Forms.Label()
        cmbClientSector = New KBotComboBox()
        lblAdresaClientT = New System.Windows.Forms.Label()
        txtClientAdresa = New KBotTextField()
        lblContClientT = New System.Windows.Forms.Label()
        txtClientCont = New KBotTextField()
        lblBancaClientT = New System.Windows.Forms.Label()
        txtClientBanca = New KBotTextField()
        flowClient = New System.Windows.Forms.FlowLayoutPanel()
        btnClientNou = New System.Windows.Forms.Button()
        btnClientSalveaza = New System.Windows.Forms.Button()
        btnClientSterge = New System.Windows.Forms.Button()
        tlyClient.SuspendLayout()
        flowClient.SuspendLayout()
        SuspendLayout()
        '
        ' tlyClient
        '
        tlyClient.ColumnCount = 2
        tlyClient.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170.0!))
        tlyClient.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyClient.Controls.Add(lblClientT, 0, 0)
        tlyClient.Controls.Add(cmbClient, 1, 0)
        tlyClient.Controls.Add(lblCnpT, 0, 1)
        tlyClient.Controls.Add(chkCnp, 1, 1)
        tlyClient.Controls.Add(lblIndT, 0, 2)
        tlyClient.Controls.Add(txtClientInd, 1, 2)
        tlyClient.Controls.Add(lblCfT, 0, 3)
        tlyClient.Controls.Add(txtClientCf, 1, 3)
        tlyClient.Controls.Add(lblDenClientT, 0, 4)
        tlyClient.Controls.Add(txtClientDen, 1, 4)
        tlyClient.Controls.Add(lblJudClientT, 0, 5)
        tlyClient.Controls.Add(cmbClientJud, 1, 5)
        tlyClient.Controls.Add(lblOrasClientT, 0, 6)
        tlyClient.Controls.Add(txtClientOras, 1, 6)
        tlyClient.Controls.Add(lblSectorT, 0, 7)
        tlyClient.Controls.Add(cmbClientSector, 1, 7)
        tlyClient.Controls.Add(lblAdresaClientT, 0, 8)
        tlyClient.Controls.Add(txtClientAdresa, 1, 8)
        tlyClient.Controls.Add(lblContClientT, 0, 9)
        tlyClient.Controls.Add(txtClientCont, 1, 9)
        tlyClient.Controls.Add(lblBancaClientT, 0, 10)
        tlyClient.Controls.Add(txtClientBanca, 1, 10)
        tlyClient.Controls.Add(flowClient, 0, 11)
        tlyClient.Dock = System.Windows.Forms.DockStyle.Fill
        tlyClient.Location = New System.Drawing.Point(3, 3)
        tlyClient.Margin = New System.Windows.Forms.Padding(0)
        tlyClient.Name = "tlyClient"
        tlyClient.Padding = New System.Windows.Forms.Padding(10, 8, 10, 8)
        tlyClient.RowCount = 13
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyClient.Size = New System.Drawing.Size(743, 556)
        tlyClient.TabIndex = 0
        '
        ' lblClientT
        '
        lblClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblClientT.Margin = New System.Windows.Forms.Padding(0)
        lblClientT.Name = "lblClientT"
        lblClientT.TabIndex = 0
        lblClientT.Text = "Client *"
        lblClientT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' cmbClient
        '
        cmbClient.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        cmbClient.Editable = True
        cmbClient.FindAsYouType = True
        cmbClient.Margin = New System.Windows.Forms.Padding(0, 3, 0, 3)
        cmbClient.Name = "cmbClient"
        cmbClient.Size = New System.Drawing.Size(553, 28)
        cmbClient.TabIndex = 1
        tips.SetToolTipHeader(cmbClient, "Clientul facturii")
        tips.SetToolTipText(cmbClient, "Alegeți un client existent (tastați o parte din denumire sau din codul fiscal) sau apăsați «Client nou».")
        '
        ' lblCnpT
        '
        lblCnpT.Dock = System.Windows.Forms.DockStyle.Fill
        lblCnpT.Margin = New System.Windows.Forms.Padding(0)
        lblCnpT.Name = "lblCnpT"
        lblCnpT.TabIndex = 2
        lblCnpT.Text = "Persoană fizică"
        lblCnpT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' chkCnp
        '
        chkCnp.AutoSize = True
        chkCnp.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        chkCnp.Margin = New System.Windows.Forms.Padding(0, 6, 0, 0)
        chkCnp.Name = "chkCnp"
        chkCnp.TabIndex = 3
        chkCnp.Text = "Codul de mai jos este CNP"
        tips.SetToolTipText(chkCnp, "Bifați când clientul este o persoană fizică și în câmpul «Cod fiscal» este CNP-ul ei.")
        '
        ' lblIndT
        '
        lblIndT.Dock = System.Windows.Forms.DockStyle.Fill
        lblIndT.Margin = New System.Windows.Forms.Padding(0)
        lblIndT.Name = "lblIndT"
        lblIndT.TabIndex = 4
        lblIndT.Text = "Prefix fiscal (RO)"
        lblIndT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtClientInd
        '
        txtClientInd.BackColor = System.Drawing.Color.Transparent
        txtClientInd.Dock = System.Windows.Forms.DockStyle.Left
        txtClientInd.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtClientInd.MaxLength = 8
        txtClientInd.Name = "txtClientInd"
        txtClientInd.Size = New System.Drawing.Size(110, 32)
        txtClientInd.TabIndex = 5
        tips.SetToolTipText(txtClientInd, "«RO» când clientul este plătitor de TVA; altfel gol.")
        '
        ' lblCfT
        '
        lblCfT.Dock = System.Windows.Forms.DockStyle.Fill
        lblCfT.Margin = New System.Windows.Forms.Padding(0)
        lblCfT.Name = "lblCfT"
        lblCfT.TabIndex = 6
        lblCfT.Text = "Cod fiscal"
        lblCfT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtClientCf
        '
        txtClientCf.BackColor = System.Drawing.Color.Transparent
        txtClientCf.Dock = System.Windows.Forms.DockStyle.Fill
        txtClientCf.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtClientCf.MaxLength = 32
        txtClientCf.Name = "txtClientCf"
        txtClientCf.Size = New System.Drawing.Size(553, 32)
        txtClientCf.TabIndex = 7
        '
        ' lblDenClientT
        '
        lblDenClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblDenClientT.Margin = New System.Windows.Forms.Padding(0)
        lblDenClientT.Name = "lblDenClientT"
        lblDenClientT.TabIndex = 8
        lblDenClientT.Text = "Denumire *"
        lblDenClientT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtClientDen
        '
        txtClientDen.BackColor = System.Drawing.Color.Transparent
        txtClientDen.Dock = System.Windows.Forms.DockStyle.Fill
        txtClientDen.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtClientDen.MaxLength = 255
        txtClientDen.Name = "txtClientDen"
        txtClientDen.Size = New System.Drawing.Size(553, 32)
        txtClientDen.TabIndex = 9
        '
        ' lblJudClientT
        '
        lblJudClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblJudClientT.Margin = New System.Windows.Forms.Padding(0)
        lblJudClientT.Name = "lblJudClientT"
        lblJudClientT.TabIndex = 10
        lblJudClientT.Text = "Județ"
        lblJudClientT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' cmbClientJud
        '
        cmbClientJud.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        cmbClientJud.Editable = True
        cmbClientJud.FindAsYouType = True
        cmbClientJud.Margin = New System.Windows.Forms.Padding(0, 3, 0, 3)
        cmbClientJud.Name = "cmbClientJud"
        cmbClientJud.Size = New System.Drawing.Size(553, 28)
        cmbClientJud.TabIndex = 11
        tips.SetToolTipText(cmbClientJud, "Județul clientului (București = B). Codul lui ajunge în fișierul trimis la ANAF.")
        '
        ' lblOrasClientT
        '
        lblOrasClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblOrasClientT.Margin = New System.Windows.Forms.Padding(0)
        lblOrasClientT.Name = "lblOrasClientT"
        lblOrasClientT.TabIndex = 12
        lblOrasClientT.Text = "Oraș"
        lblOrasClientT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtClientOras
        '
        txtClientOras.BackColor = System.Drawing.Color.Transparent
        txtClientOras.Dock = System.Windows.Forms.DockStyle.Fill
        txtClientOras.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtClientOras.MaxLength = 255
        txtClientOras.Name = "txtClientOras"
        txtClientOras.Size = New System.Drawing.Size(553, 32)
        txtClientOras.TabIndex = 13
        tips.SetToolTipText(txtClientOras, "Pentru București: SECTOR1 ... SECTOR6.")
        '
        ' lblSectorT
        '
        lblSectorT.Dock = System.Windows.Forms.DockStyle.Fill
        lblSectorT.Margin = New System.Windows.Forms.Padding(0)
        lblSectorT.Name = "lblSectorT"
        lblSectorT.TabIndex = 14
        lblSectorT.Text = "Sector"
        lblSectorT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' cmbClientSector
        '
        cmbClientSector.Anchor = System.Windows.Forms.AnchorStyles.Left
        cmbClientSector.Margin = New System.Windows.Forms.Padding(0, 3, 0, 3)
        cmbClientSector.Name = "cmbClientSector"
        cmbClientSector.Size = New System.Drawing.Size(170, 28)
        cmbClientSector.TabIndex = 15
        tips.SetToolTipText(cmbClientSector, "Doar pentru clienții din București.")
        '
        ' lblAdresaClientT
        '
        lblAdresaClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblAdresaClientT.Margin = New System.Windows.Forms.Padding(0)
        lblAdresaClientT.Name = "lblAdresaClientT"
        lblAdresaClientT.TabIndex = 16
        lblAdresaClientT.Text = "Adresă"
        lblAdresaClientT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtClientAdresa
        '
        txtClientAdresa.BackColor = System.Drawing.Color.Transparent
        txtClientAdresa.Dock = System.Windows.Forms.DockStyle.Fill
        txtClientAdresa.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtClientAdresa.MaxLength = 255
        txtClientAdresa.Name = "txtClientAdresa"
        txtClientAdresa.Size = New System.Drawing.Size(553, 32)
        txtClientAdresa.TabIndex = 17
        '
        ' lblContClientT
        '
        lblContClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblContClientT.Margin = New System.Windows.Forms.Padding(0)
        lblContClientT.Name = "lblContClientT"
        lblContClientT.TabIndex = 18
        lblContClientT.Text = "Cont (IBAN)"
        lblContClientT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtClientCont
        '
        txtClientCont.BackColor = System.Drawing.Color.Transparent
        txtClientCont.Dock = System.Windows.Forms.DockStyle.Fill
        txtClientCont.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtClientCont.MaxLength = 34
        txtClientCont.Name = "txtClientCont"
        txtClientCont.Size = New System.Drawing.Size(553, 32)
        txtClientCont.TabIndex = 19
        tips.SetToolTipText(txtClientCont, "Se păstrează la client; nu ajunge în fișierul trimis la ANAF.")
        '
        ' lblBancaClientT
        '
        lblBancaClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblBancaClientT.Margin = New System.Windows.Forms.Padding(0)
        lblBancaClientT.Name = "lblBancaClientT"
        lblBancaClientT.TabIndex = 20
        lblBancaClientT.Text = "Banca"
        lblBancaClientT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtClientBanca
        '
        txtClientBanca.BackColor = System.Drawing.Color.Transparent
        txtClientBanca.Dock = System.Windows.Forms.DockStyle.Fill
        txtClientBanca.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtClientBanca.MaxLength = 255
        txtClientBanca.Name = "txtClientBanca"
        txtClientBanca.Size = New System.Drawing.Size(553, 32)
        txtClientBanca.TabIndex = 21
        '
        ' flowClient
        '
        flowClient.AutoSize = True
        flowClient.Controls.Add(btnClientNou)
        flowClient.Controls.Add(btnClientSalveaza)
        flowClient.Controls.Add(btnClientSterge)
        tlyClient.SetColumnSpan(flowClient, 2)
        flowClient.Dock = System.Windows.Forms.DockStyle.Fill
        flowClient.Margin = New System.Windows.Forms.Padding(0)
        flowClient.Name = "flowClient"
        flowClient.Size = New System.Drawing.Size(723, 50)
        flowClient.TabIndex = 22
        flowClient.WrapContents = False
        '
        ' btnClientNou
        '
        btnClientNou.AutoSize = True
        btnClientNou.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnClientNou.Margin = New System.Windows.Forms.Padding(0, 6, 10, 0)
        btnClientNou.Name = "btnClientNou"
        btnClientNou.Padding = New System.Windows.Forms.Padding(14, 2, 14, 2)
        btnClientNou.TabIndex = 0
        btnClientNou.Text = "Client nou"
        tips.SetToolTipText(btnClientNou, "Golește câmpurile clientului ca să adăugați unul nou; apoi «Salvează clientul».")
        btnClientNou.UseVisualStyleBackColor = True
        '
        ' btnClientSalveaza
        '
        btnClientSalveaza.AutoSize = True
        btnClientSalveaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnClientSalveaza.Margin = New System.Windows.Forms.Padding(0, 6, 10, 0)
        btnClientSalveaza.Name = "btnClientSalveaza"
        btnClientSalveaza.Padding = New System.Windows.Forms.Padding(14, 2, 14, 2)
        btnClientSalveaza.TabIndex = 1
        btnClientSalveaza.Text = "Salvează clientul"
        tips.SetToolTipText(btnClientSalveaza, "Scrie clientul în baza de date (separat de factură) și îl alege pentru factura de față.")
        btnClientSalveaza.UseVisualStyleBackColor = True
        '
        ' btnClientSterge
        '
        btnClientSterge.AutoSize = True
        btnClientSterge.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnClientSterge.Margin = New System.Windows.Forms.Padding(0, 6, 0, 0)
        btnClientSterge.Name = "btnClientSterge"
        btnClientSterge.Padding = New System.Windows.Forms.Padding(14, 2, 14, 2)
        btnClientSterge.TabIndex = 2
        btnClientSterge.Text = "Șterge clientul"
        tips.SetToolTipText(btnClientSterge, "Șterge clientul ales. Un client care are facturi nu se poate șterge.")
        btnClientSterge.UseVisualStyleBackColor = True
        '
        ' VanzareCumparatorPage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyClient)
        Name = "VanzareCumparatorPage"
        Padding = New System.Windows.Forms.Padding(3)
        Size = New System.Drawing.Size(757, 526)
        tlyClient.ResumeLayout(False)
        tlyClient.PerformLayout()
        flowClient.ResumeLayout(False)
        flowClient.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyClient As KBotTableLayoutPanel
    Friend WithEvents lblClientT As System.Windows.Forms.Label
    Friend WithEvents cmbClient As KBotComboBox
    Friend WithEvents lblCnpT As System.Windows.Forms.Label
    Friend WithEvents chkCnp As System.Windows.Forms.CheckBox
    Friend WithEvents lblIndT As System.Windows.Forms.Label
    Friend WithEvents txtClientInd As KBotTextField
    Friend WithEvents lblCfT As System.Windows.Forms.Label
    Friend WithEvents txtClientCf As KBotTextField
    Friend WithEvents lblDenClientT As System.Windows.Forms.Label
    Friend WithEvents txtClientDen As KBotTextField
    Friend WithEvents lblJudClientT As System.Windows.Forms.Label
    Friend WithEvents cmbClientJud As KBotComboBox
    Friend WithEvents lblOrasClientT As System.Windows.Forms.Label
    Friend WithEvents txtClientOras As KBotTextField
    Friend WithEvents lblSectorT As System.Windows.Forms.Label
    Friend WithEvents cmbClientSector As KBotComboBox
    Friend WithEvents lblAdresaClientT As System.Windows.Forms.Label
    Friend WithEvents txtClientAdresa As KBotTextField
    Friend WithEvents lblContClientT As System.Windows.Forms.Label
    Friend WithEvents txtClientCont As KBotTextField
    Friend WithEvents lblBancaClientT As System.Windows.Forms.Label
    Friend WithEvents txtClientBanca As KBotTextField
    Friend WithEvents flowClient As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnClientNou As System.Windows.Forms.Button
    Friend WithEvents btnClientSalveaza As System.Windows.Forms.Button
    Friend WithEvents btnClientSterge As System.Windows.Forms.Button
End Class
