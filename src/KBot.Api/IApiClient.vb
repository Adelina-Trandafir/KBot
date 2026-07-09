Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

' Singurul loc care va ști BaseUrl / token bearer / retry / timeout / JSON.
Public Interface IApiClient
    ''' <summary>
    ''' Trimite lista de angajamente la /api/forexe/angajamente/upsert.
    ''' Întoarce corpul brut al răspunsului. Hard-fail (Throw) la eroare.
    ''' </summary>
    Function UpsertAngajamenteAsync(dbName As String,
                                    rows As IReadOnlyList(Of Angajament),
                                    ct As CancellationToken) As Task(Of String)
    Function GetAngajamenteAsync(dbName As String, an As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of Angajament))
    Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T)
    Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest, ct As CancellationToken) As Task(Of TResponse)
End Interface
