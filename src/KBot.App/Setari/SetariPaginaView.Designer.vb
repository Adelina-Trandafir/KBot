Imports KBot.Controls

' The «Pagina FOREXE» page of the settings window (operator, 21.09.2026): the CSS rules
' K-BOT writes into every FOREXE page - a list on the left, the selected rule's three
' fields on the right, «Regulă nouă…» opening the window with the tree of the page. All
' controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are in the
' 144 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariPaginaView
    Inherits Global.KBot.Theming.KBotThemedUserControl

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyBody = New KBotTableLayoutPanel()
        lblTitlu = New Label()
        lblHint = New Label()
        tlyMijloc = New KBotTableLayoutPanel()
        grila = New KBotDataView()
        editor = New RegulaPaginaEditor()
        tlyButoane = New KBotTableLayoutPanel()
        btnAdauga = New Button()
        btnSterge = New Button()
        btnImplicite = New Button()
        btnSalveaza = New Button()
        tlyBody.SuspendLayout()
        tlyMijloc.SuspendLayout()
        CType(grila, ComponentModel.ISupportInitialize).BeginInit()
        tlyButoane.SuspendLayout()
        SuspendLayout()
        '
        ' tlyBody
        '
        tlyBody.ColumnCount = 1
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBody.Controls.Add(lblTitlu, 0, 0)
        tlyBody.Controls.Add(lblHint, 0, 1)
        tlyBody.Controls.Add(tlyMijloc, 0, 2)
        tlyBody.Controls.Add(tlyButoane, 0, 3)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(0, 0)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.Padding = New Padding(24, 18, 24, 18)
        tlyBody.RowCount = 4
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.Size = New Size(1400, 840)
        tlyBody.TabIndex = 0
        '
        ' lblTitlu
        '
        lblTitlu.AutoSize = True
        lblTitlu.Font = New Font("Segoe UI Semibold", 12F)
        lblTitlu.Location = New Point(28, 18)
        lblTitlu.Margin = New Padding(4, 0, 4, 8)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.Size = New Size(245, 32)
        lblTitlu.TabIndex = 0
        lblTitlu.Text = "Stilurile paginii FOREXE"
        '
        ' lblHint
        '
        lblHint.AutoSize = True
        lblHint.Dock = DockStyle.Fill
        lblHint.Location = New Point(28, 58)
        lblHint.Margin = New Padding(4, 0, 4, 12)
        lblHint.Name = "lblHint"
        lblHint.Size = New Size(1336, 52)
        lblHint.TabIndex = 1
        lblHint.Text = "Reguli CSS puse în pagina FOREXE la fiecare încărcare. Rândul ales se editează în dreapta; nimic nu se aplică până la «Salvează și aplică»."
        '
        ' tlyMijloc
        '
        tlyMijloc.ColumnCount = 2
        tlyMijloc.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 58F))
        tlyMijloc.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 42F))
        tlyMijloc.Controls.Add(grila, 0, 0)
        tlyMijloc.Controls.Add(editor, 1, 0)
        tlyMijloc.Dock = DockStyle.Fill
        tlyMijloc.Location = New Point(24, 122)
        tlyMijloc.Margin = New Padding(0)
        tlyMijloc.Name = "tlyMijloc"
        tlyMijloc.RowCount = 1
        tlyMijloc.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMijloc.Size = New Size(1352, 636)
        tlyMijloc.TabIndex = 2
        '
        ' grila
        '
        grila.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grila.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Activ"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "activ"
        KBotDataColumn1.MinWidth = 40
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 50
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Ce face"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "nota"
        KBotDataColumn2.MinWidth = 80
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 170
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Selector"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "selector"
        KBotDataColumn3.MinWidth = 100
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 200
        grila.Columns.Add(KBotDataColumn1)
        grila.Columns.Add(KBotDataColumn2)
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Pagina"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "pagina"
        KBotDataColumn4.MinWidth = 60
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 150
        grila.Columns.Add(KBotDataColumn3)
        grila.Columns.Add(KBotDataColumn4)
        grila.Dock = DockStyle.Fill
        grila.FillColumnKey = "selector"
        grila.HeaderHeight = 24
        grila.Location = New Point(4, 0)
        grila.Margin = New Padding(4, 0, 4, 12)
        grila.Name = "grila"
        grila.Size = New Size(944, 624)
        grila.TabIndex = 0
        tips.SetToolTipHeader(grila, "Regulile paginii")
        tips.SetToolTipText(grila, "Rândul ales se editează în câmpurile din dreapta; bifa «Activ» se schimbă direct aici." & vbLf & "Un rând debifat rămâne în listă, dar nu se pune în pagină.")
        '
        ' editor
        '
        editor.Dock = DockStyle.Fill
        editor.Location = New Point(968, 0)
        editor.Margin = New Padding(16, 0, 0, 12)
        editor.Name = "editor"
        editor.Size = New Size(384, 624)
        editor.TabIndex = 1
        '
        ' tlyButoane
        '
        tlyButoane.AutoFitToTheme = False
        tlyButoane.AutoSize = True
        tlyButoane.ColumnCount = 5
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.Controls.Add(btnAdauga, 0, 0)
        tlyButoane.Controls.Add(btnSterge, 1, 0)
        tlyButoane.Controls.Add(btnImplicite, 2, 0)
        tlyButoane.Controls.Add(btnSalveaza, 4, 0)
        tlyButoane.Dock = DockStyle.Top
        tlyButoane.Location = New Point(28, 758)
        tlyButoane.Margin = New Padding(4, 0, 4, 0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.RowCount = 1
        tlyButoane.RowStyles.Add(New RowStyle())
        tlyButoane.Size = New Size(1344, 52)
        tlyButoane.TabIndex = 3
        '
        ' btnAdauga
        '
        btnAdauga.AutoSize = True
        btnAdauga.FlatStyle = FlatStyle.Flat
        btnAdauga.Location = New Point(0, 0)
        btnAdauga.Margin = New Padding(0, 0, 6, 0)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Padding = New Padding(6, 3, 6, 3)
        btnAdauga.Size = New Size(130, 46)
        btnAdauga.TabIndex = 0
        btnAdauga.Text = "Regulă nouă…"
        tips.SetToolTipHeader(btnAdauga, "Regulă nouă")
        tips.SetToolTipText(btnAdauga, "Deschide fereastra cu arborele paginii FOREXE: alegeți elementul, completați stilul." & vbLf & "Fără browser pornit, regula se scrie de mână.")
        btnAdauga.UseVisualStyleBackColor = True
        '
        ' btnSterge
        '
        btnSterge.AutoSize = True
        btnSterge.FlatStyle = FlatStyle.Flat
        btnSterge.Location = New Point(138, 0)
        btnSterge.Margin = New Padding(0, 0, 6, 0)
        btnSterge.Name = "btnSterge"
        btnSterge.Padding = New Padding(6, 3, 6, 3)
        btnSterge.Size = New Size(130, 46)
        btnSterge.TabIndex = 1
        btnSterge.Text = "Șterge"
        tips.SetToolTipHeader(btnSterge, "Șterge rândul")
        tips.SetToolTipText(btnSterge, "Scoate din listă rândul selectat.")
        btnSterge.UseVisualStyleBackColor = True
        '
        ' btnImplicite
        '
        btnImplicite.AutoSize = True
        btnImplicite.FlatStyle = FlatStyle.Flat
        btnImplicite.Location = New Point(276, 0)
        btnImplicite.Margin = New Padding(0, 0, 6, 0)
        btnImplicite.Name = "btnImplicite"
        btnImplicite.Padding = New Padding(6, 3, 6, 3)
        btnImplicite.Size = New Size(130, 46)
        btnImplicite.TabIndex = 2
        btnImplicite.Text = "Implicite"
        tips.SetToolTipHeader(btnImplicite, "Regulile implicite")
        tips.SetToolTipText(btnImplicite, "Înlocuiește lista cu regulile K-BOT de la început (meniul lateral ascuns cât e un angajament deschis, lățimea paginii, textul)." & vbLf & "Nimic nu se salvează până la «Salvează și aplică».")
        btnImplicite.UseVisualStyleBackColor = True
        '
        ' btnSalveaza
        '
        btnSalveaza.AutoSize = True
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(1188, 0)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(8, 3, 8, 3)
        btnSalveaza.Size = New Size(156, 46)
        btnSalveaza.TabIndex = 3
        btnSalveaza.Text = "Salvează și aplică"
        tips.SetToolTipHeader(btnSalveaza, "Salvează și aplică")
        tips.SetToolTipText(btnSalveaza, "Scrie regulile în setări și le trimite în pagina FOREXE deschisă (dacă există)." & vbLf & "Toate încărcările următoare pornesc cu ele.")
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' SetariPaginaView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyBody)
        Name = "SetariPaginaView"
        Size = New Size(1400, 840)
        tlyBody.ResumeLayout(False)
        tlyBody.PerformLayout()
        tlyMijloc.ResumeLayout(False)
        CType(grila, ComponentModel.ISupportInitialize).EndInit()
        tlyButoane.ResumeLayout(False)
        tlyButoane.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents lblTitlu As Label
    Friend WithEvents lblHint As Label
    Friend WithEvents tlyMijloc As KBotTableLayoutPanel
    Friend WithEvents grila As KBotDataView
    Friend WithEvents editor As RegulaPaginaEditor
    Friend WithEvents tlyButoane As KBotTableLayoutPanel
    Friend WithEvents btnAdauga As Button
    Friend WithEvents btnSterge As Button
    Friend WithEvents btnImplicite As Button
    Friend WithEvents btnSalveaza As Button
End Class
