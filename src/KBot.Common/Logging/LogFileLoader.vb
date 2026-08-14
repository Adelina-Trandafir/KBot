Option Strict On
Imports System.Collections.Generic
Imports System.IO

''' <summary>Ce a ieșit din încărcarea unui fișier de jurnal.</summary>
Public NotInheritable Class LogLoadResult

    Public Sub New(entries As IReadOnlyList(Of LogEntry),
                   parserName As String,
                   parserWasGuessCorrect As Boolean,
                   inheritedTimestampCount As Integer,
                   withoutTimestampCount As Integer,
                   wasTruncated As Boolean,
                   fileLengthBytes As Long)
        Me.Entries = entries
        Me.ParserName = parserName
        Me.ParserWasGuessCorrect = parserWasGuessCorrect
        Me.InheritedTimestampCount = inheritedTimestampCount
        Me.WithoutTimestampCount = withoutTimestampCount
        Me.WasTruncated = wasTruncated
        Me.FileLengthBytes = fileLengthBytes
    End Sub

    Public ReadOnly Property Entries As IReadOnlyList(Of LogEntry)

    ''' <summary>Analizorul care a CÂȘTIGAT, nu cel ghicit după nume.</summary>
    Public ReadOnly Property ParserName As String

    ''' <summary>
    ''' False dacă ghicirea după numele fișierului a picat proba de potrivire și s-a căzut pe
    ''' alegerea per linie. Merită arătat: înseamnă că un fișier nu are formatul pe care îl
    ''' promite numele lui.
    ''' </summary>
    Public ReadOnly Property ParserWasGuessCorrect As Boolean

    ''' <summary>Câte intrări au primit marcajul de la intrarea dinaintea lor.</summary>
    Public ReadOnly Property InheritedTimestampCount As Integer

    ''' <summary>Câte intrări au rămas FĂRĂ marcaj — filtrul de interval le exclude.</summary>
    Public ReadOnly Property WithoutTimestampCount As Integer

    Public ReadOnly Property WasTruncated As Boolean
    Public ReadOnly Property FileLengthBytes As Long

End Class

''' <summary>
''' Alege analizorul, coase liniile de continuare în blocuri și moștenește marcajele lipsă.
'''
''' <para><b>Ghicirea după nume se VERIFICĂ.</b> Un analizor ales greșit nu dă nicio eroare — dă o
''' grilă plină de rânduri <c>Unknown</c>, adică exact felul de defect care mănâncă o după-amiază.
''' Deci analizorul ghicit se trece peste primele linii neagole și, dacă recunoaște prea puține ca
''' anteturi, se cade pe alegerea per linie și se RAPORTEAZĂ cine a câștigat de fapt.</para>
''' </summary>
Public Module LogFileLoader

    ''' <summary>Câte linii neagole se folosesc ca probă pentru ghicirea analizorului.</summary>
    Public Const ProbeLineCount As Integer = 50

    ''' <summary>Sub acest procent de anteturi recunoscute, ghicirea după nume se consideră greșită.</summary>
    Public Const MinimumHeaderRatio As Double = 0.3

    ''' <summary>
    ''' Încarcă un fișier de pe disc: citește coada, alege analizorul, construiește intrările.
    ''' Aruncă la I/O — vezi <c>LogFileReader.ReadTail</c>.
    ''' </summary>
    Public Function LoadFile(filePath As String,
                             Optional windowBytes As Long = LogFileReader.DefaultWindowBytes) As LogLoadResult
        Dim read As LogReadResult = LogFileReader.ReadTail(filePath, windowBytes)
        Dim fileName As String = Path.GetFileName(filePath)

        ' Data fișierului: singura sursă de dată pentru formatul TreeLogger, care scrie doar ora.
        Dim fileDate As Date = Date.Today
        Try
            Dim info As New FileInfo(filePath)
            If info.Exists Then fileDate = info.LastWriteTime
        Catch ex As IOException
            ' Data nu s-a putut citi: rămâne ziua de azi. Nu merită oprită încărcarea pentru asta,
            ' dar nici ascuns — intrările TreeLogger vor purta o dată aproximativă.
            Diagnostics.Trace.WriteLine("LogFileLoader: nu am putut citi data fișierului " & filePath & ": " & ex.Message)
        End Try

        Return LoadText(read.Text, fileName, fileDate, LogOrigin.Client, read.WasTruncated, read.FileLengthBytes)
    End Function

    ''' <summary>
    ''' Încarcă din TEXT deja citit — ruta folosită pentru jurnalele de server (venite prin API)
    ''' și de teste. Pură: nu atinge discul.
    ''' </summary>
    Public Function LoadText(text As String,
                             fileName As String,
                             fileDate As Date,
                             origin As LogOrigin,
                             Optional wasTruncated As Boolean = False,
                             Optional fileLengthBytes As Long = 0L) As LogLoadResult
        Dim lines As String() = SplitLines(If(text, String.Empty))
        Dim candidates As List(Of ILogEntryParser) = BuildParsers(fileDate)

        Dim guessed As ILogEntryParser = GuessByFileName(fileName, candidates)
        Dim guessCorrect As Boolean = True
        Dim chosen As ILogEntryParser = guessed

        If guessed IsNot Nothing AndAlso Not GuessSurvivesProbe(guessed, lines) Then
            guessCorrect = False
            chosen = Nothing        ' per linie: încearcă fiecare analizor, în ordine
        ElseIf guessed Is Nothing Then
            chosen = Nothing
        End If

        Dim entries As New List(Of LogEntry)()
        Dim winner As String = If(chosen IsNot Nothing, chosen.Name, String.Empty)

        For i As Integer = 0 To lines.Length - 1
            Dim line As String = lines(i)

            ' O linie goală e MEREU continuare. Nu începe niciodată o intrare — altfel linia goală
            ' pe care GlobalErrorLog o pune după fiecare bloc ar deveni un rând în grilă.
            If String.IsNullOrWhiteSpace(line) Then
                If entries.Count > 0 Then entries(entries.Count - 1).AppendRawLine(line)
                Continue For
            End If

            Dim parsed As LogEntry = Nothing
            Dim usedParser As String = String.Empty

            If chosen IsNot Nothing Then
                If chosen.TryParseHeader(line, parsed) Then usedParser = chosen.Name
            Else
                For Each p As ILogEntryParser In candidates
                    Dim attempt As LogEntry = Nothing
                    If p.TryParseHeader(line, attempt) Then
                        parsed = attempt
                        usedParser = p.Name
                        Exit For
                    End If
                Next
            End If

            If parsed IsNot Nothing Then
                parsed.FileName = fileName
                parsed.LineNumber = i + 1
                ' Analizorul de server își pune singur Origin; pentru rest îl decide apelantul.
                If parsed.Origin <> LogOrigin.Server Then parsed.Origin = origin
                entries.Add(parsed)
                If String.IsNullOrEmpty(winner) Then winner = usedParser
            ElseIf entries.Count > 0 Then
                ' Continuare: intră în blocul dinainte. Ajunge și la Message, dar DOAR dacă
                ' mesajul e încă gol — cazul antetului GlobalErrorLog, al cărui mesaj real e
                ' prima linie a lui ex.ToString().
                Dim last As LogEntry = entries(entries.Count - 1)
                last.AppendRawLine(line)
                If String.IsNullOrEmpty(last.Message) Then last.Message = line.Trim()
            Else
                ' Continuare fără nimic înainte: fereastra de citire a tăiat blocul căruia îi
                ' aparținea. Devine propria ei intrare, nerecunoscută — niciodată o excepție.
                Dim orphan As New LogEntry(Nothing, KBotLogLevel.Unknown, String.Empty, line.Trim(), line) With {
                    .FileName = fileName,
                    .LineNumber = i + 1,
                    .Origin = origin}
                entries.Add(orphan)
            End If
        Next

        ' Moștenirea marcajelor: o intrare fără dată o ia pe a celei dinaintea ei. Fișierul de
        ' rulare al bancului de probă e cazul de bază — o rulare întreagă are o singură dată.
        Dim inherited As Integer = 0
        Dim without As Integer = 0
        Dim running As Date? = Nothing
        For Each e As LogEntry In entries
            If e.Timestamp.HasValue Then
                running = e.Timestamp
            ElseIf running.HasValue Then
                e.Timestamp = running
                e.TimestampInherited = True
                inherited += 1
            Else
                without += 1
            End If
        Next

        If String.IsNullOrEmpty(winner) Then winner = "Fallback"
        Return New LogLoadResult(entries, winner, guessCorrect, inherited, without, wasTruncated, fileLengthBytes)
    End Function

    ''' <summary>Analizoarele disponibile, în ordinea în care se încearcă la alegerea per linie.</summary>
    Private Function BuildParsers(fileDate As Date) As List(Of ILogEntryParser)
        ' Ordinea contează: cele cu antet strict înaintea celor permisive. FallbackParser NU e în
        ' listă — ar accepta orice linie și ar face inutilă orice alegere.
        Return New List(Of ILogEntryParser) From {
            New HarnessErrorParser(),
            New ApiServerParser(),
            New TreeLoggerParser(fileDate),
            New AdobeHostParser(),
            New RunLogParser()}
    End Function

    ''' <summary>Ghicirea după tiparul numelui de fișier (§5.5 din plan).</summary>
    Private Function GuessByFileName(fileName As String, candidates As List(Of ILogEntryParser)) As ILogEntryParser
        Dim name As String = If(fileName, String.Empty).ToLowerInvariant()
        ' Arhivele poartă generația la coadă (".log.3") — ghicim după numele de bază.
        Dim baseName As String = name
        For generation As Integer = 1 To LogRotation.BackupCount
            Dim suffix As String = "." & generation.ToString(Globalization.CultureInfo.InvariantCulture)
            If baseName.EndsWith(suffix, StringComparison.Ordinal) Then
                baseName = baseName.Substring(0, baseName.Length - suffix.Length)
                Exit For
            End If
        Next

        Dim wanted As String
        If baseName = "harness_errors.log" Then
            wanted = "HarnessError"
        ElseIf baseName = "adobe_preview.log" Then
            wanted = "AdobeHost"
        ElseIf baseName.StartsWith("api_", StringComparison.Ordinal) Then
            wanted = "ApiServer"
        ElseIf baseName.StartsWith("test_", StringComparison.Ordinal) Then
            wanted = "RunLog"
        ElseIf baseName.StartsWith("log_", StringComparison.Ordinal) Then
            wanted = "TreeLogger"
        Else
            Return Nothing
        End If

        For Each p As ILogEntryParser In candidates
            If p.Name = wanted Then Return p
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Trece analizorul ghicit peste primele <see cref="ProbeLineCount"/> linii neagole.
    ''' True dacă recunoaște cel puțin <see cref="MinimumHeaderRatio"/> dintre ele ca anteturi.
    ''' </summary>
    Private Function GuessSurvivesProbe(parser As ILogEntryParser, lines As String()) As Boolean
        Dim probed As Integer = 0
        Dim headers As Integer = 0
        For Each line As String In lines
            If String.IsNullOrWhiteSpace(line) Then Continue For
            probed += 1
            Dim tmp As LogEntry = Nothing
            If parser.TryParseHeader(line, tmp) Then headers += 1
            If probed >= ProbeLineCount Then Exit For
        Next
        ' Fișier gol: nu avem ce infirma, deci ghicirea rămâne valabilă.
        If probed = 0 Then Return True

        ' Format pe blocuri: pragul procentual NU se aplică (vezi ExpectsHeaderOnEveryLine).
        ' Un singur antet recunoscut dovedește că formatul e cel promis de numele fișierului.
        If Not parser.ExpectsHeaderOnEveryLine Then Return headers > 0

        Return (headers / CDbl(probed)) >= MinimumHeaderRatio
    End Function

    ''' <summary>Împarte pe linii tolerând CRLF, LF și CR.</summary>
    Private Function SplitLines(text As String) As String()
        Return text.Replace(vbCrLf, vbLf).Replace(ControlChars.Cr, ControlChars.Lf).Split(ControlChars.Lf)
    End Function

End Module
