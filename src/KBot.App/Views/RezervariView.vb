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
''' Vederea Rezervări (felia 0014) — echivalentul Access frmFX_MAIN_REZ: un master/detail
''' cu un arbore de rezervări la stânga (foldere pe lună + frunze pe (dată, tip)) și o
''' grilă continuă la dreapta cu detaliul clasificațiilor. Read-only în această felie:
''' generarea DDF declanșată de «+» este o felie ulterioară — aici «+» doar ridică un
''' eveniment. Datele vin din GET /api/forexe/rezervari, întotdeauna prin plasa de
''' re-autentificare a shell-ului (401 -> re-login -> reia o dată).
''' </summary>
Public Class RezervariView
    Implements IAngajamentView, IThemedControl

    ' Cheile coloanelor grilei — o singură definiție, folosită la creare și la umplere,
    ' ca un typo să nu ajungă o coloană goală în producție.
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_CREDIT_BUG As String = "credit_bug"
    Private Const COL_INITIALA As String = "r_initiala"
    Private Const COL_VALOARE As String = "r_valoare"
    Private Const COL_DEFINITIVA As String = "r_definitiva"

    ' CHEILE ICONIȚELOR din «image_list» (ImageList-ul pus pe vedere în designer și legat de
    ' arbore prin tree.NodeImages). Arborele rezolvă o cheie prin AdvancedTreeControl.NodeImage:
    ' cheia e numele dat pozei în editorul de ImageList, iar o cheie lipsă întoarce Nothing.
    ' Ținute ca și constante ca un typo să nu ajungă un nod fără iconiță în producție.
    Private Const ICO_LUNA As String = "month"      ' folderul de lună
    Private Const ICO_MARIRE As String = "up"       ' frunză: Mărire ▲
    Private Const ICO_MICSORARE As String = "down"  ' frunză: Micșorare ▼
    Private Const ICO_INITIALA As String = "equal"  ' frunză: Inițială «=»
    Private Const ICO_PLUS As String = "plus"       ' iconița dreapta «adaugă DDF»

    ' Format românesc: separator de mii «.» și zecimală «,» (1.091.940,00).
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    ' Plasa 401 a shell-ului (MainForm.WithReauth), specializată pe RezervariInfo:
    ' politica de re-login rămâne într-un singur loc, vederea doar o folosește.
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of RezervariInfo)), Task(Of RezervariInfo))

    ' Codul angajamentului CERUT ultima dată — vezi stale-guard din LoadAsync (identic
    ' cu SumarView): operatorul parcurge arborele rapid, iar un răspuns depășit se aruncă.
    Private _requestedCod As String

    ' Ultimele rânduri încărcate — păstrate ca ApplyTheme să poată reconstrui arborele
    ' (re-tintarea iconițelor) fără o nouă cerere de rețea.
    Private _rows As List(Of RezervareRow)

    ' Starea splitter-ului dinainte de strângerea arborelui, ca desfacerea să-l pună înapoi
    ' exact unde era (vezi tree_CollapsedChanged). 0 = arborele n-a fost încă strâns.
    Private _splitterDistanceDesfasurat As Integer
    Private _panel1MinSizeDesfasurat As Integer

    ''' <summary>
    ''' The DDF write commands (slice 0051), executed by the shell. Optional: a host that does
    ''' not supply one (the tests) still gets the whole read-only view, and the "+" icon says
    ''' so rather than doing nothing.
    '''
    ''' <para>This took the place of the dormant <c>AdaugaDdfCerut</c> event of slice 0014.
    ''' That event existed because the DDF workflow was a later slice; the later slice is
    ''' here. The command goes through the shell for the same reason <c>DdfView</c>'s does --
    ''' the 401 re-login net is private and generic in <c>MainForm</c>, so the policy stays in
    ''' one place.</para>
    ''' </summary>
    Private ReadOnly _executaComanda As Action(Of DdfComanda)

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of RezervariInfo)), Task(Of RezervariInfo)),
                   Optional executaComanda As Action(Of DdfComanda) = Nothing)
        ArgumentNullException.ThrowIfNull(apiClient)
        ArgumentNullException.ThrowIfNull(withReauth)
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        _executaComanda = executaComanda
        'BuildColumns()
        ShowEmpty("Selectați un angajament din arbore.")
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "rezervari"
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
            GlobalErrorLog.Write("RezervariView.tree_CollapsedChanged", ex)
        End Try
    End Sub

    ' Distanța splitter-ului adusă în intervalul acceptat de SplitContainer — o vedere îngustă
    ' n-are voie să transforme apăsarea butonului de strângere într-o excepție.
    Private Function ClampSplitter(dorit As Integer) As Integer
        Dim maxim As Integer = split.Width - split.Panel2MinSize - split.SplitterWidth
        If maxim < split.Panel1MinSize Then Return split.Panel1MinSize
        Return Math.Max(split.Panel1MinSize, Math.Min(dorit, maxim))
    End Function

    ' Coloanele grilei = frmFX_MAIN_REZ_LISTA: Clsf + patru coloane de bani. Toate sunt
    ' Text cu N2 aliniat la dreapta (read-only, deci un tip numeric ar fi degeaba).
    'Private Sub BuildColumns()
    '    Try
    '        grid.AddColumn(COL_CLSF, "Clasificație", KBotColumnType.Text, 190)
    '        AddMoneyColumn(COL_CREDIT_BUG, "Credit bugetar")
    '        AddMoneyColumn(COL_INITIALA, "Rezervări inițiale")
    '        AddMoneyColumn(COL_VALOARE, "Rezervare curentă")
    '        AddMoneyColumn(COL_DEFINITIVA, "Rezervări definitive")
    '        ' Clasificația e cea după care se citește tabelul — rămâne fixă la stânga.
    '        grid.FrozenColumnCount = 1
    '    Catch ex As Exception
    '        GlobalErrorLog.Write("RezervariView.BuildColumns", ex)
    '        Throw
    '    End Try
    'End Sub

    'Private Sub AddMoneyColumn(key As String, header As String)
    '    Dim col As KBotDataColumn = grid.AddColumn(key, header, KBotColumnType.Text, 130)
    '    col.FormatString = "N2"
    '    col.TextAlign = ContentAlignment.MiddleRight
    'End Sub

    ''' <summary>
    ''' Selecția din arbore s-a schimbat. Fără angajament (nod de capitol / deselectare)
    ''' NU se face niciun apel de rețea — doar se golește vederea.
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
            ShowEmpty("Se încarcă rezervările…")
            ' Fire-and-forget deliberat (handler sincron al shell-ului): metoda își
            ' tratează singură TOATE erorile — vezi comentariul din SumarView.
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("RezervariView.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Boundary UI: logăm și ARĂTĂM eroarea, nu o aruncăm mai departe (apelul e pornit
    ' fără await din SetContext, deci nu există cine să o prindă).
    Private Async Sub LoadAsync(cod As String)
        Try
            Dim data As RezervariInfo = Await _withReauth(
                Function() _apiClient.GetRezervariAsync(cod, CancellationToken.None)).ConfigureAwait(True)

            ' Răspuns depășit: între timp operatorul a selectat alt angajament. Îl aruncăm.
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return

            Dim rows As List(Of RezervareRow) = If(data Is Nothing, New List(Of RezervareRow)(), data.Rows)
            If rows Is Nothing OrElse rows.Count = 0 Then
                _rows = Nothing
                tree.Clear()
                grid.ClearRows()
                ShowEmpty("Angajamentul nu are rezervări.")
                Return
            End If

            _rows = rows
            BuildTree(rows)
            ' „Nimic selectat" -> grila arată TOTALURILE pe clasificație peste tot angajamentul
            ' (decizia §7.3 + revizuirea operator 2026-08-13: agregat, ca în Recepții).
            FillGridAgregat(rows)
            ShowContent()
        Catch ex As ApiException
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("RezervariView.LoadAsync", ex)
            _rows = Nothing
            tree.Clear()
            grid.ClearRows()
            ShowEmpty(ex.Message)   ' mesaj românesc din câmpul «error» al serverului
        Catch ex As Exception
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("RezervariView.LoadAsync", ex)
            _rows = Nothing
            tree.Clear()
            grid.ClearRows()
            ShowEmpty("Rezervările nu au putut fi încărcate. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' ── Arborele ─────────────────────────────────────────────────────────────
    ' Foldere pe (an, lună) cu total = SUM(R_Valoare) (confirmat de coloana TOTALL din
    ' qFX_REZERVARI_TREE). Frunze pe (dată, tip) cu valoare = SUM(ValoareOperatie)
    ' (= Suma din QFX_DDF_REZERVARI). Iconița stângă = tipul; iconița «+» apare doar dacă
    ' grupul are cel puțin un rând cu AreDDF = False. Fiecare nod poartă în Tag rândurile
    ' lui, ca un click să filtreze grila fără o nouă cerere.
    Private Sub BuildTree(rows As List(Of RezervareRow))
        Try
            tree.Clear()
            Dim palette As ThemePalette = TryGetPalette()

            ' «+» pe EXACT o frunză (fix 0017-04): oglindește latch-ul one-shot `existaNodCuRIcon`
            ' din Show_Rezervari — PRIMUL nod cu o rezervare fără DDF (IDREV IS NULL -> AreDDF=False)
            ' primește iconița, restul niciuna. Access ordonează după (DataRezervare, IDH, Clsf,
            ' strData); IDH NU e în payload-ul /rezervari, deci mergem pe (dată, tip) — cheia primară
            ' de dată + ordinea de tip (Inițială<Mărire<Micșorare, ca strData), fără tiebreak-ul IDH.
            ' Frunza marcată = (dată, tip)-ul primului rând eligibil în această ordine.
            Dim plusDate As Date? = Nothing
            Dim plusTip As RezervareTip = RezervareTip.Necunoscut
            Dim firstEligible As RezervareRow =
                rows.Where(Function(r) Not r.AreDDF).
                     OrderBy(Function(r) r.DataRezervare.Date).
                     ThenBy(Function(r) CInt(r.Tip)).
                     FirstOrDefault()
            If firstEligible IsNot Nothing Then
                plusDate = firstEligible.DataRezervare.Date
                plusTip = firstEligible.Tip
            End If

            ' Luni în ordine cronologică.
            Dim months = rows.GroupBy(Function(r) New With {Key .Y = r.DataRezervare.Year, Key .M = r.DataRezervare.Month}).
                              OrderBy(Function(gp) gp.Key.Y).ThenBy(Function(gp) gp.Key.M)

            For Each monthGroup In months
                Dim monthRows As List(Of RezervareRow) = monthGroup.ToList()
                Dim total As Double = monthRows.Sum(Function(r) r.RValoare)
                Dim y As Integer = monthGroup.Key.Y
                Dim m As Integer = monthGroup.Key.M
                Dim monthKey As String = $"LA_{y}_{m}"
                Dim monthCaption As String = $"{MonthLabel(m)}~~~{Money(total)}"
                ' NU «lunaIcon»: VB e insensibil la litere mari/mici, deci variabila ar fi
                ' același nume cu funcția LunaIcon și ar umbri-o.
                Dim icoLuna As Image = LunaIcon()
                ' Arborele pornește STRÂNS: se desface DOAR luna care conține frunza cu «+»
                ' (cerință operator) — adică singurul loc unde mai e ceva de făcut. Fără nicio
                ' frunză eligibilă, toate lunile rămân strânse.
                Dim areFrunzaCuPlus As Boolean = plusDate.HasValue AndAlso
                                                 plusDate.Value.Year = y AndAlso
                                                 plusDate.Value.Month = m
                Dim root As AdvancedTreeControl.TreeItem =
                    tree.AddItem(monthKey, monthCaption,
                                 pLeftIconClosed:=icoLuna, pLeftIconOpen:=icoLuna,
                                 pExpanded:=areFrunzaCuPlus)
                root.Tag = monthRows
                root.Bold = True
                ' Frunze pe (dată, tip), ordonate pe dată apoi pe rangul tipului
                ' (Inițială < Mărire < Micșorare, ca strData din Access).
                Dim leaves = monthRows.GroupBy(Function(r) New With {Key .D = r.DataRezervare.Date, Key .T = r.Tip}).
                                       OrderBy(Function(gp) gp.Key.D).ThenBy(Function(gp) CInt(gp.Key.T))

                For Each leafGroup In leaves
                    Dim leafRows As List(Of RezervareRow) = leafGroup.ToList()
                    Dim d As Date = leafGroup.Key.D
                    Dim tip As RezervareTip = leafGroup.Key.T
                    Dim leafValue As Double = leafRows.Sum(Function(r) r.ValoareOperatie)
                    ' «+» pe EXACT o frunză. Testul e «ESTE frunza aleasă mai sus?», NU «are
                    ' frunza asta vreun rând fără DDF?» — al doilea e adevărat pe TOATE frunzele
                    ' de la prima eligibilă încolo, deci punea «+» pe un șir de noduri.
                    Dim hasPlus As Boolean = plusDate.HasValue AndAlso
                                             d = plusDate.Value AndAlso
                                             tip = plusTip

                    Dim leafKey As String = $"RZ_{d:yyyyMMdd}_{CInt(tip)}"
                    Dim leafCaption As String = $"{d:dd.MM.yyyy}~~~{Money(leafValue)}"

                    'not using these anymore, but the actual images in the image_list
                    Dim leftIcon As Image = TipIconOf(tip, palette)
                    Dim rightIcon As Image = If(hasPlus, PlusIconOf(tip, palette), Nothing)

                    Dim leaf As AdvancedTreeControl.TreeItem =
                        tree.AddItem(leafKey, leafCaption, root,
                                     pLeftIconClosed:=leftIcon, pLeftIconOpen:=leftIcon,
                                     pRightIcon:=rightIcon)
                    leaf.Tag = leafRows
                    ' Valoare negativă -> nod roșu (ca cNode.foreColor = vbRed din Access).
                    If leafValue < 0 AndAlso palette IsNot Nothing Then
                        leaf.NodeForeColor = palette.ErrorColor
                    End If
                Next
            Next

            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("RezervariView.BuildTree", ex)
            Throw
        End Try
    End Sub

    ' ── Grila ────────────────────────────────────────────────────────────────
    ' BeginUpdate/EndUpdate: o singură repictare la final, nu una per rând.
    Private Sub FillGrid(rows As List(Of RezervareRow))
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            If rows IsNot Nothing Then
                For Each r As RezervareRow In rows
                    Dim row As KBotDataRow = grid.AddRow()
                    row(COL_CLSF) = r.Clsf
                    row(COL_CREDIT_BUG) = r.RCreditBug
                    row(COL_INITIALA) = r.RInitiala
                    row(COL_VALOARE) = r.RValoare
                    row(COL_DEFINITIVA) = r.RDefinitiva
                Next
            End If
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' Grila AGREGATĂ pe clasificație — starea „nimic selectat" (revizuire operator
    ''' 2026-08-13, ca în ReceptiiView): un rând per clasificație, nu unul per înregistrare
    ''' FX_Rezervari. Sumele merg pe cele trei coloane de operație (inițială / valoare /
    ''' definitivă), fiindcă acelea sunt sume de operații și se adună.
    '''
    ''' «Credit bugetar» NU se adună: e creditul indicatorului la data rezervării, nu o sumă
    ''' de operație, iar adunarea lui peste rezervările aceleiași clasificații l-ar înmulți cu
    ''' numărul lor. Se ia valoarea de la CEA MAI RECENTĂ rezervare a clasificației — creditul
    ''' în vigoare. (Presupunere; dacă operatorul îl vrea altfel — prima valoare, sau chiar
    ''' suma — se schimbă doar linia asta.)
    ''' </summary>
    Private Sub FillGridAgregat(rows As List(Of RezervareRow))
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            If rows Is Nothing Then Return

            Dim grupuri = rows.GroupBy(Function(r) r.Clsf).
                               OrderBy(Function(gp) gp.Key, StringComparer.Ordinal)
            For Each gp In grupuri
                Dim row As KBotDataRow = grid.AddRow()
                row(COL_CLSF) = gp.Key
                row(COL_CREDIT_BUG) = gp.OrderByDescending(Function(r) r.DataRezervare).
                                         First().RCreditBug
                row(COL_INITIALA) = gp.Sum(Function(r) r.RInitiala)
                row(COL_VALOARE) = gp.Sum(Function(r) r.RValoare)
                row(COL_DEFINITIVA) = gp.Sum(Function(r) r.RDefinitiva)
            Next
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    ' Click pe un nod -> filtrează grila la rândurile nodului (lună sau frunză), rând cu rând:
    ' agregarea e starea „nimic selectat", iar un nod ales cere detaliul lui. Rândurile
    ' stau în Tag, puse la construcția arborelui — niciun apel de rețea aici.
    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing Then Return
            Dim rows As List(Of RezervareRow) = TryCast(pNode.Tag, List(Of RezervareRow))
            If rows Is Nothing Then Return
            FillGrid(rows)
        Catch ex As Exception
            GlobalErrorLog.Write("RezervariView.tree_NodeMouseUp", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The "+" icon of a leaf -- the port of <c>mcTree_RightIconClick</c> in
    ''' <c>frmFX_MAIN_REZ</c>, which raised <c>AdaugaRevizie(CBool(cNode.Value2))</c> and
    ''' landed in <c>FX_Adaugare_DDF</c>. Here it opens the DDF editor (slice 0051) through
    ''' the shell.
    '''
    ''' <para><c>Value2</c> was the row's <c>EInitiala</c>, and that flag is the whole of the
    ''' choice between the two add actions: an INITIAL reservation opens the first revision,
    ''' which also creates the document; anything else opens a further revision on the
    ''' document that already exists. The choice is NOT second-guessed here. The server
    ''' refuses each of the two when the document's state contradicts it -- in Romanian --
    ''' and that refusal reaches the operator through <c>MainForm.ExecutaComandaDdf</c>.</para>
    '''
    ''' <para>The row consulted is the FIRST one of the leaf that has no DDF, not simply
    ''' <c>rows(0)</c>: that is the row <c>Show_Rezervari</c> was standing on when it set the
    ''' icon, and one leaf can hold rows of both kinds.</para>
    ''' </summary>
    Private Sub Tree_RightIconClicked(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            If pNode Is Nothing Then Return
            Dim rows As List(Of RezervareRow) = TryCast(pNode.Tag, List(Of RezervareRow))
            If rows Is Nothing OrElse rows.Count = 0 Then Return
            If String.IsNullOrWhiteSpace(_requestedCod) Then Return

            Dim tinta As RezervareRow = rows.FirstOrDefault(Function(r) Not r.AreDDF)
            If tinta Is Nothing Then tinta = rows(0)

            Dim actiune As DdfActiune = If(tinta.EInitiala,
                                           DdfActiune.AdaugaRevizieInitiala,
                                           DdfActiune.Adauga)
            CereComanda(New DdfComanda(actiune, _requestedCod))
        Catch ex As Exception
            GlobalErrorLog.Write("RezervariView.tree_RightIconClicked", ex)
        End Try
    End Sub

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
    ''' Reloads the angajament after a DDF write (called by <c>MainForm.DupaScriereaDdf</c>).
    ''' A save marks reservations as having a DDF and a delete releases them again, so the
    ''' "+" icon this tree draws is no longer where it belongs. Goes through the SAME
    ''' <c>LoadAsync</c> a normal selection uses -- there is no second loading route to keep
    ''' in step, and its stale-guard still applies.
    ''' </summary>
    Public Sub Reincarca()
        Try
            Dim cod As String = _requestedCod
            If String.IsNullOrWhiteSpace(cod) Then Return
            ShowEmpty("Se încarcă rezervările…")
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("RezervariView.Reincarca", ex)
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

    ' Numele lunii în română (Ianuarie, Februarie…), cu prima literă mare.
    Private Shared Function MonthLabel(month As Integer) As String
        Dim name As String = _roCulture.DateTimeFormat.GetMonthName(month)
        If String.IsNullOrEmpty(name) Then Return CStr(month)
        Return Char.ToUpper(name(0), _roCulture) & name.Substring(1)
    End Function

    ''' <summary>
    ''' Iconița frunzei, după tipul operației. ÎNTÂI din «image_list» (pozele alese de operator
    ''' în designer), și abia dacă lista n-are cheia respectivă se cade înapoi pe formele GDI din
    ''' <see cref="RezervariIcons"/> — altfel un ImageList incomplet ar lăsa noduri fără iconiță.
    ''' Cum se scapă de fallback: pui în «image_list» pozele cu cheile de mai sus.
    ''' </summary>
    Private Function TipIconOf(tip As RezervareTip, palette As ThemePalette) As Image
        Dim cheie As String
        Select Case tip
            Case RezervareTip.Marire : cheie = ICO_MARIRE
            Case RezervareTip.Micsorare : cheie = ICO_MICSORARE
            Case RezervareTip.Initiala : cheie = ICO_INITIALA
            Case Else : Return Nothing          ' Necunoscut -> fără iconiță, ca până acum
        End Select

        Dim dinLista As Image = tree.NodeImage(cheie)
        If dinLista IsNot Nothing Then Return dinLista

        ' Fallback GDI (se re-tintează pe paletă; imaginile din listă sunt fixe).
        If palette Is Nothing Then Return Nothing
        Dim color As Color
        Select Case tip
            Case RezervareTip.Marire : color = palette.SuccessColor
            Case RezervareTip.Micsorare : color = palette.ErrorColor
            Case Else : color = palette.TextColor       ' Inițială («=»)
        End Select
        Return RezervariIcons.TipIcon(tip, color, tree.LeftIconSize.Width)
    End Function

    ''' <summary>Iconița folderului de lună; doar din «image_list» (n-a avut niciodată formă GDI).</summary>
    Private Function LunaIcon() As Image
        Return tree.NodeImage(ICO_LUNA)
    End Function

    ''' <summary>Iconița «+» (adaugă DDF), cu aceeași regulă listă-întâi ca <see cref="TipIconOf"/>.</summary>
    Private Function PlusIconOf(tip As RezervareTip, palette As ThemePalette) As Image
        Dim dinLista As Image = tree.NodeImage(ICO_PLUS)
        If dinLista IsNot Nothing Then Return dinLista

        If palette Is Nothing Then Return Nothing
        ' Plus_Green pentru operația inițială, altfel accent — ca în Access.
        Dim color As Color = If(tip = RezervareTip.Initiala, palette.SuccessColor, palette.AccentColor)
        Return RezervariIcons.PlusIcon(color, tree.RightIconSize.Width)
    End Function

    Private Shared Function TryGetPalette() As ThemePalette
        ' Headless (teste) sau înainte de inițializarea temei: ThemeManager.Current poate
        ' fi Nothing. Atunci arborele se construiește fără iconițe/culori (structura e
        ' aceeași), iar ApplyTheme reconstruiește când tema devine disponibilă.
        Dim current As ThemeScheme = ThemeManager.Current
        Return current?.Palette
    End Function

    ''' <summary>
    ''' Reaplică culorile schemei pe arbore + starea goală (grila se auto-temează:
    ''' KBotDataView implementează el însuși IThemedControl). Reconstruiește arborele
    ''' dacă are date, ca iconițele să se re-tinteze pe noua paletă.
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

            ' Re-tintarea iconițelor de tip/«+» pe noua paletă. Arborele se reconstruiește,
            ' deci selecția se pierde — grila se întoarce la starea „nimic selectat".
            If _rows IsNot Nothing AndAlso _rows.Count > 0 Then
                BuildTree(_rows)
                FillGridAgregat(_rows)
            End If
        Catch ex As Exception
            ' Boundary UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("RezervariView.ApplyTheme", ex)
        End Try
    End Sub
End Class
