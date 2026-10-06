Option Strict On
Imports System.Globalization
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 00EF-05 -- /api/efactura/token/start, /cod, /stare: PYTHON/routes/efactura/token_routes.py (slice 00EF-04).
' Wire names are exactly the server's (ASCII, lower case; `_json` keeps property names unchanged).
Partial Public Class ApiClient
    Implements IEFacturaApi

    Private NotInheritable Class EFacturaStartWire
        Public Property authorize_url As String
        Public Property state As String
        Public Property expira_in_secunde As Integer
    End Class

    Private NotInheritable Class EFacturaStateWire
        Public Property configurat As Boolean
        Public Property exista As Boolean
        Public Property cui As String
        Public Property valabil_pana As String
        Public Property avertizeaza_de_la As String
        Public Property zile_ramase As Integer?
        Public Property avertizeaza As Boolean
        Public Property trebuie_reinnoit As Boolean
        Public Property certificat As String
        Public Property autorizat_de As String
        Public Property autorizat_la As String
        Public Property ultima_eroare As String
        Public Property ultima_eroare_la As String
    End Class

    ' Request of /cod. The code is secret-adjacent: it is serialized into the body and nowhere else
    ' (never logged, never kept in a field).
    Private NotInheritable Class EFacturaCodeRequest
        Public Property state As String
        Public Property code As String
        Public Property certificat As String
        Public Property amprenta As String
    End Class

    Public Async Function GetTokenStateAsync(ct As CancellationToken) _
        As Task(Of EFacturaTokenState) Implements IEFacturaApi.GetTokenStateAsync
        Try
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Get, "/api/efactura/token/stare", Nothing,
                                                             "citirea stării tokenului E-Factura", ct).ConfigureAwait(False)
            Return ToTokenState(respText)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetTokenStateAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function StartTokenAuthorizationAsync(ct As CancellationToken) _
        As Task(Of EFacturaTokenStart) Implements IEFacturaApi.StartTokenAuthorizationAsync
        Try
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Post, "/api/efactura/token/start", "{}",
                                                             "pornirea autorizării E-Factura", ct).ConfigureAwait(False)
            Dim wire As EFacturaStartWire = JsonSerializer.Deserialize(Of EFacturaStartWire)(respText, _json)
            If wire Is Nothing OrElse String.IsNullOrWhiteSpace(wire.authorize_url) OrElse String.IsNullOrWhiteSpace(wire.state) Then
                Throw New ApiException("Serverul nu a trimis adresa de autorizare ANAF.")
            End If
            Return New EFacturaTokenStart With {
                .AuthorizeUrl = wire.authorize_url,
                .State = wire.state,
                .ExpiresInSeconds = wire.expira_in_secunde}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.StartTokenAuthorizationAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SubmitTokenCodeAsync(k_state As String, k_code As String, k_certLabel As String,
                                               k_thumbprint As String, ct As CancellationToken) _
        As Task(Of EFacturaTokenState) Implements IEFacturaApi.SubmitTokenCodeAsync
        Try
            If String.IsNullOrWhiteSpace(k_state) Then Throw New ArgumentException("The state is required.", NameOf(k_state))
            If String.IsNullOrWhiteSpace(k_code) Then Throw New ArgumentException("The code is required.", NameOf(k_code))
            Dim body As String = JsonSerializer.Serialize(New EFacturaCodeRequest() With {
                .state = k_state, .code = k_code, .certificat = k_certLabel, .amprenta = k_thumbprint}, _json)
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Post, "/api/efactura/token/cod", body,
                                                             "trimiterea codului de autorizare ANAF", ct).ConfigureAwait(False)
            Return ToTokenState(respText)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            ' The type only: the exception text never carries the code, but keep it that way.
            GlobalErrorLog.Write("ApiClient.SubmitTokenCodeAsync",
                                 New InvalidOperationException("E-Factura code not submitted: " & ex.GetType().Name))
            Throw
        End Try
    End Function

    ' One call: bearer header, optional JSON body, the server's Romanian error text on a non-2xx.
    Private Async Function SendEFacturaAsync(k_method As HttpMethod, k_path As String, k_body As String,
                                             k_what As String, ct As CancellationToken) As Task(Of String)
        EnsureConfigured()
        Using msg As New HttpRequestMessage(k_method, k_path)
            msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
            If k_body IsNot Nothing Then msg.Content = New StringContent(k_body, Encoding.UTF8, "application/json")
            Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                If Not resp.IsSuccessStatusCode Then
                    Throw BuildApiException(respText, k_what, CInt(resp.StatusCode))
                End If
                Return respText
            End Using
        End Using
    End Function

    Private Shared Function ToTokenState(k_json As String) As EFacturaTokenState
        Dim w As EFacturaStateWire = JsonSerializer.Deserialize(Of EFacturaStateWire)(k_json, _json)
        If w Is Nothing Then Throw New ApiException("Serverul nu a trimis starea tokenului E-Factura.")
        Return New EFacturaTokenState With {
            .Configured = w.configurat,
            .Exists = w.exista,
            .Cui = If(w.cui, String.Empty),
            .ValidUntilUtc = ParseUtc(w.valabil_pana),
            .WarnFromUtc = ParseUtc(w.avertizeaza_de_la),
            .DaysLeft = w.zile_ramase,
            .ShouldWarn = w.avertizeaza,
            .MustRenew = w.trebuie_reinnoit,
            .CertificateLabel = If(w.certificat, String.Empty),
            .AuthorizedBy = If(w.autorizat_de, String.Empty),
            .AuthorizedAtUtc = ParseUtc(w.autorizat_la),
            .LastError = If(w.ultima_eroare, String.Empty),
            .LastErrorAtUtc = ParseUtc(w.ultima_eroare_la)}
    End Function

    ' The server writes UTC as ISO text ending in Z; an empty or unreadable value is "no date".
    Private Shared Function ParseUtc(k_text As String) As Date?
        If String.IsNullOrWhiteSpace(k_text) Then Return Nothing
        Dim moment As Date
        If Date.TryParse(k_text, CultureInfo.InvariantCulture,
                         DateTimeStyles.AdjustToUniversal Or DateTimeStyles.AssumeUniversal, moment) Then
            Return moment
        End If
        Return Nothing
    End Function

End Class
