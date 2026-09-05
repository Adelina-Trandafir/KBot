' Bench for OfficeDocumentHost: pick a spreadsheet or a document, watch it embed into pnlHost, and
' close it again while a live count of EXCEL.EXE / WINWORD.EXE says whether anything was left behind.
'
' WHY THE BENCH STOPS AT THE HOST. The pane the operator actually sees -- `DdfFisierPreview` and the
' «Fisiere» page around it -- lives in KBot.App, and KBot.DevHarness CANNOT reference KBot.App (that
' project references this one on Debug, so the reference would be circular). What is testable from
' here is the piece that carries all the risk anyway: starting a private Office instance, taking its
' chrome down, reparenting its window, and letting go of it deterministically.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OfficePreviewHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    Friend WithEvents pnlBara As FlowLayoutPanel
    Friend WithEvents btnExcel As Button
    Friend WithEvents btnWord As Button
    Friend WithEvents btnInchide As Button
    Friend WithEvents chkAscundeBara As CheckBox
    Friend WithEvents lblProcese As Label

    Friend WithEvents splRoot As SplitContainer
    Friend WithEvents pnlHost As Panel
    Friend WithEvents txtJurnal As TextBox

    Friend WithEvents pnlVerdict As FlowLayoutPanel
    Friend WithEvents btnPass As Button
    Friend WithEvents btnFail As Button

    Friend WithEvents dlgAlege As OpenFileDialog
    Friend WithEvents tmrProcese As Timer

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            ' The hosted window goes BEFORE the form is destroyed, or the Office process outlives
            ' the panel its window was a child of.
            If disposing Then InchideGazda()
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        pnlBara = New FlowLayoutPanel()
        btnExcel = New Button()
        btnWord = New Button()
        btnInchide = New Button()
        chkAscundeBara = New CheckBox()
        lblProcese = New Label()
        splRoot = New SplitContainer()
        pnlHost = New Panel()
        txtJurnal = New TextBox()
        pnlVerdict = New FlowLayoutPanel()
        btnPass = New Button()
        btnFail = New Button()
        dlgAlege = New OpenFileDialog()
        tmrProcese = New Timer(components)
        pnlBara.SuspendLayout()
        CType(splRoot, ComponentModel.ISupportInitialize).BeginInit()
        splRoot.Panel1.SuspendLayout()
        splRoot.Panel2.SuspendLayout()
        splRoot.SuspendLayout()
        pnlVerdict.SuspendLayout()
        SuspendLayout()
        '
        ' pnlBara
        '
        pnlBara.Controls.Add(btnExcel)
        pnlBara.Controls.Add(btnWord)
        pnlBara.Controls.Add(btnInchide)
        pnlBara.Controls.Add(chkAscundeBara)
        pnlBara.Controls.Add(lblProcese)
        pnlBara.Dock = DockStyle.Top
        pnlBara.Height = 48
        pnlBara.Name = "pnlBara"
        pnlBara.Padding = New Padding(8, 8, 8, 8)
        pnlBara.TabIndex = 0
        '
        ' btnExcel
        '
        btnExcel.AutoSize = True
        btnExcel.Name = "btnExcel"
        btnExcel.Padding = New Padding(10, 2, 10, 2)
        btnExcel.TabIndex = 0
        btnExcel.Text = "Deschide tabel (Excel)…"
        btnExcel.UseVisualStyleBackColor = True
        '
        ' btnWord
        '
        btnWord.AutoSize = True
        btnWord.Name = "btnWord"
        btnWord.Padding = New Padding(10, 2, 10, 2)
        btnWord.TabIndex = 1
        btnWord.Text = "Deschide document (Word)…"
        btnWord.UseVisualStyleBackColor = True
        '
        ' btnInchide
        '
        btnInchide.AutoSize = True
        btnInchide.Name = "btnInchide"
        btnInchide.Padding = New Padding(10, 2, 10, 2)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide containerul"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' chkAscundeBara -- which of the two Excel ribbon methods the next open uses. Read when the
        ' file is chosen, so ticking it changes the NEXT document, not the one on screen.
        '
        chkAscundeBara.AutoSize = True
        chkAscundeBara.Margin = New Padding(16, 10, 3, 0)
        chkAscundeBara.Name = "chkAscundeBara"
        chkAscundeBara.TabIndex = 3
        chkAscundeBara.Text = "Excel: ascunde fereastra panglicii (ca la Word)"
        chkAscundeBara.UseVisualStyleBackColor = True
        '
        ' lblProcese -- the whole point of the bench: does anything survive the close?
        '
        lblProcese.AutoSize = True
        lblProcese.Margin = New Padding(16, 8, 3, 0)
        lblProcese.Name = "lblProcese"
        lblProcese.TabIndex = 4
        lblProcese.Text = "EXCEL/WINWORD: —"
        '
        ' splRoot -- host on the left, working log on the right
        '
        splRoot.Dock = DockStyle.Fill
        splRoot.Name = "splRoot"
        splRoot.Panel1.Controls.Add(pnlHost)
        splRoot.Panel2.Controls.Add(txtJurnal)
        splRoot.SplitterDistance = 700
        splRoot.TabIndex = 1
        '
        ' pnlHost -- the panel the foreign window is reparented into
        '
        pnlHost.Dock = DockStyle.Fill
        pnlHost.Name = "pnlHost"
        pnlHost.TabIndex = 0
        '
        ' txtJurnal
        '
        txtJurnal.Dock = DockStyle.Fill
        txtJurnal.Multiline = True
        txtJurnal.Name = "txtJurnal"
        txtJurnal.ReadOnly = True
        txtJurnal.ScrollBars = ScrollBars.Both
        txtJurnal.TabIndex = 0
        txtJurnal.WordWrap = False
        '
        ' pnlVerdict
        '
        pnlVerdict.Controls.Add(btnPass)
        pnlVerdict.Controls.Add(btnFail)
        pnlVerdict.Dock = DockStyle.Bottom
        pnlVerdict.FlowDirection = FlowDirection.RightToLeft
        pnlVerdict.Height = 48
        pnlVerdict.Name = "pnlVerdict"
        pnlVerdict.Padding = New Padding(8, 8, 8, 8)
        pnlVerdict.TabIndex = 2
        '
        ' btnPass
        '
        btnPass.AutoSize = True
        btnPass.DialogResult = DialogResult.OK
        btnPass.Name = "btnPass"
        btnPass.Padding = New Padding(14, 2, 14, 2)
        btnPass.TabIndex = 0
        btnPass.Text = "Merge"
        btnPass.UseVisualStyleBackColor = True
        '
        ' btnFail
        '
        btnFail.AutoSize = True
        btnFail.DialogResult = DialogResult.Cancel
        btnFail.Name = "btnFail"
        btnFail.Padding = New Padding(14, 2, 14, 2)
        btnFail.TabIndex = 1
        btnFail.Text = "Nu merge"
        btnFail.UseVisualStyleBackColor = True
        '
        ' dlgAlege
        '
        dlgAlege.Title = "Alege fișierul de previzualizat"
        '
        ' tmrProcese
        '
        tmrProcese.Interval = 1000
        '
        ' OfficePreviewHarnessForm
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1100, 650)
        ' Children in REVERSE dock order: Fill first, then the docked edges.
        Controls.Add(splRoot)
        Controls.Add(pnlVerdict)
        Controls.Add(pnlBara)
        Name = "OfficePreviewHarnessForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Banc — previzualizare Office (OfficeDocumentHost)"
        pnlBara.ResumeLayout(False)
        pnlBara.PerformLayout()
        splRoot.Panel1.ResumeLayout(False)
        splRoot.Panel2.ResumeLayout(False)
        splRoot.Panel2.PerformLayout()
        CType(splRoot, ComponentModel.ISupportInitialize).EndInit()
        splRoot.ResumeLayout(False)
        pnlVerdict.ResumeLayout(False)
        pnlVerdict.PerformLayout()
        ResumeLayout(False)
    End Sub

End Class
