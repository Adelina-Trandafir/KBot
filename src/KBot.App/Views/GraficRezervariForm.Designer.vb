Imports KBot.Controls

' Fereastra graficului rezervarilor (felia 0061-02). O deschide iconita din DREAPTA antetului
' arborelui din `RezervariView`, la fel cum iconita din antetul recepțiilor deschide editorul de
' legaturi.
'
' Are graficul EI, nu unul imprumutat. La `AsociereForm` graficul se muta dintr-un formular in
' altul fiindca datele si toti tratatorii lui stateau deja acolo; aici nu exista niciun grafic al
' rezervarilor de imprumutat, deci suprafata se naste unde e si folosita. Datele intra o data,
' prin constructor, si nu se mai schimba cat fereastra e deschisa: rezervarile s-au citit deja de
' `RezervariView`, iar o a doua cerere de retea ar putea raspunde ALTCEVA decat scrie in arborele
' de sub ea.
'
' Redimensionabila: mosteneste `KBotShellForm`, care da inapoi banda de 8px de pe margini
' pierduta de FormBorderStyle.None.
'
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md).
'
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste: (96, 96) in
' AutoScaleMode.Dpi (felia 0066-02). Cele doua se schimba INTOTDEAUNA impreuna.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class GraficRezervariForm
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

    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim KBotChartTab1 As KBotChartTab = New KBotChartTab()
        Dim KBotChartTab2 As KBotChartTab = New KBotChartTab()
        pnlCard = New Panel()
        grafic = New KBotChartView()
        capBar = New KBotCaptionBar()
        btnInchide = New Button()
        tlyGrafic = New Global.KBot.Controls.KBotTableLayoutPanel()
        pnlCard.SuspendLayout()
        CType(grafic, ComponentModel.ISupportInitialize).BeginInit()
        tlyGrafic.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(grafic)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 44)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(12, 8, 12, 8)
        pnlCard.Size = New Size(940, 512)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' grafic
        '
        ' Doua file, si amandoua se desfac din ACELEASI randuri pe care le arata arborele:
        ' evolutia e suma alergatoare a operatiilor, iar «pe luni» sunt chiar totalurile scrise
        ' pe folderele de luna. O a treia cifra, care sa nu se poata gasi in arbore, ar pune
        ' operatorul sa aleaga in ce sa creada.
        grafic.Dock = DockStyle.Fill
        grafic.EmptyText = "Angajamentul nu are rezervări."
        grafic.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        grafic.HeaderBackColor = SystemColors.Control
        grafic.HeaderCaption = " REZERVĂRILE ANGAJAMENTULUI"
        grafic.HeaderGradient = 5
        grafic.HeaderHeight = 30
        grafic.HeaderSeparatorWidth = 2
        grafic.LegendVisible = True
        grafic.Location = New Point(12, 8)
        grafic.Margin = New Padding(4, 5, 4, 5)
        grafic.MomentFormat = "dd.MM.yy"
        grafic.Name = "grafic"
        grafic.PlotMargin = 12
        grafic.SelectedTabKey = "evolutie"
        grafic.Size = New Size(916, 496)
        grafic.TabHeight = 26
        grafic.TabIndex = 0
        grafic.TabPadding = 6
        grafic.ValueFormat = "N0"
        KBotChartTab1.Key = "evolutie"
        KBotChartTab1.Text = "Evoluția"
        KBotChartTab1.Tooltip = "Cât era rezervat, zi de zi" & vbCrLf & "Fiecare treaptă este o zi cu operații de rezervare; înălțimea ei este suma tuturor operațiilor de până atunci."
        KBotChartTab2.Key = "luni"
        KBotChartTab2.Text = "Pe luni"
        KBotChartTab2.Tooltip = "Totalul fiecărei luni" & vbCrLf & "Aceleași sume care scriu pe folderele de lună din arbore — nu cumulate, ci luna cu luna."
        grafic.Tabs.Add(KBotChartTab1)
        grafic.Tabs.Add(KBotChartTab2)
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
        capBar.Size = New Size(940, 44)
        capBar.TabIndex = 1
        capBar.TabStop = False
        capBar.Text = "K-BOT — Graficul rezervărilor"
        '
        ' btnInchide
        '
        btnInchide.AutoSize = True
        btnInchide.Dock = DockStyle.Right
        btnInchide.Location = New Point(810, 560)
        btnInchide.Margin = New Padding(4, 5, 4, 5)
        btnInchide.Name = "btnInchide"
        btnInchide.Padding = New Padding(17, 8, 17, 8)
        btnInchide.Size = New Size(126, 46)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' tlyGrafic
        '
        tlyGrafic.ColumnCount = 1
        tlyGrafic.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyGrafic.Controls.Add(capBar, 0, 0)
        tlyGrafic.Controls.Add(pnlCard, 0, 1)
        tlyGrafic.Controls.Add(btnInchide, 0, 2)
        tlyGrafic.Dock = DockStyle.Fill
        tlyGrafic.Location = New Point(1, 1)
        tlyGrafic.Margin = New Padding(0)
        tlyGrafic.Name = "tlyGrafic"
        tlyGrafic.RowCount = 3
        tlyGrafic.RowStyles.Add(New RowStyle())
        tlyGrafic.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyGrafic.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyGrafic.Size = New Size(940, 612)
        tlyGrafic.TabIndex = 0
        '
        ' GraficRezervariForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(942, 614)
        Controls.Add(tlyGrafic)
        FormBorderStyle = FormBorderStyle.None
        MinimizeBox = False
        MinimumSize = New Size(640, 380)
        Name = "GraficRezervariForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Graficul rezervărilor"
        CType(grafic, ComponentModel.ISupportInitialize).EndInit()
        pnlCard.ResumeLayout(False)
        tlyGrafic.ResumeLayout(False)
        tlyGrafic.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlCard As Panel
    Friend WithEvents grafic As KBotChartView
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents btnInchide As Button
    Friend WithEvents tlyGrafic As Global.KBot.Controls.KBotTableLayoutPanel
End Class
