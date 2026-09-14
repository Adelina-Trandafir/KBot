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

    ''' <summary>
    ''' Pastreaza un pachet PARTIAL (felia 0060): il scrie ca JSON, dar NU il pune in memoria
    ''' din care se ofera reutilizarea. Intoarce calea fisierului scris.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>De ce nu intra in <c>_ultimulNod</c>.</b> Acolo sta raspunsul la intrebarea
    ''' «angajamentul asta a fost deja descarcat?», iar <see cref="PachetBunDeRefolosit"/> il
    ''' ofera la urmatoarea apasare a iconitei de nod ca pe o descarcare INTREAGA. O
    ''' reimprospatare doar pe receptii n-are nici indicatori, nici istoric; oferita in locul
    ''' unei descarcari complete, ea ar duce ingestia sa creada ca angajamentul nu mai are
    ''' istoric deloc. Deci se scrie pe disc, unde e de folos la citit, si nu se raspunde
    ''' niciodata cu ea.</para>
    ''' </remarks>
    ''' <param name="eticheta">Prefixul numelui de fisier — familia reimprospatata.</param>
    Public Function SalveazaPartial(cod As String, rezultat As PrelucrareRezultat,
                                    eticheta As String) As String
        Try
            If String.IsNullOrWhiteSpace(cod) Then
                Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(cod))
            End If
            ArgumentNullException.ThrowIfNull(rezultat)
            If String.IsNullOrWhiteSpace(eticheta) Then
                Throw New ArgumentException("Eticheta este obligatorie.", NameOf(eticheta))
            End If

            Dim cale As String = Path.Combine(
                OutputFolder,
                $"{eticheta}_{CodSigur(cod)}_{DateTime.Now:yyyyMMdd_HHmmss}.json")
            Scrie(cale, rezultat)
            Return cale
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowResultStore.SalveazaPartial", ex)
            Throw
        End Try
    End Function

    ''' <summary>Numele coloanei de data din «ListaReceptii», asa cum o scrie site-ul.</summary>
    Private Const COL_DATA_RECEPTIE As String = "Data"

    ''' <summary>Numele tabelului de receptii din pachet.</summary>
    Private Const TABEL_RECEPTII As String = "ListaReceptii"

    ''' <summary>
    ''' ACELASI pachet, dar fara randurile receptiilor pe care operatorul nu le-a bifat (felia
    ''' 0060). Un pachet din care nu e nimic de scos se intoarce NESCHIMBAT, aceeasi instanta.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>De ce se taie AICI si nu doar in .wfl.</b> Workflow-ul sare peste DETALIUL unei
    ''' receptii nebifate — acolo sunt minutele — dar randul ei fusese deja citit de
    ''' <c>ScrapeTable</c> si ar pleca spre server cu suma proaspata de pe site si cu detaliul
    ''' gol. Serverul ar actualiza atunci antetul (<c>_R_UPDATE_SUMA_SQL</c>) fara sa-i atinga
    ''' liniile, adica ar lasa in baza o receptie a carei suma nu mai da suma liniilor ei. Un
    ''' rand SCOS nu e atins de nimeni: pasul 4b lucreaza rand cu rand.</para>
    ''' <para><b>Un rand a carui data NU se poate citi RAMANE.</b> «Nu stiu care e» nu are voie
    ''' sa devina «sigur nu e bifata» — asta ar scoate tacut din descarcare tocmai o receptie
    ''' pe care operatorul a cerut-o.</para>
    ''' </remarks>
    Public Shared Function FaraReceptiileSarite(rezultat As PrelucrareRezultat,
                                                dateSarite As IEnumerable(Of Date)) As PrelucrareRezultat
        Try
            If rezultat Is Nothing OrElse rezultat.Tabele Is Nothing Then Return rezultat
            If dateSarite Is Nothing Then Return rezultat

            Dim desarit As New HashSet(Of Date)(dateSarite.Select(Function(d) d.Date))
            If desarit.Count = 0 Then Return rezultat

            Dim receptii As TabelRezultat = Nothing
            If Not rezultat.Tabele.TryGetValue(TABEL_RECEPTII, receptii) OrElse receptii Is Nothing Then
                Return rezultat
            End If

            Dim pastrate As New TabelRezultat()
            For Each rand As RandTabel In receptii
                If Not EDeSarit(rand, desarit) Then pastrate.Adauga(rand)
            Next
            If pastrate.Count = receptii.Count Then Return rezultat

            Dim tabele As New Dictionary(Of String, TabelRezultat)(rezultat.Tabele)
            tabele(TABEL_RECEPTII) = pastrate
            Return New PrelucrareRezultat With {
                .CodAngajament = rezultat.CodAngajament,
                .Moment = rezultat.Moment,
                .Workflow = rezultat.Workflow,
                .Scalari = rezultat.Scalari,
                .Tabele = tabele
            }
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowResultStore.FaraReceptiileSarite", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Randul asta e al unei receptii nebifate? Data se citeste EXACT cum o scrie site-ul
    ''' (<c>zz/ll/aaaa</c>, <see cref="WorkflowCatalog.DataReceptieFormat"/>) — acelasi format
    ''' pe care il cere si serverul in <c>fx_receptii_parse_ro_date</c>. O celula care nu se
    ''' poate citi intoarce False, deci randul ramane.
    ''' </summary>
    Private Shared Function EDeSarit(rand As RandTabel, desarit As HashSet(Of Date)) As Boolean
        If rand Is Nothing Then Return False
        Dim celula As CelulaTabel = Nothing
        If Not rand.TryGetValue(COL_DATA_RECEPTIE, celula) OrElse celula Is Nothing Then Return False
        Dim text As String = celula.TextSau(String.Empty).Trim()
        If text = String.Empty Then Return False
        Dim data As Date
        If Not Date.TryParseExact(text, WorkflowCatalog.DataReceptieFormat,
                                  Globalization.CultureInfo.InvariantCulture,
                                  Globalization.DateTimeStyles.None, data) Then Return False
        Return desarit.Contains(data.Date)
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
