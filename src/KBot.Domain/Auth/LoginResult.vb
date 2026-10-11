Imports System.Text.Json.Serialization

' Raspunsul /api/auth/login: token bearer opac + SessionContext (identitate) +
' LastSS (hint pentru MainForm: ultimul SS ales de utilizator pe aceasta baza;
' poate fi Nothing pana la prima alegere). Nu mai exista session_id.
Public NotInheritable Class LoginResult
    <JsonPropertyName("Token")> Public Property Token As String
    <JsonPropertyName("SessionContext")> Public Property SessionContext As SessionContextDto
    <JsonPropertyName("LastSS")> Public Property LastSS As String
    ' Security plan S1: the password is the initial one; the answer then carries only a restricted token.
    <JsonPropertyName("password_change_required")> Public Property PasswordChangeRequired As Boolean
End Class
