Option Strict On
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 0114-26 - reservations linked to a revision that does not cover them.
Partial Public Class ApiClient
    Implements IRezervariLegaturiApi

    ' Wire shapes: field names exactly as routes/forexe/rezervari_legaturi.py writes / reads them.
    Private NotInheritable Class RepairLinksRequest
        Public Property cod As String
        Public Property aplica As Boolean
    End Class

    Private NotInheritable Class RepairLinksDetail
        Public Property data As String
        Public Property indicator As String
        Public Property idrev As Integer
        Public Property valoare As Double
        Public Property data_revizie As String
    End Class

    Private NotInheritable Class RepairLinksResponse
        Public Property cod As String
        Public Property aplicat As Boolean
        Public Property de_reparat As Integer
        Public Property reparate As Integer
        Public Property capturi_scoase As Integer
        Public Property detalii As New List(Of RepairLinksDetail)()
        Public Property avertismente As New List(Of String)()
    End Class

    Public Async Function RepairReservationLinksAsync(cod As String, apply As Boolean, ct As CancellationToken) _
        As Task(Of RezervariLegaturiResult) Implements IRezervariLegaturiApi.RepairReservationLinksAsync
        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim k_body As String = JsonSerializer.Serialize(
                New RepairLinksRequest() With {.cod = cod, .aplica = apply}, _json)
            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/rezervari/repara-legaturi")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(k_body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "repararea legăturilor rezervărilor", CInt(resp.StatusCode))
                    End If

                    Dim k_payload As RepairLinksResponse =
                        JsonSerializer.Deserialize(Of RepairLinksResponse)(respText, _json)
                    Dim k_rez As New RezervariLegaturiResult() With {.Cod = cod, .Applied = apply}
                    If k_payload Is Nothing Then Return k_rez

                    If Not String.IsNullOrEmpty(k_payload.cod) Then k_rez.Cod = k_payload.cod
                    k_rez.Applied = k_payload.aplicat
                    k_rez.ToRepair = k_payload.de_reparat
                    k_rez.Repaired = k_payload.reparate
                    k_rez.PicturesRemoved = k_payload.capturi_scoase
                    If k_payload.detalii IsNot Nothing Then
                        For Each k_det As RepairLinksDetail In k_payload.detalii
                            Dim k_data As Date
                            If Not Date.TryParseExact(k_det.data, "yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture,
                                                      Globalization.DateTimeStyles.None, k_data) Then Continue For
                            k_rez.Details.Add(New RezervareLegataGresit() With {
                                .DataRezervare = k_data, .CodIndicator = If(k_det.indicator, String.Empty),
                                .IdRevizie = k_det.idrev, .Valoare = k_det.valoare,
                                .DataRevizie = ParseZiIso(k_det.data_revizie)})
                        Next
                    End If
                    If k_payload.avertismente IsNot Nothing Then k_rez.Warnings.AddRange(k_payload.avertismente)
                    Return k_rez
                End Using
            End Using
        Catch ex As ApiException
            ' 401/HTTP tipat, tratat de apelant (WithReauth) - nu logam.
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.RepairReservationLinksAsync", ex)
            Throw
        End Try
    End Function

    ' «yyyy-MM-dd» -> a date; empty or anything else -> Nothing (a missing revision date is not an error).
    Private Shared Function ParseZiIso(k_text As String) As Date?
        If String.IsNullOrWhiteSpace(k_text) Then Return Nothing
        Dim k_zi As Date
        If Date.TryParseExact(k_text, "yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture,
                              Globalization.DateTimeStyles.None, k_zi) Then Return k_zi
        Return Nothing
    End Function

    ' «yyyy-MM-dd~indicator~idrev~valoare~yyyy-MM-dd|...» from the tree route -> the reservations linked to a
    ' revision that does not cover them. A piece that does not parse is skipped, not guessed (a warning column).
    Private Shared Function ParseLegaturiGresite(k_text As String) As IReadOnlyList(Of RezervareLegataGresit)
        Dim k_rezultat As New List(Of RezervareLegataGresit)()
        If String.IsNullOrWhiteSpace(k_text) Then Return k_rezultat
        For Each k_bucata As String In k_text.Split("|"c)
            Dim k_parti() As String = k_bucata.Split("~"c)
            If k_parti.Length <> 5 Then Continue For
            Dim k_data As Date
            Dim k_idrev As Integer
            Dim k_valoare As Double
            If Date.TryParseExact(k_parti(0), "yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture,
                                  Globalization.DateTimeStyles.None, k_data) AndAlso
               Integer.TryParse(k_parti(2), Globalization.NumberStyles.Integer, Globalization.CultureInfo.InvariantCulture, k_idrev) AndAlso
               Double.TryParse(k_parti(3), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, k_valoare) Then
                k_rezultat.Add(New RezervareLegataGresit() With {
                    .DataRezervare = k_data, .CodIndicator = k_parti(1), .IdRevizie = k_idrev,
                    .Valoare = k_valoare, .DataRevizie = ParseZiIso(k_parti(4))})
            End If
        Next
        Return k_rezultat
    End Function

End Class
