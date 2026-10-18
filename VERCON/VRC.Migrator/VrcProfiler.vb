Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Security.Cryptography
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports KBot.Common

''' <summary>
''' SLICE-VC05-01: reads one Access file READ-ONLY and describes it: tables/columns against the expected schema (absent
''' is not NULL), row counts, value ranges and decimals of the numeric columns, header-vs-lines counts (N01/N02), the
''' configuration values (Setari, NR, a few UNIT fields), linked tables, a Ver table if one exists (never created) and the
''' SHA-256 before and after (a file that changed while it was read is reported as not stable). Nothing is written to the
''' file. Personal data of the unit (names of people) is not read.
''' </summary>
Public NotInheritable Class VrcProfiler

    Private Sub New()
    End Sub

    Private Shared ReadOnly NumericClasses As String() = {"VALOARE-INT", "COTA", "NUMAR", "INT"}
    ''' <summary>Tables of the Access schema mechanism / infrastructure: not business data, never profiled.</summary>
    Private Shared ReadOnly MechanismTable As New Regex("^(_|SQL|RunOnce|Trimis|MSys|~)", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
    Private Const Tolerance As Double = 0.000000001

    Private Shared _expected As Dictionary(Of String, List(Of (Name As String, FieldClass As String))) = Nothing

    ''' <summary>Expected tables and columns, from the schema embedded at build time (VERCON/tools/expected.js).</summary>
    Public Shared Function Expected() As Dictionary(Of String, List(Of (Name As String, FieldClass As String)))
        Try
            If _expected IsNot Nothing Then Return _expected
            Dim k_result As New Dictionary(Of String, List(Of (Name As String, FieldClass As String)))(StringComparer.OrdinalIgnoreCase)
            Using k_stream As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VRC.Migrator.expected_schema.json")
                If k_stream Is Nothing Then Throw New InvalidOperationException("Schema așteptată lipsește din executabil.")
                Using k_doc As JsonDocument = JsonDocument.Parse(k_stream)
                    For Each k_table In k_doc.RootElement.GetProperty("tables").EnumerateObject()
                        Dim k_fields As New List(Of (Name As String, FieldClass As String))()
                        For Each k_field In k_table.Value.GetProperty("fields").EnumerateArray()
                            k_fields.Add((k_field.GetProperty("name").GetString(), k_field.GetProperty("class").GetString()))
                        Next
                        k_result(k_table.Name) = k_fields
                    Next
                End Using
            End Using
            _expected = k_result
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("VrcProfiler.Expected", ex)
            Throw
        End Try
    End Function

    ''' <summary>Profiles one file. A read problem is returned in <see cref="VrcFileProfile.ReadError"/>, not thrown.</summary>
    Public Shared Function Profile(k_source As VrcSourceFile, k_progress As Action(Of String)) As VrcFileProfile
        Dim k_path As String = k_source.Path
        Dim k_profile As New VrcFileProfile() With {.FileName = Path.GetFileName(k_path), .FullPath = k_path, .Kind = k_source.Kind, .Year = k_source.Year}
        Try
            k_progress?.Invoke($"{k_profile.FileName}: hash…")
            k_profile.SizeBytes = New FileInfo(k_path).Length
            k_profile.HashBefore = HashOf(k_path)
            Dim k_provider As String = String.Empty
            Using k_cn As OleDbConnection = VrcAccess.OpenReadOnly(k_path, k_provider)
                k_profile.Provider = k_provider
                ReadTables(k_cn, k_profile, k_progress)
                ReadConfig(k_cn, k_profile)
                ReadUnitName(k_cn, k_profile)
                ReadHeaderChecks(k_cn, k_profile)
                VrcUsageReader.Read(k_cn, k_profile)
            End Using
            k_profile.HashAfter = HashOf(k_path)
            k_profile.Stable = String.Equals(k_profile.HashBefore, k_profile.HashAfter, StringComparison.Ordinal)
        Catch ex As Exception
            GlobalErrorLog.Write("VrcProfiler.Profile", ex)
            k_profile.ReadError = ex.Message
        End Try
        Return k_profile
    End Function

    ' ---- tables ----------------------------------------------------------------------------------------------------
    Private Shared Sub ReadTables(k_cn As OleDbConnection, k_profile As VrcFileProfile, k_progress As Action(Of String))
        Dim k_present As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)   ' name -> TABLE or LINK
        Using k_schema As DataTable = k_cn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, New Object() {Nothing, Nothing, Nothing, Nothing})
            For Each k_row As DataRow In k_schema.Rows
                Dim k_type As String = Convert.ToString(k_row("TABLE_TYPE"), CultureInfo.InvariantCulture)
                Dim k_name As String = Convert.ToString(k_row("TABLE_NAME"), CultureInfo.InvariantCulture)
                If k_type = "TABLE" OrElse k_type = "LINK" OrElse k_type = "PASS-THROUGH" Then k_present(k_name) = k_type
            Next
        End Using
        k_profile.HasVerTable = k_present.ContainsKey("Ver")
        For Each k_pair In k_present
            If k_pair.Value <> "TABLE" Then k_profile.LinkedTables.Add(k_pair.Key)
        Next

        Dim k_expected = Expected()
        For Each k_entry In k_expected.OrderBy(Function(k_e) k_e.Key, StringComparer.OrdinalIgnoreCase)
            If Not k_present.ContainsKey(k_entry.Key) Then
                k_profile.MissingTables.Add(k_entry.Key)
                Continue For
            End If
            k_progress?.Invoke($"{k_profile.FileName}: {k_entry.Key}…")
            k_profile.Tables.Add(ProfileTable(k_cn, k_entry.Key, k_entry.Value))
        Next
        For Each k_name In k_present.Keys.OrderBy(Function(k_n) k_n, StringComparer.OrdinalIgnoreCase)
            If k_expected.ContainsKey(k_name) OrElse String.Equals(k_name, "Ver", StringComparison.OrdinalIgnoreCase) Then Continue For
            If Not MechanismTable.IsMatch(k_name) Then k_profile.OtherTables.Add(k_name)
        Next
    End Sub

    Private Shared Function ProfileTable(k_cn As OleDbConnection, k_name As String,
                                         k_fields As List(Of (Name As String, FieldClass As String))) As VrcTableProfile
        Dim k_table As New VrcTableProfile() With {.Name = k_name}
        Try
            Using k_cols As DataTable = k_cn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, New Object() {Nothing, Nothing, k_name, Nothing})
                For Each k_row As DataRow In k_cols.Rows
                    k_table.Columns.Add(Convert.ToString(k_row("COLUMN_NAME"), CultureInfo.InvariantCulture))
                Next
            End Using
            Dim k_have As New HashSet(Of String)(k_table.Columns, StringComparer.OrdinalIgnoreCase)
            For Each k_field In k_fields
                If Not k_have.Contains(k_field.Name) Then k_table.Absent.Add(k_field.Name)
            Next
            Dim k_known As New HashSet(Of String)(k_fields.Select(Function(k_f) k_f.Name), StringComparer.OrdinalIgnoreCase)
            For Each k_column In k_table.Columns
                If Not k_known.Contains(k_column) Then k_table.Extra.Add(k_column)
            Next
            Using k_cmd As New OleDbCommand($"SELECT COUNT(*) FROM [{k_name}]", k_cn)
                k_table.Rows = Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
            End Using
            If k_table.Rows = 0 Then Return k_table

            Dim k_measured = k_fields.Where(Function(k_f) NumericClasses.Contains(k_f.FieldClass) AndAlso k_have.Contains(k_f.Name)).ToList()
            If k_measured.Count = 0 Then Return k_table
            Dim k_stats = k_measured.Select(Function(k_f) New VrcFieldStat() With {.Name = k_f.Name, .FieldClass = k_f.FieldClass}).ToList()
            Dim k_sql As String = "SELECT " & String.Join(", ", k_measured.Select(Function(k_f) $"[{k_f.Name}]")) & $" FROM [{k_name}]"
            Using k_cmd As New OleDbCommand(k_sql, k_cn)
                Using k_reader = k_cmd.ExecuteReader()
                    While k_reader.Read()
                        For k_i As Integer = 0 To k_stats.Count - 1
                            If k_reader.IsDBNull(k_i) Then
                                k_stats(k_i).Nulls += 1
                            Else
                                Observe(k_stats(k_i), Convert.ToDouble(k_reader.GetValue(k_i), CultureInfo.InvariantCulture))
                            End If
                        Next
                    End While
                End Using
            End Using
            k_table.Fields.AddRange(k_stats)
        Catch ex As Exception
            GlobalErrorLog.Write("VrcProfiler.ProfileTable", ex)
            k_table.Problem = ex.Message
        End Try
        Return k_table
    End Function

    Private Shared Sub Observe(k_stat As VrcFieldStat, k_value As Double)
        k_stat.NonNull += 1
        If k_value = 0 Then k_stat.Zeros += 1
        If k_value < 0 Then k_stat.Negatives += 1
        If Double.IsNaN(k_stat.Minimum) OrElse k_value < k_stat.Minimum Then k_stat.Minimum = k_value
        If Double.IsNaN(k_stat.Maximum) OrElse k_value > k_stat.Maximum Then k_stat.Maximum = k_value
        Dim k_decimals As Integer = DecimalsNeeded(k_value)
        k_stat.ByDecimals(k_decimals) += 1
        If k_decimals > k_stat.MaxDecimals Then k_stat.MaxDecimals = k_decimals
    End Sub

    ''' <summary>Fewest decimals (0..4) that represent the value within float noise; 5 when more than 4 are needed.</summary>
    Friend Shared Function DecimalsNeeded(k_value As Double) As Integer
        For k_scale As Integer = 0 To 4
            If Math.Abs(k_value - Math.Round(k_value, k_scale, MidpointRounding.AwayFromZero)) < Tolerance Then Return k_scale
        Next
        Return 5
    End Function

    ' ---- configuration ---------------------------------------------------------------------------------------------
    Private Shared Sub ReadConfig(k_cn As OleDbConnection, k_profile As VrcFileProfile)
        Try
            If TableExists(k_profile, "Setari") Then
                Using k_cmd As New OleDbCommand("SELECT S, V FROM Setari", k_cn)
                    Using k_reader = k_cmd.ExecuteReader()
                        While k_reader.Read()
                            If Not k_reader.IsDBNull(0) Then
                                k_profile.Config("Setari." & Convert.ToString(k_reader.GetValue(0), CultureInfo.InvariantCulture)) =
                                    If(k_reader.IsDBNull(1), "NULL", Convert.ToString(k_reader.GetValue(1), CultureInfo.InvariantCulture))
                            End If
                        End While
                    End Using
                End Using
            End If
            If TableExists(k_profile, "NR") Then
                Using k_cmd As New OleDbCommand("SELECT * FROM NR", k_cn)
                    Using k_reader = k_cmd.ExecuteReader()
                        Dim k_rows As Integer = 0
                        While k_reader.Read()
                            k_rows += 1
                            If k_rows > 1 Then Continue While
                            For k_i As Integer = 0 To k_reader.FieldCount - 1
                                Dim k_column As String = k_reader.GetName(k_i)
                                If String.Equals(k_column, "ID", StringComparison.OrdinalIgnoreCase) OrElse
                                   String.Equals(k_column, "NUM", StringComparison.OrdinalIgnoreCase) Then Continue For
                                k_profile.Config("NR." & k_column) = If(k_reader.IsDBNull(k_i), "NULL", Convert.ToString(k_reader.GetValue(k_i), CultureInfo.InvariantCulture))
                            Next
                        End While
                        k_profile.Config("NR.randuri") = k_rows.ToString(CultureInfo.InvariantCulture)
                    End Using
                End Using
            End If
            If TableExists(k_profile, "UNIT") Then
                ' only the identity fields; names of people (Director, Contabil, ...) and contacts are NOT read
                Using k_cmd As New OleDbCommand("SELECT DC, AnDate FROM UNIT", k_cn)
                    Using k_reader = k_cmd.ExecuteReader()
                        If k_reader.Read() Then
                            k_profile.Config("UNIT.DC") = If(k_reader.IsDBNull(0), "NULL", Convert.ToString(k_reader.GetValue(0), CultureInfo.InvariantCulture))
                            k_profile.Config("UNIT.AnDate") = If(k_reader.IsDBNull(1), "NULL", Convert.ToString(k_reader.GetValue(1), CultureInfo.InvariantCulture))
                        End If
                    End Using
                End Using
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("VrcProfiler.ReadConfig", ex)
            k_profile.Config("config.eroare") = ex.Message
        End Try
    End Sub

    ''' <summary>UNIT.NumeUnitate (the name of the unit, not of a person). Its own query: a base without the column still gives its DC and AnDate.</summary>
    Private Shared Sub ReadUnitName(k_cn As OleDbConnection, k_profile As VrcFileProfile)
        Try
            If Not TableExists(k_profile, "UNIT") Then Return
            Using k_cmd As New OleDbCommand("SELECT NumeUnitate FROM UNIT", k_cn)
                Using k_reader = k_cmd.ExecuteReader()
                    If k_reader.Read() Then k_profile.Config("UNIT.NumeUnitate") = If(k_reader.IsDBNull(0), "NULL", Convert.ToString(k_reader.GetValue(0), CultureInfo.InvariantCulture))
                End Using
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("VrcProfiler.ReadUnitName", ex)   ' not every base has the column: the name is then simply not proposed
        End Try
    End Sub

    Private Shared Function TableExists(k_profile As VrcFileProfile, k_name As String) As Boolean
        Return k_profile.Tables.Any(Function(k_t) String.Equals(k_t.Name, k_name, StringComparison.OrdinalIgnoreCase))
    End Function

    ' ---- header vs lines (N01/N02): counted, never changed -----------------------------------------------------------
    Private Shared Sub ReadHeaderChecks(k_cn As OleDbConnection, k_profile As VrcFileProfile)
        Try
            AddHeaderCheck(k_cn, k_profile, "NIR", "IdIntrare", "Valoare", "Intrari", "IdIntrare", "ValoareCUTVA")
            AddHeaderCheck(k_cn, k_profile, "Iesiri_H", "IdBonConsum", "ValoareBC", "Iesiri", "IdBonConsum", "ValoareCUTVA")
        Catch ex As Exception
            GlobalErrorLog.Write("VrcProfiler.ReadHeaderChecks", ex)
            k_profile.Config("antete.eroare") = ex.Message
        End Try
    End Sub

    Private Shared Sub AddHeaderCheck(k_cn As OleDbConnection, k_profile As VrcFileProfile, k_header As String, k_headerKey As String,
                                      k_headerValue As String, k_lines As String, k_lineKey As String, k_lineValue As String)
        If Not (TableExists(k_profile, k_header) AndAlso TableExists(k_profile, k_lines)) Then Return
        Dim k_tableHeader = k_profile.Tables.First(Function(k_t) String.Equals(k_t.Name, k_header, StringComparison.OrdinalIgnoreCase))
        Dim k_tableLines = k_profile.Tables.First(Function(k_t) String.Equals(k_t.Name, k_lines, StringComparison.OrdinalIgnoreCase))
        If k_tableHeader.Absent.Contains(k_headerKey) OrElse k_tableHeader.Absent.Contains(k_headerValue) OrElse
           k_tableLines.Absent.Contains(k_lineKey) OrElse k_tableLines.Absent.Contains(k_lineValue) Then Return

        Dim k_sums As New Dictionary(Of Long, Decimal)()
        Using k_cmd As New OleDbCommand($"SELECT [{k_lineKey}], [{k_lineValue}] FROM [{k_lines}]", k_cn)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    If k_reader.IsDBNull(0) Then Continue While
                    Dim k_id As Long = Convert.ToInt64(k_reader.GetValue(0), CultureInfo.InvariantCulture)
                    Dim k_value As Decimal = If(k_reader.IsDBNull(1), 0D, Convert.ToDecimal(k_reader.GetValue(1), CultureInfo.InvariantCulture))
                    Dim k_sum As Decimal = 0D
                    k_sums.TryGetValue(k_id, k_sum)
                    k_sums(k_id) = k_sum + k_value
                End While
            End Using
        End Using

        Dim k_check As New VrcHeaderCheck() With {.HeaderTable = k_header}
        Using k_cmd As New OleDbCommand($"SELECT [{k_headerKey}], [{k_headerValue}] FROM [{k_header}]", k_cn)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    If k_reader.IsDBNull(0) Then Continue While
                    k_check.Headers += 1
                    Dim k_id As Long = Convert.ToInt64(k_reader.GetValue(0), CultureInfo.InvariantCulture)
                    Dim k_sum As Decimal = 0D
                    Dim k_hasLines As Boolean = k_sums.TryGetValue(k_id, k_sum)
                    If Not k_hasLines Then k_check.WithoutLines += 1
                    If k_reader.IsDBNull(1) Then
                        k_check.HeaderNull += 1
                    Else
                        Dim k_headerAmount As Decimal = Convert.ToDecimal(k_reader.GetValue(1), CultureInfo.InvariantCulture)
                        If k_headerAmount = 0D Then
                            k_check.HeaderZero += 1
                        Else
                            Dim k_diff As Decimal = Math.Abs(k_headerAmount - k_sum)
                            If k_diff > 1D Then k_check.DiffOverOneLeu += 1
                            If k_diff > k_check.MaxAbsDiff Then k_check.MaxAbsDiff = k_diff
                        End If
                    End If
                End While
            End Using
        End Using
        k_profile.HeaderChecks.Add(k_check)
    End Sub

    Private Shared Function HashOf(k_path As String) As String
        ' Shared read: the file may be open in Access; the hash only reads it.
        Using k_stream As New FileStream(k_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Using k_sha = SHA256.Create()
                Return Convert.ToHexString(k_sha.ComputeHash(k_stream))
            End Using
        End Using
    End Function

End Class
