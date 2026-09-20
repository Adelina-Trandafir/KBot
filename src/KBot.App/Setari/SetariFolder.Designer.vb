Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariFolder
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


    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        lblFoldereStare = New Label()
        lblFoldereHint = New Label()
        gridFoldere = New KBotDataView()
        btnSalveazaFoldere = New Button()
        lblTitluFoldere = New Label()
        tlyBody = New KBotTableLayoutPanel()
        tips = New KBotToolTip(components)
        CType(gridFoldere, ComponentModel.ISupportInitialize).BeginInit()
        tlyBody.SuspendLayout()
        SuspendLayout()
        ' 
        ' lblFoldereStare
        ' 
        lblFoldereStare.AutoEllipsis = True
        lblFoldereStare.Dock = DockStyle.Fill
        lblFoldereStare.Location = New Point(4, 548)
        lblFoldereStare.Margin = New Padding(4, 0, 4, 0)
        lblFoldereStare.Name = "lblFoldereStare"
        lblFoldereStare.Size = New Size(551, 52)
        lblFoldereStare.TabIndex = 7
        lblFoldereStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblFoldereHint
        ' 
        tlyBody.SetColumnSpan(lblFoldereHint, 2)
        lblFoldereHint.Dock = DockStyle.Fill
        lblFoldereHint.Location = New Point(4, 40)
        lblFoldereHint.Margin = New Padding(4, 0, 4, 8)
        lblFoldereHint.Name = "lblFoldereHint"
        lblFoldereHint.Size = New Size(750, 56)
        lblFoldereHint.TabIndex = 10
        lblFoldereHint.Text = "Calea goală înseamnă «implicit». O cale relativă se rezolvă față de folderul aplicației. Folderele se verifică la pornire — schimbarea are efect la următoarea pornire."
        ' 
        ' gridFoldere
        ' 
        gridFoldere.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridFoldere.BackColor = SystemColors.Window
        gridFoldere.ColumnFillMode = KBotFillMode.LastColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Setare"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn1.Key = "cheie"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 150
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Ce este"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Key = "descriere"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 330
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Implicit"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn3.Key = "implicit"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 190
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Calea configurată"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn4.Key = "cale"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.Width = 230
        gridFoldere.Columns.Add(KBotDataColumn1)
        gridFoldere.Columns.Add(KBotDataColumn2)
        gridFoldere.Columns.Add(KBotDataColumn3)
        gridFoldere.Columns.Add(KBotDataColumn4)
        tlyBody.SetColumnSpan(gridFoldere, 2)
        gridFoldere.Dock = DockStyle.Fill
        gridFoldere.Location = New Point(4, 104)
        gridFoldere.Margin = New Padding(4, 0, 4, 8)
        gridFoldere.Name = "gridFoldere"
        gridFoldere.Size = New Size(750, 436)
        gridFoldere.TabIndex = 11
        tips.SetToolTipHeader(gridFoldere, "Folderele în care scrie aplicația")
        tips.SetToolTipText(gridFoldere, "Doar coloana «Calea configurată» se editează. Apasă «Salvează folderele» după.")
        ' 
        ' btnSalveazaFoldere
        ' 
        btnSalveazaFoldere.Dock = DockStyle.Fill
        btnSalveazaFoldere.Enabled = False
        btnSalveazaFoldere.FlatStyle = FlatStyle.Flat
        btnSalveazaFoldere.Font = New Font("Segoe UI Semibold", 9.0F)
        btnSalveazaFoldere.Location = New Point(563, 548)
        btnSalveazaFoldere.Margin = New Padding(4, 0, 4, 0)
        btnSalveazaFoldere.Name = "btnSalveazaFoldere"
        btnSalveazaFoldere.Size = New Size(191, 52)
        btnSalveazaFoldere.TabIndex = 8
        btnSalveazaFoldere.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveazaFoldere, "Salvează folderele")
        tips.SetToolTipText(btnSalveazaFoldere, "Scrie căile în settings.json (AppData). Se verifică la următoarea pornire.")
        btnSalveazaFoldere.UseVisualStyleBackColor = True
        ' 
        ' lblTitluFoldere
        ' 
        lblTitluFoldere.Dock = DockStyle.Fill
        lblTitluFoldere.Font = New Font("Segoe UI Semibold", 12.0F)
        lblTitluFoldere.Location = New Point(4, 0)
        lblTitluFoldere.Margin = New Padding(4, 0, 4, 4)
        lblTitluFoldere.Name = "lblTitluFoldere"
        lblTitluFoldere.Size = New Size(551, 36)
        lblTitluFoldere.TabIndex = 9
        lblTitluFoldere.Text = "Foldere"
        ' 
        ' tlyBody
        ' 
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 73.86489F))
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 26.1351051F))
        tlyBody.Controls.Add(lblTitluFoldere, 0, 0)
        tlyBody.Controls.Add(lblFoldereHint, 0, 1)
        tlyBody.Controls.Add(btnSalveazaFoldere, 1, 3)
        tlyBody.Controls.Add(gridFoldere, 0, 2)
        tlyBody.Controls.Add(lblFoldereStare, 0, 3)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(20, 18)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 4
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0F))
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Absolute, 64.0F))
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Absolute, 52.0F))
        tlyBody.Size = New Size(758, 600)
        tlyBody.TabIndex = 12
        ' 
        ' SetariFolder
        ' 
        AutoScaleDimensions = New SizeF(9.0F, 22.0F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(tlyBody)
        Name = "SetariFolder"
        Padding = New Padding(20, 18, 20, 18)
        Size = New Size(798, 636)
        CType(gridFoldere, ComponentModel.ISupportInitialize).EndInit()
        tlyBody.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents lblFoldereStare As Label
    Friend WithEvents lblFoldereHint As Label
    Friend WithEvents gridFoldere As KBotDataView
    Friend WithEvents btnSalveazaFoldere As Button
    Friend WithEvents lblTitluFoldere As Label
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents tips As KBotToolTip

End Class
