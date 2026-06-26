Imports Microsoft.Playwright
Imports Newtonsoft.Json.Linq
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteDebugAsync(action As DebugAction) As Task
        Dim selector As String = ReplaceInternalVariables(action.Selector)
        LogStep(action, $"Debug selector: {selector}")
        _logger.LogInfo($"[DEBUG] Analizez selectorul: '{selector}'")

        Dim locator = _page.Locator(selector)
        Dim count As Integer = Await locator.CountAsync()

        If count = 0 Then
            _logger.LogError($"[DEBUG] 0 elemente găsite.")
            Return
        End If

        _logger.LogWarning($"[DEBUG] Găsite {count} elemente.")

        For i As Integer = 0 To count - 1
            Dim el = locator.Nth(i)
            _logger.LogInfo("══════════════════════════════════════════════════")
            _logger.LogInfo($"ELEMENT #{i + 1}")
            Await DebugLogElementInfoAsync(el, 0)

            If action.IncludeChildren Then
                _logger.LogInfo($"  ┌─ COPII ai ELEMENTULUI #{i + 1}:")

                Dim childrenJson As String = String.Empty
                Try
                    childrenJson = Await el.EvaluateAsync(Of String)(GetEmbeddedJs("DebugChildren.js"))
                Catch ex As Exception
                    _logger.LogError($"[DEBUG] Eroare la citire copii: {ex.Message}")
                End Try

                If Not String.IsNullOrEmpty(childrenJson) Then
                    Try
                        Dim childArray = JArray.Parse(childrenJson)

                        If childArray.Count = 0 Then
                            _logger.LogInfo("  └─ (fără copii)")
                        Else
                            Dim childIdx As Integer = 0
                            For Each childToken In childArray
                                childIdx += 1
                                Dim obj = DirectCast(childToken, JObject)

                                Dim depthVal As Integer = 0
                                Integer.TryParse(obj("depth")?.ToString(), depthVal)
                                Dim indent As String = New String(" "c, depthVal * 2 + 2)

                                _logger.LogInfo($"{indent}──────────────────────────────────────────────")
                                _logger.LogInfo($"{indent}COPIL #{childIdx}  (Adâncime: {depthVal})")

                                For Each prop In obj.Properties()
                                    If prop.Name = "depth" Then Continue For
                                    If prop.Name = "htmlPreview" Then
                                        _logger.LogInfo($"{indent}   > HTML: {System.Web.HttpUtility.HtmlDecode(prop.Value.ToString())}")
                                    Else
                                        _logger.LogInfo($"{indent}   > {prop.Name}: {prop.Value}")
                                    End If
                                Next
                            Next
                            _logger.LogInfo($"  └─ Total copii: {childArray.Count}")
                        End If
                    Catch ex As Exception
                        _logger.LogError($"[DEBUG] Eroare parsare copii JSON: {ex.Message}")
                        _logger.LogInfo($"   RAW: {childrenJson}")
                    End Try
                End If
            End If
        Next
        _logger.LogInfo("══════════════════════════════════════════════════")

        If action.HaltWhenDone Then
            Throw New Exception("[DEBUG STOP] Analiză completă. Verifică log-ul de mai sus.")
        End If
    End Function

    Private Async Function DebugLogElementInfoAsync(el As ILocator, depth As Integer) As Task
        Dim indent As String = New String(" "c, depth * 2 + 2)

        Dim isVisible As Boolean = False
        Try : isVisible = Await el.IsVisibleAsync() : Catch : End Try

        Dim boxInfo As String = "N/A"
        Try
            Dim box = Await el.BoundingBoxAsync()
            If box IsNot Nothing Then
                boxInfo = $"X:{CInt(box.X)}, Y:{CInt(box.Y)}, W:{CInt(box.Width)}, H:{CInt(box.Height)}"
            End If
        Catch : End Try

        Dim rawJson As String
        Try
            rawJson = Await el.EvaluateAsync(Of String)(GetEmbeddedJs("DebugElement.js"))
        Catch ex As Exception
            rawJson = $"EROARE .NET: {ex.Message}"
        End Try

        Dim statusIcon As String = If(isVisible, "✅ VIZIBIL", "❌ ASCUNS")
        _logger.LogInfo($"{indent}Stare: {statusIcon}  |  Locație: {boxInfo}")

        If rawJson.StartsWith("EROARE") Then
            _logger.LogError($"{indent}{rawJson}")
            Return
        End If

        Try
            Dim obj = JObject.Parse(rawJson)
            For Each prop In obj.Properties()
                If prop.Name = "htmlPreview" Then
                    _logger.LogInfo($"{indent}   > HTML: {System.Web.HttpUtility.HtmlDecode(prop.Value.ToString())}")
                Else
                    _logger.LogInfo($"{indent}   > {prop.Name}: {prop.Value}")
                End If
            Next
        Catch ex As Exception
            _logger.LogInfo($"{indent}   JSON RAW: {rawJson}")
        End Try
    End Function

End Class