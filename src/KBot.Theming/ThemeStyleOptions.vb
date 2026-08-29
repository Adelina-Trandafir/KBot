Imports System.Windows.Forms
Imports System.Text.Json.Serialization

''' <summary>Strategia de randare a butoanelor pentru o schemă.</summary>
Public Enum ButtonRenderStyle
    ''' <summary>Buton system (UseVisualStyleBackColor) — nu se pictează nimic.</summary>
    [System] = 0
    ''' <summary>FlatStyle.Flat cu culori din paletă (tema dark actuală).</summary>
    Flat = 1
    ''' <summary>Owner-drawn modern: colțuri rotunjite, hover/pressed pictat.</summary>
    ModernOwnerDrawn = 2
End Enum

''' <summary>
''' „Mai mult decât culori”: flagurile care descriu comportamentul vizual al unei
''' scheme, independent de paletă. Serializabil JSON (editorul viitor le scrie direct).
''' </summary>
Public NotInheritable Class ThemeStyleOptions

    ''' <summary>True (Classic) => sari peste orice pictură custom, deferă la SystemColors.</summary>
    Public Property UseSystemColors As Boolean = True

    ''' <summary>Button/Tab FlatStyle.Flat.</summary>
    Public Property FlatControls As Boolean = False

    ''' <summary>Strategia de randare a butoanelor.</summary>
    Public Property ButtonRender As ButtonRenderStyle = ButtonRenderStyle.System

    ''' <summary>Rază colț în px logici @96dpi; scalată la DPI la pictare. 0 = pătrat.</summary>
    Public Property CornerRadius As Integer = 0

    ''' <summary>Numele fontului de bază (fallback „Segoe UI” dacă lipsește).</summary>
    Public Property BaseFontName As String = "Segoe UI"

    ''' <summary>Dimensiunea fontului de bază (pt). 0 sau negativ => nu schimbă fontul.</summary>
    Public Property BaseFontSize As Single = 0F

    ''' <summary>
    ''' Recolour monochrome icons from the palette instead of drawing them as authored.
    '''
    ''' <para>Off everywhere except Modern, and deliberately so: the K-BOT icon set is not all
    ''' monochrome, and flattening a multi-coloured icon to one palette colour would lose the very
    ''' thing that makes it recognisable. A scheme opts in when its artwork is monochrome.</para>
    ''' </summary>
    Public Property TintIcons As Boolean = False

    ' ── Carduri si geometrie de rand (felia 0049) ─────────────────────────────
    ' Every one of these is 0 / "no opinion" by default, which is exactly today's output:
    ' a scheme written before this slice deserializes into the neutral behaviour.

    ''' <summary>
    ''' Raza colturilor unui card, in px logici @96dpi. 0 = card dreptunghiular, si atunci
    ''' pictura ia calea PLATA: nicio cale rotunjita, niciun antialias, niciun colt umplut.
    ''' Distincta de <see cref="CornerRadius"/>, care e raza CONTROALELOR (butoane, inputuri,
    ''' randuri) — un card vrea un colt mult mai generos decat un buton.
    ''' </summary>
    Public Property CardRadius As Integer = 0

    ''' <summary>
    ''' Cat de departe cade umbra unui card, in px logici. 0 = fara umbra, si atunci nu se
    ''' aloca si nu se deseneaza absolut nimic — pasul de umbra e sarit din radacina.
    ''' </summary>
    Public Property CardShadow As Integer = 0

    ''' <summary>
    ''' Cat de apasata e umbra, 0..100 (procent de opacitate la marginea cardului, de unde
    ''' scade spre exterior). Traieste separat de <c>Palette.Shadow</c> fiindca paleta se
    ''' serializeaza ca "#RRGGBB" si nu are canal alfa — vezi <see cref="ColorHex"/>.
    ''' </summary>
    Public Property CardShadowOpacity As Integer = 0

    ''' <summary>
    ''' Aerul pe care motorul il lasa in jurul cardurilor, in px logici, scriindu-l ca
    ''' <c>Padding</c> pe PARINTELE lor. 0 = nu atinge padding-ul parintelui, deci suprafetele
    ''' autorite in designer raman exact unde sunt. Fara el, un card lipit de marginea
    ''' parintelui n-are unde sa-si arunce umbra.
    ''' </summary>
    Public Property CardGutter As Integer = 0

    ''' <summary>Inaltimea unui element din bara de navigare, px logici. 0 = cea a controlului.</summary>
    Public Property NavItemHeight As Integer = 0

    ''' <summary>Inaltimea unui rand de arbore/lista, px logici. 0 = cea a controlului.</summary>
    Public Property ListRowHeight As Integer = 0

    ''' <summary>Inaltimea unui rand de grila, px logici. 0 = cea a controlului.</summary>
    Public Property GridRowHeight As Integer = 0

    ''' <summary>Inaltimea benzii de antet a grilei, px logici. 0 = cea a controlului.</summary>
    Public Property GridHeaderHeight As Integer = 0

    ''' <summary>Padding intern pentru inputuri/butoane (serializat ca 4 întregi).</summary>
    Public Property ControlPadding As PaddingDto = New PaddingDto()

    ''' <summary>Inel/underline accent pe focus la inputuri.</summary>
    Public Property FocusAccent As Boolean = False

    ''' <summary>Bară de titlu dark (DWM attr 20).</summary>
    Public Property DarkTitleBar As Boolean = False

    ''' <summary>Owner-draw pe header-ele de tab (reutilizează OnDrawTab).</summary>
    Public Property OwnerDrawTabs As Boolean = False

    ''' <summary>
    ''' «Nu-mi atinge culorile.» True ⇒ motorul NU scrie <c>BackColor</c>/<c>ForeColor</c>/
    ''' <c>Font</c> pe controale, ci le pune la loc pe cele reținute de
    ''' <see cref="DesignerBaseline"/> (adică exact ce s-a autorit în designer). Steagul schemei
    ''' «Colorful» din felia 0028.
    '''
    ''' Ce NU face: controalele <c>IThemedControl</c> primesc în continuare
    ''' <c>ApplyTheme(schema)</c> — culorile lor INTERNE (hover, selecție, linii, benzi) nu sunt
    ''' proprietăți de designer ale gazdei și trebuie să vină de undeva; contractul lor propriu
    ''' «<c>Color.Empty</c> = din temă, orice culoare pusă în designer câștigă» le apără oricum
    ''' alegerile. Peste asta, cele trei proprietăți ambientale ale controlului se restaurează
    ''' din instantaneu.
    ''' </summary>
    Public Property PreserveDesignerColors As Boolean = False

    ''' <summary>Padding-ul efectiv ca <see cref="Padding"/> WinForms.</summary>
    <JsonIgnore> Public ReadOnly Property PaddingValue As Padding
        Get
            Return ControlPadding.ToPadding()
        End Get
    End Property

End Class

''' <summary>
''' DTO serializabil pentru <see cref="Padding"/> (structul WinForms nu are un
''' contract JSON stabil). Toate valorile în px logici.
''' </summary>
Public NotInheritable Class PaddingDto
    Public Property Left As Integer = 0
    Public Property Top As Integer = 0
    Public Property Right As Integer = 0
    Public Property Bottom As Integer = 0

    Public Sub New()
    End Sub

    Public Sub New(all As Integer)
        Left = all : Top = all : Right = all : Bottom = all
    End Sub

    Public Sub New(left As Integer, top As Integer, right As Integer, bottom As Integer)
        Me.Left = left : Me.Top = top : Me.Right = right : Me.Bottom = bottom
    End Sub

    Public Function ToPadding() As Padding
        Return New Padding(Left, Top, Right, Bottom)
    End Function
End Class
