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

' Slices AV02-03 / AV02-04 - the posting windows. Wire field names exactly as routes/avacont/introducere_date/operatii/op9.py and facturi_plati.py (all lowercase).
Partial Public Class ApiClient
    Implements IOperatiiApi

    Private NotInheritable Class OpNotaWire
        Public Property numarnc As Integer
        Public Property datanc As String
        Public Property documente As Integer
    End Class

    Private NotInheritable Class OpSubunitateWire
        Public Property idsubunitate As Integer
        Public Property denumire As String
        Public Property sursasector As String
    End Class

    Private NotInheritable Class OpContextResponse
        Public Property subunitate As OpSubunitateWire
        Public Property note As List(Of OpNotaWire)
        Public Property urmatoare As Integer
        Public Property inchidere As String
        Public Property parteneri As List(Of FpPartenerWire)
        Public Property clasificatii As List(Of FpClasificatieWire)
    End Class

    Private NotInheritable Class FpPartenerWire
        Public Property idpartener As Integer
        Public Property cod As String
        Public Property denumire As String
        Public Property cui As String
        Public Property iban As String
    End Class

    Private NotInheritable Class FpSablonWire
        Public Property idsablon As Integer
        Public Property explicatie As String
        Public Property felop As String
    End Class

    Private NotInheritable Class FpClasificatieWire
        Public Property idclsf As Integer
        Public Property clsf As String
        Public Property denumire As String
        Public Property forma As String
        Public Property sabloane_e As List(Of FpSablonWire)
        Public Property sabloane_p As List(Of FpSablonWire)
    End Class

    Private NotInheritable Class FpFacturaWire
        Public Property idoper As Integer
        Public Property numar As String
        Public Property data As String
        Public Property fel As String
        Public Property valoare As Decimal
        Public Property platit As Decimal
        Public Property rest As Decimal
    End Class

    Private NotInheritable Class FpPlataWire
        Public Property idop As Integer
        Public Property nrop As Integer
        Public Property dataop As String
        Public Property valoare As Decimal
        Public Property explicatie As String
        Public Property documente As String
        Public Property blocat As Boolean
        Public Property motiv As String
    End Class

    Private NotInheritable Class FpDeschiseResponse
        Public Property curent As List(Of FpFacturaWire)
        Public Property anterior As List(Of FpFacturaWire)
        Public Property plati As List(Of FpPlataWire)
        Public Property numarop As Integer
        Public Property contiban As String
    End Class

    Private NotInheritable Class OpLinieRequest
        Public Property idlinie As Integer
        Public Property suma As Decimal
    End Class

    Private NotInheritable Class OpDiverseRequest
        Public Property idsubunitate As Integer
        Public Property an As Integer
        Public Property idsablon As Integer
        Public Property numarnc As Integer
        Public Property numardocument As String
        Public Property feldocument As String
        Public Property datadocument As String
        Public Property idpartener As Integer?
        Public Property linii As List(Of OpLinieRequest)
    End Class

    Private NotInheritable Class OpDiverseOperatieWire
        Public Property idlinie As Integer
        Public Property iddocument As Integer
        Public Property idoper As Integer
        Public Property nrang As Integer
    End Class

    Private NotInheritable Class OpDiverseResponse
        Public Property idnota As Integer
        Public Property numarnc As Integer
        Public Property an As Integer
        Public Property operatii As List(Of OpDiverseOperatieWire)
    End Class

    Private NotInheritable Class FpNouaRequest
        Public Property numar As String
        Public Property data As String
        Public Property suma As Decimal
        Public Property fel As String
        Public Property platit As Decimal
    End Class

    Private NotInheritable Class FpExistentaRequest
        Public Property idoper As Integer
        Public Property platit As Decimal
    End Class

    Private NotInheritable Class FpSalveazaRequest
        Public Property idsubunitate As Integer
        Public Property an As Integer
        Public Property idpartener As Integer
        Public Property idclsf As Integer
        Public Property idsablone As Integer?
        Public Property idsablonp As Integer?
        Public Property numarnc As Integer
        Public Property dataop As String
        Public Property explicatieop As String
        Public Property noi As List(Of FpNouaRequest)
        Public Property existente As List(Of FpExistentaRequest)
    End Class

    Private NotInheritable Class FpEvidentaWire
        Public Property numar As String
        Public Property idoper As Integer
    End Class

    Private NotInheritable Class FpPlataSalvataWire
        Public Property idoper As Integer
        Public Property idop As Integer
        Public Property nrop As Integer
        Public Property valoare As Decimal
        Public Property dataop As String
    End Class

    Private NotInheritable Class FpSalveazaResponse
        Public Property idnota As Integer
        Public Property numarnc As Integer
        Public Property an As Integer
        Public Property evidente As List(Of FpEvidentaWire)
        Public Property plata As FpPlataSalvataWire
    End Class

    Private Const OpIso As String = "yyyy-MM-dd"

    Private Shared Function OpDate(k_text As String) As Date?
        Dim k_value As Date
        If Date.TryParseExact(If(k_text, String.Empty), OpIso, CultureInfo.InvariantCulture, DateTimeStyles.None, k_value) Then Return k_value
        Return Nothing
    End Function

    Private Shared Function OpQuery(k_idSubunitate As Integer, k_an As Integer) As String
        Return "idsubunitate=" & k_idSubunitate.ToString(CultureInfo.InvariantCulture) & "&an=" & k_an.ToString(CultureInfo.InvariantCulture)
    End Function

    Private Shared Function MapOpContext(k_payload As OpContextResponse) As OperatiiContext
        Dim k_ctx As New OperatiiContext()
        If k_payload Is Nothing Then Return k_ctx
        k_ctx.IdSubunitate = If(k_payload.subunitate Is Nothing, 0, k_payload.subunitate.idsubunitate)
        k_ctx.Subunitate = If(k_payload.subunitate Is Nothing, String.Empty, If(k_payload.subunitate.denumire, String.Empty))
        k_ctx.Urmatoare = k_payload.urmatoare
        k_ctx.Inchidere = OpDate(k_payload.inchidere)
        k_ctx.Note = If(k_payload.note, New List(Of OpNotaWire)()).Select(
            Function(k_n) New OperatiiNotaInfo() With {.NumarNC = k_n.numarnc, .DataNc = OpDate(k_n.datanc), .Documente = k_n.documente}).ToList()
        Return k_ctx
    End Function

    Private Shared Function MapFpSabloane(k_list As List(Of FpSablonWire)) As IReadOnlyList(Of FacturiPlatiSablon)
        Return If(k_list, New List(Of FpSablonWire)()).Select(
            Function(k_s) New FacturiPlatiSablon() With {.IdSablon = k_s.idsablon, .Explicatie = If(k_s.explicatie, String.Empty), .FelOP = If(k_s.felop, String.Empty)}).ToList()
    End Function

    Private Shared Function MapFpFacturi(k_list As List(Of FpFacturaWire)) As IReadOnlyList(Of FacturaDeschisa)
        Return If(k_list, New List(Of FpFacturaWire)()).Select(
            Function(k_f) New FacturaDeschisa() With {
                .IdOper = k_f.idoper, .Numar = If(k_f.numar, String.Empty), .Data = OpDate(k_f.data), .Fel = If(k_f.fel, String.Empty),
                .Valoare = k_f.valoare, .Platit = k_f.platit, .Rest = k_f.rest}).ToList()
    End Function

    Public Async Function GetOperatiiContextAsync(k_idSubunitate As Integer, k_an As Integer, ct As CancellationToken) As Task(Of OperatiiContext) _
        Implements IOperatiiApi.GetOperatiiContextAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"{AvacontRoot}/operatii-diverse/context?{OpQuery(k_idSubunitate, k_an)}", Nothing,
                "citirea datelor pentru operații diverse", ct).ConfigureAwait(False)
            Return MapOpContext(JsonSerializer.Deserialize(Of OpContextResponse)(k_resp, _json))
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetOperatiiContextAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SalveazaOperatiiDiverseAsync(k_cerere As OperatiiDiverseCerere, ct As CancellationToken) As Task(Of OperatiiDiverseRezultat) _
        Implements IOperatiiApi.SalveazaOperatiiDiverseAsync
        Try
            ArgumentNullException.ThrowIfNull(k_cerere)
            Dim k_body As New OpDiverseRequest() With {
                .idsubunitate = k_cerere.IdSubunitate, .an = k_cerere.An, .idsablon = k_cerere.IdSablon, .numarnc = k_cerere.NumarNC,
                .numardocument = k_cerere.NumarDocument, .feldocument = k_cerere.FelDocument,
                .datadocument = k_cerere.DataDocument.ToString(OpIso, CultureInfo.InvariantCulture),
                .idpartener = If(k_cerere.IdPartener > 0, CType(k_cerere.IdPartener, Integer?), Nothing),
                .linii = k_cerere.Linii.Select(Function(k_l) New OpLinieRequest() With {.idlinie = k_l.IdLinie, .suma = k_l.Suma}).ToList()}
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{AvacontRoot}/operatii-diverse/salveaza", k_body, "salvarea operațiilor diverse", ct).ConfigureAwait(False)
            Dim k_payload As OpDiverseResponse = JsonSerializer.Deserialize(Of OpDiverseResponse)(k_resp, _json)
            Return New OperatiiDiverseRezultat() With {
                .NumarNC = If(k_payload Is Nothing, 0, k_payload.numarnc), .An = If(k_payload Is Nothing, 0, k_payload.an),
                .Operatii = If(k_payload Is Nothing OrElse k_payload.operatii Is Nothing, 0, k_payload.operatii.Count)}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SalveazaOperatiiDiverseAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetFacturiPlatiContextAsync(k_idSubunitate As Integer, k_an As Integer, ct As CancellationToken) As Task(Of FacturiPlatiContext) _
        Implements IOperatiiApi.GetFacturiPlatiContextAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"{AvacontRoot}/facturi-plati/context?{OpQuery(k_idSubunitate, k_an)}", Nothing,
                "citirea datelor pentru facturi și plăți", ct).ConfigureAwait(False)
            Dim k_payload As OpContextResponse = JsonSerializer.Deserialize(Of OpContextResponse)(k_resp, _json)
            Dim k_ctx As New FacturiPlatiContext() With {.Baza = MapOpContext(k_payload)}
            If k_payload Is Nothing Then Return k_ctx
            k_ctx.Parteneri = If(k_payload.parteneri, New List(Of FpPartenerWire)()).Select(
                Function(k_p) New FacturiPlatiPartener() With {
                    .IdPartener = k_p.idpartener, .Cod = If(k_p.cod, String.Empty), .Denumire = If(k_p.denumire, String.Empty),
                    .Cui = If(k_p.cui, String.Empty), .Iban = If(k_p.iban, String.Empty)}).ToList()
            k_ctx.Clasificatii = If(k_payload.clasificatii, New List(Of FpClasificatieWire)()).Select(
                Function(k_c) New FacturiPlatiClasificatie() With {
                    .IdClsf = k_c.idclsf, .Clsf = If(k_c.clsf, String.Empty), .Denumire = If(k_c.denumire, String.Empty), .Forma = If(k_c.forma, String.Empty),
                    .SabloaneE = MapFpSabloane(k_c.sabloane_e), .SabloaneP = MapFpSabloane(k_c.sabloane_p)}).ToList()
            Return k_ctx
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetFacturiPlatiContextAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetFacturiPartenerAsync(k_idSubunitate As Integer, k_an As Integer, k_idPartener As Integer, k_idClsf As Integer,
                                                  ct As CancellationToken) As Task(Of FacturiPartener) _
        Implements IOperatiiApi.GetFacturiPartenerAsync
        Try
            Dim k_url As String = $"{AvacontRoot}/facturi-plati/deschise?{OpQuery(k_idSubunitate, k_an)}&idpartener={k_idPartener.ToString(CultureInfo.InvariantCulture)}"
            If k_idClsf > 0 Then k_url &= "&idclsf=" & k_idClsf.ToString(CultureInfo.InvariantCulture)
            Dim k_resp As String = Await SendNomenclatoareAsync(HttpMethod.Get, k_url, Nothing, "citirea facturilor partenerului", ct).ConfigureAwait(False)
            Dim k_payload As FpDeschiseResponse = JsonSerializer.Deserialize(Of FpDeschiseResponse)(k_resp, _json)
            If k_payload Is Nothing Then Return New FacturiPartener()
            Return New FacturiPartener() With {
                .Curent = MapFpFacturi(k_payload.curent), .Anterior = MapFpFacturi(k_payload.anterior), .NumarOP = k_payload.numarop,
                .ContIban = If(k_payload.contiban, String.Empty),
                .Plati = If(k_payload.plati, New List(Of FpPlataWire)()).Select(
                    Function(k_p) New PlataOrdin() With {
                        .IdOP = k_p.idop, .NrOP = k_p.nrop, .DataOP = OpDate(k_p.dataop), .Valoare = k_p.valoare, .Explicatie = If(k_p.explicatie, String.Empty),
                        .Documente = If(k_p.documente, String.Empty), .Blocat = k_p.blocat, .Motiv = If(k_p.motiv, String.Empty)}).ToList()}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetFacturiPartenerAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SalveazaFacturiPlatiAsync(k_cerere As FacturiPlatiCerere, ct As CancellationToken) As Task(Of FacturiPlatiRezultat) _
        Implements IOperatiiApi.SalveazaFacturiPlatiAsync
        Try
            ArgumentNullException.ThrowIfNull(k_cerere)
            Dim k_body As New FpSalveazaRequest() With {
                .idsubunitate = k_cerere.IdSubunitate, .an = k_cerere.An, .idpartener = k_cerere.IdPartener, .idclsf = k_cerere.IdClsf,
                .idsablone = If(k_cerere.IdSablonE > 0, CType(k_cerere.IdSablonE, Integer?), Nothing),
                .idsablonp = If(k_cerere.IdSablonP > 0, CType(k_cerere.IdSablonP, Integer?), Nothing),
                .numarnc = k_cerere.NumarNC, .dataop = k_cerere.DataOP.ToString(OpIso, CultureInfo.InvariantCulture), .explicatieop = k_cerere.ExplicatieOP,
                .noi = k_cerere.Noi.Select(Function(k_f) New FpNouaRequest() With {
                    .numar = k_f.Numar, .data = k_f.Data.ToString(OpIso, CultureInfo.InvariantCulture), .suma = k_f.Suma, .fel = k_f.Fel, .platit = k_f.Platit}).ToList(),
                .existente = k_cerere.Existente.Select(Function(k_f) New FpExistentaRequest() With {.idoper = k_f.IdOper, .platit = k_f.Platit}).ToList()}
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{AvacontRoot}/facturi-plati/salveaza", k_body, "salvarea facturilor și plăților", ct).ConfigureAwait(False)
            Dim k_payload As FpSalveazaResponse = JsonSerializer.Deserialize(Of FpSalveazaResponse)(k_resp, _json)
            Dim k_result As New FacturiPlatiRezultat()
            If k_payload Is Nothing Then Return k_result
            k_result.NumarNC = k_payload.numarnc
            k_result.An = k_payload.an
            k_result.Facturi = If(k_payload.evidente Is Nothing, 0, k_payload.evidente.Count)
            If k_payload.plata IsNot Nothing Then
                k_result.NrOP = k_payload.plata.nrop
                k_result.IdOP = k_payload.plata.idop
                k_result.ValoarePlata = k_payload.plata.valoare
            End If
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SalveazaFacturiPlatiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function StergePlataFacturiAsync(k_idOP As Integer, ct As CancellationToken) As Task _
        Implements IOperatiiApi.StergePlataFacturiAsync
        Try
            Await SendNomenclatoareAsync(HttpMethod.Delete, $"{AvacontRoot}/facturi-plati/plata/{k_idOP.ToString(CultureInfo.InvariantCulture)}", Nothing,
                                         "ștergerea plății", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.StergePlataFacturiAsync", ex)
            Throw
        End Try
    End Function

End Class
