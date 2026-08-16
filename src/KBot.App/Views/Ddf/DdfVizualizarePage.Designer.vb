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
        pagValori = New DdfValoriPage()
        lblNota = New Label()
        SuspendLayout()
        ' 
        ' pagValori
        ' 
        pagValori.Dock = DockStyle.Fill
        pagValori.Location = New Point(0, 108)
        pagValori.Margin = New Padding(4, 5, 4, 5)
        pagValori.Name = "pagValori"
        pagValori.Size = New Size(840, 372)
        pagValori.TabIndex = 1
        ' 
        ' lblNota
        ' 
        lblNota.Dock = DockStyle.Top
        lblNota.Font = New Font("Segoe UI", 10F)
        lblNota.Location = New Point(0, 0)
        lblNota.Name = "lblNota"
        lblNota.Padding = New Padding(8)
        lblNota.Size = New Size(840, 108)
        lblNota.TabIndex = 0
        lblNota.Text = "Selectați o revizie din arbore."
        lblNota.TextAlign = ContentAlignment.TopLeft
        ' Antetul e text de date: un «&» dintr-un nume de instituție trebuie să se vadă, nu să
        ' sublinieze litera următoare.
        lblNota.UseMnemonic = False
        ' 
        ' DdfVizualizarePage
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(pagValori)
        Controls.Add(lblNota)
        Margin = New Padding(4, 5, 4, 5)
        Name = "DdfVizualizarePage"
        Size = New Size(840, 480)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pagValori As DdfValoriPage
    Friend WithEvents lblNota As Label
End Class
