Imports System.Globalization
Imports System.IO
Imports Microsoft.Playwright
Imports Newtonsoft.Json.Linq

Public Module ForexeSNM

    ''' <summary>
    ''' Descarcă extrasele ForexeSNM via fetch autentificat, filtrând după dată.
    ''' Progresul este raportat prin progressCallback(current, serverTotal, mesaj).
    ''' Edge cases gestionate:
    '''   - server total = 0  → callback(0, 0, "Nu există extrase noi.")
    '''   - niciun extras nou față de dataDeLa → callback(0, total, "Nu există extrase noi față de data selectată.")
    ''' </summary>
    ' ─────────────────────────────────────────────────────────────────────────────
    ' 1. Parse data reală a extrasului din câmpul "descriere"
    '    Returnează DateTime.MinValue dacă parsarea eșuează.
    ' ─────────────────────────────────────────────────────────────────────────────
    Private Function ParseDateDinDescriere(descriere As String) As DateTime
        If String.IsNullOrEmpty(descriere) Then Return DateTime.MinValue

        Dim prefix = "din data "
        Dim idx = descriere.IndexOf(prefix, StringComparison.Ordinal)
        If idx < 0 Then Return DateTime.MinValue

        Dim dateStr = descriere.Substring(idx + prefix.Length).Trim()
        Dim result As DateTime

        If DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm:ss",
                              CultureInfo.InvariantCulture,
                              Globalization.DateTimeStyles.None,
                              result) Then
            Return result
        End If

        Return DateTime.MinValue
    End Function


    ' ─────────────────────────────────────────────────────────────────────────────
    ' 2. Descarcă un singur PDF, îl salvează, opțional face dump BIN, extrage XML.
    '    Returnează JObject cu { PdfFisier, DataFisier, XmlContent }
    '    sau Nothing dacă fișierul e gol sau XML lipsește.
    ' ─────────────────────────────────────────────────────────────────────────────
    Private Async Function DescarcaFisierExtras(page As IPage,
                                            id As Long,
                                            fileName As String,
                                            dtExtras As DateTime,
                                            dtCreare As DateTime,
                                            downloadFolder As String,
                                            dumpFolder As String,
                                            logger As RichTextBoxLogger) As Task(Of JObject)
        Dim downloadUrl =
        $"https://forexe.mfinante.gov.ro/ForexeSNM/messages/downloadFile.do?id={id}&fileName={fileName}&nomCategoryCode=5"

        logger.LogAction($"Download: {fileName}")

        Dim bytes = Await page.EvaluateAsync(Of Byte())(
        $"async () => {{
            const r = await fetch('{downloadUrl}', {{ credentials: 'include' }});
            const b = await r.arrayBuffer();
            return Array.from(new Uint8Array(b));
        }}"
    )

        If bytes Is Nothing OrElse bytes.Length = 0 Then
            logger.LogWarning($"Fisier gol: {fileName}")
            Return Nothing
        End If

        ' ── Dump BIN (opțional) ───────────────────────────────────────────────────
        If Not String.IsNullOrEmpty(dumpFolder) Then
            Try
                If Not Directory.Exists(dumpFolder) Then Directory.CreateDirectory(dumpFolder)

                Dim safeName = $"{id}_{fileName}".Replace(":", "_").Replace("/", "_")
                Dim binPath = Path.Combine(dumpFolder, safeName & ".bin")

                File.WriteAllBytes(binPath, bytes)
                File.WriteAllText(binPath & ".json", New JObject From {
                {"id", id},
                {"fileName", fileName},
                {"dataCreare", dtCreare.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)},
                {"dataExtras", dtExtras.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}
            }.ToString())

                logger.LogDebug($"DUMP BIN: {safeName}")
            Catch exDump As Exception
                logger.LogException(exDump, "Dump BIN")
            End Try
        End If

        ' ── Salvare locală ────────────────────────────────────────────────────────
        Dim fullPath = Path.Combine(downloadFolder, fileName)
        File.WriteAllBytes(fullPath, bytes)
        logger.LogSuccess($"Salvat: {fileName}")

        ' ── Extragere XML ─────────────────────────────────────────────────────────
        Try
            Dim xml = PdfXmlExtractor.PdfHelper.ExtractXmlFromPdfSingle(fullPath, logger)

            If xml IsNot Nothing Then
                logger.LogDebug($"XML extras: {fileName}")
                Return New JObject From {
                {"PdfFisier", fileName},
                {"DataFisier", dtExtras.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)},
                {"XmlContent", xml.XmlContent}
            }
            Else
                logger.LogWarning($"XML lipsa: {fileName}")
                Return Nothing
            End If

        Catch exPdf As Exception
            logger.LogException(exPdf, $"Eroare procesare PDF: {fileName}")
            Return Nothing
        End Try
    End Function


    ' ─────────────────────────────────────────────────────────────────────────────
    ' 3. Paginare + filtrare + orchestrare
    ' ─────────────────────────────────────────────────────────────────────────────
    Public Async Function DescarcaExtraseForexeAPI(page As IPage,
                                               logger As RichTextBoxLogger,
                                               downloadFolder As String,
                                               Optional dataDeLa As DateTime? = Nothing,
                                               Optional dumpFolder As String = Nothing,
                                               Optional progressCallback As Action(Of Integer, Integer, String) = Nothing) As Task(Of JArray)

        Dim rezultate As New JArray()
        Dim downloaded As Integer = 0

        Try
            logger.LogAction("FOREXE API: Start descarcare extrase")

            If Not Directory.Exists(downloadFolder) Then
                Directory.CreateDirectory(downloadFolder)
                logger.LogInfo($"Folder creat: {downloadFolder}")
            End If

            Dim start As Integer = 0
            Dim limit As Integer = 200
            Dim total As Integer = Integer.MaxValue

            While start < total

                Dim url = $"https://forexe.mfinante.gov.ro/ForexeSNM/messages/loadAll.do?nomCategoryCode=5&start={start}&limit={limit}"
                logger.LogDebug($"Request: start={start}, limit={limit}")

                Dim jsonText = Await page.EvaluateAsync(Of String)(
                    $"async () => {{
                        const r = await fetch('{url}', {{ credentials: 'include' }});
                        return await r.text();
                    }}"
                )

                If String.IsNullOrWhiteSpace(jsonText) Then
                    logger.LogError("Raspuns gol de la server")
                    Exit While
                End If

                Dim json = JObject.Parse(jsonText)
                total = json("total").Value(Of Integer)

                If total = 0 Then
                    logger.LogInfo("Nu există niciun extras pe server.")
                    progressCallback?.Invoke(0, 0, "Nu există extrase noi.")
                    Return rezultate
                End If

                logger.LogInfo($"Total extrase server: {total}")

                For Each row In json("rows")
                    Try
                        Dim msg = row("mesajTab")

                        Dim id = msg("id").Value(Of Long)
                        Dim fileName = msg("numeFisier").ToString()
                        Dim dtCreare = DateTimeOffset.FromUnixTimeMilliseconds(
                                       msg("dataCreare").Value(Of Long)).LocalDateTime

                        logger.LogDebug($"Procesare: {id} (dataCreare: {dtCreare.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)})")

                        ' ── Stop paginare: serverul sortează descrescător după dataCreare ──
                        If dataDeLa.HasValue AndAlso dtCreare < dataDeLa.Value Then
                            logger.LogInfo($"Stop paginare la dataCreare: {dtCreare.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)}")
                            If downloaded = 0 Then
                                progressCallback?.Invoke(0, total, "Nu există extrase noi față de data selectată.")
                            End If
                            Return rezultate
                        End If

                        ' ── Parsare dată reală extras ─────────────────────────────
                        Dim dtExtras = ParseDateDinDescriere(msg("descriere")?.ToString())

                        If dtExtras = DateTime.MinValue Then
                            logger.LogWarning($"Skip: nu am putut parsa data din descriere pentru: {fileName}")
                            Continue For
                        End If

                        ' ── Skip individual dacă extrasul e mai vechi ─────────────
                        'If dataDeLa.HasValue AndAlso dtExtras < dataDeLa.Value Then
                        '    logger.LogInfo($"Skip (extras mai vechi): {fileName} — din data {dtExtras:dd.MM.yyyy HH:mm}")
                        '    Continue For
                        'End If

                        ' ── Download + procesare ──────────────────────────────────
                        Dim rezultatRand = Await DescarcaFisierExtras(
                        page, id, fileName, dtExtras, dtCreare, downloadFolder, dumpFolder, logger)

                        If rezultatRand IsNot Nothing Then
                            rezultate.Add(rezultatRand)
                            downloaded += 1
                            progressCallback?.Invoke(downloaded, total, $"S-au descărcat {downloaded} extrase noi")
                        End If

                    Catch exRow As Exception
                        logger.LogException(exRow, "Eroare procesare rand")
                    End Try
                Next

                start += limit
            End While

            logger.LogSuccess("FOREXE API: Finalizat descarcare extrase")

        Catch ex As Exception
            logger.LogException(ex, "FOREXE API: Eroare generala")
        End Try

        Return rezultate
    End Function

    ''' <summary>
    ''' Replayed extrase din fișiere .bin salvate anterior în dumpFolder.
    ''' Nu modifică UI — nu are progressCallback (e folosit doar din STANDALONE).
    ''' </summary>
    Public Function ReplayExtraseFromDump(dumpFolder As String,
                                          logger As RichTextBoxLogger) As JArray

        Dim rezultate As New JArray()

        Try
            If Not Directory.Exists(dumpFolder) Then
                logger.LogError($"Folder inexistent: {dumpFolder}")
                Return rezultate
            End If

            Dim files = Directory.GetFiles(dumpFolder, "*.bin")

            For Each f In files
                Try
                    Dim bytes = File.ReadAllBytes(f)

                    Dim tempPdf = Path.Combine(Path.GetTempPath(),
                                               Path.GetFileNameWithoutExtension(f) & ".pdf")

                    File.WriteAllBytes(tempPdf, bytes)

                    Dim metaPath = f & ".json"
                    Dim fileName As String = Path.GetFileName(tempPdf)
                    Dim dt As DateTime = DateTime.MinValue

                    If File.Exists(metaPath) Then
                        Dim meta = JObject.Parse(File.ReadAllText(metaPath))
                        fileName = meta("fileName")?.ToString()
                        DateTime.TryParseExact(meta("dataCreare")?.ToString(),
                                               "yyyy-MM-dd HH:mm:ss",
                                               CultureInfo.InvariantCulture,
                                               Globalization.DateTimeStyles.None,
                                               dt)
                    End If

                    Dim xml = PdfXmlExtractor.PdfHelper.ExtractXmlFromPdfSingle(tempPdf, logger)

                    If xml IsNot Nothing Then
                        rezultate.Add(New JObject From {
                            {"PdfFisier", fileName},
                            {"DataFisier", dt.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)},
                            {"XmlContent", xml.XmlContent}
                        })

                        logger.LogDebug($"REPLAY OK: {fileName}")
                    Else
                        logger.LogWarning($"REPLAY XML lipsa: {fileName}")
                    End If

                Catch ex As Exception
                    logger.LogException(ex, $"Replay fisier: {f}")
                End Try
            Next

            logger.LogSuccess($"REPLAY complet: {rezultate.Count} extrase")

        Catch ex As Exception
            logger.LogException(ex, "ReplayExtraseFromDump")
        End Try

        Return rezultate
    End Function

End Module