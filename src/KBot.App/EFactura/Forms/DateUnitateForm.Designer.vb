Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DateUnitateForm
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
        components = New System.ComponentModel.Container()
        tips = New Global.KBot.Controls.KBotToolTip(components)
        pnlCard = New System.Windows.Forms.Panel()
        tlyCampuri = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblDen = New System.Windows.Forms.Label()
        pnlDenCf = New System.Windows.Forms.Panel()
        txtDen = New Global.KBot.Controls.KBotTextField()
        lblCf = New System.Windows.Forms.Label()
        txtCf = New Global.KBot.Controls.KBotTextField()
        lblJud = New System.Windows.Forms.Label()
        cmbJud = New Global.KBot.Controls.KBotComboBox()
        lblOras = New System.Windows.Forms.Label()
        txtOras = New Global.KBot.Controls.KBotTextField()
        lblAdresa = New System.Windows.Forms.Label()
        txtAdresa = New Global.KBot.Controls.KBotTextField()
        lblTel = New System.Windows.Forms.Label()
        txtTel = New Global.KBot.Controls.KBotTextField()
        lblMail = New System.Windows.Forms.Label()
        txtMail = New Global.KBot.Controls.KBotTextField()
        lblSerie = New System.Windows.Forms.Label()
        txtSerie = New Global.KBot.Controls.KBotTextField()
        lblNumar = New System.Windows.Forms.Label()
        txtNumar = New Global.KBot.Controls.KBotTextField()
        lblNota = New System.Windows.Forms.Label()
        ntfMesaj = New Global.KBot.Controls.KBotNotice()
        pnlJos = New System.Windows.Forms.Panel()
        btnSalveaza = New System.Windows.Forms.Button()
        btnAnaf = New System.Windows.Forms.Button()
        lblSep1 = New System.Windows.Forms.Label()
        btnIesire = New System.Windows.Forms.Button()
        busy = New Global.KBot.Controls.KBotBusyBar()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlyCampuri.SuspendLayout()
        pnlDenCf.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlyCampuri)
        pnlCard.Controls.Add(ntfMesaj)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(busy)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(2, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(716, 466)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' tlyCampuri
        '
        tlyCampuri.ColumnCount = 4
        tlyCampuri.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120.0!))
        tlyCampuri.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55.0!))
        tlyCampuri.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110.0!))
        tlyCampuri.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.0!))
        tlyCampuri.Controls.Add(lblDen, 0, 0)
        tlyCampuri.Controls.Add(pnlDenCf, 1, 0)
        tlyCampuri.Controls.Add(lblJud, 0, 1)
        tlyCampuri.Controls.Add(cmbJud, 1, 1)
        tlyCampuri.Controls.Add(lblOras, 2, 1)
        tlyCampuri.Controls.Add(txtOras, 3, 1)
        tlyCampuri.Controls.Add(lblAdresa, 0, 2)
        tlyCampuri.Controls.Add(txtAdresa, 1, 2)
        tlyCampuri.Controls.Add(lblTel, 0, 3)
        tlyCampuri.Controls.Add(txtTel, 1, 3)
        tlyCampuri.Controls.Add(lblMail, 2, 3)
        tlyCampuri.Controls.Add(txtMail, 3, 3)
        tlyCampuri.Controls.Add(lblSerie, 0, 4)
        tlyCampuri.Controls.Add(txtSerie, 1, 4)
        tlyCampuri.Controls.Add(lblNumar, 2, 4)
        tlyCampuri.Controls.Add(txtNumar, 3, 4)
        tlyCampuri.Controls.Add(lblNota, 0, 5)
        tlyCampuri.Dock = System.Windows.Forms.DockStyle.Fill
        tlyCampuri.Location = New System.Drawing.Point(0, 90)
        tlyCampuri.Margin = New System.Windows.Forms.Padding(0)
        tlyCampuri.Name = "tlyCampuri"
        tlyCampuri.Padding = New System.Windows.Forms.Padding(16, 12, 16, 8)
        tlyCampuri.RowCount = 6
        tlyCampuri.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46.0!))
        tlyCampuri.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46.0!))
        tlyCampuri.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46.0!))
        tlyCampuri.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46.0!))
        tlyCampuri.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46.0!))
        tlyCampuri.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyCampuri.Size = New System.Drawing.Size(716, 318)
        tlyCampuri.TabIndex = 0
        '
        ' lblDen
        '
        lblDen.Dock = System.Windows.Forms.DockStyle.Fill
        lblDen.Margin = New System.Windows.Forms.Padding(0)
        lblDen.Name = "lblDen"
        lblDen.TabIndex = 0
        lblDen.Text = "Denumire *"
        lblDen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' pnlDenCf
        '
        tlyCampuri.SetColumnSpan(pnlDenCf, 3)
        pnlDenCf.Controls.Add(txtDen)
        pnlDenCf.Controls.Add(lblCf)
        pnlDenCf.Controls.Add(txtCf)
        pnlDenCf.Dock = System.Windows.Forms.DockStyle.Fill
        pnlDenCf.Margin = New System.Windows.Forms.Padding(0)
        pnlDenCf.Name = "pnlDenCf"
        pnlDenCf.Size = New System.Drawing.Size(564, 46)
        pnlDenCf.TabIndex = 1
        '
        ' txtDen
        '
        txtDen.BackColor = System.Drawing.Color.Transparent
        txtDen.Dock = System.Windows.Forms.DockStyle.Fill
        txtDen.Location = New System.Drawing.Point(0, 0)
        txtDen.MaxLength = 255
        txtDen.Name = "txtDen"
        txtDen.Size = New System.Drawing.Size(244, 46)
        txtDen.TabIndex = 0
        tips.SetToolTipText(txtDen, "Denumirea unității, așa cum merge în factură.")
        '
        ' lblCf
        '
        lblCf.Dock = System.Windows.Forms.DockStyle.Right
        lblCf.Name = "lblCf"
        lblCf.Padding = New System.Windows.Forms.Padding(0, 0, 10, 0)
        lblCf.Size = New System.Drawing.Size(120, 46)
        lblCf.TabIndex = 1
        lblCf.Text = "Cod fiscal *"
        lblCf.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        ' txtCf
        '
        txtCf.BackColor = System.Drawing.Color.Transparent
        txtCf.Dock = System.Windows.Forms.DockStyle.Right
        txtCf.MaxLength = 32
        txtCf.Name = "txtCf"
        txtCf.Size = New System.Drawing.Size(200, 46)
        txtCf.TabIndex = 2
        tips.SetToolTipText(txtCf, "Cifre, cu «RO» în față dacă unitatea este plătitoare de TVA (exact cum se trimite la ANAF).")
        '
        ' lblJud
        '
        lblJud.Dock = System.Windows.Forms.DockStyle.Fill
        lblJud.Margin = New System.Windows.Forms.Padding(0)
        lblJud.Name = "lblJud"
        lblJud.TabIndex = 2
        lblJud.Text = "Județ"
        lblJud.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' cmbJud
        '
        cmbJud.Dock = System.Windows.Forms.DockStyle.Fill
        cmbJud.Editable = True
        cmbJud.FindAsYouType = True
        cmbJud.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        cmbJud.Name = "cmbJud"
        cmbJud.TabIndex = 3
        tips.SetToolTipText(cmbJud, "Județul unității; în XML se trimite codul lui (de exemplu PH).")
        '
        ' lblOras
        '
        lblOras.Dock = System.Windows.Forms.DockStyle.Fill
        lblOras.Margin = New System.Windows.Forms.Padding(0)
        lblOras.Name = "lblOras"
        lblOras.Padding = New System.Windows.Forms.Padding(14, 0, 0, 0)
        lblOras.TabIndex = 4
        lblOras.Text = "Oraș"
        lblOras.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtOras
        '
        txtOras.BackColor = System.Drawing.Color.Transparent
        txtOras.Dock = System.Windows.Forms.DockStyle.Fill
        txtOras.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtOras.MaxLength = 255
        txtOras.Name = "txtOras"
        txtOras.TabIndex = 5
        '
        ' lblAdresa
        '
        lblAdresa.Dock = System.Windows.Forms.DockStyle.Fill
        lblAdresa.Margin = New System.Windows.Forms.Padding(0)
        lblAdresa.Name = "lblAdresa"
        lblAdresa.TabIndex = 6
        lblAdresa.Text = "Adresă"
        lblAdresa.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtAdresa
        '
        txtAdresa.BackColor = System.Drawing.Color.Transparent
        tlyCampuri.SetColumnSpan(txtAdresa, 3)
        txtAdresa.Dock = System.Windows.Forms.DockStyle.Fill
        txtAdresa.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtAdresa.MaxLength = 255
        txtAdresa.Name = "txtAdresa"
        txtAdresa.TabIndex = 7
        '
        ' lblTel
        '
        lblTel.Dock = System.Windows.Forms.DockStyle.Fill
        lblTel.Margin = New System.Windows.Forms.Padding(0)
        lblTel.Name = "lblTel"
        lblTel.TabIndex = 8
        lblTel.Text = "Telefon"
        lblTel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtTel
        '
        txtTel.BackColor = System.Drawing.Color.Transparent
        txtTel.Dock = System.Windows.Forms.DockStyle.Fill
        txtTel.InputMask = "0000.000.000"
        txtTel.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtTel.MaxLength = 64
        txtTel.Name = "txtTel"
        txtTel.TabIndex = 9
        tips.SetToolTipText(txtTel, "Zece cifre; punctele se pun singure (de exemplu 0721.123.456). Poate rămâne gol.")
        '
        ' lblMail
        '
        lblMail.Dock = System.Windows.Forms.DockStyle.Fill
        lblMail.Margin = New System.Windows.Forms.Padding(0)
        lblMail.Name = "lblMail"
        lblMail.Padding = New System.Windows.Forms.Padding(14, 0, 0, 0)
        lblMail.TabIndex = 10
        lblMail.Text = "E-mail"
        lblMail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtMail
        '
        txtMail.BackColor = System.Drawing.Color.Transparent
        txtMail.Dock = System.Windows.Forms.DockStyle.Fill
        txtMail.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtMail.MaxLength = 255
        txtMail.Name = "txtMail"
        txtMail.TabIndex = 11
        '
        ' lblSerie
        '
        lblSerie.Dock = System.Windows.Forms.DockStyle.Fill
        lblSerie.Margin = New System.Windows.Forms.Padding(0)
        lblSerie.Name = "lblSerie"
        lblSerie.TabIndex = 12
        lblSerie.Text = "Seria facturilor"
        lblSerie.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtSerie
        '
        txtSerie.BackColor = System.Drawing.Color.Transparent
        txtSerie.Dock = System.Windows.Forms.DockStyle.Fill
        txtSerie.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtSerie.MaxLength = 10
        txtSerie.Name = "txtSerie"
        txtSerie.TabIndex = 13
        tips.SetToolTipText(txtSerie, "Se pune în fața numărului fiecărei facturi (de exemplu «AB»). Poate rămâne goală. După prima factură emisă nu se mai schimbă.")
        '
        ' lblNumar
        '
        lblNumar.Dock = System.Windows.Forms.DockStyle.Fill
        lblNumar.Margin = New System.Windows.Forms.Padding(0)
        lblNumar.Name = "lblNumar"
        lblNumar.Padding = New System.Windows.Forms.Padding(14, 0, 0, 0)
        lblNumar.TabIndex = 14
        lblNumar.Text = "Primul număr"
        lblNumar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtNumar
        '
        txtNumar.BackColor = System.Drawing.Color.Transparent
        txtNumar.Dock = System.Windows.Forms.DockStyle.Fill
        txtNumar.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        txtNumar.MaxLength = 9
        txtNumar.Name = "txtNumar"
        txtNumar.TabIndex = 15
        tips.SetToolTipText(txtNumar, "De aici începe numerotarea facturilor seriei. După prima factură emisă nu se mai schimbă.")
        '
        ' lblNota
        '
        tlyCampuri.SetColumnSpan(lblNota, 4)
        lblNota.Dock = System.Windows.Forms.DockStyle.Fill
        lblNota.Margin = New System.Windows.Forms.Padding(0, 8, 0, 0)
        lblNota.Name = "lblNota"
        lblNota.TabIndex = 16
        lblNota.Text = "ATENȚIE! Orice modificare a datelor unității are efect asupra tuturor documentelor generate de aici înainte."
        lblNota.TextAlign = System.Drawing.ContentAlignment.TopLeft
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = System.Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Top
        ntfMesaj.Location = New System.Drawing.Point(0, 46)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(716, 44)
        ntfMesaj.TabIndex = 2
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnSalveaza)
        pnlJos.Controls.Add(btnAnaf)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Controls.Add(btnIesire)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 408)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(716, 58)
        pnlJos.TabIndex = 4
        pnlJos.Tag = "Card"
        '
        ' btnSalveaza
        '
        btnSalveaza.Dock = System.Windows.Forms.DockStyle.Right
        btnSalveaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnSalveaza.Location = New System.Drawing.Point(540, 8)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New System.Drawing.Size(168, 42)
        btnSalveaza.TabIndex = 0
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Scrie datele unității emitente în baza de date.")
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' btnAnaf
        '
        btnAnaf.Dock = System.Windows.Forms.DockStyle.Left
        btnAnaf.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnAnaf.Location = New System.Drawing.Point(160, 8)
        btnAnaf.Name = "btnAnaf"
        btnAnaf.Size = New System.Drawing.Size(220, 42)
        btnAnaf.TabIndex = 2
        btnAnaf.Text = "Preia de la ANAF"
        tips.SetToolTipHeader(btnAnaf, "Preia de la ANAF")
        tips.SetToolTipText(btnAnaf, "Completează denumirea și adresa unității după codul ei fiscal din lista unităților. Se poate face o singură dată; după aceea butonul se închide.")
        btnAnaf.UseVisualStyleBackColor = True
        '
        ' lblSep1
        '
        lblSep1.Dock = System.Windows.Forms.DockStyle.Left
        lblSep1.Location = New System.Drawing.Point(148, 8)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(12, 42)
        lblSep1.TabIndex = 3
        '
        ' btnIesire
        '
        btnIesire.Dock = System.Windows.Forms.DockStyle.Left
        btnIesire.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnIesire.Location = New System.Drawing.Point(8, 8)
        btnIesire.Name = "btnIesire"
        btnIesire.Size = New System.Drawing.Size(140, 42)
        btnIesire.TabIndex = 1
        btnIesire.Text = "Ieșire"
        tips.SetToolTipHeader(btnIesire, "Ieșire")
        tips.SetToolTipText(btnIesire, "Închide fereastra; dacă sunt modificări nesalvate, întreabă mai întâi.")
        btnIesire.UseVisualStyleBackColor = True
        '
        ' busy
        '
        busy.Dock = System.Windows.Forms.DockStyle.Top
        busy.Location = New System.Drawing.Point(0, 40)
        busy.Name = "busy"
        busy.Size = New System.Drawing.Size(716, 6)
        busy.TabIndex = 1
        '
        ' capBar
        '
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New System.Drawing.Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New System.Drawing.Size(716, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Date Unitate"
        '
        ' DateUnitateForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnIesire
        ClientSize = New System.Drawing.Size(720, 470)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "DateUnitateForm"
        Padding = New System.Windows.Forms.Padding(2)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Date Unitate"
        pnlCard.ResumeLayout(False)
        tlyCampuri.ResumeLayout(False)
        pnlDenCf.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents busy As Global.KBot.Controls.KBotBusyBar
    Friend WithEvents ntfMesaj As Global.KBot.Controls.KBotNotice
    Friend WithEvents tlyCampuri As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblDen As System.Windows.Forms.Label
    Friend WithEvents pnlDenCf As System.Windows.Forms.Panel
    Friend WithEvents txtDen As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblCf As System.Windows.Forms.Label
    Friend WithEvents txtCf As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblJud As System.Windows.Forms.Label
    Friend WithEvents cmbJud As Global.KBot.Controls.KBotComboBox
    Friend WithEvents lblOras As System.Windows.Forms.Label
    Friend WithEvents txtOras As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblAdresa As System.Windows.Forms.Label
    Friend WithEvents txtAdresa As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblTel As System.Windows.Forms.Label
    Friend WithEvents txtTel As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblMail As System.Windows.Forms.Label
    Friend WithEvents txtMail As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblSerie As System.Windows.Forms.Label
    Friend WithEvents txtSerie As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblNumar As System.Windows.Forms.Label
    Friend WithEvents txtNumar As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblNota As System.Windows.Forms.Label
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnSalveaza As System.Windows.Forms.Button
    Friend WithEvents btnAnaf As System.Windows.Forms.Button
    Friend WithEvents lblSep1 As System.Windows.Forms.Label
    Friend WithEvents btnIesire As System.Windows.Forms.Button
End Class
