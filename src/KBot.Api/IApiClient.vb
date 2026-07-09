Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
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

    ''' <summary>
    ''' Trimite un Excel (base64) la server pentru conversie în JSON (/api/tools/process_excel).
    ''' Întoarce conținutul câmpului "data" din răspuns. Autorizare: bearer-ul sesiunii
    ''' curente (în ApiClient). Hard-fail (Throw ApiException) la non-2xx.
    ''' </summary>
    Function ProcessExcelAsync(job As ExcelJob, ct As CancellationToken) As Task(Of String)

    Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T)
    Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest, ct As CancellationToken) As Task(Of TResponse)
End Interface
