Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Clientul HTTP real. Deține BaseUrl / retry / timeout / JSON. BaseAddress + Timeout
' se setează pe HttpClient la înregistrarea DI (Program.vb); autorizarea e token-ul
' bearer opac citit per-request din SessionContext (singleton, populat de login).
Public Class ApiClient
    Implements IApiClient

    Private ReadOnly _http As HttpClient
    Private ReadOnly _options As ApiOptions
    Private ReadOnly _session As SessionContext

    ' PropertyNamingPolicy=Nothing => numele proprietăților DTO rămân neschimbate
    ' (db_name / Cod / Descriere / Stare), exact ca în contractul rutei Python.
    Private Shared ReadOnly _json As JsonSerializerOptions =
        New JsonSerializerOptions With {.PropertyNamingPolicy = Nothing}

    Public Sub New(http As HttpClient, options As ApiOptions, session As SessionContext)
        If http Is Nothing Then Throw New ArgumentNullException(NameOf(http))
        If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
        If session Is Nothing Then Throw New ArgumentNullException(NameOf(session))
        _http = http
        _options = options
        _session = session
    End Sub

    Public Async Function UpsertAngajamenteAsync(dbName As String,
                                                 rows As IReadOnlyList(Of Angajament),
                                                 ct As CancellationToken) As Task(Of String) Implements IApiClient.UpsertAngajamenteAsync
        ' Plasă de siguranță la limită: logăm orice eșec (rețea/JSON/HTTP) și rearuncăm —
        ' apelantul (App) vede eroarea, dar avem urma completă în harness_errors.log.
        Try
            EnsureConfigured()
            If String.IsNullOrEmpty(dbName) Then Throw New ArgumentException("dbName gol.", NameOf(dbName))
            If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))

            Dim req As New UpsertAngajamenteRequest() With {.db_name = dbName}
            For Each a In rows
                req.rows.Add(New AngajamentRow() With {
                    .Cod = a.CodAngajament,
                    .Descriere = a.Descriere,
                    .Stare = a.Stare
                })
            Next

            Dim body As String = JsonSerializer.Serialize(req, _json)

            Dim maxAttempts As Integer = Math.Max(1, _options.MaxRetries)
            Dim attempt As Integer = 0
            Do
                attempt += 1
                ' VB.NET nu permite Await într-un Catch; marcăm reîncercarea și așteptăm după.
                Dim retryDelay As TimeSpan = TimeSpan.Zero
                Try
                    Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/angajamente/upsert")
                        msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                        msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                        Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                            Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                            If Not resp.IsSuccessStatusCode Then
                                ' ApiException NU e prinsă de catch-urile tranzitorii de mai jos:
                                ' un 401 iese direct spre stratul App (re-login §4.9), fără retry aici.
                                Throw BuildApiException(respText, "upsert angajamente", CInt(resp.StatusCode))
                            End If
                            Return respText
                        End Using
                    End Using
                Catch ex As HttpRequestException When attempt < maxAttempts
                    retryDelay = TimeSpan.FromSeconds(2 * attempt)
                Catch ex As TaskCanceledException When (Not ct.IsCancellationRequested) AndAlso attempt < maxAttempts
                    ' Timeout tranzitoriu (nu anulare de la apelant) — reîncercăm.
                    retryDelay = TimeSpan.FromSeconds(2 * attempt)
                End Try
                Await Task.Delay(retryDelay, ct).ConfigureAwait(False)
            Loop
        Catch ex As ApiException
            ' Excepție tipată, cu sens, tratată de apelant (ex. 401 -> WithReauth):
            ' control-flow, nu eroare — rearuncăm fără să poluăm sink-ul.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.UpsertAngajamenteAsync", ex)
            Throw
        End Try
    End Function

    ' List query for the MainForm list view (mirrors Angajamente_SQL). Filters by
    ' COALESCE(IdUnitate,0)=idUnitate; doarAnulate switches to the anulate/suspendat/
    ' ascuns filter. Hard-fail (Throw ApiException) on non-2xx; a 401 bubbles to
    ' WithReauth (no retry here).
    Public Async Function GetAngajamenteAsync(dbName As String, idUnitate As Integer, doarAnulate As Boolean,
                                              ct As CancellationToken) As Task(Of IReadOnlyList(Of Angajament)) Implements IApiClient.GetAngajamenteAsync
        Try
            EnsureConfigured()
            If String.IsNullOrEmpty(dbName) Then Throw New ArgumentException("dbName gol.", NameOf(dbName))

            Dim url As String = $"/api/forexe/angajamente?db_name={Uri.EscapeDataString(dbName)}&id_unitate={idUnitate}&doar_anulate={If(doarAnulate, 1, 0)}"

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea angajamentelor", CInt(resp.StatusCode))
                    End If

                    Dim payload As GetAngajamenteResponse = JsonSerializer.Deserialize(Of GetAngajamenteResponse)(respText, _json)
                    Dim result As New List(Of Angajament)()
                    If payload IsNot Nothing AndAlso payload.rows IsNot Nothing Then
                        For Each r As GetAngajamenteRow In payload.rows
                            result.Add(New Angajament() With {
                                .CodAngajament = If(r.Cod, String.Empty),
                                .Descriere = If(r.Descriere, String.Empty),
                                .Stare = If(r.Stare, String.Empty),
                                .IDDF = r.IDDF,
                                .Surse = r.Surse,
                                .Incarcat = r.Incarcat,
                                .Preluat = r.Preluat,
                                .Ascuns = r.Ascuns,
                                .DataCreare = r.DataCreare
                            })
                        Next
                    End If
                    Return result
                End Using
            End Using
        Catch ex As ApiException
            ' 401/HTTP tipat, tratat de apelant (WithReauth) — nu logăm.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetAngajamenteAsync", ex)
            Throw
        End Try
    End Function

    ' Tree query for the MainForm tree (slice 0008). Filters by an + SS; includeHidden
    ' brings ASCUNS rows back (btnOpt). The database is NOT sent — the server reads it
    ' from the session (one database = one unit), so a token cannot target another base.
    ' Hard-fail (Throw ApiException) on non-2xx; a 401 bubbles to WithReauth (no retry).
    Public Async Function GetTreeAsync(an As Integer, ss As String, includeHidden As Boolean,
                                       ct As CancellationToken) As Task(Of IReadOnlyList(Of AngajamentTreeInfo)) Implements IApiClient.GetTreeAsync
        Try
            EnsureConfigured()
            If String.IsNullOrEmpty(ss) Then Throw New ArgumentException("ss gol.", NameOf(ss))

            Dim url As String = $"/api/forexe/tree?an={an}&ss={Uri.EscapeDataString(ss)}&include_hidden={If(includeHidden, 1, 0)}"

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea arborelui de angajamente", CInt(resp.StatusCode))
                    End If

                    Dim payload As GetTreeResponse = JsonSerializer.Deserialize(Of GetTreeResponse)(respText, _json)
                    Dim result As New List(Of AngajamentTreeInfo)()
                    If payload IsNot Nothing AndAlso payload.rows IsNot Nothing Then
                        For Each r As GetTreeRow In payload.rows
                            Dim cod As String = If(r.CodAngajament, String.Empty)
                            result.Add(New AngajamentTreeInfo() With {
                                .NodeKey = cod,
                                .Caption = If(r.Descriere, String.Empty),
                                .CodAngajament = cod,
                                .Descriere = If(r.Descriere, String.Empty),
                                .Stare = If(r.Stare, String.Empty),
                                .DataCreare = r.DataCreare,
                                .DataDefinitivare = r.DataDefinitivare,
                                .IDDF = r.IDDF,
                                .EIncarcat = r.Incarcat,
                                .EPreluat = r.Preluat,
                                .Salarii = r.Salarii,
                                .Ascuns = r.Ascuns,
                                .Surse = If(r.Surse, String.Empty),
                                .AreIndicatori = r.AreIndicatori,
                                .AreIstoric = r.AreIstoric,
                                .AreRevizii = r.AreRevizii,
                                .AreRezervari = r.AreRezervari,
                                .AreReceptii = r.AreReceptii,
                                .ArePlati = r.ArePlati,
                                .AreDDF = r.AreDDF,
                                .ArePartener = r.ArePartener,
                                .AreORD = r.AreOrd
                            })
                        Next
                    End If
                    Return result
                End Using
            End Using
        Catch ex As ApiException
            ' 401/HTTP tipat, tratat de apelant (WithReauth) — nu logăm.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetTreeAsync", ex)
            Throw
        End Try
    End Function

    ' Sumarul unui angajament (slice 0011), pentru SumarView. Un singur parametru:
    ' cod = CodAngajament, escapat in query string. NU se trimite baza (o citeste
    ' serverul din sesiune) si NU exista filtru SS — sumarul arata TOTI indicatorii.
    ' Un cod necunoscut intoarce 200 cu header null / rows [], deci aici rezulta un
    ' SumarInfo gol, nu o exceptie: un angajament fara indicatori e legitim.
    ' Hard-fail (Throw ApiException) pe non-2xx; un 401 curge spre WithReauth.
    Public Async Function GetSumarAsync(cod As String, ct As CancellationToken) _
        As Task(Of SumarInfo) Implements IApiClient.GetSumarAsync

        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim url As String = $"/api/forexe/sumar?cod={Uri.EscapeDataString(cod)}"

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea sumarului angajamentului", CInt(resp.StatusCode))
                    End If

                    Dim payload As GetSumarResponse = JsonSerializer.Deserialize(Of GetSumarResponse)(respText, _json)
                    Dim result As New SumarInfo()
                    If payload Is Nothing Then Return result

                    ' Header ramane Nothing daca serverul a trimis null — SumarView
                    ' trateaza asta ca „angajament fara indicatori”, nu ca eroare.
                    If payload.header IsNot Nothing Then
                        result.Header = New SumarHeader() With {
                            .CodAngajament = If(payload.header.cod_angajament, String.Empty),
                            .DataFX = payload.header.data_fx,
                            .DataCreare = payload.header.data_creare,
                            .DataDefinitivare = payload.header.data_definitivare,
                            .Descriere = If(payload.header.descriere, String.Empty),
                            .Stare = If(payload.header.stare, String.Empty),
                            .Incarcat = payload.header.incarcat,
                            .Preluat = payload.header.preluat
                        }
                    End If

                    If payload.rows IsNot Nothing Then
                        For Each r As GetSumarRow In payload.rows
                            result.Rows.Add(New SumarRow() With {
                                .Clsf = If(r.clsf, String.Empty),
                                .CodIndicator = If(r.cod_indicator, String.Empty),
                                .Partener = If(r.partener, String.Empty),
                                .TotalRezervari = r.total_rezervari,
                                .TotalReceptii = r.total_receptii,
                                .TotalPlati = r.total_plati,
                                .TotalRevizii = r.total_revizii,
                                .TotalOrdonantari = r.total_ordonantari
                            })
                        Next
                    End If
                    Return result
                End Using
            End Using
        Catch ex As ApiException
            ' 401/HTTP tipat, tratat de apelant (WithReauth) — nu logăm.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetSumarAsync", ex)
            Throw
        End Try
    End Function

    ' Rezervarile unui angajament (slice 0014), pentru RezervariView. Un singur parametru:
    ' cod = CodAngajament, escapat in query string. NU se trimite baza (o citeste
    ' serverul din sesiune). Un cod fara rezervari intoarce 200 cu rows [], deci aici
    ' rezulta un RezervariInfo gol, nu o exceptie. Hard-fail (Throw ApiException) pe
    ' non-2xx; un 401 curge spre WithReauth.
    Public Async Function GetRezervariAsync(cod As String, ct As CancellationToken) _
        As Task(Of RezervariInfo) Implements IApiClient.GetRezervariAsync

        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim url As String = $"/api/forexe/rezervari?cod={Uri.EscapeDataString(cod)}"

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea rezervărilor angajamentului", CInt(resp.StatusCode))
                    End If

                    Dim payload As GetRezervariResponse = JsonSerializer.Deserialize(Of GetRezervariResponse)(respText, _json)
                    Dim result As New RezervariInfo()
                    If payload Is Nothing OrElse payload.rows Is Nothing Then Return result

                    For Each r As GetRezervareRow In payload.rows
                        result.Rows.Add(New RezervareRow() With {
                            .Idrz = r.idrz,
                            .CodIndicator = If(r.cod_indicator, String.Empty),
                            .Clsf = If(r.clsf, String.Empty),
                            .Denumire = If(r.denumire, String.Empty),
                            .DataRezervare = If(r.data_rezervare.HasValue, r.data_rezervare.Value, Date.MinValue),
                            .RCreditBug = r.r_credit_bug,
                            .RInitiala = r.r_initiala,
                            .RValoare = r.r_valoare,
                            .RDefinitiva = r.r_definitiva,
                            .EInitiala = r.e_initiala,
                            .EMarire = r.e_marire,
                            .EMicsorare = r.e_micsorare,
                            .AreDDF = r.are_ddf
                        })
                    Next
                    Return result
                End Using
            End Using
        Catch ex As ApiException
            ' 401/HTTP tipat, tratat de apelant (WithReauth) — nu logăm.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetRezervariAsync", ex)
            Throw
        End Try
    End Function

    ' Receptiile unui angajament (slice 0015), pentru ReceptiiView. Un singur parametru:
    ' cod = CodAngajament, escapat in query string. NU se trimite baza (o citeste serverul
    ' din sesiune). Un cod fara receptii intoarce 200 cu receptii [], deci aici rezulta un
    ' ReceptiiInfo gol, nu o exceptie. Envelope-ul poarta si `plati` (pentru tooltip, felia
    ' 0015-02) intr-un singur apel. Hard-fail (Throw ApiException) pe non-2xx; un 401 curge
    ' spre WithReauth.
    Public Async Function GetReceptiiAsync(cod As String, ct As CancellationToken) _
        As Task(Of ReceptiiInfo) Implements IApiClient.GetReceptiiAsync

        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim url As String = $"/api/forexe/receptii?cod={Uri.EscapeDataString(cod)}"

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea recepțiilor angajamentului", CInt(resp.StatusCode))
                    End If

                    Dim payload As GetReceptiiResponse = JsonSerializer.Deserialize(Of GetReceptiiResponse)(respText, _json)
                    Dim result As New ReceptiiInfo()
                    If payload Is Nothing Then Return result

                    result.Cod = If(payload.cod, If(cod, String.Empty))

                    If payload.receptii IsNot Nothing Then
                        For Each r As GetReceptieRow In payload.receptii
                            result.Receptii.Add(New ReceptieRow() With {
                                .Idrr = r.idrr,
                                .NrCrtR = r.nrcrt_r,
                                .DataR = r.data_r,
                                .SumaAntet = r.suma_antet,
                                .Incarcat = r.incarcat,
                                .Preluat = r.preluat,
                                .Idrh = r.idrh,
                                .NrCrtH = r.nrcrt_h,
                                .DataH = r.data_h,
                                .Total = r.total,
                                .Difh = r.difh,
                                .StersH = r.sters_h,
                                .DescriereH = If(r.descriere_h, String.Empty),
                                .Idr = r.idr,
                                .IdClsf = r.id_clsf,
                                .CodIndicator = If(r.cod_indicator, String.Empty),
                                .Clsf = If(r.clsf, String.Empty),
                                .NrCrtInd = r.nrcrt_ind,
                                .Valoare = r.valoare,
                                .Dif = r.dif
                            })
                        Next
                    End If

                    If payload.plati IsNot Nothing Then
                        For Each p As GetReceptiePlata In payload.plati
                            result.Plati.Add(New ReceptiePlata() With {
                                .DataPlata = p.data_plata,
                                .Suma = p.suma
                            })
                        Next
                    End If
                    Return result
                End Using
            End Using
        Catch ex As ApiException
            ' 401/HTTP tipat, tratat de apelant (WithReauth) — nu logăm.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetReceptiiAsync", ex)
            Throw
        End Try
    End Function

    ' Conversie Excel -> JSON pe server. FOREXE nu mai face HTTP direct: umple un
    ' ExcelJob și îl dă aici, unde stau adresa, token-ul bearer și POST-ul. Un singur
    ' apel, fără retry (upload base64 mare; reîncercarea e scumpă). Non-2xx -> ApiException
    ' cu status, deci un 401 curge spre App (re-login §4.9), la fel ca celelalte apeluri.
    Public Async Function ProcessExcelAsync(job As ExcelJob, ct As CancellationToken) _
        As Task(Of String) Implements IApiClient.ProcessExcelAsync

        Try
            If job Is Nothing Then Throw New ArgumentNullException(NameOf(job))
            EnsureConfigured()

            Dim payload As New Dictionary(Of String, Object) From {
                {"file_base64", job.FileBase64},
                {"header_rows", job.HeaderRows},
                {"skipFirstNRows", job.SkipFirstNRows},
                {"skipLastNRows", job.SkipLastNRows},
                {"skipFirstNColumns", job.SkipFirstNColumns},
                {"skipLastNColumns", job.SkipLastNColumns}
            }
            If Not String.IsNullOrEmpty(job.ComplexFilter) Then
                payload("complex_filter") = job.ComplexFilter
            ElseIf Not String.IsNullOrEmpty(job.FilterColumn) AndAlso Not String.IsNullOrEmpty(job.Filter) Then
                payload("col_to_filter") = job.FilterColumn
                payload("filter") = job.Filter
            End If

            Dim body As String = JsonSerializer.Serialize(payload, _json)
            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/tools/process_excel")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "procesarea Excel", CInt(resp.StatusCode))
                    End If
                    ' Serverul întoarce {"data": ...}; scoatem doar "data", ca înainte.
                    Using doc As JsonDocument = JsonDocument.Parse(respText)
                        Dim dataEl As JsonElement = Nothing
                        If doc.RootElement.TryGetProperty("data", dataEl) Then
                            Return dataEl.GetRawText()
                        End If
                        Return respText
                    End Using
                End Using
            End Using
        Catch ex As ApiException
            ' HTTP tipat, tratat de apelant — nu logăm.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.ProcessExcelAsync", ex)
            Throw
        End Try
    End Function

    ' Apelurile de date cer o adresă de server ȘI o sesiune autentificată (token viu).
    ' Fara BaseAddress, caile relative arunca o exceptie criptica de framework; fara
    ' token, serverul ar raspunde oricum 401 — dar aici dam mesajul clar, local.
    Private Sub EnsureConfigured()
        If _http.BaseAddress Is Nothing Then
            Throw New ApiException(
                "Configurație lipsă: adresa serverului nu este setată. Contactați administratorul.")
        End If
        If String.IsNullOrEmpty(_session.Token) Then
            Throw New ApiException("Nu există o sesiune activă. Autentificați-vă.")
        End If
    End Sub

    ' Non-2xx -> ApiException cu mesajul român al serverului (câmpul "error"), codul
    ' HTTP și codul-motiv ("reason"). Nu mai expunem niciodată corpul JSON brut.
    Private Shared Function BuildApiException(respText As String, actiune As String, status As Integer) As ApiException
        Dim body As ApiErrorBody = ApiErrorBody.Parse(respText)
        Return New ApiException(body.MessageOrFallback(actiune, status), status, body.Reason)
    End Function

    Public Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T) Implements IApiClient.GetAsync
        Throw New NotImplementedException()
    End Function

    Public Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest, ct As CancellationToken) As Task(Of TResponse) Implements IApiClient.PostAsync
        Throw New NotImplementedException()
    End Function
End Class
