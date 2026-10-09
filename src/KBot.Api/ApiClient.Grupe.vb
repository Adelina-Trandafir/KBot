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

' Slice 0008-02 - groups of angajamente. Wire field names exactly as routes/forexe/grupe.py
' writes / reads them.
Partial Public Class ApiClient
    Implements IGrupeApi

    Private Const GrupeRoot As String = "/api/forexe/grupe"

    Private NotInheritable Class GrupaIndicatorWire
        Public Property clsf As String
        Public Property denumire As String
        Public Property ss As String
    End Class

    Private NotInheritable Class GrupaAngajamentWire
        Public Property cod As String
        Public Property descriere As String
        Public Property [alias] As String
        Public Property indicatori As List(Of GrupaIndicatorWire)
    End Class

    Private NotInheritable Class GrupaWire
        Public Property idgr As Integer
        Public Property denumire As String
        Public Property culoare As String
        Public Property coduri As List(Of String)
    End Class

    Private NotInheritable Class GrupeResponse
        Public Property grupe As List(Of GrupaWire)
        Public Property angajamente As List(Of GrupaAngajamentWire)
    End Class

    Private NotInheritable Class GrupaSaveRequest
        Public Property idgr As Integer?
        Public Property denumire As String
        Public Property culoare As String
        Public Property coduri As List(Of String)
    End Class

    Private NotInheritable Class GrupaSaveResponse
        Public Property idgr As Integer
    End Class

    Private NotInheritable Class GrupaCodRequest
        Public Property cod As String
    End Class

    Private NotInheritable Class AliasRequest
        Public Property cod As String
        Public Property [alias] As String
    End Class

    Public Async Function GetGrupeAsync(ct As CancellationToken) As Task(Of GrupeCatalog) _
        Implements IGrupeApi.GetGrupeAsync
        Try
            Dim respText As String = Await SendNomenclatoareAsync(HttpMethod.Get, GrupeRoot, Nothing,
                                                                  "citirea grupelor de angajamente", ct).ConfigureAwait(False)
            Dim payload As GrupeResponse = JsonSerializer.Deserialize(Of GrupeResponse)(respText, _json)
            Dim result As New GrupeCatalog()
            If payload Is Nothing Then Return result
            result.Grupe = If(payload.grupe, New List(Of GrupaWire)()).Select(Function(k_g) New GrupaInfo() With {
                .IdGr = k_g.idgr, .Denumire = If(k_g.denumire, String.Empty),
                .Culoare = If(String.IsNullOrWhiteSpace(k_g.culoare), "#000000", k_g.culoare),
                .Coduri = If(k_g.coduri, New List(Of String)())}).ToList()
            result.Angajamente = If(payload.angajamente, New List(Of GrupaAngajamentWire)()).Select(
                Function(k_a) New GrupaAngajament() With {
                    .Cod = If(k_a.cod, String.Empty), .Descriere = If(k_a.descriere, String.Empty),
                    .AliasAng = If(k_a.alias, String.Empty),
                    .Indicatori = If(k_a.indicatori, New List(Of GrupaIndicatorWire)()).Select(
                        Function(k_i) New GrupaIndicator() With {
                            .Clsf = If(k_i.clsf, String.Empty), .Denumire = If(k_i.denumire, String.Empty),
                            .Ss = If(k_i.ss, String.Empty)}).ToList()}).ToList()
            Return result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetGrupeAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveGrupaAsync(idGr As Integer?, denumire As String, culoare As String,
                                         coduri As IReadOnlyList(Of String), ct As CancellationToken) As Task(Of Integer) _
        Implements IGrupeApi.SaveGrupaAsync
        Try
            ArgumentNullException.ThrowIfNull(coduri)
            Dim respText As String = Await SendNomenclatoareAsync(
                HttpMethod.Post, GrupeRoot,
                New GrupaSaveRequest() With {.idgr = idGr, .denumire = denumire, .culoare = culoare,
                                             .coduri = coduri.ToList()},
                "salvarea grupei", ct).ConfigureAwait(False)
            Dim payload As GrupaSaveResponse = JsonSerializer.Deserialize(Of GrupaSaveResponse)(respText, _json)
            If payload Is Nothing Then Throw New ApiException("Serverul nu a întors grupa salvată.")
            Return payload.idgr
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveGrupaAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function AdaugaInGrupaAsync(idGr As Integer, cod As String, ct As CancellationToken) As Task _
        Implements IGrupeApi.AdaugaInGrupaAsync
        Try
            Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{GrupeRoot}/{idGr.ToString(CultureInfo.InvariantCulture)}/angajamente",
                New GrupaCodRequest() With {.cod = cod}, "adăugarea angajamentului în grupă", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.AdaugaInGrupaAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveAliasAsync(cod As String, aliasAng As String, ct As CancellationToken) As Task _
        Implements IGrupeApi.SaveAliasAsync
        Try
            Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{GrupeRoot}/alias",
                New AliasRequest() With {.cod = cod, .[alias] = aliasAng}, "salvarea aliasului", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveAliasAsync", ex)
            Throw
        End Try
    End Function

End Class
