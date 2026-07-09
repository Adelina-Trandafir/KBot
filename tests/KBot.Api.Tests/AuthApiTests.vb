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
        Public Property LastAuthorization As String
        Public Property LastBody As String

        Protected Overrides Async Function SendAsync(request As HttpRequestMessage,
                                                     cancellationToken As CancellationToken) _
            As Task(Of HttpResponseMessage)
            LastRequestUri = request.RequestUri
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
            .ResponseBody = "{""units"":[{""IdUnitate"":136,""DbName"":""000_DEMO"",""NumeUnitate"":""DEMO"",""AlteDetalii"":""x"",""Sursa"":""02A"",""AnDate"":2026,""DC"":""dc""}]}"
        }
        Dim api = NewApi(h)

        Dim units = Await api.GetUnitsAsync("u", "p", Nothing, CancellationToken.None)

        Assert.Single(units)
        Assert.Equal("000_DEMO", units(0).DbName)
        Assert.Equal("/api/auth/units", h.LastRequestUri.AbsolutePath)
        ' Apel pre-auth: credentialele sunt in corp, NICIUN header de autorizare.
        Assert.Null(h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function Login_DeserializesTokenContextAndRole() As Task
        Dim h As New StubHandler With {
            .ResponseBody = "{""Token"":""tok-opaque-123"",""SessionContext"":{""DbName"":""000_DEMO"",""IdUnitate"":136,""ANL"":2026,""CodProgram"":""P"",""SectorSursa"":""02A"",""CF"":""123"",""NumeUnitate"":""DEMO"",""Role"":""Contabil""}}"
        }
        Dim api = NewApi(h)

        Dim result = Await api.LoginAsync("u", "p", 136, "PC1", CancellationToken.None)

        Assert.Equal("tok-opaque-123", result.Token)
        Assert.Equal("000_DEMO", result.SessionContext.DbName)
        Assert.Equal("Contabil", result.SessionContext.Role)
        Assert.Equal(2026, result.SessionContext.ANL)
        Assert.Equal("/api/auth/login", h.LastRequestUri.AbsolutePath)
        Assert.Null(h.LastAuthorization)
    End Function

    <Fact>
    Public Async Function Login_MissingToken_Throws() As Task
        Dim h As New StubHandler With {
            .ResponseBody = "{""SessionContext"":{""DbName"":""000_DEMO"",""IdUnitate"":136,""ANL"":2026,""CodProgram"":""P"",""SectorSursa"":""02A"",""CF"":""123"",""NumeUnitate"":""DEMO"",""Role"":""Contabil""}}"
        }
        Dim api = NewApi(h)

        Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await api.LoginAsync("u", "p", 136, "PC1", CancellationToken.None))
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
    Public Async Function NonSuccess_WithErrorJson_ThrowsServerMessageWithStatus() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.Unauthorized,
            .ResponseBody = "{""error"":""Utilizator sau parolă incorecte.""}"
        }
        Dim api = NewApi(h)

        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await api.GetUnitsAsync("u", "p", Nothing, CancellationToken.None))
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
            Async Function() Await api.GetUnitsAsync("u", "p", Nothing, CancellationToken.None))
        Assert.Contains("cod 500", ex.Message)
        Assert.True(ex.StatusCode.HasValue)
        Assert.Equal(500, ex.StatusCode.Value)
    End Function
End Class
