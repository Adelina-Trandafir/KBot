' The preview pane of the DDF editor's «Fisiere» page: cell (1;0) of `tlyRoot`, next to the grid.
'
' FOUR SURFACES, one visible at a time. They are declared HERE and never built in code, the same
' rule every other page in this editor follows (docs/kbot-forms-ui-convention.md):
'   lblTitlu    the chosen file's name, always on top of whichever surface is showing
'   pnlGazda    the panel foreign windows are reparented into -- Excel, Word or Adobe
'   picImagine  images, scaled to fit
'   lblMesaj    everything else: nothing selected, unsupported type, or a failure
'
' `pnlGazda` is shared by both hosts on purpose. Only one document is previewed at a time, and one
' panel means there is no way to leave a stale Excel window sitting behind an Adobe one.
'
' Children are added in REVERSE dock order: the three Fill surfaces first, the Top label last, so
' the label gets its band and the surfaces take what is left.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfFisierPreview
    Inherits Global.KBot.Theming.KBotThemedUserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            ' The hosted window goes BEFORE the control is destroyed, or the Office process outlives
            ' the panel its window was a child of (see DdfFisierPreview.EliberezaGazdele).
            If disposing Then EliberezaGazdele()
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        tips = New Global.KBot.Controls.KBotToolTip(components)
        pnlGazda = New Panel()
        picImagine = New PictureBox()
        lblMesaj = New Label()
        lblTitlu = New Label()
        CType(picImagine, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pnlGazda
        '
        pnlGazda.Dock = DockStyle.Fill
        pnlGazda.Location = New Point(0, 28)
        pnlGazda.Margin = New Padding(0)
        pnlGazda.Name = "pnlGazda"
        pnlGazda.Size = New Size(356, 498)
        pnlGazda.TabIndex = 1
        pnlGazda.Visible = False
        '
        ' picImagine
        '
        picImagine.Dock = DockStyle.Fill
        picImagine.Location = New Point(0, 28)
        picImagine.Margin = New Padding(0)
        picImagine.Name = "picImagine"
        picImagine.Size = New Size(356, 498)
        picImagine.SizeMode = PictureBoxSizeMode.Zoom
        picImagine.TabIndex = 2
        picImagine.TabStop = False
        picImagine.Visible = False
        '
        ' lblMesaj
        '
        lblMesaj.Dock = DockStyle.Fill
        lblMesaj.Font = New Font("Calibri", 9F)
        lblMesaj.Location = New Point(0, 28)
        lblMesaj.Name = "lblMesaj"
        lblMesaj.Padding = New Padding(16)
        lblMesaj.Size = New Size(356, 498)
        lblMesaj.TabIndex = 3
        lblMesaj.Text = "Selectează un fișier din listă."
        lblMesaj.TextAlign = ContentAlignment.MiddleCenter
        '
        ' lblTitlu
        '
        lblTitlu.Dock = DockStyle.Top
        lblTitlu.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblTitlu.Location = New Point(0, 0)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.Padding = New Padding(8, 0, 8, 0)
        lblTitlu.Size = New Size(356, 28)
        lblTitlu.TabIndex = 0
        lblTitlu.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblTitlu, "Previzualizare")
        tips.SetToolTipText(lblTitlu, "Se afișează imagini, documente Word, tabele Excel și PDF-uri." & vbLf & "Restul tipurilor se pot doar salva pe disc.")
        '
        ' DdfFisierPreview
        '
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(pnlGazda)
        Controls.Add(picImagine)
        Controls.Add(lblMesaj)
        Controls.Add(lblTitlu)
        Margin = New Padding(0)
        Name = "DdfFisierPreview"
        Size = New Size(356, 526)
        CType(picImagine, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlGazda As Panel
    Friend WithEvents picImagine As PictureBox
    Friend WithEvents lblMesaj As Label
    Friend WithEvents lblTitlu As Label
End Class
