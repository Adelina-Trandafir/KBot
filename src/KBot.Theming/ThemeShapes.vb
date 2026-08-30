Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>
''' Ajutoare geometrice partajate de controalele tematizate (KBotCaptionBar,
''' KBotTextField, KBotBusyBar, KBotNotice, KBotNavList): scalare DPI, cale dreptunghi
''' rotunjit, amestec de culori și umplerea „modern" cu gradient. Pur funcțional, fără stare.
''' PUBLIC (nu Friend): controalele care le folosesc trăiesc acum în KBot.Controls —
''' toate controalele K-BOT stau în acel proiect, motorul de teme rămâne doar motor.
''' </summary>
Public Module ThemeShapes

    ''' <summary>
    ''' Scalează o valoare logică (px @96dpi) la scara controlului.
    '''
    ''' <para>Din felia 0036 formula NU mai stă aici: răspunsul vine din
    ''' <see cref="AppScaling"/>, sursa unică a scării, fiindcă operatorul o poate acum fixa la
    ''' 100% sau pune un factor al lui. Funcția rămâne pe loc — o cheamă vreo 157 de locuri din
    ''' pictura controalelor — dar e un drum, nu o decizie.</para>
    '''
    ''' <para>Comportamentul implicit e neschimbat: pe <see cref="ScalingMode.Automatic"/> tot
    ''' <c>DeviceDpi / 96</c> se calculează, cu aceeași cădere pe 96 când handle-ul încă nu
    ''' există.</para>
    ''' </summary>
    Public Function ScaleDpi(ctrl As Control, logical As Integer) As Integer
        Return AppScaling.Scale(ctrl, logical)
    End Function

    ''' <summary>
    ''' Cale dreptunghi cu colțuri rotunjite. <paramref name="radius"/> e deja în px
    ''' scalați. radius &lt;= 0 => dreptunghi simplu. Diametrul e limitat la latura mică.
    ''' </summary>
    Public Function RoundedRect(bounds As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim d As Integer = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height))
        If d <= 0 Then
            path.AddRectangle(bounds)
            Return path
        End If
        Dim arc As New Rectangle(bounds.Location, New Size(d, d))
        path.AddArc(arc, 180, 90)                 ' stânga-sus
        arc.X = bounds.Right - d
        path.AddArc(arc, 270, 90)                 ' dreapta-sus
        arc.Y = bounds.Bottom - d
        path.AddArc(arc, 0, 90)                   ' dreapta-jos
        arc.X = bounds.Left
        path.AddArc(arc, 90, 90)                  ' stânga-jos
        path.CloseFigure()
        Return path
    End Function

    ''' <summary>Amestec liniar între două culori: t=0 => a, t=1 => b (t clamp-at 0..1).</summary>
    Public Function Blend(a As Color, b As Color, t As Double) As Color
        Dim tt As Double = Math.Max(0.0, Math.Min(1.0, t))
        Dim r As Integer = CInt(CDbl(a.R) + (CDbl(b.R) - a.R) * tt)
        Dim g As Integer = CInt(CDbl(a.G) + (CDbl(b.G) - a.G) * tt)
        Dim bl As Integer = CInt(CDbl(a.B) + (CDbl(b.B) - a.B) * tt)
        Return Color.FromArgb(r, g, bl)
    End Function

    ''' <summary>Aceeași culoare, trasă spre alb cu o fracție (0..1).</summary>
    Public Function Lighten(c As Color, amount As Double) As Color
        Return Blend(c, Color.White, amount)
    End Function

    ''' <summary>Aceeași culoare, trasă spre negru cu o fracție (0..1).</summary>
    Public Function Darken(c As Color, amount As Double) As Color
        Return Blend(c, Color.Black, amount)
    End Function

    ''' <summary>
    ''' Umple o cale cu aspectul „modern": un gradient vertical fin în jurul culorii de bază, mai
    ''' deschis sus și mai închis jos. Culoarea de bază vine din PALETĂ (fundal de selecție, de
    ''' hover, …) — de aici, tot ce se pictează rămâne al schemei active: nu se introduce nicio
    ''' culoare nouă, doar două nuanțe derivate din una existentă. Merge la fel pe schemele deschise
    ''' și pe cele întunecate, fiindcă amândouă capetele se calculează din bază, nu din constante.
    '''
    ''' <paramref name="strength"/> e 0..100 (se limitează). **0 => umplere PLATĂ**, fără gradient
    ''' și fără alocare — exact ce se picta înainte de 0025-08, deci o schemă sau un control care
    ''' nu cere gradient arată bit cu bit ca înainte.
    '''
    ''' Cele două capete NU sunt simetrice: partea de jos se închide cu doar 3/4 din cât se deschide
    ''' partea de sus. Un gradient simetric arată a buton Windows XP; unul deschis-sus/abia-închis-jos
    ''' citește ca lumină căzând de sus, care e tot ce înseamnă „modern" aici.
    ''' </summary>
    Public Sub FillModern(g As Graphics, path As GraphicsPath, bounds As Rectangle,
                          baseColor As Color, strength As Integer)
        Dim s As Integer = Math.Max(0, Math.Min(100, strength))
        If s = 0 OrElse bounds.Width <= 0 OrElse bounds.Height <= 1 Then
            Using b As New SolidBrush(baseColor)
                g.FillPath(b, path)
            End Using
            Return
        End If
        ' 100 = ±32% față de bază: destul cât să se vadă pe un rând de 36 px, nu cât să pară lucios.
        Dim amount As Double = s / 100.0 * 0.32
        ' LinearGradientBrush pictează primul/ultimul RÂND cu culoarea capătului opus (artefact
        ' vechi de rotunjire). Se umflă zona pe verticală cu un pixel de fiecare parte, iar rândurile
        ' stricate cad în afara căii.
        Dim area As New Rectangle(bounds.X, bounds.Y - 1, Math.Max(1, bounds.Width), bounds.Height + 2)
        Using b As New LinearGradientBrush(area, Lighten(baseColor, amount),
                                           Darken(baseColor, amount * 0.75), LinearGradientMode.Vertical)
            g.FillPath(b, path)
        End Using
    End Sub


    ' =========================================================================
    ' CARDURI (felia 0049) — umbra, umplere, colturi
    ' =========================================================================

    ''' <summary>
    ''' Umbra care cade in jurul unui card: contururi rotunjite concentrice, desenate spre
    ''' EXTERIORUL lui <paramref name="bounds"/>, cu alfa care scade cu distanta.
    '''
    ''' <para><paramref name="size"/> &lt;= 0 se intoarce imediat, fara sa atinga
    ''' <paramref name="g"/> — asta e calea PLATA, si de ea depinde ca schemele neutre sa se
    ''' picteze bit cu bit ca inainte de felie.</para>
    '''
    ''' <para>Se cheama pe PARINTELE cardului, inainte ca acesta sa se picteze: un control
    ''' WinForms n-are alfa fata de parintele lui, deci umbra nu poate veni de la cardul insusi.</para>
    ''' </summary>
    ''' <param name="opacity">0..100 — cat de apasat e primul contur, cel lipit de card.</param>
    Public Sub DrawCardShadow(g As Graphics, bounds As Rectangle, radius As Integer,
                              color As Color, size As Integer, opacity As Integer)
        If g Is Nothing Then Return
        If size <= 0 OrElse opacity <= 0 Then Return
        If bounds.Width <= 0 OrElse bounds.Height <= 0 Then Return

        Dim op As Integer = Math.Max(0, Math.Min(100, opacity))
        Dim oldMode As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            ' Each outline is inflated by i px all round. Alpha falls QUADRATICALLY, not linearly:
            ' a linear fall leaves a visible edge where the shadow stops.
            '
            ' `opacity` is the alpha of the ring TOUCHING the card, as a percentage — nothing more
            ' elaborate. The rings are 1px outlines at different offsets, so they barely overlap
            ' and each pixel takes essentially one ring's alpha; that makes the number mean on
            ' screen what it says on the tin. An earlier version also divided by `size` to keep the
            ' total ink constant, which drove a 6% shadow down to alpha 3 out of 255 — present in
            ' the bitmap, invisible to a human.
            For i As Integer = size To 1 Step -1
                Dim fade As Double = CDbl(size - i + 1) / CDbl(size)     ' 1 next to the card, ~0 at the far edge
                Dim a As Integer = CInt(Math.Round(255.0 * (op / 100.0) * fade * fade))
                If a <= 0 Then Continue For
                If a > 255 Then a = 255
                Dim r As New Rectangle(bounds.X - i, bounds.Y - i,
                                       bounds.Width + i * 2, bounds.Height + i * 2)
                Using path As GraphicsPath = RoundedRect(r, radius + i)
                    Using pen As New Pen(Color.FromArgb(a, color), 1.0F)
                        g.DrawPath(pen, path)
                    End Using
                End Using
            Next
        Finally
            g.SmoothingMode = oldMode
        End Try
    End Sub

    ''' <summary>
    ''' Umple cardul si ii trage conturul. <paramref name="radius"/> &lt;= 0 ia calea plata —
    ''' <c>FillRectangle</c> curat, fara antialias — deci un card cu raza 0 se picteaza exact
    ''' ca umplerea dreptunghiulara de dinainte de felie.
    ''' <paramref name="border"/> transparent sau <c>Color.Empty</c> = fara contur.
    ''' </summary>
    Public Sub FillCard(g As Graphics, bounds As Rectangle, radius As Integer,
                        fill As Color, border As Color)
        If g Is Nothing Then Return
        If bounds.Width <= 0 OrElse bounds.Height <= 0 Then Return

        Dim hasBorder As Boolean = border <> Color.Empty AndAlso border.A > 0

        If radius <= 0 Then
            Using b As New SolidBrush(fill)
                g.FillRectangle(b, bounds)
            End Using
            If hasBorder Then
                Using pen As New Pen(border, 1.0F)
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1)
                End Using
            End If
            Return
        End If

        Dim oldMode As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            Using path As GraphicsPath = RoundedRect(bounds, radius)
                Using b As New SolidBrush(fill)
                    g.FillPath(b, path)
                End Using
            End Using
            If hasBorder Then
                ' Conturul se traseaza pe un dreptunghi micsorat cu 1: altfel jumatate din
                ' latimea creionului cade in afara controlului si latura dreapta/de jos dispare.
                Dim inner As New Rectangle(bounds.X, bounds.Y,
                                           Math.Max(1, bounds.Width - 1), Math.Max(1, bounds.Height - 1))
                Using path As GraphicsPath = RoundedRect(inner, radius)
                    Using pen As New Pen(border, 1.0F)
                        g.DrawPath(pen, path)
                    End Using
                End Using
            End If
        Finally
            g.SmoothingMode = oldMode
        End Try
    End Sub

    ''' <summary>
    ''' Umple cele patru pene de colt — ce e in dreptunghiul controlului dar in afara caii
    ''' rotunjite — cu culoarea pinzei.
    '''
    ''' <para>Asta e tot ce face un card sa PARA rotunjit: un control WinForms n-are canal alfa
    ''' fata de parintele lui, deci colturile nu pot fi «gaurite», ci doar vopsite cu ce ar fi
    ''' fost dedesubt. De aici si cele doua limite scrise in worklog: cardurile nu au voie sa se
    ''' suprapuna, iar pinza trebuie sa fie o culoare plina.</para>
    '''
    ''' <para><paramref name="radius"/> &lt;= 0 nu are ce umple si se intoarce imediat.</para>
    ''' </summary>
    Public Sub PaintCardCorners(g As Graphics, bounds As Rectangle, radius As Integer,
                                canvas As Color)
        If g Is Nothing Then Return
        If radius <= 0 Then Return
        If bounds.Width <= 0 OrElse bounds.Height <= 0 Then Return

        Dim oldMode As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            Using path As GraphicsPath = RoundedRect(bounds, radius)
                Using rgn As New Region(bounds)
                    rgn.Exclude(path)
                    Using b As New SolidBrush(canvas)
                        g.FillRegion(b, rgn)
                    End Using
                End Using
            End Using
        Finally
            g.SmoothingMode = oldMode
        End Try
    End Sub

End Module
