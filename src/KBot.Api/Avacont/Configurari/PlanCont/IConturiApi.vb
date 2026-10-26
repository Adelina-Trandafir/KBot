Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Subunits and the chart of accounts (slice AV01-04): <c>/api/avacont/subunitati</c> and
''' <c>/api/avacont/conturi</c> in <c>routes/avacont/</c>. Kept out of <see cref="IApiClient"/>, like
''' <see cref="IGrupeApi"/>: only the accounts window calls it. <see cref="ApiClient"/> implements it.
''' Every method throws <see cref="ApiException"/> on a non-2xx, with the server's Romanian message
''' (and <see cref="ApiException.Reason"/> when the server gives one, see <see cref="ConturiReasons"/>).
''' </summary>
Public Interface IConturiApi

    ''' <summary>Every subunit of the unit. Empty = the unit has none yet (the window asks for the first one).</summary>
    Function GetSubunitatiAsync(ct As CancellationToken) As Task(Of IReadOnlyList(Of SubunitateInfo))

    ''' <summary>
    ''' Saves a subunit and returns its number. <see cref="SubunitateInfo.IdSubunitate"/> 0 = a new one.
    ''' The first subunit of a unit is always the default one.
    ''' </summary>
    Function SaveSubunitateAsync(k_subunitate As SubunitateInfo, ct As CancellationToken) As Task(Of Integer)

    ''' <summary>Deletes a subunit that is not the default one and holds no account.</summary>
    Function DeleteSubunitateAsync(k_idSubunitate As Integer, ct As CancellationToken) As Task

    ''' <summary>
    ''' The chart of accounts of a subunit; <paramref name="k_idSubunitate"/> Nothing = the default subunit.
    ''' A unit with no subunit answers with <see cref="PlanConturiInfo.Subunitate"/> Nothing.
    ''' </summary>
    Function GetPlanConturiAsync(k_idSubunitate As Integer?, ct As CancellationToken) As Task(Of PlanConturiInfo)

    ''' <summary>The base accounts, classes and classifications the account editor offers.</summary>
    Function GetNomenclatorConturiAsync(ct As CancellationToken) As Task(Of NomenclatorConturi)

    ''' <summary>
    ''' Saves an account. An analytic on a 401* / class 6 / class 7 account is first refused with reason
    ''' <see cref="ConturiReasons.ConfirmareAnalitic"/>; ask the operator and send it again with
    ''' <paramref name="k_confirmatAnalitic"/> True.
    ''' </summary>
    Function SaveContAsync(k_cont As ContSave, k_confirmatAnalitic As Boolean, ct As CancellationToken) As Task(Of ContSalvat)

    ''' <summary>The balances and turnovers of the accounts of a subunit for a year; only the accounts that have a stored row.</summary>
    Function GetSolduriAsync(k_idSubunitate As Integer, k_an As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of SoldCont))

    ''' <summary>Writes the opening balance and the turnovers of ONE account for a year (replaces the stored row).</summary>
    Function SaveSoldAsync(k_sold As SoldCont, k_an As Integer, ct As CancellationToken) As Task

    ''' <summary>The active accounts of every subunit: the list a partner's balances are put on (slice AV01-08).</summary>
    Function GetConturiDisponibileAsync(ct As CancellationToken) As Task(Of IReadOnlyList(Of ContDisponibil))

    ''' <summary>The numbers of the partners that have a balance row in a year (the partner list can show them, slice AV01-09).</summary>
    Function GetParteneriCuSolduriAsync(k_an As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of Integer))

    ''' <summary>The balances of one partner for a year (slice AV01-08).</summary>
    Function GetSolduriPartenerAsync(k_idPartener As Integer, k_an As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of SoldPartener))

    ''' <summary>REPLACES the balances of one partner for a year: the rows sent are written, the others of that partner and year are deleted.</summary>
    Function SaveSolduriPartenerAsync(k_idPartener As Integer, k_an As Integer, k_randuri As IReadOnlyList(Of SoldPartener), ct As CancellationToken) As Task

    ''' <summary>Deletes an account used nowhere. One with operations is refused, whatever the user.</summary>
    Function DeleteContAsync(k_idCont As Integer, ct As CancellationToken) As Task

End Interface
