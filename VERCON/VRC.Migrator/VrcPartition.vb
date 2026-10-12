Imports System.Globalization

''' <summary>
''' A year file can hold both Alimente and Materiale. The split is decided by the TYPE OF THE ACCOUNT (Masini.TIP): "A" is
''' Alimente; "M", empty or anything else is Materiale (operator, 12.10.2026). Every row is placed by its account: Produse by
''' Produse.IdMasina, ProduseP through Produse, NIR / bon de consum by their IdMasina (or, when it is empty, by their lines),
''' lines by their document. The food lists and what only they use are Alimente. Tax, commissions and clients are shared and go
''' to every part. Pure logic over rows already read; it reads nothing itself.
''' </summary>
Friend NotInheritable Class VrcPartition

    Friend Const Alimente As String = "A"
    Friend Const Materiale As String = "M"
    ''' <summary>Returned by <see cref="PartOf"/> for a row that belongs to every part (shared nomenclators).</summary>
    Friend Const Shared_ As String = "*"

    Private ReadOnly _masina As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Private ReadOnly _produs As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Private ReadOnly _produsP As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Private ReadOnly _nir As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Private ReadOnly _bon As New Dictionary(Of String, String)(StringComparer.Ordinal)

    ' counts for the plan (documents by part, and the cases that had no account)
    Friend Property AccountsAlimente As Long
    Friend Property AccountsMateriale As Long
    Friend Property NirAlimente As Long
    Friend Property NirMateriale As Long
    Friend Property BonAlimente As Long
    Friend Property BonMateriale As Long
    ''' <summary>Documents with no usable account and no line to tell: they follow the default (Materiale).</summary>
    Friend Property Unclassified As Long
    ''' <summary>Documents whose lines belong to both parts: cannot be placed; blocking.</summary>
    Friend Property Conflicts As Long

    Friend Shared Function PartOfAccountType(k_type As Object) As String
        Dim k_text As String = Convert.ToString(k_type, CultureInfo.InvariantCulture)
        Return If(k_text IsNot Nothing AndAlso String.Equals(k_text.Trim(), "A", StringComparison.OrdinalIgnoreCase), Alimente, Materiale)
    End Function

    ''' <summary>Builds the maps from the rows of Masini, Produse, ProduseP, NIR, Intrari, Iesiri_H and Iesiri (a missing table gives no rows).</summary>
    Friend Shared Function Build(k_read As Func(Of String, List(Of Dictionary(Of String, Object)))) As VrcPartition
        Dim k_p As New VrcPartition()
        For Each k_row In k_read("Masini")
            Dim k_part As String = PartOfAccountType(Value(k_row, "TIP"))
            k_p._masina(Key(k_row, "IdMasina")) = k_part
            If k_part = Alimente Then k_p.AccountsAlimente += 1 Else k_p.AccountsMateriale += 1
        Next
        For Each k_row In k_read("Produse")
            Dim k_part As String = Nothing
            If k_p._masina.TryGetValue(Key(k_row, "IdMasina"), k_part) Then k_p._produs(Key(k_row, "ID")) = k_part Else k_p._produs(Key(k_row, "ID")) = Materiale
        Next
        For Each k_row In k_read("ProduseP")
            Dim k_part As String = Nothing
            k_p._produsP(Key(k_row, "IdProdus")) = If(k_p._produs.TryGetValue(Key(k_row, "ID"), k_part), k_part, Materiale)
        Next
        k_p.PlaceDocuments(k_read("NIR"), k_read("Intrari"), "IdIntrare", k_p._nir, True)
        k_p.PlaceDocuments(k_read("Iesiri_H"), k_read("Iesiri"), "IdBonConsum", k_p._bon, False)
        Return k_p
    End Function

    ''' <summary>Each header goes to its account's part; without one, to the part its lines agree on; lines of both parts are a conflict.</summary>
    Private Sub PlaceDocuments(k_headers As List(Of Dictionary(Of String, Object)), k_lines As List(Of Dictionary(Of String, Object)),
                               k_key As String, k_target As Dictionary(Of String, String), k_isNir As Boolean)
        Dim k_byLines As New Dictionary(Of String, HashSet(Of String))(StringComparer.Ordinal)
        For Each k_line In k_lines
            Dim k_part As String = Nothing
            If Not _produsP.TryGetValue(Key(k_line, "IdProdus"), k_part) Then Continue For
            Dim k_id As String = Key(k_line, k_key)
            If k_id.Length = 0 Then Continue For
            Dim k_set As HashSet(Of String) = Nothing
            If Not k_byLines.TryGetValue(k_id, k_set) Then
                k_set = New HashSet(Of String)()
                k_byLines(k_id) = k_set
            End If
            k_set.Add(k_part)
        Next
        For Each k_header In k_headers
            Dim k_id As String = Key(k_header, k_key)
            If k_id.Length = 0 Then Continue For
            Dim k_part As String = Nothing
            Dim k_set As HashSet(Of String) = Nothing
            k_byLines.TryGetValue(k_id, k_set)
            If _masina.TryGetValue(Key(k_header, "IdMasina"), k_part) Then
                ' the account decides; lines of the other part would be a conflict
                If k_set IsNot Nothing AndAlso k_set.Any(Function(k_s) k_s <> k_part) Then Conflicts += 1
            ElseIf k_set IsNot Nothing AndAlso k_set.Count = 1 Then
                k_part = k_set.First()
            ElseIf k_set IsNot Nothing AndAlso k_set.Count > 1 Then
                Conflicts += 1
                k_part = Materiale
            Else
                k_part = Materiale
                Unclassified += 1
            End If
            k_target(k_id) = k_part
            If k_isNir Then
                If k_part = Alimente Then NirAlimente += 1 Else NirMateriale += 1
            Else
                If k_part = Alimente Then BonAlimente += 1 Else BonMateriale += 1
            End If
        Next
    End Sub

    ''' <summary>The part of a row of <paramref name="k_table"/>: "A", "M" or "*" (shared by every part).</summary>
    Friend Function PartOf(k_table As String, k_row As Dictionary(Of String, Object)) As String
        Dim k_part As String = Nothing
        Select Case k_table
            Case "Masini"
                Return PartOfAccountType(Value(k_row, "TIP"))
            Case "Produse"
                Return If(_produs.TryGetValue(Key(k_row, "ID"), k_part), k_part, Materiale)
            Case "ProduseP"
                Return If(_produsP.TryGetValue(Key(k_row, "IdProdus"), k_part), k_part, Materiale)
            Case "NIR", "Intrari"
                Return If(_nir.TryGetValue(Key(k_row, "IdIntrare"), k_part), k_part, Materiale)
            Case "IntrariStorno"
                If _nir.TryGetValue(Key(k_row, "IdIntrare"), k_part) Then Return k_part
                Return If(_produsP.TryGetValue(Key(k_row, "IdProdus"), k_part), k_part, Materiale)
            Case "Iesiri_H", "Iesiri", "IesiriRetur"
                Return If(_bon.TryGetValue(Key(k_row, "IdBonConsum"), k_part), k_part, Materiale)
            Case "LABC", "ListaAlimente", "LegBCLA", "MicDejun", "Dejun", "Cina", "FeluriMancare", "Retetar"
                Return Alimente
            Case Else
                Return Shared_
        End Select
    End Function

    Private Shared Function Value(k_row As Dictionary(Of String, Object), k_name As String) As Object
        Dim k_v As Object = Nothing
        k_row.TryGetValue(k_name, k_v)
        Return k_v
    End Function

    Private Shared Function Key(k_row As Dictionary(Of String, Object), k_name As String) As String
        Return VrcConvert.KeyText(Value(k_row, k_name))
    End Function

End Class
