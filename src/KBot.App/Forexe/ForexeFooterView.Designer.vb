Imports KBot.Controls
Imports KBot.Theming

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ForexeFooterView
    Inherits Global.KBot.Theming.KBotThemedUserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                Dezleaga()
                If components IsNot Nothing Then components.Dispose()
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
        btnExtinde = New Button()
        btnIstoric = New Button()
        btnConectare = New Button()
        btnBrowser = New Button()
        pbProgress = New KBotProgressBar()
        lblCert = New Label()
        lblStatus = New Label()
        lblSpacer = New Label()
        SuspendLayout()
        ' 
        ' btnExtinde
        ' 
        btnExtinde.Dock = DockStyle.Right
        btnExtinde.FlatStyle = FlatStyle.Flat
        btnExtinde.Image = My.Resources.Resources.Wefunction_Woofunction_Window_app_list_info_32
        btnExtinde.Location = New Point(1174, 0)
        btnExtinde.Margin = New Padding(4, 5, 4, 5)
        btnExtinde.Name = "btnExtinde"
        btnExtinde.Size = New Size(56, 49)
        btnExtinde.TabIndex = 6
        tips.SetToolTipHeader(btnExtinde, "Consolă")
        tips.SetToolTipText(btnExtinde, "Deschide consola FOREXE: progres detaliat, jurnal și descărcări.")
        btnExtinde.UseVisualStyleBackColor = True
        ' 
        ' btnIstoric
        ' 
        btnIstoric.Dock = DockStyle.Right
        btnIstoric.FlatStyle = FlatStyle.Flat
        btnIstoric.Location = New Point(1060, 0)
        btnIstoric.Margin = New Padding(4, 5, 4, 5)
        btnIstoric.Name = "btnIstoric"
        btnIstoric.Size = New Size(57, 49)
        btnIstoric.TabIndex = 5
        btnIstoric.Text = "⟲"
        tips.SetToolTipHeader(btnIstoric, "Istoric")
        tips.SetToolTipText(btnIstoric, "Istoricul acțiunilor duse prin FOREXE în această sesiune," & vbLf & "cu rezultatul și jurnalul fiecăreia.")
        btnIstoric.UseVisualStyleBackColor = True
        btnIstoric.Visible = False
        ' 
        ' btnConectare
        ' 
        btnConectare.BackgroundImageLayout = ImageLayout.None
        btnConectare.Dock = DockStyle.Left
        btnConectare.FlatAppearance.BorderColor = SystemColors.ActiveBorder
        btnConectare.FlatStyle = FlatStyle.Flat
        btnConectare.Font = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnConectare.Image = My.Resources.Resources.FX_24
        btnConectare.ImageAlign = ContentAlignment.MiddleLeft
        btnConectare.Location = New Point(0, 0)
        btnConectare.Margin = New Padding(0)
        btnConectare.Name = "btnConectare"
        btnConectare.Padding = New Padding(17, 0, 0, 0)
        btnConectare.Size = New Size(219, 49)
        btnConectare.TabIndex = 1
        btnConectare.Text = "Conectare"
        tips.SetToolTipHeader(btnConectare, "Conectare FOREXE")
        tips.SetToolTipText(btnConectare, "Pornește sesiunea către portalul FOREXE." & vbLf & "Se cere certificatul o singură dată pe sesiune.")
        btnConectare.UseVisualStyleBackColor = True
        ' 
        ' btnBrowser
        ' 
        btnBrowser.Dock = DockStyle.Right
        btnBrowser.FlatStyle = FlatStyle.Flat
        btnBrowser.Image = My.Resources.Resources.Semlabs_Web_Blog_Earth_search_48_resized
        btnBrowser.Location = New Point(1117, 0)
        btnBrowser.Margin = New Padding(4, 5, 4, 5)
        btnBrowser.Name = "btnBrowser"
        btnBrowser.Size = New Size(57, 49)
        btnBrowser.TabIndex = 8
        btnBrowser.Text = "⟲"
        tips.SetToolTipFooter(btnBrowser, "ATENȚIE! Orice modificare in browser este live pe serverul FOREXECAB!")
        tips.SetToolTipHeader(btnBrowser, "Browser")
        tips.SetToolTipText(btnBrowser, "Afișează browserul")
        btnBrowser.UseVisualStyleBackColor = True
        ' 
        ' pbProgress
        ' 
        pbProgress.Dock = DockStyle.Left
        pbProgress.Location = New Point(234, 0)
        pbProgress.Margin = New Padding(6)
        pbProgress.Name = "pbProgress"
        pbProgress.Size = New Size(213, 49)
        pbProgress.TabIndex = 2
        ' 
        ' lblCert
        ' 
        lblCert.AutoEllipsis = True
        lblCert.Dock = DockStyle.Left
        lblCert.Location = New Point(447, 0)
        lblCert.Margin = New Padding(4, 0, 4, 0)
        lblCert.Name = "lblCert"
        lblCert.Padding = New Padding(10, 0, 0, 0)
        lblCert.Size = New Size(321, 49)
        lblCert.TabIndex = 3
        lblCert.Text = "Certificat: —"
        lblCert.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoEllipsis = True
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Location = New Point(768, 0)
        lblStatus.Margin = New Padding(4, 0, 4, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(406, 49)
        lblStatus.TabIndex = 4
        lblStatus.Text = "În așteptare..."
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblSpacer
        ' 
        lblSpacer.Dock = DockStyle.Left
        lblSpacer.Location = New Point(219, 0)
        lblSpacer.Margin = New Padding(4, 0, 4, 0)
        lblSpacer.Name = "lblSpacer"
        lblSpacer.Size = New Size(15, 49)
        lblSpacer.TabIndex = 7
        ' 
        ' ForexeFooterView
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(btnIstoric)
        Controls.Add(btnBrowser)
        Controls.Add(lblStatus)
        Controls.Add(btnExtinde)
        Controls.Add(lblCert)
        Controls.Add(pbProgress)
        Controls.Add(lblSpacer)
        Controls.Add(btnConectare)
        Margin = New Padding(4, 5, 4, 5)
        Name = "ForexeFooterView"
        Size = New Size(1230, 49)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pbProgress As Global.KBot.Controls.KBotProgressBar
    Friend WithEvents lblCert As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnIstoric As Button
    Friend WithEvents btnExtinde As Button
    Friend WithEvents btnConectare As Button
    Friend WithEvents lblSpacer As Label
    Friend WithEvents btnBrowser As Button
End Class
