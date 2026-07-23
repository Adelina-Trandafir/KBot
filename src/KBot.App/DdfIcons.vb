Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D

''' <summary>
''' Iconițele arborelui de revizii DDF (felia 0020), desenate din forme GDI, nu din
''' resurse binare — același tipar ca <see cref="RezervariIcons"/> / <see cref="PlatiIcons"/>
''' (o clasă de iconițe per vedere). Se desenează cu o culoare dată (din paleta temei),
''' deci se re-tintează la schimbarea temei. Cache pe (stare, culoare, dimensiune) —
''' bitmap-urile sunt imutabile și partajate.
'''
''' NB: <see cref="FxIcons"/> NU se folosește aici (deși planul feliei îl numea): acela
''' încarcă iconițe EMBEDDED pentru starea unui angajament, nu forme desenate și tintate
''' din paletă.
'''
''' Oglindește REV_SUS / REV_JOS / REV_NOT din Access (frmFX_MAIN_DDF.Show_Revizii):
''' Incarcat -&gt; sus, altfel Preluat -&gt; jos, altfel neutru.
''' </summary>
Public NotInheritable Class DdfIcons

    Private Sub New()
    End Sub

    ''' <summary>Starea vizuală a reviziei, derivată din Incarcat/Preluat.</summary>
    Public Enum Stare
        ''' <summary>Nici încărcată, nici preluată (REV_NOT).</summary>
        Neutru = 0
        ''' <summary>Revizie încărcată (REV_SUS).</summary>
        Sus = 1
        ''' <summary>Revizie preluată, dar nu încărcată (REV_JOS).</summary>
        Jos = 2
    End Enum

    Private Shared ReadOnly _cache As New Dictionary(Of String, Image)(StringComparer.Ordinal)
    Private Shared ReadOnly _sync As New Object()

    ''' <summary>Iconița stării reviziei. Culoarea vine din paletă (succes/accent/estompat).</summary>
    Public Shared Function StatusIcon(stare As Stare, color As Color, size As Integer) As Image
        Return GetOrDraw($"rev:{stare}:{color.ToArgb()}:{size}", size,
                         Sub(g) DrawStatus(g, stare, color, size))
    End Function

    ''' <summary>Iconița rădăcinii de lună (un „dosar" simplificat — grupul de revizii).</summary>
    Public Shared Function LunaIcon(color As Color, size As Integer) As Image
        Return GetOrDraw($"luna:{color.ToArgb()}:{size}", size,
                         Sub(g) DrawLuna(g, color, size))
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
                    ' triunghi cu vârful în sus (revizie încărcată) — REV_SUS
                    g.FillPolygon(brush, New PointF() {
                        New PointF(size / 2.0F, m),
                        New PointF(size - m, size - m),
                        New PointF(m, size - m)})
                Case Stare.Jos
                    ' triunghi cu vârful în jos (revizie preluată) — REV_JOS
                    g.FillPolygon(brush, New PointF() {
                        New PointF(m, m),
                        New PointF(size - m, m),
                        New PointF(size / 2.0F, size - m)})
                Case Else   ' Neutru -> o bară orizontală (nici sus, nici jos) — REV_NOT
                    Dim barH As Single = Math.Max(1.5F, size * 0.16F)
                    Dim y As Single = size / 2.0F - barH / 2.0F
                    g.FillRectangle(brush, m, y, w, barH)
            End Select
        End Using
    End Sub

    Private Shared Sub DrawLuna(g As Graphics, color As Color, size As Integer)
        ' Un „dosar" simplificat: o clapetă îngustă peste un corp dreptunghiular.
        Dim m As Single = size * 0.15F
        Dim w As Single = size - 2 * m
        Dim tabH As Single = Math.Max(1.5F, size * 0.14F)
        Dim bodyY As Single = m + tabH
        Using brush As New SolidBrush(color)
            g.FillRectangle(brush, m, m, w * 0.45F, tabH)                 ' clapeta
            g.FillRectangle(brush, m, bodyY, w, size - m - bodyY)         ' corpul
        End Using
    End Sub

End Class
