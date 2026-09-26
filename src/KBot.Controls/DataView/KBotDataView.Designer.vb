Option Strict On
Imports System.Windows.Forms

''' <summary>
''' The designer part of <see cref="KBotDataView"/>. Per the house rule, ALL child controls are
''' declared here (never built in code on demand): the two floating editors and the two scroll
''' bars. Positions are set at runtime (layout places them), but the fields live in the Designer.
''' Only ONE editor is visible at a time, and only while a cell is being edited.
''' </summary>
Partial Class KBotDataView

    ''' <summary>The components container (standard designer contract).</summary>
    Private components As System.ComponentModel.IContainer

    ''' <summary>The floating text editor (hidden by default). Borderless and not auto-sized: slice 0085 sizes it to one line of the cell's text (see PlaceEditor).</summary>
    Friend WithEvents editText As TextBox

    ''' <summary>The floating combo editor (hidden by default). DropDownStyle is switched per column.</summary>
    Friend WithEvents editCombo As ComboBox

    ''' <summary>The vertical scroll bar.</summary>
    Friend WithEvents vScroll As VScrollBar

    ''' <summary>The horizontal scroll bar.</summary>
    Friend WithEvents hScroll As HScrollBar

    Private Sub InitializeComponent()
        Me.editText = New TextBox()
        Me.editCombo = New ComboBox()
        Me.vScroll = New VScrollBar()
        Me.hScroll = New HScrollBar()
        Me.SuspendLayout()
        '
        ' editText -- floating text editor
        '
        Me.editText.AutoSize = False
        Me.editText.BorderStyle = BorderStyle.None
        Me.editText.Visible = False
        '
        ' editCombo -- floating combo editor
        '
        Me.editCombo.Visible = False
        '
        ' vScroll -- vertical bar (placed/shown by virtualization, slice 0010-02)
        '
        Me.vScroll.Visible = False
        '
        ' hScroll -- horizontal bar (placed/shown by virtualization, slice 0010-02)
        '
        Me.hScroll.Visible = False
        '
        ' KBotDataView
        '
        Me.Controls.Add(Me.editText)
        Me.Controls.Add(Me.editCombo)
        Me.Controls.Add(Me.vScroll)
        Me.Controls.Add(Me.hScroll)
        Me.ResumeLayout(False)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                components?.Dispose()
                DisposeThemeResources()
                DisposeCellTooltip()
                DisposeButtonTips()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

End Class
