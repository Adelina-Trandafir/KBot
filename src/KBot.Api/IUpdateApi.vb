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
    ''' <remarks>
    ''' No kind named: the server answers with the Access package, the only one that existed before
    ''' slice 0104. K-BOT itself asks with <see cref="GetLatestAsync(Boolean, CancellationToken)"/>.
    ''' </remarks>
    Function GetLatestAsync(ct As CancellationToken) As Task(Of UpdateInfo)

    ''' <summary>
    ''' Slice 0104: <c>GET /api/update/latest?access=0|1</c> -- the latest package of the client's own
    ''' kind (<paramref name="access"/> True = with the Migrator and the Access code, False = without).
    ''' Nothing when nothing was published for that kind.
    ''' </summary>
    Function GetLatestAsync(access As Boolean, ct As CancellationToken) As Task(Of UpdateInfo)

    ''' <summary>
    ''' <c>GET /api/update/download</c>, streamed into <paramref name="destinationPath"/>.
    ''' The SHA-256 is computed over the bytes as they arrive and compared with
    ''' <paramref name="expectedSha256"/>; on a mismatch the file is deleted and an
    ''' <see cref="ApiException"/> (reason SHA_MISMATCH) is thrown, so a corrupt package
    ''' never reaches the updater. <paramref name="progress"/> receives bytes received so far.
    ''' </summary>
    ''' <remarks>No kind named: the Access package, as before slice 0104.</remarks>
    Function DownloadAsync(destinationPath As String, expectedSha256 As String,
                           progress As IProgress(Of Long), ct As CancellationToken) As Task

    ''' <summary>
    ''' Slice 0104: the same download, of the package of the client's own kind
    ''' (<c>GET /api/update/download?access=0|1</c>) -- the kind the version was read for.
    ''' </summary>
    Function DownloadAsync(destinationPath As String, expectedSha256 As String, access As Boolean,
                           progress As IProgress(Of Long), ct As CancellationToken) As Task
End Interface
