Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariMultithreadView
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
        tlyPagina = New KBotTableLayoutPanel()
        lblTitlu = New Label()
        lblServer = New Label()
        tlyOptiuni = New KBotTableLayoutPanel()
        chkMultiThread = New CheckBox()
        lblFire = New Label()
        txtFire = New KBotTextField()
        chkAutoVechi = New CheckBox()
        lblZile = New Label()
        txtZile = New KBotTextField()
        chkToateReceptiile = New CheckBox()
        tlyPagina.SuspendLayout()
        tlyOptiuni.SuspendLayout()
        SuspendLayout()
        '
        ' tlyPagina
        '
        tlyPagina.AutoScroll = True
        tlyPagina.ColumnCount = 1
        tlyPagina.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPagina.Controls.Add(lblTitlu, 0, 0)
        tlyPagina.Controls.Add(lblServer, 0, 1)
        tlyPagina.Controls.Add(tlyOptiuni, 0, 2)
        tlyPagina.Dock = DockStyle.Fill
        tlyPagina.Location = New Point(0, 0)
        tlyPagina.Margin = New Padding(0)
        tlyPagina.Name = "tlyPagina"
        tlyPagina.Padding = New Padding(24, 18, 24, 18)
        tlyPagina.RowCount = 4
        tlyPagina.RowStyles.Add(New RowStyle())
        tlyPagina.RowStyles.Add(New RowStyle())
        tlyPagina.RowStyles.Add(New RowStyle())
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tlyPagina.Size = New Size(960, 457)
        tlyPagina.TabIndex = 0
        '
        ' lblTitlu
        '
        lblTitlu.AutoSize = True
        lblTitlu.Font = New Font("Segoe UI Semibold", 12F)
        lblTitlu.Location = New Point(28, 18)
        lblTitlu.Margin = New Padding(4, 0, 4, 8)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.Size = New Size(274, 32)
        lblTitlu.TabIndex = 0
        lblTitlu.Text = "Descărcări multiple"
        '
        ' lblServer
        '
        lblServer.AutoSize = True
        lblServer.Location = New Point(28, 58)
        lblServer.Margin = New Padding(4, 0, 4, 12)
        lblServer.Name = "lblServer"
        lblServer.Size = New Size(400, 24)
        lblServer.TabIndex = 1
        lblServer.Text = "Mai multe angajamente se descarcă deodată, câte un tab FOREXE fiecare."
        '
        ' tlyOptiuni
        '
        tlyOptiuni.AutoFitToTheme = False
        tlyOptiuni.AutoSize = True
        tlyOptiuni.ColumnCount = 2
        tlyOptiuni.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 400F))
        tlyOptiuni.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyOptiuni.Controls.Add(chkMultiThread, 0, 0)
        tlyOptiuni.Controls.Add(lblFire, 0, 1)
        tlyOptiuni.Controls.Add(txtFire, 1, 1)
        tlyOptiuni.Controls.Add(chkAutoVechi, 0, 2)
        tlyOptiuni.Controls.Add(lblZile, 0, 3)
        tlyOptiuni.Controls.Add(txtZile, 1, 3)
        tlyOptiuni.Controls.Add(chkToateReceptiile, 0, 4)
        tlyOptiuni.Dock = DockStyle.Top
        tlyOptiuni.Location = New Point(28, 94)
        tlyOptiuni.Margin = New Padding(4, 0, 4, 24)
        tlyOptiuni.Name = "tlyOptiuni"
        tlyOptiuni.RowCount = 5
        tlyOptiuni.RowStyles.Add(New RowStyle())
        tlyOptiuni.RowStyles.Add(New RowStyle())
        tlyOptiuni.RowStyles.Add(New RowStyle())
        tlyOptiuni.RowStyles.Add(New RowStyle())
        tlyOptiuni.RowStyles.Add(New RowStyle())
        tlyOptiuni.Size = New Size(904, 220)
        tlyOptiuni.TabIndex = 2
        '
        ' chkMultiThread
        '
        chkMultiThread.AutoSize = True
        tlyOptiuni.SetColumnSpan(chkMultiThread, 2)
        chkMultiThread.Location = New Point(4, 0)
        chkMultiThread.Margin = New Padding(4, 0, 4, 10)
        chkMultiThread.Name = "chkMultiThread"
        chkMultiThread.Size = New Size(462, 26)
        chkMultiThread.TabIndex = 0
        chkMultiThread.Text = "Descarcă mai multe angajamente deodată"
        tips.SetToolTipHeader(chkMultiThread, "Descărcare pe mai multe taburi")
        tips.SetToolTipText(chkMultiThread, "Bifat: fiecare descărcare rulează pe un tab FOREXE al ei, mai multe deodată (aceeași conectare, fără altă autentificare)." & vbLf & "Cele peste numărul de taburi așteaptă la rând și pornesc pe măsură ce se eliberează un tab." & vbLf & "Apare un rând nou în meniul arborelui: «Actualizează angajamente...»." & vbLf & "Debifat: descărcările merg una câte una, ca până acum." & vbLf & "Pagina există numai cât timp serverul permite descărcarea pe mai multe taburi.")
        chkMultiThread.UseVisualStyleBackColor = True
        '
        ' lblFire
        '
        lblFire.AutoSize = True
        lblFire.Dock = DockStyle.Fill
        lblFire.Location = New Point(4, 36)
        lblFire.Margin = New Padding(28, 0, 4, 10)
        lblFire.Name = "lblFire"
        lblFire.Size = New Size(392, 40)
        lblFire.TabIndex = 1
        lblFire.Text = "Numărul de taburi deodată (1–10)"
        lblFire.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtFire
        '
        txtFire.Anchor = AnchorStyles.Left
        txtFire.BackColor = Color.Transparent
        txtFire.Location = New Point(404, 36)
        txtFire.Margin = New Padding(4, 0, 4, 10)
        txtFire.MaxLength = 2
        txtFire.Name = "txtFire"
        txtFire.PlaceholderText = "3"
        txtFire.Size = New Size(150, 40)
        txtFire.TabIndex = 2
        txtFire.TextAlign = HorizontalAlignment.Center
        txtFire.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtFire, "Câte descărcări deodată")
        tips.SetToolTipText(txtFire, "Atâtea taburi FOREXE lucrează în același timp; cel mult numărul îngăduit de server (cel mult 10)." & vbLf & "Nu e o limită a angajamentelor alese: cele în plus așteaptă la rând." & vbLf & "Se salvează la Enter sau când ieși din câmp.")
        '
        ' chkAutoVechi
        '
        chkAutoVechi.AutoSize = True
        tlyOptiuni.SetColumnSpan(chkAutoVechi, 2)
        chkAutoVechi.Location = New Point(4, 86)
        chkAutoVechi.Margin = New Padding(28, 0, 4, 10)
        chkAutoVechi.Name = "chkAutoVechi"
        chkAutoVechi.Size = New Size(462, 26)
        chkAutoVechi.TabIndex = 3
        chkAutoVechi.Text = "La conectare, actualizează angajamentele vechi"
        tips.SetToolTipHeader(chkAutoVechi, "Actualizare la conectare")
        tips.SetToolTipText(chkAutoVechi, "Bifat: după conectarea la FOREXE, angajamentele descărcate deja, dar neactualizate de cel puțin numărul de zile de mai jos, se descarcă singure (mai multe deodată)." & vbLf & "Nu se pune nicio întrebare și se citesc toate recepțiile." & vbLf & "Data ultimei actualizări se vede în descrierea de la trecerea mouse-ului peste angajament.")
        chkAutoVechi.UseVisualStyleBackColor = True
        '
        ' lblZile
        '
        lblZile.AutoSize = True
        lblZile.Dock = DockStyle.Fill
        lblZile.Location = New Point(4, 122)
        lblZile.Margin = New Padding(56, 0, 4, 10)
        lblZile.Name = "lblZile"
        lblZile.Size = New Size(392, 40)
        lblZile.TabIndex = 4
        lblZile.Text = "Neactualizate de (zile, cel mult 10)"
        lblZile.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtZile
        '
        txtZile.Anchor = AnchorStyles.Left
        txtZile.BackColor = Color.Transparent
        txtZile.Location = New Point(404, 122)
        txtZile.Margin = New Padding(4, 0, 4, 10)
        txtZile.MaxLength = 2
        txtZile.Name = "txtZile"
        txtZile.PlaceholderText = "7"
        txtZile.Size = New Size(150, 40)
        txtZile.TabIndex = 5
        txtZile.TextAlign = HorizontalAlignment.Center
        txtZile.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtZile, "Zilele de la ultima actualizare")
        tips.SetToolTipText(txtZile, "De la 1 la 10. Un angajament neactualizat de atâtea zile (sau mai mult) se descarcă la conectare." & vbLf & "Se salvează la Enter sau când ieși din câmp.")
        '
        ' chkToateReceptiile
        '
        chkToateReceptiile.AutoSize = True
        tlyOptiuni.SetColumnSpan(chkToateReceptiile, 2)
        chkToateReceptiile.Location = New Point(4, 172)
        chkToateReceptiile.Margin = New Padding(28, 0, 4, 10)
        chkToateReceptiile.Name = "chkToateReceptiile"
        chkToateReceptiile.Size = New Size(462, 26)
        chkToateReceptiile.TabIndex = 6
        chkToateReceptiile.Text = "Actualizează implicit toate recepțiile"
        tips.SetToolTipHeader(chkToateReceptiile, "Toate recepțiile, fără întrebare")
        tips.SetToolTipText(chkToateReceptiile, "Bifat: cât timp descărcarea pe mai multe taburi e pornită, fereastra de alegere a recepțiilor nu se mai arată; se citesc toate recepțiile." & vbLf & "Debifat: alegerea recepțiilor se face ca până acum." & vbLf & "Fără descărcarea pe mai multe taburi, fereastra se arată ca până acum.")
        chkToateReceptiile.UseVisualStyleBackColor = True
        '
        ' SetariMultithreadView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyPagina)
        Name = "SetariMultithreadView"
        Size = New Size(960, 457)
        tlyPagina.ResumeLayout(False)
        tlyPagina.PerformLayout()
        tlyOptiuni.ResumeLayout(False)
        tlyOptiuni.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyPagina As KBotTableLayoutPanel
    Friend WithEvents lblTitlu As Label
    Friend WithEvents lblServer As Label
    Friend WithEvents tlyOptiuni As KBotTableLayoutPanel
    Friend WithEvents chkMultiThread As CheckBox
    Friend WithEvents lblFire As Label
    Friend WithEvents txtFire As KBotTextField
    Friend WithEvents chkAutoVechi As CheckBox
    Friend WithEvents lblZile As Label
    Friend WithEvents txtZile As KBotTextField
    Friend WithEvents chkToateReceptiile As CheckBox
End Class
