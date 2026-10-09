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
        components = New ComponentModel.Container()
        tips = New KBotToolTip(components)
        cmbClient = New KBotComboBox()
        chkCnp = New System.Windows.Forms.CheckBox()
        txtClientCf = New KBotTextField()
        txtClientInd = New KBotTextField()
        cmbClientJud = New KBotComboBox()
        cmbClientSector = New KBotComboBox()
        txtClientCont = New KBotTextField()
        btnClientNou = New System.Windows.Forms.Button()
        btnClientSalveaza = New System.Windows.Forms.Button()
        btnClientSterge = New System.Windows.Forms.Button()
        tlyClient = New KBotTableLayoutPanel()
        lblClientT = New System.Windows.Forms.Label()
        lblCnpT = New System.Windows.Forms.Label()
        lblCfT = New System.Windows.Forms.Label()
        tlyCf = New KBotTableLayoutPanel()
        lblIndT = New System.Windows.Forms.Label()
        lblDenClientT = New System.Windows.Forms.Label()
        txtClientDen = New KBotTextField()
        lblJudClientT = New System.Windows.Forms.Label()
        tlyLoc = New KBotTableLayoutPanel()
        lblOrasClientT = New System.Windows.Forms.Label()
        txtClientOras = New KBotTextField()
        lblSectorT = New System.Windows.Forms.Label()
        lblAdresaClientT = New System.Windows.Forms.Label()
        txtClientAdresa = New KBotTextField()
        lblContClientT = New System.Windows.Forms.Label()
        lblBancaClientT = New System.Windows.Forms.Label()
        txtClientBanca = New KBotTextField()
        tlyBtn = New KBotTableLayoutPanel()
        tlyClient.SuspendLayout()
        tlyCf.SuspendLayout()
        tlyLoc.SuspendLayout()
        tlyBtn.SuspendLayout()
        SuspendLayout()
        ' 
        ' cmbClient
        ' 
        cmbClient.Anchor = System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right
        cmbClient.Editable = True
        cmbClient.FindAsYouType = True
        cmbClient.Location = New System.Drawing.Point(165, 18)
        cmbClient.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        cmbClient.Name = "cmbClient"
        cmbClient.Size = New System.Drawing.Size(838, 37)
        cmbClient.TabIndex = 1
        tips.SetToolTipHeader(cmbClient, "Clientul facturii")
        tips.SetToolTipText(cmbClient, "Alegeți un client existent (tastați o parte din denumire sau din codul fiscal) sau apăsați «Client nou».")
        ' 
        ' chkCnp
        ' 
        chkCnp.AutoSize = True
        chkCnp.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        chkCnp.Location = New System.Drawing.Point(165, 74)
        chkCnp.Margin = New System.Windows.Forms.Padding(0, 9, 0, 0)
        chkCnp.Name = "chkCnp"
        chkCnp.Size = New System.Drawing.Size(221, 26)
        chkCnp.TabIndex = 3
        chkCnp.Text = "Codul de mai jos este CNP"
        tips.SetToolTipText(chkCnp, "Bifați când clientul este o persoană fizică și în câmpul «Cod fiscal» este CNP-ul ei.")
        ' 
        ' txtClientCf
        ' 
        txtClientCf.Anchor = System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right
        txtClientCf.BackColor = Drawing.Color.Transparent
        txtClientCf.Location = New System.Drawing.Point(0, 6)
        txtClientCf.Margin = New System.Windows.Forms.Padding(0, 0, 27, 0)
        txtClientCf.MaxLength = 32
        txtClientCf.Name = "txtClientCf"
        txtClientCf.Size = New System.Drawing.Size(330, 48)
        txtClientCf.TabIndex = 0
        tips.SetToolTipText(txtClientCf, "Tastați codul fiscal: denumirea, adresa și prefixul se completează de la ANAF.")
        ' 
        ' txtClientInd
        ' 
        txtClientInd.Anchor = System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right
        txtClientInd.BackColor = Drawing.Color.Transparent
        txtClientInd.Location = New System.Drawing.Point(499, 6)
        txtClientInd.Margin = New System.Windows.Forms.Padding(0)
        txtClientInd.MaxLength = 8
        txtClientInd.Name = "txtClientInd"
        txtClientInd.Size = New System.Drawing.Size(165, 48)
        txtClientInd.TabIndex = 2
        tips.SetToolTipText(txtClientInd, "«RO» când clientul este plătitor de TVA; altfel gol.")
        ' 
        ' cmbClientJud
        ' 
        cmbClientJud.Anchor = System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right
        cmbClientJud.Editable = True
        cmbClientJud.FindAsYouType = True
        cmbClientJud.Location = New System.Drawing.Point(0, 9)
        cmbClientJud.Margin = New System.Windows.Forms.Padding(0, 0, 21, 0)
        cmbClientJud.Name = "cmbClientJud"
        cmbClientJud.Size = New System.Drawing.Size(225, 37)
        cmbClientJud.TabIndex = 0
        tips.SetToolTipText(cmbClientJud, "Județul clientului (București = B). Codul lui ajunge în fișierul trimis la ANAF.")
        ' 
        ' cmbClientSector
        ' 
        cmbClientSector.Anchor = System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right
        cmbClientSector.Location = New System.Drawing.Point(647, 9)
        cmbClientSector.Margin = New System.Windows.Forms.Padding(0)
        cmbClientSector.Name = "cmbClientSector"
        cmbClientSector.Size = New System.Drawing.Size(180, 37)
        cmbClientSector.TabIndex = 4
        tips.SetToolTipText(cmbClientSector, "Doar pentru clienții din București.")
        cmbClientSector.Visible = False
        ' 
        ' txtClientCont
        ' 
        txtClientCont.BackColor = Drawing.Color.Transparent
        txtClientCont.Dock = System.Windows.Forms.DockStyle.Fill
        txtClientCont.Location = New System.Drawing.Point(165, 359)
        txtClientCont.Margin = New System.Windows.Forms.Padding(0, 6, 0, 6)
        txtClientCont.MaxLength = 34
        txtClientCont.Name = "txtClientCont"
        txtClientCont.Size = New System.Drawing.Size(838, 48)
        txtClientCont.TabIndex = 19
        tips.SetToolTipText(txtClientCont, "Se păstrează la client; nu ajunge în fișierul trimis la ANAF.")
        ' 
        ' btnClientNou
        ' 
        btnClientNou.Anchor = System.Windows.Forms.AnchorStyles.Left
        btnClientNou.AutoSize = True
        btnClientNou.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnClientNou.Location = New System.Drawing.Point(0, 9)
        btnClientNou.Margin = New System.Windows.Forms.Padding(0, 0, 15, 0)
        btnClientNou.Name = "btnClientNou"
        btnClientNou.Padding = New System.Windows.Forms.Padding(21, 3, 21, 3)
        btnClientNou.Size = New System.Drawing.Size(198, 62)
        btnClientNou.TabIndex = 0
        btnClientNou.Text = "Client nou"
        tips.SetToolTipText(btnClientNou, "Golește câmpurile clientului ca să adăugați unul nou; apoi «Salvează clientul».")
        btnClientNou.UseVisualStyleBackColor = True
        ' 
        ' btnClientSalveaza
        ' 
        btnClientSalveaza.Anchor = System.Windows.Forms.AnchorStyles.Left
        btnClientSalveaza.AutoSize = True
        btnClientSalveaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnClientSalveaza.Location = New System.Drawing.Point(213, 9)
        btnClientSalveaza.Margin = New System.Windows.Forms.Padding(0, 0, 15, 0)
        btnClientSalveaza.Name = "btnClientSalveaza"
        btnClientSalveaza.Padding = New System.Windows.Forms.Padding(21, 3, 21, 3)
        btnClientSalveaza.Size = New System.Drawing.Size(268, 62)
        btnClientSalveaza.TabIndex = 1
        btnClientSalveaza.Text = "Salvează clientul"
        tips.SetToolTipText(btnClientSalveaza, "Scrie clientul în baza de date (separat de factură) și îl alege pentru factura de față.")
        btnClientSalveaza.UseVisualStyleBackColor = True
        ' 
        ' btnClientSterge
        ' 
        btnClientSterge.Anchor = System.Windows.Forms.AnchorStyles.Left
        btnClientSterge.AutoSize = True
        btnClientSterge.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnClientSterge.Location = New System.Drawing.Point(496, 9)
        btnClientSterge.Margin = New System.Windows.Forms.Padding(0)
        btnClientSterge.Name = "btnClientSterge"
        btnClientSterge.Padding = New System.Windows.Forms.Padding(21, 3, 21, 3)
        btnClientSterge.Size = New System.Drawing.Size(243, 62)
        btnClientSterge.TabIndex = 2
        btnClientSterge.Text = "Șterge clientul"
        tips.SetToolTipText(btnClientSterge, "Șterge clientul ales. Un client care are facturi nu se poate șterge.")
        btnClientSterge.UseVisualStyleBackColor = True
        ' 
        ' tlyClient
        ' 
        tlyClient.ColumnCount = 2
        tlyClient.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 155F))
        tlyClient.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyClient.Controls.Add(lblClientT, 0, 0)
        tlyClient.Controls.Add(cmbClient, 1, 0)
        tlyClient.Controls.Add(lblCnpT, 0, 1)
        tlyClient.Controls.Add(chkCnp, 1, 1)
        tlyClient.Controls.Add(lblCfT, 0, 2)
        tlyClient.Controls.Add(tlyCf, 1, 2)
        tlyClient.Controls.Add(lblDenClientT, 0, 3)
        tlyClient.Controls.Add(txtClientDen, 1, 3)
        tlyClient.Controls.Add(lblJudClientT, 0, 4)
        tlyClient.Controls.Add(tlyLoc, 1, 4)
        tlyClient.Controls.Add(lblAdresaClientT, 0, 5)
        tlyClient.Controls.Add(txtClientAdresa, 1, 5)
        tlyClient.Controls.Add(lblContClientT, 0, 6)
        tlyClient.Controls.Add(txtClientCont, 1, 6)
        tlyClient.Controls.Add(lblBancaClientT, 0, 7)
        tlyClient.Controls.Add(txtClientBanca, 1, 7)
        tlyClient.Controls.Add(tlyBtn, 0, 8)
        tlyClient.Dock = System.Windows.Forms.DockStyle.Fill
        tlyClient.Location = New System.Drawing.Point(4, 4)
        tlyClient.Margin = New System.Windows.Forms.Padding(0)
        tlyClient.Name = "tlyClient"
        tlyClient.Padding = New System.Windows.Forms.Padding(10, 8, 10, 8)
        tlyClient.RowCount = 10
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F))
        tlyClient.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyClient.Size = New System.Drawing.Size(1013, 562)
        tlyClient.TabIndex = 0
        ' 
        ' lblClientT
        ' 
        lblClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblClientT.Location = New System.Drawing.Point(10, 8)
        lblClientT.Margin = New System.Windows.Forms.Padding(0)
        lblClientT.Name = "lblClientT"
        lblClientT.Size = New System.Drawing.Size(155, 57)
        lblClientT.TabIndex = 0
        lblClientT.Text = "Client *"
        lblClientT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblCnpT
        ' 
        lblCnpT.Dock = System.Windows.Forms.DockStyle.Fill
        lblCnpT.Location = New System.Drawing.Point(10, 65)
        lblCnpT.Margin = New System.Windows.Forms.Padding(0)
        lblCnpT.Name = "lblCnpT"
        lblCnpT.Size = New System.Drawing.Size(155, 48)
        lblCnpT.TabIndex = 2
        lblCnpT.Text = "Persoană fizică"
        lblCnpT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblCfT
        ' 
        lblCfT.Dock = System.Windows.Forms.DockStyle.Fill
        lblCfT.Location = New System.Drawing.Point(10, 113)
        lblCfT.Margin = New System.Windows.Forms.Padding(0)
        lblCfT.Name = "lblCfT"
        lblCfT.Size = New System.Drawing.Size(155, 60)
        lblCfT.TabIndex = 4
        lblCfT.Text = "Cod fiscal"
        lblCfT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' tlyCf
        ' 
        tlyCf.ColumnCount = 4
        tlyCf.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 357F))
        tlyCf.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize))
        tlyCf.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 165F))
        tlyCf.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyCf.Controls.Add(txtClientCf, 0, 0)
        tlyCf.Controls.Add(lblIndT, 1, 0)
        tlyCf.Controls.Add(txtClientInd, 2, 0)
        tlyCf.Dock = System.Windows.Forms.DockStyle.Fill
        tlyCf.Location = New System.Drawing.Point(165, 113)
        tlyCf.Margin = New System.Windows.Forms.Padding(0)
        tlyCf.Name = "tlyCf"
        tlyCf.RowCount = 1
        tlyCf.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyCf.Size = New System.Drawing.Size(838, 60)
        tlyCf.TabIndex = 5
        ' 
        ' lblIndT
        ' 
        lblIndT.Anchor = System.Windows.Forms.AnchorStyles.Left
        lblIndT.AutoSize = True
        lblIndT.Location = New System.Drawing.Point(357, 16)
        lblIndT.Margin = New System.Windows.Forms.Padding(0, 0, 12, 0)
        lblIndT.Name = "lblIndT"
        lblIndT.Size = New System.Drawing.Size(130, 22)
        lblIndT.TabIndex = 1
        lblIndT.Text = "Prefix fiscal (RO)"
        ' 
        ' lblDenClientT
        ' 
        lblDenClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblDenClientT.Location = New System.Drawing.Point(10, 173)
        lblDenClientT.Margin = New System.Windows.Forms.Padding(0)
        lblDenClientT.Name = "lblDenClientT"
        lblDenClientT.Size = New System.Drawing.Size(155, 60)
        lblDenClientT.TabIndex = 8
        lblDenClientT.Text = "Denumire *"
        lblDenClientT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' txtClientDen
        ' 
        txtClientDen.BackColor = Drawing.Color.Transparent
        txtClientDen.Dock = System.Windows.Forms.DockStyle.Fill
        txtClientDen.Location = New System.Drawing.Point(165, 179)
        txtClientDen.Margin = New System.Windows.Forms.Padding(0, 6, 0, 6)
        txtClientDen.MaxLength = 255
        txtClientDen.Name = "txtClientDen"
        txtClientDen.Size = New System.Drawing.Size(838, 48)
        txtClientDen.TabIndex = 9
        ' 
        ' lblJudClientT
        ' 
        lblJudClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblJudClientT.Location = New System.Drawing.Point(10, 233)
        lblJudClientT.Margin = New System.Windows.Forms.Padding(0)
        lblJudClientT.Name = "lblJudClientT"
        lblJudClientT.Size = New System.Drawing.Size(155, 60)
        lblJudClientT.TabIndex = 10
        lblJudClientT.Text = "Județ"
        lblJudClientT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' tlyLoc
        ' 
        tlyLoc.ColumnCount = 6
        tlyLoc.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 246F))
        tlyLoc.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize))
        tlyLoc.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 276F))
        tlyLoc.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize))
        tlyLoc.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F))
        tlyLoc.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyLoc.Controls.Add(cmbClientJud, 0, 0)
        tlyLoc.Controls.Add(lblOrasClientT, 1, 0)
        tlyLoc.Controls.Add(txtClientOras, 2, 0)
        tlyLoc.Controls.Add(lblSectorT, 3, 0)
        tlyLoc.Controls.Add(cmbClientSector, 4, 0)
        tlyLoc.Dock = System.Windows.Forms.DockStyle.Fill
        tlyLoc.Location = New System.Drawing.Point(165, 233)
        tlyLoc.Margin = New System.Windows.Forms.Padding(0)
        tlyLoc.Name = "tlyLoc"
        tlyLoc.RowCount = 1
        tlyLoc.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyLoc.Size = New System.Drawing.Size(838, 60)
        tlyLoc.TabIndex = 11
        ' 
        ' lblOrasClientT
        ' 
        lblOrasClientT.Anchor = System.Windows.Forms.AnchorStyles.Left
        lblOrasClientT.AutoSize = True
        lblOrasClientT.Location = New System.Drawing.Point(246, 16)
        lblOrasClientT.Margin = New System.Windows.Forms.Padding(0, 0, 12, 0)
        lblOrasClientT.Name = "lblOrasClientT"
        lblOrasClientT.Size = New System.Drawing.Size(44, 22)
        lblOrasClientT.TabIndex = 1
        lblOrasClientT.Text = "Oraș"
        ' 
        ' txtClientOras
        ' 
        txtClientOras.Anchor = System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right
        txtClientOras.BackColor = Drawing.Color.Transparent
        txtClientOras.Location = New System.Drawing.Point(302, 6)
        txtClientOras.Margin = New System.Windows.Forms.Padding(0, 0, 21, 0)
        txtClientOras.MaxLength = 255
        txtClientOras.Name = "txtClientOras"
        txtClientOras.Size = New System.Drawing.Size(255, 48)
        txtClientOras.TabIndex = 2
        ' 
        ' lblSectorT
        ' 
        lblSectorT.Anchor = System.Windows.Forms.AnchorStyles.Left
        lblSectorT.AutoSize = True
        lblSectorT.Location = New System.Drawing.Point(578, 16)
        lblSectorT.Margin = New System.Windows.Forms.Padding(0, 0, 12, 0)
        lblSectorT.Name = "lblSectorT"
        lblSectorT.Size = New System.Drawing.Size(57, 22)
        lblSectorT.TabIndex = 3
        lblSectorT.Text = "Sector"
        lblSectorT.Visible = False
        ' 
        ' lblAdresaClientT
        ' 
        lblAdresaClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblAdresaClientT.Location = New System.Drawing.Point(10, 293)
        lblAdresaClientT.Margin = New System.Windows.Forms.Padding(0)
        lblAdresaClientT.Name = "lblAdresaClientT"
        lblAdresaClientT.Size = New System.Drawing.Size(155, 60)
        lblAdresaClientT.TabIndex = 16
        lblAdresaClientT.Text = "Adresă"
        lblAdresaClientT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' txtClientAdresa
        ' 
        txtClientAdresa.BackColor = Drawing.Color.Transparent
        txtClientAdresa.Dock = System.Windows.Forms.DockStyle.Fill
        txtClientAdresa.Location = New System.Drawing.Point(165, 299)
        txtClientAdresa.Margin = New System.Windows.Forms.Padding(0, 6, 0, 6)
        txtClientAdresa.MaxLength = 255
        txtClientAdresa.Name = "txtClientAdresa"
        txtClientAdresa.Size = New System.Drawing.Size(838, 48)
        txtClientAdresa.TabIndex = 17
        ' 
        ' lblContClientT
        ' 
        lblContClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblContClientT.Location = New System.Drawing.Point(10, 353)
        lblContClientT.Margin = New System.Windows.Forms.Padding(0)
        lblContClientT.Name = "lblContClientT"
        lblContClientT.Size = New System.Drawing.Size(155, 60)
        lblContClientT.TabIndex = 18
        lblContClientT.Text = "Cont (IBAN)"
        lblContClientT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' lblBancaClientT
        ' 
        lblBancaClientT.Dock = System.Windows.Forms.DockStyle.Fill
        lblBancaClientT.Location = New System.Drawing.Point(10, 413)
        lblBancaClientT.Margin = New System.Windows.Forms.Padding(0)
        lblBancaClientT.Name = "lblBancaClientT"
        lblBancaClientT.Size = New System.Drawing.Size(155, 60)
        lblBancaClientT.TabIndex = 20
        lblBancaClientT.Text = "Banca"
        lblBancaClientT.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' txtClientBanca
        ' 
        txtClientBanca.BackColor = Drawing.Color.Transparent
        txtClientBanca.Dock = System.Windows.Forms.DockStyle.Fill
        txtClientBanca.Location = New System.Drawing.Point(165, 419)
        txtClientBanca.Margin = New System.Windows.Forms.Padding(0, 6, 0, 6)
        txtClientBanca.MaxLength = 255
        txtClientBanca.Name = "txtClientBanca"
        txtClientBanca.Size = New System.Drawing.Size(838, 48)
        txtClientBanca.TabIndex = 21
        ' 
        ' tlyBtn
        ' 
        tlyClient.SetColumnSpan(tlyBtn, 2)
        tlyBtn.ColumnCount = 4
        tlyBtn.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize))
        tlyBtn.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize))
        tlyBtn.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize))
        tlyBtn.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyBtn.Controls.Add(btnClientNou, 0, 0)
        tlyBtn.Controls.Add(btnClientSalveaza, 1, 0)
        tlyBtn.Controls.Add(btnClientSterge, 2, 0)
        tlyBtn.Dock = System.Windows.Forms.DockStyle.Fill
        tlyBtn.Location = New System.Drawing.Point(10, 473)
        tlyBtn.Margin = New System.Windows.Forms.Padding(0)
        tlyBtn.Name = "tlyBtn"
        tlyBtn.RowCount = 1
        tlyBtn.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyBtn.Size = New System.Drawing.Size(993, 75)
        tlyBtn.TabIndex = 22
        ' 
        ' VanzareCumparatorPage
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyClient)
        Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Name = "VanzareCumparatorPage"
        Padding = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Size = New System.Drawing.Size(1021, 570)
        tlyClient.ResumeLayout(False)
        tlyClient.PerformLayout()
        tlyCf.ResumeLayout(False)
        tlyCf.PerformLayout()
        tlyLoc.ResumeLayout(False)
        tlyLoc.PerformLayout()
        tlyBtn.ResumeLayout(False)
        tlyBtn.PerformLayout()
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
    Friend WithEvents tlyLoc As KBotTableLayoutPanel
    Friend WithEvents tlyCf As KBotTableLayoutPanel
    Friend WithEvents tlyBtn As KBotTableLayoutPanel
    Friend WithEvents btnClientNou As System.Windows.Forms.Button
    Friend WithEvents btnClientSalveaza As System.Windows.Forms.Button
    Friend WithEvents btnClientSterge As System.Windows.Forms.Button
End Class
