Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>Felul unei notificări (culoarea și pictograma).</summary>
Public Enum NoticeKind
    [Error] = 0
    Warning = 1
    Success = 2
End Enum

''' <summary>
''' Casetă de notificare rotunjită (eroare / avertisment / succes): fundal tentat în
''' culoarea stării, pictogramă desenată (GDI+) și textul mesajului. Culorile vin din
''' schema activă. Ascunsă implicit; <see cref="Show"/> o afișează, <see cref="Clear"/>
''' o ascunde.
''' </summary>
<ToolboxItem(False)>
Public NotInheritable Class KBotNotice
    Inherits Control
    Implements IThemedControl

    Private _message As String = String.Empty
    Private _kind As NoticeKind = NoticeKind.[Error]
    Private _scheme As ThemeScheme

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        BackColor = Color.Transparent
        Visible = False
        Height = 40
    End Sub

    ''' <summary>Afișează notificarea cu mesajul și felul date.</summary>
    Public Overloads Sub Show(message As String, kind As NoticeKind)
        _message = If(message, String.Empty)
        _kind = kind
        Visible = True
        MeasureAndResize()
        Invalidate()
    End Sub

    ''' <summary>Ascunde notificarea și golește mesajul.</summary>
    Public Sub Clear()
        _message = String.Empty
        Visible = False
        Invalidate()
    End Sub

    ''' <summary>Reaplică culorile schemei.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        _scheme = scheme
        MeasureAndResize()
        Invalidate()
    End Sub

    Private Function KindColor() As Color
        Dim p As ThemePalette = If(_scheme, ThemeManager.Current).Palette
        Select Case _kind
            Case NoticeKind.Warning : Return p.WarningColor
            Case NoticeKind.Success : Return p.SuccessColor
            Case Else : Return p.ErrorColor
        End Select
    End Function

    Private Function TextArea() As Rectangle
        Dim padX As Integer = ThemeShapes.ScaleDpi(Me, 10)
        Dim iconW As Integer = ThemeShapes.ScaleDpi(Me, 28)
        Return New Rectangle(padX + iconW, ThemeShapes.ScaleDpi(Me, 6),
                             Math.Max(0, Width - padX * 2 - iconW),
                             Math.Max(0, Height - ThemeShapes.ScaleDpi(Me, 12)))
    End Function

    ' Recalculează înălțimea din mesaj (rândul AutoSize se așază pe ea).
    Private Sub MeasureAndResize()
        If Not Visible OrElse String.IsNullOrEmpty(_message) OrElse Width <= 0 Then Return
        Dim ta As Rectangle = TextArea()
        Dim proposed As New Size(Math.Max(1, ta.Width), Integer.MaxValue)
        Dim sz As Size = TextRenderer.MeasureText(_message, Font, proposed, TextFormatFlags.WordBreak)
        Dim minH As Integer = ThemeShapes.ScaleDpi(Me, 40)
        Dim needed As Integer = sz.Height + ThemeShapes.ScaleDpi(Me, 12)
        Height = Math.Max(minH, needed)
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            If Not Visible OrElse String.IsNullOrEmpty(_message) Then Return

            Dim kc As Color = KindColor()
            Dim surface As Color = If(_scheme, ThemeManager.Current).Palette.SurfaceAltColor
            Dim tint As Color = ThemeShapes.Blend(surface, kc, 0.12)

            Dim radius As Integer = ThemeShapes.ScaleDpi(Me, 6)
            Dim rect As New Rectangle(0, 0, Width - 1, Height - 1)
            Using path As GraphicsPath = ThemeShapes.RoundedRect(rect, radius)
                Using fill As New SolidBrush(tint)
                    g.FillPath(fill, path)
                End Using
                Using pen As New Pen(ThemeShapes.Blend(surface, kc, 0.35), ThemeShapes.ScaleDpi(Me, 1))
                    g.DrawPath(pen, path)
                End Using
            End Using

            DrawIcon(g, kc)

            Dim textColor As Color = If(_scheme, ThemeManager.Current).Palette.TextColor
            TextRenderer.DrawText(g, _message, Font, TextArea(), textColor,
                TextFormatFlags.WordBreak Or TextFormatFlags.VerticalCenter Or TextFormatFlags.Left)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNotice.OnPaint", ex)
        End Try
    End Sub

    ' Pictogramă desenată: cerc (eroare/succes) sau triunghi (avertisment) + glif.
    Private Sub DrawIcon(g As Graphics, kc As Color)
        Dim side As Integer = ThemeShapes.ScaleDpi(Me, 18)
        Dim x As Integer = ThemeShapes.ScaleDpi(Me, 10)
        Dim y As Integer = (Height - side) \ 2
        Dim penW As Single = ThemeShapes.ScaleDpi(Me, 2)

        Using brush As New SolidBrush(kc)
            Using pen As New Pen(Color.White, penW)
                Select Case _kind
                    Case NoticeKind.Warning
                        Dim pts() As Point = {
                            New Point(x + side \ 2, y),
                            New Point(x + side, y + side),
                            New Point(x, y + side)}
                        g.FillPolygon(brush, pts)
                        ' „!”
                        g.DrawLine(pen, x + side \ 2, y + side \ 3, x + side \ 2, y + side - side \ 4)
                        g.FillEllipse(Brushes.White, x + side \ 2 - penW / 2.0F, y + side - side \ 5, penW, penW)
                    Case NoticeKind.Success
                        g.FillEllipse(brush, x, y, side, side)
                        ' bifă
                        g.DrawLines(pen, New Point() {
                            New Point(x + side \ 4, y + side \ 2),
                            New Point(x + side \ 2 - 1, y + side - side \ 3),
                            New Point(x + side - side \ 5, y + side \ 4)})
                    Case Else ' Error
                        g.FillEllipse(brush, x, y, side, side)
                        ' „!”
                        g.DrawLine(pen, x + side \ 2, y + side \ 4, x + side \ 2, y + side - side \ 3)
                        g.FillEllipse(Brushes.White, x + side \ 2 - penW / 2.0F, y + side - side \ 4, penW, penW)
                End Select
            End Using
        End Using
    End Sub

End Class
