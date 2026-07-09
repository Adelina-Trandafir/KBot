Imports System.Text.Json.Serialization

' Raspunsul /api/auth/login: token bearer opac + SessionContext. Nu mai exista
' session_id (FX_LoginLog a iesit din scop odata cu trecerea pe token).
Public NotInheritable Class LoginResult
    <JsonPropertyName("Token")> Public Property Token As String
    <JsonPropertyName("SessionContext")> Public Property SessionContext As SessionContextDto
End Class
