Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' The «Clasificatii bugetare» and «Parteneri» windows (slice 0087):
''' <c>/api/forexe/nomenclatoare/...</c> in <c>routes/forexe/clasificatii_edit.py</c> and
''' <c>routes/forexe/parteneri_edit.py</c>. Kept out of <see cref="IApiClient"/>, like
''' <see cref="IMarcajApi"/>: only those two windows call it, and every fake of the big interface
''' would otherwise have to learn it. <see cref="ApiClient"/> implements both. Every method throws
''' <see cref="ApiException"/> on a non-2xx, with the server's Romanian message.
''' </summary>
Public Interface INomenclatoareApi

    ''' <summary>Every classification of the session's database and the names of the tree levels.</summary>
    Function GetClasificatiiAsync(ct As CancellationToken) As Task(Of ClasificatiiCatalog)

    ''' <summary>The budget of one classification for <paramref name="an"/> and that year's corrections.</summary>
    Function GetBugetClasificatieAsync(idClsf As Integer, an As Integer,
                                       ct As CancellationToken) As Task(Of BugetClasificatie)

    ''' <summary>
    ''' Saves the budget and the corrections of one classification in one transaction and returns
    ''' them as read back from the database.
    ''' </summary>
    Function SaveBugetClasificatieAsync(idClsf As Integer, an As Integer, budget As QuarterAmounts,
                                        corrections As IReadOnlyList(Of RectificareBugetara),
                                        deletedIds As IReadOnlyList(Of Integer),
                                        ct As CancellationToken) As Task(Of BugetClasificatie)

    ''' <summary>The sector-sources of this database and the functional / economic dictionaries.</summary>
    Function GetClasificatiiNomenclatorAsync(an As Integer, ct As CancellationToken) As Task(Of ClasificatiiNomenclator)

    ''' <summary>Adds every sector-source x functional x economic combination not present yet.</summary>
    Function AddClasificatiiAsync(an As Integer, sectorSources As IReadOnlyList(Of String),
                                  functionalCodes As IReadOnlyList(Of String),
                                  economicCodes As IReadOnlyList(Of String),
                                  ct As CancellationToken) As Task(Of ClasificatiiAddResult)

    ''' <summary>Every partner of the session's database, with its codes and the combo lists.</summary>
    Function GetParteneriAsync(ct As CancellationToken) As Task(Of ParteneriCatalog)

    ''' <summary>Adds or updates one partner and its codes; returns its id.</summary>
    Function SavePartenerAsync(request As PartenerSaveRequest, ct As CancellationToken) As Task(Of Integer)

    ''' <summary>Deletes a partner no document uses (the server refuses the others with a 409).</summary>
    Function DeletePartenerAsync(idPartener As Integer, ct As CancellationToken) As Task

End Interface
