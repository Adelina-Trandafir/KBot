Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 0088 -- «Nota contabila corectie CAB»: routes/forexe/note_cab.py + the «nc» PDF family.
Partial Public Class ApiClient
    Implements ICabNotesApi

    ' Wire shapes: field names exactly as routes/forexe/note_cab.py reads / writes them.
    Private NotInheritable Class CabIndicatorWire
        Public Property cod_indicator As String
        Public Property cod_ai As String
        Public Property ss As String
        Public Property clsf_sal As String
        Public Property clsf As String
        Public Property denumire As String
        Public Property programe As List(Of String)
    End Class

    Private NotInheritable Class CabCommitmentWire
        Public Property cod_angajament As String
        Public Property descriere As String
        Public Property indicatori As List(Of CabIndicatorWire)
    End Class

    Private NotInheritable Class CabPreparationWire
        Public Property urmatorul_numar As Integer
        Public Property an As Integer
        Public Property numere_folosite As List(Of Integer)
        Public Property angajamente As List(Of CabCommitmentWire)
    End Class

    Private NotInheritable Class CabCorrectionWire
        Public Property idfxp As Integer?
        Public Property referinta_trezor As String
        Public Property nr_doc As String
        Public Property simbol_cont As String
        Public Property cod_program As String
        Public Property data_oper_initiala As String
        Public Property coloana As String
        Public Property suma As Decimal
        Public Property cod_angajament As String
        Public Property cod_indicator As String
        Public Property cod_ai As String
        Public Property simbol_cont_corectie As String
        Public Property cod_program_corectie As String
        Public Property explicatii As String
    End Class

    Private NotInheritable Class CabNoteWire
        Public Property idnc As Integer
        Public Property nr_nota As Integer
        Public Property an As Integer
        Public Property data_nota As String
        Public Property denumire_ep As String
        Public Property cif_ep As String
        Public Property semnatura As String
        Public Property trimis As Boolean
        Public Property data_trimitere As String
        Public Property raspuns_trimitere As String
        Public Property pdf_sha256 As String
        Public Property index_inregistrare As String
        Public Property recipisa As CabReceiptWire
        Public Property corectii As List(Of CabCorrectionWire)
    End Class

    ' Slice 0088-04: the receipt, both ways (the POST adds id_mesaj and continut).
    Private NotInheritable Class CabReceiptWire
        Public Property idrcp As Integer
        Public Property index As String
        Public Property numar_inregistrare As String
        Public Property id_mesaj As String
        Public Property descriere As String
        Public Property data_mesaj As String
        Public Property nume_fisier As String
        Public Property dimensiune As Integer
        Public Property sha256 As String
        Public Property continut As String
    End Class

    Private NotInheritable Class CabReceiptSavedWire
        Public Property recipisa As CabReceiptWire
    End Class

    Private NotInheritable Class CabNotesWire
        Public Property note As List(Of CabNoteWire)
    End Class

    Private NotInheritable Class CabSaveWire
        Public Property nr_nota As Integer
        Public Property data_nota As String
        Public Property denumire_ep As String
        Public Property cif_ep As String
        Public Property corectii As List(Of CabCorrectionWire)
    End Class

    Private NotInheritable Class CabSavedWire
        Public Property idnc As Integer
        Public Property nr_nota As Integer
    End Class

    ' Slice 0088-05: every note of one save of the window.
    Private NotInheritable Class CabBatchNoteWire
        Public Property nr_nota As Integer
        Public Property corectii As List(Of CabCorrectionWire)
    End Class

    Private NotInheritable Class CabBatchWire
        Public Property data_nota As String
        Public Property denumire_ep As String
        Public Property cif_ep As String
        Public Property note As List(Of CabBatchNoteWire)
    End Class

    Private NotInheritable Class CabBatchSavedWire
        Public Property note As List(Of CabSavedWire)
    End Class

    Private NotInheritable Class CabSentWire
        Public Property raspuns As String
        Public Property index As String
    End Class

    Public Async Function GetCabNotePreparationAsync(ct As CancellationToken) As Task(Of CabNotePreparation) _
        Implements ICabNotesApi.GetCabNotePreparationAsync
        Try
            Dim text As String = Await SendCabAsync(HttpMethod.Get, "/api/forexe/note-cab/pregatire", Nothing,
                                                    "citirea datelor pentru nota de corecție", ct).ConfigureAwait(False)
            Dim wire As CabPreparationWire = JsonSerializer.Deserialize(Of CabPreparationWire)(text, _json)
            Dim result As New CabNotePreparation()
            If wire Is Nothing Then Return result
            result.NextNumber = wire.urmatorul_numar
            result.Year = wire.an
            If wire.numere_folosite IsNot Nothing Then result.UsedNumbers.UnionWith(wire.numere_folosite)
            For Each a As CabCommitmentWire In If(wire.angajamente, New List(Of CabCommitmentWire)())
                Dim c As New CabCommitment With {
                    .Code = If(a.cod_angajament, String.Empty), .Description = If(a.descriere, String.Empty)}
                For Each i As CabIndicatorWire In If(a.indicatori, New List(Of CabIndicatorWire)())
                    c.Indicators.Add(New CabCommitmentIndicator With {
                        .Code = If(i.cod_indicator, String.Empty), .CodAi = If(i.cod_ai, String.Empty),
                        .Ss = If(i.ss, String.Empty), .ClsfSal = If(i.clsf_sal, String.Empty),
                        .Clsf = If(i.clsf, String.Empty), .Name = If(i.denumire, String.Empty),
                        .Programs = If(i.programe, New List(Of String)())})
                Next
                result.Commitments.Add(c)
            Next
            Return result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetCabNotePreparationAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveCabNoteAsync(note As CabCorrectionNote, ct As CancellationToken) As Task(Of Integer) _
        Implements ICabNotesApi.SaveCabNoteAsync
        Try
            ArgumentNullException.ThrowIfNull(note)
            Dim body As New CabSaveWire With {
                .nr_nota = note.NoteNumber,
                .data_nota = IsoDate(note.NoteDate),
                .denumire_ep = note.EntityName,
                .cif_ep = note.EntityTaxCode,
                .corectii = note.Corrections.Select(AddressOf ToWire).ToList()}
            Dim text As String = Await SendCabAsync(HttpMethod.Post, "/api/forexe/note-cab",
                                                    JsonSerializer.Serialize(body, _json),
                                                    "salvarea notei de corecție", ct).ConfigureAwait(False)
            Dim saved As CabSavedWire = JsonSerializer.Deserialize(Of CabSavedWire)(text, _json)
            If saved Is Nothing OrElse saved.idnc <= 0 Then Throw New ApiException("Serverul nu a întors numărul notei salvate.", 200)
            Return saved.idnc
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveCabNoteAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveCabNotesAsync(notes As IReadOnlyList(Of CabCorrectionNote), ct As CancellationToken) As Task _
        Implements ICabNotesApi.SaveCabNotesAsync
        Try
            If notes Is Nothing OrElse notes.Count = 0 Then Throw New ArgumentException("No notes.", NameOf(notes))
            Dim first As CabCorrectionNote = notes(0)
            Dim body As New CabBatchWire With {
                .data_nota = IsoDate(first.NoteDate),
                .denumire_ep = first.EntityName,
                .cif_ep = first.EntityTaxCode,
                .note = notes.Select(Function(n) New CabBatchNoteWire With {
                    .nr_nota = n.NoteNumber,
                    .corectii = n.Corrections.Select(AddressOf ToWire).ToList()}).ToList()}
            Dim text As String = Await SendCabAsync(HttpMethod.Post, "/api/forexe/note-cab/lot",
                                                    JsonSerializer.Serialize(body, _json),
                                                    "salvarea notelor de corecție", ct).ConfigureAwait(False)
            Dim saved As CabBatchSavedWire = JsonSerializer.Deserialize(Of CabBatchSavedWire)(text, _json)
            For Each n As CabCorrectionNote In notes
                Dim s As CabSavedWire = saved?.note?.FirstOrDefault(Function(x) x.nr_nota = n.NoteNumber)
                If s Is Nothing OrElse s.idnc <= 0 Then
                    Throw New ApiException($"Serverul nu a întors nota nr. {n.NoteNumber} salvată.", 200)
                End If
                n.IdNc = s.idnc
            Next
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveCabNotesAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetCabNotesAsync(codAngajament As String, ct As CancellationToken) As Task(Of List(Of CabCorrectionNote)) _
        Implements ICabNotesApi.GetCabNotesAsync
        Try
            If String.IsNullOrWhiteSpace(codAngajament) Then Throw New ArgumentException("Empty angajament code.", NameOf(codAngajament))
            Dim text As String = Await SendCabAsync(HttpMethod.Get,
                                                    $"/api/forexe/note-cab?cod={Uri.EscapeDataString(codAngajament)}",
                                                    Nothing, "citirea notelor de corecție", ct).ConfigureAwait(False)
            Dim wire As CabNotesWire = JsonSerializer.Deserialize(Of CabNotesWire)(text, _json)
            Dim result As New List(Of CabCorrectionNote)()
            If wire Is Nothing OrElse wire.note Is Nothing Then Return result
            For Each n As CabNoteWire In wire.note
                Dim note As New CabCorrectionNote With {
                    .IdNc = n.idnc, .NoteNumber = n.nr_nota, .Year = n.an,
                    .NoteDate = ParseIsoDate(n.data_nota).GetValueOrDefault(),
                    .EntityName = If(n.denumire_ep, String.Empty),
                    .EntityTaxCode = If(n.cif_ep, String.Empty),
                    .Signature = If(n.semnatura, String.Empty),
                    .Sent = n.trimis,
                    .SentAt = ParseIsoDateTime(n.data_trimitere),
                    .SentAnswer = If(n.raspuns_trimitere, String.Empty),
                    .PdfSha256 = If(n.pdf_sha256, String.Empty),
                    .RegistrationIndex = If(n.index_inregistrare, String.Empty),
                    .Receipt = FromWire(n.recipisa)}
                For Each c As CabCorrectionWire In If(n.corectii, New List(Of CabCorrectionWire)())
                    note.Corrections.Add(FromWire(c))
                Next
                result.Add(note)
            Next
            Return result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetCabNotesAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function MarkCabNoteSentAsync(idNc As Integer, answer As String, registrationIndex As String,
                                               ct As CancellationToken) As Task _
        Implements ICabNotesApi.MarkCabNoteSentAsync
        Try
            Dim body As New CabSentWire With {
                .raspuns = If(answer, String.Empty),
                .index = If(String.IsNullOrWhiteSpace(registrationIndex), Nothing, registrationIndex.Trim())}
            Await SendCabAsync(HttpMethod.Post, $"/api/forexe/note-cab/{idNc}/trimitere",
                               JsonSerializer.Serialize(body, _json),
                               "marcarea notei ca trimisă", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.MarkCabNoteSentAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveCabNoteReceiptAsync(idNc As Integer, receipt As CabNoteReceipt, content As Byte(),
                                                  ct As CancellationToken) As Task(Of CabNoteReceipt) _
        Implements ICabNotesApi.SaveCabNoteReceiptAsync
        Try
            ArgumentNullException.ThrowIfNull(receipt)
            If content Is Nothing OrElse content.Length = 0 Then Throw New ArgumentException("Empty receipt.", NameOf(content))
            Dim body As New CabReceiptWire With {
                .index = receipt.RegistrationIndex,
                .numar_inregistrare = receipt.RegistrationNumber,
                .id_mesaj = If(receipt.MessageId > 0, receipt.MessageId.ToString(CultureInfo.InvariantCulture), Nothing),
                .descriere = receipt.MessageText,
                .data_mesaj = If(receipt.MessageDate.HasValue,
                                 receipt.MessageDate.Value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture), Nothing),
                .nume_fisier = receipt.FileName,
                .continut = Convert.ToBase64String(content)}
            Dim text As String = Await SendCabAsync(HttpMethod.Post, $"/api/forexe/note-cab/{idNc}/recipisa",
                                                    JsonSerializer.Serialize(body, _json),
                                                    "salvarea recipisei FOREXE", ct).ConfigureAwait(False)
            Dim saved As CabReceiptSavedWire = JsonSerializer.Deserialize(Of CabReceiptSavedWire)(text, _json)
            Dim result As CabNoteReceipt = FromWire(saved?.recipisa)
            If result Is Nothing Then Throw New ApiException("Serverul nu a întors recipisa salvată.", 200)
            Return result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveCabNoteReceiptAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function DownloadCabNoteReceiptAsync(idReceipt As Integer, ct As CancellationToken) As Task(Of Byte()) _
        Implements ICabNotesApi.DownloadCabNoteReceiptAsync
        Try
            EnsureConfigured()
            Using msg As New HttpRequestMessage(HttpMethod.Get, $"/api/forexe/note-cab/recipisa/{idReceipt}")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Dim text As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                        Throw BuildApiException(text, "descărcarea recipisei FOREXE", CInt(resp.StatusCode))
                    End If
                    Return Await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(False)
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.DownloadCabNoteReceiptAsync", ex)
            Throw
        End Try
    End Function

    Private Shared Function FromWire(r As CabReceiptWire) As CabNoteReceipt
        If r Is Nothing OrElse r.idrcp <= 0 Then Return Nothing
        Dim messageId As Long
        Long.TryParse(If(r.id_mesaj, String.Empty), NumberStyles.None, CultureInfo.InvariantCulture, messageId)
        Return New CabNoteReceipt With {
            .IdReceipt = r.idrcp,
            .RegistrationIndex = If(r.index, String.Empty),
            .RegistrationNumber = If(r.numar_inregistrare, String.Empty),
            .MessageId = messageId,
            .MessageText = If(r.descriere, String.Empty),
            .MessageDate = ParseIsoDateTime(r.data_mesaj),
            .FileName = If(r.nume_fisier, String.Empty),
            .Size = r.dimensiune,
            .Sha256 = If(r.sha256, String.Empty)}
    End Function

    Public Function DownloadCabNotePdfAsync(idNc As Integer, cachedSha As String, ct As CancellationToken) _
        As Task(Of PdfDownloadResult) Implements ICabNotesApi.DownloadCabNotePdfAsync
        Return DownloadPdfAsync($"/api/forexe/nc/pdf/{idNc}", cachedSha,
                                "descărcarea PDF-ului notei de corecție",
                                "ApiClient.DownloadCabNotePdfAsync", ct)
    End Function

    Public Function UploadCabNotePdfAsync(idNc As Integer, continut As Byte(), shaPrecedent As String,
                                          semnatura As String, semnaturi As IReadOnlyList(Of PdfSignatureRecord),
                                          ct As CancellationToken) _
        As Task(Of PutPdfResponse) Implements ICabNotesApi.UploadCabNotePdfAsync
        Return UploadPdfAsync($"/api/forexe/nc/pdf/{idNc}", continut, shaPrecedent, semnatura, semnaturi,
                              "salvarea PDF-ului notei de corecție",
                              "ApiClient.UploadCabNotePdfAsync", ct)
    End Function

    Private Shared Function ToWire(c As CabNoteCorrection) As CabCorrectionWire
        Return New CabCorrectionWire With {
            .idfxp = If(c.IdFxp > 0, c.IdFxp, CType(Nothing, Integer?)),
            .referinta_trezor = c.TreasuryReference,
            .nr_doc = c.DocumentNumber,
            .simbol_cont = c.AccountSymbol,
            .cod_program = c.ProgramCode,
            .data_oper_initiala = IsoDate(c.OriginalOperationDate),
            .coloana = c.AmountColumn,
            .suma = c.Amount,
            .cod_angajament = c.CommitmentCode,
            .cod_indicator = c.IndicatorCode,
            .cod_ai = c.CodAi,
            .simbol_cont_corectie = c.CorrectionAccountSymbol,
            .cod_program_corectie = c.CorrectionProgramCode,
            .explicatii = c.Explanation}
    End Function

    Private Shared Function FromWire(c As CabCorrectionWire) As CabNoteCorrection
        Return New CabNoteCorrection With {
            .IdFxp = c.idfxp.GetValueOrDefault(),
            .TreasuryReference = If(c.referinta_trezor, String.Empty),
            .DocumentNumber = If(c.nr_doc, String.Empty),
            .AccountSymbol = If(c.simbol_cont, String.Empty),
            .ProgramCode = If(c.cod_program, String.Empty),
            .OriginalOperationDate = ParseIsoDate(c.data_oper_initiala).GetValueOrDefault(),
            .AmountColumn = If(c.coloana, "C"),
            .Amount = c.suma,
            .CommitmentCode = If(c.cod_angajament, String.Empty),
            .IndicatorCode = If(c.cod_indicator, String.Empty),
            .CodAi = If(c.cod_ai, String.Empty),
            .CorrectionAccountSymbol = If(c.simbol_cont_corectie, String.Empty),
            .CorrectionProgramCode = If(c.cod_program_corectie, String.Empty),
            .Explanation = If(c.explicatii, String.Empty)}
    End Function

    ' One request of this family: bearer, optional JSON body, Romanian ApiException on a non-2xx.
    Private Async Function SendCabAsync(method As HttpMethod, url As String, jsonBody As String,
                                        action As String, ct As CancellationToken) As Task(Of String)
        EnsureConfigured()
        Using msg As New HttpRequestMessage(method, url)
            msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
            If jsonBody IsNot Nothing Then msg.Content = New StringContent(jsonBody, Encoding.UTF8, "application/json")
            Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                Dim text As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                If Not resp.IsSuccessStatusCode Then Throw BuildApiException(text, action, CInt(resp.StatusCode))
                Return text
            End Using
        End Using
    End Function

    Private Shared Function IsoDate(value As Date) As String
        Return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    End Function

    Private Shared Function ParseIsoDate(text As String) As Date?
        Dim d As Date
        If Date.TryParseExact(If(text, String.Empty), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then Return d
        Return Nothing
    End Function

    Private Shared Function ParseIsoDateTime(text As String) As DateTime?
        Dim d As DateTime
        If DateTime.TryParse(If(text, String.Empty), CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then Return d
        Return Nothing
    End Function

End Class
