Option Strict On
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 0081-09 - the program -> SS map of section A (routes/forexe/ddf_edit.py).
Partial Public Class ApiClient
    Implements IDdfProgramApi

    ' Wire shapes: field names exactly as ddf_edit.py writes them.
    Private NotInheritable Class SurseProgramResponse
        Public Property surse As List(Of SursaProgramWire)
    End Class

    Private NotInheritable Class SursaProgramWire
        Public Property program As String
        Public Property ss As String
        Public Property denumire As String
    End Class

    Public Async Function GetDdfSurseProgramAsync(ct As CancellationToken) _
        As Task(Of List(Of DdfSursaProgram)) Implements IDdfProgramApi.GetDdfSurseProgramAsync

        Try
            EnsureConfigured()
            Using msg As New HttpRequestMessage(HttpMethod.Get, "/api/forexe/ddf/surse-program")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea surselor programelor", CInt(resp.StatusCode))
                    End If
                    Dim payload As SurseProgramResponse =
                        JsonSerializer.Deserialize(Of SurseProgramResponse)(respText, _json)
                    Dim rezultat As New List(Of DdfSursaProgram)()
                    If payload Is Nothing OrElse payload.surse Is Nothing Then Return rezultat
                    For Each s As SursaProgramWire In payload.surse
                        If s Is Nothing Then Continue For
                        rezultat.Add(New DdfSursaProgram() With {
                            .Program = If(s.program, String.Empty),
                            .Ss = If(s.ss, String.Empty),
                            .Denumire = If(s.denumire, String.Empty)})
                    Next
                    Return rezultat
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetDdfSurseProgramAsync", ex)
            Throw
        End Try
    End Function

    ' Slice 0114-27 - wire shapes: field names exactly as routes/forexe/ddf_program.py writes / reads them.
    Private NotInheritable Class RepairProgramRequest
        Public Property cod As String
        Public Property aplica As Boolean
    End Class

    Private NotInheritable Class RepairProgramResponse
        Public Property cod As String
        Public Property aplicat As Boolean
        Public Property de_reparat As Integer
        Public Property reparate As Integer
        Public Property actual As String
        Public Property asteptat As String
        Public Property surse As String
        Public Property detalii As New List(Of RepairProgramDetail)()
        Public Property avertismente As New List(Of String)()
    End Class

    Private NotInheritable Class RepairProgramDetail
        Public Property tip As String
        Public Property numar As String
        Public Property ss As String
        Public Property program As String
    End Class

    Public Async Function RepairDdfProgramAsync(cod As String, apply As Boolean, ct As CancellationToken) _
        As Task(Of DdfProgramRepairResult) Implements IDdfProgramApi.RepairDdfProgramAsync
        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Dim k_body As String = JsonSerializer.Serialize(
                New RepairProgramRequest() With {.cod = cod, .aplica = apply}, _json)
            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/ddf/repara-program")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(k_body, System.Text.Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "repararea programului documentului", CInt(resp.StatusCode))
                    End If

                    Dim k_payload As RepairProgramResponse =
                        JsonSerializer.Deserialize(Of RepairProgramResponse)(respText, _json)
                    Dim k_rez As New DdfProgramRepairResult() With {.Cod = cod, .Applied = apply}
                    If k_payload Is Nothing Then Return k_rez

                    If Not String.IsNullOrEmpty(k_payload.cod) Then k_rez.Cod = k_payload.cod
                    k_rez.Applied = k_payload.aplicat
                    k_rez.ToRepair = k_payload.de_reparat
                    k_rez.Repaired = k_payload.reparate
                    k_rez.Current = If(k_payload.actual, String.Empty)
                    k_rez.Expected = If(k_payload.asteptat, String.Empty)
                    k_rez.Sources = If(k_payload.surse, String.Empty)
                    If k_payload.detalii IsNot Nothing Then
                        For Each k_det As RepairProgramDetail In k_payload.detalii
                            If k_det Is Nothing Then Continue For
                            k_rez.Details.Add(New DdfProgramDovada() With {
                                .Sursa = ParseSursaProgram(k_det.tip), .Numar = If(k_det.numar, String.Empty),
                                .Ss = If(k_det.ss, String.Empty), .Program = If(k_det.program, String.Empty)})
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
            GlobalErrorLog.Write("ApiClient.RepairDdfProgramAsync", ex)
            Throw
        End Try
    End Function

    ' «IND» / «REV» / «ORD» as the server writes them; anything else is a defect of the wire, not a guess.
    Private Shared Function ParseSursaProgram(k_tip As String) As DdfProgramSursa
        Select Case If(k_tip, String.Empty).Trim().ToUpperInvariant()
            Case "IND" : Return DdfProgramSursa.Indicatori
            Case "REV" : Return DdfProgramSursa.Revizie
            Case "ORD" : Return DdfProgramSursa.Ordonantare
            Case Else : Throw New ArgumentException($"Tip necunoscut pentru programul greșit: «{k_tip}».", NameOf(k_tip))
        End Select
    End Function

    ' «actual~asteptat~tip^numar^ss^program|...» from the tree route -> the program mistake; empty or malformed -> Nothing.
    Private Shared Function ParseProgramGresit(k_text As String) As DdfProgramGresit
        If String.IsNullOrWhiteSpace(k_text) Then Return Nothing
        Dim k_parti() As String = k_text.Split("~"c)
        If k_parti.Length <> 3 Then Return Nothing
        Dim k_rez As New DdfProgramGresit() With {.Actual = k_parti(0), .Asteptat = k_parti(1)}
        For Each k_bucata As String In k_parti(2).Split("|"c)
            Dim k_camp() As String = k_bucata.Split("^"c)
            If k_camp.Length <> 4 Then Continue For
            k_rez.Dovezi.Add(New DdfProgramDovada() With {
                .Sursa = ParseSursaProgram(k_camp(0)), .Numar = k_camp(1), .Ss = k_camp(2), .Program = k_camp(3)})
        Next
        Return k_rez
    End Function
End Class
