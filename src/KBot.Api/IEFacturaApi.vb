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

    ''' <summary>POST /api/efactura/clienti/anaf -- the customer fields ANAF knows for a tax code (nothing is stored).</summary>
    Function TakeClientFromAnafAsync(k_codFiscal As String, ct As CancellationToken) As Task(Of EFacturaClient)

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
    ''' <param name="k_attachmentPdf">The classic PDF of the invoice (slice 00EF-14): required by the server when the invoice has
    ''' «Atașează factura originală» ticked, which embeds it in the XML as base64; Nothing otherwise.</param>
    Function SendFacturaAsync(k_idFactura As Integer, k_attachmentPdf As Byte(), ct As CancellationToken) As Task(Of EFacturaTrimitere)

    ''' <summary>
    ''' POST /api/efactura/facturi/{id}/trimite with a <c>corectie</c> -- corrects an ACCEPTED invoice (type 384): only the comment and
    ''' the order reference change, and only once ANAF took the new file.
    ''' </summary>
    Function SendCorectieAsync(k_idFactura As Integer, k_comentarii As String, k_bt13 As String, k_attachmentPdf As Byte(),
                               ct As CancellationToken) As Task(Of EFacturaTrimitere)

    ''' <summary>POST /api/efactura/facturi/{id}/verifica -- reads the state at ANAF of a sent invoice and stores it.</summary>
    Function VerifyFacturaAsync(k_idFactura As Integer, ct As CancellationToken) As Task(Of EFacturaVerificare)

    ''' <summary>
    ''' POST /api/efactura/facturi/{id}/storno -- cancels an accepted invoice: the server writes the storno and the
    ''' replacement described by <paramref name="k_replacement"/> (customer, date, account, lines), both as drafts.
    ''' </summary>
    Function StornoFacturaAsync(k_idFactura As Integer, k_replacement As EFacturaFactura, ct As CancellationToken) As Task(Of EFacturaStornare)

    ''' <summary>GET /api/efactura/facturi/{id}/pdf-anaf -- an accepted invoice drawn by ANAF (the bytes of a PDF).</summary>
    Function GetAnafPdfAsync(k_idFactura As Integer, ct As CancellationToken) As Task(Of Byte())

    ' ── Slice 00EF-18: the received invoices (PYTHON/routes/efactura/primite_routes.py, slice 00EF-17) ──

    ''' <summary>
    ''' POST /api/efactura/primite/sincronizeaza -- downloads the new received messages of the last <paramref name="k_zile"/> days (1-60),
    ''' at most <paramref name="k_limita"/> per call; the answer says how many are left, so the caller repeats it.
    ''' </summary>
    Function SyncPrimiteAsync(k_zile As Integer, k_limita As Integer, ct As CancellationToken) As Task(Of EFacturaSincronizare)

    ''' <summary>GET /api/efactura/primite -- newest first; <paramref name="k_idDdf"/> = those of that DDF (partners' tax codes + manual links).</summary>
    Function GetPrimiteAsync(k_year As Integer?, k_month As Integer?, k_query As String, k_idDdf As Integer?,
                             ct As CancellationToken) As Task(Of List(Of EFacturaPrimita))

    ''' <summary>GET /api/efactura/primite/{id} -- header, lines, VAT rates, notes, messages, embedded files and the DDF links.</summary>
    Function GetPrimitaAsync(k_idPrimita As Integer, ct As CancellationToken) As Task(Of EFacturaPrimitaDetaliu)

    ''' <summary>POST /api/efactura/primite/{id}/citita -- the message is no longer new.</summary>
    Function MarkPrimitaCititaAsync(k_idPrimita As Integer, ct As CancellationToken) As Task

    ''' <summary>GET .../xml -- the XML kept from ANAF.</summary>
    Function GetPrimitaXmlAsync(k_idPrimita As Integer, ct As CancellationToken) As Task(Of Byte())

    ''' <summary>GET .../zip -- the signed archive, downloaded again from ANAF.</summary>
    Function GetPrimitaZipAsync(k_idPrimita As Integer, ct As CancellationToken) As Task(Of Byte())

    ''' <summary>GET .../pdf -- the invoice as ANAF's service draws it from the saved XML.</summary>
    Function GetPrimitaPdfAsync(k_idPrimita As Integer, ct As CancellationToken) As Task(Of Byte())

    ''' <summary>GET .../atasamente/{n} -- the n-th (0-based) file embedded in the XML.</summary>
    Function GetPrimitaAtasamentAsync(k_idPrimita As Integer, k_index As Integer, ct As CancellationToken) As Task(Of Byte())

    ''' <summary>POST .../asociere -- links the invoice to a DDF by the operator's own choice.</summary>
    Function LinkPrimitaAsync(k_idPrimita As Integer, k_idDdf As Integer, ct As CancellationToken) As Task

    ''' <summary>DELETE .../asociere/{iddf} -- removes a manual link.</summary>
    Function UnlinkPrimitaAsync(k_idPrimita As Integer, k_idDdf As Integer, ct As CancellationToken) As Task

    ''' <summary>GET /api/efactura/primite-ddf -- the DDFs a received invoice can be linked to (newest first, at most 300), narrowed by <paramref name="k_query"/> (slice 00EF-20).</summary>
    Function GetDdfAlegereAsync(k_query As String, ct As CancellationToken) As Task(Of List(Of EFacturaDdfAlegere))

End Interface
