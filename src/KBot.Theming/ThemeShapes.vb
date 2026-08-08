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
    ''' Scalează o valoare logică (px @96dpi) la DPI-ul controlului. Fallback 96 dacă
    ''' handle-ul încă nu există (DeviceDpi poate arunca înainte de creare).
    ''' </summary>
    Public Function ScaleDpi(ctrl As Control, logical As Integer) As Integer
        Dim dpi As Integer = 96
        Try
            If ctrl IsNot Nothing Then dpi = ctrl.DeviceDpi
        Catch
            dpi = 96
        End Try
        Return CInt(Math.Round(logical * dpi / 96.0))
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

End Module
