Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Unicode
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe

''' <summary>
''' Depozitul LOCAL al rezultatelor descărcate din FOREXE (felia 0034). Atât — în această
''' felie nu se scrie nimic pe server pe căile noi: rezultatele stau în memorie (pentru
''' inspecție imediată) și pe disc, ca JSON cu marcaj de timp, în folderul
''' <c>WorkflowResults</c> de lângă executabil.
'''
''' <para>Lista de angajamente se păstrează în FORMA MAPATĂ (<see cref="Angajament"/>) —
''' exact forma pe care o va cere viitorul upsert în MariaDB. Rezultatul unui nod se
''' păstrează BRUT: pentru «Prelucrare Completa» nu există încă un mapper de ingestie, iar
''' unul scris pe nevăzute ar fi o invenție. Operatorul se uită întâi la coloanele reale.</para>
''' </summary>
Public NotInheritable Class WorkflowResultStore

    ''' <summary>Numele folderului de ieșire, lângă executabil.</summary>
    Public Const OutputFolderName As String = "WorkflowResults"

    ' Diacritice LITERALE în fișier (regula casei): fără encoder-ul relaxat peste latina
    ' extinsă, System.Text.Json ar scrie „ș". Fișierele astea sunt menite citirii de om.
    Private Shared ReadOnly _jsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True,
        .Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement,
                                            UnicodeRanges.LatinExtendedA, UnicodeRanges.LatinExtendedB)
    }

    ' ── Starea în memorie ────────────────────────────────────────────────
    Private _ultimaLista As IReadOnlyList(Of Angajament)
    Private _momentLista As Date?
    Private ReadOnly _ultimulNod As New Dictionary(Of String, PrelucrareRezultat)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Ultima listă de angajamente descărcată (mapată). Nothing = nicio descărcare.</summary>
    Public ReadOnly Property UltimaLista As IReadOnlyList(Of Angajament)
        Get
            Return _ultimaLista
        End Get
    End Property

    ''' <summary>
    ''' When the list in <see cref="UltimaLista"/> was downloaded. Nothing = no download in
    ''' this session.
    ''' </summary>
    ''' <remarks>
    ''' Kept because it is the very question the operator asks before deciding whether to reuse
    ''' what is in memory: "how old is this?". A node package carries its own moment
    ''' (<see cref="PrelucrareRezultat.Moment"/>); the list has nowhere to carry one, so here.
    ''' </remarks>
    Public ReadOnly Property MomentLista As Date?
        Get
            Return _momentLista
        End Get
    End Property

    ''' <summary>Ultimul rezultat de prelucrare pentru un cod (Nothing = nedescărcat).</summary>
    Public Function RezultatNod(cod As String) As PrelucrareRezultat
        If String.IsNullOrEmpty(cod) Then Return Nothing
        Dim rezultat As PrelucrareRezultat = Nothing
        _ultimulNod.TryGetValue(cod, rezultat)
        Return rezultat
    End Function

    ''' <summary>
    ''' How many rows a processing package holds in all, across every table.
    ''' </summary>
    ''' <remarks>
    ''' ROWS, not tables: the workflow returns its five tables even when all of them are empty,
    ''' so <c>Tabele.Count</c> would answer 5 for a package with nothing in it. The same
    ''' arithmetic is in <c>ForexeController.DownloadNodeAsync</c> and in
    ''' <c>MainForm.DuLaIngestieAsync</c>; it lives here as a function because it decides
    ''' whether a package in memory is good enough to be offered instead of a fresh download.
    ''' </remarks>
    Public Shared Function NumaraRanduri(rezultat As PrelucrareRezultat) As Integer
        If rezultat Is Nothing OrElse rezultat.Tabele Is Nothing Then Return 0
        Return rezultat.Tabele.Values.Sum(Function(t) If(t Is Nothing, 0, t.Count))
    End Function

    ''' <summary>
    ''' The in-memory package for <paramref name="cod"/>, BUT only when it is fit to reuse: it
    ''' exists and holds at least one row. Nothing in every other case.
    ''' </summary>
    ''' <remarks>
    ''' "Only if they were downloaded correctly" (operator, 09.09.2026). The store receives a
    ''' package only after the robot reported success — a failed download never reaches here —
    ''' so the one check still missing is the one the ingest itself makes before anything goes
    ''' to the server: a package without a single row is not a download, it is an empty one, and
    ''' offering it instead of a fresh download would close the operator into a circle.
    ''' </remarks>
    Public Function PachetBunDeRefolosit(cod As String) As PrelucrareRezultat
        Dim rezultat As PrelucrareRezultat = RezultatNod(cod)
        If rezultat Is Nothing Then Return Nothing
        If NumaraRanduri(rezultat) = 0 Then Return Nothing
        Return rezultat
    End Function

    ''' <summary>Folderul de ieșire (creat la nevoie).</summary>
    Public Shared ReadOnly Property OutputFolder As String
        Get
            Return KBotPaths.FolderRezultateWorkflow
        End Get
    End Property

    ''' <summary>
    ''' Păstrează lista mapată și o scrie ca JSON. Întoarce calea fișierului scris.
    ''' </summary>
    Public Function SalveazaLista(randuri As IReadOnlyList(Of Angajament)) As String
        Try
            ArgumentNullException.ThrowIfNull(randuri)
            _ultimaLista = randuri
            _momentLista = DateTime.Now
            Dim cale As String = Path.Combine(OutputFolder,
                                              $"ListaAngajamente_{DateTime.Now:yyyyMMdd_HHmmss}.json")
            Scrie(cale, randuri)
            Return cale
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowResultStore.SalveazaLista", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Păstrează rezultatul brut al unei prelucrări complete și îl scrie ca JSON.
    ''' Întoarce calea fișierului scris.
    ''' </summary>
    Public Function SalveazaNod(cod As String, rezultat As PrelucrareRezultat) As String
        Try
            If String.IsNullOrWhiteSpace(cod) Then
                Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(cod))
            End If
            ArgumentNullException.ThrowIfNull(rezultat)

            _ultimulNod(cod) = rezultat
            Dim cale As String = Path.Combine(OutputFolder,
                                              $"PrelucrareCompleta_{CodSigur(cod)}_{DateTime.Now:yyyyMMdd_HHmmss}.json")
            Scrie(cale, rezultat)
            Return cale
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowResultStore.SalveazaNod", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Sparge un <see cref="JobResult"/> de prelucrare completă în forma păstrată: tabelele
    ''' aşa cum le-a rupt runner-ul, plus DOAR variabilele scalare. Cheile care au devenit
    ''' tabele se sar din <c>Data</c> — altfel fiecare tabel ar fi scris de două ori: o dată
    ''' structurat şi o dată ca şirul JSON brut din care a fost parsat.
    ''' </summary>
    Public Shared Function DinJobResult(cod As String, rezultat As JobResult) As PrelucrareRezultat
        Try
            ArgumentNullException.ThrowIfNull(rezultat)

            Dim scalari As New Dictionary(Of String, String)(StringComparer.Ordinal)
            For Each kvp In rezultat.Data
                If Not rezultat.Tables.ContainsKey(kvp.Key) Then scalari(kvp.Key) = kvp.Value
            Next

            Return New PrelucrareRezultat With {
                .CodAngajament = If(cod, String.Empty),
                .Moment = DateTime.Now,
                .Workflow = If(rezultat.Message, String.Empty),
                .Scalari = scalari,
                .Tabele = New Dictionary(Of String, TabelRezultat)(rezultat.Tables)
            }
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowResultStore.DinJobResult", ex)
            Throw
        End Try
    End Function

    ' Scrierea propriu-zisă. Frontieră de I/O: logăm și rearuncăm (regula casei) — un
    ' rezultat descărcat dar nesalvat NU are voie să treacă drept salvat.
    Private Shared Sub Scrie(cale As String, continut As Object)
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(cale))
            File.WriteAllText(cale, JsonSerializer.Serialize(continut, _jsonOptions), Text.Encoding.UTF8)
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowResultStore.Scrie", ex)
            Throw
        End Try
    End Sub

    ' Codul angajamentului intră într-un NUME DE FIȘIER: scoatem tot ce Windows refuză.
    Private Shared Function CodSigur(cod As String) As String
        Dim rau As Char() = Path.GetInvalidFileNameChars()
        Return New String(cod.Where(Function(c) Not rau.Contains(c)).ToArray())
    End Function

End Class

' PrelucrareRezultat s-a mutat in KBot.Domain (felia 0048-02): KBot.Api il are nevoie
' ca sa compuna cererea POST /api/forexe/prelucrare si nu poate referi KBot.App.
