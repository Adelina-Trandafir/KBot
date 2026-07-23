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

    ''' <summary>«+» a fost apăsat pe o frunză (grup fără DDF). Felia curentă doar
    ''' semnalează; workflow-ul IncarcaRezervare/DDF este o felie ulterioară.</summary>
    Public Event AdaugaDdfCerut(tip As RezervareTip, data As Date)

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of RezervariInfo)), Task(Of RezervariInfo)))
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
            Return "rezervari"
        End Get
    End Property

    ' Font + comportament neacoperite de Designer (setter-ele reconstruiesc fontul intern).
    Private Sub ConfigureTree()
        Try
            tree.FontName = "Segoe UI"
            tree.FontSize = 9.0F
            tree.RootExpander = True
        Catch ex As Exception
            GlobalErrorLog.Write("RezervariView.ConfigureTree", ex)
            Throw
        End Try
    End Sub

    ' Coloanele grilei = frmFX_MAIN_REZ_LISTA: Clsf + patru coloane de bani. Toate sunt
    ' Text cu N2 aliniat la dreapta (read-only, deci un tip numeric ar fi degeaba).
    Private Sub BuildColumns()
        Try
            grid.AddColumn(COL_CLSF, "Clasificație", KBotColumnType.Text, 190)
            AddMoneyColumn(COL_CREDIT_BUG, "Credit bugetar")
            AddMoneyColumn(COL_INITIALA, "Rezervări inițiale")
            AddMoneyColumn(COL_VALOARE, "Rezervare curentă")
            AddMoneyColumn(COL_DEFINITIVA, "Rezervări definitive")
            ' Clasificația e cea după care se citește tabelul — rămâne fixă la stânga.
            grid.FrozenColumnCount = 1
        Catch ex As Exception
            GlobalErrorLog.Write("RezervariView.BuildColumns", ex)
            Throw
        End Try
    End Sub

    Private Sub AddMoneyColumn(key As String, header As String)
        Dim col As KBotDataColumn = grid.AddColumn(key, header, KBotColumnType.Text, 130)
        col.FormatString = "N2"
        col.TextAlign = ContentAlignment.MiddleRight
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
            ' „Nimic selectat" -> grila arată TOATE rândurile angajamentului (decizia §7.3).
            FillGrid(rows)
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
                Dim monthCaption As String = $"{MonthLabel(m)}/{y}~~~{Money(total)}"

                Dim root As AdvancedTreeControl.TreeItem =
                    tree.AddItem(monthKey, monthCaption, pExpanded:=True)
                root.Tag = monthRows

                ' Frunze pe (dată, tip), ordonate pe dată apoi pe rangul tipului
                ' (Inițială < Mărire < Micșorare, ca strData din Access).
                Dim leaves = monthRows.GroupBy(Function(r) New With {Key .D = r.DataRezervare.Date, Key .T = r.Tip}).
                                       OrderBy(Function(gp) gp.Key.D).ThenBy(Function(gp) CInt(gp.Key.T))

                For Each leafGroup In leaves
                    Dim leafRows As List(Of RezervareRow) = leafGroup.ToList()
                    Dim d As Date = leafGroup.Key.D
                    Dim tip As RezervareTip = leafGroup.Key.T
                    Dim leafValue As Double = leafRows.Sum(Function(r) r.ValoareOperatie)
                    Dim hasPlus As Boolean = leafRows.Any(Function(r) Not r.AreDDF)

                    Dim leafKey As String = $"RZ_{d:yyyyMMdd}_{CInt(tip)}"
                    Dim leafCaption As String = $"{d:dd.MM.yyyy}~~~{Money(leafValue)}"

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

    ' Click pe un nod -> filtrează grila la rândurile nodului (lună sau frunză). Rândurile
    ' stau în Tag, puse la construcția arborelui — niciun apel de rețea aici.
    Private Sub tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing Then Return
            Dim rows As List(Of RezervareRow) = TryCast(pNode.Tag, List(Of RezervareRow))
            If rows Is Nothing Then Return
            FillGrid(rows)
        Catch ex As Exception
            GlobalErrorLog.Write("RezervariView.tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' Click pe iconița «+» a unei frunze -> semnalăm doar (workflow DDF = felie ulterioară).
    Private Sub tree_RightIconClicked(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            If pNode Is Nothing Then Return
            Dim rows As List(Of RezervareRow) = TryCast(pNode.Tag, List(Of RezervareRow))
            If rows Is Nothing OrElse rows.Count = 0 Then Return
            Dim first As RezervareRow = rows(0)
            RaiseEvent AdaugaDdfCerut(first.Tip, first.DataRezervare)
        Catch ex As Exception
            GlobalErrorLog.Write("RezervariView.tree_RightIconClicked", ex)
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

    Private Function TipIconOf(tip As RezervareTip, palette As ThemePalette) As Image
        If palette Is Nothing Then Return Nothing
        Dim color As Color
        Select Case tip
            Case RezervareTip.Marire : color = palette.SuccessColor
            Case RezervareTip.Micsorare : color = palette.ErrorColor
            Case Else : color = palette.TextColor       ' Inițială («=»)
        End Select
        Return RezervariIcons.TipIcon(tip, color, tree.LeftIconSize.Width)
    End Function

    Private Function PlusIconOf(tip As RezervareTip, palette As ThemePalette) As Image
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
        Return If(current Is Nothing, Nothing, current.Palette)
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

            ' Re-tintarea iconițelor de tip/«+» pe noua paletă.
            If _rows IsNot Nothing AndAlso _rows.Count > 0 Then
                BuildTree(_rows)
                FillGrid(_rows)
            End If
        Catch ex As Exception
            ' Boundary UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("RezervariView.ApplyTheme", ex)
        End Try
    End Sub

End Class
