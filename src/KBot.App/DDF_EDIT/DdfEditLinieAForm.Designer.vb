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
' Left column = captions, right column = the fields. Slice 0081-12 (operator, 26.09.2026): every
' value is in ONE row of `grdValori` -- Buget, Val. receptii, Disponibil (= buget - receptii),
' Val. precedenta, Val. curenta (the only editable cell), Val. ramasa (= disponibil - curenta);
' 90 wide, Standard format, no footer. The indicator code stays a label. All controls are declared
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(DdfEditLinieAForm))
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn9 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn10 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn11 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn12 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        cmbSursa = New KBotComboBox()
        cmbClasificatie = New KBotComboBox()
        txtElement = New KBotTextField()
        lblCodIndicator = New Label()
        grdValori = New KBotDataView()
        btnOk = New Button()
        pnlCard = New Panel()
        tlyCorp = New KBotTableLayoutPanel()
        lblSursaCaption = New Label()
        lblClasificatieCaption = New Label()
        lblDenumireCaption = New Label()
        lblDenumire = New Label()
        lblElementCaption = New Label()
        lblParametriiCaption = New Label()
        txtParametrii = New KBotTextField()
        lblCodIndicatorCaption = New Label()
        pnlJos = New Panel()
        btnRenunta = New Button()
        lblStare = New Label()
        capBar = New KBotCaptionBar()
        CType(grdValori, ComponentModel.ISupportInitialize).BeginInit()
        pnlCard.SuspendLayout()
        tlyCorp.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        ' 
        ' cmbSursa
        ' 
        cmbSursa.Dock = DockStyle.Fill
        cmbSursa.DrawMode = DrawMode.OwnerDrawFixed
        cmbSursa.DropDownStyle = ComboBoxStyle.DropDownList
        cmbSursa.FlatStyle = FlatStyle.Flat
        cmbSursa.ItemHeight = 31
        cmbSursa.Location = New Point(322, 17)
        cmbSursa.Margin = New Padding(4, 3, 4, 3)
        cmbSursa.Name = "cmbSursa"
        cmbSursa.Size = New Size(520, 37)
        cmbSursa.TabIndex = 1
        tips.SetToolTipHeader(cmbSursa, "Sursa / sectorul rândului")
        tips.SetToolTipText(cmbSursa, resources.GetString("cmbSursa.ToolTipText"))
        ' 
        ' cmbClasificatie
        ' 
        cmbClasificatie.Dock = DockStyle.Fill
        cmbClasificatie.DrawMode = DrawMode.OwnerDrawFixed
        cmbClasificatie.Editable = True
        cmbClasificatie.FindAfterNChars = 2
        cmbClasificatie.FindAsYouType = True
        cmbClasificatie.FlatStyle = FlatStyle.Flat
        cmbClasificatie.InputMask = "00.00.00.00.00.00"
        cmbClasificatie.ItemHeight = 31
        cmbClasificatie.Location = New Point(322, 77)
        cmbClasificatie.Margin = New Padding(4, 3, 4, 3)
        cmbClasificatie.MaxDropDownItems = 14
        cmbClasificatie.Name = "cmbClasificatie"
        cmbClasificatie.Size = New Size(520, 37)
        cmbClasificatie.TabIndex = 3
        tips.SetToolTipHeader(cmbClasificatie, "Clasificația")
        tips.SetToolTipText(cmbClasificatie, "Tastați doar cifrele: punctele se pun singure." & vbLf & "După două cifre apare lista clasificațiilor potrivite; săgețile sus / jos și Enter aleg una." & vbLf & "Clasificațiile deja folosite în secțiunea A nu apar.")
        ' 
        ' txtElement
        ' 
        txtElement.BackColor = Color.Transparent
        txtElement.Dock = DockStyle.Fill
        txtElement.Location = New Point(322, 191)
        txtElement.Margin = New Padding(4, 3, 4, 3)
        txtElement.Name = "txtElement"
        txtElement.Size = New Size(520, 54)
        txtElement.TabIndex = 7
        txtElement.TextPadding = New Padding(12, 0, 12, 0)
        tips.SetToolTipHeader(txtElement, "Elementul de fundamentare")
        tips.SetToolTipText(txtElement, "Obligatoriu. Se completează cu denumirea clasificației la alegerea ei;" & vbLf & "se poate rescrie.")
        ' 
        ' lblCodIndicator
        ' 
        lblCodIndicator.Dock = DockStyle.Fill
        lblCodIndicator.Location = New Point(322, 308)
        lblCodIndicator.Margin = New Padding(4, 0, 4, 0)
        lblCodIndicator.Name = "lblCodIndicator"
        lblCodIndicator.Size = New Size(520, 48)
        lblCodIndicator.TabIndex = 11
        lblCodIndicator.Text = "—"
        lblCodIndicator.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblCodIndicator, "Codul indicatorului")
        tips.SetToolTipText(lblCodIndicator, "Al clasificației, dacă angajamentul îl are deja;" & vbLf & "altfel unul nou, «!» + trei caractere, ca în Access.")
        ' 
        ' grdValori
        ' 
        grdValori.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grdValori.BackColor = SystemColors.Window
        grdValori.ColumnFillMode = KBotFillMode.FirstColumn
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn7.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn7.HeaderText = "Buget"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "buget"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn7.Width = 90
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn8.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn8.DecimalPlaces = 2
        KBotDataColumn8.Format = KBotFormat.Standard
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn8.HeaderText = "Val. recepții"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "val_rec"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn8.ValueType = KBotValueType.Number
        KBotDataColumn8.Width = 90
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn9.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn9.HeaderText = "Disponibil"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "disponibil"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBotValueType.Number
        KBotDataColumn9.Width = 90
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn10.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn10.DecimalPlaces = 2
        KBotDataColumn10.Format = KBotFormat.Standard
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn10.HeaderText = "Val. precedentă"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "val_prec"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn10.ValueType = KBotValueType.Number
        KBotDataColumn10.Width = 90
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn11.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn11.DecimalPlaces = 2
        KBotDataColumn11.Format = KBotFormat.Standard
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn11.HeaderText = "Val. curentă *"
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "val_cur"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn11.ValueType = KBotValueType.Number
        KBotDataColumn11.Width = 90
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn12.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn12.DecimalPlaces = 2
        KBotDataColumn12.Format = KBotFormat.Standard
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn12.HeaderText = "Val. rămasă"
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "val_ramasa"
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.ReadOnly = True
        KBotDataColumn12.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn12.ValueType = KBotValueType.Number
        KBotDataColumn12.Width = 90
        grdValori.Columns.Add(KBotDataColumn7)
        grdValori.Columns.Add(KBotDataColumn8)
        grdValori.Columns.Add(KBotDataColumn9)
        grdValori.Columns.Add(KBotDataColumn10)
        grdValori.Columns.Add(KBotDataColumn11)
        grdValori.Columns.Add(KBotDataColumn12)
        tlyCorp.SetColumnSpan(grdValori, 2)
        grdValori.Dock = DockStyle.Fill
        grdValori.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        grdValori.Location = New Point(22, 362)
        grdValori.Margin = New Padding(4, 6, 4, 6)
        grdValori.Name = "grdValori"
        grdValori.ShrinkColumnsToFit = False
        grdValori.Size = New Size(820, 118)
        grdValori.TabIndex = 12
        tips.SetToolTipHeader(grdValori, "Valorile rândului")
        tips.SetToolTipText(grdValori, "Se tastează doar valoarea curentă: obligatorie și diferită de 0." & vbLf & "Disponibil = buget − recepții; valoarea rămasă = disponibil − valoarea curentă.")
        ' 
        ' btnOk
        ' 
        btnOk.Dock = DockStyle.Right
        btnOk.FlatStyle = FlatStyle.Flat
        btnOk.Location = New Point(657, 10)
        btnOk.Margin = New Padding(0)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(189, 46)
        btnOk.TabIndex = 2
        btnOk.Text = "Adaugă rândul"
        tips.SetToolTipHeader(btnOk, "Rândul în secțiunea A")
        tips.SetToolTipText(btnOk, "Pune rândul în secțiunea A. Documentul se scrie abia la «Salvează».")
        btnOk.UseVisualStyleBackColor = True
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
        pnlCard.Size = New Size(864, 672)
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
        tlyCorp.Controls.Add(lblCodIndicatorCaption, 0, 5)
        tlyCorp.Controls.Add(lblCodIndicator, 1, 5)
        tlyCorp.Controls.Add(grdValori, 0, 6)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 60)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(18, 14, 18, 6)
        tlyCorp.RowCount = 8
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 54F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 130F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.Size = New Size(864, 536)
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
        lblDenumire.Size = New Size(520, 54)
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
        txtParametrii.Size = New Size(520, 54)
        txtParametrii.TabIndex = 9
        txtParametrii.TextPadding = New Padding(12, 0, 12, 0)
        ' 
        ' lblCodIndicatorCaption
        ' 
        lblCodIndicatorCaption.Dock = DockStyle.Fill
        lblCodIndicatorCaption.Location = New Point(22, 308)
        lblCodIndicatorCaption.Margin = New Padding(4, 0, 4, 0)
        lblCodIndicatorCaption.Name = "lblCodIndicatorCaption"
        lblCodIndicatorCaption.Size = New Size(292, 48)
        lblCodIndicatorCaption.TabIndex = 10
        lblCodIndicatorCaption.Text = "Cod indicator"
        lblCodIndicatorCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlJos
        ' 
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Controls.Add(btnOk)
        pnlJos.Controls.Add(lblStare)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 596)
        pnlJos.Margin = New Padding(4)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(18, 10, 18, 20)
        pnlJos.Size = New Size(864, 76)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        ' 
        ' btnRenunta
        ' 
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Right
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(510, 10)
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(147, 46)
        btnRenunta.TabIndex = 1
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
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
        capBar.Size = New Size(864, 60)
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
        ClientSize = New Size(868, 676)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "DdfEditLinieAForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Rând nou în secțiunea A"
        CType(grdValori, ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents lblCodIndicatorCaption As Label
    Friend WithEvents lblCodIndicator As Label
    Friend WithEvents grdValori As KBotDataView
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnOk As Button
    Friend WithEvents btnRenunta As Button
End Class
