Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

' Slice 0100-02 -- GET /api/setari: routes/setari.py.
Partial Public Class ApiClient
    Implements ISetariApi

    ' Wire shape: field names exactly as routes/setari.py writes them. Valoare is a number, a string or null.
    Private NotInheritable Class SetariWireRow
        Public Property Cheie As String
        Public Property TextVizibil As String
        Public Property Tip As String
        Public Property Valoare As JsonElement?
    End Class

    Private NotInheritable Class SetariWire
        Public Property rows As List(Of SetariWireRow)
    End Class

    Public Async Function GetServerSettingsAsync(ct As CancellationToken) _
        As Task(Of IReadOnlyList(Of ServerSettingRow)) Implements ISetariApi.GetServerSettingsAsync
        Try
            EnsureConfigured()
            Using msg As New HttpRequestMessage(HttpMethod.Get, "/api/setari")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea setărilor serverului", CInt(resp.StatusCode))
                    End If
                    Dim wire As SetariWire = JsonSerializer.Deserialize(Of SetariWire)(respText, _json)
                    Dim result As New List(Of ServerSettingRow)()
                    If wire IsNot Nothing AndAlso wire.rows IsNot Nothing Then
                        For Each r As SetariWireRow In wire.rows
                            result.Add(New ServerSettingRow With {
                                .Key = If(r.Cheie, String.Empty),
                                .Text = If(r.TextVizibil, String.Empty),
                                .Kind = If(r.Tip, String.Empty),
                                .Value = ValueText(r.Valoare)})
                        Next
                    End If
                    Return result
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetServerSettingsAsync", ex)
            Throw
        End Try
    End Function

    ' The value as invariant text; null / absent -> Nothing.
    Private Shared Function ValueText(v As JsonElement?) As String
        If Not v.HasValue Then Return Nothing
        Select Case v.Value.ValueKind
            Case JsonValueKind.Null, JsonValueKind.Undefined : Return Nothing
            Case JsonValueKind.String : Return v.Value.GetString()
            Case JsonValueKind.Number : Return v.Value.GetRawText()
            Case JsonValueKind.True : Return "1"
            Case JsonValueKind.False : Return "0"
            Case Else
                Throw New ArgumentException("Unexpected JSON kind for a setting value: " & v.Value.ValueKind.ToString())
        End Select
    End Function

End Class
