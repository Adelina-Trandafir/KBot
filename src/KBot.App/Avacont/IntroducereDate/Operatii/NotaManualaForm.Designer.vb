Option Strict On
Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class NotaManualaForm
    Inherits Global.KBot.Theming.KBotModalForm

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        notaManualaView = New NotaManualaView()
        tlyMain.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(notaManualaView, 0, 1)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(4, 4)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 57F))
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tlyMain.Size = New Size(1610, 864)
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
        capBar.Size = New Size(1610, 57)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Notă manuală"
        ' 
        ' notaManualaView
        ' 
        notaManualaView.Dock = DockStyle.Fill
        notaManualaView.Font = New Font("Calibri", 9F)
        notaManualaView.Location = New Point(0, 57)
        notaManualaView.Margin = New Padding(0)
        notaManualaView.Name = "notaManualaView"
        notaManualaView.Size = New Size(1610, 630)
        notaManualaView.TabIndex = 2
        ' 
        ' NotaManualaForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1618, 872)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MinimizeBox = False
        MinimumSize = New Size(1320, 810)
        Name = "NotaManualaForm"
        Padding = New Padding(4)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Notă manuală"
        tlyMain.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Private tlyMain As KBotTableLayoutPanel
    Private capBar As KBotCaptionBar
    Private WithEvents notaManualaView As NotaManualaView
End Class
