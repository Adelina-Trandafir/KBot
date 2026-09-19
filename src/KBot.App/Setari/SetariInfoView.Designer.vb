Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariInfoView
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
        tlyBody = New KBotTableLayoutPanel()
        lblTitluCont = New Label()
        tlyCont = New KBotTableLayoutPanel()
        lblOperatorCaption = New Label()
        lblOperator = New Label()
        lblUnitateCaption = New Label()
        lblUnitate = New Label()
        lblRolCaption = New Label()
        lblRol = New Label()
        lblBazaCaption = New Label()
        lblBaza = New Label()
        lblTitluLicenta = New Label()
        tlyLicenta = New KBotTableLayoutPanel()
        lblTipCaption = New Label()
        lblTip = New Label()
        lblVersiuneCaption = New Label()
        lblVersiune = New Label()
        btnActualizari = New Button()
        lblTitluParola = New Label()
        tlyParola = New KBotTableLayoutPanel()
        lblParolaActuala = New Label()
        txtParolaActuala = New KBotTextField()
        btnTrimiteCod = New Button()
        lblCod = New Label()
        txtCod = New KBotTextField()
        lblParolaNoua = New Label()
        txtParolaNoua = New KBotTextField()
        lblParolaConfirmare = New Label()
        txtParolaConfirmare = New KBotTextField()
        btnSchimbaParola = New Button()
        ntfParola = New KBotNotice()
        tlyBody.SuspendLayout()
        tlyCont.SuspendLayout()
        tlyLicenta.SuspendLayout()
        tlyParola.SuspendLayout()
        SuspendLayout()
        '
        ' tlyBody
        '
        tlyBody.AutoScroll = True
        tlyBody.ColumnCount = 1
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBody.Controls.Add(lblTitluCont, 0, 0)
        tlyBody.Controls.Add(tlyCont, 0, 1)
        tlyBody.Controls.Add(lblTitluLicenta, 0, 2)
        tlyBody.Controls.Add(tlyLicenta, 0, 3)
        tlyBody.Controls.Add(lblTitluParola, 0, 4)
        tlyBody.Controls.Add(tlyParola, 0, 5)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(0, 0)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.Padding = New Padding(24, 18, 24, 18)
        tlyBody.RowCount = 7
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.Size = New Size(960, 840)
        tlyBody.TabIndex = 0
        '
        ' lblTitluCont
        '
        lblTitluCont.AutoSize = True
        lblTitluCont.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluCont.Location = New Point(28, 18)
        lblTitluCont.Margin = New Padding(4, 0, 4, 8)
        lblTitluCont.Name = "lblTitluCont"
        lblTitluCont.Size = New Size(72, 32)
        lblTitluCont.TabIndex = 0
        lblTitluCont.Text = "Contul"
        '
        ' tlyCont
        '
        tlyCont.AutoFitToTheme = False
        tlyCont.AutoSize = True
        tlyCont.ColumnCount = 2
        tlyCont.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyCont.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCont.Controls.Add(lblOperatorCaption, 0, 0)
        tlyCont.Controls.Add(lblOperator, 1, 0)
        tlyCont.Controls.Add(lblUnitateCaption, 0, 1)
        tlyCont.Controls.Add(lblUnitate, 1, 1)
        tlyCont.Controls.Add(lblRolCaption, 0, 2)
        tlyCont.Controls.Add(lblRol, 1, 2)
        tlyCont.Controls.Add(lblBazaCaption, 0, 3)
        tlyCont.Controls.Add(lblBaza, 1, 3)
        tlyCont.Dock = DockStyle.Top
        tlyCont.Location = New Point(28, 58)
        tlyCont.Margin = New Padding(4, 0, 4, 24)
        tlyCont.Name = "tlyCont"
        tlyCont.RowCount = 4
        tlyCont.RowStyles.Add(New RowStyle())
        tlyCont.RowStyles.Add(New RowStyle())
        tlyCont.RowStyles.Add(New RowStyle())
        tlyCont.RowStyles.Add(New RowStyle())
        tlyCont.Size = New Size(904, 128)
        tlyCont.TabIndex = 1
        '
        ' lblOperatorCaption
        '
        lblOperatorCaption.AutoSize = True
        lblOperatorCaption.Dock = DockStyle.Fill
        lblOperatorCaption.Location = New Point(4, 0)
        lblOperatorCaption.Margin = New Padding(4, 0, 4, 6)
        lblOperatorCaption.Name = "lblOperatorCaption"
        lblOperatorCaption.Size = New Size(212, 26)
        lblOperatorCaption.TabIndex = 0
        lblOperatorCaption.Text = "Operator (e-mail)"
        '
        ' lblOperator
        '
        lblOperator.AutoSize = True
        lblOperator.Dock = DockStyle.Fill
        lblOperator.Font = New Font("Segoe UI Semibold", 9F)
        lblOperator.Location = New Point(224, 0)
        lblOperator.Margin = New Padding(4, 0, 4, 6)
        lblOperator.Name = "lblOperator"
        lblOperator.Size = New Size(676, 26)
        lblOperator.TabIndex = 1
        lblOperator.Text = "—"
        '
        ' lblUnitateCaption
        '
        lblUnitateCaption.AutoSize = True
        lblUnitateCaption.Dock = DockStyle.Fill
        lblUnitateCaption.Location = New Point(4, 32)
        lblUnitateCaption.Margin = New Padding(4, 0, 4, 6)
        lblUnitateCaption.Name = "lblUnitateCaption"
        lblUnitateCaption.Size = New Size(212, 26)
        lblUnitateCaption.TabIndex = 2
        lblUnitateCaption.Text = "Unitate"
        '
        ' lblUnitate
        '
        lblUnitate.AutoSize = True
        lblUnitate.Dock = DockStyle.Fill
        lblUnitate.Font = New Font("Segoe UI Semibold", 9F)
        lblUnitate.Location = New Point(224, 32)
        lblUnitate.Margin = New Padding(4, 0, 4, 6)
        lblUnitate.Name = "lblUnitate"
        lblUnitate.Size = New Size(676, 26)
        lblUnitate.TabIndex = 3
        lblUnitate.Text = "—"
        '
        ' lblRolCaption
        '
        lblRolCaption.AutoSize = True
        lblRolCaption.Dock = DockStyle.Fill
        lblRolCaption.Location = New Point(4, 64)
        lblRolCaption.Margin = New Padding(4, 0, 4, 6)
        lblRolCaption.Name = "lblRolCaption"
        lblRolCaption.Size = New Size(212, 26)
        lblRolCaption.TabIndex = 4
        lblRolCaption.Text = "Rol"
        '
        ' lblRol
        '
        lblRol.AutoSize = True
        lblRol.Dock = DockStyle.Fill
        lblRol.Font = New Font("Segoe UI Semibold", 9F)
        lblRol.Location = New Point(224, 64)
        lblRol.Margin = New Padding(4, 0, 4, 6)
        lblRol.Name = "lblRol"
        lblRol.Size = New Size(676, 26)
        lblRol.TabIndex = 5
        lblRol.Text = "—"
        '
        ' lblBazaCaption
        '
        lblBazaCaption.AutoSize = True
        lblBazaCaption.Dock = DockStyle.Fill
        lblBazaCaption.Location = New Point(4, 96)
        lblBazaCaption.Margin = New Padding(4, 0, 4, 6)
        lblBazaCaption.Name = "lblBazaCaption"
        lblBazaCaption.Size = New Size(212, 26)
        lblBazaCaption.TabIndex = 6
        lblBazaCaption.Text = "Baza de date / perioada"
        '
        ' lblBaza
        '
        lblBaza.AutoSize = True
        lblBaza.Dock = DockStyle.Fill
        lblBaza.Font = New Font("Segoe UI Semibold", 9F)
        lblBaza.Location = New Point(224, 96)
        lblBaza.Margin = New Padding(4, 0, 4, 6)
        lblBaza.Name = "lblBaza"
        lblBaza.Size = New Size(676, 26)
        lblBaza.TabIndex = 7
        lblBaza.Text = "—"
        '
        ' lblTitluLicenta
        '
        lblTitluLicenta.AutoSize = True
        lblTitluLicenta.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluLicenta.Location = New Point(28, 210)
        lblTitluLicenta.Margin = New Padding(4, 0, 4, 8)
        lblTitluLicenta.Name = "lblTitluLicenta"
        lblTitluLicenta.Size = New Size(236, 32)
        lblTitluLicenta.TabIndex = 2
        lblTitluLicenta.Text = "Licență și versiune"
        '
        ' tlyLicenta
        '
        tlyLicenta.AutoFitToTheme = False
        tlyLicenta.AutoSize = True
        tlyLicenta.ColumnCount = 3
        tlyLicenta.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyLicenta.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyLicenta.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 240F))
        tlyLicenta.Controls.Add(lblTipCaption, 0, 0)
        tlyLicenta.Controls.Add(lblTip, 1, 0)
        tlyLicenta.Controls.Add(lblVersiuneCaption, 0, 1)
        tlyLicenta.Controls.Add(lblVersiune, 1, 1)
        tlyLicenta.Controls.Add(btnActualizari, 2, 1)
        tlyLicenta.Dock = DockStyle.Top
        tlyLicenta.Location = New Point(28, 250)
        tlyLicenta.Margin = New Padding(4, 0, 4, 24)
        tlyLicenta.Name = "tlyLicenta"
        tlyLicenta.RowCount = 2
        tlyLicenta.RowStyles.Add(New RowStyle())
        tlyLicenta.RowStyles.Add(New RowStyle())
        tlyLicenta.Size = New Size(904, 88)
        tlyLicenta.TabIndex = 3
        '
        ' lblTipCaption
        '
        lblTipCaption.AutoSize = True
        lblTipCaption.Dock = DockStyle.Fill
        lblTipCaption.Location = New Point(4, 0)
        lblTipCaption.Margin = New Padding(4, 0, 4, 6)
        lblTipCaption.Name = "lblTipCaption"
        lblTipCaption.Size = New Size(212, 26)
        lblTipCaption.TabIndex = 0
        lblTipCaption.Text = "Tip instalare"
        '
        ' lblTip
        '
        lblTip.AutoSize = True
        tlyLicenta.SetColumnSpan(lblTip, 2)
        lblTip.Dock = DockStyle.Fill
        lblTip.Font = New Font("Segoe UI Semibold", 9F)
        lblTip.Location = New Point(224, 0)
        lblTip.Margin = New Padding(4, 0, 4, 6)
        lblTip.Name = "lblTip"
        lblTip.Size = New Size(676, 26)
        lblTip.TabIndex = 1
        lblTip.Text = "—"
        '
        ' lblVersiuneCaption
        '
        lblVersiuneCaption.AutoSize = True
        lblVersiuneCaption.Dock = DockStyle.Fill
        lblVersiuneCaption.Location = New Point(4, 32)
        lblVersiuneCaption.Margin = New Padding(4, 0, 4, 6)
        lblVersiuneCaption.Name = "lblVersiuneCaption"
        lblVersiuneCaption.Size = New Size(212, 50)
        lblVersiuneCaption.TabIndex = 2
        lblVersiuneCaption.Text = "Versiune K-BOT"
        lblVersiuneCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblVersiune
        '
        lblVersiune.AutoSize = True
        lblVersiune.Dock = DockStyle.Fill
        lblVersiune.Font = New Font("Segoe UI Semibold", 9F)
        lblVersiune.Location = New Point(224, 32)
        lblVersiune.Margin = New Padding(4, 0, 4, 6)
        lblVersiune.Name = "lblVersiune"
        lblVersiune.Size = New Size(428, 50)
        lblVersiune.TabIndex = 3
        lblVersiune.Text = "—"
        lblVersiune.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnActualizari
        '
        btnActualizari.Dock = DockStyle.Fill
        btnActualizari.FlatStyle = FlatStyle.Flat
        btnActualizari.Font = New Font("Segoe UI Semibold", 9F)
        btnActualizari.Location = New Point(664, 32)
        btnActualizari.Margin = New Padding(4, 0, 4, 6)
        btnActualizari.Name = "btnActualizari"
        btnActualizari.Size = New Size(236, 50)
        btnActualizari.TabIndex = 4
        btnActualizari.Text = "Caută actualizări"
        tips.SetToolTipHeader(btnActualizari, "Caută actualizări")
        tips.SetToolTipText(btnActualizari, "Întreabă serverul dacă există o versiune mai nouă." & vbLf & "Dacă da, o descarcă și repornește aplicația.")
        btnActualizari.UseVisualStyleBackColor = True
        '
        ' lblTitluParola
        '
        lblTitluParola.AutoSize = True
        lblTitluParola.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluParola.Location = New Point(28, 362)
        lblTitluParola.Margin = New Padding(4, 0, 4, 8)
        lblTitluParola.Name = "lblTitluParola"
        lblTitluParola.Size = New Size(211, 32)
        lblTitluParola.TabIndex = 4
        lblTitluParola.Text = "Schimbarea parolei"
        '
        ' tlyParola
        '
        tlyParola.AutoFitToTheme = False
        tlyParola.AutoSize = True
        tlyParola.ColumnCount = 3
        tlyParola.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyParola.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyParola.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 240F))
        tlyParola.Controls.Add(lblParolaActuala, 0, 0)
        tlyParola.Controls.Add(txtParolaActuala, 1, 0)
        tlyParola.Controls.Add(btnTrimiteCod, 2, 0)
        tlyParola.Controls.Add(lblCod, 0, 1)
        tlyParola.Controls.Add(txtCod, 1, 1)
        tlyParola.Controls.Add(lblParolaNoua, 0, 2)
        tlyParola.Controls.Add(txtParolaNoua, 1, 2)
        tlyParola.Controls.Add(lblParolaConfirmare, 0, 3)
        tlyParola.Controls.Add(txtParolaConfirmare, 1, 3)
        tlyParola.Controls.Add(btnSchimbaParola, 2, 3)
        tlyParola.Controls.Add(ntfParola, 0, 4)
        tlyParola.Dock = DockStyle.Top
        tlyParola.Location = New Point(28, 402)
        tlyParola.Margin = New Padding(4, 0, 4, 0)
        tlyParola.Name = "tlyParola"
        tlyParola.RowCount = 5
        tlyParola.RowStyles.Add(New RowStyle())
        tlyParola.RowStyles.Add(New RowStyle())
        tlyParola.RowStyles.Add(New RowStyle())
        tlyParola.RowStyles.Add(New RowStyle())
        tlyParola.RowStyles.Add(New RowStyle())
        tlyParola.Size = New Size(904, 360)
        tlyParola.TabIndex = 5
        '
        ' lblParolaActuala
        '
        lblParolaActuala.AutoSize = True
        lblParolaActuala.Dock = DockStyle.Fill
        lblParolaActuala.Location = New Point(4, 0)
        lblParolaActuala.Margin = New Padding(4, 0, 4, 10)
        lblParolaActuala.Name = "lblParolaActuala"
        lblParolaActuala.Size = New Size(212, 60)
        lblParolaActuala.TabIndex = 0
        lblParolaActuala.Text = "Parola actuală"
        lblParolaActuala.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtParolaActuala
        '
        txtParolaActuala.BackColor = Color.Transparent
        txtParolaActuala.Dock = DockStyle.Fill
        txtParolaActuala.Location = New Point(224, 0)
        txtParolaActuala.Margin = New Padding(4, 0, 4, 10)
        txtParolaActuala.Name = "txtParolaActuala"
        txtParolaActuala.Size = New Size(428, 60)
        txtParolaActuala.TabIndex = 1
        tips.SetToolTipHeader(txtParolaActuala, "Parola actuală")
        tips.SetToolTipText(txtParolaActuala, "Parola cu care te-ai autentificat acum." & vbLf & "Se verifică pe server înainte de a trimite codul.")
        txtParolaActuala.UseSystemPasswordChar = True
        '
        ' btnTrimiteCod
        '
        btnTrimiteCod.Dock = DockStyle.Fill
        btnTrimiteCod.FlatStyle = FlatStyle.Flat
        btnTrimiteCod.Font = New Font("Segoe UI Semibold", 9F)
        btnTrimiteCod.Location = New Point(664, 0)
        btnTrimiteCod.Margin = New Padding(4, 0, 4, 10)
        btnTrimiteCod.Name = "btnTrimiteCod"
        btnTrimiteCod.Size = New Size(236, 60)
        btnTrimiteCod.TabIndex = 2
        btnTrimiteCod.Text = "Trimite codul pe e-mail"
        tips.SetToolTipHeader(btnTrimiteCod, "Al doilea factor")
        tips.SetToolTipText(btnTrimiteCod, "Serverul verifică parola actuală și trimite un cod de confirmare" & vbLf & "pe adresa de e-mail cu care ești înregistrat (numele de utilizator).")
        btnTrimiteCod.UseVisualStyleBackColor = True
        '
        ' lblCod
        '
        lblCod.AutoSize = True
        lblCod.Dock = DockStyle.Fill
        lblCod.Location = New Point(4, 70)
        lblCod.Margin = New Padding(4, 0, 4, 10)
        lblCod.Name = "lblCod"
        lblCod.Size = New Size(212, 60)
        lblCod.TabIndex = 3
        lblCod.Text = "Codul primit pe e-mail"
        lblCod.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtCod
        '
        txtCod.BackColor = Color.Transparent
        txtCod.Dock = DockStyle.Fill
        txtCod.Enabled = False
        txtCod.Location = New Point(224, 70)
        txtCod.Margin = New Padding(4, 0, 4, 10)
        txtCod.MaxLength = 6
        txtCod.Name = "txtCod"
        txtCod.PlaceholderText = "6 cifre"
        txtCod.Size = New Size(428, 60)
        txtCod.TabIndex = 4
        tips.SetToolTipHeader(txtCod, "Codul de confirmare")
        tips.SetToolTipText(txtCod, "Cele 6 cifre din e-mail. Codul e valabil 10 minute și o singură dată.")
        '
        ' lblParolaNoua
        '
        lblParolaNoua.AutoSize = True
        lblParolaNoua.Dock = DockStyle.Fill
        lblParolaNoua.Location = New Point(4, 140)
        lblParolaNoua.Margin = New Padding(4, 0, 4, 10)
        lblParolaNoua.Name = "lblParolaNoua"
        lblParolaNoua.Size = New Size(212, 60)
        lblParolaNoua.TabIndex = 5
        lblParolaNoua.Text = "Parola nouă"
        lblParolaNoua.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtParolaNoua
        '
        txtParolaNoua.BackColor = Color.Transparent
        txtParolaNoua.Dock = DockStyle.Fill
        txtParolaNoua.Enabled = False
        txtParolaNoua.Location = New Point(224, 140)
        txtParolaNoua.Margin = New Padding(4, 0, 4, 10)
        txtParolaNoua.Name = "txtParolaNoua"
        txtParolaNoua.Size = New Size(428, 60)
        txtParolaNoua.TabIndex = 6
        tips.SetToolTipHeader(txtParolaNoua, "Parola nouă")
        tips.SetToolTipText(txtParolaNoua, "Cel puțin 8 caractere, diferită de cea actuală.")
        txtParolaNoua.UseSystemPasswordChar = True
        '
        ' lblParolaConfirmare
        '
        lblParolaConfirmare.AutoSize = True
        lblParolaConfirmare.Dock = DockStyle.Fill
        lblParolaConfirmare.Location = New Point(4, 210)
        lblParolaConfirmare.Margin = New Padding(4, 0, 4, 10)
        lblParolaConfirmare.Name = "lblParolaConfirmare"
        lblParolaConfirmare.Size = New Size(212, 60)
        lblParolaConfirmare.TabIndex = 7
        lblParolaConfirmare.Text = "Confirmă parola nouă"
        lblParolaConfirmare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtParolaConfirmare
        '
        txtParolaConfirmare.BackColor = Color.Transparent
        txtParolaConfirmare.Dock = DockStyle.Fill
        txtParolaConfirmare.Enabled = False
        txtParolaConfirmare.Location = New Point(224, 210)
        txtParolaConfirmare.Margin = New Padding(4, 0, 4, 10)
        txtParolaConfirmare.Name = "txtParolaConfirmare"
        txtParolaConfirmare.Size = New Size(428, 60)
        txtParolaConfirmare.TabIndex = 8
        txtParolaConfirmare.UseSystemPasswordChar = True
        '
        ' btnSchimbaParola
        '
        btnSchimbaParola.Dock = DockStyle.Fill
        btnSchimbaParola.Enabled = False
        btnSchimbaParola.FlatStyle = FlatStyle.Flat
        btnSchimbaParola.Font = New Font("Segoe UI Semibold", 9F)
        btnSchimbaParola.Location = New Point(664, 210)
        btnSchimbaParola.Margin = New Padding(4, 0, 4, 10)
        btnSchimbaParola.Name = "btnSchimbaParola"
        btnSchimbaParola.Size = New Size(236, 60)
        btnSchimbaParola.TabIndex = 9
        btnSchimbaParola.Text = "Schimbă parola"
        tips.SetToolTipHeader(btnSchimbaParola, "Schimbă parola")
        tips.SetToolTipText(btnSchimbaParola, "Trimite codul și parola nouă la server." & vbLf & "De la următoarea autentificare se folosește parola nouă.")
        btnSchimbaParola.UseVisualStyleBackColor = True
        '
        ' ntfParola
        '
        ntfParola.BackColor = Color.Transparent
        tlyParola.SetColumnSpan(ntfParola, 3)
        ntfParola.Dock = DockStyle.Fill
        ntfParola.Location = New Point(4, 290)
        ntfParola.Margin = New Padding(4, 10, 4, 0)
        ntfParola.Name = "ntfParola"
        ntfParola.Size = New Size(896, 60)
        ntfParola.TabIndex = 10
        ntfParola.TabStop = False
        ntfParola.Visible = False
        '
        ' SetariInfoView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyBody)
        Name = "SetariInfoView"
        Size = New Size(960, 840)
        tlyBody.ResumeLayout(False)
        tlyBody.PerformLayout()
        tlyCont.ResumeLayout(False)
        tlyCont.PerformLayout()
        tlyLicenta.ResumeLayout(False)
        tlyLicenta.PerformLayout()
        tlyParola.ResumeLayout(False)
        tlyParola.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents lblTitluCont As Label
    Friend WithEvents tlyCont As KBotTableLayoutPanel
    Friend WithEvents lblOperatorCaption As Label
    Friend WithEvents lblOperator As Label
    Friend WithEvents lblUnitateCaption As Label
    Friend WithEvents lblUnitate As Label
    Friend WithEvents lblRolCaption As Label
    Friend WithEvents lblRol As Label
    Friend WithEvents lblBazaCaption As Label
    Friend WithEvents lblBaza As Label
    Friend WithEvents lblTitluLicenta As Label
    Friend WithEvents tlyLicenta As KBotTableLayoutPanel
    Friend WithEvents lblTipCaption As Label
    Friend WithEvents lblTip As Label
    Friend WithEvents lblVersiuneCaption As Label
    Friend WithEvents lblVersiune As Label
    Friend WithEvents btnActualizari As Button
    Friend WithEvents lblTitluParola As Label
    Friend WithEvents tlyParola As KBotTableLayoutPanel
    Friend WithEvents lblParolaActuala As Label
    Friend WithEvents txtParolaActuala As KBotTextField
    Friend WithEvents btnTrimiteCod As Button
    Friend WithEvents lblCod As Label
    Friend WithEvents txtCod As KBotTextField
    Friend WithEvents lblParolaNoua As Label
    Friend WithEvents txtParolaNoua As KBotTextField
    Friend WithEvents lblParolaConfirmare As Label
    Friend WithEvents txtParolaConfirmare As KBotTextField
    Friend WithEvents btnSchimbaParola As Button
    Friend WithEvents ntfParola As KBotNotice
End Class
