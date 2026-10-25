Imports KBot.Controls

' «Plan de conturi» (slice AV01-04): the shell. It hosts the two views, one at a time (the subunit is chosen in the main window's title bar, slice AV01-06):
' PlanContConturiView (tree + fields of the account) and PlanContSolduriView (grid of opening balances).
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PlanContForm
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
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        pnlCard = New Panel()
        tlyBody = New KBotTableLayoutPanel()
        pnlHost = New Panel()
        viewSolduri = New PlanContSolduriView()
        viewConturi = New PlanContConturiView()
        ntfMesaj = New KBotNotice()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        pnlHost.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(ntfMesaj, 0, 1)
        tlyMain.Controls.Add(pnlCard, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(2, 2)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.Size = New Size(1436, 836)
        tlyMain.TabIndex = 0
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.cells
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(1436, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Plan de conturi"
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlyBody)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 66)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(12, 9, 12, 9)
        pnlCard.Size = New Size(1436, 692)
        pnlCard.TabIndex = 2
        pnlCard.Tag = "Card"
        ' 
        ' tlyBody
        ' 
        tlyBody.ColumnCount = 1
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBody.Controls.Add(pnlHost, 0, 0)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(12, 9)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.Size = New Size(1412, 674)
        tlyBody.TabIndex = 0
        ' 
        ' pnlHost
        ' 
        pnlHost.Controls.Add(viewSolduri)
        pnlHost.Controls.Add(viewConturi)
        pnlHost.Dock = DockStyle.Fill
        pnlHost.Location = New Point(0, 0)
        pnlHost.Margin = New Padding(0)
        pnlHost.Name = "pnlHost"
        pnlHost.Size = New Size(1412, 674)
        pnlHost.TabIndex = 1
        ' 
        ' viewSolduri
        ' 
        viewSolduri.Dock = DockStyle.Fill
        viewSolduri.Font = New Font("Calibri", 9F)
        viewSolduri.Location = New Point(0, 0)
        viewSolduri.Margin = New Padding(0)
        viewSolduri.Name = "viewSolduri"
        viewSolduri.Size = New Size(1412, 674)
        viewSolduri.TabIndex = 1
        viewSolduri.Visible = False
        ' 
        ' viewConturi
        ' 
        viewConturi.Dock = DockStyle.Fill
        viewConturi.Font = New Font("Calibri", 9F)
        viewConturi.Location = New Point(0, 0)
        viewConturi.Margin = New Padding(0)
        viewConturi.Name = "viewConturi"
        viewConturi.Size = New Size(1412, 674)
        viewConturi.TabIndex = 0
        ' 
        ' ntfMesaj
        ' 
        ntfMesaj.BackColor = Color.Transparent
        ntfMesaj.Dock = DockStyle.Fill
        ntfMesaj.Location = New Point(12, 72)
        ntfMesaj.Margin = New Padding(12, 9, 12, 0)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New Size(1412, 60)
        ntfMesaj.TabIndex = 1
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' PlanContForm
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1440, 840)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MinimizeBox = False
        MinimumSize = New Size(1440, 840)
        Name = "PlanContForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Plan de conturi"
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyBody.ResumeLayout(False)
        pnlHost.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents pnlHost As Panel
    Friend WithEvents viewConturi As PlanContConturiView
    Friend WithEvents viewSolduri As PlanContSolduriView
    Friend WithEvents ntfMesaj As KBotNotice
End Class
