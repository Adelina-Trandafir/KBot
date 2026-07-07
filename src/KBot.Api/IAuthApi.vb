Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

' Clientul de login al aplicatiei K-BOT. Fluxul in doua faze:
'   GetUnitsAsync  -> valideaza credentialele, listeaza unitatile accesibile
'   LoginAsync     -> alege o unitate, scrie auditul, intoarce session_id + context
'   LogoutAsync    -> stampileaza LogoutTime pe sesiune
' Hard-fail (Throw ApiException) la orice raspuns non-2xx; nu inghite niciodata.
Public Interface IAuthApi
    Function GetUnitsAsync(username As String, password As String,
                           an As Integer?, ct As CancellationToken) _
                           As Task(Of IReadOnlyList(Of UnitInfo))

    Function LoginAsync(username As String, password As String, idUnitate As Integer,
                        pcname As String, ct As CancellationToken) As Task(Of LoginResult)

    Function LogoutAsync(sessionId As Integer, ct As CancellationToken) As Task
End Interface
