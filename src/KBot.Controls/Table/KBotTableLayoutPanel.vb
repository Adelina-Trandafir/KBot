Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The house <c>TableLayoutPanel</c> (slice 0062): fixed rows and columns that follow the theme
''' and the application scale on their own, and cell borders in the palette's colour.
'''
''' <para><b>What it fixes.</b> The designer lays a table out on the Classic scheme: system
''' controls, no padding, the designer font. A 40px row is chosen looking at a 32px button. The
''' Modern scheme then grows that button to 48 (padding + font, see <c>ModernRenderer</c>) inside
''' a row that stayed 40 -- clipped text, controls on top of each other. Until now every form
''' had to wire <see cref="ThemeTableFit"/> by hand (and most did not).</para>
'''
''' <para><b>The rule for a fixed row/column</b> is the one from <see cref="ThemeTableFit"/>:
''' the AUTHORED measure plus exactly the surplus its greediest child asks for, and back to the
''' authored measure when the surplus goes away. Never "the row becomes as big as its content":
''' the air the operator left between controls would vanish at the first scheme switch.</para>
'''
''' <para><b>The baseline lives in platform pixels and follows the platform.</b> Measured (slice
''' 0062, a 150% monitor): WinForms' font autoscale DOES rescale Absolute styles together with
''' the client size (80 -> 120 -> 187 -> 120 as the form font went 9 -> 13.5 -> 9), and it scales
''' whatever value is CURRENT -- so a fitted value would be scaled again. Therefore the snapshot
''' of the authored styles is taken lazily (first <see cref="ApplyTheme"/> or
''' <see cref="RefreshDpiMetrics"/>, i.e. already after the platform's autoscale), and
''' <see cref="ScaleControl"/> multiplies the snapshot by the same factor the platform applies to
''' the live styles. Every rewrite is then "snapshot + surplus": the current, possibly already
''' grown value is never the source (C2).</para>
'''
''' <para><b><see cref="ScaleAbsoluteStyles"/></b> adds only what the platform does NOT do: under
''' <c>ScalingMode.Fixed100</c> / <c>Manual</c> our own measures (tree rows, grid rows, paint
''' constants) follow <c>AppScaling.FactorFor</c> while the platform keeps scaling by DPI x text
''' size through the font. The styles are multiplied by the ratio of the two, so a fixed column
''' keeps step with the drawn measures next to it. Under <c>Automatic</c> the ratio is exactly 1
''' and nothing is rewritten for scale.</para>
'''
''' <para><b><see cref="ResetStyleBaseline"/></b> exists because code writes styles at runtime
''' (a band collapsed to <c>Height = 0</c>, slice 0049-02). Without it the snapshot would revive
''' the band at the next theme switch. Only the styles that changed OUTSIDE this control are
''' re-based; the ones still at the value this control last wrote keep their authored snapshot,
''' so a reset cannot turn a fitted value into an authored one.</para>
'''
''' <para><b><see cref="ThemedCellBorder"/></b>: the native <c>CellBorderStyle</c> is drawn by
''' Windows in SYSTEM colours and cannot be recoloured -- the same defect class as the separator
''' <c>Label</c>s that went invisible under the generic Label rule. With it on, the panel paints
''' the grid itself, in <see cref="EffectiveCellBorderColor"/>, in the gap the native style
''' reserves (so the lines are not hidden under docked children).</para>
'''
''' <para><c>Control.Visible</c> is never read on the fit path (slices 0030, 0049-02): the getter
''' answers about the parent chain, so on a form not yet shown everything reports False.</para>
'''
''' <para>Doc: <c>Table/KBotTableLayoutPanel.md</c>. Conventions C1..C9 in <c>CONTROLS.md</c>.</para>
''' </summary>
<ToolboxItem(True)>
Public Class KBotTableLayoutPanel
    Inherits TableLayoutPanel
    Implements IThemedContainer, IDpiScaledControl

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

    ' ── Baseline of Absolute styles, in platform pixels (Nothing = not captured yet) ──
    Private _baseCols As Single()
    Private _baseRows As Single()
    ' What this control last WROTE, per style (NaN = never written). Lets ResetStyleBaseline tell
    ' an outside write from our own fitted value.
    Private _lastCols As Single()
    Private _lastRows As Single()

    ' A platform scale rounds to whole pixels (measured); our snapshot does not.
    Private Const SameValueTolerance As Single = 0.75F

    Public Sub New()
        SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
    End Sub

    ' ═══ Switches ═════════════════════════════════════════════════════════════

    ''' <summary>True (default): fixed rows/columns grow to their themed content (<see cref="ThemeTableFit"/> rule).</summary>
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
            Refit()
        End Set
    End Property

    ''' <summary>True (default): Absolute styles follow <c>AppScaling.FactorFor</c> where it differs from the platform's own scaling.</summary>
    <Category("K-BOT")>
    <DefaultValue(True)>
    <Description("Măsurile fixe urmează scara K-BOT (Fix 100% / Manual) acolo unde ea diferă de scalarea Windows.")>
    Public Property ScaleAbsoluteStyles As Boolean
        Get
            Return _scaleAbsoluteStyles
        End Get
        Set(value As Boolean)
            If _scaleAbsoluteStyles = value Then Return
            _scaleAbsoluteStyles = value
            Refit()
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

    ' ═══ Theme + scale ════════════════════════════════════════════════════════

    ''' <summary>
    ''' Called by <c>ThemeManager.Traverse</c> AFTER the children were themed (this is an
    ''' <see cref="IThemedContainer"/>): takes the colours, then refits the fixed styles against
    ''' the content as the scheme left it.
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
            Refit()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.ApplyTheme", ex)
        End Try
    End Sub

    Private Sub ApplyThemeColors()
        If Not _backPinned Then MyBase.BackColor = _surfaceTheme
    End Sub

    ''' <summary>Called by <c>AppScaling.Broadcast</c> after the fonts were rescaled: refits from the snapshot.</summary>
    Public Sub RefreshDpiMetrics() Implements IDpiScaledControl.RefreshDpiMetrics
        Try
            Refit()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.RefreshDpiMetrics", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Re-reads the authored styles from the CURRENT values -- but only for the styles somebody
    ''' else changed. A style still at the value this control last wrote keeps its snapshot, so
    ''' a fitted value can never become the authored one. Call it right after writing a style at
    ''' runtime (collapsing a band to 0, for instance).
    ''' </summary>
    Public Sub ResetStyleBaseline()
        Try
            If _baseCols Is Nothing Then Return   ' nothing captured yet: the first Refit reads the live values anyway
            RebaseChanged(ColumnStyles, _baseCols, _lastCols)
            RebaseChanged(RowStyles, _baseRows, _lastRows)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.ResetStyleBaseline", ex)
            Throw
        End Try
    End Sub

    Private Shared Sub RebaseChanged(styles As TableLayoutStyleCollection, base As Single(), last As Single())
        For i As Integer = 0 To Math.Min(styles.Count, base.Length) - 1
            Dim current As Single = StyleValue(styles, i)
            If Single.IsNaN(last(i)) OrElse Math.Abs(current - last(i)) > SameValueTolerance Then
                base(i) = current
                last(i) = Single.NaN
            End If
        Next
    End Sub

    ''' <summary>The authored (snapshot) measure of a column, or -1 when nothing was captured. Test seam.</summary>
    Public Function DebugAuthoredColumn(index As Integer) As Single
        If _baseCols Is Nothing OrElse index < 0 OrElse index >= _baseCols.Length Then Return -1.0F
        Return _baseCols(index)
    End Function

    ''' <summary>The authored (snapshot) measure of a row, or -1 when nothing was captured. Test seam.</summary>
    Public Function DebugAuthoredRow(index As Integer) As Single
        If _baseRows Is Nothing OrElse index < 0 OrElse index >= _baseRows.Length Then Return -1.0F
        Return _baseRows(index)
    End Function

    ''' <summary>True once the snapshot of the authored styles was taken.</summary>
    <Browsable(False)>
    Public ReadOnly Property HasStyleBaseline As Boolean
        Get
            Return _baseCols IsNot Nothing
        End Get
    End Property

    ' The platform rescaled the live styles by this factor (measured: yes, it does, and it
    ' rounds); the snapshot must follow, or the next refit would undo the platform's work.
    Protected Overrides Sub ScaleControl(factor As SizeF, specified As BoundsSpecified)
        MyBase.ScaleControl(factor, specified)
        Try
            If _baseCols Is Nothing Then Return
            ScaleArray(_baseCols, factor.Width)
            ScaleArray(_baseRows, factor.Height)
            ScaleArray(_lastCols, factor.Width)
            ScaleArray(_lastRows, factor.Height)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.ScaleControl", ex)
        End Try
    End Sub

    Private Shared Sub ScaleArray(values As Single(), factor As Single)
        If values Is Nothing OrElse factor <= 0F OrElse Single.IsNaN(factor) OrElse Single.IsInfinity(factor) Then Return
        For i As Integer = 0 To values.Length - 1
            If Not Single.IsNaN(values(i)) Then values(i) *= factor
        Next
    End Sub

    ' ── The fit itself ─────────────────────────────────────────────────────────

    ' Lazily takes the snapshot; the platform's autoscale has already been over the styles by the
    ' time anybody calls a theme or scale hook, so the snapshot is "authored x autoscale".
    Private Sub EnsureBaseline()
        If _baseCols IsNot Nothing AndAlso _baseCols.Length = ColumnStyles.Count AndAlso
           _baseRows.Length = RowStyles.Count Then Return
        ' First capture, or the style count changed under us (rows added at runtime): a fresh
        ' snapshot from the live values is the only honest answer.
        _baseCols = ReadStyles(ColumnStyles)
        _baseRows = ReadStyles(RowStyles)
        _lastCols = NaNs(ColumnStyles.Count)
        _lastRows = NaNs(RowStyles.Count)
    End Sub

    Private Shared Function ReadStyles(styles As TableLayoutStyleCollection) As Single()
        Dim m(Math.Max(0, styles.Count - 1)) As Single
        For i As Integer = 0 To styles.Count - 1
            m(i) = StyleValue(styles, i)
        Next
        If styles.Count = 0 Then Return New Single() {}
        Return m
    End Function

    Private Shared Function NaNs(count As Integer) As Single()
        Dim m(Math.Max(0, count - 1)) As Single
        For i As Integer = 0 To m.Length - 1
            m(i) = Single.NaN
        Next
        If count = 0 Then Return New Single() {}
        Return m
    End Function

    Private Shared Function StyleValue(styles As TableLayoutStyleCollection, index As Integer) As Single
        Dim rs As RowStyle = TryCast(styles(index), RowStyle)
        If rs IsNot Nothing Then Return rs.Height
        Return DirectCast(styles(index), ColumnStyle).Width
    End Function

    Private Shared Function IsAbsolute(styles As TableLayoutStyleCollection, index As Integer) As Boolean
        If index < 0 OrElse index >= styles.Count Then Return False
        Return styles(index).SizeType = SizeType.Absolute
    End Function

    ''' <summary>
    ''' The factor our modes add on top of the platform's own scaling. Exactly 1 under
    ''' <c>Automatic</c> (the platform already did DPI x text size through the font); under
    ''' <c>Fixed100</c>/<c>Manual</c> the ratio between what our drawn measures use and what the
    ''' platform applied. Also 1 at design time and when the switch is off.
    ''' </summary>
    Private Function OwnScaleRatio() As Single
        If Not _scaleAbsoluteStyles OrElse KBotDesignTime.IsDesignTime(Me) Then Return 1.0F
        If AppScaling.Mode = ScalingMode.Automatic Then Return 1.0F
        Dim platform As Single = CSng(DeviceDpi / 96.0) * AppScaling.TextScale
        If platform <= 0F Then Return 1.0F
        Return AppScaling.FactorFor(Me) / platform
    End Function

    ''' <summary>
    ''' Rewrites every Absolute style from the snapshot: authored x own ratio, plus the surplus
    ''' the greediest child in that row/column asks for. Percent and AutoSize styles are never
    ''' touched; a child spanning several rows/columns is skipped on that axis. Writes only when
    ''' the value changes.
    ''' </summary>
    Private Sub Refit()
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            If ColumnStyles.Count = 0 AndAlso RowStyles.Count = 0 Then Return
            EnsureBaseline()

            Dim ratio As Single = OwnScaleRatio()
            Dim nrC As Integer = ColumnStyles.Count
            Dim nrR As Integer = RowStyles.Count
            Dim surplusC(Math.Max(0, nrC - 1)) As Single
            Dim surplusR(Math.Max(0, nrR - 1)) As Single

            If _autoFitToTheme Then
                For Each c As Control In Controls
                    If c Is Nothing Then Continue For
                    Dim wants As Size = Demand(c)
                    If GetColumnSpan(c) = 1 Then
                        Dim col As Integer = GetColumn(c)
                        If IsAbsolute(ColumnStyles, col) AndAlso col < _baseCols.Length Then
                            surplusC(col) = Math.Max(surplusC(col), wants.Width - _baseCols(col) * ratio)
                        End If
                    End If
                    If GetRowSpan(c) = 1 Then
                        Dim row As Integer = GetRow(c)
                        If IsAbsolute(RowStyles, row) AndAlso row < _baseRows.Length Then
                            surplusR(row) = Math.Max(surplusR(row), wants.Height - _baseRows(row) * ratio)
                        End If
                    End If
                Next
            End If

            SuspendLayout()
            Try
                For col As Integer = 0 To nrC - 1
                    If Not IsAbsolute(ColumnStyles, col) OrElse col >= _baseCols.Length Then Continue For
                    Dim want As Single = _baseCols(col) * ratio + Math.Max(0F, surplusC(col))
                    If Math.Abs(ColumnStyles(col).Width - want) > 0.01F Then ColumnStyles(col).Width = want
                    _lastCols(col) = want
                Next
                For row As Integer = 0 To nrR - 1
                    If Not IsAbsolute(RowStyles, row) OrElse row >= _baseRows.Length Then Continue For
                    Dim want As Single = _baseRows(row) * ratio + Math.Max(0F, surplusR(row))
                    If Math.Abs(RowStyles(row).Height - want) > 0.01F Then RowStyles(row).Height = want
                    _lastRows(row) = want
                Next
            Finally
                ResumeLayout(True)
            End Try
        Catch ex As Exception
            ' Theme/scale boundary: a failed fit leaves the table on its previous measures.
            GlobalErrorLog.Write("KBotTableLayoutPanel.Refit", ex)
        End Try
    End Sub

    ''' <summary>
    ''' What a child asks for NOW, margins included -- never its Width/Height: the table clips a
    ''' docked child to its cell, so a 56px button in a 40px cell reports 40 and would never ask
    ''' for more (slice 0030). A leaf answers through its own <c>GetPreferredSize</c>; a container
    ''' is measured by its content (<see cref="ThemeFormFit.ContentDemand"/>), because its
    ''' <c>GetPreferredSize</c> would echo the cell it sits in and the row would never come back
    ''' from a growth. Neither reads the cell.
    ''' </summary>
    Private Shared Function Demand(c As Control) As Size
        Dim d As Size = ThemeFormFit.ContentDemand(c)
        Return New Size(d.Width + c.Margin.Horizontal, d.Height + c.Margin.Vertical)
    End Function

    ''' <summary>
    ''' The table's demand, from CONTENT only: every Absolute row/column at Max(its style, what
    ''' its children ask), every Percent/AutoSize one at what its children ask (0 when they are
    ''' containers with nothing fixed inside), plus the cell gaps and the padding. It never reads
    ''' the table's own size, so it can be asked before the first fit (the 0030 failure: a 56px
    ''' button in a 40px cell reported 40) and it is stable after it. Children spanning several
    ''' cells are not attributed to any of them.
    '''
    ''' <para>With <see cref="AutoFitToTheme"/> off the base answer is returned: the size is the
    ''' designer's business then.</para>
    ''' </summary>
    Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
        Try
            If Not _autoFitToTheme OrElse KBotDesignTime.IsDesignTime(Me) Then Return MyBase.GetPreferredSize(proposedSize)
            Dim nrC As Integer = ColumnStyles.Count
            Dim nrR As Integer = RowStyles.Count
            If nrC = 0 OrElse nrR = 0 Then Return MyBase.GetPreferredSize(proposedSize)

            Dim wantC(nrC - 1) As Single
            Dim wantR(nrR - 1) As Single
            For Each c As Control In Controls
                If c Is Nothing Then Continue For
                Dim d As Size = Demand(c)
                If GetColumnSpan(c) = 1 Then
                    Dim col As Integer = GetColumn(c)
                    If col >= 0 AndAlso col < nrC Then wantC(col) = Math.Max(wantC(col), d.Width)
                End If
                If GetRowSpan(c) = 1 Then
                    Dim row As Integer = GetRow(c)
                    If row >= 0 AndAlso row < nrR Then wantR(row) = Math.Max(wantR(row), d.Height)
                End If
            Next

            Dim gap As Integer = NativeBorderWidth()
            Dim w As Single = Padding.Horizontal + gap * (nrC + 1)
            For col As Integer = 0 To nrC - 1
                w += If(ColumnStyles(col).SizeType = SizeType.Absolute, Math.Max(ColumnStyles(col).Width, wantC(col)), wantC(col))
            Next
            Dim h As Single = Padding.Vertical + gap * (nrR + 1)
            For row As Integer = 0 To nrR - 1
                h += If(RowStyles(row).SizeType = SizeType.Absolute, Math.Max(RowStyles(row).Height, wantR(row)), wantR(row))
            Next
            Return New Size(CInt(Math.Ceiling(w)), CInt(Math.Ceiling(h)))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.GetPreferredSize", ex)
            Return MyBase.GetPreferredSize(proposedSize)
        End Try
    End Function

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

    ' The gap the native style reserves around every cell, in device pixels (WinForms' own table).
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
