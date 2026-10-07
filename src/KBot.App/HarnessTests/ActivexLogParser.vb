#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

' Slice 0078-15: reads Logs\activex_check.log (written by AcroPdfViewer) into lines and the ELEMENTS each line mentions
' -- its event, the window's class / title / handle, the process id, a process name#pid, a milestone -- so that the
' viewer can list the elements, let the operator pick some, and show only the lines that carry them.

''' <summary>One kind of element a line can mention.</summary>
Friend Enum ActivexElementKind
    EventName
    Place
    ClassName
    Title
    Handle
    Pid
    Process
    Milestone
End Enum

''' <summary>One element, as a key that two lines share when they mention the same thing.</summary>
Friend NotInheritable Class ActivexElement
    Public ReadOnly Property Kind As ActivexElementKind
    Public ReadOnly Property Value As String
    Public Property Count As Integer

    Public Sub New(k_kind As ActivexElementKind, k_value As String)
        Kind = k_kind
        Value = k_value
    End Sub

    Public ReadOnly Property Key As String
        Get
            Return KeyOf(Kind, Value)
        End Get
    End Property

    Public Shared Function KeyOf(k_kind As ActivexElementKind, k_value As String) As String
        Return CInt(k_kind).ToString() & "|" & k_value
    End Function

    ''' <summary>The element's kind as the operator reads it.</summary>
    Public ReadOnly Property KindText As String
        Get
            Select Case Kind
                Case ActivexElementKind.EventName : Return "Eveniment"
                Case ActivexElementKind.Place : Return "Unde"
                Case ActivexElementKind.ClassName : Return "Clasă"
                Case ActivexElementKind.Title : Return "Titlu"
                Case ActivexElementKind.Handle : Return "Fereastră"
                Case ActivexElementKind.Pid : Return "Pid"
                Case ActivexElementKind.Process : Return "Proces"
                Case Else : Return "Etapă"
            End Select
        End Get
    End Property
End Class

''' <summary>One line of the log, cut into its parts.</summary>
Friend NotInheritable Class ActivexLogLine
    Public Property Index As Integer
    ''' <summary>0 = before the first load in the file; then 1, 2, … one per «===== LOAD».</summary>
    Public Property Run As Integer
    Public Property Stamp As String = ""
    Public Property Offset As String = ""
    Public Property EventName As String = ""
    Public Property Text As String = ""
    Public Property Raw As String = ""
    Public ReadOnly Property Elements As New HashSet(Of String)()
End Class

''' <summary>One load (a «===== LOAD» line and what follows it).</summary>
Friend NotInheritable Class ActivexLogRun
    Public Property Number As Integer
    Public Property Caption As String = ""
End Class

