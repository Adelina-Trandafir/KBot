Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' K-BOT's own push button: a rounded face drawn from the active theme (no system colours, no
''' <c>FlatStyle</c> tricks). Works as a form's <c>AcceptButton</c> / <c>CancelButton</c> and carries a
''' <see cref="DialogResult"/> like a <c>Button</c>, so a dialog built from it needs no extra code.
'''
''' <para><see cref="Primary"/> = the accent-filled face (the action the dialog is for). The face
''' follows the theme's corner radius; every pixel metric is logical and scaled with the dpi.</para>
''' </summary>
<ToolboxItem(True)>
<DefaultEvent("Click")>
Public Class KBotButton
    Inherits Control
    Implements IButtonControl
    Implements IThemedControl

    Private _scheme As ThemeScheme
    Private _dialogResult As DialogResult = DialogResult.None
    Private _isDefault As Boolean
    Private _primary As Boolean
    Private _hover As Boolean
    Private _pressed As Boolean
    Private _spaceDown As Boolean

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable Or ControlStyles.StandardClick Or
                 ControlStyles.StandardDoubleClick, True)
        TabStop = True
        Size = New Size(100, 32)
        AccessibleRole = AccessibleRole.PushButton
    End Sub

    ' ---------------- properties ----------------

    <Category("K-BOT")>
    <Description("Fața plină, în culoarea de accent: acțiunea pentru care există fereastra.")>
    <DefaultValue(False)>
    Public Property Primary As Boolean
        Get
            Return _primary
        End Get
        Set(k_value As Boolean)
            If _primary = k_value Then Return
            _primary = k_value
            Invalidate()
        End Set
    End Property

    <Category("Behavior")>
    <Description("Rezultatul pe care îl primește fereastra când se apasă butonul.")>
    <DefaultValue(DialogResult.None)>
    Public Property DialogResult As DialogResult Implements IButtonControl.DialogResult
        Get
            Return _dialogResult
        End Get
        Set(k_value As DialogResult)
            _dialogResult = k_value
        End Set
    End Property

    Public Sub NotifyDefault(k_value As Boolean) Implements IButtonControl.NotifyDefault
        If _isDefault = k_value Then Return
        _isDefault = k_value
        Invalidate()
    End Sub

    Public Sub PerformClick() Implements IButtonControl.PerformClick
        If Enabled AndAlso Visible Then OnClick(EventArgs.Empty)
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        _scheme = k_scheme
        Invalidate()
    End Sub

    ' ---------------- size ----------------

    Public Overrides Function GetPreferredSize(k_proposed As Size) As Size
        Dim k_text As Size = TextRenderer.MeasureText(If(Text, String.Empty), Font,
                                                      New Size(Integer.MaxValue, Integer.MaxValue),
                                                      TextFormatFlags.NoPadding Or TextFormatFlags.NoPrefix)
        Return New Size(k_text.Width + ThemeShapes.ScaleDpi(Me, 28), Math.Max(k_text.Height + ThemeShapes.ScaleDpi(Me, 14), ThemeShapes.ScaleDpi(Me, 30)))
    End Function

    ' ---------------- input ----------------

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
        If k_e.Button <> MouseButtons.Left Then Return
        Focus()
        _pressed = True
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseUp(k_e As MouseEventArgs)
        _pressed = False
        Invalidate()
        MyBase.OnMouseUp(k_e)
    End Sub

    Protected Overrides Sub OnKeyDown(k_e As KeyEventArgs)
        MyBase.OnKeyDown(k_e)
        If k_e.KeyCode = Keys.Space AndAlso Not _spaceDown Then
            _spaceDown = True
            _pressed = True
            Invalidate()
            k_e.Handled = True
        End If
    End Sub

    Protected Overrides Sub OnKeyUp(k_e As KeyEventArgs)
        MyBase.OnKeyUp(k_e)
        If k_e.KeyCode = Keys.Space AndAlso _spaceDown Then
            _spaceDown = False
            _pressed = False
            Invalidate()
            PerformClick()
            k_e.Handled = True
        End If
    End Sub

    Protected Overrides Function IsInputKey(k_keyData As Keys) As Boolean
        Return k_keyData = Keys.Space OrElse MyBase.IsInputKey(k_keyData)
    End Function

    Protected Overrides Sub OnClick(k_e As EventArgs)
        ' Like Button: a result on the button closes the dialog it sits in.
        If _dialogResult <> DialogResult.None Then
            Dim k_form As Form = FindForm()
            If k_form IsNot Nothing Then k_form.DialogResult = _dialogResult
        End If
        MyBase.OnClick(k_e)
    End Sub

    Protected Overrides Sub OnGotFocus(k_e As EventArgs)
        MyBase.OnGotFocus(k_e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnLostFocus(k_e As EventArgs)
        MyBase.OnLostFocus(k_e)
        _pressed = False
        _spaceDown = False
        Invalidate()
    End Sub

    Protected Overrides Sub OnEnabledChanged(k_e As EventArgs)
        MyBase.OnEnabledChanged(k_e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnTextChanged(k_e As EventArgs)
        MyBase.OnTextChanged(k_e)
        Invalidate()
    End Sub

    ' ---------------- painting ----------------

    Protected Overrides Sub OnPaint(k_e As PaintEventArgs)
        Try
            Dim k_scheme As ThemeScheme = If(_scheme, ThemeManager.Current)
            Dim k_p As ThemePalette = k_scheme.Palette
            Dim k_g As Graphics = k_e.Graphics
            k_g.SmoothingMode = SmoothingMode.AntiAlias

            ' The surface behind the rounded corners.
            Dim k_behind As Color = If(Parent IsNot Nothing AndAlso Parent.BackColor.A = 255, Parent.BackColor, k_p.SurfaceColor)
            k_g.Clear(k_behind)

            Dim k_back As Color
            Dim k_border As Color
            Dim k_text As Color
            If Not Enabled Then
                k_back = k_p.SurfaceAltColor
                k_border = k_p.BorderColor
                k_text = k_p.DisabledTextColor
            ElseIf _primary Then
                k_back = If(_pressed, ThemeShapes.Darken(k_p.AccentColor, 0.15), If(_hover, k_p.AccentHoverColor, k_p.AccentColor))
                k_border = k_back
                k_text = k_p.AccentTextColor
            Else
                k_back = If(_pressed, k_p.ButtonPressedColor, If(_hover, k_p.ButtonHoverColor, k_p.ButtonBackColor))
                k_border = If(_isDefault OrElse Focused, k_p.AccentColor, k_p.ButtonBorderColor)
                k_text = k_p.ButtonTextColor
            End If

            Dim k_rect As New Rectangle(0, 0, Width - 1, Height - 1)
            Dim k_radius As Integer = ThemeShapes.ScaleDpi(Me, k_scheme.Style.CornerRadius)
            Using k_path As GraphicsPath = ThemeShapes.RoundedRect(k_rect, k_radius)
                Using k_brush As New SolidBrush(k_back)
                    k_g.FillPath(k_brush, k_path)
                End Using
                Using k_pen As New Pen(k_border, If(_isDefault AndAlso Not _primary, 2.0F, 1.0F))
                    k_g.DrawPath(k_pen, k_path)
                End Using
            End Using

            If Focused AndAlso ShowFocusCues Then
                Dim k_inner As Rectangle = Rectangle.Inflate(k_rect, -ThemeShapes.ScaleDpi(Me, 3), -ThemeShapes.ScaleDpi(Me, 3))
                If k_inner.Width > 2 AndAlso k_inner.Height > 2 Then
                    Using k_pen As New Pen(k_p.FocusRingColor) With {.DashStyle = DashStyle.Dot}
                        k_g.DrawRectangle(k_pen, k_inner)
                    End Using
                End If
            End If

            Dim k_offset As Integer = If(_pressed, 1, 0)
            Dim k_area As New Rectangle(ThemeShapes.ScaleDpi(Me, 6) + k_offset, k_offset,
                                        Math.Max(1, Width - ThemeShapes.ScaleDpi(Me, 12)), Height)
            TextRenderer.DrawText(k_g, If(Text, String.Empty), Font, k_area, k_text,
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
                                  TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix Or TextFormatFlags.SingleLine)
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotButton.OnPaint", ex)
        End Try
    End Sub

End Class
