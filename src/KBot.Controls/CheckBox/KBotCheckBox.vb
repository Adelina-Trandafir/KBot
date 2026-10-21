Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' K-BOT's own check box (slice ME01-06): a <c>CheckBox</c> that paints itself from the active theme, with no system
''' colours. It keeps the whole <c>CheckBox</c> surface (<c>Checked</c>, <c>CheckState</c>, <c>ThreeState</c>, <c>CheckedChanged</c>,
''' <c>Text</c>, <c>AutoSize</c>), so it replaces a plain one by changing the type only. The box has the input colours, a ticked
''' one is filled with the accent colour and holds a white tick (a square when the state is indeterminate); the caption
''' stands right of the box and its colour follows the scheme until the operator pins <c>ForeColor</c> (C4).
''' </summary>
<ToolboxItem(True)>
Public Class KBotCheckBox
    Inherits CheckBox
    Implements IThemedControl

    ' Logical (96 dpi) measures.
    Private Const BoxSize As Integer = 16
    Private Const Gap As Integer = 6

    Private _forePinned As Boolean
    Private _applying As Boolean
    Private _hover As Boolean
    Private _pressed As Boolean

    Public Sub New()
        MyBase.New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or ControlStyles.SupportsTransparentBackColor, True)
        MyBase.BackColor = Color.Transparent
        If Not KBotDesignTime.IsDesignTime(Me) Then ApplyColors()
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            ApplyColors()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCheckBox.ApplyTheme", ex)
        End Try
    End Sub

    Private Sub ApplyColors()
        _applying = True
        Try
            If Not _forePinned Then MyBase.ForeColor = ThemeManager.Current.Palette.TextColor
        Finally
            _applying = False
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Size
    ' ------------------------------------------------------------------

    Public Overrides Function GetPreferredSize(k_proposed As Size) As Size
        Dim k_box As Integer = ThemeShapes.ScaleDpi(Me, BoxSize)
        Dim k_text As Size = TextRenderer.MeasureText(If(Text, String.Empty), Font, New Size(Integer.MaxValue, Integer.MaxValue),
                                                      TextFormatFlags.NoPadding Or TextFormatFlags.NoPrefix Or TextFormatFlags.SingleLine)
        Return New Size(k_box + ThemeShapes.ScaleDpi(Me, Gap) + k_text.Width + ThemeShapes.ScaleDpi(Me, 4), Math.Max(k_box, k_text.Height) + ThemeShapes.ScaleDpi(Me, 4))
    End Function

    ' ------------------------------------------------------------------
    ' Input: the base class already toggles on click and on Space; these only repaint
    ' ------------------------------------------------------------------

    Protected Overrides Sub OnMouseEnter(k_e As EventArgs)
        MyBase.OnMouseEnter(k_e)
        _hover = True
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseLeave(k_e As EventArgs)
        MyBase.OnMouseLeave(k_e)
        _hover = False
        _pressed = False
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseDown(k_e As MouseEventArgs)
        MyBase.OnMouseDown(k_e)
        _pressed = True
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseUp(k_e As MouseEventArgs)
        MyBase.OnMouseUp(k_e)
        _pressed = False
        Invalidate()
    End Sub

    Protected Overrides Sub OnGotFocus(k_e As EventArgs)
        MyBase.OnGotFocus(k_e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnLostFocus(k_e As EventArgs)
        MyBase.OnLostFocus(k_e)
        Invalidate()
    End Sub

    ' ------------------------------------------------------------------
    ' Paint
    ' ------------------------------------------------------------------

    Protected Overrides Sub OnPaint(k_e As PaintEventArgs)
        Try
            Dim k_g As Graphics = k_e.Graphics
            ButtonRenderer.DrawParentBackground(k_g, ClientRectangle, Me)
            Dim k_pal As ThemePalette = ThemeManager.Current.Palette
            Dim k_box As Integer = ThemeShapes.ScaleDpi(Me, BoxSize)
            Dim k_rect As New Rectangle(0, (Height - k_box) \ 2, k_box - 1, k_box - 1)
            Dim k_on As Boolean = CheckState <> CheckState.Unchecked
            Dim k_back As Color = If(Not Enabled, k_pal.SurfaceAltColor, If(k_on, If(_pressed, ThemeShapes.Darken(k_pal.AccentColor, 0.15), If(_hover, k_pal.AccentHoverColor, k_pal.AccentColor)),
                                     If(_hover, k_pal.ButtonHoverColor, k_pal.InputBackColor)))
            Dim k_border As Color = If(Not Enabled, k_pal.BorderColor, If(k_on OrElse Focused OrElse _hover, k_pal.AccentColor, k_pal.InputBorderColor))
            k_g.SmoothingMode = SmoothingMode.AntiAlias
            Dim k_radius As Integer = Math.Max(1, ThemeShapes.ScaleDpi(Me, 2))
            Using k_path As GraphicsPath = ThemeShapes.RoundedRect(k_rect, k_radius)
                Using k_brush As New SolidBrush(k_back)
                    k_g.FillPath(k_brush, k_path)
                End Using
                Using k_pen As New Pen(k_border, 1.0F)
                    k_g.DrawPath(k_pen, k_path)
                End Using
            End Using
            Dim k_mark As Color = If(Enabled, k_pal.AccentTextColor, k_pal.DisabledTextColor)
            Dim k_w As Single = Math.Max(1.5F, ThemeShapes.ScaleDpi(Me, 2))
            If CheckState = CheckState.Checked Then
                Using k_pen As New Pen(k_mark, k_w) With {.StartCap = LineCap.Round, .EndCap = LineCap.Round, .LineJoin = LineJoin.Round}
                    k_g.DrawLines(k_pen, {New PointF(k_rect.Left + k_rect.Width * 0.24F, k_rect.Top + k_rect.Height * 0.54F),
                                          New PointF(k_rect.Left + k_rect.Width * 0.43F, k_rect.Top + k_rect.Height * 0.73F),
                                          New PointF(k_rect.Left + k_rect.Width * 0.77F, k_rect.Top + k_rect.Height * 0.29F)})
                End Using
            ElseIf CheckState = CheckState.Indeterminate Then
                Dim k_inset As Single = k_rect.Width * 0.28F
                Using k_brush As New SolidBrush(k_mark)
                    k_g.FillRectangle(k_brush, k_rect.Left + k_inset, k_rect.Top + k_inset, k_rect.Width - 2 * k_inset, k_rect.Height - 2 * k_inset)
                End Using
            End If
            k_g.SmoothingMode = SmoothingMode.Default

            Dim k_textLeft As Integer = k_box + ThemeShapes.ScaleDpi(Me, Gap)
            Dim k_area As New Rectangle(k_textLeft, 0, Math.Max(1, Width - k_textLeft), Height)
            Dim k_color As Color = If(Enabled, ForeColor, k_pal.DisabledTextColor)
            TextRenderer.DrawText(k_g, If(Text, String.Empty), Font, k_area, k_color,
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or
                                  TextFormatFlags.NoPrefix Or TextFormatFlags.SingleLine)
            If Focused AndAlso ShowFocusCues AndAlso Not String.IsNullOrEmpty(Text) Then
                Dim k_size As Size = TextRenderer.MeasureText(Text, Font, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.NoPadding Or TextFormatFlags.NoPrefix)
                Dim k_focus As New Rectangle(k_textLeft - 2, (Height - k_size.Height) \ 2 - 1, Math.Min(k_size.Width + 4, k_area.Width), k_size.Height + 2)
                Using k_pen As New Pen(k_pal.FocusRingColor) With {.DashStyle = DashStyle.Dot}
                    k_g.DrawRectangle(k_pen, k_focus)
                End Using
            End If
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotCheckBox.OnPaint", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Designer serialization (C1, C4)
    ' ------------------------------------------------------------------

    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(k_value As Color)
            If Not _applying Then _forePinned = (k_value <> Color.Empty)
            MyBase.ForeColor = k_value
        End Set
    End Property

    Public Function ShouldSerializeForeColor() As Boolean
        Return _forePinned
    End Function

    Public Overrides Sub ResetForeColor()
        _forePinned = False
        ApplyColors()
    End Sub

End Class
