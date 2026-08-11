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
        lstValori = New CheckedListBox()
        pnlButoane = New Panel()
        btnOk = New Button()
        btnAnuleaza = New Button()
        chkSelecteazaTot = New CheckBox()
        txtCauta = New KBotTextField()
        sepConditii = New Panel()
        btnConditii = New Button()
        btnStergeFiltru = New Button()
        sepSortare = New Panel()
        btnSortDesc = New Button()
        btnSortAsc = New Button()
        pnlCorp.SuspendLayout()
        pnlButoane.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlCorp
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
        pnlCorp.Location = New Point(1, 1)
        pnlCorp.Name = "pnlCorp"
        pnlCorp.Padding = New Padding(0, 4, 0, 0)
        pnlCorp.Size = New Size(310, 486)
        pnlCorp.TabIndex = 0
        ' 
        ' lstValori
        ' 
        lstValori.BorderStyle = BorderStyle.None
        lstValori.CheckOnClick = True
        lstValori.Dock = DockStyle.Fill
        lstValori.IntegralHeight = False
        lstValori.Location = New Point(0, 204)
        lstValori.Name = "lstValori"
        lstValori.Size = New Size(310, 234)
        lstValori.TabIndex = 7
        ' 
        ' pnlButoane
        ' 
        pnlButoane.Controls.Add(btnOk)
        pnlButoane.Controls.Add(btnAnuleaza)
        pnlButoane.Dock = DockStyle.Bottom
        pnlButoane.Location = New Point(0, 438)
        pnlButoane.Margin = New Padding(0)
        pnlButoane.Name = "pnlButoane"
        pnlButoane.Padding = New Padding(6, 0, 6, 0)
        pnlButoane.Size = New Size(310, 48)
        pnlButoane.TabIndex = 8
        ' 
        ' btnOk
        ' 
        btnOk.Dock = DockStyle.Right
        btnOk.Location = New Point(184, 0)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(120, 48)
        btnOk.TabIndex = 8
        btnOk.Text = "OK"
        btnOk.UseVisualStyleBackColor = True
        ' 
        ' btnAnuleaza
        ' 
        btnAnuleaza.DialogResult = DialogResult.Cancel
        btnAnuleaza.Dock = DockStyle.Left
        btnAnuleaza.Location = New Point(6, 0)
        btnAnuleaza.Name = "btnAnuleaza"
        btnAnuleaza.Size = New Size(120, 48)
        btnAnuleaza.TabIndex = 9
        btnAnuleaza.Text = "Anulează"
        btnAnuleaza.UseVisualStyleBackColor = True
        ' 
        ' chkSelecteazaTot
        ' 
        chkSelecteazaTot.Dock = DockStyle.Top
        chkSelecteazaTot.Location = New Point(0, 174)
        chkSelecteazaTot.Name = "chkSelecteazaTot"
        chkSelecteazaTot.Padding = New Padding(4, 0, 0, 0)
        chkSelecteazaTot.Size = New Size(310, 30)
        chkSelecteazaTot.TabIndex = 6
        chkSelecteazaTot.Text = "(Selectează tot)"
        chkSelecteazaTot.ThreeState = True
        chkSelecteazaTot.UseVisualStyleBackColor = True
        ' 
        ' txtCauta
        ' 
        txtCauta.BackColor = Color.Transparent
        txtCauta.Dock = DockStyle.Top
        txtCauta.Location = New Point(0, 134)
        txtCauta.Margin = New Padding(6)
        txtCauta.MaxLength = 32767
        txtCauta.Name = "txtCauta"
        txtCauta.PlaceholderText = "Caută…"
        txtCauta.Size = New Size(310, 40)
        txtCauta.TabIndex = 5
        txtCauta.TabStop = False
        txtCauta.UseSystemPasswordChar = False
        ' 
        ' sepConditii
        ' 
        sepConditii.Dock = DockStyle.Top
        sepConditii.Location = New Point(0, 133)
        sepConditii.Name = "sepConditii"
        sepConditii.Size = New Size(310, 1)
        sepConditii.TabIndex = 9
        ' 
        ' btnConditii
        ' 
        btnConditii.Dock = DockStyle.Top
        btnConditii.FlatAppearance.BorderSize = 0
        btnConditii.FlatStyle = FlatStyle.Flat
        btnConditii.Font = New Font("Segoe UI", 9F, FontStyle.Italic)
        btnConditii.Location = New Point(0, 101)
        btnConditii.Name = "btnConditii"
        btnConditii.Padding = New Padding(8, 0, 0, 0)
        btnConditii.Size = New Size(310, 32)
        btnConditii.TabIndex = 4
        btnConditii.Text = "Filtre..."
        btnConditii.TextAlign = ContentAlignment.MiddleLeft
        btnConditii.UseVisualStyleBackColor = True
        ' 
        ' btnStergeFiltru
        ' 
        btnStergeFiltru.Dock = DockStyle.Top
        btnStergeFiltru.FlatAppearance.BorderSize = 0
        btnStergeFiltru.FlatStyle = FlatStyle.Flat
        btnStergeFiltru.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        btnStergeFiltru.ForeColor = Color.Firebrick
        btnStergeFiltru.Location = New Point(0, 69)
        btnStergeFiltru.Margin = New Padding(0)
        btnStergeFiltru.Name = "btnStergeFiltru"
        btnStergeFiltru.Padding = New Padding(8, 0, 0, 0)
        btnStergeFiltru.Size = New Size(310, 32)
        btnStergeFiltru.TabIndex = 3
        btnStergeFiltru.Text = "Șterge filtrul"
        btnStergeFiltru.TextAlign = ContentAlignment.MiddleLeft
        btnStergeFiltru.UseVisualStyleBackColor = True
        ' 
        ' sepSortare
        ' 
        sepSortare.Dock = DockStyle.Top
        sepSortare.Location = New Point(0, 68)
        sepSortare.Margin = New Padding(0, 3, 0, 3)
        sepSortare.Name = "sepSortare"
        sepSortare.Size = New Size(310, 1)
        sepSortare.TabIndex = 10
        ' 
        ' btnSortDesc
        ' 
        btnSortDesc.Dock = DockStyle.Top
        btnSortDesc.FlatAppearance.BorderSize = 0
        btnSortDesc.FlatStyle = FlatStyle.Flat
        btnSortDesc.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        btnSortDesc.Location = New Point(0, 36)
        btnSortDesc.Margin = New Padding(0)
        btnSortDesc.Name = "btnSortDesc"
        btnSortDesc.Padding = New Padding(8, 0, 0, 0)
        btnSortDesc.Size = New Size(310, 32)
        btnSortDesc.TabIndex = 2
        btnSortDesc.Text = "Sortează descrescător"
        btnSortDesc.TextAlign = ContentAlignment.MiddleLeft
        btnSortDesc.UseVisualStyleBackColor = True
        ' 
        ' btnSortAsc
        ' 
        btnSortAsc.Dock = DockStyle.Top
        btnSortAsc.FlatAppearance.BorderSize = 0
        btnSortAsc.FlatStyle = FlatStyle.Flat
        btnSortAsc.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        btnSortAsc.Location = New Point(0, 4)
        btnSortAsc.Margin = New Padding(0)
        btnSortAsc.Name = "btnSortAsc"
        btnSortAsc.Padding = New Padding(8, 0, 0, 0)
        btnSortAsc.Size = New Size(310, 32)
        btnSortAsc.TabIndex = 1
        btnSortAsc.Text = "Sortează crescător"
        btnSortAsc.TextAlign = ContentAlignment.MiddleLeft
        btnSortAsc.UseVisualStyleBackColor = True
        ' 
        ' KBotFilterPopup
        ' 
        AutoScaleMode = AutoScaleMode.None
        CancelButton = btnAnuleaza
        ClientSize = New Size(312, 488)
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
        pnlButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
