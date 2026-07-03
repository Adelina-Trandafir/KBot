Option Strict On
Imports System.Collections.Generic
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Domain

' Offline test (no session): real-shaped 9-column FOREXE rows through
' AngajamentMapper + upsert request serialization. Verifies:
'   1) extra scraped columns are ignored; 2) empty-Cod rows are skipped;
'   3) a renamed required column throws (loud drift detection);
'   4) JSON wire keys are EXACTLY db_name / rows / Cod / Descriere / Stare.
Public NotInheritable Class ListaAngajamenteMapperTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "FOREXE — ListaAngajamente: mapper (real columns) + upsert serialization"
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

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) _
        As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync

        ' Real-shaped rows: all 9 live columns present.
        Dim rows As New List(Of Dictionary(Of String, String)) From {
            NewRow("1.", "AAB5AF3PCM4", "LIBRARIE NET SRL - 2026", "În derulare", "2.936,78", "2.936,78"),
            NewRow("26.", "AAB2SC2HFAR", "PEO - 2026", "În derulare", "840.838,26", "824.928,42"),
            NewRow("", "", "empty-cod row - must be skipped", "x", "0,00", "0,00")
        }

        Dim mapped As List(Of Angajament) = AngajamentMapper.FromListaAngajamenteResult(rows)

        If mapped.Count <> 2 Then
            Return Task.FromResult(HarnessTestResult.Failed(
                $"Expected 2 rows (empty-Cod skipped), got {mapped.Count}."))
        End If
        If mapped(0).CodAngajament <> "AAB5AF3PCM4" OrElse mapped(1).CodAngajament <> "AAB2SC2HFAR" Then
            Return Task.FromResult(HarnessTestResult.Failed(
                $"Wrong Cod mapping: '{mapped(0).CodAngajament}', '{mapped(1).CodAngajament}'."))
        End If
        If mapped(0).Descriere <> "LIBRARIE NET SRL - 2026" OrElse mapped(0).Stare <> "În derulare" Then
            Return Task.FromResult(HarnessTestResult.Failed(
                $"Wrong Descriere/Stare mapping: '{mapped(0).Descriere}' / '{mapped(0).Stare}'."))
        End If

        ' Renamed Cod column must throw (loud), not blank silently.
        Dim renamed As New List(Of Dictionary(Of String, String)) From {
            New Dictionary(Of String, String) From {
                {"Cod_Angajament", "AAB5AF3PCM4"}, {"Descriere", "x"}, {"Stare", "y"}}
        }
        Dim threw As Boolean = False
        Try
            AngajamentMapper.FromListaAngajamenteResult(renamed)
        Catch ex As KeyNotFoundException
            threw = True
        End Try
        If Not threw Then
            Return Task.FromResult(HarnessTestResult.Failed(
                "Renamed Cod column did NOT throw — drift would pass silently."))
        End If

        ' Wire serialization: exactly db_name/rows/Cod/Descriere/Stare, no CodAngajament.
        Dim req As New UpsertAngajamenteRequest() With {.db_name = "000_DEMO"}
        For Each a In mapped
            req.rows.Add(New AngajamentRow() With {.Cod = a.CodAngajament, .Descriere = a.Descriere, .Stare = a.Stare})
        Next
        Dim opts As New JsonSerializerOptions With {.PropertyNamingPolicy = Nothing}
        Dim json As String = JsonSerializer.Serialize(req, opts)

        For Each expectedKey In New String() {"""db_name""", """rows""", """Cod""", """Descriere""", """Stare"""}
            If Not json.Contains(expectedKey) Then
                Return Task.FromResult(HarnessTestResult.Failed($"Missing JSON key: {expectedKey}.", json))
            End If
        Next
        If json.Contains("CodAngajament") Then
            Return Task.FromResult(HarnessTestResult.Failed(
                "JSON contains 'CodAngajament' — contract requires 'Cod'.", json))
        End If

        Return Task.FromResult(HarnessTestResult.Passed(
            $"2 rows mapped from 9-column input (extras ignored); rename throws; JSON keys correct. {json}"))
    End Function

    ' Builds a row with all 9 live FOREXE columns. The mapper must use only Cod/Descriere/Stare.
    Private Shared Function NewRow(nrCrt As String, cod As String, descriere As String,
                                   stare As String, angLegal As String, plati As String) _
                                   As Dictionary(Of String, String)
        Return New Dictionary(Of String, String) From {
            {"Nr_crt", nrCrt},
            {"Cod", cod},
            {"Descriere", descriere},
            {"Stare", stare},
            {"Angajament_legal", angLegal},
            {"Credit_bugetar_rezervat_definitiv_in_anul_curent", angLegal},
            {"Receptii", angLegal},
            {"Plati", plati},
            {"Col_9", ""}
        }
    End Function
End Class
