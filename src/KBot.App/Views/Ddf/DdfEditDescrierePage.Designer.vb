Imports KBot.Controls

' «Descriere» of the DDF editor (slice 0051).
'
' The short description and the long one. The short one ALSO lives in the form's header band
' and the two stay in step: editing it here writes into the same draft object the header
' reads, and the form pushes the new text back into its own field.
'
' The long description is the ported `VBA_DDF_INFO` editing surface (`KBotRichTextEditor`).
' Its ATTACHMENT BUTTON is not wired and not shown: the «Fisiere» page owns attachments now,
' and a second, half-working way to add them would be a trap.
'
' Both faces of the long description are kept, because both columns are written:
' `Desc_Lunga` gets the RTF and `Desc_Lunga_ANSI` the plain text. The plain-text one is what
' the frozen read route of slice 0020 serves and what `DdfXmlBuilder` puts into the signed
' XFA document, which cannot take RTF control words.
'
' All controls are declared HERE (docs/kbot-forms-ui-convention.md).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfEditDescrierePage
    Inherits UserControl

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(DdfEditDescrierePage))
        tips = New KBotToolTip(components)
        txtScurta = New KBotTextField()
        tlyRoot = New TableLayoutPanel()
        edtLunga = New KBotRichTextEditor()
        il_rtb = New ImageList(components)
        lblScurtaCaption = New Label()
        lblLungaCaption = New Label()
        tlyRoot.SuspendLayout()
        SuspendLayout()
        ' 
        ' txtScurta
        ' 
        txtScurta.BackColor = Color.Transparent
        txtScurta.Dock = DockStyle.Fill
        txtScurta.Location = New Point(249, 10)
        txtScurta.Margin = New Padding(6, 10, 11, 10)
        txtScurta.MaxLength = 32767
        txtScurta.Name = "txtScurta"
        txtScurta.PlaceholderText = ""
        txtScurta.Size = New Size(691, 30)
        txtScurta.TabIndex = 0
        txtScurta.TabStop = False
        tips.SetToolTipHeader(txtScurta, "Descrierea scurtă")
        tips.SetToolTipText(txtScurta, "Același câmp ca în antetul formularului." & vbLf & "Când îl schimbi, descrierea lungă primește același text.")
        txtScurta.UseSystemPasswordChar = False
        ' 
        ' tlyRoot
        ' 
        tlyRoot.ColumnCount = 2
        tlyRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 243F))
        tlyRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRoot.Controls.Add(edtLunga, 1, 1)
        tlyRoot.Controls.Add(lblScurtaCaption, 0, 0)
        tlyRoot.Controls.Add(txtScurta, 1, 0)
        tlyRoot.Controls.Add(lblLungaCaption, 0, 1)
        tlyRoot.Dock = DockStyle.Fill
        tlyRoot.Location = New Point(0, 0)
        tlyRoot.Margin = New Padding(0)
        tlyRoot.Name = "tlyRoot"
        tlyRoot.RowCount = 2
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRoot.Size = New Size(951, 504)
        tlyRoot.TabIndex = 0
        ' 
        ' edtLunga
        ' 
        edtLunga.BoldImageKey = "bold"
        edtLunga.ButtonImageLayout = RichTextImageLayout.Zoom
        edtLunga.ButtonSize = New Size(24, 24)
        edtLunga.ButtonSpacing = 4
        edtLunga.ComboFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        edtLunga.Dock = DockStyle.Fill
        edtLunga.EditorFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        edtLunga.FontComboWidth = 200
        edtLunga.FooterFont = New Font("Calibri", 8F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        edtLunga.FooterHeight = 30
        edtLunga.FooterSeparatorColor = SystemColors.ActiveBorder
        edtLunga.FooterSeparatorWidth = 2
        edtLunga.GroupSpacing = 100
        edtLunga.HeaderHeight = 30
        edtLunga.HeaderPadding = New Padding(4, 0, 4, 0)
        edtLunga.HeaderSeparatorColor = SystemColors.ActiveBorder
        edtLunga.HeaderSeparatorWidth = 2
        edtLunga.HighlightImageKey = "text_backcolor"
        edtLunga.Images = il_rtb
        edtLunga.ItalicImageKey = "italic"
        edtLunga.Location = New Point(249, 50)
        edtLunga.Margin = New Padding(6, 0, 11, 13)
        edtLunga.Name = "edtLunga"
        edtLunga.Size = New Size(691, 441)
        edtLunga.SizeComboWidth = 100
        edtLunga.TabIndex = 1
        edtLunga.TextColorImageKey = "text_forecolor"
        edtLunga.UnderlineImageKey = "underline"
        ' 
        ' il_rtb
        ' 
        il_rtb.ColorDepth = ColorDepth.Depth32Bit
        il_rtb.ImageStream = CType(resources.GetObject("il_rtb.ImageStream"), ImageListStreamer)
        il_rtb.TransparentColor = Color.Transparent
        il_rtb.Images.SetKeyName(0, "text_backcolor")
        il_rtb.Images.SetKeyName(1, "text_forecolor")
        il_rtb.Images.SetKeyName(2, "bold")
        il_rtb.Images.SetKeyName(3, "italic")
        il_rtb.Images.SetKeyName(4, "underline")
        ' 
        ' lblScurtaCaption
        ' 
        lblScurtaCaption.AutoSize = True
        lblScurtaCaption.Dock = DockStyle.Fill
        lblScurtaCaption.Font = New Font("Calibri", 9F)
        lblScurtaCaption.Location = New Point(11, 0)
        lblScurtaCaption.Margin = New Padding(11, 0, 6, 0)
        lblScurtaCaption.Name = "lblScurtaCaption"
        lblScurtaCaption.Size = New Size(226, 50)
        lblScurtaCaption.TabIndex = 0
        lblScurtaCaption.Text = "Descriere scurtă"
        lblScurtaCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblLungaCaption
        ' 
        lblLungaCaption.AutoSize = True
        lblLungaCaption.Dock = DockStyle.Top
        lblLungaCaption.Font = New Font("Calibri", 9F)
        lblLungaCaption.Location = New Point(11, 50)
        lblLungaCaption.Margin = New Padding(11, 0, 6, 0)
        lblLungaCaption.Name = "lblLungaCaption"
        lblLungaCaption.Size = New Size(226, 22)
        lblLungaCaption.TabIndex = 1
        lblLungaCaption.Text = "Descriere lungă"
        lblLungaCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' DdfEditDescrierePage
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(tlyRoot)
        Margin = New Padding(0)
        Name = "DdfEditDescrierePage"
        Size = New Size(951, 504)
        tlyRoot.ResumeLayout(False)
        tlyRoot.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents tlyRoot As TableLayoutPanel
    Friend WithEvents lblScurtaCaption As Label
    Friend WithEvents txtScurta As KBot.Controls.KBotTextField
    Friend WithEvents lblLungaCaption As Label
    Friend WithEvents edtLunga As KBot.Controls.KBotRichTextEditor
    Friend WithEvents il_rtb As ImageList
End Class
