Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class BrowserView
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        tmrResync = New Timer(components)
        pnlBrowser = New Panel()
        lblGol = New Label()
        pnlAntet = New Panel()
        lblStare = New Label()
        btnAdu = New Button()
        busy = New KBotBusyBar()
        pnlBrowser.SuspendLayout()
        pnlAntet.SuspendLayout()
        SuspendLayout()
        '
        ' pnlBrowser - the HOST of the docked browser window (SetParent target). Added FIRST
        ' (card rule: Fill first, then the bands). The label inside is only seen while the
        ' browser is not here; docked, the Chromium window covers the whole panel.
        '
        pnlBrowser.Controls.Add(lblGol)
        pnlBrowser.Dock = DockStyle.Fill
        pnlBrowser.Location = New Point(0, 39)
        pnlBrowser.Name = "pnlBrowser"
        pnlBrowser.Size = New Size(800, 461)
        pnlBrowser.TabIndex = 2
        '
        ' lblGol
        '
        lblGol.Dock = DockStyle.Fill
        lblGol.Font = New Font("Segoe UI", 11.0F)
        lblGol.Location = New Point(0, 0)
        lblGol.Name = "lblGol"
        lblGol.Size = New Size(800, 461)
        lblGol.TabIndex = 0
        lblGol.Text = "Nu există o sesiune FOREXE."
        lblGol.TextAlign = ContentAlignment.MiddleCenter
        '
        ' pnlAntet - one line of state and the «bring it here» button
        '
        pnlAntet.Controls.Add(lblStare)
        pnlAntet.Controls.Add(btnAdu)
        pnlAntet.Dock = DockStyle.Top
        pnlAntet.Location = New Point(0, 3)
        pnlAntet.Name = "pnlAntet"
        pnlAntet.Padding = New Padding(10, 4, 10, 4)
        pnlAntet.Size = New Size(800, 36)
        pnlAntet.TabIndex = 1
        '
        ' lblStare
        '
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(10, 4)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(620, 28)
        lblStare.TabIndex = 0
        lblStare.Text = "Browser FOREXE"
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnAdu - visible only while the browser is somewhere else (the recorder) or hidden
        '
        btnAdu.Dock = DockStyle.Right
        btnAdu.FlatStyle = FlatStyle.Flat
        btnAdu.Location = New Point(630, 4)
        btnAdu.Name = "btnAdu"
        btnAdu.Size = New Size(160, 28)
        btnAdu.TabIndex = 1
        btnAdu.Text = "Adu browserul aici"
        btnAdu.UseVisualStyleBackColor = True
        btnAdu.Visible = False
        '
        ' busy - runs while the browser is docking or an angajament is being opened
        '
        busy.Dock = DockStyle.Top
        busy.Location = New Point(0, 0)
        busy.Name = "busy"
        busy.Size = New Size(800, 3)
        busy.TabIndex = 0
        '
        ' tmrResync - debounce for the host panel's Resize
        '
        tmrResync.Interval = 150
        '
        ' tips
        '
        tips.SetToolTipHeader(btnAdu, "Adu browserul aici")
        tips.SetToolTipText(btnAdu, "Andochează browserul FOREXE în această vedere (îl ia din fereastra Recorder dacă e acolo).")
        '
        ' BrowserView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(pnlBrowser)
        Controls.Add(pnlAntet)
        Controls.Add(busy)
        Name = "BrowserView"
        Size = New Size(800, 500)
        pnlBrowser.ResumeLayout(False)
        pnlAntet.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tmrResync As Timer
    Friend WithEvents pnlBrowser As Panel
    Friend WithEvents lblGol As Label
    Friend WithEvents pnlAntet As Panel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnAdu As Button
    Friend WithEvents busy As KBotBusyBar
End Class
