Option Strict On

''' <summary>
''' One signature ADDED by an upload (slice 0079), as K-BOT read it from the PDF. Sent with the
''' upload (header <c>X-Semnaturi</c>); the server writes one <c>FX_PDF_SEMNATURI</c> row per record,
''' next to who uploaded it, from where and from which computer. POCO -&gt; no Try/Catch.
''' Property names are the JSON keys (ASCII, lower case), shared with PYTHON/routes/forexe/pdf.py.
''' </summary>
Public NotInheritable Class PdfSignatureRecord
    ''' <summary>Full name of the signature field, e.g. <c>form1[0].SubformSemnaturaA[0].SignatureField1[0]</c>.</summary>
    Public Property camp As String = String.Empty
    ''' <summary>Role the field maps to (DDF: A / B / Ordonator; ORD: AB / CD / Ordonator), "" when unknown.</summary>
    Public Property rol As String = String.Empty
    ''' <summary>Signer name from the certificate (e.g. AD.CREDIT), "" when it could not be read.</summary>
    Public Property semnatar As String = String.Empty
    ''' <summary>Signing time stored in the signature, ISO 8601 with offset; "" when absent.</summary>
    Public Property data As String = String.Empty
End Class
