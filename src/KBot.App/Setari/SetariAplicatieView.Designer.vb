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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyBody = New KBotTableLayoutPanel()
        lblTitluComutatoare = New Label()
        tlyComutatoare = New KBotTableLayoutPanel()
        lblVerbose = New Label()
        cboVerbose = New KBotComboBox()
        chkLogViewer = New CheckBox()
        chkShowBrowser = New CheckBox()
        chkReceptii = New CheckBox()
        lblTitluDocumente = New Label()
        tlyDocumente = New KBotTableLayoutPanel()
        lblAdobeMotor = New Label()
        cboAdobeMotor = New KBotComboBox()
        lblAdobeMod = New Label()
        cboAdobeMod = New KBotComboBox()
        lblAdobeInst = New Label()
        cboAdobeInst = New KBotComboBox()
        lblAdobeDetach = New Label()
        cboAdobeDetach = New KBotComboBox()
        chkAdobePopup = New CheckBox()
        lblExcelRibbon = New Label()
        cboExcelRibbon = New KBotComboBox()
        lblTitluFoldere = New Label()
        lblFoldereHint = New Label()
        gridFoldere = New KBotDataView()
        tlyFoldereButoane = New KBotTableLayoutPanel()
        lblFoldereStare = New Label()
        btnSalveazaFoldere = New Button()
        tlyBody.SuspendLayout()
        tlyComutatoare.SuspendLayout()
        tlyDocumente.SuspendLayout()
        CType(gridFoldere, ComponentModel.ISupportInitialize).BeginInit()
        tlyFoldereButoane.SuspendLayout()
        SuspendLayout()
        '
        ' tlyBody
        '
        tlyBody.AutoScroll = True
        tlyBody.ColumnCount = 1
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBody.Controls.Add(lblTitluComutatoare, 0, 0)
        tlyBody.Controls.Add(tlyComutatoare, 0, 1)
        tlyBody.Controls.Add(lblTitluDocumente, 0, 2)
        tlyBody.Controls.Add(tlyDocumente, 0, 3)
        tlyBody.Controls.Add(lblTitluFoldere, 0, 4)
        tlyBody.Controls.Add(lblFoldereHint, 0, 5)
        tlyBody.Controls.Add(gridFoldere, 0, 6)
        tlyBody.Controls.Add(tlyFoldereButoane, 0, 7)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(0, 0)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.Padding = New Padding(24, 18, 24, 18)
        tlyBody.RowCount = 8
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Absolute, 450F))
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.Size = New Size(960, 840)
        tlyBody.TabIndex = 0
        '
        ' lblTitluComutatoare
        '
        lblTitluComutatoare.AutoSize = True
        lblTitluComutatoare.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluComutatoare.Location = New Point(28, 18)
        lblTitluComutatoare.Margin = New Padding(4, 0, 4, 8)
        lblTitluComutatoare.Name = "lblTitluComutatoare"
        lblTitluComutatoare.Size = New Size(146, 32)
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
        tlyComutatoare.Dock = DockStyle.Top
        tlyComutatoare.Location = New Point(28, 58)
        tlyComutatoare.Margin = New Padding(4, 0, 4, 24)
        tlyComutatoare.Name = "tlyComutatoare"
        tlyComutatoare.RowCount = 4
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.Size = New Size(904, 170)
        tlyComutatoare.TabIndex = 1
        '
        ' lblVerbose
        '
        lblVerbose.AutoSize = True
        lblVerbose.Dock = DockStyle.Fill
        lblVerbose.Location = New Point(4, 0)
        lblVerbose.Margin = New Padding(4, 0, 4, 10)
        lblVerbose.Name = "lblVerbose"
        lblVerbose.Size = New Size(312, 42)
        lblVerbose.TabIndex = 0
        lblVerbose.Text = "Consola FOREXE detaliată (VerboseLogging)"
        lblVerbose.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboVerbose
        '
        cboVerbose.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboVerbose.CornerRadius = 4
        cboVerbose.DrawMode = DrawMode.OwnerDrawFixed
        cboVerbose.DropDownStyle = ComboBoxStyle.DropDownList
        cboVerbose.FlatStyle = FlatStyle.Flat
        cboVerbose.ItemHeight = 30
        cboVerbose.Location = New Point(324, 0)
        cboVerbose.Margin = New Padding(4, 0, 4, 10)
        cboVerbose.Name = "cboVerbose"
        cboVerbose.Size = New Size(360, 36)
        cboVerbose.TabIndex = 1
        tips.SetToolTipHeader(cboVerbose, "Cât scrie consola FOREXE")
        tips.SetToolTipText(cboVerbose, "«Implicit» = pornit pe Debug, oprit pe Release (felia 0071)." & vbLf & "Pornit arată pașii, așteptările, andocarea și stivele; oprit arată doar <Log> și erorile." & vbLf & "Fișierul-jurnal primește oricum tot.")
        '
        ' chkLogViewer
        '
        chkLogViewer.AutoSize = True
        tlyComutatoare.SetColumnSpan(chkLogViewer, 2)
        chkLogViewer.Location = New Point(4, 52)
        chkLogViewer.Margin = New Padding(4, 0, 4, 10)
        chkLogViewer.Name = "chkLogViewer"
        chkLogViewer.Size = New Size(420, 29)
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
        chkShowBrowser.Location = New Point(4, 91)
        chkShowBrowser.Margin = New Padding(4, 0, 4, 10)
        chkShowBrowser.Name = "chkShowBrowser"
        chkShowBrowser.Size = New Size(420, 29)
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
        chkReceptii.Location = New Point(4, 130)
        chkReceptii.Margin = New Padding(4, 0, 4, 10)
        chkReceptii.Name = "chkReceptii"
        chkReceptii.Size = New Size(420, 29)
        chkReceptii.TabIndex = 4
        chkReceptii.Text = "Selectorul de recepții se deschide cu toate recepțiile bifate"
        tips.SetToolTipHeader(chkReceptii, "Recepțiile de descărcat")
        tips.SetToolTipText(chkReceptii, "Bifat: apăsarea obișnuită aduce tot; debifat: nimic până nu alegi." & vbLf & "Cerut configurabil de operator la 10.09.2026.")
        chkReceptii.UseVisualStyleBackColor = True
        '
        ' lblTitluDocumente
        '
        lblTitluDocumente.AutoSize = True
        lblTitluDocumente.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluDocumente.Location = New Point(28, 252)
        lblTitluDocumente.Margin = New Padding(4, 0, 4, 8)
        lblTitluDocumente.Name = "lblTitluDocumente"
        lblTitluDocumente.Size = New Size(360, 32)
        lblTitluDocumente.TabIndex = 2
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
        tlyDocumente.Controls.Add(lblAdobeMod, 0, 1)
        tlyDocumente.Controls.Add(cboAdobeMod, 1, 1)
        tlyDocumente.Controls.Add(lblAdobeInst, 0, 2)
        tlyDocumente.Controls.Add(cboAdobeInst, 1, 2)
        tlyDocumente.Controls.Add(lblAdobeDetach, 0, 3)
        tlyDocumente.Controls.Add(cboAdobeDetach, 1, 3)
        tlyDocumente.Controls.Add(chkAdobePopup, 0, 4)
        tlyDocumente.Controls.Add(lblExcelRibbon, 0, 5)
        tlyDocumente.Controls.Add(cboExcelRibbon, 1, 5)
        tlyDocumente.Dock = DockStyle.Top
        tlyDocumente.Location = New Point(28, 292)
        tlyDocumente.Margin = New Padding(4, 0, 4, 24)
        tlyDocumente.Name = "tlyDocumente"
        tlyDocumente.RowCount = 6
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.Size = New Size(904, 270)
        tlyDocumente.TabIndex = 3
        '
        ' lblAdobeMotor
        '
        lblAdobeMotor.AutoSize = True
        lblAdobeMotor.Dock = DockStyle.Fill
        lblAdobeMotor.Location = New Point(4, 0)
        lblAdobeMotor.Margin = New Padding(4, 0, 4, 10)
        lblAdobeMotor.Name = "lblAdobeMotor"
        lblAdobeMotor.Size = New Size(312, 36)
        lblAdobeMotor.TabIndex = 0
        lblAdobeMotor.Text = "PDF — motor de previzualizare"
        lblAdobeMotor.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboAdobeMotor
        '
        cboAdobeMotor.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboAdobeMotor.CornerRadius = 4
        cboAdobeMotor.DrawMode = DrawMode.OwnerDrawFixed
        cboAdobeMotor.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeMotor.FlatStyle = FlatStyle.Flat
        cboAdobeMotor.ItemHeight = 30
        cboAdobeMotor.Location = New Point(324, 0)
        cboAdobeMotor.Margin = New Padding(4, 0, 4, 10)
        cboAdobeMotor.Name = "cboAdobeMotor"
        cboAdobeMotor.Size = New Size(360, 36)
        cboAdobeMotor.TabIndex = 1
        tips.SetToolTipHeader(cboAdobeMotor, "Motorul de previzualizare PDF")
        tips.SetToolTipText(cboAdobeMotor, "«Fereastră găzduită» = fereastra Adobe mutată în panoul K-BOT (singura rulată în aplicație)." & vbLf & "«ActiveX» = controlul AcroPDF, în proces.")
        '
        ' lblAdobeMod
        '
        lblAdobeMod.AutoSize = True
        lblAdobeMod.Dock = DockStyle.Fill
        lblAdobeMod.Location = New Point(4, 46)
        lblAdobeMod.Margin = New Padding(4, 0, 4, 10)
        lblAdobeMod.Name = "lblAdobeMod"
        lblAdobeMod.Size = New Size(312, 36)
        lblAdobeMod.TabIndex = 2
        lblAdobeMod.Text = "PDF — mod vizualizator Adobe"
        lblAdobeMod.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboAdobeMod
        '
        cboAdobeMod.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboAdobeMod.CornerRadius = 4
        cboAdobeMod.DrawMode = DrawMode.OwnerDrawFixed
        cboAdobeMod.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeMod.FlatStyle = FlatStyle.Flat
        cboAdobeMod.ItemHeight = 30
        cboAdobeMod.Location = New Point(324, 46)
        cboAdobeMod.Margin = New Padding(4, 0, 4, 10)
        cboAdobeMod.Name = "cboAdobeMod"
        cboAdobeMod.Size = New Size(360, 36)
        cboAdobeMod.TabIndex = 3
        tips.SetToolTipHeader(cboAdobeMod, "Profilul de găzduire Adobe")
        tips.SetToolTipText(cboAdobeMod, "Automat: K-BOT recunoaște singur interfața Adobe (modernă / clasică)." & vbLf & "Modern / Clasic forțează rețeta, iar jurnalul spune dacă arborele o contrazice.")
        '
        ' lblAdobeInst
        '
        lblAdobeInst.AutoSize = True
        lblAdobeInst.Dock = DockStyle.Fill
        lblAdobeInst.Location = New Point(4, 92)
        lblAdobeInst.Margin = New Padding(4, 0, 4, 10)
        lblAdobeInst.Name = "lblAdobeInst"
        lblAdobeInst.Size = New Size(312, 36)
        lblAdobeInst.TabIndex = 4
        lblAdobeInst.Text = "PDF — instanță nouă Adobe (/n)"
        lblAdobeInst.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboAdobeInst
        '
        cboAdobeInst.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboAdobeInst.CornerRadius = 4
        cboAdobeInst.DrawMode = DrawMode.OwnerDrawFixed
        cboAdobeInst.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeInst.FlatStyle = FlatStyle.Flat
        cboAdobeInst.ItemHeight = 30
        cboAdobeInst.Location = New Point(324, 92)
        cboAdobeInst.Margin = New Padding(4, 0, 4, 10)
        cboAdobeInst.Name = "cboAdobeInst"
        cboAdobeInst.Size = New Size(360, 36)
        cboAdobeInst.TabIndex = 5
        tips.SetToolTipHeader(cboAdobeInst, "Comutatorul /n")
        tips.SetToolTipText(cboAdobeInst, "Da: Adobe pornește un proces nou, al K-BOT." & vbLf & "Nu: Adobe poate preda documentul unei instanțe deja deschise de tine." & vbLf & "Automat: decide profilul.")
        '
        ' lblAdobeDetach
        '
        lblAdobeDetach.AutoSize = True
        lblAdobeDetach.Dock = DockStyle.Fill
        lblAdobeDetach.Location = New Point(4, 138)
        lblAdobeDetach.Margin = New Padding(4, 0, 4, 10)
        lblAdobeDetach.Name = "lblAdobeDetach"
        lblAdobeDetach.Size = New Size(312, 36)
        lblAdobeDetach.TabIndex = 6
        lblAdobeDetach.Text = "PDF — la schimbarea documentului"
        lblAdobeDetach.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboAdobeDetach
        '
        cboAdobeDetach.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboAdobeDetach.CornerRadius = 4
        cboAdobeDetach.DrawMode = DrawMode.OwnerDrawFixed
        cboAdobeDetach.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeDetach.FlatStyle = FlatStyle.Flat
        cboAdobeDetach.ItemHeight = 30
        cboAdobeDetach.Location = New Point(324, 138)
        cboAdobeDetach.Margin = New Padding(4, 0, 4, 10)
        cboAdobeDetach.Name = "cboAdobeDetach"
        cboAdobeDetach.Size = New Size(360, 36)
        cboAdobeDetach.TabIndex = 7
        tips.SetToolTipHeader(cboAdobeDetach, "Cum se eliberează fereastra Adobe")
        tips.SetToolTipText(cboAdobeDetach, "A: oprește procesul pornit de K-BOT — determinist." & vbLf & "B: închide doar fereastra și lasă procesul cald, deci următorul document pornește mai repede.")
        '
        ' chkAdobePopup
        '
        chkAdobePopup.AutoSize = True
        tlyDocumente.SetColumnSpan(chkAdobePopup, 2)
        chkAdobePopup.Location = New Point(4, 184)
        chkAdobePopup.Margin = New Padding(4, 0, 4, 10)
        chkAdobePopup.Name = "chkAdobePopup"
        chkAdobePopup.Size = New Size(420, 29)
        chkAdobePopup.TabIndex = 8
        chkAdobePopup.Text = "PDF — ascunde fereastra plutitoare a Adobe (insigna de peste document)"
        tips.SetToolTipHeader(chkAdobePopup, "Fereastra plutitoare")
        tips.SetToolTipText(chkAdobePopup, "Un supraveghetor mătură ecranul la 500 ms și ascunde ferestrele AVL_AVPopup ale Adobe cât timp documentul e afișat.")
        chkAdobePopup.UseVisualStyleBackColor = True
        '
        ' lblExcelRibbon
        '
        lblExcelRibbon.AutoSize = True
        lblExcelRibbon.Dock = DockStyle.Fill
        lblExcelRibbon.Location = New Point(4, 223)
        lblExcelRibbon.Margin = New Padding(4, 0, 4, 10)
        lblExcelRibbon.Name = "lblExcelRibbon"
        lblExcelRibbon.Size = New Size(312, 36)
        lblExcelRibbon.TabIndex = 9
        lblExcelRibbon.Text = "Excel — cum se ascunde panglica"
        lblExcelRibbon.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboExcelRibbon
        '
        cboExcelRibbon.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboExcelRibbon.CornerRadius = 4
        cboExcelRibbon.DrawMode = DrawMode.OwnerDrawFixed
        cboExcelRibbon.DropDownStyle = ComboBoxStyle.DropDownList
        cboExcelRibbon.FlatStyle = FlatStyle.Flat
        cboExcelRibbon.ItemHeight = 30
        cboExcelRibbon.Location = New Point(324, 223)
        cboExcelRibbon.Margin = New Padding(4, 0, 4, 10)
        cboExcelRibbon.Name = "cboExcelRibbon"
        cboExcelRibbon.Size = New Size(360, 36)
        cboExcelRibbon.TabIndex = 10
        tips.SetToolTipHeader(cboExcelRibbon, "Panglica Excel în previzualizare")
        tips.SetToolTipText(cboExcelRibbon, "Macro: Excel își ascunde singur panglica (poate fi refuzat de o politică)." & vbLf & "Fereastră: se ascunde fereastra panglicii, ca la Word — nu poate fi refuzat." & vbLf & "Word are o singură metodă și nu se configurează.")
        '
        ' lblTitluFoldere
        '
        lblTitluFoldere.AutoSize = True
        lblTitluFoldere.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluFoldere.Location = New Point(28, 586)
        lblTitluFoldere.Margin = New Padding(4, 0, 4, 4)
        lblTitluFoldere.Name = "lblTitluFoldere"
        lblTitluFoldere.Size = New Size(93, 32)
        lblTitluFoldere.TabIndex = 4
        lblTitluFoldere.Text = "Foldere"
        '
        ' lblFoldereHint
        '
        lblFoldereHint.AutoSize = True
        lblFoldereHint.Dock = DockStyle.Top
        lblFoldereHint.Location = New Point(28, 622)
        lblFoldereHint.Margin = New Padding(4, 0, 4, 8)
        lblFoldereHint.Name = "lblFoldereHint"
        lblFoldereHint.Size = New Size(904, 25)
        lblFoldereHint.TabIndex = 5
        lblFoldereHint.Text = "Calea goală înseamnă «implicit». O cale relativă se rezolvă față de folderul aplicației. Folderele se verifică la pornire — schimbarea are efect la următoarea pornire."
        '
        ' gridFoldere
        '
        gridFoldere.AlternatingRows = True
        gridFoldere.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridFoldere.ColumnFillMode = KBotFillMode.LastColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Setare"
        KBotDataColumn1.Key = "cheie"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 150
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Ce este"
        KBotDataColumn2.Key = "descriere"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 330
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Implicit"
        KBotDataColumn3.Key = "implicit"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 190
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Calea configurată"
        KBotDataColumn4.Key = "cale"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.Width = 230
        gridFoldere.Columns.Add(KBotDataColumn1)
        gridFoldere.Columns.Add(KBotDataColumn2)
        gridFoldere.Columns.Add(KBotDataColumn3)
        gridFoldere.Columns.Add(KBotDataColumn4)
        gridFoldere.Dock = DockStyle.Fill
        gridFoldere.EnterKeyMode = KBotEnterKeyMode.NextRow
        gridFoldere.HeaderHeight = 30
        gridFoldere.Location = New Point(28, 655)
        gridFoldere.Margin = New Padding(4, 0, 4, 8)
        gridFoldere.Name = "gridFoldere"
        gridFoldere.RowHeight = 28
        gridFoldere.Size = New Size(904, 442)
        gridFoldere.TabIndex = 6
        tips.SetToolTipHeader(gridFoldere, "Folderele în care scrie aplicația")
        tips.SetToolTipText(gridFoldere, "Doar coloana «Calea configurată» se editează. Apasă «Salvează folderele» după.")
        '
        ' tlyFoldereButoane
        '
        tlyFoldereButoane.AutoFitToTheme = False
        tlyFoldereButoane.AutoSize = True
        tlyFoldereButoane.ColumnCount = 2
        tlyFoldereButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFoldereButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 240F))
        tlyFoldereButoane.Controls.Add(lblFoldereStare, 0, 0)
        tlyFoldereButoane.Controls.Add(btnSalveazaFoldere, 1, 0)
        tlyFoldereButoane.Dock = DockStyle.Top
        tlyFoldereButoane.Location = New Point(28, 773)
        tlyFoldereButoane.Margin = New Padding(4, 0, 4, 0)
        tlyFoldereButoane.Name = "tlyFoldereButoane"
        tlyFoldereButoane.RowCount = 1
        tlyFoldereButoane.RowStyles.Add(New RowStyle())
        tlyFoldereButoane.Size = New Size(904, 49)
        tlyFoldereButoane.TabIndex = 7
        '
        ' lblFoldereStare
        '
        lblFoldereStare.AutoEllipsis = True
        lblFoldereStare.Dock = DockStyle.Fill
        lblFoldereStare.Location = New Point(4, 0)
        lblFoldereStare.Margin = New Padding(4, 0, 4, 0)
        lblFoldereStare.Name = "lblFoldereStare"
        lblFoldereStare.Size = New Size(656, 49)
        lblFoldereStare.TabIndex = 0
        lblFoldereStare.Text = ""
        lblFoldereStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnSalveazaFoldere
        '
        btnSalveazaFoldere.Dock = DockStyle.Fill
        btnSalveazaFoldere.Enabled = False
        btnSalveazaFoldere.FlatStyle = FlatStyle.Flat
        btnSalveazaFoldere.Font = New Font("Segoe UI Semibold", 9F)
        btnSalveazaFoldere.Location = New Point(668, 0)
        btnSalveazaFoldere.Margin = New Padding(4, 0, 4, 0)
        btnSalveazaFoldere.Name = "btnSalveazaFoldere"
        btnSalveazaFoldere.Size = New Size(232, 49)
        btnSalveazaFoldere.TabIndex = 1
        btnSalveazaFoldere.Text = "Salvează folderele"
        tips.SetToolTipHeader(btnSalveazaFoldere, "Salvează folderele")
        tips.SetToolTipText(btnSalveazaFoldere, "Scrie căile în settings.json (AppData). Se verifică la următoarea pornire.")
        btnSalveazaFoldere.UseVisualStyleBackColor = True
        '
        ' SetariAplicatieView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyBody)
        Name = "SetariAplicatieView"
        Size = New Size(960, 840)
        tlyBody.ResumeLayout(False)
        tlyBody.PerformLayout()
        tlyComutatoare.ResumeLayout(False)
        tlyComutatoare.PerformLayout()
        tlyDocumente.ResumeLayout(False)
        tlyDocumente.PerformLayout()
        CType(gridFoldere, ComponentModel.ISupportInitialize).EndInit()
        tlyFoldereButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents lblTitluComutatoare As Label
    Friend WithEvents tlyComutatoare As KBotTableLayoutPanel
    Friend WithEvents lblVerbose As Label
    Friend WithEvents cboVerbose As KBotComboBox
    Friend WithEvents chkLogViewer As CheckBox
    Friend WithEvents chkShowBrowser As CheckBox
    Friend WithEvents chkReceptii As CheckBox
    Friend WithEvents lblTitluDocumente As Label
    Friend WithEvents tlyDocumente As KBotTableLayoutPanel
    Friend WithEvents lblAdobeMotor As Label
    Friend WithEvents cboAdobeMotor As KBotComboBox
    Friend WithEvents lblAdobeMod As Label
    Friend WithEvents cboAdobeMod As KBotComboBox
    Friend WithEvents lblAdobeInst As Label
    Friend WithEvents cboAdobeInst As KBotComboBox
    Friend WithEvents lblAdobeDetach As Label
    Friend WithEvents cboAdobeDetach As KBotComboBox
    Friend WithEvents chkAdobePopup As CheckBox
    Friend WithEvents lblExcelRibbon As Label
    Friend WithEvents cboExcelRibbon As KBotComboBox
    Friend WithEvents lblTitluFoldere As Label
    Friend WithEvents lblFoldereHint As Label
    Friend WithEvents gridFoldere As KBotDataView
    Friend WithEvents tlyFoldereButoane As KBotTableLayoutPanel
    Friend WithEvents lblFoldereStare As Label
    Friend WithEvents btnSalveazaFoldere As Button
End Class
