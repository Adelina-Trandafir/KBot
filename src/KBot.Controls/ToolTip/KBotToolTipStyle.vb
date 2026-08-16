Option Strict On
Imports System.ComponentModel
Imports System.Drawing

''' <summary>
''' STILUL unei etichete plutitoare K-BOT: tot ce se poate alege din grila de proprietăți despre
''' cum ARATĂ eticheta — fundal, text, contur, rotunjire, marginile interioare, lățimea maximă,
''' plus cele trei benzi (antet / corp / subsol) și linia despărțitoare dintre ele.
'''
''' <para><b>De ce un obiect de stil separat de componentă.</b> Cerința e ca două controale de
''' pe ACELAȘI formular să poată avea etichete cu înfățișări diferite. Un singur obiect global
''' de tooltip (cum e <c>System.Windows.Forms.ToolTip</c>) nu poate face asta: proprietățile lui
''' sunt ale componentei, deci ale tuturor controalelor pe care le deservește. Aici stilul e o
''' valoare, nu o componentă: <see cref="KBotToolTip"/> are un stil implicit, iar fiecare control
''' poate primi, prin extender, PROPRIUL lui stil. Cine vrea două înfățișări pune două
''' <see cref="KBotToolTip"/>-uri pe formular, sau un singur tooltip cu stiluri pe control.</para>
'''
''' <para><b>Gol = din temă.</b> Regula casei: <c>Color.Empty</c> / <c>Nothing</c> înseamnă
''' „rezolvă din schema activă la pictare"; orice valoare pusă explicit câștigă și rămâne
''' câștigătoare la o comutare de temă. Perechile ShouldSerialize/Reset există pentru fiecare
''' proprietate care nu poate purta <c>DefaultValue</c> (culori, fonturi, imagini,
''' <see cref="Padding"/>) — fără ele Visual Studio ar îngheța în <c>.Designer.vb</c> valoarea
''' REZOLVATĂ, iar aceea s-ar citi pentru totdeauna ca o alegere a operatorului.</para>
''' </summary>
<TypeConverter(GetType(ExpandableObjectConverter))>
Public NotInheritable Class KBotToolTipStyle

    Private _backColor As Color = Color.Empty
    Private _foreColor As Color = Color.Empty
    Private _borderColor As Color = Color.Empty
    Private _borderWidth As Integer = 1
    Private _cornerRadius As Integer = 6
    Private _font As Font = Nothing
    Private _padding As New Padding(8, 6, 8, 6)
    Private _maxWidth As Integer = 420
    Private ReadOnly _header As New KBotToolTipBand(KBotToolTipBandKind.Header)
    Private ReadOnly _footer As New KBotToolTipBand(KBotToolTipBandKind.Footer)
    Private ReadOnly _separator As New KBotToolTipSeparator()

    ''' <summary>Cine trebuie anunțat că stilul s-a schimbat (ca o etichetă deschisă să se refacă).</summary>
    Friend Property Owner As KBotToolTip

    Public Sub New()
        _header.Owner = Me
        _footer.Owner = Me
        _separator.Owner = Me
    End Sub

    ' Un singur canal de „s-a schimbat ceva": stilul nu știe să deseneze, doar să spună.
    Friend Sub Changed()
        Owner?.OnStyleChanged()
    End Sub

    ''' <summary>Fundalul etichetei. Gol (implicit) = <c>SurfaceAlt</c> din schema activă.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Fundalul etichetei. Gol = culoarea din schema activă.")>
    Public Property BackColor As Color
        Get
            Return _backColor
        End Get
        Set(value As Color)
            _backColor = value
            Changed()
        End Set
    End Property
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColor <> Color.Empty
    End Function
    Public Sub ResetBackColor()
        BackColor = Color.Empty
    End Sub

    ''' <summary>Culoarea textului din CORP. Gol (implicit) = <c>Text</c> din schema activă.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Culoarea textului din corpul etichetei. Gol = culoarea din schema activă.")>
    Public Property ForeColor As Color
        Get
            Return _foreColor
        End Get
        Set(value As Color)
            _foreColor = value
            Changed()
        End Set
    End Property
    Public Function ShouldSerializeForeColor() As Boolean
        Return _foreColor <> Color.Empty
    End Function
    Public Sub ResetForeColor()
        ForeColor = Color.Empty
    End Sub

    ''' <summary>
    ''' Culoarea conturului. Gol (implicit) = <c>Border</c> din schema activă. Conturul e ce
    ''' desprinde eticheta de fereastra de sub ea — fără el, pe o schemă deschisă, eticheta pare
    ''' o pată de aceeași culoare cu formularul.
    ''' </summary>
    <Category("K-BOT Etichetă")>
    <Description("Culoarea conturului etichetei. Gol = culoarea din schema activă.")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
            Changed()
        End Set
    End Property
    Public Function ShouldSerializeBorderColor() As Boolean
        Return _borderColor <> Color.Empty
    End Function
    Public Sub ResetBorderColor()
        BorderColor = Color.Empty
    End Sub

    ''' <summary>Grosimea conturului (px logici). <c>0</c> = fără contur.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Grosimea conturului (px la 96 dpi). 0 = fără contur.")>
    <DefaultValue(1)>
    Public Property BorderWidth As Integer
        Get
            Return _borderWidth
        End Get
        Set(value As Integer)
            _borderWidth = Math.Max(0, value)
            Changed()
        End Set
    End Property

    ''' <summary>Raza colțurilor rotunjite (px logici). <c>0</c> = colțuri drepte.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Raza colțurilor (px la 96 dpi). 0 = colțuri drepte.")>
    <DefaultValue(6)>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            _cornerRadius = Math.Max(0, value)
            Changed()
        End Set
    End Property

    ''' <summary>
    ''' Fontul CORPULUI. <c>Nothing</c> (implicit) = fontul controlului peste care stă cursorul —
    ''' o etichetă care scrie cu alte litere decât ecranul de sub ea se citește ca altă aplicație.
    ''' </summary>
    <Category("K-BOT Etichetă")>
    <Description("Fontul corpului. Nesetat = fontul controlului care a cerut eticheta.")>
    Public Property Font As Font
        Get
            Return _font
        End Get
        Set(value As Font)
            _font = value
            Changed()
        End Set
    End Property
    Public Function ShouldSerializeFont() As Boolean
        Return _font IsNot Nothing
    End Function
    Public Sub ResetFont()
        Font = Nothing
    End Sub

    ''' <summary>Marginile interioare ale etichetei (px logici).</summary>
    <Category("K-BOT Etichetă")>
    <Description("Marginile interioare ale etichetei (px la 96 dpi).")>
    Public Property Padding As Padding
        Get
            Return _padding
        End Get
        Set(value As Padding)
            _padding = value
            Changed()
        End Set
    End Property
    Public Function ShouldSerializePadding() As Boolean
        Return _padding <> New Padding(8, 6, 8, 6)
    End Function
    Public Sub ResetPadding()
        Padding = New Padding(8, 6, 8, 6)
    End Sub

    ''' <summary>
    ''' Lățimea maximă (px logici) la care se rupe textul. Nu e o tăiere: textul mai lung coboară
    ''' pe rândul următor. O etichetă există ca să spună ceva întreg.
    ''' </summary>
    <Category("K-BOT Etichetă")>
    <Description("Lățimea maximă (px la 96 dpi); peste ea, textul se rupe pe rânduri. Implicit 420.")>
    <DefaultValue(420)>
    Public Property MaxWidth As Integer
        Get
            Return _maxWidth
        End Get
        Set(value As Integer)
            _maxWidth = Math.Max(80, value)
            Changed()
        End Set
    End Property

    ''' <summary>Banda de ANTET: pictogramă la stânga + titlu (font, aliniere, fundal propriu).</summary>
    <Category("K-BOT Etichetă")>
    <Description("Banda de antet: pictogramă la stânga, titlu, font, aliniere, fundal.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Header As KBotToolTipBand
        Get
            Return _header
        End Get
    End Property

    ''' <summary>Banda de SUBSOL: aceeași structură ca antetul, jos.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Banda de subsol: pictogramă la stânga, text, font, aliniere, fundal.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Footer As KBotToolTipBand
        Get
            Return _footer
        End Get
    End Property

    ''' <summary>Linia despărțitoare dintre benzile VIZIBILE (culoare + grosime).</summary>
    <Category("K-BOT Etichetă")>
    <Description("Linia despărțitoare dintre secțiunile vizibile ale etichetei.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Separator As KBotToolTipSeparator
        Get
            Return _separator
        End Get
    End Property

    ''' <summary>
    ''' Copie a stilului — folosită când un control cere „ca implicitul, dar cu o schimbare".
    ''' Fonturile și imaginile se împart (sunt imuabile din punctul nostru de vedere).
    ''' </summary>
    Public Function Clone() As KBotToolTipStyle
        Dim c As New KBotToolTipStyle()
        c._backColor = _backColor
        c._foreColor = _foreColor
        c._borderColor = _borderColor
        c._borderWidth = _borderWidth
        c._cornerRadius = _cornerRadius
        c._font = _font
        c._padding = _padding
        c._maxWidth = _maxWidth
        _header.CopyTo(c._header)
        _footer.CopyTo(c._footer)
        _separator.CopyTo(c._separator)
        Return c
    End Function

    Public Overrides Function ToString() As String
        Return "Stil etichetă"
    End Function

End Class

''' <summary>Care dintre cele două benzi de capăt e aceasta — antetul sau subsolul.</summary>
Public Enum KBotToolTipBandKind
    ''' <summary>Banda de sus.</summary>
    Header = 0
    ''' <summary>Banda de jos.</summary>
    Footer = 1
End Enum

''' <summary>
''' O BANDĂ DE CAPĂT a etichetei (antet sau subsol): pictogramă la stânga, text, font, aliniere,
''' culoare de text și fundal propriu.
'''
''' <para><b>Fundalul e transparent implicit</b>, adică banda se pictează pe fundalul etichetei,
''' nu pe unul al ei. Așa arată ca o secțiune a aceleiași etichete; o bandă cu fundal propriu se
''' cere explicit, când chiar trebuie să se desprindă (un antet de avertizare, de exemplu).</para>
'''
''' <para><b>Banda se vede doar dacă are ce arăta</b> — text sau pictogramă. <c>Visible = False</c>
''' o stinge chiar și atunci; nu există bandă goală care să mănânce înălțime degeaba.</para>
''' </summary>
<TypeConverter(GetType(ExpandableObjectConverter))>
Public NotInheritable Class KBotToolTipBand

    Private ReadOnly _kind As KBotToolTipBandKind
    Private _visible As Boolean = True
    Private _text As String = String.Empty
    Private _font As Font = Nothing
    Private _foreColor As Color = Color.Empty
    Private _backColor As Color = Color.Transparent
    Private _textAlign As ContentAlignment = ContentAlignment.MiddleLeft
    Private _icon As Image = Nothing
    Private _iconSize As New Size(16, 16)
    Private _iconGap As Integer = 6
    Private _padding As New Padding(0, 2, 0, 2)

    Friend Property Owner As KBotToolTipStyle

    Friend Sub New(kind As KBotToolTipBandKind)
        _kind = kind
        ' Antetul e, prin obicei, mai apăsat decât corpul: aliniat la stânga, îngroșat prin font
        ' propriu doar dacă operatorul îl cere. Subsolul, dimpotrivă, e o notă: culoarea lui
        ' implicită se rezolvă în TextDim la pictare (vezi KBotToolTipWindow).
    End Sub

    ''' <summary>Antet sau subsol — decide culoarea implicită la pictare (subsolul e mai stins).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Kind As KBotToolTipBandKind
        Get
            Return _kind
        End Get
    End Property

    ''' <summary>Banda apare deloc? Implicit True — dar tot n-apare dacă e goală.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Banda se afișează (dacă are text sau pictogramă).")>
    <DefaultValue(True)>
    Public Property Visible As Boolean
        Get
            Return _visible
        End Get
        Set(value As Boolean)
            _visible = value
            Owner?.Changed()
        End Set
    End Property

    ''' <summary>
    ''' Textul benzii. Acceptă aceleași marcaje ca și corpul (<c>&lt;b&gt;</c>, <c>&lt;color=#…&gt;</c>…).
    ''' Se poate suprascrie pe control, prin extender-ul <see cref="KBotToolTip"/>.
    ''' </summary>
    <Category("K-BOT Etichetă")>
    <Description("Textul benzii (acceptă marcajele <b>, <i>, <u>, <color=#…>, <back=#…>).")>
    <DefaultValue("")>
    Public Property Text As String
        Get
            Return _text
        End Get
        Set(value As String)
            _text = If(value, String.Empty)
            Owner?.Changed()
        End Set
    End Property

    ''' <summary>Fontul benzii. <c>Nothing</c> = fontul corpului (antetul: îngroșat).</summary>
    <Category("K-BOT Etichetă")>
    <Description("Fontul benzii. Nesetat = fontul corpului (antetul se îngroașă automat).")>
    Public Property Font As Font
        Get
            Return _font
        End Get
        Set(value As Font)
            _font = value
            Owner?.Changed()
        End Set
    End Property
    Public Function ShouldSerializeFont() As Boolean
        Return _font IsNot Nothing
    End Function
    Public Sub ResetFont()
        Font = Nothing
    End Sub

    ''' <summary>Culoarea textului. Gol = din temă (antet: <c>Text</c>, subsol: <c>TextDim</c>).</summary>
    <Category("K-BOT Etichetă")>
    <Description("Culoarea textului benzii. Gol = culoarea din schema activă.")>
    Public Property ForeColor As Color
        Get
            Return _foreColor
        End Get
        Set(value As Color)
            _foreColor = value
            Owner?.Changed()
        End Set
    End Property
    Public Function ShouldSerializeForeColor() As Boolean
        Return _foreColor <> Color.Empty
    End Function
    Public Sub ResetForeColor()
        ForeColor = Color.Empty
    End Sub

    ''' <summary>
    ''' Fundalul benzii. <b>Implicit transparent</b> = se vede fundalul etichetei.
    ''' </summary>
    <Category("K-BOT Etichetă")>
    <Description("Fundalul benzii. Transparent (implicit) = fundalul etichetei.")>
    Public Property BackColor As Color
        Get
            Return _backColor
        End Get
        Set(value As Color)
            _backColor = value
            Owner?.Changed()
        End Set
    End Property
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColor <> Color.Transparent
    End Function
    Public Sub ResetBackColor()
        BackColor = Color.Transparent
    End Sub

    ''' <summary>Alinierea textului în bandă.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Alinierea textului în bandă.")>
    <DefaultValue(ContentAlignment.MiddleLeft)>
    Public Property TextAlign As ContentAlignment
        Get
            Return _textAlign
        End Get
        Set(value As ContentAlignment)
            _textAlign = value
            Owner?.Changed()
        End Set
    End Property

    ''' <summary>
    ''' Pictograma din STÂNGA benzii. <c>Nothing</c> = fără pictogramă; se poate suprascrie pe
    ''' control, prin extender.
    ''' </summary>
    <Category("K-BOT Etichetă")>
    <Description("Pictograma din stânga benzii. Nesetată = fără pictogramă.")>
    Public Property Icon As Image
        Get
            Return _icon
        End Get
        Set(value As Image)
            _icon = value
            Owner?.Changed()
        End Set
    End Property
    Public Function ShouldSerializeIcon() As Boolean
        Return _icon IsNot Nothing
    End Function
    Public Sub ResetIcon()
        Icon = Nothing
    End Sub

    ''' <summary>Mărimea la care se desenează pictograma (px logici).</summary>
    <Category("K-BOT Etichetă")>
    <Description("Mărimea pictogramei (px la 96 dpi).")>
    Public Property IconSize As Size
        Get
            Return _iconSize
        End Get
        Set(value As Size)
            _iconSize = value
            Owner?.Changed()
        End Set
    End Property
    Public Function ShouldSerializeIconSize() As Boolean
        Return _iconSize <> New Size(16, 16)
    End Function
    Public Sub ResetIconSize()
        IconSize = New Size(16, 16)
    End Sub

    ''' <summary>Spațiul dintre pictogramă și text (px logici).</summary>
    <Category("K-BOT Etichetă")>
    <Description("Spațiul dintre pictogramă și text (px la 96 dpi).")>
    <DefaultValue(6)>
    Public Property IconGap As Integer
        Get
            Return _iconGap
        End Get
        Set(value As Integer)
            _iconGap = Math.Max(0, value)
            Owner?.Changed()
        End Set
    End Property

    ''' <summary>Marginile interioare ale benzii (px logici), peste cele ale etichetei.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Marginile interioare ale benzii (px la 96 dpi).")>
    Public Property Padding As Padding
        Get
            Return _padding
        End Get
        Set(value As Padding)
            _padding = value
            Owner?.Changed()
        End Set
    End Property
    Public Function ShouldSerializePadding() As Boolean
        Return _padding <> New Padding(0, 2, 0, 2)
    End Function
    Public Sub ResetPadding()
        Padding = New Padding(0, 2, 0, 2)
    End Sub

    ''' <summary>Are ce arăta? (text sau pictogramă, și nestinsă).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HasContent As Boolean
        Get
            Return _visible AndAlso (Not String.IsNullOrEmpty(_text) OrElse _icon IsNot Nothing)
        End Get
    End Property

    ''' <summary>Copiază valorile în <paramref name="target"/> (folosit de <c>Clone</c>).</summary>
    Friend Sub CopyTo(target As KBotToolTipBand)
        If target Is Nothing Then Return
        target._visible = _visible
        target._text = _text
        target._font = _font
        target._foreColor = _foreColor
        target._backColor = _backColor
        target._textAlign = _textAlign
        target._icon = _icon
        target._iconSize = _iconSize
        target._iconGap = _iconGap
        target._padding = _padding
    End Sub

    Public Overrides Function ToString() As String
        If Not _visible Then Return "Stinsă"
        Return If(String.IsNullOrEmpty(_text), "(fără text)", _text)
    End Function

End Class

''' <summary>
''' LINIA DESPĂRȚITOARE dintre secțiunile vizibile ale etichetei: culoare + grosime + cât de mult
''' se retrage de la marginile etichetei.
'''
''' <para>Se desenează DOAR între două secțiuni care se văd amândouă. O linie sub un antet care
''' nu există ar fi o linie deasupra corpului, adică un chenar rupt.</para>
''' </summary>
<TypeConverter(GetType(ExpandableObjectConverter))>
Public NotInheritable Class KBotToolTipSeparator

    Private _visible As Boolean = True
    Private _foreColor As Color = Color.Empty
    Private _width As Integer = 1
    Private _inset As Integer = 0
    Private _margin As Integer = 4

    Friend Property Owner As KBotToolTipStyle

    ''' <summary>Se desenează linia? Implicit True.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Se desenează linia despărțitoare dintre secțiunile vizibile.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean
        Get
            Return _visible
        End Get
        Set(value As Boolean)
            _visible = value
            Owner?.Changed()
        End Set
    End Property

    ''' <summary>Culoarea liniei. Gol (implicit) = <c>Border</c> din schema activă.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Culoarea liniei despărțitoare. Gol = culoarea din schema activă.")>
    Public Property ForeColor As Color
        Get
            Return _foreColor
        End Get
        Set(value As Color)
            _foreColor = value
            Owner?.Changed()
        End Set
    End Property
    Public Function ShouldSerializeForeColor() As Boolean
        Return _foreColor <> Color.Empty
    End Function
    Public Sub ResetForeColor()
        ForeColor = Color.Empty
    End Sub

    ''' <summary>Grosimea liniei (px logici). <c>0</c> = fără linie, ca și <c>Visible = False</c>.</summary>
    <Category("K-BOT Etichetă")>
    <Description("Grosimea liniei despărțitoare (px la 96 dpi). 0 = fără linie.")>
    <DefaultValue(1)>
    Public Property Width As Integer
        Get
            Return _width
        End Get
        Set(value As Integer)
            _width = Math.Max(0, value)
            Owner?.Changed()
        End Set
    End Property

    ''' <summary>Cât se retrage linia de la marginile interioare, pe fiecare capăt (px logici).</summary>
    <Category("K-BOT Etichetă")>
    <Description("Retragerea liniei față de marginile interioare, pe fiecare capăt (px la 96 dpi).")>
    <DefaultValue(0)>
    Public Property Inset As Integer
        Get
            Return _inset
        End Get
        Set(value As Integer)
            _inset = Math.Max(0, value)
            Owner?.Changed()
        End Set
    End Property

    ''' <summary>Spațiul liber deasupra și dedesubtul liniei (px logici).</summary>
    <Category("K-BOT Etichetă")>
    <Description("Spațiul liber deasupra și dedesubtul liniei (px la 96 dpi).")>
    <DefaultValue(4)>
    Public Property Margin As Integer
        Get
            Return _margin
        End Get
        Set(value As Integer)
            _margin = Math.Max(0, value)
            Owner?.Changed()
        End Set
    End Property

    ''' <summary>Chiar se desenează ceva? (vizibilă ȘI cu grosime).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsDrawn As Boolean
        Get
            Return _visible AndAlso _width > 0
        End Get
    End Property

    ''' <summary>Copiază valorile în <paramref name="target"/> (folosit de <c>Clone</c>).</summary>
    Friend Sub CopyTo(target As KBotToolTipSeparator)
        If target Is Nothing Then Return
        target._visible = _visible
        target._foreColor = _foreColor
        target._width = _width
        target._inset = _inset
        target._margin = _margin
    End Sub

    Public Overrides Function ToString() As String
        Return If(IsDrawn, $"{_width} px", "Fără linie")
    End Function

End Class
