<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfDocumentPage
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
        components = New ComponentModel.Container()
        tips = New KBot.Controls.KBotToolTip(components)
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(DdfDocumentPage))
        tlyPDF = New TableLayoutPanel()
        previewPdf = New ReaderHostPreview()
        pnlBottomButtons = New Panel()
        tlyBottomButtons = New TableLayoutPanel()
        btnOpenInAdobe = New Button()
        btnSaveLocalCopy = New Button()
        pnlAdobe = New Panel()
        cboAdobeInst = New ComboBox()
        lblAdobeInst = New Label()
        cboAdobeMod = New ComboBox()
        lblAdobeMod = New Label()
        cboAdobeMotor = New ComboBox()
        lblAdobeMotor = New Label()
        lblAvizSetari = New Label()
        tlyPDF.SuspendLayout()
        pnlBottomButtons.SuspendLayout()
        tlyBottomButtons.SuspendLayout()
        pnlAdobe.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlyPDF
        ' 
        tlyPDF.ColumnCount = 1
        tlyPDF.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPDF.Controls.Add(previewPdf, 0, 0)
        tlyPDF.Controls.Add(pnlBottomButtons, 0, 1)
        tlyPDF.Dock = DockStyle.Fill
        tlyPDF.Location = New Point(0, 28)
        tlyPDF.Margin = New Padding(0)
        tlyPDF.Name = "tlyPDF"
        tlyPDF.RowCount = 2
        tlyPDF.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyPDF.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyPDF.Size = New Size(849, 460)
        tlyPDF.TabIndex = 3
        ' 
        ' previewPdf
        ' 
        previewPdf.BackColor = SystemColors.Window
        previewPdf.BorderStyle = BorderStyle.FixedSingle
        previewPdf.Dock = DockStyle.Fill
        previewPdf.Location = New Point(3, 3)
        previewPdf.Margin = New Padding(3, 3, 3, 0)
        previewPdf.Name = "previewPdf"
        previewPdf.Size = New Size(843, 417)
        previewPdf.TabIndex = 1
        ' 
        ' pnlBottomButtons
        ' 
        pnlBottomButtons.Controls.Add(tlyBottomButtons)
        pnlBottomButtons.Dock = DockStyle.Fill
        pnlBottomButtons.Location = New Point(0, 420)
        pnlBottomButtons.Margin = New Padding(0)
        pnlBottomButtons.Name = "pnlBottomButtons"
        pnlBottomButtons.Size = New Size(849, 40)
        pnlBottomButtons.TabIndex = 2
        ' 
        ' tlyBottomButtons
        ' 
        tlyBottomButtons.BackColor = Color.Transparent
        tlyBottomButtons.ColumnCount = 3
        tlyBottomButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBottomButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyBottomButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyBottomButtons.Controls.Add(btnOpenInAdobe, 1, 0)
        tlyBottomButtons.Controls.Add(btnSaveLocalCopy, 2, 0)
        tlyBottomButtons.Dock = DockStyle.Fill
        tlyBottomButtons.Location = New Point(0, 0)
        tlyBottomButtons.Margin = New Padding(0)
        tlyBottomButtons.Name = "tlyBottomButtons"
        tlyBottomButtons.RowCount = 1
        tlyBottomButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBottomButtons.Size = New Size(849, 40)
        tlyBottomButtons.TabIndex = 0
        ' 
        ' btnOpenInAdobe
        ' 
        btnOpenInAdobe.Dock = DockStyle.Fill
        btnOpenInAdobe.FlatAppearance.BorderSize = 0
        btnOpenInAdobe.FlatStyle = FlatStyle.Flat
        btnOpenInAdobe.Image = CType(resources.GetObject("btnOpenInAdobe.Image"), Image)
        btnOpenInAdobe.Location = New Point(689, 0)
        btnOpenInAdobe.Margin = New Padding(0)
        btnOpenInAdobe.Name = "btnOpenInAdobe"
        btnOpenInAdobe.Size = New Size(80, 40)
        btnOpenInAdobe.TabIndex = 0
        btnOpenInAdobe.UseVisualStyleBackColor = True
        ' 
        ' btnSaveLocalCopy
        ' 
        btnSaveLocalCopy.Dock = DockStyle.Fill
        btnSaveLocalCopy.FlatAppearance.BorderSize = 0
        btnSaveLocalCopy.FlatStyle = FlatStyle.Flat
        btnSaveLocalCopy.Image = CType(resources.GetObject("btnSaveLocalCopy.Image"), Image)
        btnSaveLocalCopy.Location = New Point(769, 0)
        btnSaveLocalCopy.Margin = New Padding(0)
        btnSaveLocalCopy.Name = "btnSaveLocalCopy"
        btnSaveLocalCopy.Size = New Size(80, 40)
        btnSaveLocalCopy.TabIndex = 1
        btnSaveLocalCopy.UseVisualStyleBackColor = True
        ' 
        ' pnlAdobe
        ' 
        pnlAdobe.Controls.Add(cboAdobeInst)
        pnlAdobe.Controls.Add(lblAdobeInst)
        pnlAdobe.Controls.Add(cboAdobeMod)
        pnlAdobe.Controls.Add(lblAdobeMod)
        pnlAdobe.Controls.Add(cboAdobeMotor)
        pnlAdobe.Controls.Add(lblAdobeMotor)
        pnlAdobe.Location = New Point(0, 0)
        pnlAdobe.Name = "pnlAdobe"
        pnlAdobe.Padding = New Padding(6, 4, 6, 4)
        pnlAdobe.Size = New Size(849, 37)
        pnlAdobe.TabIndex = 0
        pnlAdobe.Visible = False
        ' 
        ' cboAdobeInst
        ' 
        cboAdobeInst.Dock = DockStyle.Left
        cboAdobeInst.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeInst.FlatStyle = FlatStyle.Flat
        cboAdobeInst.Location = New Point(691, 4)
        cboAdobeInst.Name = "cboAdobeInst"
        cboAdobeInst.Size = New Size(158, 33)
        cboAdobeInst.TabIndex = 3
        ' 
        ' lblAdobeInst
        ' 
        lblAdobeInst.AutoSize = True
        lblAdobeInst.Dock = DockStyle.Left
        lblAdobeInst.Location = New Point(543, 4)
        lblAdobeInst.Name = "lblAdobeInst"
        lblAdobeInst.Padding = New Padding(16, 5, 8, 0)
        lblAdobeInst.Size = New Size(148, 30)
        lblAdobeInst.TabIndex = 2
        lblAdobeInst.Text = "Instanță nouă:"
        lblAdobeInst.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboAdobeMod
        ' 
        cboAdobeMod.Dock = DockStyle.Left
        cboAdobeMod.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeMod.FlatStyle = FlatStyle.Flat
        cboAdobeMod.Location = New Point(385, 4)
        cboAdobeMod.Name = "cboAdobeMod"
        cboAdobeMod.Size = New Size(158, 33)
        cboAdobeMod.TabIndex = 1
        ' 
        ' lblAdobeMod
        ' 
        lblAdobeMod.AutoSize = True
        lblAdobeMod.Dock = DockStyle.Left
        lblAdobeMod.Location = New Point(238, 4)
        lblAdobeMod.Name = "lblAdobeMod"
        lblAdobeMod.Padding = New Padding(0, 5, 8, 0)
        lblAdobeMod.Size = New Size(147, 30)
        lblAdobeMod.TabIndex = 0
        lblAdobeMod.Text = "Mod vizualizare:"
        lblAdobeMod.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboAdobeMotor
        ' 
        cboAdobeMotor.Dock = DockStyle.Left
        cboAdobeMotor.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeMotor.FlatStyle = FlatStyle.Flat
        cboAdobeMotor.Location = New Point(80, 4)
        cboAdobeMotor.Name = "cboAdobeMotor"
        cboAdobeMotor.Size = New Size(158, 33)
        cboAdobeMotor.TabIndex = 5
        ' 
        ' lblAdobeMotor
        ' 
        lblAdobeMotor.AutoSize = True
        lblAdobeMotor.Dock = DockStyle.Left
        lblAdobeMotor.Location = New Point(6, 4)
        lblAdobeMotor.Name = "lblAdobeMotor"
        lblAdobeMotor.Padding = New Padding(0, 5, 8, 0)
        lblAdobeMotor.Size = New Size(74, 30)
        lblAdobeMotor.TabIndex = 4
        lblAdobeMotor.Text = "Motor:"
        lblAdobeMotor.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblAvizSetari
        ' 
        lblAvizSetari.Dock = DockStyle.Top
        lblAvizSetari.Location = New Point(0, 0)
        lblAvizSetari.Name = "lblAvizSetari"
        lblAvizSetari.Padding = New Padding(8, 4, 8, 4)
        lblAvizSetari.Size = New Size(849, 28)
        lblAvizSetari.TabIndex = 4
        lblAvizSetari.TextAlign = ContentAlignment.MiddleLeft
        lblAvizSetari.Visible = False
        ' 
        ' DdfDocumentPage
        ' 
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(tlyPDF)
        Controls.Add(pnlAdobe)
        Controls.Add(lblAvizSetari)
        Margin = New Padding(4, 5, 4, 5)
        Name = "DdfDocumentPage"
        Size = New Size(849, 488)
        tlyPDF.ResumeLayout(False)
        pnlBottomButtons.ResumeLayout(False)
        tlyBottomButtons.ResumeLayout(False)
        pnlAdobe.ResumeLayout(False)
        pnlAdobe.PerformLayout()
        '
        ' tips — etichetele de survolare (felia 0035), toate în română.
        '
        tips.SetToolTipHeader(btnOpenInAdobe, "Deschide în Adobe")
        tips.SetToolTipText(btnOpenInAdobe, "Deschide documentul generat într-o fereastră Adobe Reader separată.")
        tips.SetToolTipHeader(btnSaveLocalCopy, "Salvează o copie")
        tips.SetToolTipText(btnSaveLocalCopy, "Salvează documentul generat într-un dosar ales de tine.")
        tips.SetToolTipHeader(cboAdobeMod, "Mod de afișare")
        tips.SetToolTipText(cboAdobeMod, "Cum se arată documentul: găzduit în fereastra K-BOT sau într-o fereastră Adobe proprie.")
        tips.SetToolTipHeader(cboAdobeInst, "Instanță Adobe")
        tips.SetToolTipText(cboAdobeInst, "Ce exemplar de Adobe primește documentul." & vbLf & "Adobe predă documentele unei instanțe care rulează deja.")
        tips.SetToolTipHeader(cboAdobeMotor, "Motor de afișare")
        tips.SetToolTipText(cboAdobeMotor, "Componenta care desenează PDF-ul.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents tlyPDF As TableLayoutPanel
    Friend WithEvents previewPdf As ReaderHostPreview
    Friend WithEvents pnlBottomButtons As Panel
    Friend WithEvents tlyBottomButtons As TableLayoutPanel
    Friend WithEvents btnOpenInAdobe As Button
    Friend WithEvents btnSaveLocalCopy As Button
    Friend WithEvents pnlAdobe As Panel
    Friend WithEvents lblAdobeMod As Label
    Friend WithEvents cboAdobeMod As ComboBox
    Friend WithEvents lblAdobeInst As Label
    Friend WithEvents cboAdobeInst As ComboBox
    Friend WithEvents lblAdobeMotor As Label
    Friend WithEvents cboAdobeMotor As ComboBox
    Friend WithEvents lblAvizSetari As Label
End Class
