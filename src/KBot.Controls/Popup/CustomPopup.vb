Option Strict On
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Meniul contextual K-BOT: arată ca meniul de sistem (rândurile, banda de pictograme din stânga,
''' litera de acces subliniată, evidențierea care urmează mouse-ul ȘI tastele), dar e desenat de noi
''' — deci ia culorile schemei active, exact ca restul controalelor.
'''
''' De ce nu <c>ContextMenuStrip</c>: fața lui e desenată de <c>ToolStripRenderer</c>, iar sub o
''' schemă întunecată rămâne o fâșie albă cu margini de sistem, la fel cum <c>ComboBox</c> rămânea
''' alb înainte de <c>KBotComboBox</c>. Un renderer propriu ar fi rezolvat culorile, dar nu și
''' cerințele care contează aici: o pictogramă pe fiecare rând, o selecție ALEASĂ DE APELANT în
''' clipa deschiderii și tastatura ca drum egal cu mouse-ul.
'''
''' <para><b>E o FEREASTRĂ, nu un control de pus pe formular.</b> Se construiește în cod, se arată
''' cu <see cref="ShowAt"/> / <see cref="ShowBelow"/> / <see cref="ShowAtCursor"/> și se închide
''' singură — la un clic pe un rând, la Esc, sau când primește <c>Deactivate</c> (adică operatorul
''' a dat clic în altă parte). Fiind arătată nemodal, WinForms O ELIBEREAZĂ SINGURĂ la închidere:
''' <b>nu o pune într-un <c>Using</c></b>, altfel se distruge înainte s-o vadă cineva. Rezultatul
''' se citește din <see cref="ItemClicked"/> (sau din <see cref="ClickedItem"/> în
''' <c>FormClosed</c>); <c>Nothing</c> = a fost respinsă.</para>
'''
''' <para><b>Compromisul activării.</b> Spre deosebire de <c>KBotNavFlyout</c>/<c>TreeNodeFlyout</c>
''' — care sunt <c>WS_EX_NOACTIVATE</c> fiindcă n-au nevoie decât să fie văzute — popup-ul ăsta SE
''' ACTIVEAZĂ: fără activare nu există focus de tastatură, iar tastatura e jumătate din cerință.
''' Prețul e că bara de titlu a formularului de dedesubt se vede «inactivă» cât timp meniul e
''' deschis. Restul trucurilor rămân: <c>WS_EX_TOOLWINDOW</c> (fără buton în bara de activități) și
''' <c>CS_DROPSHADOW</c> (umbra pe care o are orice meniu de sistem).</para>
'''
''' <para>Contractul de culoare e cel al casei (vezi <c>AdvancedTreeControl</c>,
''' <c>KBotComboBox</c>): <c>Color.Empty</c> = «din temă», orice culoare pusă explicit câștigă și
''' supraviețuiește oricărei comutări de schemă.</para>
''' </summary>
<ToolboxItem(False)>
<DesignerCategory("Code")>
<DefaultEvent("ItemClicked")>
Public Class CustomPopup
    Inherits Form
    Implements IThemedControl

    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const CS_DROPSHADOW As Integer = &H20000

    ' Măsuri logice (px @96dpi), scalate la DPI la fiecare recalculare.
    Private Const PadXLogical As Integer = 8       ' aer stânga/dreapta, în interiorul ramei
    Private Const PadYLogical As Integer = 4       ' aer sus/jos, în interiorul ramei
    Private Const IconGapLogical As Integer = 6    ' între banda de pictograme și text
    Private Const RowAirLogical As Integer = 8     ' înălțimea rândului = fontul + atât
    Private Const SeparatorLogical As Integer = 7  ' înălțimea slotului de separator
    Private Const BorderThickness As Integer = 1

    ' ── Culorile «auto» (fallback pentru orice proprietate lăsată Empty) ──────────
    ' Valorile inițiale = look-ul light implicit, ca un popup arătat fără nicio schemă aplicată
    ' (bancul de probă, un host netematizat) să arate rezonabil.
    Private _autoBack As Color = Color.White
    Private _autoBorder As Color = Color.FromArgb(170, 170, 170)
    Private _autoFore As Color = Color.FromArgb(30, 30, 30)
    Private _autoDisabled As Color = Color.FromArgb(140, 140, 140)
    Private _autoHighlightBack As Color = Color.FromArgb(0, 122, 204)
    Private _autoHighlightFore As Color = Color.White
    Private _autoSeparator As Color = Color.FromArgb(200, 200, 200)

    Private _popupBackColor As Color = Color.Empty
    Private _borderColor As Color = Color.Empty
    Private _itemForeColor As Color = Color.Empty
    Private _disabledForeColor As Color = Color.Empty
    Private _highlightBackColor As Color = Color.Empty
    Private _highlightForeColor As Color = Color.Empty
    Private _separatorColor As Color = Color.Empty

    Private _cornerRadius As Integer = -1
    Private _itemGradient As Integer = 14
    Private _imageSize As Integer = 16
    Private _itemHeight As Integer = 0
    Private _minimumPopupWidth As Integer = 120
    Private _maximumPopupWidth As Integer = 420

    Private ReadOnly _items As New CustomPopupItemCollection()
    Private _rows As Rectangle() = Array.Empty(Of Rectangle)()
    Private _naturalSize As Size = Size.Empty
    Private _layoutDirty As Boolean = True
    Private _selectedIndex As Integer = -1
    Private _scroll As Integer = 0
    Private _showMnemonics As Boolean
    Private _closing As Boolean = False
    ''' <summary>Controlul care a desfășurat meniul, dacă știe să rămână aprins (vezi <see cref="IPopupAnchor"/>).</summary>
    Private _anchor As IPopupAnchor

    ''' <summary>Popup gol; elementele se adaugă în <see cref="Items"/>.</summary>
    Public Sub New()
        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        ControlBox = False
        MinimizeBox = False
        MaximizeBox = False
        Text = String.Empty
        ' Fără autoscalare: poziția și mărimea se calculează în px DEJA scalați (ThemeShapes.ScaleDpi),
        ' iar o a doua ajustare a formularului ar muta meniul de sub cursor.
        AutoScaleMode = AutoScaleMode.None
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)

        _items.Owner = Me
        ' Windows arată sublinierile de acces doar după Alt, dacă așa e configurat sistemul —
        ' pornim de la setarea lui și le aprindem la prima tastă (vezi ProcessCmdKey).
        _showMnemonics = SystemInformation.MenuAccessKeysUnderlined

        ' Meniul se tematizează SINGUR la construcție. Fiind o fereastră de sine stătătoare, nu-l
        ' prinde traversarea gazdei, iar un apelant care ar trebui să-și amintească de fiecare dată
        ' un ThemeManager.Apply e chiar felul în care se strecoară o fereastră albă într-o schemă
        ' întunecată. Comutarea de schemă cât timp meniul e DESCHIS nu se urmărește, dinadins:
        ' trăiește câteva secunde, iar ThemeManager.SetScheme îl prinde oricum prin OpenForms.
        ApplyTheme(ThemeManager.Current)
    End Sub

    ''' <summary>Popup cu elemente, fără nimic selectat.</summary>
    Public Sub New(items As IEnumerable(Of CustomPopupItem))
        Me.New(items, Nothing)
    End Sub

    ''' <summary>
    ''' Popup cu elemente ȘI cu rândul evidențiat din clipa deschiderii — cerința pentru care
    ''' există constructorul ăsta: meniul se deschide DEJA pe alegerea curentă, ca operatorul să
    ''' poată confirma cu Enter fără să caute rândul.
    '''
    ''' <paramref name="selectedKey"/> gol/Nothing = nimic selectat. O cheie care nu există ARUNCĂ
    ''' <see cref="ArgumentException"/> — un meniu care se deschide tăcut pe nimic e chiar felul de
    ''' eșec pe care regula casei îl interzice.
    ''' </summary>
    Public Sub New(items As IEnumerable(Of CustomPopupItem), selectedKey As String)
        Me.New()
        Try
            If items IsNot Nothing Then
                For Each it As CustomPopupItem In items
                    _items.Add(it)
                Next
            End If
            ' «Me.» e OBLIGATORIU: VB e case-insensitive, deci parametrul «selectedKey» umbrește
            ' proprietatea «SelectedKey», iar atribuirea necalificată ar scrie parametrul în el
            ' însuși — un no-op perfect tăcut (capcana din feliile 0010 / 0019, prinsă aici de
            ' testul «Constructorul_deschide_meniul_pe_cheia_ceruta»).
            If Not String.IsNullOrWhiteSpace(selectedKey) Then Me.SelectedKey = selectedKey
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.New", ex)
            Throw
        End Try
    End Sub

    ' =====================================================================
    ' ELEMENTE ȘI SELECȚIE
    ' =====================================================================

    ''' <summary>Rândurile meniului, în ordinea afișării.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Items As CustomPopupItemCollection
        Get
            Return _items
        End Get
    End Property

    ''' <summary>
    ''' Rândul EVIDENȚIAT: unul singur, împărțit de mouse și de tastatură, exact ca la un meniu de
    ''' sistem (survolarea mută evidențierea, săgețile la fel). -1 = niciunul. O poziție care nu se
    ''' poate selecta (separator, element dezactivat, în afara colecției) se citește ca -1.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedIndex As Integer
        Get
            Return _selectedIndex
        End Get
        Set(value As Integer)
            Dim normalized As Integer = If(IsSelectable(value), value, -1)
            If normalized = _selectedIndex Then Return
            _selectedIndex = normalized
            EnsureVisible(normalized)
            Invalidate()
            RaiseEvent SelectedItemChanged(Me, EventArgs.Empty)
        End Set
    End Property

    ''' <summary>Elementul evidențiat, sau Nothing.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SelectedItem As CustomPopupItem
        Get
            If _selectedIndex < 0 OrElse _selectedIndex >= _items.Count Then Return Nothing
            Return _items(_selectedIndex)
        End Get
    End Property

    ''' <summary>
    ''' Cheia rândului evidențiat. La scriere: gol/Nothing golește selecția, o cheie necunoscută
    ''' ARUNCĂ (vezi <see cref="ItemByKey"/>).
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedKey As String
        Get
            Dim it As CustomPopupItem = SelectedItem
            Return it?.Key
        End Get
        Set(value As String)
            If String.IsNullOrWhiteSpace(value) Then
                SelectedIndex = -1
                Return
            End If
            SelectedIndex = _items.IndexOf(ItemByKey(value))
        End Set
    End Property

    ''' <summary>
    ''' Elementul ales de operator, sau Nothing dacă meniul a fost respins (Esc / clic în afară).
    ''' Se citește după închidere — în <see cref="ItemClicked"/> sau în <c>FormClosed</c>.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ClickedItem As CustomPopupItem

    ''' <summary>Elementul cu cheia dată. O cheie necunoscută ARUNCĂ — fără no-op tăcut.</summary>
    Public Function ItemByKey(key As String) As CustomPopupItem
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Cheia nu poate fi goală.", NameOf(key))
        For Each it As CustomPopupItem In _items
            If Not it.IsSeparator AndAlso String.Equals(it.Key, key, StringComparison.Ordinal) Then Return it
        Next
        Throw New ArgumentException("Element inexistent în popup: «" & key & "».", NameOf(key))
    End Function

    ''' <summary>Varianta care nu aruncă — pentru cine trebuie doar să ÎNTREBE dacă există cheia.</summary>
    Public Function ContainsKey(key As String) As Boolean
        If String.IsNullOrWhiteSpace(key) Then Return False
        For Each it As CustomPopupItem In _items
            If Not it.IsSeparator AndAlso String.Equals(it.Key, key, StringComparison.Ordinal) Then Return True
        Next
        Return False
    End Function

    ''' <summary>Ridicat când operatorul alege un rând — chiar înainte ca fereastra să se închidă.</summary>
    Public Event ItemClicked As EventHandler(Of CustomPopupItemEventArgs)

    ''' <summary>Ridicat la fiecare mutare a evidențierii (mouse sau tastatură).</summary>
    Public Event SelectedItemChanged As EventHandler

    ' =====================================================================
    ' GEOMETRIE
    ' =====================================================================

    ''' <summary>Latura pictogramei, în px logici. Banda din stânga se rezervă doar dacă are cine s-o umple.</summary>
    <DefaultValue(16)>
    Public Property ImageSize As Integer
        Get
            Return _imageSize
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _imageSize Then Return
            _imageSize = clamped
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>Înălțimea unui rând, px logici. 0 = derivată din font (implicit).</summary>
    <DefaultValue(0)>
    Public Property ItemHeight As Integer
        Get
            Return _itemHeight
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _itemHeight Then Return
            _itemHeight = clamped
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>Lățimea minimă a meniului, px logici.</summary>
    <DefaultValue(120)>
    Public Property MinimumPopupWidth As Integer
        Get
            Return _minimumPopupWidth
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _minimumPopupWidth Then Return
            _minimumPopupWidth = clamped
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>Lățimea maximă, px logici — peste ea textul se taie cu «…».</summary>
    <DefaultValue(420)>
    Public Property MaximumPopupWidth As Integer
        Get
            Return _maximumPopupWidth
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(1, value)
            If clamped = _maximumPopupWidth Then Return
            _maximumPopupWidth = clamped
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>Raza colțurilor, px logici. -1 = din temă (Style.CornerRadius), 0 = pătrat.</summary>
    <DefaultValue(-1)>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(-1, value)
            If clamped = _cornerRadius Then Return
            _cornerRadius = clamped
            ApplyRegion()
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Intensitatea gradientului de pe rândul evidențiat (0..100; 0 = umplere plată), exact ca
    ''' <c>KBotNavList.ItemGradient</c> și prin același <c>ThemeShapes.FillModern</c>: nu introduce
    ''' nicio culoare nouă, doar două nuanțe derivate din culoarea de evidențiere a schemei.
    ''' </summary>
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
        End Set
    End Property

    ' =====================================================================
    ' CULORI (Color.Empty = «din temă»)
    ' =====================================================================

    ''' <summary>Fundalul meniului. Gol = din temă (SurfaceAlt).</summary>
    Public Property PopupBackColor As Color
        Get
            Return _popupBackColor
        End Get
        Set(value As Color)
            _popupBackColor = value
            MyBase.BackColor = EffectiveBackColor
            Invalidate()
        End Set
    End Property

    ''' <summary>Conturul de 1 px. Gol = din temă (Border).</summary>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
            Invalidate()
        End Set
    End Property

    ''' <summary>Textul rândurilor. Gol = din temă (Text).</summary>
    Public Property ItemForeColor As Color
        Get
            Return _itemForeColor
        End Get
        Set(value As Color)
            _itemForeColor = value
            Invalidate()
        End Set
    End Property

    ''' <summary>Textul rândurilor dezactivate. Gol = din temă (DisabledText).</summary>
    Public Property DisabledForeColor As Color
        Get
            Return _disabledForeColor
        End Get
        Set(value As Color)
            _disabledForeColor = value
            Invalidate()
        End Set
    End Property

    ''' <summary>Fundalul rândului evidențiat. Gol = din temă (Accent).</summary>
    Public Property HighlightBackColor As Color
        Get
            Return _highlightBackColor
        End Get
        Set(value As Color)
            _highlightBackColor = value
            Invalidate()
        End Set
    End Property

    ''' <summary>Textul rândului evidențiat. Gol = din temă (AccentText).</summary>
    Public Property HighlightForeColor As Color
        Get
            Return _highlightForeColor
        End Get
        Set(value As Color)
            _highlightForeColor = value
            Invalidate()
        End Set
    End Property

    ''' <summary>Linia separatorilor. Gol = din temă (Border).</summary>
    Public Property SeparatorColor As Color
        Get
            Return _separatorColor
        End Get
        Set(value As Color)
            _separatorColor = value
            Invalidate()
        End Set
    End Property

    ' ── Culorile efective (proprietatea dacă e aleasă, altfel «auto» din temă) ────

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveBackColor As Color
        Get
            Return If(_popupBackColor = Color.Empty, _autoBack, _popupBackColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveBorderColor As Color
        Get
            Return If(_borderColor = Color.Empty, _autoBorder, _borderColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveItemForeColor As Color
        Get
            Return If(_itemForeColor = Color.Empty, _autoFore, _itemForeColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveDisabledForeColor As Color
        Get
            Return If(_disabledForeColor = Color.Empty, _autoDisabled, _disabledForeColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveHighlightBackColor As Color
        Get
            Return If(_highlightBackColor = Color.Empty, _autoHighlightBack, _highlightBackColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveHighlightForeColor As Color
        Get
            Return If(_highlightForeColor = Color.Empty, _autoHighlightFore, _highlightForeColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveSeparatorColor As Color
        Get
            Return If(_separatorColor = Color.Empty, _autoSeparator, _separatorColor)
        End Get
    End Property

    ''' <summary>
    ''' Reaplică schema. Culorile alese explicit nu se ating — doar sloturile «auto» se rescriu.
    ''' Popup-ul implementează <c>IThemedControl</c> ca <c>ThemeManager.Traverse</c> să se OPREASCĂ
    ''' la el: fără asta, regula generică de formular i-ar scrie <c>BackColor</c>-ul peste
    ''' <see cref="PopupBackColor"/> la fiecare comutare de schemă.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            _autoBack = p.SurfaceAltColor
            _autoBorder = p.BorderColor
            _autoFore = p.TextColor
            _autoDisabled = p.DisabledTextColor
            _autoHighlightBack = p.AccentColor
            _autoHighlightFore = p.AccentTextColor
            ' Separatorul e conturul tras spre fundal: o linie plină de culoarea ramei taie meniul
            ' în două în loc să grupeze rândurile.
            _autoSeparator = ThemeShapes.Blend(p.BorderColor, p.SurfaceAltColor, 0.45)

            ' MyBase, nu Me: scrisul temei nu are voie să treacă drept alegere a apelantului.
            MyBase.BackColor = EffectiveBackColor
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.ApplyTheme", ex)
        End Try
    End Sub

    ' =====================================================================
    ' AȘEZARE ȘI DESCHIDERE
    ' =====================================================================

    ''' <summary>
    ''' Deschide meniul cu colțul din stânga-sus în punctul dat (coordonate de ECRAN). Dacă nu
    ''' încape, se răstoarnă spre stânga și/sau în sus, ca orice meniu de sistem.
    ''' </summary>
    Public Sub ShowAt(anchor As Control, screenPoint As Point)
        ' Ancorat într-un PUNCT nu există nimic la care să se alinieze răsturnarea: meniul se
        ' deschide spre stânga, respectiv în sus, chiar din punctul cerut.
        ShowCore(anchor, screenPoint, screenPoint.X, screenPoint.Y)
    End Sub

    ''' <summary>Deschide meniul la cursor — cazul clasic al clicului dreapta.</summary>
    Public Sub ShowAtCursor(anchor As Control)
        ShowAt(anchor, Cursor.Position)
    End Sub

    ''' <summary>
    ''' Deschide meniul lipit sub <paramref name="anchor"/>, aliniat la stânga lui — cazul unui
    ''' buton care desfășoară o listă. Când nu încape dedesubt se răstoarnă DEASUPRA butonului,
    ''' iar când nu încape spre dreapta se aliniază la dreapta lui, nu lângă el.
    ''' </summary>
    Public Sub ShowBelow(anchor As Control)
        ArgumentNullException.ThrowIfNull(anchor)
        ShowBelow(anchor, anchor.ClientRectangle)
    End Sub

    ''' <summary>
    ''' Deschide meniul sub un DREPTUNGHI din interiorul lui <paramref name="anchor"/>
    ''' (coordonate client). Există pentru butoanele DESENATE, care n-au <c>Bounds</c> propriu:
    ''' butonul de opțiuni al lui <c>KBotCaptionBar</c>, butonul de strângere al arborelui,
    ''' iconițele de capăt. Fără el, gazda ar trebui să traducă singură geometria — adică s-o
    ''' reproducă, adică s-o lase în urmă la prima schimbare.
    ''' </summary>
    Public Sub ShowBelow(anchor As Control, anchorRect As Rectangle)
        ArgumentNullException.ThrowIfNull(anchor)
        If anchorRect.IsEmpty Then Throw New ArgumentException(
            "Dreptunghiul de ancorare e gol — butonul sub care s-ar deschide meniul nu e vizibil.",
            NameOf(anchorRect))

        Dim sus As Point = anchor.PointToScreen(New Point(anchorRect.Left, anchorRect.Top))
        ' Alternativele de răsturnare sunt CELELALTE DOUĂ laturi ale butonului: marginea lui
        ' dreaptă devine marginea dreaptă a meniului, vârful lui devine baza meniului. Așa,
        ' un buton lipit de marginea ecranului desfășoară meniul PESTE el, nu alături de el.
        ShowCore(anchor, New Point(sus.X, sus.Y + anchorRect.Height),
                 sus.X + anchorRect.Width, sus.Y)
    End Sub

    ' Drumul comun: măsoară, așază pe ecranul potrivit, arată și ia focusul.
    ' Punct de intrare (creare de fereastră, geometrie de ecran) => loghează și RE-ARUNCĂ.
    Private Sub ShowCore(anchor As Control, at As Point, altRight As Integer, altBottom As Integer)
        Try
            ArgumentNullException.ThrowIfNull(anchor)
            If _items.Count = 0 Then Throw New InvalidOperationException("CustomPopup fără elemente nu se poate deschide.")

            ' Fontul ambiant al gazdei: meniul e o fereastră de sine stătătoare, deci nu-l
            ' moștenește singur, iar un meniu cu alt font decât formularul se vede imediat.
            If anchor.Font IsNot Nothing Then MyBase.Font = anchor.Font

            EnsureLayout()
            Dim wa As Rectangle = Screen.FromPoint(at).WorkingArea
            Bounds = FitToWorkArea(_naturalSize, at, altRight, altBottom, wa)
            _scroll = 0
            EnsureVisible(_selectedIndex)

            Owner = anchor.FindForm()

            ' Butonul care ne-a desfășurat rămâne aprins cât suntem pe ecran — meniul trebuie să
            ' pară continuarea lui, nu o fereastră care plutește alături. Stingerea o facem NOI,
            ' în OnFormClosed: Esc, clicul în afară și alegerea unui rând sunt trei drumuri, iar
            ' o gazdă care ar trebui să le acopere pe toate ar uita unul.
            _anchor = TryCast(anchor, IPopupAnchor)
            _anchor?.SetPopupOpen(True)

            Show()
            ' Fără asta fereastra apare, dar tastele rămân la formularul de dedesubt.
            Activate()
            Focus()
        Catch ex As Exception
            ' Dacă deschiderea a crăpat DUPĂ ce am aprins butonul, îl stingem aici: altfel ar
            ' rămâne aprins la nesfârșit, arătând un meniu care nu s-a deschis niciodată.
            _anchor?.SetPopupOpen(False)
            _anchor = Nothing
            GlobalErrorLog.Write("CustomPopup.ShowCore", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Unde încape dreptunghiul dorit. Colțul preferat e <paramref name="at"/>; când meniul nu
    ''' încape, cele două ALTERNATIVE spun ce latură devine capătul opus:
    ''' <paramref name="altRight"/> = X-ul care devine marginea DREAPTĂ, <paramref name="altBottom"/>
    ''' = Y-ul care devine marginea de JOS. Pentru un punct sunt chiar coordonatele lui (meniul
    ''' crește spre stânga/în sus din punct); pentru un buton sunt marginea lui dreaptă și vârful
    ''' lui, deci meniul se aliniază la buton în loc să se mute alături de el. La urmă se strânge
    ''' în zona de lucru.
    '''
    ''' Funcție pură: aici stă toată regula de așezare, ca s-o poată ține fixă testele.
    ''' </summary>
    Friend Shared Function FitToWorkArea(desired As Size, at As Point,
                                         altRight As Integer, altBottom As Integer,
                                         workArea As Rectangle) As Rectangle
        Dim w As Integer = Math.Min(desired.Width, workArea.Width)
        Dim h As Integer = Math.Min(desired.Height, workArea.Height)

        Dim x As Integer = at.X
        If x + w > workArea.Right Then x = altRight - w
        If x < workArea.Left Then x = workArea.Left
        If x + w > workArea.Right Then x = workArea.Right - w

        Dim y As Integer = at.Y
        If y + h > workArea.Bottom Then y = altBottom - h
        If y < workArea.Top Then y = workArea.Top
        If y + h > workArea.Bottom Then y = workArea.Bottom - h

        Return New Rectangle(x, y, w, h)
    End Function

    ''' <summary>
    ''' Închide meniul cu un rezultat. <paramref name="picked"/> Nothing = respins (Esc, clic în
    ''' afară). Se apără de reintrare: <c>Close</c> ridică <c>Deactivate</c>, care ar reintra aici.
    ''' </summary>
    Friend Sub CloseWith(picked As CustomPopupItem, index As Integer)
        If _closing Then Return
        _closing = True
        Try
            _ClickedItem = picked
            _lastClosedAt = DateTime.UtcNow
            If picked IsNot Nothing Then
                RaiseEvent ItemClicked(Me, New CustomPopupItemEventArgs(picked, index))
            End If
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.CloseWith", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Alege rândul dat (dacă se poate alege) și închide. Drumul comun al mouse-ului și al
    ''' tastaturii.
    '''
    ''' <para>Un rând-CURSOR se evidențiază, dar NU se alege și nu închide meniul (felia 0036-01):
    ''' toată ideea lui e să vezi efectul în timp ce tragi. Enter pe el nu e o alegere, deci nu
    ''' face nimic — iar asta nu e un no-op tăcut, ci refuzul cinstit al unui rând care n-a promis
    ''' niciodată o alegere (n-are nici literă de acces, tocmai de aceea).</para>
    ''' </summary>
    Friend Sub ActivateItem(index As Integer)
        If Not IsSelectable(index) Then Return
        SelectedIndex = index
        If IsSliderRow(index) Then Return
        CloseWith(_items(index), index)
    End Sub

    ''' <summary>
    ''' Clic în afară = meniu respins. Exact ce face orice meniu de sistem.
    '''
    ''' <para><b>Excepția:</b> cât timp se predă valoarea unui rând-cursor. Gazda face atunci
    ''' lucruri care REAȘAZĂ toate ferestrele deschise (mărimea textului rescrie fonturile
    ''' aplicației), iar fereastra de dedesubt se reactivează — deci am pierde activarea din
    ''' PROPRIA noastră comandă, nu dintr-un clic al operatorului. Fără garda asta, meniul dispărea
    ''' la prima mișcare a cursorului, ceea ce îl făcea de nefolosit.</para>
    ''' </summary>
    Protected Overrides Sub OnDeactivate(e As EventArgs)
        MyBase.OnDeactivate(e)
        If IsCommittingSlider Then Return
        CloseWith(Nothing, -1)
    End Sub

    ' Momentul ultimei închideri. STATIC, și e în regulă: un CustomPopup se închide de îndată ce
    ' pierde activarea, deci nu pot exista niciodată două deschise simultan.
    Private Shared _lastClosedAt As DateTime = DateTime.MinValue

    ''' <summary>
    ''' «Tocmai s-a închis un meniu» — răspunsul la AL DOILEA clic pe butonul care l-a deschis.
    '''
    ''' Apăsarea aceea face două lucruri, în ordinea asta: activează fereastra de dedesubt (deci
    ''' meniul se închide singur, prin <c>Deactivate</c>) și abia apoi ajunge la buton. Un buton
    ''' care doar deschide ar redeschide meniul instantaneu, iar operatorul ar vedea un meniu care
    ''' refuză să se închidă. Gazda întreabă asta la începutul handler-ului și se retrage:
    ''' <code>If CustomPopup.ClosedJustNow Then Return</code>
    ''' Aceeași problemă și aceeași soluție ca la <c>ToolStripDropDown</c>; fereastra de 250 ms e
    ''' sub pragul dublului clic, deci nu poate înghiți o a doua deschidere intenționată.
    ''' </summary>
    Public Shared ReadOnly Property ClosedJustNow As Boolean
        Get
            Return (DateTime.UtcNow - _lastClosedAt).TotalMilliseconds < 250
        End Get
    End Property

    ''' <summary>
    ''' Ștampila se pune în DOUĂ locuri, fiindcă niciunul nu le acoperă pe amândouă:
    ''' <see cref="CloseWith"/> e drumul nostru (rând ales, Esc, clic în afară) și e singurul care
    ''' trece și pe un meniu care n-a ajuns niciodată pe ecran — <c>Form.Close</c> pe o fereastră
    ''' fără handle doar face <c>Dispose</c>, fără să ridice <c>FormClosed</c>; iar
    ''' <see cref="OnFormClosed"/> prinde închiderile care nu vin de la noi (Alt+F4, gazda care
    ''' închide fereastra). Ambele sunt idempotente — e o simplă ștampilă de timp.
    ''' </summary>
    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        _lastClosedAt = DateTime.UtcNow
        ' Sinkul prin care trec TOATE drumurile de închidere — aici se stinge butonul care ne-a
        ' desfășurat, o singură dată, oricum s-ar fi închis meniul.
        Dim ancora As IPopupAnchor = _anchor
        _anchor = Nothing
        ancora?.SetPopupOpen(False)
        MyBase.OnFormClosed(e)
    End Sub

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            ' TOOLWINDOW: fără buton în bara de activități și fără loc în Alt+Tab.
            ' NOACTIVATE lipsește INTENȚIONAT (vezi rezumatul clasei): fără activare n-ar exista
            ' focus de tastatură. CS_DROPSHADOW e umbra pe care o are orice meniu de sistem.
            cp.ExStyle = cp.ExStyle Or WS_EX_TOOLWINDOW
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW
            Return cp
        End Get
    End Property

    ' =====================================================================
    ' GEOMETRIE INTERNĂ
    ' =====================================================================

    ''' <summary>Cere o recalculare la următoarea folosire. Chemat de colecție la orice mutație.</summary>
    Friend Sub InvalidateLayout()
        _layoutDirty = True
        If _selectedIndex >= _items.Count Then _selectedIndex = -1
        Invalidate()
    End Sub

    ''' <summary>Mărimea de care ar avea nevoie meniul ca să încapă tot (înainte de strângerea la ecran).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property NaturalSize As Size
        Get
            EnsureLayout()
            Return _naturalSize
        End Get
    End Property

    ''' <summary>Dreptunghiul rândului în coordonate de CONȚINUT (fără derulare). Gol în afara colecției.</summary>
    Friend Function RowBounds(index As Integer) As Rectangle
        EnsureLayout()
        If index < 0 OrElse index >= _rows.Length Then Return Rectangle.Empty
        Return _rows(index)
    End Function

    ''' <summary>Rândul de sub un punct din CLIENT, sau -1. Ține cont de derulare.</summary>
    Friend Function HitTest(clientPoint As Point) As Integer
        EnsureLayout()
        Dim y As Integer = clientPoint.Y + _scroll
        For i As Integer = 0 To _rows.Length - 1
            If y >= _rows(i).Top AndAlso y < _rows(i).Bottom Then Return i
        Next
        Return -1
    End Function

    Friend ReadOnly Property ScrollOffset As Integer
        Get
            Return _scroll
        End Get
    End Property

    Friend ReadOnly Property MaxScroll As Integer
        Get
            EnsureLayout()
            Return Math.Max(0, _naturalSize.Height - ClientSize.Height)
        End Get
    End Property

    Friend Sub ScrollBy(delta As Integer)
        Dim clamped As Integer = Math.Max(0, Math.Min(MaxScroll, _scroll + delta))
        If clamped = _scroll Then Return
        _scroll = clamped
        Invalidate()
    End Sub

    ' Trage rândul înăuntru când meniul e mai înalt decât ecranul și navigarea iese din vizor.
    Private Sub EnsureVisible(index As Integer)
        If index < 0 OrElse index >= _rows.Length Then Return
        Dim h As Integer = ClientSize.Height
        If h <= 0 Then Return
        Dim r As Rectangle = _rows(index)
        If r.Top < _scroll Then
            _scroll = r.Top
        ElseIf r.Bottom > _scroll + h Then
            _scroll = r.Bottom - h
        Else
            Return
        End If
        _scroll = Math.Max(0, Math.Min(MaxScroll, _scroll))
        Invalidate()
    End Sub

    Private Sub EnsureLayout()
        If Not _layoutDirty Then Return
        RecalcLayout()
    End Sub

    ''' <summary>
    ''' Măsoară tot meniul: banda de pictograme (rezervată doar dacă are cine s-o umple — un meniu
    ''' fără nicio pictogramă n-are de ce să lase o coloană goală), lățimea celui mai lat text,
    ''' înălțimea fiecărui rând. Rezultatul e un dreptunghi per element, în coordonate de conținut.
    ''' </summary>
    Private Sub RecalcLayout()
        Try
            _layoutDirty = False

            Dim padX As Integer = ThemeShapes.ScaleDpi(Me, PadXLogical)
            Dim padY As Integer = ThemeShapes.ScaleDpi(Me, PadYLogical)
            Dim gutter As Integer = IconGutter()
            Dim rowH As Integer = EffectiveRowHeight()
            Dim sepH As Integer = ThemeShapes.ScaleDpi(Me, SeparatorLogical)

            Dim textW As Integer = 0
            For Each it As CustomPopupItem In _items
                If it.IsSeparator Then Continue For
                Dim sz As Size = TextRenderer.MeasureText(If(it.Text, String.Empty), Font,
                                                          New Size(Integer.MaxValue, Integer.MaxValue),
                                                          MeasureFlags())
                Dim latime As Integer = sz.Width
                ' Un rând-CURSOR are, pe lângă etichetă, o șină și o valoare (felia 0036-01). Fără
                ' cele trei adunate aici, meniul s-ar croi pe cel mai lat TEXT, iar șina ar primi
                ' ce rămâne — adică, într-un meniu cu etichete scurte, aproape nimic.
                If it.IsSlider Then
                    latime += ThemeShapes.ScaleDpi(Me, SliderGapLogical * 2 +
                                                       SliderValueWidthLogical +
                                                       SliderMinTrackLogical)
                End If
                If latime > textW Then textW = latime
            Next

            Dim w As Integer = BorderThickness * 2 + padX + gutter + textW + padX
            w = Math.Max(w, ThemeShapes.ScaleDpi(Me, _minimumPopupWidth))
            w = Math.Min(w, ThemeShapes.ScaleDpi(Me, _maximumPopupWidth))

            If _items.Count = 0 Then
                _rows = Array.Empty(Of Rectangle)()
            Else
                _rows = New Rectangle(_items.Count - 1) {}
            End If

            Dim y As Integer = BorderThickness + padY
            For i As Integer = 0 To _items.Count - 1
                Dim h As Integer = If(_items(i).IsSeparator, sepH, rowH)
                _rows(i) = New Rectangle(BorderThickness, y, w - BorderThickness * 2, h)
                y += h
            Next

            _naturalSize = New Size(w, y + padY + BorderThickness)
        Catch ex As Exception
            ' Frontieră de UI: geometria e chemată din pictură și din hit-test, unde un throw ar
            ' dărâma procesul. Un meniu gol e vizibil imediat, spre deosebire de un log tăcut.
            GlobalErrorLog.Write("CustomPopup.RecalcLayout", ex)
        End Try
    End Sub

    ''' <summary>Lățimea benzii de pictograme (0 = niciun element n-are pictogramă).</summary>
    Friend Function IconGutter() As Integer
        Dim side As Integer = ThemeShapes.ScaleDpi(Me, _imageSize)
        If side <= 0 Then Return 0
        For Each it As CustomPopupItem In _items
            If Not it.IsSeparator AndAlso it.Image IsNot Nothing Then
                Return side + ThemeShapes.ScaleDpi(Me, IconGapLogical)
            End If
        Next
        Return 0
    End Function

    ''' <summary>Înălțimea unui rând: cea cerută, altfel fontul + aer, dar niciodată sub pictogramă.</summary>
    Friend Function EffectiveRowHeight() As Integer
        If _itemHeight > 0 Then Return ThemeShapes.ScaleDpi(Me, _itemHeight)
        Return Math.Max(Font.Height + ThemeShapes.ScaleDpi(Me, RowAirLogical),
                        ThemeShapes.ScaleDpi(Me, _imageSize + 4))
    End Function

    ''' <summary>Se poate evidenția rândul ăsta? Separatorii și cei dezactivați nu.</summary>
    Friend Function IsSelectable(index As Integer) As Boolean
        If index < 0 OrElse index >= _items.Count Then Return False
        Dim it As CustomPopupItem = _items(index)
        Return Not it.IsSeparator AndAlso it.Enabled
    End Function

    ' Fontul se poate schimba (ShowCore îl ia de la gazdă) => rândurile se remăsoară.
    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        InvalidateLayout()
    End Sub

    ' Colțurile rotunjite se taie din FEREASTRĂ, nu doar din desen: altfel în colțuri s-ar vedea
    ' dreptunghiul ferestrei peste ce e dedesubt. Setter-ul Region eliberează regiunea veche.
    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        ApplyRegion()
    End Sub

    Private Sub ApplyRegion()
        Try
            Dim radius As Integer = EffectiveRadius()
            If radius <= 0 OrElse ClientSize.Width <= 0 OrElse ClientSize.Height <= 0 Then
                Region = Nothing
                Return
            End If
            Using path As GraphicsPath = ThemeShapes.RoundedRect(
                    New Rectangle(0, 0, ClientSize.Width, ClientSize.Height), radius)
                Region = New Region(path)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("CustomPopup.ApplyRegion", ex)
        End Try
    End Sub

    Friend Function EffectiveRadius() As Integer
        Dim logical As Integer = If(_cornerRadius >= 0, _cornerRadius, ThemeManager.Current.Style.CornerRadius)
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logical))
    End Function

    ''' <summary>
    ''' Steagurile de text, ACELEAȘI la măsurare și la desen — altfel lățimea calculată n-ar fi a
    ''' textului desenat. Desenul mai adaugă doar <c>EndEllipsis</c>, care taie exact ce oricum nu
    ''' încăpea în <see cref="MaximumPopupWidth"/>, deci nu schimbă nicio măsurătoare.
    '''
    ''' <c>HidePrefix</c> ascunde SUBLINIEREA, nu și marcajul («&amp;» tot nu se vede în niciun
    ''' caz), deci cele două variante măsoară identic — de aceea aprinderea sublinierilor la prima
    ''' tastă nu face meniul să sară în lățime.
    ''' </summary>
    Friend Function MeasureFlags() As TextFormatFlags
        Dim f As TextFormatFlags = TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                   TextFormatFlags.SingleLine Or TextFormatFlags.NoPadding
        If Not _showMnemonics Then f = f Or TextFormatFlags.HidePrefix
        Return f
    End Function

    ''' <summary>Sunt sublinierile de acces aprinse? Se aprind la prima tastă, ca la meniurile Windows.</summary>
    Friend ReadOnly Property MnemonicsVisible As Boolean
        Get
            Return _showMnemonics
        End Get
    End Property

    Friend Sub RevealMnemonics()
        If _showMnemonics Then Return
        _showMnemonics = True
        InvalidateLayout()
    End Sub

End Class
