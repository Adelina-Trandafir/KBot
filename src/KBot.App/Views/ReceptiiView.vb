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
''' Vederea Recepții (felia 0015) — echivalentul Access frmFX_MAIN_REC: un master/detail
''' cu un arbore de recepții pe 2 niveluri la stânga (rădăcină = recepția R / IDRR, nod =
''' antetul H / IDRH) și o grilă continuă la dreapta (LISTA) cu detaliul pe clasificații al
''' antetului selectat. Read-only în această felie. Datele vin din GET /api/forexe/receptii,
''' întotdeauna prin plasa de re-autentificare a shell-ului (401 -> re-login -> reia o dată).
''' Selectarea unei rădăcini golește grila (Access conduce LISTA doar la nivel de antet);
''' selectarea unui antet o umple cu un rând-total sintetic + un rând per clasificație.
''' </summary>
Public Class ReceptiiView
    Implements IAngajamentView, IThemedControl

    ' Cheile coloanelor grilei — o singură definiție, folosită la creare și la umplere,
    ' ca un typo să nu ajungă o coloană goală în producție.
    Private Const COL_NRCRT As String = "nrcrt"
    Private Const COL_DESCRIERE As String = "descriere"
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_VALOARE As String = "valoare"

    ' Format românesc: separator de mii «.» și zecimală «,» (1.091.940,00).
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    ' Plasa 401 a shell-ului (MainForm.WithReauth), specializată pe ReceptiiInfo:
    ' politica de re-login rămâne într-un singur loc, vederea doar o folosește.
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of ReceptiiInfo)), Task(Of ReceptiiInfo))

    ' Codul angajamentului CERUT ultima dată — vezi stale-guard din LoadAsync (identic cu
    ' Sumar/Rezervări): operatorul parcurge arborele rapid, iar un răspuns depășit se aruncă.
    Private _requestedCod As String

    ' Ultimele date încărcate — păstrate ca ApplyTheme să reconstruiască arborele
    ' (re-tintarea iconițelor) fără o nouă cerere de rețea. `_plati` alimentează
    ' tooltip-ul de recepție (felia 0015-02).
    Private _rows As List(Of ReceptieRow)
    Private _plati As List(Of ReceptiePlata)

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of ReceptiiInfo)), Task(Of ReceptiiInfo)))
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
            Return "receptii"
        End Get
    End Property

    ' Font + comportament neacoperite de Designer (setter-ele reconstruiesc fontul intern).
    Private Sub ConfigureTree()
        Try
            tree.FontName = "Segoe UI"
            tree.FontSize = 9.0F
            tree.RootExpander = True
        Catch ex As Exception
            GlobalErrorLog.Write("ReceptiiView.ConfigureTree", ex)
            Throw
        End Try
    End Sub

    ' Coloanele grilei = frmFX_MAIN_REC_LISTA (qFX_MAIN_REC_LISTA_IND): NrCrt, Descriere,
    ' Clsf, Valoare. NrCrt + Valoare aliniate la dreapta (read-only, deci un tip numeric ar
    ' fi degeaba — Text cu format aliniat e suficient).
    Private Sub BuildColumns()
        Try
            Dim colNr As KBotDataColumn = grid.AddColumn(COL_NRCRT, "NrCrt", KBotColumnType.Text, 60)
            colNr.TextAlign = ContentAlignment.MiddleRight
            grid.AddColumn(COL_DESCRIERE, "Descriere", KBotColumnType.Text, 220)
            grid.AddColumn(COL_CLSF, "Clasificație", KBotColumnType.Text, 180)
            Dim colVal As KBotDataColumn = grid.AddColumn(COL_VALOARE, "Valoare", KBotColumnType.Text, 130)
            colVal.FormatString = "N2"
            colVal.TextAlign = ContentAlignment.MiddleRight
        Catch ex As Exception
            GlobalErrorLog.Write("ReceptiiView.BuildColumns", ex)
            Throw
        End Try
    End Sub

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
            ' Nimic selectat -> grila e goală: Access conduce LISTA doar la nivel de antet.
            grid.ClearRows()
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
    ' Două niveluri (qFX_MAIN_REC_TREE): rădăcină = recepția (IDRR), nod = antetul (IDRH).
    ' Rândurile vin deja ordonate de server (R.NRCRT, R.DataR, H.NrCrt, H.DataH), deci
    ' „distinct în ordine" e suficient. Iconița rădăcinii reflectă starea (Incarcat -> sus,
    ' Preluat -> jos, altfel neutru). Fiecare nod poartă în Tag rândurile antetului lui, ca
    ' un click să umple grila fără o nouă cerere; fiecare rădăcină poartă rândurile
    ' recepției (baza tooltip-ului din felia 0015-02).
    Private Sub BuildTree(rows As List(Of ReceptieRow))
        Try
            tree.Clear()
            Dim palette As ThemePalette = TryGetPalette()

            Dim roots As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
            Dim rootRows As New Dictionary(Of Integer, List(Of ReceptieRow))()
            Dim nodes As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
            Dim nodeRows As New Dictionary(Of Integer, List(Of ReceptieRow))()

            For Each r As ReceptieRow In rows
                ' --- rădăcina (recepția) ---
                Dim root As AdvancedTreeControl.TreeItem = Nothing
                If Not roots.TryGetValue(r.Idrr, root) Then
                    Dim icon As Image = StatusIconOf(r, palette)
                    Dim caption As String = $"{ShortDate(r.DataR)}~~~{Money(r.SumaAntet)}"
                    root = tree.AddItem($"r_{r.Idrr}", caption,
                                        pLeftIconClosed:=icon, pLeftIconOpen:=icon)
                    Dim rr As New List(Of ReceptieRow)()
                    root.Tag = rr
                    roots(r.Idrr) = root
                    rootRows(r.Idrr) = rr
                End If
                rootRows(r.Idrr).Add(r)

                ' --- nodul (antetul) ---
                Dim node As AdvancedTreeControl.TreeItem = Nothing
                If Not nodes.TryGetValue(r.Idrh, node) Then
                    Dim caption As String = $"{ShortDate(r.DataH)}~~~{Money(r.Total)}"
                    node = tree.AddItem($"h_{r.Idrh}", caption, root)
                    Dim nr As New List(Of ReceptieRow)()
                    node.Tag = nr
                    nodes(r.Idrh) = node
                    nodeRows(r.Idrh) = nr
                End If
                nodeRows(r.Idrh).Add(r)
            Next

            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("ReceptiiView.BuildTree", ex)
            Throw
        End Try
    End Sub

    ' ── Grila (LISTA) ────────────────────────────────────────────────────────
    ' Detaliul unui antet: un rând-total sintetic „Toți indicatorii" (Valoare = Sum(DIF)
    ' pe antet), apoi un rând per clasificație (Valoare = Sum(Valoare) grupat pe Clsf,
    ' NrCrt din indicator, Descrierea = a antetului). Ordinea grupurilor: DataH apoi NrCrt
    ' (DataH e constant pe un antet, deci efectiv NrCrt) — ca în ORDER BY-ul Access.
    Private Sub FillGridForAntet(antetRows As List(Of ReceptieRow))
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            If antetRows Is Nothing OrElse antetRows.Count = 0 Then Return

            ' Randul-total: Sum(DIF) peste toate liniile antetului.
            Dim totalDif As Double = antetRows.Sum(Function(r) r.Dif)
            Dim descriereAntet As String = antetRows(0).DescriereH
            Dim rowTot As KBotDataRow = grid.AddRow()
            rowTot(COL_NRCRT) = String.Empty
            rowTot(COL_DESCRIERE) = "Toți indicatorii"
            rowTot(COL_CLSF) = String.Empty
            rowTot(COL_VALOARE) = totalDif

            ' Rânduri per clasificație — doar liniile reale (un antet fără linii dă doar total).
            Dim lines = antetRows.Where(Function(r) r.Idr.HasValue)
            Dim groups = lines.GroupBy(Function(r) r.Clsf).
                               OrderBy(Function(gp) MinNrCrt(gp)).
                               ThenBy(Function(gp) gp.Key, StringComparer.Ordinal)

            For Each grp In groups
                Dim r As KBotDataRow = grid.AddRow()
                Dim nrCrt As Integer? = grp.Select(Function(x) x.NrCrtInd).
                                            FirstOrDefault(Function(v) v.HasValue)
                r(COL_NRCRT) = If(nrCrt.HasValue, CObj(nrCrt.Value), CObj(String.Empty))
                r(COL_DESCRIERE) = descriereAntet
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

    ' Click pe un nod. Rădăcină (recepția, Level 0) -> golește grila (Access conduce LISTA
    ' doar la antet). Nod (antetul, Level 1) -> umple grila din rândurile lui (în Tag).
    Private Sub tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing Then Return
            If pNode.Level = 0 Then
                grid.ClearRows()
                Return
            End If
            Dim rows As List(Of ReceptieRow) = TryCast(pNode.Tag, List(Of ReceptieRow))
            If rows Is Nothing Then Return
            FillGridForAntet(rows)
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

    ' Iconița stării recepției (finding 5): Incarcat -> sus (verde), altfel Preluat -> jos
    ' (accent), altfel neutru (estompat).
    Private Function StatusIconOf(row As ReceptieRow, palette As ThemePalette) As Image
        If palette Is Nothing Then Return Nothing
        Dim stare As ReceptiiIcons.Stare
        Dim color As Color
        If row.Incarcat Then
            stare = ReceptiiIcons.Stare.Sus
            color = palette.SuccessColor
        ElseIf row.Preluat Then
            stare = ReceptiiIcons.Stare.Jos
            color = palette.AccentColor
        Else
            stare = ReceptiiIcons.Stare.Neutru
            color = palette.TextDimColor
        End If
        Return ReceptiiIcons.StatusIcon(stare, color, tree.LeftIconSize.Width)
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

            tree.BackColor = p.SurfaceAltColor
            tree.ForeColor = p.TextColor
            tree.HoverBackColor = p.ButtonHoverColor
            tree.SelectedBackColor = p.ButtonPressedColor
            tree.SelectedBorderColor = p.AccentColor
            tree.LineColor = p.BorderColor
            tree.HeaderBackColor = p.SurfaceAltColor
            tree.HeaderForeColor = p.TextColor

            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor

            ' Re-tintarea iconițelor de stare pe noua paletă (grila rămâne golită — LISTA
            ' se reface la următorul click pe un antet).
            If _rows IsNot Nothing AndAlso _rows.Count > 0 Then
                BuildTree(_rows)
            End If
        Catch ex As Exception
            ' Boundary UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("ReceptiiView.ApplyTheme", ex)
        End Try
    End Sub

End Class
