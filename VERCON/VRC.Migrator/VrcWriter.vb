Imports System.Data.OleDb
Imports System.Globalization
Imports System.Text
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>
''' The MariaDB side of VRC.Migrator: the checks before the transfer (<see cref="Preflight"/>, the «Testează» button), the single
''' transfer (<see cref="Run"/>) and the read-only diagnostic after a lost answer (<see cref="Diagnose"/>). Nothing here
''' alters the structure of a database (no DDL) or writes to Access.
''' </summary>
Public NotInheritable Class VrcWriter

    ''' <summary>Control tables the transfer needs, with the columns it writes.</summary>
    Private Shared ReadOnly ControlTables As (Name As String, Columns As String())() = {
        ("VC_Subunitati", {"SubunitId", "Name"}),
        ("VC_Gestiuni", {"GestiuneId", "SubunitId", "Name", "Type", "State", "Zec", "Scara", "Populated"}),
        ("VC_Exercitii", {"ExercitiuId", "SubunitId", "GestiuneId", "An"}),
        ("VC_Imports", {"ImportId", "SubunitId", "ConfirmedBy", "ToolVersion", "PlanHash", "Summary", "ConfirmedAt"}),
        ("VC_ImportSources", {"SourceId", "ImportId", "SubunitId", "GestiuneId", "SourceFile", "SourceHash", "SourceYear", "SizeBytes"}),
        ("VC_IdMap", {"SourceId", "TableName", "Kind", "SourceKey", "TargetId"}),
        ("VC_MigrationIssues", {"IssueId", "ImportId", "SourceId", "TableName", "SourceKey", "FieldName", "RuleCode", "Severity", "OriginalValue", "TargetValue", "Justification"}),
        ("VC_Numbering", {"SubunitId", "GestiuneId", "ExercitiuId", "Kind", "LastNumber"}),
        ("VC_Settings", {"SubunitId", "GestiuneId", "SettingKey", "IntValue"}),
        ("Parteneri", {"IdPartener", "CodFiscal"})}

    Private Sub New()
    End Sub

    ' ---- Testează ---------------------------------------------------------------------------------------------------------
    ''' <summary>What stops the transfer on the server side. Empty list = ready. Reads only.</summary>
    Public Shared Function Preflight(k_server As TargetServer, k_plan As VrcPlan) As List(Of String)
        Try
            Dim k_found As New List(Of String)()
            If Not k_plan.IsComplete Then
                k_found.Add("Planul nu este complet: " & String.Join(" ", k_plan.Blockers.Take(3)))
                Return k_found
            End If
            If Not k_server.DatabaseNames().Contains(k_plan.Dc, StringComparer.OrdinalIgnoreCase) Then
                k_found.Add($"Pe server nu există baza «{k_plan.Dc}» (DC-ul din Access).")
                Return k_found
            End If
            Using k_cn = k_server.Open(k_plan.Dc)
                Dim k_ready As Boolean = True
                Dim k_tables As New List(Of (Name As String, Columns As String()))(ControlTables)
                For Each k_t In VrcTargetSchema.Tables()
                    Dim k_names As New List(Of String) From {"SubunitId", "GestiuneId"}
                    If k_t.IsExercise Then k_names.Add("ExercitiuId")
                    k_names.AddRange(k_t.Columns.Where(Function(k_c) k_c.Link <> VrcLinkKind.Skip AndAlso k_c.Link <> VrcLinkKind.Supplier).Select(Function(k_c) k_c.Name))
                    If k_t.Name = "NIR" Then k_names.AddRange({"IdPartener", "CodFiscalSursa", "TipDoc"})
                    k_tables.Add((k_t.TargetName, k_names.ToArray()))
                Next
                For Each k_table In k_tables
                    Dim k_have = ColumnsOf(k_cn, k_plan.Dc, k_table.Name)
                    If k_have.Count = 0 Then
                        k_found.Add($"Lipsește tabelul {k_table.Name}. Aplicați schema VERCON (VC_AVACONT_SURSA.sql, sincronizare) pe baza «{k_plan.Dc}».")
                        k_ready = False
                        Continue For
                    End If
                    For Each k_column In k_table.Columns
                        If Not k_have.Contains(k_column) Then
                            k_found.Add($"Lipsește coloana {k_table.Name}.{k_column}.")
                            k_ready = False
                        End If
                    Next
                Next
                If Not k_ready Then Return k_found

                For Each k_table In k_tables.Where(Function(k_x) k_x.Name.StartsWith("VC_", StringComparison.Ordinal))
                    Using k_cmd As New MySqlCommand("SELECT ENGINE, TABLE_COLLATION FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=@t", k_cn)
                        k_cmd.Parameters.AddWithValue("@t", k_table.Name)
                        Using k_reader = k_cmd.ExecuteReader()
                            If k_reader.Read() Then
                                If Not String.Equals(Convert.ToString(k_reader.GetValue(0), CultureInfo.InvariantCulture), "InnoDB", StringComparison.OrdinalIgnoreCase) Then k_found.Add($"{k_table.Name} nu este InnoDB.")
                                If Not String.Equals(Convert.ToString(k_reader.GetValue(1), CultureInfo.InvariantCulture), "utf8mb3_general_ci", StringComparison.OrdinalIgnoreCase) Then k_found.Add($"{k_table.Name} nu este utf8mb3_general_ci.")
                            End If
                        End Using
                    End Using
                Next

                Using k_cmd As New MySqlCommand("SELECT s.SubunitId, (SELECT COUNT(*) FROM VC_Imports i WHERE i.SubunitId=s.SubunitId), (SELECT COUNT(*) FROM VC_Gestiuni g WHERE g.SubunitId=s.SubunitId) FROM VC_Subunitati s WHERE s.Name=@n", k_cn)
                    k_cmd.Parameters.AddWithValue("@n", k_plan.SubunitName.Trim())
                    Using k_reader = k_cmd.ExecuteReader()
                        If k_reader.Read() Then
                            If k_reader.GetInt64(1) > 0 Then
                                k_found.Add($"Subunitatea «{k_plan.SubunitName.Trim()}» are deja un transfer confirmat; nu se mai poate migra (nu există reimport).")
                            ElseIf k_reader.GetInt64(2) > 0 Then
                                k_found.Add($"Subunitatea «{k_plan.SubunitName.Trim()}» există și are gestiuni; transferul cere o subunitate nouă.")
                            Else
                                k_found.Add($"Subunitatea «{k_plan.SubunitName.Trim()}» există deja; alegeți o altă denumire.")
                            End If
                        End If
                    End Using
                End Using
            End Using
            Return k_found
        Catch ex As Exception
            GlobalErrorLog.Write("VrcWriter.Preflight", ex)
            Throw
        End Try
    End Function

    ''' <summary>Suppliers of the sources whose tax code matches no KBOT partner (warning only: those NIR are imported without a partner).</summary>
    Public Shared Function UnmatchedSuppliers(k_server As TargetServer, k_plan As VrcPlan) As List(Of String)
        Try
            Dim k_partners As New HashSet(Of String)(StringComparer.Ordinal)
            Using k_cn = k_server.Open(k_plan.Dc)
                Using k_cmd As New MySqlCommand("SELECT CodFiscal FROM Parteneri WHERE CodFiscal IS NOT NULL AND CodFiscal <> ''", k_cn)
                    Using k_reader = k_cmd.ExecuteReader()
                        While k_reader.Read()
                            k_partners.Add(VrcConvert.NormalizeTaxCode(k_reader.GetString(0)))
                        End While
                    End Using
                End Using
            End Using
            Dim k_missing As New SortedSet(Of String)(StringComparer.Ordinal)
            For Each k_source In k_plan.Sources
                If k_source.Profile.Tables.All(Function(k_t) Not String.Equals(k_t.Name, "Furnizori", StringComparison.OrdinalIgnoreCase)) Then Continue For
                Dim k_provider As String = String.Empty
                Using k_cn = VrcAccess.OpenReadOnly(k_source.Source.Path, k_provider)
                    Using k_cmd As New OleDbCommand("SELECT DISTINCT f.CF FROM Furnizori f INNER JOIN NIR n ON n.IDF = f.IDF", k_cn)
                        Using k_reader = k_cmd.ExecuteReader()
                            While k_reader.Read()
                                Dim k_code As String = If(k_reader.IsDBNull(0), String.Empty, VrcConvert.NormalizeTaxCode(Convert.ToString(k_reader.GetValue(0), CultureInfo.InvariantCulture)))
                                If k_code.Length = 0 OrElse Not k_partners.Contains(k_code) Then k_missing.Add(If(k_code.Length = 0, "(fără cod fiscal)", k_code))
                            End While
                        End Using
                    End Using
                End Using
            Next
            Return k_missing.ToList()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcWriter.UnmatchedSuppliers", ex)
            Throw
        End Try
    End Function

    Private Shared Function ColumnsOf(k_cn As MySqlConnection, k_database As String, k_table As String) As HashSet(Of String)
        Dim k_result As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Using k_cmd As New MySqlCommand("SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@d AND TABLE_NAME=@t", k_cn)
            k_cmd.Parameters.AddWithValue("@d", k_database)
            k_cmd.Parameters.AddWithValue("@t", k_table)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    k_result.Add(k_reader.GetString(0))
                End While
            End Using
        End Using
        Return k_result
    End Function

    ' ---- Migrează ---------------------------------------------------------------------------------------------------------
    ''' <summary>The one transfer. Never throws for a data problem: the result carries the state (rollback, commit, unknown) and the findings.</summary>
    Public Shared Function Run(k_server As TargetServer, k_plan As VrcPlan, k_user As String, k_cancel As CancellationToken, k_log As Action(Of String)) As VrcRunResult
        Try
            Return New VrcRunner(k_server, k_plan, k_user, k_cancel, k_log).Run()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcWriter.Run", ex)
            Throw
        End Try
    End Function

    ' ---- Diagnostic (read-only) -------------------------------------------------------------------------------------------
    ''' <summary>
    ''' After a lost COMMIT answer, or any time: is the subunit's transfer there? Reads the marker, the sources and the row counts.
    ''' It never inserts or repeats anything.
    ''' </summary>
    Public Shared Function Diagnose(k_server As TargetServer, k_dc As String, k_subunit As String) As String
        Try
            Dim k_sb As New StringBuilder()
            k_sb.AppendLine($"# Diagnostic transfer — DC `{k_dc}`, subunitate «{k_subunit.Trim()}»")
            k_sb.AppendLine()
            Using k_cn = k_server.Open(k_dc)
                Dim k_subunitId As Long = 0
                Using k_cmd As New MySqlCommand("SELECT SubunitId FROM VC_Subunitati WHERE Name=@n", k_cn)
                    k_cmd.Parameters.AddWithValue("@n", k_subunit.Trim())
                    Dim k_value = k_cmd.ExecuteScalar()
                    If k_value Is Nothing OrElse k_value Is DBNull.Value Then
                        k_sb.AppendLine("Subunitatea nu există pe server: **nimic nu a rămas** (transferul nu a fost confirmat). Se poate relua după verificarea planului.")
                        Return k_sb.ToString()
                    End If
                    k_subunitId = Convert.ToInt64(k_value, CultureInfo.InvariantCulture)
                End Using
                Using k_cmd As New MySqlCommand("SELECT ImportId, ConfirmedAt, ConfirmedBy, ToolVersion, PlanHash FROM VC_Imports WHERE SubunitId=@s", k_cn)
                    k_cmd.Parameters.AddWithValue("@s", k_subunitId)
                    Using k_reader = k_cmd.ExecuteReader()
                        If k_reader.Read() Then
                            k_sb.AppendLine($"**Transfer confirmat**: ImportId {k_reader.GetInt64(0)}, {k_reader.GetDateTime(1):yyyy-MM-dd HH:mm}, {k_reader.GetString(2)}, {k_reader.GetString(3)}, plan `{k_reader.GetString(4)}`.")
                            k_sb.AppendLine("Subunitatea este închisă pentru orice alt import: **nu repetați transferul**.")
                        Else
                            k_sb.AppendLine("Subunitatea există, dar **fără marker de import**: transfer neconfirmat. Nu continuați manual; spuneți administratorului.")
                        End If
                    End Using
                End Using
                k_sb.AppendLine()
                k_sb.AppendLine("| Tabel | Rânduri în subunitate |")
                k_sb.AppendLine("|---|---:|")
                For Each k_table In VrcTargetSchema.Tables()
                    Using k_cmd As New MySqlCommand($"SELECT COUNT(*) FROM `{k_table.TargetName}` WHERE SubunitId=@s", k_cn)
                        k_cmd.Parameters.AddWithValue("@s", k_subunitId)
                        k_sb.AppendLine($"| {k_table.TargetName} | {Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture):N0} |")
                    End Using
                Next
                For Each k_name In {"VC_Gestiuni", "VC_Exercitii", "VC_ImportSources", "VC_Numbering", "VC_Settings"}
                    Using k_cmd As New MySqlCommand($"SELECT COUNT(*) FROM `{k_name}` WHERE SubunitId=@s", k_cn)
                        k_cmd.Parameters.AddWithValue("@s", k_subunitId)
                        k_sb.AppendLine($"| {k_name} | {Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture):N0} |")
                    End Using
                Next
                Using k_cmd As New MySqlCommand("SELECT Severity, COUNT(*) FROM VC_MigrationIssues WHERE ImportId IN (SELECT ImportId FROM VC_Imports WHERE SubunitId=@s) GROUP BY Severity", k_cn)
                    k_cmd.Parameters.AddWithValue("@s", k_subunitId)
                    Using k_reader = k_cmd.ExecuteReader()
                        While k_reader.Read()
                            k_sb.AppendLine($"| Constatări {k_reader.GetString(0)} | {k_reader.GetInt64(1):N0} |")
                        End While
                    End Using
                End Using
            End Using
            Return k_sb.ToString()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcWriter.Diagnose", ex)
            Throw
        End Try
    End Function

End Class
