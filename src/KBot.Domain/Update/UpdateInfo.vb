Option Strict On
Imports System.Text.Json.Serialization

''' <summary>
''' What <c>GET /api/update/latest</c> answers (slice 0067): the published version, the
''' lowest version still allowed to run, and the package to fetch. Field names are the
''' server's, verbatim (snake_case, ASCII) -- see <c>PYTHON/routes/update.py</c>.
''' </summary>
Public NotInheritable Class UpdateInfo
    <JsonPropertyName("version")> Public Property Version As String
    <JsonPropertyName("minimum")> Public Property Minimum As String
    <JsonPropertyName("file")> Public Property File As String
    <JsonPropertyName("size")> Public Property Size As Long
    <JsonPropertyName("sha256")> Public Property Sha256 As String
    <JsonPropertyName("published_utc")> Public Property PublishedUtc As String
    <JsonPropertyName("notes")> Public Property Notes As String
    ''' <summary>The Playwright driver package the server also holds, or Nothing when none was published.</summary>
    <JsonPropertyName("driver")> Public Property Driver As DriverInfo
End Class

''' <summary>
''' The Playwright driver (the <c>.playwright</c> folder: node.exe + cli) as its own package. It is not inside
''' the app packages; a client fetches it only when its installed driver version differs.
''' </summary>
Public NotInheritable Class DriverInfo
    <JsonPropertyName("version")> Public Property Version As String
    <JsonPropertyName("file")> Public Property File As String
    <JsonPropertyName("size")> Public Property Size As Long
    <JsonPropertyName("sha256")> Public Property Sha256 As String
End Class
