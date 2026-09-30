Option Strict On
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The list of the help popup and of the help window's search (slice 0000-20): headers, search
''' hits (title › section, a few words of the text, «Deschide ...» / «Tur ghidat» buttons), the
''' topic of the screen, tours and folders of tours. Drawn by us, so it takes the scheme colours;
''' the rows come from the application (<see cref="KBotHelpRow"/>).
'''
''' <para>Not selectable: the search box keeps the keyboard and hands Up / Down / Enter over
''' (<see cref="MoveSelection"/>, <see cref="InvokeSelected"/>). A folder opens and closes here;
''' every other use of a row is raised as <see cref="RowInvoked"/>.</para>
''' </summary>
<ToolboxItem(False)>
<DesignerCategory("Code")>
Public NotInheritable Class KBotHelpList
    Inherits Control
    Implements IThemedControl

    ' Logical px (96 dpi), scaled at layout time (C2).
    Private Const PadXLogical As Integer = 10
    Private Const RowAirLogical As Integer = 12
    Private Const HitAirLogical As Integer = 8
    Private Const IndentLogical As Integer = 18
    Private Const ButtonGapLogical As Integer = 6
    Private Const ButtonPadXLogical As Integer = 10
    Private Const ButtonAirLogical As Integer = 8
    Private Const WheelStepLogical As Integer = 48
    Private Const SnippetLines As Integer = 2

    Private Shared ReadOnly PlayMark As String = ChrW(&H25B6) & "  "
    Private Shared ReadOnly FolderClosedMark As String = ChrW(&H25B8) & "  "
    Private Shared ReadOnly FolderOpenMark As String = ChrW(&H25BE) & "  "
    Private Shared ReadOnly SectionMark As String = "  " & ChrW(&H203A) & "  "

    ''' <summary>One drawn line of the list: a row, where it is, and where its buttons are.</summary>
    Private NotInheritable Class Slot
        Public Row As KBotHelpRow
        Public Depth As Integer
        Public Bounds As Rectangle          ' content coordinates (before scrolling)
        Public OpenButton As Rectangle      ' Empty = none
        Public TourButton As Rectangle
    End Class

    Private ReadOnly _rows As New List(Of KBotHelpRow)()
    Private ReadOnly _slots As New List(Of Slot)()
    Private ReadOnly _scroll As New KBotScrollBar()
    Private ReadOnly _tips As New KBotToolTip()
    Private ReadOnly _tipContent As New KBotToolTipContent()
    Private _contentHeight As Integer
    Private _selected As Integer = -1
    Private _hover As Integer = -1
    Private _hoverPart As KBotHelpRowAction = KBotHelpRowAction.Activate
    Private _tipKey As String = String.Empty
    Private _boldFont As Font

    ' Theme colours (fallbacks = light look, as an unthemed host would show it).
    Private _back As Color = SystemColors.Window
    Private _fore As Color = SystemColors.WindowText
    Private _dim As Color = SystemColors.GrayText
    Private _hoverBack As Color = SystemColors.ControlLight
    Private _accent As Color = SystemColors.Highlight
    Private _buttonBack As Color = SystemColors.Control
    Private _buttonFore As Color = SystemColors.ControlText
    Private _buttonBorder As Color = SystemColors.ControlDark
    Private _buttonHover As Color = SystemColors.ControlLight

    ''' <summary>A row was used (clicked, Enter, one of its buttons). Folders are not raised.</summary>
    Public Event RowInvoked As EventHandler(Of KBotHelpRowEventArgs)

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        SetStyle(ControlStyles.Selectable, False)
        TabStop = False
        _scroll.Dock = DockStyle.Right
        _scroll.Visible = False
        _scroll.Width = KBotScrollBar.GrosimeImplicita
        AddHandler _scroll.ValueChanged, AddressOf Scroll_ValueChanged
        Controls.Add(_scroll)
    End Sub

    Protected Overrides ReadOnly Property DefaultSize As Size
        Get
            Return New Size(300, 200)
        End Get
    End Property

    ' ── Rows ──────────────────────────────────────────────────────────────────────

    ''' <summary>The rows, as given to <see cref="SetRows"/>.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Rows As IReadOnlyList(Of KBotHelpRow)
        Get
            Return _rows
        End Get
    End Property

    ''' <summary>Replaces the rows; the first selectable one is selected and the list goes to the top.</summary>
    Public Sub SetRows(rows As IEnumerable(Of KBotHelpRow))
        Try
            _rows.Clear()
            If rows IsNot Nothing Then _rows.AddRange(rows.Where(Function(r) r IsNot Nothing))
            _hover = -1
            HideTip()
            Relayout()
            _selected = NextSelectable(-1, 1)
            _scroll.Value = 0
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpList.SetRows", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The selected row, or Nothing.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SelectedRow As KBotHelpRow
        Get
            Return If(_selected >= 0 AndAlso _selected < _slots.Count, _slots(_selected).Row, Nothing)
        End Get
    End Property

    ''' <summary>Moves the selection by <paramref name="delta"/> selectable rows (Up = -1, Down = +1).</summary>
    Public Sub MoveSelection(delta As Integer)
        If _slots.Count = 0 OrElse delta = 0 Then Return
        Dim n As Integer = NextSelectable(_selected, Math.Sign(delta))
        If n < 0 Then Return
        _selected = n
        EnsureVisible(n)
        Invalidate()
    End Sub

    ''' <summary>Enter: uses the selected row (a folder opens or closes).</summary>
    Public Sub InvokeSelected()
        If _selected < 0 OrElse _selected >= _slots.Count Then Return
        Use(_selected, KBotHelpRowAction.Activate)
    End Sub

    Private Function NextSelectable(from As Integer, direction As Integer) As Integer
        Dim i As Integer = from + direction
        While i >= 0 AndAlso i < _slots.Count
            If _slots(i).Row.IsSelectable Then Return i
            i += direction
        End While
        Return If(from >= 0 AndAlso from < _slots.Count, from, -1)
    End Function

    Private Sub Use(index As Integer, action As KBotHelpRowAction)
        Dim row As KBotHelpRow = _slots(index).Row
        If Not row.IsSelectable Then Return
        If row.Kind = KBotHelpRowKind.Folder Then
            row.Expanded = Not row.Expanded
            Relayout()
            _selected = Math.Min(index, _slots.Count - 1)
            Invalidate()
            Return
        End If
        RaiseEvent RowInvoked(Me, New KBotHelpRowEventArgs(row, action))
    End Sub

    ' ── Layout ────────────────────────────────────────────────────────────────────

    Private Function Px(logical As Integer) As Integer
        If KBotDesignTime.IsDesignTime(Me) Then Return logical
        Return ThemeShapes.ScaleDpi(Me, logical)
    End Function

    Private ReadOnly Property BoldFont As Font
        Get
            If _boldFont Is Nothing Then _boldFont = New Font(Font, FontStyle.Bold)
            Return _boldFont
        End Get
    End Property

    ' Content width: the scroll bar's column is kept free whenever the list could need it.
    Private Function ContentWidth() As Integer
        Return Math.Max(40, ClientSize.Width - If(_scroll.Visible, _scroll.Width, 0))
    End Function

    Private Sub Relayout()
        BuildSlots()
        UpdateScroll()
    End Sub

    ' Every visible row (open folders with their tours) at its place, for the current width.
    Private Sub BuildSlots()
        _slots.Clear()
        Dim y As Integer = 0
        For Each r As KBotHelpRow In _rows
            y = AddSlot(r, 0, y)
            If r.Kind = KBotHelpRowKind.Folder AndAlso r.Expanded Then
                For Each c As KBotHelpRow In r.Children
                    y = AddSlot(c, 1, y)
                Next
            End If
        Next
        _contentHeight = y
    End Sub

    Private Function AddSlot(r As KBotHelpRow, depth As Integer, y As Integer) As Integer
        Dim s As New Slot With {.Row = r, .Depth = depth}
        Dim w As Integer = ContentWidth()
        Dim lineH As Integer = Font.Height
        Dim h As Integer
        Select Case r.Kind
            Case KBotHelpRowKind.Header
                h = lineH + Px(RowAirLogical)
            Case KBotHelpRowKind.Note
                Dim textW As Integer = Math.Max(20, w - 2 * Px(PadXLogical))
                h = TextRenderer.MeasureText(r.Title, Font, New Size(textW, Integer.MaxValue), TextFormatFlags.WordBreak).Height + Px(RowAirLogical)
            Case KBotHelpRowKind.Hit
                h = Px(HitAirLogical) + lineH
                If r.Snippet.Length > 0 Then h += lineH * SnippetLines
                If r.OpenText.Length > 0 OrElse r.TourText.Length > 0 Then
                    Dim bh As Integer = lineH + Px(ButtonAirLogical)
                    Dim bx As Integer = Px(PadXLogical)
                    Dim by As Integer = y + h + Px(4)
                    If r.OpenText.Length > 0 Then
                        Dim bw As Integer = TextRenderer.MeasureText(r.OpenText, Font).Width + 2 * Px(ButtonPadXLogical)
                        s.OpenButton = New Rectangle(bx, by, bw, bh)
                        bx += bw + Px(ButtonGapLogical)
                    End If
                    If r.TourText.Length > 0 Then
                        Dim bw As Integer = TextRenderer.MeasureText(r.TourText, Font).Width + 2 * Px(ButtonPadXLogical)
                        s.TourButton = New Rectangle(bx, by, bw, bh)
                    End If
                    h += bh + Px(4)
                End If
                h += Px(HitAirLogical)
            Case Else
                h = lineH + Px(RowAirLogical)
        End Select
        s.Bounds = New Rectangle(0, y, w, h)
        _slots.Add(s)
        Return y + h
    End Function

    Private Sub UpdateScroll()
        Dim need As Boolean = _contentHeight > ClientSize.Height AndAlso ClientSize.Height > 0
        If need <> _scroll.Visible Then
            _scroll.Visible = need
            BuildSlots()   ' the width available changed: measure again
        End If
        _scroll.Width = Px(KBotScrollBar.GrosimeImplicita)
        If _scroll.Visible Then
            _scroll.SmallChange = Px(WheelStepLogical) \ 2
            _scroll.SetRange(0, Math.Max(0, _contentHeight - 1), Math.Max(1, ClientSize.Height), _scroll.Value)
        End If
    End Sub

    Private ReadOnly Property Offset As Integer
        Get
            Return If(_scroll.Visible, _scroll.Value, 0)
        End Get
    End Property

    Private Sub EnsureVisible(index As Integer)
        If Not _scroll.Visible Then Return
        Dim b As Rectangle = _slots(index).Bounds
        If b.Top < _scroll.Value Then
            _scroll.Value = b.Top
        ElseIf b.Bottom > _scroll.Value + ClientSize.Height Then
            _scroll.Value = b.Bottom - ClientSize.Height
        End If
    End Sub

    Private Sub Scroll_ValueChanged(sender As Object, e As EventArgs)
        HideTip()
        Invalidate()
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Try
            Relayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpList.OnResize", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        Try
            _boldFont?.Dispose()
            _boldFont = Nothing
            Relayout()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpList.OnFontChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        Try
            Relayout()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpList.OnDpiChangedAfterParent", ex)
        End Try
    End Sub

    ' ── Painting ──────────────────────────────────────────────────────────────────

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        Using b As New SolidBrush(_back)
            e.Graphics.FillRectangle(b, ClientRectangle)
        End Using
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            Dim off As Integer = Offset
            For i As Integer = 0 To _slots.Count - 1
                Dim s As Slot = _slots(i)
                Dim r As Rectangle = s.Bounds
                r.Offset(0, -off)
                If r.Bottom < 0 Then Continue For
                If r.Top > ClientSize.Height Then Exit For
                PaintSlot(g, s, r, i)
            Next
        Catch ex As Exception
            ' UI boundary (OnPaint).
            GlobalErrorLog.Write("KBotHelpList.OnPaint", ex)
        End Try
    End Sub

    Private Sub PaintSlot(g As Graphics, s As Slot, r As Rectangle, index As Integer)
        Dim row As KBotHelpRow = s.Row
        Dim padX As Integer = Px(PadXLogical) + s.Depth * Px(IndentLogical)
        Dim lineH As Integer = Font.Height
        Dim selected As Boolean = index = _selected AndAlso row.IsSelectable
        Dim hovered As Boolean = index = _hover AndAlso row.IsSelectable

        If selected OrElse hovered Then
            Using b As New SolidBrush(_hoverBack)
                g.FillRectangle(b, r)
            End Using
        End If
        If selected Then
            Using b As New SolidBrush(_accent)
                g.FillRectangle(b, r.Left, r.Top, Px(3), r.Height)
            End Using
        End If

        Dim textW As Integer = Math.Max(10, r.Width - padX - Px(PadXLogical))
        Const OneLine As TextFormatFlags = TextFormatFlags.EndEllipsis Or TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix Or TextFormatFlags.VerticalCenter
        Select Case row.Kind
            Case KBotHelpRowKind.Header
                TextRenderer.DrawText(g, row.Title, BoldFont, New Rectangle(r.Left + padX, r.Top, textW, r.Height), _dim, OneLine Or TextFormatFlags.Bottom)
            Case KBotHelpRowKind.Note
                TextRenderer.DrawText(g, row.Title, Font, New Rectangle(r.Left + padX, r.Top + Px(RowAirLogical) \ 2, textW, r.Height),
                                      _dim, TextFormatFlags.WordBreak Or TextFormatFlags.NoPrefix)
            Case KBotHelpRowKind.Tour
                TextRenderer.DrawText(g, PlayMark & row.Title, Font, New Rectangle(r.Left + padX, r.Top, textW, r.Height), _fore, OneLine)
            Case KBotHelpRowKind.Folder
                Dim caption As String = If(row.Expanded, FolderOpenMark, FolderClosedMark) & row.Title & "  (" & row.Children.Count.ToString(Globalization.CultureInfo.InvariantCulture) & ")"
                TextRenderer.DrawText(g, caption, BoldFont, New Rectangle(r.Left + padX, r.Top, textW, r.Height), _fore, OneLine)
            Case KBotHelpRowKind.Topic
                TextRenderer.DrawText(g, row.Title, BoldFont, New Rectangle(r.Left + padX, r.Top, textW, r.Height), _fore, OneLine)
            Case KBotHelpRowKind.Hit
                Dim y As Integer = r.Top + Px(HitAirLogical)
                Dim titleW As Integer = Math.Min(textW, TextRenderer.MeasureText(g, row.Title, BoldFont, Size.Empty, TextFormatFlags.NoPrefix Or TextFormatFlags.NoPadding).Width)
                TextRenderer.DrawText(g, row.Title, BoldFont, New Rectangle(r.Left + padX, y, titleW, lineH), _fore, OneLine Or TextFormatFlags.NoPadding)
                If row.Subtitle.Length > 0 AndAlso titleW < textW Then
                    TextRenderer.DrawText(g, SectionMark & row.Subtitle, Font, New Rectangle(r.Left + padX + titleW, y, textW - titleW, lineH), _fore, OneLine Or TextFormatFlags.NoPadding)
                End If
                y += lineH
                If row.Snippet.Length > 0 Then
                    TextRenderer.DrawText(g, row.Snippet, Font, New Rectangle(r.Left + padX, y, textW, lineH * SnippetLines), _dim,
                                          TextFormatFlags.WordBreak Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix Or TextFormatFlags.NoPadding)
                End If
                Dim shift As Integer = r.Top - s.Bounds.Top
                If Not s.OpenButton.IsEmpty Then PaintButton(g, s.OpenButton, shift, row.OpenText, index = _hover AndAlso _hoverPart = KBotHelpRowAction.Open)
                If Not s.TourButton.IsEmpty Then PaintButton(g, s.TourButton, shift, row.TourText, index = _hover AndAlso _hoverPart = KBotHelpRowAction.Tour)
        End Select
    End Sub

    Private Sub PaintButton(g As Graphics, b As Rectangle, shift As Integer, text As String, hot As Boolean)
        Dim rect As New Rectangle(b.X, b.Y + shift, b.Width, b.Height)
        Dim radius As Integer = ThemeShapes.ScaleDpi(Me, Math.Max(0, ThemeManager.Current.Style.CornerRadius))
        Dim old As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Using path As GraphicsPath = ThemeShapes.RoundedRect(New Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1), radius)
            Using fill As New SolidBrush(If(hot, _buttonHover, _buttonBack))
                g.FillPath(fill, path)
            End Using
            Using pen As New Pen(If(hot, _accent, _buttonBorder))
                g.DrawPath(pen, path)
            End Using
        End Using
        g.SmoothingMode = old
        TextRenderer.DrawText(g, text, Font, rect, _buttonFore,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix)
    End Sub

    ' ── Mouse ─────────────────────────────────────────────────────────────────────

    Private Function HitTest(p As Point, ByRef part As KBotHelpRowAction) As Integer
        part = KBotHelpRowAction.Activate
        If p.X >= ContentWidth() Then Return -1
        Dim y As Integer = p.Y + Offset
        For i As Integer = 0 To _slots.Count - 1
            Dim s As Slot = _slots(i)
            If y < s.Bounds.Top OrElse y >= s.Bounds.Bottom Then Continue For
            Dim content As New Point(p.X, y)
            If Not s.OpenButton.IsEmpty AndAlso s.OpenButton.Contains(content) Then part = KBotHelpRowAction.Open
            If Not s.TourButton.IsEmpty AndAlso s.TourButton.Contains(content) Then part = KBotHelpRowAction.Tour
            Return i
        Next
        Return -1
    End Function

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            Dim part As KBotHelpRowAction
            Dim i As Integer = HitTest(e.Location, part)
            If i >= 0 AndAlso Not _slots(i).Row.IsSelectable Then i = -1
            If i <> _hover OrElse part <> _hoverPart Then
                _hover = i
                _hoverPart = part
                Cursor = If(i >= 0, Cursors.Hand, Cursors.Default)
                Invalidate()
                UpdateTip(i, part, e.Location)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpList.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        Try
            If _hover >= 0 Then
                _hover = -1
                Invalidate()
            End If
            HideTip()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpList.OnMouseLeave", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Dim part As KBotHelpRowAction
            Dim i As Integer = HitTest(e.Location, part)
            If i < 0 OrElse Not _slots(i).Row.IsSelectable Then Return
            HideTip()
            _selected = i
            Invalidate()
            Use(i, part)
        Catch ex As Exception
            ' UI boundary (mouse handler).
            GlobalErrorLog.Write("KBotHelpList.OnMouseClick", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Try
            If Not _scroll.Visible Then Return
            _scroll.Value -= Math.Sign(e.Delta) * Px(WheelStepLogical)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpList.OnMouseWheel", ex)
        End Try
    End Sub

    ' Hover text: the button's own caption as the header, the row's text as the body.
    Private Sub UpdateTip(index As Integer, part As KBotHelpRowAction, p As Point)
        If index < 0 Then
            HideTip()
            Return
        End If
        Dim row As KBotHelpRow = _slots(index).Row
        Dim header As String
        Select Case part
            Case KBotHelpRowAction.Open : header = row.OpenText
            Case KBotHelpRowAction.Tour : header = row.TourText
            Case Else : header = Nothing
        End Select
        Dim body As String = row.ToolTipText
        If String.IsNullOrEmpty(body) AndAlso header Is Nothing Then
            HideTip()
            Return
        End If
        Dim key As String = index.ToString(Globalization.CultureInfo.InvariantCulture) & "|" & CInt(part).ToString(Globalization.CultureInfo.InvariantCulture)
        If key = _tipKey Then Return
        _tipKey = key
        _tipContent.HeaderText = header
        _tipContent.Text = If(part = KBotHelpRowAction.Activate, body, TipForButton(row, part))
        _tips.ShowAt(Me, _tipContent, PointToScreen(New Point(p.X, p.Y + Px(18))))
    End Sub

    Private Shared Function TipForButton(row As KBotHelpRow, part As KBotHelpRowAction) As String
        If part = KBotHelpRowAction.Open Then Return "K-BOT deschide ecranul despre care e vorba aici."
        Return "K-BOT îți arată pe ecran, pas cu pas, unde e fiecare lucru."
    End Function

    Private Sub HideTip()
        _tipKey = String.Empty
        _tips.HideNow()
    End Sub

    ' ── Theme ─────────────────────────────────────────────────────────────────────

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            _back = p.SurfaceAltColor
            _fore = p.TextColor
            _dim = p.TextDimColor
            _hoverBack = p.ButtonHoverColor
            _accent = p.AccentColor
            _buttonBack = p.ButtonBackColor
            _buttonFore = p.ButtonTextColor
            _buttonBorder = p.ButtonBorderColor
            _buttonHover = p.ButtonHoverColor
            _scroll.ApplyTheme(scheme)
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpList.ApplyTheme", ex)
        End Try
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                _tips.Dispose()
                _boldFont?.Dispose()
                _boldFont = Nothing
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

End Class
