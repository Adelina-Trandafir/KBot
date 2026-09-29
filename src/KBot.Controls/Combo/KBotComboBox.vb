Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The K-BOT drop-down. Slice 0094: a plain <c>Control</c> of our own, no longer a
''' <c>ComboBox</c>. Everything on screen is ours: the face (rounded rectangle, 1 px outline, a GDI+
''' arrow) is painted here, the typed text lives in a borderless <c>TextBox</c> child placed exactly
''' where the painted caption would be, and the list is ONE window of our own
''' (<see cref="KBotComboList"/>) -- the arrow opens it with every item, typing with
''' <see cref="FindAsYouType"/> opens it with the matching ones.
'''
''' <para><b>Why not a ComboBox any more.</b> The native control positions its own EDIT child,
''' fixes its own height from the font and draws its own list; every one of those had to be fought
''' (measured offsets, re-alignment after three window messages, a second list for the search),
''' and it still could not be centred inside a grid cell like a text box. Data binding
''' (<c>DataSource</c>/<c>DisplayMember</c>/<c>ValueMember</c>) is gone with it: items are added to
''' <see cref="Items"/> and shown by <see cref="CaptionSelector"/> or <c>ToString()</c>.</para>
'''
''' <para>The colour contract is the house one (C1): <c>Color.Empty</c> = "from the theme", and any
''' colour set in the designer wins. <c>BackColor</c>/<c>ForeColor</c>/<c>Font</c> carry a pinned
''' flag plus the <c>ShouldSerialize*</c>/<c>Reset*</c> pair.</para>
'''
''' <para><b><see cref="Editable"/></b> shows the text box (the operator types); off, the caption is
''' painted and the control itself takes the focus and the keys. <b><see cref="LimitToList"/></b>
''' decides what happens to typed text that is not in the list: off = it is kept, on = the field
''' goes back to the last accepted value. The verdict is given when the field is left and on Enter
''' -- or whenever the host calls <see cref="CommitText"/>.</para>
''' </summary>
<ToolboxItem(True)>
<DefaultEvent("SelectedIndexChanged")>
Partial Public Class KBotComboBox
    Inherits Control
    Implements IThemedControl

    ' -- The "auto" colours (fallback for every property left Empty) --------------
    ' The starting values are the default light look, so that an UNTHEMED host (the test bench,
    ' the Visual Studio designer) still looks reasonable with no scheme applied.
    Private _autoBack As Color = Color.White
    Private _autoFore As Color = Color.FromArgb(30, 30, 30)
    Private _autoBorder As Color = Color.FromArgb(170, 170, 170)
    Private _autoHover As Color = Color.FromArgb(232, 241, 251)
    Private _autoArrow As Color = Color.FromArgb(90, 90, 90)
    Private _autoSelBack As Color = Color.FromArgb(200, 220, 255)
    Private _autoSelFore As Color = Color.FromArgb(30, 30, 30)

    Private _hoverColor As Color = Color.Empty
    Private _borderColor As Color = Color.Empty
    Private _arrowColor As Color = Color.Empty
    Private _selectionBackColor As Color = Color.Empty
    Private _selectionForeColor As Color = Color.Empty
    Private _cornerRadius As Integer = -1

    ' "The operator pinned this" flags -- see ShouldSerializeBackColor.
    Private _backColorPinned As Boolean = False
    Private _foreColorPinned As Boolean = False
    Private _fontPinned As Boolean = False

    Private _hovered As Boolean = False
    Private _mouseInside As Boolean = False      ' MouseEnter raised, MouseLeave not yet

    ' -- Items and selection ------------------------------------------------------------
    Private ReadOnly _items As KBotComboItemCollection
    Private _selectedIndex As Integer = -1
    Private _captionSelector As Func(Of Object, String)
    Private _updateCount As Integer = 0

    ' -- The text box -------------------------------------------------------------------
    Private ReadOnly _edit As KBotComboEdit
    Private _programmatic As Boolean = False      ' our own text writes are not operator edits
    Private _textAlign As HorizontalAlignment = HorizontalAlignment.Left
    Private _textOffsetY As Integer = 0

    ' -- Typing in the box --------------------------------------------------------------
    Private _editable As Boolean = False
    Private _limitToList As Boolean = True

    ' The last ACCEPTED text: what the field goes back to when LimitToList is on and the operator
    ' typed something that is not in the list. Kept up to date on every selection change, except
    ' while CommitText is moving the selection itself (or it would overwrite its own result).
    Private _lastAcceptedText As String = String.Empty
    Private _committing As Boolean = False

    ' -- Find as you type + input mask (slice 0082) ----------------------------------------
    Private _findAsYouType As Boolean = False
    Private _findAfterNChars As Integer = 1
    Private _findFirstGroupCount As Integer
    Private _inputMask As String = String.Empty
    Private _mask As KBotInputMask                ' Nothing = no mask

    ' -- The «new item» row (slice 0083) ----------------------------------------------------
    Private Const DefaultOfferNewItemText As String = "Adaugă un element nou…"
    Private _offerNewItem As Boolean = False
    Private _offerNewItemText As String = DefaultOfferNewItemText

    ' -- Cell editor mode (slice 0094, set by KBotDataView) ---------------------------------
    ' No frame, no arrow (the grid paints the cell and its chevron), free height, text flush with
    ' the left edge -- the grid places the control on the cell text exactly like its text editor.
    Private _cellEditorMode As Boolean = False

    Private Const EM_SETMARGINS As Integer = &HD3
    Private Const EC_LEFTMARGIN As Integer = &H1
    Private Const EC_RIGHTMARGIN As Integer = &H2

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    ''' <summary>The selection changed (from the list, the keyboard, or the host).</summary>
    <Category("K-BOT Combo")>
    <Description("The selection changed (list, keyboard or code).")>
    Public Event SelectedIndexChanged As EventHandler

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable, True)
        _items = New KBotComboItemCollection(Me)

        ' Through MyBase: the theme's defaults, not an operator's choice.
        MyBase.BackColor = _autoBack
        MyBase.ForeColor = _autoFore

        _edit = New KBotComboEdit(Me) With {
            .BorderStyle = BorderStyle.None,
            .AutoSize = False,
            .Multiline = False,
            .Visible = False
        }
        AddHandler _edit.TextChanged, AddressOf Edit_TextChanged
        AddHandler _edit.KeyDown, AddressOf Edit_KeyDown
        AddHandler _edit.KeyPress, AddressOf Edit_KeyPress
        AddHandler _edit.KeyUp, AddressOf Edit_KeyUp
        AddHandler _edit.GotFocus, AddressOf Edit_FocusChanged
        AddHandler _edit.LostFocus, AddressOf Edit_FocusChanged
        AddHandler _edit.MouseEnter, AddressOf Edit_MouseEnter
        AddHandler _edit.MouseLeave, AddressOf Edit_MouseLeave
        AddHandler _edit.MouseDown, AddressOf Edit_MouseDown
        AddHandler _edit.MouseWheel, AddressOf Edit_MouseWheel
        AddHandler _edit.HandleCreated, AddressOf Edit_HandleCreated
        Controls.Add(_edit)
        ApplyEditColors()
    End Sub

    Protected Overrides ReadOnly Property DefaultSize As Size
        Get
            Return New Size(121, 28)
        End Get
    End Property

    ' =====================================================================
    ' ITEMS AND SELECTION
    ' =====================================================================

    ''' <summary>The items. Shown by <see cref="CaptionSelector"/>, or by <c>ToString()</c>.</summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Items As KBotComboItemCollection
        Get
            Return _items
        End Get
    End Property

    ''' <summary>
    ''' What the list and the face show for an item. Nothing (the default) = <c>ToString()</c>.
    ''' Replaces <c>DisplayMember</c>: set in code by a host whose items are objects.
    ''' </summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property CaptionSelector As Func(Of Object, String)
        Get
            Return _captionSelector
        End Get
        Set(value As Func(Of Object, String))
            _captionSelector = value
            If _selectedIndex >= 0 Then WriteDisplayText(CaptionOf(_items(_selectedIndex)))
            Invalidate()
        End Set
    End Property

    ''' <summary>The selected item's index; -1 = none. Out of range THROWS.</summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedIndex As Integer
        Get
            Return _selectedIndex
        End Get
        Set(value As Integer)
            If value < -1 OrElse value >= _items.Count Then
                Throw New ArgumentOutOfRangeException(NameOf(value), value,
                    "SelectedIndex must be -1 or the index of an item.")
            End If
            If value = _selectedIndex Then Return
            SetSelectedIndexCore(value, writeText:=True)
        End Set
    End Property

    ''' <summary>The selected item; Nothing = none. An item that is not in the list THROWS.</summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedItem As Object
        Get
            Return If(_selectedIndex >= 0, _items(_selectedIndex), Nothing)
        End Get
        Set(value As Object)
            If value Is Nothing Then
                SelectedIndex = -1
                Return
            End If
            Dim index As Integer = _items.IndexOf(value)
            If index < 0 Then
                Throw New ArgumentException("The item is not in the combo's list.", NameOf(value))
            End If
            SelectedIndex = index
        End Set
    End Property

    ''' <summary>
    ''' The text on the face. Editable: what is in the box (a host write selects the item with
    ''' exactly that caption, or none). Not editable: the selected item's caption, and a write
    ''' selects the item with that caption (none = no selection).
    ''' </summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overrides Property Text As String
        Get
            Return If(MyBase.Text, String.Empty)
        End Get
        Set(value As String)
            Dim v As String = If(value, String.Empty)
            Dim match As Integer = If(v.Length = 0, -1, FindStringExact(v))
            If _editable Then
                ' Text first: a SelectedIndexChanged handler reads the new text, not the old one.
                WriteDisplayText(v)
                SetSelectedIndexCore(match, writeText:=False)
                _lastAcceptedText = v
            Else
                SetSelectedIndexCore(match, writeText:=True)
            End If
        End Set
    End Property

    ''' <summary>Stops repainting while a host fills <see cref="Items"/>; pair with
    ''' <see cref="EndUpdate"/>.</summary>
    Public Sub BeginUpdate()
        _updateCount += 1
    End Sub

    Public Sub EndUpdate()
        _updateCount = Math.Max(0, _updateCount - 1)
        If _updateCount = 0 Then Invalidate()
    End Sub

    ''' <summary>The first item whose caption equals <paramref name="s"/> (case ignored); -1 = none.</summary>
    Public Function FindStringExact(s As String) As Integer
        If s Is Nothing Then Return -1
        For i As Integer = 0 To _items.Count - 1
            If String.Equals(CaptionOf(_items(i)), s, StringComparison.CurrentCultureIgnoreCase) Then Return i
        Next
        Return -1
    End Function

    ''' <summary>The height of one list row: the font plus a little air.</summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ItemHeight As Integer
        Get
            Return Math.Max(1, Font.Height + ThemeShapes.ScaleDpi(Me, 6))
        End Get
    End Property

    ''' <summary>
    ''' The height the box takes outside a grid: a row plus the 3 px frame a native combo had on
    ''' each side, so the forms laid out around the old control keep their measurements.
    ''' </summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property PreferredHeight As Integer
        Get
            Return ItemHeight + 6
        End Get
    End Property

    ' Moves the selection. writeText = put the item's caption on the face (False when the caller
    ' writes the text itself). Raises SelectedIndexChanged only on a real change.
    Private Sub SetSelectedIndexCore(value As Integer, writeText As Boolean)
        Dim changed As Boolean = (value <> _selectedIndex)
        _selectedIndex = value
        If writeText Then WriteDisplayText(If(value >= 0, CaptionOf(_items(value)), String.Empty))
        If changed Then
            ' Picking from the list is by definition an accepted value -- it becomes the fallback.
            If Not _committing Then
                _lastAcceptedText = If(value >= 0, CaptionOf(_items(value)), String.Empty)
            End If
            OnSelectedIndexChanged(EventArgs.Empty)
        End If
        Invalidate()
    End Sub

    Protected Overridable Sub OnSelectedIndexChanged(e As EventArgs)
        RaiseEvent SelectedIndexChanged(Me, e)
    End Sub

    ' -- Notifications from KBotComboItemCollection -----------------------------------------

    Friend Sub OnItemInserted(index As Integer)
        If _selectedIndex >= 0 AndAlso index <= _selectedIndex Then _selectedIndex += 1
        ItemsChanged()
    End Sub

    Friend Sub OnItemsAppended()
        ItemsChanged()
    End Sub

    Friend Sub OnItemRemoved(index As Integer)
        If index = _selectedIndex Then
            SetSelectedIndexCore(-1, writeText:=Not _editable)
        ElseIf index < _selectedIndex Then
            _selectedIndex -= 1
        End If
        ItemsChanged()
    End Sub

    Friend Sub OnItemsCleared()
        If _selectedIndex >= 0 Then SetSelectedIndexCore(-1, writeText:=Not _editable)
        ItemsChanged()
    End Sub

    Friend Sub OnItemReplaced(index As Integer)
        If index = _selectedIndex Then WriteDisplayText(CaptionOf(_items(index)))
        ItemsChanged()
    End Sub

    ' An open list shows rows by item index: after any change it would point at the wrong items.
    Private Sub ItemsChanged()
        CloseDropDown()
        If _updateCount = 0 Then Invalidate()
    End Sub

    ' Honours CaptionSelector; ToString() otherwise.
    Friend Function CaptionOf(item As Object) As String
        If item Is Nothing Then Return String.Empty
        If _captionSelector IsNot Nothing Then Return If(_captionSelector(item), String.Empty)
        Return If(item.ToString(), String.Empty)
    End Function

    ' The captions of every item, in list order.
    Private Function AllCaptions() As List(Of String)
        Dim list As New List(Of String)(_items.Count)
        For Each it As Object In _items
            list.Add(CaptionOf(it))
        Next
        Return list
    End Function

    ' Writes the face text without it counting as an operator edit.
    Private Sub WriteDisplayText(value As String)
        Dim v As String = If(value, String.Empty)
        _programmatic = True
        Try
            If _editable Then
                If Not String.Equals(_edit.Text, v, StringComparison.Ordinal) Then _edit.Text = v
            End If
            If Not String.Equals(MyBase.Text, v, StringComparison.Ordinal) Then MyBase.Text = v
        Finally
            _programmatic = False
        End Try
        Invalidate()
    End Sub

    ' =====================================================================
    ' TYPING IN THE BOX
    ' =====================================================================

    ''' <summary>
    ''' On = the operator can TYPE in the box (a text box takes the face); off = they can only pick
    ''' from the list, and the control itself takes the focus and the keys.
    ''' </summary>
    <Category("K-BOT Combo")>
    <Description("Allow typing in the box. Off = pick from the list only.")>
    <DefaultValue(False)>
    Public Property Editable As Boolean
        Get
            Return _editable
        End Get
        Set(value As Boolean)
            If _editable = value Then Return
            _editable = value
            If value Then
                WriteDisplayText(MyBase.Text)
            ElseIf _selectedIndex < 0 Then
                WriteDisplayText(String.Empty)
            End If
            UpdateEditVisibility()
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' What happens to typed text that is NOT in the list. On (the default) = the field goes back
    ''' to the last accepted value; off = the text stays exactly as typed and <c>SelectedIndex</c>
    ''' becomes -1, because it no longer stands for any row.
    ''' Without <see cref="Editable"/> it has nothing to do: the list is the only source anyway.
    ''' </summary>
    <Category("K-BOT Combo")>
    <Description("Accept list values only. Off = text typed by hand survives leaving the field.")>
    <DefaultValue(True)>
    Public Property LimitToList As Boolean
        Get
            Return _limitToList
        End Get
        Set(value As Boolean)
            _limitToList = value
        End Set
    End Property

    ''' <summary>
    ''' An optical nudge of the text, in logical px (scaled at runtime). 0 (the default) leaves the
    ''' text on the exact vertical centre. Positive moves it down, negative up.
    ''' </summary>
    <Category("K-BOT Combo")>
    <Description("Optical nudge of the text, px @96dpi. Positive = down, negative = up. 0 = exact vertical centre.")>
    <DefaultValue(0)>
    Public Property TextOffsetY As Integer
        Get
            Return _textOffsetY
        End Get
        Set(value As Integer)
            _textOffsetY = value
            LayoutEdit()
            Invalidate()
        End Set
    End Property

    ''' <summary>Horizontal alignment of the text on the face (typed or painted).</summary>
    <Category("K-BOT Combo")>
    <Description("Horizontal alignment of the text on the face.")>
    <DefaultValue(HorizontalAlignment.Left)>
    Public Property TextAlign As HorizontalAlignment
        Get
            Return _textAlign
        End Get
        Set(value As HorizontalAlignment)
            If Not [Enum].IsDefined(GetType(HorizontalAlignment), value) Then
                Throw New ArgumentException("Unknown alignment: " & value.ToString(), NameOf(value))
            End If
            _textAlign = value
            _edit.TextAlign = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Slice 0082. On = while the operator types, the list shows only the rows that match the text
    ''' so far (rows that START with it first, then rows that CONTAIN it; case and diacritics
    ''' ignored). Up/Down move through it, Enter or a click takes a row, Escape closes it. The
    ''' arrow still opens the whole list. Needs <see cref="Editable"/>.
    ''' </summary>
    <Category("K-BOT Combo")>
    <Description("Show the matching rows in the list while typing. Needs Editable = True.")>
    <DefaultValue(False)>
    Public Property FindAsYouType As Boolean
        Get
            Return _findAsYouType
        End Get
        Set(value As Boolean)
            _findAsYouType = value
            If Not value AndAlso Not _openedByArrow Then CloseDropDown()
        End Set
    End Property

    ''' <summary>
    ''' How many rows at the TOP of the list form a group the search keeps ahead of the rest
    ''' (e.g. the values already in use, then the others). Set at run time by the host that
    ''' ordered the items; 0 = no group.
    ''' </summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property FindFirstGroupCount As Integer
        Get
            Return _findFirstGroupCount
        End Get
        Set(value As Integer)
            _findFirstGroupCount = Math.Max(0, value)
        End Set
    End Property

    ''' <summary>
    ''' Slice 0082. The search starts once at least this many characters have been typed. With an
    ''' <see cref="InputMask"/> only the characters the operator types are counted -- the literals
    ''' the mask writes by itself (the dots of a classification) are not.
    ''' </summary>
    <Category("K-BOT Combo")>
    <Description("FindAsYouType starts after this many typed characters (mask literals not counted). Minimum 1.")>
    <DefaultValue(1)>
    Public Property FindAfterNChars As Integer
        Get
            Return _findAfterNChars
        End Get
        Set(value As Integer)
            If value < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(value), value, "FindAfterNChars must be at least 1.")
            End If
            _findAfterNChars = value
        End Set
    End Property

    ''' <summary>
    ''' Slice 0082. What the operator may type, one character per position: <c>0</c> = digit,
    ''' <c>L</c> = letter, <c>A</c> = letter or digit, <c>&amp;</c> = any character, <c>\x</c> = the
    ''' literal <c>x</c>; every other character is a literal the mask writes by itself. Empty = no
    ''' mask. Needs <see cref="Editable"/>. An invalid mask THROWS. See <see cref="KBotInputMask"/>.
    ''' </summary>
    <Category("K-BOT Combo")>
    <Description("Input mask: 0 = digit, L = letter, A = letter or digit, & = any, \x = literal x, anything else = literal written automatically. Empty = none. Needs Editable = True.")>
    <DefaultValue("")>
    Public Property InputMask As String
        Get
            Return _inputMask
        End Get
        Set(value As String)
            Dim v As String = If(value, String.Empty)
            ' Parsed BEFORE anything is stored: a mask that throws leaves the old one in place.
            _mask = If(v.Length = 0, Nothing, New KBotInputMask(v))
            _inputMask = v
        End Set
    End Property

    ''' <summary>
    ''' Slice 0083. With <see cref="LimitToList"/> on, a list that has nothing to show -- nothing
    ''' matches the typed text, or the combo has no items at all -- shows ONE row instead,
    ''' <see cref="OfferNewItemText"/>. Clicking it (or Enter on it) raises
    ''' <see cref="NewItemRequested"/>; the host adds the item.
    ''' </summary>
    <Category("K-BOT Combo")>
    <Description("With LimitToList on: when the list has nothing to show, offer one row (OfferNewItemText) that raises NewItemRequested.")>
    <DefaultValue(False)>
    Public Property OfferNewItem As Boolean
        Get
            Return _offerNewItem
        End Get
        Set(value As Boolean)
            _offerNewItem = value
            If Not value Then CloseDropDown()
        End Set
    End Property

    ''' <summary>Slice 0083. The text of the «new item» row. Nothing / empty goes back to the default.</summary>
    <Category("K-BOT Combo")>
    <Description("Text of the row OfferNewItem shows when the list has nothing to show.")>
    <DefaultValue(DefaultOfferNewItemText)>
    Public Property OfferNewItemText As String
        Get
            Return _offerNewItemText
        End Get
        Set(value As String)
            _offerNewItemText = If(String.IsNullOrEmpty(value), DefaultOfferNewItemText, value)
        End Set
    End Property

    ''' <summary>Slice 0083. The operator chose the «new item» row. <c>e.Text</c> is what was typed.
    ''' When the handler adds an item whose caption is exactly that text, the combo selects it.</summary>
    <Category("K-BOT Combo")>
    <Description("The operator chose the OfferNewItem row. e.Text = the typed text.")>
    Public Event NewItemRequested As EventHandler(Of KBotComboNewItemEventArgs)

    ''' <summary>Does the «new item» row apply now? (<see cref="OfferNewItem"/> needs
    ''' <see cref="LimitToList"/>.)</summary>
    Private ReadOnly Property OffersNewItem As Boolean
        Get
            Return _offerNewItem AndAlso _limitToList
        End Get
    End Property

    ''' <summary>
    ''' The «new item» row was chosen: the list closes, the host is told, and an item the host
    ''' added under exactly the typed text becomes the selection.
    ''' </summary>
    Friend Sub RequestNewItem()
        CloseDropDown()
        Dim typed As String = If(_editable, Text, String.Empty)
        RaiseEvent NewItemRequested(Me, New KBotComboNewItemEventArgs(typed))
        If typed.Length > 0 AndAlso _selectedIndex < 0 Then
            Dim added As Integer = FindStringExact(typed)
            If added >= 0 Then AcceptRow(added)
        End If
    End Sub

    ''' <summary>The raw value under the mask (the typed characters only, no literals); the whole
    ''' text when there is no mask.</summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property UnmaskedText As String
        Get
            Return If(_mask Is Nothing, Text, _mask.ExtractRaw(Text))
        End Get
    End Property

    ' -- The text box's selection, for hosts that move between fields at the text's edges ----

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectionStart As Integer
        Get
            Return If(_editable, _edit.SelectionStart, 0)
        End Get
        Set(value As Integer)
            If _editable Then _edit.SelectionStart = value
        End Set
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectionLength As Integer
        Get
            Return If(_editable, _edit.SelectionLength, 0)
        End Get
        Set(value As Integer)
            If _editable Then _edit.SelectionLength = value
        End Set
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property TextLength As Integer
        Get
            Return Text.Length
        End Get
    End Property

    ''' <summary>Selects the whole typed text (no-op when not <see cref="Editable"/>).</summary>
    Public Sub SelectAll()
        If _editable Then _edit.SelectAll()
    End Sub

    ''' <summary>Where the text box sits inside the control (diagnostics: the DevHarness bench).</summary>
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EditBounds As Rectangle
        Get
            Return If(_edit.Visible, _edit.Bounds, Rectangle.Empty)
        End Get
    End Property

    ''' <summary>
    ''' The rows of <paramref name="captions"/> that match <paramref name="typed"/>, as indexes:
    ''' those that START with it first, then those that only CONTAIN it, each group in list order.
    ''' Case and diacritics are ignored. Pure, so it is tested on its own.
    ''' </summary>
    Public Shared Function FindMatches(captions As IList(Of String), typed As String) As List(Of Integer)
        Return FindMatches(captions, typed, 0)
    End Function

    ''' <summary>
    ''' The same search, with the first <paramref name="firstGroupCount"/> rows kept AHEAD of the
    ''' rest: the matches among them (start, then contain) come first, then the matches among the
    ''' others (start, then contain). 0 = one group, the plain search.
    ''' </summary>
    Public Shared Function FindMatches(captions As IList(Of String), typed As String,
                                       firstGroupCount As Integer) As List(Of Integer)
        Dim result As New List(Of Integer)()
        If captions Is Nothing OrElse String.IsNullOrEmpty(typed) Then Return result
        Dim split As Integer = Math.Max(0, Math.Min(firstGroupCount, captions.Count))
        If split > 0 Then result.AddRange(MatchRange(captions, typed, 0, split))
        result.AddRange(MatchRange(captions, typed, split, captions.Count))
        Return result
    End Function

    ' The rows [fromIndex, toIndex) that match: those that start with the text, then those that
    ' only contain it, each in list order.
    Private Shared Function MatchRange(captions As IList(Of String), typed As String,
                                       fromIndex As Integer, toIndex As Integer) As List(Of Integer)
        Dim starts As New List(Of Integer)()
        Dim contains As New List(Of Integer)()
        Dim ci As Globalization.CompareInfo = Globalization.CultureInfo.CurrentCulture.CompareInfo
        For i As Integer = fromIndex To toIndex - 1
            Dim c As String = If(captions(i), String.Empty)
            If ci.IsPrefix(c, typed, LooseCompare) Then
                starts.Add(i)
            ElseIf ci.IndexOf(c, typed, LooseCompare) >= 0 Then
                contains.Add(i)
            End If
        Next
        starts.AddRange(contains)
        Return starts
    End Function

    Private Const LooseCompare As Globalization.CompareOptions =
        Globalization.CompareOptions.IgnoreCase Or Globalization.CompareOptions.IgnoreNonSpace

    ' The one row whose caption STARTS with the text; -1 when none or several do.
    Private Function UniquePrefixMatch(typed As String) As Integer
        Dim ci As Globalization.CompareInfo = Globalization.CultureInfo.CurrentCulture.CompareInfo
        Dim found As Integer = -1
        Dim captions As List(Of String) = AllCaptions()
        For i As Integer = 0 To captions.Count - 1
            If Not ci.IsPrefix(captions(i), typed, LooseCompare) Then Continue For
            If found >= 0 Then Return -1
            found = i
        Next
        Return found
    End Function

    ' The first row whose caption STARTS with the text; -1 when none does or the text is empty.
    Private Function FirstPrefixMatch(typed As String) As Integer
        If String.IsNullOrEmpty(typed) Then Return -1
        Dim ci As Globalization.CompareInfo = Globalization.CultureInfo.CurrentCulture.CompareInfo
        For i As Integer = 0 To _items.Count - 1
            If ci.IsPrefix(CaptionOf(_items(i)), typed, LooseCompare) Then Return i
        Next
        Return -1
    End Function

    ''' <summary>
    ''' Give the verdict on the text typed NOW, without waiting for the field to be left. A host
    ''' calls it when it reads the value from a button the operator can reach without moving the
    ''' focus. Idempotent: calling it twice changes nothing the second time.
    '''
    ''' <para>Slice 0082: with <see cref="FindAsYouType"/> on, a text that is not a whole row but
    ''' is the START of exactly ONE row takes that row -- a full classification code typed through
    ''' the mask picks its «code — name» row without the operator having to type the name.</para>
    ''' </summary>
    Public Sub CommitText()
        Try
            If Not _editable Then Return
            CloseDropDown()

            Dim typed As String = Text
            Dim match As Integer = If(typed.Length = 0, -1, FindStringExact(typed))
            If match < 0 AndAlso _findAsYouType AndAlso typed.Length > 0 Then match = UniquePrefixMatch(typed)

            _committing = True
            Try
                If match >= 0 Then
                    ' The text IS in the list: the selection follows it, with the list's spelling.
                    WriteDisplayText(CaptionOf(_items(match)))
                    SetSelectedIndexCore(match, writeText:=False)
                    _lastAcceptedText = Text
                ElseIf _limitToList Then
                    ' Refused: back to the last accepted value (empty = empty field, no selection).
                    Dim target As Integer = If(_lastAcceptedText.Length = 0, -1, FindStringExact(_lastAcceptedText))
                    WriteDisplayText(_lastAcceptedText)
                    SetSelectedIndexCore(target, writeText:=False)
                Else
                    ' Accepted as free text: the selection is no longer allowed to claim a row.
                    WriteDisplayText(typed)
                    SetSelectedIndexCore(-1, writeText:=False)
                    _lastAcceptedText = typed
                End If
            Finally
                _committing = False
            End Try

            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.CommitText", ex)
        End Try
    End Sub

    ' A row of the list becomes the selection -- SelectedIndexChanged fires, the row becomes the
    ' last accepted value, and its caption goes on the face even when the index did not move.
    Private Sub AcceptRow(itemIndex As Integer)
        CloseDropDown()
        If itemIndex < 0 OrElse itemIndex >= _items.Count Then Return
        Dim caption As String = CaptionOf(_items(itemIndex))
        WriteDisplayText(caption)
        SetSelectedIndexCore(itemIndex, writeText:=False)
        _lastAcceptedText = caption
        If _editable Then
            _edit.SelectionStart = _edit.TextLength
            _edit.SelectionLength = 0
        End If
        Invalidate()
    End Sub

    ' Up/Down (and PageUp/PageDown/Home/End) with the list closed: the next item becomes the
    ' selection, like the native control did.
    Private Sub StepSelection(delta As Integer, toEdge As Boolean)
        If _items.Count = 0 Then Return
        Dim target As Integer
        If toEdge Then
            target = If(delta > 0, _items.Count - 1, 0)
        ElseIf _selectedIndex < 0 Then
            target = If(delta > 0, 0, _items.Count - 1)
        Else
            target = Math.Max(0, Math.Min(_items.Count - 1, _selectedIndex + delta))
        End If
        AcceptRow(target)
        If _editable Then _edit.SelectAll()
    End Sub

    ' Not editable: a typed character selects the next item whose caption starts with it.
    Private Sub JumpToChar(c As Char)
        If _items.Count = 0 Then Return
        Dim ci As Globalization.CompareInfo = Globalization.CultureInfo.CurrentCulture.CompareInfo
        Dim s As String = c.ToString()
        For k As Integer = 1 To _items.Count
            Dim i As Integer = (Math.Max(-1, _selectedIndex) + k) Mod _items.Count
            If ci.IsPrefix(CaptionOf(_items(i)), s, LooseCompare) Then
                AcceptRow(i)
                Return
            End If
        Next
    End Sub

    ' =====================================================================
    ' MASK
    ' =====================================================================

    ''' <summary>How many characters count towards <see cref="FindAfterNChars"/>: the typed ones
    ''' under a mask, the whole text otherwise.</summary>
    Private Function SignificantLength() As Integer
        Return UnmaskedText.Length
    End Function

    ' Writes the result of one masked keystroke: the text, then the caret.
    Private Sub ApplyMaskEdit(edit As KBotMaskEdit)
        If edit Is Nothing Then Return
        WriteDisplayText(edit.Text)
        _edit.SelectionStart = edit.Caret
        _edit.SelectionLength = 0
        AfterOperatorEdit()
    End Sub

    ' Every change of the text box's text. Ours (WriteDisplayText) only mirrors it; the
    ' operator's (typing without a mask, Ctrl+X, the context-menu paste) is re-shaped by the mask
    ' and then drives the list.
    Private Sub Edit_TextChanged(sender As Object, e As EventArgs)
        Try
            If _programmatic Then Return
            MyBase.Text = _edit.Text
            If _mask IsNot Nothing Then
                Dim shaped As String = _mask.Normalize(_edit.Text)
                If Not String.Equals(shaped, _edit.Text, StringComparison.Ordinal) Then
                    ApplyMaskEdit(New KBotMaskEdit(shaped, shaped.Length))
                    Return
                End If
            End If
            AfterOperatorEdit()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.Edit_TextChanged", ex)
        End Try
    End Sub

    ' =====================================================================
    ' KEYBOARD
    ' =====================================================================

    ' The text box's keys are the combo's keys: hosts subscribe to the COMBO's KeyDown (the
    ' grid's editor does), so they are raised here, through the combo's own overrides.
    Private Sub Edit_KeyDown(sender As Object, e As KeyEventArgs)
        OnKeyDown(e)
    End Sub

    Private Sub Edit_KeyPress(sender As Object, e As KeyPressEventArgs)
        OnKeyPress(e)
    End Sub

    Private Sub Edit_KeyUp(sender As Object, e As KeyEventArgs)
        OnKeyUp(e)
    End Sub

    ''' <summary>
    ''' The keys the combo keeps for itself before the host sees them: the list's navigation while
    ''' it is open, the mask's Delete/paste, F4 / Alt+Down to open the list. Enter gives the verdict
    ''' on the typed text BEFORE the host's handler runs, so a host reading the value on Enter reads
    ''' the committed one.
    ''' </summary>
    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Try
            If HandleListKeys(e) Then Return
            If HandleMaskKeys(e) Then Return
            If (e.KeyCode = Keys.F4 AndAlso Not e.Alt) OrElse
               (e.Alt AndAlso (e.KeyCode = Keys.Down OrElse e.KeyCode = Keys.Up)) Then
                DroppedDown = Not DroppedDown
                e.Handled = True
                e.SuppressKeyPress = True
                Return
            End If
            If e.KeyCode = Keys.Enter AndAlso _editable Then CommitText()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnKeyDown", ex)
        End Try

        MyBase.OnKeyDown(e)

        Try
            If e.Handled OrElse e.Alt Then Return
            Select Case e.KeyCode
                Case Keys.Down
                    StepSelection(1, toEdge:=False)
                Case Keys.Up
                    StepSelection(-1, toEdge:=False)
                Case Keys.PageDown
                    StepSelection(Math.Max(1, MaxDropDownItems), toEdge:=False)
                Case Keys.PageUp
                    StepSelection(-Math.Max(1, MaxDropDownItems), toEdge:=False)
                Case Keys.Home, Keys.End
                    ' In the text box they move the caret.
                    If _editable Then Return
                    StepSelection(If(e.KeyCode = Keys.End, 1, -1), toEdge:=True)
                Case Else
                    Return
            End Select
            e.Handled = True
            e.SuppressKeyPress = True
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnKeyDown", ex)
        End Try
    End Sub

    ' While the list is open the navigation keys drive IT. True = the key was used.
    Private Function HandleListKeys(e As KeyEventArgs) As Boolean
        If Not DroppedDown OrElse e.Alt Then Return False
        Select Case e.KeyCode
            Case Keys.Down, Keys.Up, Keys.PageDown, Keys.PageUp
                Dim page As Integer = _list.PageSize
                _list.MoveSelection(
                    If(e.KeyCode = Keys.Down, 1, If(e.KeyCode = Keys.Up, -1,
                       If(e.KeyCode = Keys.PageDown, page, -page))))
            Case Keys.Home, Keys.End
                If _editable Then Return False
                _list.MoveToEdge(e.KeyCode = Keys.End)
            Case Keys.Enter
                If _list.SelectedIsNewItem Then
                    RequestNewItem()
                ElseIf _list.SelectedItemIndex >= 0 Then
                    AcceptRow(_list.SelectedItemIndex)
                Else
                    CloseDropDown()
                End If
            Case Keys.Escape
                CloseDropDown()
            Case Keys.Tab
                CloseDropDown()
                Return False
            Case Else
                Return False
        End Select
        e.Handled = True
        e.SuppressKeyPress = True
        Return True
    End Function

    ' Under a mask, Delete and paste are edits of the raw value too. True = the key was used.
    Private Function HandleMaskKeys(e As KeyEventArgs) As Boolean
        If Not _editable OrElse _mask Is Nothing Then Return False
        If e.KeyCode = Keys.Delete AndAlso Not e.Control AndAlso Not e.Shift Then
            ApplyMaskEdit(_mask.DeleteForward(Text, _edit.SelectionStart, _edit.SelectionLength))
        ElseIf (e.KeyCode = Keys.V AndAlso e.Control) OrElse (e.KeyCode = Keys.Insert AndAlso e.Shift) Then
            Dim pasted As String = If(Clipboard.ContainsText(), Clipboard.GetText(), String.Empty)
            ApplyMaskEdit(_mask.Paste(Text, _edit.SelectionStart, _edit.SelectionLength, pasted))
        Else
            Return False
        End If
        e.Handled = True
        e.SuppressKeyPress = True
        Return True
    End Function

    ''' <summary>
    ''' Typed characters. Under a mask each goes through <see cref="KBotInputMask.InsertChar"/>
    ''' (a character no slot takes is simply not written), Backspace through its own rule, and
    ''' Ctrl+V is swallowed because <see cref="OnKeyDown"/> already pasted. Not editable: a
    ''' character jumps to the next item starting with it.
    ''' </summary>
    Protected Overrides Sub OnKeyPress(e As KeyPressEventArgs)
        MyBase.OnKeyPress(e)
        Try
            If e.Handled Then Return
            Dim c As Char = e.KeyChar
            If Not _editable Then
                If Not Char.IsControl(c) Then JumpToChar(c)
                e.Handled = True
                Return
            End If
            ' Enter / Escape have been dealt with in OnKeyDown; the text box would only beep.
            If AscW(c) = 13 OrElse AscW(c) = 27 Then
                e.Handled = True
                Return
            End If
            If _mask Is Nothing Then Return
            Select Case AscW(c)
                Case 8          ' Backspace
                    ApplyMaskEdit(_mask.Backspace(Text, _edit.SelectionStart, _edit.SelectionLength))
                    e.Handled = True
                Case 22         ' Ctrl+V -- pasted in OnKeyDown
                    e.Handled = True
                Case Else
                    If Char.IsControl(c) Then Return        ' Ctrl+A/C/X/Z
                    Dim edit As KBotMaskEdit = _mask.InsertChar(Text, _edit.SelectionStart, _edit.SelectionLength, c)
                    If edit IsNot Nothing Then ApplyMaskEdit(edit)
                    e.Handled = True
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnKeyPress", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The keys the control needs as input when it has the focus itself (not editable): the
    ''' arrows always; Enter and Escape while the list is open, or a dialog's AcceptButton /
    ''' CancelButton would take them first.
    ''' </summary>
    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        Select Case keyData And Keys.KeyCode
            Case Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.PageUp, Keys.PageDown, Keys.Home, Keys.End
                Return True
        End Select
        If WantsKey(keyData) Then Return True
        Return MyBase.IsInputKey(keyData)
    End Function

    ''' <summary>The keys the list needs while it is open (asked by the text box too).</summary>
    Friend Function WantsKey(keyData As Keys) As Boolean
        If Not DroppedDown OrElse (keyData And Keys.Alt) <> Keys.None Then Return False
        Select Case keyData And Keys.KeyCode
            Case Keys.Enter, Keys.Escape, Keys.Up, Keys.Down, Keys.PageUp, Keys.PageDown
                Return True
        End Select
        Return False
    End Function

    ' =====================================================================
    ' INHERITED PROPERTIES WITH A PINNED FLAG
    ' =====================================================================

    ''' <summary>The background of the face AND of the list; not pinned here, it follows the theme.</summary>
    <Category("K-BOT Combo Colors")>
    <Description("The background of the face and of the list; not pinned here, it follows the theme.")>
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
    ''' The truth is the flag, not Control's property bag: <c>Control.ShouldSerializeBackColor</c>
    ''' answers True as soon as the property has ever been WRITTEN -- including by
    ''' <see cref="ApplyTheme"/> -- and the designer would freeze a colour nobody chose.
    ''' </summary>
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColorPinned
    End Function

    ' The flag goes out AFTER the colour is written: ResetBackColor goes through the VIRTUAL setter.
    Public Overrides Sub ResetBackColor()
        MyBase.BackColor = _autoBack
        _backColorPinned = False
        Invalidate()
    End Sub

    <Category("K-BOT Combo Colors")>
    <Description("The text colour; not pinned here, it follows the theme.")>
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

    ''' <summary>The font; not pinned here, inherited from the ambient one (so ApplyBaseFont reaches it).</summary>
    <Category("K-BOT Combo")>
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

    ''' <summary>The flag goes out AFTER the base reset -- <c>Control.ResetFont</c> writes through the virtual setter.</summary>
    Public Overrides Sub ResetFont()
        MyBase.ResetFont()
        _fontPinned = False
        Invalidate()
    End Sub

    ''' <summary>The face text is never designer data: it comes from the items or the host.</summary>
    Public Function ShouldSerializeText() As Boolean
        Return False
    End Function

    ' =====================================================================
    ' OWN PROPERTIES (Color.Empty = "from the theme")
    ' =====================================================================

    <Category("K-BOT Combo Colors")>
    <Description("The background of the face under the cursor. Empty = from the theme.")>
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

    <Category("K-BOT Combo Colors")>
    <Description("The 1 px outline of the face. Empty = from the theme.")>
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

    <Category("K-BOT Combo Colors")>
    <Description("The drop-down arrow. Empty = from the theme.")>
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

    <Category("K-BOT Combo Colors")>
    <Description("The background of the highlighted list row. Empty = from the theme.")>
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

    <Category("K-BOT Combo Colors")>
    <Description("The text of the highlighted list row. Empty = from the theme.")>
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

    ''' <summary>Corner radius of the face, in logical px. -1 = from the theme (Style.CornerRadius).</summary>
    <Category("K-BOT Combo")>
    <Description("Corner radius of the face, px @96dpi. -1 = from the theme, 0 = square.")>
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

    ' -- The effective colours (the property if it was chosen, otherwise "auto" from the theme) ---
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveHoverColor As Color
        Get
            Return If(_hoverColor = Color.Empty, _autoHover, _hoverColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveBorderColor As Color
        Get
            Return If(_borderColor = Color.Empty, _autoBorder, _borderColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveArrowColor As Color
        Get
            Return If(_arrowColor = Color.Empty, _autoArrow, _arrowColor)
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

    ' =====================================================================
    ' THEME
    ' =====================================================================

    ''' <summary>Re-applies the scheme. Colours pinned in the designer are left alone; the rest take the palette.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            _autoBack = p.InputBackColor
            _autoFore = p.InputTextColor
            _autoBorder = p.InputBorderColor
            _autoHover = p.ButtonHoverColor
            _autoArrow = p.TextDimColor
            _autoSelBack = p.AccentColor
            _autoSelFore = p.AccentTextColor

            ' MyBase, not Me: the theme writing a colour must not pass for an operator's choice.
            If Not _backColorPinned Then MyBase.BackColor = _autoBack
            If Not _foreColorPinned Then MyBase.ForeColor = _autoFore
            ApplyEditColors()
            If _list IsNot Nothing AndAlso Not _list.IsDisposed Then _list.Invalidate()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.ApplyTheme", ex)
        End Try
    End Sub

    ' The text box paints its own rectangle: it takes the face's colours.
    Private Sub ApplyEditColors()
        ' The constructor writes BackColor/ForeColor before the text box exists (and the base
        ' class can raise the change events during construction too).
        If _edit Is Nothing Then Return
        _edit.BackColor = BackColor
        _edit.ForeColor = If(Enabled, ForeColor, ThemeManager.Current.Palette.DisabledTextColor)
    End Sub

    ' The effective radius, in DPI-scaled px.
    Private Function EffectiveRadius() As Integer
        Dim logical As Integer = If(_cornerRadius >= 0, _cornerRadius, ThemeManager.Current.Style.CornerRadius)
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logical))
    End Function

    ' =====================================================================
    ' LAYOUT: the text box sits exactly where the painted caption would be
    ' =====================================================================

    ''' <summary>
    ''' Slice 0094 (for <see cref="KBotDataView"/>): no frame, no arrow, free height, text flush
    ''' with the left edge. The grid paints the cell and its chevron and places this control on the
    ''' cell text, exactly like its text editor.
    ''' </summary>
    Friend Property CellEditorMode As Boolean
        Get
            Return _cellEditorMode
        End Get
        Set(value As Boolean)
            _cellEditorMode = value
            LayoutEdit()
            Invalidate()
        End Set
    End Property

    ' The rectangle the text goes in (painted caption or text box), full height.
    Private Function TextArea() As Rectangle
        Dim left As Integer = If(_cellEditorMode, 0, ThemeShapes.ScaleDpi(Me, 8))
        Dim right As Integer = If(_cellEditorMode, ClientSize.Width, ArrowRect().Left)
        Return New Rectangle(left, 0, Math.Max(0, right - left), ClientSize.Height)
    End Function

    ' The arrow area: a square on the right, as wide as the control is tall (capped).
    Private Function ArrowRect() As Rectangle
        If _cellEditorMode Then Return Rectangle.Empty
        Dim w As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, 24), Math.Max(1, Width \ 3))
        Return New Rectangle(Width - w, 0, w, Height)
    End Function

    ''' <summary>
    ''' Puts the text box on the painted caption's pixels: as wide as the text area, one line
    ''' high, vertically centred the way <c>TextFormatFlags.VerticalCenter</c> centres (GDI rounds
    ''' the spare height UP), and with the edit control's own margins replaced by the glyph
    ''' padding <c>TextRenderer</c> puts around a line -- the same recipe as the grid's text
    ''' editor (slice 0085), so a typed and a painted caption start on the same pixel.
    ''' </summary>
    Private Sub LayoutEdit()
        Try
            If Not IsHandleCreated Then Return
            Dim area As Rectangle = TextArea()
            Dim lineH As Integer
            Dim padLeft As Integer
            Dim padRight As Integer
            MeasureTextPadding(lineH, padLeft, padRight)

            Dim h As Integer = Math.Max(1, Math.Min(lineH, ClientSize.Height))
            Dim top As Integer = (ClientSize.Height - h + 1) \ 2 + ThemeShapes.ScaleDpi(Me, _textOffsetY)
            top = Math.Max(0, Math.Min(top, ClientSize.Height - h))
            _edit.Bounds = New Rectangle(area.Left, top, Math.Max(1, area.Width), h)

            ' WM_SETFONT resets the margins, so this runs after every font change too.
            If _edit.IsHandleCreated Then
                Dim margins As Integer = (padRight << 16) Or (padLeft And &HFFFF)
                SendMessage(_edit.Handle, EM_SETMARGINS,
                            New IntPtr(EC_LEFTMARGIN Or EC_RIGHTMARGIN), New IntPtr(margins))
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.LayoutEdit", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Line height and the left/right glyph padding <c>TextRenderer.DrawText</c> puts around a
    ''' single line when <c>NoPadding</c> is NOT set. Measured on this control's own device
    ''' context, so it is in device pixels at the current DPI; the renderer's rule is left =
    ''' ceil(h/6), the rest on the right.
    ''' </summary>
    Private Sub MeasureTextPadding(ByRef lineH As Integer, ByRef padLeft As Integer, ByRef padRight As Integer)
        Const flags As TextFormatFlags = TextFormatFlags.SingleLine
        Dim big As New Size(Integer.MaxValue, Integer.MaxValue)
        Using g As Graphics = CreateGraphics()
            Dim bare As Size = TextRenderer.MeasureText(g, "Wg", Font, big, flags Or TextFormatFlags.NoPadding)
            Dim padded As Size = TextRenderer.MeasureText(g, "Wg", Font, big, flags)
            lineH = Math.Max(1, bare.Height)
            Dim total As Integer = Math.Max(0, padded.Width - bare.Width)
            padLeft = Math.Min(total, CInt(Math.Ceiling(lineH / 6.0)))
            padRight = total - padLeft
        End Using
    End Sub

    ' The text box is on screen only while typing is possible; disabled, the caption is painted.
    Private Sub UpdateEditVisibility()
        If _edit Is Nothing Then Return
        Dim show As Boolean = _editable AndAlso Enabled
        Dim hadFocus As Boolean = ContainsFocus
        _edit.Visible = show
        SetStyle(ControlStyles.Selectable, Not show)
        If show Then LayoutEdit()
        If hadFocus Then
            If show Then _edit.Focus() Else Focus()
        End If
    End Sub

    ''' <summary>Outside a grid the height follows the font, like the native control did.</summary>
    Protected Overrides Sub SetBoundsCore(x As Integer, y As Integer, width As Integer, height As Integer,
                                          specified As BoundsSpecified)
        If Not _cellEditorMode Then height = PreferredHeight
        MyBase.SetBoundsCore(x, y, width, height, specified)
    End Sub

    Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
        Return New Size(Math.Max(Width, 1), PreferredHeight)
    End Function

    ' =====================================================================
    ' PAINTING
    ' =====================================================================

    ''' <summary>The face: rounded background + outline + arrow, and the caption when no text box shows it.</summary>
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            If _updateCount > 0 Then Return
            Dim g As Graphics = e.Graphics

            If _cellEditorMode Then
                g.Clear(BackColor)
            Else
                g.SmoothingMode = SmoothingMode.AntiAlias
                ' With the text box showing, it repaints its own rectangle with its own
                ' background, so a hover wash would only show as a thick frame: hover moves to
                ' the outline instead.
                Dim hot As Boolean = _hovered OrElse DroppedDown
                Dim fill As Color = If(hot AndAlso Not _edit.Visible, EffectiveHoverColor, BackColor)
                Dim outline As Color = If(ContainsFocus OrElse (hot AndAlso _edit.Visible),
                                          ThemeManager.Current.Palette.FocusRingColor, EffectiveBorderColor)
                Dim rect As New Rectangle(0, 0, Width - 1, Height - 1)
                Using path As GraphicsPath = ThemeShapes.RoundedRect(rect, EffectiveRadius())
                    Using b As New SolidBrush(fill)
                        g.FillPath(b, path)
                    End Using
                    Using pen As New Pen(outline)
                        g.DrawPath(pen, path)
                    End Using
                End Using
                DrawArrow(g, ArrowRect())
            End If

            ' The text belongs to the text box while it shows -- we would draw it twice.
            If _edit.Visible Then Return
            Dim caption As String = Text
            If caption.Length = 0 Then Return
            TextRenderer.DrawText(g, caption, Font, TextArea(),
                                  If(Enabled, ForeColor, ThemeManager.Current.Palette.DisabledTextColor),
                                  HorizontalFlags() Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or
                                  TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        Catch ex As Exception
            ' Painting boundary: a throw from here would bring the process down.
            GlobalErrorLog.Write("KBotComboBox.OnPaint", ex)
        End Try
    End Sub

    Private Function HorizontalFlags() As TextFormatFlags
        Select Case _textAlign
            Case HorizontalAlignment.Right
                Return TextFormatFlags.Right
            Case HorizontalAlignment.Center
                Return TextFormatFlags.HorizontalCenter
            Case Else
                Return TextFormatFlags.Left
        End Select
    End Function

    ' The arrow: a "v" of two lines, not a filled triangle -- it reads the same at any DPI.
    Private Sub DrawArrow(g As Graphics, area As Rectangle)
        Dim half As Integer = ThemeShapes.ScaleDpi(Me, 4)
        Dim cx As Single = area.Left + area.Width / 2.0F
        Dim cy As Single = area.Top + area.Height / 2.0F
        Using pen As New Pen(If(Enabled, EffectiveArrowColor, ThemeManager.Current.Palette.DisabledTextColor),
                             ThemeShapes.ScaleDpi(Me, 2))
            pen.StartCap = LineCap.Round
            pen.EndCap = LineCap.Round
            g.DrawLine(pen, cx - half, cy - half \ 2, cx, cy + half \ 2)
            g.DrawLine(pen, cx, cy + half \ 2, cx + half, cy - half \ 2)
        End Using
    End Sub

    ' =====================================================================
    ' STATE / INVALIDATION
    ' =====================================================================

    ' MouseEnter / MouseLeave are reported for the WHOLE combo, text box included: moving from
    ' the frame onto the text box is not leaving the combo. KBotToolTip hangs off these two, so
    ' without this a tooltip would vanish the moment the cursor reached the text.
    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        EnterCombo(e)
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        LeaveCombo(e)
    End Sub

    Private Sub Edit_MouseEnter(sender As Object, e As EventArgs)
        EnterCombo(e)
    End Sub

    Private Sub Edit_MouseLeave(sender As Object, e As EventArgs)
        LeaveCombo(e)
    End Sub

    Private Sub EnterCombo(e As EventArgs)
        Try
            _hovered = True
            Invalidate()
            If _mouseInside Then Return
            _mouseInside = True
            MyBase.OnMouseEnter(e)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.EnterCombo", ex)
        End Try
    End Sub

    Private Sub LeaveCombo(e As EventArgs)
        Try
            Dim inside As Boolean = ClientRectangle.Contains(PointToClient(MousePosition))
            _hovered = inside
            Invalidate()
            If inside OrElse Not _mouseInside Then Return
            _mouseInside = False
            MyBase.OnMouseLeave(e)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.LeaveCombo", ex)
        End Try
    End Sub

    ' A press in the text box is a press on the combo for whoever listens to its MouseDown (a
    ' tooltip hides on it), in the combo's coordinates. It does not toggle the list: that is
    ' the arrow's job.
    Private Sub Edit_MouseDown(sender As Object, e As MouseEventArgs)
        Try
            MyBase.OnMouseDown(New MouseEventArgs(e.Button, e.Clicks, e.X + _edit.Left, e.Y + _edit.Top, e.Delta))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.Edit_MouseDown", ex)
        End Try
    End Sub

    Private Sub Edit_FocusChanged(sender As Object, e As EventArgs)
        Invalidate()
    End Sub

    Private Sub Edit_HandleCreated(sender As Object, e As EventArgs)
        LayoutEdit()
    End Sub

    ''' <summary>A click on the arrow -- or anywhere on a face that cannot be typed in -- opens or
    ''' closes the list.</summary>
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            If _edit.Visible Then
                If Not _edit.Focused Then _edit.Focus()
                If Not ArrowRect().Contains(e.Location) Then Return
            ElseIf Not Focused Then
                Focus()
            End If
            DroppedDown = Not DroppedDown
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        ScrollListByWheel(e.Delta)
    End Sub

    Private Sub Edit_MouseWheel(sender As Object, e As MouseEventArgs)
        ScrollListByWheel(e.Delta)
    End Sub

    ' With the list open the wheel scrolls it; closed, it changes nothing (no accidental picks).
    Private Sub ScrollListByWheel(delta As Integer)
        Try
            If Not DroppedDown Then Return
            _list.ScrollBy(-Math.Sign(delta) * Math.Max(1, SystemInformation.MouseWheelScrollLines))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.ScrollListByWheel", ex)
        End Try
    End Sub

    ''' <summary>The text box takes the focus the control is given while typing is possible.</summary>
    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        Try
            If _edit.Visible Then _edit.Focus()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnGotFocus", ex)
        End Try
    End Sub

    Protected Overrides Sub OnLostFocus(e As EventArgs)
        MyBase.OnLostFocus(e)
        Invalidate()
    End Sub

    ''' <summary>
    ''' Leaving the field is when the verdict on the typed text is given -- BEFORE the host's Leave
    ''' handlers run, so they read the committed value. The list, which never takes the focus,
    ''' goes with it.
    ''' </summary>
    Protected Overrides Sub OnLeave(e As EventArgs)
        CloseDropDown()
        CommitText()
        MyBase.OnLeave(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        LayoutEdit()
        RepositionDropDown()
    End Sub

    Protected Overrides Sub OnLocationChanged(e As EventArgs)
        MyBase.OnLocationChanged(e)
        RepositionDropDown()
    End Sub

    Protected Overrides Sub OnEnabledChanged(e As EventArgs)
        MyBase.OnEnabledChanged(e)
        If Not Enabled Then CloseDropDown()
        ApplyEditColors()
        UpdateEditVisibility()
        Invalidate()
    End Sub

    Protected Overrides Sub OnBackColorChanged(e As EventArgs)
        MyBase.OnBackColorChanged(e)
        ApplyEditColors()
    End Sub

    Protected Overrides Sub OnForeColorChanged(e As EventArgs)
        MyBase.OnForeColorChanged(e)
        ApplyEditColors()
    End Sub

    Protected Overrides Sub OnTabStopChanged(e As EventArgs)
        MyBase.OnTabStopChanged(e)
        If _edit IsNot Nothing Then _edit.TabStop = TabStop
    End Sub

    ''' <summary>The row height and the box height follow the font.</summary>
    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        Try
            If Not _cellEditorMode Then Height = PreferredHeight
            LayoutEdit()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnFontChanged", ex)
        End Try
    End Sub

    ''' <summary>A DPI move changes the line height in device pixels -- lay out again.</summary>
    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        Try
            If Not _cellEditorMode Then Height = PreferredHeight
            LayoutEdit()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnDpiChangedAfterParent", ex)
        End Try
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Try
            If Not _cellEditorMode Then Height = PreferredHeight
            UpdateEditVisibility()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnHandleCreated", ex)
        End Try
    End Sub

End Class

''' <summary>
''' The text box inside <see cref="KBotComboBox"/>. Its only difference from a plain one: while the
''' combo's list is open, Enter, Escape and the navigation keys are INPUT keys, so a dialog's
''' AcceptButton / CancelButton does not take them first.
''' </summary>
Friend NotInheritable Class KBotComboEdit
    Inherits TextBox

    Private ReadOnly _owner As KBotComboBox

    Public Sub New(owner As KBotComboBox)
        _owner = owner
    End Sub

    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        If _owner.WantsKey(keyData) Then Return True
        Return MyBase.IsInputKey(keyData)
    End Function
End Class
