Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Bară de titlu proprie pentru formularele fără chenar (FormBorderStyle.None):
''' pictogramă + titlu în stânga, buton de închidere (și, opțional, minimizare) în
''' dreapta, tragerea ferestrei de pe zona liberă. Toate culorile vin din schema
''' activă (via <see cref="ApplyTheme"/>); nicio culoare hardcodată.
''' </summary>
<ToolboxItem(False)>
Public NotInheritable Class KBotCaptionBar
    Inherits Control
    Implements IThemedControl

    ' ── Culori derivate din paletă (setate în ApplyTheme) ─────────────────────
    Private _backColor As Color = SystemColors.Control
    Private _titleColor As Color = SystemColors.ControlText
    Private _glyphColor As Color = SystemColors.ControlText
    Private _closeHoverColor As Color = Color.FromArgb(196, 43, 28)
    Private _btnHoverColor As Color = SystemColors.ControlLight

    ' ── Stare ─────────────────────────────────────────────────────────────────
    Private _iconImage As Image
    Private _showMinimize As Boolean = False
    Private _showMaximize As Boolean = False
    Private _hoverClose As Boolean = False
    Private _hoverMin As Boolean = False
    Private _hoverMax As Boolean = False

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Height = 40
    End Sub

    ''' <summary>Pictograma afișată la stânga titlului (opțională).</summary>
    Public Property IconImage As Image
        Get
            Return _iconImage
        End Get
        Set(value As Image)
            _iconImage = value
            Invalidate()
        End Set
    End Property

    ''' <summary>Arată și butonul de minimizare (implicit doar închiderea).</summary>
    Public Property ShowMinimize As Boolean
        Get
            Return _showMinimize
        End Get
        Set(value As Boolean)
            _showMinimize = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Arată și butonul de maximizare/restaurare (implicit ascuns — dialogurile gen
    ''' LoginForm rămân neatinse). Activează și dublu-click pe zona de tragere.
    ''' </summary>
    Public Property ShowMaximize As Boolean
        Get
            Return _showMaximize
        End Get
        Set(value As Boolean)
            _showMaximize = value
            Invalidate()
        End Set
    End Property

    ' Titlul e Text-ul controlului (setat în designer). Repictăm la schimbare.
    Protected Overrides Sub OnTextChanged(e As EventArgs)
        MyBase.OnTextChanged(e)
        Invalidate()
    End Sub

    ''' <summary>Reaplică culorile schemei.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If scheme Is Nothing Then Return
        Dim p As ThemePalette = scheme.Palette
        _backColor = p.SurfaceAltColor
        _titleColor = p.TextColor
        _glyphColor = p.TextDimColor
        _closeHoverColor = p.ErrorColor
        _btnHoverColor = ThemeShapes.Blend(p.SurfaceAltColor, p.BorderColor, 0.6)
        BackColor = _backColor
        Invalidate()
    End Sub

    ' ── Metrici butoane (calculate din înălțime/DPI) ──────────────────────────
    ' Pozițiile se derivă dintr-un singur loc (SlotRect): slotul 0 e lipit de dreapta,
    ' următoarele merg spre stânga. Ordinea dreapta→stânga: close, maximize, minimize.
    Private Function BtnWidth() As Integer
        Return ThemeShapes.ScaleDpi(Me, 46)
    End Function

    Private Function SlotRect(slot As Integer) As Rectangle
        Dim w As Integer = BtnWidth()
        Return New Rectangle(Width - w * (slot + 1), 0, w, Height)
    End Function

    Private Function CloseRect() As Rectangle
        Return SlotRect(0)
    End Function

    ' Valid doar când _showMaximize e True (altfel slotul aparține minimizării).
    Private Function MaxRect() As Rectangle
        Return SlotRect(1)
    End Function

    Private Function MinRect() As Rectangle
        Return SlotRect(If(_showMaximize, 2, 1))
    End Function

    ' Limita din dreapta a titlului = marginea stângă a celui mai din stânga buton vizibil.
    Private Function TitleRightLimit() As Integer
        If _showMinimize Then Return MinRect().Left
        If _showMaximize Then Return MaxRect().Left
        Return CloseRect().Left
    End Function

    ' Comută starea ferestrei părinte între Normal și Maximized.
    Private Sub ToggleMaximize()
        Dim f As Form = FindForm()
        If f Is Nothing Then Return
        f.WindowState = If(f.WindowState = FormWindowState.Maximized,
                           FormWindowState.Normal, FormWindowState.Maximized)
        Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.Clear(_backColor)

            Dim pad As Integer = ThemeShapes.ScaleDpi(Me, 12)
            Dim x As Integer = pad

            ' Pictogramă (pătrată, centrată vertical).
            If _iconImage IsNot Nothing Then
                Dim side As Integer = Math.Min(Height - ThemeShapes.ScaleDpi(Me, 14), ThemeShapes.ScaleDpi(Me, 24))
                If side > 0 Then
                    Dim iy As Integer = (Height - side) \ 2
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    g.DrawImage(_iconImage, New Rectangle(x, iy, side, side))
                    x += side + ThemeShapes.ScaleDpi(Me, 8)
                End If
            End If

            ' Titlu.
            If Not String.IsNullOrEmpty(Text) Then
                Dim rightLimit As Integer = TitleRightLimit()
                Dim titleRect As New Rectangle(x, 0, Math.Max(0, rightLimit - x - pad), Height)
                TextRenderer.DrawText(g, Text, Font, titleRect, _titleColor,
                    TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
            End If

            g.SmoothingMode = SmoothingMode.AntiAlias

            ' Buton minimizare (opțional).
            If _showMinimize Then
                Dim mr As Rectangle = MinRect()
                If _hoverMin Then
                    Using hb As New SolidBrush(_btnHoverColor)
                        g.FillRectangle(hb, mr)
                    End Using
                End If
                Using pen As New Pen(_glyphColor, ThemeShapes.ScaleDpi(Me, 1))
                    Dim cy As Integer = mr.Top + mr.Height \ 2
                    Dim half As Integer = ThemeShapes.ScaleDpi(Me, 5)
                    g.DrawLine(pen, mr.Left + mr.Width \ 2 - half, cy, mr.Left + mr.Width \ 2 + half, cy)
                End Using
            End If

            ' Buton maximizare / restaurare (opțional).
            If _showMaximize Then
                Dim xr As Rectangle = MaxRect()
                If _hoverMax Then
                    Using hb As New SolidBrush(_btnHoverColor)
                        g.FillRectangle(hb, xr)
                    End Using
                End If
                Dim parentForm As Form = FindForm()
                Dim maximized As Boolean = parentForm IsNot Nothing AndAlso
                                           parentForm.WindowState = FormWindowState.Maximized
                Using pen As New Pen(_glyphColor, ThemeShapes.ScaleDpi(Me, 1))
                    Dim half As Integer = ThemeShapes.ScaleDpi(Me, 5)
                    Dim cx As Integer = xr.Left + xr.Width \ 2
                    Dim cy As Integer = xr.Top + xr.Height \ 2
                    If maximized Then
                        ' Restaurare: două pătrate suprapuse (cel din spate decalat dreapta-sus).
                        Dim off As Integer = ThemeShapes.ScaleDpi(Me, 2)
                        Dim side As Integer = 2 * half - off
                        g.DrawRectangle(pen, cx - half + off, cy - half, side, side)
                        Using bg As New SolidBrush(If(_hoverMax, _btnHoverColor, _backColor))
                            g.FillRectangle(bg, cx - half, cy - half + off, side, side)
                        End Using
                        g.DrawRectangle(pen, cx - half, cy - half + off, side, side)
                    Else
                        ' Maximizare: un pătrat.
                        g.DrawRectangle(pen, cx - half, cy - half, 2 * half, 2 * half)
                    End If
                End Using
            End If

            ' Buton închidere.
            Dim cr As Rectangle = CloseRect()
            Dim closeGlyph As Color = _glyphColor
            If _hoverClose Then
                Using hb As New SolidBrush(_closeHoverColor)
                    g.FillRectangle(hb, cr)
                End Using
                closeGlyph = Color.White
            End If
            Using pen As New Pen(closeGlyph, ThemeShapes.ScaleDpi(Me, 1))
                Dim half As Integer = ThemeShapes.ScaleDpi(Me, 5)
                Dim ccx As Integer = cr.Left + cr.Width \ 2
                Dim ccy As Integer = cr.Top + cr.Height \ 2
                g.DrawLine(pen, ccx - half, ccy - half, ccx + half, ccy + half)
                g.DrawLine(pen, ccx + half, ccy - half, ccx - half, ccy + half)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCaptionBar.OnPaint", ex)
        End Try
    End Sub

    ' True dacă punctul e pe oricare dintre butoanele vizibile.
    Private Function IsOnButton(location As Point) As Boolean
        If CloseRect().Contains(location) Then Return True
        If _showMaximize AndAlso MaxRect().Contains(location) Then Return True
        If _showMinimize AndAlso MinRect().Contains(location) Then Return True
        Return False
    End Function

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            Dim overClose As Boolean = CloseRect().Contains(e.Location)
            Dim overMax As Boolean = _showMaximize AndAlso MaxRect().Contains(e.Location)
            Dim overMin As Boolean = _showMinimize AndAlso MinRect().Contains(e.Location)
            If overClose <> _hoverClose OrElse overMin <> _hoverMin OrElse overMax <> _hoverMax Then
                _hoverClose = overClose
                _hoverMin = overMin
                _hoverMax = overMax
                Invalidate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCaptionBar.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hoverClose OrElse _hoverMin OrElse _hoverMax Then
            _hoverClose = False
            _hoverMin = False
            _hoverMax = False
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            ' Pe butoane NU tragem fereastra (Click-ul le va acționa); altfel drag.
            If IsOnButton(e.Location) Then Return
            ' Al doilea click al unui dublu-click (Clicks=2) NU pornește drag-ul:
            ' DragMove ar intra în bucla modală de mutare și ar înghiți dublu-click-ul
            ' (OnMouseDoubleClick de mai jos face comutarea maximize/restore).
            If _showMaximize AndAlso e.Clicks >= 2 Then Return
            Dim f As Form = FindForm()
            If f IsNot Nothing Then NativeMethods.DragMove(f)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCaptionBar.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Dim f As Form = FindForm()
            If f Is Nothing Then Return
            If CloseRect().Contains(e.Location) Then
                f.Close()
            ElseIf _showMaximize AndAlso MaxRect().Contains(e.Location) Then
                ToggleMaximize()
            ElseIf _showMinimize AndAlso MinRect().Contains(e.Location) Then
                f.WindowState = FormWindowState.Minimized
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCaptionBar.OnMouseClick", ex)
        End Try
    End Sub

    ' Dublu-click pe zona de tragere (nu pe butoane) comută maximize/restore —
    ' doar când bara are butonul de maximizare (ShowMaximize=True).
    Protected Overrides Sub OnMouseDoubleClick(e As MouseEventArgs)
        MyBase.OnMouseDoubleClick(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            If Not _showMaximize Then Return
            If IsOnButton(e.Location) Then Return
            ToggleMaximize()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCaptionBar.OnMouseDoubleClick", ex)
        End Try
    End Sub

End Class
