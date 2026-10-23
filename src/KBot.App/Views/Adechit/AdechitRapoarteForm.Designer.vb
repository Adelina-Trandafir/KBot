Imports KBot.Controls

' Slice 0123-01 -- the page «ADECHIT Rapoarte». All the controls are declared HERE (docs/kbot-forms-ui-convention.md).
' Left: what the reports need (subunit, year, month, group, interval, child) with one button per report. Right: the report as a PDF in the
' embedded Adobe surface, and the notice line. Rows are Dock.Top panels with a fixed height, added bottom row first (the last one added is on top);
' children are added in reverse dock order (Fill first, then the edges).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdechitRapoarteForm
    Inherits Global.KBot.Theming.KBotShellForm

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
        tmrCauta = New System.Windows.Forms.Timer(components)
        capBar = New KBotCaptionBar()
        busyBar = New KBotBusyBar()
        pnlCard = New System.Windows.Forms.Panel()
        pnlParam = New System.Windows.Forms.Panel()
        rowExcel = New System.Windows.Forms.Panel()
        btnExcel = New KBotButton()
        rowAnulate = New System.Windows.Forms.Panel()
        btnAnulate = New KBotButton()
        rowFisa = New System.Windows.Forms.Panel()
        btnFisa = New KBotButton()
        rowToti = New System.Windows.Forms.Panel()
        chkToti = New KBotCheckBox()
        rowCopil = New System.Windows.Forms.Panel()
        cboCopil = New KBotComboBox()
        lblCopil = New System.Windows.Forms.Label()
        rowCauta = New System.Windows.Forms.Panel()
        txtCauta = New KBotTextBox()
        lblCauta = New System.Windows.Forms.Label()
        rowRap2 = New System.Windows.Forms.Panel()
        btnBanca = New KBotButton()
        btnCasa = New KBotButton()
        rowInterval = New System.Windows.Forms.Panel()
        dtpPana = New KBotDatePicker()
        lblPana = New System.Windows.Forms.Label()
        dtpDeLa = New KBotDatePicker()
        lblDeLa = New System.Windows.Forms.Label()
        rowRap1 = New System.Windows.Forms.Panel()
        btnPlata = New KBotButton()
        btnSituatie = New KBotButton()
        rowToate = New System.Windows.Forms.Panel()
        chkSepara = New KBotCheckBox()
        chkToate = New KBotCheckBox()
        rowGrupa = New System.Windows.Forms.Panel()
        cboGrupa = New KBotComboBox()
        lblGrupa = New System.Windows.Forms.Label()
        rowLuna = New System.Windows.Forms.Panel()
        cboLuna = New KBotComboBox()
        lblLuna = New System.Windows.Forms.Label()
        rowAn = New System.Windows.Forms.Panel()
        cboAn = New KBotComboBox()
        lblAn = New System.Windows.Forms.Label()
        rowSub = New System.Windows.Forms.Panel()
        cboSub = New KBotComboBox()
        lblSub = New System.Windows.Forms.Label()
        pnlRaport = New System.Windows.Forms.Panel()
        previewPdf = New ReaderHostPreview()
        ntfMesaj = New KBotNotice()
        pnlCard.SuspendLayout()
        pnlParam.SuspendLayout()
        rowExcel.SuspendLayout()
        rowAnulate.SuspendLayout()
        rowFisa.SuspendLayout()
        rowToti.SuspendLayout()
        rowCopil.SuspendLayout()
        rowCauta.SuspendLayout()
        rowRap2.SuspendLayout()
        rowInterval.SuspendLayout()
        rowRap1.SuspendLayout()
        rowToate.SuspendLayout()
        rowGrupa.SuspendLayout()
        rowLuna.SuspendLayout()
        rowAn.SuspendLayout()
        rowSub.SuspendLayout()
        pnlRaport.SuspendLayout()
        SuspendLayout()
        '
        ' capBar
        '
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New System.Drawing.Point(3, 3)
        capBar.Margin = New System.Windows.Forms.Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowOptionsButton = False
        capBar.ShowTextScaleSlider = False
        capBar.Size = New System.Drawing.Size(1194, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "ADECHIT — Rapoarte"
        '
        ' busyBar: slides while anything is asked of the server (slice 0123-03), so the page never looks stuck
        '
        busyBar.Dock = System.Windows.Forms.DockStyle.Top
        busyBar.Location = New System.Drawing.Point(3, 69)
        busyBar.Margin = New System.Windows.Forms.Padding(0)
        busyBar.Name = "busyBar"
        busyBar.Size = New System.Drawing.Size(1194, 5)
        busyBar.TabIndex = 2
        busyBar.TabStop = False
        '
        ' lblSub / cboSub: the subunit (evidence) the reports are made for
        '
        lblSub.Dock = System.Windows.Forms.DockStyle.Left
        lblSub.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblSub.Location = New System.Drawing.Point(0, 0)
        lblSub.Margin = New System.Windows.Forms.Padding(0)
        lblSub.Name = "lblSub"
        lblSub.Size = New System.Drawing.Size(110, 34)
        lblSub.TabIndex = 0
        lblSub.Text = "Subunitate"
        lblSub.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        cboSub.Dock = System.Windows.Forms.DockStyle.Fill
        cboSub.Enabled = False
        cboSub.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cboSub.Location = New System.Drawing.Point(110, 0)
        cboSub.Margin = New System.Windows.Forms.Padding(0)
        cboSub.Name = "cboSub"
        cboSub.Size = New System.Drawing.Size(290, 34)
        cboSub.TabIndex = 1
        tips.SetToolTipHeader(cboSub, "Subunitate")
        tips.SetToolTipText(cboSub, "Evidența pentru care se fac rapoartele. Apare doar cele la care ai acces.")
        '
        ' rowSub
        '
        rowSub.Controls.Add(cboSub)
        rowSub.Controls.Add(lblSub)
        rowSub.Dock = System.Windows.Forms.DockStyle.Top
        rowSub.Location = New System.Drawing.Point(0, 0)
        rowSub.Margin = New System.Windows.Forms.Padding(0)
        rowSub.Name = "rowSub"
        rowSub.Size = New System.Drawing.Size(400, 34)
        rowSub.TabIndex = 12
        '
        ' lblAn / cboAn
        '
        lblAn.Dock = System.Windows.Forms.DockStyle.Left
        lblAn.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblAn.Location = New System.Drawing.Point(0, 0)
        lblAn.Margin = New System.Windows.Forms.Padding(0)
        lblAn.Name = "lblAn"
        lblAn.Size = New System.Drawing.Size(110, 34)
        lblAn.TabIndex = 0
        lblAn.Text = "An"
        lblAn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        cboAn.Dock = System.Windows.Forms.DockStyle.Fill
        cboAn.Enabled = False
        cboAn.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cboAn.Location = New System.Drawing.Point(110, 0)
        cboAn.Margin = New System.Windows.Forms.Padding(0)
        cboAn.Name = "cboAn"
        cboAn.Size = New System.Drawing.Size(290, 34)
        cboAn.TabIndex = 1
        '
        ' rowAn
        '
        rowAn.Controls.Add(cboAn)
        rowAn.Controls.Add(lblAn)
        rowAn.Dock = System.Windows.Forms.DockStyle.Top
        rowAn.Location = New System.Drawing.Point(0, 34)
        rowAn.Margin = New System.Windows.Forms.Padding(0)
        rowAn.Name = "rowAn"
        rowAn.Size = New System.Drawing.Size(400, 34)
        rowAn.TabIndex = 11
        '
        ' lblLuna / cboLuna
        '
        lblLuna.Dock = System.Windows.Forms.DockStyle.Left
        lblLuna.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblLuna.Location = New System.Drawing.Point(0, 0)
        lblLuna.Margin = New System.Windows.Forms.Padding(0)
        lblLuna.Name = "lblLuna"
        lblLuna.Size = New System.Drawing.Size(110, 34)
        lblLuna.TabIndex = 0
        lblLuna.Text = "Luna"
        lblLuna.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        cboLuna.Dock = System.Windows.Forms.DockStyle.Fill
        cboLuna.Enabled = False
        cboLuna.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cboLuna.Location = New System.Drawing.Point(110, 0)
        cboLuna.Margin = New System.Windows.Forms.Padding(0)
        cboLuna.Name = "cboLuna"
        cboLuna.Size = New System.Drawing.Size(290, 34)
        cboLuna.TabIndex = 1
        '
        ' rowLuna
        '
        rowLuna.Controls.Add(cboLuna)
        rowLuna.Controls.Add(lblLuna)
        rowLuna.Dock = System.Windows.Forms.DockStyle.Top
        rowLuna.Location = New System.Drawing.Point(0, 68)
        rowLuna.Margin = New System.Windows.Forms.Padding(0)
        rowLuna.Name = "rowLuna"
        rowLuna.Size = New System.Drawing.Size(400, 34)
        rowLuna.TabIndex = 10
        '
        ' lblGrupa / cboGrupa
        '
        lblGrupa.Dock = System.Windows.Forms.DockStyle.Left
        lblGrupa.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblGrupa.Location = New System.Drawing.Point(0, 0)
        lblGrupa.Margin = New System.Windows.Forms.Padding(0)
        lblGrupa.Name = "lblGrupa"
        lblGrupa.Size = New System.Drawing.Size(110, 34)
        lblGrupa.TabIndex = 0
        lblGrupa.Text = "Grupa"
        lblGrupa.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        cboGrupa.Dock = System.Windows.Forms.DockStyle.Fill
        cboGrupa.Enabled = False
        cboGrupa.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cboGrupa.Location = New System.Drawing.Point(110, 0)
        cboGrupa.Margin = New System.Windows.Forms.Padding(0)
        cboGrupa.Name = "cboGrupa"
        cboGrupa.Size = New System.Drawing.Size(290, 34)
        cboGrupa.TabIndex = 1
        tips.SetToolTipHeader(cboGrupa, "Grupa")
        tips.SetToolTipText(cboGrupa, "Grupele care au avut prezență în luna aleasă. Pentru situația lunară și situația de plată; nu e nevoie dacă bifezi «Toate grupele».")
        '
        ' rowGrupa
        '
        rowGrupa.Controls.Add(cboGrupa)
        rowGrupa.Controls.Add(lblGrupa)
        rowGrupa.Dock = System.Windows.Forms.DockStyle.Top
        rowGrupa.Location = New System.Drawing.Point(0, 102)
        rowGrupa.Margin = New System.Windows.Forms.Padding(0)
        rowGrupa.Name = "rowGrupa"
        rowGrupa.Size = New System.Drawing.Size(400, 34)
        rowGrupa.TabIndex = 9
        '
        ' chkToate / chkSepara
        '
        chkToate.AutoSize = False
        chkToate.Dock = System.Windows.Forms.DockStyle.Left
        chkToate.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        chkToate.Location = New System.Drawing.Point(0, 0)
        chkToate.Margin = New System.Windows.Forms.Padding(0)
        chkToate.Name = "chkToate"
        chkToate.Size = New System.Drawing.Size(180, 30)
        chkToate.TabIndex = 0
        chkToate.Text = "Toate grupele"
        tips.SetToolTipHeader(chkToate, "Toate grupele")
        tips.SetToolTipText(chkToate, "Raportul se face pentru toate grupele lunii, nu doar pentru cea aleasă.")
        chkSepara.AutoSize = False
        chkSepara.Dock = System.Windows.Forms.DockStyle.Fill
        chkSepara.Enabled = False
        chkSepara.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        chkSepara.Location = New System.Drawing.Point(180, 0)
        chkSepara.Margin = New System.Windows.Forms.Padding(0)
        chkSepara.Name = "chkSepara"
        chkSepara.Size = New System.Drawing.Size(220, 30)
        chkSepara.TabIndex = 1
        chkSepara.Text = "Separă grupele"
        tips.SetToolTipHeader(chkSepara, "Separă grupele")
        tips.SetToolTipText(chkSepara, "La situația lunară pe toate grupele: fiecare grupă pe pagină nouă.")
        '
        ' rowToate
        '
        rowToate.Controls.Add(chkSepara)
        rowToate.Controls.Add(chkToate)
        rowToate.Dock = System.Windows.Forms.DockStyle.Top
        rowToate.Location = New System.Drawing.Point(0, 136)
        rowToate.Margin = New System.Windows.Forms.Padding(0)
        rowToate.Name = "rowToate"
        rowToate.Size = New System.Drawing.Size(400, 30)
        rowToate.TabIndex = 8
        '
        ' btnSituatie / btnPlata
        '
        btnSituatie.Dock = System.Windows.Forms.DockStyle.Left
        btnSituatie.Enabled = False
        btnSituatie.Location = New System.Drawing.Point(0, 4)
        btnSituatie.Margin = New System.Windows.Forms.Padding(0)
        btnSituatie.Name = "btnSituatie"
        btnSituatie.Size = New System.Drawing.Size(196, 42)
        btnSituatie.TabIndex = 0
        btnSituatie.Text = "Situație lunară"
        tips.SetToolTipHeader(btnSituatie, "Situație lunară")
        tips.SetToolTipText(btnSituatie, "Situația debitorilor pe luna și grupa alese (sau pe toate grupele).")
        btnPlata.Dock = System.Windows.Forms.DockStyle.Right
        btnPlata.Enabled = False
        btnPlata.Location = New System.Drawing.Point(204, 4)
        btnPlata.Margin = New System.Windows.Forms.Padding(0)
        btnPlata.Name = "btnPlata"
        btnPlata.Size = New System.Drawing.Size(196, 42)
        btnPlata.TabIndex = 1
        btnPlata.Text = "Situație de plată"
        tips.SetToolTipHeader(btnPlata, "Situație de plată")
        tips.SetToolTipText(btnPlata, "Solduri inițiale și taxa lunară integrală. Pe toate grupele, fiecare grupă este pe pagină nouă.")
        '
        ' rowRap1
        '
        rowRap1.Controls.Add(btnPlata)
        rowRap1.Controls.Add(btnSituatie)
        rowRap1.Dock = System.Windows.Forms.DockStyle.Top
        rowRap1.Location = New System.Drawing.Point(0, 166)
        rowRap1.Margin = New System.Windows.Forms.Padding(0)
        rowRap1.Name = "rowRap1"
        rowRap1.Padding = New System.Windows.Forms.Padding(0, 4, 0, 4)
        rowRap1.Size = New System.Drawing.Size(400, 50)
        rowRap1.TabIndex = 7
        '
        ' lblDeLa / dtpDeLa / lblPana / dtpPana: the interval of the cash and bank reports
        '
        lblDeLa.Dock = System.Windows.Forms.DockStyle.Left
        lblDeLa.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblDeLa.Location = New System.Drawing.Point(0, 0)
        lblDeLa.Margin = New System.Windows.Forms.Padding(0)
        lblDeLa.Name = "lblDeLa"
        lblDeLa.Size = New System.Drawing.Size(50, 34)
        lblDeLa.TabIndex = 0
        lblDeLa.Text = "De la"
        lblDeLa.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        dtpDeLa.Dock = System.Windows.Forms.DockStyle.Left
        dtpDeLa.GlyphImage = My.Resources.Resources.calendar
        dtpDeLa.Location = New System.Drawing.Point(50, 0)
        dtpDeLa.Margin = New System.Windows.Forms.Padding(0)
        dtpDeLa.Name = "dtpDeLa"
        dtpDeLa.PlaceholderText = "_"
        dtpDeLa.Size = New System.Drawing.Size(140, 34)
        dtpDeLa.TabIndex = 1
        lblPana.Dock = System.Windows.Forms.DockStyle.Left
        lblPana.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblPana.Location = New System.Drawing.Point(190, 0)
        lblPana.Margin = New System.Windows.Forms.Padding(0)
        lblPana.Name = "lblPana"
        lblPana.Padding = New System.Windows.Forms.Padding(8, 0, 0, 0)
        lblPana.Size = New System.Drawing.Size(70, 34)
        lblPana.TabIndex = 2
        lblPana.Text = "Până la"
        lblPana.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        dtpPana.Dock = System.Windows.Forms.DockStyle.Fill
        dtpPana.GlyphImage = My.Resources.Resources.calendar
        dtpPana.Location = New System.Drawing.Point(260, 0)
        dtpPana.Margin = New System.Windows.Forms.Padding(0)
        dtpPana.Name = "dtpPana"
        dtpPana.PlaceholderText = "_"
        dtpPana.Size = New System.Drawing.Size(140, 34)
        dtpPana.TabIndex = 3
        '
        ' rowInterval
        '
        rowInterval.Controls.Add(dtpPana)
        rowInterval.Controls.Add(lblPana)
        rowInterval.Controls.Add(dtpDeLa)
        rowInterval.Controls.Add(lblDeLa)
        rowInterval.Dock = System.Windows.Forms.DockStyle.Top
        rowInterval.Location = New System.Drawing.Point(0, 216)
        rowInterval.Margin = New System.Windows.Forms.Padding(0)
        rowInterval.Name = "rowInterval"
        rowInterval.Size = New System.Drawing.Size(400, 34)
        rowInterval.TabIndex = 6
        '
        ' btnCasa / btnBanca
        '
        btnCasa.Dock = System.Windows.Forms.DockStyle.Left
        btnCasa.Enabled = False
        btnCasa.Location = New System.Drawing.Point(0, 4)
        btnCasa.Margin = New System.Windows.Forms.Padding(0)
        btnCasa.Name = "btnCasa"
        btnCasa.Size = New System.Drawing.Size(196, 42)
        btnCasa.TabIndex = 0
        btnCasa.Text = "Registru casă"
        tips.SetToolTipHeader(btnCasa, "Registru de casă")
        tips.SetToolTipText(btnCasa, "Registrul de casă pe intervalul ales.")
        btnBanca.Dock = System.Windows.Forms.DockStyle.Right
        btnBanca.Enabled = False
        btnBanca.Location = New System.Drawing.Point(204, 4)
        btnBanca.Margin = New System.Windows.Forms.Padding(0)
        btnBanca.Name = "btnBanca"
        btnBanca.Size = New System.Drawing.Size(196, 42)
        btnBanca.TabIndex = 1
        btnBanca.Text = "Raport bancă"
        tips.SetToolTipHeader(btnBanca, "Raport bancă")
        tips.SetToolTipText(btnBanca, "Încasările din extrasele de bancă, pe intervalul ales.")
        '
        ' rowRap2
        '
        rowRap2.Controls.Add(btnBanca)
        rowRap2.Controls.Add(btnCasa)
        rowRap2.Dock = System.Windows.Forms.DockStyle.Top
        rowRap2.Location = New System.Drawing.Point(0, 250)
        rowRap2.Margin = New System.Windows.Forms.Padding(0)
        rowRap2.Name = "rowRap2"
        rowRap2.Padding = New System.Windows.Forms.Padding(0, 4, 0, 4)
        rowRap2.Size = New System.Drawing.Size(400, 50)
        rowRap2.TabIndex = 5
        '
        ' tmrCauta: the child is looked for once the operator stops typing (one request, not one per key)
        '
        tmrCauta.Interval = 700
        '
        ' lblCauta / txtCauta: the child is looked for by name or CNP, three characters at least
        '
        lblCauta.Dock = System.Windows.Forms.DockStyle.Left
        lblCauta.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblCauta.Location = New System.Drawing.Point(0, 0)
        lblCauta.Margin = New System.Windows.Forms.Padding(0)
        lblCauta.Name = "lblCauta"
        lblCauta.Size = New System.Drawing.Size(110, 34)
        lblCauta.TabIndex = 0
        lblCauta.Text = "Caută copil"
        lblCauta.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        txtCauta.Dock = System.Windows.Forms.DockStyle.Fill
        txtCauta.Enabled = False
        txtCauta.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        txtCauta.Location = New System.Drawing.Point(110, 0)
        txtCauta.Margin = New System.Windows.Forms.Padding(0)
        txtCauta.MaxLength = 100
        txtCauta.Name = "txtCauta"
        txtCauta.PlaceholderText = "nume sau CNP, minim 3 caractere"
        txtCauta.Size = New System.Drawing.Size(290, 34)
        txtCauta.TabIndex = 1
        tips.SetToolTipHeader(txtCauta, "Caută copil")
        tips.SetToolTipText(txtCauta, "Scrie cel puțin 3 caractere din numele sau CNP-ul copilului, în luna aleasă. Căutarea pornește singură după o scurtă pauză de tastare (sau imediat, cu Enter). Rezultatele apar în lista de mai jos.")
        '
        ' rowCauta
        '
        rowCauta.Controls.Add(txtCauta)
        rowCauta.Controls.Add(lblCauta)
        rowCauta.Dock = System.Windows.Forms.DockStyle.Top
        rowCauta.Location = New System.Drawing.Point(0, 300)
        rowCauta.Margin = New System.Windows.Forms.Padding(0)
        rowCauta.Name = "rowCauta"
        rowCauta.Size = New System.Drawing.Size(400, 34)
        rowCauta.TabIndex = 4
        '
        ' lblCopil / cboCopil: what the search found
        '
        lblCopil.Dock = System.Windows.Forms.DockStyle.Left
        lblCopil.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblCopil.Location = New System.Drawing.Point(0, 0)
        lblCopil.Margin = New System.Windows.Forms.Padding(0)
        lblCopil.Name = "lblCopil"
        lblCopil.Size = New System.Drawing.Size(110, 34)
        lblCopil.TabIndex = 0
        lblCopil.Text = "Copil"
        lblCopil.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        cboCopil.Dock = System.Windows.Forms.DockStyle.Fill
        cboCopil.Enabled = False
        cboCopil.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cboCopil.Location = New System.Drawing.Point(110, 0)
        cboCopil.Margin = New System.Windows.Forms.Padding(0)
        cboCopil.Name = "cboCopil"
        cboCopil.Size = New System.Drawing.Size(290, 34)
        cboCopil.TabIndex = 1
        '
        ' rowCopil
        '
        rowCopil.Controls.Add(cboCopil)
        rowCopil.Controls.Add(lblCopil)
        rowCopil.Dock = System.Windows.Forms.DockStyle.Top
        rowCopil.Location = New System.Drawing.Point(0, 334)
        rowCopil.Margin = New System.Windows.Forms.Padding(0)
        rowCopil.Name = "rowCopil"
        rowCopil.Size = New System.Drawing.Size(400, 34)
        rowCopil.TabIndex = 3
        '
        ' chkToti
        '
        chkToti.AutoSize = False
        chkToti.Dock = System.Windows.Forms.DockStyle.Fill
        chkToti.Enabled = False
        chkToti.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        chkToti.Location = New System.Drawing.Point(0, 0)
        chkToti.Margin = New System.Windows.Forms.Padding(0)
        chkToti.Name = "chkToti"
        chkToti.Size = New System.Drawing.Size(400, 30)
        chkToti.TabIndex = 0
        chkToti.Text = "Toți copiii din grupa aleasă"
        tips.SetToolTipHeader(chkToti, "Toți copiii din grupă")
        tips.SetToolTipText(chkToti, "Fișa de cont se face pentru fiecare copil al grupei alese, nu doar pentru cel din listă.")
        '
        ' rowToti
        '
        rowToti.Controls.Add(chkToti)
        rowToti.Dock = System.Windows.Forms.DockStyle.Top
        rowToti.Location = New System.Drawing.Point(0, 368)
        rowToti.Margin = New System.Windows.Forms.Padding(0)
        rowToti.Name = "rowToti"
        rowToti.Size = New System.Drawing.Size(400, 30)
        rowToti.TabIndex = 2
        '
        ' btnFisa
        '
        btnFisa.Dock = System.Windows.Forms.DockStyle.Fill
        btnFisa.Enabled = False
        btnFisa.Location = New System.Drawing.Point(0, 4)
        btnFisa.Margin = New System.Windows.Forms.Padding(0)
        btnFisa.Name = "btnFisa"
        btnFisa.Size = New System.Drawing.Size(400, 42)
        btnFisa.TabIndex = 0
        btnFisa.Text = "Fișă cont"
        tips.SetToolTipHeader(btnFisa, "Fișă cont")
        tips.SetToolTipText(btnFisa, "Fișa de cont a copilului ales sau a tuturor copiilor din grupă.")
        '
        ' rowFisa
        '
        rowFisa.Controls.Add(btnFisa)
        rowFisa.Dock = System.Windows.Forms.DockStyle.Top
        rowFisa.Location = New System.Drawing.Point(0, 398)
        rowFisa.Margin = New System.Windows.Forms.Padding(0)
        rowFisa.Name = "rowFisa"
        rowFisa.Padding = New System.Windows.Forms.Padding(0, 4, 0, 4)
        rowFisa.Size = New System.Drawing.Size(400, 50)
        rowFisa.TabIndex = 1
        '
        ' btnAnulate
        '
        btnAnulate.Dock = System.Windows.Forms.DockStyle.Fill
        btnAnulate.Enabled = False
        btnAnulate.Location = New System.Drawing.Point(0, 4)
        btnAnulate.Margin = New System.Windows.Forms.Padding(0)
        btnAnulate.Name = "btnAnulate"
        btnAnulate.Size = New System.Drawing.Size(400, 42)
        btnAnulate.TabIndex = 0
        btnAnulate.Text = "Documente anulate"
        tips.SetToolTipHeader(btnAnulate, "Documente anulate")
        tips.SetToolTipText(btnAnulate, "Documentele anulate în luna aleasă.")
        '
        ' rowAnulate
        '
        rowAnulate.Controls.Add(btnAnulate)
        rowAnulate.Dock = System.Windows.Forms.DockStyle.Top
        rowAnulate.Location = New System.Drawing.Point(0, 448)
        rowAnulate.Margin = New System.Windows.Forms.Padding(0)
        rowAnulate.Name = "rowAnulate"
        rowAnulate.Padding = New System.Windows.Forms.Padding(0, 4, 0, 4)
        rowAnulate.Size = New System.Drawing.Size(400, 50)
        rowAnulate.TabIndex = 0
        '
        ' btnExcel: the report on screen as an Excel file
        '
        btnExcel.Dock = System.Windows.Forms.DockStyle.Fill
        btnExcel.Enabled = False
        btnExcel.Location = New System.Drawing.Point(0, 4)
        btnExcel.Margin = New System.Windows.Forms.Padding(0)
        btnExcel.Name = "btnExcel"
        btnExcel.Size = New System.Drawing.Size(400, 42)
        btnExcel.TabIndex = 0
        btnExcel.Text = "Salvează Excel"
        tips.SetToolTipHeader(btnExcel, "Salvează Excel")
        tips.SetToolTipText(btnExcel, "Salvează raportul de pe ecran ca fișier Excel, cu toate coloanele. Funcționează doar cât timp raportul este deschis aici.")
        '
        ' rowExcel
        '
        rowExcel.Controls.Add(btnExcel)
        rowExcel.Dock = System.Windows.Forms.DockStyle.Bottom
        rowExcel.Location = New System.Drawing.Point(0, 590)
        rowExcel.Margin = New System.Windows.Forms.Padding(0)
        rowExcel.Name = "rowExcel"
        rowExcel.Padding = New System.Windows.Forms.Padding(0, 4, 0, 4)
        rowExcel.Size = New System.Drawing.Size(400, 50)
        rowExcel.TabIndex = 13
        '
        ' pnlParam: the Dock.Top rows are added bottom row first, so the last one added (rowSub) is on top
        '
        pnlParam.Controls.Add(rowAnulate)
        pnlParam.Controls.Add(rowFisa)
        pnlParam.Controls.Add(rowToti)
        pnlParam.Controls.Add(rowCopil)
        pnlParam.Controls.Add(rowCauta)
        pnlParam.Controls.Add(rowRap2)
        pnlParam.Controls.Add(rowInterval)
        pnlParam.Controls.Add(rowRap1)
        pnlParam.Controls.Add(rowToate)
        pnlParam.Controls.Add(rowGrupa)
        pnlParam.Controls.Add(rowLuna)
        pnlParam.Controls.Add(rowAn)
        pnlParam.Controls.Add(rowSub)
        pnlParam.Controls.Add(rowExcel)
        pnlParam.Dock = System.Windows.Forms.DockStyle.Left
        pnlParam.Location = New System.Drawing.Point(12, 9)
        pnlParam.Margin = New System.Windows.Forms.Padding(0)
        pnlParam.Name = "pnlParam"
        pnlParam.Size = New System.Drawing.Size(400, 670)
        pnlParam.TabIndex = 1
        '
        ' previewPdf: the report as a PDF (the embedded Adobe surface, as the other pages that show a PDF)
        '
        previewPdf.Dock = System.Windows.Forms.DockStyle.Fill
        previewPdf.Location = New System.Drawing.Point(0, 0)
        previewPdf.Margin = New System.Windows.Forms.Padding(0)
        previewPdf.Name = "previewPdf"
        previewPdf.Size = New System.Drawing.Size(758, 640)
        previewPdf.TabIndex = 0
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = System.Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 640)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(0)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(758, 30)
        ntfMesaj.TabIndex = 1
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' pnlRaport
        '
        pnlRaport.Controls.Add(previewPdf)
        pnlRaport.Controls.Add(ntfMesaj)
        pnlRaport.Dock = System.Windows.Forms.DockStyle.Fill
        pnlRaport.Location = New System.Drawing.Point(412, 9)
        pnlRaport.Margin = New System.Windows.Forms.Padding(0)
        pnlRaport.Name = "pnlRaport"
        pnlRaport.Padding = New System.Windows.Forms.Padding(12, 0, 0, 0)
        pnlRaport.Size = New System.Drawing.Size(770, 670)
        pnlRaport.TabIndex = 0
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(pnlRaport)
        pnlCard.Controls.Add(pnlParam)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(3, 69)
        pnlCard.Margin = New System.Windows.Forms.Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New System.Windows.Forms.Padding(12, 9, 12, 9)
        pnlCard.Size = New System.Drawing.Size(1194, 688)
        pnlCard.TabIndex = 1
        pnlCard.Tag = "Card"
        '
        ' AdechitRapoarteForm
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        ClientSize = New System.Drawing.Size(1200, 760)
        Controls.Add(pnlCard)
        Controls.Add(busyBar)
        Controls.Add(capBar)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(1100, 700)
        Name = "AdechitRapoarteForm"
        Padding = New System.Windows.Forms.Padding(3)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "ADECHIT — Rapoarte"
        pnlCard.ResumeLayout(False)
        pnlParam.ResumeLayout(False)
        rowExcel.ResumeLayout(False)
        rowAnulate.ResumeLayout(False)
        rowFisa.ResumeLayout(False)
        rowToti.ResumeLayout(False)
        rowCopil.ResumeLayout(False)
        rowCauta.ResumeLayout(False)
        rowRap2.ResumeLayout(False)
        rowInterval.ResumeLayout(False)
        rowRap1.ResumeLayout(False)
        rowToate.ResumeLayout(False)
        rowGrupa.ResumeLayout(False)
        rowLuna.ResumeLayout(False)
        rowAn.ResumeLayout(False)
        rowSub.ResumeLayout(False)
        pnlRaport.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tmrCauta As System.Windows.Forms.Timer
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents busyBar As KBotBusyBar
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents pnlParam As System.Windows.Forms.Panel
    Friend WithEvents rowExcel As System.Windows.Forms.Panel
    Friend WithEvents btnExcel As KBotButton
    Friend WithEvents rowAnulate As System.Windows.Forms.Panel
    Friend WithEvents btnAnulate As KBotButton
    Friend WithEvents rowFisa As System.Windows.Forms.Panel
    Friend WithEvents btnFisa As KBotButton
    Friend WithEvents rowToti As System.Windows.Forms.Panel
    Friend WithEvents chkToti As KBotCheckBox
    Friend WithEvents rowCopil As System.Windows.Forms.Panel
    Friend WithEvents cboCopil As KBotComboBox
    Friend WithEvents lblCopil As System.Windows.Forms.Label
    Friend WithEvents rowCauta As System.Windows.Forms.Panel
    Friend WithEvents txtCauta As KBotTextBox
    Friend WithEvents lblCauta As System.Windows.Forms.Label
    Friend WithEvents rowRap2 As System.Windows.Forms.Panel
    Friend WithEvents btnBanca As KBotButton
    Friend WithEvents btnCasa As KBotButton
    Friend WithEvents rowInterval As System.Windows.Forms.Panel
    Friend WithEvents dtpPana As KBotDatePicker
    Friend WithEvents lblPana As System.Windows.Forms.Label
    Friend WithEvents dtpDeLa As KBotDatePicker
    Friend WithEvents lblDeLa As System.Windows.Forms.Label
    Friend WithEvents rowRap1 As System.Windows.Forms.Panel
    Friend WithEvents btnPlata As KBotButton
    Friend WithEvents btnSituatie As KBotButton
    Friend WithEvents rowToate As System.Windows.Forms.Panel
    Friend WithEvents chkSepara As KBotCheckBox
    Friend WithEvents chkToate As KBotCheckBox
    Friend WithEvents rowGrupa As System.Windows.Forms.Panel
    Friend WithEvents cboGrupa As KBotComboBox
    Friend WithEvents lblGrupa As System.Windows.Forms.Label
    Friend WithEvents rowLuna As System.Windows.Forms.Panel
    Friend WithEvents cboLuna As KBotComboBox
    Friend WithEvents lblLuna As System.Windows.Forms.Label
    Friend WithEvents rowAn As System.Windows.Forms.Panel
    Friend WithEvents cboAn As KBotComboBox
    Friend WithEvents lblAn As System.Windows.Forms.Label
    Friend WithEvents rowSub As System.Windows.Forms.Panel
    Friend WithEvents cboSub As KBotComboBox
    Friend WithEvents lblSub As System.Windows.Forms.Label
    Friend WithEvents pnlRaport As System.Windows.Forms.Panel
    Friend WithEvents previewPdf As ReaderHostPreview
    Friend WithEvents ntfMesaj As KBotNotice
End Class
