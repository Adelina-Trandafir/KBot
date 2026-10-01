Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

''' <summary>
''' Slice 0100-02 -- the settings the server decides (routes/setari.py, table <c>Setari</c> of the connected
''' database). Kept out of <see cref="IApiClient"/> on purpose, like <see cref="IPrintCountApi"/>:
''' <see cref="ApiClient"/> implements it and the shell asks for it with a TryCast. Throws
''' <see cref="ApiException"/> on a non-2xx, with the server's Romanian «error» text.
''' </summary>
Public Interface ISetariApi

    ''' <summary>GET /api/setari -- every row of the connected unit's <c>Setari</c> table (an empty list when the table is not there yet).</summary>
    Function GetServerSettingsAsync(ct As CancellationToken) As Task(Of IReadOnlyList(Of ServerSettingRow))

End Interface
