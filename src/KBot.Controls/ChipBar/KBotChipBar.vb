Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Bară de jetoane («chips») owner-drawn: fratele MULTI-SELECT al lui <c>KBotNavList</c>. Un jeton
''' = cheie + text + pastilă de număr opțională + <c>Enabled</c> + <c>Visible</c> + <c>Checked</c>;
''' oricâte pot fi bifate deodată, iar bifarea e chiar rezultatul pe care îl citește apelantul
''' (<see cref="CheckedKeys"/>).
'''
''' Jetoanele curg de la stânga la dreapta și TREC PE RÂNDUL URMĂTOR când nu mai încap — o bară de
''' filtre stă într-o fâșie îngustă, iar la ferestre mici tăierea ultimelor jetoane ar ascunde
''' exact filtrul căutat. Înălțimea de care ar fi nevoie pentru toate rândurile se citește din
''' <see cref="PreferredBarHeight"/>; bara NU-și scrie singură <c>Height</c>-ul (gazda o andochează,
''' deci gazda îi deține dimensiunea — aceeași regulă ca <c>HostOwnsWidth</c> la arbore).
'''
''' Deciziile de desen, de tastatură și de design-time sunt COPIATE din <c>KBotNavList</c>, nu
''' reinventate: aceleași <c>SetStyle</c>, același <c>ThemeShapes.FillModern</c>, același chenar
''' roșu de 2 px pe cheile greșite în designer, aceeași suspendare a validării între
''' <c>BeginInit</c> și <c>EndInit</c>.
'''
''' Culorile vin toate din schema activă (<see cref="ApplyTheme"/>). Singura excepție e
''' <see cref="KBotChip.AccentOverride"/>, iar culoarea de acolo o dă APELANTUL, tot din paletă —
''' bara nu numește nicio culoare.
''' </summary>
<ToolboxItem(True)>
<DefaultProperty("Chips")>
<DefaultEvent("CheckedChanged")>
Public NotInheritable Class KBotChipBar
    Inherits Control
    Implements IThemedControl
    Implements ISupportInitialize

    Private ReadOnly _chips As New KBotChipCollection()
    Private _hoverIndex As Integer = -1
    Private _focusIndex As Integer = -1
    Private _layoutValid As Boolean
    Private _rowCount As Integer

    ' Măsuri în px logici, scalate la DPI la fiecare așezare (ThemeShapes.ScaleDpi).
    Private _chipHeight As Integer = 24
    Private _chipPadding As Integer = 10
    Private _chipSpacing As Integer = 6
    Private _chipCornerRadius As Integer = -1
    Private _chipGradient As Integer = 14
    Private _minimumRequiredChecked As Integer = 0

    ' Refuzul de a stinge ultimul jeton bifat nu aruncă: e un gest de mouse, nu un apel de API.
    ' Se vede printr-o clipire scurtă pe jetonul refuzat — altfel operatorul ar crede că bara e moartă.
    Private _flashIndex As Integer = -1
    Private _flashTimer As System.Windows.Forms.Timer

    ' ── Inițializare din designer (ISupportInitialize) ────────────────────────
    ' Între BeginInit și EndInit bara acceptă ce scrie InitializeComponent FĂRĂ să valideze:
    ' designer-ul emite proprietățile în ordinea lui, iar o cheie pe jumătate tastată ar arunca
    ' din InitializeComponent, adică formularul nu s-ar mai deschide deloc.
    Private _initializing As Boolean

    ' ── Culori derivate din paletă (setate în ApplyTheme) ─────────────────────
    Private _scheme As ThemeScheme
    Private _chipBack As Color = SystemColors.Control
    Private _chipText As Color = SystemColors.ControlText
    Private _chipBorder As Color = SystemColors.ControlDark
    Private _chipHover As Color = SystemColors.ControlLight
    Private _accent As Color = SystemColors.Highlight
    Private _accentText As Color = SystemColors.HighlightText
    Private _textDisabled As Color = SystemColors.GrayText
    Private _badgeFill As Color = SystemColors.Control
    Private _badgeText As Color = SystemColors.GrayText

    ' Creionul/pensula care NU depind de starea jetonului se fac o dată, în ApplyTheme, și se
    ' eliberează în Dispose — ca la KBotDataView.RebuildThemeResources. Restul (umplerea jetonului,
    ' textul) se schimbă de la un jeton la altul: umplerea trece oricum prin ThemeShapes.FillModern,
    ' care ia o CULOARE, iar TextRenderer desenează tot cu o culoare, nu cu o pensulă — deci acolo
    ' n-ar avea ce să se memoreze.
    Private _borderPen As Pen
    Private _badgeBrush As SolidBrush

    ''' <summary>Ridicat când starea de bifare a unui jeton SE SCHIMBĂ (mouse, tastatură sau API).</summary>
    Public Event CheckedChanged(chipKey As String)

    ''' <summary>Ridicat la orice apăsare pe un jeton activ, chiar dacă bifarea a fost refuzată.</summary>
    Public Event ChipClicked(chipKey As String)

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable, True)
        TabStop = True
        Height = 32
        _chips.Owner = Me
    End Sub

    ' =====================================================================
    ' API PUBLIC
    ' =====================================================================

    ''' <summary>
    ''' Jetoanele, în ordinea de afișare. Editabile din grila de proprietăți (dialogul standard de
    ''' colecție) sau din cod, prin <see cref="AddChip"/> — aceeași colecție.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Jetoanele barei, în ordinea de afișare.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Chips As KBotChipCollection
        Get
            Return _chips
        End Get
    End Property

    ''' <summary>Înălțimea unui jeton, px logici (scalați la DPI). Implicit 24.</summary>
    <Category("K-BOT")>
    <Description("Înălțimea unui jeton (px logici). Implicit 24.")>
    <DefaultValue(24)>
    Public Property ChipHeight As Integer
        Get
            Return _chipHeight
        End Get
        Set(value As Integer)
            ' Ca la KBotNavList: o măsură negativă se aduce la limită, nu aruncă — un setter de
            ' dimensiune care aruncă ar rupe InitializeComponent la o valoare greșită din designer.
            Dim clamped As Integer = Math.Max(1, value)
            If clamped = _chipHeight Then Return
            _chipHeight = clamped
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>Aerul stânga/dreapta din interiorul unui jeton, px logici. Implicit 10.</summary>
    <Category("K-BOT")>
    <Description("Aerul (px logici) dintre marginea jetonului și textul lui. Implicit 10.")>
    <DefaultValue(10)>
    Public Property ChipPadding As Integer
        Get
            Return _chipPadding
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _chipPadding Then Return
            _chipPadding = clamped
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>Distanța dintre jetoane (pe orizontală ȘI între rânduri), px logici. Implicit 6.</summary>
    <Category("K-BOT")>
    <Description("Distanța (px logici) dintre jetoane și dintre rânduri. Implicit 6.")>
    <DefaultValue(6)>
    Public Property ChipSpacing As Integer
        Get
            Return _chipSpacing
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If clamped = _chipSpacing Then Return
            _chipSpacing = clamped
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>
    ''' Raza colțurilor jetoanelor, px logici. <b>-1 (implicit) = raza schemei active</b>, singura
    ''' valoare care urmează tema; 0 = colțuri drepte. Un jeton e prin tradiție o pastilă, deci
    ''' pentru forma aceea se dă o rază mare (ex. jumătate din <see cref="ChipHeight"/>).
    ''' </summary>
    <Category("K-BOT")>
    <Description("Raza colțurilor jetoanelor (px logici). -1 = raza schemei active; 0 = colțuri drepte.")>
    <DefaultValue(-1)>
    Public Property ChipCornerRadius As Integer
        Get
            Return _chipCornerRadius
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(-1, value)
            If clamped = _chipCornerRadius Then Return
            _chipCornerRadius = clamped
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Intensitatea gradientului de pe fundalul jetoanelor (0..100; 0 = umplere plată), exact ca
    ''' <c>KBotNavList.ItemGradient</c> și prin aceeași <c>ThemeShapes.FillModern</c>: nu introduce
    ''' nicio culoare nouă.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Intensitatea gradientului de pe jetoane (0..100). Implicit 14; 0 = umplere plată.")>
    <DefaultValue(14)>
    Public Property ChipGradient As Integer
        Get
            Return _chipGradient
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, Math.Min(100, value))
            If clamped = _chipGradient Then Return
            _chipGradient = clamped
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' Câte jetoane trebuie să rămână bifate. Implicit 0 = fără constrângere.
    '''
    ''' La 1, stingerea ULTIMULUI jeton bifat cu mouse-ul sau cu tastatura se refuză: jetonul
    ''' clipește scurt și rămâne bifat, fără excepție și fără eveniment. E un gest de operator, nu
    ''' un apel de API — de aceea <see cref="SetChecked"/> NU e oprit de prag (codul care cere
    ''' explicit o stare o primește; vezi acolo).
    ''' </summary>
    <Category("K-BOT")>
    <Description("Câte jetoane trebuie să rămână bifate. 0 = fără constrângere; 1 = ultimul bifat nu se poate stinge cu mouse-ul.")>
    <DefaultValue(0)>
    Public Property MinimumRequiredChecked As Integer
        Get
            Return _minimumRequiredChecked
        End Get
        Set(value As Integer)
            _minimumRequiredChecked = Math.Max(0, value)
        End Set
    End Property

    ''' <summary>
    ''' Înălțimea de care ar avea nevoie bara ca să încapă toate rândurile de jetoane la lățimea
    ''' curentă. Gazda o citește ca să-și dimensioneze fâșia — bara nu-și scrie singură
    ''' <c>Height</c>-ul.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property PreferredBarHeight As Integer
        Get
            EnsureLayout()
            If _rowCount <= 0 Then Return ThemeShapes.ScaleDpi(Me, _chipHeight)
            Dim h As Integer = ThemeShapes.ScaleDpi(Me, _chipHeight)
            Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _chipSpacing)
            Return _rowCount * h + (_rowCount - 1) * gap
        End Get
    End Property

    ''' <summary>Cheile jetoanelor bifate, în ordinea din bară. Jetoanele ascunse NU se raportează.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property CheckedKeys As IReadOnlyList(Of String)
        Get
            Dim result As New List(Of String)()
            For Each c As KBotChip In _chips
                If c.Visible AndAlso c.Checked Then result.Add(c.Key)
            Next
            Return result
        End Get
    End Property

    ''' <summary>Adaugă un jeton nebifat. Cheia trebuie să fie nevidă și unică.</summary>
    Public Sub AddChip(key As String, text As String)
        AddChip(key, text, False)
    End Sub

    ''' <summary>Adaugă un jeton cu starea de bifare dată. Cheia trebuie să fie nevidă și unică.</summary>
    Public Sub AddChip(key As String, text As String, checked As Boolean)
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Cheie vidă.", NameOf(key))
        If FindIndex(key) >= 0 Then Throw New ArgumentException($"Cheie duplicată: '{key}'.", NameOf(key))
        ' Colecția invalidează singură așezarea.
        _chips.Add(New KBotChip(key, text, checked))
    End Sub

    ''' <summary>
    ''' Bifează/debifează din cod. Cheie necunoscută => <c>ArgumentException</c>; setarea valorii
    ''' deja ținute NU ridică <see cref="CheckedChanged"/>.
    '''
    ''' <see cref="MinimumRequiredChecked"/> nu se aplică aici, dinadins: pragul apără gestul
    ''' operatorului de un click care golește grila, nu codul de propria lui intenție (ex.
    ''' <c>UncheckAll</c> urmat de bifarea unui set nou).
    ''' </summary>
    Public Sub SetChecked(key As String, checked As Boolean)
        Dim idx As Integer = RequireIndex(key)
        If _chips(idx).Checked = checked Then Return
        _chips(idx).Checked = checked
        Invalidate()
        RaiseEvent CheckedChanged(_chips(idx).Key)
    End Sub

    ''' <summary>
    ''' Varianta care NU aruncă — pentru cine trebuie doar să ÎNTREBE dacă cheia există (o gazdă
    ''' care își construiește jetoanele dintr-o listă și le citește tot din ea).
    ''' </summary>
    Public Function ContainsChip(key As String) As Boolean
        Return FindIndex(key) >= 0
    End Function

    ''' <summary>Starea de bifare a unui jeton. Cheie necunoscută => excepție.</summary>
    Public Function IsChecked(key As String) As Boolean
        Return _chips(RequireIndex(key)).Checked
    End Function

    ''' <summary>Activează/dezactivează un jeton. Cheie necunoscută => excepție.</summary>
    Public Sub SetChipEnabled(key As String, enabled As Boolean)
        _chips(RequireIndex(key)).Enabled = enabled
        Invalidate()
    End Sub

    ''' <summary>Arată/ascunde un jeton (ascuns = fără slot, fără pictare, sărit de tastatură).</summary>
    Public Sub SetChipVisible(key As String, visible As Boolean)
        _chips(RequireIndex(key)).Visible = visible
        InvalidateLayout()
    End Sub

    ''' <summary>Setează numărul din pastilă (0 = pastila dispare). Cheie necunoscută => excepție.</summary>
    Public Sub SetBadge(key As String, count As Integer)
        _chips(RequireIndex(key)).Count = count
        ' Pastila schimbă LĂȚIMEA jetonului, deci e o reașezare, nu doar o repictare.
        InvalidateLayout()
    End Sub

    ''' <summary>Bifează toate jetoanele vizibile și active. Ridică evenimentul o dată per schimbare reală.</summary>
    Public Sub CheckAll()
        SetAll(True)
    End Sub

    ''' <summary>
    ''' Debifează toate jetoanele vizibile și active. Ca <see cref="SetChecked"/>, nu se uită la
    ''' <see cref="MinimumRequiredChecked"/>: e un apel de API, nu un gest.
    ''' </summary>
    Public Sub UncheckAll()
        SetAll(False)
    End Sub

    Private Sub SetAll(checked As Boolean)
        Dim changed As New List(Of String)()
        For Each c As KBotChip In _chips
            If Not c.Visible OrElse Not c.Enabled Then Continue For
            If c.Checked = checked Then Continue For
            c.Checked = checked
            changed.Add(c.Key)
        Next
        If changed.Count = 0 Then Return
        Invalidate()
        For Each k As String In changed
            RaiseEvent CheckedChanged(k)
        Next
    End Sub

    ' =====================================================================
    ' ISupportInitialize
    ' =====================================================================

    ''' <summary>Începutul blocului de inițializare emis de designer (validările se suspendă).</summary>
    Public Sub BeginInit() Implements ISupportInitialize.BeginInit
        _initializing = True
    End Sub

    ''' <summary>
    ''' Sfârșitul blocului de inițializare: se validează jetoanele (cheie nevidă și unică) și se
    ''' reașază bara.
    '''
    ''' În DESIGNER validarea se sare — o cheie pe jumătate tastată ar arunca din
    ''' <c>InitializeComponent</c>, adică formularul nu s-ar mai deschide deloc. Defectul se
    ''' semnalează vizual, cu chenar roșu (vezi <see cref="OnPaint"/>), exact ca la
    ''' <c>KBotNavList</c>.
    ''' </summary>
    Public Sub EndInit() Implements ISupportInitialize.EndInit
        Try
            _initializing = False
            If Not KBotDesignTime.IsDesignTime(Me) Then ValidateChips()
            InvalidateLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChipBar.EndInit", ex)
            Throw
        End Try
    End Sub

    Private Sub ValidateChips()
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        For i As Integer = 0 To _chips.Count - 1
            Dim c As KBotChip = _chips(i)
            If String.IsNullOrWhiteSpace(c.Key) Then
                Throw New ArgumentException($"Cheie vidă la jetonul {i} («{If(c.Text, String.Empty)}»).", NameOf(Chips))
            End If
            If Not seen.Add(c.Key) Then
                Throw New ArgumentException($"Cheie duplicată: '{c.Key}' (jetonul {i}).", NameOf(Chips))
            End If
        Next
    End Sub

    ' =====================================================================
    ' INTERNE
    ' =====================================================================

    Private Function FindIndex(key As String) As Integer
        If String.IsNullOrEmpty(key) Then Return -1
        For i As Integer = 0 To _chips.Count - 1
            If String.Equals(_chips(i).Key, key, StringComparison.Ordinal) Then Return i
        Next
        Return -1
    End Function

    ' Indexul cheii sau ArgumentException — fără no-op-uri tăcute (regula casei).
    Private Function RequireIndex(key As String) As Integer
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Cheie vidă.", NameOf(key))
        Dim idx As Integer = FindIndex(key)
        If idx < 0 Then Throw New ArgumentException($"Cheie necunoscută: '{key}'.", NameOf(key))
        Return idx
    End Function

    ''' <summary>Cere o reașezare la următoarea folosire. Chemată de colecție la orice mutație.</summary>
    Friend Sub InvalidateLayout()
        _layoutValid = False
        Invalidate()
    End Sub

    Private Sub EnsureLayout()
        If Not _layoutValid Then RecalcLayout()
    End Sub

    Private Function ChipRadius() As Integer
        Dim logical As Integer = If(_chipCornerRadius >= 0, _chipCornerRadius,
                                    If(_scheme IsNot Nothing, _scheme.Style.CornerRadius, 0))
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logical))
    End Function

    ''' <summary>
    ''' Lățimea cerută de conținutul unui jeton: aer + text + pastilă. O SINGURĂ funcție, folosită
    ''' și la așezare și la pictare, ca cele două să nu se poată despărți.
    ''' </summary>
    Private Function ChipWidth(c As KBotChip) As Integer
        Dim padX As Integer = ThemeShapes.ScaleDpi(Me, _chipPadding)
        Dim ts As Size = TextRenderer.MeasureText(If(c.Text, String.Empty), Font)
        Dim w As Integer = ts.Width + 2 * padX
        If c.Count > 0 Then w += BadgeWidth(c)
        Return w
    End Function

    ' Lățimea pastilei + aerul dinaintea ei (0 dacă jetonul n-are număr).
    Private Function BadgeWidth(c As KBotChip) As Integer
        If c.Count <= 0 Then Return 0
        Dim ts As Size = TextRenderer.MeasureText(c.Count.ToString(), Font)
        Dim bh As Integer = BadgeHeight()
        Return Math.Max(bh, ts.Width + ThemeShapes.ScaleDpi(Me, 8)) + ThemeShapes.ScaleDpi(Me, 6)
    End Function

    Private Function BadgeHeight() As Integer
        Return ThemeShapes.ScaleDpi(Me, Math.Max(10, _chipHeight - 8))
    End Function

    ''' <summary>
    ''' (Re)calculează slotul fiecărui jeton: curgere de la stânga la dreapta, cu trecere pe rândul
    ''' următor când jetonul nu mai încape. Un jeton mai lat decât bara întreagă primește totuși un
    ''' rând al lui, tăiat la lățimea barei — textul se taie cu «…», dar jetonul rămâne apăsabil.
    ''' Jetoanele ascunse primesc <see cref="Rectangle.Empty"/>.
    ''' </summary>
    Private Sub RecalcLayout()
        _layoutValid = True
        For Each c As KBotChip In _chips
            c.Bounds = Rectangle.Empty
        Next

        Dim h As Integer = ThemeShapes.ScaleDpi(Me, _chipHeight)
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _chipSpacing)
        Dim available As Integer = Math.Max(1, Width)

        Dim x As Integer = 0
        Dim y As Integer = 0
        Dim rows As Integer = 0

        For Each c As KBotChip In _chips
            If Not c.Visible Then Continue For
            Dim w As Integer = Math.Min(ChipWidth(c), available)
            If x > 0 AndAlso x + w > available Then
                ' Nu mai încape pe rândul curent => rând nou.
                x = 0
                y += h + gap
            End If
            c.Bounds = New Rectangle(x, y, w, h)
            rows = Math.Max(rows, (y \ Math.Max(1, h + gap)) + 1)
            x += w + gap
        Next

        _rowCount = rows
    End Sub

    Private Function IndexAt(location As Point) As Integer
        EnsureLayout()
        For i As Integer = 0 To _chips.Count - 1
            Dim c As KBotChip = _chips(i)
            If c.Visible AndAlso c.Bounds.Contains(location) Then Return i
        Next
        Return -1
    End Function

    ' Câte jetoane VIZIBILE sunt bifate acum — baza pragului MinimumRequiredChecked.
    Private Function CheckedCount() As Integer
        Dim n As Integer = 0
        For Each c As KBotChip In _chips
            If c.Visible AndAlso c.Checked Then n += 1
        Next
        Return n
    End Function

    ''' <summary>
    ''' Drumul COMUN al mouse-ului și al tastaturii: comută jetonul, dacă pragul îi dă voie.
    ''' Refuzul nu aruncă și nu ridică nimic — clipește.
    ''' </summary>
    Private Sub ToggleByGesture(index As Integer)
        If index < 0 OrElse index >= _chips.Count Then Return
        Dim c As KBotChip = _chips(index)
        If Not c.Visible OrElse Not c.Enabled Then Return

        RaiseEvent ChipClicked(c.Key)

        If c.Checked AndAlso CheckedCount() <= _minimumRequiredChecked Then
            FlashChip(index)
            Return
        End If

        c.Checked = Not c.Checked
        Invalidate()
        RaiseEvent CheckedChanged(c.Key)
    End Sub

    ' Clipirea de refuz: 140 ms de chenar pe accent, apoi înapoi. Un cronometru, nu o animație —
    ' e un «nu», nu un efect.
    Private Sub FlashChip(index As Integer)
        _flashIndex = index
        If _flashTimer Is Nothing Then
            _flashTimer = New System.Windows.Forms.Timer()
            _flashTimer.Interval = 140
            AddHandler _flashTimer.Tick, AddressOf FlashTick
        End If
        _flashTimer.Stop()
        _flashTimer.Start()
        Invalidate()
    End Sub

    Private Sub FlashTick(sender As Object, e As EventArgs)
        Try
            _flashTimer?.Stop()
            _flashIndex = -1
            Invalidate()
        Catch ex As Exception
            ' Frontieră de UI (tic de cronometru): loghează și înghite — nu poate rearunca.
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotChipBar.FlashTick", ex)
        End Try
    End Sub

    ' =====================================================================
    ' TEMĂ
    ' =====================================================================

    ''' <summary>Reaplică culorile schemei și reface pensulele memorate.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            _scheme = scheme
            Dim p As ThemePalette = scheme.Palette

            _chipBack = p.ButtonBackColor
            _chipText = p.ButtonTextColor
            _chipBorder = p.ButtonBorderColor
            _chipHover = p.ButtonHoverColor
            _accent = p.AccentColor
            _accentText = p.AccentTextColor
            _textDisabled = p.DisabledTextColor
            _badgeFill = p.SurfaceColor
            _badgeText = p.TextDimColor
            BackColor = p.SurfaceColor

            RebuildThemeResources()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChipBar.ApplyTheme", ex)
        End Try
    End Sub

    Private Sub RebuildThemeResources()
        _borderPen?.Dispose()
        _badgeBrush?.Dispose()
        _borderPen = New Pen(_chipBorder)
        _badgeBrush = New SolidBrush(_badgeFill)
    End Sub

    ' =====================================================================
    ' PICTARE
    ' =====================================================================

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim designTime As Boolean = KBotDesignTime.IsDesignTime(Me)
        Try
            EnsureLayout()
            Dim g As Graphics = e.Graphics
            g.Clear(BackColor)
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim radius As Integer = ChipRadius()
            Dim padX As Integer = ThemeShapes.ScaleDpi(Me, _chipPadding)
            Dim badKeys As HashSet(Of String) = If(designTime, DuplicateKeys(), Nothing)

            For i As Integer = 0 To _chips.Count - 1
                Dim c As KBotChip = _chips(i)
                If Not c.Visible Then Continue For
                Dim r As Rectangle = c.Bounds
                If r.Width <= 0 OrElse r.Height <= 0 Then Continue For

                Dim isHover As Boolean = (i = _hoverIndex) AndAlso c.Enabled
                Dim fill As Color
                Dim fore As Color
                If c.Checked Then
                    ' Culoarea jetonului bifat: cea dată de apelant, altfel accentul schemei.
                    fill = If(c.AccentOverride = Color.Empty, _accent, c.AccentOverride)
                    fore = _accentText
                ElseIf isHover Then
                    fill = _chipHover
                    fore = _chipText
                Else
                    fill = _chipBack
                    fore = _chipText
                End If
                If Not c.Enabled Then fore = _textDisabled

                Using path As GraphicsPath = ThemeShapes.RoundedRect(r, radius)
                    ThemeShapes.FillModern(g, path, r, fill, _chipGradient)
                    ' Jetonul bifat n-are contur propriu: conturul lui E umplerea, altfel un
                    ' chenar gri ar tăia pastila colorată de jur împrejur.
                    If c.Checked Then
                        Using pen As New Pen(fill)
                            g.DrawPath(pen, path)
                        End Using
                    ElseIf _borderPen IsNot Nothing Then
                        g.DrawPath(_borderPen, path)
                    End If
                End Using

                ' Pastila numărului, desenată înaintea textului ca să-i putem rezerva lățimea.
                Dim textRight As Integer = r.Right - padX
                If c.Count > 0 Then
                    Dim badgeStr As String = c.Count.ToString()
                    Dim bh As Integer = Math.Min(BadgeHeight(), r.Height - ThemeShapes.ScaleDpi(Me, 4))
                    Dim ts As Size = TextRenderer.MeasureText(g, badgeStr, Font)
                    Dim bw As Integer = Math.Max(bh, ts.Width + ThemeShapes.ScaleDpi(Me, 8))
                    Dim br As New Rectangle(r.Right - bw - ThemeShapes.ScaleDpi(Me, 6),
                                            r.Top + (r.Height - bh) \ 2, bw, bh)
                    Using bpath As GraphicsPath = ThemeShapes.RoundedRect(br, bh \ 2)
                        ' Pe un jeton bifat pastila stă pe accent: fundalul ei trebuie să fie
                        ' culoarea textului de accent, altfel dispare în el.
                        If c.Checked Then
                            Using b As New SolidBrush(_accentText)
                                g.FillPath(b, bpath)
                            End Using
                        ElseIf _badgeBrush IsNot Nothing Then
                            g.FillPath(_badgeBrush, bpath)
                        End If
                    End Using
                    TextRenderer.DrawText(g, badgeStr, Font, br,
                                          If(c.Checked, fill, _badgeText),
                                          TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                    textRight = br.Left - ThemeShapes.ScaleDpi(Me, 2)
                End If

                Dim tr As New Rectangle(r.Left + padX, r.Top,
                                        Math.Max(0, textRight - (r.Left + padX)), r.Height)
                TextRenderer.DrawText(g, If(c.Text, String.Empty), Font, tr, fore,
                                      TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                      TextFormatFlags.EndEllipsis)

                ' Inelul de focus al tastaturii: bara e un singur stop de Tab, deci trebuie să se
                ' vadă PE CARE jeton ar acționa Space.
                If Focused AndAlso i = _focusIndex Then
                    Using pen As New Pen(If(_scheme IsNot Nothing, _scheme.Palette.FocusRingColor, _accent))
                        pen.DashStyle = DashStyle.Dot
                        Using fpath As GraphicsPath = ThemeShapes.RoundedRect(
                                New Rectangle(r.Left + 2, r.Top + 2, Math.Max(1, r.Width - 5), Math.Max(1, r.Height - 5)),
                                Math.Max(0, radius - 1))
                            g.DrawPath(pen, fpath)
                        End Using
                    End Using
                End If

                ' Clipirea de refuz (vezi MinimumRequiredChecked).
                If i = _flashIndex Then
                    Using pen As New Pen(_accent, 2)
                        Using fpath As GraphicsPath = ThemeShapes.RoundedRect(r, radius)
                            g.DrawPath(pen, fpath)
                        End Using
                    End Using
                End If

                ' Marcajul de eroare din designer: cheie vidă sau duplicată.
                If designTime AndAlso
                   (String.IsNullOrWhiteSpace(c.Key) OrElse badKeys.Contains(c.Key)) Then
                    Using pen As New Pen(Color.Red, 2)
                        g.DrawRectangle(pen, r.Left + 1, r.Top + 1, Math.Max(1, r.Width - 3), Math.Max(1, r.Height - 3))
                    End Using
                End If
            Next
        Catch ex As Exception
            ' Nu logăm din procesul designer-ului (vezi KBotDesignTime).
            If Not designTime Then GlobalErrorLog.Write("KBotChipBar.OnPaint", ex)
        End Try
    End Sub

    ' Cheile care apar de mai multe ori. Doar design-time.
    Private Function DuplicateKeys() As HashSet(Of String)
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        Dim dup As New HashSet(Of String)(StringComparer.Ordinal)
        For Each c As KBotChip In _chips
            If String.IsNullOrWhiteSpace(c.Key) Then Continue For
            If Not seen.Add(c.Key) Then dup.Add(c.Key)
        Next
        Return dup
    End Function

    ' =====================================================================
    ' MOUSE
    ' =====================================================================

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            Dim idx As Integer = IndexAt(e.Location)
            If idx >= 0 AndAlso Not _chips(idx).Enabled Then idx = -1
            If idx <> _hoverIndex Then
                _hoverIndex = idx
                Invalidate()
            End If
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotChipBar.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hoverIndex <> -1 Then
            _hoverIndex = -1
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Focus()
            Dim idx As Integer = IndexAt(e.Location)
            If idx < 0 Then Return
            ' În designer un click NU comută: ar murdări formularul cuiva cu o stare pe care n-a
            ' ales-o (aceeași regulă ca butonul de strângere al lui KBotNavList).
            If KBotDesignTime.IsDesignTime(Me) Then Return
            _focusIndex = idx
            ToggleByGesture(idx)
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotChipBar.OnMouseDown", ex)
        End Try
    End Sub

    ' =====================================================================
    ' TASTATURĂ
    ' =====================================================================

    ' Fără asta formularul mănâncă săgețile și spațiul înainte să ajungă la bară.
    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        If keyData = Keys.Left OrElse keyData = Keys.Right OrElse keyData = Keys.Space Then Return True
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            Select Case e.KeyCode
                Case Keys.Left
                    MoveFocus(-1)
                    e.Handled = True
                Case Keys.Right
                    MoveFocus(1)
                    e.Handled = True
                Case Keys.Space
                    ' Spațiul e drumul tastaturii spre EXACT ce face clicul, prag inclusiv.
                    If _focusIndex < 0 Then MoveFocus(1)
                    ToggleByGesture(_focusIndex)
                    e.Handled = True
            End Select
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotChipBar.OnKeyDown", ex)
        End Try
    End Sub

    ' Următorul jeton VIZIBIL și ACTIV în direcția dată, fără wrap (ca la KBotNavList).
    Private Sub MoveFocus(direction As Integer)
        If _chips.Count = 0 Then Return
        Dim idx As Integer = _focusIndex + direction
        If _focusIndex < 0 Then idx = If(direction > 0, 0, _chips.Count - 1)
        While idx >= 0 AndAlso idx < _chips.Count
            Dim c As KBotChip = _chips(idx)
            If c.Visible AndAlso c.Enabled Then
                _focusIndex = idx
                Invalidate()
                Return
            End If
            idx += direction
        End While
    End Sub

    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        ' Primul Tab pe bară trebuie să aprindă un jeton, altfel Space n-ar avea pe ce lucra.
        If _focusIndex < 0 Then MoveFocus(1)
        Invalidate()
    End Sub

    Protected Overrides Sub OnLostFocus(e As EventArgs)
        MyBase.OnLostFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        InvalidateLayout()
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        ' Lățimea decide unde se rupe rândul, deci orice redimensionare e o reașezare.
        InvalidateLayout()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _flashTimer?.Stop()
            _flashTimer?.Dispose()
            _flashTimer = Nothing
            _borderPen?.Dispose()
            _borderPen = Nothing
            _badgeBrush?.Dispose()
            _badgeBrush = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

    ' =====================================================================
    ' CÂRLIGE FRIEND PENTRU TESTE (headless, fără ecran)
    ' =====================================================================

    ''' <summary>Friend test hook: forțează reașezarea, fără pictare.</summary>
    Friend Sub DebugEnsureLayout()
        EnsureLayout()
    End Sub

    ''' <summary>Friend test hook: slotul calculat al unui jeton (Empty dacă e ascuns).</summary>
    Friend Function DebugBounds(index As Integer) As Rectangle
        EnsureLayout()
        Return _chips(index).Bounds
    End Function

    ''' <summary>Friend test hook: câte rânduri a ocupat ultima așezare.</summary>
    Friend Function DebugRowCount() As Integer
        EnsureLayout()
        Return _rowCount
    End Function

    ''' <summary>Friend test hook: indexul jetonului de sub un punct client (-1 = niciunul).</summary>
    Friend Function DebugIndexAt(location As Point) As Integer
        Return IndexAt(location)
    End Function

    ''' <summary>Friend test hook: click stânga pe drumul real (inclusiv pragul de bifare).</summary>
    Friend Sub DebugClickAt(location As Point)
        OnMouseDown(New MouseEventArgs(MouseButtons.Left, 1, location.X, location.Y, 0))
    End Sub

    ''' <summary>Friend test hook: trimite o tastă pe drumul real de navigare.</summary>
    Friend Sub DebugKeyDown(key As Keys)
        OnKeyDown(New KeyEventArgs(key))
    End Sub

    ''' <summary>Friend test hook: jetonul pe care ar acționa Space (-1 = niciunul).</summary>
    Friend Function DebugFocusIndex() As Integer
        Return _focusIndex
    End Function

    ''' <summary>Friend test hook: jetonul care clipește acum (-1 = niciunul).</summary>
    Friend Function DebugFlashIndex() As Integer
        Return _flashIndex
    End Function

End Class
