Imports System.Globalization
Imports System.IO
Imports Newtonsoft.Json.Linq
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteExtractXmlFromPdfAsync(action As ExtractXmlFromPdfAction) As Task
        Dim resolvedFolder As String = ReplaceInternalVariables(action.Folder)
        Dim resolvedDataDeLa As String = ReplaceInternalVariables(action.DataDeLa)
        Dim resolvedSaveTo As String = ReplaceInternalVariables(action.SaveTo)

        LogStep(action, $"ExtractXmlFromPdf — folder: '{resolvedFolder}'" &
                        $"{If(Not String.IsNullOrEmpty(resolvedDataDeLa), $" | dataDeLa: {resolvedDataDeLa}", "")}")

        ' ── 1. Validare folder ────────────────────────────────────────────────
        If Not Directory.Exists(resolvedFolder) Then
            Throw New DirectoryNotFoundException(
                $"[ExtractXmlFromPdf] Folderul nu există: '{resolvedFolder}'")
        End If

        ' ── 2. Parse dataDeLa ─────────────────────────────────────────────────
        Dim filterDate As DateTime = DateTime.MinValue

        If Not String.IsNullOrEmpty(resolvedDataDeLa) Then
            Dim parsed As DateTime
            Dim parseOk As Boolean = DateTime.TryParseExact(
                resolvedDataDeLa,
                New String() {"dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm", "dd.MM.yyyy"},
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                parsed)

            If parseOk Then
                filterDate = parsed
                _logger.LogInfo($"[ExtractXmlFromPdf] Filtru dată activ: >= {filterDate:dd.MM.yyyy HH:mm:ss}")
            Else
                _logger.LogWarning($"[ExtractXmlFromPdf] dataDeLa '{resolvedDataDeLa}' nu a putut fi parsat — procesez toate fișierele.")
            End If
        End If

        ' ── 3. Enumerez PDF-urile ─────────────────────────────────────────────
        Dim allPdfs As String() = Nothing

        Try
            allPdfs = Directory.GetFiles(resolvedFolder, "*.pdf", SearchOption.TopDirectoryOnly)
        Catch ex As Exception
            Throw New IOException($"[ExtractXmlFromPdf] Eroare la listarea folderului '{resolvedFolder}': {ex.Message}", ex)
        End Try

        _logger.LogInfo($"[ExtractXmlFromPdf] PDF-uri găsite: {allPdfs.Length}")

        ' ── 4. Procesare fișiere ──────────────────────────────────────────────
        Dim resultArray As New JArray()
        Dim cProcessed As Integer = 0
        Dim cSkipped As Integer = 0
        Dim cFailed As Integer = 0

        For Each pdfPath As String In allPdfs
            Dim fileName As String = Path.GetFileName(pdfPath)

            ' A. Extrag data din nume
            Dim fileDate As DateTime = PdfXmlExtractor.ParseDateFromFileName(fileName)

            If fileDate = DateTime.MinValue Then
                _logger.LogWarning($"[ExtractXmlFromPdf] '{fileName}' — dată negăsită în nume fișier, sar peste.")
                cSkipped += 1
                Continue For
            End If

            ' B. Aplic filtrul B3
            If filterDate > DateTime.MinValue AndAlso fileDate < filterDate Then
                _logger.LogInfo($"[ExtractXmlFromPdf] '{fileName}' ({fileDate:dd.MM.yyyy HH:mm}) este înainte de filtru, sar peste.")
                cSkipped += 1
                Continue For
            End If

            ' C. Extrag XML
            Dim xmlContent As String = Nothing
            Dim errMsg As String = Nothing
            Dim ok As Boolean = PdfXmlExtractor.TryExtract(pdfPath, xmlContent, errMsg)

            If Not ok Then
                _logger.LogWarning($"[ExtractXmlFromPdf] '{fileName}' — {errMsg}")
                cFailed += 1
                Continue For
            End If

            ' D. Construiesc obiectul result
            Dim obj As New JObject()
            obj("PdfFisier") = fileName
            obj("DataFisier") = fileDate.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)
            obj("XmlContent") = xmlContent

            resultArray.Add(obj)
            cProcessed += 1

            _logger.LogInfo($"[ExtractXmlFromPdf] '{fileName}' → XML extras OK ({fileDate:dd.MM.yyyy HH:mm:ss})")
        Next

        ' ── 5. Salvez rezultatul ──────────────────────────────────────────────
        SetVariable(resolvedSaveTo, resultArray.ToString(Newtonsoft.Json.Formatting.None))

        _logger.LogSuccess(
            $"[ExtractXmlFromPdf] Gata. Procesate: {cProcessed} | Sărite: {cSkipped} | Eșuate: {cFailed}" &
            $" → [[{resolvedSaveTo}]]")

        Await Task.CompletedTask
    End Function

End Class