Option Strict On
Imports System.Globalization
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Vederea Recepții (felia 0015) — echivalentul Access frmFX_MAIN_REC: un master/detail
''' cu un arbore de recepții la stânga (rădăcina «Toate recepțiile» -> folder lună/an ->
''' recepția R / IDRR) și o grilă continuă la dreapta (LISTA) cu detaliul pe clasificații.
''' Read-only în această felie. Datele vin din GET /api/forexe/receptii, întotdeauna prin
''' plasa de re-autentificare a shell-ului (401 -> re-login -> reia o dată).
'''
''' <para><b>Regula din felia 0065 (operator, 17.09.2026): o recepție VALOREAZĂ ultimul ei
''' instantaneu.</b> Lanțul unei recepții e un șir de instantanee (anteturi H) în ordinea
''' DataH, fiecare cu VALOAREA ÎNTREGII recepții la acel moment (F3). Deci ce e adevărat
''' acum despre o recepție e ULTIMUL antet: totalul lui e totalul recepției, liniile lui
''' sunt indicatorii ei. Click pe ORICE nod umple grila cu un rând per clasificație din
''' ultimul antet al fiecărei recepții a nodului — NU un agregat peste tot lanțul, care ar
''' aduna de mai multe ori aceeași sumă. Tooltip de reconciliere recepții/plăți pe rădăcină,
''' pe folderele de lună ȘI pe recepții (revizuire operator 2026-07-22), cumulul fiind tot
''' suma totalurilor ultimului antet — nu suma DIFH-urilor, care e NULL pe un antet așezat
''' din editorul de legături înainte de 0065 și pierdea recepția din total.</para>
'''
''' Din felia 0062 arborele mai are un dosar, «Instantanee neașezate», pentru anteturile
''' fără recepție (H.IDRR NULL) — și o iconiță în stânga subsolului care reface din istoric
''' anteturile și liniile lipsă.
''' </summary>
Public Class ReceptiiView
    Implements IAngajamentView, IThemedControl

    ' Cheile coloanelor grilei — o singură definiție, folosită la creare și la umplere,
    ' ca un typo să nu ajungă o coloană goală în producție.
    Private Const COL_NRCRT As String = "nrcrt"
    Private Const COL_DESCRIERE As String = "descriere"
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_VALOARE As String = "valoare"

    ' CHEILE ICONIȚELOR din «image_list» (ImageList-ul pus pe vedere în designer și legat de
    ' arbore prin tree.NodeImages), ca în RezervariView: arborele rezolvă o cheie prin
    ' AdvancedTreeControl.NodeImage, iar o cheie lipsă întoarce Nothing. Ținute ca și
    ' constante ca un typo să nu ajungă un nod fără iconiță în producție.
    Private Const ICO_LUNA As String = "month"      ' folderul de lună
    Private Const ICO_SUS As String = "up"          ' recepție cu valoare pozitivă ▲
    Private Const ICO_JOS As String = "down"        ' recepție cu valoare negativă ▼

    ' Cheia nodului-dosar al anteturilor NEAȘEZATE (felia 0062). Friend pentru teste.
    Friend Const UNPLACED_KEY As String = "unplaced"

    ' Cheia rădăcinii «Toate recepțiile» (felia 0065). Friend pentru teste.
    Friend Const ROOT_KEY As String = "all"

    ' Format românesc: separator de mii «.» și zecimală «,» (1.091.940,00).
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    ' Plasa 401 a shell-ului (MainForm.WithReauth), specializată pe ReceptiiInfo:
    ' politica de re-login rămâne într-un singur loc, vederea doar o folosește.
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of ReceptiiInfo)), Task(Of ReceptiiInfo))

    ' Deschiderea editorului de legături R ▸ H (felia 0048-04). Vine de la shell fiindcă
    ' formularul are nevoie de plasa de re-autentificare pe DOUĂ forme de răspuns, iar
    ' politica aia trăiește într-un singur loc, în MainForm.
    '
    ' Nothing = gazda nu îl oferă. Atunci iconița din antet se STINGE, nu rămâne un buton
    ' care nu face nimic: un no-op tăcut e mai rău decât un buton lipsă.
    Private ReadOnly _deschideLegaturi As Action(Of String)

    ' Codul angajamentului CERUT ultima dată — vezi stale-guard din LoadAsync (identic cu
    ' Sumar/Rezervări): operatorul parcurge arborele rapid, iar un răspuns depășit se aruncă.
    Private _requestedCod As String

    ' Ultimele date încărcate — păstrate ca ApplyTheme să reconstruiască arborele
    ' (re-tintarea iconițelor) fără o nouă cerere de rețea. `_plati` alimentează
    ' tooltip-ul de recepție (felia 0015-02).
    Private _rows As List(Of ReceptieRow)
    Private _plati As List(Of ReceptiePlata)

    ' Starea splitter-ului dinainte de strângerea arborelui, ca desfacerea să-l pună înapoi
    ' exact unde era (vezi tree_CollapsedChanged). 0 = arborele n-a fost încă strâns.
    Private _splitterDistanceDesfasurat As Integer
    Private _panel1MinSizeDesfasurat As Integer

    ''' <summary>
    ''' Reîmprospătarea recepțiilor din FOREXE (felia 0060) — iconița din DREAPTA subsolului
    ''' arborelui. Vine de la shell din același motiv ca <see cref="_deschideLegaturi"/>:
    ''' pornește robotul și duce pachetul prin ingestie, iar amândouă cer plasa de
    ''' re-autentificare, care trăiește într-un singur loc.
    '''
    ''' <para>Nothing = gazda nu o oferă. Atunci iconița se STINGE, nu rămâne un buton care nu
    ''' face nimic — un no-op tăcut e mai rău decât un buton lipsă.</para>
    ''' </summary>
    Private ReadOnly _reimprospateaza As Action(Of String)

    ''' <summary>
    ''' Refacerea instantaneelor și liniilor LIPSĂ din istoric (felia 0062) — iconița din
    ''' STÂNGA subsolului arborelui. Vine de la shell din același motiv ca celelalte două:
    ''' apelul are nevoie de plasa de re-autentificare pe o formă de răspuns proprie
    ''' (<see cref="ReceptiiRebuildResult"/>), iar politica aia trăiește într-un singur loc.
    '''
    ''' <para>Nothing = gazda nu o oferă. Atunci iconița se STINGE, nu rămâne un buton care nu
    ''' face nimic.</para>
    ''' </summary>
    Private ReadOnly _rebuildMissing As Action(Of String)

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of ReceptiiInfo)), Task(Of ReceptiiInfo)),
                   Optional deschideLegaturi As Action(Of String) = Nothing,
                   Optional reimprospateaza As Action(Of String) = Nothing,
                   Optional rebuildMissing As Action(Of String) = Nothing)
        If apiClient Is Nothing Then Throw New ArgumentNullException(NameOf(apiClient))
        If withReauth Is Nothing Then Throw New ArgumentNullException(NameOf(withReauth))
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        _deschideLegaturi = deschideLegaturi
        _reimprospateaza = reimprospateaza
        _rebuildMissing = rebuildMissing
        If _deschideLegaturi Is Nothing Then
            tree.HeaderRightIcon = Nothing
            tree.HeaderRightIconTooltip = String.Empty
        End If
        If _reimprospateaza Is Nothing Then
            tree.FooterRightIcon = Nothing
            tree.FooterRightIconTooltip = String.Empty
        End If
        If _rebuildMissing Is Nothing Then
            tree.FooterLeftIcon = Nothing
            tree.FooterLeftIconTooltip = String.Empty
        End If
        'BuildColumns()
        ShowEmpty("Selectați un angajament din arbore.")
    End Sub

    ''' <summary>
    ''' Iconița din antetul arborelui deschide editorul de legături recepție ▸ instantaneu.
    '''
    ''' <para>ORICÂND, nu doar după o descărcare — cerința operatorului din 29.08.2026, și
    ''' totodată ce făcea și Access prin gazda `frmFX_ASOC`. Are nevoie doar de angajamentul
    ''' selectat; dacă nu e niciunul, spune asta în loc să deschidă o fereastră goală.</para>
    ''' </summary>
    Private Sub tree_HeaderRightIconClicked(e As MouseEventArgs) Handles tree.HeaderRightIconClicked
        Try
            If _deschideLegaturi Is Nothing Then Return
            If String.IsNullOrWhiteSpace(_requestedCod) Then
                KBotMessage.Show(Me, "Selectați întâi un angajament din arbore.",
                                "K-BOT — Legăturile recepțiilor",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            _deschideLegaturi(_requestedCod)
        Catch ex As Exception
            ' Graniță de UI: se loghează și se înghite — un throw dintr-un tratator de eveniment
            ' ar cădea pe firul de UI.
            GlobalErrorLog.Write("ReceptiiView.tree_HeaderRightIconClicked", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Iconița din DREAPTA subsolului arborelui cere o reîmprospătare din FOREXE a
    ''' RECEPȚIILOR angajamentului selectat (felia 0060) — nu o simplă recitire de pe server.
    ''' </summary>
    ''' <remarks>
    ''' <para>Butonul spunea până acum «Reîncarcă recepțiile de la server» și nu era legat la
    ''' nimic. Cererea operatorului din 10.09.2026 îl face să însemne ce se aștepta de la el:
    ''' du-te în FOREXE și adu ce s-a schimbat. Alegerea recepțiilor de citit se face în
    ''' macheta pe care o deschide shell-ul, înainte să pornească robotul.</para>
    ''' <para>Vederea NU se reîncarcă de aici: după ingestie shell-ul reîncarcă arborele cu
    ''' nodul păstrat, iar asta împinge singură contextul nou încoace.</para>
    ''' </remarks>
    Private Sub Tree_FooterRightIconClicked(e As MouseEventArgs) Handles tree.FooterRightIconClicked
        Try
            If _reimprospateaza Is Nothing Then Return
            If String.IsNullOrWhiteSpace(_requestedCod) Then
                KBotMessage.Show(Me, "Selectați întâi un angajament din arbore.",
                                "K-BOT — Recepții", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            _reimprospateaza(_requestedCod)
        Catch ex As Exception
            ' Graniță de UI: se loghează și se înghite — un throw dintr-un tratator de eveniment
            ' ar cădea pe firul de UI.
            GlobalErrorLog.Write("ReceptiiView.Tree_FooterRightIconClicked", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Iconița din STÂNGA subsolului arborelui cere refacerea, din istoric, a instantaneelor
    ''' și liniilor care lipsesc din FX_Receptii_H / FX_Receptii (felia 0062).
    ''' </summary>
    ''' <remarks>
    ''' Cererea operatorului din 15.09.2026: Access pierde IDRH/IDRR pe anteturi, iar după
    ''' migrare rândurile lipsesc sau stau neașezate. Istoricul le are pe toate, după IDH.
    ''' Shell-ul face proba, cere confirmarea, scrie și reîncarcă arborele cu nodul păstrat —
    ''' ceea ce împinge singur contextul nou încoace, deci vederea nu se reîncarcă de aici.
    ''' </remarks>
    Private Sub Tree_FooterLeftIconClicked(e As MouseEventArgs) Handles tree.FooterLeftIconClicked
        Try
            If _rebuildMissing Is Nothing Then Return
            If String.IsNullOrWhiteSpace(_requestedCod) Then
                KBotMessage.Show(Me, "Selectați întâi un angajament din arbore.",
                                "K-BOT — Recepții", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            _rebuildMissing(_requestedCod)
        Catch ex As Exception
            ' Graniță de UI: se loghează și se înghite.
            GlobalErrorLog.Write("ReceptiiView.Tree_FooterLeftIconClicked", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Reîncarcă recepțiile angajamentului curent. O cheamă shell-ul după ce editorul de
    ''' legături a salvat ceva: legăturile schimbate mută anteturile între recepții, deci ce
    ''' se vede pe ecran nu mai e adevărat.
    ''' </summary>
    Public Sub Reincarca()
        Try
            If String.IsNullOrWhiteSpace(_requestedCod) Then Return
            LoadAsync(_requestedCod)
        Catch ex As Exception
            GlobalErrorLog.Write("ReceptiiView.Reincarca", ex)
        End Try
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "receptii"
        End Get
    End Property

    ''' <summary>
    ''' Strângerea arborelui (felia 0028, aceeași înțelegere ca în MainForm): arborele e
    ''' <c>Dock = Fill</c> în <c>split.Panel1</c>, deci lățimea NU e a lui — el schimbă starea
    ''' și ne anunță, GAZDA mută splitter-ul. <c>Panel1MinSize</c> păzește TRAGEREA splitter-ului;
    ''' strângerea e o comandă, nu o tragere, deci coborâm paza cât ține starea.
    ''' </summary>
    'Private Sub tree_CollapsedChanged(collapsed As Boolean) Handles tree.CollapsedChanged
    '    Try
    '        Dim padStanga As Integer = split.Panel1.Padding.Left
    '        If collapsed Then
    '            _splitterDistanceDesfasurat = split.SplitterDistance
    '            _panel1MinSizeDesfasurat = split.Panel1MinSize
    '            Dim tinta As Integer = tree.MinimumCollapsedWidth + padStanga
    '            split.Panel1MinSize = Math.Min(_panel1MinSizeDesfasurat, tinta)
    '            split.SplitterDistance = ClampSplitter(tinta)
    '            split.IsSplitterFixed = True
    '        Else
    '            split.IsSplitterFixed = False
    '            If _panel1MinSizeDesfasurat > 0 Then split.Panel1MinSize = _panel1MinSizeDesfasurat
    '            Dim tinta As Integer = If(_splitterDistanceDesfasurat > 0,
    '                                      _splitterDistanceDesfasurat,
    '                                      tree.ExpandedWidth + padStanga)
    '            split.SplitterDistance = ClampSplitter(tinta)
    '        End If
    '    Catch ex As Exception
    '        GlobalErrorLog.Write("ReceptiiView.tree_CollapsedChanged", ex)
    '    End Try
    'End Sub

    ' Distanța splitter-ului adusă în intervalul acceptat de SplitContainer — o vedere îngustă
    ' n-are voie să transforme apăsarea butonului de strângere într-o excepție.
    Private Function ClampSplitter(dorit As Integer) As Integer
        Dim maxim As Integer = split.Width - split.Panel2MinSize - split.SplitterWidth
        If maxim < split.Panel1MinSize Then Return split.Panel1MinSize
        Return Math.Max(split.Panel1MinSize, Math.Min(dorit, maxim))
    End Function

    ' Coloanele grilei = frmFX_MAIN_REC_LISTA (qFX_MAIN_REC_LISTA_IND): NrCrt, Descriere,
    ' Clsf, Valoare. NrCrt + Valoare aliniate la dreapta (read-only, deci un tip numeric ar
    ' fi degeaba — Text cu format aliniat e suficient).
    'Private Sub BuildColumns()
    '    Try
    '        Dim colNr As KBotDataColumn = grid.AddColumn(COL_NRCRT, "NrCrt", KBotColumnType.Text, 60)
    '        colNr.TextAlign = ContentAlignment.MiddleRight
    '        grid.AddColumn(COL_DESCRIERE, "Descriere", KBotColumnType.Text, 220)
    '        grid.AddColumn(COL_CLSF, "Clasificație", KBotColumnType.Text, 180)
    '        Dim colVal As KBotDataColumn = grid.AddColumn(COL_VALOARE, "Valoare", KBotColumnType.Text, 130)
    '        colVal.FormatString = "N2"
    '        colVal.TextAlign = ContentAlignment.MiddleRight
    '    Catch ex As Exception
    '        GlobalErrorLog.Write("ReceptiiView.BuildColumns", ex)
    '        Throw
    '    End Try
    'End Sub

    ''' <summary>
    ''' Selecția din arbore s-a schimbat. Fără angajament (nod de capitol / deselectare)
    ''' NU se face niciun apel de rețea — doar se golește vederea.
    ''' </summary>
    Public Sub SetContext(info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        Try
            Dim cod As String = If(info Is Nothing, Nothing, info.CodAngajament)
            If String.IsNullOrWhiteSpace(cod) Then
                _requestedCod = Nothing
                _rows = Nothing
                _plati = Nothing
                tree.Clear()
                grid.ClearRows()
                ShowEmpty("Selectați un angajament din arbore.")
                Return
            End If

            _requestedCod = cod
            ShowEmpty("Se încarcă recepțiile…")
            ' Fire-and-forget deliberat (handler sincron al shell-ului): metoda își tratează
            ' singură TOATE erorile — vezi comentariul din SumarView/RezervariView.
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("ReceptiiView.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Boundary UI: logăm și ARĂTĂM eroarea, nu o aruncăm mai departe (apelul e pornit fără
    ' await din SetContext, deci nu există cine să o prindă).
    Private Async Sub LoadAsync(cod As String)
        Try
            Dim data As ReceptiiInfo = Await _withReauth(
                Function() _apiClient.GetReceptiiAsync(cod, CancellationToken.None)).ConfigureAwait(True)

            ' Răspuns depășit: între timp operatorul a selectat alt angajament. Îl aruncăm.
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return

            Dim rows As List(Of ReceptieRow) = If(data Is Nothing, New List(Of ReceptieRow)(), data.Receptii)
            _plati = If(data Is Nothing, New List(Of ReceptiePlata)(), data.Plati)
            If rows Is Nothing OrElse rows.Count = 0 Then
                _rows = Nothing
                tree.Clear()
                grid.ClearRows()
                ShowEmpty("Angajamentul nu are recepții.")
                Return
            End If

            _rows = rows
            BuildTree(rows)
            ' „Nimic selectat" -> grila arată TOATE recepțiile angajamentului (ca în
            ' RezervariView, și ca rădăcina); un click pe un nod o restrânge apoi la ale lui.
            FillGridFromRows(rows)
            ShowContent()
        Catch ex As ApiException
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("ReceptiiView.LoadAsync", ex)
            _rows = Nothing
            tree.Clear()
            grid.ClearRows()
            ShowEmpty(ex.Message)   ' mesaj românesc din câmpul «error» al serverului
        Catch ex As Exception
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("ReceptiiView.LoadAsync", ex)
            _rows = Nothing
            tree.Clear()
            grid.ClearRows()
            ShowEmpty("Recepțiile nu au putut fi încărcate. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' ── Arborele ─────────────────────────────────────────────────────────────
    ' TREI niveluri (felia 0065): rădăcina «Toate recepțiile» -> folder lună/an (grupat pe
    ' DataR) -> recepția (IDRR, iconiță după VALOARE: totalul ultimului antet >= 0 -> «up»,
    ' negativ -> «down»). Nivelul de ANTET (IDRH) nu există ca nod (revizuire operator
    ' 2026-08-13) — anteturile rămân rânduri în Tag-ul recepției. Aici NU există iconița «+»
    ' din Rezervări (recepțiile n-au nevoie de ea), deci nu se rezervă loc la dreapta.
    ' Rândurile vin ordonate de server (R.NRCRT, R.DataR, H.NrCrt, H.DataH); lunile se
    ' ordonează cronologic, iar în interiorul unei luni „distinct în ordine" e suficient.
    ' Fiecare nod poartă în Tag rândurile lui — TOATE liniile TUTUROR anteturilor
    ' recepțiilor lui —, iar grila alege singură din ele ultimul antet al fiecărei recepții
    ' (`FillGridFromRows`). Tooltip-ul de reconciliere stă pe rădăcină, pe folderul de lună
    ' și pe recepție.
    Private Sub BuildTree(rows As List(Of ReceptieRow))
        Try
            tree.Clear()
            Dim palette As ThemePalette = TryGetPalette()

            Dim monthItems As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
            Dim receptieItems As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()

            ' Anteturile NEAȘEZATE (Idrr = 0, felia 0062) nu au recepție, deci nici lună de
            ' DataR: ele merg într-un dosar separat, la sfârșit, nu în cronologie.
            Dim placedRows As List(Of ReceptieRow) = rows.Where(Function(r) r.Idrr > 0).ToList()
            Dim unplacedRows As List(Of ReceptieRow) = rows.Where(Function(r) r.Idrr <= 0).ToList()

            Dim rootItem As AdvancedTreeControl.TreeItem = Nothing
            If placedRows.Count > 0 Then
                ' Rădăcina: toate recepțiile așezate, cu totalul lor = suma valorii (ultimul
                ' antet) fiecărei recepții.
                Dim icoRoot As Image = LunaIcon()
                rootItem = tree.AddItem(ROOT_KEY, $"Toate recepțiile~~~{Money(TotalReceptii(placedRows))}",
                                        pLeftIconClosed:=icoRoot, pLeftIconOpen:=icoRoot,
                                        pExpanded:=True)
                rootItem.Tag = placedRows
                rootItem.Bold = True
            End If

            ' Grupare pe lună (an, lună din DataR), cronologic.
            Dim monthGroups = placedRows.GroupBy(Function(r) MonthKeyOf(r.DataR)).
                                         OrderBy(Function(g) g.Key)

            For Each mg In monthGroups
                Dim monthRows As List(Of ReceptieRow) = mg.ToList()
                ' Totalul lunii = suma valorii (ultimul antet) pe recepțiile DISTINCTE ale lunii.
                Dim monthTotal As Double = TotalReceptii(monthRows)
                ' NU «lunaIcon»: VB e insensibil la litere mari/mici, deci variabila ar purta
                ' același nume cu funcția LunaIcon și ar umbri-o (capcana din RezervariView).
                Dim icoLuna As Image = LunaIcon()
                Dim monthItem As AdvancedTreeControl.TreeItem =
                    tree.AddItem($"m_{mg.Key}", $"{MonthLabel(mg.Key Mod 100)}~~~{Money(monthTotal)}",
                                 rootItem,
                                 pLeftIconClosed:=icoLuna, pLeftIconOpen:=icoLuna,
                                 pExpanded:=True)
                monthItem.Tag = monthRows
                monthItem.Bold = True
                monthItem.Expanded = False
                monthItems(mg.Key) = monthItem

                ' Recepțiile sub folderul lunii (al TREILEA și ultimul nivel), în ordinea
                ' serverului (R.NRCRT, R.DataR) — GroupBy păstrează ordinea primei apariții.
                For Each gp In monthRows.GroupBy(Function(r) r.Idrr)
                    Dim recRows As List(Of ReceptieRow) = gp.ToList()
                    Dim r As ReceptieRow = recRows(0)
                    ' Valoarea recepției = totalul ULTIMULUI ei antet (felia 0065), nu
                    ' SumaAntet al rândului R: ea e ce a scris ultima salvare de pe site.
                    Dim ultimul As ReceptieRow = UltimulAntet(recRows)
                    Dim valoare As Double = If(ultimul Is Nothing, 0.0, ultimul.Total)
                    Dim icon As Image = ValoareIconOf(valoare, palette)
                    ' DataR (the day) + the TIME of the last header (DataH): two receptii on
                    ' the same day would otherwise read the same (operator's request,
                    ' 17.09.2026).
                    Dim caption As String =
                        $"{ShortDate(r.DataR)}{TimeOfHeader(ultimul)}{SemnReconstituire(r)}{SemnStergere(ultimul)}~~~{Money(valoare)}"
                    Dim root As AdvancedTreeControl.TreeItem =
                        tree.AddItem($"r_{gp.Key}", caption, monthItem,
                                     pLeftIconClosed:=icon, pLeftIconOpen:=icon)
                    ' Toate liniile TUTUROR anteturilor recepției intră în Tag-ul ei: nivelul
                    ' de antet nu există ca nod, iar grila alege din ele ultimul antet.
                    root.Tag = recRows
                    receptieItems(gp.Key) = root
                Next
            Next

            ' Tooltip de reconciliere pe rădăcină, pe folderele de lună ȘI pe recepții
            ' (fereastra de plăți se întinde până la prima recepție a lunii URMĂTOARE —
            ' revizuirea operatorului). Doar peste rândurile AȘEZATE: un antet fără recepție nu
            ' are DataR și nu intră în niciun cumul.
            ComputeTooltips(placedRows, rootItem, monthItems, receptieItems)

            AddUnplacedFolder(unplacedRows, palette)

            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("ReceptiiView.BuildTree", ex)
            Throw
        End Try
    End Sub

    ' ── Ultimul antet al unei recepții (felia 0065) ──────────────────────────
    ' O recepție e un lanț de instantanee; fiecare poartă VALOAREA ÎNTREGII recepții la acel
    ' moment (F3). Ultimul, în ordinea DataH apoi IDRH, e adevărul de acum. Anteturile marcate
    ' Sters (H.Sters — «nu consemnează nicio schimbare», F17) nu fac parte din lanț.

    ''' <summary>Un rând al ultimului antet al recepției (toate rândurile unui antet poartă
    ''' aceleași Total / DataH / DescriereH), sau Nothing dacă recepția nu are niciun antet.</summary>
    Private Shared Function UltimulAntet(recRows As IEnumerable(Of ReceptieRow)) As ReceptieRow
        Dim grp = AnteturiDescrescator(recRows).FirstOrDefault()
        Return If(grp Is Nothing, Nothing, grp.First())
    End Function

    ''' <summary>
    ''' Liniile (indicatorii) recepției AȘA CUM SUNT ACUM: liniile ultimului antet care ARE
    ''' linii. Rândul de ștergere (F21) e ultimul antet al unei recepții șterse și nu are linii
    ''' prin definiție; ce s-a șters sunt liniile antetului dinaintea lui.
    ''' </summary>
    Private Shared Function LiniileUltimuluiAntet(recRows As IEnumerable(Of ReceptieRow)) As List(Of ReceptieRow)
        For Each grp In AnteturiDescrescator(recRows)
            ' The server sends one row per indicator of the receptie for EVERY header
            ' (Idr Nothing + Valoare 0 where the header has no line on that indicator).
            ' A header counts as «with lines» if any indicator has a real line; then ALL
            ' its indicator rows go to the grid, so the operator sees the zero ones too
            ' (operator's request, 17.09.2026: «12100 and 0», «0 and 10350»).
            If grp.Any(Function(r) r.Idr.HasValue) Then Return grp.ToList()
        Next
        Return New List(Of ReceptieRow)()
    End Function

    ' Anteturile recepției, cel mai nou primul (DataH desc, apoi IDRH desc), fără cele Sters.
    Private Shared Function AnteturiDescrescator(recRows As IEnumerable(Of ReceptieRow)) As IEnumerable(Of IGrouping(Of Integer, ReceptieRow))
        Return recRows.Where(Function(r) Not r.StersH).
                       GroupBy(Function(r) r.Idrh).
                       OrderByDescending(Function(gp) If(gp.First().DataH.HasValue, gp.First().DataH.Value, Date.MinValue)).
                       ThenByDescending(Function(gp) gp.Key)
    End Function

    ''' <summary>Valoarea unei recepții = totalul ultimului ei antet (0 dacă n-are niciunul).</summary>
    Private Shared Function ValoareaReceptiei(recRows As IEnumerable(Of ReceptieRow)) As Double
        Dim ultimul As ReceptieRow = UltimulAntet(recRows)
        Return If(ultimul Is Nothing, 0.0, ultimul.Total)
    End Function

    ''' <summary>Suma valorilor recepțiilor DISTINCTE din rândurile date (rădăcină / lună).</summary>
    Private Shared Function TotalReceptii(rows As IEnumerable(Of ReceptieRow)) As Double
        Return rows.GroupBy(Function(r) r.Idrr).Sum(Function(gp) ValoareaReceptiei(gp))
    End Function

    ''' <summary>
    ''' Liniile de arătat în grilă pentru un nod: ultimul antet al FIECĂREI recepții din
    ''' rândurile lui. Pe un nod de recepție sunt exact liniile ultimului ei antet; pe lună și
    ''' pe rădăcină, ale fiecărei recepții, apoi grila le grupează pe clasificație.
    ''' Anteturile neașezate (Idrr = 0) n-au recepție, deci se iau așa cum sunt.
    ''' </summary>
    Private Shared Function LiniileDeAratat(nodeRows As IEnumerable(Of ReceptieRow)) As List(Of ReceptieRow)
        Dim out As New List(Of ReceptieRow)()
        For Each gp In nodeRows.GroupBy(Function(r) r.Idrr)
            If gp.Key > 0 Then
                out.AddRange(LiniileUltimuluiAntet(gp))
            Else
                out.AddRange(gp.Where(Function(r) r.Idr.HasValue))
            End If
        Next
        Return out
    End Function

    ''' <summary>Semnul de pe o recepție al cărei ultim antet e rândul de ștergere (F21).</summary>
    Private Shared Function SemnStergere(ultimul As ReceptieRow) As String
        If ultimul IsNot Nothing AndAlso ultimul.EsteStergere Then Return "  [ștearsă]"
        Return String.Empty
    End Function

    ''' <summary>
    ''' Dosarul «Instantanee neașezate» (felia 0062): un nod per antet (IDRH) cu
    ''' <c>H.IDRR NULL</c>, adică un instantaneu care nu stă pe nicio recepție.
    ''' </summary>
    ''' <remarks>
    ''' Până la 0062 serverul nici nu le trimitea (INNER JOIN pe R), iar vederea spunea
    ''' «angajamentul nu are recepții» când el avea — doar pierdute de Access la IDRR. Acum se
    ''' văd, cu data și totalul lor, iar operatorul le așază din editorul de legături (iconița
    ''' din antetul arborelui). Fără tooltip de reconciliere: nu au DataR, deci nu intră în
    ''' niciun cumul. Un click pe nod umple grila cu liniile antetului, ca la orice nod.
    ''' </remarks>
    Private Sub AddUnplacedFolder(unplacedRows As List(Of ReceptieRow), palette As ThemePalette)
        If unplacedRows Is Nothing OrElse unplacedRows.Count = 0 Then Return

        ' Totalul dosarului = suma Total pe anteturi DISTINCTE (Total e constant pe liniile
        ' unui antet).
        Dim folderTotal As Double = unplacedRows.GroupBy(Function(r) r.Idrh).
                                                 Sum(Function(gp) gp.First().Total)
        Dim icoLuna As Image = LunaIcon()
        Dim folder As AdvancedTreeControl.TreeItem =
            tree.AddItem(UNPLACED_KEY, $"Instantanee neașezate~~~{Money(folderTotal)}",
                         pLeftIconClosed:=icoLuna, pLeftIconOpen:=icoLuna,
                         pExpanded:=True)
        folder.Tag = unplacedRows
        folder.Bold = True
        folder.Tooltip = BuildUnplacedTooltipXml(unplacedRows.GroupBy(Function(r) r.Idrh).Count())

        ' Un nod per antet, în ordinea DataH apoi IDRH (ordinea serverului pentru rândurile
        ' fără recepție: H.NrCrt, H.DataH).
        Dim perAntet = unplacedRows.GroupBy(Function(r) r.Idrh).
                                    OrderBy(Function(gp) If(gp.First().DataH.HasValue, gp.First().DataH.Value, Date.MinValue)).
                                    ThenBy(Function(gp) gp.Key)
        For Each gp In perAntet
            Dim first As ReceptieRow = gp.First()
            Dim icon As Image = ValoareIconOf(first.Total, palette)
            Dim descriere As String = If(String.IsNullOrWhiteSpace(first.DescriereH), String.Empty, "  " & first.DescriereH.Trim())
            Dim caption As String = $"{ShortDate(first.DataH)}{descriere}~~~{Money(first.Total)}"
            Dim node As AdvancedTreeControl.TreeItem =
                tree.AddItem($"h_{gp.Key}", caption, folder,
                             pLeftIconClosed:=icon, pLeftIconOpen:=icon)
            node.Tag = gp.ToList()
        Next
    End Sub

    ' Tooltip-ul dosarului de neașezate: câte anteturi sunt și ce e de făcut cu ele.
    Private Shared Function BuildUnplacedTooltipXml(count As Integer) As String
        Dim sb As New StringBuilder()
        sb.Append("<table>")
        sb.Append("<header>")
        sb.Append("<cell Align=""left"" Bold=""1"">Instantanee neașezate</cell>")
        sb.Append("<cell Align=""right"" Bold=""1"">").Append(count).Append("</cell>")
        sb.Append("</header>")
        AppendTtRow(sb, "Antete fără recepție (IDRR gol)", CStr(count), Nothing)
        AppendTtRow(sb, "Se așază din editorul de legături", "iconița din antetul arborelui", Nothing)
        sb.Append("</table>")
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Semnul pus lângă data unei recepții RECONSTITUITE (F26) — și, dacă gruparea ei nu a
    ''' putut fi verificată de program, semnul mai apăsat al lui F28.
    ''' </summary>
    ''' <remarks>
    ''' Recepția reconstituită s-a construit din propriile ei instantanee, fiindcă a fost
    ''' creată ȘI ștearsă înainte ca K-BOT să fi descărcat vreodată angajamentul. Când pe
    ''' același angajament sunt DOUĂ astfel de recepții, instantaneele lor nu se pot deosebi
    ''' altfel decât după sumă și indicator (F27): gruparea a fost o judecată a operatorului,
    ''' nu o verificare. Cine citește un total peste luni trebuie să poată afla asta din
    ''' arbore, nu dintr-un worklog.
    '''
    ''' Text, nu pictogramă: nu se adaugă și nu se scoate niciun control din designer, iar
    ''' semnul se vede și pe un rând îngust.
    ''' </remarks>
    Private Shared Function SemnReconstituire(r As ReceptieRow) As String
        If r.ReconstituitNesigur Then Return "  ⚠ reconstituită (grupare neverificată)"
        If r.Reconstituit Then Return "  ⟲ reconstituită"
        Return String.Empty
    End Function

    ' ── Tooltip de reconciliere (rădăcină + lună + recepție) ─────────────────
    ' Oglindește NewRootPlatiTooltip din frmFX_MAIN_REC, generalizat la lună (revizuire
    ' operator 2026-07-22): Data recepție/Lună / Descriere (pe recepție, felia 0065) /
    ' Recepții cumulate (recCum) / Plăți cumulate (platiCum) / Diferență (recCum − platiCum,
    ' roșu dacă <0, albastru >0).
    '   * recCum = sumă rulantă a VALORII recepției — totalul ULTIMULUI ei antet (felia
    '     0065) —, cumulată pe recepții în ordinea DataR. Recepția = cumul până la ea; luna =
    '     cumul până la ULTIMA recepție a lunii; rădăcina = cumulul întreg.
    '     Până la 0065 se însuma DIFH pe anteturi (qFX_MAIN_REC_TT_DIFH). Pe un lanț complet
    '     suma DIFH-urilor E totalul ultimului antet, dar DIFH e calculat de noi, la pasul 4d,
    '     și lipsea (NULL -> 0) pe orice antet așezat din editorul de legături — recepția
    '     dispărea din total (25.410 în loc de 29.645, cazul operatorului). Totalul antetului
    '     vine de pe site și nu poate lipsi.
    '   * platiCum on a MONTH = Sum(Suma) over payments with DataPlata < the DataR of the
    '     FIRST receptie of the NEXT month (everything paid before the next month starts,
    '     operator's request). Last month -> all payments. Root -> all payments.
    '   * platiCum on a RECEPTIE (a day) = Sum(Suma) over payments with DataPlata < the DataR
    '     of the NEXT receptie (chronological, any month); the last receptie -> all payments
    '     (operator's request, 2026-09-17: R1=01.01, R2=04.01, P1=02.01, P2=05.01 -> R1 has
    '     P1, R2 has P1+P2). It no longer shares the month window.
    ' valAsoc NU e folosit (linia lui e comentată în Access). Nu se face niciun apel de rețea.
    Private Sub ComputeTooltips(rows As List(Of ReceptieRow),
                                rootItem As AdvancedTreeControl.TreeItem,
                                monthItems As Dictionary(Of Integer, AdvancedTreeControl.TreeItem),
                                receptieItems As Dictionary(Of Integer, AdvancedTreeControl.TreeItem))
        Dim plati As List(Of ReceptiePlata) = If(_plati, New List(Of ReceptiePlata)())

        ' Recepții, cronologic pe DataR.
        Dim perReceptie = rows.GroupBy(Function(r) r.Idrr).
            Select(Function(gp) New ReceptieTtRow With {
                .Idrr = gp.Key,
                .DataR = gp.Select(Function(x) x.DataR).FirstOrDefault(Function(d) d.HasValue),
                .MonthKey = MonthKeyOf(gp.Select(Function(x) x.DataR).FirstOrDefault(Function(d) d.HasValue)),
                .Valoare = ValoareaReceptiei(gp),
                .Descriere = DescriereaReceptiei(gp)
            }).
            OrderBy(Function(x) If(x.DataR.HasValue, x.DataR.Value, Date.MinValue)).
            ThenBy(Function(x) x.Idrr).
            ToList()

        ' Lunile în ordine cronologică + prima DataR a fiecărei luni.
        Dim monthsOrdered As List(Of Integer) =
            perReceptie.Select(Function(x) x.MonthKey).Distinct().OrderBy(Function(k) k).ToList()
        Dim firstDataRByMonth As New Dictionary(Of Integer, Date)()
        For Each rec As ReceptieTtRow In perReceptie
            If rec.DataR.HasValue AndAlso Not firstDataRByMonth.ContainsKey(rec.MonthKey) Then
                firstDataRByMonth(rec.MonthKey) = rec.DataR.Value
            End If
        Next

        ' Fereastra de plăți pe lună: toate plățile cu DataPlata < prima recepție a lunii
        ' URMĂTOARE (bariera e exclusivă); ultima lună -> toate plățile.
        Dim platiWindowByMonth As New Dictionary(Of Integer, Double)()
        For i As Integer = 0 To monthsOrdered.Count - 1
            Dim boundary As Date? = Nothing
            For j As Integer = i + 1 To monthsOrdered.Count - 1
                If firstDataRByMonth.ContainsKey(monthsOrdered(j)) Then
                    boundary = firstDataRByMonth(monthsOrdered(j))
                    Exit For
                End If
            Next
            platiWindowByMonth(monthsOrdered(i)) = PlatiBefore(plati, boundary)
        Next

        ' Cumulul valorilor, în ordinea DataR. Recepția = cumul până la ea; luna reține cumulul
        ' până la ULTIMA recepție a lunii (ultima scriere câștigă).
        ' The receptie's payments are everything dated before the NEXT receptie (the last
        ' receptie takes all of them), not the month window.
        Dim recCum As Double = 0
        Dim monthCum As New Dictionary(Of Integer, Double)()
        For i As Integer = 0 To perReceptie.Count - 1
            Dim rec As ReceptieTtRow = perReceptie(i)
            recCum += rec.Valoare
            monthCum(rec.MonthKey) = recCum
            Dim nextDataR As Date? = Nothing
            For j As Integer = i + 1 To perReceptie.Count - 1
                If perReceptie(j).DataR.HasValue Then
                    nextDataR = perReceptie(j).DataR
                    Exit For
                End If
            Next
            Dim platiCum As Double = PlatiBefore(plati, nextDataR)
            Dim ri As AdvancedTreeControl.TreeItem = Nothing
            If receptieItems.TryGetValue(rec.Idrr, ri) Then
                ri.Tooltip = BuildReconTooltipXml("Data recepție", ShortDate(rec.DataR), recCum, platiCum,
                                                  rec.Descriere)
            End If
        Next

        For Each mk As Integer In monthsOrdered
            Dim mi As AdvancedTreeControl.TreeItem = Nothing
            If monthItems.TryGetValue(mk, mi) Then
                mi.Tooltip = BuildReconTooltipXml("Lună", MonthYearLabel(mk),
                                                  LookupOrZero(monthCum, mk),
                                                  LookupOrZero(platiWindowByMonth, mk), Nothing)
            End If
        Next

        ' Rădăcina: tot ce s-a recepționat față de tot ce s-a plătit.
        If rootItem IsNot Nothing Then
            rootItem.Tooltip = BuildReconTooltipXml("Toate recepțiile",
                                                    perReceptie.Count.ToString(_roCulture),
                                                    recCum, plati.Sum(Function(p) p.Suma), Nothing)
        End If
    End Sub

    ''' <summary>
    ''' Descrierea de arătat pe o recepție: a recepției (R.Descriere) dacă are una, altfel a
    ''' ultimului ei antet. Rândul R al unei recepții reconstituite (F26) poate să n-aibă niciuna.
    ''' </summary>
    Private Shared Function DescriereaReceptiei(recRows As IEnumerable(Of ReceptieRow)) As String
        Dim first As ReceptieRow = recRows.FirstOrDefault()
        If first IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(first.DescriereR) Then
            Return first.DescriereR.Trim()
        End If
        Dim ultimul As ReceptieRow = UltimulAntet(recRows)
        If ultimul IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(ultimul.DescriereH) Then
            Return ultimul.DescriereH.Trim()
        End If
        Return String.Empty
    End Function

    ''' <summary>
    ''' Sum of the payments dated strictly before <paramref name="boundary"/>; no boundary
    ''' (last month / last receptie) -> all payments.
    ''' </summary>
    Private Shared Function PlatiBefore(plati As List(Of ReceptiePlata), boundary As Date?) As Double
        If Not boundary.HasValue Then Return plati.Sum(Function(p) p.Suma)
        Return plati.
            Where(Function(p) p.DataPlata.HasValue AndAlso p.DataPlata.Value < boundary.Value).
            Sum(Function(p) p.Suma)
    End Function

    Private Shared Function LookupOrZero(map As Dictionary(Of Integer, Double), key As Integer) As Double
        Dim v As Double
        Return If(map.TryGetValue(key, v), v, 0.0)
    End Function

    ' Construiește tabelul-tooltip XML (<table>) citit de TooltipTableParser al arborelui.
    ' firstLabel/firstValue = primul rând (Data recepție + data, sau Lună + „Ianuarie/2026").
    ' descriere = rândul «Descriere» de sub el (felia 0065, doar pe recepție); gol -> lipsește.
    Private Shared Function BuildReconTooltipXml(firstLabel As String, firstValue As String,
                                                 recCum As Double, platiCum As Double,
                                                 descriere As String) As String
        Dim dif As Double = Math.Round(recCum - platiCum, 2)
        ' Roșu dacă negativ, albastru dacă pozitiv (Switch din Access). ParseColor ia #RRGGBB.
        Dim difColor As String = If(dif < 0, "#CC0000", If(dif > 0, "#0033CC", Nothing))

        Dim sb As New StringBuilder()

        sb.Append("<table>")
        sb.Append("<header>")
        sb.Append("<cell Align=""left"" Bold=""1"">").Append(XmlEscape(firstLabel)).Append("</cell>")
        sb.Append("<cell Align=""right"" Bold=""1"">Valoare</cell>")
        sb.Append("</header>")
        AppendTtRow(sb, firstLabel, firstValue, Nothing)
        If Not String.IsNullOrEmpty(descriere) Then AppendTtRow(sb, "Descriere", descriere, Nothing)
        AppendTtRow(sb, "Recepții cumulate", Money(recCum), Nothing)
        AppendTtRow(sb, "Plăți cumulate", Money(platiCum), Nothing)
        AppendTtRow(sb, "Diferență", Money(dif), difColor)
        sb.Append("</table>")
        Return sb.ToString()
    End Function

    Private Shared Sub AppendTtRow(sb As StringBuilder, label As String, value As String, valueColor As String)
        sb.Append("<row>")
        sb.Append("<cell Align=""left"">").Append(XmlEscape(label)).Append("</cell>")
        sb.Append("<cell Align=""right""")
        If Not String.IsNullOrEmpty(valueColor) Then
            sb.Append(" ForeColor=""").Append(valueColor).Append(""""c)
        End If
        sb.Append(">"c).Append(XmlEscape(value)).Append("</cell>")
        sb.Append("</row>")
    End Sub

    Private Shared Function XmlEscape(s As String) As String
        If String.IsNullOrEmpty(s) Then Return String.Empty
        Return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("""", "&quot;")
    End Function

    ' Agregat de lucru per recepție pentru cumulul din tooltip (nu părăsește vederea).
    Private NotInheritable Class ReceptieTtRow
        Public Property Idrr As Integer
        Public Property DataR As Date?
        Public Property MonthKey As Integer
        ''' <summary>Valoarea recepției = totalul ultimului ei antet (felia 0065).</summary>
        Public Property Valoare As Double
        Public Property Descriere As String = String.Empty
    End Class

    ' ── Grila (LISTA) ────────────────────────────────────────────────────────
    ' Detaliul unui nod (rădăcină / lună / recepție / antet neașezat): un rând per
    ' clasificație (Valoare = Sum(Valoare) grupat pe Clsf, NrCrt din indicator, Descriere =
    ' Denumirea clasificației — bine definită la orice nivel de agregare, spre deosebire de
    ' descrierea antetului). Grupurile: pe NrCrt apoi Clsf.
    ' Din felia 0065 liniile sunt ale ULTIMULUI antet al fiecărei recepții a nodului
    ' (`LiniileDeAratat`): pe o recepție, exact indicatorii ei de acum; pe lună / rădăcină,
    ' indicatorii fiecărei recepții, adunați pe clasificație. Un agregat peste toate
    ' anteturile lanțului ar aduna aceeași sumă o dată pentru fiecare salvare de pe site.
    ' Fără rândul-total sintetic (scos la revizuirea operatorului din 2026-08-13).
    Private Sub FillGridFromRows(nodeRows As List(Of ReceptieRow))
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            If nodeRows Is Nothing OrElse nodeRows.Count = 0 Then Return

            ' Randul-total: Sum(DIF) peste toate liniile nodului.
            'Dim totalDif As Double = nodeRows.Sum(Function(r) r.Dif)
            'Dim rowTot As KBotDataRow = grid.AddRow()
            'rowTot(COL_NRCRT) = String.Empty
            'rowTot(COL_DESCRIERE) = "Toți indicatorii"
            'rowTot(COL_CLSF) = String.Empty
            'rowTot(COL_VALOARE) = totalDif

            ' Rânduri per clasificație — liniile ultimului antet al fiecărei recepții.
            Dim lines = LiniileDeAratat(nodeRows)
            Dim groups = lines.GroupBy(Function(r) r.Clsf).
                               OrderBy(Function(gp) MinNrCrt(gp)).
                               ThenBy(Function(gp) gp.Key, StringComparer.Ordinal)

            For Each grp In groups
                Dim r As KBotDataRow = grid.AddRow()
                'Dim nrCrt As Integer? = grp.Select(Function(x) x.NrCrtInd).FirstOrDefault(Function(v) v.HasValue)
                'r(COL_NRCRT) = If(nrCrt.HasValue, CObj(nrCrt.Value), CObj(String.Empty))
                r(COL_DESCRIERE) = FirstNonEmpty(grp.Select(Function(x) x.Denumire))
                r(COL_CLSF) = grp.Key
                r(COL_VALOARE) = grp.Sum(Function(x) x.Valoare)
            Next
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    ' Cheia de ordonare a grupurilor: cel mai mic NrCrt din grup (fără -> mare, la coadă).
    Private Shared Function MinNrCrt(grp As IEnumerable(Of ReceptieRow)) As Integer
        Dim vals = grp.Where(Function(r) r.NrCrtInd.HasValue).Select(Function(r) r.NrCrtInd.Value)
        Return If(vals.Any(), vals.Min(), Integer.MaxValue)
    End Function

    ' Prima denumire ne-goală din grup (LEFT JOIN poate lăsa unele goale pe o clasificație).
    Private Shared Function FirstNonEmpty(values As IEnumerable(Of String)) As String
        For Each v As String In values
            If Not String.IsNullOrEmpty(v) Then Return v
        Next
        Return String.Empty
    End Function

    ' Click pe orice nod (rădăcină / lună / recepție) -> umple grila din rândurile
    ' nodului (în Tag). Fără apel de rețea.
    Private Sub tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing Then Return
            Dim rows As List(Of ReceptieRow) = TryCast(pNode.Tag, List(Of ReceptieRow))
            If rows Is Nothing Then Return
            FillGridFromRows(rows)
        Catch ex As Exception
            GlobalErrorLog.Write("ReceptiiView.tree_NodeMouseUp", ex)
        End Try
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
    End Sub

    ' ── Formatare / iconițe ──────────────────────────────────────────────────
    Private Shared Function Money(value As Double) As String
        Return value.ToString("N2", _roCulture)
    End Function

    ' Data scurtă în format românesc (dd.MM.yyyy). Nothing -> gol.
    ' The time of the last header (« 10:08:18»), for the receptie caption. No header or
    ' no DataH -> empty.
    Private Shared Function TimeOfHeader(ultimul As ReceptieRow) As String
        If ultimul Is Nothing OrElse Not ultimul.DataH.HasValue Then Return String.Empty
        Return " " & ultimul.DataH.Value.ToString("HH:mm:ss", _roCulture)
    End Function

    Private Shared Function ShortDate(value As Date?) As String
        If Not value.HasValue Then Return String.Empty
        Return value.Value.ToString("dd.MM.yyyy", _roCulture)
    End Function

    ' Cheia de lună = an*100 + lună din DataR (0 dacă lipsește data). Ordonabilă cronologic.
    Private Shared Function MonthKeyOf(value As Date?) As Integer
        If Not value.HasValue Then Return 0
        Return value.Value.Year * 100 + value.Value.Month
    End Function

    ' Eticheta lună/an dintr-o cheie de lună: „Ianuarie/2026". Cheia 0 -> „(fără dată)".
    Private Shared Function MonthYearLabel(monthKey As Integer) As String
        If monthKey <= 0 Then Return "(fără dată)"
        Dim y As Integer = monthKey \ 100
        Dim m As Integer = monthKey Mod 100
        Return $"{MonthLabel(m)}/{y}"
    End Function


    ' Numele lunii în română (Ianuarie…), cu prima literă mare (ca în RezervariView).
    Private Shared Function MonthLabel(month As Integer) As String
        If month < 1 OrElse month > 12 Then Return CStr(month)
        Dim name As String = _roCulture.DateTimeFormat.GetMonthName(month)
        If String.IsNullOrEmpty(name) Then Return CStr(month)
        Return Char.ToUpper(name(0), _roCulture) & name.Substring(1)
    End Function

    ''' <summary>
    ''' Iconița recepției după VALOAREA ei (revizuire operator 2026-08-13, ca în RezervariView):
    ''' valoare pozitivă sau zero -> «up», valoare negativă -> «down». Steagurile
    ''' Incarcat/Preluat NU mai aleg iconița — ele spun în ce stadiu e recepția, nu dacă suma
    ''' urcă sau coboară, și lăsau toate rândurile cu aceeași săgeată în jos.
    '''
    ''' ÎNTÂI din «image_list» (pozele alese de operator în designer, aceeași regulă
    ''' listă-întâi ca în RezervariView) și abia dacă lista n-are cheia respectivă se cade
    ''' înapoi pe formele GDI din <see cref="ReceptiiIcons"/> — altfel un ImageList incomplet
    ''' ar lăsa noduri fără iconiță.
    ''' </summary>
    Private Function ValoareIconOf(valoare As Double, palette As ThemePalette) As Image
        Dim urca As Boolean = valoare >= 0

        Dim dinLista As Image = tree.NodeImage(If(urca, ICO_SUS, ICO_JOS))
        If dinLista IsNot Nothing Then Return dinLista

        ' Fallback GDI (se re-tintează pe paletă; imaginile din listă sunt fixe).
        If palette Is Nothing Then Return Nothing
        Dim stare As ReceptiiIcons.Stare = If(urca, ReceptiiIcons.Stare.Sus, ReceptiiIcons.Stare.Jos)
        Dim color As Color = If(urca, palette.SuccessColor, palette.ErrorColor)
        Return ReceptiiIcons.StatusIcon(stare, color, tree.LeftIconSize.Width)
    End Function

    ''' <summary>Iconița folderului de lună; doar din «image_list» (n-are formă GDI).</summary>
    Private Function LunaIcon() As Image
        Return tree.NodeImage(ICO_LUNA)
    End Function

    Private Shared Function TryGetPalette() As ThemePalette
        ' Headless (teste) sau înainte de inițializarea temei: ThemeManager.Current poate fi
        ' Nothing. Atunci arborele se construiește fără iconițe/culori (structura e aceeași),
        ' iar ApplyTheme reconstruiește când tema devine disponibilă.
        Dim current As ThemeScheme = ThemeManager.Current
        Return If(current Is Nothing, Nothing, current.Palette)
    End Function

    ''' <summary>
    ''' Reaplică culorile schemei pe arbore + starea goală (grila se auto-temează:
    ''' KBotDataView implementează el însuși IThemedControl). Reconstruiește arborele dacă
    ''' are date, ca iconițele de stare să se re-tinteze pe noua paletă.
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

            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor

            ' Re-tintarea iconițelor de stare pe noua paletă (grila rămâne golită — LISTA
            ' se reface la următorul click pe un nod).
            If _rows IsNot Nothing AndAlso _rows.Count > 0 Then
                BuildTree(_rows)
            End If
        Catch ex As Exception
            ' Boundary UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("ReceptiiView.ApplyTheme", ex)
        End Try
    End Sub

End Class
