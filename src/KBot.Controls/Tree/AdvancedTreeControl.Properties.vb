Imports System.ComponentModel

Partial Public Class AdvancedTreeControl
    ' Categorii pentru grila de proprietăți (design-time). Ținute ca literale pentru că
    ' atributele VB cer expresii constante.
    Public Enum TreeCheckState
        Unchecked = 0       ' Nebifat
        Checked = 1         ' Bifat complet
        Indeterminate = 2   ' Parțial bifat (pătrățel plin sau liniuță)
    End Enum

    Public treeID As String

    ' Nodes
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Items As New List(Of TreeItem)

    Public RaiseLeftClickOnRightClick As Boolean = True
    Public ReRaiseClickOnSameNode As Boolean = True

    ' Tooltip
    Public AutoHideTooltipMs As Integer = 5000

    ' FONTUL ARBORELUI = <see cref="Font"/> (partiala .Theming). A existat până acum o a DOUA
    ' proprietate, «TreeFont», cu care se desenau nodurile, în timp ce înălțimea rândului
    ' (RecalculateItemHeight) și banda de căutare se luau după Font — două surse de adevăr pentru
    ' aceeași măsură, deci un arbore care putea desena text mai mare decât rândul care-l ținea.
    ' A rămas una singură: Font. Odată cu ea au dispărut și accesoriile FontName/FontSize, care
    ' nu erau decât «mută TreeFont» scris pe bucăți.

    ' MĂSURILE ÎN PIXELI vin acum în PERECHI: «…Logic» = ce a scris operatorul (px la 96 dpi,
    ' valoarea pe care o întoarce proprietatea și pe care o serializează designerul), iar câmpul
    ' fără sufix = aceeași măsură SCALATĂ la DPI-ul ecranului, cu care se pictează. Recalcularea
    ' e în AdvancedTreeControl.Dpi.vb (ApplyMetricScale); motivul, tot acolo.
    Private _expanderSizeLogic As Integer = 12
    Private m_ExpanderSize As Integer = 12
    <Category("K-BOT Arbore")>
    <Description("Latura (px la 96 dpi) a butonului de expandare +/-.")>
    <DefaultValue(12)>
    Public Property ExpanderSize As Integer
        Get
            Return _expanderSizeLogic
        End Get
        Set(value As Integer)
            _expanderSizeLogic = value
            m_ExpanderSize = SX(value)
            Me.Invalidate() ' Redesenează imediat controlul când se schimbă setarea
        End Set
    End Property

    Private _indentLogic As Integer = 10
    Private m_Indent As Integer = 10
    <Category("K-BOT Arbore")>
    <Description("Indentarea (px la 96 dpi) pe nivel de adâncime.")>
    <DefaultValue(10)>
    Public Property Indent As Integer
        Get
            Return _indentLogic
        End Get
        Set(value As Integer)
            _indentLogic = value
            m_Indent = SX(value)
            Me.Invalidate() ' Redesenează imediat controlul când se schimbă setarea
        End Set
    End Property

    Private _checkBoxSizeLogic As Integer = 16
    Private _checkBoxSize As Integer = 16
    <Category("K-BOT Arbore")>
    <Description("Latura (px la 96 dpi) a checkbox-ului/radio-ului de nod.")>
    <DefaultValue(16)>
    Public Property CheckBoxSize As Integer
        Get
            Return _checkBoxSizeLogic
        End Get
        Set(value As Integer)
            _checkBoxSizeLogic = value
            _checkBoxSize = SX(value)
            Me.Invalidate()
        End Set
    End Property

    ' Înălțimea rândului (calculată automat sau setată manual)
    Private _autoHeight As Boolean = False
    Private _itemHeightLogic As Integer = 22
    Private _itemHeight As Integer = 22
    <Category("K-BOT Arbore")>
    <Description("Înălțimea (px la 96 dpi) a unui rând de nod.")>
    <DefaultValue(22)>
    Public Property ItemHeight As Integer
        Get
            Return _itemHeightLogic
        End Get
        Set(value As Integer)
            _itemHeightLogic = value
            _itemHeight = SY(value)
            '_autoHeight = False
            RefreshSearchBarMetrics()   ' banda de căutare se dimensionează după rând
            Me.Invalidate()
        End Set
    End Property

    ' Iconițe - Setarea lor declanșează recalcularea înălțimii rândului
    Private _leftIconSizeLogic As New Size(18, 18)
    Private _leftIconSize As New Size(18, 18)
    <Category("K-BOT Arbore")>
    <Description("Dimensiunea (px la 96 dpi) a iconiței din stânga nodului.")>
    Public Property LeftIconSize As Size
        Get
            Return _leftIconSizeLogic
        End Get
        Set(value As Size)
            _leftIconSizeLogic = value
            _leftIconSize = New Size(SX(value.Width), SY(value.Height))
            RecalculateItemHeight()
        End Set
    End Property
    Public Function ShouldSerializeLeftIconSize() As Boolean
        Return _leftIconSizeLogic <> New Size(18, 18)
    End Function
    Public Sub ResetLeftIconSize()
        LeftIconSize = New Size(18, 18)
    End Sub

    Private _rightIconSizeLogic As New Size(18, 18)
    Private _rightIconSize As New Size(18, 18)
    <Category("K-BOT Arbore")>
    <Description("Dimensiunea (px la 96 dpi) a iconiței din dreapta nodului.")>
    Public Property RightIconSize As Size
        Get
            Return _rightIconSizeLogic
        End Get
        Set(value As Size)
            _rightIconSizeLogic = value
            _rightIconSize = New Size(SX(value.Width), SY(value.Height))
            RecalculateItemHeight()
        End Set
    End Property
    Public Function ShouldSerializeRightIconSize() As Boolean
        Return _rightIconSizeLogic <> New Size(18, 18)
    End Function
    Public Sub ResetRightIconSize()
        RightIconSize = New Size(18, 18)
    End Sub

    ' RightIconRightPadding s-a mutat în partiala .Paddings (toate marginile într-un singur fișier).

    ' Comutatorul de REZERVARE a locului iconiței din dreapta. Implicit False: locul NU se
    ' rezervă, deci textul nodului folosește toată lățimea și se îngustează abia când iconița
    ' chiar apare (la hover). Pus pe True, locul e ținut permanent: textul nu se mai mișcă la
    ' hover, cu prețul unei fâșii goale pe fiecare rând.
    Private _reserveRightIconSpace As Boolean = False
    <Category("K-BOT Arbore")>
    <Description("Ține permanent locul iconiței din dreapta (textul nu se mai îngustează la hover). " &
                 "Implicit False: locul se ia doar cât iconița e pe ecran.")>
    <DefaultValue(False)>
    Public Property ReserveRightIconSpace As Boolean
        Get
            Return _reserveRightIconSpace
        End Get
        Set(value As Boolean)
            _reserveRightIconSpace = value
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Lățimea pe care o ia iconița din dreapta DIN TEXTUL unui nod anume.
    '''
    ''' O iconiță PERMANENTĂ ia mereu locul: textul n-are voie să treacă pe sub ea. O iconiță
    ''' HOVER-ONLY (<see cref="ShowRightIconOnHover"/>, global sau per nod) nu ia nimic cât nodul
    ''' nu e survolat — ăsta e tot rostul lui «hover-only», ca textul să aibă toată lățimea — și
    ''' îngustează textul abia când apare. <see cref="ReserveRightIconSpace"/> = True cere locul
    ''' fix și pentru ea, dacă operatorul preferă un text care nu se mișcă.
    ''' </summary>
    Friend Function RightIconGutter(it As TreeItem) As Integer
        If it Is Nothing OrElse it.RightIcon Is Nothing Then Return 0
        Dim latime As Integer = RightIconSize.Width + RightIconRightPaddingPx
        If Not IsRightIconHoverOnly(it) Then Return latime
        If _reserveRightIconSpace Then Return latime
        Return If(it Is pHoveredItem, latime, 0)
    End Function

    ''' <summary>Iconița din dreapta a acestui nod apare doar la survolare? (global SAU per nod)</summary>
    Friend Function IsRightIconHoverOnly(it As TreeItem) As Boolean
        Return _showRightIconOnHover OrElse (it IsNot Nothing AndAlso it.ShowRightIconOnHover)
    End Function

    ''' <summary>
    ''' Cârlig de test: pune nodul survolat fără mouse. Survolarea e stare INTERNĂ (o scrie
    ''' <c>OnMouseMove</c>), dar regula de rezervare a locului depinde de ea, iar o regulă
    ''' care nu se poate proba decât cu un cursor real e o regulă neprobată.
    ''' </summary>
    Friend Sub DebugSetHoveredItem(it As TreeItem)
        pHoveredItem = it
    End Sub

    ''' <summary>
    ''' Lățimea rezervată la dreapta pentru BANDA DE COLOANE (TreeListView). Aici rezervarea NU e
    ''' condiționată de hover, cu bună știință: coloanele sunt o geometrie pe tot controlul, iar o
    ''' bandă care se re-așază la fiecare trecere a cursorului ar fi de nefolosit. Doar textul de
    ''' nod se îngustează la hover — vezi <see cref="RightIconGutter"/>.
    ''' </summary>
    Friend Function ReservedRightIconWidth() As Integer
        Return If(_reserveRightIconSpace, RightIconSize.Width + RightIconRightPaddingPx, 0)
    End Function

    Private _RootExpander As Boolean = True
    <Category("K-BOT Arbore")>
    <Description("Afișează expanderul pe nodurile rădăcină (Level 0).")>
    <DefaultValue(True)>
    Public Property RootExpander As Boolean
        Get
            Return _RootExpander
        End Get
        Set(value As Boolean)
            _RootExpander = value
            Me.Invalidate()
        End Set
    End Property

    Private _rightClickFunc As String = ""
    <Category("K-BOT Arbore")>
    <Description("Numele funcției VBA apelate la click-dreapta (integrare FOREXE).")>
    <DefaultValue("")>
    Public Property RightClickFunction As String
        Get
            Return _rightClickFunc
        End Get
        Set(value As String)
            _rightClickFunc = value
            Me.Invalidate()
        End Set
    End Property

    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedNode As TreeItem
        Get
            Return pSelectedItem
        End Get
        Set(value As TreeItem)
            If pSelectedItem IsNot value Then
                pSelectedItem = value
                ' Nodul nou selectat poate fi chiar cel peste care stă eticheta plutitoare.
                EnsureCollapsedFlyoutStillAllowed()
                ' Invalidate to trigger redraw and show the new selection
                Me.Invalidate()
            End If
        End Set
    End Property

    Private _checkBoxes As Boolean = False
    <Category("K-BOT Arbore")>
    <Description("Activează checkbox-urile de nod (mod normal, fără radio).")>
    <DefaultValue(False)>
    Public Property CheckBoxes As Boolean
        Get
            Return _checkBoxes
        End Get
        Set(value As Boolean)
            _checkBoxes = value
            Me.Invalidate() ' Redesenează imediat controlul când se schimbă setarea
        End Set
    End Property

    Private _hasNodeIcons As Boolean = True
    <Category("K-BOT Arbore")>
    <Description("Desenează iconițele de nod (stânga).")>
    <DefaultValue(True)>
    Public Property HasNodeIcons As Boolean
        Get
            Return _hasNodeIcons
        End Get
        Set(value As Boolean)
            _hasNodeIcons = value
            Me.Invalidate() ' Redesenează imediat controlul când se schimbă setarea
        End Set
    End Property

    Private _isPopupTree As Boolean = False
    <Category("K-BOT Arbore")>
    <Description("Arborele rulează ca popup (nu ridică dublu-click de nod).")>
    <DefaultValue(False)>
    Public Property IsPopupTree As Boolean
        Get
            Return _isPopupTree
        End Get
        Set(value As Boolean)
            _isPopupTree = value
            Me.Invalidate() ' Redesenează imediat controlul când se schimbă setarea
        End Set
    End Property

    Private _popupGraceMs As Integer = 1500
    <Category("K-BOT Arbore")>
    <Description("Timpul de grație (ms) înainte de închiderea automată a popup-ului.")>
    <DefaultValue(1500)>
    Public Property PopupGraceMs() As Integer
        Get
            Return _popupGraceMs
        End Get
        Set(value As Integer)
            _popupGraceMs = value
        End Set
    End Property

    Private _radioButtonLevel As Integer = -1  ' -1 = dezactivat
    <Category("K-BOT Arbore")>
    <Description("Nivelul care primește butoane radio; -1 = dezactivat.")>
    <DefaultValue(-1)>
    Public Property RadioButtonLevel As Integer
        Get
            Return _radioButtonLevel
        End Get
        Set(value As Integer)
            _radioButtonLevel = value
            Me.Invalidate()
        End Set
    End Property

    ' NOTĂ pentru toate culorile de mai jos: Color.Empty = «auto», adică valoarea din tema
    ' activă (vezi partiala .Theming). Getterul întoarce culoarea REZOLVATĂ — ce se desenează
    ' efectiv — iar perechea ShouldSerialize*/Reset* face ca designerul să scrie o linie doar
    ' pentru o alegere reală a operatorului, nu pentru implicitul rezolvat.
    Private m_BorderColor As Color = Color.Empty
    <Category("K-BOT Arbore - Culori")>
    <Description("Culoarea bordurii controlului; Transparent = fără bordură, gol = din temă.")>
    Public Property BorderColor As Color
        Get
            Return If(m_BorderColor <> Color.Empty, m_BorderColor, _autoBorder)
        End Get
        Set(value As Color)
            m_BorderColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeBorderColor() As Boolean
        Return m_BorderColor <> Color.Empty
    End Function
    Public Sub ResetBorderColor()
        m_BorderColor = Color.Empty
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Grosimea chenarului, în pixeli LOGICI (la 96 dpi). <c>0</c> = fără chenar, la fel ca
    ''' <c>BorderColor = Transparent</c>. Adăugată odată cu perechea din <c>KBotDataView</c>: aici
    ''' culoarea se putea alege dintotdeauna, dar grosimea era scrisă în pictură.
    ''' </summary>
    Private _borderWidth As Integer = DEFAULT_SEPARATOR_WIDTH
    <Category("K-BOT Arbore - Culori")>
    <Description("Grosimea chenarului, în pixeli la 96 dpi. 0 = fără chenar.")>
    <DefaultValue(DEFAULT_SEPARATOR_WIDTH)>
    Public Property BorderWidth As Integer
        Get
            Return _borderWidth
        End Get
        Set(value As Integer)
            If value < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(value),
                    $"Grosimea chenarului nu poate fi negativă (primit «{value}»); 0 = fără chenar.")
            End If
            _borderWidth = value
            Me.Invalidate()
        End Set
    End Property

    Private m_HoverBackColor As Color = Color.Empty
    <Category("K-BOT Arbore - Culori")>
    <Description("Fundalul rândului la hover; gol = din temă.")>
    Public Property HoverBackColor As Color
        Get
            Return If(m_HoverBackColor <> Color.Empty, m_HoverBackColor, _autoHoverBack)
        End Get
        Set(value As Color)
            m_HoverBackColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeHoverBackColor() As Boolean
        Return m_HoverBackColor <> Color.Empty
    End Function
    Public Sub ResetHoverBackColor()
        m_HoverBackColor = Color.Empty
        Me.Invalidate()
    End Sub

    Private m_SelectedBackColor As Color = Color.Empty
    <Category("K-BOT Arbore - Culori")>
    <Description("Fundalul rândului selectat; gol = din temă.")>
    Public Property SelectedBackColor As Color
        Get
            Return If(m_SelectedBackColor <> Color.Empty, m_SelectedBackColor, _autoSelectedBack)
        End Get
        Set(value As Color)
            m_SelectedBackColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSelectedBackColor() As Boolean
        Return m_SelectedBackColor <> Color.Empty
    End Function
    Public Sub ResetSelectedBackColor()
        m_SelectedBackColor = Color.Empty
        Me.Invalidate()
    End Sub

    Private m_SelectedBorderColor As Color = Color.Empty
    <Category("K-BOT Arbore - Culori")>
    <Description("Bordura rândului selectat; gol = din temă.")>
    Public Property SelectedBorderColor As Color
        Get
            Return If(m_SelectedBorderColor <> Color.Empty, m_SelectedBorderColor, _autoSelectedBorder)
        End Get
        Set(value As Color)
            m_SelectedBorderColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSelectedBorderColor() As Boolean
        Return m_SelectedBorderColor <> Color.Empty
    End Function
    Public Sub ResetSelectedBorderColor()
        m_SelectedBorderColor = Color.Empty
        Me.Invalidate()
    End Sub

    Private m_LineColor As Color = Color.Empty
    <Category("K-BOT Arbore - Culori")>
    <Description("Culoarea liniilor punctate ale arborelui; gol = din temă.")>
    Public Property LineColor As Color
        Get
            Return If(m_LineColor <> Color.Empty, m_LineColor, _autoLine)
        End Get
        Set(value As Color)
            m_LineColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeLineColor() As Boolean
        Return m_LineColor <> Color.Empty
    End Function
    Public Sub ResetLineColor()
        m_LineColor = Color.Empty
        Me.Invalidate()
    End Sub

    Private _tooltipDelayMs As Integer = 600
    <Category("K-BOT Arbore - Tooltip")>
    <Description("Întârzierea (ms) până la apariția tooltip-ului.")>
    <DefaultValue(600)>
    Public Property TooltipDelayMs As Integer
        Get
            Return _tooltipDelayMs
        End Get
        Set(value As Integer)
            _tooltipDelayMs = value
            TooltipTimer.Interval = value
        End Set
    End Property

    Private m_leftTextWidth As Integer = 0
    <Category("K-BOT Arbore")>
    <Description("Lățime fixă (px @96dpi) rezervată textului din stânga caption; 0 = dinamic.")>
    <DefaultValue(0)>
    Public Property LeftTextWidth As Integer
        Get
            Return m_leftTextWidth
        End Get
        Set(value As Integer)
            m_leftTextWidth = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ' Lățime fixă rezervată pentru textul drept din caption cu separator ~~~
    ' 0 = nelimitat (dinamic)
    Private m_rightTextWidth As Integer = 0
    <Category("K-BOT Arbore")>
    <Description("Lățime fixă (px @96dpi) rezervată textului din dreapta caption (separator ~~~); 0 = dinamic.")>
    <DefaultValue(0)>
    Public Property RightTextWidth As Integer
        Get
            Return m_rightTextWidth
        End Get
        Set(value As Integer)
            m_rightTextWidth = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' <see cref="LeftTextWidth"/> în pixeli de ecran. Proprietatea publică rămâne LOGICĂ
    ''' (px @96dpi) — asta a tastat operatorul, asta se serializează —, iar pictura folosește
    ''' varianta de aici. Fără scalare, la 150% cele 100 de puncte cerute rămâneau 100 de pixeli
    ''' pe un rând cu litere cu 50% mai mari, adică o rezervare mai strâmtă decât cea cerută,
    ''' și restul de caption primea aerul rămas (felia 0040).
    ''' </summary>
    Friend ReadOnly Property LeftTextWidthPx As Integer
        Get
            Return SX(m_leftTextWidth)
        End Get
    End Property

    ''' <summary><see cref="RightTextWidth"/> în pixeli de ecran — vezi <see cref="LeftTextWidthPx"/>.</summary>
    Friend ReadOnly Property RightTextWidthPx As Integer
        Get
            Return SX(m_rightTextWidth)
        End Get
    End Property

    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property OldSelectedNode As TreeItem
        Get
            Return pOldSelectedItem
        End Get
    End Property

    ' Când True, iconița din dreapta este vizibilă DOAR la hover pe nodul respectiv.
    ' Spațiul din dreapta e rezervat întotdeauna (textul nu sare la hover).
    ' Per-nod: TreeItem.ShowRightIconOnHover suprascrie globalul DOAR pentru nodul respectiv.
    Private _showRightIconOnHover As Boolean = False
    <Category("K-BOT Arbore")>
    <Description("Iconița din dreapta apare doar la hover pe nod (spațiul rămâne rezervat).")>
    <DefaultValue(False)>
    Public Property ShowRightIconOnHover As Boolean
        Get
            Return _showRightIconOnHover
        End Get
        Set(value As Boolean)
            _showRightIconOnHover = value
            Me.Invalidate()
        End Set
    End Property

    ' ══════════════════════════════════════════════════
    ' HEADER PROPERTIES
    ' ══════════════════════════════════════════════════

    Private _headerVisible As Boolean = False
    <Category("K-BOT Arbore - Antet")>
    <Description("Afișează banda de antet deasupra arborelui.")>
    <DefaultValue(False)>
    Public Property HeaderVisible As Boolean
        Get
            Return _headerVisible
        End Get
        Set(value As Boolean)
            _headerVisible = value
            If _isSearchMode Then PositionSearchTextBox()   ' banda stă sub antet
            Me.Invalidate()
        End Set
    End Property

    Private _headerHeightLogic As Integer = 32
    Private _headerHeight As Integer = 32
    <Category("K-BOT Arbore - Antet")>
    <Description("Înălțimea (px la 96 dpi) benzii de antet.")>
    <DefaultValue(32)>
    Public Property HeaderHeight As Integer
        Get
            Return _headerHeightLogic
        End Get
        Set(value As Integer)
            _headerHeightLogic = Math.Max(16, value)
            _headerHeight = SY(_headerHeightLogic)
            If _isSearchMode Then PositionSearchTextBox()
            Me.Invalidate()
        End Set
    End Property

    Private _headerCaption As String = ""
    <Category("K-BOT Arbore - Antet")>
    <Description("Textul afișat în banda de antet.")>
    <DefaultValue("")>
    Public Property HeaderCaption As String
        Get
            Return _headerCaption
        End Get
        Set(value As String)
            _headerCaption = value
            Me.Invalidate()
        End Set
    End Property

    ' Resolved images — set directly or via ResolveHeaderIcons()
    Private _headerLeftIcon As Image = Nothing
    Private _headerRightIcon As Image = Nothing
    Private _headerSearchIcon As Image = Nothing

    ' Iconițele se pot alege DIRECT din designer (selectorul de imagini le pune în .resx) sau
    ' se pot rezolva la rulare din cache-ul de iconițe / NodeImages prin cheile *IconKey de mai
    ' jos. Cine setează ultimul câștigă: ResolveHeaderIcons nu suprascrie decât cheile care
    ' chiar găsesc o imagine.
    <Category("K-BOT Arbore - Antet")>
    <Description("Iconița din stânga antetului.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property HeaderLeftIcon As Image
        Get
            Return _headerLeftIcon
        End Get
        Set(value As Image)
            _headerLeftIcon = value : Me.Invalidate()
        End Set
    End Property
    <Category("K-BOT Arbore - Antet")>
    <Description("Iconița din dreapta antetului (ridică HeaderRightIconClicked).")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property HeaderRightIcon As Image
        Get
            Return _headerRightIcon
        End Get
        Set(value As Image)
            _headerRightIcon = value
            Me.Invalidate()
        End Set
    End Property
    <Category("K-BOT Arbore - Antet")>
    <Description("Iconița de căutare din antet; prezentă, ea deschide/închide banda de căutare.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property HeaderSearchIcon As Image
        Get
            Return _headerSearchIcon
        End Get
        Set(value As Image)
            _headerSearchIcon = value
            ' Fără iconiță de toggle, SearchShow înseamnă bandă permanentă — re-evaluăm.
            ApplySearchShow()
            Me.Invalidate()
        End Set
    End Property

    ' Icon keys — stored for resolution after image cache is loaded
    Private _headerLeftIconKey As String = ""
    Private _headerRightIconKey As String = ""
    Private _headerSearchIconKey As String = ""

    <Category("K-BOT Arbore - Antet")>
    <Description("Cheia iconiței din stânga antetului (rezolvată din cache-ul de iconițe).")>
    <DefaultValue("")>
    Public Property HeaderLeftIconKey As String
        Get
            Return _headerLeftIconKey
        End Get
        Set(value As String)
            _headerLeftIconKey = value
            ResolveHeaderIconsFromNodeImages()
            Me.Invalidate()
        End Set
    End Property
    <Category("K-BOT Arbore - Antet")>
    <Description("Cheia iconiței din dreapta antetului (rezolvată din cache-ul de iconițe).")>
    <DefaultValue("")>
    Public Property HeaderRightIconKey As String
        Get
            Return _headerRightIconKey
        End Get
        Set(value As String)
            _headerRightIconKey = value
            ResolveHeaderIconsFromNodeImages()
            Me.Invalidate()
        End Set
    End Property
    <Category("K-BOT Arbore - Antet")>
    <Description("Cheia iconiței de căutare din antet (rezolvată din cache-ul de iconițe).")>
    <DefaultValue("")>
    Public Property HeaderSearchIconKey As String
        Get
            Return _headerSearchIconKey
        End Get
        Set(value As String)
            _headerSearchIconKey = value
            ResolveHeaderIconsFromNodeImages()
            Me.Invalidate()
        End Set
    End Property

    ' Perechea logic/scalat, ca la LeftIconSize: proprietatea întoarce ce a tastat operatorul
    ' (px la 96 dpi), câmpul fără sufix e cel cu care se PICTEAZĂ. Până în felia 0039 exista doar
    ' al doilea, așa că la 150% iconițele de antet rămâneau de 16 px lângă un text de 1,5×.
    Private _headerIconSizeLogic As New Size(16, 16)
    Private _headerIconSize As New Size(16, 16)
    <Category("K-BOT Arbore - Antet")>
    <Description("Dimensiunea (px la 96 dpi) a iconițelor din antet.")>
    Public Property HeaderIconSize As Size
        Get
            Return _headerIconSizeLogic
        End Get
        Set(value As Size)
            _headerIconSizeLogic = value
            _headerIconSize = New Size(SX(value.Width), SY(value.Height))
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeHeaderIconSize() As Boolean
        Return _headerIconSizeLogic <> New Size(16, 16)
    End Function
    Public Sub ResetHeaderIconSize()
        HeaderIconSize = New Size(16, 16)
    End Sub

    Private _headerBackColor As Color = Color.Empty
    <Category("K-BOT Arbore - Antet")>
    <Description("Fundalul benzii de antet; gol = din temă. Pe schemă întunecată se ignoră și " &
                 "se ia oricum din temă (vezi BandColorsFromThemeOnly).")>
    Public Property HeaderBackColor As Color
        Get
            If _isDarkScheme Then Return _autoHeaderBack
            Return If(_headerBackColor <> Color.Empty, _headerBackColor, _autoHeaderBack)
        End Get
        Set(value As Color)
            _headerBackColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeHeaderBackColor() As Boolean
        Return _headerBackColor <> Color.Empty
    End Function
    Public Sub ResetHeaderBackColor()
        _headerBackColor = Color.Empty
        Me.Invalidate()
    End Sub

    Private _headerForeColor As Color = Color.Empty
    <Category("K-BOT Arbore - Antet")>
    <Description("Culoarea textului din antet; gol = din temă. Pe schemă întunecată se ignoră.")>
    Public Property HeaderForeColor As Color
        Get
            If _isDarkScheme Then Return _autoHeaderFore
            Return If(_headerForeColor <> Color.Empty, _headerForeColor, _autoHeaderFore)
        End Get
        Set(value As Color)
            _headerForeColor = value
            RestyleSearchChildren()      ' eticheta de căutare cade pe culoarea antetului
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeHeaderForeColor() As Boolean
        Return _headerForeColor <> Color.Empty
    End Function
    Public Sub ResetHeaderForeColor()
        _headerForeColor = Color.Empty
        RestyleSearchChildren()
        Me.Invalidate()
    End Sub

    ' ── Linia dintre antet și corp (felia 0038) ──────────────────────────────────
    ' Până acum era o deducere fixă din culoarea textului de antet (aceeași culoare, la 60 din 255
    ' opacitate) și avea un pixel, nediscutabil. Rămâne exact aceea cât timp nimeni nu cere altceva
    ' — «gol = automat», ca toate culorile de mai sus.
    Private _headerSeparatorColor As Color = Color.Empty
    <Category("K-BOT Arbore - Antet")>
    <Description("Culoarea liniei dintre antet și corp; gol = deducerea din culoarea textului de antet.")>
    Public Property HeaderSeparatorColor As Color
        Get
            If _headerSeparatorColor <> Color.Empty Then Return _headerSeparatorColor
            Return Color.FromArgb(60, HeaderForeColor)
        End Get
        Set(value As Color)
            _headerSeparatorColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeHeaderSeparatorColor() As Boolean
        Return _headerSeparatorColor <> Color.Empty
    End Function
    Public Sub ResetHeaderSeparatorColor()
        _headerSeparatorColor = Color.Empty
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Grosimea liniei dintre antet și corp, în pixeli LOGICI (la 96 dpi) — se scalează cu ecranul,
    ''' ca toate măsurile arborelui de la felia 0035. <c>0</c> = fără linie.
    ''' </summary>
    Private _headerSeparatorWidth As Integer = DEFAULT_SEPARATOR_WIDTH
    <Category("K-BOT Arbore - Antet")>
    <Description("Grosimea liniei dintre antet și corp, în pixeli la 96 dpi. 0 = fără linie.")>
    <DefaultValue(DEFAULT_SEPARATOR_WIDTH)>
    Public Property HeaderSeparatorWidth As Integer
        Get
            Return _headerSeparatorWidth
        End Get
        Set(value As Integer)
            If value < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(value),
                    $"Grosimea liniei nu poate fi negativă (primit «{value}»); 0 = fără linie.")
            End If
            _headerSeparatorWidth = value
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>Grosimea logică implicită a liniei dintre benzi (px la 96 dpi).</summary>
    Friend Const DEFAULT_SEPARATOR_WIDTH As Integer = 1

    ' ── Linia de sub banda de căutare (felia 0038) ───────────────────────────────
    ' ATENȚIE la implicit: până acum era `Color.FromArgb(80, Color.Black)` — NEGRU LITERAL, adică
    ' singura culoare scrisă în sursă din tot controlul, deci una care nu se schimba cu tema și se
    ' pierdea pe schemele întunecate. «Gol» înseamnă acum aceeași transparență peste LineColor, care
    ' VINE din paletă. E o schimbare de aspect, mică și deliberată: regula casei e zero culori
    ' literale, iar cine chiar vrea negru îl poate cere acum pe față.
    Private _searchSeparatorColor As Color = Color.Empty
    <Category("K-BOT Arbore - Cautare")>
    <Description("Culoarea liniei de sub banda de căutare; gol = culoarea de linie a temei, transparentizată.")>
    Public Property SearchSeparatorColor As Color
        Get
            If _searchSeparatorColor <> Color.Empty Then Return _searchSeparatorColor
            Return Color.FromArgb(80, LineColor)
        End Get
        Set(value As Color)
            _searchSeparatorColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSearchSeparatorColor() As Boolean
        Return _searchSeparatorColor <> Color.Empty
    End Function
    Public Sub ResetSearchSeparatorColor()
        _searchSeparatorColor = Color.Empty
        Me.Invalidate()
    End Sub

    ''' <summary>Grosimea liniei de sub banda de căutare, în pixeli LOGICI. <c>0</c> = fără linie.</summary>
    Private _searchSeparatorWidth As Integer = DEFAULT_SEPARATOR_WIDTH
    <Category("K-BOT Arbore - Cautare")>
    <Description("Grosimea liniei de sub banda de căutare, în pixeli la 96 dpi. 0 = fără linie.")>
    <DefaultValue(DEFAULT_SEPARATOR_WIDTH)>
    Public Property SearchSeparatorWidth As Integer
        Get
            Return _searchSeparatorWidth
        End Get
        Set(value As Integer)
            If value < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(value),
                    $"Grosimea liniei nu poate fi negativă (primit «{value}»); 0 = fără linie.")
            End If
            _searchSeparatorWidth = value
            Me.Invalidate()
        End Set
    End Property

    ' ── Liniile benzii de coloane / TreeListView (felia 0038) ────────────────────
    ' Un SINGUR reglaj pentru toate cele trei linii verticale (începutul zonei de coloane, capătul
    ' fiecărei coloane pe rând, și aceleași muchii în banda de titluri): ochiul le vede ca pe una
    ' singură, care coboară prin tot controlul. Trei proprietăți care trebuie ținute la fel n-ar fi
    ' un reglaj mai fin, ar fi trei ocazii ca ele să nu mai semene.
    Private _columnSeparatorColor As Color = Color.Empty
    <Category("K-BOT Arbore - Coloane")>
    <Description("Culoarea liniilor verticale dintre coloane (rânduri și titluri); gol = linia temei, transparentizată.")>
    Public Property ColumnSeparatorColor As Color
        Get
            If _columnSeparatorColor <> Color.Empty Then Return _columnSeparatorColor
            Return Color.FromArgb(COLUMN_SEPARATOR_COLOR_ALPHA, LineColor)
        End Get
        Set(value As Color)
            _columnSeparatorColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeColumnSeparatorColor() As Boolean
        Return _columnSeparatorColor <> Color.Empty
    End Function
    Public Sub ResetColumnSeparatorColor()
        _columnSeparatorColor = Color.Empty
        Me.Invalidate()
    End Sub

    ''' <summary>Grosimea liniilor verticale dintre coloane, în pixeli LOGICI. <c>0</c> = fără ele.</summary>
    Private _columnSeparatorWidth As Integer = DEFAULT_SEPARATOR_WIDTH
    <Category("K-BOT Arbore - Coloane")>
    <Description("Grosimea liniilor verticale dintre coloane, în pixeli la 96 dpi. 0 = fără linii.")>
    <DefaultValue(DEFAULT_SEPARATOR_WIDTH)>
    Public Property ColumnSeparatorWidth As Integer
        Get
            Return _columnSeparatorWidth
        End Get
        Set(value As Integer)
            If value < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(value),
                    $"Grosimea liniei nu poate fi negativă (primit «{value}»); 0 = fără linii.")
            End If
            _columnSeparatorWidth = value
            Me.Invalidate()
        End Set
    End Property

    ' Linia ORIZONTALĂ de sub banda de titluri de coloană — sora celei de sub antet, dar a altei
    ' benzi, deci reglaj propriu.
    Private _columnHeaderSeparatorColor As Color = Color.Empty
    <Category("K-BOT Arbore - Coloane")>
    <Description("Culoarea liniei de sub banda de titluri de coloană; gol = culoarea de linie a temei.")>
    Public Property ColumnHeaderSeparatorColor As Color
        Get
            If _columnHeaderSeparatorColor <> Color.Empty Then Return _columnHeaderSeparatorColor
            Return LineColor
        End Get
        Set(value As Color)
            _columnHeaderSeparatorColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeColumnHeaderSeparatorColor() As Boolean
        Return _columnHeaderSeparatorColor <> Color.Empty
    End Function
    Public Sub ResetColumnHeaderSeparatorColor()
        _columnHeaderSeparatorColor = Color.Empty
        Me.Invalidate()
    End Sub

    ''' <summary>Grosimea liniei de sub banda de titluri de coloană, în px LOGICI. <c>0</c> = fără ea.</summary>
    Private _columnHeaderSeparatorWidth As Integer = DEFAULT_SEPARATOR_WIDTH
    <Category("K-BOT Arbore - Coloane")>
    <Description("Grosimea liniei de sub banda de titluri de coloană, în pixeli la 96 dpi. 0 = fără linie.")>
    <DefaultValue(DEFAULT_SEPARATOR_WIDTH)>
    Public Property ColumnHeaderSeparatorWidth As Integer
        Get
            Return _columnHeaderSeparatorWidth
        End Get
        Set(value As Integer)
            If value < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(value),
                    $"Grosimea liniei nu poate fi negativă (primit «{value}»); 0 = fără linie.")
            End If
            _columnHeaderSeparatorWidth = value
            Me.Invalidate()
        End Set
    End Property

    ' ── Antet: font, aliniere, stil de fundal ─────────────────────────────────────
    Private _headerFont As Font = Nothing          ' Nothing = fontul arborelui (Font)
    <Category("K-BOT Arbore - Antet")>
    <Description("Fontul textului din antet (toate atributele); nesetat = Font.")>
    Public Property HeaderFont As Font
        Get
            Return If(_headerFont, Me.Font)
        End Get
        Set(value As Font)
            _headerFont = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeHeaderFont() As Boolean
        Return _headerFont IsNot Nothing
    End Function
    Public Sub ResetHeaderFont()
        _headerFont = Nothing
        Me.Invalidate()
    End Sub

    Private _headerTextAlign As ContentAlignment = ContentAlignment.MiddleLeft
    <Category("K-BOT Arbore - Antet")>
    <Description("Alinierea textului din antet, în spațiul rămas între iconițe.")>
    <DefaultValue(ContentAlignment.MiddleLeft)>
    Public Property HeaderTextAlign As ContentAlignment
        Get
            Return _headerTextAlign
        End Get
        Set(value As ContentAlignment)
            _headerTextAlign = value
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>Stilul de fundal al benzii de antet.</summary>
    Public Enum En_HeaderBackStyle
        Solid = 0
        GradientVertical = 1
        GradientHorizontal = 2
    End Enum

    Private _headerBackStyle As En_HeaderBackStyle = En_HeaderBackStyle.Solid
    <Category("K-BOT Arbore - Antet")>
    <Description("Fundal plin sau în degrade (vertical/orizontal) pornind din HeaderBackColor.")>
    <DefaultValue(En_HeaderBackStyle.Solid)>
    Public Property HeaderBackStyle As En_HeaderBackStyle
        Get
            Return _headerBackStyle
        End Get
        Set(value As En_HeaderBackStyle)
            _headerBackStyle = value
            Me.Invalidate()
        End Set
    End Property

    Private _headerGradientEndColor As Color = Color.Empty
    <Category("K-BOT Arbore - Antet")>
    <Description("Capătul degradeului de antet; gol = automat (spre alb dacă baza e deschisă, spre " &
                 "negru dacă e închisă). Pe schemă întunecată se calculează întotdeauna automat — " &
                 "rămâne CĂ e degrade, nu și culoarea aleasă pentru tema luminoasă.")>
    Public Property HeaderGradientEndColor As Color
        Get
            If _isDarkScheme Then Return AutoGradientEnd(HeaderBackColor)
            Return If(_headerGradientEndColor <> Color.Empty,
                      _headerGradientEndColor, AutoGradientEnd(HeaderBackColor))
        End Get
        Set(value As Color)
            _headerGradientEndColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeHeaderGradientEndColor() As Boolean
        Return _headerGradientEndColor <> Color.Empty
    End Function
    Public Sub ResetHeaderGradientEndColor()
        _headerGradientEndColor = Color.Empty
        Me.Invalidate()
    End Sub

    ' ══════════════════════════════════════════════════
    ' FOOTER PROPERTIES
    ' ══════════════════════════════════════════════════
    ' Banda de subsol e sora antetului: aceleași unelte (înălțime, fundal plin/degrade, iconiță
    ' stânga, caption cu font/culori proprii) plus BUTONUL DE STRÂNGERE, care e singura piesă
    ' fără corespondent sus. Desenul e în partiala .Footer, exact ca al antetului în .Header.

    ''' <summary>Latura pe care stă butonul de strângere din subsol.</summary>
    Public Enum En_FooterButtonPosition
        Right = 0
        Left = 1
    End Enum

    Private _footerVisible As Boolean = False
    <Category("K-BOT Arbore - Subsol")>
    <Description("Afișează banda de subsol sub arbore.")>
    <DefaultValue(False)>
    Public Property FooterVisible As Boolean
        Get
            Return _footerVisible
        End Get
        Set(value As Boolean)
            _footerVisible = value
            CancelCollapsedFlyout()          ' banda plecată = orice etichetă afară e orfană
            RefreshScrollVisibility()        ' zona de noduri s-a scurtat/lungit
            Me.Invalidate()
        End Set
    End Property

    Private _footerHeightLogic As Integer = 28
    Private _footerHeight As Integer = 28
    <Category("K-BOT Arbore - Subsol")>
    <Description("Înălțimea (px la 96 dpi) benzii de subsol.")>
    <DefaultValue(28)>
    Public Property FooterHeight As Integer
        Get
            Return _footerHeightLogic
        End Get
        Set(value As Integer)
            _footerHeightLogic = Math.Max(16, value)
            _footerHeight = SY(_footerHeightLogic)
            RefreshScrollVisibility()
            Me.Invalidate()
        End Set
    End Property

    Private _footerCaption As String = ""
    <Category("K-BOT Arbore - Subsol")>
    <Description("Textul afișat în banda de subsol.")>
    <DefaultValue("")>
    Public Property FooterCaption As String
        Get
            Return _footerCaption
        End Get
        Set(value As String)
            _footerCaption = value
            Me.Invalidate()
        End Set
    End Property

    Private _footerLeftIcon As Image = Nothing
    <Category("K-BOT Arbore - Subsol")>
    <Description("Iconița din stânga subsolului (ridică FooterLeftIconClicked). " &
                 "IGNORATĂ dacă butonul de strângere stă tot în stânga.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property FooterLeftIcon As Image
        Get
            Return _footerLeftIcon
        End Get
        Set(value As Image)
            _footerLeftIcon = value
            Me.Invalidate()
        End Set
    End Property

    Private _footerLeftIconKey As String = ""
    <Category("K-BOT Arbore - Subsol")>
    <Description("Cheia iconiței din stânga subsolului (rezolvată din cache-ul de iconițe).")>
    <DefaultValue("")>
    Public Property FooterLeftIconKey As String
        Get
            Return _footerLeftIconKey
        End Get
        Set(value As String)
            _footerLeftIconKey = value
            ResolveHeaderIconsFromNodeImages()
            Me.Invalidate()
        End Set
    End Property

    Private _footerRightIcon As Image = Nothing
    <Category("K-BOT Arbore - Subsol")>
    <Description("Iconița din dreapta subsolului (ridică FooterRightIconClicked). " &
                 "IGNORATĂ dacă butonul de strângere stă tot în dreapta.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property FooterRightIcon As Image
        Get
            Return _footerRightIcon
        End Get
        Set(value As Image)
            _footerRightIcon = value
            Me.Invalidate()
        End Set
    End Property

    Private _footerRightIconKey As String = ""
    <Category("K-BOT Arbore - Subsol")>
    <Description("Cheia iconiței din dreapta subsolului (rezolvată din cache-ul de iconițe).")>
    <DefaultValue("")>
    Public Property FooterRightIconKey As String
        Get
            Return _footerRightIconKey
        End Get
        Set(value As String)
            _footerRightIconKey = value
            ResolveHeaderIconsFromNodeImages()
            Me.Invalidate()
        End Set
    End Property

    ' Vezi HeaderIconSize: logic în proprietate, scalat în câmpul cu care se pictează.
    Private _footerIconSizeLogic As New Size(16, 16)
    Private _footerIconSize As New Size(16, 16)
    <Category("K-BOT Arbore - Subsol")>
    <Description("Dimensiunea (px la 96 dpi) a iconiței din subsol.")>
    Public Property FooterIconSize As Size
        Get
            Return _footerIconSizeLogic
        End Get
        Set(value As Size)
            _footerIconSizeLogic = value
            _footerIconSize = New Size(SX(value.Width), SY(value.Height))
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterIconSize() As Boolean
        Return _footerIconSizeLogic <> New Size(16, 16)
    End Function
    Public Sub ResetFooterIconSize()
        FooterIconSize = New Size(16, 16)
    End Sub

    Private _footerBackColor As Color = Color.Empty
    <Category("K-BOT Arbore - Subsol")>
    <Description("Fundalul benzii de subsol; gol = din temă. Pe schemă întunecată se ignoră și " &
                 "se ia oricum din temă (vezi BandColorsFromThemeOnly).")>
    Public Property FooterBackColor As Color
        Get
            If _isDarkScheme Then Return _autoFooterBack
            Return If(_footerBackColor <> Color.Empty, _footerBackColor, _autoFooterBack)
        End Get
        Set(value As Color)
            _footerBackColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterBackColor() As Boolean
        Return _footerBackColor <> Color.Empty
    End Function
    Public Sub ResetFooterBackColor()
        _footerBackColor = Color.Empty
        Me.Invalidate()
    End Sub

    Private _footerForeColor As Color = Color.Empty
    <Category("K-BOT Arbore - Subsol")>
    <Description("Culoarea de prim-plan a subsolului (unghiul butonului, implicitul textului); " &
                 "gol = din temă. Pe schemă întunecată se ignoră.")>
    Public Property FooterForeColor As Color
        Get
            If _isDarkScheme Then Return _autoFooterFore
            Return If(_footerForeColor <> Color.Empty, _footerForeColor, _autoFooterFore)
        End Get
        Set(value As Color)
            _footerForeColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterForeColor() As Boolean
        Return _footerForeColor <> Color.Empty
    End Function
    Public Sub ResetFooterForeColor()
        _footerForeColor = Color.Empty
        Me.Invalidate()
    End Sub

    ' ── Linia dintre corp și subsol (felia 0038) — perechea celei de sub antet ───
    Private _footerSeparatorColor As Color = Color.Empty
    <Category("K-BOT Arbore - Subsol")>
    <Description("Culoarea liniei dintre corp și subsol; gol = deducerea din culoarea de prim-plan a subsolului.")>
    Public Property FooterSeparatorColor As Color
        Get
            If _footerSeparatorColor <> Color.Empty Then Return _footerSeparatorColor
            Return Color.FromArgb(60, FooterForeColor)
        End Get
        Set(value As Color)
            _footerSeparatorColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterSeparatorColor() As Boolean
        Return _footerSeparatorColor <> Color.Empty
    End Function
    Public Sub ResetFooterSeparatorColor()
        _footerSeparatorColor = Color.Empty
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Grosimea liniei dintre corp și subsol, în pixeli LOGICI (la 96 dpi). <c>0</c> = fără linie.
    ''' </summary>
    Private _footerSeparatorWidth As Integer = DEFAULT_SEPARATOR_WIDTH
    <Category("K-BOT Arbore - Subsol")>
    <Description("Grosimea liniei dintre corp și subsol, în pixeli la 96 dpi. 0 = fără linie.")>
    <DefaultValue(DEFAULT_SEPARATOR_WIDTH)>
    Public Property FooterSeparatorWidth As Integer
        Get
            Return _footerSeparatorWidth
        End Get
        Set(value As Integer)
            If value < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(value),
                    $"Grosimea liniei nu poate fi negativă (primit «{value}»); 0 = fără linie.")
            End If
            _footerSeparatorWidth = value
            Me.Invalidate()
        End Set
    End Property

    Private _footerBackStyle As En_HeaderBackStyle = En_HeaderBackStyle.Solid
    <Category("K-BOT Arbore - Subsol")>
    <Description("Fundal plin sau în degrade (vertical/orizontal) pornind din FooterBackColor.")>
    <DefaultValue(En_HeaderBackStyle.Solid)>
    Public Property FooterBackStyle As En_HeaderBackStyle
        Get
            Return _footerBackStyle
        End Get
        Set(value As En_HeaderBackStyle)
            _footerBackStyle = value
            Me.Invalidate()
        End Set
    End Property

    Private _footerGradientEndColor As Color = Color.Empty
    <Category("K-BOT Arbore - Subsol")>
    <Description("Capătul degradeului de subsol; gol = automat (spre alb dacă baza e deschisă, spre " &
                 "negru dacă e închisă). Pe schemă întunecată se calculează întotdeauna automat — " &
                 "rămâne CĂ e degrade, nu și culoarea aleasă pentru tema luminoasă.")>
    Public Property FooterGradientEndColor As Color
        Get
            If _isDarkScheme Then Return AutoGradientEnd(FooterBackColor)
            Return If(_footerGradientEndColor <> Color.Empty,
                      _footerGradientEndColor, AutoGradientEnd(FooterBackColor))
        End Get
        Set(value As Color)
            _footerGradientEndColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterGradientEndColor() As Boolean
        Return _footerGradientEndColor <> Color.Empty
    End Function
    Public Sub ResetFooterGradientEndColor()
        _footerGradientEndColor = Color.Empty
        Me.Invalidate()
    End Sub

    ' ── Subsol: eticheta (caption) cu font și culori proprii ─────────────────────
    Private _footerCaptionFont As Font = Nothing        ' Nothing = fontul controlului
    <Category("K-BOT Arbore - Subsol")>
    <Description("Fontul textului din subsol (toate atributele); nesetat = Font.")>
    Public Property FooterCaptionFont As Font
        Get
            Return If(_footerCaptionFont, Me.Font)
        End Get
        Set(value As Font)
            _footerCaptionFont = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterCaptionFont() As Boolean
        Return _footerCaptionFont IsNot Nothing
    End Function
    Public Sub ResetFooterCaptionFont()
        _footerCaptionFont = Nothing
        Me.Invalidate()
    End Sub

    Private _footerCaptionForeColor As Color = Color.Empty
    <Category("K-BOT Arbore - Subsol")>
    <Description("Culoarea textului din subsol; gol = FooterForeColor. Pe schemă întunecată se ignoră.")>
    Public Property FooterCaptionForeColor As Color
        Get
            If _isDarkScheme Then Return FooterForeColor
            Return If(_footerCaptionForeColor <> Color.Empty, _footerCaptionForeColor, FooterForeColor)
        End Get
        Set(value As Color)
            _footerCaptionForeColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterCaptionForeColor() As Boolean
        Return _footerCaptionForeColor <> Color.Empty
    End Function
    Public Sub ResetFooterCaptionForeColor()
        _footerCaptionForeColor = Color.Empty
        Me.Invalidate()
    End Sub

    ' Gol NU înseamnă aici «din temă», ci «fără plajă proprie»: eticheta se desenează direct pe
    ' banda de subsol. E singura culoare a arborelui cu sensul ăsta, tocmai fiindcă o etichetă
    ' fără fundal e ce vrea oricine în mod normal.
    Private _footerCaptionBackColor As Color = Color.Empty
    <Category("K-BOT Arbore - Subsol")>
    <Description("Fundalul din spatele textului din subsol; gol = fără (se vede banda). " &
                 "Pe schemă întunecată se ignoră, deci textul stă direct pe bandă.")>
    Public Property FooterCaptionBackColor As Color
        Get
            If _isDarkScheme Then Return Color.Empty
            Return _footerCaptionBackColor
        End Get
        Set(value As Color)
            _footerCaptionBackColor = value
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFooterCaptionBackColor() As Boolean
        Return _footerCaptionBackColor <> Color.Empty
    End Function
    Public Sub ResetFooterCaptionBackColor()
        _footerCaptionBackColor = Color.Empty
        Me.Invalidate()
    End Sub

    Private _footerTextAlign As ContentAlignment = ContentAlignment.MiddleLeft
    <Category("K-BOT Arbore - Subsol")>
    <Description("Alinierea textului din subsol, în spațiul rămas între iconiță și buton.")>
    <DefaultValue(ContentAlignment.MiddleLeft)>
    Public Property FooterTextAlign As ContentAlignment
        Get
            Return _footerTextAlign
        End Get
        Set(value As ContentAlignment)
            _footerTextAlign = value
            Me.Invalidate()
        End Set
    End Property

    ' ── Subsol: butonul de strângere ─────────────────────────────────────────────
    Private _footerCollapseButton As Boolean = False
    <Category("K-BOT Arbore - Subsol")>
    <Description("Afișează în subsol butonul care strânge/desfășoară arborele.")>
    <DefaultValue(False)>
    Public Property FooterCollapseButton As Boolean
        Get
            Return _footerCollapseButton
        End Get
        Set(value As Boolean)
            _footerCollapseButton = value
            ' Fără buton nu mai există cale de întoarcere din starea strânsă: o desfacem noi,
            ' altfel arborele ar rămâne îngust pentru totdeauna (aceeași grijă ca la NavList).
            If Not _footerCollapseButton AndAlso _collapsed Then Collapsed = False
            Me.Invalidate()
        End Set
    End Property

    ' Logic/scalat, ca iconițele de bandă — butonul stă lângă ele și trebuie să crească la fel.
    Private _footerCollapseButtonSizeLogic As Integer = 16
    Private _footerCollapseButtonSize As Integer = 16
    <Category("K-BOT Arbore - Subsol")>
    <Description("Latura (px la 96 dpi) a butonului de strângere din subsol.")>
    <DefaultValue(16)>
    Public Property FooterCollapseButtonSize As Integer
        Get
            Return _footerCollapseButtonSizeLogic
        End Get
        Set(value As Integer)
            _footerCollapseButtonSizeLogic = Math.Max(8, value)
            _footerCollapseButtonSize = SX(_footerCollapseButtonSizeLogic)
            Me.Invalidate()
        End Set
    End Property

    Private _footerCollapseButtonPosition As En_FooterButtonPosition = En_FooterButtonPosition.Right
    <Category("K-BOT Arbore - Subsol")>
    <Description("Latura pe care stă butonul de strângere. Pe Left, FooterLeftIcon nu se mai desenează.")>
    <DefaultValue(En_FooterButtonPosition.Right)>
    Public Property FooterCollapseButtonPosition As En_FooterButtonPosition
        Get
            Return _footerCollapseButtonPosition
        End Get
        Set(value As En_FooterButtonPosition)
            _footerCollapseButtonPosition = value
            Me.Invalidate()
        End Set
    End Property

    ' Ca la KBotNavList: două imagini, una pentru fiecare stare. Fără ele se desenează unghiul
    ' («‹» strâns, «›» desfășurat) din FooterForeColor, deci butonul e folosibil din start.
    Private _footerCollapseExpandedImage As Image = Nothing
    <Category("K-BOT Arbore - Subsol")>
    <Description("Pictograma butonului cât timp arborele e DESFĂȘURAT; nesetată = unghi desenat.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property FooterCollapseExpandedImage As Image
        Get
            Return _footerCollapseExpandedImage
        End Get
        Set(value As Image)
            _footerCollapseExpandedImage = value
            Me.Invalidate()
        End Set
    End Property

    Private _footerCollapseCollapsedImage As Image = Nothing
    <Category("K-BOT Arbore - Subsol")>
    <Description("Pictograma butonului cât timp arborele e STRÂNS; nesetată = unghi desenat.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property FooterCollapseCollapsedImage As Image
        Get
            Return _footerCollapseCollapsedImage
        End Get
        Set(value As Image)
            _footerCollapseCollapsedImage = value
            Me.Invalidate()
        End Set
    End Property

    Private _minimumCollapsedWidthLogic As Integer = 100
    Private _minimumCollapsedWidth As Integer = 100
    <Category("K-BOT Arbore - Subsol")>
    <Description("Lățimea (px la 96 dpi) la care se strânge arborele. Implicit 100.")>
    <DefaultValue(100)>
    Public Property MinimumCollapsedWidth As Integer
        Get
            Return _minimumCollapsedWidthLogic
        End Get
        Set(value As Integer)
            _minimumCollapsedWidthLogic = Math.Max(16, value)
            _minimumCollapsedWidth = SX(_minimumCollapsedWidthLogic)
            If _collapsed Then ApplyCollapseExtent()
            Me.Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Starea de strângere. STARE DE RULARE, nu valoare de designer (vezi
    ''' <c>KBotNavList.CollapseState</c>): serializată, ar îngheța formularul strâns și s-ar bate
    ''' cu <c>Size</c>-ul scris tot de designer.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Collapsed As Boolean
        Get
            Return _collapsed
        End Get
        Set(value As Boolean)
            If value = _collapsed Then Return
            If value AndAlso Not _footerCollapseButton Then
                Throw New InvalidOperationException(
                    "Arborele nu se poate strânge cât timp FooterCollapseButton e False.")
            End If
            ApplyCollapsedState(value)
        End Set
    End Property

    <Category("K-BOT Arbore - Subsol")>
    <Description("True => cât timp arborele e strâns, hover-ul pe un nod scoate spre dreapta nodul întreg.")>
    <DefaultValue(True)>
    Public Property CollapsedFlyout As Boolean
        Get
            Return _flyoutEnabled
        End Get
        Set(value As Boolean)
            If value = _flyoutEnabled Then Return
            _flyoutEnabled = value
            If Not _flyoutEnabled Then CancelCollapsedFlyout()
        End Set
    End Property

    ''' <summary>
    ''' Primește și nodul SELECTAT etichetă plutitoare? Implicit **False**: nu.
    '''
    ''' Nodul selectat e singurul pe care operatorul îl are deja în minte — l-a ales el. O etichetă
    ''' care iese peste el nu-i spune nimic nou, dar acoperă vederea de alături exact în locul spre
    ''' care se uită, și o face de fiecare dată când cursorul trece pe deasupra. Restul rândurilor
    ''' rămân neatinse: suprimarea e DOAR pentru rândul selectat, nu o stingere a etichetei.
    ''' </summary>
    <Category("K-BOT Arbore - Subsol")>
    <Description("Scoate etichetă plutitoare și pentru nodul SELECTAT. Implicit False: nodul ales de operator nu mai iese.")>
    <DefaultValue(False)>
    Public Property FlyoutSelectedNode As Boolean
        Get
            Return _flyoutSelectedNode
        End Get
        Set(value As Boolean)
            If value = _flyoutSelectedNode Then Return
            _flyoutSelectedNode = value
            ' Stins cât o etichetă e afară peste nodul selectat, o retragem pe loc.
            If Not _flyoutSelectedNode Then EnsureCollapsedFlyoutStillAllowed()
        End Set
    End Property

    <Category("K-BOT Arbore - Subsol")>
    <Description("Cât așteaptă hover-ul (ms) înainte să scoată nodul. Implicit 250; 0 = imediat.")>
    <DefaultValue(250)>
    Public Property FlyoutDelay As Integer
        Get
            Return _flyoutDelay
        End Get
        Set(value As Integer)
            _flyoutDelay = Math.Max(0, value)
        End Set
    End Property

    <Category("K-BOT Arbore - Subsol")>
    <Description("Durata (ms) desfășurării nodului spre dreapta. Implicit 120; 0 = fără animație.")>
    <DefaultValue(120)>
    Public Property FlyoutSlideDuration As Integer
        Get
            Return _flyoutSlide
        End Get
        Set(value As Integer)
            _flyoutSlide = Math.Max(0, value)
        End Set
    End Property

    ' ══════════════════════════════════════════════════
    ' SEARCH PROPERTIES
    ' ══════════════════════════════════════════════════

    Private _searchPropertiesConfigured As Boolean = False

    Private _searchShow As Boolean = False
    <Category("K-BOT Arbore - Căutare")>
    <Description("Afișează banda de căutare. Fără iconiță de căutare în antet banda e permanentă; " &
                 "cu iconiță, aceasta o deschide/închide.")>
    <DefaultValue(False)>
    Public Property SearchShow As Boolean
        Get
            Return _searchShow
        End Get
        Set(value As Boolean)
            If _searchShow = value Then Return
            _searchShow = value
            ApplySearchShow()
        End Set
    End Property

    Private _searchDefaultText As String = ""
    <Category("K-BOT Arbore - Căutare")>
    <Description("Textul placeholder din caseta de căutare.")>
    <DefaultValue("")>
    Public Property SearchDefaultText As String
        Get
            Return _searchDefaultText
        End Get
        Set(value As String)
            _searchDefaultText = value
            ApplySearchPlaceholder()
            Me.Invalidate()
        End Set
    End Property

    Private _searchType As En_Tree_SearchType = En_Tree_SearchType.SearchType_Contains
    <Category("K-BOT Arbore - Căutare")>
    <Description("Potrivire căutare: conține sau începe cu.")>
    <DefaultValue(En_Tree_SearchType.SearchType_Contains)>
    Public Property SearchType As En_Tree_SearchType
        Get
            Return _searchType
        End Get
        Set(value As En_Tree_SearchType)
            _searchType = value
            _searchPropertiesConfigured = True
        End Set
    End Property

    Private _searchIn As En_Tree_SearchIn = En_Tree_SearchIn.SearchIn_Caption
    <Category("K-BOT Arbore - Căutare")>
    <Description("Unde se caută: caption, tag sau ambele.")>
    <DefaultValue(En_Tree_SearchIn.SearchIn_Caption)>
    Public Property SearchIn As En_Tree_SearchIn
        Get
            Return _searchIn
        End Get
        Set(value As En_Tree_SearchIn)
            _searchIn = value
            _searchPropertiesConfigured = True
        End Set
    End Property

    Private _searchMode As En_Tree_SearchMode = En_Tree_SearchMode.SearchMode_Tree
    <Category("K-BOT Arbore - Căutare")>
    <Description("Modul de afișare a rezultatelor: arbore sau listă.")>
    <DefaultValue(En_Tree_SearchMode.SearchMode_Tree)>
    Public Property SearchMode As En_Tree_SearchMode
        Get
            Return _searchMode
        End Get
        Set(value As En_Tree_SearchMode)
            _searchMode = value
            _searchPropertiesConfigured = True
        End Set
    End Property


    Private _searchBackColor As Color = Color.Empty
    <Category("K-BOT Arbore - Căutare")>
    <Description("Fundalul benzii de căutare; gol = din temă.")>
    Public Property SearchBackColor As Color
        Get
            Return If(_searchBackColor <> Color.Empty, _searchBackColor, _autoSearchBack)
        End Get
        Set(value As Color)
            _searchBackColor = value
            RestyleSearchChildren()
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSearchBackColor() As Boolean
        Return _searchBackColor <> Color.Empty
    End Function
    Public Sub ResetSearchBackColor()
        _searchBackColor = Color.Empty
        RestyleSearchChildren()
        Me.Invalidate()
    End Sub

    Private _searchBoxBackColor As Color = Color.Empty
    <Category("K-BOT Arbore - Căutare")>
    <Description("Fundalul casetei de căutare; gol = din temă (sau fundalul controlului, fără temă).")>
    Public Property SearchBoxBackColor As Color
        Get
            If _searchBoxBackColor <> Color.Empty Then Return _searchBoxBackColor
            If _autoSearchBoxBack <> Color.Empty Then Return _autoSearchBoxBack
            Return Me.BackColor
        End Get
        Set(value As Color)
            _searchBoxBackColor = value
            RestyleSearchChildren()
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSearchBoxBackColor() As Boolean
        Return _searchBoxBackColor <> Color.Empty
    End Function
    Public Sub ResetSearchBoxBackColor()
        _searchBoxBackColor = Color.Empty
        RestyleSearchChildren()
        Me.Invalidate()
    End Sub

    Private _searchBarLabelText As String = "Cautare: "
    <Category("K-BOT Arbore - Căutare")>
    <Description("Eticheta afișată înaintea casetei de căutare.")>
    <DefaultValue("Cautare: ")>
    Public Property SearchBarLabelText As String
        Get
            Return _searchBarLabelText
        End Get
        Set(value As String)
            _searchBarLabelText = value
            _searchPropertiesConfigured = True
            RefreshSearchBarLabel()
            Me.Invalidate()
        End Set
    End Property

    Private _searchBarLabelForeColor As Color = Color.Empty
    <Category("K-BOT Arbore - Căutare")>
    <Description("Culoarea etichetei de căutare; gol = culoarea antetului.")>
    Public Property SearchBarLabelForeColor As Color
        Get
            Return If(_searchBarLabelForeColor <> Color.Empty, _searchBarLabelForeColor, HeaderForeColor)
        End Get
        Set(value As Color)
            _searchBarLabelForeColor = value
            _searchPropertiesConfigured = True
            RestyleSearchChildren()
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSearchBarLabelForeColor() As Boolean
        Return _searchBarLabelForeColor <> Color.Empty
    End Function
    Public Sub ResetSearchBarLabelForeColor()
        _searchBarLabelForeColor = Color.Empty
        RestyleSearchChildren()
        Me.Invalidate()
    End Sub

    ' ── Fonturile benzii de căutare ───────────────────────────────────────────────
    ' Un Font întreg pentru fiecare (etichetă și casetă), editabil din designer cu tot ce are
    ' un font. Au înlocuit perechea SearchBarLabelBold/Italic, care nu putea exprima decât două
    ' atribute din opt. SearchBarFontName/FontSize supraviețuiesc ca accesori ASCUNȘI peste
    ' SearchBarFont, ca fișierele de designer existente și aplicatorul XML (Tree.Builder) să
    ' compileze neatinse.
    Private _searchBarLabelFont As Font = Nothing        ' Nothing = fontul controlului
    <Category("K-BOT Arbore - Căutare")>
    <Description("Fontul etichetei de căutare (toate atributele); nesetat = fontul controlului.")>
    Public Property SearchBarLabelFont As Font
        Get
            Return If(_searchBarLabelFont, Me.Font)
        End Get
        Set(value As Font)
            _searchBarLabelFont = value
            _searchPropertiesConfigured = True
            UpdateSearchBarLabelFont()
            If _isSearchMode Then PositionSearchTextBox()
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSearchBarLabelFont() As Boolean
        Return _searchBarLabelFont IsNot Nothing
    End Function
    Public Sub ResetSearchBarLabelFont()
        _searchBarLabelFont = Nothing
        UpdateSearchBarLabelFont()
        Me.Invalidate()
    End Sub

    Private _searchBarFont As Font = New Font("Calibri", 10.0F)
    <Category("K-BOT Arbore - Căutare")>
    <Description("Fontul casetei de căutare (toate atributele).")>
    Public Property SearchBarFont As Font
        Get
            Return If(_searchBarFont, Me.Font)
        End Get
        Set(value As Font)
            _searchBarFont = value
            _searchPropertiesConfigured = True
            UpdateSearchTextBoxFont()
            If _isSearchMode Then PositionSearchTextBox()
            Me.Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeSearchBarFont() As Boolean
        Return _searchBarFont IsNot Nothing AndAlso
               Not (_searchBarFont.Name = "Calibri" AndAlso _searchBarFont.Size = 10.0F AndAlso
                    _searchBarFont.Style = FontStyle.Regular)
    End Function
    Public Sub ResetSearchBarFont()
        SearchBarFont = New Font("Calibri", 10.0F)
    End Sub

    Private Sub UpdateSearchBarLabelFont()
        If _searchBarLabel Is Nothing Then Return
        _searchBarLabel.Font = SearchBarLabelFont
    End Sub

    Friend Sub MarkSearchConfigured()
        _searchPropertiesConfigured = True
    End Sub

    ' ── Accesori legacy peste SearchBarFont (calea XML/FOREXE + designerele existente) ────
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SearchBarFontName As String
        Get
            Return SearchBarFont.Name
        End Get
        Set(value As String)
            If String.IsNullOrEmpty(value) Then Return
            SearchBarFont = New Font(value, SearchBarFont.Size, SearchBarFont.Style)
        End Set
    End Property

    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SearchBarFontSize As Single
        Get
            Return SearchBarFont.Size
        End Get
        Set(value As Single)
            If value <= 0 Then Return
            SearchBarFont = New Font(SearchBarFont.Name, value, SearchBarFont.Style)
        End Set
    End Property

    Private _searchClearButton As Boolean = False
    <Category("K-BOT Arbore - Căutare")>
    <Description("Afișează butonul de golire în caseta de căutare.")>
    <DefaultValue(False)>
    Public Property SearchClearButton As Boolean
        Get
            Return _searchClearButton
        End Get
        Set(value As Boolean)
            If _searchClearButton = value Then Return
            _searchClearButton = value
            RefreshClearButton()
            Me.Invalidate()
        End Set
    End Property

    ' SearchClearButtonPadding s-a mutat în partiala .Paddings (toate marginile într-un singur fișier).

    Private _searchClearButtonImage As Image = Nothing
    <Category("K-BOT Arbore - Căutare")>
    <Description("Imaginea butonului de golire; nesetată = glifa «✕».")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property SearchClearButtonImage As Image
        Get
            Return _searchClearButtonImage
        End Get
        Set(value As Image)
            _searchClearButtonImage = value
            ApplyClearButtonLook()
            If _isSearchMode Then PositionSearchTextBox()
            Me.Invalidate()
        End Set
    End Property

    ' Lățimea totală rezervată butonului ✕ = glifă/imagine + padding-ul din jur.
    Friend ReadOnly Property SearchClearButtonWidth As Integer
        Get
            ' Glifa desenată de noi e o măsură LOGICĂ (deci SX); o IMAGINE a operatorului se ia
            ' cât e — pictogramele nu se măresc, se aleg la rezoluția potrivită.
            Dim latimeGlifa As Integer = If(_searchClearButtonImage IsNot Nothing,
                                            _searchClearButtonImage.Width, SX(CLEAR_BTN_WIDTH))
            Return latimeGlifa + SearchClearButtonPaddingPx.Horizontal
        End Get
    End Property

    Private _scrollBarTheme As En_ScrollBarTheme = En_ScrollBarTheme.Explorer
    <Category("K-BOT Arbore")>
    <Description("Tema barei de derulare verticale (Default/Explorer/DarkMode).")>
    <DefaultValue(En_ScrollBarTheme.Explorer)>
    Public Property ScrollBarTheme As En_ScrollBarTheme
        Get
            Return _scrollBarTheme
        End Get
        Set(value As En_ScrollBarTheme)
            _scrollBarTheme = value
            ApplyScrollBarTheme()
        End Set
    End Property

    Private _tooltipShow As Boolean = True
    <Category("K-BOT Arbore - Tooltip")>
    <Description("Activează tooltip-urile de nod.")>
    <DefaultValue(True)>
    Public Property TooltipShow As Boolean
        Get
            Return _tooltipShow
        End Get
        Set(value As Boolean)
            _tooltipShow = value
        End Set
    End Property

    Private _tooltipBackColor As Color = Color.Empty
    <Category("K-BOT Arbore - Tooltip")>
    <Description("Fundalul tooltip-ului; gol = din temă.")>
    Public Property TooltipBackColor As Color
        Get
            Return If(_tooltipBackColor <> Color.Empty, _tooltipBackColor, _autoTooltipBack)
        End Get
        Set(value As Color)
            _tooltipBackColor = value
        End Set
    End Property
    Public Function ShouldSerializeTooltipBackColor() As Boolean
        Return _tooltipBackColor <> Color.Empty
    End Function
    Public Sub ResetTooltipBackColor()
        _tooltipBackColor = Color.Empty
    End Sub

    Private _tooltipForeColor As Color = Color.Empty
    <Category("K-BOT Arbore - Tooltip")>
    <Description("Culoarea textului din tooltip; gol = din temă.")>
    Public Property TooltipForeColor As Color
        Get
            Return If(_tooltipForeColor <> Color.Empty, _tooltipForeColor, _autoTooltipFore)
        End Get
        Set(value As Color)
            _tooltipForeColor = value
        End Set
    End Property
    Public Function ShouldSerializeTooltipForeColor() As Boolean
        Return _tooltipForeColor <> Color.Empty
    End Function
    Public Sub ResetTooltipForeColor()
        _tooltipForeColor = Color.Empty
    End Sub

    ' Când True, tooltip-ul se afișează DOAR dacă cursorul se află deasupra
    ' iconului stânga al nodului (cu padding PaddingTooltipIconHit).
    ' Dacă nodul nu are icon stânga → fallback la comportamentul normal (tot rândul).
    ' Subordonat lui TooltipShow: dacă TooltipShow = False, această setare e ignorată.
    Private _tooltipShowOnlyOnLeftIcon As Boolean = False
    <Category("K-BOT Arbore - Tooltip")>
    <Description("Tooltip-ul apare doar când cursorul e pe iconița din stânga a nodului.")>
    <DefaultValue(False)>
    Public Property TooltipShowOnlyOnLeftIcon As Boolean
        Get
            Return _tooltipShowOnlyOnLeftIcon
        End Get
        Set(value As Boolean)
            _tooltipShowOnlyOnLeftIcon = value
        End Set
    End Property

    Private _treeListViewEnabled As Boolean = False          ' master switch
    <Category("K-BOT Arbore - Coloane")>
    <Description("Comutatorul principal al modului TreeListView (coloane pe rânduri).")>
    <DefaultValue(False)>
    Public Property TreeListView As Boolean
        Get
            Return _treeListViewEnabled
        End Get
        Set(value As Boolean)
            _treeListViewEnabled = value
            If Not value Then
                _activeColFilters.Clear()
                _colFilterActive = False
                _colFilterSet.Clear()
                _activeColFilterPopup?.Close()
                _activeColFilterPopup = Nothing
            End If
            Me.Invalidate()
        End Set
    End Property

    Private _dynamicColumns As Boolean = True               ' True = comportament actual (ColHeaderText per nod)
    <Category("K-BOT Arbore - Coloane")>
    <Description("True = coloane rezolvate per-nod din ColHeaderText; False = bandă statică pe ColumnsLevel.")>
    <DefaultValue(True)>
    Public Property DynamicColumns As Boolean
        Get
            Return _dynamicColumns
        End Get
        Set(value As Boolean)
            _dynamicColumns = value
            Me.Invalidate()
        End Set
    End Property

    Private _columnsLevel As Integer = -1                   ' DynamicColumns=False: nivelul care primeste coloane
    <Category("K-BOT Arbore - Coloane")>
    <Description("Nivelul care primește coloane când DynamicColumns=False; -1 = niciunul.")>
    <DefaultValue(-1)>
    Public Property ColumnsLevel As Integer
        Get
            Return _columnsLevel
        End Get
        Set(value As Integer)
            _columnsLevel = value
            Me.Invalidate()
        End Set
    End Property

    Friend Sub UpdateSearchTextBoxFont()
        If _searchTextBox Is Nothing Then Return
        _searchTextBox.Font = SearchBarFont
        If _searchClearBtn IsNot Nothing Then _searchClearBtn.Font = SearchBarFont
    End Sub

    Private ReadOnly Property ScrollBarWidth As Integer
        Get
            Return If(_vScroll IsNot Nothing AndAlso _vScroll.Visible, _vScroll.Width, 0)
        End Get
    End Property

    ''' <summary>
    ''' Grosimea barei de derulare, la scara arborelui (felia 0040).
    '''
    ''' <para><c>SystemInformation.VerticalScrollBarWidth</c> răspunde pentru DPI-ul de la
    ''' pornirea procesului, nu pentru monitorul pe care stă acum fereastra: pe un al doilea ecran
    ''' la altă scalare bara ieșea mai îngustă (sau mai lată) decât spațiul rezervat ei, iar textul
    ''' nodurilor fie se tăia, fie lăsa o dungă goală. Perechea …ForDpi întreabă pentru un DPI
    ''' anume — i-l dăm pe al nostru, cel din <see cref="AppScaling"/>, ca bara și restul
    ''' geometriei să crească din aceeași sursă (inclusiv când operatorul fixează scara la 100%).</para>
    ''' </summary>
    Private ReadOnly Property ScrollBarThicknessPx As Integer
        Get
            Return SystemInformation.GetVerticalScrollBarWidthForDpi(CInt(Math.Round(96 * DpiScaleX)))
        End Get
    End Property
End Class
