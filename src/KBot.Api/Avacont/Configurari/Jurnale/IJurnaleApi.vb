Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' The accounting journals of the unit (slice AV01-09): <c>/api/avacont/jurnale</c> in <c>routes/avacont/configurari/jurnale/</c>.
''' Kept out of <see cref="IApiClient"/>, like <see cref="IConturiApi"/>: only the journals window calls it.
''' Every method throws <see cref="ApiException"/> on a non-2xx, with the server's Romanian message.
''' </summary>
Public Interface IJurnaleApi

    ''' <summary>Every journal of the unit database (the window keeps the ones of the title bar's sector+source) and every sector+source.</summary>
    Function GetJurnaleAsync(ct As CancellationToken) As Task(Of JurnaleCatalog)

    ''' <summary>
    ''' Writes the list the window shows in one step: <paramref name="k_sterse"/> are deleted, a row with <see cref="JurnalInfo.IdJurnal"/> 0 is added,
    ''' the others are updated. Only the rows sent are touched. A code that exists at the same sector+source, or a journal that is used and cannot be deleted, is refused with the server's message.
    ''' </summary>
    Function SaveJurnaleAsync(k_randuri As IReadOnlyList(Of JurnalInfo), k_sterse As IReadOnlyList(Of Integer), ct As CancellationToken) As Task

End Interface
