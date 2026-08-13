Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports KBot.Common

''' <summary>
''' GRUPAREA <see cref="KBotDataView"/> (slice 0029) — echivalentul secțiunilor de grup dintr-un
''' raport Access: pe fiecare nivel, o bandă de antet și una de subsol, cu agregate proprii,
''' culori proprii și fonturi proprii.
'''
''' <para><b>A TREIA numerotare, și cea mai ușor de confundat.</b> Partiala <c>.Filtering</c>
''' descrie două: <i>indicii de MODEL</i> (ordinea în care apelantul a adăugat rândurile) și
''' <i>pozițiile de VEDERE</i> (ordinea de pe ecran, după filtrare și sortare). Gruparea adaugă
''' <i>indicii de BANDĂ</i> — rândurile DESENATE, printre care se numără și antetele/subsolurile de
''' grup, care nu sunt rânduri de model deloc. Regula rămâne aceeași, cu un etaj în plus:</para>
'''
''' <list type="bullet">
''' <item><description><b>API-ul public vorbește în indici de MODEL.</b> Gruparea nu schimbă
''' nimic acolo: <c>Item(cheie, index)</c>, <c>CurrentRowIndex</c>, <c>CellClick</c> înseamnă azi
''' ce însemnau înainte de ea.</description></item>
''' <item><description><b><see cref="_view"/> rămâne lista rândurilor de DATE</b>, în ordinea de
''' pe ecran. Gruparea NU scoate rânduri din ea — nici măcar cele ale unui grup strâns (vezi mai
''' jos), deci agregatele grilei și măsurarea coloanelor lucrează exact ca înainte.</description></item>
''' <item><description><b>Geometria lucrează în indici de BANDĂ</b> — și, de la felia asta,
''' benzile n-au toate aceeași înălțime, deci aritmetica întreagă a virtualizării
''' (<c>poziție × RowHeight</c>) e înlocuită cu offset-uri cumulate + căutare binară. Vezi
''' <see cref="BandIndexAtOffset"/>.</description></item>
''' </list>
'''
''' <para><b>Strângerea nu e o filtrare.</b> Un grup strâns își ascunde rândurile de pe ecran, dar
''' ele rămân în <see cref="_view"/>: totalul grilei nu se schimbă când operatorul închide o lună,
''' și nici totalurile grupurilor de deasupra. Un filtru SCOATE rânduri și atunci totalurile se
''' schimbă — pentru că atunci s-a schimbat pagina, nu doar ce se vede din ea.</para>
'''
''' <para><b>Fără niciun nivel activ, nimic din fișierul acesta nu costă.</b>
''' <see cref="IsGrouped"/> e False, tabloul de benzi nu se construiește deloc, iar geometria
''' rămâne pe înmulțirea de dinainte. Asta contează: tabloul de benzi e O(rânduri vizibile), adică
''' exact ce evita virtualizarea, și n-are voie să fie plătit de cele șase vederi care nu grupează.</para>
''' </summary>
Partial Class KBotDataView

    ' ══════════════════════════════════════════════════════════════════════════
    ' MODELUL
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Un rând DESENAT. Nivelul și indexul de model nu se țin aici, ci se deduc (din
    ''' <c>_groups(GroupIndex)</c>, respectiv din <c>_view(ViewPosition)</c>): structura se
    ''' alocă o dată per rând vizibil, deci fiecare câmp în plus se înmulțește cu zecile de mii.
    ''' </summary>
    Friend Structure KBotBand
        ''' <summary>Antet de grup, subsol de grup, sau rând de date.</summary>
        Public Kind As KBotGroupBandKind
        ''' <summary>Indexul grupului în <c>_groups</c>; -1 pentru un rând de date.</summary>
        Public GroupIndex As Integer
        ''' <summary>Poziția de vedere a rândului; -1 pentru o bandă de grup.</summary>
        Public ViewPosition As Integer
        ''' <summary>Offset-ul Y (px) față de vârful CONȚINUTULUI derulabil, nu față de client.</summary>
        Public Top As Integer
        ''' <summary>Înălțimea benzii (px).</summary>
        Public Height As Integer
    End Structure

    ''' <summary>
    ''' Un grup: o valoare a coloanei de grupare, pe un nivel, peste un interval CONTIGUU de
    ''' poziții de vedere. Contiguitatea e garantată prin construcție — sortarea așază cheile de
    ''' grupare înaintea oricărei alte sortări (vezi <c>KBotDataView.Filtering</c>).
    ''' </summary>
    Friend NotInheritable Class KBotGroupNode
        Public Level As Integer
        ''' <summary>Cheia de rupere: TEXTUL AFIȘAT al valorii (vezi <see cref="GroupKeyText"/>).</summary>
        Public Key As String
        ''' <summary>Valoarea brută, luată din primul rând al grupului (pentru handler-ul de formatare).</summary>
        Public Value As Object
        Public FirstViewPos As Integer
        Public LastViewPos As Integer
        Public ParentIndex As Integer = -1
        Public ReadOnly Children As New List(Of Integer)()
        ''' <summary>Calea completă (cheile părinților + a lui) — identitatea sub care se ține strângerea.</summary>
        Public Path As String

        ''' <summary>
        ''' Indexul benzii lui de ANTET, sau -1 (nivel fără antet, ori grup nedesenat). E ancora
        ''' pe care se sprijină navigarea când rândul curent e închis într-un grup strâns — vezi
        ''' <c>AnchorBandOfRow</c>.
        ''' </summary>
        Public HeaderBandIndex As Integer = -1

        ''' <summary>Câte rânduri de date are grupul (cu tot cu sub-grupuri).</summary>
        Public ReadOnly Property RowCount As Integer
            Get
                Return Math.Max(0, LastViewPos - FirstViewPos + 1)
            End Get
        End Property

        ''' <summary>Textele agregate, cache-uite LENEȘ pe cheie de coloană — vezi GroupAggregateText.</summary>
        Public ReadOnly Aggregates As New Dictionary(Of String, String)(StringComparer.Ordinal)
    End Class

    ' ── Stare ───────────────────────────────────────────────────────────────────

    Private ReadOnly _levels As New KBotGroupLevelCollection()

    ' Nivelurile care chiar grupează: cheie nevidă ȘI o coloană cunoscută în spatele ei.
    Private ReadOnly _activeLevels As New List(Of KBotGroupLevel)()

    Private ReadOnly _groups As New List(Of KBotGroupNode)()

    ' Indicii grupurilor de nivel 0, în ordinea de pe ecran — punctul de pornire al emiterii.
    ' Ținuți separat ca emiterea să nu caute niciodată un nod în listă: o căutare per grup ar face
    ' trecerea pătratică exact pe grilele cu multe grupuri, adică pe cele pentru care există.
    Private ReadOnly _rootGroups As New List(Of Integer)()

    Private _bands As KBotBand() = Array.Empty(Of KBotBand)()
    Private _bandCount As Integer = 0

    ' Poziție de vedere -> index de bandă, sau -1 dacă rândul stă într-un grup STRÂNS.
    Private _bandOfView As Integer() = Array.Empty(Of Integer)()

    Private _contentHeight As Integer = 0
    Private _bandsDirty As Boolean = True

    ' Strângerea se ține pe CALE, nu pe index: indicii se schimbă la fiecare resortare sau
    ' refiltrare, iar o lună închisă de operator trebuie să rămână închisă și după ce a venit
    ' o încărcare nouă cu aceleași chei.
    Private ReadOnly _collapsedPaths As New HashSet(Of String)(StringComparer.Ordinal)

    ' Căile cărora li s-a aplicat deja CollapsedByDefault (ca o desfacere de-a operatorului să nu
    ' fie „corectată” înapoi la fiecare reconstrucție).
    Private ReadOnly _defaultsApplied As New HashSet(Of String)(StringComparer.Ordinal)

    ' Instanță REFOLOSITĂ de argumente pentru GroupFormatting (zero alocări per bandă pictată).
    Private ReadOnly _groupArgs As New KBotGroupFormattingEventArgs()

    ''' <summary>Un grup s-a strâns sau s-a desfăcut.</summary>
    Public Event GroupCollapsedChanged As EventHandler

    ''' <summary>
    ''' Ridicat pentru fiecare bandă de grup pictată, înaintea desenului ei. Argumentele sunt
    ''' REFOLOSITE — nu le reține.
    ''' </summary>
    Public Event GroupFormatting As EventHandler(Of KBotGroupFormattingEventArgs)

    ' ══════════════════════════════════════════════════════════════════════════
    ' API PUBLIC — nivelurile
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Nivelurile de grupare, de la cel dinafară spre cel dinăuntru. Editabile din grila de
    ''' proprietăți (dialogul standard de colecție) sau din cod — aceeași colecție.
    ''' Colecție goală = grilă negrupată, adică exact purtarea de dinainte de slice 0029.
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Nivelurile de grupare, de la cel dinafară spre cel dinăuntru. Goală = grilă negrupată.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Groups As KBotGroupLevelCollection
        Get
            Return _levels
        End Get
    End Property

    ''' <summary>Grila grupează după cel puțin un nivel valabil?</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsGrouped As Boolean
        Get
            RefreshActiveLevels()
            Return _activeLevels.Count > 0
        End Get
    End Property

    ''' <summary>
    ''' Grupare rapidă după o singură coloană: șterge nivelurile existente și pune unul.
    ''' Cheie necunoscută => <see cref="ArgumentException"/>, ca peste tot.
    ''' </summary>
    Public Function GroupBy(colKey As String,
                            Optional direction As KBotSortDirection = KBotSortDirection.Ascending) As KBotGroupLevel
        Try
            Column(colKey)                      ' cheie necunoscută => ArgumentException
            _levels.Clear()
            Dim nivel As New KBotGroupLevel(colKey, direction)
            _levels.Add(nivel)
            Return nivel
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.GroupBy", ex)
            Throw
        End Try
    End Function

    ''' <summary>Ridică toată gruparea (sortarea cerută de operator rămâne pe loc).</summary>
    Public Sub ClearGrouping()
        Try
            If _levels.Count = 0 Then Return
            _levels.Clear()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ClearGrouping", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Meniul de coloană îi oferă operatorului fila «Grupare» (slice 0030). Implicit False.
    '''
    ''' <para><b>Nu atinge gruparea AUTORATĂ.</b> <see cref="Groups"/> rămâne exact ce e:
    ''' o grilă poate porni grupată din designer cu steagul acesta stins, și atunci gruparea e a
    ''' machetei, nu una pe care operatorul o poate desface din meniu. Steagul spune un singur
    ''' lucru: dacă fila de grupare se vede sau nu în <c>KBotFilterPopup</c>.</para>
    '''
    ''' <para>Implicit STINS fiindcă cele șase vederi livrate n-au cerut niciodată o grupare pe care
    ''' s-o schimbe operatorul; o filă nouă apărută peste noapte în meniul lor ar fi o schimbare de
    ''' comportament, nu o îmbunătățire.</para>
    ''' </summary>
    <Category("K-BOT: Grupare")>
    <Description("Meniul de coloană arată fila «Grupare». Nu atinge nivelurile autorate în designer.")>
    <DefaultValue(False)>
    Public Property EnableGrouping As Boolean
        Get
            Return _enableGrouping
        End Get
        Set(value As Boolean)
            _enableGrouping = value
        End Set
    End Property
    Private _enableGrouping As Boolean

    ''' <summary>
    ''' Așază, înlocuiește sau RIDICĂ nivelul de grupare al unei coloane — drumul pe care îl face
    ''' fila «Grupare» din meniul de coloană. <c>Nothing</c> = coloana nu mai grupează.
    '''
    ''' <para>Nivelul primit e ADOPTAT ca atare (i se pune cheia coloanei și intră în
    ''' <see cref="Groups"/>), nu copiat: apelantul construiește un <see cref="KBotGroupLevel"/>
    ''' liber, cu opțiunile lui, și-l predă. Un nivel existent pe aceeași coloană se înlocuiește pe
    ''' LOCUL LUI din ierarhie — o schimbare de opțiuni n-are voie să mute coloana de pe nivelul 1
    ''' pe ultimul, fiindcă ordinea nivelurilor E ierarhia.</para>
    '''
    ''' <para>Strângerea grupurilor nu se pierde: ea se ține pe CALE, în grilă, nu pe obiectul de
    ''' nivel (vezi <c>_collapsedPaths</c>).</para>
    ''' </summary>
    Public Sub SetColumnGroupLevel(colKey As String, level As KBotGroupLevel)
        Try
            Column(colKey)                      ' cheie necunoscută => ArgumentException
            Dim idx As Integer = -1
            For i As Integer = 0 To _levels.Count - 1
                If _levels(i) IsNot Nothing AndAlso
                   String.Equals(_levels(i).ColumnKey, colKey, StringComparison.Ordinal) Then
                    idx = i
                    Exit For
                End If
            Next

            If level Is Nothing Then
                If idx >= 0 Then _levels.RemoveAt(idx)
                Return
            End If

            level.ColumnKey = colKey
            If idx >= 0 Then
                _levels(idx) = level
            Else
                _levels.Add(level)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.SetColumnGroupLevel", ex)
            Throw
        End Try
    End Sub

    ' ── Legătura colecție -> control ────────────────────────────────────────────

    ''' <summary>
    ''' Chemată de <see cref="KBotGroupLevelCollection"/> și de nivelurile ei după o schimbare
    ''' STRUCTURALĂ (altă coloană, alt sens, altă bandă, altă înălțime): se reface și harta de
    ''' vedere — cheile de grupare intră în sortare — și tot tabloul de benzi.
    ''' </summary>
    Friend Sub OnGroupLevelsChanged()
        Try
            RefreshActiveLevels()
            If _initializing Then Return
            AfterViewStateChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnGroupLevelsChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Chemată după o schimbare doar de ASPECT (culoare, font de subsol, șablon de titlu,
    ''' retragere): benzile rămân unde sunt, se repictează doar.
    ''' </summary>
    Friend Sub OnGroupLevelAppearanceChanged()
        Try
            If _initializing Then Return
            InvalidateContent()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnGroupLevelAppearanceChanged", ex)
        End Try
    End Sub

    ' Reface lista nivelurilor ACTIVE. Un nivel fără cheie se sare tăcut (designerul inserează un
    ' element gol la «Add»); unul cu o cheie necunoscută se sare AICI și se reclamă zgomotos la
    ' EndInit — o pictare n-are cum să arunce pentru o greșeală de model.
    Private Sub RefreshActiveLevels()
        _activeLevels.Clear()
        For Each nivel In _levels
            If nivel Is Nothing OrElse Not nivel.IsActive Then Continue For
            If Not _columnIndex.ContainsKey(nivel.ColumnKey) Then Continue For
            _activeLevels.Add(nivel)
        Next
    End Sub

    ''' <summary>
    ''' Verificarea zgomotoasă de la <c>EndInit</c>: un nivel care arată către o coloană
    ''' inexistentă e o greșeală de model, nu o stare intermediară de tastare.
    ''' </summary>
    Friend Sub ValidateGroupLevels()
        For i As Integer = 0 To _levels.Count - 1
            Dim nivel As KBotGroupLevel = _levels(i)
            If nivel Is Nothing OrElse Not nivel.IsActive Then Continue For
            If _columnIndex.ContainsKey(nivel.ColumnKey) Then Continue For
            Throw New ArgumentException(
                $"Nivelul de grupare {i} arată către o coloană inexistentă: '{nivel.ColumnKey}'.", NameOf(Groups))
        Next
    End Sub

    ''' <summary>
    ''' Nivelurile active, în ordine. Friend: le citesc sortarea, construcția benzilor și pictarea
    ''' — o a doua listă ar însemna să se sorteze după alte chei decât cele pe care se rupe.
    ''' </summary>
    Friend Function ActiveLevels() As IReadOnlyList(Of KBotGroupLevel)
        Return _activeLevels
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' CONSTRUCȚIA BENZILOR
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Marchează tabloul de benzi murdar. O cheamă <c>InvalidateView</c>.</summary>
    Friend Sub InvalidateBands()
        _bandsDirty = True
    End Sub

    ''' <summary>
    ''' Se asigură că tabloul de benzi e la zi. O cheamă FIECARE cititor de geometrie. Ieftină cât
    ''' timp nu s-a schimbat nimic, și un no-op complet pe o grilă negrupată.
    ''' </summary>
    Friend Sub EnsureBands()
        EnsureView()                            ' benzile se așază peste harta de vedere
        If Not _bandsDirty Then Return
        RebuildBands()
    End Sub

    ' Reconstruiește grupurile și benzile. PURĂ față de restul controlului (nu invalidează, nu
    ' ridică evenimente, nu atinge selecția), din același motiv ca RebuildView: e citită din
    ' mijlocul unei pictări.
    Private Sub RebuildBands()
        Try
            _groups.Clear()
            _rootGroups.Clear()
            _bandCount = 0
            _contentHeight = 0

            Dim nrVedere As Integer = _view.Count
            If _bandOfView.Length <> nrVedere Then ReDim _bandOfView(Math.Max(0, nrVedere - 1))

            ' Calea negrupată: nicio bandă, nicio alocare, geometria rămâne pe înmulțire.
            If _activeLevels.Count = 0 Then
                For i As Integer = 0 To _bandOfView.Length - 1
                    _bandOfView(i) = -1
                Next
                _contentHeight = nrVedere * _rowHeight
                _bandsDirty = False
                Return
            End If

            BuildGroupTree(nrVedere)
            ApplyCollapseDefaults()
            EmitBands(nrVedere)
            _bandsDirty = False
        Catch ex As Exception
            ' Boundary: tabloul e citit din pictare, deci o excepție aici ar cădea în bucla de
            ' mesaje. Logăm și cădem pe benzi PLATE — grila arată toate rândurile, fără antete de
            ' grup: prea puțină structură se vede, un rând pierdut nu.
            GlobalErrorLog.Write("KBotDataView.RebuildBands", ex)
            FallbackFlatBands()
        End Try
    End Sub

    ' Benzile plate (un rând de date = o bandă), plasa de siguranță de mai sus.
    Private Sub FallbackFlatBands()
        _groups.Clear()
        _rootGroups.Clear()
        Dim n As Integer = _view.Count
        If _bandOfView.Length <> n Then ReDim _bandOfView(Math.Max(0, n - 1))
        EnsureBandCapacity(n)
        Dim y As Integer = 0
        For i As Integer = 0 To n - 1
            _bands(i) = New KBotBand With {.Kind = KBotGroupBandKind.Data, .GroupIndex = -1,
                                           .ViewPosition = i, .Top = y, .Height = _rowHeight}
            _bandOfView(i) = i
            y += _rowHeight
        Next
        _bandCount = n
        _contentHeight = y
        _bandsDirty = False
    End Sub

    ' ── Pasul 1: arborele de grupuri ────────────────────────────────────────────
    '
    ' O singură trecere peste vedere. Se rupe la PRIMUL nivel a cărui cheie s-a schimbat, iar de
    ' acolo în jos totul se închide și se redeschide — de aceea „ianuarie” sub 2024 și „ianuarie”
    ' sub 2025 sunt două grupuri, deși cheia lor de nivel 1 e aceeași.
    Private Sub BuildGroupTree(nrVedere As Integer)
        Dim nrNiveluri As Integer = _activeLevels.Count
        Dim deschise(nrNiveluri - 1) As Integer
        Dim cheiPrecedente(nrNiveluri - 1) As String
        Dim cheiCurente(nrNiveluri - 1) As String
        For d As Integer = 0 To nrNiveluri - 1
            deschise(d) = -1
        Next

        For vp As Integer = 0 To nrVedere - 1
            Dim row As KBotDataRow = _rows(_view(vp))

            Dim divergenta As Integer = nrNiveluri
            For d As Integer = 0 To nrNiveluri - 1
                cheiCurente(d) = GroupKeyText(row, _activeLevels(d))
                If divergenta = nrNiveluri AndAlso
                   (vp = 0 OrElse Not String.Equals(cheiCurente(d), cheiPrecedente(d), StringComparison.Ordinal)) Then
                    divergenta = d
                End If
            Next

            If divergenta < nrNiveluri Then
                ' Se închid grupurile deschise, de la cel mai dinăuntru spre divergență.
                For d As Integer = nrNiveluri - 1 To divergenta Step -1
                    If deschise(d) >= 0 Then
                        _groups(deschise(d)).LastViewPos = vp - 1
                        deschise(d) = -1
                    End If
                Next
                ' Și se deschid cele noi, de la divergență spre înăuntru.
                For d As Integer = divergenta To nrNiveluri - 1
                    Dim parinte As Integer = If(d = 0, -1, deschise(d - 1))
                    Dim nod As New KBotGroupNode With {
                        .Level = d,
                        .Key = cheiCurente(d),
                        .Value = row(_activeLevels(d).ColumnKey),
                        .FirstViewPos = vp,
                        .LastViewPos = vp,
                        .ParentIndex = parinte
                    }
                    nod.Path = If(parinte < 0, nod.Key, _groups(parinte).Path & ChrW(1) & nod.Key)
                    _groups.Add(nod)
                    deschise(d) = _groups.Count - 1
                    If parinte >= 0 Then
                        _groups(parinte).Children.Add(deschise(d))
                    Else
                        _rootGroups.Add(deschise(d))
                    End If
                Next
            End If

            For d As Integer = 0 To nrNiveluri - 1
                cheiPrecedente(d) = cheiCurente(d)
            Next
        Next

        ' Ce a rămas deschis se închide pe ultimul rând.
        For d As Integer = nrNiveluri - 1 To 0 Step -1
            If deschise(d) >= 0 Then _groups(deschise(d)).LastViewPos = nrVedere - 1
        Next
    End Sub

    ''' <summary>
    ''' Cheia pe care se RUPE un grup: textul AFIȘAT al valorii, exact cum îl citește operatorul în
    ''' celulă (aceeași regulă ca la <see cref="KBotAggregate.CountDistinct"/>). Două valori care se
    ''' scriu la fel — două date cu ore diferite sub «dd.MM.yyyy» — sunt o singură zi, ceea ce e
    ''' chiar întrebarea pe care o pune cineva care grupează pe zi.
    '''
    ''' <para>Ordinea, în schimb, o dă valoarea BRUTĂ (vezi sortarea): pe text, «10» ar sta
    ''' înaintea lui «9».</para>
    ''' </summary>
    Private Function GroupKeyText(row As KBotDataRow, nivel As KBotGroupLevel) As String
        Dim col As KBotDataColumn = Nothing
        If Not _columnIndex.TryGetValue(nivel.ColumnKey, col) Then Return String.Empty
        Dim brut As Object = row(nivel.ColumnKey)
        If KBotFilterEngine.IsBlank(brut) Then Return String.Empty
        Return FormatValue(brut, col)
    End Function

    ' Prima construcție a unui grup îi aplică CollapsedByDefault — o singură dată pe cale, ca o
    ' desfacere de-a operatorului să nu fie „corectată” înapoi la următoarea resortare.
    Private Sub ApplyCollapseDefaults()
        For Each nod In _groups
            Dim nivel As KBotGroupLevel = _activeLevels(nod.Level)
            If Not nivel.EffectiveCollapsible Then Continue For
            If Not nivel.CollapsedByDefault Then Continue For
            If Not _defaultsApplied.Add(nod.Path) Then Continue For
            _collapsedPaths.Add(nod.Path)
        Next
    End Sub

    ' ── Pasul 2: benzile ────────────────────────────────────────────────────────

    Private Sub EmitBands(nrVedere As Integer)
        ' Cel mai rău caz: fiecare rând, plus un antet și un subsol pe nivel pentru fiecare grup.
        EnsureBandCapacity(nrVedere + 2 * _groups.Count)
        For i As Integer = 0 To _bandOfView.Length - 1
            _bandOfView(i) = -1
        Next

        Dim y As Integer = 0
        For Each gi In _rootGroups
            EmitGroup(gi, y)
        Next
        _contentHeight = y

        ' Fără niciun grup (vedere goală) rămâne un tablou gol — corect: n-are ce se picta.
    End Sub

    ' Emite un grup și, dacă nu e strâns, tot ce e sub el. Recursivă pe ADÂNCIMEA NIVELURILOR
    ' (câteva), nu pe rânduri. Acoperită tranzitiv de boundary-ul din RebuildBands.
    Private Sub EmitGroup(gi As Integer, ByRef y As Integer)
        Dim nod As KBotGroupNode = _groups(gi)
        Dim nivel As KBotGroupLevel = _activeLevels(nod.Level)

        nod.HeaderBandIndex = -1
        If nivel.ShowHeader Then
            nod.HeaderBandIndex = _bandCount
            AddBand(KBotGroupBandKind.GroupHeader, gi, -1, y, GroupBandHeight(nivel, antet:=True))
        End If

        If IsGroupCollapsedNode(nod) Then Return                ' strâns: doar antetul rămâne

        If nod.Children.Count = 0 Then
            ' Nivelul cel mai dinăuntru: rândurile lui de date.
            For vp As Integer = nod.FirstViewPos To nod.LastViewPos
                _bandOfView(vp) = _bandCount
                AddBand(KBotGroupBandKind.Data, -1, vp, y, _rowHeight)
            Next
        Else
            For Each ci In nod.Children
                EmitGroup(ci, y)
            Next
        End If

        If nivel.ShowFooter Then
            AddBand(KBotGroupBandKind.GroupFooter, gi, -1, y, GroupBandHeight(nivel, antet:=False))
        End If
    End Sub

    Private Sub AddBand(kind As KBotGroupBandKind, groupIndex As Integer, viewPosition As Integer,
                        ByRef y As Integer, height As Integer)
        EnsureBandCapacity(_bandCount + 1)
        _bands(_bandCount) = New KBotBand With {.Kind = kind, .GroupIndex = groupIndex,
                                                .ViewPosition = viewPosition, .Top = y, .Height = height}
        _bandCount += 1
        y += height
    End Sub

    Private Sub EnsureBandCapacity(needed As Integer)
        If _bands.Length >= needed Then Return
        Dim nou As Integer = Math.Max(needed, Math.Max(16, _bands.Length * 2))
        ReDim Preserve _bands(nou - 1)
    End Sub

    ''' <summary>Înălțimea unei benzi de grup: cea cerută pe nivel, sau <c>RowHeight</c>.</summary>
    Private Function GroupBandHeight(nivel As KBotGroupLevel, antet As Boolean) As Integer
        Dim ceruta As Integer = If(antet, nivel.HeaderHeight, nivel.FooterHeight)
        Return If(ceruta > 0, ceruta, _rowHeight)
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' GEOMETRIE — indici de bandă
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Câte benzi se desenează (rânduri de date + antete + subsoluri de grup).</summary>
    Friend Function BandCount() As Integer
        EnsureBands()
        If _activeLevels.Count = 0 Then Return _view.Count
        Return _bandCount
    End Function

    ''' <summary>
    ''' Înălțimea totală a conținutului derulabil. UN SINGUR loc — bara de derulare, predicția ei
    ''' din auto-size și <c>EnsureVisible</c> îl citesc pe acesta; două formule ar însemna o bară
    ''' care promite o înălțime pe care grila n-o are.
    ''' </summary>
    Friend Function ContentHeight() As Integer
        EnsureBands()
        Return _contentHeight
    End Function

    ''' <summary>Banda de la un index (Kind = <c>Data</c> cu ViewPosition = -1 dacă indexul e greșit).</summary>
    Friend Function BandAt(index As Integer) As KBotBand
        EnsureBands()
        If _activeLevels.Count = 0 Then
            ' Negrupat: benzile sunt rândurile de date, calculate pe loc (fără tablou).
            If index < 0 OrElse index >= _view.Count Then
                Return New KBotBand With {.Kind = KBotGroupBandKind.Data, .GroupIndex = -1, .ViewPosition = -1}
            End If
            Return New KBotBand With {.Kind = KBotGroupBandKind.Data, .GroupIndex = -1, .ViewPosition = index,
                                      .Top = index * _rowHeight, .Height = _rowHeight}
        End If
        If index < 0 OrElse index >= _bandCount Then
            Return New KBotBand With {.Kind = KBotGroupBandKind.Data, .GroupIndex = -1, .ViewPosition = -1}
        End If
        Return _bands(index)
    End Function

    ''' <summary>
    ''' Indexul benzii care conține un offset Y din CONȚINUT (nu din client), sau -1.
    ''' Negrupat: o împărțire, ca înainte. Grupat: căutare binară pe offset-urile cumulate — de
    ''' aceea benzile pot avea înălțimi diferite fără ca virtualizarea să plătească pentru asta.
    ''' </summary>
    Friend Function BandIndexAtOffset(offsetY As Integer) As Integer
        EnsureBands()
        If offsetY < 0 Then Return -1

        If _activeLevels.Count = 0 Then
            If _rowHeight <= 0 Then Return -1
            Dim idx As Integer = offsetY \ _rowHeight
            Return If(idx >= _view.Count, -1, idx)
        End If

        If _bandCount = 0 OrElse offsetY >= _contentHeight Then Return -1
        Dim lo As Integer = 0
        Dim hi As Integer = _bandCount - 1
        While lo <= hi
            Dim mid As Integer = lo + (hi - lo) \ 2
            Dim b As KBotBand = _bands(mid)
            If offsetY < b.Top Then
                hi = mid - 1
            ElseIf offsetY >= b.Top + b.Height Then
                lo = mid + 1
            Else
                Return mid
            End If
        End While
        Return -1
    End Function

    ''' <summary>
    ''' Indexul benzii unui rând de date dat prin poziția lui de vedere, sau -1 dacă rândul e
    ''' într-un grup STRÂNS (deci nu se desenează nicăieri).
    ''' </summary>
    Friend Function BandIndexOfViewPosition(viewPosition As Integer) As Integer
        EnsureBands()
        If viewPosition < 0 OrElse viewPosition >= _view.Count Then Return -1
        If _activeLevels.Count = 0 Then Return viewPosition
        If viewPosition >= _bandOfView.Length Then Return -1
        Return _bandOfView(viewPosition)
    End Function

    ''' <summary>Grupul de la un index, sau <c>Nothing</c>.</summary>
    Friend Function GroupAt(groupIndex As Integer) As KBotGroupNode
        If groupIndex < 0 OrElse groupIndex >= _groups.Count Then Return Nothing
        Return _groups(groupIndex)
    End Function

    ''' <summary>
    ''' Rândul (index de MODEL) se DESENEAZĂ undeva? Adică trece de filtre ȘI nu stă într-un grup
    ''' strâns. Cele două motive sunt diferite — vezi <c>DropSelectionIfHidden</c> pentru de ce
    ''' NU se tratează la fel — dar pentru desen, editare și etichetă răspunsul e același.
    ''' </summary>
    Friend Function RowIsOnScreen(modelIndex As Integer) As Boolean
        Dim vp As Integer = ViewPositionOf(modelIndex)
        If vp < 0 Then Return False
        Return BandIndexOfViewPosition(vp) >= 0
    End Function

    ''' <summary>
    ''' ANCORA de navigare a unui rând: banda lui, iar dacă e închis într-un grup strâns, banda de
    ''' ANTET a celui mai din AFARĂ grup strâns care îl ascunde. -1 = rândul e filtrat afară, deci
    ''' nu are nicio ancoră.
    '''
    ''' <para>De aici vine purtarea care face strângerea reversibilă de la tastatură: un rând
    ''' selectat și apoi închis nu dispare din model, ci se sprijină pe antetul grupului. Săgeata în
    ''' jos pleacă de la acel antet — deci sare peste tot grupul, cum face și ochiul — iar
    ''' <c>EnsureVisible</c> derulează la antet, nu la un rând care nu se desenează.</para>
    ''' </summary>
    Friend Function AnchorBandOfRow(modelIndex As Integer) As Integer
        Dim vp As Integer = ViewPositionOf(modelIndex)
        If vp < 0 Then Return -1
        Dim bi As Integer = BandIndexOfViewPosition(vp)
        If bi >= 0 Then Return bi

        ' Rândul e ascuns: se urcă până la cel mai din afară strămoș STRÂNS — el e cel care se
        ' vede efectiv pe ecran, ceilalți sunt și ei închiși înăuntrul lui.
        Dim ascuns As Integer = -1
        Dim gi As Integer = InnermostGroupIndexAt(vp)
        While gi >= 0
            If IsGroupCollapsedNode(_groups(gi)) Then ascuns = gi
            gi = _groups(gi).ParentIndex
        End While
        If ascuns < 0 Then Return -1
        Return _groups(ascuns).HeaderBandIndex
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' STRÂNGEREA GRUPURILOR
    ' ══════════════════════════════════════════════════════════════════════════

    ' Nodul e strâns? Un nivel care nu se poate strânge nu se uită deloc în mulțimea de căi —
    ' altfel o cale rămasă acolo dintr-o configurație veche ar închide un grup pe care operatorul
    ' n-are cum să-l redeschidă.
    Private Function IsGroupCollapsedNode(nod As KBotGroupNode) As Boolean
        If nod Is Nothing Then Return False
        If Not _activeLevels(nod.Level).EffectiveCollapsible Then Return False
        Return _collapsedPaths.Contains(nod.Path)
    End Function

    ''' <summary>Banda dată e un antet de grup care se poate strânge? (Cursorul de mână o citește.)</summary>
    Friend Function GroupBandIsCollapsible(bandIndex As Integer) As Boolean
        Dim b As KBotBand = BandAt(bandIndex)
        If b.Kind <> KBotGroupBandKind.GroupHeader Then Return False
        Dim nod As KBotGroupNode = GroupAt(b.GroupIndex)
        If nod Is Nothing Then Return False
        Return _activeLevels(nod.Level).EffectiveCollapsible
    End Function

    ''' <summary>Grupul de la un index de bandă de ANTET e strâns? (False pentru orice altă bandă.)</summary>
    Friend Function IsBandCollapsed(bandIndex As Integer) As Boolean
        Dim b As KBotBand = BandAt(bandIndex)
        If b.Kind <> KBotGroupBandKind.GroupHeader Then Return False
        Return IsGroupCollapsedNode(GroupAt(b.GroupIndex))
    End Function

    ''' <summary>
    ''' Strânge sau desface grupul căruia îi aparține banda dată. Întoarce True dacă starea chiar
    ''' s-a schimbat. O bandă care nu e antet de grup (sau al cărei nivel nu se poate strânge) =
    ''' no-op tăcut: e apăsarea unui buton care nu există, nu o cerere din cod.
    ''' </summary>
    Friend Function ToggleBandCollapse(bandIndex As Integer) As Boolean
        Try
            Dim b As KBotBand = BandAt(bandIndex)
            If b.Kind <> KBotGroupBandKind.GroupHeader Then Return False
            Dim nod As KBotGroupNode = GroupAt(b.GroupIndex)
            If nod Is Nothing OrElse Not _activeLevels(nod.Level).EffectiveCollapsible Then Return False
            Return SetPathCollapsed(nod.Path, Not _collapsedPaths.Contains(nod.Path))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ToggleBandCollapse", ex)
            Throw
        End Try
    End Function

    ' Punctul UNIC prin care se schimbă starea unui grup: reface benzile, verifică selecția și
    ' ridică evenimentul o singură dată, doar la o schimbare reală.
    Private Function SetPathCollapsed(path As String, collapsed As Boolean) As Boolean
        Dim schimbat As Boolean = If(collapsed, _collapsedPaths.Add(path), _collapsedPaths.Remove(path))
        If Not schimbat Then Return False
        AfterCollapseChanged()
        Return True
    End Function

    ' Benzile s-au rearanjat, deci și înălțimea conținutului și barele de derulare.
    '
    ' Selecția NU cade — vezi DropSelectionIfHidden pentru de ce strângerea și filtrarea nu se
    ' tratează la fel. Editarea deschisă, în schimb, se abandonează: un editor real care plutește
    ' peste un rând tocmai închis ar rămâne agățat în aer, exact ca la derulare.
    Private Sub AfterCollapseChanged()
        InvalidateBands()
        If _editing AndAlso Not RowIsOnScreen(_currentRowIndex) Then CancelEdit()
        LayoutChanged()
        RaiseEvent GroupCollapsedChanged(Me, EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Grupul cel mai DINĂUNTRU care conține un rând (dat prin poziția lui de vedere), sau -1.
    ''' Căutarea merge pe rădăcini și coboară: grupurile unui nivel sunt intervale disjuncte, deci
    ''' la fiecare pas există cel mult un copil care poate conține poziția.
    ''' </summary>
    Friend Function InnermostGroupIndexAt(viewPosition As Integer) As Integer
        EnsureBands()
        Dim gasit As Integer = -1
        Dim candidati As IEnumerable(Of Integer) = _rootGroups
        Do
            Dim urmator As Integer = -1
            For Each gi In candidati
                Dim nod As KBotGroupNode = _groups(gi)
                If viewPosition >= nod.FirstViewPos AndAlso viewPosition <= nod.LastViewPos Then
                    urmator = gi
                    Exit For
                End If
            Next
            If urmator < 0 Then Exit Do
            gasit = urmator
            candidati = _groups(urmator).Children
        Loop While True
        Return gasit
    End Function

    ''' <summary>
    ''' Strânge sau desface grupul care conține rândul dat (index de MODEL). Urcă la primul
    ''' strămoș care CHIAR se poate strânge, ca tasta să facă ceva și când nivelul de dedesubt e
    ''' pornit fără antet. Întoarce True dacă starea s-a schimbat.
    ''' </summary>
    Friend Function SetGroupCollapsedForRow(modelIndex As Integer, collapsed As Boolean) As Boolean
        Try
            Dim vp As Integer = ViewPositionOf(modelIndex)
            If vp < 0 Then Return False
            Dim gi As Integer = InnermostGroupIndexAt(vp)
            While gi >= 0
                Dim nod As KBotGroupNode = _groups(gi)
                If _activeLevels(nod.Level).EffectiveCollapsible Then
                    Return SetPathCollapsed(nod.Path, collapsed)
                End If
                gi = nod.ParentIndex
            End While
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.SetGroupCollapsedForRow", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Strânge toate grupurile (opțional, doar cele de pe un nivel). Nivelurile care nu se pot
    ''' strânge se sar.
    ''' </summary>
    Public Sub CollapseAllGroups(Optional level As Integer = -1)
        Try
            SetAllGroupsCollapsed(True, level)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.CollapseAllGroups", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Desface toate grupurile (opțional, doar cele de pe un nivel).</summary>
    Public Sub ExpandAllGroups(Optional level As Integer = -1)
        Try
            SetAllGroupsCollapsed(False, level)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ExpandAllGroups", ex)
            Throw
        End Try
    End Sub

    Private Sub SetAllGroupsCollapsed(collapsed As Boolean, level As Integer)
        EnsureBands()
        Dim schimbat As Boolean = False
        For Each nod In _groups
            If level >= 0 AndAlso nod.Level <> level Then Continue For
            If Not _activeLevels(nod.Level).EffectiveCollapsible Then Continue For
            ' Căile primesc și marcajul de „implicit aplicat”: altfel o desfacere generală ar fi
            ' anulată de CollapsedByDefault la prima reconstrucție de după ea.
            _defaultsApplied.Add(nod.Path)
            If collapsed Then
                If _collapsedPaths.Add(nod.Path) Then schimbat = True
            Else
                If _collapsedPaths.Remove(nod.Path) Then schimbat = True
            End If
        Next
        If schimbat Then AfterCollapseChanged()
    End Sub

    ''' <summary>
    ''' Câte grupuri are grila pe un nivel (sau pe toate, cu <c>-1</c>). Poartă de verificare
    ''' pentru gazdă și pentru teste — headless, fără pictare.
    ''' </summary>
    Public Function GroupCount(Optional level As Integer = -1) As Integer
        Try
            EnsureBands()
            If level < 0 Then Return _groups.Count
            Dim n As Integer = 0
            For Each nod In _groups
                If nod.Level = level Then n += 1
            Next
            Return n
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.GroupCount", ex)
            Throw
        End Try
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' AGREGATELE UNUI GRUP
    ' ══════════════════════════════════════════════════════════════════════════

    ' ══════════════════════════════════════════════════════════════════════════
    ' PORȚI DE VERIFICARE (Friend — headless, fără pictare)
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Benzile, scrise scurt: <c>H0</c> = antet de nivel 0, <c>F1</c> = subsol de nivel 1,
    ''' <c>D</c> = rând de date. O grilă grupată pe o coloană cu două valori dă, de pildă,
    ''' <c>«H0 D D F0 H0 D F0»</c> — adică exact ce se vede pe ecran, într-un singur șir care se
    ''' poate pune într-un Assert. Structura benzilor e altfel invizibilă până la pictare.
    ''' </summary>
    Friend Function DebugBandSummary() As String
        EnsureBands()
        Dim bucati As New List(Of String)()
        For i As Integer = 0 To BandCount() - 1
            Dim b As KBotBand = BandAt(i)
            Select Case b.Kind
                Case KBotGroupBandKind.Data
                    bucati.Add("D")
                Case KBotGroupBandKind.GroupHeader
                    bucati.Add("H" & GroupAt(b.GroupIndex).Level.ToString(Globalization.CultureInfo.InvariantCulture))
                Case Else
                    bucati.Add("F" & GroupAt(b.GroupIndex).Level.ToString(Globalization.CultureInfo.InvariantCulture))
            End Select
        Next
        Return String.Join(" ", bucati)
    End Function

    ''' <summary>Poartă de verificare: agregatul unei coloane pentru grupul de la un index.</summary>
    Friend Function DebugGroupAggregate(groupIndex As Integer, colKey As String) As String
        EnsureBands()
        Dim nod As KBotGroupNode = GroupAt(groupIndex)
        If nod Is Nothing Then Return String.Empty
        Dim col As KBotDataColumn = Nothing
        If Not _columnIndex.TryGetValue(colKey, col) Then Return String.Empty
        Return GroupAggregateText(nod, col)
    End Function

    ''' <summary>Poartă de verificare: titlul compus al unei benzi de grup.</summary>
    Friend Function DebugGroupCaption(groupIndex As Integer, antet As Boolean) As String
        EnsureBands()
        Dim nod As KBotGroupNode = GroupAt(groupIndex)
        If nod Is Nothing Then Return String.Empty
        Return GroupCaptionFor(nod, _activeLevels(nod.Level), antet)
    End Function

    ''' <summary>Poartă de verificare: câte rânduri de date are grupul de la un index.</summary>
    Friend Function DebugGroupRowCount(groupIndex As Integer) As Integer
        EnsureBands()
        Dim nod As KBotGroupNode = GroupAt(groupIndex)
        Return If(nod Is Nothing, 0, nod.RowCount)
    End Function

    ''' <summary>
    ''' Poartă de verificare: comută strângerea grupului de la un index (testele n-au bandă la
    ''' îndemână, iar apăsarea reală cere o buclă de mesaje).
    ''' </summary>
    Friend Function DebugToggleGroup(groupIndex As Integer) As Boolean
        EnsureBands()
        Dim nod As KBotGroupNode = GroupAt(groupIndex)
        If nod Is Nothing OrElse Not _activeLevels(nod.Level).EffectiveCollapsible Then Return False
        Return SetPathCollapsed(nod.Path, Not _collapsedPaths.Contains(nod.Path))
    End Function

    ''' <summary>
    ''' Textul agregat al unei coloane PENTRU UN GRUP, cache-uit leneș pe nod.
    '''
    ''' <para>Leneș, nu la reconstrucție, și asta e o hotărâre: o grilă cu zece mii de grupuri și
    ''' cinci coloane agregate ar plăti cincizeci de mii de treceri la fiecare resortare, ca să
    ''' arate douăzeci de benzi. Se calculează ce se pictează, o singură dată, iar cache-ul moare
    ''' odată cu nodul (adică la orice schimbare de model, filtru sau sortare).</para>
    ''' </summary>
    Friend Function GroupAggregateText(nod As KBotGroupNode, col As KBotDataColumn) As String
        If nod Is Nothing OrElse col Is Nothing OrElse String.IsNullOrEmpty(col.Key) Then Return String.Empty
        Dim t As String = Nothing
        If nod.Aggregates.TryGetValue(col.Key, t) Then Return t
        t = ComputeAggregateText(col, nod.FirstViewPos, nod.LastViewPos)
        nod.Aggregates(col.Key) = t
        Return t
    End Function

End Class
