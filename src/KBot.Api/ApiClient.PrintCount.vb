Option Strict On
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

' Slice 0099 -- PrintCount: routes/forexe/print_count.py.
Partial Public Class ApiClient
    Implements IPrintCountApi

    ' Wire shape: field names exactly as routes/forexe/print_count.py writes them.
    Private NotInheritable Class PrintCountWire
        Public Property numarat As Boolean
        Public Property tinta As String
        Public Property print_count As Integer?
    End Class

    Public Async Function RecordPrintAsync(kind As PrintedDocumentKind, id As Integer, ct As CancellationToken) _
        As Task(Of PrintCountResult) Implements IPrintCountApi.RecordPrintAsync
        Try
            EnsureConfigured()
            If id <= 0 Then Throw New ArgumentException("Document id must be positive.", NameOf(id))

            Using msg As New HttpRequestMessage(HttpMethod.Post, PrintUrl(kind, id))
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "numărarea tipăririi", CInt(resp.StatusCode))
                    End If
                    Dim wire As PrintCountWire = JsonSerializer.Deserialize(Of PrintCountWire)(respText, _json)
                    If wire Is Nothing Then Throw New ApiException("Serverul nu a întors numărul de tipăriri.", CInt(resp.StatusCode))
                    Return New PrintCountResult With {
                        .Counted = wire.numarat,
                        .Target = If(wire.tinta, String.Empty),
                        .PrintCount = wire.print_count}
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.RecordPrintAsync", ex)
            Throw
        End Try
    End Function

    ' Unknown kind -> ArgumentException (house rule: no silent no-op).
    Private Shared Function PrintUrl(kind As PrintedDocumentKind, id As Integer) As String
        Select Case kind
            Case PrintedDocumentKind.Ddf : Return $"/api/forexe/ddf/pdf/{id}/print"
            Case PrintedDocumentKind.Ord : Return $"/api/forexe/ord/pdf/{id}/print"
            Case PrintedDocumentKind.CabNote : Return $"/api/forexe/nc/pdf/{id}/print"
            Case PrintedDocumentKind.CabNoteReceipt : Return $"/api/forexe/note-cab/recipisa/{id}/print"
            Case Else
                Throw New ArgumentException($"Unknown printed document kind: '{kind}'.", NameOf(kind))
        End Select
    End Function

End Class
