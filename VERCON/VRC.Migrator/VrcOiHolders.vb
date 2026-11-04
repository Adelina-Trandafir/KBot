Imports System.Globalization

' Shared source lookup for both OI conversion and the annual Materiale destination.
Friend NotInheritable Class VrcOiHolders
    Private Sub New()
    End Sub

    Friend Shared Function SourceRow(k_data As VrcOiData, k_key As String) As Dictionary(Of String, Object)
        If String.IsNullOrEmpty(k_key) OrElse k_key = "0" Then Return Nothing
        Return k_data.Rows("Clienti").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "IDC") = k_key)
    End Function

    Friend Shared Function SourceName(k_data As VrcOiData, k_key As String) As String
        Dim k_row = SourceRow(k_data, k_key)
        If k_row Is Nothing Then Return "Deținător lipsă în baza sursă"
        Dim k_name = VrcOiData.Text(k_row, "Denumire").Trim()
        Return If(k_name.Length = 0, "Deținător fără nume în baza sursă", k_name)
    End Function

    Friend Shared Function IsMissing(k_data As VrcOiData, k_key As String) As Boolean
        Dim k_row = SourceRow(k_data, k_key)
        Return k_row Is Nothing OrElse String.IsNullOrWhiteSpace(VrcOiData.Text(k_row, "Denumire"))
    End Function

    Friend Shared Function DefaultTarget(k_data As VrcOiData, k_key As String) As String
        Return If(IsMissing(k_data, k_key), String.Empty, SourceName(k_data, k_key))
    End Function

    ' A legacy holder keeps its identity across annual copies of the same source gestiune.
    ' A rename uses the latest available name; unrelated folders/subunits are never joined by their local number.
    Friend Shared Function Automatic(k_catalog As IEnumerable(Of VrcOiData)) As List(Of VrcOiHolderAssociation)
        Dim k_result As New List(Of VrcOiHolderAssociation)()
        For Each k_series In k_catalog.GroupBy(Function(k_d) Lineage(k_d))
            Dim k_latest As New Dictionary(Of String, String)(StringComparer.Ordinal)
            For Each k_data In k_series.OrderBy(Function(k_d) k_d.Source.Year).ThenBy(Function(k_d) k_d.Source.Source.Path, StringComparer.OrdinalIgnoreCase)
                For Each k_row In k_data.Rows("Clienti")
                    Dim k_key = VrcOiData.Key(k_row, "IDC")
                    Dim k_name = VrcOiData.Text(k_row, "Denumire").Trim()
                    If k_key.Length > 0 AndAlso k_key <> "0" AndAlso k_name.Length > 0 Then k_latest(k_key) = k_name
                Next
            Next
            For Each k_data In k_series
                Dim k_keys = k_data.Rows("Clienti").Select(Function(k_r) VrcOiData.Key(k_r, "IDC")).Concat(k_data.Rows("Iesiri_H").Select(Function(k_r) VrcOiData.Key(k_r, "IDC"))).Distinct(StringComparer.Ordinal)
                For Each k_key In k_keys
                    Dim k_name As String = Nothing
                    k_latest.TryGetValue(k_key, k_name)
                    k_result.Add(New VrcOiHolderAssociation With {.SourcePath = k_data.Source.Source.Path, .SourceKey = k_key, .SourceName = SourceName(k_data, k_key), .TargetName = If(k_name, String.Empty)})
                Next
            Next
        Next
        Return k_result
    End Function

    Private Shared Function Lineage(k_data As VrcOiData) As String
        Return VrcOiData.Normal(k_data.Source.SubunitName) & "|" & VrcOiData.Normal(k_data.Source.GestiuneName) & "|" & VrcOiData.Normal(IO.Path.GetDirectoryName(k_data.Source.Source.Path))
    End Function

    Friend Shared Function Resolve(k_data As VrcOiData, k_key As String, k_targetName As String,
                                  k_catalog As IEnumerable(Of VrcOiData)) As Dictionary(Of String, Object)
        If String.IsNullOrWhiteSpace(k_targetName) Then Throw New InvalidOperationException("Deținătorul nu are un nume identificabil în seria bazelor încărcate. Completați primitorul în sursă și recitiți bazele.")
        Dim k_row = SourceRow(k_data, k_key)
        If IsMissing(k_data, k_key) Then
            k_row = k_catalog.Where(Function(k_d) Lineage(k_d) = Lineage(k_data)).
                OrderByDescending(Function(k_d) k_d.Source.Year).
                SelectMany(Function(k_d) k_d.Rows("Clienti")).
                FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "IDC") <> "0" AndAlso VrcOiData.Key(k_r, "IDC").Length > 0 AndAlso VrcOiData.Normal(VrcOiData.Text(k_r, "Denumire")) = VrcOiData.Normal(k_targetName))
            If k_row Is Nothing Then Throw New InvalidOperationException($"Deținătorul «{k_targetName}» nu are o înregistrare identificabilă în seria bazelor încărcate. Completați primitorul în sursă și recitiți bazele.")
        End If
        Dim k_copy = VrcOiData.CloneRow(k_row)
        k_copy("IDC") = If(k_key.Length = 0, Nothing, CType(k_key, Object))
        k_copy("Denumire") = k_targetName.Trim()
        Return k_copy
    End Function

    Friend Shared Function DocumentLabel(k_header As Dictionary(Of String, Object)) As String
        Dim k_number = VrcOiData.Text(k_header, "NumarBC").Trim()
        Dim k_day = VrcOiData.Value(k_header, "DataBC")
        Dim k_date = If(TypeOf k_day Is Date, DirectCast(k_day, Date).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), VrcOiData.Text(k_header, "DataBC"))
        Return $"bonul «{If(k_number.Length = 0, "număr lipsă", k_number)}», data {If(k_date.Length = 0, "lipsă", k_date)}"
    End Function

    Friend Shared Function DocumentNumber(k_header As Dictionary(Of String, Object)) As Decimal?
        Dim k_match = System.Text.RegularExpressions.Regex.Match(VrcOiData.Text(k_header, "NumarBC"), "[0-9]+(?:[.,][0-9]+)?")
        Dim k_number As Decimal
        If k_match.Success AndAlso Decimal.TryParse(k_match.Value.Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, k_number) Then Return k_number
        Return Nothing
    End Function

    Friend Shared Function DocumentDate(k_header As Dictionary(Of String, Object)) As Date?
        Dim k_value = VrcOiData.Value(k_header, "DataBC")
        If TypeOf k_value Is Date Then Return DirectCast(k_value, Date).Date
        Return Nothing
    End Function
End Class
