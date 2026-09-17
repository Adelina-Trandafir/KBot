<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormFitHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    ' The bench of slice 0062: scheme buttons and the text-size slider (the two things that make
    ' a form grow), the base-of-the-window radios, the probe windows (where do they land, how big
    ' do they come up), a KBotTableLayoutPanel with authored-short rows and every one of its
    ' switches, and a journal of every measurement. Controls declared here (house rule).

    Friend WithEvents pnlTop As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnClassic As System.Windows.Forms.Button
    Friend WithEvents btnDark As System.Windows.Forms.Button
    Friend WithEvents btnModern As System.Windows.Forms.Button
    Friend WithEvents btnColorful As System.Windows.Forms.Button
    Friend WithEvents lblActive As System.Windows.Forms.Label

    Friend WithEvents pnlScale As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblScale As System.Windows.Forms.Label
    Friend WithEvents trkScale As System.Windows.Forms.TrackBar
    Friend WithEvents lblScaleValue As System.Windows.Forms.Label
    Friend WithEvents lblBaza As System.Windows.Forms.Label
    Friend WithEvents rdoScaled As System.Windows.Forms.RadioButton
    Friend WithEvents rdoRaw As System.Windows.Forms.RadioButton
    Friend WithEvents chkAutoFit As System.Windows.Forms.CheckBox
    Friend WithEvents btnRefit As System.Windows.Forms.Button

    Friend WithEvents pnlCenter As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnDialog As System.Windows.Forms.Button
    Friend WithEvents btnDialogParent As System.Windows.Forms.Button
    Friend WithEvents btnShell As System.Windows.Forms.Button
    Friend WithEvents btnSetRef As System.Windows.Forms.Button
    Friend WithEvents lblReference As System.Windows.Forms.Label

    Friend WithEvents pnlTableOptions As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents chkThemedBorder As System.Windows.Forms.CheckBox
    Friend WithEvents chkScaleStyles As System.Windows.Forms.CheckBox
    Friend WithEvents chkTableAutoFit As System.Windows.Forms.CheckBox
    Friend WithEvents btnCollapse As System.Windows.Forms.Button
    Friend WithEvents btnExpand As System.Windows.Forms.Button

    Friend WithEvents tblProbe As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblCauta As System.Windows.Forms.Label
    Friend WithEvents txtCauta As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblDeLa As System.Windows.Forms.Label
    Friend WithEvents txtDeLa As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblPanaLa As System.Windows.Forms.Label
    Friend WithEvents txtPanaLa As Global.KBot.Controls.KBotTextField
    Friend WithEvents btnReimprospateaza As System.Windows.Forms.Button
    Friend WithEvents lblBanda As System.Windows.Forms.Label
    Friend WithEvents chkBanda As System.Windows.Forms.CheckBox
    Friend WithEvents lblTableInfo As System.Windows.Forms.Label

    Friend WithEvents lstLog As System.Windows.Forms.ListBox

    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnFail As System.Windows.Forms.Button
    Friend WithEvents btnPass As System.Windows.Forms.Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlTop = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnClassic = New System.Windows.Forms.Button()
        Me.btnDark = New System.Windows.Forms.Button()
        Me.btnModern = New System.Windows.Forms.Button()
        Me.btnColorful = New System.Windows.Forms.Button()
        Me.lblActive = New System.Windows.Forms.Label()
        Me.pnlScale = New System.Windows.Forms.FlowLayoutPanel()
        Me.lblScale = New System.Windows.Forms.Label()
        Me.trkScale = New System.Windows.Forms.TrackBar()
        Me.lblScaleValue = New System.Windows.Forms.Label()
        Me.lblBaza = New System.Windows.Forms.Label()
        Me.rdoScaled = New System.Windows.Forms.RadioButton()
        Me.rdoRaw = New System.Windows.Forms.RadioButton()
        Me.chkAutoFit = New System.Windows.Forms.CheckBox()
        Me.btnRefit = New System.Windows.Forms.Button()
        Me.pnlCenter = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnDialog = New System.Windows.Forms.Button()
        Me.btnDialogParent = New System.Windows.Forms.Button()
        Me.btnShell = New System.Windows.Forms.Button()
        Me.btnSetRef = New System.Windows.Forms.Button()
        Me.lblReference = New System.Windows.Forms.Label()
        Me.pnlTableOptions = New System.Windows.Forms.FlowLayoutPanel()
        Me.chkThemedBorder = New System.Windows.Forms.CheckBox()
        Me.chkScaleStyles = New System.Windows.Forms.CheckBox()
        Me.chkTableAutoFit = New System.Windows.Forms.CheckBox()
        Me.btnCollapse = New System.Windows.Forms.Button()
        Me.btnExpand = New System.Windows.Forms.Button()
        Me.tblProbe = New Global.KBot.Controls.KBotTableLayoutPanel()
        Me.lblCauta = New System.Windows.Forms.Label()
        Me.txtCauta = New Global.KBot.Controls.KBotTextField()
        Me.lblDeLa = New System.Windows.Forms.Label()
        Me.txtDeLa = New Global.KBot.Controls.KBotTextField()
        Me.lblPanaLa = New System.Windows.Forms.Label()
        Me.txtPanaLa = New Global.KBot.Controls.KBotTextField()
        Me.btnReimprospateaza = New System.Windows.Forms.Button()
        Me.lblBanda = New System.Windows.Forms.Label()
        Me.chkBanda = New System.Windows.Forms.CheckBox()
        Me.lblTableInfo = New System.Windows.Forms.Label()
        Me.lstLog = New System.Windows.Forms.ListBox()
        Me.pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.btnPass = New System.Windows.Forms.Button()
        Me.pnlTop.SuspendLayout()
        Me.pnlScale.SuspendLayout()
        CType(Me.trkScale, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlCenter.SuspendLayout()
        Me.pnlTableOptions.SuspendLayout()
        Me.tblProbe.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlTop
        '
        Me.pnlTop.Controls.Add(Me.btnClassic)
        Me.pnlTop.Controls.Add(Me.btnDark)
        Me.pnlTop.Controls.Add(Me.btnModern)
        Me.pnlTop.Controls.Add(Me.btnColorful)
        Me.pnlTop.Controls.Add(Me.lblActive)
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Height = 44
        Me.pnlTop.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlTop.Name = "pnlTop"
        '
        'btnClassic
        '
        Me.btnClassic.AutoSize = True
        Me.btnClassic.Text = "Classic"
        Me.btnClassic.UseVisualStyleBackColor = True
        Me.btnClassic.Name = "btnClassic"
        '
        'btnDark
        '
        Me.btnDark.AutoSize = True
        Me.btnDark.Text = "Dark"
        Me.btnDark.UseVisualStyleBackColor = True
        Me.btnDark.Name = "btnDark"
        '
        'btnModern
        '
        Me.btnModern.AutoSize = True
        Me.btnModern.Text = "Modern"
        Me.btnModern.UseVisualStyleBackColor = True
        Me.btnModern.Name = "btnModern"
        '
        'btnColorful
        '
        Me.btnColorful.AutoSize = True
        Me.btnColorful.Text = "Colorat"
        Me.btnColorful.UseVisualStyleBackColor = True
        Me.btnColorful.Name = "btnColorful"
        '
        'lblActive
        '
        Me.lblActive.AutoSize = True
        Me.lblActive.Margin = New System.Windows.Forms.Padding(12, 9, 3, 0)
        Me.lblActive.Text = "activ: —"
        Me.lblActive.Name = "lblActive"
        '
        'pnlScale
        '
        Me.pnlScale.Controls.Add(Me.lblScale)
        Me.pnlScale.Controls.Add(Me.trkScale)
        Me.pnlScale.Controls.Add(Me.lblScaleValue)
        Me.pnlScale.Controls.Add(Me.lblBaza)
        Me.pnlScale.Controls.Add(Me.rdoScaled)
        Me.pnlScale.Controls.Add(Me.rdoRaw)
        Me.pnlScale.Controls.Add(Me.chkAutoFit)
        Me.pnlScale.Controls.Add(Me.btnRefit)
        Me.pnlScale.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlScale.Height = 46
        Me.pnlScale.Padding = New System.Windows.Forms.Padding(6, 4, 6, 4)
        Me.pnlScale.Name = "pnlScale"
        '
        'lblScale
        '
        Me.lblScale.AutoSize = True
        Me.lblScale.Margin = New System.Windows.Forms.Padding(3, 9, 3, 0)
        Me.lblScale.Text = "Mărime text:"
        Me.lblScale.Name = "lblScale"
        '
        'trkScale
        '
        Me.trkScale.AutoSize = False
        Me.trkScale.LargeChange = 25
        Me.trkScale.Maximum = 200
        Me.trkScale.Minimum = 75
        Me.trkScale.Size = New System.Drawing.Size(220, 30)
        Me.trkScale.SmallChange = 5
        Me.trkScale.TickFrequency = 25
        Me.trkScale.TickStyle = System.Windows.Forms.TickStyle.BottomRight
        Me.trkScale.Value = 100
        Me.trkScale.Name = "trkScale"
        '
        'lblScaleValue
        '
        Me.lblScaleValue.AutoSize = True
        Me.lblScaleValue.Margin = New System.Windows.Forms.Padding(3, 9, 12, 0)
        Me.lblScaleValue.Text = "100%"
        Me.lblScaleValue.Name = "lblScaleValue"
        '
        'lblBaza
        '
        Me.lblBaza.AutoSize = True
        Me.lblBaza.Margin = New System.Windows.Forms.Padding(3, 9, 3, 0)
        Me.lblBaza.Text = "Baza ferestrei:"
        Me.lblBaza.Name = "lblBaza"
        '
        'rdoScaled
        '
        Me.rdoScaled.AutoSize = True
        Me.rdoScaled.Checked = True
        Me.rdoScaled.Margin = New System.Windows.Forms.Padding(3, 6, 3, 3)
        Me.rdoScaled.TabStop = True
        Me.rdoScaled.Text = "urmează scalarea"
        Me.rdoScaled.UseVisualStyleBackColor = True
        Me.rdoScaled.Name = "rdoScaled"
        '
        'rdoRaw
        '
        Me.rdoRaw.AutoSize = True
        Me.rdoRaw.Margin = New System.Windows.Forms.Padding(3, 6, 12, 3)
        Me.rdoRaw.Text = "pixeli bruți"
        Me.rdoRaw.UseVisualStyleBackColor = True
        Me.rdoRaw.Name = "rdoRaw"
        '
        'chkAutoFit
        '
        Me.chkAutoFit.AutoSize = True
        Me.chkAutoFit.Checked = True
        Me.chkAutoFit.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAutoFit.Margin = New System.Windows.Forms.Padding(3, 6, 12, 3)
        Me.chkAutoFit.Text = "fereastra crește la temă"
        Me.chkAutoFit.UseVisualStyleBackColor = True
        Me.chkAutoFit.Name = "chkAutoFit"
        '
        'btnRefit
        '
        Me.btnRefit.AutoSize = True
        Me.btnRefit.Text = "Repotrivește acum"
        Me.btnRefit.UseVisualStyleBackColor = True
        Me.btnRefit.Name = "btnRefit"
        '
        'pnlCenter
        '
        Me.pnlCenter.Controls.Add(Me.btnDialog)
        Me.pnlCenter.Controls.Add(Me.btnDialogParent)
        Me.pnlCenter.Controls.Add(Me.btnShell)
        Me.pnlCenter.Controls.Add(Me.btnSetRef)
        Me.pnlCenter.Controls.Add(Me.lblReference)
        Me.pnlCenter.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlCenter.Height = 44
        Me.pnlCenter.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlCenter.Name = "pnlCenter"
        '
        'btnDialog
        '
        Me.btnDialog.AutoSize = True
        Me.btnDialog.Text = "Dialog cu chenar (CenterScreen)"
        Me.btnDialog.UseVisualStyleBackColor = True
        Me.btnDialog.Name = "btnDialog"
        '
        'btnDialogParent
        '
        Me.btnDialogParent.AutoSize = True
        Me.btnDialogParent.Text = "Dialog (CenterParent, fără owner)"
        Me.btnDialogParent.UseVisualStyleBackColor = True
        Me.btnDialogParent.Name = "btnDialogParent"
        '
        'btnShell
        '
        Me.btnShell.AutoSize = True
        Me.btnShell.Text = "Shell fără chenar"
        Me.btnShell.UseVisualStyleBackColor = True
        Me.btnShell.Name = "btnShell"
        '
        'btnSetRef
        '
        Me.btnSetRef.AutoSize = True
        Me.btnSetRef.Text = "Fereastra asta = referința"
        Me.btnSetRef.UseVisualStyleBackColor = True
        Me.btnSetRef.Name = "btnSetRef"
        '
        'lblReference
        '
        Me.lblReference.AutoSize = True
        Me.lblReference.Margin = New System.Windows.Forms.Padding(12, 9, 3, 0)
        Me.lblReference.Text = "referință: —"
        Me.lblReference.Name = "lblReference"
        '
        'pnlTableOptions
        '
        Me.pnlTableOptions.Controls.Add(Me.chkThemedBorder)
        Me.pnlTableOptions.Controls.Add(Me.chkScaleStyles)
        Me.pnlTableOptions.Controls.Add(Me.chkTableAutoFit)
        Me.pnlTableOptions.Controls.Add(Me.btnCollapse)
        Me.pnlTableOptions.Controls.Add(Me.btnExpand)
        Me.pnlTableOptions.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTableOptions.Height = 40
        Me.pnlTableOptions.Padding = New System.Windows.Forms.Padding(6, 4, 6, 4)
        Me.pnlTableOptions.Name = "pnlTableOptions"
        '
        'chkThemedBorder
        '
        Me.chkThemedBorder.AutoSize = True
        Me.chkThemedBorder.Checked = True
        Me.chkThemedBorder.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkThemedBorder.Margin = New System.Windows.Forms.Padding(3, 6, 12, 3)
        Me.chkThemedBorder.Text = "linii de celulă din temă"
        Me.chkThemedBorder.UseVisualStyleBackColor = True
        Me.chkThemedBorder.Name = "chkThemedBorder"
        '
        'chkScaleStyles
        '
        Me.chkScaleStyles.AutoSize = True
        Me.chkScaleStyles.Checked = True
        Me.chkScaleStyles.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkScaleStyles.Margin = New System.Windows.Forms.Padding(3, 6, 12, 3)
        Me.chkScaleStyles.Text = "coloanele fixe urmează scara K-BOT"
        Me.chkScaleStyles.UseVisualStyleBackColor = True
        Me.chkScaleStyles.Name = "chkScaleStyles"
        '
        'chkTableAutoFit
        '
        Me.chkTableAutoFit.AutoSize = True
        Me.chkTableAutoFit.Checked = True
        Me.chkTableAutoFit.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkTableAutoFit.Margin = New System.Windows.Forms.Padding(3, 6, 12, 3)
        Me.chkTableAutoFit.Text = "rândurile fixe cresc la conținut"
        Me.chkTableAutoFit.UseVisualStyleBackColor = True
        Me.chkTableAutoFit.Name = "chkTableAutoFit"
        '
        'btnCollapse
        '
        Me.btnCollapse.AutoSize = True
        Me.btnCollapse.Text = "Strânge banda (+ResetStyleBaseline)"
        Me.btnCollapse.UseVisualStyleBackColor = True
        Me.btnCollapse.Name = "btnCollapse"
        '
        'btnExpand
        '
        Me.btnExpand.AutoSize = True
        Me.btnExpand.Text = "Desface banda"
        Me.btnExpand.UseVisualStyleBackColor = True
        Me.btnExpand.Name = "btnExpand"
        '
        'tblProbe
        '
        ' The LogViewerForm filter row, authored SHORT (rows of 32px for 9pt controls): the worst
        ' case in the solution, and the case where the growth under Modern must be visible.
        Me.tblProbe.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single
        Me.tblProbe.ColumnCount = 10
        Me.tblProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80.0F))
        Me.tblProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200.0F))
        Me.tblProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8.0F))
        Me.tblProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80.0F))
        Me.tblProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120.0F))
        Me.tblProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8.0F))
        Me.tblProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80.0F))
        Me.tblProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120.0F))
        Me.tblProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        Me.tblProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120.0F))
        Me.tblProbe.Controls.Add(Me.lblCauta, 0, 0)
        Me.tblProbe.Controls.Add(Me.txtCauta, 1, 0)
        Me.tblProbe.Controls.Add(Me.lblDeLa, 3, 0)
        Me.tblProbe.Controls.Add(Me.txtDeLa, 4, 0)
        Me.tblProbe.Controls.Add(Me.lblPanaLa, 6, 0)
        Me.tblProbe.Controls.Add(Me.txtPanaLa, 7, 0)
        Me.tblProbe.Controls.Add(Me.btnReimprospateaza, 9, 0)
        Me.tblProbe.Controls.Add(Me.lblBanda, 0, 1)
        Me.tblProbe.Controls.Add(Me.chkBanda, 3, 1)
        Me.tblProbe.Controls.Add(Me.lblTableInfo, 0, 2)
        Me.tblProbe.Dock = System.Windows.Forms.DockStyle.Top
        Me.tblProbe.Height = 132
        Me.tblProbe.Padding = New System.Windows.Forms.Padding(6, 0, 6, 0)
        Me.tblProbe.RowCount = 3
        Me.tblProbe.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32.0F))
        Me.tblProbe.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32.0F))
        Me.tblProbe.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        Me.tblProbe.SetColumnSpan(Me.lblBanda, 3)
        Me.tblProbe.SetColumnSpan(Me.lblTableInfo, 10)
        Me.tblProbe.ThemedCellBorder = True
        Me.tblProbe.Name = "tblProbe"
        '
        'lblCauta
        '
        Me.lblCauta.AutoSize = True
        Me.lblCauta.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblCauta.Text = "Caută:"
        Me.lblCauta.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblCauta.Name = "lblCauta"
        '
        'txtCauta
        '
        Me.txtCauta.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtCauta.Margin = New System.Windows.Forms.Padding(0)
        Me.txtCauta.PlaceholderText = "text din linie"
        Me.txtCauta.Name = "txtCauta"
        '
        'lblDeLa
        '
        Me.lblDeLa.AutoSize = True
        Me.lblDeLa.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblDeLa.Text = "De la:"
        Me.lblDeLa.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblDeLa.Name = "lblDeLa"
        '
        'txtDeLa
        '
        Me.txtDeLa.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtDeLa.Margin = New System.Windows.Forms.Padding(0)
        Me.txtDeLa.PlaceholderText = "zz.ll.aaaa"
        Me.txtDeLa.Name = "txtDeLa"
        '
        'lblPanaLa
        '
        Me.lblPanaLa.AutoSize = True
        Me.lblPanaLa.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblPanaLa.Text = "Până la:"
        Me.lblPanaLa.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblPanaLa.Name = "lblPanaLa"
        '
        'txtPanaLa
        '
        Me.txtPanaLa.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtPanaLa.Margin = New System.Windows.Forms.Padding(0)
        Me.txtPanaLa.PlaceholderText = "zz.ll.aaaa"
        Me.txtPanaLa.Name = "txtPanaLa"
        '
        'btnReimprospateaza
        '
        Me.btnReimprospateaza.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnReimprospateaza.Margin = New System.Windows.Forms.Padding(0)
        Me.btnReimprospateaza.Text = "Reîmprospătează"
        Me.btnReimprospateaza.UseVisualStyleBackColor = True
        Me.btnReimprospateaza.Name = "btnReimprospateaza"
        '
        'lblBanda
        '
        Me.lblBanda.AutoSize = True
        Me.lblBanda.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblBanda.Text = "Grupează pe clasificații (banda care se strânge la 0)"
        Me.lblBanda.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblBanda.Name = "lblBanda"
        '
        'chkBanda
        '
        Me.chkBanda.AutoSize = True
        Me.chkBanda.Dock = System.Windows.Forms.DockStyle.Fill
        Me.chkBanda.Text = "bifă"
        Me.chkBanda.UseVisualStyleBackColor = True
        Me.chkBanda.Name = "chkBanda"
        '
        'lblTableInfo
        '
        Me.lblTableInfo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblTableInfo.Text = "coloane: —"
        Me.lblTableInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblTableInfo.Name = "lblTableInfo"
        '
        'lstLog
        '
        Me.lstLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstLog.HorizontalScrollbar = True
        Me.lstLog.IntegralHeight = False
        Me.lstLog.Name = "lstLog"
        '
        'pnlButtons
        '
        Me.pnlButtons.Controls.Add(Me.btnFail)
        Me.pnlButtons.Controls.Add(Me.btnPass)
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlButtons.Height = 44
        Me.pnlButtons.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlButtons.Name = "pnlButtons"
        '
        'btnFail
        '
        Me.btnFail.AutoSize = True
        Me.btnFail.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnFail.Text = "Fail"
        Me.btnFail.UseVisualStyleBackColor = True
        Me.btnFail.Name = "btnFail"
        '
        'btnPass
        '
        Me.btnPass.AutoSize = True
        Me.btnPass.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnPass.Text = "Pass"
        Me.btnPass.UseVisualStyleBackColor = True
        Me.btnPass.Name = "btnPass"
        '
        'FormFitHarnessForm
        '
        Me.CancelButton = Me.btnFail
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0F, 15.0F)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1180, 640)
        ' Reverse dock order (house rule): Fill first, then Bottom, then the Top bands from the
        ' lowest to the highest -- the last Top added ends up on top.
        Me.Controls.Add(Me.lstLog)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.tblProbe)
        Me.Controls.Add(Me.pnlTableOptions)
        Me.Controls.Add(Me.pnlCenter)
        Me.Controls.Add(Me.pnlScale)
        Me.Controls.Add(Me.pnlTop)
        Me.MinimumSize = New System.Drawing.Size(900, 500)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Potrivirea ferestrei la temă — 0062 (banc de probă)"
        Me.Name = "FormFitHarnessForm"
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        Me.pnlScale.ResumeLayout(False)
        Me.pnlScale.PerformLayout()
        CType(Me.trkScale, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlCenter.ResumeLayout(False)
        Me.pnlCenter.PerformLayout()
        Me.pnlTableOptions.ResumeLayout(False)
        Me.pnlTableOptions.PerformLayout()
        Me.tblProbe.ResumeLayout(False)
        Me.tblProbe.PerformLayout()
        Me.pnlButtons.ResumeLayout(False)
        Me.pnlButtons.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

End Class
