Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 00EF-43 -- /api/efactura/primite/access/...: PYTHON/routes/efactura/primite_access_routes.py. Wire names are exactly the server's.
' Every method is a boundary: log + rethrow (an ApiException keeps the server's Romanian text and reason).
Partial Public Class ApiClient

    Private NotInheritable Class AccessLineWire
        Public Property NrLinie As String
        Public Property Denumire As String
        Public Property Explicatie As String
        Public Property Unit As String
        Public Property Cant As Decimal
        Public Property Pret As Decimal
        Public Property Valoare As Decimal
    End Class

    Private NotInheritable Class AccessNoteWire
        Public Property Nota As String
        Public Property DataAdaugare As String
    End Class

    Private NotInheritable Class AccessInvoiceMessageWire
        Public Property IdSol As String
        Public Property IdMesajAnaf As String
        Public Property Mesaj As String
        Public Property DataMesaj As String
        Public Property DataAdaugare As String
    End Class

    Private NotInheritable Class AccessOperationWire
        Public Property IdOperatiePrimita As Integer
        Public Property IdOperatie As Integer
        Public Property IdClsfAcc As Integer
        Public Property IdUnitate As Integer
        Public Property Sursa As String
        Public Property Suma As Decimal
        Public Property DataAdaugare As String
    End Class

    Private NotInheritable Class AccessInvoiceWire
        Public Property NrFact As String
        Public Property DataFact As String
        Public Property DataScad As String
        Public Property CotaTVA As Decimal
        Public Property TVA As Decimal
        Public Property Valoare As Decimal
        Public Property Total As Decimal
        Public Property CUI As String
        Public Property DenumireP As String
        Public Property Adresa As String
        Public Property Atasament As String
        Public Property Tip As String
        Public Property Semn As Integer
        Public Property Ref As String
        Public Property RefIdSol As String
        Public Property RefNrFact As String
        Public Property DataAdaugare As String
        Public Property linii As List(Of AccessLineWire)
        Public Property note As List(Of AccessNoteWire)
        Public Property mesaje As List(Of AccessInvoiceMessageWire)
        Public Property operatii As List(Of AccessOperationWire)
    End Class

    Private NotInheritable Class AccessMessageWire
        Public Property IdSol As String
        Public Property IdIncarcare As String
        Public Property CifEmitent As String
        Public Property DataMesaj As String
        Public Property CuiUnitate As String
        Public Property Nou As Integer
        Public Property NumeFisier As String
        Public Property Xml As String
        Public Property DataAdaugare As String
        Public Property facturi As List(Of AccessInvoiceWire)
    End Class

    ' Slice 00EF-44: the PC's account of the run, for the server's journal only. Lower-case names = the server's keys.
    Private NotInheritable Class AccessContextWire
        Public Property an As Integer
        Public Property fisier As String
        Public Property cui As String
        Public Property lot As Integer
        Public Property loturi As Integer
        Public Property cerute As Integer
        Public Property citite_lot As Integer
        Public Property citite As Integer
        Public Property acces_total As Integer
        Public Property necitite_total As Integer
        Public Property necitite As List(Of String)
    End Class

    Private NotInheritable Class AccessPreiaBodyWire
        Public Property mesaje As List(Of AccessMessageWire)
        Public Property context As AccessContextWire
    End Class

    Private NotInheritable Class AccessReferenceWire
        Public Property IdSol As String
        Public Property NrFact As String
        Public Property RefIdSol As String
        Public Property RefNrFact As String
    End Class

    Private NotInheritable Class AccessFinalBodyWire
        Public Property id_primite As List(Of Integer)
        Public Property referinte As List(Of AccessReferenceWire)
        Public Property context As AccessContextWire
    End Class

    Private NotInheritable Class AccessExportBodyWire
        Public Property an As Integer
        Public Property deja As List(Of String)
        Public Property limita As Integer
    End Class

    Private NotInheritable Class AccessErrorWire
        Public Property id_sol As String
        Public Property motiv As String
    End Class

    Private NotInheritable Class AccessPreiaAnswerWire
        Public Property mesaje_noi As Integer
        Public Property mesaje_sarite As Integer
        Public Property facturi_noi As Integer
        Public Property facturi_sarite As Integer
        Public Property linii As Integer
        Public Property note As Integer
        Public Property mesaje_factura As Integer
        Public Property operatii As Integer
        Public Property operatii_sarite As Integer
        Public Property id_primite As List(Of Integer)
        Public Property probleme As List(Of String)
        Public Property erori As List(Of AccessErrorWire)
    End Class

    Private NotInheritable Class AccessLinkAnswerWire
        Public Property citite As Integer
        Public Property legate As Integer
        Public Property fara_furnizor As Integer
        Public Property fara_receptie As Integer
        Public Property nesigure As Integer
        Public Property mai_multe_clasificatii As Integer
        Public Property conflicte As Integer
        Public Property omisa As String
    End Class

    Private NotInheritable Class AccessFinalAnswerWire
        Public Property referinte_setate As Integer
        Public Property legare As AccessLinkAnswerWire
    End Class

    Private NotInheritable Class AccessExportAnswerWire
        Public Property mesaje As List(Of AccessMessageWire)
        Public Property ramase As Integer
        Public Property total_lipsa As Integer
    End Class

    Private NotInheritable Class AccessCuiWire
        Public Property cui As String
    End Class

    Private Const AccessDateTimeFormat As String = "yyyy-MM-dd'T'HH:mm:ss"

    Private Shared Function WireDate(k_value As Date?) As String
        Return If(k_value.HasValue, k_value.Value.ToString(AccessDateTimeFormat, CultureInfo.InvariantCulture), Nothing)
    End Function

    Private Shared Function ParseWireDate(k_value As String) As Date?
        Dim k_parsed As Date
        If Not String.IsNullOrWhiteSpace(k_value) AndAlso
           Date.TryParse(k_value, CultureInfo.InvariantCulture, DateTimeStyles.None, k_parsed) Then Return k_parsed
        Return Nothing
    End Function

    Public Async Function GetAccessCuiAsync(ct As CancellationToken) As Task(Of String) _
        Implements IEFacturaApi.GetAccessCuiAsync
        Try
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Get, "/api/efactura/primite/access/stare", Nothing,
                                                           "citirea codului fiscal al unității", ct).ConfigureAwait(False)
            Dim k_wire As AccessCuiWire = JsonSerializer.Deserialize(Of AccessCuiWire)(k_text, _json)
            If k_wire Is Nothing OrElse String.IsNullOrWhiteSpace(k_wire.cui) Then Throw New ApiException("Serverul nu a trimis codul fiscal al unității.")
            Return k_wire.cui
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetAccessCuiAsync", ex)
            Throw
        End Try
    End Function

    Private Shared Function ContextToWire(k_context As AccessPreluareContext) As AccessContextWire
        If k_context Is Nothing Then Return Nothing
        Return New AccessContextWire() With {
            .an = k_context.An, .fisier = k_context.Fisier, .cui = k_context.Cui, .lot = k_context.Lot, .loturi = k_context.Loturi,
            .cerute = k_context.Cerute, .citite_lot = k_context.CititeInLot, .citite = k_context.Citite, .acces_total = k_context.AccesTotal,
            .necitite_total = k_context.NecititeTotal, .necitite = k_context.Necitite.ToList()}
    End Function

    Public Async Function PreiaAccessAsync(k_messages As IReadOnlyList(Of AccessEfMessage), k_context As AccessPreluareContext, ct As CancellationToken) _
        As Task(Of EFacturaAccessPreluare) Implements IEFacturaApi.PreiaAccessAsync
        Try
            ArgumentNullException.ThrowIfNull(k_messages)
            Dim k_body As New AccessPreiaBodyWire() With {.mesaje = k_messages.Select(AddressOf ToWire).ToList(), .context = ContextToWire(k_context)}
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Post, "/api/efactura/primite/access/preia",
                                                           JsonSerializer.Serialize(k_body, _json),
                                                           "preluarea facturilor din Access", ct).ConfigureAwait(False)
            Dim k_wire As AccessPreiaAnswerWire = JsonSerializer.Deserialize(Of AccessPreiaAnswerWire)(k_text, _json)
            If k_wire Is Nothing Then Throw New ApiException("Serverul nu a trimis rezultatul preluării.")
            Dim k_result As New EFacturaAccessPreluare() With {
                .MesajeNoi = k_wire.mesaje_noi, .MesajeSarite = k_wire.mesaje_sarite, .FacturiNoi = k_wire.facturi_noi,
                .FacturiSarite = k_wire.facturi_sarite, .Linii = k_wire.linii, .Note = k_wire.note,
                .MesajeFactura = k_wire.mesaje_factura, .Operatii = k_wire.operatii, .OperatiiSarite = k_wire.operatii_sarite}
            If k_wire.id_primite IsNot Nothing Then k_result.IdPrimite.AddRange(k_wire.id_primite)
            If k_wire.probleme IsNot Nothing Then k_result.Probleme.AddRange(k_wire.probleme)
            If k_wire.erori IsNot Nothing Then
                For Each k_e As AccessErrorWire In k_wire.erori
                    k_result.Erori.Add($"{k_e.id_sol}: {k_e.motiv}")
                Next
            End If
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.PreiaAccessAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function FinalizeazaAccessAsync(k_idPrimite As IReadOnlyList(Of Integer), k_references As IReadOnlyList(Of AccessEfReference),
                                                 k_context As AccessPreluareContext, ct As CancellationToken) As Task(Of EFacturaAccessFinal) _
        Implements IEFacturaApi.FinalizeazaAccessAsync
        Try
            ArgumentNullException.ThrowIfNull(k_idPrimite)
            ArgumentNullException.ThrowIfNull(k_references)
            Dim k_body As New AccessFinalBodyWire() With {
                .id_primite = k_idPrimite.ToList(), .context = ContextToWire(k_context),
                .referinte = k_references.Select(Function(k_r) New AccessReferenceWire() With {
                    .IdSol = k_r.IdSol, .NrFact = k_r.NrFact, .RefIdSol = k_r.RefIdSol, .RefNrFact = k_r.RefNrFact}).ToList()}
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Post, "/api/efactura/primite/access/finalizeaza",
                                                           JsonSerializer.Serialize(k_body, _json),
                                                           "încheierea preluării din Access", ct).ConfigureAwait(False)
            Dim k_wire As AccessFinalAnswerWire = JsonSerializer.Deserialize(Of AccessFinalAnswerWire)(k_text, _json)
            If k_wire Is Nothing Then Throw New ApiException("Serverul nu a trimis rezultatul încheierii preluării.")
            Dim k_result As New EFacturaAccessFinal() With {.ReferinteSetate = k_wire.referinte_setate}
            If k_wire.legare IsNot Nothing Then
                k_result.Citite = k_wire.legare.citite
                k_result.Legate = k_wire.legare.legate
                k_result.FaraFurnizor = k_wire.legare.fara_furnizor
                k_result.FaraReceptie = k_wire.legare.fara_receptie
                k_result.Nesigure = k_wire.legare.nesigure
                k_result.MaiMulteClasificatii = k_wire.legare.mai_multe_clasificatii
                k_result.Conflicte = k_wire.legare.conflicte
                k_result.Omisa = If(k_wire.legare.omisa, String.Empty)
            End If
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.FinalizeazaAccessAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function ExportaAccessAsync(k_year As Integer, k_have As IEnumerable(Of String), k_limit As Integer, ct As CancellationToken) _
        As Task(Of EFacturaAccessExport) Implements IEFacturaApi.ExportaAccessAsync
        Try
            ArgumentNullException.ThrowIfNull(k_have)
            Dim k_body As New AccessExportBodyWire() With {.an = k_year, .deja = k_have.ToList(), .limita = k_limit}
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Post, "/api/efactura/primite/access/exporta",
                                                           JsonSerializer.Serialize(k_body, _json),
                                                           "citirea facturilor pentru Access", ct).ConfigureAwait(False)
            Dim k_wire As AccessExportAnswerWire = JsonSerializer.Deserialize(Of AccessExportAnswerWire)(k_text, _json)
            If k_wire Is Nothing Then Throw New ApiException("Serverul nu a trimis facturile.")
            Dim k_result As New EFacturaAccessExport() With {.Ramase = k_wire.ramase, .TotalLipsa = k_wire.total_lipsa}
            If k_wire.mesaje IsNot Nothing Then
                For Each k_m As AccessMessageWire In k_wire.mesaje
                    k_result.Mesaje.Add(FromWire(k_m))
                Next
            End If
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.ExportaAccessAsync", ex)
            Throw
        End Try
    End Function

    ' ---- DTO <-> wire (reached only through the wrapped methods above) -----------------------------------------------------

    Private Shared Function ToWire(k_m As AccessEfMessage) As AccessMessageWire
        Return New AccessMessageWire() With {
            .IdSol = k_m.IdSol, .IdIncarcare = k_m.IdIncarcare, .CifEmitent = k_m.CifEmitent, .DataMesaj = WireDate(k_m.DataMesaj),
            .CuiUnitate = k_m.CuiUnitate, .Nou = If(k_m.Nou, 1, 0), .NumeFisier = k_m.NumeFisier,
            .Xml = If(k_m.Xml Is Nothing OrElse k_m.Xml.Length = 0, Nothing, Convert.ToBase64String(k_m.Xml)),
            .DataAdaugare = WireDate(k_m.Added),
            .facturi = k_m.Invoices.Select(Function(k_i) New AccessInvoiceWire() With {
                .NrFact = k_i.NrFact, .DataFact = WireDate(k_i.DataFact), .DataScad = WireDate(k_i.DataScad), .CotaTVA = k_i.CotaTva,
                .TVA = k_i.Tva, .Valoare = k_i.Valoare, .Total = k_i.Total, .CUI = k_i.Cui, .DenumireP = k_i.DenumireP,
                .Adresa = k_i.Adresa, .Atasament = k_i.Atasament, .Tip = k_i.Tip, .Semn = k_i.Semn, .Ref = k_i.Ref,
                .RefIdSol = k_i.RefIdSol, .RefNrFact = k_i.RefNrFact, .DataAdaugare = WireDate(k_i.Added),
                .linii = k_i.Lines.Select(Function(k_l) New AccessLineWire() With {
                    .NrLinie = k_l.NrLinie, .Denumire = k_l.Denumire, .Explicatie = k_l.Explicatie, .Unit = k_l.Unit,
                    .Cant = k_l.Cant, .Pret = k_l.Pret, .Valoare = k_l.Valoare}).ToList(),
                .note = k_i.Notes.Select(Function(k_n) New AccessNoteWire() With {.Nota = k_n.Nota, .DataAdaugare = WireDate(k_n.Added)}).ToList(),
                .mesaje = k_i.Messages.Select(Function(k_x) New AccessInvoiceMessageWire() With {
                    .IdSol = k_x.IdSol, .IdMesajAnaf = k_x.IdMesajAnaf, .Mesaj = k_x.Mesaj, .DataMesaj = WireDate(k_x.DataMesaj),
                    .DataAdaugare = WireDate(k_x.Added)}).ToList(),
                .operatii = k_i.Operations.Select(Function(k_o) New AccessOperationWire() With {
                    .IdOperatiePrimita = k_o.IdOperatiePrimita, .IdOperatie = k_o.IdOperatie, .IdClsfAcc = k_o.IdClsfAcc,
                    .IdUnitate = k_o.IdUnitate, .Sursa = k_o.Sursa, .Suma = k_o.Suma, .DataAdaugare = WireDate(k_o.Added)}).ToList()
            }).ToList()}
    End Function

    Private Shared Function FromWire(k_w As AccessMessageWire) As AccessEfMessage
        Dim k_m As New AccessEfMessage() With {
            .IdSol = If(k_w.IdSol, String.Empty), .IdIncarcare = If(k_w.IdIncarcare, String.Empty), .CifEmitent = If(k_w.CifEmitent, String.Empty),
            .DataMesaj = ParseWireDate(k_w.DataMesaj), .CuiUnitate = If(k_w.CuiUnitate, String.Empty), .Nou = k_w.Nou <> 0,
            .NumeFisier = If(k_w.NumeFisier, String.Empty), .Added = ParseWireDate(k_w.DataAdaugare),
            .Xml = If(String.IsNullOrEmpty(k_w.Xml), Nothing, Convert.FromBase64String(k_w.Xml))}
        If k_w.facturi Is Nothing Then Return k_m
        For Each k_i As AccessInvoiceWire In k_w.facturi
            Dim k_inv As New AccessEfInvoice() With {
                .NrFact = If(k_i.NrFact, String.Empty), .DataFact = ParseWireDate(k_i.DataFact), .DataScad = ParseWireDate(k_i.DataScad),
                .CotaTva = k_i.CotaTVA, .Tva = k_i.TVA, .Valoare = k_i.Valoare, .Total = k_i.Total, .Cui = If(k_i.CUI, String.Empty),
                .DenumireP = If(k_i.DenumireP, String.Empty), .Adresa = If(k_i.Adresa, String.Empty), .Atasament = If(k_i.Atasament, String.Empty),
                .Tip = If(String.IsNullOrEmpty(k_i.Tip), "FC", k_i.Tip), .Semn = If(k_i.Semn < 0, -1, 1), .Ref = If(k_i.Ref, String.Empty),
                .RefIdSol = If(k_i.RefIdSol, String.Empty), .RefNrFact = If(k_i.RefNrFact, String.Empty), .Added = ParseWireDate(k_i.DataAdaugare)}
            If k_i.linii IsNot Nothing Then
                For Each k_l As AccessLineWire In k_i.linii
                    k_inv.Lines.Add(New AccessEfLine() With {
                        .NrLinie = If(k_l.NrLinie, String.Empty), .Denumire = If(k_l.Denumire, String.Empty), .Explicatie = If(k_l.Explicatie, String.Empty),
                        .Unit = If(k_l.Unit, String.Empty), .Cant = k_l.Cant, .Pret = k_l.Pret, .Valoare = k_l.Valoare})
                Next
            End If
            If k_i.note IsNot Nothing Then
                For Each k_n As AccessNoteWire In k_i.note
                    k_inv.Notes.Add(New AccessEfNote() With {.Nota = If(k_n.Nota, String.Empty), .Added = ParseWireDate(k_n.DataAdaugare)})
                Next
            End If
            If k_i.mesaje IsNot Nothing Then
                For Each k_x As AccessInvoiceMessageWire In k_i.mesaje
                    k_inv.Messages.Add(New AccessEfInvoiceMessage() With {
                        .IdSol = If(k_x.IdSol, String.Empty), .IdMesajAnaf = If(k_x.IdMesajAnaf, String.Empty), .Mesaj = If(k_x.Mesaj, String.Empty),
                        .DataMesaj = ParseWireDate(k_x.DataMesaj), .Added = ParseWireDate(k_x.DataAdaugare)})
                Next
            End If
            k_m.Invoices.Add(k_inv)
        Next
        Return k_m
    End Function

End Class
