Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.Json

''' <summary>
''' The «Tabele» page of «Baze de date»: the tables of the database chosen in the left tree, opened read-only in a grid. Left: the tables (and
''' views) of the database. Right, top: the rows of the chosen table (the first <see cref="PageSize"/>, then «Încarcă încă»). Right, bottom:
''' what the chosen cell holds (a text box that shows what the grid has to cut), the facts about the table and the facts about the chosen
''' column (type, keys, and the table a foreign key points at). Every panel is resized with the mouse.
'''
''' <para>Column widths follow the type, not the data: an id ~100 px, a date as wide as «30.12.2026», a date-time with its time, numbers with the
''' thousands separator (and decimals unless the column is a whole number), tinyint(1) / bit(1) as a tick box, a long text at a preset width
''' with an ellipsis, a blob as «&lt;BLOB&gt;». Nothing here is editable. The server half is <c>routes/admin_cli/admin_dbview.py</c>.</para>
'''
''' <para><b>Linked tables.</b> A cell of a column that is a foreign key (or is pointed at by other tables) is drawn as a link (accent colour, underlined) and ONE click on it opens a popup with
''' the linked tables; choosing one opens it with only the rows whose linked column equals the value of the cell (the filter is made by the
''' server, so it covers the whole table, not just the loaded rows). Every such step is a <see cref="NavNode"/> of a history TREE: the
''' «back» button leaves for the parent (a list of all the steps when there are several), the «forward» button for the children (see
''' <see cref="ForwardList"/>). A table opened from the left tree starts a new history. Whatever was read is kept in memory
''' (<see cref="_tablesCache"/>, <see cref="_viewCache"/>) until «Reîncarcă» is pressed.</para>
''' </summary>
Public Class DbBrowserView
    Implements IAdminView
    Implements IThemedContainer

    Private Const PageSize As Integer = 1000
    Private Const KeyCol As String = "k"
    Private Const ValCol As String = "v"
    Private Const TablePrefix As String = "t|"
    Private Const GroupTagPrefix As String = "g|"
    Private Const NoGroup As String = "_Fara Grupare"
    ''' <summary>The tree group of the catalog queries (<see cref="QueryCatalog"/>), always the first one; its rows carry <see cref="QueryPrefix"/> + the id of the query as tag.</summary>
    Private Const QueryGroup As String = "_Interogari"
    Private Const QueryPrefix As String = "q|"
    ''' <summary>The tag of a row that stands for a query file that cannot be read: a click says what is wrong with it.</summary>
    Private Const QueryProblemPrefix As String = "qe|"
    ''' <summary>The most rows a query shows (the server's own maximum); a query that returns more is cut and the page says so.</summary>
    Private Const QueryLimit As Integer = 5000
    ''' <summary>What the title of a query result starts with, so it can never be taken for the name of a table.</summary>
    Private Const QueryTitlePrefix As String = "Interogare: "

    ''' <summary>The groups the owner named: prefix of the table name -> what the group shows. Any other prefix shows as it is. The groups are ordered by the name they show.</summary>
    Private Shared ReadOnly GroupNames As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
        {"AD_", "ADECHIT"}, {"EF_", "EFACTURA"}, {"FX_", "FOREXE"}, {"TZ_", "AVACONT"}, {"VC_", "VERCON"}}
    Private Const BlobText As String = "<BLOB>"
    Private Const IdWidth As Integer = 100
    Private Const CellPad As Integer = 16
    ''' <summary>Extra room of a date / date-time column: the cell padding inside the grid cuts a text measured to the pixel.</summary>
    Private Const DatePad As Integer = 40
    Private Const HeaderIcons As Integer = 44      ' header text + the filter button and the sort mark
    Private Const HeaderPad As Integer = 14
    Private Const MinColumnWidth As Integer = 40
    Private Const FitSampleRows As Integer = 300

    ''' <summary>How a column is shown: decided by its MariaDB type (and its name, for the ids).</summary>
    Private Enum CellKind
        Plain
        LongText
        Whole
        Fixed
        Floating
        DateOnly
        Stamp
        TimeOnly
        Flag
        Blob
    End Enum

    ''' <summary>A blob cell: only its length is known on this side.</summary>
    Private NotInheritable Class BlobMarker
        Public ReadOnly Length As Long
        Public Sub New(k_length As Long)
            Length = k_length
        End Sub
    End Class

    ''' <summary>A long text cut by the server: the first characters, the rest is fetched on double click.</summary>
    Private NotInheritable Class CutText
        Public ReadOnly Preview As String
        Public Sub New(k_preview As String)
            Preview = k_preview
        End Sub
    End Class

    ''' <summary>What a grid row remembers: the typed value of every cell, where the row is in the table, and how to find it again on the server.</summary>
    Private NotInheritable Class RowTag
        Public Raw As Object()
        Public Cells As List(Of JsonElement)
        Public Offset As Integer
        Public Key As String = String.Empty
    End Class

    ''' <summary>
    ''' A table a cell of the open table leads to: <see cref="Outgoing"/> = this table's foreign key points at it, else it points at this table.
    ''' <see cref="Columns"/> are the columns of the TARGET table and <see cref="SourceColumns"/> the columns of the open table they pair with, in
    ''' key order: more than one means a composite key.
    ''' </summary>
    Private NotInheritable Class LinkTarget
        Public ReadOnly Table As String
        Public ReadOnly Columns As List(Of String)
        Public ReadOnly SourceColumns As List(Of String)
        Public ReadOnly Outgoing As Boolean
        Public Sub New(k_table As String, k_columns As List(Of String), k_sourceColumns As List(Of String), k_outgoing As Boolean)
            Table = k_table
            Columns = k_columns
            SourceColumns = k_sourceColumns
            Outgoing = k_outgoing
        End Sub

        Public ReadOnly Property ColumnsText As String
            Get
                Return If(Columns.Count = 1, Columns(0), "(" & String.Join(", ", Columns) & ")")
            End Get
        End Property

        Public ReadOnly Property Caption As String
            Get
                Return If(Outgoing, "→ ", "← ") & Table & "." & ColumnsText
            End Get
        End Property
    End Class

    ''' <summary>One condition of a filtered view: the column of the table equals a value (the JSON the server compares with, and the text the operator reads).</summary>
    Private NotInheritable Class FilterPart
        Public ReadOnly Column As String
        Public ReadOnly Json As String
        Public ReadOnly Text As String
        Public Sub New(k_column As String, k_json As String, k_text As String)
            Column = k_column
            Json = k_json
            Text = k_text
        End Sub

        Public ReadOnly Property Caption As String
            Get
                Return Column & " = " & If(Text.Length > 40, Text.Substring(0, 40) & "...", Text)
            End Get
        End Property
    End Class

    ''' <summary>One view of the rows grid: a table, optionally only the rows where every <see cref="Filters"/> column equals its value. The nodes form a tree (the history).</summary>
    Private NotInheritable Class NavNode
        Public ReadOnly Table As String
        Public ReadOnly Filters As New List(Of FilterPart)()
        Public ReadOnly Parent As NavNode
        Public ReadOnly Children As New List(Of NavNode)()

        Public Sub New(k_table As String)
            Table = k_table
        End Sub

        Public Sub New(k_parent As NavNode, k_table As String, k_filters As IEnumerable(Of FilterPart))
            Parent = k_parent
            Table = k_table
            Filters.AddRange(k_filters)
        End Sub

        Public ReadOnly Property HasFilter As Boolean
            Get
                Return Filters.Count > 0
            End Get
        End Property

        ''' <summary>What identifies the view (the cache key and the way to find a step already taken).</summary>
        Public ReadOnly Property Key As String
            Get
                Return Table & "|" & String.Join(",", Filters.Select(Function(k_f) k_f.Column & "=" & k_f.Json))
            End Get
        End Property

        ''' <summary>The filter as the server takes it: a JSON object {column: value}.</summary>
        Public ReadOnly Property WhereJson As String
            Get
                Return WhereJsonOf(Filters)
            End Get
        End Property

        Public Shared Function WhereJsonOf(k_filters As IEnumerable(Of FilterPart)) As String
            Return "{" & String.Join(",", k_filters.Select(Function(k_f) JsonSerializer.Serialize(k_f.Column) & ":" & k_f.Json)) & "}"
        End Function

        ''' <summary>The filter short enough for a caption.</summary>
        Public ReadOnly Property FilterCaption As String
            Get
                Return String.Join(", ", Filters.Select(Function(k_f) k_f.Caption))
            End Get
        End Property

        ''' <summary>The filter whole.</summary>
        Public ReadOnly Property FilterFull As String
            Get
                Return String.Join(", ", Filters.Select(Function(k_f) k_f.Column & " = " & k_f.Text))
            End Get
        End Property

        Public ReadOnly Property Caption As String
            Get
                Return If(HasFilter, Table & "  [" & FilterCaption & "]", Table)
            End Get
        End Property
    End Class

    Private _context As AdminContext
    Private _loadedDb As String = String.Empty
    Private _tableName As String = String.Empty
    Private _current As DbTableResult
    Private _kinds As New List(Of CellKind)()
    Private _formats As New List(Of String)()
    Private _maxWidths As New List(Of Integer)()
    Private ReadOnly _colIndex As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
    Private ReadOnly _fkByColumn As New Dictionary(Of String, List(Of DbForeignKey))(StringComparer.Ordinal)
    Private ReadOnly _refByColumn As New Dictionary(Of String, List(Of DbReference))(StringComparer.Ordinal)
    Private ReadOnly _indexByColumn As New Dictionary(Of String, List(Of DbIndexInfo))(StringComparer.Ordinal)
    Private _pkIndexes As New List(Of Integer)()
    Private _loadedRows As Integer
    Private _hasMore As Boolean
    Private _request As Integer
    Private _columnShown As String = String.Empty
    Private _cellShown As String = String.Empty
    Private _ratiosApplied As Boolean
    Private _filling As Boolean
    ''' <summary>The view the rows grid shows now (a node of the history tree); Nothing while no table is open.</summary>
    Private _node As NavNode
    ''' <summary>The list of tables of every database read so far, by database: not asked again until «Reîncarcă».</summary>
    Private ReadOnly _tablesCache As New Dictionary(Of String, DbTablesResult)(StringComparer.OrdinalIgnoreCase)
    ''' <summary>Every view read so far (with the pages «Încarcă încă» added), by database + <see cref="NavNode.Key"/>: not asked again until «Reîncarcă».</summary>
    Private ReadOnly _viewCache As New Dictionary(Of String, DbTableResult)(StringComparer.Ordinal)
    ''' <summary>
    ''' The columns of the open table whose cells lead to other tables, each with True when the column is a foreign key of this table (it leads OUT, to the
    ''' tables it points at; the accent colour) and False when it is only pointed at by other tables (it leads IN, to the tables that refer to it; the success
    ''' colour). A column that is both counts as a foreign key. Drawn as links, underlined, with the hand over them; a click opens the popup of the linked tables.
    ''' </summary>
    Private ReadOnly _linkColumns As New Dictionary(Of String, Boolean)(StringComparer.Ordinal)
    ''' <summary>The underlined twin of each font a link cell was drawn with (made once per font; disposed with the view).</summary>
    Private ReadOnly _linkFonts As New Dictionary(Of Font, Font)()
    ''' <summary>The pick lists of composite-key jumps already read (database + table + column + what is chosen so far + the text typed): not asked again until «Reîncarcă».</summary>
    Private ReadOnly _distinctCache As New Dictionary(Of String, DbDistinctResult)(StringComparer.Ordinal)
    Private ReadOnly _treeItems As New Dictionary(Of String, AdvancedTreeControl.TreeItem)(StringComparer.Ordinal)
    ''' <summary>The rows grid shows the result of a catalog query (not a table): no paging, no links, no history; «Reîncarcă» runs the query again.</summary>
    Private _queryMode As Boolean
    ''' <summary>The query on screen and the values it ran with (for «Reîncarcă»); Nothing while the grid shows a table.</summary>
    Private _lastQuery As QueryDef
    Private _lastQueryValues As IDictionary(Of String, Object)
    ''' <summary>The facts about the query on screen, for the upper fact grid (what ran, with what, how long).</summary>
    Private _queryFacts As New List(Of KeyValuePair(Of String, String))()
    ''' <summary>What the operator typed in the parameter window of each query (query id -> parameter name -> text): offered again the next time, kept until the program closes.</summary>
    Private ReadOnly _queryTexts As New Dictionary(Of String, Dictionary(Of String, String))(StringComparer.Ordinal)
    ''' <summary>What is wrong with each query file the tree shows as wrong (file id -> message), read when the tree was built.</summary>
    Private ReadOnly _queryProblems As New Dictionary(Of String, String)(StringComparer.Ordinal)
    ''' <summary>The columns the operator ticked «Fix» for each query (query id -> column names): kept until the program closes, applied every time that query runs.</summary>
    Private ReadOnly _queryLocks As New Dictionary(Of String, HashSet(Of String))(StringComparer.Ordinal)
    ''' <summary>The id of the query on screen (the key of <see cref="_queryLocks"/>); empty while the grid shows a table.</summary>
    Private _queryId As String = String.Empty
    ''' <summary>The sentence that ends the line above the grid when the query returned more rows than it shows.</summary>
    Private _queryCutNote As String = String.Empty

    Public Sub New()
        InitializeComponent()
        AddHandler Disposed, AddressOf OnViewDisposed
        ShowHint("Alegeți o bază din arborele din stânga, apoi un tabel.")
    End Sub

    Private Sub OnViewDisposed(k_sender As Object, e As EventArgs)
        For Each k_font As Font In _linkFonts.Values
            k_font.Dispose()
        Next
        _linkFonts.Clear()
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    ''' <summary>
    ''' The buttons of this page are plain flat buttons (no inner padding, square corners, no body of their own): ThemeManager gives them its
    ''' generic button look first (the children are themed before this container), this puts the transparent one back over it.
    ''' </summary>
    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            For Each k_button As Button In New Button() {btnReload, btnBack, btnForward}
                ButtonStyles.ApplyTrans(k_button, k_scheme)
                k_button.Padding = Padding.Empty
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.ApplyTheme", ex)
        End Try
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        ApplyRatios()
        Await EnsureTreeAsync(False)
    End Function

    Public Async Sub SelectionChanged() Implements IAdminView.SelectionChanged
        Try
            Await EnsureTreeAsync(False)
        Catch ex As Exception
            ' UI boundary (async void): UiRun.Guard already showed and logged what failed; this is the last wall.
            GlobalErrorLog.Write("DbBrowserView.SelectionChanged", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Panels
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' The starting proportions, once the page has its real size: the tree ~270 px; under the rows ~20% of the height; in that strip the
    ''' cell box takes half of the width and the two fact grids a quarter each. Afterwards the operator's mouse decides.
    ''' </summary>
    Private Sub ApplyRatios()
        Try
            If _ratiosApplied Then Return
            PerformLayout()
            If splitMain.Width < 400 OrElse splitRight.Height < 200 Then Return
            SetDistance(splitMain, CInt(270 * DeviceDpi / 96.0))
            SetDistance(splitRight, CInt(splitRight.Height * 0.8))
            SetDistance(splitBottom, splitBottom.Width \ 2)
            SetDistance(splitInfo, splitInfo.Width \ 2)
            _ratiosApplied = True
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.ApplyRatios", ex)
        End Try
    End Sub

    Private Shared Sub SetDistance(k_split As SplitContainer, k_distance As Integer)
        Dim k_total As Integer = If(k_split.Orientation = Orientation.Vertical, k_split.Width, k_split.Height)
        Dim k_max As Integer = k_total - k_split.Panel2MinSize - k_split.SplitterWidth
        k_split.SplitterDistance = Math.Max(k_split.Panel1MinSize, Math.Min(k_distance, k_max))
    End Sub

    ' ------------------------------------------------------------------
    ' The tables of the database
    ' ------------------------------------------------------------------

    Private Async Function EnsureTreeAsync(k_force As Boolean) As Task
        If _context Is Nothing Then Return
        If _context.IsAll Then
            ClearAll("Alegeți o singură bază din arborele din stânga (nu «" & AdminContext.AllDatabasesCaption & "»).")
            Return
        End If
        Dim k_db As String = _context.SelectedDb
        If Not k_force AndAlso String.Equals(k_db, _loadedDb, StringComparison.OrdinalIgnoreCase) Then Return
        Dim k_result As DbTablesResult = Nothing
        Dim k_cached As Boolean = Not k_force AndAlso _tablesCache.TryGetValue(k_db, k_result)
        If Not k_cached Then
            Await UiRun.Guard("DbBrowserView.EnsureTreeAsync", _context,
                Async Function()
                    _context.Say("Se citesc tabelele bazei " & k_db & "...")
                    k_result = Await _context.Client.CallAsync(Of DbTablesResult)("db-tables", "--db", k_db)
                End Function)
            If k_result IsNot Nothing Then _tablesCache(k_db) = k_result
        End If
        If k_result Is Nothing OrElse Not String.Equals(k_db, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then Return
        TableNavLog.Write("TABLES", "database " & k_db & ": " & k_result.Tables.Count.ToString(CultureInfo.InvariantCulture) & " tables, source=" & If(k_cached, "MEMORY", "SERVER") & If(k_force, " (forced)", String.Empty))
        Dim k_keep As String = If(String.Equals(k_db, _loadedDb, StringComparison.OrdinalIgnoreCase), _tableName, String.Empty)
        If Not String.Equals(k_db, _loadedDb, StringComparison.OrdinalIgnoreCase) Then ClearGrids()
        _loadedDb = k_result.Db
        BuildTree(k_result, k_keep)
        _context.Say("Gata: " & k_result.Tables.Count.ToString(CultureInfo.CurrentCulture) & " tabele în " & k_result.Db & If(k_cached, " (din memorie).", "."))
        If k_keep = String.Empty Then ShowHint("Alegeți un tabel din arbore.")
    End Function

    ''' <summary>Forgets everything read from one database (its list of tables and every view of its tables): the next use asks the server again.</summary>
    Private Sub ClearCache(k_db As String)
        TableNavLog.Write("CACHE", "cleared for " & k_db & ": list of tables " & If(_tablesCache.ContainsKey(k_db), "1", "0") & ", views " &
                          _viewCache.Keys.Where(Function(k_k) k_k.StartsWith(k_db & "|", StringComparison.OrdinalIgnoreCase)).Count().ToString(CultureInfo.InvariantCulture) & ", pick lists " &
                          _distinctCache.Keys.Where(Function(k_k) k_k.StartsWith(k_db & "|", StringComparison.OrdinalIgnoreCase)).Count().ToString(CultureInfo.InvariantCulture))
        _tablesCache.Remove(k_db)
        Dim k_prefix As String = k_db & "|"
        For Each k_key As String In _viewCache.Keys.Where(Function(k_k) k_k.StartsWith(k_prefix, StringComparison.OrdinalIgnoreCase)).ToList()
            _viewCache.Remove(k_key)
        Next
        For Each k_key As String In _distinctCache.Keys.Where(Function(k_k) k_k.StartsWith(k_prefix, StringComparison.OrdinalIgnoreCase)).ToList()
            _distinctCache.Remove(k_key)
        Next
    End Sub

    Private Sub BuildTree(k_result As DbTablesResult, k_keep As String)
        treeTables.Clear()
        _treeItems.Clear()
        treeTables.RootExpander = True   ' no single root: every group is a root that must open
        Dim k_select As AdvancedTreeControl.TreeItem = Nothing
        AddQueryGroup()
        Dim k_groups As IEnumerable(Of IGrouping(Of String, DbTableSummary)) = k_result.Tables.GroupBy(Function(k_t) GroupOf(k_t.Name), StringComparer.Ordinal).
            OrderBy(Function(k_g) If(String.Equals(k_g.Key, NoGroup, StringComparison.Ordinal), 0, 1)).ThenBy(Function(k_g) GroupCaption(k_g.Key), StringComparer.OrdinalIgnoreCase)
        For Each k_group As IGrouping(Of String, DbTableSummary) In k_groups
            Dim k_holdsKept As Boolean = k_group.Any(Function(k_t) String.Equals(k_t.Name, k_keep, StringComparison.Ordinal))
            Dim k_root As AdvancedTreeControl.TreeItem = treeTables.AddItem("g:" & k_group.Key, GroupCaption(k_group.Key) & "~~~" & k_group.Count().ToString(CultureInfo.CurrentCulture),
                                                                          Nothing, Nothing, Nothing, Nothing, GroupTagPrefix & k_group.Key, k_holdsKept)
            For Each k_table As DbTableSummary In k_group
                Dim k_isView As Boolean = String.Equals(k_table.Kind, "VIEW", StringComparison.OrdinalIgnoreCase)
                Dim k_right As String = If(k_isView, "vizualizare", "~" & k_table.RowsEst.ToString("N0", CultureInfo.CurrentCulture))
                Dim k_item As AdvancedTreeControl.TreeItem = treeTables.AddItem("t:" & k_table.Name, ShortName(k_table.Name, k_group.Key) & "~~~" & k_right, k_root,
                                                                               Nothing, Nothing, Nothing, TablePrefix & k_table.Name)
                k_item.Tooltip = DescribeTable(k_table)
                _treeItems(k_table.Name) = k_item
                If String.Equals(k_table.Name, k_keep, StringComparison.Ordinal) Then k_select = k_item
            Next
        Next
        If k_select IsNot Nothing Then treeTables.SelectAndReveal(k_select)
        If _queryMode AndAlso _lastQuery IsNot Nothing Then RevealInTree(QueryPrefix & _lastQuery.Id)
    End Sub

    ''' <summary>
    ''' The group «_Interogari», first in the tree: one row per query FILE of the queries folder, read now (see <see cref="QueryCatalog"/>). A file that cannot be
    ''' read is a row marked «(greșit)»; a missing folder is one row saying where it was looked for. A click on a good row opens its parameter window
    ''' (<see cref="RunQueryFromTreeAsync"/>).
    ''' </summary>
    Private Sub AddQueryGroup()
        Dim k_folder As String = QueriesFolder()
        Dim k_catalog As QueryCatalogResult = QueryCatalog.Load(k_folder)
        _queryProblems.Clear()
        Dim k_root As AdvancedTreeControl.TreeItem = treeTables.AddItem("g:" & QueryGroup, QueryGroup & "~~~" & k_catalog.Queries.Count.ToString(CultureInfo.CurrentCulture),
                                                                      Nothing, Nothing, Nothing, Nothing, GroupTagPrefix & QueryGroup, _queryMode)
        k_root.Tooltip = "Interogări scrise de proprietar, care rulează pe baza aleasă. Un clic pe una deschide fereastra cu parametrii ei." & vbLf &
                         "Fișierele sunt în " & If(k_folder = String.Empty, "(folderul nu a fost găsit)", k_folder) & "; «Reîncarcă» le citește din nou."
        If Not k_catalog.FolderFound Then
            AddQueryProblem(k_root, "_folder", "Folderul interogărilor nu a fost găsit", "Folderul interogărilor nu există" & If(k_folder = String.Empty, ": nu am găsit KBot.sln deasupra programului și «QueriesFolder» din admin_settings.json e gol.", ": " & k_folder))
        End If
        For Each k_query As QueryDef In k_catalog.Queries
            Dim k_item As AdvancedTreeControl.TreeItem = treeTables.AddItem("q:" & k_query.Id, k_query.Title & "~~~interogare", k_root, Nothing, Nothing, Nothing, QueryPrefix & k_query.Id)
            k_item.Tooltip = k_query.Title & vbLf & k_query.Description
            _treeItems(QueryPrefix & k_query.Id) = k_item
        Next
        For Each k_problem As KeyValuePair(Of String, String) In k_catalog.Problems
            AddQueryProblem(k_root, k_problem.Key, k_problem.Key & " (greșit)", k_problem.Value)
        Next
    End Sub

    Private Sub AddQueryProblem(k_root As AdvancedTreeControl.TreeItem, k_id As String, k_caption As String, k_message As String)
        _queryProblems(k_id) = k_message
        Dim k_item As AdvancedTreeControl.TreeItem = treeTables.AddItem("qe:" & k_id, k_caption & "~~~citește-mă", k_root, Nothing, Nothing, Nothing, QueryProblemPrefix & k_id)
        k_item.Tooltip = k_message
    End Sub

    Private Function QueriesFolder() As String
        Return If(_context Is Nothing, String.Empty, _context.Client.Settings.EffectiveQueriesFolder())
    End Function

    ''' <summary>The name a table shows under its group: without the prefix the group already says (AD_Prezenta -> Prezenta). The tag, the tooltip and every call to the server keep the full name.</summary>
    Private Shared Function ShortName(k_name As String, k_groupKey As String) As String
        Return If(Not String.Equals(k_groupKey, NoGroup, StringComparison.Ordinal) AndAlso k_name.Length > k_groupKey.Length, k_name.Substring(k_groupKey.Length), k_name)
    End Function

    Private Shared Function GroupCaption(k_key As String) As String
        Dim k_name As String = Nothing
        Return If(GroupNames.TryGetValue(k_key, k_name), k_name, k_key)
    End Function

    ''' <summary>The group of a table: its first three characters when they are of the form «AD_» / «VC_» / «FX_» (two letters or digits and an underscore), else the group «_Fara Grupare».</summary>
    Private Shared Function GroupOf(k_name As String) As String
        If k_name.Length > 3 AndAlso k_name(2) = "_"c AndAlso Char.IsLetterOrDigit(k_name(0)) AndAlso Char.IsLetterOrDigit(k_name(1)) Then
            Return k_name.Substring(0, 3).ToUpperInvariant()
        End If
        Return NoGroup
    End Function

    Private Shared Function DescribeTable(k_table As DbTableSummary) As String
        Dim k_text As New StringBuilder()
        k_text.AppendLine(k_table.Name)
        k_text.Append("Coloane: ").Append(k_table.ColumnsN.ToString(CultureInfo.CurrentCulture))
        k_text.Append(",  rânduri (estimat): ").AppendLine(k_table.RowsEst.ToString("N0", CultureInfo.CurrentCulture))
        k_text.Append("Date: ").Append(UiRun.FormatBytes(k_table.DataBytes)).Append(",  indexuri: ").Append(UiRun.FormatBytes(k_table.IndexBytes))
        If Not String.IsNullOrWhiteSpace(k_table.Engine) Then k_text.AppendLine().Append("Motor: ").Append(k_table.Engine)
        If Not String.IsNullOrWhiteSpace(k_table.Comment) Then k_text.AppendLine().Append(k_table.Comment)
        Return k_text.ToString()
    End Function

    Private Sub TreeTables_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles treeTables.NodeMouseUp
        ' A query opens a window: only a left click does it (a table opens on any button, as before).
        If IsQueryNode(k_node) AndAlso e.Button <> MouseButtons.Left Then Return
        OpenFromNode(k_node)
    End Sub

    Private Sub TreeTables_KeyUp(k_sender As Object, e As KeyEventArgs) Handles treeTables.KeyUp
        ' Walking down the tree with the arrows must not open a window on every query it passes: a query opens on Enter only.
        If IsQueryNode(treeTables.SelectedNode) AndAlso e.KeyCode <> Keys.Enter Then Return
        OpenFromNode(treeTables.SelectedNode)
    End Sub

    Private Shared Function IsQueryNode(k_node As AdvancedTreeControl.TreeItem) As Boolean
        If k_node Is Nothing Then Return False
        Dim k_tag As String = Convert.ToString(k_node.Tag, CultureInfo.InvariantCulture)
        Return k_tag.StartsWith(QueryPrefix, StringComparison.Ordinal) OrElse k_tag.StartsWith(QueryProblemPrefix, StringComparison.Ordinal)
    End Function

    Private Async Sub OpenFromNode(k_node As AdvancedTreeControl.TreeItem)
        Try
            If k_node Is Nothing Then Return
            Dim k_tag As String = Convert.ToString(k_node.Tag, CultureInfo.InvariantCulture)
            If k_tag.StartsWith(QueryPrefix, StringComparison.Ordinal) Then
                Await RunQueryFromTreeAsync(k_tag.Substring(QueryPrefix.Length))
                Return
            End If
            If k_tag.StartsWith(QueryProblemPrefix, StringComparison.Ordinal) Then
                Dim k_message As String = Nothing
                _queryProblems.TryGetValue(k_tag.Substring(QueryProblemPrefix.Length), k_message)
                KBotMessage.Show(FindForm(), If(k_message, "Fișierul nu poate fi citit."), "Interogare greșită", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If Not k_tag.StartsWith(TablePrefix, StringComparison.Ordinal) Then Return
            Dim k_name As String = k_tag.Substring(TablePrefix.Length)
            ' The table is already open as the start of a history (nothing filtered): nothing to do. Anywhere else a click on the tree starts a new history.
            If _node IsNot Nothing AndAlso _node.Parent Is Nothing AndAlso Not _node.HasFilter AndAlso String.Equals(k_name, _tableName, StringComparison.Ordinal) Then
                TableNavLog.Write("TREE", "click on " & k_name & ": already the start of the history, nothing done")
                Return
            End If
            TableNavLog.Write("TREE", "click on " & k_name & ": a NEW history starts (the one of " & If(_node Is Nothing, "nothing", _node.Caption) & " is dropped)")
            Await OpenNodeAsync(New NavNode(k_name))
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.OpenFromNode", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' One view of one table (from memory when it was read before)
    ' ------------------------------------------------------------------

    Private Function CacheKey(k_node As NavNode) As String
        Return _loadedDb & "|" & k_node.Key
    End Function

    ''' <summary>The arguments of the server call that reads the rows of a view: its table and, when filtered, the column and the value.</summary>
    Private Function ViewArgs(k_node As NavNode, k_offset As Integer?) As List(Of String)
        Dim k_args As New List(Of String) From {"--db", _loadedDb, "--table", k_node.Table}
        If k_offset.HasValue Then k_args.AddRange({"--offset", k_offset.Value.ToString(CultureInfo.InvariantCulture)})
        k_args.AddRange({"--limit", PageSize.ToString(CultureInfo.InvariantCulture)})
        If k_node.HasFilter Then k_args.AddRange({"--where-json", k_node.WhereJson})
        Return k_args
    End Function

    ''' <summary>
    ''' Shows a view in the rows grid: from memory when it was read before, else from the server (and then kept). Only when it is shown
    ''' does it become the current node, and a step taken from the current node joins the history (so a failed read leaves the history as it was).
    ''' </summary>
    Private Async Function OpenNodeAsync(k_node As NavNode) As Task
        _request += 1
        Dim k_id As Integer = _request
        Dim k_key As String = CacheKey(k_node)
        Dim k_result As DbTableResult = Nothing
        Dim k_cached As Boolean = _viewCache.TryGetValue(k_key, k_result)
        TableNavLog.Write("OPEN", "request #" & k_id.ToString(CultureInfo.InvariantCulture) & ": " & k_node.Caption & "  key=" & k_node.Key & "  parent=" &
                          If(k_node.Parent Is Nothing, "-", k_node.Parent.Caption) & "  source=" & If(k_cached, "MEMORY", "SERVER"))
        If Not k_cached Then
            TableNavLog.Write("OPEN", "db-table " & String.Join(" ", ViewArgs(k_node, Nothing)))
            Await UiRun.Guard("DbBrowserView.OpenNodeAsync", _context,
                Async Function()
                    _context.Say("Se citește " & k_node.Caption & "...")
                    k_result = Await _context.Client.CallAsync(Of DbTableResult)("db-table", ViewArgs(k_node, Nothing).ToArray())
                End Function)
            If k_result Is Nothing OrElse k_id <> _request Then
                TableNavLog.Write("OPEN", "request #" & k_id.ToString(CultureInfo.InvariantCulture) & " NOT shown: " &
                                  If(k_result Is Nothing, "the server call failed (the error was shown)", "a newer request (#" & _request.ToString(CultureInfo.InvariantCulture) & ") replaced it"))
                Return
            End If
            _viewCache(k_key) = k_result
            TableNavLog.Write("OPEN", "server answered: " & k_result.Page.Rows.Count.ToString(CultureInfo.InvariantCulture) & " rows, hasMore=" & k_result.Page.HasMore.ToString() & ", kept in memory")
        End If
        Dim k_isNew As Boolean = k_node.Parent IsNot Nothing AndAlso Not k_node.Parent.Children.Contains(k_node)
        If k_isNew Then k_node.Parent.Children.Add(k_node)
        _node = k_node
        _queryMode = False
        _queryId = String.Empty
        _queryCutNote = String.Empty
        _lastQuery = Nothing
        _lastQueryValues = Nothing
        SyncSidePanel()
        ShowTable(k_result)
        UpdateNavButtons()
        RevealInTree(k_node.Table)
        TableNavLog.Write("OPEN", "shown: " & k_node.Caption & ", " & _loadedRows.ToString(CultureInfo.InvariantCulture) & " rows in the grid, " &
                          If(k_node.Parent Is Nothing, "start of a history", If(k_isNew, "NEW step joined the history under " & k_node.Parent.Caption, "step already in the history, reused")))
        LogHistory("after open")
        _context.Say("Gata: " & k_node.Caption & ", " & _loadedRows.ToString("N0", CultureInfo.CurrentCulture) & " rânduri încărcate" & If(k_cached, " (din memorie).", "."))
    End Function

    Private Sub RevealInTree(k_table As String)
        Dim k_item As AdvancedTreeControl.TreeItem = Nothing
        If _treeItems.TryGetValue(k_table, k_item) Then treeTables.SelectAndReveal(k_item)
    End Sub

    ' ------------------------------------------------------------------
    ' The queries of the group «_Interogari» (catalog in KBotAdmin, server half admin_query.py)
    ' ------------------------------------------------------------------

    ''' <summary>The title a query result carries in the page (and as the name of its view): never the name of a table.</summary>
    Private Shared Function QueryCaption(k_query As QueryDef) As String
        Return QueryTitlePrefix & k_query.Title
    End Function

    ''' <summary>A click on a query row: the window with its parameters, then the run. The values typed last time are offered again.</summary>
    Private Async Function RunQueryFromTreeAsync(k_id As String) As Task
        If _context Is Nothing OrElse String.IsNullOrEmpty(_loadedDb) Then Return
        ' The file is read now: an edit made since the tree was built is used (and a header broken since then is told, not run).
        Dim k_query As QueryDef
        Try
            k_query = QueryCatalog.Find(QueriesFolder(), k_id)
        Catch ex As ArgumentException
            KBotMessage.Show(FindForm(), ex.Message, "Interogare greșită", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End Try
        If k_query Is Nothing Then
            KBotMessage.Show(FindForm(), "Fișierul interogării «" & k_id & "» nu mai există în folder. Apăsați «Reîncarcă».", "Interogare", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim k_db As String = _loadedDb
        ' Lets the click that chose the row finish before a modal window opens over it.
        Await Task.Yield()
        Dim k_previous As Dictionary(Of String, String) = Nothing
        _queryTexts.TryGetValue(k_query.Id, k_previous)
        Dim k_values As Dictionary(Of String, Object) = Nothing
        Using k_form As New QueryParamsForm(k_query, k_db, k_previous)
            If k_form.ShowDialog(FindForm()) <> DialogResult.OK Then
                TableNavLog.Write("QUERY", k_query.Id & ": parameter window closed WITHOUT running")
                Return
            End If
            k_values = New Dictionary(Of String, Object)(k_form.Values, StringComparer.Ordinal)
            _queryTexts(k_query.Id) = k_form.TypedTexts()
        End Using
        If Not String.Equals(k_db, _loadedDb, StringComparison.OrdinalIgnoreCase) Then
            TableNavLog.Write("QUERY", k_query.Id & ": NOT run, the database changed while the window was open")
            Return
        End If
        Await ExecuteQueryAsync(k_query, k_values)
    End Function

    ''' <summary>Runs a query on the database of the page and shows its rows in the grid, like a table (the facts above the grid say what ran and how long it took).</summary>
    Private Async Function ExecuteQueryAsync(k_query As QueryDef, k_values As IDictionary(Of String, Object)) As Task
        _request += 1
        Dim k_id As Integer = _request
        Dim k_db As String = _loadedDb
        Dim k_result As DbTableResult = Nothing
        Dim k_args As String() = k_query.BuildArgs(k_db, k_values, QueryLimit)
        TableNavLog.Write("QUERY", "request #" & k_id.ToString(CultureInfo.InvariantCulture) & ": " & k_query.Id & " on " & k_db & " with " &
                          String.Join(", ", k_values.Select(Function(k_p) k_p.Key & "=" & Convert.ToString(k_p.Value, CultureInfo.InvariantCulture))))
        Await UiRun.Guard("DbBrowserView.ExecuteQueryAsync", _context,
            Async Function()
                _context.Say("Se rulează interogarea «" & k_query.Title & "» pe " & k_db & "...")
                k_result = Await _context.Client.CallAsync(Of DbTableResult)("query-run", k_args)
            End Function)
        If k_result Is Nothing OrElse k_id <> _request OrElse Not String.Equals(k_db, _loadedDb, StringComparison.OrdinalIgnoreCase) Then
            TableNavLog.Write("QUERY", "request #" & k_id.ToString(CultureInfo.InvariantCulture) & " NOT shown: " &
                              If(k_result Is Nothing, "the server call failed (the error was shown)", "a newer request or another database replaced it"))
            Return
        End If
        Dim k_cut As Boolean = k_result.Page.HasMore
        k_result.Page.HasMore = False      ' a query has no «Încarcă încă»: the rest is not reachable, the page says it was cut
        k_result.Info.Name = QueryCaption(k_query)
        k_result.Info.Kind = "QUERY"
        k_result.Info.RowsEst = k_result.Page.Rows.Count
        _queryFacts = New List(Of KeyValuePair(Of String, String)) From {
            New KeyValuePair(Of String, String)("Interogare", k_query.Title),
            New KeyValuePair(Of String, String)("Baza", k_db),
            New KeyValuePair(Of String, String)("Ce face", k_query.Description)}
        For Each k_param As QueryParamDef In k_query.Params
            Dim k_value As Object = Nothing
            k_values.TryGetValue(k_param.Name, k_value)
            _queryFacts.Add(New KeyValuePair(Of String, String)(k_param.Label, If(k_value Is Nothing, "(gol)", Convert.ToString(k_value, CultureInfo.CurrentCulture))))
        Next
        _queryFacts.Add(New KeyValuePair(Of String, String)("Rânduri", k_result.Page.Rows.Count.ToString("N0", CultureInfo.CurrentCulture) &
                        If(k_cut, " (primele " & QueryLimit.ToString("N0", CultureInfo.CurrentCulture) & "; sunt mai multe)", String.Empty)))
        _queryFacts.Add(New KeyValuePair(Of String, String)("Durată pe server", k_result.ElapsedMs.ToString("N0", CultureInfo.CurrentCulture) & " ms"))
        _node = New NavNode(QueryCaption(k_query))
        _queryMode = True
        _queryId = k_query.Id
        _lastQuery = k_query
        _lastQueryValues = k_values
        _queryCutNote = If(k_cut, " Rezultatul a fost tăiat la primele " & QueryLimit.ToString("N0", CultureInfo.CurrentCulture) & " de rânduri: restrângeți-l cu parametrii.", String.Empty)
        FillQueryColumns(k_result)
        ShowTable(k_result)
        UpdateNavButtons()
        RevealInTree(QueryPrefix & k_query.Id)
        TableNavLog.Write("QUERY", "shown: " & k_query.Id & ", " & _loadedRows.ToString(CultureInfo.InvariantCulture) & " rows" & If(k_cut, " (cut at the limit)", String.Empty) & ", " & k_result.ElapsedMs.ToString(CultureInfo.InvariantCulture) & " ms")
        _context.Say("Gata: " & k_query.Title & ", " & _loadedRows.ToString("N0", CultureInfo.CurrentCulture) & " rânduri" & If(k_cut, " (tăiat la limită)", String.Empty) & ".")
    End Function

    ' ------------------------------------------------------------------
    ' The log of the walk (TableNavLog)
    ' ------------------------------------------------------------------

    Private Shared Function Names(k_views As IEnumerable(Of NavNode)) As String
        Return "[" & String.Join(" | ", k_views.Select(Function(k_v) k_v.Caption)) & "]"
    End Function

    ''' <summary>The whole history tree, the current view marked with «&gt;», and what ◀ ▶ offer from it: written after every step, so a wrong list is seen at once.</summary>
    Private Sub LogHistory(k_why As String)
        If Not TableNavLog.Enabled Then Return
        If _node Is Nothing Then
            TableNavLog.Write("HISTORY", k_why & ": no view open")
            Return
        End If
        Dim k_root As NavNode = _node
        While k_root.Parent IsNot Nothing
            k_root = k_root.Parent
        End While
        TableNavLog.Write("HISTORY", k_why & ": current=" & _node.Caption & " back=" & btnBack.Enabled.ToString() & " " & Names(BackList()) &
                          " forward=" & btnForward.Enabled.ToString() & " " & Names(ForwardList()))
        LogTree(k_root, 0)
    End Sub

    ''' <summary>The log was just switched on: writes where the page is now (the database, the open view, the history), so the file starts from a known state.</summary>
    Public Sub LogCurrentState()
        TableNavLog.Write("STATE", "database=" & If(String.IsNullOrEmpty(_loadedDb), "none", _loadedDb) & " memory: " & _tablesCache.Count.ToString(CultureInfo.InvariantCulture) &
                          " lists of tables, " & _viewCache.Count.ToString(CultureInfo.InvariantCulture) & " views, " & _distinctCache.Count.ToString(CultureInfo.InvariantCulture) & " pick lists")
        LogHistory("state when the log was switched on")
    End Sub

    Private Sub LogTree(k_at As NavNode, k_depth As Integer)
        TableNavLog.Write("HISTORY", New String(" "c, 2 * k_depth + 2) & If(ReferenceEquals(k_at, _node), "> ", "  ") & k_at.Caption)
        For Each k_child As NavNode In k_at.Children
            LogTree(k_child, k_depth + 1)
        Next
    End Sub

    Private Sub ShowTable(k_result As DbTableResult)
        _current = k_result
        _tableName = k_result.Info.Name
        _columnShown = String.Empty
        _cellShown = String.Empty
        BuildLookups()
        _filling = True
        gridData.BeginUpdate()
        Try
            gridData.ClearAllFilters()
            gridData.ApplySort(Nothing, KBotSortDirection.None)
            gridData.ClearRows()
            gridData.Columns.Clear()
            _loadedRows = 0
            BuildColumns()
            AppendRows(k_result.Page)
            FitColumns()
        Finally
            gridData.EndUpdate()
            _filling = False
        End Try
        ShowTableInfo()
        If k_result.Columns.Count > 0 Then ShowColumn(k_result.Columns(0).Name)
        edCell.TextBox.Text = String.Empty
    End Sub

    Private Sub BuildLookups()
        _colIndex.Clear()
        _fkByColumn.Clear()
        _refByColumn.Clear()
        _indexByColumn.Clear()
        For k_i As Integer = 0 To _current.Columns.Count - 1
            _colIndex(_current.Columns(k_i).Name) = k_i
        Next
        _pkIndexes = _current.PrimaryKey.Where(Function(k_c) _colIndex.ContainsKey(k_c)).Select(Function(k_c) _colIndex(k_c)).ToList()
        For Each k_fk As DbForeignKey In _current.ForeignKeys
            For Each k_col As String In k_fk.Columns
                AddTo(_fkByColumn, k_col, k_fk)
            Next
        Next
        For Each k_ref As DbReference In _current.ReferencedBy
            For Each k_col As String In k_ref.RefColumns
                AddTo(_refByColumn, k_col, k_ref)
            Next
        Next
        For Each k_index As DbIndexInfo In _current.Indexes
            For Each k_col As String In k_index.Columns
                AddTo(_indexByColumn, k_col, k_index)
            Next
        Next
    End Sub

    Private Shared Sub AddTo(Of T)(k_map As Dictionary(Of String, List(Of T)), k_key As String, k_item As T)
        Dim k_list As List(Of T) = Nothing
        If Not k_map.TryGetValue(k_key, k_list) Then
            k_list = New List(Of T)()
            k_map(k_key) = k_list
        End If
        If Not k_list.Contains(k_item) Then k_list.Add(k_item)
    End Sub

    ' ------------------------------------------------------------------
    ' Columns: width, format and kind come from the type
    ' ------------------------------------------------------------------

    ''' <summary>
    ''' The columns kept on the left when the rows scroll: for a table its primary key (in key order); for a query the columns the operator ticked «Fix» in the list on
    ''' the right (in the order of the query, as many as ticked: the locks add up).
    ''' </summary>
    Private Function FrozenIndexes() As List(Of Integer)
        If Not _queryMode Then Return _pkIndexes
        Dim k_locked As HashSet(Of String) = Nothing
        Dim k_list As New List(Of Integer)()
        If Not _queryLocks.TryGetValue(_queryId, k_locked) Then Return k_list
        For k_i As Integer = 0 To _current.Columns.Count - 1
            If k_locked.Contains(_current.Columns(k_i).Name) Then k_list.Add(k_i)
        Next
        Return k_list
    End Function

    ''' <summary>
    ''' The grid builds the columns in this order (indexes into <c>_current.Columns</c>): the frozen ones first (<see cref="FrozenIndexes"/>), then the others as the table
    ''' (or the query) has them. The frozen ones are the band that stays put when the operator scrolls to the right (<see cref="BuildColumns"/>). For the usual table, whose key is the
    ''' first column, nothing moves.
    ''' </summary>
    Private Function DisplayOrder() As List(Of Integer)
        Dim k_order As New List(Of Integer)(FrozenIndexes())
        For k_i As Integer = 0 To _current.Columns.Count - 1
            If Not k_order.Contains(k_i) Then k_order.Add(k_i)
        Next
        Return k_order
    End Function

    Private Sub BuildColumns()
        ' The three lists are indexed like _current.Columns, whatever order the grid columns are built in.
        _kinds = New List(Of CellKind)(Enumerable.Repeat(CellKind.Plain, _current.Columns.Count))
        _formats = New List(Of String)(Enumerable.Repeat(String.Empty, _current.Columns.Count))
        _maxWidths = New List(Of Integer)(Enumerable.Repeat(0, _current.Columns.Count))
        _linkColumns.Clear()
        For Each k_at As Integer In DisplayOrder()
            Dim k_col As DbColumnInfo = _current.Columns(k_at)
            Dim k_isId As Boolean = LooksLikeId(k_col)
            Dim k_kind As CellKind = KindOf(k_col)
            Dim k_format As String = FormatOf(k_kind, k_col, k_isId)
            ' The preset width is the MAXIMUM of the first display (FitColumns); the column itself has no MaxWidth, the mouse can widen it freely.
            Dim k_width As Integer = WidthOf(k_kind, k_col, k_isId, k_format)
            Dim k_column As KBotDataColumn = gridData.AddColumn(k_col.Name, k_col.Name,
                                                                If(k_kind = CellKind.Flag, KBotColumnType.CheckBox, KBotColumnType.Text), k_width)
            k_column.ReadOnly = True
            k_column.ValueType = ValueTypeOf(k_kind)
            If k_format <> String.Empty Then k_column.FormatString = k_format
            If k_kind = CellKind.Whole OrElse k_kind = CellKind.Fixed OrElse k_kind = CellKind.Floating Then k_column.TextAlign = ContentAlignment.MiddleRight
            ConfigureHeaderMenu(k_column, k_kind)
            _kinds(k_at) = k_kind
            _formats(k_at) = k_format
            _maxWidths(k_at) = k_width
            ' A column that leads to other tables is drawn as a link (blobs, long texts and tick boxes keep their own meaning on a click).
            If k_kind <> CellKind.Flag AndAlso k_kind <> CellKind.Blob AndAlso k_kind <> CellKind.LongText Then
                Dim k_links As List(Of LinkTarget) = LinksOf(k_col.Name)
                If k_links.Count > 0 Then _linkColumns(k_col.Name) = k_links.Any(Function(k_l) k_l.Outgoing)
            End If
        Next
        ' The primary key stays on screen when the grid scrolls to the right (set again by FitColumns once the widths are known). A table without a key freezes nothing.
        gridData.FrozenColumnCount = FrozenIndexes().Count
    End Sub

    ''' <summary>
    ''' What the header menu of a column offers, by type: text and dates (and enum / set) sort, filter and group; long texts, numbers and times
    ''' sort and filter; a tick box only sorts; a blob has no menu. A date or date-time groups on the day (the time never counts), the month or the year.
    ''' </summary>
    Private Shared Sub ConfigureHeaderMenu(k_column As KBotDataColumn, k_kind As CellKind)
        Select Case k_kind
            Case CellKind.Blob
                k_column.ShowColumnFilter = False
            Case CellKind.Flag
                k_column.ShowColumnFilter = True
                k_column.AllowFiltering = False
                k_column.AllowGrouping = False
            Case CellKind.Plain, CellKind.DateOnly, CellKind.Stamp
                k_column.ShowColumnFilter = True
            Case Else
                k_column.ShowColumnFilter = True
                k_column.AllowGrouping = False
        End Select
    End Sub

    ''' <summary>
    ''' The first display: each column is as wide as its header and its text need (the first rows are measured), never wider than its preset
    ''' maximum. Done once per opened table; afterwards the widths are the operator's. A header the maximum still cuts gets a tooltip with the full name.
    ''' </summary>
    Private Sub FitColumns()
        Using k_boldFont As New Font(gridData.Font, FontStyle.Bold)
            For k_i As Integer = 0 To _current.Columns.Count - 1
                Dim k_name As String = _current.Columns(k_i).Name
                Dim k_column As KBotDataColumn = gridData.Column(k_name)
                Dim k_header As Integer = TextWidth(k_name, k_boldFont) + If(k_column.ShowColumnFilter, HeaderIcons, HeaderPad)
                Dim k_content As Integer = 0
                If _kinds(k_i) <> CellKind.Flag Then
                    Dim k_pad As Integer = If(_kinds(k_i) = CellKind.DateOnly OrElse _kinds(k_i) = CellKind.Stamp, DatePad, CellPad)
                    For k_r As Integer = 0 To Math.Min(gridData.RowCount, FitSampleRows) - 1
                        Dim k_tag As RowTag = TryCast(gridData.Rows(k_r).Tag, RowTag)
                        If k_tag Is Nothing Then Continue For
                        Dim k_text As String = DisplayText(k_tag.Raw(k_i), k_i)
                        If k_text.Length > 80 Then k_text = k_text.Substring(0, 80)
                        k_content = Math.Max(k_content, TextWidth(k_text, gridData.Font) + k_pad)
                    Next
                End If
                Dim k_width As Integer = Math.Min(_maxWidths(k_i), Math.Max(MinColumnWidth, Math.Max(k_header, k_content)))
                k_column.Width = k_width
                If k_header > k_width Then k_column.HeaderTooltip = k_name & "  (" & _current.Columns(k_i).ColumnType & ")"
            Next
        End Using
        LimitFrozenBand()
    End Sub

    ''' <summary>
    ''' A key made of wide columns must not leave no room for the rest: the frozen band keeps the key columns only while together they take at most half of the grid
    ''' (widths are logical px, the grid is in device px). A key that is too wide freezes fewer of its columns, down to none.
    ''' </summary>
    Private Sub LimitFrozenBand()
        ' What the operator ticked is theirs to untick; only the key of a table is held back when it is too wide.
        If _queryMode Then Return
        Dim k_room As Double = gridData.ClientSize.Width * 96.0 / DeviceDpi / 2.0
        If k_room <= 0 Then Return
        Dim k_taken As Double = 0
        Dim k_count As Integer = 0
        For Each k_i As Integer In _pkIndexes
            k_taken += gridData.Column(_current.Columns(k_i).Name).Width
            If k_taken > k_room Then Exit For
            k_count += 1
        Next
        gridData.FrozenColumnCount = k_count
    End Sub

    ''' <summary>The text a cell shows (the grid formats a value the same way): a blob is «&lt;BLOB&gt;», a text is one line, a number or a date follows the column format.</summary>
    Private Function DisplayText(k_raw As Object, k_columnIndex As Integer) As String
        If k_raw Is Nothing Then Return String.Empty
        Dim k_shown As Object = DisplayValue(k_raw)
        Dim k_text As String = TryCast(k_shown, String)
        If k_text IsNot Nothing Then Return k_text
        Dim k_formattable As IFormattable = TryCast(k_shown, IFormattable)
        If k_formattable IsNot Nothing Then
            Dim k_format As String = _formats(k_columnIndex)
            Return k_formattable.ToString(If(k_format = String.Empty, Nothing, k_format), CultureInfo.CurrentCulture)
        End If
        Return Convert.ToString(k_shown, CultureInfo.CurrentCulture)
    End Function

    Private Function LooksLikeId(k_col As DbColumnInfo) As Boolean
        If KindOf(k_col) <> CellKind.Whole OrElse String.Equals(k_col.DataType, "year", StringComparison.Ordinal) Then Return False
        If String.Equals(k_col.ColKey, "PRI", StringComparison.Ordinal) OrElse _fkByColumn.ContainsKey(k_col.Name) Then Return True
        Dim k_name As String = k_col.Name
        Return k_name.StartsWith("Id", StringComparison.OrdinalIgnoreCase) OrElse k_name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) OrElse
               k_name.EndsWith("_id", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function KindOf(k_col As DbColumnInfo) As CellKind
        Select Case k_col.DataType
            Case "tinyint"
                Return If(k_col.ColumnType.StartsWith("tinyint(1)", StringComparison.OrdinalIgnoreCase), CellKind.Flag, CellKind.Whole)
            Case "bit"
                Return If(k_col.NumPrec.GetValueOrDefault(0) = 1, CellKind.Flag, CellKind.Whole)
            Case "smallint", "mediumint", "int", "integer", "bigint", "year"
                Return CellKind.Whole
            Case "decimal", "numeric"
                Return CellKind.Fixed
            Case "float", "double", "real"
                Return CellKind.Floating
            Case "date"
                Return CellKind.DateOnly
            Case "datetime", "timestamp"
                Return CellKind.Stamp
            Case "time"
                Return CellKind.TimeOnly
            Case "tinyblob", "blob", "mediumblob", "longblob", "binary", "varbinary"
                Return CellKind.Blob
            Case "text", "mediumtext", "longtext"
                Return CellKind.LongText
            Case Else
                Return CellKind.Plain
        End Select
    End Function

    Private Shared Function ValueTypeOf(k_kind As CellKind) As KBotValueType
        Select Case k_kind
            Case CellKind.Whole, CellKind.Fixed, CellKind.Floating
                Return KBotValueType.Number
            Case CellKind.DateOnly, CellKind.Stamp
                Return KBotValueType.DateTime
            Case CellKind.Flag
                Return KBotValueType.Boolean
            Case Else
                Return KBotValueType.Text
        End Select
    End Function

    ''' <summary>The .NET format of a column: Romanian dates (dd.MM.yyyy), thousands separator, decimals as the column has them (none for whole numbers).</summary>
    Private Shared Function FormatOf(k_kind As CellKind, k_col As DbColumnInfo, k_isId As Boolean) As String
        Select Case k_kind
            Case CellKind.Whole
                Return If(k_isId OrElse String.Equals(k_col.DataType, "year", StringComparison.Ordinal), "0", "#,##0")
            Case CellKind.Fixed
                Dim k_scale As Integer = Math.Max(0, k_col.NumScale.GetValueOrDefault(0))
                Return If(k_scale > 0, "#,##0." & New String("0"c, k_scale), "#,##0")
            Case CellKind.Floating
                Return "#,##0.######"
            Case CellKind.DateOnly
                Return "dd.MM.yyyy"
            Case CellKind.Stamp
                Dim k_fraction As Integer = Math.Min(6, Math.Max(0, k_col.DtPrec.GetValueOrDefault(0)))
                Return "dd.MM.yyyy HH:mm:ss" & If(k_fraction > 0, "." & New String("f"c, k_fraction), String.Empty)
            Case Else
                Return String.Empty
        End Select
    End Function

    ''' <summary>The preset width (logical px): ids ~100, dates and date-times as wide as a typical value, numbers by their digits, long texts a fixed width.</summary>
    Private Function WidthOf(k_kind As CellKind, k_col As DbColumnInfo, k_isId As Boolean, k_format As String) As Integer
        Dim k_font As Font = gridData.Font
        Select Case k_kind
            Case CellKind.Whole
                If k_isId Then Return IdWidth
                Return Math.Max(70, Math.Min(170, TextWidth(DigitsSample(WholeDigits(k_col)), k_font) + CellPad))
            Case CellKind.Fixed
                Dim k_scale As Integer = Math.Max(0, k_col.NumScale.GetValueOrDefault(0))
                Dim k_integer As Integer = Math.Max(1, k_col.NumPrec.GetValueOrDefault(12) - k_scale)
                Dim k_sample As String = DigitsSample(k_integer) & If(k_scale > 0, "," & New String("8"c, k_scale), String.Empty)
                Return Math.Max(80, Math.Min(190, TextWidth(k_sample, k_font) + CellPad))
            Case CellKind.Floating
                Return 120
            Case CellKind.DateOnly
                Return TextWidth("88.88.8888", k_font) + DatePad
            Case CellKind.Stamp
                Dim k_fraction As Integer = Math.Min(6, Math.Max(0, k_col.DtPrec.GetValueOrDefault(0)))
                Return TextWidth("88.88.8888 88:88:88" & If(k_fraction > 0, "." & New String("8"c, k_fraction), String.Empty), k_font) + DatePad
            Case CellKind.TimeOnly
                Return TextWidth("-838:59:59", k_font) + CellPad
            Case CellKind.Flag
                Return 60
            Case CellKind.Blob
                Return Math.Max(80, TextWidth(BlobText, k_font) + CellPad)
            Case CellKind.LongText
                Return 260
            Case Else
                If k_col.DataType = "enum" OrElse k_col.DataType = "set" Then Return 120
                Dim k_length As Integer = CInt(Math.Min(40L, Math.Max(4L, k_col.CharLen.GetValueOrDefault(20))))
                Return Math.Max(70, Math.Min(320, TextWidth(New String("n"c, k_length), k_font) + CellPad))
        End Select
    End Function

    Private Shared Function WholeDigits(k_col As DbColumnInfo) As Integer
        Select Case k_col.DataType
            Case "tinyint" : Return 3
            Case "smallint" : Return 5
            Case "mediumint" : Return 8
            Case "int", "integer" : Return 10
            Case "bigint" : Return 19
            Case "year" : Return 4
            Case Else : Return Math.Max(1, k_col.NumPrec.GetValueOrDefault(10))
        End Select
    End Function

    ''' <summary>"88 888 888" style sample with the group separator every three digits, to measure a number of that many digits.</summary>
    Private Shared Function DigitsSample(k_digits As Integer) As String
        Dim k_sb As New StringBuilder()
        For k_i As Integer = 0 To k_digits - 1
            If k_i > 0 AndAlso (k_digits - k_i) Mod 3 = 0 Then k_sb.Append("."c)
            k_sb.Append("8"c)
        Next
        Return k_sb.ToString()
    End Function

    ''' <summary>Width of a text in LOGICAL pixels (the grid keeps its widths at 96 dpi).</summary>
    Private Function TextWidth(k_text As String, k_font As Font) As Integer
        Dim k_px As Integer = TextRenderer.MeasureText(k_text, k_font).Width
        Return CInt(Math.Ceiling(k_px * 96.0 / DeviceDpi))
    End Function

    ' ------------------------------------------------------------------
    ' Rows
    ' ------------------------------------------------------------------

    Private Sub AppendRows(k_page As DbRowsPage)
        Dim k_count As Integer = _current.Columns.Count
        Dim k_position As Integer = k_page.Offset
        For Each k_cells As List(Of JsonElement) In k_page.Rows
            Dim k_tag As New RowTag With {.Offset = k_position, .Raw = New Object(k_count - 1) {}, .Cells = k_cells}
            k_position += 1
            Dim k_row As KBotDataRow = gridData.AddRow()
            For k_i As Integer = 0 To Math.Min(k_count, k_cells.Count) - 1
                Dim k_raw As Object = ParseCell(k_cells(k_i), _kinds(k_i))
                k_tag.Raw(k_i) = k_raw
                Dim k_shown As Object = DisplayValue(k_raw)
                If k_shown IsNot Nothing Then k_row(_current.Columns(k_i).Name) = k_shown
            Next
            k_tag.Key = KeyOf(k_cells)
            k_row.Tag = k_tag
            _loadedRows += 1
        Next
        _hasMore = k_page.HasMore
        btnMore.Enabled = _hasMore
        If _queryMode Then
            lblInfo.Text = _tableName & ": " & _loadedRows.ToString("N0", CultureInfo.CurrentCulture) & " rânduri." & _queryCutNote
            Return
        End If
        Dim k_filtered As Boolean = _node IsNot Nothing AndAlso _node.HasFilter
        lblInfo.Text = "Tabel " & _tableName & If(k_filtered, " [doar rândurile cu " & _node.FilterCaption & "]", String.Empty) & ": " &
                       _loadedRows.ToString("N0", CultureInfo.CurrentCulture) & " rânduri încărcate" &
                       If(_hasMore, " (mai sunt; «Încarcă încă» le aduce pe următoarele)", " (toate)") &
                       ", estimat în tabel ~" & _current.Info.RowsEst.ToString("N0", CultureInfo.CurrentCulture) & If(k_filtered, " (fără filtru).", ".")
    End Sub

    ''' <summary>The key of a row as JSON (the primary key's cells exactly as the server sent them), or empty when the table has none.</summary>
    Private Function KeyOf(k_cells As List(Of JsonElement)) As String
        If _pkIndexes.Count = 0 Then Return String.Empty
        Dim k_key As New Dictionary(Of String, JsonElement)(StringComparer.Ordinal)
        For Each k_i As Integer In _pkIndexes
            If k_i >= k_cells.Count OrElse k_cells(k_i).ValueKind = JsonValueKind.Object OrElse k_cells(k_i).ValueKind = JsonValueKind.Null Then Return String.Empty
            k_key(_current.Columns(k_i).Name) = k_cells(k_i)
        Next
        Return JsonSerializer.Serialize(k_key)
    End Function

    ''' <summary>The typed value of a cell (Nothing = NULL, a marker object for a blob or a cut text, else a number, a date, a Boolean or a string).</summary>
    Private Shared Function ParseCell(k_element As JsonElement, k_kind As CellKind) As Object
        Select Case k_element.ValueKind
            Case JsonValueKind.Null, JsonValueKind.Undefined
                Return Nothing
            Case JsonValueKind.True
                Return True
            Case JsonValueKind.False
                Return False
            Case JsonValueKind.Object
                Dim k_part As JsonElement
                If k_element.TryGetProperty("b", k_part) Then Return New BlobMarker(k_part.GetInt64())
                If k_element.TryGetProperty("t", k_part) Then Return New CutText(k_part.GetString())
                Return k_element.GetRawText()
            Case JsonValueKind.Number
                Select Case k_kind
                    Case CellKind.Flag
                        Return k_element.GetDecimal() <> 0D
                    Case CellKind.Whole
                        Dim k_long As Long
                        If k_element.TryGetInt64(k_long) Then Return k_long
                        Return k_element.GetDecimal()
                    Case CellKind.Fixed
                        Return k_element.GetDecimal()
                    Case CellKind.Floating
                        Return k_element.GetDouble()
                    Case Else
                        Return k_element.GetRawText()
                End Select
            Case Else
                Dim k_text As String = k_element.GetString()
                Select Case k_kind
                    Case CellKind.Fixed
                        Dim k_decimal As Decimal
                        If Decimal.TryParse(k_text, NumberStyles.Float, CultureInfo.InvariantCulture, k_decimal) Then Return k_decimal
                    Case CellKind.DateOnly, CellKind.Stamp
                        Dim k_date As DateTime
                        If DateTime.TryParse(k_text, CultureInfo.InvariantCulture, DateTimeStyles.None, k_date) Then Return k_date
                    Case CellKind.Flag
                        Return Not (k_text = "0" OrElse k_text = String.Empty)
                    Case CellKind.Whole
                        Dim k_long As Long
                        If Long.TryParse(k_text, NumberStyles.Integer, CultureInfo.InvariantCulture, k_long) Then Return k_long
                End Select
                Return k_text
        End Select
    End Function

    ''' <summary>What the grid cell holds: a blob is «&lt;BLOB&gt;», a text loses its line breaks (the full text is in the box under the grid).</summary>
    Private Shared Function DisplayValue(k_raw As Object) As Object
        If k_raw Is Nothing Then Return Nothing
        If TypeOf k_raw Is BlobMarker Then Return BlobText
        Dim k_cut As CutText = TryCast(k_raw, CutText)
        If k_cut IsNot Nothing Then Return OneLine(k_cut.Preview)
        Dim k_text As String = TryCast(k_raw, String)
        If k_text IsNot Nothing Then Return OneLine(k_text)
        Return k_raw
    End Function

    Private Shared Function OneLine(k_text As String) As String
        If k_text.IndexOfAny({ControlChars.Cr, ControlChars.Lf, ControlChars.Tab}) < 0 Then Return k_text
        Return k_text.Replace(vbCrLf, " ").Replace(vbCr, " ").Replace(vbLf, " ").Replace(vbTab, " ")
    End Function

    Private Async Sub BtnMore_Click(k_sender As Object, e As EventArgs) Handles btnMore.Click
        Try
            If _current Is Nothing OrElse Not _hasMore Then Return
            If _node Is Nothing Then Return
            Dim k_id As Integer = _request
            Dim k_offset As Integer = _loadedRows
            Dim k_args As String() = ViewArgs(_node, k_offset).ToArray()
            TableNavLog.Write("MORE", _node.Caption & ": db-rows " & String.Join(" ", k_args))
            Dim k_result As DbRowsResult = Nothing
            Await UiRun.Guard("DbBrowserView.BtnMore_Click", _context,
                Async Function()
                    _context.Say("Se aduc încă " & PageSize.ToString(CultureInfo.CurrentCulture) & " rânduri...")
                    k_result = Await _context.Client.CallAsync(Of DbRowsResult)("db-rows", k_args)
                End Function)
            If k_result Is Nothing OrElse k_id <> _request OrElse k_offset <> _loadedRows Then
                TableNavLog.Write("MORE", "page NOT added: " & If(k_result Is Nothing, "the server call failed", "the view changed meanwhile"))
                Return
            End If
            TableNavLog.Write("MORE", "page added: +" & k_result.Page.Rows.Count.ToString(CultureInfo.InvariantCulture) & " rows, hasMore=" & k_result.Page.HasMore.ToString() & ", kept in memory with the view")
            _filling = True
            gridData.BeginUpdate()
            Try
                AppendRows(k_result.Page)
            Finally
                gridData.EndUpdate()
                _filling = False
            End Try
            ' The view kept in memory grows with the grid: coming back to it shows every page already read.
            _current.Page.Rows.AddRange(k_result.Page.Rows)
            _current.Page.HasMore = k_result.Page.HasMore
            ShowTableInfo()
            _context.Say("Gata: " & _loadedRows.ToString("N0", CultureInfo.CurrentCulture) & " rânduri încărcate.")
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.BtnMore_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnReload_Click(k_sender As Object, e As EventArgs) Handles btnReload.Click
        Try
            If _context Is Nothing OrElse _context.IsAll Then Return
            Dim k_shown As NavNode = _node
            Dim k_query As QueryDef = _lastQuery
            Dim k_queryValues As IDictionary(Of String, Object) = _lastQueryValues
            TableNavLog.Write("RELOAD", "«Reincarca» pressed; view now: " & If(k_shown Is Nothing, "none", k_shown.Caption))
            ClearCache(_context.SelectedDb)
            Await EnsureTreeAsync(True)
            ' A query on screen runs again with the same values; it has no history.
            If _queryMode AndAlso k_query IsNot Nothing AndAlso k_queryValues IsNot Nothing Then
                If String.Equals(_loadedDb, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then Await ExecuteQueryAsync(k_query, k_queryValues)
                Return
            End If
            ' The history stays; only what was read is read again.
            If k_shown IsNot Nothing AndAlso String.Equals(_loadedDb, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then Await OpenNodeAsync(k_shown)
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.BtnReload_Click", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' The cell box
    ' ------------------------------------------------------------------

    ''' <summary>A cell of a column that leads to other tables looks like a link: the accent colour of the theme and underlined (a NULL cell leads nowhere and stays plain).</summary>
    Private Sub GridData_CellFormatting(k_sender As Object, e As KBotCellFormattingEventArgs) Handles gridData.CellFormatting
        Try
            Dim k_outgoing As Boolean
            If e.Value Is Nothing OrElse Not _linkColumns.TryGetValue(e.ColumnKey, k_outgoing) Then Return
            Dim k_base As Font = e.Font
            If k_base Is Nothing Then Return
            Dim k_link As Font = Nothing
            If Not _linkFonts.TryGetValue(k_base, k_link) Then
                k_link = New Font(k_base, k_base.Style Or FontStyle.Underline)
                _linkFonts(k_base) = k_link
            End If
            e.Font = k_link
            ' Two kinds of link, two colours of the theme: a foreign key (leads out) is the accent, a column only pointed at (leads in) is the success colour.
            e.ForeColor = If(k_outgoing, ThemeManager.Current.Palette.AccentColor, ThemeManager.Current.Palette.SuccessColor)
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.GridData_CellFormatting", ex)
        End Try
    End Sub

    ''' <summary>The hand over a link cell (a column that leads to other tables, the cell not NULL), as over any link.</summary>
    Private Sub GridData_CellCursor(k_sender As Object, e As KBotCellCursorEventArgs) Handles gridData.CellCursor
        Try
            If _current Is Nothing OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridData.RowCount OrElse Not _linkColumns.ContainsKey(e.ColumnKey) Then Return
            If gridData.Rows(e.RowIndex)(e.ColumnKey) IsNot Nothing Then e.Cursor = Cursors.Hand
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.GridData_CellCursor", ex)
        End Try
    End Sub

    Private Sub GridData_CellClick(k_sender As Object, e As KBotCellEventArgs) Handles gridData.CellClick
        Try
            ShowColumn(e.ColumnKey)
            ShowCell(e.RowIndex, e.ColumnKey)
            ' One click on a link cell opens the popup of the linked tables.
            If _current IsNot Nothing AndAlso e.RowIndex >= 0 AndAlso e.RowIndex < gridData.RowCount AndAlso _linkColumns.ContainsKey(e.ColumnKey) Then
                Dim k_tag As RowTag = TryCast(gridData.Rows(e.RowIndex).Tag, RowTag)
                Dim k_i As Integer
                If k_tag IsNot Nothing AndAlso _colIndex.TryGetValue(e.ColumnKey, k_i) Then ShowLinkMenu(k_tag, e.ColumnKey, k_i)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.GridData_CellClick", ex)
        End Try
    End Sub

    Private Sub GridData_SelectionChanged(k_sender As Object, e As EventArgs) Handles gridData.SelectionChanged
        Try
            If _filling OrElse _current Is Nothing Then Return
            Dim k_col As String = gridData.CurrentColumnKey
            If String.IsNullOrEmpty(k_col) OrElse gridData.CurrentRowIndex < 0 Then Return
            ShowColumn(k_col)
            ShowCell(gridData.CurrentRowIndex, k_col)
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.GridData_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub GridData_HeaderClick(k_sender As Object, e As KBotCellEventArgs) Handles gridData.HeaderClick
        Try
            ShowColumn(e.ColumnKey)
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.GridData_HeaderClick", ex)
        End Try
    End Sub

    Private Sub ShowCell(k_rowIndex As Integer, k_columnKey As String)
        If _current Is Nothing OrElse k_rowIndex < 0 OrElse k_rowIndex >= gridData.RowCount Then Return
        Dim k_stamp As String = k_rowIndex.ToString(CultureInfo.InvariantCulture) & "|" & k_columnKey
        If k_stamp = _cellShown Then Return
        Dim k_tag As RowTag = TryCast(gridData.Rows(k_rowIndex).Tag, RowTag)
        Dim k_i As Integer
        If k_tag Is Nothing OrElse Not _colIndex.TryGetValue(k_columnKey, k_i) Then Return
        _cellShown = k_stamp
        edCell.TextBox.Text = CellText(k_tag.Raw(k_i), k_i)
    End Sub

    Private Function CellText(k_raw As Object, k_columnIndex As Integer) As String
        If k_raw Is Nothing Then Return "(NULL)"
        Dim k_blob As BlobMarker = TryCast(k_raw, BlobMarker)
        If k_blob IsNot Nothing Then
            Return BlobText & " - " & k_blob.Length.ToString("N0", CultureInfo.CurrentCulture) & " octeți (" & UiRun.FormatBytes(k_blob.Length) &
                   "). Dublu clic pe celulă ca să-i vezi conținutul."
        End If
        Dim k_cut As CutText = TryCast(k_raw, CutText)
        If k_cut IsNot Nothing Then
            Return k_cut.Preview & vbLf & vbLf & "[... textul continuă; aici se văd doar primele " & k_cut.Preview.Length.ToString("N0", CultureInfo.CurrentCulture) &
                   " caractere. Dublu clic pe celulă pentru tot textul.]"
        End If
        If TypeOf k_raw Is Boolean Then Return If(CBool(k_raw), "Da (1)", "Nu (0)")
        Dim k_formattable As IFormattable = TryCast(k_raw, IFormattable)
        If k_formattable IsNot Nothing Then
            Dim k_format As String = If(k_columnIndex >= 0 AndAlso k_columnIndex < _formats.Count, _formats(k_columnIndex), String.Empty)
            Return k_formattable.ToString(If(k_format = String.Empty, Nothing, k_format), CultureInfo.CurrentCulture)
        End If
        Return Convert.ToString(k_raw, CultureInfo.CurrentCulture)
    End Function

    ' ------------------------------------------------------------------
    ' Double click: the whole cell in a window
    ' ------------------------------------------------------------------

    Private Async Sub GridData_CellDoubleClick(k_sender As Object, e As KBotCellEventArgs) Handles gridData.CellDoubleClick
        Try
            If _current Is Nothing OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridData.RowCount Then Return
            Dim k_tag As RowTag = TryCast(gridData.Rows(e.RowIndex).Tag, RowTag)
            Dim k_i As Integer
            If k_tag Is Nothing OrElse Not _colIndex.TryGetValue(e.ColumnKey, k_i) Then Return
            Dim k_raw As Object = k_tag.Raw(k_i)
            Dim k_title As String = _tableName & "." & e.ColumnKey
            If _queryMode Then
                If TypeOf k_raw Is BlobMarker Then _context.Say("Valoarea binară dintr-o interogare nu se poate deschide; citiți-o din tabelul ei.")
                Return
            End If
            If TypeOf k_raw Is BlobMarker OrElse TypeOf k_raw Is CutText Then
                TableNavLog.Write("DBLCLICK", k_title & " row " & (k_tag.Offset + 1).ToString(CultureInfo.InvariantCulture) & ": blob / cut text, the content window opens (no link menu)")
                Await OpenFetchedAsync(k_tag, e.ColumnKey, k_title)
            ElseIf _kinds(k_i) = CellKind.LongText AndAlso TypeOf k_raw Is String Then
                TableNavLog.Write("DBLCLICK", k_title & " row " & (k_tag.Offset + 1).ToString(CultureInfo.InvariantCulture) & ": long text, the content window opens (no link menu)")
                Dim k_text As String = DirectCast(k_raw, String)
                OpenWindow(New CellContentForm(k_title, k_title & " - rândul " & (k_tag.Offset + 1).ToString("N0", CultureInfo.CurrentCulture) & " - " &
                                               k_text.Length.ToString("N0", CultureInfo.CurrentCulture) & " caractere", k_text, FileNameOf(k_title, k_tag, ".txt")))
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.GridData_CellDoubleClick", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Double click on a linked cell: the tables it leads to
    ' ------------------------------------------------------------------

    Private Function InLoadedDb(k_db As String) As Boolean
        Return String.IsNullOrEmpty(k_db) OrElse String.Equals(k_db, _loadedDb, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Sub AddLink(k_list As List(Of LinkTarget), k_link As LinkTarget)
        If Not k_list.Any(Function(k_l) k_l.Outgoing = k_link.Outgoing AndAlso String.Equals(k_l.Table, k_link.Table, StringComparison.Ordinal) AndAlso
                                        k_l.Columns.SequenceEqual(k_link.Columns, StringComparer.Ordinal)) Then k_list.Add(k_link)
    End Sub

    ''' <summary>The tables a column leads to: the ones its foreign keys point at, then the ones whose foreign keys point at it (only tables of the open database: the server reads no other).</summary>
    Private Function LinksOf(k_column As String) As List(Of LinkTarget)
        Dim k_list As New List(Of LinkTarget)()
        Dim k_fks As List(Of DbForeignKey) = Nothing
        If _fkByColumn.TryGetValue(k_column, k_fks) Then
            For Each k_fk As DbForeignKey In k_fks
                If k_fk.Columns.Count > 0 AndAlso k_fk.Columns.Count = k_fk.RefColumns.Count AndAlso InLoadedDb(k_fk.RefDb) Then
                    AddLink(k_list, New LinkTarget(k_fk.RefTable, k_fk.RefColumns, k_fk.Columns, True))
                End If
            Next
        End If
        Dim k_refs As List(Of DbReference) = Nothing
        If _refByColumn.TryGetValue(k_column, k_refs) Then
            For Each k_ref As DbReference In k_refs
                If k_ref.Columns.Count > 0 AndAlso k_ref.Columns.Count = k_ref.RefColumns.Count AndAlso InLoadedDb(k_ref.FromDb) Then
                    AddLink(k_list, New LinkTarget(k_ref.Table, k_ref.Columns, k_ref.RefColumns, False))
                End If
            Next
        End If
        Return k_list
    End Function

    ''' <summary>Opens the popup with the tables a cell leads to (nothing when the cell is NULL or its column has no link). Choosing one opens that table with only the rows equal to this cell.</summary>
    Private Sub ShowLinkMenu(k_tag As RowTag, k_columnKey As String, k_columnIndex As Integer)
        Dim k_cell As String = _tableName & "." & k_columnKey & " row " & (k_tag.Offset + 1).ToString(CultureInfo.InvariantCulture)
        If k_tag.Raw(k_columnIndex) Is Nothing OrElse k_tag.Cells Is Nothing OrElse k_columnIndex >= k_tag.Cells.Count Then
            TableNavLog.Write("CLICK", k_cell & ": NULL (or no raw value), leads nowhere")
            Return
        End If
        Dim k_targets As List(Of LinkTarget) = LinksOf(k_columnKey)
        If k_targets.Count = 0 Then
            TableNavLog.Write("CLICK", k_cell & ": the column has no link (not a foreign key, not referenced)")
            Return
        End If
        Dim k_json As String = k_tag.Cells(k_columnIndex).GetRawText()
        Dim k_text As String = DisplayText(k_tag.Raw(k_columnIndex), k_columnIndex)
        TableNavLog.Write("CLICK", k_cell & ": value=" & k_text & " json=" & k_json & "; links offered: " & k_targets.Count.ToString(CultureInfo.InvariantCulture))
        For Each k_link As LinkTarget In k_targets
            TableNavLog.Write("CLICK", "    " & k_link.Caption & "   (" & String.Join(",", k_link.SourceColumns) & " -> " & String.Join(",", k_link.Columns) & ")" & If(k_link.Columns.Count > 1, "   COMPOSITE", String.Empty))
        Next
        Dim k_shown As String = If(k_text.Length > 40, k_text.Substring(0, 40) & "...", k_text)
        Dim k_items As New List(Of CustomPopupItem) From {
            New CustomPopupItem("h", "Legat prin " & k_columnKey & " = " & k_shown.Replace("&", "&&")) With {.Enabled = False},
            CustomPopupItem.Separator()
        }
        For k_n As Integer = 0 To k_targets.Count - 1
            Dim k_target As LinkTarget = k_targets(k_n)
            k_items.Add(New CustomPopupItem(k_n.ToString(CultureInfo.InvariantCulture), k_target.Caption.Replace("&", "&&")) With {.Tag = k_target})
        Next
        ' A little off the cursor, so the popup never opens right under it.
        Dim k_point As New Point(Cursor.Position.X + 2, Cursor.Position.Y + 2)
        ' NOT in a «Using»: shown modeless, the popup disposes itself when it closes.
        Dim k_menu As New CustomPopup(k_items)
        AddHandler k_menu.ItemClicked,
            Async Sub(k_sender As Object, k_e As CustomPopupItemEventArgs)
                Try
                    Dim k_picked As LinkTarget = TryCast(k_e.Item.Tag, LinkTarget)
                    If k_picked IsNot Nothing Then
                        TableNavLog.Write("LINK", "chosen in the popup: " & k_picked.Caption)
                        Await OpenLinkAsync(k_picked, k_columnKey, k_json, k_text)
                    End If
                Catch ex As Exception
                    GlobalErrorLog.Write("DbBrowserView.LinkMenu_ItemClicked", ex)
                End Try
            End Sub
        AddHandler k_menu.FormClosed, Sub(k_sender As Object, k_e As FormClosedEventArgs) If k_menu.ClickedItem Is Nothing Then TableNavLog.Write("LINK", "popup closed without a choice")
        k_menu.ShowAt(gridData, k_point)
    End Sub

    ''' <summary>
    ''' Opens the table a link leads to, only with the rows equal to the cell. A composite key has more columns than the cell gives: the
    ''' values of the others are asked one by one in a combo limited to the list (<see cref="AskValue"/>), so the answer can never be a value
    ''' the target table does not hold. It is a step of the history taken from the current view: when that step was taken
    ''' before it is the same node (and what lies after it is still there for «forward»).
    ''' </summary>
    Private Async Function OpenLinkAsync(k_target As LinkTarget, k_clickedColumn As String, k_json As String, k_text As String) As Task
        If _node Is Nothing Then Return
        Dim k_from As NavNode = _node
        Dim k_db As String = _loadedDb
        ' Lets the popup that chose the link close before the next one opens.
        Await Task.Yield()
        Dim k_chosen(k_target.Columns.Count - 1) As FilterPart
        Dim k_clickedAt As Integer = k_target.SourceColumns.IndexOf(k_clickedColumn)
        If k_clickedAt < 0 Then
            TableNavLog.Write("LINK", "STOPPED: the clicked column " & k_clickedColumn & " is not among the source columns " & String.Join(",", k_target.SourceColumns))
            Return
        End If
        k_chosen(k_clickedAt) = New FilterPart(k_target.Columns(k_clickedAt), k_json, k_text)
        TableNavLog.Write("LINK", "from " & k_from.Caption & " to " & k_target.Table & ": " & k_target.Columns(k_clickedAt) & " = " & k_text & " (from the clicked cell)" &
                          If(k_chosen.Length > 1, "; " & (k_chosen.Length - 1).ToString(CultureInfo.InvariantCulture) & " more column(s) to ask", String.Empty))
        For k_at As Integer = 0 To k_chosen.Length - 1
            If k_chosen(k_at) IsNot Nothing Then Continue For
            Dim k_part As FilterPart = AskValue(k_target, k_at, k_chosen.Where(Function(k_p) k_p IsNot Nothing).ToList())
            ' Dismissed, nothing to choose from, or the page moved on while the list was open.
            If k_part Is Nothing OrElse Not ReferenceEquals(_node, k_from) OrElse Not String.Equals(_loadedDb, k_db, StringComparison.Ordinal) Then
                TableNavLog.Write("LINK", "STOPPED while asking " & k_target.Columns(k_at) & ": " & If(k_part Is Nothing, "no value chosen (window closed)", "the page moved on meanwhile"))
                Return
            End If
            k_chosen(k_at) = k_part
        Next
        Dim k_next As New NavNode(k_from, k_target.Table, k_chosen)
        Dim k_known As NavNode = k_from.Children.FirstOrDefault(Function(k_c) String.Equals(k_c.Key, k_next.Key, StringComparison.Ordinal))
        TableNavLog.Write("LINK", "step " & k_next.Caption & ": " & If(k_known Is Nothing, "NEW step", "the same step was taken before, its node is reused"))
        Await OpenNodeAsync(If(k_known, k_next))
    End Function

    ''' <summary>
    ''' Asks for the value of one column of the target table in a window with a combo the operator types in (<see cref="LinkValueForm"/>):
    ''' limited to the list, the values that contain the typed text come from the server after the first character, only from the rows that
    ''' match what is already chosen. Nothing = the window was closed without a choice.
    ''' </summary>
    Private Function AskValue(k_target As LinkTarget, k_at As Integer, k_known As List(Of FilterPart)) As FilterPart
        Dim k_column As String = k_target.Columns(k_at)
        Dim k_where As String = NavNode.WhereJsonOf(k_known)
        Dim k_about As String = String.Join(", ", k_known.Select(Function(k_p) k_p.Caption))
        TableNavLog.Write("ASK", k_target.Table & "." & k_column & ": window opens; already chosen " & k_about & "; where=" & k_where)
        Using k_form As New LinkValueForm("Tabele legate", "În " & k_target.Table & " alegeți valoarea pentru " & k_column & vbLf & "(deja alese: " & k_about & ")",
                                          Function(k_typed As String) FetchValuesAsync(k_target.Table, k_column, k_where, k_typed))
            If k_form.ShowDialog(FindForm()) <> DialogResult.OK Then
                TableNavLog.Write("ASK", k_target.Table & "." & k_column & ": window closed WITHOUT a value (jump cancelled)")
                Return Nothing
            End If
            TableNavLog.Write("ASK", k_target.Table & "." & k_column & ": value chosen = " & k_form.ChosenText & " json=" & k_form.ChosenJson)
            Return New FilterPart(k_column, k_form.ChosenJson, k_form.ChosenText)
        End Using
    End Function

    ''' <summary>The values of a column that contain a text (and sit in rows matching <paramref name="k_where"/>), from memory when asked before. Nothing = the call failed (already shown).</summary>
    Private Async Function FetchValuesAsync(k_table As String, k_column As String, k_where As String, k_typed As String) As Task(Of DbDistinctResult)
        Dim k_key As String = _loadedDb & "|" & k_table & "|" & k_column & "|" & k_where & "|" & k_typed.ToLowerInvariant()
        Dim k_result As DbDistinctResult = Nothing
        If _distinctCache.TryGetValue(k_key, k_result) Then
            TableNavLog.Write("FETCH", k_table & "." & k_column & " containing «" & k_typed & "»: " & k_result.Values.Count.ToString(CultureInfo.InvariantCulture) & " values, source=MEMORY")
            Return k_result
        End If
        TableNavLog.Write("FETCH", k_table & "." & k_column & " containing «" & k_typed & "»: db-distinct --where-json " & k_where & " --like=" & k_typed & " --limit " & LinkValueForm.PickLimit.ToString(CultureInfo.InvariantCulture))
        Await UiRun.Guard("DbBrowserView.FetchValuesAsync", _context,
            Async Function()
                _context.Say("Se caută în " & k_table & "." & k_column & " valorile care conțin «" & k_typed & "»...")
                ' «--like=» glued to the text: a text that starts with a dash is not taken for an option.
                k_result = Await _context.Client.CallAsync(Of DbDistinctResult)("db-distinct", "--db", _loadedDb, "--table", k_table, "--column", k_column,
                                                                               "--where-json", k_where, "--like=" & k_typed,
                                                                               "--limit", LinkValueForm.PickLimit.ToString(CultureInfo.InvariantCulture))
            End Function)
        If k_result Is Nothing Then
            TableNavLog.Write("FETCH", "the server call FAILED (the error was shown)")
        Else
            _distinctCache(k_key) = k_result
            TableNavLog.Write("FETCH", "server answered: " & k_result.Values.Count.ToString(CultureInfo.InvariantCulture) & " values, hasMore=" & k_result.HasMore.ToString() & ", kept in memory")
        End If
        Return k_result
    End Function

    ' ------------------------------------------------------------------
    ' Back / forward
    ' ------------------------------------------------------------------

    Private Sub UpdateNavButtons()
        btnBack.Enabled = _node IsNot Nothing AndAlso _node.Parent IsNot Nothing
        btnForward.Enabled = _node IsNot Nothing AndAlso _node.Children.Count > 0
    End Sub

    ''' <summary>The steps behind the current view, the nearest first, up to where it began.</summary>
    Private Function BackList() As List(Of NavNode)
        Dim k_list As New List(Of NavNode)()
        Dim k_at As NavNode = If(_node Is Nothing, Nothing, _node.Parent)
        While k_at IsNot Nothing
            k_list.Add(k_at)
            k_at = k_at.Parent
        End While
        Return k_list
    End Function

    ''' <summary>
    ''' The views «forward» offers: from the current one, while it has a single child that child and then ITS children, in the order they were
    ''' visited; where a view has several children those are listed and the walk stops (only them, not what lies behind each).
    ''' So T1 -> T2 -> T3 seen from T1 offers T2, T3; but T1 -> T2 -> T3 and T1 -> T4 -> T5 seen from T1 offers only T2 and T4.
    ''' </summary>
    Private Function ForwardList() As List(Of NavNode)
        Dim k_list As New List(Of NavNode)()
        Dim k_at As NavNode = _node
        While k_at IsNot Nothing AndAlso k_at.Children.Count > 0
            If k_at.Children.Count = 1 Then
                k_at = k_at.Children(0)
                k_list.Add(k_at)
            Else
                k_list.AddRange(k_at.Children)
                Exit While
            End If
        End While
        Return k_list
    End Function

    Private Async Sub BtnBack_Click(k_sender As Object, e As EventArgs) Handles btnBack.Click
        Try
            If CustomPopup.ClosedJustNow Then
                TableNavLog.Write("BACK", "click ignored (a popup closed a moment ago)")
                Return
            End If
            Dim k_list As List(Of NavNode) = BackList()
            TableNavLog.Write("BACK", "clicked on " & If(_node Is Nothing, "no view", _node.Caption) & "; steps behind (nearest first): " & Names(k_list))
            Await GoToAsync("BACK", k_list, btnBack)
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.BtnBack_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnForward_Click(k_sender As Object, e As EventArgs) Handles btnForward.Click
        Try
            If CustomPopup.ClosedJustNow Then
                TableNavLog.Write("FORWARD", "click ignored (a popup closed a moment ago)")
                Return
            End If
            Dim k_list As List(Of NavNode) = ForwardList()
            TableNavLog.Write("FORWARD", "clicked on " & If(_node Is Nothing, "no view", _node.Caption) & "; offered: " & Names(k_list) & "; children of the view: " & Names(If(_node Is Nothing, New List(Of NavNode)(), _node.Children)))
            Await GoToAsync("FORWARD", k_list, btnForward)
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.BtnForward_Click", ex)
        End Try
    End Sub

    ''' <summary>One view to go to: straight there. Several: a popup with all of them, the operator chooses.</summary>
    Private Async Function GoToAsync(k_area As String, k_views As List(Of NavNode), k_anchor As Control) As Task
        If k_views.Count = 0 Then
            TableNavLog.Write(k_area, "nothing to go to")
            Return
        End If
        If k_views.Count = 1 Then
            TableNavLog.Write(k_area, "one view only, straight there: " & k_views(0).Caption)
            Await OpenNodeAsync(k_views(0))
            Return
        End If
        TableNavLog.Write(k_area, "popup with " & k_views.Count.ToString(CultureInfo.InvariantCulture) & " views")
        Dim k_items As New List(Of CustomPopupItem)()
        For k_n As Integer = 0 To k_views.Count - 1
            k_items.Add(New CustomPopupItem(k_n.ToString(CultureInfo.InvariantCulture), k_views(k_n).Caption.Replace("&", "&&")) With {.Tag = k_views(k_n)})
        Next
        Dim k_menu As New CustomPopup(k_items)
        AddHandler k_menu.ItemClicked,
            Async Sub(k_sender As Object, k_e As CustomPopupItemEventArgs)
                Try
                    Dim k_picked As NavNode = TryCast(k_e.Item.Tag, NavNode)
                    If k_picked IsNot Nothing Then
                        TableNavLog.Write(k_area, "chosen in the popup: " & k_picked.Caption)
                        Await OpenNodeAsync(k_picked)
                    End If
                Catch ex As Exception
                    GlobalErrorLog.Write("DbBrowserView.HistoryMenu_ItemClicked", ex)
                End Try
            End Sub
        AddHandler k_menu.FormClosed, Sub(k_sender As Object, k_e As FormClosedEventArgs) If k_menu.ClickedItem Is Nothing Then TableNavLog.Write(k_area, "popup closed without a choice")
        k_menu.ShowBelow(k_anchor)
    End Function

    Private Async Function OpenFetchedAsync(k_tag As RowTag, k_column As String, k_title As String) As Task
        Dim k_args As New List(Of String) From {"--db", _loadedDb, "--table", _tableName, "--column", k_column}
        If k_tag.Key <> String.Empty Then
            k_args.AddRange({"--key-json", k_tag.Key})
        Else
            k_args.AddRange({"--offset", k_tag.Offset.ToString(CultureInfo.InvariantCulture)})
        End If
        Dim k_cell As DbCellResult = Nothing
        Await UiRun.Guard("DbBrowserView.OpenFetchedAsync", _context,
            Async Function()
                _context.Say("Se citește conținutul celulei " & k_title & "...")
                k_cell = Await _context.Client.CallAsync(Of DbCellResult)("db-cell", k_args.ToArray())
            End Function)
        If k_cell Is Nothing Then Return
        _context.Say("Gata.")
        Dim k_info As String = k_title & " - rândul " & (k_tag.Offset + 1).ToString("N0", CultureInfo.CurrentCulture)
        If k_tag.Key = String.Empty Then k_info &= " (tabel fără cheie primară: rândul e găsit după poziție)"
        Select Case k_cell.Kind
            Case "blob"
                Dim k_bytes As Byte() = Convert.FromBase64String(k_cell.B64)
                k_info &= " - " & k_cell.Length.ToString("N0", CultureInfo.CurrentCulture) & " octeți" &
                          If(k_cell.Truncated, ", serverul a trimis doar primii " & UiRun.FormatBytes(k_bytes.Length), String.Empty) &
                          If(k_bytes.Length > CellContentForm.DumpBytes, ". Dump-ul arată primii " & UiRun.FormatBytes(CellContentForm.DumpBytes) & "; fișierul salvat are tot ce a venit.", String.Empty)
                OpenWindow(New CellContentForm(k_title, k_info, k_bytes, FileNameOf(k_title, k_tag, ".bin")))
            Case "text"
                k_info &= " - " & k_cell.Length.ToString("N0", CultureInfo.CurrentCulture) & " caractere" &
                          If(k_cell.Truncated, ", serverul a trimis doar începutul", String.Empty)
                OpenWindow(New CellContentForm(k_title, k_info, If(k_cell.Text, String.Empty), FileNameOf(k_title, k_tag, ".txt")))
            Case Else
                _context.Say("Celula " & k_title & " e NULL.")
        End Select
    End Function

    Private Sub OpenWindow(k_form As CellContentForm)
        Using k_form
            k_form.ShowDialog(FindForm())
        End Using
    End Sub

    Private Shared Function FileNameOf(k_title As String, k_tag As RowTag, k_extension As String) As String
        Dim k_name As String = k_title & "_" & (k_tag.Offset + 1).ToString(CultureInfo.InvariantCulture)
        For Each k_bad As Char In Path.GetInvalidFileNameChars()
            k_name = k_name.Replace(k_bad, "_"c)
        Next
        Return k_name & k_extension
    End Function

    ' ------------------------------------------------------------------
    ' The two fact grids
    ' ------------------------------------------------------------------

    Private Sub ShowTableInfo()
        If _current Is Nothing Then Return
        If _queryMode Then
            FillKvp(gridTable, _queryFacts)
            Return
        End If
        Dim k_info As DbTableInfo = _current.Info
        Dim k_items As New List(Of KeyValuePair(Of String, String))()
        Dim k_add As Action(Of String, String) = Sub(k_k, k_v) k_items.Add(New KeyValuePair(Of String, String)(k_k, If(String.IsNullOrEmpty(k_v), "-", k_v)))
        k_add("Tabel", k_info.Name)
        If _node IsNot Nothing AndAlso _node.HasFilter Then k_add("Filtru", _node.FilterFull)
        k_add("Tip", If(String.Equals(k_info.Kind, "VIEW", StringComparison.OrdinalIgnoreCase), "Vizualizare", "Tabel"))
        k_add("Motor", k_info.Engine)
        k_add("Colație", k_info.Collation)
        k_add("Rânduri (estimat)", "~" & k_info.RowsEst.ToString("N0", CultureInfo.CurrentCulture))
        k_add("Rânduri încărcate", _loadedRows.ToString("N0", CultureInfo.CurrentCulture) & If(_hasMore, " (mai sunt)", String.Empty))
        k_add("Mărime date", UiRun.FormatBytes(k_info.DataBytes))
        k_add("Mărime indexuri", UiRun.FormatBytes(k_info.IndexBytes))
        k_add("Auto-increment", If(k_info.AutoIncrement.HasValue, k_info.AutoIncrement.Value.ToString("N0", CultureInfo.CurrentCulture), String.Empty))
        k_add("Creat", FormatStamp(k_info.Created))
        k_add("Modificat", FormatStamp(k_info.Updated))
        k_add("Coloane", _current.Columns.Count.ToString(CultureInfo.CurrentCulture))
        k_add("Cheie primară", String.Join(", ", _current.PrimaryKey))
        k_add("Chei străine", String.Join("; ", _current.ForeignKeys.Select(Function(k_fk) ForeignKeyText(k_fk))))
        k_add("Referit de", String.Join("; ", _current.ReferencedBy.Select(Function(k_r) k_r.Table & "(" & String.Join(", ", k_r.Columns) & ")")))
        k_add("Indexuri", String.Join("; ", _current.Indexes.Select(Function(k_x) IndexText(k_x))))
        k_add("Comentariu", k_info.Comment)
        FillKvp(gridTable, k_items)
    End Sub

    Private Function ForeignKeyText(k_fk As DbForeignKey) As String
        Return "(" & String.Join(", ", k_fk.Columns) & ") -> " & QualifiedTable(k_fk) & "(" & String.Join(", ", k_fk.RefColumns) & ")"
    End Function

    Private Function QualifiedTable(k_fk As DbForeignKey) As String
        Return If(String.Equals(k_fk.RefDb, _loadedDb, StringComparison.OrdinalIgnoreCase), k_fk.RefTable, k_fk.RefDb & "." & k_fk.RefTable)
    End Function

    Private Shared Function IndexText(k_index As DbIndexInfo) As String
        Return k_index.Name & " (" & String.Join(", ", k_index.Columns) & ")" & If(k_index.Unique, " unic", String.Empty)
    End Function

    ''' <summary>The facts about one column: its type, whether it is a key, and — for a foreign key — the column it points at and what happens to the rows.</summary>
    Private Sub ShowColumn(k_name As String)
        If _current Is Nothing OrElse String.IsNullOrEmpty(k_name) OrElse String.Equals(k_name, _columnShown, StringComparison.Ordinal) Then Return
        Dim k_i As Integer
        If Not _colIndex.TryGetValue(k_name, k_i) Then Return
        _columnShown = k_name
        Dim k_col As DbColumnInfo = _current.Columns(k_i)
        Dim k_items As New List(Of KeyValuePair(Of String, String))()
        Dim k_add As Action(Of String, String) = Sub(k_k, k_v) k_items.Add(New KeyValuePair(Of String, String)(k_k, If(String.IsNullOrEmpty(k_v), "-", k_v)))
        k_add("Coloană", k_col.Name)
        k_add("Poziție", k_col.Pos.ToString(CultureInfo.CurrentCulture))
        k_add("Tip", k_col.ColumnType)
        If k_col.CharLen.HasValue Then k_add("Lungime maximă", k_col.CharLen.Value.ToString("N0", CultureInfo.CurrentCulture))
        If k_col.NumPrec.HasValue Then k_add("Precizie / scală", k_col.NumPrec.Value.ToString(CultureInfo.CurrentCulture) & " / " & k_col.NumScale.GetValueOrDefault(0).ToString(CultureInfo.CurrentCulture))
        k_add("Poate fi NULL", If(k_col.Nullable, "Da", "Nu"))
        k_add("Valoare implicită", k_col.DefaultValue)
        k_add("Extra", k_col.Extra)
        k_add("Colație", k_col.Collation)
        Dim k_pkPosition As Integer = _current.PrimaryKey.IndexOf(k_col.Name)
        k_add("Cheie primară", If(k_pkPosition < 0, "Nu", "Da" & If(_current.PrimaryKey.Count > 1, " (poziția " & (k_pkPosition + 1).ToString(CultureInfo.CurrentCulture) & " din " & _current.PrimaryKey.Count.ToString(CultureInfo.CurrentCulture) & ")", String.Empty)))
        Dim k_fks As List(Of DbForeignKey) = Nothing
        If _fkByColumn.TryGetValue(k_col.Name, k_fks) Then
            k_add("Cheie străină", "Da")
            k_add("Legată cu", String.Join("; ", k_fks.Select(Function(k_fk) QualifiedTable(k_fk) & "." & k_fk.RefColumns(k_fk.Columns.IndexOf(k_col.Name)))))
            k_add("La modificare / ștergere", String.Join("; ", k_fks.Select(Function(k_fk) k_fk.OnUpdate & " / " & k_fk.OnDelete)))
            k_add("Constrângere", String.Join("; ", k_fks.Select(Function(k_fk) k_fk.Name)))
        Else
            k_add("Cheie străină", "Nu")
        End If
        Dim k_refs As List(Of DbReference) = Nothing
        If _refByColumn.TryGetValue(k_col.Name, k_refs) Then
            k_add("Referită de", String.Join("; ", k_refs.Select(Function(k_r) k_r.Table & "." & k_r.Columns(k_r.RefColumns.IndexOf(k_col.Name)))))
        End If
        Dim k_indexes As List(Of DbIndexInfo) = Nothing
        If _indexByColumn.TryGetValue(k_col.Name, k_indexes) Then k_add("Indexuri", String.Join("; ", k_indexes.Select(Function(k_x) k_x.Name & If(k_x.Unique, " (unic)", String.Empty))))
        k_add("Comentariu", k_col.Comment)
        FillKvp(gridColumn, k_items)
    End Sub

    Private Shared Sub FillKvp(k_grid As KBotDataView, k_items As List(Of KeyValuePair(Of String, String)))
        k_grid.BeginUpdate()
        Try
            k_grid.ClearRows()
            For Each k_item As KeyValuePair(Of String, String) In k_items
                Dim k_row As KBotDataRow = k_grid.AddRow()
                k_row(KeyCol) = k_item.Key
                k_row(ValCol) = OneLine(k_item.Value)
                k_row.Tag = k_item.Value
            Next
        Finally
            k_grid.EndUpdate()
        End Try
    End Sub

    ''' <summary>A click in a fact grid puts its value in the box under the rows grid, where a long list can be read whole.</summary>
    Private Sub ShowFact(k_grid As KBotDataView, k_rowIndex As Integer)
        If k_rowIndex < 0 OrElse k_rowIndex >= k_grid.RowCount Then Return
        _cellShown = String.Empty
        edCell.TextBox.Text = Convert.ToString(k_grid.Rows(k_rowIndex).Tag, CultureInfo.CurrentCulture)
    End Sub

    Private Sub GridTable_CellClick(k_sender As Object, e As KBotCellEventArgs) Handles gridTable.CellClick
        Try
            ShowFact(gridTable, e.RowIndex)
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.GridTable_CellClick", ex)
        End Try
    End Sub

    Private Sub GridColumn_CellClick(k_sender As Object, e As KBotCellEventArgs) Handles gridColumn.CellClick
        Try
            ShowFact(gridColumn, e.RowIndex)
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.GridColumn_CellClick", ex)
        End Try
    End Sub

    Private Shared Function FormatStamp(k_iso As String) As String
        Dim k_date As DateTime
        If Not String.IsNullOrEmpty(k_iso) AndAlso DateTime.TryParse(k_iso, CultureInfo.InvariantCulture, DateTimeStyles.None, k_date) Then
            Return k_date.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)
        End If
        Return String.Empty
    End Function

    ' ------------------------------------------------------------------
    ' The columns of a query (the list on the right, instead of the facts about one column)
    ' ------------------------------------------------------------------

    ''' <summary>The list of the columns of the query takes the place of the facts about the chosen column while the rows grid shows a query.</summary>
    Private Sub SyncSidePanel()
        gridQueryColumns.Visible = _queryMode
        gridColumn.Visible = Not _queryMode
    End Sub

    ''' <summary>Fills the list on the right with the columns of the query result: a tick «Fix», the name, the type. The ticks are the ones kept for this query.</summary>
    Private Sub FillQueryColumns(k_result As DbTableResult)
        Dim k_locked As HashSet(Of String) = Nothing
        _queryLocks.TryGetValue(_queryId, k_locked)
        _filling = True
        gridQueryColumns.BeginUpdate()
        Try
            gridQueryColumns.ClearRows()
            For Each k_col As DbColumnInfo In k_result.Columns
                Dim k_row As KBotDataRow = gridQueryColumns.AddRow()
                k_row("on") = k_locked IsNot Nothing AndAlso k_locked.Contains(k_col.Name)
                k_row("name") = k_col.Name
                k_row("type") = k_col.ColumnType
                k_row.Tag = k_col.Name
            Next
        Finally
            gridQueryColumns.EndUpdate()
            _filling = False
        End Try
        SyncSidePanel()
    End Sub

    ''' <summary>
    ''' A tick in the list: the column joins (or leaves) the ones kept on the left. The locks add up, in any number. The grid has no way to move a column, so it is built
    ''' again with the ticked ones first (what the operator sorted or filtered in the rows grid is lost, the rows themselves are not read again).
    ''' </summary>
    Private Sub GridQueryColumns_CellValueChanged(k_sender As Object, e As KBotCellValueEventArgs) Handles gridQueryColumns.CellValueChanged
        Try
            If _filling OrElse Not _queryMode OrElse _current Is Nothing OrElse e.ColumnKey <> "on" OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridQueryColumns.RowCount Then Return
            Dim k_name As String = Convert.ToString(gridQueryColumns.Rows(e.RowIndex).Tag, CultureInfo.InvariantCulture)
            Dim k_locked As HashSet(Of String) = Nothing
            If Not _queryLocks.TryGetValue(_queryId, k_locked) Then
                k_locked = New HashSet(Of String)(StringComparer.Ordinal)
                _queryLocks(_queryId) = k_locked
            End If
            If Convert.ToBoolean(e.NewValue, CultureInfo.InvariantCulture) Then k_locked.Add(k_name) Else k_locked.Remove(k_name)
            TableNavLog.Write("QUERY", _queryId & ": locked on the left now: " & String.Join(", ", k_locked))
            ShowTable(_current)
        Catch ex As Exception
            GlobalErrorLog.Write("DbBrowserView.GridQueryColumns_CellValueChanged", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Clearing
    ' ------------------------------------------------------------------

    Private Sub ShowHint(k_text As String)
        btnMore.Enabled = False
        lblInfo.Text = k_text
    End Sub

    ''' <summary>No usable database (all databases chosen): nothing is shown and the page says why.</summary>
    Private Sub ClearAll(k_hint As String)
        _loadedDb = String.Empty
        treeTables.Clear()
        ClearGrids()
        ShowHint(k_hint)
    End Sub

    Private Sub ClearGrids()
        _request += 1
        _current = Nothing
        _queryMode = False
        _queryId = String.Empty
        _queryCutNote = String.Empty
        _lastQuery = Nothing
        _lastQueryValues = Nothing
        gridQueryColumns.ClearRows()
        SyncSidePanel()
        _linkColumns.Clear()
        If _node IsNot Nothing Then TableNavLog.Write("CLEAR", "grids cleared (database changed or none usable): the history of " & _node.Caption & " is dropped")
        _node = Nothing
        UpdateNavButtons()
        _tableName = String.Empty
        _columnShown = String.Empty
        _cellShown = String.Empty
        _loadedRows = 0
        _hasMore = False
        gridData.BeginUpdate()
        Try
            gridData.ClearAllFilters()
            gridData.ApplySort(Nothing, KBotSortDirection.None)
            gridData.ClearRows()
            gridData.Columns.Clear()
        Finally
            gridData.EndUpdate()
        End Try
        gridTable.ClearRows()
        gridColumn.ClearRows()
        edCell.TextBox.Text = String.Empty
    End Sub

End Class
