Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The house <c>TableLayoutPanel</c>: fixed rows, fixed columns and the table's own padding are
''' AUTHORED in logical pixels (96 dpi) and computed for the screen the same way the tree and
''' the grid compute their rows -- from <c>AppScaling.FactorFor</c>, never from the platform's
''' font ratio (slice 0066, on top of the 0062 fit).
'''
''' <para><b>What was wrong before 0066.</b> The platform's font autoscale rewrites a table's
''' Absolute styles and its Padding with the ratio between two font heights: measured on a 150%
''' monitor it is 1.43 on X and 1.67 on Y -- not 1.5, and not the same on both axes. Next to it
''' the tree drew 22px rows at exactly 33 (<c>AdvancedTreeControl.Dpi.vb</c>) and the grid did
''' the same. A 40px table row came out at 67 and the two never lined up. The 0062 control kept
''' a snapshot of what the platform had produced and corrected it only under Fixed100/Manual;
''' the Automatic case, the one every operator runs, kept the platform's numbers.</para>
'''
''' <para><b>The model now</b> (partial <c>.Dpi</c>): the authored measures are captured ONCE,
''' before anything scales them (the first <c>ScaleControl</c> call sees the values exactly as
''' <c>InitializeComponent</c> wrote them), and every live value is recomputed from them:
''' <c>authored x AppScaling.FactorFor(Me)</c>, whole pixels. The platform is still allowed to
''' multiply the live styles on every autoscale -- and every time it does, the control writes
''' them again from the logical source. Idempotent by construction: nothing is ever derived from
''' a live value. The children's <c>Margin</c>s stay the platform's: they are the children's
''' properties, and the tree does not reach into other controls either.</para>
'''
''' <para><b>The fit</b> (partial <c>.Fit</c>) is the 0062 rule unchanged: a fixed row/column is
''' its scaled authored measure PLUS exactly the surplus its greediest child asks for, and back
''' to the authored measure when the surplus goes away. Never "as big as the content": the air
''' the operator left between controls would vanish at the first scheme switch.</para>
'''
''' <para><b>Runtime writes go through the control</b>: <see cref="SetRowCollapsed"/>,
''' <see cref="SetColumnCollapsed"/>, <see cref="SetRowHeight"/>, <see cref="SetColumnWidth"/>
''' and the shadowed <see cref="Padding"/> keep the logical source in step. Writing a
''' <c>RowStyle</c> directly still works, but it is a write in DEVICE pixels that the next scale
''' or theme pass would undo -- <see cref="ResetStyleBaseline"/> is the escape hatch that adopts
''' such a write as the new authored value.</para>
'''
''' <para><b><see cref="ThemedCellBorder"/></b>: the native <c>CellBorderStyle</c> is drawn by
''' Windows in SYSTEM colours and cannot be recoloured. With it on, the panel paints the grid
''' itself, in <see cref="EffectiveCellBorderColor"/>, in the gap the native style reserves.</para>
'''
''' <para><c>Control.Visible</c> is never read on the fit path (slices 0030, 0049-02): the getter
''' answers about the parent chain, so on a form not yet shown everything reports False.</para>
'''
''' <para>Doc: <c>Table/KBotTableLayoutPanel.md</c>. Conventions C1..C9 in <c>CONTROLS.md</c>.</para>
''' </summary>
<ToolboxItem(True)>
Public Class KBotTableLayoutPanel
    Inherits TableLayoutPanel
    Implements IThemedContainer

    ' ── Switches ──────────────────────────────────────────────────────────────
    Private _autoFitToTheme As Boolean = True
    Private _scaleAbsoluteStyles As Boolean = True
    Private _themedCellBorder As Boolean = False

    ' ── Colours: theme value + operator pin (C1) ──────────────────────────────
    Private _borderTheme As Color = Color.Gray
    Private _borderPinned As Color = Color.Empty
    Private _surfaceTheme As Color = SystemColors.Control
    Private _backPinned As Boolean
    Private _forePinned As Boolean
    Private _fontPinned As Boolean

    Public Sub New()
        SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
    End Sub

    ' ═══ Switches ═════════════════════════════════════════════════════════════

    ''' <summary>True (default): fixed rows/columns grow to their themed content and come back (the 0062 rule).</summary>
    <Category("K-BOT")>
    <DefaultValue(True)>
    <Description("Rândurile și coloanele fixe cresc cât le cere conținutul tematizat și revin la măsura autorată.")>
    Public Property AutoFitToTheme As Boolean
        Get
            Return _autoFitToTheme
        End Get
        Set(value As Boolean)
            If _autoFitToTheme = value Then Return
            _autoFitToTheme = value
            SyncDpiScale()
        End Set
    End Property

    ''' <summary>
    ''' True (default): Absolute styles and the table's own <see cref="Padding"/> are computed for
    ''' the screen from their logical measure, at <c>AppScaling.FactorFor</c>. False: they stay at
    ''' the authored logical pixels whatever the screen (the platform's own scaling of them is
    ''' undone too) -- the same thing <c>ScalingMode.Fixed100</c> does for the whole application,
    ''' available per table.
    ''' </summary>
    <Category("K-BOT")>
    <DefaultValue(True)>
    <Description("Măsurile fixe și marginea interioară se calculează pentru ecran din valoarea logică (96 dpi), la scara K-BOT.")>
    Public Property ScaleAbsoluteStyles As Boolean
        Get
            Return _scaleAbsoluteStyles
        End Get
        Set(value As Boolean)
            If _scaleAbsoluteStyles = value Then Return
            _scaleAbsoluteStyles = value
            SyncDpiScale()
        End Set
    End Property

    ''' <summary>
    ''' True: the cell grid is painted here, in the theme's border colour, instead of Windows'
    ''' system-coloured lines. Needs a <c>CellBorderStyle</c> other than <c>None</c> for the gap
    ''' between cells; <c>None</c> is promoted to <c>Single</c> when this is switched on.
    ''' </summary>
    <Category("K-BOT")>
    <DefaultValue(False)>
    <Description("Liniile dintre celule se desenează în culoarea de chenar a temei, nu în culorile sistemului.")>
    Public Property ThemedCellBorder As Boolean
        Get
            Return _themedCellBorder
        End Get
        Set(value As Boolean)
            If _themedCellBorder = value Then Return
            _themedCellBorder = value
            If value AndAlso MyBase.CellBorderStyle = TableLayoutPanelCellBorderStyle.None Then
                MyBase.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            End If
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' The native style still decides the GAP between cells; with <see cref="ThemedCellBorder"/>
    ''' on, <c>None</c> is refused (no gap = nothing to paint in) -- C3, no silent no-op.
    ''' </summary>
    <DefaultValue(TableLayoutPanelCellBorderStyle.None)>
    Public Shadows Property CellBorderStyle As TableLayoutPanelCellBorderStyle
        Get
            Return MyBase.CellBorderStyle
        End Get
        Set(value As TableLayoutPanelCellBorderStyle)
            If _themedCellBorder AndAlso value = TableLayoutPanelCellBorderStyle.None Then
                Throw New ArgumentException(
                    "With ThemedCellBorder on, CellBorderStyle cannot be None: there would be no gap to paint the themed lines in.",
                    NameOf(value))
            End If
            MyBase.CellBorderStyle = value
        End Set
    End Property

    ' ═══ Colours (C1 + C4) ════════════════════════════════════════════════════

    ''' <summary>Colour of the themed cell lines; Empty = <c>BorderColor</c> from the theme.</summary>
    <Category("K-BOT")>
    <Description("Culoarea liniilor dintre celule; goală = BorderColor din temă.")>
    Public Property CellBorderColor As Color
        Get
            Return If(_borderPinned <> Color.Empty, _borderPinned, _borderTheme)
        End Get
        Set(value As Color)
            _borderPinned = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeCellBorderColor() As Boolean
        Return _borderPinned <> Color.Empty
    End Function
    Public Sub ResetCellBorderColor()
        _borderPinned = Color.Empty
        Invalidate()
    End Sub

    ''' <summary>The colour the cell lines are actually painted with.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveCellBorderColor As Color
        Get
            Return CellBorderColor
        End Get
    End Property

    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(value As Color)
            _backPinned = True
            MyBase.BackColor = value
        End Set
    End Property
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backPinned
    End Function
    ' The flag is cleared AFTER the base call: Control.ResetX assigns through the VIRTUAL setter,
    ' i.e. through our override, which would pin the value again (measured by the C4 test).
    Public Overrides Sub ResetBackColor()
        MyBase.ResetBackColor()
        _backPinned = False
        ApplyThemeColors()
    End Sub

    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(value As Color)
            _forePinned = True
            MyBase.ForeColor = value
        End Set
    End Property
    Public Function ShouldSerializeForeColor() As Boolean
        Return _forePinned
    End Function
    Public Overrides Sub ResetForeColor()
        MyBase.ResetForeColor()
        _forePinned = False
    End Sub

    Public Overrides Property Font As Font
        Get
            Return MyBase.Font
        End Get
        Set(value As Font)
            _fontPinned = True
            MyBase.Font = value
        End Set
    End Property
    Public Function ShouldSerializeFont() As Boolean
        Return _fontPinned
    End Function
    Public Overrides Sub ResetFont()
        MyBase.ResetFont()
        _fontPinned = False
    End Sub

    ' ═══ Theme ════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Called by <c>ThemeManager.Traverse</c> AFTER the children were themed (this is an
    ''' <see cref="IThemedContainer"/>): takes the colours, then recomputes padding and fixed
    ''' styles against the content as the scheme left it.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            _borderTheme = p.BorderColor
            ' Same surface as the generic TableLayoutPanel rule; "Card" tagged panels take the
            ' alternate surface, exactly as ThemeManager.IsCard does.
            Dim tag As String = TryCast(Me.Tag, String)
            _surfaceTheme = If(String.Equals(tag, "Card", StringComparison.OrdinalIgnoreCase), p.SurfaceAltColor, p.SurfaceColor)
            ApplyThemeColors()
            SyncDpiScale()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.ApplyTheme", ex)
        End Try
    End Sub

    Private Sub ApplyThemeColors()
        If Not _backPinned Then MyBase.BackColor = _surfaceTheme
    End Sub

    ' ═══ Painting: the themed grid ════════════════════════════════════════════

    ''' <summary>
    ''' With <see cref="ThemedCellBorder"/> on, the background is filled here and the grid is
    ''' painted in the theme colour in the gap the native style reserves; the base's system-colour
    ''' border painting is skipped. Otherwise the base paints as always.
    ''' </summary>
    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        Try
            If Not _themedCellBorder OrElse MyBase.CellBorderStyle = TableLayoutPanelCellBorderStyle.None Then
                MyBase.OnPaintBackground(e)
                Return
            End If

            Using b As New SolidBrush(BackColor)
                e.Graphics.FillRectangle(b, e.ClipRectangle)
            End Using
            PaintThemedGrid(e.Graphics)
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotTableLayoutPanel.OnPaintBackground", ex)
        End Try
    End Sub

    ' The gap the native style reserves around every cell, in device pixels (WinForms' own table;
    ' the platform never scales it).
    Private Function NativeBorderWidth() As Integer
        Select Case MyBase.CellBorderStyle
            Case TableLayoutPanelCellBorderStyle.None : Return 0
            Case TableLayoutPanelCellBorderStyle.Single : Return 1
            Case TableLayoutPanelCellBorderStyle.Inset, TableLayoutPanelCellBorderStyle.Outset : Return 2
            Case Else : Return 3
        End Select
    End Function

    ''' <summary>
    ''' Lines on the cell boundaries. Column widths and row heights from the layout engine
    ''' already include the reserved gap, so the k-th vertical line sits at the sum of the first
    ''' k widths, offset by the display rectangle; the outer frame closes the grid.
    ''' </summary>
    Private Sub PaintThemedGrid(g As Graphics)
        Dim widths As Integer() = GetColumnWidths()
        Dim heights As Integer() = GetRowHeights()
        If widths Is Nothing OrElse heights Is Nothing OrElse widths.Length = 0 OrElse heights.Length = 0 Then Return

        Dim gap As Integer = NativeBorderWidth()
        Dim area As Rectangle = DisplayRectangle
        Dim totalW As Integer = 0
        For Each w As Integer In widths
            totalW += w
        Next
        Dim totalH As Integer = 0
        For Each h As Integer In heights
            totalH += h
        Next

        Using p As New Pen(EffectiveCellBorderColor, gap)
            p.Alignment = Drawing2D.PenAlignment.Inset
            ' Outer frame around the whole grid.
            Dim frame As New Rectangle(area.Left, area.Top, totalW, totalH)
            g.DrawRectangle(p, frame)
            ' Inner vertical lines: after each column but the last.
            Dim x As Integer = area.Left
            For i As Integer = 0 To widths.Length - 2
                x += widths(i)
                g.DrawLine(p, x, area.Top, x, area.Top + totalH)
            Next
            ' Inner horizontal lines: after each row but the last.
            Dim y As Integer = area.Top
            For i As Integer = 0 To heights.Length - 2
                y += heights(i)
                g.DrawLine(p, area.Left, y, area.Left + totalW, y)
            Next
        End Using
    End Sub

End Class
