Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Partea de TEMĂ a <see cref="KBotDataView"/>: maparea sloturilor din <see cref="ThemePalette"/>
''' pe rolurile grilei, plus cache-ul de resurse GDI (pensule/creioane) recreat la fiecare
''' <c>ApplyTheme</c>. Paleta nu are sloturi dedicate de grilă, deci rolurile derivate se obțin
''' prin <c>Blend</c> — NICIO culoare literală în sursă.
''' </summary>
Partial Class KBotDataView

    ' ── Culori pe roluri (setate în ApplyTheme; default = SystemColors) ──────────
    Private _cHeaderBack As Color
    Private _cHeaderText As Color
    Private _cHeaderSep As Color
    Private _cHeaderBaseline As Color
    Private _cRowBack As Color
    Private _cRowAltBack As Color
    Private _cSelBack As Color
    Private _cSelAltBack As Color
    Private _cSelText As Color
    Private _cGridLine As Color
    Private _cCellText As Color
    Private _cCheckBorder As Color
    Private _cCheckFill As Color
    Private _cCheckMark As Color
    Private _cComboChevron As Color
    Private _cOptionBorder As Color
    Private _cOptionFill As Color
    Private _cOptionDot As Color
    Private _cButtonFace As Color
    Private _cButtonBorder As Color
    Private _cButtonText As Color
    Private _cProgressTrack As Color
    Private _cProgressFill As Color
    Private _cDisabledText As Color
    Private _cDisabledWash As Color

    ' ── Resurse GDI cache-uite (recreate în ApplyTheme, eliberate în Dispose) ─────
    Private _bRowBack As SolidBrush
    Private _bRowAltBack As SolidBrush
    Private _bSelBack As SolidBrush
    Private _bSelAltBack As SolidBrush
    Private _bHeaderBack As SolidBrush
    Private _bCheckFill As SolidBrush
    Private _bComboChevron As SolidBrush
    Private _bOptionFill As SolidBrush
    Private _bOptionDot As SolidBrush
    Private _bButtonFace As SolidBrush
    Private _bProgressTrack As SolidBrush
    Private _bProgressFill As SolidBrush
    Private _bDisabledWash As SolidBrush
    Private _bDisabledMark As SolidBrush
    Private _pDisabledMark As Pen
    Private _pBorder As Pen
    Private _pHeaderSep As Pen
    Private _pGridLine As Pen
    Private _pCheckBorder As Pen
    Private _pCheckFill As Pen
    Private _pHeaderBaseline As Pen
    Private _pOptionBorder As Pen
    Private _pOptionFill As Pen
    Private _pButtonBorder As Pen

    ' Font semibold pentru antet (derivat lazy din fontul ambient).
    Private _headerFont As Font

    ''' <summary>
    ''' Reaplică culorile schemei active. Boundary de temă/pictare: logăm și ÎNGHIȚIM —
    ''' o excepție aici ar rupe traversarea ThemeManager pentru tot formularul.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            ' Antet.
            _cHeaderBack = p.SurfaceAltColor
            _cHeaderText = p.TextColor
            _cHeaderSep = p.BorderColor
            _cHeaderBaseline = p.AccentColor

            ' Zona de date.
            _cRowBack = p.InputBackColor
            _cRowAltBack = Blend(p.InputBackColor, p.SurfaceColor, 0.5)
            _cCellText = p.TextColor
            _cGridLine = p.BorderColor

            ' Selecție: spălare ușoară de accent peste fundalul REAL al rândului, ca textul
            ' să rămână lizibil (de aceea două variante: rând normal / rând alternant).
            _cSelBack = Blend(_cRowBack, p.AccentColor, 0.18)
            _cSelAltBack = Blend(_cRowAltBack, p.AccentColor, 0.18)
            _cSelText = p.TextColor

            ' Bifă / opțiune — aceleași convenții de accent.
            _cCheckBorder = p.BorderColor
            _cCheckFill = p.AccentColor
            _cCheckMark = p.AccentTextColor
            _cOptionBorder = p.BorderColor
            _cOptionFill = p.AccentColor
            _cOptionDot = p.AccentTextColor

            ' Combo / buton / bară de progres.
            _cComboChevron = p.TextDimColor
            _cButtonFace = p.ButtonBackColor
            _cButtonBorder = p.ButtonBorderColor
            _cButtonText = p.ButtonTextColor
            _cProgressTrack = Blend(_cRowBack, p.BorderColor, 0.4)
            _cProgressFill = p.AccentColor

            ' Dezactivat: text șters + o spălare FAINT spre suprafață (nu un gri opac).
            _cDisabledText = p.DisabledTextColor
            _cDisabledWash = Blend(_cRowBack, p.SurfaceColor, 0.4)

            ' Editorii flotanți (controale reale) — tematizați direct.
            editText.BackColor = p.InputBackColor
            editText.ForeColor = p.InputTextColor
            editCombo.BackColor = p.InputBackColor
            editCombo.ForeColor = p.InputTextColor
            editCombo.FlatStyle = FlatStyle.Flat

            BackColor = _cRowBack
            RebuildThemeResources()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ApplyTheme", ex)
        End Try
    End Sub

    ' Culorile pre-temă (până la primul ApplyTheme): SystemColors, ca randarea în designer.
    Private Sub SetDefaultColors()
        _cHeaderBack = SystemColors.Control
        _cHeaderText = SystemColors.ControlText
        _cHeaderSep = SystemColors.ControlDark
        _cHeaderBaseline = SystemColors.Highlight
        _cRowBack = SystemColors.Window
        _cRowAltBack = Blend(SystemColors.Window, SystemColors.Control, 0.5)
        _cSelBack = Blend(SystemColors.Window, SystemColors.Highlight, 0.18)
        _cSelAltBack = Blend(_cRowAltBack, SystemColors.Highlight, 0.18)
        _cSelText = SystemColors.WindowText
        _cCellText = SystemColors.WindowText
        _cGridLine = SystemColors.ControlLight
        _cCheckBorder = SystemColors.ControlDark
        _cCheckFill = SystemColors.Highlight
        _cCheckMark = SystemColors.HighlightText
        _cOptionBorder = SystemColors.ControlDark
        _cOptionFill = SystemColors.Highlight
        _cOptionDot = SystemColors.HighlightText
        _cComboChevron = SystemColors.GrayText
        _cButtonFace = SystemColors.Control
        _cButtonBorder = SystemColors.ControlDark
        _cButtonText = SystemColors.ControlText
        _cProgressTrack = Blend(SystemColors.Window, SystemColors.ControlDark, 0.4)
        _cProgressFill = SystemColors.Highlight
        _cDisabledText = SystemColors.GrayText
        _cDisabledWash = Blend(SystemColors.Window, SystemColors.Control, 0.4)
        BackColor = _cRowBack
    End Sub

    ' Recreează pensulele/creioanele din culorile curente (eliberează-le pe cele vechi).
    Private Sub RebuildThemeResources()
        DisposeThemeResources()
        _bRowBack = New SolidBrush(_cRowBack)
        _bRowAltBack = New SolidBrush(_cRowAltBack)
        _bSelBack = New SolidBrush(_cSelBack)
        _bSelAltBack = New SolidBrush(_cSelAltBack)
        _bHeaderBack = New SolidBrush(_cHeaderBack)
        _bCheckFill = New SolidBrush(_cCheckFill)
        _bComboChevron = New SolidBrush(_cComboChevron)
        _bOptionFill = New SolidBrush(_cOptionFill)
        _bOptionDot = New SolidBrush(_cOptionDot)
        _bButtonFace = New SolidBrush(_cButtonFace)
        _bProgressTrack = New SolidBrush(_cProgressTrack)
        _bProgressFill = New SolidBrush(_cProgressFill)
        _bDisabledWash = New SolidBrush(_cDisabledWash)
        _bDisabledMark = New SolidBrush(_cDisabledText)
        _pDisabledMark = New Pen(_cDisabledText)
        _pBorder = New Pen(_cHeaderSep)
        _pHeaderSep = New Pen(_cHeaderSep)
        _pGridLine = New Pen(_cGridLine)
        _pCheckBorder = New Pen(_cCheckBorder)
        _pCheckFill = New Pen(_cCheckFill)
        _pHeaderBaseline = New Pen(_cHeaderBaseline, 2.0F)
        _pOptionBorder = New Pen(_cOptionBorder)
        _pOptionFill = New Pen(_cOptionFill)
        _pButtonBorder = New Pen(_cButtonBorder)
    End Sub

    ' Eliberează resursele GDI cache-uite + fontul de antet (fără scurgeri).
    Private Sub DisposeThemeResources()
        _bRowBack?.Dispose() : _bRowBack = Nothing
        _bRowAltBack?.Dispose() : _bRowAltBack = Nothing
        _bSelBack?.Dispose() : _bSelBack = Nothing
        _bSelAltBack?.Dispose() : _bSelAltBack = Nothing
        _bHeaderBack?.Dispose() : _bHeaderBack = Nothing
        _bCheckFill?.Dispose() : _bCheckFill = Nothing
        _bComboChevron?.Dispose() : _bComboChevron = Nothing
        _bOptionFill?.Dispose() : _bOptionFill = Nothing
        _bOptionDot?.Dispose() : _bOptionDot = Nothing
        _bButtonFace?.Dispose() : _bButtonFace = Nothing
        _bProgressTrack?.Dispose() : _bProgressTrack = Nothing
        _bProgressFill?.Dispose() : _bProgressFill = Nothing
        _bDisabledWash?.Dispose() : _bDisabledWash = Nothing
        _bDisabledMark?.Dispose() : _bDisabledMark = Nothing
        _pDisabledMark?.Dispose() : _pDisabledMark = Nothing
        _pBorder?.Dispose() : _pBorder = Nothing
        _pHeaderSep?.Dispose() : _pHeaderSep = Nothing
        _pGridLine?.Dispose() : _pGridLine = Nothing
        _pCheckBorder?.Dispose() : _pCheckBorder = Nothing
        _pCheckFill?.Dispose() : _pCheckFill = Nothing
        _pHeaderBaseline?.Dispose() : _pHeaderBaseline = Nothing
        _pOptionBorder?.Dispose() : _pOptionBorder = Nothing
        _pOptionFill?.Dispose() : _pOptionFill = Nothing
        _pButtonBorder?.Dispose() : _pButtonBorder = Nothing
        _headerFont?.Dispose() : _headerFont = Nothing
    End Sub

    ' Fontul antetului: „semibold” derivat lazy din fontul ambient (fallback: bold).
    Private Function HeaderFont() As Font
        If _headerFont Is Nothing Then
            Try
                _headerFont = New Font("Segoe UI Semibold", Font.Size)
            Catch ex As Exception
                GlobalErrorLog.Write("KBotDataView.HeaderFont", ex)
                _headerFont = New Font(Font, FontStyle.Bold)
            End Try
        End If
        Return _headerFont
    End Function

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        _headerFont?.Dispose()
        _headerFont = Nothing
        Invalidate()
    End Sub

    ' ── Ajutoare pure (ThemeShapes din KBot.Theming e Friend, invizibil de aici) ──

    ''' <summary>Amestec liniar între două culori: t=0 => a, t=1 => b (t limitat la 0..1).</summary>
    Private Shared Function Blend(a As Color, b As Color, t As Double) As Color
        Dim tt As Double = Math.Max(0.0, Math.Min(1.0, t))
        Dim r As Integer = CInt(CDbl(a.R) + (CDbl(b.R) - a.R) * tt)
        Dim g As Integer = CInt(CDbl(a.G) + (CDbl(b.G) - a.G) * tt)
        Dim bl As Integer = CInt(CDbl(a.B) + (CDbl(b.B) - a.B) * tt)
        Return Color.FromArgb(r, g, bl)
    End Function

    ''' <summary>Cale dreptunghi cu colțuri rotunjite (radius deja în px scalați).</summary>
    Private Shared Function RoundedRect(bounds As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim d As Integer = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height))
        If d <= 0 Then
            path.AddRectangle(bounds)
            Return path
        End If
        Dim arc As New Rectangle(bounds.Location, New Size(d, d))
        path.AddArc(arc, 180, 90)
        arc.X = bounds.Right - d
        path.AddArc(arc, 270, 90)
        arc.Y = bounds.Bottom - d
        path.AddArc(arc, 0, 90)
        arc.X = bounds.Left
        path.AddArc(arc, 90, 90)
        path.CloseFigure()
        Return path
    End Function

End Class
