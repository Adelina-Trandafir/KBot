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
''' Slice 0010-01 (schelet): bază double-buffered, modele coloană/rând, cache de temă +
''' ApplyTheme, pictare antet + corp gol tematizat. Virtualizarea, tipurile de coloană,
''' formatarea, input-ul și editarea vin în pașii 0010-02..06.
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

    ' Adâncimea BeginUpdate/EndUpdate — cât e > 0, invalidările interne se amână.
    Private _updateDepth As Integer = 0

    ' Layout X al coloanelor vizibile, recalculat la pictare (zeci de coloane => neglijabil).
    Private Structure ColLayout
        Public Column As KBotDataColumn
        Public X As Integer
    End Structure
    Private ReadOnly _colLayout As New List(Of ColLayout)()

    ' ── Culori derivate din paletă (setate în ApplyTheme; default = SystemColors) ─
    Private _cBodyBack As Color
    Private _cHeaderBack As Color
    Private _cHeaderFore As Color
    Private _cBorder As Color
    Private _cSeparator As Color

    ' ── Resurse GDI cache-uite (recreate în ApplyTheme, eliberate în Dispose) ─────
    Private _bBodyBack As SolidBrush
    Private _bHeaderBack As SolidBrush
    Private _pBorder As Pen
    Private _pSeparator As Pen

    ' Font semibold pentru antet (derivat lazy din fontul ambient).
    Private _headerFont As Font

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.Selectable, True)
        TabStop = True
        InitializeComponent()
        SetDefaultColors()
        RebuildThemeResources()
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
        InvalidateContent()
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

    ''' <summary>Numărul de coloane înghețate (non-scrolling) din stânga. Folosit la virtualizare.</summary>
    Public Property FrozenColumnCount As Integer
        Get
            Return _frozenColumnCount
        End Get
        Set(value As Integer)
            _frozenColumnCount = Math.Max(0, value)
            InvalidateContent()
        End Set
    End Property

    ' ========================================================================
    ' API PUBLIC — Rânduri
    ' ========================================================================

    ''' <summary>Adaugă un rând gol și îl întoarce ca să-l umple caller-ul.</summary>
    Public Function AddRow() As KBotDataRow
        Dim r As New KBotDataRow()
        _rows.Add(r)
        InvalidateContent()
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

    ''' <summary>Golește toate rândurile.</summary>
    Public Sub ClearRows()
        _rows.Clear()
        InvalidateContent()
    End Sub

    ''' <summary>Rândurile marcate „murdare” (editate). Semantica se rafinează la editare (0010-06).</summary>
    Public Function GetDirtyRows() As IReadOnlyList(Of KBotDataRow)
        Dim dirty As New List(Of KBotDataRow)()
        For Each r In _rows
            If r.IsDirty Then dirty.Add(r)
        Next
        Return dirty
    End Function

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
            InvalidateCell(colKey, rowIndex)
        End Set
    End Property

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
            InvalidateContent()
        End Set
    End Property

    ''' <summary>Înălțimea benzii de antet (px). Implicit 30.</summary>
    Public Property HeaderHeight As Integer
        Get
            Return _headerHeight
        End Get
        Set(value As Integer)
            _headerHeight = Math.Max(0, value)
            InvalidateContent()
        End Set
    End Property

    ''' <summary>Afișează banda de antet. Implicit True.</summary>
    Public Property ShowHeader As Boolean
        Get
            Return _showHeader
        End Get
        Set(value As Boolean)
            _showHeader = value
            InvalidateContent()
        End Set
    End Property

    ''' <summary>Fundal alternant pe rânduri pare/impare. Implicit True (folosit din 0010-02).</summary>
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

    ' ========================================================================
    ' API PUBLIC — Bulk / refresh
    ' ========================================================================

    ''' <summary>Suspendă invalidările pe durata adăugărilor în masă.</summary>
    Public Sub BeginUpdate()
        _updateDepth += 1
    End Sub

    ''' <summary>Reia invalidarea; la revenirea la 0, repictează o dată.</summary>
    Public Sub EndUpdate()
        If _updateDepth > 0 Then _updateDepth -= 1
        If _updateDepth = 0 Then Invalidate()
    End Sub

    ''' <summary>Invalidează o celulă. 0010-01: invalidare integrală (dreptunghiul exact vine la virtualizare).</summary>
    Public Sub InvalidateCell(colKey As String, rowIndex As Integer)
        InvalidateContent()
    End Sub

    ''' <summary>Invalidează un rând. 0010-01: invalidare integrală (dreptunghiul exact vine la virtualizare).</summary>
    Public Sub InvalidateRow(rowIndex As Integer)
        InvalidateContent()
    End Sub

    ' Invalidare internă care respectă BeginUpdate/EndUpdate.
    Private Sub InvalidateContent()
        If _updateDepth = 0 Then Invalidate()
    End Sub

    ' ========================================================================
    ' TEMĂ
    ' ========================================================================

    ''' <summary>Reaplică culorile schemei active în cache-ul de resurse GDI și invalidează.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If scheme Is Nothing Then Return
        Dim p As ThemePalette = scheme.Palette
        ' Maparea sloturilor de paletă pe rolurile grilei. Fără culori literale.
        _cBodyBack = p.InputBackColor          ' corp / rând normal (alb în Classic)
        _cHeaderBack = p.ButtonBackColor       ' antet ușor „ridicat”
        _cHeaderFore = p.ButtonTextColor
        _cBorder = p.BorderColor
        _cSeparator = p.BorderColor
        BackColor = _cBodyBack
        RebuildThemeResources()
        Invalidate()
    End Sub

    ' Culorile pre-temă (până la primul ApplyTheme): SystemColors, ca randarea în designer.
    Private Sub SetDefaultColors()
        _cBodyBack = SystemColors.Window
        _cHeaderBack = SystemColors.Control
        _cHeaderFore = SystemColors.ControlText
        _cBorder = SystemColors.ControlDark
        _cSeparator = SystemColors.ControlLight
        BackColor = _cBodyBack
    End Sub

    ' Recreează pensulele/creioanele din culorile curente (eliberează-le pe cele vechi).
    Private Sub RebuildThemeResources()
        DisposeThemeResources()
        _bBodyBack = New SolidBrush(_cBodyBack)
        _bHeaderBack = New SolidBrush(_cHeaderBack)
        _pBorder = New Pen(_cBorder)
        _pSeparator = New Pen(_cSeparator)
    End Sub

    ' Eliberează resursele GDI cache-uite + fontul de antet (fără scurgeri).
    Private Sub DisposeThemeResources()
        _bBodyBack?.Dispose() : _bBodyBack = Nothing
        _bHeaderBack?.Dispose() : _bHeaderBack = Nothing
        _pBorder?.Dispose() : _pBorder = Nothing
        _pSeparator?.Dispose() : _pSeparator = Nothing
        _headerFont?.Dispose() : _headerFont = Nothing
    End Sub

    ' Fontul antetului: „semibold” derivat lazy din fontul ambient (fallback: bold).
    Private Function HeaderFont() As Font
        If _headerFont Is Nothing Then
            Try
                _headerFont = New Font("Segoe UI Semibold", Font.Size)
            Catch ex As Exception
                GlobalErrorLog.Write("KBotDataView.HeaderFont", ex)
                _headerFont = New Font(Font, FontStyle.Bold)
            End Try
        End If
        Return _headerFont
    End Function

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        _headerFont?.Dispose()
        _headerFont = Nothing
        Invalidate()
    End Sub

    ' ========================================================================
    ' PICTARE
    ' ========================================================================

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.FillRectangle(_bBodyBack, ClientRectangle)   ' corp gol = fundal rând normal

            RecalcColumnLayout()

            If _showHeader AndAlso _headerHeight > 0 Then
                DrawHeader(g)
            End If

            ' Corp: rândurile se pictează din 0010-02 (virtualizare). Aici doar fundalul.

            ' Chenar exterior.
            g.DrawRectangle(_pBorder, New Rectangle(0, 0, Width - 1, Height - 1))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnPaint", ex)
        End Try
    End Sub

    ' Banda de antet: fundal, textul fiecărei coloane vizibile, separatoare, linie de bază.
    ' Transitiv acoperit de Try/Catch-ul din OnPaint (regula casei) — fără Try propriu.
    Private Sub DrawHeader(g As Graphics)
        Dim headerRect As New Rectangle(0, 0, Width, _headerHeight)
        g.FillRectangle(_bHeaderBack, headerRect)

        Dim padX As Integer = ScaleDpi(8)
        Dim hf As Font = HeaderFont()

        For Each cl In _colLayout
            Dim cellRect As New Rectangle(cl.X, 0, cl.Column.Width, _headerHeight)

            Dim textRect As New Rectangle(cellRect.Left + padX, cellRect.Top,
                                          Math.Max(0, cellRect.Width - 2 * padX), cellRect.Height)
            TextRenderer.DrawText(g, cl.Column.HeaderText, hf, textRect, _cHeaderFore,
                HorizontalFlags(cl.Column.TextAlign) Or TextFormatFlags.VerticalCenter Or
                TextFormatFlags.EndEllipsis)

            Dim sepX As Integer = cellRect.Right - 1
            g.DrawLine(_pSeparator, sepX, 0, sepX, _headerHeight - 1)
        Next

        g.DrawLine(_pSeparator, 0, _headerHeight - 1, Width - 1, _headerHeight - 1)
    End Sub

    ' Recalculează offset-ul X al coloanelor vizibile (fără scroll orizontal în 0010-01).
    Private Sub RecalcColumnLayout()
        _colLayout.Clear()
        Dim x As Integer = 0
        For Each c In _columns
            If Not c.Visible Then Continue For
            _colLayout.Add(New ColLayout With {.Column = c, .X = x})
            x += c.Width
        Next
    End Sub

    ' ========================================================================
    ' AJUTOARE (geometrie pură; ThemeShapes din KBot.Theming e Friend, nu se vede de aici)
    ' ========================================================================

    ' Scalează o valoare logică (px @96dpi) la DPI-ul controlului. Fallback 96 înainte de handle.
    Private Function ScaleDpi(logical As Integer) As Integer
        Dim dpi As Integer = 96
        Try
            dpi = DeviceDpi
        Catch
            dpi = 96
        End Try
        Return CInt(Math.Round(logical * dpi / 96.0))
    End Function

    ' Alinierea orizontală a textului dintr-un ContentAlignment.
    Private Shared Function HorizontalFlags(align As ContentAlignment) As TextFormatFlags
        Select Case align
            Case ContentAlignment.TopRight, ContentAlignment.MiddleRight, ContentAlignment.BottomRight
                Return TextFormatFlags.Right
            Case ContentAlignment.TopCenter, ContentAlignment.MiddleCenter, ContentAlignment.BottomCenter
                Return TextFormatFlags.HorizontalCenter
            Case Else
                Return TextFormatFlags.Left
        End Select
    End Function

End Class
