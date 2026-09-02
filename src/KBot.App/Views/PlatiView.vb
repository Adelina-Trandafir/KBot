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
''' Vederea Plăți (felia 0017) — echivalentul Access frmFX_MAIN_PLATI: un arbore pe DOUĂ
''' niveluri la stânga (folder lună/an -> plata însăși, etichetată cu ziua ei), o grilă
''' continuă LISTA sus-dreapta și un panou de detaliu jos-dreapta cu extrasul bancar.
''' Read-only în această felie. Datele vin din GET /api/forexe/plati, întotdeauna prin plasa
''' de re-autentificare a shell-ului (401 -> re-login -> reia o dată). Click pe orice nod
''' (lună / plată) FILTREAZĂ grila la rândurile nodului (nu agregă — spre deosebire de
''' Recepții). Selectarea unui rând din grilă umple panoul de detaliu din datele deja pe rând
''' (fără al doilea apel).
'''
''' Cele două niveluri sunt exact cele construite de Access în Show_Plati: rădăcina «m_»&LunaAn
''' și frunza «d_»&Data, o frunză per înregistrare FX_Plati. Nivelul de zi agregată din felia
''' 0017 a fost scos — Access nu l-a avut niciodată.
''' </summary>
Public Class PlatiView
    Implements IAngajamentView, IThemedControl

    ' Cheile coloanelor grilei — o singură definiție, folosită la creare și la umplere.
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_PLATITOR As String = "platitor"
    Private Const COL_NRDOC As String = "nrdoc"
    Private Const COL_DATA As String = "data"
    Private Const COL_SUMA As String = "suma"

    ' Cheile iconițelor din «image_list» (ImageList-ul autorat în designer, legat prin
    ' tree.NodeImages). Arborele desenează DIN ELE; formele GDI rămân doar ca plasă pentru
    ' cheile care încă nu există în listă (starea neutră și «+»).
    Private Const ICO_LUNA As String = "month"      ' folderul de lună
    Private Const ICO_SUS As String = "up"          ' plată încărcată (Access REV_SUS)
    Private Const ICO_JOS As String = "down"        ' plată preluată (Access REV_JOS)

    ' Format românesc: separator de mii «.» și zecimală «,» (1.091.940,00).
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    ' Plasa 401 a shell-ului (MainForm.WithReauth), specializată pe PlatiInfo.
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of PlatiInfo)), Task(Of PlatiInfo))

    ' Codul angajamentului CERUT ultima dată — stale-guard (identic cu Recepții/Rezervări).
    Private _requestedCod As String

    ' Ultimele plăți încărcate — păstrate ca ApplyTheme să reconstruiască arborele
    ' (re-tintarea iconițelor) fără o nouă cerere de rețea.
    Private _rows As List(Of PlataRow)

    ' Starea splitter-ului dinainte de strângerea arborelui, ca desfacerea să-l pună înapoi
    ' exact unde era (vezi tree_CollapsedChanged). 0 = arborele n-a fost încă strâns.
    Private _splitterDistanceDesfasurat As Integer
    Private _panel1MinSizeDesfasurat As Integer

    ''' <summary>«+» apăsat pe rădăcina unei luni (nivel 0) — oglindește
    ''' RaiseEvent AdaugareOrdonantari(LunaAn), pe care Access il prinde in
    ''' <c>fxPlati_AdaugareOrdonantari</c>. Abonatul de azi e chiar aceasta vedere: il
    ''' traduce in <c>OrdComanda.LotPeLuna</c> si il da shell-ului. Ramane public fiindca e
    ''' semnalul brut al arborelui — un alt abonat (teste, o gazda viitoare) il poate asculta
    ''' fara sa treaca prin comanda.</summary>
    Public Event AdaugaOrdonantariCerut(sender As Object, e As LunaAnEventArgs)

    ''' <summary>«+» apasat pe o zi (nivel 1) — oglindeste RaiseEvent
    ''' AdaugareOrdonantare(IdPlataFX, DataPlata), pe care Access il prinde in
    ''' <c>fxPlati_AdaugareOrdonantare</c>. Vezi nota de mai sus.</summary>
    Public Event AdaugaOrdonantareCerut(sender As Object, e As PlataOrdEventArgs)

    ' Comanda de ordonantare, data shell-ului (MainForm.ExecutaComandaOrd). Nothing = vederea
    ' a fost construita fara ea (teste headless, gazde vechi) — atunci «+» doar ridica
    ' evenimentele de mai sus, ca inainte de felia asta, fara no-op tacut si fara sa cada.
    Private ReadOnly _executaComandaOrd As Action(Of OrdComanda)

    Public Sub New(apiClient As IApiClient, withReauth As Func(Of Func(Of Task(Of PlatiInfo)), Task(Of PlatiInfo)),
                   Optional executaComandaOrd As Action(Of OrdComanda) = Nothing)
        ArgumentNullException.ThrowIfNull(apiClient)
        ArgumentNullException.ThrowIfNull(withReauth)
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        _executaComandaOrd = executaComandaOrd
        BuildDetailRows()
        ShowEmpty("Selectați un angajament din arbore.")
    End Sub

    ''' <summary>
    ''' Reincarca platile angajamentului curent. Portul lui
    ''' <c>fxPlati.RefreshPlati CodAngajament, True</c> din <c>frmFX_MAIN</c>: dupa ce s-a
    ''' scris o ordonantare, platile ei nu mai sunt neordonantate, deci «+»-ul sta pe o zi
    ''' gresita si starea nodurilor minte. Fara angajament curent nu face nimic.
    ''' </summary>
    Public Sub Reincarca()
        Try
            If String.IsNullOrWhiteSpace(_requestedCod) Then Return
            ShowEmpty("Se încarcă plățile…")
            ' Fire-and-forget deliberat, ca in SetContext: LoadAsync isi trateaza singura
            ' TOATE erorile.
            LoadAsync(_requestedCod)
        Catch ex As Exception
            GlobalErrorLog.Write("PlatiView.Reincarca", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Pune cele zece perechi etichetă/valoare în <c>detailTable</c>. Stă AICI, nu în
    ''' <c>InitializeComponent</c>: apelurile au trăit acolo până când o deschidere a vederii în
    ''' designerul Visual Studio a REGENERAT metoda și le-a șters — controalele rămâneau create
    ''' și denumite, dar neatașate tabelului, deci panoul de detaliu ieșea gol. Orice cod scris
    ''' de mână în <c>InitializeComponent</c> se pierde la primul dute-vino prin designer;
    ''' constructorul e singurul loc pe care designerul nu-l rescrie.
    ''' </summary>
    Private Sub BuildDetailRows()
        Try
            If detailTable.Controls.Count > 0 Then Return   ' idempotent
            InitDetailPair(capNrDoc, "Nr. document", valNrDoc, 0)
            InitDetailPair(capDataBanca, "Data bancă", valDataBanca, 1)
            InitDetailPair(capDataDoc, "Data document", valDataDoc, 2)
            InitDetailPair(capReferinta, "Referință", valReferinta, 3)
            InitDetailPair(capPlatitor, "Plătitor", valPlatitor, 4)
            InitDetailPair(capCui, "CUI", valCui, 5)
            InitDetailPair(capIban, "IBAN", valIban, 6)
            InitDetailPair(capDebit, "Sumă debit", valDebit, 7)
            InitDetailPair(capCredit, "Sumă credit", valCredit, 8)
            InitDetailPair(capExplicatii, "Explicații", valExplicatii, 9)
        Catch ex As Exception
            GlobalErrorLog.Write("PlatiView.BuildDetailRows", ex)
            Throw
        End Try
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "plati"
        End Get
    End Property

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
            GlobalErrorLog.Write("PlatiView.tree_CollapsedChanged", ex)
        End Try
    End Sub

    ' Distanța splitter-ului adusă în intervalul acceptat de SplitContainer — o vedere îngustă
    ' n-are voie să transforme apăsarea butonului de strângere într-o excepție.
    Private Function ClampSplitter(dorit As Integer) As Integer
        Dim maxim As Integer = split.Width - split.Panel2MinSize - split.SplitterWidth
        If maxim < split.Panel1MinSize Then Return split.Panel1MinSize
        Return Math.Max(split.Panel1MinSize, Math.Min(dorit, maxim))
    End Function

    ' Coloanele grilei (= frmFX_MAIN_PLATI_LISTA: Clasificație, Plătitor, Nr. doc, Data plății,
    ' Suma) sunt AUTORATE ÎN DESIGNER, nu construite aici. Un BuildColumns() care le adăuga a
    ' doua oară arunca «Cheie de coloană duplicată: 'clsf'» din constructor și dobora activarea
    ' vederii. Cheile COL_* de mai sus trebuie să rămână identice cu cele din designer — ele
    ' sunt singura legătură dintre coloanele autorate și FillGrid.

    ''' <summary>
    ''' Selecția din arbore s-a schimbat. Fără angajament (nod de capitol / deselectare) NU se
    ''' face niciun apel de rețea — doar se golește vederea.
    ''' </summary>
    Public Sub SetContext(info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        Try
            Dim cod As String = info?.CodAngajament
            If String.IsNullOrWhiteSpace(cod) Then
                _requestedCod = Nothing
                _rows = Nothing
                tree.Clear()
                grid.ClearRows()
                ShowEmpty("Selectați un angajament din arbore.")
                Return
            End If

            _requestedCod = cod
            ShowEmpty("Se încarcă plățile…")
            ' Fire-and-forget deliberat (handler sincron al shell-ului): metoda își tratează
            ' singură TOATE erorile — vezi comentariul din ReceptiiView/RezervariView.
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("PlatiView.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Boundary UI: logăm și ARĂTĂM eroarea, nu o aruncăm mai departe (apelul e pornit fără
    ' await din SetContext, deci nu există cine să o prindă).
    Private Async Sub LoadAsync(cod As String)
        Try
            Dim data As PlatiInfo = Await _withReauth(
                Function() _apiClient.GetPlatiAsync(cod, CancellationToken.None)).ConfigureAwait(True)

            ' Răspuns depășit: între timp operatorul a selectat alt angajament. Îl aruncăm.
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return

            Dim rows As List(Of PlataRow) = If(data Is Nothing, New List(Of PlataRow)(), data.Plati)
            If rows Is Nothing OrElse rows.Count = 0 Then
                _rows = Nothing
                tree.Clear()
                grid.ClearRows()
                ShowEmpty("Angajamentul nu are plăți.")
                Return
            End If

            _rows = rows
            BuildTree(rows)
            ' Nimic selectat -> grila e goală; se umple la click pe orice nod al arborelui.
            grid.ClearRows()
            ShowContent()
        Catch ex As ApiException
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("PlatiView.LoadAsync", ex)
            _rows = Nothing
            tree.Clear()
            grid.ClearRows()
            ShowEmpty(ex.Message)   ' mesaj românesc din câmpul «error» al serverului
        Catch ex As Exception
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("PlatiView.LoadAsync", ex)
            _rows = Nothing
            tree.Clear()
            grid.ClearRows()
            ShowEmpty("Plățile nu au putut fi încărcate. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' ── Arborele ─────────────────────────────────────────────────────────────
    ' DOUĂ niveluri: un folder per lună (SUM lună) -> o frunză per ZI, care strânge TOATE
    ' plățile zilei într-un singur nod (SUM zi). Nodul de plată individuală din felia 0017 a
    ' fost scos — arborele se oprește la zi.
    ' Fiecare nod poartă în Tag rândurile lui, ca un click să FILTREZE grila fără o nouă cerere.
    '
    ' Iconițe — din «image_list» (autorat în designer, legat prin tree.NodeImages):
    '   * luna -> «month» (folderul; Access: FolderClosed/FolderOpen — NU o stare merjată,
    '     de aceea luna nu se colorează și nu poartă iconiță de stare);
    '   * ziua -> starea MERJATĂ a plăților ei: ORICE Incarcat -> «up», altfel ORICE Preluat
    '     -> «down», altfel neutru (Access: REV_SUS / REV_JOS / REV_NOT, acolo per plată —
    '     merjarea e a noastră, fiindcă frunza noastră e ziua, nu plata).
    ' Starea neutră (Access REV_NOT) N-ARE încă cheie în «image_list», deci cade pe forma GDI.
    ' Colorare INCASARE (verde din paletă): pe zi, doar dacă TOATE plățile ei sunt încasări.
    '
    ' «+» (iconița din dreapta): îl primește cea mai veche zi ne-ordonantată, iar luna care o
    ' conține îl primește și ea — Access: cLeaf.IconRight urmat de cLeaf.ParentNode.IconRight.
    ' Rădăcinile stau STRÂNSE; se deschide doar luna care poartă «+» (cerință operator — în
    ' Access toate erau Expanded = False).
    Private Sub BuildTree(rows As List(Of PlataRow))
        Try
            tree.Clear()
            Dim palette As ThemePalette = TryGetPalette()

            ' Cea mai veche zi cu cel puțin o plată ne-ordonantată -> «+» pe ea (o singură zi).
            Dim plusDay As Date? = OldestUnordonantatDay(rows)
            Dim monthIcon As Image = LunaIcon()

            ' Foldere de lună, cronologic.
            Dim monthGroups = rows.GroupBy(Function(r) MonthKeyOf(r.DataPlata)).
                                   OrderBy(Function(g) g.Key)

            For Each mg In monthGroups
                Dim monthRows As List(Of PlataRow) = mg.ToList()
                Dim monthSum As Double = monthRows.Sum(Function(r) r.Suma)
                Dim monthContainsPlus As Boolean = monthRows.Any(Function(r) SameDay(r.DataPlata, plusDay))
                Dim monthPlus As Image = If(monthContainsPlus, PlusIcon(palette), Nothing)

                Dim monthItem As AdvancedTreeControl.TreeItem =
                    tree.AddItem($"m_{mg.Key}", $"{MonthLabel(mg.Key Mod 100)}~~~{Money(monthSum)}",
                                 pLeftIconClosed:=monthIcon, pLeftIconOpen:=monthIcon,
                                 pRightIcon:=monthPlus, pExpanded:=monthContainsPlus)
                monthItem.Tag = monthRows
                monthItem.Bold = True
                ' Frunze = ZIUA (toate plățile ei într-un singur nod), cronologic.
                Dim dayGroups = monthRows.GroupBy(Function(r) DayKeyOf(r.DataPlata)).
                                          OrderBy(Function(g) g.Key)
                For Each dg In dayGroups
                    Dim dayRows As List(Of PlataRow) = dg.ToList()
                    Dim daySum As Double = dayRows.Sum(Function(r) r.Suma)
                    Dim dayIsPlus As Boolean = plusDay.HasValue AndAlso dg.Key = plusDay.Value.Date
                    Dim dayIcon As Image = StareIconOf(MergedStare(dayRows), palette)
                    Dim dayPlus As Image = If(dayIsPlus, PlusIcon(palette), Nothing)

                    Dim dayItem As AdvancedTreeControl.TreeItem =
                        tree.AddItem($"d_{dg.Key:yyyyMMdd}", $"{ShortDate(dg.Key)}~~~{Money(daySum)}",
                                     monthItem, pLeftIconClosed:=dayIcon, pLeftIconOpen:=dayIcon,
                                     pRightIcon:=dayPlus)
                    dayItem.Tag = dayRows
                    If AllIncasare(dayRows) AndAlso palette IsNot Nothing Then
                        dayItem.NodeForeColor = palette.SuccessColor
                    End If
                Next
            Next

            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("PlatiView.BuildTree", ex)
            Throw
        End Try
    End Sub

    ' Cea mai veche zi (Min DataPlata) care conține cel puțin o plată cu AreOrd = False.
    ' Nothing dacă toate sunt deja ordonantate. Oglindește snapshot-ul TOP 1 din Show_Plati.
    Private Shared Function OldestUnordonantatDay(rows As List(Of PlataRow)) As Date?
        Dim eligible = rows.Where(Function(r) r.DataPlata.HasValue AndAlso Not r.AreOrd).
                            Select(Function(r) r.DataPlata.Value.Date)
        If Not eligible.Any() Then Return Nothing
        Return eligible.Min()
    End Function

    ' ── Grila (LISTA) — FILTRU, nu agregat ───────────────────────────────────
    ' Click pe orice nod -> grila arată EXACT rândurile nodului. Fiecare rând poartă în Tag
    ' PlataRow, ca selectarea unui rând să umple panoul de detaliu fără un alt apel.
    Private Sub FillGrid(rows As List(Of PlataRow))
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            If rows IsNot Nothing Then
                For Each r As PlataRow In rows
                    Dim row As KBotDataRow = grid.AddRow()
                    row.Tag = r
                    row(COL_CLSF) = r.ClsfEfectiv
                    row(COL_PLATITOR) = If(r.Extras IsNot Nothing, r.Extras.PlatitorNume, String.Empty)
                    row(COL_NRDOC) = r.NrOP
                    row(COL_DATA) = ShortDate(r.DataPlata)
                    row(COL_SUMA) = r.Suma
                Next
            End If
        Finally
            grid.EndUpdate()
        End Try
        ' Nimic selectat imediat după umplere (ClearRows nu ridică SelectionChanged).
        ShowDetailMessage("Selectați o plată.")
    End Sub

    ' Click pe orice nod -> filtrează grila la rândurile nodului (în Tag). Fără apel de rețea.
    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing Then Return
            Dim rows As List(Of PlataRow) = TryCast(pNode.Tag, List(Of PlataRow))
            If rows Is Nothing Then Return
            FillGrid(rows)
        Catch ex As Exception
            GlobalErrorLog.Write("PlatiView.tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' Click pe iconita «+» -> ordonantarea platilor nodului (mcTree_RightIconClick din Access).
    ' Pe doua niveluri, exact ca acolo:
    '   nivel 0 (luna) -> AdaugaOrdonantariCerut(LunaAn) -> FX_Adaugare_ORD_Din_Plati_Batch,
    '                     adica OrdComanda.LotPeLuna: se genereaza SI SE SALVEAZA, fara editor,
    '                     cate o ordonantare pentru fiecare zi a lunii cu plati neordonantate;
    '   nivel 1 (ziua)  -> AdaugaOrdonantareCerut(-1, data) -> FX_Adaugare_ORD_Din_Plati cu
    '                     vIdPlataFX = -1, adica OrdComanda.DinPlati fara plata anume: se
    '                     genereaza graful (nimic scris) si SE DESCHIDE editorul.
    ' Ramura Access de nivel 2 (o plata anume) n-are nod care s-o ridice cat timp frunza e ziua;
    ' de-asta comanda de zi pleaca mereu cu IdPlataFx = Nothing.
    '
    ' Evenimentele raman ridicate INAINTE de comanda — ele sunt semnalul brut al arborelui, iar
    ' un abonat (test, gazda viitoare) trebuie sa-l vada chiar daca shell-ul n-a dat comanda.
    Private Sub Tree_RightIconClicked(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            If pNode Is Nothing Then Return
            Dim rows As List(Of PlataRow) = TryCast(pNode.Tag, List(Of PlataRow))
            If rows Is Nothing OrElse rows.Count = 0 Then Return
            If String.IsNullOrWhiteSpace(_requestedCod) Then Return

            Select Case pNode.Level
                Case 0
                    RaiseEvent AdaugaOrdonantariCerut(Me, New LunaAnEventArgs(LunaAnOf(rows(0))))
                    Dim luna As Date? = PrimaDataDin(rows)
                    ' Gruparea nodului e chiar luna, deci prima data din el o numeste. O luna
                    ' fara nicio data (grupul «fara data», MonthKeyOf = 0) n-are ce lot sa ceara.
                    If luna.HasValue Then
                        CereComandaOrd(OrdComanda.LotPeLuna(_requestedCod, luna.Value.Month, luna.Value.Year))
                    End If
                Case 1
                    Dim zi As Date? = PrimaDataDin(rows)
                    RaiseEvent AdaugaOrdonantareCerut(Me, New PlataOrdEventArgs(-1, If(zi, Date.MinValue)))
                    If zi.HasValue Then
                        CereComandaOrd(OrdComanda.DinPlati(_requestedCod, zi.Value))
                    End If
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("PlatiView.tree_RightIconClicked", ex)
        End Try
    End Sub

    ' Prima data reala din randurile unui nod (nodurile sunt grupate pe luna/zi, deci oricare
    ' rand cu data o numeste). Nothing pentru grupul «fara data».
    Private Shared Function PrimaDataDin(rows As List(Of PlataRow)) As Date?
        For Each r As PlataRow In rows
            If r.DataPlata.HasValue Then Return r.DataPlata.Value.Date
        Next
        Return Nothing
    End Function

    ' Da comanda shell-ului, daca vederea a primit una. Fara shell (teste headless) raman doar
    ' evenimentele — si se spune de ce, ca sa nu treaca drept no-op tacut.
    Private Sub CereComandaOrd(comanda As OrdComanda)
        If _executaComandaOrd Is Nothing Then Return
        _executaComandaOrd(comanda)
    End Sub

    ' ── Panoul de detaliu (extrasul bancar) ──────────────────────────────────
    ' Condus de selecția din grilă, din datele deja pe rând. Fără al doilea apel de rețea.
    Private Sub Grid_SelectionChanged(sender As Object, e As EventArgs) Handles grid.SelectionChanged
        Try
            Dim cur As KBotDataRow = grid.CurrentRow
            Dim r As PlataRow = If(cur Is Nothing, Nothing, TryCast(cur.Tag, PlataRow))
            UpdateDetail(r)
        Catch ex As Exception
            GlobalErrorLog.Write("PlatiView.grid_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub UpdateDetail(r As PlataRow)
        If r Is Nothing Then
            ShowDetailMessage("Selectați o plată.")
            Return
        End If
        If r.Extras Is Nothing Then
            ShowDetailMessage("Fără extras bancar asociat.")
            Return
        End If

        Dim ex As ExtrasBancar = r.Extras
        valNrDoc.Text = ex.NrDoc
        valDataBanca.Text = ShortDate(ex.DataBanca)
        valDataDoc.Text = ex.DataDoc
        valReferinta.Text = ex.Referinta
        valPlatitor.Text = ex.PlatitorNume
        valCui.Text = ex.PlatitorCui
        valIban.Text = ex.PlatitorIban
        valDebit.Text = Money(ex.SumaDebit)
        valCredit.Text = Money(ex.SumaCredit)
        valExplicatii.Text = ex.Explicatii

        lblDetailMessage.Visible = False
        detailTable.Visible = True
    End Sub

    Private Sub ShowDetailMessage(message As String)
        lblDetailMessage.Text = message
        detailTable.Visible = False
        lblDetailMessage.Visible = True
    End Sub

    ' ── Stare goală / conținut ───────────────────────────────────────────────
    Private Sub ShowEmpty(message As String)
        lblEmpty.Text = message
        lblEmpty.Visible = True
        split.Visible = False
    End Sub

    Private Sub ShowContent()
        lblEmpty.Visible = False
        split.Visible = True
        ShowDetailMessage("Selectați o plată.")
    End Sub

    ' ── Formatare / iconițe ──────────────────────────────────────────────────
    Private Shared Function Money(value As Double) As String
        Return value.ToString("N2", _roCulture)
    End Function

    ' Data scurtă în format românesc (dd.MM.yyyy). Nothing -> gol.
    Private Shared Function ShortDate(value As Date?) As String
        If Not value.HasValue Then Return String.Empty
        Return value.Value.ToString("dd.MM.yyyy", _roCulture)
    End Function

    Private Shared Function ShortDate(value As Date) As String
        Return value.ToString("dd.MM.yyyy", _roCulture)
    End Function

    ' Cheia de lună = an*100 + lună din DataPlata (0 dacă lipsește). Ordonabilă cronologic.
    Private Shared Function MonthKeyOf(value As Date?) As Integer
        If Not value.HasValue Then Return 0
        Return value.Value.Year * 100 + value.Value.Month
    End Function

    ' Cheia de zi (data fără oră). Fără dată -> Date.MinValue (edge; grupul „(fără dată)").
    Private Shared Function DayKeyOf(value As Date?) As Date
        Return If(value.HasValue, value.Value.Date, Date.MinValue)
    End Function

    ' Numele lunii în română (Ianuarie…), cu prima literă mare (ca în Recepții/Rezervări).
    Private Shared Function MonthLabel(month As Integer) As String
        If month < 1 OrElse month > 12 Then Return CStr(month)
        Dim name As String = _roCulture.DateTimeFormat.GetMonthName(month)
        If String.IsNullOrEmpty(name) Then Return CStr(month)
        Return Char.ToUpper(name(0), _roCulture) & name.Substring(1)
    End Function

    ' LunaAn în formatul Access (Month/Year, ex. „1/2026") pentru evenimentul de ordonantare.
    Private Shared Function LunaAnOf(r As PlataRow) As String
        If Not r.DataPlata.HasValue Then Return String.Empty
        Return $"{r.DataPlata.Value.Month}/{r.DataPlata.Value.Year}"
    End Function

    ' NOTA: `DayOf(r)` a disparut aici — singurul lui apel, din Tree_RightIconClicked, a trecut
    ' pe `PrimaDataDin(rows)`, care scaneaza randurile pana gaseste una cu data in loc sa se
    ' bazeze pe primul rand si sa cada pe Date.MinValue cand acela n-are.

    Private Shared Function SameDay(a As Date?, b As Date?) As Boolean
        Return a.HasValue AndAlso b.HasValue AndAlso a.Value.Date = b.Value.Date
    End Function

    ' Starea merjată a unei zile: ORICE sus -> sus, altfel ORICE jos -> jos, altfel neutru.
    Private Shared Function MergedStare(rows As List(Of PlataRow)) As PlatiIcons.Stare
        If rows.Any(Function(r) r.Incarcat) Then Return PlatiIcons.Stare.Sus
        If rows.Any(Function(r) r.Preluat) Then Return PlatiIcons.Stare.Jos
        Return PlatiIcons.Stare.Neutru
    End Function

    ' Ziua e verde doar dacă TOATE plățile ei sunt încasări.
    Private Shared Function AllIncasare(rows As List(Of PlataRow)) As Boolean
        Return rows.Count > 0 AndAlso rows.All(Function(r) r.EsteIncasare)
    End Function

    ' Iconița de stare, luată din «image_list»: «up» = încărcată, «down» = preluată.
    ' Starea neutră n-are cheie în listă (și nici «up»/«down» nu sunt garantate), deci rămâne
    ' plasa GDI, care se re-tintează pe paletă — imaginile din listă sunt fixe.
    Private Function StareIconOf(stare As PlatiIcons.Stare, palette As ThemePalette) As Image
        Dim cheie As String = String.Empty
        Select Case stare
            Case PlatiIcons.Stare.Sus : cheie = ICO_SUS
            Case PlatiIcons.Stare.Jos : cheie = ICO_JOS
        End Select

        Dim dinLista As Image = If(cheie.Length = 0, Nothing, tree.NodeImage(cheie))
        If dinLista IsNot Nothing Then Return dinLista

        If palette Is Nothing Then Return Nothing
        Dim color As Color
        Select Case stare
            Case PlatiIcons.Stare.Sus : color = palette.SuccessColor
            Case PlatiIcons.Stare.Jos : color = palette.AccentColor
            Case Else : color = palette.TextDimColor
        End Select
        Return PlatiIcons.StatusIcon(stare, color, tree.LeftIconSize.Width)
    End Function

    ''' <summary>Iconița folderului de lună; doar din «image_list» (n-are formă GDI).</summary>
    Private Function LunaIcon() As Image
        Return tree.NodeImage(ICO_LUNA)
    End Function

    ' Iconița «+» (accent din paletă — Access folosește doar „Plus" pentru plăți).
    Private Function PlusIcon(palette As ThemePalette) As Image
        If palette Is Nothing Then Return Nothing
        Return PlatiIcons.PlusIcon(palette.AccentColor, tree.RightIconSize.Width)
    End Function

    Private Shared Function TryGetPalette() As ThemePalette
        ' Headless (teste) sau înainte de inițializarea temei: ThemeManager.Current poate fi
        ' Nothing. Atunci arborele se construiește fără iconițe/culori (structura e aceeași),
        ' iar ApplyTheme reconstruiește când tema devine disponibilă.
        Dim current As ThemeScheme = ThemeManager.Current
        Return current?.Palette
    End Function

    ''' <summary>
    ''' Reaplică culorile schemei pe arbore + panoul de detaliu + starea goală (grila se
    ''' auto-temează: KBotDataView implementează el însuși IThemedControl). Reconstruiește
    ''' arborele dacă are date, ca iconițele să se re-tinteze pe noua paletă.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            split.BackColor = p.SurfaceAltColor
            split.Panel1.BackColor = p.SurfaceAltColor
            split.Panel2.BackColor = p.SurfaceAltColor
            innerSplit.BackColor = p.SurfaceAltColor
            innerSplit.Panel1.BackColor = p.SurfaceAltColor
            innerSplit.Panel2.BackColor = p.SurfaceAltColor

            ' Arborele e IThemedControl: își ia singur paleta, iar ThemeManager nu mai recurge
            ' în copiii lui. Culorile puse în designer câștigă; cele lăsate goale urmează tema.

            ' Panoul de detaliu: fundal de suprafață, etichete estompate, valori pline.
            detailPane.BackColor = p.SurfaceAltColor
            detailTable.BackColor = p.SurfaceAltColor
            For Each cap As Label In New Label() {capNrDoc, capDataBanca, capDataDoc, capReferinta,
                                                  capPlatitor, capCui, capIban, capDebit, capCredit, capExplicatii}
                cap.ForeColor = p.TextDimColor
                cap.BackColor = Color.Transparent
            Next
            For Each val As Label In New Label() {valNrDoc, valDataBanca, valDataDoc, valReferinta,
                                                  valPlatitor, valCui, valIban, valDebit, valCredit, valExplicatii}
                val.ForeColor = p.TextColor
                val.BackColor = Color.Transparent
            Next
            lblDetailMessage.ForeColor = p.TextDimColor
            lblDetailMessage.BackColor = p.SurfaceAltColor

            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor

            ' Re-tintarea iconițelor pe noua paletă (grila rămâne golită — LISTA se reface la
            ' următorul click pe un nod).
            If _rows IsNot Nothing AndAlso _rows.Count > 0 Then
                BuildTree(_rows)
            End If
        Catch ex As Exception
            ' Boundary UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("PlatiView.ApplyTheme", ex)
        End Try
    End Sub

End Class

''' <summary>Argumentul evenimentului «+» pe rădăcina unei luni (nivel 0). Oglindește
''' RaiseEvent AdaugareOrdonantari(LunaAn) din Access. POCO -> fără Try/Catch.</summary>
Public NotInheritable Class LunaAnEventArgs
    Inherits EventArgs
    Public ReadOnly Property LunaAn As String
    Public Sub New(lunaAn As String)
        Me.LunaAn = If(lunaAn, String.Empty)
    End Sub
End Class

''' <summary>Argumentul evenimentului «+» pe o zi (IdPlataFx = -1) sau o plată (IdPlataFx real).
''' Oglindește RaiseEvent AdaugareOrdonantare(IdPlataFX, DataPlata) din Access. POCO.</summary>
Public NotInheritable Class PlataOrdEventArgs
    Inherits EventArgs
    Public ReadOnly Property IdPlataFx As Integer
    Public ReadOnly Property DataPlata As Date
    Public Sub New(idPlataFx As Integer, dataPlata As Date)
        Me.IdPlataFx = idPlataFx
        Me.DataPlata = dataPlata
    End Sub
End Class