Friend NotInheritable Class ActivexLogParser

    Private Sub New()
    End Sub

    Private Shared ReadOnly LineRx As New Regex("^(?<date>\d{4}-\d\d-\d\d) (?<time>\d\d:\d\d:\d\d\.\d{3})  \[(?<tag>[^\]]*)\] (?<body>.*)$", RegexOptions.Compiled)
    Private Shared ReadOnly OffsetRx As New Regex("^\+(?<ms>\d+) ms", RegexOptions.Compiled)
    Private Shared ReadOnly EventRx As New Regex(
        "\b(?<e>BOX TEXT|SIZE NUDGE|CTRL\+H|LOCATIONCHANGE|STATECHANGE|FOREGROUND|MILESTONE|REACTIVATE|CREATE|DESTROY|SHOW|HIDE|FOCUS|CLICK|OPERATOR|PRIMER|DOCUMENT|RELEASE|EnsureHost|Clear|processes at load|mouse hook|form closed|watch stopped|ShowDocument EXCEPTION)\b",
        RegexOptions.Compiled)
    Private Shared ReadOnly PlaceRx As New Regex("\b(?<p>in control|Adobe window)\b", RegexOptions.Compiled)
    Private Shared ReadOnly ClassRx As New Regex("class=(?<v>\S+)", RegexOptions.Compiled)
    Private Shared ReadOnly TitleRx As New Regex("title=«(?<v>[^»]*)»", RegexOptions.Compiled)
    Private Shared ReadOnly HandleRx As New Regex("\b0x[0-9A-F]{4,}\b", RegexOptions.Compiled)
    Private Shared ReadOnly PidRx As New Regex("\bpid=(?<v>\d+)", RegexOptions.Compiled)
    Private Shared ReadOnly ProcessRx As New Regex("\b(?<n>[A-Za-z][\w.]*)#(?<v>\d+)", RegexOptions.Compiled)
    Private Shared ReadOnly MilestoneRx As New Regex("MILESTONE: (?<v>.*?)(?:: |$)", RegexOptions.Compiled)
    Private Shared ReadOnly LoadRx As New Regex("===== LOAD «(?<path>[^»]*)» \((?<size>[^,)]*)", RegexOptions.Compiled)

    ''' <summary>
    ''' The whole file, read while the viewer may still be writing to it (shared read). Risky boundary (I/O): the caller
    ''' logs.
    ''' </summary>
    Public Shared Function ReadAll(k_path As String) As List(Of String)
        Dim k_lines As New List(Of String)()
        Using k_stream As New FileStream(k_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Using k_reader As New StreamReader(k_stream, Encoding.UTF8, detectEncodingFromByteOrderMarks:=True)
                Dim k_line As String = k_reader.ReadLine()
                While k_line IsNot Nothing
                    k_lines.Add(k_line)
                    k_line = k_reader.ReadLine()
                End While
            End Using
        End Using
        Return k_lines
    End Function

    ''' <summary>Cuts every line into its parts and elements; the loads are returned in <paramref name="k_runs"/>.</summary>
    Public Shared Function Parse(k_raw As List(Of String), k_runs As List(Of ActivexLogRun)) As List(Of ActivexLogLine)
        Dim k_result As New List(Of ActivexLogLine)(k_raw.Count)
        Dim k_run As Integer = 0
        For k_i As Integer = 0 To k_raw.Count - 1
            Dim k_line As New ActivexLogLine With {.Index = k_i, .Raw = k_raw(k_i)}
            Dim k_match As Match = LineRx.Match(k_raw(k_i))
            Dim k_body As String = k_raw(k_i)
            If k_match.Success Then
                k_line.Stamp = k_match.Groups("time").Value
                k_body = k_match.Groups("body").Value
            End If

            Dim k_load As Match = LoadRx.Match(k_body)
            If k_load.Success Then
                k_run += 1
                k_runs.Add(New ActivexLogRun With {
                    .Number = k_run,
                    .Caption = $"{k_match.Groups("date").Value} {k_line.Stamp} — {Path.GetFileName(k_load.Groups("path").Value)} ({k_load.Groups("size").Value})"})
                k_line.EventName = "LOAD"
            End If
            k_line.Run = k_run
            k_line.Text = k_body.Trim()

            Dim k_offset As Match = OffsetRx.Match(k_line.Text)
            If k_offset.Success Then k_line.Offset = k_offset.Groups("ms").Value

            If k_line.EventName.Length = 0 Then
                If k_body.StartsWith("   ", StringComparison.Ordinal) Then
                    k_line.EventName = "tree row"
                ElseIf k_body.StartsWith("tree (", StringComparison.Ordinal) Then
                    k_line.EventName = "tree"
                Else
                    Dim k_event As Match = EventRx.Match(k_body)
                    If k_event.Success Then k_line.EventName = k_event.Groups("e").Value
                End If
            End If

            CollectElements(k_line, k_body)
            k_result.Add(k_line)
        Next
        Return k_result
    End Function

    Private Shared Sub CollectElements(k_line As ActivexLogLine, k_body As String)
        If k_line.EventName.Length > 0 Then Add(k_line, ActivexElementKind.EventName, k_line.EventName)
        Dim k_place As Match = PlaceRx.Match(k_body)
        If k_place.Success Then Add(k_line, ActivexElementKind.Place, k_place.Groups("p").Value)
        For Each k_m As Match In ClassRx.Matches(k_body)
            Add(k_line, ActivexElementKind.ClassName, k_m.Groups("v").Value)
        Next
        For Each k_m As Match In TitleRx.Matches(k_body)
            If k_m.Groups("v").Value.Length > 0 Then Add(k_line, ActivexElementKind.Title, k_m.Groups("v").Value)
        Next
        For Each k_m As Match In HandleRx.Matches(k_body)
            Add(k_line, ActivexElementKind.Handle, k_m.Value)
        Next
        For Each k_m As Match In PidRx.Matches(k_body)
            Add(k_line, ActivexElementKind.Pid, k_m.Groups("v").Value)
        Next
        For Each k_m As Match In ProcessRx.Matches(k_body)
            Add(k_line, ActivexElementKind.Process, k_m.Groups("n").Value & "#" & k_m.Groups("v").Value)
            Add(k_line, ActivexElementKind.Pid, k_m.Groups("v").Value)
        Next
        Dim k_milestone As Match = MilestoneRx.Match(k_body)
        If k_milestone.Success Then Add(k_line, ActivexElementKind.Milestone, k_milestone.Groups("v").Value.Trim())
    End Sub

    Private Shared Sub Add(k_line As ActivexLogLine, k_kind As ActivexElementKind, k_value As String)
        k_line.Elements.Add(ActivexElement.KeyOf(k_kind, k_value))
    End Sub

    ''' <summary>The elements of the given lines, each with how many of those lines mention it.</summary>
    Public Shared Function CountElements(k_lines As IEnumerable(Of ActivexLogLine)) As List(Of ActivexElement)
        Dim k_map As New Dictionary(Of String, ActivexElement)()
        For Each k_line As ActivexLogLine In k_lines
            For Each k_key As String In k_line.Elements
                Dim k_element As ActivexElement = Nothing
                If Not k_map.TryGetValue(k_key, k_element) Then
                    Dim k_bar As Integer = k_key.IndexOf("|"c)
                    k_element = New ActivexElement(CType(CInt(k_key.Substring(0, k_bar)), ActivexElementKind), k_key.Substring(k_bar + 1))
                    k_map.Add(k_key, k_element)
                End If
                k_element.Count += 1
            Next
        Next
        Dim k_list As New List(Of ActivexElement)(k_map.Values)
        k_list.Sort(Function(a, b)
                        Dim k_byKind As Integer = a.Kind.CompareTo(b.Kind)
                        If k_byKind <> 0 Then Return k_byKind
                        Return b.Count.CompareTo(a.Count)
                    End Function)
        Return k_list
    End Function

End Class
#End If
