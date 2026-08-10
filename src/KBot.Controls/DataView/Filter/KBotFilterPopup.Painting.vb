Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' PICTAREA și INPUT-ul lui <see cref="KBotFilterPopup"/>. Totul e desenat de noi, din culorile
''' schemei — vezi comentariul clasei principale pentru de ce nu e un <c>ContextMenuStrip</c> cu un
''' <c>CheckedListBox</c> înăuntru.
'''
''' <para>Metodele de desen NU-și poartă propriul <c>Try</c>: sunt chemate doar din
''' <see cref="OnPaint"/>, care e boundary-ul care loghează și înghite (regula de acoperire
''' tranzitivă din CLAUDE.md).</para>
''' </summary>
Partial Class KBotFilterPopup

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Recalc()
            Dim g As Graphics = e.Graphics

            Using b As New SolidBrush(_cBack)
                g.FillRectangle(b, ClientRectangle)
            End Using

            DrawMenuRows(g)
            DrawSelectAllRow(g)
            DrawValueRows(g)
            DrawButtons(g)

            Using p As New Pen(_cBorder)
                g.DrawRectangle(p, 0, 0, Width - 1, Height - 1)
            End Using
        Catch ex As Exception
            ' Boundary de pictare: un throw de aici ar dărâma procesul.
            GlobalErrorLog.Write("KBotFilterPopup.OnPaint", ex)
        End Try
    End Sub

    Private Sub DrawMenuRows(g As Graphics)
        For i As Integer = 0 To _menu.Count - 1
            Dim r As MenuRow = _menu(i)

            If r.Kind = MenuRowKind.Separator Then
                Dim y As Integer = r.Bounds.Top + r.Bounds.Height \ 2
                Using p As New Pen(_cSeparator)
                    g.DrawLine(p, r.Bounds.Left + Sc(PadXLogical), y, r.Bounds.Right - Sc(PadXLogical), y)
                End Using
                Continue For
            End If

            Dim hot As Boolean = (i = _hotMenu) AndAlso r.Enabled
            If hot Then
                Using b As New SolidBrush(_cHighlightBack)
                    g.FillRectangle(b, r.Bounds)
                End Using
            End If

            Dim culoare As Color = If(Not r.Enabled, _cDisabled, If(hot, _cHighlightText, _cText))
            Dim textRect As New Rectangle(r.Bounds.Left + Sc(PadXLogical) + IconGutter(), r.Bounds.Top,
                                          r.Bounds.Width - 2 * Sc(PadXLogical) - IconGutter(), r.Bounds.Height)
            TextRenderer.DrawText(g, r.Text, Font, textRect, culoare,
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                  TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)

            ' Sortarea ACTIVĂ poartă un semn în banda din stânga: fără el, meniul n-ar spune
            ' niciodată după ce e sortată coloana pe care tocmai s-a deschis.
            If EsteSortareaActiva(r.Kind) Then
                DrawSortMark(g, New Rectangle(r.Bounds.Left + Sc(PadXLogical), r.Bounds.Top,
                                              IconGutter(), r.Bounds.Height),
                             r.Kind = MenuRowKind.SortAscending, If(hot, _cHighlightText, _cAccent))
            End If
        Next
    End Sub

    Private Function EsteSortareaActiva(kind As MenuRowKind) As Boolean
        Select Case kind
            Case MenuRowKind.SortAscending
                Return _currentSort = KBotSortDirection.Ascending
            Case MenuRowKind.SortDescending
                Return _currentSort = KBotSortDirection.Descending
            Case Else
                Return False
        End Select
    End Function

    ' Săgeata de sortare — în sus pentru crescător, în jos pentru descrescător.
    Private Sub DrawSortMark(g As Graphics, zona As Rectangle, ascending As Boolean, culoare As Color)
        Dim latura As Integer = Math.Max(6, Sc(8))
        Dim cx As Integer = zona.Left + zona.Width \ 2
        Dim cy As Integer = zona.Top + zona.Height \ 2
        Dim jum As Integer = latura \ 2

        Dim puncte As Point()
        If ascending Then
            puncte = New Point() {New Point(cx, cy - jum), New Point(cx + jum, cy + jum), New Point(cx - jum, cy + jum)}
        Else
            puncte = New Point() {New Point(cx, cy + jum), New Point(cx + jum, cy - jum), New Point(cx - jum, cy - jum)}
        End If

        Dim vechi As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            Using b As New SolidBrush(culoare)
                g.FillPolygon(b, puncte)
            End Using
        Finally
            g.SmoothingMode = vechi
        End Try
    End Sub

    ' Banda din stânga rândurilor de meniu (semnul de sortare) — aceeași lățime ca o casetă de
    ' bifat, ca textele din meniu și cele din listă să înceapă pe aceeași verticală.
    Private Function IconGutter() As Integer
        Return Sc(CheckBoxLogical) + Sc(CheckGapLogical)
    End Function

    Private Sub DrawSelectAllRow(g As Graphics)
        Dim hot As Boolean = (_hotMenu = HotSelectAll)
        DrawCheckRow(g, _selectAllRect, "(Selectează tot)", ToateAratateBifate(), hot,
                     _shown.Count > 0 AndAlso Not ToateAratateBifate() AndAlso AreBifeAmestecate())
    End Sub

    ' Bifă „amestecată” (unele da, altele nu) — pătratul plin din mijloc, ca la Explorer.
    Private Function AreBifeAmestecate() As Boolean
        Dim bifate As Integer = 0
        For Each i In _shown
            If _checked.Contains(_values(i)) Then bifate += 1
        Next
        Return bifate > 0 AndAlso bifate < _shown.Count
    End Function

    Private Sub DrawValueRows(g As Graphics)
        If _listRect.Height <= 0 Then Return
        Dim rowH As Integer = RowHeight()

        g.SetClip(_listRect)
        Try
            Dim fereastra As Integer = ListWindow()
            For slot As Integer = 0 To fereastra - 1
                Dim idx As Integer = _listScroll + slot
                If idx < 0 OrElse idx >= _shown.Count Then Continue For

                Dim r As New Rectangle(_listRect.Left, _listRect.Top + slot * rowH, _listRect.Width, rowH)
                Dim valoare As String = _values(_shown(idx))
                DrawCheckRow(g, r, EtichetaValorii(valoare), _checked.Contains(valoare),
                             idx = _hotValue, False)
            Next
        Finally
            g.ResetClip()
        End Try

        DrawListScrollHint(g)
    End Sub

    ' Un indicator subțire de derulare, cât timp lista are mai multe valori decât încap. Nu e o
    ' bară adevărată (nu se trage de ea) — e semnul că mai e ceva dedesubt, care altfel lipsește
    ' cu totul dintr-o listă tăiată exact la marginea de jos.
    Private Sub DrawListScrollHint(g As Graphics)
        If _shown.Count <= ListWindow() OrElse _listRect.Height <= 0 Then Return

        Dim latime As Integer = Math.Max(3, Sc(4))
        Dim sina As New Rectangle(_listRect.Right - latime - BorderThickness, _listRect.Top,
                                  latime, _listRect.Height)
        Dim inaltime As Integer = Math.Max(Sc(16), CInt(sina.Height * (ListWindow() / CDbl(_shown.Count))))
        Dim maximScroll As Integer = Math.Max(1, _shown.Count - ListWindow())
        Dim y As Integer = sina.Top + CInt((sina.Height - inaltime) * (_listScroll / CDbl(maximScroll)))

        Using b As New SolidBrush(Color.FromArgb(70, _cText))
            Using path As GraphicsPath = ThemeShapes.RoundedRect(New Rectangle(sina.Left, y, latime, inaltime), latime \ 2)
                g.FillPath(b, path)
            End Using
        End Using
    End Sub

    ' Un rând cu casetă de bifat: caseta la stânga, textul lângă ea.
    Private Sub DrawCheckRow(g As Graphics, r As Rectangle, text As String, checked As Boolean,
                             hot As Boolean, mixed As Boolean)
        If hot Then
            Using b As New SolidBrush(_cHighlightBack)
                g.FillRectangle(b, r)
            End Using
        End If

        Dim latura As Integer = Sc(CheckBoxLogical)
        Dim caseta As New Rectangle(r.Left + Sc(PadXLogical), r.Top + (r.Height - latura) \ 2, latura, latura)
        DrawCheckBox(g, caseta, checked, mixed)

        Dim textRect As New Rectangle(caseta.Right + Sc(CheckGapLogical), r.Top,
                                      r.Right - caseta.Right - Sc(CheckGapLogical) - Sc(PadXLogical), r.Height)
        TextRenderer.DrawText(g, text, Font, textRect, If(hot, _cHighlightText, _cText),
                              TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                              TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
    End Sub

    Private Sub DrawCheckBox(g As Graphics, r As Rectangle, checked As Boolean, mixed As Boolean)
        If checked OrElse mixed Then
            Using b As New SolidBrush(_cAccent)
                g.FillRectangle(b, r)
            End Using
        End If
        Using p As New Pen(If(checked OrElse mixed, _cAccent, _cBorder))
            g.DrawRectangle(p, r.Left, r.Top, r.Width - 1, r.Height - 1)
        End Using

        If mixed Then
            Dim inset As Integer = Math.Max(2, r.Width \ 4)
            Using b As New SolidBrush(_cAccentText)
                g.FillRectangle(b, Rectangle.Inflate(r, -inset, -inset))
            End Using
            Return
        End If

        If Not checked Then Return

        ' Bifa, trasată din trei puncte — nu un caracter, ca să nu depindă de fontul instalat.
        Dim vechi As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            Using p As New Pen(_cAccentText, Math.Max(1.4F, r.Width / 8.0F))
                p.StartCap = LineCap.Round
                p.EndCap = LineCap.Round
                Dim a As New PointF(r.Left + r.Width * 0.24F, r.Top + r.Height * 0.52F)
                Dim b As New PointF(r.Left + r.Width * 0.44F, r.Top + r.Height * 0.72F)
                Dim c As New PointF(r.Left + r.Width * 0.76F, r.Top + r.Height * 0.3F)
                g.DrawLines(p, New PointF() {a, b, c})
            End Using
        Finally
            g.SmoothingMode = vechi
        End Try
    End Sub

    Private Sub DrawButtons(g As Graphics)
        DrawButton(g, _okRect, "OK", _hotMenu = HotOk)
        DrawButton(g, _cancelRect, "Anulează", _hotMenu = HotCancel)
    End Sub

    Private Sub DrawButton(g As Graphics, r As Rectangle, text As String, hot As Boolean)
        Using b As New SolidBrush(If(hot, _cHighlightBack, _cButtonFace))
            g.FillRectangle(b, r)
        End Using
        Using p As New Pen(If(hot, _cHighlightBack, _cButtonBorder))
            g.DrawRectangle(p, r.Left, r.Top, r.Width - 1, r.Height - 1)
        End Using
        TextRenderer.DrawText(g, text, Font, r, If(hot, _cHighlightText, _cText),
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
                              TextFormatFlags.NoPrefix)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' INPUT — toți handler-ii sunt boundary UI: loghează și ÎNGHIT
    ' ══════════════════════════════════════════════════════════════════════════

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            Dim meniuVechi As Integer = _hotMenu
            Dim valoareVeche As Integer = _hotValue
            _hotMenu = HotNone
            _hotValue = -1

            If _okRect.Contains(e.Location) Then
                _hotMenu = HotOk
            ElseIf _cancelRect.Contains(e.Location) Then
                _hotMenu = HotCancel
            ElseIf _selectAllRect.Contains(e.Location) Then
                _hotMenu = HotSelectAll
            ElseIf _listRect.Contains(e.Location) Then
                _hotValue = ValoareLaPunct(e.Location)
            Else
                For i As Integer = 0 To _menu.Count - 1
                    If _menu(i).Kind = MenuRowKind.Separator Then Continue For
                    If _menu(i).Bounds.Contains(e.Location) Then
                        _hotMenu = i
                        Exit For
                    End If
                Next
            End If

            If _hotMenu <> meniuVechi OrElse _hotValue <> valoareVeche Then Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        Try
            If _hotMenu = HotNone AndAlso _hotValue < 0 Then Return
            _hotMenu = HotNone
            _hotValue = -1
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnMouseLeave", ex)
        End Try
    End Sub

    ' Indexul (în _shown) al valorii de sub un punct, sau -1.
    Private Function ValoareLaPunct(pt As Point) As Integer
        If Not _listRect.Contains(pt) Then Return -1
        Dim rowH As Integer = RowHeight()
        If rowH <= 0 Then Return -1
        Dim slot As Integer = (pt.Y - _listRect.Top) \ rowH
        Dim idx As Integer = _listScroll + slot
        If idx < 0 OrElse idx >= _shown.Count Then Return -1
        Return idx
    End Function

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button <> MouseButtons.Left Then Return

            If _okRect.Contains(e.Location) Then
                AcceptaFiltrul()
                Return
            End If
            If _cancelRect.Contains(e.Location) Then
                Close()
                Return
            End If
            If _selectAllRect.Contains(e.Location) Then
                ComutaToate()
                Return
            End If
            If _listRect.Contains(e.Location) Then
                ComutaValoarea(ValoareLaPunct(e.Location))
                Return
            End If

            For i As Integer = 0 To _menu.Count - 1
                Dim r As MenuRow = _menu(i)
                If r.Kind = MenuRowKind.Separator OrElse Not r.Bounds.Contains(e.Location) Then Continue For
                If Not r.Enabled Then Return          ' rând stins: apăsarea se consumă, nu face nimic
                ActiveazaRandDeMeniu(r)
                Return
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnMouseDown", ex)
        End Try
    End Sub

    Private Sub ActiveazaRandDeMeniu(r As MenuRow)
        Select Case r.Kind
            Case MenuRowKind.SortAscending
                CereSortare(KBotSortDirection.Ascending)
            Case MenuRowKind.SortDescending
                CereSortare(KBotSortDirection.Descending)
            Case MenuRowKind.ClearFilter
                StergeFiltrul()
            Case MenuRowKind.Conditions
                DeschideConditii(r.Bounds)
        End Select
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Try
            Dim pasi As Integer = e.Delta \ 120
            If pasi = 0 OrElse _shown.Count <= ListWindow() Then Return
            _listScroll -= pasi
            ClampScroll()
            _hotValue = ValoareLaPunct(PointToClient(Cursor.Position))
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnMouseWheel", ex)
        End Try
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            Select Case e.KeyCode
                Case Keys.Escape
                    e.SuppressKeyPress = True
                    Close()
                Case Keys.Enter
                    e.SuppressKeyPress = True
                    AcceptaFiltrul()
                Case Keys.Space
                    ' Bara comută valoarea survolată — drumul de la tastatură, egal cu mouse-ul.
                    If _hotValue >= 0 Then
                        e.SuppressKeyPress = True
                        ComutaValoarea(_hotValue)
                    End If
                Case Keys.Down, Keys.Up
                    e.SuppressKeyPress = True
                    MutaValoareaSurvolata(If(e.KeyCode = Keys.Down, 1, -1))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnKeyDown", ex)
        End Try
    End Sub

    ' Săgețile plimbă evidențierea prin lista de valori și derulează după ea.
    Private Sub MutaValoareaSurvolata(pas As Integer)
        If _shown.Count = 0 Then Return
        Dim nou As Integer = If(_hotValue < 0, 0, _hotValue + pas)
        nou = Math.Max(0, Math.Min(nou, _shown.Count - 1))
        _hotValue = nou
        If nou < _listScroll Then _listScroll = nou
        If nou >= _listScroll + ListWindow() Then _listScroll = nou - ListWindow() + 1
        ClampScroll()
        Invalidate()
    End Sub

End Class
