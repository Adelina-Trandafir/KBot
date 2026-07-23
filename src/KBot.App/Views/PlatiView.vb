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
''' Vederea Plăți (felia 0017) — echivalentul Access frmFX_MAIN_PLATI: un arbore pe TREI
''' niveluri la stânga (folder lună/an -> zi -> plata/IdPlataFX), o grilă continuă LISTA sus-
''' dreapta și un panou de detaliu jos-dreapta cu extrasul bancar. Read-only în această felie.
''' Datele vin din GET /api/forexe/plati, întotdeauna prin plasa de re-autentificare a
''' shell-ului (401 -> re-login -> reia o dată). Click pe orice nod (TOATE / lună / zi / plată)
''' FILTREAZĂ grila la rândurile nodului (nu agregă — spre deosebire de Recepții). Selectarea
''' unui rând din grilă umple panoul de detaliu din datele deja pe rând (fără al doilea apel).
'''
''' Nivelul 2 (plata) reînvie codul dormant Level=2 din Access (mcTree_Click /
''' RightIconClick / RefreshPlataLista aveau ramura, dar Show_Plati nu construia nodul).
''' </summary>
Public Class PlatiView
    Implements IAngajamentView, IThemedControl

    ' Cheile coloanelor grilei — o singură definiție, folosită la creare și la umplere.
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_PLATITOR As String = "platitor"
    Private Const COL_NRDOC As String = "nrdoc"
    Private Const COL_DATA As String = "data"
    Private Const COL_SUMA As String = "suma"

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

    ''' <summary>«+» apăsat pe rădăcina unei luni (nivel 0) — oglindește
    ''' RaiseEvent AdaugareOrdonantari(LunaAn). Fără abonat în această felie.</summary>
    Public Event AdaugaOrdonantariCerut(sender As Object, e As LunaAnEventArgs)

    ''' <summary>«+» apăsat pe o zi (nivel 1, IdPlataFx = -1) sau pe o plată (nivel 2, IdPlataFx
    ''' real) — oglindește RaiseEvent AdaugareOrdonantare(IdPlataFX, DataPlata). Fără abonat.</summary>
    Public Event AdaugaOrdonantareCerut(sender As Object, e As PlataOrdEventArgs)

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of PlatiInfo)), Task(Of PlatiInfo)))
        If apiClient Is Nothing Then Throw New ArgumentNullException(NameOf(apiClient))
        If withReauth Is Nothing Then Throw New ArgumentNullException(NameOf(withReauth))
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        ConfigureTree()
        BuildColumns()
        ShowEmpty("Selectați un angajament din arbore.")
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "plati"
        End Get
    End Property

    ' Font + comportament neacoperite de Designer (setter-ele reconstruiesc fontul intern).
    Private Sub ConfigureTree()
        Try
            tree.FontName = "Segoe UI"
            tree.FontSize = 9.0F
            tree.RootExpander = True
        Catch ex As Exception
            GlobalErrorLog.Write("PlatiView.ConfigureTree", ex)
            Throw
        End Try
    End Sub

    ' Coloanele grilei = frmFX_MAIN_PLATI_LISTA: Clasificație, Plătitor, Nr. doc, Data plății,
    ' Suma. Rând de totaluri (Designer): Count pe clasificație, Sum pe Suma. Suma e Double (ca
    ' agregatul să însumeze), formatată N2 la dreapta; restul sunt text.
    Private Sub BuildColumns()
        Try
            Dim colClsf As KBotDataColumn = grid.AddColumn(COL_CLSF, "Clasificație", KBotColumnType.Text, 160)
            colClsf.Aggregate = KBotAggregate.Count
            grid.AddColumn(COL_PLATITOR, "Plătitor", KBotColumnType.Text, 200)
            grid.AddColumn(COL_NRDOC, "Nr. doc", KBotColumnType.Text, 90)
            grid.AddColumn(COL_DATA, "Data plății", KBotColumnType.Text, 100)
            Dim colSuma As KBotDataColumn = grid.AddColumn(COL_SUMA, "Suma", KBotColumnType.Text, 120)
            colSuma.FormatString = "N2"
            colSuma.TextAlign = ContentAlignment.MiddleRight
            colSuma.Aggregate = KBotAggregate.Sum
        Catch ex As Exception
            GlobalErrorLog.Write("PlatiView.BuildColumns", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Selecția din arbore s-a schimbat. Fără angajament (nod de capitol / deselectare) NU se
    ''' face niciun apel de rețea — doar se golește vederea.
    ''' </summary>
    Public Sub SetContext(info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        Try
            Dim cod As String = If(info Is Nothing, Nothing, info.CodAngajament)
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
    ' TREI niveluri: rădăcina specială « TOATE PLĂȚILE » (SUM peste tot) + un folder per lună
    ' (SUM lună) -> o frunză per zi (TOATE rândurile zilei, SUM zi) -> un nod per plată (Suma).
    ' Fiecare nod poartă în Tag rândurile lui, ca un click să FILTREZE grila fără o nouă cerere.
    '
    ' Iconițe (finding operator, notat în worklog — Access nu dă sursă pentru lună/zi merjate):
    '   * nivel 2 (plată): per rând — Incarcat->sus, altfel Preluat->jos, altfel neutru;
    '   * lună + zi (merjate): ORICE rând Incarcat->sus; altfel ORICE Preluat->jos; altfel neutru.
    ' Colorare INCASARE (verde din paletă): nivel 2 per rând; lună/zi verde doar dacă TOATE
    ' rândurile sunt INCASARE (asumat, notat în worklog).
    Private Sub BuildTree(rows As List(Of PlataRow))
        Try
            tree.Clear()
            Dim palette As ThemePalette = TryGetPalette()

            ' « TOATE PLĂȚILE » — rădăcină specială, prima, colapsată; SUM(Suma) peste tot.
            Dim allSum As Double = rows.Sum(Function(r) r.Suma)
            Dim toateIcon As Image = If(palette Is Nothing, Nothing,
                                        PlatiIcons.ToateIcon(palette.TextColor, tree.LeftIconSize.Width))
            Dim allItem As AdvancedTreeControl.TreeItem =
                tree.AddItem("ALL", $"« TOATE PLĂȚILE »~~~{Money(allSum)}",
                             pLeftIconClosed:=toateIcon, pLeftIconOpen:=toateIcon, pExpanded:=False)
            allItem.Tag = rows

            ' Cea mai veche zi cu cel puțin o plată ne-ordonantată -> «+» pe ea (o singură zi).
            Dim plusDay As Date? = OldestUnordonantatDay(rows)

            ' Foldere de lună, cronologic.
            Dim monthGroups = rows.GroupBy(Function(r) MonthKeyOf(r.DataPlata)).
                                   OrderBy(Function(g) g.Key)

            For Each mg In monthGroups
                Dim monthRows As List(Of PlataRow) = mg.ToList()
                Dim monthSum As Double = monthRows.Sum(Function(r) r.Suma)
                Dim monthContainsPlus As Boolean =
                    plusDay.HasValue AndAlso monthRows.Any(Function(r) SameDay(r.DataPlata, plusDay))
                Dim monthIcon As Image = IconFor(MergedStare(monthRows), palette)
                Dim monthPlus As Image = If(monthContainsPlus, PlusIcon(palette), Nothing)

                Dim monthItem As AdvancedTreeControl.TreeItem =
                    tree.AddItem($"m_{mg.Key}", $"{MonthYearLabel(mg.Key)}~~~{Money(monthSum)}",
                                 pLeftIconClosed:=monthIcon, pLeftIconOpen:=monthIcon,
                                 pRightIcon:=monthPlus, pExpanded:=False)
                monthItem.Tag = monthRows
                If AllIncasare(monthRows) AndAlso palette IsNot Nothing Then
                    monthItem.NodeForeColor = palette.SuccessColor
                End If

                ' Frunze de zi (TOATE rândurile zilei într-un nod), cronologic.
                Dim dayGroups = monthRows.GroupBy(Function(r) DayKeyOf(r.DataPlata)).
                                          OrderBy(Function(g) g.Key)
                For Each dg In dayGroups
                    Dim dayRows As List(Of PlataRow) = dg.ToList()
                    Dim daySum As Double = dayRows.Sum(Function(r) r.Suma)
                    Dim dayIsPlus As Boolean = plusDay.HasValue AndAlso dg.Key = plusDay.Value.Date
                    Dim dayIcon As Image = IconFor(MergedStare(dayRows), palette)
                    Dim dayPlus As Image = If(dayIsPlus, PlusIcon(palette), Nothing)

                    Dim dayItem As AdvancedTreeControl.TreeItem =
                        tree.AddItem($"d_{dg.Key:yyyyMMdd}", $"{ShortDate(dg.Key)}~~~{Money(daySum)}",
                                     monthItem, pLeftIconClosed:=dayIcon, pLeftIconOpen:=dayIcon,
                                     pRightIcon:=dayPlus)
                    dayItem.Tag = dayRows
                    If AllIncasare(dayRows) AndAlso palette IsNot Nothing Then
                        dayItem.NodeForeColor = palette.SuccessColor
                    End If

                    ' Noduri de plată (nivel 2), în ordinea serverului (Data_plata, IdPlataFX).
                    For Each r As PlataRow In dayRows
                        Dim payIcon As Image = IconFor(StareOf(r), palette)
                        Dim payIsPlus As Boolean = dayIsPlus AndAlso Not r.AreOrd
                        Dim payPlus As Image = If(payIsPlus, PlusIcon(palette), Nothing)

                        Dim payItem As AdvancedTreeControl.TreeItem =
                            tree.AddItem($"p_{r.IdPlataFX}", $"{r.EtichetaPlata}~~~{Money(r.Suma)}",
                                         dayItem, pLeftIconClosed:=payIcon, pLeftIconOpen:=payIcon,
                                         pRightIcon:=payPlus)
                        payItem.Tag = New List(Of PlataRow) From {r}
                        If r.EsteIncasare AndAlso palette IsNot Nothing Then
                            payItem.NodeForeColor = palette.SuccessColor
                        End If
                    Next
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
    Private Sub tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing Then Return
            Dim rows As List(Of PlataRow) = TryCast(pNode.Tag, List(Of PlataRow))
            If rows Is Nothing Then Return
            FillGrid(rows)
        Catch ex As Exception
            GlobalErrorLog.Write("PlatiView.tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' Click pe iconița «+» -> ridicăm evenimentele dormante (mcTree_RightIconClick), fără abonat
    ' în această felie. Nivel 0 (lună) -> AdaugaOrdonantariCerut(LunaAn); nivel 1 (zi) ->
    ' AdaugaOrdonantareCerut(-1, data); nivel 2 (plată) -> AdaugaOrdonantareCerut(IdPlataFX, data).
    Private Sub tree_RightIconClicked(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            If pNode Is Nothing Then Return
            Dim rows As List(Of PlataRow) = TryCast(pNode.Tag, List(Of PlataRow))
            If rows Is Nothing OrElse rows.Count = 0 Then Return

            Select Case pNode.Level
                Case 0
                    If String.Equals(pNode.Key, "ALL", StringComparison.Ordinal) Then Return
                    RaiseEvent AdaugaOrdonantariCerut(Me, New LunaAnEventArgs(LunaAnOf(rows(0))))
                Case 1
                    RaiseEvent AdaugaOrdonantareCerut(Me, New PlataOrdEventArgs(-1, DayOf(rows(0))))
                Case 2
                    RaiseEvent AdaugaOrdonantareCerut(Me, New PlataOrdEventArgs(rows(0).IdPlataFX, DayOf(rows(0))))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("PlatiView.tree_RightIconClicked", ex)
        End Try
    End Sub

    ' ── Panoul de detaliu (extrasul bancar) ──────────────────────────────────
    ' Condus de selecția din grilă, din datele deja pe rând. Fără al doilea apel de rețea.
    Private Sub grid_SelectionChanged(sender As Object, e As EventArgs) Handles grid.SelectionChanged
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

    ' Eticheta lună/an dintr-o cheie de lună: „Ianuarie/2026". Cheia 0 -> „(fără dată)".
    Private Shared Function MonthYearLabel(monthKey As Integer) As String
        If monthKey <= 0 Then Return "(fără dată)"
        Dim y As Integer = monthKey \ 100
        Dim m As Integer = monthKey Mod 100
        Return $"{MonthLabel(m)}/{y}"
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

    Private Shared Function DayOf(r As PlataRow) As Date
        Return If(r.DataPlata.HasValue, r.DataPlata.Value.Date, Date.MinValue)
    End Function

    Private Shared Function SameDay(a As Date?, b As Date?) As Boolean
        Return a.HasValue AndAlso b.HasValue AndAlso a.Value.Date = b.Value.Date
    End Function

    ' Starea vizuală a unei plăți: Incarcat->sus, altfel Preluat->jos, altfel neutru.
    Private Shared Function StareOf(r As PlataRow) As PlatiIcons.Stare
        If r.Incarcat Then Return PlatiIcons.Stare.Sus
        If r.Preluat Then Return PlatiIcons.Stare.Jos
        Return PlatiIcons.Stare.Neutru
    End Function

    ' Starea merjată a unui grup (lună/zi): ORICE sus->sus, altfel ORICE jos->jos, altfel neutru.
    Private Shared Function MergedStare(rows As List(Of PlataRow)) As PlatiIcons.Stare
        If rows.Any(Function(r) r.Incarcat) Then Return PlatiIcons.Stare.Sus
        If rows.Any(Function(r) r.Preluat) Then Return PlatiIcons.Stare.Jos
        Return PlatiIcons.Stare.Neutru
    End Function

    Private Shared Function AllIncasare(rows As List(Of PlataRow)) As Boolean
        Return rows.Count > 0 AndAlso rows.All(Function(r) r.EsteIncasare)
    End Function

    ' Iconița stării, cu culoarea din paletă după stare (sus=succes, jos=accent, neutru=estompat).
    Private Function IconFor(stare As PlatiIcons.Stare, palette As ThemePalette) As Image
        If palette Is Nothing Then Return Nothing
        Dim color As Color
        Select Case stare
            Case PlatiIcons.Stare.Sus : color = palette.SuccessColor
            Case PlatiIcons.Stare.Jos : color = palette.AccentColor
            Case Else : color = palette.TextDimColor
        End Select
        Return PlatiIcons.StatusIcon(stare, color, tree.LeftIconSize.Width)
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
        Return If(current Is Nothing, Nothing, current.Palette)
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

            tree.BackColor = p.SurfaceAltColor
            tree.ForeColor = p.TextColor
            tree.HoverBackColor = p.ButtonHoverColor
            tree.SelectedBackColor = p.ButtonPressedColor
            tree.SelectedBorderColor = p.AccentColor
            tree.LineColor = p.BorderColor
            tree.HeaderBackColor = p.SurfaceAltColor
            tree.HeaderForeColor = p.TextColor

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
