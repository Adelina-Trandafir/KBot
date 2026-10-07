#If DEBUG Then
' Slice 0078-16: the lane picture of one load of activex_check.log -- a lane per root of the viewer's left tree, a marker
' per leaf, time of day only.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ActivexLaneForm
    Inherits KBot.Theming.KBotThemedForm

    Friend WithEvents laneView As KBot.Controls.KBotLaneView
    Friend WithEvents lblStatus As Label

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        laneView = New KBot.Controls.KBotLaneView()
        lblStatus = New Label()
        CType(laneView, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' laneView -- one lane per root of the left tree; the axis is the time of day with its milliseconds
        '
        laneView.AxisVisible = True
        laneView.Dock = DockStyle.Fill
        laneView.EmptyText = "Nu e nimic de arătat."
        laneView.EnlargeButtonVisible = False
        laneView.HeaderGradient = 5
        laneView.HeaderHeight = 30
        laneView.LaneCaptionWidth = 170
        laneView.LaneCaptionsVisible = True
        laneView.LaneHeight = 34
        laneView.LaneSpacing = 6
        laneView.MarkerSize = 9
        laneView.MomentFormat = "HH:mm:ss.fff"
        laneView.Name = "laneView"
        laneView.PlotMargin = 10
        laneView.SegmentWidth = 4
        laneView.TabIndex = 0
        laneView.TrailingSpace = 40
        '
        ' lblStatus
        '
        lblStatus.AutoSize = False
        lblStatus.Dock = DockStyle.Bottom
        lblStatus.Height = 28
        lblStatus.Name = "lblStatus"
        lblStatus.Padding = New Padding(8, 6, 8, 0)
        lblStatus.TabIndex = 1
        lblStatus.Text = ""
        '
        ' ActivexLaneForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1500, 420)
        ' Children in REVERSE dock order: Fill first, then the docked edge.
        Controls.Add(laneView)
        Controls.Add(lblStatus)
        MinimumSize = New Size(700, 300)
        Name = "ActivexLaneForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Jurnalul ActiveX — pe culoare"
        CType(laneView, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

End Class
#End If
