Imports KBot.Controls

' «Rand din Sectiunea A» (slice 0081-08, operator 26.09.2026): every value section A expects of
' one line, in one window -- opened by «Adauga rand» for a new line and by a double click on a
' line to change it. The classification is a KBotComboBox with FindAsYouType + the classification
' InputMask (slice 0082): the operator types only the digits, the dots appear by themselves, and
' the list under the box narrows as they type.
'
' Slice 0081-09: the first row is the source / sector (SS) -- one of the SSs of the document's
' program (AVACONT_COMUN.DefaProgram), for a new angajament and a new revision alike -- and the
' list of classifications follows it. The line's partner is not here: it is the header's, the same for
' every line.
'
' Left column = captions, right column = the fields. The derived values (previous value,
' receptions, total, indicator code) are labels: they are never typed. All controls are declared
' HERE (docs/kbot-forms-ui-convention.md). Coordinates are in the 144 dpi the form was authored at.
' Card: children in REVERSE dock order (Fill first, then Bottom, then Top).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfEditLinieAForm
    Inherits Global.KBot.Theming.KBotThemedForm

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
        pnlCard = New Panel()
        tlyCorp = New KBotTableLayoutPanel()
        lblSursaCaption = New Label()
        cmbSursa = New KBotComboBox()
        lblClasificatieCaption = New Label()
        cmbClasificatie = New KBotComboBox()
        lblDenumireCaption = New Label()
        lblDenumire = New Label()
        lblElementCaption = New Label()
        txtElement = New KBotTextField()
        lblParametriiCaption = New Label()
        txtParametrii = New KBotTextField()
        lblValCurCaption = New Label()
        txtValCur = New KBotTextField()
        lblValPrecCaption = New Label()
        lblValPrec = New Label()
        lblValRecCaption = New Label()
        lblValRec = New Label()
        lblValTotCaption = New Label()
        lblValTot = New Label()
        lblCodIndicatorCaption = New Label()
        lblCodIndicator = New Label()
        pnlJos = New Panel()
        btnRenunta = New Button()
        btnOk = New Button()
        lblStare = New Label()
        capBar = New KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlyCorp.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlyCorp)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(1196, 716)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' tlyCorp
        '
        tlyCorp.ColumnCount = 2
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 300F))
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCorp.Controls.Add(lblSursaCaption, 0, 0)
        tlyCorp.Controls.Add(cmbSursa, 1, 0)
        tlyCorp.Controls.Add(lblClasificatieCaption, 0, 1)
        tlyCorp.Controls.Add(cmbClasificatie, 1, 1)
        tlyCorp.Controls.Add(lblDenumireCaption, 0, 2)
        tlyCorp.Controls.Add(lblDenumire, 1, 2)
        tlyCorp.Controls.Add(lblElementCaption, 0, 3)
        tlyCorp.Controls.Add(txtElement, 1, 3)
        tlyCorp.Controls.Add(lblParametriiCaption, 0, 4)
        tlyCorp.Controls.Add(txtParametrii, 1, 4)
        tlyCorp.Controls.Add(lblValCurCaption, 0, 5)
        tlyCorp.Controls.Add(txtValCur, 1, 5)
        tlyCorp.Controls.Add(lblValPrecCaption, 0, 6)
        tlyCorp.Controls.Add(lblValPrec, 1, 6)
        tlyCorp.Controls.Add(lblValRecCaption, 0, 7)
        tlyCorp.Controls.Add(lblValRec, 1, 7)
        tlyCorp.Controls.Add(lblValTotCaption, 0, 8)
        tlyCorp.Controls.Add(lblValTot, 1, 8)
        tlyCorp.Controls.Add(lblCodIndicatorCaption, 0, 9)
        tlyCorp.Controls.Add(lblCodIndicator, 1, 9)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 60)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(18, 14, 18, 6)
        tlyCorp.RowCount = 11
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 54F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.Size = New Size(1196, 580)
        tlyCorp.TabIndex = 1
        '
        ' lblSursaCaption
        '
        lblSursaCaption.Dock = DockStyle.Fill
        lblSursaCaption.Location = New Point(22, 14)
        lblSursaCaption.Margin = New Padding(4, 0, 4, 0)
        lblSursaCaption.Name = "lblSursaCaption"
        lblSursaCaption.Size = New Size(292, 60)
        lblSursaCaption.TabIndex = 0
        lblSursaCaption.Text = "Sursă / sector"
        lblSursaCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbSursa
        '
        cmbSursa.Dock = DockStyle.Fill
        cmbSursa.Location = New Point(322, 17)
        cmbSursa.Margin = New Padding(4, 3, 4, 3)
        cmbSursa.Name = "cmbSursa"
        cmbSursa.Size = New Size(852, 34)
        cmbSursa.TabIndex = 1
        tips.SetToolTipHeader(cmbSursa, "Sursa / sectorul rândului")
        tips.SetToolTipText(cmbSursa, "Sursele pe care le are programul din antet (tabela DefaProgram)." & vbLf &"Sursa aleasă în fereastra principală K-BOT e propusă, dacă aparține programului." & vbLf & "Lista de clasificații arată doar clasificațiile sursei alese.")
        '
        ' lblClasificatieCaption
        '
        lblClasificatieCaption.Dock = DockStyle.Fill
        lblClasificatieCaption.Location = New Point(22, 74)
        lblClasificatieCaption.Margin = New Padding(4, 0, 4, 0)
        lblClasificatieCaption.Name = "lblClasificatieCaption"
        lblClasificatieCaption.Size = New Size(292, 60)
        lblClasificatieCaption.TabIndex = 2
        lblClasificatieCaption.Text = "Clasificație"
        lblClasificatieCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbClasificatie
        '
        cmbClasificatie.Dock = DockStyle.Fill
        cmbClasificatie.Editable = True
        cmbClasificatie.FindAfterNChars = 2
        cmbClasificatie.FindAsYouType = True
        cmbClasificatie.InputMask = "00.00.00.00.00.00"
        cmbClasificatie.Location = New Point(322, 77)
        cmbClasificatie.Margin = New Padding(4, 3, 4, 3)
        cmbClasificatie.MaxDropDownItems = 14
        cmbClasificatie.Name = "cmbClasificatie"
        cmbClasificatie.Size = New Size(852, 34)
        cmbClasificatie.TabIndex = 3
        tips.SetToolTipHeader(cmbClasificatie, "Clasificația")
        tips.SetToolTipText(cmbClasificatie, "Tastați doar cifrele: punctele se pun singure." & vbLf & "După două cifre apare lista clasificațiilor potrivite; săgețile sus / jos și Enter aleg una." & vbLf & "Clasificațiile deja folosite în secțiunea A nu apar.")
        '
        ' lblDenumireCaption
        '
        lblDenumireCaption.Dock = DockStyle.Fill
        lblDenumireCaption.Location = New Point(22, 134)
        lblDenumireCaption.Margin = New Padding(4, 0, 4, 0)
        lblDenumireCaption.Name = "lblDenumireCaption"
        lblDenumireCaption.Size = New Size(292, 54)
        lblDenumireCaption.TabIndex = 4
        lblDenumireCaption.Text = "Denumire"
        lblDenumireCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblDenumire
        '
        lblDenumire.AutoEllipsis = True
        lblDenumire.Dock = DockStyle.Fill
        lblDenumire.Location = New Point(322, 134)
        lblDenumire.Margin = New Padding(4, 0, 4, 0)
        lblDenumire.Name = "lblDenumire"
        lblDenumire.Size = New Size(852, 54)
        lblDenumire.TabIndex = 5
        lblDenumire.Text = "—"
        lblDenumire.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblElementCaption
        '
        lblElementCaption.Dock = DockStyle.Fill
        lblElementCaption.Location = New Point(22, 188)
        lblElementCaption.Margin = New Padding(4, 0, 4, 0)
        lblElementCaption.Name = "lblElementCaption"
        lblElementCaption.Size = New Size(292, 60)
        lblElementCaption.TabIndex = 6
        lblElementCaption.Text = "Element fundamentare *"
        lblElementCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtElement
        '
        txtElement.BackColor = Color.Transparent
        txtElement.Dock = DockStyle.Fill
        txtElement.Location = New Point(322, 191)
        txtElement.Margin = New Padding(4, 3, 4, 3)
        txtElement.Name = "txtElement"
        txtElement.Size = New Size(852, 54)
        txtElement.TabIndex = 7
        txtElement.TextPadding = New Padding(12, 0, 12, 0)
        tips.SetToolTipHeader(txtElement, "Elementul de fundamentare")
        tips.SetToolTipText(txtElement, "Obligatoriu. Se completează cu denumirea clasificației la alegerea ei;" & vbLf & "se poate rescrie.")
        '
        ' lblParametriiCaption
        '
        lblParametriiCaption.Dock = DockStyle.Fill
        lblParametriiCaption.Location = New Point(22, 248)
        lblParametriiCaption.Margin = New Padding(4, 0, 4, 0)
        lblParametriiCaption.Name = "lblParametriiCaption"
        lblParametriiCaption.Size = New Size(292, 60)
        lblParametriiCaption.TabIndex = 8
        lblParametriiCaption.Text = "Parametrii"
        lblParametriiCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtParametrii
        '
        txtParametrii.BackColor = Color.Transparent
        txtParametrii.Dock = DockStyle.Fill
        txtParametrii.Location = New Point(322, 251)
        txtParametrii.Margin = New Padding(4, 3, 4, 3)
        txtParametrii.Name = "txtParametrii"
        txtParametrii.Size = New Size(852, 54)
        txtParametrii.TabIndex = 9
        txtParametrii.TextPadding = New Padding(12, 0, 12, 0)
        '
        ' lblValCurCaption
        '
        lblValCurCaption.Dock = DockStyle.Fill
        lblValCurCaption.Location = New Point(22, 308)
        lblValCurCaption.Margin = New Padding(4, 0, 4, 0)
        lblValCurCaption.Name = "lblValCurCaption"
        lblValCurCaption.Size = New Size(292, 60)
        lblValCurCaption.TabIndex = 10
        lblValCurCaption.Text = "Valoare curentă *"
        lblValCurCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtValCur
        '
        txtValCur.BackColor = Color.Transparent
        txtValCur.Dock = DockStyle.Fill
        txtValCur.Location = New Point(322, 311)
        txtValCur.Margin = New Padding(4, 3, 4, 3)
        txtValCur.Name = "txtValCur"
        txtValCur.Size = New Size(852, 54)
        txtValCur.TabIndex = 11
        txtValCur.TextAlign = HorizontalAlignment.Right
        txtValCur.TextPadding = New Padding(12, 0, 12, 0)
        tips.SetToolTipHeader(txtValCur, "Valoarea curentă")
        tips.SetToolTipText(txtValCur, "Obligatorie și diferită de 0. Negativă doar cât valoarea rămasă" & vbLf & "nu coboară sub valoarea recepțiilor.")
        '
        ' lblValPrecCaption
        '
        lblValPrecCaption.Dock = DockStyle.Fill
        lblValPrecCaption.Location = New Point(22, 368)
        lblValPrecCaption.Margin = New Padding(4, 0, 4, 0)
        lblValPrecCaption.Name = "lblValPrecCaption"
        lblValPrecCaption.Size = New Size(292, 48)
        lblValPrecCaption.TabIndex = 12
        lblValPrecCaption.Text = "Valoare precedentă"
        lblValPrecCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblValPrec
        '
        lblValPrec.Dock = DockStyle.Fill
        lblValPrec.Location = New Point(322, 368)
        lblValPrec.Margin = New Padding(4, 0, 16, 0)
        lblValPrec.Name = "lblValPrec"
        lblValPrec.Size = New Size(840, 48)
        lblValPrec.TabIndex = 13
        lblValPrec.Text = "0,00"
        lblValPrec.TextAlign = ContentAlignment.MiddleRight
        '
        ' lblValRecCaption
        '
        lblValRecCaption.Dock = DockStyle.Fill
        lblValRecCaption.Location = New Point(22, 416)
        lblValRecCaption.Margin = New Padding(4, 0, 4, 0)
        lblValRecCaption.Name = "lblValRecCaption"
        lblValRecCaption.Size = New Size(292, 48)
        lblValRecCaption.TabIndex = 14
        lblValRecCaption.Text = "Valoare recepții"
        lblValRecCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblValRec
        '
        lblValRec.Dock = DockStyle.Fill
        lblValRec.Location = New Point(322, 416)
        lblValRec.Margin = New Padding(4, 0, 16, 0)
        lblValRec.Name = "lblValRec"
        lblValRec.Size = New Size(840, 48)
        lblValRec.TabIndex = 15
        lblValRec.Text = "0,00"
        lblValRec.TextAlign = ContentAlignment.MiddleRight
        '
        ' lblValTotCaption
        '
        lblValTotCaption.Dock = DockStyle.Fill
        lblValTotCaption.Location = New Point(22, 464)
        lblValTotCaption.Margin = New Padding(4, 0, 4, 0)
        lblValTotCaption.Name = "lblValTotCaption"
        lblValTotCaption.Size = New Size(292, 48)
        lblValTotCaption.TabIndex = 16
        lblValTotCaption.Text = "Valoare totală"
        lblValTotCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblValTot
        '
        lblValTot.Dock = DockStyle.Fill
        lblValTot.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblValTot.Location = New Point(322, 464)
        lblValTot.Margin = New Padding(4, 0, 16, 0)
        lblValTot.Name = "lblValTot"
        lblValTot.Size = New Size(840, 48)
        lblValTot.TabIndex = 17
        lblValTot.Text = "0,00"
        lblValTot.TextAlign = ContentAlignment.MiddleRight
        tips.SetToolTipHeader(lblValTot, "Valoarea totală")
        tips.SetToolTipText(lblValTot, "Valoarea precedentă + valoarea curentă. Nu se tastează.")
        '
        ' lblCodIndicatorCaption
        '
        lblCodIndicatorCaption.Dock = DockStyle.Fill
        lblCodIndicatorCaption.Location = New Point(22, 512)
        lblCodIndicatorCaption.Margin = New Padding(4, 0, 4, 0)
        lblCodIndicatorCaption.Name = "lblCodIndicatorCaption"
        lblCodIndicatorCaption.Size = New Size(292, 48)
        lblCodIndicatorCaption.TabIndex = 18
        lblCodIndicatorCaption.Text = "Cod indicator"
        lblCodIndicatorCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblCodIndicator
        '
        lblCodIndicator.Dock = DockStyle.Fill
        lblCodIndicator.Location = New Point(322, 512)
        lblCodIndicator.Margin = New Padding(4, 0, 4, 0)
        lblCodIndicator.Name = "lblCodIndicator"
        lblCodIndicator.Size = New Size(852, 48)
        lblCodIndicator.TabIndex = 19
        lblCodIndicator.Text = "—"
        lblCodIndicator.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblCodIndicator, "Codul indicatorului")
        tips.SetToolTipText(lblCodIndicator, "Al clasificației, dacă angajamentul îl are deja;" & vbLf & "altfel unul nou, «!» + trei caractere, ca în Access.")
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Controls.Add(btnOk)
        pnlJos.Controls.Add(lblStare)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 640)
        pnlJos.Margin = New Padding(4)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(18, 10, 18, 20)
        pnlJos.Size = New Size(1196, 76)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Right
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(842, 10)
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(147, 46)
        btnRenunta.TabIndex = 1
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnOk
        '
        btnOk.Dock = DockStyle.Right
        btnOk.FlatStyle = FlatStyle.Flat
        btnOk.Location = New Point(989, 10)
        btnOk.Margin = New Padding(0)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(189, 46)
        btnOk.TabIndex = 2
        btnOk.Text = "Adaugă rândul"
        tips.SetToolTipHeader(btnOk, "Rândul în secțiunea A")
        tips.SetToolTipText(btnOk, "Pune rândul în secțiunea A. Documentul se scrie abia la «Salvează».")
        btnOk.UseVisualStyleBackColor = True
        '
        ' lblStare
        '
        lblStare.AutoSize = True
        lblStare.Location = New Point(18, 22)
        lblStare.Margin = New Padding(4, 0, 4, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(0, 22)
        lblStare.TabIndex = 0
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New Size(1196, 60)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Rând nou în secțiunea A"
        '
        ' DdfEditLinieAForm
        '
        AcceptButton = btnOk
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(1200, 720)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "DdfEditLinieAForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Rând nou în secțiunea A"
        pnlCard.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        pnlJos.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents tlyCorp As KBotTableLayoutPanel
    Friend WithEvents lblSursaCaption As Label
    Friend WithEvents cmbSursa As KBotComboBox
    Friend WithEvents lblClasificatieCaption As Label
    Friend WithEvents cmbClasificatie As KBotComboBox
    Friend WithEvents lblDenumireCaption As Label
    Friend WithEvents lblDenumire As Label
    Friend WithEvents lblElementCaption As Label
    Friend WithEvents txtElement As KBotTextField
    Friend WithEvents lblParametriiCaption As Label
    Friend WithEvents txtParametrii As KBotTextField
    Friend WithEvents lblValCurCaption As Label
    Friend WithEvents txtValCur As KBotTextField
    Friend WithEvents lblValPrecCaption As Label
    Friend WithEvents lblValPrec As Label
    Friend WithEvents lblValRecCaption As Label
    Friend WithEvents lblValRec As Label
    Friend WithEvents lblValTotCaption As Label
    Friend WithEvents lblValTot As Label
    Friend WithEvents lblCodIndicatorCaption As Label
    Friend WithEvents lblCodIndicator As Label
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnOk As Button
    Friend WithEvents btnRenunta As Button
End Class
