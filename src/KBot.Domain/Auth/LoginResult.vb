Imports System.Text.Json.Serialization

' Raspunsul /api/auth/login: session_id + SessionContext.
Public NotInheritable Class LoginResult
    <JsonPropertyName("session_id")> Public Property SessionId As Integer
    <JsonPropertyName("SessionContext")> Public Property SessionContext As SessionContextDto
End Class
