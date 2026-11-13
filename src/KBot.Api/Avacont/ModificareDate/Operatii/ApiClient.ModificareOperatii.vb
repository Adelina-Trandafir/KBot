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

' Slice AV02-05 - «Modificare operatii». Wire field names exactly as routes/avacont/modificare_date/operatii/lista.py and stergere.py (all lowercase).
Partial Public Class ApiClient
    Implements IModificareOperatiiApi

    Private NotInheritable Class MoOperatieWire
        Public Property idoper As Integer
        Public Property iddocument As Integer
        Public Property numarnc As Integer
        Public Property numardocument As String
        Public Property datadocument As String
        Public Property dataoperatie As String
        Public Property clsf As String
        Public Property feldocument As String
        Public Property fel As String
        Public Property jurnal As String
        Public Property explicatie As String
        Public Property debit As String
        Public Property credit As String
        Public Property suma As Decimal
        Public Property chelt As Boolean
        Public Property cfpp As Boolean
        Public Property ord As String
        Public Property receptie As String
        Public Property refuz As String
    End Class

    Private NotInheritable Class MoListaResponse
        Public Property operatii As List(Of MoOperatieWire)
        Public Property inchidere As String
    End Class

    Private NotInheritable Class MoFacturaWire
        Public Property numar As String
        Public Property data As String
        Public Property platit As Decimal
    End Class

    Private NotInheritable Class MoOpWire
        Public Property numar As Integer
        Public Property data As String
    End Class

    Private NotInheritable Class MoPartenerWire
        Public Property cod As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class MoSoldWire
        Public Property debit As Decimal
        Public Property credit As Decimal
    End Class

    Private NotInheritable Class MoInfoResponse
        Public Property fel As String
        Public Property factura As String
        Public Property op As MoOpWire
        Public Property facturi As List(Of MoFacturaWire)
        Public Property partener As MoPartenerWire
        Public Property datadocument As String
        Public Property sold_la_data As MoSoldWire
        Public Property sold_anual As MoSoldWire
    End Class

    Private NotInheritable Class MoVerificaResponse
        Public Property refuz As String
        Public Property intrebare As String
        Public Property facturi As List(Of MoFacturaWire)
    End Class

    Private NotInheritable Class MoStergeResponse
        Public Property operatii As Integer
    End Class

    Private Const MoRoot As String = "/modificare-operatii"

    Private Shared Function MoOrd(k_text As String) As OperatieStareOrd
        Select Case If(k_text, String.Empty).Trim().ToUpperInvariant()
            Case "G" : Return OperatieStareOrd.Generat
            Case "AB" : Return OperatieStareOrd.SemnatAB
            Case "CD" : Return OperatieStareOrd.SemnatCD
            Case "O" : Return OperatieStareOrd.Ordonator
            Case Else : Return OperatieStareOrd.Niciuna
        End Select
    End Function

    Private Shared Function MoReceptie(k_text As String) As OperatieStareReceptie
        Select Case If(k_text, String.Empty).Trim().ToUpperInvariant()
            Case "T" : Return OperatieStareReceptie.Toate
            Case "P" : Return OperatieStareReceptie.Partiala
            Case Else : Return OperatieStareReceptie.Niciuna
        End Select
    End Function

    Private Shared Function MoFacturi(k_list As List(Of MoFacturaWire)) As IReadOnlyList(Of OperatieFacturaPlatita)
        Return If(k_list, New List(Of MoFacturaWire)()).Select(
            Function(k_f) New OperatieFacturaPlatita() With {.Numar = If(k_f.numar, String.Empty), .Data = OpDate(k_f.data), .Platit = k_f.platit}).ToList()
    End Function

    Private Shared Function MoSold(k_wire As MoSoldWire) As OperatieSold
        If k_wire Is Nothing Then Return Nothing
        Return New OperatieSold() With {.Debit = k_wire.debit, .Credit = k_wire.credit}
    End Function

    Private Shared Function MoVerificare(k_payload As MoVerificaResponse) As OperatieStergereVerificare
        If k_payload Is Nothing Then Return New OperatieStergereVerificare()
        Return New OperatieStergereVerificare() With {
            .Refuz = If(k_payload.refuz, String.Empty), .Intrebare = If(k_payload.intrebare, String.Empty), .Facturi = MoFacturi(k_payload.facturi)}
    End Function

    Private Shared Function MoId(k_id As Integer) As String
        Return k_id.ToString(CultureInfo.InvariantCulture)
    End Function

    Public Async Function GetOperatiiContabileAsync(k_idSubunitate As Integer, k_an As Integer, ct As CancellationToken) As Task(Of OperatiiContabileLista) _
        Implements IModificareOperatiiApi.GetOperatiiContabileAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"{AvacontRoot}{MoRoot}/lista?{OpQuery(k_idSubunitate, k_an)}", Nothing,
                "citirea operațiilor contabile", ct).ConfigureAwait(False)
            Dim k_payload As MoListaResponse = JsonSerializer.Deserialize(Of MoListaResponse)(k_resp, _json)
            Dim k_result As New OperatiiContabileLista()
            If k_payload Is Nothing Then Return k_result
            k_result.Inchidere = OpDate(k_payload.inchidere)
            k_result.Operatii = If(k_payload.operatii, New List(Of MoOperatieWire)()).Select(
                Function(k_o) New OperatieContabila() With {
                    .IdOper = k_o.idoper, .IdDocument = k_o.iddocument, .NumarNC = k_o.numarnc,
                    .NumarDocument = If(k_o.numardocument, String.Empty), .DataDocument = OpDate(k_o.datadocument), .DataOperatie = OpDate(k_o.dataoperatie),
                    .Clsf = If(k_o.clsf, String.Empty), .FelDocument = If(k_o.feldocument, String.Empty), .Fel = If(k_o.fel, String.Empty),
                    .Jurnal = If(k_o.jurnal, String.Empty), .Explicatie = If(k_o.explicatie, String.Empty),
                    .Debit = If(k_o.debit, String.Empty), .Credit = If(k_o.credit, String.Empty), .Suma = k_o.suma,
                    .Chelt = k_o.chelt, .Cfpp = k_o.cfpp, .Ord = MoOrd(k_o.ord), .Receptie = MoReceptie(k_o.receptie),
                    .Refuz = If(k_o.refuz, String.Empty)}).ToList()
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetOperatiiContabileAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetOperatieInfoAsync(k_idOper As Integer, ct As CancellationToken) As Task(Of OperatieInfo) _
        Implements IModificareOperatiiApi.GetOperatieInfoAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"{AvacontRoot}{MoRoot}/info/{MoId(k_idOper)}", Nothing,
                "citirea informațiilor operației", ct).ConfigureAwait(False)
            Dim k_payload As MoInfoResponse = JsonSerializer.Deserialize(Of MoInfoResponse)(k_resp, _json)
            Dim k_info As New OperatieInfo()
            If k_payload Is Nothing Then Return k_info
            k_info.Fel = If(k_payload.fel, String.Empty)
            k_info.Factura = If(k_payload.factura, String.Empty)
            If k_payload.op IsNot Nothing Then
                k_info.NumarOP = k_payload.op.numar
                k_info.DataOP = OpDate(k_payload.op.data)
            End If
            k_info.Facturi = MoFacturi(k_payload.facturi)
            If k_payload.partener IsNot Nothing Then
                k_info.CodPartener = If(k_payload.partener.cod, String.Empty)
                k_info.DenumirePartener = If(k_payload.partener.denumire, String.Empty)
            End If
            k_info.DataDocument = OpDate(k_payload.datadocument)
            k_info.SoldLaData = MoSold(k_payload.sold_la_data)
            k_info.SoldAnual = MoSold(k_payload.sold_anual)
            Return k_info
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetOperatieInfoAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function VerificaStergereOperatieAsync(k_idOper As Integer, ct As CancellationToken) As Task(Of OperatieStergereVerificare) _
        Implements IModificareOperatiiApi.VerificaStergereOperatieAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"{AvacontRoot}{MoRoot}/operatie/{MoId(k_idOper)}/verifica", Nothing,
                "verificarea ștergerii operației", ct).ConfigureAwait(False)
            Return MoVerificare(JsonSerializer.Deserialize(Of MoVerificaResponse)(k_resp, _json))
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.VerificaStergereOperatieAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function VerificaStergereDocumentAsync(k_idDocument As Integer, ct As CancellationToken) As Task(Of OperatieStergereVerificare) _
        Implements IModificareOperatiiApi.VerificaStergereDocumentAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"{AvacontRoot}{MoRoot}/document/{MoId(k_idDocument)}/verifica", Nothing,
                "verificarea ștergerii documentului", ct).ConfigureAwait(False)
            Return MoVerificare(JsonSerializer.Deserialize(Of MoVerificaResponse)(k_resp, _json))
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.VerificaStergereDocumentAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function StergeOperatieAsync(k_idOper As Integer, ct As CancellationToken) As Task(Of Integer) _
        Implements IModificareOperatiiApi.StergeOperatieAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Delete, $"{AvacontRoot}{MoRoot}/operatie/{MoId(k_idOper)}", Nothing,
                "ștergerea operației", ct).ConfigureAwait(False)
            Dim k_payload As MoStergeResponse = JsonSerializer.Deserialize(Of MoStergeResponse)(k_resp, _json)
            Return If(k_payload Is Nothing, 0, k_payload.operatii)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.StergeOperatieAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function StergeDocumentAsync(k_idDocument As Integer, ct As CancellationToken) As Task(Of Integer) _
        Implements IModificareOperatiiApi.StergeDocumentAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Delete, $"{AvacontRoot}{MoRoot}/document/{MoId(k_idDocument)}", Nothing,
                "ștergerea documentului", ct).ConfigureAwait(False)
            Dim k_payload As MoStergeResponse = JsonSerializer.Deserialize(Of MoStergeResponse)(k_resp, _json)
            Return If(k_payload Is Nothing, 0, k_payload.operatii)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.StergeDocumentAsync", ex)
            Throw
        End Try
    End Function

End Class
