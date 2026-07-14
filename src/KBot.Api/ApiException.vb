Option Strict On

' Eroare de API cu mesaj destinat utilizatorului (deja în română — fie mesajul
' serverului, fie un fallback pe codul de status). AuthApi/ApiClient NU înghit
' niciodată un răspuns non-2xx: îl transformă în ApiException. StatusCode (opțional)
' poartă codul HTTP, ca stratul App să poată distinge 401 (sesiune expirată ->
' re-login) de orice alt eșec.
Public NotInheritable Class ApiException
    Inherits Exception

    Public ReadOnly Property StatusCode As Integer?

    ' Cod-motiv stabil, citit din corpul de eroare al serverului (câmpul "reason":
    ' TOKEN_UNKNOWN / EXPIRED_IDLE / EXPIRED_ABSOLUTE / CONTEXT_MISMATCH ...). Permite
    ' stratului App să distingă, la un 401, un token mort de un defect de server
    ' (ex. al doilea 401 imediat după re-login). Poate fi Nothing (server vechi / alt corp).
    Public ReadOnly Property Reason As String

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

    Public Sub New(message As String, statusCode As Integer)
        MyBase.New(message)
        Me.StatusCode = statusCode
    End Sub

    Public Sub New(message As String, statusCode As Integer, reason As String)
        MyBase.New(message)
        Me.StatusCode = statusCode
        Me.Reason = reason
    End Sub

    Public Sub New(message As String, inner As Exception)
        MyBase.New(message, inner)
    End Sub
End Class
