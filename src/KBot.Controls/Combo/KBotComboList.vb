Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The ONE list of <see cref="KBotComboBox"/>: opened by the arrow (every item, the selected one
''' highlighted) or by typing with <see cref="KBotComboBox.FindAsYouType"/> (only the matching
''' items). Slice 0094: before it there was also a native ComboBox list; now this window is the
''' only one, so the two ways of choosing look and behave the same.
'''
''' <para><b>It never takes the focus.</b> <c>WS_EX_NOACTIVATE</c> plus
''' <c>ShowWithoutActivation</c> plus <c>MA_NOACTIVATE</c> on a click: the caret stays in the
''' combo, the keys keep arriving there, and the combo drives this list (Up/Down/PageUp/PageDown,
''' Enter, Escape). A click on a row is reported through <see cref="RowChosen"/>.</para>
'''
''' <para>Every colour and the font are read from the owning combo at paint time, so there is
''' nothing of its own to theme; it implements <see cref="IThemedControl"/> only so the generic
''' traversal leaves it alone.</para>
''' </summary>
Friend NotInheritable Class KBotComboList
    Inherits Form
    Implements IThemedControl

    Private Const WS_EX_NOACTIVATE As Integer = &H8000000
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const CS_DROPSHADOW As Integer = &H20000
    Private Const WM_MOUSEACTIVATE As Integer = &H21
    Private Const MA_NOACTIVATE As Integer = 3

    ' Width of the scroll track, logical px. Wide enough to grab with the mouse.
    Private Const SCROLL_WIDTH As Integer = 8

    Private ReadOnly _combo As KBotComboBox
    Private ReadOnly _rows As New List(Of KBotComboListRow)()
    Private _selected As Integer = -1
    Private _top As Integer = 0

    ' Thumb drag: the offset between the press point and the thumb's top.
    Private _dragging As Boolean = False
    Private _dragOffset As Integer = 0

    ''' <summary>A row was clicked. The argument is the index in the COMBO's items.</summary>
    Public Event RowChosen(itemIndex As Integer)
    ''' <summary>The «new item» row (<see cref="KBotComboBox.OfferNewItem"/>) was clicked.</summary>
    Public Event NewItemChosen()

    Public Sub New(combo As KBotComboBox)
        ArgumentNullException.ThrowIfNull(combo)
        _combo = combo
        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        ControlBox = False
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_NOACTIVATE Or WS_EX_TOOLWINDOW
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW
            Return cp
        End Get
    End Property

    ' A click must not activate the window: the focus -- and the caret -- stay in the combo.
    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = WM_MOUSEACTIVATE Then
            m.Result = New IntPtr(MA_NOACTIVATE)
            Return
        End If
        MyBase.WndProc(m)
    End Sub

    ''' <summary>How many rows are shown.</summary>
    Public ReadOnly Property RowCount As Integer
        Get
            Return _rows.Count
        End Get
    End Property

    ''' <summary>The highlighted row's index in the COMBO's items; -1 = none.</summary>
    Public ReadOnly Property SelectedItemIndex As Integer
        Get
            If _selected < 0 OrElse _selected >= _rows.Count Then Return -1
            Return _rows(_selected).ItemIndex
        End Get
    End Property

    ''' <summary>Is the highlighted row the «new item» row?</summary>
    Public ReadOnly Property SelectedIsNewItem As Boolean
        Get
            Return _selected >= 0 AndAlso _selected < _rows.Count AndAlso _rows(_selected).IsNewItem
        End Get
    End Property

    ''' <summary>
    ''' Shows <paramref name="rows"/> under (or, without room below, above) the screen rectangle
    ''' <paramref name="anchor"/>, at least <paramref name="minWidth"/> wide.
    ''' <paramref name="highlight"/> is the ROW to highlight and scroll into view (-1 = none).
    ''' </summary>
    Public Sub ShowRows(rows As List(Of KBotComboListRow), owner As Form, anchor As Rectangle,
                        minWidth As Integer, highlight As Integer)
        _rows.Clear()
        _rows.AddRange(rows)
        _selected = If(highlight >= 0 AndAlso highlight < _rows.Count, highlight, -1)
        _top = 0
        _dragging = False

        Place(anchor, minWidth)
        If _selected >= 0 Then EnsureVisible(_selected, centre:=True)

        If Not Visible Then
            If owner IsNot Nothing Then Show(owner) Else Show()
        End If
        Invalidate()
    End Sub

    ''' <summary>Follows the box when it moves or changes size while the list is open.</summary>
    Public Sub Reposition(anchor As Rectangle, minWidth As Integer)
        If Not Visible Then Return
        Place(anchor, minWidth)
        Invalidate()
    End Sub

    Private Sub Place(anchor As Rectangle, minWidth As Integer)
        Dim rowH As Integer = RowHeight()
        Dim shownRows As Integer = Math.Max(1, Math.Min(_rows.Count, Math.Max(1, _combo.MaxDropDownItems)))
        Dim w As Integer = Math.Max(anchor.Width, minWidth)
        Dim h As Integer = shownRows * rowH + 2

        Dim area As Rectangle = Screen.FromRectangle(anchor).WorkingArea
        Dim y As Integer = anchor.Bottom
        If y + h > area.Bottom Then
            Dim above As Integer = anchor.Top - h
            If above >= area.Top Then y = above
        End If
        Dim x As Integer = Math.Max(area.Left, Math.Min(anchor.Left, area.Right - w))
        Bounds = New Rectangle(x, y, w, h)
        _top = Math.Max(0, Math.Min(_top, MaxTop()))
    End Sub

    ''' <summary>Moves the highlight by <paramref name="delta"/> rows (a page = the visible
    ''' height), keeping it in view.</summary>
    Public Sub MoveSelection(delta As Integer)
        If _rows.Count = 0 Then Return
        _selected = Math.Max(0, Math.Min(_rows.Count - 1, If(_selected < 0, 0, _selected + delta)))
        EnsureVisible(_selected, centre:=False)
        Invalidate()
    End Sub

    ''' <summary>Highlights the first (<paramref name="toEnd"/> False) or last row.</summary>
    Public Sub MoveToEdge(toEnd As Boolean)
        If _rows.Count = 0 Then Return
        _selected = If(toEnd, _rows.Count - 1, 0)
        EnsureVisible(_selected, centre:=False)
        Invalidate()
    End Sub

    ''' <summary>Scrolls by <paramref name="rowsDelta"/> rows without moving the highlight.</summary>
    Public Sub ScrollBy(rowsDelta As Integer)
        _top = Math.Max(0, Math.Min(_top + rowsDelta, MaxTop()))
        Invalidate()
    End Sub

    ''' <summary>How many rows fit in the window.</summary>
    Public ReadOnly Property PageSize As Integer
        Get
            Return Math.Max(1, (ClientSize.Height - 2) \ Math.Max(1, RowHeight()))
        End Get
    End Property

    Private Function MaxTop() As Integer
        Return Math.Max(0, _rows.Count - PageSize)
    End Function

    Private Sub EnsureVisible(i As Integer, centre As Boolean)
        If centre AndAlso (i < _top OrElse i >= _top + PageSize) Then
            _top = i - PageSize \ 2
        Else
            If i < _top Then _top = i
            If i >= _top + PageSize Then _top = i - PageSize + 1
        End If
        _top = Math.Max(0, Math.Min(_top, MaxTop()))
    End Sub

    Private Function RowHeight() As Integer
        Return Math.Max(1, _combo.ItemHeight)
    End Function

    Private Function HasScrollBar() As Boolean
        Return _rows.Count > PageSize
    End Function

    Private Function ScrollWidthPx() As Integer
        Return If(HasScrollBar(), ThemeShapes.ScaleDpi(_combo, SCROLL_WIDTH), 0)
    End Function

    ' The scroll track and thumb, in client coordinates (inside the 1 px frame).
    Private Function TrackRect() As Rectangle
        Dim w As Integer = ScrollWidthPx()
        Return New Rectangle(ClientSize.Width - 1 - w, 1, w, Math.Max(0, ClientSize.Height - 2))
    End Function

    Private Function ThumbRect() As Rectangle
        Dim track As Rectangle = TrackRect()
        If track.Width = 0 OrElse _rows.Count = 0 Then Return Rectangle.Empty
        Dim thumbH As Integer = Math.Max(ThemeShapes.ScaleDpi(_combo, 16), track.Height * PageSize \ _rows.Count)
        thumbH = Math.Min(thumbH, track.Height)
        Dim room As Integer = Math.Max(0, track.Height - thumbH)
        Dim maxT As Integer = Math.Max(1, MaxTop())
        Return New Rectangle(track.Left, track.Top + room * _top \ maxT, track.Width, thumbH)
    End Function

    Private Function RowAt(p As Point) As Integer
        If p.Y < 1 Then Return -1
        If HasScrollBar() AndAlso p.X >= TrackRect().Left Then Return -1
        Dim i As Integer = _top + (p.Y - 1) \ RowHeight()
        Return If(i >= 0 AndAlso i < _rows.Count AndAlso i < _top + PageSize, i, -1)
    End Function

    ' =====================================================================
    ' PAINTING -- every colour comes from the combo
    ' =====================================================================

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            Using b As New SolidBrush(_combo.BackColor)
                g.FillRectangle(b, ClientRectangle)
            End Using

            Dim rowH As Integer = RowHeight()
            Dim padX As Integer = ThemeShapes.ScaleDpi(_combo, 8)
            Dim scrollW As Integer = ScrollWidthPx()
            Dim last As Integer = Math.Min(_rows.Count - 1, _top + PageSize - 1)
            For i As Integer = _top To last
                Dim r As New Rectangle(1, 1 + (i - _top) * rowH, ClientSize.Width - 2 - scrollW, rowH)
                Dim sel As Boolean = (i = _selected)
                If sel Then
                    Using b As New SolidBrush(_combo.EffectiveSelectionBackColor)
                        g.FillRectangle(b, r)
                    End Using
                End If
                Dim textArea As New Rectangle(r.Left + padX, r.Top, Math.Max(0, r.Width - padX), r.Height)
                Dim fore As Color = If(sel, _combo.EffectiveSelectionForeColor, _combo.ForeColor)
                Const flags As TextFormatFlags = TextFormatFlags.VerticalCenter Or TextFormatFlags.Left Or
                                                 TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix Or
                                                 TextFormatFlags.SingleLine
                If _rows(i).IsNewItem Then
                    ' The «new item» row is an action, not a value: italic, so it never reads as one.
                    Using f As New Font(_combo.Font, FontStyle.Italic)
                        TextRenderer.DrawText(g, _rows(i).Caption, f, textArea, fore, flags)
                    End Using
                Else
                    TextRenderer.DrawText(g, _rows(i).Caption, _combo.Font, textArea, fore, flags)
                End If
            Next

            ' The thumb says "there is more than you see"; it can be dragged, the track paged.
            If scrollW > 0 Then
                Using b As New SolidBrush(_combo.EffectiveBorderColor)
                    g.FillRectangle(b, ThumbRect())
                End Using
            End If

            Using pen As New Pen(_combo.EffectiveBorderColor)
                g.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboList.OnPaint", ex)
        End Try
    End Sub

    ' =====================================================================
    ' MOUSE -- hover highlights, a click chooses, the wheel and the thumb scroll
    ' =====================================================================

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            If _dragging Then
                Dim track As Rectangle = TrackRect()
                Dim thumb As Rectangle = ThumbRect()
                Dim room As Integer = Math.Max(1, track.Height - thumb.Height)
                Dim y As Integer = Math.Max(0, Math.Min(room, e.Y - _dragOffset - track.Top))
                Dim newTop As Integer = CInt(Math.Round(y * MaxTop() / CDbl(room)))
                If newTop <> _top Then
                    _top = newTop
                    Invalidate()
                End If
                Return
            End If
            Dim i As Integer = RowAt(e.Location)
            If i >= 0 AndAlso i <> _selected Then
                _selected = i
                Invalidate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboList.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button <> MouseButtons.Left Then Return

            If HasScrollBar() AndAlso e.X >= TrackRect().Left Then
                Dim thumb As Rectangle = ThumbRect()
                If thumb.Contains(e.Location) Then
                    _dragging = True
                    _dragOffset = e.Y - thumb.Top
                Else
                    ScrollBy(If(e.Y < thumb.Top, -PageSize, PageSize))
                End If
                Return
            End If

            Dim i As Integer = RowAt(e.Location)
            If i < 0 Then Return
            If _rows(i).IsNewItem Then
                RaiseEvent NewItemChosen()
            Else
                RaiseEvent RowChosen(_rows(i).ItemIndex)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboList.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        _dragging = False
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Try
            ScrollBy(-Math.Sign(e.Delta) * Math.Max(1, SystemInformation.MouseWheelScrollLines))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboList.OnMouseWheel", ex)
        End Try
    End Sub

    ''' <summary>Nothing of its own to theme -- the colours are the combo's, read at paint time.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Invalidate()
    End Sub
End Class

''' <summary>One row of the list: what it shows and which of the combo's items it stands for.
''' The «new item» row stands for none (<see cref="ItemIndex"/> = -1).</summary>
Friend NotInheritable Class KBotComboListRow
    Public ReadOnly Property ItemIndex As Integer
    Public ReadOnly Property Caption As String
    Public ReadOnly Property IsNewItem As Boolean

    Public Sub New(itemIndex As Integer, caption As String)
        Me.ItemIndex = itemIndex
        Me.Caption = If(caption, String.Empty)
    End Sub

    ''' <summary>The row <see cref="KBotComboBox.OfferNewItem"/> shows when nothing matches.</summary>
    Public Shared Function NewItem(caption As String) As KBotComboListRow
        Dim r As New KBotComboListRow(-1, caption)
        r._IsNewItem = True
        Return r
    End Function
End Class
