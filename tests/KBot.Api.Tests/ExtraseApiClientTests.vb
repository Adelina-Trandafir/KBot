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

' Offline tests for slice 0057: the angajamente refresh (`doar_noi`) and the SNM statement
' import. A stub handler captures the request body and answers a fixed one -- no network.
'
' What these pin is the WIRE. Two assertions carry the weight:
'   * `doar_noi` actually reaches the server. Without it the route falls back to the old
'     upsert and REWRITES Descriere/Stare on every angajament the operator already has --
'     the exact thing the footer button must not do, and a change nothing would surface.
'   * the statement fields keep their Access names (PdfFisier / DataFisier / XmlContent).
'     Two of them are hashed together on the server into the per-file HASH, so a rename
'     would not fail: it would re-import every statement, for ever.
Public Class ExtraseApiClientTests

    Private NotInheritable Class StubHandler
        Inherits HttpMessageHandler

        Public Property Status As HttpStatusCode = HttpStatusCode.OK
        Public Property ResponseBody As String = "{}"
        Public Property LastBody As String
        Public Property LastUri As String
        Public Property LastMethod As String

        Protected Overrides Function SendAsync(request As HttpRequestMessage,
                                               cancellationToken As CancellationToken) _
            As Task(Of HttpResponseMessage)
            LastUri = request.RequestUri.PathAndQuery
            LastMethod = request.Method.Method
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

    Private Shared Function Randuri() As List(Of Angajament)
        Return New List(Of Angajament) From {
            New Angajament() With {.CodAngajament = "AAB4FBAT96M", .Descriere = "Servicii", .Stare = "Activ"},
            New Angajament() With {.CodAngajament = "AAB2MAACHXB", .Descriere = "Lucrări", .Stare = "Activ"}
        }
    End Function

    ' ── Lista de angajamente ─────────────────────────────────────────────

    <Fact>
    Public Async Function AdaugareaNoilor_TrimiteDoarNoi() As Task
        Dim h As New StubHandler() With {
            .ResponseBody = "{""status"": ""success"", ""received"": 2, ""candidates"": 2, " &
                            """inserate"": 1, ""existente"": 1}"}

        Dim r = Await NewClient(h).AdaugaAngajamenteNoiAsync("000_DEMO", Randuri(), CancellationToken.None)

        Assert.Equal("/api/forexe/angajamente/upsert", h.LastUri)
        Using doc As JsonDocument = JsonDocument.Parse(h.LastBody)
            ' THE assertion of this file: without the flag the route rewrites what exists.
            Assert.True(doc.RootElement.GetProperty("doar_noi").GetBoolean())
            Assert.Equal("000_DEMO", doc.RootElement.GetProperty("db_name").GetString())
            Assert.Equal(2, doc.RootElement.GetProperty("rows").GetArrayLength())
            Assert.Equal("AAB4FBAT96M", doc.RootElement.GetProperty("rows")(0).GetProperty("Cod").GetString())
        End Using

        Assert.Equal(2, r.Primite)
        Assert.Equal(2, r.Candidate)
        Assert.Equal(1, r.Inserate)
        Assert.Equal(1, r.Existente)
    End Function

    ' The flat upsert is the OTHER caller of the same route (the options menu's
    ' «Sincronizare»). It must keep saying doar_noi = false, or one button would quietly
    ' start doing the other's job.
    <Fact>
    Public Async Function UpsertulPlat_NuCereDoarNoi() As Task
        Dim h As New StubHandler() With {.ResponseBody = "{""status"": ""success""}"}

        Await NewClient(h).UpsertAngajamenteAsync("000_DEMO", Randuri(), CancellationToken.None)

        Using doc As JsonDocument = JsonDocument.Parse(h.LastBody)
            Assert.False(doc.RootElement.GetProperty("doar_noi").GetBoolean())
        End Using
    End Function

    ' A 2xx whose body is not our object means the write happened and the counts are
    ' unknown. Reporting zeroes would tell the operator nothing was added.
    <Fact>
    Public Async Function AdaugareaNoilor_CorpNul_Arunca() As Task
        Dim h As New StubHandler() With {.ResponseBody = "null"}
        Dim client As ApiClient = NewClient(h)

        Await Assert.ThrowsAsync(Of InvalidOperationException)(
            Function() client.AdaugaAngajamenteNoiAsync("000_DEMO", Randuri(), CancellationToken.None))
    End Function

    ' ── Extrase de cont (SNM) ────────────────────────────────────────────

    <Fact>
    Public Async Function ImportulExtraselor_PastreazaNumeleDeCampuriAccess() As Task
        Dim h As New StubHandler() With {
            .ResponseBody = "{""primite"": 1, ""importate"": 1, ""sarite"": 0, ""randuri"": 12, " &
                            """avertismente"": [""Nomenclatorul «Clasificatii_Venituri» nu există""]}"}
        Dim extrase As New List(Of ExtrasPentruImport) From {
            New ExtrasPentruImport() With {
                .PdfFisier = "TREZ521_ExtrasEP_XML_SIGNED_31122025h1327.pdf",
                .DataFisier = "31.12.2025 13:27:00",
                .XmlContent = "<extras Data_extras=""31.12.2025""/>",
                .CaleLocala = "C:\KBOT\Extrase\x.pdf"}}

        Dim r = Await NewClient(h).ImportaExtraseAsync(extrase, CancellationToken.None)

        Assert.Equal("POST", h.LastMethod)
        Assert.Equal("/api/forexe/extrase/import", h.LastUri)
        Using doc As JsonDocument = JsonDocument.Parse(h.LastBody)
            Dim rand As JsonElement = doc.RootElement.GetProperty("extrase")(0)
            ' The three Access names, verbatim. Two of them make the file HASH.
            Assert.Equal("TREZ521_ExtrasEP_XML_SIGNED_31122025h1327.pdf",
                         rand.GetProperty("PdfFisier").GetString())
            Assert.Equal("31.12.2025 13:27:00", rand.GetProperty("DataFisier").GetString())
            Assert.Equal("<extras Data_extras=""31.12.2025""/>", rand.GetProperty("XmlContent").GetString())
            Assert.Equal("C:\KBOT\Extrase\x.pdf", rand.GetProperty("CaleLocala").GetString())
        End Using

        Assert.Equal(1, r.Importate)
        Assert.Equal(0, r.Sarite)
        Assert.Equal(12, r.Randuri)
        ' Warnings are carried through, not dropped: a missing nomenclator leaves every
        ' statement header without a unit, and the operator has to hear about it.
        Assert.Single(r.Avertismente)
        Assert.Contains("Clasificatii_Venituri", r.Avertismente(0))
    End Function

    <Fact>
    Public Async Function UltimaData_NullInseamnaFaraData() As Task
        Dim h As New StubHandler() With {.ResponseBody = "{""data_extras"": null}"}

        Dim r As Date? = Await NewClient(h).GetUltimaDataExtrasAsync(CancellationToken.None)

        Assert.Equal("/api/forexe/extrase/ultima", h.LastUri)
        ' Not an error: the first run has nothing behind it and walks the whole inbox.
        Assert.False(r.HasValue)
    End Function

    <Fact>
    Public Async Function UltimaData_SeCitesteInvariant() As Task
        Dim h As New StubHandler() With {.ResponseBody = "{""data_extras"": ""2026-02-11""}"}

        Dim r As Date? = Await NewClient(h).GetUltimaDataExtrasAsync(CancellationToken.None)

        Assert.True(r.HasValue)
        Assert.Equal(New Date(2026, 2, 11), r.Value)
    End Function

    ' An unreadable date is NOT quietly turned into "no date": that would re-download the
    ' whole inbox and look like a slow day rather than a broken contract.
    <Fact>
    Public Async Function UltimaData_DataStricata_Arunca() As Task
        Dim h As New StubHandler() With {.ResponseBody = "{""data_extras"": ""ieri""}"}
        Dim client As ApiClient = NewClient(h)

        Await Assert.ThrowsAsync(Of InvalidOperationException)(
            Function() client.GetUltimaDataExtrasAsync(CancellationToken.None))
    End Function

    <Fact>
    Public Async Function ImportulExtraselor_NonDouaSuteArunca() As Task
        Dim h As New StubHandler() With {
            .Status = HttpStatusCode.InternalServerError,
            .ResponseBody = "{""error"": ""tabela lipsește""}"}
        Dim client As ApiClient = NewClient(h)

        Dim ex As ApiException = Await Assert.ThrowsAsync(Of ApiException)(
            Function() client.ImportaExtraseAsync(New List(Of ExtrasPentruImport)(), CancellationToken.None))
        Assert.Contains("tabela lipsește", ex.Message)
    End Function

End Class
