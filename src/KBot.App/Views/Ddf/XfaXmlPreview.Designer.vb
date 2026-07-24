<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class XfaXmlPreview
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
        pnlContent = New Panel()
        grid = New Controls.KBotDataView()
        pnlHeader = New Panel()
        lblNota = New Label()
        tblHeader = New TableLayoutPanel()
        pnlMissing = New Panel()
        tblMissing = New TableLayoutPanel()
        lblMissing = New Label()
        btnGenereaza = New Button()
        lblMessage = New Label()
        pnlContent.SuspendLayout()
        pnlHeader.SuspendLayout()
        pnlMissing.SuspendLayout()
        tblMissing.SuspendLayout()
        SuspendLayout()
        '
        ' pnlContent — antetul (sus) peste grila secțiunii A (Fill)
        '
        ' Ordine INVERSĂ de andocare: grila (Fill) întâi, apoi antetul (Top).
        pnlContent.Controls.Add(grid)
        pnlContent.Controls.Add(pnlHeader)
        pnlContent.Dock = DockStyle.Fill
        pnlContent.Location = New Point(0, 0)
        pnlContent.Name = "pnlContent"
        pnlContent.TabIndex = 0
        pnlContent.Visible = False
        '
        ' grid — liniile secțiunii A (read-only)
        '
        grid.AlternatingRows = True
        grid.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.ToContent
        grid.BackColor = SystemColors.Window
        grid.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
        grid.Dock = DockStyle.Fill
        grid.HeaderHeight = 30
        grid.Location = New Point(0, 120)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 28
        grid.ShowHeader = True
        grid.ShowTotalsRow = False
        grid.TabIndex = 1
        '
        ' pnlHeader — perechile de antet + nota
        '
        ' Ordine INVERSĂ: nota (Top, adăugată prima => cea mai jos) apoi tabelul (Top).
        pnlHeader.Controls.Add(lblNota)
        pnlHeader.Controls.Add(tblHeader)
        pnlHeader.Dock = DockStyle.Top
        pnlHeader.Height = 120
        pnlHeader.Location = New Point(0, 0)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Padding = New Padding(8, 6, 8, 6)
        pnlHeader.TabIndex = 0
        '
        ' tblHeader — perechile etichetă/valoare, umplute la rulare
        '
        tblHeader.AutoSize = True
        tblHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tblHeader.ColumnCount = 2
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170.0F))
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        tblHeader.Dock = DockStyle.Top
        tblHeader.Name = "tblHeader"
        tblHeader.TabIndex = 0
        '
        ' lblNota — descrierea obiectului (Desc_Scurta/Lunga), sub perechi
        '
        lblNota.AutoSize = False
        lblNota.Dock = DockStyle.Fill
        lblNota.Name = "lblNota"
        lblNota.TabIndex = 1
        lblNota.Text = String.Empty
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
        ' lblMessage — starea generică (selectați o revizie / nu pot citi documentul)
        '
        lblMessage.Dock = DockStyle.Fill
        lblMessage.Font = New Font("Segoe UI", 10F)
        lblMessage.Location = New Point(0, 0)
        lblMessage.Name = "lblMessage"
        lblMessage.TabIndex = 2
        lblMessage.Text = "Selectați o revizie din arbore."
        lblMessage.TextAlign = ContentAlignment.MiddleCenter
        '
        ' XfaXmlPreview
        '
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(pnlContent)
        Controls.Add(pnlMissing)
        Controls.Add(lblMessage)
        Name = "XfaXmlPreview"
        Size = New Size(641, 460)
        pnlContent.ResumeLayout(False)
        pnlHeader.ResumeLayout(False)
        pnlHeader.PerformLayout()
        pnlMissing.ResumeLayout(False)
        tblMissing.ResumeLayout(False)
        tblMissing.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlContent As Panel
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblNota As Label
    Friend WithEvents tblHeader As TableLayoutPanel
    Friend WithEvents pnlMissing As Panel
    Friend WithEvents tblMissing As TableLayoutPanel
    Friend WithEvents lblMissing As Label
    Friend WithEvents btnGenereaza As Button
    Friend WithEvents lblMessage As Label
End Class
