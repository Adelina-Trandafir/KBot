<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VrcReportForm
    Inherits KBot.Theming.KBotThemedForm

    Private components As System.ComponentModel.IContainer
    Friend WithEvents tlpRoot As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents webRaport As System.Windows.Forms.WebBrowser
    Friend WithEvents tlpButoane As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents btnSalveazaHtml As System.Windows.Forms.Button
    Friend WithEvents btnInchide As System.Windows.Forms.Button
    Friend WithEvents sfdHtml As System.Windows.Forms.SaveFileDialog

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        tlpRoot = New Global.KBot.Controls.KBotTableLayoutPanel()
        webRaport = New System.Windows.Forms.WebBrowser()
        tlpButoane = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblInfo = New System.Windows.Forms.Label()
        btnSalveazaHtml = New System.Windows.Forms.Button()
        btnInchide = New System.Windows.Forms.Button()
        sfdHtml = New System.Windows.Forms.SaveFileDialog()
        tlpRoot.SuspendLayout()
        tlpButoane.SuspendLayout()
        SuspendLayout()
        '
        ' tlpRoot
        '
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpRoot.RowCount = 2
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 44.0F))
        tlpRoot.Controls.Add(webRaport, 0, 0)
        tlpRoot.Controls.Add(tlpButoane, 0, 1)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Name = "tlpRoot"
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
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110.0F))
        tlpButoane.RowCount = 1
        tlpButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpButoane.Controls.Add(lblInfo, 0, 0)
        tlpButoane.Controls.Add(btnSalveazaHtml, 1, 0)
        tlpButoane.Controls.Add(btnInchide, 2, 0)
        tlpButoane.Dock = DockStyle.Fill
        tlpButoane.Name = "tlpButoane"
        tlpButoane.TabIndex = 1
        '
        ' lblInfo
        '
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Name = "lblInfo"
        lblInfo.Text = "Raportul afișează rezultatele deja citite."
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        lblInfo.TabIndex = 0
        '
        ' btnSalveazaHtml
        '
        btnSalveazaHtml.Dock = DockStyle.Fill
        btnSalveazaHtml.FlatStyle = FlatStyle.Flat
        btnSalveazaHtml.Name = "btnSalveazaHtml"
        btnSalveazaHtml.Text = "Salvează HTML…"
        btnSalveazaHtml.UseVisualStyleBackColor = True
        btnSalveazaHtml.TabIndex = 1
        '
        ' btnInchide
        '
        btnInchide.DialogResult = DialogResult.Cancel
        btnInchide.Dock = DockStyle.Fill
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Name = "btnInchide"
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        btnInchide.TabIndex = 2
        '
        ' sfdHtml
        '
        sfdHtml.DefaultExt = "html"
        sfdHtml.Filter = "Raport HTML (*.html)|*.html"
        sfdHtml.Title = "Salvați raportul HTML"
        '
        ' VrcReportForm
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnInchide
        ClientSize = New Size(1200, 780)
        Controls.Add(tlpRoot)
        MinimumSize = New Size(900, 600)
        MinimizeBox = False
        Name = "VrcReportForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "VRC.Migrator — raport HTML"
        tlpButoane.ResumeLayout(False)
        tlpRoot.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
End Class
