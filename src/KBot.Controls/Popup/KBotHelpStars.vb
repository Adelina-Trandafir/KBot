Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The rating line under the help's search results (slice 0000-21): a question on the left and
''' five stars. A click gives 1..5; a click on the star already given takes the rating back (0).
''' The mouse lights the stars it would give. Drawn by us, colours from the palette.
''' </summary>
<ToolboxItem(False)>
<DesignerCategory("Code")>
Public NotInheritable Class KBotHelpStars
    Inherits Control
    Implements IThemedControl

    Private Const StarCount As Integer = 5
    Private Const StarLogical As Integer = 18
    Private Const GapLogical As Integer = 4
    Private Const PadXLogical As Integer = 10

    Private _value As Integer
    Private _hover As Integer
    Private _caption As String = "A fost util răspunsul?"

    Private _back As Color = SystemColors.Window
    Private _fore As Color = SystemColors.GrayText
    Private _on As Color = SystemColors.Highlight
    Private _off As Color = SystemColors.ControlDark

    ''' <summary>The operator gave or took back a rating (<see cref="Value"/>).</summary>
    Public Event RatingChanged As EventHandler

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        SetStyle(ControlStyles.Selectable, False)
        TabStop = False
    End Sub

    Protected Overrides ReadOnly Property DefaultSize As Size
        Get
            Return New Size(300, 30)
        End Get
    End Property

    ''' <summary>0 = not rated, else 1..5. Set by the host without raising <see cref="RatingChanged"/>.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Value As Integer
        Get
            Return _value
        End Get
        Set(v As Integer)
            v = Math.Max(0, Math.Min(StarCount, v))   ' C3: numbers are clamped
            If v = _value Then Return
            _value = v
            Invalidate()
        End Set
    End Property

    Private Function Px(logical As Integer) As Integer
        If KBotDesignTime.IsDesignTime(Me) Then Return logical
        Return ThemeShapes.ScaleDpi(Me, logical)
    End Function

    ' Star i (1..5), right-aligned in the control.
    Private Function StarRect(i As Integer) As Rectangle
        Dim s As Integer = Px(StarLogical)
        Dim gap As Integer = Px(GapLogical)
        Dim total As Integer = StarCount * s + (StarCount - 1) * gap
        Dim x0 As Integer = ClientSize.Width - Px(PadXLogical) - total
        Return New Rectangle(x0 + (i - 1) * (s + gap), (ClientSize.Height - s) \ 2, s, s)
    End Function

    Private Function StarAt(p As Point) As Integer
        For i As Integer = 1 To StarCount
            Dim r As Rectangle = StarRect(i)
            r.Inflate(Px(GapLogical) \ 2, Px(4))
            If r.Contains(p) Then Return i
        Next
        Return 0
    End Function

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        Using b As New SolidBrush(_back)
            e.Graphics.FillRectangle(b, ClientRectangle)
        End Using
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            Dim first As Rectangle = StarRect(1)
            TextRenderer.DrawText(g, _caption, Font, New Rectangle(Px(PadXLogical), 0, Math.Max(0, first.Left - Px(PadXLogical) - Px(GapLogical)), ClientSize.Height),
                                  _fore, TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix)
            g.SmoothingMode = SmoothingMode.AntiAlias
            Dim lit As Integer = If(_hover > 0, _hover, _value)
            For i As Integer = 1 To StarCount
                Using path As GraphicsPath = StarPath(StarRect(i))
                    If i <= lit Then
                        Using b As New SolidBrush(_on)
                            g.FillPath(b, path)
                        End Using
                    End If
                    Using pen As New Pen(If(i <= lit, _on, _off), Math.Max(1.0F, Px(1)))
                        g.DrawPath(pen, path)
                    End Using
                End Using
            Next
        Catch ex As Exception
            ' UI boundary (OnPaint).
            GlobalErrorLog.Write("KBotHelpStars.OnPaint", ex)
        End Try
    End Sub

    ' A five-pointed star inside r.
    Private Shared Function StarPath(r As Rectangle) As GraphicsPath
        Dim cx As Single = r.X + r.Width / 2.0F
        Dim cy As Single = r.Y + r.Height / 2.0F
        Dim outer As Single = r.Width / 2.0F
        Dim inner As Single = outer * 0.45F
        Dim pts(9) As PointF
        For k As Integer = 0 To 9
            Dim radius As Single = If(k Mod 2 = 0, outer, inner)
            Dim angle As Double = -Math.PI / 2 + k * Math.PI / 5
            pts(k) = New PointF(cx + CSng(radius * Math.Cos(angle)), cy + CSng(radius * Math.Sin(angle)))
        Next
        Dim path As New GraphicsPath()
        path.AddPolygon(pts)
        Return path
    End Function

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            Dim i As Integer = StarAt(e.Location)
            If i = _hover Then Return
            _hover = i
            Cursor = If(i > 0, Cursors.Hand, Cursors.Default)
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpStars.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hover <> 0 Then
            _hover = 0
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Dim i As Integer = StarAt(e.Location)
            If i = 0 Then Return
            _value = If(i = _value, 0, i)
            Invalidate()
            RaiseEvent RatingChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            ' UI boundary (mouse handler).
            GlobalErrorLog.Write("KBotHelpStars.OnMouseClick", ex)
        End Try
    End Sub

    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        Invalidate()
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            _back = p.SurfaceAltColor
            _fore = p.TextDimColor
            _on = p.WarningColor
            _off = p.BorderColor
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpStars.ApplyTheme", ex)
        End Try
    End Sub

End Class
