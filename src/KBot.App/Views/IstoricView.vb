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
''' Vederea Istoric (felia 0022) — echivalentul Access frmFX_ISTORIC (acolo un dialog popup,
''' aici o vedere găzduită în shell). Read-only, fără nicio scriere. Arată fiecare rând
''' FX_Istoric al angajamentului selectat, cu cele trei filtre pe coloană pe care le avea
''' Access (Clasificație / TipRand / DataFX), un rând de totaluri pe cele trei coloane pe care
''' Access le totaliza, și un panou de detaliu pentru cele două câmpuri lungi (Descriere,
''' Observații).
'''
''' Datele vin dintr-un SINGUR apel GET /api/forexe/istoric pentru tot CodAngajament-ul, prin
''' plasa de re-autentificare a shell-ului (401 -> re-login -> reia o dată). Cele trei meniuri
''' și filtrarea rulează LOCAL, fără alte cereri (decizia din 0020-01 §7).
'''
''' ABATERI DELIBERATE de la Access (în worklog-ul feliei, §9):
'''   * meniurile TipRand și DataFX sunt SCOPATE la angajamentul curent (Access interoga
'''     FX_Istoric NEscopat — un defect, listând valori din toată baza);
'''   * TipRand se potrivește CASE-INSENSITIVE (Access lua asta din Option Compare Database);
'''   * `cod` e potrivire EXACTĂ (Access folosea Like);
'''   * rândul Access pe două benzi e aplatizat la un rând de grilă + panoul de detaliu;
'''   * captionul Alineat = Clasificatii.Denumire (decizie de operator);
'''   * butonul `bFilter` care urmărea mouse-ul e înlocuit cu o bandă de trei butoane de meniu.
''' </summary>
Public Class IstoricView
    Implements IAngajamentView, IThemedControl

    ' Cheile coloanelor grilei principale. Cererea operatorului: FĂRĂ coloanele de valori și
    ' FĂRĂ rând de totaluri; ultima coloană («Observații») se întinde. Valorile trec în grila
    ' mică din panoul de detaliu (vezi COL_V_TIP / COL_V_VAL).
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_TIP As String = "tip"
    Private Const COL_DATA As String = "data"
    Private Const COL_DESC As String = "desc"
    Private Const COL_OBS As String = "obs"

    ' Cheile grilei mici de valori (panoul de detaliu): un rând per valoare <> 0 a rândului
    ' selectat — un „unpivot"/crosstab pe rând (Tip | Valoare).
    Private Const COL_V_TIP As String = "vtip"
    Private Const COL_V_VAL As String = "vval"

    ' Format românesc: separator de mii «.» și zecimală «,» (1.091.940,00).
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    ' Plasa 401 a shell-ului (MainForm.WithReauth), specializată pe IstoricInfo.
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of IstoricInfo)), Task(Of IstoricInfo))

    ' Codul angajamentului CERUT ultima dată — stale-guard (identic cu DDF/Plăți/Recepții).
    Private _requestedCod As String

    ' Ultimele rânduri încărcate — sursa grilei și a celor trei meniuri; filtrarea se aplică
    ' peste ele, fără o nouă cerere de rețea.
    Private _rows As List(Of IstoricRand)
    ' Ultima ierarhie de clasificații — sursa meniului de clasificație.
    Private _clasificatii As List(Of IstoricClasificatie)

    ' Motorul de filtrare pur (portul ApplyColumnFilter) — trei segmente independente.
    Private ReadOnly _filter As New IstoricFilter()

    ''' <summary>
    ''' Rândul selectat s-a schimbat — oglindește evenimentul dormant Access
    ''' <c>Public Event RowChanged(key)</c> (cu <c>RaiseEvent</c> comentat) și textbox-ul ascuns
    ''' <c>CL = Me!ID</c>. Poartă <c>ID</c>-ul rândului. INTENȚIONAT fără abonat în această felie
    ''' (aceeași formă ca <c>AdaugaDdfCerut</c> din Rezervări) — a NU se „curăța" ca membru nefolosit.
    ''' </summary>
    Public Event RandSchimbat(sender As Object, e As IstoricRandEventArgs)

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of IstoricInfo)), Task(Of IstoricInfo)))
        If apiClient Is Nothing Then Throw New ArgumentNullException(NameOf(apiClient))
        If withReauth Is Nothing Then Throw New ArgumentNullException(NameOf(withReauth))
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        BuildColumns()
        BuildValoriColumns()
        ShowEmpty("Selectați un angajament din arbore.")
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "istoric"
        End Get
    End Property

    ' Grila principală: Clasificație, Tip rând, Data, Descriere, Observații — FĂRĂ coloanele de
    ' valori și FĂRĂ rând de totaluri (cererea operatorului). «Observații» e ultima și se întinde
    ' (ColumnFillMode.LastColumn din Designer). Valorile <> 0 se mută în grila mică de detaliu.
    Private Sub BuildColumns()
        Try
            grid.AddColumn(COL_CLSF, "Clasificație", KBotColumnType.Text, 150)
            grid.AddColumn(COL_TIP, "Tip rând", KBotColumnType.Text, 120)
            grid.AddColumn(COL_DATA, "Data", KBotColumnType.Text, 90)
            grid.AddColumn(COL_DESC, "Descriere", KBotColumnType.Text, 240)
            grid.AddColumn(COL_OBS, "Observații", KBotColumnType.Text, 240)
        Catch ex As Exception
            GlobalErrorLog.Write("IstoricView.BuildColumns", ex)
            Throw
        End Try
    End Sub

    ' Grila mică de detaliu: Tip (text) | Valoare (N2, dreapta). Un rând per valoare <> 0 a
    ' rândului selectat (crosstab/unpivot pe rând). Fără totaluri.
    Private Sub BuildValoriColumns()
        Try
            gridValori.AddColumn(COL_V_TIP, "Tip", KBotColumnType.Text, 150)
            Dim colVal As KBotDataColumn = gridValori.AddColumn(COL_V_VAL, "Valoare", KBotColumnType.Text, 120)
            colVal.FormatString = "N2"
            colVal.TextAlign = ContentAlignment.MiddleRight
        Catch ex As Exception
            GlobalErrorLog.Write("IstoricView.BuildValoriColumns", ex)
            Throw
        End Try
    End Sub

    ' Etichetele celor șapte valori, în ordinea Access. Perechea (etichetă, extractor) e sursa
    ' unpivot-ului din grila de detaliu.
    Private Shared ReadOnly _valori As (Eticheta As String, Extractor As Func(Of IstoricRand, Double))() = {
        ("Rezervare inițială", Function(r) r.ValRezervareI),
        ("Rezervare definitivă", Function(r) r.ValRezervareD),
        ("Rezervare anterioară", Function(r) r.ValRezervareAnt),
        ("Rezervare diferență", Function(r) r.ValRezervareDif),
        ("Angajament legal", Function(r) r.ValAngLeg),
        ("Recepție", Function(r) r.ValReceptie),
        ("Plată", Function(r) r.ValPlata)
    }

    ''' <summary>
    ''' Selecția din arbore s-a schimbat. Fără angajament (nod de capitol / deselectare) NU se
    ''' face niciun apel de rețea — doar se golește vederea (§6).
    ''' </summary>
    Public Sub SetContext(info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        Try
            Dim cod As String = If(info Is Nothing, Nothing, info.CodAngajament)
            If String.IsNullOrWhiteSpace(cod) Then
                ClearAll()
                ShowEmpty("Selectați un angajament din arbore.")
                Return
            End If

            _requestedCod = cod
            ShowEmpty("Se încarcă istoricul…")
            ' Fire-and-forget deliberat (handler sincron al shell-ului): metoda își tratează
            ' singură TOATE erorile — vezi comentariul din PlatiView/DdfView.
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("IstoricView.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Boundary UI: logăm și ARĂTĂM eroarea, nu o aruncăm mai departe (apelul e pornit fără
    ' await din SetContext, deci nu există cine să o prindă).
    Private Async Sub LoadAsync(cod As String)
        Try
            Dim data As IstoricInfo = Await _withReauth(
                Function() _apiClient.GetIstoricAsync(cod, CancellationToken.None)).ConfigureAwait(True)

            ' Răspuns depășit: între timp operatorul a selectat alt angajament. Îl aruncăm (§6).
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return

            Dim rows As List(Of IstoricRand) =
                If(data Is Nothing, New List(Of IstoricRand)(), data.Randuri)
            If rows Is Nothing OrElse rows.Count = 0 Then
                ClearAll()
                ShowEmpty("Angajamentul nu are istoric.")
                Return
            End If

            _rows = rows
            _clasificatii = If(data.Clasificatii, New List(Of IstoricClasificatie)())
            ' Filtrul din angajamentul precedent NU are voie să supraviețuiască (§6, aceeași
            ' regulă necondiționată ca resetul combo-ului din DdfView).
            _filter.ClearAll()
            BuildMenus()
            FillFiltered()
            ShowContent()
        Catch ex As ApiException
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("IstoricView.LoadAsync", ex)
            ClearAll()
            ShowEmpty(ex.Message)   ' mesaj românesc din câmpul «error» al serverului
        Catch ex As Exception
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("IstoricView.LoadAsync", ex)
            ClearAll()
            ShowEmpty("Istoricul nu a putut fi încărcat. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' ── Grila ────────────────────────────────────────────────────────────────
    ' Umple grila cu rândurile care trec de filtrul curent și actualizează rezumatul benzii.
    ' Fiecare rând poartă în Tag IstoricRand-ul, ca selecția să umple panoul de detaliu fără
    ' un alt apel. Ordinea serverului (DataFX, Clsf) e păstrată.
    Private Sub FillFiltered()
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            Dim vizibile As List(Of IstoricRand) = _filter.Apply(_rows)
            For Each r As IstoricRand In vizibile
                Dim row As KBotDataRow = grid.AddRow()
                row.Tag = r
                row(COL_CLSF) = r.Clsf
                row(COL_TIP) = r.TipRand
                row(COL_DATA) = ShortDate(r.DataFx)
                row(COL_DESC) = r.Descriere
                row(COL_OBS) = r.Observatii
            Next
        Finally
            grid.EndUpdate()
        End Try
        lblFiltruActiv.Text = _filter.Summary
        ' Nimic selectat imediat după umplere (ClearRows nu ridică SelectionChanged).
        UpdateDetail(Nothing)
    End Sub

    ' Panoul de detaliu urmează selecția din grilă (din datele deja pe rând). Ridică și
    ' evenimentul dormant RandSchimbat cu ID-ul rândului (fără abonat, §7).
    Private Sub grid_SelectionChanged(sender As Object, e As EventArgs) Handles grid.SelectionChanged
        Try
            Dim cur As KBotDataRow = grid.CurrentRow
            Dim r As IstoricRand = If(cur Is Nothing, Nothing, TryCast(cur.Tag, IstoricRand))
            UpdateDetail(r)
            RaiseEvent RandSchimbat(Me, New IstoricRandEventArgs(If(r Is Nothing, 0, r.Id)))
        Catch ex As Exception
            GlobalErrorLog.Write("IstoricView.grid_SelectionChanged", ex)
        End Try
    End Sub

    ' Fără rând -> Descriere goală + grila de valori goală. Cu rând -> Descriere + un rând per
    ' valoare <> 0 (Tip | Valoare).
    Private Sub UpdateDetail(r As IstoricRand)
        txtDescriere.Text = If(r Is Nothing, String.Empty, r.Descriere)
        gridValori.BeginUpdate()
        Try
            gridValori.ClearRows()
            If r IsNot Nothing Then
                For Each v In _valori
                    Dim val As Double = v.Extractor(r)
                    If val <> 0 Then
                        Dim vr As KBotDataRow = gridValori.AddRow()
                        vr(COL_V_TIP) = v.Eticheta
                        vr(COL_V_VAL) = val
                    End If
                Next
            End If
        Finally
            gridValori.EndUpdate()
        End Try
    End Sub

    ' ── Cele trei meniuri de filtrare (construite din datele încărcate) ───────
    Private Sub BuildMenus()
        BuildClsfMenu()
        BuildTipRandMenu()
        BuildDataMenu()
    End Sub

    ' Clasificație: ierarhic Subcapitol -> Articol -> Alineat. «TOATE» pe rădăcină; un «TOATE»
    ' per nivel DOAR unde acel nivel are mai mult de un copil (regula Access dictArtPerSub /
    ' dictAlinPerArt). Captiunile sunt cele trei câmpuri den_*.
    Private Sub BuildClsfMenu()
        menuClsf.Items.Clear()
        menuClsf.Items.Add(NewItem("TOATE", Sub() ApplyFilterChange(Sub() _filter.ClearClsf())))
        If _clasificatii Is Nothing Then Return

        For Each subGrp In _clasificatii.GroupBy(Function(c) c.Subcapitol).OrderBy(Function(g) g.Key, StringComparer.Ordinal)
            Dim subRows As List(Of IstoricClasificatie) = subGrp.ToList()
            Dim subItem As New ToolStripMenuItem(CaptionOf(FirstNonEmpty(subRows.Select(Function(c) c.DenSubcapitol)), subGrp.Key))

            Dim articole = subRows.GroupBy(Function(c) c.Articol).OrderBy(Function(g) g.Key, StringComparer.Ordinal).ToList()
            ' «TOATE» pe subcapitol doar când are mai mult de un articol.
            If articole.Count > 1 Then
                Dim idsSub As List(Of Integer) = subRows.Select(Function(c) c.IdClsf).Distinct().ToList()
                Dim labelSub As String = CaptionOf(FirstNonEmpty(subRows.Select(Function(c) c.DenSubcapitol)), subGrp.Key)
                subItem.DropDownItems.Add(NewItem("TOATE", Sub() ApplyFilterChange(Sub() _filter.SetClsf(idsSub, labelSub))))
            End If

            For Each artGrp In articole
                Dim artRows As List(Of IstoricClasificatie) = artGrp.ToList()
                Dim artItem As New ToolStripMenuItem(CaptionOf(FirstNonEmpty(artRows.Select(Function(c) c.DenArticol)), artGrp.Key))

                ' «TOATE» pe articol doar când are mai mult de un alineat.
                If artRows.Count > 1 Then
                    Dim idsArt As List(Of Integer) = artRows.Select(Function(c) c.IdClsf).Distinct().ToList()
                    Dim labelArt As String = CaptionOf(FirstNonEmpty(artRows.Select(Function(c) c.DenArticol)), artGrp.Key)
                    artItem.DropDownItems.Add(NewItem("TOATE", Sub() ApplyFilterChange(Sub() _filter.SetClsf(idsArt, labelArt))))
                End If

                For Each leaf As IstoricClasificatie In artRows
                    Dim id As Integer = leaf.IdClsf
                    Dim labelLeaf As String = CaptionOf(leaf.DenAlineat, If(String.IsNullOrEmpty(leaf.Clsf), leaf.Alineat, leaf.Clsf))
                    artItem.DropDownItems.Add(NewItem(labelLeaf,
                        Sub() ApplyFilterChange(Sub() _filter.SetClsf(New Integer() {id}, labelLeaf))))
                Next
                subItem.DropDownItems.Add(artItem)
            Next
            menuClsf.Items.Add(subItem)
        Next
    End Sub

    ' TipRand: distinct din rândurile încărcate (SCOPAT la angajament — ABATERE §9.1).
    ' Submeniu «REZERVĂRI» pentru Rez_ și «PLĂȚI» pentru Plata_ (case-insensitive, §9.2),
    ' restul plat la rădăcină. «TOATE» pe rădăcină; un «TOATE» per grup doar când grupul are
    ' mai mult de o intrare (Access: rezCount > 1). «TOATE»-ul de grup filtrează pe prefix.
    Private Sub BuildTipRandMenu()
        menuTipRand.Items.Clear()
        menuTipRand.Items.Add(NewItem("TOATE", Sub() ApplyFilterChange(Sub() _filter.ClearTipRand())))
        If _rows Is Nothing Then Return

        Dim distincte = _rows.Select(Function(r) If(r.TipRand, String.Empty)).
                              Where(Function(t) Not String.IsNullOrWhiteSpace(t)).
                              Distinct(StringComparer.OrdinalIgnoreCase).
                              OrderBy(Function(t) t, StringComparer.OrdinalIgnoreCase).ToList()

        Dim rez = distincte.Where(Function(t) t.StartsWith("Rez_", StringComparison.OrdinalIgnoreCase)).ToList()
        Dim plata = distincte.Where(Function(t) t.StartsWith("Plata_", StringComparison.OrdinalIgnoreCase)).ToList()
        Dim rest = distincte.Where(Function(t) Not t.StartsWith("Rez_", StringComparison.OrdinalIgnoreCase) AndAlso
                                               Not t.StartsWith("Plata_", StringComparison.OrdinalIgnoreCase)).ToList()

        AddTipRandGroup(rez, "REZERVĂRI", "Rez_")
        AddTipRandGroup(plata, "PLĂȚI", "Plata_")

        For Each v As String In rest
            Dim val As String = v
            menuTipRand.Items.Add(NewItem(val, Sub() ApplyFilterChange(Sub() _filter.SetTipRandExact(val))))
        Next
    End Sub

    Private Sub AddTipRandGroup(values As List(Of String), caption As String, prefix As String)
        If values Is Nothing OrElse values.Count = 0 Then Return
        Dim grpItem As New ToolStripMenuItem(caption)
        ' «TOATE» de grup doar când grupul are mai mult de o intrare (Access: rezCount > 1).
        If values.Count > 1 Then
            grpItem.DropDownItems.Add(NewItem("TOATE",
                Sub() ApplyFilterChange(Sub() _filter.SetTipRandPrefix(prefix, caption))))
        End If
        For Each v As String In values
            Dim val As String = v
            grpItem.DropDownItems.Add(NewItem(val, Sub() ApplyFilterChange(Sub() _filter.SetTipRandExact(val))))
        Next
        menuTipRand.Items.Add(grpItem)
    End Sub

    ' DataFX: lună/an (MM-YYYY) -> Ziua. «TOATE» pe rădăcină; un «TOATE» per lună doar când
    ' luna are mai mult de o zi (Access: dictZiPerAL).
    Private Sub BuildDataMenu()
        menuData.Items.Clear()
        menuData.Items.Add(NewItem("TOATE", Sub() ApplyFilterChange(Sub() _filter.ClearDataFx())))
        If _rows Is Nothing Then Return

        Dim cuData = _rows.Where(Function(r) r.DataFx.HasValue).ToList()
        Dim luni = cuData.GroupBy(Function(r) r.DataFx.Value.Year * 100 + r.DataFx.Value.Month).
                          OrderBy(Function(g) g.Key)

        For Each lunaGrp In luni
            Dim an As Integer = lunaGrp.Key \ 100
            Dim luna As Integer = lunaGrp.Key Mod 100
            Dim lunaLabel As String = $"{luna:00}-{an}"
            Dim lunaItem As New ToolStripMenuItem(lunaLabel)

            Dim zile = lunaGrp.Select(Function(r) r.DataFx.Value.Date).Distinct().OrderBy(Function(d) d).ToList()
            ' «TOATE» pe lună doar când luna are mai mult de o zi.
            If zile.Count > 1 Then
                Dim anCopy As Integer = an
                Dim lunaCopy As Integer = luna
                Dim labelLuna As String = $"{IstoricFilter.MonthLabel(luna)}/{an}"
                lunaItem.DropDownItems.Add(NewItem("TOATE",
                    Sub() ApplyFilterChange(Sub() _filter.SetDataFxMonth(anCopy, lunaCopy, labelLuna))))
            End If

            For Each zi As Date In zile
                Dim ziCopy As Date = zi
                Dim ziLabel As String = zi.ToString("dd.MM.yyyy", _roCulture)
                lunaItem.DropDownItems.Add(NewItem(ziLabel,
                    Sub() ApplyFilterChange(Sub() _filter.SetDataFxDay(ziCopy, ziLabel))))
            Next
            menuData.Items.Add(lunaItem)
        Next
    End Sub

    ' Un articol de meniu care rulează o acțiune la click, cu boundary UI (log + înghițire).
    Private Function NewItem(text As String, onClick As Action) As ToolStripMenuItem
        Dim item As New ToolStripMenuItem(text)
        AddHandler item.Click,
            Sub(s As Object, e As EventArgs)
                Try
                    onClick()
                Catch ex As Exception
                    GlobalErrorLog.Write("IstoricView.MenuItemClick", ex)
                End Try
            End Sub
        Return item
    End Function

    ' Aplică o schimbare de segment, apoi re-umple grila. Un singur loc pentru „modifică
    ' filtrul -> re-filtrează".
    Private Sub ApplyFilterChange(change As Action)
        change()
        FillFiltered()
    End Sub

    ' ── Deschiderea meniurilor din butoane ───────────────────────────────────
    Private Sub btnFiltruClsf_Click(sender As Object, e As EventArgs) Handles btnFiltruClsf.Click
        Try
            menuClsf.Show(btnFiltruClsf, New Point(0, btnFiltruClsf.Height))
        Catch ex As Exception
            GlobalErrorLog.Write("IstoricView.btnFiltruClsf_Click", ex)
        End Try
    End Sub

    Private Sub btnFiltruTipRand_Click(sender As Object, e As EventArgs) Handles btnFiltruTipRand.Click
        Try
            menuTipRand.Show(btnFiltruTipRand, New Point(0, btnFiltruTipRand.Height))
        Catch ex As Exception
            GlobalErrorLog.Write("IstoricView.btnFiltruTipRand_Click", ex)
        End Try
    End Sub

    Private Sub btnFiltruData_Click(sender As Object, e As EventArgs) Handles btnFiltruData.Click
        Try
            menuData.Show(btnFiltruData, New Point(0, btnFiltruData.Height))
        Catch ex As Exception
            GlobalErrorLog.Write("IstoricView.btnFiltruData_Click", ex)
        End Try
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        Try
            _filter.ClearAll()
            FillFiltered()
        Catch ex As Exception
            GlobalErrorLog.Write("IstoricView.btnReset_Click", ex)
        End Try
    End Sub

    ' ── Stare goală / conținut ───────────────────────────────────────────────
    Private Sub ClearAll()
        _requestedCod = Nothing
        _rows = Nothing
        _clasificatii = Nothing
        _filter.ClearAll()
        grid.ClearRows()
        menuClsf.Items.Clear()
        menuTipRand.Items.Clear()
        menuData.Items.Clear()
        lblFiltruActiv.Text = String.Empty
        UpdateDetail(Nothing)
    End Sub

    Private Sub ShowEmpty(message As String)
        lblEmpty.Text = message
        lblEmpty.Visible = True
        split.Visible = False
        pnlFiltre.Visible = False
    End Sub

    Private Sub ShowContent()
        lblEmpty.Visible = False
        split.Visible = True
        pnlFiltre.Visible = True
    End Sub

    ' ── Utilitare ────────────────────────────────────────────────────────────
    Private Shared Function ShortDate(value As Date?) As String
        If Not value.HasValue Then Return String.Empty
        Return value.Value.ToString("dd.MM.yyyy", _roCulture)
    End Function

    ' Captiunea unui nivel: den_* dacă e populat, altfel căderea pe cod (Subcapitol/Articol/Clsf).
    Private Shared Function CaptionOf(den As String, fallback As String) As String
        Dim d As String = If(den, String.Empty).Trim()
        If Not String.IsNullOrEmpty(d) Then Return d
        Return If(fallback, String.Empty)
    End Function

    Private Shared Function FirstNonEmpty(values As IEnumerable(Of String)) As String
        If values Is Nothing Then Return String.Empty
        For Each v As String In values
            If Not String.IsNullOrWhiteSpace(v) Then Return v
        Next
        Return String.Empty
    End Function

    ' ── Temă ─────────────────────────────────────────────────────────────────
    ''' <summary>
    ''' Reaplică culorile schemei pe bandă, butoane, panoul de detaliu, starea goală și cele
    ''' trei meniuri contextuale (care NU sunt în arborele de controale, deci ThemeManager nu le
    ''' atinge). Grila se auto-temează (KBotDataView implementează el însuși IThemedControl).
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            split.BackColor = p.SurfaceAltColor
            split.Panel1.BackColor = p.SurfaceAltColor
            split.Panel2.BackColor = p.SurfaceAltColor

            pnlFiltre.BackColor = p.SurfaceColor
            pnlDetaliu.BackColor = p.SurfaceColor
            detailTable.BackColor = p.SurfaceColor
            lblCapDescriere.ForeColor = p.TextDimColor
            lblCapDescriere.BackColor = Color.Transparent
            lblCapValori.ForeColor = p.TextDimColor
            lblCapValori.BackColor = Color.Transparent
            lblFiltruActiv.ForeColor = p.TextDimColor
            lblFiltruActiv.BackColor = Color.Transparent

            ' Doar Descriere e un TextBox; grila de valori (gridValori) se auto-temează (IThemedControl).
            txtDescriere.BackColor = p.InputBackColor
            txtDescriere.ForeColor = p.InputTextColor
            txtDescriere.BorderStyle = BorderStyle.FixedSingle

            ButtonStyles.ApplySecondary(btnFiltruClsf, scheme)
            ButtonStyles.ApplySecondary(btnFiltruTipRand, scheme)
            ButtonStyles.ApplySecondary(btnFiltruData, scheme)
            ButtonStyles.ApplySecondary(btnReset, scheme)

            ThemeMenu(menuClsf, p)
            ThemeMenu(menuTipRand, p)
            ThemeMenu(menuData, p)

            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor
        Catch ex As Exception
            ' Boundary UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("IstoricView.ApplyTheme", ex)
        End Try
    End Sub

    ' Temează un ContextMenuStrip din paletă: fundal, text, chenare, selecție.
    Private Shared Sub ThemeMenu(menu As ContextMenuStrip, p As ThemePalette)
        menu.BackColor = p.SurfaceColor
        menu.ForeColor = p.TextColor
        menu.RenderMode = ToolStripRenderMode.Professional
        menu.Renderer = New ToolStripProfessionalRenderer(New KBotMenuColorTable(p)) With {.RoundedEdges = False}
    End Sub

End Class

''' <summary>Argumentul evenimentului <see cref="IstoricView.RandSchimbat"/> — poartă ID-ul
''' rândului selectat (echivalentul textbox-ului ascuns Access <c>CL = Me!ID</c>). POCO.</summary>
Public NotInheritable Class IstoricRandEventArgs
    Inherits EventArgs
    Public ReadOnly Property Id As Integer
    Public Sub New(id As Integer)
        Me.Id = id
    End Sub
End Class

''' <summary>Tabelă de culori pentru meniurile Istoric, derivată din paleta temei active —
''' fundal de suprafață, chenar din bordură, selecție din hover. POCO de configurare.</summary>
Friend NotInheritable Class KBotMenuColorTable
    Inherits ProfessionalColorTable

    Private ReadOnly _p As ThemePalette

    Public Sub New(palette As ThemePalette)
        _p = palette
    End Sub

    Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
        Get
            Return _p.SurfaceColor
        End Get
    End Property
    Public Overrides ReadOnly Property MenuBorder As Color
        Get
            Return _p.BorderColor
        End Get
    End Property
    Public Overrides ReadOnly Property MenuItemBorder As Color
        Get
            Return _p.AccentColor
        End Get
    End Property
    Public Overrides ReadOnly Property MenuItemSelected As Color
        Get
            Return _p.ButtonHoverColor
        End Get
    End Property
    Public Overrides ReadOnly Property MenuItemSelectedGradientBegin As Color
        Get
            Return _p.ButtonHoverColor
        End Get
    End Property
    Public Overrides ReadOnly Property MenuItemSelectedGradientEnd As Color
        Get
            Return _p.ButtonHoverColor
        End Get
    End Property
    Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
        Get
            Return _p.SurfaceColor
        End Get
    End Property
    Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
        Get
            Return _p.SurfaceColor
        End Get
    End Property
    Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
        Get
            Return _p.SurfaceColor
        End Get
    End Property
End Class
