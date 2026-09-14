Option Strict On
Imports System.ComponentModel
Imports System.Linq

''' <summary>
''' SELECTING SEVERAL ROWS AT ONCE (Ctrl and Shift, as in Windows).
'''
''' <para><b>One root at a time.</b> A group never spans two roots. The tree is not a flat list:
''' its roots are the things themselves (a receipt, the basket of loose snapshots) and its leaves
''' belong to one of them. A group reaching across two roots would be a group of rows that have
''' nothing in common, and every host would have to split it again before it could do anything
''' with it. Clicking with Ctrl or Shift on another root's row therefore does not extend the
''' group — it starts a new one there.</para>
'''
''' <para><b>The focus row stays what it always was.</b> <see cref="SelectedNode"/> keeps meaning
''' «the row the operator last touched», and it is always part of the group. Every host written
''' before this file keeps working unchanged: with <see cref="MultiSelect"/> off the group never
''' holds more than that one row.</para>
'''
''' <para><b>Plain click inside a group does not break it — yet.</b> Windows collapses a group to
''' the clicked row on mouse UP, not on mouse DOWN, and for one reason: pressing on a row of the
''' group is also how a drag of the WHOLE group starts. Collapsing on the press would make
''' dragging several rows impossible, because by the time the mouse moved there would be one row
''' left. See <c>_pendingSingleSelect</c> below.</para>
'''
''' <para><b>Off by default.</b> The nine views that already use the tree do not change behaviour
''' because this file exists.</para>
''' </summary>
Partial Public Class AdvancedTreeControl

    ' The group, focus row included. Empty means «whatever pSelectedItem says», so the two can
    ' never disagree about a single-row selection.
    Private ReadOnly _selection As New List(Of TreeItem)()

    ' Where a Shift range starts from. Also the row whose root decides which rows may join.
    Private _selectionAnchor As TreeItem = Nothing

    ' A plain click landed inside an existing group. The collapse to that one row is owed, and
    ' is paid on mouse up — unless a drag started in between, which cancels the debt.
    Private _pendingSingleSelect As TreeItem = Nothing

    ' ══════════════════════════════════════════════════════════════════════════
    ' Properties
    ' ══════════════════════════════════════════════════════════════════════════

    Private _multiSelect As Boolean = False
    <Category("K-BOT: Selection")>
    <Description("Permite selectarea mai multor rânduri ale ACELEIAȘI rădăcini, cu Ctrl și Shift.")>
    <DefaultValue(False)>
    Public Property MultiSelect As Boolean
        Get
            Return _multiSelect
        End Get
        Set(value As Boolean)
            If _multiSelect = value Then Return
            _multiSelect = value
            ' Turning it off leaves the focus row alone on screen: a group nobody can see or
            ' undo would still be a group as far as the hosts reading SelectedNodes go.
            If Not value Then ResetSelectionTo(pSelectedItem)
        End Set
    End Property

    ''' <summary>
    ''' The selected rows, focus row included, in the order they appear on screen. Never
    ''' <c>Nothing</c>; one row when nothing special is going on; empty when nothing is selected.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SelectedNodes As IReadOnlyList(Of TreeItem)
        Get
            If _selection.Count > 0 Then Return _selection.ToArray()
            If pSelectedItem Is Nothing Then Return Array.Empty(Of TreeItem)()
            Return New TreeItem() {pSelectedItem}
        End Get
    End Property

    ''' <summary>How many rows are selected. Cheaper than building <see cref="SelectedNodes"/>.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SelectedNodeCount As Integer
        Get
            If _selection.Count > 0 Then Return _selection.Count
            Return If(pSelectedItem Is Nothing, 0, 1)
        End Get
    End Property

    ''' <summary>The group changed — by mouse, by keyboard or from outside.</summary>
    Public Event SelectedNodesChanged(sender As Object, e As EventArgs)

    ' ══════════════════════════════════════════════════════════════════════════
    ' Public gestures
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Is this row part of the current group?</summary>
    Public Function IsNodeSelected(node As TreeItem) As Boolean
        Return IsRowSelected(node)
    End Function

    ''' <summary>Leaves the focus row selected and nothing else.</summary>
    Public Sub ClearNodeSelection()
        Try
            ResetSelectionTo(pSelectedItem)
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.ClearNodeSelection", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Selects a group from outside — a search result, a chart, a list beside the tree. Rows of
    ''' another root than the first one are dropped, by the same rule the mouse follows.
    ''' </summary>
    Public Sub SelectNodes(nodes As IEnumerable(Of TreeItem))
        Try
            If nodes Is Nothing Then
                ResetSelectionTo(Nothing)
                Return
            End If

            Dim cerute As List(Of TreeItem) = nodes.Where(Function(n) n IsNot Nothing).ToList()
            If cerute.Count = 0 Then
                ResetSelectionTo(Nothing)
                Return
            End If

            Dim root As TreeItem = RootOf(cerute(0))
            Dim ordonate As New List(Of TreeItem)()
            For Each it As TreeItem In GetVisibleItems()
                If it.IsLoader Then Continue For
                If RootOf(it) IsNot root Then Continue For
                If ContainsRef(cerute, it) Then ordonate.Add(it)
            Next
            If ordonate.Count = 0 Then ordonate.Add(cerute(0))

            _selection.Clear()
            _selection.AddRange(ordonate)
            pSelectedItem = ordonate(ordonate.Count - 1)
            _selectionAnchor = ordonate(0)
            AnuntaSelectia()
            Me.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.SelectNodes", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The mouse — called from OnMouseDown / OnMouseUp
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' What a press on <paramref name="it"/> does to the group. The only entry point the mouse
    ''' has: every branch of <c>OnMouseDown</c> that used to write <c>pSelectedItem</c> by hand
    ''' comes through here or through <see cref="SelectSingle"/>, so the group and the focus row
    ''' cannot drift apart.
    ''' </summary>
    Friend Sub ApplyMouseSelection(it As TreeItem, e As MouseEventArgs)
        _pendingSingleSelect = Nothing
        If it Is Nothing Then Return

        If Not _multiSelect Then
            pSelectedItem = it
            Return
        End If

        ' Right button: a row already in the group keeps the group, because the menu that
        ' follows is about all of them. Any other row starts over, alone.
        If e.Button = MouseButtons.Right Then
            If IsRowSelected(it) Then
                pSelectedItem = it
                Return
            End If
            SelectSingle(it)
            Return
        End If

        Dim ctrl As Boolean = (Control.ModifierKeys And Keys.Control) = Keys.Control
        Dim shift As Boolean = (Control.ModifierKeys And Keys.Shift) = Keys.Shift

        If shift AndAlso _selectionAnchor IsNot Nothing AndAlso SameRoot(_selectionAnchor, it) Then
            SelectRange(_selectionAnchor, it)
            Return
        End If

        If ctrl Then
            ' Another root: not an extension, a fresh start there.
            If _selectionAnchor Is Nothing OrElse Not SameRoot(_selectionAnchor, it) Then
                SelectSingle(it)
                Return
            End If
            ToggleNode(it)
            Return
        End If

        ' No modifier, inside a group: the collapse is owed until mouse up — see the class
        ' comment. Without this the operator could never drag more than one row.
        If _selection.Count > 1 AndAlso IsRowSelected(it) Then
            _pendingSingleSelect = it
            pSelectedItem = it
            Me.Invalidate()
            Return
        End If

        SelectSingle(it)
    End Sub

    ''' <summary>
    ''' Pays the collapse owed by a plain click inside a group. Called first thing in
    ''' <c>OnMouseUp</c>; a drag that started in between has already cleared the debt.
    ''' </summary>
    Friend Sub SettlePendingSelection()
        If _pendingSingleSelect Is Nothing Then Return
        Dim it As TreeItem = _pendingSingleSelect
        _pendingSingleSelect = Nothing
        SelectSingle(it)
    End Sub

    ''' <summary>One row selected and no group. The plain, single-row case of everything above.</summary>
    Friend Sub SelectSingle(it As TreeItem)
        ResetSelectionTo(it)
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' The rows that a drag started on <paramref name="it"/> carries: the whole group when the
    ''' press landed inside one, that row alone otherwise.
    ''' </summary>
    Friend Function DragGroupFor(it As TreeItem) As List(Of TreeItem)
        Dim grup As New List(Of TreeItem)()
        If it Is Nothing Then Return grup
        If _multiSelect AndAlso _selection.Count > 1 AndAlso ContainsRef(_selection, it) Then
            grup.AddRange(_selection)
            Return grup
        End If
        grup.Add(it)
        Return grup
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' The keyboard — called from OnKeyDown
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' What an arrow key does to the group, after the focus row has already moved. Shift keeps
    ''' the anchor and stretches the range to the new row; anything else drops the group.
    ''' </summary>
    Friend Sub ApplyKeySelection(modifiers As Keys)
        If Not _multiSelect Then Return
        If (modifiers And Keys.Shift) = Keys.Shift AndAlso
           _selectionAnchor IsNot Nothing AndAlso pSelectedItem IsNot Nothing AndAlso
           SameRoot(_selectionAnchor, pSelectedItem) Then
            Dim ancora As TreeItem = _selectionAnchor
            SelectRange(ancora, pSelectedItem)
            _selectionAnchor = ancora
            Return
        End If
        ResetSelectionTo(pSelectedItem)
    End Sub

    ''' <summary>
    ''' Ctrl+A: every visible row of the focus row's root, the root itself excepted. Answers
    ''' False when there was nothing to select, so the key stays unhandled.
    ''' </summary>
    Friend Function SelectWholeRoot() As Boolean
        If Not _multiSelect Then Return False
        If pSelectedItem Is Nothing Then Return False

        Dim root As TreeItem = RootOf(pSelectedItem)
        Dim gasite As New List(Of TreeItem)()
        For Each it As TreeItem In GetVisibleItems()
            If it.IsLoader OrElse it Is root Then Continue For
            If RootOf(it) Is root Then gasite.Add(it)
        Next
        If gasite.Count = 0 Then Return False

        _selection.Clear()
        _selection.AddRange(gasite)
        _selectionAnchor = gasite(0)
        pSelectedItem = gasite(gasite.Count - 1)
        AnuntaSelectia()
        Me.Invalidate()
        Return True
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' Inside
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Is this row painted as selected? Read by the painting partial.</summary>
    Friend Function IsRowSelected(it As TreeItem) As Boolean
        If it Is Nothing Then Return False
        If it Is pSelectedItem Then Return True
        If Not _multiSelect Then Return False
        Return ContainsRef(_selection, it)
    End Function

    ''' <summary>Forgets the group entirely — called from <c>Clear</c>, where the rows go away.</summary>
    Friend Sub ForgetSelection()
        _selection.Clear()
        _selectionAnchor = Nothing
        _pendingSingleSelect = Nothing
    End Sub

    ''' <summary>
    ''' The group becomes exactly one row (or none). The single place that writes both halves,
    ''' so «the focus row is always in the group» holds by construction.
    ''' </summary>
    Friend Sub ResetSelectionTo(it As TreeItem)
        Dim eraGrup As Boolean = _selection.Count > 1
        _selection.Clear()
        If it IsNot Nothing Then _selection.Add(it)
        pSelectedItem = it
        _selectionAnchor = it
        _pendingSingleSelect = Nothing
        If eraGrup OrElse it IsNot Nothing Then AnuntaSelectia()
    End Sub

    Private Sub SelectRange(fromItem As TreeItem, toItem As TreeItem)
        Dim vizibile As List(Of TreeItem) = GetVisibleItems()
        Dim i1 As Integer = IndexOfRef(vizibile, fromItem)
        Dim i2 As Integer = IndexOfRef(vizibile, toItem)
        If i1 < 0 OrElse i2 < 0 Then
            SelectSingle(toItem)
            Return
        End If

        Dim start As Integer = Math.Min(i1, i2)
        Dim stop_ As Integer = Math.Max(i1, i2)
        Dim root As TreeItem = RootOf(toItem)

        _selection.Clear()
        For i As Integer = start To stop_
            Dim it As TreeItem = vizibile(i)
            If it.IsLoader Then Continue For
            ' The rows in between may belong to another root — a range dragged past the end of
            ' one receipt's chain would otherwise swallow the next receipt.
            If RootOf(it) IsNot root Then Continue For
            _selection.Add(it)
        Next
        If _selection.Count = 0 Then _selection.Add(toItem)

        pSelectedItem = toItem
        _selectionAnchor = fromItem
        AnuntaSelectia()
        Me.Invalidate()
    End Sub

    Private Sub ToggleNode(it As TreeItem)
        ' A group of one that nobody built by hand: the focus row joins first, otherwise the
        ' very first Ctrl+click would throw away the row the operator was standing on.
        If _selection.Count = 0 AndAlso pSelectedItem IsNot Nothing Then _selection.Add(pSelectedItem)

        Dim idx As Integer = IndexOfRef(_selection, it)
        If idx >= 0 Then
            _selection.RemoveAt(idx)
            ' The focus row cannot be one that is no longer selected: the last row left takes
            ' over, and an empty group leaves nothing selected at all.
            If it Is pSelectedItem Then
                pSelectedItem = If(_selection.Count > 0, _selection(_selection.Count - 1), Nothing)
            End If
        Else
            _selection.Add(it)
            pSelectedItem = it
        End If

        _selectionAnchor = it
        AnuntaSelectia()
        Me.Invalidate()
    End Sub

    ''' <summary>The level-0 ancestor — what «the same root» means everywhere in this file.</summary>
    Private Shared Function RootOf(it As TreeItem) As TreeItem
        If it Is Nothing Then Return Nothing
        Dim cursor As TreeItem = it
        While cursor.Parent IsNot Nothing
            cursor = cursor.Parent
        End While
        Return cursor
    End Function

    Private Shared Function SameRoot(a As TreeItem, b As TreeItem) As Boolean
        If a Is Nothing OrElse b Is Nothing Then Return False
        Return RootOf(a) Is RootOf(b)
    End Function

    ' Reference identity, not Equals: two different rows may well carry equal contents, and the
    ' group is about the rows on screen.
    Private Shared Function IndexOfRef(lista As List(Of TreeItem), it As TreeItem) As Integer
        If lista Is Nothing OrElse it Is Nothing Then Return -1
        For i As Integer = 0 To lista.Count - 1
            If lista(i) Is it Then Return i
        Next
        Return -1
    End Function

    Private Shared Function ContainsRef(lista As List(Of TreeItem), it As TreeItem) As Boolean
        Return IndexOfRef(lista, it) >= 0
    End Function

    ''' <summary>
    ''' Raises <see cref="SelectedNodesChanged"/>. A UI boundary: a host that throws while
    ''' reading the group must not take the tree's mouse handling down with it.
    ''' </summary>
    Private Sub AnuntaSelectia()
        Try
            RaiseEvent SelectedNodesChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.AnuntaSelectia", ex)
        End Try
    End Sub
End Class
