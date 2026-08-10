Option Strict On
Imports System.ComponentModel
Imports System.Linq
Imports KBot.Common

''' <summary>
''' SORTAREA și FILTRAREA <see cref="KBotDataView"/> (slice 0028-03) — adică harta dintre ce
''' ține grila și ce se vede pe ecran.
'''
''' <para><b>Două numerotări, și niciodată amestecate.</b> Un <i>index de model</i> arată în
''' <c>_rows</c>, adică în ordinea în care apelantul a adăugat rândurile. O <i>poziție de vedere</i>
''' arată în <see cref="_view"/>, adică în ordinea de pe ecran, după ce filtrarea a scos rânduri și
''' sortarea le-a rearanjat. Regula, fără excepție:</para>
'''
''' <list type="bullet">
''' <item><description><b>API-ul public vorbește în indici de MODEL</b> — <c>Item(cheie, index)</c>,
''' <c>CurrentRowIndex</c>, <c>CellClick</c>, <c>EnsureVisible</c>, <c>IsRowEnabled</c> înseamnă azi
''' exact ce însemnau înainte de filtrare. Un apelant care ține minte un index nu are ce afla
''' despre filtre.</description></item>
''' <item><description><b>Geometria lucrează în poziții de VEDERE</b> — <c>RowTop</c>,
''' <c>FirstVisibleRow</c>, virtualizarea, hit-testul. Ele nu au ce face cu rândurile ascunse.</description></item>
''' </list>
'''
''' <para><b>De ce se reconstruiește leneș.</b> <see cref="_view"/> se marchează murdar la orice
''' schimbare de model și se recalculează la prima CITIRE (<see cref="EnsureView"/>), nu pe loc.
''' Altfel o încărcare în masă între <c>BeginUpdate</c> și <c>EndUpdate</c> ar lăsa harta în urma
''' rândurilor, iar o pictare căzută la mijloc ar indexa în gol — exact felul de excepție care apare
''' o dată la o mie de încărcări și nu se reproduce niciodată.</para>
'''
''' <para><b>Ordinea: întâi filtrarea, apoi sortarea.</b> Invers ar însemna să ordonăm rânduri pe
''' care oricum le aruncăm.</para>
''' </summary>
Partial Class KBotDataView

    ' Indicii de MODEL, în ordinea de afișare. Rebuild leneș — vezi EnsureView.
    Private ReadOnly _view As New List(Of Integer)()

    ' Drumul invers: index de model -> poziție de vedere, sau -1 dacă rândul e filtrat afară.
    ' Ținut ca tablou (nu dicționar): o căutare per rând pictat, de mii de ori pe secundă.
    Private _viewOf As Integer() = Array.Empty(Of Integer)()

    Private _viewDirty As Boolean = True

    ' Sortarea curentă (o singură coloană, ca în foaia de date Access).
    Private _sortKey As String = Nothing
    Private _sortDirection As KBotSortDirection = KBotSortDirection.None

    ' Filtrele active, pe cheie de coloană. Un filtru inactiv nu intră aici (vezi SetColumnFilter).
    Private ReadOnly _filters As New Dictionary(Of String, KBotColumnFilter)(StringComparer.Ordinal)

    ''' <summary>Filtrele s-au schimbat (așezate, modificate sau șterse).</summary>
    Public Event FilterChanged As EventHandler

    ''' <summary>Sortarea s-a schimbat.</summary>
    Public Event SortChanged As EventHandler

    ' ══════════════════════════════════════════════════════════════════════════
    ' HARTA — reconstrucție leneșă
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Marchează harta murdară. O cheamă orice schimbare de model (rânduri adăugate/golite,
    ''' valoare scrisă, format schimbat) — vezi <c>RecomputeDerived</c>.
    ''' </summary>
    Friend Sub InvalidateView()
        _viewDirty = True
    End Sub

    ''' <summary>
    ''' Se asigură că harta e la zi. O cheamă FIECARE cititor (pictare, geometrie, hit-test,
    ''' agregate) — e ieftină cât timp nu s-a schimbat nimic.
    ''' </summary>
    Friend Sub EnsureView()
        If Not _viewDirty Then Return
        RebuildView()
    End Sub

    ' Reconstruiește harta: întâi filtrarea, apoi sortarea. PURĂ față de restul controlului —
    ' nu invalidează, nu ridică evenimente, nu atinge selecția. Așa poate fi chemată din
    ' mijlocul unei pictări fără să pornească o recursie de invalidări.
    Private Sub RebuildView()
        Try
            _view.Clear()

            For i As Integer = 0 To _rows.Count - 1
                If RowPassesFilters(i) Then _view.Add(i)
            Next

            If _sortDirection <> KBotSortDirection.None AndAlso Not String.IsNullOrEmpty(_sortKey) Then
                SortView()
            End If

            ' Drumul invers, refăcut din zero: un rând filtrat afară rămâne pe -1.
            If _viewOf.Length <> _rows.Count Then ReDim _viewOf(Math.Max(0, _rows.Count - 1))
            For i As Integer = 0 To _viewOf.Length - 1
                _viewOf(i) = -1
            Next
            For pozitie As Integer = 0 To _view.Count - 1
                _viewOf(_view(pozitie)) = pozitie
            Next

            _viewDirty = False
        Catch ex As Exception
            ' Boundary: harta e citită din pictare, deci o excepție aici ar cădea în bucla de
            ' mesaje. Logăm și lăsăm o hartă IDENTITATE — grila arată tot, adică prea mult,
            ' niciodată prea puțin: un filtru care se pierde se vede, unul care ascunde greșit nu.
            GlobalErrorLog.Write("KBotDataView.RebuildView", ex)
            FallbackIdentityView()
        End Try
    End Sub

    ' Harta identitate (fără filtrare, fără sortare) — plasa de siguranță de mai sus.
    Private Sub FallbackIdentityView()
        _view.Clear()
        If _viewOf.Length <> _rows.Count Then ReDim _viewOf(Math.Max(0, _rows.Count - 1))
        For i As Integer = 0 To _rows.Count - 1
            _view.Add(i)
            _viewOf(i) = i
        Next
        _viewDirty = False
    End Sub

    ' Sortarea vederii. List.Sort NU e stabilă, deci comparatorul cade la egalitate pe indexul de
    ' model: fără asta, două rânduri cu aceeași valoare și-ar schimba locul între ele la fiecare
    ' resortare, iar o grilă care se rearanjează singură arată ca un bug.
    Private Sub SortView()
        Dim col As KBotDataColumn = Nothing
        If Not _columnIndex.TryGetValue(_sortKey, col) Then Return

        Dim cheie As String = col.Key
        Dim tip As KBotValueType = col.ValueType
        Dim descrescator As Boolean = (_sortDirection = KBotSortDirection.Descending)

        _view.Sort(Function(a As Integer, b As Integer) As Integer
                       Dim semn As Integer = KBotFilterEngine.Compare(_rows(a)(cheie), _rows(b)(cheie), tip)
                       If semn <> 0 Then Return If(descrescator, -semn, semn)
                       Return a.CompareTo(b)
                   End Function)
    End Sub

    ' Rândul trece de TOATE filtrele active? Un filtru pe o coloană dispărută între timp se sare.
    Private Function RowPassesFilters(modelIndex As Integer) As Boolean
        If _filters.Count = 0 Then Return True
        Dim row As KBotDataRow = _rows(modelIndex)

        For Each pereche In _filters
            Dim col As KBotDataColumn = Nothing
            If Not _columnIndex.TryGetValue(pereche.Key, col) Then Continue For
            Dim brut As Object = row(pereche.Key)
            If Not pereche.Value.Matches(brut, FormatValue(brut, col), col.ValueType) Then Return False
        Next

        Return True
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' TRADUCEREA celor două numerotări
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Câte rânduri se văd (după filtrare). Nu confunda cu numărul celor de pe ecran.</summary>
    Friend Function ViewCount() As Integer
        EnsureView()
        Return _view.Count
    End Function

    ''' <summary>Indexul de MODEL al unei poziții de vedere, sau -1 dacă poziția nu există.</summary>
    Friend Function ModelIndexAt(viewPosition As Integer) As Integer
        EnsureView()
        If viewPosition < 0 OrElse viewPosition >= _view.Count Then Return -1
        Return _view(viewPosition)
    End Function

    ''' <summary>Poziția de VEDERE a unui index de model, sau -1 dacă rândul e filtrat afară.</summary>
    Friend Function ViewPositionOf(modelIndex As Integer) As Integer
        EnsureView()
        If modelIndex < 0 OrElse modelIndex >= _viewOf.Length Then Return -1
        Return _viewOf(modelIndex)
    End Function

    ''' <summary>Rândul de la o poziție de vedere, sau Nothing.</summary>
    Friend Function ViewRowAt(viewPosition As Integer) As KBotDataRow
        Dim mi As Integer = ModelIndexAt(viewPosition)
        If mi < 0 Then Return Nothing
        Return _rows(mi)
    End Function

    ''' <summary>
    ''' Rândurile VIZIBILE, în ordinea de afișare. Pe ele lucrează agregatele din subsol și
    ''' măsurarea de auto-dimensionare: un total care ar aduna și rândurile ascunse de un filtru
    ''' n-ar mai fi totalul paginii pe care o citește operatorul.
    ''' </summary>
    Friend Function ViewRows() As IEnumerable(Of KBotDataRow)
        EnsureView()
        Return _view.Select(Function(mi) _rows(mi))
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' API PUBLIC — sortare
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Cheia coloanei după care se sortează (Nothing = nesortat).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SortColumnKey As String
        Get
            Return _sortKey
        End Get
    End Property

    ''' <summary>Sensul sortării curente.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SortDirection As KBotSortDirection
        Get
            Return _sortDirection
        End Get
    End Property

    ''' <summary>
    ''' Sortează după o coloană. <see cref="KBotSortDirection.None"/> (sau o cheie vidă) readuce
    ''' ordinea de încărcare. Cheie necunoscută => <see cref="ArgumentException"/>, ca peste tot.
    ''' </summary>
    Public Sub ApplySort(colKey As String, direction As KBotSortDirection)
        Try
            If direction = KBotSortDirection.None OrElse String.IsNullOrEmpty(colKey) Then
                _sortKey = Nothing
                _sortDirection = KBotSortDirection.None
            Else
                Column(colKey)                  ' cheie necunoscută => ArgumentException
                _sortKey = colKey
                _sortDirection = direction
            End If
            AfterViewStateChanged()
            RaiseEvent SortChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ApplySort", ex)
            Throw
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' API PUBLIC — filtrare
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Vreo coloană e filtrată?</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsFiltered As Boolean
        Get
            Return _filters.Count > 0
        End Get
    End Property

    ''' <summary>Câte rânduri trec de filtrele active (= câte se văd).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property FilteredRowCount As Integer
        Get
            Return ViewCount()
        End Get
    End Property

    ''' <summary>Filtrul așezat pe o coloană, sau <c>Nothing</c> dacă nu e niciunul.</summary>
    Public Function ColumnFilter(colKey As String) As KBotColumnFilter
        Dim f As KBotColumnFilter = Nothing
        If colKey IsNot Nothing AndAlso _filters.TryGetValue(colKey, f) Then Return f
        Return Nothing
    End Function

    ''' <summary>Coloana e filtrată? (Pictograma de antet o citește ca să se arate „aprinsă”.)</summary>
    Public Function HasColumnFilter(colKey As String) As Boolean
        Return ColumnFilter(colKey) IsNot Nothing
    End Function

    ''' <summary>
    ''' Așază (sau înlocuiește) filtrul unei coloane. Un filtru care nu restrânge nimic
    ''' (<see cref="KBotColumnFilter.IsActive"/> = False) ȘTERGE filtrul coloanei — altfel antetul
    ''' ar purta semnul de «filtrat» fără ca ceva să fie filtrat.
    ''' </summary>
    Public Sub SetColumnFilter(filter As KBotColumnFilter)
        Try
            If filter Is Nothing Then Throw New ArgumentNullException(NameOf(filter))
            Column(filter.ColumnKey)            ' cheie necunoscută => ArgumentException
            If filter.IsActive Then
                _filters(filter.ColumnKey) = filter
            Else
                _filters.Remove(filter.ColumnKey)
            End If
            AfterViewStateChanged()
            RaiseEvent FilterChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.SetColumnFilter", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Ridică filtrul unei coloane. O coloană nefiltrată = no-op tăcut (nu e o eroare).</summary>
    Public Sub ClearColumnFilter(colKey As String)
        Try
            If colKey Is Nothing OrElse Not _filters.Remove(colKey) Then Return
            AfterViewStateChanged()
            RaiseEvent FilterChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ClearColumnFilter", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Ridică TOATE filtrele (sortarea rămâne pe loc).</summary>
    Public Sub ClearAllFilters()
        Try
            If _filters.Count = 0 Then Return
            _filters.Clear()
            AfterViewStateChanged()
            RaiseEvent FilterChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ClearAllFilters", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Valorile DISTINCTE ale unei coloane, așa cum se afișează ele, pentru lista de bifat.
    ''' Celulele goale intră o singură dată, sub <see cref="KBotFilterEngine.CheieGol"/>.
    '''
    ''' <para>Se calculează peste rândurile care trec de filtrele CELORLALTE coloane, nu peste
    ''' toate: exact ca în Access, unde o listă deja restrânsă nu-ți mai oferă valori care oricum
    ''' n-ar aduce niciun rând. Filtrul coloanei ÎNSEȘI se ignoră — altfel, odată bifată o valoare,
    ''' celelalte ar dispărea din listă și nu s-ar mai putea răzgândi nimeni.</para>
    ''' </summary>
    Public Function DistinctDisplayValues(colKey As String) As List(Of String)
        Try
            Dim col As KBotDataColumn = Column(colKey)      ' cheie necunoscută => ArgumentException
            Dim vazute As New HashSet(Of String)(StringComparer.CurrentCultureIgnoreCase)
            Dim rezultat As New List(Of String)()

            For i As Integer = 0 To _rows.Count - 1
                If Not RowPassesOtherFilters(i, colKey) Then Continue For
                Dim brut As Object = _rows(i)(colKey)
                Dim text As String = If(KBotFilterEngine.IsBlank(brut),
                                        KBotFilterEngine.CheieGol,
                                        FormatValue(brut, col))
                If vazute.Add(text) Then rezultat.Add(text)
            Next

            ' Ordinea din listă e cea de sortare a coloanei, nu cea de întâlnire: o listă de valori
            ' în ordine aleatoare se citește la fel de greu ca o coloană nesortată.
            rezultat.Sort(Function(a, b) KBotFilterEngine.Compare(a, b, col.ValueType))
            Return rezultat
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.DistinctDisplayValues", ex)
            Throw
        End Try
    End Function

    ' Rândul trece de filtrele TUTUROR coloanelor în afară de una (vezi DistinctDisplayValues).
    Private Function RowPassesOtherFilters(modelIndex As Integer, exceptKey As String) As Boolean
        If _filters.Count = 0 Then Return True
        Dim row As KBotDataRow = _rows(modelIndex)

        For Each pereche In _filters
            If String.Equals(pereche.Key, exceptKey, StringComparison.Ordinal) Then Continue For
            Dim col As KBotDataColumn = Nothing
            If Not _columnIndex.TryGetValue(pereche.Key, col) Then Continue For
            Dim brut As Object = row(pereche.Key)
            If Not pereche.Value.Matches(brut, FormatValue(brut, col), col.ValueType) Then Return False
        Next

        Return True
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' DUPĂ o schimbare de sortare/filtrare
    ' ══════════════════════════════════════════════════════════════════════════

    ' Harta se reface, subsolul se re-agregă peste rândurile rămase, iar selecția se verifică:
    ' rândul curent poate să fi fost tocmai filtrat afară, și o selecție pe un rând invizibil ar
    ' muta editarea și săgețile într-un loc pe care nimeni nu-l vede.
    Private Sub AfterViewStateChanged()
        RecomputeDerived()          ' marchează harta murdară ȘI re-agregă subsolul peste ea
        DropSelectionIfHidden()
        LayoutChanged()             ' rândurile vizibile s-au schimbat => alt conținut de derulat
    End Sub

    ' Selecția cade dacă rândul ei nu mai e vizibil. Editarea deschisă se abandonează întâi —
    ' un editor plutind peste un rând care tocmai a dispărut ar rămâne agățat în aer.
    Private Sub DropSelectionIfHidden()
        If _currentRowIndex < 0 Then Return
        If ViewPositionOf(_currentRowIndex) >= 0 Then Return
        If _editing Then CancelEdit()
        _currentRowIndex = -1
        RaiseEvent SelectionChanged(Me, EventArgs.Empty)
    End Sub

End Class
