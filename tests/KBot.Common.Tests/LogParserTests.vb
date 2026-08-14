Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Common
Imports Xunit

''' <summary>
''' Testele 1–6 din planul feliei 0031: analizoarele și încărcătorul.
'''
''' Liniile de probă sunt copiate din FORMATUL REAL al scriitorilor (<c>GlobalErrorLog.Write</c>,
''' <c>AdobeHostLog.Write</c>, <c>TreeLogger.Write</c>, <c>DevHarnessForm</c>, <c>utils/logger.py</c>),
''' nu din tabelul planului — planul cere explicit verificarea față de scriitor.
''' </summary>
Public Class LogParserTests

    ' ── Testul 1: câte o linie reală per format ──────────────────────────────

    <Fact>
    Public Sub HarnessErrorParser_CiteșteAntetulReal()
        Dim parser As New HarnessErrorParser()
        Dim entry As LogEntry = Nothing

        ' Exact ce compune GlobalErrorLog.Write: DOUĂ spații înainte de paranteză.
        Dim ok As Boolean = parser.TryParseHeader("==== 2026-08-13 14:22:31.123  [MainForm.LoadTreeAsync] ====", entry)

        Assert.True(ok)
        Assert.Equal(New Date(2026, 8, 13, 14, 22, 31, 123), entry.Timestamp.Value)
        Assert.Equal(KBotLogLevel.Error, entry.Level)
        Assert.Equal("MainForm.LoadTreeAsync", entry.Source)
    End Sub

    <Fact>
    Public Sub AdobeHostParser_CiteșteLiniaReală()
        Dim parser As New AdobeHostParser()
        Dim entry As LogEntry = Nothing

        Dim ok As Boolean = parser.TryParseHeader("2026-08-13 14:22:31.123  profil=Modern fereastră acceptată", entry)

        Assert.True(ok)
        Assert.Equal(New Date(2026, 8, 13, 14, 22, 31, 123), entry.Timestamp.Value)
        Assert.Equal(KBotLogLevel.Info, entry.Level)
        Assert.Equal("profil=Modern fereastră acceptată", entry.Message)
    End Sub

    <Fact>
    Public Sub TreeLoggerParser_CiteșteLiniaReală_CuNivelCompletatCuSpații()
        ' TreeLogger.Write completează nivelul la 5 caractere: "INFO ", "ERR  ".
        Dim parser As New TreeLoggerParser(New Date(2026, 8, 13))
        Dim entry As LogEntry = Nothing

        Dim ok As Boolean = parser.TryParseHeader("[14:22:31.123] [12.345s] [INFO ] [TT.dbg] tooltip afișat", entry)

        Assert.True(ok)
        Assert.Equal(KBotLogLevel.Info, entry.Level)
        Assert.Equal("TT.dbg", entry.Source)
        Assert.Equal("tooltip afișat", entry.Message)
    End Sub

    <Fact>
    Public Sub TreeLoggerParser_ParantezaSursei_LipseșteCuTotulCândSursaEGoală()
        ' Scriitorul pune srcStr = "" (nu "[]") când sursa e goală.
        Dim parser As New TreeLoggerParser(New Date(2026, 8, 13))
        Dim entry As LogEntry = Nothing

        Dim ok As Boolean = parser.TryParseHeader("[14:22:31.123] [0.001s] [ERR  ] ceva a picat", entry)

        Assert.True(ok)
        Assert.Equal(KBotLogLevel.Error, entry.Level)
        Assert.Equal(String.Empty, entry.Source)
        Assert.Equal("ceva a picat", entry.Message)
    End Sub

    <Fact>
    Public Sub RunLogParser_VerdicteleRealeAjungLaNivelurileDecise()
        Dim parser As New RunLogParser()

        Assert.Equal(KBotLogLevel.Info, ParseLevel(parser, "[PASSED] TemaGalerie  (12 ms)  ok"))
        Assert.Equal(KBotLogLevel.Warn, ParseLevel(parser, "[SKIPPED] TestLive  (0 ms)  skipped by user"))
        Assert.Equal(KBotLogLevel.Error, ParseLevel(parser, "[FAILED] Ceva  (5 ms)  aserțiune picată"))
        Assert.Equal(KBotLogLevel.Error, ParseLevel(parser, "[ERROR] Altceva  (5 ms)  excepție"))
    End Sub

    <Fact>
    Public Sub RunLogParser_LiniaEROARE_EAntetDeBloc()
        Dim parser As New RunLogParser()
        Dim entry As LogEntry = Nothing

        Dim ok As Boolean = parser.TryParseHeader("EROARE [btnRunAll_Click]: System.InvalidOperationException: ceva", entry)

        Assert.True(ok)
        Assert.Equal(KBotLogLevel.Error, entry.Level)
        Assert.Equal("btnRunAll_Click", entry.Source)
    End Sub

    <Fact>
    Public Sub RunLogParser_LiniileDeStivă_SuntContinuare_NuAnteturi()
        Dim parser As New RunLogParser()
        Dim entry As LogEntry = Nothing

        Assert.False(parser.TryParseHeader("   at KBot.App.MainForm.Load()", entry))
        Assert.False(parser.TryParseHeader("   --- End of stack trace from previous location ---", entry))
    End Sub

    ' ── Testul 2: antet + cinci linii de stivă = O SINGURĂ intrare ───────────

    <Fact>
    Public Sub HarnessError_AntetPlusCinciLiniiDeStivă_DauOSingurăIntrare()
        Dim text As String = String.Join(Environment.NewLine, {
            "==== 2026-08-13 14:22:31.123  [Sursa.Metoda] ====",
            "System.InvalidOperationException: ceva a mers prost",
            "   at KBot.App.A.B()",
            "   at KBot.App.C.D()",
            "   at KBot.App.E.F()",
            "   at KBot.App.G.H()",
            ""})

        Dim result As LogLoadResult = LogFileLoader.LoadText(
            text, "harness_errors.log", New Date(2026, 8, 13), LogOrigin.Client)

        Assert.Single(result.Entries)
        Dim only As LogEntry = result.Entries(0)
        Assert.Equal(KBotLogLevel.Error, only.Level)
        ' Blocul brut păstrează TOATE liniile, inclusiv antetul.
        Assert.Contains("System.InvalidOperationException", only.Raw)
        Assert.Contains("KBot.App.G.H()", only.Raw)
        Assert.Contains("====", only.Raw)
        ' Mesajul intrării e prima linie a lui ex.ToString(), nu antetul.
        Assert.Equal("System.InvalidOperationException: ceva a mers prost", only.Message)
    End Sub

    ' ── Testul 3: linie nerecunoscută fără nimic înainte ─────────────────────

    <Fact>
    Public Sub LinieNerecunoscutăLaÎnceput_DevinePropriaIntrareUnknown_NuOExcepție()
        ' Fereastra de citire a tăiat blocul căruia îi aparținea linia asta.
        Dim text As String = String.Join(Environment.NewLine, {
            "   at KBot.App.Taiat.LaJumatate()",
            "==== 2026-08-13 14:22:31.123  [Sursa] ===="})

        Dim result As LogLoadResult = LogFileLoader.LoadText(
            text, "harness_errors.log", New Date(2026, 8, 13), LogOrigin.Client)

        Assert.Equal(2, result.Entries.Count)
        Assert.Equal(KBotLogLevel.Unknown, result.Entries(0).Level)
        Assert.False(result.Entries(0).Timestamp.HasValue)
        Assert.Equal(1, result.Entries(0).LineNumber)
    End Sub

    ' ── Testul 4: ora fără dată se leagă de data fișierului ──────────────────

    <Fact>
    Public Sub TreeLogger_OraFărăDată_PrimeșteDataFișierului()
        Dim fileDate As New Date(2026, 8, 13)
        Dim text As String = "[14:22:31.123] [1.000s] [INFO ] [X] prima"

        Dim result As LogLoadResult = LogFileLoader.LoadText(text, "log_tree1.txt", fileDate, LogOrigin.Client)

        Assert.Single(result.Entries)
        Assert.Equal(New Date(2026, 8, 13, 14, 22, 31, 123), result.Entries(0).Timestamp.Value)
    End Sub

    <Fact>
    Public Sub TreeLogger_PesteMiezulNopții_OreleRămânPeAceeașiDată_Documentat()
        ' Comportament DECIS, nu accidental: fișierul nu poartă dată, deci o oră mai mică decât
        ' cea dinainte NU se promovează pe ziua următoare. Se preferă o dată uneori greșită dar
        ' declarată, în locul unei ghiciri.
        Dim fileDate As New Date(2026, 8, 13)
        Dim text As String = String.Join(Environment.NewLine, {
            "[23:59:59.999] [1.000s] [INFO ] [X] înainte de miezul nopții",
            "[00:00:01.000] [2.000s] [INFO ] [X] după miezul nopții"})

        Dim result As LogLoadResult = LogFileLoader.LoadText(text, "log_tree1.txt", fileDate, LogOrigin.Client)

        Assert.Equal(2, result.Entries.Count)
        Assert.Equal(fileDate.Date, result.Entries(0).Timestamp.Value.Date)
        Assert.Equal(fileDate.Date, result.Entries(1).Timestamp.Value.Date)
        ' Orele chiar «merg înapoi» — asta e limitarea, scrisă negru pe alb.
        Assert.True(result.Entries(1).Timestamp.Value < result.Entries(0).Timestamp.Value)
    End Sub

    ' ── Testul 5: ghicire greșită de analizor ────────────────────────────────

    <Fact>
    Public Sub GhicireGreșită_ConținutAdobeÎntrUnFișierHarness_EstePrinsăȘiRaportată()
        ' Numele promite formatul GlobalErrorLog; conținutul e format AdobeHostLog.
        Dim lines As New List(Of String)()
        For i As Integer = 0 To 9
            lines.Add("2026-08-13 14:22:3" & i.ToString() & ".123  linie de adobe")
        Next

        Dim result As LogLoadResult = LogFileLoader.LoadText(
            String.Join(Environment.NewLine, lines), "harness_errors.log", New Date(2026, 8, 13), LogOrigin.Client)

        ' Ghicirea NU a supraviețuit probei…
        Assert.False(result.ParserWasGuessCorrect)
        ' …și se raportează cine a câștigat de fapt.
        Assert.Equal("AdobeHost", result.ParserName)
        Assert.Equal(10, result.Entries.Count)
        Assert.All(result.Entries, Sub(e) Assert.Equal(KBotLogLevel.Info, e.Level))
    End Sub

    <Fact>
    Public Sub GhicireCorectă_FișierHarnessSănătos_NuEDeclaratGreșit()
        ' Regresie pentru capcana pragului: un harness_errors.log NORMAL are un antet și multe
        ' linii de stivă, deci sub 30% anteturi. Pragul procentual NU se aplică formatelor pe
        ' blocuri — altfel fișierul cel mai important al feliei ar fi declarat mereu ghicit greșit.
        Dim lines As New List(Of String) From {"==== 2026-08-13 14:22:31.123  [Sursa] ===="}
        For i As Integer = 0 To 19
            lines.Add("   at KBot.App.Cadru" & i.ToString() & "()")
        Next

        Dim result As LogLoadResult = LogFileLoader.LoadText(
            String.Join(Environment.NewLine, lines), "harness_errors.log", New Date(2026, 8, 13), LogOrigin.Client)

        Assert.True(result.ParserWasGuessCorrect)
        Assert.Equal("HarnessError", result.ParserName)
        Assert.Single(result.Entries)
    End Sub

    ' ── Testul 6: ambele forme de server ─────────────────────────────────────

    <Fact>
    Public Sub ApiServerParser_FormaNouăIso_CuDecalajPropriu()
        Dim parser As New ApiServerParser()
        Dim entry As LogEntry = Nothing

        Dim ok As Boolean = parser.TryParseHeader(
            "2026-08-13T14:22:31.123+03:00 - ERROR - 000_DEMO - 86.120.4.11 - forexe_tree DB error: x", entry)

        Assert.True(ok)
        Assert.Equal(KBotLogLevel.Error, entry.Level)
        Assert.Equal("86.120.4.11", entry.Source)
        Assert.Equal(LogOrigin.Server, entry.Origin)
        ' Linia își poartă decalajul, deci NU mai are nevoie de corecția de ceas.
        Assert.False(entry.TimestampNeedsClockCorrection)
    End Sub

    <Fact>
    Public Sub ApiServerParser_FormaVeche_PatruCâmpuri_CuVirgulăLaMilisecunde()
        ' Formatul de AZI din utils/logger.py: '%(asctime)s - %(levelname)s - %(ip)s - %(message)s',
        ' cu marcajul implicit al modulului logging (virgulă înainte de milisecunde).
        Dim parser As New ApiServerParser()
        Dim entry As LogEntry = Nothing

        Dim ok As Boolean = parser.TryParseHeader(
            "2026-08-13 14:22:31,123 - WARNING - 86.120.4.11 - sesiune expirată", entry)

        Assert.True(ok)
        Assert.Equal(KBotLogLevel.Warn, entry.Level)
        Assert.Equal("86.120.4.11", entry.Source)
        Assert.Equal(New Date(2026, 8, 13, 14, 22, 31, 123), entry.Timestamp.Value)
        ' Fără decalaj în linie -> are nevoie de ServerClock.
        Assert.True(entry.TimestampNeedsClockCorrection)
    End Sub

    <Fact>
    Public Sub ApiServerParser_NivelurilePython_AjungLaNivelurileNoastre()
        Dim parser As New ApiServerParser()
        Assert.Equal(KBotLogLevel.Debug, ParseServerLevel(parser, "DEBUG"))
        Assert.Equal(KBotLogLevel.Info, ParseServerLevel(parser, "INFO"))
        Assert.Equal(KBotLogLevel.Warn, ParseServerLevel(parser, "WARNING"))
        Assert.Equal(KBotLogLevel.Error, ParseServerLevel(parser, "ERROR"))
        Assert.Equal(KBotLogLevel.Error, ParseServerLevel(parser, "CRITICAL"))
    End Sub

    ' ── Moștenirea marcajelor (folosită de filtru) ───────────────────────────

    <Fact>
    Public Sub RunLog_IntrărileFărăDată_MoștenescMarcajulDeLaLiniaData()
        ' Fișierul de rulare NU pune marcaj pe linie; singura dată e pe linia "Data      : …".
        Dim text As String = String.Join(Environment.NewLine, {
            "=== K-BOT Dev Harness — rulare teste ===",
            "Data      : 2026-08-13 14:22:31",
            "[PASSED] Test1  (12 ms)  ok"})

        Dim result As LogLoadResult = LogFileLoader.LoadText(
            text, "test_20260813_142231_123.log", New Date(2026, 8, 13), LogOrigin.Client)

        Dim passed As LogEntry = result.Entries.First(Function(e) e.Raw.StartsWith("[PASSED]", StringComparison.Ordinal))
        Assert.True(passed.Timestamp.HasValue)
        Assert.True(passed.TimestampInherited)
        Assert.Equal(New Date(2026, 8, 13, 14, 22, 31), passed.Timestamp.Value)
        ' Prima linie, dinaintea liniei "Data", rămâne fără dată — și se numără.
        Assert.Equal(1, result.WithoutTimestampCount)
        Assert.Equal(1, result.InheritedTimestampCount)
    End Sub

    <Fact>
    Public Sub LiniileGoale_SuntContinuare_NuÎncepNiciodatăOIntrare()
        ' GlobalErrorLog pune o linie goală după fiecare bloc; ea nu are voie să devină rând.
        Dim text As String = String.Join(Environment.NewLine, {
            "==== 2026-08-13 14:22:31.123  [A] ====",
            "System.Exception: x",
            "",
            "==== 2026-08-13 14:22:32.123  [B] ====",
            "System.Exception: y",
            ""})

        Dim result As LogLoadResult = LogFileLoader.LoadText(
            text, "harness_errors.log", New Date(2026, 8, 13), LogOrigin.Client)

        Assert.Equal(2, result.Entries.Count)
    End Sub

    ' ── ajutoare ─────────────────────────────────────────────────────────────

    Private Shared Function ParseLevel(parser As ILogEntryParser, line As String) As KBotLogLevel
        Dim entry As LogEntry = Nothing
        Assert.True(parser.TryParseHeader(line, entry), "Linia ar fi trebuit recunoscută: " & line)
        Return entry.Level
    End Function

    Private Shared Function ParseServerLevel(parser As ILogEntryParser, levelName As String) As KBotLogLevel
        Return ParseLevel(parser, "2026-08-13 14:22:31,123 - " & levelName & " - 1.2.3.4 - mesaj")
    End Function

End Class
