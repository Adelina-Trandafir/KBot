' Optiunile clientului HTTP. NU mai exista nicio cheie API client-side (Felia 1
' auth): autentificarea e token bearer opac, emis de server la login.
Public Class ApiOptions
    ' Singurul loc unde e scrisă adresa serverului. Hostname public, nu e secret.
    ' Nu se mai citește din mediu / config de pe PC-ul clientului.
    Public Const DefaultBaseUrl As String = "https://kbot.avatarsoft.ro"

    Public Property BaseUrl As String = DefaultBaseUrl
    Public Property TimeoutSeconds As Integer = 100
    Public Property MaxRetries As Integer = 3
End Class
