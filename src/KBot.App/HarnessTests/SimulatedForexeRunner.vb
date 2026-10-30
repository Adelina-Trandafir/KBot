#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography.X509Certificates
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe

' Slice 0125 -- a FOREXE that exists only inside K-BOT, for testing the DDF signatures (A, B, director)
' without touching forexecab. Debug only: the file does not exist in a Release build, and the only way
' to switch it on is the «Aplicatia cu FOREXE simulat» entry of the Debug start window.

''' <summary>
''' The one switch of the simulation. The start window sets it BEFORE the shell is built; the runner
''' registration in <c>Program.ConfigureServices</c> reads it when the runner is first asked for.
''' </summary>
Public NotInheritable Class SimulatedForexeSwitch

    Private Sub New()
    End Sub

    ''' <summary>True = the application runs on <see cref="SimulatedForexeRunner"/>, not on the real robot.</summary>
    Public Shared Property Active As Boolean
End Class

''' <summary>
''' Slice 0125: a FOREXE robot with a memory. It answers the sending workflows of slice 0081
''' (<c>CreareAngajament</c>, <c>IncarcaRezervare</c>, <c>DefinitivareAngajament</c>,
''' <c>DerulareAngajament</c>) and the read-back (<c>RezervariAngajament</c>) the way forexecab does:
''' the values K-BOT sends are the values the next read-back shows, so the check of the DDF send
''' (section A against forexecab's table) passes by itself.
''' </summary>
''' <remarks>
''' <para><b>The read-back is shaped like the real one</b> (checked against
''' <c>PYTHON/tests/fixtures/prelucrare_AAB37CNBK95.json</c> and the server's import steps): the six
''' header values, the indicator table (<c>TabelIndicatori</c>, as scraped, and
''' <c>TabelIndicatori_results</c>, as collected) and the history (<c>TabelIstoric</c>) with the
''' «Rand contract: ...» texts the server reads, the reason of every change carrying «(REV:n)». It goes
''' through the REAL import on the server, so FX_Rezervari gets its rows and the revision gets
''' «AreRezervari» -- which is what lets section B be written. Use the 000_DEMO database only: the
''' import writes the simulated figures like any others.</para>
''' <para><b>What it does NOT do.</b> It never opens a browser and never reaches forexecab. Any other
''' workflow is refused with a clear message. The captures are generated pictures marked
''' «SIMULARE». The memory lives only until K-BOT closes; every history row keeps the moment it was made,
''' so a second read-back gives the server the same rows again (the import skips what it already has).</para>
''' <para><b>An angajament the simulator has never seen</b> (a real one, downloaded earlier) is created
''' in its memory the first time a workflow names it, with the rows that workflow sends; the read-back
''' then carries only those rows and the changes made in this run.</para>
''' </remarks>
Public NotInheritable Class SimulatedForexeRunner
    Implements IForexeRunner

    ' The workflow names JobBuilder gives (the contract with the controller).
    Private Const WfCreare As String = "CreareAngajament"
    Private Const WfIncarca As String = "IncarcaRezervare"
    Private Const WfDefinitivare As String = "DefinitivareAngajament"
    Private Const WfDerulare As String = "DerulareAngajament"
    Private Const WfRezervari As String = "RezervariAngajament"

    ' The states as forexecab writes them (shown in the tree of K-BOT).
    Private Const StareInitial As String = "Initial"
    Private Const StareInDefinitivare As String = "În definitivare"
    Private Const StareInDerulare As String = "În derulare"

    Private Const Utilizator As String = "00000000 operator simulat 00000000"
    Private Const MomentFormat As String = "dd/MM/yyyy HH:mm:ss"
    Private Const Steps As Integer = 6
    Private Const CodeAlphabet As String = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"

    Private ReadOnly _gate As New Object()
    Private ReadOnly _angajamente As New Dictionary(Of String, SimAngajament)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _rng As New Random()

    ''' <summary>How long one job takes, in seconds.</summary>
    Public Property RunSeconds As Double = 1.5

    Public Event StatusUpdated As EventHandler(Of String) Implements IForexeRunner.StatusUpdated
    Public Event BrowserVisibilityChanged As EventHandler Implements IForexeRunner.BrowserVisibilityChanged
    Public Event OperationCaptured As EventHandler(Of ForexeWatchEvent) Implements IForexeRunner.OperationCaptured

    ''' <summary>Always "connected": there is no login to FOREXE to do.</summary>
    Public ReadOnly Property HasLiveSession As Boolean Implements IForexeRunner.HasLiveSession
        Get
            Return True
        End Get
    End Property

    ' ── The workflows ───────────────────────────────────────────────────────────

    Public Function RunAsync(job As JobRequest, certificate As X509Certificate2,
                             progress As IProgress(Of Integer), ct As CancellationToken) As Task(Of JobResult) _
        Implements IForexeRunner.RunAsync
        Return Task.FromResult(New JobResult With {.Success = True, .Message = "Conectare (simulată)"})
    End Function

    Public Async Function RunJobAsync(job As JobRequest, progress As IProgress(Of Integer),
                                      ct As CancellationToken) As Task(Of JobResult) _
        Implements IForexeRunner.RunJobAsync
        Try
            ArgumentNullException.ThrowIfNull(job)
            Dim k_name As String = If(job.WorkflowName, String.Empty)
            If Not Await PretendAsync(k_name, progress, ct).ConfigureAwait(True) Then
                Return Fail("Anulat de operator (simulat).")
            End If
            Select Case k_name
                Case WfCreare, WfIncarca, WfDefinitivare, WfDerulare
                    ' The dry run: a saving workflow stops right before it would save.
                    If job.StopBeforeSave Then
                        Return New JobResult With {
                            .Success = True, .StoppedBeforeSave = True,
                            .Message = $"Simulare: «{k_name}» s-a oprit înainte de primul pas care salvează."}
                    End If
            End Select
            Select Case k_name
                Case WfCreare : Return DoCreare(job)
                Case WfIncarca : Return DoIncarca(job)
                Case WfDefinitivare : Return DoStare(job, StareInDefinitivare, "Poza_Definitivare")
                Case WfDerulare : Return DoStare(job, StareInDerulare, "Poza_Derulare")
                Case WfRezervari : Return DoRezervari(job)
                Case Else
                    Return Fail($"Workflow-ul «{k_name}» nu este simulat. În modul FOREXE simulat se pot trimite " &
                                "rezervări (creare, încărcare, definitivare, derulare) și se pot citi înapoi.")
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("SimulatedForexeRunner.RunJobAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function RunJobsParallelAsync(jobs As IReadOnlyList(Of JobRequest), maxThreads As Integer,
                                               jobFinished As Action(Of ParallelJobOutcome),
                                               hooks As ParallelRunHooks,
                                               ct As CancellationToken) As Task(Of List(Of ParallelJobOutcome)) _
        Implements IForexeRunner.RunJobsParallelAsync
        Dim k_outcomes As New List(Of ParallelJobOutcome)()
        For Each k_job As JobRequest In If(jobs, New List(Of JobRequest)())
            Dim k_now As Date = Date.Now
            Dim k_outcome As New ParallelJobOutcome With {
                .Job = k_job, .Worker = 0, .StartedAt = k_now, .FinishedAt = k_now,
                .Result = Fail("Descărcarea pe mai multe file nu este simulată.")}
            k_outcomes.Add(k_outcome)
            jobFinished?.Invoke(k_outcome)
        Next
        Await Task.CompletedTask
        Return k_outcomes
    End Function

    Public Function DescarcaExtraseAsync(folderDescarcare As String, dataDeLa As Date?,
                                         progres As Action(Of Integer, Integer, String),
                                         ct As CancellationToken) As Task(Of List(Of ExtrasDescarcat)) _
        Implements IForexeRunner.DescarcaExtraseAsync
        Return Task.FromResult(New List(Of ExtrasDescarcat)())
    End Function

    ' A few short pauses with progress, so the busy bar and the footer behave as in a real run.
    ' False = cancelled. The cancel is expected control flow here, not an error.
    Private Async Function PretendAsync(k_name As String, k_progress As IProgress(Of Integer),
                                        k_ct As CancellationToken) As Task(Of Boolean)
        RaiseEvent StatusUpdated(Me, $"[simulat] {k_name} pornește...")
        Dim k_pause As Integer = CInt(Math.Max(30, RunSeconds * 1000 / Steps))
        For k_step As Integer = 1 To Steps
            If k_ct.IsCancellationRequested Then Return False
            Try
                Await Task.Delay(k_pause, k_ct).ConfigureAwait(True)
            Catch ex As OperationCanceledException
                Return False
            End Try
            k_progress?.Report(k_step * 100 \ Steps)
            RaiseEvent StatusUpdated(Me, $"[simulat] {k_name}: pasul {k_step}/{Steps}")
        Next
        Return True
    End Function

    ' ── Creare Angajament ───────────────────────────────────────────────────────

    Private Function DoCreare(k_job As JobRequest) As JobResult
        Dim k_rows As List(Of Dictionary(Of String, String)) = ParseRows(Param(k_job, WorkflowCatalog.VarDateReceptie))
        If k_rows.Count = 0 Then Return Fail("Simulare: angajamentul nou nu are niciun rând de trimis.")

        Dim k_res As New JobResult With {.Success = True, .Message = WfCreare}
        SyncLock _gate
            Dim k_cod As String = NewAngajamentCode()
            Dim k_ang As New SimAngajament With {
                .Cod = k_cod, .Stare = StareInitial,
                .Descriere = Param(k_job, WorkflowCatalog.VarDescriereAngajament)}
            _angajamente(k_cod) = k_ang

            Dim k_index As Integer = 0
            For Each k_row As Dictionary(Of String, String) In k_rows
                Dim k_val As Double = DdfSendInputs.ParseSuma(Cell(k_row, "CB_INITIAL")).GetValueOrDefault()
                Dim k_new As SimRand = NewRow(k_ang, Cell(k_row, "Sursa"), Cell(k_row, "Clasificatia"), k_val, k_val)
                ' The history in the order forexecab writes it: the first row, the angajament, then each added row.
                k_ang.Note(If(k_index = 0, "Initializare angajament", "adaugare rand"), RandContract(k_new.Cod, Nothing, k_val, Nothing))
                If k_index = 0 Then
                    k_ang.Note("Angajament nou.", $"Contract: {k_cod}: {k_ang.Descriere}, ajustare titlul 85: 0")
                End If
                k_index += 1

                Dim k_cheie As String = Cell(k_row, "Cheie")
                k_res.Data("Poza_" & k_cheie) = Picture("Rând salvat în angajamentul nou", k_ang, $"Rând {k_new.Cod}")
                k_res.Data(WorkflowCatalog.VarCodAngajamentRandPrefix & k_cheie) = k_cod
            Next
            k_res.Data("Poza_Final") = Picture("Angajamentul nou, după ultimul rând", k_ang, String.Empty)
            k_res.Data(WorkflowCatalog.VarCodAngajamentFinal) = k_cod
            PutTable(k_res, WorkflowCatalog.TabelIndicatori, GridScraped(k_ang))
        End SyncLock
        Return k_res
    End Function

    ' ── Incarca Rezervare ───────────────────────────────────────────────────────

    Private Function DoIncarca(k_job As JobRequest) As JobResult
        Dim k_cod As String = Param(k_job, WorkflowCatalog.VarCodAngajament)
        If k_cod.Length = 0 Then Return Fail("Simulare: lipsește codul angajamentului.")
        Dim k_rows As List(Of Dictionary(Of String, String)) = ParseRows(Param(k_job, WorkflowCatalog.VarDateModificare))
        If k_rows.Count = 0 Then Return Fail("Simulare: nu există niciun rând de încărcat.")

        Dim k_res As New JobResult With {.Success = True, .Message = WfIncarca}
        SyncLock _gate
            Dim k_ang As SimAngajament = FindOrCreate(k_cod, StareInDerulare)
            k_res.Data("Poza_Inainte") = Picture("Rezervările înainte de modificare", k_ang, String.Empty)

            For Each k_row As Dictionary(Of String, String) In k_rows
                Dim k_ind As String = Cell(k_row, "Indicator_ang")
                Dim k_ss As String = Cell(k_row, "Sursa")
                Dim k_sal As String = Cell(k_row, "ClsfSal")
                Dim k_val As Double = DdfSendInputs.ParseSuma(Cell(k_row, "ValoareNoua")).GetValueOrDefault()
                Dim k_motiv As String = Cell(k_row, "Motiv")
                Dim k_cheie As String = Cell(k_row, "Cheie")
                Dim k_estenou As Boolean = String.Equals(k_ind, DdfSendInputs.NewRowMarker, StringComparison.Ordinal)

                Dim k_target As SimRand = Nothing
                If Not k_estenou AndAlso k_ind.Length > 0 Then
                    k_target = k_ang.Rows.FirstOrDefault(Function(r) String.Equals(r.Cod, k_ind, StringComparison.OrdinalIgnoreCase))
                End If
                If k_target Is Nothing Then
                    k_target = k_ang.Rows.FirstOrDefault(Function(r) r.ClsfSal = k_sal AndAlso
                        String.Equals(r.Ss, k_ss, StringComparison.OrdinalIgnoreCase))
                End If
                Dim k_adaugat As Boolean = k_target Is Nothing
                If k_adaugat Then
                    ' A row forexecab has not got: a new one is added; an existing code the simulator never saw keeps its code.
                    k_target = NewRow(k_ang, k_ss, Cell(k_row, "Clasificatia"), If(k_estenou, 0.0R, k_val), k_val,
                                      If(k_estenou OrElse k_ind.Length = 0, String.Empty, k_ind))
                Else
                    k_target.Definitiv = k_val
                End If
                ' The reason is the description of the history row: the server reads «(REV:n)» from it.
                k_ang.Note(If(k_motiv.Length > 0, k_motiv, "Modificare rezervare"),
                           RandContract(k_target.Cod, k_val, k_target.Initial, k_val))

                k_res.Data("Poza_" & k_cheie) = Picture("Rând încărcat", k_ang, $"Rând {k_target.Cod} = {DdfSendInputs.FormatSuma(k_val)}")
                If k_adaugat Then k_res.Data(WorkflowCatalog.VarCodIndicatorRandPrefix & k_cheie) = k_target.Cod
            Next
            k_res.Data("Poza_Final") = Picture("Rezervările după modificare", k_ang, String.Empty)
            If String.Equals(Param(k_job, WorkflowCatalog.VarCapturaInfoComplete), "true", StringComparison.OrdinalIgnoreCase) Then
                k_res.Data("Poza_InfoComplete") = Picture("Informații complete contract", k_ang, String.Empty)
            End If
            PutTable(k_res, WorkflowCatalog.TabelIndicatori, GridScraped(k_ang))
        End SyncLock
        Return k_res
    End Function

    ' ── Definitivare / Derulare ─────────────────────────────────────────────────

    Private Function DoStare(k_job As JobRequest, k_stare As String, k_picture As String) As JobResult
        Dim k_cod As String = Param(k_job, WorkflowCatalog.VarCodAngajament)
        If k_cod.Length = 0 Then Return Fail("Simulare: lipsește codul angajamentului.")
        Dim k_definitivare As Boolean = k_picture = "Poza_Definitivare"
        Dim k_motiv As String = Param(k_job, If(k_definitivare, WorkflowCatalog.VarMotivDefinitivare, WorkflowCatalog.VarMotivDerulare))

        Dim k_res As New JobResult With {.Success = True, .Message = k_job.WorkflowName}
        SyncLock _gate
            Dim k_ang As SimAngajament = FindOrCreate(k_cod, k_stare)
            Dim k_inainte As String = k_ang.Stare
            k_ang.Stare = k_stare
            k_ang.Note($"Modificare stare: {k_inainte} -> {k_stare}: DA", String.Empty)
            If k_definitivare Then
                ' Definitivare sets each row's definitive value, with the reason (which carries «(REV:0)»).
                For k_i As Integer = 0 To k_ang.Rows.Count - 1
                    Dim k_r As SimRand = k_ang.Rows(k_i)
                    k_r.Definitiv = k_r.Initial
                    k_ang.Note(If(k_motiv.Length > 0, k_motiv, "Definitivare"), RandContract(k_r.Cod, k_r.Definitiv, k_r.Initial, k_r.Definitiv))
                    k_res.Data($"{k_picture}_{k_i}") = Picture("Indicator completat înainte de salvare", k_ang, $"Rând {k_r.Cod}")
                Next
            Else
                k_ang.DataDerulare = Date.Today
            End If
            k_res.Data(k_picture) = Picture($"Angajament «{k_stare}»", k_ang, String.Empty)
            PutHeader(k_res, k_ang)
            PutTable(k_res, "TabelIndicatoriFinali", GridScraped(k_ang))
        End SyncLock
        Return k_res
    End Function

    ' ── The read-back (what the send checks, and what the import reads) ─────────

    Private Function DoRezervari(k_job As JobRequest) As JobResult
        Dim k_cod As String = Param(k_job, WorkflowCatalog.VarCodAngajament)
        If k_cod.Length = 0 Then Return Fail("Simulare: lipsește codul angajamentului.")
        Dim k_res As New JobResult With {.Success = True, .Message = WfRezervari}
        SyncLock _gate
            Dim k_ang As SimAngajament = Nothing
            If Not _angajamente.TryGetValue(k_cod, k_ang) Then
                ' Never sent in this run of K-BOT: an empty answer, not an invented one.
                k_ang = New SimAngajament With {.Cod = k_cod, .Stare = StareInDerulare}
            End If
            PutHeader(k_res, k_ang)
            PutTable(k_res, WorkflowCatalog.TabelIndicatori, GridScraped(k_ang))
            PutTable(k_res, "TabelIndicatori_results", GridCollected(k_ang))

            Dim k_istoric As New List(Of RandTabel)()
            For Each k_h As SimIstoric In k_ang.History
                k_istoric.Add(Row(
                    ("Timp", k_h.Moment.ToString(MomentFormat, CultureInfo.InvariantCulture)),
                    ("Utilizator", Utilizator),
                    ("Descriere", k_h.Descriere),
                    ("Observatii", k_h.Observatii)))
            Next
            PutTable(k_res, "TabelIstoric", New TabelRezultat(k_istoric))
        End SyncLock
        Return k_res
    End Function

    ' The six header values the workflow reads off the angajament page.
    Private Shared Sub PutHeader(k_res As JobResult, k_ang As SimAngajament)
        k_res.Data("CodAngajament") = k_ang.Cod
        k_res.Data("DataAngajament") = k_ang.Created.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
        k_res.Data("DataInceputDerulare") = If(k_ang.DataDerulare.HasValue,
                                               k_ang.DataDerulare.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), String.Empty)
        k_res.Data("UltimaModificare") = k_ang.LastMoment.ToString(MomentFormat, CultureInfo.InvariantCulture)
        k_res.Data("DescriereAngajament") = k_ang.Descriere
        k_res.Data("StareAngajament") = k_ang.Stare
    End Sub

    ' ── The memory ──────────────────────────────────────────────────────────────

    Private Function FindOrCreate(k_cod As String, k_stare As String) As SimAngajament
        Dim k_ang As SimAngajament = Nothing
        If Not _angajamente.TryGetValue(k_cod, k_ang) Then
            k_ang = New SimAngajament With {.Cod = k_cod, .Stare = k_stare}
            _angajamente(k_cod) = k_ang
        End If
        Return k_ang
    End Function

    Private Function NewAngajamentCode() As String
        Dim k_cod As String
        Do
            k_cod = "SIM" & New String(Enumerable.Range(0, 8).Select(
                Function(i) CodeAlphabet(_rng.Next(CodeAlphabet.Length))).ToArray())
        Loop While _angajamente.ContainsKey(k_cod)
        Return k_cod
    End Function

    ' Row codes are three letters, AAA, AAB, ... unique inside one angajament.
    Private Shared Function NewRow(k_ang As SimAngajament, k_ss As String, k_clsf As String,
                                   k_initial As Double, k_definitiv As Double,
                                   Optional k_code As String = "") As SimRand
        Dim k_final As String = k_code
        If k_final.Length = 0 Then
            Dim k_n As Integer = k_ang.Rows.Count
            Do
                k_final = ChrW(65 + (k_n \ 676) Mod 26) & ChrW(65 + (k_n \ 26) Mod 26) & ChrW(65 + k_n Mod 26)
                k_n += 1
            Loop While k_ang.Rows.Any(Function(r) String.Equals(r.Cod, k_final, StringComparison.OrdinalIgnoreCase))
        End If
        Dim k_row As New SimRand With {
            .Cod = k_final, .Ss = k_ss, .Clsf = k_clsf, .ClsfSal = DdfSendInputs.DigitsAndLetters(k_clsf),
            .Initial = k_initial, .Definitiv = k_definitiv}
        k_ang.Rows.Add(k_row)
        Return k_row
    End Function

    ' «02A- 65. 04. 02. 20. 01. 01»: the cell of forexecab's table, which the server splits on the first dash.
    Private Shared Function SectorSursaIndicator(k_row As SimRand) As String
        Return k_row.Ss & "- " & String.Join(". ", k_row.Clsf.Split("."c))
    End Function

    ' The table as scraped (what K-BOT checks section A against): the credit columns included.
    Private Shared Function GridScraped(k_ang As SimAngajament) As TabelRezultat
        Dim k_list As New List(Of RandTabel)()
        For Each k_r As SimRand In k_ang.Rows
            k_list.Add(Row(
                ("Indicator_ang", k_r.Cod),
                ("Program", "0000000000"),
                ("Obiectiv", "---"),
                ("Proiect", "---"),
                ("Sector_Sursa_Indicator", SectorSursaIndicator(k_r)),
                ("Limita_credit_angajament", DdfSendInputs.FormatSuma(CreditBugetar(k_r))),
                ("Total_credit_angajament", DdfSendInputs.FormatSuma(CreditBugetar(k_r))),
                ("Credit_bugetar", DdfSendInputs.FormatSuma(CreditBugetar(k_r))),
                ("Angajament_legal", DdfSendInputs.FormatSuma(k_r.Definitiv)),
                ("Credit_bugetar_rezervat_initial", DdfSendInputs.FormatSuma(k_r.Initial)),
                ("Credit_bugetar_rezervat_definitiv_an_curent", DdfSendInputs.FormatSuma(k_r.Definitiv)),
                ("Credit_bugetar_rezervat_definitiv_ani_urmatori", "0,00"),
                ("Col_12", String.Empty)))
        Next
        Return New TabelRezultat(k_list)
    End Function

    ' The same rows as the workflow's loop collects them (the table the server reads). The nested
    ' budget table is left out: the server only checks its shape when it is there.
    Private Shared Function GridCollected(k_ang As SimAngajament) As TabelRezultat
        Dim k_list As New List(Of RandTabel)()
        For Each k_r As SimRand In k_ang.Rows
            k_list.Add(Row(
                ("Indicator_ang", k_r.Cod),
                ("Program", "0000000000"),
                ("Obiectiv", "---"),
                ("Proiect", "---"),
                ("Sector_Sursa_Indicator", SectorSursaIndicator(k_r)),
                ("Limita_credit_angajament", DdfSendInputs.FormatSuma(CreditBugetar(k_r))),
                ("Total_credit_angajament", DdfSendInputs.FormatSuma(CreditBugetar(k_r))),
                ("Credit_bugetar", DdfSendInputs.FormatSuma(CreditBugetar(k_r))),
                ("Angajament_legal", DdfSendInputs.FormatSuma(k_r.Definitiv)),
                ("Credit_bugetar_rezervat_definitiv_an_curent", DdfSendInputs.FormatSuma(k_r.Definitiv)),
                ("Credit_bugetar_rezervat_definitiv_ani_urmatori", "0,00"),
                ("Col_12", String.Empty)))
        Next
        Return New TabelRezultat(k_list)
    End Function

    ' The simulator does not know the real budget of a classification, so it says one that covers the row.
    ' The server writes it per classification: another reason to run this on 000_DEMO only.
    Private Shared Function CreditBugetar(k_row As SimRand) As Double
        Return Math.Max(k_row.Initial, k_row.Definitiv) + 100000.0R
    End Function

    ' «Rand contract: AAB, angajament legal: 210, credit de angajament rezervat intial: 210, ...»: the text of a
    ' history row that the server reads with its label parsers. No thousands separator and no comma inside a
    ' number (the comma ends the value); "n/a" where forexecab has no figure yet.
    Private Shared Function RandContract(k_cod As String, k_legal As Double?, k_initial As Double,
                                         k_curent As Double?) As String
        Return $"Rand contract: {k_cod}, angajament legal: {Amount(k_legal)}, " &
               $"credit de angajament rezervat intial: {Amount(k_initial)}, " &
               $"credit bugetar rezervat definitiv: an curent: {Amount(k_curent)}, an+1: n/a, an+2: n/a, an+3: n/a, alti ani: n/a"
    End Function

    Private Shared Function Amount(k_value As Double?) As String
        If Not k_value.HasValue Then Return "n/a"
        Return k_value.Value.ToString("0.##", CultureInfo.InvariantCulture)
    End Function

    Private Shared Sub PutTable(k_res As JobResult, k_key As String, k_table As TabelRezultat)
        k_res.Tables(k_key) = k_table
        k_res.Data(k_key) = $"[{k_table.Count} rânduri]"
    End Sub

    Private Shared Function Row(ParamArray k_cells As (Key As String, Value As String)()) As RandTabel
        Dim k_pairs As New List(Of KeyValuePair(Of String, CelulaTabel))()
        For Each k_c In k_cells
            k_pairs.Add(New KeyValuePair(Of String, CelulaTabel)(k_c.Key, CelulaTabel.DinText(k_c.Value)))
        Next
        Return New RandTabel(k_pairs)
    End Function

    Private Shared Function Fail(k_message As String) As JobResult
        Return New JobResult With {.Success = False, .Message = k_message}
    End Function

    Private Shared Function Param(k_job As JobRequest, k_key As String) As String
        Dim k_value As String = Nothing
        If k_job.Parameters IsNot Nothing AndAlso k_job.Parameters.TryGetValue(k_key, k_value) Then
            Return If(k_value, String.Empty).Trim()
        End If
        Return String.Empty
    End Function

    Private Shared Function Cell(k_row As Dictionary(Of String, String), k_key As String) As String
        Dim k_value As String = Nothing
        If k_row.TryGetValue(k_key, k_value) Then Return If(k_value, String.Empty).Trim()
        Return String.Empty
    End Function

    ' Reached only through RunJobAsync, which logs and rethrows.
    Private Shared Function ParseRows(k_json As String) As List(Of Dictionary(Of String, String))
        If String.IsNullOrWhiteSpace(k_json) Then Return New List(Of Dictionary(Of String, String))()
        Return If(JsonSerializer.Deserialize(Of List(Of Dictionary(Of String, String)))(k_json),
                  New List(Of Dictionary(Of String, String))())
    End Function

    ' ── The captures: pictures of a made-up FOREXE page, marked as simulated ────

    ''' <summary>One JPEG (base64), the way the workflows' <c>Screenshot</c> leaves it in a variable.</summary>
    Private Shared Function Picture(k_title As String, k_ang As SimAngajament, k_note As String) As String
        Dim k_lines As Integer = Math.Max(3, k_ang.Rows.Count)
        Dim k_height As Integer = 190 + 26 * k_lines
        Using k_bmp As New Bitmap(900, k_height)
            Using k_g As Graphics = Graphics.FromImage(k_bmp)
                k_g.Clear(Color.White)
                k_g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
                Using k_band As New SolidBrush(Color.FromArgb(170, 30, 30)),
                      k_ink As New SolidBrush(Color.FromArgb(30, 30, 30)),
                      k_pale As New SolidBrush(Color.White),
                      k_big As New Font("Calibri", 15.0F, FontStyle.Bold),
                      k_mid As New Font("Calibri", 12.0F, FontStyle.Regular),
                      k_bold As New Font("Calibri", 11.0F, FontStyle.Bold)
                    k_g.FillRectangle(k_band, 0, 0, 900, 34)
                    k_g.DrawString("SIMULARE — aceasta nu este o pagină FOREXE reală", k_big, k_pale, 10, 4)
                    k_g.DrawString(k_title, k_big, k_ink, 10, 44)
                    k_g.DrawString($"Angajament {k_ang.Cod} — {k_ang.Stare}", k_mid, k_ink, 10, 74)
                    If k_note.Length > 0 Then k_g.DrawString(k_note, k_mid, k_ink, 10, 98)
                    Dim k_y As Integer = 130
                    k_g.DrawString("Sursa", k_bold, k_ink, 10, k_y)
                    k_g.DrawString("Clasificație", k_bold, k_ink, 90, k_y)
                    k_g.DrawString("Cod", k_bold, k_ink, 430, k_y)
                    k_g.DrawString("CB inițial", k_bold, k_ink, 520, k_y)
                    k_g.DrawString("CB definitiv an curent", k_bold, k_ink, 680, k_y)
                    For Each k_r As SimRand In k_ang.Rows
                        k_y += 26
                        k_g.DrawString(k_r.Ss, k_mid, k_ink, 10, k_y)
                        k_g.DrawString(k_r.Clsf, k_mid, k_ink, 90, k_y)
                        k_g.DrawString(k_r.Cod, k_mid, k_ink, 430, k_y)
                        k_g.DrawString(DdfSendInputs.FormatSuma(k_r.Initial), k_mid, k_ink, 520, k_y)
                        k_g.DrawString(DdfSendInputs.FormatSuma(k_r.Definitiv), k_mid, k_ink, 680, k_y)
                    Next
                End Using
            End Using
            Using k_ms As New MemoryStream()
                k_bmp.Save(k_ms, ImageFormat.Jpeg)
                Return Convert.ToBase64String(k_ms.ToArray())
            End Using
        End Using
    End Function

    ' ── Browser / page members: there is no browser, so these are quiet ─────────

    Public Function ShowBrowserAsync(owner As IWin32Window) As Task Implements IForexeRunner.ShowBrowserAsync
        Return Task.CompletedTask
    End Function

    Public Function HideBrowserAsync() As Task Implements IForexeRunner.HideBrowserAsync
        Return Task.CompletedTask
    End Function

    Public ReadOnly Property IsBrowserVisible As Boolean Implements IForexeRunner.IsBrowserVisible
        Get
            Return False
        End Get
    End Property

    Public Function DockBrowserAsync(host As Control) As Task Implements IForexeRunner.DockBrowserAsync
        Return Task.CompletedTask
    End Function

    Public Function ReleaseBrowserAsync(host As Control) As Task Implements IForexeRunner.ReleaseBrowserAsync
        Return Task.CompletedTask
    End Function

    Public Function SyncBrowserBoundsAsync() As Task Implements IForexeRunner.SyncBrowserBoundsAsync
        Return Task.CompletedTask
    End Function

    Public ReadOnly Property BrowserHost As Control Implements IForexeRunner.BrowserHost
        Get
            Return Nothing
        End Get
    End Property

    Public Function ReadPageAngajamentAsync() As Task(Of String) Implements IForexeRunner.ReadPageAngajamentAsync
        Return Task.FromResult(String.Empty)
    End Function

    Public Function ReadUncorrectedOperationsAsync() As Task(Of String) Implements IForexeRunner.ReadUncorrectedOperationsAsync
        Return Task.FromResult("{""found"":false,""pages"":0,""rows"":[]}")
    End Function

    Public Function ApplyPageConfigAsync() As Task Implements IForexeRunner.ApplyPageConfigAsync
        Return Task.CompletedTask
    End Function

    Public Function ReadPageElementsAsync() As Task(Of String) Implements IForexeRunner.ReadPageElementsAsync
        Return Task.FromResult("[]")
    End Function

    Public Function HighlightPageElementAsync(index As Integer) As Task(Of String) Implements IForexeRunner.HighlightPageElementAsync
        Return Task.FromResult(String.Empty)
    End Function

    Public Function CapturePaginaAsync(paginaOriginala As Boolean) As Task(Of Byte()) Implements IForexeRunner.CapturePaginaAsync
        Return Task.FromResult(Of Byte())(Nothing)
    End Function

    Public Function CaptureInfoCompleteAsync(paginaOriginala As Boolean) As Task(Of Byte()) Implements IForexeRunner.CaptureInfoCompleteAsync
        Return Task.FromResult(Of Byte())(Nothing)
    End Function

    Public Sub SetCapturaProvider(provider As Func(Of String, CancellationToken, Task(Of Boolean))) Implements IForexeRunner.SetCapturaProvider
    End Sub

    Public Function ResetShotSessionAsync() As Task Implements IForexeRunner.ResetShotSessionAsync
        Return Task.CompletedTask
    End Function

    Public Sub ShowRecorder(owner As IWin32Window) Implements IForexeRunner.ShowRecorder
    End Sub

    ' ── What the simulator remembers ────────────────────────────────────────────

    Private NotInheritable Class SimRand
        Public Property Cod As String = String.Empty
        Public Property Ss As String = String.Empty
        ''' <summary>Dotted, in forexecab's spelling («65.04.02.20.01.01»).</summary>
        Public Property Clsf As String = String.Empty
        ''' <summary>The same without separators, upper case.</summary>
        Public Property ClsfSal As String = String.Empty
        Public Property Initial As Double
        Public Property Definitiv As Double
    End Class

    ''' <summary>One history row, with the moment it was made (fixed: a second read-back must repeat it exactly).</summary>
    Private NotInheritable Class SimIstoric
        Public Property Moment As Date
        Public Property Descriere As String = String.Empty
        Public Property Observatii As String = String.Empty
    End Class

    Private NotInheritable Class SimAngajament
        Public Property Cod As String = String.Empty
        Public Property Stare As String = String.Empty
        Public Property Descriere As String = String.Empty
        Public Property Created As Date = Date.Today
        Public Property DataDerulare As Date?
        Public ReadOnly Property Rows As New List(Of SimRand)()
        Public ReadOnly Property History As New List(Of SimIstoric)()

        ''' <summary>The moment of the last history row, or now when there is none.</summary>
        Public ReadOnly Property LastMoment As Date
            Get
                Return If(History.Count = 0, Date.Now, History(History.Count - 1).Moment)
            End Get
        End Property

        ''' <summary>Adds a history row stamped now, to the second (the unit of forexecab's «Timp» column).</summary>
        Public Sub Note(k_descriere As String, k_observatii As String)
            Dim k_now As Date = Date.Now
            History.Add(New SimIstoric With {
                .Moment = k_now.AddTicks(-(k_now.Ticks Mod TimeSpan.TicksPerSecond)),
                .Descriere = k_descriere, .Observatii = k_observatii})
        End Sub
    End Class
End Class
#End If
