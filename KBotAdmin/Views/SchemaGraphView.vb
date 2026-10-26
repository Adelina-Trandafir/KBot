Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' The «Schema bazelor» page: the tables of the server schemas SURSA and COMUN as a graph (a dot per table, a line per foreign key), read from
''' the dumps kept in <c>MariaDB_Schema\</c> (<see cref="SchemaScanner"/>). Nothing is sent to the server and nothing is written. Tables are
''' clustered and coloured by the prefix of their name inside each schema; hovering a table fades everything but the tables it is linked to.
''' </summary>
Public Class SchemaGraphView
    Implements IAdminView

    Private Const AllSchemas As Integer = 0

    Private _context As AdminContext
    Private _corpus As SchemaCorpus
    Private ReadOnly _groupIndex As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
    Private ReadOnly _schemaNames As New List(Of String)()
    Private ReadOnly _familyNames As New List(Of String)()
    Private _filling As Boolean

    Public Sub New()
        InitializeComponent()
        cmbSchema.Items.Add("Ambele")
        cmbSchema.SelectedIndex = AllSchemas
        cmbFamily.Items.Add("Toate")
        cmbFamily.SelectedIndex = 0
        _familyNames.Add(String.Empty)
        ' The label is for reading: it never times out. It goes only when the pointer leaves its dot.
        graphView.PointTooltip.AutoPopDelay = 0
        lblDetail.Text = "Pune cursorul pe un tabel ca să-i vezi coloanele."
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        If _corpus IsNot Nothing Then Return
        Await UiRun.Guard("SchemaGraphView.ActivatedAsync", _context, AddressOf LoadAsync)
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' The dumps do not belong to a database chosen in the tree.
    End Sub

    ' ------------------------------------------------------------------
    ' Loading
    ' ------------------------------------------------------------------

    Private Async Function LoadAsync() As Task
        _context.Say("Se citesc dump-urile schemei...")
        Dim k_folder As String = SchemaScanner.FindFolder(AdminSettingsStore.Load().EffectiveLocalRoot())
        If k_folder.Length = 0 Then Throw New AdminException("Nu găsesc folderul MariaDB_Schema lângă KBot.sln. Folderul e pe discul tău, nu vine din git.")
        Dim k_corpus As SchemaCorpus = Await Task.Run(Function() SchemaScanner.Scan(k_folder))
        _corpus = k_corpus

        _groupIndex.Clear()
        Dim k_groups As List(Of String) = k_corpus.Tables.Select(Function(k_t) k_t.GroupKey).Distinct(StringComparer.Ordinal).OrderBy(Function(k_g) k_g, StringComparer.Ordinal).ToList()
        For k_i As Integer = 0 To k_groups.Count - 1
            _groupIndex(k_groups(k_i)) = k_i
        Next
        FillSchemas()
        FillFamilies()
        lblDetail.Text = "Pune cursorul pe un tabel ca să-i vezi coloanele."
        Await BuildGraphAsync()

        Dim k_sources As String = String.Join(", ", k_corpus.Sources.Select(Function(k_p) k_p.Key & " din " & k_p.Value.Item2.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)))
        _context.Say("Gata: " & k_corpus.Tables.Count.ToString(CultureInfo.InvariantCulture) & " tabele, " &
                     k_corpus.Links.Count.ToString(CultureInfo.InvariantCulture) & " legături (dump-uri: " & k_sources & ")." &
                     If(k_corpus.UnresolvedKeys > 0, " " & k_corpus.UnresolvedKeys.ToString(CultureInfo.InvariantCulture) & " chei străine arată spre tabele din afara dump-urilor.", String.Empty))
    End Function

    Private Sub FillSchemas()
        Dim k_previous As String = SelectedSchema()
        _filling = True
        Try
            cmbSchema.Items.Clear()
            _schemaNames.Clear()
            cmbSchema.Items.Add("Ambele (" & _corpus.Tables.Count.ToString(CultureInfo.InvariantCulture) & ")")
            _schemaNames.Add(String.Empty)
            Dim k_keep As Integer = 0
            For Each k_group In _corpus.Tables.GroupBy(Function(k_t) k_t.Schema).OrderBy(Function(k_g) k_g.Key, StringComparer.Ordinal)
                cmbSchema.Items.Add(k_group.Key & " (" & k_group.Count().ToString(CultureInfo.InvariantCulture) & ")")
                _schemaNames.Add(k_group.Key)
                If String.Equals(k_group.Key, k_previous, StringComparison.Ordinal) Then k_keep = _schemaNames.Count - 1
            Next
            cmbSchema.SelectedIndex = k_keep
        Finally
            _filling = False
        End Try
    End Sub

    Private Function SelectedSchema() As String
        Dim k_index As Integer = cmbSchema.SelectedIndex
        If k_index <= 0 OrElse k_index >= _schemaNames.Count Then Return String.Empty
        Return _schemaNames(k_index)
    End Function

    ''' <summary>The families (name prefixes) of the tables of the schema chosen, each with its number of tables; the choice made before is kept when it still exists.</summary>
    Private Sub FillFamilies()
        Dim k_previous As String = SelectedFamily()
        Dim k_schema As String = SelectedSchema()
        _filling = True
        Try
            cmbFamily.Items.Clear()
            _familyNames.Clear()
            Dim k_tables As List(Of SchemaTable) = _corpus.Tables.Where(Function(k_t) k_schema.Length = 0 OrElse String.Equals(k_t.Schema, k_schema, StringComparison.Ordinal)).ToList()
            cmbFamily.Items.Add("Toate (" & k_tables.Count.ToString(CultureInfo.InvariantCulture) & ")")
            _familyNames.Add(String.Empty)
            Dim k_keep As Integer = 0
            For Each k_group In k_tables.GroupBy(Function(k_t) k_t.Prefix).OrderByDescending(Function(k_g) k_g.Count()).ThenBy(Function(k_g) k_g.Key, StringComparer.Ordinal)
                cmbFamily.Items.Add(k_group.Key & " (" & k_group.Count().ToString(CultureInfo.InvariantCulture) & ")")
                _familyNames.Add(k_group.Key)
                If String.Equals(k_group.Key, k_previous, StringComparison.Ordinal) Then k_keep = _familyNames.Count - 1
            Next
            cmbFamily.SelectedIndex = k_keep
        Finally
            _filling = False
        End Try
    End Sub

    Private Function SelectedFamily() As String
        Dim k_index As Integer = cmbFamily.SelectedIndex
        If k_index <= 0 OrElse k_index >= _familyNames.Count Then Return String.Empty
        Return _familyNames(k_index)
    End Function

    Private Async Sub CmbSchema_SelectedIndexChanged(k_sender As Object, e As EventArgs) Handles cmbSchema.SelectedIndexChanged
        Try
            If _filling OrElse _corpus Is Nothing Then Return
            FillFamilies()
            Await UiRun.Guard("SchemaGraphView.CmbSchema_SelectedIndexChanged", _context, AddressOf BuildGraphAsync)
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaGraphView.CmbSchema_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Async Sub CmbFamily_SelectedIndexChanged(k_sender As Object, e As EventArgs) Handles cmbFamily.SelectedIndexChanged
        Try
            If _filling OrElse _corpus Is Nothing Then Return
            Await UiRun.Guard("SchemaGraphView.CmbFamily_SelectedIndexChanged", _context, AddressOf BuildGraphAsync)
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaGraphView.CmbFamily_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Async Sub ChkLinked_CheckedChanged(k_sender As Object, e As EventArgs) Handles chkLinked.CheckedChanged
        Try
            If _corpus Is Nothing Then Return
            Await UiRun.Guard("SchemaGraphView.ChkLinked_CheckedChanged", _context, AddressOf BuildGraphAsync)
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaGraphView.ChkLinked_CheckedChanged", ex)
        End Try
    End Sub

    Private Async Sub BtnReload_Click(k_sender As Object, e As EventArgs) Handles btnReload.Click
        Try
            Await UiRun.Guard("SchemaGraphView.BtnReload_Click", _context, AddressOf LoadAsync)
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaGraphView.BtnReload_Click", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' The graph
    ' ------------------------------------------------------------------

    Private Async Function BuildGraphAsync() As Task
        Dim k_schema As String = SelectedSchema()
        Dim k_family As String = SelectedFamily()
        Dim k_inSchema As List(Of SchemaTable) = _corpus.Tables.Where(Function(k_t) (k_schema.Length = 0 OrElse String.Equals(k_t.Schema, k_schema, StringComparison.Ordinal)) AndAlso
                                                                         (k_family.Length = 0 OrElse String.Equals(k_t.Prefix, k_family, StringComparison.Ordinal))).ToList()
        Dim k_tables As List(Of SchemaTable) = If(chkLinked.Checked,
                                                  k_inSchema.Where(Function(k_t) k_t.RefersTo.Count > 0 OrElse k_t.ReferencedBy.Count > 0).ToList(),
                                                  k_inSchema)
        Dim k_position As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For k_i As Integer = 0 To k_tables.Count - 1
            k_position(k_tables(k_i).Key) = k_i
        Next
        Dim k_groupOf(k_tables.Count - 1) As Integer
        For k_i As Integer = 0 To k_tables.Count - 1
            k_groupOf(k_i) = _groupIndex(k_tables(k_i).GroupKey)
        Next
        Dim k_links As List(Of SchemaLink) = _corpus.Links.Where(Function(k_l) k_position.ContainsKey(k_l.FromTable.Key) AndAlso k_position.ContainsKey(k_l.ToTable.Key)).ToList()
        Dim k_from(k_links.Count - 1) As Integer
        Dim k_to(k_links.Count - 1) As Integer
        For k_i As Integer = 0 To k_links.Count - 1
            k_from(k_i) = k_position(k_links(k_i).FromTable.Key)
            k_to(k_i) = k_position(k_links(k_i).ToTable.Key)
        Next

        _context.Say("Se așază tabelele pe graf (" & k_tables.Count.ToString(CultureInfo.InvariantCulture) & " puncte, " & k_links.Count.ToString(CultureInfo.InvariantCulture) & " legături)...")
        Dim k_positions As PointF() = Await Task.Run(Function() KBotGraphLayout.Compute(k_groupOf, k_from, k_to, 140))

        graphView.BeginUpdate()
        Try
            graphView.ClearGraph()
            For k_i As Integer = 0 To k_tables.Count - 1
                Dim k_table As SchemaTable = k_tables(k_i)
                Dim k_node As KBotGraphNode = graphView.AddNode(k_table.Key, k_table.Name)
                k_node.Group = k_table.GroupKey
                k_node.NodeColor = graphView.AutoColor(k_groupOf(k_i))
                k_node.Tooltip = TableTip(k_table)
                k_node.Tag = k_table
                k_node.X = k_positions(k_i).X
                k_node.Y = k_positions(k_i).Y
                k_node.Radius = 3 + Math.Min(6, CInt(Math.Floor(Math.Log(1 + k_table.RefersTo.Count + k_table.ReferencedBy.Count, 2))))
            Next
            For Each k_link As SchemaLink In k_links
                graphView.AddEdge(k_link.FromTable.Key, k_link.ToTable.Key)
            Next
        Finally
            graphView.EndUpdate()
        End Try
        graphView.FitToView()
    End Function

    Private Sub GraphView_NodeHovered(k_key As String) Handles graphView.NodeHovered
        Try
            If k_key Is Nothing Then Return
            ShowDetail(DirectCast(graphView.FindNode(k_key).Tag, SchemaTable))
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaGraphView.GraphView_NodeHovered", ex)
        End Try
    End Sub

    Private Sub GraphView_NodeClicked(k_key As String) Handles graphView.NodeClicked
        Try
            If k_key Is Nothing Then Return
            ShowDetail(DirectCast(graphView.FindNode(k_key).Tag, SchemaTable))
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaGraphView.GraphView_NodeClicked", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Labels
    ' ------------------------------------------------------------------

    Private Sub ShowDetail(k_table As SchemaTable)
        Dim k_text As New StringBuilder()
        k_text.Append(k_table.Name).Append(" (").Append(k_table.Schema).Append(") · ")
        k_text.Append(k_table.Columns.Count.ToString(CultureInfo.InvariantCulture)).Append(" coloane")
        Dim k_primary As String = String.Join(", ", k_table.PrimaryKeyColumns)
        If k_primary.Length > 0 Then k_text.Append(" · cheie primară: ").Append(k_primary)
        k_text.Append(ControlChars.Lf)
        k_text.Append("spre: ").Append(NameList(k_table.RefersTo)).Append(" · referit de: ").Append(NameList(k_table.ReferencedBy))
        lblDetail.Text = k_text.ToString()
    End Sub

    Private Shared Function NameList(k_tables As List(Of SchemaTable)) As String
        If k_tables.Count = 0 Then Return "nimic"
        Dim k_names As String = String.Join(", ", k_tables.Take(8).Select(Function(k_t) k_t.Name))
        Return If(k_tables.Count > 8, k_names & " (+" & (k_tables.Count - 8).ToString(CultureInfo.InvariantCulture) & ")", k_names)
    End Function

    ''' <summary>The floating label of a table: its columns (key columns bold, foreign-key columns with the table they point at).</summary>
    Private Shared Function TableTip(k_table As SchemaTable) As String
        Dim k_target As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each k_key As SchemaForeignKey In k_table.ForeignKeys
            For Each k_column As String In k_key.Columns
                If Not k_target.ContainsKey(k_column) Then k_target(k_column) = k_key.RefTable
            Next
        Next

        Dim k_tip As New StringBuilder()
        k_tip.Append("<b>").Append(Plain(k_table.Name)).Append("</b>").Append(ControlChars.Lf)
        k_tip.Append("<i>").Append(k_table.Schema).Append(" · ").Append(k_table.Columns.Count.ToString(CultureInfo.InvariantCulture)).Append(" coloane · ")
        k_tip.Append(k_table.ForeignKeys.Count.ToString(CultureInfo.InvariantCulture)).Append(" chei străine · ")
        k_tip.Append(k_table.ReferencedBy.Count.ToString(CultureInfo.InvariantCulture)).Append(" tabele îl referă</i>").Append(ControlChars.Lf)
        Const MaxColumns As Integer = 16
        For Each k_column As SchemaColumn In k_table.Columns.Take(MaxColumns)
            k_tip.Append(ControlChars.Lf)
            If k_column.IsPrimary Then
                k_tip.Append("<b>").Append(Plain(k_column.Name)).Append("</b>")
            Else
                k_tip.Append(Plain(k_column.Name))
            End If
            k_tip.Append("  <i>").Append(Plain(k_column.TypeText)).Append("</i>")
            Dim k_ref As String = Nothing
            If k_target.TryGetValue(k_column.Name, k_ref) Then k_tip.Append(" → ").Append(Plain(k_ref))
        Next
        If k_table.Columns.Count > MaxColumns Then
            k_tip.Append(ControlChars.Lf).Append("<i>... încă ").Append((k_table.Columns.Count - MaxColumns).ToString(CultureInfo.InvariantCulture)).Append(" coloane</i>")
        End If
        Return k_tip.ToString()
    End Function

    Private Shared Function Plain(k_text As String) As String
        Return k_text.Replace("<"c, " "c).Replace(">"c, " "c)
    End Function

End Class
