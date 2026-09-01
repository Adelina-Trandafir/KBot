Imports KBot.Controls

' Fereastra mare a benzilor de așezare (felia 0048-07). Aceeași suprafață ca banda strâmtă din
' `AsociereForm`, la o mărime la care încap denumirile, sumele și datele.
'
' Nu are buton de salvare: D-H spune o singură salvare, la sfârșit, iar aceea stă în
' `AsociereForm`. Aici se mută marcaje si atat; tabloul local e comun, deci nu e nimic de impacat
' la inchidere.
'
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md): formularul trebuie sa se
' randeze in designerul Visual Studio, nu sa se construiasca la rulare.
'
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AsociereBenziForm
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

    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        pnlCard = New Panel()
        benziMari = New KBotLaneView()
        capBar = New KBotCaptionBar()
        btnInchide = New Button()
        tlyBenzi = New TableLayoutPanel()
        pnlCard.SuspendLayout()
        CType(benziMari, ComponentModel.ISupportInitialize).BeginInit()
        tlyBenzi.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(benziMari)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 44)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(12, 8, 12, 8)
        pnlCard.Size = New Size(1000, 552)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' benziMari
        '
        benziMari.AxisVisible = True
        benziMari.Dock = DockStyle.Fill
        benziMari.EmptyText = "Angajamentul nu are niciun instantaneu."
        benziMari.EnlargeButtonVisible = False
        benziMari.HeaderCaption = " AȘEZAREA INSTANTANEELOR"
        benziMari.HeaderGradient = 5
        benziMari.HeaderHeight = 30
        benziMari.HeaderSeparatorWidth = 2
        benziMari.LaneCaptionWidth = 190
        benziMari.LaneCaptionsVisible = True
        benziMari.LaneHeight = 26
        benziMari.LaneSpacing = 4
        benziMari.Location = New Point(12, 8)
        benziMari.MarkerSize = 11
        benziMari.Name = "benziMari"
        benziMari.PlotMargin = 10
        benziMari.SegmentWidth = 5
        benziMari.Size = New Size(976, 536)
        benziMari.TabIndex = 0
        benziMari.TrailingSpace = 60
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.ShowThemeEditor = False
        capBar.ShowThemeOptions = False
        capBar.Size = New Size(1000, 44)
        capBar.TabIndex = 1
        capBar.TabStop = False
        capBar.Text = "K-BOT — Benzile de așezare"
        '
        ' btnInchide
        '
        btnInchide.AutoSize = True
        btnInchide.Dock = DockStyle.Right
        btnInchide.Location = New Point(870, 600)
        btnInchide.Margin = New Padding(4, 5, 4, 5)
        btnInchide.Name = "btnInchide"
        btnInchide.Padding = New Padding(17, 8, 17, 8)
        btnInchide.Size = New Size(126, 46)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' tlyBenzi
        '
        tlyBenzi.ColumnCount = 1
        tlyBenzi.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBenzi.Controls.Add(capBar, 0, 0)
        tlyBenzi.Controls.Add(pnlCard, 0, 1)
        tlyBenzi.Controls.Add(btnInchide, 0, 2)
        tlyBenzi.Dock = DockStyle.Fill
        tlyBenzi.Location = New Point(1, 1)
        tlyBenzi.Margin = New Padding(0)
        tlyBenzi.Name = "tlyBenzi"
        tlyBenzi.RowCount = 3
        tlyBenzi.RowStyles.Add(New RowStyle())
        tlyBenzi.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBenzi.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyBenzi.Size = New Size(1000, 652)
        tlyBenzi.TabIndex = 0
        '
        ' AsociereBenziForm
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1002, 654)
        Controls.Add(tlyBenzi)
        FormBorderStyle = FormBorderStyle.None
        MinimizeBox = False
        MinimumSize = New Size(700, 400)
        Name = "AsociereBenziForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Benzile de așezare"
        WindowState = FormWindowState.Maximized
        pnlCard.ResumeLayout(False)
        CType(benziMari, ComponentModel.ISupportInitialize).EndInit()
        tlyBenzi.ResumeLayout(False)
        tlyBenzi.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlCard As Panel
    Friend WithEvents benziMari As KBot.Controls.KBotLaneView
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents btnInchide As Button
    Friend WithEvents tlyBenzi As TableLayoutPanel
End Class
