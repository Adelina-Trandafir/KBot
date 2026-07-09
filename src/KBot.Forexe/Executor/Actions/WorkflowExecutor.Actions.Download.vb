Imports System.IO
Imports System.Text
Imports KBot.Common
Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteDownloadAsync(action As DownloadAction) As Task
        Dim parsedSelector = ReplaceInternalVariables(action.Selector)
        LogStep(action, $"[Download] Inițiez descărcarea: {parsedSelector}")

        Dim locator = _page.Locator(parsedSelector)
        Dim downloadTask = _page.WaitForDownloadAsync(New PageWaitForDownloadOptions With {.Timeout = action.Timeout * 1000})

        Try
            Await locator.ClickAsync()
        Catch ex As Exception
            _logger.LogError($"[Download] Eroare click: {ex.Message}")
            Throw
        End Try

        Dim download = Await downloadTask
        Dim fileName As String = If(String.IsNullOrEmpty(action.FileName), download.SuggestedFilename, action.FileName)
        Dim folderPath As String = If(String.IsNullOrEmpty(action.SaveFolder), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads"), action.SaveFolder)

        If Not Directory.Exists(folderPath) Then Directory.CreateDirectory(folderPath)

        Dim finalPath As String = Path.Combine(folderPath, fileName)
        Await download.SaveAsAsync(finalPath)
        _logger.LogSuccess($"[Download] Salvat local: {finalPath}")

        ' ==============================================================================
        ' LOGICA DE SALVARE ÎN VARIABILĂ + VALIDĂRI DIMENSIUNE (1MB LIMIT)
        ' ==============================================================================
        If Not String.IsNullOrEmpty(action.SaveTo) Then

            Dim resultToSave As String = String.Empty
            Dim limitBytes As Long = 1048576 ' 1 MB (1024 * 1024)
            Dim realVarName As String = ReplaceInternalVariables(action.SaveTo)

            If action.ParseExcel Then
                ' --- CAZ 1: CU PARSARE (Excel -> JSON prin ApiClient) ---
                ' Executorul nu mai face HTTP: umple un ExcelJob și îl dă procesorului
                ' (adresă + token bearer + POST stau în ApiClient). Un 401 iese ca
                ' ApiException și curge în MainForm.WithReauth (re-login + o reîncercare).
                Try
                    _logger.LogInfo("[Download] Trimit la server pentru conversie JSON...")
                    If _excelProcessor Is Nothing Then
                        Throw New InvalidOperationException("parseExcel necesită procesorul Excel (neconfigurat).")
                    End If

                    Dim fileBytes As Byte() = File.ReadAllBytes(finalPath)
                    Dim job As New ExcelJob With {
                        .FileBase64 = Convert.ToBase64String(fileBytes),
                        .HeaderRows = action.HeaderRows,
                        .SkipFirstNRows = action.SkipFirstNRows,
                        .SkipLastNRows = action.SkipLastNRows,
                        .SkipFirstNColumns = action.SkipFirstNColumns,
                        .SkipLastNColumns = action.SkipLastNColumns,
                        .ComplexFilter = action.ComplexFilter,
                        .FilterColumn = action.FilterColumn,
                        .Filter = action.Filter
                    }

                    resultToSave = Await _excelProcessor.Invoke(job, _cancellationToken).ConfigureAwait(False)

                    ' VALIDARE: dimensiunea JSON-ului rezultat (regulă de business, rămâne aici).
                    Dim jsonSize As Integer = Encoding.UTF8.GetByteCount(resultToSave)
                    If jsonSize > limitBytes Then
                        Throw New Exception($"[LIMITĂ DEPĂȘITĂ] JSON-ul rezultat are {jsonSize / 1024:F2} KB. Limita este 1 MB.")
                    End If
                    _logger.LogSuccess($"[Download] JSON validat ({jsonSize} bytes).")

                Catch ex As Exception
                    _logger.LogError($"[Download] Eroare conversie: {ex.Message}")
                    Throw
                End Try

            Else
                ' --- CAZ 2: FĂRĂ PARSARE (Fișier Brut -> Base64) ---
                ' Aici am "pierdut ramura" data trecută. Acum o tratăm.

                Dim fileInfo As New FileInfo(finalPath)

                ' VALIDARE 2: Dimensiunea fișierului brut de pe disc
                If fileInfo.Length > limitBytes Then
                    Throw New Exception($"[LIMITĂ DEPĂȘITĂ] Fișierul descărcat are {fileInfo.Length / 1024:F2} KB. Limita este 1 MB.")
                End If

                _logger.LogInfo($"[Download] Fișierul brut ({fileInfo.Length} bytes) se încadrează în limită.")

                ' Convertim în Base64 pentru a-l pune în variabilă (ca string)
                Dim bytes = File.ReadAllBytes(finalPath)
                resultToSave = Convert.ToBase64String(bytes)
            End If

            ' --- SALVARE FINALĂ ÎN VARIABILĂ ---
            SetVariable(realVarName, resultToSave)
            _logger.LogSuccess($"[Download] Datele au fost salvate în variabila {{{{{action.SaveTo}}}}}.")

        End If

        ' Deschidere fișier (Opțional)
        If action.OpenFile Then
            Try
                Process.Start(New ProcessStartInfo(finalPath) With {.UseShellExecute = True})
            Catch
            End Try
        End If
    End Function

End Class
