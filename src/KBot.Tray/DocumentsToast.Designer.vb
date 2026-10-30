' The tray's message above the clock (slice TRAY-01): the documents waiting for the signature, one row each.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates at 96 dpi, AutoScaleMode.Dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DocumentsToast
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
        components = New System.ComponentModel.Container()
        lstDocs = New ListBox()
        lblFooter = New Label()
        lblHeader = New Label()
        tmrHide = New Timer(components)
        SuspendLayout()
        '
        ' lstDocs
        '
        lstDocs.BorderStyle = BorderStyle.None
        lstDocs.Dock = DockStyle.Fill
        lstDocs.DrawMode = DrawMode.OwnerDrawFixed
        lstDocs.FormattingEnabled = False
        lstDocs.IntegralHeight = False
        lstDocs.ItemHeight = 46
        lstDocs.Location = New Point(1, 33)
        lstDocs.Name = "lstDocs"
        lstDocs.Size = New Size(398, 120)
        lstDocs.TabIndex = 0
        '
        ' lblFooter
        '
        lblFooter.Dock = DockStyle.Bottom
        lblFooter.Location = New Point(1, 153)
        lblFooter.Name = "lblFooter"
        lblFooter.Size = New Size(398, 26)
        lblFooter.TabIndex = 1
        lblFooter.Text = "Clic pe un document îl deschide în K-BOT."
        lblFooter.TextAlign = ContentAlignment.MiddleLeft
        lblFooter.Padding = New Padding(10, 0, 10, 0)
        '
        ' lblHeader
        '
        lblHeader.Dock = DockStyle.Top
        lblHeader.Location = New Point(1, 1)
        lblHeader.Name = "lblHeader"
        lblHeader.Size = New Size(398, 32)
        lblHeader.TabIndex = 2
        lblHeader.Text = "Documente de semnat"
        lblHeader.TextAlign = ContentAlignment.MiddleLeft
        lblHeader.Padding = New Padding(10, 0, 10, 0)
        '
        ' tmrHide
        '
        tmrHide.Interval = 30000
        '
        ' DocumentsToast
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(400, 180)
        Controls.Add(lstDocs)
        Controls.Add(lblFooter)
        Controls.Add(lblHeader)
        FormBorderStyle = FormBorderStyle.None
        KeyPreview = True
        Name = "DocumentsToast"
        Padding = New Padding(1)
        ShowIcon = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Documente de semnat"
        TopMost = True
        ResumeLayout(False)
    End Sub

    Friend WithEvents lstDocs As ListBox
    Friend WithEvents lblFooter As Label
    Friend WithEvents lblHeader As Label
    Friend WithEvents tmrHide As Timer
End Class
