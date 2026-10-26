' The «Cereri conturi noi» page (slice ME01): the registration requests, their approval plan, approve /
' reject / new password link. It replaces the web page /operator, which no longer exists. All controls are
' declared HERE; coordinates are in the 96 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RequestsView
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
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        cmbState = New KBotComboBox()
        btnRefresh = New KBotButton()
        btnApprove = New KBotButton()
        btnReject = New KBotButton()
        btnLink = New KBotButton()
        grid = New KBotDataView()
        tlyEdit = New KBotTableLayoutPanel()
        lblDb = New Label()
        txtDb = New KBotTextField()
        lblCodes = New Label()
        txtCodes = New KBotTextField()
        btnPlan = New KBotButton()
        txtDetail = New KBotTextBox()
        lblState = New Label()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        tlyEdit.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(grid, 0, 1)
        tlyMain.Controls.Add(tlyEdit, 0, 2)
        tlyMain.Controls.Add(txtDetail, 0, 3)
        tlyMain.Controls.Add(lblState, 0, 4)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 5
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 38F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 62F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 6
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(cmbState, 0, 0)
        tlyBar.Controls.Add(btnRefresh, 1, 0)
        tlyBar.Controls.Add(btnReject, 2, 0)
        tlyBar.Controls.Add(btnApprove, 3, 0)
        tlyBar.Controls.Add(btnLink, 4, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' cmbState
        '
        cmbState.Dock = DockStyle.Fill
        cmbState.Location = New Point(0, 0)
        cmbState.Margin = New Padding(0, 4, 8, 8)
        cmbState.Name = "cmbState"
        cmbState.Size = New Size(192, 32)
        cmbState.TabIndex = 0
        tips.SetToolTipHeader(cmbState, "Starea cererilor")
        tips.SetToolTipText(cmbState, "În așteptare, eșuate (au încercat și s-au desfăcut), aprobate, respinse sau toate.")
        '
        ' btnRefresh
        '
        tips.SetToolTipHeader(btnRefresh, "Reîmprospătează")
        tips.SetToolTipText(btnRefresh, "Recitește cererile de pe server.")
        btnRefresh.Dock = DockStyle.Fill
        btnRefresh.Location = New Point(200, 0)
        btnRefresh.Margin = New Padding(0, 2, 6, 6)
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Size = New Size(144, 36)
        btnRefresh.TabIndex = 1
        btnRefresh.Text = "Reîmprospătează"
        '
        ' btnReject
        '
        btnReject.Dock = DockStyle.Fill
        btnReject.Location = New Point(350, 0)
        btnReject.Margin = New Padding(0, 2, 6, 6)
        btnReject.Name = "btnReject"
        btnReject.Size = New Size(124, 36)
        btnReject.TabIndex = 2
        btnReject.Text = "Respinge…"
        tips.SetToolTipHeader(btnReject, "Respinge")
        tips.SetToolTipText(btnReject, "Respinge cererea aleasă. Motivul se cere și îl primește solicitantul pe e-mail." & vbLf & "Merge doar pe cereri în așteptare sau eșuate.")
        '
        ' btnApprove
        '
        btnApprove.Dock = DockStyle.Fill
        btnApprove.Location = New Point(480, 0)
        btnApprove.Margin = New Padding(0, 2, 6, 6)
        btnApprove.Name = "btnApprove"
        btnApprove.Primary = True
        btnApprove.Size = New Size(124, 36)
        btnApprove.TabIndex = 3
        btnApprove.Text = "Aprobă"
        tips.SetToolTipHeader(btnApprove, "Aprobă")
        tips.SetToolTipText(btnApprove, "Creează baza, rândurile de clasificații și contul, apoi trimite linkul de parolă." & vbLf & "Folosește numele bazei și CodProgram de mai jos, exact cum sunt scrise.")
        '
        ' btnLink
        '
        btnLink.Dock = DockStyle.Fill
        btnLink.Location = New Point(610, 0)
        btnLink.Margin = New Padding(0, 2, 6, 6)
        btnLink.Name = "btnLink"
        btnLink.Size = New Size(184, 36)
        btnLink.TabIndex = 4
        btnLink.Text = "Link nou de parolă"
        tips.SetToolTipHeader(btnLink, "Link nou de parolă")
        tips.SetToolTipText(btnLink, "Pentru o cerere aprobată al cărei link s-a pierdut sau a expirat." & vbLf & "Cel vechi nu mai funcționează.")
        '
        ' grid
        '
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Nr."
        KBotDataColumn1.HeaderTooltip = "Numărul cererii."
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "id"
        KBotDataColumn1.MinWidth = 40
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn1.Width = 60
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Data cererii"
        KBotDataColumn2.HeaderTooltip = "Când a fost trimisă cererea."
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "date"
        KBotDataColumn2.MinWidth = 80
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 130
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Starea"
        KBotDataColumn3.HeaderTooltip = "În așteptare, aprobată sau respinsă."
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "state"
        KBotDataColumn3.MinWidth = 60
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 100
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Unitatea"
        KBotDataColumn4.HeaderTooltip = "Numele unității care cere cont."
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "name"
        KBotDataColumn4.MinWidth = 100
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 240
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "E-mail"
        KBotDataColumn5.HeaderTooltip = "Adresa solicitantului; aici ajunge linkul de parolă sau motivul respingerii."
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "email"
        KBotDataColumn5.MinWidth = 100
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 210
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "CF"
        KBotDataColumn6.HeaderTooltip = "Codul fiscal al unității."
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "cf"
        KBotDataColumn6.MinWidth = 60
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 90
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "An"
        KBotDataColumn7.HeaderTooltip = "Anul de lucru cerut."
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "year"
        KBotDataColumn7.MinWidth = 40
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Width = 60
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Rânduri"
        KBotDataColumn8.HeaderTooltip = "Numărul de rânduri de date trimise cu cererea."
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "rows"
        KBotDataColumn8.MinWidth = 50
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn8.Width = 80
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Baza creată / decis de"
        KBotDataColumn9.HeaderTooltip = "Baza creată la aprobare sau cine a decis."
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "decision"
        KBotDataColumn9.MinWidth = 100
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.Width = 240
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
        grid.FillColumnKey = "decision"
        grid.HeaderHeight = 24
        grid.Location = New Point(8, 52)
        grid.Margin = New Padding(0, 0, 0, 6)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.Size = New Size(884, 190)
        grid.TabIndex = 1
        tips.SetToolTipHeader(grid, "Cererile")
        tips.SetToolTipText(grid, "Cele mai noi sus. Alegeți un rând: dedesubt se vede cererea cu planul aprobării.")
        '
        ' tlyEdit
        '
        tlyEdit.AutoFitToTheme = False
        tlyEdit.ColumnCount = 5
        tlyEdit.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 56F))
        tlyEdit.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyEdit.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 96F))
        tlyEdit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyEdit.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlyEdit.Controls.Add(lblDb, 0, 0)
        tlyEdit.Controls.Add(txtDb, 1, 0)
        tlyEdit.Controls.Add(lblCodes, 2, 0)
        tlyEdit.Controls.Add(txtCodes, 3, 0)
        tlyEdit.Controls.Add(btnPlan, 4, 0)
        tlyEdit.Dock = DockStyle.Fill
        tlyEdit.Location = New Point(8, 248)
        tlyEdit.Margin = New Padding(0)
        tlyEdit.Name = "tlyEdit"
        tlyEdit.RowCount = 1
        tlyEdit.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyEdit.Size = New Size(884, 44)
        tlyEdit.TabIndex = 2
        '
        ' lblDb
        '
        tips.SetToolTipHeader(lblDb, "Baza")
        tips.SetToolTipText(lblDb, "Numele bazei care se creează la aprobare. Se poate corecta înainte.")
        lblDb.Dock = DockStyle.Fill
        lblDb.Location = New Point(0, 0)
        lblDb.Margin = New Padding(0)
        lblDb.Name = "lblDb"
        lblDb.Size = New Size(56, 44)
        lblDb.TabIndex = 0
        lblDb.Text = "Baza:"
        lblDb.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtDb
        '
        txtDb.BackColor = Color.Transparent
        txtDb.Dock = DockStyle.Fill
        txtDb.Location = New Point(56, 0)
        txtDb.Margin = New Padding(0, 4, 8, 6)
        txtDb.Name = "txtDb"
        txtDb.Size = New Size(142, 34)
        txtDb.TextPadding = New Padding(8, 0, 8, 0)
        txtDb.TabIndex = 1
        tips.SetToolTipHeader(txtDb, "Numele bazei")
        tips.SetToolTipText(txtDb, "Forma 1nn_SSSS. Propunerea serverului; o puteți schimba înainte de aprobare." & vbLf & "Se creează exact numele scris aici.")
        '
        ' lblCodes
        '
        tips.SetToolTipHeader(lblCodes, "CodProgram")
        tips.SetToolTipText(lblCodes, "Perechi SS=COD separate prin «;». Se pot corecta înainte de aprobare.")
        lblCodes.Dock = DockStyle.Fill
        lblCodes.Location = New Point(206, 0)
        lblCodes.Margin = New Padding(0)
        lblCodes.Name = "lblCodes"
        lblCodes.Size = New Size(96, 44)
        lblCodes.TabIndex = 2
        lblCodes.Text = "CodProgram:"
        lblCodes.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtCodes
        '
        txtCodes.BackColor = Color.Transparent
        txtCodes.Dock = DockStyle.Fill
        txtCodes.Location = New Point(302, 0)
        txtCodes.Margin = New Padding(0, 4, 8, 6)
        txtCodes.Name = "txtCodes"
        txtCodes.Size = New Size(414, 34)
        txtCodes.TextPadding = New Padding(8, 0, 8, 0)
        txtCodes.TabIndex = 3
        tips.SetToolTipHeader(txtCodes, "CodProgram")
        tips.SetToolTipText(txtCodes, "Perechi SURSA-SECTOR=COD separate prin «;». Propunerea serverului; schimbați doar ce trebuie." & vbLf & "Exemplu: 02A-01=ABC;02B-01=DEF")
        '
        ' btnPlan
        '
        btnPlan.Dock = DockStyle.Fill
        btnPlan.Location = New Point(724, 0)
        btnPlan.Margin = New Padding(0, 2, 0, 6)
        btnPlan.Name = "btnPlan"
        btnPlan.Size = New Size(160, 36)
        btnPlan.TabIndex = 4
        btnPlan.Text = "Revalidează"
        tips.SetToolTipHeader(btnPlan, "Revalidează")
        tips.SetToolTipText(btnPlan, "Rulează din nou toate verificările cererii (inclusiv drepturile contului de provizionare), cu numele și CodProgram de lângă." & vbLf & "Pentru o cerere eșuată: arată dacă cauza a dispărut. Nu scrie nimic.")
        '
        ' txtDetail
        '
        txtDetail.CornerRadius = 0
        txtDetail.Dock = DockStyle.Fill
        txtDetail.Font = New Font("Consolas", 9.75F)
        txtDetail.Location = New Point(8, 292)
        txtDetail.Margin = New Padding(0)
        txtDetail.MaxLength = 1000000
        txtDetail.Name = "txtDetail"
        txtDetail.ReadOnly = True
        txtDetail.ScrollBars = ScrollBars.Both
        txtDetail.Size = New Size(884, 240)
        txtDetail.TabIndex = 3
        tips.SetToolTipHeader(txtDetail, "Cererea și planul")
        tips.SetToolTipText(txtDetail, "Datele solicitantului, clasificațiile cerute și ce ar face aprobarea. La aprobare, aici curge mersul ei.")
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Rezultatul ultimei acțiuni asupra cererii alese.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 532)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 28)
        lblState.TabIndex = 4
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' RequestsView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "RequestsView"
        Size = New Size(900, 560)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        tlyEdit.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents cmbState As KBotComboBox
    Friend WithEvents btnRefresh As KBotButton
    Friend WithEvents btnApprove As KBotButton
    Friend WithEvents btnReject As KBotButton
    Friend WithEvents btnLink As KBotButton
    Friend WithEvents grid As KBotDataView
    Friend WithEvents tlyEdit As KBotTableLayoutPanel
    Friend WithEvents lblDb As Label
    Friend WithEvents txtDb As KBotTextField
    Friend WithEvents lblCodes As Label
    Friend WithEvents txtCodes As KBotTextField
    Friend WithEvents btnPlan As KBotButton
    Friend WithEvents txtDetail As KBotTextBox
    Friend WithEvents lblState As Label
End Class
