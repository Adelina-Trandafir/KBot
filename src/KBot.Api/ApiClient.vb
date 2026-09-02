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

    ' Optiuni SEPARATE, folosite doar la scrierea cererii de prelucrare (felia 0048-03).
    ' WhenWritingNull tine campurile care apartin doar fazei de salvare (`amprenta`,
    ' `decizii`) afara din corp cand sunt Nothing, ca o propunere sa arate pe fir exact
    ' cum arata inainte de felia asta.
    '
    ' De ce nu s-a schimbat `_json` de mai sus: el serializeaza TOATE celelalte cereri ale
    ' clientului, iar «omite null-urile» e o schimbare de contract pentru fiecare dintre
    ' ele. O ruta care deosebeste «cheia lipseste» de «cheia e null» s-ar schimba tacut.
    Private Shared ReadOnly _jsonFaraNull As JsonSerializerOptions =
        New JsonSerializerOptions With {
            .PropertyNamingPolicy = Nothing,
            .DefaultIgnoreCondition = Serialization.JsonIgnoreCondition.WhenWritingNull}

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
                                .Reconstituit = r.reconstituit,
                                .ReconstituitNesigur = r.reconstituit_nesigur,
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
                                .Denumire = If(r.denumire, String.Empty),
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

    ' Plățile unui angajament (slice 0017), pentru PlatiView. Un singur parametru:
    ' cod = CodAngajament, escapat in query string. NU se trimite baza (o citeste serverul
    ' din sesiune). Un cod fara plati intoarce 200 cu plati [], deci aici rezulta un PlatiInfo
    ' gol, nu o exceptie. Extrasul bancar vine FLAT pe rand si se pliaza intr-un ExtrasBancar
    ' (Nothing cand idfxe e null). Hard-fail (Throw ApiException) pe non-2xx; un 401 curge
    ' spre WithReauth.
    Public Async Function GetPlatiAsync(cod As String, ct As CancellationToken) _
        As Task(Of PlatiInfo) Implements IApiClient.GetPlatiAsync

        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim url As String = $"/api/forexe/plati?cod={Uri.EscapeDataString(cod)}"

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea plăților angajamentului", CInt(resp.StatusCode))
                    End If

                    Dim payload As GetPlatiResponse = JsonSerializer.Deserialize(Of GetPlatiResponse)(respText, _json)
                    Dim result As New PlatiInfo()
                    If payload Is Nothing Then Return result

                    result.Cod = If(payload.cod, If(cod, String.Empty))

                    If payload.plati IsNot Nothing Then
                        For Each p As GetPlataRow In payload.plati
                            Dim row As New PlataRow() With {
                                .IdPlataFX = p.id_plata_fx,
                                .IdClsf = p.id_clsf,
                                .CodAI = If(p.cod_ai, String.Empty),
                                .CodIndicator = If(p.cod_indicator, String.Empty),
                                .NrOP = If(p.nr_op, String.Empty),
                                .DataPlata = p.data_plata,
                                .Suma = p.suma,
                                .Tip = If(p.tip, String.Empty),
                                .Incarcat = p.incarcat,
                                .Preluat = p.preluat,
                                .ReferintaTrezor = If(p.referinta_trezor, String.Empty),
                                .Clsf = If(p.clsf, String.Empty),
                                .Denumire = If(p.denumire, String.Empty),
                                .ClsfPlata = If(p.clsf_plata, String.Empty)
                            }
                            row.AreOrd = p.are_ord
                            ' Extrasul se pliaza doar cand exista (idfxe non-null); altfel Nothing.
                            If p.idfxe.HasValue Then
                                row.Extras = New ExtrasBancar() With {
                                    .Idfxe = p.idfxe.Value,
                                    .DataBanca = p.data_banca,
                                    .DataDoc = If(p.data_doc, String.Empty),
                                    .NrDoc = If(p.nr_doc_extras, String.Empty),
                                    .Referinta = If(p.referinta, String.Empty),
                                    .PlatitorNume = If(p.platitor_nume, String.Empty),
                                    .PlatitorCui = If(p.platitor_cui, String.Empty),
                                    .PlatitorIban = If(p.platitor_iban, String.Empty),
                                    .SumaDebit = p.suma_debit,
                                    .SumaCredit = p.suma_credit,
                                    .Explicatii = If(p.explicatii, String.Empty)
                                }
                            End If
                            result.Plati.Add(row)
                        Next
                    End If
                    Return result
                End Using
            End Using
        Catch ex As ApiException
            ' 401/HTTP tipat, tratat de apelant (WithReauth) — nu logăm.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetPlatiAsync", ex)
            Throw
        End Try
    End Function

    ' Documentul de fundamentare al unui angajament (slice 0020), pentru DdfView. Un singur
    ' parametru: cod = CodAngajament, escapat in query string. NU se trimite baza (o citeste
    ' serverul din sesiune). Un cod fara DDF intoarce 200 cu cele trei liste goale, deci aici
    ' rezulta un DdfInfo gol, nu o exceptie. Envelope-ul poarta antet + revizii + linii intr-un
    ' SINGUR apel (vederea filtreaza local, fara alte cereri). Hard-fail (Throw ApiException)
    ' pe non-2xx; un 401 curge spre WithReauth.
    Public Async Function GetDdfAsync(cod As String, ct As CancellationToken,
                                      Optional pentruGenerare As Boolean = False) _
        As Task(Of DdfInfo) Implements IApiClient.GetDdfAsync

        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim url As String = $"/api/forexe/ddf?cod={Uri.EscapeDataString(cod)}"
            If pentruGenerare Then url &= "&pentru_generare=1"

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea documentului de fundamentare", CInt(resp.StatusCode))
                    End If

                    Dim payload As GetDdfResponse = JsonSerializer.Deserialize(Of GetDdfResponse)(respText, _json)
                    Dim result As New DdfInfo()
                    If payload Is Nothing Then Return result

                    result.Cod = If(payload.cod, If(cod, String.Empty))

                    If payload.antet IsNot Nothing Then
                        For Each a As GetDdfAntetRow In payload.antet
                            result.Antet.Add(New DdfAntet() With {
                                .Iddf = a.iddf,
                                .CodAngajament = If(a.cod_angajament, String.Empty),
                                .Cual = a.cual,
                                .ObiectDDF = If(a.obiect_ddf, String.Empty),
                                .Comp = If(a.comp, String.Empty),
                                .Program = If(a.program, String.Empty),
                                .DataCreare = a.data_creare,
                                .DataDef = a.data_def,
                                .Stare = If(a.stare, String.Empty),
                                .PartAng = a.part_ang,
                                .CodFiscal = If(a.cod_fiscal, String.Empty),
                                .NumePartener = If(a.nume_partener, String.Empty),
                                .Salarii = a.salarii,
                                .Incarcat = a.incarcat,
                                .Preluat = a.preluat
                            })
                        Next
                    End If

                    If payload.revizii IsNot Nothing Then
                        For Each r As GetDdfRevizieRow In payload.revizii
                            result.Revizii.Add(New RevizieRow() With {
                                .Idrev = r.idrev,
                                .Iddf = r.iddf,
                                .NumarRev = r.numar_rev,
                                .DataRev = r.data_rev,
                                .DescScurta = If(r.desc_scurta, String.Empty),
                                .DescLunga = If(r.desc_lunga, String.Empty),
                                .Tip = If(r.tip, String.Empty),
                                .Incarcat = r.incarcat,
                                .Preluat = r.preluat,
                                .Semnatura = If(r.semnatura, String.Empty),
                                .TotalRevizie = r.total_revizie,
                                .PdfSha256 = If(r.pdf_sha256, String.Empty),
                                .PdfDimensiune = r.pdf_dimensiune,
                                .PdfDataModif = r.pdf_data_modif
                            })
                        Next
                    End If

                    If payload.linii IsNot Nothing Then
                        For Each l As GetDdfLinieRow In payload.linii
                            result.Linii.Add(New LinieSaRow() With {
                                .IdSecA = l.id_sec_a,
                                .Idrev = l.idrev,
                                .IdClsf = l.id_clsf,
                                .Clsf = If(l.clsf, String.Empty),
                                .SS = If(l.ss, String.Empty),
                                .ElementFund = If(l.element_fund, String.Empty),
                                .ParametriiFund = If(l.parametrii_fund, String.Empty),
                                .ValPrec = l.val_prec,
                                .ValCur = l.val_cur,
                                .ValTot = l.val_tot
                            })
                        Next
                    End If

                    If payload.sectiuneb IsNot Nothing Then
                        For Each s As GetDdfSectiuneBRow In payload.sectiuneb
                            result.SectiuneB.Add(New SectiuneBRow() With {
                                .IdSecB = s.id_sec_b,
                                .Idrev = s.idrev,
                                .CodAngajament = If(s.cod_angajament, String.Empty),
                                .CodIndicator = If(s.cod_indicator, String.Empty),
                                .CodSSI = If(s.cod_ssi, String.Empty),
                                .CaAnterior = s.ca_anterior,
                                .Inf1 = s.inf1,
                                .CbAnterior = s.cb_anterior,
                                .Inf2 = s.inf2
                            })
                        Next
                    End If

                    If payload.atasamente IsNot Nothing Then
                        For Each a As GetDdfAtasamentRow In payload.atasamente
                            result.Atasamente.Add(New AtasamentRow() With {
                                .IdRevAtt = a.id_rev_att,
                                .Idrev = a.idrev,
                                .CaleFisier = If(a.cale_fisier, String.Empty),
                                .PrtScr = a.prt_scr,
                                .DateFisier = If(a.date_fisier, String.Empty)
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
            GlobalErrorLog.Write("ApiClient.GetDdfAsync", ex)
            Throw
        End Try
    End Function

    ' Ordonantarile unui angajament (slice 0033), pentru OrdView. Un singur parametru:
    ' cod = CodAngajament, escapat in query string. NU se trimite baza (o citeste serverul din
    ' sesiune). Un cod fara ordonantari intoarce 200 cu ambele liste goale, deci aici rezulta
    ' un OrdInfo gol, nu o exceptie. Doua liste intr-un singur apel (antete + linii); vederea
    ' filtreaza local pe IDORDP. Hard-fail (Throw ApiException) pe non-2xx; un 401 curge spre
    ' WithReauth.
    Public Async Function GetOrdAsync(cod As String, ct As CancellationToken) _
        As Task(Of OrdInfo) Implements IApiClient.GetOrdAsync

        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim url As String = $"/api/forexe/ord?cod={Uri.EscapeDataString(cod)}"

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea ordonanțărilor", CInt(resp.StatusCode))
                    End If

                    Dim payload As GetOrdResponse = JsonSerializer.Deserialize(Of GetOrdResponse)(respText, _json)
                    Dim result As New OrdInfo()
                    If payload Is Nothing Then Return result

                    result.Cod = If(payload.cod, If(cod, String.Empty))

                    If payload.ordonantari IsNot Nothing Then
                        For Each o As GetOrdHeaderRow In payload.ordonantari
                            result.Ordonantari.Add(New OrdHeaderRow() With {
                                .Idordp = o.idordp,
                                .NrOrd = o.nr_ord,
                                .DataOrd = o.data_ord,
                                .TotalOrd = o.total_ord,
                                .CalePdfInregistrata = If(o.pdf, String.Empty),
                                .PartAng = o.part_ang,
                                .NumePartener = If(o.nume_partener, String.Empty),
                                .Incarcat = o.incarcat,
                                .Preluat = o.preluat,
                                .PdfSha256 = If(o.pdf_sha256, String.Empty),
                                .PdfDimensiune = o.pdf_dimensiune,
                                .PdfDataModif = o.pdf_data_modif
                            })
                        Next
                    End If

                    If payload.linii IsNot Nothing Then
                        For Each l As GetOrdLinieRow In payload.linii
                            result.Linii.Add(New OrdLinieRow() With {
                                .Idordtblp = l.idordtblp,
                                .Idordp = l.idordp,
                                .Idordpartp = l.idordpartp,
                                .Clsf = If(l.clsf, String.Empty),
                                .Descriere = If(l.descriere, String.Empty),
                                .TotalReceptii = l.total_receptii,
                                .PlatiAnt = l.plati_ant,
                                .Valoare = l.valoare,
                                .Ramas = l.ramas,
                                .DenBene = If(l.den_bene, String.Empty),
                                .CodFiscal = If(l.cod_fiscal, String.Empty),
                                .ContIban = If(l.cont_iban, String.Empty),
                                .DocJust = If(l.doc_just, String.Empty),
                                .ObiectDdf = If(l.obiect_ddf, String.Empty)
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
            GlobalErrorLog.Write("ApiClient.GetOrdAsync", ex)
            Throw
        End Try
    End Function

    ' Istoricul unui angajament (slice 0022), pentru IstoricView. Un singur parametru:
    ' cod = CodAngajament, escapat in query string. NU se trimite baza (o citeste serverul din
    ' sesiune). Un cod fara istoric intoarce 200 cu ambele liste goale, deci aici rezulta un
    ' IstoricInfo gol, nu o exceptie. Doua liste intr-un singur apel (randuri + clasificatii);
    ' vederea filtreaza local. Hard-fail (Throw ApiException) pe non-2xx; un 401 curge spre
    ' WithReauth.
    Public Async Function GetIstoricAsync(cod As String, ct As CancellationToken) _
        As Task(Of IstoricInfo) Implements IApiClient.GetIstoricAsync

        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim url As String = $"/api/forexe/istoric?cod={Uri.EscapeDataString(cod)}"

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea istoricului angajamentului", CInt(resp.StatusCode))
                    End If

                    Dim payload As GetIstoricResponse = JsonSerializer.Deserialize(Of GetIstoricResponse)(respText, _json)
                    Dim result As New IstoricInfo()
                    If payload Is Nothing Then Return result

                    result.Cod = If(payload.cod, If(cod, String.Empty))

                    If payload.randuri IsNot Nothing Then
                        For Each r As GetIstoricRandRow In payload.randuri
                            result.Randuri.Add(New IstoricRand() With {
                                .Id = r.id,
                                .DataFx = r.data_fx,
                                .Clsf = If(r.clsf, String.Empty),
                                .IdClsf = r.id_clsf,
                                .TipRand = If(r.tip_rand, String.Empty),
                                .CodIndicator = If(r.cod_indicator, String.Empty),
                                .CodAI = If(r.cod_ai, String.Empty),
                                .Descriere = If(r.descriere, String.Empty),
                                .Observatii = If(r.observatii, String.Empty),
                                .ValRezervareI = r.val_rezervare_i,
                                .ValRezervareD = r.val_rezervare_d,
                                .ValRezervareAnt = r.val_rezervare_ant,
                                .ValRezervareDif = r.val_rezervare_dif,
                                .ValAngLeg = r.val_ang_leg,
                                .ValReceptie = r.val_receptie,
                                .ValPlata = r.val_plata,
                                .IdTrezor = If(r.id_trezor, String.Empty),
                                .Doc = If(r.doc, String.Empty),
                                .Idrev = r.idrev
                            })
                        Next
                    End If

                    If payload.clasificatii IsNot Nothing Then
                        For Each c As GetIstoricClasificatieRow In payload.clasificatii
                            result.Clasificatii.Add(New IstoricClasificatie() With {
                                .IdClsf = c.id_clsf,
                                .Clsf = If(c.clsf, String.Empty),
                                .Capitol = If(c.capitol, String.Empty),
                                .Subcapitol = If(c.subcapitol, String.Empty),
                                .Articol = If(c.articol, String.Empty),
                                .Alineat = If(c.alineat, String.Empty),
                                .DenSubcapitol = If(c.den_subcapitol, String.Empty),
                                .DenArticol = If(c.den_articol, String.Empty),
                                .DenAlineat = If(c.den_alineat, String.Empty)
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
            GlobalErrorLog.Write("ApiClient.GetIstoricAsync", ex)
            Throw
        End Try
    End Function

    ' ── PDF-uri SEMNATE stocate pe server (felia 0041) ───────────────────────
    ' Antetele contractului. `X-Sha256` = suma calculata de client peste octetii trimisi;
    ' `X-Sha-Precedent` = suma pe care clientul a vazut-o ultima data pentru document
    ' («-» cand crede ca nu exista rand) -> concurenta optimista, 409 la nepotrivire.
    Private Const H_SHA As String = "X-Sha256"
    Private Const H_SHA_PREC As String = "X-Sha-Precedent"
    ''' <summary>Valoarea lui <c>X-Sha-Precedent</c> pentru «cred că nu există rând pe server».</summary>
    Public Const ShaFaraRand As String = "-"

    ''' <summary>
    ''' Descarcă PDF-ul semnat al unei revizii DDF (GET /api/forexe/ddf/pdf/{idrev}).
    ''' Vezi <see cref="IApiClient.DownloadDdfPdfAsync"/> pentru contract.
    ''' </summary>
    Public Function DownloadDdfPdfAsync(idrev As Integer, cachedSha As String, ct As CancellationToken) _
        As Task(Of PdfDownloadResult) Implements IApiClient.DownloadDdfPdfAsync

        Return DownloadPdfAsync($"/api/forexe/ddf/pdf/{idrev}", cachedSha,
                                "descărcarea PDF-ului documentului de fundamentare",
                                "ApiClient.DownloadDdfPdfAsync", ct)
    End Function

    ''' <summary>
    ''' Descarcă PDF-ul semnat al unei ordonanțări (GET /api/forexe/ord/pdf/{idordp}).
    ''' </summary>
    Public Function DownloadOrdPdfAsync(idordp As Integer, cachedSha As String, ct As CancellationToken) _
        As Task(Of PdfDownloadResult) Implements IApiClient.DownloadOrdPdfAsync

        Return DownloadPdfAsync($"/api/forexe/ord/pdf/{idordp}", cachedSha,
                                "descărcarea PDF-ului ordonanțării",
                                "ApiClient.DownloadOrdPdfAsync", ct)
    End Function

    ''' <summary>
    ''' Încarcă PDF-ul semnat al unei revizii DDF (PUT /api/forexe/ddf/pdf/{idrev}).
    ''' Vezi <see cref="IApiClient.UploadDdfPdfAsync"/> pentru contract.
    ''' </summary>
    Public Function UploadDdfPdfAsync(idrev As Integer, continut As Byte(), shaPrecedent As String,
                                      ct As CancellationToken) _
        As Task(Of PutPdfResponse) Implements IApiClient.UploadDdfPdfAsync

        Return UploadPdfAsync($"/api/forexe/ddf/pdf/{idrev}", continut, shaPrecedent,
                              "salvarea PDF-ului documentului de fundamentare",
                              "ApiClient.UploadDdfPdfAsync", ct)
    End Function

    ''' <summary>
    ''' Încarcă PDF-ul semnat al unei ordonanțări (PUT /api/forexe/ord/pdf/{idordp}).
    ''' </summary>
    Public Function UploadOrdPdfAsync(idordp As Integer, continut As Byte(), shaPrecedent As String,
                                      ct As CancellationToken) _
        As Task(Of PutPdfResponse) Implements IApiClient.UploadOrdPdfAsync

        Return UploadPdfAsync($"/api/forexe/ord/pdf/{idordp}", continut, shaPrecedent,
                              "salvarea PDF-ului ordonanțării",
                              "ApiClient.UploadOrdPdfAsync", ct)
    End Function

    ' Descarcarea, o singura data pentru amandoua familiile de documente.
    '
    ' OCTETI BRUTI: `ReadAsByteArrayAsync`, NICIODATA `ReadAsStringAsync` — o trecere prin text
    ' ar schimba octetii si ar rupe semnatura digitala. Pe eroare (non-2xx, non-304) corpul se
    ' citeste ca text: acolo serverul trimite JSON romanesc, nu PDF.
    '
    ' `If-None-Match` cu suma cache-ului local -> 304 -> NotModified: cache-ul e bun, nu se
    ' descarca nimic. 404 -> NotFound (nu exceptie): «documentul n-are PDF semnat» e o stare
    ' normala a fluxului, iar apelantul cade pe regenerare.
    Private Async Function DownloadPdfAsync(url As String, cachedSha As String, actiune As String,
                                            sink As String, ct As CancellationToken) _
        As Task(Of PdfDownloadResult)
        Try
            EnsureConfigured()

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                If Not String.IsNullOrWhiteSpace(cachedSha) AndAlso
                   Not String.Equals(cachedSha, ShaFaraRand, StringComparison.Ordinal) Then
                    ' Ghilimelele fac parte din formatul ETag; serverul le taie la comparare.
                    msg.Headers.TryAddWithoutValidation("If-None-Match", """" & cachedSha.Trim() & """")
                End If

                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    If CInt(resp.StatusCode) = 304 Then Return PdfDownloadResult.NotModified()
                    If CInt(resp.StatusCode) = 404 Then Return PdfDownloadResult.NotFound()
                    If Not resp.IsSuccessStatusCode Then
                        Dim errText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                        Throw BuildApiException(errText, actiune, CInt(resp.StatusCode))
                    End If

                    Dim octeti As Byte() = Await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(False)

                    ' Regula bit-cu-bit 2, sensul DESCARCARE: recalculam suma peste octetii CHIAR
                    ' PRIMITI si o comparam cu ETag-ul. La nepotrivire NU intoarcem nimic — un PDF
                    ' semnat corupt scris in cache ar arata ca un document valid.
                    Dim etag As String = If(resp.Headers.ETag Is Nothing, String.Empty,
                                            resp.Headers.ETag.Tag.Trim(""""c))
                    Dim shaLocal As String = PdfHash.Compute(octeti)
                    If Not PdfHash.AreEqual(etag, shaLocal) Then
                        Throw New ApiException(
                            "Documentul a sosit corupt: suma de control nu corespunde. Încercați din nou.",
                            CInt(resp.StatusCode), "SHA_MISMATCH")
                    End If

                    Return PdfDownloadResult.FromContent(octeti, shaLocal)
                End Using
            End Using
        Catch ex As ApiException
            ' HTTP tipat, tratat de apelant (401 -> WithReauth) — nu logăm.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write(sink, ex)
            Throw
        End Try
    End Function

    ' Incarcarea, o singura data pentru amandoua familiile.
    '
    ' Corp = `ByteArrayContent` cu `application/octet-stream` — niciodata JSON, niciodata base64.
    ' Suma se calculeaza AICI (o singura implementare, `PdfHash`), deci apelantul nu o poate
    ' gresi. Numele fisierului NU se trimite: il deriva serverul, sursa unica.
    ' 409 / 400 / 404 ies ca `ApiException` cu mesajul romanesc al serverului, prin acelasi
    ' parser `ApiErrorBody` ca restul apelurilor.
    Private Async Function UploadPdfAsync(url As String, continut As Byte(), shaPrecedent As String,
                                          actiune As String, sink As String, ct As CancellationToken) _
        As Task(Of PutPdfResponse)
        Try
            EnsureConfigured()
            If continut Is Nothing OrElse continut.Length = 0 Then
                Throw New ArgumentException("Conținut PDF gol.", NameOf(continut))
            End If

            Dim sha As String = PdfHash.Compute(continut)
            ' Gol = «cred ca nu exista rand». Explicit, nu tacut: serverul cere antetul.
            Dim precedent As String = If(String.IsNullOrWhiteSpace(shaPrecedent), ShaFaraRand, shaPrecedent.Trim())

            Using msg As New HttpRequestMessage(HttpMethod.Put, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Headers.TryAddWithoutValidation(H_SHA, sha)
                msg.Headers.TryAddWithoutValidation(H_SHA_PREC, precedent)
                Dim body As New ByteArrayContent(continut)
                body.Headers.ContentType = New Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream")
                msg.Content = body

                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, actiune, CInt(resp.StatusCode))
                    End If
                    Dim payload As PutPdfResponse = JsonSerializer.Deserialize(Of PutPdfResponse)(respText, _json)
                    If payload Is Nothing Then
                        Throw New ApiException("Serverul a confirmat salvarea, dar fără detalii.",
                                               CInt(resp.StatusCode))
                    End If
                    Return payload
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write(sink, ex)
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

    ' ══════════════════════════════════════════════════════════════════════════════════
    ' EDITORUL DE ORDONANTARE (felia 0049)
    '
    ' Opt apeluri, toate pe rutele noi din `routes/forexe/ord_edit.py`. Baza NU se trimite
    ' niciodata: serverul o ia din sesiune (o baza = o unitate). Un 401 curge spre
    ' `WithReauth` (fara retry aici), exact ca la restul clientului.
    '
    ' Traducerea DTO ▸ POCO traieste aici, intr-un singur loc: `KBot.Api` cunoaste
    ' snake_case-ul firului, `KBot.Domain` nu-l vede niciodata.
    ' ══════════════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Cere serverului graful PROPUS al unei ordonantari noi (POST /api/forexe/ord/genereaza).
    ''' NIMIC nu se scrie — e portul lui <c>Genereaza_ORD</c>, mutat pe server fiindca
    ''' interogarile lui bat numai tabele care traiesc acum in MariaDB.
    ''' </summary>
    Public Async Function GenereazaOrdAsync(cod As String, dataOrd As Date, idPlataFx As Integer?,
                                            ct As CancellationToken) _
        As Task(Of OrdDraft) Implements IApiClient.GenereazaOrdAsync

        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim req As New GenereazaOrdRequest() With {
                .cod = cod,
                .data = dataOrd.ToString("yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture)}
            ' `id_plata_fx` lipsa = toate platile neordonantate ale zilei (VBA: `"*"`).
            If idPlataFx.HasValue AndAlso idPlataFx.Value > 0 Then req.id_plata_fx = idPlataFx.Value

            Dim body As String = JsonSerializer.Serialize(req, _jsonFaraNull)

            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/ord/genereaza")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        ' Inclusiv 404 «nu exista plati neordonantate in ziua asta»: e un
                        ' refuz cu motiv, nu o lista goala care ar minti operatorul.
                        Throw BuildApiException(respText, "generarea ordonanțării", CInt(resp.StatusCode))
                    End If
                    Return CitesteDraft(respText, cod)
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GenereazaOrdAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Citeste graful unei ordonantari EXISTENTE, in forma pe care o editeaza formularul
    ''' (GET /api/forexe/ord/draft/{idordp}).
    '''
    ''' <para>De ce nu <c>GetOrdAsync</c>: acela e apelul VEDERII 0033 si isi alege coloanele
    ''' deliberat — nu intoarce CodAI, CodIndicator, IdClsf, CodSSI, Explicatie, partenerul
    ''' liniei, randurile de document una cate una (le aduna intr-un singur text), legaturile
    ''' cu platile sau atasamentele. Editorul are nevoie de toate.</para>
    ''' </summary>
    Public Async Function GetOrdDraftAsync(idordp As Integer, ct As CancellationToken) _
        As Task(Of OrdDraft) Implements IApiClient.GetOrdDraftAsync

        Try
            EnsureConfigured()
            If idordp <= 0 Then Throw New ArgumentException("idordp invalid.", NameOf(idordp))

            Using msg As New HttpRequestMessage(HttpMethod.Get, $"/api/forexe/ord/draft/{idordp}")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea ordonanțării", CInt(resp.StatusCode))
                    End If
                    Return CitesteDraft(respText, String.Empty)
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetOrdDraftAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Zilele cu plati neordonantate ale unui angajament (GET /api/forexe/ord/zile) —
    ''' sursa modului in lot. <paramref name="luna"/> / <paramref name="an"/> sunt optionale.
    '''
    ''' <para>Access filtra cu <c>Month &amp; "/" &amp; Year LIKE "*"</c>; aici filtrul e
    ''' explicit, fiindca <c>*</c> nu e metacaracter in MariaDB si un LIKE cu el ar fi tacut
    ''' gresit.</para>
    ''' </summary>
    Public Async Function GetOrdZileAsync(cod As String, luna As Integer?, an As Integer?,
                                          ct As CancellationToken) _
        As Task(Of OrdZileInfo) Implements IApiClient.GetOrdZileAsync

        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim url As String = $"/api/forexe/ord/zile?cod={Uri.EscapeDataString(cod)}"
            If luna.HasValue Then url &= $"&luna={luna.Value}"
            If an.HasValue Then url &= $"&an={an.Value}"

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea zilelor cu plăți", CInt(resp.StatusCode))
                    End If

                    Dim payload As OrdZileResponse =
                        JsonSerializer.Deserialize(Of OrdZileResponse)(respText, _json)
                    Dim rez As New OrdZileInfo() With {.Cod = cod}
                    If payload Is Nothing Then Return rez
                    If Not String.IsNullOrEmpty(payload.cod) Then rez.Cod = payload.cod
                    rez.TotalEstimat = payload.total_estimat
                    If payload.zile IsNot Nothing Then
                        For Each z As OrdZiDto In payload.zile
                            rez.Zile.Add(New OrdZiCandidat() With {
                                .Data = z.data, .Plati = z.plati, .Ordonantari = z.ordonantari})
                        Next
                    End If
                    Return rez
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetOrdZileAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Numarul pe care l-ar primi ACUM o ordonantare noua (GET /api/forexe/ord/nr-urmator).
    ''' Presupunere, nu rezervare — vezi <see cref="IApiClient.GetOrdNrUrmatorAsync"/>.
    ''' </summary>
    Public Async Function GetOrdNrUrmatorAsync(ct As CancellationToken) _
        As Task(Of Integer) Implements IApiClient.GetOrdNrUrmatorAsync

        Try
            EnsureConfigured()

            Using msg As New HttpRequestMessage(HttpMethod.Get, "/api/forexe/ord/nr-urmator")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea numărului următor", CInt(resp.StatusCode))
                    End If

                    Dim payload As OrdNrUrmatorResponse =
                        JsonSerializer.Deserialize(Of OrdNrUrmatorResponse)(respText, _json)
                    Return If(payload Is Nothing, 0, payload.nr_ord)
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetOrdNrUrmatorAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Scrie TOT graful ordonantarii intr-o singura tranzactie (POST /api/forexe/ord/save)
    ''' si intoarce cheile reale.
    '''
    ''' <para>Lantul VBA in cinci pasi (staging pe server ▸ proba locala ▸ confirmare ▸
    ''' actualizarea id-urilor ▸ commit local) NU are corespondent: e un singur apel. Un refuz
    ''' de validare soseste ca <see cref="ApiException"/> cu mesajul romanesc al serverului,
    ''' care enumera TOATE motivele deodata, nu primul.</para>
    '''
    ''' <para>Octetii atasamentelor NU pleaca de aici — un <c>IDORDATTP</c> trebuie sa existe
    ''' inainte ca ei sa poata atarna de el. Se urca dupa, cu
    ''' <see cref="PutOrdAtasamentAsync"/>, folosind harta din raspuns.</para>
    ''' </summary>
    Public Async Function SaveOrdAsync(draft As OrdDraft, ct As CancellationToken) _
        As Task(Of OrdSaveRezultat) Implements IApiClient.SaveOrdAsync

        Try
            EnsureConfigured()
            If draft Is Nothing Then Throw New ArgumentNullException(NameOf(draft))

            Dim dto As OrdDraftDto = CatreFir(draft)
            Dim body As String = JsonSerializer.Serialize(dto, _jsonFaraNull)

            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/ord/save")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "salvarea ordonanțării", CInt(resp.StatusCode))
                    End If

                    Dim payload As OrdSaveResponse =
                        JsonSerializer.Deserialize(Of OrdSaveResponse)(respText, _json)
                    If payload Is Nothing OrElse payload.idordp <= 0 Then
                        Throw New ApiException(
                            "Serverul a confirmat salvarea, dar nu a întors cheia ordonanțării.",
                            CInt(resp.StatusCode))
                    End If

                    Dim rez As New OrdSaveRezultat() With {
                        .Idordp = payload.idordp, .NrOrd = payload.nr_ord}
                    If payload.harta IsNot Nothing Then
                        CopiazaHarta(payload.harta.parts, rez.Parts)
                        CopiazaHarta(payload.harta.linii, rez.Linii)
                        CopiazaHarta(payload.harta.rec, rez.Rec)
                        CopiazaHarta(payload.harta.doc, rez.Doc)
                        CopiazaHarta(payload.harta.att, rez.Att)
                    End If
                    Return rez
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveOrdAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Sterge o ordonantare cu tot ce atarna de ea (DELETE /api/forexe/ord/{idordp}).
    '''
    ''' <para>Cascadele bazei fac treaba; serverul numara INAINTE, deci raspunsul poate spune
    ''' un numar adevarat in loc de «gata». Platile se intorc singure in rezerva de
    ''' neordonantate, fiindca raspunsul la «plata asta e ordonantata?» sta tocmai in
    ''' <c>FX_ORD_TBL_REC</c>, pe care cascada il goleste.</para>
    ''' </summary>
    Public Async Function DeleteOrdAsync(idordp As Integer, ct As CancellationToken) _
        As Task(Of OrdStergereRezultat) Implements IApiClient.DeleteOrdAsync

        Try
            EnsureConfigured()
            If idordp <= 0 Then Throw New ArgumentException("idordp invalid.", NameOf(idordp))

            Using msg As New HttpRequestMessage(HttpMethod.Delete, $"/api/forexe/ord/{idordp}")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "ștergerea ordonanțării", CInt(resp.StatusCode))
                    End If

                    Dim payload As OrdStergereResponse =
                        JsonSerializer.Deserialize(Of OrdStergereResponse)(respText, _json)
                    Dim rez As New OrdStergereRezultat() With {.Idordp = idordp}
                    If payload Is Nothing Then Return rez
                    rez.Idordp = payload.idordp
                    rez.NrOrd = payload.nr_ord
                    rez.DataOrd = payload.data_ord
                    rez.Cod = If(payload.cod, String.Empty)
                    If payload.sterse IsNot Nothing Then
                        rez.Parteneri = payload.sterse.parteneri
                        rez.Linii = payload.sterse.linii
                        rez.Documente = payload.sterse.documente
                        rez.Atasamente = payload.sterse.atasamente
                        rez.Pdf = payload.sterse.pdf
                        rez.PlatiEliberate = payload.sterse.plati_eliberate
                    End If
                    Return rez
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.DeleteOrdAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Descarca octetii imaginii unui atasament
    ''' (GET /api/forexe/ord/att/{idordattp}/imagine). Acelasi contract ca la PDF-uri:
    ''' <paramref name="cachedSha"/> merge ca <c>If-None-Match</c>, un 304 intoarce
    ''' <see cref="PdfDownloadStatus.NotModified"/>, un 404 intoarce
    ''' <see cref="PdfDownloadStatus.NotFound"/> (nu exceptie — «atasamentul n-are imagine» e
    ''' o stare normala), iar octetii primiti sunt DEJA verificati pe SHA-256 fata de ETag.
    ''' </summary>
    Public Function GetOrdAtasamentAsync(idordattp As Integer, cachedSha As String,
                                         ct As CancellationToken) _
        As Task(Of PdfDownloadResult) Implements IApiClient.GetOrdAtasamentAsync

        Return DownloadPdfAsync($"/api/forexe/ord/att/{idordattp}/imagine", cachedSha,
                                "citirea imaginii atașamentului",
                                "ApiClient.GetOrdAtasamentAsync", ct)
    End Function

    ''' <summary>
    ''' Urca octetii imaginii unui atasament (PUT /api/forexe/ord/att/{idordattp}/imagine).
    '''
    ''' <para>Spre deosebire de PDF-uri, numele fisierului SE TRIMITE (antetul
    ''' <c>X-Nume-Fisier</c>): la ordonantare el e alegerea operatorului, nu o conventie pe
    ''' care serverul ar putea-o deriva singur. Tipul MIME, in schimb, il deduce serverul din
    ''' primii octeti — la fel cum facea <c>DetectMimeType</c> in Access, dar peste octeti
    ''' bruti, nu peste base64.</para>
    '''
    ''' <para><paramref name="shaPrecedent"/> = suma pe care apelantul a vazut-o ULTIMA DATA
    ''' (gol / «-» cand crede ca nu exista rand). O suma diferita pe server da 409 si nu se
    ''' scrie nimic.</para>
    ''' </summary>
    Public Async Function PutOrdAtasamentAsync(idordattp As Integer, numeFisier As String,
                                               continut As Byte(), shaPrecedent As String,
                                               ct As CancellationToken) _
        As Task(Of PutAtasamentResponse) Implements IApiClient.PutOrdAtasamentAsync

        Try
            EnsureConfigured()
            If idordattp <= 0 Then Throw New ArgumentException("idordattp invalid.", NameOf(idordattp))
            If continut Is Nothing OrElse continut.Length = 0 Then
                Throw New ArgumentException("Conținut imagine gol.", NameOf(continut))
            End If
            If String.IsNullOrWhiteSpace(numeFisier) Then
                Throw New ArgumentException("Numele fișierului este obligatoriu.", NameOf(numeFisier))
            End If

            Dim sha As String = PdfHash.Compute(continut)
            Dim precedent As String = If(String.IsNullOrWhiteSpace(shaPrecedent), ShaFaraRand, shaPrecedent.Trim())

            Using msg As New HttpRequestMessage(HttpMethod.Put, $"/api/forexe/ord/att/{idordattp}/imagine")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Headers.TryAddWithoutValidation(H_SHA, sha)
                msg.Headers.TryAddWithoutValidation(H_SHA_PREC, precedent)
                msg.Headers.TryAddWithoutValidation("X-Nume-Fisier", numeFisier.Trim())
                Dim payloadBody As New ByteArrayContent(continut)
                payloadBody.Headers.ContentType = New Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream")
                msg.Content = payloadBody

                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "salvarea imaginii atașamentului",
                                                CInt(resp.StatusCode))
                    End If
                    Dim payload As PutAtasamentResponse =
                        JsonSerializer.Deserialize(Of PutAtasamentResponse)(respText, _json)
                    If payload Is Nothing Then
                        Throw New ApiException("Serverul a confirmat salvarea, dar fără detalii.",
                                               CInt(resp.StatusCode))
                    End If
                    Return payload
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.PutOrdAtasamentAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Sterge octetii imaginii unui atasament, lasand randul de atasament pe loc
    ''' (DELETE /api/forexe/ord/att/{idordattp}/imagine).
    ''' </summary>
    Public Async Function DeleteOrdAtasamentAsync(idordattp As Integer, ct As CancellationToken) _
        As Task Implements IApiClient.DeleteOrdAtasamentAsync

        Try
            EnsureConfigured()
            If idordattp <= 0 Then Throw New ArgumentException("idordattp invalid.", NameOf(idordattp))

            Using msg As New HttpRequestMessage(HttpMethod.Delete, $"/api/forexe/ord/att/{idordattp}/imagine")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                        Throw BuildApiException(respText, "ștergerea imaginii atașamentului",
                                                CInt(resp.StatusCode))
                    End If
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.DeleteOrdAtasamentAsync", ex)
            Throw
        End Try
    End Function

    ' ── Traducerile fir ▸ domeniu si domeniu ▸ fir ───────────────────────────────────────

    ' Corpul de 200 al generarii / citirii -> POCO-ul de domeniu. Aceeasi functie pentru
    ' amandoua rutele: ele intorc EXACT aceeasi forma, ca formularul sa nu poata deosebi o
    ' ordonantare propusa de una existenta in altceva decat `Idordp`.
    Private Shared Function CitesteDraft(respText As String, codImplicit As String) As OrdDraft
        Dim payload As OrdDraftDto = JsonSerializer.Deserialize(Of OrdDraftDto)(respText, _json)
        Dim d As New OrdDraft() With {.CodAngajament = If(codImplicit, String.Empty)}
        If payload Is Nothing Then Return d

        If Not String.IsNullOrEmpty(payload.cod) Then d.CodAngajament = payload.cod

        If payload.antet IsNot Nothing Then
            Dim a As OrdDraftAntetDto = payload.antet
            d.Idordp = a.idordp
            d.NrOrd = a.nr_ord
            d.DataOrd = CitesteZi(a.data_ord)
            d.Iddf = If(a.iddf.HasValue, a.iddf.Value, 0)
            d.Cual = If(a.cual, String.Empty)
            d.Comp = If(a.comp, String.Empty)
            If Not String.IsNullOrEmpty(a.cod_angajament) Then d.CodAngajament = a.cod_angajament
            d.Incarcat = a.incarcat
            d.Preluat = a.preluat
            d.ObiectDdf = If(a.obiect_ddf, String.Empty)
            d.PartAng = a.part_ang
            d.NumePartener = If(a.nume_partener, String.Empty)
        End If

        If payload.parteneri IsNot Nothing Then
            For Each p As OrdDraftPartDto In payload.parteneri
                d.Parteneri.Add(New OrdDraftPart() With {
                    .TempId = p.temp_id, .Idordpartp = p.idordpartp,
                    .Counter = If(p.counter, String.Empty),
                    .DenBene = If(p.den_bene, String.Empty),
                    .CodFiscal = If(p.cod_fiscal, String.Empty),
                    .ContIban = If(p.cont_iban, String.Empty),
                    .Banca = If(p.banca, String.Empty)})
            Next
        End If

        If payload.linii IsNot Nothing Then
            For Each l As OrdDraftLinieDto In payload.linii
                d.Linii.Add(New OrdDraftLinie() With {
                    .TempId = l.temp_id, .Idordtblp = l.idordtblp,
                    .PartTempId = l.part_temp_id, .Idordpartp = l.idordpartp,
                    .CodAi = If(l.cod_ai, String.Empty),
                    .CodAngajament = If(l.cod_angajament, String.Empty),
                    .CodIndicator = If(l.cod_indicator, String.Empty),
                    .CodSsi = If(l.cod_ssi, String.Empty),
                    .IdClsf = If(l.id_clsf.HasValue, l.id_clsf.Value, 0),
                    .IdClsfAcc = If(l.id_clsf_acc.HasValue, l.id_clsf_acc.Value, 0),
                    .Clsf = If(l.clsf, String.Empty),
                    .Denumire = If(l.denumire, String.Empty),
                    .IdUnitate = If(l.id_unitate.HasValue, l.id_unitate.Value, 0),
                    .TotalReceptii = l.total_receptii, .PlatiAnt = l.plati_ant,
                    .Valoare = l.valoare, .Ramas = l.ramas,
                    .Explicatie = If(l.explicatie, String.Empty),
                    .CodPartener = If(l.cod_partener, String.Empty),
                    .IdPartener = If(l.id_partener.HasValue, l.id_partener.Value, 0)})
            Next
        End If

        If payload.rec IsNot Nothing Then
            For Each r As OrdDraftRecDto In payload.rec
                d.Rec.Add(New OrdDraftRec() With {
                    .TempId = r.temp_id, .Idordrecp = r.idordrecp,
                    .LinieTempId = r.linie_temp_id, .Idordtblp = r.idordtblp,
                    .IdPlataFx = If(r.id_plata_fx.HasValue, r.id_plata_fx.Value, 0),
                    .Valoare = r.valoare})
            Next
        End If

        If payload.documente IsNot Nothing Then
            For Each o As OrdDraftDocDto In payload.documente
                d.Documente.Add(New OrdDraftDoc() With {
                    .TempId = o.temp_id, .Idorddocp = o.idorddocp,
                    .PartTempId = o.part_temp_id, .Idordpartp = o.idordpartp,
                    .DocJust = If(o.doc_just, String.Empty),
                    .NumeDoc = If(o.nume_doc, String.Empty),
                    .TipDoc = If(o.tip_doc, "text")})
            Next
        End If

        If payload.atasamente IsNot Nothing Then
            For Each t As OrdDraftAttDto In payload.atasamente
                d.Atasamente.Add(New OrdDraftAtt() With {
                    .TempId = t.temp_id, .Idordattp = t.idordattp,
                    .PartTempId = t.part_temp_id, .Idordpartp = t.idordpartp,
                    .NumeFisier = If(t.nume_fisier, String.Empty),
                    .TipMime = If(t.tip_mime, String.Empty),
                    .Dimensiune = t.dimensiune,
                    .Sha256 = If(t.sha256, String.Empty),
                    .DataModif = t.data_modif})
            Next
        End If

        If payload.avertismente IsNot Nothing Then d.Avertismente.AddRange(payload.avertismente)
        Return d
    End Function

    ' POCO-ul de domeniu -> forma de pe fir. Oglinda exacta a lui `CitesteDraft`.
    Private Shared Function CatreFir(d As OrdDraft) As OrdDraftDto
        Dim dto As New OrdDraftDto() With {
            .cod = d.CodAngajament,
            .antet = New OrdDraftAntetDto() With {
                .idordp = d.Idordp,
                .nr_ord = d.NrOrd,
                .data_ord = If(d.DataOrd.HasValue,
                               d.DataOrd.Value.ToString("yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture),
                               Nothing),
                .cual = d.Cual,
                .comp = d.Comp,
                .cod_angajament = d.CodAngajament,
                .incarcat = d.Incarcat,
                .preluat = d.Preluat,
                .obiect_ddf = d.ObiectDdf,
                .part_ang = d.PartAng,
                .nume_partener = d.NumePartener}}
        ' `Iddf = 0` inseamna «niciunul» si pleaca drept null, nu zero — zero ar fi citit ca
        ' o cheie straina reala. Aceeasi regula ca la `Idrr` din asociere.
        If d.Iddf > 0 Then dto.antet.iddf = d.Iddf

        For Each p As OrdDraftPart In d.Parteneri
            dto.parteneri.Add(New OrdDraftPartDto() With {
                .temp_id = p.TempId, .idordpartp = p.Idordpartp, .counter = p.Counter,
                .den_bene = p.DenBene, .cod_fiscal = p.CodFiscal,
                .cont_iban = p.ContIban, .banca = p.Banca})
        Next

        For Each l As OrdDraftLinie In d.Linii
            Dim ldto As New OrdDraftLinieDto() With {
                .temp_id = l.TempId, .idordtblp = l.Idordtblp,
                .part_temp_id = l.PartTempId, .idordpartp = l.Idordpartp,
                .cod_ai = l.CodAi, .cod_angajament = l.CodAngajament,
                .cod_indicator = l.CodIndicator, .cod_ssi = l.CodSsi,
                .clsf = l.Clsf, .denumire = l.Denumire,
                .total_receptii = l.TotalReceptii, .plati_ant = l.PlatiAnt,
                .valoare = l.Valoare, .ramas = l.Ramas,
                .explicatie = l.Explicatie, .cod_partener = l.CodPartener}
            If l.IdClsf > 0 Then ldto.id_clsf = l.IdClsf
            If l.IdClsfAcc > 0 Then ldto.id_clsf_acc = l.IdClsfAcc
            If l.IdUnitate > 0 Then ldto.id_unitate = l.IdUnitate
            If l.IdPartener > 0 Then ldto.id_partener = l.IdPartener
            dto.linii.Add(ldto)
        Next

        For Each r As OrdDraftRec In d.Rec
            Dim rdto As New OrdDraftRecDto() With {
                .temp_id = r.TempId, .idordrecp = r.Idordrecp,
                .linie_temp_id = r.LinieTempId, .idordtblp = r.Idordtblp,
                .valoare = r.Valoare}
            If r.IdPlataFx > 0 Then rdto.id_plata_fx = r.IdPlataFx
            dto.rec.Add(rdto)
        Next

        ' `NumeDoc` gol pleaca drept null: pe server, `NumeDoc IS NULL` INSEAMNA «rand
        ' text», iar un sir vid n-ar mai fi NULL si randul n-ar mai fi text.
        For Each o As OrdDraftDoc In d.Documente
            dto.documente.Add(New OrdDraftDocDto() With {
                .temp_id = o.TempId, .idorddocp = o.Idorddocp,
                .part_temp_id = o.PartTempId, .idordpartp = o.Idordpartp,
                .doc_just = o.DocJust,
                .nume_doc = If(String.IsNullOrWhiteSpace(o.NumeDoc), Nothing, o.NumeDoc),
                .tip_doc = If(String.IsNullOrWhiteSpace(o.TipDoc), "text", o.TipDoc)})
        Next

        ' Octetii NU pleaca aici (faza a doua); doar randul si metadatele lui.
        For Each t As OrdDraftAtt In d.Atasamente
            dto.atasamente.Add(New OrdDraftAttDto() With {
                .temp_id = t.TempId, .idordattp = t.Idordattp,
                .part_temp_id = t.PartTempId, .idordpartp = t.Idordpartp,
                .nume_fisier = t.NumeFisier, .tip_mime = t.TipMime,
                .dimensiune = t.Dimensiune, .sha256 = t.Sha256})
        Next

        Return dto
    End Function

    ' Harta de pe fir (cheile sunt text, fiindca asa arata cheile unui obiect JSON) -> harta
    ' de domeniu, cu chei intregi. O cheie care nu e numar se SARE: e un rand pe care nu-l
    ' putem lega de nimic, iar o exceptie aici ar arunca o salvare deja reusita.
    Private Shared Sub CopiazaHarta(sursa As Dictionary(Of String, Integer),
                                    tinta As Dictionary(Of Integer, Integer))
        If sursa Is Nothing OrElse tinta Is Nothing Then Return
        For Each kvp As KeyValuePair(Of String, Integer) In sursa
            Dim cheie As Integer
            If Integer.TryParse(kvp.Key, Globalization.NumberStyles.Integer,
                                Globalization.CultureInfo.InvariantCulture, cheie) Then
                tinta(cheie) = kvp.Value
            End If
        Next
    End Sub

    ' «AAAA-LL-ZZ» (sau ISO cu ora) -> Date?. Un text neinteles intoarce Nothing, nu ziua de
    ' azi: o data inventata s-ar scrie tacut in document la urmatoarea salvare.
    Private Shared Function CitesteZi(text As String) As Date?
        If String.IsNullOrWhiteSpace(text) Then Return Nothing
        Dim rezultat As Date
        If Date.TryParse(text, Globalization.CultureInfo.InvariantCulture,
                         Globalization.DateTimeStyles.None, rezultat) Then
            Return rezultat.Date
        End If
        Return Nothing
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
    ' ── POST /api/forexe/prelucrare (ingestia FOREXE, felia 0048) ─────────────────────
    ' Un 409 cu reason=ALEGERE_UNITATE NU este o eroare: serverul a derulat tranzacția
    ' înapoi și cere o informație pe care doar operatorul o are (ca formularul modal
    ' FX_Unitate din Access). Se întoarce ca STARE, nu ca excepție — același tipar ca
    ' PdfDownloadStatus.NotFound, unde „nu există" e o cale normală, nu un eșec.
    ' Fără retry aici: cererea nu e idempotentă la nivel de rețea în sensul reîncercării
    ' oarbe, iar un 401 curge spre WithReauth.
    Public Async Function TrimitePrelucrareAsync(rezultat As PrelucrareRezultat,
                                                 alegeri As IReadOnlyList(Of AlegereUnitate),
                                                 ct As CancellationToken) As Task(Of PrelucrareRaspuns) Implements IApiClient.TrimitePrelucrareAsync
        Try
            EnsureConfigured()
            If rezultat Is Nothing Then Throw New ArgumentNullException(NameOf(rezultat))
            If String.IsNullOrWhiteSpace(rezultat.CodAngajament) Then
                Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(rezultat))
            End If

            Dim req As New PostPrelucrareRequest() With {
                .cod = rezultat.CodAngajament,
                .workflow = If(rezultat.Workflow, String.Empty),
                .moment = rezultat.Moment
            }
            If rezultat.Scalari IsNot Nothing Then
                req.scalari = New Dictionary(Of String, String)(rezultat.Scalari)
            End If
            If rezultat.Tabele IsNot Nothing Then
                req.tabele = TabeleJson.Catre(rezultat.Tabele)
            End If
            If alegeri IsNot Nothing Then
                For Each a As AlegereUnitate In alegeri
                    req.alegeri.Add(New PostPrelucrareAlegere() With {
                        .ss = a.Ss, .clsfe = a.ClsfE,
                        .id_unitate = a.IdUnitate, .retine = a.Retine})
                Next
            End If

            Dim body As String = JsonSerializer.Serialize(req, _json)

            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/prelucrare")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)

                    If CInt(resp.StatusCode) = 409 Then
                        Dim intrebare As PrelucrareRaspuns = CitesteAlegeri(respText)
                        ' 409 fără corpul așteptat = alt conflict, nu întrebarea noastră.
                        If intrebare IsNot Nothing Then Return intrebare
                    End If

                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "trimiterea prelucrării", CInt(resp.StatusCode))
                    End If

                    Return CitesteSalvat(respText, rezultat.CodAngajament)
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.TrimitePrelucrareAsync", ex)
            Throw
        End Try
    End Function

    ' Corpul de 200 -> POCO-ul de domeniu. Cheia «Indicatori» din `are` poate LIPSI: la
    ' server, o cheie absentă înseamnă „pasul nu a rulat", nu „a rulat și n-a găsit nimic".
    Private Shared Function CitesteSalvat(respText As String, cod As String) As PrelucrareRaspuns
        Dim payload As PostPrelucrareResponse =
            JsonSerializer.Deserialize(Of PostPrelucrareResponse)(respText, _json)
        Dim raspuns As New PrelucrareRaspuns() With {
            .Stare = PrelucrareStare.Salvat,
            .CodAngajament = cod
        }
        If payload Is Nothing Then Return raspuns
        If Not String.IsNullOrEmpty(payload.cod) Then raspuns.CodAngajament = payload.cod
        Dim areIndicatori As Boolean = False
        If payload.are IsNot Nothing Then payload.are.TryGetValue("Indicatori", areIndicatori)
        raspuns.AreIndicatori = areIndicatori
        If payload.scrise IsNot Nothing Then
            For Each kvp In payload.scrise
                raspuns.Scrise(kvp.Key) = kvp.Value
            Next
        End If
        If payload.avertismente IsNot Nothing Then raspuns.Avertismente.AddRange(payload.avertismente)
        Return raspuns
    End Function

    ' Corpul de 409 -> POCO-ul de domeniu, sau Nothing dacă nu e întrebarea noastră
    ' (alt cod-motiv, corp non-JSON, listă goală). Nothing lasă apelantul să arunce
    ' ApiException, ca la orice alt conflict.
    Private Shared Function CitesteAlegeri(respText As String) As PrelucrareRaspuns
        Dim payload As PostPrelucrareChoiceBody
        Try
            payload = JsonSerializer.Deserialize(Of PostPrelucrareChoiceBody)(respText, _json)
        Catch ex As JsonException
            ' Corp non-JSON pe un 409 — cale normală, apelantul cade pe ApiException.
            Return Nothing
        End Try
        If payload Is Nothing Then Return Nothing
        If Not String.Equals(payload.reason, PrelucrareRaspuns.MotivAlegereUnitate, StringComparison.Ordinal) Then Return Nothing
        If payload.alegeri_necesare Is Nothing OrElse payload.alegeri_necesare.Count = 0 Then Return Nothing

        Dim raspuns As New PrelucrareRaspuns() With {
            .Stare = PrelucrareStare.AlegereUnitate,
            .CodAngajament = If(payload.cod, String.Empty),
            .Mesaj = If(payload.error, String.Empty)
        }
        For Each n As PostPrelucrareAlegereNecesara In payload.alegeri_necesare
            Dim necesara As New AlegereNecesara() With {
                .Ss = If(n.ss, String.Empty),
                .ClsfE = If(n.clsfe, String.Empty),
                .Clsf = If(n.clsf, String.Empty),
                .CodIndicator = If(n.cod_indicator, String.Empty)
            }
            If n.indicatori IsNot Nothing Then necesara.Indicatori.AddRange(n.indicatori)
            If n.unitati IsNot Nothing Then
                For Each u As PostPrelucrareUnitate In n.unitati
                    necesara.Unitati.Add(New UnitateCandidat() With {
                        .IdUnitate = u.id_unitate,
                        .Detalii = If(u.detalii, String.Empty),
                        .SursaSector = If(u.sursa_sector, String.Empty),
                        .CodProgram = If(u.cod_program, String.Empty)})
                Next
            End If
            raspuns.AlegeriNecesare.Add(necesara)
        Next
        Return raspuns
    End Function

    Private Shared Function BuildApiException(respText As String, actiune As String, status As Integer) As ApiException
        Dim body As ApiErrorBody = ApiErrorBody.Parse(respText)
        Return New ApiException(body.MessageOrFallback(actiune, status), status, body.Reason)
    End Function


    ' ── Felia 0048-03: cele DOUA faze ────────────────────────────────────────────────
    ' Ambele lovesc ACEEASI ruta, cu `mod` diferit. Ce le desparte nu e adresa, ci ce are
    ' voie serverul sa faca la coada tranzactiei: sa o deruleze inapoi, sau sa o comita.
    '
    ' Sarcina utila e IDENTICA in amandoua. `RandIstoric` din decizii e indicele randului
    ' in TabelIstoric (F24), nu o cheie de baza de date, deci faza a doua TREBUIE sa
    ' trimita exact ce a vazut faza intai. De-asta fisierul local pastreaza payload-ul, nu
    ' doar deciziile.

    Private Function ConstruiesteCerere(rezultat As PrelucrareRezultat,
                                        alegeri As IReadOnlyList(Of AlegereUnitate),
                                        modul As String) As PostPrelucrareRequest
        If rezultat Is Nothing Then Throw New ArgumentNullException(NameOf(rezultat))
        If String.IsNullOrWhiteSpace(rezultat.CodAngajament) Then
            Throw New ArgumentException("Codul angajamentului este obligatoriu.", NameOf(rezultat))
        End If

        Dim req As New PostPrelucrareRequest() With {
            .cod = rezultat.CodAngajament,
            .workflow = If(rezultat.Workflow, String.Empty),
            .moment = rezultat.Moment,
            .mod = modul
        }
        If rezultat.Scalari IsNot Nothing Then
            req.scalari = New Dictionary(Of String, String)(rezultat.Scalari)
        End If
        If rezultat.Tabele IsNot Nothing Then
            req.tabele = TabeleJson.Catre(rezultat.Tabele)
        End If
        If alegeri IsNot Nothing Then
            For Each a As AlegereUnitate In alegeri
                req.alegeri.Add(New PostPrelucrareAlegere() With {
                    .ss = a.Ss, .clsfe = a.ClsfE,
                    .id_unitate = a.IdUnitate, .retine = a.Retine})
            Next
        End If
        Return req
    End Function

    Public Async Function CerePropunereAsync(rezultat As PrelucrareRezultat,
                                             alegeri As IReadOnlyList(Of AlegereUnitate),
                                             ct As CancellationToken) As Task(Of PrelucrareRaspuns) Implements IApiClient.CerePropunereAsync
        Try
            EnsureConfigured()
            Dim req As PostPrelucrareRequest = ConstruiesteCerere(rezultat, alegeri, "propunere")
            Dim body As String = JsonSerializer.Serialize(req, _jsonFaraNull)

            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/prelucrare")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)

                    ' 409 ALEGERE_UNITATE se poate declansa SI in faza intai — un angajament
                    ' poate avea nevoie de doua drumuri dus-intors inainte ca operatorul sa
                    ' vada formularul de asociere. Nu e o eroare; nu s-a scris nimic.
                    If CInt(resp.StatusCode) = 409 Then
                        Dim intrebare As PrelucrareRaspuns = CitesteAlegeri(respText)
                        If intrebare IsNot Nothing Then Return intrebare
                    End If

                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "cererea propunerii", CInt(resp.StatusCode))
                    End If

                    Return New PrelucrareRaspuns() With {
                        .Stare = PrelucrareStare.Propunere,
                        .CodAngajament = rezultat.CodAngajament,
                        .Propunere = CitestePropunere(respText, rezultat.CodAngajament)
                    }
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.CerePropunereAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SalveazaAsociereaAsync(rezultat As PrelucrareRezultat,
                                                 amprenta As String,
                                                 decizii As IReadOnlyList(Of DecizieAsociere),
                                                 alegeri As IReadOnlyList(Of AlegereUnitate),
                                                 ct As CancellationToken) As Task(Of PrelucrareRaspuns) Implements IApiClient.SalveazaAsociereaAsync
        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(amprenta) Then
                Throw New ArgumentException("Amprenta propunerii este obligatorie.", NameOf(amprenta))
            End If
            If decizii Is Nothing Then Throw New ArgumentNullException(NameOf(decizii))

            Dim req As PostPrelucrareRequest = ConstruiesteCerere(rezultat, alegeri, "salvare")
            req.amprenta = amprenta
            req.decizii = New List(Of PostPrelucrareDecizie)()
            For Each d As DecizieAsociere In decizii
                req.decizii.Add(CatreFir(d))
            Next

            Dim body As String = JsonSerializer.Serialize(req, _jsonFaraNull)

            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/prelucrare")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)

                    If CInt(resp.StatusCode) = 409 Then
                        Dim intrebare As PrelucrareRaspuns = CitesteAlegeri(respText)
                        If intrebare IsNot Nothing Then Return intrebare
                    End If

                    ' STARE_MODIFICATA ajunge ca EXCEPTIE, nu ca stare: spre deosebire de
                    ' ALEGERE_UNITATE, aici nu exista nimic de raspuns. Baza s-a miscat, nu
                    ' s-a scris nimic, si singurul drum inainte e o descarcare noua.
                    ' BuildApiException pune deja `reason` in ApiException.Reason, deci
                    ' apelantul poate testa dupa PrelucrarePropunere.MotivStareModificata.
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "salvarea asocierii", CInt(resp.StatusCode))
                    End If

                    Return CitesteSalvat(respText, rezultat.CodAngajament)
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SalveazaAsociereaAsync", ex)
            Throw
        End Try
    End Function

    ' O decizie -> forma de pe fir. Regula celor doua tinte care se exclud traieste la
    ' server (care respinge cu 400); aici se trimite doar ce a fost pus, fara sa se
    ' fabrice nimic: `Idrr = 0` inseamna «niciuna» si devine null, nu zero.
    Private Shared Function CatreFir(d As DecizieAsociere) As PostPrelucrareDecizie
        ' `Desprins` exista doar in editorul de oricand (felia 0048-04): in ingestie nimic
        ' nu e inca atasat, deci nu e nimic de desprins, iar serverul nici nu cunoaste
        ' numele pe calea aceea. Se refuza aici, nu la server, ca mesajul sa spuna DE CE.
        If d.Actiune = ActiuneAsociere.Desprins Then
            Throw New ArgumentException(
                "Acțiunea «desprins» nu are sens în ingestie: acolo niciun instantaneu " &
                "nu este încă atașat. Ea aparține editorului de asociere.", NameOf(d))
        End If
        Dim pe As New PostPrelucrareDecizie() With {
            .rand_istoric = d.RandIstoric,
            .data_h = d.DataH.ToString("yyyy-MM-ddTHH:mm:ss", Globalization.CultureInfo.InvariantCulture),
            .actiune = NumeActiune(d.Actiune)
        }
        If d.Idrr > 0 Then pe.idrr = d.Idrr
        If Not String.IsNullOrWhiteSpace(d.ReceptieNoua) Then pe.receptie_noua = d.ReceptieNoua
        Return pe
    End Function

    ' Numele de pe fir sunt ASCII pe amandoua laturile (regula 0). Primele patru sunt cele
    ' pe care le accepta routes/forexe/prelucrare_asociere.py; a cincea, «desprins»,
    ' traieste doar in routes/forexe/asociere.py si e refuzata in `CatreFir`, pe calea de
    ' ingestie. O valoare necunoscuta de enum ridica — fara implicit tacut.
    Private Shared Function NumeActiune(a As ActiuneAsociere) As String
        Select Case a
            Case ActiuneAsociere.Asociat : Return "asociat"
            Case ActiuneAsociere.Ignorat : Return "ignorat"
            Case ActiuneAsociere.Stergere : Return "stergere"
            Case ActiuneAsociere.Reconstituire : Return "reconstituire"
            Case ActiuneAsociere.Desprins : Return "desprins"
            Case Else
                Throw New ArgumentException($"Acțiune necunoscută: {a}", NameOf(a))
        End Select
    End Function

    ' ── GET / POST /api/forexe/asociere (editorul R ▸ H de ORICAND, felia 0048-04) ────
    ' Un singur parametru la citire: cod = CodAngajament, escapat in query string. NU se
    ' trimite baza (o citeste serverul din sesiune). Un angajament fara recepții intoarce
    ' 200 cu liste goale, deci aici rezulta un AsociereStare gol, nu o exceptie.
    Public Async Function GetAsociereAsync(cod As String, ct As CancellationToken) _
        As Task(Of AsociereStare) Implements IApiClient.GetAsociereAsync

        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim url As String = $"/api/forexe/asociere?cod={Uri.EscapeDataString(cod)}"

            Using msg As New HttpRequestMessage(HttpMethod.Get, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea asocierii recepțiilor", CInt(resp.StatusCode))
                    End If
                    Return CitesteAsociere(respText, cod)
                End Using
            End Using
        Catch ex As ApiException
            ' 401/HTTP tipat, tratat de apelant (WithReauth) — nu logăm.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetAsociereAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SalveazaLegaturiAsync(cod As String,
                                                amprenta As String,
                                                comenzi As IReadOnlyList(Of ComandaAsociere),
                                                ct As CancellationToken) As Task(Of AsociereRezultat) Implements IApiClient.SalveazaLegaturiAsync
        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))
            If String.IsNullOrWhiteSpace(amprenta) Then
                Throw New ArgumentException("Amprenta tabloului este obligatorie.", NameOf(amprenta))
            End If
            If comenzi Is Nothing Then Throw New ArgumentNullException(NameOf(comenzi))
            If comenzi.Count = 0 Then
                ' O salvare fara nicio comanda nu e o salvare partiala, e o greseala de
                ' apelant. Serverul o refuza si el; se opreste aici ca sa nu plece degeaba.
                Throw New ArgumentException("Nu s-a cerut nicio modificare.", NameOf(comenzi))
            End If

            Dim req As New PostAsociereRequest() With {.cod = cod, .amprenta = amprenta}
            For Each c As ComandaAsociere In comenzi
                Dim pc As New PostAsociereComanda() With {
                    .idrh = c.Idrh, .actiune = NumeActiune(c.Actiune)}
                ' `Idrr = 0` inseamna «niciuna» si devine null, nu zero — la fel ca la
                ' decizii. Zero ar fi citit ca o tinta, nu ca o tacere.
                If c.Idrr > 0 Then pc.idrr = c.Idrr
                If Not String.IsNullOrWhiteSpace(c.ReceptieNoua) Then pc.receptie_noua = c.ReceptieNoua
                req.comenzi.Add(pc)
            Next

            Dim body As String = JsonSerializer.Serialize(req, _jsonFaraNull)

            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/asociere")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)

                    ' STARE_MODIFICATA si INSTANTANEU_BLOCAT ajung ca EXCEPTIE cu `Reason`
                    ' completat (BuildApiException il extrage): in niciunul dintre cazuri
                    ' nu exista ceva de raspuns aici, doar de reincarcat tabloul.
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "salvarea legăturilor", CInt(resp.StatusCode))
                    End If

                    Dim payload As PostAsociereResponse =
                        JsonSerializer.Deserialize(Of PostAsociereResponse)(respText, _json)
                    Dim rez As New AsociereRezultat() With {.CodAngajament = cod}
                    If payload Is Nothing Then Return rez

                    If Not String.IsNullOrEmpty(payload.cod) Then rez.CodAngajament = payload.cod
                    rez.Amprenta = If(payload.amprenta, String.Empty)
                    If payload.scrise IsNot Nothing Then
                        For Each kvp In payload.scrise
                            rez.Scrise(kvp.Key) = kvp.Value
                        Next
                    End If
                    If payload.avertismente IsNot Nothing Then rez.Avertismente.AddRange(payload.avertismente)
                    Return rez
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SalveazaLegaturiAsync", ex)
            Throw
        End Try
    End Function

    ' Corpul de 200 al citirii -> POCO-ul de domeniu.
    Private Shared Function CitesteAsociere(respText As String, cod As String) As AsociereStare
        Dim payload As GetAsociereResponse =
            JsonSerializer.Deserialize(Of GetAsociereResponse)(respText, _json)
        Dim s As New AsociereStare() With {.CodAngajament = cod}
        If payload Is Nothing Then Return s

        If Not String.IsNullOrEmpty(payload.cod) Then s.CodAngajament = payload.cod
        s.Amprenta = If(payload.amprenta, String.Empty)

        If payload.receptii IsNot Nothing Then
            For Each r As PostPropunereReceptie In payload.receptii
                Dim rec As New ReceptiePropusa() With {
                    .Idrr = r.idrr,
                    .DataR = CitesteData(r.data_r),
                    .SumaAntet = r.suma_antet,
                    .Descriere = If(r.descriere, String.Empty),
                    .Sters = r.sters,
                    .Reconstituit = r.reconstituit,
                    .ReconstituitNesigur = r.reconstituit_nesigur}
                If r.rhr IsNot Nothing Then
                    For Each l As PostPropunereLinieR In r.rhr
                        rec.Rhr.Add(New LinieReceptie() With {
                            .CodIndicator = If(l.cod_indicator, String.Empty),
                            .CodAi = If(l.cod_ai, String.Empty),
                            .CodSsi = If(l.cod_ssi, String.Empty),
                            .CreditBugetar = l.credit_bugetar,
                            .Valoare = l.valoare,
                            .ValoareN = l.valoare_n})
                    Next
                End If
                s.Receptii.Add(rec)
            Next
        End If

        If payload.instantanee IsNot Nothing Then
            For Each i As GetAsociereInstantaneu In payload.instantanee
                Dim inst As New InstantaneuLegat() With {
                    .Idrh = i.idrh,
                    .Idrr = i.idrr,
                    .Idh = i.idh,
                    .DataH = CitesteData(i.data_h),
                    .Descriere = If(i.descriere, String.Empty),
                    .Total = i.total,
                    .TipReceptie = If(i.tip_receptie, String.Empty),
                    .Stergere = i.stergere,
                    .Ignorat = i.ignorat,
                    .Blocat = i.blocat}
                If i.motive IsNot Nothing Then inst.Motive.AddRange(i.motive)
                If i.linii IsNot Nothing Then
                    For Each l As PostPropunereLinieI In i.linii
                        inst.Linii.Add(New LinieInstantaneu() With {
                            .CodIndicator = If(l.cod_indicator, String.Empty),
                            .CodAi = If(l.cod_ai, String.Empty),
                            .CodSsi = If(l.cod_ssi, String.Empty),
                            .IdClsf = If(l.id_clsf.HasValue, l.id_clsf.Value, 0),
                            .Valoare = l.valoare})
                    Next
                End If
                s.Instantanee.Add(inst)
            Next
        End If

        If payload.plati IsNot Nothing Then
            For Each p As GetAsocierePlata In payload.plati
                s.Plati.Add(New PlataAsociere() With {
                    .DataPlata = CitesteData(p.data_plata),
                    .Suma = p.suma,
                    .NrOp = If(p.nr_op, String.Empty)})
            Next
        End If

        Return s
    End Function

    ' Corpul de 200 al propunerii -> POCO-ul de domeniu.
    Private Shared Function CitestePropunere(respText As String, cod As String) As PrelucrarePropunere
        Dim payload As PostPropunereResponse =
            JsonSerializer.Deserialize(Of PostPropunereResponse)(respText, _json)
        Dim p As New PrelucrarePropunere() With {.CodAngajament = cod}
        If payload Is Nothing Then Return p

        If Not String.IsNullOrEmpty(payload.cod) Then p.CodAngajament = payload.cod
        p.Amprenta = If(payload.amprenta, String.Empty)

        If payload.are IsNot Nothing Then
            For Each kvp In payload.are
                p.Are(kvp.Key) = kvp.Value
            Next
        End If
        If payload.scrise IsNot Nothing Then
            For Each kvp In payload.scrise
                p.Scrise(kvp.Key) = kvp.Value
            Next
        End If
        If payload.avertismente IsNot Nothing Then p.Avertismente.AddRange(payload.avertismente)

        If payload.receptii IsNot Nothing Then
            For Each r As PostPropunereReceptie In payload.receptii
                Dim rec As New ReceptiePropusa() With {
                    .Idrr = r.idrr,
                    .DataR = CitesteData(r.data_r),
                    .SumaAntet = r.suma_antet,
                    .Descriere = If(r.descriere, String.Empty),
                    .Sters = r.sters,
                    .Reconstituit = r.reconstituit,
                    .ReconstituitNesigur = r.reconstituit_nesigur}
                If r.rhr IsNot Nothing Then
                    For Each l As PostPropunereLinieR In r.rhr
                        rec.Rhr.Add(New LinieReceptie() With {
                            .CodIndicator = If(l.cod_indicator, String.Empty),
                            .CodAi = If(l.cod_ai, String.Empty),
                            .CodSsi = If(l.cod_ssi, String.Empty),
                            .CreditBugetar = l.credit_bugetar,
                            .Valoare = l.valoare,
                            .ValoareN = l.valoare_n})
                    Next
                End If
                p.Receptii.Add(rec)
            Next
        End If

        If payload.instantanee IsNot Nothing Then
            For Each i As PostPropunereInstantaneu In payload.instantanee
                Dim inst As New InstantaneuPropus() With {
                    .RandIstoric = i.rand_istoric,
                    .DataH = CitesteData(i.data_h),
                    .Descriere = If(i.descriere, String.Empty),
                    .Total = i.total,
                    .Stergere = i.stergere,
                    .SugestieIdrr = If(i.sugestie_idrr.HasValue, i.sugestie_idrr.Value, 0),
                    .SugestieAutomata = i.sugestie_automata}
                If i.linii IsNot Nothing Then
                    For Each l As PostPropunereLinieI In i.linii
                        inst.Linii.Add(New LinieInstantaneu() With {
                            .CodIndicator = If(l.cod_indicator, String.Empty),
                            .CodAi = If(l.cod_ai, String.Empty),
                            .CodSsi = If(l.cod_ssi, String.Empty),
                            .IdClsf = If(l.id_clsf.HasValue, l.id_clsf.Value, 0),
                            .Valoare = l.valoare})
                    Next
                End If
                p.Instantanee.Add(inst)
            Next
        End If

        Return p
    End Function

    ' Datele sosesc ca text: Flask serializeaza datetime-urile MariaDB cu `default=str`,
    ' deci "2026-05-20 00:36:12". Se citesc cu cultura INVARIANTA — cultura masinii nu are
    ' nimic de-a face cu ce a scris serverul. O data necitibila ridica: o dată tăcut zero
    ' ar strica vetoul de dată (F13), care e chiar paza contra asocierii greșite.
    Private Shared Function CitesteData(text As String) As Date
        If String.IsNullOrWhiteSpace(text) Then Return Nothing
        Dim d As Date
        If Date.TryParse(text, Globalization.CultureInfo.InvariantCulture,
                         Globalization.DateTimeStyles.None, d) Then Return d
        Throw New ApiException($"Serverul a trimis o dată pe care nu o pot citi: «{text}».")
    End Function

    Public Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T) Implements IApiClient.GetAsync
        Throw New NotImplementedException()
    End Function

    Public Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest, ct As CancellationToken) As Task(Of TResponse) Implements IApiClient.PostAsync
        Throw New NotImplementedException()
    End Function
End Class
