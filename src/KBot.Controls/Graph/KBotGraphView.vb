Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' An owner-drawn map of dots and lines - the K-BOT control for "what is connected to what".
''' </summary>
''' <remarks>
''' <para><b>What it draws.</b> <see cref="KBotGraphNode"/> dots joined by <see cref="KBotGraphEdge"/>
''' lines, on a surface the operator can zoom (wheel) and move (drag the background). Resting the
''' pointer on a dot opens a floating <see cref="KBotToolTip"/> and fades everything that is not
''' that dot or one of its neighbours, which is how "what is this linked to" is read at a glance.
''' Clicking a dot selects it and keeps the fade.</para>
''' <para><b>It never lays out by itself.</b> The positions are WORLD coordinates on each node, set by
''' the host - usually from <see cref="KBotGraphLayout.Compute"/>. Showing and hiding nodes
''' (<see cref="KBotGraphNode.Visible"/>) never moves the others, so a host can step a graph through
''' time and the picture stays still while it fills up.</para>
''' <para><b>Colours.</b> None written here: <c>Color.Empty</c> = from the theme (C1). A dot without a
''' colour takes the scheme's accent; <see cref="AutoColor"/> hands out the n-th colour of the same set
''' the chart and the lanes use, so one thing can carry one colour on every surface.</para>
''' <para>Pixel metrics are LOGICAL pixels (96 dpi), scaled at paint time (C2).</para>
''' </remarks>
<ToolboxItem(True)>
<DefaultEvent("NodeClicked")>
Partial Public NotInheritable Class KBotGraphView
    Inherits Control
    Implements IThemedControl

    Private ReadOnly _nodes As New List(Of KBotGraphNode)()
    Private ReadOnly _edges As New List(Of KBotGraphEdge)()
    Private ReadOnly _byKey As New Dictionary(Of String, KBotGraphNode)(StringComparer.Ordinal)
    Private _neighbours As List(Of Integer)()
    Private _updateDepth As Integer

    ' ---- appearance ----
    Private _nodeRadius As Integer = 4
    Private _edgeWidth As Integer = 1
    Private _edgeOpacity As Integer = 45
    Private _labelsVisible As Boolean
    Private _hoverRadius As Integer = 6
    Private _emptyText As String = String.Empty
    Private _plotBackColor As Color = Color.Empty
    Private _edgeColor As Color = Color.Empty
    Private _pointTooltipEnabled As Boolean = True

    ' ---- view ----
    Private _zoom As Double = 1.0
    Private _panX As Double
    Private _panY As Double
    Private _needsFit As Boolean = True

    ' ---- runtime state ----
    Private _scheme As ThemeScheme
    Private _isDarkScheme As Boolean
    Private _hoverIndex As Integer = -1
    Private _selectedIndex As Integer = -1
    Private _panning As Boolean
    Private _pressed As Boolean
    Private _pressPoint As Point
    Private _panStartX As Double
    Private _panStartY As Double
    Private _pressedIndex As Integer = -1
    Private _moved As Boolean
    Private _pointTooltip As KBotToolTip
    Private _currentTipKey As String
    Private ReadOnly _tipContent As New KBotToolTipContent()

    Private _edgePen As Pen
    Private _fillBrush As SolidBrush
    Private _labelBrush As SolidBrush
    Private _borderPen As Pen
    Private _ringPen As Pen

    Private _backColorPinned As Boolean
    Private _foreColorPinned As Boolean
    Private _fontPinned As Boolean

    ''' <summary>Raised on a left click on a dot, or on the empty background (key = <c>Nothing</c>, the selection is cleared).</summary>
    Public Event NodeClicked(k_key As String)

    ''' <summary>Raised on a double click on a dot.</summary>
    Public Event NodeDoubleClicked(k_key As String)

    ''' <summary>Raised when the dot under the pointer changes; the key is <c>Nothing</c> when the pointer leaves every dot.</summary>
    Public Event NodeHovered(k_key As String)

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable, True)
        TabStop = True
    End Sub

    ' =====================================================================
    ' INHERITED PROPERTIES WITH A PIN FLAG (C4)
    ' =====================================================================

    <Category("K-BOT Graph Appearance")>
    <Description("Background of the whole control. Not pinned here, it follows the active theme.")>
    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(k_value As Color)
            _backColorPinned = True
            MyBase.BackColor = k_value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColorPinned
    End Function

    Public Overrides Sub ResetBackColor()
        MyBase.ResetBackColor()
        _backColorPinned = False
        Invalidate()
    End Sub

    <Category("K-BOT Graph Appearance")>
    <Description("Default text colour of the graph. Not pinned here, it follows the active theme.")>
    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(k_value As Color)
            _foreColorPinned = True
            MyBase.ForeColor = k_value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeForeColor() As Boolean
        Return _foreColorPinned
    End Function

    Public Overrides Sub ResetForeColor()
        MyBase.ResetForeColor()
        _foreColorPinned = False
        Invalidate()
    End Sub

    <Category("K-BOT Graph Appearance")>
    <Description("Font of the graph. Not pinned here, it follows the ambient font of the active scheme.")>
    Public Overrides Property Font As Font
        Get
            Return MyBase.Font
        End Get
        Set(k_value As Font)
            _fontPinned = True
            MyBase.Font = k_value
        End Set
    End Property

    Public Function ShouldSerializeFont() As Boolean
        Return _fontPinned
    End Function

    Public Overrides Sub ResetFont()
        MyBase.ResetFont()
        _fontPinned = False
        Invalidate()
    End Sub

    Protected Overrides ReadOnly Property DefaultSize As Size
        Get
            Return New Size(420, 300)
        End Get
    End Property

    ' =====================================================================
    ' DATA
    ' =====================================================================

    ''' <summary>The dots, in the order they were added.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Nodes As IReadOnlyList(Of KBotGraphNode)
        Get
            Return _nodes
        End Get
    End Property

    ''' <summary>The lines, in the order they were added.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Edges As IReadOnlyList(Of KBotGraphEdge)
        Get
            Return _edges
        End Get
    End Property

    ''' <summary>Adds a dot. Empty or duplicate key = <see cref="ArgumentException"/> (C3).</summary>
    Public Function AddNode(k_key As String, k_text As String) As KBotGraphNode
        If String.IsNullOrWhiteSpace(k_key) Then Throw New ArgumentException("Empty node key.", NameOf(k_key))
        If _byKey.ContainsKey(k_key) Then Throw New ArgumentException($"Duplicate node key: '{k_key}'.", NameOf(k_key))
        Dim k_node As New KBotGraphNode(k_key, k_text) With {.Index = _nodes.Count}
        _nodes.Add(k_node)
        _byKey(k_key) = k_node
        _neighbours = Nothing
        Touch()
        Return k_node
    End Function

    ''' <summary>Adds a line between two existing dots. An unknown key = <see cref="ArgumentException"/> (C3).</summary>
    Public Function AddEdge(k_fromKey As String, k_toKey As String) As KBotGraphEdge
        Dim k_from As KBotGraphNode = RequireNode(k_fromKey)
        Dim k_to As KBotGraphNode = RequireNode(k_toKey)
        Dim k_edge As New KBotGraphEdge(k_from, k_to)
        _edges.Add(k_edge)
        _neighbours = Nothing
        Touch()
        Return k_edge
    End Function

    ''' <summary>The dot with this key, or <c>Nothing</c>.</summary>
    Public Function FindNode(k_key As String) As KBotGraphNode
        If k_key Is Nothing Then Return Nothing
        Dim k_node As KBotGraphNode = Nothing
        _byKey.TryGetValue(k_key, k_node)
        Return k_node
    End Function

    ''' <summary>Removes every dot and line and clears the selection. The view (zoom, pan) is kept.</summary>
    Public Sub ClearGraph()
        HideTip()
        _nodes.Clear()
        _edges.Clear()
        _byKey.Clear()
        _neighbours = Nothing
        _hoverIndex = -1
        _selectedIndex = -1
        _needsFit = True
        Touch()
    End Sub

    ''' <summary>Several changes, one repaint at the end.</summary>
    Public Sub BeginUpdate()
        _updateDepth += 1
    End Sub

    ''' <summary>Ends <see cref="BeginUpdate"/>.</summary>
    Public Sub EndUpdate()
        If _updateDepth > 0 Then _updateDepth -= 1
        If _updateDepth = 0 Then Invalidate()
    End Sub

    ''' <summary>Asks for a repaint after the host changed a node (Visible, Highlighted, colour) directly.</summary>
    Public Sub RefreshGraph()
        Invalidate()
    End Sub

    ''' <summary>
    ''' Works out the world positions of every dot with <see cref="KBotGraphLayout"/> (grouped by
    ''' <see cref="KBotGraphNode.Group"/>), writes them to the nodes and fits the view. On the UI thread;
    ''' for a big graph call <see cref="KBotGraphLayout.Compute"/> in the background and write the
    ''' positions yourself.
    ''' </summary>
    Public Sub LayoutNodes(Optional k_iterations As Integer = 120)
        Dim k_groupIds As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        Dim k_groupOf(_nodes.Count - 1) As Integer
        For k_i As Integer = 0 To _nodes.Count - 1
            Dim k_name As String = If(String.IsNullOrEmpty(_nodes(k_i).Group), "#" & k_i.ToString(Globalization.CultureInfo.InvariantCulture), _nodes(k_i).Group)
            Dim k_id As Integer
            If Not k_groupIds.TryGetValue(k_name, k_id) Then
                k_id = k_groupIds.Count
                k_groupIds(k_name) = k_id
            End If
            k_groupOf(k_i) = k_id
        Next
        Dim k_from(_edges.Count - 1) As Integer
        Dim k_to(_edges.Count - 1) As Integer
        For k_e As Integer = 0 To _edges.Count - 1
            k_from(k_e) = _edges(k_e).FromNode.Index
            k_to(k_e) = _edges(k_e).ToNode.Index
        Next
        Dim k_positions As PointF() = KBotGraphLayout.Compute(k_groupOf, k_from, k_to, k_iterations)
        For k_i As Integer = 0 To _nodes.Count - 1
            _nodes(k_i).X = k_positions(k_i).X
            _nodes(k_i).Y = k_positions(k_i).Y
        Next
        FitToView()
    End Sub

    Private Function RequireNode(k_key As String) As KBotGraphNode
        If String.IsNullOrWhiteSpace(k_key) Then Throw New ArgumentException("Empty node key.", NameOf(k_key))
        Dim k_node As KBotGraphNode = Nothing
        If Not _byKey.TryGetValue(k_key, k_node) Then Throw New ArgumentException($"Unknown node key: '{k_key}'.", NameOf(k_key))
        Return k_node
    End Function

    Private Sub Touch()
        If _updateDepth = 0 Then Invalidate()
    End Sub

    ''' <summary>Who is joined to whom, by node index. Built on first use after a change.</summary>
    Private Function Neighbours() As List(Of Integer)()
        If _neighbours IsNot Nothing Then Return _neighbours
        Dim k_result(_nodes.Count - 1) As List(Of Integer)
        For k_i As Integer = 0 To _nodes.Count - 1
            k_result(k_i) = New List(Of Integer)()
        Next
        For Each k_edge As KBotGraphEdge In _edges
            k_result(k_edge.FromNode.Index).Add(k_edge.ToNode.Index)
            k_result(k_edge.ToNode.Index).Add(k_edge.FromNode.Index)
        Next
        _neighbours = k_result
        Return k_result
    End Function

    ''' <summary>The keys of the dots joined to this one (any visibility). Empty for an unknown key.</summary>
    Public Function NeighbourKeys(k_key As String) As IReadOnlyList(Of String)
        Dim k_node As KBotGraphNode = FindNode(k_key)
        Dim k_result As New List(Of String)()
        If k_node Is Nothing Then Return k_result
        For Each k_i As Integer In Neighbours()(k_node.Index)
            k_result.Add(_nodes(k_i).Key)
        Next
        Return k_result
    End Function

    ' =====================================================================
    ' SELECTION AND VIEW
    ' =====================================================================

    ''' <summary>The key of the selected dot, or empty. Setting it does NOT raise <see cref="NodeClicked"/> (C-chart rule: an assignment is the host stating a fact).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedKey As String
        Get
            Return If(_selectedIndex >= 0 AndAlso _selectedIndex < _nodes.Count, _nodes(_selectedIndex).Key, String.Empty)
        End Get
        Set(k_value As String)
            Dim k_node As KBotGraphNode = FindNode(k_value)
            _selectedIndex = If(k_node Is Nothing, -1, k_node.Index)
            Invalidate()
        End Set
    End Property

    ''' <summary>Zoom so that every dot (visible or not) is in view, centred. The picture does not jump while dots appear.</summary>
    Public Sub FitToView()
        _needsFit = False
        If _nodes.Count = 0 OrElse ClientSize.Width < 10 OrElse ClientSize.Height < 10 Then
            _needsFit = _nodes.Count > 0
            Invalidate()
            Return
        End If
        Dim k_minX As Double = Double.MaxValue
        Dim k_minY As Double = Double.MaxValue
        Dim k_maxX As Double = Double.MinValue
        Dim k_maxY As Double = Double.MinValue
        For Each k_node As KBotGraphNode In _nodes
            If k_node.X < k_minX Then k_minX = k_node.X
            If k_node.X > k_maxX Then k_maxX = k_node.X
            If k_node.Y < k_minY Then k_minY = k_node.Y
            If k_node.Y > k_maxY Then k_maxY = k_node.Y
        Next
        Dim k_margin As Double = ThemeShapes.ScaleDpi(Me, 24)
        Dim k_w As Double = Math.Max(1.0, k_maxX - k_minX)
        Dim k_h As Double = Math.Max(1.0, k_maxY - k_minY)
        _zoom = Math.Min((ClientSize.Width - 2 * k_margin) / k_w, (ClientSize.Height - 2 * k_margin) / k_h)
        _zoom = Math.Max(MinZoom, Math.Min(MaxZoom, _zoom))
        _panX = ClientSize.Width / 2.0 - (k_minX + k_maxX) / 2.0 * _zoom
        _panY = ClientSize.Height / 2.0 - (k_minY + k_maxY) / 2.0 * _zoom
        Invalidate()
    End Sub

    Private Const MinZoom As Double = 0.01
    Private Const MaxZoom As Double = 60.0

    ' =====================================================================
    ' APPEARANCE
    ' =====================================================================

    <Category("K-BOT Graph Appearance")>
    <Description("Default radius of a dot, in logical px. A node's own Radius wins.")>
    <DefaultValue(4)>
    Public Property NodeRadius As Integer
        Get
            Return _nodeRadius
        End Get
        Set(k_value As Integer)
            _nodeRadius = Math.Max(1, k_value)
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Graph Appearance")>
    <Description("Thickness of a line, in logical px.")>
    <DefaultValue(1)>
    Public Property EdgeWidth As Integer
        Get
            Return _edgeWidth
        End Get
        Set(k_value As Integer)
            _edgeWidth = Math.Max(1, k_value)
            RebuildThemeResources()
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Graph Appearance")>
    <Description("How solid the lines are, 5 to 100 percent.")>
    <DefaultValue(45)>
    Public Property EdgeOpacity As Integer
        Get
            Return _edgeOpacity
        End Get
        Set(k_value As Integer)
            _edgeOpacity = Math.Max(5, Math.Min(100, k_value))
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Graph Appearance")>
    <Description("True = the name is written beside every dot. False = only beside the hovered or selected dot and its neighbours.")>
    <DefaultValue(False)>
    Public Property LabelsVisible As Boolean
        Get
            Return _labelsVisible
        End Get
        Set(k_value As Boolean)
            _labelsVisible = k_value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Graph Appearance")>
    <Description("Text in the middle of the surface while no dot is visible.")>
    <DefaultValue("")>
    Public Property EmptyText As String
        Get
            Return _emptyText
        End Get
        Set(k_value As String)
            _emptyText = If(k_value, String.Empty)
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Graph Appearance")>
    <Description("Colour of the surface behind the dots. Empty = from the theme.")>
    Public Property PlotBackColor As Color
        Get
            Return _plotBackColor
        End Get
        Set(k_value As Color)
            _plotBackColor = k_value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializePlotBackColor() As Boolean
        Return _plotBackColor <> Color.Empty
    End Function

    Public Sub ResetPlotBackColor()
        PlotBackColor = Color.Empty
    End Sub

    <Category("K-BOT Graph Appearance")>
    <Description("Default colour of a line. Empty = from the theme. An edge's own EdgeColor wins.")>
    Public Property EdgeColor As Color
        Get
            Return _edgeColor
        End Get
        Set(k_value As Color)
            _edgeColor = k_value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeEdgeColor() As Boolean
        Return _edgeColor <> Color.Empty
    End Function

    Public Sub ResetEdgeColor()
        EdgeColor = Color.Empty
    End Sub

    <Category("K-BOT Graph Tooltip")>
    <Description("How far from a dot the pointer may be and still name it (logical px beyond the dot).")>
    <DefaultValue(6)>
    Public Property HoverRadius As Integer
        Get
            Return _hoverRadius
        End Get
        Set(k_value As Integer)
            _hoverRadius = Math.Max(0, k_value)
        End Set
    End Property

    <Category("K-BOT Graph Tooltip")>
    <Description("False = hovering a dot fades the others but opens no label.")>
    <DefaultValue(True)>
    Public Property PointTooltipEnabled As Boolean
        Get
            Return _pointTooltipEnabled
        End Get
        Set(k_value As Boolean)
            _pointTooltipEnabled = k_value
            If Not k_value Then HideTip()
        End Set
    End Property

    ''' <summary>The floating label of the dots. Dressable from the property grid (<c>PointTooltip.Style.…</c>).</summary>
    <Category("K-BOT Graph Tooltip")>
    <Description("The floating label shown on a hovered dot. Its look is set through PointTooltip.Style.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property PointTooltip As KBotToolTip
        Get
            ' Created lazily and never inside the designer: that would open a window inside Visual Studio.
            If _pointTooltip Is Nothing Then _pointTooltip = New KBotToolTip()
            Return _pointTooltip
        End Get
    End Property

    ''' <summary>The n-th colour of the automatic set, the same one the chart and the lanes use. Never red.</summary>
    Public Function AutoColor(k_index As Integer) As Color
        Return KBotAutoPalette.ColorAt(Palette().AccentColor, _isDarkScheme, k_index)
    End Function

    ' =====================================================================
    ' THEME
    ' =====================================================================

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            _scheme = k_scheme
            _isDarkScheme = k_scheme.IsDark
            MyBase.BackColor = k_scheme.Palette.SurfaceColor
            MyBase.ForeColor = k_scheme.Palette.TextColor
            RebuildThemeResources()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotGraphView.ApplyTheme", ex)
        End Try
    End Sub

    Private Function Palette() As ThemePalette
        Return If(_scheme, ThemeManager.Current).Palette
    End Function

    Private Function EffectivePlotBackColor() As Color
        Return If(_plotBackColor = Color.Empty, Palette().SurfaceAltColor, _plotBackColor)
    End Function

    Private Sub RebuildThemeResources()
        _edgePen?.Dispose()
        _borderPen?.Dispose()
        _ringPen?.Dispose()
        _edgePen = New Pen(Color.Gray, CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, _edgeWidth))))
        _borderPen = New Pen(Palette().BorderColor, 1.0F)
        _ringPen = New Pen(Palette().AccentColor, CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 2))))
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        RebuildThemeResources()
    End Sub

    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        RebuildThemeResources()
        Invalidate()
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        If _needsFit Then FitToView()
    End Sub

    Protected Overrides Sub Dispose(k_disposing As Boolean)
        If k_disposing Then
            _edgePen?.Dispose()
            _edgePen = Nothing
            _borderPen?.Dispose()
            _borderPen = Nothing
            _ringPen?.Dispose()
            _ringPen = Nothing
            _fillBrush?.Dispose()
            _fillBrush = Nothing
            _labelBrush?.Dispose()
            _labelBrush = Nothing
            _pointTooltip?.Dispose()
            _pointTooltip = Nothing
        End If
        MyBase.Dispose(k_disposing)
    End Sub

End Class
