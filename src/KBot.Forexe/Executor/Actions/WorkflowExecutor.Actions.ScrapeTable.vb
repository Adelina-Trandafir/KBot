Imports Microsoft.Playwright
Imports Newtonsoft.Json.Linq
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    ' ============================================================
    '  ORCHESTRATOR
    ' ============================================================
    Private Async Function ExecuteScrapeTableAsync(action As ScrapeTableAction) As Task
        Dim parsedSelector = ReplaceInternalVariables(action.Selector)
        _logger.LogInfo($"[ScrapeTable] Încep extragerea: {parsedSelector}")

        ' --- NAVIGARE PAGE/ROW ---
        If Not String.IsNullOrEmpty(action.Page) Then
            Await NavigateToPageAsync(action)
        Else
            Await NavigateToLastPageIfNeededAsync(action)
        End If

        ' --- SINGLE ROW MODE ---
        If Not String.IsNullOrEmpty(action.Row) Then
            Dim waitOk = Await TryWaitForElementAsync(parsedSelector, WaitForSelectorState.Attached, action.Timeout)
            If Not waitOk Then Throw New TimeoutException($"[ScrapeTable] Tabelul '{parsedSelector}' nu a apărut în {action.Timeout}s.")

            Dim rawRows = Await ExtractRawRowsAsync(parsedSelector, action)

            If rawRows.Count = 0 Then
                _logger.LogWarning("[ScrapeTable] Row mode: niciun rând găsit.")
                Return
            End If

            Dim targetIndex As Integer
            Select Case action.Row.Trim().ToLower()
                Case "last" : targetIndex = rawRows.Count - 1
                Case "first" : targetIndex = 0
                Case Else
                    If Not Integer.TryParse(action.Row, targetIndex) Then targetIndex = 0
                    targetIndex = Math.Max(0, Math.Min(targetIndex - 1, rawRows.Count - 1))
            End Select

            Dim finalSaveTo = ReplaceInternalVariables(action.SaveTo)
            If Not String.IsNullOrEmpty(finalSaveTo) Then
                Dim jObj As JObject = JObject.FromObject(rawRows(targetIndex))
                Dim finalObj = FilterColumns(jObj, action)
                Dim singleArray As New JArray(finalObj)
                SetVariable(finalSaveTo, singleArray.ToString(Newtonsoft.Json.Formatting.None))
                _logger.LogSuccess($"[ScrapeTable] Rând '{action.Row}' (index {targetIndex}) salvat în '{finalSaveTo}' ca array.")
            End If
            Return
        End If

        ' --- MOD NORMAL ---
        Dim allData As New List(Of Object)
        Dim currentPage As Integer = 1
        Dim exitScrape As Boolean

        Do
            Try
                Dim waitOk = Await TryWaitForElementAsync(parsedSelector, WaitForSelectorState.Attached, action.Timeout)
                If Not waitOk Then
                    Throw New TimeoutException($"[ScrapeTable] Tabelul '{parsedSelector}' nu a apărut în {action.Timeout}s.")
                End If

                Dim physPage = Await GetPhysicalPageAsync()
                Dim pageLabel = If(String.IsNullOrEmpty(physPage), $"{currentPage}", $"{currentPage} [fizic: {physPage}]")
                LogStep(action, $"Procesez pagina {pageLabel}")

                Dim rawRows = Await ExtractRawRowsAsync(parsedSelector, action)
                exitScrape = ProcessRawRows(rawRows, action, allData)

                If exitScrape Then Exit Do

                Dim shouldContinue As Boolean
                If action.StartFromLast Then
                    shouldContinue = Await NavigateBackwardAsync(action, currentPage)
                Else
                    shouldContinue = Await NavigateForwardAsync(action, currentPage)
                End If

                If Not shouldContinue Then Exit Do

            Catch ex As Exception
                _logger.LogError($"[ScrapeTable] Eroare critică la pagina {currentPage}: {ex.Message}")
                Throw
            End Try

            currentPage += 1
        Loop

        SaveScrapeResults(allData, action, exitScrape)
    End Function

    ' ============================================================
    '  INIT — navighează la ultima pagină dacă StartFromLast
    ' ============================================================
    Private Async Function NavigateToLastPageIfNeededAsync(action As ScrapeTableAction) As Task
        If Not action.StartFromLast OrElse String.IsNullOrEmpty(action.LastPageSelector) Then Return
        Await NavigateToLastPageAsync(action)
    End Function

    ' ============================================================
    '  EXTRAGERE JS — returnează lista de rânduri brute
    ' ============================================================
    Private Async Function ExtractRawRowsAsync(parsedSelector As String,
                                               action As ScrapeTableAction) As Task(Of List(Of Object))
        Dim locator = If(action.Strict,
                         _page.Locator(parsedSelector),
                         _page.Locator(parsedSelector).First)

        Await locator.WaitForAsync(New LocatorWaitForOptions With {
            .State = WaitForSelectorState.Visible,
            .Timeout = action.Timeout * 1000
        })

        Dim pageData = Await locator.EvaluateAsync(Of Object)(GetEmbeddedJs("ScrapeTableExtract.js"))

        Dim rawRows As New List(Of Object)()

        If pageData Is Nothing Then
            _logger.LogWarning("[ScrapeTable] Fără date pe pagina curentă.")
            Return rawRows
        End If

        Dim pageDataList = TryCast(pageData, IEnumerable)
        If pageDataList Is Nothing Then
            _logger.LogWarning("[ScrapeTable] Date în format neașteptat.")
            Return rawRows
        End If

        For Each item In pageDataList
            rawRows.Add(item)
        Next

        If action.StartFromLast Then rawRows.Reverse()
        Return rawRows
    End Function

    ' ============================================================
    '  PROCESARE RÂNDURI — skip + filtrare coloane + exit conditions
    '  Returnează True dacă exit-ul a fost declanșat
    ' ============================================================
    Private Function ProcessRawRows(rawRows As List(Of Object),
                                    action As ScrapeTableAction,
                                    allData As List(Of Object)) As Boolean
        Dim totalRows As Integer = rawRows.Count
        Dim startRow As Integer = Math.Max(action.SkipFirstNRows, 0)
        Dim endRow As Integer = Math.Min(totalRows - action.SkipLastNRows, totalRows)
        Dim addedCount As Integer = 0
        Dim hasEquals As Boolean = Not String.IsNullOrEmpty(action.ExitIfCellEquals)
        Dim hasDate As Boolean = Not String.IsNullOrEmpty(action.ExitIfCellDate)

        For i As Integer = startRow To endRow - 1
            Try
                Dim jObj As JObject = JObject.FromObject(rawRows(i))
                Dim finalObj As JObject = FilterColumns(jObj, action)

                If hasEquals AndAlso EvaluateCellEquals(finalObj, action.ExitIfCellEquals) Then
                    _logger.LogInfo($"[ScrapeTable] exitIfCellEquals declanșat la rândul {i}.")
                    Return True
                End If

                If hasDate AndAlso EvaluateCellDate(finalObj, action.ExitIfCellDate) Then
                    _logger.LogInfo($"[ScrapeTable] exitIfCellDate declanșat la rândul {i}.")
                    Return True
                End If

                If action.StartFromLast Then allData.Insert(0, finalObj) Else allData.Add(finalObj)
                addedCount += 1

            Catch ex As Exception
                _logger.LogWarning($"[ScrapeTable] Eroare rând {i}: {ex.Message}. Adaug brut.")
                If action.StartFromLast Then allData.Insert(0, rawRows(i)) Else allData.Add(rawRows(i))
                addedCount += 1
            End Try
        Next

        _logger.LogDebug($"[ScrapeTable] {addedCount} rânduri păstrate (din {totalRows}).")
        Return False
    End Function

    ' ============================================================
    '  FILTRARE COLOANE
    ' ============================================================
    Private Shared Function FilterColumns(jObj As JObject, action As ScrapeTableAction) As JObject
        If action.SkipFirstNColumns = 0 AndAlso action.SkipLastNColumns = 0 Then Return jObj

        Dim props As List(Of JProperty) = jObj.Properties().ToList()
        Dim totalCols As Integer = props.Count
        Dim startCol As Integer = Math.Max(action.SkipFirstNColumns, 0)
        Dim endCol As Integer = Math.Min(totalCols - action.SkipLastNColumns, totalCols)
        Dim filtered As New JObject()

        For c As Integer = startCol To endCol - 1
            If c < props.Count Then filtered.Add(props(c))
        Next

        Return filtered
    End Function

    ' ============================================================
    '  NavigateForwardAsync
    ' ============================================================
    Private Async Function NavigateForwardAsync(action As ScrapeTableAction, currentPage As Integer) As Task(Of Boolean)
        If String.IsNullOrEmpty(action.NextPageSelector) Then Return False

        Dim nextSelector = ReplaceInternalVariables(action.NextPageSelector)
        Dim nextBtn = _page.Locator(nextSelector)

        If Await nextBtn.CountAsync() = 0 OrElse Not Await nextBtn.IsVisibleAsync() Then
            _logger.LogInfo("[ScrapeTable] Butonul Next nu mai e disponibil. Gata.")
            Return False
        End If

        _logger.LogInfo($"[ScrapeTable] Navigare → pagina {currentPage + 1} (de pe pagina {currentPage})...")

        Dim clicked As Boolean = False
        Dim attempts As Integer = 0

        While Not clicked AndAlso attempts < 3
            attempts += 1
            Dim shouldRetry As Boolean = False
            Dim shouldExit As Boolean = False

            Try
                Await nextBtn.ClickAsync()
                clicked = True
            Catch ex As PlaywrightException When ex.Message.Contains("element is not stable") OrElse
                                                 ex.Message.Contains("was detached from the DOM")
                _logger.LogWarning($"[ScrapeTable] Buton instabil, tentativa {attempts}.")
                If attempts >= 3 Then shouldExit = True Else shouldRetry = True
            Catch ex As PlaywrightException
                _logger.LogWarning($"[ScrapeTable] Eroare click Next: {ex.Message}")
                Return False
            End Try

            If shouldExit Then
                _logger.LogWarning("[ScrapeTable] Abandon după 3 tentative.")
                Return False
            End If

            If shouldRetry Then
                nextBtn = _page.Locator(nextSelector)
                Await nextBtn.WaitForAsync(New LocatorWaitForOptions With {
                    .State = WaitForSelectorState.Visible,
                    .Timeout = action.Timeout * 1000
                })
            End If
        End While

        Await WaitForWicketIdleAsync()
        Await LogCurrentPageAsync()
        Return True
    End Function

    ' ============================================================
    '  NavigateBackwardAsync
    ' ============================================================
    Private Async Function NavigateBackwardAsync(action As ScrapeTableAction, currentPage As Integer) As Task(Of Boolean)
        Dim prevSelector = If(String.IsNullOrEmpty(action.PrevPageSelector), "a.prev", action.PrevPageSelector)
        Dim prevBtn = _page.Locator(prevSelector)

        If Await prevBtn.CountAsync() = 0 Then
            _logger.LogInfo("[ScrapeTable] Am ajuns la prima pagină. Opresc.")
            Return False
        End If

        _logger.LogInfo($"[ScrapeTable] Navigare ← pagina {currentPage - 1} (de pe pagina {currentPage})...")

        Dim clicked As Boolean = False
        Dim attempts As Integer = 0

        While Not clicked AndAlso attempts < 3
            attempts += 1
            Dim shouldRetry As Boolean = False
            Dim shouldExit As Boolean = False

            Try
                Await prevBtn.ClickAsync()
                clicked = True
            Catch ex As PlaywrightException When ex.Message.Contains("element is not stable") OrElse
                                                 ex.Message.Contains("was detached from the DOM")
                _logger.LogWarning($"[ScrapeTable] Buton instabil, tentativa {attempts}.")
                If attempts >= 3 Then shouldExit = True Else shouldRetry = True
            Catch ex As PlaywrightException
                _logger.LogWarning($"[ScrapeTable] Eroare click Prev: {ex.Message}")
                Return False
            End Try

            If shouldExit Then
                _logger.LogWarning("[ScrapeTable] Abandon după 3 tentative.")
                Return False
            End If

            If shouldRetry Then
                prevBtn = _page.Locator(prevSelector)
                Await prevBtn.WaitForAsync(New LocatorWaitForOptions With {
                    .State = WaitForSelectorState.Visible,
                    .Timeout = action.Timeout * 1000
                })
            End If
        End While

        Await WaitForWicketIdleAsync()
        Await LogCurrentPageAsync()
        Return True
    End Function

    ' ============================================================
    '  NAVIGARE LA PAGINA SPECIFICATA (page="first/last/N")
    ' ============================================================
    Private Async Function NavigateToPageAsync(action As ScrapeTableAction) As Task
        Select Case action.Page.Trim().ToLower()
            Case "last"
                Await NavigateToLastPageAsync(action)
            Case "first"
                Await NavigateToFirstPageAsync(action)
            Case Else
                Dim targetPage As Integer
                If Integer.TryParse(action.Page.Trim(), targetPage) AndAlso targetPage > 0 Then
                    Await NavigateToPageNumberAsync(action, targetPage)
                Else
                    _logger.LogWarning($"[ScrapeTable] page='{action.Page}' — valoare invalida. Raman pe pagina curenta.")
                End If
        End Select
    End Function

    ' ============================================================
    '  NavigateToLastPageAsync
    ' ============================================================
    Private Async Function NavigateToLastPageAsync(action As ScrapeTableAction) As Task
        If String.IsNullOrEmpty(action.LastPageSelector) Then
            Throw New InvalidOperationException("[ScrapeTable] page=last dar lastPageSelector lipseste.")
        End If

        Dim lastBtn = _page.Locator(action.LastPageSelector)
        If Await lastBtn.CountAsync() = 0 OrElse Not Await lastBtn.IsVisibleAsync() Then
            _logger.LogInfo("[ScrapeTable] Suntem deja pe ultima pagina.")
            Await LogCurrentPageAsync()
            Return
        End If

        _logger.LogInfo("[ScrapeTable] Navighez la ultima pagina (page=last)...")

        Dim attempts As Integer = 0
        While attempts < 3
            attempts += 1

            Await lastBtn.ClickAsync()
            Await WaitForWicketIdleAsync()

            Dim lastBtnNow = _page.Locator(action.LastPageSelector)
            If Await lastBtnNow.CountAsync() = 0 OrElse Not Await lastBtnNow.IsVisibleAsync() Then
                Await LogCurrentPageAsync()
                Return
            End If

            _logger.LogWarning($"[ScrapeTable] Navigare la ultima pagina nereușită (tentativa {attempts}/3). Reincerc...")
            Await Task.Delay(500)
        End While

        _logger.LogWarning("[ScrapeTable] Nu am reușit să ajung la ultima pagina după 3 tentative.")
    End Function

    ' ============================================================
    '  NavigateToFirstPageAsync
    ' ============================================================
    Private Async Function NavigateToFirstPageAsync(action As ScrapeTableAction) As Task
        If String.IsNullOrEmpty(action.FirstPageSelector) Then
            Throw New InvalidOperationException("[ScrapeTable] page=first dar firstPageSelector lipseste.")
        End If

        Dim firstBtn = _page.Locator(action.FirstPageSelector)
        If Await firstBtn.CountAsync() = 0 OrElse Not Await firstBtn.IsVisibleAsync() Then
            _logger.LogInfo("[ScrapeTable] Suntem deja pe prima pagina.")
            Return
        End If

        _logger.LogInfo("[ScrapeTable] Navighez la prima pagina (page=first)...")

        Await firstBtn.ClickAsync()
        Await WaitForWicketIdleAsync()
    End Function

    ' ============================================================
    '  NavigateToPageNumberAsync
    ' ============================================================
    Private Async Function NavigateToPageNumberAsync(action As ScrapeTableAction, targetPage As Integer) As Task
        If String.IsNullOrEmpty(action.FirstPageSelector) Then
            Throw New InvalidOperationException($"[ScrapeTable] page={targetPage} dar firstPageSelector lipseste.")
        End If
        If String.IsNullOrEmpty(action.NextPageSelector) Then
            Throw New InvalidOperationException($"[ScrapeTable] page={targetPage} dar nextPageSelector lipseste.")
        End If

        _logger.LogInfo($"[ScrapeTable] Navighez la pagina {targetPage} — pornesc de la prima pagina...")
        Await NavigateToFirstPageAsync(action)

        If targetPage = 1 Then Return

        For i As Integer = 1 To targetPage - 1
            Dim nextBtn = _page.Locator(action.NextPageSelector)

            If Await nextBtn.CountAsync() = 0 OrElse Not Await nextBtn.IsVisibleAsync() Then
                Throw New InvalidOperationException($"[ScrapeTable] Butonul Next nu mai e disponibil la pagina {i}. Pagina {targetPage} nu exista.")
            End If

            _logger.LogInfo($"[ScrapeTable] Navighez la pagina {i + 1} din {targetPage}...")
            Await nextBtn.ClickAsync()
            Await WaitForWicketIdleAsync()
        Next
    End Function

    ' ============================================================
    '  SALVARE FINALĂ
    ' ============================================================
    Private Sub SaveScrapeResults(allData As List(Of Object), action As ScrapeTableAction, exitScrape As Boolean)
        Dim finalJson = Newtonsoft.Json.JsonConvert.SerializeObject(allData, Newtonsoft.Json.Formatting.Indented)
        Dim finalSaveTo = ReplaceInternalVariables(action.SaveTo)

        If String.IsNullOrEmpty(finalSaveTo) Then Return

        SetVariable(finalSaveTo, finalJson)
        _logger.LogSuccess($"[ScrapeTable] Finalizat{If(exitScrape, " (exit condiție)", "")}. " &
                           $"Total {allData.Count} rânduri în '{finalSaveTo}'.")
    End Sub

    ' ============================================================
    '  GetPhysicalPageAsync
    ' ============================================================
    Private Async Function GetPhysicalPageAsync() As Task(Of String)
        Try
            Dim loc = _page.Locator("span.goto em span")
            If Await loc.CountAsync() > 0 Then
                Return (Await loc.First.InnerTextAsync()).Trim()
            End If
        Catch
        End Try
        Return String.Empty
    End Function

    ' ============================================================
    '  LogCurrentPageAsync
    ' ============================================================
    Private Async Function LogCurrentPageAsync() As Task
        Dim pageNum = Await GetPhysicalPageAsync()
        If Not String.IsNullOrEmpty(pageNum) Then
            RaiseEvent OnLogMessage($"[ScrapeTable] Pagina curentă: {pageNum}")
        End If
    End Function

End Class