Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteMesajePage
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
        components = New System.ComponentModel.Container()
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        gridMesaje = New KBotDataView()
        pnlTrimite = New System.Windows.Forms.Panel()
        txtMesaj = New KBotTextBox()
        btnTrimite = New KBotButton()
        lblTrimite = New System.Windows.Forms.Label()
        CType(gridMesaje, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlTrimite.SuspendLayout()
        SuspendLayout()
        '
        ' gridMesaje
        '
        gridMesaje.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridMesaje.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Data"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "data"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 130
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Mesaj"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "text"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 560
        gridMesaje.Columns.Add(KBotDataColumn1)
        gridMesaje.Columns.Add(KBotDataColumn2)
        gridMesaje.Dock = System.Windows.Forms.DockStyle.Fill
        gridMesaje.FillColumnKey = "text"
        gridMesaje.Location = New System.Drawing.Point(3, 3)
        gridMesaje.Margin = New System.Windows.Forms.Padding(0)
        gridMesaje.Name = "gridMesaje"
        gridMesaje.ReadOnlyGrid = True
        gridMesaje.Size = New System.Drawing.Size(751, 410)
        gridMesaje.TabIndex = 0
        ' tips.SetToolTipHeader(gridMesaje, "Mesaje")
        ' tips.SetToolTipText(gridMesaje, "Mesajele schimbate prin sistemul E-Factura despre această factură.")
        '
        ' pnlTrimite
        '
        pnlTrimite.Controls.Add(txtMesaj)
        pnlTrimite.Controls.Add(btnTrimite)
        pnlTrimite.Controls.Add(lblTrimite)
        pnlTrimite.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlTrimite.Location = New System.Drawing.Point(3, 413)
        pnlTrimite.Margin = New System.Windows.Forms.Padding(0)
        pnlTrimite.Name = "pnlTrimite"
        pnlTrimite.Size = New System.Drawing.Size(751, 110)
        pnlTrimite.TabIndex = 1
        '
        ' txtMesaj
        '
        txtMesaj.Dock = System.Windows.Forms.DockStyle.Fill
        txtMesaj.Location = New System.Drawing.Point(0, 22)
        txtMesaj.Margin = New System.Windows.Forms.Padding(0)
        txtMesaj.MaxLength = 2000
        txtMesaj.Name = "txtMesaj"
        txtMesaj.Size = New System.Drawing.Size(611, 88)
        txtMesaj.TabIndex = 1
        tips.SetToolTipHeader(txtMesaj, "Mesaj către furnizor")
        tips.SetToolTipText(txtMesaj, "Scrieți mesajul (cel mult 2000 de caractere) și apăsați «Trimite». Mesajul pleacă prin ANAF și nu mai poate fi retras.")
        '
        ' btnTrimite
        '
        btnTrimite.Dock = System.Windows.Forms.DockStyle.Right
        btnTrimite.Location = New System.Drawing.Point(611, 22)
        btnTrimite.Margin = New System.Windows.Forms.Padding(0)
        btnTrimite.Name = "btnTrimite"
        btnTrimite.Size = New System.Drawing.Size(140, 88)
        btnTrimite.TabIndex = 2
        btnTrimite.Text = "Trimite"
        tips.SetToolTipHeader(btnTrimite, "Trimite mesajul")
        tips.SetToolTipText(btnTrimite, "Trimite mesajul scris, prin ANAF, despre factura aleasă.")
        '
        ' lblTrimite
        '
        lblTrimite.Dock = System.Windows.Forms.DockStyle.Top
        lblTrimite.Location = New System.Drawing.Point(0, 0)
        lblTrimite.Margin = New System.Windows.Forms.Padding(0)
        lblTrimite.Name = "lblTrimite"
        lblTrimite.Size = New System.Drawing.Size(751, 22)
        lblTrimite.TabIndex = 0
        lblTrimite.Text = "Mesaj către furnizor (se trimite prin ANAF):"
        lblTrimite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' PrimiteMesajePage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(gridMesaje)
        Controls.Add(pnlTrimite)
        Name = "PrimiteMesajePage"
        Padding = New System.Windows.Forms.Padding(3, 3, 3, 0)
        Size = New System.Drawing.Size(757, 526)
        CType(gridMesaje, System.ComponentModel.ISupportInitialize).EndInit()
        pnlTrimite.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents gridMesaje As KBotDataView
    Friend WithEvents pnlTrimite As System.Windows.Forms.Panel
    Friend WithEvents txtMesaj As KBotTextBox
    Friend WithEvents btnTrimite As KBotButton
    Friend WithEvents lblTrimite As System.Windows.Forms.Label
End Class
