Option Strict On
Imports System.ComponentModel
Imports System.Drawing

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
    Private _columnFont As New Font("Calibri", 9.0F, FontStyle.Regular, GraphicsUnit.Point)
    Private _headerFont As New Font("Calibri", 9.0F, FontStyle.Bold, GraphicsUnit.Point)
    Private _headerTextAlign As ContentAlignment = ContentAlignment.MiddleLeft
    Private _autoSizeMode As KBotAutoSizeMode = KBotAutoSizeMode.Inherit

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

    ''' <summary>Textul din antet.</summary>
    <Category("K-BOT: Header")>
    <Description("Textul afișat în banda de antet.")>
    Public Property HeaderText As String

    ''' <summary>
    ''' Lățimea în pixeli. Nu coboară niciodată sub <see cref="MinWidth"/>.
    '''
    ''' English (slice 0028-05): a write here is the CALLER's width (designer, code, or the
    ''' operator's drag) and is remembered as such — see <see cref="SetLayoutWidth"/>.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Lățimea în pixeli. Se limitează întotdeauna la intervalul [MinWidth, MaxWidth].")>
    <DefaultValue(100)>
    Public Property Width As Integer
        Get
            Return _width
        End Get
        Set(value As Integer)
            ' English (slice 0013): clamp to [MinWidth, MaxWidth] on every write so the
            ' auto-size / fill / shrink passes can assign freely and let the model enforce
            ' the bounds. MaxWidth is never below MinWidth (see the MaxWidth setter).
            _width = ClampWidth(value)
            _authoredWidth = _width
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
    Friend Sub SetLayoutWidth(value As Integer)
        _width = ClampWidth(value)
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
    Private Function ClampWidth(value As Integer) As Integer
        Dim podea As Integer = EffectiveMinWidth
        Dim plafon As Integer = Math.Max(_maxWidth, podea)
        Return Math.Min(Math.Max(value, podea), plafon)
    End Function

    ''' <summary>Lățimea minimă (px). Implicit 40. Ridicarea ei împinge și <see cref="Width"/>.</summary>
    <Category("K-BOT")>
    <Description("Lățimea minimă (px). Ridicarea ei împinge și Width.")>
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
    <Description("Lățimea maximă (px). Implicit nelimitată. Auto-dimensionarea și umplerea nu depășesc niciodată această valoare.")>
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

    ''' <summary>Spațiul (px logici) dintre marginea celulei de antet și prima piesă din ea.</summary>
    Friend Const HeaderIconPad As Integer = 8

    ''' <summary>Spațiul (px logici) dintre o pictogramă de antet și titlu.</summary>
    Friend Const HeaderIconGap As Integer = 4

    ''' <summary>Mărimea implicită a unei pictograme de antet.</summary>
    Friend Shared ReadOnly DefaultHeaderIconSize As New Size(16, 16)

    <Category("K-BOT: Header")>
    <Description("Font-ul antetului.")>
    Public Property HeaderFont As Font
        Get
            Return _headerFont
        End Get
        Set(value As Font)
            If _headerFont Is value Then Return
            _headerFont = value
            Owner?.Invalidate()
        End Set
    End Property

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

    <Category("K-BOT")>
    <Description("Font-ul coloanei selectate ")>
    Public Property ColumnFont As Font
        Get
            Return _columnFont
        End Get
        Set(value As Font)
            If _columnFont Is value Then Return
            _columnFont = value
            Owner?.Invalidate()
        End Set
    End Property

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

    ''' <summary>Mărimea (px) a butonului de filtrare. Implicit 16×16.</summary>
    <Category("K-BOT: Filtrare")>
    <Description("Mărimea (px) a butonului de filtrare din antet.")>
    Public Property ColumnFilterIconSize As Size
        Get
            Return _columnFilterIconSize
        End Get
        Set(value As Size)
            Dim nou As New Size(Math.Max(1, value.Width), Math.Max(1, value.Height))
            If _columnFilterIconSize = nou Then Return
            _columnFilterIconSize = nou
            OnIconsChanged()
        End Set
    End Property

    ' Size nu poate purta <DefaultValue> (atributul cere o constantă) — fără perechea
    ' ShouldSerialize/Reset, designerul ar scrie 16×16 în fiecare formular gazdă.
    Private Function ShouldSerializeColumnFilterIconSize() As Boolean
        Return _columnFilterIconSize <> DefaultFilterIconSize
    End Function

    Private Sub ResetColumnFilterIconSize()
        ColumnFilterIconSize = DefaultFilterIconSize
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
            Dim filtru As Integer = If(FilterEnabled, _columnFilterIconSize.Width, 0)
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

    ''' <summary>
    ''' English (slice 0013): set when the operator has dragged this column's edge. A
    ''' <see cref="KBotAutoSizeMode.ToContent"/> pass leaves such a column alone, but fill /
    ''' shrink still applies to it. Cleared by <c>KBotDataView.ResetColumnSizing</c>.
    ''' </summary>
    Friend Property UserSized As Boolean

    ''' <summary>Vizibilă. Implicit True. False => coloana nu se pictează și nu ocupă spațiu.</summary>
    <Category("K-BOT")>
    <Description("False => coloana nu se pictează și nu ocupă spațiu.")>
    <DefaultValue(True)>
    Public Property Visible As Boolean = True

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
    ''' English (slice 0016): whether the column is actually on screen right now — the caller
    ''' shows it (<see cref="Visible"/>) AND the fit pass has not auto-hidden it. Read-only:
    ''' the caller drives it through <see cref="Visible"/> / <see cref="AutoHide"/>.
    ''' Derived state: never shown in the property grid, never serialized.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsEffectivelyVisible As Boolean
        Get
            Return Visible AndAlso Not AutoHidden
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
