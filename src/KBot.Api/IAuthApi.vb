Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

' Clientul de login al aplicatiei K-BOT. Fluxul in doua faze:
'   GetUnitsAsync  -> valideaza credentialele, listeaza unitatile accesibile
'   LoginAsync     -> alege o unitate, intoarce token-ul bearer opac + context
'   LogoutAsync    -> revoca token-ul pe server (best-effort la inchidere)
' Hard-fail (Throw ApiException) la orice raspuns non-2xx; nu inghite niciodata.
Public Interface IAuthApi
    Function GetUnitsAsync(username As String, password As String,
                           an As Integer?, ct As CancellationToken) _
                           As Task(Of IReadOnlyList(Of UnitInfo))

    Function LoginAsync(username As String, password As String, idUnitate As Integer,
                        pcname As String, ct As CancellationToken) As Task(Of LoginResult)

    Function LogoutAsync(token As String, ct As CancellationToken) As Task
End Interface
