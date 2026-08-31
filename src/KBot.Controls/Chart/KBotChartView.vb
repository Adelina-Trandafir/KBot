Option Strict On
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Drawing
Imports System.Drawing.Design
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' An owner-drawn time chart with a button band on top — the K-BOT control for "how did this
''' thing move".
'''
''' <para><b>What it draws.</b> One or more <see cref="KBotChartSeries"/>, each a line through its
''' points, all sharing one real time axis. A series marked <c>Emphasis</c> is drawn last and
''' thicker, which is how a total is told apart from the parts that add up to it.</para>
'''
''' <para><b>The band on top.</b> The header is a SINGLE-SELECT strip of buttons that stands in
''' for a tab control (<see cref="Tabs"/>, <see cref="SelectedTabKey"/>,
''' <see cref="TabSelected"/>). It is drawn, not composed out of child controls, for the same
''' reason the tree draws its own header: a band of real buttons would need its own theming, its
''' own DPI pass and its own designer serialization, and it would still not line up with the plot
''' underneath. The chart never decides what a tab MEANS — it raises the key and the host refills
''' the series.</para>
'''
''' <para><b>Hovering.</b> Resting the pointer near a marker opens a floating
''' <see cref="KBotToolTip"/> naming that point. Never <c>System.Windows.Forms.ToolTip</c>: the
''' markers are painted regions, not controls, so there is nothing for it to extend (the same
''' reason the tree's header buttons use <c>ShowAt</c>).</para>
'''
''' <para><b>Colours.</b> None are written here. Every colour property defaults to
''' <c>Color.Empty</c> = "from the theme", and a series without a colour is handed one derived
''' from the active palette, so an untouched chart follows the scheme. Anything set explicitly
''' wins and keeps winning.</para>
'''
''' <para><b>Pixel metrics are LOGICAL pixels (96 dpi)</b> and are scaled at paint time through
''' <c>ThemeShapes.ScaleDpi</c>. The public properties stay logical — serialising the scaled value
''' would make the next load scale it again.</para>
''' </summary>
<ToolboxItem(True)>
<DefaultProperty("Series")>
<DefaultEvent("TabSelected")>
Partial Public NotInheritable Class KBotChartView
    Inherits Control
    Implements IThemedControl
    Implements ISupportInitialize

    Private ReadOnly _series As New KBotChartSeriesCollection()
    Private ReadOnly _tabs As New KBotChartTabCollection()

    ' ── Header band ──────────────────────────────────────────────────────────
    Private _headerVisible As Boolean = True
    Private _headerHeight As Integer = 28
    Private _headerCaption As String = String.Empty
    Private _headerFont As Font
    Private _headerBackColor As Color = Color.Empty
    Private _headerTextColor As Color = Color.Empty
    Private _headerSeparatorColor As Color = Color.Empty
    Private _headerSeparatorWidth As Integer = 1
    Private _headerGradient As Integer = 0

    ' ── Tab buttons inside the band ──────────────────────────────────────────
    Private _tabAlign As KBotChartTabAlign = KBotChartTabAlign.Right
    Private _tabHeight As Integer = 20
    Private _tabPadding As Integer = 12
    Private _tabSpacing As Integer = 4
    Private _tabCornerRadius As Integer = -1
    Private _tabGradient As Integer = 14
    Private _tabIconSize As Size = New Size(14, 14)
    Private _selectedTabKey As String = String.Empty

    ' ── Plot ─────────────────────────────────────────────────────────────────
    Private _plotMargin As Integer = 10
    Private _lineWidth As Integer = 2
    Private _emphasisLineWidth As Integer = 3
    Private _markerSize As Integer = 6
    Private _markerStyle As KBotChartMarkerStyle = KBotChartMarkerStyle.Circle
    Private _areaFillOpacity As Integer = 18
    Private _plotBackColor As Color = Color.Empty
    Private _borderVisible As Boolean = True
    Private _borderColor As Color = Color.Empty
    Private _cornerRadius As Integer = -1

    ' ── Axes ─────────────────────────────────────────────────────────────────
    Private _axisVisible As Boolean = True
    Private _axisColor As Color = Color.Empty
    Private _axisTextColor As Color = Color.Empty
    Private _gridColor As Color = Color.Empty
    Private _horizontalGridLines As Boolean = True
    Private _verticalGridLines As Boolean = False
    Private _valueTickCount As Integer = 4
    Private _axisFont As Font
    Private _valueFormat As String = "N0"
    Private _momentFormat As String = "dd.MM.yy"
    Private _axisLabelGap As Integer = 6
    Private _valueAxisMode As KBotChartValueAxisMode = KBotChartValueAxisMode.FromZero

    ' ── Legend ───────────────────────────────────────────────────────────────
    Private _legendVisible As Boolean = True
    Private _legendHeight As Integer = 18
    Private _legendSpacing As Integer = 14
    Private _legendTextColor As Color = Color.Empty

    ' ── Empty state ──────────────────────────────────────────────────────────
    Private _emptyText As String = String.Empty
    Private _emptyTextColor As Color = Color.Empty

    ' ── Hovering and the floating label ──────────────────────────────────────
    Private _pointTooltip As KBotToolTip
    Private _pointTooltipEnabled As Boolean = True
    Private _hoverRadius As Integer = 14

    ' ── Runtime state (never serialized) ─────────────────────────────────────
    Private _scheme As ThemeScheme

    ''' <summary>
    ''' THE DARK-SCHEME EXCEPTION, for the header band. Same rule, and same reason, as
    ''' <c>AdvancedTreeControl.BandColorsFromThemeOnly</c>.
    '''
    ''' <para>The general rule of this control is "the designer wins": a colour that is not
    ''' <see cref="Color.Empty"/> beats the palette. On a dark scheme that rule produces exactly
    ''' what nobody wants — a light grey band chosen for the light scheme stays light grey above a
    ''' dark chart, with the scheme's now-light caption text written on it, which is unreadable.
    ''' While the scheme is dark the band's designer colours are therefore IGNORED, not erased:
    ''' they come back whole on the way to a light scheme, and the designer keeps serializing
    ''' them.</para>
    '''
    ''' <para>What SURVIVES is the band's shape rather than its colour — <see cref="HeaderGradient"/>
    ''' still applies, over the colour the theme chose. The buttons never needed the exception:
    ''' they take their fill and their text from the palette and have no designer colour to
    ''' ignore.</para>
    ''' </summary>
    Private _isDarkScheme As Boolean
    Private _layoutValid As Boolean
    Private _updateDepth As Integer
    Private _initializing As Boolean
    Private _plotRect As Rectangle = Rectangle.Empty
    Private _headerRect As Rectangle = Rectangle.Empty
    Private _legendRect As Rectangle = Rectangle.Empty
    Private _minMoment As Date = Date.MinValue
    Private _maxMoment As Date = Date.MinValue
    Private _minValue As Double
    Private _maxValue As Double
    Private _hoverSeriesIndex As Integer = -1
    Private _hoverPointIndex As Integer = -1
    Private _hoverTabIndex As Integer = -1
    Private _focusTabIndex As Integer = -1

    ' The key of whatever is "in the label" right now. Without it every pixel of movement over the
    ' same marker would reschedule the label and it would never actually appear.
    Private _currentTipKey As String
    Private ReadOnly _tipContent As New KBotToolTipContent()

    ' Pens and brushes that do NOT depend on the series are built once, in ApplyTheme, and freed in
    ' Dispose — as in KBotChipBar. Series colours change from line to line, so nothing about them
    ' can be remembered here.
    Private _axisPen As Pen
    Private _gridPen As Pen
    Private _borderPen As Pen

    ' Fonts DERIVED from Font (bold header, smaller axis). Cached because deriving a font on every
    ' paint allocates a GDI handle per repaint; rebuilt whenever Font or the override changes.
    Private _derivedHeaderFont As Font
    Private _derivedAxisFont As Font

    ' "The operator pinned this" flags — see ShouldSerializeBackColor.
    Private _backColorPinned As Boolean
    Private _foreColorPinned As Boolean
    Private _fontPinned As Boolean

    ''' <summary>Raised when the operator moves the band to a different button.</summary>
    Public Event TabSelected(tabKey As String)

    ''' <summary>Raised on a left click on a marker. Index is the position inside that series.</summary>
    Public Event PointClicked(seriesKey As String, pointIndex As Integer)

    ''' <summary>
    ''' Raised when the marker under the pointer changes. <paramref name="seriesKey"/> is
    ''' <c>Nothing</c> and <paramref name="pointIndex"/> is -1 when the pointer leaves every marker.
    ''' </summary>
    Public Event PointHovered(seriesKey As String, pointIndex As Integer)

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable, True)
        TabStop = True
        _series.Owner = Me
        _tabs.Owner = Me
    End Sub

    ' =====================================================================
    ' INHERITED PROPERTIES WITH A PIN FLAG
    ' =====================================================================

    ''' <summary>The surface behind the whole control; not pinned here, it follows the theme.</summary>
    <Category("K-BOT Chart Appearance")>
    <Description("Background of the whole control. Not pinned here, it follows the active theme.")>
    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(value As Color)
            _backColorPinned = True
            MyBase.BackColor = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' CRITICAL. <c>Control.ShouldSerializeBackColor</c> answers True as soon as the property has
    ''' ever been WRITTEN — including by <see cref="ApplyTheme"/>. Without this pair Visual Studio
    ''' would write a <c>chart.BackColor = …</c> nobody chose into the host form, and on reload
    ''' that line would run through the setter above and pin the colour forever. The flag is the
    ''' truth, not <c>Control</c>'s property bag.
    ''' </summary>
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColorPinned
    End Function

    ' The flag is cleared AFTER the colour is written: ResetBackColor goes through the VIRTUAL
    ' setter, that is, through ours, which would light it again.
    Public Overrides Sub ResetBackColor()
        MyBase.ResetBackColor()
        _backColorPinned = False
        Invalidate()
    End Sub

    <Category("K-BOT Chart Appearance")>
    <Description("Default text colour of the chart. Not pinned here, it follows the active theme.")>
    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(value As Color)
            _foreColorPinned = True
            MyBase.ForeColor = value
            Invalidate()
        End Set
    End Property

    ''' <summary>The twin of <see cref="ShouldSerializeBackColor"/>, for the same reason.</summary>
    Public Function ShouldSerializeForeColor() As Boolean
        Return _foreColorPinned
    End Function

    Public Overrides Sub ResetForeColor()
        MyBase.ResetForeColor()
        _foreColorPinned = False
        Invalidate()
    End Sub

    ''' <summary>
    ''' The font of the chart. Not pinned here, so it inherits ambiently and answers to the
    ''' scheme's base font. <c>Font</c> cannot carry a <c>DefaultValue</c> attribute (that needs a
    ''' constant), so the ShouldSerialize / Reset pair is the only thing keeping it out of the
    ''' host's .Designer.vb.
    ''' </summary>
    <Category("K-BOT Chart Appearance")>
    <Description("Font of the chart. Not pinned here, it follows the ambient font of the active scheme.")>
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

    ''' <summary>The flag is cleared AFTER the base reset — <c>Control.ResetFont</c> writes through the virtual setter.</summary>
    Public Overrides Sub ResetFont()
        MyBase.ResetFont()
        _fontPinned = False
        RebuildDerivedFonts()
        InvalidateChartLayout()
    End Sub

    ''' <summary>
    ''' <c>Size</c> cannot carry a <c>DefaultValue</c> either, so a freshly dropped chart would
    ''' otherwise write a size line into every host form. The default below is the one the designer
    ''' gets, and it is deliberately a shape, not a square: a time chart that is taller than it is
    ''' wide reads as a bar chart nobody asked for.
    ''' </summary>
    Protected Overrides ReadOnly Property DefaultSize As Size
        Get
            Return New Size(420, 220)
        End Get
    End Property

    ' =====================================================================
    ' DATA
    ' =====================================================================

    ''' <summary>
    ''' The series, in the order they are drawn. Editable from the property grid (the standard
    ''' collection dialog) or from code through <see cref="AddSeries"/> — the same collection.
    ''' </summary>
    <Category("K-BOT Chart Data")>
    <Description("The lines of the chart, in drawing order. A series marked Emphasis is drawn last, on top.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Series As KBotChartSeriesCollection
        Get
            Return _series
        End Get
    End Property

    ''' <summary>Text drawn in the middle of the plot when there is nothing to draw.</summary>
    <Category("K-BOT Chart Data")>
    <Description("Text drawn in the middle of the plot when no visible series has any point. This one IS read by the operator, so write it in the operator's language.")>
    <DefaultValue("")>
    Public Property EmptyText As String
        Get
            Return _emptyText
        End Get
        Set(value As String)
            _emptyText = If(value, String.Empty)
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Data")>
    <Description("Colour of the EmptyText line. Empty = the dimmed text colour of the theme.")>
    Public Property EmptyTextColor As Color
        Get
            Return _emptyTextColor
        End Get
        Set(value As Color)
            _emptyTextColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeEmptyTextColor() As Boolean
        Return _emptyTextColor <> Color.Empty
    End Function

    Public Sub ResetEmptyTextColor()
        EmptyTextColor = Color.Empty
    End Sub

    ' =====================================================================
    ' HEADER BAND
    ' =====================================================================

    <Category("K-BOT Chart Header")>
    <Description("False => no band at all; the plot takes the whole control and the tabs are unreachable.")>
    <DefaultValue(True)>
    Public Property HeaderVisible As Boolean
        Get
            Return _headerVisible
        End Get
        Set(value As Boolean)
            If _headerVisible = value Then Return
            _headerVisible = value
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Header")>
    <Description("Height of the band (logical px at 96 dpi, scaled at paint time). Default 28.")>
    <DefaultValue(28)>
    Public Property HeaderHeight As Integer
        Get
            Return _headerHeight
        End Get
        Set(value As Integer)
            ' A negative measure is clamped, it does not throw: a setter that throws would break
            ' InitializeComponent on a bad designer value, and the form would not open at all.
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _headerHeight Then Return
            _headerHeight = clamped
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Header")>
    <Description("Title written at the free end of the band. This is read by the operator. Empty = no title.")>
    <DefaultValue("")>
    Public Property HeaderCaption As String
        Get
            Return _headerCaption
        End Get
        Set(value As String)
            _headerCaption = If(value, String.Empty)
            Invalidate()
        End Set
    End Property

    ''' <summary>Font of the band; <c>Nothing</c> = the control font, in bold.</summary>
    <Category("K-BOT Chart Header")>
    <Description("Font of the band title and of the tab captions. Nothing = the control font in bold.")>
    Public Property HeaderFont As Font
        Get
            Return _headerFont
        End Get
        Set(value As Font)
            _headerFont = value
            RebuildDerivedFonts()
            InvalidateChartLayout()
        End Set
    End Property

    Public Function ShouldSerializeHeaderFont() As Boolean
        Return _headerFont IsNot Nothing
    End Function

    Public Sub ResetHeaderFont()
        HeaderFont = Nothing
    End Sub

    <Category("K-BOT Chart Header")>
    <Description("Background of the band. Empty = the alternate surface colour of the theme.")>
    Public Property HeaderBackColor As Color
        Get
            Return _headerBackColor
        End Get
        Set(value As Color)
            _headerBackColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeHeaderBackColor() As Boolean
        Return _headerBackColor <> Color.Empty
    End Function

    Public Sub ResetHeaderBackColor()
        HeaderBackColor = Color.Empty
    End Sub

    <Category("K-BOT Chart Header")>
    <Description("Colour of the band title. Empty = the text colour of the theme.")>
    Public Property HeaderTextColor As Color
        Get
            Return _headerTextColor
        End Get
        Set(value As Color)
            _headerTextColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeHeaderTextColor() As Boolean
        Return _headerTextColor <> Color.Empty
    End Function

    Public Sub ResetHeaderTextColor()
        HeaderTextColor = Color.Empty
    End Sub

    <Category("K-BOT Chart Header")>
    <Description("Colour of the line under the band. Empty = the border colour of the theme.")>
    Public Property HeaderSeparatorColor As Color
        Get
            Return _headerSeparatorColor
        End Get
        Set(value As Color)
            _headerSeparatorColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeHeaderSeparatorColor() As Boolean
        Return _headerSeparatorColor <> Color.Empty
    End Function

    Public Sub ResetHeaderSeparatorColor()
        HeaderSeparatorColor = Color.Empty
    End Sub

    <Category("K-BOT Chart Header")>
    <Description("Thickness of the line under the band (logical px). 0 = no line.")>
    <DefaultValue(1)>
    Public Property HeaderSeparatorWidth As Integer
        Get
            Return _headerSeparatorWidth
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _headerSeparatorWidth Then Return
            _headerSeparatorWidth = clamped
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Header")>
    <Description("Strength of the gradient on the band background (0..100; 0 = flat fill). Goes through ThemeShapes.FillModern, so it introduces no new colour.")>
    <DefaultValue(0)>
    Public Property HeaderGradient As Integer
        Get
            Return _headerGradient
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, Math.Min(100, value))
            If clamped = _headerGradient Then Return
            _headerGradient = clamped
            Invalidate()
        End Set
    End Property

    ' =====================================================================
    ' TABS
    ' =====================================================================

    ''' <summary>
    ''' The buttons of the band, in display order. Single-select: exactly one is current, because
    ''' the plot below can only show one thing at a time.
    ''' </summary>
    <Category("K-BOT Chart Tabs")>
    <Description("The buttons of the band, in display order. The band is single-select.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Tabs As KBotChartTabCollection
        Get
            Return _tabs
        End Get
    End Property

    ''' <summary>
    ''' The key of the current button. Setting it from here does NOT raise
    ''' <see cref="TabSelected"/> — an assignment is the host stating a fact, not the operator
    ''' making a choice, and a host that raised its own event would loop through its own handler.
    ''' </summary>
    <Category("K-BOT Chart Tabs")>
    <Description("Key of the current button. Setting it does not raise TabSelected: an assignment is the host stating a fact, not the operator choosing.")>
    <DefaultValue("")>
    Public Property SelectedTabKey As String
        Get
            Return _selectedTabKey
        End Get
        Set(value As String)
            Dim key As String = If(value, String.Empty)
            If String.Equals(key, _selectedTabKey, StringComparison.Ordinal) Then Return
            _selectedTabKey = key
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Tabs")>
    <Description("Which end of the band the buttons sit on. The title takes the other end.")>
    <DefaultValue(KBotChartTabAlign.Right)>
    Public Property TabAlign As KBotChartTabAlign
        Get
            Return _tabAlign
        End Get
        Set(value As KBotChartTabAlign)
            If _tabAlign = value Then Return
            _tabAlign = value
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Tabs")>
    <Description("Height of a button (logical px). Clamped to the band height at layout time.")>
    <DefaultValue(20)>
    Public Property TabHeight As Integer
        Get
            Return _tabHeight
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(1, value)
            If clamped = _tabHeight Then Return
            _tabHeight = clamped
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Tabs")>
    <Description("Air between the edge of a button and its caption (logical px). Default 12.")>
    <DefaultValue(12)>
    Public Property TabPadding As Integer
        Get
            Return _tabPadding
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _tabPadding Then Return
            _tabPadding = clamped
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Tabs")>
    <Description("Distance between two buttons (logical px). Default 4.")>
    <DefaultValue(4)>
    Public Property TabSpacing As Integer
        Get
            Return _tabSpacing
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _tabSpacing Then Return
            _tabSpacing = clamped
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Tabs")>
    <Description("Corner radius of a button (logical px). -1 = the radius of the active scheme; 0 = square corners.")>
    <DefaultValue(-1)>
    Public Property TabCornerRadius As Integer
        Get
            Return _tabCornerRadius
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(-1, value)
            If clamped = _tabCornerRadius Then Return
            _tabCornerRadius = clamped
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Tabs")>
    <Description("Strength of the gradient on the buttons (0..100; 0 = flat fill). Default 14.")>
    <DefaultValue(14)>
    Public Property TabGradient As Integer
        Get
            Return _tabGradient
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, Math.Min(100, value))
            If clamped = _tabGradient Then Return
            _tabGradient = clamped
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Tabs")>
    <Description("Drawn size of a button icon (logical px).")>
    Public Property TabIconSize As Size
        Get
            Return _tabIconSize
        End Get
        Set(value As Size)
            Dim clamped As New Size(Math.Max(0, value.Width), Math.Max(0, value.Height))
            If clamped = _tabIconSize Then Return
            _tabIconSize = clamped
            InvalidateChartLayout()
        End Set
    End Property

    ''' <summary><c>Size</c> cannot carry a <c>DefaultValue</c>, so it needs the pair.</summary>
    Public Function ShouldSerializeTabIconSize() As Boolean
        Return _tabIconSize <> New Size(14, 14)
    End Function

    Public Sub ResetTabIconSize()
        TabIconSize = New Size(14, 14)
    End Sub

    ' =====================================================================
    ' PLOT
    ' =====================================================================

    <Category("K-BOT Chart Plot")>
    <Description("Air between the plot and the edges of the control, outside the axis labels (logical px).")>
    <DefaultValue(10)>
    Public Property PlotMargin As Integer
        Get
            Return _plotMargin
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _plotMargin Then Return
            _plotMargin = clamped
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Plot")>
    <Description("Thickness of an ordinary line (logical px). Default 2.")>
    <DefaultValue(2)>
    Public Property LineWidth As Integer
        Get
            Return _lineWidth
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(1, value)
            If clamped = _lineWidth Then Return
            _lineWidth = clamped
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Plot")>
    <Description("Thickness of a series marked Emphasis (logical px). Default 3.")>
    <DefaultValue(3)>
    Public Property EmphasisLineWidth As Integer
        Get
            Return _emphasisLineWidth
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(1, value)
            If clamped = _emphasisLineWidth Then Return
            _emphasisLineWidth = clamped
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Plot")>
    <Description("Diameter of a marker (logical px). Markers are also the hit target of the floating label.")>
    <DefaultValue(6)>
    Public Property MarkerSize As Integer
        Get
            Return _markerSize
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _markerSize Then Return
            _markerSize = clamped
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Plot")>
    <Description("Shape drawn on every point. None also removes the hit target, so no point can be named on hover.")>
    <DefaultValue(KBotChartMarkerStyle.Circle)>
    Public Property MarkerStyle As KBotChartMarkerStyle
        Get
            Return _markerStyle
        End Get
        Set(value As KBotChartMarkerStyle)
            If _markerStyle = value Then Return
            _markerStyle = value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Plot")>
    <Description("Opacity of the tint under a series whose FillArea is True (0..100). 0 = no fill at all.")>
    <DefaultValue(18)>
    Public Property AreaFillOpacity As Integer
        Get
            Return _areaFillOpacity
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, Math.Min(100, value))
            If clamped = _areaFillOpacity Then Return
            _areaFillOpacity = clamped
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Plot")>
    <Description("Background of the plot rectangle. Empty = the alternate surface colour of the theme.")>
    Public Property PlotBackColor As Color
        Get
            Return _plotBackColor
        End Get
        Set(value As Color)
            _plotBackColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializePlotBackColor() As Boolean
        Return _plotBackColor <> Color.Empty
    End Function

    Public Sub ResetPlotBackColor()
        PlotBackColor = Color.Empty
    End Sub

    <Category("K-BOT Chart Plot")>
    <Description("False => no outline around the control.")>
    <DefaultValue(True)>
    Public Property BorderVisible As Boolean
        Get
            Return _borderVisible
        End Get
        Set(value As Boolean)
            If _borderVisible = value Then Return
            _borderVisible = value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Plot")>
    <Description("Colour of the outline. Empty = the border colour of the theme.")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeBorderColor() As Boolean
        Return _borderColor <> Color.Empty
    End Function

    Public Sub ResetBorderColor()
        BorderColor = Color.Empty
    End Sub

    <Category("K-BOT Chart Plot")>
    <Description("Corner radius of the control (logical px). -1 = the radius of the active scheme; 0 = square corners.")>
    <DefaultValue(-1)>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(-1, value)
            If clamped = _cornerRadius Then Return
            _cornerRadius = clamped
            Invalidate()
        End Set
    End Property

    ' =====================================================================
    ' AXES
    ' =====================================================================

    <Category("K-BOT Chart Axes")>
    <Description("False => neither axis lines nor axis labels; the plot is drawn edge to edge.")>
    <DefaultValue(True)>
    Public Property AxisVisible As Boolean
        Get
            Return _axisVisible
        End Get
        Set(value As Boolean)
            If _axisVisible = value Then Return
            _axisVisible = value
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Axes")>
    <Description("Colour of the two axis lines. Empty = the border colour of the theme.")>
    Public Property AxisColor As Color
        Get
            Return _axisColor
        End Get
        Set(value As Color)
            _axisColor = value
            RebuildThemeResources()
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeAxisColor() As Boolean
        Return _axisColor <> Color.Empty
    End Function

    Public Sub ResetAxisColor()
        AxisColor = Color.Empty
    End Sub

    <Category("K-BOT Chart Axes")>
    <Description("Colour of the axis labels. Empty = the dimmed text colour of the theme.")>
    Public Property AxisTextColor As Color
        Get
            Return _axisTextColor
        End Get
        Set(value As Color)
            _axisTextColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeAxisTextColor() As Boolean
        Return _axisTextColor <> Color.Empty
    End Function

    Public Sub ResetAxisTextColor()
        AxisTextColor = Color.Empty
    End Sub

    <Category("K-BOT Chart Axes")>
    <Description("Colour of the grid lines. Empty = the border colour of the theme, blended into the plot background.")>
    Public Property GridColor As Color
        Get
            Return _gridColor
        End Get
        Set(value As Color)
            _gridColor = value
            RebuildThemeResources()
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeGridColor() As Boolean
        Return _gridColor <> Color.Empty
    End Function

    Public Sub ResetGridColor()
        GridColor = Color.Empty
    End Sub

    <Category("K-BOT Chart Axes")>
    <Description("Draw the horizontal grid lines, one per value tick.")>
    <DefaultValue(True)>
    Public Property HorizontalGridLines As Boolean
        Get
            Return _horizontalGridLines
        End Get
        Set(value As Boolean)
            If _horizontalGridLines = value Then Return
            _horizontalGridLines = value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Axes")>
    <Description("Draw a vertical grid line at the first and last moment. Off by default: on a real time axis the verticals are rarely regular, so they read as noise.")>
    <DefaultValue(False)>
    Public Property VerticalGridLines As Boolean
        Get
            Return _verticalGridLines
        End Get
        Set(value As Boolean)
            If _verticalGridLines = value Then Return
            _verticalGridLines = value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Axes")>
    <Description("How many intervals the value axis is cut into. 0 = no ticks and no horizontal grid.")>
    <DefaultValue(4)>
    Public Property ValueTickCount As Integer
        Get
            Return _valueTickCount
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, Math.Min(20, value))
            If clamped = _valueTickCount Then Return
            _valueTickCount = clamped
            InvalidateChartLayout()
        End Set
    End Property

    ''' <summary>Font of the axis labels; <c>Nothing</c> = the control font, one point smaller.</summary>
    <Category("K-BOT Chart Axes")>
    <Description("Font of the axis labels. Nothing = the control font, one point smaller.")>
    Public Property AxisFont As Font
        Get
            Return _axisFont
        End Get
        Set(value As Font)
            _axisFont = value
            RebuildDerivedFonts()
            InvalidateChartLayout()
        End Set
    End Property

    Public Function ShouldSerializeAxisFont() As Boolean
        Return _axisFont IsNot Nothing
    End Function

    Public Sub ResetAxisFont()
        AxisFont = Nothing
    End Sub

    <Category("K-BOT Chart Axes")>
    <Description("Numeric format of the value labels and of the value inside the floating label (standard .NET format string).")>
    <DefaultValue("N0")>
    Public Property ValueFormat As String
        Get
            Return _valueFormat
        End Get
        Set(value As String)
            _valueFormat = If(String.IsNullOrWhiteSpace(value), "N0", value)
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Axes")>
    <Description("Date format of the time labels (standard .NET format string).")>
    <DefaultValue("dd.MM.yy")>
    Public Property MomentFormat As String
        Get
            Return _momentFormat
        End Get
        Set(value As String)
            _momentFormat = If(String.IsNullOrWhiteSpace(value), "dd.MM.yy", value)
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Axes")>
    <Description("Air between an axis label and the plot (logical px).")>
    <DefaultValue(6)>
    Public Property AxisLabelGap As Integer
        Get
            Return _axisLabelGap
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _axisLabelGap Then Return
            _axisLabelGap = clamped
            InvalidateChartLayout()
        End Set
    End Property

    ''' <summary>
    ''' Where the value axis starts. <c>FromZero</c> tells the truth about magnitude,
    ''' <c>FromMinimum</c> tells the truth about movement; neither is right in general.
    ''' </summary>
    <Category("K-BOT Chart Axes")>
    <Description("FromZero = the baseline is 0, so magnitudes compare honestly. FromMinimum = the baseline is the smallest value, so small movements stay visible.")>
    <DefaultValue(KBotChartValueAxisMode.FromZero)>
    Public Property ValueAxisMode As KBotChartValueAxisMode
        Get
            Return _valueAxisMode
        End Get
        Set(value As KBotChartValueAxisMode)
            If _valueAxisMode = value Then Return
            _valueAxisMode = value
            InvalidateChartLayout()
        End Set
    End Property

    ' =====================================================================
    ' LEGEND
    ' =====================================================================

    <Category("K-BOT Chart Legend")>
    <Description("Show the strip that names the visible series, under the plot.")>
    <DefaultValue(True)>
    Public Property LegendVisible As Boolean
        Get
            Return _legendVisible
        End Get
        Set(value As Boolean)
            If _legendVisible = value Then Return
            _legendVisible = value
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Legend")>
    <Description("Height of the legend strip (logical px).")>
    <DefaultValue(18)>
    Public Property LegendHeight As Integer
        Get
            Return _legendHeight
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _legendHeight Then Return
            _legendHeight = clamped
            InvalidateChartLayout()
        End Set
    End Property

    <Category("K-BOT Chart Legend")>
    <Description("Distance between two legend entries (logical px).")>
    <DefaultValue(14)>
    Public Property LegendSpacing As Integer
        Get
            Return _legendSpacing
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _legendSpacing Then Return
            _legendSpacing = clamped
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Chart Legend")>
    <Description("Colour of the legend text. Empty = the dimmed text colour of the theme.")>
    Public Property LegendTextColor As Color
        Get
            Return _legendTextColor
        End Get
        Set(value As Color)
            _legendTextColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeLegendTextColor() As Boolean
        Return _legendTextColor <> Color.Empty
    End Function

    Public Sub ResetLegendTextColor()
        LegendTextColor = Color.Empty
    End Sub

    ' =====================================================================
    ' FLOATING LABEL
    ' =====================================================================

    ''' <summary>
    ''' The floating label the chart uses for its points and its buttons. Dressable from the
    ''' property grid (<c>PointTooltip.Style.…</c>) — colours, header, footer, separator.
    ''' </summary>
    <Category("K-BOT Chart Tooltip")>
    <Description("The floating label shown on a hovered point or button. Its look is set through PointTooltip.Style.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property PointTooltip As KBotToolTip
        Get
            ' Created lazily, at first need, and NEVER inside the designer: that would mean a
            ' window opened inside Visual Studio.
            If _pointTooltip Is Nothing Then _pointTooltip = New KBotToolTip()
            Return _pointTooltip
        End Get
    End Property

    <Category("K-BOT Chart Tooltip")>
    <Description("False => hovering a point highlights it but opens no label.")>
    <DefaultValue(True)>
    Public Property PointTooltipEnabled As Boolean
        Get
            Return _pointTooltipEnabled
        End Get
        Set(value As Boolean)
            _pointTooltipEnabled = value
            If Not value Then HideChartTip()
        End Set
    End Property

    <Category("K-BOT Chart Tooltip")>
    <Description("How far from a marker the pointer may be and still name it (logical px). Default 14.")>
    <DefaultValue(14)>
    Public Property HoverRadius As Integer
        Get
            Return _hoverRadius
        End Get
        Set(value As Integer)
            _hoverRadius = Math.Max(1, value)
        End Set
    End Property

    ' =====================================================================
    ' PUBLIC API
    ' =====================================================================

    ''' <summary>
    ''' Suspends layout and painting while the host refills the chart. Nested calls are counted, so
    ''' a helper that brackets its own work does not undo its caller's bracket.
    ''' </summary>
    Public Sub BeginUpdate()
        _updateDepth += 1
    End Sub

    ''' <summary>Ends the block opened by <see cref="BeginUpdate"/> and repaints once.</summary>
    Public Sub EndUpdate()
        If _updateDepth > 0 Then _updateDepth -= 1
        If _updateDepth = 0 Then InvalidateChartLayout()
    End Sub

    ''' <summary>Removes every series. Tabs and settings are untouched.</summary>
    Public Sub ClearSeries()
        _series.Clear()
    End Sub

    ''' <summary>Appends a series and returns it. The key must be non-empty and unique.</summary>
    Public Function AddSeries(key As String, text As String) As KBotChartSeries
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Empty key.", NameOf(key))
        If FindSeriesIndex(key) >= 0 Then Throw New ArgumentException($"Duplicate key: '{key}'.", NameOf(key))
        Dim s As New KBotChartSeries(key, text)
        _series.Add(s)
        Return s
    End Function

    ''' <summary>The series with that key, or Nothing. The asking form — it does not throw.</summary>
    Public Function FindSeries(key As String) As KBotChartSeries
        Dim idx As Integer = FindSeriesIndex(key)
        If idx < 0 Then Return Nothing
        Return _series(idx)
    End Function

    ''' <summary>Shows or hides one series. Unknown key => <c>ArgumentException</c>.</summary>
    Public Sub SetSeriesVisible(key As String, visible As Boolean)
        _series(RequireSeriesIndex(key)).Visible = visible
        InvalidateChartLayout()
    End Sub

    ''' <summary>Appends a button to the band. The key must be non-empty and unique.</summary>
    Public Function AddTab(key As String, text As String) As KBotChartTab
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Empty key.", NameOf(key))
        If FindTabIndex(key) >= 0 Then Throw New ArgumentException($"Duplicate key: '{key}'.", NameOf(key))
        Dim t As New KBotChartTab(key, text)
        _tabs.Add(t)
        Return t
    End Function

    ''' <summary>
    ''' Moves the band to that button AS IF the operator had pressed it —
    ''' <see cref="TabSelected"/> IS raised. Unknown key => <c>ArgumentException</c>; selecting the
    ''' button that is already current does nothing.
    ''' </summary>
    Public Sub SelectTab(key As String)
        Dim idx As Integer = RequireTabIndex(key)
        If String.Equals(_tabs(idx).Key, _selectedTabKey, StringComparison.Ordinal) Then Return
        _selectedTabKey = _tabs(idx).Key
        Invalidate()
        RaiseEvent TabSelected(_selectedTabKey)
    End Sub

    ''' <summary>Enables or disables one button. Unknown key => <c>ArgumentException</c>.</summary>
    Public Sub SetTabEnabled(key As String, enabled As Boolean)
        _tabs(RequireTabIndex(key)).Enabled = enabled
        Invalidate()
    End Sub

    ''' <summary>Shows or hides one button (hidden = no slot, not painted, skipped by the keyboard).</summary>
    Public Sub SetTabVisible(key As String, visible As Boolean)
        _tabs(RequireTabIndex(key)).Visible = visible
        InvalidateChartLayout()
    End Sub

    ''' <summary>The asking form for a button key — it does not throw.</summary>
    Public Function ContainsTab(key As String) As Boolean
        Return FindTabIndex(key) >= 0
    End Function

    ' =====================================================================
    ' ISupportInitialize
    ' =====================================================================

    ''' <summary>Start of the designer's initialization block (validation is suspended).</summary>
    Public Sub BeginInit() Implements ISupportInitialize.BeginInit
        _initializing = True
    End Sub

    ''' <summary>
    ''' End of the initialization block: the keys are validated (non-empty and unique) and the
    ''' chart is laid out again.
    '''
    ''' In the DESIGNER validation is skipped — a half-typed key would throw out of
    ''' <c>InitializeComponent</c>, which means the form would not open at all. The defect is
    ''' reported visually instead, with a red frame (see the painting file), exactly as
    ''' <c>KBotChipBar</c> does it.
    ''' </summary>
    Public Sub EndInit() Implements ISupportInitialize.EndInit
        Try
            _initializing = False
            If Not KBotDesignTime.IsDesignTime(Me) Then ValidateKeys()
            InvalidateChartLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChartView.EndInit", ex)
            Throw
        End Try
    End Sub

    Private Sub ValidateKeys()
        Dim seenTabs As New HashSet(Of String)(StringComparer.Ordinal)
        For i As Integer = 0 To _tabs.Count - 1
            Dim t As KBotChartTab = _tabs(i)
            If String.IsNullOrWhiteSpace(t.Key) Then
                Throw New ArgumentException($"Empty key on tab {i} ('{If(t.Text, String.Empty)}').", NameOf(Tabs))
            End If
            If Not seenTabs.Add(t.Key) Then
                Throw New ArgumentException($"Duplicate key: '{t.Key}' (tab {i}).", NameOf(Tabs))
            End If
        Next

        Dim seenSeries As New HashSet(Of String)(StringComparer.Ordinal)
        For i As Integer = 0 To _series.Count - 1
            Dim s As KBotChartSeries = _series(i)
            If String.IsNullOrWhiteSpace(s.Key) Then
                Throw New ArgumentException($"Empty key on series {i} ('{If(s.Text, String.Empty)}').", NameOf(Series))
            End If
            If Not seenSeries.Add(s.Key) Then
                Throw New ArgumentException($"Duplicate key: '{s.Key}' (series {i}).", NameOf(Series))
            End If
        Next
    End Sub

    ' =====================================================================
    ' THEME
    ' =====================================================================

    ''' <summary>Reapplies the colours of the scheme and rebuilds the cached pens.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            _scheme = scheme
            ' Set BEFORE anything repaints, so the band already draws under the new rule.
            _isDarkScheme = scheme.IsDark
            ' Written through MyBase on purpose: the public setters light the pin flags, and a
            ' colour the theme chose is not a colour the operator chose.
            MyBase.BackColor = scheme.Palette.SurfaceColor
            MyBase.ForeColor = scheme.Palette.TextColor
            RebuildThemeResources()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChartView.ApplyTheme", ex)
        End Try
    End Sub

    ''' <summary>The active palette — the scheme handed to <see cref="ApplyTheme"/>, or the current one.</summary>
    Private Function Palette() As ThemePalette
        Return If(_scheme, ThemeManager.Current).Palette
    End Function

    Private Sub RebuildThemeResources()
        _axisPen?.Dispose()
        _gridPen?.Dispose()
        _borderPen?.Dispose()
        Dim p As ThemePalette = Palette()
        Dim axis As Color = If(_axisColor = Color.Empty, p.BorderColor, _axisColor)
        Dim grid As Color = If(_gridColor = Color.Empty,
                               ThemeShapes.Blend(EffectivePlotBackColor(), p.BorderColor, 0.55),
                               _gridColor)
        Dim border As Color = If(_borderColor = Color.Empty, p.BorderColor, _borderColor)
        _axisPen = New Pen(axis)
        _gridPen = New Pen(grid)
        _borderPen = New Pen(border)
    End Sub

    ' =====================================================================
    ' INTERNALS SHARED WITH THE PAINTING FILE
    ' =====================================================================

    Private ReadOnly Property AxisPen As Pen
        Get
            If _axisPen Is Nothing Then RebuildThemeResources()
            Return _axisPen
        End Get
    End Property

    Private ReadOnly Property GridPen As Pen
        Get
            If _gridPen Is Nothing Then RebuildThemeResources()
            Return _gridPen
        End Get
    End Property

    Private ReadOnly Property BorderPen As Pen
        Get
            If _borderPen Is Nothing Then RebuildThemeResources()
            Return _borderPen
        End Get
    End Property

    ''' <summary>The band's fill. Designer choice unless the scheme is dark — see <see cref="_isDarkScheme"/>.</summary>
    Private Function EffectiveHeaderBackColor() As Color
        If _isDarkScheme OrElse _headerBackColor = Color.Empty Then Return Palette().SurfaceAltColor
        Return _headerBackColor
    End Function

    ''' <summary>The caption colour. Follows the band, for the reason given on <see cref="_isDarkScheme"/>.</summary>
    Private Function EffectiveHeaderTextColor() As Color
        If _isDarkScheme OrElse _headerTextColor = Color.Empty Then Return Palette().TextColor
        Return _headerTextColor
    End Function

    ''' <summary>The line under the band. Follows the band, same reason.</summary>
    Private Function EffectiveHeaderSeparatorColor() As Color
        If _isDarkScheme OrElse _headerSeparatorColor = Color.Empty Then Return Palette().BorderColor
        Return _headerSeparatorColor
    End Function

    Private Function EffectivePlotBackColor() As Color
        Return If(_plotBackColor = Color.Empty, Palette().SurfaceAltColor, _plotBackColor)
    End Function

    Private Function EffectiveHeaderFont() As Font
        If _headerFont IsNot Nothing Then Return _headerFont
        If _derivedHeaderFont Is Nothing Then RebuildDerivedFonts()
        Return If(_derivedHeaderFont, Font)
    End Function

    Private Function EffectiveAxisFont() As Font
        If _axisFont IsNot Nothing Then Return _axisFont
        If _derivedAxisFont Is Nothing Then RebuildDerivedFonts()
        Return If(_derivedAxisFont, Font)
    End Function

    ' The two derived fonts own GDI handles, so they are built once and disposed, never created
    ' inside OnPaint.
    Private Sub RebuildDerivedFonts()
        Try
            Dim baseFont As Font = MyBase.Font
            If baseFont Is Nothing Then Return
            _derivedHeaderFont?.Dispose()
            _derivedAxisFont?.Dispose()
            _derivedHeaderFont = New Font(baseFont, FontStyle.Bold)
            _derivedAxisFont = New Font(baseFont.FontFamily, Math.Max(6.0F, baseFont.Size - 1.0F), FontStyle.Regular)
        Catch ex As Exception
            ' A font family that cannot produce the derived style is not worth a crash: the caller
            ' falls back to Font when the cached one is Nothing.
            _derivedHeaderFont = Nothing
            _derivedAxisFont = Nothing
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotChartView.RebuildDerivedFonts", ex)
        End Try
    End Sub

    ''' <summary>Asks for a fresh layout on next use. Called by both collections.</summary>
    Friend Sub InvalidateChartLayout()
        _layoutValid = False
        If _updateDepth > 0 Then Return
        Invalidate()
    End Sub

    Private Function FindSeriesIndex(key As String) As Integer
        If String.IsNullOrEmpty(key) Then Return -1
        For i As Integer = 0 To _series.Count - 1
            If String.Equals(_series(i).Key, key, StringComparison.Ordinal) Then Return i
        Next
        Return -1
    End Function

    ' The index or ArgumentException — no silent no-ops (house rule).
    Private Function RequireSeriesIndex(key As String) As Integer
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Empty key.", NameOf(key))
        Dim idx As Integer = FindSeriesIndex(key)
        If idx < 0 Then Throw New ArgumentException($"Unknown series key: '{key}'.", NameOf(key))
        Return idx
    End Function

    Private Function FindTabIndex(key As String) As Integer
        If String.IsNullOrEmpty(key) Then Return -1
        For i As Integer = 0 To _tabs.Count - 1
            If String.Equals(_tabs(i).Key, key, StringComparison.Ordinal) Then Return i
        Next
        Return -1
    End Function

    Private Function RequireTabIndex(key As String) As Integer
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Empty key.", NameOf(key))
        Dim idx As Integer = FindTabIndex(key)
        If idx < 0 Then Throw New ArgumentException($"Unknown tab key: '{key}'.", NameOf(key))
        Return idx
    End Function

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        RebuildDerivedFonts()
        InvalidateChartLayout()
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        InvalidateChartLayout()
    End Sub

    ''' <summary>
    ''' Before the handle exists <c>DeviceDpi</c> reports 96 even at 150%, so every logical measure
    ''' resolved earlier would be wrong. The layout is thrown away here on purpose.
    ''' </summary>
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        RebuildDerivedFonts()
        InvalidateChartLayout()
    End Sub

    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        InvalidateChartLayout()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _axisPen?.Dispose()
            _axisPen = Nothing
            _gridPen?.Dispose()
            _gridPen = Nothing
            _borderPen?.Dispose()
            _borderPen = Nothing
            _derivedHeaderFont?.Dispose()
            _derivedHeaderFont = Nothing
            _derivedAxisFont?.Dispose()
            _derivedAxisFont = Nothing
            _pointTooltip?.Dispose()
            _pointTooltip = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
