Option Strict On

' Eroare de API cu mesaj destinat utilizatorului (deja în română — fie mesajul
' serverului, fie un fallback pe codul de status). AuthApi NU înghite niciodată un
' răspuns non-2xx: îl transformă în ApiException. UI-ul (LoginForm) o prinde separat
' de Exception generic ca să afișeze textul serverului direct operatorului.
Public NotInheritable Class ApiException
    Inherits Exception

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

    Public Sub New(message As String, inner As Exception)
        MyBase.New(message, inner)
    End Sub
End Class
