Imports System.Collections.Generic
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Domain

' Test offline (fără sesiune): trece un rezultat ListaAngajamente fals prin
' AngajamentMapper + serializează cererea de upsert și verifică:
'   1) rândul cu Cod gol este eliminat;
'   2) cheile JSON sunt EXACT db_name / Cod / Descriere / Stare (contractul rutei);
'   3) Stare se reîmprospătează (rândul "existent-cu-Stare-schimbată" ajunge cu noua stare).
Public NotInheritable Class ListaAngajamenteMapperTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "FOREXE — ListaAngajamente: mapper + serializare upsert"
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

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        ' Rezultat tabelar fals: un rând cu Cod gol, unul nou, unul cu Stare schimbată.
        Dim table As New List(Of Dictionary(Of String, String)) From {
            New Dictionary(Of String, String) From {{"Cod", "   "}, {"Descriere", "gol - ignorat"}, {"Stare", "x"}},
            New Dictionary(Of String, String) From {{"Cod", "A1"}, {"Descriere", "Angajament nou"}, {"Stare", "Nou"}},
            New Dictionary(Of String, String) From {{"Cod", "A2"}, {"Descriere", "Angajament existent"}, {"Stare", "Definitivat"}}
        }

        Dim mapped As List(Of Angajament) = AngajamentMapper.FromListaAngajamenteResult(table)

        If mapped.Count <> 2 Then
            Return Task.FromResult(HarnessTestResult.Failed(
                $"Așteptam 2 angajamente (Cod gol eliminat), am primit {mapped.Count}."))
        End If
        If mapped(0).CodAngajament <> "A1" OrElse mapped(1).CodAngajament <> "A2" Then
            Return Task.FromResult(HarnessTestResult.Failed(
                $"Coduri mapate greșit: '{mapped(0).CodAngajament}', '{mapped(1).CodAngajament}'."))
        End If
        If mapped(1).Stare <> "Definitivat" Then
            Return Task.FromResult(HarnessTestResult.Failed(
                $"Stare mapată greșit pentru A2: '{mapped(1).Stare}'."))
        End If

        ' Construim cererea de wire exact ca ApiClient.UpsertAngajamenteAsync.
        Dim req As New UpsertAngajamenteRequest() With {.db_name = "018_GRRS"}
        For Each a In mapped
            req.rows.Add(New AngajamentRow() With {.Cod = a.CodAngajament, .Descriere = a.Descriere, .Stare = a.Stare})
        Next

        Dim opts As New JsonSerializerOptions With {.PropertyNamingPolicy = Nothing}
        Dim json As String = JsonSerializer.Serialize(req, opts)

        For Each expectedKey In New String() {"""db_name""", """rows""", """Cod""", """Descriere""", """Stare"""}
            If Not json.Contains(expectedKey) Then
                Return Task.FromResult(HarnessTestResult.Failed(
                    $"Cheie JSON lipsă: {expectedKey}.", json))
            End If
        Next
        ' Numele de domeniu NU trebuie să apară în wire (contractul e Cod, nu CodAngajament).
        If json.Contains("CodAngajament") Then
            Return Task.FromResult(HarnessTestResult.Failed(
                "JSON conține 'CodAngajament' — contractul cere 'Cod'.", json))
        End If

        Return Task.FromResult(HarnessTestResult.Passed(
            $"2 angajamente mapate (Cod gol eliminat); chei JSON corecte. {json}"))
    End Function
End Class
