Option Strict On
Imports System.Collections.Generic
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

    ' ── Slice 00EF-08: the issued invoices (PYTHON/routes/efactura/factura_routes.py, slice 00EF-06) ──────
    ' The unit is the session's: no method takes a unit. A refusal (a rule, a taken number, a missing table) comes as
    ' an ApiException carrying the server's Romanian text and its «reason» code.

    ''' <summary>GET /api/efactura/furnizor -- the unit as the issuer; Nothing when its data are not filled in yet.</summary>
    Function GetFurnizorAsync(ct As CancellationToken) As Task(Of EFacturaFurnizor)

    ''' <summary>PUT /api/efactura/furnizor -- writes the issuer's data; answers what the server stored.</summary>
    Function SaveFurnizorAsync(k_furnizor As EFacturaFurnizor, ct As CancellationToken) As Task(Of EFacturaFurnizor)

    ''' <summary>POST /api/efactura/furnizor/anaf -- takes the unit's name and address from ANAF ONCE (409 afterwards); answers what the server stored.</summary>
    Function TakeFurnizorFromAnafAsync(ct As CancellationToken) As Task(Of EFacturaFurnizor)

    ''' <summary>GET /api/efactura/furnizor/conturi -- the unit's own IBANs, each with the bank deduced from it (slice 00EF-12).</summary>
    Function GetConturiAsync(ct As CancellationToken) As Task(Of List(Of EFacturaCont))

    ''' <summary>
    ''' PUT /api/efactura/furnizor/conturi -- replaces the whole list with <paramref name="k_conturi"/> (only the IBAN is sent;
    ''' the server checks it and deduces the bank). Answers what it stored. Slice 00EF-12.
    ''' </summary>
    Function SaveConturiAsync(k_conturi As IEnumerable(Of EFacturaCont), ct As CancellationToken) As Task(Of List(Of EFacturaCont))

    ''' <summary>GET /api/efactura/um -- the units of measure (UN/ECE codes), filtered by <paramref name="k_query"/> when given.</summary>
    Function GetUmAsync(k_query As String, ct As CancellationToken) As Task(Of List(Of EFacturaUm))

    ''' <summary>GET /api/efactura/clienti -- the customers by name, filtered by <paramref name="k_query"/> when given.</summary>
    Function GetClientiAsync(k_query As String, ct As CancellationToken) As Task(Of List(Of EFacturaClient))

    ''' <summary>POST (IdClient = 0) or PUT /api/efactura/clienti -- answers the stored customer.</summary>
    Function SaveClientAsync(k_client As EFacturaClient, ct As CancellationToken) As Task(Of EFacturaClient)

    ''' <summary>DELETE /api/efactura/clienti/{id} -- refused by the server while the customer has invoices.</summary>
    Function DeleteClientAsync(k_idClient As Integer, ct As CancellationToken) As Task

    ''' <summary>GET /api/efactura/facturi -- headers newest first, with the customer's name and the total.</summary>
    Function GetFacturiAsync(k_year As Integer?, k_query As String, ct As CancellationToken) As Task(Of List(Of EFacturaFactura))

    ''' <summary>GET /api/efactura/facturi/{id} -- the whole invoice: customer, lines and what may be done with it.</summary>
    Function GetFacturaAsync(k_idFactura As Integer, ct As CancellationToken) As Task(Of EFacturaFactura)

    ''' <summary>GET /api/efactura/facturi/numar-urmator -- the series and number a new invoice gets now.</summary>
    Function GetNextNumberAsync(ct As CancellationToken) As Task(Of EFacturaNumarUrmator)

    ''' <summary>
    ''' POST (IdFactura = 0) or PUT /api/efactura/facturi -- a draft with its lines. Series, number and type are the
    ''' server's; the lines' value is computed there. Answers the stored invoice.
    ''' </summary>
    Function SaveFacturaAsync(k_factura As EFacturaFactura, ct As CancellationToken) As Task(Of EFacturaFactura)

    ''' <summary>DELETE /api/efactura/facturi/{id} -- only a draft with the last number of its series.</summary>
    Function DeleteFacturaAsync(k_idFactura As Integer, ct As CancellationToken) As Task

    ' ── Slice 00EF-09: sending, state, storno and the PDF of ANAF (PYTHON/routes/efactura/trimitere_routes.py, 00EF-07) ──

    ''' <summary>
    ''' POST /api/efactura/facturi/{id}/trimite -- checks, validation at ANAF and upload of a draft. A refusal comes as an
    ''' <see cref="ApiException"/> whose text lists the findings (<c>INVALIDA</c>, <c>RESPINSA_DE_VALIDATOR</c>,
    ''' <c>ANAF_REFUZA_INCARCAREA</c>, <c>TOKEN_NECESAR</c>, ...).
    ''' </summary>
    Function SendFacturaAsync(k_idFactura As Integer, ct As CancellationToken) As Task(Of EFacturaTrimitere)

    ''' <summary>POST /api/efactura/facturi/{id}/verifica -- reads the state at ANAF of a sent invoice and stores it.</summary>
    Function VerifyFacturaAsync(k_idFactura As Integer, ct As CancellationToken) As Task(Of EFacturaVerificare)

    ''' <summary>
    ''' POST /api/efactura/facturi/{id}/storno -- cancels an accepted invoice: the server writes the storno and the
    ''' replacement described by <paramref name="k_replacement"/> (customer, date, account, lines), both as drafts.
    ''' </summary>
    Function StornoFacturaAsync(k_idFactura As Integer, k_replacement As EFacturaFactura, ct As CancellationToken) As Task(Of EFacturaStornare)

    ''' <summary>GET /api/efactura/facturi/{id}/pdf-anaf -- an accepted invoice drawn by ANAF (the bytes of a PDF).</summary>
    Function GetAnafPdfAsync(k_idFactura As Integer, ct As CancellationToken) As Task(Of Byte())

End Interface
