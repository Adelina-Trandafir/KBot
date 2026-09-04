<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfVizualizarePage
    Inherits KBot.Theming.KBotThemedUserControl

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
        pagValori = New DdfValoriPage()
        tblHeader = New TableLayoutPanel()
        lblCodCaption = New Label()
        lblCod = New Label()
        lblDataCreareCaption = New Label()
        lblDataCreare = New Label()
        lblCompartimentCaption = New Label()
        lblCompartiment = New Label()
        lblStareCaption = New Label()
        lblCUAL = New Label()
        lblObiectDDFCaption = New Label()
        lblObiectDDF = New Label()
        lblBeneficiarCaption = New Label()
        lblBeneficiar = New Label()
        tlyMain = New TableLayoutPanel()
        tblHeader.SuspendLayout()
        tlyMain.SuspendLayout()
        SuspendLayout()
        ' 
        ' pagValori
        ' 
        pagValori.Dock = DockStyle.Fill
        pagValori.Location = New Point(4, 159)
        pagValori.Margin = New Padding(4, 5, 4, 5)
        pagValori.Name = "pagValori"
        pagValori.Size = New Size(655, 324)
        pagValori.TabIndex = 1
        ' 
        ' tblHeader
        ' 
        tblHeader.ColumnCount = 4
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tblHeader.Controls.Add(lblCodCaption, 0, 0)
        tblHeader.Controls.Add(lblCod, 1, 0)
        tblHeader.Controls.Add(lblDataCreareCaption, 0, 1)
        tblHeader.Controls.Add(lblDataCreare, 1, 1)
        tblHeader.Controls.Add(lblCompartimentCaption, 2, 1)
        tblHeader.Controls.Add(lblCompartiment, 3, 1)
        tblHeader.Controls.Add(lblStareCaption, 2, 0)
        tblHeader.Controls.Add(lblCUAL, 3, 0)
        tblHeader.Controls.Add(lblObiectDDFCaption, 0, 3)
        tblHeader.Controls.Add(lblObiectDDF, 1, 3)
        tblHeader.Controls.Add(lblBeneficiarCaption, 0, 2)
        tblHeader.Controls.Add(lblBeneficiar, 1, 2)
        tblHeader.Dock = DockStyle.Fill
        tblHeader.Location = New Point(4, 5)
        tblHeader.Margin = New Padding(4, 5, 4, 5)
        tblHeader.Name = "tblHeader"
        tblHeader.RowCount = 4
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tblHeader.Size = New Size(655, 144)
        tblHeader.TabIndex = 2
        ' 
        ' lblCodCaption
        ' 
        lblCodCaption.Dock = DockStyle.Fill
        lblCodCaption.Location = New Point(4, 0)
        lblCodCaption.Margin = New Padding(4, 0, 4, 0)
        lblCodCaption.Name = "lblCodCaption"
        lblCodCaption.Size = New Size(152, 30)
        lblCodCaption.TabIndex = 0
        lblCodCaption.Text = "Cod angajament:"
        lblCodCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCod
        ' 
        lblCod.Dock = DockStyle.Fill
        lblCod.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblCod.Location = New Point(164, 0)
        lblCod.Margin = New Padding(4, 0, 4, 0)
        lblCod.Name = "lblCod"
        lblCod.Size = New Size(159, 30)
        lblCod.TabIndex = 1
        lblCod.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataCreareCaption
        ' 
        lblDataCreareCaption.Dock = DockStyle.Fill
        lblDataCreareCaption.Location = New Point(4, 30)
        lblDataCreareCaption.Margin = New Padding(4, 0, 4, 0)
        lblDataCreareCaption.Name = "lblDataCreareCaption"
        lblDataCreareCaption.Size = New Size(152, 30)
        lblDataCreareCaption.TabIndex = 4
        lblDataCreareCaption.Text = "Data creare:"
        lblDataCreareCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataCreare
        ' 
        lblDataCreare.Dock = DockStyle.Fill
        lblDataCreare.Location = New Point(164, 30)
        lblDataCreare.Margin = New Padding(4, 0, 4, 0)
        lblDataCreare.Name = "lblDataCreare"
        lblDataCreare.Size = New Size(159, 30)
        lblDataCreare.TabIndex = 5
        lblDataCreare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCompartimentCaption
        ' 
        lblCompartimentCaption.Dock = DockStyle.Fill
        lblCompartimentCaption.Location = New Point(331, 30)
        lblCompartimentCaption.Margin = New Padding(4, 0, 4, 0)
        lblCompartimentCaption.Name = "lblCompartimentCaption"
        lblCompartimentCaption.Size = New Size(152, 30)
        lblCompartimentCaption.TabIndex = 6
        lblCompartimentCaption.Text = "Compartimentul"
        lblCompartimentCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCompartiment
        ' 
        lblCompartiment.Dock = DockStyle.Fill
        lblCompartiment.Location = New Point(491, 30)
        lblCompartiment.Margin = New Padding(4, 0, 4, 0)
        lblCompartiment.Name = "lblCompartiment"
        lblCompartiment.Size = New Size(160, 30)
        lblCompartiment.TabIndex = 7
        lblCompartiment.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblStareCaption
        ' 
        lblStareCaption.Dock = DockStyle.Fill
        lblStareCaption.Location = New Point(331, 0)
        lblStareCaption.Margin = New Padding(4, 0, 4, 0)
        lblStareCaption.Name = "lblStareCaption"
        lblStareCaption.Size = New Size(152, 30)
        lblStareCaption.TabIndex = 8
        lblStareCaption.Text = "CUAL:"
        lblStareCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCUAL
        ' 
        lblCUAL.Dock = DockStyle.Fill
        lblCUAL.Location = New Point(491, 0)
        lblCUAL.Margin = New Padding(4, 0, 4, 0)
        lblCUAL.Name = "lblCUAL"
        lblCUAL.Size = New Size(160, 30)
        lblCUAL.TabIndex = 9
        lblCUAL.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblObiectDDFCaption
        ' 
        lblObiectDDFCaption.Dock = DockStyle.Fill
        lblObiectDDFCaption.Location = New Point(4, 90)
        lblObiectDDFCaption.Margin = New Padding(4, 0, 4, 0)
        lblObiectDDFCaption.Name = "lblObiectDDFCaption"
        lblObiectDDFCaption.Size = New Size(152, 54)
        lblObiectDDFCaption.TabIndex = 12
        lblObiectDDFCaption.Text = "Obiect DDF"
        lblObiectDDFCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblObiectDDF
        ' 
        tblHeader.SetColumnSpan(lblObiectDDF, 3)
        lblObiectDDF.Dock = DockStyle.Fill
        lblObiectDDF.Location = New Point(164, 90)
        lblObiectDDF.Margin = New Padding(4, 0, 4, 0)
        lblObiectDDF.Name = "lblObiectDDF"
        lblObiectDDF.Size = New Size(487, 54)
        lblObiectDDF.TabIndex = 13
        lblObiectDDF.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblBeneficiarCaption
        ' 
        lblBeneficiarCaption.Dock = DockStyle.Fill
        lblBeneficiarCaption.Location = New Point(4, 60)
        lblBeneficiarCaption.Margin = New Padding(4, 0, 4, 0)
        lblBeneficiarCaption.Name = "lblBeneficiarCaption"
        lblBeneficiarCaption.Size = New Size(152, 30)
        lblBeneficiarCaption.TabIndex = 10
        lblBeneficiarCaption.Text = "Beneficiar:"
        lblBeneficiarCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblBeneficiar
        ' 
        tblHeader.SetColumnSpan(lblBeneficiar, 3)
        lblBeneficiar.Dock = DockStyle.Fill
        lblBeneficiar.Location = New Point(164, 60)
        lblBeneficiar.Margin = New Padding(4, 0, 4, 0)
        lblBeneficiar.Name = "lblBeneficiar"
        lblBeneficiar.Size = New Size(487, 30)
        lblBeneficiar.TabIndex = 11
        lblBeneficiar.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tblHeader, 0, 0)
        tlyMain.Controls.Add(pagValori, 0, 1)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 2
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 154F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.Size = New Size(663, 488)
        tlyMain.TabIndex = 3
        ' 
        ' DdfVizualizarePage
        ' 
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(tlyMain)
        Margin = New Padding(4, 5, 4, 5)
        Name = "DdfVizualizarePage"
        Size = New Size(663, 488)
        tblHeader.ResumeLayout(False)
        tlyMain.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pagValori As DdfValoriPage
    Friend WithEvents tblHeader As TableLayoutPanel
    Friend WithEvents lblCodCaption As Label
    Friend WithEvents lblCod As Label
    Friend WithEvents lblDataCreareCaption As Label
    Friend WithEvents lblDataCreare As Label
    Friend WithEvents lblCompartimentCaption As Label
    Friend WithEvents lblCompartiment As Label
    Friend WithEvents lblStareCaption As Label
    Friend WithEvents lblCUAL As Label
    Friend WithEvents lblBeneficiarCaption As Label
    Friend WithEvents lblBeneficiar As Label
    Friend WithEvents lblObiectDDFCaption As Label
    Friend WithEvents lblObiectDDF As Label
    Friend WithEvents tlyMain As TableLayoutPanel
End Class
