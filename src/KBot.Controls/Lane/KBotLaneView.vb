Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' An owner-drawn PLACEMENT surface: one horizontal lane per thing that owns markers, one marker
''' per dated event, and a drag that moves a marker from one lane to another.
'''
''' <para><b>What it is for.</b> A chart answers "how did this move". This answers the other half —
''' "which one does it belong to" — for a picture the operator is building rather than reading. It
''' was written for the reception ▸ snapshot editor, where twenty receptions of twenty snapshots
''' each is the ordinary case and the whole picture has to be visible at once, because a placement
''' that can only be judged one row at a time cannot be judged at all.</para>
'''
''' <para><b>Compact by default: NO TEXT.</b> <see cref="LaneCaptionsVisible"/> and
''' <see cref="MarkerLabelsVisible"/> are both False, so the surface is markers on lanes and
''' nothing else. That is not minimalism, it is arithmetic: four hundred markers with labels is not
''' a picture, and the names are one hover away. The same control set roomy — captions on, labels
''' on, axis on, <see cref="LaneHeight"/> around 26 — is the enlarged reading of the same data.</para>
'''
''' <para><b>The control never moves a marker.</b> It raises <c>MarkerDropped</c> and stops. The
''' lanes are a PROJECTION of the host's picture; a marker moved locally would show a placement
''' nobody has recorded. Same rule, and the same reason, as
''' <c>AdvancedTreeControl.NodeDropped</c>.</para>
'''
''' <para><b>Guides.</b> The dated lines drawn floor to ceiling across every lane are the SAME
''' <see cref="KBotChartGuide"/> objects a <see cref="KBotChartView"/> draws, so a payment falls on
''' the same date in both surfaces. That is the point of putting the two on one axis: a marker
''' dropped on the wrong side of a payment line is visible as such at the moment of the drop.</para>
'''
''' <para><b>Colours.</b> None are written here. Every colour property defaults to
''' <c>Color.Empty</c> = "from the theme", and a lane without a colour is handed one from
''' <see cref="AutoColor"/> — the same set the chart uses, so a lane and the line that means the
''' same thing can be told to match.</para>
'''
''' <para><b>Pixel metrics are LOGICAL pixels (96 dpi)</b> and are scaled at paint time through
''' <c>ThemeShapes.ScaleDpi(Me, …)</c>, one source. Fonts are in points and scale themselves.</para>
'''
''' <para>Conventions C1..C9: <c>src/KBot.Controls/CONTROLS.md</c>.</para>
''' </summary>
<ToolboxItem(True)>
<DefaultProperty("Lanes")>
<DefaultEvent("MarkerDropped")>
Partial Public NotInheritable Class KBotLaneView
    Inherits Control
    Implements IThemedControl
    Implements ISupportInitialize
    Implements IKBotGuideHost

    Private ReadOnly _lanes As New KBotLaneCollection()
    Private ReadOnly _guides As New KBotChartGuideCollection()

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

    ' ── The enlarge button, drawn at the right of the band ───────────────────
    Private _enlargeButtonVisible As Boolean = True
    Private _enlargeButtonImage As Image
    Private _enlargeButtonSize As Size = New Size(16, 16)
    Private _enlargeButtonTooltip As String = String.Empty

    ' ── Lanes ────────────────────────────────────────────────────────────────
    Private _laneHeight As Integer = 13
    Private _laneSpacing As Integer = 2
    Private _laneCaptionsVisible As Boolean = False
    Private _laneCaptionWidth As Integer = 120
    Private _markerLabelsVisible As Boolean = False
    Private _markerSize As Integer = 7
    Private _laneLineWidth As Integer = 1
    Private _segmentedRail As Boolean = True
    Private _segmentWidth As Integer = 0
    Private _laneLineColor As Color = Color.Empty
    Private _laneHoverBackColor As Color = Color.Empty
    Private _separatorColor As Color = Color.Empty
    Private _separatorWidth As Integer = 1
    Private _endMarkSize As Integer = 9

    ' ── Plot ─────────────────────────────────────────────────────────────────
    Private _plotMargin As Integer = 6
    Private _plotBackColor As Color = Color.Empty
    Private _borderVisible As Boolean = True
    Private _borderColor As Color = Color.Empty
    Private _borderWidth As Integer = 1
    Private _cornerRadius As Integer = -1
    Private _trailingSpace As Integer = 0

    ' ── Axis ─────────────────────────────────────────────────────────────────
    Private _axisVisible As Boolean = False
    Private _axisTextColor As Color = Color.Empty
    Private _axisFont As Font
    Private _momentFormat As String = "dd.MM.yy"
    Private _axisLabelGap As Integer = 4

    ' ── Empty state ──────────────────────────────────────────────────────────
    Private _emptyText As String = String.Empty
    Private _emptyTextColor As Color = Color.Empty

    ' ── Hovering and the floating label ──────────────────────────────────────
    Private _markerTooltip As KBotToolTip
    Private _markerTooltipEnabled As Boolean = True
    Private _hoverRadius As Integer = 10

    ' ── Runtime state (never serialized) ─────────────────────────────────────
    Private _scheme As ThemeScheme

    ''' <summary>
    ''' THE DARK-SCHEME EXCEPTION, for the header band. Same rule, and same reason, as
    ''' <c>KBotChartView</c> and <c>AdvancedTreeControl.BandColorsFromThemeOnly</c>: a band colour
    ''' chosen for the light scheme stays light grey above a dark surface, with the scheme's
    ''' now-light caption written on it, which is unreadable. While the scheme is dark the band's
    ''' designer colours are IGNORED, not erased — they come back whole on the way to a light
    ''' scheme, and the designer keeps serializing them.
    ''' </summary>
    Private _isDarkScheme As Boolean
    Private _layoutValid As Boolean
    Private _updateDepth As Integer
    Private _initializing As Boolean
    Private _plotRect As Rectangle = Rectangle.Empty
    Private _headerRect As Rectangle = Rectangle.Empty
    Private _enlargeRect As Rectangle = Rectangle.Empty
    Private _minTicks As Double
    Private _maxTicks As Double
    Private _minMoment As Date = Date.MinValue
    Private _maxMoment As Date = Date.MinValue

    ' The whole stack of lanes, in device pixels, before scrolling. What the scrollbar measures.
    Private _contentHeight As Integer

    Private _hoverLaneIndex As Integer = -1
    Private _hoverMarkerIndex As Integer = -1
    Private _hoverGuideIndex As Integer = -1
    Private _hoverEnlarge As Boolean

    ' Runtime overrides of the time axis. Date.MinValue on either = "work it out from the markers".
    Private _rangeStart As Date = Date.MinValue
    Private _rangeEnd As Date = Date.MinValue

    ' The key of whatever is "in the label" right now. Without it every pixel of movement over the
    ' same marker would reschedule the label and it would never actually appear.
    Private _currentTipKey As String
    Private ReadOnly _tipContent As New KBotToolTipContent()

    ' Vertical only. A lane view never scrolls sideways: the horizontal axis is TIME, and a time
    ' axis that runs off the edge has stopped being a comparison between lanes, which is the one
    ' thing this surface is for. Too many lanes to fit is an ordinary scroll; too long a span is
    ' answered by the enlarged window.
    Private ReadOnly _vScroll As New VScrollBar()

    ' Pens that do NOT depend on a lane are built once, in ApplyTheme, and freed in Dispose.
    Private _borderPen As Pen
    Private _laneLinePen As Pen
    Private _separatorPen As Pen

    ' Font DERIVED from Font (bold header). Cached because deriving a font on every paint
    ' allocates a GDI handle per repaint; rebuilt whenever Font or the override changes.
    Private _derivedHeaderFont As Font
    Private _derivedAxisFont As Font

    ' "The operator pinned this" flags — see ShouldSerializeBackColor.
    Private _backColorPinned As Boolean
    Private _foreColorPinned As Boolean
    Private _fontPinned As Boolean

    ''' <summary>
    ''' Raised when the operator presses the enlarge button on the band. The view does nothing
    ''' about it — the host decides what "bigger" means, which is usually a window holding a second
    ''' instance of this same control set roomy.
    ''' </summary>
    Public Event EnlargeRequested()

    ''' <summary>
    ''' Raised when the marker under the pointer changes. <paramref name="laneKey"/> is
    ''' <c>Nothing</c> and <paramref name="markerIndex"/> is -1 when the pointer leaves every marker.
    ''' </summary>
    Public Event MarkerHovered(laneKey As String, markerIndex As Integer)

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable, True)
        TabStop = True
        _lanes.Owner = Me
        _guides.Owner = Me

        _vScroll.Minimum = 0
        _vScroll.Maximum = 0
        _vScroll.Visible = False
        AddHandler _vScroll.Scroll, AddressOf OnVScrollScroll
        Controls.Add(_vScroll)

        ' AllowDrop is what makes a control receive DragOver/DragDrop. Set here rather than in a
        ' property because this control exists in order to be dropped on — unlike the tree, whose
        ' nine other hosts must not gain a behaviour they did not ask for.
        If Not KBotDesignTime.IsDesignTime(Me) Then AllowDrop = True
    End Sub

    ' =====================================================================
    ' INHERITED PROPERTIES WITH A PIN FLAG
    ' =====================================================================

    ''' <summary>The surface behind the whole control; not pinned here, it follows the theme.</summary>
    <Category("K-BOT Lane Appearance")>
    <Description("Background of the whole control. Not pinned here, it follows the active theme.")>
    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(value As Color)
            _backColorPinned = True
            MyBase.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Answers from the pin flag, not from "has it ever been written". Without this the designer
    ''' freezes the colour our own <c>ApplyTheme</c> wrote, and that value then reads as a
    ''' deliberate operator choice forever (see CONTROLS.md C4).
    ''' </summary>
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColorPinned
    End Function

    Public Overrides Sub ResetBackColor()
        _backColorPinned = False
        If _scheme IsNot Nothing Then MyBase.BackColor = _scheme.Palette.SurfaceColor
    End Sub

    <Category("K-BOT Lane Appearance")>
    <Description("Default text colour. Not pinned here, it follows the active theme.")>
    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(value As Color)
            _foreColorPinned = True
            MyBase.ForeColor = value
        End Set
    End Property

    Public Function ShouldSerializeForeColor() As Boolean
        Return _foreColorPinned
    End Function

    Public Overrides Sub ResetForeColor()
        _foreColorPinned = False
        If _scheme IsNot Nothing Then MyBase.ForeColor = _scheme.Palette.TextColor
    End Sub

    ''' <summary>
    ''' <c>Font</c> cannot carry a <c>DefaultValue</c> (the attribute needs a constant), so without
    ''' the pair below a freshly dropped control writes a font line into every host form.
    ''' </summary>
    <Category("K-BOT Lane Appearance")>
    <Description("Font of the captions and labels. Not pinned here, it follows the ambient font of the active scheme.")>
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
        _fontPinned = False
        MyBase.ResetFont()
    End Sub

    ''' <summary>
    ''' <c>Size</c> cannot carry a <c>DefaultValue</c> either. The default below is deliberately
    ''' wide and short: a strip of lanes on one time axis, not a square panel.
    ''' </summary>
    Protected Overrides ReadOnly Property DefaultSize As Size
        Get
            Return New Size(420, 150)
        End Get
    End Property

    ' =====================================================================
    ' DATA
    ' =====================================================================

    ''' <summary>
    ''' The lanes, top to bottom. Editable from the property grid (the standard collection dialog)
    ''' or from code through <see cref="AddLane"/> — the same collection.
    ''' </summary>
    <Category("K-BOT Lane Data")>
    <Description("The lanes, top to bottom. A lane with SeparatorAbove cuts the surface in two.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Lanes As KBotLaneCollection
        Get
            Return _lanes
        End Get
    End Property

    ''' <summary>
    ''' The dated lines drawn floor to ceiling across every lane, BEHIND the markers. The same
    ''' <see cref="KBotChartGuide"/> type a <see cref="KBotChartView"/> draws, so the two surfaces
    ''' can be given the same objects and cannot then disagree about a date.
    ''' </summary>
    ''' <remarks>
    ''' A guide does not stretch the time axis: one outside the span of the markers is simply not
    ''' drawn. The axis belongs to the data, and a payment made months after the last marker would
    ''' otherwise squash every lane against one edge.
    ''' </remarks>
    <Category("K-BOT Lane Data")>
    <Description("Dated vertical lines drawn across all lanes, behind the markers. They do not stretch the time axis.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Guides As KBotChartGuideCollection
        Get
            Return _guides
        End Get
    End Property

    ''' <summary>Text drawn in the middle when there is no visible lane with a marker.</summary>
    <Category("K-BOT Lane Data")>
    <Description("Text drawn in the middle when nothing is on the surface. This one IS read by the operator, so write it in the operator's language.")>
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

    <Category("K-BOT Lane Data")>
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

    ''' <summary>
    ''' Start of the time axis, or <c>Date.MinValue</c> to work it out from the markers.
    ''' </summary>
    ''' <remarks>
    ''' <para>Runtime only, never serialized: a pinned range is a statement about the DATA a host
    ''' is about to load, not about how the control looks.</para>
    ''' <para>Pinning it lets a host line this surface up with a chart above it, pixel for pixel.
    ''' The cost is that markers outside the pinned range are not drawn — so pin it only when the
    ''' two surfaces really are showing the same span, and never when this one is the only place an
    ''' unplaced marker can be seen.</para>
    ''' </remarks>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property RangeStart As Date
        Get
            Return _rangeStart
        End Get
        Set(value As Date)
            _rangeStart = value
            InvalidateLaneLayout()
        End Set
    End Property

    ''' <summary>End of the time axis, or <c>Date.MinValue</c> to work it out from the markers.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property RangeEnd As Date
        Get
            Return _rangeEnd
        End Get
        Set(value As Date)
            _rangeEnd = value
            InvalidateLaneLayout()
        End Set
    End Property

    ''' <summary>The first moment actually drawn. <c>Date.MinValue</c> when there is nothing.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property PlottedRangeStart As Date
        Get
            EnsureLayout()
            Return _minMoment
        End Get
    End Property

    ''' <summary>The last moment actually drawn. <c>Date.MinValue</c> when there is nothing.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property PlottedRangeEnd As Date
        Get
            EnsureLayout()
            Return _maxMoment
        End Get
    End Property

    ' =====================================================================
    ' HEADER BAND
    ' =====================================================================

    <Category("K-BOT Lane Header")>
    <Description("False => no band at all; the lanes take the whole control and the enlarge button is unreachable.")>
    <DefaultValue(True)>
    Public Property HeaderVisible As Boolean
        Get
            Return _headerVisible
        End Get
        Set(value As Boolean)
            _headerVisible = value
            InvalidateLaneLayout()
        End Set
    End Property

    <Category("K-BOT Lane Header")>
    <Description("Height of the band (logical px at 96 dpi, scaled at paint time). Default 28.")>
    <DefaultValue(28)>
    Public Property HeaderHeight As Integer
        Get
            Return _headerHeight
        End Get
        Set(value As Integer)
            _headerHeight = Math.Max(0, value)
            InvalidateLaneLayout()
        End Set
    End Property

    <Category("K-BOT Lane Header")>
    <Description("Title written at the left of the band. This is read by the operator. Empty = no title.")>
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

    <Category("K-BOT Lane Header")>
    <Description("Font of the band title. Nothing = the control font in bold.")>
    Public Property HeaderFont As Font
        Get
            Return _headerFont
        End Get
        Set(value As Font)
            _headerFont = value
            RebuildDerivedFonts()
            InvalidateLaneLayout()
        End Set
    End Property

    Public Function ShouldSerializeHeaderFont() As Boolean
        Return _headerFont IsNot Nothing
    End Function

    Public Sub ResetHeaderFont()
        HeaderFont = Nothing
    End Sub

    <Category("K-BOT Lane Header")>
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

    <Category("K-BOT Lane Header")>
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

    <Category("K-BOT Lane Header")>
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

    <Category("K-BOT Lane Header")>
    <Description("Thickness of the line under the band (logical px). 0 = no line.")>
    <DefaultValue(1)>
    Public Property HeaderSeparatorWidth As Integer
        Get
            Return _headerSeparatorWidth
        End Get
        Set(value As Integer)
            _headerSeparatorWidth = Math.Max(0, value)
            InvalidateLaneLayout()
        End Set
    End Property

    <Category("K-BOT Lane Header")>
    <Description("Strength of the gradient on the band background (0..100; 0 = flat fill). Goes through ThemeShapes.FillModern, so it introduces no new colour.")>
    <DefaultValue(0)>
    Public Property HeaderGradient As Integer
        Get
            Return _headerGradient
        End Get
        Set(value As Integer)
            _headerGradient = Math.Max(0, Math.Min(100, value))
            Invalidate()
        End Set
    End Property

    ' =====================================================================
    ' THE ENLARGE BUTTON
    ' =====================================================================

    <Category("K-BOT Lane Header")>
    <Description("False => no enlarge button; EnlargeRequested can then only be raised by the keyboard.")>
    <DefaultValue(True)>
    Public Property EnlargeButtonVisible As Boolean
        Get
            Return _enlargeButtonVisible
        End Get
        Set(value As Boolean)
            _enlargeButtonVisible = value
            InvalidateLaneLayout()
        End Set
    End Property

    ''' <summary>
    ''' Icon of the enlarge button. <c>Nothing</c> = the control draws its own — two arrows pushing
    ''' apart — so a host that provides no image still gets a button that reads as one.
    ''' </summary>
    <Category("K-BOT Lane Header")>
    <Description("Icon of the enlarge button. Nothing = a drawn arrows glyph.")>
    <DefaultValue(CType(Nothing, Image))>
    Public Property EnlargeButtonImage As Image
        Get
            Return _enlargeButtonImage
        End Get
        Set(value As Image)
            _enlargeButtonImage = value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Lane Header")>
    <Description("Drawn size of the enlarge button glyph (logical px).")>
    Public Property EnlargeButtonSize As Size
        Get
            Return _enlargeButtonSize
        End Get
        Set(value As Size)
            _enlargeButtonSize = New Size(Math.Max(1, value.Width), Math.Max(1, value.Height))
            InvalidateLaneLayout()
        End Set
    End Property

    Public Function ShouldSerializeEnlargeButtonSize() As Boolean
        Return _enlargeButtonSize <> New Size(16, 16)
    End Function

    Public Sub ResetEnlargeButtonSize()
        EnlargeButtonSize = New Size(16, 16)
    End Sub

    <Category("K-BOT Lane Header")>
    <Description("Floating label of the enlarge button (multiple lines; accepts the rich-text markup of KBotToolTip). Read by the operator. Empty = no label.")>
    <DefaultValue("")>
    Public Property EnlargeButtonTooltip As String
        Get
            Return _enlargeButtonTooltip
        End Get
        Set(value As String)
            _enlargeButtonTooltip = If(value, String.Empty)
        End Set
    End Property

    ' =====================================================================
    ' LANES
    ' =====================================================================

    ''' <summary>
    ''' Height of one lane, logical px at 96 dpi. Default 13 — twenty-one lanes in 275 px.
    ''' </summary>
    <Category("K-BOT Lane Layout")>
    <Description("Height of one lane (logical px at 96 dpi, scaled at paint time). Default 13; around 26 is the roomy reading.")>
    <DefaultValue(13)>
    Public Property LaneHeight As Integer
        Get
            Return _laneHeight
        End Get
        Set(value As Integer)
            _laneHeight = Math.Max(3, value)
            InvalidateLaneLayout()
        End Set
    End Property

    <Category("K-BOT Lane Layout")>
    <Description("Air between two lanes (logical px). Default 2.")>
    <DefaultValue(2)>
    Public Property LaneSpacing As Integer
        Get
            Return _laneSpacing
        End Get
        Set(value As Integer)
            _laneSpacing = Math.Max(0, value)
            InvalidateLaneLayout()
        End Set
    End Property

    ''' <summary>
    ''' True => each lane's <c>Text</c> is painted at its left, in a gutter
    ''' <see cref="LaneCaptionWidth"/> wide. False (the default) => no text at all.
    ''' </summary>
    <Category("K-BOT Lane Layout")>
    <Description("True => the lane names are painted at the left. False (default) => no text at all; the names are one hover away.")>
    <DefaultValue(False)>
    Public Property LaneCaptionsVisible As Boolean
        Get
            Return _laneCaptionsVisible
        End Get
        Set(value As Boolean)
            _laneCaptionsVisible = value
            InvalidateLaneLayout()
        End Set
    End Property

    <Category("K-BOT Lane Layout")>
    <Description("Width of the caption gutter at the left (logical px). Ignored while LaneCaptionsVisible is False.")>
    <DefaultValue(120)>
    Public Property LaneCaptionWidth As Integer
        Get
            Return _laneCaptionWidth
        End Get
        Set(value As Integer)
            _laneCaptionWidth = Math.Max(0, value)
            InvalidateLaneLayout()
        End Set
    End Property

    ''' <summary>
    ''' True => each marker's <c>Text</c> is painted beside it. Meant for the roomy reading only:
    ''' on a compact strip the labels of markers minutes apart overwrite each other.
    ''' </summary>
    <Category("K-BOT Lane Layout")>
    <Description("True => each marker's name is painted beside it. Meant for the roomy reading; on a compact strip the labels collide.")>
    <DefaultValue(False)>
    Public Property MarkerLabelsVisible As Boolean
        Get
            Return _markerLabelsVisible
        End Get
        Set(value As Boolean)
            _markerLabelsVisible = value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Lane Layout")>
    <Description("Drawn size of a marker (logical px). Default 7.")>
    <DefaultValue(7)>
    Public Property MarkerSize As Integer
        Get
            Return _markerSize
        End Get
        Set(value As Integer)
            _markerSize = Math.Max(2, value)
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Lane Layout")>
    <Description("Thickness of the line drawn along a lane (logical px). 0 = no line; the markers then float on their own.")>
    <DefaultValue(1)>
    Public Property LaneLineWidth As Integer
        Get
            Return _laneLineWidth
        End Get
        Set(value As Integer)
            _laneLineWidth = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Every marker paints the stretch of rail it OWNS: from itself to the next marker, and — for
    ''' the last one — to the right-hand end of the surface.
    ''' </summary>
    ''' <remarks>
    ''' <para>This is a statement about the data, not a decoration. What a marker records holds
    ''' until the next marker changes it; the same truth the chart draws as a step line. A single
    ''' flat rail said the opposite — that the lane is one undifferentiated thing — and it made the
    ''' one question the surface exists to answer ("which stretch is this, and where does it end")
    ''' something the operator had to work out from marker positions alone.</para>
    ''' <para>The plain rail is still drawn UNDERNEATH, full width, in
    ''' <see cref="LaneLineColor"/>: a lane holding no marker at all has to stay visible as
    ''' somewhere to drop, and a lane whose markers start late has to show the empty run before
    ''' them as empty rather than as absent.</para>
    ''' <para>Use <see cref="TrailingSpace"/> to give the last stretch of every lane somewhere to
    ''' be. Without it the last marker sits exactly on the right edge and owns no pixels.</para>
    ''' </remarks>
    <Category("K-BOT Lane Layout")>
    <Description("True => each marker paints the rail from itself to the next one (the last, to the right-hand end), in its own colour. False => one flat rail.")>
    <DefaultValue(True)>
    Public Property SegmentedRail As Boolean
        Get
            Return _segmentedRail
        End Get
        Set(value As Boolean)
            _segmentedRail = value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Lane Layout")>
    <Description("Thickness of the coloured stretch a marker owns (logical px). 0 = the same thickness as LaneLineWidth.")>
    <DefaultValue(0)>
    Public Property SegmentWidth As Integer
        Get
            Return _segmentWidth
        End Get
        Set(value As Integer)
            _segmentWidth = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Room kept at the right-hand end of the time axis, past the latest moment (logical px).
    ''' </summary>
    ''' <remarks>
    ''' The latest marker lands on the right edge of the surface, so the stretch it owns is zero
    ''' pixels wide and the operator cannot see it at all — the one marker whose stretch is still
    ''' open is the one that disappears. This is the space it runs into. Guides move with it: the
    ''' whole time axis is compressed into the narrower run, so a payment line still falls where
    ''' its date falls.
    ''' </remarks>
    <Category("K-BOT Lane Plot")>
    <Description("Room kept to the right of the latest moment (logical px), so the last stretch of every lane is visible.")>
    <DefaultValue(0)>
    Public Property TrailingSpace As Integer
        Get
            Return _trailingSpace
        End Get
        Set(value As Integer)
            _trailingSpace = Math.Max(0, value)
            InvalidateLaneLayout()
        End Set
    End Property

    <Category("K-BOT Lane Layout")>
    <Description("Colour of the line drawn along a lane. Empty = the border colour of the theme, so the rail stays behind the markers.")>
    Public Property LaneLineColor As Color
        Get
            Return _laneLineColor
        End Get
        Set(value As Color)
            _laneLineColor = value
            RebuildThemeResources()
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeLaneLineColor() As Boolean
        Return _laneLineColor <> Color.Empty
    End Function

    Public Sub ResetLaneLineColor()
        LaneLineColor = Color.Empty
    End Sub

    <Category("K-BOT Lane Layout")>
    <Description("Background of the lane under the pointer. Empty = the alternate surface colour of the theme.")>
    Public Property LaneHoverBackColor As Color
        Get
            Return _laneHoverBackColor
        End Get
        Set(value As Color)
            _laneHoverBackColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeLaneHoverBackColor() As Boolean
        Return _laneHoverBackColor <> Color.Empty
    End Function

    Public Sub ResetLaneHoverBackColor()
        LaneHoverBackColor = Color.Empty
    End Sub

    <Category("K-BOT Lane Layout")>
    <Description("Colour of the line drawn above a lane marked SeparatorAbove. Empty = the border colour of the theme.")>
    Public Property SeparatorColor As Color
        Get
            Return _separatorColor
        End Get
        Set(value As Color)
            _separatorColor = value
            RebuildThemeResources()
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeSeparatorColor() As Boolean
        Return _separatorColor <> Color.Empty
    End Function

    Public Sub ResetSeparatorColor()
        SeparatorColor = Color.Empty
    End Sub

    <Category("K-BOT Lane Layout")>
    <Description("Thickness of the separator above a lane marked SeparatorAbove (logical px). 0 = no line.")>
    <DefaultValue(1)>
    Public Property SeparatorWidth As Integer
        Get
            Return _separatorWidth
        End Get
        Set(value As Integer)
            _separatorWidth = Math.Max(0, value)
            RebuildThemeResources()
            InvalidateLaneLayout()
        End Set
    End Property

    <Category("K-BOT Lane Layout")>
    <Description("Drawn size of the mark at the closed end of a lane (logical px). Default 9.")>
    <DefaultValue(9)>
    Public Property EndMarkSize As Integer
        Get
            Return _endMarkSize
        End Get
        Set(value As Integer)
            _endMarkSize = Math.Max(2, value)
            InvalidateLaneLayout()
        End Set
    End Property

    ' =====================================================================
    ' PLOT
    ' =====================================================================

    <Category("K-BOT Lane Plot")>
    <Description("Air between the lanes and the edges of the control (logical px).")>
    <DefaultValue(6)>
    Public Property PlotMargin As Integer
        Get
            Return _plotMargin
        End Get
        Set(value As Integer)
            _plotMargin = Math.Max(0, value)
            InvalidateLaneLayout()
        End Set
    End Property

    <Category("K-BOT Lane Plot")>
    <Description("Background behind the lanes. Empty = the surface colour of the theme.")>
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

    <Category("K-BOT Lane Plot")>
    <Description("False => no frame around the control.")>
    <DefaultValue(True)>
    Public Property BorderVisible As Boolean
        Get
            Return _borderVisible
        End Get
        Set(value As Boolean)
            _borderVisible = value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Lane Plot")>
    <Description("Colour of the frame. Empty = the border colour of the theme.")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
            RebuildThemeResources()
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeBorderColor() As Boolean
        Return _borderColor <> Color.Empty
    End Function

    Public Sub ResetBorderColor()
        BorderColor = Color.Empty
    End Sub

    ''' <summary>Thickness of the frame, in LOGICAL pixels — scaled at paint time like every other measure here.</summary>
    <Category("K-BOT Lane Plot")>
    <Description("Thickness of the frame (logical px). 0 = no frame, the same as BorderVisible = False.")>
    <DefaultValue(1)>
    Public Property BorderWidth As Integer
        Get
            Return _borderWidth
        End Get
        Set(value As Integer)
            _borderWidth = Math.Max(0, value)
            RebuildThemeResources()
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Lane Plot")>
    <Description("Corner radius of the frame (logical px). -1 = the radius of the active scheme; 0 = square corners.")>
    <DefaultValue(-1)>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            _cornerRadius = Math.Max(-1, value)
            Invalidate()
        End Set
    End Property

    ' =====================================================================
    ' AXIS
    ' =====================================================================

    ''' <summary>
    ''' True => the first and last moment are written under the lanes. Off by default: on a compact
    ''' strip the two dates cost a whole lane's worth of height to say what the chart above already
    ''' says.
    ''' </summary>
    <Category("K-BOT Lane Axis")>
    <Description("True => the first and last moment are written under the lanes. Off by default.")>
    <DefaultValue(False)>
    Public Property AxisVisible As Boolean
        Get
            Return _axisVisible
        End Get
        Set(value As Boolean)
            _axisVisible = value
            InvalidateLaneLayout()
        End Set
    End Property

    <Category("K-BOT Lane Axis")>
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

    <Category("K-BOT Lane Axis")>
    <Description("Font of the axis labels and of the marker labels. Nothing = the control font, one point smaller.")>
    Public Property AxisFont As Font
        Get
            Return _axisFont
        End Get
        Set(value As Font)
            _axisFont = value
            RebuildDerivedFonts()
            InvalidateLaneLayout()
        End Set
    End Property

    Public Function ShouldSerializeAxisFont() As Boolean
        Return _axisFont IsNot Nothing
    End Function

    Public Sub ResetAxisFont()
        AxisFont = Nothing
    End Sub

    <Category("K-BOT Lane Axis")>
    <Description("Format of the two axis labels. Standard .NET date format string.")>
    <DefaultValue("dd.MM.yy")>
    Public Property MomentFormat As String
        Get
            Return _momentFormat
        End Get
        Set(value As String)
            _momentFormat = If(String.IsNullOrEmpty(value), "dd.MM.yy", value)
            InvalidateLaneLayout()
        End Set
    End Property

    <Category("K-BOT Lane Axis")>
    <Description("Air between the lanes and the axis labels (logical px).")>
    <DefaultValue(4)>
    Public Property AxisLabelGap As Integer
        Get
            Return _axisLabelGap
        End Get
        Set(value As Integer)
            _axisLabelGap = Math.Max(0, value)
            InvalidateLaneLayout()
        End Set
    End Property

    ' =====================================================================
    ' THE FLOATING LABEL
    ' =====================================================================

    ''' <summary>
    ''' The floating label used for markers, lanes, guides and the enlarge button. Always a
    ''' <see cref="KBotToolTip"/>: everything here is a painted region, not a control, so there is
    ''' nothing for <c>System.Windows.Forms.ToolTip</c> to extend (CONTROLS.md C8).
    ''' </summary>
    <Category("K-BOT Lane Tooltip")>
    <Description("The floating label. Change its look through its Style; the text comes from the lanes, the markers and the guides.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property MarkerTooltip As KBotToolTip
        Get
            ' Created lazily, at first need, and NEVER inside the designer: that would mean a
            ' window opened inside Visual Studio.
            If _markerTooltip Is Nothing Then _markerTooltip = New KBotToolTip()
            Return _markerTooltip
        End Get
    End Property

    <Category("K-BOT Lane Tooltip")>
    <Description("False => hovering highlights but opens no label.")>
    <DefaultValue(True)>
    Public Property MarkerTooltipEnabled As Boolean
        Get
            Return _markerTooltipEnabled
        End Get
        Set(value As Boolean)
            _markerTooltipEnabled = value
            If Not value Then HideLaneTip()
        End Set
    End Property

    <Category("K-BOT Lane Tooltip")>
    <Description("How far from a marker the pointer may be and still name it (logical px). Default 10.")>
    <DefaultValue(10)>
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
    ''' Suspends layout and painting while the host refills the surface. Nested calls are counted,
    ''' so a helper that brackets its own work does not undo its caller's bracket.
    ''' </summary>
    Public Sub BeginUpdate()
        _updateDepth += 1
    End Sub

    ''' <summary>Ends the block opened by <see cref="BeginUpdate"/> and repaints once.</summary>
    Public Sub EndUpdate()
        If _updateDepth > 0 Then _updateDepth -= 1
        If _updateDepth = 0 Then InvalidateLaneLayout()
    End Sub

    ''' <summary>Removes every lane. Guides and settings are untouched.</summary>
    Public Sub ClearLanes()
        _lanes.Clear()
        _hoverLaneIndex = -1
        _hoverMarkerIndex = -1
    End Sub

    ''' <summary>Removes every guide. Lanes and settings are untouched.</summary>
    Public Sub ClearGuides()
        _guides.Clear()
    End Sub

    ''' <summary>Appends a lane and returns it. The key must be non-empty and unique.</summary>
    Public Function AddLane(key As String, text As String) As KBotLane
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Empty key.", NameOf(key))
        If FindLaneIndex(key) >= 0 Then Throw New ArgumentException($"Duplicate key: '{key}'.", NameOf(key))
        Dim ln As New KBotLane(key, text)
        _lanes.Add(ln)
        Return ln
    End Function

    ''' <summary>Convenience for code: append a guide and return it.</summary>
    Public Function AddGuide(moment As Date, text As String) As KBotChartGuide
        Dim gd As New KBotChartGuide(moment, text)
        _guides.Add(gd)
        Return gd
    End Function

    ''' <summary>The lane with that key, or Nothing. The asking form — it does not throw.</summary>
    Public Function FindLane(key As String) As KBotLane
        Dim idx As Integer = FindLaneIndex(key)
        If idx < 0 Then Return Nothing
        Return _lanes(idx)
    End Function

    ''' <summary>Shows or hides one lane. Unknown key => <c>ArgumentException</c>.</summary>
    Public Sub SetLaneVisible(key As String, visible As Boolean)
        _lanes(RequireLaneIndex(key)).Visible = visible
        InvalidateLaneLayout()
    End Sub

    ''' <summary>The asking form for a lane key — it does not throw.</summary>
    Public Function ContainsLane(key As String) As Boolean
        Return FindLaneIndex(key) >= 0
    End Function

    ''' <summary>
    ''' The <paramref name="index"/>-th colour of the automatic set — the same set
    ''' <c>KBotChartView.AutoColor</c> hands out, so a lane and the chart line that mean the same
    ''' thing can be given the same colour rather than two that drift apart on the next theme
    ''' change. Never red; see <c>KBotAutoPalette</c>.
    ''' </summary>
    Public Function AutoColor(index As Integer) As Color
        Return KBotAutoPalette.ColorAt(Palette().AccentColor, _isDarkScheme, index)
    End Function

    ' =====================================================================
    ' ISupportInitialize
    ' =====================================================================

    ''' <summary>Start of the designer's initialization block (validation is suspended).</summary>
    Public Sub BeginInit() Implements ISupportInitialize.BeginInit
        _initializing = True
    End Sub

    ''' <summary>
    ''' End of the initialization block: the lane keys are validated (non-empty and unique) and the
    ''' surface is laid out again.
    '''
    ''' In the DESIGNER validation is skipped — a half-typed key would throw out of
    ''' <c>InitializeComponent</c>, which means the form would not open at all. The defect is
    ''' reported visually instead, with a red frame (see the painting file).
    ''' </summary>
    Public Sub EndInit() Implements ISupportInitialize.EndInit
        Try
            _initializing = False
            If Not KBotDesignTime.IsDesignTime(Me) Then ValidateKeys()
            InvalidateLaneLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLaneView.EndInit", ex)
            Throw
        End Try
    End Sub

    Private Sub ValidateKeys()
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        For i As Integer = 0 To _lanes.Count - 1
            Dim ln As KBotLane = _lanes(i)
            If String.IsNullOrWhiteSpace(ln.Key) Then
                Throw New ArgumentException($"Empty key on lane {i} ('{If(ln.Text, String.Empty)}').", NameOf(Lanes))
            End If
            If Not seen.Add(ln.Key) Then
                Throw New ArgumentException($"Duplicate key: '{ln.Key}' (lane {i}).", NameOf(Lanes))
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
            ApplyScrollBarTheme()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLaneView.ApplyTheme", ex)
        End Try
    End Sub

    ''' <summary>The active palette — the scheme handed to <see cref="ApplyTheme"/>, or the current one.</summary>
    Private Function Palette() As ThemePalette
        Return If(_scheme, ThemeManager.Current).Palette
    End Function

    Private Sub RebuildThemeResources()
        _borderPen?.Dispose()
        _laneLinePen?.Dispose()
        _separatorPen?.Dispose()
        Dim pal As ThemePalette = Palette()
        _borderPen = New Pen(If(_borderColor = Color.Empty, pal.BorderColor, _borderColor),
                             CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, Math.Max(1, _borderWidth)))))
        _laneLinePen = New Pen(If(_laneLineColor = Color.Empty, pal.BorderColor, _laneLineColor))
        _separatorPen = New Pen(If(_separatorColor = Color.Empty, pal.BorderColor, _separatorColor))
    End Sub

    Private ReadOnly Property BorderPen As Pen
        Get
            If _borderPen Is Nothing Then RebuildThemeResources()
            Return _borderPen
        End Get
    End Property

    Private ReadOnly Property LaneLinePen As Pen
        Get
            If _laneLinePen Is Nothing Then RebuildThemeResources()
            Return _laneLinePen
        End Get
    End Property

    Private ReadOnly Property SeparatorPen As Pen
        Get
            If _separatorPen Is Nothing Then RebuildThemeResources()
            Return _separatorPen
        End Get
    End Property

    Private Function EffectiveHeaderBackColor() As Color
        If _isDarkScheme OrElse _headerBackColor = Color.Empty Then Return Palette().SurfaceAltColor
        Return _headerBackColor
    End Function

    Private Function EffectiveHeaderTextColor() As Color
        If _isDarkScheme OrElse _headerTextColor = Color.Empty Then Return Palette().TextColor
        Return _headerTextColor
    End Function

    Private Function EffectiveHeaderSeparatorColor() As Color
        If _isDarkScheme OrElse _headerSeparatorColor = Color.Empty Then Return Palette().BorderColor
        Return _headerSeparatorColor
    End Function

    Private Function EffectivePlotBackColor() As Color
        If _plotBackColor = Color.Empty Then Return Palette().SurfaceColor
        Return _plotBackColor
    End Function

    Private Function EffectiveLaneHoverBackColor() As Color
        If _laneHoverBackColor = Color.Empty Then Return Palette().SurfaceAltColor
        Return _laneHoverBackColor
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

    ''' <summary>
    ''' Rebuilds the fonts DERIVED from <see cref="Font"/>. Cached because deriving a font on every
    ''' paint allocates a GDI handle per repaint.
    ''' </summary>
    Private Sub RebuildDerivedFonts()
        _derivedHeaderFont?.Dispose()
        _derivedAxisFont?.Dispose()
        _derivedHeaderFont = Nothing
        _derivedAxisFont = Nothing
        Dim f As Font = Font
        If f Is Nothing Then Return
        Try
            If _headerFont Is Nothing Then _derivedHeaderFont = New Font(f, FontStyle.Bold)
            If _axisFont Is Nothing Then
                _derivedAxisFont = New Font(f.FontFamily, Math.Max(6.0F, f.Size - 1.0F), f.Style)
            End If
        Catch ex As Exception
            ' A font family that refuses the derived style is not worth killing a repaint over:
            ' the fallback is the control font, which always exists.
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.RebuildDerivedFonts", ex)
        End Try
    End Sub

    ' =====================================================================
    ' PLUMBING
    ' =====================================================================

    ''' <summary>The layout is stale; recompute it before the next paint.</summary>
    Friend Sub InvalidateLaneLayout()
        _layoutValid = False
        If _initializing OrElse _updateDepth > 0 Then Return
        Invalidate()
    End Sub

    ' The guide collection is shared with KBotChartView, so it talks to its owner through an
    ' interface rather than to one control. Private implementations: a host never holds one.
    Private Sub InvalidateGuides() Implements IKBotGuideHost.InvalidateGuides
        InvalidateLaneLayout()
    End Sub

    Private ReadOnly Property GuideHostControl As Control Implements IKBotGuideHost.GuideHostControl
        Get
            Return Me
        End Get
    End Property

    Private Function FindLaneIndex(key As String) As Integer
        If String.IsNullOrEmpty(key) Then Return -1
        For i As Integer = 0 To _lanes.Count - 1
            If String.Equals(_lanes(i).Key, key, StringComparison.Ordinal) Then Return i
        Next
        Return -1
    End Function

    ''' <summary>Same as <see cref="FindLaneIndex"/> but an unknown key is a programming error.</summary>
    Private Function RequireLaneIndex(key As String) As Integer
        Dim idx As Integer = FindLaneIndex(key)
        If idx < 0 Then Throw New ArgumentException($"Unknown lane key: '{key}'.", NameOf(key))
        Return idx
    End Function

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        RebuildDerivedFonts()
        InvalidateLaneLayout()
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        InvalidateLaneLayout()
    End Sub

    ''' <summary>
    ''' Before the handle exists <c>DeviceDpi</c> reports 96 even at 150%, so everything measured
    ''' in the constructor is measured at the wrong scale. This is where it gets measured again.
    ''' </summary>
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        ApplyScrollBarTheme()
        ' Before the handle exists, DeviceDpi answers 96 whatever the screen is, so the frame pen
        ' built earlier carries the wrong thickness. Rebuilt here, where the answer is finally true.
        RebuildThemeResources()
        InvalidateLaneLayout()
    End Sub

    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        RebuildThemeResources()
        InvalidateLaneLayout()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _borderPen?.Dispose()
            _laneLinePen?.Dispose()
            _separatorPen?.Dispose()
            _derivedHeaderFont?.Dispose()
            _derivedAxisFont?.Dispose()
            _markerTooltip?.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
