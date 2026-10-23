Imports KBot.Controls

' Slice 0123-02 -- the page «VERCON Rapoarte». All the controls are declared HERE (docs/kbot-forms-ui-convention.md).
' Three columns: the choices of the report (subunit, gestiune, year, period, options, the report types), the data of the report (the selection
' tree with a box on each row, filled on demand), and the report as a PDF in the embedded Adobe surface with the notice line under it. Rows are
' Dock.Top panels with a fixed height, added bottom row first (the last one added is on top); children are added in reverse dock order
' (Fill first, then the edges).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VerconRapoarteForm
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
        tmrLookups = New System.Windows.Forms.Timer(components)
        capBar = New KBotCaptionBar()
        busyBar = New KBotBusyBar()
        pnlCard = New System.Windows.Forms.Panel()
        pnlParam = New System.Windows.Forms.Panel()
        pnlSel = New System.Windows.Forms.Panel()
        pnlRaport = New System.Windows.Forms.Panel()
        rowSub = New System.Windows.Forms.Panel()
        lblSub = New System.Windows.Forms.Label()
        cboSub = New KBotComboBox()
        rowGest = New System.Windows.Forms.Panel()
        lblGest = New System.Windows.Forms.Label()
        cboGest = New KBotComboBox()
        rowAn = New System.Windows.Forms.Panel()
        lblAn = New System.Windows.Forms.Label()
        cboAn = New KBotComboBox()
        lblPerTitlu = New System.Windows.Forms.Label()
        rowPer = New System.Windows.Forms.Panel()
        lblPer = New System.Windows.Forms.Label()
        cboPer = New KBotComboBox()
        rowDeLa = New System.Windows.Forms.Panel()
        lblDeLa = New System.Windows.Forms.Label()
        dtpDeLa = New KBotDatePicker()
        rowPana = New System.Windows.Forms.Panel()
        lblPana = New System.Windows.Forms.Label()
        dtpPana = New KBotDatePicker()
        rowGrupeaza = New System.Windows.Forms.Panel()
        chkGrupeaza = New KBotCheckBox()
        rowFara = New System.Windows.Forms.Panel()
        chkFara = New KBotCheckBox()
        rowDocumente = New System.Windows.Forms.Panel()
        chkDocumente = New KBotCheckBox()
        rowPrimitori = New System.Windows.Forms.Panel()
        chkPrimitori = New KBotCheckBox()
        rowGrupPrim = New System.Windows.Forms.Panel()
        chkGrupPrim = New KBotCheckBox()
        treeRapoarte = New AdvancedTreeControl()
        treeSel = New AdvancedTreeControl()
        rowSelHead = New System.Windows.Forms.Panel()
        chkToate = New KBotCheckBox()
        lblStare = New System.Windows.Forms.Label()
        rowAct = New System.Windows.Forms.Panel()
        btnListare = New KBotButton()
        btnExcel = New KBotButton()
        previewPdf = New ReaderHostPreview()
        ntfMesaj = New KBotNotice()
        pnlCard.SuspendLayout()
        pnlParam.SuspendLayout()
        pnlSel.SuspendLayout()
        pnlRaport.SuspendLayout()
        rowSelHead.SuspendLayout()
        rowAct.SuspendLayout()
        rowSub.SuspendLayout()
        rowGest.SuspendLayout()
        rowAn.SuspendLayout()
        rowPer.SuspendLayout()
        rowDeLa.SuspendLayout()
        rowPana.SuspendLayout()
        rowGrupeaza.SuspendLayout()
        rowFara.SuspendLayout()
        rowDocumente.SuspendLayout()
        rowPrimitori.SuspendLayout()
        rowGrupPrim.SuspendLayout()
        SuspendLayout()
        '
        ' tmrLookups: the data of the report is asked for once the operator stops changing the dates (one request, not one per edit)
        '
        tmrLookups.Interval = 500
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
        capBar.Size = New System.Drawing.Size(1494, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "VERCON — Rapoarte"
        '
        ' busyBar: slides while anything is asked of the server, so the page never looks stuck
        '
        busyBar.Dock = System.Windows.Forms.DockStyle.Top
        busyBar.Location = New System.Drawing.Point(3, 69)
        busyBar.Margin = New System.Windows.Forms.Padding(0)
        busyBar.Name = "busyBar"
        busyBar.Size = New System.Drawing.Size(1494, 5)
        busyBar.TabIndex = 2
        busyBar.TabStop = False
        '
        ' lblSub / cboSub
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
        cboSub.Size = New System.Drawing.Size(230, 34)
        cboSub.TabIndex = 1
        tips.SetToolTipHeader(cboSub, "Subunitate")
        tips.SetToolTipText(cboSub, "Subunitatea pentru care se fac rapoartele. Apar doar cele la care ai acces.")
        '
        ' rowSub
        '
        rowSub.Controls.Add(cboSub)
        rowSub.Controls.Add(lblSub)
        rowSub.Dock = System.Windows.Forms.DockStyle.Top
        rowSub.Location = New System.Drawing.Point(0, 0)
        rowSub.Margin = New System.Windows.Forms.Padding(0)
        rowSub.Name = "rowSub"
        rowSub.Size = New System.Drawing.Size(340, 34)
        rowSub.TabIndex = 12

        '
        ' lblGest / cboGest
        '
        lblGest.Dock = System.Windows.Forms.DockStyle.Left
        lblGest.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblGest.Location = New System.Drawing.Point(0, 0)
        lblGest.Margin = New System.Windows.Forms.Padding(0)
        lblGest.Name = "lblGest"
        lblGest.Size = New System.Drawing.Size(110, 34)
        lblGest.TabIndex = 0
        lblGest.Text = "Gestiune"
        lblGest.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        cboGest.Dock = System.Windows.Forms.DockStyle.Fill
        cboGest.Enabled = False
        cboGest.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cboGest.Location = New System.Drawing.Point(110, 0)
        cboGest.Margin = New System.Windows.Forms.Padding(0)
        cboGest.Name = "cboGest"
        cboGest.Size = New System.Drawing.Size(230, 34)
        cboGest.TabIndex = 1
        tips.SetToolTipHeader(cboGest, "Gestiune")
        tips.SetToolTipText(cboGest, "Gestiunea subunității. Tipul ei (materiale, alimente, obiecte de inventar, mijloace fixe) hotărăște ce rapoarte există.")
        '
        ' rowGest
        '
        rowGest.Controls.Add(cboGest)
        rowGest.Controls.Add(lblGest)
        rowGest.Dock = System.Windows.Forms.DockStyle.Top
        rowGest.Location = New System.Drawing.Point(0, 0)
        rowGest.Margin = New System.Windows.Forms.Padding(0)
        rowGest.Name = "rowGest"
        rowGest.Size = New System.Drawing.Size(340, 34)
        rowGest.TabIndex = 11
        rowGest.Visible = False

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
        cboAn.Size = New System.Drawing.Size(230, 34)
        cboAn.TabIndex = 1
        tips.SetToolTipHeader(cboAn, "An")
        tips.SetToolTipText(cboAn, "Exercițiul gestiunii. La obiecte de inventar și mijloace fixe perioada se alege direct, fără an.")
        '
        ' rowAn
        '
        rowAn.Controls.Add(cboAn)
        rowAn.Controls.Add(lblAn)
        rowAn.Dock = System.Windows.Forms.DockStyle.Top
        rowAn.Location = New System.Drawing.Point(0, 0)
        rowAn.Margin = New System.Windows.Forms.Padding(0)
        rowAn.Name = "rowAn"
        rowAn.Size = New System.Drawing.Size(340, 34)
        rowAn.TabIndex = 10
        rowAn.Visible = False

        '
        ' lblPer / cboPer
        '
        lblPer.Dock = System.Windows.Forms.DockStyle.Left
        lblPer.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblPer.Location = New System.Drawing.Point(0, 0)
        lblPer.Margin = New System.Windows.Forms.Padding(0)
        lblPer.Name = "lblPer"
        lblPer.Size = New System.Drawing.Size(110, 34)
        lblPer.TabIndex = 0
        lblPer.Text = "Perioada"
        lblPer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        cboPer.Dock = System.Windows.Forms.DockStyle.Fill
        cboPer.Enabled = False
        cboPer.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cboPer.Location = New System.Drawing.Point(110, 0)
        cboPer.Margin = New System.Windows.Forms.Padding(0)
        cboPer.Name = "cboPer"
        cboPer.Size = New System.Drawing.Size(230, 34)
        cboPer.TabIndex = 1
        cboPer.Editable = True
        cboPer.FindAsYouType = True
        cboPer.LimitToList = False
        cboPer.PlaceholderText = "Tastează anul sau luna…"
        tips.SetToolTipHeader(cboPer, "Perioada")
        tips.SetToolTipText(cboPer, "Anul, tot anul sau o lună, cu cele mai noi sus. Alegerea schimbă datele de mai jos; dacă le schimbi tu, apare «Interval personalizat».")
        '
        ' rowPer
        '
        rowPer.Controls.Add(cboPer)
        rowPer.Controls.Add(lblPer)
        rowPer.Dock = System.Windows.Forms.DockStyle.Top
        rowPer.Location = New System.Drawing.Point(0, 0)
        rowPer.Margin = New System.Windows.Forms.Padding(0)
        rowPer.Name = "rowPer"
        rowPer.Size = New System.Drawing.Size(340, 34)
        rowPer.TabIndex = 8

        '
        ' lblDeLa / dtpDeLa
        '
        lblDeLa.Dock = System.Windows.Forms.DockStyle.Left
        lblDeLa.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblDeLa.Location = New System.Drawing.Point(0, 0)
        lblDeLa.Margin = New System.Windows.Forms.Padding(0)
        lblDeLa.Name = "lblDeLa"
        lblDeLa.Size = New System.Drawing.Size(110, 34)
        lblDeLa.TabIndex = 0
        lblDeLa.Text = "Dată început"
        lblDeLa.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        dtpDeLa.Dock = System.Windows.Forms.DockStyle.Fill
        dtpDeLa.Location = New System.Drawing.Point(110, 0)
        dtpDeLa.Margin = New System.Windows.Forms.Padding(0)
        dtpDeLa.Name = "dtpDeLa"
        dtpDeLa.Size = New System.Drawing.Size(230, 34)
        dtpDeLa.TabIndex = 1
        dtpDeLa.GlyphImage = My.Resources.Resources.calendar
        dtpDeLa.PlaceholderText = "_"
        '
        ' rowDeLa
        '
        rowDeLa.Controls.Add(dtpDeLa)
        rowDeLa.Controls.Add(lblDeLa)
        rowDeLa.Dock = System.Windows.Forms.DockStyle.Top
        rowDeLa.Location = New System.Drawing.Point(0, 0)
        rowDeLa.Margin = New System.Windows.Forms.Padding(0)
        rowDeLa.Name = "rowDeLa"
        rowDeLa.Size = New System.Drawing.Size(340, 34)
        rowDeLa.TabIndex = 7

        '
        ' lblPana / dtpPana
        '
        lblPana.Dock = System.Windows.Forms.DockStyle.Left
        lblPana.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblPana.Location = New System.Drawing.Point(0, 0)
        lblPana.Margin = New System.Windows.Forms.Padding(0)
        lblPana.Name = "lblPana"
        lblPana.Size = New System.Drawing.Size(110, 34)
        lblPana.TabIndex = 0
        lblPana.Text = "Dată sfârșit"
        lblPana.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        dtpPana.Dock = System.Windows.Forms.DockStyle.Fill
        dtpPana.Location = New System.Drawing.Point(110, 0)
        dtpPana.Margin = New System.Windows.Forms.Padding(0)
        dtpPana.Name = "dtpPana"
        dtpPana.Size = New System.Drawing.Size(230, 34)
        dtpPana.TabIndex = 1
        dtpPana.GlyphImage = My.Resources.Resources.calendar
        dtpPana.PlaceholderText = "_"
        '
        ' rowPana
        '
        rowPana.Controls.Add(dtpPana)
        rowPana.Controls.Add(lblPana)
        rowPana.Dock = System.Windows.Forms.DockStyle.Top
        rowPana.Location = New System.Drawing.Point(0, 0)
        rowPana.Margin = New System.Windows.Forms.Padding(0)
        rowPana.Name = "rowPana"
        rowPana.Size = New System.Drawing.Size(340, 34)
        rowPana.TabIndex = 6

        '
        ' chkGrupeaza
        '
        chkGrupeaza.AutoSize = False
        chkGrupeaza.Dock = System.Windows.Forms.DockStyle.Fill
        chkGrupeaza.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        chkGrupeaza.Location = New System.Drawing.Point(0, 0)
        chkGrupeaza.Margin = New System.Windows.Forms.Padding(0)
        chkGrupeaza.Name = "chkGrupeaza"
        chkGrupeaza.Size = New System.Drawing.Size(340, 30)
        chkGrupeaza.TabIndex = 0
        chkGrupeaza.Text = "Grupează pe cont, cu pagină nouă"
        tips.SetToolTipHeader(chkGrupeaza, "Grupează pe cont")
        tips.SetToolTipText(chkGrupeaza, "Balanța se desparte pe conturi, fiecare cont începe pe pagină nouă.")
        '
        ' rowGrupeaza
        '
        rowGrupeaza.Controls.Add(chkGrupeaza)
        rowGrupeaza.Dock = System.Windows.Forms.DockStyle.Top
        rowGrupeaza.Location = New System.Drawing.Point(0, 0)
        rowGrupeaza.Margin = New System.Windows.Forms.Padding(0)
        rowGrupeaza.Name = "rowGrupeaza"
        rowGrupeaza.Size = New System.Drawing.Size(340, 30)
        rowGrupeaza.TabIndex = 0
        rowGrupeaza.Visible = False

        '
        ' chkFara
        '
        chkFara.AutoSize = False
        chkFara.Dock = System.Windows.Forms.DockStyle.Fill
        chkFara.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        chkFara.Location = New System.Drawing.Point(0, 0)
        chkFara.Margin = New System.Windows.Forms.Padding(0)
        chkFara.Name = "chkFara"
        chkFara.Size = New System.Drawing.Size(340, 30)
        chkFara.TabIndex = 0
        chkFara.Text = "Afiseaza FARA miscari"
        tips.SetToolTipHeader(chkFara, "Afișează fără mișcări")
        tips.SetToolTipText(chkFara, "Include și produsele care nu au avut nicio mișcare în perioadă.")
        '
        ' rowFara
        '
        rowFara.Controls.Add(chkFara)
        rowFara.Dock = System.Windows.Forms.DockStyle.Top
        rowFara.Location = New System.Drawing.Point(0, 0)
        rowFara.Margin = New System.Windows.Forms.Padding(0)
        rowFara.Name = "rowFara"
        rowFara.Size = New System.Drawing.Size(340, 30)
        rowFara.TabIndex = 0
        rowFara.Visible = False

        '
        ' chkDocumente
        '
        chkDocumente.AutoSize = False
        chkDocumente.Dock = System.Windows.Forms.DockStyle.Fill
        chkDocumente.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        chkDocumente.Location = New System.Drawing.Point(0, 0)
        chkDocumente.Margin = New System.Windows.Forms.Padding(0)
        chkDocumente.Name = "chkDocumente"
        chkDocumente.Size = New System.Drawing.Size(340, 30)
        chkDocumente.TabIndex = 0
        chkDocumente.Text = "Afișează Documente"
        tips.SetToolTipHeader(chkDocumente, "Afișează documente")
        tips.SetToolTipText(chkDocumente, "Selecția datelor se face pe documente, nu pe produse.")
        '
        ' rowDocumente
        '
        rowDocumente.Controls.Add(chkDocumente)
        rowDocumente.Dock = System.Windows.Forms.DockStyle.Top
        rowDocumente.Location = New System.Drawing.Point(0, 0)
        rowDocumente.Margin = New System.Windows.Forms.Padding(0)
        rowDocumente.Name = "rowDocumente"
        rowDocumente.Size = New System.Drawing.Size(340, 30)
        rowDocumente.TabIndex = 0
        rowDocumente.Visible = False

        '
        ' chkPrimitori
        '
        chkPrimitori.AutoSize = False
        chkPrimitori.Dock = System.Windows.Forms.DockStyle.Fill
        chkPrimitori.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        chkPrimitori.Location = New System.Drawing.Point(0, 0)
        chkPrimitori.Margin = New System.Windows.Forms.Padding(0)
        chkPrimitori.Name = "chkPrimitori"
        chkPrimitori.Size = New System.Drawing.Size(340, 30)
        chkPrimitori.TabIndex = 0
        chkPrimitori.Text = "Afișează Primitori"
        tips.SetToolTipHeader(chkPrimitori, "Afișează primitori")
        tips.SetToolTipText(chkPrimitori, "Selecția datelor se face pe primitori.")
        '
        ' rowPrimitori
        '
        rowPrimitori.Controls.Add(chkPrimitori)
        rowPrimitori.Dock = System.Windows.Forms.DockStyle.Top
        rowPrimitori.Location = New System.Drawing.Point(0, 0)
        rowPrimitori.Margin = New System.Windows.Forms.Padding(0)
        rowPrimitori.Name = "rowPrimitori"
        rowPrimitori.Size = New System.Drawing.Size(340, 30)
        rowPrimitori.TabIndex = 0
        rowPrimitori.Visible = False

        '
        ' chkGrupPrim
        '
        chkGrupPrim.AutoSize = False
        chkGrupPrim.Dock = System.Windows.Forms.DockStyle.Fill
        chkGrupPrim.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        chkGrupPrim.Location = New System.Drawing.Point(0, 0)
        chkGrupPrim.Margin = New System.Windows.Forms.Padding(0)
        chkGrupPrim.Name = "chkGrupPrim"
        chkGrupPrim.Size = New System.Drawing.Size(340, 30)
        chkGrupPrim.TabIndex = 0
        chkGrupPrim.Text = "Grupează după Primitori"
        tips.SetToolTipHeader(chkGrupPrim, "Grupează după primitori")
        tips.SetToolTipText(chkGrupPrim, "Amortizarea se grupează pe primitori.")
        '
        ' rowGrupPrim
        '
        rowGrupPrim.Controls.Add(chkGrupPrim)
        rowGrupPrim.Dock = System.Windows.Forms.DockStyle.Top
        rowGrupPrim.Location = New System.Drawing.Point(0, 0)
        rowGrupPrim.Margin = New System.Windows.Forms.Padding(0)
        rowGrupPrim.Name = "rowGrupPrim"
        rowGrupPrim.Size = New System.Drawing.Size(340, 30)
        rowGrupPrim.TabIndex = 0
        rowGrupPrim.Visible = False

        '
        ' treeRapoarte: the reports of the gestiune, by group; a press on a report chooses it
        '
        treeRapoarte.Dock = System.Windows.Forms.DockStyle.Fill
        treeRapoarte.ExpanderSize = 10
        treeRapoarte.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        treeRapoarte.HasNodeIcons = False
        treeRapoarte.HeaderCaption = " TIPUL RAPORTULUI"
        treeRapoarte.HeaderFont = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CByte(0))
        treeRapoarte.HeaderHeight = 30
        treeRapoarte.HeaderVisible = True
        treeRapoarte.Indent = 8
        treeRapoarte.ItemHeight = 24
        treeRapoarte.Location = New System.Drawing.Point(0, 0)
        treeRapoarte.Margin = New System.Windows.Forms.Padding(0)
        treeRapoarte.Name = "treeRapoarte"
        treeRapoarte.PaddingExpanderGap = 8
        treeRapoarte.PaddingIconGap = 8
        treeRapoarte.PaddingTreeEnd = 8
        treeRapoarte.PaddingTreeStart = 8
        treeRapoarte.Size = New System.Drawing.Size(340, 300)
        treeRapoarte.TabIndex = 0
        tips.SetToolTipHeader(treeRapoarte, "Tipul raportului")
        tips.SetToolTipText(treeRapoarte, "Grupele de rapoarte ale gestiunii. Alege un raport; apoi alege datele din coloana următoare.")

        '
        ' treeSel: the data of the report: a box on each row, the branches are read from the server when they are opened
        '
        treeSel.CheckBoxes = True
        treeSel.Dock = System.Windows.Forms.DockStyle.Fill
        treeSel.ExpanderSize = 10
        treeSel.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        treeSel.HasNodeIcons = False
        treeSel.HeaderCaption = " SELECTARE DATE"
        treeSel.HeaderFont = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CByte(0))
        treeSel.HeaderHeight = 30
        treeSel.HeaderIconSize = New System.Drawing.Size(18, 18)
        treeSel.HeaderSearchIcon = My.Resources.Resources.Everaldo_Crystal_Clear_App_xmag_search_48
        treeSel.HeaderSearchIconTooltip = "Deschide banda de căutare peste arbore." & vbLf & "ESC golește căutarea și o închide."
        treeSel.HeaderVisible = True
        treeSel.Indent = 8
        treeSel.ItemHeight = 24
        treeSel.Location = New System.Drawing.Point(0, 0)
        treeSel.Margin = New System.Windows.Forms.Padding(0)
        treeSel.Name = "treeSel"
        treeSel.PaddingExpanderGap = 8
        treeSel.PaddingIconGap = 8
        treeSel.PaddingTreeEnd = 8
        treeSel.PaddingTreeStart = 8
        treeSel.SearchBarFont = New System.Drawing.Font("Calibri", 9.0!)
        treeSel.SearchClearButton = True
        treeSel.SearchDefaultText = "... minim 3 caractere ..."
        treeSel.Size = New System.Drawing.Size(340, 300)
        treeSel.TabIndex = 0
        tips.SetToolTipHeader(treeSel, "Selectare date")
        tips.SetToolTipText(treeSel, "Datele raportului. Deschide o ramură ca să o citești de pe server, bifează ce vrei în raport. Fără nicio bifă se folosesc toate datele. Lupa din antet caută în tot arborele, minim 3 caractere.")

        '
        ' lblPerTitlu: the heading of the period rows
        '
        lblPerTitlu.Dock = System.Windows.Forms.DockStyle.Top
        lblPerTitlu.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblPerTitlu.Location = New System.Drawing.Point(0, 0)
        lblPerTitlu.Margin = New System.Windows.Forms.Padding(0)
        lblPerTitlu.Name = "lblPerTitlu"
        lblPerTitlu.Size = New System.Drawing.Size(340, 30)
        lblPerTitlu.TabIndex = 9
        lblPerTitlu.Text = "Selectează perioada"
        lblPerTitlu.TextAlign = System.Drawing.ContentAlignment.BottomLeft
        '
        ' pnlParam: the Dock.Top rows are added bottom row first, so the last one added (rowSub) is on top
        '
        pnlParam.Controls.Add(treeRapoarte)
        pnlParam.Controls.Add(rowGrupPrim)
        pnlParam.Controls.Add(rowPrimitori)
        pnlParam.Controls.Add(rowDocumente)
        pnlParam.Controls.Add(rowFara)
        pnlParam.Controls.Add(rowGrupeaza)
        pnlParam.Controls.Add(rowPana)
        pnlParam.Controls.Add(rowDeLa)
        pnlParam.Controls.Add(rowPer)
        pnlParam.Controls.Add(lblPerTitlu)
        pnlParam.Controls.Add(rowAn)
        pnlParam.Controls.Add(rowGest)
        pnlParam.Controls.Add(rowSub)
        pnlParam.Dock = System.Windows.Forms.DockStyle.Left
        pnlParam.Location = New System.Drawing.Point(12, 9)
        pnlParam.Margin = New System.Windows.Forms.Padding(0)
        pnlParam.Name = "pnlParam"
        pnlParam.Padding = New System.Windows.Forms.Padding(0, 0, 12, 0)
        pnlParam.Size = New System.Drawing.Size(352, 670)
        pnlParam.TabIndex = 2
        '
        ' chkToate: ticks / clears every row of the selection tree (reads the branches not loaded yet first)
        '
        chkToate.AutoSize = False
        chkToate.Dock = System.Windows.Forms.DockStyle.Fill
        chkToate.Enabled = False
        chkToate.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        chkToate.Location = New System.Drawing.Point(0, 0)
        chkToate.Margin = New System.Windows.Forms.Padding(0)
        chkToate.Name = "chkToate"
        chkToate.Size = New System.Drawing.Size(340, 30)
        chkToate.TabIndex = 0
        chkToate.Text = "Selectează TOATE"
        tips.SetToolTipHeader(chkToate, "Selectează TOATE")
        tips.SetToolTipText(chkToate, "Bifează toate datele raportului, inclusiv ramurile încă neîncărcate. Fără nicio bifă se folosesc toate datele.")
        '
        ' rowSelHead
        '
        rowSelHead.Controls.Add(chkToate)
        rowSelHead.Dock = System.Windows.Forms.DockStyle.Top
        rowSelHead.Location = New System.Drawing.Point(0, 0)
        rowSelHead.Margin = New System.Windows.Forms.Padding(0)
        rowSelHead.Name = "rowSelHead"
        rowSelHead.Size = New System.Drawing.Size(340, 34)
        rowSelHead.TabIndex = 3
        rowSelHead.Padding = New System.Windows.Forms.Padding(0, 4, 0, 0)
        '
        ' lblStare: why the report cannot be asked for yet
        '
        lblStare.Dock = System.Windows.Forms.DockStyle.Bottom
        lblStare.Font = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblStare.Location = New System.Drawing.Point(0, 0)
        lblStare.Margin = New System.Windows.Forms.Padding(0)
        lblStare.Name = "lblStare"
        lblStare.Size = New System.Drawing.Size(340, 40)
        lblStare.TabIndex = 2
        lblStare.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' btnListare / btnExcel
        '
        btnListare.Dock = System.Windows.Forms.DockStyle.Fill
        btnListare.Enabled = False
        btnListare.Location = New System.Drawing.Point(170, 4)
        btnListare.Margin = New System.Windows.Forms.Padding(0)
        btnListare.Name = "btnListare"
        btnListare.Size = New System.Drawing.Size(170, 42)
        btnListare.TabIndex = 1
        btnListare.Text = "Listare"
        tips.SetToolTipHeader(btnListare, "Listare")
        tips.SetToolTipText(btnListare, "Pregătește raportul cu perioada și datele alese și îl arată ca PDF, în dreapta.")
        btnExcel.Dock = System.Windows.Forms.DockStyle.Left
        btnExcel.Enabled = False
        btnExcel.Location = New System.Drawing.Point(0, 4)
        btnExcel.Margin = New System.Windows.Forms.Padding(0)
        btnExcel.Name = "btnExcel"
        btnExcel.Size = New System.Drawing.Size(160, 42)
        btnExcel.TabIndex = 0
        btnExcel.Text = "Export Excel"
        tips.SetToolTipHeader(btnExcel, "Export Excel")
        tips.SetToolTipText(btnExcel, "Pregătește raportul cu perioada și datele alese și îl salvează ca fișier Excel, cu toate coloanele.")
        '
        ' rowAct
        '
        rowAct.Controls.Add(btnListare)
        rowAct.Controls.Add(btnExcel)
        rowAct.Dock = System.Windows.Forms.DockStyle.Bottom
        rowAct.Location = New System.Drawing.Point(0, 620)
        rowAct.Margin = New System.Windows.Forms.Padding(0)
        rowAct.Name = "rowAct"
        rowAct.Padding = New System.Windows.Forms.Padding(0, 4, 0, 4)
        rowAct.Size = New System.Drawing.Size(340, 50)
        rowAct.TabIndex = 1
        '
        ' pnlSel: the data of the report (the tree), the reason line and the two buttons
        '
        pnlSel.Controls.Add(treeSel)
        pnlSel.Controls.Add(lblStare)
        pnlSel.Controls.Add(rowAct)
        pnlSel.Controls.Add(rowSelHead)
        pnlSel.Dock = System.Windows.Forms.DockStyle.Left
        pnlSel.Location = New System.Drawing.Point(364, 9)
        pnlSel.Margin = New System.Windows.Forms.Padding(0)
        pnlSel.Name = "pnlSel"
        pnlSel.Padding = New System.Windows.Forms.Padding(0, 0, 12, 0)
        pnlSel.Size = New System.Drawing.Size(352, 670)
        pnlSel.TabIndex = 1
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
        pnlRaport.Location = New System.Drawing.Point(716, 9)
        pnlRaport.Margin = New System.Windows.Forms.Padding(0)
        pnlRaport.Name = "pnlRaport"
        pnlRaport.Size = New System.Drawing.Size(766, 670)
        pnlRaport.TabIndex = 0
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(pnlRaport)
        pnlCard.Controls.Add(pnlSel)
        pnlCard.Controls.Add(pnlParam)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(3, 69)
        pnlCard.Margin = New System.Windows.Forms.Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New System.Windows.Forms.Padding(12, 9, 12, 9)
        pnlCard.Size = New System.Drawing.Size(1494, 688)
        pnlCard.TabIndex = 1
        pnlCard.Tag = "Card"
        '
        ' VerconRapoarteForm
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        ClientSize = New System.Drawing.Size(1500, 760)
        Controls.Add(pnlCard)
        Controls.Add(busyBar)
        Controls.Add(capBar)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(1300, 700)
        Name = "VerconRapoarteForm"
        Padding = New System.Windows.Forms.Padding(3)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "VERCON — Rapoarte"
        pnlCard.ResumeLayout(False)
        pnlParam.ResumeLayout(False)
        pnlSel.ResumeLayout(False)
        pnlRaport.ResumeLayout(False)
        rowSelHead.ResumeLayout(False)
        rowAct.ResumeLayout(False)
        rowSub.ResumeLayout(False)
        rowGest.ResumeLayout(False)
        rowAn.ResumeLayout(False)
        rowPer.ResumeLayout(False)
        rowDeLa.ResumeLayout(False)
        rowPana.ResumeLayout(False)
        rowGrupeaza.ResumeLayout(False)
        rowFara.ResumeLayout(False)
        rowDocumente.ResumeLayout(False)
        rowPrimitori.ResumeLayout(False)
        rowGrupPrim.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tmrLookups As System.Windows.Forms.Timer
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents busyBar As KBotBusyBar
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents pnlParam As System.Windows.Forms.Panel
    Friend WithEvents pnlSel As System.Windows.Forms.Panel
    Friend WithEvents pnlRaport As System.Windows.Forms.Panel
    Friend WithEvents rowSub As System.Windows.Forms.Panel
    Friend WithEvents lblSub As System.Windows.Forms.Label
    Friend WithEvents cboSub As KBotComboBox
    Friend WithEvents rowGest As System.Windows.Forms.Panel
    Friend WithEvents lblGest As System.Windows.Forms.Label
    Friend WithEvents cboGest As KBotComboBox
    Friend WithEvents rowAn As System.Windows.Forms.Panel
    Friend WithEvents lblAn As System.Windows.Forms.Label
    Friend WithEvents cboAn As KBotComboBox
    Friend WithEvents lblPerTitlu As System.Windows.Forms.Label
    Friend WithEvents rowPer As System.Windows.Forms.Panel
    Friend WithEvents lblPer As System.Windows.Forms.Label
    Friend WithEvents cboPer As KBotComboBox
    Friend WithEvents rowDeLa As System.Windows.Forms.Panel
    Friend WithEvents lblDeLa As System.Windows.Forms.Label
    Friend WithEvents dtpDeLa As KBotDatePicker
    Friend WithEvents rowPana As System.Windows.Forms.Panel
    Friend WithEvents lblPana As System.Windows.Forms.Label
    Friend WithEvents dtpPana As KBotDatePicker
    Friend WithEvents rowGrupeaza As System.Windows.Forms.Panel
    Friend WithEvents chkGrupeaza As KBotCheckBox
    Friend WithEvents rowFara As System.Windows.Forms.Panel
    Friend WithEvents chkFara As KBotCheckBox
    Friend WithEvents rowDocumente As System.Windows.Forms.Panel
    Friend WithEvents chkDocumente As KBotCheckBox
    Friend WithEvents rowPrimitori As System.Windows.Forms.Panel
    Friend WithEvents chkPrimitori As KBotCheckBox
    Friend WithEvents rowGrupPrim As System.Windows.Forms.Panel
    Friend WithEvents chkGrupPrim As KBotCheckBox
    Friend WithEvents treeRapoarte As AdvancedTreeControl
    Friend WithEvents treeSel As AdvancedTreeControl
    Friend WithEvents rowSelHead As System.Windows.Forms.Panel
    Friend WithEvents chkToate As KBotCheckBox
    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents rowAct As System.Windows.Forms.Panel
    Friend WithEvents btnListare As KBotButton
    Friend WithEvents btnExcel As KBotButton
    Friend WithEvents previewPdf As ReaderHostPreview
    Friend WithEvents ntfMesaj As KBotNotice
End Class
