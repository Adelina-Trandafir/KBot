Imports System.IO
Imports System.IO.Compression
Imports System.Globalization
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml
Imports GeneralClasses

''' <summary>
''' Extrage XML-ul embedded dintr-un PDF de tip extras de cont ForexeSNM.
''' Parsează și data din numele fișierului (format: _SIGNED_DDMMYYYYhHHMM.pdf).
''' </summary>
Public Class PdfXmlExtractor
    Private Shared ReadOnly Inv As CultureInfo = CultureInfo.InvariantCulture

    Public Class PdfHelper
        Friend Shared Function ExtractXmlFromPdfSingle(pdfPath As String, logger As RichTextBoxLogger) As PdfExtractResult
            Dim result As New PdfExtractResult With {
                .Success = False,
                .XmlContent = Nothing,
                .FileName = Path.GetFileName(pdfPath),
                .FileDate = DateTime.MinValue,
                .ErrorMessage = Nothing
            }

            Try
                logger.LogAction($"PDF: Procesez {pdfPath}")

                ' ─────────────────────────────────────────────
                ' 1. Verific fișier
                ' ─────────────────────────────────────────────
                If Not File.Exists(pdfPath) Then
                    result.ErrorMessage = "Fișier inexistent"
                    logger.LogError($"PDF: Fișier inexistent: {pdfPath}")
                    Return result
                End If

                ' ─────────────────────────────────────────────
                ' 2. Parse date din nume
                ' ─────────────────────────────────────────────
                Try
                    result.FileDate = PdfXmlExtractor.ParseDateFromFileName(result.FileName)

                    If result.FileDate = DateTime.MinValue Then
                        logger.LogWarning($"PDF: Nu am putut extrage data din nume: {result.FileName}")
                    Else
                        logger.LogDebug($"PDF: Data extrasă = {result.FileDate:dd.MM.yyyy HH:mm:ss}")
                    End If

                Catch ex As Exception
                    logger.LogException(ex, "PDF: Eroare parse dată din nume")
                End Try

                ' ─────────────────────────────────────────────
                ' 3. Extrage XML
                ' ─────────────────────────────────────────────
                Dim xmlText As String = Nothing
                Dim err As String = Nothing

                Dim ok As Boolean = PdfXmlExtractor.TryExtract(pdfPath, xmlText, err)

                If Not ok Then
                    result.ErrorMessage = err
                    logger.LogWarning($"PDF: Nu am extras XML: {err}")
                    Return result
                End If

                ' ─────────────────────────────────────────────
                ' 4. Validare minimă
                ' ─────────────────────────────────────────────
                If String.IsNullOrWhiteSpace(xmlText) Then
                    result.ErrorMessage = "XML gol"
                    logger.LogWarning("PDF: XML gol după extracție")
                    Return result
                End If

                ' ─────────────────────────────────────────────
                ' 5. SUCCESS
                ' ─────────────────────────────────────────────
                result.Success = True
                result.XmlContent = xmlText

                logger.LogSuccess($"PDF: XML extras cu succes ({result.FileName})")

                Return result

            Catch ex As Exception
                result.ErrorMessage = ex.Message
                logger.LogException(ex, $"PDF: Eroare critică la procesare {pdfPath}")
                Return result
            End Try

        End Function

    End Class

    ' =========================================================================
    '  ENTRY POINT
    ' =========================================================================

    ''' <summary>
    ''' Încearcă extragerea XML-ului embedded din PDF.
    ''' Returnează True dacă a reușit, False dacă nu există XML sau PDF-ul e invalid.
    ''' Nu aruncă excepții — erorile sunt returnate în errorMessage.
    ''' </summary>
    Public Shared Function TryExtract(pdfPath As String,
                                      ByRef xmlText As String,
                                      ByRef errorMessage As String) As Boolean
        xmlText = Nothing
        errorMessage = Nothing

        Dim pdfBytes As Byte() = Nothing
        Dim pdfAscii As String = Nothing

        ' ── 1. Citesc fișierul ───────────────────────────────────────────────
        Try
            pdfBytes = File.ReadAllBytes(pdfPath)
            pdfAscii = Encoding.ASCII.GetString(pdfBytes)
        Catch ex As Exception
            errorMessage = $"Citire PDF eșuată: {ex.Message}"
            Return False
        End Try

        ' ── 2. Găsesc numărul obiectului EmbeddedFile ────────────────────────
        Dim embeddedObjNo As Integer = -1
        Try
            embeddedObjNo = FindEmbeddedFileObjectNumber(pdfAscii)
        Catch ex As Exception
            errorMessage = $"Eroare căutare obiect EmbeddedFile: {ex.Message}"
            Return False
        End Try

        If embeddedObjNo <= 0 Then
            errorMessage = "Nu există referință /EF /F către obiect EmbeddedFile în PDF."
            Return False
        End If

        ' ── 3. Găsesc startul obiectului ─────────────────────────────────────
        Dim objectStart As Integer = -1
        Try
            objectStart = FindObjectStart(pdfAscii, embeddedObjNo)
        Catch ex As Exception
            errorMessage = $"Eroare localizare obiect {embeddedObjNo}: {ex.Message}"
            Return False
        End Try

        If objectStart < 0 Then
            errorMessage = $"Obiectul {embeddedObjNo} 0 obj nu a fost găsit în PDF."
            Return False
        End If

        ' ── 4. Extrag stream-ul ───────────────────────────────────────────────
        Dim streamInfo As PdfStreamInfo = Nothing
        Try
            streamInfo = ExtractObjectStream(pdfBytes, pdfAscii, objectStart)
        Catch ex As Exception
            errorMessage = $"Eroare extragere stream din obiectul {embeddedObjNo}: {ex.Message}"
            Return False
        End Try

        If streamInfo Is Nothing Then
            errorMessage = $"Obiectul {embeddedObjNo} nu conține un stream valid."
            Return False
        End If

        ' ── 5. Încerc XML direct (necomprimat) ───────────────────────────────
        Dim rawXml As String = Nothing
        Try
            If TryGetXmlFromRawBytes(streamInfo.StreamData, rawXml) Then
                xmlText = rawXml
                Return True
            End If
        Catch ex As Exception
            ' Nu opresc execuția — încerc în continuare cu decompresia
        End Try

        ' ── 6. Încerc decompresia FlateDecode ────────────────────────────────
        If Not HasFlateDecode(streamInfo.FilterText) Then
            errorMessage = "Stream-ul nu e raw XML și nu declară /FlateDecode — nu pot extrage XML."
            Return False
        End If

        Dim decompressedXml As String = Nothing
        Try
            If TryDecompressZLib(streamInfo.StreamData, decompressedXml) Then
                xmlText = decompressedXml
                Return True
            End If
        Catch ex As Exception
            ' Continuăm cu fallback Deflate
        End Try

        Try
            If TryDecompressDeflate(streamInfo.StreamData, decompressedXml) Then
                xmlText = decompressedXml
                Return True
            End If
        Catch ex As Exception
            errorMessage = $"Decompresia a eșuat (ZLib + Deflate): {ex.Message}"
            Return False
        End Try

        errorMessage = "XML negăsit după toate metodele de extracție."
        Return False
    End Function

    ' =========================================================================
    '  PARSE DATA DIN NUMELE FIȘIERULUI
    '  Format: ..._SIGNED_DDMMYYYYhHHMM.pdf
    '  ex: TREZ521_ExtrasEP_PDFCLI_2845508_XML_SIGNED_07042026h1614.pdf
    '      → 07.04.2026 16:14:00
    ' =========================================================================

    Public Shared Function ParseDateFromFileName(fileName As String) As DateTime
        Try
            Dim m = Regex.Match(fileName,
                "_SIGNED_(\d{2})(\d{2})(\d{4})h(\d{2})(\d{2})\.pdf",
                RegexOptions.IgnoreCase)

            If Not m.Success Then Return DateTime.MinValue

            Dim day As Integer = Integer.Parse(m.Groups(1).Value, Inv)
            Dim month As Integer = Integer.Parse(m.Groups(2).Value, Inv)
            Dim year As Integer = Integer.Parse(m.Groups(3).Value, Inv)
            Dim hour As Integer = Integer.Parse(m.Groups(4).Value, Inv)
            Dim min As Integer = Integer.Parse(m.Groups(5).Value, Inv)

            Return New DateTime(year, month, day, hour, min, 0)
        Catch ex As Exception
            Return DateTime.MinValue
        End Try
    End Function

    ' =========================================================================
    '  FIND EMBEDDED FILE OBJECT NUMBER
    ' =========================================================================

    Private Shared Function FindEmbeddedFileObjectNumber(pdfText As String) As Integer
        Dim patterns As String() = {
            "/Type\s*/Filespec[\s\S]{0,400}?/EF\s*<<[\s\S]{0,200}?/F\s+(\d+)\s+0\s+R",
            "/EF\s*<<[\s\S]{0,200}?/F\s+(\d+)\s+0\s+R"
        }

        For Each pattern As String In patterns
            Dim m As Match = Regex.Match(pdfText, pattern, RegexOptions.IgnoreCase)
            If m.Success Then
                Dim objNo As Integer
                If Integer.TryParse(m.Groups(1).Value, NumberStyles.Integer, Inv, objNo) Then
                    Return objNo
                End If
            End If
        Next

        Return -1
    End Function

    ' =========================================================================
    '  FIND OBJECT START
    ' =========================================================================

    Private Shared Function FindObjectStart(pdfText As String, objNo As Integer) As Integer
        Dim pattern As String = "(?m)(^|\r|\n)" & objNo.ToString(Inv) & "\s+0\s+obj\b"
        Dim m As Match = Regex.Match(pdfText, pattern)
        If m.Success Then
            Return m.Index + m.Groups(1).Length
        End If
        Return -1
    End Function

    ' =========================================================================
    '  EXTRACT OBJECT STREAM
    ' =========================================================================

    Private Shared Function ExtractObjectStream(pdfBytes As Byte(),
                                                pdfText As String,
                                                objectStart As Integer) As PdfStreamInfo
        Dim streamWordIndex As Integer = pdfText.IndexOf("stream", objectStart, StringComparison.Ordinal)
        If streamWordIndex < 0 Then Return Nothing

        Dim endObjIndex As Integer = pdfText.IndexOf("endobj", objectStart, StringComparison.Ordinal)
        If endObjIndex < 0 Then Return Nothing
        If streamWordIndex > endObjIndex Then Return Nothing

        Dim endStreamIndex As Integer = pdfText.IndexOf("endstream", streamWordIndex, StringComparison.Ordinal)
        If endStreamIndex < 0 OrElse endStreamIndex > endObjIndex Then Return Nothing

        Dim dictText As String = pdfText.Substring(objectStart, streamWordIndex - objectStart)

        ' Sar peste \r\n sau \n sau \r după "stream"
        Dim dataStart As Integer = streamWordIndex + "stream".Length
        If dataStart < pdfBytes.Length Then
            If pdfBytes(dataStart) = &HD AndAlso
               dataStart + 1 < pdfBytes.Length AndAlso
               pdfBytes(dataStart + 1) = &HA Then
                dataStart += 2
            ElseIf pdfBytes(dataStart) = &HA OrElse pdfBytes(dataStart) = &HD Then
                dataStart += 1
            End If
        End If

        ' Tail trimming
        Dim dataEnd As Integer = endStreamIndex
        While dataEnd > dataStart AndAlso
              (pdfBytes(dataEnd - 1) = &HA OrElse pdfBytes(dataEnd - 1) = &HD)
            dataEnd -= 1
        End While

        If dataEnd <= dataStart Then Return Nothing

        Dim length As Integer = dataEnd - dataStart
        Dim data(length - 1) As Byte
        Buffer.BlockCopy(pdfBytes, dataStart, data, 0, length)

        Return New PdfStreamInfo With {
            .DictionaryText = dictText,
            .FilterText = ExtractFilterText(dictText),
            .StreamData = data
        }
    End Function

    ' =========================================================================
    '  FILTER TEXT
    ' =========================================================================

    Private Shared Function ExtractFilterText(dictText As String) As String
        If String.IsNullOrWhiteSpace(dictText) Then Return String.Empty

        Dim mArray As Match = Regex.Match(dictText,
            "/Filter\s*\[(.*?)\]",
            RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        If mArray.Success Then Return mArray.Groups(1).Value

        Dim mSingle As Match = Regex.Match(dictText,
            "/Filter\s*/([A-Za-z0-9]+)",
            RegexOptions.IgnoreCase)
        If mSingle.Success Then Return "/" & mSingle.Groups(1).Value

        Return String.Empty
    End Function

    Private Shared Function HasFlateDecode(filterText As String) As Boolean
        If String.IsNullOrWhiteSpace(filterText) Then Return False
        Return filterText.IndexOf("FlateDecode", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    ' =========================================================================
    '  XML FROM RAW BYTES
    ' =========================================================================

    Private Shared Function TryGetXmlFromRawBytes(data As Byte(),
                                                   ByRef xmlText As String) As Boolean
        xmlText = Nothing

        Dim encodings As Encoding() = {
            New UTF8Encoding(False, False),
            Encoding.Unicode,
            Encoding.BigEndianUnicode,
            Encoding.ASCII
        }

        For Each enc As Encoding In encodings
            Try
                Dim candidate As String = enc.GetString(data).Trim(ChrW(0), " "c, vbTab, vbCr, vbLf)
                If LooksLikeXml(candidate) AndAlso IsValidXml(candidate) Then
                    xmlText = candidate
                    Return True
                End If
            Catch ex As Exception
                ' Încerc următoarea codificare
            End Try
        Next

        Return False
    End Function

    ' =========================================================================
    '  DECOMPRESIE
    ' =========================================================================

    Private Shared Function TryDecompressZLib(data As Byte(),
                                               ByRef xmlText As String) As Boolean
        xmlText = Nothing
        Try
            Using inputMs As New MemoryStream(data)
                Using z As New ZLibStream(inputMs, CompressionMode.Decompress)
                    Using outMs As New MemoryStream()
                        z.CopyTo(outMs)
                        Dim raw As Byte() = outMs.ToArray()
                        Return TryGetXmlFromRawBytes(raw, xmlText)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Shared Function TryDecompressDeflate(data As Byte(),
                                                  ByRef xmlText As String) As Boolean
        xmlText = Nothing
        Try
            Using inputMs As New MemoryStream(data)
                Using d As New DeflateStream(inputMs, CompressionMode.Decompress)
                    Using outMs As New MemoryStream()
                        d.CopyTo(outMs)
                        Dim raw As Byte() = outMs.ToArray()
                        Return TryGetXmlFromRawBytes(raw, xmlText)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    ' =========================================================================
    '  XML VALIDATION
    ' =========================================================================

    Private Shared Function LooksLikeXml(text As String) As Boolean
        If String.IsNullOrWhiteSpace(text) Then Return False
        Dim t As String = text.TrimStart()
        If t.StartsWith("<?xml", StringComparison.Ordinal) Then Return True
        If t.StartsWith("<", StringComparison.Ordinal) AndAlso
           t.Contains(">") AndAlso
           t.Contains("</") Then Return True
        Return False
    End Function

    Private Shared Function IsValidXml(text As String) As Boolean
        Try
            Dim doc As New XmlDocument()
            doc.XmlResolver = Nothing
            doc.LoadXml(text)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    ' =========================================================================
    '  MODEL INTERN
    ' =========================================================================

    Private Class PdfStreamInfo
        Public Property DictionaryText As String
        Public Property FilterText As String
        Public Property StreamData As Byte()
    End Class

End Class