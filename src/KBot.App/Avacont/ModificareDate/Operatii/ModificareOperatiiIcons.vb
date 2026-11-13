Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports KBot.Domain

''' <summary>
''' Slice AV02-05: the icons of the columns «Ord» and «R» of «Modificare operatii» (operator, 10.11.2026: icons, not words).
''' Drawn from GDI shapes in a colour of the theme palette, like <see cref="DdfIcons"/>: each state has its own SHAPE, not only its own colour,
''' so it reads without colour too. Cached per (state, colour, size); the bitmaps are shared and never disposed by the callers.
''' </summary>
Public NotInheritable Class ModificareOperatiiIcons

    Private Sub New()
    End Sub

    Private Shared ReadOnly _cache As New Dictionary(Of String, Image)(StringComparer.Ordinal)
    Private Shared ReadOnly _sync As New Object()

    ''' <summary>
    ''' The stage of the ordonantare: hollow circle = generated, left half filled = signed AB, three quarters filled = signed CD,
    ''' filled circle with a tick = signed by the Ordonator. Nothing = no ordonantare (no icon).
    ''' </summary>
    Public Shared Function OrdIcon(k_state As OperatieStareOrd, k_color As Color, k_tick As Color, k_size As Integer) As Image
        If k_state = OperatieStareOrd.Niciuna Then Return Nothing
        Return GetOrDraw($"ord:{k_state}:{k_color.ToArgb()}:{k_tick.ToArgb()}:{k_size}", k_size,
                         Sub(g) DrawOrd(g, k_state, k_color, k_tick, k_size))
    End Function

    ''' <summary>
    ''' The reception of the invoices: a filled square with a tick = all have one; a hollow square with its lower-left half filled = only some.
    ''' Nothing = none (no icon).
    ''' </summary>
    Public Shared Function ReceptieIcon(k_state As OperatieStareReceptie, k_color As Color, k_tick As Color, k_size As Integer) As Image
        If k_state = OperatieStareReceptie.Niciuna Then Return Nothing
        Return GetOrDraw($"rec:{k_state}:{k_color.ToArgb()}:{k_tick.ToArgb()}:{k_size}", k_size,
                         Sub(g) DrawReceptie(g, k_state, k_color, k_tick, k_size))
    End Function

    ''' <summary>
    ''' The kind of the document (the Access row picture by Documente.Fel): a rounded square with the first letter of the kind
    ''' (E, P, A, I...; «M» for a manual note).
    ''' </summary>
    Public Shared Function TipIcon(k_fel As String, k_color As Color, k_size As Integer) As Image
        Dim k_letter As String = If(String.IsNullOrWhiteSpace(k_fel), "M", k_fel.Trim().Substring(0, 1).ToUpperInvariant())
        Return GetOrDraw($"tip:{k_letter}:{k_color.ToArgb()}:{k_size}", k_size,
                         Sub(g) DrawTip(g, k_letter, k_color, k_size))
    End Function

    Private Shared Sub DrawTip(g As Graphics, k_letter As String, k_color As Color, k_size As Integer)
        Dim penW As Single = Math.Max(1.0F, k_size * 0.07F)
        Dim k_box As New RectangleF(penW, penW, k_size - 2 * penW - 1, k_size - 2 * penW - 1)
        Using k_pen As New Pen(k_color, penW), k_brush As New SolidBrush(k_color),
              k_font As New Font("Calibri", Math.Max(6.0F, k_size * 0.6F), FontStyle.Bold, GraphicsUnit.Pixel),
              k_format As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
            g.DrawRectangle(k_pen, k_box.X, k_box.Y, k_box.Width, k_box.Height)
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
            g.DrawString(k_letter, k_font, k_brush, k_box, k_format)
        End Using
    End Sub

    Private Shared Sub DrawOrd(g As Graphics, k_state As OperatieStareOrd, k_color As Color, k_tick As Color, k_size As Integer)
        Dim m As Single = k_size * 0.15F
        Dim d As Single = k_size - 2 * m
        Dim penW As Single = Math.Max(1.2F, k_size * 0.09F)
        Using k_brush As New SolidBrush(k_color), k_pen As New Pen(k_color, penW)
            Select Case k_state
                Case OperatieStareOrd.Generat
                    g.DrawEllipse(k_pen, m, m, d, d)
                Case OperatieStareOrd.SemnatAB
                    g.DrawEllipse(k_pen, m, m, d, d)
                    g.FillPie(k_brush, m, m, d, d, 90.0F, 180.0F)
                Case OperatieStareOrd.SemnatCD
                    g.DrawEllipse(k_pen, m, m, d, d)
                    g.FillPie(k_brush, m, m, d, d, 0.0F, 270.0F)
                Case OperatieStareOrd.Ordonator
                    g.FillEllipse(k_brush, m, m, d, d)
                    DrawTick(g, k_tick, k_size, m)
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(k_state), k_state, "Unknown ordonantare state.")
            End Select
        End Using
    End Sub

    Private Shared Sub DrawReceptie(g As Graphics, k_state As OperatieStareReceptie, k_color As Color, k_tick As Color, k_size As Integer)
        Dim m As Single = k_size * 0.17F
        Dim d As Single = k_size - 2 * m
        Dim penW As Single = Math.Max(1.2F, k_size * 0.09F)
        Using k_brush As New SolidBrush(k_color), k_pen As New Pen(k_color, penW)
            Select Case k_state
                Case OperatieStareReceptie.Toate
                    g.FillRectangle(k_brush, m, m, d, d)
                    DrawTick(g, k_tick, k_size, m)
                Case OperatieStareReceptie.Partiala
                    g.DrawRectangle(k_pen, m, m, d, d)
                    g.FillPolygon(k_brush, New PointF() {New PointF(m, m), New PointF(m, m + d), New PointF(m + d, m + d)})
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(k_state), k_state, "Unknown reception state.")
            End Select
        End Using
    End Sub

    Private Shared Sub DrawTick(g As Graphics, k_tick As Color, k_size As Integer, m As Single)
        Using k_pen As New Pen(k_tick, Math.Max(1.4F, k_size * 0.11F))
            k_pen.StartCap = LineCap.Round
            k_pen.EndCap = LineCap.Round
            k_pen.LineJoin = LineJoin.Round
            g.DrawLines(k_pen, New PointF() {
                New PointF(m + k_size * 0.12F, k_size * 0.52F),
                New PointF(k_size * 0.44F, k_size - m - k_size * 0.12F),
                New PointF(k_size - m - k_size * 0.1F, m + k_size * 0.12F)})
        End Using
    End Sub

    Private Shared Function GetOrDraw(k_key As String, k_size As Integer, k_paint As Action(Of Graphics)) As Image
        SyncLock _sync
            Dim k_cached As Image = Nothing
            If _cache.TryGetValue(k_key, k_cached) Then Return k_cached
            Dim k_bmp As New Bitmap(k_size, k_size)
            Using g As Graphics = Graphics.FromImage(k_bmp)
                g.SmoothingMode = SmoothingMode.AntiAlias
                k_paint(g)
            End Using
            _cache(k_key) = k_bmp
            Return k_bmp
        End SyncLock
    End Function

End Class
