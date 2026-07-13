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
Imports KBot.Domain

' Teste offline pentru AuthApi. Folosesc un HttpMessageHandler stub care capteaza
' cererea (URI / Authorization / body) si intoarce un raspuns configurat — fara retea.
Public Class AuthApiTests

    Private NotInheritable Class StubHandler
        Inherits HttpMessageHandler

        Public Property Status As HttpStatusCode = HttpStatusCode.OK
        Public Property ResponseBody As String = "{}"
        Public Property LastRequestUri As Uri
        Public Property LastMethod As HttpMethod
        Public Property LastAuthorization As String
        Public Property LastBody As String

        Protected Overrides Async Function SendAsync(request As HttpRequestMessage,
                                                     cancellationToken As CancellationToken) _
            As Task(Of HttpResponseMessage)
            LastRequestUri = request.RequestUri
            LastMethod = request.Method
            LastAuthorization = If(request.Headers.Authorization IsNot Nothing,
                                   request.Headers.Authorization.ToString(), Nothing)
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
        Return New AuthApi(http)
    End Function

    <Fact>
    Public Async Function GetUnits_PostsWithoutAuthHeader_AndDeserializes() As Task
        Dim h As New StubHandler With {
            .ResponseBody = "{""units"":[{""DC"":""000_DEMO"",""NumeUnitate"":""DEMO""}]}"
        }
        Dim api = NewApi(h)

        Dim units = Await api.GetUnitsAsync("u", "p", CancellationToken.None)

        Assert.Single(units)
        Assert.Equal("000_DEMO", units(0).DC)
        Assert.Equal("DEMO", units(0).NumeUnitate)
        Assert.Equal("/api/auth/units", h.LastRequestUri.AbsolutePath)
        ' Corpul poarta cheile lowercase pe care serverul le citeste.
        Assert.Contains("""username""", h.LastBody)
        Assert.Contains("""password""", h.LastBody)
        ' Apel pre-auth: credentialele sunt in corp, NICIUN header de autorizare.
        Assert.Null(h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function Login_SendsDbName_DeserializesTokenContextAndLastSs() As Task
        Dim h As New StubHandler With {
            .ResponseBody = "{""Token"":""tok-opaque-123"",""SessionContext"":{""DbName"":""000_DEMO"",""CF"":""123"",""NumeUnitate"":""DEMO"",""Role"":""Contabil""},""LastSS"":""02A""}"
        }
        Dim api = NewApi(h)

        Dim result = Await api.LoginAsync("u", "p", "000_DEMO", "PC1", CancellationToken.None)

        Assert.Equal("tok-opaque-123", result.Token)
        Assert.Equal("000_DEMO", result.SessionContext.DbName)
        Assert.Equal("Contabil", result.SessionContext.Role)
        Assert.Equal("02A", result.LastSS)
        Assert.Equal("/api/auth/login", h.LastRequestUri.AbsolutePath)
        ' Clientul trimite DC-ul sub cheia db_name; machine e prezent.
        Assert.Contains("""db_name""", h.LastBody)
        Assert.Contains("000_DEMO", h.LastBody)
        Assert.Contains("""machine""", h.LastBody)
        Assert.Null(h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function Login_MissingToken_Throws() As Task
        Dim h As New StubHandler With {
            .ResponseBody = "{""SessionContext"":{""DbName"":""000_DEMO"",""CF"":""123"",""NumeUnitate"":""DEMO"",""Role"":""Contabil""}}"
        }
        Dim api = NewApi(h)

        Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await api.LoginAsync("u", "p", "000_DEMO", "PC1", CancellationToken.None))
    End Function

    <Fact>
    Public Async Function Logout_SendsBearerToken() As Task
        Dim h As New StubHandler With {.ResponseBody = "{""ok"":true}"}
        Dim api = NewApi(h)

        Await api.LogoutAsync("tok-opaque-123", CancellationToken.None)

        Assert.Equal("/api/auth/logout", h.LastRequestUri.AbsolutePath)
        Assert.Equal("Bearer tok-opaque-123", h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function GetPeriods_SendsBearer_ParsesCatalog() As Task
        Dim h As New StubHandler With {
            .ResponseBody = "{""periods"":[{""AN"":2026,""SS"":""02A"",""CodProgram"":""P01""},{""AN"":2025,""SS"":""02B"",""CodProgram"":""P02""}]}"
        }
        Dim api = NewApi(h)

        Dim periods = Await api.GetPeriodsAsync("tok-opaque-123", "000_DEMO", CancellationToken.None)

        Assert.Equal(2, periods.Count)
        Assert.Equal(2026, periods(0).AN)
        Assert.Equal("02A", periods(0).SS)
        Assert.Equal("P01", periods(0).CodProgram)
        Assert.Equal(HttpMethod.Get, h.LastMethod)
        Assert.Equal("/api/auth/periods", h.LastRequestUri.AbsolutePath)
        ' db_name calatoreste in query string.
        Assert.Contains("db_name=000_DEMO", h.LastRequestUri.Query)
        Assert.Equal("Bearer tok-opaque-123", h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function SaveLastSs_SendsBearerAndSs() As Task
        Dim h As New StubHandler With {.ResponseBody = "{""ok"":true}"}
        Dim api = NewApi(h)

        Await api.SaveLastSsAsync("tok-opaque-123", "02A", CancellationToken.None)

        Assert.Equal("/api/auth/last-ss", h.LastRequestUri.AbsolutePath)
        Assert.Equal("Bearer tok-opaque-123", h.LastAuthorization)
        Assert.Contains("""ss""", h.LastBody)
        Assert.Contains("02A", h.LastBody)
    End Function

    <Fact>
    Public Async Function NonSuccess_WithErrorJson_ThrowsServerMessageWithStatus() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.Unauthorized,
            .ResponseBody = "{""error"":""Utilizator sau parolă incorecte.""}"
        }
        Dim api = NewApi(h)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await api.GetUnitsAsync("u", "p", CancellationToken.None))
        Assert.Equal("Utilizator sau parolă incorecte.", ex.Message)
        Assert.True(ex.StatusCode.HasValue)
        Assert.Equal(401, ex.StatusCode.Value)
    End Function

    <Fact>
    Public Async Function NonSuccess_WithNonJsonBody_ThrowsStatusFallback() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.InternalServerError,
            .ResponseBody = "Internal Server Error"
        }
        Dim api = NewApi(h)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await api.GetUnitsAsync("u", "p", CancellationToken.None))
        Assert.Contains("cod 500", ex.Message)
        Assert.True(ex.StatusCode.HasValue)
        Assert.Equal(500, ex.StatusCode.Value)
    End Function
End Class
