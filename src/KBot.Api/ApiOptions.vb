' Optiunile clientului HTTP. NU mai exista nicio cheie API client-side (Felia 1
' auth): autentificarea e token bearer opac, emis de server la login.
Public Class ApiOptions
    ' Singurul loc unde e scrisă adresa serverului. Hostname public, nu e secret.
    ' Nu se mai citește din mediu / config de pe PC-ul clientului.
    Public Const DefaultBaseUrl As String = "https://kbot.avatarsoft.ro"

    Public Property BaseUrl As String = DefaultBaseUrl
    Public Property TimeoutSeconds As Integer = 100
    Public Property MaxRetries As Integer = 3

    ' Gardă https: refuzăm orice adresă ne-https, ca un token bearer să nu plece
    ' niciodată necriptat. Aruncă prin construcție (nu prinde nimic, deci fără
    ' Try/Catch) — apelată la pornire din Program, unde plasele globale o duc la
    ' ShowFatal. Prinde doar o editare greșită viitoare a constantei de mai sus.
    ' Stă aici, lângă adresă, ca să fie testabilă fără a porni aplicația.
    Public Sub EnsureHttpsBaseUrl()
        If String.IsNullOrWhiteSpace(BaseUrl) Then
            Throw New InvalidOperationException(
                "Adresa serverului lipsește. Trebuie să fie o adresă https.")
        End If
        If Not BaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException(
                "Adresa serverului trebuie să folosească https. Valoare: " & BaseUrl)
        End If
    End Sub
End Class
