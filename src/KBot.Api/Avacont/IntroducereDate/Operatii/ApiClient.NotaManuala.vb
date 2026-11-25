Option Strict On
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

Public Interface INotaManualaApi
    Function GetNotaManualaContextAsync(sid As Integer, an As Integer, idOper As Integer?, ct As CancellationToken) As Task(Of NotaManualaContext)
    Function GetNotaManualaDocumenteAsync(sid As Integer, an As Integer, idNota As Integer, ct As CancellationToken) As Task(Of List(Of NotaManualaDocument))
    Function SalveazaNotaManualaAsync(cerere As NotaManualaCerere, ct As CancellationToken) As Task(Of NotaManualaRezultat)
End Interface

''' <summary>
''' SLICE-AV08-05: a refusal of the accounting editor's save, with the box / line to correct (<see cref="NotaManualaRefuz"/>).
''' A 401 is never this: it stays an <see cref="ApiException"/> so the re-login works as everywhere.
''' </summary>
Public NotInheritable Class NotaManualaException
    Inherits Exception

    Public ReadOnly Property Refuz As NotaManualaRefuz

    Public Sub New(k_refuz As NotaManualaRefuz)
        MyBase.New(k_refuz.Mesaj)
        Refuz = k_refuz
    End Sub
End Class

Partial Public Class ApiClient
    Implements INotaManualaApi

    Private Const NotaManualaRoot As String = "/note-manuale"
    Private Shared ReadOnly NotaManualaJson As New JsonSerializerOptions With {.NumberHandling = JsonNumberHandling.AllowReadingFromString}

    Public Async Function GetNotaManualaContextAsync(sid As Integer, an As Integer, idOper As Integer?, ct As CancellationToken) As Task(Of NotaManualaContext) _
        Implements INotaManualaApi.GetNotaManualaContextAsync
        Dim query As String = OpQuery(sid, an)
        If idOper.HasValue Then query &= $"&idoper={MoId(idOper.Value)}"
        Dim response As String = Await SendNomenclatoareAsync(HttpMethod.Get, $"{AvacontRoot}{NotaManualaRoot}/context?{query}", Nothing,
            "citirea notei manuale", ct).ConfigureAwait(False)
        Return JsonSerializer.Deserialize(Of NotaManualaContext)(response, NotaManualaJson)
    End Function

    Public Async Function GetNotaManualaDocumenteAsync(sid As Integer, an As Integer, idNota As Integer, ct As CancellationToken) As Task(Of List(Of NotaManualaDocument)) _
        Implements INotaManualaApi.GetNotaManualaDocumenteAsync
        Dim response As String = Await SendNomenclatoareAsync(HttpMethod.Get, $"{AvacontRoot}{NotaManualaRoot}/documente?{OpQuery(sid, an)}&idnota={MoId(idNota)}&toate=1", Nothing,
            "citirea documentelor notei contabile", ct).ConfigureAwait(False)
        Return JsonSerializer.Deserialize(Of List(Of NotaManualaDocument))(response, NotaManualaJson)
    End Function

    ''' <summary>SLICE-AV08-05: the save answers what was saved; a refusal comes back as <see cref="NotaManualaException"/> (box, line, warning).</summary>
    Public Async Function SalveazaNotaManualaAsync(cerere As NotaManualaCerere, ct As CancellationToken) As Task(Of NotaManualaRezultat) _
        Implements INotaManualaApi.SalveazaNotaManualaAsync
        Try
            EnsureConfigured()
            Using msg As New HttpRequestMessage(HttpMethod.Post, $"{AvacontRoot}{NotaManualaRoot}/salveaza")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(JsonSerializer.Serialize(cerere, _json), Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim k_text As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    Dim k_status As Integer = CInt(resp.StatusCode)
                    If resp.IsSuccessStatusCode Then Return JsonSerializer.Deserialize(Of NotaManualaRezultat)(k_text, NotaManualaJson)
                    Dim k_refuz As NotaManualaRefuz = CitesteRefuz(k_text, k_status)
                    If k_status = 401 OrElse k_refuz Is Nothing Then Throw BuildApiException(k_text, "salvarea notei contabile", k_status)
                    Throw New NotaManualaException(k_refuz)
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As NotaManualaException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SalveazaNotaManualaAsync", ex)
            Throw
        End Try
    End Function

    ' The refusal body of note_manuale.py: {error, reason, camp?, parte?, linie?, confirmare?}. Nothing when the body is not that JSON.
    Private Shared Function CitesteRefuz(k_text As String, k_status As Integer) As NotaManualaRefuz
        If String.IsNullOrWhiteSpace(k_text) Then Return Nothing
        Try
            Using k_doc As JsonDocument = JsonDocument.Parse(k_text)
                Dim k_root As JsonElement = k_doc.RootElement
                If k_root.ValueKind <> JsonValueKind.Object Then Return Nothing
                Dim k_error As JsonElement
                If Not k_root.TryGetProperty("error", k_error) OrElse k_error.ValueKind <> JsonValueKind.String Then Return Nothing
                Dim k_refuz As New NotaManualaRefuz With {.Mesaj = k_error.GetString(), .StatusCode = k_status,
                    .Reason = RefuzText(k_root, "reason"), .Camp = RefuzText(k_root, "camp"), .Parte = RefuzText(k_root, "parte"),
                    .Confirmare = RefuzText(k_root, "confirmare")}
                Dim k_linie As JsonElement
                If k_root.TryGetProperty("linie", k_linie) AndAlso k_linie.ValueKind = JsonValueKind.Number Then k_refuz.Linie = k_linie.GetInt32()
                Return k_refuz
            End Using
        Catch ex As JsonException
            Return Nothing
        End Try
    End Function

    Private Shared Function RefuzText(k_root As JsonElement, k_name As String) As String
        Dim k_value As JsonElement
        If k_root.TryGetProperty(k_name, k_value) AndAlso k_value.ValueKind = JsonValueKind.String Then Return k_value.GetString()
        Return Nothing
    End Function
End Class
