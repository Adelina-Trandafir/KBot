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

' Offline tests for ApiClient.TrimitePrelucrareAsync (slice 0048-02). A stub
' HttpMessageHandler captures the request body and returns a configured response — no
' network, no server.
'
' The point of the whole method is that a 409 is NOT a failure: the server rolled its
' transaction back and is asking a question. These tests pin that both directions of the
' round trip survive the wire: the question comes back as data, and the answer goes out
' under the exact JSON keys routes/forexe/prelucrare.py reads.
Public Class PrelucrareApiClientTests

    Private NotInheritable Class StubHandler
        Inherits HttpMessageHandler

        Public Property Status As HttpStatusCode = HttpStatusCode.OK
        Public Property ResponseBody As String = "{}"
        Public Property LastRequestUri As Uri
        Public Property LastMethod As HttpMethod
        Public Property LastAuthorization As String
        Public Property LastBody As String

        Protected Overrides Function SendAsync(request As HttpRequestMessage,
                                               cancellationToken As CancellationToken) _
            As Task(Of HttpResponseMessage)
            LastRequestUri = request.RequestUri
            LastMethod = request.Method
            LastAuthorization = If(request.Headers.Authorization IsNot Nothing,
                                   request.Headers.Authorization.ToString(), Nothing)
            LastBody = If(request.Content IsNot Nothing,
                          request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult(),
                          Nothing)
            Return Task.FromResult(New HttpResponseMessage(Status) With {
                .Content = New StringContent(ResponseBody, Encoding.UTF8, "application/json")
            })
        End Function
    End Class

    Private Shared Function NewClient(handler As StubHandler) As ApiClient
        Dim http As New HttpClient(handler) With {.BaseAddress = New Uri("http://localhost/")}
        Dim session As New SessionContext() With {.Token = "tok-opaque-123"}
        Return New ApiClient(http, New ApiOptions(), session)
    End Function

    Private Shared Function Pachet() As PrelucrareRezultat
        Dim p As New PrelucrareRezultat() With {
            .CodAngajament = "AAB37CNBK95",
            .Workflow = "adlop - Prelucrare Completa.wfl",
            .Moment = New DateTime(2026, 8, 25, 10, 12, 0)
        }
        p.Scalari("DataAngajament") = "10/02/2026"
        p.Tabele("TabelIndicatori_results") = New List(Of Dictionary(Of String, String)) From {
            New Dictionary(Of String, String) From {
                {"Indicator_ang", "AAB"},
                {"Sector_Sursa_Indicator", "02E- 65. 04. 02. 20. 01. 01"}}
        }
        Return p
    End Function

    Private Const CorpAlegere As String =
        "{""error"":""O clasificație se potrivește cu mai multe unități."",""reason"":""ALEGERE_UNITATE""," &
        """cod"":""AAB37CNBK95"",""alegeri_necesare"":[{""ss"":""02E"",""clsfe"":""200101""," &
        """clsf"":""02E- 65. 04. 02. 20. 01. 01"",""cod_indicator"":""AAB""," &
        """indicatori"":[""AAB"",""AAC""],""unitati"":[" &
        "{""id_unitate"":75,""detalii"":""SC29 LOCAL"",""sursa_sector"":""02A"",""cod_program"":""P75""}," &
        "{""id_unitate"":76,""detalii"":""ENERGETIC ISJ"",""sursa_sector"":""02E"",""cod_program"":""P76""}]}]}"

    Private Const CorpSalvat As String =
        "{""cod"":""AAB37CNBK95"",""are"":{""Indicatori"":true}," &
        """scrise"":{""FX_Angajamente"":1,""FX_Indicatori"":2}," &
        """avertismente"":[""Pașii 3–8 ai ingestiei nu sunt încă portați.""]}"

    ' ── 200 ───────────────────────────────────────────────────────────────
    <Fact>
    Public Async Function Salvat_MapeazaContoareleSiAvertismentele() As Task
        Dim h As New StubHandler With {.ResponseBody = CorpSalvat}
        Dim r = Await NewClient(h).TrimitePrelucrareAsync(Pachet(), Nothing, CancellationToken.None)

        Assert.Equal(PrelucrareStare.Salvat, r.Stare)
        Assert.Equal("AAB37CNBK95", r.CodAngajament)
        Assert.True(r.AreIndicatori)
        Assert.Equal(1, r.Scrise("FX_Angajamente"))
        Assert.Equal(2, r.Scrise("FX_Indicatori"))
        Assert.Single(r.Avertismente)
        Assert.Empty(r.AlegeriNecesare)
    End Function

    <Fact>
    Public Async Function Salvat_PosteazaPeRutaCorecta_CuBearer() As Task
        Dim h As New StubHandler With {.ResponseBody = CorpSalvat}
        Await NewClient(h).TrimitePrelucrareAsync(Pachet(), Nothing, CancellationToken.None)

        Assert.Equal(HttpMethod.Post, h.LastMethod)
        Assert.Equal("/api/forexe/prelucrare", h.LastRequestUri.AbsolutePath)
        Assert.Equal("Bearer tok-opaque-123", h.LastAuthorization)
    End Function

    ' Baza NU se trimite niciodată: serverul o ia din sesiune (o bază = o unitate).
    <Fact>
    Public Async Function Cererea_NuPoartaNumeleBazei() As Task
        Dim h As New StubHandler With {.ResponseBody = CorpSalvat}
        Await NewClient(h).TrimitePrelucrareAsync(Pachet(), Nothing, CancellationToken.None)

        Assert.DoesNotContain("db_name", h.LastBody)
    End Function

    <Fact>
    Public Async Function Cererea_PoartaCheileCititeDeServer() As Task
        Dim h As New StubHandler With {.ResponseBody = CorpSalvat}
        Await NewClient(h).TrimitePrelucrareAsync(Pachet(), Nothing, CancellationToken.None)

        Using doc As JsonDocument = JsonDocument.Parse(h.LastBody)
            Dim root = doc.RootElement
            Assert.Equal("AAB37CNBK95", root.GetProperty("cod").GetString())
            Assert.Equal("adlop - Prelucrare Completa.wfl", root.GetProperty("workflow").GetString())
            Assert.Equal("10/02/2026",
                         root.GetProperty("scalari").GetProperty("DataAngajament").GetString())
            Assert.Equal(1, root.GetProperty("tabele").GetProperty("TabelIndicatori_results").GetArrayLength())
        End Using
    End Function

    ' ── 409 ───────────────────────────────────────────────────────────────
    <Fact>
    Public Async Function Alegere_NuAruncaCiIntoarceIntrebarea() As Task
        Dim h As New StubHandler With {.Status = CType(409, HttpStatusCode), .ResponseBody = CorpAlegere}
        Dim r = Await NewClient(h).TrimitePrelucrareAsync(Pachet(), Nothing, CancellationToken.None)

        Assert.Equal(PrelucrareStare.AlegereUnitate, r.Stare)
        Assert.Equal("AAB37CNBK95", r.CodAngajament)
        Assert.Contains("mai multe unități", r.Mesaj)
        Dim q = Assert.Single(r.AlegeriNecesare)
        Assert.Equal("02E", q.Ss)
        Assert.Equal("200101", q.ClsfE)
        Assert.Equal("02E- 65. 04. 02. 20. 01. 01", q.Clsf)
        Assert.Equal("AAB", q.CodIndicator)
        Assert.Equal(New String() {"AAB", "AAC"}, q.Indicatori)
        Assert.Equal(2, q.Unitati.Count)
        ' Nume, nu doar numere — de asta există dus-întorsul.
        Assert.Equal("SC29 LOCAL", q.Unitati(0).Detalii)
        Assert.Equal("ENERGETIC ISJ", q.Unitati(1).Detalii)
        Assert.Equal(76, q.Unitati(1).IdUnitate)
        Assert.Equal("P76", q.Unitati(1).CodProgram)
    End Function

    ' Un 409 cu alt cod-motiv NU e întrebarea noastră: iese ca orice alt conflict.
    <Fact>
    Public Async Function Alegere_AltCodMotiv_Arunca() As Task
        Dim h As New StubHandler With {
            .Status = CType(409, HttpStatusCode),
            .ResponseBody = "{""error"":""Altceva."",""reason"":""SHA_MISMATCH""}"}
        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Function() NewClient(h).TrimitePrelucrareAsync(Pachet(), Nothing, CancellationToken.None))
        Assert.Equal(409, ex.StatusCode)
        Assert.Equal("SHA_MISMATCH", ex.Reason)
    End Function

    ' Corp non-JSON pe un 409 — nu se pretinde că e o întrebare.
    <Fact>
    Public Async Function Alegere_CorpNeJson_Arunca() As Task
        Dim h As New StubHandler With {.Status = CType(409, HttpStatusCode), .ResponseBody = "<html/>"}
        Await Assert.ThrowsAsync(Of ApiException)(
            Function() NewClient(h).TrimitePrelucrareAsync(Pachet(), Nothing, CancellationToken.None))
    End Function

    ' ── retrimiterea cu alegeri ────────────────────────────────────────────
    <Fact>
    Public Async Function Alegerile_MergPeFirCuCheileServerului() As Task
        Dim h As New StubHandler With {.ResponseBody = CorpSalvat}
        Dim alegeri As New List(Of AlegereUnitate) From {
            New AlegereUnitate() With {.Ss = "02E", .ClsfE = "200101",
                                       .IdUnitate = 76, .Retine = True}}
        Await NewClient(h).TrimitePrelucrareAsync(Pachet(), alegeri, CancellationToken.None)

        Using doc As JsonDocument = JsonDocument.Parse(h.LastBody)
            Dim a = doc.RootElement.GetProperty("alegeri")(0)
            Assert.Equal("02E", a.GetProperty("ss").GetString())
            Assert.Equal("200101", a.GetProperty("clsfe").GetString())
            Assert.Equal(76, a.GetProperty("id_unitate").GetInt32())
            Assert.True(a.GetProperty("retine").GetBoolean())
        End Using
    End Function

    <Fact>
    Public Async Function FaraAlegeri_ListaEGoalaNuLipsa() As Task
        Dim h As New StubHandler With {.ResponseBody = CorpSalvat}
        Await NewClient(h).TrimitePrelucrareAsync(Pachet(), Nothing, CancellationToken.None)

        Using doc As JsonDocument = JsonDocument.Parse(h.LastBody)
            Assert.Equal(0, doc.RootElement.GetProperty("alegeri").GetArrayLength())
        End Using
    End Function

    ' ── erori ──────────────────────────────────────────────────────────────
    <Fact>
    Public Async Function Non2xx_AruncaCuMesajulRomanescAlServerului() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.BadRequest,
            .ResponseBody = "{""error"":""Clasificația «02E» nu aparține niciunei unități.""}"}
        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Function() NewClient(h).TrimitePrelucrareAsync(Pachet(), Nothing, CancellationToken.None))
        Assert.Contains("nu aparține niciunei unități", ex.Message)
    End Function

    <Fact>
    Public Async Function CodGol_ERespinsInainteDeRetea() As Task
        Dim h As New StubHandler With {.ResponseBody = CorpSalvat}
        Await Assert.ThrowsAsync(Of ArgumentException)(
            Function() NewClient(h).TrimitePrelucrareAsync(New PrelucrareRezultat(), Nothing,
                                                           CancellationToken.None))
        Assert.Null(h.LastRequestUri)
    End Function

End Class
