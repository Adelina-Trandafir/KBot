Option Strict On
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Slice 0104 -- <c>POST /api/access/client-type</c> (routes/access.py): is the client behind this
''' e-mail one with the Access application? PUBLIC on the server -- no bearer -- because it is asked
''' before login (the answer picks the update package, and the update check runs before login).
''' Hard-fail (<see cref="ApiException"/>) on any non-2xx.
''' </summary>
Public Interface IAccessTypeApi

    ''' <summary>
    ''' True = an Access client (the package WITH the Migrator and the Access code), False = not. An
    ''' e-mail the server does not know answers False, not an error. Throws <see cref="ApiException"/>
    ''' on a refused / failed call (the caller treats the answer as unknown).
    ''' </summary>
    Function GetAccessAsync(email As String, ct As CancellationToken) As Task(Of Boolean)
End Interface
