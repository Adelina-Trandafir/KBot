Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
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

''' <summary>
''' Navigație owner-drawn — „tab-urile" simulate ale shell-ului (înlocuiește
''' TabControl-ul netematizabil). Un element = buton (cheie + text + badge opțional +
''' Enabled + Visible) SAU un separator (linie fină, neselectabilă). Elementele se pot
''' alinia la început sau la capătul barei (<see cref="KBotNavAlign"/>) și bara poate fi
''' verticală sau orizontală (<see cref="Orientation"/>). Selecția se schimbă cu click
''' sau Sus/Jos (Stânga/Dreapta în orizontal); re-selectarea aceleiași chei NU re-ridică
''' <see cref="SelectionChanged"/>. Toate culorile vin din schema activă (ApplyTheme).
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

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable, True)
        TabStop = True
        Width = 170
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
    ''' </summary>
    <DefaultValue(KBotNavOrientation.Vertical)>
    Public Property Orientation As KBotNavOrientation
        Get
            Return _orientation
        End Get
        Set(value As KBotNavOrientation)
            If value = _orientation Then Return
            _orientation = value
            InvalidateLayout()
        End Set
    End Property

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
        RaiseEvent SelectionChanged(it.Key)
    End Sub

    ' Marchează layout-ul „murdar" și cere repictare. FRIEND din 0025: colecția de elemente
    ' (KBotNavItemCollection) o cheamă la fiecare adăugare/ștergere/reordonare.
    Friend Sub InvalidateLayout()
        _layoutValid = False
        Invalidate()
    End Sub

    Private Function ItemThickness() As Integer
        Return ThemeShapes.ScaleDpi(Me, 36)
    End Function

    Private Function SeparatorExtent() As Integer
        Return ThemeShapes.ScaleDpi(Me, 11)
    End Function

    ' Extinderea (pe axa principală) a unui element vizibil.
    Private Function ItemExtent(it As KBotNavItem) As Integer
        If it.IsSeparator Then Return SeparatorExtent()
        If _orientation = KBotNavOrientation.Vertical Then Return ItemThickness()
        ' Orizontal: lățimea butonului = textul măsurat + padding (+ loc de badge).
        Dim padX As Integer = ThemeShapes.ScaleDpi(Me, 12)
        Dim ts As Size = TextRenderer.MeasureText(If(it.Text, String.Empty), Font)
        Dim w As Integer = ts.Width + 2 * padX
        If it.Badge > 0 Then w += ThemeShapes.ScaleDpi(Me, 26)
        Return Math.Max(w, ThemeShapes.ScaleDpi(Me, 48))
    End Function

    ' (Re)calculează slotul fiecărui element. Butoanele/separatorii ascunși primesc
    ' Rectangle.Empty. Grupul „Near" curge de la început, grupul „Far" de la capăt.
    Private Sub RecalcLayout()
        _layoutValid = True
        For Each it In _items
            it.Bounds = Rectangle.Empty
        Next

        Dim margin As Integer = ThemeShapes.ScaleDpi(Me, 6)
        Dim vertical As Boolean = (_orientation = KBotNavOrientation.Vertical)
        Dim mainLen As Integer = If(vertical, Height, Width)
        Dim crossLen As Integer = If(vertical, Width, Height)
        Dim crossStart As Integer = margin
        Dim crossSpan As Integer = Math.Max(0, crossLen - 2 * margin)

        ' Grupul Near: de la început spre capăt.
        Dim nearCursor As Integer = margin
        ' Grupul Far: se așază de la (capăt - extindereTotală) în ordinea listei.
        Dim farTotal As Integer = 0
        For Each it In _items
            If it.Visible AndAlso it.Align = KBotNavAlign.Far Then farTotal += ItemExtent(it)
        Next
        Dim farCursor As Integer = Math.Max(nearCursor, mainLen - margin - farTotal)

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
                it.Bounds = New Rectangle(crossStart, mainPos, crossSpan, ext)
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

    ''' <summary>Friend test hook: indexul elementului de sub un punct client (-1 = niciunul).</summary>
    Friend Function DebugIndexAt(location As Point) As Integer
        Return IndexAt(location)
    End Function

    ''' <summary>Friend test hook: trimite o tastă pe drumul real de navigare.</summary>
    Friend Sub DebugKeyDown(key As Keys)
        OnKeyDown(New KeyEventArgs(key))
    End Sub

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
        ' Accent „soft" pentru fundalul selecției: paleta nu are un slot AccentSoft —
        ' se derivă amestecând 14% accent în SurfaceAlt (nu adăugăm slot nou).
        _selectedFill = ThemeShapes.Blend(p.SurfaceAltColor, p.AccentColor, 0.14)
        _accent = p.AccentColor
        _hoverFill = p.ButtonHoverColor
        _textNormal = p.TextDimColor
        _textDisabled = p.DisabledTextColor
        _badgeFill = p.SurfaceColor
        _badgeText = p.TextDimColor
        _separatorColor = p.BorderColor
        BackColor = p.SurfaceColor
        Invalidate()
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        _semiboldFont?.Dispose()
        _semiboldFont = Nothing
        InvalidateLayout()
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
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

            Dim radius As Integer = ThemeShapes.ScaleDpi(Me, If(_scheme IsNot Nothing, _scheme.Style.CornerRadius, 0))
            Dim padX As Integer = ThemeShapes.ScaleDpi(Me, 12)

            ' Design time only: which keys are wrong. Validation is relaxed inside Visual Studio
            ' (throwing would kill the design surface), so the mistake has to be VISIBLE instead.
            Dim badKeys As HashSet(Of String) = If(designTime, DuplicateKeys(), Nothing)

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
                If isSelected OrElse isHover Then
                    Using path As GraphicsPath = ThemeShapes.RoundedRect(r, radius)
                        Using b As New SolidBrush(If(isSelected, _selectedFill, _hoverFill))
                            g.FillPath(b, path)
                        End Using
                    End Using
                End If

                ' Badge (pastilă rotunjită, aliniată dreapta) — desenat înaintea
                ' textului ca să-i putem rezerva lățimea.
                Dim textRight As Integer = r.Right - padX
                If it.Badge > 0 Then
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
                Dim tr As New Rectangle(r.Left + padX, r.Top, Math.Max(0, textRight - r.Left - padX), r.Height)
                Dim flags As TextFormatFlags = TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or
                    If(_orientation = KBotNavOrientation.Vertical, TextFormatFlags.Left, TextFormatFlags.HorizontalCenter)
                TextRenderer.DrawText(g, it.Text, textFont, tr, textColor, flags)

                ' Marcajul de eroare din designer: cheie vidă sau duplicată.
                If designTime AndAlso
                   (String.IsNullOrWhiteSpace(it.Key) OrElse badKeys.Contains(it.Key)) Then
                    Using pen As New Pen(Color.Red, 2)
                        g.DrawRectangle(pen, r.Left + 1, r.Top + 1, Math.Max(1, r.Width - 3), Math.Max(1, r.Height - 3))
                    End Using
                End If
            Next
        Catch ex As Exception
            ' Nu logăm din procesul designer-ului (vezi KBotDesignTime): fișierul de erori ar
            ' ajunge lângă devenv.exe și ar fi zgomot, nu diagnostic.
            If Not designTime Then GlobalErrorLog.Write("KBotNavList.OnPaint", ex)
        End Try
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
            Dim idx As Integer = IndexAt(e.Location)
            If idx >= 0 AndAlso Not _items(idx).Enabled Then idx = -1   ' fără hover pe disabled
            If idx <> _hoverIndex Then
                _hoverIndex = idx
                Invalidate()
            End If
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotNavList.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hoverIndex <> -1 Then
            _hoverIndex = -1
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Focus()
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
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
