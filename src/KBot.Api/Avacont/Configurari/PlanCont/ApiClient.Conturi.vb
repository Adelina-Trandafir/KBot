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

' Slice AV01-04 - subunits and the chart of accounts. Wire field names exactly as routes/avacont/subunitati.py
' and conturi.py write / read them.
Partial Public Class ApiClient
    Implements IConturiApi

    Private Const AvacontRoot As String = "/api/avacont"

    Private NotInheritable Class SubunitateWire
        Public Property idsubunitate As Integer
        Public Property denumire As String
        Public Property capitol As String
        Public Property sursasector As String
        Public Property zecimale As Integer
        Public Property implicita As Boolean
        Public Property activ As Boolean
        Public Property conturi As Integer
    End Class

    Private NotInheritable Class SubunitatiResponse
        Public Property subunitati As List(Of SubunitateWire)
    End Class

    Private NotInheritable Class SubunitateSaveRequest
        Public Property idsubunitate As Integer?
        Public Property denumire As String
        Public Property capitol As String
        Public Property sursasector As String
        Public Property zecimale As Integer
        Public Property implicita As Boolean
        Public Property activ As Boolean
    End Class

    Private NotInheritable Class SubunitateSaveResponse
        Public Property idsubunitate As Integer
    End Class

    Private NotInheritable Class ContPlanWire
        Public Property idcont As Integer
        Public Property cont As Integer
        Public Property sursasector As String
        Public Property analitic As String
        Public Property clsff As String
        Public Property clsfe As String
        Public Property simbol As String
        Public Property denumire As String
        Public Property fel As String
        Public Property extra As Boolean
        Public Property activ As Boolean
    End Class

    Private NotInheritable Class PlanConturiResponse
        Public Property subunitate As SubunitateWire
        Public Property conturi As List(Of ContPlanWire)
    End Class

    Private NotInheritable Class ClasaContWire
        Public Property clasa As Integer
        Public Property denumire As String
    End Class

    Private NotInheritable Class ContNomenclatorWire
        Public Property simbol As Integer
        Public Property denumire As String
        Public Property tip As String
        Public Property clsff As Integer
        Public Property clsfe As Integer
    End Class

    Private NotInheritable Class ClsfContWire
        Public Property cod As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class NomenclatorConturiResponse
        Public Property clase As List(Of ClasaContWire)
        Public Property conturi As List(Of ContNomenclatorWire)
        Public Property clsff As List(Of ClsfContWire)
        Public Property clsfe As List(Of ClsfContWire)
        Public Property surse As List(Of ClsfContWire)
    End Class

    Private NotInheritable Class SoldWire
        Public Property idcont As Integer
        Public Property sold_debitor As Double
        Public Property sold_creditor As Double
        Public Property rulaj_debitor As Double
        Public Property rulaj_creditor As Double
    End Class

    Private NotInheritable Class SolduriResponse
        Public Property solduri As List(Of SoldWire)
    End Class

    Private NotInheritable Class SoldSaveRequest
        Public Property idcont As Integer
        Public Property an As Integer
        Public Property sold_debitor As Decimal
        Public Property sold_creditor As Decimal
        Public Property rulaj_debitor As Decimal
        Public Property rulaj_creditor As Decimal
    End Class

    Private NotInheritable Class ContSaveRequest
        Public Property idcont As Integer?
        Public Property idsubunitate As Integer
        Public Property cont As Integer
        Public Property sursasector As String
        Public Property analitic As String
        Public Property clsff As String
        Public Property clsfe As String
        Public Property denumire As String
        Public Property fel As String
        Public Property extra As Boolean
        Public Property activ As Boolean
        Public Property confirmat_analitic As Boolean
    End Class

    Private NotInheritable Class ContSaveResponse
        Public Property idcont As Integer
        Public Property simbol As String
    End Class

    Private Shared Function ToSubunitate(k_wire As SubunitateWire) As SubunitateInfo
        Return New SubunitateInfo() With {
            .IdSubunitate = k_wire.idsubunitate, .Denumire = If(k_wire.denumire, String.Empty),
            .Capitol = If(k_wire.capitol, String.Empty), .SursaSector = If(k_wire.sursasector, String.Empty),
            .Zecimale = k_wire.zecimale, .Implicita = k_wire.implicita, .Activ = k_wire.activ,
            .NrConturi = k_wire.conturi}
    End Function

    Public Async Function GetSubunitatiAsync(ct As CancellationToken) As Task(Of IReadOnlyList(Of SubunitateInfo)) _
        Implements IConturiApi.GetSubunitatiAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(HttpMethod.Get, $"{AvacontRoot}/subunitati", Nothing,
                                                                "citirea subunităților", ct).ConfigureAwait(False)
            Dim k_payload As SubunitatiResponse = JsonSerializer.Deserialize(Of SubunitatiResponse)(k_resp, _json)
            If k_payload Is Nothing OrElse k_payload.subunitati Is Nothing Then Return New List(Of SubunitateInfo)()
            Return k_payload.subunitati.Select(AddressOf ToSubunitate).ToList()
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetSubunitatiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveSubunitateAsync(k_subunitate As SubunitateInfo, ct As CancellationToken) As Task(Of Integer) _
        Implements IConturiApi.SaveSubunitateAsync
        Try
            ArgumentNullException.ThrowIfNull(k_subunitate)
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{AvacontRoot}/subunitati",
                New SubunitateSaveRequest() With {
                    .idsubunitate = If(k_subunitate.IdSubunitate > 0, CType(k_subunitate.IdSubunitate, Integer?), Nothing),
                    .denumire = k_subunitate.Denumire, .capitol = k_subunitate.Capitol,
                    .sursasector = k_subunitate.SursaSector, .zecimale = k_subunitate.Zecimale,
                    .implicita = k_subunitate.Implicita, .activ = k_subunitate.Activ},
                "salvarea subunității", ct).ConfigureAwait(False)
            Dim k_payload As SubunitateSaveResponse = JsonSerializer.Deserialize(Of SubunitateSaveResponse)(k_resp, _json)
            If k_payload Is Nothing Then Throw New ApiException("Serverul nu a întors subunitatea salvată.")
            Return k_payload.idsubunitate
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveSubunitateAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function DeleteSubunitateAsync(k_idSubunitate As Integer, ct As CancellationToken) As Task _
        Implements IConturiApi.DeleteSubunitateAsync
        Try
            Await SendNomenclatoareAsync(
                HttpMethod.Delete, $"{AvacontRoot}/subunitati/{k_idSubunitate.ToString(CultureInfo.InvariantCulture)}",
                Nothing, "ștergerea subunității", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.DeleteSubunitateAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetPlanConturiAsync(k_idSubunitate As Integer?, ct As CancellationToken) As Task(Of PlanConturiInfo) _
        Implements IConturiApi.GetPlanConturiAsync
        Try
            Dim k_url As String = $"{AvacontRoot}/conturi"
            If k_idSubunitate.HasValue Then
                k_url &= "?subunitate=" & k_idSubunitate.Value.ToString(CultureInfo.InvariantCulture)
            End If
            Dim k_resp As String = Await SendNomenclatoareAsync(HttpMethod.Get, k_url, Nothing,
                                                                "citirea planului de conturi", ct).ConfigureAwait(False)
            Dim k_payload As PlanConturiResponse = JsonSerializer.Deserialize(Of PlanConturiResponse)(k_resp, _json)
            Dim k_result As New PlanConturiInfo()
            If k_payload Is Nothing Then Return k_result
            If k_payload.subunitate IsNot Nothing Then k_result.Subunitate = ToSubunitate(k_payload.subunitate)
            k_result.Conturi = If(k_payload.conturi, New List(Of ContPlanWire)()).Select(
                Function(k_c) New ContPlan() With {
                    .IdCont = k_c.idcont, .Cont = k_c.cont, .SursaSector = If(k_c.sursasector, String.Empty),
                    .Analitic = If(k_c.analitic, String.Empty),
                    .ClsfF = If(k_c.clsff, String.Empty), .ClsfE = If(k_c.clsfe, String.Empty),
                    .Simbol = If(k_c.simbol, String.Empty), .Denumire = If(k_c.denumire, String.Empty),
                    .Fel = If(k_c.fel, String.Empty), .Extra = k_c.extra, .Activ = k_c.activ}).ToList()
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetPlanConturiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetNomenclatorConturiAsync(ct As CancellationToken) As Task(Of NomenclatorConturi) _
        Implements IConturiApi.GetNomenclatorConturiAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(HttpMethod.Get, $"{AvacontRoot}/conturi/nomenclator", Nothing,
                                                                "citirea nomenclatorului de conturi", ct).ConfigureAwait(False)
            Dim k_payload As NomenclatorConturiResponse = JsonSerializer.Deserialize(Of NomenclatorConturiResponse)(k_resp, _json)
            Dim k_result As New NomenclatorConturi()
            If k_payload Is Nothing Then Return k_result
            k_result.Clase = If(k_payload.clase, New List(Of ClasaContWire)()).Select(
                Function(k_c) New ClasaCont() With {.Clasa = k_c.clasa, .Denumire = If(k_c.denumire, String.Empty)}).ToList()
            k_result.Conturi = If(k_payload.conturi, New List(Of ContNomenclatorWire)()).Select(
                Function(k_c) New ContNomenclator() With {
                    .Simbol = k_c.simbol, .Denumire = If(k_c.denumire, String.Empty), .Tip = If(k_c.tip, String.Empty),
                    .CereClsfF = k_c.clsff <> 0, .CereClsfE = k_c.clsfe <> 0}).ToList()
            k_result.ClsfF = If(k_payload.clsff, New List(Of ClsfContWire)()).Select(
                Function(k_c) New ClsfCont() With {.Cod = If(k_c.cod, String.Empty), .Denumire = If(k_c.denumire, String.Empty)}).ToList()
            k_result.ClsfE = If(k_payload.clsfe, New List(Of ClsfContWire)()).Select(
                Function(k_c) New ClsfCont() With {.Cod = If(k_c.cod, String.Empty), .Denumire = If(k_c.denumire, String.Empty)}).ToList()
            k_result.SurseSector = If(k_payload.surse, New List(Of ClsfContWire)()).Select(
                Function(k_c) New ClsfCont() With {.Cod = If(k_c.cod, String.Empty), .Denumire = If(k_c.denumire, String.Empty)}).ToList()
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetNomenclatorConturiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveContAsync(k_cont As ContSave, k_confirmatAnalitic As Boolean, ct As CancellationToken) As Task(Of ContSalvat) _
        Implements IConturiApi.SaveContAsync
        Try
            ArgumentNullException.ThrowIfNull(k_cont)
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{AvacontRoot}/conturi",
                New ContSaveRequest() With {
                    .idcont = k_cont.IdCont, .idsubunitate = k_cont.IdSubunitate, .cont = k_cont.Cont,
                    .sursasector = k_cont.SursaSector, .analitic = k_cont.Analitic, .clsff = k_cont.ClsfF, .clsfe = k_cont.ClsfE,
                    .denumire = k_cont.Denumire, .fel = k_cont.Fel, .extra = k_cont.Extra, .activ = k_cont.Activ,
                    .confirmat_analitic = k_confirmatAnalitic},
                "salvarea contului", ct).ConfigureAwait(False)
            Dim k_payload As ContSaveResponse = JsonSerializer.Deserialize(Of ContSaveResponse)(k_resp, _json)
            If k_payload Is Nothing Then Throw New ApiException("Serverul nu a întors contul salvat.")
            Return New ContSalvat() With {.IdCont = k_payload.idcont, .Simbol = If(k_payload.simbol, String.Empty)}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveContAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetSolduriAsync(k_idSubunitate As Integer, k_an As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of SoldCont)) _
        Implements IConturiApi.GetSolduriAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get,
                $"{AvacontRoot}/conturi/solduri?subunitate={k_idSubunitate.ToString(CultureInfo.InvariantCulture)}&an={k_an.ToString(CultureInfo.InvariantCulture)}",
                Nothing, "citirea soldurilor", ct).ConfigureAwait(False)
            Dim k_payload As SolduriResponse = JsonSerializer.Deserialize(Of SolduriResponse)(k_resp, _json)
            If k_payload Is Nothing OrElse k_payload.solduri Is Nothing Then Return New List(Of SoldCont)()
            Return k_payload.solduri.Select(
                Function(k_s) New SoldCont() With {
                    .IdCont = k_s.idcont, .SoldDebitor = CDec(k_s.sold_debitor), .SoldCreditor = CDec(k_s.sold_creditor),
                    .RulajDebitor = CDec(k_s.rulaj_debitor), .RulajCreditor = CDec(k_s.rulaj_creditor)}).ToList()
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetSolduriAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveSoldAsync(k_sold As SoldCont, k_an As Integer, ct As CancellationToken) As Task _
        Implements IConturiApi.SaveSoldAsync
        Try
            ArgumentNullException.ThrowIfNull(k_sold)
            Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{AvacontRoot}/conturi/solduri",
                New SoldSaveRequest() With {
                    .idcont = k_sold.IdCont, .an = k_an, .sold_debitor = k_sold.SoldDebitor, .sold_creditor = k_sold.SoldCreditor,
                    .rulaj_debitor = k_sold.RulajDebitor, .rulaj_creditor = k_sold.RulajCreditor},
                "salvarea soldurilor", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveSoldAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function DeleteContAsync(k_idCont As Integer, ct As CancellationToken) As Task _
        Implements IConturiApi.DeleteContAsync
        Try
            Await SendNomenclatoareAsync(
                HttpMethod.Delete, $"{AvacontRoot}/conturi/{k_idCont.ToString(CultureInfo.InvariantCulture)}",
                Nothing, "ștergerea contului", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.DeleteContAsync", ex)
            Throw
        End Try
    End Function

End Class
