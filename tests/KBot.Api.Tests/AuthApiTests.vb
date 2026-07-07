Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports Xunit
Imports KBot.Api
Imports KBot.Domain

' Teste offline pentru AuthApi. Folosesc un HttpMessageHandler stub care capteaza
' cererea (URI / X-Api-Key / body) si intoarce un raspuns configurat — fara retea.
Public Class AuthApiTests

    Private NotInheritable Class StubHandler
        Inherits HttpMessageHandler

        Public Property Status As HttpStatusCode = HttpStatusCode.OK
        Public Property ResponseBody As String = "{}"
        Public Property LastRequestUri As Uri
        Public Property LastApiKey As String
        Public Property LastBody As String

        Protected Overrides Async Function SendAsync(request As HttpRequestMessage,
                                                     cancellationToken As CancellationToken) _
            As Task(Of HttpResponseMessage)
            LastRequestUri = request.RequestUri
            Dim vals As IEnumerable(Of String) = Nothing
            If request.Headers.TryGetValues("X-Api-Key", vals) Then LastApiKey = vals.First()
            If request.Content IsNot Nothing Then
                LastBody = Await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(False)
            End If
            Return New HttpResponseMessage(Status) With {
                .Content = New StringContent(ResponseBody, Encoding.UTF8, "application/json")
            }
        End Function
    End Class

    Private Shared Function NewApi(handler As StubHandler) As AuthApi
        Dim http As New HttpClient(handler) With {.BaseAddress = New Uri("http://localhost/")}
        Return New AuthApi(http, New ApiOptions With {.ApiKey = "test-key", .BaseUrl = "http://localhost/api"})
    End Function

    <Fact>
    Public Async Function GetUnits_PostsWithApiKey_AndDeserializes() As Task
        Dim h As New StubHandler With {
            .ResponseBody = "{""units"":[{""IdUnitate"":136,""DbName"":""000_DEMO"",""NumeUnitate"":""DEMO"",""AlteDetalii"":""x"",""Sursa"":""02A"",""AnDate"":2026,""DC"":""dc""}]}"
        }
        Dim api = NewApi(h)

        Dim units = Await api.GetUnitsAsync("u", "p", Nothing, CancellationToken.None)

        Assert.Single(units)
        Assert.Equal("000_DEMO", units(0).DbName)
        Assert.Equal("/api/auth/units", h.LastRequestUri.AbsolutePath)
        Assert.Equal("test-key", h.LastApiKey)
    End Function

    <Fact>
    Public Async Function Login_DeserializesSessionIdContextAndRole() As Task
        Dim h As New StubHandler With {
            .ResponseBody = "{""session_id"":42,""SessionContext"":{""DbName"":""000_DEMO"",""IdUnitate"":136,""ANL"":2026,""CodProgram"":""P"",""SectorSursa"":""02A"",""CF"":""123"",""NumeUnitate"":""DEMO"",""Role"":""Contabil""}}"
        }
        Dim api = NewApi(h)

        Dim result = Await api.LoginAsync("u", "p", 136, "PC1", CancellationToken.None)

        Assert.Equal(42, result.SessionId)
        Assert.Equal("000_DEMO", result.SessionContext.DbName)
        Assert.Equal("Contabil", result.SessionContext.Role)
        Assert.Equal(2026, result.SessionContext.ANL)
        Assert.Equal("/api/auth/login", h.LastRequestUri.AbsolutePath)
    End Function

    <Fact>
    Public Async Function Logout_PostsSessionId() As Task
        Dim h As New StubHandler With {.ResponseBody = "{""status"":""ok"",""stamped"":1}"}
        Dim api = NewApi(h)

        Await api.LogoutAsync(7, CancellationToken.None)

        Assert.Equal("/api/auth/logout", h.LastRequestUri.AbsolutePath)
        Assert.Contains("""session_id"":7", h.LastBody)
    End Function

    <Fact>
    Public Async Function NonSuccess_WithErrorJson_ThrowsServerMessage() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.Unauthorized,
            .ResponseBody = "{""error"":""Utilizator sau parolă incorecte.""}"
        }
        Dim api = NewApi(h)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await api.GetUnitsAsync("u", "p", Nothing, CancellationToken.None))
        Assert.Equal("Utilizator sau parolă incorecte.", ex.Message)
    End Function

    <Fact>
    Public Async Function NonSuccess_WithNonJsonBody_ThrowsStatusFallback() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.InternalServerError,
            .ResponseBody = "Internal Server Error"
        }
        Dim api = NewApi(h)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await api.GetUnitsAsync("u", "p", Nothing, CancellationToken.None))
        Assert.Contains("cod 500", ex.Message)
    End Function
End Class
