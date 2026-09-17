Imports System.Drawing
Imports System.Windows.Forms

' The date field of slice 0050, rebuilt as a UserControl so it opens in the Visual Studio
' designer like any other surface of the house: the inner edit box is declared HERE
' (docs/kbot-forms-ui-convention.md), not built in code.
'
' WHAT THE NUMBERS MEAN. The bounds written for txtDate are what the design surface shows at
' 96 dpi for a field of the default size: the outline (1 px), the default TextPadding (8 left
' and right, 0 top and bottom) and the calendar button strip (28 px plus a 2 px gap) taken out.
' At runtime the box is placed by KBotDatePicker.PositionInner on every layout pass, from the
' published metrics (Padding, TextPadding, ButtonWidth, BorderWidth) scaled to the DPI in
' force -- so the box is always as tall as the field lets it be, whatever was dragged here.
'
' The box is a KBotDateEditBox: a multiline TextBox, which is what frees its height, with the
' text kept on one centred line (EM_SETRECT). Never set Multiline on it.
'
' AutoScaleMode is Inherit on purpose. The control keeps no stamp of its own: the host form
' scales the frame like any other control, and the inner box is re-placed by PositionInner
' afterwards, so there is nothing a second, private scaling pass could get right.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KBotDatePicker
    Inherits UserControl

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        txtDate = New KBotDateEditBox()
        SuspendLayout()
        ' 
        ' txtDate
        ' 
        txtDate.BorderStyle = BorderStyle.None
        txtDate.Location = New Point(9, 1)
        txtDate.Margin = New Padding(0)
        txtDate.Name = "txtDate"
        txtDate.Size = New Size(102, 26)
        txtDate.TabIndex = 0
        txtDate.TextAlign = HorizontalAlignment.Center
        txtDate.WordWrap = False
        ' 
        ' KBotDatePicker
        ' 
        AutoScaleMode = AutoScaleMode.Inherit
        BackColor = SystemColors.Window
        Controls.Add(txtDate)
        Name = "KBotDatePicker"
        Size = New Size(150, 28)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents txtDate As KBotDateEditBox
End Class
