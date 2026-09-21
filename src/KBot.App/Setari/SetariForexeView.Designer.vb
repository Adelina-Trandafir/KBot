Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariForexeView
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
        components = New ComponentModel.Container()
        tips = New KBotToolTip(components)
        tlyBody = New KBotTableLayoutPanel()
        lblTitluStare = New Label()
        tlyStare = New KBotTableLayoutPanel()
        lblConexiuneCaption = New Label()
        lblConexiune = New Label()
        lblCertificatCaption = New Label()
        lblCertificat = New Label()
        lblTitluCertificat = New Label()
        tlyCertificat = New KBotTableLayoutPanel()
        lblCertMemoratCaption = New Label()
        lblCertMemorat = New Label()
        btnUitaCertificat = New Button()
        lblTitluBrowser = New Label()
        tlyBrowser = New KBotTableLayoutPanel()
        chkHideChrome = New CheckBox()
        chkDevTools = New CheckBox()
        lblTitluFoldere = New Label()
        tlyFoldere = New KBotTableLayoutPanel()
        lblWorkflowsCaption = New Label()
        lblWorkflows = New Label()
        lblRezultateCaption = New Label()
        lblRezultate = New Label()
        lblExtraseCaption = New Label()
        lblExtrase = New Label()
        lblFoldereHint = New Label()
        tlyBody.SuspendLayout()
        tlyStare.SuspendLayout()
        tlyCertificat.SuspendLayout()
        tlyBrowser.SuspendLayout()
        tlyFoldere.SuspendLayout()
        SuspendLayout()
        '
        ' tlyBody
        '
        tlyBody.AutoScroll = True
        tlyBody.ColumnCount = 1
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBody.Controls.Add(lblTitluStare, 0, 0)
        tlyBody.Controls.Add(tlyStare, 0, 1)
        tlyBody.Controls.Add(lblTitluCertificat, 0, 2)
        tlyBody.Controls.Add(tlyCertificat, 0, 3)
        tlyBody.Controls.Add(lblTitluBrowser, 0, 4)
        tlyBody.Controls.Add(tlyBrowser, 0, 5)
        tlyBody.Controls.Add(lblTitluFoldere, 0, 6)
        tlyBody.Controls.Add(tlyFoldere, 0, 7)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(0, 0)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.Padding = New Padding(24, 18, 24, 18)
        tlyBody.RowCount = 9
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.Size = New Size(960, 840)
        tlyBody.TabIndex = 0
        '
        ' lblTitluStare
        '
        lblTitluStare.AutoSize = True
        lblTitluStare.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluStare.Location = New Point(28, 18)
        lblTitluStare.Margin = New Padding(4, 0, 4, 8)
        lblTitluStare.Name = "lblTitluStare"
        lblTitluStare.Size = New Size(160, 32)
        lblTitluStare.TabIndex = 0
        lblTitluStare.Text = "Starea robotului"
        '
        ' tlyStare
        '
        tlyStare.AutoFitToTheme = False
        tlyStare.AutoSize = True
        tlyStare.ColumnCount = 2
        tlyStare.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyStare.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyStare.Controls.Add(lblConexiuneCaption, 0, 0)
        tlyStare.Controls.Add(lblConexiune, 1, 0)
        tlyStare.Controls.Add(lblCertificatCaption, 0, 1)
        tlyStare.Controls.Add(lblCertificat, 1, 1)
        tlyStare.Dock = DockStyle.Top
        tlyStare.Location = New Point(28, 58)
        tlyStare.Margin = New Padding(4, 0, 4, 24)
        tlyStare.Name = "tlyStare"
        tlyStare.RowCount = 2
        tlyStare.RowStyles.Add(New RowStyle())
        tlyStare.RowStyles.Add(New RowStyle())
        tlyStare.Size = New Size(904, 64)
        tlyStare.TabIndex = 1
        '
        ' lblConexiuneCaption
        '
        lblConexiuneCaption.AutoSize = True
        lblConexiuneCaption.Dock = DockStyle.Fill
        lblConexiuneCaption.Location = New Point(4, 0)
        lblConexiuneCaption.Margin = New Padding(4, 0, 4, 6)
        lblConexiuneCaption.Name = "lblConexiuneCaption"
        lblConexiuneCaption.Size = New Size(252, 26)
        lblConexiuneCaption.TabIndex = 0
        lblConexiuneCaption.Text = "Sesiune FOREXE"
        '
        ' lblConexiune
        '
        lblConexiune.AutoSize = True
        lblConexiune.Dock = DockStyle.Fill
        lblConexiune.Font = New Font("Segoe UI Semibold", 9F)
        lblConexiune.Location = New Point(264, 0)
        lblConexiune.Margin = New Padding(4, 0, 4, 6)
        lblConexiune.Name = "lblConexiune"
        lblConexiune.Size = New Size(636, 26)
        lblConexiune.TabIndex = 1
        lblConexiune.Text = "—"
        '
        ' lblCertificatCaption
        '
        lblCertificatCaption.AutoSize = True
        lblCertificatCaption.Dock = DockStyle.Fill
        lblCertificatCaption.Location = New Point(4, 32)
        lblCertificatCaption.Margin = New Padding(4, 0, 4, 6)
        lblCertificatCaption.Name = "lblCertificatCaption"
        lblCertificatCaption.Size = New Size(252, 26)
        lblCertificatCaption.TabIndex = 2
        lblCertificatCaption.Text = "Certificatul sesiunii"
        '
        ' lblCertificat
        '
        lblCertificat.AutoSize = True
        lblCertificat.Dock = DockStyle.Fill
        lblCertificat.Font = New Font("Segoe UI Semibold", 9F)
        lblCertificat.Location = New Point(264, 32)
        lblCertificat.Margin = New Padding(4, 0, 4, 6)
        lblCertificat.Name = "lblCertificat"
        lblCertificat.Size = New Size(636, 26)
        lblCertificat.TabIndex = 3
        lblCertificat.Text = "—"
        '
        ' lblTitluCertificat
        '
        lblTitluCertificat.AutoSize = True
        lblTitluCertificat.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluCertificat.Location = New Point(28, 146)
        lblTitluCertificat.Margin = New Padding(4, 0, 4, 8)
        lblTitluCertificat.Name = "lblTitluCertificat"
        lblTitluCertificat.Size = New Size(220, 32)
        lblTitluCertificat.TabIndex = 2
        lblTitluCertificat.Text = "Certificatul memorat"
        '
        ' tlyCertificat
        '
        tlyCertificat.AutoFitToTheme = False
        tlyCertificat.AutoSize = True
        tlyCertificat.ColumnCount = 3
        tlyCertificat.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyCertificat.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCertificat.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 240F))
        tlyCertificat.Controls.Add(lblCertMemoratCaption, 0, 0)
        tlyCertificat.Controls.Add(lblCertMemorat, 1, 0)
        tlyCertificat.Controls.Add(btnUitaCertificat, 2, 0)
        tlyCertificat.Dock = DockStyle.Top
        tlyCertificat.Location = New Point(28, 186)
        tlyCertificat.Margin = New Padding(4, 0, 4, 24)
        tlyCertificat.Name = "tlyCertificat"
        tlyCertificat.RowCount = 1
        tlyCertificat.RowStyles.Add(New RowStyle())
        tlyCertificat.Size = New Size(904, 50)
        tlyCertificat.TabIndex = 3
        '
        ' lblCertMemoratCaption
        '
        lblCertMemoratCaption.AutoSize = True
        lblCertMemoratCaption.Dock = DockStyle.Fill
        lblCertMemoratCaption.Location = New Point(4, 0)
        lblCertMemoratCaption.Margin = New Padding(4, 0, 4, 0)
        lblCertMemoratCaption.Name = "lblCertMemoratCaption"
        lblCertMemoratCaption.Size = New Size(252, 50)
        lblCertMemoratCaption.TabIndex = 0
        lblCertMemoratCaption.Text = "Propus la următoarea conectare"
        lblCertMemoratCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblCertMemorat
        '
        lblCertMemorat.AutoSize = True
        lblCertMemorat.Dock = DockStyle.Fill
        lblCertMemorat.Font = New Font("Segoe UI Semibold", 9F)
        lblCertMemorat.Location = New Point(264, 0)
        lblCertMemorat.Margin = New Padding(4, 0, 4, 0)
        lblCertMemorat.Name = "lblCertMemorat"
        lblCertMemorat.Size = New Size(388, 50)
        lblCertMemorat.TabIndex = 1
        lblCertMemorat.Text = "—"
        lblCertMemorat.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnUitaCertificat
        '
        btnUitaCertificat.Dock = DockStyle.Fill
        btnUitaCertificat.Enabled = False
        btnUitaCertificat.FlatStyle = FlatStyle.Flat
        btnUitaCertificat.Font = New Font("Segoe UI Semibold", 9F)
        btnUitaCertificat.Location = New Point(664, 0)
        btnUitaCertificat.Margin = New Padding(4, 0, 4, 0)
        btnUitaCertificat.Name = "btnUitaCertificat"
        btnUitaCertificat.Size = New Size(236, 50)
        btnUitaCertificat.TabIndex = 2
        btnUitaCertificat.Text = "Uită certificatul"
        tips.SetToolTipHeader(btnUitaCertificat, "Uită certificatul")
        tips.SetToolTipText(btnUitaCertificat, "Șterge certificatul memorat (doar partea publică, din AppData)." & vbLf & "La următoarea conectare se alege din nou.")
        btnUitaCertificat.UseVisualStyleBackColor = True
        '
        ' lblTitluBrowser
        '
        lblTitluBrowser.AutoSize = True
        lblTitluBrowser.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluBrowser.Location = New Point(28, 260)
        lblTitluBrowser.Margin = New Padding(4, 0, 4, 8)
        lblTitluBrowser.Name = "lblTitluBrowser"
        lblTitluBrowser.Size = New Size(98, 32)
        lblTitluBrowser.TabIndex = 4
        lblTitluBrowser.Text = "Browserul"
        '
        ' tlyBrowser
        '
        tlyBrowser.AutoFitToTheme = False
        tlyBrowser.AutoSize = True
        tlyBrowser.ColumnCount = 1
        tlyBrowser.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBrowser.Controls.Add(chkHideChrome, 0, 0)
        tlyBrowser.Controls.Add(chkDevTools, 0, 1)
        tlyBrowser.Dock = DockStyle.Top
        tlyBrowser.Location = New Point(28, 300)
        tlyBrowser.Margin = New Padding(4, 0, 4, 24)
        tlyBrowser.Name = "tlyBrowser"
        tlyBrowser.RowCount = 2
        tlyBrowser.RowStyles.Add(New RowStyle())
        tlyBrowser.RowStyles.Add(New RowStyle())
        tlyBrowser.Size = New Size(904, 78)
        tlyBrowser.TabIndex = 5
        '
        ' chkHideChrome
        '
        chkHideChrome.AutoSize = True
        chkHideChrome.Checked = True
        chkHideChrome.CheckState = CheckState.Checked
        chkHideChrome.Location = New Point(4, 0)
        chkHideChrome.Margin = New Padding(4, 0, 4, 10)
        chkHideChrome.Name = "chkHideChrome"
        chkHideChrome.Size = New Size(420, 29)
        chkHideChrome.TabIndex = 0
        chkHideChrome.Text = "În vizualizator, bara browserului (file, adresă) rămâne în afara panoului"
        tips.SetToolTipHeader(chkHideChrome, "Bara browserului")
        tips.SetToolTipText(chkHideChrome, "Bifat, operatorul vede doar pagina FOREXE." & vbLf & "Debifat, apare și bara Chromium. Se aplică de la următoarea lucrare.")
        chkHideChrome.UseVisualStyleBackColor = True
        '
        ' chkDevTools
        '
        chkDevTools.AutoSize = True
        chkDevTools.Location = New Point(4, 39)
        chkDevTools.Margin = New Padding(4, 0, 4, 10)
        chkDevTools.Name = "chkDevTools"
        chkDevTools.Size = New Size(420, 29)
        chkDevTools.TabIndex = 1
        chkDevTools.Text = "Permite instrumentele pentru dezvoltatori în pagina FOREXE (F12, Ctrl+Shift+I, clic dreapta)"
        tips.SetToolTipHeader(chkDevTools, "Instrumente pentru dezvoltatori")
        tips.SetToolTipText(chkDevTools, "Debifat, pagina înghite F12, Ctrl+Shift+I / J / C, Ctrl+U și meniul de clic dreapta." & vbLf & "Bifat, toate rămân la îndemână. Se aplică imediat în pagina deschisă.")
        chkDevTools.UseVisualStyleBackColor = True
        '
        ' lblTitluFoldere
        '
        lblTitluFoldere.AutoSize = True
        lblTitluFoldere.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluFoldere.Location = New Point(28, 363)
        lblTitluFoldere.Margin = New Padding(4, 0, 4, 8)
        lblTitluFoldere.Name = "lblTitluFoldere"
        lblTitluFoldere.Size = New Size(93, 32)
        lblTitluFoldere.TabIndex = 6
        lblTitluFoldere.Text = "Foldere"
        '
        ' tlyFoldere
        '
        tlyFoldere.AutoFitToTheme = False
        tlyFoldere.AutoSize = True
        tlyFoldere.ColumnCount = 2
        tlyFoldere.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyFoldere.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFoldere.Controls.Add(lblWorkflowsCaption, 0, 0)
        tlyFoldere.Controls.Add(lblWorkflows, 1, 0)
        tlyFoldere.Controls.Add(lblRezultateCaption, 0, 1)
        tlyFoldere.Controls.Add(lblRezultate, 1, 1)
        tlyFoldere.Controls.Add(lblExtraseCaption, 0, 2)
        tlyFoldere.Controls.Add(lblExtrase, 1, 2)
        tlyFoldere.Controls.Add(lblFoldereHint, 0, 3)
        tlyFoldere.Dock = DockStyle.Top
        tlyFoldere.Location = New Point(28, 403)
        tlyFoldere.Margin = New Padding(4, 0, 4, 0)
        tlyFoldere.Name = "tlyFoldere"
        tlyFoldere.RowCount = 4
        tlyFoldere.RowStyles.Add(New RowStyle())
        tlyFoldere.RowStyles.Add(New RowStyle())
        tlyFoldere.RowStyles.Add(New RowStyle())
        tlyFoldere.RowStyles.Add(New RowStyle())
        tlyFoldere.Size = New Size(904, 130)
        tlyFoldere.TabIndex = 7
        '
        ' lblWorkflowsCaption
        '
        lblWorkflowsCaption.AutoSize = True
        lblWorkflowsCaption.Dock = DockStyle.Fill
        lblWorkflowsCaption.Location = New Point(4, 0)
        lblWorkflowsCaption.Margin = New Padding(4, 0, 4, 6)
        lblWorkflowsCaption.Name = "lblWorkflowsCaption"
        lblWorkflowsCaption.Size = New Size(252, 26)
        lblWorkflowsCaption.TabIndex = 0
        lblWorkflowsCaption.Text = "Definițiile de workflow (.wfl)"
        '
        ' lblWorkflows
        '
        lblWorkflows.AutoEllipsis = True
        lblWorkflows.AutoSize = False
        lblWorkflows.Dock = DockStyle.Fill
        lblWorkflows.Font = New Font("Segoe UI Semibold", 9F)
        lblWorkflows.Location = New Point(264, 0)
        lblWorkflows.Margin = New Padding(4, 0, 4, 6)
        lblWorkflows.Name = "lblWorkflows"
        lblWorkflows.Size = New Size(636, 26)
        lblWorkflows.TabIndex = 1
        lblWorkflows.Text = "—"
        '
        ' lblRezultateCaption
        '
        lblRezultateCaption.AutoSize = True
        lblRezultateCaption.Dock = DockStyle.Fill
        lblRezultateCaption.Location = New Point(4, 32)
        lblRezultateCaption.Margin = New Padding(4, 0, 4, 6)
        lblRezultateCaption.Name = "lblRezultateCaption"
        lblRezultateCaption.Size = New Size(252, 26)
        lblRezultateCaption.TabIndex = 2
        lblRezultateCaption.Text = "Rezultatele brute (JSON)"
        '
        ' lblRezultate
        '
        lblRezultate.AutoEllipsis = True
        lblRezultate.AutoSize = False
        lblRezultate.Dock = DockStyle.Fill
        lblRezultate.Font = New Font("Segoe UI Semibold", 9F)
        lblRezultate.Location = New Point(264, 32)
        lblRezultate.Margin = New Padding(4, 0, 4, 6)
        lblRezultate.Name = "lblRezultate"
        lblRezultate.Size = New Size(636, 26)
        lblRezultate.TabIndex = 3
        lblRezultate.Text = "—"
        '
        ' lblExtraseCaption
        '
        lblExtraseCaption.AutoSize = True
        lblExtraseCaption.Dock = DockStyle.Fill
        lblExtraseCaption.Location = New Point(4, 64)
        lblExtraseCaption.Margin = New Padding(4, 0, 4, 6)
        lblExtraseCaption.Name = "lblExtraseCaption"
        lblExtraseCaption.Size = New Size(252, 26)
        lblExtraseCaption.TabIndex = 4
        lblExtraseCaption.Text = "Extrasele de cont (PDF)"
        '
        ' lblExtrase
        '
        lblExtrase.AutoEllipsis = True
        lblExtrase.AutoSize = False
        lblExtrase.Dock = DockStyle.Fill
        lblExtrase.Font = New Font("Segoe UI Semibold", 9F)
        lblExtrase.Location = New Point(264, 64)
        lblExtrase.Margin = New Padding(4, 0, 4, 6)
        lblExtrase.Name = "lblExtrase"
        lblExtrase.Size = New Size(636, 26)
        lblExtrase.TabIndex = 5
        lblExtrase.Text = "—"
        '
        ' lblFoldereHint
        '
        lblFoldereHint.AutoSize = True
        tlyFoldere.SetColumnSpan(lblFoldereHint, 2)
        lblFoldereHint.Dock = DockStyle.Fill
        lblFoldereHint.Location = New Point(4, 96)
        lblFoldereHint.Margin = New Padding(4, 6, 4, 0)
        lblFoldereHint.Name = "lblFoldereHint"
        lblFoldereHint.Size = New Size(896, 26)
        lblFoldereHint.TabIndex = 6
        lblFoldereHint.Text = "Căile se schimbă din pagina «Aplicație», secțiunea «Foldere»."
        '
        ' SetariForexeView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyBody)
        Name = "SetariForexeView"
        Size = New Size(960, 840)
        tlyBody.ResumeLayout(False)
        tlyBody.PerformLayout()
        tlyStare.ResumeLayout(False)
        tlyStare.PerformLayout()
        tlyCertificat.ResumeLayout(False)
        tlyCertificat.PerformLayout()
        tlyBrowser.ResumeLayout(False)
        tlyBrowser.PerformLayout()
        tlyFoldere.ResumeLayout(False)
        tlyFoldere.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents lblTitluStare As Label
    Friend WithEvents tlyStare As KBotTableLayoutPanel
    Friend WithEvents lblConexiuneCaption As Label
    Friend WithEvents lblConexiune As Label
    Friend WithEvents lblCertificatCaption As Label
    Friend WithEvents lblCertificat As Label
    Friend WithEvents lblTitluCertificat As Label
    Friend WithEvents tlyCertificat As KBotTableLayoutPanel
    Friend WithEvents lblCertMemoratCaption As Label
    Friend WithEvents lblCertMemorat As Label
    Friend WithEvents btnUitaCertificat As Button
    Friend WithEvents lblTitluBrowser As Label
    Friend WithEvents tlyBrowser As KBotTableLayoutPanel
    Friend WithEvents chkHideChrome As CheckBox
    Friend WithEvents chkDevTools As CheckBox
    Friend WithEvents lblTitluFoldere As Label
    Friend WithEvents tlyFoldere As KBotTableLayoutPanel
    Friend WithEvents lblWorkflowsCaption As Label
    Friend WithEvents lblWorkflows As Label
    Friend WithEvents lblRezultateCaption As Label
    Friend WithEvents lblRezultate As Label
    Friend WithEvents lblExtraseCaption As Label
    Friend WithEvents lblExtrase As Label
    Friend WithEvents lblFoldereHint As Label
End Class
