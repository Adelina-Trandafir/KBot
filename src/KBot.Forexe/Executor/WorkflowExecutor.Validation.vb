Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    ' =========================================================================
    ' VALIDARE VARIABILE JSON
    ' Apelată la începutul ExecuteAsync, înainte de execuția acțiunilor.
    '
    ' Scanează toate ForEachVarAction din workflow și verifică:
    '   1. Câmpuri referite în WFL ca [[PREFIX.FIELD]] dar lipsesc din JSON → EROARE
    '   2. Câmpuri prezente în JSON dar nereferite în WFL → WARNING (log only)
    '   3. Câmpuri din JSON care colidează cu variabile rezervate de sistem → EROARE
    ' =========================================================================

    ' Câmpuri rezervate, injectate de executor la runtime — nu pot apărea în date
    Private Shared ReadOnly SystemFields As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "IsLast"
    }

    Private Sub ValidateJsonVariables(workflow As Workflow)
        Dim targets As New List(Of ForEachVarAction)
        CollectForEachVarActions(workflow.Actions, targets)

        If targets.Count = 0 Then Return

        Dim hasErrors As Boolean = False

        For Each action In targets
            Dim sourceName = ReplaceInternalVariables(action.Source)
            Dim jsonRaw = GetVariable(sourceName)

            If String.IsNullOrEmpty(jsonRaw) Then Continue For

            Dim array As JArray
            Try
                array = JArray.Parse(jsonRaw)
            Catch ex As Exception
                Continue For
            End Try

            If array.Count = 0 Then Continue For

            ' Extragem câmpurile referite în WFL pentru prefixul curent
            Dim referencedFields As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            CollectReferencedFields(action.Children, action.ItemPrefix, referencedFields)

            If referencedFields.Count = 0 Then Continue For

            ' Excludem câmpurile de sistem din verificarea de lipsă — sunt injectate de executor
            referencedFields.ExceptWith(SystemFields)

            ' Verificăm fiecare obiect din array
            For i As Integer = 0 To array.Count - 1
                Dim item = TryCast(array(i), JObject)
                If item Is Nothing Then Continue For

                Dim jsonKeys As New HashSet(Of String)(
                    item.Properties().Select(Function(p) p.Name),
                    StringComparer.OrdinalIgnoreCase)

                ' --- 1. Câmpuri referite în WFL dar lipsesc din JSON → EROARE ---
                For Each field In referencedFields
                    If Not jsonKeys.Contains(field) Then
                        _logger.LogError($"[Validare JSON] '{sourceName}' obiect[{i}]: " &
                                         $"câmpul '{field}' este referit ca [[{action.ItemPrefix}.{field}]] " &
                                         $"în WFL dar lipsește din JSON!")
                        hasErrors = True
                    End If
                Next

                ' --- 2. Câmpuri în JSON dar nereferite în WFL → WARNING ---
                For Each key In jsonKeys
                    If Not referencedFields.Contains(key) AndAlso Not SystemFields.Contains(key) Then
                        _logger.LogWarning($"[Validare JSON] '{sourceName}' obiect[{i}]: " &
                                        $"câmpul '{key}' există în JSON dar nu este folosit în WFL.")
                    End If
                Next

                ' --- 3. Câmpuri din JSON care colidează cu variabile de sistem → EROARE ---
                For Each key In jsonKeys
                    If SystemFields.Contains(key) Then
                        _logger.LogError($"[Validare JSON] '{sourceName}' obiect[{i}]: " &
                                         $"câmpul '{key}' este rezervat de sistem și nu poate fi folosit în date. " &
                                         $"Redenumește câmpul în JSON.")
                        hasErrors = True
                    End If
                Next
            Next
        Next

        If hasErrors Then
            Throw New InvalidOperationException(
                "Validare JSON eșuată: câmpuri lipsă sau rezervate în datele primite. Verifică log-ul pentru detalii.")
        End If
    End Sub

    ' -------------------------------------------------------------------------
    ' Colectează recursiv toate ForEachVarAction din arborele de acțiuni.
    ' -------------------------------------------------------------------------
    Private Sub CollectForEachVarActions(actions As List(Of IWorkflowAction),
                                         result As List(Of ForEachVarAction))
        For Each action In actions
            If TypeOf action Is ForEachVarAction Then
                Dim fev = DirectCast(action, ForEachVarAction)
                result.Add(fev)
                CollectForEachVarActions(fev.Children, result)
            Else
                Dim childrenProp = action.GetType().GetProperty("Children")
                If childrenProp IsNot Nothing Then
                    Dim children = TryCast(childrenProp.GetValue(action), List(Of IWorkflowAction))
                    If children IsNot Nothing AndAlso children.Count > 0 Then
                        CollectForEachVarActions(children, result)
                    End If
                End If

                Dim elseProp = action.GetType().GetProperty("ElseChildren")
                If elseProp IsNot Nothing Then
                    Dim elseChildren = TryCast(elseProp.GetValue(action), List(Of IWorkflowAction))
                    If elseChildren IsNot Nothing AndAlso elseChildren.Count > 0 Then
                        CollectForEachVarActions(elseChildren, result)
                    End If
                End If
            End If
        Next
    End Sub

    ' -------------------------------------------------------------------------
    ' Extrage recursiv toate câmpurile [[PREFIX.FIELD]] referite în acțiunile copil.
    ' -------------------------------------------------------------------------
    Private Sub CollectReferencedFields(actions As List(Of IWorkflowAction),
                                        prefix As String,
                                        result As HashSet(Of String))
        Dim pattern As New Regex($"\[\[{Regex.Escape(prefix)}\.([A-Za-z0-9_]+)\]\]",
                                 RegexOptions.IgnoreCase)

        For Each action In actions
            For Each prop In action.GetType().GetProperties()
                If prop.PropertyType IsNot GetType(String) Then Continue For
                Dim val = TryCast(prop.GetValue(action), String)
                If String.IsNullOrEmpty(val) Then Continue For

                For Each m As Match In pattern.Matches(val)
                    result.Add(m.Groups(1).Value)
                Next
            Next

            Dim childrenProp = action.GetType().GetProperty("Children")
            If childrenProp IsNot Nothing Then
                Dim children = TryCast(childrenProp.GetValue(action), List(Of IWorkflowAction))
                If children IsNot Nothing AndAlso children.Count > 0 Then
                    CollectReferencedFields(children, prefix, result)
                End If
            End If

            Dim elseProp = action.GetType().GetProperty("ElseChildren")
            If elseProp IsNot Nothing Then
                Dim elseChildren = TryCast(elseProp.GetValue(action), List(Of IWorkflowAction))
                If elseChildren IsNot Nothing AndAlso elseChildren.Count > 0 Then
                    CollectReferencedFields(elseChildren, prefix, result)
                End If
            End If
        Next
    End Sub

End Class