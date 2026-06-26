Imports System.Text.RegularExpressions
Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteSwitchTabAsync(action As SwitchTabAction) As Task
        LogStep(action, $"SwitchTab — caut tab: " &
            $"{If(action.TabIndex >= 1, $"index={action.TabIndex} ", "")}" &
            $"{If(Not String.IsNullOrEmpty(action.UrlEquals), $"equals={action.UrlEquals} ", "")}" &
            $"{If(Not String.IsNullOrEmpty(action.UrlContains), $"contains={action.UrlContains} ", "")}" &
            $"{If(Not String.IsNullOrEmpty(action.UrlPattern), $"pattern={action.UrlPattern}", "")}")

        ' ── 1. Salvez referința și URL-ul tabului curent ─────────────────────────
        Dim previousPage As IPage = _page

        If Not String.IsNullOrEmpty(action.SavePreviousTabTo) Then
            SetVariable(action.SavePreviousTabTo, _page.Url)
            _logger.LogInfo($"[SwitchTab] URL anterior salvat '{_page.Url}' → [[{action.SavePreviousTabTo}]]")
        End If

        ' ── 2. Lista taburi deschise ──────────────────────────────────────────────
        Dim pages = _context.Pages.Where(Function(p) Not p.IsClosed).ToList()

        _logger.LogInfo($"[SwitchTab] Taburi deschise ({pages.Count}) " &
                     String.Join(" | ", pages.Select(Function(p, i) $"[{i + 1}] {p.Url}")))

        Dim targetPage As IPage = Nothing

        ' ── 3. Identificare tab ───────────────────────────────────────────────────

        ' A. Index (1-based)
        If action.TabIndex >= 1 AndAlso targetPage Is Nothing Then
            Dim idx As Integer = action.TabIndex - 1
            If idx < pages.Count Then
                targetPage = pages(idx)
                _logger.LogInfo($"[SwitchTab] Găsit prin index {action.TabIndex} {targetPage.Url}")
            Else
                _logger.LogWarning($"[SwitchTab] TabIndex={action.TabIndex} depășește numărul de taburi ({pages.Count}).")
            End If
        End If

        ' B. UrlEquals
        If targetPage Is Nothing AndAlso Not String.IsNullOrEmpty(action.UrlEquals) Then
            Dim resolved = ReplaceInternalVariables(action.UrlEquals)
            targetPage = pages.FirstOrDefault(
                Function(p) p.Url.Equals(resolved, StringComparison.OrdinalIgnoreCase))
            If targetPage IsNot Nothing Then
                _logger.LogInfo($"[SwitchTab] Găsit prin urlEquals '{resolved}': {targetPage.Url}")
            End If
        End If

        ' C. UrlContains
        If targetPage Is Nothing AndAlso Not String.IsNullOrEmpty(action.UrlContains) Then
            Dim resolved = ReplaceInternalVariables(action.UrlContains)
            targetPage = pages.FirstOrDefault(
                Function(p) p.Url.IndexOf(resolved, StringComparison.OrdinalIgnoreCase) >= 0)
            If targetPage IsNot Nothing Then
                _logger.LogInfo($"[SwitchTab] Găsit prin urlContains '{resolved}': {targetPage.Url}")
            End If
        End If

        ' D. UrlPattern (regex)
        If targetPage Is Nothing AndAlso Not String.IsNullOrEmpty(action.UrlPattern) Then
            Dim resolved = ReplaceInternalVariables(action.UrlPattern)
            Try
                Dim rx As New Regex(resolved, RegexOptions.IgnoreCase)
                targetPage = pages.FirstOrDefault(Function(p) rx.IsMatch(p.Url))
                If targetPage IsNot Nothing Then
                    _logger.LogInfo($"[SwitchTab] Găsit prin urlPattern '{resolved}': {targetPage.Url}")
                End If
            Catch ex As ArgumentException
                Throw New Exception($"[SwitchTab] urlPattern invalid (regex): '{resolved}' — {ex.Message}")
            End Try
        End If

        ' ── 4. Niciun tab găsit → eroare critică ─────────────────────────────────
        If targetPage Is Nothing Then
            Dim criteria As String = String.Join(", ",
                (New String() {
                    If(action.TabIndex >= 1, $"tabIndex={action.TabIndex}", Nothing),
                    If(Not String.IsNullOrEmpty(action.UrlEquals), $"urlEquals={action.UrlEquals}", Nothing),
                    If(Not String.IsNullOrEmpty(action.UrlContains), $"urlContains={action.UrlContains}", Nothing),
                    If(Not String.IsNullOrEmpty(action.UrlPattern), $"urlPattern={action.UrlPattern}", Nothing)
                }).Where(Function(s) s IsNot Nothing))

            Throw New Exception(
                $"[SwitchTab] EROARE CRITICĂ: Niciun tab găsit pentru [{criteria}]." & Environment.NewLine &
                $"Taburi disponibile ({pages.Count}): " &
                String.Join(" | ", pages.Select(Function(p, i) $"[{i + 1}] {p.Url}")))
        End If

        ' ── 5. Switch ─────────────────────────────────────────────────────────────
        _page = targetPage

        Dim bringEx As Exception = Nothing
        Try
            Await _page.BringToFrontAsync()
        Catch ex As Exception
            bringEx = ex
        End Try
        If bringEx IsNot Nothing Then
            _logger.LogWarning($"[SwitchTab] BringToFront eșuat (ignorat): {bringEx.Message}")
        End If

        _logger.LogSuccess($"[SwitchTab] Context mutat pe tab: {_page.Url}")

        ' ── 6. Reload opțional ───────────────────────────────────────────────────
        If action.Reload Then
            _logger.LogInfo("[SwitchTab] Reîncărc tabul...")
            Await _page.ReloadAsync(New PageReloadOptions With {
                .WaitUntil = WaitUntilState.NetworkIdle,
                .Timeout = action.Timeout * 1000
            })
            _logger.LogInfo($"[SwitchTab] Tab reîncărcat. URL curent: {_page.Url}")
        End If

        ' ── 7. SaveCurrentUrlTo ───────────────────────────────────────────────────
        If Not String.IsNullOrEmpty(action.SaveCurrentUrlTo) Then
            SetVariable(action.SaveCurrentUrlTo, _page.Url)
            _logger.LogInfo($"[SwitchTab] URL curent salvat: '{_page.Url}' → [[{action.SaveCurrentUrlTo}]]")
        End If

        ' ── 8. Verificare expectedUrl → Children / ElseChildren ──────────────────
        If Not String.IsNullOrEmpty(action.ExpectedUrl) Then
            Dim resolvedExpected = ReplaceInternalVariables(action.ExpectedUrl)
            Dim urlOk As Boolean = _page.Url.IndexOf(resolvedExpected, StringComparison.OrdinalIgnoreCase) >= 0

            If urlOk Then
                _logger.LogSuccess($"[SwitchTab] URL verificat OK (conține '{resolvedExpected}').")
                If action.Children.Count > 0 Then
                    Await ExecuteActionsAsync(action.Children, True)
                End If
            Else
                _logger.LogWarning($"[SwitchTab] URL mismatch — așteptat '{resolvedExpected}', actual: '{_page.Url}'.")
                If action.ElseChildren.Count > 0 Then
                    Await ExecuteActionsAsync(action.ElseChildren, True)
                End If
            End If
        End If

        ' ── 9. CloseTabWhenDone ───────────────────────────────────────────────────
        If action.CloseTabWhenDone Then
            Dim pageToClose As IPage = _page

            ' Dacă switchăm pe noi înșine (același tab), nu închidem nimic
            If pageToClose Is previousPage Then
                _logger.LogWarning("[SwitchTab] CloseTabWhenDone ignorat: tabul țintă e același cu cel anterior.")
            Else
                ' Revenim pe tabul anterior (dacă e încă deschis), altfel primul disponibil
                If Not previousPage.IsClosed Then
                    _page = previousPage
                    _logger.LogInfo($"[SwitchTab] Revin pe tabul anterior: {_page.Url}")
                Else
                    Dim fallback = _context.Pages.FirstOrDefault(Function(p) Not p.IsClosed AndAlso Not p Is pageToClose)
                    If fallback IsNot Nothing Then
                        _page = fallback
                        _logger.LogWarning($"[SwitchTab] Tabul anterior era închis. Revin pe: {_page.Url}")
                    Else
                        _logger.LogWarning("[SwitchTab] Nu există alt tab deschis după close. _page rămâne pe tabul închis.")
                    End If
                End If

                ' Închidem tabul țintă
                Dim closeEx As Exception = Nothing
                Try
                    Await pageToClose.CloseAsync()
                Catch ex As Exception
                    closeEx = ex
                End Try

                If closeEx IsNot Nothing Then
                    _logger.LogWarning($"[SwitchTab] Eroare la închiderea tabului (ignorat): {closeEx.Message}")
                Else
                    _logger.LogSuccess($"[SwitchTab] Tab închis. Context curent: {_page.Url}")
                End If

                ' BringToFront pe tabul pe care am revenit
                Dim bringBackEx As Exception = Nothing
                Try
                    Await _page.BringToFrontAsync()
                Catch ex As Exception
                    bringBackEx = ex
                End Try
                If bringBackEx IsNot Nothing Then
                    _logger.LogWarning($"[SwitchTab] BringToFront (revenire) eșuat (ignorat): {bringBackEx.Message}")
                End If
            End If
        End If
    End Function
End Class
