Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' The operation templates of the unit (slice AV01-14): <c>/api/avacont/sabloane-operatii</c> in <c>routes/avacont/configurari/sablon_operatii/</c>.
''' Kept out of <see cref="IApiClient"/>, like <see cref="IJurnaleApi"/>: only the templates window calls it.
''' Every method throws <see cref="ApiException"/> on a non-2xx, with the server's Romanian message.
''' </summary>
Public Interface ISablonOperatiiApi

    ''' <summary>Every template of the unit database with its lines (the window keeps the ones of the title bar's sector+source), and the lists of journals, base accounts and partners.</summary>
    Function GetSabloaneOperatiiAsync(ct As CancellationToken) As Task(Of SabloaneOperatiiCatalog)

    ''' <summary>
    ''' Writes the changed templates in one step: <paramref name="k_sterse"/> are deleted, a template with <see cref="SablonOperatieInfo.IdSablon"/> 0 is added,
    ''' the others are updated together with ALL their lines. Only the templates sent are touched. An invalid template is refused with the server's message.
    ''' </summary>
    Function SaveSabloaneOperatiiAsync(k_sabloane As IReadOnlyList(Of SablonOperatieInfo), k_sterse As IReadOnlyList(Of Integer), ct As CancellationToken) As Task

End Interface
