Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 00EF-09 -- /api/efactura/facturi/{id}/trimite, /verifica, /storno, /pdf-anaf: PYTHON/routes/efactura/trimitere_routes.py
' (slice 00EF-07) and factura_routes.py. Wire names are exactly the server's. Every method is a boundary: log + rethrow
' (an ApiException keeps the server's Romanian text, with the findings of a refused send listed under it).
Partial Public Class ApiClient

    Private NotInheritable Class EFacturaConstatareWire
        Public Property nivel As String
        Public Property cod As String
        Public Property mesaj As String
    End Class

    Private NotInheritable Class EFacturaTrimitereWire
        Public Property factura As EFacturaFacturaWire
        Public Property id_incarcare As String
        Public Property constatari As List(Of EFacturaConstatareWire)
    End Class

    Private NotInheritable Class EFacturaVerificareWire
        Public Property rezultat As String
        Public Property stare_anaf As String
        Public Property mesaj As String
        Public Property factura As EFacturaFacturaWire
    End Class

    Private NotInheritable Class EFacturaStornareWire
        Public Property storno As EFacturaFacturaWire
        Public Property factura As EFacturaFacturaWire
    End Class

    Public Function SendFacturaAsync(k_idFactura As Integer, k_attachmentPdf As Byte(), ct As CancellationToken) _
        As Task(Of EFacturaTrimitere) Implements IEFacturaApi.SendFacturaAsync
        Dim k_body As Dictionary(Of String, Object) = Nothing
        If k_attachmentPdf IsNot Nothing Then
            k_body = New Dictionary(Of String, Object) From {{"atasament_pdf", Convert.ToBase64String(k_attachmentPdf)}}
        End If
        Return SendFacturaCoreAsync(k_idFactura, k_body, ct)
    End Function

    Public Function SendCorectieAsync(k_idFactura As Integer, k_comentarii As String, k_bt13 As String, k_attachmentPdf As Byte(),
                                      ct As CancellationToken) As Task(Of EFacturaTrimitere) Implements IEFacturaApi.SendCorectieAsync
        Dim k_body As New Dictionary(Of String, Object) From {
            {"corectie", New Dictionary(Of String, String) From {
                {"Comentarii", If(k_comentarii, String.Empty)}, {"BT_13", If(k_bt13, String.Empty)}}}}
        If k_attachmentPdf IsNot Nothing Then k_body("atasament_pdf") = Convert.ToBase64String(k_attachmentPdf)
        Return SendFacturaCoreAsync(k_idFactura, k_body, ct)
    End Function

    ' k_body = Nothing sends a draft; a body with «corectie» corrects an accepted invoice (type 384).
    Private Async Function SendFacturaCoreAsync(k_idFactura As Integer, k_data As Dictionary(Of String, Object), ct As CancellationToken) As Task(Of EFacturaTrimitere)
        Try
            If k_idFactura <= 0 Then Throw New ArgumentException("The invoice id is required.", NameOf(k_idFactura))
            Dim k_body As String = If(k_data Is Nothing, Nothing, JsonSerializer.Serialize(k_data, _json))
            Dim respText As String = Await SendEFacturaAsync(
                HttpMethod.Post, "/api/efactura/facturi/" & k_idFactura.ToString(CultureInfo.InvariantCulture) & "/trimite", k_body,
                If(k_data IsNot Nothing AndAlso k_data.ContainsKey("corectie"), "trimiterea corecției facturii la ANAF", "trimiterea facturii la ANAF"),
                ct).ConfigureAwait(False)
            Dim wire As EFacturaTrimitereWire = JsonSerializer.Deserialize(Of EFacturaTrimitereWire)(respText, _json)
            If wire Is Nothing OrElse wire.factura Is Nothing Then Throw New ApiException("Serverul nu a trimis factura după trimitere.")
            Dim k_result As New EFacturaTrimitere() With {
                .Factura = ToFactura(wire.factura), .IdIncarcare = If(wire.id_incarcare, String.Empty)}
            If wire.constatari IsNot Nothing Then
                For Each w As EFacturaConstatareWire In wire.constatari
                    If Not String.IsNullOrWhiteSpace(w.mesaj) Then k_result.Avertismente.Add(w.mesaj)
                Next
            End If
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SendFacturaCoreAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function VerifyFacturaAsync(k_idFactura As Integer, ct As CancellationToken) _
        As Task(Of EFacturaVerificare) Implements IEFacturaApi.VerifyFacturaAsync
        Try
            If k_idFactura <= 0 Then Throw New ArgumentException("The invoice id is required.", NameOf(k_idFactura))
            Dim respText As String = Await SendEFacturaAsync(
                HttpMethod.Post, "/api/efactura/facturi/" & k_idFactura.ToString(CultureInfo.InvariantCulture) & "/verifica", Nothing,
                "citirea stării facturii la ANAF", ct).ConfigureAwait(False)
            Dim wire As EFacturaVerificareWire = JsonSerializer.Deserialize(Of EFacturaVerificareWire)(respText, _json)
            If wire Is Nothing OrElse wire.factura Is Nothing Then Throw New ApiException("Serverul nu a trimis starea facturii.")
            Return New EFacturaVerificare() With {
                .Rezultat = If(wire.rezultat, EFacturaRezultat.Necunoscut), .StareAnaf = If(wire.stare_anaf, String.Empty),
                .Mesaj = If(wire.mesaj, String.Empty), .Factura = ToFactura(wire.factura)}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.VerifyFacturaAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function StornoFacturaAsync(k_idFactura As Integer, k_replacement As EFacturaFactura, ct As CancellationToken) _
        As Task(Of EFacturaStornare) Implements IEFacturaApi.StornoFacturaAsync
        Try
            If k_idFactura <= 0 Then Throw New ArgumentException("The invoice id is required.", NameOf(k_idFactura))
            ArgumentNullException.ThrowIfNull(k_replacement)
            Dim respText As String = Await SendEFacturaAsync(
                HttpMethod.Post, "/api/efactura/facturi/" & k_idFactura.ToString(CultureInfo.InvariantCulture) & "/storno",
                JsonSerializer.Serialize(ToSaveWire(k_replacement), _json), "stornarea facturii E-Factura", ct).ConfigureAwait(False)
            Dim wire As EFacturaStornareWire = JsonSerializer.Deserialize(Of EFacturaStornareWire)(respText, _json)
            If wire Is Nothing OrElse wire.storno Is Nothing OrElse wire.factura Is Nothing Then
                Throw New ApiException("Serverul nu a trimis facturile create la stornare.")
            End If
            Return New EFacturaStornare() With {.Storno = ToFactura(wire.storno), .Factura = ToFactura(wire.factura)}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.StornoFacturaAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetAnafPdfAsync(k_idFactura As Integer, ct As CancellationToken) _
        As Task(Of Byte()) Implements IEFacturaApi.GetAnafPdfAsync
        Try
            If k_idFactura <= 0 Then Throw New ArgumentException("The invoice id is required.", NameOf(k_idFactura))
            Dim k_bytes As Byte() = Await SendEFacturaBytesAsync(
                "/api/efactura/facturi/" & k_idFactura.ToString(CultureInfo.InvariantCulture) & "/pdf-anaf",
                "citirea facturii desenate de ANAF", ct).ConfigureAwait(False)
            If k_bytes Is Nothing OrElse k_bytes.Length = 0 Then Throw New ApiException("Serverul nu a trimis PDF-ul facturii.")
            Return k_bytes
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetAnafPdfAsync", ex)
            Throw
        End Try
    End Function

End Class
