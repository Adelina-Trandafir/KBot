Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The K-BOT calendar surface: a month grid, a year of months and a decade of years, all three
''' drawn by us so every pixel comes from the active scheme.
'''
''' <para><b>Why not <c>MonthCalendar</c>.</b> The stock control is a native window painted by
''' Windows: <c>BackColor</c>/<c>ForeColor</c> reach only part of it, the header keeps the system
''' colours, and on a dark scheme it stays a white card in the middle of the form — the same
''' reason <c>ComboBox</c> became <see cref="KBotComboBox"/> and the context menu became
''' <c>CustomPopup</c>. It also refuses most sizes: it snaps to whole month tiles, so it cannot be
''' docked or stretched to fill the space it was given.</para>
'''
''' <para><b>The zoom axis.</b> The header title is a button: it zooms OUT (days to months to
''' years). Picking a cell zooms back IN, and only a pick in <see cref="KBotCalendarView.Days"/>
''' produces a value. The arrows page by month, year, or decade depending on the view.</para>
'''
''' <para><b>Two events, on purpose.</b> <see cref="ValueChanged"/> fires for every move, including
''' walking the grid with the arrow keys; <see cref="DateSelected"/> fires only when the operator
''' CHOSE (click, Enter, the "today" row). A drop-down closes on the second, never on the first —
''' otherwise the first arrow key would shut it.</para>
'''
''' <para><b>Colours</b> follow the house contract: <c>Color.Empty</c> = "from the theme", anything
''' set explicitly wins and keeps winning across scheme switches, and every one has an
''' <c>Effective*</c> counterpart = what is actually painted (C1).</para>
'''
''' <para><b>Pixel metrics are LOGICAL pixels (96 dpi)</b>, scaled at paint time through
''' <c>ThemeShapes.ScaleDpi</c> (C2). <b>Month, day and "today" wording is Romanian</b> because the
''' operator reads it: it comes from <see cref="CultureName"/>, which defaults to <c>ro-RO</c>
''' rather than to the machine culture, so the same build reads the same everywhere.</para>
''' </summary>
<ToolboxItem(True)>
<DefaultProperty("Value")>
<DefaultEvent("DateSelected")>
Partial Public NotInheritable Class KBotCalendar
    Inherits Control
    Implements IThemedControl

    ' ── Hit-test answers that are not a cell index (cells are 0..n-1) ────────────
    Friend Const HitNone As Integer = -1
    Friend Const HitPrev As Integer = -2
    Friend Const HitNext As Integer = -3
    Friend Const HitTitle As Integer = -4
    Friend Const HitToday As Integer = -5

    ' ── Value and page ───────────────────────────────────────────────────────────
    Private _value As Date = Date.Today
    Private _displayMonth As Date = New Date(Date.Today.Year, Date.Today.Month, 1)
    Private _view As KBotCalendarView = KBotCalendarView.Days
    Private _minDate As Date = New Date(1900, 1, 1)
    Private _maxDate As Date = New Date(2100, 12, 31)

    ' ── Wording ──────────────────────────────────────────────────────────────────
    Private _cultureName As String = DefaultCultureName
    Private _culture As CultureInfo = CultureInfo.GetCultureInfo(DefaultCultureName)
    Private _firstDayOfWeek As DayOfWeek = DayOfWeek.Monday
    Private _todayFormat As String = "dd.MM.yyyy"

    ''' <summary>The operator reads month and day names, so they are Romanian by default.</summary>
    Friend Const DefaultCultureName As String = "ro-RO"

    ' ── What is on the page ──────────────────────────────────────────────────────
    Private _showToday As Boolean = True
    Private _showWeekNumbers As Boolean = False
    Private _showTrailingDays As Boolean = True
    Private _highlightWeekend As Boolean = True

    ' ── Metrics (logical px @96dpi) ──────────────────────────────────────────────
    Private _headerHeight As Integer = 32
    Private _dayNamesHeight As Integer = 22
    Private _footerHeight As Integer = 28
    Private _borderWidth As Integer = 1
    Private _cornerRadius As Integer = -1
    Private _cellCornerRadius As Integer = -1
    Private _cellGradient As Integer = 0

    ' ── The air, band by band (logical px @96dpi). The inherited Padding is the outer one. ──
    Private _headerPadding As Padding = New Padding(0)
    Private _gridPadding As Padding = New Padding(0)
    Private _cellPadding As Padding = New Padding(2)
    Private _footerPadding As Padding = New Padding(0)

    ' ── "Auto" colours: the light look, so an untouched control is readable in the
    '    Visual Studio designer and on the bench, before any scheme is applied ─────
    Private _autoBack As Color = Color.White
    Private _autoFore As Color = Color.FromArgb(30, 30, 30)
    Private _autoBorder As Color = Color.FromArgb(170, 170, 170)
    Private _autoHeaderBack As Color = Color.FromArgb(240, 240, 240)
    Private _autoHeaderFore As Color = Color.FromArgb(30, 30, 30)
    Private _autoArrow As Color = Color.FromArgb(90, 90, 90)
    Private _autoDayName As Color = Color.FromArgb(115, 115, 115)
    Private _autoWeekNumber As Color = Color.FromArgb(150, 150, 150)
    Private _autoTrailing As Color = Color.FromArgb(160, 160, 160)
    Private _autoWeekend As Color = Color.FromArgb(180, 40, 40)
    Private _autoSelBack As Color = Color.FromArgb(0, 122, 204)
    Private _autoSelFore As Color = Color.White
    Private _autoHover As Color = Color.FromArgb(232, 241, 251)
    Private _autoToday As Color = Color.FromArgb(0, 122, 204)
    Private _autoGrid As Color = Color.FromArgb(224, 224, 224)
    Private _autoFooter As Color = Color.FromArgb(0, 122, 204)
    Private _autoDisabled As Color = Color.FromArgb(160, 160, 160)

    ' ── Chosen colours (Empty = from the theme) ──────────────────────────────────
    Private _borderColor As Color = Color.Empty
    Private _headerBackColor As Color = Color.Empty
    Private _headerForeColor As Color = Color.Empty
    Private _arrowColor As Color = Color.Empty
    Private _dayNameColor As Color = Color.Empty
    Private _weekNumberColor As Color = Color.Empty
    Private _trailingForeColor As Color = Color.Empty
    Private _weekendForeColor As Color = Color.Empty
    Private _selectionBackColor As Color = Color.Empty
    Private _selectionForeColor As Color = Color.Empty
    Private _hoverColor As Color = Color.Empty
    Private _todayColor As Color = Color.Empty
    Private _gridColor As Color = Color.Empty
    Private _footerForeColor As Color = Color.Empty

    ' "The operator pinned this" flags — see ShouldSerializeBackColor.
    Private _backColorPinned As Boolean
    Private _foreColorPinned As Boolean
    Private _fontPinned As Boolean

    ' ── Runtime state (never serialized) ─────────────────────────────────────────
    Private _hot As Integer = HitNone
    Private _pressed As Integer = HitNone

    ''' <summary>Fires on every move of the value, arrow keys included.</summary>
    Public Event ValueChanged As EventHandler

    ''' <summary>Fires only on a real choice: a click on a day, Enter, or the "today" row.</summary>
    Public Event DateSelected As EventHandler(Of KBotDateSelectedEventArgs)

    ''' <summary>Fires when the zoom level changes (days / months / years).</summary>
    Public Event ViewChanged As EventHandler

    ''' <summary>Fires when the page moves to another month, year or decade.</summary>
    Public Event DisplayMonthChanged As EventHandler

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable, True)
        TabStop = True
    End Sub

    ''' <summary>
    ''' The size a freshly dropped calendar starts at. Overridden rather than assigned in the
    ''' constructor: <c>Control.ShouldSerializeSize</c> compares against THIS, so a control the
    ''' operator has not resized writes no <c>Size</c> line into the host's .Designer.vb (C4).
    ''' </summary>
    Protected Overrides ReadOnly Property DefaultSize As Size
        Get
            Return New Size(240, 220)
        End Get
    End Property

    ' =====================================================================
    ' INHERITED PROPERTIES WITH A PIN FLAG
    ' =====================================================================

    <Category("K-BOT Calendar Colors")>
    <Description("The grid background; not pinned here, it follows the theme.")>
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
    ''' would write a <c>cal.BackColor = …</c> nobody chose into the host form, and on reload that
    ''' line would run through the setter above and pin the colour forever (C4).
    ''' </summary>
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColorPinned
    End Function

    ' The flag is cleared AFTER the colour is written: ResetBackColor goes through the VIRTUAL
    ' setter, that is, through ours, which would light it again.
    Public Overrides Sub ResetBackColor()
        MyBase.BackColor = _autoBack
        _backColorPinned = False
        Invalidate()
    End Sub

    <Category("K-BOT Calendar Colors")>
    <Description("The day number colour; not pinned here, it follows the theme.")>
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

    Public Function ShouldSerializeForeColor() As Boolean
        Return _foreColorPinned
    End Function

    Public Overrides Sub ResetForeColor()
        MyBase.ForeColor = _autoFore
        _foreColorPinned = False
        Invalidate()
    End Sub

    <Category("K-BOT Calendar")>
    <Description("The control font; not pinned here, it follows the ambient font of the scheme.")>
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
        InvalidateLayout()
    End Sub

    ' =====================================================================
    ' VALUE AND PAGE
    ' =====================================================================

    ''' <summary>
    ''' The selected day (time stripped). Out-of-range values are CLAMPED to
    ''' <see cref="MinDate"/>/<see cref="MaxDate"/> (C3), and writing it brings its month onto the
    ''' page — a value nobody can see is a value nobody can correct.
    ''' </summary>
    <Category("K-BOT Calendar")>
    <Description("The selected day. Clamped to MinDate/MaxDate; setting it pages to that month.")>
    Public Property Value As Date
        Get
            Return _value
        End Get
        Set(value As Date)
            SetValueCore(value, raiseChanged:=True, page:=True)
        End Set
    End Property

    ''' <summary>Fixed by the code, not by the operator: it is state, not authoring.</summary>
    Public Function ShouldSerializeValue() As Boolean
        Return False
    End Function

    ''' <summary>The first day of the month currently on the page.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property DisplayMonth As Date
        Get
            Return _displayMonth
        End Get
        Set(value As Date)
            Dim nou As New Date(value.Year, value.Month, 1)
            If nou = _displayMonth Then Return
            _displayMonth = nou
            InvalidateLayout()
            RaiseEvent DisplayMonthChanged(Me, EventArgs.Empty)
        End Set
    End Property

    ''' <summary>Days, months or years — the zoom level of the page.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property View As KBotCalendarView
        Get
            Return _view
        End Get
        Set(value As KBotCalendarView)
            If Not [Enum].IsDefined(GetType(KBotCalendarView), value) Then
                Throw New ArgumentException("Nivel de calendar necunoscut.", NameOf(value))
            End If
            If value = _view Then Return
            _view = value
            _hot = HitNone
            InvalidateLayout()
            RaiseEvent ViewChanged(Me, EventArgs.Empty)
        End Set
    End Property

    ''' <summary>Earliest day the operator may reach. Anything past <see cref="MaxDate"/> throws.</summary>
    <Category("K-BOT Calendar")>
    <Description("Earliest selectable day.")>
    Public Property MinDate As Date
        Get
            Return _minDate
        End Get
        Set(value As Date)
            If value.Date > _maxDate Then
                Throw New ArgumentException("MinDate nu poate depăși MaxDate.", NameOf(value))
            End If
            _minDate = value.Date
            SetValueCore(_value, raiseChanged:=True, page:=False)
            InvalidateLayout()
        End Set
    End Property

    Public Function ShouldSerializeMinDate() As Boolean
        Return _minDate <> New Date(1900, 1, 1)
    End Function

    Public Sub ResetMinDate()
        MinDate = New Date(1900, 1, 1)
    End Sub

    ''' <summary>Latest day the operator may reach. Anything before <see cref="MinDate"/> throws.</summary>
    <Category("K-BOT Calendar")>
    <Description("Latest selectable day.")>
    Public Property MaxDate As Date
        Get
            Return _maxDate
        End Get
        Set(value As Date)
            If value.Date < _minDate Then
                Throw New ArgumentException("MaxDate nu poate fi înaintea lui MinDate.", NameOf(value))
            End If
            _maxDate = value.Date
            SetValueCore(_value, raiseChanged:=True, page:=False)
            InvalidateLayout()
        End Set
    End Property

    Public Function ShouldSerializeMaxDate() As Boolean
        Return _maxDate <> New Date(2100, 12, 31)
    End Function

    Public Sub ResetMaxDate()
        MaxDate = New Date(2100, 12, 31)
    End Sub

    ' =====================================================================
    ' WORDING
    ' =====================================================================

    ''' <summary>
    ''' The culture the month and day names are read from. Defaults to <c>ro-RO</c> — NOT to the
    ''' machine culture: the operator must get Romanian months on any workstation. An unknown name
    ''' throws (C3).
    ''' </summary>
    <Category("K-BOT Calendar")>
    <Description("Culture of the month and day names. Default ro-RO.")>
    <DefaultValue(DefaultCultureName)>
    Public Property CultureName As String
        Get
            Return _cultureName
        End Get
        Set(value As String)
            Dim nume As String = If(value, String.Empty).Trim()
            If nume.Length = 0 Then
                Throw New ArgumentException("Numele culturii nu poate fi gol.", NameOf(value))
            End If
            Try
                _culture = CultureInfo.GetCultureInfo(nume)
            Catch ex As CultureNotFoundException
                GlobalErrorLog.Write("KBotCalendar.CultureName", ex)
                Throw New ArgumentException("Cultură necunoscută: " & nume, NameOf(value), ex)
            End Try
            _cultureName = nume
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>The resolved culture behind <see cref="CultureName"/>.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Culture As CultureInfo
        Get
            Return _culture
        End Get
    End Property

    ''' <summary>Which column the week starts in. Monday here, as the operator writes weeks.</summary>
    <Category("K-BOT Calendar")>
    <Description("First column of the week. Monday by default.")>
    <DefaultValue(GetType(DayOfWeek), "Monday")>
    Public Property FirstDayOfWeek As DayOfWeek
        Get
            Return _firstDayOfWeek
        End Get
        Set(value As DayOfWeek)
            If Not [Enum].IsDefined(GetType(DayOfWeek), value) Then
                Throw New ArgumentException("Zi a săptămânii necunoscută.", NameOf(value))
            End If
            _firstDayOfWeek = value
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>How the date on the "today" row is written.</summary>
    <Category("K-BOT Calendar")>
    <Description("Date format of the today row.")>
    <DefaultValue("dd.MM.yyyy")>
    Public Property TodayFormat As String
        Get
            Return _todayFormat
        End Get
        Set(value As String)
            Dim f As String = If(value, String.Empty).Trim()
            If f.Length = 0 Then
                Throw New ArgumentException("Formatul datei nu poate fi gol.", NameOf(value))
            End If
            _todayFormat = f
            Invalidate()
        End Set
    End Property

    ' =====================================================================
    ' WHAT IS ON THE PAGE
    ' =====================================================================

    <Category("K-BOT Calendar")>
    <Description("Show the today row at the bottom.")>
    <DefaultValue(True)>
    Public Property ShowToday As Boolean
        Get
            Return _showToday
        End Get
        Set(value As Boolean)
            _showToday = value
            InvalidateLayout()
        End Set
    End Property

    <Category("K-BOT Calendar")>
    <Description("Show the ISO week number in a left-hand column.")>
    <DefaultValue(False)>
    Public Property ShowWeekNumbers As Boolean
        Get
            Return _showWeekNumbers
        End Get
        Set(value As Boolean)
            _showWeekNumbers = value
            InvalidateLayout()
        End Set
    End Property

    <Category("K-BOT Calendar")>
    <Description("Draw the days of the neighbouring months in the empty cells.")>
    <DefaultValue(True)>
    Public Property ShowTrailingDays As Boolean
        Get
            Return _showTrailingDays
        End Get
        Set(value As Boolean)
            _showTrailingDays = value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Calendar")>
    <Description("Saturday and Sunday get their own text colour.")>
    <DefaultValue(True)>
    Public Property HighlightWeekend As Boolean
        Get
            Return _highlightWeekend
        End Get
        Set(value As Boolean)
            _highlightWeekend = value
            Invalidate()
        End Set
    End Property

    ' =====================================================================
    ' METRICS (logical px @96dpi)
    ' =====================================================================

    <Category("K-BOT Calendar")>
    <Description("Height of the header band, px @96dpi.")>
    <DefaultValue(32)>
    Public Property HeaderHeight As Integer
        Get
            Return _headerHeight
        End Get
        Set(value As Integer)
            _headerHeight = Math.Max(0, value)
            InvalidateLayout()
        End Set
    End Property

    <Category("K-BOT Calendar")>
    <Description("Height of the day-name strip, px @96dpi. 0 hides it.")>
    <DefaultValue(22)>
    Public Property DayNamesHeight As Integer
        Get
            Return _dayNamesHeight
        End Get
        Set(value As Integer)
            _dayNamesHeight = Math.Max(0, value)
            InvalidateLayout()
        End Set
    End Property

    <Category("K-BOT Calendar")>
    <Description("Height of the today row, px @96dpi.")>
    <DefaultValue(28)>
    Public Property FooterHeight As Integer
        Get
            Return _footerHeight
        End Get
        Set(value As Integer)
            _footerHeight = Math.Max(0, value)
            InvalidateLayout()
        End Set
    End Property

    <Category("K-BOT Calendar")>
    <Description("Outer border thickness, px @96dpi. 0 = no border.")>
    <DefaultValue(1)>
    Public Property BorderWidth As Integer
        Get
            Return _borderWidth
        End Get
        Set(value As Integer)
            _borderWidth = Math.Max(0, value)
            InvalidateLayout()
        End Set
    End Property

    <Category("K-BOT Calendar")>
    <Description("Outer corner radius, px @96dpi. -1 = from the theme, 0 = square.")>
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

    <Category("K-BOT Calendar")>
    <Description("Corner radius of a day cell, px @96dpi. -1 = from the theme, 0 = square.")>
    <DefaultValue(-1)>
    Public Property CellCornerRadius As Integer
        Get
            Return _cellCornerRadius
        End Get
        Set(value As Integer)
            _cellCornerRadius = Math.Max(-1, value)
            Invalidate()
        End Set
    End Property

    ''' <summary>Vertical gradient on the selected/hovered cell, 0..100. 0 = flat fill.</summary>
    <Category("K-BOT Calendar")>
    <Description("Gradient strength on the filled cells, 0..100. 0 = flat.")>
    <DefaultValue(0)>
    Public Property CellGradient As Integer
        Get
            Return _cellGradient
        End Get
        Set(value As Integer)
            _cellGradient = Math.Max(0, Math.Min(100, value))
            Invalidate()
        End Set
    End Property

    ' =====================================================================
    ' THE AIR (logical px @96dpi, each side on its own)
    ' =====================================================================
    ' Five of them, nested outwards in: the inherited Padding holds the whole card off the border,
    ' then each band keeps its own. They are separate because they do different jobs — GridPadding
    ' moves the day names AND the cells together, so the columns stay lined up, while CellPadding
    ' shrinks the coloured tile inside one cell without moving anything.

    ' The outer one is the INHERITED Padding (the designer shows it under Layout). It is left
    ' inherited on purpose: shadowing it to move it into our category would cut it off from the
    ' framework's own ShouldSerializePadding, and a padding nobody set would start being written
    ' into the host's .Designer.vb (C4). It is honoured in EnsureLayout and in OnPaddingChanged.

    ''' <summary>The air inside the header band: it moves the arrows and the title, not the band.</summary>
    <Category("K-BOT Calendar")>
    <Description("Air inside the header band, px @96dpi.")>
    Public Property HeaderPadding As Padding
        Get
            Return _headerPadding
        End Get
        Set(value As Padding)
            _headerPadding = ClampPad(value)
            InvalidateLayout()
        End Set
    End Property
    Public Function ShouldSerializeHeaderPadding() As Boolean
        Return _headerPadding <> New Padding(0)
    End Function
    Public Sub ResetHeaderPadding()
        HeaderPadding = New Padding(0)
    End Sub

    ''' <summary>
    ''' The air around the grid — the day-name strip and the cells together, so the headings stay
    ''' over their columns. This is the one to reach for when the days sit too tight to the border.
    ''' </summary>
    <Category("K-BOT Calendar")>
    <Description("Air around the day-name strip and the cells together, px @96dpi.")>
    Public Property GridPadding As Padding
        Get
            Return _gridPadding
        End Get
        Set(value As Padding)
            _gridPadding = ClampPad(value)
            InvalidateLayout()
        End Set
    End Property
    Public Function ShouldSerializeGridPadding() As Boolean
        Return _gridPadding <> New Padding(0)
    End Function
    Public Sub ResetGridPadding()
        GridPadding = New Padding(0)
    End Sub

    ''' <summary>
    ''' The air inside ONE cell: the gap between the cell and its coloured tile (selection, hover,
    ''' the ring around today). The number stays where it is — it is centred on the whole cell.
    ''' </summary>
    <Category("K-BOT Calendar")>
    <Description("Air between a cell and its filled tile, px @96dpi.")>
    Public Property CellPadding As Padding
        Get
            Return _cellPadding
        End Get
        Set(value As Padding)
            _cellPadding = ClampPad(value)
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeCellPadding() As Boolean
        Return _cellPadding <> New Padding(2)
    End Function
    Public Sub ResetCellPadding()
        CellPadding = New Padding(2)
    End Sub

    ''' <summary>The air inside the today row: it moves the text, not the band or its hover fill.</summary>
    <Category("K-BOT Calendar")>
    <Description("Air inside the today row, px @96dpi.")>
    Public Property FooterPadding As Padding
        Get
            Return _footerPadding
        End Get
        Set(value As Padding)
            _footerPadding = ClampPad(value)
            InvalidateLayout()
        End Set
    End Property
    Public Function ShouldSerializeFooterPadding() As Boolean
        Return _footerPadding <> New Padding(0)
    End Function
    Public Sub ResetFooterPadding()
        FooterPadding = New Padding(0)
    End Sub

    ' Negative air is not air: C3 says clamp a number, not throw for it.
    Private Shared Function ClampPad(p As Padding) As Padding
        Return New Padding(Math.Max(0, p.Left), Math.Max(0, p.Top),
                           Math.Max(0, p.Right), Math.Max(0, p.Bottom))
    End Function

    ''' <summary>The inherited <c>Padding</c> is real air here, so a change to it must relayout.</summary>
    Protected Overrides Sub OnPaddingChanged(e As EventArgs)
        MyBase.OnPaddingChanged(e)
        InvalidateLayout()
    End Sub

    ' =====================================================================
    ' OWN COLOURS (Color.Empty = from the theme)
    ' =====================================================================

    <Category("K-BOT Calendar Colors")> <Description("Outer border. Empty = from the theme.")>
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

    <Category("K-BOT Calendar Colors")> <Description("Header band background. Empty = from the theme.")>
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

    <Category("K-BOT Calendar Colors")> <Description("Header title text. Empty = from the theme.")>
    Public Property HeaderForeColor As Color
        Get
            Return _headerForeColor
        End Get
        Set(value As Color)
            _headerForeColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeHeaderForeColor() As Boolean
        Return _headerForeColor <> Color.Empty
    End Function
    Public Sub ResetHeaderForeColor()
        HeaderForeColor = Color.Empty
    End Sub

    <Category("K-BOT Calendar Colors")> <Description("Paging arrows. Empty = from the theme.")>
    Public Property ArrowColor As Color
        Get
            Return _arrowColor
        End Get
        Set(value As Color)
            _arrowColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeArrowColor() As Boolean
        Return _arrowColor <> Color.Empty
    End Function
    Public Sub ResetArrowColor()
        ArrowColor = Color.Empty
    End Sub

    <Category("K-BOT Calendar Colors")> <Description("Day-name strip text. Empty = from the theme.")>
    Public Property DayNameColor As Color
        Get
            Return _dayNameColor
        End Get
        Set(value As Color)
            _dayNameColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeDayNameColor() As Boolean
        Return _dayNameColor <> Color.Empty
    End Function
    Public Sub ResetDayNameColor()
        DayNameColor = Color.Empty
    End Sub

    <Category("K-BOT Calendar Colors")> <Description("Week-number column. Empty = from the theme.")>
    Public Property WeekNumberColor As Color
        Get
            Return _weekNumberColor
        End Get
        Set(value As Color)
            _weekNumberColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeWeekNumberColor() As Boolean
        Return _weekNumberColor <> Color.Empty
    End Function
    Public Sub ResetWeekNumberColor()
        WeekNumberColor = Color.Empty
    End Sub

    <Category("K-BOT Calendar Colors")> <Description("Days of the neighbouring months. Empty = from the theme.")>
    Public Property TrailingForeColor As Color
        Get
            Return _trailingForeColor
        End Get
        Set(value As Color)
            _trailingForeColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeTrailingForeColor() As Boolean
        Return _trailingForeColor <> Color.Empty
    End Function
    Public Sub ResetTrailingForeColor()
        TrailingForeColor = Color.Empty
    End Sub

    <Category("K-BOT Calendar Colors")> <Description("Saturday and Sunday. Empty = from the theme.")>
    Public Property WeekendForeColor As Color
        Get
            Return _weekendForeColor
        End Get
        Set(value As Color)
            _weekendForeColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeWeekendForeColor() As Boolean
        Return _weekendForeColor <> Color.Empty
    End Function
    Public Sub ResetWeekendForeColor()
        WeekendForeColor = Color.Empty
    End Sub

    <Category("K-BOT Calendar Colors")> <Description("Selected cell background. Empty = from the theme.")>
    Public Property SelectionBackColor As Color
        Get
            Return _selectionBackColor
        End Get
        Set(value As Color)
            _selectionBackColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSelectionBackColor() As Boolean
        Return _selectionBackColor <> Color.Empty
    End Function
    Public Sub ResetSelectionBackColor()
        SelectionBackColor = Color.Empty
    End Sub

    <Category("K-BOT Calendar Colors")> <Description("Selected cell text. Empty = from the theme.")>
    Public Property SelectionForeColor As Color
        Get
            Return _selectionForeColor
        End Get
        Set(value As Color)
            _selectionForeColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSelectionForeColor() As Boolean
        Return _selectionForeColor <> Color.Empty
    End Function
    Public Sub ResetSelectionForeColor()
        SelectionForeColor = Color.Empty
    End Sub

    <Category("K-BOT Calendar Colors")> <Description("Cell under the pointer. Empty = from the theme.")>
    Public Property HoverColor As Color
        Get
            Return _hoverColor
        End Get
        Set(value As Color)
            _hoverColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeHoverColor() As Boolean
        Return _hoverColor <> Color.Empty
    End Function
    Public Sub ResetHoverColor()
        HoverColor = Color.Empty
    End Sub

    <Category("K-BOT Calendar Colors")> <Description("Ring around today. Empty = from the theme.")>
    Public Property TodayColor As Color
        Get
            Return _todayColor
        End Get
        Set(value As Color)
            _todayColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeTodayColor() As Boolean
        Return _todayColor <> Color.Empty
    End Function
    Public Sub ResetTodayColor()
        TodayColor = Color.Empty
    End Sub

    <Category("K-BOT Calendar Colors")> <Description("Separator lines. Empty = from the theme.")>
    Public Property GridColor As Color
        Get
            Return _gridColor
        End Get
        Set(value As Color)
            _gridColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeGridColor() As Boolean
        Return _gridColor <> Color.Empty
    End Function
    Public Sub ResetGridColor()
        GridColor = Color.Empty
    End Sub

    <Category("K-BOT Calendar Colors")> <Description("Today row text. Empty = from the theme.")>
    Public Property FooterForeColor As Color
        Get
            Return _footerForeColor
        End Get
        Set(value As Color)
            _footerForeColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterForeColor() As Boolean
        Return _footerForeColor <> Color.Empty
    End Function
    Public Sub ResetFooterForeColor()
        FooterForeColor = Color.Empty
    End Sub

    ' ── What is actually painted ────────────────────────────────────────────────
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveBorderColor As Color
        Get
            Return If(_borderColor = Color.Empty, _autoBorder, _borderColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveHeaderBackColor As Color
        Get
            Return If(_headerBackColor = Color.Empty, _autoHeaderBack, _headerBackColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveHeaderForeColor As Color
        Get
            Return If(_headerForeColor = Color.Empty, _autoHeaderFore, _headerForeColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveArrowColor As Color
        Get
            Return If(_arrowColor = Color.Empty, _autoArrow, _arrowColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveDayNameColor As Color
        Get
            Return If(_dayNameColor = Color.Empty, _autoDayName, _dayNameColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveWeekNumberColor As Color
        Get
            Return If(_weekNumberColor = Color.Empty, _autoWeekNumber, _weekNumberColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveTrailingForeColor As Color
        Get
            Return If(_trailingForeColor = Color.Empty, _autoTrailing, _trailingForeColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveWeekendForeColor As Color
        Get
            Return If(_weekendForeColor = Color.Empty, _autoWeekend, _weekendForeColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveSelectionBackColor As Color
        Get
            Return If(_selectionBackColor = Color.Empty, _autoSelBack, _selectionBackColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveSelectionForeColor As Color
        Get
            Return If(_selectionForeColor = Color.Empty, _autoSelFore, _selectionForeColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveHoverColor As Color
        Get
            Return If(_hoverColor = Color.Empty, _autoHover, _hoverColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveTodayColor As Color
        Get
            Return If(_todayColor = Color.Empty, _autoToday, _todayColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveGridColor As Color
        Get
            Return If(_gridColor = Color.Empty, _autoGrid, _gridColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveFooterForeColor As Color
        Get
            Return If(_footerForeColor = Color.Empty, _autoFooter, _footerForeColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveDisabledColor As Color
        Get
            Return _autoDisabled
        End Get
    End Property

    ' =====================================================================
    ' THEME
    ' =====================================================================

    ''' <summary>Re-reads the scheme. Colours pinned in the designer are left alone (C1/C5).</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            _autoBack = p.InputBackColor
            _autoFore = p.InputTextColor
            _autoBorder = p.InputBorderColor
            _autoHeaderBack = p.SurfaceColor
            _autoHeaderFore = p.TextColor
            _autoArrow = p.TextDimColor
            _autoDayName = p.TextDimColor
            _autoWeekNumber = ThemeShapes.Blend(p.TextDimColor, p.InputBackColor, 0.25)
            _autoTrailing = p.DisabledTextColor
            _autoDisabled = p.DisabledTextColor
            ' The weekend colour is DERIVED from the palette's error red, not invented: pulled a
            ' quarter of the way towards the grid background so a whole column of it does not read
            ' as a whole column of errors.
            _autoWeekend = ThemeShapes.Blend(p.ErrorColor, p.InputBackColor, 0.25)
            _autoSelBack = p.AccentColor
            _autoSelFore = p.AccentTextColor
            _autoHover = p.ButtonHoverColor
            _autoToday = p.AccentColor
            _autoGrid = ThemeShapes.Blend(p.BorderColor, p.InputBackColor, 0.5)
            _autoFooter = p.AccentColor

            ' MyBase, not Me: writing the theme must never pass for a choice of the operator.
            If Not _backColorPinned Then MyBase.BackColor = _autoBack
            If Not _foreColorPinned Then MyBase.ForeColor = _autoFore

            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCalendar.ApplyTheme", ex)
        End Try
    End Sub

    ' =====================================================================
    ' COMMANDS
    ' =====================================================================

    ''' <summary>Moves the page by <paramref name="delta"/> units: months, years or decades.</summary>
    Public Sub StepPage(delta As Integer)
        Try
            If delta = 0 Then Return
            Select Case _view
                Case KBotCalendarView.Days
                    DisplayMonth = _displayMonth.AddMonths(delta)
                Case KBotCalendarView.Months
                    DisplayMonth = _displayMonth.AddYears(delta)
                Case Else
                    DisplayMonth = _displayMonth.AddYears(delta * 10)
            End Select
        Catch ex As ArgumentOutOfRangeException
            ' Paging past Date.MinValue/MaxValue: the page simply stays where it is.
            GlobalErrorLog.Write("KBotCalendar.StepPage", ex)
        End Try
    End Sub

    ''' <summary>Zooms out one level. On <see cref="KBotCalendarView.Years"/> there is nowhere to go.</summary>
    Public Sub ZoomOut()
        Select Case _view
            Case KBotCalendarView.Days
                View = KBotCalendarView.Months
            Case KBotCalendarView.Months
                View = KBotCalendarView.Years
        End Select
    End Sub

    ''' <summary>Selects today and reports it as a real choice. Refused when today is out of range (C3).</summary>
    Public Sub GoToToday()
        If Date.Today < _minDate OrElse Date.Today > _maxDate Then
            Throw New InvalidOperationException("Ziua de azi este în afara intervalului MinDate..MaxDate.")
        End If
        View = KBotCalendarView.Days
        SetValueCore(Date.Today, raiseChanged:=True, page:=True)
        RaiseEvent DateSelected(Me, New KBotDateSelectedEventArgs(_value))
    End Sub

    ''' <summary>The one writer of <c>_value</c>: clamps, pages, invalidates, reports.</summary>
    Private Sub SetValueCore(candidate As Date, raiseChanged As Boolean, page As Boolean)
        Dim v As Date = candidate.Date
        If v < _minDate Then v = _minDate
        If v > _maxDate Then v = _maxDate

        Dim schimbat As Boolean = (v <> _value)
        _value = v
        If page Then DisplayMonth = New Date(v.Year, v.Month, 1)
        InvalidateLayout()
        If schimbat AndAlso raiseChanged Then RaiseEvent ValueChanged(Me, EventArgs.Empty)
    End Sub

    ''' <summary>A cell was activated: zoom in, or — in the day view — choose.</summary>
    Private Sub ActivateCell(index As Integer)
        If index < 0 OrElse index >= _cellCount Then Return
        If Not _cellEnabled(index) Then Return

        Select Case _view
            Case KBotCalendarView.Days
                SetValueCore(_cellDate(index), raiseChanged:=True, page:=True)
                RaiseEvent DateSelected(Me, New KBotDateSelectedEventArgs(_value))
            Case KBotCalendarView.Months
                DisplayMonth = _cellDate(index)
                View = KBotCalendarView.Days
            Case Else
                DisplayMonth = New Date(_cellDate(index).Year, _displayMonth.Month, 1)
                View = KBotCalendarView.Months
        End Select
    End Sub

    ' =====================================================================
    ' MOUSE
    ' =====================================================================

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            Dim h As Integer = HitTest(e.Location)
            If h <> _hot Then
                _hot = h
                Invalidate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCalendar.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hot <> HitNone Then
            _hot = HitNone
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Focus()
            _pressed = HitTest(e.Location)
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCalendar.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Dim pressed As Integer = _pressed
            _pressed = HitNone
            Dim h As Integer = HitTest(e.Location)
            Invalidate()
            If h <> pressed Then Return

            Select Case h
                Case HitPrev : StepPage(-1)
                Case HitNext : StepPage(1)
                Case HitTitle : ZoomOut()
                Case HitToday : GoToToday()
                Case HitNone
                Case Else : ActivateCell(h)
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCalendar.OnMouseUp", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Try
            If e.Delta = 0 Then Return
            StepPage(If(e.Delta > 0, -1, 1))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCalendar.OnMouseWheel", ex)
        End Try
    End Sub

    ' =====================================================================
    ' KEYBOARD
    ' =====================================================================

    ''' <summary>The arrows are OURS: without this WinForms hands them to the container.</summary>
    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        Select Case keyData And Keys.KeyCode
            Case Keys.Left, Keys.Right, Keys.Up, Keys.Down,
                 Keys.PageUp, Keys.PageDown, Keys.Home, Keys.End, Keys.Enter
                Return True
        End Select
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            Dim pas As Integer = If(_view = KBotCalendarView.Days, 7, 4)
            Select Case e.KeyCode
                Case Keys.Left : MoveBy(-1) : e.Handled = True
                Case Keys.Right : MoveBy(1) : e.Handled = True
                Case Keys.Up : MoveBy(-pas) : e.Handled = True
                Case Keys.Down : MoveBy(pas) : e.Handled = True
                Case Keys.PageUp : StepPage(-1) : e.Handled = True
                Case Keys.PageDown : StepPage(1) : e.Handled = True
                Case Keys.Home
                    SetValueCore(New Date(_value.Year, _value.Month, 1), True, True)
                    e.Handled = True
                Case Keys.End
                    SetValueCore(New Date(_value.Year, _value.Month,
                                          Date.DaysInMonth(_value.Year, _value.Month)), True, True)
                    e.Handled = True
                Case Keys.Enter, Keys.Space
                    KeyActivate()
                    e.Handled = True
                Case Keys.Back
                    ZoomOut()
                    e.Handled = True
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCalendar.OnKeyDown", ex)
        End Try
    End Sub

    ' One step of the cursor, in the unit of the current view.
    Private Sub MoveBy(steps As Integer)
        Try
            Select Case _view
                Case KBotCalendarView.Days
                    SetValueCore(_value.AddDays(steps), True, True)
                Case KBotCalendarView.Months
                    SetValueCore(_value.AddMonths(steps), True, True)
                Case Else
                    SetValueCore(_value.AddYears(steps), True, True)
            End Select
        Catch ex As ArgumentOutOfRangeException
            ' Walked off Date.MinValue/MaxValue — the cursor stays where it is.
            GlobalErrorLog.Write("KBotCalendar.MoveBy", ex)
        End Try
    End Sub

    ' Enter: in the day view that is a choice; higher up it zooms one level in.
    Private Sub KeyActivate()
        Select Case _view
            Case KBotCalendarView.Days
                RaiseEvent DateSelected(Me, New KBotDateSelectedEventArgs(_value))
            Case KBotCalendarView.Months
                DisplayMonth = New Date(_value.Year, _value.Month, 1)
                View = KBotCalendarView.Days
            Case Else
                DisplayMonth = New Date(_value.Year, _displayMonth.Month, 1)
                View = KBotCalendarView.Months
        End Select
    End Sub

    ' =====================================================================
    ' STATE THAT FORCES A REPAINT
    ' =====================================================================

    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnLostFocus(e As EventArgs)
        MyBase.OnLostFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnEnabledChanged(e As EventArgs)
        MyBase.OnEnabledChanged(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        InvalidateLayout()
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        InvalidateLayout()
    End Sub

    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        InvalidateLayout()
    End Sub

End Class
