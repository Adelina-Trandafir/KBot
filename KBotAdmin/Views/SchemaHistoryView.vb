Option Strict On
Imports System.Globalization
Imports System.Text

''' <summary>
''' The «Evoluția schemei» page: what the schema sync changed in one database over time. The source is the server's own journal
''' <c>schema_diff_log</c> (read from the COMUN dump in <c>MariaDB_Schema\</c>, see <see cref="SchemaLogScanner"/>), which records every DDL
''' statement with the moment it ran. A lane per table, a dot per statement: full = it ran, hollow = planned but not run or failed, cross = a DROP.
''' Nothing is sent to the server and nothing is written.
''' </summary>
Public Class SchemaHistoryView
    Implements IAdminView

    Private Const DefaultDb As String = "000_DEMO"

    Private _context As AdminContext
    Private _all As List(Of SchemaChange)
    Private ReadOnly _dbNames As New List(Of String)()
    Private ReadOnly _familyNames As New List(Of String)()
    Private _filling As Boolean

    ''' <summary>The kinds the «Arată» list offers: caption and the object types of the journal it keeps.</summary>
    Private Shared ReadOnly Kinds As (Caption As String, Types As String())() = {
        ("Toate", Nothing),
        ("Tabele create sau șterse", New String() {"TABLE"}),
        ("Coloane", New String() {"COLUMN"}),
        ("Indexuri și chei", New String() {"INDEX", "PK", "FK"}),
        ("Altele (colații, curățări)", New String() {"COLLATION", "RENAME_CLEANUP"})
    }

    Public Sub New()
        InitializeComponent()
        For Each k_kind In Kinds
            cmbKind.Items.Add(k_kind.Caption)
        Next
        cmbKind.SelectedIndex = 0
        cmbFamily.Items.Add("Toate")
        cmbFamily.SelectedIndex = 0
        _familyNames.Add(String.Empty)
        ' The label is for reading: it never times out. It goes only when the pointer leaves its dot.
        laneView.MarkerTooltip.AutoPopDelay = 0
        lblDetail.Text = "Pune cursorul pe un punct ca să vezi instrucțiunea."
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        If _all IsNot Nothing Then Return
        Await UiRun.Guard("SchemaHistoryView.ActivatedAsync", _context, AddressOf LoadAsync)
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' The journal covers every database; the one shown is chosen on this page.
    End Sub

    ' ------------------------------------------------------------------
    ' Loading
    ' ------------------------------------------------------------------

    Private Async Function LoadAsync() As Task
        _context.Say("Se citește jurnalul schema_diff_log din dump-ul COMUN...")
        Dim k_folder As String = SchemaScanner.FindFolder(AdminSettingsStore.Load().EffectiveLocalRoot())
        If k_folder.Length = 0 Then Throw New AdminException("Nu găsesc folderul MariaDB_Schema lângă KBot.sln. Folderul e pe discul tău, nu vine din git.")
        Dim k_skipped As Integer = 0
        Dim k_written As Date = Date.MinValue
        Dim k_rows As List(Of SchemaChange) = Await Task.Run(Function() SchemaLogScanner.Read(k_folder, k_skipped, k_written))
        _all = k_rows
        FillDatabases()
        FillFamilies()
        BuildLanes()
        Dim k_last As String = If(k_rows.Count = 0, "-", k_rows(k_rows.Count - 1).Moment.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture))
        _context.Say("Gata: " & k_rows.Count.ToString(CultureInfo.InvariantCulture) & " instrucțiuni în jurnal, ultima din " & k_last &
                     "; dump-ul e din " & k_written.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture) & "." &
                     If(k_skipped > 0, " " & k_skipped.ToString(CultureInfo.InvariantCulture) & " rânduri n-au putut fi citite.", String.Empty))
    End Function

    ''' <summary>The databases of the journal, with their number of statements; 000_DEMO is first and chosen unless the operator had picked another.</summary>
    Private Sub FillDatabases()
        Dim k_previous As String = SelectedDb()
        _filling = True
        Try
            cmbDb.Items.Clear()
            _dbNames.Clear()
            Dim k_groups = _all.GroupBy(Function(k_c) k_c.TargetDb).OrderBy(Function(k_g) If(k_g.Key = DefaultDb, 0, 1)).ThenBy(Function(k_g) k_g.Key, StringComparer.Ordinal).ToList()
            Dim k_keep As Integer = 0
            For Each k_group In k_groups
                cmbDb.Items.Add(k_group.Key & " (" & k_group.Count().ToString(CultureInfo.InvariantCulture) & ")")
                _dbNames.Add(k_group.Key)
                If k_previous.Length > 0 AndAlso String.Equals(k_group.Key, k_previous, StringComparison.Ordinal) Then k_keep = _dbNames.Count - 1
            Next
            If _dbNames.Count > 0 Then cmbDb.SelectedIndex = k_keep
        Finally
            _filling = False
        End Try
    End Sub

    Private Function SelectedDb() As String
        Dim k_index As Integer = cmbDb.SelectedIndex
        If k_index < 0 OrElse k_index >= _dbNames.Count Then Return String.Empty
        Return _dbNames(k_index)
    End Function

    ''' <summary>The families (name prefixes) of the tables changed in the chosen database, each with its number of statements; the choice made before is kept when it still exists.</summary>
    Private Sub FillFamilies()
        Dim k_previous As String = SelectedFamily()
        Dim k_db As String = SelectedDb()
        _filling = True
        Try
            cmbFamily.Items.Clear()
            _familyNames.Clear()
            Dim k_rows As List(Of SchemaChange) = _all.Where(Function(k_c) String.Equals(k_c.TargetDb, k_db, StringComparison.Ordinal)).ToList()
            cmbFamily.Items.Add("Toate (" & k_rows.Count.ToString(CultureInfo.InvariantCulture) & ")")
            _familyNames.Add(String.Empty)
            Dim k_keep As Integer = 0
            For Each k_group In k_rows.GroupBy(Function(k_c) SchemaScanner.PrefixOf(k_c.TableName)).OrderByDescending(Function(k_g) k_g.Count()).ThenBy(Function(k_g) k_g.Key, StringComparer.Ordinal)
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

    Private Sub CmbDb_SelectedIndexChanged(k_sender As Object, e As EventArgs) Handles cmbDb.SelectedIndexChanged
        Try
            If _filling OrElse _all Is Nothing Then Return
            FillFamilies()
            BuildLanes()
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaHistoryView.CmbDb_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub Filter_Changed(k_sender As Object, e As EventArgs) Handles cmbKind.SelectedIndexChanged, cmbFamily.SelectedIndexChanged
        Try
            If _filling OrElse _all Is Nothing Then Return
            BuildLanes()
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaHistoryView.Filter_Changed", ex)
        End Try
    End Sub

    Private Async Sub BtnReload_Click(k_sender As Object, e As EventArgs) Handles btnReload.Click
        Try
            Await UiRun.Guard("SchemaHistoryView.BtnReload_Click", _context, AddressOf LoadAsync)
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaHistoryView.BtnReload_Click", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' A lane per table
    ' ------------------------------------------------------------------

    Private Sub BuildLanes()
        Dim k_db As String = SelectedDb()
        Dim k_types As String() = Kinds(Math.Max(0, cmbKind.SelectedIndex)).Types
        Dim k_family As String = SelectedFamily()
        Dim k_rows As List(Of SchemaChange) = _all.Where(Function(k_c) String.Equals(k_c.TargetDb, k_db, StringComparison.Ordinal) AndAlso
                                                            (k_types Is Nothing OrElse k_types.Contains(k_c.ObjectType, StringComparer.Ordinal)) AndAlso
                                                            (k_family.Length = 0 OrElse String.Equals(SchemaScanner.PrefixOf(k_c.TableName), k_family, StringComparison.Ordinal))).ToList()
        Dim k_byTable As List(Of IGrouping(Of String, SchemaChange)) = k_rows.GroupBy(Function(k_c) If(k_c.TableName.Length = 0, "(fără tabel)", k_c.TableName), StringComparer.Ordinal).
            OrderBy(Function(k_g) k_g.Min(Function(k_c) k_c.Moment)).ThenBy(Function(k_g) k_g.Key, StringComparer.Ordinal).ToList()

        laneView.BeginUpdate()
        Try
            laneView.ClearLanes()
            For k_i As Integer = 0 To k_byTable.Count - 1
                Dim k_group As IGrouping(Of String, SchemaChange) = k_byTable(k_i)
                Dim k_lane As KBotLane = laneView.AddLane(k_group.Key, k_group.Key)
                k_lane.LaneColor = laneView.AutoColor(k_i)
                k_lane.Tooltip = "<b>" & Plain(k_group.Key) & "</b>" & ControlChars.Lf &
                                 k_group.Count().ToString(CultureInfo.InvariantCulture) & " instrucțiuni · de la " & DateText(k_group.Min(Function(k_c) k_c.Moment)) &
                                 " la " & DateText(k_group.Max(Function(k_c) k_c.Moment))
                For Each k_change As SchemaChange In k_group
                    Dim k_marker As KBotLaneMarker = k_lane.AddMarker(k_change.Moment, Caption(k_change))
                    k_marker.Style = If(Not k_change.Executed, KBotLaneMarkerStyle.NoChange,
                                        If(String.Equals(k_change.ActionType, "DROP", StringComparison.Ordinal), KBotLaneMarkerStyle.Deletion, KBotLaneMarkerStyle.Normal))
                    k_marker.Tooltip = ChangeTip(k_change)
                    k_marker.Tag = k_change
                Next
            Next
        Finally
            laneView.EndUpdate()
        End Try
    End Sub

    Private Sub LaneView_MarkerHovered(k_laneKey As String, k_markerIndex As Integer) Handles laneView.MarkerHovered
        Try
            If k_laneKey Is Nothing OrElse k_markerIndex < 0 Then Return
            Dim k_lane As KBotLane = laneView.FindLane(k_laneKey)
            If k_lane Is Nothing OrElse k_markerIndex >= k_lane.Markers.Count Then Return
            Dim k_change As SchemaChange = TryCast(k_lane.Markers(k_markerIndex).Tag, SchemaChange)
            If k_change Is Nothing Then Return
            lblDetail.Text = Caption(k_change) & " · " & k_change.TargetDb & "." & k_change.TableName & " · " & StateText(k_change) & ControlChars.Lf & FirstLine(k_change.Ddl)
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaHistoryView.LaneView_MarkerHovered", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' Labels
    ' ------------------------------------------------------------------

    Private Shared Function Caption(k_change As SchemaChange) As String
        Return k_change.ActionType & " " & k_change.ObjectType & If(k_change.ObjectName.Length > 0, " " & k_change.ObjectName, String.Empty)
    End Function

    Private Shared Function StateText(k_change As SchemaChange) As String
        Dim k_text As New StringBuilder()
        If k_change.ErrorText.Length > 0 Then
            k_text.Append("a eșuat")
        ElseIf k_change.ExecutedAt = Date.MinValue Then
            k_text.Append("neexecutat (planificat ").Append(k_change.CreatedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)).Append(")")
        Else
            k_text.Append("executat ").Append(k_change.ExecutedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture))
        End If
        k_text.Append(" · ").Append(k_change.SyncMode)
        If k_change.Destructive Then k_text.Append(" · distructiv")
        Return k_text.ToString()
    End Function

    ''' <summary>The floating label: what it was, where and when, and the start of the statement (italic, so the text of the SQL is not read as markup).</summary>
    Private Shared Function ChangeTip(k_change As SchemaChange) As String
        Dim k_tip As New StringBuilder()
        k_tip.Append("<b>").Append(Plain(Caption(k_change))).Append("</b>").Append(ControlChars.Lf)
        k_tip.Append("<i>").Append(Plain(k_change.TargetDb)).Append(".").Append(Plain(k_change.TableName)).Append("</i>").Append(ControlChars.Lf)
        k_tip.Append(StateText(k_change))
        Const MaxLines As Integer = 10
        Dim k_lines As String() = k_change.Ddl.Replace(vbCr, String.Empty).Split(ControlChars.Lf)
        k_tip.Append(ControlChars.Lf)
        For Each k_line As String In k_lines.Take(MaxLines)
            Dim k_clipped As String = If(k_line.Length > 110, k_line.Substring(0, 110) & "...", k_line)
            k_tip.Append(ControlChars.Lf).Append("<i>").Append(Plain(k_clipped)).Append("</i>")
        Next
        If k_lines.Length > MaxLines Then k_tip.Append(ControlChars.Lf).Append("<i>... încă ").Append((k_lines.Length - MaxLines).ToString(CultureInfo.InvariantCulture)).Append(" rânduri</i>")
        If k_change.ErrorText.Length > 0 Then k_tip.Append(ControlChars.Lf).Append(ControlChars.Lf).Append("Eroare: ").Append(Plain(If(k_change.ErrorText.Length > 200, k_change.ErrorText.Substring(0, 200) & "...", k_change.ErrorText)))
        Return k_tip.ToString()
    End Function

    Private Shared Function FirstLine(k_text As String) As String
        Dim k_line As String = k_text.Replace(vbCr, String.Empty).Split(ControlChars.Lf)(0)
        Return If(k_line.Length > 160, k_line.Substring(0, 160) & "...", k_line)
    End Function

    Private Shared Function DateText(k_date As Date) As String
        Return k_date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
    End Function

    Private Shared Function Plain(k_text As String) As String
        Return k_text.Replace("<"c, " "c).Replace(">"c, " "c)
    End Function

End Class
