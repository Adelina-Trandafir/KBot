<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfDocumentPage
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(DdfDocumentPage))
        tips = New KBot.Controls.KBotToolTip(components)
        btnOpenInAdobe = New Button()
        btnSaveLocalCopy = New Button()
        tlyPDF = New Controls.KBotTableLayoutPanel()
        previewPdf = New ReaderHostPreview()
        pnlBottomButtons = New Panel()
        tlyBottomButtons = New Controls.KBotTableLayoutPanel()
        tlyPDF.SuspendLayout()
        pnlBottomButtons.SuspendLayout()
        tlyBottomButtons.SuspendLayout()
        SuspendLayout()
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
        tips.SetToolTipHeader(btnOpenInAdobe, "Deschide în Adobe")
        tips.SetToolTipText(btnOpenInAdobe, "Deschide documentul generat într-o fereastră Adobe Reader separată.")
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
        tips.SetToolTipHeader(btnSaveLocalCopy, "Salvează o copie")
        tips.SetToolTipText(btnSaveLocalCopy, "Salvează documentul generat într-un dosar ales de tine.")
        btnSaveLocalCopy.UseVisualStyleBackColor = True
        ' 
        ' tlyPDF
        ' 
        tlyPDF.ColumnCount = 1
        tlyPDF.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPDF.Controls.Add(previewPdf, 0, 0)
        tlyPDF.Controls.Add(pnlBottomButtons, 0, 1)
        tlyPDF.Dock = DockStyle.Fill
        tlyPDF.Location = New Point(0, 0)
        tlyPDF.Margin = New Padding(0)
        tlyPDF.Name = "tlyPDF"
        tlyPDF.RowCount = 2
        tlyPDF.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyPDF.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyPDF.Size = New Size(849, 488)
        tlyPDF.TabIndex = 3
        ' 
        ' previewPdf
        ' 
        previewPdf.BackColor = SystemColors.Window
        previewPdf.BorderStyle = BorderStyle.FixedSingle
        previewPdf.Dock = DockStyle.Fill
        previewPdf.Font = New Font("Calibri", 9F)
        previewPdf.Location = New Point(3, 3)
        previewPdf.Margin = New Padding(3, 3, 3, 0)
        previewPdf.Name = "previewPdf"
        previewPdf.Size = New Size(843, 445)
        previewPdf.TabIndex = 1
        ' 
        ' pnlBottomButtons
        ' 
        pnlBottomButtons.Controls.Add(tlyBottomButtons)
        pnlBottomButtons.Dock = DockStyle.Fill
        pnlBottomButtons.Location = New Point(0, 448)
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
        ' DdfDocumentPage
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyPDF)
        Margin = New Padding(4, 5, 4, 5)
        Name = "DdfDocumentPage"
        Size = New Size(849, 488)
        tlyPDF.ResumeLayout(False)
        pnlBottomButtons.ResumeLayout(False)
        tlyBottomButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents tlyPDF As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents previewPdf As ReaderHostPreview
    Friend WithEvents pnlBottomButtons As Panel
    Friend WithEvents tlyBottomButtons As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnOpenInAdobe As Button
    Friend WithEvents btnSaveLocalCopy As Button
End Class
