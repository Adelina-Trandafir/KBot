Imports System.Text.RegularExpressions
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteIfVarAsync(action As IfVarAction) As Task
        Dim resolvedValue = ReplaceInternalVariables(action.Value)
        Dim isTrue As Boolean = False

        If Not String.IsNullOrEmpty(action.Compare) Then
            ' Sintaxa: "eq:2", "neq:0", "gt:1", "lt:10", "gte:5", "lte:5"
            Dim parts = action.Compare.Split(":"c)
            If parts.Length = 2 Then
                Dim op = parts(0).ToLower().Trim()
                Dim compareTo = ReplaceInternalVariables(parts(1).Trim())
                Dim a, b As Double
                Dim isNumeric = Double.TryParse(resolvedValue, a) AndAlso Double.TryParse(compareTo, b)

                Select Case op
                    Case "=", "eq" : isTrue = If(isNumeric, a = b, resolvedValue = compareTo)
                    Case "!=", "neq" : isTrue = If(isNumeric, a <> b, resolvedValue <> compareTo)
                    Case ">", "gt" : isTrue = isNumeric AndAlso a > b
                    Case "<", "lt" : isTrue = isNumeric AndAlso a < b
                    Case ">=", "gte" : isTrue = isNumeric AndAlso a >= b
                    Case "<=", "lte" : isTrue = isNumeric AndAlso a <= b
                    Case "regex", "~"
                        Try
                            ' Sintaxa: regex:PATTERN sau regex:PATTERN:FLAGS
                            ' Cautam ultimul ":" care precede flags-urile (i, m, s)
                            Dim lastColon = compareTo.LastIndexOf(":"c)
                            Dim pattern As String
                            Dim flags As String = ""

                            If lastColon > 0 Then
                                Dim possibleFlags = compareTo.Substring(lastColon + 1).ToLower()
                                ' Verificam daca e intr-adevar flags (doar i, m, s)
                                If Regex.IsMatch(possibleFlags, "^[ims]+$") Then
                                    pattern = compareTo.Substring(0, lastColon)
                                    flags = possibleFlags
                                Else
                                    pattern = compareTo
                                End If
                            Else
                                pattern = compareTo
                            End If

                            Dim options = RegexOptions.None
                            If flags.Contains("i"c) Then options = options Or RegexOptions.IgnoreCase
                            If flags.Contains("m"c) Then options = options Or RegexOptions.Multiline
                            If flags.Contains("s"c) Then options = options Or RegexOptions.Singleline

                            isTrue = Regex.IsMatch(resolvedValue, pattern, options)
                            _logger.LogInfo($"[IfVar] '{resolvedValue}' regex '{pattern}'" &
                                         If(flags <> "", $" flags='{flags}'", "") & $" -> {isTrue}")
                        Catch ex As Exception
                            _logger.LogWarning($"[IfVar] Regex invalid: '{compareTo}' -> {ex.Message}")
                            isTrue = False
                        End Try
                    Case Else
                        _logger.LogWarning($"[IfVar] Operator necunoscut: '{op}'")
                End Select

                _logger.LogInfo($"[IfVar] '{resolvedValue}' {op} '{compareTo}' -> {isTrue}")
            Else
                _logger.LogWarning($"[IfVar] Sintaxa 'compare' invalida: '{action.Compare}'. Asteptat: 'op:valoare'")
            End If
        Else
            ' Comportament vechi - non-gol si ne-nerezolvat
            isTrue = Not String.IsNullOrWhiteSpace(resolvedValue) AndAlso
                 Not resolvedValue.Contains("{{")
            _logger.LogInfo($"[IfVar] Valoare: '{resolvedValue}' -> {isTrue}")
        End If

        If isTrue Then
            _logger.LogInfo($"[IfVar] Conditie indeplinita. Execut ramura principala.")
            For Each childAction In action.Children
                Await ExecuteActionAsync(childAction)
            Next
        Else
            _logger.LogInfo($"[IfVar] Conditie NEINDEPLINITA.")
            If action.ElseChildren.Count > 0 Then
                _logger.LogInfo($"[IfVar] Execut ramura Else.")
                For Each childAction In action.ElseChildren
                    Await ExecuteActionAsync(childAction)
                Next
            End If
        End If
    End Function

End Class
