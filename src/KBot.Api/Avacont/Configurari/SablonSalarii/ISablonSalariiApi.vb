Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' The salary templates of the unit (slice AV01-15): <c>/api/avacont/sabloane-salarii</c> in <c>routes/avacont/configurari/sablon_salarii/</c>.
''' Kept out of <see cref="IApiClient"/>, like <see cref="ISablonOperatiiApi"/>: only the templates window calls it.
''' Every method throws <see cref="ApiException"/> on a non-2xx, with the server's Romanian message.
''' </summary>
Public Interface ISablonSalariiApi

    ''' <summary>Every article of the unit database with its lines (the window keeps the ones of the chosen subunit), and the lists of subunits, base accounts and partners.</summary>
    Function GetSabloaneSalariiAsync(ct As CancellationToken) As Task(Of SabloaneSalariiCatalog)

    ''' <summary>
    ''' Writes the changed articles in one step: <paramref name="k_sterse"/> are deleted, an article with <see cref="SablonSalariiArticolInfo.IdArticol"/> 0 is added,
    ''' the others are updated with ALL their lines. An article without a virament, or any other invalid one, is refused with the server's message.
    ''' </summary>
    Function SaveSabloaneSalariiAsync(k_articole As IReadOnlyList(Of SablonSalariiArticolInfo), k_sterse As IReadOnlyList(Of Integer), ct As CancellationToken) As Task

End Interface
