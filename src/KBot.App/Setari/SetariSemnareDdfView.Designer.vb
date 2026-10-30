Imports KBot.Controls

' The «Semnare DDF» page of the settings window (slice 0124-05): which role signs section A of a DDF created in K-BOT, per compartment.
' One line per compartment (the ones the documents of the unit use, plus the ones already in the table); the role is picked in the
' second column. All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are in the 144 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariSemnareDdfView
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
        tips = New KBotToolTip(components)
        tlyBody = New KBotTableLayoutPanel()
        lblTitlu = New Label()
        lblHint = New Label()
        grila = New KBotDataView()
        tlyButoane = New KBotTableLayoutPanel()
        btnReincarca = New Button()
        btnSalveaza = New Button()
        tlyBody.SuspendLayout()
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
        tlyBody.Controls.Add(grila, 0, 2)
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
        lblTitlu.Size = New Size(330, 32)
        lblTitlu.TabIndex = 0
        lblTitlu.Text = "Cine semnează Secțiunea A, pe compartimente"
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
        lblHint.Text = "Pentru un document de fundamentare nou, creat în K-BOT, Secțiunea A o semnează rolul ales aici pentru compartimentul documentului. Documentul apare în lista de semnat a acelui rol. Un compartiment fără rol oprește generarea documentului, cu un mesaj. Nimic nu se aplică până la «Salvează»."
        '
        ' grila
        '
        grila.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grila.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Compartiment"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "comp"
        KBotDataColumn1.MinWidth = 160
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 500
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnType = KBotColumnType.Combo
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Rolul care semnează A"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "rol"
        KBotDataColumn2.MinWidth = 160
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Width = 320
        grila.Columns.Add(KBotDataColumn1)
        grila.Columns.Add(KBotDataColumn2)
        grila.Dock = DockStyle.Fill
        grila.FillColumnKey = "comp"
        grila.HeaderHeight = 24
        grila.Location = New Point(28, 122)
        grila.Margin = New Padding(4, 0, 4, 12)
        grila.Name = "grila"
        grila.Size = New Size(1344, 640)
        grila.TabIndex = 2
        '
        ' tlyButoane
        '
        tlyButoane.AutoFitToTheme = False
        tlyButoane.AutoSize = True
        tlyButoane.ColumnCount = 3
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle())
        tlyButoane.Controls.Add(btnReincarca, 0, 0)
        tlyButoane.Controls.Add(btnSalveaza, 2, 0)
        tlyButoane.Dock = DockStyle.Top
        tlyButoane.Location = New Point(28, 774)
        tlyButoane.Margin = New Padding(4, 0, 4, 0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.RowCount = 1
        tlyButoane.RowStyles.Add(New RowStyle())
        tlyButoane.Size = New Size(1344, 52)
        tlyButoane.TabIndex = 3
        '
        ' btnReincarca
        '
        btnReincarca.AutoSize = True
        btnReincarca.FlatStyle = FlatStyle.Flat
        btnReincarca.Location = New Point(0, 0)
        btnReincarca.Margin = New Padding(0, 0, 6, 0)
        btnReincarca.Name = "btnReincarca"
        btnReincarca.Padding = New Padding(6, 3, 6, 3)
        btnReincarca.Size = New Size(170, 46)
        btnReincarca.TabIndex = 0
        btnReincarca.Text = "Reîncarcă"
        tips.SetToolTipHeader(btnReincarca, "Reîncarcă")
        tips.SetToolTipText(btnReincarca, "Citește din nou compartimentele și rolurile de pe server." & vbLf & "Alegerile nesalvate se pierd.")
        btnReincarca.UseVisualStyleBackColor = True
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
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Scrie pe server rolurile alese pentru compartimente." & vbLf & "Se aplică documentelor noi, generate de acum înainte.")
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' SetariSemnareDdfView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyBody)
        Name = "SetariSemnareDdfView"
        Size = New Size(1400, 840)
        tlyBody.ResumeLayout(False)
        tlyBody.PerformLayout()
        CType(grila, ComponentModel.ISupportInitialize).EndInit()
        tlyButoane.ResumeLayout(False)
        tlyButoane.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents lblTitlu As Label
    Friend WithEvents lblHint As Label
    Friend WithEvents grila As KBotDataView
    Friend WithEvents tlyButoane As KBotTableLayoutPanel
    Friend WithEvents btnReincarca As Button
    Friend WithEvents btnSalveaza As Button
End Class
