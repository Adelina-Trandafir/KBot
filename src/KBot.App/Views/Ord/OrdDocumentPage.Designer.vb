<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OrdDocumentPage
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
        previewPdf = New ReaderHostPreview()
        lblEmpty = New Label()
        SuspendLayout()
        '
        ' previewPdf — PDF-ul REAL al ordonanțării (aceeași suprafață ca pagina «Document» a
        ' DDF-ului). Se montează LENEȘ, din cod: vezi nota clasei.
        '
        previewPdf.BackColor = SystemColors.Window
        previewPdf.BorderStyle = BorderStyle.FixedSingle
        previewPdf.Dock = DockStyle.Fill
        previewPdf.Location = New Point(0, 0)
        previewPdf.Margin = New Padding(0)
        previewPdf.Name = "previewPdf"
        previewPdf.Size = New Size(849, 488)
        previewPdf.TabIndex = 0
        '
        ' lblEmpty — starea goală a paginii (fără ordonanțare selectată / fără PDF generat).
        ' Cât timp e vizibilă, suprafața PDF e ascunsă: fără generare în felia asta, suprafața
        ' „document lipsă" a lui ReaderHostPreview ar arăta un buton «Generează» care nu face
        ' nimic.
        '
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Margin = New Padding(4, 0, 4, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(849, 488)
        lblEmpty.TabIndex = 1
        lblEmpty.Text = "Selectați o ordonanțare din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        '
        ' OrdDocumentPage
        '
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(previewPdf)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "OrdDocumentPage"
        Size = New Size(849, 488)
        ResumeLayout(False)
    End Sub

    Friend WithEvents previewPdf As ReaderHostPreview
    Friend WithEvents lblEmpty As Label
End Class
