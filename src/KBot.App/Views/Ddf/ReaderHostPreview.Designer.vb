<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ReaderHostPreview
    Inherits System.Windows.Forms.UserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            ' Eliberăm fereastra Adobe găzduită ÎNAINTE de a distruge controlul, ca să nu
            ' rămână un proces orfan (boundary de resurse — vezi DetachReader).
            If disposing Then DetachReader()
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        pnlHost = New Panel()
        pnlMissing = New Panel()
        tblMissing = New TableLayoutPanel()
        lblMissing = New Label()
        btnGenereaza = New Button()
        lblMessage = New Label()
        pnlMissing.SuspendLayout()
        tblMissing.SuspendLayout()
        SuspendLayout()
        '
        ' pnlHost — panoul în care se reparentează fereastra Adobe Reader
        '
        pnlHost.Dock = DockStyle.Fill
        pnlHost.Location = New Point(0, 0)
        pnlHost.Name = "pnlHost"
        pnlHost.TabIndex = 0
        pnlHost.Visible = False
        '
        ' pnlMissing — starea „document lipsă"
        '
        pnlMissing.Controls.Add(tblMissing)
        pnlMissing.Dock = DockStyle.Fill
        pnlMissing.Location = New Point(0, 0)
        pnlMissing.Name = "pnlMissing"
        pnlMissing.TabIndex = 1
        pnlMissing.Visible = False
        '
        ' tblMissing — mesaj (sus) + buton (jos), ambele centrate
        '
        tblMissing.ColumnCount = 1
        tblMissing.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tblMissing.Controls.Add(lblMissing, 0, 0)
        tblMissing.Controls.Add(btnGenereaza, 0, 1)
        tblMissing.Dock = DockStyle.Fill
        tblMissing.Name = "tblMissing"
        tblMissing.RowCount = 2
        tblMissing.RowStyles.Add(New RowStyle(SizeType.Percent, 55.0F))
        tblMissing.RowStyles.Add(New RowStyle(SizeType.Percent, 45.0F))
        tblMissing.TabIndex = 0
        '
        ' lblMissing
        '
        lblMissing.Dock = DockStyle.Fill
        lblMissing.Font = New Font("Segoe UI", 10F)
        lblMissing.Name = "lblMissing"
        lblMissing.TabIndex = 0
        lblMissing.Text = "Documentul nu a fost încă generat."
        lblMissing.TextAlign = ContentAlignment.BottomCenter
        '
        ' btnGenereaza — ridică GenerateRequested
        '
        btnGenereaza.Anchor = AnchorStyles.Top
        btnGenereaza.AutoSize = True
        btnGenereaza.FlatStyle = FlatStyle.Flat
        btnGenereaza.Name = "btnGenereaza"
        btnGenereaza.Padding = New Padding(14, 6, 14, 6)
        btnGenereaza.TabIndex = 1
        btnGenereaza.Text = "Generează documentul"
        btnGenereaza.UseVisualStyleBackColor = True
        '
        ' lblMessage — starea generică
        '
        lblMessage.Dock = DockStyle.Fill
        lblMessage.Font = New Font("Segoe UI", 10F)
        lblMessage.Location = New Point(0, 0)
        lblMessage.Name = "lblMessage"
        lblMessage.TabIndex = 2
        lblMessage.Text = "Selectați o revizie din arbore."
        lblMessage.TextAlign = ContentAlignment.MiddleCenter
        '
        ' ReaderHostPreview
        '
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(pnlHost)
        Controls.Add(pnlMissing)
        Controls.Add(lblMessage)
        Name = "ReaderHostPreview"
        Size = New Size(641, 460)
        pnlMissing.ResumeLayout(False)
        tblMissing.ResumeLayout(False)
        tblMissing.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlHost As Panel
    Friend WithEvents pnlMissing As Panel
    Friend WithEvents tblMissing As TableLayoutPanel
    Friend WithEvents lblMissing As Label
    Friend WithEvents btnGenereaza As Button
    Friend WithEvents lblMessage As Label
End Class
