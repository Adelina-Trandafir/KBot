Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 0087 - the «Clasificatii bugetare» and «Parteneri» windows. Wire field names exactly as
' routes/forexe/clasificatii_edit.py and routes/forexe/parteneri_edit.py read / write them.
Partial Public Class ApiClient
    Implements INomenclatoareApi

    Private Const NomenclatoareRoot As String = "/api/forexe/nomenclatoare"

    ' ── Wire shapes ─────────────────────────────────────────────────────────────
    Private NotInheritable Class ClsfItemWire
        Public Property id_clsf As Integer
        Public Property capitol As String
        Public Property subcapitol As String
        Public Property articol As String
        Public Property alineat As String
        Public Property denumire As String
        Public Property ss As String
        Public Property clsf As String
    End Class

    Private NotInheritable Class ClsfNamesWire
        Public Property capitol As Dictionary(Of String, String)
        Public Property subcapitol As Dictionary(Of String, String)
        Public Property articol As Dictionary(Of String, String)
        Public Property ss As Dictionary(Of String, String)
    End Class

    Private NotInheritable Class ClsfTreeResponse
        Public Property items As List(Of ClsfItemWire)
        Public Property names As ClsfNamesWire
    End Class

    Private NotInheritable Class QuartersWire
        Public Property trim1 As Double?
        Public Property trim2 As Double?
        Public Property trim3 As Double?
        Public Property trim4 As Double?
    End Class

    Private NotInheritable Class CorrectionWire
        Public Property id As Integer?
        Public Property document As String
        Public Property data As String
        Public Property trim1 As Double?
        Public Property trim2 As Double?
        Public Property trim3 As Double?
        Public Property trim4 As Double?
    End Class

    Private NotInheritable Class BudgetResponse
        Public Property budget As QuartersWire
        Public Property corrections As List(Of CorrectionWire)
    End Class

    Private NotInheritable Class BudgetRequest
        Public Property an As Integer
        Public Property budget As QuartersWire
        Public Property corrections As List(Of CorrectionWire)
        Public Property deleted As List(Of Integer)
    End Class

    Private NotInheritable Class CodeNameWire
        Public Property code As String
        Public Property name As String
    End Class

    Private NotInheritable Class CodeListWire
        Public Property codes As List(Of CodeNameWire)
        Public Property groups As Dictionary(Of String, String)
    End Class

    Private NotInheritable Class NomenclatorResponse
        Public Property ss As List(Of CodeNameWire)
        Public Property f As CodeListWire
        Public Property e As CodeListWire
    End Class

    Private NotInheritable Class AddClsfRequest
        Public Property an As Integer
        Public Property ss As List(Of String)
        Public Property f As List(Of String)
        Public Property e As List(Of String)
    End Class

    Private NotInheritable Class AddClsfResponse
        Public Property requested As Integer
        Public Property inserted As Integer
        Public Property existing As Integer
    End Class

    Private NotInheritable Class PartnerCodeWire
        Public Property id As Integer?
        Public Property id_clsf As Integer
        Public Property clsf As String
        Public Property denumire_clsf As String
        Public Property cont_bancar As String
        Public Property cod_ang As String
        Public Property cod_ind As String
    End Class

    Private NotInheritable Class PartnerWire
        Public Property id_partener As Integer?
        Public Property id_unitate As Integer
        Public Property ss As String
        Public Property cod_partener As String
        Public Property denumire As String
        Public Property cod_fiscal As String
        Public Property cont_iban As String
        Public Property banca As String
        Public Property adresa As String
        Public Property tip As String
        Public Property ascuns As Boolean
        Public Property activ As Boolean
        Public Property coduri As List(Of PartnerCodeWire)
    End Class

    Private NotInheritable Class ClsfOptionWire
        Public Property id_clsf As Integer
        Public Property clsf As String
        Public Property denumire As String
        Public Property ss As String
        Public Property id_unitate As Integer?
    End Class

    Private NotInheritable Class PartnersResponse
        Public Property partners As List(Of PartnerWire)
        Public Property clasificatii As List(Of ClsfOptionWire)
        Public Property bic As Dictionary(Of String, String)
        Public Property cf_unitate As String
    End Class

    Private NotInheritable Class PartnerAnafWire
        Public Property cui As String
        Public Property denumire As String
        Public Property adresa As String
    End Class

    Private NotInheritable Class PartnerSaveWire
        Public Property partner As PartnerWire
        Public Property coduri As List(Of PartnerCodeWire)
        Public Property coduri_sterse As List(Of Integer)
    End Class

    Private NotInheritable Class PartnerSaveResponse
        Public Property id_partener As Integer
    End Class

    ' ── Clasificatii ────────────────────────────────────────────────────────────

    Public Async Function GetClasificatiiAsync(ct As CancellationToken) As Task(Of ClasificatiiCatalog) _
        Implements INomenclatoareApi.GetClasificatiiAsync
        Try
            Dim respText As String = Await SendNomenclatoareAsync(HttpMethod.Get, $"{NomenclatoareRoot}/clasificatii",
                                                                  Nothing, "citirea clasificațiilor", ct).ConfigureAwait(False)
            Dim payload As ClsfTreeResponse = JsonSerializer.Deserialize(Of ClsfTreeResponse)(respText, _json)
            Dim result As New ClasificatiiCatalog()
            If payload Is Nothing Then Return result
            For Each w As ClsfItemWire In If(payload.items, New List(Of ClsfItemWire)())
                result.Items.Add(New Clasificatie() With {
                    .IdClsf = w.id_clsf,
                    .Capitol = If(w.capitol, String.Empty),
                    .Subcapitol = If(w.subcapitol, String.Empty),
                    .Articol = If(w.articol, String.Empty),
                    .Alineat = If(w.alineat, String.Empty),
                    .Denumire = If(w.denumire, String.Empty),
                    .Ss = If(w.ss, String.Empty),
                    .Clsf = If(w.clsf, String.Empty)})
            Next
            If payload.names IsNot Nothing Then
                CopyNames(payload.names.capitol, result.CapitolNames)
                CopyNames(payload.names.subcapitol, result.SubcapitolNames)
                CopyNames(payload.names.articol, result.ArticolNames)
                CopyNames(payload.names.ss, result.SsNames)
            End If
            Return result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetClasificatiiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetBugetClasificatieAsync(idClsf As Integer, an As Integer,
                                                    ct As CancellationToken) As Task(Of BugetClasificatie) _
        Implements INomenclatoareApi.GetBugetClasificatieAsync
        Try
            Dim url As String = $"{NomenclatoareRoot}/clasificatii/{idClsf.ToString(CultureInfo.InvariantCulture)}/buget" &
                                $"?an={an.ToString(CultureInfo.InvariantCulture)}"
            Dim respText As String = Await SendNomenclatoareAsync(HttpMethod.Get, url, Nothing,
                                                                  "citirea bugetului", ct).ConfigureAwait(False)
            Return ReadBudget(respText)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetBugetClasificatieAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveBugetClasificatieAsync(idClsf As Integer, an As Integer, budget As QuarterAmounts,
                                                     corrections As IReadOnlyList(Of RectificareBugetara),
                                                     deletedIds As IReadOnlyList(Of Integer),
                                                     ct As CancellationToken) As Task(Of BugetClasificatie) _
        Implements INomenclatoareApi.SaveBugetClasificatieAsync
        Try
            ArgumentNullException.ThrowIfNull(budget)
            ArgumentNullException.ThrowIfNull(corrections)
            Dim request As New BudgetRequest() With {
                .an = an,
                .budget = ToWire(budget),
                .corrections = corrections.Select(Function(c) New CorrectionWire() With {
                    .id = c.Id,
                    .document = c.Document,
                    .data = If(c.Data.HasValue, c.Data.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Nothing),
                    .trim1 = ToDouble(c.Amounts.Trim1), .trim2 = ToDouble(c.Amounts.Trim2),
                    .trim3 = ToDouble(c.Amounts.Trim3), .trim4 = ToDouble(c.Amounts.Trim4)}).ToList(),
                .deleted = If(deletedIds Is Nothing, New List(Of Integer)(), deletedIds.ToList())
            }
            Dim url As String = $"{NomenclatoareRoot}/clasificatii/{idClsf.ToString(CultureInfo.InvariantCulture)}/buget"
            Dim respText As String = Await SendNomenclatoareAsync(HttpMethod.Post, url, request,
                                                                  "salvarea bugetului", ct).ConfigureAwait(False)
            Return ReadBudget(respText)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveBugetClasificatieAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetClasificatiiNomenclatorAsync(an As Integer, ct As CancellationToken) As Task(Of ClasificatiiNomenclator) _
        Implements INomenclatoareApi.GetClasificatiiNomenclatorAsync
        Try
            Dim url As String = $"{NomenclatoareRoot}/clasificatii/nomenclator?an={an.ToString(CultureInfo.InvariantCulture)}"
            Dim respText As String = Await SendNomenclatoareAsync(HttpMethod.Get, url, Nothing,
                                                                  "citirea nomenclatoarelor", ct).ConfigureAwait(False)
            Dim payload As NomenclatorResponse = JsonSerializer.Deserialize(Of NomenclatorResponse)(respText, _json)
            Dim result As New ClasificatiiNomenclator()
            If payload Is Nothing Then Return result
            result.SectorSources.AddRange(ToCodeNames(payload.ss))
            If payload.f IsNot Nothing Then
                result.FunctionalCodes.AddRange(ToCodeNames(payload.f.codes))
                CopyNames(payload.f.groups, result.FunctionalGroups)
            End If
            If payload.e IsNot Nothing Then
                result.EconomicCodes.AddRange(ToCodeNames(payload.e.codes))
                CopyNames(payload.e.groups, result.EconomicGroups)
            End If
            Return result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetClasificatiiNomenclatorAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function AddClasificatiiAsync(an As Integer, sectorSources As IReadOnlyList(Of String),
                                               functionalCodes As IReadOnlyList(Of String),
                                               economicCodes As IReadOnlyList(Of String),
                                               ct As CancellationToken) As Task(Of ClasificatiiAddResult) _
        Implements INomenclatoareApi.AddClasificatiiAsync
        Try
            ArgumentNullException.ThrowIfNull(sectorSources)
            ArgumentNullException.ThrowIfNull(functionalCodes)
            ArgumentNullException.ThrowIfNull(economicCodes)
            Dim request As New AddClsfRequest() With {
                .an = an, .ss = sectorSources.ToList(), .f = functionalCodes.ToList(), .e = economicCodes.ToList()}
            Dim respText As String = Await SendNomenclatoareAsync(HttpMethod.Post, $"{NomenclatoareRoot}/clasificatii/adauga",
                                                                  request, "adăugarea clasificațiilor", ct).ConfigureAwait(False)
            Dim payload As AddClsfResponse = JsonSerializer.Deserialize(Of AddClsfResponse)(respText, _json)
            If payload Is Nothing Then Throw New ApiException("Serverul nu a întors rezultatul adăugării.")
            Return New ClasificatiiAddResult() With {
                .Requested = payload.requested, .Inserted = payload.inserted, .Existing = payload.existing}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.AddClasificatiiAsync", ex)
            Throw
        End Try
    End Function

    ' ── Parteneri ───────────────────────────────────────────────────────────────

    Public Async Function GetParteneriAsync(ct As CancellationToken) As Task(Of ParteneriCatalog) _
        Implements INomenclatoareApi.GetParteneriAsync
        Try
            Dim respText As String = Await SendNomenclatoareAsync(HttpMethod.Get, $"{NomenclatoareRoot}/parteneri",
                                                                  Nothing, "citirea partenerilor", ct).ConfigureAwait(False)
            Dim payload As PartnersResponse = JsonSerializer.Deserialize(Of PartnersResponse)(respText, _json)
            Dim result As New ParteneriCatalog()
            If payload Is Nothing Then Return result
            For Each w As PartnerWire In If(payload.partners, New List(Of PartnerWire)())
                Dim p As New Partener() With {
                    .IdPartener = w.id_partener, .IdUnitate = w.id_unitate, .Ss = If(w.ss, String.Empty),
                    .CodPartener = If(w.cod_partener, String.Empty), .Denumire = If(w.denumire, String.Empty),
                    .CodFiscal = If(w.cod_fiscal, String.Empty), .ContIban = If(w.cont_iban, String.Empty),
                    .Banca = If(w.banca, String.Empty), .Adresa = If(w.adresa, String.Empty),
                    .Tip = If(w.tip, String.Empty), .Ascuns = w.ascuns, .Activ = w.activ}
                For Each c As PartnerCodeWire In If(w.coduri, New List(Of PartnerCodeWire)())
                    p.Coduri.Add(New PartenerCod() With {
                        .Id = c.id, .IdClsf = c.id_clsf, .Clsf = If(c.clsf, String.Empty),
                        .DenumireClsf = If(c.denumire_clsf, String.Empty),
                        .ContBancar = If(c.cont_bancar, String.Empty), .CodAng = If(c.cod_ang, String.Empty),
                        .CodInd = If(c.cod_ind, String.Empty)})
                Next
                result.Partners.Add(p)
            Next
            For Each kv As KeyValuePair(Of String, String) In If(payload.bic, New Dictionary(Of String, String)())
                result.Bic(kv.Key) = If(kv.Value, String.Empty)
            Next
            result.CfUnitate = If(payload.cf_unitate, String.Empty)
            For Each o As ClsfOptionWire In If(payload.clasificatii, New List(Of ClsfOptionWire)())
                result.Clasificatii.Add(New ClasificatieOption() With {
                    .IdClsf = o.id_clsf, .Clsf = If(o.clsf, String.Empty), .Denumire = If(o.denumire, String.Empty),
                    .Ss = If(o.ss, String.Empty), .IdUnitate = o.id_unitate})
            Next
            Return result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetParteneriAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SavePartenerAsync(request As PartenerSaveRequest, ct As CancellationToken) As Task(Of Integer) _
        Implements INomenclatoareApi.SavePartenerAsync
        Try
            ArgumentNullException.ThrowIfNull(request)
            ArgumentNullException.ThrowIfNull(request.Partner)
            Dim p As Partener = request.Partner
            Dim wire As New PartnerSaveWire() With {
                .partner = New PartnerWire() With {
                    .id_partener = p.IdPartener, .id_unitate = p.IdUnitate, .ss = p.Ss,
                    .cod_partener = p.CodPartener, .denumire = p.Denumire, .cod_fiscal = p.CodFiscal,
                    .cont_iban = p.ContIban, .banca = p.Banca, .adresa = p.Adresa, .tip = p.Tip,
                    .ascuns = p.Ascuns},
                .coduri = request.Coduri.Select(Function(c) New PartnerCodeWire() With {
                    .id = c.Id, .id_clsf = c.IdClsf, .cont_bancar = c.ContBancar,
                    .cod_ang = c.CodAng, .cod_ind = c.CodInd}).ToList(),
                .coduri_sterse = request.CoduriSterse.ToList()
            }
            Dim respText As String = Await SendNomenclatoareAsync(HttpMethod.Post, $"{NomenclatoareRoot}/parteneri",
                                                                  wire, "salvarea partenerului", ct).ConfigureAwait(False)
            Dim payload As PartnerSaveResponse = JsonSerializer.Deserialize(Of PartnerSaveResponse)(respText, _json)
            If payload Is Nothing Then Throw New ApiException("Serverul nu a întors partenerul salvat.")
            Return payload.id_partener
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SavePartenerAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function DeletePartenerAsync(idPartener As Integer, ct As CancellationToken) As Task _
        Implements INomenclatoareApi.DeletePartenerAsync
        Try
            Dim url As String = $"{NomenclatoareRoot}/parteneri/{idPartener.ToString(CultureInfo.InvariantCulture)}"
            Await SendNomenclatoareAsync(HttpMethod.Delete, url, Nothing, "ștergerea partenerului", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.DeletePartenerAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetPartenerAnafAsync(codFiscal As String, ct As CancellationToken) As Task(Of PartenerAnaf) _
        Implements INomenclatoareApi.GetPartenerAnafAsync
        Try
            Dim url As String = $"{NomenclatoareRoot}/parteneri/anaf/{Uri.EscapeDataString(If(codFiscal, String.Empty).Trim())}"
            Dim respText As String = Await SendNomenclatoareAsync(HttpMethod.Get, url, Nothing,
                                                                  "căutarea codului fiscal la ANAF", ct).ConfigureAwait(False)
            Dim payload As PartnerAnafWire = JsonSerializer.Deserialize(Of PartnerAnafWire)(respText, _json)
            If payload Is Nothing Then Throw New ApiException("Serverul nu a întors datele de la ANAF.")
            Return New PartenerAnaf() With {
                .Cui = If(payload.cui, String.Empty), .Denumire = If(payload.denumire, String.Empty),
                .Adresa = If(payload.adresa, String.Empty)}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetPartenerAnafAsync", ex)
            Throw
        End Try
    End Function

    ' ── Helpers (reached only through the wrapped methods above) ────────────────

    ''' <summary>One bearer request; returns the body text of a 2xx, throws ApiException otherwise.</summary>
    Private Async Function SendNomenclatoareAsync(method As HttpMethod, url As String, body As Object,
                                                  action As String, ct As CancellationToken) As Task(Of String)
        EnsureConfigured()
        Using msg As New HttpRequestMessage(method, url)
            msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
            If body IsNot Nothing Then
                msg.Content = New StringContent(JsonSerializer.Serialize(body, body.GetType(), _json),
                                                Encoding.UTF8, "application/json")
            End If
            Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                If Not resp.IsSuccessStatusCode Then
                    Throw BuildApiException(respText, action, CInt(resp.StatusCode))
                End If
                Return respText
            End Using
        End Using
    End Function

    Private Shared Function ReadBudget(respText As String) As BugetClasificatie
        Dim payload As BudgetResponse = JsonSerializer.Deserialize(Of BudgetResponse)(respText, _json)
        Dim result As New BugetClasificatie()
        If payload Is Nothing Then Return result
        If payload.budget IsNot Nothing Then
            result.Budget = New QuarterAmounts() With {
                .Trim1 = ToDecimal(payload.budget.trim1), .Trim2 = ToDecimal(payload.budget.trim2),
                .Trim3 = ToDecimal(payload.budget.trim3), .Trim4 = ToDecimal(payload.budget.trim4)}
        End If
        For Each c As CorrectionWire In If(payload.corrections, New List(Of CorrectionWire)())
            Dim day As Date
            Dim hasDay As Boolean = Date.TryParseExact(If(c.data, String.Empty), "yyyy-MM-dd",
                                                       CultureInfo.InvariantCulture, DateTimeStyles.None, day)
            result.Corrections.Add(New RectificareBugetara() With {
                .Id = c.id,
                .Document = If(c.document, String.Empty),
                .Data = If(hasDay, day, CType(Nothing, Date?)),
                .Amounts = New QuarterAmounts() With {
                    .Trim1 = ToDecimal(c.trim1), .Trim2 = ToDecimal(c.trim2),
                    .Trim3 = ToDecimal(c.trim3), .Trim4 = ToDecimal(c.trim4)}})
        Next
        Return result
    End Function

    Private Shared Function ToWire(q As QuarterAmounts) As QuartersWire
        Return New QuartersWire() With {
            .trim1 = ToDouble(q.Trim1), .trim2 = ToDouble(q.Trim2),
            .trim3 = ToDouble(q.Trim3), .trim4 = ToDouble(q.Trim4)}
    End Function

    Private Shared Function ToDouble(value As Decimal?) As Double?
        If Not value.HasValue Then Return Nothing
        Return CDbl(value.Value)
    End Function

    Private Shared Function ToDecimal(value As Double?) As Decimal?
        If Not value.HasValue Then Return Nothing
        Return Math.Round(CDec(value.Value), 2)
    End Function

    Private Shared Function ToCodeNames(list As List(Of CodeNameWire)) As IEnumerable(Of CodeName)
        If list Is Nothing Then Return Enumerable.Empty(Of CodeName)()
        Return list.Select(Function(w) New CodeName() With {
            .Code = If(w.code, String.Empty), .Name = If(w.name, String.Empty)})
    End Function

    Private Shared Sub CopyNames(source As Dictionary(Of String, String), target As Dictionary(Of String, String))
        If source Is Nothing Then Return
        For Each kv As KeyValuePair(Of String, String) In source
            target(kv.Key) = If(kv.Value, String.Empty)
        Next
    End Sub

End Class
