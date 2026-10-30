Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice AV01-15 - the salary templates. Wire field names exactly as routes/avacont/configurari/sablon_salarii/sablon_salarii.py.
Partial Public Class ApiClient
    Implements ISablonSalariiApi

    Private NotInheritable Class SalLinieWire
        Public Property idlinie As Integer?
        Public Property ref As Integer
        Public Property parinte As Integer?
        Public Property tip As String
        Public Property explicatie As String
        Public Property contdebit As Integer
        Public Property contcredit As Integer
        Public Property partdebitmod As String
        Public Property idpartdebit As Integer?
        Public Property partcreditmod As String
        Public Property idpartcredit As Integer?
        Public Property estecm As Boolean
    End Class

    ' The server's GET names the parent of a line "idparinte" (a database id); the window's POST names it "parinte" (a ref).
    Private NotInheritable Class SalLinieReadWire
        Public Property idlinie As Integer?
        Public Property idparinte As Integer?
        Public Property tip As String
        Public Property explicatie As String
        Public Property contdebit As Integer
        Public Property contcredit As Integer
        Public Property partdebitmod As String
        Public Property idpartdebit As Integer?
        Public Property partcreditmod As String
        Public Property idpartcredit As Integer?
        Public Property estecm As Boolean
    End Class

    Private NotInheritable Class SalArticolReadWire
        Public Property idarticol As Integer?
        Public Property idsubunitate As Integer
        Public Property clasificatie As String
        Public Property explicatie As String
        Public Property contdebit As Integer
        Public Property contcredit As Integer
        Public Property partdebitmod As String
        Public Property idpartdebit As Integer?
        Public Property partcreditmod As String
        Public Property idpartcredit As Integer?
        Public Property linii As List(Of SalLinieReadWire)
    End Class

    Private NotInheritable Class SalArticolWire
        Public Property idarticol As Integer?
        Public Property idsubunitate As Integer
        Public Property clasificatie As String
        Public Property explicatie As String
        Public Property contdebit As Integer
        Public Property contcredit As Integer
        Public Property partdebitmod As String
        Public Property idpartdebit As Integer?
        Public Property partcreditmod As String
        Public Property idpartcredit As Integer?
        Public Property linii As List(Of SalLinieWire)
    End Class

    Private NotInheritable Class SalSubunitWire
        Public Property idsubunitate As Integer
        Public Property capitol As String
        Public Property sursasector As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class SabloaneSalariiResponse
        Public Property articole As List(Of SalArticolReadWire)
        Public Property subunitati As List(Of SalSubunitWire)
        Public Property conturi As List(Of SablonContWire)
        Public Property parteneri As List(Of SablonPartenerWire)
    End Class

    Private NotInheritable Class SabloaneSalariiSaveRequest
        Public Property articole As List(Of SalArticolWire)
        Public Property sterse As List(Of Integer)
    End Class

    Public Async Function GetSabloaneSalariiAsync(ct As CancellationToken) As Task(Of SabloaneSalariiCatalog) _
        Implements ISablonSalariiApi.GetSabloaneSalariiAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"{AvacontRoot}/sabloane-salarii", Nothing, "citirea șabloanelor de salarii", ct).ConfigureAwait(False)
            Dim k_payload As SabloaneSalariiResponse = JsonSerializer.Deserialize(Of SabloaneSalariiResponse)(k_resp, _json)
            If k_payload Is Nothing Then Return New SabloaneSalariiCatalog()
            Return New SabloaneSalariiCatalog() With {
                .Articole = If(k_payload.articole, New List(Of SalArticolReadWire)()).Select(AddressOf ToArticol).ToList(),
                .Subunitati = If(k_payload.subunitati, New List(Of SalSubunitWire)()).Select(
                    Function(k_s) New SablonSubunitInfo() With {
                        .IdSubunitate = k_s.idsubunitate, .Capitol = If(k_s.capitol, String.Empty),
                        .SursaSector = If(k_s.sursasector, String.Empty), .Denumire = If(k_s.denumire, String.Empty)}).ToList(),
                .Conturi = If(k_payload.conturi, New List(Of SablonContWire)()).Select(
                    Function(k_c) New SablonContInfo() With {.Simbol = k_c.simbol, .Denumire = If(k_c.denumire, String.Empty)}).ToList(),
                .Parteneri = If(k_payload.parteneri, New List(Of SablonPartenerWire)()).Select(
                    Function(k_p) New SablonPartenerInfo() With {
                        .IdPartener = k_p.idpartener, .Cod = If(k_p.cod, String.Empty), .Denumire = If(k_p.denumire, String.Empty)}).ToList()}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetSabloaneSalariiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveSabloaneSalariiAsync(k_articole As IReadOnlyList(Of SablonSalariiArticolInfo), k_sterse As IReadOnlyList(Of Integer), ct As CancellationToken) As Task _
        Implements ISablonSalariiApi.SaveSabloaneSalariiAsync
        Try
            ArgumentNullException.ThrowIfNull(k_articole)
            ArgumentNullException.ThrowIfNull(k_sterse)
            Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{AvacontRoot}/sabloane-salarii",
                New SabloaneSalariiSaveRequest() With {
                    .articole = k_articole.Select(AddressOf ToArticolWire).ToList(),
                    .sterse = k_sterse.ToList()},
                "salvarea șabloanelor de salarii", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveSabloaneSalariiAsync", ex)
            Throw
        End Try
    End Function

    ' Pure mapping, reached only through the two wrapped methods above. The server names the parent of a line by database id; the Ref of a
    ' saved line IS its id, so the parent of a line read from the server is the Ref of the line it names.
    Private Shared Function ToArticol(k_w As SalArticolReadWire) As SablonSalariiArticolInfo
        Return New SablonSalariiArticolInfo() With {
            .IdArticol = If(k_w.idarticol, 0), .IdSubunitate = k_w.idsubunitate, .Clasificatie = If(k_w.clasificatie, String.Empty),
            .Explicatie = If(k_w.explicatie, String.Empty), .ContDebit = k_w.contdebit, .ContCredit = k_w.contcredit,
            .PartDebitMod = If(k_w.partdebitmod, SablonOperatiiModuri.Nimic), .IdPartDebit = If(k_w.idpartdebit, 0),
            .PartCreditMod = If(k_w.partcreditmod, SablonOperatiiModuri.Nimic), .IdPartCredit = If(k_w.idpartcredit, 0),
            .Linii = If(k_w.linii, New List(Of SalLinieReadWire)()).Select(
                Function(k_l) New SablonSalariiLinieInfo() With {
                    .IdLinie = If(k_l.idlinie, 0), .Ref = If(k_l.idlinie, 0), .Parinte = If(k_l.idparinte, 0),
                    .Tip = If(k_l.tip, SablonSalariiTipuri.Retinere), .Explicatie = If(k_l.explicatie, String.Empty),
                    .ContDebit = k_l.contdebit, .ContCredit = k_l.contcredit,
                    .PartDebitMod = If(k_l.partdebitmod, SablonOperatiiModuri.Nimic), .IdPartDebit = If(k_l.idpartdebit, 0),
                    .PartCreditMod = If(k_l.partcreditmod, SablonOperatiiModuri.Nimic), .IdPartCredit = If(k_l.idpartcredit, 0),
                    .EsteCM = k_l.estecm}).ToList()}
    End Function

    Private Shared Function ToArticolWire(k_a As SablonSalariiArticolInfo) As SalArticolWire
        Return New SalArticolWire() With {
            .idarticol = If(k_a.IdArticol > 0, CType(k_a.IdArticol, Integer?), Nothing), .idsubunitate = k_a.IdSubunitate,
            .clasificatie = k_a.Clasificatie, .explicatie = k_a.Explicatie, .contdebit = k_a.ContDebit, .contcredit = k_a.ContCredit,
            .partdebitmod = k_a.PartDebitMod, .idpartdebit = If(k_a.IdPartDebit > 0, CType(k_a.IdPartDebit, Integer?), Nothing),
            .partcreditmod = k_a.PartCreditMod, .idpartcredit = If(k_a.IdPartCredit > 0, CType(k_a.IdPartCredit, Integer?), Nothing),
            .linii = k_a.Linii.Select(
                Function(k_l) New SalLinieWire() With {
                    .idlinie = If(k_l.IdLinie > 0, CType(k_l.IdLinie, Integer?), Nothing), .ref = k_l.Ref,
                    .parinte = If(k_l.Parinte <> 0, CType(k_l.Parinte, Integer?), Nothing),
                    .tip = k_l.Tip, .explicatie = k_l.Explicatie, .contdebit = k_l.ContDebit, .contcredit = k_l.ContCredit,
                    .partdebitmod = k_l.PartDebitMod, .idpartdebit = If(k_l.IdPartDebit > 0, CType(k_l.IdPartDebit, Integer?), Nothing),
                    .partcreditmod = k_l.PartCreditMod, .idpartcredit = If(k_l.IdPartCredit > 0, CType(k_l.IdPartCredit, Integer?), Nothing),
                    .estecm = k_l.EsteCM}).ToList()}
    End Function

End Class
