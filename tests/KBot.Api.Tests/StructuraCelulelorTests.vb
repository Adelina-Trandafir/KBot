Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports Xunit
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' Decizia D-N — structura călătorește. O coloană imbricată trebuie să plece pe fir ca un
''' ARRAY JSON adevărat, nu ca un șir care conține JSON.
'''
''' <para>
''' Ce se pinuiește aici e chiar defectul reparat pe 26.08.2026:
''' <c>ForexeRunner.TryParseTable</c> turtea fiecare celulă cu <c>prop.Value.ToString()</c>,
''' iar pentru <c>ListaReceptii.Detaliu</c> asta însemna o listă deghizată în text —
''' pe care serverul trebuia apoi să o parseze a doua oară.
''' </para>
''' <para>
''' Testul se uită la JSON-ul brut, nu la obiecte, fiindcă tocmai forma de pe fir e cea care
''' s-a stricat. Un test peste tipurile din memorie ar fi trecut și înainte de reparație.
''' </para>
''' </summary>
Public Class StructuraCelulelorTests

    Private NotInheritable Class StubHandler
        Inherits HttpMessageHandler

        Public Property ResponseBody As String = "{}"
        Public Property LastBody As String

        Protected Overrides Function SendAsync(request As HttpRequestMessage,
                                               cancellationToken As CancellationToken) _
            As Task(Of HttpResponseMessage)
            LastBody = If(request.Content IsNot Nothing,
                          request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult(),
                          Nothing)
            Return Task.FromResult(New HttpResponseMessage(HttpStatusCode.OK) With {
                .Content = New StringContent(ResponseBody, Encoding.UTF8, "application/json")
            })
        End Function
    End Class

    Private Const CorpPropunere As String =
        "{""cod"":""AAB37CNBK95"",""faza"":""propunere"",""amprenta"":""abc""," &
        """receptii"":[],""instantanee"":[],""are"":{},""scrise"":{},""avertismente"":[]}"

    Private Shared Function NewClient(handler As StubHandler) As ApiClient
        Dim http As New HttpClient(handler) With {.BaseAddress = New Uri("http://localhost/")}
        Dim session As New SessionContext() With {.Token = "tok-opaque-123"}
        Return New ApiClient(http, New ApiOptions(), session)
    End Function

    ''' <summary>
    ''' O celulă imbricată pe TREI niveluri: listă ▸ obiect ▸ listă ▸ obiect.
    ''' Nu e forma pe care o produce site-ul azi (acolo e un singur nivel), dar tipul o
    ''' promite recursiv, iar o promisiune netestată e o promisiune.
    ''' </summary>
    Private Shared Function CelulaPeTreiNiveluri() As CelulaTabel
        ' RandTabel E o secvență de perechi nume/celulă, deci servește și ca fel de a scrie
        ' un obiect imbricat fără să înșiri KeyValuePair-uri de mână.
        Dim frunza As CelulaTabel = CelulaTabel.DinObiect(New RandTabel From {
            {"Cod", "AAB"},
            {"Valoare", "1.234,50"}})

        Dim mijloc As CelulaTabel = CelulaTabel.DinObiect(New RandTabel From {
            {"Grupa", CelulaTabel.DinText("20")},
            {"Linii", CelulaTabel.DinLista(New List(Of CelulaTabel) From {frunza})}})

        Return CelulaTabel.DinLista(New List(Of CelulaTabel) From {mijloc})
    End Function

    Private Shared Function Pachet() As PrelucrareRezultat
        Dim p As New PrelucrareRezultat() With {
            .CodAngajament = "AAB37CNBK95",
            .Workflow = "adlop - Prelucrare Completa.wfl",
            .Moment = New DateTime(2026, 8, 26, 10, 12, 0)}
        p.Tabele("ListaReceptii_results") = New TabelRezultat From {
            New RandTabel From {
                {"Tip", "Plata ces"},
                {"Data", "11/02/2026"},
                {"Detaliu", CelulaPeTreiNiveluri()}}}
        Return p
    End Function

    Private Shared Async Function CorpTrimis() As Task(Of JsonElement)
        Dim handler As New StubHandler() With {.ResponseBody = CorpPropunere}
        Await NewClient(handler).CerePropunereAsync(Pachet(), Nothing, CancellationToken.None)
        Return JsonDocument.Parse(handler.LastBody).RootElement
    End Function

    <Fact>
    Public Async Function CelulaImbricata_PleacaCaArrayJson_NuCaSir() As Task
        Dim detaliu As JsonElement =
            (Await CorpTrimis()).GetProperty("tabele").
                GetProperty("ListaReceptii_results")(0).GetProperty("Detaliu")

        ' ĂSTA e testul. Înainte de D-N aici era JsonValueKind.String.
        Assert.Equal(JsonValueKind.Array, detaliu.ValueKind)
    End Function

    <Fact>
    Public Async Function CelulaImbricata_PastreazaToateCeleTreiNiveluri() As Task
        Dim detaliu As JsonElement =
            (Await CorpTrimis()).GetProperty("tabele").
                GetProperty("ListaReceptii_results")(0).GetProperty("Detaliu")

        Dim mijloc As JsonElement = detaliu(0)                       ' nivel 1: array
        Assert.Equal(JsonValueKind.Object, mijloc.ValueKind)         ' nivel 2: obiect
        Assert.Equal("20", mijloc.GetProperty("Grupa").GetString())

        Dim linii As JsonElement = mijloc.GetProperty("Linii")
        Assert.Equal(JsonValueKind.Array, linii.ValueKind)           ' nivel 3: array
        Assert.Equal(JsonValueKind.Object, linii(0).ValueKind)
        Assert.Equal("AAB", linii(0).GetProperty("Cod").GetString())
        Assert.Equal("1.234,50", linii(0).GetProperty("Valoare").GetString())
    End Function

    <Fact>
    Public Async Function CelulaScalara_RamaneSir() As Task
        Dim rand As JsonElement =
            (Await CorpTrimis()).GetProperty("tabele").GetProperty("ListaReceptii_results")(0)

        Assert.Equal(JsonValueKind.String, rand.GetProperty("Tip").ValueKind)
        Assert.Equal("Plata ces", rand.GetProperty("Tip").GetString())
    End Function

    <Fact>
    Public Async Function CorpulNuMaiContineJsonScapatCaText() As Task
        Dim handler As New StubHandler() With {.ResponseBody = CorpPropunere}
        Await NewClient(handler).CerePropunereAsync(Pachet(), Nothing, CancellationToken.None)

        ' Un JSON imbricat turtit în text apare pe fir cu ghilimelele escapate (\"Cod\").
        ' Absența secvenței e dovada directă că nimic nu s-a aplatizat pe drum.
        Dim ghilimeaEscapata As String = "\" & Chr(34) & "Cod"
        Assert.DoesNotContain(ghilimeaEscapata, handler.LastBody)
    End Function

    ''' <summary>
    ''' Dus-întors prin JSON, în amândouă direcțiile: tipurile ▸ arbore ▸ tipuri.
    ''' Puntea e folosită și la citirea dosarului local de asociere, deci direcția de
    ''' întoarcere nu e teoretică.
    ''' </summary>
    <Fact>
    Public Sub PunteaJson_DusIntors_PastreazaFormaExact()
        Dim tabele As New Dictionary(Of String, TabelRezultat) From {
            {"ListaReceptii_results", New TabelRezultat From {
                New RandTabel From {{"Detaliu", CelulaPeTreiNiveluri()}}}}}

        Dim inapoi As Dictionary(Of String, TabelRezultat) =
            TabeleJson.Din(TabeleJson.Catre(tabele))

        Dim celula As CelulaTabel = inapoi("ListaReceptii_results")(0)("Detaliu")
        Assert.True(celula.EsteLista)
        Assert.True(celula.Lista(0).EsteObiect)
        Assert.True(celula.Lista(0).Membru("Linii").EsteLista)
        Assert.Equal("AAB", celula.Lista(0).Membru("Linii").Lista(0).Membru("Cod").Text)
    End Sub

    ''' <summary>
    ''' Citirea unei celule imbricate ca text RIDICĂ. Turtirea tăcută e chiar defectul
    ''' reparat; un apelant care vrea o redare citibilă are <c>ToDebugString</c>.
    ''' </summary>
    <Fact>
    Public Sub CitireaCaTextAUneiCeluleImbricate_Ridica()
        Dim ex As InvalidOperationException =
            Assert.Throws(Of InvalidOperationException)(Function() CelulaPeTreiNiveluri().Text)
        Assert.Contains("listă", ex.Message)
    End Sub

    ''' <summary>Ce vede Adelina în fereastra de urmărire, fără să apeleze nimic.</summary>
    <Fact>
    Public Sub Tabelul_ISiSpuneColoaneleImbricate()
        Dim tabel As TabelRezultat = Pachet().Tabele("ListaReceptii_results")

        Assert.Equal(New String() {"Detaliu"}, tabel.ColoaneImbricate())
        Assert.Contains("imbricate: Detaliu", tabel.ToString())
        Assert.Contains("3 coloane", tabel(0).ToString())
        Assert.Equal("listă[1]", tabel(0)("Detaliu").ToString())
    End Sub

    <Fact>
    Public Sub ToDebugString_AratăArboreleIntreg()
        Dim text As String = Pachet().ToDebugString()

        Assert.Contains("ListaReceptii_results [1 rânduri] · imbricate: Detaliu", text)
        Assert.Contains("Grupa: 20", text)
        Assert.Contains("Cod: AAB", text)
    End Sub
End Class
