<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RobotQueueForm
    Inherits Global.KBot.Theming.KBotThemedForm

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
        tips = New KBot.Controls.KBotToolTip(components)
        btnPauza = New Button()
        btnScoate = New Button()
        btnGoleste = New Button()
        btnOpreste = New Button()
        pnlCard = New Panel()
        lstCoada = New ListBox()
        pnlFoot = New Panel()
        lblCurent = New Label()
        capBar = New Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlFoot.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnPauza
        ' 
        btnPauza.Dock = DockStyle.Left
        btnPauza.FlatStyle = FlatStyle.Flat
        btnPauza.Image = My.Resources.Resources.pause
        btnPauza.Location = New Point(40, 0)
        btnPauza.Margin = New Padding(4, 4, 4, 4)
        btnPauza.Name = "btnPauza"
        btnPauza.Size = New Size(40, 40)
        btnPauza.TabIndex = 0
        tips.SetToolTipHeader(btnPauza, "Pauză / Continuă")
        tips.SetToolTipText(btnPauza, "Pauză: sarcina în lucru se termină, următoarele nu mai pornesc." & vbLf & "Continuă: coada pornește din nou, în aceeași ordine.")
        btnPauza.UseVisualStyleBackColor = True
        ' 
        ' btnScoate
        ' 
        btnScoate.Dock = DockStyle.Right
        btnScoate.FlatStyle = FlatStyle.Flat
        btnScoate.Image = My.Resources.Resources.minus_red
        btnScoate.Location = New Point(453, 0)
        btnScoate.Margin = New Padding(4, 4, 4, 4)
        btnScoate.Name = "btnScoate"
        btnScoate.Size = New Size(40, 40)
        btnScoate.TabIndex = 1
        tips.SetToolTipHeader(btnScoate, "Scoate")
        tips.SetToolTipText(btnScoate, "Scoate din coadă sarcina selectată în listă." & vbLf & "Sarcina în lucru nu se poate scoate de aici.")
        btnScoate.UseVisualStyleBackColor = True
        ' 
        ' btnGoleste
        ' 
        btnGoleste.Dock = DockStyle.Right
        btnGoleste.FlatStyle = FlatStyle.Flat
        btnGoleste.Image = My.Resources.Resources.stop_round
        btnGoleste.Location = New Point(493, 0)
        btnGoleste.Margin = New Padding(4, 4, 4, 4)
        btnGoleste.Name = "btnGoleste"
        btnGoleste.Size = New Size(40, 40)
        btnGoleste.TabIndex = 2
        tips.SetToolTipHeader(btnGoleste, "Golește coada")
        tips.SetToolTipText(btnGoleste, "Scoate toate sarcinile care așteaptă." & vbLf & "Sarcina în lucru se termină normal.")
        btnGoleste.UseVisualStyleBackColor = True
        ' 
        ' btnOpreste
        ' 
        btnOpreste.Dock = DockStyle.Left
        btnOpreste.FlatStyle = FlatStyle.Flat
        btnOpreste.Image = My.Resources.Resources._stop
        btnOpreste.Location = New Point(0, 0)
        btnOpreste.Margin = New Padding(4, 4, 4, 4)
        btnOpreste.Name = "btnOpreste"
        btnOpreste.Size = New Size(40, 40)
        btnOpreste.TabIndex = 3
        tips.SetToolTipHeader(btnOpreste, "Oprește curenta")
        tips.SetToolTipText(btnOpreste, "Oprește robotul din sarcina în lucru (ca «Anulează» din consolă)." & vbLf & "Se poate doar cât robotul lucrează în FOREXE; ce s-a salvat deja rămâne.")
        btnOpreste.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(lstCoada)
        pnlCard.Controls.Add(pnlFoot)
        pnlCard.Controls.Add(lblCurent)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(4, 4, 4, 4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(533, 339)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' lstCoada
        ' 
        lstCoada.BorderStyle = BorderStyle.None
        lstCoada.Dock = DockStyle.Fill
        lstCoada.Font = New Font("Segoe UI", 9.75F)
        lstCoada.IntegralHeight = False
        lstCoada.ItemHeight = 28
        lstCoada.Location = New Point(0, 100)
        lstCoada.Margin = New Padding(4, 4, 4, 4)
        lstCoada.Name = "lstCoada"
        lstCoada.Size = New Size(533, 199)
        lstCoada.TabIndex = 1
        ' 
        ' pnlFoot
        ' 
        pnlFoot.Controls.Add(btnPauza)
        pnlFoot.Controls.Add(btnScoate)
        pnlFoot.Controls.Add(btnGoleste)
        pnlFoot.Controls.Add(btnOpreste)
        pnlFoot.Dock = DockStyle.Bottom
        pnlFoot.Location = New Point(0, 299)
        pnlFoot.Margin = New Padding(4, 4, 4, 4)
        pnlFoot.Name = "pnlFoot"
        pnlFoot.Size = New Size(533, 40)
        pnlFoot.TabIndex = 2
        pnlFoot.Tag = "Card"
        ' 
        ' lblCurent
        ' 
        lblCurent.Dock = DockStyle.Top
        lblCurent.Font = New Font("Segoe UI Semibold", 9.75F)
        lblCurent.Location = New Point(0, 40)
        lblCurent.Margin = New Padding(4, 0, 4, 0)
        lblCurent.Name = "lblCurent"
        lblCurent.Padding = New Padding(18, 4, 18, 4)
        lblCurent.Size = New Size(533, 60)
        lblCurent.TabIndex = 0
        lblCurent.Text = "Nicio sarcină în lucru."
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4, 4, 4, 4)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New Size(533, 40)
        capBar.TabIndex = 3
        capBar.TabStop = False
        capBar.Text = "Coada robotului"
        ' 
        ' RobotQueueForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(537, 343)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 4, 4, 4)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(400, 300)
        Name = "RobotQueueForm"
        Padding = New Padding(2, 2, 2, 2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Coada robotului"
        pnlCard.ResumeLayout(False)
        pnlFoot.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblCurent As Label
    Friend WithEvents lstCoada As ListBox
    Friend WithEvents pnlFoot As Panel
    Friend WithEvents btnPauza As Button
    Friend WithEvents btnScoate As Button
    Friend WithEvents btnGoleste As Button
    Friend WithEvents btnOpreste As Button
End Class
