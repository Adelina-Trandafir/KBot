Option Strict On
Imports System.Collections.Generic

' Mapper: the tabular ListaAngajamente result -> List(Of Angajament).
' Pure layer (no JSON / no dependencies): receives the already-parsed rows
' (column -> value) produced by ForexeRunner.RunJobAsync in
' JobResult.Tables("ListaAngajamente"). Column keys are resolved through
' IAngajamenteColumnMap (see ForexeColumnMap.vb) so a FOREXE rename is fixed
' in exactly one place.
Public NotInheritable Class AngajamentMapper

    Private Sub New()
        ' Static-only helper; no instances.
    End Sub

    ' Backward-compatible overload: uses the static default column map.
    Public Shared Function FromListaAngajamenteResult(
        rows As List(Of Dictionary(Of String, String))) As List(Of Angajament)

        Return FromListaAngajamenteResult(rows, New DefaultAngajamenteColumnMap())
    End Function

    ' Maps scraped rows -> domain Angajament using the supplied column map.
    '   - Key lookup is case-insensitive and trimmed.
    '   - Every other scraped column (Nr_crt, Angajament_legal, Col_9, anything FOREXE
    '     adds later) is ignored.
    '   - Rows whose Cod is empty/whitespace are skipped (mirror VBA: IsEmpty(dRow("Cod"))).
    '   - If a REQUIRED source key is absent from a row, throws (no swallow) with a message
    '     listing the keys that WERE present, so a FOREXE rename fails loudly and precisely.
    Public Shared Function FromListaAngajamenteResult(
        rows As List(Of Dictionary(Of String, String)),
        map As IAngajamenteColumnMap) As List(Of Angajament)

        If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))
        If map Is Nothing Then Throw New ArgumentNullException(NameOf(map))

        Dim result As New List(Of Angajament)()

        For Each row As Dictionary(Of String, String) In rows
            If row Is Nothing Then Continue For

            ' Case-insensitive, trimmed snapshot for lookups and diagnostics.
            Dim ci As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For Each kvp As KeyValuePair(Of String, String) In row
                If kvp.Key IsNot Nothing Then ci(kvp.Key.Trim()) = kvp.Value
            Next

            Dim cod As String = RequireKey(ci, map.CodKey)
            ' Skip empty-Cod rows (blank / footer / total).
            If String.IsNullOrWhiteSpace(cod) Then Continue For

            Dim descriere As String = RequireKey(ci, map.DescriereKey)
            Dim stare As String = RequireKey(ci, map.StareKey)

            result.Add(New Angajament() With {
                .CodAngajament = cod.Trim(),
                .Descriere = descriere,
                .Stare = stare
            })
        Next

        Return result
    End Function

    ' Returns the value for the mapped key; throws precisely if the key is missing.
    Private Shared Function RequireKey(
        row As Dictionary(Of String, String), key As String) As String

        If key Is Nothing Then Throw New ArgumentNullException(NameOf(key))

        Dim value As String = Nothing
        If Not row.TryGetValue(key.Trim(), value) Then
            Throw New KeyNotFoundException(
                $"FOREXE column '{key}' is missing from the scraped row. " &
                $"Keys present: {String.Join(", ", row.Keys)}. " &
                "The FOREXE app likely renamed the column — update IAngajamenteColumnMap.")
        End If

        Return If(value, String.Empty)
    End Function
End Class
