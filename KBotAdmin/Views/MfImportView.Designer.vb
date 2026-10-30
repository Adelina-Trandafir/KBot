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
        Dim gridCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol2 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol3 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol4 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol5 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol6 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol7 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol8 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol9 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyFile = New KBotTableLayoutPanel()
        txtFile = New KBotTextBox()
        btnBrowse = New KBotButton()
        btnRead = New KBotButton()
        btnTargets = New KBotButton()
        tlyTarget = New KBotTableLayoutPanel()
        lblSubunit = New Label()
        cmbSubunit = New KBotComboBox()
        lblGestiune = New Label()
        cmbGestiune = New KBotComboBox()
        lblAccount = New Label()
        cmbAccount = New KBotComboBox()
        btnCheck = New KBotButton()
        btnImport = New KBotButton()
        grid = New KBotDataView()
        lblState = New Label()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyFile.SuspendLayout()
        tlyTarget.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyFile, 0, 0)
        tlyMain.Controls.Add(tlyTarget, 0, 1)
        tlyMain.Controls.Add(grid, 0, 2)
        tlyMain.Controls.Add(lblState, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(1000, 600)
        tlyMain.TabIndex = 0
        '
        ' tlyFile
        '
        tlyFile.AutoFitToTheme = False
        tlyFile.ColumnCount = 4
        tlyFile.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFile.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyFile.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyFile.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyFile.Controls.Add(txtFile, 0, 0)
        tlyFile.Controls.Add(btnBrowse, 1, 0)
        tlyFile.Controls.Add(btnRead, 2, 0)
        tlyFile.Controls.Add(btnTargets, 3, 0)
        tlyFile.Dock = DockStyle.Fill
        tlyFile.Location = New Point(0, 0)
        tlyFile.Margin = New Padding(0)
        tlyFile.Name = "tlyFile"
        tlyFile.RowCount = 1
        tlyFile.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyFile.Size = New Size(984, 44)
        tlyFile.TabIndex = 1
        '
        ' txtFile
        '
        txtFile.Dock = DockStyle.Fill
        txtFile.Location = New Point(0, 0)
        txtFile.Margin = New Padding(0, 4, 6, 6)
        txtFile.Name = "txtFile"
        txtFile.Size = New Size(600, 34)
        txtFile.TabIndex = 2
        tips.SetToolTipHeader(txtFile, "Fișierul Access")
        tips.SetToolTipText(txtFile, "Calea către mf.mdb. Fișierul se copiază într-un loc temporar și se citește de acolo; originalul nu se atinge.")
        '
        ' btnBrowse
        '
        btnBrowse.Dock = DockStyle.Fill
        btnBrowse.Location = New Point(0, 0)
        btnBrowse.Margin = New Padding(0, 2, 6, 6)
        btnBrowse.Name = "btnBrowse"
        btnBrowse.Size = New Size(124, 36)
        btnBrowse.TabIndex = 3
        btnBrowse.Text = "Alege fișier..."
        tips.SetToolTipHeader(btnBrowse, "Alege fișier")
        tips.SetToolTipText(btnBrowse, "Alege fișierul mf.mdb din vechiul sistem de mijloace fixe.")
        '
        ' btnRead
        '
        btnRead.Dock = DockStyle.Fill
        btnRead.Location = New Point(0, 0)
        btnRead.Margin = New Padding(0, 2, 6, 6)
        btnRead.Name = "btnRead"
        btnRead.Size = New Size(224, 36)
        btnRead.TabIndex = 4
        btnRead.Text = "Citește mijloacele fixe"
        tips.SetToolTipHeader(btnRead, "Citește mijloacele fixe")
        tips.SetToolTipText(btnRead, "Citește din fișier mijloacele fixe care nu sunt șterse. Istoricul amortizării și ieșirile nu se citesc: VERCON calculează amortizarea la afișare.")
        '
        ' btnTargets
        '
        btnTargets.Dock = DockStyle.Fill
        btnTargets.Location = New Point(0, 0)
        btnTargets.Margin = New Padding(0, 2, 0, 6)
        btnTargets.Name = "btnTargets"
        btnTargets.Size = New Size(220, 36)
        btnTargets.TabIndex = 5
        btnTargets.Text = "Citește subunitățile"
        tips.SetToolTipHeader(btnTargets, "Citește subunitățile")
        tips.SetToolTipText(btnTargets, "Citește de pe server subunitățile, gestiunile de mijloace fixe și conturile de magazie ale unității bifate în arborele din stânga (o singură unitate).")
        '
        ' tlyTarget
        '
        tlyTarget.AutoFitToTheme = False
        tlyTarget.ColumnCount = 9
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190F))
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
        tlyTarget.Location = New Point(0, 0)
        tlyTarget.Margin = New Padding(0)
        tlyTarget.Name = "tlyTarget"
        tlyTarget.RowCount = 1
        tlyTarget.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyTarget.Size = New Size(984, 44)
        tlyTarget.TabIndex = 6
        '
        ' lblSubunit
        '
        lblSubunit.Dock = DockStyle.Fill
        lblSubunit.Location = New Point(0, 0)
        lblSubunit.Margin = New Padding(0)
        lblSubunit.Name = "lblSubunit"
        lblSubunit.Size = New Size(90, 44)
        lblSubunit.TabIndex = 7
        lblSubunit.Text = "Subunitate"
        lblSubunit.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbSubunit
        '
        cmbSubunit.Dock = DockStyle.Fill
        cmbSubunit.Location = New Point(0, 0)
        cmbSubunit.Margin = New Padding(0, 4, 8, 8)
        cmbSubunit.Name = "cmbSubunit"
        cmbSubunit.Size = New Size(222, 32)
        cmbSubunit.TabIndex = 8
        tips.SetToolTipHeader(cmbSubunit, "Subunitatea")
        tips.SetToolTipText(cmbSubunit, "Subunitatea în care intră mijloacele fixe.")
        '
        ' lblGestiune
        '
        lblGestiune.Dock = DockStyle.Fill
        lblGestiune.Location = New Point(0, 0)
        lblGestiune.Margin = New Padding(0)
        lblGestiune.Name = "lblGestiune"
        lblGestiune.Size = New Size(80, 44)
        lblGestiune.TabIndex = 9
        lblGestiune.Text = "Gestiune"
        lblGestiune.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbGestiune
        '
        cmbGestiune.Dock = DockStyle.Fill
        cmbGestiune.Location = New Point(0, 0)
        cmbGestiune.Margin = New Padding(0, 4, 8, 8)
        cmbGestiune.Name = "cmbGestiune"
        cmbGestiune.Size = New Size(222, 32)
        cmbGestiune.TabIndex = 10
        tips.SetToolTipHeader(cmbGestiune, "Gestiunea")
        tips.SetToolTipText(cmbGestiune, "Gestiunea de mijloace fixe (tip MF) a subunității alese.")
        '
        ' lblAccount
        '
        lblAccount.Dock = DockStyle.Fill
        lblAccount.Location = New Point(0, 0)
        lblAccount.Margin = New Padding(0)
        lblAccount.Name = "lblAccount"
        lblAccount.Size = New Size(110, 44)
        lblAccount.TabIndex = 11
        lblAccount.Text = "Cont magazie"
        lblAccount.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbAccount
        '
        cmbAccount.Dock = DockStyle.Fill
        cmbAccount.Location = New Point(0, 0)
        cmbAccount.Margin = New Padding(0, 4, 8, 8)
        cmbAccount.Name = "cmbAccount"
        cmbAccount.Size = New Size(252, 32)
        cmbAccount.TabIndex = 12
        tips.SetToolTipHeader(cmbAccount, "Contul de magazie")
        tips.SetToolTipText(cmbAccount, "Contul în care intră toate mijloacele fixe importate. Ele rămân în gestiune, fără primitor, până la bonul de predare.")
        '
        ' btnCheck
        '
        btnCheck.Dock = DockStyle.Fill
        btnCheck.Location = New Point(0, 0)
        btnCheck.Margin = New Padding(0, 2, 6, 6)
        btnCheck.Name = "btnCheck"
        btnCheck.Size = New Size(114, 36)
        btnCheck.TabIndex = 13
        btnCheck.Text = "Verifică"
        tips.SetToolTipHeader(btnCheck, "Verifică")
        tips.SetToolTipText(btnCheck, "Verifică rândurile bifate pe server (clasificare, durată, număr de inventar, cota TVA 0) fără să scrie nimic.")
        '
        ' btnImport
        '
        btnImport.Dock = DockStyle.Fill
        btnImport.Location = New Point(0, 0)
        btnImport.Margin = New Padding(0, 2, 6, 6)
        btnImport.Name = "btnImport"
        btnImport.Primary = True
        btnImport.Size = New Size(184, 36)
        btnImport.TabIndex = 14
        btnImport.Text = "Importă rândurile bifate"
        tips.SetToolTipHeader(btnImport, "Importă")
        tips.SetToolTipText(btnImport, "Scrie în gestiune câte un document «Stoc inițial» pentru fiecare mijloc fix bifat, datat la data intrării lui." & vbLf & "Totul sau nimic: dacă un rând e refuzat, nu se scrie niciunul.")
        '
        ' grid
        '
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        gridCol1.AggregateFormatString = Nothing
        gridCol1.ColumnType = KBotColumnType.CheckBox
        gridCol1.FormatString = Nothing
        gridCol1.HeaderText = "Alege"
        gridCol1.HeaderTooltip = "Bifați mijloacele fixe care se importă."
        gridCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol1.Key = "sel"
        gridCol1.MinWidth = 50
        gridCol1.OptionGroup = Nothing
        gridCol1.Width = 64
        gridCol2.AggregateFormatString = Nothing
        gridCol2.FormatString = Nothing
        gridCol2.HeaderText = "Nr. inventar"
        gridCol2.HeaderTooltip = "CodProdus din Access; devine numărul de inventar."
        gridCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol2.Key = "code"
        gridCol2.MinWidth = 70
        gridCol2.OptionGroup = Nothing
        gridCol2.ReadOnly = True
        gridCol2.Width = 110
        gridCol3.AggregateFormatString = Nothing
        gridCol3.FormatString = Nothing
        gridCol3.HeaderText = "Denumire"
        gridCol3.HeaderTooltip = "Denumirea mijlocului fix."
        gridCol3.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol3.Key = "name"
        gridCol3.MinWidth = 100
        gridCol3.OptionGroup = Nothing
        gridCol3.ReadOnly = True
        gridCol3.Width = 260
        gridCol4.AggregateFormatString = Nothing
        gridCol4.FormatString = Nothing
        gridCol4.HeaderText = "Data intrării"
        gridCol4.HeaderTooltip = "DataIntrare din Access; documentul de stoc inițial se datează la ea."
        gridCol4.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol4.Key = "entry"
        gridCol4.MinWidth = 70
        gridCol4.OptionGroup = Nothing
        gridCol4.ReadOnly = True
        gridCol4.Width = 100
        gridCol5.AggregateFormatString = Nothing
        gridCol5.FormatString = Nothing
        gridCol5.HeaderText = "Valoare"
        gridCol5.HeaderTooltip = "ValoareIntrare din Access, păstrată ca atare (fără TVA în plus)."
        gridCol5.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol5.Key = "value"
        gridCol5.MinWidth = 70
        gridCol5.OptionGroup = Nothing
        gridCol5.ReadOnly = True
        gridCol5.TextAlign = ContentAlignment.MiddleRight
        gridCol5.Width = 110
        gridCol6.AggregateFormatString = Nothing
        gridCol6.FormatString = Nothing
        gridCol6.HeaderText = "DNF"
        gridCol6.HeaderTooltip = "Durata normală de funcționare, în luni."
        gridCol6.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol6.Key = "dnf"
        gridCol6.MinWidth = 40
        gridCol6.OptionGroup = Nothing
        gridCol6.ReadOnly = True
        gridCol6.TextAlign = ContentAlignment.MiddleRight
        gridCol6.Width = 60
        gridCol7.AggregateFormatString = Nothing
        gridCol7.FormatString = Nothing
        gridCol7.HeaderText = "DF"
        gridCol7.HeaderTooltip = "Durata deja folosită înainte de prima predare, în luni."
        gridCol7.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol7.Key = "df"
        gridCol7.MinWidth = 40
        gridCol7.OptionGroup = Nothing
        gridCol7.ReadOnly = True
        gridCol7.TextAlign = ContentAlignment.MiddleRight
        gridCol7.Width = 60
        gridCol8.AggregateFormatString = Nothing
        gridCol8.FormatString = Nothing
        gridCol8.HeaderText = "Clasificare"
        gridCol8.HeaderTooltip = "Categorie din Access; trebuie să existe în catalogul comun al clasificărilor MF."
        gridCol8.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol8.Key = "cc"
        gridCol8.MinWidth = 70
        gridCol8.OptionGroup = Nothing
        gridCol8.ReadOnly = True
        gridCol8.Width = 110
        gridCol9.AggregateFormatString = Nothing
        gridCol9.FormatString = Nothing
        gridCol9.HeaderText = "Verificare"
        gridCol9.HeaderTooltip = "Rezultatul verificării de pe server."
        gridCol9.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol9.Key = "verdict"
        gridCol9.MinWidth = 80
        gridCol9.OptionGroup = Nothing
        gridCol9.ReadOnly = True
        gridCol9.Width = 360
        grid.Columns.Add(gridCol1)
        grid.Columns.Add(gridCol2)
        grid.Columns.Add(gridCol3)
        grid.Columns.Add(gridCol4)
        grid.Columns.Add(gridCol5)
        grid.Columns.Add(gridCol6)
        grid.Columns.Add(gridCol7)
        grid.Columns.Add(gridCol8)
        grid.Columns.Add(gridCol9)
        grid.Dock = DockStyle.Fill
        grid.FillColumnKey = "verdict"
        grid.HeaderHeight = 24
        grid.Location = New Point(0, 0)
        grid.Margin = New Padding(0)
        grid.Name = "grid"
        grid.Size = New Size(984, 300)
        grid.TabIndex = 15
        tips.SetToolTipHeader(grid, "Mijloace fixe")
        tips.SetToolTipText(grid, "Un rând = un mijloc fix activ din fișierul Access." & vbLf & "Rândurile refuzate la verificare apar în roșu, cu motivul în ultima coloană.")
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Ce s-a citit, ce s-a verificat și ce s-a importat.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(0, 0)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(984, 28)
        lblState.TabIndex = 16
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' MfImportView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "MfImportView"
        Size = New Size(1000, 600)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyFile.ResumeLayout(False)
        tlyTarget.ResumeLayout(False)
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
    Friend WithEvents grid As KBotDataView
    Friend WithEvents lblState As Label
End Class
