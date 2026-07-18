<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SumarView
    Inherits System.Windows.Forms.UserControl

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
        pnlHeader = New Panel()
        tblHeader = New TableLayoutPanel()
        lblCodCaption = New Label()
        lblCod = New Label()
        lblDataFxCaption = New Label()
        lblDataFx = New Label()
        lblDataCreareCaption = New Label()
        lblDataCreare = New Label()
        lblDataDefCaption = New Label()
        lblDataDef = New Label()
        lblStareCaption = New Label()
        lblStare = New Label()
        lblStatusCaption = New Label()
        lblStatus = New Label()
        lblDescriereCaption = New Label()
        lblDescriere = New Label()
        grid = New KBot.Controls.KBotDataView()
        lblEmpty = New Label()
        pnlHeader.SuspendLayout()
        tblHeader.SuspendLayout()
        SuspendLayout()
        '
        ' tblHeader — patru coloane: etichetă / valoare / etichetă / valoare.
        ' Ordinea rândurilor oglindește frmFX_MAIN_Sumar (Cod + DataFX pe primul rând).
        '
        tblHeader.ColumnCount = 4
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130.0F))
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130.0F))
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        tblHeader.RowCount = 4
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 26.0F))
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 26.0F))
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 26.0F))
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 26.0F))
        tblHeader.Dock = DockStyle.Fill
        tblHeader.Location = New Point(12, 10)
        tblHeader.Name = "tblHeader"
        tblHeader.Size = New Size(776, 104)
        tblHeader.TabIndex = 0
        '
        ' lblCodCaption
        '
        lblCodCaption.AutoSize = False
        lblCodCaption.Dock = DockStyle.Fill
        lblCodCaption.Name = "lblCodCaption"
        lblCodCaption.Text = "Cod angajament:"
        lblCodCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblCod
        '
        lblCod.AutoSize = False
        lblCod.Dock = DockStyle.Fill
        lblCod.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        lblCod.Name = "lblCod"
        lblCod.Text = ""
        lblCod.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblDataFxCaption
        '
        lblDataFxCaption.AutoSize = False
        lblDataFxCaption.Dock = DockStyle.Fill
        lblDataFxCaption.Name = "lblDataFxCaption"
        lblDataFxCaption.Text = "Data FX:"
        lblDataFxCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblDataFx
        '
        lblDataFx.AutoSize = False
        lblDataFx.Dock = DockStyle.Fill
        lblDataFx.Name = "lblDataFx"
        lblDataFx.Text = ""
        lblDataFx.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblDataCreareCaption
        '
        lblDataCreareCaption.AutoSize = False
        lblDataCreareCaption.Dock = DockStyle.Fill
        lblDataCreareCaption.Name = "lblDataCreareCaption"
        lblDataCreareCaption.Text = "Data creare:"
        lblDataCreareCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblDataCreare
        '
        lblDataCreare.AutoSize = False
        lblDataCreare.Dock = DockStyle.Fill
        lblDataCreare.Name = "lblDataCreare"
        lblDataCreare.Text = ""
        lblDataCreare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblDataDefCaption
        '
        lblDataDefCaption.AutoSize = False
        lblDataDefCaption.Dock = DockStyle.Fill
        lblDataDefCaption.Name = "lblDataDefCaption"
        lblDataDefCaption.Text = "Data definitivare:"
        lblDataDefCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblDataDef
        '
        lblDataDef.AutoSize = False
        lblDataDef.Dock = DockStyle.Fill
        lblDataDef.Name = "lblDataDef"
        lblDataDef.Text = ""
        lblDataDef.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblStareCaption
        '
        lblStareCaption.AutoSize = False
        lblStareCaption.Dock = DockStyle.Fill
        lblStareCaption.Name = "lblStareCaption"
        lblStareCaption.Text = "Stare:"
        lblStareCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblStare
        '
        lblStare.AutoSize = False
        lblStare.Dock = DockStyle.Fill
        lblStare.Name = "lblStare"
        lblStare.Text = ""
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblStatusCaption — Încărcat / Preluat pe un singur rând (Da/Nu, ca în Access).
        '
        lblStatusCaption.AutoSize = False
        lblStatusCaption.Dock = DockStyle.Fill
        lblStatusCaption.Name = "lblStatusCaption"
        lblStatusCaption.Text = "Încărcat / Preluat:"
        lblStatusCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblStatus
        '
        lblStatus.AutoSize = False
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Name = "lblStatus"
        lblStatus.Text = ""
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblDescriereCaption
        '
        lblDescriereCaption.AutoSize = False
        lblDescriereCaption.Dock = DockStyle.Fill
        lblDescriereCaption.Name = "lblDescriereCaption"
        lblDescriereCaption.Text = "Descriere:"
        lblDescriereCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblDescriere — se întinde peste ultimele trei coloane (text lung).
        '
        lblDescriere.AutoSize = False
        lblDescriere.Dock = DockStyle.Fill
        lblDescriere.Name = "lblDescriere"
        lblDescriere.Text = ""
        lblDescriere.TextAlign = ContentAlignment.MiddleLeft
        '
        ' pnlHeader
        '
        pnlHeader.Controls.Add(tblHeader)
        pnlHeader.Dock = DockStyle.Top
        pnlHeader.Location = New Point(0, 0)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Padding = New Padding(12, 10, 12, 10)
        pnlHeader.Size = New Size(800, 124)
        pnlHeader.TabIndex = 0
        '
        ' grid — sumarul e READ-ONLY prin definiție (vedere de consultare).
        '
        grid.Dock = DockStyle.Fill
        grid.Location = New Point(0, 124)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.AlternatingRows = True
        grid.Size = New Size(800, 376)
        grid.TabIndex = 1
        '
        ' lblEmpty — starea goală; acoperă zona grilei când nu e nimic de arătat.
        '
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10.0F)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        lblEmpty.Visible = True
        '
        ' SumarView
        ' Într-un card, copiii se adaugă în ordine INVERSĂ de dock: întâi Fill, apoi Top
        ' (ultimul Top adăugat ajunge cel mai sus). lblEmpty se adaugă după grid ca să
        ' stea DEASUPRA ei când e vizibil.
        '
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(grid)
        Controls.Add(lblEmpty)
        Controls.Add(pnlHeader)
        Name = "SumarView"
        Size = New Size(800, 500)
        tblHeader.Controls.Add(lblCodCaption, 0, 0)
        tblHeader.Controls.Add(lblCod, 1, 0)
        tblHeader.Controls.Add(lblDataFxCaption, 2, 0)
        tblHeader.Controls.Add(lblDataFx, 3, 0)
        tblHeader.Controls.Add(lblDataCreareCaption, 0, 1)
        tblHeader.Controls.Add(lblDataCreare, 1, 1)
        tblHeader.Controls.Add(lblDataDefCaption, 2, 1)
        tblHeader.Controls.Add(lblDataDef, 3, 1)
        tblHeader.Controls.Add(lblStareCaption, 0, 2)
        tblHeader.Controls.Add(lblStare, 1, 2)
        tblHeader.Controls.Add(lblStatusCaption, 2, 2)
        tblHeader.Controls.Add(lblStatus, 3, 2)
        tblHeader.Controls.Add(lblDescriereCaption, 0, 3)
        tblHeader.Controls.Add(lblDescriere, 1, 3)
        tblHeader.SetColumnSpan(lblDescriere, 3)
        pnlHeader.ResumeLayout(False)
        tblHeader.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlHeader As Panel
    Friend WithEvents tblHeader As TableLayoutPanel
    Friend WithEvents lblCodCaption As Label
    Friend WithEvents lblCod As Label
    Friend WithEvents lblDataFxCaption As Label
    Friend WithEvents lblDataFx As Label
    Friend WithEvents lblDataCreareCaption As Label
    Friend WithEvents lblDataCreare As Label
    Friend WithEvents lblDataDefCaption As Label
    Friend WithEvents lblDataDef As Label
    Friend WithEvents lblStareCaption As Label
    Friend WithEvents lblStare As Label
    Friend WithEvents lblStatusCaption As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents lblDescriereCaption As Label
    Friend WithEvents lblDescriere As Label
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents lblEmpty As Label
End Class
