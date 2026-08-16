Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Partea de GEOMETRIE a <see cref="KBotDataView"/>: benzile (antet / coloane înghețate /
''' corp derulat), offset-urile X ale coloanelor, matematica de virtualizare (înălțime fixă
''' de rând => aritmetică întreagă) și barele de derulare.
'''
''' Regula de aur a virtualizării: se pictează DOAR rândurile vizibile, deci costul unei
''' pictări nu depinde de <c>RowCount</c> (5.000 sau 500.000 — la fel).
''' </summary>
Partial Class KBotDataView

    ' Offset-ul X al unei coloane în banda ei (înghețată sau derulată).
    Private Structure ColLayout
        Public Column As KBotDataColumn
        Public X As Integer
    End Structure

    Private ReadOnly _frozenLayout As New List(Of ColLayout)()
    Private ReadOnly _scrollLayout As New List(Of ColLayout)()
    Private _frozenBandWidth As Integer = 0
    Private _scrollBandWidth As Integer = 0

    ' Gardă de reintrare: schimbarea vizibilității barelor declanșează layout.
    Private _inLayout As Boolean = False

    ' Derulare orizontală pe coloane (ScrollByColumn): starea de aliniere la margini.
    Private _scrollByColumn As Boolean = False
    Private _lastHScrollValue As Integer = 0
    Private _snappingHScroll As Boolean = False
    ' Cât timp operatorul TRAGE thumb-ul: derulare liberă, aliniere abia la eliberare.
    Private _hThumbTracking As Boolean = False

    ' ── Recalcul coloane ────────────────────────────────────────────────────────

    ''' <summary>
    ''' Reconstruiește offset-urile X pe cele două benzi. Pur (nu atinge controale), deci
    ''' e sigur de apelat și din pictare.
    ''' </summary>
    Private Sub RecalcColumnLayout()
        ' Lățimile tocmai s-au putut schimba, iar înălțimea benzii de antet e o funcție de ele:
        ' se uită măsurătoarea veche AICI, nu la fiecare interogare, ca o trecere să vadă un
        ' singur număr de la cap la coadă. Recalculul propriu-zis e leneș (EffectiveHeaderHeight).
        InvalidateHeaderHeight()
        _frozenLayout.Clear()
        _scrollLayout.Clear()
        Dim frozenX As Integer = 0
        Dim scrollX As Integer = 0
        Dim visibleIndex As Integer = 0

        For Each c In _columns
            If Not c.IsEffectivelyVisible Then Continue For
            If visibleIndex < _frozenColumnCount Then
                _frozenLayout.Add(New ColLayout With {.Column = c, .X = frozenX})
                frozenX += c.WidthPx
            Else
                _scrollLayout.Add(New ColLayout With {.Column = c, .X = scrollX})
                scrollX += c.WidthPx
            End If
            visibleIndex += 1
        Next

        _frozenBandWidth = frozenX
        _scrollBandWidth = scrollX
    End Sub

    ' ── Geometrie ───────────────────────────────────────────────────────────────

    ''' <summary>Înălțimea efectivă a benzii de antet (0 dacă e ascunsă).</summary>
    Private Function HeaderBandHeight() As Integer
        Return If(_showHeader, EffectiveHeaderHeight(), 0)
    End Function

    ' ── Înălțimea benzii de antet, când titlurile se scriu pe mai multe linii ────
    '
    ' Banda are DOUĂ înălțimi și e important să nu fie confundate:
    '
    '   • HeaderHeight — cea cerută de operator, în designer. Nu se schimbă niciodată singură.
    '   • EffectiveHeaderHeight — cea desenată: HeaderHeight, ridicată cât cere cea mai înaltă
    '     coloană cu MultiLine, plafonată la MaxHeaderHeight.
    '
    ' A doua e o FUNCȚIE de lățimile curente, deci trebuie să și COBOARE: coloana lărgită (fie de
    ' o umplere, fie de tragerea operatorului) încape pe mai puține rânduri, iar banda se strânge
    ' la loc. De aceea măsurătoarea se uită la fiecare RecalcColumnLayout și nu se ține între
    ' treceri — un cache care nu s-ar șterge ar face banda să crească o dată și să rămână așa.
    '
    ' Și e o înălțime de GEOMETRIE, nu de desen: din ea pleacă rândurile, corpul derulabil,
    ' barele și hit-testul benzii. Prima versiune a mărit doar dreptunghiul pictat, iar rândurile
    ' au rămas să înceapă la vechiul HeaderHeight — adică sub antetul înalt.

    ' -1 = trebuie măsurată din nou (vezi InvalidateHeaderHeight).
    Private _measuredHeaderHeight As Integer = -1

    ' Plafonul benzii; 0 = fără plafon. Vezi proprietatea MaxHeaderHeight.
    Private _maxHeaderHeight As Integer = 0

    ' Banda se măsoară după text, sau rămâne fixă? Vezi proprietatea AutoSizeHeaderHeight.
    Private _autoSizeHeaderHeight As Boolean = True

    ''' <summary>
    ''' Spațiul (px logici) lăsat deasupra și dedesubtul titlului pe mai multe linii. E MIC pe
    ''' bună dreptate: înălțimea măsurată a textului include deja interlinia proprie a fontului
    ''' (~3px sus și jos la Segoe UI 9), deci ce se adaugă aici se vede DUBLU. Cu 4 de fiecare
    ''' parte, două rânduri de titlu ajungeau la o bandă de 38px pentru 24px de litere.
    ''' </summary>
    Private Const HeaderTextPadY As Integer = 2

    ''' <summary>Uită înălțimea măsurată a antetului; următoarea interogare o recalculează.</summary>
    Private Sub InvalidateHeaderHeight()
        _measuredHeaderHeight = -1
    End Sub

    ''' <summary>
    ''' Înălțimea pe care o are efectiv banda de antet: <see cref="HeaderHeight"/> ridicată cât
    ''' cere cea mai înaltă coloană cu <see cref="KBotDataColumn.MultiLine"/> și plafonată la
    ''' <see cref="MaxHeaderHeight"/>. Friend: o citește pictarea, hit-testul și testele — o a
    ''' doua formulă ar însemna o bandă desenată altundeva decât se apasă.
    ''' </summary>
    Friend Function EffectiveHeaderHeight() As Integer
        If _measuredHeaderHeight < 0 Then _measuredHeaderHeight = MeasureHeaderBandHeight()
        Return _measuredHeaderHeight
    End Function

    ' Cât cere banda: minimul cerut de operator, urcat de coloanele pe mai multe linii, apoi
    ' plafonat. Plafonul NU coboară sub HeaderHeight — vezi MaxHeaderHeight.
    Private Function MeasureHeaderBandHeight() As Integer
        ' Măsurarea stinsă: banda e exact cât s-a cerut, indiferent câte rânduri ar avea titlurile.
        ' Ce nu încape se taie — asta e chiar înțelesul lui AutoSizeHeaderHeight = False.
        If Not _autoSizeHeaderHeight Then Return _headerHeight

        Dim inaltime As Integer = _headerHeight
        Dim pad As Integer = ScaleDpi(HeaderTextPadY)

        For Each c In _columns
            If Not c.IsEffectivelyVisible OrElse Not HeaderIsMultiLine(c) Then Continue For
            Dim textH As Integer = MeasureHeaderTextHeight(c)
            If textH <= 0 Then Continue For
            inaltime = Math.Max(inaltime, textH + 2 * pad)
        Next

        If _maxHeaderHeight > 0 Then inaltime = Math.Min(inaltime, Math.Max(_headerHeight, _maxHeaderHeight))
        Return inaltime
    End Function

    ''' <summary>
    ''' Cât de înalt iese titlul unei coloane rupt la lățimea LUI DE TEXT — nu la lățimea coloanei:
    ''' pictogramele de antet mănâncă din ea, iar o măsurare peste lățimea întreagă ar tăia ultimul
    ''' rând. Se măsoară cu fontul și cu steagurile cu care se și desenează
    ''' (<see cref="HeaderTextFlags"/>), altfel banda și textul ar fi calculate din două formule.
    ''' </summary>
    Private Function MeasureHeaderTextHeight(col As KBotDataColumn) As Integer
        If col Is Nothing OrElse String.IsNullOrEmpty(col.HeaderText) Then Return 0
        Dim latime As Integer = HeaderTextWidthFor(col)
        If latime <= 0 Then Return 0
        Return TextRenderer.MeasureText(col.HeaderText, HeaderFontFor(col),
                                        New Size(latime, Integer.MaxValue),
                                        HeaderTextFlags(col)).Height
    End Function

    ''' <summary>
    ''' Lățimea rămasă titlului într-o celulă de antet, după pictograme. Nu depinde de înălțimea
    ''' benzii (așezarea folosește înălțimea doar ca să centreze pictogramele pe verticală), deci
    ''' se poate cere ÎNAINTE ca înălțimea să fie știută — altfel calculul s-ar mușca de coadă.
    ''' </summary>
    Private Function HeaderTextWidthFor(col As KBotDataColumn) As Integer
        Return HeaderLayoutFor(col, New Rectangle(0, 0, col.WidthPx, _headerHeight)).Text.Width
    End Function

    ''' <summary>Înălțimea efectivă a benzii de subsol (0 dacă e stinsă).</summary>
    Private Function FooterBandHeight() As Integer
        If Not _showFooter Then Return 0
        ' Câmpurile SCALATE, nu proprietatea: aceea întoarce valoarea logică (px la 96 dpi), care
        ' e ce a cerut operatorul, nu ce se desenează. Vezi KBotDataView.Dpi.vb.
        Return If(_footerHeight > 0, _footerHeight, _headerHeight)
    End Function

    ''' <summary>Y-ul (client) la care începe banda de subsol — sub antet și sub corp.</summary>
    Private Function FooterBandTop() As Integer
        Return HeaderBandHeight() + ViewportHeight()
    End Function

    ''' <summary>Lățimea zonei utile (client minus bara verticală, dacă e vizibilă).</summary>
    Private Function ViewportWidth() As Integer
        Return Math.Max(0, ClientSize.Width - If(vScroll.Visible, vScroll.Width, 0))
    End Function

    ''' <summary>
    ''' Înălțimea zonei de date (client minus antet minus banda de subsol minus bara orizontală).
    ''' Banda de subsol mănâncă din înălțimea corpului, exact ca antetul.
    '''
    ''' Strâns pe verticală (slice 0028) corpul are înălțime ZERO: nu e o zonă ascunsă sub
    ''' altceva, ci una care nu există — virtualizarea, hit-testul și barele de derulare se
    ''' opresc toate din acest singur număr.
    ''' </summary>
    Private Function ViewportHeight() As Integer
        If BodyIsCollapsed() Then Return 0
        Return Math.Max(0, ClientSize.Height - HeaderBandHeight() - FooterBandHeight() -
                           If(hScroll.Visible, hScroll.Height, 0))
    End Function

    ''' <summary>Offset-ul vertical curent, în pixeli.</summary>
    Private Function VScrollOffset() As Integer
        Return If(vScroll.Visible, vScroll.Value, 0)
    End Function

    ''' <summary>Offset-ul orizontal curent al benzii derulate, în pixeli.</summary>
    Private Function HScrollOffset() As Integer
        Return If(hScroll.Visible, hScroll.Value, 0)
    End Function

    ' ── Virtualizare ────────────────────────────────────────────────────────────

    ' English (slice 0028-03): everything below counts in VIEW POSITIONS — the on-screen order
    ' after filtering and sorting — never in model indices. See KBotDataView.Filtering for the two
    ' numbering schemes and why they must not be mixed.
    '
    ' English (slice 0029): and now in BAND INDICES, which is a third numbering — the rows actually
    ' DRAWN, group headers and group footers included. The old arithmetic (position × RowHeight)
    ' survives untouched on a grid nobody grouped: KBotDataView.Grouping keeps that fast path and
    ' allocates no band table at all. Grouped, the bands carry cumulative offsets and the lookups
    ' below become a binary search — which is what buys group bands their OWN heights without
    ' making a painting pass depend on the row count again.

    ''' <summary>Primul INDEX DE BANDĂ vizibil, dedus din offset-ul în pixeli. -1 = nimic de pictat.</summary>
    Private Function FirstVisibleBand() As Integer
        Dim n As Integer = BandCount()
        If n = 0 Then Return -1
        Dim idx As Integer = BandIndexAtOffset(VScrollOffset())
        If idx < 0 Then Return If(VScrollOffset() <= 0, 0, n - 1)
        Return idx
    End Function

    ''' <summary>
    ''' Ultimul INDEX DE BANDĂ de pictat. Cuprinde și banda tăiată de marginea de jos: decuparea
    ''' zonei de date o retează oricum, iar o bandă lipsă la margine s-ar vedea ca o gaură.
    ''' </summary>
    Private Function LastVisibleBand() As Integer
        Dim n As Integer = BandCount()
        If n = 0 Then Return -1
        Dim idx As Integer = BandIndexAtOffset(Math.Max(0, VScrollOffset() + ViewportHeight()))
        If idx < 0 Then Return n - 1
        Return Math.Min(n - 1, idx)
    End Function

    ''' <summary>Y-ul (client) al unei benzi, ținând cont de antet și de derulare.</summary>
    Private Function BandTop(bandIndex As Integer) As Integer
        Return HeaderBandHeight() + BandAt(bandIndex).Top - VScrollOffset()
    End Function

    ''' <summary>
    ''' Y-ul (client) al unei POZIȚII DE VEDERE. <see cref="Integer.MinValue"/> = rândul nu se
    ''' desenează nicăieri, fiindcă stă într-un grup STRÂNS (slice 0029).
    ''' </summary>
    Private Function RowTop(viewPosition As Integer) As Integer
        Dim bi As Integer = BandIndexOfViewPosition(viewPosition)
        If bi < 0 Then Return Integer.MinValue
        Return BandTop(bi)
    End Function

    ''' <summary>
    ''' Y-ul (client) al unui rând dat prin indexul lui de MODEL — poarta prin care API-ul public
    ''' (celule, editare, etichetă) ajunge la geometrie. <see cref="Integer.MinValue"/> = rândul nu
    ''' are niciun Y, fie pentru că e filtrat afară, fie pentru că grupul lui e strâns. Apelanții
    ''' (<c>CellRect</c>, <c>CellRectangle</c>) verifică deja exact valoarea asta.
    ''' </summary>
    Private Function RowTopForModel(modelIndex As Integer) As Integer
        Dim vp As Integer = ViewPositionOf(modelIndex)
        If vp < 0 Then Return Integer.MinValue
        Return RowTop(vp)
    End Function

    ' ── Bare de derulare ────────────────────────────────────────────────────────

    ' Legarea evenimentelor (apelată din constructor, după InitializeComponent).
    Private Sub WireScrollBars()
        AddHandler vScroll.ValueChanged, AddressOf OnScrollValueChanged
        AddHandler hScroll.ValueChanged, AddressOf OnScrollValueChanged
        AddHandler hScroll.Scroll, AddressOf OnHScrollBarScroll
    End Sub

    ' Evenimentul Scroll spune CUM se derulează (thumb-track vs. săgeți/șină/eliberare) —
    ' informație pe care ValueChanged n-o are. Cât timp se TRAGE thumb-ul, derulăm liber
    ' (pixel cu pixel), altfel alinierea ar smuci thumb-ul înapoi la fiecare mișcare a
    ' mouse-ului. Alinierea se face DOAR la eliberare (EndScroll), la marginea cea mai
    ' apropiată. Săgețile/șina/rotița trec prin ValueChanged (aliniere direcțională) ca înainte.
    Private Sub OnHScrollBarScroll(sender As Object, e As ScrollEventArgs)
        Try
            If e.Type = ScrollEventType.ThumbTrack Then
                _hThumbTracking = True
            ElseIf e.Type = ScrollEventType.EndScroll Then
                EndHorizontalThumbDrag()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnHScrollBarScroll", ex)
        End Try
    End Sub

    ' Încheie tragerea thumb-ului: aliniere la marginea cea mai APROPIATĂ (la eliberare
    ' „cea mai apropiată” e intuitiv; la săgeți/rotiță e direcțional — vezi SnappedHValue).
    ''' <summary>Friend: testele nu pot ridica evenimentul Scroll fără buclă de mesaje.</summary>
    Friend Sub EndHorizontalThumbDrag()
        _hThumbTracking = False
        If Not _scrollByColumn OrElse _snappingHScroll Then Return
        If Not hScroll.Visible OrElse _scrollLayout.Count = 0 Then Return
        ApplySnappedHValue(NearestColumnStart(hScroll.Value))
        _lastHScrollValue = hScroll.Value
    End Sub

    ''' <summary>Friend: pornește o tragere de thumb simulată (doar pentru teste).</summary>
    Friend Sub BeginHorizontalThumbDrag()
        _hThumbTracking = True
    End Sub

    Private Sub OnScrollValueChanged(sender As Object, e As EventArgs)
        Try
            ' Aliniere la margini de coloană (dacă ScrollByColumn e activ) ÎNAINTE de a picta.
            ' Se SARE cât timp se trage thumb-ul (derulare liberă, aliniere abia la eliberare).
            If ReferenceEquals(sender, hScroll) Then
                SnapHScrollToColumn()
                _lastHScrollValue = hScroll.Value
            End If

            ' Derularea comite editarea deschisă: un editor real care plutește peste o celulă
            ' care tocmai a ieșit din fereastră ar rămâne agățat în aer. Din același motiv
            ' cade și eticheta: celula ei tocmai s-a mutat de sub cursor.
            If _editing Then CommitEdit()
            CancelCellTooltip()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnScrollValueChanged", ex)
        End Try
    End Sub

    ' ── Derulare orizontală pe coloane ──────────────────────────────────────────

    ''' <summary>
    ''' Când e True, derularea ORIZONTALĂ se aliniază la marginile coloanelor (o coloană
    ''' întreagă odată), în loc să meargă pixel cu pixel. Nu atinge derularea verticală —
    ''' aceea e deja „pe rând”, prin virtualizare.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Derularea orizontală se aliniază la marginile coloanelor, nu pixel cu pixel.")>
    <DefaultValue(False)>
    Public Property ScrollByColumn As Boolean
        Get
            Return _scrollByColumn
        End Get
        Set(value As Boolean)
            If _scrollByColumn = value Then Return
            _scrollByColumn = value
            If value Then
                ' Activarea aliniază pe loc poziția curentă (care putea fi la mijloc de coloană).
                SnapHScrollToColumn()
                _lastHScrollValue = hScroll.Value
            End If
            Invalidate()
        End Set
    End Property

    ' Aliniază hScroll.Value la o margine de coloană, în DIRECȚIA mișcării: la creștere urcă
    ' la marginea următoare, la scădere coboară la cea precedentă. Așa un pas mic (o săgeată,
    ' o rotiță) tot avansează o coloană întreagă, nu se lipește de aceeași margine.
    ' Se SARE cât timp se trage thumb-ul — atunci derularea e liberă, alinierea vine la EndScroll.
    Private Sub SnapHScrollToColumn()
        If Not _scrollByColumn OrElse _snappingHScroll OrElse _hThumbTracking Then Return
        If Not hScroll.Visible OrElse _scrollLayout.Count = 0 Then Return
        ApplySnappedHValue(SnappedHValue(hScroll.Value))
    End Sub

    ' Scrie valoarea aliniată în bară, cu gardă de reintrare (setarea re-ridică ValueChanged).
    Private Sub ApplySnappedHValue(snapped As Integer)
        If snapped = hScroll.Value Then Return
        _snappingHScroll = True
        Try
            hScroll.Value = snapped
        Finally
            _snappingHScroll = False
        End Try
    End Sub

    ' Maximul util al barei orizontale (semantica WinForms: Maximum − LargeChange + 1).
    Private Function HScrollMaxValue() As Integer
        Return Math.Max(0, hScroll.Maximum - hScroll.LargeChange + 1)
    End Function

    ' Valoarea aliniată DIRECȚIONAL (săgeți/rotiță/pas mic): ceil la creștere, floor la scădere.
    Private Function SnappedHValue(rawValue As Integer) As Integer
        Dim target As Integer
        If rawValue >= _lastHScrollValue Then
            target = CeilToColumnStart(rawValue)     ' creștere => marginea următoare
        Else
            target = FloorToColumnStart(rawValue)    ' scădere => marginea precedentă
        End If
        Return Math.Max(0, Math.Min(target, HScrollMaxValue()))
    End Function

    ' Marginea de coloană cea mai APROPIATĂ de offset (folosită la eliberarea thumb-ului).
    ' Capătul (maximul util) e și el o poziție validă de așezare pentru ultimele coloane.
    Private Function NearestColumnStart(rawValue As Integer) As Integer
        Dim maxValue As Integer = HScrollMaxValue()
        Dim best As Integer = 0
        Dim bestDist As Integer = Math.Abs(rawValue)             ' candidatul 0
        For Each cl In _scrollLayout
            If cl.X > maxValue Then Continue For
            Dim d As Integer = Math.Abs(cl.X - rawValue)
            If d < bestDist Then
                bestDist = d
                best = cl.X
            End If
        Next
        If Math.Abs(maxValue - rawValue) < bestDist Then best = maxValue
        Return best
    End Function

    ' Cea mai mică margine de coloană (start în banda derulată) >= v. Dincolo de ultima
    ' margine => lățimea benzii (va fi limitată apoi la maximul util => se vede coada).
    Private Function CeilToColumnStart(v As Integer) As Integer
        For Each cl In _scrollLayout
            If cl.X >= v Then Return cl.X
        Next
        Return _scrollBandWidth
    End Function

    ' Cea mai mare margine de coloană <= v (offset-urile sunt crescătoare prin construcție).
    Private Function FloorToColumnStart(v As Integer) As Integer
        Dim best As Integer = 0
        For Each cl In _scrollLayout
            If cl.X <= v Then best = cl.X Else Exit For
        Next
        Return best
    End Function

    ''' <summary>
    ''' Recalculează coloanele și reconfigurează barele de derulare. Gardă de reintrare:
    ''' comutarea vizibilității unei bare schimbă spațiul disponibil pentru cealaltă.
    ''' </summary>
    Private Sub UpdateLayout()
        If _inLayout Then Return
        _inLayout = True
        Try
            ' English (slice 0013): size the columns first, then compute offsets and scrollbars.
            ' The pass no-ops while _updateDepth > 0 and runs once from EndUpdate.
            PerformAutoSize()
            RecalcColumnLayout()
            UpdateScrollBars()
        Finally
            _inLayout = False
        End Try
    End Sub

    ' Decide vizibilitatea/valorile barelor. Cele două se influențează reciproc, deci
    ' evaluăm în două treceri (bara verticală mănâncă lățime, cea orizontală înălțime).
    Private Sub UpdateScrollBars()
        ' Strâns pe verticală nu mai există corp de derulat: barele se sting amândouă, altfel ar
        ' rămâne atârnate peste cele două benzi.
        If BodyIsCollapsed() Then
            If vScroll.Visible Then vScroll.Visible = False
            If hScroll.Visible Then hScroll.Visible = False
            vScroll.Value = 0
            hScroll.Value = 0
            Return
        End If

        Dim vw As Integer = SystemInformation.VerticalScrollBarWidth
        Dim hh As Integer = SystemInformation.HorizontalScrollBarHeight
        Dim headerH As Integer = HeaderBandHeight()
        ' Banda de subsol stă între corp și bara orizontală, deci se scade din înălțimea
        ' disponibilă a corpului exact ca antetul.
        Dim totalsH As Integer = FooterBandHeight()

        ' Se derulează doar rândurile care trec de filtre — altfel bara ar promite o înălțime de
        ' conținut pe care grila n-o mai are și s-ar putea derula sub ultimul rând vizibil.
        ' Slice 0029: și doar BENZILE desenate, deci fără rândurile din grupurile strânse și cu
        ' tot cu antetele/subsolurile de grup. ContentHeight() e singurul loc care o știe.
        Dim contentH As Integer = ContentHeight()
        Dim totalColsW As Integer = _frozenBandWidth + _scrollBandWidth

        Dim availW As Integer = ClientSize.Width
        Dim availH As Integer = Math.Max(0, ClientSize.Height - headerH - totalsH)

        Dim needV As Boolean = contentH > availH
        If needV Then availW = Math.Max(0, availW - vw)

        Dim needH As Boolean = totalColsW > availW
        If needH Then
            availH = Math.Max(0, availH - hh)
            ' A doua trecere: pierderea de înălțime poate cere acum și bara verticală.
            If Not needV AndAlso contentH > availH Then
                needV = True
                availW = Math.Max(0, availW - vw)
                needH = totalColsW > availW
            End If
        End If

        ' Verticală.
        If needV Then
            vScroll.Bounds = New Rectangle(ClientSize.Width - vw, headerH, vw, availH)
            ConfigureScrollBar(vScroll, contentH, availH, _rowHeight)
        End If
        If vScroll.Visible <> needV Then vScroll.Visible = needV
        If Not needV Then vScroll.Value = 0

        ' Orizontală — derulează DOAR banda ne-înghețată.
        If needH Then
            hScroll.Bounds = New Rectangle(0, ClientSize.Height - hh, availW, hh)
            Dim scrollViewport As Integer = Math.Max(0, availW - _frozenBandWidth)
            ConfigureScrollBar(hScroll, _scrollBandWidth, scrollViewport, Math.Max(1, _rowHeight))
        End If
        If hScroll.Visible <> needH Then hScroll.Visible = needH
        If Not needH Then hScroll.Value = 0

        ' Lățimile s-au putut schimba (auto-size, slice 0013): re-aliniază la o margine.
        If needH AndAlso _scrollByColumn Then
            SnapHScrollToColumn()
            _lastHScrollValue = hScroll.Value
        End If
    End Sub

    ' Setează intervalul unei bare. Semantica WinForms: valoarea maximă atinsă efectiv este
    ' Maximum - LargeChange + 1, deci Maximum = conținut - 1 și LargeChange = fereastra.
    Private Shared Sub ConfigureScrollBar(bar As ScrollBar, contentSize As Integer,
                                          viewportSize As Integer, smallChange As Integer)
        Dim viewport As Integer = Math.Max(1, viewportSize)
        bar.Minimum = 0
        bar.Maximum = Math.Max(0, contentSize - 1)
        bar.LargeChange = viewport
        bar.SmallChange = Math.Max(1, smallChange)
        ' Clamp: după micșorarea conținutului, Value poate depăși noul maxim util.
        Dim maxValue As Integer = Math.Max(0, contentSize - viewport)
        If bar.Value > maxValue Then bar.Value = maxValue
    End Sub

    ''' <summary>
    ''' Derulează astfel încât rândul dat să fie complet vizibil. <paramref name="rowIndex"/> e un
    ''' index de MODEL (API public); un rând care nu se desenează nicăieri — filtrat afară, sau
    ''' într-un grup strâns — nu are unde să fie derulat, deci e un no-op. Nu o eroare: apelantul
    ''' obișnuit (o selecție, un commit) n-are de unde ști că tocmai a fost ascuns.
    ''' </summary>
    Public Sub EnsureVisible(rowIndex As Integer)
        Try
            If rowIndex < 0 OrElse rowIndex >= _rows.Count Then Return
            ' Ancora, nu banda: un rând închis într-un grup strâns se derulează la ANTETUL
            ' grupului — acela e ce se vede din el, și e locul de unde se poate redeschide.
            Dim bandIndex As Integer = AnchorBandOfRow(rowIndex)
            If bandIndex < 0 Then Return
            If Not vScroll.Visible Then Return

            Dim banda As KBotBand = BandAt(bandIndex)
            Dim viewH As Integer = ViewportHeight()
            Dim top As Integer = banda.Top
            Dim bottom As Integer = top + banda.Height
            Dim current As Integer = vScroll.Value
            Dim target As Integer = current

            If top < current Then
                target = top                              ' iese pe sus
            ElseIf bottom > current + viewH Then
                target = bottom - viewH                   ' iese pe jos
            End If

            Dim maxValue As Integer = Math.Max(0, ContentHeight() - viewH)
            target = Math.Max(0, Math.Min(target, maxValue))
            If target <> current Then vScroll.Value = target
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.EnsureVisible", ex)
            Throw
        End Try
    End Sub

    ' ── Rotița mouse-ului ───────────────────────────────────────────────────────

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Try
            Dim notches As Integer = e.Delta \ 120
            If notches = 0 Then Return

            ' Shift + rotiță => derulare orizontală (convenție Windows).
            Dim bar As ScrollBar = If((ModifierKeys And Keys.Shift) = Keys.Shift, CType(hScroll, ScrollBar), CType(vScroll, ScrollBar))
            If bar Is Nothing OrElse Not bar.Visible Then Return

            Dim linesPerNotch As Integer = Math.Max(1, SystemInformation.MouseWheelScrollLines)
            Dim delta As Integer = notches * linesPerNotch * bar.SmallChange
            Dim maxValue As Integer = Math.Max(0, bar.Maximum - bar.LargeChange + 1)
            bar.Value = Math.Max(0, Math.Min(bar.Value - delta, maxValue))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnMouseWheel", ex)
        End Try
    End Sub

    ' ── Ajutor DPI ──────────────────────────────────────────────────────────────

    ' Scalează o valoare logică (px @96dpi) la scara controlului.
    '
    ' Răspunsul vine din AppScaling — SURSA UNICĂ a scării de când operatorul o poate fixa la
    ' 100% sau pune un factor al lui (felia 0036). Aici se calcula până acum direct din
    ' `DeviceDpi`, adică exact a doua formulă de care se ferește nota de DPI din .Dpi.vb:
    ' măsurile proprii (rând, antet, lățimi de coloană) treceau prin AppScaling, iar
    ' constantele de pictură — spațiile dintre pictograme, caseta de bifă, chevronul — nu, deci
    ' pe scalare fixată la 100% pictogramele rămâneau mari într-un antet care se strânsese.
    Private Function ScaleDpi(logical As Integer) As Integer
        Return ThemeShapes.ScaleDpi(Me, logical)
    End Function

End Class
