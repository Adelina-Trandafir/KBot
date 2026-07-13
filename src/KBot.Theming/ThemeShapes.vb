Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>
''' Ajutoare geometrice partajate de controalele tematizate (KBotCaptionBar,
''' KBotTextField, KBotBusyBar, KBotNotice): scalare DPI, cale dreptunghi rotunjit,
''' amestec de culori. Pur funcțional, fără stare.
''' </summary>
Friend Module ThemeShapes

    ''' <summary>
    ''' Scalează o valoare logică (px @96dpi) la DPI-ul controlului. Fallback 96 dacă
    ''' handle-ul încă nu există (DeviceDpi poate arunca înainte de creare).
    ''' </summary>
    Friend Function ScaleDpi(ctrl As Control, logical As Integer) As Integer
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
    Friend Function RoundedRect(bounds As Rectangle, radius As Integer) As GraphicsPath
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
    Friend Function Blend(a As Color, b As Color, t As Double) As Color
        Dim tt As Double = Math.Max(0.0, Math.Min(1.0, t))
        Dim r As Integer = CInt(CDbl(a.R) + (CDbl(b.R) - a.R) * tt)
        Dim g As Integer = CInt(CDbl(a.G) + (CDbl(b.G) - a.G) * tt)
        Dim bl As Integer = CInt(CDbl(a.B) + (CDbl(b.B) - a.B) * tt)
        Return Color.FromArgb(r, g, bl)
    End Function

End Module
