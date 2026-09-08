<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class BrowserDockHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    ' Bench for the docked browser: a real Chromium window reparented into pnlBrowser, the
    ' real WicketMonitorForm hosted inside pnlMonitor, and the executor log underneath.
    ' Controls are declared here (house rule: WinForms controls live in .Designer.vb).

    Friend WithEvents pnlTop As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnPornire As System.Windows.Forms.Button
    Friend WithEvents btnAndocare As System.Windows.Forms.Button
    Friend WithEvents btnDetasare As System.Windows.Forms.Button
    Friend WithEvents btnSincronizare As System.Windows.Forms.Button
    Friend WithEvents btnReincarca As System.Windows.Forms.Button
    Friend WithEvents btnMonitor As System.Windows.Forms.Button
    Friend WithEvents chkBaraBrowser As System.Windows.Forms.CheckBox
    Friend WithEvents lblStare As System.Windows.Forms.Label

    Friend WithEvents splitMain As System.Windows.Forms.SplitContainer
    Friend WithEvents pnlBrowser As System.Windows.Forms.Panel
    Friend WithEvents pnlMonitor As System.Windows.Forms.Panel

    Friend WithEvents pnlLog As System.Windows.Forms.Panel
    Friend WithEvents rtbLog As System.Windows.Forms.RichTextBox

    Friend WithEvents pnlVerdict As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnPass As System.Windows.Forms.Button
    Friend WithEvents btnFail As System.Windows.Forms.Button

    Friend WithEvents tmrResync As System.Windows.Forms.Timer

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.pnlTop = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnPornire = New System.Windows.Forms.Button()
        Me.btnAndocare = New System.Windows.Forms.Button()
        Me.btnDetasare = New System.Windows.Forms.Button()
        Me.btnSincronizare = New System.Windows.Forms.Button()
        Me.btnReincarca = New System.Windows.Forms.Button()
        Me.btnMonitor = New System.Windows.Forms.Button()
        Me.chkBaraBrowser = New System.Windows.Forms.CheckBox()
        Me.lblStare = New System.Windows.Forms.Label()
        Me.splitMain = New System.Windows.Forms.SplitContainer()
        Me.pnlBrowser = New System.Windows.Forms.Panel()
        Me.pnlMonitor = New System.Windows.Forms.Panel()
        Me.pnlLog = New System.Windows.Forms.Panel()
        Me.rtbLog = New System.Windows.Forms.RichTextBox()
        Me.pnlVerdict = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnPass = New System.Windows.Forms.Button()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.tmrResync = New System.Windows.Forms.Timer(Me.components)
        Me.pnlTop.SuspendLayout()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.pnlLog.SuspendLayout()
        Me.pnlVerdict.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlTop
        '
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Height = 46
        Me.pnlTop.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlTop.WrapContents = False
        Me.pnlTop.Controls.Add(Me.btnPornire)
        Me.pnlTop.Controls.Add(Me.btnAndocare)
        Me.pnlTop.Controls.Add(Me.btnDetasare)
        Me.pnlTop.Controls.Add(Me.btnSincronizare)
        Me.pnlTop.Controls.Add(Me.btnReincarca)
        Me.pnlTop.Controls.Add(Me.btnMonitor)
        Me.pnlTop.Controls.Add(Me.chkBaraBrowser)
        Me.pnlTop.Controls.Add(Me.lblStare)
        Me.pnlTop.Name = "pnlTop"
        '
        'btnPornire
        '
        Me.btnPornire.AutoSize = True
        Me.btnPornire.Name = "btnPornire"
        Me.btnPornire.Size = New System.Drawing.Size(170, 28)
        Me.btnPornire.Text = "Pornește + navighează"
        '
        'btnAndocare
        '
        Me.btnAndocare.AutoSize = True
        Me.btnAndocare.Name = "btnAndocare"
        Me.btnAndocare.Size = New System.Drawing.Size(110, 28)
        Me.btnAndocare.Text = "Andochează"
        '
        'btnDetasare
        '
        Me.btnDetasare.AutoSize = True
        Me.btnDetasare.Name = "btnDetasare"
        Me.btnDetasare.Size = New System.Drawing.Size(100, 28)
        Me.btnDetasare.Text = "Detașează"
        '
        'btnSincronizare
        '
        Me.btnSincronizare.AutoSize = True
        Me.btnSincronizare.Name = "btnSincronizare"
        Me.btnSincronizare.Size = New System.Drawing.Size(130, 28)
        Me.btnSincronizare.Text = "Resincronizează"
        '
        'btnReincarca
        '
        Me.btnReincarca.AutoSize = True
        Me.btnReincarca.Name = "btnReincarca"
        Me.btnReincarca.Size = New System.Drawing.Size(130, 28)
        Me.btnReincarca.Text = "Reîncarcă pagina"
        '
        'btnMonitor
        '
        Me.btnMonitor.AutoSize = True
        Me.btnMonitor.Name = "btnMonitor"
        Me.btnMonitor.Size = New System.Drawing.Size(130, 28)
        Me.btnMonitor.Text = "Oprește monitorul"
        '
        'chkBaraBrowser
        '
        Me.chkBaraBrowser.AutoSize = True
        Me.chkBaraBrowser.Checked = True
        Me.chkBaraBrowser.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkBaraBrowser.Margin = New System.Windows.Forms.Padding(14, 9, 3, 0)
        Me.chkBaraBrowser.Name = "chkBaraBrowser"
        Me.chkBaraBrowser.Text = "Ascunde bara browserului"
        '
        'lblStare
        '
        Me.lblStare.AutoSize = True
        Me.lblStare.Margin = New System.Windows.Forms.Padding(14, 9, 3, 0)
        Me.lblStare.Name = "lblStare"
        Me.lblStare.Text = "Sesiune inactivă."
        '
        'splitMain
        '
        Me.splitMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitMain.Orientation = System.Windows.Forms.Orientation.Vertical
        Me.splitMain.Name = "splitMain"
        ' Size BEFORE the minimums and the distance: a fresh SplitContainer is 150px wide and
        ' EndInit refuses a distance that does not fit between Panel1MinSize and
        ' Width - Panel2MinSize. Dock = Fill takes over at the first layout pass.
        Me.splitMain.Size = New System.Drawing.Size(1360, 638)
        Me.splitMain.SplitterWidth = 6
        Me.splitMain.Panel1MinSize = 300
        Me.splitMain.Panel2MinSize = 240
        Me.splitMain.SplitterDistance = 880
        Me.splitMain.Panel1.Controls.Add(Me.pnlBrowser)
        Me.splitMain.Panel2.Controls.Add(Me.pnlMonitor)
        '
        'pnlBrowser — gazda ferestrei Chromium reparentate
        '
        Me.pnlBrowser.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlBrowser.Name = "pnlBrowser"
        '
        'pnlMonitor — gazda WicketMonitorForm (TopLevel = False)
        '
        Me.pnlMonitor.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMonitor.Name = "pnlMonitor"
        '
        'pnlLog
        '
        Me.pnlLog.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlLog.Height = 150
        Me.pnlLog.Name = "pnlLog"
        Me.pnlLog.Controls.Add(Me.rtbLog)
        '
        'rtbLog
        '
        Me.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.rtbLog.Name = "rtbLog"
        Me.rtbLog.ReadOnly = True
        Me.rtbLog.WordWrap = False
        Me.rtbLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both
        '
        'pnlVerdict
        '
        Me.pnlVerdict.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlVerdict.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlVerdict.Height = 46
        Me.pnlVerdict.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlVerdict.WrapContents = False
        Me.pnlVerdict.Controls.Add(Me.btnPass)
        Me.pnlVerdict.Controls.Add(Me.btnFail)
        Me.pnlVerdict.Name = "pnlVerdict"
        '
        'btnPass
        '
        Me.btnPass.AutoSize = True
        Me.btnPass.Name = "btnPass"
        Me.btnPass.Size = New System.Drawing.Size(130, 28)
        Me.btnPass.Text = "Andocarea merge"
        '
        'btnFail
        '
        Me.btnFail.AutoSize = True
        Me.btnFail.Name = "btnFail"
        Me.btnFail.Size = New System.Drawing.Size(140, 28)
        Me.btnFail.Text = "Andocarea nu merge"
        '
        'tmrResync
        '
        Me.tmrResync.Interval = 120
        '
        'BrowserDockHarnessForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1360, 880)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Banc de probă — browser andocat + monitor Wicket"
        ' Ordine inversă de andocare: întâi Fill, apoi marginile.
        Me.Controls.Add(Me.splitMain)
        Me.Controls.Add(Me.pnlLog)
        Me.Controls.Add(Me.pnlVerdict)
        Me.Controls.Add(Me.pnlTop)
        Me.Name = "BrowserDockHarnessForm"
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel2.ResumeLayout(False)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.pnlLog.ResumeLayout(False)
        Me.pnlVerdict.ResumeLayout(False)
        Me.pnlVerdict.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

End Class
