Option Strict On
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The calendar of a date cell (operator, 29.10.2026). An editable Text column whose
''' <see cref="KBotDataColumn.ValueType"/> is DateTime paints a small calendar button on the right of
''' the cell; a click on it, or Alt+Down / F4 in the editor, drops a <see cref="KBotCalendarPopup"/>
''' under the cell. The chosen day is written into the editor as the cell's formatted text and
''' committed through the normal <c>CommitEdit</c> path, so <c>CellValidating</c> parses it exactly
''' as it parses typed text -- the host needs no code. Typing a date still works as before.
'''
''' <para>The button strip belongs to the grid (like the chevron of a combo cell): the painter keeps
''' the text out of it and the text editor stops before it, so a click on it never reaches the
''' editor.</para>
''' </summary>
Partial Class KBotDataView

    ' Width of the calendar button strip on the right of a date cell's content, logical px.
    Private Const DateButtonZone As Integer = 20

    ' The calendar while it is open; Nothing otherwise. The editor's Leave is ignored meanwhile.
    Private _datePopup As KBotCalendarPopup

    ' True while a chosen day is being committed: the popup is already closing itself then.
    Private _dateCommitting As Boolean

    ''' <summary>True when the cell of this column shows the calendar button: an editable Text column of dates.</summary>
    Private Function HasDateButton(k_col As KBotDataColumn) As Boolean
        If k_col Is Nothing OrElse _readOnlyGrid OrElse k_col.ReadOnly Then Return False
        Return k_col.ColumnType = KBotColumnType.Text AndAlso k_col.ValueType = KBotValueType.DateTime
    End Function

    ' The content rectangle without the button strip: where the text is painted and edited.
    Private Function DateTextRect(k_content As Rectangle) As Rectangle
        Return New Rectangle(k_content.Left, k_content.Top, Math.Max(0, k_content.Width - ScaleDpi(DateButtonZone)), k_content.Height)
    End Function

    ' The button strip of a cell, full cell height (a click anywhere in it opens the calendar).
    Private Function DateButtonRect(k_col As KBotDataColumn, k_cellBox As Rectangle) As Rectangle
        Dim k_content As Rectangle = CellContentRect(k_col, k_cellBox)
        Dim k_left As Integer = Math.Max(k_cellBox.Left, k_content.Right - ScaleDpi(DateButtonZone))
        Return New Rectangle(k_left, k_cellBox.Top, Math.Max(0, k_cellBox.Right - k_left), k_cellBox.Height)
    End Function

    ''' <summary>True when <paramref name="k_pt"/> is on the calendar button of a date cell.</summary>
    Private Function IsOnDateButton(k_col As KBotDataColumn, k_rowIndex As Integer, k_pt As Point) As Boolean
        If Not HasDateButton(k_col) Then Return False
        Dim k_cellBox As Rectangle = CellRect(k_col, k_rowIndex)
        If k_cellBox.Width <= 0 OrElse k_cellBox.Height <= 0 Then Return False
        Return DateButtonRect(k_col, k_cellBox).Contains(k_pt)
    End Function

    ' A small calendar: frame, a filled header band, two rings and a grid of days.
    Private Sub DrawDateGlyph(g As Graphics, k_content As Rectangle, k_enabled As Boolean)
        Dim k_zone As Integer = ScaleDpi(DateButtonZone)
        Dim k_size As Integer = ScaleDpi(12)
        If k_content.Width < k_zone OrElse k_content.Height < k_size Then Return
        Dim k_x As Integer = k_content.Right - k_zone + (k_zone - k_size) \ 2
        Dim k_y As Integer = k_content.Top + (k_content.Height - k_size) \ 2
        Dim k_brush As SolidBrush = If(k_enabled, _bComboChevron, _bDisabledMark)
        Dim k_head As Integer = Math.Max(2, k_size \ 4)
        Dim k_unit As Integer = Math.Max(1, k_size \ 6)
        Using k_pen As New Pen(k_brush.Color, Math.Max(1.0F, DeviceDpi / 96.0F))
            g.DrawRectangle(k_pen, k_x, k_y + k_unit, k_size - 1, k_size - k_unit - 1)
            g.DrawLine(k_pen, k_x + k_size \ 4, k_y, k_x + k_size \ 4, k_y + k_unit * 2)
            g.DrawLine(k_pen, k_x + k_size * 3 \ 4, k_y, k_x + k_size * 3 \ 4, k_y + k_unit * 2)
        End Using
        g.FillRectangle(k_brush, k_x, k_y + k_unit, k_size, k_head)
        Dim k_dot As Integer = Math.Max(1, k_size \ 6)
        For k_row As Integer = 0 To 1
            For k_colIdx As Integer = 0 To 2
                g.FillRectangle(k_brush, k_x + k_size \ 6 + k_colIdx * (k_size \ 3), k_y + k_unit + k_head + k_unit + k_row * (k_dot * 2), k_dot, k_dot)
            Next
        Next
    End Sub

    ''' <summary>
    ''' Opens the calendar under the cell being edited. No-op when the cell is not a date cell, nothing is edited or the
    ''' calendar is open. UI step: logs and swallows.
    ''' </summary>
    Private Sub ShowDateCalendar()
        Try
            If Not _editing OrElse _datePopup IsNot Nothing Then Return
            Dim k_col As KBotDataColumn = Column(_editColumnKey)
            If Not HasDateButton(k_col) Then Return
            ' The press that closed the calendar a moment ago must not open it again.
            If KBotCalendarPopup.ClosedJustNow Then Return
            Dim k_cellBox As Rectangle = CellRect(k_col, _editRowIndex)
            If k_cellBox.Width <= 0 OrElse k_cellBox.Height <= 0 Then Return

            Dim k_start As Date = Date.Today
            Dim k_typed As Date
            Dim k_stored As Object = _rows(_editRowIndex)(_editColumnKey)
            If Date.TryParse(editText.Text, CultureInfo.CurrentCulture, DateTimeStyles.None, k_typed) Then
                k_start = k_typed.Date
            ElseIf TypeOf k_stored Is Date Then
                k_start = DirectCast(k_stored, Date).Date
            End If

            Dim k_popup As New KBotCalendarPopup()
            k_popup.Calendar.Value = k_start
            AddHandler k_popup.DateCommitted, AddressOf OnDatePopupCommitted
            AddHandler k_popup.FormClosed, AddressOf OnDatePopupClosed
            _datePopup = k_popup
            k_popup.ShowBelow(Me, k_cellBox)
        Catch ex As Exception
            _datePopup = Nothing
            GlobalErrorLog.Write("KBotDataView.ShowDateCalendar", ex)
        End Try
    End Sub

    ' A day was chosen: it becomes the editor's text and is committed like typed text.
    Private Sub OnDatePopupCommitted(sender As Object, e As KBotDateSelectedEventArgs)
        Try
            If Not _editing Then Return
            Dim k_col As KBotDataColumn = Column(_editColumnKey)
            editText.Text = FormatValue(e.Value.Date, k_col)
            _dateCommitting = True
            CommitEdit()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnDatePopupCommitted", ex)
        Finally
            _dateCommitting = False
        End Try
    End Sub

    ' The calendar closed (a day chosen or dismissed): the editor, if still open, takes the keyboard back.
    Private Sub OnDatePopupClosed(sender As Object, e As FormClosedEventArgs)
        Try
            Dim k_popup As KBotCalendarPopup = TryCast(sender, KBotCalendarPopup)
            If k_popup IsNot Nothing Then
                RemoveHandler k_popup.DateCommitted, AddressOf OnDatePopupCommitted
                RemoveHandler k_popup.FormClosed, AddressOf OnDatePopupClosed
            End If
            _datePopup = Nothing
            If _editing AndAlso Not IsDisposed Then
                FocusActiveEditor()
                If editText.Visible Then editText.SelectAll()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnDatePopupClosed", ex)
        End Try
    End Sub

    ' The edit ended some other way (the cell moved, the grid was cleared): the calendar goes with it.
    Private Sub CloseDatePopup()
        Dim k_popup As KBotCalendarPopup = _datePopup
        If k_popup Is Nothing OrElse _dateCommitting Then Return
        _datePopup = Nothing
        If Not k_popup.IsDisposed Then k_popup.Close()
    End Sub

End Class
