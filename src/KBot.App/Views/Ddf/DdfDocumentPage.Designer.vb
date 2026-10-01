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
        tips = New KBot.Controls.KBotToolTip(components)
        tlyPDF = New Controls.KBotTableLayoutPanel()
        previewPdf = New ReaderHostPreview()
        tlyPDF.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlyPDF
        ' 
        tlyPDF.ColumnCount = 1
        tlyPDF.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPDF.Controls.Add(previewPdf, 0, 0)
        tlyPDF.Dock = DockStyle.Fill
        tlyPDF.Location = New Point(0, 0)
        tlyPDF.Margin = New Padding(0)
        tlyPDF.Name = "tlyPDF"
        tlyPDF.RowCount = 1
        tlyPDF.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyPDF.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
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
        previewPdf.Size = New Size(843, 485)
        previewPdf.TabIndex = 1
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
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents tlyPDF As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents previewPdf As ReaderHostPreview
End Class
