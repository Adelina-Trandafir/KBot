Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 00EF-09 -- the little pictures of the invoice tree, drawn from shapes in the colours of the theme palette (so they follow
''' a theme switch; the tree is rebuilt then): the state dot on the left of an invoice, the client mark on a root, the «more» sign that
''' shows on the right of an invoice when the mouse is over it, and the magnifier of the tree's search band. Cached by (shape,
''' colour, size): the bitmaps are never changed after they are drawn.
''' </summary>
Friend NotInheritable Class FacturaIcons

    Private Sub New()
    End Sub

    Private Shared ReadOnly _cache As New Dictionary(Of String, Image)(StringComparer.Ordinal)
    Private Shared ReadOnly _sync As New Object()

    ''' <summary>
    ''' The state of an invoice as a dot: grey ring = not sent, orange = sent and not confirmed, green = accepted, red = refused
    ''' (operator, 07.10.2026).
    ''' </summary>
    Friend Shared Function StateDot(k_stare As String, k_palette As ThemePalette, k_size As Integer) As Image
        Dim k_color As Color
        Dim k_hollow As Boolean
        Select Case k_stare
            Case EFacturaStare.Incarcata
                k_color = k_palette.WarningColor
            Case EFacturaStare.Acceptata
                k_color = k_palette.SuccessColor
            Case EFacturaStare.Refuzata
                k_color = k_palette.ErrorColor
            Case Else
                k_color = k_palette.TextDimColor
                k_hollow = True
        End Select
        Dim k_inner As Color = k_palette.SurfaceColor
        Return GetOrDraw($"dot:{k_hollow}:{k_color.ToArgb()}:{k_inner.ToArgb()}:{k_size}", k_size,
                         Sub(k_g)
                             Dim k_m As Single = k_size * 0.16F
                             Dim k_d As Single = k_size - 2 * k_m
                             If k_hollow Then
                                 Using k_fill As New SolidBrush(k_inner), k_pen As New Pen(k_color, Math.Max(1.5F, k_size * 0.12F))
                                     k_g.FillEllipse(k_fill, k_m, k_m, k_d, k_d)
                                     k_g.DrawEllipse(k_pen, k_m, k_m, k_d, k_d)
                                 End Using
                             Else
                                 Using k_brush As New SolidBrush(k_color)
                                     k_g.FillEllipse(k_brush, k_m, k_m, k_d, k_d)
                                 End Using
                             End If
                         End Sub)
    End Function

    ''' <summary>A person: the mark of a client (a root of the tree).</summary>
    Friend Shared Function Client(k_palette As ThemePalette, k_size As Integer) As Image
        Dim k_color As Color = k_palette.TextDimColor
        Return GetOrDraw($"client:{k_color.ToArgb()}:{k_size}", k_size,
                         Sub(k_g)
                             Using k_brush As New SolidBrush(k_color)
                                 Dim k_head As Single = k_size * 0.36F
                                 k_g.FillEllipse(k_brush, (k_size - k_head) / 2.0F, k_size * 0.1F, k_head, k_head)
                                 k_g.SetClip(New RectangleF(0, 0, k_size, k_size))
                                 k_g.FillEllipse(k_brush, k_size * 0.14F, k_size * 0.54F, k_size * 0.72F, k_size * 0.9F)
                             End Using
                         End Sub)
    End Function

    ''' <summary>Three dots one under the other: the sign that an invoice has a menu.</summary>
    Friend Shared Function More(k_palette As ThemePalette, k_size As Integer) As Image
        Dim k_color As Color = k_palette.TextColor
        Return GetOrDraw($"more:{k_color.ToArgb()}:{k_size}", k_size,
                         Sub(k_g)
                             Using k_brush As New SolidBrush(k_color)
                                 Dim k_r As Single = Math.Max(1.2F, k_size * 0.1F)
                                 For k_i As Integer = 0 To 2
                                     Dim k_y As Single = k_size * (0.2F + 0.3F * k_i)
                                     k_g.FillEllipse(k_brush, k_size / 2.0F - k_r, k_y - k_r, 2 * k_r, 2 * k_r)
                                 Next
                             End Using
                         End Sub)
    End Function

    ''' <summary>A magnifier, for the tree's search band.</summary>
    Friend Shared Function Search(k_palette As ThemePalette, k_size As Integer) As Image
        Dim k_color As Color = k_palette.TextColor
        Return GetOrDraw($"search:{k_color.ToArgb()}:{k_size}", k_size,
                         Sub(k_g)
                             Using k_pen As New Pen(k_color, Math.Max(1.5F, k_size * 0.1F))
                                 Dim k_d As Single = k_size * 0.55F
                                 k_g.DrawEllipse(k_pen, k_size * 0.12F, k_size * 0.12F, k_d, k_d)
                                 k_pen.StartCap = LineCap.Round
                                 k_pen.EndCap = LineCap.Round
                                 k_g.DrawLine(k_pen, k_size * 0.62F, k_size * 0.62F, k_size * 0.88F, k_size * 0.88F)
                             End Using
                         End Sub)
    End Function

    Private Shared Function GetOrDraw(k_key As String, k_size As Integer, k_paint As Action(Of Graphics)) As Image
        SyncLock _sync
            Dim k_cached As Image = Nothing
            If _cache.TryGetValue(k_key, k_cached) Then Return k_cached
            Dim k_bmp As New Bitmap(Math.Max(8, k_size), Math.Max(8, k_size))
            Using k_g As Graphics = Graphics.FromImage(k_bmp)
                k_g.SmoothingMode = SmoothingMode.AntiAlias
                k_paint(k_g)
            End Using
            _cache(k_key) = k_bmp
            Return k_bmp
        End SyncLock
    End Function

End Class
