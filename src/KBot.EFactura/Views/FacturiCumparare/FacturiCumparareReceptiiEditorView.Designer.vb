<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FacturiCumparareReceptiiEditorView
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
        Dim KBotNavItem1 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        Dim KBotNavItem2 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        Dim KBotNavItem3 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        pnlRecViewHost = New System.Windows.Forms.Panel()
        navMod = New Controls.KBotNavList()
        CType(navMod, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' pnlRecViewHost
        ' 
        pnlRecViewHost.Dock = System.Windows.Forms.DockStyle.Fill
        pnlRecViewHost.Location = New System.Drawing.Point(0, 45)
        pnlRecViewHost.Margin = New System.Windows.Forms.Padding(0)
        pnlRecViewHost.Name = "pnlRecViewHost"
        pnlRecViewHost.Size = New System.Drawing.Size(540, 471)
        pnlRecViewHost.TabIndex = 0
        ' 
        ' navMod
        ' 
        navMod.Dock = System.Windows.Forms.DockStyle.Top
        navMod.IconSize = 18
        navMod.ItemCornerRadius = 2
        navMod.ItemPadding = New System.Windows.Forms.Padding(0)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Image = My.Resources.Resources.add_fact
        KBotNavItem1.Key = "noua"
        KBotNavItem1.Text = "Adaugă"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Image = My.Resources.Resources.invoice
        KBotNavItem2.Key = "asociate"
        KBotNavItem2.Text = "Asociate"
        KBotNavItem3.AutoSize = True
        KBotNavItem3.Image = My.Resources.Resources.table
        KBotNavItem3.Key = "existente"
        KBotNavItem3.Text = "Existente"
        navMod.Items.Add(KBotNavItem1)
        navMod.Items.Add(KBotNavItem2)
        navMod.Items.Add(KBotNavItem3)
        navMod.Location = New System.Drawing.Point(0, 0)
        navMod.Margin = New System.Windows.Forms.Padding(4)
        navMod.Name = "navMod"
        navMod.Orientation = KBot.Controls.KBotNavOrientation.Horizontal
        navMod.SelectedKey = "noua"
        navMod.Size = New System.Drawing.Size(540, 45)
        navMod.TabIndex = 3
        '
        ' FacturiCumparareReceptiiEditorView
        '
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(pnlRecViewHost)
        Controls.Add(navMod)
        Margin = New System.Windows.Forms.Padding(0)
        Name = "FacturiCumparareReceptiiEditorView"
        Size = New System.Drawing.Size(540, 516)
        CType(navMod, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlRecViewHost As System.Windows.Forms.Panel
    Friend WithEvents navMod As Global.KBot.Controls.KBotNavList
End Class
