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
End Class
