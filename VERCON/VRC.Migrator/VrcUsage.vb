Imports System.Data.OleDb
Imports System.Globalization
Imports KBot.Common

''' <summary>ProduseP rows against the documents that use them (decided 12.10.2026: only rows with a use are migrated).</summary>
Public NotInheritable Class VrcProductCheck

    Public Property Total As Long
    ''' <summary>Rows whose IdProdus is used by Intrari, Iesiri, IesiriRetur or an OI/MF table (these are migrated).</summary>
    Public Property Used As Long
    ''' <summary>Rows with a negative PUTVA that are used: warning. An unused one is simply not migrated.</summary>
    Public Property NegativeUsed As Long
    Public Property NegativeUnused As Long
    ''' <summary>Tables searched for uses (the ones present with an IdProdus column).</summary>
    Public Property SearchedIn As New List(Of String)()

End Class

''' <summary>Signs of quantity and price on the lines of one document table (Intrari, Iesiri, IesiriRetur).</summary>
Public NotInheritable Class VrcLineSignCheck

    Public Property Table As String = String.Empty
    Public Property Lines As Long
    ''' <summary>Negative price, non-negative quantity: migrated with the price positive and the quantity negative (warning).</summary>
    Public Property NegativePriceOnly As Long
    ''' <summary>Negative quantity, non-negative price: kept as it is (warning).</summary>
    Public Property NegativeQuantityOnly As Long
    ''' <summary>BOTH quantity and price negative: blocking.</summary>
    Public Property BothNegative As Long

End Class

