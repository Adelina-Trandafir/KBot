<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KBOT_IPC
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(KBOT_IPC))
        rtbLog = New System.Windows.Forms.RichTextBox()
        lblTitleLog = New System.Windows.Forms.Label()
        pnlProgress = New System.Windows.Forms.Panel()
        TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        btnAfiseazaBrowser = New System.Windows.Forms.Button()
        lblStatus = New System.Windows.Forms.Label()
        pbProgress = New System.Windows.Forms.ProgressBar()
        btnAnulare = New System.Windows.Forms.Button()
        btnAfiseazaLog = New System.Windows.Forms.Button()
        tlpLayout = New System.Windows.Forms.TableLayoutPanel()
        chkAutoSalveazaResend = New System.Windows.Forms.CheckBox()
        TT = New System.Windows.Forms.ToolTip(components)
        pnlProgress.SuspendLayout()
        TableLayoutPanel1.SuspendLayout()
        tlpLayout.SuspendLayout()
        SuspendLayout()
        ' 
        ' rtbLog
        ' 
        rtbLog.BackColor = Drawing.Color.White
        rtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None
        tlpLayout.SetColumnSpan(rtbLog, 2)
        rtbLog.Dock = System.Windows.Forms.DockStyle.Fill
        rtbLog.Font = New System.Drawing.Font("Consolas", 9.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        rtbLog.ForeColor = Drawing.Color.Lime
        rtbLog.Location = New System.Drawing.Point(9, 122)
        rtbLog.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        rtbLog.Name = "rtbLog"
        rtbLog.ReadOnly = True
        rtbLog.Size = New System.Drawing.Size(627, 308)
        rtbLog.TabIndex = 4
        rtbLog.Text = ""
        ' 
        ' lblTitleLog
        ' 
        lblTitleLog.AutoSize = True
        lblTitleLog.Font = New System.Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        lblTitleLog.Location = New System.Drawing.Point(9, 82)
        lblTitleLog.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblTitleLog.Name = "lblTitleLog"
        lblTitleLog.Padding = New System.Windows.Forms.Padding(5)
        lblTitleLog.Size = New System.Drawing.Size(156, 35)
        lblTitleLog.TabIndex = 3
        lblTitleLog.Text = "Jurnal Execuție:"
        ' 
        ' pnlProgress
        ' 
        tlpLayout.SetColumnSpan(pnlProgress, 2)
        pnlProgress.Controls.Add(TableLayoutPanel1)
        pnlProgress.Dock = System.Windows.Forms.DockStyle.Fill
        pnlProgress.Location = New System.Drawing.Point(9, 10)
        pnlProgress.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        pnlProgress.Name = "pnlProgress"
        pnlProgress.Size = New System.Drawing.Size(627, 67)
        pnlProgress.TabIndex = 2
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.ColumnCount = 5
        TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52.0F))
        TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52.0F))
        TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52.0F))
        TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52.0F))
        TableLayoutPanel1.Controls.Add(btnAfiseazaBrowser, 3, 1)
        TableLayoutPanel1.Controls.Add(lblStatus, 0, 0)
        TableLayoutPanel1.Controls.Add(pbProgress, 0, 1)
        TableLayoutPanel1.Controls.Add(btnAnulare, 1, 1)
        TableLayoutPanel1.Controls.Add(btnAfiseazaLog, 4, 1)
        TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(0)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 2
        TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38.80597F))
        TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 61.19403F))
        TableLayoutPanel1.Size = New System.Drawing.Size(627, 67)
        TableLayoutPanel1.TabIndex = 0
        ' 
        ' btnAfiseazaBrowser
        ' 
        btnAfiseazaBrowser.Dock = System.Windows.Forms.DockStyle.Fill
        btnAfiseazaBrowser.FlatAppearance.BorderColor = Drawing.SystemColors.Control
        btnAfiseazaBrowser.FlatAppearance.MouseOverBackColor = Drawing.Color.FromArgb(CByte(255), CByte(224), CByte(192))
        btnAfiseazaBrowser.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnAfiseazaBrowser.Image = CType(resources.GetObject("btnAfiseazaBrowser.Image"), Drawing.Image)
        btnAfiseazaBrowser.Location = New System.Drawing.Point(523, 26)
        btnAfiseazaBrowser.Margin = New System.Windows.Forms.Padding(0)
        btnAfiseazaBrowser.Name = "btnAfiseazaBrowser"
        btnAfiseazaBrowser.Size = New System.Drawing.Size(52, 41)
        btnAfiseazaBrowser.TabIndex = 5
        btnAfiseazaBrowser.TextAlign = Drawing.ContentAlignment.MiddleLeft
        TT.SetToolTip(btnAfiseazaBrowser, "Afișeză fereastră browser - NERECOMANDAT!!!")
        btnAfiseazaBrowser.UseVisualStyleBackColor = True
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        TableLayoutPanel1.SetColumnSpan(lblStatus, 5)
        lblStatus.Dock = System.Windows.Forms.DockStyle.Top
        lblStatus.Font = New System.Drawing.Font("Segoe UI", 8.25F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        lblStatus.Location = New System.Drawing.Point(4, 0)
        lblStatus.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New System.Drawing.Size(619, 23)
        lblStatus.TabIndex = 3
        lblStatus.Text = "Inițializare..."
        ' 
        ' pbProgress
        ' 
        pbProgress.Dock = System.Windows.Forms.DockStyle.Fill
        pbProgress.Location = New System.Drawing.Point(3, 29)
        pbProgress.Name = "pbProgress"
        pbProgress.Size = New System.Drawing.Size(413, 35)
        pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        pbProgress.TabIndex = 2
        ' 
        ' btnAnulare
        ' 
        btnAnulare.Dock = System.Windows.Forms.DockStyle.Fill
        btnAnulare.FlatAppearance.BorderColor = Drawing.SystemColors.Control
        btnAnulare.FlatAppearance.MouseOverBackColor = Drawing.Color.FromArgb(CByte(255), CByte(224), CByte(192))
        btnAnulare.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnAnulare.Image = CType(resources.GetObject("btnAnulare.Image"), Drawing.Image)
        btnAnulare.Location = New System.Drawing.Point(419, 26)
        btnAnulare.Margin = New System.Windows.Forms.Padding(0)
        btnAnulare.Name = "btnAnulare"
        btnAnulare.Size = New System.Drawing.Size(52, 41)
        btnAnulare.TabIndex = 4
        btnAnulare.TextAlign = Drawing.ContentAlignment.MiddleLeft
        TT.SetToolTip(btnAnulare, "Oprește execuția fluxului curent")
        btnAnulare.UseVisualStyleBackColor = True
        ' 
        ' btnAfiseazaLog
        ' 
        btnAfiseazaLog.BackColor = Drawing.SystemColors.Control
        btnAfiseazaLog.Dock = System.Windows.Forms.DockStyle.Fill
        btnAfiseazaLog.FlatAppearance.BorderColor = Drawing.SystemColors.Control
        btnAfiseazaLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnAfiseazaLog.Font = New System.Drawing.Font("Segoe UI", 8.25F)
        btnAfiseazaLog.Image = My.Resources.Resources.Show_32
        btnAfiseazaLog.Location = New System.Drawing.Point(577, 26)
        btnAfiseazaLog.Margin = New System.Windows.Forms.Padding(2, 0, 0, 0)
        btnAfiseazaLog.Name = "btnAfiseazaLog"
        btnAfiseazaLog.Size = New System.Drawing.Size(50, 41)
        btnAfiseazaLog.TabIndex = 6
        btnAfiseazaLog.Tag = "ThemeToggle_nu"
        TT.SetToolTip(btnAfiseazaLog, "Arată istoricul acțiunilor")
        btnAfiseazaLog.UseVisualStyleBackColor = False
        ' 
        ' tlpLayout
        ' 
        tlpLayout.ColumnCount = 2
        tlpLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        tlpLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 218.0F))
        tlpLayout.Controls.Add(pnlProgress, 0, 0)
        tlpLayout.Controls.Add(lblTitleLog, 0, 1)
        tlpLayout.Controls.Add(rtbLog, 0, 2)
        tlpLayout.Controls.Add(chkAutoSalveazaResend, 1, 1)
        tlpLayout.Dock = System.Windows.Forms.DockStyle.Fill
        tlpLayout.Location = New System.Drawing.Point(0, 0)
        tlpLayout.Margin = New System.Windows.Forms.Padding(0)
        tlpLayout.Name = "tlpLayout"
        tlpLayout.Padding = New System.Windows.Forms.Padding(5)
        tlpLayout.RowCount = 3
        tlpLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 77.0F))
        tlpLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        tlpLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        tlpLayout.Size = New System.Drawing.Size(645, 440)
        tlpLayout.TabIndex = 0
        ' 
        ' chkAutoSalveazaResend
        ' 
        chkAutoSalveazaResend.AutoSize = True
        chkAutoSalveazaResend.Dock = System.Windows.Forms.DockStyle.Right
        chkAutoSalveazaResend.Location = New System.Drawing.Point(469, 85)
        chkAutoSalveazaResend.Name = "chkAutoSalveazaResend"
        chkAutoSalveazaResend.RightToLeft = System.Windows.Forms.RightToLeft.Yes
        chkAutoSalveazaResend.Size = New System.Drawing.Size(168, 29)
        chkAutoSalveazaResend.TabIndex = 5
        chkAutoSalveazaResend.Text = "Salvează rezultat"
        chkAutoSalveazaResend.UseVisualStyleBackColor = True
        ' 
        ' TT
        ' 
        TT.AutoPopDelay = 6000
        TT.InitialDelay = 500
        TT.IsBalloon = True
        TT.ReshowDelay = 100
        ' 
        ' KBOT_IPC
        ' 
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit
        ClientSize = New System.Drawing.Size(645, 440)
        Controls.Add(tlpLayout)
        DoubleBuffered = True
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        MaximizeBox = False
        Name = "KBOT_IPC"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "K-BOT - Pune botu' la treaba"
        TopMost = True
        pnlProgress.ResumeLayout(False)
        TableLayoutPanel1.ResumeLayout(False)
        TableLayoutPanel1.PerformLayout()
        tlpLayout.ResumeLayout(False)
        tlpLayout.PerformLayout()
        ResumeLayout(False)

    End Sub

    Friend WithEvents rtbLog As System.Windows.Forms.RichTextBox
    Friend WithEvents lblTitleLog As System.Windows.Forms.Label
    Friend WithEvents pnlProgress As System.Windows.Forms.Panel
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents pbProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents btnAnulare As System.Windows.Forms.Button
    Friend WithEvents tlpLayout As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents btnAfiseazaBrowser As System.Windows.Forms.Button
    Friend WithEvents chkAutoSalveazaResend As System.Windows.Forms.CheckBox
    Friend WithEvents btnAfiseazaLog As System.Windows.Forms.Button
    Friend WithEvents TT As System.Windows.Forms.ToolTip
End Class