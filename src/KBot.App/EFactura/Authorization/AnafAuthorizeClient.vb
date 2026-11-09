Option Strict On
Imports System.Net.Http
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web
Imports KBot.Common

''' <summary>
''' The one call that must be made from the operator's PC (slice 00EF-05): ANAF's authorise address, over TLS with the
''' CLIENT CERTIFICATE (the qualified certificate on the token / card). ANAF's login answers with a redirect chain that
''' ends on the application's redirect address with <c>?code=...</c>; the code is read from that FINAL address.
''' No browser is involved.
'''
''' <para>Ported from <c>EF.EXE</c> (<c>GetAuthorizationCode</c>, <c>Surse\EF_SURSA\Program.vb</c>, read 06.10.2026): the
''' same call, the same three attempts. ONE DELIBERATE DIFFERENCE: <c>EF.EXE</c> accepted any server certificate
''' (<c>ServerCertificateCustomValidationCallback = True</c>); here ANAF's server certificate is checked like any other.
''' If a PC does not trust it, the failure says so and that is the first thing to look at (UNVERIFIED on a real
''' machine).</para>
'''
''' <para>The certificate's private key never leaves the token: Windows' TLS layer asks the token's software for the
''' signature, and for the PIN, itself. The code is returned to the caller and never logged; neither is the address
''' (its query is the code).</para>
''' </summary>
Public NotInheritable Class AnafAuthorizeClient

    ''' <summary>Same as the old EF.EXE: the call is repeated while ANAF answers without a code.</summary>
    Public Const MaxAttempts As Integer = 3

    ' The PIN prompt of the token is inside this wait.
    Private Shared ReadOnly CallTimeout As TimeSpan = TimeSpan.FromMinutes(2)

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Calls <paramref name="k_authorizeUrl"/> with <paramref name="k_certificate"/> and returns the one-time code.
    ''' Throws <see cref="EFacturaException"/> (Romanian text) when the connection fails or ANAF gives no code.
    ''' </summary>
    Public Shared Async Function GetCodeAsync(k_authorizeUrl As String, k_certificate As X509Certificate2,
                                              ct As CancellationToken) As Task(Of String)
        ArgumentNullException.ThrowIfNull(k_certificate)
        Dim k_uri As Uri = Nothing
        If Not Uri.TryCreate(k_authorizeUrl, UriKind.Absolute, k_uri) OrElse k_uri.Scheme <> Uri.UriSchemeHttps Then
            Throw New EFacturaException("Adresa de autorizare primită de la server nu este o adresă https. Contactați administratorul.")
        End If

        Try
            Using k_handler As New HttpClientHandler()
                k_handler.AllowAutoRedirect = True
                k_handler.MaxAutomaticRedirections = 10
                k_handler.UseCookies = True
                k_handler.ClientCertificateOptions = ClientCertificateOption.Manual
                k_handler.ClientCertificates.Add(k_certificate)
                Using k_client As New HttpClient(k_handler)
                    k_client.Timeout = CallTimeout
                    For k_attempt As Integer = 1 To MaxAttempts
                        Using k_response As HttpResponseMessage = Await k_client.GetAsync(k_uri, ct).ConfigureAwait(True)
                            Dim k_final As Uri = k_response.RequestMessage?.RequestUri
                            If k_response.IsSuccessStatusCode AndAlso k_final IsNot Nothing Then
                                Dim k_code As String = CodeFromQuery(k_final.Query)
                                If k_code.Length > 0 Then Return k_code
                            End If
                        End Using
                    Next
                End Using
            End Using
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            ' Risky / boundary (TLS with a hardware certificate): log and rethrow as a message the operator can act on.
            ' The exception text of HttpClient does not carry the address query; the code is not in it.
            GlobalErrorLog.Write("AnafAuthorizeClient.GetCodeAsync", ex)
            Throw New EFacturaException(
                "Conexiunea cu ANAF folosind certificatul nu a reușit. Verificați că tokenul sau cardul este conectat, " &
                "că PIN-ul a fost introdus corect și că aveți conexiune la internet.", ex)
        End Try

        Throw New EFacturaException(
            "ANAF nu a întors codul de autorizare după " & MaxAttempts & " încercări. Încercați din nou mai târziu.")
    End Function

    ''' <summary>The value of <c>code</c> in a query string (with or without the leading «?»); empty when there is none.</summary>
    Public Shared Function CodeFromQuery(k_query As String) As String
        If String.IsNullOrEmpty(k_query) Then Return String.Empty
        Dim k_values As Collections.Specialized.NameValueCollection = HttpUtility.ParseQueryString(k_query)
        Return If(k_values("code"), String.Empty).Trim()
    End Function

End Class
