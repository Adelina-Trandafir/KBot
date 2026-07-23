Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Grilă owner-drawn, NELEGATĂ de date, virtualizată — imită un formular continuu Access.
''' Widget-ul de listă partajat pentru vederile K-BOT (Sumar întâi, apoi Istoric, Recepții…).
''' Pictăm doar rândurile vizibile și plutim UN singur editor real peste celula activă, deci
''' numărul de handle-uri rămâne mic indiferent de câte rânduri sunt.
'''
''' Se auto-tematizează prin <see cref="IThemedControl"/> (ca familia KBotNavList): culorile
''' vin din paleta schemei active (ApplyTheme), niciodată literale.
'''
''' Fișierul acesta ține STAREA + API-ul public. Restul e împărțit pe parțiale, ca la
''' AdvancedTreeControl: <c>.Theming</c> (culori/resurse GDI), <c>.Layout</c> (geometrie,
''' virtualizare, scrollbar-uri), <c>.Painting</c> (pictare).
''' </summary>
<ToolboxItem(True)>
Public Class KBotDataView
    Inherits Control
    Implements IThemedControl

    ' ── Model (deținut de control) ──────────────────────────────────────────────
    Private ReadOnly _columns As New List(Of KBotDataColumn)()
    Private ReadOnly _columnIndex As New Dictionary(Of String, KBotDataColumn)(StringComparer.Ordinal)
    Private ReadOnly _rows As New List(Of KBotDataRow)()

    ' ── Aspect / comportament ───────────────────────────────────────────────────
    Private _rowHeight As Integer = 28
    Private _headerHeight As Integer = 30
    Private _showHeader As Boolean = True
    Private _alternatingRows As Boolean = True
    Private _readOnlyGrid As Boolean = False
    Private _frozenColumnCount As Integer = 0

    ' ── Totals row (slice 0017-01) ───────────────────────────────────────────────
    ' A pinned band at the bottom, painted with the header's band styling. It is NOT a row:
    ' excluded from _rows, RowCount, virtualization, selection, hit-testing and dirty tracking.
    ' _totalsRowHeight <= 0 means "track HeaderHeight"; a real height overrides it.
    Private _showTotalsRow As Boolean = False
    Private _totalsRowHeight As Integer = 0
    ' Cached formatted aggregate text per column key, recomputed when the model changes
    ' (AddRow / ClearRows / EndUpdate / committed edit) so paint never re-aggregates.
    Private ReadOnly _totalsText As New Dictionary(Of String, String)(StringComparer.Ordinal)

    ' Celula curentă (selecție). -1 / Nothing = fără selecție. Se schimbă DOAR prin
    ' SetCurrentCell (vezi partiala .Input), ca evenimentul să se ridice o singură dată.
    Private _currentRowIndex As Integer = -1
    Private _currentColumnKey As String

    ' Adâncimea BeginUpdate/EndUpdate — cât e > 0, invalidările interne se amână.
    Private _updateDepth As Integer = 0

    ' ── Evenimente de formatare (handler-ele „bogate” vin în 0010-04) ────────────

    ''' <summary>Ridicat pentru fiecare celulă pictată. Argumentele sunt REFOLOSITE — nu le reține.</summary>
    Public Event CellFormatting As EventHandler(Of KBotCellFormattingEventArgs)

    ''' <summary>Ridicat o dată pe rând, înaintea celulelor. Argumentele sunt REFOLOSITE — nu le reține.</summary>
    Public Event RowFormatting As EventHandler(Of KBotRowFormattingEventArgs)

    ' ── Evenimente de interacțiune (argumente proaspete, nu refolosite) ─────────

    ''' <summary>Click simplu pe o celulă.</summary>
    Public Event CellClick As EventHandler(Of KBotCellEventArgs)

    ''' <summary>Dublu-click pe o celulă.</summary>
    Public Event CellDoubleClick As EventHandler(Of KBotCellEventArgs)

    ''' <summary>Celulă de tip Button acționată (click sau Space).</summary>
    Public Event ButtonClick As EventHandler(Of KBotButtonClickEventArgs)

    ''' <summary>Valoarea unei celule s-a schimbat (comutare sau commit de editare).</summary>
    Public Event CellValueChanged As EventHandler(Of KBotCellValueEventArgs)

    ''' <summary>Rândul sau coloana curentă s-a schimbat.</summary>
    Public Event SelectionChanged As EventHandler

    ' Instanțe REFOLOSITE de argumente (zero alocări per celulă la mii de rânduri).
    Private ReadOnly _cellArgs As New KBotCellFormattingEventArgs()
    Private ReadOnly _rowArgs As New KBotRowFormattingEventArgs()

    ' Instanțe SEPARATE pentru interogări din afara pictării (IsCellEnabled / input), ca o
    ' întrebare să nu calce peste argumentele unei pictări în curs.
    Private ReadOnly _probeCellArgs As New KBotCellFormattingEventArgs()
    Private ReadOnly _probeRowArgs As New KBotRowFormattingEventArgs()

    ''' <summary>
    ''' Câte rânduri de date a pictat ultimul <c>OnPaint</c>. Poarta de verificare a
    ''' virtualizării în teste (headless, prin DrawToBitmap) — nu e API public.
    ''' </summary>
    Friend Property DebugLastPaintedDataRows As Integer

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable, True)
        TabStop = True
        InitializeComponent()
        SetDefaultColors()
        RebuildThemeResources()
        WireScrollBars()
        WireEditors()
    End Sub

    ' ========================================================================
    ' API PUBLIC — Coloane
    ' ========================================================================

    ''' <summary>Adaugă o coloană. Cheia trebuie să fie nevidă și unică (altfel ArgumentException).</summary>
    Public Function AddColumn(key As String, headerText As String, type As KBotColumnType, width As Integer) As KBotDataColumn
        If String.IsNullOrWhiteSpace(key) Then Throw New ArgumentException("Cheie vidă.", NameOf(key))
        If _columnIndex.ContainsKey(key) Then Throw New ArgumentException($"Cheie de coloană duplicată: '{key}'.", NameOf(key))
        Dim col As New KBotDataColumn(key, headerText, type, width)
        _columns.Add(col)
        _columnIndex(key) = col
        RecomputeTotals()
        LayoutChanged()
        Return col
    End Function

    ''' <summary>Coloanele, în ordinea de inserare (doar-citire).</summary>
    Public ReadOnly Property Columns As IReadOnlyList(Of KBotDataColumn)
        Get
            Return _columns
        End Get
    End Property

    ''' <summary>Coloana după cheie. Cheie necunoscută => ArgumentException (fără no-op tăcut).</summary>
    Public Function Column(key As String) As KBotDataColumn
        Dim col As KBotDataColumn = Nothing
        If key IsNot Nothing AndAlso _columnIndex.TryGetValue(key, col) Then Return col
        Throw New ArgumentException($"Cheie de coloană necunoscută: '{key}'.", NameOf(key))
    End Function

    ''' <summary>
    ''' Câte coloane vizibile din stânga sunt înghețate (nu derulează orizontal).
    ''' NOTĂ: acesta e mecanismul autoritar; <c>KBotDataColumn.Frozen</c> rămâne metadata
    ''' neutilizată deocamdată (vezi worklog 0010-02).
    ''' </summary>
    Public Property FrozenColumnCount As Integer
        Get
            Return _frozenColumnCount
        End Get
        Set(value As Integer)
            _frozenColumnCount = Math.Max(0, value)
            LayoutChanged()
        End Set
    End Property

    ' ========================================================================
    ' API PUBLIC — Rânduri
    ' ========================================================================

    ''' <summary>Adaugă un rând gol și îl întoarce ca să-l umple caller-ul.</summary>
    Public Function AddRow() As KBotDataRow
        Dim r As New KBotDataRow()
        _rows.Add(r)
        RecomputeTotals()
        LayoutChanged()
        Return r
    End Function

    ''' <summary>Rândurile, în ordinea de inserare (doar-citire).</summary>
    Public ReadOnly Property Rows As IReadOnlyList(Of KBotDataRow)
        Get
            Return _rows
        End Get
    End Property

    ''' <summary>Numărul de rânduri.</summary>
    Public ReadOnly Property RowCount As Integer
        Get
            Return _rows.Count
        End Get
    End Property

    ''' <summary>Golește toate rândurile (și selecția).</summary>
    Public Sub ClearRows()
        _rows.Clear()
        _currentRowIndex = -1
        RecomputeTotals()
        LayoutChanged()
    End Sub

    ''' <summary>
    ''' Rândurile EDITATE DE OPERATOR de la ultima curățare (commit de editare sau comutare de
    ''' bifă/opțiune). Încărcarea programatică a datelor NU apare aici — vezi
    ''' <see cref="KBotDataRow.IsDirty"/>.
    ''' </summary>
    Public Function GetDirtyRows() As IReadOnlyList(Of KBotDataRow)
        Dim dirty As New List(Of KBotDataRow)()
        For Each r In _rows
            If r.IsDirty Then dirty.Add(r)
        Next
        Return dirty
    End Function

    ''' <summary>
    ''' Coboară steagul „editat” pe TOATE rândurile — baseline curat după o încărcare sau
    ''' după ce modificările au fost trimise la server.
    ''' </summary>
    Public Sub ClearDirty()
        For Each r In _rows
            r.MarkClean()
        Next
    End Sub

    ' ========================================================================
    ' API PUBLIC — Valori
    ' ========================================================================

    ''' <summary>
    ''' Valoarea celulei (cheie coloană × index rând). SET scrie în rând (ridică IsDirty) și
    ''' invalidează celula. Index de rând în afara intervalului => excepție.
    ''' </summary>
    Default Public Property Item(colKey As String, rowIndex As Integer) As Object
        Get
            Return _rows(rowIndex)(colKey)
        End Get
        Set(value As Object)
            _rows(rowIndex)(colKey) = value
            ' English (slice 0017-01): a per-cell write through the control keeps the totals
            ' band live (guarded internally against BeginUpdate batches — bulk loads recompute
            ' once at EndUpdate). Writing straight to KBotDataRow bypasses this by design; those
            ' loads are always wrapped in BeginUpdate/EndUpdate.
            RecomputeTotals()
            InvalidateCell(colKey, rowIndex)
        End Set
    End Property

    ' ========================================================================
    ' API PUBLIC — Activare efectivă (coloană × rând × celulă)
    ' ========================================================================

    ''' <summary>
    ''' Rândul e activ? = <c>Row.Enabled</c>, apoi eventualul veto din <c>RowFormatting</c>.
    ''' </summary>
    Public Function IsRowEnabled(rowIndex As Integer) As Boolean
        Try
            Dim row As KBotDataRow = _rows(rowIndex)
            _probeRowArgs.Reset(rowIndex, row, BackColor, ForeColor, row.Enabled)
            RaiseEvent RowFormatting(Me, _probeRowArgs)
            Return _probeRowArgs.Enabled
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.IsRowEnabled", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Activarea EFECTIVĂ a unei celule: <c>Column.Enabled AndAlso Row.Enabled AndAlso
    ''' cellEnabled</c>, unde cele două din urmă trec prin <c>RowFormatting</c>, respectiv
    ''' <c>CellFormatting</c> (handler-ul poate coborî pe False, nu poate ridica peste
    ''' rând/coloană). Aceeași rezoluție ca la pictare — și baza pentru blocarea
    ''' input-ului/editării (0010-05/06).
    ''' </summary>
    Public Function IsCellEnabled(colKey As String, rowIndex As Integer) As Boolean
        Try
            Dim col As KBotDataColumn = Column(colKey)   ' cheie necunoscută => ArgumentException
            If Not col.Enabled Then Return False
            If Not IsRowEnabled(rowIndex) Then Return False

            Dim row As KBotDataRow = _rows(rowIndex)
            Dim value As Object = row(colKey)
            _probeCellArgs.Reset(col, row, rowIndex, value, String.Empty,
                                 BackColor, ForeColor, Font, col.TextAlign, True)
            RaiseEvent CellFormatting(Me, _probeCellArgs)
            Return _probeCellArgs.Enabled
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.IsCellEnabled", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Setează o celulă de tip <see cref="KBotColumnType.OptionButton"/>. Bifarea (True) le
    ''' stinge pe celelalte opțiuni din ACELAȘI RÂND care au același <c>OptionGroup</c> —
    ''' exclusivitatea cerută de un grup de butoane radio. Un grup vid => opțiune independentă.
    ''' </summary>
    Public Sub SetOptionValue(colKey As String, rowIndex As Integer, value As Boolean)
        Try
            Dim col As KBotDataColumn = Column(colKey)   ' cheie necunoscută => ArgumentException
            If col.ColumnType <> KBotColumnType.OptionButton Then
                Throw New ArgumentException($"Coloana '{colKey}' nu e de tip OptionButton.", NameOf(colKey))
            End If
            Dim row As KBotDataRow = _rows(rowIndex)
            row(colKey) = value
            If value Then ClearOptionSiblings(row, col)
            row.IsDirty = True          ' e o COMUTARE, nu o încărcare
            InvalidateRow(rowIndex)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.SetOptionValue", ex)
            Throw
        End Try
    End Sub

    ' Stinge opțiunile-surori din același rând + același grup (exclusivitate radio).
    Private Sub ClearOptionSiblings(row As KBotDataRow, col As KBotDataColumn)
        If String.IsNullOrEmpty(col.OptionGroup) Then Return
        For Each other In _columns
            If ReferenceEquals(other, col) Then Continue For
            If other.ColumnType <> KBotColumnType.OptionButton Then Continue For
            If Not String.Equals(other.OptionGroup, col.OptionGroup, StringComparison.Ordinal) Then Continue For
            row(other.Key) = False
        Next
    End Sub

    ' ========================================================================
    ' API PUBLIC — Aspect / comportament
    ' ========================================================================

    ''' <summary>Înălțimea fixă a unui rând (px). Implicit 28.</summary>
    Public Property RowHeight As Integer
        Get
            Return _rowHeight
        End Get
        Set(value As Integer)
            _rowHeight = Math.Max(1, value)
            LayoutChanged()
        End Set
    End Property

    ''' <summary>Înălțimea benzii de antet (px). Implicit 30.</summary>
    Public Property HeaderHeight As Integer
        Get
            Return _headerHeight
        End Get
        Set(value As Integer)
            _headerHeight = Math.Max(0, value)
            LayoutChanged()
        End Set
    End Property

    ''' <summary>Afișează banda de antet. Implicit True.</summary>
    Public Property ShowHeader As Boolean
        Get
            Return _showHeader
        End Get
        Set(value As Boolean)
            _showHeader = value
            LayoutChanged()
        End Set
    End Property

    ''' <summary>Fundal alternant pe rânduri pare/impare. Implicit True.</summary>
    Public Property AlternatingRows As Boolean
        Get
            Return _alternatingRows
        End Get
        Set(value As Boolean)
            _alternatingRows = value
            InvalidateContent()
        End Set
    End Property

    ''' <summary>True => nicio celulă nu intră vreodată în editare (vederi read-only, ex. Sumar).</summary>
    Public Property ReadOnlyGrid As Boolean
        Get
            Return _readOnlyGrid
        End Get
        Set(value As Boolean)
            _readOnlyGrid = value
        End Set
    End Property

    ''' <summary>
    ''' English (slice 0017-01): show a pinned totals band at the bottom. Default False. The band
    ''' aggregates each column per <see cref="KBotDataColumn.Aggregate"/>, participates in
    ''' horizontal scroll + frozen columns exactly like the body, and is never a selectable row.
    ''' Turning it on shrinks the scrollable body by <see cref="TotalsRowHeight"/>.
    ''' </summary>
    Public Property ShowTotalsRow As Boolean
        Get
            Return _showTotalsRow
        End Get
        Set(value As Boolean)
            If _showTotalsRow = value Then Return
            _showTotalsRow = value
            RecomputeTotals()
            LayoutChanged()
        End Set
    End Property

    ''' <summary>
    ''' English (slice 0017-01): height of the pinned totals band, in pixels. Defaults to
    ''' <see cref="HeaderHeight"/> until set to a positive value; a non-positive value restores
    ''' the "track HeaderHeight" default.
    ''' </summary>
    Public Property TotalsRowHeight As Integer
        Get
            Return If(_totalsRowHeight > 0, _totalsRowHeight, _headerHeight)
        End Get
        Set(value As Integer)
            _totalsRowHeight = Math.Max(0, value)
            LayoutChanged()
        End Set
    End Property

    ' ========================================================================
    ' API PUBLIC — Bulk / refresh
    ' ========================================================================

    ''' <summary>Suspendă invalidările/relayout-ul pe durata adăugărilor în masă.</summary>
    Public Sub BeginUpdate()
        _updateDepth += 1
    End Sub

    ''' <summary>Reia pictarea; la revenirea la 0 recalculează layout-ul și repictează o dată.</summary>
    Public Sub EndUpdate()
        If _updateDepth > 0 Then _updateDepth -= 1
        If _updateDepth = 0 Then
            RecomputeTotals()
            UpdateLayout()
            Invalidate()
        End If
    End Sub

    ''' <summary>Invalidează o celulă. Deocamdată invalidare integrală (suficient, repictarea e ieftină).</summary>
    Public Sub InvalidateCell(colKey As String, rowIndex As Integer)
        InvalidateContent()
    End Sub

    ''' <summary>Invalidează un rând. Deocamdată invalidare integrală.</summary>
    Public Sub InvalidateRow(rowIndex As Integer)
        InvalidateContent()
    End Sub

    ' Invalidare care respectă BeginUpdate/EndUpdate (doar repictare).
    Private Sub InvalidateContent()
        If _updateDepth = 0 Then Invalidate()
    End Sub

    ' Schimbare care afectează geometria (coloane/rânduri/înălțimi): relayout + repictare.
    Private Sub LayoutChanged()
        If _updateDepth <> 0 Then Return
        UpdateLayout()
        Invalidate()
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Try
            UpdateLayout()
        Catch ex As Exception
            ' Boundary UI: relayout-ul nu are voie să arunce în bucla de mesaje.
            GlobalErrorLog.Write("KBotDataView.OnResize", ex)
        End Try
    End Sub

End Class
