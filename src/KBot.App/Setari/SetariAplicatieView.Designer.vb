Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariAplicatieView
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
        cboVerbose = New KBotComboBox()
        chkLogViewer = New CheckBox()
        chkShowBrowser = New CheckBox()
        chkReceptii = New CheckBox()
        cboAdobeMotor = New KBotComboBox()
        btnAdobeGazduire = New Button()
        cboExcelRibbon = New KBotComboBox()
        tlyBody = New KBotTableLayoutPanel()
        lblTitluComutatoare = New Label()
        tlyComutatoare = New KBotTableLayoutPanel()
        lblVerbose = New Label()
        lblTitluDocumente = New Label()
        tlyDocumente = New KBotTableLayoutPanel()
        lblAdobeMotor = New Label()
        lblExcelRibbon = New Label()
        tlyBody.SuspendLayout()
        tlyComutatoare.SuspendLayout()
        tlyDocumente.SuspendLayout()
        SuspendLayout()
        ' 
        ' cboVerbose
        ' 
        cboVerbose.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboVerbose.CornerRadius = 4
        cboVerbose.DrawMode = DrawMode.OwnerDrawFixed
        cboVerbose.DropDownStyle = ComboBoxStyle.DropDownList
        cboVerbose.FlatStyle = FlatStyle.Flat
        cboVerbose.ItemHeight = 31
        cboVerbose.Location = New Point(404, 0)
        cboVerbose.Margin = New Padding(4, 0, 4, 10)
        cboVerbose.Name = "cboVerbose"
        cboVerbose.Size = New Size(496, 37)
        cboVerbose.TabIndex = 1
        tips.SetToolTipHeader(cboVerbose, "Cât scrie consola FOREXE")
        tips.SetToolTipText(cboVerbose, "«Implicit» = pornit pe Debug, oprit pe Release (felia 0071)." & vbLf & "Pornit arată pașii, așteptările, andocarea și stivele; oprit arată doar <Log> și erorile." & vbLf & "Fișierul-jurnal primește oricum tot.")
        ' 
        ' chkLogViewer
        ' 
        chkLogViewer.AutoSize = True
        tlyComutatoare.SetColumnSpan(chkLogViewer, 2)
        chkLogViewer.Location = New Point(4, 47)
        chkLogViewer.Margin = New Padding(4, 0, 4, 10)
        chkLogViewer.Name = "chkLogViewer"
        chkLogViewer.Size = New Size(498, 26)
        chkLogViewer.TabIndex = 2
        chkLogViewer.Text = "Rândul «Arată jurnal» în meniul de opțiuni al ferestrei principale"
        tips.SetToolTipHeader(chkLogViewer, "Vizualizatorul de jurnale")
        tips.SetToolTipText(chkLogViewer, "Debifat, rândul dispare din meniul butonului de opțiuni." & vbLf & "Jurnalele se scriu în continuare pe disc.")
        chkLogViewer.UseVisualStyleBackColor = True
        ' 
        ' chkShowBrowser
        ' 
        chkShowBrowser.AutoSize = True
        tlyComutatoare.SetColumnSpan(chkShowBrowser, 2)
        chkShowBrowser.Location = New Point(4, 83)
        chkShowBrowser.Margin = New Padding(4, 0, 4, 10)
        chkShowBrowser.Name = "chkShowBrowser"
        chkShowBrowser.Size = New Size(359, 26)
        chkShowBrowser.TabIndex = 3
        chkShowBrowser.Text = "Butonul «Arată browserul» în banda FOREXE"
        tips.SetToolTipHeader(chkShowBrowser, "Butonul de browser")
        tips.SetToolTipText(chkShowBrowser, "Debifat, operatorul nu mai poate deschide vizualizatorul browserului FOREXE." & vbLf & "Robotul rulează la fel; doar butonul dispare.")
        chkShowBrowser.UseVisualStyleBackColor = True
        ' 
        ' chkReceptii
        ' 
        chkReceptii.AutoSize = True
        tlyComutatoare.SetColumnSpan(chkReceptii, 2)
        chkReceptii.Location = New Point(4, 119)
        chkReceptii.Margin = New Padding(4, 0, 4, 10)
        chkReceptii.Name = "chkReceptii"
        chkReceptii.Size = New Size(462, 26)
        chkReceptii.TabIndex = 4
        chkReceptii.Text = "Selectorul de recepții se deschide cu toate recepțiile bifate"
        tips.SetToolTipHeader(chkReceptii, "Recepțiile de descărcat")
        tips.SetToolTipText(chkReceptii, "Bifat: apăsarea obișnuită aduce tot; debifat: nimic până nu alegi." & vbLf & "Cerut configurabil de operator la 10.09.2026.")
        chkReceptii.UseVisualStyleBackColor = True
        ' 
        ' cboAdobeMotor
        ' 
        cboAdobeMotor.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboAdobeMotor.CornerRadius = 4
        cboAdobeMotor.DrawMode = DrawMode.OwnerDrawFixed
        cboAdobeMotor.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeMotor.FlatStyle = FlatStyle.Flat
        cboAdobeMotor.ItemHeight = 31
        cboAdobeMotor.Location = New Point(404, 0)
        cboAdobeMotor.Margin = New Padding(4, 0, 4, 10)
        cboAdobeMotor.Name = "cboAdobeMotor"
        cboAdobeMotor.Size = New Size(496, 37)
        cboAdobeMotor.TabIndex = 1
        tips.SetToolTipHeader(cboAdobeMotor, "Motorul de previzualizare PDF")
        tips.SetToolTipText(cboAdobeMotor, "«Fereastră găzduită» = fereastra Adobe mutată în panoul K-BOT (singura rulată în aplicație)." & vbLf & "«ActiveX» = controlul AcroPDF, în proces.")
        ' 
        ' btnAdobeGazduire
        ' 
        btnAdobeGazduire.AutoSize = True
        btnAdobeGazduire.Dock = DockStyle.Left
        btnAdobeGazduire.FlatStyle = FlatStyle.Flat
        btnAdobeGazduire.Location = New Point(404, 47)
        btnAdobeGazduire.Margin = New Padding(4, 0, 4, 10)
        btnAdobeGazduire.Name = "btnAdobeGazduire"
        btnAdobeGazduire.Padding = New Padding(12, 4, 12, 4)
        btnAdobeGazduire.Size = New Size(320, 45)
        btnAdobeGazduire.TabIndex = 2
        btnAdobeGazduire.Text = "Opțiuni fereastră găzduită…"
        tips.SetToolTipHeader(btnAdobeGazduire, "Fereastra găzduită Adobe")
        tips.SetToolTipText(btnAdobeGazduire, "Modul vizualizatorului, comutatorul /n, eliberarea ferestrei și fereastra plutitoare." & vbLf & "Se deschide singur când alegi «Fereastră găzduită»; de aici le poți revedea oricând.")
        btnAdobeGazduire.UseVisualStyleBackColor = True
        ' 
        ' cboExcelRibbon
        ' 
        cboExcelRibbon.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboExcelRibbon.CornerRadius = 4
        cboExcelRibbon.DrawMode = DrawMode.OwnerDrawFixed
        cboExcelRibbon.DropDownStyle = ComboBoxStyle.DropDownList
        cboExcelRibbon.FlatStyle = FlatStyle.Flat
        cboExcelRibbon.ItemHeight = 31
        cboExcelRibbon.Location = New Point(404, 102)
        cboExcelRibbon.Margin = New Padding(4, 0, 4, 10)
        cboExcelRibbon.Name = "cboExcelRibbon"
        cboExcelRibbon.Size = New Size(496, 37)
        cboExcelRibbon.TabIndex = 4
        tips.SetToolTipHeader(cboExcelRibbon, "Panglica Excel în previzualizare")
        tips.SetToolTipText(cboExcelRibbon, "Macro: Excel își ascunde singur panglica (poate fi refuzat de o politică)." & vbLf & "Fereastră: se ascunde fereastra panglicii, ca la Word — nu poate fi refuzat." & vbLf & "Word are o singură metodă și nu se configurează.")
        ' 
        ' tlyBody
        ' 
        tlyBody.AutoScroll = True
        tlyBody.ColumnCount = 1
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBody.Controls.Add(lblTitluComutatoare, 0, 0)
        tlyBody.Controls.Add(tlyComutatoare, 0, 1)
        tlyBody.Controls.Add(lblTitluDocumente, 0, 2)
        tlyBody.Controls.Add(tlyDocumente, 0, 3)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(0, 0)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.Padding = New Padding(24, 18, 24, 18)
        tlyBody.RowCount = 4
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tlyBody.Size = New Size(960, 457)
        tlyBody.TabIndex = 0
        ' 
        ' lblTitluComutatoare
        ' 
        lblTitluComutatoare.AutoSize = True
        lblTitluComutatoare.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluComutatoare.Location = New Point(28, 18)
        lblTitluComutatoare.Margin = New Padding(4, 0, 4, 8)
        lblTitluComutatoare.Name = "lblTitluComutatoare"
        lblTitluComutatoare.Size = New Size(158, 32)
        lblTitluComutatoare.TabIndex = 0
        lblTitluComutatoare.Text = "Comutatoare"
        ' 
        ' tlyComutatoare
        ' 
        tlyComutatoare.AutoFitToTheme = False
        tlyComutatoare.AutoSize = True
        tlyComutatoare.ColumnCount = 2
        tlyComutatoare.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 400F))
        tlyComutatoare.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyComutatoare.Controls.Add(lblVerbose, 0, 0)
        tlyComutatoare.Controls.Add(cboVerbose, 1, 0)
        tlyComutatoare.Controls.Add(chkLogViewer, 0, 1)
        tlyComutatoare.Controls.Add(chkShowBrowser, 0, 2)
        tlyComutatoare.Controls.Add(chkReceptii, 0, 3)
        tlyComutatoare.Dock = DockStyle.Top
        tlyComutatoare.Location = New Point(28, 58)
        tlyComutatoare.Margin = New Padding(4, 0, 4, 24)
        tlyComutatoare.Name = "tlyComutatoare"
        tlyComutatoare.RowCount = 4
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.RowStyles.Add(New RowStyle())
        tlyComutatoare.Size = New Size(904, 155)
        tlyComutatoare.TabIndex = 1
        ' 
        ' lblVerbose
        ' 
        lblVerbose.AutoSize = True
        lblVerbose.Dock = DockStyle.Fill
        lblVerbose.Location = New Point(4, 0)
        lblVerbose.Margin = New Padding(4, 0, 4, 10)
        lblVerbose.Name = "lblVerbose"
        lblVerbose.Size = New Size(392, 37)
        lblVerbose.TabIndex = 0
        lblVerbose.Text = "Consola FOREXE detaliată (VerboseLogging)"
        lblVerbose.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblTitluDocumente
        ' 
        lblTitluDocumente.AutoSize = True
        lblTitluDocumente.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluDocumente.Location = New Point(28, 237)
        lblTitluDocumente.Margin = New Padding(4, 0, 4, 8)
        lblTitluDocumente.Name = "lblTitluDocumente"
        lblTitluDocumente.Size = New Size(343, 32)
        lblTitluDocumente.TabIndex = 2
        lblTitluDocumente.Text = "Documente (PDF, Word, Excel)"
        ' 
        ' tlyDocumente
        ' 
        tlyDocumente.AutoFitToTheme = False
        tlyDocumente.AutoSize = True
        tlyDocumente.ColumnCount = 2
        tlyDocumente.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 400F))
        tlyDocumente.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyDocumente.Controls.Add(lblAdobeMotor, 0, 0)
        tlyDocumente.Controls.Add(cboAdobeMotor, 1, 0)
        tlyDocumente.Controls.Add(btnAdobeGazduire, 1, 1)
        tlyDocumente.Controls.Add(lblExcelRibbon, 0, 2)
        tlyDocumente.Controls.Add(cboExcelRibbon, 1, 2)
        tlyDocumente.Dock = DockStyle.Top
        tlyDocumente.Location = New Point(28, 277)
        tlyDocumente.Margin = New Padding(4, 0, 4, 24)
        tlyDocumente.Name = "tlyDocumente"
        tlyDocumente.RowCount = 3
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.RowStyles.Add(New RowStyle())
        tlyDocumente.Size = New Size(904, 149)
        tlyDocumente.TabIndex = 3
        ' 
        ' lblAdobeMotor
        ' 
        lblAdobeMotor.AutoSize = True
        lblAdobeMotor.Dock = DockStyle.Fill
        lblAdobeMotor.Location = New Point(4, 0)
        lblAdobeMotor.Margin = New Padding(4, 0, 4, 10)
        lblAdobeMotor.Name = "lblAdobeMotor"
        lblAdobeMotor.Size = New Size(392, 37)
        lblAdobeMotor.TabIndex = 0
        lblAdobeMotor.Text = "PDF — motor de previzualizare"
        lblAdobeMotor.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblExcelRibbon
        ' 
        lblExcelRibbon.AutoSize = True
        lblExcelRibbon.Dock = DockStyle.Fill
        lblExcelRibbon.Location = New Point(4, 102)
        lblExcelRibbon.Margin = New Padding(4, 0, 4, 10)
        lblExcelRibbon.Name = "lblExcelRibbon"
        lblExcelRibbon.Size = New Size(392, 37)
        lblExcelRibbon.TabIndex = 3
        lblExcelRibbon.Text = "Excel — cum se ascunde panglica"
        lblExcelRibbon.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' SetariAplicatieView
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyBody)
        Name = "SetariAplicatieView"
        Size = New Size(960, 457)
        tlyBody.ResumeLayout(False)
        tlyBody.PerformLayout()
        tlyComutatoare.ResumeLayout(False)
        tlyComutatoare.PerformLayout()
        tlyDocumente.ResumeLayout(False)
        tlyDocumente.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents lblTitluComutatoare As Label
    Friend WithEvents tlyComutatoare As KBotTableLayoutPanel
    Friend WithEvents lblVerbose As Label
    Friend WithEvents cboVerbose As KBotComboBox
    Friend WithEvents chkLogViewer As CheckBox
    Friend WithEvents chkShowBrowser As CheckBox
    Friend WithEvents chkReceptii As CheckBox
    Friend WithEvents lblTitluDocumente As Label
    Friend WithEvents tlyDocumente As KBotTableLayoutPanel
    Friend WithEvents lblAdobeMotor As Label
    Friend WithEvents cboAdobeMotor As KBotComboBox
    Friend WithEvents btnAdobeGazduire As Button
    Friend WithEvents lblExcelRibbon As Label
    Friend WithEvents cboExcelRibbon As KBotComboBox
End Class
