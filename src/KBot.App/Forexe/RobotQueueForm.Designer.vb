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
        tips = New Global.KBot.Controls.KBotToolTip(components)
        pnlCard = New Panel()
        lstCoada = New ListBox()
        pnlFoot = New Panel()
        btnPauza = New Button()
        btnScoate = New Button()
        btnGoleste = New Button()
        btnOpreste = New Button()
        lblCurent = New Label()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlFoot.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard -- the root card; children in REVERSE dock order:
        ' lstCoada (Fill) first, then pnlFoot (Bottom), lblCurent (Top), capBar (Top, topmost).
        '
        pnlCard.Controls.Add(lstCoada)
        pnlCard.Controls.Add(pnlFoot)
        pnlCard.Controls.Add(lblCurent)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 1)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(458, 358)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' lstCoada -- the waiting tasks, in the order they will run.
        '
        lstCoada.BorderStyle = BorderStyle.None
        lstCoada.Dock = DockStyle.Fill
        lstCoada.Font = New Font("Segoe UI", 9.75F)
        lstCoada.IntegralHeight = False
        lstCoada.Location = New Point(0, 104)
        lstCoada.Name = "lstCoada"
        lstCoada.Size = New Size(458, 206)
        lstCoada.TabIndex = 1
        '
        ' pnlFoot -- the four commands.
        '
        pnlFoot.Controls.Add(btnPauza)
        pnlFoot.Controls.Add(btnScoate)
        pnlFoot.Controls.Add(btnGoleste)
        pnlFoot.Controls.Add(btnOpreste)
        pnlFoot.Dock = DockStyle.Bottom
        pnlFoot.Location = New Point(0, 310)
        pnlFoot.Name = "pnlFoot"
        pnlFoot.Size = New Size(458, 48)
        pnlFoot.TabIndex = 2
        pnlFoot.Tag = "Card"
        '
        ' btnPauza
        '
        btnPauza.FlatStyle = FlatStyle.Flat
        btnPauza.Location = New Point(8, 8)
        btnPauza.Name = "btnPauza"
        btnPauza.Size = New Size(100, 32)
        btnPauza.TabIndex = 0
        btnPauza.Text = "Pauză"
        btnPauza.UseVisualStyleBackColor = True
        '
        ' btnScoate
        '
        btnScoate.FlatStyle = FlatStyle.Flat
        btnScoate.Location = New Point(114, 8)
        btnScoate.Name = "btnScoate"
        btnScoate.Size = New Size(100, 32)
        btnScoate.TabIndex = 1
        btnScoate.Text = "Scoate"
        btnScoate.UseVisualStyleBackColor = True
        '
        ' btnGoleste
        '
        btnGoleste.FlatStyle = FlatStyle.Flat
        btnGoleste.Location = New Point(220, 8)
        btnGoleste.Name = "btnGoleste"
        btnGoleste.Size = New Size(110, 32)
        btnGoleste.TabIndex = 2
        btnGoleste.Text = "Golește coada"
        btnGoleste.UseVisualStyleBackColor = True
        '
        ' btnOpreste
        '
        btnOpreste.FlatStyle = FlatStyle.Flat
        btnOpreste.Location = New Point(336, 8)
        btnOpreste.Name = "btnOpreste"
        btnOpreste.Size = New Size(114, 32)
        btnOpreste.TabIndex = 3
        btnOpreste.Text = "Oprește curenta"
        btnOpreste.UseVisualStyleBackColor = True
        '
        ' lblCurent -- the running task and what it waits for.
        '
        lblCurent.Dock = DockStyle.Top
        lblCurent.Font = New Font("Segoe UI Semibold", 9.75F)
        lblCurent.Location = New Point(0, 40)
        lblCurent.Name = "lblCurent"
        lblCurent.Padding = New Padding(12, 8, 12, 6)
        lblCurent.Size = New Size(458, 64)
        lblCurent.TabIndex = 0
        lblCurent.Text = "Nicio sarcină în lucru."
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.Location = New Point(0, 0)
        capBar.Name = "capBar"
        capBar.ShowMaximize = False
        capBar.ShowMinimize = False
        capBar.Size = New Size(458, 40)
        capBar.TabIndex = 3
        capBar.TabStop = False
        capBar.Text = "Coada robotului"
        '
        ' RobotQueueForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(460, 360)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(460, 260)
        Name = "RobotQueueForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Coada robotului"
        pnlCard.ResumeLayout(False)
        pnlFoot.ResumeLayout(False)
        '
        ' tips
        '
        tips.SetToolTipHeader(btnPauza, "Pauză / Continuă")
        tips.SetToolTipText(btnPauza, "Pauză: sarcina în lucru se termină, următoarele nu mai pornesc." & vbLf & "Continuă: coada pornește din nou, în aceeași ordine.")
        tips.SetToolTipHeader(btnScoate, "Scoate")
        tips.SetToolTipText(btnScoate, "Scoate din coadă sarcina selectată în listă." & vbLf & "Sarcina în lucru nu se poate scoate de aici.")
        tips.SetToolTipHeader(btnGoleste, "Golește coada")
        tips.SetToolTipText(btnGoleste, "Scoate toate sarcinile care așteaptă." & vbLf & "Sarcina în lucru se termină normal.")
        tips.SetToolTipHeader(btnOpreste, "Oprește curenta")
        tips.SetToolTipText(btnOpreste, "Oprește robotul din sarcina în lucru (ca «Anulează» din consolă)." & vbLf & "Se poate doar cât robotul lucrează în FOREXE; ce s-a salvat deja rămâne.")
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
