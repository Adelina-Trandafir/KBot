Imports KBot.Controls

' The partners associated with a DDF (slice 0084-02) -- ONE view, two hosts: the «Parteneri»
' page of the DDF editor (slice 0094-02) and the window opened by the Sumar button
' (SumarPartnersForm). Both need the same thing: the partners the document has, a picker for
' one more, and a way to take one out.
'
' Top to bottom:
'   tlyPick     the picker -- every candidate partner NOT yet associated, and «Asociaza»
'   grd         the associated partners (fiscal code, name, role); read-only
'   tlyBottom   the state sentence and «Scoate din asociere»
'
' The view holds no document and makes no request: the host hands it the lists
' (SetCandidates / SetAssociated) and acts on the two events (AddRequested / RemoveRequested).
'
' All controls are declared HERE (docs/kbot-forms-ui-convention.md).
' Coordinates are written at 144 dpi; AutoScaleDimensions goes with them (slice 0066-02).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfPartnersView
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
        tips = New KBotToolTip(components)
        cmbPartner = New KBotComboBox()
        btnAdd = New Button()
        btnRemove = New Button()
        tlyRoot = New KBotTableLayoutPanel()
        tlyPick = New KBotTableLayoutPanel()
        lblPick = New Label()
        grd = New KBotDataView()
        tlyBottom = New KBotTableLayoutPanel()
        lblState = New Label()
        tlyRoot.SuspendLayout()
        tlyPick.SuspendLayout()
        CType(grd, ComponentModel.ISupportInitialize).BeginInit()
        tlyBottom.SuspendLayout()
        SuspendLayout()
        '
        ' cmbPartner
        '
        cmbPartner.Dock = DockStyle.Fill
        cmbPartner.Editable = True
        cmbPartner.FindAsYouType = True
        cmbPartner.Font = New Font("Calibri", 9F, FontStyle.Bold)
        cmbPartner.Location = New Point(150, 11)
        cmbPartner.Margin = New Padding(6, 11, 6, 11)
        cmbPartner.Name = "cmbPartner"
        cmbPartner.Size = New Size(600, 34)
        cmbPartner.TabIndex = 1
        tips.SetToolTipHeader(cmbPartner, "Partener de asociat")
        tips.SetToolTipText(cmbPartner, "Alege un partener din listă (scrie începutul numelui sau al codului fiscal)." & vbLf & "Partenerii deja asociați nu mai apar aici.")
        '
        ' btnAdd
        '
        btnAdd.Dock = DockStyle.Fill
        btnAdd.Location = New Point(762, 8)
        btnAdd.Margin = New Padding(6, 8, 11, 8)
        btnAdd.Name = "btnAdd"
        btnAdd.Size = New Size(200, 40)
        btnAdd.TabIndex = 2
        btnAdd.Text = "Asociază"
        tips.SetToolTipHeader(btnAdd, "Asociază partenerul")
        tips.SetToolTipText(btnAdd, "Adaugă partenerul ales la lista de mai jos." & vbLf & "Un partener deja asociat nu se adaugă a doua oară.")
        btnAdd.UseVisualStyleBackColor = True
        '
        ' btnRemove
        '
        btnRemove.Dock = DockStyle.Fill
        btnRemove.Enabled = False
        btnRemove.Location = New Point(762, 8)
        btnRemove.Margin = New Padding(6, 8, 11, 8)
        btnRemove.Name = "btnRemove"
        btnRemove.Size = New Size(260, 40)
        btnRemove.TabIndex = 4
        btnRemove.Text = "Scoate din asociere"
        tips.SetToolTipHeader(btnRemove, "Scoate din asociere")
        tips.SetToolTipText(btnRemove, "Scoate partenerul selectat din lista documentului." & vbLf & "Partenerul principal (cel din antet) se schimbă din antet, nu de aici.")
        btnRemove.UseVisualStyleBackColor = True
        '
        ' tlyRoot
        '
        tlyRoot.ColumnCount = 1
        tlyRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRoot.Controls.Add(tlyPick, 0, 0)
        tlyRoot.Controls.Add(grd, 0, 1)
        tlyRoot.Controls.Add(tlyBottom, 0, 2)
        tlyRoot.Dock = DockStyle.Fill
        tlyRoot.Location = New Point(0, 0)
        tlyRoot.Margin = New Padding(0)
        tlyRoot.Name = "tlyRoot"
        tlyRoot.RowCount = 3
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyRoot.Size = New Size(973, 420)
        tlyRoot.TabIndex = 0
        '
        ' tlyPick
        '
        tlyPick.ColumnCount = 3
        tlyPick.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 144F))
        tlyPick.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPick.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 217F))
        tlyPick.Controls.Add(lblPick, 0, 0)
        tlyPick.Controls.Add(cmbPartner, 1, 0)
        tlyPick.Controls.Add(btnAdd, 2, 0)
        tlyPick.Dock = DockStyle.Fill
        tlyPick.Location = New Point(0, 0)
        tlyPick.Margin = New Padding(0)
        tlyPick.Name = "tlyPick"
        tlyPick.RowCount = 1
        tlyPick.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyPick.Size = New Size(973, 56)
        tlyPick.TabIndex = 0
        '
        ' lblPick
        '
        lblPick.Dock = DockStyle.Fill
        lblPick.Font = New Font("Calibri", 9F)
        lblPick.Location = New Point(8, 0)
        lblPick.Margin = New Padding(8, 0, 4, 0)
        lblPick.Name = "lblPick"
        lblPick.Size = New Size(132, 56)
        lblPick.TabIndex = 0
        lblPick.Text = "Partener"
        lblPick.TextAlign = ContentAlignment.MiddleLeft
        '
        ' grd
        '
        grd.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grd.BackColor = SystemColors.Window
        grd.BorderColor = SystemColors.ActiveBorder
        grd.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn1.HeaderText = "Cod fiscal"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "cod_fiscal"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 160
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn2.HeaderText = "Denumire"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "nume"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 420
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn3.HeaderText = "Rol"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "rol"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 160
        grd.Columns.Add(KBotDataColumn1)
        grd.Columns.Add(KBotDataColumn2)
        grd.Columns.Add(KBotDataColumn3)
        grd.Dock = DockStyle.Fill
        grd.FillColumnKey = "nume"
        grd.HeaderBackColor = SystemColors.Control
        grd.HeaderSeparatorColor = SystemColors.ActiveBorder
        grd.Location = New Point(6, 63)
        grd.Margin = New Padding(6, 7, 6, 7)
        grd.Name = "grd"
        grd.ReadOnlyGrid = True
        grd.ShrinkColumnsToFit = False
        grd.Size = New Size(961, 294)
        grd.TabIndex = 3
        '
        ' tlyBottom
        '
        tlyBottom.ColumnCount = 2
        tlyBottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBottom.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 277F))
        tlyBottom.Controls.Add(lblState, 0, 0)
        tlyBottom.Controls.Add(btnRemove, 1, 0)
        tlyBottom.Dock = DockStyle.Fill
        tlyBottom.Location = New Point(0, 364)
        tlyBottom.Margin = New Padding(0)
        tlyBottom.Name = "tlyBottom"
        tlyBottom.RowCount = 1
        tlyBottom.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBottom.Size = New Size(973, 56)
        tlyBottom.TabIndex = 2
        '
        ' lblState
        '
        lblState.Dock = DockStyle.Fill
        lblState.Font = New Font("Calibri", 9F)
        lblState.Location = New Point(8, 0)
        lblState.Margin = New Padding(8, 0, 6, 0)
        lblState.Name = "lblState"
        lblState.Size = New Size(678, 56)
        lblState.TabIndex = 3
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' DdfPartnersView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyRoot)
        Margin = New Padding(0)
        Name = "DdfPartnersView"
        Size = New Size(973, 420)
        tlyRoot.ResumeLayout(False)
        tlyPick.ResumeLayout(False)
        CType(grd, ComponentModel.ISupportInitialize).EndInit()
        tlyBottom.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents tlyRoot As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents tlyPick As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblPick As Label
    Friend WithEvents cmbPartner As Global.KBot.Controls.KBotComboBox
    Friend WithEvents btnAdd As Button
    Friend WithEvents grd As Global.KBot.Controls.KBotDataView
    Friend WithEvents tlyBottom As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblState As Label
    Friend WithEvents btnRemove As Button
End Class
