Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' DPI SCALING of the table's own measures (slice 0066) -- the pair of
''' <c>AdvancedTreeControl.Dpi.vb</c> and <c>KBotDataView.Dpi.vb</c>, same disease, same cure.
'''
''' <para><b>Two values for every measure.</b> The AUTHORED value is logical (px at 96 dpi):
''' what the designer wrote, what <see cref="Padding"/> returns, what the tests read. The LIVE
''' value -- the <c>RowStyle</c>/<c>ColumnStyle</c> the layout engine reads, <c>MyBase.Padding</c>
''' -- is the authored value times the scale, in whole pixels. The scale is
''' <c>AppScaling.FactorFor(Me)</c>: <c>DeviceDpi / 96</c> x the operator's text size under
''' <c>Automatic</c>, 1 under <c>Fixed100</c>, the operator's number under <c>Manual</c>. One
''' source, the same the tree and the grid read, so a 40px row and a 22px tree item keep their
''' proportion on every screen.</para>
'''
''' <para><b>Why the platform's factor is not the source.</b> WinForms' font autoscale rewrites
''' Absolute styles and Padding with the ratio between two font heights, which at 150% measured
''' 1.43 on X and 1.67 on Y (slice 0066 probe). It also rewrites them AGAIN at every font change,
''' multiplying whatever is current. So the platform stays the TRIGGER (<see cref="ScaleControl"/>
''' is where it does its work) but never the source: after every base call the live values are
''' written back from the logical snapshot. Nothing is ever derived from a live value, so two
''' passes cannot compound.</para>
'''
''' <para><b>When the snapshot is taken.</b> At the first hook that runs, whichever it is:
''' <see cref="ScaleControl"/> BEFORE the base call (the platform's first autoscale is the first
''' thing that touches the styles after <c>InitializeComponent</c>, so what it finds is the
''' authored value), <c>OnHandleCreated</c>, <c>ApplyTheme</c> or <see cref="RefreshDpiMetrics"/>
''' for a table built in code and added to a form already scaled. Rows or columns added at
''' runtime (a style count that changed) are read from their live value, unscaled.</para>
'''
''' <para><b>In the designer nothing is scaled</b> (C6): the scale reads 1, the live values are
''' the authored ones, and the serializer writes what the operator typed.</para>
''' </summary>
Partial Public Class KBotTableLayoutPanel
    ' Declared HERE, in the partial that owns the scale: the interface has one member, and it is
    ' right below. VB accepts an Implements in any partial of the class.
    Implements IDpiScaledControl

    ' The scale of our own measures. 1 = 96 dpi.
    Private _dpiScale As Single = 1.0F

    ' The AUTHORED measures of the styles, logical px. Nothing = not captured yet. Percent and
    ' AutoSize entries hold the style's raw number and are never written back.
    Private _logicalCols As Single()
    Private _logicalRows As Single()
    ' What this control last WROTE per style, device px (NaN = never written). Lets
    ' ResetStyleBaseline tell an outside write from our own computed value.
    Private _lastCols As Single()
    Private _lastRows As Single()
    ' Rows/columns collapsed through the API: written as 0 whatever their authored measure.
    Private ReadOnly _collapsedRows As New HashSet(Of Integer)()
    Private ReadOnly _collapsedCols As New HashSet(Of Integer)()
    ' The table's own padding, logical. MyBase.Padding carries the device value.
    Private _logicalPadding As Padding = Padding.Empty

    ' A platform scale rounds to whole pixels (measured); a "same value" test needs a tolerance.
    Private Const SameValueTolerance As Single = 0.75F

    ''' <summary>The current scale of the table's own measures (1 = 96 dpi). Diagnostic, not a setting.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property DpiScale As Single
        Get
            Return _dpiScale
        End Get
    End Property

    ''' <summary>Logical (px @96dpi) to device px on the horizontal axis, whole pixels.</summary>
    Friend Function SX(logical As Single) As Single
        Return CSng(Math.Round(logical * StyleScale()))
    End Function

    ''' <summary>Logical (px @96dpi) to device px on the vertical axis, whole pixels.</summary>
    Friend Function SY(logical As Single) As Single
        Return CSng(Math.Round(logical * StyleScale()))
    End Function

    ''' <summary>The way back: a device value to its logical measure (an outside write, a row added at runtime).</summary>
    Friend Function Unscale(device As Single) As Single
        Dim k As Single = StyleScale()
        If k <= 0F Then Return device
        Return device / k
    End Function

    ' The scale the styles and the padding use: the DPI scale, or 1 when the operator switched
    ' scaling off for this table (ScaleAbsoluteStyles) and at design time.
    Private Function StyleScale() As Single
        If Not _scaleAbsoluteStyles Then Return 1.0F
        Return _dpiScale
    End Function

    ' ═══ Padding: logical in, device out ══════════════════════════════════════

    ''' <summary>
    ''' The table's own padding, in LOGICAL pixels (96 dpi) -- what the designer wrote. The value
    ''' the layout engine reads (<c>MyBase.Padding</c>) is this times the scale. <c>Control.Padding</c>
    ''' is not virtual, so this shadows it: code that holds the table as a <c>Control</c> (layout
    ''' engine, <c>ThemeFormFit</c>) keeps reading the device value it expects.
    ''' </summary>
    <Category("Layout")>
    <Description("Marginea interioară a tabelului, în pixeli logici (96 dpi); se calculează pentru ecran la scara K-BOT.")>
    Public Shadows Property Padding As Padding
        Get
            Return _logicalPadding
        End Get
        Set(value As Padding)
            _logicalPadding = value
            MyBase.Padding = ScalePadding(value)
        End Set
    End Property
    Public Function ShouldSerializePadding() As Boolean
        Return _logicalPadding <> Padding.Empty
    End Function
    Public Sub ResetPadding()
        Padding = Padding.Empty
    End Sub

    ''' <summary>The padding the layout engine is using right now, in device pixels.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property PaddingPx As Padding
        Get
            Return MyBase.Padding
        End Get
    End Property

    Private Function ScalePadding(logical As Padding) As Padding
        Return New Padding(CInt(SX(logical.Left)), CInt(SY(logical.Top)), CInt(SX(logical.Right)), CInt(SY(logical.Bottom)))
    End Function

    ' ═══ Triggers ═════════════════════════════════════════════════════════════

    ' Re-reads the scale. The answer comes from AppScaling -- the single source -- because the
    ' operator can fix it at 100% or type a factor; under Automatic it is exactly DeviceDpi / 96
    ' times the text size. 1 at design time (C6), unlike the tree: the VS surface stamps device
    ' pixels into a table's styles, so scaling them again there would show them twice as big.
    Private Function RefreshDpiScale() As Boolean
        Dim fresh As Single = If(KBotDesignTime.IsDesignTime(Me), 1.0F, AppScaling.FactorFor(Me))
        If fresh <= 0F OrElse Single.IsNaN(fresh) OrElse Single.IsInfinity(fresh) OrElse fresh = _dpiScale Then Return False
        _dpiScale = fresh
        Return True
    End Function

    ''' <summary>
    ''' The platform's autoscale. The base multiplies the live Absolute styles and the padding by
    ''' ITS factor; the snapshot is taken BEFORE that (first call = authored values), and after the
    ''' base the live values are written again from the logical source. The platform's number
    ''' therefore never survives, on any axis.
    ''' </summary>
    Protected Overrides Sub ScaleControl(factor As SizeF, specified As BoundsSpecified)
        Try
            EnsureBaseline()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.ScaleControl", ex)
        End Try
        MyBase.ScaleControl(factor, specified)
        Try
            SyncDpiScale()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.ScaleControl", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The handle is the moment <c>DeviceDpi</c> is certain, and the first hook for a table that
    ''' never goes through an autoscale (built in code, added to a form already shown).
    ''' </summary>
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Try
            SyncDpiScale()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.OnHandleCreated", ex)
        End Try
    End Sub

    ' Moved to a monitor with another scale -- the certain signal, whether or not the parent
    ' rescales its children.
    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        Try
            SyncDpiScale()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.OnDpiChangedAfterParent", ex)
        End Try
    End Sub

    ''' <summary>
    ''' <see cref="IDpiScaledControl.RefreshDpiMetrics"/> -- the gate through which
    ''' <c>AppScaling.Broadcast</c> reaches these measures when the operator changes the scaling
    ''' mode or the text size. Idempotent: everything is recomputed from the logical source.
    ''' </summary>
    Public Sub RefreshDpiMetrics() Implements IDpiScaledControl.RefreshDpiMetrics
        Try
            SyncDpiScale()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.RefreshDpiMetrics", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Recomputes padding and fixed styles from their logical measures and the content, now.
    ''' The same pass the theme and the scale run; public for a host that changed the content
    ''' (a caption grew, a control appeared) and wants the rows to follow before it measures
    ''' the window.
    ''' </summary>
    Public Sub RefitToTheme()
        Try
            SyncDpiScale()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.RefitToTheme", ex)
            Throw
        End Try
    End Sub

    ' Snapshot if needed, re-read the scale, recompute everything. Always applies: the pass is
    ' idempotent and writes a style only when its value differs.
    Friend Sub SyncDpiScale()
        EnsureBaseline()
        RefreshDpiScale()
        ApplyMetricScale()
    End Sub

    ''' <summary>
    ''' Rebuilds EVERY live measure from its logical pair: the padding, then the Absolute styles
    ''' (scaled authored measure + surplus, or 0 when collapsed -- see the <c>.Fit</c> partial).
    ''' Idempotent: computes from the logical value, never composes over the live one.
    ''' </summary>
    Private Sub ApplyMetricScale()
        Try
            If KBotDesignTime.IsDesignTime(Me) Then
                ' The designer sees exactly what was authored.
                If MyBase.Padding <> _logicalPadding Then MyBase.Padding = _logicalPadding
                Return
            End If
            Dim wanted As Padding = ScalePadding(_logicalPadding)
            If MyBase.Padding <> wanted Then MyBase.Padding = wanted
            Refit()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.ApplyMetricScale", ex)
        End Try
    End Sub

    ' ═══ The logical source of the styles ═════════════════════════════════════

    ' Takes the snapshot at the first hook; the live values are the authored ones then (nothing
    ' has scaled them yet -- see the class summary for why the first ScaleControl qualifies).
    ' When the style count changed under us (rows added or removed at runtime) the arrays are
    ' rebuilt: an entry still at the value this control last wrote keeps its logical measure, any
    ' other is read from the live value and unscaled -- a style written at runtime is a device
    ' value like every live style.
    Private Sub EnsureBaseline()
        If _logicalCols IsNot Nothing AndAlso _logicalCols.Length = ColumnStyles.Count AndAlso
           _logicalRows.Length = RowStyles.Count Then Return
        _logicalCols = Rebase(ColumnStyles, _logicalCols, _lastCols)
        _logicalRows = Rebase(RowStyles, _logicalRows, _lastRows)
        _lastCols = NaNs(ColumnStyles.Count)
        _lastRows = NaNs(RowStyles.Count)
    End Sub

    Private Function Rebase(styles As TableLayoutStyleCollection, old As Single(), last As Single()) As Single()
        Dim m As Single() = New Single(Math.Max(0, styles.Count - 1)) {}
        If styles.Count = 0 Then Return New Single() {}
        For i As Integer = 0 To styles.Count - 1
            Dim live As Single = StyleValue(styles, i)
            If old Is Nothing Then
                m(i) = live                                     ' first capture: authored = logical
            ElseIf i < old.Length AndAlso i < last.Length AndAlso Not Single.IsNaN(last(i)) AndAlso
                   Math.Abs(live - last(i)) <= SameValueTolerance Then
                m(i) = old(i)                                   ' untouched since we wrote it
            ElseIf IsAbsolute(styles, i) Then
                m(i) = Unscale(live)                            ' written at runtime, in device px
            Else
                m(i) = live                                     ' Percent / AutoSize: a raw number
            End If
        Next
        Return m
    End Function

    Private Shared Function NaNs(count As Integer) As Single()
        If count = 0 Then Return New Single() {}
        Dim m As Single() = New Single(count - 1) {}
        For i As Integer = 0 To m.Length - 1
            m(i) = Single.NaN
        Next
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
    ''' Re-reads the authored measure from the LIVE value -- but only for the styles somebody
    ''' else changed (a <c>RowStyle.Height</c> written directly, in device pixels). A style still
    ''' at the value this control last wrote keeps its logical measure, so a computed value can
    ''' never become the authored one. The preferred path is <see cref="SetRowHeight"/> /
    ''' <see cref="SetRowCollapsed"/>, which need no reset; this is the escape hatch for code
    ''' that writes the styles itself.
    ''' </summary>
    Public Sub ResetStyleBaseline()
        Try
            If _logicalCols Is Nothing Then Return   ' nothing captured yet: the first hook reads the live values anyway
            RebaseChanged(ColumnStyles, _logicalCols, _lastCols)
            RebaseChanged(RowStyles, _logicalRows, _lastRows)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.ResetStyleBaseline", ex)
            Throw
        End Try
    End Sub

    Private Sub RebaseChanged(styles As TableLayoutStyleCollection, logical As Single(), last As Single())
        For i As Integer = 0 To Math.Min(styles.Count, logical.Length) - 1
            Dim current As Single = StyleValue(styles, i)
            If Single.IsNaN(last(i)) OrElse Math.Abs(current - last(i)) > SameValueTolerance Then
                logical(i) = If(IsAbsolute(styles, i), Unscale(current), current)
                last(i) = Single.NaN
            End If
        Next
    End Sub

    ' ═══ Runtime writes, in logical pixels ════════════════════════════════════

    ''' <summary>
    ''' Writes the authored (logical) height of a row and makes it Absolute. The live style
    ''' becomes <c>logicalHeight x scale</c> (+ the content surplus); a collapse on that row is
    ''' lifted. Out-of-range index throws; a negative height is clamped to 0 (C3).
    ''' </summary>
    Public Sub SetRowHeight(index As Integer, logicalHeight As Single)
        Try
            CheckIndex(index, RowStyles.Count, "row")
            EnsureBaseline()
            RowStyles(index).SizeType = SizeType.Absolute
            _logicalRows(index) = Math.Max(0F, logicalHeight)
            _lastRows(index) = Single.NaN
            _collapsedRows.Remove(index)
            SyncDpiScale()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.SetRowHeight", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The column pair of <see cref="SetRowHeight"/>.</summary>
    Public Sub SetColumnWidth(index As Integer, logicalWidth As Single)
        Try
            CheckIndex(index, ColumnStyles.Count, "column")
            EnsureBaseline()
            ColumnStyles(index).SizeType = SizeType.Absolute
            _logicalCols(index) = Math.Max(0F, logicalWidth)
            _lastCols(index) = Single.NaN
            _collapsedCols.Remove(index)
            SyncDpiScale()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.SetColumnWidth", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Collapses a fixed row to 0 (or opens it again to its authored measure) and keeps it so
    ''' across every scale and theme pass. In a table with fixed rows a control set
    ''' <c>Visible = False</c> leaves a hole the size of its row; this is the other half, said
    ''' explicitly -- <c>Control.Visible</c> cannot be asked (see the class summary). Only an
    ''' Absolute row can be collapsed: a Percent or AutoSize one has no authored measure to come
    ''' back to, so asking throws (C3) -- <see cref="SetRowHeight"/> with 0 is the honest write
    ''' for those.
    ''' </summary>
    Public Sub SetRowCollapsed(index As Integer, collapsed As Boolean)
        Try
            CheckIndex(index, RowStyles.Count, "row")
            If Not IsAbsolute(RowStyles, index) Then
                Throw New ArgumentException("Row " & index & " is not Absolute; only a fixed row can be collapsed (SetRowHeight(index, 0) for the others).", NameOf(index))
            End If
            If collapsed Then
                If Not _collapsedRows.Add(index) Then Return
            Else
                If Not _collapsedRows.Remove(index) Then Return
            End If
            SyncDpiScale()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.SetRowCollapsed", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The column pair of <see cref="SetRowCollapsed"/>.</summary>
    Public Sub SetColumnCollapsed(index As Integer, collapsed As Boolean)
        Try
            CheckIndex(index, ColumnStyles.Count, "column")
            If Not IsAbsolute(ColumnStyles, index) Then
                Throw New ArgumentException("Column " & index & " is not Absolute; only a fixed column can be collapsed (SetColumnWidth(index, 0) for the others).", NameOf(index))
            End If
            If collapsed Then
                If Not _collapsedCols.Add(index) Then Return
            Else
                If Not _collapsedCols.Remove(index) Then Return
            End If
            SyncDpiScale()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTableLayoutPanel.SetColumnCollapsed", ex)
            Throw
        End Try
    End Sub

    ''' <summary>True when the row was collapsed through <see cref="SetRowCollapsed"/>.</summary>
    Public Function IsRowCollapsed(index As Integer) As Boolean
        Return _collapsedRows.Contains(index)
    End Function

    ''' <summary>True when the column was collapsed through <see cref="SetColumnCollapsed"/>.</summary>
    Public Function IsColumnCollapsed(index As Integer) As Boolean
        Return _collapsedCols.Contains(index)
    End Function

    Private Shared Sub CheckIndex(index As Integer, count As Integer, what As String)
        If index < 0 OrElse index >= count Then
            Throw New ArgumentOutOfRangeException(NameOf(index), "No " & what & " at index " & index & " (the table has " & count & ").")
        End If
    End Sub

    ' ═══ Seams ════════════════════════════════════════════════════════════════

    ''' <summary>The authored (logical) measure of a column, or -1 when nothing was captured. Test seam.</summary>
    Public Function DebugAuthoredColumn(index As Integer) As Single
        If _logicalCols Is Nothing OrElse index < 0 OrElse index >= _logicalCols.Length Then Return -1.0F
        Return _logicalCols(index)
    End Function

    ''' <summary>The authored (logical) measure of a row, or -1 when nothing was captured. Test seam.</summary>
    Public Function DebugAuthoredRow(index As Integer) As Single
        If _logicalRows Is Nothing OrElse index < 0 OrElse index >= _logicalRows.Length Then Return -1.0F
        Return _logicalRows(index)
    End Function

    ''' <summary>True once the snapshot of the authored styles was taken.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HasStyleBaseline As Boolean
        Get
            Return _logicalCols IsNot Nothing
        End Get
    End Property

End Class
