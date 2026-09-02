Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The date field: a text box you can type into, with a drawn calendar button on the right that
''' drops a <see cref="KBotCalendar"/>. Same visual language as the calendar it opens — the same
''' rounded outline, the same input colours, the same focus ring — so field and calendar read as
''' one control, not two.
'''
''' <para><b>Why not <c>DateTimePicker</c>. It cannot be made taller.</b> The stock control
''' overrides its own bounds and snaps the height back to whatever the system says a combo box is,
''' so it can never line up with a taller row, a taller neighbour field, or a stretched form. This
''' one is a plain <c>Control</c> that never touches its own bounds: <b>set <c>Height</c> to
''' anything, or dock it, and it fills what it was given</b> — the outline stretches, the text
''' stays vertically centred, and the button grows with the field. On top of that, the stock
''' control is a native window: its face keeps the system colours on a dark scheme, exactly like
''' <c>ComboBox</c> before <see cref="KBotComboBox"/>.</para>
'''
''' <para><b>Typing and picking are both first class.</b> The text is real and editable
''' (<c>dd.MM.yyyy</c> by default, plus the shorthands the operator actually types: <c>2.9.26</c>,
''' <c>02092026</c>, <c>2/9/2026</c>). It is read back on Enter and on leaving the field; text that
''' is not a date at all puts the last good value back, because a half-typed date is not a value
''' anybody can save. F4 or Alt+Down drops the calendar, Esc closes it.</para>
'''
''' <para><b>The empty field.</b> <see cref="AllowEmpty"/> is what an Access date column looks like
''' when nobody has filled it in: <see cref="HasValue"/> is then False and
''' <see cref="PlaceholderText"/> is what the operator sees. Without it, clearing the text is
''' refused and the previous date comes back.</para>
'''
''' <para><b>It carries the time of day too.</b> <see cref="Value"/> is a full <c>Date</c>, not a
''' day: give <see cref="Format"/> a time part (<c>dd.MM.yyyy HH:mm</c>) and the field shows and
''' reads back the hour. The rule that keeps that honest: <b>the field never destroys a part of the
''' value it does not display</b> — with a date-only format, picking a day or retyping the date
''' keeps the hour that is already in the value, instead of quietly zeroing it.</para>
'''
''' <para><b>The air is all yours.</b> <see cref="Padding"/> insets everything inside the outline,
''' <see cref="TextPadding"/> is the air around the text, <see cref="ButtonPadding"/> the air around
''' the drawn calendar, and <see cref="GlyphSize"/> how big that calendar is — all four editable in
''' the designer, all in logical px @96dpi (C2).</para>
'''
''' <para>Colours follow the house contract (C1), pixel metrics are logical px @96dpi (C2), and
''' the control themes itself — it owns a child <c>TextBox</c>, so it MUST (C5).</para>
''' </summary>
<ToolboxItem(True)>
<DefaultProperty("Value")>
<DefaultEvent("ValueChanged")>
Public NotInheritable Class KBotDatePicker
    Inherits Control
    Implements IThemedControl

    ''' <summary>
    ''' The shorthands the operator types, tried after <see cref="Format"/> itself. The ones that
    ''' carry an hour come FIRST: a date-only pattern refuses a text with a time in it, so trying
    ''' them the other way round would only waste the attempt, and a typed hour must survive.
    ''' </summary>
    Private Shared ReadOnly ExtraFormats As String() = {
        "dd.MM.yyyy HH:mm", "dd.MM.yyyy HH:mm:ss", "d.M.yyyy HH:mm", "d.M.yyyy HH:mm:ss",
        "dd/MM/yyyy HH:mm", "d/M/yyyy HH:mm",
        "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss",
        "dd.MM.yyyy", "d.M.yyyy", "dd.MM.yy", "d.M.yy",
        "dd/MM/yyyy", "d/M/yyyy", "dd/MM/yy", "d/M/yy",
        "dd-MM-yyyy", "d-M-yyyy", "ddMMyyyy", "ddMMyy",
        "yyyy-MM-dd"}

    ''' <summary>The range the field accepts when nobody narrows it. The last one is the LAST
    ''' INSTANT of that day, not its midnight: a value carries a time, so a max of midnight would
    ''' refuse every hour of the final day.</summary>
    Private Shared ReadOnly DefaultMinDate As Date = New Date(1900, 1, 1)
    Private Shared ReadOnly DefaultMaxDate As Date = New Date(2100, 12, 31, 23, 59, 59)

    Private ReadOnly _inner As New TextBox()

    ' ── Value ────────────────────────────────────────────────────────────────────
    Private _value As Date = Date.Today
    Private _hasValue As Boolean = True
    Private _allowEmpty As Boolean = False
    Private _minDate As Date = DefaultMinDate
    Private _maxDate As Date = DefaultMaxDate
    Private _format As String = "dd.MM.yyyy"
    Private _cultureName As String = KBotCalendar.DefaultCultureName
    Private _culture As CultureInfo = CultureInfo.GetCultureInfo(KBotCalendar.DefaultCultureName)

    ' ── What the drop-down calendar is told ──────────────────────────────────────
    Private _showToday As Boolean = True
    Private _showWeekNumbers As Boolean = False
    Private _firstDayOfWeek As DayOfWeek = DayOfWeek.Monday

    ' ── Metrics (logical px @96dpi) ──────────────────────────────────────────────
    Private _borderWidth As Integer = 1
    Private _cornerRadius As Integer = -1
    Private _buttonWidth As Integer = 28
    Private _textPadding As Padding = New Padding(8, 0, 8, 0)
    Private _buttonPadding As Padding = New Padding(6)
    Private _glyphSize As Integer = 14
    Private _showDropDownButton As Boolean = True
    Private _readOnlyText As Boolean = False

    ' ── "Auto" colours: the light look, so an untouched control is readable before any
    '    scheme has been applied (the designer surface, the bench) ─────────────────
    Private _autoBack As Color = Color.White
    Private _autoFore As Color = Color.FromArgb(30, 30, 30)
    Private _autoBorder As Color = Color.FromArgb(170, 170, 170)
    Private _autoFocus As Color = Color.FromArgb(0, 122, 204)
    Private _autoHover As Color = Color.FromArgb(232, 241, 251)
    Private _autoDisabled As Color = Color.FromArgb(160, 160, 160)

    Private _borderColor As Color = Color.Empty
    Private _focusBorderColor As Color = Color.Empty
    Private _hoverColor As Color = Color.Empty
    Private _glyphColor As Color = Color.Empty

    ' "The operator pinned this" flags — see ShouldSerializeBackColor.
    Private _backColorPinned As Boolean
    Private _foreColorPinned As Boolean
    Private _fontPinned As Boolean

    ' ── Runtime state (never serialized) ─────────────────────────────────────────
    Private _focused As Boolean = False
    Private _hoverButton As Boolean = False
    Private _popup As KBotCalendarPopup
    Private _writingText As Boolean = False

    ''' <summary>Raised when the date behind the field changes, however it changed.</summary>
    Public Event ValueChanged As EventHandler

    ''' <summary>Raised after the calendar window is on screen.</summary>
    Public Event DropDownOpened As EventHandler

    ''' <summary>Raised after the calendar window closes, picked or dismissed.</summary>
    Public Event DropDownClosed As EventHandler

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        ' The frame itself is never selectable: Tab lands straight on the inner box, exactly as in
        ' KBotTextField. Everything the frame needs from the keyboard it gets through the box.
        SetStyle(ControlStyles.Selectable, False)
        TabStop = False

        _inner.BorderStyle = BorderStyle.None
        _inner.Multiline = False
        _inner.AutoSize = False
        AddHandler _inner.Enter, AddressOf OnInnerEnter
        AddHandler _inner.Leave, AddressOf OnInnerLeave
        AddHandler _inner.KeyDown, AddressOf OnInnerKeyDown
        Controls.Add(_inner)

        WriteText()
    End Sub

    ''' <summary>The real edit control, for a host that needs to reach it (selection, IME, …).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property InnerTextBox As TextBox
        Get
            Return _inner
        End Get
    End Property

    ' =====================================================================
    ' INHERITED PROPERTIES WITH A PIN FLAG
    ' =====================================================================

    <Category("K-BOT Date Colors")>
    <Description("The field background; not pinned here, it follows the theme.")>
    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(value As Color)
            _backColorPinned = True
            MyBase.BackColor = value
            _inner.BackColor = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' CRITICAL. <c>Control.ShouldSerializeBackColor</c> answers True as soon as the property has
    ''' ever been WRITTEN — including by <see cref="ApplyTheme"/>. Without this pair Visual Studio
    ''' would freeze a colour nobody chose into the host form (C4).
    ''' </summary>
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColorPinned
    End Function

    Public Overrides Sub ResetBackColor()
        MyBase.BackColor = _autoBack
        _inner.BackColor = _autoBack
        _backColorPinned = False
        Invalidate()
    End Sub

    <Category("K-BOT Date Colors")>
    <Description("The text colour; not pinned here, it follows the theme.")>
    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(value As Color)
            _foreColorPinned = True
            MyBase.ForeColor = value
            _inner.ForeColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeForeColor() As Boolean
        Return _foreColorPinned
    End Function

    Public Overrides Sub ResetForeColor()
        MyBase.ForeColor = _autoFore
        _inner.ForeColor = _autoFore
        _foreColorPinned = False
        Invalidate()
    End Sub

    <Category("K-BOT Date")>
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
        PerformLayout()
        Invalidate()
    End Sub

    ''' <summary>
    ''' The whole point of this control: <b>the height is free</b>. Nothing here overrides
    ''' <c>SetBoundsCore</c>, which is what <c>DateTimePicker</c> does to snap itself back to the
    ''' system combo height. The default is only a starting size.
    ''' </summary>
    Protected Overrides ReadOnly Property DefaultSize As Size
        Get
            Return New Size(150, 28)
        End Get
    End Property

    ' =====================================================================
    ' VALUE
    ' =====================================================================

    ''' <summary>
    ''' The value in the field, <b>time of day included</b> — it is a full <c>Date</c> and nothing
    ''' here strips it, so a field with a time in its <see cref="Format"/> round-trips the hour.
    ''' Out-of-range values are CLAMPED to <see cref="MinDate"/>/<see cref="MaxDate"/> (C3). Writing
    ''' it always makes the field non-empty.
    ''' </summary>
    <Category("K-BOT Date")>
    <Description("The value in the field, time of day included. Clamped to MinDate/MaxDate.")>
    Public Property Value As Date
        Get
            Return _value
        End Get
        Set(value As Date)
            SetValueCore(value, hasValue:=True)
        End Set
    End Property

    ''' <summary>Runtime state, not authoring: it never goes into the host's .Designer.vb.</summary>
    Public Function ShouldSerializeValue() As Boolean
        Return False
    End Function

    ''' <summary>
    ''' False when the field is empty. Only reachable with <see cref="AllowEmpty"/>; setting it
    ''' False without that permission throws (C3), rather than quietly keeping the old date.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property HasValue As Boolean
        Get
            Return _hasValue
        End Get
        Set(value As Boolean)
            If value Then
                SetValueCore(_value, hasValue:=True)
            Else
                ClearValue()
            End If
        End Set
    End Property

    ''' <summary>Empties the field. Refused unless <see cref="AllowEmpty"/> is on.</summary>
    Public Sub ClearValue()
        If Not _allowEmpty Then
            Throw New InvalidOperationException(
                "Câmpul nu poate rămâne gol: AllowEmpty este False.")
        End If
        If Not _hasValue Then Return
        _hasValue = False
        WriteText()
        Invalidate()
        RaiseEvent ValueChanged(Me, EventArgs.Empty)
    End Sub

    ''' <summary>May the field be empty? That is what an unfilled Access date column looks like.</summary>
    <Category("K-BOT Date")>
    <Description("Allow the field to be empty (an unfilled date).")>
    <DefaultValue(False)>
    Public Property AllowEmpty As Boolean
        Get
            Return _allowEmpty
        End Get
        Set(value As Boolean)
            _allowEmpty = value
            If Not value AndAlso Not _hasValue Then
                _hasValue = True
                WriteText()
                Invalidate()
                RaiseEvent ValueChanged(Me, EventArgs.Empty)
            End If
        End Set
    End Property

    ''' <summary>What the empty field says. Operator-facing, therefore Romanian.</summary>
    <Category("K-BOT Date")>
    <Description("Text shown while the field is empty.")>
    <DefaultValue("")>
    Public Property PlaceholderText As String
        Get
            Return _inner.PlaceholderText
        End Get
        Set(value As String)
            _inner.PlaceholderText = If(value, String.Empty)
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' How the value is written in the field, and the first pattern the typed text is read back
    ''' with. Give it a time part — <c>dd.MM.yyyy HH:mm</c>, <c>dd.MM.yyyy HH:mm:ss</c> — and the
    ''' field becomes a date-time field: the hour is shown, typed and kept. Empty throws (C3).
    ''' </summary>
    <Category("K-BOT Date")>
    <Description("Format of the field, e.g. dd.MM.yyyy or dd.MM.yyyy HH:mm for a date-time value.")>
    <DefaultValue("dd.MM.yyyy")>
    Public Property Format As String
        Get
            Return _format
        End Get
        Set(value As String)
            Dim f As String = If(value, String.Empty).Trim()
            If f.Length = 0 Then
                Throw New ArgumentException("Formatul datei nu poate fi gol.", NameOf(value))
            End If
            _format = f
            WriteText()
            Invalidate()
        End Set
    End Property

    ''' <summary>Culture of the field and of the calendar it drops. Unknown names throw (C3).</summary>
    <Category("K-BOT Date")>
    <Description("Culture of the field and the drop-down calendar. Default ro-RO.")>
    <DefaultValue(KBotCalendar.DefaultCultureName)>
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
                GlobalErrorLog.Write("KBotDatePicker.CultureName", ex)
                Throw New ArgumentException("Cultură necunoscută: " & nume, NameOf(value), ex)
            End Try
            _cultureName = nume
            WriteText()
            Invalidate()
        End Set
    End Property

    ''' <summary>Earliest value the field accepts. Kept whole — a time here is honoured.</summary>
    <Category("K-BOT Date")>
    <Description("Earliest accepted value.")>
    Public Property MinDate As Date
        Get
            Return _minDate
        End Get
        Set(value As Date)
            If value > _maxDate Then
                Throw New ArgumentException("MinDate nu poate depăși MaxDate.", NameOf(value))
            End If
            _minDate = value
            SetValueCore(_value, _hasValue)
        End Set
    End Property

    Public Function ShouldSerializeMinDate() As Boolean
        Return _minDate <> DefaultMinDate
    End Function

    Public Sub ResetMinDate()
        MinDate = DefaultMinDate
    End Sub

    ''' <summary>Latest value the field accepts. Kept whole — a time here is honoured.</summary>
    <Category("K-BOT Date")>
    <Description("Latest accepted value.")>
    Public Property MaxDate As Date
        Get
            Return _maxDate
        End Get
        Set(value As Date)
            If value < _minDate Then
                Throw New ArgumentException("MaxDate nu poate fi înaintea lui MinDate.", NameOf(value))
            End If
            _maxDate = value
            SetValueCore(_value, _hasValue)
        End Set
    End Property

    Public Function ShouldSerializeMaxDate() As Boolean
        Return _maxDate <> DefaultMaxDate
    End Function

    Public Sub ResetMaxDate()
        MaxDate = DefaultMaxDate
    End Sub

    ' =====================================================================
    ' THE DROP-DOWN CALENDAR
    ' =====================================================================

    <Category("K-BOT Date")>
    <Description("Show the today row in the drop-down calendar.")>
    <DefaultValue(True)>
    Public Property ShowToday As Boolean
        Get
            Return _showToday
        End Get
        Set(value As Boolean)
            _showToday = value
        End Set
    End Property

    <Category("K-BOT Date")>
    <Description("Show ISO week numbers in the drop-down calendar.")>
    <DefaultValue(False)>
    Public Property ShowWeekNumbers As Boolean
        Get
            Return _showWeekNumbers
        End Get
        Set(value As Boolean)
            _showWeekNumbers = value
        End Set
    End Property

    <Category("K-BOT Date")>
    <Description("First column of the week in the drop-down calendar.")>
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
        End Set
    End Property

    <Category("K-BOT Date")>
    <Description("Show the calendar button on the right.")>
    <DefaultValue(True)>
    Public Property ShowDropDownButton As Boolean
        Get
            Return _showDropDownButton
        End Get
        Set(value As Boolean)
            _showDropDownButton = value
            PerformLayout()
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' The calendar inside the open drop-down, or <c>Nothing</c> while it is closed. This is where
    ''' a host reaches to set the calendar's own air or colours, from <see cref="DropDownOpened"/> —
    ''' the window is built on demand, so there is nothing to configure before it opens.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property DropDownCalendar As KBotCalendar
        Get
            If Not IsDropDownOpen Then Return Nothing
            Return _popup.Calendar
        End Get
    End Property

    ''' <summary>True while the calendar is on screen.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsDropDownOpen As Boolean
        Get
            Return _popup IsNot Nothing AndAlso Not _popup.IsDisposed
        End Get
    End Property

    ''' <summary>Drops the calendar under the field. A second call while it is open does nothing.</summary>
    Public Sub ShowDropDown()
        Try
            If IsDropDownOpen Then Return
            If Not Enabled Then Return

            Dim p As New KBotCalendarPopup()
            p.Calendar.CultureName = _cultureName
            p.Calendar.MinDate = _minDate
            p.Calendar.MaxDate = _maxDate
            p.Calendar.ShowToday = _showToday
            p.Calendar.ShowWeekNumbers = _showWeekNumbers
            p.Calendar.FirstDayOfWeek = _firstDayOfWeek
            p.Calendar.Value = If(_hasValue, _value, Date.Today)
            AddHandler p.DateCommitted, AddressOf OnPopupDateCommitted
            AddHandler p.FormClosed, AddressOf OnPopupClosed
            _popup = p

            ' Anchored on the WHOLE field, not on the button: the calendar should line up with the
            ' field's left edge the way a combo list lines up with its face.
            p.ShowBelow(Me, ClientRectangle)
            Invalidate()
            RaiseEvent DropDownOpened(Me, EventArgs.Empty)
        Catch ex As Exception
            _popup = Nothing
            GlobalErrorLog.Write("KBotDatePicker.ShowDropDown", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Closes the calendar if it is open. Closing a closed drop-down is not an error.</summary>
    Public Sub CloseDropDown()
        Try
            Dim p As KBotCalendarPopup = _popup
            If p Is Nothing OrElse p.IsDisposed Then Return
            p.Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDatePicker.CloseDropDown", ex)
        End Try
    End Sub

    Private Sub OnPopupDateCommitted(sender As Object, e As KBotDateSelectedEventArgs)
        Try
            ' The calendar hands back a DAY. The hour that was already in the field is not the
            ' calendar's to throw away, so it rides along.
            SetValueCore(e.Value.Date + CurrentTimeOfDay(), hasValue:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDatePicker.OnPopupDateCommitted", ex)
        End Try
    End Sub

    Private Sub OnPopupClosed(sender As Object, e As FormClosedEventArgs)
        Try
            Dim p As KBotCalendarPopup = TryCast(sender, KBotCalendarPopup)
            If p IsNot Nothing Then
                RemoveHandler p.DateCommitted, AddressOf OnPopupDateCommitted
                RemoveHandler p.FormClosed, AddressOf OnPopupClosed
            End If
            _popup = Nothing
            Invalidate()
            RaiseEvent DropDownClosed(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDatePicker.OnPopupClosed", ex)
        End Try
    End Sub

    ' =====================================================================
    ' TEXT: WRITING AND READING BACK
    ' =====================================================================

    ''' <summary>The text of the field. Reading it is the formatted date; writing it parses.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overrides Property Text As String
        Get
            Return _inner.Text
        End Get
        Set(value As String)
            _inner.Text = If(value, String.Empty)
            CommitText()
        End Set
    End Property

    ''' <summary>Typing is off: the field can then only be filled from the calendar.</summary>
    <Category("K-BOT Date")>
    <Description("The operator cannot type; the date comes only from the calendar.")>
    <DefaultValue(False)>
    Public Property ReadOnlyText As Boolean
        Get
            Return _readOnlyText
        End Get
        Set(value As Boolean)
            _readOnlyText = value
            _inner.ReadOnly = value
        End Set
    End Property

    ' Writes the value into the box. Guarded so our own writing is never read back as typing.
    Private Sub WriteText()
        Try
            _writingText = True
            _inner.Text = If(_hasValue, _value.ToString(_format, _culture), String.Empty)
        Catch ex As FormatException
            ' A format string the operator invented: log it and fall back to the house format,
            ' rather than leaving the field showing nothing.
            GlobalErrorLog.Write("KBotDatePicker.WriteText", ex)
            _inner.Text = If(_hasValue, _value.ToString("dd.MM.yyyy", _culture), String.Empty)
        Finally
            _writingText = False
        End Try
    End Sub

    ''' <summary>
    ''' Reads the typed text back into the value. Empty text empties the field when
    ''' <see cref="AllowEmpty"/> allows it; text that is not a date puts the last good value back.
    ''' </summary>
    Public Sub CommitText()
        Try
            If _writingText Then Return
            Dim brut As String = If(_inner.Text, String.Empty).Trim()

            If brut.Length = 0 Then
                If _allowEmpty Then
                    If _hasValue Then
                        _hasValue = False
                        WriteText()
                        Invalidate()
                        RaiseEvent ValueChanged(Me, EventArgs.Empty)
                    End If
                Else
                    WriteText()
                End If
                Return
            End If

            Dim parsed As Date
            If TryParseDate(brut, parsed) Then
                ' A format that shows no hour cannot have been used to type one, so a plain date
                ' typed over a date-time keeps the hour it could not see. When the format DOES show
                ' the time, what was typed is what is meant — midnight included.
                If parsed.TimeOfDay = TimeSpan.Zero AndAlso Not FormatHasTime(_format) Then
                    parsed = parsed.Date + CurrentTimeOfDay()
                End If
                SetValueCore(parsed, hasValue:=True)
                ' Even a value that did not change must be rewritten: "2.9.26" becomes "02.09.2026".
                WriteText()
            Else
                WriteText()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDatePicker.CommitText", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The parser, in one place: <see cref="Format"/> first, then the shorthands the operator
    ''' types, then the culture's own reading. Pure — it touches nothing on the control.
    ''' </summary>
    Friend Shared Function TryParseDate(text As String, format As String, culture As CultureInfo,
                                        ByRef result As Date) As Boolean
        result = Date.MinValue
        Dim brut As String = If(text, String.Empty).Trim()
        If brut.Length = 0 Then Return False
        Dim c As CultureInfo = If(culture, CultureInfo.InvariantCulture)

        If Not String.IsNullOrEmpty(format) AndAlso
           Date.TryParseExact(brut, format, c, DateTimeStyles.None, result) Then Return True

        For Each f As String In ExtraFormats
            If Date.TryParseExact(brut, f, c, DateTimeStyles.None, result) Then Return True
        Next

        If Date.TryParse(brut, c, DateTimeStyles.None, result) Then Return True
        Return Date.TryParse(brut, CultureInfo.InvariantCulture, DateTimeStyles.None, result)
    End Function

    Private Function TryParseDate(text As String, ByRef result As Date) As Boolean
        Return TryParseDate(text, _format, _culture, result)
    End Function

    ' The hour behind the field right now; midnight while the field is empty.
    Private Function CurrentTimeOfDay() As TimeSpan
        Return If(_hasValue, _value.TimeOfDay, TimeSpan.Zero)
    End Function

    ''' <summary>
    ''' Does this format string SHOW a time? That is the whole question behind "never destroy what
    ''' you do not display". Quoted runs and the escape character are skipped, so a literal such as
    ''' <c>dd.MM.yyyy 'ora'</c> is not mistaken for an hour. Pure — deliberately testable.
    ''' </summary>
    Friend Shared Function FormatHasTime(format As String) As Boolean
        If String.IsNullOrEmpty(format) Then Return False
        Dim i As Integer = 0
        While i < format.Length
            Dim c As Char = format(i)
            Select Case c
                Case "\"c
                    i += 2                          ' the escaped character is a literal
                    Continue While
                Case "'"c, """"c
                    Dim final As Integer = format.IndexOf(c, i + 1)
                    If final < 0 Then Return False  ' unterminated literal: nothing else counts
                    i = final + 1
                    Continue While
                Case "h"c, "H"c, "m"c, "s"c, "t"c, "f"c, "F"c
                    Return True
            End Select
            i += 1
        End While
        Return False
    End Function

    ' The one writer of the value: clamps, rewrites the text, reports. The time of day is NOT
    ' stripped here — that is what makes the field able to hold a date-time.
    Private Sub SetValueCore(candidate As Date, hasValue As Boolean)
        Dim v As Date = candidate
        If v < _minDate Then v = _minDate
        If v > _maxDate Then v = _maxDate

        Dim schimbat As Boolean = (v <> _value) OrElse (hasValue <> _hasValue)
        _value = v
        _hasValue = hasValue
        WriteText()
        Invalidate()
        If schimbat Then RaiseEvent ValueChanged(Me, EventArgs.Empty)
    End Sub

    ' =====================================================================
    ' METRICS AND OWN COLOURS
    ' =====================================================================

    <Category("K-BOT Date")>
    <Description("Outline thickness, px @96dpi. 0 = no outline.")>
    <DefaultValue(1)>
    Public Property BorderWidth As Integer
        Get
            Return _borderWidth
        End Get
        Set(value As Integer)
            _borderWidth = Math.Max(0, value)
            PerformLayout()
            Invalidate()
        End Set
    End Property

    <Category("K-BOT Date")>
    <Description("Corner radius, px @96dpi. -1 = from the theme, 0 = square.")>
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

    <Category("K-BOT Date")>
    <Description("Width of the calendar button, px @96dpi.")>
    <DefaultValue(28)>
    Public Property ButtonWidth As Integer
        Get
            Return _buttonWidth
        End Get
        Set(value As Integer)
            _buttonWidth = Math.Max(0, value)
            PerformLayout()
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' The air around the TEXT, each side on its own, px @96dpi. Left and right push the text away
    ''' from the outline and from the button; top and bottom squeeze the strip the one line of text
    ''' is centred in, which is how you sit the text high or low in a tall field.
    ''' </summary>
    <Category("K-BOT Date")>
    <Description("Air around the text: left, top, right, bottom, px @96dpi.")>
    Public Property TextPadding As Padding
        Get
            Return _textPadding
        End Get
        Set(value As Padding)
            _textPadding = Clamp(value)
            PerformLayout()
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeTextPadding() As Boolean
        Return _textPadding <> New Padding(8, 0, 8, 0)
    End Function
    Public Sub ResetTextPadding()
        TextPadding = New Padding(8, 0, 8, 0)
    End Sub

    ''' <summary>The air around the drawn calendar, inside the button strip, px @96dpi.</summary>
    <Category("K-BOT Date")>
    <Description("Air around the calendar glyph inside the button, px @96dpi.")>
    Public Property ButtonPadding As Padding
        Get
            Return _buttonPadding
        End Get
        Set(value As Padding)
            _buttonPadding = Clamp(value)
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeButtonPadding() As Boolean
        Return _buttonPadding <> New Padding(6)
    End Function
    Public Sub ResetButtonPadding()
        ButtonPadding = New Padding(6)
    End Sub

    ''' <summary>
    ''' How big the drawn calendar is, px @96dpi. <c>0</c> = as big as <see cref="ButtonPadding"/>
    ''' leaves room for, which is how you get a glyph that grows with a stretched field.
    ''' </summary>
    <Category("K-BOT Date")>
    <Description("Size of the calendar glyph, px @96dpi. 0 = fill what ButtonPadding leaves.")>
    <DefaultValue(14)>
    Public Property GlyphSize As Integer
        Get
            Return _glyphSize
        End Get
        Set(value As Integer)
            _glyphSize = Math.Max(0, value)
            Invalidate()
        End Set
    End Property

    ' Negative air is not air: C3 says clamp a number, not throw for it.
    Private Shared Function Clamp(p As Padding) As Padding
        Return New Padding(Math.Max(0, p.Left), Math.Max(0, p.Top),
                           Math.Max(0, p.Right), Math.Max(0, p.Bottom))
    End Function

    ' Logical px -> device px, one side at a time (C2).
    Private Function ScalePad(p As Padding) As Padding
        Return New Padding(ThemeShapes.ScaleDpi(Me, p.Left), ThemeShapes.ScaleDpi(Me, p.Top),
                           ThemeShapes.ScaleDpi(Me, p.Right), ThemeShapes.ScaleDpi(Me, p.Bottom))
    End Function

    ' A rectangle with the air taken out of it, never smaller than nothing.
    Private Shared Function Shrink(r As Rectangle, p As Padding) As Rectangle
        Return New Rectangle(r.Left + p.Left, r.Top + p.Top,
                             Math.Max(0, r.Width - p.Horizontal), Math.Max(0, r.Height - p.Vertical))
    End Function

    <Category("K-BOT Date Colors")> <Description("The outline. Empty = from the theme.")>
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

    <Category("K-BOT Date Colors")> <Description("The outline while the field has focus. Empty = from the theme.")>
    Public Property FocusBorderColor As Color
        Get
            Return _focusBorderColor
        End Get
        Set(value As Color)
            _focusBorderColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFocusBorderColor() As Boolean
        Return _focusBorderColor <> Color.Empty
    End Function
    Public Sub ResetFocusBorderColor()
        FocusBorderColor = Color.Empty
    End Sub

    <Category("K-BOT Date Colors")> <Description("The calendar button under the pointer. Empty = from the theme.")>
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

    ''' <summary>
    ''' The colour of the drawn calendar. <c>Empty</c> = derived from the colours the field is
    ''' ACTUALLY painted with, so it can never end up invisible: see
    ''' <see cref="EffectiveGlyphColor"/>.
    ''' </summary>
    <Category("K-BOT Date Colors")> <Description("The calendar drawn on the button. Empty = from the theme.")>
    Public Property GlyphColor As Color
        Get
            Return _glyphColor
        End Get
        Set(value As Color)
            _glyphColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeGlyphColor() As Boolean
        Return _glyphColor <> Color.Empty
    End Function
    Public Sub ResetGlyphColor()
        GlyphColor = Color.Empty
    End Sub

    ' ── What is actually painted ────────────────────────────────────────────────
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveBorderColor As Color
        Get
            Return If(_borderColor = Color.Empty, _autoBorder, _borderColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveFocusBorderColor As Color
        Get
            Return If(_focusBorderColor = Color.Empty, _autoFocus, _focusBorderColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveHoverColor As Color
        Get
            Return If(_hoverColor = Color.Empty, _autoHover, _hoverColor)
        End Get
    End Property

    ''' <summary>
    ''' The colour the glyph is really drawn in. When nothing is pinned it is <b>derived from the
    ''' pair the field is painted with</b> — the text colour pulled a third of the way towards the
    ''' background — rather than from a colour slot of its own.
    '''
    ''' <para>That is deliberate, and it is the fix for a real failure: a stored dim-grey slot is
    ''' only dim against the background it was chosen for. On a dark scheme, or on a field whose
    ''' background was set by hand, it can land almost exactly on its own background and the button
    ''' goes blank. Derived this way the glyph moves WITH the field, whatever the field is
    ''' painted with.</para>
    ''' </summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveGlyphColor As Color
        Get
            If _glyphColor <> Color.Empty Then Return _glyphColor
            Return ThemeShapes.Blend(ForeColor, BackColor, 0.3)
        End Get
    End Property

    ' =====================================================================
    ' THEME
    ' =====================================================================

    ''' <summary>
    ''' Reapplies the scheme. It also writes the inner box's colours: the box is a child control,
    ''' and a control that owns children must theme them itself (C5) — the generic traversal would
    ''' paint them by the per-type rules instead.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            _autoBack = p.InputBackColor
            _autoFore = p.InputTextColor
            _autoBorder = p.InputBorderColor
            _autoFocus = p.FocusRingColor
            _autoHover = p.ButtonHoverColor
            _autoDisabled = p.DisabledTextColor
            ' No slot is read for the glyph on purpose — EffectiveGlyphColor derives it from the
            ' text and background actually in force, which is what keeps it visible on every scheme.

            ' MyBase, not Me: writing the theme must never pass for a choice of the operator.
            If Not _backColorPinned Then MyBase.BackColor = _autoBack
            If Not _foreColorPinned Then MyBase.ForeColor = _autoFore
            _inner.BackColor = BackColor
            _inner.ForeColor = ForeColor

            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDatePicker.ApplyTheme", ex)
        End Try
    End Sub

    ' =====================================================================
    ' LAYOUT
    ' =====================================================================

    ''' <summary>
    ''' Everything the field draws inside lives here: the client area minus the outline, minus
    ''' <see cref="Control.Padding"/>. That is what makes the inherited <c>Padding</c> mean
    ''' something on this control instead of being an inert property in the grid.
    ''' </summary>
    Private Function ContentRect() As Rectangle
        Dim b As Integer = ThemeShapes.ScaleDpi(Me, _borderWidth)
        Return Shrink(Rectangle.Inflate(ClientRectangle, -b, -b), ScalePad(Padding))
    End Function

    ' The calendar button: a strip at the right edge of the content area, full height.
    Private Function ButtonRect() As Rectangle
        If Not _showDropDownButton Then Return Rectangle.Empty
        Dim zona As Rectangle = ContentRect()
        If zona.Width <= 0 OrElse zona.Height <= 0 Then Return Rectangle.Empty
        Dim w As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, _buttonWidth), Math.Max(1, zona.Width \ 2))
        If w <= 0 Then Return Rectangle.Empty
        Return New Rectangle(zona.Right - w, zona.Top, w, zona.Height)
    End Function

    ''' <summary>
    ''' Places the inner box. The field may be ANY height: the text stays one line, vertically
    ''' centred in the strip <see cref="TextPadding"/> leaves it, and the button grows with the
    ''' field.
    ''' </summary>
    Private Sub PositionInner()
        Try
            Dim zona As Rectangle = ContentRect()
            Dim pad As Padding = ScalePad(_textPadding)
            Dim buton As Rectangle = ButtonRect()
            Dim dreapta As Integer = If(buton.IsEmpty, zona.Right, buton.Left - ThemeShapes.ScaleDpi(Me, 2))

            Dim stanga As Integer = zona.Left + pad.Left
            Dim latime As Integer = Math.Max(0, (dreapta - pad.Right) - stanga)

            Dim sus0 As Integer = zona.Top + pad.Top
            Dim disponibil As Integer = Math.Max(1, (zona.Bottom - pad.Bottom) - sus0)
            Dim inaltime As Integer = Math.Min(disponibil, _inner.PreferredHeight)
            Dim sus As Integer = sus0 + Math.Max(0, (disponibil - inaltime) \ 2)

            _inner.SetBounds(stanga, sus, latime, inaltime)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDatePicker.PositionInner", ex)
        End Try
    End Sub

    ''' <summary>The inherited <c>Padding</c> is real air here, so a change to it must relayout.</summary>
    Protected Overrides Sub OnPaddingChanged(e As EventArgs)
        MyBase.OnPaddingChanged(e)
        PositionInner()
        Invalidate()
    End Sub

    Protected Overrides Sub OnLayout(levent As LayoutEventArgs)
        MyBase.OnLayout(levent)
        PositionInner()
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        PositionInner()
        Invalidate()
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        PositionInner()
        Invalidate()
    End Sub

    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        PositionInner()
        Invalidate()
    End Sub

    ' =====================================================================
    ' PAINT
    ' =====================================================================

    Private Function EffectiveRadius() As Integer
        Dim logic As Integer = If(_cornerRadius >= 0, _cornerRadius, ThemeManager.Current.Style.CornerRadius)
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logic))
    End Function

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim afara As New Rectangle(0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1))
            If afara.Width <= 0 OrElse afara.Height <= 0 Then Return

            Using cale As GraphicsPath = ThemeShapes.RoundedRect(afara, EffectiveRadius())
                Using b As New SolidBrush(BackColor)
                    g.FillPath(b, cale)
                End Using

                Dim buton As Rectangle = ButtonRect()
                If Not buton.IsEmpty AndAlso Enabled AndAlso (_hoverButton OrElse IsDropDownOpen) Then
                    ' Clipped to the outline so the button's own corner cannot poke out of the card.
                    Dim stare As GraphicsState = g.Save()
                    g.SetClip(cale, CombineMode.Intersect)
                    Using b As New SolidBrush(EffectiveHoverColor)
                        g.FillRectangle(b, buton)
                    End Using
                    g.Restore(stare)
                End If

                If _borderWidth > 0 Then
                    Dim culoare As Color = If(Not Enabled, _autoDisabled,
                                              If(_focused OrElse IsDropDownOpen,
                                                 EffectiveFocusBorderColor, EffectiveBorderColor))
                    Using p As New Pen(culoare, ThemeShapes.ScaleDpi(Me, _borderWidth))
                        g.DrawPath(p, cale)
                    End Using
                End If

                If Not buton.IsEmpty Then DrawCalendarGlyph(g, buton)
            End Using
        Catch ex As Exception
            ' Paint boundary: a throw from here would take the process down.
            GlobalErrorLog.Write("KBotDatePicker.OnPaint", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The button's calendar, drawn with GDI+: a page, the binding band across its top, two rings
    ''' above it and a row of day dots. Drawn rather than an image so it takes the scheme's colour
    ''' and stays sharp at every DPI.
    ''' </summary>
    Private Sub DrawCalendarGlyph(g As Graphics, area As Rectangle)
        Dim culoare As Color = If(Enabled, EffectiveGlyphColor, _autoDisabled)
        Dim zona As Rectangle = Shrink(area, ScalePad(_buttonPadding))
        Dim incape As Integer = Math.Min(zona.Width, zona.Height)
        If incape < 6 Then Return
        Dim latura As Integer = If(_glyphSize > 0,
                                   Math.Min(ThemeShapes.ScaleDpi(Me, _glyphSize), incape),
                                   incape)
        If latura < 6 Then Return

        Dim x As Integer = zona.Left + (zona.Width - latura) \ 2
        Dim y As Integer = zona.Top + (zona.Height - latura) \ 2
        Dim pagina As New Rectangle(x, y + latura \ 6, latura, latura - latura \ 6)

        Using p As New Pen(culoare, 1.0F)
            g.DrawRectangle(p, pagina)
            ' The binding band.
            Using b As New SolidBrush(culoare)
                g.FillRectangle(b, New Rectangle(pagina.Left, pagina.Top, pagina.Width,
                                                 Math.Max(2, latura \ 4)))
            End Using
            ' The two rings that hold the page.
            Dim inel As Integer = Math.Max(1, latura \ 7)
            g.DrawLine(p, pagina.Left + inel, y, pagina.Left + inel, pagina.Top)
            g.DrawLine(p, pagina.Right - inel, y, pagina.Right - inel, pagina.Top)
        End Using

        ' A row of days, only when there is room for them to read as dots and not as a smudge.
        Dim punct As Integer = Math.Max(1, latura \ 7)
        Dim randY As Integer = pagina.Top + Math.Max(2, latura \ 4) + punct
        If randY + punct <= pagina.Bottom - 1 Then
            Using b As New SolidBrush(culoare)
                For i As Integer = 0 To 2
                    Dim px As Integer = pagina.Left + punct + i * (punct * 2 + 1)
                    If px + punct > pagina.Right - 1 Then Exit For
                    g.FillRectangle(b, New Rectangle(px, randY, punct, punct))
                Next
            End Using
        End If
    End Sub

    ' =====================================================================
    ' MOUSE AND KEYBOARD
    ' =====================================================================

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            Dim peste As Boolean = ButtonRect().Contains(e.Location)
            If peste <> _hoverButton Then
                _hoverButton = peste
                Invalidate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDatePicker.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hoverButton Then
            _hoverButton = False
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button <> MouseButtons.Left Then Return

            ' The second click on the button: the press already closed the calendar by activating
            ' the window underneath, so opening it again here would make it look unclosable.
            If KBotCalendarPopup.ClosedJustNow Then
                _inner.Focus()
                Return
            End If

            If ButtonRect().Contains(e.Location) Then
                _inner.Focus()
                ShowDropDown()
            ElseIf _readOnlyText Then
                ' Typing is off, so the whole face is the button.
                _inner.Focus()
                ShowDropDown()
            Else
                _inner.Focus()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDatePicker.OnMouseDown", ex)
        End Try
    End Sub

    Private Sub OnInnerEnter(sender As Object, e As EventArgs)
        _focused = True
        Invalidate()
    End Sub

    Private Sub OnInnerLeave(sender As Object, e As EventArgs)
        _focused = False
        CommitText()
        Invalidate()
    End Sub

    Private Sub OnInnerKeyDown(sender As Object, e As KeyEventArgs)
        Try
            If e.KeyCode = Keys.F4 OrElse (e.Alt AndAlso e.KeyCode = Keys.Down) Then
                e.Handled = True
                e.SuppressKeyPress = True
                If IsDropDownOpen Then CloseDropDown() Else ShowDropDown()
                Return
            End If

            Select Case e.KeyCode
                Case Keys.Enter
                    CommitText()
                    e.Handled = True
                    e.SuppressKeyPress = True
                Case Keys.Escape
                    ' Back to the value behind the field, whatever was typed over it.
                    WriteText()
                    e.Handled = True
                    e.SuppressKeyPress = True
                Case Keys.Up, Keys.Down
                    If Not e.Alt AndAlso Not _readOnlyText Then
                        StepValue(If(e.KeyCode = Keys.Up, 1, -1))
                        e.Handled = True
                        e.SuppressKeyPress = True
                    End If
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDatePicker.OnInnerKeyDown", ex)
        End Try
    End Sub

    ' Up/Down nudge the date by a day, the way the stock picker nudges the field under the caret.
    Private Sub StepValue(days As Integer)
        Try
            If Not _hasValue Then
                SetValueCore(Date.Today, hasValue:=True)
                Return
            End If
            SetValueCore(_value.AddDays(days), hasValue:=True)
        Catch ex As ArgumentOutOfRangeException
            ' Off the end of Date's own range: the value stays where it is.
            GlobalErrorLog.Write("KBotDatePicker.StepValue", ex)
        End Try
    End Sub

    Protected Overrides Sub OnEnabledChanged(e As EventArgs)
        MyBase.OnEnabledChanged(e)
        _inner.Enabled = Enabled
        Invalidate()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                Dim p As KBotCalendarPopup = _popup
                _popup = Nothing
                If p IsNot Nothing AndAlso Not p.IsDisposed Then p.Close()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDatePicker.Dispose", ex)
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

End Class