''' <summary>Alimente / Materiale split of a year file by the type of the account (Masini.TIP): what each part would receive.</summary>
Public NotInheritable Class VrcSplitCheck

    Public Property AccountsAlimente As Long
    Public Property AccountsMateriale As Long
    Public Property NirAlimente As Long
    Public Property NirMateriale As Long
    Public Property BonAlimente As Long
    Public Property BonMateriale As Long
    Public Property Unclassified As Long
    Public Property Conflicts As Long
    Public Property Lists As Long

    Public ReadOnly Property HasAlimente As Boolean
        Get
            Return AccountsAlimente > 0 OrElse Lists > 0 OrElse NirAlimente > 0 OrElse BonAlimente > 0
        End Get
    End Property

    Public ReadOnly Property HasMateriale As Boolean
        Get
            Return AccountsMateriale > 0 OrElse NirMateriale > 0 OrElse BonMateriale > 0 OrElse Not HasAlimente
        End Get
    End Property

End Class

''' <summary>Counts for the profile report (VC05-01): ProduseP uses and line signs. Read-only; values are only counted.</summary>
Friend NotInheritable Class VrcUsageReader

    Private Shared ReadOnly ProductUsers As String() = {"Intrari", "Iesiri", "IesiriRetur", "OI_Casare", "OI_Predare", "OI_Produse", "OI_Transfer"}

    Private Sub New()
    End Sub

    Private Shared Function TableOf(k_profile As VrcFileProfile, k_name As String) As VrcTableProfile
        Return k_profile.Tables.FirstOrDefault(Function(k_t) String.Equals(k_t.Name, k_name, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Shared Function HasColumn(k_table As VrcTableProfile, k_column As String) As Boolean
        Return k_table IsNot Nothing AndAlso k_table.Columns.Contains(k_column, StringComparer.OrdinalIgnoreCase)
    End Function

    ''' <summary>Fills <see cref="VrcFileProfile.ProductCheck"/> and <see cref="VrcFileProfile.LineSigns"/>. A failure is recorded in Config, not thrown.</summary>
    Friend Shared Sub Read(k_cn As OleDbConnection, k_profile As VrcFileProfile)
        ReadProducts(k_cn, k_profile)
        ReadSigns(k_cn, k_profile)
        ReadSplit(k_cn, k_profile)
        ReadMinNirYear(k_cn, k_profile)
    End Sub

    ''' <summary>All rows of a table of the open file, every column; a table the file lacks gives no rows.</summary>
    Friend Shared Function AllRows(k_cn As OleDbConnection, k_profile As VrcFileProfile, k_table As String) As List(Of Dictionary(Of String, Object))
        Dim k_rows As New List(Of Dictionary(Of String, Object))()
        If TableOf(k_profile, k_table) Is Nothing Then Return k_rows
        Using k_cmd As New OleDbCommand($"SELECT * FROM [{k_table}]", k_cn)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    Dim k_row As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
                    For k_i As Integer = 0 To k_reader.FieldCount - 1
                        k_row(k_reader.GetName(k_i)) = If(k_reader.IsDBNull(k_i), Nothing, k_reader.GetValue(k_i))
                    Next
                    k_rows.Add(k_row)
                End While
            End Using
        End Using
        Return k_rows
    End Function

    Private Shared Sub ReadSplit(k_cn As OleDbConnection, k_profile As VrcFileProfile)
        Try
            If k_profile.Kind <> VrcFileKind.Year Then Return
            Dim k_partition = VrcPartition.Build(Function(k_t) AllRows(k_cn, k_profile, k_t))
            Dim k_lists = TableOf(k_profile, "LABC")
            k_profile.Split = New VrcSplitCheck() With {
                .AccountsAlimente = k_partition.AccountsAlimente, .AccountsMateriale = k_partition.AccountsMateriale,
                .NirAlimente = k_partition.NirAlimente, .NirMateriale = k_partition.NirMateriale,
                .BonAlimente = k_partition.BonAlimente, .BonMateriale = k_partition.BonMateriale,
                .Unclassified = k_partition.Unclassified, .Conflicts = k_partition.Conflicts,
                .Lists = If(k_lists Is Nothing, 0L, Math.Max(0L, k_lists.Rows))}
        Catch ex As Exception
            GlobalErrorLog.Write("VrcUsageReader.ReadSplit", ex)
            k_profile.Config("conturi.eroare") = ex.Message
        End Try
    End Sub

    Private Shared Sub ReadMinNirYear(k_cn As OleDbConnection, k_profile As VrcFileProfile)
        Try
            If Not HasColumn(TableOf(k_profile, "NIR"), "Data") Then Return
            Using k_cmd As New OleDbCommand("SELECT Min([Data]) FROM [NIR]", k_cn)
                Dim k_value = k_cmd.ExecuteScalar()
                If k_value IsNot Nothing AndAlso k_value IsNot DBNull.Value Then k_profile.MinNirYear = Convert.ToDateTime(k_value, CultureInfo.InvariantCulture).Year
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("VrcUsageReader.ReadMinNirYear", ex)
            k_profile.Config("an_nir.eroare") = ex.Message
        End Try
    End Sub

    Private Shared Sub ReadProducts(k_cn As OleDbConnection, k_profile As VrcFileProfile)
        Try
            Dim k_products = TableOf(k_profile, "ProduseP")
            If Not (HasColumn(k_products, "IdProdus") AndAlso HasColumn(k_products, "PUTVA")) Then Return
            Dim k_used As New HashSet(Of Long)()
            Dim k_check As New VrcProductCheck()
            For Each k_name In ProductUsers
                If Not HasColumn(TableOf(k_profile, k_name), "IdProdus") Then Continue For
                k_check.SearchedIn.Add(k_name)
                Using k_cmd As New OleDbCommand($"SELECT DISTINCT [IdProdus] FROM [{k_name}] WHERE [IdProdus] IS NOT NULL", k_cn)
                    Using k_reader = k_cmd.ExecuteReader()
                        While k_reader.Read()
                            k_used.Add(Convert.ToInt64(k_reader.GetValue(0), CultureInfo.InvariantCulture))
                        End While
                    End Using
                End Using
            Next
            Using k_cmd As New OleDbCommand("SELECT [IdProdus], [PUTVA] FROM [ProduseP]", k_cn)
                Using k_reader = k_cmd.ExecuteReader()
                    While k_reader.Read()
                        If k_reader.IsDBNull(0) Then Continue While
                        k_check.Total += 1
                        Dim k_isUsed As Boolean = k_used.Contains(Convert.ToInt64(k_reader.GetValue(0), CultureInfo.InvariantCulture))
                        If k_isUsed Then k_check.Used += 1
                        If Not k_reader.IsDBNull(1) AndAlso Convert.ToDouble(k_reader.GetValue(1), CultureInfo.InvariantCulture) < 0 Then
                            If k_isUsed Then k_check.NegativeUsed += 1 Else k_check.NegativeUnused += 1
                        End If
                    End While
                End Using
            End Using
            k_profile.ProductCheck = k_check
        Catch ex As Exception
            GlobalErrorLog.Write("VrcUsageReader.ReadProducts", ex)
            k_profile.Config("produse.eroare") = ex.Message
        End Try
    End Sub

    Private Shared Sub ReadSigns(k_cn As OleDbConnection, k_profile As VrcFileProfile)
        Try
            For Each k_name In {"Intrari", "Iesiri", "IesiriRetur"}
                Dim k_table = TableOf(k_profile, k_name)
                If k_table Is Nothing OrElse Not HasColumn(k_table, "Cant") Then Continue For
                Dim k_prices = k_table.Columns.Where(Function(k_c) k_c.Equals("Pret", StringComparison.OrdinalIgnoreCase) OrElse
                                                                    k_c.Equals("PUTVA", StringComparison.OrdinalIgnoreCase) OrElse
                                                                    k_c.Equals("PU", StringComparison.OrdinalIgnoreCase)).ToList()
                If k_prices.Count = 0 Then Continue For
                Dim k_check As New VrcLineSignCheck() With {.Table = k_name}
                Dim k_sql As String = $"SELECT [Cant], {String.Join(", ", k_prices.Select(Function(k_p) $"[{k_p}]"))} FROM [{k_name}]"
                Using k_cmd As New OleDbCommand(k_sql, k_cn)
                    Using k_reader = k_cmd.ExecuteReader()
                        While k_reader.Read()
                            k_check.Lines += 1
                            Dim k_quantityNegative As Boolean = Not k_reader.IsDBNull(0) AndAlso Convert.ToDouble(k_reader.GetValue(0), CultureInfo.InvariantCulture) < 0
                            Dim k_priceNegative As Boolean = False
                            For k_i As Integer = 1 To k_prices.Count
                                If Not k_reader.IsDBNull(k_i) AndAlso Convert.ToDouble(k_reader.GetValue(k_i), CultureInfo.InvariantCulture) < 0 Then k_priceNegative = True
                            Next
                            If k_quantityNegative AndAlso k_priceNegative Then
                                k_check.BothNegative += 1
                            ElseIf k_priceNegative Then
                                k_check.NegativePriceOnly += 1
                            ElseIf k_quantityNegative Then
                                k_check.NegativeQuantityOnly += 1
                            End If
                        End While
                    End Using
                End Using
                k_profile.LineSigns.Add(k_check)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("VrcUsageReader.ReadSigns", ex)
            k_profile.Config("semne.eroare") = ex.Message
        End Try
    End Sub

End Class
