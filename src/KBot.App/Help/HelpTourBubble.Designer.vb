Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class HelpTourBubble
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
        pnlRoot = New Panel()
        tlyCorp = New KBotTableLayoutPanel()
        lblPas = New Label()
        lblTitlu = New Label()
        lblText = New KBotHtmlLabel()
        lblNota = New Label()
        chkNuMaiArata = New CheckBox()
        btnSariLaObligatoriu = New Button()
        tlyButoane = New KBotTableLayoutPanel()
        btnInapoi = New Button()
        btnInainte = New Button()
        btnInchide = New Button()
        pnlRoot.SuspendLayout()
        tlyCorp.SuspendLayout()
        tlyButoane.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlRoot
        ' 
        pnlRoot.Controls.Add(tlyCorp)
        pnlRoot.Dock = DockStyle.Fill
        pnlRoot.Location = New Point(2, 2)
        pnlRoot.Margin = New Padding(0)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Size = New Size(656, 386)
        pnlRoot.TabIndex = 0
        pnlRoot.Tag = "Card"
        ' 
        ' tlyCorp
        ' 
        tlyCorp.ColumnCount = 1
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCorp.Controls.Add(lblPas, 0, 0)
        tlyCorp.Controls.Add(lblTitlu, 0, 1)
        tlyCorp.Controls.Add(lblText, 0, 2)
        tlyCorp.Controls.Add(lblNota, 0, 3)
        tlyCorp.Controls.Add(chkNuMaiArata, 0, 4)
        tlyCorp.Controls.Add(tlyButoane, 0, 6)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 0)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(16, 12, 16, 12)
        tlyCorp.RowCount = 7
        tlyCorp.RowStyles.Add(New RowStyle())
        tlyCorp.RowStyles.Add(New RowStyle())
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.RowStyles.Add(New RowStyle())
        tlyCorp.RowStyles.Add(New RowStyle())
        tlyCorp.RowStyles.Add(New RowStyle())
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 66F))
        tlyCorp.Size = New Size(656, 386)
        tlyCorp.TabIndex = 1
        ' 
        ' lblPas
        ' 
        lblPas.AutoSize = True
        lblPas.Dock = DockStyle.Fill
        lblPas.Font = New Font("Segoe UI", 9.75F)
        lblPas.Location = New Point(16, 12)
        lblPas.Margin = New Padding(0, 0, 0, 6)
        lblPas.Name = "lblPas"
        lblPas.Size = New Size(624, 28)
        lblPas.TabIndex = 0
        ' 
        ' lblTitlu
        ' 
        lblTitlu.AutoSize = True
        lblTitlu.Dock = DockStyle.Fill
        lblTitlu.Font = New Font("Segoe UI Semibold", 12.5F)
        lblTitlu.Location = New Point(16, 46)
        lblTitlu.Margin = New Padding(0, 0, 0, 12)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.Size = New Size(624, 35)
        lblTitlu.TabIndex = 1
        ' 
        ' lblText
        ' 
        lblText.Dock = DockStyle.Fill
        lblText.Font = New Font("Segoe UI", 10.5F)
        lblText.Location = New Point(16, 93)
        lblText.Margin = New Padding(0)
        lblText.Name = "lblText"
        lblText.Size = New Size(624, 134)
        lblText.TabIndex = 2
        ' 
        ' lblNota
        ' 
        lblNota.AutoSize = True
        lblNota.Dock = DockStyle.Fill
        lblNota.Font = New Font("Segoe UI", 10F)
        lblNota.Location = New Point(16, 236)
        lblNota.Margin = New Padding(0, 9, 0, 0)
        lblNota.Name = "lblNota"
        lblNota.Size = New Size(624, 28)
        lblNota.TabIndex = 3
        lblNota.Visible = False
        ' 
        ' chkNuMaiArata
        ' 
        chkNuMaiArata.AutoSize = True
        chkNuMaiArata.Font = New Font("Segoe UI", 9.75F)
        chkNuMaiArata.Location = New Point(16, 276)
        chkNuMaiArata.Margin = New Padding(0, 12, 0, 0)
        chkNuMaiArata.Name = "chkNuMaiArata"
        chkNuMaiArata.Size = New Size(249, 32)
        chkNuMaiArata.TabIndex = 5
        chkNuMaiArata.Text = "Nu mai arăta turul inițial"
        chkNuMaiArata.UseVisualStyleBackColor = True
        chkNuMaiArata.Visible = False
        ' 
        ' btnSariLaObligatoriu
        ' 
        btnSariLaObligatoriu.Dock = DockStyle.Fill
        btnSariLaObligatoriu.FlatStyle = FlatStyle.Flat
        btnSariLaObligatoriu.Font = New Font("Segoe UI", 10F)
        btnSariLaObligatoriu.Image = My.Resources.Resources.Skip_Green_32
        btnSariLaObligatoriu.Location = New Point(570, 0)
        btnSariLaObligatoriu.Margin = New Padding(0)
        btnSariLaObligatoriu.Name = "btnSariLaObligatoriu"
        btnSariLaObligatoriu.Size = New Size(54, 54)
        btnSariLaObligatoriu.TabIndex = 6
        btnSariLaObligatoriu.UseVisualStyleBackColor = True
        btnSariLaObligatoriu.Visible = False
        ' 
        ' tlyButoane
        ' 
        tlyButoane.ColumnCount = 5
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.Controls.Add(btnInchide, 0, 0)
        tlyButoane.Controls.Add(btnInapoi, 2, 0)
        tlyButoane.Controls.Add(btnInainte, 3, 0)
        tlyButoane.Controls.Add(btnSariLaObligatoriu, 4, 0)
        tlyButoane.Dock = DockStyle.Fill
        tlyButoane.Location = New Point(16, 320)
        tlyButoane.Margin = New Padding(0, 12, 0, 0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.RowCount = 1
        tlyButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButoane.Size = New Size(624, 54)
        tlyButoane.TabIndex = 4
        ' 
        ' btnInapoi
        ' 
        btnInapoi.Dock = DockStyle.Fill
        btnInapoi.FlatStyle = FlatStyle.Flat
        btnInapoi.Font = New Font("Segoe UI", 10F)
        btnInapoi.Image = My.Resources.Resources.Prev_Green_32
        btnInapoi.Location = New Point(462, 0)
        btnInapoi.Margin = New Padding(0)
        btnInapoi.Name = "btnInapoi"
        btnInapoi.Size = New Size(54, 54)
        btnInapoi.TabIndex = 0
        btnInapoi.UseVisualStyleBackColor = True
        ' 
        ' btnInainte
        ' 
        btnInainte.Dock = DockStyle.Fill
        btnInainte.FlatStyle = FlatStyle.Flat
        btnInainte.Font = New Font("Segoe UI", 10F)
        btnInainte.Image = My.Resources.Resources.Next_Green_32
        btnInainte.Location = New Point(516, 0)
        btnInainte.Margin = New Padding(0)
        btnInainte.Name = "btnInainte"
        btnInainte.Size = New Size(54, 54)
        btnInainte.TabIndex = 1
        btnInainte.UseVisualStyleBackColor = True
        ' 
        ' btnInchide
        ' 
        btnInchide.Dock = DockStyle.Fill
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Font = New Font("Segoe UI", 10F)
        btnInchide.Image = My.Resources.Resources.Stop_Green_32
        btnInchide.Location = New Point(0, 0)
        btnInchide.Margin = New Padding(0)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New Size(54, 54)
        btnInchide.TabIndex = 2
        btnInchide.UseVisualStyleBackColor = True
        ' 
        ' HelpTourBubble
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        BorderlessShadow = False
        ClientSize = New Size(660, 390)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        KeyPreview = True
        Margin = New Padding(4, 4, 4, 4)
        Name = "HelpTourBubble"
        Padding = New Padding(2, 2, 2, 2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Tur ghidat"
        TopMost = True
        pnlRoot.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        tlyCorp.PerformLayout()
        tlyButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlRoot As Panel
    Friend WithEvents tlyCorp As KBotTableLayoutPanel
    Friend WithEvents lblPas As Label
    Friend WithEvents lblTitlu As Label
    Friend WithEvents lblText As KBotHtmlLabel
    Friend WithEvents lblNota As Label
    Friend WithEvents chkNuMaiArata As CheckBox
    Friend WithEvents btnSariLaObligatoriu As Button
    Friend WithEvents tlyButoane As KBotTableLayoutPanel
    Friend WithEvents btnInapoi As Button
    Friend WithEvents btnInainte As Button
    Friend WithEvents btnInchide As Button
End Class
