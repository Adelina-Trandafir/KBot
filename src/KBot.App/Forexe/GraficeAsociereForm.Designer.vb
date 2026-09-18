Imports KBot.Controls

' Fereastra graficelor asocierii (felia 0061). Aici stau, cat e deschisa, cele trei suprafete de
' privit ale lui `AsociereForm`: banda de nume (`navGrafice`), graficul evolutiei (`grafic`) si
' benzile de asezare (`benzi`) -- si, in stanga lor, ARBORELE RECEPTIILOR (`treeLant`).
'
' Arborele e aici fiindca graficul se desface pe recepția ALEASA: fara el, fereastra arata ce
' fusese ales inainte de deschidere si atat, iar «alege o recepție in stanga» nu avea unde sa se
' intample. Tot el e si celalalt capat al culorilor -- fiecare punct din grafic are culoarea
' randului lui din arbore -- si al clicului pe punct, care selecteaza randul. La BENZI arborele
' se strange: acolo nu se alege nimic, se trage (cererea operatorului din 10.09.2026).
'
' NU are controalele ei. Si panoul cu grafice (`pnlGrafice`), si arborele sunt declarate in
' `AsociereForm` si se MUTA aici la deschidere, inapoi la inchidere. Motivul e ca datele si toti
' tratatorii stau in formularul-parinte: o a doua copie a suprafetelor ar insemna o a doua serie
' de culori, de repere si de reguli de tragere, care s-ar putea abate de la primele. Aceeasi
' hotarare ca la `AsociereBenziForm`, dusa mai departe.
'
' Redimensionabila: mosteneste `KBotShellForm`, care da inapoi banda de 8px de pe margini
' pierduta de FormBorderStyle.None.
'
' Toate controalele PROPRII se declara AICI (docs/kbot-forms-ui-convention.md).
'
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste: Calibri 9 se masoara
' (6, 14) acolo (felia 0052). Cele doua se schimba INTOTDEAUNA impreuna.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class GraficeAsociereForm
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
        pnlCard = New Panel()
        splitGrafice = New SplitContainer()
        capBar = New KBotCaptionBar()
        btnInchide = New Button()
        tlyGrafice = New Global.KBot.Controls.KBotTableLayoutPanel()
        pnlCard.SuspendLayout()
        CType(splitGrafice, ComponentModel.ISupportInitialize).BeginInit()
        splitGrafice.SuspendLayout()
        tlyGrafice.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        ' Singurul copil e despartitorul; cele doua panouri ale lui raman GOALE in designer,
        ' fiindca si arborele recepțiilor, si panoul cu grafice vin de la formularul-parinte,
        ' la deschidere.
        pnlCard.Controls.Add(splitGrafice)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 44)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(12, 8, 12, 8)
        pnlCard.Size = New Size(1000, 552)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' splitGrafice
        '
        ' Arborele recepțiilor in stanga, graficul (sau benzile) in dreapta. Amandoua panourile
        ' sunt goale aici: controalele vin imprumutate de la `AsociereForm`, la deschidere.
        ' Panoul din stanga se STRANGE cand se trece pe benzi -- acolo nu se alege nimic, deci
        ' arborele ar lua loc degeaba (vezi GraficeAsociereForm.AratArborele).
        splitGrafice.Dock = DockStyle.Fill
        splitGrafice.Location = New Point(12, 8)
        splitGrafice.Margin = New Padding(0)
        splitGrafice.Name = "splitGrafice"
        splitGrafice.Panel1MinSize = 180
        splitGrafice.Panel2MinSize = 320
        splitGrafice.Size = New Size(976, 536)
        splitGrafice.SplitterDistance = 300
        splitGrafice.SplitterWidth = 9
        splitGrafice.TabIndex = 0
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
        capBar.Text = "K-BOT — Grafice și benzi"
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
        ' tlyGrafice
        '
        tlyGrafice.ColumnCount = 1
        tlyGrafice.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyGrafice.Controls.Add(capBar, 0, 0)
        tlyGrafice.Controls.Add(pnlCard, 0, 1)
        tlyGrafice.Controls.Add(btnInchide, 0, 2)
        tlyGrafice.Dock = DockStyle.Fill
        tlyGrafice.Location = New Point(1, 1)
        tlyGrafice.Margin = New Padding(0)
        tlyGrafice.Name = "tlyGrafice"
        tlyGrafice.RowCount = 3
        tlyGrafice.RowStyles.Add(New RowStyle())
        tlyGrafice.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyGrafice.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyGrafice.Size = New Size(1000, 652)
        tlyGrafice.TabIndex = 0
        '
        ' GraficeAsociereForm
        '
        AutoScaleDimensions = New SizeF(6F, 14F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1002, 654)
        Controls.Add(tlyGrafice)
        FormBorderStyle = FormBorderStyle.None
        MinimizeBox = False
        MinimumSize = New Size(700, 400)
        Name = "GraficeAsociereForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Grafice și benzi"
        CType(splitGrafice, ComponentModel.ISupportInitialize).EndInit()
        splitGrafice.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyGrafice.ResumeLayout(False)
        tlyGrafice.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlCard As Panel
    Friend WithEvents splitGrafice As SplitContainer
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents btnInchide As Button
    Friend WithEvents tlyGrafice As Global.KBot.Controls.KBotTableLayoutPanel
End Class
