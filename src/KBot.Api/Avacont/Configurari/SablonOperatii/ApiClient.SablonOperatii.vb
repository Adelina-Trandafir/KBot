Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice AV01-14 - the operation templates. Wire field names exactly as routes/avacont/configurari/sablon_operatii/sablon_operatii.py.
Partial Public Class ApiClient
    Implements ISablonOperatiiApi

    Private NotInheritable Class SablonLinieWire
        Public Property idlinie As Integer?
        Public Property forma As String
        Public Property contdebit As Integer
        Public Property contcredit As Integer
        Public Property partdebitmod As String
        Public Property idpartdebit As Integer?
        Public Property partcreditmod As String
        Public Property idpartcredit As Integer?
        Public Property pa As Boolean
        Public Property ordonantare As Boolean
        Public Property chelt As Boolean
        Public Property idclsf As Integer?
        Public Property analiticdebit As String
        Public Property analiticcredit As String
    End Class

    Private NotInheritable Class SablonClsfWire
        Public Property idclsf As Integer
        Public Property clsf As String
        Public Property clsff As String
        Public Property clsfe As String
        Public Property ss As String
        Public Property capitol As String
        Public Property articol As String
        Public Property alineat As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class SablonContPlanWire
        Public Property ss As String
        Public Property simbol As String
        Public Property cont As Integer
        Public Property analitic As String
        Public Property clsff As String
        Public Property clsfe As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class SablonWire
        Public Property idsablon As Integer?
        Public Property idjurnal As Integer
        Public Property tip As String
        Public Property fel As String
        Public Property explicatie As String
        Public Property compartiment As String
        Public Property felop As String
        Public Property cheie As String
        Public Property linii As List(Of SablonLinieWire)
    End Class

    Private NotInheritable Class SablonJurnalWire
        Public Property idjurnal As Integer
        Public Property sursasector As String
        Public Property cod As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class SablonContWire
        Public Property simbol As Integer
        Public Property denumire As String
    End Class

    Private NotInheritable Class SablonPartenerWire
        Public Property idpartener As Integer
        Public Property cod As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class SablonClasificatieWire
        Public Property cheie As String
        Public Property ss As String
        Public Property forme As List(Of String)
    End Class

    Private NotInheritable Class SablonNumeWire
        Public Property cod As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class SabloaneOperatiiResponse
        Public Property sabloane As List(Of SablonWire)
        Public Property jurnale As List(Of SablonJurnalWire)
        Public Property conturi As List(Of SablonContWire)
        Public Property conturi_plan As List(Of SablonContPlanWire)
        Public Property clsf As List(Of SablonClsfWire)
        Public Property parteneri As List(Of SablonPartenerWire)
        Public Property clasificatii As List(Of SablonClasificatieWire)
        Public Property nume_clsfe As List(Of SablonNumeWire)
    End Class

    Private NotInheritable Class SabloaneOperatiiSaveRequest
        Public Property sabloane As List(Of SablonWire)
        Public Property sterse As List(Of Integer)
    End Class

    Public Async Function GetSabloaneOperatiiAsync(ct As CancellationToken) As Task(Of SabloaneOperatiiCatalog) _
        Implements ISablonOperatiiApi.GetSabloaneOperatiiAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"{AvacontRoot}/sabloane-operatii", Nothing, "citirea șabloanelor de operații", ct).ConfigureAwait(False)
            Dim k_payload As SabloaneOperatiiResponse = JsonSerializer.Deserialize(Of SabloaneOperatiiResponse)(k_resp, _json)
            If k_payload Is Nothing Then Return New SabloaneOperatiiCatalog()
            Return New SabloaneOperatiiCatalog() With {
                .Sabloane = If(k_payload.sabloane, New List(Of SablonWire)()).Select(AddressOf ToSablon).ToList(),
                .Jurnale = If(k_payload.jurnale, New List(Of SablonJurnalWire)()).Select(
                    Function(k_j) New JurnalInfo() With {
                        .IdJurnal = k_j.idjurnal, .SursaSector = If(k_j.sursasector, String.Empty),
                        .Cod = If(k_j.cod, String.Empty), .Denumire = If(k_j.denumire, String.Empty)}).ToList(),
                .Conturi = If(k_payload.conturi, New List(Of SablonContWire)()).Select(
                    Function(k_c) New SablonContInfo() With {.Simbol = k_c.simbol, .Denumire = If(k_c.denumire, String.Empty)}).ToList(),
                .ClasificatiiLinii = If(k_payload.clsf, New List(Of SablonClsfWire)()).Select(
                    Function(k_c) New SablonClsfInfo() With {
                        .IdClsf = k_c.idclsf, .Clsf = If(k_c.clsf, String.Empty), .ClsfF = If(k_c.clsff, String.Empty), .ClsfE = If(k_c.clsfe, String.Empty),
                        .SursaSector = If(k_c.ss, String.Empty), .Capitol = If(k_c.capitol, String.Empty), .Articol = If(k_c.articol, String.Empty),
                        .Alineat = If(k_c.alineat, String.Empty), .Denumire = If(k_c.denumire, String.Empty)}).ToList(),
                .ConturiPlan = If(k_payload.conturi_plan, New List(Of SablonContPlanWire)()).Select(
                    Function(k_c) New SablonContPlanInfo() With {
                        .SursaSector = If(k_c.ss, String.Empty), .Simbol = If(k_c.simbol, String.Empty), .Cont = k_c.cont,
                        .Analitic = If(k_c.analitic, String.Empty), .ClsfF = If(k_c.clsff, String.Empty), .ClsfE = If(k_c.clsfe, String.Empty),
                        .Denumire = If(k_c.denumire, String.Empty)}).ToList(),
                .Parteneri = If(k_payload.parteneri, New List(Of SablonPartenerWire)()).Select(
                    Function(k_p) New SablonPartenerInfo() With {
                        .IdPartener = k_p.idpartener, .Cod = If(k_p.cod, String.Empty), .Denumire = If(k_p.denumire, String.Empty)}).ToList(),
                .Clasificatii = If(k_payload.clasificatii, New List(Of SablonClasificatieWire)()).Select(
                    Function(k_c) New SablonClasificatieInfo() With {
                        .Cheie = If(k_c.cheie, String.Empty), .SursaSector = If(k_c.ss, String.Empty),
                        .Forme = If(k_c.forme, New List(Of String)())}).ToList(),
                .NumeClsfE = If(k_payload.nume_clsfe, New List(Of SablonNumeWire)()).
                    Where(Function(k_n) Not String.IsNullOrEmpty(k_n.cod)).
                    GroupBy(Function(k_n) k_n.cod, StringComparer.Ordinal).
                    ToDictionary(Function(k_g) k_g.Key, Function(k_g) If(k_g.First().denumire, String.Empty), StringComparer.Ordinal)}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetSabloaneOperatiiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveSabloaneOperatiiAsync(k_sabloane As IReadOnlyList(Of SablonOperatieInfo), k_sterse As IReadOnlyList(Of Integer), ct As CancellationToken) As Task _
        Implements ISablonOperatiiApi.SaveSabloaneOperatiiAsync
        Try
            ArgumentNullException.ThrowIfNull(k_sabloane)
            ArgumentNullException.ThrowIfNull(k_sterse)
            Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{AvacontRoot}/sabloane-operatii",
                New SabloaneOperatiiSaveRequest() With {
                    .sabloane = k_sabloane.Select(AddressOf ToWire).ToList(),
                    .sterse = k_sterse.ToList()},
                "salvarea șabloanelor de operații", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveSabloaneOperatiiAsync", ex)
            Throw
        End Try
    End Function

    ' Pure mapping, reached only through the two wrapped methods above.
    Private Shared Function ToSablon(k_w As SablonWire) As SablonOperatieInfo
        Return New SablonOperatieInfo() With {
            .IdSablon = If(k_w.idsablon, 0), .IdJurnal = k_w.idjurnal, .Tip = If(k_w.tip, "M"), .Fel = If(k_w.fel, "E"),
            .Explicatie = If(k_w.explicatie, String.Empty), .Compartiment = If(k_w.compartiment, String.Empty),
            .FelOP = If(k_w.felop, String.Empty), .Cheie = If(k_w.cheie, String.Empty),
            .Linii = If(k_w.linii, New List(Of SablonLinieWire)()).Select(
                Function(k_l) New SablonLinieInfo() With {
                    .IdLinie = If(k_l.idlinie, 0), .Forma = If(k_l.forma, String.Empty),
                    .ContDebit = k_l.contdebit, .ContCredit = k_l.contcredit,
                    .PartDebitMod = If(k_l.partdebitmod, SablonOperatiiModuri.Nimic), .IdPartDebit = If(k_l.idpartdebit, 0),
                    .PartCreditMod = If(k_l.partcreditmod, SablonOperatiiModuri.Nimic), .IdPartCredit = If(k_l.idpartcredit, 0),
                    .PA = k_l.pa, .Ordonantare = k_l.ordonantare, .Chelt = k_l.chelt,
                    .IdClsf = If(k_l.idclsf, 0), .AnaliticDebit = If(k_l.analiticdebit, String.Empty),
                    .AnaliticCredit = If(k_l.analiticcredit, String.Empty)}).ToList()}
    End Function

    Private Shared Function ToWire(k_s As SablonOperatieInfo) As SablonWire
        Return New SablonWire() With {
            .idsablon = If(k_s.IdSablon > 0, CType(k_s.IdSablon, Integer?), Nothing), .idjurnal = k_s.IdJurnal, .tip = k_s.Tip, .fel = k_s.Fel,
            .explicatie = k_s.Explicatie, .compartiment = k_s.Compartiment, .felop = k_s.FelOP, .cheie = k_s.Cheie,
            .linii = k_s.Linii.Select(
                Function(k_l) New SablonLinieWire() With {
                    .idlinie = If(k_l.IdLinie > 0, CType(k_l.IdLinie, Integer?), Nothing), .forma = k_l.Forma,
                    .contdebit = k_l.ContDebit, .contcredit = k_l.ContCredit,
                    .partdebitmod = k_l.PartDebitMod, .idpartdebit = If(k_l.IdPartDebit > 0, CType(k_l.IdPartDebit, Integer?), Nothing),
                    .partcreditmod = k_l.PartCreditMod, .idpartcredit = If(k_l.IdPartCredit > 0, CType(k_l.IdPartCredit, Integer?), Nothing),
                    .pa = k_l.PA, .ordonantare = k_l.Ordonantare, .chelt = k_l.Chelt,
                    .idclsf = If(k_l.IdClsf > 0, CType(k_l.IdClsf, Integer?), Nothing), .analiticdebit = k_l.AnaliticDebit, .analiticcredit = k_l.AnaliticCredit}).ToList()}
    End Function

End Class
