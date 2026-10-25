<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AvcReportForm
    Inherits KBot.Theming.KBotThemedForm

    ' Every control is declared HERE so the form renders in the VS designer (docs/kbot-forms-ui-convention.md).
    ' Pixel values are logical, saved at 96 dpi.
    Private components As System.ComponentModel.IContainer

    Friend WithEvents tlpRoot As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents webRaport As System.Windows.Forms.WebBrowser
    Friend WithEvents tlpButoane As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnInchide As System.Windows.Forms.Button
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents btnSalveaza As System.Windows.Forms.Button
    Friend WithEvents sfdHtml As System.Windows.Forms.SaveFileDialog

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        tlpRoot = New Global.KBot.Controls.KBotTableLayoutPanel()
        webRaport = New System.Windows.Forms.WebBrowser()
        tlpButoane = New Global.KBot.Controls.KBotTableLayoutPanel()
        btnInchide = New System.Windows.Forms.Button()
        lblInfo = New System.Windows.Forms.Label()
        btnSalveaza = New System.Windows.Forms.Button()
        sfdHtml = New System.Windows.Forms.SaveFileDialog()
        tlpRoot.SuspendLayout()
        tlpButoane.SuspendLayout()
        SuspendLayout()
        '
        ' tlpRoot
        '
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpRoot.Controls.Add(webRaport, 0, 0)
        tlpRoot.Controls.Add(tlpButoane, 0, 1)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Name = "tlpRoot"
        tlpRoot.RowCount = 2
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 44.0F))
        tlpRoot.TabIndex = 0
        '
        ' webRaport
        '
        webRaport.AllowWebBrowserDrop = False
        webRaport.Dock = DockStyle.Fill
        webRaport.IsWebBrowserContextMenuEnabled = False
        webRaport.Margin = New Padding(0)
        webRaport.Name = "webRaport"
        webRaport.ScriptErrorsSuppressed = True
        webRaport.TabIndex = 0
        '
        ' tlpButoane
        '
        tlpButoane.ColumnCount = 3
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170.0F))
        tlpButoane.Controls.Add(btnInchide, 0, 0)
        tlpButoane.Controls.Add(lblInfo, 1, 0)
        tlpButoane.Controls.Add(btnSalveaza, 2, 0)
        tlpButoane.Dock = DockStyle.Fill
        tlpButoane.Name = "tlpButoane"
        tlpButoane.RowCount = 1
        tlpButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpButoane.TabIndex = 1
        '
        ' btnInchide
        '
        btnInchide.DialogResult = DialogResult.Cancel
        btnInchide.Dock = DockStyle.Fill
        btnInchide.Name = "btnInchide"
        btnInchide.TabIndex = 0
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' lblInfo
        '
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Name = "lblInfo"
        lblInfo.TabIndex = 1
        lblInfo.Text = "Raportul arată datele deja citite; nu interoghează serverul."
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnSalveaza
        '
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.TabIndex = 2
        btnSalveaza.Text = "Salvează HTML…"
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' sfdHtml
        '
        sfdHtml.DefaultExt = "html"
        sfdHtml.Filter = "Raport HTML (*.html)|*.html"
        sfdHtml.Title = "Salvați raportul HTML"
        '
        ' AvcReportForm
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnInchide
        ClientSize = New Size(1200, 780)
        Controls.Add(tlpRoot)
        MinimizeBox = False
        MinimumSize = New Size(900, 600)
        Name = "AvcReportForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "AVC.Migrator — raport HTML"
        tlpRoot.ResumeLayout(False)
        tlpButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
