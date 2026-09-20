Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' What the three tabs of the column menu share: the look of a MENU ROW and the two
''' measurements behind <see cref="IKBotFilterMenuView.RequiredHeight"/>. Kept out of the views
''' so the sort tab and the filter tab cannot drift apart in how a row is painted or measured.
''' </summary>
Friend NotInheritable Class KBotFilterMenuLayout

    Private Sub New()
    End Sub

    ''' <summary>How many list rows show without scrolling -- the rest goes to the list's own bar.</summary>
    Friend Const MaxListRows As Integer = 10

    ''' <summary>
    ''' A MENU ROW: flat, borderless, full width, in the colour of the surface it sits on --
    ''' hover is the only thing that lifts it, exactly as in a system menu.
    '''
    ''' <para><b>Why the generic button rule is not enough.</b> The Modern scheme owner-draws
    ''' every <c>Button</c>: it clips the corners with a radius-8 <c>Region</c> and paints the
    ''' button background. On a row as wide as the menu the surface underneath showed through
    ''' the clipped corners, so the menu looked like grey pills glued on a white sheet, not a
    ''' list of commands. <c>DetachButton</c> removes the Region and, on the way, restores the
    ''' AUTHORED margin and height the Modern scheme had grown for its padding. The other
    ''' schemes round nothing and the call is idempotent, so the row comes out the same
    ''' everywhere.</para>
    ''' </summary>
    Friend Shared Sub ApplyMenuRow(b As Button, p As ThemePalette, textColor As Color)
        ModernRenderer.DetachButton(b)
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.BackColor = p.SurfaceColor
        b.ForeColor = textColor
        b.FlatAppearance.MouseOverBackColor = p.ButtonHoverColor
        b.FlatAppearance.MouseDownBackColor = p.ButtonPressedColor
        b.UseVisualStyleBackColor = False
    End Sub

    ''' <summary>The sum of a table's Absolute rows (the Percent ones belong to the elastic list).</summary>
    Friend Shared Function FixedRowsHeight(tlp As TableLayoutPanel) As Integer
        Dim total As Single = 0
        For i As Integer = 0 To tlp.RowStyles.Count - 1
            Dim rs As RowStyle = tlp.RowStyles(i)
            If rs.SizeType = SizeType.Absolute Then total += rs.Height
        Next
        Return CInt(Math.Ceiling(total))
    End Function

    ''' <summary>How much room a list wants to show its rows, up to <see cref="MaxListRows"/> (with its margins).</summary>
    Friend Shared Function ListHeight(lb As ListBox, itemCount As Integer) As Integer
        Dim rows As Integer = Math.Max(1, Math.Min(itemCount, MaxListRows))
        Return rows * lb.ItemHeight + lb.Margin.Vertical
    End Function

End Class
