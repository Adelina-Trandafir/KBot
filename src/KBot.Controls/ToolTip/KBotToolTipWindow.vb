Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' FEREASTRA etichetei plutitoare K-BOT — sora lui <c>KBotCellTooltipWindow</c> și
''' <c>TreeNodeFlyout</c>, cu aceleași trucuri și din aceleași motive:
''' <c>WS_EX_NOACTIVATE</c> + <see cref="ShowWithoutActivation"/> (nu fură focusul din formularul
''' de dedesubt), <c>WS_EX_TOOLWINDOW</c> (fără buton în bara de activități), afișare prin
''' <c>ShowWindow(SW_SHOWNOACTIVATE)</c> în loc de <c>Visible = True</c> (acela ar atinge
''' <c>Application.ActiveForm</c>) și <c>HTTRANSPARENT</c> pe <c>WM_NCHITTEST</c>, ca mouse-ul să
''' treacă PRIN ea la controlul de sub ea.
'''
''' <para>Ultimul punct nu e cosmetic: eticheta se așază lângă cursor și îl poate atinge. Fără
''' click-through, apariția ei ar scoate cursorul de pe control, ceea ce ar ascunde eticheta,
''' ceea ce ar readuce cursorul pe control… la nesfârșit.</para>
'''
''' <para><b>Nu e un <see cref="System.Windows.Forms.ToolTip"/>.</b> Acela își ia culorile din
''' sistem, nu se poate rotunji, n-are antet, n-are subsol, n-are linie despărțitoare și nu poate
''' scrie text îmbogățit. Toate cele cinci sunt cerute aici.</para>
'''
''' <para><b>Așezarea</b> se face pe secțiuni, de sus în jos: antet, linie, corp, linie, subsol.
''' O secțiune fără conținut nu ocupă nimic, iar linia dinaintea ei dispare odată cu ea.</para>
''' </summary>
Friend NotInheritable Class KBotToolTipWindow
    Inherits Form

    Private Const WM_NCHITTEST As Integer = &H84
    Private Const HTTRANSPARENT As Integer = -1
    Private Const WS_EX_NOACTIVATE As Integer = &H8000000
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_TRANSPARENT As Integer = &H20
    Private Const SW_SHOWNOACTIVATE As Integer = 4
    Private Const HWND_TOPMOST As Integer = -1
    Private Const SWP_NOACTIVATE As UInteger = &H10
    Private Const SWP_SHOWWINDOW As UInteger = &H40

    <Runtime.InteropServices.DllImport("user32.dll")>
    Private Shared Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

    <Runtime.InteropServices.DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As IntPtr,
                                         X As Integer, Y As Integer, cx As Integer, cy As Integer,
                                         uFlags As UInteger) As Boolean
    End Function

    ' ── Ce se pictează (rezolvat deja: aici nu mai există „gol = din temă") ────
    Private _style As KBotToolTipStyle
    Private _content As KBotToolTipContent
    Private _bodyFont As Font
    Private _headerFont As Font
    Private _footerFont As Font
    Private _headerFontDerived As Boolean
    Private _footerFontDerived As Boolean

    ' Culorile rezolvate din temă la fiecare afișare.
    Private _fill As Color = SystemColors.Info
    Private _fore As Color = SystemColors.InfoText
    Private _border As Color = SystemColors.ActiveBorder
    Private _headerFore As Color = SystemColors.InfoText
    Private _footerFore As Color = SystemColors.GrayText
    Private _sepFore As Color = SystemColors.ActiveBorder

    ' Așezarea calculată la ultima măsurătoare.
    Private _headerRect As Rectangle
    Private _bodyRect As Rectangle
    Private _footerRect As Rectangle
    Private _sepTop As Integer = -1
    Private _sepBottom As Integer = -1
    Private _bodyLayout As KBotRichText.RichLayout
    Private _headerLayout As KBotRichText.RichLayout
    Private _footerLayout As KBotRichText.RichLayout
    Private _headerIconRect As Rectangle
    Private _footerIconRect As Rectangle

    Private ReadOnly _autoHide As New Timer()

    Friend Sub New()
        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        ControlBox = False
        MinimizeBox = False
        MaximizeBox = False
        TopMost = True
        Text = String.Empty
        ' Fără autoscalare: primim Bounds în px DEJA scalați la DPI-ul ecranului pe care apărem.
        ' O a doua ajustare ar muta eticheta de lângă controlul ei.
        AutoScaleMode = AutoScaleMode.None
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer, True)
        AddHandler _autoHide.Tick, AddressOf AutoHideTick
    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_NOACTIVATE Or WS_EX_TOOLWINDOW Or WS_EX_TRANSPARENT
            Return cp
        End Get
    End Property

    ' Regula casei lasă WndProc pe plasa globală Application.ThreadException: un Try/Catch aici
    ' ar risca să rupă contractul de mesaje al ferestrei.
    Protected Overrides Sub WndProc(ByRef m As Message)
        MyBase.WndProc(m)
        If m.Msg = WM_NCHITTEST Then m.Result = New IntPtr(HTTRANSPARENT)
    End Sub

    ' Ocolim MyBase.SetVisibleCore(True): acela actualizează Application.ActiveForm și
    ' Application.OpenForms și poate destabiliza ActiveControl din formularul de dedesubt.
    Protected Overrides Sub SetVisibleCore(value As Boolean)
        If value Then
            If Not IsHandleCreated Then CreateHandle()
            ShowWindow(Handle, SW_SHOWNOACTIVATE)
            OnVisibleChanged(EventArgs.Empty)
        Else
            MyBase.SetVisibleCore(False)
        End If
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' AFIȘARE
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Măsoară, așază și arată eticheta lângă <paramref name="screenPos"/> (poziția cursorului,
    ''' în coordonate de ECRAN). <paramref name="autoHideMs"/> &lt;= 0 înseamnă „nu se stinge
    ''' singură" — dispariția rămâne treaba componentei, la ieșirea cursorului.
    ''' </summary>
    Friend Sub ShowTip(content As KBotToolTipContent, style As KBotToolTipStyle,
                       controlFont As Font, screenPos As Point, autoHideMs As Integer)
        Try
            _autoHide.Stop()
            _content = content
            _style = style
            If _content Is Nothing OrElse _style Is Nothing Then Return

            ReleaseFonts()
            ResolveColors()
            ResolveFonts(controlFont)

            Dim size As Size = MeasureAll()
            If size.Width <= 0 OrElse size.Height <= 0 Then Return

            Dim ecran As Screen = Screen.FromPoint(screenPos)
            Dim wa As Rectangle = ecran.WorkingArea
            ' Sub cursor, la dreapta lui — locul obișnuit. Dacă nu încape, se întoarce peste
            ' cursor / la stânga lui, niciodată în afara ecranului de lucru.
            Dim x As Integer = screenPos.X + Dpi(16)
            Dim y As Integer = screenPos.Y + Dpi(20)
            If x + size.Width > wa.Right Then x = screenPos.X - size.Width - Dpi(4)
            If y + size.Height > wa.Bottom Then y = screenPos.Y - size.Height - Dpi(4)
            If x < wa.Left Then x = wa.Left
            If y < wa.Top Then y = wa.Top

            Bounds = New Rectangle(x, y, size.Width, size.Height)
            BackColor = _fill
            ApplyRegion()
            Invalidate()

            Visible = True
            SetWindowPos(Handle, New IntPtr(HWND_TOPMOST), Left, Top, Width, Height,
                         SWP_NOACTIVATE Or SWP_SHOWWINDOW)

            If autoHideMs > 0 Then
                _autoHide.Interval = autoHideMs
                _autoHide.Start()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTipWindow.ShowTip", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Stinge eticheta și oprește ceasul de auto-ascundere.</summary>
    Friend Sub HideTip()
        Try
            _autoHide.Stop()
            If Visible Then Visible = False
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTipWindow.HideTip", ex)
        End Try
    End Sub

    Private Sub AutoHideTick(sender As Object, e As EventArgs)
        Try
            _autoHide.Stop()
            HideTip()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTipWindow.AutoHideTick", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' REZOLVAREA temei (gol = din schema activă)
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub ResolveColors()
        Dim p As ThemePalette = ThemeManager.Current.Palette
        _fill = If(_style.BackColor <> Color.Empty, _style.BackColor, p.SurfaceAltColor)
        _fore = If(_style.ForeColor <> Color.Empty, _style.ForeColor, p.TextColor)
        _border = If(_style.BorderColor <> Color.Empty, _style.BorderColor, p.BorderColor)
        _sepFore = If(_style.Separator.ForeColor <> Color.Empty, _style.Separator.ForeColor, p.BorderColor)
        ' Antetul poartă culoarea textului obișnuit; subsolul e o notă, deci mai stins. Asta e
        ' singura diferență de tratament între cele două benzi.
        _headerFore = If(_style.Header.ForeColor <> Color.Empty, _style.Header.ForeColor, p.TextColor)
        _footerFore = If(_style.Footer.ForeColor <> Color.Empty, _style.Footer.ForeColor, p.TextDimColor)
    End Sub

    ' Fontul corpului: cel din stil, altfel cel al controlului care a cerut eticheta.
    ' Antetul, dacă n-are font propriu, îl îngroașă pe al corpului — un antet care arată exact ca
    ' și corpul nu e antet.
    Private Sub ResolveFonts(controlFont As Font)
        _bodyFont = If(_style.Font, If(controlFont, SystemFonts.DefaultFont))

        If _style.Header.Font IsNot Nothing Then
            _headerFont = _style.Header.Font
            _headerFontDerived = False
        Else
            _headerFont = New Font(_bodyFont, _bodyFont.Style Or FontStyle.Bold)
            _headerFontDerived = True
        End If

        If _style.Footer.Font IsNot Nothing Then
            _footerFont = _style.Footer.Font
            _footerFontDerived = False
        Else
            _footerFont = _bodyFont
            _footerFontDerived = False
        End If
    End Sub

    Private Sub ReleaseFonts()
        If _headerFontDerived AndAlso _headerFont IsNot Nothing Then _headerFont.Dispose()
        If _footerFontDerived AndAlso _footerFont IsNot Nothing Then _footerFont.Dispose()
        _headerFont = Nothing
        _footerFont = Nothing
        _headerFontDerived = False
        _footerFontDerived = False
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' MĂSURAREA — de sus în jos, secțiune cu secțiune
    ' ══════════════════════════════════════════════════════════════════════════

    ' Valoare logică (px @96dpi) -> px la DPI-ul ferestrei. Toate măsurile din stil sunt logice;
    ' fonturile se scalează singure (sunt în puncte), deci NUMAI geometria trece pe aici.
    Private Function Dpi(logical As Integer) As Integer
        Dim d As Integer = 96
        Try
            d = DeviceDpi
        Catch
            d = 96
        End Try
        Return CInt(Math.Round(logical * d / 96.0))
    End Function

    Private Function MeasureAll() As Size
        _headerRect = Rectangle.Empty
        _bodyRect = Rectangle.Empty
        _footerRect = Rectangle.Empty
        _headerIconRect = Rectangle.Empty
        _footerIconRect = Rectangle.Empty
        _sepTop = -1
        _sepBottom = -1

        Dim padL As Integer = Dpi(_style.Padding.Left)
        Dim padT As Integer = Dpi(_style.Padding.Top)
        Dim padR As Integer = Dpi(_style.Padding.Right)
        Dim padB As Integer = Dpi(_style.Padding.Bottom)
        Dim maxBody As Integer = Dpi(_style.MaxWidth)

        Dim antetText As String = _content.EffectiveHeaderText(_style)
        Dim antetIcon As Image = _content.EffectiveHeaderIcon(_style)
        Dim subsolText As String = _content.EffectiveFooterText(_style)
        Dim subsolIcon As Image = _content.EffectiveFooterIcon(_style)

        Dim areAntet As Boolean = _style.Header.Visible AndAlso
                                  (Not String.IsNullOrEmpty(antetText) OrElse antetIcon IsNot Nothing)
        Dim areSubsol As Boolean = _style.Footer.Visible AndAlso
                                   (Not String.IsNullOrEmpty(subsolText) OrElse subsolIcon IsNot Nothing)
        Dim areCorp As Boolean = Not String.IsNullOrEmpty(_content.Text)

        Dim latimeMax As Integer = 0
        Dim y As Integer = padT

        Using g As Graphics = CreateGraphics()
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit

            ' ── Antet ────────────────────────────────────────────────────────
            Dim hAntet As Integer = 0
            If areAntet Then
                Dim iconW As Integer = 0
                Dim iconH As Integer = 0
                If antetIcon IsNot Nothing Then
                    iconW = Dpi(_style.Header.IconSize.Width)
                    iconH = Dpi(_style.Header.IconSize.Height)
                End If
                Dim gap As Integer = If(antetIcon IsNot Nothing AndAlso Not String.IsNullOrEmpty(antetText),
                                        Dpi(_style.Header.IconGap), 0)
                Dim latText As Integer = Math.Max(16, maxBody - iconW - gap)
                _headerLayout = LayoutOf(antetText, _headerFont, _headerFore, g, latText)

                Dim bandaPadT As Integer = Dpi(_style.Header.Padding.Top)
                Dim bandaPadB As Integer = Dpi(_style.Header.Padding.Bottom)
                hAntet = Math.Max(_headerLayout.Height, iconH) + bandaPadT + bandaPadB
                latimeMax = Math.Max(latimeMax, iconW + gap + _headerLayout.Width +
                                                Dpi(_style.Header.Padding.Left) + Dpi(_style.Header.Padding.Right))

                _headerRect = New Rectangle(padL, y, 0, hAntet)   ' lățimea se completează la final
                If antetIcon IsNot Nothing Then
                    _headerIconRect = New Rectangle(padL + Dpi(_style.Header.Padding.Left),
                                                    y + bandaPadT + Math.Max(0, (Math.Max(_headerLayout.Height, iconH) - iconH) \ 2),
                                                    iconW, iconH)
                End If
                y += hAntet
            End If

            ' ── Linia de sub antet ───────────────────────────────────────────
            If areAntet AndAlso (areCorp OrElse areSubsol) AndAlso _style.Separator.IsDrawn Then
                y += Dpi(_style.Separator.Margin)
                _sepTop = y
                y += Dpi(_style.Separator.Width) + Dpi(_style.Separator.Margin)
            End If

            ' ── Corp ─────────────────────────────────────────────────────────
            If areCorp Then
                _bodyLayout = LayoutOf(_content.Text, _bodyFont, _fore, g, maxBody)
                _bodyRect = New Rectangle(padL, y, _bodyLayout.Width, _bodyLayout.Height)
                latimeMax = Math.Max(latimeMax, _bodyLayout.Width)
                y += _bodyLayout.Height
            End If

            ' ── Linia de deasupra subsolului ─────────────────────────────────
            If areSubsol AndAlso (areCorp OrElse areAntet) AndAlso _style.Separator.IsDrawn Then
                y += Dpi(_style.Separator.Margin)
                _sepBottom = y
                y += Dpi(_style.Separator.Width) + Dpi(_style.Separator.Margin)
            End If

            ' ── Subsol ───────────────────────────────────────────────────────
            If areSubsol Then
                Dim iconW As Integer = 0
                Dim iconH As Integer = 0
                If subsolIcon IsNot Nothing Then
                    iconW = Dpi(_style.Footer.IconSize.Width)
                    iconH = Dpi(_style.Footer.IconSize.Height)
                End If
                Dim gap As Integer = If(subsolIcon IsNot Nothing AndAlso Not String.IsNullOrEmpty(subsolText),
                                        Dpi(_style.Footer.IconGap), 0)
                Dim latText As Integer = Math.Max(16, maxBody - iconW - gap)
                _footerLayout = LayoutOf(subsolText, _footerFont, _footerFore, g, latText)

                Dim bandaPadT As Integer = Dpi(_style.Footer.Padding.Top)
                Dim bandaPadB As Integer = Dpi(_style.Footer.Padding.Bottom)
                Dim hSubsol As Integer = Math.Max(_footerLayout.Height, iconH) + bandaPadT + bandaPadB
                latimeMax = Math.Max(latimeMax, iconW + gap + _footerLayout.Width +
                                                Dpi(_style.Footer.Padding.Left) + Dpi(_style.Footer.Padding.Right))

                _footerRect = New Rectangle(padL, y, 0, hSubsol)
                If subsolIcon IsNot Nothing Then
                    _footerIconRect = New Rectangle(padL + Dpi(_style.Footer.Padding.Left),
                                                    y + bandaPadT + Math.Max(0, (Math.Max(_footerLayout.Height, iconH) - iconH) \ 2),
                                                    iconW, iconH)
                End If
                y += hSubsol
            End If
        End Using

        If latimeMax <= 0 Then Return Size.Empty

        ' Lățimea finală o știm abia acum: benzile se întind pe toată eticheta, ca fundalul lor
        ' propriu (dacă îl au) să fie o BANDĂ, nu un dreptunghi cât textul.
        Dim latimeTotala As Integer = latimeMax + padL + padR
        Dim interior As Integer = latimeTotala - padL - padR
        If _headerRect.Height > 0 Then _headerRect.Width = interior
        If _footerRect.Height > 0 Then _footerRect.Width = interior
        If _bodyRect.Height > 0 Then _bodyRect.Width = interior

        Return New Size(latimeTotala, y + padB)
    End Function

    Private Function LayoutOf(text As String, f As Font, culoare As Color,
                              g As Graphics, maxWidth As Integer) As KBotRichText.RichLayout
        If String.IsNullOrEmpty(text) Then
            Return New KBotRichText.RichLayout With {.Lines = New List(Of KBotRichText.RichLine), .Width = 0, .Height = 0}
        End If
        Dim runs As List(Of KBotRichText.RichRun) = KBotRichText.Parse(text, f, culoare)
        Return KBotRichText.Layout(runs, g, maxWidth)
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' PICTAREA
    ' ══════════════════════════════════════════════════════════════════════════

    ' Colțurile se taie din FEREASTRĂ, nu doar din desen: altfel în colțuri s-ar vedea
    ' dreptunghiul ei peste formularul de dedesubt.
    Private Sub ApplyRegion()
        Try
            Dim raza As Integer = Dpi(_style.CornerRadius)
            If raza <= 0 OrElse ClientSize.Width <= 0 OrElse ClientSize.Height <= 0 Then
                Region = Nothing
                Return
            End If
            Using path As GraphicsPath = ThemeShapes.RoundedRect(
                    New Rectangle(0, 0, ClientSize.Width, ClientSize.Height), raza)
                Region = New Region(path)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTipWindow.ApplyRegion", ex)
        End Try
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            If _style Is Nothing OrElse _content Is Nothing Then Return
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit

            DrawChrome(g)
            DrawBand(g, _style.Header, _headerRect, _headerIconRect,
                     _content.EffectiveHeaderIcon(_style), _headerLayout)
            DrawSeparator(g, _sepTop)
            If _bodyRect.Height > 0 Then
                KBotRichText.Draw(g, _bodyLayout, _bodyRect, ContentAlignment.TopLeft)
            End If
            DrawSeparator(g, _sepBottom)
            DrawBand(g, _style.Footer, _footerRect, _footerIconRect,
                     _content.EffectiveFooterIcon(_style), _footerLayout)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotToolTipWindow.OnPaint", ex)
        End Try
    End Sub

    Private Sub DrawChrome(g As Graphics)
        Dim grosime As Integer = Dpi(_style.BorderWidth)
        Dim raza As Integer = Dpi(_style.CornerRadius)
        Dim r As New Rectangle(0, 0, Math.Max(1, ClientSize.Width - 1), Math.Max(1, ClientSize.Height - 1))
        Using path As GraphicsPath = ThemeShapes.RoundedRect(r, raza)
            Using b As New SolidBrush(_fill)
                g.FillPath(b, path)
            End Using
            If grosime > 0 Then
                Using pen As New Pen(_border, grosime)
                    g.DrawPath(pen, path)
                End Using
            End If
        End Using
    End Sub

    Private Sub DrawBand(g As Graphics, band As KBotToolTipBand, rect As Rectangle,
                         iconRect As Rectangle, icon As Image, layout As KBotRichText.RichLayout)
        If rect.Height <= 0 Then Return

        ' Fundal propriu doar dacă s-a cerut. Transparent = se vede fundalul etichetei, deci nu
        ' se pictează nimic (a picta „transparent" ar șterge fundalul, nu l-ar păstra).
        If band.BackColor <> Color.Transparent AndAlso band.BackColor <> Color.Empty Then
            Using b As New SolidBrush(band.BackColor)
                g.FillRectangle(b, rect)
            End Using
        End If

        If icon IsNot Nothing AndAlso iconRect.Width > 0 AndAlso iconRect.Height > 0 Then
            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            g.DrawImage(icon, iconRect)
        End If

        If layout.Lines Is Nothing OrElse layout.Height <= 0 Then Return
        Dim stanga As Integer = rect.X + Dpi(band.Padding.Left)
        If iconRect.Width > 0 Then stanga = iconRect.Right + Dpi(band.IconGap)
        Dim textRect As New Rectangle(stanga,
                                      rect.Y + Dpi(band.Padding.Top),
                                      Math.Max(0, rect.Right - Dpi(band.Padding.Right) - stanga),
                                      Math.Max(0, rect.Height - Dpi(band.Padding.Top) - Dpi(band.Padding.Bottom)))
        KBotRichText.Draw(g, layout, textRect, band.TextAlign)
    End Sub

    Private Sub DrawSeparator(g As Graphics, y As Integer)
        If y < 0 OrElse Not _style.Separator.IsDrawn Then Return
        Dim inset As Integer = Dpi(_style.Separator.Inset)
        Dim x1 As Integer = Dpi(_style.Padding.Left) + inset
        Dim x2 As Integer = ClientSize.Width - Dpi(_style.Padding.Right) - inset
        If x2 <= x1 Then Return
        Dim grosime As Integer = Math.Max(1, Dpi(_style.Separator.Width))
        Using b As New SolidBrush(_sepFore)
            g.FillRectangle(b, x1, y, x2 - x1, grosime)
        End Using
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _autoHide.Dispose()
            ReleaseFonts()
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
