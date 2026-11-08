Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteView
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
        tips = New KBotToolTip(components)
        tlyBody = New KBotTableLayoutPanel()
        pnlArbore = New System.Windows.Forms.Panel()
        tree = New AdvancedTreeControl()
        pnlDetaliu = New System.Windows.Forms.Panel()
        pgLista = New PrimiteListaPage()
        detalii = New PrimiteDetaliiView()
        ntfMesaj = New KBotNotice()
        angajamente = New KeAngajamenteView()
        receptii = New KeReceptiiView()
        tlyBody.SuspendLayout()
        pnlArbore.SuspendLayout()
        pnlDetaliu.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlyBody
        ' 
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F))
        tlyBody.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F))
        tlyBody.Controls.Add(pnlArbore, 0, 0)
        tlyBody.Controls.Add(pnlDetaliu, 1, 0)
        tlyBody.Dock = System.Windows.Forms.DockStyle.Fill
        tlyBody.Location = New System.Drawing.Point(0, 0)
        tlyBody.Margin = New System.Windows.Forms.Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyBody.Size = New System.Drawing.Size(1500, 900)
        tlyBody.TabIndex = 0
        ' 
        ' pnlArbore
        ' 
        pnlArbore.Controls.Add(tree)
        pnlArbore.Dock = System.Windows.Forms.DockStyle.Fill
        pnlArbore.Location = New System.Drawing.Point(0, 0)
        pnlArbore.Margin = New System.Windows.Forms.Padding(0, 0, 9, 6)
        pnlArbore.Name = "pnlArbore"
        pnlArbore.Size = New System.Drawing.Size(360, 900)
        pnlArbore.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.BorderColor = Drawing.SystemColors.ActiveBorder
        tree.CollapseButtonTooltip ="Strânge arborele la o bandă îngustă." & vbLf & "Rândurile se citesc atunci prin eticheta care iese la survolare."
        tree.Dock = System.Windows.Forms.DockStyle.Fill
        tree.DynamicColumns = False
        tree.ExpandButtonTooltip = "Desfă arborele la loc, pe toată lățimea lui."
        tree.ExpanderSize = 10
        tree.Font = New System.Drawing.Font("Calibri", 9F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        tree.FooterCaption = "Actualizează"
        tree.FooterIconSize = New System.Drawing.Size(18, 18)
        tree.FooterRightIcon = My.Resources.Resources.Jonas_Rask_Danish_Royalty_Free_Refresh_32
        tree.FooterRightIconTooltip = "Aduce de la ANAF facturile noi." & vbLf & "Alegeți perioada sau scrieți numărul de zile (1-60)."
        tree.FooterTextAlign = Drawing.ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " FACTURI PRIMITE, PE LUNI"
        tree.HeaderForeColor = Drawing.Color.Black
        tree.HeaderIconSize = New System.Drawing.Size(18, 18)
        tree.HeaderRightIcon = My.Resources.Resources.filter
        tree.HeaderRightIconTooltip = "Opțiunile arborelui: filtrele și gruparea facturilor."
        tree.HeaderSearchIcon = My.Resources.Resources.search
        tree.HeaderSearchIconTooltip = "Deschide banda de căutare peste arbore." & vbLf & "ESC golește căutarea și o închide."
        tree.HeaderVisible = True
        tree.Indent = 8
        tree.ItemHeight = 24
        tree.LeftIconSize = New System.Drawing.Size(14, 14)
        tree.Location = New System.Drawing.Point(0, 0)
        tree.Margin = New System.Windows.Forms.Padding(0)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.PaddingExpanderGap = 8
        tree.PaddingIconGap = 8
        tree.PaddingTreeEnd = 8
        tree.PaddingTreeStart = 8
        tree.RightClickSelects = False
        tree.RightIconSize = New System.Drawing.Size(14, 14)
        tree.RightTextWidth = 96
        tree.SearchBarFont = New System.Drawing.Font("Calibri", 9F)
        tree.SearchBoxBackColor = Drawing.SystemColors.Control
        tree.SearchClearButton = True
        tree.SearchClearButtonHoverColor = Drawing.SystemColors.Control
        tree.SearchDefaultText = "... tastează minim 3 caractere ..."
        tree.SearchSeparatorColor = Drawing.SystemColors.ActiveBorder
        tree.SearchSeparatorWidth = 2
        tree.Size = New System.Drawing.Size(360, 900)
        tree.TabIndex = 0
        ' 
        ' pnlDetaliu
        ' 
        pnlDetaliu.Controls.Add(pgLista)
        pnlDetaliu.Controls.Add(detalii)
        pnlDetaliu.Controls.Add(ntfMesaj)
        pnlDetaliu.Dock = System.Windows.Forms.DockStyle.Fill
        pnlDetaliu.Location = New System.Drawing.Point(360, 0)
        pnlDetaliu.Margin = New System.Windows.Forms.Padding(0)
        pnlDetaliu.Name = "pnlDetaliu"
        pnlDetaliu.Size = New System.Drawing.Size(1140, 900)
        pnlDetaliu.TabIndex = 0
        pnlDetaliu.Tag = "Card"
        ' 
        ' pgLista
        ' 
        pgLista.Dock = System.Windows.Forms.DockStyle.Fill
        pgLista.Font = New System.Drawing.Font("Calibri", 9F)
        pgLista.Location = New System.Drawing.Point(0, 0)
        pgLista.Margin = New System.Windows.Forms.Padding(0)
        pgLista.Name = "pgLista"
        pgLista.Padding = New System.Windows.Forms.Padding(0)
        pgLista.Size = New System.Drawing.Size(1140, 856)
        pgLista.TabIndex = 3
        pgLista.Visible = False
        '
        ' detalii
        '
        detalii.Dock = System.Windows.Forms.DockStyle.Fill
        detalii.Location = New System.Drawing.Point(0, 0)
        detalii.Margin = New System.Windows.Forms.Padding(0)
        detalii.Name = "detalii"
        detalii.Size = New System.Drawing.Size(1140, 856)
        detalii.TabIndex = 2
        ' 
        ' ntfMesaj
        ' 
        ntfMesaj.BackColor = Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 856)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(1140, 44)
        ntfMesaj.TabIndex = 1
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        ' 
        ' angajamente
        ' 
        angajamente.Dock = System.Windows.Forms.DockStyle.Fill
        angajamente.Location = New System.Drawing.Point(0, 900)
        angajamente.Margin = New System.Windows.Forms.Padding(0)
        angajamente.Name = "angajamente"
        angajamente.Size = New System.Drawing.Size(360, 1)
        angajamente.TabIndex = 2
        angajamente.Visible = False
        ' 
        ' receptii
        ' 
        receptii.Dock = System.Windows.Forms.DockStyle.Fill
        receptii.Location = New System.Drawing.Point(0, 0)
        receptii.Margin = New System.Windows.Forms.Padding(0)
        receptii.Name = "receptii"
        receptii.Size = New System.Drawing.Size(1140, 901)
        receptii.TabIndex = 0
        receptii.Visible = False
        ' 
        ' PrimiteView
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(receptii)
        Controls.Add(tlyBody)
        Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Name = "PrimiteView"
        Size = New System.Drawing.Size(1500, 900)
        tlyBody.ResumeLayout(False)
        pnlArbore.ResumeLayout(False)
        pnlDetaliu.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents pnlArbore As System.Windows.Forms.Panel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents pnlDetaliu As System.Windows.Forms.Panel
    Friend WithEvents angajamente As KeAngajamenteView
    Friend WithEvents receptii As KeReceptiiView
    Friend WithEvents detalii As PrimiteDetaliiView
    Friend WithEvents pgLista As PrimiteListaPage
    Friend WithEvents ntfMesaj As KBotNotice
End Class
