Imports System.Text.Json.Serialization

' Slice 0072: the two answers of the password-change conversation with the server.
' POCOs, no Try/Catch. Field names are the wire contract (ASCII, both sides).

' Answer of POST /api/auth/password/code: the one-time code went out by e-mail.
Public NotInheritable Class PasswordCodeInfo
    ' The address the code was sent to, MASKED by the server (a***@domain) -- shown to the
    ' operator so they know which inbox to open; never the full address back over the wire.
    <JsonPropertyName("email_masked")> Public Property EmailMasked As String
    ' How long the code stays valid, in seconds.
    <JsonPropertyName("expires_in")> Public Property ExpiresInSeconds As Integer
End Class

' Answer of POST /api/auth/password/change: the password was changed on the K-BOT server.
Public NotInheritable Class PasswordChangeResult
    <JsonPropertyName("ok")> Public Property Ok As Boolean
    ' True when the legacy (Access) server took the new password too; False when it could not
    ' be reached or refused -- the operator then has two passwords until an administrator
    ' aligns them, and must be told.
    <JsonPropertyName("legacy_updated")> Public Property LegacyUpdated As Boolean
End Class
