Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The TITLE SELECTORS of the caption bar: non-editable drop-downs painted right after the title,
''' «K-BOT — [Unit name ▾]   An Date [2026 ▾]   Sursă/Sector [02A ▾]». Three of them, each
''' addressed by name: <see cref="SelectorUnit"/> (slice 0097), <see cref="SelectorYear"/> and
''' <see cref="SelectorSector"/> (slice 0097-03, moved here from the main window's header band).
''' Picking a choice raises <see cref="SelectorChanged"/>; the host does the work.
'''
''' <para><b>When one is drawn.</b> The unit selector only with two or more choices (with one the
''' bar shows its <c>Text</c> exactly as before -- the host keeps writing «K-BOT — name» there).
''' The year and sector selectors with one or more choices. A selector the host hid
''' (<see cref="SetSelectorShown"/>) is not drawn whatever it holds.</para>
'''
''' <para><b>The selection moves only when the host says so.</b> A click does not change the key:
''' the switch can fail on the server, and a selector that already shows the new unit would lie.
''' The host sets the key (<see cref="SetSelectorKey"/>) after the work succeeded.</para>
'''
''' <para>Painted, not child controls: the bar is one painted surface (buttons, title, drag
''' area), and real combos inside it would have to be themed, scaled and hit-tested apart.
''' The list is the same <see cref="CustomPopup"/> as the theme menu.</para>
''' </summary>
Partial Public NotInheritable Class KBotCaptionBar

    ''' <summary>The unit selector (shown after the title, only with two or more units).</summary>
    Public Const SelectorUnit As String = "unit"
    ''' <summary>The year selector («An Date»).</summary>
    Public Const SelectorYear As String = "year"
    ''' <summary>The source / sector selector («Sursă/Sector»).</summary>
    Public Const SelectorSector As String = "ss"
    ''' <summary>The invoice-kind selector of the E-Factura window (sales / purchases).</summary>
    Public Const SelectorKind As String = "kind"

    ''' <summary>One painted drop-down: its choices, its state and where it was drawn last.</summary>
    Private NotInheritable Class TitleSelector
        Public ReadOnly Name As String
        ' Written before the box (operator-visible); empty = no label.
        Public ReadOnly Caption As String
        ' The fewest choices that make the selector worth drawing.
        Public ReadOnly MinChoices As Integer
        Public ReadOnly Items As New List(Of KeyValuePair(Of String, String))()
        Public Key As String
        ' The host can hide it (the sector selector hides when the tree is sorted by date).
        Public Shown As Boolean = True
        Public Hover As Boolean
        Public Active As Boolean
        ' Where the box was drawn last (client coordinates); Empty = not drawn.
        Public Rect As Rectangle = Rectangle.Empty
        ' Where the chosen text was written last (inside Rect); Empty = not drawn.
        Public TextRect As Rectangle = Rectangle.Empty

        Public Sub New(name As String, caption As String, minChoices As Integer)
            Me.Name = name
            Me.Caption = caption
            Me.MinChoices = minChoices
        End Sub

        Public ReadOnly Property Visible As Boolean
            Get
                Return Shown AndAlso Items.Count >= MinChoices
            End Get
        End Property

        ' The text of the selected choice (empty when none is selected).
        Public Function ChosenText() As String
            For Each p As KeyValuePair(Of String, String) In Items
                If String.Equals(p.Key, Key, StringComparison.Ordinal) Then Return If(p.Value, String.Empty)
            Next
            Return String.Empty
        End Function
    End Class

    ' Left to right, as drawn.
    Private ReadOnly _selectors As TitleSelector() = {
        New TitleSelector(SelectorUnit, String.Empty, 2),
        New TitleSelector(SelectorYear, "An Date", 1),
        New TitleSelector(SelectorSector, "Sursă/Sector", 1),
        New TitleSelector(SelectorKind, "Tip factură", 1)}
    ' Raised for the duration of the opening, so the common IPopupAnchor sink knows which
    ' painted element unfolded (see SetPopupOpen).
    Private _selectorMenuOpening As Boolean
    Private _menuSelector As TitleSelector
    Private _selectorBorderColor As Color = SystemColors.ControlDark

    ''' <summary>The operator picked another choice. The selector does NOT move by itself.</summary>
    Public Event SelectorChanged As EventHandler(Of CaptionSelectorChangedEventArgs)

    ' The selector called <paramref name="name"/>; an unknown name is a programming error.
    Private Function Sel(name As String) As TitleSelector
        For Each s As TitleSelector In _selectors
            If String.Equals(s.Name, name, StringComparison.Ordinal) Then Return s
        Next
        Throw New ArgumentException($"Selector necunoscut: '{name}'.", NameOf(name))
    End Function

    ''' <summary>The unit selector is drawn (two or more choices).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SelectorVisible As Boolean
        Get
            Return Sel(SelectorUnit).Visible
        End Get
    End Property

    ''' <summary>The key of the unit shown. Setting it does not raise <see cref="SelectorChanged"/>.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectorSelectedKey As String
        Get
            Return Sel(SelectorUnit).Key
        End Get
        Set(value As String)
            SetSelectorKey(SelectorUnit, value)
        End Set
    End Property

    ''' <summary>The choices of the unit selector (key, text), in order, and the one selected.</summary>
    Public Sub SetSelectorItems(items As IEnumerable(Of KeyValuePair(Of String, String)), selectedKey As String)
        SetSelectorItems(SelectorUnit, items, selectedKey)
    End Sub

    ''' <summary>
    ''' The choices of the selector <paramref name="selector"/> (key, text shown), in order, and the
    ''' one selected. Too few choices = the selector is not drawn. Unknown <paramref name="selector"/>
    ''' or <paramref name="selectedKey"/> -&gt; <see cref="ArgumentException"/>.
    ''' </summary>
    Public Sub SetSelectorItems(selector As String, items As IEnumerable(Of KeyValuePair(Of String, String)), selectedKey As String)
        Dim s As TitleSelector = Sel(selector)
        s.Items.Clear()
        If items IsNot Nothing Then s.Items.AddRange(items)
        If s.Items.Count > 0 AndAlso Not String.IsNullOrEmpty(selectedKey) AndAlso
           Not s.Items.Any(Function(p) String.Equals(p.Key, selectedKey, StringComparison.Ordinal)) Then
            Throw New ArgumentException($"Alegere necunoscută: '{selectedKey}'.", NameOf(selectedKey))
        End If
        s.Key = selectedKey
        Invalidate()
    End Sub

    ''' <summary>Removes every choice of the unit selector: the bar goes back to its plain title.</summary>
    Public Sub ClearSelector()
        ClearSelector(SelectorUnit)
    End Sub

    ''' <summary>Removes every choice of <paramref name="selector"/>: it is no longer drawn.</summary>
    Public Sub ClearSelector(selector As String)
        Dim s As TitleSelector = Sel(selector)
        s.Items.Clear()
        s.Key = Nothing
        s.Rect = Rectangle.Empty
        s.TextRect = Rectangle.Empty
        Invalidate()
    End Sub

    ''' <summary>The key of the choice shown by <paramref name="selector"/> (Nothing = none).</summary>
    Public Function GetSelectorKey(selector As String) As String
        Return Sel(selector).Key
    End Function

    ''' <summary>
    ''' Moves <paramref name="selector"/> to <paramref name="key"/> without raising
    ''' <see cref="SelectorChanged"/>. Unknown selector -&gt; <see cref="ArgumentException"/>.
    ''' </summary>
    Public Sub SetSelectorKey(selector As String, key As String)
        Sel(selector).Key = key
        Invalidate()
    End Sub

    ''' <summary>Hides or shows <paramref name="selector"/> without touching its choices.</summary>
    Public Sub SetSelectorShown(selector As String, shown As Boolean)
        Dim s As TitleSelector = Sel(selector)
        If s.Shown = shown Then Return
        s.Shown = shown
        If Not shown Then
            s.Rect = Rectangle.Empty
            s.TextRect = Rectangle.Empty
            s.Hover = False
        End If
        Invalidate()
    End Sub

    ' At least one selector is drawn: the title is then painted by DrawTitleWithSelectors.
    Private Function AnySelectorVisible() As Boolean
        Return _selectors.Any(Function(s) s.Visible)
    End Function

    ''' <summary>
    ''' Draws «title — [unit ▾]  An Date [year ▾]  Sursă/Sector [ss ▾]» from <paramref name="x"/>, up
    ''' to <paramref name="rightLimit"/>. The title keeps its natural width; the selectors take what is
    ''' left (the one that does not fit is not drawn, the one that half fits is cut with «…»).
    ''' Called only from OnPaint, which is already wrapped (transitive coverage).
    ''' </summary>
    Private Sub DrawTitleWithSelectors(g As Graphics, x As Integer, rightLimit As Integer)
        Dim pad As Integer = ThemeShapes.ScaleDpi(Me, 12)
        Const flags As TextFormatFlags = TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                         TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix
        For Each s As TitleSelector In _selectors
            s.Rect = Rectangle.Empty
            s.TextRect = Rectangle.Empty
        Next

        Dim title As String = If(Text, String.Empty) & If(Sel(SelectorUnit).Visible, TitleSeparator, String.Empty)
        Dim titleW As Integer = TextRenderer.MeasureText(g, title, Font, New Size(Integer.MaxValue, Height), flags).Width
        Dim avail As Integer = Math.Max(0, rightLimit - x - pad)
        titleW = Math.Min(titleW, avail)
        TextRenderer.DrawText(g, title, Font, New Rectangle(x, 0, titleW, Height), _titleColor,
                              flags Or TextFormatFlags.EndEllipsis)

        Dim sx As Integer = x + titleW
        For Each s As TitleSelector In _selectors
            If s.Visible Then sx = DrawOneSelector(g, s, sx, rightLimit - pad, flags)
        Next
    End Sub

    ' One selector: its label (when it has one), then the box. Returns where the next one starts
    ' (unchanged when this one did not fit). Called only from OnPaint.
    Private Function DrawOneSelector(g As Graphics, s As TitleSelector, sx As Integer, limit As Integer,
                                     flags As TextFormatFlags) As Integer
        Dim inner As Integer = ThemeShapes.ScaleDpi(Me, 8)
        Dim chevronW As Integer = ThemeShapes.ScaleDpi(Me, 10)
        Dim boxStart As Integer = sx

        If Not String.IsNullOrEmpty(s.Caption) Then
            Dim gap As Integer = ThemeShapes.ScaleDpi(Me, 16)
            Dim capGap As Integer = ThemeShapes.ScaleDpi(Me, 4)
            Dim capW As Integer = TextRenderer.MeasureText(g, s.Caption, Font, New Size(Integer.MaxValue, Height), flags).Width
            boxStart = sx + gap + capW + capGap
            ' No room for the label and a box that can show anything: neither is drawn.
            If boxStart + chevronW + 2 * inner >= limit Then Return sx
            TextRenderer.DrawText(g, s.Caption, Font, New Rectangle(sx + gap, 0, capW, Height), _glyphColor, flags)
        End If

        Dim choice As String = s.ChosenText()
        Dim textW As Integer = TextRenderer.MeasureText(g, choice, Font, New Size(Integer.MaxValue, Height), flags).Width
        Dim want As Integer = inner + textW + inner + chevronW + inner
        Dim w As Integer = Math.Min(want, Math.Max(0, limit - boxStart))
        Dim h As Integer = Math.Max(0, Height - ThemeShapes.ScaleDpi(Me, 12))
        If w <= chevronW + 2 * inner OrElse h <= 0 Then Return sx
        Dim r As New Rectangle(boxStart, (Height - h) \ 2, w, h)
        s.Rect = r

        g.SmoothingMode = SmoothingMode.AntiAlias
        Using path As GraphicsPath = ThemeShapes.RoundedRect(r, ThemeShapes.ScaleDpi(Me, 4))
            If s.Hover OrElse s.Active Then
                Using b As New SolidBrush(_optBtnHoverColor)
                    g.FillPath(b, path)
                End Using
            End If
            Using pen As New Pen(_selectorBorderColor)
                g.DrawPath(pen, path)
            End Using
        End Using

        Dim textRect As New Rectangle(r.Left + inner, r.Top, Math.Max(0, r.Width - 3 * inner - chevronW), r.Height)
        TextRenderer.DrawText(g, choice, Font, textRect, _titleColor, flags Or TextFormatFlags.EndEllipsis)
        ' Slice 0000-23: the capture blurs only the chosen text, not the box around it.
        s.TextRect = New Rectangle(textRect.Left, textRect.Top, Math.Min(textW, textRect.Width), textRect.Height)

        ' The chevron: a small filled triangle, in the glyph colour.
        Dim cx As Integer = r.Right - inner - chevronW \ 2
        Dim cy As Integer = r.Top + r.Height \ 2
        Dim half As Integer = Math.Max(2, chevronW \ 2)
        Dim tri As Point() = {New Point(cx - half, cy - half \ 2), New Point(cx + half, cy - half \ 2),
                              New Point(cx, cy + half \ 2 + 1)}
        Using b As New SolidBrush(_glyphColor)
            g.FillPolygon(b, tri)
        End Using
        Return r.Right
    End Function

    ' The painted selector under the point (Nothing = none).
    Private Function HitSelector(location As Point) As TitleSelector
        For Each s As TitleSelector In _selectors
            If s.Visible AndAlso Not s.Rect.IsEmpty AndAlso s.Rect.Contains(location) Then Return s
        Next
        Return Nothing
    End Function

    ' Lights the selector under the mouse (Nothing = none); True when any hover state changed.
    Private Function UpdateSelectorHover(hot As TitleSelector) As Boolean
        Dim changed As Boolean
        For Each s As TitleSelector In _selectors
            Dim over As Boolean = s Is hot
            If s.Hover <> over Then
                s.Hover = over
                changed = True
            End If
        Next
        Return changed
    End Function

    Private Function AnySelectorActive() As Boolean
        Return _selectors.Any(Function(s) s.Active)
    End Function

    ''' <summary>Opens the list of choices under <paramref name="s"/>. UI boundary: log and swallow.</summary>
    Private Sub ShowSelectorMenu(s As TitleSelector)
        Try
            If s Is Nothing OrElse Not s.Visible OrElse s.Rect.IsEmpty Then Return
            ' The second click on the selector CLOSES the list (see ShowThemeMenu).
            If CustomPopup.ClosedJustNow Then Return

            Dim items As New List(Of CustomPopupItem)()
            For Each p As KeyValuePair(Of String, String) In s.Items
                items.Add(New CustomPopupItem(p.Key, p.Value) With {
                    .Checked = String.Equals(p.Key, s.Key, StringComparison.Ordinal)})
            Next
            Dim meniu As New CustomPopup(items, s.Key)
            ' The popup's widths are LOGICAL px; the selector's rectangle is device px.
            Dim factor As Double = Math.Max(0.01, ThemeShapes.ScaleDpi(Me, 1000) / 1000.0)
            meniu.MinimumPopupWidth = CInt(s.Rect.Width / factor)
            meniu.MaximumPopupWidth = Math.Max(meniu.MaximumPopupWidth, 560)
            AddHandler meniu.ItemClicked, Sub(sender As Object, e As CustomPopupItemEventArgs) SelectorMenu_ItemClicked(s, e)
            Try
                _menuSelector = s
                _selectorMenuOpening = True
                meniu.ShowBelow(Me, s.Rect)
            Finally
                _selectorMenuOpening = False
            End Try
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotCaptionBar.ShowSelectorMenu", ex)
        End Try
    End Sub

    Private Sub SelectorMenu_ItemClicked(s As TitleSelector, e As CustomPopupItemEventArgs)
        Try
            Dim key As String = e?.Item?.Key
            If String.IsNullOrEmpty(key) OrElse String.Equals(key, s.Key, StringComparison.Ordinal) Then Return
            RaiseEvent SelectorChanged(Me, New CaptionSelectorChangedEventArgs(s.Name, key))
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotCaptionBar.SelectorMenu_ItemClicked", ex)
        End Try
    End Sub

End Class

''' <summary>The choice the operator picked in one of <see cref="KBotCaptionBar"/>'s selectors. POCO.</summary>
Public NotInheritable Class CaptionSelectorChangedEventArgs
    Inherits EventArgs

    ''' <summary>Which selector: <see cref="KBotCaptionBar.SelectorUnit"/>, <see cref="KBotCaptionBar.SelectorYear"/> or <see cref="KBotCaptionBar.SelectorSector"/>.</summary>
    Public ReadOnly Property Selector As String
    Public ReadOnly Property Key As String

    Public Sub New(selector As String, key As String)
        Me.Selector = selector
        Me.Key = key
    End Sub
End Class
