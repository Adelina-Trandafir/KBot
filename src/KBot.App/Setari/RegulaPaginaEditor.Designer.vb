Imports KBot.Controls

' The four fields of one page rule - «Ce face», «Selector», «Pagina», «Stil» - as one control, used
' twice (operator, 21.09.2026): the right panel of the «Pagina FOREXE» settings page, and
' the right half of the «Regulă nouă» window. All controls are declared HERE
' (docs/kbot-forms-ui-convention.md). Coordinates are in the 144 dpi it was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RegulaPaginaEditor
    Inherits Global.KBot.Theming.KBotThemedUserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        tips = New KBotToolTip(components)
        tlyCampuri = New KBotTableLayoutPanel()
        lblNota = New Label()
        txtNota = New KBotTextField()
        lblSelector = New Label()
        txtSelector = New KBotTextField()
        lblPagina = New Label()
        txtPagina = New KBotTextField()
        lblStil = New Label()
        txtStil = New KBotTextBox()
        lblIndiciu = New Label()
        tlyCampuri.SuspendLayout()
        SuspendLayout()
        '
        ' tlyCampuri
        '
        tlyCampuri.ColumnCount = 1
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCampuri.Controls.Add(lblNota, 0, 0)
        tlyCampuri.Controls.Add(txtNota, 0, 1)
        tlyCampuri.Controls.Add(lblSelector, 0, 2)
        tlyCampuri.Controls.Add(txtSelector, 0, 3)
        tlyCampuri.Controls.Add(lblPagina, 0, 4)
        tlyCampuri.Controls.Add(txtPagina, 0, 5)
        tlyCampuri.Controls.Add(lblStil, 0, 6)
        tlyCampuri.Controls.Add(txtStil, 0, 7)
        tlyCampuri.Controls.Add(lblIndiciu, 0, 8)
        tlyCampuri.Dock = DockStyle.Fill
        tlyCampuri.Location = New Point(0, 0)
        tlyCampuri.Margin = New Padding(0)
        tlyCampuri.Name = "tlyCampuri"
        tlyCampuri.RowCount = 9
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.Size = New Size(480, 560)
        tlyCampuri.TabIndex = 0
        '
        ' lblNota
        '
        lblNota.AutoSize = True
        lblNota.Location = New Point(4, 0)
        lblNota.Margin = New Padding(4, 0, 4, 2)
        lblNota.Name = "lblNota"
        lblNota.Size = New Size(72, 26)
        lblNota.TabIndex = 0
        lblNota.Text = "Ce face"
        '
        ' txtNota
        '
        txtNota.BackColor = Color.Transparent
        txtNota.Dock = DockStyle.Fill
        txtNota.Location = New Point(4, 30)
        txtNota.Margin = New Padding(4, 0, 4, 10)
        txtNota.Name = "txtNota"
        txtNota.PlaceholderText = "o vorbă despre regulă (numai pentru dumneavoastră)"
        txtNota.Size = New Size(472, 40)
        txtNota.TabIndex = 1
        txtNota.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtNota, "Ce face")
        tips.SetToolTipText(txtNota, "O descriere liberă. Nu ajunge în pagină; se vede doar în listă.")
        '
        ' lblSelector
        '
        lblSelector.AutoSize = True
        lblSelector.Location = New Point(4, 86)
        lblSelector.Margin = New Padding(4, 0, 4, 2)
        lblSelector.Name = "lblSelector"
        lblSelector.Size = New Size(76, 26)
        lblSelector.TabIndex = 2
        lblSelector.Text = "Selector"
        '
        ' txtSelector
        '
        txtSelector.BackColor = Color.Transparent
        txtSelector.Dock = DockStyle.Fill
        txtSelector.Location = New Point(4, 116)
        txtSelector.Margin = New Padding(4, 0, 4, 10)
        txtSelector.Name = "txtSelector"
        txtSelector.PlaceholderText = "ex.: #main.container  sau  body:has(.well) [class*='col-lg-2']"
        txtSelector.Size = New Size(472, 40)
        txtSelector.TabIndex = 3
        txtSelector.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtSelector, "Selector CSS")
        tips.SetToolTipText(txtSelector, "Pe ce element(e) se aplică stilul. Fără id-urile Wicket (idXX): se schimbă la fiecare afișare a paginii.")
        '
        ' lblPagina
        '
        lblPagina.AutoSize = True
        lblPagina.Location = New Point(4, 172)
        lblPagina.Margin = New Padding(4, 0, 4, 2)
        lblPagina.Name = "lblPagina"
        lblPagina.Size = New Size(248, 26)
        lblPagina.TabIndex = 4
        lblPagina.Text = "Pagina (gol = pe orice pagină)"
        '
        ' txtPagina
        '
        txtPagina.BackColor = Color.Transparent
        txtPagina.Dock = DockStyle.Fill
        txtPagina.Location = New Point(4, 202)
        txtPagina.Margin = New Padding(4, 0, 4, 10)
        txtPagina.Name = "txtPagina"
        txtPagina.PlaceholderText = "ex.: https://forexe.mfinante.gov.ro/CABWeb/contract"
        txtPagina.Size = New Size(472, 40)
        txtPagina.TabIndex = 5
        txtPagina.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtPagina, "Pagina")
        tips.SetToolTipText(txtPagina, "Regula ține doar pe această pagină; gol = pe toate. Adresa fără «?…»: întreagă (https://…/CABWeb/contract), doar calea (/CABWeb/contract) sau doar ultimul cuvânt al ei (contract).")
        '
        ' lblStil
        '
        lblStil.AutoSize = True
        lblStil.Location = New Point(4, 258)
        lblStil.Margin = New Padding(4, 0, 4, 2)
        lblStil.Name = "lblStil"
        lblStil.Size = New Size(232, 26)
        lblStil.TabIndex = 6
        lblStil.Text = "Stil (proprietate: valoare; ...)"
        '
        ' txtStil
        '
        txtStil.AcceptsReturn = True
        txtStil.Dock = DockStyle.Fill
        txtStil.Font = New Font("Consolas", 10.5F)
        txtStil.Location = New Point(4, 288)
        txtStil.Margin = New Padding(4, 0, 4, 6)
        txtStil.Multiline = True
        txtStil.Name = "txtStil"
        txtStil.PlaceholderText = "max-width: 90%;" & vbLf & "visibility: hidden;"
        txtStil.ScrollBars = ScrollBars.Vertical
        txtStil.Size = New Size(472, 214)
        txtStil.TabIndex = 7
        txtStil.WordWrap = True
        tips.SetToolTipHeader(txtStil, "Stil")
        tips.SetToolTipText(txtStil, "Declarații CSS, una pe rând sau despărțite de «;». Fiecare primește !important în pagină, ca să bată stilurile FOREXE.")
        '
        ' lblIndiciu
        '
        lblIndiciu.AutoSize = True
        lblIndiciu.Dock = DockStyle.Fill
        lblIndiciu.Location = New Point(4, 510)
        lblIndiciu.Margin = New Padding(4, 0, 4, 0)
        lblIndiciu.Name = "lblIndiciu"
        lblIndiciu.Size = New Size(472, 50)
        lblIndiciu.TabIndex = 8
        lblIndiciu.Text = "Modificările se văd imediat în listă; în pagină ajung la «Salvează și aplică»."
        '
        ' RegulaPaginaEditor
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyCampuri)
        Name = "RegulaPaginaEditor"
        Size = New Size(480, 560)
        tlyCampuri.ResumeLayout(False)
        tlyCampuri.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyCampuri As KBotTableLayoutPanel
    Friend WithEvents lblNota As Label
    Friend WithEvents txtNota As KBotTextField
    Friend WithEvents lblSelector As Label
    Friend WithEvents txtSelector As KBotTextField
    Friend WithEvents lblPagina As Label
    Friend WithEvents txtPagina As KBotTextField
    Friend WithEvents lblStil As Label
    Friend WithEvents txtStil As KBotTextBox
    Friend WithEvents lblIndiciu As Label
End Class
