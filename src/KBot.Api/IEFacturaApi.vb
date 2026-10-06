Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

''' <summary>
''' Slice 00EF-05 -- the E-Factura token routes (PYTHON/routes/efactura/token_routes.py, slice 00EF-04). Kept out
''' of <see cref="IApiClient"/> like the other side interfaces: <see cref="ApiClient"/> implements it and the
''' shell asks for it with a TryCast. Throws <see cref="ApiException"/> on a non-2xx, with the server's Romanian
''' «error» text and its «reason» code (<c>EF_NECONFIGURAT</c>, <c>STATE_INVALID</c>, <c>ANAF_REFUZ</c>, ...).
'''
''' <para>No method returns, takes or stores a token or a secret: the PC only carries the one-time code from
''' ANAF's login to the server.</para>
''' </summary>
Public Interface IEFacturaApi

    ''' <summary>GET /api/efactura/token/stare -- dates, certificate label, who authorised, last error.</summary>
    Function GetTokenStateAsync(ct As CancellationToken) As Task(Of EFacturaTokenState)

    ''' <summary>POST /api/efactura/token/start -- the authorise address and the single-use state.</summary>
    Function StartTokenAuthorizationAsync(ct As CancellationToken) As Task(Of EFacturaTokenStart)

    ''' <summary>
    ''' POST /api/efactura/token/cod -- sends the one-time code read from ANAF's final address, with the state
    ''' of the start. <paramref name="k_certLabel"/> and <paramref name="k_thumbprint"/> are display text only.
    ''' Answers the new state.
    ''' </summary>
    Function SubmitTokenCodeAsync(k_state As String, k_code As String, k_certLabel As String,
                                  k_thumbprint As String, ct As CancellationToken) As Task(Of EFacturaTokenState)

End Interface
