Option Strict On
Imports System.Text.Json
Imports System.Text.Json.Serialization

' Corpul standard de eroare al API-ului: { "error": "<mesaj român>", "reason": "<cod>" }.
' Parsat în ACELAȘI loc de AuthApi și ApiClient, ca operatorul să vadă mereu mesajul
' serverului (niciodată JSON brut) și stratul App să poată citi codul-motiv de pe
' ApiException. Oricare câmp poate lipsi (server vechi / corp non-JSON).
Friend NotInheritable Class ApiErrorBody

    <JsonPropertyName("error")> Public Property [Error] As String
    <JsonPropertyName("reason")> Public Property Reason As String

    Private Shared ReadOnly _json As JsonSerializerOptions =
        New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True}

    ' Parsează corpul de eroare; nu aruncă niciodată (un corp non-JSON e o cale
    ' normală — cade pe câmpuri goale, iar apelantul folosește un fallback pe cod).
    Public Shared Function Parse(respText As String) As ApiErrorBody
        If String.IsNullOrWhiteSpace(respText) Then Return New ApiErrorBody()
        Try
            Dim body As ApiErrorBody = JsonSerializer.Deserialize(Of ApiErrorBody)(respText, _json)
            Return If(body, New ApiErrorBody())
        Catch
            ' Corpul nu era JSON-ul de eroare așteptat — control-flow, nu eroare.
            Return New ApiErrorBody()
        End Try
    End Function

    ' Mesaj pentru operator: câmpul "error" al serverului dacă există, altfel un
    ' fallback lizibil pe acțiune + cod HTTP.
    Public Function MessageOrFallback(actiune As String, status As Integer) As String
        If Not String.IsNullOrWhiteSpace(Me.Error) Then Return Me.Error
        Return $"Eroare la {actiune} (cod {status})."
    End Function
End Class
