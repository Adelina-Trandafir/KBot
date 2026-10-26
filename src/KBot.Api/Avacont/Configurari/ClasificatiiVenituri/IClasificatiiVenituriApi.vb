Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' The revenue classifications (slice AV01-11): <c>/api/avacont/clasificatii-venituri</c> in <c>routes/avacont/configurari/clasificatii_venituri/</c>.
''' Kept out of <see cref="IApiClient"/>, like <see cref="IConturiApi"/>: only the «Venituri» page of the «Clasificatii» window calls it.
''' The budget belongs to a UNIT: <c>k_ss</c> (the sector+source of the title bar) and <c>k_an</c> name it on the server. Every method throws
''' <see cref="ApiException"/> on a non-2xx, with the server's Romanian message.
''' </summary>
Public Interface IClasificatiiVenituriApi

    ''' <summary>
    ''' The revenue classifications of the database, with the names of the chapters and sub-chapters. <c>k_ss</c> narrows the list to one
    ''' sector+source; Nothing / empty = every source («Toate» in the title bar).
    ''' </summary>
    Function GetClasificatiiVenituriAsync(k_ss As String, ct As CancellationToken) As Task(Of ClasificatiiVenituriCatalog)

    ''' <summary>The revenue codes of the nomenclator, each with whether it is already a classification of the source <c>k_ss</c> (a concrete one).</summary>
    Function GetNomenclatorVenituriAsync(k_ss As String, ct As CancellationToken) As Task(Of IReadOnlyList(Of CodVenitNomenclator))

    ''' <summary>Adds the classification of a nomenclator code (six characters) to the source <c>k_ss</c>; returns its key. A code that already exists on that source is refused.</summary>
    Function AddClasificatieVenitAsync(k_clsfv As String, k_ss As String, ct As CancellationToken) As Task(Of Integer)

    ''' <summary>Deletes a classification that is used nowhere; the server refuses one with a budget, corrections or bank-statement headers, naming where it is used.</summary>
    Function DeleteClasificatieVenitAsync(k_idClsfV As Integer, ct As CancellationToken) As Task

    ''' <summary>
    ''' The last budget version and the corrections total of every classification that has one, for the unit and the year; plus the child subunits of the
    ''' source, if any (<see cref="VenitSumarUnitate.Copii"/>: they work on the parent's data, the page says so).
    ''' </summary>
    Function GetSumarVenituriAsync(k_an As Integer, k_ss As String, ct As CancellationToken) As Task(Of VenitSumarUnitate)

    ''' <summary>The budget versions and the corrections of one classification, for the unit and the year.</summary>
    Function GetBugetVenitAsync(k_idClsfV As Integer, k_an As Integer, k_ss As String, ct As CancellationToken) As Task(Of BugetClasificatie)

    ''' <summary>Writes the budget versions and the corrections of one classification in one transaction and returns them as read back.</summary>
    Function SaveBugetVenitAsync(k_idClsfV As Integer, k_an As Integer, k_ss As String,
                                 k_budgets As IReadOnlyList(Of BudgetVersion), k_deletedBudgetIds As IReadOnlyList(Of Integer),
                                 k_corrections As IReadOnlyList(Of RectificareBugetara), k_deletedIds As IReadOnlyList(Of Integer),
                                 ct As CancellationToken) As Task(Of BugetClasificatie)

End Interface
