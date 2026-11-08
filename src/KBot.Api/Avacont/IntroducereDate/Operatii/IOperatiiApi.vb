Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' The posting windows of Avacont, stage 2 (slices AV02-03 and AV02-04): «Operatii diverse» (<c>/api/avacont/operatii-diverse</c>) and «Facturi si plati»
''' (<c>/api/avacont/facturi-plati</c>), in <c>routes/avacont/introducere_date/operatii/</c>. Kept out of <see cref="IApiClient"/>: only the
''' <c>OperatiiSabloane</c> window calls it. Every method throws <see cref="ApiException"/> on a non-2xx, with the server's Romanian message.
''' </summary>
Public Interface IOperatiiApi

    ''' <summary>The notes of the subunit and year, the next note number and the last closing.</summary>
    Function GetOperatiiContextAsync(k_idSubunitate As Integer, k_an As Integer, ct As CancellationToken) As Task(Of OperatiiContext)

    ''' <summary>Posts the lines with an amount of a template of type A: a document and an operation each, in one step.</summary>
    Function SalveazaOperatiiDiverseAsync(k_cerere As OperatiiDiverseCerere, ct As CancellationToken) As Task(Of OperatiiDiverseRezultat)

    ''' <summary>The notes, the partners and the classifications with templates, for «Facturi si plati».</summary>
    Function GetFacturiPlatiContextAsync(k_idSubunitate As Integer, k_an As Integer, ct As CancellationToken) As Task(Of FacturiPlatiContext)

    ''' <summary>The unpaid invoices (current and previous year) and the orders of the year of one partner, and the provisional number of the next order.</summary>
    Function GetFacturiPartenerAsync(k_idSubunitate As Integer, k_an As Integer, k_idPartener As Integer, k_idClsf As Integer, ct As CancellationToken) As Task(Of FacturiPartener)

    ''' <summary>Books the typed invoices and/or the payment with its order, in one step. The result carries the REAL order number.</summary>
    Function SalveazaFacturiPlatiAsync(k_cerere As FacturiPlatiCerere, ct As CancellationToken) As Task(Of FacturiPlatiRezultat)

    ''' <summary>Deletes an order that is not locked, together with its payment.</summary>
    Function StergePlataFacturiAsync(k_idOP As Integer, ct As CancellationToken) As Task

End Interface
