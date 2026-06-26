Imports Microsoft.Playwright
Imports Newtonsoft.Json.Linq
Imports WorkflowModels

' =============================================================================
'  ClickMonitor — captureaza click-uri si tastare din browser.
'
'  JS: Resources/WicketJavaScript.js (Build Action = Embedded Resource)
'  Incarcare JS: GetEmbeddedJs() din WorkflowExecutor.JsLoader.vb
'
'  Source="CLICK" : element + parinte + bunic, proprietati per tip tag
'  Source="KEY"   : primul eveniment "input" intr-un element nou
' =============================================================================
Partial Public Class WorkflowExecutor

    ' =========================================================================
    '  Helper shared — construieste sirul "key:val  key:val" din JObject props
    ' =========================================================================
    Private Shared Function BuildPropsString(propsObj As JObject) As String
        If propsObj Is Nothing Then Return ""
        Dim sb As New System.Text.StringBuilder()
        For Each prop As JProperty In propsObj.Properties()
            If sb.Length > 0 Then sb.Append("  ")
            sb.Append(prop.Name).Append(":"c).Append(prop.Value.ToString())
        Next
        Return sb.ToString()
    End Function

    ' =========================================================================
    '  Helper shared — "← selector  props" pentru un nivel de ascendent
    ' =========================================================================
    Private Shared Function BuildAncestorSegment(ancestorObj As JObject) As String
        If ancestorObj Is Nothing Then Return ""
        Dim sel As String = If(ancestorObj.Value(Of String)("selector"), "")
        Dim props As JObject = TryCast(ancestorObj("props"), JObject)
        Dim info As String = BuildPropsString(props)
        Dim sb As New System.Text.StringBuilder("← ")
        sb.Append(sel)
        If Not String.IsNullOrEmpty(info) Then sb.Append("  ").Append(info)
        Return sb.ToString()
    End Function

    ' =========================================================================
    '  StartClickMonitoringAsync
    '  Apelat din StartWicketMonitoringAsync — acelasi ciclu de viata.
    ' =========================================================================
    Friend Async Function StartClickMonitoringAsync() As Task
        If _page Is Nothing Then Return

        Dim initEx As Exception = Nothing
        Try
            ' ── 1. Click callback ─────────────────────────────────────────────
            Await _page.ExposeFunctionAsync(Of String)(
                "_clickMonitorCallback",
                Sub(jsonArgs As String)
                    Dim parseEx As Exception = Nothing
                    Dim entry As WicketMonitorEntry = Nothing
                    Try
                        Dim obj As JObject = JObject.Parse(jsonArgs)

                        Dim selector As String = If(obj.Value(Of String)("selector"), "")
                        Dim pageUrl As String = If(obj.Value(Of String)("url"), "")
                        Dim propsObj As JObject = TryCast(obj("props"), JObject)
                        Dim parentObj As JObject = TryCast(obj("parent"), JObject)
                        Dim grandObj As JObject = TryCast(obj("grandparent"), JObject)

                        Dim state As New System.Text.StringBuilder(BuildPropsString(propsObj))

                        Dim parentSeg As String = BuildAncestorSegment(parentObj)
                        If Not String.IsNullOrEmpty(parentSeg) Then
                            If state.Length > 0 Then state.Append("  ")
                            state.Append(parentSeg)
                        End If

                        Dim grandSeg As String = BuildAncestorSegment(grandObj)
                        If Not String.IsNullOrEmpty(grandSeg) Then
                            If state.Length > 0 Then state.Append("  ")
                            state.Append(grandSeg)
                        End If

                        entry = New WicketMonitorEntry With {
                            .Timestamp = DateTime.Now,
                            .Source = "CLICK",
                            .Element = selector,
                            .State = state.ToString(),
                            .PageUrl = pageUrl
                        }
                    Catch ex As Exception
                        parseEx = ex
                    End Try

                    If parseEx IsNot Nothing OrElse entry Is Nothing Then Return
                    RaiseEvent OnWicketStateChange(entry)
                End Sub)

            ' ── 2. Key callback ───────────────────────────────────────────────
            Await _page.ExposeFunctionAsync(Of String)(
                "_keyMonitorCallback",
                Sub(jsonArgs As String)
                    Dim parseEx As Exception = Nothing
                    Dim entry As WicketMonitorEntry = Nothing
                    Try
                        Dim obj As JObject = JObject.Parse(jsonArgs)

                        Dim selector As String = If(obj.Value(Of String)("selector"), "")
                        Dim pageUrl As String = If(obj.Value(Of String)("url"), "")
                        Dim propsObj As JObject = TryCast(obj("props"), JObject)

                        entry = New WicketMonitorEntry With {
                            .Timestamp = DateTime.Now,
                            .Source = "KEY",
                            .Element = selector,
                            .State = BuildPropsString(propsObj),
                            .PageUrl = pageUrl
                        }
                    Catch ex As Exception
                        parseEx = ex
                    End Try

                    If parseEx IsNot Nothing OrElse entry Is Nothing Then Return
                    RaiseEvent OnWicketStateChange(entry)
                End Sub)

            ' ── 3. Init script — JS din cache ─────────────────────────────────
            Await _page.AddInitScriptAsync(GetEmbeddedJs("WicketJavaScript.js"))

        Catch ex As Exception
            initEx = ex
        End Try

        If initEx IsNot Nothing Then
            _logger.LogError($"[ClickMonitor] Eroare la activare: {initEx.Message}")
        End If
    End Function

End Class