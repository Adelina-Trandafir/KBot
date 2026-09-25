Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariAplicatieView
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
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        cboVerbose = New KBotComboBox()
        chkLogViewer = New CheckBox()
        chkShowBrowser = New CheckBox()
        chkReceptii = New CheckBox()
        chkAvansate = New CheckBox()
        cboAdobeMotor = New KBotComboBox()
        btnAdobeGazduire = New Button()
        chkAcroTrace = New CheckBox()
        chkAcroNou = New CheckBox()
        btnMesajeAdobe = New Button()
        cboExcelRibbon = New KBotComboBox()
        cboSortare = New KBotComboBox()
        cboOrdine = New KBotComboBox()
        chkNumeCod = New CheckBox()
        chkNumeSurse = New CheckBox()
        chkDataCod = New CheckBox()
        chkDataSurse = New CheckBox()
        txtLatimeCod = New KBotTextField()
        txtLatimeSurse = New KBotTextField()
        navPagini = New KBotNavList()
        tlyGenerale = New KBotTableLayoutPanel()
        lblTitluComutatoare = New Label()
        tlyComutatoare = New KBotTableLayoutPanel()
        lblVerbose = New Label()
        tlyPaginaDocumente = New KBotTableLayoutPanel()
        lblTitluDocumente = New Label()
        tlyDocumente = New KBotTableLayoutPanel()
        lblAdobeMotor = New Label()
        lblExcelRibbon = New Label()
        tlyPaginaKbot = New KBotTableLayoutPanel()
        lblTitluArbore = New Label()
        tlyArbore = New KBotTableLayoutPanel()
        lblSortare = New Label()
        lblOrdine = New Label()
        lblColoaneNume = New Label()
        lblColoaneData = New Label()
        lblLatimeCod = New Label()
        lblLatimeSurse = New Label()
        CType(navPagini, ComponentModel.ISupportInitialize).BeginInit()
        tlyGenerale.SuspendLayout()
        tlyComutatoare.SuspendLayout()
        tlyPaginaDocumente.SuspendLayout()
        tlyDocumente.SuspendLayout()
        tlyPaginaKbot.SuspendLayout()
        tlyArbore.SuspendLayout()
        SuspendLayout()
        '
        ' cboVerbose
        '
        cboVerbose.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboVerbose.CornerRadius = 4
        cboVerbose.DrawMode = DrawMode.OwnerDrawFixed
        cboVerbose.DropDownStyle = ComboBoxStyle.DropDownList
        cboVerbose.FlatStyle = FlatStyle.Flat
        cboVerbose.ItemHeight = 31
        cboVerbose.Location = New Point(404, 0)
        cboVerbose.Margin = New Padding(4, 0, 4, 10)
        cboVerbose.Name = "cboVerbose"
        cboVerbose.Size = New Size(496, 37)
        cboVerbose.TabIndex = 1
        tips.SetToolTipHeader(cboVerbose, "Cât scrie consola FOREXE")
        tips.SetToolTipText(cboVerbose, "«Implicit» = pornit pe Debug, oprit pe Release (felia 0071)." & vbLf & "Pornit arată pașii, așteptările, andocarea și stivele; oprit arată doar <Log> și erorile." & vbLf & "Fișierul-jurnal primește oricum tot.")
        '
        ' chkLogViewer
        '
        chkLogViewer.AutoSize = True
        tlyComutatoare.SetColumnSpan(chkLogViewer, 2)
        chkLogViewer.Location = New Point(4, 47)
        chkLogViewer.Margin = New Padding(4, 0, 4, 10)
        chkLogViewer.Name = "chkLogViewer"
        chkLogViewer.Size = New Size(498, 26)
        chkLogViewer.TabIndex = 2
        chkLogViewer.Text = "Rândul «Arată jurnal» în meniul de opțiuni al ferestrei principale"
        tips.SetToolTipHeader(chkLogViewer, "Vizualizatorul de jurnale")
        tips.SetToolTipText(chkLogViewer, "Debifat, rândul dispare din meniul butonului de opțiuni." & vbLf & "Jurnalele se scriu în continuare pe disc.")
        chkLogViewer.UseVisualStyleBackColor = True
        '
        ' chkShowBrowser
        '
        chkShowBrowser.AutoSize = True
        tlyComutatoare.SetColumnSpan(chkShowBrowser, 2)
        chkShowBrowser.Location = New Point(4, 83)
        chkShowBrowser.Margin = New Padding(4, 0, 4, 10)
        chkShowBrowser.Name = "chkShowBrowser"
        chkShowBrowser.Size = New Size(359, 26)
        chkShowBrowser.TabIndex = 3
        chkShowBrowser.Text = "Butonul «Arată browserul» în banda FOREXE"
        tips.SetToolTipHeader(chkShowBrowser, "Butonul de browser")
        tips.SetToolTipText(chkShowBrowser, "Debifat, operatorul nu mai poate deschide vizualizatorul browserului FOREXE." & vbLf & "Robotul rulează la fel; doar butonul dispare.")
        chkShowBrowser.UseVisualStyleBackColor = True
        '
        ' chkReceptii
        '
        chkReceptii.AutoSize = True
        tlyComutatoare.SetColumnSpan(chkReceptii, 2)
        chkReceptii.Location = New Point(4, 119)
        chkReceptii.Margin = New Padding(4, 0, 4, 10)
        chkReceptii.Name = "chkReceptii"
        chkReceptii.Size = New Size(462, 26)
        chkReceptii.TabIndex = 4
        chkReceptii.Text = "Selectorul de recepții se deschide cu toate recepțiile bifate"
        tips.SetToolTipHeader(chkReceptii, "Recepțiile de descărcat")
        tips.SetToolTipText(chkReceptii, "Bifat: apăsarea obișnuită aduce tot; debifat: nimic până nu alegi." & vbLf & "Cerut configurabil de operator la 10.09.2026.")
        chkReceptii.UseVisualStyleBackColor = True
        '
        ' chkAvansate
        '
        chkAvansate.AutoSize = True
        tlyComutatoare.SetColumnSpan(chkAvansate, 2)
        chkAvansate.Location = New Point(4, 155)
        chkAvansate.Margin = New Padding(4, 0, 4, 10)
        chkAvansate.Name = "chkAvansate"
        chkAvansate.Size = New Size(260, 26)
        chkAvansate.TabIndex = 5
        chkAvansate.Text = "Activează opțiuni avansate"
        tips.SetToolTipHeader(chkAvansate, "Opțiuni avansate")
        tips.SetToolTipText(chkAvansate, "Bifat: apar paginile «Documente» (aici), «Pagina FOREXE», «Temă» și «Căi fișiere»." & vbLf & "Bifarea cere parola; debifarea nu.")
        chkAvansate.UseVisualStyleBackColor = True
        '
        ' cboAdobeMotor
        '
        cboAdobeMotor.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboAdobeMotor.CornerRadius = 4
        cboAdobeMotor.DrawMode = DrawMode.OwnerDrawFixed
        cboAdobeMotor.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeMotor.FlatStyle = FlatStyle.Flat
        cboAdobeMotor.ItemHeight = 31
        cboAdobeMotor.Location = New Point(404, 0)
        cboAdobeMotor.Margin = New Padding(4, 0, 4, 10)
        cboAdobeMotor.Name = "cboAdobeMotor"
        cboAdobeMotor.Size = New Size(496, 37)
        cboAdobeMotor.TabIndex = 1
        tips.SetToolTipHeader(cboAdobeMotor, "Motorul de previzualizare PDF")
        tips.SetToolTipText(cboAdobeMotor, "«Fereastră găzduită» = fereastra Adobe mutată în panoul K-BOT (singura rulată în aplicație)." & vbLf & "«ActiveX» = controlul AcroPDF, în proces.")
        '
        ' btnAdobeGazduire
        '
        btnAdobeGazduire.AutoSize = True
        btnAdobeGazduire.Dock = DockStyle.Left
        btnAdobeGazduire.FlatStyle = FlatStyle.Flat
        btnAdobeGazduire.Location = New Point(404, 47)
        btnAdobeGazduire.Margin = New Padding(4, 0, 4, 10)
        btnAdobeGazduire.Name = "btnAdobeGazduire"
        btnAdobeGazduire.Padding = New Padding(12, 4, 12, 4)
        btnAdobeGazduire.Size = New Size(320, 45)
        btnAdobeGazduire.TabIndex = 2
        btnAdobeGazduire.Text = "Opțiuni fereastră găzduită…"
        tips.SetToolTipHeader(btnAdobeGazduire, "Fereastra găzduită Adobe")
        tips.SetToolTipText(btnAdobeGazduire, "Modul vizualizatorului, comutatorul /n, eliberarea ferestrei și fereastra plutitoare." & vbLf & "Se deschide singur când alegi «Fereastră găzduită»; de aici le poți revedea oricând.")
        btnAdobeGazduire.UseVisualStyleBackColor = True
        '
        ' chkAcroTrace
        '
        chkAcroTrace.AutoSize = True
        chkAcroTrace.Location = New Point(404, 102)
        chkAcroTrace.Margin = New Padding(4, 0, 4, 10)
        chkAcroTrace.Name = "chkAcroTrace"
        chkAcroTrace.Size = New Size(462, 26)
        chkAcroTrace.TabIndex = 3
        chkAcroTrace.Text = "ActiveX — jurnal de diagnostic detaliat (acropdf_trace.log)"
        tips.SetToolTipHeader(chkAcroTrace, "Jurnal de diagnostic ActiveX")
        tips.SetToolTipText(chkAcroTrace, "Bifat: vizualizatorul ActiveX scrie în Logs\acropdf_trace.log tot ce vede și tot ce face (cronometre, ferestre, procese, salvări)." & vbLf & "Înregistrează de la deschiderea unui document până când e deschis sau apare o eroare care blochează." & vbLf & "Nu se salvează: la fiecare pornire a aplicației este oprit.")
        chkAcroTrace.UseVisualStyleBackColor = True
        '
        ' chkAcroNou
        '
        chkAcroNou.AutoSize = True
        chkAcroNou.Location = New Point(404, 138)
        chkAcroNou.Margin = New Padding(4, 0, 4, 10)
        chkAcroNou.Name = "chkAcroNou"
        chkAcroNou.Size = New Size(462, 26)
        chkAcroNou.TabIndex = 4
        chkAcroNou.Text = "ActiveX — control Adobe nou la fiecare document"
        tips.SetToolTipHeader(chkAcroNou, "Control Adobe nou la fiecare document")
        tips.SetToolTipText(chkAcroNou, "Bifat: la alegerea altei revizii (DDF și ORD) controlul Adobe se închide și se distruge, iar documentul nou se deschide într-un control nou." & vbLf & "Debifat: documentul nou se încarcă în același control." & vbLf & "Doar pentru motoarele ActiveX.")
        chkAcroNou.UseVisualStyleBackColor = True
        '
        ' btnMesajeAdobe
        '
        btnMesajeAdobe.AutoSize = True
        btnMesajeAdobe.Dock = DockStyle.Left
        btnMesajeAdobe.FlatStyle = FlatStyle.Flat
        btnMesajeAdobe.Location = New Point(404, 174)
        btnMesajeAdobe.Margin = New Padding(4, 0, 4, 10)
        btnMesajeAdobe.Name = "btnMesajeAdobe"
        btnMesajeAdobe.Padding = New Padding(12, 4, 12, 4)
        btnMesajeAdobe.Size = New Size(320, 45)
        btnMesajeAdobe.TabIndex = 5
        btnMesajeAdobe.Text = "Mesaje de script Adobe…"
        tips.SetToolTipHeader(btnMesajeAdobe, "Mesaje de script Adobe")
        tips.SetToolTipText(btnMesajeAdobe, "Lista mesajelor «Warning: JavaScript Window» pe care K-BOT le închide singur (expresii regulate)." & vbLf & "Un mesaj care nu se potrivește cu lista rămâne pe ecran, pentru tine." & vbLf & "Pentru ambele motoare de previzualizare.")
        btnMesajeAdobe.UseVisualStyleBackColor = True
        '
        ' cboExcelRibbon
        '
        cboExcelRibbon.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboExcelRibbon.CornerRadius = 4
        cboExcelRibbon.DrawMode = DrawMode.OwnerDrawFixed
        cboExcelRibbon.DropDownStyle = ComboBoxStyle.DropDownList
        cboExcelRibbon.FlatStyle = FlatStyle.Flat
        cboExcelRibbon.ItemHeight = 31
        cboExcelRibbon.Location = New Point(404, 229)
        cboExcelRibbon.Margin = New Padding(4, 0, 4, 10)
        cboExcelRibbon.Name = "cboExcelRibbon"
        cboExcelRibbon.Size = New Size(496, 37)
        cboExcelRibbon.TabIndex = 7
        tips.SetToolTipHeader(cboExcelRibbon, "Panglica Excel în previzualizare")
        tips.SetToolTipText(cboExcelRibbon, "Macro: Excel își ascunde singur panglica (poate fi refuzat de o politică)." & vbLf & "Fereastră: se ascunde fereastra panglicii, ca la Word — nu poate fi refuzat." & vbLf & "Word are o singură metodă și nu se configurează.")
        '
        ' cboSortare
        '
        cboSortare.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        tlyArbore.SetColumnSpan(cboSortare, 2)
        cboSortare.CornerRadius = 4
        cboSortare.DrawMode = DrawMode.OwnerDrawFixed
        cboSortare.DropDownStyle = ComboBoxStyle.DropDownList
        cboSortare.FlatStyle = FlatStyle.Flat
        cboSortare.ItemHeight = 31
        cboSortare.Location = New Point(404, 0)
        cboSortare.Margin = New Padding(4, 0, 4, 10)
        cboSortare.Name = "cboSortare"
        cboSortare.Size = New Size(496, 37)
        cboSortare.TabIndex = 1
        tips.SetToolTipHeader(cboSortare, "Ordinea arborelui de angajamente")
        tips.SetToolTipText(cboSortare, "«După nume» = după descrierea angajamentului." & vbLf & "«După data creării» = din TOATE sursele anului (nu doar SS-ul ales); cele fără dată descărcată vin la urmă, după nume." & vbLf & "Aceeași alegere e și în meniul iconiței din antetul arborelui.")
        '
        ' cboOrdine
        '
        cboOrdine.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        tlyArbore.SetColumnSpan(cboOrdine, 2)
        cboOrdine.CornerRadius = 4
        cboOrdine.DrawMode = DrawMode.OwnerDrawFixed
        cboOrdine.DropDownStyle = ComboBoxStyle.DropDownList
        cboOrdine.FlatStyle = FlatStyle.Flat
        cboOrdine.ItemHeight = 31
        cboOrdine.Location = New Point(404, 47)
        cboOrdine.Margin = New Padding(4, 0, 4, 10)
        cboOrdine.Name = "cboOrdine"
        cboOrdine.Size = New Size(496, 37)
        cboOrdine.TabIndex = 2
        tips.SetToolTipHeader(cboOrdine, "Ordinea sortării")
        tips.SetToolTipText(cboOrdine, "«Crescătoare» = A…Z, respectiv de la cel mai vechi (implicit)." & vbLf & "«Descrescătoare» = Z…A, respectiv de la cel mai nou." & vbLf & "Cele fără dată descărcată rămân oricum la urmă.")
        '
        ' lblOrdine
        '
        lblOrdine.AutoSize = True
        lblOrdine.Dock = DockStyle.Fill
        lblOrdine.Location = New Point(4, 47)
        lblOrdine.Margin = New Padding(4, 0, 4, 10)
        lblOrdine.Name = "lblOrdine"
        lblOrdine.Size = New Size(392, 37)
        lblOrdine.TabIndex = 1
        lblOrdine.Text = "Ordine"
        lblOrdine.TextAlign = ContentAlignment.MiddleLeft
        '
        ' chkNumeCod
        '
        chkNumeCod.AutoSize = True
        chkNumeCod.Location = New Point(404, 47)
        chkNumeCod.Margin = New Padding(4, 0, 4, 10)
        chkNumeCod.Name = "chkNumeCod"
        chkNumeCod.Size = New Size(180, 26)
        chkNumeCod.TabIndex = 3
        chkNumeCod.Text = "CODANGAJAMENT"
        tips.SetToolTipHeader(chkNumeCod, "Coloana CODANGAJAMENT")
        tips.SetToolTipText(chkNumeCod, "Afișată cât timp arborele e sortat după nume." & vbLf & "Implicit: afișată.")
        chkNumeCod.UseVisualStyleBackColor = True
        '
        ' chkNumeSurse
        '
        chkNumeSurse.AutoSize = True
        chkNumeSurse.Location = New Point(684, 47)
        chkNumeSurse.Margin = New Padding(4, 0, 4, 10)
        chkNumeSurse.Name = "chkNumeSurse"
        chkNumeSurse.Size = New Size(90, 26)
        chkNumeSurse.TabIndex = 4
        chkNumeSurse.Text = "SURSE"
        tips.SetToolTipHeader(chkNumeSurse, "Coloana SURSE")
        tips.SetToolTipText(chkNumeSurse, "Afișată cât timp arborele e sortat după nume." & vbLf & "Implicit: ascunsă.")
        chkNumeSurse.UseVisualStyleBackColor = True
        '
        ' chkDataCod
        '
        chkDataCod.AutoSize = True
        chkDataCod.Location = New Point(404, 83)
        chkDataCod.Margin = New Padding(4, 0, 4, 10)
        chkDataCod.Name = "chkDataCod"
        chkDataCod.Size = New Size(180, 26)
        chkDataCod.TabIndex = 6
        chkDataCod.Text = "CODANGAJAMENT"
        tips.SetToolTipHeader(chkDataCod, "Coloana CODANGAJAMENT")
        tips.SetToolTipText(chkDataCod, "Afișată cât timp arborele e sortat după data creării." & vbLf & "Implicit: ascunsă.")
        chkDataCod.UseVisualStyleBackColor = True
        '
        ' chkDataSurse
        '
        chkDataSurse.AutoSize = True
        chkDataSurse.Location = New Point(684, 83)
        chkDataSurse.Margin = New Padding(4, 0, 4, 10)
        chkDataSurse.Name = "chkDataSurse"
        chkDataSurse.Size = New Size(90, 26)
        chkDataSurse.TabIndex = 7
        chkDataSurse.Text = "SURSE"
        tips.SetToolTipHeader(chkDataSurse, "Coloana SURSE")
        tips.SetToolTipText(chkDataSurse, "Afișată cât timp arborele e sortat după data creării." & vbLf & "Implicit: afișată.")
        chkDataSurse.UseVisualStyleBackColor = True
        '
        ' navPagini
        '
        navPagini.Dock = DockStyle.Top
        navPagini.ItemCornerRadius = 8
        navPagini.ItemPadding = New Padding(6)
        KBotNavItem1.Image = My.Resources.Resources.settings__1_
        KBotNavItem1.Key = "generale"
        KBotNavItem1.Text = "Generale"
        KBotNavItem2.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Pdf_exports_24
        KBotNavItem2.Key = "documente"
        KBotNavItem2.Text = "Documente"
        KBotNavItem3.Image = My.Resources.Resources.kbot_64
        KBotNavItem3.Key = "kbot"
        KBotNavItem3.Text = "KBOT"
        navPagini.Items.Add(KBotNavItem1)
        navPagini.Items.Add(KBotNavItem2)
        navPagini.Items.Add(KBotNavItem3)
        navPagini.Location = New Point(0, 0)
        navPagini.Margin = New Padding(0)
        navPagini.Name = "navPagini"
        navPagini.Orientation = KBotNavOrientation.Horizontal
        navPagini.SelectedKey = Nothing
        navPagini.Size = New Size(960, 60)
        navPagini.TabIndex = 0
        '
        ' tlyGenerale
        '
        tlyGenerale.AutoScroll = True
        tlyGenerale.ColumnCount = 1
        tlyGenerale.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyGenerale.Controls.Add(lblTitluComutatoare, 0, 0)
        tlyGenerale.Controls.Add(tlyComutatoare, 0, 1)
        tlyGenerale.Dock = DockStyle.Fill
        tlyGenerale.Location = New Point(0, 60)
        tlyGenerale.Margin = New Padding(0)
        tlyGenerale.Name = "tlyGenerale"
        tlyGenerale.Padding = New Padding(24, 18, 24, 18)
        tlyGenerale.RowCount = 3
        tlyGenerale.RowStyles.Add(New RowStyle())
        tlyGenerale.RowStyles.Add(New RowStyle())
        tlyGenerale.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tlyGenerale.Size = New Size(960, 397)
        tlyGenerale.TabIndex = 1
        '
        ' lblTitluComutatoare
        '
        lblTitluComutatoare.AutoSize = True
        lblTitluComutatoare.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluComutatoare.Location = New Point(28, 18)
        lblTitluComutatoare.Margin = New Padding(4, 0, 4, 8)
        lblTitluComutatoare.Name = "lblTitluComutatoare"
        lblTitluComutatoare.Size = New Size(158, 32)
        lblTitluComutatoare.TabIndex = 0
        lblTitluComutatoare.Text = "Comutatoare"
        '
        ' tlyComutatoare
        '
        tlyComutatoare.AutoFitToTheme = False
        tlyComutatoare.AutoSize = True
        tlyComutatoare.ColumnCount = 2
        tlyComutatoare.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 400F))
        tlyComutatoare.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyComutatoare.Controls.Add(lblVerbose, 0, 0)
        tlyComutatoare.Controls.Add(cboVerbose, 1, 0)
        tlyComutatoare.Controls.Add(chkLogViewer, 0, 1)
        tlyComutatoare.Controls.Add(chkShowBrowser, 0, 2)
        tlyComutatoare.Controls.Add(chkReceptii, 0, 3)
        tlyComutatoare.Controls.Add(chkAvansate, 0, 4)
        tlyComutatoare.Dock = DockStyle.Top
        tlyComutatoare.Location = New Point(28, 58)
        tlyComutatoare.Margin = New Padding(4, 0, 4, 24)
        tlyComutatoare.Name = "tlyComutatoare"
        tlyComutatoare.RowCount = 5
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.Size = New Size(904, 191)
        tlyComutatoare.TabIndex = 1
        '
        ' lblVerbose
        '
        lblVerbose.AutoSize = True
        lblVerbose.Dock = DockStyle.Fill
        lblVerbose.Location = New Point(4, 0)
        lblVerbose.Margin = New Padding(4, 0, 4, 10)
        lblVerbose.Name = "lblVerbose"
        lblVerbose.Size = New Size(392, 37)
        lblVerbose.TabIndex = 0
        lblVerbose.Text = "Consola FOREXE detaliată (VerboseLogging)"
        lblVerbose.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyPaginaDocumente
        '
        tlyPaginaDocumente.AutoScroll = True
        tlyPaginaDocumente.ColumnCount = 1
        tlyPaginaDocumente.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPaginaDocumente.Controls.Add(lblTitluDocumente, 0, 0)
        tlyPaginaDocumente.Controls.Add(tlyDocumente, 0, 1)
        tlyPaginaDocumente.Dock = DockStyle.Fill
        tlyPaginaDocumente.Location = New Point(0, 60)
        tlyPaginaDocumente.Margin = New Padding(0)
        tlyPaginaDocumente.Name = "tlyPaginaDocumente"
        tlyPaginaDocumente.Padding = New Padding(24, 18, 24, 18)
        tlyPaginaDocumente.RowCount = 3
        tlyPaginaDocumente.RowStyles.Add(New RowStyle())
        tlyPaginaDocumente.RowStyles.Add(New RowStyle())
        tlyPaginaDocumente.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tlyPaginaDocumente.Size = New Size(960, 397)
        tlyPaginaDocumente.TabIndex = 2
        tlyPaginaDocumente.Visible = False
        '
        ' lblTitluDocumente
        '
        lblTitluDocumente.AutoSize = True
        lblTitluDocumente.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluDocumente.Location = New Point(28, 18)
        lblTitluDocumente.Margin = New Padding(4, 0, 4, 8)
        lblTitluDocumente.Name = "lblTitluDocumente"
        lblTitluDocumente.Size = New Size(343, 32)
        lblTitluDocumente.TabIndex = 0
        lblTitluDocumente.Text = "Documente (PDF, Word, Excel)"
        '
        ' tlyDocumente
        '
        tlyDocumente.AutoFitToTheme = False
        tlyDocumente.AutoSize = True
        tlyDocumente.ColumnCount = 2
        tlyDocumente.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 400F))
        tlyDocumente.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyDocumente.Controls.Add(lblAdobeMotor, 0, 0)
        tlyDocumente.Controls.Add(cboAdobeMotor, 1, 0)
        tlyDocumente.Controls.Add(btnAdobeGazduire, 1, 1)
        tlyDocumente.Controls.Add(chkAcroTrace, 1, 2)
        tlyDocumente.Controls.Add(chkAcroNou, 1, 3)
        tlyDocumente.Controls.Add(btnMesajeAdobe, 1, 4)
        tlyDocumente.Controls.Add(lblExcelRibbon, 0, 5)
        tlyDocumente.Controls.Add(cboExcelRibbon, 1, 5)
        tlyDocumente.Dock = DockStyle.Top
        tlyDocumente.Location = New Point(28, 58)
        tlyDocumente.Margin = New Padding(4, 0, 4, 24)
        tlyDocumente.Name = "tlyDocumente"
        tlyDocumente.RowCount = 6
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.Size = New Size(904, 276)
        tlyDocumente.TabIndex = 1
        '
        ' lblAdobeMotor
        '
        lblAdobeMotor.AutoSize = True
        lblAdobeMotor.Dock = DockStyle.Fill
        lblAdobeMotor.Location = New Point(4, 0)
        lblAdobeMotor.Margin = New Padding(4, 0, 4, 10)
        lblAdobeMotor.Name = "lblAdobeMotor"
        lblAdobeMotor.Size = New Size(392, 37)
        lblAdobeMotor.TabIndex = 0
        lblAdobeMotor.Text = "PDF — motor de previzualizare"
        lblAdobeMotor.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblExcelRibbon
        '
        lblExcelRibbon.AutoSize = True
        lblExcelRibbon.Dock = DockStyle.Fill
        lblExcelRibbon.Location = New Point(4, 229)
        lblExcelRibbon.Margin = New Padding(4, 0, 4, 10)
        lblExcelRibbon.Name = "lblExcelRibbon"
        lblExcelRibbon.Size = New Size(392, 37)
        lblExcelRibbon.TabIndex = 6
        lblExcelRibbon.Text = "Excel — cum se ascunde panglica"
        lblExcelRibbon.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyPaginaKbot
        '
        tlyPaginaKbot.AutoScroll = True
        tlyPaginaKbot.ColumnCount = 1
        tlyPaginaKbot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPaginaKbot.Controls.Add(lblTitluArbore, 0, 0)
        tlyPaginaKbot.Controls.Add(tlyArbore, 0, 1)
        tlyPaginaKbot.Dock = DockStyle.Fill
        tlyPaginaKbot.Location = New Point(0, 60)
        tlyPaginaKbot.Margin = New Padding(0)
        tlyPaginaKbot.Name = "tlyPaginaKbot"
        tlyPaginaKbot.Padding = New Padding(24, 18, 24, 18)
        tlyPaginaKbot.RowCount = 3
        tlyPaginaKbot.RowStyles.Add(New RowStyle())
        tlyPaginaKbot.RowStyles.Add(New RowStyle())
        tlyPaginaKbot.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tlyPaginaKbot.Size = New Size(960, 397)
        tlyPaginaKbot.TabIndex = 3
        tlyPaginaKbot.Visible = False
        '
        ' lblTitluArbore
        '
        lblTitluArbore.AutoSize = True
        lblTitluArbore.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluArbore.Location = New Point(28, 18)
        lblTitluArbore.Margin = New Padding(4, 0, 4, 8)
        lblTitluArbore.Name = "lblTitluArbore"
        lblTitluArbore.Size = New Size(300, 32)
        lblTitluArbore.TabIndex = 0
        lblTitluArbore.Text = "Arborele de angajamente"
        '
        ' tlyArbore
        '
        tlyArbore.AutoFitToTheme = False
        tlyArbore.AutoSize = True
        tlyArbore.ColumnCount = 3
        tlyArbore.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 400F))
        tlyArbore.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 280F))
        tlyArbore.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyArbore.Controls.Add(lblSortare, 0, 0)
        tlyArbore.Controls.Add(cboSortare, 1, 0)
        tlyArbore.Controls.Add(lblOrdine, 0, 1)
        tlyArbore.Controls.Add(cboOrdine, 1, 1)
        tlyArbore.Controls.Add(lblColoaneNume, 0, 2)
        tlyArbore.Controls.Add(chkNumeCod, 1, 2)
        tlyArbore.Controls.Add(chkNumeSurse, 2, 2)
        tlyArbore.Controls.Add(lblColoaneData, 0, 3)
        tlyArbore.Controls.Add(chkDataCod, 1, 3)
        tlyArbore.Controls.Add(chkDataSurse, 2, 3)
        tlyArbore.Controls.Add(lblLatimeCod, 0, 4)
        tlyArbore.Controls.Add(txtLatimeCod, 1, 4)
        tlyArbore.Controls.Add(lblLatimeSurse, 0, 5)
        tlyArbore.Controls.Add(txtLatimeSurse, 1, 5)
        tlyArbore.Dock = DockStyle.Top
        tlyArbore.Location = New Point(28, 58)
        tlyArbore.Margin = New Padding(4, 0, 4, 24)
        tlyArbore.Name = "tlyArbore"
        tlyArbore.RowCount = 6
        tlyArbore.RowStyles.Add(New RowStyle())
        tlyArbore.RowStyles.Add(New RowStyle())
        tlyArbore.RowStyles.Add(New RowStyle())
        tlyArbore.RowStyles.Add(New RowStyle())
        tlyArbore.RowStyles.Add(New RowStyle())
        tlyArbore.RowStyles.Add(New RowStyle())
        tlyArbore.Size = New Size(904, 266)
        tlyArbore.TabIndex = 1
        '
        ' lblSortare
        '
        lblSortare.AutoSize = True
        lblSortare.Dock = DockStyle.Fill
        lblSortare.Location = New Point(4, 0)
        lblSortare.Margin = New Padding(4, 0, 4, 10)
        lblSortare.Name = "lblSortare"
        lblSortare.Size = New Size(392, 37)
        lblSortare.TabIndex = 0
        lblSortare.Text = "Sortare"
        lblSortare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblColoaneNume
        '
        lblColoaneNume.AutoSize = True
        lblColoaneNume.Dock = DockStyle.Fill
        lblColoaneNume.Location = New Point(4, 47)
        lblColoaneNume.Margin = New Padding(4, 0, 4, 10)
        lblColoaneNume.Name = "lblColoaneNume"
        lblColoaneNume.Size = New Size(392, 26)
        lblColoaneNume.TabIndex = 2
        lblColoaneNume.Text = "Coloane afișate la sortarea după nume"
        lblColoaneNume.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblColoaneData
        '
        lblColoaneData.AutoSize = True
        lblColoaneData.Dock = DockStyle.Fill
        lblColoaneData.Location = New Point(4, 83)
        lblColoaneData.Margin = New Padding(4, 0, 4, 10)
        lblColoaneData.Name = "lblColoaneData"
        lblColoaneData.Size = New Size(392, 26)
        lblColoaneData.TabIndex = 5
        lblColoaneData.Text = "Coloane afișate la sortarea după dată"
        lblColoaneData.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblLatimeCod
        '
        lblLatimeCod.AutoSize = True
        lblLatimeCod.Dock = DockStyle.Fill
        lblLatimeCod.Location = New Point(4, 119)
        lblLatimeCod.Margin = New Padding(4, 0, 4, 10)
        lblLatimeCod.Name = "lblLatimeCod"
        lblLatimeCod.Size = New Size(392, 40)
        lblLatimeCod.TabIndex = 8
        lblLatimeCod.Text = "Lățimea coloanei CODANGAJAMENT (px)"
        lblLatimeCod.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtLatimeCod
        '
        txtLatimeCod.Anchor = AnchorStyles.Left
        txtLatimeCod.BackColor = Color.Transparent
        txtLatimeCod.Location = New Point(404, 119)
        txtLatimeCod.Margin = New Padding(4, 0, 4, 10)
        txtLatimeCod.MaxLength = 3
        txtLatimeCod.Name = "txtLatimeCod"
        txtLatimeCod.PlaceholderText = "100"
        txtLatimeCod.Size = New Size(150, 40)
        txtLatimeCod.TabIndex = 9
        txtLatimeCod.TextAlign = HorizontalAlignment.Center
        txtLatimeCod.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtLatimeCod, "Lățimea coloanei CODANGAJAMENT")
        tips.SetToolTipText(txtLatimeCod, "În pixeli la 100% (se mărește singură pe ecranele scalate)." & vbLf & "Între 30 și 600. Se salvează la Enter sau când ieși din câmp.")
        '
        ' lblLatimeSurse
        '
        lblLatimeSurse.AutoSize = True
        lblLatimeSurse.Dock = DockStyle.Fill
        lblLatimeSurse.Location = New Point(4, 169)
        lblLatimeSurse.Margin = New Padding(4, 0, 4, 10)
        lblLatimeSurse.Name = "lblLatimeSurse"
        lblLatimeSurse.Size = New Size(392, 40)
        lblLatimeSurse.TabIndex = 10
        lblLatimeSurse.Text = "Lățimea coloanei SURSE (px)"
        lblLatimeSurse.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtLatimeSurse
        '
        txtLatimeSurse.Anchor = AnchorStyles.Left
        txtLatimeSurse.BackColor = Color.Transparent
        txtLatimeSurse.Location = New Point(404, 169)
        txtLatimeSurse.Margin = New Padding(4, 0, 4, 10)
        txtLatimeSurse.MaxLength = 3
        txtLatimeSurse.Name = "txtLatimeSurse"
        txtLatimeSurse.PlaceholderText = "70"
        txtLatimeSurse.Size = New Size(150, 40)
        txtLatimeSurse.TabIndex = 11
        txtLatimeSurse.TextAlign = HorizontalAlignment.Center
        txtLatimeSurse.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtLatimeSurse, "Lățimea coloanei SURSE")
        tips.SetToolTipText(txtLatimeSurse, "În pixeli la 100% (se mărește singură pe ecranele scalate)." & vbLf & "Între 30 și 600. Se salvează la Enter sau când ieși din câmp.")
        '
        ' SetariAplicatieView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyGenerale)
        Controls.Add(tlyPaginaDocumente)
        Controls.Add(tlyPaginaKbot)
        Controls.Add(navPagini)
        Name = "SetariAplicatieView"
        Size = New Size(960, 457)
        CType(navPagini, ComponentModel.ISupportInitialize).EndInit()
        tlyGenerale.ResumeLayout(False)
        tlyGenerale.PerformLayout()
        tlyComutatoare.ResumeLayout(False)
        tlyComutatoare.PerformLayout()
        tlyPaginaDocumente.ResumeLayout(False)
        tlyPaginaDocumente.PerformLayout()
        tlyDocumente.ResumeLayout(False)
        tlyDocumente.PerformLayout()
        tlyPaginaKbot.ResumeLayout(False)
        tlyPaginaKbot.PerformLayout()
        tlyArbore.ResumeLayout(False)
        tlyArbore.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents navPagini As KBotNavList
    Friend WithEvents tlyGenerale As KBotTableLayoutPanel
    Friend WithEvents lblTitluComutatoare As Label
    Friend WithEvents tlyComutatoare As KBotTableLayoutPanel
    Friend WithEvents lblVerbose As Label
    Friend WithEvents cboVerbose As KBotComboBox
    Friend WithEvents chkLogViewer As CheckBox
    Friend WithEvents chkShowBrowser As CheckBox
    Friend WithEvents chkReceptii As CheckBox
    Friend WithEvents chkAvansate As CheckBox
    Friend WithEvents tlyPaginaDocumente As KBotTableLayoutPanel
    Friend WithEvents lblTitluDocumente As Label
    Friend WithEvents tlyDocumente As KBotTableLayoutPanel
    Friend WithEvents lblAdobeMotor As Label
    Friend WithEvents cboAdobeMotor As KBotComboBox
    Friend WithEvents btnAdobeGazduire As Button
    Friend WithEvents chkAcroTrace As CheckBox
    Friend WithEvents chkAcroNou As CheckBox
    Friend WithEvents btnMesajeAdobe As Button
    Friend WithEvents lblExcelRibbon As Label
    Friend WithEvents cboExcelRibbon As KBotComboBox
    Friend WithEvents tlyPaginaKbot As KBotTableLayoutPanel
    Friend WithEvents lblTitluArbore As Label
    Friend WithEvents tlyArbore As KBotTableLayoutPanel
    Friend WithEvents lblSortare As Label
    Friend WithEvents cboSortare As KBotComboBox
    Friend WithEvents lblOrdine As Label
    Friend WithEvents cboOrdine As KBotComboBox
    Friend WithEvents lblColoaneNume As Label
    Friend WithEvents chkNumeCod As CheckBox
    Friend WithEvents chkNumeSurse As CheckBox
    Friend WithEvents lblColoaneData As Label
    Friend WithEvents chkDataCod As CheckBox
    Friend WithEvents chkDataSurse As CheckBox
    Friend WithEvents lblLatimeCod As Label
    Friend WithEvents txtLatimeCod As KBotTextField
    Friend WithEvents lblLatimeSurse As Label
    Friend WithEvents txtLatimeSurse As KBotTextField
End Class
