Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariAccessView
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyPagina = New KBotTableLayoutPanel()
        lblTitlu = New Label()
        lblStare = New Label()
        tlyCale = New KBotTableLayoutPanel()
        lblCale = New Label()
        txtCale = New KBotTextField()
        btnAlege = New Button()
        btnReincarca = New Button()
        gridRegistru = New KBotDataView()
        dlgCale = New OpenFileDialog()
        tlyPagina.SuspendLayout()
        tlyCale.SuspendLayout()
        CType(gridRegistru, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' tlyPagina
        '
        tlyPagina.ColumnCount = 1
        tlyPagina.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPagina.Controls.Add(lblTitlu, 0, 0)
        tlyPagina.Controls.Add(lblStare, 0, 1)
        tlyPagina.Controls.Add(tlyCale, 0, 2)
        tlyPagina.Controls.Add(gridRegistru, 0, 3)
        tlyPagina.Dock = DockStyle.Fill
        tlyPagina.Location = New Point(0, 0)
        tlyPagina.Margin = New Padding(0)
        tlyPagina.Name = "tlyPagina"
        tlyPagina.Padding = New Padding(24, 18, 24, 18)
        tlyPagina.RowCount = 4
        tlyPagina.RowStyles.Add(New RowStyle())
        tlyPagina.RowStyles.Add(New RowStyle())
        tlyPagina.RowStyles.Add(New RowStyle())
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyPagina.Size = New Size(640, 400)
        tlyPagina.TabIndex = 0
        '
        ' lblTitlu
        '
        lblTitlu.AutoSize = True
        lblTitlu.Font = New Font("Segoe UI Semibold", 12F)
        lblTitlu.Location = New Point(28, 18)
        lblTitlu.Margin = New Padding(4, 0, 4, 8)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.Size = New Size(60, 21)
        lblTitlu.TabIndex = 0
        lblTitlu.Text = "Access"
        '
        ' lblStare
        '
        lblStare.AutoSize = True
        lblStare.Location = New Point(28, 47)
        lblStare.Margin = New Padding(4, 0, 4, 12)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(10, 15)
        lblStare.TabIndex = 1
        lblStare.Text = " "
        '
        ' tlyCale
        '
        tlyCale.AutoFitToTheme = False
        tlyCale.AutoSize = True
        tlyCale.ColumnCount = 4
        tlyCale.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190F))
        tlyCale.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCale.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90F))
        tlyCale.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyCale.Controls.Add(lblCale, 0, 0)
        tlyCale.Controls.Add(txtCale, 1, 0)
        tlyCale.Controls.Add(btnAlege, 2, 0)
        tlyCale.Controls.Add(btnReincarca, 3, 0)
        tlyCale.Dock = DockStyle.Top
        tlyCale.Location = New Point(28, 80)
        tlyCale.Margin = New Padding(4, 0, 4, 12)
        tlyCale.Name = "tlyCale"
        tlyCale.RowCount = 1
        tlyCale.RowStyles.Add(New RowStyle())
        tlyCale.Size = New Size(584, 40)
        tlyCale.TabIndex = 2
        '
        ' lblCale
        '
        lblCale.AutoSize = True
        lblCale.Dock = DockStyle.Fill
        lblCale.Location = New Point(4, 0)
        lblCale.Margin = New Padding(4, 0, 4, 0)
        lblCale.Name = "lblCale"
        lblCale.Size = New Size(182, 30)
        lblCale.TabIndex = 0
        lblCale.Text = "Calea către «cale.accdb»"
        lblCale.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtCale
        '
        txtCale.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtCale.BackColor = Color.Transparent
        txtCale.Location = New Point(194, 0)
        txtCale.Margin = New Padding(4, 0, 4, 0)
        txtCale.MaxLength = 260
        txtCale.Name = "txtCale"
        txtCale.PlaceholderText = "C:\AVACONT\cale.accdb"
        txtCale.Size = New Size(186, 30)
        txtCale.TabIndex = 1
        txtCale.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtCale, "Registrul AVACONT")
        tips.SetToolTipText(txtCale, "Fișierul «cale.accdb», care spune unde se află fișierul Access al fiecărei unități." & vbLf & "Se salvează la Enter sau când ieși din câmp. Câmpul gol readuce calea implicită.")
        '
        ' btnAlege
        '
        btnAlege.Dock = DockStyle.Fill
        btnAlege.FlatStyle = FlatStyle.Flat
        btnAlege.Location = New Point(388, 0)
        btnAlege.Margin = New Padding(4, 0, 4, 0)
        btnAlege.Name = "btnAlege"
        btnAlege.Size = New Size(82, 30)
        btnAlege.TabIndex = 2
        btnAlege.Text = "Alege…"
        tips.SetToolTipHeader(btnAlege, "Alege fișierul")
        tips.SetToolTipText(btnAlege, "Alegi fișierul «cale.accdb» de pe disc.")
        btnAlege.UseVisualStyleBackColor = True
        '
        ' btnReincarca
        '
        btnReincarca.Dock = DockStyle.Fill
        btnReincarca.FlatStyle = FlatStyle.Flat
        btnReincarca.Location = New Point(474, 0)
        btnReincarca.Margin = New Padding(4, 0, 4, 0)
        btnReincarca.Name = "btnReincarca"
        btnReincarca.Size = New Size(106, 30)
        btnReincarca.TabIndex = 3
        btnReincarca.Text = "Reîncarcă"
        tips.SetToolTipHeader(btnReincarca, "Reîncarcă")
        tips.SetToolTipText(btnReincarca, "Citește din nou registrul, pentru anul și sursa alese acum în K-BOT.")
        btnReincarca.UseVisualStyleBackColor = True
        '
        ' gridRegistru
        '
        gridRegistru.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridRegistru.BackColor = SystemColors.Window
        gridRegistru.ColumnFillMode = KBotFillMode.SpecificColumn
        gridRegistru.FillColumnKey = "numeunitate"
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "IdUnitate"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "idunitate"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 80
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "DC"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "dc"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 110
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "NumeUnitate"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "numeunitate"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 220
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "SURSA"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "sursa"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 70
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "AnDate"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "andate"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Width = 70
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "FullPath"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "fullpath"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 260
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "CaleForexe"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "caleforexe"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Width = 260
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "AlteDetalii"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "altedetalii"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 120
        gridRegistru.Columns.Add(KBotDataColumn1)
        gridRegistru.Columns.Add(KBotDataColumn2)
        gridRegistru.Columns.Add(KBotDataColumn3)
        gridRegistru.Columns.Add(KBotDataColumn4)
        gridRegistru.Columns.Add(KBotDataColumn5)
        gridRegistru.Columns.Add(KBotDataColumn6)
        gridRegistru.Columns.Add(KBotDataColumn7)
        gridRegistru.Columns.Add(KBotDataColumn8)
        gridRegistru.Dock = DockStyle.Fill
        gridRegistru.FooterBackColor = SystemColors.Control
        gridRegistru.FooterCaption = "Rânduri"
        gridRegistru.FooterSeparatorColor = SystemColors.ActiveBorder
        gridRegistru.FooterVisible = True
        gridRegistru.HeaderBackColor = SystemColors.Control
        gridRegistru.HeaderHeight = 24
        gridRegistru.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridRegistru.Location = New Point(28, 132)
        gridRegistru.Margin = New Padding(4, 0, 4, 0)
        gridRegistru.Name = "gridRegistru"
        gridRegistru.RowHeight = 24
        gridRegistru.Size = New Size(584, 250)
        gridRegistru.TabIndex = 3
        tips.SetToolTipHeader(gridRegistru, "Tabelul «cai» din cale.accdb")
        tips.SetToolTipText(gridRegistru, "Rândurile registrului pentru anul și sursa alese acum în K-BOT (coloanele AnDate și SURSA)." & vbLf & "Căile din FullPath și CaleForexe sunt afișate așa cum sunt scrise în registru.")
        '
        ' dlgCale
        '
        dlgCale.Filter = "Registru Access (*.accdb;*.mdb)|*.accdb;*.mdb|Toate fișierele (*.*)|*.*"
        dlgCale.Title = "Alege registrul cale.accdb"
        '
        ' SetariAccessView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyPagina)
        Name = "SetariAccessView"
        Size = New Size(640, 400)
        tlyPagina.ResumeLayout(False)
        tlyPagina.PerformLayout()
        tlyCale.ResumeLayout(False)
        tlyCale.PerformLayout()
        CType(gridRegistru, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyPagina As KBotTableLayoutPanel
    Friend WithEvents lblTitlu As Label
    Friend WithEvents lblStare As Label
    Friend WithEvents tlyCale As KBotTableLayoutPanel
    Friend WithEvents lblCale As Label
    Friend WithEvents txtCale As KBotTextField
    Friend WithEvents btnAlege As Button
    Friend WithEvents btnReincarca As Button
    Friend WithEvents gridRegistru As KBotDataView
    Friend WithEvents dlgCale As OpenFileDialog
End Class
