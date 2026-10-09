Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 00EF-08 -- /api/efactura/furnizor, /um, /clienti, /facturi: PYTHON/routes/efactura/factura_routes.py (slice 00EF-06).
' Wire names are exactly the server's (the columns of the unit database, ASCII; `_json` keeps property names unchanged
' and is case sensitive). Every method is a boundary: log + rethrow (an ApiException keeps the server's Romanian text).
Partial Public Class ApiClient

    Private NotInheritable Class EFacturaFurnizorWire
        Public Property Denumire As String
        Public Property CodFiscal As String
        Public Property Adresa As String
        Public Property Orasul As String
        Public Property Judetul As String
        Public Property Mail As String
        Public Property Telefon As String
        Public Property SerieFactura As String
        Public Property NumarInitial As Integer
        Public Property AfiseazaPrimiteNoi As Integer
    End Class

    Private NotInheritable Class EFacturaFurnizorAnswerWire
        Public Property exista As Boolean
        Public Property furnizor As EFacturaFurnizorWire
        Public Property are_facturi As Boolean
    End Class

    Private NotInheritable Class EFacturaClientWire
        Public Property IdClient As Integer
        Public Property DenumireClient As String
        Public Property CodFiscal As String
        Public Property IndFiscal As String
        Public Property Cont As String
        Public Property Banca As String
        Public Property Adresa As String
        Public Property Judetul As String
        Public Property Orasul As String
        Public Property Sector As String
        Public Property CNP As Integer
    End Class

    Private NotInheritable Class EFacturaClientiAnswerWire
        Public Property clienti As List(Of EFacturaClientWire)
    End Class

    Private NotInheritable Class EFacturaClientAnafAnswerWire
        Public Property client As EFacturaClientWire
    End Class

    Private NotInheritable Class EFacturaUmWire
        Public Property Cod As String
        Public Property Explicatie As String
    End Class

    Private NotInheritable Class EFacturaUmAnswerWire
        Public Property um As List(Of EFacturaUmWire)
    End Class

    Private NotInheritable Class EFacturaLinieWire
        Public Property NrCrt As String
        Public Property Continut As String
        Public Property Um As String
        Public Property Cant As Decimal
        Public Property PU As Decimal
        Public Property Valoare As Decimal
        Public Property Platit As Integer
        Public Property Grup As Integer
    End Class

    ' Answer of the list and of the detail: the same names; the list has no client / linii.
    Private NotInheritable Class EFacturaFacturaWire
        Public Property IdFactura As Integer
        Public Property IdClient As Integer
        Public Property SerieFactura As String
        Public Property NumarFactura As Integer
        Public Property DataFactura As String
        Public Property TipFactura As String
        Public Property Comentarii As String
        Public Property BT_13 As String
        Public Property ContPlata As String
        Public Property AtasamentOriginal As Integer
        Public Property EroareAnaf As String
        Public Property IdFacturaA As Integer?
        Public Property SerieFacturaA As String
        Public Property NumarFacturaA As String
        Public Property Corectata As Integer
        Public Property ClientDenumire As String
        Public Property total As Decimal
        Public Property stare As String
        Public Property este_storno As Boolean
        Public Property poate_modifica As Boolean
        Public Property poate_sterge As Boolean
        Public Property poate_modifica_data As Boolean
        Public Property data_minima As String
        Public Property poate_storna As Boolean
        Public Property client As EFacturaClientWire
        Public Property linii As List(Of EFacturaLinieWire)
    End Class

    Private NotInheritable Class EFacturaFacturiAnswerWire
        Public Property facturi As List(Of EFacturaFacturaWire)
    End Class

    Private NotInheritable Class EFacturaNumarWire
        Public Property serie As String
        Public Property numar As Integer
        Public Property data_minima As String
    End Class

    ' What a save sends: the header the server reads and the lines. Series, number, type and ANAF's ids are the server's.
    Private NotInheritable Class EFacturaSaveLinieWire
        Public Property NrCrt As String
        Public Property Continut As String
        Public Property Um As String
        Public Property Cant As Decimal
        Public Property PU As Decimal
        Public Property Platit As Integer
        Public Property Grup As Integer
    End Class

    Private NotInheritable Class EFacturaSaveWire
        Public Property IdClient As Integer
        Public Property DataFactura As String
        Public Property Comentarii As String
        Public Property BT_13 As String
        Public Property ContPlata As String
        Public Property AtasamentOriginal As Integer
        Public Property linii As List(Of EFacturaSaveLinieWire)
    End Class

    ' ── Issuer ──────────────────────────────────────────────────────────────────

    Public Async Function GetFurnizorAsync(ct As CancellationToken) _
        As Task(Of EFacturaFurnizor) Implements IEFacturaApi.GetFurnizorAsync
        Try
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Get, "/api/efactura/furnizor", Nothing,
                                                             "citirea datelor unității emitente E-Factura", ct).ConfigureAwait(False)
            Dim wire As EFacturaFurnizorAnswerWire = JsonSerializer.Deserialize(Of EFacturaFurnizorAnswerWire)(respText, _json)
            If wire Is Nothing Then Throw New ApiException("Serverul nu a trimis datele unității emitente.")
            Return If(wire.exista AndAlso wire.furnizor IsNot Nothing, ToFurnizor(wire.furnizor, wire.are_facturi), Nothing)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetFurnizorAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveFurnizorAsync(k_furnizor As EFacturaFurnizor, ct As CancellationToken) _
        As Task(Of EFacturaFurnizor) Implements IEFacturaApi.SaveFurnizorAsync
        Try
            ArgumentNullException.ThrowIfNull(k_furnizor)
            Dim body As String = JsonSerializer.Serialize(New EFacturaFurnizorWire() With {
                .Denumire = k_furnizor.Denumire, .CodFiscal = k_furnizor.CodFiscal, .Adresa = k_furnizor.Adresa,
                .Orasul = k_furnizor.Orasul, .Judetul = k_furnizor.Judetul, .Mail = k_furnizor.Mail,
                .Telefon = k_furnizor.Telefon,
                .SerieFactura = k_furnizor.SerieFactura, .NumarInitial = k_furnizor.NumarInitial,
                .AfiseazaPrimiteNoi = If(k_furnizor.AfiseazaPrimiteNoi, 1, 0)}, _json)
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Put, "/api/efactura/furnizor", body,
                                                             "salvarea datelor unității emitente E-Factura", ct).ConfigureAwait(False)
            Dim wire As EFacturaFurnizorAnswerWire = JsonSerializer.Deserialize(Of EFacturaFurnizorAnswerWire)(respText, _json)
            If wire Is Nothing OrElse wire.furnizor Is Nothing Then Throw New ApiException("Serverul nu a trimis datele salvate.")
            Return ToFurnizor(wire.furnizor, wire.are_facturi)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveFurnizorAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function TakeFurnizorFromAnafAsync(ct As CancellationToken) _
        As Task(Of EFacturaFurnizor) Implements IEFacturaApi.TakeFurnizorFromAnafAsync
        Try
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Post, "/api/efactura/furnizor/anaf", "{}",
                                                             "preluarea datelor unității de la ANAF", ct).ConfigureAwait(False)
            Dim wire As EFacturaFurnizorAnswerWire = JsonSerializer.Deserialize(Of EFacturaFurnizorAnswerWire)(respText, _json)
            If wire Is Nothing OrElse wire.furnizor Is Nothing Then Throw New ApiException("Serverul nu a trimis datele unității.")
            Return ToFurnizor(wire.furnizor, wire.are_facturi)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.TakeFurnizorFromAnafAsync", ex)
            Throw
        End Try
    End Function

    ' ── Units of measure ────────────────────────────────────────────────────────

    Public Async Function GetUmAsync(k_query As String, ct As CancellationToken) _
        As Task(Of List(Of EFacturaUm)) Implements IEFacturaApi.GetUmAsync
        Try
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Get, "/api/efactura/um" & QueryOf(k_query), Nothing,
                                                             "citirea unităților de măsură E-Factura", ct).ConfigureAwait(False)
            Dim wire As EFacturaUmAnswerWire = JsonSerializer.Deserialize(Of EFacturaUmAnswerWire)(respText, _json)
            Dim k_result As New List(Of EFacturaUm)()
            If wire Is Nothing OrElse wire.um Is Nothing Then Return k_result
            For Each w As EFacturaUmWire In wire.um
                k_result.Add(New EFacturaUm() With {.Cod = If(w.Cod, String.Empty), .Explicatie = If(w.Explicatie, String.Empty)})
            Next
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetUmAsync", ex)
            Throw
        End Try
    End Function

    ' ── Customers ───────────────────────────────────────────────────────────────

    Public Async Function GetClientiAsync(k_query As String, ct As CancellationToken) _
        As Task(Of List(Of EFacturaClient)) Implements IEFacturaApi.GetClientiAsync
        Try
            ' The server's own cap is 2000 per call; the default (200) would cut a long customer list.
            Dim k_path As String = "/api/efactura/clienti?limit=2000" &
                                 If(String.IsNullOrWhiteSpace(k_query), String.Empty, "&q=" & Uri.EscapeDataString(k_query.Trim()))
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Get, k_path, Nothing,
                                                             "citirea clienților E-Factura", ct).ConfigureAwait(False)
            Dim wire As EFacturaClientiAnswerWire = JsonSerializer.Deserialize(Of EFacturaClientiAnswerWire)(respText, _json)
            Dim k_result As New List(Of EFacturaClient)()
            If wire Is Nothing OrElse wire.clienti Is Nothing Then Return k_result
            For Each w As EFacturaClientWire In wire.clienti
                k_result.Add(ToClient(w))
            Next
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetClientiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveClientAsync(k_client As EFacturaClient, ct As CancellationToken) _
        As Task(Of EFacturaClient) Implements IEFacturaApi.SaveClientAsync
        Try
            ArgumentNullException.ThrowIfNull(k_client)
            Dim body As String = JsonSerializer.Serialize(New EFacturaClientWire() With {
                .IdClient = k_client.IdClient, .DenumireClient = k_client.DenumireClient, .CodFiscal = k_client.CodFiscal,
                .IndFiscal = k_client.IndFiscal, .Cont = k_client.Cont, .Banca = k_client.Banca, .Adresa = k_client.Adresa,
                .Judetul = k_client.Judetul, .Orasul = k_client.Orasul, .Sector = k_client.Sector,
                .CNP = If(k_client.Cnp, 1, 0)}, _json)
            Dim isNew As Boolean = k_client.IdClient <= 0
            Dim k_path As String = If(isNew, "/api/efactura/clienti",
                                    "/api/efactura/clienti/" & k_client.IdClient.ToString(CultureInfo.InvariantCulture))
            Dim respText As String = Await SendEFacturaAsync(If(isNew, HttpMethod.Post, HttpMethod.Put), k_path, body,
                                                             "salvarea clientului E-Factura", ct).ConfigureAwait(False)
            Dim wire As EFacturaClientWire = JsonSerializer.Deserialize(Of EFacturaClientWire)(respText, _json)
            If wire Is Nothing Then Throw New ApiException("Serverul nu a trimis clientul salvat.")
            Return ToClient(wire)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveClientAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function TakeClientFromAnafAsync(k_codFiscal As String, ct As CancellationToken) _
        As Task(Of EFacturaClient) Implements IEFacturaApi.TakeClientFromAnafAsync
        Try
            Dim body As String = JsonSerializer.Serialize(New EFacturaClientWire() With {.CodFiscal = k_codFiscal}, _json)
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Post, "/api/efactura/clienti/anaf", body,
                                                             "preluarea datelor clientului de la ANAF", ct).ConfigureAwait(False)
            Dim wire As EFacturaClientAnafAnswerWire = JsonSerializer.Deserialize(Of EFacturaClientAnafAnswerWire)(respText, _json)
            If wire Is Nothing OrElse wire.client Is Nothing Then Throw New ApiException("Serverul nu a trimis datele clientului.")
            Return ToClient(wire.client)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.TakeClientFromAnafAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function DeleteClientAsync(k_idClient As Integer, ct As CancellationToken) _
        As Task Implements IEFacturaApi.DeleteClientAsync
        Try
            If k_idClient <= 0 Then Throw New ArgumentException("The customer id is required.", NameOf(k_idClient))
            Await SendEFacturaAsync(HttpMethod.Delete, "/api/efactura/clienti/" & k_idClient.ToString(CultureInfo.InvariantCulture),
                                    Nothing, "ștergerea clientului E-Factura", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.DeleteClientAsync", ex)
            Throw
        End Try
    End Function

    ' ── Invoices ────────────────────────────────────────────────────────────────

    Public Async Function GetFacturiAsync(k_year As Integer?, k_query As String, ct As CancellationToken) _
        As Task(Of List(Of EFacturaFactura)) Implements IEFacturaApi.GetFacturiAsync
        Try
            Dim k_path As String = "/api/efactura/facturi"
            Dim k_parts As New List(Of String)()
            If k_year.HasValue Then k_parts.Add("an=" & k_year.Value.ToString(CultureInfo.InvariantCulture))
            If Not String.IsNullOrWhiteSpace(k_query) Then k_parts.Add("q=" & Uri.EscapeDataString(k_query.Trim()))
            If k_parts.Count > 0 Then k_path &= "?" & String.Join("&", k_parts)
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Get, k_path, Nothing,
                                                             "citirea facturilor E-Factura", ct).ConfigureAwait(False)
            Dim wire As EFacturaFacturiAnswerWire = JsonSerializer.Deserialize(Of EFacturaFacturiAnswerWire)(respText, _json)
            Dim k_result As New List(Of EFacturaFactura)()
            If wire Is Nothing OrElse wire.facturi Is Nothing Then Return k_result
            For Each w As EFacturaFacturaWire In wire.facturi
                k_result.Add(ToFactura(w))
            Next
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetFacturiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetFacturaAsync(k_idFactura As Integer, ct As CancellationToken) _
        As Task(Of EFacturaFactura) Implements IEFacturaApi.GetFacturaAsync
        Try
            If k_idFactura <= 0 Then Throw New ArgumentException("The invoice id is required.", NameOf(k_idFactura))
            Dim respText As String = Await SendEFacturaAsync(
                HttpMethod.Get, "/api/efactura/facturi/" & k_idFactura.ToString(CultureInfo.InvariantCulture), Nothing,
                "citirea facturii E-Factura", ct).ConfigureAwait(False)
            Return ToFacturaDetail(respText)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetFacturaAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetNextNumberAsync(ct As CancellationToken) _
        As Task(Of EFacturaNumarUrmator) Implements IEFacturaApi.GetNextNumberAsync
        Try
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Get, "/api/efactura/facturi/numar-urmator", Nothing,
                                                             "citirea următorului număr de factură", ct).ConfigureAwait(False)
            Dim wire As EFacturaNumarWire = JsonSerializer.Deserialize(Of EFacturaNumarWire)(respText, _json)
            If wire Is Nothing Then Throw New ApiException("Serverul nu a trimis numărul următor.")
            Return New EFacturaNumarUrmator() With {.Serie = If(wire.serie, String.Empty), .Numar = wire.numar,
                                                    .DataMinima = ParseDay(wire.data_minima)}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetNextNumberAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveFacturaAsync(k_factura As EFacturaFactura, ct As CancellationToken) _
        As Task(Of EFacturaFactura) Implements IEFacturaApi.SaveFacturaAsync
        Try
            ArgumentNullException.ThrowIfNull(k_factura)
            Dim k_save As EFacturaSaveWire = ToSaveWire(k_factura)
            Dim isNew As Boolean = k_factura.IdFactura <= 0
            Dim k_path As String = If(isNew, "/api/efactura/facturi",
                                    "/api/efactura/facturi/" & k_factura.IdFactura.ToString(CultureInfo.InvariantCulture))
            Dim respText As String = Await SendEFacturaAsync(If(isNew, HttpMethod.Post, HttpMethod.Put), k_path,
                                                             JsonSerializer.Serialize(k_save, _json),
                                                             "salvarea facturii E-Factura", ct).ConfigureAwait(False)
            Return ToFacturaDetail(respText)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveFacturaAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function DeleteFacturaAsync(k_idFactura As Integer, ct As CancellationToken) _
        As Task Implements IEFacturaApi.DeleteFacturaAsync
        Try
            If k_idFactura <= 0 Then Throw New ArgumentException("The invoice id is required.", NameOf(k_idFactura))
            Await SendEFacturaAsync(HttpMethod.Delete, "/api/efactura/facturi/" & k_idFactura.ToString(CultureInfo.InvariantCulture),
                                    Nothing, "ștergerea facturii E-Factura", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.DeleteFacturaAsync", ex)
            Throw
        End Try
    End Function

    ' ── Mapping ─────────────────────────────────────────────────────────────────

    ' What a save (and a storno's replacement) sends: the header and the lines, never the number, the series or the type.
    Private Shared Function ToSaveWire(k_factura As EFacturaFactura) As EFacturaSaveWire
        Dim k_save As New EFacturaSaveWire() With {
            .IdClient = k_factura.IdClient,
            .DataFactura = k_factura.DataFactura.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            .Comentarii = k_factura.Comentarii, .BT_13 = k_factura.BT_13, .ContPlata = k_factura.ContPlata,
            .AtasamentOriginal = If(k_factura.AtasamentOriginal, 1, 0),
            .linii = New List(Of EFacturaSaveLinieWire)()}
        For Each k_line As EFacturaLinie In k_factura.Linii
            k_save.linii.Add(New EFacturaSaveLinieWire() With {
                .NrCrt = k_line.NrCrt, .Continut = k_line.Continut, .Um = k_line.Um, .Cant = k_line.Cant, .PU = k_line.PU,
                .Platit = If(k_line.Platit, 1, 0), .Grup = k_line.Grup})
        Next
        Return k_save
    End Function

    ' «yyyy-MM-dd» (the server's ISO date) -> a date; empty or unreadable = no date.
    Private Shared Function ParseDay(k_text As String) As Date?
        Dim k_day As Date
        If Not String.IsNullOrWhiteSpace(k_text) AndAlso
           Date.TryParse(k_text, CultureInfo.InvariantCulture, DateTimeStyles.None, k_day) Then Return k_day
        Return Nothing
    End Function

    Private Shared Function QueryOf(k_query As String) As String
        Return If(String.IsNullOrWhiteSpace(k_query), String.Empty, "?q=" & Uri.EscapeDataString(k_query.Trim()))
    End Function

    Private Shared Function ToFurnizor(k_wire As EFacturaFurnizorWire, k_areFacturi As Boolean) As EFacturaFurnizor
        Return New EFacturaFurnizor() With {
            .Denumire = If(k_wire.Denumire, String.Empty), .CodFiscal = If(k_wire.CodFiscal, String.Empty),
            .Adresa = If(k_wire.Adresa, String.Empty), .Orasul = If(k_wire.Orasul, String.Empty),
            .Judetul = If(k_wire.Judetul, String.Empty), .Mail = If(k_wire.Mail, String.Empty),
            .Telefon = If(k_wire.Telefon, String.Empty),
            .SerieFactura = If(k_wire.SerieFactura, String.Empty), .NumarInitial = Math.Max(1, k_wire.NumarInitial),
            .AfiseazaPrimiteNoi = k_wire.AfiseazaPrimiteNoi <> 0, .AreFacturi = k_areFacturi}
    End Function

    Private Shared Function ToClient(k_wire As EFacturaClientWire) As EFacturaClient
        If k_wire Is Nothing Then Return Nothing
        Return New EFacturaClient() With {
            .IdClient = k_wire.IdClient, .DenumireClient = If(k_wire.DenumireClient, String.Empty),
            .CodFiscal = If(k_wire.CodFiscal, String.Empty), .IndFiscal = If(k_wire.IndFiscal, String.Empty),
            .Cont = If(k_wire.Cont, String.Empty), .Banca = If(k_wire.Banca, String.Empty),
            .Adresa = If(k_wire.Adresa, String.Empty), .Judetul = If(k_wire.Judetul, String.Empty),
            .Orasul = If(k_wire.Orasul, String.Empty), .Sector = If(k_wire.Sector, String.Empty),
            .Cnp = k_wire.CNP <> 0}
    End Function

    Private Shared Function ToFactura(k_wire As EFacturaFacturaWire) As EFacturaFactura
        Dim moment As Date
        Date.TryParse(If(k_wire.DataFactura, String.Empty), CultureInfo.InvariantCulture, DateTimeStyles.None, moment)
        Dim k_result As New EFacturaFactura() With {
            .IdFactura = k_wire.IdFactura, .IdClient = k_wire.IdClient,
            .SerieFactura = If(k_wire.SerieFactura, String.Empty), .NumarFactura = k_wire.NumarFactura,
            .DataFactura = moment, .TipFactura = If(k_wire.TipFactura, "380"),
            .Comentarii = If(k_wire.Comentarii, String.Empty), .BT_13 = If(k_wire.BT_13, String.Empty),
            .ContPlata = If(k_wire.ContPlata, String.Empty), .AtasamentOriginal = k_wire.AtasamentOriginal <> 0,
            .EroareAnaf = If(k_wire.EroareAnaf, String.Empty), .IdFacturaA = k_wire.IdFacturaA,
            .SerieFacturaA = If(k_wire.SerieFacturaA, String.Empty), .NumarFacturaA = If(k_wire.NumarFacturaA, String.Empty),
            .Corectata = k_wire.Corectata <> 0, .ClientDenumire = If(k_wire.ClientDenumire, String.Empty),
            .Total = k_wire.total, .Stare = If(k_wire.stare, EFacturaStare.Ciorna), .EsteStorno = k_wire.este_storno,
            .PoateModifica = k_wire.poate_modifica, .PoateSterge = k_wire.poate_sterge,
            .PoateModificaData = k_wire.poate_modifica_data, .DataMinima = ParseDay(k_wire.data_minima),
            .PoateStorna = k_wire.poate_storna,
            .Client = ToClient(k_wire.client)}
        If k_wire.client IsNot Nothing Then k_result.ClientDenumire = k_result.Client.DenumireClient
        If k_wire.linii IsNot Nothing Then
            For Each w As EFacturaLinieWire In k_wire.linii
                k_result.Linii.Add(New EFacturaLinie() With {
                    .NrCrt = If(w.NrCrt, String.Empty), .Continut = If(w.Continut, String.Empty), .Um = If(w.Um, String.Empty),
                    .Cant = w.Cant, .PU = w.PU, .Valoare = w.Valoare, .Platit = w.Platit <> 0, .Grup = w.Grup})
            Next
        End If
        Return k_result
    End Function

    Private Shared Function ToFacturaDetail(k_json As String) As EFacturaFactura
        Dim wire As EFacturaFacturaWire = JsonSerializer.Deserialize(Of EFacturaFacturaWire)(k_json, _json)
        If wire Is Nothing Then Throw New ApiException("Serverul nu a trimis factura.")
        Return ToFactura(wire)
    End Function

End Class
