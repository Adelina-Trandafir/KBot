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
''' Vederea ORD (felia 0033) — Ordonanțările unui angajament. Ultima dintre cele trei vederi
''' reale rămase. Read-only în felia asta.
'''
''' Formă: aceeași cu a sub-vederilor DDF — un arbore de ordonanțări pe DOUĂ niveluri la
''' stânga (lună -&gt; ordonanțare, ca la <c>RezervariView</c>) și, la dreapta, o sub-navigare
''' ORIZONTALĂ cu două pagini leneșe: «Vizualizare» (liniile FX_ORD_TBL, grilă) și «Document»
''' (PDF-ul real). Părintele deține datele; paginile sunt proaste și redau ce li se dă.
'''
''' Datele vin dintr-un SINGUR apel GET /api/forexe/ord pentru tot CodAngajament-ul, prin
''' plasa de re-autentificare a shell-ului (401 -&gt; re-login -&gt; reia o dată). Un click în
''' arbore FILTREAZĂ datele deja încărcate — nicio cerere de rețea.
'''
''' AMÂNAT DELIBERAT (felii ulterioare, NU scăpări): generarea PDF-ului, paginile Atașamente
''' (FX_ORD_ATT), Documente (FX_ORD_DOC) și Fișiere, gruparea pe beneficiar (FX_ORD_PART) în
''' grilă, și bifele de selecție multiplă din formularul Access (ORD-plăți în lot).
'''
''' ABATERE DELIBERATĂ de la Access: valoarea unei ordonanțări este SUM(Valoare) peste
''' liniile ei, calculat pe server printr-o subinterogare scalară. Access o adună peste un
''' join cu FX_ORD_PART, unde mai mulți beneficiari pot umfla totalul (familia de defect
''' `aggOrd` / `aggRev`).
''' </summary>
Public Class OrdView
    Implements IAngajamentView, IThemedControl

    ' Cheile paginilor sub-navigării — o singură definiție, folosită la creare și la comutare.
    ' Designerul le scrie ca LITERALE în navSub.Items, deci cele două trebuie să rămână în
    ' acord; dacă se desincronizează, atribuirea din BuildNav aruncă ArgumentException pe
    ' cheie necunoscută — zgomotos, nu tăcut.
    Private Const PAGE_VIZUALIZARE As String = "vizualizare"
    Private Const PAGE_DOCUMENT As String = "document"

    ' CHEILE ICONIȚELOR din «image_list» (legat de arbore prin tree.NodeImages). O cheie lipsă
    ' întoarce Nothing, deci nodul rămâne fără iconiță — ORD nu are clasă de iconițe GDI, iar
    ' remediul e să pui poza în designer, nu să desenezi una în cod.
    Private Const ICO_LUNA As String = "month"
    Private Const ICO_SUS As String = "up"       ' total pozitiv ▲
    Private Const ICO_JOS As String = "down"     ' total negativ ▼
    Private Const ICO_NEUTRU As String = "neutral"   ' nici una, nici alta

    ' Format românesc: separator de mii «.» și zecimală «,» (1.091.940,00).
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    ' Plasa 401 a shell-ului (MainForm.WithReauth), specializată pe OrdInfo.
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of OrdInfo)), Task(Of OrdInfo))
    ' Sesiunea (globalii unității pentru banda de antet a paginii «Vizualizare»). Poate fi
    ' Nothing în teste — atunci antetul își sare rândurile de unitate. Același tipar ca DdfView.
    Private ReadOnly _session As SessionContext

    ' Sub-paginile create până acum (leneș, la prima activare) și cea vizibilă acum.
    Private ReadOnly _pages As New Dictionary(Of String, IOrdPage)(StringComparer.Ordinal)
    Private _activePage As IOrdPage
    ' Contextul împins paginilor: reconstruit la fiecare schimbare de nod.
    Private _currentCtx As OrdPageContext

    ' Actiunea de SCRIERE (felia 0049), data de shell: vederea ramane read-only, comenzile care
    ' scriu (adauga / modifica / sterge / lot) traiesc in MainForm, unde e plasa de re-login.
    ' Nothing in teste si in orice gazda care nu le da.
    Private ReadOnly _executaComanda As Action(Of OrdComanda)

    ' Ordonantarea de re-selectat dupa o reincarcare (dupa o salvare, de pilda); 0 = niciuna.
    Private _idordpDeSelectat As Integer

    ' Frunzele arborelui dupa IDORDP, umplute la construirea lui. Arborele nu are cautare
    ' dupa cheie, iar re-selectarea de dupa o salvare are nevoie de nodul insusi.
    Private ReadOnly _noduriOrd As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()

    ' Cheile meniului contextual.
    Private Const MENIU_ADAUGA As String = "adauga"
    Private Const MENIU_MODIFICA As String = "modifica"
    Private Const MENIU_STERGE As String = "sterge"
    Private Const MENIU_LOT As String = "lot"

    ' Codul angajamentului CERUT ultima dată — stale-guard (identic cu DDF/Plăți/Rezervări).
    Private _requestedCod As String

    ' Ultimele date încărcate — păstrate ca ApplyTheme să reconstruiască arborele fără o nouă
    ' cerere de rețea, și ca un click în arbore să filtreze local.
    Private _ordonantari As List(Of OrdHeaderRow)
    Private _linii As List(Of OrdLinieRow)

    ' Nodul selectat acum: liniile lui, dacă e rădăcină de lună, și — pe frunză — ordonanțarea.
    Private _nodeLinii As List(Of OrdLinieRow)
    Private _nodeIsRoot As Boolean
    Private _selectedOrd As OrdHeaderRow
    ' Calea REZOLVATĂ a documentului nodului curent (felia 0041): fișierul semnat adus în cache
    ' de pe server. Gol până când rezolvarea se întoarce.
    Private _pdfPathRezolvat As String
    ' Ordonanțarea pentru care s-a rezolvat calea de mai sus — stale-guard, ca la DDF.
    Private _pdfRezolvatPentruIdordp As Integer

    ' Starea splitter-ului dinainte de strângerea arborelui, ca desfacerea să-l pună înapoi
    ' exact unde era (vezi Tree_CollapsedChanged). 0 = arborele n-a fost încă strâns.
    Private _splitterDistanceDesfasurat As Integer
    Private _panel1MinSizeDesfasurat As Integer

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of OrdInfo)), Task(Of OrdInfo)),
                   Optional session As SessionContext = Nothing,
                   Optional executaComanda As Action(Of OrdComanda) = Nothing)
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

    ' ── Punctele de intrare ale EDITORULUI (felia 0049) ─────────────────────────

    ''' <summary>
    ''' Reincarca ordonantarile angajamentului curent si, daca i se cere, selecteaza o anume.
    ''' Gazda o cheama dupa fiecare salvare sau stergere: ce a ramas pe ecran nu mai e adevarat.
    ''' </summary>
    Public Sub Reincarca(Optional idordpDeSelectat As Integer = 0)
        Try
            If String.IsNullOrWhiteSpace(_requestedCod) Then Return
            _idordpDeSelectat = idordpDeSelectat
            ShowEmpty("Se încarcă ordonanțările…")
            LoadAsync(_requestedCod)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.Reincarca", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Butonul din subsolul arborelui («+ Adauga Ordonantare») — punctul de intrare
    ''' «Adauga». Cere shell-ului comanda; toata reteaua traieste acolo.
    ''' </summary>
    Private Sub Tree_FooterRightIconClicked(e As MouseEventArgs) Handles tree.FooterRightIconClicked
        Try
            CereComanda(OrdActiune.Adauga)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.Tree_FooterRightIconClicked", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Meniul contextual al arborelui: cele patru puncte de intrare, poarta fiecaruia fiind
    ''' ce e selectat. «Modifica» si «Sterge» au nevoie de o FRUNZA (o ordonantare anume);
    ''' «Adauga» si «Generare in lot» au nevoie doar de un angajament incarcat.
    ''' </summary>
    Private Sub AratatMeniulContextual(nod As AdvancedTreeControl.TreeItem)
        If String.IsNullOrWhiteSpace(_requestedCod) Then Return

        Dim payload As OrdNodePayload = TryCast(If(nod Is Nothing, Nothing, nod.Tag), OrdNodePayload)
        Dim ordonantare As OrdHeaderRow = If(payload Is Nothing, Nothing, payload.Ordonantare)

        Dim intrari As New List(Of CustomPopupItem)()
        intrari.Add(New CustomPopupItem(MENIU_ADAUGA, "&Adaugă ordonanțare…"))
        If ordonantare IsNot Nothing Then
            intrari.Add(New CustomPopupItem(MENIU_MODIFICA, "&Modifică ordonanțarea"))
            intrari.Add(New CustomPopupItem(MENIU_STERGE, "Șter&ge ordonanțarea"))
        End If
        intrari.Add(New CustomPopupItem(MENIU_LOT, "Generare în &lot…"))

        Dim meniu As New CustomPopup(intrari)
        AddHandler meniu.ItemClicked,
            Sub(s As Object, ev As CustomPopupItemEventArgs) AplicaComandaDeMeniu(ev.Item.Key, ordonantare)
        meniu.ShowAtCursor(tree)
    End Sub

    Private Sub AplicaComandaDeMeniu(cheie As String, ordonantare As OrdHeaderRow)
        Try
            Select Case cheie
                Case MENIU_ADAUGA : CereComanda(OrdActiune.Adauga)
                Case MENIU_MODIFICA : CereComanda(OrdActiune.Modifica, ordonantare)
                Case MENIU_STERGE : CereComanda(OrdActiune.Sterge, ordonantare)
                Case MENIU_LOT : CereComanda(OrdActiune.Lot)
                Case Else
                    ' Fara no-op-uri tacute: o cheie necunoscuta e un defect de programare.
                    Throw New ArgumentException($"Comandă de meniu necunoscută: {cheie}", NameOf(cheie))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.AplicaComandaDeMeniu", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Trimite comanda spre shell. Fara actiune legata (teste, sau o gazda care nu o da), se
    ''' spune operatorului — nu se inghite tacut.
    ''' </summary>
    Private Sub CereComanda(actiune As OrdActiune, Optional ordonantare As OrdHeaderRow = Nothing)
        If _executaComanda Is Nothing Then
            MessageBox.Show(Me, "Editorul de ordonanțări nu este disponibil în acest context.",
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        _executaComanda(New OrdComanda(actiune, _requestedCod, ordonantare))
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "ord"
        End Get
    End Property

    ' ── Găzduirea sub-paginilor ──────────────────────────────────────────────
    ' Intrările sub-navigării sunt AUTORITE ÎN DESIGNER (navSub.Items). Aici rămâne doar
    ' selecția inițială: atribuirea e cea care ridică SelectionChanged și, prin ea,
    ' ActivatePage arată prima pagină. NU se activează a doua oară de mână — ar rula DUPĂ
    ' eveniment și ar ascunde exact pagina tocmai arătată.
    Private Sub BuildNav()
        Try
            navSub.SelectedKey = PAGE_VIZUALIZARE
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.BuildNav", ex)
            Throw
        End Try
    End Sub

    Private Sub NavSub_SelectionChanged(key As String) Handles navSub.SelectionChanged
        Try
            ActivatePage(key)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.NavSub_SelectionChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Arată pagina cerută, creând-o la prima activare — același tipar leneș ca
    ''' <c>DdfView.ActivatePage</c> / <c>MainForm.ActivateView</c>. O singură pagină vizibilă
    ''' odată; contextul curent se împinge abia acum, deci o pagină creată târziu nu pierde
    ''' selecția făcută înainte de ea.
    ''' </summary>
    Private Sub ActivatePage(key As String)
        Try
            Dim page As IOrdPage = Nothing
            If Not _pages.TryGetValue(key, page) Then
                page = CreatePage(key)
                Dim ctrl As Control = DirectCast(page, Control)
                ctrl.Dock = DockStyle.Fill
                ctrl.Visible = False
                pnlPages.Controls.Add(ctrl)
                ThemeManager.Apply(ctrl)
                _pages(key) = page
            End If

            Dim previous As IOrdPage = _activePage
            _activePage = page
            DirectCast(page, Control).Visible = True
            If previous IsNot Nothing AndAlso Not ReferenceEquals(previous, page) Then
                DirectCast(previous, Control).Visible = False
            End If
            ' Doar pagina ACTIVĂ primește contextul; celelalte îl primesc la activare.
            page.SetContext(_currentCtx)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.ActivatePage", ex)
            Throw
        End Try
    End Sub

    Private Function CreatePage(key As String) As IOrdPage
        Try
            Select Case key
                Case PAGE_VIZUALIZARE : Return New OrdVizualizarePage()
                Case PAGE_DOCUMENT : Return New OrdDocumentPage()
                Case Else
                    Throw New ArgumentException($"Pagină ORD necunoscută: '{key}'.", NameOf(key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.CreatePage", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Contextul nodului selectat acum. <c>Nothing</c> cât timp nu s-a încărcat niciun
    ''' angajament. Calea PDF există DOAR pentru o frunză: o lună nu are un singur document.
    ''' </summary>
    Private Function BuildCurrentContext() As OrdPageContext
        Try
            If String.IsNullOrWhiteSpace(_requestedCod) OrElse _ordonantari Is Nothing Then Return Nothing

            ' FELIA 0041 — calea rezolvată de EnsureSignedPdfAsync (cache-ul semnat validat prin
            ' sumă) are întâietate; cât timp rezolvarea nu s-a întors, se folosește calea
            ' AȘTEPTATĂ, ca un fișier deja prezent să se vadă imediat.
            Dim pdfPath As String = Nothing
            If Not _nodeIsRoot AndAlso _selectedOrd IsNot Nothing Then
                If Not String.IsNullOrEmpty(_pdfPathRezolvat) AndAlso
                   _pdfRezolvatPentruIdordp = _selectedOrd.Idordp Then
                    pdfPath = _pdfPathRezolvat
                Else
                    pdfPath = OrdPdfLocator.ExpectedPath(KBotPaths.Current.OrdPdfRoot, _selectedOrd, _requestedCod)
                End If
            End If
            ' Existența fișierului se decide printr-o probă pe discul clientului. Serverul spune
            ' acum dacă EXISTĂ un PDF semnat (`PdfSha256`), dar calea locală rămâne a clientului.
            Dim exists As Boolean = Not String.IsNullOrEmpty(pdfPath) AndAlso IO.File.Exists(pdfPath)

            ' Antetul întreg + globalii unității: banda de antet a paginii «Vizualizare» îi
            ' folosește. Sesiunea poate lipsi (teste) -> rândurile ei se sar, banda nu se rupe.
            Return New OrdPageContext(_nodeLinii, _nodeIsRoot,
                                      If(_selectedOrd Is Nothing, 0, _selectedOrd.NrOrd),
                                      If(_selectedOrd Is Nothing, Nothing, _selectedOrd.DataOrd),
                                      _requestedCod, pdfPath, exists,
                                      _selectedOrd,
                                      If(_session Is Nothing, String.Empty, _session.NumeUnitate),
                                      If(_session Is Nothing, String.Empty, _session.CF))
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.BuildCurrentContext", ex)
            Throw
        End Try
    End Function

    ' Reconstruiește contextul și îl împinge paginii active. Singura cale de randare a
    ' paginilor — nicio pagină n-are metodă de reîmprospătare.
    Private Sub PushToActivePage()
        _currentCtx = BuildCurrentContext()
        _activePage?.SetContext(_currentCtx)
    End Sub

    ' ── Contextul shell-ului ─────────────────────────────────────────────────
    ''' <summary>
    ''' Selecția din arbore s-a schimbat. Fără angajament (nod de capitol / deselectare) NU se
    ''' face niciun apel de rețea — doar se golește vederea.
    ''' </summary>
    Public Sub SetContext(info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        Try
            Dim cod As String = info?.CodAngajament
            If String.IsNullOrWhiteSpace(cod) Then
                ClearAll()
                ShowEmpty("Selectați un angajament din arbore.")
                Return
            End If

            _requestedCod = cod
            ShowEmpty("Se încarcă ordonanțările…")
            ' Fire-and-forget deliberat (handler sincron al shell-ului): metoda își tratează
            ' singură TOATE erorile — vezi comentariul din DdfView/PlatiView.
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Boundary UI: logăm și ARĂTĂM eroarea, nu o aruncăm mai departe (apelul e pornit fără
    ' await din SetContext, deci nu există cine să o prindă).
    Private Async Sub LoadAsync(cod As String)
        Try
            Dim data As OrdInfo = Await _withReauth(
                Function() _apiClient.GetOrdAsync(cod, CancellationToken.None)).ConfigureAwait(True)

            ' Răspuns depășit: între timp operatorul a selectat alt angajament. Îl aruncăm.
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return

            Dim ordonantari As List(Of OrdHeaderRow) =
                If(data Is Nothing, New List(Of OrdHeaderRow)(), data.Ordonantari)
            If ordonantari Is Nothing OrElse ordonantari.Count = 0 Then
                ClearAll()
                ShowEmpty("Angajamentul nu are ordonanțări.")
                Return
            End If

            _ordonantari = ordonantari
            _linii = If(data.Linii, New List(Of OrdLinieRow)())
            BuildTree(ordonantari)
            ' Dupa o salvare, gazda cere sa se revina pe ordonantarea scrisa (felia 0049).
            If _idordpDeSelectat > 0 Then
                Dim tinta As OrdHeaderRow = ordonantari.FirstOrDefault(Function(o) o.Idordp = _idordpDeSelectat)
                _idordpDeSelectat = 0
                If tinta IsNot Nothing Then
                    _nodeLinii = LiniiFor(tinta.Idordp)
                    _nodeIsRoot = False
                    _selectedOrd = tinta
                    _pdfPathRezolvat = Nothing
                    _pdfRezolvatPentruIdordp = 0
                    Dim nod As AdvancedTreeControl.TreeItem = Nothing
                    If _noduriOrd.TryGetValue(tinta.Idordp, nod) Then tree.SelectAndReveal(nod)
                    PushToActivePage()
                    ShowContent()
                    Return
                End If
            End If
            ' „Nimic selectat" -> paginile văd TOATE liniile angajamentului, ca la DDF: e
            ' aceeași vedere ca a unei rădăcini de lună, doar peste toate ordonanțările.
            _nodeLinii = _linii
            _nodeIsRoot = True
            _selectedOrd = Nothing
            _pdfPathRezolvat = Nothing
            _pdfRezolvatPentruIdordp = 0
            PushToActivePage()
            ShowContent()
        Catch ex As ApiException
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("OrdView.LoadAsync", ex)
            ClearAll()
            ShowEmpty(ex.Message)   ' mesaj românesc din câmpul «error» al serverului
        Catch ex As Exception
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("OrdView.LoadAsync", ex)
            ClearAll()
            ShowEmpty("Ordonanțările nu au putut fi încărcate. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' Liniile unei ordonanțări, filtrate local pe cheia MariaDB (IDORDP — capcana „...P").
    Private Function LiniiFor(idordp As Integer) As List(Of OrdLinieRow)
        Dim rezultat As New List(Of OrdLinieRow)()
        If _linii Is Nothing Then Return rezultat
        For Each l As OrdLinieRow In _linii
            If l.Idordp = idordp Then rezultat.Add(l)
        Next
        Return rezultat
    End Function

    ' ── Arborele ─────────────────────────────────────────────────────────────
    ' DOUĂ niveluri, ca la Rezervări: rădăcină de lună («LA_{yyyy}_{M}», valoarea = suma
    ' ordonanțărilor ei) -> frunză de ordonanțare («ORD_{IDORDP}», valoarea = TotalOrd).
    ' Fiecare nod poartă în Tag ce acoperă, ca un click să filtreze fără cerere de rețea.
    Private Sub BuildTree(ordonantari As List(Of OrdHeaderRow))
        Try
            tree.Clear()
            _noduriOrd.Clear()
            Dim palette As ThemePalette = TryGetPalette()

            Dim months = ordonantari.GroupBy(Function(o) MonthKeyOf(o.DataOrd)).
                                     OrderBy(Function(g) g.Key)

            For Each mg In months
                Dim monthOrds As List(Of OrdHeaderRow) = mg.ToList()
                Dim monthSum As Double = monthOrds.Sum(Function(o) o.TotalOrd)
                Dim monthLinii As New List(Of OrdLinieRow)()
                For Each o As OrdHeaderRow In monthOrds
                    monthLinii.AddRange(LiniiFor(o.Idordp))
                Next

                Dim icoLuna As Image = tree.NodeImage(ICO_LUNA)
                Dim root As AdvancedTreeControl.TreeItem =
                    tree.AddItem(MonthKeyText(mg.Key), $"{MonthYearLabel(mg.Key)}~~~{Money(monthSum)}",
                                 pLeftIconClosed:=icoLuna, pLeftIconOpen:=icoLuna,
                                 pExpanded:=True)
                root.Tag = New OrdNodePayload(monthLinii, isRoot:=True)
                root.Bold = True
                ' Roșu doar când PROPRIUL total e negativ (ca la Rezervări/DDF).
                If monthSum < 0 AndAlso palette IsNot Nothing Then
                    root.NodeForeColor = palette.ErrorColor
                End If

                ' Frunze de ordonanțare, în ordinea serverului (DataORD, NrORD).
                For Each o As OrdHeaderRow In monthOrds
                    Dim leafIcon As Image = StareIconOf(o)
                    Dim leaf As AdvancedTreeControl.TreeItem =
                        tree.AddItem($"ORD_{o.Idordp}", $"{o.EtichetaOrd}~~~{Money(o.TotalOrd)}",
                                     root, pLeftIconClosed:=leafIcon, pLeftIconOpen:=leafIcon)
                    leaf.Tag = New OrdNodePayload(LiniiFor(o.Idordp), isRoot:=False, ordonantare:=o)
                    _noduriOrd(o.Idordp) = leaf
                    If o.TotalOrd < 0 AndAlso palette IsNot Nothing Then
                        leaf.NodeForeColor = palette.ErrorColor
                    End If
                Next
            Next

            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.BuildTree", ex)
            Throw
        End Try
    End Sub

    ' Click pe orice nod -> se reconstruiește contextul și se împinge paginii active. Fără
    ' apel de rețea.
    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing Then Return
            Dim payload As OrdNodePayload = TryCast(pNode.Tag, OrdNodePayload)
            If payload Is Nothing Then Return

            ' Clic DREAPTA -> meniul de comenzi (felia 0049). Selectia se muta intai, ca meniul
            ' sa se refere la nodul de sub cursor, nu la cel de dinainte.
            If e IsNot Nothing AndAlso e.Button = MouseButtons.Right Then
                _nodeLinii = payload.Linii
                _nodeIsRoot = payload.IsRoot
                _selectedOrd = payload.Ordonantare
                PushToActivePage()
                AratatMeniulContextual(pNode)
                Return
            End If

            _nodeLinii = payload.Linii
            _nodeIsRoot = payload.IsRoot
            _selectedOrd = payload.Ordonantare
            ' O selecție nouă anulează calea rezolvată pentru nodul anterior (felia 0041).
            _pdfPathRezolvat = Nothing
            _pdfRezolvatPentruIdordp = 0

            PushToActivePage()
            ' Felia 0041: pe o frunză, aducem documentul SEMNAT de pe server (sau confirmăm că
            ' cel din cache e la zi) și abia apoi re-împingem contextul. Fire-and-forget
            ' deliberat, ca LoadAsync: metoda își tratează singură toate erorile.
            EnsureSignedPdfAsync(_selectedOrd)
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Aduce la zi cache-ul PDF-ului SEMNAT al ordonanțării selectate (felia 0041) și
    ''' re-împinge contextul. Sora lui <c>DdfView.EnsureSignedPdfAsync</c>, cu aceeași regulă:
    ''' fără <c>PdfSha256</c> nu se face niciun apel — documentul rămâne unul nesemnat, care se
    ''' produce local. Boundary UI async: loghează și înghite.
    ''' </summary>
    Private Async Sub EnsureSignedPdfAsync(ordonantare As OrdHeaderRow)
        Try
            If ordonantare Is Nothing Then Return
            If Not ordonantare.ArePdfSemnat Then Return

            Dim cachePath As String =
                OrdPdfLocator.ExpectedPath(KBotPaths.Current.OrdPdfRoot, ordonantare, _requestedCod)
            If String.IsNullOrEmpty(cachePath) Then Return

            Dim idordp As Integer = ordonantare.Idordp
            Dim rezultat As PdfCacheResult = Await PdfCache.EnsureAsync(
                cachePath, ordonantare.PdfSha256,
                Function(shaLocal) _apiClient.DownloadOrdPdfAsync(idordp, shaLocal, CancellationToken.None)).ConfigureAwait(True)

            ' Stale-guard: între timp operatorul a dat click pe alt nod.
            If _selectedOrd Is Nothing OrElse _selectedOrd.Idordp <> idordp Then Return

            Select Case rezultat.Status
                Case PdfCacheStatus.Gata
                    _pdfPathRezolvat = rezultat.Cale
                    _pdfRezolvatPentruIdordp = idordp
                    PushToActivePage()
                Case PdfCacheStatus.Eroare
                    ' EXISTĂ un document semnat pe care nu-l putem aduce — o spunem, nu cădem
                    ' tăcut pe „nu are PDF".
                    ShowEmpty(rezultat.Mesaj)
                Case Else
                    ' Nesemnat (cursă: rândul a dispărut) — rămâne calea așteptată.
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("OrdView.EnsureSignedPdfAsync", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Strângerea arborelui (felia 0028, aceeași înțelegere ca în MainForm): arborele e
    ''' <c>Dock = Fill</c> în <c>split.Panel1</c>, deci lățimea NU e a lui — el schimbă starea
    ''' și ne anunță, GAZDA mută splitter-ul. <c>Panel1MinSize</c> păzește TRAGEREA
    ''' splitter-ului; strângerea e o comandă, nu o tragere, deci coborâm paza cât ține starea.
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
            GlobalErrorLog.Write("OrdView.Tree_CollapsedChanged", ex)
        End Try
    End Sub

    ' Distanța splitter-ului adusă în intervalul acceptat de SplitContainer — o vedere îngustă
    ' n-are voie să transforme apăsarea butonului de strângere într-o excepție.
    Private Function ClampSplitter(dorit As Integer) As Integer
        Dim maxim As Integer = split.Width - split.Panel2MinSize - split.SplitterWidth
        If maxim < split.Panel1MinSize Then Return split.Panel1MinSize
        Return Math.Max(split.Panel1MinSize, Math.Min(dorit, maxim))
    End Function

    ' ── Stare goală / conținut ───────────────────────────────────────────────
    Private Sub ClearAll()
        _requestedCod = Nothing
        _ordonantari = Nothing
        _linii = Nothing
        _nodeLinii = Nothing
        _nodeIsRoot = False
        _selectedOrd = Nothing
        _pdfPathRezolvat = Nothing
        _pdfRezolvatPentruIdordp = 0
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

    ' Cheia de lună = an*100 + lună din DataORD (0 dacă lipsește). Ordonabilă cronologic.
    Private Shared Function MonthKeyOf(value As Date?) As Integer
        If Not value.HasValue Then Return 0
        Return value.Value.Year * 100 + value.Value.Month
    End Function

    ' Cheia de nod a rădăcinii: «LA_{yyyy}_{M}» (ca la DDF). Fără dată -> «LA_0_0».
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

    ' Numele lunii în română (Ianuarie…), cu prima literă mare (ca în celelalte vederi).
    Private Shared Function MonthLabel(month As Integer) As String
        If month < 1 OrElse month > 12 Then Return CStr(month)
        Dim name As String = _roCulture.DateTimeFormat.GetMonthName(month)
        If String.IsNullOrEmpty(name) Then Return CStr(month)
        Return Char.ToUpper(name(0), _roCulture) & name.Substring(1)
    End Function

    ''' <summary>
    ''' Iconița frunzei, dată de SEMNUL TOTALULUI ordonanțării: pozitiv → «sus», negativ →
    ''' «jos», exact zero → neutru. Aceeași axă ca la Rezervări și ca la DDF, și aceeași cu a
    ''' culorii rândului, care e deja roșu când totalul e negativ. Se ia DOAR din «image_list»;
    ''' o cheie lipsă lasă nodul fără iconiță, iar remediul e o poză în designer.
    '''
    ''' <para>ÎNAINTE se citea starea de încărcare (<c>Incarcat</c> → sus, altfel
    ''' <c>Preluat</c> → jos). E o axă DIFERITĂ de semn, și în practică arăta ▼ pe aproape tot,
    ''' fiindcă ordonanțările sunt de regulă preluate.</para>
    ''' </summary>
    Private Function StareIconOf(o As OrdHeaderRow) As Image
        If o Is Nothing Then Return Nothing
        If o.TotalOrd > 0 Then Return tree.NodeImage(ICO_SUS)
        If o.TotalOrd < 0 Then Return tree.NodeImage(ICO_JOS)
        Return tree.NodeImage(ICO_NEUTRU)
    End Function

    Private Shared Function TryGetPalette() As ThemePalette
        ' Headless (teste) sau înainte de inițializarea temei: ThemeManager.Current poate fi
        ' Nothing. Atunci arborele se construiește fără culori (structura e aceeași), iar
        ' ApplyTheme reconstruiește când tema devine disponibilă.
        Dim current As ThemeScheme = ThemeManager.Current
        Return current?.Palette
    End Function

    ''' <summary>
    ''' Reaplică schema pe arbore, splitter, gazda paginilor și starea goală, apoi CASCADEAZĂ
    ''' spre sub-paginile deja create (fiecare își temează singură conținutul). Paginile
    ''' necreate primesc tema la activare, prin <c>ThemeManager.Apply</c>.
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

            For Each page As IOrdPage In _pages.Values
                Dim themed As IThemedControl = TryCast(page, IThemedControl)
                themed?.ApplyTheme(scheme)
            Next

            ' Culorile nodurilor (roșul valorilor negative) se re-iau din noua paletă. Arborele
            ' se reconstruiește, deci selecția se pierde — paginile rămân cum sunt până la
            ' următorul click pe un nod.
            If _ordonantari IsNot Nothing AndAlso _ordonantari.Count > 0 Then
                BuildTree(_ordonantari)
            End If
        Catch ex As Exception
            ' Boundary UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("OrdView.ApplyTheme", ex)
        End Try
    End Sub

End Class

''' <summary>
''' Ce acoperă un nod din arborele ORD: liniile pe care le arată grila, dacă nodul e o
''' rădăcină de lună (o lună nu are UN singur document) și — pentru frunze — ordonanțarea
''' însăși, din care se compune calea PDF-ului. POCO -&gt; fără Try/Catch.
''' </summary>
Friend NotInheritable Class OrdNodePayload
    Public ReadOnly Property Linii As List(Of OrdLinieRow)
    Public ReadOnly Property IsRoot As Boolean
    ''' <summary>Ordonanțarea frunzei; Nothing pe o rădăcină de lună.</summary>
    Public ReadOnly Property Ordonantare As OrdHeaderRow

    Public Sub New(linii As List(Of OrdLinieRow), isRoot As Boolean,
                   Optional ordonantare As OrdHeaderRow = Nothing)
        Me.Linii = If(linii, New List(Of OrdLinieRow)())
        Me.IsRoot = isRoot
        Me.Ordonantare = ordonantare
    End Sub
End Class
