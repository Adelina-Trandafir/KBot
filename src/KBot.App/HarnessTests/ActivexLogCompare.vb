#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Text
Imports System.Text.RegularExpressions
Imports KBot.Theming

' Slice 0078-15: two loads of activex_check.log side by side. Each side is a tree: one root per kind of element (event,
' class, title, ...), one node per element, the lines that mention it as leaves. Every line is reduced to a SIGNATURE
' (the line without what changes on every run: times, +ms, handles, pids) and paired with the line of the other side
' that has the same signature (the n-th with the n-th); lines left over are paired with a SIMILAR one (same event,
' place, class and title) when there is one.

''' <summary>How a node or a line of one side stands against the other side.</summary>
Friend Enum CompareStatus
    Same
    Differs
    Missing
    NotCompared
End Enum

''' <summary>One line of one side, with what it is compared by and what it was paired with.</summary>
Friend NotInheritable Class CompareLine
    Public Property Line As ActivexLogLine
    Public Property Signature As String = ""
    Public Property Loose As String = ""
    ''' <summary>How many earlier lines of the same side have the same signature.</summary>
    Public Property Occurrence As Integer
    ''' <summary>The line of the other side with the same signature.</summary>
    Public Property Twin As CompareLine
    ''' <summary>When there is no twin: a line of the other side with the same event / place / class / title.</summary>
    Public Property Similar As CompareLine
    ''' <summary>The element keys (compare keys) this line sits under.</summary>
    Public ReadOnly Property Keys As New List(Of String)()
    ''' <summary>The leaf of this line under each element node, by element key.</summary>
    Public ReadOnly Property Nodes As New Dictionary(Of String, TreeNode)()

    Public ReadOnly Property Status As CompareStatus
        Get
            If Twin IsNot Nothing Then Return CompareStatus.Same
            If Similar IsNot Nothing Then Return CompareStatus.Differs
            Return CompareStatus.Missing
        End Get
    End Property

    Public ReadOnly Property Counterpart As CompareLine
        Get
            Return If(Twin, Similar)
        End Get
    End Property
End Class

''' <summary>One element of one side: its node and its lines.</summary>
Friend NotInheritable Class CompareElement
    Public Property Key As String = ""
    Public Property Kind As ActivexElementKind
    Public Property Value As String = ""
    Public ReadOnly Property Lines As New List(Of CompareLine)()
    Public Property Node As TreeNode
    Public Property Status As CompareStatus
End Class

