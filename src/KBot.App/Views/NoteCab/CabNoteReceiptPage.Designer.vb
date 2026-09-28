<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CabNoteReceiptPage
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
        ' previewPdf — the note's FOREXE receipt (the same surface as the «Document» page). Mounted
        ' LAZILY, from code; its «document lipsă» words are set in code (SetMissingTexts).
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
        ' lblEmpty — the page's empty state (no note selected).
        '
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Margin = New Padding(4, 0, 4, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(849, 488)
        lblEmpty.TabIndex = 1
        lblEmpty.Text = "Selectați o notă de corecție din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        '
        ' CabNoteReceiptPage
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(previewPdf)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "CabNoteReceiptPage"
        Size = New Size(849, 488)
        ResumeLayout(False)
    End Sub

    Friend WithEvents previewPdf As ReaderHostPreview
    Friend WithEvents lblEmpty As Label
End Class
