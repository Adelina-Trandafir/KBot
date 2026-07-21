Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports KBot.Domain

''' <summary>
''' Iconițele arborelui de rezervări (felia 0014), desenate din forme GDI, nu din
''' resurse binare: tipul operației (Inițială «=», Mărire «▲», Micșorare «▼») și
''' acțiunea «+». Se desenează cu o culoare dată (din paleta temei), deci se re-tintează
''' la schimbarea temei. Cache pe (fel, culoare, dimensiune) — bitmap-urile sunt
''' imutabile și partajate, exact ca <see cref="FxIcons"/>.
''' </summary>
Public NotInheritable Class RezervariIcons

    Private Sub New()
    End Sub

    Private Shared ReadOnly _cache As New Dictionary(Of String, Image)(StringComparer.Ordinal)
    Private Shared ReadOnly _sync As New Object()

    ''' <summary>Iconița stânga pentru tipul operației. Necunoscut -> Nothing (fără icon).</summary>
    Public Shared Function TipIcon(tip As RezervareTip, color As Color, size As Integer) As Image
        If tip = RezervareTip.Necunoscut Then Return Nothing
        Return GetOrDraw($"tip:{tip}:{color.ToArgb()}:{size}", size,
                         Sub(g) DrawTip(g, tip, color, size))
    End Function

    ''' <summary>Iconița dreapta «+» (adaugă DDF). Verde pentru operația inițială,
    ''' altfel culoarea accent — oglindește Plus_Green / Plus din Access.</summary>
    Public Shared Function PlusIcon(color As Color, size As Integer) As Image
        Return GetOrDraw($"plus:{color.ToArgb()}:{size}", size,
                         Sub(g) DrawPlus(g, color, size))
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

    Private Shared Sub DrawTip(g As Graphics, tip As RezervareTip, color As Color, size As Integer)
        Dim m As Single = size * 0.18F                    ' margine
        Dim w As Single = size - 2 * m
        Using brush As New SolidBrush(color)
            Select Case tip
                Case RezervareTip.Marire
                    ' triunghi cu vârful în sus (creștere)
                    g.FillPolygon(brush, New PointF() {
                        New PointF(size / 2.0F, m),
                        New PointF(size - m, size - m),
                        New PointF(m, size - m)})
                Case RezervareTip.Micsorare
                    ' triunghi cu vârful în jos (scădere)
                    g.FillPolygon(brush, New PointF() {
                        New PointF(m, m),
                        New PointF(size - m, m),
                        New PointF(size / 2.0F, size - m)})
                Case Else   ' Initiala -> semnul «=» (două bare orizontale)
                    Dim barH As Single = Math.Max(1.5F, size * 0.16F)
                    Dim gap As Single = size * 0.16F
                    Dim yTop As Single = size / 2.0F - gap / 2.0F - barH
                    Dim yBot As Single = size / 2.0F + gap / 2.0F
                    g.FillRectangle(brush, m, yTop, w, barH)
                    g.FillRectangle(brush, m, yBot, w, barH)
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

End Class
