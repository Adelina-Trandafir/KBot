Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimitePdfPage
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
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        pgViewer = New VanzarePdfPage()
        picAtas = New System.Windows.Forms.PictureBox()
        navDoc = New KBotNavList()
        CType(picAtas, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(navDoc, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pgViewer
        '
        pgViewer.Dock = System.Windows.Forms.DockStyle.Fill
        pgViewer.Location = New System.Drawing.Point(0, 36)
        pgViewer.Margin = New System.Windows.Forms.Padding(0)
        pgViewer.Name = "pgViewer"
        pgViewer.Size = New System.Drawing.Size(757, 490)
        pgViewer.TabIndex = 1
        '
        ' picAtas
        '
        picAtas.Dock = System.Windows.Forms.DockStyle.Fill
        picAtas.Location = New System.Drawing.Point(0, 36)
        picAtas.Margin = New System.Windows.Forms.Padding(0)
        picAtas.Name = "picAtas"
        picAtas.Size = New System.Drawing.Size(757, 490)
        picAtas.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        picAtas.TabIndex = 2
        picAtas.TabStop = False
        picAtas.Visible = False
        '
        ' navDoc
        '
        navDoc.Dock = System.Windows.Forms.DockStyle.Top
        navDoc.IconSize = 16
        navDoc.ItemCornerRadius = 2
        navDoc.ItemPadding = New System.Windows.Forms.Padding(0)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Image = My.Resources.Resources.invoice
        KBotNavItem1.Key = "clasica"
        KBotNavItem1.Text = "Factură clasică"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Image = My.Resources.Resources.invoice
        KBotNavItem2.Key = "anaf"
        KBotNavItem2.Text = "Factură ANAF"
        KBotNavItem3.AutoSize = True
        KBotNavItem3.Image = My.Resources.Resources.attach
        KBotNavItem3.Key = "atasamente"
        KBotNavItem3.Text = "Atașamente"
        KBotNavItem3.Visible = False
        navDoc.Items.Add(KBotNavItem1)
        navDoc.Items.Add(KBotNavItem2)
        navDoc.Items.Add(KBotNavItem3)
        navDoc.Location = New System.Drawing.Point(0, 0)
        navDoc.Name = "navDoc"
        navDoc.Orientation = KBotNavOrientation.Horizontal
        navDoc.SelectedKey = Nothing
        navDoc.Size = New System.Drawing.Size(757, 36)
        navDoc.TabIndex = 0
        '
        ' PrimitePdfPage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(pgViewer)
        Controls.Add(picAtas)
        Controls.Add(navDoc)
        Name = "PrimitePdfPage"
        Size = New System.Drawing.Size(757, 526)
        CType(picAtas, System.ComponentModel.ISupportInitialize).EndInit()
        CType(navDoc, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pgViewer As VanzarePdfPage
    Friend WithEvents picAtas As System.Windows.Forms.PictureBox
    Friend WithEvents navDoc As KBotNavList
End Class
