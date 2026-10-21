Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 00EF-47 (EV03) -- /api/efactura/vercon/... and /api/efactura/primite/{id}/vercon...: PYTHON/routes/efactura/primite_vercon_routes.py (slice 00EF-45).
' Wire names are exactly the server's (the JSON options keep the property names as written; matching is case-sensitive). Every method is a boundary:
' log + rethrow (an ApiException keeps the server's Romanian text and reason).
Partial Public Class ApiClient

    Private NotInheritable Class VerconContWire
        Public Property IdMasina As Integer
        Public Property Masina As String
    End Class

    Private NotInheritable Class VerconGestiuneWire
        Public Property SubunitId As Integer
        Public Property GestiuneId As Integer
        Public Property Subunitate As String
        Public Property Gestiune As String
        Public Property Tip As String
        Public Property conturi As List(Of VerconContWire)
    End Class

    Private NotInheritable Class VerconConturiWire
        Public Property gestiuni As List(Of VerconGestiuneWire)
    End Class

    Private NotInheritable Class VerconLinieWire
        Public Property IdLinie As Integer
        Public Property NrLinie As String
        Public Property Denumire As String
        Public Property Cpv As String
        Public Property Unit As String
        Public Property Cant As Decimal
        Public Property Pret As Decimal
        Public Property Valoare As Decimal
        Public Property ValoareCuTVA As Decimal
        Public Property IdMasina As Integer?
        Public Property Cont As String
        Public Property Gestiune As String
    End Class

    Private NotInheritable Class VerconNirWire
        Public Property SubunitId As Integer
        Public Property GestiuneId As Integer
        Public Property Gestiune As String
        Public Property ExercitiuId As Integer
        Public Property IdIntrare As Integer
        Public Property Numar As Integer?
        Public Property NumarSub As Integer?
        Public Property Data As String
        Public Property Anulat As Boolean
        Public Property Valid As Boolean
    End Class

    Private NotInheritable Class VerconTrimitereWire
        Public Property IdTrimitere As Integer
        Public Property Stare As String
        Public Property StareSalvata As String
        Public Property Revenita As Boolean
        Public Property Observatie As String
        Public Property UnTrimis As String
        Public Property DataTrimis As String
        Public Property MotivReturnare As String
        Public Property UnReturnat As String
        Public Property DataReturnat As String
        Public Property UnPreluat As String
        Public Property DataPreluat As String
        Public Property nir As List(Of VerconNirWire)
    End Class

    Private NotInheritable Class VerconFacturaWire
        Public Property IdPrimita As Integer
        Public Property NrFact As String
        Public Property DataFact As String
        Public Property Furnizor As String
        Public Property CuiNormalizat As String
        Public Property Total As Decimal
    End Class

    Private NotInheritable Class VerconPartenerWire
        Public Property IdPartener As Integer
        Public Property Denumire As String
    End Class

    Private NotInheritable Class VerconDuplicatWire
        Public Property Gestiune As String
        Public Property Numar As Integer?
        Public Property NumarSub As Integer?
        Public Property IdIntrare As Integer
        Public Property Data As String
    End Class

    Private NotInheritable Class VerconStareWire
        Public Property poate_scrie As Boolean
        Public Property poate_trimite As Boolean
        Public Property motiv As String
        Public Property factura As VerconFacturaWire
        Public Property partener As VerconPartenerWire
        Public Property duplicate As List(Of VerconDuplicatWire)
        Public Property trimitere As VerconTrimitereWire
        Public Property istoric As List(Of VerconTrimitereWire)
        Public Property linii As List(Of VerconLinieWire)
    End Class

    Private NotInheritable Class VerconSumarNirWire
        Public Property Gestiune As String
        Public Property Numar As Integer?
        Public Property NumarSub As Integer?
    End Class

    Private NotInheritable Class VerconSumarWire
        Public Property IdPrimita As Integer
        Public Property IdTrimitere As Integer
        Public Property Stare As String
        Public Property Revenita As Boolean
        Public Property MotivReturnare As String
        Public Property Nir As List(Of VerconSumarNirWire)
    End Class

    Private NotInheritable Class VerconStariWire
        Public Property trimiteri As List(Of VerconSumarWire)
    End Class

    ' ── The calls ───────────────────────────────────────────────────────────────

    Public Async Function GetVerconConturiAsync(ct As CancellationToken) _
        As Task(Of List(Of EFacturaVerconGestiune)) Implements IEFacturaApi.GetVerconConturiAsync
        Try
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Get, "/api/efactura/vercon/conturi", Nothing,
                                                           "citirea conturilor din Vercon", ct).ConfigureAwait(False)
            Dim wire As VerconConturiWire = JsonSerializer.Deserialize(Of VerconConturiWire)(k_text, _json)
            Dim k_result As New List(Of EFacturaVerconGestiune)()
            If wire Is Nothing OrElse wire.gestiuni Is Nothing Then Return k_result
            For Each w As VerconGestiuneWire In wire.gestiuni
                Dim k_g As New EFacturaVerconGestiune() With {
                    .SubunitId = w.SubunitId, .GestiuneId = w.GestiuneId, .Subunitate = If(w.Subunitate, String.Empty),
                    .Gestiune = If(w.Gestiune, String.Empty), .Tip = If(w.Tip, String.Empty)}
                If w.conturi IsNot Nothing Then
                    For Each c As VerconContWire In w.conturi
                        k_g.Conturi.Add(New EFacturaVerconCont() With {.IdMasina = c.IdMasina, .Masina = If(c.Masina, String.Empty)})
                    Next
                End If
                k_result.Add(k_g)
            Next
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetVerconConturiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetVerconStariAsync(ct As CancellationToken) _
        As Task(Of List(Of EFacturaVerconSumar)) Implements IEFacturaApi.GetVerconStariAsync
        Try
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Get, "/api/efactura/vercon/stari", Nothing,
                                                           "citirea stării facturilor trimise la Vercon", ct).ConfigureAwait(False)
            Dim wire As VerconStariWire = JsonSerializer.Deserialize(Of VerconStariWire)(k_text, _json)
            Dim k_result As New List(Of EFacturaVerconSumar)()
            If wire Is Nothing OrElse wire.trimiteri Is Nothing Then Return k_result
            For Each w As VerconSumarWire In wire.trimiteri
                Dim k_s As New EFacturaVerconSumar() With {
                    .IdPrimita = w.IdPrimita, .IdTrimitere = w.IdTrimitere, .Stare = If(w.Stare, EFacturaVerconStare.Trimisa),
                    .Revenita = w.Revenita, .MotivReturnare = If(w.MotivReturnare, String.Empty)}
                If w.Nir IsNot Nothing Then
                    For Each n As VerconSumarNirWire In w.Nir
                        k_s.Nir.Add(NirText(n.Gestiune, n.Numar, n.NumarSub))
                    Next
                End If
                k_result.Add(k_s)
            Next
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetVerconStariAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetVerconAsync(k_idPrimita As Integer, ct As CancellationToken) _
        As Task(Of EFacturaVerconStareFactura) Implements IEFacturaApi.GetVerconAsync
        Try
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Get, PrimitaPath(k_idPrimita, "vercon"), Nothing,
                                                           "citirea trimiterii facturii la Vercon", ct).ConfigureAwait(False)
            Return ToVerconStare(k_text)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetVerconAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SendVerconAsync(k_idPrimita As Integer, k_conturi As IEnumerable(Of EFacturaVerconContLinie), k_observatie As String, ct As CancellationToken) _
        As Task(Of EFacturaVerconStareFactura) Implements IEFacturaApi.SendVerconAsync
        Try
            ArgumentNullException.ThrowIfNull(k_conturi)
            Dim k_body As String = JsonSerializer.Serialize(New Dictionary(Of String, Object) From {
                {"conturi", ContLinii(k_conturi)}, {"observatie", If(String.IsNullOrWhiteSpace(k_observatie), Nothing, k_observatie.Trim())}}, _json)
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Post, PrimitaPath(k_idPrimita, "vercon"), k_body,
                                                           "trimiterea facturii la Vercon", ct).ConfigureAwait(False)
            Return ToVerconStare(k_text)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SendVerconAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SetVerconConturiAsync(k_idPrimita As Integer, k_conturi As IEnumerable(Of EFacturaVerconContLinie), ct As CancellationToken) _
        As Task(Of EFacturaVerconStareFactura) Implements IEFacturaApi.SetVerconConturiAsync
        Try
            ArgumentNullException.ThrowIfNull(k_conturi)
            Dim k_body As String = JsonSerializer.Serialize(New Dictionary(Of String, Object) From {{"conturi", ContLinii(k_conturi)}}, _json)
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Put, PrimitaPath(k_idPrimita, "vercon/conturi"), k_body,
                                                           "schimbarea conturilor propuse pentru Vercon", ct).ConfigureAwait(False)
            Return ToVerconStare(k_text)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SetVerconConturiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function WithdrawVerconAsync(k_idPrimita As Integer, k_idTrimitere As Integer, ct As CancellationToken) _
        As Task(Of EFacturaVerconStareFactura) Implements IEFacturaApi.WithdrawVerconAsync
        Try
            If k_idTrimitere <= 0 Then Throw New ArgumentException("The sending id is required.", NameOf(k_idTrimitere))
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Delete,
                PrimitaPath(k_idPrimita, "vercon/" & k_idTrimitere.ToString(CultureInfo.InvariantCulture)), Nothing,
                "retragerea facturii de la Vercon", ct).ConfigureAwait(False)
            Return ToVerconStare(k_text)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.WithdrawVerconAsync", ex)
            Throw
        End Try
    End Function

    ' ── Helpers ─────────────────────────────────────────────────────────────────

    Private Shared Function ContLinii(k_conturi As IEnumerable(Of EFacturaVerconContLinie)) As List(Of Dictionary(Of String, Object))
        Dim k_list As New List(Of Dictionary(Of String, Object))()
        For Each k_c As EFacturaVerconContLinie In k_conturi
            k_list.Add(New Dictionary(Of String, Object) From {{"IdLinie", k_c.IdLinie}, {"IdMasina", k_c.IdMasina}})
        Next
        Return k_list
    End Function

    ' «Gestiune: 123» -- the number of a NIR with its fraction (707.1 = Numar 707 + NumarSub 10, in hundredths).
    Private Shared Function NirText(k_gestiune As String, k_numar As Integer?, k_numarSub As Integer?) As String
        Dim k_number As String = If(k_numar.HasValue, k_numar.Value.ToString(CultureInfo.InvariantCulture), "?")
        If k_numarSub.HasValue AndAlso k_numarSub.Value <> 0 Then
            k_number &= "." & k_numarSub.Value.ToString("00", CultureInfo.InvariantCulture).TrimEnd("0"c)
        End If
        Return If(String.IsNullOrWhiteSpace(k_gestiune), k_number, k_gestiune & ": " & k_number)
    End Function

    Private Shared Function ToVerconTrimitere(w As VerconTrimitereWire) As EFacturaVerconTrimitere
        Dim k_t As New EFacturaVerconTrimitere() With {
            .IdTrimitere = w.IdTrimitere, .Stare = If(w.Stare, EFacturaVerconStare.Trimisa), .StareSalvata = If(w.StareSalvata, EFacturaVerconStare.Trimisa),
            .Revenita = w.Revenita, .Observatie = If(w.Observatie, String.Empty), .UnTrimis = If(w.UnTrimis, String.Empty), .DataTrimis = ParseDay(w.DataTrimis),
            .MotivReturnare = If(w.MotivReturnare, String.Empty), .UnReturnat = If(w.UnReturnat, String.Empty), .DataReturnat = ParseDay(w.DataReturnat),
            .UnPreluat = If(w.UnPreluat, String.Empty), .DataPreluat = ParseDay(w.DataPreluat)}
        If w.nir IsNot Nothing Then
            For Each n As VerconNirWire In w.nir
                k_t.Nir.Add(New EFacturaVerconNir() With {
                    .SubunitId = n.SubunitId, .GestiuneId = n.GestiuneId, .Gestiune = If(n.Gestiune, String.Empty), .ExercitiuId = n.ExercitiuId,
                    .IdIntrare = n.IdIntrare, .Numar = n.Numar, .NumarSub = n.NumarSub, .Data = ParseDay(n.Data), .Anulat = n.Anulat, .Valid = n.Valid})
            Next
        End If
        Return k_t
    End Function

    Private Shared Function ToVerconStare(k_text As String) As EFacturaVerconStareFactura
        Dim wire As VerconStareWire = JsonSerializer.Deserialize(Of VerconStareWire)(k_text, _json)
        If wire Is Nothing Then Throw New ApiException("Serverul nu a trimis starea facturii pentru Vercon.")
        Dim k_result As New EFacturaVerconStareFactura() With {
            .PoateScrie = wire.poate_scrie, .PoateTrimite = wire.poate_trimite, .Motiv = If(wire.motiv, String.Empty)}
        If wire.factura IsNot Nothing Then
            k_result.Factura = New EFacturaVerconFactura() With {
                .IdPrimita = wire.factura.IdPrimita, .NrFact = If(wire.factura.NrFact, String.Empty), .DataFact = ParseDay(wire.factura.DataFact),
                .Furnizor = If(wire.factura.Furnizor, String.Empty), .CuiNormalizat = If(wire.factura.CuiNormalizat, String.Empty), .Total = wire.factura.Total}
        End If
        If wire.partener IsNot Nothing Then
            k_result.PartenerId = wire.partener.IdPartener
            k_result.PartenerDenumire = If(wire.partener.Denumire, String.Empty)
        End If
        If wire.duplicate IsNot Nothing Then
            For Each d As VerconDuplicatWire In wire.duplicate
                k_result.Duplicate.Add(New EFacturaVerconDuplicat() With {
                    .Gestiune = If(d.Gestiune, String.Empty), .Numar = d.Numar, .NumarSub = d.NumarSub, .IdIntrare = d.IdIntrare, .Data = ParseDay(d.Data)})
            Next
        End If
        If wire.trimitere IsNot Nothing Then k_result.Trimitere = ToVerconTrimitere(wire.trimitere)
        If wire.istoric IsNot Nothing Then
            For Each h As VerconTrimitereWire In wire.istoric
                k_result.Istoric.Add(ToVerconTrimitere(h))
            Next
        End If
        If wire.linii IsNot Nothing Then
            For Each l As VerconLinieWire In wire.linii
                k_result.Linii.Add(New EFacturaVerconLinie() With {
                    .IdLinie = l.IdLinie, .NrLinie = If(l.NrLinie, String.Empty), .Denumire = If(l.Denumire, String.Empty), .Cpv = If(l.Cpv, String.Empty),
                    .Unit = If(l.Unit, String.Empty), .Cant = l.Cant, .Pret = l.Pret, .Valoare = l.Valoare, .ValoareCuTva = l.ValoareCuTVA,
                    .IdMasina = l.IdMasina, .Cont = If(l.Cont, String.Empty), .Gestiune = If(l.Gestiune, String.Empty)})
            Next
        End If
        Return k_result
    End Function

End Class
