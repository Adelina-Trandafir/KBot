Imports System.Globalization
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports KBot.Common

' SLICE-VC10-01. A read-only snapshot; the source files are never changed.
Public NotInheritable Class VrcOiData
    Public Property Source As VrcPlanSource
    Public Property Tables As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.OrdinalIgnoreCase)

    Public Function Rows(k_table As String) As List(Of Dictionary(Of String, Object))
        Dim k_rows As List(Of Dictionary(Of String, Object)) = Nothing
        If Tables.TryGetValue(k_table, k_rows) Then Return k_rows
        Return New List(Of Dictionary(Of String, Object))()
    End Function

    Public Shared Function Load(k_source As VrcPlanSource) As VrcOiData
        EnsureFresh(k_source)
        Dim k_data As New VrcOiData With {.Source = k_source}
        Dim k_provider As String = String.Empty
        Using k_cn = VrcAccess.OpenReadOnly(k_source.Source.Path, k_provider)
            For Each k_table In VrcImportMap.Tables()
                Dim k_rows = VrcUsageReader.AllRows(k_cn, k_source.Profile, k_table.Name)
                For Each k_row In k_rows
                    For Each k_column In k_table.Columns
                        If Not k_row.ContainsKey(k_column.Name) Then k_row(k_column.Name) = Nothing
                    Next
                Next
                k_data.Tables(k_table.Name) = k_rows
            Next
            For Each k_table In {"Furnizori", "NR"}
                k_data.Tables(k_table) = VrcUsageReader.AllRows(k_cn, k_source.Profile, k_table)
            Next
        End Using
        EnsureFresh(k_source)
        If k_source.Source.Kind = VrcFileKind.Year Then
            Dim k_partition = VrcPartition.Build(Function(k_t) k_data.Rows(k_t))
            For Each k_table In k_data.Tables.Keys.ToList()
                k_data.Tables(k_table) = k_data.Rows(k_table).Where(Function(k_r) k_partition.PartOf(k_table, k_r) = k_source.Part OrElse k_partition.PartOf(k_table, k_r) = VrcPartition.Shared_).ToList()
            Next
        End If
        Return k_data
    End Function

    Public Shared Sub EnsureFresh(k_source As VrcPlanSource)
        Using k_stream As New FileStream(k_source.Source.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Dim k_hash = Convert.ToHexString(SHA256.HashData(k_stream))
            If k_hash <> k_source.Profile.HashBefore Then Throw New InvalidOperationException($"{k_source.Profile.Label}: baza s-a schimbat. Recitiți bazele și refaceți separarea OI.")
        End Using
    End Sub

    Public Shared Function LoadMany(k_sources As IEnumerable(Of VrcPlanSource), k_problems As IList(Of String)) As List(Of VrcOiData)
        Dim k_result As New List(Of VrcOiData)()
        For Each k_source In k_sources
            Try
                k_result.Add(Load(k_source))
            Catch ex As Exception When TypeOf ex Is IO.IOException OrElse TypeOf ex Is UnauthorizedAccessException OrElse TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is System.Data.OleDb.OleDbException OrElse TypeOf ex Is System.Data.DataException OrElse TypeOf ex Is System.Security.SecurityException
                GlobalErrorLog.Write("VrcOiData.LoadMany", ex)
                k_problems.Add($"{k_source.Profile.Label}: {ex.Message}")
            End Try
        Next
        Return k_result
    End Function

    Public Shared Function Value(k_row As Dictionary(Of String, Object), k_field As String) As Object
        Dim k_value As Object = Nothing
        k_row.TryGetValue(k_field, k_value)
        Return k_value
    End Function

    Public Shared Function Key(k_row As Dictionary(Of String, Object), k_field As String) As String
        Return VrcConvert.KeyText(Value(k_row, k_field))
    End Function

    Public Shared Function Text(k_row As Dictionary(Of String, Object), k_field As String) As String
        Return Convert.ToString(Value(k_row, k_field), CultureInfo.InvariantCulture)
    End Function

    Public Shared Function Number(k_row As Dictionary(Of String, Object), k_field As String) As Decimal
        Dim k_number = VrcConvert.ToDecimal(Value(k_row, k_field))
        If Not k_number.HasValue Then Throw New InvalidOperationException($"Lipsește {If(k_field = "Cant", "cantitatea", If(k_field = "PUTVA", "prețul unitar", k_field))} sau valoarea nu este numerică.")
        Return Decimal.Round(k_number.Value, 4)
    End Function

    Public Shared Function Normal(k_text As String) As String
        Return If(k_text, String.Empty).Trim().Normalize(NormalizationForm.FormC).ToUpper(CultureInfo.GetCultureInfo("ro-RO"))
    End Function

    Public Shared Function CloneRow(k_row As Dictionary(Of String, Object)) As Dictionary(Of String, Object)
        Return New Dictionary(Of String, Object)(k_row, StringComparer.OrdinalIgnoreCase)
    End Function

    Public Shared Function Fingerprint(k_tables As Dictionary(Of String, List(Of Dictionary(Of String, Object)))) As String
        Dim k_text As New StringBuilder()
        For Each k_table In k_tables.OrderBy(Function(k_t) k_t.Key, StringComparer.Ordinal)
            k_text.AppendLine(k_table.Key)
            For Each k_row In k_table.Value
                For Each k_cell In k_row.OrderBy(Function(k_c) k_c.Key, StringComparer.Ordinal)
                    Dim k_value = If(TypeOf k_cell.Value Is Date, DirectCast(k_cell.Value, Date).ToString("O", CultureInfo.InvariantCulture), Convert.ToString(k_cell.Value, CultureInfo.InvariantCulture))
                    k_text.Append(k_cell.Key.Length).Append(":"c).Append(k_cell.Key).Append(If(k_cell.Value Is Nothing, -1, k_value.Length)).Append(":"c).Append(k_value)
                Next
                k_text.AppendLine()
            Next
        Next
        Return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(k_text.ToString())))
    End Function
