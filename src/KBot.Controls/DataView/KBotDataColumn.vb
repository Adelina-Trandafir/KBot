Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Modelul unei coloane <see cref="KBotDataView"/> (control NELEGAT de date). Controlul
''' deține colecția de coloane; caller-ul o construiește prin <c>AddColumn</c> și apoi
''' citește/scrie proprietățile de aici. Offset-ul X al coloanei e cache-uit de CONTROL
''' (nu se ține aici — depinde de scroll/freeze).
'''
''' English (slice 0025): also authorable from the Visual Studio property grid, through
''' <see cref="KBotDataView.Columns"/> and the stock collection dialog. That is why there is a
''' parameterless constructor and why <see cref="Key"/> / <see cref="ColumnType"/> are no longer
''' <c>ReadOnly</c> — the dialog creates an empty column first and fills it in afterwards. Both
''' setters are guarded once the column belongs to a grid that already holds rows.
''' </summary>
Public NotInheritable Class KBotDataColumn

    Private _key As String
    Private _columnType As KBotColumnType
    Private _minWidth As Integer = 40
    Private _maxWidth As Integer = Integer.MaxValue
    Private _width As Integer = 100
    ' Nothing = «din grilă» (vezi ColumnFont / HeaderFont). Un font construit AICI ar fi fost o
    ' alegere fixată în cod, care ar fi bătut tema și banda pe fiecare coloană, pentru totdeauna.
    Private _columnFont As Font
    Private _headerFont As Font
    Private _headerTextAlign As ContentAlignment = ContentAlignment.MiddleLeft
    Private _autoSizeMode As KBotAutoSizeMode = KBotAutoSizeMode.Inherit
    Private _headerMultiline As Boolean = False

    ''' <summary>
    ''' English (slice 0025): the grid this column belongs to, set by
    ''' <c>KBotDataColumnCollection</c> on insert and cleared on removal. Friend: it is plumbing,
    ''' never a designer property. Nothing => the column is free-floating (the designer's case)
    ''' and the key/type guards do not apply.
    ''' </summary>
    Friend Property Owner As KBotDataView

    ''' <summary>
    ''' English (slice 0028-04): per-column auto-sizing, and it BEATS the grid-wide
    ''' <see cref="KBotDataView.AutoSizeColumnsMode"/> wherever it is set — a column marked
    ''' <see cref="KBotAutoSizeMode.None"/> keeps the width the caller gave it even while the rest
    ''' of the grid measures to content, and a column marked <see cref="KBotAutoSizeMode.ToContent"/>
    ''' is measured even while the grid is in the manual mode. The default,
    ''' <see cref="KBotAutoSizeMode.Inherit"/>, states no opinion, so the grid decides — which is
    ''' why adding this knob changed nothing for existing callers.
    '''
    ''' Only the MEASURING pass is at stake here. <c>ColumnFillMode</c> is a separate knob and
    ''' still spends the leftover / absorbs the overflow across every visible column, exactly as
    ''' it already does for a column the operator has drag-resized.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Auto-dimensionarea coloanei; bate modul grilei. Implicit Inherit (decide grila).")>
    <DefaultValue(KBotAutoSizeMode.Inherit)>
    Public Property AutoSizeMode As KBotAutoSizeMode
        Get
            Return _autoSizeMode
        End Get
        Set(value As KBotAutoSizeMode)
            If Not [Enum].IsDefined(GetType(KBotAutoSizeMode), value) Then
                Throw New ArgumentException($"Mod de auto-dimensionare necunoscut: «{value}».", NameOf(value))
            End If
            If _autoSizeMode = value Then Return
            _autoSizeMode = value
            Owner?.OnColumnAutoSizeModeChanged()
        End Set
    End Property

    ''' <summary>
    ''' Identificator unic și stabil (folosit de API și de evenimente).
    '''
    ''' English (slice 0025): writable, but ONLY while the grid has no rows. Cell values are
    ''' stored per column key inside <see cref="KBotDataRow"/>'s dictionary, so renaming a key
    ''' under populated rows would orphan every stored cell and the column would silently paint
    ''' empty — a no-op that looks like data loss. That is an exception, not a silent skip.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Identificator unic și stabil al coloanei. Nu se poate schimba cât timp grila are rânduri.")>
    Public Property Key As String
        Get
            Return _key
        End Get
        Set(value As String)
            If String.Equals(_key, value, StringComparison.Ordinal) Then Return
            GuardModelChange(NameOf(Key))
            Dim oldKey As String = _key
            _key = value
            Owner?.OnColumnKeyChanged(oldKey, value)
        End Set
    End Property

    ''' <summary>
    ''' Tipul coloanei (determină pictarea/editarea).
    '''
    ''' English (slice 0025): writable under the same "no rows" rule as <see cref="Key"/> — the
    ''' painter and the editor branch on it, and an in-progress edit belongs to the OLD type.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Tipul coloanei — determină cum se pictează și cum se editează. Nu se poate schimba cât timp grila are rânduri.")>
    <DefaultValue(KBotColumnType.Text)>
    Public Property ColumnType As KBotColumnType
        Get
            Return _columnType
        End Get
        Set(value As KBotColumnType)
            If _columnType = value Then Return
            GuardModelChange(NameOf(ColumnType))
            ' Perechea tip × filtrare se verifică din AMÂNDOUĂ direcțiile: altfel s-ar putea aprinde
            ' filtrarea pe o coloană de text și apoi tipul mutat pe Button, ocolind regula.
            If _showColumnFilter Then ValidateFilterable(value)
            _columnType = value
            ' O editare deschisă aparține tipului VECHI — se abandonează, nu se convertește.
            Owner?.CancelEdit()
        End Set
    End Property

    ' Cheia și tipul descriu FORMA datelor: se pot schimba doar pe o grilă fără rânduri.
    Private Sub GuardModelChange(propertyName As String)
        If Owner IsNot Nothing AndAlso Owner.RowCount > 0 Then
            Throw New InvalidOperationException(
                $"«{propertyName}» nu se poate schimba cât timp grila are rânduri: valorile celulelor sunt " &
                "păstrate pe cheia veche și ar rămâne orfane. Golește rândurile întâi (ClearRows).")
        End If
    End Sub

    ''' <summary>
    ''' Titlul coloanei se scrie pe MAI MULTE LINII: se rupe singur între cuvinte la lățimea
    ''' coloanei și respectă și rupturile scrise de mână în <see cref="HeaderText"/> (Enter în
    ''' editorul din grila de proprietăți).
    '''
    ''' <para>Aprinderea nu e o simplă schimbare de desen: banda de antet se ÎNALȚĂ cât cere cea
    ''' mai înaltă coloană cu mai multe linii (vezi <c>KBotDataView.EffectiveHeaderHeight</c>) și
    ''' COBOARĂ la loc când coloana se lărgește și textul încape pe mai puține linii — de aceea
    ''' scrierea aici cere o trecere de layout, nu doar o repictare. <c>HeaderHeight</c> rămâne
    ''' MINIMUL benzii; el nu se atinge niciodată.</para>
    '''
    ''' <para>Măsurarea la conținut ține cont și ea: pentru o coloană cu mai multe linii, titlul
    ''' cere doar cât cel mai lung CUVÂNT al lui, nu cât toată propoziția — altfel coloana ar fi
    ''' lărgită exact atât cât să nu mai fie nevoie de rupere, iar proprietatea n-ar face nimic.</para>
    ''' </summary>
    <Category("K-BOT: Header")>
    <Description("Titlul coloanei se scrie pe mai multe linii (rupere între cuvinte + rupturile scrise cu Enter). Banda de antet crește și scade după el.")>
    <DefaultValue(False)>
    Public Property MultiLine As Boolean
        Get
            Return _headerMultiline
        End Get
        Set(value As Boolean)
            If _headerMultiline = value Then Return
            _headerMultiline = value
            Owner?.OnColumnHeaderChanged()
        End Set
    End Property

    <Category("K-BOT: Header")>
    <Description("Alinierea textului din antet.")>
    Public Property HeaderTextAlign As ContentAlignment
        Get
            Return _headerTextAlign
        End Get
        Set(value As ContentAlignment)
            If _headerTextAlign = value Then Return
            _headerTextAlign = value
            Owner?.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Textul din antet.
    '''
    ''' <para><b>Se poate scrie pe mai multe linii, din designer.</b> Grila de proprietăți dă un
    ''' singur rând, iar Enter acolo închide editarea — de aceea proprietatea poartă editorul
    ''' standard cu mai multe linii (săgeata din dreapta valorii deschide o cutie în care Enter
    ''' chiar rupe rândul). Ruptura scrisă cu mâna se respectă DOAR când coloana are
    ''' <see cref="MultiLine"/> aprins; pe o coloană cu o singură linie ea ar fi desenată ca un
    ''' pătrățel, nu ca o ruptură.</para>
    ''' </summary>
    <Category("K-BOT: Header")>
    <Description("Textul afișat în banda de antet. Cu MultiLine aprins, se pot scrie mai multe rânduri (Enter în editorul din dreapta valorii).")>
    <Editor(GetType(System.ComponentModel.Design.MultilineStringEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property HeaderText As String
        Get
            Return _headerText
        End Get
        Set(value As String)
            If String.Equals(_headerText, value, StringComparison.Ordinal) Then Return
            _headerText = value
            ' Nu e doar desen: titlul intră în măsurarea la conținut ȘI în înălțimea benzii
            ' (când coloana e pe mai multe linii), deci cere o trecere de layout.
            Owner?.OnColumnHeaderChanged()
        End Set
    End Property
    Private _headerText As String

    ''' <summary>
    ''' Lățimea în pixeli LOGICI (96 dpi). Nu coboară niciodată sub <see cref="MinWidth"/>.
    '''
    ''' English (slice 0028-05): a write here is the CALLER's width (designer, code, or the
    ''' operator's drag) and is remembered as such — see <see cref="SetLayoutWidth"/>.
    '''
    ''' <para>Felia 0035-01: valoarea e și rămâne în pixeli LOGICI (96 dpi) — asta se scrie în
    ''' designer, asta se citește înapoi. Ce se desenează efectiv pe un ecran la 150% e
    ''' <see cref="WidthPx"/>, derivată la folosire. Dacă numărul ținut aici ar fi cel scalat,
    ''' designerul ar reciti 255 acolo unde s-a scris 170, l-ar îngheța, și la următoarea
    ''' deschidere l-ar scala din nou — capcana <c>ShouldSerialize</c> în formă numerică.</para>
    ''' </summary>
    <Category("K-BOT")>
    <Description("Lățimea în pixeli la 96 dpi (se scalează cu ecranul). Se limitează întotdeauna la intervalul [MinWidth, MaxWidth].")>
    <DefaultValue(100)>
    Public Property Width As Integer
        Get
            Return _width
        End Get
        Set(value As Integer)
            ' English (slice 0013): clamp to [MinWidth, MaxWidth] on every write so the
            ' auto-size / fill / shrink passes can assign freely and let the model enforce
            ' the bounds. MaxWidth is never below MinWidth (see the MaxWidth setter).
            Dim nou As Integer = ClampWidth(value)
            Dim schimbat As Boolean = (nou <> _width) OrElse (nou <> _authoredWidth)
            _width = nou
            _authoredWidth = nou
            ' Scrierea de AICI e a caller-ului, deci grila trebuie să se re-așeze: offset-uri,
            ' bare și — de când există MultiLine — înălțimea benzii de antet, care e o funcție de
            ' lățimi. Trecerea de layout scrie prin SetLayoutWidth, care NU trece pe aici, deci
            ' nu se poate declanșa singură.
            If schimbat Then Owner?.OnColumnWidthChanged()
        End Set
    End Property

    ''' <summary>
    ''' English (slice 0028-05): the width the CALLER asked for, kept apart from the one currently
    ''' painted. Without it the layout pass compounds its own output: a grid that is briefly narrow
    ''' shrinks a column to its floor, and when the space comes back nothing knows what the width
    ''' used to be — the caller's 200px column stays at 65px forever, which reads as «the property
    ''' does not work». The pass restores this baseline before every run
    ''' (<see cref="RestoreAuthoredWidth"/>), so a pass is a function of (authored widths,
    ''' available space) and not of how the window happened to be resized.
    ''' </summary>
    Private _authoredWidth As Integer = 100

    ''' <summary>
    ''' Scrierea făcută de o trecere de layout (măsurare / umplere / strâmtare): schimbă lățimea
    ''' PICTATĂ, dar NU atinge lățimea cerută de caller. Friend: e mecanismul trecerii, nu API.
    ''' </summary>
    ''' <remarks>
    ''' Felia 0035-01: <paramref name="value"/> vine în px de ECRAN — trecerea de layout măsoară
    ''' text real și împarte spațiu real, deci lucrează în pixeli de ecran de la un capăt la
    ''' altul. Se păstrează însă LOGIC, ca tot restul modelului de coloană: altfel valoarea
    ''' scalată ar ajunge în <see cref="Width"/>, iar designerul ar îngheța-o.
    ''' </remarks>
    Friend Sub SetLayoutWidth(value As Integer)
        _width = ClampWidth(UnscalePx(value))
    End Sub

    ''' <summary>
    ''' Ca <see cref="SetLayoutWidth"/>, dar garantat FĂRĂ depășire: lățimea pictată iese cel mult
    ''' <paramref name="value"/>, niciodată peste.
    '''
    ''' <para><b>De ce e nevoie de ea.</b> Lățimea se ține logic și se re-derivă la folosire, deci
    ''' o scriere face dus-întorsul <c>round(v / s) * s</c>. Acela poate ieși cu un pixel PESTE cât
    ''' s-a cerut: la 125%, o cerere de 277 se ține ca 222 logic și se recitește 278. Pentru o
    ''' coloană măsurată după text, un pixel în plus nu se vede. Pentru coloana care ÎNCAPE un
    ''' spațiu dat, acel pixel e o bară de derulare orizontală — s-a văzut exact așa în
    ''' <c>DdfValoriPage</c> la 125%: 639 disponibili, 640 desenați.</para>
    '''
    ''' <para>De aceea rotunjirea de aici e în JOS. Un pixel pierdut nu se vede; unul câștigat
    ''' costă o bară. Podeaua <see cref="EffectiveMinWidth"/> bate totuși coborârea, ca la orice
    ''' scriere de lățime — vezi <c>ClampWidth</c>.</para>
    ''' </summary>
    Friend Sub SetLayoutWidthAtMost(value As Integer)
        Dim s As Single = DpiScale
        If s <= 0 Then
            _width = ClampWidth(value)
            Return
        End If
        _width = ClampWidth(CInt(Math.Floor(value / s)))
    End Sub

    ''' <summary>Readuce lățimea la cea cerută de caller. O cheamă trecerea, la începutul ei.</summary>
    Friend Sub RestoreAuthoredWidth()
        _width = ClampWidth(_authoredWidth)
    End Sub

    ''' <summary>
    ''' Limitarea unei lățimi cerute la intervalul valabil. PODEAUA BATE PLAFONUL: dacă
    ''' <see cref="MaxWidth"/> ar fi sub <see cref="EffectiveMinWidth"/> (un plafon mai mic decât
    ''' cer pictogramele de antet), plafonul cedează — altfel coloana ar fi obligată la o lățime
    ''' la care piesele ei se suprapun, adică la un desen greșit cerut de două proprietăți care
    ''' se contrazic.
    ''' </summary>
    ''' <remarks>Lucrează în px LOGICI, ca tot modelul de coloană.</remarks>
    Private Function ClampWidth(value As Integer) As Integer
        Dim podea As Integer = EffectiveMinWidth
        Dim plafon As Integer = Math.Max(_maxWidth, podea)
        Return Math.Min(Math.Max(value, podea), plafon)
    End Function

    ''' <summary>Lățimea minimă (px). Implicit 40. Ridicarea ei împinge și <see cref="Width"/>.</summary>
    <Category("K-BOT")>
    <Description("Lățimea minimă, în pixeli la 96 dpi (se scalează cu ecranul). Ridicarea ei împinge și Width.")>
    <DefaultValue(40)>
    Public Property MinWidth As Integer
        Get
            Return _minWidth
        End Get
        Set(value As Integer)
            _minWidth = Math.Max(0, value)
            ' English (slice 0013): keep the invariant MinWidth <= MaxWidth, then re-clamp Width.
            If _maxWidth < _minWidth Then _maxWidth = _minWidth
            _width = ClampWidth(_width)
            _authoredWidth = ClampWidth(_authoredWidth)
        End Set
    End Property

    ''' <summary>
    ''' English (slice 0013): maximum width in pixels. Default <see cref="Integer.MaxValue"/>
    ''' (uncapped). Auto-sizing and fill modes never grow a column past this. Kept at or above
    ''' <see cref="MinWidth"/>; lowering it re-clamps <see cref="Width"/>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Lățimea maximă, în pixeli la 96 dpi (se scalează cu ecranul). Implicit nelimitată. Auto-dimensionarea și umplerea nu depășesc niciodată această valoare.")>
    <DefaultValue(Integer.MaxValue)>
    Public Property MaxWidth As Integer
        Get
            Return _maxWidth
        End Get
        Set(value As Integer)
            _maxWidth = Math.Max(value, _minWidth)
            _width = ClampWidth(_width)
            _authoredWidth = ClampWidth(_authoredWidth)
        End Set
    End Property

    ' ── Pictogramele de antet (slice 0028-02) ──────────────────────────────────────
    ' Vezi KBotDataView.HeaderIcons pentru așezare, ordinea sacrificiului și hit-test.

    Private _headerLeftIcon As Image
    Private _headerRightIcon As Image
    Private _headerLeftIconSize As New Size(16, 16)
    Private _headerRightIconSize As New Size(16, 16)
    Private _headerRightIconHoverColor As Color = Color.Empty

    ''' <summary>Spațiul (px logici) dintre marginea celulei de antet și prima PICTOGRAMĂ din ea.</summary>
    Friend Const HeaderIconPad As Integer = 8

    ''' <summary>
    ''' Retragerea (px logici) a TITLULUI de la marginea celulei de antet, pe latura unde nu stă
    ''' nicio pictogramă. Mai mică decât <see cref="HeaderIconPad"/>, și potrivită cu retragerea
    ''' celulelor din corp — cât timp era una singură, antetul stătea vizibil mai retras decât
    ''' coloana de sub el și pierdea 16px din lățimea la care se rupe un titlu pe mai multe linii.
    ''' </summary>
    Friend Const HeaderTextPad As Integer = 4

    ''' <summary>Spațiul (px logici) dintre o pictogramă de antet și titlu.</summary>
    Friend Const HeaderIconGap As Integer = 4

    ''' <summary>Mărimea implicită a unei pictograme de antet.</summary>
    Friend Shared ReadOnly DefaultHeaderIconSize As New Size(16, 16)

    ''' <summary>
    ''' Fontul cu care se scrie titlul ACESTEI coloane. <c>Nothing</c> (implicit) = fontul BENZII:
    ''' cel fixat pe grilă (<c>KBotDataView.HeaderFont</c>) sau, lipsă și el, cel derivat din schema
    ''' activă. Un font pus aici bate banda — dar numai pentru coloana lui.
    '''
    ''' <para><b>Nu e o simplă repictare.</b> Fontul intră și în MĂSURAREA la conținut, și în
    ''' ÎNĂLȚIMEA benzii de antet (un titlu pe mai multe linii scris mai mare cere mai multe
    ''' rânduri), deci scrierea aici cere o trecere de layout. Desenul și amândouă măsurătorile
    ''' citesc aceeași funcție, <c>KBotDataView.HeaderFontFor</c>: două formule ar însemna o
    ''' coloană măsurată cu un font și scrisă cu altul, adică tăiată cu elipsă exact pe titlul
    ''' pentru care tocmai fusese lărgită.</para>
    '''
    ''' <para>Perechea ShouldSerialize/Reset e obligatorie (regula casei): <c>Font</c> nu poate
    ''' purta <c>DefaultValue</c>, deci fără ea designerul ar scrie fontul rezolvat în fiecare
    ''' formular-gazdă, iar valoarea aceea ar citi pe vecie ca alegerea operatorului.</para>
    ''' </summary>
    <Category("K-BOT: Header")>
    <Description("Fontul titlului acestei coloane. Nesetat = fontul benzii de antet (de pe grilă, altfel din temă).")>
    Public Property HeaderFont As Font
        Get
            Return _headerFont
        End Get
        Set(value As Font)
            If _headerFont Is value Then Return
            _headerFont = value
            Owner?.OnColumnHeaderChanged()
        End Set
    End Property

    Private Function ShouldSerializeHeaderFont() As Boolean
        Return _headerFont IsNot Nothing
    End Function

    Private Sub ResetHeaderFont()
        HeaderFont = Nothing
    End Sub

    ''' <summary>
    ''' Pictograma dinaintea titlului de coloană. E un SEMN, nu un buton: nu are eveniment și
    ''' cade prima când coloana se îngustează (vezi <see cref="EffectiveMinWidth"/>).
    ''' </summary>
    <Category("K-BOT: Header")>
    <Description("Pictograma dinaintea titlului de coloană. Decorativă: fără eveniment de apăsare.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property HeaderLeftIcon As Image
        Get
            Return _headerLeftIcon
        End Get
        Set(value As Image)
            If value Is _headerLeftIcon Then Return
            _headerLeftIcon = value
            OnIconsChanged()
        End Set
    End Property

    Private Function ShouldSerializeHeaderLeftIcon() As Boolean
        Return _headerLeftIcon IsNot Nothing
    End Function

    Private Sub ResetHeaderLeftIcon()
        HeaderLeftIcon = Nothing
    End Sub

    ''' <summary>
    ''' Pictograma din dreapta titlului de coloană — cea care se APASĂ (filtru, sortare, meniu de
    ''' coloană): apăsarea ridică <c>KBotDataView.HeaderRightIconClicked</c>. Nu se sacrifică
    ''' niciodată la îngustare.
    ''' </summary>
    <Category("K-BOT: Header")>
    <Description("Pictograma din dreapta titlului. Apăsarea ei ridică HeaderRightIconClicked.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property HeaderRightIcon As Image
        Get
            Return _headerRightIcon
        End Get
        Set(value As Image)
            If value Is _headerRightIcon Then Return
            _headerRightIcon = value
            OnIconsChanged()
        End Set
    End Property

    Private Function ShouldSerializeHeaderRightIcon() As Boolean
        Return _headerRightIcon IsNot Nothing
    End Function

    Private Sub ResetHeaderRightIcon()
        HeaderRightIcon = Nothing
    End Sub

    ' ── ETICHETELE de survolare ale antetului (felia 0035) ────────────────────
    ' Trei texte, fiindcă în antetul unei coloane se pot apăsa trei lucruri diferite: titlul
    ' (care de obicei sortează), pictograma din dreapta și cea de filtrare. Un singur text pentru
    ' toate ar explica ceva ce nu s-a survolat. Toate sunt pe MAI MULTE RÂNDURI și acceptă
    ' marcajele de text îmbogățit — vezi KBotToolTip.

    Private _headerTooltip As String = String.Empty
    ''' <summary>Eticheta titlului de coloană (nu a pictogramelor). Gol = fără etichetă.</summary>
    <Category("K-BOT: Header")>
    <Description("Eticheta de survolare a titlului de coloană (mai multe rânduri; acceptă <b>, <color=#…>).")>
    <Editor(GetType(Design.MultilineStringEditor), GetType(Drawing.Design.UITypeEditor))>
    <DefaultValue("")>
    Public Property HeaderTooltip As String
        Get
            Return _headerTooltip
        End Get
        Set(value As String)
            _headerTooltip = If(value, String.Empty)
        End Set
    End Property

    Private _headerRightIconTooltip As String = String.Empty
    ''' <summary>Eticheta pictogramei din dreapta titlului — butonul propriu-zis al antetului.</summary>
    <Category("K-BOT: Header")>
    <Description("Eticheta de survolare a pictogramei din dreapta titlului (mai multe rânduri).")>
    <Editor(GetType(Design.MultilineStringEditor), GetType(Drawing.Design.UITypeEditor))>
    <DefaultValue("")>
    Public Property HeaderRightIconTooltip As String
        Get
            Return _headerRightIconTooltip
        End Get
        Set(value As String)
            _headerRightIconTooltip = If(value, String.Empty)
        End Set
    End Property

    Private _filterIconTooltip As String = String.Empty
    ''' <summary>
    ''' Eticheta pictogramei de FILTRARE a coloanei. Gol = cea comună a grilei
    ''' (<c>KBotDataView.FilterIconTooltip</c>) — filtrul face același lucru pe orice coloană,
    ''' deci de obicei nu are ce spune diferit.
    ''' </summary>
    <Category("K-BOT: Header")>
    <Description("Eticheta pictogramei de filtrare. Gol = eticheta comună a grilei.")>
    <Editor(GetType(Design.MultilineStringEditor), GetType(Drawing.Design.UITypeEditor))>
    <DefaultValue("")>
    Public Property FilterIconTooltip As String
        Get
            Return _filterIconTooltip
        End Get
        Set(value As String)
            _filterIconTooltip = If(value, String.Empty)
        End Set
    End Property

    ''' <summary>Mărimea (px) a pictogramei din stânga. Implicit 16×16.</summary>
    <Category("K-BOT: Header")>
    <Description("Mărimea (px) a pictogramei din stânga antetului.")>
    Public Property HeaderLeftIconSize As Size
        Get
            Return _headerLeftIconSize
        End Get
        Set(value As Size)
            Dim nou As New Size(Math.Max(1, value.Width), Math.Max(1, value.Height))
            If _headerLeftIconSize = nou Then Return
            _headerLeftIconSize = nou
            OnIconsChanged()
        End Set
    End Property

    ''' <summary>
    ''' Fontul cu care se scriu CELULELE acestei coloane. <c>Nothing</c> (implicit) = fontul
    ''' grilei (<c>KBotDataView.Font</c>), adică cel venit din temă. Perechea antetului, pe
    ''' cealaltă față a coloanei — vezi <see cref="HeaderFont"/>.
    '''
    ''' <para>Ca și acolo, nu e doar desen: fontul intră în măsurarea la conținut, deci scrierea
    ''' aici cere o trecere de layout, iar pictarea, măsurarea și eticheta de depășire citesc toate
    ''' aceeași funcție (<c>KBotDataView.CellFontFor</c>). Cât timp coloana ținea un font construit
    ''' în cod, celulele se scriau cu el iar coloana se măsura cu fontul grilei — două formule, deci
    ''' o coloană tăiată cu elipsă la fontul cel mai mare dintre ele.</para>
    '''
    ''' <para>Rămâne o valoare IMPLICITĂ: handler-ul de <c>CellFormatting</c> o primește în
    ''' <c>Font</c> și o poate schimba pe celula lui.</para>
    ''' </summary>
    <Category("K-BOT")>
    <Description("Fontul celulelor din coloană. Nesetat = fontul grilei (din temă).")>
    Public Property ColumnFont As Font
        Get
            Return _columnFont
        End Get
        Set(value As Font)
            If _columnFont Is value Then Return
            _columnFont = value
            Owner?.OnColumnHeaderChanged()
        End Set
    End Property

    Private Function ShouldSerializeColumnFont() As Boolean
        Return _columnFont IsNot Nothing
    End Function

    Private Sub ResetColumnFont()
        ColumnFont = Nothing
    End Sub

    ' Size nu poate purta <DefaultValue> (atributul cere o constantă) — vezi regula casei:
    ' fără perechea ShouldSerialize/Reset, designerul ar scrie 16×16 în fiecare formular gazdă.
    Private Function ShouldSerializeHeaderLeftIconSize() As Boolean
        Return _headerLeftIconSize <> DefaultHeaderIconSize
    End Function

    Private Sub ResetHeaderLeftIconSize()
        HeaderLeftIconSize = DefaultHeaderIconSize
    End Sub

    ''' <summary>Mărimea (px) a pictogramei din dreapta. Implicit 16×16.</summary>
    <Category("K-BOT: Header")>
    <Description("Mărimea (px) a pictogramei din dreapta antetului.")>
    Public Property HeaderRightIconSize As Size
        Get
            Return _headerRightIconSize
        End Get
        Set(value As Size)
            Dim nou As New Size(Math.Max(1, value.Width), Math.Max(1, value.Height))
            If _headerRightIconSize = nou Then Return
            _headerRightIconSize = nou
            OnIconsChanged()
        End Set
    End Property

    Private Function ShouldSerializeHeaderRightIconSize() As Boolean
        Return _headerRightIconSize <> DefaultHeaderIconSize
    End Function

    Private Sub ResetHeaderRightIconSize()
        HeaderRightIconSize = DefaultHeaderIconSize
    End Sub

    ''' <summary>
    ''' Culoarea de sub pictograma din dreapta cât timp cursorul e peste ea. <c>Color.Empty</c>
    ''' (implicit) = o spălare din culoarea de text a antetului, adică din temă.
    '''
    ''' Doar pictograma din dreapta are hover: ea e cea care răspunde la apăsare, iar o
    ''' evidențiere sub una inertă ar promite o acțiune care nu există.
    ''' </summary>
    <Category("K-BOT: Header")>
    <Description("Culoarea de hover a pictogramei din dreapta. Gol = o spălare din culoarea temei.")>
    Public Property HeaderRightIconHoverColor As Color
        Get
            Return _headerRightIconHoverColor
        End Get
        Set(value As Color)
            If _headerRightIconHoverColor = value Then Return
            _headerRightIconHoverColor = value
            Owner?.Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeHeaderRightIconHoverColor() As Boolean
        Return _headerRightIconHoverColor <> Color.Empty
    End Function

    Private Sub ResetHeaderRightIconHoverColor()
        HeaderRightIconHoverColor = Color.Empty
    End Sub

    ' Pictogramele schimbă podeaua de lățime, deci lățimea curentă se re-limitează pe loc și
    ' grila se re-măsoară. Fără asta, o coloană rămasă îngustă ar picta pictograme suprapuse
    ' până la următoarea trecere de layout.
    Private Sub OnIconsChanged()
        _width = ClampWidth(_width)
        Owner?.OnColumnIconsChanged()
    End Sub

    ' ── Filtrul de coloană (slice 0028-03) ────────────────────────────────────────
    ' Se hotărăște PE COLOANĂ, în designer: fiecare coloană spune singură dacă poartă butonul de
    ' filtrare și cum arată el. Vezi KBotDataView.FilterIcon pentru așezare, pictare și meniu.

    Private _showColumnFilter As Boolean = False
    Private _columnFilterIcon As Image
    Private _columnFilterIconSize As New Size(16, 16)
    ' A scris-o operatorul pe COLOANA asta? Cât timp e False, mărimea vine de la grilă
    ' (KBotDataView.FilterIconSize) — nesetat = «de la gazdă», setat câștigă.
    Private _columnFilterIconSizeSet As Boolean = False
    Private _columnFilterHoverColor As Color = Color.Empty

    ''' <summary>Mărimea implicită a pictogramei de filtrare.</summary>
    Friend Shared ReadOnly DefaultFilterIconSize As New Size(16, 16)

    ''' <summary>
    ''' Coloana poartă butonul de FILTRARE în antet (slice 0028-03)? Implicit False — se aprinde
    ''' coloană cu coloană, din designer.
    '''
    ''' <para><b>Nu se poate aprinde pe <see cref="KBotColumnType.Button"/> și
    ''' <see cref="KBotColumnType.ProgressBar"/>.</b> Acelea nu poartă o valoare pe care s-o cauți:
    ''' o celulă-buton arată o comandă, una de progres arată o fracțiune desenată. O listă de valori
    ''' distincte peste ele ar fi o listă de nimic, iar «sortează A → Z» n-ar avea ce ordona.
    ''' Încercarea ARUNCĂ, nu se stinge tăcut — un buton care nu apare acolo unde a fost cerut e
    ''' exact felul de no-op pe care regula casei îl interzice.</para>
    ''' </summary>
    <Category("K-BOT: Filtrare")>
    <Description("Coloana poartă butonul de filtrare în antet (meniu de sortare + filtrare, ca în Access). Interzis pe coloane Button și ProgressBar.")>
    <DefaultValue(False)>
    Public Property ShowColumnFilter As Boolean
        Get
            Return _showColumnFilter
        End Get
        Set(value As Boolean)
            If _showColumnFilter = value Then Return
            If value Then ValidateFilterable(_columnType)
            _showColumnFilter = value
            OnIconsChanged()
        End Set
    End Property

    ''' <summary>
    ''' The column menu of THIS column offers the «Grupare» tab when the grid has
    ''' <see cref="KBotDataView.EnableGrouping"/> on (slice 0080-02). On by default, so a grid that
    ''' already offers grouping keeps offering it on every filterable column; turned off where a
    ''' column may be filtered but grouping on it makes no sense (a payer's name, say).
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Meniul acestei coloane arată fila «Grupare» (când grila are EnableGrouping).")>
    <DefaultValue(True)>
    Public Property AllowGrouping As Boolean = True

    ''' <summary>
    ''' Imaginea butonului de filtrare. <c>Nothing</c> (implicit) = pâlnia desenată din culoarea
    ''' temei — plină cât timp coloana chiar are un filtru așezat.
    ''' </summary>
    <Category("K-BOT: Filtrare")>
    <Description("Imaginea butonului de filtrare. Nesetată = pâlnia desenată din culoarea temei.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property ColumnFilterIcon As Image
        Get
            Return _columnFilterIcon
        End Get
        Set(value As Image)
            If value Is _columnFilterIcon Then Return
            _columnFilterIcon = value
            Owner?.Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeColumnFilterIcon() As Boolean
        Return _columnFilterIcon IsNot Nothing
    End Function

    Private Sub ResetColumnFilterIcon()
        ColumnFilterIcon = Nothing
    End Sub

    ''' <summary>
    ''' Mărimea (px la 96 dpi) a butonului de filtrare AL ACESTEI COLOANE. Cât timp nu i s-a scris
    ''' nimic, întoarce mărimea grilei (<c>KBotDataView.FilterIconSize</c>, implicit 16×16) — acolo
    ''' se pune o dată, pentru tot antetul; aici doar dacă o coloană chiar are nevoie de altceva.
    ''' </summary>
    <Category("K-BOT: Filtrare")>
    <Description("Mărimea (px @96dpi) a butonului de filtrare pe ACEASTĂ coloană. Nesetată = mărimea grilei (KBotDataView.FilterIconSize).")>
    Public Property ColumnFilterIconSize As Size
        Get
            If _columnFilterIconSizeSet Then Return _columnFilterIconSize
            Return If(Owner Is Nothing, DefaultFilterIconSize, Owner.FilterIconSize)
        End Get
        Set(value As Size)
            Dim nou As New Size(Math.Max(1, value.Width), Math.Max(1, value.Height))
            If _columnFilterIconSizeSet AndAlso _columnFilterIconSize = nou Then Return
            _columnFilterIconSize = nou
            _columnFilterIconSizeSet = True
            OnIconsChanged()
        End Set
    End Property

    ' Size nu poate purta <DefaultValue> (atributul cere o constantă) — fără perechea
    ' ShouldSerialize/Reset, designerul ar scrie 16×16 în fiecare formular gazdă.
    '
    ' Răspunsul e STEAGUL, nu o comparație cu implicitul: o coloană pusă dinadins pe 16×16 într-o
    ' grilă trecută pe 24×24 e o alegere care trebuie să supraviețuiască salvării.
    Private Function ShouldSerializeColumnFilterIconSize() As Boolean
        Return _columnFilterIconSizeSet
    End Function

    ' Reset = «înapoi la mărimea grilei», deci STINGE steagul (nu scrie implicitul prin setter,
    ' care l-ar reaprinde și ar îngheța 16×16 în .Designer.vb).
    Private Sub ResetColumnFilterIconSize()
        If Not _columnFilterIconSizeSet Then Return
        _columnFilterIconSizeSet = False
        _columnFilterIconSize = DefaultFilterIconSize
        OnIconsChanged()
    End Sub

    ''' <summary>
    ''' Culoarea de sub butonul de filtrare cât timp cursorul e peste el. <c>Color.Empty</c>
    ''' (implicit) = o spălare din culoarea de text a antetului, adică din temă.
    ''' </summary>
    <Category("K-BOT: Filtrare")>
    <Description("Culoarea de hover a butonului de filtrare. Gol = o spălare din culoarea temei.")>
    Public Property ColumnFilterHoverColor As Color
        Get
            Return _columnFilterHoverColor
        End Get
        Set(value As Color)
            If _columnFilterHoverColor = value Then Return
            _columnFilterHoverColor = value
            Owner?.Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeColumnFilterHoverColor() As Boolean
        Return _columnFilterHoverColor <> Color.Empty
    End Function

    Private Sub ResetColumnFilterHoverColor()
        ColumnFilterHoverColor = Color.Empty
    End Sub

    ''' <summary>
    ''' Tipurile de coloană pe care filtrarea nu are ce însemna. Verificarea se AMÂNĂ cât timp
    ''' coloana e liberă sau grila e în <c>BeginInit</c>, din exact același motiv ca perechea
    ''' <c>ValueType × Aggregate</c>: designerul emite proprietățile în ordinea LUI, deci
    ''' <c>ShowColumnFilter</c> poate ajunge înaintea lui <c>ColumnType</c>, iar o excepție în
    ''' <c>InitializeComponent</c> ar închide formularul cu totul. Perechea AȘEZATĂ se verifică la
    ''' <c>EndInit</c>, prin <see cref="ValidateSettled"/>.
    ''' </summary>
    Private Sub ValidateFilterable(type As KBotColumnType)
        If Owner Is Nothing OrElse Owner.IsInitializing Then Return
        If Not IsFilterForbidden(type) Then Return
        Throw New ArgumentException(MesajFiltruInterzis(_key, type), NameOf(ShowColumnFilter))
    End Sub

    Private Shared Function IsFilterForbidden(type As KBotColumnType) As Boolean
        Return type = KBotColumnType.Button OrElse type = KBotColumnType.ProgressBar
    End Function

    Private Shared Function MesajFiltruInterzis(key As String, type As KBotColumnType) As String
        Return $"Coloana «{If(key, String.Empty)}» e de tip «{type}», iar pe ea filtrarea nu are ce " &
               "însemna: nu poartă o valoare care să se caute sau să se sorteze. Lasă " &
               "«ShowColumnFilter» pe False pentru coloanele Button și ProgressBar."
    End Function

    ''' <summary>
    ''' Filtrarea e efectiv pornită pe coloana asta? Ține cont ȘI de tip, nu doar de steag: în
    ''' designer perechea poate rămâne nepotrivită (verificarea e amânată), iar pictarea nu are voie
    ''' să deseneze un buton pe care apoi <c>EndInit</c> îl refuză.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property FilterEnabled As Boolean
        Get
            Return _showColumnFilter AndAlso Not IsFilterForbidden(_columnType)
        End Get
    End Property

    ''' <summary>
    ''' Cât cer pictogramele de antet, cu spațiile dintre ele (px logici). 0 = coloana n-are
    ''' pictograme. E podeaua sub care lățimea coloanei nu are voie să coboare.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HeaderIconsWidth As Integer
        Get
            Dim st As Integer = If(_headerLeftIcon IsNot Nothing, _headerLeftIconSize.Width, 0)
            Dim dr As Integer = If(_headerRightIcon IsNot Nothing, _headerRightIconSize.Width, 0)
            ' Prin PROPRIETATE, nu prin câmp: nesetată, mărimea vine de la grilă.
            Dim filtru As Integer = If(FilterEnabled, ColumnFilterIconSize.Width, 0)
            If st = 0 AndAlso dr = 0 AndAlso filtru = 0 Then Return 0
            ' Un spațiu între fiecare pereche de piese vecine care există amândouă.
            Dim piese As Integer = If(st > 0, 1, 0) + If(dr > 0, 1, 0) + If(filtru > 0, 1, 0)
            Dim gap As Integer = Math.Max(0, piese - 1) * HeaderIconGap
            Return 2 * HeaderIconPad + st + dr + filtru + gap
        End Get
    End Property

    ''' <summary>
    ''' Lățimea minimă REALĂ a coloanei: <see cref="MinWidth"/>, dar niciodată sub cât cer
    ''' pictogramele de antet (<see cref="HeaderIconsWidth"/>). Ea limitează scrierile în
    ''' <see cref="Width"/> și o folosesc toate trecerile de auto-dimensionare — o coloană
    ''' strâmtată sub ea și-ar suprapune pictogramele.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveMinWidth As Integer
        Get
            Return Math.Max(_minWidth, HeaderIconsWidth)
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' SCALAREA LA DPI (felia 0035-01)
    ' ══════════════════════════════════════════════════════════════════════════
    '
    ' TOT modelul de coloană — Width, MinWidth, MaxWidth, mărimile de pictogramă — e în pixeli
    ' LOGICI (96 dpi): ce a scris operatorul în designer, ce se serializează în .Designer.vb, ce
    ' întorc proprietățile. Nimic nu s-a schimbat la înțelesul lor.
    '
    ' Grila desenează însă în pixeli de ECRAN, iar textul, fiind în puncte, se scalează singur.
    ' Fără accesoriile de mai jos, o coloană de 170 px rămânea 170 px și la 150%, în timp ce
    ' textul din ea creștea cu jumătate — adică se tăia exact acolo unde operatorul o făcuse
    ' destul de largă.
    '
    ' Scalarea se face deci LA FOLOSIRE, nu la stocare: grila citește `…Px`, iar când trecerea de
    ' layout scrie înapoi o măsură luată de pe ecran (SetLayoutWidth), aceea se aduce la loc în
    ' logic. Dacă valoarea scalată ar fi ținută în `_width`, ea ar ajunge în `Width`, iar
    ' designerul ar citi 255 acolo unde s-a scris 170 și l-ar îngheța — capcana `ShouldSerialize`
    ' în formă numerică.
    '
    ' Scara vine de la GRILĂ (KBotDataView.Dpi.vb). O coloană fără proprietar — cea pe care
    ' designerul tocmai a construit-o, înainte de a o adăuga în colecție — e la scara 1, ceea ce e
    ' și corect: nu se știe încă pe ce ecran va ajunge.

    ''' <summary>Factorul de scalare pe orizontală al grilei care ne deține (1 = 96 dpi).</summary>
    Private ReadOnly Property DpiScale As Single
        Get
            Return If(Owner Is Nothing, 1.0F, Owner.DpiScaleX)
        End Get
    End Property

    ''' <summary>Valoare logică (px @96dpi) → px de ecran, la scara grilei.</summary>
    Friend Function ScalePx(logical As Integer) As Integer
        If logical = Integer.MaxValue Then Return Integer.MaxValue   ' plafon „nelimitat"
        Return CInt(Math.Round(logical * DpiScale))
    End Function

    ''' <summary>Drumul invers: px de ecran → valoare logică.</summary>
    Friend Function UnscalePx(device As Integer) As Integer
        Dim s As Single = DpiScale
        If s <= 0 Then Return device
        Return CInt(Math.Round(device / s))
    End Function

    ''' <summary>Lățimea PICTATĂ (px de ecran). Cea cu care lucrează așezarea și pictarea.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend ReadOnly Property WidthPx As Integer
        Get
            Return ScalePx(_width)
        End Get
    End Property

    ''' <summary>Podeaua, în px de ecran.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend ReadOnly Property EffectiveMinWidthPx As Integer
        Get
            Return Math.Max(ScalePx(_minWidth), HeaderIconsWidthPx)
        End Get
    End Property

    ''' <summary>Plafonul, în px de ecran (<c>Integer.MaxValue</c> rămâne «nelimitat»).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend ReadOnly Property MaxWidthPx As Integer
        Get
            Return ScalePx(_maxWidth)
        End Get
    End Property

    ''' <summary>Mărimea pictogramei din stânga antetului, în px de ecran.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend ReadOnly Property HeaderLeftIconSizePx As Size
        Get
            Return New Size(ScalePx(_headerLeftIconSize.Width), ScalePx(_headerLeftIconSize.Height))
        End Get
    End Property

    ''' <summary>Mărimea pictogramei din dreapta antetului, în px de ecran.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend ReadOnly Property HeaderRightIconSizePx As Size
        Get
            Return New Size(ScalePx(_headerRightIconSize.Width), ScalePx(_headerRightIconSize.Height))
        End Get
    End Property

    ''' <summary>Mărimea pictogramei de filtrare, în px de ecran.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend ReadOnly Property ColumnFilterIconSizePx As Size
        Get
            ' Prin PROPRIETATE: nesetată pe coloană, mărimea logică vine de la grilă.
            Dim logic As Size = ColumnFilterIconSize
            Return New Size(ScalePx(logic.Width), ScalePx(logic.Height))
        End Get
    End Property

    ''' <summary>
    ''' Cât cer pictogramele de antet, în px de ecran. Se calculează din mărimile SCALATE și din
    ''' spațiile scalate, nu prin scalarea totalului logic: altfel podeaua și desenul ar ieși din
    ''' două formule diferite, iar coloana s-ar putea strâmta sub ce chiar se pictează — exact
    ''' scăparea de câțiva pixeli pe care o descria nota de DPI din <c>KBotDataView.HeaderIcons</c>.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend ReadOnly Property HeaderIconsWidthPx As Integer
        Get
            Dim st As Integer = If(_headerLeftIcon IsNot Nothing, HeaderLeftIconSizePx.Width, 0)
            Dim dr As Integer = If(_headerRightIcon IsNot Nothing, HeaderRightIconSizePx.Width, 0)
            Dim filtru As Integer = If(FilterEnabled, ColumnFilterIconSizePx.Width, 0)
            If st = 0 AndAlso dr = 0 AndAlso filtru = 0 Then Return 0
            Dim piese As Integer = If(st > 0, 1, 0) + If(dr > 0, 1, 0) + If(filtru > 0, 1, 0)
            Dim gap As Integer = Math.Max(0, piese - 1) * ScalePx(HeaderIconGap)
            Return 2 * ScalePx(HeaderIconPad) + st + dr + filtru + gap
        End Get
    End Property

    ''' <summary>
    ''' Re-limitează lățimea după o schimbare de scară. Valoarea rămâne logică — se schimbă doar
    ''' PODEAUA, fiindcă <see cref="EffectiveMinWidth"/> ține cont de pictogramele de antet, iar
    ''' rotunjirea lor la alt DPI poate cere un pixel în plus.
    ''' </summary>
    Friend Sub RefreshWidthScale()
        _width = ClampWidth(_width)
    End Sub

    ''' <summary>
    ''' English (slice 0013): set when the operator has dragged this column's edge. A
    ''' <see cref="KBotAutoSizeMode.ToContent"/> pass leaves such a column alone, but fill /
    ''' shrink still applies to it. Cleared by <c>KBotDataView.ResetColumnSizing</c>.
    ''' </summary>
    Friend Property UserSized As Boolean

    ''' <summary>
    ''' Whether the column is on screen: <see cref="KBotColumnVisibility.Visible"/> (default),
    ''' <see cref="KBotColumnVisibility.Hidden"/>, or <see cref="KBotColumnVisibility.WhenRoom"/> —
    ''' hidden until the visible columns leave enough room for it (at least its
    ''' <see cref="MinWidth"/>), then shown by the layout pass; see
    ''' <see cref="KBotDataView.ShowColumnsWhenRoom"/>. An unknown value is a model error and
    ''' raises <c>ArgumentException</c>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Visible = se pictează; Hidden = nu se pictează și nu ocupă spațiu; WhenRoom = ascunsă, dar apare când coloanele vizibile lasă loc cel puțin pentru MinWidth-ul ei.")>
    <DefaultValue(KBotColumnVisibility.Visible)>
    Public Property Visible As KBotColumnVisibility
        Get
            Return _visible
        End Get
        Set(value As KBotColumnVisibility)
            If Not [Enum].IsDefined(GetType(KBotColumnVisibility), value) Then
                Throw New ArgumentException($"Vizibilitate de coloană necunoscută: «{value}».", NameOf(value))
            End If
            If _visible = value Then Return
            _visible = value
            AutoShown = False
            Owner?.OnColumnVisibleChanged()
        End Set
    End Property
    Private _visible As KBotColumnVisibility = KBotColumnVisibility.Visible

    ''' <summary>
    ''' English (slice 0016): the column MAY be auto-hidden when the grid would otherwise need a
    ''' horizontal scrollbar. The fit pass hides auto-hideable columns (rightmost first) until
    ''' the rest fit or none remain; if none remain, the scrollbar appears normally. The fill
    ''' target (<c>ColumnFillMode</c> First/Last) is never auto-hidden — stretching wins over
    ''' hiding. Distinct from <see cref="Visible"/>, which stays the caller's explicit show/hide.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Coloana POATE fi ascunsă automat când grila n-ar încăpea fără bară orizontală (cea mai din dreapta prima).")>
    <DefaultValue(False)>
    Public Property AutoHide As Boolean = False

    ''' <summary>
    ''' English (slice 0016): set by the auto-hide pass when THIS column was hidden for lack of
    ''' room (never by the caller). Recomputed from scratch every layout, so a widened grid
    ''' brings the column back. Friend — the caller toggles <see cref="AutoHide"/>, not this.
    ''' </summary>
    Friend Property AutoHidden As Boolean

    ''' <summary>
    ''' The mirror of <see cref="AutoHidden"/>: set by the layout pass when THIS
    ''' <see cref="KBotColumnVisibility.WhenRoom"/> column was shown because there was room for it
    ''' (never by the caller). Recomputed from scratch every layout, so a narrowed grid hides the
    ''' column again. Friend — the caller chooses <see cref="Visible"/>, not this.
    ''' </summary>
    Friend Property AutoShown As Boolean

    ''' <summary>
    ''' English (slice 0016): whether the column is actually on screen right now. A
    ''' <see cref="KBotColumnVisibility.Visible"/> column is, unless the fit pass auto-hid it; a
    ''' <see cref="KBotColumnVisibility.Hidden"/> one never is; a
    ''' <see cref="KBotColumnVisibility.WhenRoom"/> one is only while the pass has shown it
    ''' (<see cref="AutoShown"/>). Read-only: the caller drives it through <see cref="Visible"/> /
    ''' <see cref="AutoHide"/>. Derived state: never shown in the property grid, never serialized.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsEffectivelyVisible As Boolean
        Get
            Select Case _visible
                Case KBotColumnVisibility.Visible
                    Return Not AutoHidden
                Case KBotColumnVisibility.WhenRoom
                    Return AutoShown
                Case Else
                    Return False
            End Select
        End Get
    End Property

    ''' <summary>Coloană înghețată (non-scrolling): se randează la stânga, înaintea zonei derulate.</summary>
    <Category("K-BOT")>
    <Description("Metadata de coloană înghețată. NOTĂ: mecanismul autoritar e KBotDataView.FrozenColumnCount.")>
    <DefaultValue(False)>
    Public Property Frozen As Boolean = False

    ''' <summary>Coloana nu intră niciodată în editare.</summary>
    <Category("K-BOT")>
    <Description("True => nicio celulă din coloană nu intră în editare.")>
    <DefaultValue(False)>
    Public Property [ReadOnly] As Boolean = False

    ''' <summary>Implicit True. False => întreaga coloană e ștearsă (gri) și inertă.</summary>
    <Category("K-BOT")>
    <Description("False => întreaga coloană e desenată ștearsă și nu răspunde la input.")>
    <DefaultValue(True)>
    Public Property Enabled As Boolean = True

    ''' <summary>Alinierea conținutului (se reutilizează enum-ul WinForms).</summary>
    <Category("K-BOT")>
    <Description("Alinierea conținutului în celulă.")>
    <DefaultValue(ContentAlignment.MiddleLeft)>
    Public Property TextAlign As ContentAlignment = ContentAlignment.MiddleLeft

    ''' <summary>
    ''' Retragerea conținutului față de marginile celulei, pe coloana asta. Implicit
    ''' <c>6, 0, 6, 0</c> — exact cât era fixat în cod până acum, deci o coloană pe care nimeni
    ''' n-a atins-o se pictează neschimbată.
    '''
    ''' <para>Ea hotărăște DREPTUNGHIUL DE CONȚINUT al celulei: acolo se scrie textul, acolo se
    ''' centrează bifa sau butonul de opțiune, și tot de acolo pleacă și MĂSURAREA la conținut —
    ''' altfel o retragere mare ar tăia cu elipsă exact textul pentru care coloana tocmai fusese
    ''' lărgită.</para>
    '''
    ''' <para><b>Nu atinge celulele <see cref="KBotColumnType.Button"/> și
    ''' <see cref="KBotColumnType.ProgressBar"/>.</b> Acelea nu scriu un conținut în celulă, ci
    ''' desenează o FORMĂ (o față rotunjită, o șină), iar marginile lor sunt parte din desenul
    ''' formei, nu o retragere a textului. O retragere pusă peste ele s-ar aduna la marginile
    ''' proprii și ar strâmta butonul fără ca cineva să fi cerut asta. Nici antetul nu se atinge:
    ''' banda de antet are retragerile ei (vezi <see cref="HeaderTextPad"/>).</para>
    ''' </summary>
    <Category("K-BOT")>
    <Description("Retragerea conținutului față de marginile celulei. Implicit 6, 0, 6, 0. Nu se aplică celulelor Button și ProgressBar.")>
    Public Property CellPadding As Padding
        Get
            Return _cellPadding
        End Get
        Set(value As Padding)
            Dim nou As New Padding(Math.Max(0, value.Left), Math.Max(0, value.Top),
                                   Math.Max(0, value.Right), Math.Max(0, value.Bottom))
            If _cellPadding = nou Then Return
            _cellPadding = nou
            ' Retragerea intră în măsurarea la conținut, deci nu e doar o repictare.
            Owner?.OnColumnHeaderChanged()
        End Set
    End Property
    Private _cellPadding As Padding = DefaultCellPadding

    ''' <summary>Retragerea implicită a conținutului unei celule (cea fixată în cod până în 0028-09).</summary>
    Public Shared ReadOnly DefaultCellPadding As New Padding(6, 0, 6, 0)

    ' Padding nu poate purta <DefaultValue> printr-o constantă — vezi regula casei: fără perechea
    ' ShouldSerialize/Reset, designerul ar scrie «6, 0, 6, 0» în fiecare formular gazdă, iar
    ' valoarea aceea ar citi apoi ca o alegere deliberată a operatorului, pe vecie.
    Private Function ShouldSerializeCellPadding() As Boolean
        Return _cellPadding <> DefaultCellPadding
    End Function

    Private Sub ResetCellPadding()
        CellPadding = DefaultCellPadding
    End Sub

    ' -- Cell borders (slice 0085-03) ----------------------------------------------------------
    ' The grid lines ARE the cell borders: by default every cell draws its right and bottom side
    ' in the theme's grid-line colour, which is exactly what the grid always looked like. A column
    ' can change the colour and the sides; a CellFormatting handler can change them per cell.

    ''' <summary>The cell borders of old: the right and the bottom side (the grid lines).</summary>
    Public Const DefaultCellBorders As KBotBorderSides = KBotBorderSides.Right Or KBotBorderSides.Bottom

    Private _cellBorderColor As Color = Color.Empty
    Private _cellBorders As KBotBorderSides = DefaultCellBorders

    ''' <summary>
    ''' Colour of this column's cell borders. <c>Color.Empty</c> (default) = the theme's grid-line
    ''' colour. <c>Color.Transparent</c> = invisible lines. A <c>CellFormatting</c> handler can
    ''' replace it for a single cell (<c>BorderColor</c>).
    ''' </summary>
    <Category("K-BOT")>
    <Description("Border colour of the cells in this column. Empty = the theme's grid-line colour.")>
    Public Property CellBorderColor As Color
        Get
            Return _cellBorderColor
        End Get
        Set(value As Color)
            If _cellBorderColor = value Then Return
            _cellBorderColor = value
            Owner?.Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeCellBorderColor() As Boolean
        Return Not _cellBorderColor.IsEmpty
    End Function

    Private Sub ResetCellBorderColor()
        CellBorderColor = Color.Empty
    End Sub

    ''' <summary>
    ''' Which sides of the cells in this column get a border line (Left, Top, Right, Bottom;
    ''' combine them). Default Right + Bottom = the grid lines. Two neighbouring cells that both
    ''' draw the side they share make a double line: pick one side per edge for a single line.
    ''' A <c>CellFormatting</c> handler can replace it for a single cell (<c>Borders</c>).
    ''' </summary>
    <Category("K-BOT")>
    <Description("Which sides of the cells get a border: Left, Top, Right, Bottom (combine). Default Right + Bottom = the grid lines.")>
    Public Property CellBorders As KBotBorderSides
        Get
            Return _cellBorders
        End Get
        Set(value As KBotBorderSides)
            If (value And Not KBotBorderSides.All) <> KBotBorderSides.None Then
                Throw New ArgumentException($"Unknown cell border sides: {value}.", NameOf(value))
            End If
            If _cellBorders = value Then Return
            _cellBorders = value
            Owner?.Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeCellBorders() As Boolean
        Return _cellBorders <> DefaultCellBorders
    End Function

    Private Sub ResetCellBorders()
        CellBorders = DefaultCellBorders
    End Sub

    ' -- Button cells (slice 0085-02) ---------------------------------------------------------
    ' Everything in this block applies to KBotColumnType.Button only; the other types ignore it.
    ' All pixel metrics (margin, padding, size) are LOGICAL (96 dpi): the grid scales them with
    ' its own DPI when it paints, measures and hit-tests, so one source of truth, no write-back.
    ' Defaults reproduce the button exactly as it was drawn before these properties existed.

    ''' <summary>Outer margin of the button face inside the cell (logical px). Default 4, 3, 4, 3.</summary>
    Public Shared ReadOnly DefaultButtonMargin As New Padding(4, 3, 4, 3)

    Private _buttonText As String = String.Empty
    Private _buttonImage As Image
    Private _buttonBackColor As Color = Color.Empty
    Private _buttonBorderColor As Color = Color.Empty
    Private _buttonBorders As KBotBorderSides = KBotBorderSides.All
    Private _buttonPadding As Padding = Padding.Empty
    Private _buttonMargin As Padding = DefaultButtonMargin
    Private _buttonAlign As ContentAlignment = ContentAlignment.MiddleCenter
    Private _buttonSize As Size = Size.Empty
    Private _buttonFont As Font

    ''' <summary>
    ''' Caption of the button, the same on every row. Empty = the cell's own text, then the column
    ''' header -- unless <see cref="ButtonImage"/> is set, in which case an empty caption stays
    ''' empty (an icon-only button). A <c>CellFormatting</c> handler can still replace it per row.
    ''' </summary>
    <Category("K-BOT: Button")>
    <Description("Caption of the button (same on every row). Empty = cell text, then the column header; with an image set, empty = no caption.")>
    <DefaultValue("")>
    Public Property ButtonText As String
        Get
            Return _buttonText
        End Get
        Set(value As String)
            Dim k_new As String = If(value, String.Empty)
            If String.Equals(_buttonText, k_new, StringComparison.Ordinal) Then Return
            _buttonText = k_new
            Owner?.OnColumnHeaderChanged()
        End Set
    End Property

    ''' <summary>
    ''' Picture on the button, drawn before the caption (or alone, centred, when there is no
    ''' caption). Drawn at its own pixel size scaled by the DPI, and shrunk to fit the button's
    ''' content area keeping its proportions.
    ''' </summary>
    <Category("K-BOT: Button")>
    <Description("Picture on the button, drawn before the caption (or alone when there is no caption).")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property ButtonImage As Image
        Get
            Return _buttonImage
        End Get
        Set(value As Image)
            If value Is _buttonImage Then Return
            _buttonImage = value
            Owner?.OnColumnHeaderChanged()
        End Set
    End Property

    Private Function ShouldSerializeButtonImage() As Boolean
        Return _buttonImage IsNot Nothing
    End Function

    Private Sub ResetButtonImage()
        ButtonImage = Nothing
    End Sub

    ''' <summary>
    ''' Face colour of the button. <c>Color.Empty</c> (default) = from the theme.
    ''' <c>Color.Transparent</c> = a flat button: no face, and no border either unless
    ''' <see cref="ButtonBorderColor"/> is set explicitly. Any other colour is drawn as the face.
    ''' </summary>
    <Category("K-BOT: Button")>
    <Description("Face colour of the button. Empty = from the theme. Transparent = no face, and no border unless ButtonBorderColor is set.")>
    Public Property ButtonBackColor As Color
        Get
            Return _buttonBackColor
        End Get
        Set(value As Color)
            If _buttonBackColor = value Then Return
            _buttonBackColor = value
            Owner?.Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeButtonBackColor() As Boolean
        Return Not _buttonBackColor.IsEmpty
    End Function

    Private Sub ResetButtonBackColor()
        ButtonBackColor = Color.Empty
    End Sub

    ''' <summary>
    ''' Colour of the button border. <c>Color.Empty</c> (default) = from the theme (greyed when the
    ''' cell is disabled). <c>Color.Transparent</c> = no border line. An explicit colour also
    ''' brings the border back on a flat button (<see cref="ButtonBackColor"/> transparent).
    ''' </summary>
    <Category("K-BOT: Button")>
    <Description("Border colour of the button. Empty = from the theme. Transparent = no border.")>
    Public Property ButtonBorderColor As Color
        Get
            Return _buttonBorderColor
        End Get
        Set(value As Color)
            If _buttonBorderColor = value Then Return
            _buttonBorderColor = value
            Owner?.Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeButtonBorderColor() As Boolean
        Return Not _buttonBorderColor.IsEmpty
    End Function

    Private Sub ResetButtonBorderColor()
        ButtonBorderColor = Color.Empty
    End Sub

    ''' <summary>
    ''' Which sides of the button get a border line (Left, Top, Right, Bottom; combine them).
    ''' Default All = the rounded border of old. With any other value the border is drawn as
    ''' straight lines on the chosen sides and the face is a plain rectangle (rounded corners need
    ''' a closed outline).
    ''' </summary>
    <Category("K-BOT: Button")>
    <Description("Which sides of the button get a border: Left, Top, Right, Bottom (combine). All = rounded border. Anything else = straight lines on the chosen sides.")>
    <DefaultValue(KBotBorderSides.All)>
    Public Property ButtonBorders As KBotBorderSides
        Get
            Return _buttonBorders
        End Get
        Set(value As KBotBorderSides)
            If (value And Not KBotBorderSides.All) <> KBotBorderSides.None Then
                Throw New ArgumentException($"Unknown button border sides: {value}.", NameOf(value))
            End If
            If _buttonBorders = value Then Return
            _buttonBorders = value
            Owner?.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' INNER padding: the gap between the button face and its picture / caption (logical px).
    ''' Default 0, 0, 0, 0.
    ''' </summary>
    <Category("K-BOT: Button")>
    <Description("Inner padding between the button face and its picture / caption (logical px). Default 0, 0, 0, 0.")>
    Public Property ButtonPadding As Padding
        Get
            Return _buttonPadding
        End Get
        Set(value As Padding)
            Dim k_new As New Padding(Math.Max(0, value.Left), Math.Max(0, value.Top),
                                     Math.Max(0, value.Right), Math.Max(0, value.Bottom))
            If _buttonPadding = k_new Then Return
            _buttonPadding = k_new
            Owner?.OnColumnHeaderChanged()
        End Set
    End Property

    Private Function ShouldSerializeButtonPadding() As Boolean
        Return _buttonPadding <> Padding.Empty
    End Function

    Private Sub ResetButtonPadding()
        ButtonPadding = Padding.Empty
    End Sub

    ''' <summary>
    ''' OUTER margin: the gap between the cell edges and the button face (logical px). Together
    ''' with <see cref="ButtonAlign"/> and <see cref="ButtonSize"/> it places the button inside
    ''' the cell. Default 4, 3, 4, 3.
    ''' </summary>
    <Category("K-BOT: Button")>
    <Description("Outer margin between the cell edges and the button (logical px). Default 4, 3, 4, 3.")>
    Public Property ButtonMargin As Padding
        Get
            Return _buttonMargin
        End Get
        Set(value As Padding)
            Dim k_new As New Padding(Math.Max(0, value.Left), Math.Max(0, value.Top),
                                     Math.Max(0, value.Right), Math.Max(0, value.Bottom))
            If _buttonMargin = k_new Then Return
            _buttonMargin = k_new
            Owner?.OnColumnHeaderChanged()
        End Set
    End Property

    Private Function ShouldSerializeButtonMargin() As Boolean
        Return _buttonMargin <> DefaultButtonMargin
    End Function

    Private Sub ResetButtonMargin()
        ButtonMargin = DefaultButtonMargin
    End Sub

    ''' <summary>
    ''' Where the button sits inside the cell (inside the area left by <see cref="ButtonMargin"/>).
    ''' It only shows when the button is smaller than that area, i.e. when
    ''' <see cref="ButtonSize"/> is set. Default MiddleCenter.
    ''' </summary>
    <Category("K-BOT: Button")>
    <Description("Where the button sits inside the cell. Only visible when ButtonSize makes the button smaller than the cell.")>
    <DefaultValue(ContentAlignment.MiddleCenter)>
    Public Property ButtonAlign As ContentAlignment
        Get
            Return _buttonAlign
        End Get
        Set(value As ContentAlignment)
            If Not [Enum].IsDefined(GetType(ContentAlignment), value) Then
                Throw New ArgumentException($"Unknown button alignment: {value}.", NameOf(value))
            End If
            If _buttonAlign = value Then Return
            _buttonAlign = value
            Owner?.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Size of the button face (logical px). A dimension left at 0 fills the cell on that axis
    ''' (minus <see cref="ButtonMargin"/>), so the default 0, 0 is the full-cell button of old.
    ''' A size larger than the cell is cut to the cell.
    ''' </summary>
    <Category("K-BOT: Button")>
    <Description("Size of the button (logical px). 0 on an axis = fill the cell on that axis. Default 0, 0.")>
    Public Property ButtonSize As Size
        Get
            Return _buttonSize
        End Get
        Set(value As Size)
            Dim k_new As New Size(Math.Max(0, value.Width), Math.Max(0, value.Height))
            If _buttonSize = k_new Then Return
            _buttonSize = k_new
            Owner?.OnColumnHeaderChanged()
        End Set
    End Property

    Private Function ShouldSerializeButtonSize() As Boolean
        Return _buttonSize <> Size.Empty
    End Function

    Private Sub ResetButtonSize()
        ButtonSize = Size.Empty
    End Sub

    ''' <summary>
    ''' Font of the button caption. <c>Nothing</c> (default) = the column's cell font, then the
    ''' grid font. Like <see cref="ColumnFont"/> it feeds painting AND measuring through
    ''' <c>KBotDataView.CellFontFor</c>, so the column is never measured with one font and drawn
    ''' with another.
    ''' </summary>
    <Category("K-BOT: Button")>
    <Description("Font of the button caption. Not set = the column's cell font, then the grid font.")>
    Public Property ButtonFont As Font
        Get
            Return _buttonFont
        End Get
        Set(value As Font)
            If _buttonFont Is value Then Return
            _buttonFont = value
            Owner?.OnColumnHeaderChanged()
        End Set
    End Property

    Private Function ShouldSerializeButtonFont() As Boolean
        Return _buttonFont IsNot Nothing
    End Function

    Private Sub ResetButtonFont()
        ButtonFont = Nothing
    End Sub

    ''' <summary>
    ''' Format .NET aplicat valorii la afișare (ex. „N2”, „dd.MM.yyyy”). Vid => ToString().
    ''' Portița pentru un format oarecare; pentru lista obișnuită vezi <see cref="Format"/>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Format .NET aplicat valorii la afișare (ex. N2, dd.MM.yyyy). Vid => ToString(). Nu se folosește împreună cu Format.")>
    Public Property FormatString As String
        Get
            Return _formatString
        End Get
        Set(value As String)
            If String.Equals(_formatString, value, StringComparison.Ordinal) Then Return
            ValidateFormatPair(_format, value)
            _formatString = value
            Owner?.OnColumnFormatChanged()
        End Set
    End Property
    Private _formatString As String

    ''' <summary>
    ''' Formatul de afișare NUMIT, în vocabularul proprietății <c>Format</c> a unui textbox Access
    ''' (slice 0028-02): „Standard”, „Percent”, „Short Date”, „Yes/No”… Implicit
    ''' <see cref="KBotFormat.None"/>, adică se folosește <see cref="FormatString"/>.
    '''
    ''' <para>Formatul numit CITEȘTE valoarea în tipul cerut, nu doar o formatează: o coloană de
    ''' text care poartă numere („1234.5”) se scrie „Standard” corect — vezi
    ''' <see cref="KBotColumnFormat"/>. Câte zecimale scrie vine din <see cref="DecimalPlaces"/>
    ''' când e fixat, altfel 2, ca în Access.</para>
    '''
    ''' <para><b>Nu se combină cu <see cref="FormatString"/>.</b> Sunt două fețe ale aceluiași
    ''' lucru, deci amândouă setate ARUNCĂ — niciodată una câștigând tăcut în fața celeilalte, că
    ''' atunci proprietatea nefolosită ar rămâne în formular arătând ca o setare activă.</para>
    ''' </summary>
    <Category("K-BOT")>
    <Description("Formatul de afișare numit, în vocabularul Access (Standard, Percent, Short Date, Yes/No…). Nu se folosește împreună cu FormatString.")>
    <DefaultValue(KBotFormat.None)>
    Public Property Format As KBotFormat
        Get
            Return _format
        End Get
        Set(value As KBotFormat)
            If _format = value Then Return
            ValidateFormatPair(value, _formatString)
            _format = value
            Owner?.OnColumnFormatChanged()
        End Set
    End Property
    Private _format As KBotFormat = KBotFormat.None

    ' Perechea Format × FormatString, verificată LOUD — dar nu în mijlocul unui bloc de
    ' inițializare, din exact același motiv ca perechea tip × agregat: designerul le emite în
    ' ordinea lui, iar o excepție în InitializeComponent ar închide formularul, nu ar corecta
    ' modelul. Perechea AȘEZATĂ se verifică la EndInit (vezi ValidateSettled).
    Private Sub ValidateFormatPair(format As KBotFormat, formatString As String)
        If Owner Is Nothing OrElse Owner.IsInitializing Then Return
        If format = KBotFormat.None OrElse String.IsNullOrEmpty(formatString) Then Return
        Dim argumentException As New ArgumentException(MesajFormatDublu(_key, format, formatString), NameOf(format))
        Throw argumentException
    End Sub

    Private Shared Function MesajFormatDublu(key As String, format As KBotFormat, formatString As String) As String
        Return $"Coloana «{If(key, String.Empty)}» are și «Format» ({format}), și «FormatString» " &
               $"(«{formatString}»). Sunt două fețe ale aceluiași lucru: alege una și lasă cealaltă goală."
    End Function

    ''' <summary>
    ''' Tipul VALORII din coloană (slice 0028) — ce fel de date ține, nu cum se pictează
    ''' (aceea e <see cref="ColumnType"/>). El hotărăște ce agregate are voie coloana să aducă
    ''' în subsol: o coloană de tip <see cref="KBotColumnType.Text"/> poate purta numere, și
    ''' atunci se poate aduna.
    '''
    ''' Schimbarea lui NU stinge tăcut un agregat devenit invalid — aruncă, spunând care sunt
    ''' agregatele permise. Coboară întâi <see cref="Aggregate"/> pe <c>None</c>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Tipul valorilor din coloană (Text/Number/DateTime/Boolean). Hotărăște ce agregate se pot alege în subsol.")>
    <DefaultValue(KBotValueType.Text)>
    <RefreshProperties(RefreshProperties.All)>
    Public Property ValueType As KBotValueType
        Get
            Return _valueType
        End Get
        Set(value As KBotValueType)
            If _valueType = value Then Return
            ' Perechile (tip nou × agregat curent) și (tip nou × zecimale) trebuie să rămână
            ' valide. Verificarea e amânată în designer și în blocul de inițializare — vezi
            ' ValidateAggregate.
            ValidateAggregate(value, _aggregate)
            ValidateDecimalPlaces(value, _decimalPlaces)
            _valueType = value
        End Set
    End Property
    Private _valueType As KBotValueType = KBotValueType.Text

    ''' <summary>
    ''' Câte ZECIMALE se afișează, pentru coloanele numerice (slice 0028). Implicit
    ''' <c>-1</c> = nefixat, adică valoarea se afișează așa cum vine (prin
    ''' <see cref="FormatString"/>, dacă există).
    '''
    ''' Când e fixat, o valoare cu mai multe zecimale se ROTUNJEȘTE — rotunjire NORMALĂ, cea de
    ''' la școală și din contabilitate: 0,5 urcă (<c>MidpointRounding.AwayFromZero</c>). NU e
    ''' implicitul .NET: <c>Math.Round(2.5)</c> dă 2, nu 3, fiindcă rotunjește „la par” — exact
    ''' felul de surpriză care se descoperă într-o notă contabilă, nu într-un test.
    '''
    ''' <para>Rotunjirea e o regulă de AFIȘARE: valoarea stocată în rând rămâne întreagă, cu toate
    ''' zecimalele ei. Dar tot ce se VEDE trece prin ea, inclusiv agregatele din subsol — altfel
    ''' coloana ar arăta trei sume care nu dau totalul de dedesubt, iar pentru cine citește
    ''' pagina asta e pur și simplu o greșeală de calcul.</para>
    '''
    ''' Se poate fixa doar pe coloane <see cref="KBotValueType.Number"/> (altfel n-ar avea ce
    ''' rotunji) și doar în intervalul 0..15, cât acceptă <c>Math.Round</c>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Câte zecimale se afișează (rotunjire normală, 0,5 în sus). -1 = nefixat. Doar pentru coloane Number.")>
    <DefaultValue(-1)>
    Public Property DecimalPlaces As Integer
        Get
            Return _decimalPlaces
        End Get
        Set(value As Integer)
            If _decimalPlaces = value Then Return
            If value > MaxDecimalPlaces Then
                Throw New ArgumentOutOfRangeException(NameOf(DecimalPlaces), value,
                    $"«DecimalPlaces» acceptă cel mult {MaxDecimalPlaces} zecimale (limita Math.Round). -1 = nefixat.")
            End If
            ' Orice negativ înseamnă «nefixat» — se normalizează la -1, ca ShouldSerialize și
            ' comparațiile să aibă o singură formă a stării „gol”.
            Dim normalizat As Integer = If(value < 0, NoDecimalPlaces, value)
            ValidateDecimalPlaces(_valueType, normalizat)
            _decimalPlaces = normalizat
            Owner?.OnColumnAggregateChanged()      ' subsolul se re-formatează cu noua rotunjire
        End Set
    End Property
    Private _decimalPlaces As Integer = NoDecimalPlaces

    ''' <summary>Valoarea lui <see cref="DecimalPlaces"/> care înseamnă «nefixat».</summary>
    Public Const NoDecimalPlaces As Integer = -1

    ''' <summary>Câte zecimale acceptă <c>Math.Round</c> — plafonul lui <see cref="DecimalPlaces"/>.</summary>
    Public Const MaxDecimalPlaces As Integer = 15

    ''' <summary>Coloana are un număr de zecimale fixat de apelant?</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HasDecimalPlaces As Boolean
        Get
            Return _decimalPlaces >= 0
        End Get
    End Property

    ' Zecimalele au sens doar pe numere. Se amână în designer/BeginInit din ACELAȘI motiv ca
    ' perechea tip × agregat: designerul poate emite DecimalPlaces înaintea lui ValueType.
    Private Sub ValidateDecimalPlaces(valueType As KBotValueType, places As Integer)
        If Owner Is Nothing OrElse Owner.IsInitializing Then Return
        If places < 0 OrElse valueType = KBotValueType.Number Then Return
        Dim argumentException As New ArgumentException(
            $"«DecimalPlaces» se poate fixa doar pe o coloană «{NameOf(KBotValueType.Number)}»; " &
            $"coloana «{If(_key, String.Empty)}» e de tip «{valueType}».", NameOf(DecimalPlaces))
        Throw argumentException
    End Sub

    ''' <summary>
    ''' Agregatul pe care coloana îl aduce în banda de subsol. Implicit
    ''' <see cref="KBotAggregate.None"/> (celulă goală ȘI fără separatoare verticale).
    ''' Contează doar când <see cref="KBotDataView.FooterVisible"/> e True.
    '''
    ''' Oferta e filtrată după <see cref="ValueType"/> (vezi <see cref="KBotAggregateRules"/>):
    ''' în grila de proprietăți se văd doar agregatele valabile, iar din cod o pereche nepermisă
    ''' aruncă <see cref="ArgumentException"/> — niciodată o celulă goală în tăcere.
    ''' </summary>
    <Category("K-BOT: Footer")>
    <Description("Ce agregat aduce coloana în subsol. Oferta depinde de ValueType. Contează doar când FooterVisible e True.")>
    <DefaultValue(KBotAggregate.None)>
    <TypeConverter(GetType(KBotAggregateConverter))>
    Public Property Aggregate As KBotAggregate
        Get
            Return _aggregate
        End Get
        Set(value As KBotAggregate)
            If _aggregate = value Then Return
            ValidateAggregate(_valueType, value)
            _aggregate = value
            Owner?.OnColumnAggregateChanged()
        End Set
    End Property
    Private _aggregate As KBotAggregate = KBotAggregate.None

    ''' <summary>
    ''' Perechea tip × agregat, verificată LOUD — dar nu în mijlocul unui bloc de inițializare.
    '''
    ''' Designerul emite proprietățile în ordinea lui, deci <c>Aggregate</c> poate ajunge înaintea
    ''' lui <c>ValueType</c>: o excepție acolo ar arunca din <c>InitializeComponent</c>, adică
    ''' formularul nu s-ar mai deschide DELOC (aceeași capcană pentru care <c>ValidateColumns</c>
    ''' se sare în designer). Cât timp coloana e liberă sau grila e în <c>BeginInit</c>, scrierea
    ''' trece; perechea finală e verificată la <c>EndInit</c>, unde eroarea e a modelului, nu a
    ''' ordinii de emitere.
    ''' </summary>
    Private Sub ValidateAggregate(valueType As KBotValueType, aggregate As KBotAggregate)
        If Owner Is Nothing OrElse Owner.IsInitializing Then Return
        If KBotAggregateRules.IsAllowed(valueType, aggregate) Then Return
        Throw New ArgumentException(KBotAggregateRules.MesajNepermis(_key, valueType, aggregate),
                                    NameOf(Aggregate))
    End Sub

    ''' <summary>
    ''' Verificarea de la <c>KBotDataView.EndInit</c>: perechile AȘEZATE (tip × agregat, tip ×
    ''' zecimale, Format × FormatString), indiferent în ce ordine au sosit proprietățile.
    ''' Friend — o cheamă grila, nu apelantul.
    ''' </summary>
    Friend Sub ValidateSettled()
        If _showColumnFilter AndAlso IsFilterForbidden(_columnType) Then
            Throw New ArgumentException(MesajFiltruInterzis(_key, _columnType), NameOf(ShowColumnFilter))
        End If
        If _decimalPlaces >= 0 AndAlso _valueType <> KBotValueType.Number Then
            Throw New ArgumentException(
                $"«DecimalPlaces» se poate fixa doar pe o coloană «{NameOf(KBotValueType.Number)}»; " &
                $"coloana «{If(_key, String.Empty)}» e de tip «{_valueType}».", NameOf(DecimalPlaces))
        End If
        If _format <> KBotFormat.None AndAlso Not String.IsNullOrEmpty(_formatString) Then
            Throw New ArgumentException(MesajFormatDublu(_key, _format, _formatString), NameOf(Format))
        End If
        If KBotAggregateRules.IsAllowed(_valueType, _aggregate) Then Return
        Throw New ArgumentException(KBotAggregateRules.MesajNepermis(_key, _valueType, _aggregate),
                                    NameOf(Aggregate))
    End Sub

    ''' <summary>
    ''' English (slice 0017-01): optional .NET format string for THIS column's aggregate value.
    ''' When empty, the value-returning aggregates (<see cref="KBotAggregate.Sum"/>,
    ''' <see cref="KBotAggregate.Average"/>, <see cref="KBotAggregate.Min"/>,
    ''' <see cref="KBotAggregate.Max"/>) reuse <see cref="FormatString"/>; the counting ones
    ''' ignore both and always format as a plain integer.
    ''' </summary>
    <Category("K-BOT: Footer")>
    <Description("Format .NET pentru valoarea agregată. Vid => se reia FormatString (numărătorile ignoră ambele).")>
    Public Property AggregateFormatString As String

    ''' <summary>
    ''' Sursa combo partajată pe coloană (override per-celulă prin evenimentul de formatare).
    ''' NU se serializează din designer: o listă de Object nu poate face rotunda prin
    ''' <c>InitializeComponent</c>, iar o sursă pe jumătate serializată e mai rea decât niciuna.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ComboItems As IList(Of Object)

    ''' <summary>
    ''' Grupul de exclusivitate pentru coloanele <see cref="KBotColumnType.OptionButton"/>.
    ''' Bifarea unei opțiuni le stinge pe celelalte din ACELAȘI RÂND care au același grup.
    ''' Vid => opțiunea e independentă (nu stinge nimic).
    ''' </summary>
    <Category("K-BOT")>
    <Description("Grupul de exclusivitate al opțiunilor din același rând. Vid => opțiune independentă.")>
    Public Property OptionGroup As String

    ''' <summary>Minimul barei de progres (doar pentru <see cref="KBotColumnType.ProgressBar"/>).</summary>
    <Category("K-BOT")>
    <Description("Minimul barei de progres (doar pentru coloane ProgressBar).")>
    <DefaultValue(0.0R)>
    Public Property ProgressMin As Double = 0

    ''' <summary>Maximul barei de progres (doar pentru <see cref="KBotColumnType.ProgressBar"/>).</summary>
    <Category("K-BOT")>
    <Description("Maximul barei de progres (doar pentru coloane ProgressBar).")>
    <DefaultValue(100.0R)>
    Public Property ProgressMax As Double = 100

    ''' <summary>Redimensionabilă prin tragerea marginii din antet. Implicit True.</summary>
    <Category("K-BOT")>
    <Description("Redimensionabilă prin tragerea marginii din antet.")>
    <DefaultValue(True)>
    Public Property Resizable As Boolean = True

    ''' <summary>
    ''' Payload al caller-ului (nefolosit de control). NU se serializează din designer —
    ''' un Object arbitrar nu poate face rotunda prin <c>InitializeComponent</c>.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Tag As Object

    ''' <summary>
    ''' English (slice 0025): parameterless constructor, required by the designer's collection
    ''' dialog — it creates the item first and fills in Key / ColumnType afterwards. A column
    ''' built this way is usable the moment it has a key.
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>Cheia + tipul sunt fixate la creare; restul se lasă pe valorile implicite.</summary>
    Public Sub New(key As String, headerText As String, type As KBotColumnType, width As Integer)
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Cheie vidă.", NameOf(key))
        _key = key
        _columnType = type
        ' „Me.” e OBLIGATORIU: VB e case-insensitive, deci parametrul „headerText” ascunde
        ' proprietatea „HeaderText”, iar o atribuire nekalificată s-ar face parametrului.
        Me.HeaderText = If(headerText, String.Empty)
        _width = Math.Max(width, _minWidth)
        _authoredWidth = _width          ' lățimea cerută la creare e tot a caller-ului
    End Sub

    ''' <summary>Ce arată lista dialogului de colecție din designer.</summary>
    Public Overrides Function ToString() As String
        Dim shownKey As String = If(String.IsNullOrWhiteSpace(_key), "<fără cheie>", _key)
        Return shownKey & " — """ & If(HeaderText, String.Empty) & """ (" & _columnType.ToString() & ")"
    End Function

End Class
