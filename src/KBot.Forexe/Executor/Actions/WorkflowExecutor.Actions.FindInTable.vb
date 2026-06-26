Imports Microsoft.Playwright
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteFindInTableAsync(action As FindInTableAction) As Task
        Dim parsedSelector = ReplaceInternalVariables(action.Selector)
        Dim parsedFieldName = ReplaceInternalVariables(action.FieldName)
        Dim resolvedValue = ReplaceInternalVariables(action.Value)

        LogStep(action, $"[FindInTable] Caut în '{parsedSelector}' (First) unde '{parsedFieldName}' == '{resolvedValue}'")

        Dim tableLocator = _page.Locator(parsedSelector).First

        Await tableLocator.WaitForAsync(New LocatorWaitForOptions With {
            .State = WaitForSelectorState.Visible,
            .Timeout = action.Timeout * 1000
        })

#Disable Warning IDE0037 ' Use inferred member name
        Dim rawResult As Object = Await tableLocator.EvaluateAsync(Of Object)(
            GetEmbeddedJs("FindInTable.js"),
            New With {
                .field = action.FieldName,
                .val = resolvedValue,
                .fieldTransform = action.FieldTransform
            })
#Enable Warning IDE0037 ' Use inferred member name

        Dim jsonStr As String = JsonConvert.SerializeObject(rawResult)
        Dim result As JObject = JObject.Parse(jsonStr)

        If result("error") IsNot Nothing Then
            Throw New Exception($"[FindInTable Error] {result("error")}")
        End If

        If result("debug_valueToFind") IsNot Nothing Then
            _logger.LogInfo($"[FindInTable DEBUG] ValueToFind='{result("debug_valueToFind")}'")
            _logger.LogInfo($"[FindInTable DEBUG] FirstCellText='{result("debug_firstCellText")}'")
            _logger.LogInfo($"[FindInTable DEBUG] FirstCellText_AfterTransform='{result("debug_firstCellText_transformed")}'")
        End If

        Dim found As Boolean = result("found").ToObject(Of Boolean)()

        If found Then
            Dim rowIndex As Integer = result("rowIndex").ToObject(Of Integer)()
            _logger.LogSuccess($"[FindInTable] Rând găsit la indexul {rowIndex}.")

            Dim lastSaveTo = ReplaceInternalVariables(action.SaveRowTo)
            If Not String.IsNullOrEmpty(lastSaveTo) Then
                SetVariable(lastSaveTo, result("data").ToString(Formatting.Indented))
            End If

            Dim rowLocator = tableLocator.Locator("tbody tr").Nth(rowIndex)

            If Not String.IsNullOrEmpty(action.ClickSelector) Then
                Dim childElement = rowLocator.Locator(action.ClickSelector).First
                Await childElement.WaitForAsync(New LocatorWaitForOptions With {
                    .State = WaitForSelectorState.Visible,
                    .Timeout = action.Timeout * 1000
                })
                Await childElement.ClickAsync()
                Await WaitForWicketIdleAsync()
                _logger.LogSuccess($"[FindInTable] Click executat.")
            End If

            If action.Children.Count > 0 Then
                _logger.LogInfo($"[FindInTable] Execut {action.Children.Count} acțiuni copil pe rândul {rowIndex}.")
                For Each child In action.Children
                    Await ExecuteActionInRowContextAsync(child, rowLocator)
                Next
            End If
        Else
            Throw New Exception($"[FindInTable] Nu s-a găsit rândul cu {action.FieldName} = '{resolvedValue}'")
        End If
    End Function

    Private Async Function ExecuteActionInRowContextAsync(action As IWorkflowAction,
                                                          rowLocator As ILocator) As Task
        Select Case True
            Case TypeOf action Is FillAction
                Dim a = DirectCast(action, FillAction)
                Dim resolvedSelector = ReplaceInternalVariables(a.Selector)
                Dim resolvedValue = ReplaceInternalVariables(a.Value)
                LogStep(action, $"[FindInTable:Fill] {resolvedSelector} = '{resolvedValue}'")
                Dim el = rowLocator.Locator(resolvedSelector).First
                If a.Clear Then Await el.ClearAsync()
                Await el.FillAsync(resolvedValue)

            Case TypeOf action Is ClickAction
                Dim a = DirectCast(action, ClickAction)
                Dim resolvedSelector = ReplaceInternalVariables(a.Selector)
                LogStep(action, $"[FindInTable:Click] {resolvedSelector}")
                Await rowLocator.Locator(resolvedSelector).First.ClickAsync()

            Case TypeOf action Is ReadAction
                Dim a = DirectCast(action, ReadAction)
                Dim resolvedSelector = ReplaceInternalVariables(a.Selector)
                Dim el = rowLocator.Locator(resolvedSelector).First
                Dim value = (Await el.InnerTextAsync()).Trim()
                Dim resolvedSaveTo = ReplaceInternalVariables(a.SaveTo)
                If Not String.IsNullOrEmpty(resolvedSaveTo) Then
                    SetVariable(resolvedSaveTo, value)
                    _logger.LogSuccess($"[FindInTable:Read] '{value}' -> [[{resolvedSaveTo}]]")
                End If

            Case Else
                _logger.LogWarning($"[FindInTable] Acțiunea '{action.ActionType}' nu e suportată în context de rând. Rulez global.")
                Await ExecuteActionAsync(action)
        End Select
    End Function

End Class