Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Câmp de text „modern”: un TextBox fără chenar așezat într-un cadru care pictează
''' un contur rotunjit de 1px (accent la focus), cu padding intern. Opțional, pentru
''' parole, desenează un ochi de dezvăluire (GDI+) în dreapta.
'''
''' Cadrul NU e selectabil (Tab-ul aterizează direct pe TextBox-ul intern). Fiindcă
''' rama nu primește niciodată focus, un KeyDown pe ea n-ar declanșa; de aceea
''' re-ridicăm evenimentul KeyDown al TextBox-ului intern ca <see cref="FieldKeyDown"/>.
''' </summary>
<ToolboxItem(False)>
Public NotInheritable Class KBotTextField
    Inherits Control
    Implements IThemedControl

    Private ReadOnly _inner As New TextBox()

    ' ── Culori derivate din paletă ────────────────────────────────────────────
    Private _fillColor As Color = Color.White
    Private _borderColor As Color = Color.Gray
    Private _focusColor As Color = Color.DodgerBlue
    Private _glyphColor As Color = Color.Gray

    ' ── Stare ─────────────────────────────────────────────────────────────────
    Private _passwordMode As Boolean = False
    Private _revealed As Boolean = False
    Private _focused As Boolean = False
    Private _hoverEye As Boolean = False

    ''' <summary>KeyDown-ul TextBox-ului intern, re-ridicat pe cadru (aceeași semnătură).</summary>
    Public Event FieldKeyDown As KeyEventHandler

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        SetStyle(ControlStyles.Selectable, False)
        TabStop = False
        BackColor = Color.Transparent

        _inner.BorderStyle = BorderStyle.None
        _inner.Multiline = False
        AddHandler _inner.Enter, AddressOf OnInnerEnter
        AddHandler _inner.Leave, AddressOf OnInnerLeave
        AddHandler _inner.KeyDown, AddressOf OnInnerKeyDown
        Controls.Add(_inner)

        Height = 36
    End Sub

    ' ── Suprafață publică (subset; restul prin InnerTextBox) ──────────────────
    ''' <summary>Textul câmpului (delegat la TextBox-ul intern).</summary>
    Public Overrides Property Text As String
        Get
            Return _inner.Text
        End Get
        Set(value As String)
            _inner.Text = value
        End Set
    End Property

    ''' <summary>Placeholder-ul afișat când câmpul e gol.</summary>
    Public Property PlaceholderText As String
        Get
            Return _inner.PlaceholderText
        End Get
        Set(value As String)
            _inner.PlaceholderText = value
        End Set
    End Property

    ''' <summary>Câmp de parolă: ascunde textul și arată ochiul de dezvăluire.</summary>
    Public Property UseSystemPasswordChar As Boolean
        Get
            Return _passwordMode
        End Get
        Set(value As Boolean)
            _passwordMode = value
            _revealed = False
            _inner.UseSystemPasswordChar = value
            LayoutBox()
            Invalidate()
        End Set
    End Property

    ''' <summary>Lungimea maximă (delegat la TextBox-ul intern).</summary>
    Public Property MaxLength As Integer
        Get
            Return _inner.MaxLength
        End Get
        Set(value As Integer)
            _inner.MaxLength = value
        End Set
    End Property

    ''' <summary>TextBox-ul intern, pentru operații nesurfacate (SelectAll, SelectionStart…).</summary>
    Public ReadOnly Property InnerTextBox As TextBox
        Get
            Return _inner
        End Get
    End Property

    ''' <summary>Dă focus TextBox-ului intern (cadrul nu e selectabil).</summary>
    Public Sub FocusInput()
        _inner.Focus()
    End Sub

    ' ── Layout: poziționăm manual TextBox-ul intern, centrat vertical ─────────
    Private Function PadLeft() As Integer
        Return ThemeShapes.ScaleDpi(Me, 12)
    End Function

    Private Function PadRight() As Integer
        Dim p As Integer = ThemeShapes.ScaleDpi(Me, 12)
        If _passwordMode Then p += EyeWidth()
        Return p
    End Function

    Private Function EyeWidth() As Integer
        Return ThemeShapes.ScaleDpi(Me, 30)
    End Function

    Private Function EyeRect() As Rectangle
        Dim w As Integer = EyeWidth()
        Return New Rectangle(Width - w, 0, w, Height)
    End Function

    Private Sub LayoutBox()
        Dim left As Integer = PadLeft()
        Dim w As Integer = Math.Max(0, Width - left - PadRight())
        Dim top As Integer = Math.Max(0, (Height - _inner.Height) \ 2)
        _inner.SetBounds(left, top, w, _inner.Height)
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Try
            LayoutBox()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextField.OnResize", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        Try
            _inner.Font = Font
            LayoutBox()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextField.OnFontChanged", ex)
        End Try
    End Sub

    ''' <summary>Reaplică culorile schemei.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If scheme Is Nothing Then Return
        Dim p As ThemePalette = scheme.Palette
        _fillColor = p.InputBackColor
        _borderColor = p.InputBorderColor
        _focusColor = p.AccentColor
        _glyphColor = p.TextDimColor
        _inner.BackColor = _fillColor
        _inner.ForeColor = p.InputTextColor
        Invalidate()
    End Sub

    ' ── Pictură: fundal rotunjit + contur (accent la focus) + ochi opțional ───
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim radius As Integer = ThemeShapes.ScaleDpi(Me, 6)
            Dim rect As New Rectangle(0, 0, Width - 1, Height - 1)
            Using path As GraphicsPath = ThemeShapes.RoundedRect(rect, radius)
                Using fill As New SolidBrush(_fillColor)
                    g.FillPath(fill, path)
                End Using
                Dim lineColor As Color = If(_focused, _focusColor, _borderColor)
                Dim thickness As Single = If(_focused, ThemeShapes.ScaleDpi(Me, 2), ThemeShapes.ScaleDpi(Me, 1))
                Using pen As New Pen(lineColor, thickness)
                    g.DrawPath(pen, path)
                End Using
            End Using

            If _passwordMode Then DrawEye(g, EyeRect())
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextField.OnPaint", ex)
        End Try
    End Sub

    ' Ochi desenat cu GDI+: elipsă + pupilă; linie oblică („tăiat”) când parola e vizibilă.
    Private Sub DrawEye(g As Graphics, area As Rectangle)
        Dim col As Color = If(_hoverEye, _focusColor, _glyphColor)
        Dim cx As Single = area.Left + area.Width / 2.0F
        Dim cy As Single = area.Top + area.Height / 2.0F
        Dim ew As Single = ThemeShapes.ScaleDpi(Me, 16)
        Dim eh As Single = ThemeShapes.ScaleDpi(Me, 9)
        Using pen As New Pen(col, ThemeShapes.ScaleDpi(Me, 1))
            g.DrawEllipse(pen, cx - ew / 2.0F, cy - eh / 2.0F, ew, eh)
            Dim pr As Single = ThemeShapes.ScaleDpi(Me, 2)
            Using b As New SolidBrush(col)
                g.FillEllipse(b, cx - pr, cy - pr, pr * 2, pr * 2)
            End Using
            If _revealed Then
                g.DrawLine(pen, cx - ew / 2.0F, cy + eh / 2.0F, cx + ew / 2.0F, cy - eh / 2.0F)
            End If
        End Using
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If _passwordMode AndAlso EyeRect().Contains(e.Location) Then
                _revealed = Not _revealed
                _inner.UseSystemPasswordChar = _passwordMode AndAlso Not _revealed
                Invalidate()
                _inner.Focus()
            Else
                _inner.Focus()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextField.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Dim over As Boolean = _passwordMode AndAlso EyeRect().Contains(e.Location)
        If over <> _hoverEye Then
            _hoverEye = over
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hoverEye Then
            _hoverEye = False
            Invalidate()
        End If
    End Sub

    Private Sub OnInnerEnter(sender As Object, e As EventArgs)
        _focused = True
        Invalidate()
    End Sub

    Private Sub OnInnerLeave(sender As Object, e As EventArgs)
        _focused = False
        Invalidate()
    End Sub

    Private Sub OnInnerKeyDown(sender As Object, e As KeyEventArgs)
        RaiseEvent FieldKeyDown(Me, e)
    End Sub

End Class
