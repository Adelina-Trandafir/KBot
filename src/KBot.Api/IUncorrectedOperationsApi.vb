Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' The «Operatiuni necorectate» of the FOREXE landing page (slice 0084):
''' <c>POST /api/forexe/operatiuni/necorectate</c>. Kept out of <see cref="IApiClient"/> on
''' purpose, like <see cref="IMarcajApi"/> - one call, one caller (the shell after the FOREXE
''' login). <see cref="ApiClient"/> implements both.
''' </summary>
Public Interface IUncorrectedOperationsApi

    ''' <summary>
    ''' Saves the rows into <c>FX_Operatiuni</c> of the session's database. Only rows not there
    ''' yet (same treasury reference + document number) are inserted; nothing is deleted.
    ''' Throws <see cref="ApiException"/> on any non-2xx.
    ''' </summary>
    Function SaveUncorrectedOperationsAsync(rows As IReadOnlyList(Of UncorrectedOperation),
                                            ct As CancellationToken) As Task(Of UncorrectedOperationsSaveResult)

    ''' <summary>
    ''' Slice 0088: the «ERRRRRRRRRR» operations stored in <c>FX_Operatiuni</c> that no CAB
    ''' correction note covers yet (GET /api/forexe/operatiuni/necorelate). Read from the database,
    ''' not from FOREXE. Throws <see cref="ApiException"/> on any non-2xx.
    ''' </summary>
    Function GetUncorrelatedOperationsAsync(ct As CancellationToken) As Task(Of List(Of UncorrectedOperation))

End Interface

''' <summary>What the server did with the rows sent.</summary>
Public NotInheritable Class UncorrectedOperationsSaveResult
    Public Property Received As Integer
    Public Property Inserted As Integer
    Public Property AlreadySaved As Integer
    ''' <summary>Rows not saved and why (Romanian, operator-facing).</summary>
    Public Property Warnings As New List(Of String)()
    ''' <summary>Slice 0088: FX_Operatiuni.IDFXP of the rows THIS save inserted (the new ones).</summary>
    Public Property NewIds As New List(Of Integer)()
End Class
