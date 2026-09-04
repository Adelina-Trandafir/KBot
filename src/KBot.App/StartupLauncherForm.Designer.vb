<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class StartupLauncherForm
    Inherits KBot.Theming.KBotThemedForm

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
        tlyRadacina = New TableLayoutPanel()
        capBar = New KBot.Controls.KBotCaptionBar()
        lblIntro = New Label()
        navPorniri = New KBot.Controls.KBotNavList()
        tlyButoane = New TableLayoutPanel()
        btnPorneste = New Button()
        btnIesire = New Button()
        CType(navPorniri, System.ComponentModel.ISupportInitialize).BeginInit()
        tlyRadacina.SuspendLayout()
        tlyButoane.SuspendLayout()
        SuspendLayout()
        '
        ' tlyRadacina — patru rânduri: bara de titlu, textul de îndrumare, lista de porniri
        ' (singura care se întinde) și rândul de butoane.
        '
        tlyRadacina.ColumnCount = 1
        tlyRadacina.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRadacina.Controls.Add(capBar, 0, 0)
        tlyRadacina.Controls.Add(lblIntro, 0, 1)
        tlyRadacina.Controls.Add(navPorniri, 0, 2)
        tlyRadacina.Controls.Add(tlyButoane, 0, 3)
        tlyRadacina.Dock = DockStyle.Fill
        tlyRadacina.Location = New Point(1, 1)
        tlyRadacina.Margin = New Padding(0)
        tlyRadacina.Name = "tlyRadacina"
        tlyRadacina.RowCount = 4
        tlyRadacina.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyRadacina.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyRadacina.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRadacina.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyRadacina.Size = New Size(438, 358)
        tlyRadacina.TabIndex = 0
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Fill
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.ShowMaximize = False
        capBar.ShowMinimize = False
        capBar.Size = New Size(438, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — pornire"
        '
        ' lblIntro
        '
        lblIntro.Dock = DockStyle.Fill
        lblIntro.Location = New Point(12, 40)
        lblIntro.Margin = New Padding(12, 0, 12, 0)
        lblIntro.Name = "lblIntro"
        lblIntro.Padding = New Padding(0, 10, 0, 0)
        lblIntro.Size = New Size(414, 44)
        lblIntro.TabIndex = 1
        lblIntro.Text = "Alegeți fereastra de pornire:"
        '
        ' navPorniri — elementele se adaugă din cod (StartupLauncherForm.New), fiindcă lista lor
        ' e chiar contractul public al ferestrei (cheile din PORNIRI).
        '
        navPorniri.Dock = DockStyle.Fill
        navPorniri.ItemPadding = New Padding(12, 8, 12, 8)
        navPorniri.Location = New Point(12, 84)
        navPorniri.Margin = New Padding(12, 0, 12, 0)
        navPorniri.Name = "navPorniri"
        navPorniri.Orientation = KBot.Controls.KBotNavOrientation.Vertical
        navPorniri.SelectedKey = Nothing
        navPorniri.Size = New Size(414, 222)
        navPorniri.TabIndex = 2
        '
        ' tlyButoane — spațiu elastic la stânga, cele două butoane la dreapta.
        '
        tlyButoane.ColumnCount = 3
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlyButoane.Controls.Add(btnPorneste, 1, 0)
        tlyButoane.Controls.Add(btnIesire, 2, 0)
        tlyButoane.Dock = DockStyle.Fill
        tlyButoane.Location = New Point(12, 306)
        tlyButoane.Margin = New Padding(12, 0, 12, 0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.RowCount = 1
        tlyButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButoane.Size = New Size(414, 52)
        tlyButoane.TabIndex = 3
        '
        ' btnPorneste
        '
        btnPorneste.Dock = DockStyle.Fill
        btnPorneste.FlatStyle = FlatStyle.Flat
        btnPorneste.Location = New Point(184, 8)
        btnPorneste.Margin = New Padding(0, 8, 8, 12)
        btnPorneste.Name = "btnPorneste"
        btnPorneste.Size = New Size(122, 32)
        btnPorneste.TabIndex = 0
        btnPorneste.Text = "Pornește"
        btnPorneste.UseVisualStyleBackColor = True
        '
        ' btnIesire
        '
        btnIesire.DialogResult = DialogResult.Cancel
        btnIesire.Dock = DockStyle.Fill
        btnIesire.FlatStyle = FlatStyle.Flat
        btnIesire.Location = New Point(314, 8)
        btnIesire.Margin = New Padding(0, 8, 0, 12)
        btnIesire.Name = "btnIesire"
        btnIesire.Size = New Size(100, 32)
        btnIesire.TabIndex = 1
        btnIesire.Text = "Ieșire"
        btnIesire.UseVisualStyleBackColor = True
        '
        ' StartupLauncherForm
        '
        AcceptButton = btnPorneste
        AutoScaleDimensions = New SizeF(6F, 14F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnIesire
        ClientSize = New Size(440, 360)
        Controls.Add(tlyRadacina)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(380, 300)
        Name = "StartupLauncherForm"
        Padding = New Padding(1)
        ShowInTaskbar = True
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — pornire"
        CType(navPorniri, System.ComponentModel.ISupportInitialize).EndInit()
        tlyRadacina.ResumeLayout(False)
        tlyButoane.ResumeLayout(False)
        '
        ' tips — etichetele de survolare (felia 0035), toate în română.
        '
        tips.SetToolTipHeader(btnPorneste, "Pornește")
        tips.SetToolTipText(btnPorneste, "Deschide fereastra aleasă în listă.")
        tips.SetToolTipHeader(btnIesire, "Ieșire")
        tips.SetToolTipText(btnIesire, "Închide K-BOT fără să pornească nimic.")
        tips.SetToolTipHeader(navPorniri, "Porniri")
        tips.SetToolTipText(navPorniri, "Alege ce se deschide: aplicația, bancul de probă sau jurnalele." & vbLf & "Lista apare doar în build-ul de dezvoltare.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents tlyRadacina As TableLayoutPanel
    Friend WithEvents capBar As KBot.Controls.KBotCaptionBar
    Friend WithEvents lblIntro As Label
    Friend WithEvents navPorniri As KBot.Controls.KBotNavList
    Friend WithEvents tlyButoane As TableLayoutPanel
    Friend WithEvents btnPorneste As Button
    Friend WithEvents btnIesire As Button
End Class
