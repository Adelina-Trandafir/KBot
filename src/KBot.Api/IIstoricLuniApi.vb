Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Deleting the last month of the FOREXE history (Setari > FOREXE, advanced options),
''' the routes of <c>routes/forexe/istoric_luni.py</c>. Kept out of <see cref="IApiClient"/> on
''' purpose, like <see cref="IDdfSendApi"/>: its fakes would all have to learn it. A caller
''' holding an <see cref="IApiClient"/> asks for this one with <c>TryCast</c>.
''' Every method throws <see cref="ApiException"/> on a non-2xx, carrying the server's Romanian text.
''' </summary>
Public Interface IIstoricLuniApi

    ''' <summary>The last month of the history and what deleting it removes; a month with
    ''' <c>Luna</c> empty means the history is empty.</summary>
    Function CitesteUltimaLunaIstoricAsync(ct As CancellationToken) As Task(Of IstoricLunaInfo)

    ''' <summary>Deletes that month. Refused (409) unless it is the LAST month and it has no DDF / ORD.</summary>
    Function StergeLunaIstoricAsync(luna As String, ct As CancellationToken) As Task(Of IstoricLunaInfo)

End Interface
