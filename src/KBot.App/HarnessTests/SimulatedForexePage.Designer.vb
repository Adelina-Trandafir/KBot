#If DEBUG Then
' Slice 0125-02: the made-up FOREXE page the simulated runner docks into the «Browser FOREXE» view -- the angajament it
' shows, its indicator rows and the one thing the operator does there: save a reservation.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SimulatedForexePage
    Inherits KBot.Theming.KBotThemedUserControl

    Friend WithEvents pnlSus As Panel
    Friend WithEvents lblTitlu As Label
    Friend WithEvents lblAngajament As Label
    Friend WithEvents lstRanduri As ListView
    Friend WithEvents colCod As ColumnHeader
    Friend WithEvents colSursa As ColumnHeader
    Friend WithEvents colClasificatie As ColumnHeader
    Friend WithEvents colInitial As ColumnHeader
    Friend WithEvents colDefinitiv As ColumnHeader
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblIndicator As Label
    Friend WithEvents cmbIndicator As ComboBox
    Friend WithEvents lblValoare As Label
    Friend WithEvents txtValoare As TextBox
    Friend WithEvents lblMotiv As Label
    Friend WithEvents txtMotiv As TextBox
    Friend WithEvents btnSalveaza As Button
    Friend WithEvents lblStare As Label

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        pnlSus = New Panel()
        lblTitlu = New Label()
        lblAngajament = New Label()
        lstRanduri = New ListView()
        colCod = New ColumnHeader()
        colSursa = New ColumnHeader()
        colClasificatie = New ColumnHeader()
        colInitial = New ColumnHeader()
        colDefinitiv = New ColumnHeader()
        pnlJos = New Panel()
        lblIndicator = New Label()
        cmbIndicator = New ComboBox()
        lblValoare = New Label()
        txtValoare = New TextBox()
        lblMotiv = New Label()
        txtMotiv = New TextBox()
        btnSalveaza = New Button()
        lblStare = New Label()
        pnlSus.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlSus -- the banner and the angajament the page shows
        '
        pnlSus.Controls.Add(lblAngajament)
        pnlSus.Controls.Add(lblTitlu)
        pnlSus.Dock = DockStyle.Top
        pnlSus.Height = 62
        pnlSus.Name = "pnlSus"
        pnlSus.TabIndex = 0
        '
        ' lblTitlu
        '
        lblTitlu.AutoSize = False
        lblTitlu.Dock = DockStyle.Top
        lblTitlu.Font = New Font("Calibri", 11.0F, FontStyle.Bold)
        lblTitlu.ForeColor = Color.White
        lblTitlu.BackColor = Color.FromArgb(170, 30, 30)
        lblTitlu.Height = 28
        lblTitlu.Name = "lblTitlu"
        lblTitlu.Padding = New Padding(8, 0, 0, 0)
        lblTitlu.TabIndex = 0
        lblTitlu.Text = "SIMULARE — aceasta nu este pagina FOREXE reală"
        lblTitlu.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblAngajament
        '
        lblAngajament.AutoSize = False
        lblAngajament.Dock = DockStyle.Fill
        lblAngajament.Name = "lblAngajament"
        lblAngajament.Padding = New Padding(8, 0, 0, 0)
        lblAngajament.TabIndex = 1
        lblAngajament.Text = ""
        lblAngajament.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lstRanduri -- the indicator rows of the open angajament
        '
        lstRanduri.Columns.AddRange(New ColumnHeader() {colCod, colSursa, colClasificatie, colInitial, colDefinitiv})
        lstRanduri.Dock = DockStyle.Fill
        lstRanduri.FullRowSelect = True
        lstRanduri.GridLines = True
        lstRanduri.HideSelection = False
        lstRanduri.MultiSelect = False
        lstRanduri.Name = "lstRanduri"
        lstRanduri.TabIndex = 1
        lstRanduri.UseCompatibleStateImageBehavior = False
        lstRanduri.View = View.Details
        colCod.Text = "Cod"
        colCod.Width = 70
        colSursa.Text = "Sursa"
        colSursa.Width = 70
        colClasificatie.Text = "Clasificație"
        colClasificatie.Width = 200
        colInitial.Text = "CB inițial"
        colInitial.TextAlign = HorizontalAlignment.Right
        colInitial.Width = 110
        colDefinitiv.Text = "CB definitiv an curent"
        colDefinitiv.TextAlign = HorizontalAlignment.Right
        colDefinitiv.Width = 150
        '
        ' pnlJos -- the reservation: indicator, new value, motive, save
        '
        pnlJos.Controls.Add(lblStare)
        pnlJos.Controls.Add(btnSalveaza)
        pnlJos.Controls.Add(txtMotiv)
        pnlJos.Controls.Add(lblMotiv)
        pnlJos.Controls.Add(txtValoare)
        pnlJos.Controls.Add(lblValoare)
        pnlJos.Controls.Add(cmbIndicator)
        pnlJos.Controls.Add(lblIndicator)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Height = 150
        pnlJos.Name = "pnlJos"
        pnlJos.TabIndex = 2
        '
        ' lblIndicator
        '
        lblIndicator.AutoSize = True
        lblIndicator.Location = New Point(10, 14)
        lblIndicator.Name = "lblIndicator"
        lblIndicator.TabIndex = 0
        lblIndicator.Text = "Indicator"
        '
        ' cmbIndicator
        '
        cmbIndicator.DropDownStyle = ComboBoxStyle.DropDownList
        cmbIndicator.Location = New Point(90, 10)
        cmbIndicator.Name = "cmbIndicator"
        cmbIndicator.Size = New Size(110, 24)
        cmbIndicator.TabIndex = 1
        '
        ' lblValoare
        '
        lblValoare.AutoSize = True
        lblValoare.Location = New Point(220, 14)
        lblValoare.Name = "lblValoare"
        lblValoare.TabIndex = 2
        lblValoare.Text = "Valoare nouă"
        '
        ' txtValoare
        '
        txtValoare.Location = New Point(320, 10)
        txtValoare.Name = "txtValoare"
        txtValoare.Size = New Size(130, 24)
        txtValoare.TabIndex = 3
        txtValoare.TextAlign = HorizontalAlignment.Right
        '
        ' lblMotiv
        '
        lblMotiv.AutoSize = True
        lblMotiv.Location = New Point(10, 48)
        lblMotiv.Name = "lblMotiv"
        lblMotiv.TabIndex = 4
        lblMotiv.Text = "Motiv"
        '
        ' txtMotiv
        '
        txtMotiv.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtMotiv.Location = New Point(90, 44)
        txtMotiv.Name = "txtMotiv"
        txtMotiv.Size = New Size(560, 24)
        txtMotiv.TabIndex = 5
        '
        ' btnSalveaza
        '
        btnSalveaza.AutoSize = True
        btnSalveaza.Location = New Point(90, 80)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(8, 2, 8, 2)
        btnSalveaza.TabIndex = 6
        btnSalveaza.Text = "Salvează rezervarea"
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' lblStare
        '
        lblStare.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblStare.AutoSize = False
        lblStare.Location = New Point(260, 78)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(390, 60)
        lblStare.TabIndex = 7
        lblStare.Text = ""
        '
        ' SimulatedForexePage
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ' Children in REVERSE dock order: Fill first, then the docked edges.
        Controls.Add(lstRanduri)
        Controls.Add(pnlJos)
        Controls.Add(pnlSus)
        Name = "SimulatedForexePage"
        Size = New Size(660, 420)
        pnlSus.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        pnlJos.PerformLayout()
        ResumeLayout(False)
    End Sub

End Class
#End If
