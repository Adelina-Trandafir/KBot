Option Strict On
Imports System
Imports System.IO
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' <see cref="IUpdateApi"/> over the shared <see cref="HttpClient"/> (BaseAddress + Timeout
''' from DI, like AuthApi). No Authorization header on either call -- see the interface.
'''
''' <para>The download is streamed to disk, never buffered whole: the package is ~40 MB and
''' the progress form needs the byte count as it grows. The hash is computed on the same
''' pass, so integrity costs no second read.</para>
''' </summary>
Public NotInheritable Class UpdateApi
    Implements IUpdateApi

    Private Const LATEST_PATH As String = "/api/update/latest"
    Private Const DOWNLOAD_PATH As String = "/api/update/download"
    Private Const COPY_BUFFER As Integer = 81920

    Private ReadOnly _http As HttpClient

    Private Shared ReadOnly _json As JsonSerializerOptions =
        New JsonSerializerOptions With {
            .PropertyNamingPolicy = Nothing,
            .PropertyNameCaseInsensitive = True
        }

    Public Sub New(http As HttpClient)
        If http Is Nothing Then Throw New ArgumentNullException(NameOf(http))
        _http = http
    End Sub

    Public Async Function GetLatestAsync(ct As CancellationToken) As Task(Of UpdateInfo) _
        Implements IUpdateApi.GetLatestAsync
        Try
            EnsureConfigured()
            Using msg As New HttpRequestMessage(HttpMethod.Get, LATEST_PATH)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If CInt(resp.StatusCode) = 404 Then Return Nothing
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "verificarea actualizărilor", CInt(resp.StatusCode))
                    End If
                    Dim info As UpdateInfo = JsonSerializer.Deserialize(Of UpdateInfo)(respText, _json)
                    If info Is Nothing OrElse String.IsNullOrWhiteSpace(info.Version) OrElse
                       String.IsNullOrWhiteSpace(info.Minimum) OrElse String.IsNullOrWhiteSpace(info.Sha256) Then
                        Throw New ApiException("Răspuns invalid de la server la verificarea actualizărilor.",
                                               CInt(resp.StatusCode), "LATEST_INVALID")
                    End If
                    Return info
                End Using
            End Using
        Catch ex As ApiException
            ' Typed, operator-facing, handled by the caller -- control flow, not logged here.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("UpdateApi.GetLatestAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function DownloadAsync(destinationPath As String, expectedSha256 As String,
                                        progress As IProgress(Of Long), ct As CancellationToken) As Task _
        Implements IUpdateApi.DownloadAsync
        If String.IsNullOrWhiteSpace(destinationPath) Then Throw New ArgumentException("Calea de destinație lipsește.", NameOf(destinationPath))
        If String.IsNullOrWhiteSpace(expectedSha256) Then Throw New ArgumentException("Suma de control așteptată lipsește.", NameOf(expectedSha256))

        Dim written As Boolean = False
        Try
            EnsureConfigured()
            Dim dir As String = Path.GetDirectoryName(destinationPath)
            If Not String.IsNullOrEmpty(dir) Then Directory.CreateDirectory(dir)

            Using msg As New HttpRequestMessage(HttpMethod.Get, DOWNLOAD_PATH)
                ' Headers first: the body is streamed below, not buffered by HttpClient.
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Dim errText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                        Throw BuildApiException(errText, "descărcarea actualizării", CInt(resp.StatusCode))
                    End If

                    Dim total As Long = 0
                    Using sha As IncrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256)
                        Using src As Stream = Await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(False)
                            Using dst As New FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, COPY_BUFFER, useAsync:=True)
                                written = True
                                Dim buffer(COPY_BUFFER - 1) As Byte
                                Do
                                    Dim n As Integer = Await src.ReadAsync(buffer.AsMemory(0, buffer.Length), ct).ConfigureAwait(False)
                                    If n <= 0 Then Exit Do
                                    Await dst.WriteAsync(buffer.AsMemory(0, n), ct).ConfigureAwait(False)
                                    sha.AppendData(buffer, 0, n)
                                    total += n
                                    progress?.Report(total)
                                Loop
                            End Using
                        End Using

                        Dim actual As String = Convert.ToHexString(sha.GetHashAndReset())
                        If Not String.Equals(actual, expectedSha256.Trim(), StringComparison.OrdinalIgnoreCase) Then
                            Throw New ApiException(
                                "Pachetul de actualizare a sosit corupt: suma de control nu corespunde. Încercați din nou.",
                                CInt(resp.StatusCode), "SHA_MISMATCH")
                        End If
                    End Using
                End Using
            End Using
        Catch ex As ApiException
            If written Then DeleteQuietly(destinationPath)
            Throw
        Catch ex As Exception
            If written Then DeleteQuietly(destinationPath)
            GlobalErrorLog.Write("UpdateApi.DownloadAsync", ex)
            Throw
        End Try
    End Function

    ' A half-written or corrupt package must not stay on disk: the updater would
    ' happily try to apply it. Deletion failing here is not worth a second error.
    Private Shared Sub DeleteQuietly(path As String)
        Try
            If File.Exists(path) Then File.Delete(path)
        Catch ex As Exception
            GlobalErrorLog.Write("UpdateApi.DeleteQuietly", ex)
        End Try
    End Sub

    Private Sub EnsureConfigured()
        If _http.BaseAddress Is Nothing Then
            Throw New ApiException(
            "Configurație lipsă: adresa serverului nu este setată. Contactați administratorul.")
        End If
    End Sub

    Private Shared Function BuildApiException(respText As String, actiune As String, status As Integer) As ApiException
        Dim body As ApiErrorBody = ApiErrorBody.Parse(respText)
        Return New ApiException(body.MessageOrFallback(actiune, status), status, body.Reason)
    End Function
End Class
