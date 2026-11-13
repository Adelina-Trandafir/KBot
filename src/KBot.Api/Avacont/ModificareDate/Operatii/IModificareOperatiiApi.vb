Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' «Modificare operatii» (slice AV02-05, the Access form ModificareNC1280): <c>/api/avacont/modificare-operatii</c> in
''' <c>routes/avacont/modificare_date/operatii/</c>. Kept out of <see cref="IApiClient"/>: only the «Modificare operatii» page calls it.
''' Every method throws <see cref="ApiException"/> on a non-2xx, with the server's Romanian message.
''' </summary>
Public Interface IModificareOperatiiApi

    ''' <summary>All the posted operations of the subunit and year, one row each.</summary>
    Function GetOperatiiContabileAsync(k_idSubunitate As Integer, k_an As Integer, ct As CancellationToken) As Task(Of OperatiiContabileLista)

    ''' <summary>The info panel of one operation.</summary>
    Function GetOperatieInfoAsync(k_idOper As Integer, ct As CancellationToken) As Task(Of OperatieInfo)

    ''' <summary>Can the operation be deleted, and what to ask the operator.</summary>
    Function VerificaStergereOperatieAsync(k_idOper As Integer, ct As CancellationToken) As Task(Of OperatieStergereVerificare)

    ''' <summary>Can the whole document be deleted, and what to ask the operator.</summary>
    Function VerificaStergereDocumentAsync(k_idDocument As Integer, ct As CancellationToken) As Task(Of OperatieStergereVerificare)

    ''' <summary>Deletes one operation (the server checks again). Returns how many operations went.</summary>
    Function StergeOperatieAsync(k_idOper As Integer, ct As CancellationToken) As Task(Of Integer)

    ''' <summary>Deletes a document with all its operations (the server checks again). Returns how many operations went.</summary>
    Function StergeDocumentAsync(k_idDocument As Integer, ct As CancellationToken) As Task(Of Integer)

End Interface
