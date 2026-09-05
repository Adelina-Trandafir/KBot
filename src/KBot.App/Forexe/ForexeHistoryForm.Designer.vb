<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ForexeHistoryForm
    Inherits Global.KBot.Theming.KBotShellForm

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
        splitMain = New SplitContainer()
        treeJobs = New Global.KBot.Controls.AdvancedTreeControl()
        rtbDetalii = New RichTextBox()
        pnlFoot = New Panel()
        btnReimprospateaza = New Button()
        btnExport = New Button()
        btnInchide = New Button()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        CType(splitMain, ComponentModel.ISupportInitialize).BeginInit()
        splitMain.Panel1.SuspendLayout()
        splitMain.Panel2.SuspendLayout()
        splitMain.SuspendLayout()
        pnlFoot.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard — în panoul-card copiii se adaugă în ordine INVERSĂ de andocare:
        ' Fill primul, apoi Bottom, iar Top ultimul (ultimul adăugat stă cel mai sus).
        '
        pnlCard.Controls.Add(splitMain)
        pnlCard.Controls.Add(pnlFoot)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 3)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(1010, 655)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' splitMain
        '
        splitMain.Dock = DockStyle.Fill
        splitMain.Location = New Point(0, 48)
        splitMain.Name = "splitMain"
        splitMain.Panel1.Controls.Add(treeJobs)
        splitMain.Panel1MinSize = 260
        splitMain.Panel2.Controls.Add(rtbDetalii)
        splitMain.Panel2MinSize = 240
        splitMain.Size = New Size(1010, 543)
        splitMain.SplitterDistance = 400
        splitMain.SplitterWidth = 6
        splitMain.TabIndex = 0
        '
        ' treeJobs — lista lucrărilor, cea mai recentă sus. Captionul folosește separatorul
        ' «~~~»: în stânga ora și numele, în dreapta rezultatul, cu lățime rezervată.
        '
        treeJobs.Dock = DockStyle.Fill
        treeJobs.Font = New Font("Calibri", 9F)
        treeJobs.HeaderCaption = " ISTORIC FOREXE"
        treeJobs.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        treeJobs.HeaderHeight = 30
        treeJobs.HeaderVisible = True
        treeJobs.Indent = 10
        treeJobs.ItemHeight = 20
        treeJobs.Location = New Point(0, 0)
        treeJobs.Name = "treeJobs"
        treeJobs.PaddingExpanderGap = 10
        treeJobs.PaddingTreeStart = 8
        treeJobs.RightTextWidth = 70
        treeJobs.SearchShow = True
        treeJobs.Size = New Size(400, 543)
        treeJobs.TabIndex = 0
        '
        ' rtbDetalii
        '
        rtbDetalii.BorderStyle = BorderStyle.None
        rtbDetalii.Dock = DockStyle.Fill
        rtbDetalii.Font = New Font("Consolas", 9F)
        rtbDetalii.Location = New Point(0, 0)
        rtbDetalii.Name = "rtbDetalii"
        rtbDetalii.ReadOnly = True
        rtbDetalii.ScrollBars = RichTextBoxScrollBars.Both
        rtbDetalii.Size = New Size(604, 543)
        rtbDetalii.TabIndex = 0
        rtbDetalii.Text = ""
        rtbDetalii.WordWrap = False
        '
        ' pnlFoot
        '
        pnlFoot.Controls.Add(btnReimprospateaza)
        pnlFoot.Controls.Add(btnExport)
        pnlFoot.Controls.Add(btnInchide)
        pnlFoot.Dock = DockStyle.Bottom
        pnlFoot.Location = New Point(0, 591)
        pnlFoot.Name = "pnlFoot"
        pnlFoot.Padding = New Padding(14, 10, 14, 10)
        pnlFoot.Size = New Size(1010, 64)
        pnlFoot.TabIndex = 1
        '
        ' btnReimprospateaza
        '
        btnReimprospateaza.Dock = DockStyle.Left
        btnReimprospateaza.FlatStyle = FlatStyle.Flat
        btnReimprospateaza.Location = New Point(14, 10)
        btnReimprospateaza.Name = "btnReimprospateaza"
        btnReimprospateaza.Size = New Size(170, 44)
        btnReimprospateaza.TabIndex = 0
        btnReimprospateaza.Text = "Reîmprospătează"
        btnReimprospateaza.UseVisualStyleBackColor = True
        '
        ' btnExport
        '
        btnExport.Dock = DockStyle.Right
        btnExport.FlatStyle = FlatStyle.Flat
        btnExport.Location = New Point(646, 10)
        btnExport.Name = "btnExport"
        btnExport.Size = New Size(200, 44)
        btnExport.TabIndex = 1
        btnExport.Text = "Exportă istoricul..."
        btnExport.UseVisualStyleBackColor = True
        '
        ' btnInchide
        '
        btnInchide.Dock = DockStyle.Right
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Location = New Point(846, 10)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New Size(150, 44)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.Size = New Size(1010, 48)
        capBar.TabIndex = 2
        capBar.TabStop = False
        capBar.Text = "Istoric acțiuni FOREXE"
        '
        ' ForexeHistoryForm
        '
        AutoScaleDimensions = New SizeF(6F, 14F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1012, 661)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MinimumSize = New Size(760, 460)
        Name = "ForexeHistoryForm"
        Padding = New Padding(1, 3, 1, 3)
        StartPosition = FormStartPosition.CenterParent
        Text = "Istoric acțiuni FOREXE"
        pnlCard.ResumeLayout(False)
        splitMain.Panel1.ResumeLayout(False)
        splitMain.Panel2.ResumeLayout(False)
        CType(splitMain, ComponentModel.ISupportInitialize).EndInit()
        splitMain.ResumeLayout(False)
        pnlFoot.ResumeLayout(False)
        '
        ' tips — etichetele de survolare, toate în română.
        '
        tips.SetToolTipHeader(btnReimprospateaza, "Reîmprospătează")
        tips.SetToolTipText(btnReimprospateaza, "Recitește lista lucrărilor FOREXE." & vbLf & "Folosește-o după ce o descărcare s-a încheiat cu fereastra deschisă.")
        tips.SetToolTipHeader(btnExport, "Exportă")
        tips.SetToolTipText(btnExport, "Scrie tot istoricul într-un fișier text: lucrări, rezultate și jurnalul fiecăreia.")
        tips.SetToolTipHeader(btnInchide, "Închide")
        tips.SetToolTipText(btnInchide, "Ascunde fereastra." & vbLf & "Istoricul rămâne în memorie cât timp K-BOT e deschis.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents splitMain As SplitContainer
    Friend WithEvents treeJobs As Global.KBot.Controls.AdvancedTreeControl
    Friend WithEvents rtbDetalii As RichTextBox
    Friend WithEvents pnlFoot As Panel
    Friend WithEvents btnReimprospateaza As Button
    Friend WithEvents btnExport As Button
    Friend WithEvents btnInchide As Button
End Class
