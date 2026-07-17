<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits KBot.Theming.KBotShellForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        pnlRoot = New Panel()
        capBar = New KBot.Theming.KBotCaptionBar()
        busyBar = New KBot.Theming.KBotBusyBar()
        pnlHeader = New Panel()
        lblUnit = New Label()
        lblAn = New Label()
        cboAn = New ComboBox()
        lblSs = New Label()
        cboSs = New ComboBox()
        lblForexe = New Label()
        pnlStatus = New Panel()
        lblOperator = New Label()
        lblProgram = New Label()
        btnIstoric = New Button()
        btnSinc = New Button()
        pnlWork = New Panel()
        navViews = New KBot.Theming.KBotNavList()
        split = New SplitContainer()
        pnlTree = New Panel()
        pnlTreeHead = New Panel()
        lblTree = New Label()
        btnInfo = New Button()
        btnSort = New Button()
        btnOpt = New Button()
        tree = New KBot.Controls.AdvancedTreeControl()
        viewHost = New Panel()
        pnlRoot.SuspendLayout()
        pnlHeader.SuspendLayout()
        pnlStatus.SuspendLayout()
        pnlWork.SuspendLayout()
        CType(split, System.ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        pnlTree.SuspendLayout()
        pnlTreeHead.SuspendLayout()
        SuspendLayout()
        '
        ' pnlRoot — cardul rădăcină; copiii se adaugă în ordine INVERSĂ de dock:
        ' pnlWork (Fill) primul, apoi pnlStatus (Bottom), pnlHeader / busyBar / capBar (Top).
        '
        pnlRoot.Controls.Add(pnlWork)
        pnlRoot.Controls.Add(pnlStatus)
        pnlRoot.Controls.Add(pnlHeader)
        pnlRoot.Controls.Add(busyBar)
        pnlRoot.Controls.Add(capBar)
        pnlRoot.Dock = DockStyle.Fill
        pnlRoot.Location = New Point(1, 1)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Size = New Size(1278, 758)
        pnlRoot.TabIndex = 0
        pnlRoot.Tag = "Card"
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.Location = New Point(0, 0)
        capBar.Name = "capBar"
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.Size = New Size(1278, 40)
        capBar.TabIndex = 4
        capBar.TabStop = False
        capBar.Text = "K-BOT"
        '
        ' busyBar
        '
        busyBar.Dock = DockStyle.Top
        busyBar.Location = New Point(0, 40)
        busyBar.Name = "busyBar"
        busyBar.Size = New Size(1278, 3)
        busyBar.TabIndex = 3
        busyBar.TabStop = False
        '
        ' pnlHeader — banda sub caption: unitate, An/SS, stare Forexe.
        '
        pnlHeader.Controls.Add(lblUnit)
        pnlHeader.Controls.Add(lblAn)
        pnlHeader.Controls.Add(cboAn)
        pnlHeader.Controls.Add(lblSs)
        pnlHeader.Controls.Add(cboSs)
        pnlHeader.Controls.Add(lblForexe)
        pnlHeader.Dock = DockStyle.Top
        pnlHeader.Location = New Point(0, 43)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Size = New Size(1278, 40)
        pnlHeader.TabIndex = 2
        pnlHeader.Tag = "Card"
        '
        ' lblUnit
        '
        lblUnit.AutoSize = True
        lblUnit.Font = New Font("Segoe UI Semibold", 10.0F)
        lblUnit.Location = New Point(12, 10)
        lblUnit.Name = "lblUnit"
        lblUnit.Size = New Size(63, 19)
        lblUnit.TabIndex = 0
        lblUnit.Text = "Unitate"
        '
        ' lblAn
        '
        lblAn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblAn.AutoSize = True
        lblAn.Location = New Point(846, 12)
        lblAn.Name = "lblAn"
        lblAn.Size = New Size(27, 15)
        lblAn.TabIndex = 1
        lblAn.Text = "An:"
        '
        ' cboAn
        '
        cboAn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        cboAn.DropDownStyle = ComboBoxStyle.DropDownList
        cboAn.FlatStyle = FlatStyle.Flat
        cboAn.Location = New Point(880, 8)
        cboAn.Name = "cboAn"
        cboAn.Size = New Size(72, 23)
        cboAn.TabIndex = 2
        '
        ' lblSs
        '
        lblSs.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblSs.AutoSize = True
        lblSs.Location = New Point(966, 12)
        lblSs.Name = "lblSs"
        lblSs.Size = New Size(24, 15)
        lblSs.TabIndex = 3
        lblSs.Text = "SS:"
        '
        ' cboSs
        '
        cboSs.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        cboSs.DropDownStyle = ComboBoxStyle.DropDownList
        cboSs.FlatStyle = FlatStyle.Flat
        cboSs.Location = New Point(1000, 8)
        cboSs.Name = "cboSs"
        cboSs.Size = New Size(90, 23)
        cboSs.TabIndex = 4
        '
        ' lblForexe — indicator de stare FOREXE (doar afișaj; ownership-ul e la runner).
        '
        lblForexe.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblForexe.Location = New Point(1106, 12)
        lblForexe.Name = "lblForexe"
        lblForexe.Size = New Size(160, 15)
        lblForexe.TabIndex = 5
        lblForexe.Text = "● Forexe: neconectat"
        lblForexe.TextAlign = ContentAlignment.MiddleRight
        '
        ' pnlStatus — bara de jos: operator + program, Istoric, Sincronizare.
        '
        pnlStatus.Controls.Add(lblOperator)
        pnlStatus.Controls.Add(lblProgram)
        pnlStatus.Controls.Add(btnIstoric)
        pnlStatus.Controls.Add(btnSinc)
        pnlStatus.Dock = DockStyle.Bottom
        pnlStatus.Location = New Point(0, 714)
        pnlStatus.Name = "pnlStatus"
        pnlStatus.Size = New Size(1278, 44)
        pnlStatus.TabIndex = 1
        pnlStatus.Tag = "Card"
        '
        ' lblOperator
        '
        lblOperator.AutoSize = True
        lblOperator.Location = New Point(12, 14)
        lblOperator.Name = "lblOperator"
        lblOperator.Size = New Size(57, 15)
        lblOperator.TabIndex = 0
        lblOperator.Text = "Operator"
        '
        ' lblProgram
        '
        lblProgram.AutoSize = True
        lblProgram.Location = New Point(380, 14)
        lblProgram.Name = "lblProgram"
        lblProgram.Size = New Size(55, 15)
        lblProgram.TabIndex = 1
        lblProgram.Text = "Program"
        '
        ' btnIstoric
        '
        btnIstoric.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnIstoric.FlatStyle = FlatStyle.Flat
        btnIstoric.Location = New Point(996, 6)
        btnIstoric.Name = "btnIstoric"
        btnIstoric.Size = New Size(110, 32)
        btnIstoric.TabIndex = 2
        btnIstoric.Text = "Istoric"
        btnIstoric.UseVisualStyleBackColor = True
        '
        ' btnSinc
        '
        btnSinc.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnSinc.FlatStyle = FlatStyle.Flat
        btnSinc.Font = New Font("Segoe UI Semibold", 10.0F)
        btnSinc.Location = New Point(1116, 6)
        btnSinc.Name = "btnSinc"
        btnSinc.Size = New Size(150, 32)
        btnSinc.TabIndex = 3
        btnSinc.Text = "Sincronizare"
        btnSinc.UseVisualStyleBackColor = True
        '
        ' pnlWork — zona de lucru: navigație stânga + arbore/detaliu (Surface, fără Card).
        '
        pnlWork.Controls.Add(split)
        pnlWork.Controls.Add(navViews)
        pnlWork.Dock = DockStyle.Fill
        pnlWork.Location = New Point(0, 83)
        pnlWork.Name = "pnlWork"
        pnlWork.Padding = New Padding(8)
        pnlWork.Size = New Size(1278, 631)
        pnlWork.TabIndex = 0
        '
        ' navViews
        '
        navViews.Dock = DockStyle.Left
        navViews.Location = New Point(8, 8)
        navViews.Name = "navViews"
        navViews.Size = New Size(170, 615)
        navViews.TabIndex = 0
        '
        ' split
        '
        split.Dock = DockStyle.Fill
        split.Location = New Point(178, 8)
        split.Name = "split"
        '
        ' split.Panel1
        '
        split.Panel1.Controls.Add(pnlTree)
        split.Panel1.Padding = New Padding(8, 0, 0, 0)
        split.Panel1MinSize = 240
        '
        ' split.Panel2
        '
        split.Panel2.Controls.Add(viewHost)
        split.Panel2.Padding = New Padding(8, 0, 0, 0)
        split.Panel2MinSize = 400
        split.Size = New Size(1092, 615)
        split.SplitterDistance = 380
        split.SplitterWidth = 6
        split.TabIndex = 1
        '
        ' pnlTree — cardul arborelui: antet (titlu + sortare + opțiuni) + arbore.
        '
        pnlTree.Controls.Add(tree)
        pnlTree.Controls.Add(pnlTreeHead)
        pnlTree.Dock = DockStyle.Fill
        pnlTree.Location = New Point(8, 0)
        pnlTree.Name = "pnlTree"
        pnlTree.Size = New Size(372, 615)
        pnlTree.TabIndex = 0
        pnlTree.Tag = "Card"
        '
        ' pnlTreeHead
        '
        pnlTreeHead.Controls.Add(lblTree)
        pnlTreeHead.Controls.Add(btnInfo)
        pnlTreeHead.Controls.Add(btnSort)
        pnlTreeHead.Controls.Add(btnOpt)
        pnlTreeHead.Dock = DockStyle.Top
        pnlTreeHead.Location = New Point(0, 0)
        pnlTreeHead.Name = "pnlTreeHead"
        pnlTreeHead.Size = New Size(372, 36)
        pnlTreeHead.TabIndex = 1
        pnlTreeHead.Tag = "Card"
        '
        ' lblTree
        '
        lblTree.AutoSize = True
        lblTree.Font = New Font("Segoe UI Semibold", 10.0F)
        lblTree.Location = New Point(10, 8)
        lblTree.Name = "lblTree"
        lblTree.Size = New Size(103, 19)
        lblTree.TabIndex = 0
        lblTree.Text = "Angajamente"
        '
        ' btnInfo — deschide fereastra nemodală «Informații interne» (flag-urile Are*).
        '
        btnInfo.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnInfo.FlatStyle = FlatStyle.Flat
        btnInfo.Location = New Point(270, 4)
        btnInfo.Name = "btnInfo"
        btnInfo.Size = New Size(28, 28)
        btnInfo.TabIndex = 1
        btnInfo.Text = "ⓘ"
        btnInfo.UseVisualStyleBackColor = True
        '
        ' btnSort
        '
        btnSort.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnSort.FlatStyle = FlatStyle.Flat
        btnSort.Location = New Point(304, 4)
        btnSort.Name = "btnSort"
        btnSort.Size = New Size(28, 28)
        btnSort.TabIndex = 1
        btnSort.Text = "↕"
        btnSort.UseVisualStyleBackColor = True
        '
        ' btnOpt
        '
        btnOpt.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnOpt.FlatStyle = FlatStyle.Flat
        btnOpt.Location = New Point(338, 4)
        btnOpt.Name = "btnOpt"
        btnOpt.Size = New Size(28, 28)
        btnOpt.TabIndex = 2
        btnOpt.Text = "…"
        btnOpt.UseVisualStyleBackColor = True
        '
        ' tree
        '
        tree.Dock = DockStyle.Fill
        tree.Location = New Point(0, 36)
        tree.Name = "tree"
        tree.Size = New Size(372, 579)
        tree.TabIndex = 0
        '
        ' viewHost — gazda vederilor (UserControl-urile IAngajamentView, create lazy).
        '
        viewHost.Dock = DockStyle.Fill
        viewHost.Location = New Point(8, 0)
        viewHost.Name = "viewHost"
        viewHost.Size = New Size(698, 615)
        viewHost.TabIndex = 0
        viewHost.Tag = "Card"
        '
        ' MainForm
        '
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1280, 760)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        MinimumSize = New Size(1100, 640)
        Name = "MainForm"
        Padding = New Padding(1)
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT"
        pnlRoot.ResumeLayout(False)
        pnlHeader.ResumeLayout(False)
        pnlHeader.PerformLayout()
        pnlStatus.ResumeLayout(False)
        pnlStatus.PerformLayout()
        pnlWork.ResumeLayout(False)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, System.ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        pnlTree.ResumeLayout(False)
        pnlTreeHead.ResumeLayout(False)
        pnlTreeHead.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlRoot As Panel
    Friend WithEvents capBar As KBot.Theming.KBotCaptionBar
    Friend WithEvents busyBar As KBot.Theming.KBotBusyBar
    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblUnit As Label
    Friend WithEvents lblAn As Label
    Friend WithEvents cboAn As ComboBox
    Friend WithEvents lblSs As Label
    Friend WithEvents cboSs As ComboBox
    Friend WithEvents lblForexe As Label
    Friend WithEvents pnlStatus As Panel
    Friend WithEvents lblOperator As Label
    Friend WithEvents lblProgram As Label
    Friend WithEvents btnIstoric As Button
    Friend WithEvents btnSinc As Button
    Friend WithEvents pnlWork As Panel
    Friend WithEvents navViews As KBot.Theming.KBotNavList
    Friend WithEvents split As SplitContainer
    Friend WithEvents pnlTree As Panel
    Friend WithEvents pnlTreeHead As Panel
    Friend WithEvents lblTree As Label
    Friend WithEvents btnInfo As Button
    Friend WithEvents btnSort As Button
    Friend WithEvents btnOpt As Button
    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents viewHost As Panel
End Class
