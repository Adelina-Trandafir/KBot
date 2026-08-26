Option Strict On
Imports System.Collections.Generic
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Text.Json.Serialization

''' <summary>
''' Rezultatul BRUT al unei prelucrări complete FOREXE, în forma în care se salvează pe
''' disc și în care se trimite la <c>POST /api/forexe/prelucrare</c>: cele cinci tabele
''' (vezi <c>WorkflowCatalog.PrelucrareCompletaTables</c>) plus scalarii citiți din
''' antetul angajamentului. POCO — fără logică, deci fără Try/Catch (regula casei).
''' </summary>
''' <remarks>
''' A stat până la felia 0048-02 în <c>KBot.App\Forexe\WorkflowResultStore.vb</c>. S-a
''' mutat aici fiindcă <c>KBot.Api</c> are nevoie de el ca să compună cererea de ingestie,
''' iar <c>KBot.Api</c> nu poate referi <c>KBot.App</c> (ar fi o referință inversă).
''' </remarks>
Public NotInheritable Class PrelucrareRezultat
    Public Property CodAngajament As String = String.Empty
    Public Property Moment As DateTime
    ''' <summary>Mesajul workflow-ului care l-a produs (completă vs. REVERSE).</summary>
    Public Property Workflow As String = String.Empty
    Public Property Scalari As New Dictionary(Of String, String)

    ''' <summary>
    ''' Tabelele, rând cu rând, celulă cu celulă — în forma în care le-a produs site-ul.
    ''' Celula e <see cref="CelulaTabel"/> (text, listă sau obiect, recursiv) din 26.08.2026,
    ''' decizia D-N: structura călătorește până la server, nu se aplatizează în text pe drum.
    ''' </summary>
    ''' <remarks>
    ''' <see cref="JsonIgnoreAttribute"/> fiindcă System.Text.Json nu știe să scrie
    ''' <see cref="CelulaTabel"/> singur, iar un <c>JsonConverter</c> NU se poate scrie în
    ''' VB.NET (<c>Utf8JsonReader</c> e <c>ref struct</c>, <c>BC30668</c>). Serializarea trece
    ''' prin <see cref="TabeleSerializate"/>, care e chiar aceleași date ca un arbore
    ''' <see cref="JsonNode"/>.
    ''' </remarks>
    <JsonIgnore>
    Public Property Tabele As New Dictionary(Of String, TabelRezultat)

    ''' <summary>
    ''' Puntea de serializare a lui <see cref="Tabele"/>: ACELEAȘI date, ca arbore JSON.
    ''' Numele pe disc și pe fir rămâne <c>Tabele</c>, deci fișierele scrise înainte de
    ''' 26.08.2026 se citesc mai departe — o celulă care era text rămâne text, iar una care
    ''' era un șir cu JSON rămâne, corect, un șir: fișierul chiar asta conținea.
    ''' </summary>
    <JsonPropertyName("Tabele")>
    Public Property TabeleSerializate As JsonObject
        Get
            Return TabeleJson.Catre(Tabele)
        End Get
        Set(value As JsonObject)
            Tabele = TabeleJson.Din(value)
        End Set
    End Property

    ''' <summary>
    ''' Tot pachetul ca arbore indentat, pentru citit în fereastra de urmărire sau într-un
    ''' jurnal. Nu e un format de fir și nu se parsează niciodată înapoi.
    ''' </summary>
    Public Function ToDebugString() As String
        Dim sb As New StringBuilder()
        sb.AppendLine($"PrelucrareRezultat {CodAngajament} ({Moment:yyyy-MM-dd HH:mm:ss})")
        sb.AppendLine($"  workflow: {Workflow}")
        sb.AppendLine($"  scalari ({Scalari.Count})")
        For Each kvp As KeyValuePair(Of String, String) In Scalari
            sb.AppendLine($"    {kvp.Key}: {kvp.Value}")
        Next
        sb.AppendLine($"  tabele ({Tabele.Count})")
        For Each tabel As KeyValuePair(Of String, TabelRezultat) In Tabele
            Dim randuri As TabelRezultat = tabel.Value
            If randuri Is Nothing Then
                sb.AppendLine($"    {tabel.Key} [0 rânduri]")
                Continue For
            End If
            sb.AppendLine($"    {tabel.Key} [{randuri.Count} rânduri]{ImbricatePe(randuri)}")
            For i As Integer = 0 To randuri.Count - 1
                sb.AppendLine($"      rând {i}")
                For Each celula As KeyValuePair(Of String, CelulaTabel) In randuri(i)
                    Dim v As CelulaTabel = If(celula.Value, CelulaTabel.Gol)
                    Dim text As String = v.ToDebugString().Replace(
                        Environment.NewLine, Environment.NewLine & "        ")
                    sb.AppendLine($"        {celula.Key}: {text}")
                Next
            Next
        Next
        Return sb.ToString()
    End Function

    Private Shared Function ImbricatePe(tabel As TabelRezultat) As String
        Dim imbricate As IReadOnlyList(Of String) = tabel.ColoaneImbricate()
        If imbricate.Count = 0 Then Return String.Empty
        Return " · imbricate: " & String.Join(", ", imbricate)
    End Function
End Class
