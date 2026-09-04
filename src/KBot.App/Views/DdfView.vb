Option Strict On
Imports System.Globalization
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Vederea DDF (felia 0020) — echivalentul Access frmFX_MAIN_DDF: un arbore de revizii pe
''' DOUĂ niveluri la stânga (lună/an -> revizie) și, la dreapta, o sub-navigare ORIZONTALĂ
''' (decizia 8) cu paginile «Vizualizare» (felia 03), «Document» (PDF-ul real) și «Fișiere»
''' (felia 04). Read-only.
'''
''' Datele vin dintr-un SINGUR apel GET /api/forexe/ddf pentru tot CodAngajament-ul, prin
''' plasa de re-autentificare a shell-ului (401 -> re-login -> reia o dată). Un click în
''' arbore FILTREAZĂ datele deja încărcate — nu declanșează nicio cerere de rețea
''' (decizia 7).
'''
''' FELIA 0032 — SUB-PAGINI: cele patru pagini nu mai sunt panouri în acest designer, ci
''' UserControl-uri separate în <c>Views\Ddf\</c>, fiecare cu designerul lui. Găzduirea copiază
''' tiparul leneș din <c>MainForm.ActivateView</c>: pagina se creează la prima selecție din
''' <c>navSub</c>, se andochează în <c>pnlPages</c> și doar cea activă e vizibilă.
'''
''' Diferența față de MainForm: vederile shell-ului își aduc SINGURE datele, sub-paginile de aici
''' NU. Ele n-au nici client API, nici sesiune — primesc un <see cref="DdfPageContext"/> de la
''' această vedere. Altfel decizia 7 (o singură cerere pe CodAngajament) s-ar rupe: patru pagini
''' cu DI proprie ar însemna patru încărcări.
'''
''' ABATERI DELIBERATE de la Access (motivate în worklog-ul feliei):
'''   * valoarea unei frunze este SUM(ValCur) peste revizie, calculat pe server —
'''     Access aliază `SA.ValCur AS TotalRevizie` și afișează o linie ARBITRARĂ;
'''   * valoarea unei rădăcini de lună este suma reală a frunzelor ei — Access trimite
'''     literalul `0` în AddTree_Root;
'''   * o rădăcină e roșie doar când PROPRIUL ei total e negativ — Access copiază culoarea
'''     ultimei frunze procesate în părinte (`cRoot.foreColor = cNode.foreColor`), ceea ce
'''     face culoarea rădăcinii să depindă de ordinea de parcurgere. Accidental, nu intenționat.
''' </summary>
Public Class DdfView
    Implements IAngajamentView, IThemedControl

    ' Cheile paginilor sub-navigării — o singură definiție, folosită la creare și la comutare.
    '
    ' «valori» e PARCATĂ (decizie de operator, felia 0025, dusă mai departe în 0032): nu are
    ' intrare în navigație, deci pagina nu se activează și nici măcar nu se construiește. Codul ei
    ' rămâne viu în <c>DdfValoriPage</c> și are deja un caz în <c>CreatePage</c>.
    '
    ' CA SĂ O ADUCI ÎNAPOI: în designerul acestei vederi, adaugă în `navSub.Items` un element cu
    ' Key = "valori", Text = "Valori", pe prima poziție. Atât — restul e deja pe loc. (Opțional:
    ' schimbă selecția inițială din `BuildNav` înapoi pe PAGE_VALORI.)
    Private Const PAGE_VALORI As String = "valori"
    Private Const PAGE_PREVIEW As String = "previzualizare"
    ' «Document» = PDF-ul REAL (ReaderHostPreview), distinct de «Vizualizare» (reconstrucția XFA).
    Private Const PAGE_PDF As String = "document"
    Private Const PAGE_FISIERE As String = "fisiere"

    ' CHEILE ICONIȚELOR din «image_list» (ImageList-ul pus pe vedere în designer și legat
    ' de arbore prin tree.NodeImages). Felia 0033 §12 (cererea operatorului): arborele DDF
    ' rezolvă iconițele ÎNTÂI din listă, exact ca RezervariView, și abia dacă lista n-are cheia
    ' cade înapoi pe formele GDI din DdfIcons. Cum se scapă de fallback: pui pozele în listă.
    ' Căutarea cheii e insensibilă la litere mari/mici (ImageList.IndexOfKey), deci «Up» din
    ' listă răspunde și la «up».
    Private Const ICO_MONTH As String = "month"      ' rădăcină de lună (închis sau deschis)
    Private Const ICO_SUS As String = "up"        ' revizie încărcată ▲
    Private Const ICO_JOS As String = "down"      ' revizie preluată ▼
    Private Const ICO_NEUTRU As String = "neutral"

    ' Format românesc: separator de mii «.» și zecimală «,» (1.091.940,00).
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    ' Plasa 401 a shell-ului (MainForm.WithReauth), specializată pe DdfInfo.
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of DdfInfo)), Task(Of DdfInfo))
    ' Sesiunea (globalii unității pentru constructorul de XML, felia 05). Poate fi Nothing în
    ' teste — atunci contextul de generare e gol.
    Private ReadOnly _session As SessionContext

    ' Sub-paginile create până acum (leneș, la prima activare) și cea vizibilă acum.
    Private ReadOnly _pages As New Dictionary(Of String, IDdfPage)(StringComparer.Ordinal)
    Private _activePage As IDdfPage
    ' Contextul împins paginilor: reconstruit la fiecare schimbare de nod și după o generare.
    Private _currentCtx As DdfPageContext

    ' Codul angajamentului CERUT ultima dată — stale-guard (identic cu Plăți/Recepții/Rezervări).
    Private _requestedCod As String
    ' IDDF preferat, din nodul de arbore al shell-ului — alege antetul când sunt mai multe.
    Private _preferredIddf As Integer

    ' Ultimele date încărcate — păstrate ca ApplyTheme să reconstruiască arborele
    ' (re-tintarea iconițelor) fără o nouă cerere de rețea.
    Private _revizii As List(Of RevizieRow)
    Private _liniiByRev As Dictionary(Of Integer, List(Of LinieSaRow))
    ' Antetul de lucru — poartă CUAL / PartAng / NumePartener pentru calea PDF (feliile 03-04).
    Private _antet As DdfAntet

    ' Rândurile nodului selectat acum — sursa grilei paginii «Valori».
    Private _nodeRows As List(Of LinieSaRow)
    ' Nodul selectat e o rădăcină de lună? O lună NU are un singur document, deci nu are cale PDF.
    Private _nodeIsRoot As Boolean
    ' Revizia frunzei selectate acum — ținta generării (felia 05) și sursa căii PDF.
    Private _selectedRevizie As RevizieRow
    ' Calea REZOLVATĂ a documentului nodului curent (felia 0041): fie fișierul semnat adus în
    ' cache de pe server, fie cel NESEMNAT regenerat în zona de lucru. Gol până când una din
    ' cele două se întâmplă — de aceea nu se mai compune calea direct în BuildCurrentContext.
    Private _pdfPathRezolvat As String
    ' Revizia pentru care s-a rezolvat calea de mai sus — stale-guard: un răspuns care sosește
    ' după ce operatorul a dat click pe alt nod se aruncă, exact ca stale-guard-ul pe _requestedCod.
    Private _pdfRezolvatPentruIdrev As Integer
    ' Fișierul ales din lista paginii «Fișiere», care ÎNLOCUIEȘTE calea calculată din revizie
    ' până la următorul click în arbore. Fără el, alegerea unui fișier n-ar avea cum să ajungă la
    ' pagina «Document»: contextul se compune din revizia selectată, nu din listă.
    Private _pdfPathOverride As String
    ' O generare e în curs? Blochează re-invocarea butonului.
    Private _generating As Boolean

    ' Starea splitter-ului dinainte de strângerea arborelui, ca desfacerea să-l pună înapoi
    ' exact unde era (vezi Tree_CollapsedChanged). 0 = arborele n-a fost încă strâns.
    Private _splitterDistanceDesfasurat As Integer
    Private _panel1MinSizeDesfasurat As Integer

    ''' <summary>
    ''' The write commands of slice 0051, executed by the shell. Optional: a host that does
    ''' not supply it (the tests) still gets the full read-only view, and the menu says so
    ''' rather than doing nothing.
    ''' </summary>
    Private ReadOnly _executaComanda As Action(Of DdfComanda)

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of DdfInfo)), Task(Of DdfInfo)),
                   Optional session As SessionContext = Nothing,
                   Optional executaComanda As Action(Of DdfComanda) = Nothing)
        ArgumentNullException.ThrowIfNull(apiClient)
        ArgumentNullException.ThrowIfNull(withReauth)
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        _session = session
        _executaComanda = executaComanda
        BuildNav()
        ShowEmpty("Selectați un angajament din arbore.")
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "ddf"
        End Get
    End Property

    ' ── Găzduirea sub-paginilor ──────────────────────────────────────────────
    ' Sub-navigarea orizontală: intrările sunt AUTORITE ÎN DESIGNER, în `navSub.Items` (felia
    ' 0025). Aici rămâne doar selecția inițială: atribuirea e cea care ridică SelectionChanged
    ' și, prin ea, ActivatePage arată prima pagină. NU se activează a doua oară de mână — ar rula
    ' DUPĂ eveniment și ar ascunde exact pagina tocmai arătată.
    '
    ' Designer-ul scrie cheile ca LITERALE (nu poate referi constantele private de mai sus), deci
    ' cele două trebuie să rămână în acord. Dacă se desincronizează, atribuirea de mai jos aruncă
    ' ArgumentException pe cheie necunoscută — zgomotos, nu tăcut.
    Private Sub BuildNav()
        Try
            navSub.SelectedKey = PAGE_PREVIEW   ' pagina implicită de la parcarea lui «valori»
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.BuildNav", ex)
            Throw
        End Try
    End Sub

    Private Sub NavSub_SelectionChanged(key As String) Handles navSub.SelectionChanged
        Try
            ActivatePage(key)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.NavSub_SelectionChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Arată pagina cerută, creând-o la prima activare — același tipar leneș ca
    ''' <c>MainForm.ActivateView</c>. O singură pagină vizibilă odată; contextul curent se împinge
    ''' abia acum, deci o pagină creată târziu nu pierde selecția făcută înainte de ea.
    ''' </summary>
    Private Sub ActivatePage(key As String)
        Try
            Dim page As IDdfPage = Nothing
            If Not _pages.TryGetValue(key, page) Then
                page = CreatePage(key)
                Dim ctrl As Control = DirectCast(page, Control)
                ctrl.Dock = DockStyle.Fill
                ctrl.Visible = False
                pnlPages.Controls.Add(ctrl)
                ThemeManager.Apply(ctrl)
                _pages(key) = page
            End If

            Dim previous As IDdfPage = _activePage
            _activePage = page
            DirectCast(page, Control).Visible = True
            If previous IsNot Nothing AndAlso Not ReferenceEquals(previous, page) Then
                DirectCast(previous, Control).Visible = False
            End If
            ' Doar pagina ACTIVĂ primește contextul; celelalte îl primesc la activare.
            page.SetContext(_currentCtx)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.ActivatePage", ex)
            Throw
        End Try
    End Sub

    Private Function CreatePage(key As String) As IDdfPage
        Try
            Dim page As IDdfPage
            Select Case key
                Case PAGE_VALORI : page = New DdfValoriPage()   ' parcată: fără intrare în navSub
                Case PAGE_PREVIEW : page = New DdfVizualizarePage()
                Case PAGE_PDF : page = New DdfDocumentPage()
                Case PAGE_FISIERE : page = New DdfFisierePage()
                Case Else
                    Throw New ArgumentException($"Pagină DDF necunoscută: '{key}'.", NameOf(key))
            End Select
            ' Abonare UNIFORMĂ: paginile care n-au ce ridica pur și simplu nu ridică niciodată.
            AddHandler page.GenerateRequested, AddressOf OnGenerateRequested
            AddHandler page.FileActivated, AddressOf OnFileActivated
            Return page
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.CreatePage", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Contextul nodului selectat acum. <c>Nothing</c> cât timp nu s-a încărcat niciun
    ''' angajament. Calea PDF există DOAR pentru o frunză (o lună nu are un singur document) sau
    ''' când operatorul a ales un fișier din listă — atunci fișierul ales are întâietate.
    ''' </summary>
    Private Function BuildCurrentContext() As DdfPageContext
        Try
            If String.IsNullOrWhiteSpace(_requestedCod) OrElse _revizii Is Nothing Then Return Nothing

            ' FELIA 0041 — de unde vine calea, în ordinea de precedență:
            '   1. fișierul ales explicit din lista paginii «Fișiere» (are întâietate mereu);
            '   2. calea REZOLVATĂ de EnsureSignedPdfAsync pentru revizia selectată — fie
            '      cache-ul semnat validat prin sumă, fie documentul regenerat în zona de lucru;
            '   3. calea AȘTEPTATĂ a cache-ului semnat, cât timp rezolvarea încă nu s-a întors:
            '      dacă fișierul e deja acolo, se arată imediat, fără să pâlpâie ecranul gol.
            Dim pdfPath As String = Nothing
            If Not String.IsNullOrEmpty(_pdfPathOverride) Then
                pdfPath = _pdfPathOverride
            ElseIf Not _nodeIsRoot AndAlso _selectedRevizie IsNot Nothing AndAlso _antet IsNot Nothing Then
                If Not String.IsNullOrEmpty(_pdfPathRezolvat) AndAlso
                   _pdfRezolvatPentruIdrev = _selectedRevizie.Idrev Then
                    pdfPath = _pdfPathRezolvat
                Else
                    pdfPath = DdfPdfLocator.ExpectedPath(KBotPaths.Current.DdfPdfRoot, _antet, _selectedRevizie.NumarRev)
                End If
            End If
            Dim exists As Boolean = Not String.IsNullOrEmpty(pdfPath) AndAlso IO.File.Exists(pdfPath)

            ' Globalii unității pentru antetul paginii «Vizualizare». Sesiunea poate lipsi (teste)
            ' -> antetul își sare rândurile goale, ca în XfaXmlPreview.
            Return New DdfPageContext(_antet, _nodeRows, _revizii, _nodeIsRoot, _selectedRevizie,
                                      _requestedCod, pdfPath, exists,
                                      If(_session Is Nothing, String.Empty, _session.NumeUnitate),
                                      If(_session Is Nothing, String.Empty, _session.CF))
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.BuildCurrentContext", ex)
            Throw
        End Try
    End Function

    ' Reconstruiește contextul și îl împinge paginii active. Singura cale de randare a paginilor —
    ' aceeași pentru un click în arbore, un fișier ales din listă și sfârșitul unei generări.
    Private Sub PushToActivePage()
        _currentCtx = BuildCurrentContext()
        _activePage?.SetContext(_currentCtx)
    End Sub

    ' ── Contextul shell-ului ─────────────────────────────────────────────────
    ''' <summary>
    ''' Selecția din arbore s-a schimbat. Fără angajament (nod de capitol / deselectare) sau
    ''' fără DDF (<c>AreDDF = False</c>) NU se face niciun apel de rețea — doar se golește vederea.
    ''' </summary>
    Public Sub SetContext(info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        Try
            Dim cod As String = info?.CodAngajament
            If String.IsNullOrWhiteSpace(cod) Then
                ClearAll()
                ShowEmpty("Selectați un angajament din arbore.")
                Return
            End If
            If Not info.AreDDF Then
                ' Intrarea de navigare e deja ascunsă de shell; aici doar nu cerem nimic.
                ClearAll()
                ShowEmpty("Angajamentul nu are document de fundamentare.")
                Return
            End If

            _requestedCod = cod
            _preferredIddf = If(info.IDDF.HasValue, CInt(info.IDDF.Value), 0)
            ShowEmpty("Se încarcă documentul de fundamentare…")
            ' Fire-and-forget deliberat (handler sincron al shell-ului): metoda își tratează
            ' singură TOATE erorile — vezi comentariul din PlatiView/ReceptiiView.
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Boundary UI: logăm și ARĂTĂM eroarea, nu o aruncăm mai departe (apelul e pornit fără
    ' await din SetContext, deci nu există cine să o prindă).
    Private Async Sub LoadAsync(cod As String)
        Try
            Dim data As DdfInfo = Await _withReauth(
                Function() _apiClient.GetDdfAsync(cod, CancellationToken.None)).ConfigureAwait(True)

            ' Răspuns depășit: între timp operatorul a selectat alt angajament. Îl aruncăm.
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return

            Dim revizii As List(Of RevizieRow) =
                If(data Is Nothing, New List(Of RevizieRow)(), data.Revizii)
            If revizii Is Nothing OrElse revizii.Count = 0 Then
                ClearAll()
                ShowEmpty("Angajamentul nu are revizii.")
                Return
            End If

            ' Antetul de lucru: cel cu IDDF-ul nodului de arbore, altfel primul. Nimic tacit —
            ' când sunt mai multe antete o spunem în jurnal (schema le permite, §2.7).
            _antet = data.AntetDeLucru(_preferredIddf)
            If data.Antet IsNot Nothing AndAlso data.Antet.Count > 1 Then
                GlobalErrorLog.Write("DdfView.LoadAsync",
                    New InvalidOperationException(
                        $"Angajamentul {cod} are {data.Antet.Count} antete FX_DDF; " &
                        $"s-a ales IDDF={If(_antet Is Nothing, 0, _antet.Iddf)}."))
            End If

            _revizii = revizii
            _liniiByRev = GroupLinii(data.Linii)
            BuildTree(revizii)

            ' Slice 0051: after a save or a delete, land back on the revision the operator was
            ' working on rather than resetting to "nothing selected". A revision that no
            ' longer exists (it was the one deleted) falls through to the normal path below.
            If _idrevDeReselectat > 0 Then
                Dim tinta As RevizieRow = revizii.FirstOrDefault(Function(r) r.Idrev = _idrevDeReselectat)
                _idrevDeReselectat = 0
                If tinta IsNot Nothing Then
                    _nodeRows = LiniiFor(tinta.Idrev)
                    _nodeIsRoot = False
                    _selectedRevizie = tinta
                    _pdfPathOverride = Nothing
                    _pdfPathRezolvat = Nothing
                    _pdfRezolvatPentruIdrev = 0
                    Dim nod As AdvancedTreeControl.TreeItem = Nothing
                    If _noduriRevizie.TryGetValue(tinta.Idrev, nod) Then tree.SelectAndReveal(nod)
                    PushToActivePage()
                    ShowContent()
                    Return
                End If
            End If

            ' „Nimic selectat" -> grila arată TOATE liniile angajamentului, ca în ReceptiiView
            ' (revizuire operator 2026-08-13). E aceeași vedere ca a unei rădăcini de lună,
            ' doar peste toate reviziile: listă plată, fără un document unic.
            _nodeRows = ToateLiniile(revizii)
            _nodeIsRoot = True
            _selectedRevizie = Nothing
            _pdfPathOverride = Nothing
            _pdfPathRezolvat = Nothing
            _pdfRezolvatPentruIdrev = 0
            PushToActivePage()
            ShowContent()
        Catch ex As ApiException
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("DdfView.LoadAsync", ex)
            ClearAll()
            ShowEmpty(ex.Message)   ' mesaj românesc din câmpul «error» al serverului
        Catch ex As Exception
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("DdfView.LoadAsync", ex)
            ClearAll()
            ShowEmpty("Documentul de fundamentare nu a putut fi încărcat. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' Liniile de secțiune A, grupate pe revizie. O revizie fără linii pur și simplu lipsește
    ' din dicționar -> nodul ei primește o listă goală (rămâne vizibil, cu total 0).
    Private Shared Function GroupLinii(linii As List(Of LinieSaRow)) As Dictionary(Of Integer, List(Of LinieSaRow))
        Dim map As New Dictionary(Of Integer, List(Of LinieSaRow))()
        If linii Is Nothing Then Return map
        For Each l As LinieSaRow In linii
            Dim bucket As List(Of LinieSaRow) = Nothing
            If Not map.TryGetValue(l.Idrev, bucket) Then
                bucket = New List(Of LinieSaRow)()
                map(l.Idrev) = bucket
            End If
            bucket.Add(l)
        Next
        Return map
    End Function

    ''' <summary>
    ''' Liniile TUTUROR reviziilor, în ordinea reviziilor primite de la server — starea
    ''' „nimic selectat" a grilei. Se trece prin <see cref="LiniiFor"/>, deci ia exact
    ''' liniile pe care le acoperă și arborele: o linie cu un IDREV care nu apare printre
    ''' revizii n-are unde să fie văzută, deci n-are ce căuta nici în listă.
    ''' </summary>
    Private Function ToateLiniile(revizii As List(Of RevizieRow)) As List(Of LinieSaRow)
        Dim toate As New List(Of LinieSaRow)()
        If revizii Is Nothing Then Return toate
        For Each r As RevizieRow In revizii
            toate.AddRange(LiniiFor(r.Idrev))
        Next
        Return toate
    End Function

    Private Function LiniiFor(idrev As Integer) As List(Of LinieSaRow)
        Dim bucket As List(Of LinieSaRow) = Nothing
        If _liniiByRev IsNot Nothing AndAlso _liniiByRev.TryGetValue(idrev, bucket) Then Return bucket
        Return New List(Of LinieSaRow)()
    End Function

    ' ── Arborele ─────────────────────────────────────────────────────────────
    ' DOUĂ niveluri: rădăcină de lună (cheia «LA_{yyyy}_{M}», valoarea = suma frunzelor ei)
    ' -> frunză de revizie (cheia «RC_{IDREV}», valoarea = TotalRevizie). Fiecare nod poartă
    ' în Tag liniile pe care le acoperă, ca un click să filtreze grila fără cerere de rețea.
    ' Rădăcinile sunt EXPANDATE (planul §5).
    Private Sub BuildTree(revizii As List(Of RevizieRow))
        Try
            tree.Clear()
            _noduriRevizie.Clear()
            Dim palette As ThemePalette = TryGetPalette()

            Dim monthGroups = revizii.GroupBy(Function(r) MonthKeyOf(r.DataRev)).
                                      OrderBy(Function(g) g.Key)

            For Each mg In monthGroups
                Dim monthRevs As List(Of RevizieRow) = mg.ToList()
                ' Valoarea rădăcinii = suma TOTALURILOR frunzelor ei (Access trimite literalul 0).
                Dim monthSum As Double = monthRevs.Sum(Function(r) r.TotalRevizie)
                Dim monthLines As New List(Of LinieSaRow)()
                For Each r As RevizieRow In monthRevs
                    monthLines.AddRange(LiniiFor(r.Idrev))
                Next

                ' Lista poate purta două poze pentru lună (închis / deschis); când n-are decât
                ' una — sau niciuna — amândouă cad pe aceeași imagine, ca înainte.
                Dim monthIconInchis As Image = LunaIcon(ICO_MONTH, palette)
                Dim monthIconDeschis As Image = LunaIcon(ICO_MONTH, palette)
                Dim monthItem As AdvancedTreeControl.TreeItem =
                    tree.AddItem(MonthKeyText(mg.Key), $"{MonthYearLabel(mg.Key)}~~~{Money(monthSum)}",
                                 pLeftIconClosed:=monthIconInchis, pLeftIconOpen:=monthIconDeschis,
                                 pExpanded:=True)
                monthItem.Tag = New DdfNodeRows(monthLines, isRoot:=True)
                monthItem.Bold = True
                ' Roșu doar când PROPRIUL total e negativ (Access copiază culoarea ultimei frunze).
                If monthSum < 0 AndAlso palette IsNot Nothing Then
                    monthItem.NodeForeColor = palette.ErrorColor
                End If

                ' Frunze de revizie, în ordinea serverului (DataRev, NumarRev).
                For Each r As RevizieRow In monthRevs
                    Dim leafIcon As Image = IconFor(StareOf(r), palette)
                    Dim leafItem As AdvancedTreeControl.TreeItem =
                        tree.AddItem($"RC_{r.Idrev}", $"{r.EtichetaRevizie}~~~{Money(r.TotalRevizie)}",
                                     monthItem, pLeftIconClosed:=leafIcon, pLeftIconOpen:=leafIcon)
                    leafItem.Tag = New DdfNodeRows(LiniiFor(r.Idrev), isRoot:=False, revizie:=r)
                    ' Slice 0051: remembered so `Reincarca` can land back on the revision the
                    ' operator was working on. Read-only behaviour is untouched by this.
                    _noduriRevizie(r.Idrev) = leafItem
                    leafItem.Tooltip = r.DescScurta
                    If r.TotalRevizie < 0 AndAlso palette IsNot Nothing Then
                        leafItem.NodeForeColor = palette.ErrorColor
                    End If
                Next
            Next

            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.BuildTree", ex)
            Throw
        End Try
    End Sub

    ' Click pe orice nod -> se reconstruiește contextul și se împinge paginii active. Fără apel
    ' de rețea (decizia 7).
    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing Then Return
            Dim payload As DdfNodeRows = TryCast(pNode.Tag, DdfNodeRows)
            If payload Is Nothing Then Return

            _nodeRows = payload.Linii
            _nodeIsRoot = payload.IsRoot
            ' Revizia frunzei (Nothing pe o rădăcină) = ținta unei eventuale generări (felia 05)
            ' și sursa căii PDF calculate în BuildCurrentContext.
            _selectedRevizie = payload.Revizie
            ' O selecție nouă în arbore ANULEAZĂ fișierul ales din lista paginii «Fișiere» ȘI
            ' calea rezolvată pentru nodul anterior.
            _pdfPathOverride = Nothing
            _pdfPathRezolvat = Nothing
            _pdfRezolvatPentruIdrev = 0

            PushToActivePage()
            ' Felia 0041: pe o frunză, aducem documentul SEMNAT de pe server (sau confirmăm că
            ' cel din cache e la zi) și abia apoi re-împingem contextul. Fire-and-forget
            ' deliberat, ca LoadAsync: metoda își tratează singură toate erorile.
            EnsureSignedPdfAsync(_selectedRevizie)

            ' Slice 0051: the right button opens the write commands. Wired AFTER the read
            ' path above, so a failure to build the menu cannot stop the selection from
            ' working -- the view stays usable read-only whatever happens here.
            If e IsNot Nothing AndAlso e.Button = MouseButtons.Right Then
                AratatMeniulContextual(pNode, payload)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Aduce la zi cache-ul PDF-ului SEMNAT al reviziei selectate (felia 0041) și re-împinge
    ''' contextul.
    '''
    ''' O revizie fără <c>PdfSha256</c> nu are PDF semnat pe server: nu se face niciun apel, iar
    ''' documentul rămâne pe drumul lui obișnuit — se REGENEREAZĂ la cererea operatorului, în
    ''' zona de lucru (<c>TempPdf\</c>). Când există, cache-ul se validează prin sumă și se
    ''' descarcă doar la nepotrivire (vezi <see cref="PdfCache"/>).
    '''
    ''' Boundary UI async: loghează și înghite — nu există await care să prindă.
    ''' </summary>
    Private Async Sub EnsureSignedPdfAsync(revizie As RevizieRow)
        Try
            If revizie Is Nothing OrElse _antet Is Nothing Then Return
            If Not revizie.ArePdfSemnat Then Return

            Dim cachePath As String =
                DdfPdfLocator.ExpectedPath(KBotPaths.Current.DdfPdfRoot, _antet, revizie.NumarRev)
            If String.IsNullOrEmpty(cachePath) Then Return

            Dim idrev As Integer = revizie.Idrev
            Dim rezultat As PdfCacheResult = Await PdfCache.EnsureAsync(
                cachePath, revizie.PdfSha256,
                Function(shaLocal) _apiClient.DownloadDdfPdfAsync(idrev, shaLocal, CancellationToken.None)).ConfigureAwait(True)

            ' Stale-guard: între timp operatorul a dat click pe alt nod. Aruncăm răspunsul —
            ' altfel am arăta documentul nodului precedent peste selecția curentă.
            If _selectedRevizie Is Nothing OrElse _selectedRevizie.Idrev <> idrev Then Return

            Select Case rezultat.Status
                Case PdfCacheStatus.Gata
                    _pdfPathRezolvat = rezultat.Cale
                    _pdfRezolvatPentruIdrev = idrev
                    PushToActivePage()
                Case PdfCacheStatus.Eroare
                    ' Documentul semnat nu s-a putut aduce. Nu inventăm o cădere pe regenerare:
                    ' operatorul trebuie să știe că EXISTĂ un document semnat pe care nu-l vedem.
                    ShowEmpty(rezultat.Mesaj)
                Case Else
                    ' Nesemnat (cursă: rândul a dispărut) — rămâne calea așteptată, iar pagina
                    ' «Document» arată suprafața „document lipsă" cu butonul de generare.
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.EnsureSignedPdfAsync", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Strângerea arborelui (felia 0028, aceeași înțelegere ca în MainForm): arborele e
    ''' <c>Dock = Fill</c> în <c>split.Panel1</c>, deci lățimea NU e a lui — el schimbă starea
    ''' și ne anunță, GAZDA mută splitter-ul. <c>Panel1MinSize</c> păzește TRAGEREA splitter-ului;
    ''' strângerea e o comandă, nu o tragere, deci coborâm paza cât ține starea.
    ''' </summary>
    Private Sub Tree_CollapsedChanged(collapsed As Boolean) Handles tree.CollapsedChanged
        Try
            Dim padStanga As Integer = split.Panel1.Padding.Left
            If collapsed Then
                _splitterDistanceDesfasurat = split.SplitterDistance
                _panel1MinSizeDesfasurat = split.Panel1MinSize
                Dim tinta As Integer = tree.MinimumCollapsedWidth + padStanga
                split.Panel1MinSize = Math.Min(_panel1MinSizeDesfasurat, tinta)
                split.SplitterDistance = ClampSplitter(tinta)
                split.IsSplitterFixed = True
            Else
                split.IsSplitterFixed = False
                If _panel1MinSizeDesfasurat > 0 Then split.Panel1MinSize = _panel1MinSizeDesfasurat
                Dim tinta As Integer = If(_splitterDistanceDesfasurat > 0,
                                          _splitterDistanceDesfasurat,
                                          tree.ExpandedWidth + padStanga)
                split.SplitterDistance = ClampSplitter(tinta)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.Tree_CollapsedChanged", ex)
        End Try
    End Sub

    ' Distanța splitter-ului adusă în intervalul acceptat de SplitContainer — o vedere îngustă
    ' n-are voie să transforme apăsarea butonului de strângere într-o excepție.
    Private Function ClampSplitter(dorit As Integer) As Integer
        Dim maxim As Integer = split.Width - split.Panel2MinSize - split.SplitterWidth
        If maxim < split.Panel1MinSize Then Return split.Panel1MinSize
        Return Math.Max(split.Panel1MinSize, Math.Min(dorit, maxim))
    End Function

    ' ── Evenimentele urcate de sub-pagini ────────────────────────────────────
    ' Un fișier ales din lista paginii «Fișiere» -> devine calea PDF a contextului, apoi comutăm
    ' pe «Document». NU comutăm pe «Vizualizare» (reconstrucția XFA) — cererea operatorului.
    Private Sub OnFileActivated(sender As Object, pdfPath As String)
        Try
            If String.IsNullOrWhiteSpace(pdfPath) Then Return
            _pdfPathOverride = pdfPath
            PushToActivePage()
            navSub.SelectedKey = PAGE_PDF      ' ridică SelectionChanged -> ActivatePage -> îl arată
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.OnFileActivated", ex)
        End Try
    End Sub

    ' «Generează documentul» de pe suprafața „document lipsă" (felia 05). Boundary UI async:
    ' loghează și înghite (NU rearuncă — nu există await care să prindă).
    Private Async Sub OnGenerateRequested(sender As Object, e As EventArgs)
        Try
            If _generating Then Return
            Dim cod As String = _requestedCod
            Dim revizie As RevizieRow = _selectedRevizie
            If String.IsNullOrWhiteSpace(cod) OrElse revizie Is Nothing OrElse _antet Is Nothing Then Return

            _generating = True
            Try
                ' 1. Datele de generare (secțiunea B + atașamentele) — un apel opt-in.
                Dim data As DdfInfo = Await _withReauth(
                    Function() _apiClient.GetDdfAsync(cod, CancellationToken.None, pentruGenerare:=True)).ConfigureAwait(True)
                If data Is Nothing Then Return
                ' Ținta s-a schimbat între timp? Renunțăm.
                If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return

                ' 2. Doar rândurile reviziei-țintă (generarea e per revizie, ca tmpFX_* din Access).
                Dim liniiRev = data.Linii.Where(Function(l) l.Idrev = revizie.Idrev).ToList()
                Dim sbRev = data.SectiuneB.Where(Function(s) s.Idrev = revizie.Idrev).ToList()
                Dim attRev = data.Atasamente.Where(Function(a) a.Idrev = revizie.Idrev).ToList()

                ' 3. XML-ul complet (form1 + NOTAFD + atașamente).
                Dim ctx As DdfXmlBuilder.Context = DdfXmlBuilder.Context.FromSession(_session)
                Dim xml As String = DdfXmlBuilder.BuildComplete(ctx, _antet, revizie, liniiRev, sbRev, attRev)

                ' 4. FELIA 0041 — documentul generat aici este NESEMNAT, deci un artefact DERIVAT:
                ' merge în zona de lucru (`<AppDir>\TempPdf\`, golită la fiecare pornire), NU în
                ' cache-ul persistent al PDF-urilor semnate și NU pe server. Numele fișierului
                ' rămâne cel din convenție — se schimbă doar folderul, iar zona e plată (fără
                ' subfolder de partener: se golește oricum). Siblingul .xml stă lângă el.
                Dim numeFisier As String = IO.Path.GetFileName(
                    DdfPdfLocator.ExpectedPath(KBotPaths.Current.DdfPdfRoot, _antet, revizie.NumarRev))
                If String.IsNullOrEmpty(numeFisier) Then Return
                TempPdfStore.EnsureRoot()
                Dim pdfPath As String = TempPdfStore.PathFor(numeFisier)
                Dim xmlPath As String = IO.Path.ChangeExtension(pdfPath, ".xml")
                IO.File.WriteAllText(xmlPath, xml, New Text.UTF8Encoding(False))

                ' 5. Generarea PROPRIU-ZISĂ pe thread de fundal (descarcă macheta, completează XFA,
                ' embedează atașamentele, scrie PDF-ul). XfaWriter loghează + rearuncă la graniță;
                ' NU adăugăm un al doilea strat de catch în jur — îl lăsăm să urce în catch-ul de aici.
                Await Task.Run(Sub() KBot.Xfa.XfaWriter.Genereaza(xmlPath, pdfPath, "DDF", deschidePdf:=False)).ConfigureAwait(True)

                ' 6. Fără scriere înapoi în bază și FĂRĂ încărcare pe server: documentul e
                ' nesemnat, iar felia 0041 stochează DOAR semnate (încărcarea vine cu felia de
                ' semnare, 0021). Existența se decide prin probă pe disc: reconstruim contextul
                ' (PdfExists tocmai a trecut din False în True) și îl re-împingem paginii active,
                ' care se re-randează pe calea ei normală. Nicio pagină n-are metodă de
                ' „reîmprospătare" — SetContext e singura. Garda paginii «Document» e pe perechea
                ' (cale, existență) tocmai ca acest salt să forțeze re-încorporarea deși calea a
                ' rămas aceeași.
                _pdfPathOverride = pdfPath
                PushToActivePage()
            Finally
                _generating = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.OnGenerateRequested", ex)
        End Try
    End Sub

    ' ── Stare goală / conținut ───────────────────────────────────────────────
    Private Sub ClearAll()
        _requestedCod = Nothing
        _revizii = Nothing
        _liniiByRev = Nothing
        _antet = Nothing
        _nodeRows = Nothing
        _nodeIsRoot = False
        _selectedRevizie = Nothing
        _pdfPathOverride = Nothing
        _pdfPathRezolvat = Nothing
        _pdfRezolvatPentruIdrev = 0
        tree.Clear()
        ' Contextul devine Nothing -> pagina activă își arată starea goală.
        PushToActivePage()
    End Sub

    Private Sub ShowEmpty(message As String)
        lblEmpty.Text = message
        lblEmpty.Visible = True
        split.Visible = False
    End Sub

    Private Sub ShowContent()
        lblEmpty.Visible = False
        split.Visible = True
    End Sub

    ' ── Formatare / iconițe ──────────────────────────────────────────────────
    Private Shared Function Money(value As Double) As String
        Return value.ToString("N2", _roCulture)
    End Function

    ' Cheia de lună = an*100 + lună din DataRev (0 dacă lipsește). Ordonabilă cronologic.
    Private Shared Function MonthKeyOf(value As Date?) As Integer
        If Not value.HasValue Then Return 0
        Return value.Value.Year * 100 + value.Value.Month
    End Function

    ' Cheia de nod a rădăcinii: «LA_{yyyy}_{M}» (planul §5). Fără dată -> «LA_0_0».
    Private Shared Function MonthKeyText(monthKey As Integer) As String
        If monthKey <= 0 Then Return "LA_0_0"
        Return $"LA_{monthKey \ 100}_{monthKey Mod 100}"
    End Function

    Private Shared Function MonthYearLabel(monthKey As Integer) As String
        If monthKey <= 0 Then Return "(fără dată)"
        Dim y As Integer = monthKey \ 100
        Dim m As Integer = monthKey Mod 100
        Return $"{MonthLabel(m)}" '/{y}"
    End Function

    ' Numele lunii în română (Ianuarie…), cu prima literă mare (ca în Plăți/Recepții/Rezervări).
    Private Shared Function MonthLabel(month As Integer) As String
        If month < 1 OrElse month > 12 Then Return CStr(month)
        Dim name As String = _roCulture.DateTimeFormat.GetMonthName(month)
        If String.IsNullOrEmpty(name) Then Return CStr(month)
        Return Char.ToUpper(name(0), _roCulture) & name.Substring(1)
    End Function

    ''' <summary>
    ''' Starea vizuală a unei revizii, dată de SEMNUL TOTALULUI: pozitiv → sus, negativ → jos,
    ''' exact zero → neutru. Aceeași axă ca la Rezervări (mărire ▲ / micșorare ▼) și aceeași cu
    ''' a culorii rândului, care e deja roșu când totalul e negativ.
    '''
    ''' <para>ÎNAINTE se citea starea de încărcare (<c>Incarcat</c> → sus, altfel <c>Preluat</c>
    ''' → jos), portare literală din <c>frmFX_MAIN_DDF.Show_Revizii</c>. E o axă DIFERITĂ de
    ''' semn, și în practică arăta ▼ pe aproape tot, fiindcă reviziile sunt de regulă preluate.
    ''' Dacă starea de încărcare trebuie să se vadă din nou, are nevoie de propriul semn vizual
    ''' (o iconiță în dreapta, îngroșare), nu de săgeata care înseamnă acum semnul sumei.</para>
    ''' </summary>
    Private Shared Function StareOf(r As RevizieRow) As DdfIcons.Stare
        If r Is Nothing Then Return DdfIcons.Stare.Neutru
        If r.TotalRevizie > 0 Then Return DdfIcons.Stare.Sus
        If r.TotalRevizie < 0 Then Return DdfIcons.Stare.Jos
        Return DdfIcons.Stare.Neutru
    End Function

    ''' <summary>
    ''' Iconița stării unei revizii. ÎNTÂI din «image_list» (pozele alese de operator în
    ''' designer), și abia dacă lista n-are cheia respectivă se cade înapoi pe formele GDI din
    ''' <see cref="DdfIcons"/>, colorate din paletă (sus=succes, jos=accent, neutru=estompat).
    ''' Felia 0033 §12: aceeași regulă listă-întâi ca în <c>RezervariView.TipIconOf</c>.
    ''' </summary>
    Private Function IconFor(stare As DdfIcons.Stare, palette As ThemePalette) As Image
        Dim cheie As String
        Select Case stare
            Case DdfIcons.Stare.Sus : cheie = ICO_SUS
            Case DdfIcons.Stare.Jos : cheie = ICO_JOS
            Case Else : cheie = ICO_NEUTRU
        End Select

        Dim dinLista As Image = tree.NodeImage(cheie)
        If dinLista IsNot Nothing Then Return dinLista

        ' Fallback GDI (se re-tintează pe paletă; imaginile din listă sunt fixe).
        If palette Is Nothing Then Return Nothing
        Dim color As Color
        Select Case stare
            Case DdfIcons.Stare.Sus : color = palette.SuccessColor
            Case DdfIcons.Stare.Jos : color = palette.AccentColor
            Case Else : color = palette.TextDimColor
        End Select
        Return DdfIcons.StatusIcon(stare, color, tree.LeftIconSize.Width)
    End Function

    ''' <summary>
    ''' Iconița folderului de lună pentru cheia dată («folder_closed» / «folder_open»), cu
    ''' aceeași regulă listă-întâi ca <see cref="IconFor"/>.
    ''' </summary>
    Private Function LunaIcon(cheie As String, palette As ThemePalette) As Image
        Dim dinLista As Image = tree.NodeImage(cheie)
        If dinLista IsNot Nothing Then Return dinLista
        If palette Is Nothing Then Return Nothing
        Return DdfIcons.LunaIcon(palette.TextDimColor, tree.LeftIconSize.Width)
    End Function

    Private Shared Function TryGetPalette() As ThemePalette
        ' Headless (teste) sau înainte de inițializarea temei: ThemeManager.Current poate fi
        ' Nothing. Atunci arborele se construiește fără iconițe/culori (structura e aceeași),
        ' iar ApplyTheme reconstruiește când tema devine disponibilă.
        Dim current As ThemeScheme = ThemeManager.Current
        Return current?.Palette
    End Function

    ''' <summary>
    ''' Reaplică schema pe ce a rămas la vedere după felia 0032 — arborele, splitter-ul, gazda
    ''' paginilor și starea goală — apoi CASCADEAZĂ spre sub-paginile deja create (fiecare își
    ''' temează singură conținutul). Paginile necreate n-au nevoie: primesc tema la activare,
    ''' prin <c>ThemeManager.Apply</c>. Reconstruiește arborele dacă are date, ca iconițele să se
    ''' re-tinteze.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            split.BackColor = p.SurfaceAltColor
            split.Panel1.BackColor = p.SurfaceAltColor
            split.Panel2.BackColor = p.SurfaceAltColor

            ' Arborele e IThemedControl: își ia singur paleta, iar ThemeManager nu mai recurge
            ' în copiii lui. Culorile puse în designer câștigă; cele lăsate goale urmează tema.

            pnlPages.BackColor = p.SurfaceAltColor
            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor

            For Each page As IDdfPage In _pages.Values
                Dim themed As IThemedControl = TryCast(page, IThemedControl)
                themed?.ApplyTheme(scheme)
            Next

            ' Re-tintarea iconițelor pe noua paletă. Arborele se reconstruiește, deci selecția
            ' se pierde — paginile rămân cum sunt până la următorul click pe un nod.
            If _revizii IsNot Nothing AndAlso _revizii.Count > 0 Then
                BuildTree(_revizii)
            End If
        Catch ex As Exception
            ' Boundary UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("DdfView.ApplyTheme", ex)
        End Try
    End Sub


    ' ══════════════════════════════════════════════════════════════════════════════════
    ' THE WRITE COMMANDS (slice 0051)
    '
    ' The read path above is untouched. This is the only thing the write slice added to the
    ' view: a context menu on the tree, plus a reload after each command.
    ' ══════════════════════════════════════════════════════════════════════════════════

    Private Const MENIU_MODIFICA As String = "modifica"
    Private Const MENIU_STERGE_REVIZIE As String = "sterge-revizie"
    Private Const MENIU_STERGE_DOC As String = "sterge-document"
    Private Const MENIU_STERGE_LUNA As String = "sterge-luna"

    ''' <summary>Which revision the next tree build should land on; 0 = leave the selection
    ''' where the build puts it.</summary>
    Private _idrevDeReselectat As Integer

    ''' <summary>The leaf node of each revision, filled by the tree build. Only used to put
    ''' the selection back after a reload.</summary>
    Private ReadOnly _noduriRevizie As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()

    ''' <summary>
    ''' The tree's context menu. What it offers depends on WHICH LEVEL was clicked: a month
    ''' root can only have its whole month deleted; a revision leaf can be edited, deleted, or
    ''' have its whole document deleted.
    '''
    ''' <para>There is deliberately NO «Adauga» entry. The two add commands are triggered from
    ''' the RESERVATIONS tree instead -- the "+" icon on a reservation leaf -- which is where
    ''' Access triggered them (<c>fxRezervari_AdaugaRevizie</c>). One trigger, in one
    ''' place.</para>
    ''' </summary>
    Private Sub AratatMeniulContextual(nod As AdvancedTreeControl.TreeItem, payload As DdfNodeRows)
        If String.IsNullOrWhiteSpace(_requestedCod) Then Return
        If payload Is Nothing Then Return

        Dim intrari As New List(Of CustomPopupItem)()
        If payload.IsRoot Then
            intrari.Add(New CustomPopupItem(MENIU_STERGE_LUNA, "Șterge &TOATE reviziile lunii"))
        Else
            intrari.Add(New CustomPopupItem(MENIU_MODIFICA, "&Modifică revizia"))
            intrari.Add(New CustomPopupItem(MENIU_STERGE_REVIZIE, "Șter&ge revizia"))
            intrari.Add(New CustomPopupItem(MENIU_STERGE_DOC, "Șterge &documentul"))
        End If

        Dim cheieNod As String = If(nod Is Nothing, String.Empty, nod.Key)
        Dim revizie As RevizieRow = payload.Revizie
        Dim meniu As New CustomPopup(intrari)
        AddHandler meniu.ItemClicked,
            Sub(s As Object, ev As CustomPopupItemEventArgs)
                AplicaComandaDeMeniu(ev.Item.Key, revizie, cheieNod)
            End Sub
        meniu.ShowAtCursor(tree)
    End Sub

    Private Sub AplicaComandaDeMeniu(cheie As String, revizie As RevizieRow, cheieNod As String)
        Try
            Select Case cheie
                Case MENIU_MODIFICA
                    CereComanda(New DdfComanda(DdfActiune.Modifica, _requestedCod, revizie))
                Case MENIU_STERGE_REVIZIE
                    CereComanda(New DdfComanda(DdfActiune.StergeRevizie, _requestedCod, revizie))
                Case MENIU_STERGE_DOC
                    CereComanda(New DdfComanda(DdfActiune.Sterge, _requestedCod, revizie))
                Case MENIU_STERGE_LUNA
                    Dim an As Integer, luna As Integer
                    If Not CitesteLunaDinCheie(cheieNod, an, luna) Then
                        MessageBox.Show(Me, "Nu pot determina luna nodului selectat.", "K-BOT",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If
                    CereComanda(DdfComanda.PeLuna(_requestedCod, IddfCurent(), an, luna))
                Case Else
                    ' No silent no-ops: an unknown key is a programming defect.
                    Throw New ArgumentException($"Comandă de meniu necunoscută: {cheie}", NameOf(cheie))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.AplicaComandaDeMeniu", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The month behind a root node's key. The key is «LA_{yyyy}_{M}», written by
    ''' <c>MonthKeyText</c>, so it is parsed rather than guessed -- and a key that does not
    ''' parse returns False instead of a silent zero, which would delete the wrong month.
    ''' </summary>
    Private Shared Function CitesteLunaDinCheie(cheie As String, ByRef an As Integer,
                                                ByRef luna As Integer) As Boolean
        an = 0
        luna = 0
        If String.IsNullOrWhiteSpace(cheie) OrElse Not cheie.StartsWith("LA_", StringComparison.Ordinal) Then
            Return False
        End If
        Dim parti As String() = cheie.Substring(3).Split("_"c)
        If parti.Length <> 2 Then Return False
        If Not Integer.TryParse(parti(0), NumberStyles.Integer, CultureInfo.InvariantCulture, an) Then Return False
        If Not Integer.TryParse(parti(1), NumberStyles.Integer, CultureInfo.InvariantCulture, luna) Then Return False
        Return an > 0 AndAlso luna >= 1 AndAlso luna <= 12
    End Function

    ''' <summary>
    ''' The document key of the angajament currently loaded. Every revision of one angajament
    ''' belongs to the same document, so the first one that carries a key answers for all --
    ''' and a month root, which carries no revision of its own, needs exactly that.
    ''' </summary>
    Private Function IddfCurent() As Integer
        If _revizii Is Nothing Then Return 0
        For Each r As RevizieRow In _revizii
            If r.Iddf > 0 Then Return r.Iddf
        Next
        Return 0
    End Function

    ''' <summary>Sends the command to the shell. With no action bound (the tests, or a host
    ''' that does not supply one) the operator is told -- it is not swallowed.</summary>
    Private Sub CereComanda(comanda As DdfComanda)
        If _executaComanda Is Nothing Then
            MessageBox.Show(Me, "Editorul de documente de fundamentare nu este disponibil în acest context.",
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        _executaComanda(comanda)
    End Sub

    ''' <summary>
    ''' Reloads the angajament after a write command and, when asked, remembers which revision
    ''' to land on. Goes through the existing <c>SetContext</c> path, so the reload is the same
    ''' one a normal selection performs -- there is no second loading route to keep in step.
    ''' </summary>
    Public Sub Reincarca(Optional idrevDeSelectat As Integer = 0)
        Try
            Dim cod As String = _requestedCod
            If String.IsNullOrWhiteSpace(cod) Then Return
            _idrevDeReselectat = idrevDeSelectat
            ShowEmpty("Se încarcă documentul de fundamentare…")
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfView.Reincarca", ex)
        End Try
    End Sub

End Class

''' <summary>
''' Ce acoperă un nod din arborele DDF: liniile de secțiune A pe care le arată pagina «Valori»,
''' dacă nodul e o rădăcină de lună (o lună nu are UN singur document) și — pentru frunze —
''' revizia însăși, din care se compune calea PDF-ului. POCO -&gt; fără Try/Catch.
''' </summary>
Friend NotInheritable Class DdfNodeRows
    Public ReadOnly Property Linii As List(Of LinieSaRow)
    Public ReadOnly Property IsRoot As Boolean
    ''' <summary>Revizia frunzei; Nothing pe o rădăcină de lună.</summary>
    Public ReadOnly Property Revizie As RevizieRow

    Public Sub New(linii As List(Of LinieSaRow), isRoot As Boolean, Optional revizie As RevizieRow = Nothing)
        Me.Linii = If(linii, New List(Of LinieSaRow)())
        Me.IsRoot = isRoot
        Me.Revizie = revizie
    End Sub
End Class
