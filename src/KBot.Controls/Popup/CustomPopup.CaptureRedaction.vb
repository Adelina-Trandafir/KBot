Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' Slice 0000-23: the TEXT of the rows of an open popup whose text is sensitive (the unit list of
''' the caption bar, for instance) -- not the row, not its icon -- for the help capture's blur
''' (<see cref="IKBotCaptureRedaction"/>). Same left edge as <c>DrawRow</c>.
''' </summary>
Partial Public Class CustomPopup
    Implements IKBotCaptureRedaction

    Public Function SensitiveRegions(isSensitive As Func(Of String, String, Boolean)) As IEnumerable(Of Rectangle) Implements IKBotCaptureRedaction.SensitiveRegions
        ArgumentNullException.ThrowIfNull(isSensitive)
        Dim result As New List(Of Rectangle)()
        Dim viewport As New Rectangle(0, 0, ClientSize.Width, ClientSize.Height)
        Dim padX As Integer = ThemeShapes.ScaleDpi(ScaleRef, PadXLogical)
        Dim gutter As Integer = IconGutter()
        For i As Integer = 0 To Items.Count - 1
            Dim item As CustomPopupItem = Items(i)
            If item.IsSeparator OrElse Not isSensitive(String.Empty, If(item.Text, String.Empty)) Then Continue For
            Dim row As Rectangle = RowBounds(i)
            Dim textLeft As Integer = row.Left + padX + gutter
            Dim w As Integer = Math.Min(TextRenderer.MeasureText(item.Text, Font).Width, Math.Max(0, row.Right - padX - textLeft))
            Dim h As Integer = Math.Min(row.Height, Font.Height + 2)
            Dim r As New Rectangle(textLeft, row.Top + (row.Height - h) \ 2, w, h)
            r.Offset(0, -ScrollOffset)
            r.Intersect(viewport)
            If Not r.IsEmpty Then result.Add(r)
        Next
        Return result
    End Function

End Class
