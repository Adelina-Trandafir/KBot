Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 00EF-18 -- /api/efactura/primite...: PYTHON/routes/efactura/primite_routes.py (slice 00EF-17). Wire names are exactly the
' server's. Every method is a boundary: log + rethrow (an ApiException keeps the server's Romanian text and reason).
Partial Public Class ApiClient

    Private NotInheritable Class EFacturaPrimitaWire
        Public Property IdPrimita As Integer
        Public Property IdSol As String
        Public Property NrFact As String
        Public Property DataFact As String
        Public Property DataScad As String
        Public Property CotaTVA As Decimal
        Public Property TVA As Decimal
        Public Property Valoare As Decimal
        Public Property Total As Decimal
        Public Property CUI As String
        Public Property CuiNormalizat As String
        Public Property DenumireP As String
        Public Property Adresa As String
        Public Property Atasament As String
        Public Property Tip As String
        Public Property Semn As Integer
        Public Property Ref As String
        Public Property IdPrimitaRef As Integer?
        Public Property Nou As Integer
        Public Property legatura As String
    End Class

    Private NotInheritable Class EFacturaPrimiteListWire
        Public Property facturi As List(Of EFacturaPrimitaWire)
    End Class

    Private NotInheritable Class EFacturaPrimitaLinieWire
        Public Property NrLinie As String
        Public Property Denumire As String
        Public Property Explicatie As String
        Public Property Unit As String
        Public Property Cant As Decimal
        Public Property Pret As Decimal
        Public Property Valoare As Decimal
    End Class

    Private NotInheritable Class EFacturaPrimitaCotaWire
        Public Property Categorie As String
        Public Property CotaTVA As Decimal
        Public Property Baza As Decimal
        Public Property TVA As Decimal
    End Class

    Private NotInheritable Class EFacturaPrimitaMesajWire
        Public Property IdMsg As Integer
        Public Property IdMesajAnaf As String
        Public Property Mesaj As String
        Public Property DataMesaj As String
    End Class

    Private NotInheritable Class EFacturaPrimitaAtasamentWire
        Public Property nume As String
        Public Property mime As String
        Public Property octeti As Long
    End Class

    Private NotInheritable Class EFacturaPrimitaDdfWire
        Public Property IDDF As Integer
        Public Property CodAngajament As String
    End Class

    Private NotInheritable Class EFacturaPrimitaLegaturiWire
        Public Property auto As List(Of EFacturaPrimitaDdfWire)
        Public Property manual As List(Of EFacturaPrimitaDdfWire)
    End Class

    Private NotInheritable Class EFacturaPrimitaDetaliuWire
        Public Property factura As EFacturaPrimitaWire
        Public Property linii As List(Of EFacturaPrimitaLinieWire)
        Public Property cote As List(Of EFacturaPrimitaCotaWire)
        Public Property note As List(Of String)
        Public Property mesaje As List(Of EFacturaPrimitaMesajWire)
        Public Property atasamente As List(Of EFacturaPrimitaAtasamentWire)
        Public Property legaturi As EFacturaPrimitaLegaturiWire
    End Class

    Private NotInheritable Class EFacturaDdfAlegereWire
        Public Property IDDF As Integer
        Public Property CodAngajament As String
        Public Property ObiectDDF As String
        Public Property NumePartener As String
        Public Property CodFiscal As String
    End Class

    Private NotInheritable Class EFacturaDdfAlegereListWire
        Public Property ddf As List(Of EFacturaDdfAlegereWire)
    End Class

    Private NotInheritable Class EFacturaSincronizareEroareWire
        Public Property id_solicitare As String
        Public Property motiv As String
    End Class

    Private NotInheritable Class EFacturaSincronizareWire
        Public Property cui As String
        Public Property zile As Integer
        Public Property gasite As Integer
        Public Property adaugate As Integer
        Public Property sarite As Integer
        Public Property ramase As Integer
        Public Property erori As List(Of EFacturaSincronizareEroareWire)
    End Class

    Public Async Function SyncPrimiteAsync(k_zile As Integer, k_limita As Integer, ct As CancellationToken) _
        As Task(Of EFacturaSincronizare) Implements IEFacturaApi.SyncPrimiteAsync
        Try
            Dim k_body As String = JsonSerializer.Serialize(
                New Dictionary(Of String, Integer) From {{"zile", k_zile}, {"limita", k_limita}}, _json)
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Post, "/api/efactura/primite/sincronizeaza", k_body,
                                                             "sincronizarea facturilor primite", ct).ConfigureAwait(False)
            Dim wire As EFacturaSincronizareWire = JsonSerializer.Deserialize(Of EFacturaSincronizareWire)(respText, _json)
            If wire Is Nothing Then Throw New ApiException("Serverul nu a trimis rezultatul sincronizării.")
            Dim k_result As New EFacturaSincronizare() With {
                .Cui = If(wire.cui, String.Empty), .Zile = wire.zile, .Gasite = wire.gasite, .Adaugate = wire.adaugate,
                .Sarite = wire.sarite, .Ramase = wire.ramase}
            If wire.erori IsNot Nothing Then
                For Each w As EFacturaSincronizareEroareWire In wire.erori
                    k_result.Erori.Add($"{w.id_solicitare}: {w.motiv}")
                Next
            End If
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SyncPrimiteAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetPrimiteAsync(k_year As Integer?, k_month As Integer?, k_query As String, k_idDdf As Integer?,
                                          ct As CancellationToken) As Task(Of List(Of EFacturaPrimita)) _
        Implements IEFacturaApi.GetPrimiteAsync
        Try
            Dim k_parts As New List(Of String)()
            If k_year.HasValue Then k_parts.Add("an=" & k_year.Value.ToString(CultureInfo.InvariantCulture))
            If k_month.HasValue Then k_parts.Add("luna=" & k_month.Value.ToString(CultureInfo.InvariantCulture))
            If k_idDdf.HasValue Then k_parts.Add("iddf=" & k_idDdf.Value.ToString(CultureInfo.InvariantCulture))
            If Not String.IsNullOrWhiteSpace(k_query) Then k_parts.Add("q=" & Uri.EscapeDataString(k_query.Trim()))
            Dim k_path As String = "/api/efactura/primite" & If(k_parts.Count > 0, "?" & String.Join("&", k_parts), String.Empty)
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Get, k_path, Nothing,
                                                             "citirea facturilor primite", ct).ConfigureAwait(False)
            Dim wire As EFacturaPrimiteListWire = JsonSerializer.Deserialize(Of EFacturaPrimiteListWire)(respText, _json)
            Dim k_result As New List(Of EFacturaPrimita)()
            If wire Is Nothing OrElse wire.facturi Is Nothing Then Return k_result
            For Each w As EFacturaPrimitaWire In wire.facturi
                k_result.Add(ToPrimita(w))
            Next
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetPrimiteAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetPrimitaAsync(k_idPrimita As Integer, ct As CancellationToken) _
        As Task(Of EFacturaPrimitaDetaliu) Implements IEFacturaApi.GetPrimitaAsync
        Try
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Get, PrimitaPath(k_idPrimita, Nothing), Nothing,
                                                             "citirea facturii primite", ct).ConfigureAwait(False)
            Dim wire As EFacturaPrimitaDetaliuWire = JsonSerializer.Deserialize(Of EFacturaPrimitaDetaliuWire)(respText, _json)
            If wire Is Nothing OrElse wire.factura Is Nothing Then Throw New ApiException("Serverul nu a trimis factura primită.")
            Dim k_result As New EFacturaPrimitaDetaliu() With {.Factura = ToPrimita(wire.factura)}
            If wire.linii IsNot Nothing Then
                For Each w As EFacturaPrimitaLinieWire In wire.linii
                    k_result.Linii.Add(New EFacturaPrimitaLinie() With {
                        .NrLinie = If(w.NrLinie, String.Empty), .Denumire = If(w.Denumire, String.Empty),
                        .Explicatie = If(w.Explicatie, String.Empty), .Unit = If(w.Unit, String.Empty),
                        .Cant = w.Cant, .Pret = w.Pret, .Valoare = w.Valoare})
                Next
            End If
            If wire.cote IsNot Nothing Then
                For Each w As EFacturaPrimitaCotaWire In wire.cote
                    k_result.Cote.Add(New EFacturaPrimitaCota() With {
                        .Categorie = If(w.Categorie, String.Empty), .CotaTva = w.CotaTVA, .Baza = w.Baza, .Tva = w.TVA})
                Next
            End If
            If wire.note IsNot Nothing Then k_result.Note.AddRange(wire.note)
            If wire.mesaje IsNot Nothing Then
                For Each w As EFacturaPrimitaMesajWire In wire.mesaje
                    k_result.Mesaje.Add(New EFacturaPrimitaMesaj() With {
                        .IdMsg = w.IdMsg, .IdMesajAnaf = If(w.IdMesajAnaf, String.Empty), .Mesaj = If(w.Mesaj, String.Empty),
                        .DataMesaj = ParseDay(w.DataMesaj)})
                Next
            End If
            If wire.atasamente IsNot Nothing Then
                For k_i As Integer = 0 To wire.atasamente.Count - 1
                    Dim w As EFacturaPrimitaAtasamentWire = wire.atasamente(k_i)
                    k_result.Atasamente.Add(New EFacturaPrimitaAtasament() With {
                        .Index = k_i, .Nume = If(w.nume, String.Empty), .Mime = If(w.mime, String.Empty), .Octeti = w.octeti})
                Next
            End If
            If wire.legaturi IsNot Nothing Then
                AddLinks(k_result.LegaturiAuto, wire.legaturi.auto)
                AddLinks(k_result.LegaturiManuale, wire.legaturi.manual)
            End If
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetPrimitaAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function MarkPrimitaCititaAsync(k_idPrimita As Integer, ct As CancellationToken) _
        As Task Implements IEFacturaApi.MarkPrimitaCititaAsync
        Try
            Await SendEFacturaAsync(HttpMethod.Post, PrimitaPath(k_idPrimita, "citita"), "{}",
                                    "marcarea facturii primite ca citită", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.MarkPrimitaCititaAsync", ex)
            Throw
        End Try
    End Function

    Public Function GetPrimitaXmlAsync(k_idPrimita As Integer, ct As CancellationToken) _
        As Task(Of Byte()) Implements IEFacturaApi.GetPrimitaXmlAsync
        Return GetPrimitaFileAsync(k_idPrimita, "xml", "citirea XML-ului facturii primite", ct)
    End Function

    Public Function GetPrimitaZipAsync(k_idPrimita As Integer, ct As CancellationToken) _
        As Task(Of Byte()) Implements IEFacturaApi.GetPrimitaZipAsync
        Return GetPrimitaFileAsync(k_idPrimita, "zip", "descărcarea arhivei facturii primite de la ANAF", ct)
    End Function

    Public Function GetPrimitaPdfAsync(k_idPrimita As Integer, ct As CancellationToken) _
        As Task(Of Byte()) Implements IEFacturaApi.GetPrimitaPdfAsync
        Return GetPrimitaFileAsync(k_idPrimita, "pdf", "citirea facturii primite desenate de ANAF", ct)
    End Function

    Public Function GetPrimitaAtasamentAsync(k_idPrimita As Integer, k_index As Integer, ct As CancellationToken) _
        As Task(Of Byte()) Implements IEFacturaApi.GetPrimitaAtasamentAsync
        Return GetPrimitaFileAsync(k_idPrimita, "atasamente/" & k_index.ToString(CultureInfo.InvariantCulture),
                                   "citirea atașamentului facturii primite", ct)
    End Function

    Public Async Function LinkPrimitaAsync(k_idPrimita As Integer, k_idDdf As Integer, ct As CancellationToken) _
        As Task Implements IEFacturaApi.LinkPrimitaAsync
        Try
            If k_idDdf <= 0 Then Throw New ArgumentException("The DDF id is required.", NameOf(k_idDdf))
            Await SendEFacturaAsync(HttpMethod.Post, PrimitaPath(k_idPrimita, "asociere"),
                                    JsonSerializer.Serialize(New Dictionary(Of String, Integer) From {{"iddf", k_idDdf}}, _json),
                                    "legarea facturii primite de un DDF", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.LinkPrimitaAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function UnlinkPrimitaAsync(k_idPrimita As Integer, k_idDdf As Integer, ct As CancellationToken) _
        As Task Implements IEFacturaApi.UnlinkPrimitaAsync
        Try
            If k_idDdf <= 0 Then Throw New ArgumentException("The DDF id is required.", NameOf(k_idDdf))
            Await SendEFacturaAsync(HttpMethod.Delete,
                                    PrimitaPath(k_idPrimita, "asociere/" & k_idDdf.ToString(CultureInfo.InvariantCulture)), Nothing,
                                    "scoaterea legăturii facturii primite cu un DDF", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.UnlinkPrimitaAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetDdfAlegereAsync(k_query As String, ct As CancellationToken) _
        As Task(Of List(Of EFacturaDdfAlegere)) Implements IEFacturaApi.GetDdfAlegereAsync
        Try
            Dim k_path As String = "/api/efactura/primite-ddf" & If(String.IsNullOrWhiteSpace(k_query), String.Empty, "?q=" & Uri.EscapeDataString(k_query.Trim()))
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Get, k_path, Nothing, "citirea fundamentărilor", ct).ConfigureAwait(False)
            Dim wire As EFacturaDdfAlegereListWire = JsonSerializer.Deserialize(Of EFacturaDdfAlegereListWire)(respText, _json)
            Dim k_result As New List(Of EFacturaDdfAlegere)()
            If wire Is Nothing OrElse wire.ddf Is Nothing Then Return k_result
            For Each w As EFacturaDdfAlegereWire In wire.ddf
                k_result.Add(New EFacturaDdfAlegere() With {
                    .IdDdf = w.IDDF, .CodAngajament = If(w.CodAngajament, String.Empty), .Obiect = If(w.ObiectDDF, String.Empty),
                    .Partener = If(w.NumePartener, String.Empty), .CodFiscal = If(w.CodFiscal, String.Empty)})
            Next
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetDdfAlegereAsync", ex)
            Throw
        End Try
    End Function

    Private Async Function GetPrimitaFileAsync(k_idPrimita As Integer, k_tail As String, k_what As String, ct As CancellationToken) As Task(Of Byte())
        Try
            Dim k_bytes As Byte() = Await SendEFacturaBytesAsync(PrimitaPath(k_idPrimita, k_tail), k_what, ct).ConfigureAwait(False)
            If k_bytes Is Nothing OrElse k_bytes.Length = 0 Then Throw New ApiException("Serverul nu a trimis fișierul cerut.")
            Return k_bytes
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetPrimitaFileAsync", ex)
            Throw
        End Try
    End Function

    Private Shared Function PrimitaPath(k_idPrimita As Integer, k_tail As String) As String
        If k_idPrimita <= 0 Then Throw New ArgumentException("The invoice id is required.", NameOf(k_idPrimita))
        Dim k_path As String = "/api/efactura/primite/" & k_idPrimita.ToString(CultureInfo.InvariantCulture)
        Return If(String.IsNullOrEmpty(k_tail), k_path, k_path & "/" & k_tail)
    End Function

    Private Shared Sub AddLinks(k_target As List(Of EFacturaPrimitaDdf), k_wire As List(Of EFacturaPrimitaDdfWire))
        If k_wire Is Nothing Then Return
        For Each w As EFacturaPrimitaDdfWire In k_wire
            k_target.Add(New EFacturaPrimitaDdf() With {.IdDdf = w.IDDF, .CodAngajament = If(w.CodAngajament, String.Empty)})
        Next
    End Sub

    Private Shared Function ToPrimita(w As EFacturaPrimitaWire) As EFacturaPrimita
        Return New EFacturaPrimita() With {
            .IdPrimita = w.IdPrimita, .IdSol = If(w.IdSol, String.Empty), .NrFact = If(w.NrFact, String.Empty),
            .DataFact = ParseDay(w.DataFact), .DataScad = ParseDay(w.DataScad), .CotaTva = w.CotaTVA, .Tva = w.TVA,
            .Valoare = w.Valoare, .Total = w.Total, .Cui = If(w.CUI, String.Empty),
            .CuiNormalizat = If(w.CuiNormalizat, String.Empty), .Furnizor = If(w.DenumireP, String.Empty),
            .Adresa = If(w.Adresa, String.Empty), .Atasament = If(w.Atasament, String.Empty), .Tip = If(w.Tip, "FC"),
            .Semn = If(w.Semn = 0, 1, w.Semn), .Ref = If(w.Ref, String.Empty), .IdPrimitaRef = w.IdPrimitaRef,
            .Nou = w.Nou <> 0, .Legatura = If(w.legatura, String.Empty)}
    End Function

End Class
