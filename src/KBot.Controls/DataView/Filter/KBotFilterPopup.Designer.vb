Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Partea „designer” a <see cref="KBotFilterPopup"/> (slice 0028-06, refăcută în 0030). Conform
''' regulii casei, TOATE controalele copil se declară AICI, nu se construiesc în cod — meniul se
''' deschide și se citește pe suprafața de proiectare din Visual Studio, iar fonturile, mărimile și
''' așezarea se schimbă acolo, nu prin constante într-un fișier de pictură.
'''
''' <para><b>Trei FILE, nu patru etaje.</b> Meniul avea comenzile una sub alta, într-o coloană care
''' creștea cu fiecare felie. Din 0030 sunt trei file — <b>Sortare</b>, <b>Filtrare</b>,
''' <b>Grupare</b> — alese dintr-un <see cref="KBotNavList"/> orizontal pus sus. Fiecare filă e un
''' <c>Panel</c> andocat <c>Fill</c> peste același loc; se vede una singură, iar
''' <c>KBotFilterPopup.vb</c> le comută. <b>Pe suprafața de proiectare</b> ele stau una peste alta,
''' deci ca să lucrezi la una îi pui <c>Visible = True</c> din grila de proprietăți (și celorlalte
''' False) — exact ca la paginile unui TabControl, doar că fără TabControl-ul netematizabil.</para>
'''
''' <para><b>Lista de valori și lista de niveluri sunt declarate tot aici; doar CONȚINUTUL lor vine
''' la rulare.</b> Valorile distincte ale unei coloane și nivelurile de grupare ale grilei nu există
''' la proiectare — dar controlul care le arată, fontul lui, înălțimea rândului și locul din
''' fereastră sunt ale designerului, ca orice altceva.</para>
'''
''' <para><b>Ordinea de andocare e INVERSĂ celei vizuale</b> (regula casei pentru un panou-card):
''' ultimul <c>Dock = Top</c> adăugat ajunge cel mai sus. De aceea în <c>InitializeComponent</c>
''' controalele intră în ordinea „de jos în sus”: gazda filelor (Fill), bara de butoane (Bottom),
''' linia de sub navigație, apoi navigația.</para>
'''
''' <para><b>Rândurile tabelelor sunt AUTORATE pe schema Classic</b> — designerul nu știe nimic
''' despre motorul de teme. Cât cresc ele sub o schemă cu umplutură (Modern) e treaba lui
''' <c>ThemeTableFit</c>, chemat din <c>OnThemeChanged</c>; aici rămân măsurile alese cu ochiul.</para>
'''
''' <para>Fereastra e fără chenar (e un meniu), iar «chenarul» de 1px se obține din
''' <c>Padding</c>-ul formularului plus fundalul lui: culoarea vine din temă, în
''' <c>OnThemeChanged</c>, deci nu există nicio culoare scrisă aici.</para>
''' </summary>
Partial Class KBotFilterPopup
    Inherits KBot.Theming.KBotThemedForm

    Private components As System.ComponentModel.IContainer

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private Sub InitializeComponent()
        Dim navSortare As New KBotNavItem()
        Dim navFiltrare As New KBotNavItem()
        Dim navGrupare As New KBotNavItem()
        pnlCorp = New Panel()
        pnlFile = New Panel()
        pnlSortare = New Panel()
        tlySortare = New TableLayoutPanel()
        btnSortAsc = New Button()
        btnSortDesc = New Button()
        sepSortare = New Panel()
        btnSortClear = New Button()
        pnlFiltrare = New Panel()
        tlyFiltrare = New TableLayoutPanel()
        picCauta = New PictureBox()
        txtCauta = New KBotTextField()
        chkSelecteazaTot = New CheckBox()
        lstValori = New CheckedListBox()
        sepFiltrare = New Panel()
        btnConditii = New Button()
        btnStergeFiltru = New Button()
        pnlGrupare = New Panel()
        tlyGrupare = New TableLayoutPanel()
        chkGrupeaza = New CheckBox()
        pnlSensGrup = New Panel()
        rbGrupCresc = New RadioButton()
        rbGrupDesc = New RadioButton()
        sepGrupare = New Panel()
        chkGrupAntet = New CheckBox()
        chkGrupSubsol = New CheckBox()
        chkGrupAgregate = New CheckBox()
        chkGrupStrangere = New CheckBox()
        chkGrupPornitStrans = New CheckBox()
        lblNiveluri = New Label()
        lstNiveluri = New ListBox()
        pnlButoane = New Panel()
        btnOk = New Button()
        btnAnuleaza = New Button()
        sepNav = New Panel()
        navFile = New KBotNavList()
        pnlCorp.SuspendLayout()
        pnlFile.SuspendLayout()
        pnlSortare.SuspendLayout()
        tlySortare.SuspendLayout()
        pnlFiltrare.SuspendLayout()
        tlyFiltrare.SuspendLayout()
        CType(picCauta, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlGrupare.SuspendLayout()
        tlyGrupare.SuspendLayout()
        pnlSensGrup.SuspendLayout()
        pnlButoane.SuspendLayout()
        CType(navFile, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pnlCorp
        '
        pnlCorp.Controls.Add(pnlFile)
        pnlCorp.Controls.Add(pnlButoane)
        pnlCorp.Controls.Add(sepNav)
        pnlCorp.Controls.Add(navFile)
        pnlCorp.Dock = DockStyle.Fill
        pnlCorp.Location = New Point(1, 1)
        pnlCorp.Margin = New Padding(0)
        pnlCorp.Name = "pnlCorp"
        pnlCorp.Padding = New Padding(2)
        pnlCorp.Size = New Size(338, 558)
        pnlCorp.TabIndex = 0
        '
        ' pnlFile
        '
        pnlFile.Controls.Add(pnlGrupare)
        pnlFile.Controls.Add(pnlSortare)
        pnlFile.Controls.Add(pnlFiltrare)
        pnlFile.Dock = DockStyle.Fill
        pnlFile.Location = New Point(2, 41)
        pnlFile.Margin = New Padding(0)
        pnlFile.Name = "pnlFile"
        pnlFile.Size = New Size(334, 452)
        pnlFile.TabIndex = 2
        '
        ' pnlSortare
        '
        pnlSortare.Controls.Add(tlySortare)
        pnlSortare.Dock = DockStyle.Fill
        pnlSortare.Location = New Point(0, 0)
        pnlSortare.Margin = New Padding(0)
        pnlSortare.Name = "pnlSortare"
        pnlSortare.Size = New Size(334, 452)
        pnlSortare.TabIndex = 1
        pnlSortare.Visible = False
        '
        ' tlySortare
        '
        tlySortare.ColumnCount = 1
        tlySortare.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlySortare.Controls.Add(btnSortAsc, 0, 0)
        tlySortare.Controls.Add(btnSortDesc, 0, 1)
        tlySortare.Controls.Add(sepSortare, 0, 2)
        tlySortare.Controls.Add(btnSortClear, 0, 3)
        tlySortare.Dock = DockStyle.Fill
        tlySortare.Location = New Point(0, 0)
        tlySortare.Margin = New Padding(0)
        tlySortare.Name = "tlySortare"
        tlySortare.RowCount = 5
        tlySortare.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0F))
        tlySortare.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0F))
        tlySortare.RowStyles.Add(New RowStyle(SizeType.Absolute, 9.0F))
        tlySortare.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0F))
        tlySortare.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlySortare.Size = New Size(334, 452)
        tlySortare.TabIndex = 0
        '
        ' btnSortAsc
        '
        btnSortAsc.Cursor = Cursors.Hand
        btnSortAsc.Dock = DockStyle.Top
        btnSortAsc.FlatAppearance.BorderSize = 0
        btnSortAsc.FlatStyle = FlatStyle.Flat
        btnSortAsc.Image = My.Resources.Resources.sort_asc
        btnSortAsc.ImageAlign = ContentAlignment.MiddleLeft
        btnSortAsc.Location = New Point(0, 0)
        btnSortAsc.Margin = New Padding(0)
        btnSortAsc.Name = "btnSortAsc"
        btnSortAsc.Size = New Size(334, 40)
        btnSortAsc.TabIndex = 0
        btnSortAsc.Text = " Sortează &Crescător"
        btnSortAsc.TextAlign = ContentAlignment.MiddleLeft
        btnSortAsc.TextImageRelation = TextImageRelation.ImageBeforeText
        btnSortAsc.UseVisualStyleBackColor = True
        '
        ' btnSortDesc
        '
        btnSortDesc.Cursor = Cursors.Hand
        btnSortDesc.Dock = DockStyle.Top
        btnSortDesc.FlatAppearance.BorderSize = 0
        btnSortDesc.FlatStyle = FlatStyle.Flat
        btnSortDesc.Image = My.Resources.Resources.sort_desc
        btnSortDesc.ImageAlign = ContentAlignment.MiddleLeft
        btnSortDesc.Location = New Point(0, 40)
        btnSortDesc.Margin = New Padding(0)
        btnSortDesc.Name = "btnSortDesc"
        btnSortDesc.Size = New Size(334, 40)
        btnSortDesc.TabIndex = 1
        btnSortDesc.Text = " Sortează &Descrescător"
        btnSortDesc.TextAlign = ContentAlignment.MiddleLeft
        btnSortDesc.TextImageRelation = TextImageRelation.ImageBeforeText
        btnSortDesc.UseVisualStyleBackColor = True
        '
        ' sepSortare
        '
        sepSortare.Dock = DockStyle.Top
        sepSortare.Location = New Point(6, 84)
        sepSortare.Margin = New Padding(6, 4, 6, 4)
        sepSortare.Name = "sepSortare"
        sepSortare.Size = New Size(322, 1)
        sepSortare.TabIndex = 2
        '
        ' btnSortClear
        '
        btnSortClear.Cursor = Cursors.Hand
        btnSortClear.Dock = DockStyle.Top
        btnSortClear.FlatAppearance.BorderSize = 0
        btnSortClear.FlatStyle = FlatStyle.Flat
        btnSortClear.Image = My.Resources.Resources.sort_clear
        btnSortClear.ImageAlign = ContentAlignment.MiddleLeft
        btnSortClear.Location = New Point(0, 89)
        btnSortClear.Margin = New Padding(0)
        btnSortClear.Name = "btnSortClear"
        btnSortClear.Size = New Size(334, 40)
        btnSortClear.TabIndex = 3
        btnSortClear.Text = " &Resetează sortarea"
        btnSortClear.TextAlign = ContentAlignment.MiddleLeft
        btnSortClear.TextImageRelation = TextImageRelation.ImageBeforeText
        btnSortClear.UseVisualStyleBackColor = True
        '
        ' pnlFiltrare
        '
        pnlFiltrare.Controls.Add(tlyFiltrare)
        pnlFiltrare.Dock = DockStyle.Fill
        pnlFiltrare.Location = New Point(0, 0)
        pnlFiltrare.Margin = New Padding(0)
        pnlFiltrare.Name = "pnlFiltrare"
        pnlFiltrare.Size = New Size(334, 452)
        pnlFiltrare.TabIndex = 0
        '
        ' tlyFiltrare
        '
        tlyFiltrare.ColumnCount = 2
        tlyFiltrare.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 36.0F))
        tlyFiltrare.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlyFiltrare.Controls.Add(picCauta, 0, 0)
        tlyFiltrare.Controls.Add(txtCauta, 1, 0)
        tlyFiltrare.Controls.Add(chkSelecteazaTot, 0, 1)
        tlyFiltrare.Controls.Add(lstValori, 0, 2)
        tlyFiltrare.Controls.Add(sepFiltrare, 0, 3)
        tlyFiltrare.Controls.Add(btnConditii, 0, 4)
        tlyFiltrare.Controls.Add(btnStergeFiltru, 0, 5)
        tlyFiltrare.Dock = DockStyle.Fill
        tlyFiltrare.Location = New Point(0, 0)
        tlyFiltrare.Margin = New Padding(0)
        tlyFiltrare.Name = "tlyFiltrare"
        tlyFiltrare.RowCount = 6
        tlyFiltrare.RowStyles.Add(New RowStyle(SizeType.Absolute, 50.0F))
        tlyFiltrare.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
        tlyFiltrare.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlyFiltrare.RowStyles.Add(New RowStyle(SizeType.Absolute, 9.0F))
        tlyFiltrare.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0F))
        tlyFiltrare.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0F))
        tlyFiltrare.Size = New Size(334, 452)
        tlyFiltrare.TabIndex = 0
        '
        ' picCauta
        '
        picCauta.BackColor = Color.Transparent
        picCauta.Dock = DockStyle.Fill
        picCauta.Image = My.Resources.Resources.filter_search
        picCauta.Location = New Point(0, 0)
        picCauta.Margin = New Padding(0)
        picCauta.Name = "picCauta"
        picCauta.Size = New Size(36, 50)
        picCauta.SizeMode = PictureBoxSizeMode.CenterImage
        picCauta.TabIndex = 0
        picCauta.TabStop = False
        '
        ' txtCauta
        '
        txtCauta.BackColor = Color.Transparent
        txtCauta.Dock = DockStyle.Top
        txtCauta.Location = New Point(42, 6)
        txtCauta.Margin = New Padding(6)
        txtCauta.MaxLength = 32767
        txtCauta.Name = "txtCauta"
        txtCauta.PlaceholderText = "Caută…"
        txtCauta.Size = New Size(286, 38)
        txtCauta.TabIndex = 0
        txtCauta.UseSystemPasswordChar = False
        '
        ' chkSelecteazaTot
        '
        chkSelecteazaTot.AutoSize = True
        chkSelecteazaTot.Font = New Font("Consolas", 9.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tlyFiltrare.SetColumnSpan(chkSelecteazaTot, 2)
        chkSelecteazaTot.Dock = DockStyle.Top
        chkSelecteazaTot.Location = New Point(8, 56)
        chkSelecteazaTot.Margin = New Padding(8, 6, 0, 0)
        chkSelecteazaTot.Name = "chkSelecteazaTot"
        chkSelecteazaTot.Size = New Size(326, 19)
        chkSelecteazaTot.TabIndex = 1
        chkSelecteazaTot.Text = "(Selectează tot)"
        chkSelecteazaTot.ThreeState = True
        chkSelecteazaTot.UseVisualStyleBackColor = True
        '
        ' lstValori
        '
        lstValori.BorderStyle = BorderStyle.None
        lstValori.CheckOnClick = True
        tlyFiltrare.SetColumnSpan(lstValori, 2)
        lstValori.Dock = DockStyle.Fill
        lstValori.Font = New Font("Consolas", 9.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lstValori.IntegralHeight = False
        lstValori.Location = New Point(6, 84)
        lstValori.Margin = New Padding(6, 0, 0, 0)
        lstValori.Name = "lstValori"
        lstValori.Size = New Size(328, 269)
        lstValori.TabIndex = 2
        '
        ' sepFiltrare
        '
        tlyFiltrare.SetColumnSpan(sepFiltrare, 2)
        sepFiltrare.Dock = DockStyle.Top
        sepFiltrare.Location = New Point(6, 357)
        sepFiltrare.Margin = New Padding(6, 4, 6, 4)
        sepFiltrare.Name = "sepFiltrare"
        sepFiltrare.Size = New Size(322, 1)
        sepFiltrare.TabIndex = 3
        '
        ' btnConditii
        '
        tlyFiltrare.SetColumnSpan(btnConditii, 2)
        btnConditii.Cursor = Cursors.Hand
        btnConditii.Dock = DockStyle.Top
        btnConditii.FlatAppearance.BorderSize = 0
        btnConditii.FlatStyle = FlatStyle.Flat
        btnConditii.Image = My.Resources.Resources.filter_edit
        btnConditii.ImageAlign = ContentAlignment.MiddleLeft
        btnConditii.Location = New Point(0, 362)
        btnConditii.Margin = New Padding(0)
        btnConditii.Name = "btnConditii"
        btnConditii.Size = New Size(334, 40)
        btnConditii.TabIndex = 4
        btnConditii.Text = " Operatori filtru"
        btnConditii.TextAlign = ContentAlignment.MiddleLeft
        btnConditii.TextImageRelation = TextImageRelation.ImageBeforeText
        btnConditii.UseVisualStyleBackColor = True
        '
        ' btnStergeFiltru
        '
        tlyFiltrare.SetColumnSpan(btnStergeFiltru, 2)
        btnStergeFiltru.Cursor = Cursors.Hand
        btnStergeFiltru.Dock = DockStyle.Top
        btnStergeFiltru.FlatAppearance.BorderSize = 0
        btnStergeFiltru.FlatStyle = FlatStyle.Flat
        btnStergeFiltru.Image = My.Resources.Resources.filter_delete
        btnStergeFiltru.ImageAlign = ContentAlignment.MiddleLeft
        btnStergeFiltru.Location = New Point(0, 402)
        btnStergeFiltru.Margin = New Padding(0)
        btnStergeFiltru.Name = "btnStergeFiltru"
        btnStergeFiltru.Size = New Size(334, 40)
        btnStergeFiltru.TabIndex = 5
        btnStergeFiltru.Text = " Șterge &Filtrul"
        btnStergeFiltru.TextAlign = ContentAlignment.MiddleLeft
        btnStergeFiltru.TextImageRelation = TextImageRelation.ImageBeforeText
        btnStergeFiltru.UseVisualStyleBackColor = True
        '
        ' pnlGrupare
        '
        pnlGrupare.Controls.Add(tlyGrupare)
        pnlGrupare.Dock = DockStyle.Fill
        pnlGrupare.Location = New Point(0, 0)
        pnlGrupare.Margin = New Padding(0)
        pnlGrupare.Name = "pnlGrupare"
        pnlGrupare.Size = New Size(334, 452)
        pnlGrupare.TabIndex = 2
        pnlGrupare.Visible = False
        '
        ' tlyGrupare
        '
        tlyGrupare.ColumnCount = 1
        tlyGrupare.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlyGrupare.Controls.Add(chkGrupeaza, 0, 0)
        tlyGrupare.Controls.Add(pnlSensGrup, 0, 1)
        tlyGrupare.Controls.Add(sepGrupare, 0, 2)
        tlyGrupare.Controls.Add(chkGrupAntet, 0, 3)
        tlyGrupare.Controls.Add(chkGrupSubsol, 0, 4)
        tlyGrupare.Controls.Add(chkGrupAgregate, 0, 5)
        tlyGrupare.Controls.Add(chkGrupStrangere, 0, 6)
        tlyGrupare.Controls.Add(chkGrupPornitStrans, 0, 7)
        tlyGrupare.Controls.Add(lblNiveluri, 0, 8)
        tlyGrupare.Controls.Add(lstNiveluri, 0, 9)
        tlyGrupare.Dock = DockStyle.Fill
        tlyGrupare.Location = New Point(0, 0)
        tlyGrupare.Margin = New Padding(0)
        tlyGrupare.Name = "tlyGrupare"
        tlyGrupare.RowCount = 10
        tlyGrupare.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
        tlyGrupare.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))
        tlyGrupare.RowStyles.Add(New RowStyle(SizeType.Absolute, 13.0F))
        tlyGrupare.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        tlyGrupare.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        tlyGrupare.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        tlyGrupare.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        tlyGrupare.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        tlyGrupare.RowStyles.Add(New RowStyle(SizeType.Absolute, 26.0F))
        tlyGrupare.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlyGrupare.Size = New Size(334, 452)
        tlyGrupare.TabIndex = 0
        '
        ' chkGrupeaza
        '
        chkGrupeaza.AutoSize = True
        chkGrupeaza.Dock = DockStyle.Top
        chkGrupeaza.Location = New Point(8, 8)
        chkGrupeaza.Margin = New Padding(8, 8, 8, 0)
        chkGrupeaza.Name = "chkGrupeaza"
        chkGrupeaza.Size = New Size(318, 19)
        chkGrupeaza.TabIndex = 0
        chkGrupeaza.Text = "Grupează după coloana aceasta"
        chkGrupeaza.UseVisualStyleBackColor = True
        '
        ' pnlSensGrup
        '
        ' AutoSize, nu Dock=Fill: pe o schemă cu alt font butoanele radio cresc, iar un panou care
        ' își ia înălțimea din celulă n-are cum să spună asta mai departe. Așa ThemeTableFit îl
        ' întreabă cât cere și mărește rândul cu exact atât.
        pnlSensGrup.AutoSize = True
        pnlSensGrup.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlSensGrup.Controls.Add(rbGrupDesc)
        pnlSensGrup.Controls.Add(rbGrupCresc)
        pnlSensGrup.Dock = DockStyle.Top
        pnlSensGrup.Location = New Point(26, 34)
        pnlSensGrup.Margin = New Padding(26, 0, 8, 0)
        pnlSensGrup.Name = "pnlSensGrup"
        pnlSensGrup.Size = New Size(300, 25)
        pnlSensGrup.TabIndex = 1
        '
        ' rbGrupCresc
        '
        rbGrupCresc.AutoSize = True
        rbGrupCresc.Location = New Point(0, 3)
        rbGrupCresc.Name = "rbGrupCresc"
        rbGrupCresc.Size = New Size(84, 19)
        rbGrupCresc.TabIndex = 0
        rbGrupCresc.TabStop = True
        rbGrupCresc.Text = "Crescător"
        rbGrupCresc.UseVisualStyleBackColor = True
        '
        ' rbGrupDesc
        '
        rbGrupDesc.AutoSize = True
        rbGrupDesc.Location = New Point(110, 3)
        rbGrupDesc.Name = "rbGrupDesc"
        rbGrupDesc.Size = New Size(102, 19)
        rbGrupDesc.TabIndex = 1
        rbGrupDesc.Text = "Descrescător"
        rbGrupDesc.UseVisualStyleBackColor = True
        '
        ' sepGrupare
        '
        sepGrupare.Dock = DockStyle.Top
        sepGrupare.Location = New Point(6, 70)
        sepGrupare.Margin = New Padding(6, 6, 6, 6)
        sepGrupare.Name = "sepGrupare"
        sepGrupare.Size = New Size(322, 1)
        sepGrupare.TabIndex = 2
        '
        ' chkGrupAntet
        '
        chkGrupAntet.AutoSize = True
        chkGrupAntet.Dock = DockStyle.Top
        chkGrupAntet.Location = New Point(26, 77)
        chkGrupAntet.Margin = New Padding(26, 0, 8, 0)
        chkGrupAntet.Name = "chkGrupAntet"
        chkGrupAntet.Size = New Size(300, 19)
        chkGrupAntet.TabIndex = 3
        chkGrupAntet.Text = "Bandă de antet (titlul grupului)"
        chkGrupAntet.UseVisualStyleBackColor = True
        '
        ' chkGrupSubsol
        '
        chkGrupSubsol.AutoSize = True
        chkGrupSubsol.Dock = DockStyle.Top
        chkGrupSubsol.Location = New Point(26, 105)
        chkGrupSubsol.Margin = New Padding(26, 0, 8, 0)
        chkGrupSubsol.Name = "chkGrupSubsol"
        chkGrupSubsol.Size = New Size(300, 19)
        chkGrupSubsol.TabIndex = 4
        chkGrupSubsol.Text = "Bandă de subsol (totalurile grupului)"
        chkGrupSubsol.UseVisualStyleBackColor = True
        '
        ' chkGrupAgregate
        '
        chkGrupAgregate.AutoSize = True
        chkGrupAgregate.Dock = DockStyle.Top
        chkGrupAgregate.Location = New Point(26, 133)
        chkGrupAgregate.Margin = New Padding(26, 0, 8, 0)
        chkGrupAgregate.Name = "chkGrupAgregate"
        chkGrupAgregate.Size = New Size(300, 19)
        chkGrupAgregate.TabIndex = 5
        chkGrupAgregate.Text = "Totalurile și în antet (se văd și strâns)"
        chkGrupAgregate.UseVisualStyleBackColor = True
        '
        ' chkGrupStrangere
        '
        chkGrupStrangere.AutoSize = True
        chkGrupStrangere.Dock = DockStyle.Top
        chkGrupStrangere.Location = New Point(26, 161)
        chkGrupStrangere.Margin = New Padding(26, 0, 8, 0)
        chkGrupStrangere.Name = "chkGrupStrangere"
        chkGrupStrangere.Size = New Size(300, 19)
        chkGrupStrangere.TabIndex = 6
        chkGrupStrangere.Text = "Grupurile se pot strânge"
        chkGrupStrangere.UseVisualStyleBackColor = True
        '
        ' chkGrupPornitStrans
        '
        chkGrupPornitStrans.AutoSize = True
        chkGrupPornitStrans.Dock = DockStyle.Top
        chkGrupPornitStrans.Location = New Point(26, 189)
        chkGrupPornitStrans.Margin = New Padding(26, 0, 8, 0)
        chkGrupPornitStrans.Name = "chkGrupPornitStrans"
        chkGrupPornitStrans.Size = New Size(300, 19)
        chkGrupPornitStrans.TabIndex = 7
        chkGrupPornitStrans.Text = "Pornesc strânse"
        chkGrupPornitStrans.UseVisualStyleBackColor = True
        '
        ' lblNiveluri
        '
        lblNiveluri.AutoSize = True
        lblNiveluri.Dock = DockStyle.Top
        lblNiveluri.Location = New Point(8, 217)
        lblNiveluri.Margin = New Padding(8, 6, 8, 0)
        lblNiveluri.Name = "lblNiveluri"
        lblNiveluri.Size = New Size(318, 15)
        lblNiveluri.TabIndex = 8
        lblNiveluri.Text = "Niveluri de grupare, de la cel dinafară:"
        lblNiveluri.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lstNiveluri
        '
        lstNiveluri.BorderStyle = BorderStyle.None
        lstNiveluri.Dock = DockStyle.Fill
        lstNiveluri.IntegralHeight = False
        lstNiveluri.Location = New Point(8, 243)
        lstNiveluri.Margin = New Padding(8, 0, 8, 6)
        lstNiveluri.Name = "lstNiveluri"
        lstNiveluri.SelectionMode = SelectionMode.None
        lstNiveluri.Size = New Size(318, 203)
        lstNiveluri.TabIndex = 9
        '
        ' pnlButoane
        '
        pnlButoane.Controls.Add(btnOk)
        pnlButoane.Controls.Add(btnAnuleaza)
        pnlButoane.Dock = DockStyle.Bottom
        pnlButoane.Location = New Point(2, 493)
        pnlButoane.Margin = New Padding(0)
        pnlButoane.Name = "pnlButoane"
        pnlButoane.Padding = New Padding(0, 8, 0, 8)
        pnlButoane.Size = New Size(334, 63)
        pnlButoane.TabIndex = 3
        '
        ' btnOk
        '
        btnOk.Dock = DockStyle.Right
        btnOk.Location = New Point(214, 8)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(120, 47)
        btnOk.TabIndex = 0
        btnOk.Text = "OK"
        btnOk.UseVisualStyleBackColor = True
        '
        ' btnAnuleaza
        '
        btnAnuleaza.DialogResult = DialogResult.Cancel
        btnAnuleaza.Dock = DockStyle.Left
        btnAnuleaza.Location = New Point(0, 8)
        btnAnuleaza.Name = "btnAnuleaza"
        btnAnuleaza.Size = New Size(120, 47)
        btnAnuleaza.TabIndex = 1
        btnAnuleaza.Text = "Anulează"
        btnAnuleaza.UseVisualStyleBackColor = True
        '
        ' sepNav
        '
        sepNav.Dock = DockStyle.Top
        sepNav.Location = New Point(2, 40)
        sepNav.Name = "sepNav"
        sepNav.Size = New Size(334, 1)
        sepNav.TabIndex = 1
        '
        ' navFile
        '
        navSortare.Key = "sortare"
        navSortare.Text = "Sortare"
        navFiltrare.Key = "filtrare"
        navFiltrare.Text = "Filtrare"
        navGrupare.Key = "grupare"
        navGrupare.Text = "Grupare"
        navFile.Dock = DockStyle.Top
        navFile.IconSize = 0
        navFile.ItemPadding = New Padding(2)
        navFile.Items.Add(navSortare)
        navFile.Items.Add(navFiltrare)
        navFile.Items.Add(navGrupare)
        navFile.Location = New Point(2, 2)
        navFile.Margin = New Padding(0)
        navFile.Name = "navFile"
        navFile.Orientation = KBotNavOrientation.Horizontal
        navFile.SelectedKey = "filtrare"
        navFile.Size = New Size(334, 42)
        navFile.TabIndex = 0
        '
        ' KBotFilterPopup
        '
        AutoScaleMode = AutoScaleMode.None
        CancelButton = btnAnuleaza
        ClientSize = New Size(340, 560)
        ControlBox = False
        Controls.Add(pnlCorp)
        FormBorderStyle = FormBorderStyle.None
        KeyPreview = True
        MaximizeBox = False
        MinimizeBox = False
        Name = "KBotFilterPopup"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        pnlCorp.ResumeLayout(False)
        pnlFile.ResumeLayout(False)
        pnlSortare.ResumeLayout(False)
        tlySortare.ResumeLayout(False)
        pnlFiltrare.ResumeLayout(False)
        tlyFiltrare.ResumeLayout(False)
        tlyFiltrare.PerformLayout()
        CType(picCauta, System.ComponentModel.ISupportInitialize).EndInit()
        pnlGrupare.ResumeLayout(False)
        tlyGrupare.ResumeLayout(False)
        tlyGrupare.PerformLayout()
        pnlSensGrup.ResumeLayout(False)
        pnlSensGrup.PerformLayout()
        pnlButoane.ResumeLayout(False)
        CType(navFile, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlCorp As Panel
    Friend WithEvents pnlFile As Panel
    Friend WithEvents pnlSortare As Panel
    Friend WithEvents tlySortare As TableLayoutPanel
    Friend WithEvents btnSortAsc As Button
    Friend WithEvents btnSortDesc As Button
    Friend WithEvents sepSortare As Panel
    Friend WithEvents btnSortClear As Button
    Friend WithEvents pnlFiltrare As Panel
    Friend WithEvents tlyFiltrare As TableLayoutPanel
    Friend WithEvents picCauta As PictureBox
    Friend WithEvents txtCauta As KBotTextField
    Friend WithEvents chkSelecteazaTot As CheckBox
    Friend WithEvents lstValori As CheckedListBox
    Friend WithEvents sepFiltrare As Panel
    Friend WithEvents btnConditii As Button
    Friend WithEvents btnStergeFiltru As Button
    Friend WithEvents pnlGrupare As Panel
    Friend WithEvents tlyGrupare As TableLayoutPanel
    Friend WithEvents chkGrupeaza As CheckBox
    Friend WithEvents pnlSensGrup As Panel
    Friend WithEvents rbGrupCresc As RadioButton
    Friend WithEvents rbGrupDesc As RadioButton
    Friend WithEvents sepGrupare As Panel
    Friend WithEvents chkGrupAntet As CheckBox
    Friend WithEvents chkGrupSubsol As CheckBox
    Friend WithEvents chkGrupAgregate As CheckBox
    Friend WithEvents chkGrupStrangere As CheckBox
    Friend WithEvents chkGrupPornitStrans As CheckBox
    Friend WithEvents lblNiveluri As Label
    Friend WithEvents lstNiveluri As ListBox
    Friend WithEvents pnlButoane As Panel
    Friend WithEvents btnOk As Button
    Friend WithEvents btnAnuleaza As Button
    Friend WithEvents sepNav As Panel
    Friend WithEvents navFile As KBotNavList

End Class
