Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' TITLUL DIN SUBSOL al <see cref="KBotDataView"/> (slice 0028-02): un text și o pictogramă în
''' partea stângă a benzii de subsol, acolo unde altfel n-ar scrie nimic. Vocabularul e cel al
''' subsolului de arbore (<c>FooterCaption</c> / <c>FooterLeftIcon</c> / <c>FooterIconSize</c>),
''' ca operatorul să nu învețe două contracte.
'''
''' <para><b>Cât de departe se întinde.</b> Zona ține de la marginea stângă a benzii până la
''' PRIMA coloană agregată — nu până la marginea din dreapta. Restul benzii aparține totalurilor,
''' iar un titlu care ar curge pe sub o sumă ar arăta ca eticheta acelei sume, adică ar spune
''' altceva decât spune. Fără nicio coloană agregată, zona e toată banda.</para>
'''
''' <para><b>Colțul butonului de strângere rămâne al lui</b> — zona pornește din
''' <c>FooterContentRect</c>, care i l-a scăzut deja. Așa titlul se mută singur când butonul e
''' pus pe stânga, în loc să se așeze sub el.</para>
'''
''' <para><b>Pictograma</b> se desenează doar dacă încape ÎNTREAGĂ în zonă, și tot atunci se și
''' apasă: aceeași regulă ca la pictogramele de antet — ce nu se vede, nu se poate apăsa.</para>
''' </summary>
Partial Class KBotDataView

    ' ── Stare ───────────────────────────────────────────────────────────────────
    Private _footerCaption As String = String.Empty
    Private _footerLeftIcon As Image = Nothing
    Private _footerIconSize As Size = New Size(16, 16)
    Private _footerLeftIconHoverColor As Color = Color.Empty
    Private _footerIconHover As Boolean = False

    ''' <summary>Pictograma din stânga subsolului a fost apăsată.</summary>
    Public Event FooterLeftIconClicked As EventHandler

    ' ══════════════════════════════════════════════════════════════════════════
    ' PROPRIETĂȚI
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Textul scris în stânga benzii de subsol, până la prima coloană agregată. Se vede doar
    ''' când <see cref="FooterVisible"/> e True.
    ''' </summary>
    <Category("K-BOT: Footer")>
    <Description("Textul din stânga subsolului, până la prima coloană agregată. Are nevoie de FooterVisible.")>
    <DefaultValue("")>
    Public Property FooterCaption As String
        Get
            Return _footerCaption
        End Get
        Set(value As String)
            Dim nou As String = If(value, String.Empty)
            If String.Equals(_footerCaption, nou, StringComparison.Ordinal) Then Return
            _footerCaption = nou
            Invalidate()
        End Set
    End Property

    ''' <summary>Pictograma dinaintea titlului din subsol. Nesetată = doar text.</summary>
    <Category("K-BOT: Footer")>
    <Description("Pictograma dinaintea titlului din subsol. Apăsarea ei ridică FooterLeftIconClicked.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property FooterLeftIcon As Image
        Get
            Return _footerLeftIcon
        End Get
        Set(value As Image)
            If value Is _footerLeftIcon Then Return
            _footerLeftIcon = value
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeFooterLeftIcon() As Boolean
        Return _footerLeftIcon IsNot Nothing
    End Function

    Private Sub ResetFooterLeftIcon()
        FooterLeftIcon = Nothing
    End Sub

    ''' <summary>
    ''' Mărimea (px la 96 dpi) a pictogramei din subsol. Implicit 16×16. Valoare LOGICĂ: scalarea
    ''' la DPI se face la folosire, prin <see cref="FooterIconSizePx"/> (vezi <c>KBotDataView.Dpi.vb</c>).
    ''' </summary>
    <Category("K-BOT: Footer")>
    <Description("Mărimea (px @96dpi) a pictogramei din subsol.")>
    Public Property FooterIconSize As Size
        Get
            Return _footerIconSize
        End Get
        Set(value As Size)
            Dim nou As New Size(Math.Max(1, value.Width), Math.Max(1, value.Height))
            If _footerIconSize = nou Then Return
            _footerIconSize = nou
            Invalidate()
        End Set
    End Property

    ' Size nu poate purta <DefaultValue> (atributul cere o constantă), deci perechea
    ' ShouldSerialize/Reset e singurul mod în care designerul află că 16×16 e „nesetat”.
    Private Function ShouldSerializeFooterIconSize() As Boolean
        Return _footerIconSize <> New Size(16, 16)
    End Function

    Private Sub ResetFooterIconSize()
        FooterIconSize = New Size(16, 16)
    End Sub

    ''' <summary>
    ''' Culoarea de sub pictograma din subsol cât timp cursorul e peste ea. <c>Color.Empty</c>
    ''' (implicit) = o spălare din culoarea de text a subsolului, adică din temă.
    ''' </summary>
    <Category("K-BOT: Footer")>
    <Description("Culoarea de hover a pictogramei din subsol. Gol = o spălare din culoarea temei.")>
    Public Property FooterLeftIconHoverColor As Color
        Get
            Return _footerLeftIconHoverColor
        End Get
        Set(value As Color)
            If _footerLeftIconHoverColor = value Then Return
            _footerLeftIconHoverColor = value
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeFooterLeftIconHoverColor() As Boolean
        Return _footerLeftIconHoverColor <> Color.Empty
    End Function

    Private Sub ResetFooterLeftIconHoverColor()
        FooterLeftIconHoverColor = Color.Empty
    End Sub

    ''' <summary>Culoarea de hover rezolvată (fixată de operator sau spălarea din temă).</summary>
    Friend Function FooterIconHoverResolved() As Color
        If _footerLeftIconHoverColor <> Color.Empty Then Return _footerLeftIconHoverColor
        Return Color.FromArgb(40, FooterForeResolved())
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' GEOMETRIE
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' X-ul (client) al primei coloane AGREGATE, sau <see cref="Integer.MaxValue"/> dacă nu
    ''' există niciuna. Ordinea vizuală: banda înghețată întâi, apoi cea derulată.
    ''' </summary>
    Private Function FirstAggregatedColumnLeft() As Integer
        For Each cl In _frozenLayout
            If cl.Column.Aggregate <> KBotAggregate.None Then Return cl.X
        Next
        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            If cl.Column.Aggregate <> KBotAggregate.None Then Return _frozenBandWidth + cl.X - hOffset
        Next
        Return Integer.MaxValue
    End Function

    ''' <summary>
    ''' Zona în care are voie să scrie titlul: din conținutul benzii (care a scăzut deja colțul
    ''' butonului) până la prima coloană agregată. Lățime 0 = nu mai e loc, deci nu se scrie nimic.
    ''' </summary>
    Friend Function FooterCaptionZone(bandRect As Rectangle) As Rectangle
        Dim continut As Rectangle = FooterContentRect(bandRect)
        Dim capat As Integer = Math.Min(continut.Right, FirstAggregatedColumnLeft())
        Return New Rectangle(continut.Left, continut.Top,
                             Math.Max(0, capat - continut.Left), continut.Height)
    End Function

    ''' <summary>
    ''' Mărimea pictogramei din subsol în px de ECRAN — perechea scalată a lui
    ''' <see cref="FooterIconSize"/>. Era singura mărime de pictogramă a grilei rămasă nescalată:
    ''' la 150% pictograma stătea mică într-o bandă crescută, lângă un text care se mărise singur.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend ReadOnly Property FooterIconSizePx As Size
        Get
            Return New Size(ScaleDpi(_footerIconSize.Width), ScaleDpi(_footerIconSize.Height))
        End Get
    End Property

    ' Dreptunghiul pictogramei într-o zonă dată (gol = nu e pictogramă sau nu încape întreagă).
    Private Function ComputeFooterIconRect(zone As Rectangle) As Rectangle
        If _footerLeftIcon Is Nothing OrElse zone.Height <= 0 Then Return Rectangle.Empty
        Dim pad As Integer = ScaleDpi(KBotDataColumn.HeaderIconPad)
        Dim s As Size = FooterIconSizePx
        If zone.Width < pad + s.Width Then Return Rectangle.Empty
        Return New Rectangle(zone.Left + pad,
                             zone.Top + (zone.Height - s.Height) \ 2,
                             s.Width, s.Height)
    End Function

    ''' <summary>
    ''' Dreptunghiul curent al pictogramei din subsol (gol = nu se vede). Îl folosesc pictarea,
    ''' hit-testul și testele — o a doua formulă ar despărți desenul de apăsare.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property FooterLeftIconRect As Rectangle
        Get
            If Not _showFooter OrElse FooterBandHeight() <= 0 Then Return Rectangle.Empty
            Dim band As Rectangle = CurrentFooterBandRect()
            Return ComputeFooterIconRect(FooterCaptionZone(band))
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' PICTARE (acoperită tranzitiv de Try-ul din OnPaint)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Pictograma + titlul, în zona din stânga benzii de subsol.</summary>
    Private Sub DrawFooterCaption(g As Graphics, bandRect As Rectangle, tf As Font)
        Dim zone As Rectangle = FooterCaptionZone(bandRect)
        If zone.Width <= 0 Then Return

        Dim pad As Integer = ScaleDpi(KBotDataColumn.HeaderIconPad)
        Dim stanga As Integer = zone.Left + pad

        Dim icon As Rectangle = ComputeFooterIconRect(zone)
        If Not icon.IsEmpty Then
            If _footerIconHover Then
                Using b As New SolidBrush(FooterIconHoverResolved())
                    Using path As GraphicsPath = RoundedRect(Rectangle.Inflate(icon, 3, 3), ScaleDpi(3))
                        g.FillPath(b, path)
                    End Using
                End Using
            End If
            g.DrawImage(_footerLeftIcon, icon)
            stanga = icon.Right + ScaleDpi(KBotDataColumn.HeaderIconGap)
        End If

        If String.IsNullOrEmpty(_footerCaption) Then Return
        Dim textRect As New Rectangle(stanga, zone.Top,
                                      Math.Max(0, zone.Right - pad - stanga), zone.Height)
        If textRect.Width <= 0 Then Return
        TextRenderer.DrawText(g, _footerCaption, tf, textRect, FooterForeResolved(),
            TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' MOUSE (chemate din partiala .Collapse, care deține banda de subsol)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Apăsare peste pictograma din subsol. True = a fost consumată.</summary>
    Friend Function HandleFooterIconMouseDown(location As Point) As Boolean
        Dim icon As Rectangle = FooterLeftIconRect
        If icon.IsEmpty OrElse Not icon.Contains(location) Then Return False
        If Not KBotDesignTime.IsDesignTime(Me) Then
            RaiseEvent FooterLeftIconClicked(Me, EventArgs.Empty)
        End If
        Return True
    End Function

    ''' <summary>Hover peste pictograma din subsol. True = cursorul e chiar peste ea.</summary>
    Friend Function UpdateFooterIconHover(location As Point) As Boolean
        Dim icon As Rectangle = FooterLeftIconRect
        Dim hover As Boolean = Not icon.IsEmpty AndAlso icon.Contains(location)
        If hover <> _footerIconHover Then
            _footerIconHover = hover
            Invalidate()
        End If
        Return hover
    End Function

    ''' <summary>Cursorul e peste pictograma din subsol? O citește partiala .Input, pentru cursor.</summary>
    Friend ReadOnly Property FooterIconHovered As Boolean
        Get
            Return _footerIconHover
        End Get
    End Property

    ''' <summary>Stinge hover-ul pictogramei din subsol.</summary>
    Friend Sub ClearFooterIconHover()
        If Not _footerIconHover Then Return
        _footerIconHover = False
        Invalidate()
    End Sub

End Class
