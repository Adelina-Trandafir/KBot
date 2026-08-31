Imports System.Reflection

Partial Public Class AdvancedTreeControl
    ' Funcția pentru adăugarea unui element nou în arbore
    Public Function AddItem(pKey As String, pCaption As String,
                            Optional pParent As TreeItem = Nothing,
                            Optional pLeftIconClosed As Image = Nothing,
                            Optional pLeftIconOpen As Image = Nothing,
                            Optional pRightIcon As Image = Nothing,
                            Optional pTag As String = Nothing,
                            Optional pExpanded As Boolean = False,
                            Optional pLazyNode As Boolean = False) As TreeItem

        Dim it As New TreeItem With {
            .Key = pKey,
            .Tag = pTag,
            .Caption = pCaption,
            .Parent = pParent,
            .LeftIconClosed = pLeftIconClosed,
            .LeftIconOpen = pLeftIconOpen,
            .RightIcon = pRightIcon,
            .Expanded = pExpanded,
            .LazyNode = pLazyNode
        }

        If pParent Is Nothing Then
            it.Level = 0
            Items.Add(it)
        Else
            it.Level = pParent.Level + 1
            pParent.Children.Add(it)
        End If

        Me.Invalidate()
        Return it
    End Function

    ' Funcția care primește string-ul din VBA și returnează valoarea
    Public Function ProcessPropertyRequest(cmd As String) As String
        ' Format așteptat: "GET_PROPERTY||PropName||[OptionalNodeID]"
        Dim parts() As String = cmd.Split(separator, StringSplitOptions.None)

        If parts.Length < 2 Then Return "ERROR: Invalid Format"

        Dim propName As String = parts(1)
        Dim result As String = "NOT_FOUND"

        Try
            ' === CAZUL 1: PROPRIETATE A CONTROLULUI (GLOBAL) ===
            If parts.Length = 2 Then
                ' Căutăm proprietatea în clasa AdvancedTreeControl (Me)
                Dim propInfo As PropertyInfo = Me.GetType().GetProperty(propName, BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.IgnoreCase)

                If propInfo IsNot Nothing Then
                    Dim val = propInfo.GetValue(Me, Nothing)
                    result = FormatValue(val)
                Else
                    result = "ERROR: Property '" & propName & "' not found on Tree."
                End If

                ' === CAZUL 2: PROPRIETATE A UNUI NOD ===
            ElseIf parts.Length = 3 Then
                Dim nodeID As String = parts(2)

                ' 1. Găsim nodul după ID (care e Key în VBA)
                Dim node As TreeItem = FindNodeByID(nodeID)

                If node IsNot Nothing Then
                    ' 2. Căutăm proprietatea în clasa TreeItem
                    Dim propInfo As PropertyInfo = node.GetType().GetProperty(propName, BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.IgnoreCase)

                    If propInfo IsNot Nothing Then
                        Dim val = propInfo.GetValue(node, Nothing)
                        result = FormatValue(val)
                    Else
                        result = "ERROR: Property '" & propName & "' not found on Node."
                    End If
                Else
                    result = "ERROR: Node with ID '" & nodeID & "' not found."
                End If
            End If

        Catch ex As Exception
            result = "ERROR: " & ex.Message
        End Try

        Return result
    End Function

    ' Metodă publică pentru a seta starea checkbox-ului din exterior (VBA) cu propagare
    Public Sub SetItemCheckState(pItem As TreeItem, pState As TreeCheckState)
        SetNodeStateWithPropagation(pItem, pState)
        Me.Invalidate()
    End Sub

    ' Setează un nod ca radio-selectat din exterior (VBA), deselectând frații
    Public Sub SetRadioSelected(pItem As TreeItem)
        If pItem Is Nothing Then Return
        If pItem.Level <> _radioButtonLevel Then Return

        Dim siblings As List(Of TreeItem) = If(pItem.Parent IsNot Nothing, pItem.Parent.Children, Me.Items)

        ' Capturăm nodeOff
        Dim nodeOff As TreeItem = Nothing
        For Each sibling In siblings
            If sibling.Level = _radioButtonLevel AndAlso sibling.IsRadioSelected Then
                nodeOff = sibling
                Exit For
            End If
        Next

        ' Ștergem checkboxurile copiilor lui nodeOff
        If nodeOff IsNot Nothing Then
            ClearChildrenCheckboxes(nodeOff)
        End If

        ' Resetare frați + selectare
        For Each sibling In siblings
            If sibling.Level = _radioButtonLevel Then
                sibling.IsRadioSelected = False
            End If
        Next

        pItem.IsRadioSelected = True

        CheckChildrenRecursive(pItem)

        RaiseEvent NodeRadioSelected(pItem, nodeOff)
        Me.Invalidate()
    End Sub

    ' Metodă publică pentru a goli toate elementele din control
    Public Sub Clear()
        ' 1. Oprim orice desenare sau calcul de layout
        Me.SuspendLayout()

        ' 2. Golim datele
        Items.Clear()
        pSelectedItem = Nothing
        pHoveredItem = Nothing

        ' 3. Resetăm Scroll-ul la zero (CRITIC)
        Me.AutoScrollPosition = New Point(0, 0)
        Me.AutoScrollMinSize = Size.Empty

        ' 4. Repornim logica
        Me.ResumeLayout(False)
        Me.PerformLayout()

        ' 5. Forțăm redesenarea imediată a întregului control
        Me.Invalidate()
        Me.Update()
    End Sub

    ''' <summary>
    ''' Selects <paramref name="node"/> and does whatever it takes for it to be ON SCREEN: expands
    ''' every ancestor that hides it, then scrolls the least amount needed.
    ''' </summary>
    ''' <remarks>
    ''' <para>Writing <see cref="SelectedNode"/> on its own is not enough, and that is the whole
    ''' reason this exists. A node inside a collapsed parent is not in the visible list at all, so
    ''' the selection is real but invisible and the operator sees a click that did nothing. Every
    ''' caller that selects from OUTSIDE the tree — a point on a chart, a search hit, a row in a
    ''' list beside it — wants both halves, and each of them writing the same three lines is how
    ''' the two halves eventually drift apart.</para>
    ''' <para>Focus is deliberately NOT taken. The operator clicked something else; pulling the
    ''' caret over here would make their next arrow key move a tree they were not looking at.</para>
    ''' <para>A node that is not in this tree is a no-op, not an error: the caller is usually
    ''' matching two lists and «no match here» is an ordinary answer.</para>
    ''' </remarks>
    Public Sub SelectAndReveal(node As TreeItem)
        Try
            If node Is Nothing Then Return

            Dim ancestor As TreeItem = node.Parent
            While ancestor IsNot Nothing
                ancestor.Expanded = True
                ancestor = ancestor.Parent
            End While

            SelectedNode = node
            ' The expanding above changed how tall the content is, and EnsureNodeVisible measures
            ' against the scrollbar — so the range has to be right BEFORE it is asked.
            RefreshScrollVisibility()
            EnsureNodeVisible(node)
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.SelectAndReveal", ex)
        End Try
    End Sub
End Class
