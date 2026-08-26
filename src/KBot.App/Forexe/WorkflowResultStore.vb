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
    Private ReadOnly _ultimulNod As New Dictionary(Of String, PrelucrareRezultat)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Ultima listă de angajamente descărcată (mapată). Nothing = nicio descărcare.</summary>
    Public ReadOnly Property UltimaLista As IReadOnlyList(Of Angajament)
        Get
            Return _ultimaLista
        End Get
    End Property

    ''' <summary>Ultimul rezultat de prelucrare pentru un cod (Nothing = nedescărcat).</summary>
    Public Function RezultatNod(cod As String) As PrelucrareRezultat
        If String.IsNullOrEmpty(cod) Then Return Nothing
        Dim rezultat As PrelucrareRezultat = Nothing
        _ultimulNod.TryGetValue(cod, rezultat)
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
