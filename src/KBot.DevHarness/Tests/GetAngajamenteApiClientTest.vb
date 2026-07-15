Option Strict On
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

' Offline test: drives the REAL ApiClient.GetAngajamenteAsync through a capturing
' HttpMessageHandler. Verifies the GET URL (db_name/id_unitate/doar_anulate), the bearer
' header, deserialization of the new fields (incl. Surse=null for orphans), and that a
' non-2xx response THROWS ApiException carrying the HTTP status (feeds MainForm.WithReauth).
Public NotInheritable Class GetAngajamenteApiClientTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "API — GET angajamente: URL/id_unitate/bearer + deserializare + throw"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Angajamente"
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

    Private Const TwoRows As String =
        "{""db_name"":""000_DEMO"",""count"":2,""rows"":[" &
        "{""Cod"":""C1"",""Descriere"":""D1"",""Stare"":""În derulare"",""IDDF"":30," &
        "  ""Surse"":""02A;02B"",""Incarcat"":true,""Preluat"":true," &
        "  ""Ascuns"":false,""DataCreare"":""2026-01-18T00:00:00""}," &
        "{""Cod"":""C2"",""Descriere"":""D2"",""Stare"":""Anulat"",""IDDF"":null," &
        "  ""Surse"":null,""Incarcat"":false,""Preluat"":false," &
        "  ""Ascuns"":true,""DataCreare"":null}]}"

    Private Shared Function AuthedSession() As SessionContext
        Dim s As New SessionContext()
        s.Populate("test-op", "TEST-TOKEN", New SessionContextDto With {
            .DbName = "000_DEMO", .CF = "1", .NumeUnitate = "DEMO", .Role = "Contabil"})
        Return s
    End Function

    Public Async Function RunAsync(context As HarnessContext, ct As CancellationToken) _
        As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync

        ' ---- PART 1: happy path — capture request, assert URL/bearer, assert deserialization ----
        Dim okHandler As New CapturingHandler(HttpStatusCode.OK, TwoRows)
        Using http As New HttpClient(okHandler) With {.BaseAddress = New Uri("http://test.local/")}
            Dim client As IApiClient = New ApiClient(http, New ApiOptions(), AuthedSession())

            Dim rows = Await client.GetAngajamenteAsync("000_DEMO", 0, False, ct)

            Dim req As HttpRequestMessage = okHandler.CapturedRequest
            If req Is Nothing Then Return HarnessTestResult.Failed("No request captured.")
            If req.Method <> HttpMethod.Get Then Return HarnessTestResult.Failed($"Expected GET, got {req.Method}.")
            If req.RequestUri Is Nothing OrElse Not req.RequestUri.AbsolutePath.EndsWith("/api/forexe/angajamente") Then
                Return HarnessTestResult.Failed($"Wrong URL path: {req.RequestUri}.")
            End If
            Dim query As String = req.RequestUri.Query
            For Each expected In New String() {"db_name=000_DEMO", "id_unitate=0", "doar_anulate=0"}
                If Not query.Contains(expected) Then Return HarnessTestResult.Failed($"Query missing '{expected}': {query}.")
            Next
            If req.Headers.Authorization Is Nothing OrElse req.Headers.Authorization.ToString() <> "Bearer TEST-TOKEN" Then
                Return HarnessTestResult.Failed("Missing/wrong Authorization header.")
            End If

            If rows Is Nothing OrElse rows.Count <> 2 Then
                Return HarnessTestResult.Failed($"Expected 2 rows, got {If(rows Is Nothing, -1, rows.Count)}.")
            End If
            Dim r0 As Angajament = rows(0)
            If r0.CodAngajament <> "C1" OrElse r0.Surse <> "02A;02B" OrElse Not r0.IDDF.HasValue OrElse r0.IDDF.Value <> 30 _
               OrElse Not r0.Incarcat OrElse Not r0.DataCreare.HasValue OrElse r0.DataCreare.Value <> New Date(2026, 1, 18) Then
                Return HarnessTestResult.Failed($"Row0 fields wrong (Cod={r0.CodAngajament}, Surse={r0.Surse}, IDDF={r0.IDDF}, DataCreare={r0.DataCreare}).")
            End If
            Dim r1 As Angajament = rows(1)
            If r1.CodAngajament <> "C2" OrElse r1.Surse IsNot Nothing OrElse r1.IDDF.HasValue OrElse Not r1.Ascuns OrElse r1.DataCreare.HasValue Then
                Return HarnessTestResult.Failed($"Row1 (orphan) fields wrong (Surse should be null, IDDF/DataCreare should be absent).")
            End If
        End Using

        ' ---- PART 2: doar_anulate + id_unitate flow through the query ----
        Dim anulHandler As New CapturingHandler(HttpStatusCode.OK, "{""db_name"":""000_DEMO"",""count"":0,""rows"":[]}")
        Using http As New HttpClient(anulHandler) With {.BaseAddress = New Uri("http://test.local/")}
            Dim client As IApiClient = New ApiClient(http, New ApiOptions(), AuthedSession())
            Await client.GetAngajamenteAsync("000_DEMO", 7, True, ct)
            Dim query As String = anulHandler.CapturedRequest.RequestUri.Query
            For Each expected In New String() {"id_unitate=7", "doar_anulate=1"}
                If Not query.Contains(expected) Then Return HarnessTestResult.Failed($"doar_anulate query missing '{expected}': {query}.")
            Next
        End Using

        ' ---- PART 3: error path (HTTP 500) MUST throw ApiException(500) ----
        Dim errHandler As New CapturingHandler(HttpStatusCode.InternalServerError, "{""error"":""boom""}")
        Using http As New HttpClient(errHandler) With {.BaseAddress = New Uri("http://test.local/")}
            Dim client As IApiClient = New ApiClient(http, New ApiOptions(), AuthedSession())
            Dim threw As Boolean = False
            Try
                Await client.GetAngajamenteAsync("000_DEMO", 0, False, ct)
            Catch ex As ApiException
                If Not ex.StatusCode.HasValue OrElse ex.StatusCode.Value <> 500 Then
                    Return HarnessTestResult.Failed($"ApiException thrown but StatusCode={If(ex.StatusCode.HasValue, CStr(ex.StatusCode.Value), "<none>")}, expected 500.")
                End If
                threw = True
            End Try
            If Not threw Then Return HarnessTestResult.Failed("Client did NOT throw on HTTP 500 — violates the no-swallow rule.")
        End Using

        Return HarnessTestResult.Passed("GET URL/query/bearer OK; new fields + null Surse deserialized; ApiException(500) on error.")
    End Function

    ' Capturing transport: records the outgoing request, returns a canned response.
    Private NotInheritable Class CapturingHandler
        Inherits HttpMessageHandler

        Public Property CapturedRequest As HttpRequestMessage

        Private ReadOnly _status As HttpStatusCode
        Private ReadOnly _responseBody As String

        Public Sub New(status As HttpStatusCode, responseBody As String)
            _status = status
            _responseBody = responseBody
        End Sub

        Protected Overrides Function SendAsync(request As HttpRequestMessage,
                                               cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
            CapturedRequest = request
            Return Task.FromResult(New HttpResponseMessage(_status) With {
                .Content = New StringContent(_responseBody, Encoding.UTF8, "application/json")
            })
        End Function
    End Class
End Class
