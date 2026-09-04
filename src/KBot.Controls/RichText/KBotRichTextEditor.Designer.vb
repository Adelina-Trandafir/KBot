Imports KBot.Controls

' The rich-text editor of slice 0051 -- the port of the `RTB` form from the `VBA_DDF_INFO`
' project into a themed, embeddable control.
'
' WHAT CAME ACROSS: the editing surface. A RichTextBox filling the control, over a toolbar of
' bold / italic / underline / text colour / background colour plus a font-family and a
' font-size picker. That is what `frmFX_DDF_INFO` gave the operator and what the long
' description of a revision is written with.
'
' WHAT DID NOT: `Start.vb` and most of `Helpers.vb` -- the Access COM plumbing, `SetParent`,
' the parent-window monitor and the "write the value back into an Access control" handlers.
' None of it has a job here. The ATTACHMENT buttons (`flyButoane`, `AddAttachmentButton`,
' `btnAtasteazaDocumente`) are deliberately absent too: the «Fisiere» page owns attachments
' now, and a second, half-wired way to add them would be a trap.
'
' WHAT THE BANDS ARE. The header and the footer are `KBotRichTextBand` -- a strip that paints
' its own background and one edge separator, so both read like the grid's header band. NOTHING
' here is positioned by a TableLayoutPanel any more: every rectangle comes out of
' `RebuildLayout`, driven by the published metrics (HeaderHeight, ButtonSize, ButtonSpacing,
' FontComboWidth, ...). The sizes written below are the DEFAULTS at 96 dpi, so a control
' dropped on a form looks right before anyone touches a property.
'
' All controls are declared HERE (docs/kbot-forms-ui-convention.md).
' Coordinates are written at 144 dpi -- the file was saved from the designer on a 150% screen,
' and Visual Studio rewrites the coordinates and the stamp together when it does.
' AutoScaleDimensions goes with them: Calibri 9 measures (9, 22) there (slice 0052). The two
' always change together; a stamp taken from another font or another dpi squashes the window
' on open, with nothing in the designer to show it.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KBotRichTextEditor
    Inherits KBotThemedUserControl

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
        btnBold = New KBotNoFocusButton()
        btnItalic = New KBotNoFocusButton()
        btnUnderline = New KBotNoFocusButton()
        btnTextColor = New KBotNoFocusButton()
        btnHighlight = New KBotNoFocusButton()
        cmbFont = New KBotComboBox()
        cmbSize = New KBotComboBox()
        btnCollapse = New KBotNoFocusButton()
        tmrStats = New Timer(components)
        pnlHeader = New KBotRichTextBand()
        pnlFooter = New KBotRichTextBand()
        lblChars = New Label()
        lblWords = New Label()
        lblSize = New Label()
        rtb = New RichTextBox()
        pnlHeader.SuspendLayout()
        pnlFooter.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnBold
        ' 
        btnBold.FlatStyle = FlatStyle.Flat
        btnBold.Font = New Font("Calibri", 10F, FontStyle.Bold)
        btnBold.Location = New Point(6, 7)
        btnBold.Margin = New Padding(0)
        btnBold.Name = "btnBold"
        btnBold.Size = New Size(43, 50)
        btnBold.TabIndex = 0
        btnBold.TabStop = False
        btnBold.Text = "B"
        tips.SetToolTipHeader(btnBold, "Îngroșat")
        tips.SetToolTipText(btnBold, "Îngroașă textul selectat.")
        btnBold.UseVisualStyleBackColor = False
        ' 
        ' btnItalic
        ' 
        btnItalic.FlatStyle = FlatStyle.Flat
        btnItalic.Font = New Font("Calibri", 10F, FontStyle.Italic)
        btnItalic.Location = New Point(51, 7)
        btnItalic.Margin = New Padding(0)
        btnItalic.Name = "btnItalic"
        btnItalic.Size = New Size(43, 50)
        btnItalic.TabIndex = 1
        btnItalic.TabStop = False
        btnItalic.Text = "I"
        tips.SetToolTipHeader(btnItalic, "Înclinat")
        tips.SetToolTipText(btnItalic, "Înclină textul selectat.")
        btnItalic.UseVisualStyleBackColor = False
        ' 
        ' btnUnderline
        ' 
        btnUnderline.FlatStyle = FlatStyle.Flat
        btnUnderline.Font = New Font("Calibri", 10F, FontStyle.Underline)
        btnUnderline.Location = New Point(97, 7)
        btnUnderline.Margin = New Padding(0)
        btnUnderline.Name = "btnUnderline"
        btnUnderline.Size = New Size(43, 50)
        btnUnderline.TabIndex = 2
        btnUnderline.TabStop = False
        btnUnderline.Text = "U"
        tips.SetToolTipHeader(btnUnderline, "Subliniat")
        tips.SetToolTipText(btnUnderline, "Subliniază textul selectat.")
        btnUnderline.UseVisualStyleBackColor = False
        ' 
        ' btnTextColor
        ' 
        btnTextColor.FlatStyle = FlatStyle.Flat
        btnTextColor.Font = New Font("Calibri", 10F)
        btnTextColor.Location = New Point(143, 7)
        btnTextColor.Margin = New Padding(0)
        btnTextColor.Name = "btnTextColor"
        btnTextColor.Size = New Size(43, 50)
        btnTextColor.TabIndex = 3
        btnTextColor.TabStop = False
        btnTextColor.Text = "A"
        tips.SetToolTipHeader(btnTextColor, "Culoarea textului")
        tips.SetToolTipText(btnTextColor, "Schimbă culoarea textului selectat.")
        btnTextColor.UseVisualStyleBackColor = False
        ' 
        ' btnHighlight
        ' 
        btnHighlight.FlatStyle = FlatStyle.Flat
        btnHighlight.Font = New Font("Calibri", 10F)
        btnHighlight.Location = New Point(189, 7)
        btnHighlight.Margin = New Padding(0)
        btnHighlight.Name = "btnHighlight"
        btnHighlight.Size = New Size(43, 50)
        btnHighlight.TabIndex = 4
        btnHighlight.TabStop = False
        btnHighlight.Text = "▨"
        tips.SetToolTipHeader(btnHighlight, "Culoarea fundalului")
        tips.SetToolTipText(btnHighlight, "Schimbă culoarea de fundal a textului selectat.")
        btnHighlight.UseVisualStyleBackColor = False
        ' 
        ' cmbFont
        ' 
        cmbFont.DrawMode = DrawMode.OwnerDrawFixed
        cmbFont.DropDownStyle = ComboBoxStyle.DropDownList
        cmbFont.FlatStyle = FlatStyle.Flat
        cmbFont.IntegralHeight = False
        cmbFont.Location = New Point(243, 16)
        cmbFont.Margin = New Padding(0)
        cmbFont.Name = "cmbFont"
        cmbFont.Size = New Size(264, 32)
        cmbFont.TabIndex = 5
        cmbFont.TabStop = False
        tips.SetToolTipHeader(cmbFont, "Fontul")
        tips.SetToolTipText(cmbFont, "Schimbă fontul textului selectat.")
        ' 
        ' cmbSize
        ' 
        cmbSize.DrawMode = DrawMode.OwnerDrawFixed
        cmbSize.DropDownStyle = ComboBoxStyle.DropDownList
        cmbSize.FlatStyle = FlatStyle.Flat
        cmbSize.Location = New Point(516, 16)
        cmbSize.Margin = New Padding(0)
        cmbSize.Name = "cmbSize"
        cmbSize.Size = New Size(107, 32)
        cmbSize.TabIndex = 6
        cmbSize.TabStop = False
        tips.SetToolTipHeader(cmbSize, "Mărimea")
        tips.SetToolTipText(cmbSize, "Schimbă mărimea textului selectat.")
        ' 
        ' btnCollapse
        ' 
        btnCollapse.FlatStyle = FlatStyle.Flat
        btnCollapse.Font = New Font("Calibri", 10F)
        btnCollapse.Location = New Point(951, 7)
        btnCollapse.Margin = New Padding(0)
        btnCollapse.Name = "btnCollapse"
        btnCollapse.Size = New Size(43, 50)
        btnCollapse.TabIndex = 7
        btnCollapse.TabStop = False
        btnCollapse.Text = "▴"
        tips.SetToolTipHeader(btnCollapse, "Strânge editorul")
        tips.SetToolTipText(btnCollapse, "Pliază suprafața de scris și lasă doar bara de sus.")
        btnCollapse.UseVisualStyleBackColor = False
        btnCollapse.Visible = False
        ' 
        ' tmrStats
        ' 
        tmrStats.Interval = 150
        ' 
        ' pnlHeader
        ' 
        pnlHeader.Controls.Add(btnBold)
        pnlHeader.Controls.Add(btnItalic)
        pnlHeader.Controls.Add(btnUnderline)
        pnlHeader.Controls.Add(btnTextColor)
        pnlHeader.Controls.Add(btnHighlight)
        pnlHeader.Controls.Add(cmbFont)
        pnlHeader.Controls.Add(cmbSize)
        pnlHeader.Controls.Add(btnCollapse)
        pnlHeader.Location = New Point(0, 0)
        pnlHeader.Margin = New Padding(0)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Size = New Size(1000, 63)
        pnlHeader.TabIndex = 0
        ' 
        ' pnlFooter
        ' 
        pnlFooter.Controls.Add(lblChars)
        pnlFooter.Controls.Add(lblWords)
        pnlFooter.Controls.Add(lblSize)
        pnlFooter.Location = New Point(0, 493)
        pnlFooter.Margin = New Padding(0)
        pnlFooter.Name = "pnlFooter"
        pnlFooter.Size = New Size(1000, 40)
        pnlFooter.TabIndex = 2
        ' 
        ' lblChars
        ' 
        lblChars.Font = New Font("Calibri", 8.5F)
        lblChars.Location = New Point(11, 0)
        lblChars.Margin = New Padding(0)
        lblChars.Name = "lblChars"
        lblChars.Size = New Size(171, 40)
        lblChars.TabIndex = 0
        lblChars.Text = "0 caractere"
        lblChars.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblWords
        ' 
        lblWords.Font = New Font("Calibri", 8.5F)
        lblWords.Location = New Point(206, 0)
        lblWords.Margin = New Padding(0)
        lblWords.Name = "lblWords"
        lblWords.Size = New Size(171, 40)
        lblWords.TabIndex = 1
        lblWords.Text = "0 cuvinte"
        lblWords.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblSize
        ' 
        lblSize.Font = New Font("Calibri", 8.5F)
        lblSize.Location = New Point(400, 0)
        lblSize.Margin = New Padding(0)
        lblSize.Name = "lblSize"
        lblSize.Size = New Size(171, 40)
        lblSize.TabIndex = 2
        lblSize.Text = "0,0 KB"
        lblSize.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' rtb
        ' 
        rtb.AcceptsTab = True
        rtb.BorderStyle = BorderStyle.None
        rtb.Font = New Font("Calibri", 11F)
        rtb.HideSelection = False
        rtb.Location = New Point(1, 65)
        rtb.Margin = New Padding(0)
        rtb.Name = "rtb"
        rtb.ScrollBars = RichTextBoxScrollBars.Vertical
        rtb.Size = New Size(997, 427)
        rtb.TabIndex = 1
        rtb.Text = ""
        ' 
        ' KBotRichTextEditor
        ' 
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(rtb)
        Controls.Add(pnlHeader)
        Controls.Add(pnlFooter)
        Margin = New Padding(0)
        Name = "KBotRichTextEditor"
        Size = New Size(1000, 533)
        pnlHeader.ResumeLayout(False)
        pnlFooter.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents tmrStats As Timer
    Friend WithEvents pnlHeader As KBot.Controls.KBotRichTextBand
    Friend WithEvents pnlFooter As KBot.Controls.KBotRichTextBand
    Friend WithEvents btnBold As KBotNoFocusButton
    Friend WithEvents btnItalic As KBotNoFocusButton
    Friend WithEvents btnUnderline As KBotNoFocusButton
    Friend WithEvents btnTextColor As KBotNoFocusButton
    Friend WithEvents btnHighlight As KBotNoFocusButton
    Friend WithEvents btnCollapse As KBotNoFocusButton
    Friend WithEvents cmbFont As KBot.Controls.KBotComboBox
    Friend WithEvents cmbSize As KBot.Controls.KBotComboBox
    Friend WithEvents rtb As RichTextBox
    Friend WithEvents lblChars As Label
    Friend WithEvents lblWords As Label
    Friend WithEvents lblSize As Label
End Class
