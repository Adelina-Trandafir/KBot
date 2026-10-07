#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports KBot.Common

''' <summary>One marker handed to <see cref="ActivexLaneForm"/>: a leaf of the viewer's tree.</summary>
Friend NotInheritable Class ActivexLaneMarkerSpec
    ''' <summary>The log line's time of day, «HH:mm:ss.fff» (the log's date is not used).</summary>
    Public Property Stamp As String = ""
    ''' <summary>Position of the line in the file; keeps the markers of a lane in log order.</summary>
    Public Property Order As Integer
    ''' <summary>The element the leaf sits under; markers of one element share a colour.</summary>
    Public Property ColorKey As String = ""
    Public Property Title As String = ""
    Public Property Body As String = ""
End Class

''' <summary>One lane handed to <see cref="ActivexLaneForm"/>: a root of the viewer's tree and its leaves.</summary>
Friend NotInheritable Class ActivexLaneSpec
    Public Property Caption As String = ""
    Public ReadOnly Property Markers As New List(Of ActivexLaneMarkerSpec)()
End Class

''' <summary>
''' Slice 0078-16: the lane picture of what the log viewer's left tree shows. Each root of the tree is a lane, each leaf
''' a marker (one change in that lane); the horizontal axis is the time of day with its milliseconds -- the date of the
''' log is not used, so a log that runs past midnight folds back onto the start of the axis. Markers of one element
''' share a colour, so the stretch a marker owns reads as «this element until the next change».
''' </summary>
Friend NotInheritable Class ActivexLaneForm

    ' The lane view takes a Date; the day is a fixed one that is never shown (MomentFormat has no date part).
    Private Shared ReadOnly BaseDay As New Date(2000, 1, 1)
    Private Const StampFormat As String = "hh\:mm\:ss\.fff"
    Private Const MaxTooltipChars As Integer = 600

    Friend Sub New(k_title As String, k_lanes As IReadOnlyList(Of ActivexLaneSpec))
        InitializeComponent()
        Try
            Text = k_title
            Fill(k_lanes)
        Catch ex As Exception
            lblStatus.Text = "Nu pot desena culoarele: " & ex.Message
            GlobalErrorLog.Write("ActivexLaneForm.New", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The lanes of a tree built by <see cref="CompareSide.BuildTree"/>: a root is a lane, a leaf under any of its elements
    ''' a marker, the markers of a lane in log order. Risky boundary (tags): the caller logs.
    ''' </summary>
    Friend Shared Function SpecsOf(k_roots As IEnumerable(Of TreeNode)) As List(Of ActivexLaneSpec)
        Dim k_lanes As New List(Of ActivexLaneSpec)()
        For Each k_root As TreeNode In k_roots
            Dim k_kind As ActivexElementKind = DirectCast(k_root.Tag, CompareNodeTag).Kind
            Dim k_spec As New ActivexLaneSpec With {.Caption = CompareSide.KindText(k_kind)}
            For Each k_element As TreeNode In k_root.Nodes
                Dim k_value As String = DirectCast(k_element.Tag, CompareNodeTag).Element.Value
                For Each k_leaf As TreeNode In k_element.Nodes
                    Dim k_line As ActivexLogLine = DirectCast(k_leaf.Tag, CompareNodeTag).Line.Line
                    k_spec.Markers.Add(New ActivexLaneMarkerSpec With {
                        .Stamp = k_line.Stamp,
                        .Order = k_line.Index,
                        .ColorKey = k_value,
                        .Title = $"{CompareSide.KindText(k_kind)}: {k_value}",
                        .Body = $"{k_line.Stamp}  {k_line.Text}"})
                Next
            Next
            k_spec.Markers.Sort(Function(k_a, k_b) k_a.Order.CompareTo(k_b.Order))
            k_lanes.Add(k_spec)
        Next
        Return k_lanes
    End Function

    ''' <summary>
    ''' The lanes of the NEWEST load in <c>activex_check.log</c> (all of it when the file has no «===== LOAD» line), without
    ''' the window-tree dumps -- the same lines the viewer's trees start from. Risky boundary (I/O): the caller logs.
    ''' </summary>
    ''' <param name="k_caption">What the lanes are of: the load's caption.</param>
    Friend Shared Function SpecsFromLog(k_path As String, ByRef k_caption As String) As List(Of ActivexLaneSpec)
        Dim k_runs As New List(Of ActivexLogRun)()
        Dim k_all As List(Of ActivexLogLine) = ActivexLogParser.Parse(ActivexLogParser.ReadAll(k_path), k_runs)
        Dim k_number As Integer = If(k_runs.Count > 0, k_runs(k_runs.Count - 1).Number, -1)
        k_caption = If(k_runs.Count > 0, k_runs(k_runs.Count - 1).Caption, IO.Path.GetFileName(k_path))
        Dim k_lines As List(Of ActivexLogLine) = k_all.FindAll(
            Function(k_l) (k_number < 0 OrElse k_l.Run = k_number) AndAlso k_l.EventName <> "tree row")
        Return SpecsOf(CompareSide.Build(k_lines).BuildTree(False))
    End Function

    ' Reached only through the wrapped constructor.
    Private Sub Fill(k_lanes As IReadOnlyList(Of ActivexLaneSpec))
        Dim k_total As Integer = 0
        Dim k_skipped As Integer = 0
        Dim k_first As Date = Date.MaxValue
        Dim k_last As Date = Date.MinValue

        laneView.BeginUpdate()
        Try
            laneView.ClearLanes()
            For k_i As Integer = 0 To k_lanes.Count - 1
                Dim k_spec As ActivexLaneSpec = k_lanes(k_i)
                Dim k_lane As KBot.Controls.KBotLane = laneView.AddLane("lane" & k_i.ToString(CultureInfo.InvariantCulture), k_spec.Caption)
                k_lane.LaneColor = laneView.AutoColor(k_i)
                k_lane.Tooltip = $"{k_spec.Markers.Count} schimbări"
                k_lane.IsTarget = False

                Dim k_colors As New Dictionary(Of String, Color)(StringComparer.Ordinal)
                For Each k_item As ActivexLaneMarkerSpec In k_spec.Markers
                    Dim k_time As TimeSpan
                    If Not TimeSpan.TryParseExact(k_item.Stamp, StampFormat, CultureInfo.InvariantCulture, k_time) Then
                        k_skipped += 1
                        Continue For
                    End If
                    Dim k_moment As Date = BaseDay.Add(k_time)
                    Dim k_marker As KBot.Controls.KBotLaneMarker = k_lane.AddMarker(k_moment, k_item.Title)
                    k_marker.Tooltip = If(k_item.Body.Length > MaxTooltipChars, k_item.Body.Substring(0, MaxTooltipChars) & "…", k_item.Body)

                    Dim k_color As Color
                    If Not k_colors.TryGetValue(k_item.ColorKey, k_color) Then
                        k_color = laneView.AutoColor(k_colors.Count)
                        k_colors.Add(k_item.ColorKey, k_color)
                    End If
                    k_marker.MarkerColor = k_color

                    k_total += 1
                    If k_moment < k_first Then k_first = k_moment
                    If k_moment > k_last Then k_last = k_moment
                Next
            Next
        Finally
            laneView.EndUpdate()
        End Try

        If k_total = 0 Then
            lblStatus.Text = "Niciun rând cu oră în arborele din stânga."
            Return
        End If
        lblStatus.Text = $"{k_lanes.Count} culoare, {k_total} schimbări, de la {k_first:HH:mm:ss.fff} la {k_last:HH:mm:ss.fff}" &
                         If(k_skipped > 0, $" — {k_skipped} rânduri fără oră lăsate deoparte", "") &
                         "   (culoarea = elementul; treceți mouse-ul peste o schimbare pentru rând)"
    End Sub

End Class
#End If
