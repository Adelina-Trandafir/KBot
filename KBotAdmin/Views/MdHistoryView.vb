Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>
''' The «Istoric md» page: how the project evolved, read ONLY from its markdown files. Two views of the same scan (<see cref="MdScanner"/>):
''' a lane per slice with a dot per file on a time axis, and a graph of files and the links between them with a time cursor.
''' Nothing is written; double click (or «Deschide fișierul») opens a file in the program that reads .md on this PC.
''' </summary>
Public Class MdHistoryView
    Implements IAdminView

    Private Const ModeTimeline As Integer = 0
    Private Const ModeGraph As Integer = 1

    Private _context As AdminContext
    Private _corpus As MdCorpus
    Private _byPath As New Dictionary(Of String, MdFile)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _familyNames As New List(Of String)()
    Private ReadOnly _groupIndex As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
    Private _selected As MdFile
    Private _timelineStale As Boolean = True
    Private _graphStale As Boolean = True
    Private _filling As Boolean
    Private _cursorStart As Date = Date.MinValue

    Public Sub New()
        InitializeComponent()
        cmbMode.Items.Add("Cronologie pe felii")
        cmbMode.Items.Add("Graf în timp")
        cmbMode.SelectedIndex = ModeTimeline
        cmbFamily.Items.Add("Toate")
        cmbFamily.SelectedIndex = 0
        _familyNames.Add(String.Empty)
        tlyMain.RowStyles(2).Height = 0F
        ' The preview is for reading: it never times out. It goes only when the pointer leaves its dot.
        laneView.MarkerTooltip.AutoPopDelay = 0
        graphView.PointTooltip.AutoPopDelay = 0
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Async Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        If _corpus IsNot Nothing Then Return
        Await UiRun.Guard("MdHistoryView.ActivatedAsync", _context, AddressOf LoadAsync)
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ' The files do not belong to a database.
    End Sub

    ' ------------------------------------------------------------------
    ' Loading
    ' ------------------------------------------------------------------

    Private Async Function LoadAsync() As Task
        _context.Say("Se citesc fișierele md și istoricul lor din git...")
        Dim k_root As String = MdScanner.FindRoot(AdminSettingsStore.Load().EffectiveLocalRoot())
        If k_root.Length = 0 Then Throw New AdminException("Nu găsesc dosarul proiectului (KBot.sln) lângă program.")
        Dim k_corpus As MdCorpus = Await Task.Run(Function() MdScanner.Scan(k_root))
        _corpus = k_corpus
        _byPath = k_corpus.Files.ToDictionary(Function(k_f) k_f.RelPath, StringComparer.OrdinalIgnoreCase)
        _groupIndex.Clear()
        For k_i As Integer = 0 To k_corpus.Groups.Count - 1
            _groupIndex(k_corpus.Groups(k_i).Key) = k_i
        Next
        FillFamilies()
        _timelineStale = True
        _graphStale = True
        _selected = Nothing
        ShowDetail(Nothing)
        Await RefreshCurrentAsync()
        _context.Say("Gata: " & k_corpus.Files.Count.ToString(CultureInfo.InvariantCulture) & " fișiere md, " &
                     k_corpus.Links.Count.ToString(CultureInfo.InvariantCulture) & " legături, " &
                     k_corpus.Groups.Count.ToString(CultureInfo.InvariantCulture) & " felii și dosare (dată din " & k_corpus.Source & ").")
    End Function

    ''' <summary>The family list with the number of files in each; the choice made before is kept when that family still exists.</summary>
    Private Sub FillFamilies()
        Dim k_previous As String = SelectedFamily()
        _filling = True
        Try
            cmbFamily.Items.Clear()
            _familyNames.Clear()
            cmbFamily.Items.Add("Toate (" & _corpus.Files.Count.ToString(CultureInfo.InvariantCulture) & ")")
            _familyNames.Add(String.Empty)
            Dim k_keep As Integer = 0
            For Each k_pair In _corpus.Files.GroupBy(Function(k_f) k_f.Family).OrderByDescending(Function(k_g) k_g.Count())
                cmbFamily.Items.Add(k_pair.Key & " (" & k_pair.Count().ToString(CultureInfo.InvariantCulture) & ")")
                _familyNames.Add(k_pair.Key)
                If String.Equals(k_pair.Key, k_previous, StringComparison.Ordinal) Then k_keep = _familyNames.Count - 1
            Next
            cmbFamily.SelectedIndex = k_keep
        Finally
            _filling = False
        End Try
    End Sub

    ''' <summary>The family chosen in the list, or empty for all.</summary>
    Private Function SelectedFamily() As String
        Dim k_index As Integer = cmbFamily.SelectedIndex
        If k_index <= 0 OrElse k_index >= _familyNames.Count Then Return String.Empty
        Return _familyNames(k_index)
    End Function

    Private Function FilteredFiles() As List(Of MdFile)
        Dim k_family As String = SelectedFamily()
        Return _corpus.Files.Where(Function(k_f) k_family.Length = 0 OrElse String.Equals(k_f.Family, k_family, StringComparison.Ordinal)).ToList()
    End Function

    ' ------------------------------------------------------------------
    ' Which view is open
    ' ------------------------------------------------------------------

    ''' <summary>Shows the chosen view and builds it if the data or the filter changed since it was last built. Runs inside a guard of the caller.</summary>
    Private Async Function RefreshCurrentAsync() As Task
        If _corpus Is Nothing Then Return
        Dim k_graph As Boolean = cmbMode.SelectedIndex = ModeGraph
        laneView.Visible = Not k_graph
        graphView.Visible = k_graph
        tlyCursor.Visible = k_graph
        tlyMain.RowStyles(2).Height = If(k_graph, 36F, 0F)
        chkEdits.Enabled = Not k_graph
        If k_graph Then
            If _graphStale Then Await BuildGraphAsync()
        ElseIf _timelineStale Then
            BuildTimeline()
        End If
    End Function

    Private Async Sub CmbMode_SelectedIndexChanged(k_sender As Object, e As EventArgs) Handles cmbMode.SelectedIndexChanged
        Try
            If _filling OrElse _corpus Is Nothing Then Return
            Await UiRun.Guard("MdHistoryView.CmbMode_SelectedIndexChanged", _context, AddressOf RefreshCurrentAsync)
        Catch ex As Exception
            GlobalErrorLog.Write("MdHistoryView.CmbMode_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Async Sub CmbFamily_SelectedIndexChanged(k_sender As Object, e As EventArgs) Handles cmbFamily.SelectedIndexChanged
        Try
            If _filling OrElse _corpus Is Nothing Then Return
            _timelineStale = True
            _graphStale = True
            Await UiRun.Guard("MdHistoryView.CmbFamily_SelectedIndexChanged", _context, AddressOf RefreshCurrentAsync)
        Catch ex As Exception
            GlobalErrorLog.Write("MdHistoryView.CmbFamily_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub ChkEdits_CheckedChanged(k_sender As Object, e As EventArgs) Handles chkEdits.CheckedChanged
        Try
            If _corpus Is Nothing Then Return
            _timelineStale = True
            If cmbMode.SelectedIndex = ModeTimeline Then BuildTimeline()
        Catch ex As Exception
            GlobalErrorLog.Write("MdHistoryView.ChkEdits_CheckedChanged", ex)
        End Try
    End Sub

    Private Async Sub BtnReload_Click(k_sender As Object, e As EventArgs) Handles btnReload.Click
        Try
            Await UiRun.Guard("MdHistoryView.BtnReload_Click", _context, AddressOf LoadAsync)
        Catch ex As Exception
            GlobalErrorLog.Write("MdHistoryView.BtnReload_Click", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' View 1: a lane per slice
    ' ------------------------------------------------------------------

    Private Sub BuildTimeline()
        laneView.BeginUpdate()
        Try
            laneView.ClearLanes()
            Dim k_files As List(Of MdFile) = FilteredFiles()
            Dim k_byGroup As Dictionary(Of String, List(Of MdFile)) = k_files.GroupBy(Function(k_f) k_f.GroupKey).ToDictionary(Function(k_g) k_g.Key, Function(k_g) k_g.OrderBy(Function(k_f) k_f.FirstDate).ThenBy(Function(k_f) k_f.RelPath, StringComparer.Ordinal).ToList(), StringComparer.Ordinal)
            Dim k_sawSlice As Boolean = False
            Dim k_separated As Boolean = False
            For Each k_group As MdGroup In _corpus.Groups
                Dim k_members As List(Of MdFile) = Nothing
                If Not k_byGroup.TryGetValue(k_group.Key, k_members) Then Continue For
                Dim k_lane As KBotLane = laneView.AddLane(k_group.Key, k_group.Caption)
                k_lane.LaneColor = laneView.AutoColor(_groupIndex(k_group.Key))
                k_lane.Tooltip = GroupTip(k_group, k_members)
                If k_group.IsSlice Then
                    k_sawSlice = True
                ElseIf k_sawSlice AndAlso Not k_separated Then
                    k_lane.SeparatorAbove = True
                    k_separated = True
                End If
                For Each k_file As MdFile In k_members
                    Dim k_marker As KBotLaneMarker = k_lane.AddMarker(k_file.FirstDate, k_file.ShortName)
                    k_marker.Tooltip = FileTip(k_file)
                    k_marker.Tag = k_file
                    If chkEdits.Checked Then
                        For k_i As Integer = 1 To k_file.Touches.Count - 1
                            Dim k_edit As KBotLaneMarker = k_lane.AddMarker(k_file.Touches(k_i), k_file.ShortName & " (modificat)")
                            k_edit.Style = KBotLaneMarkerStyle.NoChange
                            k_edit.Tooltip = FileTip(k_file)
                            k_edit.Tag = k_file
                        Next
                    End If
                Next
            Next
        Finally
            laneView.EndUpdate()
        End Try
        _timelineStale = False
    End Sub

    Private Sub LaneView_MarkerHovered(k_laneKey As String, k_markerIndex As Integer) Handles laneView.MarkerHovered
        Try
            If k_laneKey Is Nothing OrElse k_markerIndex < 0 Then Return
            Dim k_lane As KBotLane = laneView.FindLane(k_laneKey)
            If k_lane Is Nothing OrElse k_markerIndex >= k_lane.Markers.Count Then Return
            Dim k_file As MdFile = TryCast(k_lane.Markers(k_markerIndex).Tag, MdFile)
            If k_file IsNot Nothing Then ShowDetail(k_file)
        Catch ex As Exception
            GlobalErrorLog.Write("MdHistoryView.LaneView_MarkerHovered", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' View 2: the graph with a time cursor
    ' ------------------------------------------------------------------

    Private Async Function BuildGraphAsync() As Task
        Dim k_files As List(Of MdFile) = FilteredFiles()
        Dim k_position As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For k_i As Integer = 0 To k_files.Count - 1
            k_position(k_files(k_i).RelPath) = k_i
        Next
        Dim k_groupOf(k_files.Count - 1) As Integer
        For k_i As Integer = 0 To k_files.Count - 1
            k_groupOf(k_i) = _groupIndex(k_files(k_i).GroupKey)
        Next
        Dim k_links As List(Of MdLink) = _corpus.Links.Where(Function(k_l) k_position.ContainsKey(k_l.FromFile.RelPath) AndAlso k_position.ContainsKey(k_l.ToFile.RelPath)).ToList()
        Dim k_from(k_links.Count - 1) As Integer
        Dim k_to(k_links.Count - 1) As Integer
        For k_i As Integer = 0 To k_links.Count - 1
            k_from(k_i) = k_position(k_links(k_i).FromFile.RelPath)
            k_to(k_i) = k_position(k_links(k_i).ToFile.RelPath)
        Next

        _context.Say("Se așază fișierele pe graf (" & k_files.Count.ToString(CultureInfo.InvariantCulture) & " puncte, " & k_links.Count.ToString(CultureInfo.InvariantCulture) & " legături)...")
        Dim k_positions As PointF() = Await Task.Run(Function() KBotGraphLayout.Compute(k_groupOf, k_from, k_to, 140))

        graphView.BeginUpdate()
        Try
            graphView.ClearGraph()
            For k_i As Integer = 0 To k_files.Count - 1
                Dim k_file As MdFile = k_files(k_i)
                Dim k_node As KBotGraphNode = graphView.AddNode(k_file.RelPath, k_file.ShortName)
                k_node.Group = k_file.GroupKey
                k_node.NodeColor = graphView.AutoColor(k_groupOf(k_i))
                k_node.Tooltip = FileTip(k_file)
                k_node.Tag = k_file
                k_node.X = k_positions(k_i).X
                k_node.Y = k_positions(k_i).Y
                k_node.Radius = 3 + Math.Min(5, CInt(Math.Floor(Math.Log(1 + k_file.LinksIn.Count + k_file.LinksOut.Count, 2))))
            Next
            For Each k_link As MdLink In k_links
                graphView.AddEdge(k_link.FromFile.RelPath, k_link.ToFile.RelPath).Faint = k_link.Implied
            Next
        Finally
            graphView.EndUpdate()
        End Try

        SetUpCursor(k_files)
        ApplyCursor()
        graphView.FitToView()
        _graphStale = False
    End Function

    ''' <summary>The cursor runs day by day from the first appearance of the shown files to their last change; it starts at the end (today's picture).</summary>
    Private Sub SetUpCursor(k_files As List(Of MdFile))
        Dim k_withDates As List(Of MdFile) = k_files.Where(Function(k_f) k_f.Touches.Count > 0).ToList()
        If k_withDates.Count = 0 Then
            _cursorStart = Date.Today
            scrollTime.SetRange(0, 6, 7, 0)
            Return
        End If
        _cursorStart = k_withDates.Min(Function(k_f) k_f.FirstDate)
        Dim k_end As Date = k_withDates.Max(Function(k_f) k_f.LastDate)
        Dim k_days As Integer = Math.Max(0, CInt((k_end - _cursorStart).TotalDays))
        scrollTime.SetRange(0, k_days + 6, 7, k_days)
    End Sub

    Private Sub ScrollTime_ValueChanged(k_sender As Object, e As EventArgs) Handles scrollTime.ValueChanged
        Try
            ApplyCursor()
        Catch ex As Exception
            GlobalErrorLog.Write("MdHistoryView.ScrollTime_ValueChanged", ex)
        End Try
    End Sub

    ''' <summary>Shows only what existed on the cursor's date; a ring marks what was touched in the week up to it.</summary>
    Private Sub ApplyCursor()
        If graphView.Nodes.Count = 0 Then
            lblCursor.Text = String.Empty
            Return
        End If
        Dim k_date As Date = _cursorStart.AddDays(scrollTime.Value)
        Dim k_weekStart As Date = k_date.AddDays(-6)
        Dim k_shown As Integer = 0
        Dim k_touched As Integer = 0
        For Each k_node As KBotGraphNode In graphView.Nodes
            Dim k_file As MdFile = DirectCast(k_node.Tag, MdFile)
            k_node.Visible = k_file.FirstDate <= k_date
            k_node.Highlighted = False
            If Not k_node.Visible Then Continue For
            k_shown += 1
            For Each k_touch As Date In k_file.Touches
                If k_touch <= k_date AndAlso k_touch >= k_weekStart Then
                    k_node.Highlighted = True
                    k_touched += 1
                    Exit For
                End If
            Next
        Next
        lblCursor.Text = k_date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) & "  ·  " &
                         k_shown.ToString(CultureInfo.InvariantCulture) & " fișiere md  ·  " &
                         k_touched.ToString(CultureInfo.InvariantCulture) & " atinse în ultima săptămână"
        graphView.RefreshGraph()
    End Sub

    Private Sub GraphView_NodeClicked(k_key As String) Handles graphView.NodeClicked
        Try
            If k_key Is Nothing Then Return
            ShowDetail(DirectCast(graphView.FindNode(k_key).Tag, MdFile))
        Catch ex As Exception
            GlobalErrorLog.Write("MdHistoryView.GraphView_NodeClicked", ex)
        End Try
    End Sub

    Private Sub GraphView_NodeHovered(k_key As String) Handles graphView.NodeHovered
        Try
            If k_key Is Nothing Then Return
            ShowDetail(DirectCast(graphView.FindNode(k_key).Tag, MdFile))
        Catch ex As Exception
            GlobalErrorLog.Write("MdHistoryView.GraphView_NodeHovered", ex)
        End Try
    End Sub

    Private Sub GraphView_NodeDoubleClicked(k_key As String) Handles graphView.NodeDoubleClicked
        Try
            ShowDetail(DirectCast(graphView.FindNode(k_key).Tag, MdFile))
            OpenSelected()
        Catch ex As Exception
            GlobalErrorLog.Write("MdHistoryView.GraphView_NodeDoubleClicked", ex)
        End Try
    End Sub

    ' ------------------------------------------------------------------
    ' The file under the cursor
    ' ------------------------------------------------------------------

    Private Sub ShowDetail(k_file As MdFile)
        _selected = k_file
        btnOpen.Enabled = k_file IsNot Nothing
        If k_file Is Nothing Then
            lblDetail.Text = "Pune cursorul pe un punct ca să vezi fișierul."
            Return
        End If
        lblDetail.Text = k_file.Title & "  (" & k_file.RelPath & ")" & ControlChars.Lf & DetailLine(k_file)
    End Sub

    Private Shared Function DetailLine(k_file As MdFile) As String
        Dim k_text As New StringBuilder()
        k_text.Append(KindLabel(k_file.Kind))
        If k_file.SliceKey.Length > 0 Then k_text.Append(" · felia ").Append(k_file.ShortName)
        k_text.Append(" · ").Append(k_file.Family)
        k_text.Append(" · apărut ").Append(DateText(k_file.FirstDate))
        k_text.Append(" · ultima atingere ").Append(DateText(k_file.LastDate))
        k_text.Append(" · ").Append(k_file.Touches.Count.ToString(CultureInfo.InvariantCulture)).Append(" zile cu modificări")
        k_text.Append(" · trimite spre ").Append(k_file.LinksOut.Count.ToString(CultureInfo.InvariantCulture))
        k_text.Append(", primește de la ").Append(k_file.LinksIn.Count.ToString(CultureInfo.InvariantCulture))
        Return k_text.ToString()
    End Function

    Private Shared Function KindLabel(k_kind As String) As String
        Select Case k_kind
            Case "Fisa"
                Return "Fișă de lucru"
            Case "Registru"
                Return "Registru"
            Case "Plan"
                Return "Plan"
            Case "Ajutor"
                Return "Ajutor"
            Case Else
                Return "Document"
        End Select
    End Function

    Private Shared Function DateText(k_date As Date) As String
        Return k_date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
    End Function

    ''' <summary>The floating label of a file (plain text: the markup characters are taken out of the names).</summary>
    Private Shared Function FileTip(k_file As MdFile) As String
        Dim k_tip As New StringBuilder()
        k_tip.Append("<b>").Append(Plain(k_file.Title)).Append("</b>").Append(ControlChars.Lf)
        k_tip.Append("<i>").Append(Plain(k_file.RelPath)).Append("</i>").Append(ControlChars.Lf)
        k_tip.Append(KindLabel(k_file.Kind))
        If k_file.SliceKey.Length > 0 Then k_tip.Append(" · felia ").Append(k_file.ShortName)
        k_tip.Append(" · ").Append(k_file.Family).Append(ControlChars.Lf)
        k_tip.Append("apărut ").Append(DateText(k_file.FirstDate)).Append(" · ultima atingere ").Append(DateText(k_file.LastDate))
        k_tip.Append(" · ").Append(k_file.Touches.Count.ToString(CultureInfo.InvariantCulture)).Append(" zile").Append(ControlChars.Lf)
        k_tip.Append("trimite spre ").Append(k_file.LinksOut.Count.ToString(CultureInfo.InvariantCulture))
        k_tip.Append(", primește de la ").Append(k_file.LinksIn.Count.ToString(CultureInfo.InvariantCulture))
        If k_file.Preview.Length > 0 Then k_tip.Append(ControlChars.Lf).Append(ControlChars.Lf).Append(k_file.Preview)
        Return k_tip.ToString()
    End Function

    Private Shared Function GroupTip(k_group As MdGroup, k_members As List(Of MdFile)) As String
        Return Plain(k_group.Title) & ControlChars.Lf &
               k_members.Count.ToString(CultureInfo.InvariantCulture) & " fișiere · de la " & DateText(k_group.FirstDate) & " la " & DateText(k_group.LastDate)
    End Function

    Private Shared Function Plain(k_text As String) As String
        Return k_text.Replace("<"c, " "c).Replace(">"c, " "c)
    End Function

    ' ------------------------------------------------------------------
    ' Open the file
    ' ------------------------------------------------------------------

    Private Sub BtnOpen_Click(k_sender As Object, e As EventArgs) Handles btnOpen.Click
        Try
            OpenSelected()
        Catch ex As Exception
            GlobalErrorLog.Write("MdHistoryView.BtnOpen_Click", ex)
        End Try
    End Sub

    Private Sub OpenSelected()
        If _selected Is Nothing OrElse _corpus Is Nothing Then Return
        Dim k_full As String = Path.Combine(_corpus.Root, _selected.RelPath.Replace("/"c, Path.DirectorySeparatorChar))
        Try
            Process.Start(New ProcessStartInfo(k_full) With {.UseShellExecute = True})
        Catch ex As Exception
            GlobalErrorLog.Write("MdHistoryView.OpenSelected", ex)
            KBotMessage.Show("Nu am putut deschide fișierul " & _selected.RelPath & ": " & ex.Message, "KBotAdmin", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

End Class
