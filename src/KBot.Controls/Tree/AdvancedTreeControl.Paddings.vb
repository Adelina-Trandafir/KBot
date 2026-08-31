Imports System.ComponentModel

''' <summary>
''' TOATE marginile/spațierile geometrice ale arborelui, într-un singur loc și expuse în grila
''' de proprietăți sub categoria «K-BOT: Paddings».
'''
''' Erau constante private (PADDING_TREE_START, PADDING_EXPANDER_GAP, …) împrăștiate prin
''' partiale: singura cale de a schimba o margine era recompilarea. Acum sunt proprietăți cu
''' aceleași valori implicite — constantele DEFAULT_* de mai jos ȚIN acele valori, ca
''' <c>DefaultValue</c>-ul din atribut și implicitul câmpului să nu se poată despărți.
'''
''' Contract designer: fiecare are <c>DefaultValue</c> (sau perechea ShouldSerialize/Reset, acolo
''' unde tipul nu e o constantă), deci VS NU scrie nimic în <c>.Designer.vb</c> cât timp
''' operatorul nu schimbă valoarea.
'''
''' AICI STAU TOATE. Dacă adaugi o margine nouă undeva în arbore, mut-o aici — fișierul ăsta e
''' singurul loc în care se caută o spațiere, iar două locuri înseamnă niciunul.
''' </summary>
Partial Public Class AdvancedTreeControl

    ' ── Valorile implicite (fostele constante) ────────────────────────────────
    Private Const DEFAULT_PADDING_TREE_START As Integer = 10
    Private Const DEFAULT_PADDING_SELECTION_LEFT As Integer = 4
    Private Const DEFAULT_PADDING_TREE_TOP As Integer = 5
    Private Const DEFAULT_PADDING_TREE_END As Integer = 4
    Private Const DEFAULT_PADDING_EXPANDER_GAP As Integer = 12
    Private Const DEFAULT_PADDING_TREE_LINE_H_MARGIN As Integer = 4
    Private Const DEFAULT_PADDING_CHECKBOX_GAP As Integer = 8
    Private Const DEFAULT_PADDING_ICON_GAP As Integer = 16
    Private Const DEFAULT_PADDING_SEPARATOR_GAP As Integer = 8
    Private Const DEFAULT_PADDING_TOOLTIP_ICON_HIT As Integer = 3
    Private Const DEFAULT_PADDING_RIGHT_ICON_RIGHT As Integer = 6
    Private Const DEFAULT_PADDING_HEADER_LEFT As Integer = 10

    Private Shared ReadOnly DEFAULT_PADDING_SEARCH_CLEAR_BUTTON As New Padding(2)

    ' ── Câmpurile ─────────────────────────────────────────────────────────────
    Private _paddingTreeStart As Integer = DEFAULT_PADDING_TREE_START
    Private _paddingSelectionLeft As Integer = DEFAULT_PADDING_SELECTION_LEFT
    Private _paddingTreeTop As Integer = DEFAULT_PADDING_TREE_TOP
    Private _paddingTreeEnd As Integer = DEFAULT_PADDING_TREE_END
    Private _paddingExpanderGap As Integer = DEFAULT_PADDING_EXPANDER_GAP
    Private _paddingTreeLineHMargin As Integer = DEFAULT_PADDING_TREE_LINE_H_MARGIN
    Private _paddingCheckBoxGap As Integer = DEFAULT_PADDING_CHECKBOX_GAP
    Private _paddingIconGap As Integer = DEFAULT_PADDING_ICON_GAP
    Private _paddingSeparatorGap As Integer = DEFAULT_PADDING_SEPARATOR_GAP
    Private _paddingTooltipIconHit As Integer = DEFAULT_PADDING_TOOLTIP_ICON_HIT
    Private _rightIconRightPadding As Integer = DEFAULT_PADDING_RIGHT_ICON_RIGHT
    Private _searchClearButtonPadding As Padding = DEFAULT_PADDING_SEARCH_CLEAR_BUTTON
    Private _paddingHeaderLeft As Integer = DEFAULT_PADDING_HEADER_LEFT

    ''' <summary>Marginea din stânga a antetului (bandă de căutare).</summary>
    <Category("K-BOT: Paddings")>
    <Description("Marginea (px) din stânga antetului (bandă de căutare).")>
    <DefaultValue(DEFAULT_PADDING_HEADER_LEFT)>
    Public Property PaddingHeaderLeft As Integer
        Get
            Return _paddingHeaderLeft
        End Get
        Set(value As Integer)
            _paddingHeaderLeft = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>Marginea globală din STÂNGA a întregului arbore (nodul rădăcină nu stă lipit de bordură).</summary>
    <Category("K-BOT: Paddings")>
    <Description("Marginea (px) din stânga întregului arbore — de aici începe grila nivelului 0.")>
    <DefaultValue(DEFAULT_PADDING_TREE_START)>
    Public Property PaddingTreeStart As Integer
        Get
            Return _paddingTreeStart
        End Get
        Set(value As Integer)
            _paddingTreeStart = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>Cât în stânga conținutului începe dreptunghiul de selecție/survolare.</summary>
    <Category("K-BOT: Paddings")>
    <Description("Marginea (px) cu care banda de selecție/hover depășește la stânga conținutul rândului.")>
    <DefaultValue(DEFAULT_PADDING_SELECTION_LEFT)>
    Public Property PaddingSelectionLeft As Integer
        Get
            Return _paddingSelectionLeft
        End Get
        Set(value As Integer)
            _paddingSelectionLeft = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>Spațiul dinaintea primului nod (sub antet/bandă de căutare).</summary>
    <Category("K-BOT: Paddings")>
    <Description("Marginea (px) din vârful arborelui — spațiul de dinaintea primului nod.")>
    <DefaultValue(DEFAULT_PADDING_TREE_TOP)>
    Public Property PaddingTreeTop As Integer
        Get
            Return _paddingTreeTop
        End Get
        Set(value As Integer)
            _paddingTreeTop = Math.Max(0, value)
            ' Intră în înălțimea conținutului, deci bara de derulare trebuie recalculată.
            RefreshScrollVisibility()
        End Set
    End Property

    ''' <summary>Marginea globală din DREAPTA a arborelui (înaintea iconiței de rând / barei de derulare).</summary>
    <Category("K-BOT: Paddings")>
    <Description("Marginea (px) din dreapta întregului arbore — unde se oprește textul nodului.")>
    <DefaultValue(DEFAULT_PADDING_TREE_END)>
    Public Property PaddingTreeEnd As Integer
        Get
            Return _paddingTreeEnd
        End Get
        Set(value As Integer)
            _paddingTreeEnd = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>Spațiul dintre expander/linia de arbore și conținut (checkbox sau iconiță).</summary>
    <Category("K-BOT: Paddings")>
    <Description("Spațiul (px) dintre expander/linia de arbore și conținutul rândului (bifă sau iconiță).")>
    <DefaultValue(DEFAULT_PADDING_EXPANDER_GAP)>
    Public Property PaddingExpanderGap As Integer
        Get
            Return _paddingExpanderGap
        End Get
        Set(value As Integer)
            _paddingExpanderGap = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>Cu cât se oprește linia orizontală de arbore înainte de conținut.</summary>
    <Category("K-BOT: Paddings")>
    <Description("Spațiul (px) dintre capătul liniei orizontale de arbore și conținut (bifă/iconiță).")>
    <DefaultValue(DEFAULT_PADDING_TREE_LINE_H_MARGIN)>
    Public Property PaddingTreeLineHMargin As Integer
        Get
            Return _paddingTreeLineHMargin
        End Get
        Set(value As Integer)
            _paddingTreeLineHMargin = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>Spațiul dintre checkbox/radio și elementul următor (iconiță sau text).</summary>
    <Category("K-BOT: Paddings")>
    <Description("Spațiul (px) dintre bifa nodului și elementul următor (iconiță sau text).")>
    <DefaultValue(DEFAULT_PADDING_CHECKBOX_GAP)>
    Public Property PaddingCheckBoxGap As Integer
        Get
            Return _paddingCheckBoxGap
        End Get
        Set(value As Integer)
            _paddingCheckBoxGap = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>Spațiul dintre iconița din stânga și textul nodului.</summary>
    <Category("K-BOT: Paddings")>
    <Description("Spațiul (px) dintre iconița din stânga și textul nodului.")>
    <DefaultValue(DEFAULT_PADDING_ICON_GAP)>
    Public Property PaddingIconGap As Integer
        Get
            Return _paddingIconGap
        End Get
        Set(value As Integer)
            _paddingIconGap = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>Gapul minim dintre textul din stânga și textul din dreapta (separatorul «~~~»).</summary>
    <Category("K-BOT: Paddings")>
    <Description("Gapul minim (px) dintre capătul textului stâng și începutul textului drept (separator ~~~).")>
    <DefaultValue(DEFAULT_PADDING_SEPARATOR_GAP)>
    Public Property PaddingSeparatorGap As Integer
        Get
            Return _paddingSeparatorGap
        End Get
        Set(value As Integer)
            _paddingSeparatorGap = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Lărgirea zonei de hit-test a iconiței din stânga, folosită când tooltipul apare doar
    ''' peste iconiță (<see cref="TooltipShowOnlyOnLeftIcon"/>).
    ''' </summary>
    <Category("K-BOT: Paddings")>
    <Description("Marja (px) în jurul iconiței din stânga pentru hit-testul tooltipului " &
                 "(când TooltipShowOnlyOnLeftIcon = True).")>
    <DefaultValue(DEFAULT_PADDING_TOOLTIP_ICON_HIT)>
    Public Property PaddingTooltipIconHit As Integer
        Get
            Return _paddingTooltipIconHit
        End Get
        Set(value As Integer)
            _paddingTooltipIconHit = Math.Max(0, value)
        End Set
    End Property

    ''' <summary>Marginea dintre iconița din dreapta nodului și bordura controlului.</summary>
    <Category("K-BOT: Paddings")>
    <Description("Marginea (px) dintre iconița din dreapta și bordura controlului.")>
    <DefaultValue(DEFAULT_PADDING_RIGHT_ICON_RIGHT)>
    Public Property RightIconRightPadding As Integer
        Get
            Return _rightIconRightPadding
        End Get
        Set(value As Integer)
            _rightIconRightPadding = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Spațiul din jurul butonului ✕ al benzii de căutare. E un <see cref="Padding"/>, nu un
    ''' Integer, deci nu poate purta <c>DefaultValue</c> (atributul cere o constantă) — de aici
    ''' perechea ShouldSerialize/Reset, fără de care designerul l-ar scrie în fiecare formular.
    ''' </summary>
    <Category("K-BOT: Paddings")>
    <Description("Spațiul din jurul butonului de golire al benzii de căutare " &
                 "(se adaugă la lățimea rezervată lui).")>
    Public Property SearchClearButtonPadding As Padding
        Get
            Return _searchClearButtonPadding
        End Get
        Set(value As Padding)
            _searchClearButtonPadding = value
            ApplyClearButtonLook()      ' lățimea butonului = glifă/imagine + padding
            If _isSearchMode Then PositionSearchTextBox()
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSearchClearButtonPadding() As Boolean
        Return _searchClearButtonPadding <> DEFAULT_PADDING_SEARCH_CLEAR_BUTTON
    End Function
    Public Sub ResetSearchClearButtonPadding()
        SearchClearButtonPadding = DEFAULT_PADDING_SEARCH_CLEAR_BUTTON
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' ACELEAȘI MARGINI, ÎN PIXELI DE ECRAN (felia 0039)
    ' ══════════════════════════════════════════════════════════════════════════
    '
    ' Ce era stricat: proprietățile de mai sus sunt LOGICE (px la 96 dpi) — asta a tastat
    ' operatorul — dar pictura și așezarea le citeau AȘA CUM SUNT. La 150% rândul creștea, fontul
    ' creștea, iar marginea din stânga, spațiul dintre iconiță și text sau distanța până la
    ' expander rămâneau cât la 96 dpi: totul se înghesuia spre stânga. E fix boala pe care felia
    ' 0035 a scos-o din înălțimi și lățimi, doar că marginile n-au fost prinse atunci.
    '
    ' De ce accesorii, și nu câmpuri scalate: nimeni nu scrie o margine înapoi (spre deosebire de
    ' lățimile de coloană), deci nu e nevoie de o pereche logic/scalat care se poate desincroniza.
    ' Se calculează la citire, ca ThemeShapes.ScaleDpi — un înmulțit și o rotunjire, nimic față de
    ' un DrawString. Scara vine din SX/SY, deci din AppScaling, adică din aceeași sursă unică.
    '
    ' REGULA: cod de pictură / de așezare / de hit-test citește VARIANTA …Px. Proprietatea fără
    ' sufix rămâne pentru designer, pentru serializare și pentru teste. Două citiri diferite ale
    ' aceleiași margini în același calcul înseamnă un buton care se desenează unde nu se apasă.

    Friend ReadOnly Property PaddingHeaderLeftPx As Integer
        Get
            Return SX(_paddingHeaderLeft)
        End Get
    End Property

    Friend ReadOnly Property PaddingTreeStartPx As Integer
        Get
            Return SX(_paddingTreeStart)
        End Get
    End Property

    Friend ReadOnly Property PaddingSelectionLeftPx As Integer
        Get
            Return SX(_paddingSelectionLeft)
        End Get
    End Property

    Friend ReadOnly Property PaddingTreeTopPx As Integer
        Get
            Return SY(_paddingTreeTop)
        End Get
    End Property

    Friend ReadOnly Property PaddingTreeEndPx As Integer
        Get
            Return SX(_paddingTreeEnd)
        End Get
    End Property

    Friend ReadOnly Property PaddingExpanderGapPx As Integer
        Get
            Return SX(_paddingExpanderGap)
        End Get
    End Property

    Friend ReadOnly Property PaddingTreeLineHMarginPx As Integer
        Get
            Return SX(_paddingTreeLineHMargin)
        End Get
    End Property

    Friend ReadOnly Property PaddingCheckBoxGapPx As Integer
        Get
            Return SX(_paddingCheckBoxGap)
        End Get
    End Property

    Friend ReadOnly Property PaddingIconGapPx As Integer
        Get
            Return SX(_paddingIconGap)
        End Get
    End Property

    Friend ReadOnly Property PaddingSeparatorGapPx As Integer
        Get
            Return SX(_paddingSeparatorGap)
        End Get
    End Property

    Friend ReadOnly Property PaddingTooltipIconHitPx As Integer
        Get
            Return SX(_paddingTooltipIconHit)
        End Get
    End Property

    Friend ReadOnly Property RightIconRightPaddingPx As Integer
        Get
            Return SX(_rightIconRightPadding)
        End Get
    End Property

    ''' <summary>
    ''' Cele patru laturi scalate deodată. Butonul ✕ al benzii de căutare e un control REAL, iar
    ''' <c>Control.Padding</c> se cere în pixeli de ecran — de aceea îl primește de aici.
    ''' </summary>
    Friend ReadOnly Property SearchClearButtonPaddingPx As Padding
        Get
            Return New Padding(SX(_searchClearButtonPadding.Left), SY(_searchClearButtonPadding.Top),
                               SX(_searchClearButtonPadding.Right), SY(_searchClearButtonPadding.Bottom))
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' MĂRUNȚIȘURILE DE PICTURĂ (felia 0040)
    ' ══════════════════════════════════════════════════════════════════════════
    '
    ' Numerele scrise direct în pictură — aerul din jurul textului unei celule, bulina de filtru,
    ' inelul de sub un buton survolat, spațiul benzii de căutare. Nu sunt reglaje de operator (nu
    ' se expun în grila de proprietăți), dar sunt tot pixeli LOGICI: la 150% trebuie să crească
    ' odată cu rândul și cu fontul, altfel se repetă exact boala feliilor 0035/0039 — literele
    ' cresc, aerul din jurul lor nu.
    '
    ' Stau AICI, nu în fișierele de pictură, din același motiv ca marginile de mai sus: o spațiere
    ' căutată în două locuri nu e găsită în niciunul.

    ' Aerul stânga/dreapta din interiorul unei celule de coloană (text vs. bordura celulei).
    Private Const DEFAULT_PADDING_CELL_TEXT As Integer = 4
    ' Bulina «filtru activ» din antetul unei coloane: diametrul și distanța față de marginea dreaptă.
    Private Const DEFAULT_COL_FILTER_DOT As Integer = 8
    Private Const DEFAULT_COL_FILTER_DOT_RIGHT As Integer = 13
    ' Banda de căutare: cât aer are peste rând, respectiv peste font, și cât se cere minim casetei.
    Private Const DEFAULT_SEARCH_BAR_ROW_AIR As Integer = 8
    Private Const DEFAULT_SEARCH_BAR_FONT_AIR As Integer = 10
    Private Const DEFAULT_SEARCH_BOX_MIN_WIDTH As Integer = 40
    Private Const DEFAULT_SEARCH_BOX_AIR As Integer = 2
    Private Const DEFAULT_SEARCH_LABEL_GAP As Integer = 4
    ' Butonul de strângere din subsol nu se lipește de marginile benzii.
    Private Const DEFAULT_FOOTER_BUTTON_AIR As Integer = 4
    ' Loaderul («Se încarcă…»): diametrul cercului, grosimea lui și spațiul până la text.
    Private Const DEFAULT_LOADER_SIZE As Integer = 14
    Private Const DEFAULT_LOADER_PEN As Integer = 2
    Private Const DEFAULT_LOADER_TEXT_GAP As Integer = 20
    ' Bifa/expanderul: raza colțurilor casetei, grosimea semnului, marginea liniuței expanderului.
    Private Const DEFAULT_CHECKBOX_RADIUS As Integer = 3
    Private Const DEFAULT_CHECK_MARK_PEN As Integer = 2
    Private Const DEFAULT_EXPANDER_SIGN_INSET As Integer = 2

    Friend ReadOnly Property PaddingCellTextPx As Integer
        Get
            Return SX(DEFAULT_PADDING_CELL_TEXT)
        End Get
    End Property

    Friend ReadOnly Property ColFilterDotSizePx As Integer
        Get
            Return SX(DEFAULT_COL_FILTER_DOT)
        End Get
    End Property

    Friend ReadOnly Property ColFilterDotRightPx As Integer
        Get
            Return SX(DEFAULT_COL_FILTER_DOT_RIGHT)
        End Get
    End Property

    Friend ReadOnly Property SearchBarRowAirPx As Integer
        Get
            Return SY(DEFAULT_SEARCH_BAR_ROW_AIR)
        End Get
    End Property

    Friend ReadOnly Property SearchBarFontAirPx As Integer
        Get
            Return SY(DEFAULT_SEARCH_BAR_FONT_AIR)
        End Get
    End Property

    Friend ReadOnly Property SearchBoxMinWidthPx As Integer
        Get
            Return SX(DEFAULT_SEARCH_BOX_MIN_WIDTH)
        End Get
    End Property

    Friend ReadOnly Property SearchBoxAirPx As Integer
        Get
            Return SY(DEFAULT_SEARCH_BOX_AIR)
        End Get
    End Property

    Friend ReadOnly Property SearchLabelGapPx As Integer
        Get
            Return SX(DEFAULT_SEARCH_LABEL_GAP)
        End Get
    End Property

    Friend ReadOnly Property FooterButtonAirPx As Integer
        Get
            Return SY(DEFAULT_FOOTER_BUTTON_AIR)
        End Get
    End Property

    Friend ReadOnly Property LoaderSizePx As Integer
        Get
            Return SX(DEFAULT_LOADER_SIZE)
        End Get
    End Property

    ''' <summary>Grosimile de creion rămân Single — un creion de 1,5 px e legitim, spre deosebire
    ''' de o coordonată. De aceea NU trec prin SX (care rotunjește la întreg).</summary>
    Friend ReadOnly Property LoaderPenWidthPx As Single
        Get
            Return DEFAULT_LOADER_PEN * DpiScaleX
        End Get
    End Property

    Friend ReadOnly Property LoaderTextGapPx As Integer
        Get
            Return SX(DEFAULT_LOADER_TEXT_GAP)
        End Get
    End Property

    Friend ReadOnly Property CheckBoxRadiusPx As Integer
        Get
            Return SX(DEFAULT_CHECKBOX_RADIUS)
        End Get
    End Property

    Friend ReadOnly Property CheckMarkPenWidthPx As Single
        Get
            Return DEFAULT_CHECK_MARK_PEN * DpiScaleX
        End Get
    End Property

    Friend ReadOnly Property ExpanderSignInsetPx As Integer
        Get
            Return SX(DEFAULT_EXPANDER_SIGN_INSET)
        End Get
    End Property

End Class
