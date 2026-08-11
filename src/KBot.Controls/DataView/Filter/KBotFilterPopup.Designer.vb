Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Partea „designer” a <see cref="KBotFilterPopup"/> (slice 0028-06). Conform regulii casei, TOATE
''' controalele copil se declară AICI, nu se construiesc în cod — meniul se deschide și se citește
''' pe suprafața de proiectare din Visual Studio, iar fonturile, mărimile și așezarea se schimbă
''' acolo, nu prin constante într-un fișier de pictură.
'''
''' <para><b>Lista de valori e declarată tot aici; doar CONȚINUTUL ei vine la rulare.</b> Valorile
''' distincte ale unei coloane nu există la proiectare — dar controlul care le arată, fontul lui,
''' înălțimea rândului și locul din fereastră sunt ale designerului, ca orice altceva.</para>
'''
''' <para><b>Ordinea de andocare e INVERSĂ celei vizuale</b> (regula casei pentru un panou-card):
''' ultimul <c>Dock = Top</c> adăugat ajunge cel mai sus. De aceea în <c>InitializeComponent</c>
''' controalele intră în ordinea „de jos în sus”: bara de butoane (Bottom), lista (Fill), apoi
''' banda de comenzi, de la caseta de căutare înapoi către sortare.</para>
'''
''' <para>Fereastra e fără chenar (e un meniu), iar «chenarul» de 1px se obține din
''' <c>Padding</c>-ul formularului plus fundalul lui: culoarea vine din temă, în
''' <c>OnThemeChanged</c>, deci nu există nicio culoare scrisă aici.</para>
''' </summary>
Partial Class KBotFilterPopup
    Inherits KBot.Theming.KBotThemedForm

    Private components As System.ComponentModel.IContainer

    Friend WithEvents pnlCorp As Panel
    Friend WithEvents btnSortAsc As Button
    Friend WithEvents btnSortDesc As Button
    Friend WithEvents sepSortare As Panel
    Friend WithEvents btnStergeFiltru As Button
    Friend WithEvents btnConditii As Button
    Friend WithEvents sepConditii As Panel
    Friend WithEvents txtCauta As KBotTextField
    Friend WithEvents chkSelecteazaTot As CheckBox
    Friend WithEvents lstValori As CheckedListBox
    Friend WithEvents pnlButoane As Panel
    Friend WithEvents btnOk As Button
    Friend WithEvents btnAnuleaza As Button

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private Sub InitializeComponent()
        pnlCorp = New Panel()
        btnSortAsc = New Button()
        btnSortDesc = New Button()
        sepSortare = New Panel()
        btnStergeFiltru = New Button()
        btnConditii = New Button()
        sepConditii = New Panel()
        txtCauta = New KBotTextField()
        chkSelecteazaTot = New CheckBox()
        lstValori = New CheckedListBox()
        pnlButoane = New Panel()
        btnOk = New Button()
        btnAnuleaza = New Button()
        pnlCorp.SuspendLayout()
        pnlButoane.SuspendLayout()
        SuspendLayout()
        '
        ' btnSortAsc — textul vine la rulare: «A → Z» pe text, «de la mic la mare» pe numere
        '
        btnSortAsc.Dock = DockStyle.Top
        btnSortAsc.FlatStyle = FlatStyle.Flat
        btnSortAsc.FlatAppearance.BorderSize = 0
        btnSortAsc.Height = 26
        btnSortAsc.Name = "btnSortAsc"
        btnSortAsc.Padding = New Padding(8, 0, 0, 0)
        btnSortAsc.TabIndex = 1
        btnSortAsc.Text = "Sortează crescător"
        btnSortAsc.TextAlign = Drawing.ContentAlignment.MiddleLeft
        btnSortAsc.UseVisualStyleBackColor = True
        '
        ' btnSortDesc
        '
        btnSortDesc.Dock = DockStyle.Top
        btnSortDesc.FlatStyle = FlatStyle.Flat
        btnSortDesc.FlatAppearance.BorderSize = 0
        btnSortDesc.Height = 26
        btnSortDesc.Name = "btnSortDesc"
        btnSortDesc.Padding = New Padding(8, 0, 0, 0)
        btnSortDesc.TabIndex = 2
        btnSortDesc.Text = "Sortează descrescător"
        btnSortDesc.TextAlign = Drawing.ContentAlignment.MiddleLeft
        btnSortDesc.UseVisualStyleBackColor = True
        '
        ' sepSortare — linia de 1px dintre sortare și restul (culoarea vine din temă)
        '
        sepSortare.Dock = DockStyle.Top
        sepSortare.Height = 1
        sepSortare.Margin = New Padding(0, 3, 0, 3)
        sepSortare.Name = "sepSortare"
        '
        ' btnStergeFiltru — stins cât timp coloana n-are filtru (se decide la rulare)
        '
        btnStergeFiltru.Dock = DockStyle.Top
        btnStergeFiltru.FlatStyle = FlatStyle.Flat
        btnStergeFiltru.FlatAppearance.BorderSize = 0
        btnStergeFiltru.Height = 26
        btnStergeFiltru.Name = "btnStergeFiltru"
        btnStergeFiltru.Padding = New Padding(8, 0, 0, 0)
        btnStergeFiltru.TabIndex = 3
        btnStergeFiltru.Text = "Șterge filtrul"
        btnStergeFiltru.TextAlign = Drawing.ContentAlignment.MiddleLeft
        btnStergeFiltru.UseVisualStyleBackColor = True
        '
        ' btnConditii — deschide submeniul de condiții; ascuns pe coloanele logice
        '
        btnConditii.Dock = DockStyle.Top
        btnConditii.FlatStyle = FlatStyle.Flat
        btnConditii.FlatAppearance.BorderSize = 0
        btnConditii.Height = 26
        btnConditii.Name = "btnConditii"
        btnConditii.Padding = New Padding(8, 0, 0, 0)
        btnConditii.TabIndex = 4
        btnConditii.Text = "Filtre"
        btnConditii.TextAlign = Drawing.ContentAlignment.MiddleLeft
        btnConditii.UseVisualStyleBackColor = True
        '
        ' sepConditii
        '
        sepConditii.Dock = DockStyle.Top
        sepConditii.Height = 1
        sepConditii.Name = "sepConditii"
        '
        ' txtCauta — singurul control care primește text tastat
        '
        txtCauta.Dock = DockStyle.Top
        txtCauta.Height = 28
        txtCauta.Margin = New Padding(6)
        txtCauta.Name = "txtCauta"
        txtCauta.PlaceholderText = "Caută…"
        txtCauta.TabIndex = 5
        '
        ' chkSelecteazaTot — bifă cu trei stări (a treia = «unele bifate»)
        '
        chkSelecteazaTot.Dock = DockStyle.Top
        chkSelecteazaTot.Height = 24
        chkSelecteazaTot.Name = "chkSelecteazaTot"
        chkSelecteazaTot.Padding = New Padding(4, 0, 0, 0)
        chkSelecteazaTot.TabIndex = 6
        chkSelecteazaTot.Text = "(Selectează tot)"
        chkSelecteazaTot.ThreeState = True
        chkSelecteazaTot.UseVisualStyleBackColor = True
        '
        ' lstValori — VALORILE se adaugă la rulare (nu există la proiectare); tot restul e de aici
        '
        lstValori.BorderStyle = BorderStyle.None
        lstValori.CheckOnClick = True
        lstValori.Dock = DockStyle.Fill
        lstValori.IntegralHeight = False
        lstValori.ItemHeight = 20
        lstValori.Name = "lstValori"
        lstValori.TabIndex = 7
        '
        ' btnOk
        '
        btnOk.Anchor = CType(AnchorStyles.Top Or AnchorStyles.Right, AnchorStyles)
        btnOk.Location = New Drawing.Point(88, 4)
        btnOk.Name = "btnOk"
        btnOk.Size = New Drawing.Size(84, 26)
        btnOk.TabIndex = 8
        btnOk.Text = "OK"
        btnOk.UseVisualStyleBackColor = True
        '
        ' btnAnuleaza
        '
        btnAnuleaza.Anchor = CType(AnchorStyles.Top Or AnchorStyles.Right, AnchorStyles)
        btnAnuleaza.DialogResult = DialogResult.Cancel
        btnAnuleaza.Location = New Drawing.Point(178, 4)
        btnAnuleaza.Name = "btnAnuleaza"
        btnAnuleaza.Size = New Drawing.Size(84, 26)
        btnAnuleaza.TabIndex = 9
        btnAnuleaza.Text = "Anulează"
        btnAnuleaza.UseVisualStyleBackColor = True
        '
        ' pnlButoane
        '
        pnlButoane.Controls.Add(btnOk)
        pnlButoane.Controls.Add(btnAnuleaza)
        pnlButoane.Dock = DockStyle.Bottom
        pnlButoane.Height = 36
        pnlButoane.Name = "pnlButoane"
        pnlButoane.Padding = New Padding(6, 4, 6, 4)
        '
        ' pnlCorp — ordinea de andocare e INVERSĂ celei vizuale (vezi rezumatul clasei)
        '
        pnlCorp.Controls.Add(lstValori)
        pnlCorp.Controls.Add(pnlButoane)
        pnlCorp.Controls.Add(chkSelecteazaTot)
        pnlCorp.Controls.Add(txtCauta)
        pnlCorp.Controls.Add(sepConditii)
        pnlCorp.Controls.Add(btnConditii)
        pnlCorp.Controls.Add(btnStergeFiltru)
        pnlCorp.Controls.Add(sepSortare)
        pnlCorp.Controls.Add(btnSortDesc)
        pnlCorp.Controls.Add(btnSortAsc)
        pnlCorp.Dock = DockStyle.Fill
        pnlCorp.Name = "pnlCorp"
        pnlCorp.Padding = New Padding(0, 4, 0, 0)
        '
        ' KBotFilterPopup
        '
        AutoScaleMode = AutoScaleMode.None
        CancelButton = btnAnuleaza
        ClientSize = New Drawing.Size(270, 366)
        ControlBox = False
        FormBorderStyle = FormBorderStyle.None
        KeyPreview = True
        MaximizeBox = False
        MinimizeBox = False
        Name = "KBotFilterPopup"
        ' Chenarul de 1px al meniului: marginea formularului + fundalul lui (culoarea, din temă).
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = ""
        Controls.Add(pnlCorp)
        pnlCorp.ResumeLayout(False)
        pnlButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
