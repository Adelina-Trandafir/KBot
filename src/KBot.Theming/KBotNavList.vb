Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Navigație verticală owner-drawn — „tab-urile" simulate ale shell-ului (înlocuiește
''' TabControl-ul netematizabil). Un element = cheie + text + badge opțional + Enabled.
''' Selecția se schimbă cu click sau Sus/Jos; re-selectarea aceleiași chei NU re-ridică
''' <see cref="SelectionChanged"/>. Toate culorile vin din schema activă (ApplyTheme).
''' </summary>
<ToolboxItem(False)>
Public NotInheritable Class KBotNavList
    Inherits Control
    Implements IThemedControl

    ''' <summary>Un element de navigație (model intern).</summary>
    Private NotInheritable Class NavItem
        Public Property Key As String
        Public Property Text As String
        Public Property Badge As Integer          ' 0 = ascuns
        Public Property Enabled As Boolean = True
    End Class

    Private ReadOnly _items As New List(Of NavItem)()
    Private _selectedKey As String
    Private _hoverIndex As Integer = -1

    ' ── Culori/stil derivate din paletă (setate în ApplyTheme) ────────────────
    Private _scheme As ThemeScheme
    Private _selectedFill As Color = SystemColors.ControlLight
    Private _accent As Color = SystemColors.Highlight
    Private _hoverFill As Color = SystemColors.ControlLight
    Private _textNormal As Color = SystemColors.GrayText
    Private _textDisabled As Color = SystemColors.GrayText
    Private _badgeFill As Color = SystemColors.Control
    Private _badgeText As Color = SystemColors.GrayText

    ' Font semibold pentru elementul selectat (derivat din fontul ambient).
    Private _semiboldFont As Font

    ''' <summary>Ridicat când selecția se schimbă (click, tastatură sau setter).</summary>
    Public Event SelectionChanged(key As String)

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable, True)
        TabStop = True
        Width = 170
    End Sub

    ' ── API public ─────────────────────────────────────────────────────────────

    ''' <summary>Adaugă un element. Cheia trebuie să fie nevidă și unică.</summary>
    Public Sub AddItem(key As String, text As String)
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Cheie vidă.", NameOf(key))
        If FindIndex(key) >= 0 Then Throw New ArgumentException($"Cheie duplicată: '{key}'.", NameOf(key))
        _items.Add(New NavItem With {.Key = key, .Text = If(text, String.Empty)})
        Invalidate()
    End Sub

    ''' <summary>Setează badge-ul unui element (0 = ascuns). Cheie necunoscută => excepție.</summary>
    Public Sub SetBadge(key As String, count As Integer)
        _items(RequireIndex(key)).Badge = count
        Invalidate()
    End Sub

    ''' <summary>Activează/dezactivează un element. Cheie necunoscută => excepție.</summary>
    Public Sub SetItemEnabled(key As String, enabled As Boolean)
        _items(RequireIndex(key)).Enabled = enabled
        Invalidate()
    End Sub

    ''' <summary>
    ''' Cheia selectată. Setarea unei chei necunoscute aruncă ArgumentException;
    ''' setarea aceleiași chei nu re-ridică evenimentul.
    ''' </summary>
    Public Property SelectedKey As String
        Get
            Return _selectedKey
        End Get
        Set(value As String)
            SelectIndex(RequireIndex(value))
        End Set
    End Property

    ' ── Interne ────────────────────────────────────────────────────────────────

    Private Function FindIndex(key As String) As Integer
        For i As Integer = 0 To _items.Count - 1
            If String.Equals(_items(i).Key, key, StringComparison.Ordinal) Then Return i
        Next
        Return -1
    End Function

    ' Indexul cheii sau ArgumentException — fără no-op-uri tăcute (regula casei).
    Private Function RequireIndex(key As String) As Integer
        Dim idx As Integer = FindIndex(key)
        If idx < 0 Then Throw New ArgumentException($"Cheie necunoscută: '{key}'.", NameOf(key))
        Return idx
    End Function

    ' Selectează prin index; ridică evenimentul DOAR la schimbare reală.
    Private Sub SelectIndex(index As Integer)
        Dim key As String = _items(index).Key
        If String.Equals(key, _selectedKey, StringComparison.Ordinal) Then Return
        _selectedKey = key
        Invalidate()
        RaiseEvent SelectionChanged(key)
    End Sub

    Private Function ItemHeight() As Integer
        Return ThemeShapes.ScaleDpi(Me, 36)
    End Function

    Private Function ItemRect(index As Integer) As Rectangle
        Dim margin As Integer = ThemeShapes.ScaleDpi(Me, 6)
        Dim h As Integer = ItemHeight()
        Return New Rectangle(margin, index * h, Math.Max(0, Width - 2 * margin), h - ThemeShapes.ScaleDpi(Me, 2))
    End Function

    Private Function IndexAt(location As Point) As Integer
        For i As Integer = 0 To _items.Count - 1
            If ItemRect(i).Contains(location) Then Return i
        Next
        Return -1
    End Function

    ' ── Temă ───────────────────────────────────────────────────────────────────

    ''' <summary>Reaplică culorile schemei.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If scheme Is Nothing Then Return
        _scheme = scheme
        Dim p As ThemePalette = scheme.Palette
        ' Accent „soft" pentru fundalul selecției: paleta nu are un slot AccentSoft —
        ' se derivă amestecând 14% accent în SurfaceAlt (nu adăugăm slot nou).
        _selectedFill = ThemeShapes.Blend(p.SurfaceAltColor, p.AccentColor, 0.14)
        _accent = p.AccentColor
        _hoverFill = p.ButtonHoverColor
        _textNormal = p.TextDimColor
        _textDisabled = p.DisabledTextColor
        _badgeFill = p.SurfaceColor
        _badgeText = p.TextDimColor
        BackColor = p.SurfaceColor
        Invalidate()
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        _semiboldFont?.Dispose()
        _semiboldFont = Nothing
        Invalidate()
    End Sub

    ' Fontul selecției: „semibold" derivat lazy din fontul ambient (fallback: bold).
    Private Function SemiboldFont() As Font
        If _semiboldFont Is Nothing Then
            Try
                _semiboldFont = New Font("Segoe UI Semibold", Font.Size)
            Catch ex As Exception
                GlobalErrorLog.Write("KBotNavList.SemiboldFont", ex)
                _semiboldFont = New Font(Font, FontStyle.Bold)
            End Try
        End If
        Return _semiboldFont
    End Function

    ' ── Pictare ────────────────────────────────────────────────────────────────

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.Clear(BackColor)
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim radius As Integer = ThemeShapes.ScaleDpi(Me, If(_scheme IsNot Nothing, _scheme.Style.CornerRadius, 0))
            Dim padX As Integer = ThemeShapes.ScaleDpi(Me, 12)

            For i As Integer = 0 To _items.Count - 1
                Dim it As NavItem = _items(i)
                Dim r As Rectangle = ItemRect(i)
                If r.Width <= 0 OrElse r.Height <= 0 Then Continue For

                Dim isSelected As Boolean = String.Equals(it.Key, _selectedKey, StringComparison.Ordinal)
                Dim isHover As Boolean = (i = _hoverIndex) AndAlso it.Enabled AndAlso Not isSelected

                ' Fundal: selectat = accent soft; hover = ButtonHover; normal = transparent.
                If isSelected OrElse isHover Then
                    Using path As GraphicsPath = ThemeShapes.RoundedRect(r, radius)
                        Using b As New SolidBrush(If(isSelected, _selectedFill, _hoverFill))
                            g.FillPath(b, path)
                        End Using
                    End Using
                End If

                ' Badge (pastilă rotunjită, aliniată dreapta) — desenată înaintea
                ' textului ca să-i putem rezerva lățimea.
                Dim textRight As Integer = r.Right - padX
                If it.Badge > 0 Then
                    Dim badgeText As String = it.Badge.ToString()
                    Dim ts As Size = TextRenderer.MeasureText(g, badgeText, Font)
                    Dim bh As Integer = ThemeShapes.ScaleDpi(Me, 18)
                    Dim bw As Integer = Math.Max(bh, ts.Width + ThemeShapes.ScaleDpi(Me, 10))
                    Dim br As New Rectangle(r.Right - bw - ThemeShapes.ScaleDpi(Me, 8),
                                            r.Top + (r.Height - bh) \ 2, bw, bh)
                    Using path As GraphicsPath = ThemeShapes.RoundedRect(br, bh \ 2)
                        Using b As New SolidBrush(_badgeFill)
                            g.FillPath(b, path)
                        End Using
                    End Using
                    TextRenderer.DrawText(g, badgeText, Font, br, _badgeText,
                        TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                    textRight = br.Left - ThemeShapes.ScaleDpi(Me, 4)
                End If

                ' Text.
                Dim textColor As Color
                Dim textFont As Font = Font
                If Not it.Enabled Then
                    textColor = _textDisabled
                ElseIf isSelected Then
                    textColor = _accent
                    textFont = SemiboldFont()
                Else
                    textColor = _textNormal
                End If
                Dim tr As New Rectangle(r.Left + padX, r.Top, Math.Max(0, textRight - r.Left - padX), r.Height)
                TextRenderer.DrawText(g, it.Text, textFont, tr, textColor,
                    TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNavList.OnPaint", ex)
        End Try
    End Sub

    ' ── Mouse ──────────────────────────────────────────────────────────────────

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            Dim idx As Integer = IndexAt(e.Location)
            If idx >= 0 AndAlso Not _items(idx).Enabled Then idx = -1   ' fără hover pe disabled
            If idx <> _hoverIndex Then
                _hoverIndex = idx
                Invalidate()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNavList.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hoverIndex <> -1 Then
            _hoverIndex = -1
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Focus()
            Dim idx As Integer = IndexAt(e.Location)
            If idx >= 0 AndAlso _items(idx).Enabled Then SelectIndex(idx)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNavList.OnMouseClick", ex)
        End Try
    End Sub

    ' ── Tastatură ──────────────────────────────────────────────────────────────

    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        If keyData = Keys.Up OrElse keyData = Keys.Down Then Return True
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            If e.KeyCode <> Keys.Up AndAlso e.KeyCode <> Keys.Down Then Return
            If _items.Count = 0 Then Return
            Dim direction As Integer = If(e.KeyCode = Keys.Down, 1, -1)
            Dim start As Integer = FindIndex(_selectedKey)
            ' Caută următorul element ACTIV în direcția cerută (fără wrap).
            Dim idx As Integer = start + direction
            While idx >= 0 AndAlso idx < _items.Count
                If _items(idx).Enabled Then
                    SelectIndex(idx)
                    Exit While
                End If
                idx += direction
            End While
            e.Handled = True
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNavList.OnKeyDown", ex)
        End Try
    End Sub

    ' Focusul se vede prin re-pictare (viitor inel de focus dacă va fi nevoie).
    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnLostFocus(e As EventArgs)
        MyBase.OnLostFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _semiboldFont?.Dispose()
            _semiboldFont = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
