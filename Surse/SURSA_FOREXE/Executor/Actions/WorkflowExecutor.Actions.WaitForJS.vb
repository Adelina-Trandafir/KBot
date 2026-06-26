Imports System.Text.RegularExpressions
Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteWaitForJSAsync(action As WaitForJSAction) As Task
        ' 1. Rezolvăm variabilele din expresie
        Dim resolvedExpression = ReplaceInternalVariables(action.Expression)

        Dim resolvedExpected = If(action.ExpectedValue IsNot Nothing,
                              ReplaceInternalVariables(action.ExpectedValue),
                              Nothing)

        Dim modeNorm = action.WaitMode.ToLower().Trim()

        LogStep(action, $"[WaitForJS] Evaluez: {resolvedExpression.Substring(0, Math.Min(resolvedExpression.Length, 60))}...")

        Dim timeoutMs As Integer = action.Timeout * 1000
        Dim waited As Integer = 0
        Dim lastResult As String = Nothing
        Dim conditionMet As Boolean = False

        Do
            ' 2. Evaluăm expresia în browser
            Try
                Dim jsResult = Await _page.EvaluateAsync(Of Object)($"() => {{ return ({resolvedExpression}); }}")
                lastResult = If(jsResult IsNot Nothing, jsResult.ToString(), "null")
            Catch ex As Exception
                lastResult = $"[JS Error] {ex.Message}"
                _logger.LogDebug($"[WaitForJS] Eroare evaluare JS: {ex.Message}")
            End Try

            ' 3. Evaluăm condiția
            If resolvedExpected IsNot Nothing Then
                ' Mod comparație explicită
                conditionMet = EvaluateJSComparison(lastResult, action.Compare, resolvedExpected)
            Else
                ' Mod implicit prin waitMode
                Select Case modeNorm
                    Case "nonNull"
                        conditionMet = lastResult IsNot Nothing AndAlso
                                   lastResult <> "null" AndAlso
                                   lastResult <> "undefined"
                    Case "noWait"
                        conditionMet = True  ' iese imediat după prima evaluare
                    Case Else ' "truthy"
                        conditionMet = Not String.IsNullOrEmpty(lastResult) AndAlso
                                       Not String.Equals(lastResult, "false", StringComparison.OrdinalIgnoreCase) AndAlso
                                       Not String.Equals(lastResult, "null", StringComparison.OrdinalIgnoreCase) AndAlso
                                       Not String.Equals(lastResult, "undefined", StringComparison.OrdinalIgnoreCase) AndAlso
                                       lastResult <> "0"
                End Select
            End If

            ' 4. Salvăm rezultatul INDIFERENT de condiție (util la eșec)
            If Not String.IsNullOrEmpty(action.SaveTo) Then
                Dim resolvedSaveTo = ReplaceInternalVariables(action.SaveTo)
                SetVariable(resolvedSaveTo, lastResult)
            End If

            If conditionMet Then
                _logger.LogSuccess($"[WaitForJS] Condiție îndeplinită. Rezultat: '{lastResult}'")
                Return
            End If

            ' 5. noWait nu polează — iese imediat chiar dacă nu e truthy
            If modeNorm = "noWait" Then
                _logger.LogInfo($"[WaitForJS] noWait — rezultat salvat: '{lastResult}'")
                Return
            End If

            ' 6. Timeout check
            If waited >= timeoutMs Then
                ' Salvăm ultimul rezultat înainte de excepție
                If Not String.IsNullOrEmpty(action.SaveTo) Then
                    Dim resolvedSaveTo = ReplaceInternalVariables(action.SaveTo)
                    SetVariable(resolvedSaveTo, lastResult)
                End If
                Throw New TimeoutException(
                $"[WaitForJS] Timeout după {action.Timeout}s. Ultimul rezultat: '{lastResult}'")
            End If

            ' 7. Polling
            Await Task.Delay(action.PollingMs, _cancellationToken)
            waited += action.PollingMs
        Loop
    End Function

    Private Shared Function EvaluateJSComparison(actual As String, compare As String, expected As String) As Boolean
        Dim op = compare.ToLower().Trim()

        Select Case op
            Case "eq", "="
                Return String.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
            Case "neq", "!="
                Return Not String.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
            Case "contains"
                Return actual IsNot Nothing AndAlso
                   actual.Contains(expected, StringComparison.OrdinalIgnoreCase)
            Case "regex", "~"
                Try
                    ' Sintaxa: regex cu flags opțional la sfârșit după ":"
                    ' Ex: compare="regex" expectedValue="^\d+$:i"
                    Dim lastColon = expected.LastIndexOf(":"c)
                    Dim pattern As String
                    Dim flags As String = ""

                    If lastColon > 0 Then
                        Dim possibleFlags = expected.Substring(lastColon + 1).ToLower()
                        If Regex.IsMatch(possibleFlags, "^[ims]+$") Then
                            pattern = expected.Substring(0, lastColon)
                            flags = possibleFlags
                        Else
                            pattern = expected
                        End If
                    Else
                        pattern = expected
                    End If

                    Dim options = RegexOptions.None
                    If flags.Contains("i"c) Then options = options Or RegexOptions.IgnoreCase
                    If flags.Contains("m"c) Then options = options Or RegexOptions.Multiline
                    If flags.Contains("s"c) Then options = options Or RegexOptions.Singleline

                    Return Regex.IsMatch(If(actual, ""), pattern, options)
                Catch ex As Exception
                    Return False
                End Try
            Case Else
                Return String.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
        End Select
    End Function

End Class
