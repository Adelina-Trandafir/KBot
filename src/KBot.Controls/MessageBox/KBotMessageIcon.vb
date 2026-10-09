Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The glyph of a message box, drawn (no image assets) in the active theme's colours:
''' round "i" (Info), triangle "!" (Warning), round cross (Error), round "?" (Question).
''' </summary>
Public NotInheritable Class KBotMessageIcon
    Inherits Control
    Implements IThemedControl

    Private _kind As KBotMsgKind = KBotMsgKind.Info
    Private _scheme As ThemeScheme

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.SupportsTransparentBackColor, True)
        SetStyle(ControlStyles.Selectable, False)
        TabStop = False
        BackColor = Color.Transparent
    End Sub

    <DefaultValue(KBotMsgKind.Info)>
    Public Property Kind As KBotMsgKind
        Get
            Return _kind
        End Get
        Set(k_value As KBotMsgKind)
            If _kind = k_value Then Return
            _kind = k_value
            Invalidate()
        End Set
    End Property

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        _scheme = k_scheme
        Invalidate()
    End Sub

    Private Function KindColor() As Color
        Dim k_p As ThemePalette = If(_scheme, ThemeManager.Current).Palette
        Select Case _kind
            Case KBotMsgKind.Warning : Return k_p.WarningColor
            Case KBotMsgKind.Error : Return k_p.ErrorColor
            Case Else : Return k_p.AccentColor
        End Select
    End Function

    Protected Overrides Sub OnResize(k_e As EventArgs)
        MyBase.OnResize(k_e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(k_e As PaintEventArgs)
        Try
            If _kind = KBotMsgKind.None Then Return
            Dim k_g As Graphics = k_e.Graphics
            k_g.SmoothingMode = SmoothingMode.AntiAlias
            Dim k_side As Integer = Math.Max(4, Math.Min(Width, Height) - 2)
            Dim k_rect As New Rectangle((Width - k_side) \ 2, (Height - k_side) \ 2, k_side, k_side)
            Using k_brush As New SolidBrush(KindColor())
                If _kind = KBotMsgKind.Warning Then
                    Dim k_pts() As Point = {
                        New Point(k_rect.Left + k_side \ 2, k_rect.Top),
                        New Point(k_rect.Right, k_rect.Bottom),
                        New Point(k_rect.Left, k_rect.Bottom)}
                    k_g.FillPolygon(k_brush, k_pts)
                Else
                    k_g.FillEllipse(k_brush, k_rect)
                End If
            End Using

            If _kind = KBotMsgKind.Error Then
                Dim k_m As Integer = k_side \ 3
                Using k_pen As New Pen(Color.White, Math.Max(2.0F, k_side / 10.0F))
                    k_pen.StartCap = LineCap.Round
                    k_pen.EndCap = LineCap.Round
                    k_g.DrawLine(k_pen, k_rect.Left + k_m, k_rect.Top + k_m, k_rect.Right - k_m, k_rect.Bottom - k_m)
                    k_g.DrawLine(k_pen, k_rect.Right - k_m, k_rect.Top + k_m, k_rect.Left + k_m, k_rect.Bottom - k_m)
                End Using
                Return
            End If

            Dim k_glyph As String = If(_kind = KBotMsgKind.Warning, "!", If(_kind = KBotMsgKind.Question, "?", "i"))
            Dim k_textRect As Rectangle = k_rect
            If _kind = KBotMsgKind.Warning Then k_textRect.Offset(0, k_side \ 8)
            Using k_font As New Font("Segoe UI", k_side * 0.55F, FontStyle.Bold, GraphicsUnit.Pixel)
                TextRenderer.DrawText(k_g, k_glyph, k_font, k_textRect, Color.White,
                    TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
                    TextFormatFlags.NoPadding Or TextFormatFlags.NoPrefix)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMessageIcon.OnPaint", ex)
        End Try
    End Sub

End Class
