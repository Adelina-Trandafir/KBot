Option Strict On

' Slice 00EF-05 -- what the E-Factura token routes of the server answer (PYTHON/routes/efactura/,
' slice 00EF-04). Plain data, no logic.
'
' NOTHING SECRET is in these types, because nothing secret ever reaches the PC: the server keeps the
' client secret and both ANAF tokens and answers only dates and labels. The one value that looks
' like a credential is the single-use `State`; it proves a code came back to the unit and user that
' asked for it and opens nothing by itself.

''' <summary>
''' The first answer of the certificate step (<c>POST /api/efactura/token/start</c>): the address to call
''' with the certificate and the single-use state to send back together with the code.
''' </summary>
Public NotInheritable Class EFacturaTokenStart

    ''' <summary>ANAF's authorise address, with the application's public parameters already in it.</summary>
    Public Property AuthorizeUrl As String = String.Empty

    ''' <summary>Single use, valid <see cref="ExpiresInSeconds"/> seconds; goes back with the code.</summary>
    Public Property State As String = String.Empty

    Public Property ExpiresInSeconds As Integer

End Class

''' <summary>
''' What the PC may know about the unit's ANAF token (<c>GET /api/efactura/token/stare</c>, also the answer of
''' the code step). Dates are UTC.
''' </summary>
Public NotInheritable Class EFacturaTokenState

    ''' <summary>False when the server has no ANAF client data yet (nothing can be authorised).</summary>
    Public Property Configured As Boolean

    ''' <summary>The unit has a stored token row (it may still be expired).</summary>
    Public Property Exists As Boolean

    ''' <summary>The tax code the token was issued for, digits only; empty when there is no token.</summary>
    Public Property Cui As String = String.Empty

    ''' <summary>When the refresh token stops working: from then on the certificate step is needed again.</summary>
    Public Property ValidUntilUtc As Date?

    ''' <summary>From this moment the notice is shown (7 days before <see cref="ValidUntilUtc"/>).</summary>
    Public Property WarnFromUtc As Date?

    Public Property DaysLeft As Integer?

    ''' <summary>The token still works but ends soon: show the notice and offer «Reînnoiește tokenul».</summary>
    Public Property ShouldWarn As Boolean

    ''' <summary>No usable token (none, expired, or ANAF refused the renewal): the certificate step is required.</summary>
    Public Property MustRenew As Boolean

    ''' <summary>Name of the certificate used last time, for display.</summary>
    Public Property CertificateLabel As String = String.Empty

    ''' <summary>K-BOT login (e-mail) of who did the last certificate step.</summary>
    Public Property AuthorizedBy As String = String.Empty

    Public Property AuthorizedAtUtc As Date?

    Public Property LastError As String = String.Empty

    Public Property LastErrorAtUtc As Date?

End Class
