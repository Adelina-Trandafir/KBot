Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slices KE01..KE07 -- /api/efactura/primite/{id}/ke... and /api/efactura/ke/...: PYTHON/routes/efactura/primite_ke_routes.py.
' Wire names are exactly the server's (the JSON options keep the property names as written). Every method is a boundary:
' log + rethrow (an ApiException keeps the server's Romanian text and reason).
Partial Public Class ApiClient

    Private NotInheritable Class KeLinieWire
        Public Property IdLinie As Integer
        Public Property NrLinie As String
        Public Property Denumire As String
        Public Property Cpv As String
        Public Property Valoare As Decimal
        Public Property CotaTVA As Decimal
        Public Property CotaDedusa As Boolean
        Public Property ValoareCuTVA As Decimal
        Public Property IdClsf As Integer?
        Public Property Clsf As String
    End Class

    Private NotInheritable Class KeClasificatieWire
        Public Property IdClsf As Integer
        Public Property Clsf As String
        Public Property Valoare As Decimal
        Public Property Alocat As Decimal
        Public Property Rest As Decimal
    End Class

    Private NotInheritable Class KeLegaturaWire
        Public Property IdLegatura As Integer
        Public Property CodAngajament As String
        Public Property IdClsf As Integer
        Public Property Clsf As String
        Public Property Suma As Decimal
        Public Property Tip As String
        Public Property UN As String
        Public Property IDRR As Integer?
        Public Property IDRH As Integer?
        Public Property IDR As Integer?
        Public Property DataR As String
        Public Property TipReceptie As String
        Public Property DescriereR As String
        Public Property AsteaptaDescarcarea As Boolean
        Public Property StearsaPeSite As Boolean
    End Class

    Private NotInheritable Class KeAngajamentWire
        Public Property CodAngajament As String
        Public Property Descriere As String
        Public Property Stare As String
        Public Property Sursa As String
    End Class

    Private NotInheritable Class KeStareWire
        Public Property poate_scrie As Boolean
        Public Property total As Decimal
        Public Property alocat As Decimal
        Public Property rest As Decimal
        Public Property stare As String
        Public Property nealocat_liniilor As Decimal
        Public Property fara_clasificatie As Decimal
        Public Property linii As List(Of KeLinieWire)
        Public Property pe_clasificatie As List(Of KeClasificatieWire)
        Public Property legaturi As List(Of KeLegaturaWire)
        Public Property angajamente As List(Of KeAngajamentWire)
    End Class

    Private NotInheritable Class KeAngajamentAlegereWire
        Public Property CodAngajament As String
        Public Property Descriere As String
        Public Property [Alias] As String
        Public Property Stare As String
        Public Property DataCreare As String
        Public Property Parteneri As String
    End Class

    Private NotInheritable Class KeAngajamenteListWire
        Public Property angajamente As List(Of KeAngajamentAlegereWire)
    End Class

    Private NotInheritable Class KeIndicatorWire
        Public Property CodAI As String
        Public Property CodIndicator As String
        Public Property IdClsf As Integer?
        Public Property Clsf As String
        Public Property Denumire As String
        Public Property AngajamentLegal As Decimal
        Public Property Receptii As Decimal
        Public Property Plati As Decimal
        Public Property SS As String
    End Class

    Private NotInheritable Class KeReceptieWire
        Public Property IDRR As Integer
        Public Property DataR As String
        Public Property TipReceptie As String
        Public Property Descriere As String
        Public Property IDRH As Integer?
        Public Property Total As Decimal
        Public Property Idrhs As List(Of Integer)
        Public Property pe_clasificatie As List(Of KeClasificatieWire)
    End Class

    Private NotInheritable Class KeReceptiiWire
        Public Property indicatori As List(Of KeIndicatorWire)
        Public Property receptii As List(Of KeReceptieWire)
    End Class

    Private NotInheritable Class KeDisponibilIndWire
        Public Property IdClsf As Integer?
        Public Property CodIndicator As String
        Public Property Buget As Decimal?
        Public Property Receptii As Decimal
    End Class

    Private NotInheritable Class KeDisponibilWire
        Public Property indicatori As List(Of KeDisponibilIndWire)
        Public Property avertismente As List(Of String)
    End Class

    Private NotInheritable Class KeRandWire
        Public Property IdClsf As Integer
        Public Property CodAI As String
        Public Property CodIndicator As String
        Public Property Clsf As String
        Public Property ClsfSal As String
        Public Property SS As String
        Public Property Suma As Decimal
        Public Property ValoareVeche As Decimal?
        Public Property ValoareNoua As Decimal?
    End Class

    Private NotInheritable Class KeVerificareWire
        Public Property ok As Boolean
        Public Property probleme As List(Of String)
        Public Property avertismente As List(Of String)
        Public Property randuri As List(Of KeRandWire)
    End Class

    ' ── The calls ───────────────────────────────────────────────────────────────

    Public Async Function GetKeAsync(k_idPrimita As Integer, ct As CancellationToken) _
        As Task(Of EFacturaKeStare) Implements IEFacturaApi.GetKeAsync
        Try
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Get, PrimitaPath(k_idPrimita, "ke"), Nothing,
                                                           "citirea legăturilor facturii cu recepțiile", ct).ConfigureAwait(False)
            Return ToKeStare(k_text)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetKeAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SetKeClasificatiiAsync(k_idPrimita As Integer, k_linii As IEnumerable(Of EFacturaKeLinie), ct As CancellationToken) _
        As Task(Of EFacturaKeStare) Implements IEFacturaApi.SetKeClasificatiiAsync
        Try
            ArgumentNullException.ThrowIfNull(k_linii)
            Dim k_items As New List(Of Dictionary(Of String, Object))()
            For Each k_l As EFacturaKeLinie In k_linii
                k_items.Add(New Dictionary(Of String, Object) From {{"IdLinie", k_l.IdLinie}, {"IdClsf", k_l.IdClsf}})
            Next
            Dim k_body As String = JsonSerializer.Serialize(New Dictionary(Of String, Object) From {{"linii", k_items}}, _json)
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Put, PrimitaPath(k_idPrimita, "ke/clasificatii"), k_body,
                                                           "salvarea clasificațiilor liniilor", ct).ConfigureAwait(False)
            Return ToKeStare(k_text)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SetKeClasificatiiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetKeAngajamenteAsync(k_query As String, ct As CancellationToken) _
        As Task(Of List(Of EFacturaKeAngajamentAlegere)) Implements IEFacturaApi.GetKeAngajamenteAsync
        Try
            Dim k_path As String = "/api/efactura/ke/angajamente" &
                If(String.IsNullOrWhiteSpace(k_query), String.Empty, "?q=" & Uri.EscapeDataString(k_query.Trim()))
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Get, k_path, Nothing, "citirea angajamentelor", ct).ConfigureAwait(False)
            Dim wire As KeAngajamenteListWire = JsonSerializer.Deserialize(Of KeAngajamenteListWire)(k_text, _json)
            Dim k_result As New List(Of EFacturaKeAngajamentAlegere)()
            If wire Is Nothing OrElse wire.angajamente Is Nothing Then Return k_result
            For Each w As KeAngajamentAlegereWire In wire.angajamente
                k_result.Add(New EFacturaKeAngajamentAlegere() With {
                    .CodAngajament = If(w.CodAngajament, String.Empty), .Descriere = If(w.Descriere, String.Empty),
                    .AliasName = If(w.[Alias], String.Empty), .Stare = If(w.Stare, String.Empty),
                    .DataCreare = ParseDay(w.DataCreare), .Parteneri = If(w.Parteneri, String.Empty)})
            Next
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetKeAngajamenteAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetKeReceptiiAsync(k_cod As String, ct As CancellationToken) _
        As Task(Of EFacturaKeReceptiiAngajament) Implements IEFacturaApi.GetKeReceptiiAsync
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_cod)
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Get,
                "/api/efactura/ke/angajament/" & Uri.EscapeDataString(k_cod.Trim()) & "/receptii", Nothing,
                "citirea recepțiilor angajamentului", ct).ConfigureAwait(False)
            Dim wire As KeReceptiiWire = JsonSerializer.Deserialize(Of KeReceptiiWire)(k_text, _json)
            Dim k_result As New EFacturaKeReceptiiAngajament()
            If wire Is Nothing Then Return k_result
            If wire.indicatori IsNot Nothing Then
                For Each w As KeIndicatorWire In wire.indicatori
                    k_result.Indicatori.Add(New EFacturaKeIndicator() With {
                        .CodAi = If(w.CodAI, String.Empty), .CodIndicator = If(w.CodIndicator, String.Empty), .IdClsf = w.IdClsf,
                        .Clsf = If(w.Clsf, String.Empty), .Denumire = If(w.Denumire, String.Empty),
                        .AngajamentLegal = w.AngajamentLegal, .Receptii = w.Receptii, .Plati = w.Plati, .Ss = If(w.SS, String.Empty)})
                Next
            End If
            If wire.receptii IsNot Nothing Then
                For Each w As KeReceptieWire In wire.receptii
                    Dim k_r As New EFacturaKeReceptie() With {
                        .Idrr = w.IDRR, .DataR = ParseDay(w.DataR), .TipReceptie = If(w.TipReceptie, String.Empty),
                        .Descriere = If(w.Descriere, String.Empty), .Idrh = w.IDRH, .Total = w.Total}
                    If w.Idrhs IsNot Nothing Then k_r.Idrhs.AddRange(w.Idrhs)
                    If w.pe_clasificatie IsNot Nothing Then
                        For Each c As KeClasificatieWire In w.pe_clasificatie
                            k_r.PeClasificatie.Add(New EFacturaKeReceptieClasificatie() With {
                                .IdClsf = c.IdClsf, .Clsf = If(c.Clsf, String.Empty), .Valoare = c.Valoare, .Alocat = c.Alocat, .Rest = c.Rest})
                        Next
                    End If
                    k_result.Receptii.Add(k_r)
                Next
            End If
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetKeReceptiiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetKeDisponibilAsync(k_cod As String, k_zi As Date, ct As CancellationToken) _
        As Task(Of EFacturaKeDisponibilZi) Implements IEFacturaApi.GetKeDisponibilAsync
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_cod)
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Get,
                "/api/efactura/ke/angajament/" & Uri.EscapeDataString(k_cod.Trim()) & "/disponibil?data=" & k_zi.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Nothing,
                "citirea bugetului și a recepțiilor pe indicatori", ct).ConfigureAwait(False)
            Dim wire As KeDisponibilWire = JsonSerializer.Deserialize(Of KeDisponibilWire)(k_text, _json)
            Dim k_result As New EFacturaKeDisponibilZi()
            If wire Is Nothing Then Return k_result
            If wire.indicatori IsNot Nothing Then
                For Each w As KeDisponibilIndWire In wire.indicatori
                    k_result.Indicatori.Add(New EFacturaKeDisponibil() With {
                        .IdClsf = w.IdClsf, .CodIndicator = If(w.CodIndicator, String.Empty), .Buget = w.Buget, .Receptii = w.Receptii})
                Next
            End If
            If wire.avertismente IsNot Nothing Then k_result.Avertismente.AddRange(wire.avertismente)
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetKeDisponibilAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function VerifyKeAsync(k_idPrimita As Integer, k_cerere As EFacturaKeCerere, ct As CancellationToken) _
        As Task(Of EFacturaKeVerificare) Implements IEFacturaApi.VerifyKeAsync
        Try
            ArgumentNullException.ThrowIfNull(k_cerere)
            Dim k_body As New Dictionary(Of String, Object) From {
                {"CodAngajament", k_cerere.CodAngajament}, {"Tip", k_cerere.Tip}, {"alocari", AlocariOf(k_cerere)}}
            If k_cerere.Idrr.HasValue Then k_body("IDRR") = k_cerere.Idrr.Value
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Post, PrimitaPath(k_idPrimita, "ke/verifica"),
                                                           JsonSerializer.Serialize(k_body, _json),
                                                           "verificarea alocării pe recepție", ct).ConfigureAwait(False)
            Dim wire As KeVerificareWire = JsonSerializer.Deserialize(Of KeVerificareWire)(k_text, _json)
            If wire Is Nothing Then Throw New ApiException("Serverul nu a trimis rezultatul verificării.")
            Dim k_result As New EFacturaKeVerificare() With {.Ok = wire.ok}
            If wire.probleme IsNot Nothing Then k_result.Probleme.AddRange(wire.probleme)
            If wire.avertismente IsNot Nothing Then k_result.Avertismente.AddRange(wire.avertismente)
            If wire.randuri IsNot Nothing Then
                For Each w As KeRandWire In wire.randuri
                    k_result.Randuri.Add(New EFacturaKeRand() With {
                        .IdClsf = w.IdClsf, .CodAi = If(w.CodAI, String.Empty), .CodIndicator = If(w.CodIndicator, String.Empty),
                        .Clsf = If(w.Clsf, String.Empty), .ClsfSal = If(w.ClsfSal, String.Empty), .Ss = If(w.SS, String.Empty),
                        .Suma = w.Suma, .ValoareVeche = w.ValoareVeche, .ValoareNoua = w.ValoareNoua})
                Next
            End If
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.VerifyKeAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function AddKeLegaturiAsync(k_idPrimita As Integer, k_cerere As EFacturaKeCerere, ct As CancellationToken) _
        As Task(Of EFacturaKeStare) Implements IEFacturaApi.AddKeLegaturiAsync
        Try
            ArgumentNullException.ThrowIfNull(k_cerere)
            Dim k_body As New Dictionary(Of String, Object) From {
                {"CodAngajament", k_cerere.CodAngajament}, {"Tip", k_cerere.Tip}, {"alocari", AlocariOf(k_cerere)}}
            If k_cerere.Idrr.HasValue Then k_body("IDRR") = k_cerere.Idrr.Value
            If k_cerere.Idrh.HasValue Then k_body("IDRH") = k_cerere.Idrh.Value
            If k_cerere.Idr.HasValue Then k_body("IDR") = k_cerere.Idr.Value
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Post, PrimitaPath(k_idPrimita, "ke/legaturi"),
                                                           JsonSerializer.Serialize(k_body, _json),
                                                           "legarea facturii de recepție", ct).ConfigureAwait(False)
            Return ToKeStare(k_text)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.AddKeLegaturiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function RemoveKeLegaturaAsync(k_idPrimita As Integer, k_idLegatura As Integer, ct As CancellationToken) _
        As Task(Of EFacturaKeStare) Implements IEFacturaApi.RemoveKeLegaturaAsync
        Try
            If k_idLegatura <= 0 Then Throw New ArgumentException("The link id is required.", NameOf(k_idLegatura))
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Delete,
                PrimitaPath(k_idPrimita, "ke/legaturi/" & k_idLegatura.ToString(CultureInfo.InvariantCulture)), Nothing,
                "scoaterea legăturii cu recepția", ct).ConfigureAwait(False)
            Return ToKeStare(k_text)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.RemoveKeLegaturaAsync", ex)
            Throw
        End Try
    End Function

    ' ── Helpers ─────────────────────────────────────────────────────────────────

    Private Shared Function AlocariOf(k_cerere As EFacturaKeCerere) As List(Of Dictionary(Of String, Object))
        Dim k_list As New List(Of Dictionary(Of String, Object))()
        For Each k_a As EFacturaKeAlocare In k_cerere.Alocari
            k_list.Add(New Dictionary(Of String, Object) From {{"IdClsf", k_a.IdClsf}, {"Suma", k_a.Suma}})
        Next
        Return k_list
    End Function

    Private Shared Function ToKeStare(k_text As String) As EFacturaKeStare
        Dim wire As KeStareWire = JsonSerializer.Deserialize(Of KeStareWire)(k_text, _json)
        If wire Is Nothing Then Throw New ApiException("Serverul nu a trimis legăturile facturii.")
        Dim k_result As New EFacturaKeStare() With {
            .PoateScrie = wire.poate_scrie, .Total = wire.total, .Alocat = wire.alocat, .Rest = wire.rest,
            .Stare = If(wire.stare, "nelegata"), .NealocatLiniilor = wire.nealocat_liniilor, .FaraClasificatie = wire.fara_clasificatie}
        If wire.linii IsNot Nothing Then
            For Each w As KeLinieWire In wire.linii
                k_result.Linii.Add(New EFacturaKeLinie() With {
                    .IdLinie = w.IdLinie, .NrLinie = If(w.NrLinie, String.Empty), .Denumire = If(w.Denumire, String.Empty),
                    .Cpv = If(w.Cpv, String.Empty), .Valoare = w.Valoare, .CotaTva = w.CotaTVA, .CotaDedusa = w.CotaDedusa,
                    .ValoareCuTva = w.ValoareCuTVA, .IdClsf = w.IdClsf, .Clsf = If(w.Clsf, String.Empty)})
            Next
        End If
        If wire.pe_clasificatie IsNot Nothing Then
            For Each w As KeClasificatieWire In wire.pe_clasificatie
                k_result.PeClasificatie.Add(New EFacturaKeClasificatie() With {
                    .IdClsf = w.IdClsf, .Clsf = If(w.Clsf, String.Empty), .Valoare = w.Valoare, .Alocat = w.Alocat, .Rest = w.Rest})
            Next
        End If
        If wire.legaturi IsNot Nothing Then
            For Each w As KeLegaturaWire In wire.legaturi
                k_result.Legaturi.Add(New EFacturaKeLegatura() With {
                    .IdLegatura = w.IdLegatura, .CodAngajament = If(w.CodAngajament, String.Empty), .IdClsf = w.IdClsf,
                    .Clsf = If(w.Clsf, String.Empty), .Suma = w.Suma, .Tip = If(w.Tip, EFacturaKeTip.Existenta), .Un = If(w.UN, String.Empty),
                    .Idrr = w.IDRR, .Idrh = w.IDRH, .Idr = w.IDR, .DataR = ParseDay(w.DataR), .TipReceptie = If(w.TipReceptie, String.Empty),
                    .DescriereR = If(w.DescriereR, String.Empty), .AsteaptaDescarcarea = w.AsteaptaDescarcarea, .StearsaPeSite = w.StearsaPeSite})
            Next
        End If
        If wire.angajamente IsNot Nothing Then
            For Each w As KeAngajamentWire In wire.angajamente
                k_result.Angajamente.Add(New EFacturaKeAngajament() With {
                    .CodAngajament = If(w.CodAngajament, String.Empty), .Descriere = If(w.Descriere, String.Empty),
                    .Stare = If(w.Stare, String.Empty), .Sursa = If(w.Sursa, String.Empty)})
            Next
        End If
        Return k_result
    End Function

End Class
