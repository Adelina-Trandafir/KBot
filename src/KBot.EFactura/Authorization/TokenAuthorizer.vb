Option Strict On
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' The «authorise step» of the plan, client half (slice 00EF-05). The order is chosen so that nothing reaches the
''' server until the operator has really picked a certificate:
''' <list type="number">
''' <item>list the certificates of the Windows store that qualify (<see cref="CertificateFinder"/>);</item>
''' <item>the operator picks one (the <c>k_pick</c> callback; Nothing = gave up);</item>
''' <item><c>POST /api/efactura/token/start</c> -- the authorise address and a single-use state;</item>
''' <item>the call to that address with the certificate (<see cref="AnafAuthorizeClient"/>) -- the code;</item>
''' <item><c>POST /api/efactura/token/cod</c> -- the server exchanges the code for the tokens and stores them.</item>
''' </list>
''' The PC never receives a token or a secret; the code lives in one local variable for the length of step 5.
'''
''' <para>Call it from the UI thread: <paramref name="k_pick"/> and <paramref name="k_progress"/> run there (the awaits
''' come back to the caller's context).</para>
''' </summary>
Public NotInheritable Class TokenAuthorizer

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate

    ''' <param name="k_api">The API client (<c>TryCast(_apiClient, IEFacturaApi)</c> in the shell).</param>
    ''' <param name="k_gate">The shell's re-login net (<see cref="ReauthGate"/>); <c>ReauthGate.Direct</c> when there is none.</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        _api = k_api
        _gate = k_gate
    End Sub

    ''' <summary>The unit's token state: dates, certificate label, who authorised, last error.</summary>
    Public Async Function GetStateAsync(ct As CancellationToken) As Task(Of EFacturaTokenState)
        Try
            Return Await _gate.RunAsync(Function() _api.GetTokenStateAsync(ct)).ConfigureAwait(True)
        Catch ex As ApiException
            Throw
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("TokenAuthorizer.GetStateAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The whole step. Returns the new state, or Nothing when the operator did not pick a certificate. Throws
    ''' <see cref="EFacturaException"/> (no certificate, the call to ANAF failed) or <see cref="ApiException"/> (the
    ''' server refused, with its Romanian text and reason code).
    ''' </summary>
    ''' <param name="k_pick">Shows the certificates and returns the chosen one, or Nothing.</param>
    ''' <param name="k_progress">Receives a short Romanian line before each step.</param>
    Public Async Function AuthorizeAsync(k_pick As Func(Of IReadOnlyList(Of AnafCertificate), AnafCertificate),
                                         k_progress As Action(Of String),
                                         ct As CancellationToken) As Task(Of EFacturaTokenState)
        ArgumentNullException.ThrowIfNull(k_pick)
        ArgumentNullException.ThrowIfNull(k_progress)
        Dim k_certs As IReadOnlyList(Of AnafCertificate) = Nothing
        Try
            k_progress("Se caută certificatele din Windows…")
            k_certs = Await Task.Run(Function() CertificateFinder.FindEligible(), ct).ConfigureAwait(True)
            If k_certs.Count = 0 Then
                Throw New EFacturaException(
                    "Nu am găsit niciun certificat calificat valabil pe un token sau card conectat la acest calculator. " &
                    "Conectați tokenul, instalați programul lui și încercați din nou.")
            End If

            Dim k_chosen As AnafCertificate = k_pick(k_certs)
            If k_chosen Is Nothing Then Return Nothing

            k_progress("Se cere autorizarea de la server…")
            Dim k_start As EFacturaTokenStart =
                Await _gate.RunAsync(Function() _api.StartTokenAuthorizationAsync(ct)).ConfigureAwait(True)

            k_progress("Conectare la ANAF cu certificatul (introduceți PIN-ul dacă este cerut)…")
            Dim k_code As String =
                Await AnafAuthorizeClient.GetCodeAsync(k_start.AuthorizeUrl, k_chosen.Certificate, ct).ConfigureAwait(True)

            k_progress("Se trimite codul la server…")
            Return Await _gate.RunAsync(
                Function() _api.SubmitTokenCodeAsync(k_start.State, k_code, k_chosen.CommonName, k_chosen.Thumbprint, ct)).ConfigureAwait(True)
        Catch ex As EFacturaException
            Throw
        Catch ex As ApiException
            Throw
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("TokenAuthorizer.AuthorizeAsync", ex)
            Throw
        Finally
            If k_certs IsNot Nothing Then
                For Each k_item As AnafCertificate In k_certs
                    k_item.Dispose()
                Next
            End If
        End Try
    End Function

End Class
