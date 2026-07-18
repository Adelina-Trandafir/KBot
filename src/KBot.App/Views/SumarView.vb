Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Vederea Sumar (felia 0011) — echivalentul Access frmFX_MAIN_Sumar: un bloc de
''' antet cu datele angajamentului plus o grilă continuă cu un rând per INDICATOR.
''' Read-only prin definiție (vedere de consultare); grila e un
''' <see cref="KBotDataView"/> cu <c>ReadOnlyGrid = True</c>.
''' Datele vin din GET /api/forexe/sumar, întotdeauna prin plasa de re-autentificare
''' a shell-ului (401 -> re-login -> reia o dată).
''' </summary>
Public Class SumarView
    Implements IAngajamentView, IThemedControl

    ' Cheile coloanelor — o singură definiție, folosită și la creare și la umplere,
    ' ca un typo să nu ajungă o coloană goală în producție.
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_INDICATOR As String = "cod_indicator"
    Private Const COL_REZERVARI As String = "total_rezervari"
    Private Const COL_RECEPTII As String = "total_receptii"
    Private Const COL_PLATI As String = "total_plati"
    Private Const COL_REVIZII As String = "total_revizii"
    Private Const COL_ORDONANTARI As String = "total_ordonantari"
    Private Const COL_PARTENER As String = "partener"

    Private ReadOnly _apiClient As IApiClient
    ' Plasa 401 a shell-ului (MainForm.WithReauth), specializată pe SumarInfo:
    ' politica de re-login rămâne într-un singur loc, vederea doar o folosește.
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of SumarInfo)), Task(Of SumarInfo))

    ' Codul angajamentului CERUT ultima dată. Operatorul poate parcurge arborele
    ' rapid, iar răspunsurile pot veni în altă ordine decât cererile: un răspuns a
    ' cărui cheie nu mai e cea curentă se ARUNCĂ, altfel grila ar arăta datele unui
    ' angajament sub antetul altuia.
    Private _requestedCod As String

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of SumarInfo)), Task(Of SumarInfo)))
        If apiClient Is Nothing Then Throw New ArgumentNullException(NameOf(apiClient))
        If withReauth Is Nothing Then Throw New ArgumentNullException(NameOf(withReauth))
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        BuildColumns()
        ShowEmpty("Selectați un angajament din arbore.")
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "sumar"
        End Get
    End Property

    ' Coloanele grilei. Cele cinci coloane de bani sunt Text cu FormatString="N2" și
    ' aliniere la dreapta — nu se editează nimic, deci nu e nevoie de un tip numeric.
    Private Sub BuildColumns()
        Try
            grid.AddColumn(COL_CLSF, "Clasificație", KBotColumnType.Text, 190)
            grid.AddColumn(COL_INDICATOR, "Indicator", KBotColumnType.Text, 120)
            AddMoneyColumn(COL_REZERVARI, "Rezervări")
            AddMoneyColumn(COL_RECEPTII, "Recepții")
            AddMoneyColumn(COL_PLATI, "Plăți")
            AddMoneyColumn(COL_REVIZII, "Revizii")
            AddMoneyColumn(COL_ORDONANTARI, "Ordonanțări")
            grid.AddColumn(COL_PARTENER, "Partener", KBotColumnType.Text, 220)
            ' Clasificația e cea mai lată și cea după care se citește tabelul —
            ' rămâne fixă la stânga când operatorul derulează spre coloanele de bani.
            grid.FrozenColumnCount = 1
        Catch ex As Exception
            GlobalErrorLog.Write("SumarView.BuildColumns", ex)
            Throw
        End Try
    End Sub

    Private Sub AddMoneyColumn(key As String, header As String)
        Dim col As KBotDataColumn = grid.AddColumn(key, header, KBotColumnType.Text, 110)
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
                ' Invalidează orice răspuns aflat în zbor (vezi _requestedCod).
                _requestedCod = Nothing
                ClearHeader()
                grid.ClearRows()
                ShowEmpty("Selectați un angajament din arbore.")
                Return
            End If

            _requestedCod = cod
            ShowEmpty("Se încarcă sumarul…")
            ' Fire-and-forget deliberat: SetContext e apelat dintr-un handler sincron
            ' al shell-ului, iar încărcarea nu are voie să blocheze firul UI. Metoda
            ' își tratează singură TOATE erorile (nu iese nicio excepție neobservată).
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("SumarView.SetContext", ex)
            Throw
        End Try
    End Sub

    ' Încărcarea propriu-zisă. Boundary UI: logăm și ARĂTĂM eroarea, nu o aruncăm mai
    ' departe — nu există cine să o prindă (apelul e pornit fără await din SetContext).
    Private Async Sub LoadAsync(cod As String)
        Try
            Dim data As SumarInfo = Await _withReauth(
                Function() _apiClient.GetSumarAsync(cod, CancellationToken.None)).ConfigureAwait(True)

            ' Răspuns depășit: între timp operatorul a selectat alt angajament (sau a
            ' deselectat). Îl aruncăm — altfel ar suprascrie o selecție mai nouă.
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return

            If data Is Nothing OrElse data.Header Is Nothing Then
                ClearHeader()
                grid.ClearRows()
                ShowEmpty("Angajamentul nu are indicatori.")
                Return
            End If

            FillHeader(data.Header)
            FillRows(data.Rows)
        Catch ex As ApiException
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("SumarView.LoadAsync", ex)
            ClearHeader()
            grid.ClearRows()
            ' Mesajul din câmpul «error» al serverului, deja în română — niciodată JSON brut.
            ShowEmpty(ex.Message)
        Catch ex As Exception
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("SumarView.LoadAsync", ex)
            ClearHeader()
            grid.ClearRows()
            ShowEmpty("Sumarul nu a putut fi încărcat. Detalii în jurnalul de erori.")
        End Try
    End Sub

    Private Sub FillHeader(header As SumarHeader)
        lblCod.Text = header.CodAngajament
        lblDataFx.Text = FormatDate(header.DataFX)
        lblDataCreare.Text = FormatDate(header.DataCreare)
        lblDataDef.Text = FormatDate(header.DataDefinitivare)
        lblStare.Text = header.Stare
        lblDescriere.Text = header.Descriere
        lblStatus.Text = $"{YesNo(header.Incarcat)} / {YesNo(header.Preluat)}"
    End Sub

    Private Sub ClearHeader()
        lblCod.Text = String.Empty
        lblDataFx.Text = String.Empty
        lblDataCreare.Text = String.Empty
        lblDataDef.Text = String.Empty
        lblStare.Text = String.Empty
        lblDescriere.Text = String.Empty
        lblStatus.Text = String.Empty
    End Sub

    ' BeginUpdate/EndUpdate: o singură repictare la final, nu una per rând.
    Private Sub FillRows(rows As List(Of SumarRow))
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            If rows IsNot Nothing Then
                For Each r As SumarRow In rows
                    Dim row As KBotDataRow = grid.AddRow()
                    row(COL_CLSF) = r.Clsf
                    row(COL_INDICATOR) = r.CodIndicator
                    row(COL_REZERVARI) = r.TotalRezervari
                    row(COL_RECEPTII) = r.TotalReceptii
                    row(COL_PLATI) = r.TotalPlati
                    row(COL_REVIZII) = r.TotalRevizii
                    row(COL_ORDONANTARI) = r.TotalOrdonantari
                    row(COL_PARTENER) = r.Partener
                Next
            End If
        Finally
            grid.EndUpdate()
        End Try

        If grid.RowCount = 0 Then
            ShowEmpty("Angajamentul nu are indicatori.")
        Else
            ShowGrid()
        End If
    End Sub

    ' Starea goală acoperă grila; antetul rămâne vizibil (poate purta deja date).
    Private Sub ShowEmpty(message As String)
        lblEmpty.Text = message
        lblEmpty.Visible = True
        grid.Visible = False
    End Sub

    Private Sub ShowGrid()
        lblEmpty.Visible = False
        grid.Visible = True
    End Sub

    Private Shared Function FormatDate(value As Date?) As String
        Return If(value.HasValue, value.Value.ToString("dd.MM.yyyy"), String.Empty)
    End Function

    Private Shared Function YesNo(value As Boolean) As String
        Return If(value, "Da", "Nu")
    End Function

    ''' <summary>
    ''' Reaplică culorile schemei. Grila NU se atinge aici: KBotDataView implementează
    ''' el însuși IThemedControl, iar ThemeManager.Traverse ajunge la el.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            BackColor = scheme.Palette.SurfaceAltColor

            pnlHeader.BackColor = scheme.Palette.SurfaceAltColor
            tblHeader.BackColor = scheme.Palette.SurfaceAltColor

            ' Etichetele-titlu sunt estompate; valorile stau în culoarea normală a textului.
            For Each caption As Label In New Label() {lblCodCaption, lblDataFxCaption,
                                                      lblDataCreareCaption, lblDataDefCaption,
                                                      lblStareCaption, lblStatusCaption,
                                                      lblDescriereCaption}
                caption.ForeColor = scheme.Palette.TextDimColor
                caption.BackColor = scheme.Palette.SurfaceAltColor
            Next

            For Each value As Label In New Label() {lblCod, lblDataFx, lblDataCreare,
                                                    lblDataDef, lblStare, lblStatus,
                                                    lblDescriere}
                value.ForeColor = scheme.Palette.TextColor
                value.BackColor = scheme.Palette.SurfaceAltColor
            Next

            lblEmpty.ForeColor = scheme.Palette.TextDimColor
            lblEmpty.BackColor = scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            ' Boundary UI (cascada de temă): logăm și înghițim.
            GlobalErrorLog.Write("SumarView.ApplyTheme", ex)
        End Try
    End Sub

End Class
