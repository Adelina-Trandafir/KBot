Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe

' LIVE round-trip: proves the ListaAngajamente vertical end to end against the real
' FOREXE session and the 000_DEMO database — scrape -> map -> POST -> persisted ->
' read back. Requires a live FOREXE session: run "FOREXE — Conectare (live)" first
' (same singleton runner keeps the browser open). SessionContext is demo-seeded to
' 000_DEMO in Program.SeedDemoSession until login (felia 1) lands.
Public NotInheritable Class ListaAngajamenteRoundTripLiveTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "FOREXE — ListaAngajamente: LIVE round-trip (scrape -> upsert -> read back)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Forexe"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return True
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return True
        End Get
    End Property

    Public Async Function RunAsync(context As HarnessContext, ct As CancellationToken) _
        As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync

        ' 1) Real services from DI (same singletons MainForm uses).
        Dim runner As IForexeRunner = context.GetService(Of IForexeRunner)()
        Dim session As SessionContext = context.GetService(Of SessionContext)()
        Dim apiClient As IApiClient = context.GetService(Of IApiClient)()

        ' 2) A live session is required — the scrape runs on the open browser.
        Dim concrete As ForexeRunner = TryCast(runner, ForexeRunner)
        If concrete Is Nothing OrElse Not concrete.HasLiveSession Then
            Return HarnessTestResult.Skipped(
                "No live session — run 'FOREXE — Conectare (live)' first, then this test.")
        End If
        If String.IsNullOrEmpty(session.DbName) Then
            Return HarnessTestResult.Skipped("SessionContext.DbName empty — demo seed not applied.")
        End If

        ' Fail fast on missing API config BEFORE the (slow) scrape. Env vars are read at
        ' app startup, so if these are empty they must be set and KBOT relaunched.
        Dim apiOptions As ApiOptions = context.GetService(Of ApiOptions)()
        If String.IsNullOrWhiteSpace(apiOptions.BaseUrl) OrElse String.IsNullOrWhiteSpace(apiOptions.ApiKey) Then
            Return HarnessTestResult.Skipped(
                "API not configured — set KBOT_API_BASE_URL/KBOT_API_KEY, then relaunch KBOT and reconnect.")
        End If

        ' 3) File-only logger for the scrape (no FOREXE RichTextBox here); separate file
        '    from RunLogger's test_*.log so two writers never share one file.
        Dim logsDir As String = Path.Combine(AppContext.BaseDirectory, "Logs")
        Directory.CreateDirectory(logsDir)
        Dim forexeLogPath As String = Path.Combine(logsDir, $"forexe_roundtrip_{DateTime.Now:yyyyMMdd_HHmmss}.log")
        Dim forexeLogger As New RichTextBoxLogger(New RichTextBox()) With {
            .EnableUI = False,
            .LogFilePath = forexeLogPath
        }
        concrete.AttachLogger(forexeLogger)

        ' 4) Same job MainForm builds, run on the existing session.
        Dim job As JobRequest = JobBuilder.BuildListaAngajamente(session)
        Dim progress As New Progress(Of Integer)(
            Sub(p) context.Progress.Report(New HarnessProgressInfo(Math.Min(p, 100), "listaangajamente " & p & "%")))

        Dim result As JobResult = Await runner.RunJobAsync(job, progress, ct)
        Dim logHint As String = " — log: " & forexeLogPath
        If Not result.Success Then
            Return HarnessTestResult.Failed("Scrape/run failed — " & result.Message & logHint)
        End If

        ' 5) Map the scraped table (same seam MainForm uses).
        Dim rows As List(Of Dictionary(Of String, String)) = Nothing
        If Not result.Tables.TryGetValue(WorkflowCatalog.ListaAngajamenteTable, rows) OrElse rows Is Nothing Then
            Return HarnessTestResult.Skipped(
                $"No '{WorkflowCatalog.ListaAngajamenteTable}' table in result (0 rows scraped)." & logHint)
        End If

        Dim mapped As List(Of Angajament) = AngajamentMapper.FromListaAngajamenteResult(rows)
        If mapped.Count = 0 Then
            Return HarnessTestResult.Skipped(
                $"Scrape returned {rows.Count} raw rows but 0 mappable (empty Cod) — nothing to round-trip." & logHint)
        End If

        ' 6) POST to the demo DB, then read back and prove THIS run's rows persisted.
        Await apiClient.UpsertAngajamenteAsync(session.DbName, mapped, ct)

        Dim readBack As IReadOnlyList(Of Angajament) = Await apiClient.GetAngajamenteAsync(session.DbName, session.An, ct)
        If readBack Is Nothing OrElse readBack.Count = 0 Then
            Return HarnessTestResult.Failed(
                $"Read-back returned 0 rows for '{session.DbName}' after upserting {mapped.Count}." & logHint)
        End If

        Dim persisted As New HashSet(Of String)(readBack.Select(Function(a) a.CodAngajament), StringComparer.OrdinalIgnoreCase)
        Dim missing As List(Of String) = mapped.
            Select(Function(a) a.CodAngajament).
            Where(Function(cod) Not persisted.Contains(cod)).
            ToList()
        If missing.Count > 0 Then
            Return HarnessTestResult.Failed(
                $"{missing.Count}/{mapped.Count} upserted codes NOT found on read-back (e.g. {String.Join(", ", missing.Take(5))})." & logHint)
        End If

        Return HarnessTestResult.Passed(
            $"Round-trip OK: scraped {rows.Count} raw -> mapped {mapped.Count} -> upserted to '{session.DbName}' -> " &
            $"read back {readBack.Count}, all {mapped.Count} codes present." & logHint)
    End Function
End Class
