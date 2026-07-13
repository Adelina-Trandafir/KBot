Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports Xunit
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

' Offline tests for ApiClient.GetAngajamenteAsync. A stub HttpMessageHandler captures
' the request (URI / Authorization) and returns a configured response — no network.
Public Class ApiClientTests

    Private NotInheritable Class StubHandler
        Inherits HttpMessageHandler

        Public Property Status As HttpStatusCode = HttpStatusCode.OK
        Public Property ResponseBody As String = "{}"
        Public Property LastRequestUri As Uri
        Public Property LastMethod As HttpMethod
        Public Property LastAuthorization As String

        Protected Overrides Function SendAsync(request As HttpRequestMessage,
                                               cancellationToken As CancellationToken) _
            As Task(Of HttpResponseMessage)
            LastRequestUri = request.RequestUri
            LastMethod = request.Method
            LastAuthorization = If(request.Headers.Authorization IsNot Nothing,
                                   request.Headers.Authorization.ToString(), Nothing)
            Return Task.FromResult(New HttpResponseMessage(Status) With {
                .Content = New StringContent(ResponseBody, Encoding.UTF8, "application/json")
            })
        End Function
    End Class

    Private Shared Function NewClient(handler As StubHandler, ByRef session As SessionContext) As ApiClient
        Dim http As New HttpClient(handler) With {.BaseAddress = New Uri("http://localhost/")}
        session = New SessionContext() With {.Token = "tok-opaque-123"}
        Return New ApiClient(http, New ApiOptions(), session)
    End Function

    Private Const TwoRows As String =
        "{""db_name"":""000_DEMO"",""count"":2,""rows"":[" &
        "{""Cod"":""C1"",""Descriere"":""D1"",""Stare"":""În derulare"",""IDDF"":30," &
        "  ""Surse"":""02A;02B"",""Incarcat"":true,""Preluat"":true,""Salarii"":false," &
        "  ""Ascuns"":false,""DataCreare"":""2026-01-18T00:00:00""}," &
        "{""Cod"":""C2"",""Descriere"":""D2"",""Stare"":""Anulat"",""IDDF"":null," &
        "  ""Surse"":null,""Incarcat"":false,""Preluat"":false,""Salarii"":false," &
        "  ""Ascuns"":true,""DataCreare"":null}]}"

    <Fact>
    Public Async Function GetAngajamente_BuildsUrl_SendsBearer() As Task
        Dim h As New StubHandler With {.ResponseBody = TwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await client.GetAngajamenteAsync("000_DEMO", 0, False, CancellationToken.None)

        Assert.Equal(HttpMethod.Get, h.LastMethod)
        Assert.Equal("/api/forexe/angajamente", h.LastRequestUri.AbsolutePath)
        Assert.Contains("db_name=000_DEMO", h.LastRequestUri.Query)
        Assert.Contains("id_unitate=0", h.LastRequestUri.Query)
        Assert.Contains("doar_anulate=0", h.LastRequestUri.Query)
        Assert.Equal("Bearer tok-opaque-123", h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function GetAngajamente_DoarAnulate_SetsFlag() As Task
        Dim h As New StubHandler With {.ResponseBody = "{""db_name"":""000_DEMO"",""count"":0,""rows"":[]}"}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Await client.GetAngajamenteAsync("000_DEMO", 7, True, CancellationToken.None)

        Assert.Contains("id_unitate=7", h.LastRequestUri.Query)
        Assert.Contains("doar_anulate=1", h.LastRequestUri.Query)
    End Function

    <Fact>
    Public Async Function GetAngajamente_Deserializes_AllFields_And_NullSurse() As Task
        Dim h As New StubHandler With {.ResponseBody = TwoRows}
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim rows = Await client.GetAngajamenteAsync("000_DEMO", 0, False, CancellationToken.None)

        Assert.Equal(2, rows.Count)

        Dim r0 = rows(0)
        Assert.Equal("C1", r0.CodAngajament)
        Assert.Equal("În derulare", r0.Stare)
        Assert.Equal("02A;02B", r0.Surse)
        Assert.True(r0.IDDF.HasValue)
        Assert.Equal(30, r0.IDDF.Value)
        Assert.True(r0.Incarcat)
        Assert.True(r0.Preluat)
        Assert.False(r0.Salarii)
        Assert.False(r0.Ascuns)
        Assert.True(r0.DataCreare.HasValue)
        Assert.Equal(New Date(2026, 1, 18), r0.DataCreare.Value)

        Dim r1 = rows(1)
        Assert.Equal("C2", r1.CodAngajament)
        Assert.Null(r1.Surse)                  ' orphan: Surse = null
        Assert.False(r1.IDDF.HasValue)
        Assert.True(r1.Ascuns)
        Assert.False(r1.DataCreare.HasValue)
    End Function

    <Fact>
    Public Async Function GetAngajamente_Non2xx_ThrowsWithStatus() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.InternalServerError,
            .ResponseBody = "{""error"":""boom""}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await client.GetAngajamenteAsync("000_DEMO", 0, False, CancellationToken.None))
        Assert.True(ex.StatusCode.HasValue)
        Assert.Equal(500, ex.StatusCode.Value)
    End Function

    <Fact>
    Public Async Function GetAngajamente_401_ThrowsWithStatus_ForReauth() As Task
        ' A 401 must surface as ApiException(401) with no retry so MainForm.WithReauth
        ' can catch it and re-login.
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.Unauthorized,
            .ResponseBody = "{""error"":""expired""}"
        }
        Dim session As SessionContext = Nothing
        Dim client = NewClient(h, session)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await client.GetAngajamenteAsync("000_DEMO", 0, False, CancellationToken.None))
        Assert.Equal(401, ex.StatusCode.Value)
    End Function

End Class
