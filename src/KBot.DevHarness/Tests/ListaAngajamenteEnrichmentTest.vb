Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Forexe

' Offline test: the raw JSON-array string a .wfl leaves in the executor variable
' "ListaAngajamente" must parse into JobResult.Tables rows with the exact keys
' Cod / Descriere / Stare. Exercises ForexeRunner.TryParseTable — the same seam
' PopulateResult uses after RunJobAsync (verified §0.2).
Public NotInheritable Class ListaAngajamenteEnrichmentTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "FOREXE — ListaAngajamente: raw JSON -> Tables() keys Cod/Descriere/Stare"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Forexe"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return False
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return False
        End Get
    End Property

    ' Real captured payload (subset of the live 42-row scrape). Full set can be pasted if
    ' exhaustive coverage is wanted.
    Private Const RAW_JSON As String =
        "[" &
        "{""Nr_crt"":""1."",""Cod"":""AAB5AF3PCM4"",""Descriere"":""LIBRARIE NET SRL - 2026"",""Stare"":""În derulare"",""Angajament_legal"":""2.936,78"",""Credit_bugetar_rezervat_definitiv_in_anul_curent"":""2.936,78"",""Receptii"":""2.936,78"",""Plati"":""2.936,78"",""Col_9"":""""}," &
        "{""Nr_crt"":""26."",""Cod"":""AAB2SC2HFAR"",""Descriere"":""PEO - 2026"",""Stare"":""În derulare"",""Angajament_legal"":""840.838,26"",""Credit_bugetar_rezervat_definitiv_in_anul_curent"":""840.838,26"",""Receptii"":""840.838,26"",""Plati"":""824.928,42"",""Col_9"":""""}," &
        "{""Nr_crt"":""27."",""Cod"":""AAB2N372B2E"",""Descriere"":""Deplasari interne - 2026"",""Stare"":""În derulare"",""Angajament_legal"":""0,00"",""Credit_bugetar_rezervat_definitiv_in_anul_curent"":""0,00"",""Receptii"":""0,00"",""Plati"":""0,00"",""Col_9"":""""}" &
        "]"

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) _
        As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync

        ' ACT: the real parser PopulateResult uses to build Tables("ListaAngajamente").
        Dim table As List(Of Dictionary(Of String, String)) = ForexeRunner.TryParseTable(RAW_JSON)
        If table Is Nothing Then
            ' TryParseTable classifies non-tables as Nothing; for this payload that is a failure.
            Return Task.FromResult(HarnessTestResult.Failed(
                "TryParseTable returned Nothing — the raw payload was not recognized as a JSON array of objects."))
        End If

        If table.Count <> 3 Then
            Return Task.FromResult(HarnessTestResult.Failed($"Expected 3 parsed rows, got {table.Count}."))
        End If

        Dim first As Dictionary(Of String, String) = table(0)
        For Each requiredKey In New String() {"Cod", "Descriere", "Stare"}
            If Not first.ContainsKey(requiredKey) Then
                Return Task.FromResult(HarnessTestResult.Failed(
                    $"Missing key '{requiredKey}'. Actual keys: {String.Join(",", first.Keys)}"))
            End If
        Next
        If first("Cod") <> "AAB5AF3PCM4" Then
            Return Task.FromResult(HarnessTestResult.Failed($"First Cod wrong: '{first("Cod")}'."))
        End If

        Return Task.FromResult(HarnessTestResult.Passed(
            $"Parsed {table.Count} rows; keys Cod/Descriere/Stare present; first Cod correct."))
    End Function
End Class
