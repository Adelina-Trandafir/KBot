Imports System.Drawing
Imports System.Reflection
Imports System.Text
Imports System.Windows.Forms
Imports WorkflowModels

Partial Public Class KBOT_STANDALONE

    ' ── Câmpuri ─────────────────────────────────────────────────────────────────
    Private _treeStep As TreeView = Nothing
    Private _treeStepTip As ToolTip = Nothing
    Private _onActionStartHandler As WorkflowExecutor.OnActionStartEventHandler = Nothing
    Private _lastHoveredNode As TreeNode = Nothing

    ' Atribut cheie afișat în label — primul găsit câștigă
    Private Shared ReadOnly _treeKeyAttrs As String() = {
        "Selector", "Value", "Message", "Source",
        "WorkflowPath", "FieldName", "Iterations", "Seconds", "SaveTo", "Path"
    }

    ' ── Build & Show ─────────────────────────────────────────────────────────────

    Friend Sub BuildAndShowStepTree(parsedWorkflows As List(Of (Name As String, Wfl As Workflow)))
        ' Prima rulare: creăm controalele
        If _treeStep Is Nothing Then
            _treeStep = New TreeView() With {
                .Font = New Font("Consolas", 9.0F),
                .BackColor = If(KBotTheme.IsDark, KBotTheme.CLR_BG, SystemColors.Window),
                .ForeColor = If(KBotTheme.IsDark, KBotTheme.CLR_FG, SystemColors.WindowText),
                .BorderStyle = BorderStyle.None,
                .ShowLines = True,
                .ShowRootLines = True,
                .FullRowSelect = True,
                .HotTracking = False,
                .ShowNodeToolTips = False   ' gestionăm noi manual
            }

            _treeStepTip = New ToolTip() With {
                .AutoPopDelay = 15000,
                .InitialDelay = 400,
                .ReshowDelay = 150,
                .ShowAlways = True
            }

            AddHandler _treeStep.MouseMove, AddressOf StepTree_MouseMove
            AddHandler _treeStep.MouseLeave, AddressOf StepTree_MouseLeave

            ' Adăugăm în același container ca lstWorkflows, în aceeași celulă TLP
            _treeStep.Dock = DockStyle.Fill
            _treeStep.Margin = lstWorkflows.Margin

            pnlLeftContent.Controls.Add(_treeStep)
        End If

        ' Populăm
        _treeStep.BeginUpdate()
        _treeStep.Nodes.Clear()

        For Each entry In parsedWorkflows
            Dim rootNode As New TreeNode($"▶  {entry.Name}") With {
                .ForeColor = If(KBotTheme.IsDark, Color.FromArgb(86, 156, 214), Color.FromArgb(0, 80, 170)),
                .Tag = Nothing
            }
            BuildTreeNodes(entry.Wfl.Actions, rootNode.Nodes)
            _treeStep.Nodes.Add(rootNode)
            rootNode.Expand()
        Next

        _treeStep.EndUpdate()

        ' Swap vizibilitate
        lstWorkflows.Visible = False
        _treeStep.Visible = True
        _treeStep.BringToFront()
    End Sub

    Friend Sub HideStepTree()
        If _treeStep IsNot Nothing Then
            _treeStep.Visible = False
        End If
        lstWorkflows.Visible = True
    End Sub

    ''' <summary>Re-aplică culorile TreeView-ului conform temei curente.</summary>
    Friend Sub ApplyStepTreeTheme()
        If _treeStep Is Nothing Then Return
        Dim isDark = KBotTheme.IsDark
        _treeStep.BackColor = If(isDark, KBotTheme.CLR_BG, SystemColors.Window)
        _treeStep.ForeColor = If(isDark, KBotTheme.CLR_FG, SystemColors.WindowText)
        ' Actualizăm culorile fiecărui nod
        UpdateNodeColors(_treeStep.Nodes, isDark)
        _treeStep.Invalidate()
    End Sub

    Private Sub UpdateNodeColors(nodes As TreeNodeCollection, isDark As Boolean)
        For Each node As TreeNode In nodes
            If TypeOf node.Tag Is IWorkflowAction Then
                node.ForeColor = GetActionColor(DirectCast(node.Tag, IWorkflowAction))
            ElseIf node.Text.StartsWith("▶") Then
                ' Nod rădăcină (workflow name)
                node.ForeColor = If(isDark, Color.FromArgb(86, 156, 214), Color.FromArgb(0, 80, 170))
            ElseIf node.Text = "<Else>" Then
                node.ForeColor = If(isDark, Color.FromArgb(197, 134, 192), Color.FromArgb(130, 50, 140))
            End If
            UpdateNodeColors(node.Nodes, isDark)
        Next
    End Sub

    ' ── Construire noduri ────────────────────────────────────────────────────────

    Private Sub BuildTreeNodes(actions As List(Of IWorkflowAction), nodes As TreeNodeCollection)
        For Each action In actions
            Dim label As String = BuildActionLabel(action)
            Dim node As New TreeNode(label) With {
                .Tag = action,
                .ForeColor = GetActionColor(action)
            }
            nodes.Add(node)

            ' Children (IfExists, While, ForEach, etc.)
            Dim childProp = action.GetType().GetProperty("Children")
            If childProp IsNot Nothing Then
                Dim children = TryCast(childProp.GetValue(action), List(Of IWorkflowAction))
                If children IsNot Nothing AndAlso children.Count > 0 Then
                    BuildTreeNodes(children, node.Nodes)
                    node.Expand()
                End If
            End If

            ' ElseChildren
            Dim elseProp = action.GetType().GetProperty("ElseChildren")
            If elseProp IsNot Nothing Then
                Dim elseChildren = TryCast(elseProp.GetValue(action), List(Of IWorkflowAction))
                If elseChildren IsNot Nothing AndAlso elseChildren.Count > 0 Then
                    Dim elseNode As New TreeNode("<Else>") With {
                        .ForeColor = If(KBotTheme.IsDark, Color.FromArgb(197, 134, 192), Color.FromArgb(120, 30, 140)),
                        .Tag = Nothing
                    }
                    nodes.Add(elseNode)
                    BuildTreeNodes(elseChildren, elseNode.Nodes)
                    elseNode.Expand()
                End If
            End If
        Next
    End Sub

    Private Shared Function BuildActionLabel(action As IWorkflowAction) As String
        For Each attrName In _treeKeyAttrs
            Dim prop = action.GetType().GetProperty(attrName, BindingFlags.Public Or BindingFlags.Instance)
            If prop Is Nothing OrElse prop.PropertyType IsNot GetType(String) Then Continue For
            Dim val = TryCast(prop.GetValue(action), String)
            If Not String.IsNullOrWhiteSpace(val) Then
                If val.Length > 45 Then val = val.Substring(0, 42) & "..."
                Return $"{action.ActionType}  [{val}]"
            End If
        Next
        Return action.ActionType
    End Function

    Private Shared Function GetActionColor(action As IWorkflowAction) As Color
        If KBotTheme.IsDark Then
            Select Case action.ActionType.ToLower()
                Case "click", "authclick" : Return Color.FromArgb(86, 156, 214)
                Case "fill", "select", "upload" : Return Color.FromArgb(206, 145, 120)
                Case "ifexists", "ifunique", "ifvar" : Return Color.FromArgb(197, 134, 192)
                Case "foreach", "foreachvar", "while", "repeat" : Return Color.FromArgb(78, 201, 176)
                Case "log" : Return Color.FromArgb(106, 153, 85)
                Case "screenshot", "read", "scraptable" : Return Color.FromArgb(220, 220, 100)
                Case "stop", "exit" : Return Color.FromArgb(252, 57, 57)
                Case Else : Return Color.FromArgb(200, 200, 200)
            End Select
        Else
            Select Case action.ActionType.ToLower()
                Case "click", "authclick" : Return Color.FromArgb(0, 80, 170)
                Case "fill", "select", "upload" : Return Color.FromArgb(160, 70, 30)
                Case "ifexists", "ifunique", "ifvar" : Return Color.FromArgb(120, 30, 140)
                Case "foreach", "foreachvar", "while", "repeat" : Return Color.FromArgb(0, 130, 110)
                Case "log" : Return Color.FromArgb(0, 120, 0)
                Case "screenshot", "read", "scraptable" : Return Color.FromArgb(130, 120, 0)
                Case "stop", "exit" : Return Color.FromArgb(180, 0, 0)
                Case Else : Return Color.FromArgb(40, 40, 40)
            End Select
        End If
    End Function

    ' ── Highlight live ───────────────────────────────────────────────────────────

    Friend Sub WireActionStart()
        If _onActionStartHandler IsNot Nothing Then
            RemoveHandler _executor.OnActionStart, _onActionStartHandler
        End If

        _onActionStartHandler = Sub(action As IWorkflowAction)
                                    If _treeStep Is Nothing OrElse Not _treeStep.Visible Then Return
                                    Me.Invoke(Sub() HighlightActionNode(action))
                                End Sub

        AddHandler _executor.OnActionStart, _onActionStartHandler
    End Sub

    Private Sub HighlightActionNode(action As IWorkflowAction)
        ' Reset nod anterior
        If _treeStep.SelectedNode IsNot Nothing Then
            Dim prev = _treeStep.SelectedNode
            prev.BackColor = Color.Empty
            prev.ForeColor = If(TypeOf prev.Tag Is IWorkflowAction,
                                GetActionColor(DirectCast(prev.Tag, IWorkflowAction)),
                                If(KBotTheme.IsDark, Color.FromArgb(200, 200, 200), Color.FromArgb(40, 40, 40)))
        End If

        Dim node = FindNodeByAction(action, _treeStep.Nodes)
        If node Is Nothing Then Return

        node.BackColor = If(KBotTheme.IsDark, Color.FromArgb(50, 80, 50), Color.FromArgb(200, 240, 200))
        node.ForeColor = If(KBotTheme.IsDark, Color.White, Color.FromArgb(0, 60, 0))
        _treeStep.SelectedNode = node
        node.EnsureVisible()
    End Sub

    Private Shared Function FindNodeByAction(action As IWorkflowAction, nodes As TreeNodeCollection) As TreeNode
        For Each node As TreeNode In nodes
            If ReferenceEquals(node.Tag, action) Then Return node
            Dim found = FindNodeByAction(action, node.Nodes)
            If found IsNot Nothing Then Return found
        Next
        Return Nothing
    End Function

    ' ── Tooltip cu valori live ───────────────────────────────────────────────────

    Private Sub StepTree_MouseMove(sender As Object, e As MouseEventArgs)
        Dim node = _treeStep.GetNodeAt(e.Location)

        If node Is _lastHoveredNode Then Return
        _lastHoveredNode = node

        If node Is Nothing OrElse Not TypeOf node.Tag Is IWorkflowAction Then
            _treeStepTip.Hide(_treeStep)
            Return
        End If

        Dim action = DirectCast(node.Tag, IWorkflowAction)
        Dim sb As New StringBuilder()
        sb.AppendLine($"[{action.ActionType}]")
        sb.AppendLine(New String("─"c, 40))

        For Each prop In action.GetType().GetProperties(BindingFlags.Public Or BindingFlags.Instance)
            ' Sărim non-string și cele fără sens vizual
            If prop.PropertyType IsNot GetType(String) Then Continue For
            If {"ActionType", "LogValue"}.Contains(prop.Name) Then Continue For

            Dim raw = TryCast(prop.GetValue(action), String)
            If String.IsNullOrWhiteSpace(raw) Then Continue For

            ' Rezolvăm [[INTERNAL_VAR]] din starea curentă a executorului
            Dim resolved = If(_executor IsNot Nothing, _executor.ResolveText(raw), raw)

            If resolved <> raw Then
                ' Variabila s-a rezolvat — arătăm ambele valori
                sb.AppendLine($"{prop.Name}:")
                sb.AppendLine($"  template : {Trunc(raw, 55)}")
                sb.AppendLine($"  acum     : {Trunc(resolved, 55)}")
            Else
                sb.AppendLine($"{prop.Name}: {Trunc(resolved, 70)}")
            End If
        Next

        _treeStepTip.Show(sb.ToString().TrimEnd(), _treeStep, e.Location.X + 18, e.Location.Y + 10)
    End Sub

    Private Sub StepTree_MouseLeave(sender As Object, e As EventArgs)
        _lastHoveredNode = Nothing
        _treeStepTip?.Hide(_treeStep)
    End Sub

    Private Shared Function Trunc(s As String, maxLen As Integer) As String
        If s Is Nothing OrElse s.Length <= maxLen Then Return s
        Return s.Substring(0, maxLen - 3) & "..."
    End Function

End Class