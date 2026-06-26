Imports System.IO
Imports System.Net.Http
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports Microsoft.Playwright
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    ' ============================================================
    '  ORCHESTRATOR
    ' ============================================================
    Private Async Function ExecuteActionAsync(action As IWorkflowAction) As Task
        Select Case True
            Case TypeOf action Is ClickAction
                Dim a = DirectCast(action, ClickAction)
                Await ExecuteClickAsync(a)
                If Not action.GetSkipIdleWait() Then Await WaitForWicketIdleAsync()

            Case TypeOf action Is GoBackAction
                Dim a = DirectCast(action, GoBackAction)
                Await ExecuteGoBackAsync(a)
                If Not action.GetSkipIdleWait() Then Await WaitForWicketIdleAsync()

            Case TypeOf action Is FillAction
                Await ExecuteFillAsync(DirectCast(action, FillAction))
            Case TypeOf action Is SelectAction
                Await ExecuteSelectAsync(DirectCast(action, SelectAction))
            Case TypeOf action Is ReadAction
                Await ExecuteReadAsync(DirectCast(action, ReadAction))
            Case TypeOf action Is WaitForAction
                Await ExecuteWaitForAsync(DirectCast(action, WaitForAction))
            Case TypeOf action Is WaitAction
                Await ExecuteWaitAsync(DirectCast(action, WaitAction))
            Case TypeOf action Is UploadAction
                Await ExecuteUploadAsync(DirectCast(action, UploadAction))
            Case TypeOf action Is IfExistsAction
                Await ExecuteIfExistsAsync(DirectCast(action, IfExistsAction))
            Case TypeOf action Is IfUniqueAction
                Await ExecuteIfUniqueAsync(DirectCast(action, IfUniqueAction))
            Case TypeOf action Is IfVarAction
                Await ExecuteIfVarAsync(DirectCast(action, IfVarAction))
            Case TypeOf action Is LogAction
                ExecuteLog(DirectCast(action, LogAction))
            Case TypeOf action Is ScreenshotAction
                Await ExecuteScreenshotAsync(DirectCast(action, ScreenshotAction))
            Case TypeOf action Is AuthClickAction
                Await ExecuteAuthClickAsync(DirectCast(action, AuthClickAction))
            Case TypeOf action Is MinimizeAction
                Await ExecuteMinimizeAsync(DirectCast(action, MinimizeAction))
            Case TypeOf action Is ReloadAction
                Await ExecuteReloadAsync(DirectCast(action, ReloadAction))
            Case TypeOf action Is DebugAction
                Await ExecuteDebugAsync(DirectCast(action, DebugAction))
            Case TypeOf action Is StopAction
                Await ExecuteStopAction(DirectCast(action, StopAction))
            Case TypeOf action Is ExitAction
                Await ExecuteExitAction(DirectCast(action, ExitAction))
            Case TypeOf action Is GetAttributeAction
                Await ExecuteGetAttribute(DirectCast(action, GetAttributeAction))
            Case TypeOf action Is ScrapeTableAction
                Await ExecuteScrapeTableAsync(DirectCast(action, ScrapeTableAction))
            Case TypeOf action Is ScrollToViewAction
                Await ExecuteScrollToViewAsync(DirectCast(action, ScrollToViewAction))
            Case TypeOf action Is DownloadAction
                Await ExecuteDownloadAsync(DirectCast(action, DownloadAction))
            Case TypeOf action Is FindInTableAction
                Await ExecuteFindInTableAsync(DirectCast(action, FindInTableAction))
            Case TypeOf action Is RepeatAction
                Await ExecuteRepeatAsync(DirectCast(action, RepeatAction))
            Case TypeOf action Is ForEachAction
                Await ExecuteForEachAsync(DirectCast(action, ForEachAction))
            Case TypeOf action Is WhileAction
                Await ExecuteWhileAsync(DirectCast(action, WhileAction))
            Case TypeOf action Is ForEachVarAction
                Await ExecuteForEachVarAsync(DirectCast(action, ForEachVarAction))
            Case TypeOf action Is SetInternalVarAction
                ExecuteSetInternalVar(DirectCast(action, SetInternalVarAction))
            Case TypeOf action Is WaitForJSAction
                Await ExecuteWaitForJSAsync(DirectCast(action, WaitForJSAction))
            Case TypeOf action Is SwitchTabAction
                Await ExecuteSwitchTabAsync(DirectCast(action, SwitchTabAction))
            Case TypeOf action Is ExtractXmlFromPdfAction
                Await ExecuteExtractXmlFromPdfAsync(DirectCast(action, ExtractXmlFromPdfAction))
            Case Else
                _logger.LogWarning($"Acțiune necunoscută: {action.ActionType}")
        End Select
    End Function

    Private Async Function ExecuteStopAction(action As StopAction) As Task
        LogStep(action, $"STOP: {action.Message}")
        _logger.LogWarning($"[STOP] {action.Message}")
        Await Task.CompletedTask
    End Function

    ' Funcția care OPREȘTE TOT imediat
    Private Function ExecuteExitAction(action As ExitAction) As Task
        LogStep(action, $"EXIT: {action.Message}")
        ' Aruncăm semnalul de ieșire. Asta "sare" peste orice alt cod care urma.
        Throw New WorkflowExitException(action.Message)
        Return Task.CompletedTask
    End Function

    ' ============================================================
    '  HELPERS
    ' ============================================================

    ''' <summary>
    ''' Înlocuiește placeholder-ele de tip [[NumeVariabila]] cu valoarea din dicționarul _variables.
    ''' </summary>
    Private Function ReplaceInternalVariables(input As String) As String
        If String.IsNullOrEmpty(input) Then Return String.Empty

        Dim result As String = input

        ' --- 1. REZOLVARE VARIABILE DE BUCLE (DIN STIVĂ) ---
        ' (Aici nu se schimbă nimic, rămâne la fel)
        If _executionStack.Count > 0 Then
            For Each ctx In _executionStack
                If Not String.IsNullOrEmpty(ctx.IndexVariableName) Then
                    Dim placeholder As String = "[[" & ctx.IndexVariableName & "]]"
                    If result.Contains(placeholder) Then
                        result = result.Replace(placeholder, ctx.RuntimeIndex.ToString())
                    End If
                End If
            Next
        End If

        ' --- 2. REZOLVARE VARIABILE GLOBALE (MODIFICAT) ---
        For Each kvp In _variables
            Dim placeholder As String = "[[" & kvp.Key & "]]"

            ' Optimizare: Verificăm întâi dacă string-ul conține variabila
            If result.Contains(placeholder) Then
                Dim valuesList = kvp.Value

                ' [CRITIC] Verificăm dacă lista are valori
                If valuesList IsNot Nothing AndAlso valuesList.Count > 0 Then
                    ' Luăm ULTIMA valoare adăugată (cea curentă din context)
                    Dim currentValue As String = valuesList.Last()
                    result = result.Replace(placeholder, currentValue)
                Else
                    ' Dacă lista e goală, înlocuim cu string gol ca să nu rămână [[Var]]
                    result = result.Replace(placeholder, String.Empty)
                End If
            End If
        Next

        Return result
    End Function

    ' ============================================================
    '  HELPERS EXIT CONDITIONS
    ' ============================================================
    Private Shared Function EvaluateCellEquals(jObj As JObject, raw As String) As Boolean
        If String.IsNullOrEmpty(raw) Then Return False
        Dim parts = raw.Split(":"c)
        If parts.Length < 2 Then Return False

        Dim colName = parts(0).Trim()
        Dim prop = jObj.Properties().FirstOrDefault(
        Function(p) String.Equals(p.Name, colName, StringComparison.OrdinalIgnoreCase))
        Dim actual As String = If(prop?.Value?.ToString(), "")

        If parts.Length >= 3 AndAlso parts(1).Trim() = "~" Then
            Return EvaluateJSComparison(actual, "regex", String.Join(":", parts.Skip(2)))
        End If

        Return EvaluateJSComparison(actual, "eq", String.Join(":", parts.Skip(1)))
    End Function

    Private Shared Function EvaluateCellDate(jObj As JObject, raw As String) As Boolean
        If String.IsNullOrEmpty(raw) Then Return False
        Dim parts = raw.Split(":"c)
        If parts.Length < 3 Then Return False

        Dim colName = parts(0).Trim()
        Dim op = parts(1).Trim().ToLower()
        Dim valStr = String.Join(":", parts.Skip(2)).Trim()

        Dim prop = jObj.Properties().FirstOrDefault(
        Function(p) String.Equals(p.Name, colName, StringComparison.OrdinalIgnoreCase))
        Dim actualStr As String = If(prop?.Value?.ToString(), "").Trim()

        Dim actualDate, compareDate As DateTime
        If Not TryParseDate(actualStr, actualDate) OrElse Not TryParseDate(valStr, compareDate) Then Return False

        Dim a = actualDate.ToOADate()
        Dim b = compareDate.ToOADate()

        Select Case op
            Case "eq" : Return a = b
            Case "neq" : Return a <> b
            Case "lt" : Return a < b
            Case "lte" : Return a <= b
            Case "gt" : Return a > b
            Case "gte" : Return a >= b
            Case Else : Return a = b
        End Select
    End Function

    Private Shared Function TryParseDate(s As String, ByRef result As DateTime) As Boolean
        If String.IsNullOrWhiteSpace(s) Then Return False
        Return DateTime.TryParseExact(s.Trim(),
                              {"dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy",
                               "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy"},
                              Globalization.CultureInfo.InvariantCulture,
                              Globalization.DateTimeStyles.None,
                              result)
    End Function

    ''' <summary>
    ''' Din "CodAng_IdSecA-135" cu keyValue="IdSecA-135" returnează "CodAng".
    ''' Găsește ultima apariție a cheii, elimină ea și separatorul precedent.
    ''' </summary>
    Private Shared Function StripKeySuffix(varName As String, keyValue As String) As String
        Dim idx = varName.LastIndexOf(keyValue, StringComparison.Ordinal)
        If idx < 0 Then Return varName

        ' Ce rămâne înainte de cheie (ex: "CodAng_")
        Dim before = varName.Substring(0, idx)

        ' Eliminăm separatorul trailing: _, -, .
        Return before.TrimEnd("_"c, "-"c, "."c).Trim()
    End Function

    ' ============================================================
    ' HELPER FOREACHVAR 1: Construiește un rând colectat pentru o iterație
    ' ============================================================
    Private Function BuildCollectedRow(item As JObject,
                                   collectFields As String(),
                                   collectKeyLegacy As String,
                                   itemPrefix As String) As JObject
        Dim rowObj As New JObject()
        Dim keyValue As String

        If collectFields.Length > 0 Then
            For Each fieldName In collectFields
                Dim fromItem = item(fieldName)?.ToString()
                If Not String.IsNullOrEmpty(fromItem) Then
                    rowObj(fieldName) = fromItem
                Else
                    ' Câmpul e scris dinamic în iterație (Read/ScrapeTable cu saveTo=fieldName)
                    Dim fromVar = GetVariable(fieldName)
                    If Not String.IsNullOrEmpty(fromVar) Then
                        Try
                            ' Preservă structura JArray/JObject (ex: ScrapeTable → Detaliu)
                            rowObj(fieldName) = JToken.Parse(fromVar)
                        Catch
                            rowObj(fieldName) = fromVar
                        End Try
                    Else
                        rowObj(fieldName) = ""
                    End If
                End If
            Next
            keyValue = rowObj(collectFields(0))?.ToString()
        Else
            ' Backward compat: collectKey → "_key"
            keyValue = GetVariable($"{itemPrefix}.{collectKeyLegacy}")
            If Not String.IsNullOrEmpty(keyValue) Then
                rowObj("_key") = keyValue
            End If
        End If

        If String.IsNullOrEmpty(keyValue) Then Return Nothing

        ' Scanăm variabilele care conțin cheia și le adăugăm în rând
        For Each kvp In _variables
            If Not kvp.Key.Contains(keyValue) Then Continue For
            Dim fn = StripKeySuffix(kvp.Key, keyValue)
            If String.IsNullOrEmpty(fn) Then Continue For
            rowObj(fn) = kvp.Value.LastOrDefault()
        Next

        Return rowObj
    End Function

    ' ============================================================
    ' HELPER FOREACHVAR 2: Șterge variabilele individuale după colectare
    ' ============================================================
    Private Sub CleanupCollectedVariables(collectedRows As List(Of JObject), primaryKeyField As String)
        Dim keysToDelete As New List(Of String)

        For Each row In collectedRows
            Dim keyVal = row(primaryKeyField)?.ToString()
            If String.IsNullOrEmpty(keyVal) Then Continue For
            For Each varName In _variables.Keys
                If varName.Contains(keyVal) Then keysToDelete.Add(varName)
            Next
        Next

        For Each k In keysToDelete.Distinct()
            _variables.Remove(k)
        Next
    End Sub

    ' ============================================================
    ' HELPER FOREACHVAR 3: Salvează colecția finală în variabile
    ' ============================================================
    Private Sub SaveCollectedResults(collectedRows As List(Of JObject),
                                  baseName As String,
                                  primaryKeyField As String,
                                  useMap As Boolean)
        If collectedRows.Count = 0 Then Return

        Dim arrayName = $"{baseName}_results"
        Dim resultArray As New JArray(collectedRows.Cast(Of Object)().ToArray())
        SetVariable(arrayName, resultArray.ToString(Newtonsoft.Json.Formatting.None))

        If useMap Then
            Dim mapName = $"{baseName}_results_map"
            Dim resultMap As New JObject()
            For Each row In collectedRows
                Dim key = row(primaryKeyField)?.ToString()
                If Not String.IsNullOrEmpty(key) Then
                    resultMap.Add(key, row.DeepClone())
                End If
            Next
            SetVariable(mapName, resultMap.ToString(Newtonsoft.Json.Formatting.None))
            _logger.LogSuccess($"[ForEachVar] Colecție salvată → '{arrayName}' ({collectedRows.Count} rânduri) + '{mapName}'.")
        Else
            _logger.LogSuccess($"[ForEachVar] Colecție salvată → '{arrayName}' ({collectedRows.Count} rânduri).")
        End If

        CleanupCollectedVariables(collectedRows, primaryKeyField)
    End Sub

    Private Async Function EvaluateJsConditionAsync(jsCondition As String) As Task(Of Boolean)
        Dim resolved As String = ReplaceInternalVariables(jsCondition)
        Dim jsEx As Exception = Nothing
        Dim result As Boolean = False
        Try
            result = Await _page.EvaluateAsync(Of Boolean)(resolved)
        Catch ex As Exception
            jsEx = ex
        End Try
        If jsEx IsNot Nothing Then
            _logger.LogWarning($"[JsCondition] Eroare evaluare: {jsEx.Message}")
            Return False
        End If
        Return result
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
