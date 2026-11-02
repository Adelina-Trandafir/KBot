Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 01EF-02 -- /api/efactura/contari/calcul: PYTHON/routes/efactura/primite_contari_routes.py. Wire names are exactly the server's (matching is case-sensitive).
' A boundary: log + rethrow (an ApiException keeps the server's Romanian text and reason).
Partial Public Class ApiClient

    Private NotInheritable Class ContariClsfWire
        Public Property IdClsf As Integer
        Public Property Clsf As String
        Public Property Denumire As String
        Public Property Forma As String
        Public Property AlePartenerului As Boolean
    End Class

    Private NotInheritable Class ContariSablonWire
        Public Property IdSablon As Integer
        Public Property Explicatie As String
        Public Property FelOP As String
        Public Property Jurnal As String
    End Class

    Private NotInheritable Class ContariRandWire
        Public Property IdPrimita As Integer
        Public Property NrFact As String
        Public Property NumarDocument As String
        Public Property DataDocument As String
        Public Property FelDocument As String
        Public Property Total As Decimal
        Public Property Salvat As Decimal
        Public Property Suma As Decimal
        Public Property Platit As Decimal
    End Class

    Private NotInheritable Class ContariPlataWire
        Public Property Valoare As Decimal
        Public Property ExplicatieOP As String
        Public Property ExplicatiePrea As Boolean
        Public Property DataMinima As String
        Public Property Activa As Boolean
    End Class

    Private NotInheritable Class ContariLinieWire
        Public Property Debit As String
        Public Property PartenerDebit As String
        Public Property Credit As String
        Public Property PartenerCredit As String
        Public Property PA As Boolean
        Public Property Ordonantare As Boolean
        Public Property Chelt As Boolean
    End Class

    Private NotInheritable Class ContariOperatieWire
        Public Property Titlu As String
        Public Property Faza As String
        Public Property IdPrimita As Integer?
        Public Property Suma As Decimal
        Public Property Linii As List(Of ContariLinieWire)
    End Class

    Private NotInheritable Class ContariAvertismentWire
        Public Property Cod As String
        Public Property Grav As String
        Public Property Text As String
    End Class

    Private NotInheritable Class ContariPartenerWire
        Public Property IdPartener As Integer
    End Class

    Private NotInheritable Class ContariFurnizorWire
        Public Property Denumire As String
        Public Property Partener As ContariPartenerWire
    End Class

    Private NotInheritable Class ContariCoduriWire
        Public Property CodAng As String
        Public Property CodInd As String
        Public Property ContBancar As String
    End Class

    Private NotInheritable Class ContariSubWire
        Public Property IdSubunitate As Integer
        Public Property Denumire As String
    End Class

    Private NotInheritable Class ContariNotaWire
        Public Property NumarNC As Integer
        Public Property DataNc As String
        Public Property Documente As Integer
    End Class

    Private NotInheritable Class ContariNoteWire
        Public Property An As Integer
        Public Property Urmatoare As Integer
        Public Property Existente As List(Of ContariNotaWire)
    End Class

    Private NotInheritable Class ContariRaspunsWire
        Public Property subunitate As ContariSubWire
        Public Property subunitati As List(Of ContariSubWire)
        Public Property note As ContariNoteWire
        Public Property poate_scrie As Boolean
        Public Property furnizor As ContariFurnizorWire
        Public Property clasificatii As List(Of ContariClsfWire)
        Public Property clasificatie As ContariClsfWire
        Public Property clasificatieSugerata As Integer?
        Public Property sabloaneEvidentiere As List(Of ContariSablonWire)
        Public Property sabloanePlata As List(Of ContariSablonWire)
        Public Property idSablonE As Integer?
        Public Property idSablonP As Integer?
        Public Property coduri As ContariCoduriWire
        Public Property contIban As String
        Public Property evidenta As List(Of ContariRandWire)
        Public Property plata As ContariPlataWire
        Public Property operatii As List(Of ContariOperatieWire)
        Public Property avertismente As List(Of ContariAvertismentWire)
    End Class

    Private Shared Function ToContariClsf(w As ContariClsfWire) As EFacturaContariClsf
        Return New EFacturaContariClsf() With {
            .IdClsf = w.IdClsf, .Clsf = If(w.Clsf, String.Empty), .Denumire = If(w.Denumire, String.Empty), .Forma = If(w.Forma, String.Empty),
            .AlePartenerului = w.AlePartenerului}
    End Function

    Private Shared Function ToContariSabloane(k_list As List(Of ContariSablonWire)) As List(Of EFacturaContariSablon)
        Dim k_result As New List(Of EFacturaContariSablon)()
        If k_list Is Nothing Then Return k_result
        For Each w As ContariSablonWire In k_list
            k_result.Add(New EFacturaContariSablon() With {
                .IdSablon = w.IdSablon, .Explicatie = If(w.Explicatie, String.Empty), .FelOP = If(w.FelOP, String.Empty), .Jurnal = If(w.Jurnal, String.Empty)})
        Next
        Return k_result
    End Function

    Private NotInheritable Class ContariSalvatWire
        Public Property IdNota As Integer
        Public Property NumarNC As Integer
        Public Property An As Integer
        Public Property Subunitate As String
        Public Property Operatii As List(Of Object)
    End Class

    Private Function ContariBody(k_request As EFacturaContariCerere, k_save As Boolean) As String
        Dim k_platit As New Dictionary(Of String, Decimal)()
        For Each k_pair As KeyValuePair(Of Integer, Decimal) In k_request.Platit
            k_platit(k_pair.Key.ToString(CultureInfo.InvariantCulture)) = k_pair.Value
        Next
        Dim k_fields As New Dictionary(Of String, Object) From {
            {"ids", k_request.Ids}, {"idClsf", k_request.IdClsf}, {"idSablonE", k_request.IdSablonE}, {"idSablonP", k_request.IdSablonP},
            {"platit", k_platit}, {"an", k_request.An}, {"idSubunitate", k_request.IdSubunitate}}
        If k_save Then
            k_fields("confirmari") = k_request.Confirmari
            k_fields("numarNC") = k_request.NumarNC
        End If
        Return JsonSerializer.Serialize(k_fields, _json)
    End Function

    Public Async Function SaveContariAsync(k_request As EFacturaContariCerere, ct As CancellationToken) _
        As Task(Of EFacturaContariSalvat) Implements IEFacturaApi.SaveContariAsync
        Try
            ArgumentNullException.ThrowIfNull(k_request)
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Post, "/api/efactura/contari/salveaza", ContariBody(k_request, True),
                                                           "salvarea contării facturilor", ct).ConfigureAwait(False)
            Dim wire As ContariSalvatWire = JsonSerializer.Deserialize(Of ContariSalvatWire)(k_text, _json)
            If wire Is Nothing Then Throw New ApiException("Serverul nu a confirmat salvarea contării.")
            Return New EFacturaContariSalvat() With {
                .IdNota = wire.IdNota, .NumarNC = wire.NumarNC, .An = wire.An, .Subunitate = If(wire.Subunitate, String.Empty),
                .NrOperatii = If(wire.Operatii Is Nothing, 0, wire.Operatii.Count)}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveContariAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function CalculateContariAsync(k_request As EFacturaContariCerere, ct As CancellationToken) _
        As Task(Of EFacturaContariRaspuns) Implements IEFacturaApi.CalculateContariAsync
        Try
            ArgumentNullException.ThrowIfNull(k_request)
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Post, "/api/efactura/contari/calcul", ContariBody(k_request, False),
                                                           "calculul contării facturilor", ct).ConfigureAwait(False)
            Dim wire As ContariRaspunsWire = JsonSerializer.Deserialize(Of ContariRaspunsWire)(k_text, _json)
            If wire Is Nothing Then Throw New ApiException("Serverul nu a trimis calculul contării.")
            Dim k_result As New EFacturaContariRaspuns() With {
                .PoateScrie = wire.poate_scrie, .ClasificatieSugerata = wire.clasificatieSugerata, .IdSablonE = wire.idSablonE, .IdSablonP = wire.idSablonP,
                .ContIban = If(wire.contIban, String.Empty)}
            If wire.furnizor IsNot Nothing Then
                k_result.FurnizorDenumire = If(wire.furnizor.Denumire, String.Empty)
                k_result.PartenerGasit = wire.furnizor.Partener IsNot Nothing
            End If
            If wire.clasificatii IsNot Nothing Then
                For Each w As ContariClsfWire In wire.clasificatii
                    k_result.Clasificatii.Add(ToContariClsf(w))
                Next
            End If
            If wire.clasificatie IsNot Nothing Then k_result.Clasificatie = ToContariClsf(wire.clasificatie)
            If wire.subunitate IsNot Nothing Then k_result.Subunitate = New EFacturaContariSubunitate() With {.IdSubunitate = wire.subunitate.IdSubunitate, .Denumire = If(wire.subunitate.Denumire, String.Empty)}
            If wire.subunitati IsNot Nothing Then
                For Each w As ContariSubWire In wire.subunitati
                    k_result.Subunitati.Add(New EFacturaContariSubunitate() With {.IdSubunitate = w.IdSubunitate, .Denumire = If(w.Denumire, String.Empty)})
                Next
            End If
            If wire.note IsNot Nothing Then
                k_result.NoteAn = wire.note.An
                k_result.NotaUrmatoare = wire.note.Urmatoare
                If wire.note.Existente IsNot Nothing Then
                    For Each w As ContariNotaWire In wire.note.Existente
                        k_result.Note.Add(New EFacturaContariNota() With {.NumarNC = w.NumarNC, .DataNc = ParseDay(w.DataNc), .Documente = w.Documente})
                    Next
                End If
            End If
            k_result.SabloaneEvidentiere.AddRange(ToContariSabloane(wire.sabloaneEvidentiere))
            k_result.SabloanePlata.AddRange(ToContariSabloane(wire.sabloanePlata))
            If wire.coduri IsNot Nothing Then
                k_result.CodAng = If(wire.coduri.CodAng, String.Empty)
                k_result.CodInd = If(wire.coduri.CodInd, String.Empty)
            End If
            If wire.evidenta IsNot Nothing Then
                For Each w As ContariRandWire In wire.evidenta
                    k_result.Evidenta.Add(New EFacturaContariRand() With {
                        .IdPrimita = w.IdPrimita, .NrFact = If(w.NrFact, String.Empty), .NumarDocument = If(w.NumarDocument, String.Empty),
                        .DataDocument = ParseDay(w.DataDocument), .FelDocument = If(w.FelDocument, String.Empty),
                        .Total = w.Total, .Salvat = w.Salvat, .Suma = w.Suma, .Platit = w.Platit})
                Next
            End If
            If wire.plata IsNot Nothing Then
                k_result.Plata = New EFacturaContariPlata() With {
                    .Valoare = wire.plata.Valoare, .ExplicatieOP = If(wire.plata.ExplicatieOP, String.Empty), .ExplicatiePrea = wire.plata.ExplicatiePrea,
                    .DataMinima = ParseDay(wire.plata.DataMinima), .Activa = wire.plata.Activa}
            End If
            If wire.operatii IsNot Nothing Then
                For Each w As ContariOperatieWire In wire.operatii
                    Dim k_op As New EFacturaContariOperatie() With {.Titlu = If(w.Titlu, String.Empty), .Faza = If(w.Faza, String.Empty), .IdPrimita = w.IdPrimita, .Suma = w.Suma}
                    If w.Linii IsNot Nothing Then
                        For Each l As ContariLinieWire In w.Linii
                            k_op.Linii.Add(New EFacturaContariLinie() With {
                                .Debit = If(l.Debit, String.Empty), .PartenerDebit = If(l.PartenerDebit, String.Empty), .Credit = If(l.Credit, String.Empty),
                                .PartenerCredit = If(l.PartenerCredit, String.Empty), .PA = l.PA, .Ordonantare = l.Ordonantare, .Chelt = l.Chelt})
                        Next
                    End If
                    k_result.Operatii.Add(k_op)
                Next
            End If
            If wire.avertismente IsNot Nothing Then
                For Each w As ContariAvertismentWire In wire.avertismente
                    k_result.Avertismente.Add(New EFacturaContariAvertisment() With {.Cod = If(w.Cod, String.Empty), .Grav = If(w.Grav, String.Empty), .Text = If(w.Text, String.Empty)})
                Next
            End If
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.CalculateContariAsync", ex)
            Throw
        End Try
    End Function

End Class
