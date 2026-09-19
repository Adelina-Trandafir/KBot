Option Strict On
Imports System
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' The update channel of the K-BOT server (slice 0067). Both calls are PUBLIC on the
''' server -- no bearer, no key -- because the startup check runs before login, when
''' the client holds no credential at all. Hard-fail (<see cref="ApiException"/>) on any
''' non-2xx except the documented 404 of <see cref="GetLatestAsync"/>.
''' </summary>
Public Interface IUpdateApi

    ''' <summary>
    ''' <c>GET /api/update/latest</c>. Returns <c>Nothing</c> when the server has never
    ''' published an update (404, reason NO_UPDATE_PUBLISHED) -- a normal state, not an error.
    ''' </summary>
    Function GetLatestAsync(ct As CancellationToken) As Task(Of UpdateInfo)

    ''' <summary>
    ''' <c>GET /api/update/download</c>, streamed into <paramref name="destinationPath"/>.
    ''' The SHA-256 is computed over the bytes as they arrive and compared with
    ''' <paramref name="expectedSha256"/>; on a mismatch the file is deleted and an
    ''' <see cref="ApiException"/> (reason SHA_MISMATCH) is thrown, so a corrupt package
    ''' never reaches the updater. <paramref name="progress"/> receives bytes received so far.
    ''' </summary>
    Function DownloadAsync(destinationPath As String, expectedSha256 As String,
                           progress As IProgress(Of Long), ct As CancellationToken) As Task
End Interface
