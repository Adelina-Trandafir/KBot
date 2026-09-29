#If DEBUG Then
' Slice 0078-05: picks one row of KBOT_BANC_PDF (000_DEMO) for the section B bench to load.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class BancPdfPickerForm
    Inherits KBot.Theming.KBotThemedForm

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    Friend WithEvents lstFisiere As ListView
    Friend WithEvents colId As ColumnHeader
    Friend WithEvents colPrimit As ColumnHeader
    Friend WithEvents colTip As ColumnHeader
    Friend WithEvents colIdDoc As ColumnHeader
    Friend WithEvents colFisier As ColumnHeader
    Friend WithEvents colSemnatura As ColumnHeader
    Friend WithEvents colDimensiune As ColumnHeader
    Friend WithEvents colPas As ColumnHeader
    Friend WithEvents colOperator As ColumnHeader
    Friend WithEvents pnlButoane As FlowLayoutPanel
    Friend WithEvents btnIncarca As Button
    Friend WithEvents btnRenunta As Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        lstFisiere = New ListView()
        colId = New ColumnHeader()
        colPrimit = New ColumnHeader()
        colTip = New ColumnHeader()
        colIdDoc = New ColumnHeader()
        colFisier = New ColumnHeader()
        colSemnatura = New ColumnHeader()
        colDimensiune = New ColumnHeader()
        colPas = New ColumnHeader()
        colOperator = New ColumnHeader()
        pnlButoane = New FlowLayoutPanel()
        btnIncarca = New Button()
        btnRenunta = New Button()
        pnlButoane.SuspendLayout()
        SuspendLayout()
        '
        ' lstFisiere -- one line per stored upload, newest first
        '
        lstFisiere.Columns.AddRange(New ColumnHeader() {colId, colPrimit, colTip, colIdDoc, colFisier, colSemnatura, colDimensiune, colPas, colOperator})
        lstFisiere.Dock = DockStyle.Fill
        lstFisiere.FullRowSelect = True
        lstFisiere.GridLines = True
        lstFisiere.HideSelection = False
        lstFisiere.MultiSelect = False
        lstFisiere.Name = "lstFisiere"
        lstFisiere.TabIndex = 0
        lstFisiere.UseCompatibleStateImageBehavior = False
        lstFisiere.View = View.Details
        colId.Text = "Id"
        colId.Width = 60
        colPrimit.Text = "Primit"
        colPrimit.Width = 170
        colTip.Text = "Tip"
        colTip.Width = 50
        colIdDoc.Text = "IdDoc"
        colIdDoc.Width = 60
        colFisier.Text = "Fișier"
        colFisier.Width = 250
        colSemnatura.Text = "Roluri"
        colSemnatura.Width = 130
        colDimensiune.Text = "Octeți"
        colDimensiune.TextAlign = HorizontalAlignment.Right
        colDimensiune.Width = 90
        colPas.Text = "Pas"
        colPas.Width = 260
        colOperator.Text = "Operator"
        colOperator.Width = 110
        '
        ' pnlButoane
        '
        pnlButoane.Controls.Add(btnIncarca)
        pnlButoane.Controls.Add(btnRenunta)
        pnlButoane.Dock = DockStyle.Bottom
        pnlButoane.FlowDirection = FlowDirection.RightToLeft
        pnlButoane.Height = 44
        pnlButoane.Name = "pnlButoane"
        pnlButoane.Padding = New Padding(8, 6, 8, 4)
        pnlButoane.TabIndex = 1
        '
        ' btnIncarca
        '
        btnIncarca.AutoSize = True
        btnIncarca.Name = "btnIncarca"
        btnIncarca.Padding = New Padding(8, 2, 8, 2)
        btnIncarca.TabIndex = 0
        btnIncarca.Text = "Încarcă"
        btnIncarca.UseVisualStyleBackColor = True
        '
        ' btnRenunta
        '
        btnRenunta.AutoSize = True
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New Padding(8, 2, 8, 2)
        btnRenunta.TabIndex = 1
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' BancPdfPickerForm
        '
        AcceptButton = btnIncarca
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(1240, 560)
        ' Children in REVERSE dock order: Fill first, then the docked edges.
        Controls.Add(lstFisiere)
        Controls.Add(pnlButoane)
        Name = "BancPdfPickerForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "PDF-uri din tabela de probă (KBOT_BANC_PDF, 000_DEMO)"
        pnlButoane.ResumeLayout(False)
        pnlButoane.PerformLayout()
        ResumeLayout(False)
    End Sub

End Class
#End If
