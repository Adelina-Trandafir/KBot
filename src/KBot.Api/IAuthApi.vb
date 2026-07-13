Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

' Clientul de login al aplicatiei K-BOT. Fluxul:
'   GetUnitsAsync  -> valideaza credentialele, listeaza bazele accesibile (DC + nume)
'   LoginAsync     -> alege o baza (DC), intoarce token bearer opac + identitate + LastSS
'   LogoutAsync    -> revoca token-ul pe server (best-effort la inchidere)
'   GetPeriodsAsync-> catalogul an / SS / CodProgram al bazei (combo-urile MainForm)
'   SaveLastSsAsync-> memoreaza pe server SS-ul ales de utilizator
' Hard-fail (Throw ApiException) la orice raspuns non-2xx; nu inghite niciodata.
Public Interface IAuthApi
    Function GetUnitsAsync(username As String, password As String,
                           ct As CancellationToken) As Task(Of IReadOnlyList(Of UnitInfo))

    Function LoginAsync(username As String, password As String, dc As String,
                        machine As String, ct As CancellationToken) As Task(Of LoginResult)

    Function LogoutAsync(token As String, ct As CancellationToken) As Task

    Function GetPeriodsAsync(token As String, dbName As String,
                             ct As CancellationToken) As Task(Of IReadOnlyList(Of PeriodInfo))

    Function SaveLastSsAsync(token As String, ss As String, ct As CancellationToken) As Task
End Interface
