Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>Orientarea navigației: coloană (implicit) sau rând.</summary>
Public Enum KBotNavOrientation
    ''' <summary>Elementele curg de sus în jos (bara laterală clasică).</summary>
    Vertical
    ''' <summary>Elementele curg de la stânga la dreapta (bară de tip toolbar).</summary>
    Horizontal
End Enum

''' <summary>
''' Alinierea unui element pe axa principală: la început (sus/stânga) sau la capăt
''' (jos/dreapta). „Far" desprinde un grup de butoane de restul — ex. DDF/ORD.
''' </summary>
Public Enum KBotNavAlign
    ''' <summary>Ancorat la început: sus (vertical) sau stânga (orizontal).</summary>
    Near
    ''' <summary>Ancorat la capăt: jos (vertical) sau dreapta (orizontal).</summary>
    Far
End Enum

''' <summary>Colțul barei în care stă butonul de strângere/desfășurare (0025-05).</summary>
Public Enum KBotNavCorner
    ''' <summary>Stânga-sus.</summary>
    TopLeft
    ''' <summary>Dreapta-sus (implicit — bara verticală clasică e ancorată la stânga).</summary>
    TopRight
    ''' <summary>Stânga-jos.</summary>
    BottomLeft
    ''' <summary>Dreapta-jos.</summary>
    BottomRight
End Enum

''' <summary>
''' Starea de strângere a barei (0025-05). Butonul din colț le parcurge ciclic, în ordinea
''' <c>Expanded → Icons → Complete → Expanded</c>; <see cref="Icons"/> se sare când nu e
''' disponibil (vezi <see cref="KBotNavList.IconsCollapseAvailable"/>).
''' </summary>
Public Enum KBotNavCollapseState
    ''' <summary>Desfășurată — dimensiunea inițială, exact bara de dinainte de felie.</summary>
    Expanded
    ''' <summary>
    ''' Doar pictograme: bara ȘI butoanele se strâng la latura pictogramei + puțin aer.
    ''' Numai pe verticală și numai dacă măcar un buton are <c>Image</c>.
    ''' </summary>
    Icons
    ''' <summary>
    ''' Strânsă complet: bara rămâne cu puțin mai mult decât butonul de desfășurare, iar
    ''' elementele nu mai primesc niciun slot (nu se pictează și nu se pot apăsa).
    ''' </summary>
    Complete
End Enum

''' <summary>
''' Navigație owner-drawn — „tab-urile" simulate ale shell-ului (înlocuiește
''' TabControl-ul netematizabil). Un element = buton (cheie + text + badge opțional +
''' Enabled + Visible) SAU un separator (linie fină, neselectabilă). Elementele se pot
''' alinia la început sau la capătul barei (<see cref="KBotNavAlign"/>) și bara poate fi
''' verticală sau orizontală (<see cref="Orientation"/>). Selecția se schimbă cu click
''' sau Sus/Jos (Stânga/Dreapta în orizontal); re-selectarea aceleiași chei NU re-ridică
''' <see cref="SelectionChanged"/>. Toate culorile vin din schema activă (ApplyTheme).
'''
''' 0025-05: bara poate fi STRÂNSĂ (<see cref="Collapsible"/>) dintr-un buton mic desenat în
''' colțul <see cref="CollapseCorner"/>, care parcurge ciclic stările din
''' <see cref="KBotNavCollapseState"/>; iar aerul din jurul butoanelor se dă acum din
''' <see cref="ItemPadding"/> (înainte era o margine fixă de 6 px logici).
'''
''' 0025-06: butonul din colț poate purta pictograme autorate —
''' <see cref="CollapseExpandedImage"/> / <see cref="CollapseCollapsedImage"/>; fără ele rămâne
''' unghiul desenat, care ACUM se întoarce pe orice stare strânsă, nu doar pe «Complete».
'''
''' 0025-07: bara strânsă scoate la hover o ETICHETĂ PLUTITOARE cu butonul întreg
''' (<see cref="CollapsedFlyout"/> + <c>KBotNavFlyout</c>) — nu un ToolTip, ci butonul care se
''' desfășoară din propriul dreptunghi. 0025-08: forma butoanelor se dă acum din
''' <see cref="ItemCornerRadius"/> și <see cref="ItemGradient"/>, per BARĂ, și e aceeași pentru
''' butoane, butonul din colț și etichetă.
''' </summary>
<ToolboxItem(True)>
<DefaultProperty("Items")>
<DefaultEvent("SelectionChanged")>
Public NotInheritable Class KBotNavList
    Inherits Control
    Implements IThemedControl
    Implements ISupportInitialize

    ' English (slice 0025): the item model moved OUT of here into the public KBotNavItem, so the
    ' Visual Studio property grid can author it through the stock collection dialog. The private
    ' nested NavItem is gone; nothing else about the layout / paint / input logic changed.
    Private ReadOnly _items As New KBotNavItemCollection()
    Private _selectedKey As String
    Private _hoverIndex As Integer = -1
    Private _orientation As KBotNavOrientation = KBotNavOrientation.Vertical
    Private _layoutValid As Boolean
    Private _sepSeq As Integer                            ' contor pentru cheile interne ale separatorilor
    Private _iconSize As Integer = 20                     ' latura pictogramei, px logici (0025-02)
    Private _itemWidth As Integer                         ' lățimea butoanelor, px logici; 0 = automat (0025-03)
    Private _itemCornerRadius As Integer = -1             ' -1 = raza schemei active (0025-08)
    Private _itemGradient As Integer = 14                 ' 0..100; 0 = umplere plată (0025-08)

    ' ── Padding + strângere (0025-05) ─────────────────────────────────────────
    Private _itemPadding As New System.Windows.Forms.Padding(6)   ' aerul din jurul butoanelor, px logici
    Private _collapsible As Boolean
    Private _collapseCorner As KBotNavCorner = KBotNavCorner.TopRight
    Private _collapseButtonSize As Integer = 18            ' latura butonului din colț, px logici (0025-09)
    Private _collapseState As KBotNavCollapseState = KBotNavCollapseState.Expanded
    Private _collapseHover As Boolean
    ' Pictogramele butonului din colț (0025-06). Nothing pe oricare dintre ele => pe starea aceea
    ' se desenează unghiul din DrawCollapseButton, ca înainte.
    Private _collapseImageExpanded As Image
    Private _collapseImageCollapsed As Image
    ' Dimensiunea (lățime pe verticală / înălțime pe orizontală) la care se ÎNTOARCE bara.
    ' Se reține ultima valoare avută DESFĂȘURATĂ, nu cea din constructor: operatorul poate
    ' lăți bara înainte s-o strângă, iar „mărimea inițială" înseamnă mărimea LUI.
    Private _expandedExtent As Integer
    ' True cât timp NOI schimbăm dimensiunea; altfel OnSizeChanged ar reține lățimea strânsă
    ' ca „dimensiune inițială" și bara nu s-ar mai putea desfășura niciodată.
    Private _applyingCollapseExtent As Boolean

    ' ── Eticheta plutitoare din starea strânsă (0025-07) ──────────────────────
    Private _flyoutEnabled As Boolean = True
    Private _flyoutDelay As Integer = 250                 ' ms de hover înainte să iasă
    Private _flyoutSlide As Integer = 120                 ' ms de desfășurare
    Private _flyout As KBotNavFlyout
    Private _flyoutDelayTimer As System.Windows.Forms.Timer
    Private _flyoutAnimTimer As System.Windows.Forms.Timer
    Private _flyoutIndex As Integer = -1                  ' elementul pentru care iese eticheta
    Private _flyoutProgress As Double                     ' 0 = doar banda pictogramei, 1 = butonul întreg

    ' ── Inițializare din designer (ISupportInitialize) ────────────────────────
    ' Between BeginInit and EndInit the control accepts whatever InitializeComponent writes
    ' WITHOUT validating it — the designer emits properties in its own order, so SelectedKey can
    ' arrive before Items is populated. EndInit is where the contract is enforced again.
    Private _initializing As Boolean
    Private _pendingSelectedKey As String
    Private _hasPendingSelectedKey As Boolean

    ' ── Culori/stil derivate din paletă (setate în ApplyTheme) ────────────────
    Private _scheme As ThemeScheme
    Private _selectedFill As Color = SystemColors.ControlLight
    Private _accent As Color = SystemColors.Highlight
    Private _hoverFill As Color = SystemColors.ControlLight
    Private _textNormal As Color = SystemColors.GrayText
    Private _textDisabled As Color = SystemColors.GrayText
    Private _badgeFill As Color = SystemColors.Control
    Private _badgeText As Color = SystemColors.GrayText
    Private _separatorColor As Color = SystemColors.ControlDark

    ' Font semibold pentru elementul selectat (derivat din fontul ambient).
    Private _semiboldFont As Font

    ''' <summary>Ridicat când selecția se schimbă (click, tastatură sau setter).</summary>
    Public Event SelectionChanged(key As String)

    ''' <summary>Ridicat când bara se strânge sau se desfășoară (0025-05).</summary>
    Public Event CollapseStateChanged(state As KBotNavCollapseState)

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable, True)
        TabStop = True
        Width = 170
        _expandedExtent = Width
        _items.Owner = Me
    End Sub

    ' ── API public ─────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Butoanele și separatorii, în ordinea de afișare. Editabil din grila de proprietăți
    ''' (dialogul standard de colecție) sau din cod, prin <see cref="AddItem"/> /
    ''' <see cref="AddSeparator"/> — aceeași colecție.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Butoanele și separatorii barei, în ordinea de afișare.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Items As KBotNavItemCollection
        Get
            Return _items
        End Get
    End Property

    ''' <summary>
    ''' Orientarea barei. Schimbarea reașază elementele și repictează.
    '''
    ''' Dacă bara e STRÂNSĂ, schimbarea axei o desfășoară întâi pe axa veche (altfel dimensiunea
    ''' strânsă ar rămâne agățată de o axă care nu mai e cea care se strânge), apoi reține
    ''' mărimea curentă de pe axa nouă ca dimensiune inițială și restrânge. „Icons" nu există
    ''' pe orizontală, deci acolo se retrogradează la „Complete".
    ''' </summary>
    <DefaultValue(KBotNavOrientation.Vertical)>
    Public Property Orientation As KBotNavOrientation
        Get
            Return _orientation
        End Get
        Set(value As KBotNavOrientation)
            If value = _orientation Then Return

            Dim previous As KBotNavCollapseState = _collapseState
            If previous <> KBotNavCollapseState.Expanded Then
                _collapseState = KBotNavCollapseState.Expanded
                ApplyCollapseExtent()
            End If

            _orientation = value
            _expandedExtent = CurrentExtent()

            If previous <> KBotNavCollapseState.Expanded Then
                _collapseState = If(previous = KBotNavCollapseState.Icons AndAlso Not IconsCollapseAvailable,
                                    KBotNavCollapseState.Complete, previous)
                ApplyCollapseExtent()
            End If

            InvalidateLayout()
            If _collapseState <> previous Then RaiseEvent CollapseStateChanged(_collapseState)
        End Set
    End Property

    ''' <summary>
    ''' Latura (px logici, scalați la DPI) a pătratului în care se desenează
    ''' <see cref="KBotNavItem.Image"/>. Implicit 20. Se aplică TUTUROR elementelor — pictogramele
    ''' unei bare de navigație trebuie să fie de aceeași mărime, altfel textul nu se mai aliniază.
    ''' Pictograma se scalează în pătrat, deci sursa poate fi de orice dimensiune.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Latura (px logici) a pictogramei elementelor. Implicit 20; se aplică tuturor.")>
    <DefaultValue(20)>
    Public Property IconSize As Integer
        Get
            Return _iconSize
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _iconSize Then Return
            _iconSize = clamped
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>
    ''' Lățimea butoanelor, în px logici (scalați la DPI). **0 = automat**, adică exact
    ''' comportamentul dinainte de 0025-03:
    ''' <list type="bullet">
    ''' <item>pe VERTICALĂ butonul umple lățimea barei (minus marginile);</item>
    ''' <item>pe ORIZONTALĂ lățimea se măsoară din text (+ pictogramă, + badge), cu un minim de 48.</item>
    ''' </list>
    ''' O valoare pozitivă înlocuiește măsurarea: TOATE butoanele primesc exact lățimea aceea, iar
    ''' textul prea lung se taie cu «…» (nu se rupe rândul). Pe verticală se limitează la lățimea
    ''' utilă a barei — un buton nu poate fi mai lat decât bara care îl ține; butoanele rămân
    ''' aliniate la marginea din stânga.
    '''
    ''' NU se aplică separatorilor pe orizontală (un separator e o linie fină, nu un buton); pe
    ''' verticală linia lor urmează aceeași coloană, ca să nu iasă din dreptul butoanelor.
    '''
    ''' Valorile negative se aduc la 0 (= automat), ca la <see cref="IconSize"/> — un setter de
    ''' dimensiune care aruncă ar rupe <c>InitializeComponent</c> la o valoare greșită din designer.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Lățimea butoanelor (px logici). 0 = automat: pe verticală umplu bara, pe orizontală se măsoară din text.")>
    <DefaultValue(0)>
    Public Property ItemWidth As Integer
        Get
            Return _itemWidth
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _itemWidth Then Return
            _itemWidth = clamped
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>
    ''' Raza colțurilor rotunjite ale BUTOANELOR barei și ale etichetei plutitoare, în px logici
    ''' (scalați la DPI). Se dă **pe bară, nu pe element**: într-o navigație butoanele trebuie să
    ''' aibă aceeași formă, altfel coloana arată rupt — exact ca <see cref="IconSize"/>.
    '''
    ''' <b>-1 (implicit) = raza SCHEMEI ACTIVE</b> (<c>ThemeStyleOptions.CornerRadius</c>: 0 pe
    ''' Classic și Dark, 8 pe Modern), adică exact comportamentul de dinainte de 0025-08 și singura
    ''' valoare care urmează tema când operatorul schimbă schema. O valoare de la 0 în sus o
    ''' înlocuiește: 0 = colțuri drepte chiar și pe o schemă rotunjită.
    '''
    ''' Sub -1 se limitează la -1 (nu se aruncă, ca la celelalte măsuri — un setter de dimensiune
    ''' care aruncă ar rupe <c>InitializeComponent</c> la o valoare greșită din designer).
    ''' Raza efectivă e oricum limitată la jumătatea laturii mici de <c>ThemeShapes.RoundedRect</c>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Raza colțurilor butoanelor și a etichetei (px logici). -1 = raza schemei active; 0 = colțuri drepte.")>
    <DefaultValue(-1)>
    Public Property ItemCornerRadius As Integer
        Get
            Return _itemCornerRadius
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(-1, value)
            If clamped = _itemCornerRadius Then Return
            _itemCornerRadius = clamped
            Invalidate()
            RefreshFlyout()
        End Set
    End Property

    ''' <summary>
    ''' Cât de pronunțat e gradientul „modern" de pe fundalul butoanelor (selectat / survolat) și de
    ''' pe eticheta plutitoare: 0..100. Implicit 14 — vizibil, nu lucios. <b>0 = umplere plată</b>,
    ''' adică fix ce se picta înainte de 0025-08.
    '''
    ''' NU introduce culori noi: cele două capete se derivă din culoarea de bază a schemei active
    ''' (<c>AccentSoft</c> derivat pentru selecție, <c>ButtonHover</c> pentru survolare), mai deschis
    ''' sus și puțin mai închis jos — deci gradientul se schimbă odată cu tema, ca tot restul barei.
    ''' Vezi <c>ThemeShapes.FillModern</c>. Valorile din afara intervalului se limitează.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Intensitatea gradientului de pe butoane și etichetă (0..100). Implicit 14; 0 = umplere plată.")>
    <DefaultValue(14)>
    Public Property ItemGradient As Integer
        Get
            Return _itemGradient
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, Math.Min(100, value))
            If clamped = _itemGradient Then Return
            _itemGradient = clamped
            Invalidate()
            RefreshFlyout()
        End Set
    End Property

    ''' <summary>
    ''' Aerul din jurul butoanelor, în px logici (scalați la DPI ca tot restul barei: <c>IconSize</c>,
    ''' <c>ItemWidth</c>, înălțimea rândului). Implicit 6 pe toate laturile — exact marginea fixă de
    ''' dinainte de 0025-05, ca o bară care nu atinge proprietatea să arate identic.
    '''
    ''' Pe VERTICALĂ <c>Left</c>/<c>Right</c> strâng coloana butoanelor (deci și lățimea lor, când
    ''' sunt pe „umple bara"), iar <c>Top</c>/<c>Bottom</c> depărtează primul buton de sus și
    ''' ultimul buton „Far" de jos; pe ORIZONTALĂ rolurile se inversează.
    '''
    ''' Se numește <c>ItemPadding</c>, nu <c>Padding</c>: <c>Control.Padding</c> există deja pe orice
    ''' control, e SCALAT AUTOMAT de WinForms la autoscalarea formularului și ar intra în coliziune
    ''' cu scalarea proprie a barei (aceeași valoare ajustată de două ori). Cel moștenit e ascuns
    ''' din grilă tocmai ca să nu stea acolo o proprietate care nu face nimic.
    ''' Valorile negative se aduc la 0, ca la <see cref="IconSize"/> / <see cref="ItemWidth"/>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Aerul (px logici) din jurul butoanelor. Implicit 6 pe fiecare latură.")>
    <DefaultValue(GetType(System.Windows.Forms.Padding), "6, 6, 6, 6")>
    Public Property ItemPadding As System.Windows.Forms.Padding
        Get
            Return _itemPadding
        End Get
        Set(value As System.Windows.Forms.Padding)
            Dim clamped As New System.Windows.Forms.Padding(Math.Max(0, value.Left), Math.Max(0, value.Top),
                                                            Math.Max(0, value.Right), Math.Max(0, value.Bottom))
            If clamped = _itemPadding Then Return
            _itemPadding = clamped
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>
    ''' <c>Control.Padding</c> nu are niciun efect pe bara asta (layout-ul e owner-drawn și își
    ''' calculează singur sloturile) — se ascunde din grila de proprietăți ca să nu fie confundat
    ''' cu <see cref="ItemPadding"/>, care e cel care chiar lucrează.
    ''' </summary>
    <Browsable(False)>
    <EditorBrowsable(EditorBrowsableState.Never)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property Padding As System.Windows.Forms.Padding
        Get
            Return MyBase.Padding
        End Get
        Set(value As System.Windows.Forms.Padding)
            MyBase.Padding = value
        End Set
    End Property

    ''' <summary>
    ''' True => în colțul <see cref="CollapseCorner"/> apare un buton mic care strânge/desfășoară
    ''' bara. Un singur buton parcurge ciclic stările: <c>Icons</c> (dacă e disponibil), apoi
    ''' <c>Complete</c>, apoi înapoi la mărimea inițială.
    '''
    ''' Butonul își REZERVĂ o bandă la capătul barei dinspre colțul ales (sus/jos pe verticală,
    ''' stânga/dreapta pe orizontală), ca să nu se suprapună peste primul sau ultimul buton.
    ''' Trecerea pe False desfășoară bara imediat.
    ''' </summary>
    <Category("K-BOT")>
    <Description("True => un buton mic din colț strânge bara (Icons → Complete → mărimea inițială).")>
    <DefaultValue(False)>
    Public Property Collapsible As Boolean
        Get
            Return _collapsible
        End Get
        Set(value As Boolean)
            If value = _collapsible Then Return
            _collapsible = value
            If Not _collapsible AndAlso _collapseState <> KBotNavCollapseState.Expanded Then
                _collapseState = KBotNavCollapseState.Expanded
                ApplyCollapseExtent()
                InvalidateLayout()
                RaiseEvent CollapseStateChanged(_collapseState)
                Return
            End If
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>
    ''' Colțul în care stă butonul de strângere. Implicit dreapta-sus — bara verticală clasică e
    ''' ancorată la stânga ferestrei, deci butonul cade pe marginea dinspre conținut.
    ''' Ignorat cât timp <see cref="Collapsible"/> e False.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Colțul în care se desenează butonul de strângere. Implicit dreapta-sus.")>
    <DefaultValue(KBotNavCorner.TopRight)>
    Public Property CollapseCorner As KBotNavCorner
        Get
            Return _collapseCorner
        End Get
        Set(value As KBotNavCorner)
            If value = _collapseCorner Then Return
            _collapseCorner = value
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>
    ''' Latura (px logici, scalați la DPI) a pătratului butonului de strângere din colț. Implicit
    ''' 18 — exact valoarea fixă de dinainte de 0025-09, deci o bară care nu atinge proprietatea
    ''' arată identic.
    '''
    ''' Nu schimbă doar desenul: butonul își rezervă o BANDĂ de <c>latură + 2*6</c> la capătul axei
    ''' principale, iar starea <see cref="KBotNavCollapseState.Complete"/> strânge bara exact la
    ''' banda aia. Deci un buton mai mare împinge primul (sau ultimul) element mai încolo ȘI
    ''' lățește bara complet strânsă. Ce e desenat ÎN buton — unghiul sau
    ''' <see cref="CollapseExpandedImage"/> / <see cref="CollapseCollapsedImage"/> — se scalează cu
    ''' el, nu rămâne de 18.
    '''
    ''' <b>0 = niciun buton</b>: nu se desenează, nu se poate apăsa și **nu mai rezervă bandă**
    ''' (aceeași convenție ca <see cref="IconSize"/> = 0 pentru «fără pictograme»). E o stare
    ''' validă, nu o greșeală: o aplicație care strânge bara din propriul ei buton de bară de
    ''' unelte, prin <see cref="ToggleCollapse"/> sau <see cref="CollapseState"/>, nu vrea și
    ''' unghiul din colț. Dar ATENȚIE — cu <see cref="Collapsible"/> = True și latura 0, operatorul
    ''' NU mai poate strânge sau desfășura bara cu mouse-ul; strângerea rămâne exclusiv din cod.
    '''
    ''' Valorile negative se aduc la 0, ca la celelalte măsuri (un setter de dimensiune care aruncă
    ''' ar rupe <c>InitializeComponent</c> la o valoare greșită din designer).
    ''' </summary>
    <Category("K-BOT")>
    <Description("Latura (px logici) a butonului de strângere din colț. Implicit 18; 0 = fără buton (strângere doar din cod).")>
    <DefaultValue(18)>
    Public Property CollapseButtonSize As Integer
        Get
            Return _collapseButtonSize
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _collapseButtonSize Then Return
            _collapseButtonSize = clamped
            ' Banda se schimbă => layout; iar dacă bara e STRÂNSĂ chiar acum, dimensiunea la care
            ' e strânsă se calculează din latura butonului, deci trebuie reaplicată — altfel o
            ' bară pe «Complete» ar rămâne la lățimea butonului vechi.
            If _collapsible AndAlso _collapseState <> KBotNavCollapseState.Expanded Then
                ApplyCollapseExtent()
            End If
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>
    ''' Pictograma butonului din colț cât timp bara e DESFĂȘURATĂ — adică imaginea de pe butonul
    ''' care STRÂNGE. Editabilă din grila de proprietăți cu editorul de imagine stoc (designer-ul
    ''' depune imaginea în <c>.resx</c>-ul formularului), exact ca <see cref="KBotNavItem.Image"/>.
    '''
    ''' <c>Nothing</c> (implicit) = se desenează unghiul («chevron») tematizat de dinainte de
    ''' 0025-06. Cele două pictograme sunt INDEPENDENTE: cine dă doar una primește unghiul pe
    ''' cealaltă stare — nu se cere un set complet ca să meargă jumătate din el.
    '''
    ''' Imaginea se scalează în pătratul butonului (18 px logici, minus 2 px de aer). Bara NU
    ''' deține imaginea și nu o eliberează niciodată: e a apelantului sau a resurselor
    ''' formularului, ca la <c>KBotCaptionBar.IconImage</c>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Pictograma butonului din colț când bara e desfășurată. Nimic = unghiul desenat implicit.")>
    Public Property CollapseExpandedImage As Image
        Get
            Return _collapseImageExpanded
        End Get
        Set(value As Image)
            If value Is _collapseImageExpanded Then Return
            _collapseImageExpanded = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Pictograma butonului din colț cât timp bara e STRÂNSĂ (<see cref="KBotNavCollapseState.Icons"/>
    ''' SAU <see cref="KBotNavCollapseState.Complete"/> — o singură imagine pentru amândouă, fiindcă
    ''' butonul face același lucru din amândouă: desfășoară). Vezi
    ''' <see cref="CollapseExpandedImage"/> pentru rest.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Pictograma butonului din colț când bara e strânsă. Nimic = unghiul desenat implicit.")>
    Public Property CollapseCollapsedImage As Image
        Get
            Return _collapseImageCollapsed
        End Get
        Set(value As Image)
            If value Is _collapseImageCollapsed Then Return
            _collapseImageCollapsed = value
            Invalidate()
        End Set
    End Property

    ' English (like KBotNavItem.Image, slice 0025-03): a reference type has no usable <DefaultValue>
    ' in VB, so «unset» is said with ShouldSerialize/Reset — otherwise the designer writes
    ' «navViews.CollapseExpandedImage = Nothing» into every form that never touched the property.
    ' Private on purpose: TypeDescriptor finds them by name including non-public members.
    Private Function ShouldSerializeCollapseExpandedImage() As Boolean
        Return _collapseImageExpanded IsNot Nothing
    End Function

    Private Sub ResetCollapseExpandedImage()
        CollapseExpandedImage = Nothing
    End Sub

    Private Function ShouldSerializeCollapseCollapsedImage() As Boolean
        Return _collapseImageCollapsed IsNot Nothing
    End Function

    Private Sub ResetCollapseCollapsedImage()
        CollapseCollapsedImage = Nothing
    End Sub

    ''' <summary>
    ''' True (implicit) => cât timp bara e strânsă la pictograme, hover-ul pe un buton scoate spre
    ''' dreapta o ETICHETĂ PLUTITOARE cu butonul întreg — pictograma pe loc, textul desfășurându-se
    ''' de la stânga la dreapta, ca meniul lateral al unei pagini web.
    '''
    ''' Nu e un <c>ToolTip</c> (galben, netematizabil, cu poziția lui): e o fereastră proprie care
    ''' preia toate proprietățile butonului de sub cursor — pictogramă, text, badge, starea
    ''' <c>Enabled</c> și fundalul de selecție — cu culorile schemei active. Vezi
    ''' <c>KBotNavFlyout</c>.
    '''
    ''' Implicit True, spre deosebire de <see cref="Collapsible"/>: bara nu se strânge dacă nu i se
    ''' cere, dar odată strânsă la pictograme e nefolosibilă fără etichete — nu mai scrie nimic pe
    ''' butoane. Poarta rămâne <see cref="Collapsible"/>; asta doar spune ce se întâmplă dincolo de
    ''' ea. Trecerea pe False ascunde imediat eticheta care e afară.
    '''
    ''' Se aplică DOAR barei verticale strânse la <see cref="KBotNavCollapseState.Icons"/> — și
    ''' asta nu e o limitare aleasă, ci singurul caz care există: «Icons» nu e disponibil pe
    ''' orizontală, iar în «Complete» niciun buton nu mai are slot, deci n-are ce fi survolat.
    ''' </summary>
    <Category("K-BOT")>
    <Description("True => hover pe un buton al barei strânse scoate spre dreapta o etichetă cu butonul întreg.")>
    <DefaultValue(True)>
    Public Property CollapsedFlyout As Boolean
        Get
            Return _flyoutEnabled
        End Get
        Set(value As Boolean)
            If value = _flyoutEnabled Then Return
            _flyoutEnabled = value
            If Not _flyoutEnabled Then CancelFlyout()
        End Set
    End Property

    ''' <summary>
    ''' Cât stă cursorul (ms) pe buton înainte să iasă eticheta. Implicit 250 — destul cât plimbatul
    ''' mouse-ului peste bară să nu scoată cinci etichete pe rând, destul de puțin cât să nu pară
    ''' că nu răspunde. 0 = imediat. Valorile negative se aduc la 0, ca la
    ''' <see cref="IconSize"/> / <see cref="ItemWidth"/> / <see cref="ItemPadding"/>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Cât așteaptă hover-ul (ms) înainte să scoată eticheta. Implicit 250; 0 = imediat.")>
    <DefaultValue(250)>
    Public Property FlyoutDelay As Integer
        Get
            Return _flyoutDelay
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _flyoutDelay Then Return
            _flyoutDelay = clamped
        End Set
    End Property

    ''' <summary>
    ''' Cât durează (ms) desfășurarea etichetei spre dreapta. Implicit 120. 0 = fără animație,
    ''' eticheta apare dintr-odată la lățimea ei completă. Valorile negative se aduc la 0.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Durata (ms) desfășurării etichetei spre dreapta. Implicit 120; 0 = fără animație.")>
    <DefaultValue(120)>
    Public Property FlyoutSlideDuration As Integer
        Get
            Return _flyoutSlide
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _flyoutSlide Then Return
            _flyoutSlide = clamped
        End Set
    End Property

    ''' <summary>
    ''' Starea curentă de strângere. STARE DE RULARE, nu valoare de designer: nu se serializează
    ''' (ar îngheța formularul strâns și s-ar bate cu <c>Size</c>-ul scris tot de designer).
    '''
    ''' Setarea aruncă <c>InvalidOperationException</c> pe o stare imposibilă (bara nu e
    ''' colapsabilă, sau „Icons" fără pictograme / pe orizontală) — fără no-op-uri tăcute.
    ''' Butonul din colț NU aruncă: el sare stările indisponibile.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property CollapseState As KBotNavCollapseState
        Get
            Return _collapseState
        End Get
        Set(value As KBotNavCollapseState)
            If value = _collapseState Then Return
            If value <> KBotNavCollapseState.Expanded AndAlso Not _collapsible Then
                Throw New InvalidOperationException("Bara nu e colapsabilă (Collapsible = False).")
            End If
            If value = KBotNavCollapseState.Icons AndAlso Not IconsCollapseAvailable Then
                Throw New InvalidOperationException(
                    "Starea «Icons» nu e disponibilă: bara e orizontală, IconSize e 0 sau niciun buton vizibil nu are pictogramă.")
            End If
            ApplyCollapseState(value)
        End Set
    End Property

    ''' <summary>
    ''' True dacă starea <see cref="KBotNavCollapseState.Icons"/> are sens ACUM: bară verticală,
    ''' <see cref="IconSize"/> pozitiv și măcar un buton vizibil cu pictogramă. Când e False,
    ''' butonul din colț sare direct la <see cref="KBotNavCollapseState.Complete"/>.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IconsCollapseAvailable As Boolean
        Get
            If _orientation <> KBotNavOrientation.Vertical Then Return False
            If _iconSize <= 0 Then Return False
            For Each it As KBotNavItem In _items
                If Not it.IsSeparator AndAlso it.Visible AndAlso it.Image IsNot Nothing Then Return True
            Next
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Trece la starea următoare din ciclu, exact ca un click pe butonul din colț:
    ''' desfășurată → doar pictograme (dacă se poate) → strânsă complet → desfășurată.
    ''' Aruncă dacă bara nu e colapsabilă.
    ''' </summary>
    Public Sub ToggleCollapse()
        Try
            If Not _collapsible Then Throw New InvalidOperationException("Bara nu e colapsabilă (Collapsible = False).")
            CycleCollapse()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNavList.ToggleCollapse", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Adaugă un buton (aliniat la început). Cheia trebuie să fie nevidă și unică.</summary>
    Public Sub AddItem(key As String, text As String)
        AddItem(key, text, KBotNavAlign.Near)
    End Sub

    ''' <summary>Adaugă un buton cu aliniere explicită. Cheia trebuie să fie nevidă și unică.</summary>
    Public Sub AddItem(key As String, text As String, align As KBotNavAlign)
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Cheie vidă.", NameOf(key))
        If FindIndex(key) >= 0 Then Throw New ArgumentException($"Cheie duplicată: '{key}'.", NameOf(key))
        ' The collection invalidates the layout by itself (slice 0025).
        _items.Add(New KBotNavItem(key, text, align))
    End Sub

    ''' <summary>
    ''' Adaugă un separator (linie fină neselectabilă) — se pot adăuga oricâți. În modul
    ''' „Far" desparte grupul de butoane ancorate la capăt (ex. DDF/ORD) de rest.
    ''' </summary>
    Public Sub AddSeparator(Optional align As KBotNavAlign = KBotNavAlign.Near)
        _items.Add(New KBotNavItem With {.Key = NextSeparatorKey(), .IsSeparator = True, .Align = align})
    End Sub

    ''' <summary>Setează badge-ul unui buton (0 = ascuns). Cheie necunoscută => excepție.</summary>
    Public Sub SetBadge(key As String, count As Integer)
        _items(RequireIndex(key)).Badge = count
        Invalidate()
    End Sub

    ''' <summary>Activează/dezactivează un buton. Cheie necunoscută => excepție.</summary>
    Public Sub SetItemEnabled(key As String, enabled As Boolean)
        _items(RequireIndex(key)).Enabled = enabled
        Invalidate()
    End Sub

    ''' <summary>
    ''' Arată/ascunde un buton. Un buton ascuns nu ocupă spațiu, nu se pictează, nu se
    ''' poate selecta și e sărit de navigarea cu tastatura. Cheie necunoscută => excepție.
    ''' </summary>
    Public Sub SetItemVisible(key As String, visible As Boolean)
        _items(RequireIndex(key)).Visible = visible
        InvalidateLayout()
    End Sub

    ''' <summary>
    ''' Cheia selectată. O cheie NECUNOSCUTĂ aruncă ArgumentException (regula casei: fără no-op-uri
    ''' tăcute); setarea aceleiași chei nu re-ridică evenimentul.
    '''
    ''' NIMIC / ȘIR GOL înseamnă «nicio selecție» și NU e o eroare — e o stare, nu o cheie greșită.
    ''' Distincția a costat o vedere întreagă: designer-ul WinForms serializează o proprietate String
    ''' rămasă Nothing ca <c>navSub.SelectedKey = Nothing</c>, iar la prima regenerare a fișierului
    ''' <c>DdfView.Designer.vb</c> linia aceea a început să arunce din <c>InitializeComponent</c> —
    ''' adică vederea DDF nu se mai deschidea deloc, iar din navlist nu se întâmpla «nimic».
    ''' Orice control care ajunge în designer trebuie să suporte ce scrie designer-ul despre el.
    ''' </summary>
    Public Property SelectedKey As String
        Get
            If _initializing AndAlso _hasPendingSelectedKey Then Return _pendingSelectedKey
            Return _selectedKey
        End Get
        Set(value As String)
            ' English (slice 0025): while InitializeComponent runs, STORE and return. The designer
            ' has no obligation to emit Items before SelectedKey, and the validating setter below
            ' would throw on a key that is about to exist one line later.
            If _initializing Then
                _pendingSelectedKey = value
                _hasPendingSelectedKey = True
                Return
            End If
            If String.IsNullOrEmpty(value) Then
                ClearSelection()
                Return
            End If
            SelectIndex(RequireIndex(value))
        End Set
    End Property

    ' ── ISupportInitialize ─────────────────────────────────────────────────────

    ''' <summary>Începutul blocului de inițializare emis de designer (validările se suspendă).</summary>
    Public Sub BeginInit() Implements ISupportInitialize.BeginInit
        _initializing = True
    End Sub

    ''' <summary>
    ''' Sfârșitul blocului de inițializare: se dau chei separatorilor fără cheie, se validează
    ''' butoanele (cheie nevidă și unică) și se aplică selecția reținută.
    '''
    ''' În DESIGNER validarea și aplicarea selecției se sar: o cheie pe jumătate tastată ar
    ''' arunca din <c>InitializeComponent</c>, adică formularul nu s-ar mai deschide deloc —
    ''' exact defectul pe care îl semnalăm vizual, cu chenar roșu (vezi <see cref="OnPaint"/>).
    ''' </summary>
    Public Sub EndInit() Implements ISupportInitialize.EndInit
        Try
            _initializing = False

            ' 1) Separatorii autoriți în designer nu au cheie — le-o dăm acum, fără coliziune.
            For Each it As KBotNavItem In _items
                If it.IsSeparator AndAlso String.IsNullOrWhiteSpace(it.Key) Then
                    it.Key = NextSeparatorKey()
                End If
            Next

            If KBotDesignTime.IsDesignTime(Me) Then
                ' Design time: nu validăm, dar PĂSTRĂM valoarea ca să se re-serializeze corect
                ' (dacă am ignora-o, designer-ul ar pierde-o la următoarea regenerare).
                If _hasPendingSelectedKey Then _selectedKey = _pendingSelectedKey
                _hasPendingSelectedKey = False
                _pendingSelectedKey = Nothing
                InvalidateLayout()
                Return
            End If

            ' 2) Contractul de rulare, neschimbat: cheie nevidă și unică pe orice ne-separator.
            ValidateItems()

            ' 3) Selecția reținută trece acum pe drumul normal (inclusiv excepția pe cheie greșită).
            If _hasPendingSelectedKey Then
                Dim pending As String = _pendingSelectedKey
                _hasPendingSelectedKey = False
                _pendingSelectedKey = Nothing
                SelectedKey = pending
            End If

            InvalidateLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNavList.EndInit", ex)
            Throw
        End Try
    End Sub

    ' Cheile butoanelor: nevide și unice. Separatorii sunt săriți (cheia lor e internă).
    Private Sub ValidateItems()
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        For i As Integer = 0 To _items.Count - 1
            Dim it As KBotNavItem = _items(i)
            If it.IsSeparator Then Continue For
            If String.IsNullOrWhiteSpace(it.Key) Then
                Throw New ArgumentException($"Cheie vidă la elementul {i} («{If(it.Text, String.Empty)}»).", NameOf(Items))
            End If
            If Not seen.Add(it.Key) Then
                Throw New ArgumentException($"Cheie duplicată: '{it.Key}' (elementul {i}).", NameOf(Items))
            End If
        Next
    End Sub

    ' Următoarea cheie internă de separator, garantat nefolosită de vreun element existent
    ' (un separator autorit manual în designer poate purta deja «__sep_1»).
    Private Function NextSeparatorKey() As String
        Dim k As String
        Do
            _sepSeq += 1
            k = "__sep_" & _sepSeq
        Loop While KeyExistsAnywhere(k)
        Return k
    End Function

    ' Spre deosebire de FindIndex, asta se uită ȘI la separatori.
    Private Function KeyExistsAnywhere(key As String) As Boolean
        For Each it As KBotNavItem In _items
            If String.Equals(it.Key, key, StringComparison.Ordinal) Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Deselectează tot. Ridică <c>SelectionChanged</c> doar dacă exista o selecție — aceeași regulă
    ''' ca <see cref="SelectIndex"/>: evenimentul marchează o SCHIMBARE, nu o atribuire.
    ''' </summary>
    Public Sub ClearSelection()
        If String.IsNullOrEmpty(_selectedKey) Then Return
        _selectedKey = Nothing
        InvalidateLayout()
        RaiseEvent SelectionChanged(Nothing)
    End Sub

    ' ── Interne ────────────────────────────────────────────────────────────────

    ' English (slice 0025): a lookup never sees a separator (its key is internal plumbing) and an
    ' empty key never matches anything — the designer can leave an item half-filled, and a
    ' Nothing-vs-Nothing match would silently hand out the wrong item.
    Private Function FindIndex(key As String) As Integer
        If String.IsNullOrEmpty(key) Then Return -1
        For i As Integer = 0 To _items.Count - 1
            Dim it As KBotNavItem = _items(i)
            If it.IsSeparator Then Continue For
            If String.Equals(it.Key, key, StringComparison.Ordinal) Then Return i
        Next
        Return -1
    End Function

    ' Indexul cheii sau ArgumentException — fără no-op-uri tăcute (regula casei).
    Private Function RequireIndex(key As String) As Integer
        Dim idx As Integer = FindIndex(key)
        If idx < 0 Then Throw New ArgumentException($"Cheie necunoscută: '{key}'.", NameOf(key))
        Return idx
    End Function

    ' Selectează prin index; ridică evenimentul DOAR la schimbare reală. Separatorii și
    ' butoanele ascunse nu sunt selectabili.
    Private Sub SelectIndex(index As Integer)
        Dim it As KBotNavItem = _items(index)
        If it.IsSeparator OrElse Not it.Visible Then
            Throw New ArgumentException($"Cheie neselectabilă: '{it.Key}'.", NameOf(index))
        End If
        If String.Equals(it.Key, _selectedKey, StringComparison.Ordinal) Then Return
        _selectedKey = it.Key
        Invalidate()
        ' Eticheta e afară chiar peste butonul pe care tocmai s-a dat click: fără asta ar rămâne
        ' cu fundalul de hover și fontul obișnuit, adică ar contrazice bara de dedesubt.
        RefreshFlyout()
        RaiseEvent SelectionChanged(it.Key)
    End Sub

    ' Marchează layout-ul „murdar" și cere repictare. FRIEND din 0025: colecția de elemente
    ' (KBotNavItemCollection) o cheamă la fiecare adăugare/ștergere/reordonare.
    Friend Sub InvalidateLayout()
        ' Eticheta plutitoare ține minte un INDEX și un dreptunghi din layout-ul vechi; orice
        ' lucru care mută sloturile (add/remove, vizibilitate, orientare, resize, strângere) o
        ' face să arate un buton care nu mai e acolo. Se retrage, nu se recalculează.
        CancelFlyout()
        _layoutValid = False
        Invalidate()
    End Sub

    ''' <summary>
    ''' Item height, in device pixels. Since slice 0049 it comes from the SCHEME: Modern wants
    ''' taller items than the historic 36, and the number has to be adjustable from the options
    ''' window rather than recompiled. 0 on a scheme means "leave it as it was", so the other
    ''' three schemes do not move by a pixel.
    ''' </summary>
    Private Function ItemThickness() As Integer
        Dim fromScheme As Integer = If(_scheme IsNot Nothing AndAlso _scheme.Style IsNot Nothing,
                                       _scheme.Style.NavItemHeight, 0)
        Return ThemeShapes.ScaleDpi(Me, If(fromScheme > 0, fromScheme, 36))
    End Function

    Private Function SeparatorExtent() As Integer
        Return ThemeShapes.ScaleDpi(Me, 11)
    End Function

    ' ── Pictograma elementului (0025-02) ───────────────────────────────────────
    ' O SINGURĂ funcție de geometrie, folosită și la măsurare și la pictare, ca cele două să nu
    ' poată să se despartă: dacă slotul crește, textul se mută cu el, automat.

    ' Latura nominală a pătratului pictogramei, scalată la DPI.
    Private Function IconSide() As Integer
        Return ThemeShapes.ScaleDpi(Me, _iconSize)
    End Function

    ' Spațiul dintre pictogramă și text.
    Private Function IconGap() As Integer
        Return ThemeShapes.ScaleDpi(Me, 8)
    End Function

    ' Cât mănâncă pictograma din lățimea rândului (0 dacă elementul nu are una).
    Private Function IconSlotWidth(it As KBotNavItem) As Integer
        If it.IsSeparator OrElse it.Image Is Nothing OrElse _iconSize <= 0 Then Return 0
        Return IconSide() + IconGap()
    End Function

    ' Pătratul în care se desenează pictograma, în interiorul slotului elementului. Se strânge
    ' dacă rândul e mai scund decât latura nominală; Rectangle.Empty = nimic de desenat.
    ' În starea „doar pictograme" nu mai există text lângă care să stea, deci se CENTREAZĂ.
    Private Function IconRect(it As KBotNavItem, r As Rectangle) As Rectangle
        If IconSlotWidth(it) = 0 Then Return Rectangle.Empty
        Dim side As Integer = Math.Min(IconSide(), r.Height - ThemeShapes.ScaleDpi(Me, 8))
        If side <= 0 Then Return Rectangle.Empty
        Dim top As Integer = r.Top + (r.Height - side) \ 2
        If IsIconsCollapsed() Then Return New Rectangle(r.Left + (r.Width - side) \ 2, top, side, side)
        Return New Rectangle(r.Left + ThemeShapes.ScaleDpi(Me, 12), top, side, side)
    End Function

    ''' <summary>
    ''' Raza EFECTIVĂ a colțurilor, în px scalați: cea din <see cref="ItemCornerRadius"/> dacă e
    ''' dată, altfel cea a schemei active. O singură funcție, folosită de butoane, de butonul din
    ''' colț și de eticheta plutitoare — altfel cele trei ar putea să se rotunjească diferit.
    ''' </summary>
    Private Function ItemRadius() As Integer
        Dim logical As Integer = If(_itemCornerRadius >= 0, _itemCornerRadius,
                                    If(_scheme IsNot Nothing, _scheme.Style.CornerRadius, 0))
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logical))
    End Function

    ' Lățimea impusă a butoanelor, scalată (0 = automat). Vezi ItemWidth.
    Private Function FixedItemWidth() As Integer
        If _itemWidth <= 0 Then Return 0
        Return ThemeShapes.ScaleDpi(Me, _itemWidth)
    End Function

    ' ── Strângerea barei (0025-05) ─────────────────────────────────────────────
    ' Ca la pictogramă: o SINGURĂ geometrie, folosită și la layout, și la pictare, și la
    ' lovirea cu mouse-ul — altfel butonul din colț ar putea fi desenat într-un loc și
    ' apăsat în altul.

    ' Aerul din jurul butoanelor, în px scalați.
    Private Function ScaledPadding() As System.Windows.Forms.Padding
        Return New System.Windows.Forms.Padding(ThemeShapes.ScaleDpi(Me, _itemPadding.Left),
                                                ThemeShapes.ScaleDpi(Me, _itemPadding.Top),
                                                ThemeShapes.ScaleDpi(Me, _itemPadding.Right),
                                                ThemeShapes.ScaleDpi(Me, _itemPadding.Bottom))
    End Function

    Private Function IsIconsCollapsed() As Boolean
        Return _collapsible AndAlso _collapseState = KBotNavCollapseState.Icons
    End Function

    ''' <summary>
    ''' Pictograma autorată pentru starea CURENTĂ a butonului din colț (0025-06), sau
    ''' <c>Nothing</c> dacă starea aia n-a primit niciuna — caz în care se desenează unghiul.
    ''' «Strâns» înseamnă orice stare în afară de <see cref="KBotNavCollapseState.Expanded"/>:
    ''' din <c>Icons</c> ca și din <c>Complete</c> butonul desfășoară.
    ''' </summary>
    Private Function CollapseButtonImage() As Image
        If _collapseState = KBotNavCollapseState.Expanded Then Return _collapseImageExpanded
        Return _collapseImageCollapsed
    End Function

    ''' <summary>
    ''' Încotro arată unghiul desenat implicit: True = spre desfășurare (deci «>» pe o bară
    ''' verticală), False = spre strângere («&lt;»).
    '''
    ''' Se uită la ORICE stare strânsă, nu doar la <see cref="KBotNavCollapseState.Complete"/>:
    ''' pe <c>Icons</c> unghiul rămânea identic cu cel din starea desfășurată, adică bara strânsă
    ''' arăta tot «&lt;» și părea că butonul nu face nimic — cu atât mai vizibil de când ciclul se
    ''' oprește la <c>Icons</c> (vezi <see cref="CycleCollapse"/>), deci «&lt;» era singurul unghi
    ''' pe care apuca să-l vadă cineva.
    ''' </summary>
    Private Function ChevronPointsToExpand() As Boolean
        Return _collapseState <> KBotNavCollapseState.Expanded
    End Function

    Private Function IsCompletelyCollapsed() As Boolean
        Return _collapsible AndAlso _collapseState = KBotNavCollapseState.Complete
    End Function

    ' Latura butonului din colț și aerul din jurul lui.
    Private Function CollapseButtonSide() As Integer
        Return ThemeShapes.ScaleDpi(Me, _collapseButtonSize)
    End Function

    Private Function CollapseButtonMargin() As Integer
        Return ThemeShapes.ScaleDpi(Me, 6)
    End Function

    Private Function CornerIsLeft() As Boolean
        Return _collapseCorner = KBotNavCorner.TopLeft OrElse _collapseCorner = KBotNavCorner.BottomLeft
    End Function

    Private Function CornerIsTop() As Boolean
        Return _collapseCorner = KBotNavCorner.TopLeft OrElse _collapseCorner = KBotNavCorner.TopRight
    End Function

    ''' <summary>
    ''' Pătratul butonului din colț (<see cref="Rectangle.Empty"/> dacă bara nu e colapsabilă sau
    ''' dacă <see cref="CollapseButtonSize"/> e 0 — vezi acolo de ce «fără buton» e o stare validă).
    ''' </summary>
    Private Function CollapseButtonRect() As Rectangle
        If Not _collapsible Then Return Rectangle.Empty
        Dim side As Integer = CollapseButtonSide()
        If side <= 0 Then Return Rectangle.Empty
        Dim gap As Integer = CollapseButtonMargin()
        Dim x As Integer = If(CornerIsLeft(), gap, Width - gap - side)
        Dim y As Integer = If(CornerIsTop(), gap, Height - gap - side)
        Return New Rectangle(x, y, side, side)
    End Function

    ' Cât mănâncă butonul din AXA PRINCIPALĂ (0 dacă bara nu e colapsabilă). Banda se rezervă
    ' ca butonul să nu stea peste primul/ultimul element. Un buton de latură 0 nu se desenează,
    ' deci nici nu are de ce să-și rezerve bandă (0025-09).
    Private Function CollapseBandExtent() As Integer
        If Not _collapsible OrElse CollapseButtonSide() <= 0 Then Return 0
        Return CollapseButtonSide() + 2 * CollapseButtonMargin()
    End Function

    ' La ce capăt al axei principale cade banda: pe verticală decid colțurile de sus/jos, pe
    ' orizontală cele de stânga/dreapta.
    Private Function CollapseBandAtStart() As Boolean
        Return If(_orientation = KBotNavOrientation.Vertical, CornerIsTop(), CornerIsLeft())
    End Function

    ' Lățimea unui buton în starea „doar pictograme": pătratul pictogramei + aer de o parte și de alta.
    Private Function IconOnlyItemWidth() As Integer
        Return IconSide() + 2 * ThemeShapes.ScaleDpi(Me, 8)
    End Function

    ' Dimensiunea barei pe axa care se strânge, în starea „Complete": «puțin mai mult decât butonul».
    Private Function CompleteCollapsedExtent() As Integer
        Return CollapseButtonSide() + 2 * CollapseButtonMargin()
    End Function

    ' Idem, în starea „Icons": padding + butonul îngust. Niciodată sub „Complete" — butonul din
    ' colț trebuie să încapă în continuare.
    Private Function IconsCollapsedExtent() As Integer
        Dim pad As System.Windows.Forms.Padding = ScaledPadding()
        Return Math.Max(CompleteCollapsedExtent(), pad.Left + pad.Right + IconOnlyItemWidth())
    End Function

    ' Dimensiunea curentă pe axa care se strânge.
    Private Function CurrentExtent() As Integer
        Return If(_orientation = KBotNavOrientation.Vertical, Width, Height)
    End Function

    ' Dimensiunea cerută de starea curentă.
    Private Function TargetExtent() As Integer
        Select Case _collapseState
            Case KBotNavCollapseState.Icons
                Return IconsCollapsedExtent()
            Case KBotNavCollapseState.Complete
                Return CompleteCollapsedExtent()
            Case Else
                Return _expandedExtent
        End Select
    End Function

    ' Aplică dimensiunea stării curente. Steagul oprește OnSizeChanged să confunde dimensiunea
    ' strânsă cu „dimensiunea inițială".
    Private Sub ApplyCollapseExtent()
        Dim target As Integer = TargetExtent()
        If target <= 0 OrElse target = CurrentExtent() Then Return
        _applyingCollapseExtent = True
        Try
            If _orientation = KBotNavOrientation.Vertical Then
                Width = target
            Else
                Height = target
            End If
        Finally
            _applyingCollapseExtent = False
        End Try
    End Sub

    ' Trecerea propriu-zisă într-o stare deja validată.
    Private Sub ApplyCollapseState(value As KBotNavCollapseState)
        If value = _collapseState Then Return
        _collapseState = value
        ApplyCollapseExtent()
        InvalidateLayout()
        RaiseEvent CollapseStateChanged(value)
    End Sub

    ' Ciclul butonului din colț. Spre deosebire de setter, NU aruncă pe „Icons" indisponibil:
    ' îl sare. Un buton care aruncă în fața operatorului fiindcă nicio intrare n-are pictogramă
    ' ar fi o pedeapsă pentru o alegere de autorare.
    Private Sub CycleCollapse()
        Dim [next] As KBotNavCollapseState
        Select Case _collapseState
            Case KBotNavCollapseState.Expanded
                [next] = If(IconsCollapseAvailable, KBotNavCollapseState.Icons, KBotNavCollapseState.Complete)
                'eliminat de mine (project manager). nu imi place cum arata fara nicio iconita!
                'Case KBotNavCollapseState.Icons
                '    [next] = KBotNavCollapseState.Complete
            Case Else
                [next] = KBotNavCollapseState.Expanded
        End Select
        ApplyCollapseState([next])
    End Sub

    ' ── Eticheta plutitoare (0025-07) ──────────────────────────────────────────
    ' Ca la pictogramă și la butonul din colț: geometria stă în funcții pure, folosite și la
    ' afișare și de teste. Fereastra (KBotNavFlyout) e doar randare — decizia «pentru cine, cât de
    ' desfășurată, unde» se ia AICI și rămâne calculabilă fără ecran.

    Private Const FlyoutTickMs As Integer = 15

    ''' <summary>
    ''' Lățimea COMPLETĂ a etichetei: banda pictogramei (adică exact butonul strâns) + textul
    ''' măsurat cu <see cref="MeasureFont"/> + aer la dreapta + pastila badge-ului.
    ''' Se măsoară cu fontul semibold din același motiv ca <see cref="ContentWidth"/>: lățimea nu
    ''' are voie să depindă de care buton e selectat.
    ''' </summary>
    Private Function FlyoutFullWidth(it As KBotNavItem) As Integer
        Dim rail As Integer = Math.Max(1, it.Bounds.Width)
        Dim ts As Size = TextRenderer.MeasureText(If(it.Text, String.Empty), MeasureFont())
        Dim w As Integer = rail + ts.Width + ThemeShapes.ScaleDpi(Me, 12)
        If it.Badge > 0 Then w += ThemeShapes.ScaleDpi(Me, 26)
        Return w
    End Function

    ''' <summary>
    ''' Dreptunghiul etichetei în coordonatele CLIENT ale barei, la un progres dat (0..1). Pleacă
    ''' EXACT din dreptunghiul butonului strâns și crește doar spre dreapta — de aceea la progres 0
    ''' eticheta e nedeosebită de buton, și de aceea desfășurarea arată ca a butonului, nu ca a
    ''' unei note lipite alături.
    ''' </summary>
    Private Function FlyoutClientBounds(index As Integer, progress As Double) As Rectangle
        If index < 0 OrElse index >= _items.Count Then Return Rectangle.Empty
        Dim r As Rectangle = _items(index).Bounds
        If r.Width <= 0 OrElse r.Height <= 0 Then Return Rectangle.Empty
        Dim full As Integer = FlyoutFullWidth(_items(index))
        Dim t As Double = Math.Max(0.0, Math.Min(1.0, progress))
        Dim w As Integer = r.Width + CInt(Math.Round((full - r.Width) * t))
        Return New Rectangle(r.Left, r.Top, Math.Max(r.Width, w), r.Height)
    End Function

    ''' <summary>
    ''' Pentru ce element ar trebui să iasă eticheta la un punct dat (-1 = niciunul). Cere: bară
    ''' strânsă la pictograme, eticheta activată, și un BUTON vizibil cu text sub cursor.
    '''
    ''' Separatorii sunt săriți (n-au ce arăta), la fel butoanele fără text (eticheta n-ar dezvălui
    ''' nimic în plus față de pictograma deja vizibilă). Butoanele DEZACTIVATE primesc etichetă,
    ''' spre deosebire de hover-ul obișnuit: tocmai în starea strânsă e cel mai greu de ghicit ce e
    ''' butonul stins de sub cursor, iar eticheta îl arată stins, cu culorile lui.
    ''' </summary>
    Private Function FlyoutTargetAt(location As Point) As Integer
        If Not _flyoutEnabled Then Return -1
        If Not IsIconsCollapsed() Then Return -1
        If KBotDesignTime.IsDesignTime(Me) Then Return -1
        EnsureLayout()
        For i As Integer = 0 To _items.Count - 1
            Dim it As KBotNavItem = _items(i)
            If it.IsSeparator OrElse Not it.Visible Then Continue For
            If String.IsNullOrEmpty(it.Text) Then Continue For
            If it.Bounds.Contains(location) Then Return i
        Next
        Return -1
    End Function

    ' Cursorul s-a mutat pe alt element (sau pe niciunul): reprogramează. Același element =
    ' nu se atinge nimic, altfel fiecare pixel de mișcare ar reporni temporizarea.
    Private Sub UpdateFlyout(target As Integer)
        If target = _flyoutIndex Then Return
        CancelFlyout()
        If target < 0 Then Return
        _flyoutIndex = target
        If _flyoutDelay <= 0 Then
            BeginFlyoutSlide()
            Return
        End If
        EnsureFlyoutTimers()
        _flyoutDelayTimer.Interval = _flyoutDelay
        _flyoutDelayTimer.Start()
    End Sub

    ''' <summary>Ascunde eticheta și uită elementul pentru care ieșise. Sigur de chemat oricând.</summary>
    Private Sub CancelFlyout()
        _flyoutDelayTimer?.Stop()
        _flyoutAnimTimer?.Stop()
        _flyoutIndex = -1
        _flyoutProgress = 0.0
        If _flyout IsNot Nothing AndAlso _flyout.Visible Then _flyout.Hide()
    End Sub

    ' Pornește desfășurarea. Starea (indexul + progresul) avansează INDEPENDENT de existența unei
    ' ferestre — headless și în designer nu se afișează nimic, dar calculul rămâne verificabil.
    Private Sub BeginFlyoutSlide()
        _flyoutProgress = If(_flyoutSlide <= 0, 1.0, 0.0)
        RenderFlyout()
        If _flyoutSlide <= 0 Then Return
        EnsureFlyoutTimers()
        _flyoutAnimTimer.Interval = FlyoutTickMs
        _flyoutAnimTimer.Start()
    End Sub

    Private Sub EnsureFlyoutTimers()
        If _flyoutDelayTimer Is Nothing Then
            _flyoutDelayTimer = New System.Windows.Forms.Timer()
            AddHandler _flyoutDelayTimer.Tick, AddressOf FlyoutDelayTick
        End If
        If _flyoutAnimTimer Is Nothing Then
            _flyoutAnimTimer = New System.Windows.Forms.Timer() With {.Interval = FlyoutTickMs}
            AddHandler _flyoutAnimTimer.Tick, AddressOf FlyoutAnimTick
        End If
    End Sub

    ' Cronometru = graniță de UI: se loghează și se înghite (nu are cui să arunce mai departe).
    Private Sub FlyoutDelayTick(sender As Object, e As EventArgs)
        Try
            _flyoutDelayTimer.Stop()
            If _flyoutIndex < 0 Then Return
            BeginFlyoutSlide()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNavList.FlyoutDelayTick", ex)
        End Try
    End Sub

    Private Sub FlyoutAnimTick(sender As Object, e As EventArgs)
        Try
            AdvanceFlyout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotNavList.FlyoutAnimTick", ex)
        End Try
    End Sub

    ' Un pas de desfășurare. Scos din handler ca testele să-l poată chema direct, fără cronometru.
    Private Sub AdvanceFlyout()
        If _flyoutIndex < 0 Then
            _flyoutAnimTimer?.Stop()
            Return
        End If
        _flyoutProgress += CDbl(FlyoutTickMs) / Math.Max(1, _flyoutSlide)
        If _flyoutProgress >= 1.0 Then
            _flyoutProgress = 1.0
            _flyoutAnimTimer?.Stop()
        End If
        RenderFlyout()
    End Sub

    ' Partea care chiar atinge ecranul. Se retrage tăcut când nu există fereastră de arătat
    ' (bară fără handle / nepusă pe un formular / design time) — nu e o eroare, e absența unui ecran.
    Private Sub RenderFlyout()
        If _flyoutIndex < 0 OrElse _flyoutIndex >= _items.Count Then Return
        If Not IsIconsCollapsed() Then Return
        If KBotDesignTime.IsDesignTime(Me) Then Return
        If Not IsHandleCreated OrElse Not Visible Then Return
        Dim host As Form = FindForm()
        If host Is Nothing Then Return

        Dim rect As Rectangle = FlyoutClientBounds(_flyoutIndex, _flyoutProgress)
        If rect.IsEmpty Then Return

        Dim it As KBotNavItem = _items(_flyoutIndex)
        If _flyout Is Nothing Then _flyout = New KBotNavFlyout()
        _flyout.SetContent(it.Text, ItemIcon(it, String.Equals(it.Key, _selectedKey, StringComparison.Ordinal)), it.Badge, it.Enabled, BuildFlyoutStyle(it))
        _flyout.Bounds = RectangleToScreen(rect)
        If Not _flyout.Visible Then _flyout.Show(host)
    End Sub

    ' Reîmprospătează eticheta deja afară (schimbare de selecție sau de temă sub cursor).
    Private Sub RefreshFlyout()
        If _flyoutIndex < 0 Then Return
        RenderFlyout()
    End Sub

    ''' <summary>
    ''' Culorile/fonturile/măsurile butonului, gata calculate. Aceleași reguli ca în
    ''' <see cref="OnPaint"/> — selectat = accent soft + accent + semibold, dezactivat = text
    ''' estompat, restul = fundal de hover (elementul E survolat, prin definiție).
    ''' </summary>
    Private Function BuildFlyoutStyle(it As KBotNavItem) As KBotNavFlyoutStyle
        Dim selected As Boolean = String.Equals(it.Key, _selectedKey, StringComparison.Ordinal)
        Return New KBotNavFlyoutStyle With {
            .Fill = If(selected, _selectedFill, _hoverFill),
            .Border = _separatorColor,
            .TextColor = If(Not it.Enabled, _textDisabled, If(selected, _accent, _textNormal)),
            .BadgeFill = _badgeFill,
            .BadgeText = _badgeText,
            .Radius = ItemRadius(),
            .GradientStrength = _itemGradient,
            .RailWidth = it.Bounds.Width,
            .IconSide = IconSide(),
            .PadX = ThemeShapes.ScaleDpi(Me, 12),
            .BadgeHeight = ThemeShapes.ScaleDpi(Me, 18),
            .CaptionFont = If(selected, SemiboldFont(), Font),
            .BadgeFont = Font}
    End Function

    ''' <summary>
    ''' Fontul cu care se MĂSOARĂ un element — întotdeauna cel semibold, adică cel mai LAT font cu
    ''' care poate fi pictat.
    '''
    ''' Aici a stat un defect real (0025-04): elementul SELECTAT se pictează semibold (vezi
    ''' <see cref="OnPaint"/>), dar măsurarea folosea fontul obișnuit. Semibold e mai lat la aceeași
    ''' mărime, deci textul butonului selectat nu mai încăpea în slotul calculat pentru el și se
    ''' tăia cu «…». În designer nu e nimic selectat, deci nimic nu era semibold și totul părea în
    ''' regulă — exact simptomul «în designer arată bine, la rulare e mai gros și nu mai încape».
    '''
    ''' Se măsoară MEREU cu semibold, nu doar pentru cel selectat, ca geometria să NU depindă de
    ''' selecție: altfel butoanele și-ar schimba lățimea la fiecare click, iar bara ar sări sub
    ''' degetul operatorului. Prețul e câțiva pixeli de aer pe butoanele neselectate.
    ''' </summary>
    Private Function MeasureFont() As Font
        Return SemiboldFont()
    End Function

    ''' <summary>
    ''' Lățimea cerută de CONȚINUTUL unui buton: padding + pictogramă + text (măsurat cu
    ''' <see cref="MeasureFont"/>) + pastila badge-ului. Baza pentru <c>AutoSize</c> și pentru
    ''' modul automat al barei orizontale.
    ''' </summary>
    Private Function ContentWidth(it As KBotNavItem) As Integer
        Dim padX As Integer = ThemeShapes.ScaleDpi(Me, 12)
        Dim ts As Size = TextRenderer.MeasureText(If(it.Text, String.Empty), MeasureFont())
        Dim w As Integer = ts.Width + 2 * padX + IconSlotWidth(it)
        If it.Badge > 0 Then w += ThemeShapes.ScaleDpi(Me, 26)
        Return w
    End Function

    ' Extinderea (pe axa principală) a unui element vizibil.
    Private Function ItemExtent(it As KBotNavItem) As Integer
        If it.IsSeparator Then Return SeparatorExtent()
        If _orientation = KBotNavOrientation.Vertical Then Return ItemThickness()
        ' Orizontal: axa principală E lățimea.
        ' 1. AutoSize pe element bate orice — el CERE să încapă.
        If it.AutoSize Then Return ContentWidth(it)
        ' 2. Lățimea impusă pe bară. Explicit înseamnă explicit: nu se mai aplică nici minimul de
        '    48 (acela e o gardă a MĂSURĂRII, nu o limită pentru apelant).
        Dim fixedW As Integer = FixedItemWidth()
        If fixedW > 0 Then Return fixedW
        ' 3. Automat: din conținut, cu minimul istoric de 48.
        Return Math.Max(ContentWidth(it), ThemeShapes.ScaleDpi(Me, 48))
    End Function

    ''' <summary>
    ''' Lățimea TRANSVERSALĂ a unui element pe bara verticală (pe orizontală lățimea e axa
    ''' principală și se rezolvă în <see cref="ItemExtent"/>). Aceeași ordine de precedență:
    ''' <c>AutoSize</c> pe element, apoi <c>ItemWidth</c> pe bară, apoi «umple bara».
    ''' Separatorii nu au conținut, deci ignoră <c>AutoSize</c> — linia lor urmează coloana
    ''' butoanelor, altfel ar ieși din dreptul lor.
    ''' </summary>
    Private Function CrossWidthFor(it As KBotNavItem, crossSpan As Integer) As Integer
        ' „Doar pictograme" bate tot: bara S-A strâns la lățimea unei pictograme, deci nici
        ' AutoSize, nici ItemWidth nu mai au ce lățime să ceară.
        If IsIconsCollapsed() Then Return Math.Min(IconOnlyItemWidth(), crossSpan)
        If it.AutoSize AndAlso Not it.IsSeparator Then Return Math.Min(ContentWidth(it), crossSpan)
        Dim fixedW As Integer = FixedItemWidth()
        If fixedW > 0 Then Return Math.Min(fixedW, crossSpan)
        Return crossSpan
    End Function

    ' (Re)calculează slotul fiecărui element. Butoanele/separatorii ascunși primesc
    ' Rectangle.Empty. Grupul „Near" curge de la început, grupul „Far" de la capăt.
    Private Sub RecalcLayout()
        _layoutValid = True
        For Each it In _items
            it.Bounds = Rectangle.Empty
        Next

        ' „Strânsă complet": nu încape decât butonul din colț. Toate sloturile rămân goale, deci
        ' nici pictarea, nici IndexAt, nici hover-ul nu mai ating vreun buton.
        If IsCompletelyCollapsed() Then Return

        Dim pad As System.Windows.Forms.Padding = ScaledPadding()
        Dim vertical As Boolean = (_orientation = KBotNavOrientation.Vertical)
        Dim mainStart As Integer = If(vertical, pad.Top, pad.Left)
        Dim mainEnd As Integer = If(vertical, Height - pad.Bottom, Width - pad.Right)
        Dim crossStart As Integer = If(vertical, pad.Left, pad.Top)
        Dim crossSpan As Integer = Math.Max(0, If(vertical, Width - pad.Right, Height - pad.Bottom) - crossStart)

        ' Banda rezervată butonului de strângere, la capătul dinspre colțul ales.
        Dim band As Integer = CollapseBandExtent()
        If band > 0 Then
            If CollapseBandAtStart() Then
                mainStart += band
            Else
                mainEnd -= band
            End If
        End If

        ' Grupul Near: de la început spre capăt.
        Dim nearCursor As Integer = mainStart
        ' Grupul Far: se așază de la (capăt - extindereTotală) în ordinea listei.
        Dim farTotal As Integer = 0
        For Each it In _items
            If it.Visible AndAlso it.Align = KBotNavAlign.Far Then farTotal += ItemExtent(it)
        Next
        Dim farCursor As Integer = Math.Max(nearCursor, mainEnd - farTotal)

        For Each it In _items
            If Not it.Visible Then Continue For
            Dim ext As Integer = ItemExtent(it)
            Dim mainPos As Integer
            If it.Align = KBotNavAlign.Far Then
                mainPos = farCursor
                farCursor += ext
            Else
                mainPos = nearCursor
                nearCursor += ext
            End If
            If vertical Then
                ' Pe verticală lățimea e transversală și se decide PER ELEMENT (AutoSize).
                it.Bounds = New Rectangle(crossStart, mainPos, CrossWidthFor(it, crossSpan), ext)
            Else
                it.Bounds = New Rectangle(mainPos, crossStart, ext, crossSpan)
            End If
        Next
    End Sub

    Private Sub EnsureLayout()
        If Not _layoutValid Then RecalcLayout()
    End Sub

    ' ── Cârlige Friend pentru teste (headless, fără ecran) ─────────────────────

    ''' <summary>Friend test hook: forțează recalcularea layout-ului, fără pictare.</summary>
    Friend Sub DebugEnsureLayout()
        EnsureLayout()
    End Sub

    ''' <summary>Friend test hook: slotul calculat al unui element (Rectangle.Empty dacă e ascuns).</summary>
    Friend Function DebugBounds(index As Integer) As Rectangle
        EnsureLayout()
        Return _items(index).Bounds
    End Function

    ''' <summary>Friend test hook: pătratul pictogramei unui element (Rectangle.Empty dacă n-are).</summary>
    Friend Function DebugIconRect(index As Integer) As Rectangle
        EnsureLayout()
        Return IconRect(_items(index), _items(index).Bounds)
    End Function

    ''' <summary>Friend test hook: de unde începe textul unui element (după pictogramă, dacă are).</summary>
    Friend Function DebugTextLeft(index As Integer) As Integer
        EnsureLayout()
        Dim it As KBotNavItem = _items(index)
        Return it.Bounds.Left + ThemeShapes.ScaleDpi(Me, 12) + IconSlotWidth(it)
    End Function

    ''' <summary>Friend test hook: indexul elementului de sub un punct client (-1 = niciunul).</summary>
    Friend Function DebugIndexAt(location As Point) As Integer
        Return IndexAt(location)
    End Function

    ''' <summary>Friend test hook: trimite o tastă pe drumul real de navigare.</summary>
    Friend Sub DebugKeyDown(key As Keys)
        OnKeyDown(New KeyEventArgs(key))
    End Sub

    ''' <summary>Friend test hook: pătratul butonului de strângere (Empty dacă bara nu e colapsabilă).</summary>
    Friend Function DebugCollapseButtonRect() As Rectangle
        Return CollapseButtonRect()
    End Function

    ''' <summary>
    ''' Friend test hook: pictograma pe care ar picta-o ACUM butonul din colț
    ''' (<c>Nothing</c> = se desenează unghiul).
    ''' </summary>
    Friend Function DebugCollapseButtonImage() As Image
        Return CollapseButtonImage()
    End Function

    ''' <summary>Friend test hook: True dacă unghiul butonului arată spre desfășurare.</summary>
    Friend Function DebugCollapseChevronPointsToExpand() As Boolean
        Return ChevronPointsToExpand()
    End Function

    ''' <summary>Friend test hook: click stânga pe drumul real (inclusiv butonul de strângere).</summary>
    Friend Sub DebugClickAt(location As Point)
        OnMouseClick(New MouseEventArgs(MouseButtons.Left, 1, location.X, location.Y, 0))
    End Sub

    ' ── Cârlige pentru eticheta plutitoare (0025-07) ───────────────────────────
    ' Fereastra nu se arată headless (RenderFlyout se retrage fără handle/formular), dar TOATĂ
    ' decizia și geometria rulează — deci exact ele se pot fixa în teste.

    ''' <summary>Friend test hook: mișcare de mouse pe drumul real.</summary>
    Friend Sub DebugMouseMoveTo(location As Point)
        OnMouseMove(New MouseEventArgs(MouseButtons.None, 0, location.X, location.Y, 0))
    End Sub

    ''' <summary>Friend test hook: părăsirea barei cu mouse-ul, pe drumul real.</summary>
    Friend Sub DebugMouseLeave()
        OnMouseLeave(EventArgs.Empty)
    End Sub

    ''' <summary>Friend test hook: pentru ce element ar ieși eticheta la un punct (-1 = niciunul).</summary>
    Friend Function DebugFlyoutTargetAt(location As Point) As Integer
        Return FlyoutTargetAt(location)
    End Function

    ''' <summary>Friend test hook: elementul pentru care eticheta e programată sau afară (-1 = niciunul).</summary>
    Friend Function DebugFlyoutIndex() As Integer
        Return _flyoutIndex
    End Function

    ''' <summary>Friend test hook: cât de desfășurată e eticheta (0..1).</summary>
    Friend Function DebugFlyoutProgress() As Double
        Return _flyoutProgress
    End Function

    ''' <summary>Friend test hook: lățimea completă a etichetei unui element.</summary>
    Friend Function DebugFlyoutFullWidth(index As Integer) As Integer
        EnsureLayout()
        Return FlyoutFullWidth(_items(index))
    End Function

    ''' <summary>Friend test hook: dreptunghiul etichetei (client), la un progres dat.</summary>
    Friend Function DebugFlyoutClientBounds(index As Integer, progress As Double) As Rectangle
        EnsureLayout()
        Return FlyoutClientBounds(index, progress)
    End Function

    ''' <summary>Friend test hook: declanșează temporizarea, fără să aștepte cronometrul.</summary>
    Friend Sub DebugFlyoutFireDelay()
        FlyoutDelayTick(Nothing, EventArgs.Empty)
    End Sub

    ''' <summary>Friend test hook: un pas de desfășurare, fără să aștepte cronometrul.</summary>
    Friend Sub DebugFlyoutTick()
        AdvanceFlyout()
    End Sub

    ''' <summary>Friend test hook: raza efectivă a colțurilor, în px scalați.</summary>
    Friend Function DebugItemRadius() As Integer
        Return ItemRadius()
    End Function

    ''' <summary>Friend test hook: stilul cu care s-ar picta eticheta unui element.</summary>
    Friend Function DebugFlyoutStyle(index As Integer) As KBotNavFlyoutStyle
        EnsureLayout()
        Return BuildFlyoutStyle(_items(index))
    End Function

    ''' <summary>Friend test hook: fereastra etichetei, pictată la lățimea completă (fără s-o arate).</summary>
    Friend Function DebugCreateFlyoutWindow(index As Integer) As Form
        EnsureLayout()
        Dim it As KBotNavItem = _items(index)
        Dim fly As New KBotNavFlyout()
        fly.SetContent(it.Text, ItemIcon(it, String.Equals(it.Key, _selectedKey, StringComparison.Ordinal)), it.Badge, it.Enabled, BuildFlyoutStyle(it))
        Dim r As Rectangle = FlyoutClientBounds(index, 1.0)
        fly.Size = New Size(Math.Max(1, r.Width), Math.Max(1, r.Height))
        Return fly
    End Function

    Private Function IndexAt(location As Point) As Integer
        EnsureLayout()
        For i As Integer = 0 To _items.Count - 1
            Dim it As KBotNavItem = _items(i)
            If it.Visible AndAlso Not it.IsSeparator AndAlso it.Bounds.Contains(location) Then Return i
        Next
        Return -1
    End Function

    ' ── Temă ───────────────────────────────────────────────────────────────────

    ''' <summary>Reaplică culorile schemei.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If scheme Is Nothing Then Return
        _scheme = scheme
        Dim p As ThemePalette = scheme.Palette
        ' Since slice 0049 the selection fill has a slot of its own: NavSelectedBack. On the
        ' neutral schemes that slot is filled with EXACTLY the blend used until now — 14% accent
        ' into SurfaceAlt, computed in ThemePalette.ApplyNeutralCardDefaults rather than copied by
        ' hand — so they paint byte for byte as before, while Modern picks its own colour.
        _selectedFill = p.NavSelectedBackColor
        _accent = p.AccentColor
        _hoverFill = p.ButtonHoverColor
        _textNormal = p.TextDimColor
        _textDisabled = p.DisabledTextColor
        _badgeFill = p.SurfaceColor
        _badgeText = p.TextDimColor
        _separatorColor = p.BorderColor
        BackColor = p.SurfaceColor
        ' LAYOUT too, not only colours: since slice 0049 the item height comes from the scheme
        ' (Style.NavItemHeight), so a scheme change moves every slot. With a bare Invalidate the
        ' bar would repaint in the new colours over the old geometry.
        InvalidateLayout()
        Invalidate()
        RefreshFlyout()                 ' schema s-a schimbat cu eticheta afară
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        _semiboldFont?.Dispose()
        _semiboldFont = Nothing
        InvalidateLayout()
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        ' „Mărimea inițială" la care se întoarce butonul de strângere = ultima mărime avută
        ' DESFĂȘURATĂ (operatorul poate lăți bara înainte s-o strângă). Redimensionările pe
        ' care le facem NOI (_applyingCollapseExtent) nu contează — altfel prima strângere
        ' ar deveni noua „mărime inițială" și bara nu s-ar mai putea desfășura.
        If Not _applyingCollapseExtent AndAlso _collapseState = KBotNavCollapseState.Expanded Then
            _expandedExtent = CurrentExtent()
        End If
        InvalidateLayout()
    End Sub

    ' Fontul selecției: „semibold" derivat lazy din fontul ambient (fallback: bold).
    Private Function SemiboldFont() As Font
        If _semiboldFont Is Nothing Then
            Try
                _semiboldFont = New Font("Segoe UI Semibold", Font.Size)
            Catch ex As Exception
                If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotNavList.SemiboldFont", ex)
                _semiboldFont = New Font(Font, FontStyle.Bold)
            End Try
        End If
        Return _semiboldFont
    End Function

    ' ── Pictare ────────────────────────────────────────────────────────────────

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim designTime As Boolean = KBotDesignTime.IsDesignTime(Me)
        Try
            EnsureLayout()
            Dim g As Graphics = e.Graphics
            g.Clear(BackColor)
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim radius As Integer = ItemRadius()
            Dim padX As Integer = ThemeShapes.ScaleDpi(Me, 12)

            ' Design time only: which keys are wrong. Validation is relaxed inside Visual Studio
            ' (throwing would kill the design surface), so the mistake has to be VISIBLE instead.
            Dim badKeys As HashSet(Of String) = If(designTime, DuplicateKeys(), Nothing)

            ' Starea „doar pictograme": nu mai e loc de text, badge sau elipsă.
            Dim iconsOnly As Boolean = IsIconsCollapsed()

            For i As Integer = 0 To _items.Count - 1
                Dim it As KBotNavItem = _items(i)
                If Not it.Visible Then Continue For
                Dim r As Rectangle = it.Bounds
                If r.Width <= 0 OrElse r.Height <= 0 Then Continue For

                If it.IsSeparator Then
                    DrawSeparator(g, r)
                    Continue For
                End If

                Dim isSelected As Boolean = String.Equals(it.Key, _selectedKey, StringComparison.Ordinal)
                Dim isHover As Boolean = (i = _hoverIndex) AndAlso it.Enabled AndAlso Not isSelected

                ' Fundal: selectat = accent soft; hover = ButtonHover; normal = transparent.
                ' Umplerea trece prin ThemeShapes.FillModern, deci capătă gradientul cerut de
                ' ItemGradient (0 = plat, exact ca înainte) — aceeași funcție ca la etichetă.
                If isSelected OrElse isHover Then
                    Using path As GraphicsPath = ThemeShapes.RoundedRect(r, radius)
                        ThemeShapes.FillModern(g, path, r, If(isSelected, _selectedFill, _hoverFill), _itemGradient)
                    End Using
                End If

                ' Badge (pastilă rotunjită, aliniată dreapta) — desenat înaintea
                ' textului ca să-i putem rezerva lățimea. Într-o bară strânsă la pictograme
                ' pastila n-are unde încăpea: rămâne un punct în colțul din dreapta-sus, ca
                ' informația «are ceva de arătat» să nu dispară de tot.
                Dim textRight As Integer = r.Right - padX
                If it.Badge > 0 AndAlso iconsOnly Then
                    Dim dot As Integer = ThemeShapes.ScaleDpi(Me, 6)
                    Using b As New SolidBrush(_accent)
                        g.FillEllipse(b, r.Right - dot - ThemeShapes.ScaleDpi(Me, 4),
                                      r.Top + ThemeShapes.ScaleDpi(Me, 4), dot, dot)
                    End Using
                ElseIf it.Badge > 0 Then
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

                ' Pictograma (stânga), înaintea textului — ea decide de unde începe textul.
                Dim iconR As Rectangle = IconRect(it, r)
                If Not iconR.IsEmpty Then
                    DrawItemImage(g, ItemIcon(it, isSelected), iconR, it.Enabled)
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
                If iconsOnly Then
                    ' Fără text. Un buton FĂRĂ pictogramă ar rămâne o pată goală, deci primește
                    ' inițiala — starea „Icons" se poate cere de îndată ce UN singur buton are
                    ' pictogramă, iar restul trebuie totuși să se distingă unul de altul.
                    If iconR.IsEmpty AndAlso Not String.IsNullOrEmpty(it.Text) Then
                        TextRenderer.DrawText(g, it.Text.Substring(0, 1).ToUpperInvariant(), SemiboldFont(), r, textColor,
                            TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                    End If
                Else
                    Dim textLeft As Integer = r.Left + padX + IconSlotWidth(it)
                    Dim tr As New Rectangle(textLeft, r.Top, Math.Max(0, textRight - textLeft), r.Height)
                    Dim flags As TextFormatFlags = TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or
                        If(_orientation = KBotNavOrientation.Vertical, TextFormatFlags.Left, TextFormatFlags.HorizontalCenter)
                    TextRenderer.DrawText(g, it.Text, textFont, tr, textColor, flags)
                End If

                ' Marcajul de eroare din designer: cheie vidă sau duplicată.
                If designTime AndAlso
                   (String.IsNullOrWhiteSpace(it.Key) OrElse badKeys.Contains(it.Key)) Then
                    Using pen As New Pen(Color.Red, 2)
                        g.DrawRectangle(pen, r.Left + 1, r.Top + 1, Math.Max(1, r.Width - 3), Math.Max(1, r.Height - 3))
                    End Using
                End If
            Next

            ' Butonul din colț se pictează ULTIMUL: banda lui e rezervată în layout, dar pe o bară
            ' foarte îngustă un buton lat tot i-ar putea intra pe dedesubt.
            DrawCollapseButton(g, radius)
        Catch ex As Exception
            ' Nu logăm din procesul designer-ului (vezi KBotDesignTime): fișierul de erori ar
            ' ajunge lângă devenv.exe și ar fi zgomot, nu diagnostic.
            If Not designTime Then GlobalErrorLog.Write("KBotNavList.OnPaint", ex)
        End Try
    End Sub

    ' Butonul de strângere: fundal doar la hover + pictograma autorată pentru starea curentă
    ' (0025-06) sau, dacă starea aia n-a primit una, un unghi («chevron») care arată încotro merge
    ' următorul click. DESFĂȘURATĂ arată spre începutul axei (stânga pe verticală, sus pe
    ' orizontală) = «strânge»; din ORICE stare strânsă arată invers = «desfășoară».
    Private Sub DrawCollapseButton(g As Graphics, radius As Integer)
        Dim b As Rectangle = CollapseButtonRect()
        If b.IsEmpty OrElse b.Width <= 0 OrElse b.Height <= 0 Then Return

        If _collapseHover Then
            Using path As GraphicsPath = ThemeShapes.RoundedRect(b, Math.Min(radius, b.Width \ 2))
                ThemeShapes.FillModern(g, path, b, _hoverFill, _itemGradient)
            End Using
        End If

        ' Pictograma bate unghiul. Fundalul de hover rămâne — el spune «se poate apăsa», nu
        ' «uite o săgeată», deci n-are de ce să depindă de felul glifei.
        Dim glyph As Image = CollapseButtonImage()
        If glyph IsNot Nothing Then
            ' Aerul din jurul pictogramei e PROPORȚIONAL cu butonul (1/9 din latură = 2 px la
            ' latura implicită de 18), nu fix: altfel un buton de 40 ar purta o pictogramă lipită
            ' de margini, iar unul de 8 ar rămâne fără pictogramă deloc. Unghiul desenat era deja
            ' proporțional (brațul = latura/5), deci acum se scalează amândouă la fel.
            Dim inset As Integer = Math.Max(1, b.Width \ 9)
            Dim dest As New Rectangle(b.Left + inset, b.Top + inset,
                                      Math.Max(1, b.Width - 2 * inset), Math.Max(1, b.Height - 2 * inset))
            DrawItemImage(g, glyph, dest, True)
            Return
        End If

        Dim forward As Boolean = ChevronPointsToExpand()
        Dim cx As Single = b.Left + b.Width / 2.0F
        Dim cy As Single = b.Top + b.Height / 2.0F
        Dim ax As Single = b.Width / 5.0F        ' brațul unghiului
        Dim ay As Single = b.Height / 5.0F
        Dim pts As PointF()
        If _orientation = KBotNavOrientation.Vertical Then
            Dim tip As Single = If(forward, cx + ax, cx - ax)
            Dim tail As Single = If(forward, cx - ax, cx + ax)
            pts = {New PointF(tail, cy - ay), New PointF(tip, cy), New PointF(tail, cy + ay)}
        Else
            Dim tip As Single = If(forward, cy + ay, cy - ay)
            Dim tail As Single = If(forward, cy - ay, cy + ay)
            pts = {New PointF(cx - ax, tail), New PointF(cx, tip), New PointF(cx + ax, tail)}
        End If

        Using pen As New Pen(If(_collapseHover, _accent, _textNormal), Math.Max(1.4F, b.Width / 10.0F))
            pen.StartCap = LineCap.Round
            pen.EndCap = LineCap.Round
            pen.LineJoin = LineJoin.Round
            g.DrawLines(pen, pts)
        End Using
    End Sub

    ' Cheile care apar de mai multe ori pe butoane (separatorii nu contează). Doar design-time.
    Private Function DuplicateKeys() As HashSet(Of String)
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        Dim dup As New HashSet(Of String)(StringComparer.Ordinal)
        For Each it As KBotNavItem In _items
            If it.IsSeparator OrElse String.IsNullOrWhiteSpace(it.Key) Then Continue For
            If Not seen.Add(it.Key) Then dup.Add(it.Key)
        Next
        Return dup
    End Function

    ''' <summary>
    ''' The image an item is painted with: the authored one, or a palette-recoloured version if the
    ''' scheme asked for it (<c>Style.TintIcons</c>, slice 0049).
    '''
    ''' <para>Recolouring is OPT-IN, per scheme, precisely because the K-BOT icon set is not all
    ''' monochrome: poured over a multi-coloured icon, a single palette tone erases the very thing
    ''' that makes it recognisable. A scheme that asks for nothing gets the image untouched, as
    ''' before.</para>
    '''
    ''' <para>The result belongs to <see cref="IconTint"/> and is NOT disposed here — the cache
    ''' empties itself when the scheme changes.</para>
    ''' </summary>
    Private Function ItemIcon(it As KBotNavItem, selected As Boolean) As Image
        If it.Image Is Nothing Then Return Nothing
        If _scheme Is Nothing OrElse _scheme.Style Is Nothing OrElse Not _scheme.Style.TintIcons Then
            Return it.Image
        End If
        Return IconTint.Tint(it.Image, If(selected, _accent, _textNormal))
    End Function

    ' Desenează pictograma scalată în pătratul ei. Pe un element DEZACTIVAT o estompează
    ' (desaturare + alfa), ca imaginea unui Button dezactivat — altfel un buton stins ar avea
    ' text gri lângă o pictogramă în culori vii.
    ' FRIEND din 0025-07: o folosește și eticheta plutitoare (KBotNavFlyout), ca pictograma să fie
    ' desenată și estompată exact la fel în amândouă locurile.
    Friend Shared Sub DrawItemImage(g As Graphics, img As Image, dest As Rectangle, enabled As Boolean)
        g.InterpolationMode = InterpolationMode.HighQualityBicubic
        If enabled Then
            g.DrawImage(img, dest)
            Return
        End If
        Using attrs As New ImageAttributes()
            ' Luminanța standard pe R/G/B (0.299/0.587/0.114) + alfa 45%.
            attrs.SetColorMatrix(New ColorMatrix(New Single()() {
                New Single() {0.299F, 0.299F, 0.299F, 0.0F, 0.0F},
                New Single() {0.587F, 0.587F, 0.587F, 0.0F, 0.0F},
                New Single() {0.114F, 0.114F, 0.114F, 0.0F, 0.0F},
                New Single() {0.0F, 0.0F, 0.0F, 0.45F, 0.0F},
                New Single() {0.0F, 0.0F, 0.0F, 0.0F, 1.0F}}))
            g.DrawImage(img, dest, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attrs)
        End Using
    End Sub

    ' Linia separatorului: pe mijlocul slotului, perpendiculară pe axa principală.
    Private Sub DrawSeparator(g As Graphics, r As Rectangle)
        Dim inset As Integer = ThemeShapes.ScaleDpi(Me, 8)
        Using pen As New Pen(_separatorColor)
            If _orientation = KBotNavOrientation.Vertical Then
                Dim y As Integer = r.Top + r.Height \ 2
                g.DrawLine(pen, r.Left + inset, y, r.Right - inset, y)
            Else
                Dim x As Integer = r.Left + r.Width \ 2
                g.DrawLine(pen, x, r.Top + inset, x, r.Bottom - inset)
            End If
        End Using
    End Sub

    ' ── Mouse ──────────────────────────────────────────────────────────────────

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            ' Butonul din colț e desenat PESTE bandă, deci el ia hover-ul primul.
            Dim onCollapse As Boolean = CollapseButtonRect().Contains(e.Location)
            Dim idx As Integer = If(onCollapse, -1, IndexAt(e.Location))
            If idx >= 0 AndAlso Not _items(idx).Enabled Then idx = -1   ' fără hover pe disabled
            If idx <> _hoverIndex OrElse onCollapse <> _collapseHover Then
                _hoverIndex = idx
                _collapseHover = onCollapse
                Invalidate()
            End If
            ' Eticheta plutitoare are propria țintă: ea iese ȘI pe butoanele dezactivate, și numai
            ' pe bara strânsă (vezi FlyoutTargetAt).
            UpdateFlyout(If(onCollapse, -1, FlyoutTargetAt(e.Location)))
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotNavList.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        CancelFlyout()
        If _hoverIndex <> -1 OrElse _collapseHover Then
            _hoverIndex = -1
            _collapseHover = False
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Focus()
            ' Butonul din colț înaintea elementelor — vezi OnMouseMove. În designer NU se
            ' strânge: ar redimensiona controlul și ar murdări formularul cuiva.
            If CollapseButtonRect().Contains(e.Location) Then
                If Not KBotDesignTime.IsDesignTime(Me) Then CycleCollapse()
                Return
            End If
            Dim idx As Integer = IndexAt(e.Location)
            If idx >= 0 AndAlso _items(idx).Enabled Then SelectIndex(idx)
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotNavList.OnMouseClick", ex)
        End Try
    End Sub

    ' ── Tastatură ──────────────────────────────────────────────────────────────

    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        If keyData = Keys.Up OrElse keyData = Keys.Down OrElse
           keyData = Keys.Left OrElse keyData = Keys.Right Then Return True
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            ' Sus/Jos în vertical, Stânga/Dreapta în orizontal.
            Dim forwardKey As Keys = If(_orientation = KBotNavOrientation.Vertical, Keys.Down, Keys.Right)
            Dim backKey As Keys = If(_orientation = KBotNavOrientation.Vertical, Keys.Up, Keys.Left)
            If e.KeyCode <> forwardKey AndAlso e.KeyCode <> backKey Then Return
            If _items.Count = 0 Then Return
            Dim direction As Integer = If(e.KeyCode = forwardKey, 1, -1)
            Dim start As Integer = FindIndex(_selectedKey)
            ' Caută următorul buton SELECTABIL (vizibil, activ, ne-separator) fără wrap.
            Dim idx As Integer = start + direction
            While idx >= 0 AndAlso idx < _items.Count
                Dim it As KBotNavItem = _items(idx)
                If it.Visible AndAlso it.Enabled AndAlso Not it.IsSeparator Then
                    SelectIndex(idx)
                    Exit While
                End If
                idx += direction
            End While
            e.Handled = True
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotNavList.OnKeyDown", ex)
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
            ' Eticheta e o FEREASTRĂ de nivel înalt: nu e copilul barei, deci nimeni n-o închide
            ' în locul nostru. Pictogramele și fonturile din ea rămân ale barei/apelantului.
            _flyoutDelayTimer?.Stop()
            _flyoutDelayTimer?.Dispose()
            _flyoutDelayTimer = Nothing
            _flyoutAnimTimer?.Stop()
            _flyoutAnimTimer?.Dispose()
            _flyoutAnimTimer = Nothing
            _flyout?.Dispose()
            _flyout = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