End Class

Public NotInheritable Class VrcOiChoice
    Public Property Data As VrcOiData
    Public Property AccountKey As String = String.Empty
    Public Property AccountName As String = String.Empty
    Public Property UseAccountName As String = String.Empty
    Public Property TargetName As String = String.Empty
    Public Property TargetType As String = String.Empty
End Class

Public NotInheritable Class VrcOiHolderAssociation
    Public Property SourcePath As String = String.Empty
    Public Property SourceKey As String = String.Empty
    Public Property SourceName As String = String.Empty
    Public Property TargetName As String = String.Empty

    Public ReadOnly Property Identity As String
        Get
            Return SourcePath & "|" & SourceKey
        End Get
    End Property
End Class

Public NotInheritable Class VrcOiConversion
    Public Property BaseHash As String = String.Empty
    Public Property Residual As New Dictionary(Of String, VrcOiData)(StringComparer.OrdinalIgnoreCase)
    Public Property Generated As New List(Of VrcPlanSource)()
    Public Property Blockers As New List(Of String)()
    Public Property Report As String = String.Empty

    Public Sub Apply(k_plan As VrcPlan)
        If k_plan.Hash() <> BaseHash Then
            k_plan.Blockers.Add("Planul de bază s-a schimbat. Reîncărcați conturile și generați din nou separarea OI.")
            Return
        End If
        If Blockers.Count > 0 Then
            k_plan.Blockers.AddRange(Blockers)
            Return
        End If
        For Each k_source In k_plan.Sources
            Dim k_data As VrcOiData = Nothing
            If k_source.GestiuneType = "MATERIALE" AndAlso Residual.TryGetValue(k_source.Source.Path, k_data) Then
                k_source.PreparedRows = k_data.Tables
                k_source.PreparedHash = VrcOiData.Fingerprint(k_data.Tables)
            End If
        Next
        k_plan.Sources.AddRange(Generated)
        k_plan.Evaluate()
        k_plan.Warnings.Add("Separare pe conturile alese: consultați raportul și tipul fiecărei gestiuni noi. Numai destinațiile OI transformă BC în BP + BC legat.")
    End Sub
End Class
