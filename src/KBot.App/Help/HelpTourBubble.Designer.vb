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
        components = New ComponentModel.Container()
        pnlRoot = New Panel()
        tlyCorp = New KBotTableLayoutPanel()
        lblPas = New Label()
        lblTitlu = New Label()
        lblText = New Label()
        lblNota = New Label()
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
        pnlRoot.Location = New Point(1, 1)
        pnlRoot.Margin = New Padding(0)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Size = New Size(438, 258)
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
        tlyCorp.Controls.Add(tlyButoane, 0, 4)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 0)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(16, 12, 16, 12)
        tlyCorp.RowCount = 5
        tlyCorp.RowStyles.Add(New RowStyle())
        tlyCorp.RowStyles.Add(New RowStyle())
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.RowStyles.Add(New RowStyle())
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyCorp.Size = New Size(438, 258)
        tlyCorp.TabIndex = 1
        '
        ' lblPas
        '
        lblPas.AutoSize = True
        lblPas.Dock = DockStyle.Fill
        lblPas.Font = New Font("Segoe UI", 9.75F)
        lblPas.Margin = New Padding(0, 0, 0, 4)
        lblPas.Name = "lblPas"
        lblPas.TabIndex = 0
        '
        ' lblTitlu
        '
        lblTitlu.AutoSize = True
        lblTitlu.Dock = DockStyle.Fill
        lblTitlu.Font = New Font("Segoe UI Semibold", 12.5F)
        lblTitlu.Margin = New Padding(0, 0, 0, 8)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.TabIndex = 1
        '
        ' lblText
        '
        lblText.Dock = DockStyle.Fill
        lblText.Font = New Font("Segoe UI", 10.5F)
        lblText.Margin = New Padding(0)
        lblText.Name = "lblText"
        lblText.TabIndex = 2
        '
        ' lblNota
        '
        lblNota.AutoSize = True
        lblNota.Dock = DockStyle.Fill
        lblNota.Font = New Font("Segoe UI", 10F)
        lblNota.Margin = New Padding(0, 6, 0, 0)
        lblNota.Name = "lblNota"
        lblNota.TabIndex = 3
        lblNota.Visible = False
        '
        ' tlyButoane
        '
        tlyButoane.ColumnCount = 4
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlyButoane.Controls.Add(btnInapoi, 1, 0)
        tlyButoane.Controls.Add(btnInainte, 2, 0)
        tlyButoane.Controls.Add(btnInchide, 3, 0)
        tlyButoane.Dock = DockStyle.Fill
        tlyButoane.Margin = New Padding(0, 8, 0, 0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.RowCount = 1
        tlyButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButoane.TabIndex = 4
        '
        ' btnInapoi
        '
        btnInapoi.Dock = DockStyle.Fill
        btnInapoi.FlatStyle = FlatStyle.Flat
        btnInapoi.Font = New Font("Segoe UI", 10F)
        btnInapoi.Margin = New Padding(0, 0, 6, 0)
        btnInapoi.Name = "btnInapoi"
        btnInapoi.TabIndex = 0
        btnInapoi.Text = "◄ Înapoi"
        btnInapoi.UseVisualStyleBackColor = True
        '
        ' btnInainte
        '
        btnInainte.Dock = DockStyle.Fill
        btnInainte.FlatStyle = FlatStyle.Flat
        btnInainte.Font = New Font("Segoe UI", 10F)
        btnInainte.Margin = New Padding(0, 0, 6, 0)
        btnInainte.Name = "btnInainte"
        btnInainte.TabIndex = 1
        btnInainte.Text = "Înainte ►"
        btnInainte.UseVisualStyleBackColor = True
        '
        ' btnInchide
        '
        btnInchide.Dock = DockStyle.Fill
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Font = New Font("Segoe UI", 10F)
        btnInchide.Margin = New Padding(0)
        btnInchide.Name = "btnInchide"
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' HelpTourBubble
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        BorderlessShadow = False
        ClientSize = New Size(440, 260)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        KeyPreview = True
        Name = "HelpTourBubble"
        Padding = New Padding(1)
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
    Friend WithEvents lblText As Label
    Friend WithEvents lblNota As Label
    Friend WithEvents tlyButoane As KBotTableLayoutPanel
    Friend WithEvents btnInapoi As Button
    Friend WithEvents btnInainte As Button
    Friend WithEvents btnInchide As Button
End Class
