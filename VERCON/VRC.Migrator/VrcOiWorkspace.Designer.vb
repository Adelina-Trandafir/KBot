<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VrcOiWorkspace
    Inherits System.Windows.Forms.UserControl

    Private components As System.ComponentModel.IContainer
    Friend WithEvents tlpOi As KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblOiInfo As Label
    Friend WithEvents tlpBrowse As KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents cmbDatabase As ComboBox
    Friend WithEvents cmbTable As ComboBox
    Friend WithEvents btnOpen As Button
    Friend WithEvents dgvBrowse As KBot.Controls.KBotDataView
    Friend WithEvents tlpChoices As KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnAccounts As Button
    Friend WithEvents lblName As Label
    Friend WithEvents dgvAccounts As KBot.Controls.KBotDataView
    Friend WithEvents tlpActions As KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnAnalyze As Button
    Friend WithEvents btnGenerate As Button
    Friend WithEvents btnDiscard As Button
    Friend WithEvents lblOiState As Label
    Friend WithEvents dgvRepeats As KBot.Controls.KBotDataView
    Friend WithEvents dgvHolders As KBot.Controls.KBotDataView
    Friend WithEvents rtbOi As RichTextBox

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        tlpOi = New KBot.Controls.KBotTableLayoutPanel()
        lblOiInfo = New Label()
        tlpBrowse = New KBot.Controls.KBotTableLayoutPanel()
        cmbDatabase = New ComboBox()
        cmbTable = New ComboBox()
        btnOpen = New Button()
        dgvBrowse = New KBot.Controls.KBotDataView()
        tlpChoices = New KBot.Controls.KBotTableLayoutPanel()
        btnAccounts = New Button()
        lblName = New Label()
        dgvAccounts = New KBot.Controls.KBotDataView()
        tlpActions = New KBot.Controls.KBotTableLayoutPanel()
        btnAnalyze = New Button()
        btnGenerate = New Button()
        btnDiscard = New Button()
        lblOiState = New Label()
        dgvRepeats = New KBot.Controls.KBotDataView()
        dgvHolders = New KBot.Controls.KBotDataView()
        rtbOi = New RichTextBox()
        Dim colSelect As New KBot.Controls.KBotDataColumn()
        Dim colSource As New KBot.Controls.KBotDataColumn()
        Dim colSubunit As New KBot.Controls.KBotDataColumn()
        Dim colGestiune As New KBot.Controls.KBotDataColumn()
        Dim colYear As New KBot.Controls.KBotDataColumn()
        Dim colAccount As New KBot.Controls.KBotDataColumn()
        Dim colUse As New KBot.Controls.KBotDataColumn()
        Dim colTargetType As New KBot.Controls.KBotDataColumn()
        Dim colTargetName As New KBot.Controls.KBotDataColumn()
        Dim colHolderSource As New KBot.Controls.KBotDataColumn()
        Dim colHolderYear As New KBot.Controls.KBotDataColumn()
        Dim colHolderOriginal As New KBot.Controls.KBotDataColumn()
        Dim colHolderTarget As New KBot.Controls.KBotDataColumn()
        Dim colHolderFirstDate As New KBot.Controls.KBotDataColumn()
        Dim colHolderFirstNumber As New KBot.Controls.KBotDataColumn()
        Dim colHolderDocuments As New KBot.Controls.KBotDataColumn()
        Dim colAccept As New KBot.Controls.KBotDataColumn()
        Dim colRepeatFile As New KBot.Controls.KBotDataColumn()
        Dim colPrevious As New KBot.Controls.KBotDataColumn()
        Dim colProduct As New KBot.Controls.KBotDataColumn()
        Dim colUnit As New KBot.Controls.KBotDataColumn()
        Dim colPrice As New KBot.Controls.KBotDataColumn()
        Dim colHolder As New KBot.Controls.KBotDataColumn()
        Dim colQuantity As New KBot.Controls.KBotDataColumn()
        SuspendLayout()
        CType(dgvBrowse, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(dgvAccounts, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(dgvRepeats, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(dgvHolders, System.ComponentModel.ISupportInitialize).BeginInit()
        tlpOi.Dock = DockStyle.Fill
        tlpOi.Padding = New Padding(8)
        tlpOi.ColumnCount = 1
        tlpOi.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        tlpOi.RowCount = 9
        tlpOi.RowStyles.Add(New RowStyle(SizeType.Absolute, 58))
        tlpOi.RowStyles.Add(New RowStyle(SizeType.Absolute, 36))
        tlpOi.RowStyles.Add(New RowStyle(SizeType.Percent, 30))
        tlpOi.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
        tlpOi.RowStyles.Add(New RowStyle(SizeType.Percent, 32))
        tlpOi.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
        tlpOi.RowStyles.Add(New RowStyle(SizeType.Absolute, 150))
        tlpOi.RowStyles.Add(New RowStyle(SizeType.Absolute, 100))
        tlpOi.RowStyles.Add(New RowStyle(SizeType.Percent, 38))
        tlpOi.Controls.Add(lblOiInfo, 0, 0)
        tlpOi.Controls.Add(tlpBrowse, 0, 1)
        tlpOi.Controls.Add(dgvBrowse, 0, 2)
        tlpOi.Controls.Add(tlpChoices, 0, 3)
        tlpOi.Controls.Add(dgvAccounts, 0, 4)
        tlpOi.Controls.Add(tlpActions, 0, 5)
        tlpOi.Controls.Add(dgvHolders, 0, 6)
        tlpOi.Controls.Add(dgvRepeats, 0, 7)
        tlpOi.Controls.Add(rtbOi, 0, 8)
        lblOiInfo.Dock = DockStyle.Fill
        lblOiInfo.Text = "Bifați conturile și alegeți pentru fiecare tipul și numele noii gestiuni. Numai OI transformă BC → BP + BC; Materiale păstrează documentele anuale. La deținători, alegeți același nume destinație pentru a asocia înregistrările; numele diferite rămân distincte."
        tlpBrowse.Dock = DockStyle.Fill
        tlpBrowse.ColumnCount = 3
        tlpBrowse.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 70))
        tlpBrowse.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30))
        tlpBrowse.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120))
        tlpBrowse.RowCount = 1
        tlpBrowse.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        tlpBrowse.Controls.Add(cmbDatabase, 0, 0)
        tlpBrowse.Controls.Add(cmbTable, 1, 0)
        tlpBrowse.Controls.Add(btnOpen, 2, 0)
        cmbDatabase.Dock = DockStyle.Fill
        cmbDatabase.DropDownStyle = ComboBoxStyle.DropDownList
        cmbDatabase.FlatStyle = FlatStyle.Flat
        cmbDatabase.DisplayMember = "Path"
        cmbTable.Dock = DockStyle.Fill
        cmbTable.DropDownStyle = ComboBoxStyle.DropDownList
        cmbTable.FlatStyle = FlatStyle.Flat
        btnOpen.Dock = DockStyle.Fill
        btnOpen.FlatStyle = FlatStyle.Flat
        btnOpen.Text = "Deschide baza"
        dgvBrowse.Dock = DockStyle.Fill
        tlpChoices.Dock = DockStyle.Fill
        tlpChoices.ColumnCount = 3
        tlpChoices.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 185))
        tlpChoices.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 165))
        tlpChoices.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        tlpChoices.RowCount = 1
        tlpChoices.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        tlpChoices.Controls.Add(btnAccounts, 0, 0)
        tlpChoices.Controls.Add(lblName, 1, 0)
        tlpChoices.SetColumnSpan(lblName, 2)
        btnAccounts.Dock = DockStyle.Fill
        btnAccounts.FlatStyle = FlatStyle.Flat
        btnAccounts.Text = "Încarcă conturile din plan"
        lblName.Dock = DockStyle.Fill
        lblName.Text = "Alegeți tipul și numele destinației pe fiecare cont. OI cere și contul de folosință."
        lblName.TextAlign = ContentAlignment.MiddleLeft
        dgvAccounts.Dock = DockStyle.Fill
        colSelect.Key = "select"
        colSelect.HeaderText = "Separă"
        colSelect.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        colSelect.ValueType = KBot.Controls.KBotValueType.Boolean
        colSelect.Width = 65
        colSource.Key = "source"
        colSource.HeaderText = "Bază Access"
        colSource.ReadOnly = True
        colSource.Width = 380
        colSubunit.Key = "subunit"
        colSubunit.HeaderText = "Subunitate"
        colSubunit.ReadOnly = True
        colSubunit.Width = 170
        colGestiune.Key = "gestiune"
        colGestiune.HeaderText = "Gestiune"
        colGestiune.ReadOnly = True
        colGestiune.Width = 170
        colYear.Key = "year"
        colYear.HeaderText = "An"
        colYear.ReadOnly = True
        colYear.ValueType = KBot.Controls.KBotValueType.Number
        colYear.Width = 70
        colAccount.Key = "account"
        colAccount.HeaderText = "Cont din Materiale"
        colAccount.ReadOnly = True
        colAccount.Width = 250
        colUse.Key = "use"
        colUse.HeaderText = "Cont de folosință OI (completați)"
        colUse.Width = 280
        colTargetType.Key = "targettype"
        colTargetType.HeaderText = "Tip destinație (alegeți)"
        colTargetType.ColumnType = KBot.Controls.KBotColumnType.Combo
        colTargetType.ComboItems = New List(Of Object) From {"OI", "MATERIALE"}
        colTargetType.Width = 165
        colTargetName.Key = "targetname"
        colTargetName.HeaderText = "Gestiune nouă (același nume în toți anii)"
        colTargetName.Width = 280
        dgvAccounts.Columns.Add(colSelect)
        dgvAccounts.Columns.Add(colSource)
        dgvAccounts.Columns.Add(colSubunit)
        dgvAccounts.Columns.Add(colGestiune)
        dgvAccounts.Columns.Add(colYear)
        dgvAccounts.Columns.Add(colAccount)
        dgvAccounts.Columns.Add(colTargetType)
        dgvAccounts.Columns.Add(colTargetName)
        dgvAccounts.Columns.Add(colUse)
        dgvHolders.Dock = DockStyle.Fill
        colHolderSource.Key = "source"
        colHolderSource.HeaderText = "Deținători • baza sursă"
        colHolderSource.ReadOnly = True
        colHolderSource.Width = 380
        colHolderYear.Key = "year"
        colHolderYear.HeaderText = "An"
        colHolderYear.ReadOnly = True
        colHolderYear.ValueType = KBot.Controls.KBotValueType.Number
        colHolderYear.Width = 70
        colHolderOriginal.Key = "original"
        colHolderOriginal.HeaderText = "Numele în baza veche"
        colHolderOriginal.ReadOnly = True
        colHolderOriginal.Width = 260
        colHolderTarget.Key = "target"
        colHolderTarget.HeaderText = "Deținător în destinație • alegeți pentru asociere"
        colHolderTarget.ColumnType = KBot.Controls.KBotColumnType.Combo
        colHolderTarget.Width = 330
        colHolderFirstDate.Key = "firstdate"
        colHolderFirstDate.HeaderText = "Data primului bon"
        colHolderFirstDate.ReadOnly = True
        colHolderFirstDate.ValueType = KBot.Controls.KBotValueType.DateTime
        colHolderFirstDate.Width = 125
        colHolderFirstNumber.Key = "firstnumber"
        colHolderFirstNumber.HeaderText = "Număr primul bon"
        colHolderFirstNumber.ReadOnly = True
        colHolderFirstNumber.ValueType = KBot.Controls.KBotValueType.Number
        colHolderFirstNumber.Width = 125
        colHolderDocuments.Key = "documents"
        colHolderDocuments.HeaderText = "Bonuri implicate"
        colHolderDocuments.ReadOnly = True
        colHolderDocuments.ValueType = KBot.Controls.KBotValueType.Number
        colHolderDocuments.Width = 110
        dgvHolders.Columns.Add(colHolderSource)
        dgvHolders.Columns.Add(colHolderYear)
        dgvHolders.Columns.Add(colHolderOriginal)
        dgvHolders.Columns.Add(colHolderTarget)
        dgvHolders.Columns.Add(colHolderFirstDate)
        dgvHolders.Columns.Add(colHolderFirstNumber)
        dgvHolders.Columns.Add(colHolderDocuments)
        tlpActions.Dock = DockStyle.Fill
        tlpActions.ColumnCount = 4
        tlpActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150))
        tlpActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 185))
        tlpActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 165))
        tlpActions.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        tlpActions.RowCount = 1
        tlpActions.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        tlpActions.Controls.Add(btnAnalyze, 0, 0)
        tlpActions.Controls.Add(btnGenerate, 1, 0)
        tlpActions.Controls.Add(btnDiscard, 2, 0)
        tlpActions.Controls.Add(lblOiState, 3, 0)
        btnAnalyze.Dock = DockStyle.Fill
        btnAnalyze.FlatStyle = FlatStyle.Flat
        btnAnalyze.Text = "Analizează selecția"
        btnGenerate.Dock = DockStyle.Fill
        btnGenerate.FlatStyle = FlatStyle.Flat
        btnGenerate.Text = "Generează separarea"
        btnDiscard.Dock = DockStyle.Fill
        btnDiscard.FlatStyle = FlatStyle.Flat
        btnDiscard.Text = "Renunță la separare"
        lblOiState.Dock = DockStyle.Fill
        lblOiState.TextAlign = ContentAlignment.MiddleLeft
        lblOiState.Text = "Separarea nu este generată."
        dgvRepeats.Dock = DockStyle.Fill
        colAccept.Key = "accept"
        colAccept.HeaderText = "Predare distinctă confirmată"
        colAccept.Width = 185
        colAccept.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        colAccept.ValueType = KBot.Controls.KBotValueType.Boolean
        colRepeatFile.Key = "file"
        colRepeatFile.HeaderText = "Bază"
        colRepeatFile.ReadOnly = True
        colRepeatFile.Width = 240
        colPrevious.Key = "previous"
        colPrevious.HeaderText = "Predare anterioară în"
        colPrevious.ReadOnly = True
        colPrevious.Width = 240
        colProduct.Key = "product"
        colProduct.HeaderText = "Produs"
        colProduct.ReadOnly = True
        colProduct.Width = 280
        colUnit.Key = "unit"
        colUnit.HeaderText = "UM"
        colUnit.ReadOnly = True
        colUnit.Width = 65
        colPrice.Key = "price"
        colPrice.HeaderText = "Preț"
        colPrice.ReadOnly = True
        colPrice.ValueType = KBot.Controls.KBotValueType.Number
        colPrice.Width = 100
        colHolder.Key = "holder"
        colHolder.HeaderText = "Deținător"
        colHolder.ReadOnly = True
        colHolder.Width = 180
        colQuantity.Key = "quantity"
        colQuantity.HeaderText = "Cantitate"
        colQuantity.ReadOnly = True
        colQuantity.ValueType = KBot.Controls.KBotValueType.Number
        colQuantity.Width = 100
        dgvRepeats.Columns.Add(colAccept)
        dgvRepeats.Columns.Add(colRepeatFile)
        dgvRepeats.Columns.Add(colPrevious)
        dgvRepeats.Columns.Add(colProduct)
        dgvRepeats.Columns.Add(colUnit)
        dgvRepeats.Columns.Add(colPrice)
        dgvRepeats.Columns.Add(colHolder)
        dgvRepeats.Columns.Add(colQuantity)
        rtbOi.Dock = DockStyle.Fill
        rtbOi.ReadOnly = True
        rtbOi.BorderStyle = BorderStyle.FixedSingle
        rtbOi.Text = "Citiți bazele și completați planul din fila Migrare, apoi încărcați conturile aici. Analiza nu scrie în Access sau pe server."
        AutoScaleDimensions = New SizeF(96, 96)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlpOi)
        Name = "VrcOiWorkspace"
        Size = New Size(1100, 840)
        CType(dgvBrowse, System.ComponentModel.ISupportInitialize).EndInit()
        CType(dgvAccounts, System.ComponentModel.ISupportInitialize).EndInit()
        CType(dgvRepeats, System.ComponentModel.ISupportInitialize).EndInit()
        CType(dgvHolders, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then components?.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
End Class
