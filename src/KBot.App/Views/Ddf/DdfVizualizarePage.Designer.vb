<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfVizualizarePage
    Inherits System.Windows.Forms.UserControl

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
        previewXfa = New XfaXmlPreview()
        lblPreviewGol = New Label()
        SuspendLayout()
        '
        ' previewXfa — suprafața implicită (reconstrucția din XML XFA). Adăugată PRIMA, deci
        ' stă în fața etichetei goale de dedesubt (regula ordinii Z, nu a andocării).
        '
        previewXfa.Dock = DockStyle.Fill
        previewXfa.Location = New Point(0, 0)
        previewXfa.Name = "previewXfa"
        previewXfa.Size = New Size(849, 488)
        previewXfa.TabIndex = 0
        '
        ' lblPreviewGol — plasa de dedesubt, când suprafața lipsește
        '
        lblPreviewGol.Dock = DockStyle.Fill
        lblPreviewGol.Font = New Font("Segoe UI", 10F)
        lblPreviewGol.Location = New Point(0, 0)
        lblPreviewGol.Name = "lblPreviewGol"
        lblPreviewGol.Size = New Size(849, 488)
        lblPreviewGol.TabIndex = 1
        lblPreviewGol.Text = "Selectați o revizie din arbore."
        lblPreviewGol.TextAlign = ContentAlignment.MiddleCenter
        '
        ' DdfVizualizarePage
        '
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(previewXfa)
        Controls.Add(lblPreviewGol)
        Margin = New Padding(4, 5, 4, 5)
        Name = "DdfVizualizarePage"
        Size = New Size(849, 488)
        ResumeLayout(False)
    End Sub

    Friend WithEvents previewXfa As XfaXmlPreview
    Friend WithEvents lblPreviewGol As Label
End Class
