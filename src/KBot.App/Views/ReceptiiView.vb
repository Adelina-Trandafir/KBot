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
''' cu un arbore de recepții pe 2 niveluri la stânga (folder lună/an -> recepția R / IDRR,
''' ca în RezervariView) și o grilă continuă la dreapta (LISTA) cu detaliul pe clasificații.
''' Read-only în această felie. Datele vin din GET /api/forexe/receptii, întotdeauna prin
''' plasa de re-autentificare a shell-ului (401 -> re-login -> reia o dată). Click pe ORICE
''' nod (lună / recepție) umple grila cu agregatul rândurilor lui: un rând-total
''' sintetic (Sum(DIF)) + un rând per clasificație (Sum(Valoare)). Tooltip de reconciliere
''' recepții/plăți pe folderele de lună ȘI pe recepții (revizuire operator 2026-07-22).
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

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of ReceptiiInfo)), Task(Of ReceptiiInfo)),
                   Optional deschideLegaturi As Action(Of String) = Nothing)
        If apiClient Is Nothing Then Throw New ArgumentNullException(NameOf(apiClient))
        If withReauth Is Nothing Then Throw New ArgumentNullException(NameOf(withReauth))
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        _deschideLegaturi = deschideLegaturi
        If _deschideLegaturi Is Nothing Then
            tree.HeaderRightIcon = Nothing
            tree.HeaderRightIconTooltip = String.Empty
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
                MessageBox.Show(Me, "Selectați întâi un angajament din arbore.",
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
            ' RezervariView); un click pe un nod o restrânge apoi la rândurile lui.
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
    ' DOUĂ niveluri (revizuire operator 2026-08-13, ca în RezervariView): folder lună/an
    ' (grupat pe DataR) -> recepția (IDRR, iconiță după VALOARE: SumaAntet >= 0 -> «up»,
    ' negativă -> «down»). Nivelul de ANTET (IDRH) a fost scos — anteturile rămân doar rânduri în
    ' Tag-ul recepției, agregate în grilă. Aici NU există iconița «+» din Rezervări
    ' (recepțiile n-au nevoie de ea), deci nu se rezervă loc la dreapta.
    ' Rândurile vin ordonate de server (R.NRCRT, R.DataR, H.NrCrt, H.DataH); lunile se
    ' ordonează cronologic, iar în interiorul unei luni „distinct în ordine" e suficient.
    ' Fiecare nod (lună / recepție) poartă în Tag rândurile lui, ca un click să umple grila
    ' (agregat) fără o nouă cerere. Tooltip-ul de reconciliere stă și pe folderul de lună,
    ' și pe recepție.
    Private Sub BuildTree(rows As List(Of ReceptieRow))
        Try
            tree.Clear()
            Dim palette As ThemePalette = TryGetPalette()

            Dim monthItems As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
            Dim receptieItems As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()

            ' Grupare pe lună (an, lună din DataR), cronologic.
            Dim monthGroups = rows.GroupBy(Function(r) MonthKeyOf(r.DataR)).
                                   OrderBy(Function(g) g.Key)

            For Each mg In monthGroups
                Dim monthRows As List(Of ReceptieRow) = mg.ToList()
                ' Totalul lunii = suma SumaAntet pe recepții DISTINCTE ale lunii.
                Dim monthTotal As Double = monthRows.GroupBy(Function(r) r.Idrr).
                                                     Sum(Function(g) g.First().SumaAntet)
                ' NU «lunaIcon»: VB e insensibil la litere mari/mici, deci variabila ar purta
                ' același nume cu funcția LunaIcon și ar umbri-o (capcana din RezervariView).
                Dim icoLuna As Image = LunaIcon()
                Dim monthItem As AdvancedTreeControl.TreeItem =
                    tree.AddItem($"m_{mg.Key}", $"{MonthLabel(mg.Key Mod 100)}~~~{Money(monthTotal)}",
                                 pLeftIconClosed:=icoLuna, pLeftIconOpen:=icoLuna,
                                 pExpanded:=True)
                monthItem.Tag = monthRows
                monthItem.Bold = True
                monthItem.Expanded = False
                monthItems(mg.Key) = monthItem

                ' Recepțiile sub folderul lunii (al DOILEA și ultimul nivel).
                Dim roots As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
                Dim rootRows As New Dictionary(Of Integer, List(Of ReceptieRow))()

                For Each r As ReceptieRow In monthRows
                    ' --- recepția (IDRR) ---
                    Dim root As AdvancedTreeControl.TreeItem = Nothing
                    If Not roots.TryGetValue(r.Idrr, root) Then
                        Dim icon As Image = ValoareIconOf(r.SumaAntet, palette)
                        Dim caption As String =
                            $"{ShortDate(r.DataR)}{SemnReconstituire(r)}~~~{Money(r.SumaAntet)}"
                        root = tree.AddItem($"r_{r.Idrr}", caption, monthItem,
                                            pLeftIconClosed:=icon, pLeftIconOpen:=icon)
                        Dim rr As New List(Of ReceptieRow)()
                        root.Tag = rr
                        roots(r.Idrr) = root
                        rootRows(r.Idrr) = rr
                        receptieItems(r.Idrr) = root
                    End If
                    ' Toate liniile TUTUROR anteturilor recepției intră în Tag-ul ei: nivelul
                    ' de antet nu mai există ca nod, dar agregatul din grilă rămâne complet.
                    rootRows(r.Idrr).Add(r)
                Next
            Next

            ' Tooltip de reconciliere pe folderele de lună ȘI pe recepții (fereastra de plăți
            ' se întinde până la prima recepție a lunii URMĂTOARE — revizuirea operatorului).
            ComputeTooltips(rows, monthItems, receptieItems)

            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("ReceptiiView.BuildTree", ex)
            Throw
        End Try
    End Sub

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

    ' ── Tooltip de reconciliere (lună + recepție) ────────────────────────────
    ' Oglindește NewRootPlatiTooltip din frmFX_MAIN_REC, generalizat la lună (revizuire
    ' operator 2026-07-22): patru rânduri — Data recepție/Lună / Recepții cumulate (difhCum) /
    ' Plăți cumulate (platiCum) / Diferență (difhCum − platiCum, roșu dacă <0, albastru >0).
    '   * difhCum = sumă rulantă a Sum(DIFH) pe recepție (DIFH e per antet — se însumează
    '     anteturile DISTINCTE ale recepției, EXCLUZÂND cele șterse — qFX_MAIN_REC_TT_DIFH
    '     filtrează Sters=False, deși arborele NU-l filtrează), cumulată pe recepții în ordinea
    '     DataR. Recepția = cumul până la ea; luna = cumul până la ULTIMA recepție a lunii.
    '   * platiCum = Sum(Suma) peste plăți cu DataPlata < DataR-ul PRIMEI recepții din LUNA
    '     URMĂTOARE (toate plățile de dinaintea lunii următoare — cerința operatorului).
    '     Ultima lună -> toate plățile. Toate recepțiile aceleiași luni împart aceeași
    '     fereastră de plăți.
    ' valAsoc NU e folosit (linia lui e comentată în Access). Nu se face niciun apel de rețea.
    Private Sub ComputeTooltips(rows As List(Of ReceptieRow),
                                monthItems As Dictionary(Of Integer, AdvancedTreeControl.TreeItem),
                                receptieItems As Dictionary(Of Integer, AdvancedTreeControl.TreeItem))
        Dim plati As List(Of ReceptiePlata) = If(_plati, New List(Of ReceptiePlata)())

        ' Recepții, cronologic pe DataR.
        Dim perReceptie = rows.GroupBy(Function(r) r.Idrr).
            Select(Function(gp) New ReceptieTtRow With {
                .Idrr = gp.Key,
                .DataR = gp.Select(Function(x) x.DataR).FirstOrDefault(Function(d) d.HasValue),
                .MonthKey = MonthKeyOf(gp.Select(Function(x) x.DataR).FirstOrDefault(Function(d) d.HasValue)),
                .SumDifh = SumDistinctAntetDifh(gp)
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
            Dim windowSum As Double
            If boundary.HasValue Then
                windowSum = plati.
                    Where(Function(p) p.DataPlata.HasValue AndAlso p.DataPlata.Value < boundary.Value).
                    Sum(Function(p) p.Suma)
            Else
                windowSum = plati.Sum(Function(p) p.Suma)
            End If
            platiWindowByMonth(monthsOrdered(i)) = windowSum
        Next

        ' Cumulul difuri, în ordinea DataR. Recepția = cumul până la ea; luna reține cumulul
        ' până la ULTIMA recepție a lunii (ultima scriere câștigă).
        Dim difhCum As Double = 0
        Dim monthCum As New Dictionary(Of Integer, Double)()
        For Each rec As ReceptieTtRow In perReceptie
            difhCum += rec.SumDifh
            monthCum(rec.MonthKey) = difhCum
            Dim platiCum As Double = LookupOrZero(platiWindowByMonth, rec.MonthKey)
            Dim ri As AdvancedTreeControl.TreeItem = Nothing
            If receptieItems.TryGetValue(rec.Idrr, ri) Then
                ri.Tooltip = BuildReconTooltipXml("Data recepție", ShortDate(rec.DataR), difhCum, platiCum)
            End If
        Next

        For Each mk As Integer In monthsOrdered
            Dim mi As AdvancedTreeControl.TreeItem = Nothing
            If monthItems.TryGetValue(mk, mi) Then
                mi.Tooltip = BuildReconTooltipXml("Lună", MonthYearLabel(mk),
                                                  LookupOrZero(monthCum, mk),
                                                  LookupOrZero(platiWindowByMonth, mk))
            End If
        Next
    End Sub

    Private Shared Function LookupOrZero(map As Dictionary(Of Integer, Double), key As Integer) As Double
        Dim v As Double
        Return If(map.TryGetValue(key, v), v, 0.0)
    End Function

    ' Sum(DIFH) pe anteturile DISTINCTE ale recepției, sărind cele șterse. DIFH e constant
    ' pe liniile unui antet, deci se ia o dată per IDRH.
    Private Shared Function SumDistinctAntetDifh(recRows As IEnumerable(Of ReceptieRow)) As Double
        Return recRows.Where(Function(r) Not r.StersH).
                       GroupBy(Function(r) r.Idrh).
                       Sum(Function(gp) gp.First().Difh)
    End Function

    ' Construiește tabelul-tooltip XML (<table>) citit de TooltipTableParser al arborelui.
    ' firstLabel/firstValue = primul rând (Data recepție + data, sau Lună + „Ianuarie/2026").
    Private Shared Function BuildReconTooltipXml(firstLabel As String, firstValue As String,
                                                 difhCum As Double, platiCum As Double) As String
        Dim dif As Double = Math.Round(difhCum - platiCum, 2)
        ' Roșu dacă negativ, albastru dacă pozitiv (Switch din Access). ParseColor ia #RRGGBB.
        Dim difColor As String = If(dif < 0, "#CC0000", If(dif > 0, "#0033CC", Nothing))

        Dim sb As New StringBuilder()
        sb.Append("<table>")
        sb.Append("<header>")
        sb.Append("<cell Align=""left"" Bold=""1"">").Append(XmlEscape(firstLabel)).Append("</cell>")
        sb.Append("<cell Align=""right"" Bold=""1"">Valoare</cell>")
        sb.Append("</header>")
        AppendTtRow(sb, firstLabel, firstValue, Nothing)
        AppendTtRow(sb, "Recepții cumulate", Money(difhCum), Nothing)
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
        Public Property SumDifh As Double
    End Class

    ' ── Grila (LISTA) ────────────────────────────────────────────────────────
    ' Detaliul AGREGAT al unui nod (lună / recepție): un rând-
    ' total sintetic „Toți indicatorii" (Valoare = Sum(DIF) pe rândurile nodului), apoi un
    ' rând per clasificație (Valoare = Sum(Valoare) grupat pe Clsf, NrCrt din indicator,
    ' Descriere = Denumirea clasificației — bine definită la orice nivel de agregare, spre
    ' deosebire de descrierea antetului). Grupurile: pe NrCrt apoi Clsf.
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

            ' Rânduri per clasificație — doar liniile reale (un antet fără linii dă doar total).
            Dim lines = nodeRows.Where(Function(r) r.Idr.HasValue)
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

    ' Click pe orice nod (lună / recepție) -> umple grila cu agregatul rândurilor
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
