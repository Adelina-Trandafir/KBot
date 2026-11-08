Option Strict On
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 02EF-11 -- the ordonantare made from a reception.
Partial Public Class ApiClient
    Implements IOrdReceptieApi

    ''' <summary>
    ''' The proposed graph of a new ordonantare made from a reception (POST /api/forexe/ord/genereaza-din-receptie). Nothing is written.
    ''' </summary>
    Public Async Function GenereazaOrdDinReceptieAsync(cod As String, idrh As Integer, dataOrd As Date,
                                                       ct As CancellationToken) _
        As Task(Of OrdDraft) Implements IOrdReceptieApi.GenereazaOrdDinReceptieAsync

        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))
            If idrh <= 0 Then Throw New ArgumentException("idrh trebuie să fie pozitiv.", NameOf(idrh))

            Dim req As New GenereazaOrdDinReceptieRequest() With {
                .cod = cod, .idrh = idrh,
                .data = dataOrd.ToString("yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture)}
            Dim body As String = JsonSerializer.Serialize(req, _jsonFaraNull)

            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/ord/genereaza-din-receptie")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "generarea ordonanțării din recepție", CInt(resp.StatusCode))
                    End If
                    Return CitesteDraft(respText, cod)
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GenereazaOrdDinReceptieAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>Slice 02EF-13: the pending rows a free payment may be attached to by hand.</summary>
    Public Async Function GetLiniiCandidateAsync(idPlataFx As Integer, ct As CancellationToken) _
        As Task(Of OrdLiniiCandidateResponse) Implements IOrdReceptieApi.GetLiniiCandidateAsync

        Try
            EnsureConfigured()
            If idPlataFx <= 0 Then Throw New ArgumentException("idPlataFx trebuie să fie pozitiv.", NameOf(idPlataFx))
            Using msg As New HttpRequestMessage(HttpMethod.Get, $"/api/forexe/ord/plata/{idPlataFx}/linii-candidate")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea ordonanțărilor la care se poate lega plata", CInt(resp.StatusCode))
                    End If
                    Dim payload As OrdLiniiCandidateResponse = JsonSerializer.Deserialize(Of OrdLiniiCandidateResponse)(respText, _json)
                    If payload Is Nothing Then Throw New ApiException("Serverul a trimis un răspuns gol.", CInt(resp.StatusCode))
                    Return payload
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetLiniiCandidateAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>Slice 02EF-13: attaches ONE payment to ONE pending row.</summary>
    Public Async Function LeagaPlataAsync(idPlataFx As Integer, idordrecp As Integer, ct As CancellationToken) _
        As Task Implements IOrdReceptieApi.LeagaPlataAsync

        Try
            EnsureConfigured()
            If idPlataFx <= 0 OrElse idordrecp <= 0 Then Throw New ArgumentException("Plata și rândul trebuie alese.")
            Dim body As String = JsonSerializer.Serialize(
                New LeagaPlataRequest() With {.id_plata_fx = idPlataFx, .idordrecp = idordrecp}, _jsonFaraNull)
            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/ord/leaga-plata")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "legarea plății de ordonanțare", CInt(resp.StatusCode))
                    End If
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.LeagaPlataAsync", ex)
            Throw
        End Try
    End Function

End Class
