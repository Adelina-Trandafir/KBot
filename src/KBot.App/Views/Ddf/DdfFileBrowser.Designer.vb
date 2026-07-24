<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfFileBrowser
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
        grid = New Controls.KBotDataView()
        lblEmpty = New Label()
        SuspendLayout()
        '
        ' grid — lista PDF-urilor (read-only)
        '
        grid.AlternatingRows = True
        grid.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.ToContent
        grid.BackColor = SystemColors.Window
        grid.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
        grid.Dock = DockStyle.Fill
        grid.HeaderHeight = 30
        grid.Location = New Point(0, 0)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 28
        grid.ShowHeader = True
        grid.ShowTotalsRow = False
        grid.Size = New Size(641, 460)
        grid.TabIndex = 0
        '
        ' lblEmpty — starea goală (rădăcină lipsă / niciun fișier)
        '
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.TabIndex = 1
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        '
        ' DdfFileBrowser
        '
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(grid)
        Controls.Add(lblEmpty)
        Name = "DdfFileBrowser"
        Size = New Size(641, 460)
        ResumeLayout(False)
    End Sub

    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents lblEmpty As Label
End Class
