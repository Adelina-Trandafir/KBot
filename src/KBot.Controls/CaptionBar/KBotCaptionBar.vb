Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Bară de titlu proprie pentru formularele fără chenar (FormBorderStyle.None):
''' pictogramă + titlu în stânga, buton de închidere (și, opțional, minimizare) în
''' dreapta, tragerea ferestrei de pe zona liberă. Toate culorile vin din schema
''' activă (via <see cref="ApplyTheme"/>); nicio culoare hardcodată.
'''
''' <para>Partea de SELECTOR DE TEMĂ (butonul <c>ShowThemeButton</c> și meniul lui) stă în
''' <c>KBotCaptionBar.ThemeButton.vb</c> — vezi acolo de ce bara își face singură meniul.</para>
''' </summary>
<ToolboxItem(True)>
Partial Public NotInheritable Class KBotCaptionBar
    Inherits Control
    Implements IThemedControl, IPopupAnchor

    ' ── Culori derivate din paletă (setate în ApplyTheme) ─────────────────────
    Private _backColor As Color = SystemColors.Control
    Private _titleColor As Color = SystemColors.ControlText
    Private _glyphColor As Color = SystemColors.ControlText
    Private _closeHoverColor As Color = Color.FromArgb(196, 43, 28)
    Private _btnHoverColor As Color = SystemColors.ControlLight
    Private _optBtnHoverColor As Color = SystemColors.ControlLight

    ' ── Stare ─────────────────────────────────────────────────────────────────
    Private _iconImage As Image
    Private _showMinimize As Boolean = False
    Private _showMaximize As Boolean = False
    Private _hoverClose As Boolean = False
    Private _hoverMin As Boolean = False
    Private _hoverMax As Boolean = False
    Private _optionButtonHover As Boolean = False
    Private _themeButtonHover As Boolean = False

    ' ── Optional - Options Button (in stanga ultimului buton vizibil din control box) ───────────────────────────────
    Private _showOptionsButton As Boolean = False
    Private _optionButtonImage As Image
    Private _optionButtonClick As EventHandler
    Private _optionButtonPadding As Integer = 0
    Private _tintOptionButtonImage As Boolean = True
    Private _optionButtonActive As Boolean = False

    ' ── Optional - Theme Button (selectorul de teme; vezi KBotCaptionBar.ThemeButton.vb) ──────
    Private _showThemeButton As Boolean = False
    Private _showThemeEditor As Boolean = True
    Private _themeButtonImage As Image
    Private _themeButtonPadding As Integer = 2
    Private _tintThemeButtonImage As Boolean = True
    Private _themeButtonActive As Boolean = False

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        Height = 40
    End Sub

    ''' <summary>Pictograma afișată la stânga titlului (opțională).</summary>
    <Category("K-BOT")>
    <Description("Pictograma afișată la stânga titlului. Lăsată goală, titlul începe de la marginea din stânga.")>
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
    <Category("K-BOT")>
    <Description("Arată și butonul de minimizare. Implicit False — un dialog are doar închidere.")>
    <DefaultValue(False)>
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
    <Category("K-BOT")>
    <Description("Arată și butonul de maximizare/restaurare și activează dublu-click pe zona de tragere. Implicit False.")>
    <DefaultValue(False)>
    Public Property ShowMaximize As Boolean
        Get
            Return _showMaximize
        End Get
        Set(value As Boolean)
            _showMaximize = value
            Invalidate()
        End Set
    End Property

    'butonul de optiuni (in stanga ultimului buton vizibil din control box)
    <Category("K-BOT")>
    <Description("Arată și butonul de opțiuni (în stânga")>
    <DefaultValue(False)>
    Public Property ShowOptionsButton As Boolean
        Get
            Return _showOptionsButton
        End Get
        Set(value As Boolean)
            _showOptionsButton = value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Imaginea butonului de opțiuni")>
    Public Property OptionButtonImage As Image
        Get
            Return _optionButtonImage
        End Get
        Set(value As Image)
            _optionButtonImage = value
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Padding-ul pentru imaginea din OptionButton")>
    Public Property OptionButtonPadding As Integer
        Get
            Return _optionButtonPadding
        End Get
        Set(value As Integer)
            _optionButtonPadding = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Pictograma butonului de opțiuni se RECOLOREAZĂ cu culoarea celorlalte trei glife
    ''' (minimizare / maximizare / închidere), deci urmează schema: neagră pe temele deschise,
    ''' albă pe cele întunecate. Implicit True, fiindcă pictograma de acolo e o siluetă
    ''' monocromă — pe schema întunecată, netratată, e o pată neagră pe fundal negru, adică
    ''' invizibilă.
    '''
    ''' Se stinge pentru o pictogramă cu adevărat colorată, pe care recolorarea ar turti-o.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Recolorează pictograma butonului de opțiuni cu culoarea glifelor (deci urmează tema). Stinge-l pentru o pictogramă colorată.")>
    <DefaultValue(True)>
    Public Property TintOptionButtonImage As Boolean
        Get
            Return _tintOptionButtonImage
        End Get
        Set(value As Boolean)
            If value = _tintOptionButtonImage Then Return
            _tintOptionButtonImage = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Butonul de opțiuni rămâne APRINS (fundalul de survolare) cât timp e deschis meniul pe
    ''' care l-a desfășurat. Nu se pune de mână: îl ridică și îl coboară <see cref="CustomPopup"/>
    ''' prin <see cref="IPopupAnchor"/>, pe sinkul prin care trec toate drumurile de închidere.
    ''' Stare pură de rulare — designerul n-o vede și n-o serializează.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property OptionButtonActive As Boolean
        Get
            Return _optionButtonActive
        End Get
    End Property

    ''' <summary>
    ''' Bara are ACUM două butoane care desfășoară meniuri (opțiuni și temă), iar interfața nu
    ''' spune care s-a desfășurat — <see cref="IPopupAnchor"/> primește un singur bit, fiindcă
    ''' popup-ul nu are de unde ști ce buton l-a deschis.
    '''
    ''' Diferența o face bara: meniul de temă îl deschide EA, deci ridică <c>_themeMenuOpening</c>
    ''' chiar înainte de <c>ShowBelow</c> (vezi <c>ShowThemeMenu</c>); orice altă deschidere vine de
    ''' la gazdă, prin butonul de opțiuni. Steagul se consumă aici, la prima aprindere, ca o
    ''' deschidere ratată să nu-l lase ridicat pentru următoarea.
    ''' </summary>
    Private Sub SetPopupOpen(open As Boolean) Implements IPopupAnchor.SetPopupOpen
        If open Then
            Dim eTema As Boolean = _themeMenuOpening
            _themeMenuOpening = False
            If eTema Then
                If _themeButtonActive Then Return
                _themeButtonActive = True
            Else
                If _optionButtonActive Then Return
                _optionButtonActive = True
            End If
        Else
            ' Închiderea stinge amândouă: sinkul e comun, iar un buton rămas aprins ar arăta un
            ' meniu care nu mai există.
            If Not _optionButtonActive AndAlso Not _themeButtonActive Then Return
            _optionButtonActive = False
            _themeButtonActive = False
        End If
        Invalidate()
    End Sub

    ''' <summary>
    ''' Dreptunghiul butonului de opțiuni, în coordonatele CLIENT ale barei —
    ''' <see cref="Rectangle.Empty"/> când butonul e ascuns.
    '''
    ''' Există ca să poată o gazdă să agațe ceva sub buton (un meniu, o listă) fără să-i
    ''' reproducă geometria: butonul e DESENAT, nu e un control, deci n-are `Bounds` propriu, iar
    ''' o gazdă care își calculează singură slotul rămâne în urmă în clipa în care se stinge
    ''' `ShowMinimize` sau `ShowMaximize`. Aceeași lecție ca la butonul de strângere al arborelui:
    ''' desenul, hit-testul și gazda citesc ACEEAȘI funcție.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property OptionButtonBounds As Rectangle
        Get
            If Not _showOptionsButton Then Return Rectangle.Empty
            Return OptionButtonRect()
        End Get
    End Property

    Private Shared ReadOnly newColorMatrix As Single() = New Single() {0.0F, 0.0F, 0.0F, 0.0F, 0.0F}
    Private Shared ReadOnly newColorMatrixArray As Single() = New Single() {0.0F, 0.0F, 0.0F, 0.0F, 0.0F}
    Private Shared ReadOnly newColorMatrixArray0 As Single() = New Single() {0.0F, 0.0F, 0.0F, 0.0F, 0.0F}
    Private Shared ReadOnly newColorMatrixArray1 As Single() = New Single() {0.0F, 0.0F, 0.0F, 1.0F, 0.0F}

    <Category("K-BOT")>
    <Description("Evenimentul declanșat la click pe butonul de opțiuni")>
    Public Custom Event OptionButtonClick As EventHandler
        AddHandler(value As EventHandler)
            _optionButtonClick = DirectCast(System.Delegate.Combine(_optionButtonClick, value), EventHandler)
        End AddHandler
        RemoveHandler(value As EventHandler)
            _optionButtonClick = DirectCast(System.Delegate.Remove(_optionButtonClick, value), EventHandler)
        End RemoveHandler
        RaiseEvent(sender As Object, e As EventArgs)
            _optionButtonClick?.Invoke(sender, e)
        End RaiseEvent
    End Event

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
        _optBtnHoverColor = ThemeShapes.Blend(p.SurfaceAltColor, p.BorderColor, 0.6)

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

    ' Ordinea butoanelor pe bară, dreapta → stânga: închidere, maximizare, minimizare, TEMĂ,
    ' opțiuni. Butonul de temă stă imediat după cutia de control (min/max, iar în lipsa lor după
    ' închidere), deci slotul lui e primul liber după ea; butonul de opțiuni vine la stânga lui.
    ' Toate se derivă din aceeași numărătoare — nimic nu rămâne în urmă când se stinge un buton.
    Private Function ThemeButtonSlot() As Integer
        Dim slotIndex As Integer = 1 'Close button is always in slot 0
        If _showMinimize Then slotIndex += 1
        If _showMaximize Then slotIndex += 1
        Return slotIndex
    End Function

    Private Function ThemeButtonRect() As Rectangle
        Return SlotRect(ThemeButtonSlot())
    End Function

    Private Function OptionButtonRect() As Rectangle
        Return SlotRect(ThemeButtonSlot() + If(_showThemeButton, 1, 0))
    End Function

    ' Limita din dreapta a titlului = marginea stângă a celui mai din stânga buton vizibil.
    ' Butoanele de opțiuni și de temă intră și ele în socoteală: fără asta titlul curge PE SUB
    ' ele, iar un titlu lung le acoperă.
    Private Function TitleRightLimit() As Integer
        If _showOptionsButton Then Return OptionButtonRect().Left
        If _showThemeButton Then Return ThemeButtonRect().Left
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

            ' Buton opțiuni (opțional). Aprins și cât timp meniul lui e deschis, nu doar sub
            ' cursor: meniul trebuie să pară continuarea butonului. Vezi IPopupAnchor.
            If _showOptionsButton Then
                DrawImageButton(g, OptionButtonRect(), _optionButtonImage, _optionButtonPadding,
                                _tintOptionButtonImage, _optionButtonHover OrElse _optionButtonActive)
            End If

            ' Buton temă (opțional) — aceeași față ca butonul de opțiuni, alt conținut.
            If _showThemeButton Then
                DrawImageButton(g, ThemeButtonRect(), EffectiveThemeButtonImage(), _themeButtonPadding,
                                _tintThemeButtonImage, _themeButtonHover OrElse _themeButtonActive)
            End If

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
            ' Fără log din procesul designer-ului (vezi KBotDesignTime).
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotCaptionBar.OnPaint", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Un buton cu pictogramă (opțiuni sau temă): fundalul de survolare, apoi glifa pătrată,
    ''' centrată, micșorată cu <paramref name="padding"/> pe fiecare latură.
    '''
    ''' Cele două butoane citesc ACEEAȘI funcție, ca la sloturi: sunt același obiect vizual, iar
    ''' două desene paralele s-ar despărți la prima reglare de padding.
    '''
    ''' Ajutor chemat DOAR din OnPaint, care e deja înfășurat (regula de acoperire tranzitivă).
    ''' </summary>
    Private Sub DrawImageButton(g As Graphics, bounds As Rectangle, image As Image,
                                padding As Integer, tint As Boolean, hot As Boolean)
        If hot Then
            Using hb As New SolidBrush(_optBtnHoverColor)
                g.FillRectangle(hb, bounds)
            End Using
        End If
        If image Is Nothing Then Return

        Dim pad As Integer = ThemeShapes.ScaleDpi(Me, padding)
        Dim side As Integer = Math.Min(Height - ThemeShapes.ScaleDpi(Me, 14), ThemeShapes.ScaleDpi(Me, 24))
        side = Math.Max(0, side - 2 * pad)          ' shrink by padding on both sides
        If side <= 0 Then Return

        Dim ix As Integer = bounds.Left + (bounds.Width - side) \ 2
        Dim iy As Integer = (Height - side) \ 2
        g.InterpolationMode = InterpolationMode.HighQualityBicubic
        DrawGlyphImage(g, image, New Rectangle(ix, iy, side, side), tint)
    End Sub

    ''' <summary>
    ''' Pictograma unui buton de bară. Cu <paramref name="tint"/> aprins, e desenată
    ''' RECOLORATĂ în <c>_glyphColor</c> — adică exact culoarea liniilor de la minimizare,
    ''' maximizare și închidere, deci butonul devine a patra glifă a barei și urmează schema.
    '''
    ''' Matricea turtește R/G/B la culoarea cerută și lasă ALFA neatins (rândul 4 rămâne 1 pe
    ''' poziția lui): forma pictogramei e dată de canalul alfa, deci silueta și marginile ei
    ''' antialiasate rămân intacte, se schimbă doar culoarea. Pentru o pictogramă cu adevărat
    ''' colorată asta ar fi distructiv — de aceea proprietatea se poate stinge.
    '''
    ''' Ajutor chemat DOAR din OnPaint, care e deja înfășurat (regula de acoperire tranzitivă).
    ''' </summary>
    Private Sub DrawGlyphImage(g As Graphics, image As Image, dest As Rectangle, tint As Boolean)
        If Not tint Then
            g.DrawImage(image, dest)
            Return
        End If
        Using attrs As New ImageAttributes()
            attrs.SetColorMatrix(New ColorMatrix(New Single()() {
                newColorMatrix,
                newColorMatrixArray,
                newColorMatrixArray0,
                newColorMatrixArray1,
                New Single() {_glyphColor.R / 255.0F, _glyphColor.G / 255.0F, _glyphColor.B / 255.0F, 0.0F, 1.0F}}))
            g.DrawImage(image, dest, 0, 0,
                        image.Width, image.Height, GraphicsUnit.Pixel, attrs)
        End Using
    End Sub

    ' True dacă punctul e pe oricare dintre butoanele vizibile.
    Private Function IsOnButton(location As Point) As Boolean
        If CloseRect().Contains(location) Then Return True
        If _showMaximize AndAlso MaxRect().Contains(location) Then Return True
        If _showMinimize AndAlso MinRect().Contains(location) Then Return True
        If _showOptionsButton AndAlso OptionButtonRect().Contains(location) Then Return True
        If _showThemeButton AndAlso ThemeButtonRect().Contains(location) Then Return True
        Return False
    End Function

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            Dim overClose As Boolean = CloseRect().Contains(e.Location)
            Dim overMax As Boolean = _showMaximize AndAlso MaxRect().Contains(e.Location)
            Dim overMin As Boolean = _showMinimize AndAlso MinRect().Contains(e.Location)
            Dim overOpt As Boolean = _showOptionsButton AndAlso OptionButtonRect().Contains(e.Location)
            Dim overTema As Boolean = _showThemeButton AndAlso ThemeButtonRect().Contains(e.Location)

            If overClose <> _hoverClose OrElse overMin <> _hoverMin OrElse overMax <> _hoverMax OrElse
               overOpt <> _optionButtonHover OrElse overTema <> _themeButtonHover Then
                _hoverClose = overClose
                _hoverMin = overMin
                _hoverMax = overMax
                _optionButtonHover = overOpt
                _themeButtonHover = overTema
                Invalidate()
            End If
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotCaptionBar.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hoverClose OrElse _hoverMin OrElse _hoverMax OrElse _optionButtonHover OrElse _themeButtonHover Then
            _hoverClose = False
            _hoverMin = False
            _hoverMax = False
            _optionButtonHover = False
            _themeButtonHover = False
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
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotCaptionBar.OnMouseDown", ex)
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
            ElseIf _showOptionsButton AndAlso OptionButtonRect().Contains(e.Location) Then
                RaiseEvent OptionButtonClick(Me, EventArgs.Empty)
            ElseIf _showThemeButton AndAlso ThemeButtonRect().Contains(e.Location) Then
                ' Meniul de teme îl face bara însăși — vezi KBotCaptionBar.ThemeButton.vb.
                ShowThemeMenu()
            End If
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotCaptionBar.OnMouseClick", ex)
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
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotCaptionBar.OnMouseDoubleClick", ex)
        End Try
    End Sub

End Class
