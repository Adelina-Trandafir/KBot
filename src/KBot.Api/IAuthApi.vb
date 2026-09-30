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

    ' Slice 0097 -- the unit selector in the caption bar, both on the bearer token:
    '   GetMyUnitsAsync -> every unit of the logged-in user (DC, name, CF, role);
    '   SwitchUnitAsync -> opens another of them; login-shaped answer, old token revoked.
    Function GetMyUnitsAsync(token As String, ct As CancellationToken) As Task(Of IReadOnlyList(Of UnitInfo))

    Function SwitchUnitAsync(token As String, dc As String, machine As String,
                             ct As CancellationToken) As Task(Of LoginResult)

    ' Slice 0072 -- password change in two steps, both on the bearer token:
    '   RequestPasswordCodeAsync -> the server checks the CURRENT password and e-mails a
    '                               one-time code to the operator's address (the user name);
    '   ChangePasswordAsync      -> current password + that code + the new password.
    Function RequestPasswordCodeAsync(token As String, currentPassword As String,
                                      ct As CancellationToken) As Task(Of PasswordCodeInfo)

    Function ChangePasswordAsync(token As String, currentPassword As String, code As String,
                                 newPassword As String, ct As CancellationToken) As Task(Of PasswordChangeResult)
End Interface
