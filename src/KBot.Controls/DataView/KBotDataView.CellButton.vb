Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Windows.Forms

''' <summary>
''' Button cells (<see cref="KBotColumnType.Button"/>): placement, drawing, measuring and
''' hit-testing of the button face inside its cell (slice 0085-02).
'''
''' <para>One function decides where the face is (<see cref="ButtonFaceRect"/>) and every other
''' piece reads it: the painter draws on it, the auto-size pass measures what it needs, and the
''' mouse-up test checks the click against it. Two formulas would mean a column measured for one
''' button and a click accepted on another.</para>
'''
''' <para>The face is the cell minus <see cref="KBotDataColumn.ButtonMargin"/>, cut down to
''' <see cref="KBotDataColumn.ButtonSize"/> when that is set and placed by
''' <see cref="KBotDataColumn.ButtonAlign"/>. Inside it, minus
''' <see cref="KBotDataColumn.ButtonPadding"/>, the picture and the caption are laid out as one
''' group, centred. All metrics are logical and scaled with <c>ScaleDpi</c> at use.</para>
''' </summary>
Partial Class KBotDataView

    ' Gap between the picture and the caption (logical px).
    Private Const ButtonImageGap As Integer = 4

    ''' <summary>
    ''' The text a cell starts with, before <c>CellFormatting</c>: the column's fixed button
    ''' caption when it has one, otherwise the formatted value.
    ''' </summary>
    Private Shared Function DefaultCellText(k_value As Object, col As KBotDataColumn) As String
        If col.ColumnType = KBotColumnType.Button AndAlso Not String.IsNullOrEmpty(col.ButtonText) Then
            Return col.ButtonText
        End If
        Return FormatValue(k_value, col)
    End Function

    ''' <summary>
    ''' What a button actually shows: its text; when that is empty, the column header -- unless a
    ''' picture is set, then nothing (an icon-only button must not grow a caption by itself).
    ''' </summary>
    Private Shared Function ButtonCaptionFor(col As KBotDataColumn, k_text As String) As String
        If Not String.IsNullOrEmpty(k_text) Then Return k_text
        If col.ButtonImage IsNot Nothing Then Return String.Empty
        Return col.HeaderText
    End Function

    ''' <summary>True when the button face is flat: no fill and no border.</summary>
    Private Shared Function ButtonIsFlat(col As KBotDataColumn) As Boolean
        Dim k_color As Color = col.ButtonBackColor
        Return Not k_color.IsEmpty AndAlso k_color.A = 0
    End Function

    ''' <summary>
    ''' The button face inside a cell: the cell minus the margin, cut to the requested size and
    ''' placed by the alignment. A size of 0 on an axis fills that axis.
    ''' </summary>
    Friend Function ButtonFaceRect(col As KBotDataColumn, k_cellRect As Rectangle) As Rectangle
        Dim k_margin As Padding = col.ButtonMargin
        Dim k_left As Integer = ScaleDpi(k_margin.Left)
        Dim k_top As Integer = ScaleDpi(k_margin.Top)
        Dim k_avail As New Rectangle(k_cellRect.Left + k_left, k_cellRect.Top + k_top,
                                     Math.Max(0, k_cellRect.Width - k_left - ScaleDpi(k_margin.Right)),
                                     Math.Max(0, k_cellRect.Height - k_top - ScaleDpi(k_margin.Bottom)))

        Dim k_size As Size = col.ButtonSize
        Dim k_w As Integer = If(k_size.Width > 0, Math.Min(ScaleDpi(k_size.Width), k_avail.Width), k_avail.Width)
        Dim k_h As Integer = If(k_size.Height > 0, Math.Min(ScaleDpi(k_size.Height), k_avail.Height), k_avail.Height)

        Dim k_x As Integer
        Select Case col.ButtonAlign
            Case ContentAlignment.TopLeft, ContentAlignment.MiddleLeft, ContentAlignment.BottomLeft
                k_x = k_avail.Left
            Case ContentAlignment.TopRight, ContentAlignment.MiddleRight, ContentAlignment.BottomRight
                k_x = k_avail.Right - k_w
            Case Else
                k_x = k_avail.Left + (k_avail.Width - k_w) \ 2
        End Select

        Dim k_y As Integer
        Select Case col.ButtonAlign
            Case ContentAlignment.TopLeft, ContentAlignment.TopCenter, ContentAlignment.TopRight
                k_y = k_avail.Top
            Case ContentAlignment.BottomLeft, ContentAlignment.BottomCenter, ContentAlignment.BottomRight
                k_y = k_avail.Bottom - k_h
            Case Else
                k_y = k_avail.Top + (k_avail.Height - k_h) \ 2
        End Select

        Return New Rectangle(k_x, k_y, k_w, k_h)
    End Function

    ''' <summary>True when <paramref name="pt"/> is on the button face of that cell.</summary>
    Private Function IsOnCellButton(col As KBotDataColumn, rowIndex As Integer, pt As Point) As Boolean
        If col Is Nothing OrElse col.ColumnType <> KBotColumnType.Button Then Return False
        Dim k_cell As Rectangle = CellRect(col, rowIndex)
        If k_cell.Width <= 0 OrElse k_cell.Height <= 0 Then Return False
        Return ButtonFaceRect(col, k_cell).Contains(pt)
    End Function

    ' The face minus the inner padding: where the picture and the caption go.
    Private Function ButtonContentRect(col As KBotDataColumn, k_face As Rectangle) As Rectangle
        Dim k_pad As Padding = col.ButtonPadding
        Dim k_left As Integer = ScaleDpi(k_pad.Left)
        Dim k_top As Integer = ScaleDpi(k_pad.Top)
        Return New Rectangle(k_face.Left + k_left, k_face.Top + k_top,
                             Math.Max(0, k_face.Width - k_left - ScaleDpi(k_pad.Right)),
                             Math.Max(0, k_face.Height - k_top - ScaleDpi(k_pad.Bottom)))
    End Function

    ' Natural size of the picture at this DPI, shrunk (proportions kept) to fit the content area.
    Private Function ButtonImageSize(k_image As Image, k_content As Rectangle) As Size
        If k_image Is Nothing OrElse k_content.Width <= 0 OrElse k_content.Height <= 0 Then Return Size.Empty
        Dim k_w As Double = ScaleDpi(k_image.Width)
        Dim k_h As Double = ScaleDpi(k_image.Height)
        If k_w <= 0 OrElse k_h <= 0 Then Return Size.Empty
        Dim k_ratio As Double = Math.Min(1.0, Math.Min(k_content.Width / k_w, k_content.Height / k_h))
        Return New Size(Math.Max(1, CInt(Math.Floor(k_w * k_ratio))), Math.Max(1, CInt(Math.Floor(k_h * k_ratio))))
    End Function

    ''' <summary>
    ''' Width a button column needs to show its widest caption plus the picture, the inner
    ''' padding and the outer margin. With a fixed <see cref="KBotDataColumn.ButtonSize"/> width
    ''' the content is cut to it, so the need is just that width plus the margin.
    ''' </summary>
    Private Function ButtonColumnNeed(col As KBotDataColumn) As Integer
        Dim k_margin As Padding = col.ButtonMargin
        Dim k_marginX As Integer = ScaleDpi(k_margin.Left) + ScaleDpi(k_margin.Right)
        If col.ButtonSize.Width > 0 Then Return ScaleDpi(col.ButtonSize.Width) + k_marginX

        Dim k_pad As Padding = col.ButtonPadding
        Dim k_textW As Integer = MeasureSampledCells(col)
        Dim k_imageW As Integer = If(col.ButtonImage Is Nothing, 0, ScaleDpi(col.ButtonImage.Width))
        Dim k_gap As Integer = If(k_textW > 0 AndAlso k_imageW > 0, ScaleDpi(ButtonImageGap), 0)
        Return k_textW + k_imageW + k_gap + ScaleDpi(k_pad.Left) + ScaleDpi(k_pad.Right) + k_marginX
    End Function

    ' Is any border line drawn? Not when no side is chosen, nor when the border colour is
    ' transparent, nor on a flat button (transparent face) whose border colour was never set.
    Private Shared Function ButtonBorderVisible(col As KBotDataColumn) As Boolean
        If col.ButtonBorders = KBotBorderSides.None Then Return False
        Dim k_color As Color = col.ButtonBorderColor
        If k_color.IsEmpty Then Return Not ButtonIsFlat(col)
        Return k_color.A > 0
    End Function

    ' Face fill + border. All four sides = the rounded outline of old; any other choice = a plain
    ' rectangle with straight lines on the chosen sides. Covered by the Try in OnPaint.
    Private Sub DrawButtonFace(g As Graphics, col As KBotDataColumn, k_face As Rectangle, k_enabled As Boolean)
        Dim k_fill As Boolean = Not ButtonIsFlat(col)
        Dim k_border As Boolean = ButtonBorderVisible(col)
        If Not k_fill AndAlso Not k_border Then Return

        ' Disabled cells keep the greyed border whatever colour was asked.
        Dim k_ownPen As Pen = Nothing
        Dim k_pen As Pen = _pButtonBorder
        If Not k_enabled Then
            k_pen = _pDisabledMark
        ElseIf Not col.ButtonBorderColor.IsEmpty Then
            k_ownPen = New Pen(col.ButtonBorderColor)
            k_pen = k_ownPen
        End If

        Try
            Dim k_brushOwn As SolidBrush = Nothing
            Dim k_brush As SolidBrush = _bButtonFace
            If k_fill AndAlso Not col.ButtonBackColor.IsEmpty Then
                k_brushOwn = New SolidBrush(col.ButtonBackColor)
                k_brush = k_brushOwn
            End If

            Try
                If col.ButtonBorders = KBotBorderSides.All Then
                    Dim k_oldSmooth As SmoothingMode = g.SmoothingMode
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    Using k_path As GraphicsPath = RoundedRect(k_face, ScaleDpi(3))
                        If k_fill Then g.FillPath(k_brush, k_path)
                        If k_border Then g.DrawPath(k_pen, k_path)
                    End Using
                    g.SmoothingMode = k_oldSmooth
                Else
                    If k_fill Then g.FillRectangle(k_brush, k_face)
                    If k_border Then DrawButtonSides(g, k_face, col.ButtonBorders, k_pen)
                End If
            Finally
                k_brushOwn?.Dispose()
            End Try
        Finally
            k_ownPen?.Dispose()
        End Try
    End Sub

    ' One straight line per chosen side, on the outermost pixels of the face.
    Private Shared Sub DrawButtonSides(g As Graphics, k_face As Rectangle, k_sides As KBotBorderSides, k_pen As Pen)
        Dim k_right As Integer = k_face.Right - 1
        Dim k_bottom As Integer = k_face.Bottom - 1
        If (k_sides And KBotBorderSides.Left) <> 0 Then g.DrawLine(k_pen, k_face.Left, k_face.Top, k_face.Left, k_bottom)
        If (k_sides And KBotBorderSides.Top) <> 0 Then g.DrawLine(k_pen, k_face.Left, k_face.Top, k_right, k_face.Top)
        If (k_sides And KBotBorderSides.Right) <> 0 Then g.DrawLine(k_pen, k_right, k_face.Top, k_right, k_bottom)
        If (k_sides And KBotBorderSides.Bottom) <> 0 Then g.DrawLine(k_pen, k_face.Left, k_bottom, k_right, k_bottom)
    End Sub

    ' Draws the whole button cell. Covered by the Try in OnPaint.
    Private Sub DrawButtonCell(g As Graphics, col As KBotDataColumn, k_cellRect As Rectangle,
                               k_caption As String, k_font As Font, k_enabled As Boolean)
        Dim k_face As Rectangle = ButtonFaceRect(col, k_cellRect)
        If k_face.Width <= 0 OrElse k_face.Height <= 0 Then Return

        DrawButtonFace(g, col, k_face, k_enabled)

        Dim k_content As Rectangle = ButtonContentRect(col, k_face)
        If k_content.Width <= 0 OrElse k_content.Height <= 0 Then Return

        Dim k_image As Image = col.ButtonImage
        Dim k_imageSize As Size = ButtonImageSize(k_image, k_content)
        Dim k_textW As Integer = If(String.IsNullOrEmpty(k_caption), 0,
                                    TextRenderer.MeasureText(k_caption, k_font).Width)
        Dim k_gap As Integer = If(k_imageSize.Width > 0 AndAlso k_textW > 0, ScaleDpi(ButtonImageGap), 0)
        k_textW = Math.Min(k_textW, Math.Max(0, k_content.Width - k_imageSize.Width - k_gap))

        ' Picture and caption are one group, centred in the content area; when it is wider than
        ' the area the picture keeps the left edge and the caption is cut with an ellipsis.
        Dim k_x As Integer = k_content.Left + Math.Max(0, (k_content.Width - k_imageSize.Width - k_gap - k_textW) \ 2)

        If k_imageSize.Width > 0 Then
            Dim k_dest As New Rectangle(k_x, k_content.Top + (k_content.Height - k_imageSize.Height) \ 2,
                                        k_imageSize.Width, k_imageSize.Height)
            If k_enabled Then
                g.DrawImage(k_image, k_dest)
            Else
                ' A disabled button looks inert: its picture is drawn faded.
                Using k_attrs As New ImageAttributes()
                    Dim k_matrix As New ColorMatrix()
                    k_matrix.Matrix33 = 0.4F
                    k_attrs.SetColorMatrix(k_matrix)
                    g.DrawImage(k_image, k_dest, 0, 0, k_image.Width, k_image.Height, GraphicsUnit.Pixel, k_attrs)
                End Using
            End If
            k_x += k_imageSize.Width + k_gap
        End If

        If k_textW > 0 Then
            TextRenderer.DrawText(g, k_caption, k_font,
                                  New Rectangle(k_x, k_content.Top, k_textW, k_content.Height),
                                  If(k_enabled, _cButtonText, _cDisabledText),
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                  TextFormatFlags.EndEllipsis)
        End If
    End Sub

End Class
