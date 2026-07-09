Option Strict On

' Eroare de API cu mesaj destinat utilizatorului (deja în română — fie mesajul
' serverului, fie un fallback pe codul de status). AuthApi/ApiClient NU înghit
' niciodată un răspuns non-2xx: îl transformă în ApiException. StatusCode (opțional)
' poartă codul HTTP, ca stratul App să poată distinge 401 (sesiune expirată ->
' re-login) de orice alt eșec.
Public NotInheritable Class ApiException
    Inherits Exception

    Public ReadOnly Property StatusCode As Integer?

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

    Public Sub New(message As String, statusCode As Integer)
        MyBase.New(message)
        Me.StatusCode = statusCode
    End Sub

    Public Sub New(message As String, inner As Exception)
        MyBase.New(message, inner)
    End Sub
End Class
