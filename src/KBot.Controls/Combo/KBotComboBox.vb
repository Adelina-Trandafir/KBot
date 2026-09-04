Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The K-BOT drop-down: the CLOSED face is painted by us (rounded rectangle, 1 px outline, a GDI+
''' arrow) and the list rows are owner-drawn. A stock <c>ComboBox</c> ignores <c>BackColor</c> on
''' its closed face — Windows themes it — so on a dark scheme it stayed a white rectangle.
'''
''' <para>It INHERITS <c>ComboBox</c>, not <c>Control</c>. Deliberate: hosts already bind
''' <c>DataSource</c>, <c>Items</c>, <c>SelectedItem</c>, <c>DisplayMember</c> and
''' <c>SelectedIndexChanged</c> (the An/SS pair on MainForm, for two). Rewriting from scratch would
''' have meant reimplementing data binding to arrive at the SAME place.</para>
'''
''' <para>The colour contract is the house one (C1): <c>Color.Empty</c> = "from the theme", and any
''' colour set in the designer wins. <c>BackColor</c>/<c>ForeColor</c>/<c>Font</c> carry a pinned
''' flag plus the <c>ShouldSerialize*</c>/<c>Reset*</c> pair, otherwise Visual Studio would freeze
''' whatever <see cref="ApplyTheme"/> wrote into the host .Designer.vb and nobody could tell a
''' choice from an accident.</para>
'''
''' <para><b>Typing.</b> <see cref="Editable"/> opens the face to the keyboard: the control switches
''' to <c>DropDown</c> and Windows puts a native EDIT child inside it, which draws the text itself.
''' That child is NOT left unthemed — <c>WM_CTLCOLOREDIT</c> comes back reflected to the control, so
''' the EDIT gets our own <c>BackColor</c>/<c>ForeColor</c>/<c>Font</c> — and we keep painting the
''' rounded background, the outline and the arrow around it, with the EDIT's inner margins lined up
''' to the same padding as the list rows (see <see cref="AlignEditText"/>). Vertically the child
''' is positioned from the font's own line height and a MEASURED internal offset, so the typed text
''' centres itself for any typeface at any DPI, with no constant to tune. What is lost is the
''' hover wash across the WHOLE face (the EDIT repaints its own rectangle with its own background),
''' so in editable mode hover shows on the outline instead.</para>
'''
''' <para><b><see cref="LimitToList"/></b> decides what happens to text that is not in the list: off
''' = it is kept (a free field with suggestions), on = the field goes back to the last accepted
''' value. The verdict is given when the field is left and on Enter — or whenever the host calls
''' <see cref="CommitText"/>.</para>
'''
''' <para>The one value still refused is <c>ComboBoxStyle.Simple</c>: there the list is a permanent
''' panel we neither draw nor theme. It THROWS — no silent no-op (C3).</para>
''' </summary>
<ToolboxItem(True)>
<DefaultEvent("SelectedIndexChanged")>
Public Class KBotComboBox
    Inherits ComboBox
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

    ' "The operator pinned this" flags — see ShouldSerializeBackColor.
    Private _backColorPinned As Boolean = False
    Private _foreColorPinned As Boolean = False
    Private _fontPinned As Boolean = False

    Private _hovered As Boolean = False
    Private _darkList As Boolean = False

    ' -- Typing in the box --------------------------------------------------------
    Private _editable As Boolean = False
    Private _limitToList As Boolean = True

    ' -- Vertical centring of the native EDIT's text ------------------------------
    ' A single-line EDIT draws its line at the TOP of its own client rectangle, so the glyphs land
    ' at EDIT.Top + delta and the EDIT's HEIGHT plays no part. Both terms are measured from Windows
    ' (the font's tmHeight on the EDIT's own DC, and delta through EM_POSFROMCHAR), never guessed --
    ' which is what makes the result hold for any font at any DPI, with no per-font constant.
    Private _textOffsetY As Integer = 0
    Private _lineHeight As Integer = 0          ' device px, 0 = not measured yet
    Private _editDelta As Integer = 0           ' the EDIT's internal top offset, device px
    Private _deltaMeasured As Boolean = False
    Private _aligning As Boolean = False        ' re-entrancy guard

    ' The EDIT is grown DOWNWARDS only: its top is fixed by where the text has to start, so the
    ' extra height can only go below. Invisible, because the EDIT paints with our own BackColor.
    Private Const EDIT_BLEED As Integer = 2     ' logical px
    Private Const EDIT_INSET As Integer = 1     ' device px, keeps the 1 px outline unpainted

    ' The last ACCEPTED text: what the field goes back to when LimitToList is on and the operator
    ' typed something that is not in the list. Kept up to date from OnSelectedIndexChanged, except
    ' while CommitText is moving the selection itself (or it would overwrite its own result).
    Private _lastAcceptedText As String = String.Empty
    Private _committing As Boolean = False

    ' The colour messages the native EDIT child asks its parent (us) to answer. See WndProc.
    Private Const WM_CTLCOLOREDIT As Integer = &H133
    Private Const WM_CTLCOLORSTATIC As Integer = &H138

    ' The three messages on which the combo repositions its own EDIT child. Without re-aligning
    ' after them, the rectangle we set is silently overwritten. No recursion: SetComboEditBounds
    ' moves the CHILD window, so none of the three comes back to us.
    Private Const WM_WINDOWPOSCHANGED As Integer = &H47
    Private Const WM_SETFONT As Integer = &H30
    Private Const CB_SETITEMHEIGHT As Integer = &H153

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)

        ' These are pinned by the constructor, so they would have landed in the host .Designer.vb —
        ' which is why the ShouldSerialize* pairs below keep them out of serialization.
        MyBase.DropDownStyle = ComboBoxStyle.DropDownList
        MyBase.DrawMode = DrawMode.OwnerDrawFixed
        MyBase.FlatStyle = FlatStyle.Flat
    End Sub

    ' =====================================================================
    ' TYPING IN THE BOX
    ' =====================================================================

    ''' <summary>
    ''' On = the operator can TYPE in the box (<c>DropDown</c> style); off = they can only pick from
    ''' the list (<c>DropDownList</c>, the behaviour this control had before). Switching recreates
    ''' the control's HWND, so the list window theme is asked for again in
    ''' <see cref="OnHandleCreated"/>.
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
            MyBase.DropDownStyle = If(value, ComboBoxStyle.DropDown, ComboBoxStyle.DropDownList)
            AlignEditText()
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
    ''' An optical nudge of the text typed in the box, in logical px (scaled at runtime). 0 (the
    ''' default) leaves the text on the exact vertical centre, computed from the font's own line
    ''' height -- there is no per-font, per-DPI constant to tune any more. Positive moves it down,
    ''' negative up; it is for a typeface whose glyphs sit visibly off their line box, NOT for
    ''' centring, which now happens on its own.
    ''' </summary>
    <Category("K-BOT Combo")>
    <Description("Optical nudge of the typed text, px @96dpi. Positive = down, negative = up. 0 = exact vertical centre.")>
    <DefaultValue(0)>
    Public Property TextOffsetY As Integer
        Get
            Return _textOffsetY
        End Get
        Set(value As Integer)
            _textOffsetY = value
            AlignEditText()
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Give the verdict on the text typed NOW, without waiting for the field to be left. A host
    ''' calls it when it reads the value from a button ("Salveaza") the operator can reach without
    ''' moving the focus. Idempotent: calling it twice changes nothing the second time.
    ''' </summary>
    Public Sub CommitText()
        Try
            If Not _editable Then Return

            Dim typed As String = If(Text, String.Empty)
            Dim match As Integer = If(typed.Length = 0, -1, FindStringExact(typed))

            _committing = True
            Try
                If match >= 0 Then
                    ' The text IS in the list: the selection follows it, with the list's spelling.
                    If SelectedIndex <> match Then MyBase.SelectedIndex = match
                    _lastAcceptedText = If(Text, String.Empty)
                ElseIf _limitToList Then
                    ' Refused: back to the last accepted value (empty = empty field, no selection).
                    Dim target As Integer = If(_lastAcceptedText.Length = 0, -1, FindStringExact(_lastAcceptedText))
                    If SelectedIndex <> target Then MyBase.SelectedIndex = target
                    ' The index can already BE the right one while the box shows something else —
                    ' typing does not move it. Writing the same index back is a no-op, so the text
                    ' has to be put back by hand, or the refused text would stay on screen.
                    If Not String.Equals(Text, _lastAcceptedText, StringComparison.Ordinal) Then
                        MyBase.Text = _lastAcceptedText
                    End If
                Else
                    ' Accepted as free text: the selection is no longer allowed to claim a row.
                    If SelectedIndex >= 0 Then
                        MyBase.SelectedIndex = -1
                        If Not String.Equals(Text, typed, StringComparison.Ordinal) Then MyBase.Text = typed
                    End If
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

    ''' <summary>
    ''' Line the native EDIT up with the text we draw ourselves: horizontally through its inner
    ''' margins, vertically by moving the child window.
    '''
    ''' <para>The margins are computed from the REAL rectangle of the box (not from a guessed
    ''' constant), so they come out right at any DPI: on the left whatever is missing up to our
    ''' padding, on the right whatever would otherwise slide under the arrow.</para>
    '''
    ''' <para>The vertical half is the one that used to be a hand-tuned constant. A single-line
    ''' EDIT draws its line at the TOP of its own client rectangle, so the glyphs land at
    ''' <c>EDIT.Top + delta</c> and the EDIT's height plays no part at all. Both terms are
    ''' MEASURED: the font's line height on the EDIT's own DC, and delta through
    ''' <c>EM_POSFROMCHAR</c>. The second pass is there because delta can only be read back once
    ''' the child has been moved -- it re-sets the bounds at most one more time, never in a
    ''' loop.</para>
    ''' </summary>
    Private Sub AlignEditText()
        If _aligning Then Return
        Try
            _aligning = True
            If Not _editable OrElse Not IsHandleCreated Then Return
            Dim item As Rectangle = NativeMethods.GetComboEditBounds(Me)
            If item.IsEmpty Then Return
            Dim padX As Integer = ThemeShapes.ScaleDpi(Me, 8)
            NativeMethods.SetComboEditMargins(Me,
                                              Math.Max(0, padX - item.Left),
                                              Math.Max(0, item.Right - ArrowRect().Left))

            EnsureLineHeight()
            ' No line height, no centring: leave Windows' own bounds alone rather than guess.
            If _lineHeight <= 0 Then Return

            Dim desiredTextTop As Integer =
                ((ClientSize.Height - _lineHeight) \ 2) + ThemeShapes.ScaleDpi(Me, _textOffsetY)
            ApplyEditBounds(item, desiredTextTop)

            ' Where did the glyphs ACTUALLY land? If delta is not what we believed, take the real
            ' value and set the bounds once more with it.
            Dim y As Integer = NativeMethods.GetComboEditTextTop(Me)
            If y = Integer.MinValue Then Return
            _deltaMeasured = True
            If y <> _editDelta Then
                _editDelta = y
                ApplyEditBounds(NativeMethods.GetComboEditBounds(Me), desiredTextTop)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.AlignEditText", ex)
        Finally
            _aligning = False
        End Try
    End Sub

    ''' <summary>
    ''' The font's line height on the EDIT's own DC, in device pixels. Cached until the font, the
    ''' HWND or the DPI changes -- the three things that can make it a different number.
    ''' </summary>
    Private Sub EnsureLineHeight()
        If _lineHeight > 0 Then Return
        _lineHeight = NativeMethods.GetComboEditLineHeight(Me)
    End Sub

    ''' <summary>
    ''' Puts the EDIT where <paramref name="desiredTextTop"/> asks for, given the delta believed
    ''' now. Only Top and Height are written -- Left/Width stay exactly as Windows computed them.
    ''' The height is grown DOWNWARDS only (the top is fixed by the text) and clamped inside the
    ''' 1 px outline; when the box is too short for both, the top wins and the text is clipped at
    ''' the bottom rather than dragged off its line.
    ''' </summary>
    Private Sub ApplyEditBounds(item As Rectangle, desiredTextTop As Integer)
        If item.IsEmpty Then Return
        Dim bleed As Integer = ThemeShapes.ScaleDpi(Me, EDIT_BLEED)
        Dim height As Integer = _lineHeight + 2 * bleed
        Dim top As Integer = desiredTextTop - _editDelta

        Dim maxBottom As Integer = ClientSize.Height - EDIT_INSET
        If top < EDIT_INSET Then top = EDIT_INSET
        If top + height > maxBottom Then height = maxBottom - top
        If height < 1 Then height = 1

        NativeMethods.SetComboEditBounds(Me, New Rectangle(item.Left, top, item.Width, height))
    End Sub

    ' =====================================================================
    ' INHERITED PROPERTIES WITH A PINNED FLAG
    ' =====================================================================

    ''' <summary>The background of the closed face AND of the list; not pinned here, it follows the theme.</summary>
    <Category("K-BOT Combo Colors")>
    <Description("The background of the closed face and of the list; not pinned here, it follows the theme.")>
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
    ''' ever been WRITTEN — including by <see cref="ApplyTheme"/>. Without this pair, Visual Studio
    ''' would write a "cbo.BackColor = ..." nobody chose into the host form, and on reload that line
    ''' would come back through the setter above and PIN the colour forever. The truth is the flag,
    ''' not Control's property bag.
    ''' </summary>
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColorPinned
    End Function

    ' The flag goes out AFTER the colour is written: ResetBackColor goes through the VIRTUAL setter,
    ' i.e. ours, which would light it again (the trap caught in slice 0027 on ResetFont).
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

    ''' <summary>The counterpart of <see cref="ShouldSerializeBackColor"/>, for the same reason.</summary>
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

    ''' <summary>The flag goes out AFTER the base reset — <c>Control.ResetFont</c> writes through the virtual setter.</summary>
    Public Overrides Sub ResetFont()
        MyBase.ResetFont()
        _fontPinned = False
        Invalidate()
    End Sub

    ' =====================================================================
    ' OWN PROPERTIES (Color.Empty = "from the theme")
    ' =====================================================================

    <Category("K-BOT Combo Colors")>
    <Description("The background of the closed face under the cursor. Empty = from the theme.")>
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
    <Description("The 1 px outline of the closed face. Empty = from the theme.")>
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

    ''' <summary>Corner radius of the closed face, in logical px. -1 = from the theme (Style.CornerRadius).</summary>
    <Category("K-BOT Combo")>
    <Description("Corner radius of the closed face, px @96dpi. -1 = from the theme, 0 = square.")>
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
    ' INHERITED PROPERTIES WE PIN OURSELVES — kept out of serialization
    ' =====================================================================

    ''' <summary>
    ''' <c>DropDownList</c> or <c>DropDown</c> — the same choice as <see cref="Editable"/>, written
    ''' in <c>ComboBox</c>'s own words; whoever writes it here moves the flag too, so there is a
    ''' single source of truth. <c>Simple</c> THROWS: there the list is a permanent panel we neither
    ''' draw nor theme, and the house rule forbids the silent no-op (C3).
    ''' </summary>
    Public Shadows Property DropDownStyle As ComboBoxStyle
        Get
            Return MyBase.DropDownStyle
        End Get
        Set(value As ComboBoxStyle)
            If value <> ComboBoxStyle.DropDownList AndAlso value <> ComboBoxStyle.DropDown Then
                Throw New ArgumentException(
                    "KBotComboBox accepts only ComboBoxStyle.DropDownList or DropDown (the permanent list of the Simple style cannot be themed).",
                    NameOf(value))
            End If
            Editable = (value = ComboBoxStyle.DropDown)
        End Set
    End Property

    ''' <summary>Derived from <see cref="Editable"/> so it is not serialized (a single source).</summary>
    Public Function ShouldSerializeDropDownStyle() As Boolean
        Return False
    End Function

    ''' <summary>Owner-draw is mandatory (we paint the rows) so it is not serialized.</summary>
    Public Function ShouldSerializeDrawMode() As Boolean
        Return False
    End Function

    ''' <summary>Derived from the font (see <see cref="OnFontChanged"/>) so it is not serialized.</summary>
    Public Function ShouldSerializeItemHeight() As Boolean
        Return False
    End Function

    ''' <summary>Pinned by the constructor so it is not serialized.</summary>
    Public Function ShouldSerializeFlatStyle() As Boolean
        Return False
    End Function

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
            _darkList = scheme.IsDark

            ' MyBase, not Me: the theme writing a colour must not pass for an operator's choice.
            If Not _backColorPinned Then MyBase.BackColor = _autoBack
            If Not _foreColorPinned Then MyBase.ForeColor = _autoFore

            ' The drop-down list window is a separate HWND: neither our painting nor the theme
            ' traversal reaches it. Only uxtheme darkens its scrollbar and its frame.
            NativeMethods.ApplyWindowTheme(Me, If(_darkList, "DarkMode_CFD", "Explorer"))

            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.ApplyTheme", ex)
        End Try
    End Sub

    ' The effective radius, in DPI-scaled px.
    Private Function EffectiveRadius() As Integer
        Dim logical As Integer = If(_cornerRadius >= 0, _cornerRadius, ThemeManager.Current.Style.CornerRadius)
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logical))
    End Function

    ' =====================================================================
    ' PAINTING
    ' =====================================================================

    ''' <summary>The CLOSED face: rounded background + outline + text + arrow.</summary>
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            ' In editable mode the native EDIT repaints its own rectangle with its own background,
            ' so a hover wash would only show as a thick frame. There hover moves to the outline.
            Dim hot As Boolean = _hovered OrElse DroppedDown
            Dim fill As Color = If(hot AndAlso Not _editable, EffectiveHoverColor, BackColor)
            Dim outline As Color = If(Focused OrElse (hot AndAlso _editable),
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

            Dim arrowArea As Rectangle = ArrowRect()
            DrawArrow(g, arrowArea)

            ' The text belongs to the native EDIT when typing is allowed — we would draw it twice.
            If _editable Then Return

            Dim padX As Integer = ThemeShapes.ScaleDpi(Me, 8)
            ' ClientSize.Height, not Height: the EDIT is positioned in CLIENT coordinates, and the
            ' painted caption has to sit on the same line as the typed text.
            Dim textArea As New Rectangle(padX, 0, Math.Max(0, arrowArea.Left - padX), ClientSize.Height)
            Dim caption As String = SelectedCaption()
            If caption.Length > 0 Then
                TextRenderer.DrawText(g, caption, Font, textArea,
                                      If(Enabled, ForeColor, ThemeManager.Current.Palette.DisabledTextColor),
                                      TextFormatFlags.VerticalCenter Or TextFormatFlags.Left Or
                                      TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
            End If
        Catch ex As Exception
            ' Painting boundary: a throw from here would bring the process down.
            GlobalErrorLog.Write("KBotComboBox.OnPaint", ex)
        End Try
    End Sub

    ''' <summary>One list row: background (highlighted or not) + text.</summary>
    Protected Overrides Sub OnDrawItem(e As DrawItemEventArgs)
        Try
            If e.Index < 0 OrElse e.Index >= Items.Count Then Return

            Dim selected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected
            Dim back As Color = If(selected, EffectiveSelectionBackColor, BackColor)
            Dim fore As Color = If(selected, EffectiveSelectionForeColor, ForeColor)

            Using b As New SolidBrush(back)
                e.Graphics.FillRectangle(b, e.Bounds)
            End Using

            Dim padX As Integer = ThemeShapes.ScaleDpi(Me, 8)
            Dim area As New Rectangle(e.Bounds.Left + padX, e.Bounds.Top,
                                      Math.Max(0, e.Bounds.Width - padX), e.Bounds.Height)
            TextRenderer.DrawText(e.Graphics, CaptionOf(Items(e.Index)), Font, area, fore,
                                  TextFormatFlags.VerticalCenter Or TextFormatFlags.Left Or
                                  TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnDrawItem", ex)
        End Try
    End Sub

    ' The arrow area: a square on the right, as wide as the control is tall (capped).
    Private Function ArrowRect() As Rectangle
        Dim w As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, 24), Math.Max(1, Width \ 3))
        Return New Rectangle(Width - w, 0, w, Height)
    End Function

    ' The arrow: a "v" of two lines, not a filled triangle — it reads the same at any DPI.
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

    ' The text shown on the closed face. Works with DataSource (DisplayMember) too, not only Items.
    Private Function SelectedCaption() As String
        If SelectedIndex < 0 Then Return If(Text, String.Empty)
        Return CaptionOf(SelectedItem)
    End Function

    ' Honours DisplayMember when data-bound; ToString() otherwise.
    Private Function CaptionOf(item As Object) As String
        If item Is Nothing Then Return String.Empty
        If Not String.IsNullOrEmpty(DisplayMember) Then
            Dim prop = TypeDescriptor.GetProperties(item)(DisplayMember)
            If prop IsNot Nothing Then
                Dim value As Object = prop.GetValue(item)
                Return If(value Is Nothing, String.Empty, value.ToString())
            End If
        End If
        Return item.ToString()
    End Function

    ' =====================================================================
    ' STATE / INVALIDATION
    ' =====================================================================
    ' We paint the closed face ourselves, so it has to be repainted on every change the native
    ' control would have handled on its own: hover, focus, open/close, a new selection.

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        _hovered = True
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _hovered = False
        Invalidate()
    End Sub

    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnLostFocus(e As EventArgs)
        MyBase.OnLostFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnDropDown(e As EventArgs)
        MyBase.OnDropDown(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnDropDownClosed(e As EventArgs)
        MyBase.OnDropDownClosed(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnSelectedIndexChanged(e As EventArgs)
        MyBase.OnSelectedIndexChanged(e)
        ' Picking from the list is by definition an accepted value — it becomes the fallback.
        If Not _committing Then
            _lastAcceptedText = If(SelectedIndex >= 0, CaptionOf(SelectedItem), String.Empty)
        End If
        Invalidate()
    End Sub

    ''' <summary>
    ''' Leaving the field is when the verdict on the typed text is given. <c>Leave</c>, not
    ''' <c>LostFocus</c>: the first comes from the container moving its active control, so it does
    ''' not fire when the list window takes the focus, and it lands BEFORE the click that caused it.
    ''' </summary>
    Protected Overrides Sub OnLeave(e As EventArgs)
        MyBase.OnLeave(e)
        CommitText()
    End Sub

    ''' <summary>
    ''' Enter gives the same verdict without moving the focus: on a form with a default button the
    ''' operator can confirm without ever leaving the field.
    ''' </summary>
    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            If e.KeyCode = Keys.Enter AndAlso _editable AndAlso Not DroppedDown Then CommitText()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnKeyDown", ex)
        End Try
    End Sub

    ''' <summary>The arrow moves with the width, and so does the box's right margin.</summary>
    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        AlignEditText()
    End Sub

    Protected Overrides Sub OnEnabledChanged(e As EventArgs)
        MyBase.OnEnabledChanged(e)
        Invalidate()
    End Sub

    ''' <summary>Row height follows the font — otherwise owner-draw clips the text.</summary>
    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        Try
            MyBase.ItemHeight = Math.Max(1, Font.Height + ThemeShapes.ScaleDpi(Me, 6))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnFontChanged", ex)
        End Try
        ' A new font means a new line height AND a new internal offset: measure both again.
        _lineHeight = 0
        _deltaMeasured = False
        AlignEditText()
    End Sub

    ''' <summary>A DPI move changes the line height in device pixels -- measure it again.</summary>
    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        _lineHeight = 0
        _deltaMeasured = False
        AlignEditText()
        Invalidate()
    End Sub

    ''' <summary>
    ''' The delta probe needs a character to ask about, so it cannot succeed while the box is
    ''' empty -- this is where it gets its first chance. The "already measured" flag keeps it to
    ''' once per font/HWND generation instead of once per keystroke.
    ''' </summary>
    Protected Overrides Sub OnTextChanged(e As EventArgs)
        MyBase.OnTextChanged(e)
        If _editable AndAlso Not _deltaMeasured AndAlso IsHandleCreated Then AlignEditText()
    End Sub

    ''' <summary>
    ''' The one message that keeps the editable face from being half-themed.
    '''
    ''' <para>The EDIT child's parent is THIS control, not the form, so <c>WM_CTLCOLOREDIT</c>
    ''' arrives here. Nobody answers it by default — WinForms reflects a WM_CTLCOLOR* back to the
    ''' managed control that owns the sending HWND, and the EDIT is not a managed control, so it
    ''' falls through to <c>DefWindowProc</c> and the box comes out in the SYSTEM colours: white
    ''' with black text, which is precisely the white rectangle this whole class exists to kill.
    ''' Verified on screen, not assumed. A disabled EDIT sends <c>WM_CTLCOLORSTATIC</c> instead,
    ''' hence the second message.</para>
    ''' </summary>
    Protected Overrides Sub WndProc(ByRef m As Message)
        If (m.Msg = WM_CTLCOLOREDIT OrElse m.Msg = WM_CTLCOLORSTATIC) AndAlso _editable Then
            Dim fore As Color = If(Enabled, ForeColor, ThemeManager.Current.Palette.DisabledTextColor)
            Dim brush As IntPtr = NativeMethods.ApplyControlColors(m.WParam, BackColor, fore)
            If brush <> IntPtr.Zero Then
                m.Result = brush
                Return
            End If
        End If
        MyBase.WndProc(m)

        ' The combo repositions its own EDIT on these three, silently undoing our rectangle.
        If _editable AndAlso (m.Msg = WM_WINDOWPOSCHANGED OrElse m.Msg = WM_SETFONT OrElse
                              m.Msg = CB_SETITEMHEIGHT) Then
            AlignEditText()
        End If
    End Sub

    ''' <summary>The list's uxtheme can only be asked for once the HWND exists.</summary>
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Try
            NativeMethods.ApplyWindowTheme(Me, If(_darkList, "DarkMode_CFD", "Explorer"))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnHandleCreated", ex)
        End Try
        ' The native EDIT is brand new after every HWND recreation (that is, every time Editable is
        ' switched), so the margins are asked for here, not once at construction. The new child has
        ' its own line height and its own internal offset -- neither carries over.
        _lineHeight = 0
        _deltaMeasured = False
        AlignEditText()
    End Sub

End Class
