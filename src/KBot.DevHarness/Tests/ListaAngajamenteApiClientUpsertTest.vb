Option Strict On
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Domain

' Offline test: drives the REAL ApiClient through a capturing HttpMessageHandler.
' Verifies: POST to /api/forexe/angajamente/upsert, X-Api-Key header present and
' non-empty, body keys db_name/rows/Cod/Descriere/Stare (no CodAngajament), and
' that a non-2xx response THROWS (ApiClient wraps it in ApplicationException).
Public NotInheritable Class ListaAngajamenteApiClientUpsertTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "API — ListaAngajamente upsert: URL/header/body + throw on error"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Api"
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

    Public Async Function RunAsync(context As HarnessContext, ct As CancellationToken) _
        As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync

        Dim rows As New List(Of Angajament) From {
            New Angajament() With {.CodAngajament = "A1", .Descriere = "Nou", .Stare = "Nou"},
            New Angajament() With {.CodAngajament = "A2", .Descriere = "Existent", .Stare = "Definitivat"}
        }

        ' ---- PART 1: happy path (HTTP 200) — capture and assert the request ----
        Dim okHandler As New CapturingHandler(HttpStatusCode.OK, "{""status"":""success""}")

        ' Arrange — real ApiClient ctor (verified §0.1): New ApiClient(HttpClient, ApiOptions).
        ' BaseAddress lives on the HttpClient (as in Program.vb DI); the key is sent
        ' per-request from ApiOptions.ApiKey.
        Dim body As String
        Using http As New HttpClient(okHandler) With {.BaseAddress = New Uri("http://test.local/")}
            Dim opts As New ApiOptions() With {.BaseUrl = "http://test.local/", .ApiKey = "TEST-KEY"}
            Dim client As IApiClient = New ApiClient(http, opts)

            Await client.UpsertAngajamenteAsync("000_DEMO", rows, ct)

            Dim req As HttpRequestMessage = okHandler.CapturedRequest
            If req Is Nothing Then
                Return HarnessTestResult.Failed("No request captured — client sent nothing.")
            End If
            If req.Method <> HttpMethod.Post Then
                Return HarnessTestResult.Failed($"Expected POST, got {req.Method}.")
            End If
            If req.RequestUri Is Nothing OrElse
               Not req.RequestUri.AbsolutePath.EndsWith("/api/forexe/angajamente/upsert") Then
                Return HarnessTestResult.Failed($"Wrong URL: {req.RequestUri}.")
            End If
            If Not req.Headers.Contains("X-Api-Key") Then
                Return HarnessTestResult.Failed("Missing X-Api-Key header.")
            End If
            Dim keyVals As New List(Of String)(req.Headers.GetValues("X-Api-Key"))
            If keyVals.Count = 0 OrElse String.IsNullOrWhiteSpace(keyVals(0)) Then
                Return HarnessTestResult.Failed("X-Api-Key header is empty.")
            End If

            body = If(okHandler.CapturedBody, "")
            For Each expected In New String() {"""db_name""", "000_DEMO", """rows""", """Cod""", """Descriere""", """Stare"""}
                If Not body.Contains(expected) Then
                    Return HarnessTestResult.Failed($"Body missing '{expected}'.", body)
                End If
            Next
            If body.Contains("CodAngajament") Then
                Return HarnessTestResult.Failed("Body contains 'CodAngajament' — contract requires 'Cod'.", body)
            End If
        End Using

        ' ---- PART 2: error path (HTTP 500) — the client MUST throw (no swallow) ----
        ' ApiClient only retries transport errors (HttpRequestException/timeout); a 500
        ' response becomes an immediate ApplicationException.
        Dim errHandler As New CapturingHandler(HttpStatusCode.InternalServerError, "{""error"":""boom""}")
        Using http As New HttpClient(errHandler) With {.BaseAddress = New Uri("http://test.local/")}
            Dim opts As New ApiOptions() With {.BaseUrl = "http://test.local/", .ApiKey = "TEST-KEY"}
            Dim errClient As IApiClient = New ApiClient(http, opts)

            Dim threw As Boolean = False
            Try
                Await errClient.UpsertAngajamenteAsync("000_DEMO", rows, ct)
            Catch ex As ApplicationException
                threw = True   ' expected: non-2xx must surface as an exception
            End Try
            If Not threw Then
                Return HarnessTestResult.Failed(
                    "Client did NOT throw on HTTP 500 — violates the no-swallow rule.")
            End If
        End Using

        Return HarnessTestResult.Passed(
            $"POST to correct URL, X-Api-Key present, body keys correct, throws on 500. body={body}")
    End Function

    ' Capturing transport: records the outgoing request, returns a canned response.
    Private NotInheritable Class CapturingHandler
        Inherits HttpMessageHandler

        Public Property CapturedRequest As HttpRequestMessage
        Public Property CapturedBody As String

        Private ReadOnly _status As HttpStatusCode
        Private ReadOnly _responseBody As String

        Public Sub New(status As HttpStatusCode, responseBody As String)
            _status = status
            _responseBody = responseBody
        End Sub

        Protected Overrides Async Function SendAsync(
            request As HttpRequestMessage,
            cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)

            CapturedRequest = request
            If request.Content IsNot Nothing Then
                CapturedBody = Await request.Content.ReadAsStringAsync(cancellationToken)
            End If
            Return New HttpResponseMessage(_status) With {
                .Content = New StringContent(_responseBody, Encoding.UTF8, "application/json")
            }
        End Function
    End Class
End Class
