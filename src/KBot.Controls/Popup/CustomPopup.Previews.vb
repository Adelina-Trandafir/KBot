Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

' Slice 0097-09: optional thumbnail tiles for the held-Ctrl navigation selector.
Partial Public Class CustomPopup
    Private _previewColumns As Integer
    Private _previewOpeningPointer As Point
    Private _previewPointerMoved As Boolean

    Private Function AcceptPreviewHover() As Boolean
        If _previewPointerMoved Then Return True
        If System.Windows.Forms.Cursor.Position = _previewOpeningPointer Then Return False
        _previewPointerMoved = True
        Return True
    End Function

    <Category("K-BOT Popup"), DefaultValue(0)>
    <Description("Thumbnail columns. Zero keeps the ordinary command-list layout.")>
    Public Property PreviewColumns As Integer
        Get
            Return _previewColumns
        End Get
        Set(value As Integer)
            value = Math.Max(0, Math.Min(6, value))
            If value = _previewColumns Then Return
            _previewColumns = value
            InvalidateLayout()
        End Set
    End Property

    Private Sub RecalcPreviewLayout()
        Dim k_pad As Integer = ThemeShapes.ScaleDpi(ScaleRef, 6)
        Dim k_inset As Integer = ThemeShapes.ScaleDpi(ScaleRef, 3)
        ' Double the snapshot area on both axes; keep caption, icons and padding at their existing size.
        Dim k_chromeWidth As Integer = 2 * (k_pad + k_inset)
        Dim k_chromeHeight As Integer = k_chromeWidth + PreviewCaptionHeight()
        Dim k_width As Integer = k_chromeWidth + 2 * Math.Max(1, ThemeShapes.ScaleDpi(ScaleRef, 190) - k_chromeWidth)
        Dim k_height As Integer = k_chromeHeight + 2 * Math.Max(1, ThemeShapes.ScaleDpi(ScaleRef, 150) - k_chromeHeight)
        Dim k_area As Rectangle = Screen.FromControl(ScaleRef).WorkingArea
        Dim k_columns As Integer = Math.Max(1, Math.Min(_previewColumns, Math.Min(Math.Max(1, _items.Count),
            Math.Max(1, (k_area.Width - 2 * k_pad) \ Math.Max(1, k_width)))))
        _rows = If(_items.Count = 0, Array.Empty(Of Rectangle)(), New Rectangle(_items.Count - 1) {})
        For k_index As Integer = 0 To _items.Count - 1
            If _items(k_index).IsSeparator OrElse _items(k_index).IsSlider OrElse _items(k_index).IsInput Then
                Throw New InvalidOperationException("Preview tiles accept command items only.")
            End If
            _rows(k_index) = New Rectangle(k_pad + (k_index Mod k_columns) * k_width,
                k_pad + (k_index \ k_columns) * k_height, k_width, k_height)
        Next
        Dim k_rowCount As Integer = (_items.Count + k_columns - 1) \ k_columns
        _naturalSize = New Size(2 * k_pad + k_columns * k_width, 2 * k_pad + k_rowCount * k_height)
    End Sub

    Private Function PreviewCaptionHeight() As Integer
        Return Math.Max(ThemeShapes.ScaleDpi(ScaleRef, 32), RowFont().Height + 2 * ThemeShapes.ScaleDpi(ScaleRef, 6))
    End Function

    ' Called only from the wrapped OnPaint; all images are borrowed from the application's snapshot store.
    Private Sub DrawPreviewTile(g As Graphics, index As Integer, bounds As Rectangle)
        Dim k_item As CustomPopupItem = _items(index)
        Dim k_selected As Boolean = index = SelectedIndex
        Dim k_pad As Integer = ThemeShapes.ScaleDpi(ScaleRef, 6)
        Dim k_tile As Rectangle = Rectangle.Inflate(bounds, -ThemeShapes.ScaleDpi(ScaleRef, 3), -ThemeShapes.ScaleDpi(ScaleRef, 3))
        Using k_brush As New SolidBrush(If(k_selected, EffectiveHighlightBackColor, EffectiveBackColor))
            g.FillRectangle(k_brush, k_tile)
        End Using
        Using k_pen As New Pen(If(k_selected, EffectiveHighlightForeColor, EffectiveBorderColor), ThemeShapes.ScaleDpi(ScaleRef, 1))
            g.DrawRectangle(k_pen, k_tile)
        End Using
        Dim k_captionHeight As Integer = PreviewCaptionHeight()
        Dim k_preview As New Rectangle(k_tile.Left + k_pad, k_tile.Top + k_pad,
            Math.Max(0, k_tile.Width - 2 * k_pad), Math.Max(0, k_tile.Height - k_captionHeight - 2 * k_pad))
        Dim k_image As Image = If(k_item.PreviewImage, k_item.Image)
        If k_image IsNot Nothing AndAlso k_preview.Width > 0 AndAlso k_preview.Height > 0 Then
            Dim k_ratio As Double = Math.Min(k_preview.Width / CDbl(k_image.Width), k_preview.Height / CDbl(k_image.Height))
            If k_item.PreviewImage Is Nothing Then
                k_ratio = Math.Min(k_ratio, ThemeShapes.ScaleDpi(ScaleRef, 40) / CDbl(Math.Max(k_image.Width, k_image.Height)))
            End If
            Dim k_size As New Size(Math.Max(1, CInt(k_image.Width * k_ratio)), Math.Max(1, CInt(k_image.Height * k_ratio)))
            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            g.DrawImage(k_image, New Rectangle(k_preview.Left + (k_preview.Width - k_size.Width) \ 2,
                k_preview.Top + (k_preview.Height - k_size.Height) \ 2, k_size.Width, k_size.Height))
        End If
        Dim k_caption As New Rectangle(k_tile.Left + 1, k_tile.Bottom - k_captionHeight, k_tile.Width - 2, k_captionHeight - 1)
        If IconBarVisible AndAlso Not k_selected Then
            Using k_brush As New SolidBrush(EffectiveIconBarColor)
                g.FillRectangle(k_brush, k_caption)
            End Using
        End If
        Dim k_side As Integer = ThemeShapes.ScaleDpi(ScaleRef, ImageSize)
        If k_item.Image IsNot Nothing Then
            KBotNavList.DrawItemImage(g, k_item.Image,
                New Rectangle(k_caption.Left + k_pad, k_caption.Top + (k_caption.Height - k_side) \ 2, k_side, k_side), k_item.Enabled)
        End If
        Dim k_text As New Rectangle(k_caption.Left + k_side + 2 * k_pad, k_caption.Top,
            Math.Max(0, k_caption.Width - k_side - 3 * k_pad), k_caption.Height)
        TextRenderer.DrawText(g, k_item.Text, RowFont(k_item.Italic), k_text,
            If(k_selected, EffectiveHighlightForeColor, EffectiveItemForeColor), MeasureFlags() Or TextFormatFlags.EndEllipsis)
    End Sub

    ''' <summary>Shows a preview grid centred on the anchor's client area, within its monitor.</summary>
    Public Sub ShowCentered(anchor As Control)
        ArgumentNullException.ThrowIfNull(anchor)
        If PreviewColumns <= 0 Then Throw New InvalidOperationException("A centred preview popup needs thumbnail columns.")
        _previewOpeningPointer = System.Windows.Forms.Cursor.Position
        _previewPointerMoved = False
        Dim k_area As Rectangle = anchor.RectangleToScreen(anchor.ClientRectangle)
        ShowCore(anchor, k_area.Location, k_area.Right, k_area.Bottom)
        Dim k_work As Rectangle = Screen.FromControl(anchor).WorkingArea
        Location = New Point(Math.Max(k_work.Left, Math.Min(k_area.Left + (k_area.Width - Width) \ 2, k_work.Right - Width)),
                             Math.Max(k_work.Top, Math.Min(k_area.Top + (k_area.Height - Height) \ 2, k_work.Bottom - Height)))
    End Sub
End Class
