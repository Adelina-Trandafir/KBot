Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports Xunit
Imports KBot.Api
Imports KBot.Domain

' Offline tests for UpdateApi (slice 0067): a stub handler answers the two routes,
' no network. Pinned here: the routes and the ABSENCE of an Authorization header
' (the check runs before login), 404 -> Nothing, the JSON contract, and the
' download's integrity rule -- a wrong sha leaves NO file behind.
Public Class UpdateApiTests

    Private NotInheritable Class StubHandler
        Inherits HttpMessageHandler

        Public Property Status As HttpStatusCode = HttpStatusCode.OK
        Public Property ResponseBody As Byte() = Encoding.UTF8.GetBytes("{}")
        Public Property ContentType As String = "application/json"
        Public Property LastRequestUri As Uri
        Public Property LastMethod As HttpMethod
        Public Property LastAuthorization As String
        Public Property Calls As Integer

        Protected Overrides Function SendAsync(request As HttpRequestMessage,
                                               cancellationToken As CancellationToken) _
            As Task(Of HttpResponseMessage)
            Calls += 1
            LastRequestUri = request.RequestUri
            LastMethod = request.Method
            LastAuthorization = If(request.Headers.Authorization IsNot Nothing,
                                   request.Headers.Authorization.ToString(), Nothing)
            Dim content As New ByteArrayContent(ResponseBody)
            content.Headers.ContentType = New System.Net.Http.Headers.MediaTypeHeaderValue(ContentType)
            Return Task.FromResult(New HttpResponseMessage(Status) With {.Content = content})
        End Function
    End Class

    Private Shared Function NewApi(handler As StubHandler) As UpdateApi
        Dim http As New HttpClient(handler) With {.BaseAddress = New Uri("https://localhost/")}
        Return New UpdateApi(http)
    End Function

    Private Shared Function Sha(bytes As Byte()) As String
        Return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
    End Function

    Private Shared Function TempFile() As String
        Return Path.Combine(Path.GetTempPath(), "kbot_update_test_" & Guid.NewGuid().ToString("N") & ".zip")
    End Function

    ' ---------------------------------------------------------------- GetLatestAsync

    <Fact>
    Public Async Function GetLatest_GetsRouteWithoutAuthorization_AndDeserializes() As Task
        Dim h As New StubHandler With {
            .ResponseBody = Encoding.UTF8.GetBytes(
                "{""version"":""1.0.31.0"",""minimum"":""1.0.30.0"",""file"":""KBot_1.0.31.0.zip""," &
                """size"":12345,""sha256"":""abc"",""published_utc"":""2026-09-18T10:00:00Z"",""notes"":""Notă ăâîșț""}")
        }
        Dim api = NewApi(h)

        Dim info As UpdateInfo = Await api.GetLatestAsync(CancellationToken.None)

        Assert.Equal(HttpMethod.Get, h.LastMethod)
        Assert.Equal("/api/update/latest", h.LastRequestUri.AbsolutePath)
        Assert.Null(h.LastAuthorization)
        Assert.Equal("1.0.31.0", info.Version)
        Assert.Equal("1.0.30.0", info.Minimum)
        Assert.Equal("KBot_1.0.31.0.zip", info.File)
        Assert.Equal(12345L, info.Size)
        Assert.Equal("abc", info.Sha256)
        Assert.Equal("2026-09-18T10:00:00Z", info.PublishedUtc)
        Assert.Equal("Notă ăâîșț", info.Notes)
    End Function

    <Fact>
    Public Async Function GetLatest_404_ReturnsNothing_NotAnException() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.NotFound,
            .ResponseBody = Encoding.UTF8.GetBytes("{""error"":""Nu există nicio actualizare publicată."",""reason"":""NO_UPDATE_PUBLISHED""}")
        }
        Dim info = Await NewApi(h).GetLatestAsync(CancellationToken.None)
        Assert.Null(info)
    End Function

    <Fact>
    Public Async Function GetLatest_500_ThrowsApiExceptionWithServerMessage() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.InternalServerError,
            .ResponseBody = Encoding.UTF8.GetBytes("{""error"":""Descrierea actualizării de pe server este invalidă."",""reason"":""LATEST_INVALID""}")
        }
        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await NewApi(h).GetLatestAsync(CancellationToken.None))
        Assert.True(ex.StatusCode.HasValue)
        Assert.Equal(500, ex.StatusCode.Value)
        Assert.Equal("LATEST_INVALID", ex.Reason)
        Assert.Equal("Descrierea actualizării de pe server este invalidă.", ex.Message)
    End Function

    <Fact>
    Public Async Function GetLatest_200WithMissingKeys_ThrowsLatestInvalid() As Task
        Dim h As New StubHandler With {.ResponseBody = Encoding.UTF8.GetBytes("{""version"":""1.0.31.0""}")}
        Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await NewApi(h).GetLatestAsync(CancellationToken.None))
        Assert.Equal("LATEST_INVALID", ex.Reason)
    End Function

    <Fact>
    Public Async Function GetLatest_WithoutBaseAddress_ThrowsApiException() As Task
        Dim api As New UpdateApi(New HttpClient(New StubHandler()))
        Await Assert.ThrowsAsync(Of ApiException)(
            Async Function() Await api.GetLatestAsync(CancellationToken.None))
    End Function

    ' ---------------------------------------------------------------- DownloadAsync

    <Fact>
    Public Async Function Download_StreamsToFile_ReportsProgress_AndVerifiesSha() As Task
        Dim payload(200_000 - 1) As Byte
        Call New Random(7).NextBytes(payload)
        Dim h As New StubHandler With {.ResponseBody = payload, .ContentType = "application/zip"}
        Dim dest As String = TempFile()
        Dim reports As New List(Of Long)()
        Try
            Await NewApi(h).DownloadAsync(dest, Sha(payload),
                                          New Progress(Of Long)(Sub(n)
                                                                    SyncLock reports
                                                                        reports.Add(n)
                                                                    End SyncLock
                                                                End Sub),
                                          CancellationToken.None)

            Assert.Equal(HttpMethod.Get, h.LastMethod)
            Assert.Equal("/api/update/download", h.LastRequestUri.AbsolutePath)
            Assert.Null(h.LastAuthorization)
            Assert.True(File.Exists(dest))
            Assert.Equal(payload, File.ReadAllBytes(dest))
            ' Progress is posted via SynchronizationContext; give it a moment before asserting.
            Await Task.Delay(100)
            SyncLock reports
                Assert.NotEmpty(reports)
                Assert.Equal(payload.LongLength, reports(reports.Count - 1))
            End SyncLock
        Finally
            If File.Exists(dest) Then File.Delete(dest)
        End Try
    End Function

    <Fact>
    Public Async Function Download_ShaIsCaseInsensitive() As Task
        Dim payload As Byte() = Encoding.UTF8.GetBytes("package bytes")
        Dim h As New StubHandler With {.ResponseBody = payload}
        Dim dest As String = TempFile()
        Try
            Await NewApi(h).DownloadAsync(dest, Sha(payload).ToUpperInvariant(), Nothing, CancellationToken.None)
            Assert.True(File.Exists(dest))
        Finally
            If File.Exists(dest) Then File.Delete(dest)
        End Try
    End Function

    <Fact>
    Public Async Function Download_ShaMismatch_ThrowsAndLeavesNoFile() As Task
        Dim payload As Byte() = Encoding.UTF8.GetBytes("package bytes")
        Dim h As New StubHandler With {.ResponseBody = payload}
        Dim dest As String = TempFile()
        Try
            Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
                Async Function()
                    Await NewApi(h).DownloadAsync(dest, New String("0"c, 64), Nothing, CancellationToken.None)
                End Function)
            Assert.Equal("SHA_MISMATCH", ex.Reason)
            Assert.False(File.Exists(dest))
        Finally
            If File.Exists(dest) Then File.Delete(dest)
        End Try
    End Function

    <Fact>
    Public Async Function Download_ServerError_ThrowsWithServerMessage_AndLeavesNoFile() As Task
        Dim h As New StubHandler With {
            .Status = HttpStatusCode.InternalServerError,
            .ResponseBody = Encoding.UTF8.GetBytes("{""error"":""Pachetul de actualizare lipsește de pe server."",""reason"":""PACKAGE_MISSING""}")
        }
        Dim dest As String = TempFile()
        Try
            Dim ex = Await Assert.ThrowsAsync(Of ApiException)(
                Async Function()
                    Await NewApi(h).DownloadAsync(dest, "abc", Nothing, CancellationToken.None)
                End Function)
            Assert.Equal("PACKAGE_MISSING", ex.Reason)
            Assert.Equal("Pachetul de actualizare lipsește de pe server.", ex.Message)
            Assert.False(File.Exists(dest))
        Finally
            If File.Exists(dest) Then File.Delete(dest)
        End Try
    End Function

    <Fact>
    Public Async Function Download_RejectsMissingArguments() As Task
        Dim api = NewApi(New StubHandler())
        Await Assert.ThrowsAsync(Of ArgumentException)(
            Async Function()
                Await api.DownloadAsync("", "abc", Nothing, CancellationToken.None)
            End Function)
        Await Assert.ThrowsAsync(Of ArgumentException)(
            Async Function()
                Await api.DownloadAsync(TempFile(), "", Nothing, CancellationToken.None)
            End Function)
    End Function
End Class
