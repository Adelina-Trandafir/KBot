Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports Microsoft.Playwright
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
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
                ' --- CAZ 1: CU PARSARE (Excel -> JSON) ---
                Try
                    _logger.LogInfo($"[Download] Trimit la Python API pentru conversie JSON...")

                    Dim fileBytes As Byte() = File.ReadAllBytes(finalPath)
                    Dim base64File As String = Convert.ToBase64String(fileBytes)

                    ' Autentificare pe sesiunea K-BOT (bearer), nu pe cheie compilată.
                    Dim sessionToken As String = If(_sessionTokenProvider?.Invoke(), String.Empty)
                    If String.IsNullOrEmpty(sessionToken) Then
                        Throw New Exception("parseExcel necesită o sesiune K-BOT autentificată (token bearer absent).")
                    End If

                    Using client As New HttpClient()
                        client.DefaultRequestHeaders.Authorization =
                            New Headers.AuthenticationHeaderValue("Bearer", sessionToken)
                        Dim reqBody As New JObject()
                        reqBody("file_base64") = base64File
                        reqBody("header_rows") = action.HeaderRows
                        reqBody("skipFirstNRows") = action.SkipFirstNRows
                        reqBody("skipLastNRows") = action.SkipLastNRows
                        reqBody("skipFirstNColumns") = action.SkipFirstNColumns
                        reqBody("skipLastNColumns") = action.SkipLastNColumns
                        If action.ComplexFilter <> "" Then
                            reqBody("complex_filter") = action.ComplexFilter
                        ElseIf action.FilterColumn <> "" AndAlso action.Filter <> "" Then
                            reqBody("col_to_filter") = action.FilterColumn
                            reqBody("filter") = action.Filter
                        End If

                        Dim content As New StringContent(reqBody.ToString(), Encoding.UTF8, "application/json")
                        Dim response = Await client.PostAsync(action.ApiUrl, content)
                        Dim respString = Await response.Content.ReadAsStringAsync()

                        If response.IsSuccessStatusCode Then
                            Dim jsonResp As JObject = JObject.Parse(respString)

                            If jsonResp("data") IsNot Nothing Then
                                resultToSave = jsonResp("data").ToString(Formatting.None)
                            Else
                                resultToSave = respString
                            End If

                            ' VALIDARE 1: Dimensiunea JSON-ului rezultat
                            Dim jsonSize As Integer = Encoding.UTF8.GetByteCount(resultToSave)
                            If jsonSize > limitBytes Then
                                Throw New Exception($"[LIMITĂ DEPĂȘITĂ] JSON-ul rezultat are {jsonSize / 1024:F2} KB. Limita este 1 MB.")
                            End If

                            _logger.LogSuccess($"[Download] JSON validat ({jsonSize} bytes).")
                        Else
                            Throw New Exception($"Eroare API Python: {respString}")
                        End If
                    End Using

                Catch ex As Exception
                    _logger.LogError($"[Download] Eroare Parsing: {ex.Message}")
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
