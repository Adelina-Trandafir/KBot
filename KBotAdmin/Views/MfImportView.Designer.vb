' The «Import MF din Access» page (VERCON, sliceless): the active fixed assets of an old mf.mdb into an MF gestiune.
' All controls are declared HERE; coordinates are in the 96 dpi it was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MfImportView
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
        Dim KBotDataColumn9 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        txtFile = New KBotTextBox()
        btnBrowse = New KBotButton()
        btnRead = New KBotButton()
        btnTargets = New KBotButton()
        cmbSubunit = New KBotComboBox()
        cmbGestiune = New KBotComboBox()
        cmbAccount = New KBotComboBox()
        btnCheck = New KBotButton()
        btnImport = New KBotButton()
        btnCreate = New KBotButton()
        lblState = New Label()
        tlyMain = New KBotTableLayoutPanel()
        tlyFile = New KBotTableLayoutPanel()
        tlyTarget = New KBotTableLayoutPanel()
        lblSubunit = New Label()
        lblGestiune = New Label()
        lblAccount = New Label()
        tlyCreate = New KBotTableLayoutPanel()
        lblNewGestiune = New Label()
        txtNewGestiune = New KBotTextBox()
        lblCreateDefaults = New Label()
        grid = New KBotDataView()
        tlyMain.SuspendLayout()
        tlyFile.SuspendLayout()
        tlyTarget.SuspendLayout()
        tlyCreate.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' txtFile
        ' 
        txtFile.Dock = DockStyle.Fill
        txtFile.Location = New Point(0, 4)
        txtFile.Margin = New Padding(0, 4, 0, 4)
        txtFile.Name = "txtFile"
        txtFile.Size = New Size(500, 40)
        txtFile.TabIndex = 2
        txtFile.TextPadding = New Padding(0)
        tips.SetToolTipHeader(txtFile, "Fișierul Access")
        tips.SetToolTipText(txtFile, "Calea către mf.mdb. Fișierul se copiază într-un loc temporar și se citește de acolo; originalul nu se atinge.")
        ' 
        ' btnBrowse
        ' 
        btnBrowse.AccessibleRole = AccessibleRole.PushButton
        btnBrowse.Dock = DockStyle.Fill
        btnBrowse.Location = New Point(727, 4)
        btnBrowse.Margin = New Padding(0, 4, 0, 4)
        btnBrowse.Name = "btnBrowse"
        btnBrowse.Size = New Size(200, 40)
        btnBrowse.TabIndex = 3
        btnBrowse.Text = "Alege fișier..."
        tips.SetToolTipHeader(btnBrowse, "Alege fișier")
        tips.SetToolTipText(btnBrowse, "Alege fișierul mf.mdb din vechiul sistem de mijloace fixe.")
        ' 
        ' btnRead
        ' 
        btnRead.AccessibleRole = AccessibleRole.PushButton
        btnRead.Dock = DockStyle.Fill
        btnRead.Location = New Point(927, 4)
        btnRead.Margin = New Padding(0, 4, 0, 4)
        btnRead.Name = "btnRead"
        btnRead.Size = New Size(200, 40)
        btnRead.TabIndex = 4
        btnRead.Text = "Citește mijloacele fixe"
        tips.SetToolTipHeader(btnRead, "Citește mijloacele fixe")
        tips.SetToolTipText(btnRead, "Citește din fișier mijloacele fixe care nu sunt șterse. Istoricul amortizării și ieșirile nu se citesc: VERCON calculează amortizarea la afișare.")
        ' 
        ' btnTargets
        ' 
        btnTargets.AccessibleRole = AccessibleRole.PushButton
        btnTargets.Dock = DockStyle.Fill
        btnTargets.Location = New Point(1127, 4)
        btnTargets.Margin = New Padding(0, 4, 0, 4)
        btnTargets.Name = "btnTargets"
        btnTargets.Size = New Size(200, 40)
        btnTargets.TabIndex = 5
        btnTargets.Text = "Citește subunitățile"
        tips.SetToolTipHeader(btnTargets, "Citește subunitățile")
        tips.SetToolTipText(btnTargets, "Citește de pe server subunitățile, gestiunile de mijloace fixe și conturile de magazie ale unității bifate în arborele din stânga (o singură unitate).")
        ' 
        ' cmbSubunit
        ' 
        cmbSubunit.Dock = DockStyle.Fill
        cmbSubunit.Location = New Point(135, 6)
        cmbSubunit.Margin = New Padding(0, 6, 12, 12)
        cmbSubunit.Name = "cmbSubunit"
        cmbSubunit.Size = New Size(333, 37)
        cmbSubunit.TabIndex = 8
        tips.SetToolTipHeader(cmbSubunit, "Subunitatea")
        tips.SetToolTipText(cmbSubunit, "Subunitatea în care intră mijloacele fixe.")
        ' 
        ' cmbGestiune
        ' 
        cmbGestiune.Dock = DockStyle.Fill
        cmbGestiune.Location = New Point(600, 6)
        cmbGestiune.Margin = New Padding(0, 6, 12, 12)
        cmbGestiune.Name = "cmbGestiune"
        cmbGestiune.Size = New Size(333, 37)
        cmbGestiune.TabIndex = 10
        tips.SetToolTipHeader(cmbGestiune, "Gestiunea")
        tips.SetToolTipText(cmbGestiune, "Gestiunea de mijloace fixe (tip MF) a subunității alese.")
        ' 
        ' cmbAccount
        ' 
        cmbAccount.Dock = DockStyle.Fill
        cmbAccount.Location = New Point(1110, 6)
        cmbAccount.Margin = New Padding(0, 6, 12, 12)
        cmbAccount.Name = "cmbAccount"
        cmbAccount.Size = New Size(378, 37)
        cmbAccount.TabIndex = 12
        tips.SetToolTipHeader(cmbAccount, "Contul de magazie")
        tips.SetToolTipText(cmbAccount, "Contul în care intră toate mijloacele fixe importate. Ele rămân în gestiune, fără primitor, până la bonul de predare.")
        ' 
        ' btnCheck
        ' 
        btnCheck.AccessibleRole = AccessibleRole.PushButton
        btnCheck.Dock = DockStyle.Fill
        btnCheck.Location = New Point(1500, 3)
        btnCheck.Margin = New Padding(0, 3, 9, 9)
        btnCheck.Name = "btnCheck"
        btnCheck.Size = New Size(171, 36)
        btnCheck.TabIndex = 13
        btnCheck.Text = "Verifică"
        tips.SetToolTipHeader(btnCheck, "Verifică")
        tips.SetToolTipText(btnCheck, "Verifică rândurile bifate pe server (clasificare, durată, număr de inventar, cota TVA 0) fără să scrie nimic.")
        ' 
        ' btnImport
        ' 
        btnImport.AccessibleRole = AccessibleRole.PushButton
        btnImport.Dock = DockStyle.Fill
        btnImport.Location = New Point(1680, 3)
        btnImport.Margin = New Padding(0, 3, 9, 9)
        btnImport.Name = "btnImport"
        btnImport.Primary = True
        btnImport.Size = New Size(276, 36)
        btnImport.TabIndex = 14
        btnImport.Text = "Importă rândurile bifate"
        tips.SetToolTipHeader(btnImport, "Importă")
        tips.SetToolTipText(btnImport, "Scrie în gestiune câte un document «Stoc inițial» pentru fiecare mijloc fix bifat, datat la data intrării lui." & vbLf & "Totul sau nimic: dacă un rând e refuzat, nu se scrie niciunul.")
        ' 
        ' btnCreate
        ' 
        btnCreate.AccessibleRole = AccessibleRole.PushButton
        btnCreate.Dock = DockStyle.Fill
        btnCreate.Location = New Point(510, 0)
        btnCreate.Margin = New Padding(0)
        btnCreate.Name = "btnCreate"
        btnCreate.Size = New Size(292, 48)
        btnCreate.TabIndex = 2
        btnCreate.Text = "Creează gestiune MF"
        tips.SetToolTipHeader(btnCreate, "Creează gestiune MF")
        tips.SetToolTipText(btnCreate, "Creează în subunitatea selectată gestiunea MF, conturile MFM/MFF, TVA 0% și 21%, exercițiul multianual și numerotarea. Apoi selectează noua destinație pentru import.")
        ' 
        ' lblState
        ' 
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 570)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(1327, 42)
        lblState.TabIndex = 16
        lblState.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Ce s-a citit, ce s-a verificat și ce s-a importat.")
        ' 
        ' tlyMain
        ' 
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyFile, 0, 0)
        tlyMain.Controls.Add(tlyTarget, 0, 1)
        tlyMain.Controls.Add(tlyCreate, 0, 2)
        tlyMain.Controls.Add(grid, 0, 4)
        tlyMain.Controls.Add(lblState, 0, 5)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 6
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyMain.Size = New Size(1343, 620)
        tlyMain.TabIndex = 0
        ' 
        ' tlyFile
        ' 
        tlyFile.AutoFitToTheme = False
        tlyFile.ColumnCount = 5
        tlyFile.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 500F))
        tlyFile.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFile.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyFile.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyFile.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyFile.Controls.Add(txtFile, 0, 0)
        tlyFile.Controls.Add(btnBrowse, 2, 0)
        tlyFile.Controls.Add(btnRead, 3, 0)
        tlyFile.Controls.Add(btnTargets, 4, 0)
        tlyFile.Dock = DockStyle.Fill
        tlyFile.Location = New Point(8, 8)
        tlyFile.Margin = New Padding(0)
        tlyFile.Name = "tlyFile"
        tlyFile.RowCount = 1
        tlyFile.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyFile.Size = New Size(1327, 48)
        tlyFile.TabIndex = 1
        ' 
        ' tlyTarget
        ' 
        tlyTarget.AutoFitToTheme = False
        tlyTarget.ColumnCount = 9
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 135F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 345F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 345F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 165F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 390F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 180F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 285F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyTarget.Controls.Add(lblSubunit, 0, 0)
        tlyTarget.Controls.Add(cmbSubunit, 1, 0)
        tlyTarget.Controls.Add(lblGestiune, 2, 0)
        tlyTarget.Controls.Add(cmbGestiune, 3, 0)
        tlyTarget.Controls.Add(lblAccount, 4, 0)
        tlyTarget.Controls.Add(cmbAccount, 5, 0)
        tlyTarget.Controls.Add(btnCheck, 6, 0)
        tlyTarget.Controls.Add(btnImport, 7, 0)
        tlyTarget.Dock = DockStyle.Fill
        tlyTarget.Location = New Point(8, 56)
        tlyTarget.Margin = New Padding(0)
        tlyTarget.Name = "tlyTarget"
        tlyTarget.RowCount = 1
        tlyTarget.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyTarget.Size = New Size(1327, 48)
        tlyTarget.TabIndex = 6
        ' 
        ' lblSubunit
        ' 
        lblSubunit.Dock = DockStyle.Fill
        lblSubunit.Location = New Point(0, 0)
        lblSubunit.Margin = New Padding(0)
        lblSubunit.Name = "lblSubunit"
        lblSubunit.Size = New Size(135, 48)
        lblSubunit.TabIndex = 7
        lblSubunit.Text = "Subunitate"
        lblSubunit.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblGestiune
        ' 
        lblGestiune.Dock = DockStyle.Fill
        lblGestiune.Location = New Point(480, 0)
        lblGestiune.Margin = New Padding(0)
        lblGestiune.Name = "lblGestiune"
        lblGestiune.Size = New Size(120, 48)
        lblGestiune.TabIndex = 9
        lblGestiune.Text = "Gestiune"
        lblGestiune.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblAccount
        ' 
        lblAccount.Dock = DockStyle.Fill
        lblAccount.Location = New Point(945, 0)
        lblAccount.Margin = New Padding(0)
        lblAccount.Name = "lblAccount"
        lblAccount.Size = New Size(165, 48)
        lblAccount.TabIndex = 11
        lblAccount.Text = "Cont magazie"
        lblAccount.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' tlyCreate
        ' 
        tlyCreate.AutoFitToTheme = False
        tlyCreate.ColumnCount = 4
        tlyCreate.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 165F))
        tlyCreate.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 345F))
        tlyCreate.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 292F))
        tlyCreate.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCreate.Controls.Add(lblNewGestiune, 0, 0)
        tlyCreate.Controls.Add(txtNewGestiune, 1, 0)
        tlyCreate.Controls.Add(btnCreate, 2, 0)
        tlyCreate.Controls.Add(lblCreateDefaults, 3, 0)
        tlyCreate.Dock = DockStyle.Fill
        tlyCreate.Location = New Point(8, 104)
        tlyCreate.Margin = New Padding(0)
        tlyCreate.Name = "tlyCreate"
        tlyCreate.RowCount = 1
        tlyCreate.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyCreate.Size = New Size(1327, 48)
        tlyCreate.TabIndex = 7
        ' 
        ' lblNewGestiune
        ' 
        lblNewGestiune.Dock = DockStyle.Fill
        lblNewGestiune.Location = New Point(4, 0)
        lblNewGestiune.Margin = New Padding(4, 0, 4, 0)
        lblNewGestiune.Name = "lblNewGestiune"
        lblNewGestiune.Size = New Size(157, 48)
        lblNewGestiune.TabIndex = 0
        lblNewGestiune.Text = "Gestiune nouă"
        lblNewGestiune.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtNewGestiune
        ' 
        txtNewGestiune.Dock = DockStyle.Fill
        txtNewGestiune.Location = New Point(165, 0)
        txtNewGestiune.Margin = New Padding(0)
        txtNewGestiune.MaxLength = 100
        txtNewGestiune.Name = "txtNewGestiune"
        txtNewGestiune.Size = New Size(345, 48)
        txtNewGestiune.TabIndex = 1
        txtNewGestiune.Text = "Mijloace Fixe"
        ' 
        ' lblCreateDefaults
        ' 
        lblCreateDefaults.Dock = DockStyle.Fill
        lblCreateDefaults.Location = New Point(802, 0)
        lblCreateDefaults.Margin = New Padding(0)
        lblCreateDefaults.Name = "lblCreateDefaults"
        lblCreateDefaults.Size = New Size(525, 48)
        lblCreateDefaults.TabIndex = 3
        lblCreateDefaults.Text = "Mijloace Fixe in Magazie (MFM) · Mijloace Fixe in Folosinta (MFF)" & vbLf & "TVA 0% și 21% · exercițiu multianual de la primul an citit"
        lblCreateDefaults.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' grid
        ' 
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.BackColor = SystemColors.Window
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Alege"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "sel"
        KBotDataColumn1.MinWidth = 50
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Width = 64
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Nr. inventar"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "code"
        KBotDataColumn2.MinWidth = 70
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 110
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Denumire"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "name"
        KBotDataColumn3.MinWidth = 100
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 260
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Data intrării"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "entry"
        KBotDataColumn4.MinWidth = 70
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Valoare"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "value"
        KBotDataColumn5.MinWidth = 70
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.Width = 110
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "DNF"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "dnf"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.Width = 60
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "DF"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "df"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.Width = 60
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Clasificare"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "cc"
        KBotDataColumn8.MinWidth = 70
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 110
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Verificare"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "verdict"
        KBotDataColumn9.MinWidth = 80
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.Width = 360
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Columns.Add(KBotDataColumn6)
        grid.Columns.Add(KBotDataColumn7)
        grid.Columns.Add(KBotDataColumn8)
        grid.Columns.Add(KBotDataColumn9)
        grid.Dock = DockStyle.Fill
        grid.FillColumnKey = "verdict"
        grid.HeaderHeight = 24
        grid.Location = New Point(8, 172)
        grid.Margin = New Padding(0)
        grid.Name = "grid"
        grid.Size = New Size(1327, 398)
        grid.TabIndex = 15
        ' 
        ' MfImportView
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Margin = New Padding(4)
        Name = "MfImportView"
        Size = New Size(1343, 620)
        tlyMain.ResumeLayout(False)
        tlyFile.ResumeLayout(False)
        tlyTarget.ResumeLayout(False)
        tlyCreate.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyFile As KBotTableLayoutPanel
    Friend WithEvents txtFile As KBotTextBox
    Friend WithEvents btnBrowse As KBotButton
    Friend WithEvents btnRead As KBotButton
    Friend WithEvents btnTargets As KBotButton
    Friend WithEvents tlyTarget As KBotTableLayoutPanel
    Friend WithEvents lblSubunit As Label
    Friend WithEvents cmbSubunit As KBotComboBox
    Friend WithEvents lblGestiune As Label
    Friend WithEvents cmbGestiune As KBotComboBox
    Friend WithEvents lblAccount As Label
    Friend WithEvents cmbAccount As KBotComboBox
    Friend WithEvents btnCheck As KBotButton
    Friend WithEvents btnImport As KBotButton
    Friend WithEvents tlyCreate As KBotTableLayoutPanel
    Friend WithEvents lblNewGestiune As Label
    Friend WithEvents txtNewGestiune As KBotTextBox
    Friend WithEvents btnCreate As KBotButton
    Friend WithEvents lblCreateDefaults As Label
    Friend WithEvents grid As KBotDataView
    Friend WithEvents lblState As Label
End Class
