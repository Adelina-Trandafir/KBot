Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D

''' <summary>
''' Iconițele arborelui de plăți (felia 0017), desenate din forme GDI, nu din resurse
''' binare: starea plății (sus = încărcată, jos = preluată, neutru = niciuna — oglindesc
''' REV_SUS / REV_JOS / REV_NOT din Access), acțiunea «+» (adaugă ordonanțare) și «toate»
''' (rădăcina « TOATE PLĂȚILE », oglindește PANUL). Se desenează cu o culoare dată (din
''' paleta temei), deci se re-tintează la schimbarea temei. Cache pe (fel, culoare,
''' dimensiune) — bitmap-urile sunt imutabile și partajate, exact ca <see cref="FxIcons"/> /
''' <see cref="ReceptiiIcons"/> / <see cref="RezervariIcons"/>.
''' </summary>
Public NotInheritable Class PlatiIcons

    ''' <summary>Starea vizuală a plății, derivată din Incarcat/Preluat.</summary>
    Public Enum Stare
        Neutru = 0
        Sus = 1
        Jos = 2
    End Enum

    Private Sub New()
    End Sub

    Private Shared ReadOnly _cache As New Dictionary(Of String, Image)(StringComparer.Ordinal)
    Private Shared ReadOnly _sync As New Object()

    ''' <summary>Iconița stării plății. Culoarea vine din paletă (verde/accent/estompat).</summary>
    Public Shared Function StatusIcon(stare As Stare, color As Color, size As Integer) As Image
        Return GetOrDraw($"plati:stat:{stare}:{color.ToArgb()}:{size}", size,
                         Sub(g) DrawStatus(g, stare, color, size))
    End Function

    ''' <summary>Iconița dreapta «+» (adaugă ordonanțare). Access folosește doar „Plus"
    ''' pentru plăți (fără varianta verde a rezervărilor).</summary>
    Public Shared Function PlusIcon(color As Color, size As Integer) As Image
        Return GetOrDraw($"plati:plus:{color.ToArgb()}:{size}", size,
                         Sub(g) DrawPlus(g, color, size))
    End Function

    ''' <summary>Iconița rădăcinii « TOATE PLĂȚILE » (trei bare — o „listă"; oglindește PANUL).</summary>
    Public Shared Function ToateIcon(color As Color, size As Integer) As Image
        Return GetOrDraw($"plati:toate:{color.ToArgb()}:{size}", size,
                         Sub(g) DrawToate(g, color, size))
    End Function

    ' Desenează o singură dată o formă într-un bitmap transparent și o memorează.
    Private Shared Function GetOrDraw(key As String, size As Integer, paint As Action(Of Graphics)) As Image
        SyncLock _sync
            Dim cached As Image = Nothing
            If _cache.TryGetValue(key, cached) Then Return cached

            Dim bmp As New Bitmap(size, size)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.SmoothingMode = SmoothingMode.AntiAlias
                paint(g)
            End Using
            _cache(key) = bmp
            Return bmp
        End SyncLock
    End Function

    Private Shared Sub DrawStatus(g As Graphics, stare As Stare, color As Color, size As Integer)
        Dim m As Single = size * 0.18F                    ' margine
        Dim w As Single = size - 2 * m
        Using brush As New SolidBrush(color)
            Select Case stare
                Case Stare.Sus
                    ' triunghi cu vârful în sus (plată încărcată)
                    g.FillPolygon(brush, New PointF() {
                        New PointF(size / 2.0F, m),
                        New PointF(size - m, size - m),
                        New PointF(m, size - m)})
                Case Stare.Jos
                    ' triunghi cu vârful în jos (plată preluată)
                    g.FillPolygon(brush, New PointF() {
                        New PointF(m, m),
                        New PointF(size - m, m),
                        New PointF(size / 2.0F, size - m)})
                Case Else   ' Neutru -> o bară orizontală (nici sus, nici jos)
                    Dim barH As Single = Math.Max(1.5F, size * 0.16F)
                    Dim y As Single = size / 2.0F - barH / 2.0F
                    g.FillRectangle(brush, m, y, w, barH)
            End Select
        End Using
    End Sub

    Private Shared Sub DrawPlus(g As Graphics, color As Color, size As Integer)
        Dim m As Single = size * 0.2F
        Dim thick As Single = Math.Max(2.0F, size * 0.2F)
        Dim mid As Single = size / 2.0F
        Using brush As New SolidBrush(color)
            g.FillRectangle(brush, m, mid - thick / 2.0F, size - 2 * m, thick)   ' bară orizontală
            g.FillRectangle(brush, mid - thick / 2.0F, m, thick, size - 2 * m)   ' bară verticală
        End Using
    End Sub

    Private Shared Sub DrawToate(g As Graphics, color As Color, size As Integer)
        ' Trei bare orizontale (o „listă") — rădăcina care strânge toate plățile.
        Dim m As Single = size * 0.2F
        Dim w As Single = size - 2 * m
        Dim barH As Single = Math.Max(1.5F, size * 0.12F)
        Using brush As New SolidBrush(color)
            For i As Integer = 0 To 2
                Dim y As Single = m + i * (size - 2 * m - barH) / 2.0F
                g.FillRectangle(brush, m, y, w, barH)
            Next
        End Using
    End Sub

End Class
