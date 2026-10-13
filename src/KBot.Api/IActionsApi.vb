Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

''' <summary>
''' The operator's recent data writes, kept by the server in its audit table (routes/actiuni.py). Kept out of
''' <see cref="IApiClient"/> like <see cref="ISetariApi"/>: <see cref="ApiClient"/> implements it and the shell asks
''' with a TryCast. A server that does not have the route answers an empty list.
''' </summary>
Public Interface IActionsApi

    ''' <summary>GET /api/actiuni -- the last <paramref name="k_count"/> writes of the signed-in operator on the open unit, newest first.</summary>
    Function GetRecentActionsAsync(k_count As Integer, k_ct As CancellationToken) As Task(Of IReadOnlyList(Of RecentAction))

End Interface
