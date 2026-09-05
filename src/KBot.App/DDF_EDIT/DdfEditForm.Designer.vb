Imports KBot.Controls

' The DDF EDITOR (slice 0051) -- the port of `frmFX_DDF`, `frmFX_DDF_REV`,
' `frmFX_DDF_REV_SECT_A`, `frmFX_DDF_REV_SECT_B` and `frmFX_DDF_ATT`.
'
' WHY A SEPARATE FORM and not a page of `DdfView`: Access opened `frmFX_DDF` as a popup with
' save/cancel semantics. `DdfView` stays read-only and only gains the entry points; editing
' has its own form, modal over `MainForm`. Same shape as `OrdEditForm` from slice 0049,
' deliberately.
'
' THE ANTET IS THE FORM HEADER, not a page. Every field of the `frmFX_DDF` header AND the
' revision fields of `frmFX_DDF_REV` live in `tlyAntet`, four rows of them:
'   1  CodAngajament (read-only) · CUAL · DataCreare · Total (read-only, SUM(ValCur))
'   2  ObiectDDF, full width
'   3  Program · Comp · PartAng + the partner combo
'   4  NumarRev · DataRev · Desc_Scurta
'
' THE FOUR SUBFORMS BECAME FOUR PAGES behind a horizontal `KBotNavList`:
'   «Sectiunea A» = frmFX_DDF_REV_SECT_A   (the only editable grid)
'   «Sectiunea B» = frmFX_DDF_REV_SECT_B   (read-only; recomputed from A)
'   «Descriere»   = the long description, on the ported VBA_DDF_INFO editor
'   «Fisiere»     = frmFX_DDF_ATT
'
' COMPARTMENT: ONE control, one value -- `cmbComp`, with `Editable = True` and
' `LimitToList = False`. The operator has to be able to TYPE a compartment that is not in the
' list, because there is no compartment nomenclator in MariaDB and on a fresh database the
' list is empty. Slice 0051 believed an editable combo could not be themed (its native child
' EDIT is not painted by us) and carried a `txtComp` box beside the picker for that reason;
' slice 0051-02 answered `WM_CTLCOLOREDIT` in `KBotComboBox` and the box became dead weight.
' The typed text reaches the draft through `PreiaCompartimentul`, which is called on leave and
' again from «Salveaza» -- the operator can press the button without leaving the field.
'
' All controls are declared HERE (docs/kbot-forms-ui-convention.md).
' Coordinates are written at 144 dpi -- the file was saved from the designer on a 150% screen,
' and Visual Studio rewrites the coordinates and the stamp together when it does.
' AutoScaleDimensions goes with them: Calibri 9 measures (9, 22) there (slice 0052). The two
' always change together; a stamp taken from another font or another dpi squashes the window
' on open, with nothing in the designer to show it.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfEditForm
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
        components = New ComponentModel.Container()
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem4 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        txtCual = New KBotTextField()
        dtpDataCreare = New KBotDatePicker()
        lblTotal = New Label()
        txtObiect = New KBotTextField()
        cmbProgram = New KBotComboBox()
        cmbComp = New KBotComboBox()
        chkPartAng = New CheckBox()
        cmbPartener = New KBotComboBox()
        txtNumarRev = New KBotTextField()
        dtpDataRev = New KBotDatePicker()
        txtDescScurta = New KBotTextField()
        btnRenunta = New Button()
        btnSalveaza = New Button()
        tmrLock = New Timer(components)
        tlyMain = New TableLayoutPanel()
        capBar = New KBotCaptionBar()
        busyBar = New KBotBusyBar()
        tlyAntet = New TableLayoutPanel()
        lblCodCaption = New Label()
        lblCod = New Label()
        lblCualCaption = New Label()
        lblDataCreareCaption = New Label()
        lblTotalCaption = New Label()
        lblObiectCaption = New Label()
        lblProgramCaption = New Label()
        lblCompCaption = New Label()
        lblNumarRevCaption = New Label()
        lblDataRevCaption = New Label()
        lblDescScurtaCaption = New Label()
        ntfMesaj = New KBotNotice()
        navSub = New KBotNavList()
        pnlPages = New Panel()
        tlySubsol = New TableLayoutPanel()
        tlyMain.SuspendLayout()
        tlyAntet.SuspendLayout()
        CType(navSub, ComponentModel.ISupportInitialize).BeginInit()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        ' 
        ' txtCual
        ' 
        txtCual.BackColor = Color.Transparent
        txtCual.Dock = DockStyle.Fill
        txtCual.Font = New Font("Calibri", 9F, FontStyle.Bold)
        txtCual.Location = New Point(467, 3)
        txtCual.Margin = New Padding(4, 3, 4, 3)
        txtCual.MaxLength = 32767
        txtCual.Name = "txtCual"
        txtCual.PlaceholderText = ""
        txtCual.Size = New Size(163, 34)
        txtCual.TabIndex = 0
        txtCual.TabStop = False
        tips.SetToolTipHeader(txtCual, "CUAL")
        tips.SetToolTipText(txtCual, "Numărul documentului, rezervat pe server cât timp formularul e deschis." & vbLf & "Îl poți schimba: dacă numărul cerut e liber, rezervarea se mută pe el.")
        txtCual.UseSystemPasswordChar = False
        ' 
        ' dtpDataCreare
        ' 
        dtpDataCreare.ButtonPadding = New Padding(0, 0, 4, 0)
        dtpDataCreare.ButtonWidth = 16
        dtpDataCreare.Dock = DockStyle.Fill
        dtpDataCreare.FocusBorderColor = SystemColors.Highlight
        dtpDataCreare.Font = New Font("Calibri", 9F, FontStyle.Bold)
        dtpDataCreare.ForeColor = SystemColors.ActiveCaptionText
        dtpDataCreare.GlyphColor = SystemColors.ActiveCaptionText
        dtpDataCreare.GlyphSize = 16
        dtpDataCreare.Location = New Point(798, 3)
        dtpDataCreare.Margin = New Padding(4, 3, 4, 3)
        dtpDataCreare.Name = "dtpDataCreare"
        dtpDataCreare.Size = New Size(169, 34)
        dtpDataCreare.TabIndex = 1
        dtpDataCreare.TabStop = False
        tips.SetToolTipHeader(dtpDataCreare, "Data creării")
        tips.SetToolTipText(dtpDataCreare, "Data documentului de fundamentare.")
        ' 
        ' lblTotal
        ' 
        lblTotal.AutoSize = True
        lblTotal.Dock = DockStyle.Fill
        lblTotal.Font = New Font("Calibri", 12F, FontStyle.Bold)
        lblTotal.Location = New Point(1045, 0)
        lblTotal.Margin = New Padding(4, 0, 4, 0)
        lblTotal.Name = "lblTotal"
        lblTotal.Size = New Size(241, 40)
        lblTotal.TabIndex = 5
        lblTotal.Text = "0,00"
        lblTotal.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblTotal, "Totalul reviziei")
        tips.SetToolTipText(lblTotal, "Suma valorilor curente din secțiunea A." & vbLf & "Se recalculează la fiecare modificare.")
        ' 
        ' txtObiect
        ' 
        txtObiect.BackColor = Color.Transparent
        tlyAntet.SetColumnSpan(txtObiect, 7)
        txtObiect.Dock = DockStyle.Fill
        txtObiect.Font = New Font("Calibri", 9F, FontStyle.Bold)
        txtObiect.Location = New Point(144, 43)
        txtObiect.Margin = New Padding(4, 3, 4, 3)
        txtObiect.MaxLength = 32767
        txtObiect.Name = "txtObiect"
        txtObiect.PlaceholderText = ""
        txtObiect.Size = New Size(1142, 36)
        txtObiect.TabIndex = 2
        txtObiect.TabStop = False
        tips.SetToolTipHeader(txtObiect, "Obiectul documentului")
        tips.SetToolTipText(txtObiect, "Se scrie și în descrierea angajamentului la salvare." & vbLf & "Peste 255 de caractere, descrierea angajamentului se scurtează.")
        txtObiect.UseSystemPasswordChar = False
        ' 
        ' cmbProgram
        ' 
        cmbProgram.Dock = DockStyle.Fill
        cmbProgram.DrawMode = DrawMode.OwnerDrawFixed
        cmbProgram.DropDownStyle = ComboBoxStyle.DropDownList
        cmbProgram.FlatStyle = FlatStyle.Flat
        cmbProgram.Font = New Font("Calibri", 9F, FontStyle.Bold)
        cmbProgram.ItemHeight = 28
        cmbProgram.Location = New Point(144, 85)
        cmbProgram.Margin = New Padding(4, 3, 4, 3)
        cmbProgram.Name = "cmbProgram"
        cmbProgram.Size = New Size(182, 34)
        cmbProgram.TabIndex = 3
        tips.SetToolTipHeader(cmbProgram, "Program")
        tips.SetToolTipText(cmbProgram, "Lista fixă moștenită din Access.")
        ' 
        ' cmbComp
        ' 
        tlyAntet.SetColumnSpan(cmbComp, 2)
        cmbComp.Dock = DockStyle.Fill
        cmbComp.DrawMode = DrawMode.OwnerDrawFixed
        cmbComp.Editable = True
        cmbComp.FlatStyle = FlatStyle.Flat
        cmbComp.Font = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        cmbComp.ItemHeight = 28
        cmbComp.LimitToList = False
        cmbComp.Location = New Point(467, 85)
        cmbComp.Margin = New Padding(4, 3, 4, 3)
        cmbComp.Name = "cmbComp"
        cmbComp.Size = New Size(323, 34)
        cmbComp.TabIndex = 5
        tips.SetToolTipHeader(cmbComp, "Compartiment")
        tips.SetToolTipText(cmbComp, "Alege un compartiment de pe documentele anterioare." & vbLf & "Poți și să scrii unul care nu este în listă.")
        ' 
        ' chkPartAng
        ' 
        chkPartAng.AutoSize = True
        chkPartAng.Dock = DockStyle.Fill
        chkPartAng.Font = New Font("Calibri", 9F)
        chkPartAng.Location = New Point(798, 85)
        chkPartAng.Margin = New Padding(4, 3, 4, 3)
        chkPartAng.Name = "chkPartAng"
        chkPartAng.Size = New Size(169, 36)
        chkPartAng.TabIndex = 6
        chkPartAng.Text = "Partener asociat"
        tips.SetToolTipHeader(chkPartAng, "Partener asociat")
        tips.SetToolTipText(chkPartAng, "Leagă tot documentul de un singur partener." & vbLf & "Partenerul ales se scrie pe toate rândurile din secțiunile A și B.")
        chkPartAng.UseVisualStyleBackColor = True
        ' 
        ' cmbPartener
        ' 
        tlyAntet.SetColumnSpan(cmbPartener, 2)
        cmbPartener.Dock = DockStyle.Fill
        cmbPartener.DrawMode = DrawMode.OwnerDrawFixed
        cmbPartener.DropDownStyle = ComboBoxStyle.DropDownList
        cmbPartener.FlatStyle = FlatStyle.Flat
        cmbPartener.Font = New Font("Calibri", 9F, FontStyle.Bold)
        cmbPartener.ItemHeight = 28
        cmbPartener.Location = New Point(975, 85)
        cmbPartener.Margin = New Padding(4, 3, 4, 3)
        cmbPartener.Name = "cmbPartener"
        cmbPartener.Size = New Size(311, 34)
        cmbPartener.TabIndex = 7
        tips.SetToolTipHeader(cmbPartener, "Partenerul documentului")
        tips.SetToolTipText(cmbPartener, "Se reține codul fiscal — el e cel care se salvează." & vbLf & "Schimbarea lui rescrie partenerul pe toate rândurile.")
        ' 
        ' txtNumarRev
        ' 
        txtNumarRev.BackColor = Color.Transparent
        txtNumarRev.Dock = DockStyle.Fill
        txtNumarRev.Font = New Font("Calibri", 9F, FontStyle.Bold)
        txtNumarRev.Location = New Point(144, 127)
        txtNumarRev.Margin = New Padding(4, 3, 4, 3)
        txtNumarRev.MaxLength = 32767
        txtNumarRev.Name = "txtNumarRev"
        txtNumarRev.PlaceholderText = ""
        txtNumarRev.Size = New Size(182, 37)
        txtNumarRev.TabIndex = 8
        txtNumarRev.TabStop = False
        tips.SetToolTipHeader(txtNumarRev, "Numărul reviziei")
        tips.SetToolTipText(txtNumarRev, "Rezervat pe server cât timp formularul e deschis." & vbLf & "Revizia inițială este numărul 0.")
        txtNumarRev.UseSystemPasswordChar = False
        ' 
        ' dtpDataRev
        ' 
        dtpDataRev.ButtonPadding = New Padding(0, 0, 4, 0)
        dtpDataRev.ButtonWidth = 16
        dtpDataRev.Dock = DockStyle.Fill
        dtpDataRev.FocusBorderColor = SystemColors.Highlight
        dtpDataRev.Font = New Font("Calibri", 9F, FontStyle.Bold)
        dtpDataRev.ForeColor = SystemColors.ActiveCaptionText
        dtpDataRev.GlyphColor = SystemColors.ActiveCaptionText
        dtpDataRev.GlyphSize = 16
        dtpDataRev.Location = New Point(467, 127)
        dtpDataRev.Margin = New Padding(4, 3, 4, 3)
        dtpDataRev.Name = "dtpDataRev"
        dtpDataRev.Size = New Size(163, 37)
        dtpDataRev.TabIndex = 9
        dtpDataRev.TabStop = False
        tips.SetToolTipHeader(dtpDataRev, "Data reviziei")
        tips.SetToolTipText(dtpDataRev, "Nu poate fi mai veche decât ultima revizie a angajamentului.")
        ' 
        ' txtDescScurta
        ' 
        txtDescScurta.BackColor = Color.Transparent
        tlyAntet.SetColumnSpan(txtDescScurta, 3)
        txtDescScurta.Dock = DockStyle.Fill
        txtDescScurta.Font = New Font("Calibri", 9F, FontStyle.Bold)
        txtDescScurta.Location = New Point(798, 127)
        txtDescScurta.Margin = New Padding(4, 3, 4, 3)
        txtDescScurta.MaxLength = 32767
        txtDescScurta.Name = "txtDescScurta"
        txtDescScurta.PlaceholderText = ""
        txtDescScurta.Size = New Size(488, 37)
        txtDescScurta.TabIndex = 10
        txtDescScurta.TabStop = False
        tips.SetToolTipHeader(txtDescScurta, "Descrierea scurtă")
        tips.SetToolTipText(txtDescScurta, "Când o schimbi, descrierea lungă primește același text." & vbLf & "O poți rescrie apoi pe pagina «Descriere».")
        txtDescScurta.UseSystemPasswordChar = False
        ' 
        ' btnRenunta
        ' 
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Location = New Point(10, 3)
        btnRenunta.Margin = New Padding(10, 3, 0, 3)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New Padding(14, 7, 14, 7)
        btnRenunta.Size = New Size(219, 52)
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fără să salveze nimic." & vbLf & "Numerele rezervate se eliberează.")
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.Location = New Point(1012, 3)
        btnSalveaza.Margin = New Padding(0, 3, 10, 3)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(14, 7, 14, 7)
        btnSalveaza.Size = New Size(276, 52)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează documentul"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Trimite tot documentul într-o singură tranzacție." & vbLf & "Fișierele atașate se încarcă imediat după, când rândurile lor au chei.")
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' tmrLock
        ' 
        tmrLock.Interval = 300000
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(busyBar, 0, 1)
        tlyMain.Controls.Add(tlyAntet, 0, 2)
        tlyMain.Controls.Add(ntfMesaj, 0, 3)
        tlyMain.Controls.Add(navSub, 0, 4)
        tlyMain.Controls.Add(pnlPages, 0, 5)
        tlyMain.Controls.Add(tlySubsol, 0, 6)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 2)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 7
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 57F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 7F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 177F))
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 38F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.Size = New Size(1298, 996)
        tlyMain.TabIndex = 0
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowTextScaleSlider = False
        capBar.ShowThemeEditor = False
        capBar.ShowThemeOptions = False
        capBar.Size = New Size(1298, 57)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Document de fundamentare"
        ' 
        ' busyBar
        ' 
        busyBar.Dock = DockStyle.Fill
        busyBar.Location = New Point(0, 57)
        busyBar.Margin = New Padding(0)
        busyBar.Name = "busyBar"
        busyBar.Size = New Size(1298, 7)
        busyBar.TabIndex = 1
        busyBar.TabStop = False
        ' 
        ' tlyAntet
        ' 
        tlyAntet.ColumnCount = 8
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 133F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 171F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 177F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 70F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyAntet.Controls.Add(lblCodCaption, 0, 0)
        tlyAntet.Controls.Add(lblCod, 1, 0)
        tlyAntet.Controls.Add(lblCualCaption, 2, 0)
        tlyAntet.Controls.Add(txtCual, 3, 0)
        tlyAntet.Controls.Add(lblDataCreareCaption, 4, 0)
        tlyAntet.Controls.Add(dtpDataCreare, 5, 0)
        tlyAntet.Controls.Add(lblTotalCaption, 6, 0)
        tlyAntet.Controls.Add(lblTotal, 7, 0)
        tlyAntet.Controls.Add(lblObiectCaption, 0, 1)
        tlyAntet.Controls.Add(txtObiect, 1, 1)
        tlyAntet.Controls.Add(lblProgramCaption, 0, 2)
        tlyAntet.Controls.Add(cmbProgram, 1, 2)
        tlyAntet.Controls.Add(lblCompCaption, 2, 2)
        tlyAntet.Controls.Add(cmbComp, 3, 2)
        tlyAntet.Controls.Add(chkPartAng, 5, 2)
        tlyAntet.Controls.Add(cmbPartener, 6, 2)
        tlyAntet.Controls.Add(lblNumarRevCaption, 0, 3)
        tlyAntet.Controls.Add(txtNumarRev, 1, 3)
        tlyAntet.Controls.Add(lblDataRevCaption, 2, 3)
        tlyAntet.Controls.Add(dtpDataRev, 3, 3)
        tlyAntet.Controls.Add(lblDescScurtaCaption, 4, 3)
        tlyAntet.Controls.Add(txtDescScurta, 5, 3)
        tlyAntet.Dock = DockStyle.Fill
        tlyAntet.Location = New Point(4, 69)
        tlyAntet.Margin = New Padding(4, 5, 4, 5)
        tlyAntet.Name = "tlyAntet"
        tlyAntet.RowCount = 4
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyAntet.Size = New Size(1290, 167)
        tlyAntet.TabIndex = 2
        ' 
        ' lblCodCaption
        ' 
        lblCodCaption.AutoSize = True
        lblCodCaption.Dock = DockStyle.Fill
        lblCodCaption.Font = New Font("Calibri", 9F)
        lblCodCaption.Location = New Point(4, 0)
        lblCodCaption.Margin = New Padding(4, 0, 4, 0)
        lblCodCaption.Name = "lblCodCaption"
        lblCodCaption.Size = New Size(132, 40)
        lblCodCaption.TabIndex = 0
        lblCodCaption.Text = "Cod angajament"
        lblCodCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCod
        ' 
        lblCod.AutoSize = True
        lblCod.Dock = DockStyle.Fill
        lblCod.Font = New Font("Calibri", 12F, FontStyle.Bold)
        lblCod.Location = New Point(144, 0)
        lblCod.Margin = New Padding(4, 0, 4, 0)
        lblCod.Name = "lblCod"
        lblCod.Size = New Size(182, 40)
        lblCod.TabIndex = 1
        lblCod.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCualCaption
        ' 
        lblCualCaption.AutoSize = True
        lblCualCaption.Dock = DockStyle.Fill
        lblCualCaption.Font = New Font("Calibri", 9F)
        lblCualCaption.Location = New Point(334, 0)
        lblCualCaption.Margin = New Padding(4, 0, 4, 0)
        lblCualCaption.Name = "lblCualCaption"
        lblCualCaption.Size = New Size(125, 40)
        lblCualCaption.TabIndex = 2
        lblCualCaption.Text = "CUAL"
        lblCualCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataCreareCaption
        ' 
        lblDataCreareCaption.AutoSize = True
        lblDataCreareCaption.Dock = DockStyle.Fill
        lblDataCreareCaption.Font = New Font("Calibri", 9F)
        lblDataCreareCaption.Location = New Point(638, 0)
        lblDataCreareCaption.Margin = New Padding(4, 0, 4, 0)
        lblDataCreareCaption.Name = "lblDataCreareCaption"
        lblDataCreareCaption.Size = New Size(152, 40)
        lblDataCreareCaption.TabIndex = 3
        lblDataCreareCaption.Text = "Data creării"
        lblDataCreareCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblTotalCaption
        ' 
        lblTotalCaption.AutoSize = True
        lblTotalCaption.Dock = DockStyle.Fill
        lblTotalCaption.Font = New Font("Calibri", 9F)
        lblTotalCaption.Location = New Point(975, 0)
        lblTotalCaption.Margin = New Padding(4, 0, 4, 0)
        lblTotalCaption.Name = "lblTotalCaption"
        lblTotalCaption.Size = New Size(62, 40)
        lblTotalCaption.TabIndex = 4
        lblTotalCaption.Text = "Total"
        lblTotalCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblObiectCaption
        ' 
        lblObiectCaption.AutoSize = True
        lblObiectCaption.Dock = DockStyle.Fill
        lblObiectCaption.Font = New Font("Calibri", 9F)
        lblObiectCaption.Location = New Point(4, 40)
        lblObiectCaption.Margin = New Padding(4, 0, 4, 0)
        lblObiectCaption.Name = "lblObiectCaption"
        lblObiectCaption.Size = New Size(132, 42)
        lblObiectCaption.TabIndex = 6
        lblObiectCaption.Text = "Obiectul documentului"
        lblObiectCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblProgramCaption
        ' 
        lblProgramCaption.AutoSize = True
        lblProgramCaption.Dock = DockStyle.Fill
        lblProgramCaption.Font = New Font("Calibri", 9F)
        lblProgramCaption.Location = New Point(4, 82)
        lblProgramCaption.Margin = New Padding(4, 0, 4, 0)
        lblProgramCaption.Name = "lblProgramCaption"
        lblProgramCaption.Size = New Size(132, 42)
        lblProgramCaption.TabIndex = 7
        lblProgramCaption.Text = "Program"
        lblProgramCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCompCaption
        ' 
        lblCompCaption.AutoSize = True
        lblCompCaption.Dock = DockStyle.Fill
        lblCompCaption.Font = New Font("Calibri", 9F)
        lblCompCaption.Location = New Point(334, 82)
        lblCompCaption.Margin = New Padding(4, 0, 4, 0)
        lblCompCaption.Name = "lblCompCaption"
        lblCompCaption.Size = New Size(125, 42)
        lblCompCaption.TabIndex = 8
        lblCompCaption.Text = "Compartiment"
        lblCompCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblNumarRevCaption
        ' 
        lblNumarRevCaption.AutoSize = True
        lblNumarRevCaption.Dock = DockStyle.Fill
        lblNumarRevCaption.Font = New Font("Calibri", 9F)
        lblNumarRevCaption.Location = New Point(4, 124)
        lblNumarRevCaption.Margin = New Padding(4, 0, 4, 0)
        lblNumarRevCaption.Name = "lblNumarRevCaption"
        lblNumarRevCaption.Size = New Size(132, 43)
        lblNumarRevCaption.TabIndex = 9
        lblNumarRevCaption.Text = "Număr revizie"
        lblNumarRevCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataRevCaption
        ' 
        lblDataRevCaption.AutoSize = True
        lblDataRevCaption.Dock = DockStyle.Fill
        lblDataRevCaption.Font = New Font("Calibri", 9F)
        lblDataRevCaption.Location = New Point(334, 124)
        lblDataRevCaption.Margin = New Padding(4, 0, 4, 0)
        lblDataRevCaption.Name = "lblDataRevCaption"
        lblDataRevCaption.Size = New Size(125, 43)
        lblDataRevCaption.TabIndex = 10
        lblDataRevCaption.Text = "Data reviziei"
        lblDataRevCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDescScurtaCaption
        ' 
        lblDescScurtaCaption.AutoSize = True
        lblDescScurtaCaption.Dock = DockStyle.Fill
        lblDescScurtaCaption.Font = New Font("Calibri", 9F)
        lblDescScurtaCaption.Location = New Point(638, 124)
        lblDescScurtaCaption.Margin = New Padding(4, 0, 4, 0)
        lblDescScurtaCaption.Name = "lblDescScurtaCaption"
        lblDescScurtaCaption.Size = New Size(152, 43)
        lblDescScurtaCaption.TabIndex = 11
        lblDescScurtaCaption.Text = "Descriere scurtă"
        lblDescScurtaCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ntfMesaj
        ' 
        ntfMesaj.BackColor = Color.Transparent
        ntfMesaj.Dock = DockStyle.Fill
        ntfMesaj.Location = New Point(4, 246)
        ntfMesaj.Margin = New Padding(4, 5, 4, 5)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New Size(1290, 10)
        ntfMesaj.TabIndex = 3
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        ' 
        ' navSub
        ' 
        navSub.Dock = DockStyle.Fill
        navSub.IconSize = 16
        navSub.ItemCornerRadius = 0
        navSub.ItemPadding = New Padding(3, 0, 3, 0)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Image = My.Resources.Resources.cells
        KBotNavItem1.Key = "sectiunea-a"
        KBotNavItem1.Text = "Secțiunea A"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Image = My.Resources.Resources.vertical
        KBotNavItem2.Key = "sectiunea-b"
        KBotNavItem2.Text = "Secțiunea B"
        KBotNavItem3.AutoSize = True
        KBotNavItem3.Image = My.Resources.Resources.binvoice
        KBotNavItem3.Key = "descriere"
        KBotNavItem3.Text = "Descriere"
        KBotNavItem4.AutoSize = True
        KBotNavItem4.Image = My.Resources.Resources.binvoice
        KBotNavItem4.Key = "fisiere"
        KBotNavItem4.Text = "Fișiere"
        navSub.Items.Add(KBotNavItem1)
        navSub.Items.Add(KBotNavItem2)
        navSub.Items.Add(KBotNavItem3)
        navSub.Items.Add(KBotNavItem4)
        navSub.Location = New Point(0, 261)
        navSub.Margin = New Padding(0)
        navSub.Name = "navSub"
        navSub.Orientation = KBotNavOrientation.Horizontal
        navSub.SelectedKey = Nothing
        navSub.Size = New Size(1298, 38)
        navSub.TabIndex = 4
        ' 
        ' pnlPages
        ' 
        pnlPages.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlPages.Dock = DockStyle.Fill
        pnlPages.Location = New Point(0, 299)
        pnlPages.Margin = New Padding(0)
        pnlPages.Name = "pnlPages"
        pnlPages.Size = New Size(1298, 639)
        pnlPages.TabIndex = 5
        ' 
        ' tlySubsol
        ' 
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 229F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 286F))
        tlySubsol.Controls.Add(btnRenunta, 0, 0)
        tlySubsol.Controls.Add(btnSalveaza, 2, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 938)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(1298, 58)
        tlySubsol.TabIndex = 6
        ' 
        ' DdfEditForm
        ' 
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1641, 1000)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 5, 4, 5)
        MaximizeBox = False
        MinimizeBox = False
        Name = "DdfEditForm"
        Padding = New Padding(1, 2, 1, 2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Document de fundamentare"
        tlyMain.ResumeLayout(False)
        tlyAntet.ResumeLayout(False)
        tlyAntet.PerformLayout()
        CType(navSub, ComponentModel.ISupportInitialize).EndInit()
        tlySubsol.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents tmrLock As Timer
    Friend WithEvents tlyMain As TableLayoutPanel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents busyBar As Global.KBot.Controls.KBotBusyBar
    Friend WithEvents tlyAntet As TableLayoutPanel
    Friend WithEvents lblCodCaption As Label
    Friend WithEvents lblCod As Label
    Friend WithEvents lblCualCaption As Label
    Friend WithEvents txtCual As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblDataCreareCaption As Label
    Friend WithEvents dtpDataCreare As Global.KBot.Controls.KBotDatePicker
    Friend WithEvents lblTotalCaption As Label
    Friend WithEvents lblTotal As Label
    Friend WithEvents lblObiectCaption As Label
    Friend WithEvents txtObiect As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblProgramCaption As Label
    Friend WithEvents cmbProgram As Global.KBot.Controls.KBotComboBox
    Friend WithEvents lblCompCaption As Label
    Friend WithEvents cmbComp As Global.KBot.Controls.KBotComboBox
    Friend WithEvents chkPartAng As CheckBox
    Friend WithEvents cmbPartener As Global.KBot.Controls.KBotComboBox
    Friend WithEvents lblNumarRevCaption As Label
    Friend WithEvents txtNumarRev As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblDataRevCaption As Label
    Friend WithEvents dtpDataRev As Global.KBot.Controls.KBotDatePicker
    Friend WithEvents lblDescScurtaCaption As Label
    Friend WithEvents txtDescScurta As Global.KBot.Controls.KBotTextField
    Friend WithEvents ntfMesaj As Global.KBot.Controls.KBotNotice
    Friend WithEvents navSub As Global.KBot.Controls.KBotNavList
    Friend WithEvents pnlPages As Panel
    Friend WithEvents tlySubsol As TableLayoutPanel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnSalveaza As Button
End Class
