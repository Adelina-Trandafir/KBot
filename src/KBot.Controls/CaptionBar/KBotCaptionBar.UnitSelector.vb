Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The TITLE SELECTOR of the caption bar (slice 0097): a non-editable drop-down painted right
''' after the title, «K-BOT — [Unit name ▾]». The main window puts the operator's units in it;
''' picking one raises <see cref="SelectorChanged"/> and the host switches the database.
'''
''' <para><b>Only with two or more choices.</b> With one (or none) nothing is drawn and the bar
''' shows its <c>Text</c> exactly as before -- the host keeps writing «K-BOT — name» there.</para>
'''
''' <para><b>The selection moves only when the host says so.</b> A click does not change
''' <see cref="SelectorSelectedKey"/>: the switch can fail on the server, and a selector that
''' already shows the new unit would lie. The host sets the key after the switch succeeded.</para>
'''
''' <para>Painted, not a child control: the bar is one painted surface (buttons, title, drag
''' area), and a real combo inside it would have to be themed, scaled and hit-tested apart.
''' The list is the same <see cref="CustomPopup"/> as the theme menu.</para>
''' </summary>
Partial Public NotInheritable Class KBotCaptionBar

    Private ReadOnly _selectorItems As New List(Of KeyValuePair(Of String, String))()
    Private _selectorKey As String
    Private _selectorHover As Boolean
    Private _selectorActive As Boolean
    ' Raised for the duration of the opening, so the common IPopupAnchor sink knows which
    ' painted element unfolded (see SetPopupOpen).
    Private _selectorMenuOpening As Boolean
    ' Where the selector was drawn last (client coordinates); Empty = not drawn.
    Private _selectorRect As Rectangle = Rectangle.Empty
    Private _selectorBorderColor As Color = SystemColors.ControlDark

    ''' <summary>The operator picked another choice. The selector does NOT move by itself.</summary>
    Public Event SelectorChanged As EventHandler(Of CaptionSelectorChangedEventArgs)

    ''' <summary>Two or more choices: the selector is drawn after the title.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SelectorVisible As Boolean
        Get
            Return _selectorItems.Count >= 2
        End Get
    End Property

    ''' <summary>The key of the choice shown. Setting it does not raise <see cref="SelectorChanged"/>.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectorSelectedKey As String
        Get
            Return _selectorKey
        End Get
        Set(value As String)
            _selectorKey = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' The choices (key, text shown), in order, and the one selected. Fewer than two = no
    ''' selector. Unknown <paramref name="selectedKey"/> -&gt; <see cref="ArgumentException"/>.
    ''' </summary>
    Public Sub SetSelectorItems(items As IEnumerable(Of KeyValuePair(Of String, String)), selectedKey As String)
        _selectorItems.Clear()
        If items IsNot Nothing Then _selectorItems.AddRange(items)
        If _selectorItems.Count > 0 AndAlso Not String.IsNullOrEmpty(selectedKey) AndAlso
           Not _selectorItems.Any(Function(p) String.Equals(p.Key, selectedKey, StringComparison.Ordinal)) Then
            Throw New ArgumentException($"Alegere necunoscută: '{selectedKey}'.", NameOf(selectedKey))
        End If
        _selectorKey = selectedKey
        Invalidate()
    End Sub

    ''' <summary>Removes every choice: the bar goes back to its plain title.</summary>
    Public Sub ClearSelector()
        _selectorItems.Clear()
        _selectorKey = Nothing
        _selectorRect = Rectangle.Empty
        Invalidate()
    End Sub

    ' The text of the selected choice (empty when none is selected).
    Private Function SelectorText() As String
        For Each p As KeyValuePair(Of String, String) In _selectorItems
            If String.Equals(p.Key, _selectorKey, StringComparison.Ordinal) Then Return If(p.Value, String.Empty)
        Next
        Return String.Empty
    End Function

    ''' <summary>
    ''' Draws «title — [choice ▾]» from <paramref name="x"/>, up to <paramref name="rightLimit"/>.
    ''' The title keeps its natural width; the selector takes what is left, cut with «…».
    ''' Called only from OnPaint, which is already wrapped (transitive coverage).
    ''' </summary>
    Private Sub DrawTitleWithSelector(g As Graphics, x As Integer, rightLimit As Integer)
        Dim pad As Integer = ThemeShapes.ScaleDpi(Me, 12)
        Const flags As TextFormatFlags = TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                         TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix
        Dim title As String = If(Text, String.Empty) & " — "
        Dim titleW As Integer = TextRenderer.MeasureText(g, title, Font, New Size(Integer.MaxValue, Height), flags).Width
        Dim avail As Integer = Math.Max(0, rightLimit - x - pad)
        titleW = Math.Min(titleW, avail)
        TextRenderer.DrawText(g, title, Font, New Rectangle(x, 0, titleW, Height), _titleColor,
                              flags Or TextFormatFlags.EndEllipsis)

        Dim sx As Integer = x + titleW
        Dim inner As Integer = ThemeShapes.ScaleDpi(Me, 8)
        Dim chevronW As Integer = ThemeShapes.ScaleDpi(Me, 10)
        Dim choice As String = SelectorText()
        Dim textW As Integer = TextRenderer.MeasureText(g, choice, Font, New Size(Integer.MaxValue, Height), flags).Width
        Dim want As Integer = inner + textW + inner + chevronW + inner
        Dim w As Integer = Math.Min(want, Math.Max(0, rightLimit - pad - sx))
        Dim h As Integer = Math.Max(0, Height - ThemeShapes.ScaleDpi(Me, 12))
        If w <= chevronW + 2 * inner OrElse h <= 0 Then
            _selectorRect = Rectangle.Empty
            Return
        End If
        Dim r As New Rectangle(sx, (Height - h) \ 2, w, h)
        _selectorRect = r

        g.SmoothingMode = SmoothingMode.AntiAlias
        Using path As GraphicsPath = ThemeShapes.RoundedRect(r, ThemeShapes.ScaleDpi(Me, 4))
            If _selectorHover OrElse _selectorActive Then
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

        ' The chevron: a small filled triangle, in the glyph colour.
        Dim cx As Integer = r.Right - inner - chevronW \ 2
        Dim cy As Integer = r.Top + r.Height \ 2
        Dim half As Integer = Math.Max(2, chevronW \ 2)
        Dim tri As Point() = {New Point(cx - half, cy - half \ 2), New Point(cx + half, cy - half \ 2),
                              New Point(cx, cy + half \ 2 + 1)}
        Using b As New SolidBrush(_glyphColor)
            g.FillPolygon(b, tri)
        End Using
    End Sub

    ' The painted selector under the point (only when it is shown).
    Private Function IsOnSelector(location As Point) As Boolean
        Return SelectorVisible AndAlso Not _selectorRect.IsEmpty AndAlso _selectorRect.Contains(location)
    End Function

    ''' <summary>Opens the list of choices under the selector. UI boundary: log and swallow.</summary>
    Private Sub ShowSelectorMenu()
        Try
            If Not SelectorVisible OrElse _selectorRect.IsEmpty Then Return
            ' The second click on the selector CLOSES the list (see ShowThemeMenu).
            If CustomPopup.ClosedJustNow Then Return

            Dim items As New List(Of CustomPopupItem)()
            For Each p As KeyValuePair(Of String, String) In _selectorItems
                items.Add(New CustomPopupItem(p.Key, p.Value) With {
                    .Checked = String.Equals(p.Key, _selectorKey, StringComparison.Ordinal)})
            Next
            Dim meniu As New CustomPopup(items, _selectorKey)
            ' The popup's widths are LOGICAL px; the selector's rectangle is device px.
            Dim factor As Double = Math.Max(0.01, ThemeShapes.ScaleDpi(Me, 1000) / 1000.0)
            meniu.MinimumPopupWidth = CInt(_selectorRect.Width / factor)
            meniu.MaximumPopupWidth = Math.Max(meniu.MaximumPopupWidth, 560)
            AddHandler meniu.ItemClicked, AddressOf SelectorMenu_ItemClicked
            Try
                _selectorMenuOpening = True
                meniu.ShowBelow(Me, _selectorRect)
            Finally
                _selectorMenuOpening = False
            End Try
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotCaptionBar.ShowSelectorMenu", ex)
        End Try
    End Sub

    Private Sub SelectorMenu_ItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Try
            Dim key As String = e?.Item?.Key
            If String.IsNullOrEmpty(key) OrElse String.Equals(key, _selectorKey, StringComparison.Ordinal) Then Return
            RaiseEvent SelectorChanged(Me, New CaptionSelectorChangedEventArgs(key))
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotCaptionBar.SelectorMenu_ItemClicked", ex)
        End Try
    End Sub

End Class

''' <summary>The choice the operator picked in <see cref="KBotCaptionBar"/>'s selector. POCO.</summary>
Public NotInheritable Class CaptionSelectorChangedEventArgs
    Inherits EventArgs

    Public ReadOnly Property Key As String

    Public Sub New(key As String)
        Me.Key = key
    End Sub
End Class