''' <summary>What a node of the tree stands for.</summary>
Friend NotInheritable Class CompareNodeTag
    Public Property Kind As ActivexElementKind
    ''' <summary>Nothing on a root.</summary>
    Public Property Element As CompareElement
    ''' <summary>Nothing on a root or an element node.</summary>
    Public Property Line As CompareLine
End Class

''' <summary>One side: the lines of the chosen load, its elements and its tree.</summary>
Friend NotInheritable Class CompareSide

    ''' <summary>The value an element with no event gets in the «Eveniment» root.</summary>
    Public Const NoEvent As String = "(fără eveniment)"

    Public ReadOnly Property Lines As New List(Of CompareLine)()
    Public ReadOnly Property Elements As New Dictionary(Of String, CompareElement)()
    Public ReadOnly Property Roots As New Dictionary(Of ActivexElementKind, TreeNode)()
    Public ReadOnly Property RootStatus As New Dictionary(Of ActivexElementKind, CompareStatus)()
    Private ReadOnly _bySignature As New Dictionary(Of String, List(Of CompareLine))()

    Private Shared ReadOnly LeadOffsetRx As New Regex("^\+\d+ ms\s*", RegexOptions.Compiled)
    Private Shared ReadOnly HandledRx As New Regex("\(handled \d+ ms later\)\s*", RegexOptions.Compiled)
    Private Shared ReadOnly TimeRx As New Regex("\b\d\d:\d\d:\d\d\.\d{3}\b", RegexOptions.Compiled)
    Private Shared ReadOnly MsRx As New Regex("(?<![\w.])\+?\d+ ms\b", RegexOptions.Compiled)
    Private Shared ReadOnly HandleRx As New Regex("\b0x[0-9A-Fa-f]+\b", RegexOptions.Compiled)
    Private Shared ReadOnly LongHexRx As New Regex("\b[0-9A-F]{8,}\b", RegexOptions.Compiled)
    Private Shared ReadOnly ProcessRx As New Regex("\b(?<n>[A-Za-z][\w.]*)#\d+", RegexOptions.Compiled)
    Private Shared ReadOnly PidRx As New Regex("\bpid=\d+", RegexOptions.Compiled)
    Private Shared ReadOnly PidSuffixRx As New Regex("_\d{3,}\b", RegexOptions.Compiled)
    Private Shared ReadOnly SpacesRx As New Regex("\s+", RegexOptions.Compiled)

    ''' <summary>A line (or a word of it) without what changes on every run: times, +ms, handles, pids.</summary>
    Public Shared Function Normalize(k_text As String) As String
        Dim k_out As String = LeadOffsetRx.Replace(k_text.Trim(), "")
        k_out = HandledRx.Replace(k_out, "")
        k_out = TimeRx.Replace(k_out, "#time")
        k_out = MsRx.Replace(k_out, "# ms")
        k_out = HandleRx.Replace(k_out, "0x#")
        k_out = LongHexRx.Replace(k_out, "#")
        k_out = ProcessRx.Replace(k_out, "${n}#")
        k_out = PidRx.Replace(k_out, "pid=#")
        k_out = PidSuffixRx.Replace(k_out, "_#")
        Return SpacesRx.Replace(k_out, " ").Trim()
    End Function

    ''' <summary>
    ''' The key an element is compared by: a process by its name alone (its pid changes on every start), a class without
    ''' the pid or address some classes carry.
    ''' </summary>
    Private Shared Function CompareKeyOf(k_kind As ActivexElementKind, k_value As String, ByRef k_shown As String) As String
        k_shown = k_value
        Select Case k_kind
            Case ActivexElementKind.Process
                Dim k_hash As Integer = k_value.IndexOf("#"c)
                If k_hash > 0 Then k_shown = k_value.Substring(0, k_hash)
            Case ActivexElementKind.ClassName
                k_shown = Normalize(k_value)
        End Select
        Return ActivexElement.KeyOf(k_kind, k_shown)
    End Function

    ''' <summary>Handles and pids are different on every run: they are listed, not compared.</summary>
    Public Shared Function IsCompared(k_kind As ActivexElementKind) As Boolean
        Return k_kind <> ActivexElementKind.Handle AndAlso k_kind <> ActivexElementKind.Pid
    End Function

    Public Shared Function Build(k_lines As IEnumerable(Of ActivexLogLine)) As CompareSide
        Dim k_side As New CompareSide()
        For Each k_line As ActivexLogLine In k_lines
            Dim k_item As New CompareLine With {.Line = k_line, .Signature = Normalize(k_line.Text)}
            Dim k_same As List(Of CompareLine) = Nothing
            If Not k_side._bySignature.TryGetValue(k_item.Signature, k_same) Then
                k_same = New List(Of CompareLine)()
                k_side._bySignature.Add(k_item.Signature, k_same)
            End If
            k_item.Occurrence = k_same.Count
            k_same.Add(k_item)

            Dim k_loose As New StringBuilder(k_line.EventName)
            Dim k_hasEvent As Boolean = False
            For Each k_raw As String In k_line.Elements
                Dim k_bar As Integer = k_raw.IndexOf("|"c)
                Dim k_kind As ActivexElementKind = CType(CInt(k_raw.Substring(0, k_bar)), ActivexElementKind)
                Dim k_shown As String = Nothing
                Dim k_key As String = CompareKeyOf(k_kind, k_raw.Substring(k_bar + 1), k_shown)
                If k_kind = ActivexElementKind.EventName Then k_hasEvent = True
                If k_kind = ActivexElementKind.Place OrElse k_kind = ActivexElementKind.ClassName OrElse
                   k_kind = ActivexElementKind.Title Then k_loose.Append("|"c).Append(k_key)
                k_side.AddToElement(k_item, k_kind, k_shown, k_key)
            Next
            If Not k_hasEvent Then
                k_side.AddToElement(k_item, ActivexElementKind.EventName, NoEvent,
                                    ActivexElement.KeyOf(ActivexElementKind.EventName, NoEvent))
            End If
            k_item.Loose = k_loose.ToString()
            k_side.Lines.Add(k_item)
        Next
        Return k_side
    End Function

    Private Sub AddToElement(k_item As CompareLine, k_kind As ActivexElementKind, k_shown As String, k_key As String)
        If k_item.Keys.Contains(k_key) Then Return
        Dim k_element As CompareElement = Nothing
        If Not Elements.TryGetValue(k_key, k_element) Then
            k_element = New CompareElement With {.Key = k_key, .Kind = k_kind, .Value = k_shown}
            Elements.Add(k_key, k_element)
        End If
        k_element.Lines.Add(k_item)
        k_item.Keys.Add(k_key)
    End Sub

    ''' <summary>
    ''' Pairs the two sides: first the n-th line of a signature with the n-th line of the same signature, then the lines
    ''' left over, in order, with a left-over line of the other side that has the same event / place / class / title.
    ''' Then the status of every element and root, on both sides.
    ''' </summary>
    Public Shared Sub Pair(k_left As CompareSide, k_right As CompareSide)
        For Each k_item As CompareLine In k_left.Lines
            k_item.Twin = Nothing
            k_item.Similar = Nothing
        Next
        For Each k_item As CompareLine In k_right.Lines
            k_item.Twin = Nothing
            k_item.Similar = Nothing
        Next

        For Each k_item As CompareLine In k_left.Lines
            Dim k_same As List(Of CompareLine) = Nothing
            If k_right._bySignature.TryGetValue(k_item.Signature, k_same) AndAlso k_item.Occurrence < k_same.Count Then
                k_item.Twin = k_same(k_item.Occurrence)
                k_item.Twin.Twin = k_item
            End If
        Next

        Dim k_waiting As New Dictionary(Of String, Queue(Of CompareLine))()
        For Each k_item As CompareLine In k_right.Lines
            If k_item.Twin IsNot Nothing Then Continue For
            Dim k_queue As Queue(Of CompareLine) = Nothing
            If Not k_waiting.TryGetValue(k_item.Loose, k_queue) Then
                k_queue = New Queue(Of CompareLine)()
                k_waiting.Add(k_item.Loose, k_queue)
            End If
            k_queue.Enqueue(k_item)
        Next
        For Each k_item As CompareLine In k_left.Lines
            If k_item.Twin IsNot Nothing Then Continue For
            Dim k_queue As Queue(Of CompareLine) = Nothing
            If k_waiting.TryGetValue(k_item.Loose, k_queue) AndAlso k_queue.Count > 0 Then
                k_item.Similar = k_queue.Dequeue()
                k_item.Similar.Similar = k_item
            End If
        Next

        k_left.RateAgainst(k_right)
        k_right.RateAgainst(k_left)
    End Sub

    ' An element is the same when the other side has it with as many lines, each of them the twin of one of ours.
    Private Sub RateAgainst(k_other As CompareSide)
        Dim k_kinds As New Dictionary(Of ActivexElementKind, Integer)()
        Dim k_otherKinds As New Dictionary(Of ActivexElementKind, Integer)()
        For Each k_element As CompareElement In k_other.Elements.Values
            k_otherKinds(k_element.Kind) = If(k_otherKinds.ContainsKey(k_element.Kind), k_otherKinds(k_element.Kind), 0) + 1
        Next
        RootStatus.Clear()
        For Each k_element As CompareElement In Elements.Values
            k_kinds(k_element.Kind) = If(k_kinds.ContainsKey(k_element.Kind), k_kinds(k_element.Kind), 0) + 1
            Dim k_theirs As CompareElement = Nothing
            If Not IsCompared(k_element.Kind) Then
                k_element.Status = CompareStatus.NotCompared
            ElseIf Not k_other.Elements.TryGetValue(k_element.Key, k_theirs) Then
                k_element.Status = CompareStatus.Missing
            ElseIf k_theirs.Lines.Count = k_element.Lines.Count AndAlso
                   k_element.Lines.TrueForAll(Function(k_l) k_l.Twin IsNot Nothing AndAlso k_l.Twin.Keys.Contains(k_element.Key)) Then
                k_element.Status = CompareStatus.Same
            Else
                k_element.Status = CompareStatus.Differs
            End If

            If Not RootStatus.ContainsKey(k_element.Kind) Then
                RootStatus(k_element.Kind) = If(IsCompared(k_element.Kind), CompareStatus.Same, CompareStatus.NotCompared)
            End If
            If RootStatus(k_element.Kind) = CompareStatus.Same AndAlso k_element.Status <> CompareStatus.Same Then
                RootStatus(k_element.Kind) = CompareStatus.Differs
            End If
        Next
        For Each k_pair As KeyValuePair(Of ActivexElementKind, Integer) In k_kinds
            If RootStatus(k_pair.Key) <> CompareStatus.Same Then Continue For
            Dim k_theirCount As Integer = 0
            k_otherKinds.TryGetValue(k_pair.Key, k_theirCount)
            If k_theirCount <> k_pair.Value Then RootStatus(k_pair.Key) = CompareStatus.Differs
        Next
    End Sub

    ''' <summary>The tree: one root per kind, the elements under it (most lines first), the lines as leaves.</summary>
    ''' <param name="k_hideSame">Leaves out the lines identical in the other side, and the elements left with none.</param>
    Public Function BuildTree(k_hideSame As Boolean) As List(Of TreeNode)
        Roots.Clear()
        For Each k_item As CompareLine In Lines
            k_item.Nodes.Clear()
        Next
        Dim k_result As New List(Of TreeNode)()
        Dim k_elements As New List(Of CompareElement)(Elements.Values)
        k_elements.Sort(Function(a, b)
                            Dim k_byKind As Integer = a.Kind.CompareTo(b.Kind)
                            If k_byKind <> 0 Then Return k_byKind
                            Return b.Lines.Count.CompareTo(a.Lines.Count)
                        End Function)
        For Each k_element As CompareElement In k_elements
            k_element.Node = Nothing
            If k_hideSame AndAlso k_element.Status = CompareStatus.Same Then Continue For
            Dim k_node As New TreeNode() With {.Tag = New CompareNodeTag With {.Kind = k_element.Kind, .Element = k_element}}
            For Each k_item As CompareLine In k_element.Lines
                If k_hideSame AndAlso k_item.Status = CompareStatus.Same Then Continue For
                Dim k_leaf As New TreeNode() With {
                    .Tag = New CompareNodeTag With {.Kind = k_element.Kind, .Element = k_element, .Line = k_item}}
                k_item.Nodes(k_element.Key) = k_leaf
                k_node.Nodes.Add(k_leaf)
            Next
            ' A window or a pid (not compared) whose lines are all identical has nothing left to show.
            If k_node.Nodes.Count = 0 Then Continue For
            k_element.Node = k_node
            Dim k_root As TreeNode = Nothing
            If Not Roots.TryGetValue(k_element.Kind, k_root) Then
                k_root = New TreeNode() With {.Tag = New CompareNodeTag With {.Kind = k_element.Kind}}
                Roots.Add(k_element.Kind, k_root)
                k_result.Add(k_root)
            End If
            k_root.Nodes.Add(k_node)
        Next
        Paint()
        Return k_result
    End Function

    ''' <summary>Texts and colours of every node, from the statuses (again after <see cref="Pair"/>).</summary>
    Public Sub Paint()
        For Each k_pair As KeyValuePair(Of ActivexElementKind, TreeNode) In Roots
            Dim k_status As CompareStatus = CompareStatus.NotCompared
            RootStatus.TryGetValue(k_pair.Key, k_status)
            k_pair.Value.Text = $"{KindEmoji(k_pair.Key)} {KindText(k_pair.Key)}  ({k_pair.Value.Nodes.Count}){Mark(k_status)}"
            k_pair.Value.ForeColor = StatusColor(k_status)
        Next
        For Each k_element As CompareElement In Elements.Values
            If k_element.Node Is Nothing Then Continue For
            Dim k_icon As String = If(k_element.Kind = ActivexElementKind.EventName, EventEmoji(k_element.Value) & " ", "")
            k_element.Node.Text = $"{k_icon}{k_element.Value}  ({k_element.Lines.Count}){Mark(k_element.Status)}"
            k_element.Node.ForeColor = StatusColor(k_element.Status)
        Next
        For Each k_item As CompareLine In Lines
            Dim k_text As String = LeafText(k_item)
            For Each k_leaf As TreeNode In k_item.Nodes.Values
                k_leaf.Text = k_text
                k_leaf.ForeColor = StatusColor(k_item.Status)
            Next
        Next
    End Sub

    Private Shared Function LeafText(k_item As CompareLine) As String
        Dim k_body As String = k_item.Line.Text
        If k_body.Length > 220 Then k_body = k_body.Substring(0, 220) & "…"
        Return $"{EventEmoji(k_item.Line.EventName)} {k_item.Line.Stamp}  {k_body}{Mark(k_item.Status)}"
    End Function

    ' ── What the operator sees: emoji, kind names, marks, colours ───────────────

    Public Shared Function KindText(k_kind As ActivexElementKind) As String
        Return New ActivexElement(k_kind, "").KindText
    End Function

    Public Shared Function KindEmoji(k_kind As ActivexElementKind) As String
        Select Case k_kind
            Case ActivexElementKind.EventName : Return "⚡"
            Case ActivexElementKind.Place : Return "📍"
            Case ActivexElementKind.ClassName : Return "🧩"
            Case ActivexElementKind.Title : Return "🏷"
            Case ActivexElementKind.Handle : Return "🔲"
            Case ActivexElementKind.Pid : Return "🔢"
            Case ActivexElementKind.Process : Return "⚙"
            Case Else : Return "🏁"
        End Select
    End Function

    Public Shared Function EventEmoji(k_event As String) As String
        Select Case k_event
            Case "LOAD" : Return "📂"
            Case "DOCUMENT" : Return "📄"
            Case "PRIMER" : Return "🧪"
            Case "CREATE" : Return "➕"
            Case "DESTROY" : Return "❌"
            Case "SHOW" : Return "👁"
            Case "HIDE" : Return "🙈"
            Case "FOCUS" : Return "🎯"
            Case "FOREGROUND" : Return "🔝"
            Case "LOCATIONCHANGE" : Return "↔"
            Case "STATECHANGE" : Return "🔄"
            Case "MILESTONE" : Return "🏁"
            Case "CLICK" : Return "🖱"
            Case "OPERATOR" : Return "👤"
            Case "REACTIVATE" : Return "🔁"
            Case "BOX TEXT" : Return "💬"
            Case "SIZE NUDGE" : Return "📐"
            Case "CTRL+H" : Return "⌨"
            Case "RELEASE" : Return "🗑"
            Case "EnsureHost" : Return "🏗"
            Case "Clear" : Return "🧹"
            Case "processes at load" : Return "⚙"
            Case "tree" : Return "🌳"
            Case "tree row" : Return "🌿"
            Case "mouse hook" : Return "🐭"
            Case "form closed" : Return "🚪"
            Case "watch stopped" : Return "⏹"
            Case "ShowDocument EXCEPTION" : Return "💥"
            Case Else : Return "•"
        End Select
    End Function

    Public Shared Function Mark(k_status As CompareStatus) As String
        Select Case k_status
            Case CompareStatus.Same : Return "  ✓"
            Case CompareStatus.Differs : Return "  ≠"
            Case CompareStatus.Missing : Return "  ✗"
            Case Else : Return ""
        End Select
    End Function

    Public Shared Function StatusText(k_status As CompareStatus) As String
        Select Case k_status
            Case CompareStatus.Same : Return "identic în celălalt"
            Case CompareStatus.Differs : Return "diferă față de celălalt"
            Case CompareStatus.Missing : Return "lipsește din celălalt"
            Case Else : Return "nu se compară (diferă la fiecare pornire)"
        End Select
    End Function

    Public Shared Function StatusColor(k_status As CompareStatus) As Color
        Dim k_palette As ThemePalette = ThemeManager.Current.Palette
        Select Case k_status
            Case CompareStatus.Same : Return k_palette.SuccessColor
            Case CompareStatus.Differs : Return k_palette.WarningColor
            Case CompareStatus.Missing : Return k_palette.ErrorColor
            Case Else : Return k_palette.TextDimColor
        End Select
    End Function

End Class
#End If
