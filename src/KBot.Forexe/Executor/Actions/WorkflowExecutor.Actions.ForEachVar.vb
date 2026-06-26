Imports Newtonsoft.Json.Linq
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteForEachVarAsync(action As ForEachVarAction) As Task
        ' 1. Rezolvăm sursele
        Dim resolvedSource = ReplaceInternalVariables(action.Source)
        Dim jsonRaw = GetVariable(resolvedSource)

        Dim resolvedCollectFields As String() = {}
        If Not String.IsNullOrEmpty(action.CollectFields) Then
            resolvedCollectFields = action.CollectFields.Split(","c) _
                                 .Select(Function(f) f.Trim()) _
                                 .Where(Function(f) f.Length > 0) _
                                 .ToArray()
        End If

        Dim resolvedCollectKey = ReplaceInternalVariables(action.CollectKey)
        Dim doCollect = resolvedCollectFields.Length > 0 OrElse Not String.IsNullOrEmpty(resolvedCollectKey)
        Dim primaryKeyField = If(resolvedCollectFields.Length > 0, resolvedCollectFields(0), "_key")

        ' 2. Validare sursă
        If String.IsNullOrEmpty(jsonRaw) Then
            _logger.LogWarning($"[ForEachVar] Variabila '{resolvedSource}' e goală. Sar peste.")
            Return
        End If

        ' 3. Parsăm JSON array-ul
        Dim array As JArray
        Try
            array = JArray.Parse(jsonRaw)
        Catch ex As Exception
            _logger.LogError($"[ForEachVar] '{resolvedSource}' nu e un JSON array valid: {ex.Message}")
            Throw
        End Try

        _logger.LogInfo($"[ForEachVar] {array.Count} elemente în '{resolvedSource}'.")

        ' 4. Context loop
        Dim loopCtx As New LoopContext With {
            .ActionType = "ForEachVar",
            .RuntimeIndex = 0,
            .IndexVariableName = action.IndexVariable
        }
        _executionStack.Push(loopCtx)

        Dim collectedRows As New List(Of JObject)

        Try
            For i As Integer = 0 To array.Count - 1
                loopCtx.RuntimeIndex = i + 1
                _logger.LogInfo($"--- [ForEachVar] Iterația {i + 1} din {array.Count} ---")

                Dim item = TryCast(array(i), JObject)
                If item Is Nothing Then
                    _logger.LogWarning($"[ForEachVar] Elementul {i} nu e obiect JSON. Sar.")
                    Continue For
                End If

                _logger.LogDebug($"[ForEachVar] Câmpuri: {String.Join(", ", item.Properties().Select(Function(p) p.Name))}")

                ' Expunem câmpurile ca PREFIX.NumeCamp
                For Each prop In item.Properties()
                    SetVariable($"{action.ItemPrefix}.{prop.Name}", If(prop.Value?.ToString(), ""))
                Next
                SetVariable($"{action.ItemPrefix}.IsLast", If(i = array.Count - 1, "true", "false"))

                ' Executăm copiii
                If action.Children.Count > 0 Then
                    Await ExecuteActionsAsync(action.Children, True)
                End If

                ' Colectare post-iterație
                If doCollect Then
                    Dim rowObj = BuildCollectedRow(item, resolvedCollectFields, resolvedCollectKey, action.ItemPrefix)
                    If rowObj IsNot Nothing Then
                        collectedRows.Add(rowObj)
                        _logger.LogDebug($"[ForEachVar] Rând colectat la iterația {i + 1} ({rowObj.Count} câmpuri).")
                    Else
                        _logger.LogWarning($"[ForEachVar] Câmpul cheie e gol la iterația {i + 1}. Rândul e sărit.")
                    End If
                End If
            Next

        Finally
            _executionStack.Pop()
        End Try

        ' Salvare colecție finală
        If doCollect Then
            SaveCollectedResults(collectedRows, resolvedSource, primaryKeyField, action.UseMap)
        End If

        ' Șterge variabilele dinamice scrise cu saveTo=fieldName în iterație
        ' (cele din item au fost deja curățate de CleanupCollectedVariables)
        For Each fieldName In resolvedCollectFields
            If _variables.ContainsKey(fieldName) Then
                _variables.Remove(fieldName)
            End If
        Next

        _logger.LogSuccess($"[ForEachVar] Finalizat.")
    End Function

End Class
