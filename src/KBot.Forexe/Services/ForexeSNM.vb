Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports GeneralClasses   ' PdfExtractResult / RichTextBoxLogger
Imports KBot.Common
Imports Microsoft.Playwright
Imports Newtonsoft.Json.Linq

''' <summary>
''' Downloads the SNM bank statements (extrase de cont) from FOREXE — slice 0057, the port
''' of <c>Surse/SURSA_FOREXE/Services/ForexeSNM.vb</c>.
''' </summary>
''' <remarks>
''' <para>This is NOT a workflow. There is no <c>.wfl</c> behind it and there never was:
''' FOREXE's message inbox answers a plain JSON endpoint, and the PDFs come off a second
''' one. The old system called it <c>SNM_INTERNAL</c> for that reason. Both calls are made
''' with <c>page.EvaluateAsync</c> and <c>fetch(..., credentials: 'include')</c>, so they
''' ride the session cookie of the browser the operator already authenticated — nothing
''' here knows about certificates or tokens.</para>
'''
''' <para><b>What the paging relies on.</b> The server returns the messages newest first.
''' That is why <c>dataDeLa</c> can stop the walk on the first message older than it rather
''' than filtering every page: the rest are older still. If FOREXE ever changed that order,
''' this would silently fetch too little — so the stop is logged, every time, with the date
''' that triggered it.</para>
'''
''' <para><b>Newtonsoft, not System.Text.Json.</b> The rest of K-BOT talks to our own Flask
''' server with System.Text.Json. This reads a foreign JSON body of unknown shape and digs
''' three fields out of it; <c>JObject</c> indexing is the tool for that, the package is
''' already referenced by this project, and none of these objects leave the robot —
''' <see cref="ExtrasDescarcat"/> is what crosses out.</para>
'''
''' <para><b>What it does NOT do.</b> It does not parse the statement XML and it does not
''' write to any database. The XML is read on the server, by the import route, which is the
''' only side that can resolve a unit or a classification. Access split it the same way.</para>
''' </remarks>
Public NotInheritable Class ForexeSNM

    ' The FOREXE inbox: category 5 is the bank-statement category. Both addresses are the
    ' ones the old robot used, unchanged -- they are FOREXE's, not ours to redesign.
    Private Const InboxUrl As String =
        "https://forexe.mfinante.gov.ro/ForexeSNM/messages/loadAll.do?nomCategoryCode=5&start={0}&limit={1}"
    Private Const DownloadUrl As String =
        "https://forexe.mfinante.gov.ro/ForexeSNM/messages/downloadFile.do?id={0}&fileName={1}&nomCategoryCode=5"

    ' One inbox page. 200 is what the old robot asked for and what the server answers.
    Private Const PageSize As Integer = 200

    ' The description carries the statement's real date as «... din data yyyy-MM-dd HH:mm:ss».
    ' The file name carries one too, but a coarser one (to the minute) and in another format,
    ' so the description is the source -- as in the original.
    Private Const PrefixData As String = "din data "

    Private Shared ReadOnly Inv As CultureInfo = CultureInfo.InvariantCulture

    Private Sub New()
        ' Static-only.
    End Sub

    ''' <summary>
    ''' Walks the FOREXE inbox newest-first, downloads every statement not older than
    ''' <paramref name="dataDeLa"/>, saves each PDF under <paramref name="folderDescarcare"/>
    ''' and returns the ones whose embedded XML could be unwrapped.
    ''' </summary>
    ''' <param name="dataDeLa">
    ''' Stop once a message older than this is met. Nothing walks the whole inbox.
    ''' </param>
    ''' <param name="progres">
    ''' Called as (downloaded so far, server total, a line for the operator). Optional.
    ''' </param>
    Public Shared Async Function DescarcaExtraseAsync(page As IPage,
                                                      logger As RichTextBoxLogger,
                                                      folderDescarcare As String,
                                                      dataDeLa As Date?,
                                                      progres As Action(Of Integer, Integer, String),
                                                      ct As CancellationToken) As Task(Of List(Of ExtrasDescarcat))
        If page Is Nothing Then Throw New ArgumentNullException(NameOf(page))
        If logger Is Nothing Then Throw New ArgumentNullException(NameOf(logger))
        If String.IsNullOrWhiteSpace(folderDescarcare) Then
            Throw New ArgumentException("Folderul de descărcare este obligatoriu.", NameOf(folderDescarcare))
        End If

        Dim rezultate As New List(Of ExtrasDescarcat)()
        Try
            logger.LogAction("SNM: încep descărcarea extraselor de cont.")
            Directory.CreateDirectory(folderDescarcare)

            Dim start As Integer = 0
            Dim total As Integer = Integer.MaxValue

            While start < total
                ct.ThrowIfCancellationRequested()

                Dim pagina As JObject = Await CitestePaginaAsync(page, start, logger, ct).ConfigureAwait(False)
                If pagina Is Nothing Then Exit While

                total = CitesteTotal(pagina)
                If total = 0 Then
                    logger.LogInfo("SNM: nu există niciun extras pe server.")
                    progres?.Invoke(0, 0, "Nu există extrase noi.")
                    Return rezultate
                End If
                logger.LogInfo($"SNM: {total} extrase pe server.")

                Dim randuri As JToken = pagina("rows")
                If randuri Is Nothing Then
                    ' A page with a total but no rows is a contract we do not understand;
                    ' walking on would loop over the same offset forever.
                    Throw New InvalidOperationException(
                        "Răspunsul FOREXE nu conține «rows» — formatul cutiei de mesaje s-a schimbat.")
                End If

                For Each rand As JToken In randuri
                    ct.ThrowIfCancellationRequested()

                    Dim mesaj As JToken = rand("mesajTab")
                    If mesaj Is Nothing Then
                        logger.LogWarning("SNM: rând fără «mesajTab» — sărit.")
                        Continue For
                    End If

                    Dim id As Long = CLng(mesaj("id"))
                    Dim numeFisier As String = CStr(mesaj("numeFisier"))
                    Dim dataCreare As Date =
                        DateTimeOffset.FromUnixTimeMilliseconds(CLng(mesaj("dataCreare"))).LocalDateTime

                    ' The stop, not a skip: the inbox is newest-first, so everything past
                    ' this point is older still. Said out loud with the date that caused it,
                    ' because it is the ONE assumption this whole walk rests on.
                    If dataDeLa.HasValue AndAlso dataCreare < dataDeLa.Value Then
                        logger.LogInfo($"SNM: opresc paginarea la dataCreare {dataCreare.ToString("dd.MM.yyyy HH:mm", Inv)}.")
                        If rezultate.Count = 0 Then
                            progres?.Invoke(0, total, "Nu există extrase noi față de data selectată.")
                        End If
                        Return rezultate
                    End If

                    Dim dataExtras As Date = ParseDataDinDescriere(CStr(mesaj("descriere")))
                    If dataExtras = Date.MinValue Then
                        ' DataFisier is half of the file HASH. Without it the statement
                        ' cannot be deduplicated on the server, so it is skipped rather
                        ' than sent with a made-up date that would import it every time.
                        logger.LogWarning($"SNM: nu am putut citi data din descriere — sar peste {numeFisier}.")
                        Continue For
                    End If

                    Dim extras As ExtrasDescarcat =
                        Await DescarcaUnExtrasAsync(page, id, numeFisier, dataExtras,
                                                    folderDescarcare, logger, ct).ConfigureAwait(False)
                    If extras IsNot Nothing Then
                        rezultate.Add(extras)
                        progres?.Invoke(rezultate.Count, total, $"S-au descărcat {rezultate.Count} extrase noi.")
                    End If
                Next

                start += PageSize
            End While

            logger.LogSuccess($"SNM: descărcare încheiată — {rezultate.Count} extrase.")
            Return rezultate
        Catch ex As OperationCanceledException
            ' The operator's cancel is not a defect: say how far it got and let it out.
            logger.LogWarning($"SNM: anulat după {rezultate.Count} extrase.")
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeSNM.DescarcaExtraseAsync", ex)
            Throw
        End Try
    End Function

    ' ── Interne ──────────────────────────────────────────────────────────

    ''' <summary>
    ''' One inbox page, through the page's own fetch so the session cookie travels with it.
    ''' Nothing = the server answered with an empty body (logged; the walk stops).
    ''' </summary>
    Private Shared Async Function CitestePaginaAsync(page As IPage, start As Integer,
                                                     logger As RichTextBoxLogger,
                                                     ct As CancellationToken) As Task(Of JObject)
        ct.ThrowIfCancellationRequested()
        Dim url As String = String.Format(Inv, InboxUrl, start, PageSize)
        logger.LogDebug($"SNM: cerere start={start}, limit={PageSize}.")

        Dim text As String = Await page.EvaluateAsync(Of String)(
            "async () => { const r = await fetch('" & url & "', { credentials: 'include' }); return await r.text(); }"
        ).ConfigureAwait(False)

        If String.IsNullOrWhiteSpace(text) Then
            logger.LogError("SNM: răspuns gol de la server.")
            Return Nothing
        End If
        Return JObject.Parse(text)
    End Function

    ''' <summary>The server's row count. A missing or non-numeric «total» is a broken contract.</summary>
    Private Shared Function CitesteTotal(pagina As JObject) As Integer
        Dim total As JToken = pagina("total")
        If total Is Nothing Then
            Throw New InvalidOperationException(
                "Răspunsul FOREXE nu conține «total» — formatul cutiei de mesaje s-a schimbat.")
        End If
        Return CInt(total)
    End Function

    ''' <summary>
    ''' Downloads one PDF, saves it, and unwraps the XML embedded in it. Nothing when the
    ''' file came back empty or carries no XML — both are logged, and both mean «there is
    ''' nothing here to import», not «the run failed».
    ''' </summary>
    Private Shared Async Function DescarcaUnExtrasAsync(page As IPage, id As Long, numeFisier As String,
                                                        dataExtras As Date, folderDescarcare As String,
                                                        logger As RichTextBoxLogger,
                                                        ct As CancellationToken) As Task(Of ExtrasDescarcat)
        ct.ThrowIfCancellationRequested()
        Dim url As String = String.Format(Inv, DownloadUrl, id, numeFisier)
        logger.LogAction($"SNM: descarc {numeFisier}.")

        ' The PDF comes back as a byte array built in the page: fetch -> arrayBuffer ->
        ' Array.from(Uint8Array). Playwright can only bring back JSON, so the bytes travel
        ' as numbers. Same shape as the original.
        Dim octeti As Byte() = Await page.EvaluateAsync(Of Byte())(
            "async () => { const r = await fetch('" & url & "', { credentials: 'include' }); " &
            "const b = await r.arrayBuffer(); return Array.from(new Uint8Array(b)); }"
        ).ConfigureAwait(False)

        If octeti Is Nothing OrElse octeti.Length = 0 Then
            logger.LogWarning($"SNM: fișier gol — {numeFisier}.")
            Return Nothing
        End If

        Dim cale As String = Path.Combine(folderDescarcare, numeFisier)
        File.WriteAllBytes(cale, octeti)
        logger.LogSuccess($"SNM: salvat {numeFisier}.")

        Dim extras As PdfExtractResult = PdfXmlExtractor.PdfHelper.ExtractXmlFromPdfSingle(cale, logger)
        If extras Is Nothing OrElse Not extras.Success OrElse String.IsNullOrWhiteSpace(extras.XmlContent) Then
            ' The PDF stays on disk on purpose: a statement whose XML we could not unwrap
            ' is exactly the one someone will want to open by hand.
            logger.LogWarning($"SNM: fără XML în {numeFisier} — rămâne doar PDF-ul, pe disc.")
            Return Nothing
        End If

        Return New ExtrasDescarcat() With {
            .PdfFisier = numeFisier,
            .DataFisier = dataExtras.ToString("dd.MM.yyyy HH:mm:ss", Inv),
            .XmlContent = extras.XmlContent,
            .CaleLocala = cale
        }
    End Function

    ''' <summary>
    ''' The statement's date out of the FOREXE description («... din data yyyy-MM-dd HH:mm:ss»).
    ''' Date.MinValue when it is not there or does not parse — the caller skips that statement.
    ''' </summary>
    Friend Shared Function ParseDataDinDescriere(descriere As String) As Date
        If String.IsNullOrEmpty(descriere) Then Return Date.MinValue

        Dim idx As Integer = descriere.IndexOf(PrefixData, StringComparison.Ordinal)
        If idx < 0 Then Return Date.MinValue

        Dim text As String = descriere.Substring(idx + PrefixData.Length).Trim()
        Dim rezultat As Date
        If Date.TryParseExact(text, "yyyy-MM-dd HH:mm:ss", Inv, DateTimeStyles.None, rezultat) Then
            Return rezultat
        End If
        Return Date.MinValue
    End Function

End Class
