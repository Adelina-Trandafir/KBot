#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 0078-15: the viewer of <c>Logs\activex_check.log</c>, two loads side by side (each side its own file and load).
''' Each side is a tree (<see cref="CompareSide"/>): a root per kind of element, the elements under it, the lines that
''' mention an element as its leaves; every node is marked against the other side (same / differs / missing). Selecting a
''' node selects its counterpart on the other side -- the identical one, or for a line that differs the similar one --
''' and both boxes below show what the selected nodes hold. «Diferența următoare / anterioară» walks the lines that are
''' not the same.
''' </summary>
Public NotInheritable Class ActivexLogViewerForm

    Private Const LogFileName As String = "activex_check.log"
    Private Const AllRuns As String = "(toate încărcările)"
    ' The element nodes list at most this many lines in the box below the tree.
    Private Const MaxBoxLines As Integer = 2000

    ''' <summary>One side: its file, its lines, the chosen load and its controls.</summary>
    Private NotInheritable Class SideState
        Public Property LogPath As String = ""
        Public Property AllLines As New List(Of ActivexLogLine)()
        Public ReadOnly Property RunChoices As New List(Of Integer)()
        Public Property Model As New CompareSide()
        Public Property Tree As TreeView
        Public Property Box As RichTextBox
        Public Property Runs As ComboBox
        Public Property FileLabel As Label
    End Class

    ''' <summary>One stop of «Diferența următoare»: a line that is not the same, on the side it belongs to.</summary>
    Private NotInheritable Class DiffStop
        Public Property Side As SideState
        Public Property Item As CompareLine
    End Class

    Private ReadOnly _left As SideState
    Private ReadOnly _right As SideState
    Private ReadOnly _diffs As New List(Of DiffStop)()
    Private _diffIndex As Integer = -1
    Private _loading As Boolean
    Private _syncing As Boolean

    Public Sub New()
        InitializeComponent()
        Dim k_default As String = LogPaths.Combine(LogFileName)
        _left = New SideState With {.LogPath = k_default, .Tree = tvLeft, .Box = rtbLeft, .Runs = cboRunLeft, .FileLabel = lblFileLeft}
        _right = New SideState With {.LogPath = k_default, .Tree = tvRight, .Box = rtbRight, .Runs = cboRunRight, .FileLabel = lblFileRight}
        Try
            ' The two newest loads by default: the older on the left, the newest on the right.
            ReadSide(_left, 2)
            ReadSide(_right, 1)
            CompareSides()
        Catch ex As Exception
            lblStatus.Text = "Nu pot citi jurnalul: " & ex.Message
            GlobalErrorLog.Write("ActivexLogViewerForm.New", ex)
        End Try
    End Sub

    ' ── Reading ─────────────────────────────────────────────────────────────────

    ' Reads the side's file and fills its load list; keeps the chosen load when it is still there, else picks the
    ' k_pick-th entry (1 = newest). Reached only through wrapped callers.
    Private Sub ReadSide(k_side As SideState, k_pick As Integer)
        Dim k_keep As String = TryCast(k_side.Runs.SelectedItem, String)
        Dim k_runs As New List(Of ActivexLogRun)()
        If File.Exists(k_side.LogPath) Then
            k_side.AllLines = ActivexLogParser.Parse(ActivexLogParser.ReadAll(k_side.LogPath), k_runs)
            k_side.FileLabel.Text = Path.GetFileName(k_side.LogPath)
        Else
            k_side.AllLines = New List(Of ActivexLogLine)()
            k_side.FileLabel.Text = $"Nu există «{k_side.LogPath}»."
        End If
        _loading = True
        Try
            k_side.Runs.Items.Clear()
            k_side.RunChoices.Clear()
            k_side.Runs.Items.Add(AllRuns)
            k_side.RunChoices.Add(-1)
            For k_i As Integer = k_runs.Count - 1 To 0 Step -1
                k_side.Runs.Items.Add(k_runs(k_i).Caption)
                k_side.RunChoices.Add(k_runs(k_i).Number)
            Next
            Dim k_index As Integer = If(k_keep Is Nothing, -1, k_side.Runs.Items.IndexOf(k_keep))
            If k_index < 0 Then k_index = Math.Min(k_pick, k_side.Runs.Items.Count - 1)
            k_side.Runs.SelectedIndex = k_index
        Finally
            _loading = False
        End Try
    End Sub

    Private Function ChosenLines(k_side As SideState) As List(Of ActivexLogLine)
        Dim k_index As Integer = k_side.Runs.SelectedIndex
        Dim k_choice As Integer = If(k_index >= 0 AndAlso k_index < k_side.RunChoices.Count, k_side.RunChoices(k_index), -1)
        Dim k_hideTree As Boolean = chkHideTree.Checked
        Return k_side.AllLines.FindAll(Function(k_l) (k_choice < 0 OrElse k_l.Run = k_choice) AndAlso
                                                     Not (k_hideTree AndAlso k_l.EventName = "tree row"))
    End Function

    ' Both sides again: models, pairing, trees, the list of differences.
    Private Sub CompareSides()
        ' InitializeComponent sets chkHideTree.Checked, which fires CheckedChanged before the constructor builds the sides.
        If _left Is Nothing OrElse _right Is Nothing Then Return
        _left.Model = CompareSide.Build(ChosenLines(_left))
        _right.Model = CompareSide.Build(ChosenLines(_right))
        CompareSide.Pair(_left.Model, _right.Model)
        FillTree(_left)
        FillTree(_right)

        _diffs.Clear()
        _diffIndex = -1
        For Each k_item As CompareLine In _left.Model.Lines
            If k_item.Status <> CompareStatus.Same Then _diffs.Add(New DiffStop With {.Side = _left, .Item = k_item})
        Next
        ' The right side's lines with nothing like them on the left (those with a similar one were met on the left).
        For Each k_item As CompareLine In _right.Model.Lines
            If k_item.Status = CompareStatus.Missing Then _diffs.Add(New DiffStop With {.Side = _right, .Item = k_item})
        Next
        lblStatus.Text = $"stânga {_left.Model.Lines.Count} rânduri, dreapta {_right.Model.Lines.Count} rânduri — " &
                         $"{_diffs.Count} rânduri care nu sunt identice"
    End Sub

    Private Sub FillTree(k_side As SideState)
        k_side.Tree.BeginUpdate()
        Try
            k_side.Tree.Nodes.Clear()
            k_side.Tree.Nodes.AddRange(k_side.Model.BuildTree(chkHideSame.Checked).ToArray())
        Finally
            k_side.Tree.EndUpdate()
        End Try
        k_side.Box.Clear()
    End Sub

    ' ── Selection: the counterpart on the other side ────────────────────────────

    Private Sub Selected(k_side As SideState, k_other As SideState, k_node As TreeNode)
        ShowDetails(k_side, k_node)
        If _syncing Then Return
        _syncing = True
        Try
            Dim k_target As TreeNode = CounterpartNode(k_other, k_node)
            If k_target Is Nothing Then
                k_other.Tree.SelectedNode = Nothing
                WriteBox(k_other.Box, "\cf5 " & RtfText("Nu există în acest jurnal.") & "\par")
            Else
                ' Fires the other tree's AfterSelect, which fills its box (no sync back: _syncing).
                k_other.Tree.SelectedNode = k_target
                k_target.EnsureVisible()
            End If
        Finally
            _syncing = False
        End Try
    End Sub

    Private Shared Function CounterpartNode(k_other As SideState, k_node As TreeNode) As TreeNode
        Dim k_tag As CompareNodeTag = TryCast(k_node?.Tag, CompareNodeTag)
        If k_tag Is Nothing Then Return Nothing
        If k_tag.Line IsNot Nothing Then
            Dim k_twin As CompareLine = k_tag.Line.Counterpart
            If k_twin Is Nothing Then Return Nothing
            Dim k_leaf As TreeNode = Nothing
            If k_twin.Nodes.TryGetValue(k_tag.Element.Key, k_leaf) Then Return k_leaf
            ' Under a window or a pid, which differ on every run: the same line under its event.
            Return EventLeaf(k_twin)
        End If
        If k_tag.Element IsNot Nothing Then
            Dim k_element As CompareElement = Nothing
            Return If(k_other.Model.Elements.TryGetValue(k_tag.Element.Key, k_element), k_element.Node, Nothing)
        End If
        Dim k_root As TreeNode = Nothing
        Return If(k_other.Model.Roots.TryGetValue(k_tag.Kind, k_root), k_root, Nothing)
    End Function

    ' The line's leaf under its event (every line has one: «(fără eveniment)» when it has none).
    Private Shared Function EventLeaf(k_item As CompareLine) As TreeNode
        Dim k_prefix As String = ActivexElement.KeyOf(ActivexElementKind.EventName, "")
        For Each k_key As String In k_item.Keys
            Dim k_leaf As TreeNode = Nothing
            If k_key.StartsWith(k_prefix, StringComparison.Ordinal) AndAlso k_item.Nodes.TryGetValue(k_key, k_leaf) Then Return k_leaf
        Next
        For Each k_leaf As TreeNode In k_item.Nodes.Values
            Return k_leaf
        Next
        Return Nothing
    End Function

    ' ── The box under each tree ─────────────────────────────────────────────────

    ' RTF colour table: 1 text, 2 dim, 3 same, 4 differs, 5 missing, 6 accent (marked words' back), 7 accent text.
    Private Shared Function StatusCf(k_status As CompareStatus) As String
        Select Case k_status
            Case CompareStatus.Same : Return "\cf3 "
            Case CompareStatus.Differs : Return "\cf4 "
            Case CompareStatus.Missing : Return "\cf5 "
            Case Else : Return "\cf2 "
        End Select
    End Function

    Private Sub ShowDetails(k_side As SideState, k_node As TreeNode)
        Dim k_tag As CompareNodeTag = TryCast(k_node?.Tag, CompareNodeTag)
        If k_tag Is Nothing Then
            k_side.Box.Clear()
            Return
        End If
        Dim k_rtf As New StringBuilder()
        If k_tag.Line IsNot Nothing Then
            AppendLine(k_rtf, k_tag.Line)
        ElseIf k_tag.Element IsNot Nothing Then
            AppendElement(k_rtf, k_tag.Element)
        Else
            AppendRoot(k_rtf, k_side, k_tag.Kind)
        End If
        WriteBox(k_side.Box, k_rtf.ToString())
    End Sub

    ' A line: when, how it stands, and its text -- the words its similar counterpart does not have are marked.
    Private Shared Sub AppendLine(k_rtf As StringBuilder, k_item As CompareLine)
        Dim k_line As ActivexLogLine = k_item.Line
        k_rtf.Append("\cf2 ").Append(RtfText($"{k_line.Stamp}   +{If(k_line.Offset.Length > 0, k_line.Offset, "?")} ms   "))
        k_rtf.Append(StatusCf(k_item.Status)).Append(RtfText(CompareSide.StatusText(k_item.Status)))
        Dim k_other As CompareLine = k_item.Counterpart
        If k_other IsNot Nothing Then
            k_rtf.Append("\cf2 ").Append(RtfText($"   (acolo: {k_other.Line.Stamp}, +{If(k_other.Line.Offset.Length > 0, k_other.Line.Offset, "?")} ms)"))
        End If
        k_rtf.Append("\par\par ")

        If k_item.Status = CompareStatus.Differs Then
            Dim k_theirs As New HashSet(Of String)()
            For Each k_word As String In k_other.Line.Text.Split(" "c)
                k_theirs.Add(CompareSide.Normalize(k_word))
            Next
            For Each k_word As String In k_item.Line.Text.Split(" "c)
                If k_word.Length = 0 OrElse k_theirs.Contains(CompareSide.Normalize(k_word)) Then
                    k_rtf.Append("\cf1 ").Append(RtfText(k_word))
                Else
                    k_rtf.Append("\cf7\highlight6 ").Append(RtfText(k_word)).Append("\highlight0")
                End If
                k_rtf.Append("\cf1 ").Append(" ")
            Next
        Else
            k_rtf.Append("\cf1 ").Append(RtfText(k_line.Text))
        End If
        k_rtf.Append("\par ")
    End Sub

    ' An element: its lines, each coloured by how it stands.
    Private Shared Sub AppendElement(k_rtf As StringBuilder, k_element As CompareElement)
        k_rtf.Append("\cf1\b ").Append(RtfText($"{CompareSide.KindText(k_element.Kind)}: {k_element.Value}")).Append("\b0 ")
        k_rtf.Append("\cf2 ").Append(RtfText($"   {k_element.Lines.Count} rânduri   "))
        k_rtf.Append(StatusCf(k_element.Status)).Append(RtfText(CompareSide.StatusText(k_element.Status))).Append("\par\par ")
        Dim k_count As Integer = 0
        For Each k_item As CompareLine In k_element.Lines
            k_count += 1
            If k_count > MaxBoxLines Then
                k_rtf.Append("\cf2 ").Append(RtfText($"… încă {k_element.Lines.Count - MaxBoxLines} rânduri")).Append("\par ")
                Exit For
            End If
            k_rtf.Append(StatusCf(k_item.Status)).Append(RtfText($"{CompareSide.Mark(k_item.Status).Trim()} {k_item.Line.Stamp}  "))
            k_rtf.Append("\cf1 ").Append(RtfText(k_item.Line.Text)).Append("\par ")
        Next
    End Sub

    ' A root: its elements, each coloured by how it stands.
    Private Shared Sub AppendRoot(k_rtf As StringBuilder, k_side As SideState, k_kind As ActivexElementKind)
        k_rtf.Append("\cf1\b ").Append(RtfText(CompareSide.KindText(k_kind))).Append("\b0\par\par ")
        Dim k_root As TreeNode = Nothing
        If Not k_side.Model.Roots.TryGetValue(k_kind, k_root) Then Return
        For Each k_child As TreeNode In k_root.Nodes
            Dim k_element As CompareElement = DirectCast(k_child.Tag, CompareNodeTag).Element
            k_rtf.Append(StatusCf(k_element.Status)).Append(RtfText($"{CompareSide.Mark(k_element.Status).Trim()} "))
            k_rtf.Append("\cf1 ").Append(RtfText($"{k_element.Value}  ({k_element.Lines.Count})"))
            k_rtf.Append("\cf2 ").Append(RtfText("   " & CompareSide.StatusText(k_element.Status))).Append("\par ")
        Next
    End Sub

    Private Shared Sub WriteBox(k_box As RichTextBox, k_body As String)
        Dim k_palette As ThemePalette = ThemeManager.Current.Palette
        Dim k_rtf As New StringBuilder()
        k_rtf.Append("{\rtf1\ansi\deff0{\fonttbl{\f0 Segoe UI;}}{\colortbl ;")
        k_rtf.Append(RtfColor(k_palette.TextColor)).Append(RtfColor(k_palette.TextDimColor))
        k_rtf.Append(RtfColor(k_palette.SuccessColor)).Append(RtfColor(k_palette.WarningColor))
        k_rtf.Append(RtfColor(k_palette.ErrorColor)).Append(RtfColor(k_palette.AccentColor))
        k_rtf.Append(RtfColor(k_palette.AccentTextColor))
        ' \fs is in half points: 20 = 10 pt.
        k_rtf.Append("}\f0\fs20 ").Append(k_body).Append("}")
        k_box.Rtf = k_rtf.ToString()
    End Sub

    Private Shared Function RtfColor(k_color As Color) As String
        Return $"\red{k_color.R}\green{k_color.G}\blue{k_color.B};"
    End Function

    ' Escapes RTF's own characters; anything outside ASCII goes as \uN? (a surrogate pair as two of them).
    Private Shared Function RtfText(k_value As String) As String
        Dim k_out As New StringBuilder(k_value.Length + 16)
        For Each k_char As Char In k_value
            Select Case k_char
                Case "\"c, "{"c, "}"c : k_out.Append("\"c).Append(k_char)
                Case Else
                    Dim k_code As Integer = AscW(k_char)
                    If k_code > 127 OrElse k_code < 0 Then
                        ' RTF \u takes a signed 16-bit number.
                        k_out.Append("\u").Append(If(k_code > 32767, k_code - 65536, k_code).ToString()).Append("?"c)
                    Else
                        k_out.Append(k_char)
                    End If
            End Select
        Next
        Return k_out.ToString()
    End Function

    ' ── Walking the differences ─────────────────────────────────────────────────

    Private Sub GoToDiff(k_step As Integer)
        If _diffs.Count = 0 Then
            lblStatus.Text = "Nicio diferență: cele două încărcări au aceleași rânduri."
            Return
        End If
        _diffIndex = Math.Max(0, Math.Min(_diffs.Count - 1, _diffIndex + k_step))
        Dim k_stop As DiffStop = _diffs(_diffIndex)
        Dim k_leaf As TreeNode = EventLeaf(k_stop.Item)
        If k_leaf Is Nothing Then Return
        k_stop.Side.Tree.SelectedNode = k_leaf
        k_leaf.EnsureVisible()
        k_stop.Side.Tree.Focus()
        lblStatus.Text = $"diferența {_diffIndex + 1} din {_diffs.Count} ({If(k_stop.Side Is _left, "stânga", "doar în dreapta")})"
    End Sub

    ' ── Export: both trees side by side (ActivexCompareExcel) ──────────────────
    ' What the trees show now (so «Ascunde ce e identic» applies), matching nodes on the same row: a root with the root of
    ' the same kind, an element with the element of the same key, a line with its counterpart under that element. What
    ' has no match on the other side gets a row of its own.

    Private Function ExportRows() As List(Of CompareExportRow)
        Dim k_rows As New List(Of CompareExportRow)()
        Dim k_title As New CompareExportRow With {.Bold = True, .Header = True}
        k_title.SetCell(0, $"Stânga: {cboRunLeft.Text}  —  {_left.LogPath}", CompareStatus.NotCompared)
        k_title.SetCell(3, $"Dreapta: {cboRunRight.Text}  —  {_right.LogPath}", CompareStatus.NotCompared)
        k_rows.Add(k_title)
        Dim k_heads As New CompareExportRow With {.Bold = True, .Header = True}
        For Each k_side As Integer In {0, 3}
            k_heads.SetCell(k_side, "Tip", CompareStatus.NotCompared)
            k_heads.SetCell(k_side + 1, "Element", CompareStatus.NotCompared)
            k_heads.SetCell(k_side + 2, "Rând", CompareStatus.NotCompared)
        Next
        k_rows.Add(k_heads)

        Dim k_rightRoots As New Dictionary(Of ActivexElementKind, TreeNode)()
        For Each k_root As TreeNode In tvRight.Nodes
            k_rightRoots(DirectCast(k_root.Tag, CompareNodeTag).Kind) = k_root
        Next
        For Each k_left As TreeNode In tvLeft.Nodes
            Dim k_kind As ActivexElementKind = DirectCast(k_left.Tag, CompareNodeTag).Kind
            Dim k_right As TreeNode = Nothing
            If k_rightRoots.TryGetValue(k_kind, k_right) Then k_rightRoots.Remove(k_kind)
            AddRootRows(k_rows, k_left, k_right)
        Next
        For Each k_right As TreeNode In tvRight.Nodes
            If k_rightRoots.ContainsValue(k_right) Then AddRootRows(k_rows, Nothing, k_right)
        Next
        Return k_rows
    End Function

    Private Sub AddRootRows(k_rows As List(Of CompareExportRow), k_left As TreeNode, k_right As TreeNode)
        Dim k_row As New CompareExportRow With {.Bold = True}
        If k_left IsNot Nothing Then k_row.SetCell(0, k_left.Text, RootStatusOf(_left, k_left))
        If k_right IsNot Nothing Then k_row.SetCell(3, k_right.Text, RootStatusOf(_right, k_right))
        k_rows.Add(k_row)

        Dim k_rightElements As New Dictionary(Of String, TreeNode)()
        If k_right IsNot Nothing Then
            For Each k_node As TreeNode In k_right.Nodes
                k_rightElements(DirectCast(k_node.Tag, CompareNodeTag).Element.Key) = k_node
            Next
        End If
        If k_left IsNot Nothing Then
            For Each k_node As TreeNode In k_left.Nodes
                Dim k_key As String = DirectCast(k_node.Tag, CompareNodeTag).Element.Key
                Dim k_other As TreeNode = Nothing
                If k_rightElements.TryGetValue(k_key, k_other) Then k_rightElements.Remove(k_key)
                AddElementRows(k_rows, k_node, k_other)
            Next
        End If
        If k_right IsNot Nothing Then
            For Each k_node As TreeNode In k_right.Nodes
                If k_rightElements.ContainsValue(k_node) Then AddElementRows(k_rows, Nothing, k_node)
            Next
        End If
    End Sub

    Private Shared Function RootStatusOf(k_side As SideState, k_root As TreeNode) As CompareStatus
        Dim k_status As CompareStatus = CompareStatus.NotCompared
        k_side.Model.RootStatus.TryGetValue(DirectCast(k_root.Tag, CompareNodeTag).Kind, k_status)
        Return k_status
    End Function

    Private Shared Sub AddElementRows(k_rows As List(Of CompareExportRow), k_left As TreeNode, k_right As TreeNode)
        Dim k_row As New CompareExportRow()
        If k_left IsNot Nothing Then k_row.SetCell(1, k_left.Text, DirectCast(k_left.Tag, CompareNodeTag).Element.Status)
        If k_right IsNot Nothing Then k_row.SetCell(4, k_right.Text, DirectCast(k_right.Tag, CompareNodeTag).Element.Status)
        k_rows.Add(k_row)

        Dim k_rightLeaves As New HashSet(Of TreeNode)()
        If k_right IsNot Nothing Then
            For Each k_leaf As TreeNode In k_right.Nodes
                k_rightLeaves.Add(k_leaf)
            Next
        End If
        If k_left IsNot Nothing Then
            Dim k_key As String = DirectCast(k_left.Tag, CompareNodeTag).Element.Key
            For Each k_leaf As TreeNode In k_left.Nodes
                Dim k_item As CompareLine = DirectCast(k_leaf.Tag, CompareNodeTag).Line
                Dim k_line As New CompareExportRow()
                k_line.SetCell(2, ExportLineText(k_item), k_item.Status)
                Dim k_twinLeaf As TreeNode = Nothing
                If k_item.Counterpart IsNot Nothing AndAlso k_item.Counterpart.Nodes.TryGetValue(k_key, k_twinLeaf) AndAlso
                   k_rightLeaves.Remove(k_twinLeaf) Then
                    k_line.SetCell(5, ExportLineText(k_item.Counterpart), k_item.Counterpart.Status)
                End If
                k_rows.Add(k_line)
            Next
        End If
        If k_right IsNot Nothing Then
            For Each k_leaf As TreeNode In k_right.Nodes
                If Not k_rightLeaves.Contains(k_leaf) Then Continue For
                Dim k_item As CompareLine = DirectCast(k_leaf.Tag, CompareNodeTag).Line
                Dim k_line As New CompareExportRow()
                k_line.SetCell(5, ExportLineText(k_item), k_item.Status)
                k_rows.Add(k_line)
            Next
        End If
    End Sub

    ' The whole line (the tree cuts it at 220 characters), with its +ms.
    Private Shared Function ExportLineText(k_item As CompareLine) As String
        Dim k_line As ActivexLogLine = k_item.Line
        Return $"{CompareSide.EventEmoji(k_line.EventName)} {k_line.Stamp}  {k_line.Text}{CompareSide.Mark(k_item.Status)}"
    End Function

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        Try
            If tvLeft.Nodes.Count = 0 AndAlso tvRight.Nodes.Count = 0 Then
                lblStatus.Text = "Nimic de exportat: ambii arbori sunt goi."
                Return
            End If
            dlgSave.InitialDirectory = Path.GetDirectoryName(_left.LogPath)
            dlgSave.FileName = $"activex_comparatie_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            If dlgSave.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim k_rows As List(Of CompareExportRow) = ExportRows()
            ActivexCompareExcel.Write(dlgSave.FileName, k_rows)
            lblStatus.Text = $"Exportat {k_rows.Count - 2} rânduri în «{dlgSave.FileName}»."
            Process.Start(New ProcessStartInfo(dlgSave.FileName) With {.UseShellExecute = True})?.Dispose()
        Catch ex As Exception
            lblStatus.Text = "Exportul nu a reușit: " & ex.Message
            GlobalErrorLog.Write("ActivexLogViewerForm.btnExport_Click", ex)
        End Try
    End Sub

    ' ── The left tree as lanes (ActivexLaneForm) ────────────────────────────────
    ' What the tree shows now (so «Fără rândurile arborilor de ferestre» and «Ascunde ce e identic» apply): a root is a
    ' lane, a leaf under any of its elements a marker.

    ' Not modal, like the viewer itself: it is a picture of what the left tree showed when the button was pressed.
    Private Sub btnLanes_Click(sender As Object, e As EventArgs) Handles btnLanes.Click
        Try
            If tvLeft.Nodes.Count = 0 Then
                lblStatus.Text = "Nimic de arătat: arborele din stânga e gol."
                Return
            End If
            Dim k_form As New ActivexLaneForm($"Jurnalul ActiveX — pe culoare — {cboRunLeft.Text}", ActivexLaneForm.SpecsOf(tvLeft.Nodes.Cast(Of TreeNode)()))
            k_form.Show(Me)
        Catch ex As Exception
            lblStatus.Text = "Nu pot deschide culoarele: " & ex.Message
            GlobalErrorLog.Write("ActivexLogViewerForm.btnLanes_Click", ex)
        End Try
    End Sub

    ' ── Handlers (UI boundaries: log and swallow) ───────────────────────────────

    Private Sub tvLeft_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles tvLeft.AfterSelect
        Try
            Selected(_left, _right, e.Node)
        Catch ex As Exception
            GlobalErrorLog.Write("ActivexLogViewerForm.tvLeft_AfterSelect", ex)
        End Try
    End Sub

    Private Sub tvRight_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles tvRight.AfterSelect
        Try
            Selected(_right, _left, e.Node)
        Catch ex As Exception
            GlobalErrorLog.Write("ActivexLogViewerForm.tvRight_AfterSelect", ex)
        End Try
    End Sub

    Private Sub btnPickLeft_Click(sender As Object, e As EventArgs) Handles btnPickLeft.Click
        Try
            PickFile(_left)
        Catch ex As Exception
            lblStatus.Text = "Nu pot citi jurnalul: " & ex.Message
            GlobalErrorLog.Write("ActivexLogViewerForm.btnPickLeft_Click", ex)
        End Try
    End Sub

    Private Sub btnPickRight_Click(sender As Object, e As EventArgs) Handles btnPickRight.Click
        Try
            PickFile(_right)
        Catch ex As Exception
            lblStatus.Text = "Nu pot citi jurnalul: " & ex.Message
            GlobalErrorLog.Write("ActivexLogViewerForm.btnPickRight_Click", ex)
        End Try
    End Sub

    Private Sub PickFile(k_side As SideState)
        dlgOpen.InitialDirectory = Path.GetDirectoryName(k_side.LogPath)
        If dlgOpen.ShowDialog(Me) <> DialogResult.OK Then Return
        k_side.LogPath = dlgOpen.FileName
        ' A new file: its newest load, not the caption chosen in the old one.
        _loading = True
        Try
            k_side.Runs.SelectedIndex = -1
        Finally
            _loading = False
        End Try
        ReadSide(k_side, 1)
        CompareSides()
    End Sub

    Private Sub btnReload_Click(sender As Object, e As EventArgs) Handles btnReload.Click
        Try
            ReadSide(_left, 2)
            ReadSide(_right, 1)
            CompareSides()
        Catch ex As Exception
            lblStatus.Text = "Nu pot citi jurnalul: " & ex.Message
            GlobalErrorLog.Write("ActivexLogViewerForm.btnReload_Click", ex)
        End Try
    End Sub

    Private Sub cboRun_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboRunLeft.SelectedIndexChanged, cboRunRight.SelectedIndexChanged
        Try
            If Not _loading Then CompareSides()
        Catch ex As Exception
            GlobalErrorLog.Write("ActivexLogViewerForm.cboRun_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub chkHideTree_CheckedChanged(sender As Object, e As EventArgs) Handles chkHideTree.CheckedChanged
        Try
            CompareSides()
        Catch ex As Exception
            GlobalErrorLog.Write("ActivexLogViewerForm.chkHideTree_CheckedChanged", ex)
        End Try
    End Sub

    ' Only the trees change: the pairing and the list of differences stay.
    Private Sub chkHideSame_CheckedChanged(sender As Object, e As EventArgs) Handles chkHideSame.CheckedChanged
        Try
            If _left Is Nothing OrElse _right Is Nothing Then Return
            FillTree(_left)
            FillTree(_right)
        Catch ex As Exception
            GlobalErrorLog.Write("ActivexLogViewerForm.chkHideSame_CheckedChanged", ex)
        End Try
    End Sub

    Private Sub btnNextDiff_Click(sender As Object, e As EventArgs) Handles btnNextDiff.Click
        Try
            GoToDiff(1)
        Catch ex As Exception
            GlobalErrorLog.Write("ActivexLogViewerForm.btnNextDiff_Click", ex)
        End Try
    End Sub

    Private Sub btnPrevDiff_Click(sender As Object, e As EventArgs) Handles btnPrevDiff.Click
        Try
            GoToDiff(-1)
        Catch ex As Exception
            GlobalErrorLog.Write("ActivexLogViewerForm.btnPrevDiff_Click", ex)
        End Try
    End Sub

End Class
#End If
