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
Partial Public NotInheritable Class VrcWriter

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
        ("VC_FACTURI", {"ID", "CodPartener", "NUMAR", "DATA", "MONEDA", "TOTAL", "TOTAL_CONFIRMAT", "STARE", "MIGRATA", "LOCALA", "REUNITA_IN", "VERSION", "CREAT_DE"}),
        ("VC_FACTURI_LINII", {"ID", "FACTURA_ID", "DENUMIRE", "UM", "CANT", "COTA", "VALOARE", "RECONSTRUITA"}),
        ("VC_NIR_FACTURI", {"ID", "FACTURA_ID", "SUBUNIT_ID", "GESTIUNE_ID", "EXERCITIU_ID", "NIR_ID", "TOTAL_SURSA", "SOURCE_ID", "SOURCE_KEY"}),
        ("VC_FACTURI_REPARTIZARI", {"NIR_FACTURA_ID", "FACTURA_LINIE_ID", "SUBUNIT_ID", "GESTIUNE_ID", "EXERCITIU_ID", "INTRARE_ID", "CANT", "VALOARE"}),
        ("VC_NIR_CALCULE", {"SUBUNIT_ID", "GESTIUNE_ID", "EXERCITIU_ID", "INTRARE_ID", "TOTAL_BRUT", "PRET_INITIAL", "ORIGINE_JSON"}),
        ("Parteneri", {"CodFiscal", "Ascuns"})}

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
                Dim k_schema = TargetSchema.Load(k_cn, k_plan.Dc)
                k_found.AddRange(SchemaProblems(k_schema))
                If k_found.Count > 0 Then Return k_found
                Dim k_tables = RequiredTables()
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

                For Each k_unit In k_plan.SubunitNames()
                    Using k_cmd As New MySqlCommand("SELECT s.SubunitId, (SELECT COUNT(*) FROM VC_Imports i WHERE i.SubunitId=s.SubunitId), (SELECT COUNT(*) FROM VC_Gestiuni g WHERE g.SubunitId=s.SubunitId) FROM VC_Subunitati s WHERE s.Name=@n", k_cn)
                        k_cmd.Parameters.AddWithValue("@n", k_unit)
                        Using k_reader = k_cmd.ExecuteReader()
                            If k_reader.Read() Then
                                If k_reader.GetInt64(1) > 0 Then
                                    k_found.Add($"Subunitatea «{k_unit}» are deja un transfer confirmat; nu se mai poate migra (nu există reimport).")
                                ElseIf k_reader.GetInt64(2) > 0 Then
                                    k_found.Add($"Subunitatea «{k_unit}» există și are gestiuni; transferul cere o subunitate nouă.")
                                Else
                                    k_found.Add($"Subunitatea «{k_unit}» există deja; alegeți o altă denumire.")
                                End If
                            End If
                        End Using
                    End Using
                Next
            End Using
            Return k_found
        Catch ex As Exception
            GlobalErrorLog.Write("VrcWriter.Preflight", ex)
            Throw
        End Try
    End Function

    ''' <summary>Missing display records only: NIRs retain their normalized fiscal identity independently of Parteneri.</summary>
    Public Shared Function UnmatchedSuppliers(k_server As TargetServer, k_plan As VrcPlan) As List(Of String)
        Try
            Dim k_partners As New HashSet(Of String)(StringComparer.Ordinal)
            ' Include hidden records ONLY in this existence set: never propose ANAF creation
            ' or reactivation for an existing hidden partner. This is not a document link.
            Using k_cn = k_server.Open(k_plan.Dc)
                Using k_cmd As New MySqlCommand("SELECT CodFiscal FROM Parteneri WHERE CodFiscal IS NOT NULL AND CodFiscal <> ''", k_cn)
                    Using k_reader = k_cmd.ExecuteReader()
                        While k_reader.Read()
                            k_partners.Add(VrcConvert.NormalizeTaxCode(k_reader.GetString(0)))
                        End While
                    End Using
                End Using
            End Using
            For Each k_prepared In k_plan.NewPartners
                k_partners.Add(VrcConvert.NormalizeTaxCode(k_prepared.TaxCode))   ' prepared from ANAF: added inside the transfer transaction
            Next
            Dim k_missing As New SortedSet(Of String)(StringComparer.Ordinal)
            For Each k_source In k_plan.Sources
                If k_source.Profile.Tables.All(Function(k_t) Not String.Equals(k_t.Name, "Furnizori", StringComparison.OrdinalIgnoreCase)) Then Continue For
                Dim k_provider As String = String.Empty
                Using k_cn = VrcAccess.OpenReadOnly(k_source.Source.Path, k_provider)
                    Using k_cmd As New OleDbCommand("SELECT DISTINCT f.CF, n.NrFact FROM NIR n LEFT JOIN Furnizori f ON n.IDF = f.IDF", k_cn)
                        Using k_reader = k_cmd.ExecuteReader()
                            While k_reader.Read()
                                If VrcConvert.IsOpeningStock(If(k_reader.IsDBNull(1), String.Empty, Convert.ToString(k_reader.GetValue(1), CultureInfo.InvariantCulture))) Then Continue While
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
                For Each k_table In VrcImportMap.Tables()
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
