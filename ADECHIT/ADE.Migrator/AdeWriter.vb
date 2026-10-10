Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>
''' The MariaDB side: what is in the unit database now, whether it is ready to receive the data, and the write itself.
''' Since SLICE-ADE10 the data lands in ONE SUBUNIT of the unit (DC): a new one named by the operator or an existing empty one.
''' The server allocates every key; each reference is rewritten through a per-table map, which is kept in AD_IdMap.
''' The write is one transaction. A failed commit acknowledgement is reported as an unknown outcome.
''' </summary>
Public NotInheritable Class AdeWriter

    ''' <summary>Rows per INSERT of the id map.</summary>
    Private Const MapBatch As Integer = 400

    ''' <summary>Rows per INSERT of a data table (lowered for wide tables by <see cref="MaxParameters"/>).</summary>
    Private Const InsertBatch As Integer = 500

    ''' <summary>Cap on parameters in one INSERT, so a wide table never builds a statement near max_allowed_packet.</summary>
    Private Const MaxParameters As Integer = 20000

    Private Sub New()
    End Sub

    ''' <summary>Every table the migration writes, in insertion order, with the two it builds itself.</summary>
    Private Shared Function AllTargets(k_plan As AdePlan) As List(Of AdeTable)
        Return k_plan.Tables.Select(Function(t) t.Table).ToList()
    End Function

    ''' <summary>The subunit with this name in the unit database, or Nothing. Names are unique (case-insensitive collation).</summary>
    Private Shared Function FindSubunit(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_name As String) As (Id As Long, Active As Boolean)?
        Using k_cmd As New MySqlCommand("SELECT SubunitId, Active FROM AD_Subunits WHERE Name=@n", k_cn, k_tx)
            k_cmd.Parameters.AddWithValue("@n", k_name)
            Using k_reader = k_cmd.ExecuteReader()
                If Not k_reader.Read() Then Return Nothing
                Return (k_reader.GetInt64(0), k_reader.GetBoolean(1))
            End Using
        End Using
    End Function

    ''' <summary>Rows now in each AD_ table for the subunit <paramref name="k_subunit"/>; 0 when it does not exist yet, -1 when the table does not exist.</summary>
    Public Shared Function CountRows(k_server As TargetServer, k_dc As String, k_tables As IEnumerable(Of AdeTable), k_subunit As String) As Dictionary(Of String, Integer)
        Try
            Dim k_result As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Using k_cn = k_server.Open(k_dc)
                Dim k_found = If(ColumnsOf(k_cn, k_dc, "AD_Subunits").Count > 0, FindSubunit(k_cn, Nothing, k_subunit), Nothing)
                For Each k_table In k_tables
                    k_result(k_table.Name) = CountOne(k_cn, k_table.Target, If(k_found.HasValue, k_found.Value.Id, CType(Nothing, Long?)), k_found.HasValue)
                Next
            End Using
            Return k_result
        Catch ex As Exception
            ' The UI boundary records the propagated exception once.
            Throw
        End Try
    End Function

    ''' <summary>COUNT of one table; limited to the subunit when <paramref name="k_scoped"/>, else 0 (the subunit is still to be created).</summary>
    Private Shared Function CountOne(k_cn As MySqlConnection, k_tableName As String, k_subunitId As Long?, k_scoped As Boolean, Optional k_tx As MySqlTransaction = Nothing) As Integer
        Try
            If Not k_scoped Then
                ' A subunit that does not exist yet has no rows; only tell whether the table exists.
                Return If(TableExists(k_cn, k_tableName), 0, -1)
            End If
            Using k_cmd As New MySqlCommand($"SELECT COUNT(*) FROM `{k_tableName}` WHERE SubunitId=@s", k_cn, k_tx)
                k_cmd.Parameters.AddWithValue("@s", k_subunitId.Value)
                Return Convert.ToInt32(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
            End Using
        Catch ex As MySqlException When ex.ErrorCode = MySqlErrorCode.NoSuchTable
            Return -1
        End Try
    End Function

    Private Shared Function TableExists(k_cn As MySqlConnection, k_tableName As String) As Boolean
        Using k_cmd As New MySqlCommand("SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=@t", k_cn)
            k_cmd.Parameters.AddWithValue("@t", k_tableName)
            Return Convert.ToInt32(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    ''' <summary>
    ''' What stops a write: no such database, a missing table or column (sql/AD_05_subunitati.sql not run), a subunit that is
    ''' not empty, a file already imported. Empty list = ready.
    ''' </summary>
    Public Shared Function Problems(k_server As TargetServer, k_dc As String, k_plan As AdePlan, k_source As AdeSource, k_subunit As String) As List(Of String)
        Try
            Dim k_found As New List(Of String)()
            If k_subunit Is Nothing OrElse k_subunit.Trim().Length = 0 Then
                k_found.Add("Completați denumirea subunității.")
                Return k_found
            End If
            If k_subunit.Trim().Length > 100 Then k_found.Add("Denumirea subunității poate avea cel mult 100 de caractere.")
            If Not k_server.DatabaseNames().Contains(k_dc, StringComparer.OrdinalIgnoreCase) Then
                k_found.Add($"Pe server nu există baza «{k_dc}» (DC-ul din Access).")
                Return k_found
            End If
            Using k_cn = k_server.Open(k_dc)
                Dim k_schemaReady = True
                For Each k_table In AllTargets(k_plan)
                    Dim k_have = ColumnsOf(k_cn, k_dc, k_table.Target)
                    If k_have.Count = 0 Then
                        k_found.Add($"Lipsește tabelul {k_table.Target}. Rulați întâi sql/AD_03_schema_finala.sql și sql/AD_05_subunitati.sql pe baza «{k_dc}».")
                        k_schemaReady = False
                        Continue For
                    End If
                    For Each k_col In k_table.Columns
                        If Not k_have.Contains(k_col.Name) Then k_found.Add($"Lipsește coloana {k_table.Target}.{k_col.Name}.")
                    Next
                    If Not k_have.Contains("SubunitId") Then
                        k_found.Add($"Lipsește coloana {k_table.Target}.SubunitId. Rulați sql/AD_05_subunitati.sql pe baza «{k_dc}».")
                        k_schemaReady = False
                    End If
                Next
                For Each k_pair In {("AD_Subunits", {"SubunitId", "Name", "Active"}), ("AD_ReceiptConfig", {"SubunitId", "Serie", "Numar", "Explicatie"}),
                                    ("AD_PortalParents", {"CNP", "Email", "CodAccesPortal"}),
                                    ("AD_Platitori_sub", {"CodAccesPortal"}),
                                    ("AD_IdMap", {"SubunitId", "ImportHash", "TableName", "SourceId", "TargetId"}),
                                    ("AD_Imports", {"SubunitId", "SourceHash", "SourceFile", "Manifest", "Result"})}
                    Dim k_have = ColumnsOf(k_cn, k_dc, k_pair.Item1)
                    If k_have.Count = 0 Then
                        k_found.Add($"Lipsește tabelul {k_pair.Item1}. Rulați întâi sql/AD_05_subunitati.sql pe baza «{k_dc}».")
                        k_schemaReady = False
                        Continue For
                    End If
                    For Each k_column In k_pair.Item2
                        If Not k_have.Contains(k_column) Then
                            k_found.Add($"Lipsește coloana {k_pair.Item1}.{k_column}.")
                            k_schemaReady = False
                        End If
                    Next
                Next
                If Not k_schemaReady Then Return k_found

                ' The destination subunit: an existing one must be active and completely empty.
                Dim k_existing = FindSubunit(k_cn, Nothing, k_subunit.Trim())
                If k_existing.HasValue Then
                    If Not k_existing.Value.Active Then k_found.Add($"Subunitatea «{k_subunit.Trim()}» este inactivă.")
                    For Each k_table In AllTargets(k_plan)
                        Dim k_rows = CountOne(k_cn, k_table.Target, k_existing.Value.Id, True)
                        If k_rows > 0 Then k_found.Add($"{k_table.Target} are deja {k_rows} rânduri în subunitatea «{k_subunit.Trim()}»; migrarea scrie numai într-o subunitate goală.")
                    Next
                    If CountOne(k_cn, "AD_Imports", k_existing.Value.Id, True) > 0 Then k_found.Add($"Subunitatea «{k_subunit.Trim()}» a primit deja un import.")
                    If CountOne(k_cn, "AD_ReceiptConfig", k_existing.Value.Id, True) > 0 Then
                        k_found.Add($"Seria de chitanțe a subunității «{k_subunit.Trim()}» există deja în AD_ReceiptConfig; nu se suprascrie.")
                    End If
                End If
                Using k_hash As New MySqlCommand("SELECT COUNT(*) FROM AD_Imports WHERE SourceHash=@h", k_cn)
                    k_hash.Parameters.AddWithValue("@h", k_source.FileHash)
                    If Convert.ToInt32(k_hash.ExecuteScalar(), CultureInfo.InvariantCulture) > 0 Then
                        k_found.Add("Acest fișier Access a fost deja importat în această bază (AD_Imports).")
                    End If
                End Using
                ' Rollback is only meaningful when every destination participates in the transaction.
                Dim k_targets As New List(Of String)
                For Each k_table In AllTargets(k_plan)
                    k_targets.Add(k_table.Target)
                Next
                k_targets.AddRange({"AD_Imports", "AD_Subunits", "AD_ReceiptConfig", "AD_IdMap", "AD_Lock"})
                For Each k_target In k_targets
                    Using k_engine As New MySqlCommand("SELECT ENGINE FROM information_schema.TABLES WHERE TABLE_SCHEMA=@db AND TABLE_NAME=@t", k_cn)
                        k_engine.Parameters.AddWithValue("@db", k_dc)
                        k_engine.Parameters.AddWithValue("@t", k_target)
                        If Not String.Equals(Convert.ToString(k_engine.ExecuteScalar(), CultureInfo.InvariantCulture), "InnoDB", StringComparison.OrdinalIgnoreCase) Then
                            k_found.Add($"{k_dc}.{k_target} trebuie să existe și să folosească InnoDB pentru rollback.")
                        End If
                    End Using
                Next
            End Using
            Return k_found
        Catch ex As Exception
            ' The UI boundary records the propagated exception once.
            Throw
        End Try
    End Function

    Private Shared Function ColumnsOf(k_cn As MySqlConnection, k_dc As String, k_tableName As String) As HashSet(Of String)
        Dim k_names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Using k_cmd As New MySqlCommand("SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME=@t", k_cn)
            k_cmd.Parameters.AddWithValue("@db", k_dc)
            k_cmd.Parameters.AddWithValue("@t", k_tableName)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    k_names.Add(k_reader.GetString(0))
                End While
            End Using
        End Using
        Return k_names
    End Function

    ''' <summary>Writes the whole plan in one transaction. Returns committed counts, or Nothing when stopped before COMMIT.</summary>
    Public Shared Function Write(k_server As TargetServer, k_dc As String, k_source As AdeSource, k_plan As AdePlan,
                                 k_subunit As String, k_progress As IProgress(Of String), k_cancel As CancellationToken,
                                 k_journalRoot As String) As Dictionary(Of String, Integer)
        Try
            If k_plan.HasBlocking Then Throw New InvalidOperationException("Planul are blocaje; migrarea nu poate porni.")
            Dim k_subunitName = k_subunit.Trim()
            Using k_file As New FileStream(k_source.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Dim k_hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(k_file)).ToLowerInvariant()
                If Not String.Equals(k_hash, k_source.FileHash, StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidOperationException("Fișierul Access s-a modificat după citire. Citiți din nou fișierul și verificați planul.")
                End If
            End Using
            If k_cancel.IsCancellationRequested Then Return Nothing
            Dim k_problems = Problems(k_server, k_dc, k_plan, k_source, k_subunitName)
            If k_problems.Count > 0 Then Throw New InvalidOperationException(String.Join(Environment.NewLine, k_problems))
            If k_cancel.IsCancellationRequested Then Return Nothing
            Dim k_written As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Using k_dump As New SqlDumpWriter(k_journalRoot, k_dc, Sub(message) k_progress.Report(message))
                k_progress.Report("Jurnal SQL: " & k_dump.Folder)
                k_dump.WriteInfo({"Aplicație: ADE.Migrator", "Bază destinație: " & k_dc, "Subunitate destinație: " & k_subunitName, "DC sursă Access: " & k_source.Dc,
                                  "Fișier: " & Path.GetFileName(k_source.FilePath), "SHA-256: " & k_source.FileHash})
                Using k_cn = k_server.Open(k_dc)
                    Using k_tx = k_cn.BeginTransaction()
                        Dim k_commitAttempted = False
                        Try
                            If RollbackIfStopped(k_tx, k_cancel, k_dump) Then Return Nothing
                            Using k_parentLock As New MySqlCommand("SELECT ID FROM AD_Lock WHERE ID=0 FOR UPDATE", k_cn, k_tx)
                                If k_parentLock.ExecuteScalar() Is Nothing Then Throw New InvalidOperationException("Sincronizați schema AD_08 și rulați AD_08_02_interogare_unica.sql prin AvacontPush înainte de migrare.")
                            End Using
                            Dim k_subunitId = EnsureSubunit(k_cn, k_tx, k_subunitName, k_plan, k_dump, k_progress)
                            Dim k_maps As New Dictionary(Of String, Dictionary(Of Long, Long))(StringComparer.OrdinalIgnoreCase)
                            For Each k_planTable In k_plan.Tables
                                k_progress.Report($"Scriu {k_planTable.Table.Target} ({k_planTable.Rows.Count} rânduri)...")
                                If RollbackIfStopped(k_tx, k_cancel, k_dump) Then Return Nothing
                                k_written(k_planTable.Table.Name) = InsertRows(k_cn, k_tx, k_planTable, k_subunitId, k_maps, k_cancel, k_progress, k_dump)
                                If RollbackIfStopped(k_tx, k_cancel, k_dump) Then Return Nothing
                            Next
                            k_progress.Report("Scriu maparea ID-urilor Access -> server...")
                            ProvisionPortalParents(k_cn, k_tx)
                            If RollbackIfStopped(k_tx, k_cancel, k_dump) Then Return Nothing
                            InsertIdMap(k_cn, k_tx, k_subunitId, k_source.FileHash, k_maps, k_dump)
                            If k_plan.ReceiptConfig IsNot Nothing Then
                                k_progress.Report("Scriu seria de chitanțe a subunității...")
                                If RollbackIfStopped(k_tx, k_cancel, k_dump) Then Return Nothing
                                InsertReceiptConfig(k_cn, k_tx, k_subunitId, k_plan.ReceiptConfig, k_dump)
                            End If
                            InsertImportRecord(k_cn, k_tx, k_subunitId, k_subunitName, k_source, k_written, k_dump, k_dc)
                            If RollbackIfStopped(k_tx, k_cancel, k_dump) Then Return Nothing
                            k_commitAttempted = True
                            k_tx.Commit()
                            k_dump.WriteFinal(True, k_written.Select(Function(pair) $"{pair.Key}: {pair.Value} rânduri"), Nothing)
                        Catch ex As Exception
                            Try
                                k_tx.Rollback()
                            Catch rollbackEx As Exception
                                k_dump.WriteComment("_outcome", "Rezultat necunoscut: rollback neconfirmat. Verificați serverul înainte de reluare.")
                                k_dump.FlushAll()
                                Throw New AggregateException("Rezultatul tranzacției nu poate fi confirmat. Verificați serverul înainte de reluare.", ex, rollbackEx)
                            End Try
                            ' A failed COMMIT acknowledgement can mean that the server committed before the connection failed.
                            If k_commitAttempted Then
                                k_dump.WriteComment("_outcome", "COMMIT neconfirmat. Verificați AD_Imports și tabelele pe server înainte de reluare.")
                                k_dump.FlushAll()
                                Throw New InvalidOperationException("Confirmarea COMMIT nu a fost primită. Verificați serverul înainte de reluare.", ex)
                            End If
                            k_dump.WriteFinal(False, {"Rollback confirmat."}, ex)
                            Throw
                        End Try
                    End Using
                End Using
            End Using
            Return k_written
        Catch ex As Exception
            ' Preserve the stack and let the UI boundary record the failure once.
            Throw
        End Try
    End Function

    ''' <summary>Normal stop path: confirm rollback instead of throwing a cancellation exception into the debugger.</summary>
    Private Shared Function RollbackIfStopped(k_tx As MySqlTransaction, k_cancel As CancellationToken, k_dump As SqlDumpWriter) As Boolean
        If Not k_cancel.IsCancellationRequested Then Return False
        k_tx.Rollback()
        k_dump.WriteFinal(False, {"Migrare oprită. Rollback confirmat."}, Nothing)
        Return True
    End Function

    ''' <summary>Preserve legacy contacts; ambiguous identities cannot use the portal.</summary>
    Private Shared Sub ProvisionPortalParents(k_cn As MySqlConnection, k_tx As MySqlTransaction)
        Dim k_commands = {
            "DROP TEMPORARY TABLE IF EXISTS ad08_parent_contacts",
            "CREATE TEMPORARY TABLE ad08_parent_contacts AS SELECT TRIM(CNP_Platitor) AS CNP, MAX(NULLIF(LOWER(TRIM(EMail)),'')) AS Email, COUNT(DISTINCT NULLIF(LOWER(TRIM(EMail)),'')) AS EmailCount FROM AD_Platitori_sub WHERE TRIM(CNP_Platitor) REGEXP '^[0-9]{13}$' GROUP BY TRIM(CNP_Platitor)",
            "UPDATE ad08_parent_contacts i SET Email=NULL WHERE i.EmailCount>1 OR EXISTS (SELECT 1 FROM AD_Platitori_sub p WHERE LOWER(TRIM(p.EMail))=i.Email AND COALESCE(TRIM(p.CNP_Platitor),'')<>i.CNP)",
            "UPDATE AD_PortalParents SET Email=NULL",
            "INSERT INTO AD_PortalParents (CNP,Email) SELECT CNP,Email FROM ad08_parent_contacts WHERE 1=1 ON DUPLICATE KEY UPDATE Email=VALUES(Email)",
            "UPDATE AD_PortalParents i SET CodAccesPortal=LOWER(HEX(RANDOM_BYTES(16))) WHERE i.CodAccesPortal IS NULL AND EXISTS (SELECT 1 FROM AD_Platitori_sub p JOIN AD_Platitori c ON c.IDP=p.IDP AND c.SubunitId=p.SubunitId WHERE TRIM(p.CNP_Platitor)=i.CNP AND COALESCE(c.Plecat,0)=0)",
            "UPDATE AD_Platitori_sub p JOIN AD_PortalParents i ON TRIM(p.CNP_Platitor)=i.CNP SET p.Version=p.Version+IF(NOT(p.CodAccesPortal<=>i.CodAccesPortal),1,0),p.CodAccesPortal=i.CodAccesPortal",
            "INSERT IGNORE INTO AD_Lock (ID) VALUES (0)",
            "DROP TEMPORARY TABLE ad08_parent_contacts"
        }
        ' Credentials never enter the SQL dump or application journal.
        For Each k_sql In k_commands
            Using k_cmd As New MySqlCommand(k_sql, k_cn, k_tx)
                k_cmd.ExecuteNonQuery()
            End Using
        Next
    End Sub

    ''' <summary>
    ''' Takes (or creates) the destination subunit and holds its mutation lock until COMMIT, the same row the web application
    ''' locks (AD_Lock, ID = SubunitId), so an import and a web operation in that subunit never overlap.
    ''' </summary>
    Private Shared Function EnsureSubunit(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_name As String, k_plan As AdePlan,
                                          k_dump As SqlDumpWriter, k_progress As IProgress(Of String)) As Long
        Dim k_found = FindSubunit(k_cn, k_tx, k_name)
        Dim k_id As Long
        If k_found.HasValue Then
            k_id = k_found.Value.Id
            k_progress.Report($"Subunitate existentă: «{k_name}» (id {k_id}).")
        Else
            Using k_cmd As New MySqlCommand("INSERT INTO AD_Subunits (Name) VALUES (@n)", k_cn, k_tx)
                k_cmd.Parameters.AddWithValue("@n", k_name)
                RecordCommand(k_dump, "AD_Subunits", k_cmd)
                k_cmd.ExecuteNonQuery()
                k_id = k_cmd.LastInsertedId
            End Using
            k_progress.Report($"Subunitate nouă: «{k_name}» (id {k_id}).")
        End If
        Using k_lockRow As New MySqlCommand("INSERT IGNORE INTO AD_Lock (ID) VALUES (@id)", k_cn, k_tx)
            k_lockRow.Parameters.AddWithValue("@id", k_id)
            RecordCommand(k_dump, "AD_Lock", k_lockRow)
            k_lockRow.ExecuteNonQuery()
        End Using
        Using k_lock As New MySqlCommand("SELECT ID FROM AD_Lock WHERE ID=@id FOR UPDATE", k_cn, k_tx)
            k_lock.Parameters.AddWithValue("@id", k_id)
            k_lock.ExecuteScalar()
        End Using
        ' Re-check under the lock: the whole destination must still be empty (the web application may have added rows since Testează).
        Dim k_checked As New List(Of String)(AllTargets(k_plan).Select(Function(t) t.Target))
        k_checked.AddRange({"AD_Imports", "AD_ReceiptConfig"})
        For Each k_target In k_checked
            Dim k_rows = CountOne(k_cn, k_target, k_id, True, k_tx)
            If k_rows > 0 Then
                Throw New InvalidOperationException($"{k_target} are {k_rows} rânduri în subunitatea «{k_name}» (adăugate între timp). Verificați din nou serverul.")
            End If
        Next
        Return k_id
    End Function

    Private Shared Function InsertRows(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_planTable As AdePlanTable, k_subunitId As Long,
                                      k_maps As Dictionary(Of String, Dictionary(Of Long, Long)),
                                      k_cancel As CancellationToken, k_progress As IProgress(Of String), k_dump As SqlDumpWriter) As Integer
        Dim k_table = k_planTable.Table
        If k_planTable.Rows.Count = 0 Then Return 0
        ' The server allocates every key (AUTO_INCREMENT); the Access key is only remembered in the map.
        Dim k_columns = k_table.Columns.Where(Function(c) c.Name <> k_table.Key).ToList()
        Dim k_links = AdeSchema.Relations.Concat(AdeSchema.BuiltRelations).Where(Function(r) String.Equals(r.Child, k_table.Name, StringComparison.OrdinalIgnoreCase)).
            ToDictionary(Function(r) r.Col, Function(r) r.Parent, StringComparer.Ordinal)
        Dim k_mine As New Dictionary(Of Long, Long)()
        k_maps(k_table.Name) = k_mine
        ' Months go in ascending Access order, so the server-allocated ids keep the order the Access rules compare.
        Dim k_ordered As List(Of Dictionary(Of String, Object)) = If(k_table.Name = "LunaD", k_planTable.Rows.OrderBy(Function(r) Convert.ToInt64(r("IDL"), CultureInfo.InvariantCulture)).ToList(), k_planTable.Rows)
        Dim k_head = $"INSERT INTO `{k_table.Target}` (`SubunitId`,{String.Join(",", k_columns.Select(Function(c) $"`{c.Name}`"))}) VALUES "
        Dim k_hasMap = k_planTable.SourceRows >= 0
        Dim k_perBatch = Math.Max(1, Math.Min(InsertBatch, MaxParameters \ (k_columns.Count + 1)))
        Dim k_count = 0
        ' One INSERT per batch of rows: a round trip to the server per row was the whole cost of a large table.
        For k_start = 0 To k_ordered.Count - 1 Step k_perBatch
            ' The caller rolls back the whole transaction when this loop stops early.
            If k_cancel.IsCancellationRequested Then Return k_count
            Dim k_n = Math.Min(k_perBatch, k_ordered.Count - k_start)
            Dim k_sql As New StringBuilder(k_head)
            Dim k_journal As New StringBuilder(k_head)
            Using k_cmd As New MySqlCommand(String.Empty, k_cn, k_tx)
                k_cmd.Parameters.AddWithValue("@SubunitId", k_subunitId)
                For k_i = 0 To k_n - 1
                    Dim k_row = k_ordered(k_start + k_i)
                    If k_i > 0 Then
                        k_sql.Append(","c)
                        k_journal.Append(","c)
                    End If
                    k_sql.Append("(@SubunitId")
                    k_journal.Append("(").Append(ValueConverter.ToLiteral(k_subunitId))
                    For k_j = 0 To k_columns.Count - 1
                        Dim k_col = k_columns(k_j)
                        Dim k_value = k_row(k_col.Name)
                        Dim k_parent As String = Nothing
                        If k_value IsNot Nothing AndAlso k_links.TryGetValue(k_col.Name, k_parent) Then
                            Dim k_new As Long
                            If Not k_maps(k_parent).TryGetValue(Convert.ToInt64(k_value, CultureInfo.InvariantCulture), k_new) Then
                                Throw New InvalidOperationException($"{k_table.Target}.{k_col.Name} = {k_value}: rândul părinte din {k_parent} nu a fost scris.")
                            End If
                            k_value = k_new
                        End If
                        Dim k_name = $"@v{k_i}_{k_j}"
                        k_cmd.Parameters.AddWithValue(k_name, If(k_value, DBNull.Value))
                        k_sql.Append(",").Append(k_name)
                        k_journal.Append(",").Append(ValueConverter.ToLiteral(k_value))
                    Next
                    k_sql.Append(")"c)
                    k_journal.Append(")"c)
                Next
                k_cmd.CommandText = k_sql.ToString()
                k_dump.WriteStatement(k_table.Target, k_journal.ToString())
                k_cmd.ExecuteNonQuery()
                ' Built rows have no Access key (IDGE/IDI); only source tables need an id map.
                If k_hasMap Then
                    Dim k_ids = ReadNewKeys(k_cn, k_tx, k_table, k_subunitId, k_cmd.LastInsertedId, k_n)
                    For k_i = 0 To k_n - 1
                        Dim k_key = k_ordered(k_start + k_i)(k_table.Key)
                        If k_key IsNot Nothing Then k_mine(Convert.ToInt64(k_key, CultureInfo.InvariantCulture)) = k_ids(k_i)
                    Next
                End If
            End Using
            k_count += k_n
            k_dump.FlushAll()
            k_progress.Report($"{k_table.Target}: {k_count}/{k_planTable.Rows.Count} rânduri pregătite; tranzacția nu este încă confirmată.")
        Next
        Return k_count
    End Function

    ''' <summary>
    ''' The keys the server gave to the rows of the INSERT that just ran, in row order. The subunit was empty and is locked
    ''' until COMMIT, so its rows from <paramref name="k_first"/> up are exactly this batch; reading them back does not
    ''' depend on how the server hands out AUTO_INCREMENT values (gaps, step, lock mode).
    ''' </summary>
    Private Shared Function ReadNewKeys(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_table As AdeTable, k_subunitId As Long,
                                        k_first As Long, k_expected As Integer) As List(Of Long)
        Dim k_ids As New List(Of Long)(k_expected)
        Using k_cmd As New MySqlCommand($"SELECT `{k_table.Key}` FROM `{k_table.Target}` WHERE SubunitId=@s AND `{k_table.Key}`>=@f ORDER BY `{k_table.Key}` LIMIT {k_expected}", k_cn, k_tx)
            k_cmd.Parameters.AddWithValue("@s", k_subunitId)
            k_cmd.Parameters.AddWithValue("@f", k_first)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    k_ids.Add(k_reader.GetInt64(0))
                End While
            End Using
        End Using
        If k_ids.Count <> k_expected Then
            Throw New InvalidOperationException($"{k_table.Target}: serverul a întors {k_ids.Count} chei pentru {k_expected} rânduri scrise.")
        End If
        Return k_ids
    End Function

    ''' <summary>AD_IdMap: import + table + Access id -> server id, in multi-row INSERTs, for audit and reconciliation.</summary>
    Private Shared Sub InsertIdMap(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_subunitId As Long, k_hash As String,
                                   k_maps As Dictionary(Of String, Dictionary(Of Long, Long)), k_dump As SqlDumpWriter)
        Dim k_all As New List(Of (Table As String, Source As Long, Target As Long))()
        For Each k_pair In k_maps
            For Each k_id In k_pair.Value
                k_all.Add((k_pair.Key, k_id.Key, k_id.Value))
            Next
        Next
        For k_start = 0 To k_all.Count - 1 Step MapBatch
            Dim k_part = k_all.Skip(k_start).Take(MapBatch).ToList()
            Dim k_text As New StringBuilder("INSERT INTO AD_IdMap (SubunitId,ImportHash,TableName,SourceId,TargetId) VALUES ")
            Using k_cmd As New MySqlCommand(String.Empty, k_cn, k_tx)
                k_cmd.Parameters.AddWithValue("@sub", k_subunitId)
                k_cmd.Parameters.AddWithValue("@hash", k_hash)
                For k_i = 0 To k_part.Count - 1
                    If k_i > 0 Then k_text.Append(","c)
                    k_text.Append($"(@sub,@hash,@t{k_i},@s{k_i},@g{k_i})")
                    k_cmd.Parameters.AddWithValue($"@t{k_i}", k_part(k_i).Table)
                    k_cmd.Parameters.AddWithValue($"@s{k_i}", k_part(k_i).Source)
                    k_cmd.Parameters.AddWithValue($"@g{k_i}", k_part(k_i).Target)
                Next
                k_cmd.CommandText = k_text.ToString()
                RecordCommand(k_dump, "AD_IdMap", k_cmd)
                k_cmd.ExecuteNonQuery()
            End Using
        Next
        k_dump.FlushAll()
    End Sub

    Private Shared Sub InsertReceiptConfig(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_subunitId As Long, k_config As AdeReceiptConfig, k_dump As SqlDumpWriter)
        Using k_cmd As New MySqlCommand("INSERT INTO AD_ReceiptConfig (SubunitId,Serie,Numar,Explicatie) VALUES (@sub,@s,@n,@e)", k_cn, k_tx)
            k_cmd.Parameters.AddWithValue("@sub", k_subunitId)
            k_cmd.Parameters.AddWithValue("@s", k_config.Serie)
            k_cmd.Parameters.AddWithValue("@n", k_config.Numar)
            k_cmd.Parameters.AddWithValue("@e", k_config.Explicatie)
            RecordCommand(k_dump, "AD_ReceiptConfig", k_cmd)
            k_cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Shared Sub InsertImportRecord(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_subunitId As Long, k_subunitName As String, k_source As AdeSource,
                                          k_written As Dictionary(Of String, Integer), k_dump As SqlDumpWriter, targetDc As String)
        Dim k_json = JsonSerializer.Serialize(New With {.counts = k_written, .unit = targetDc, .source_unit = k_source.Dc, .subunit = k_subunitName})
        Using k_cmd As New MySqlCommand("INSERT INTO AD_Imports (SubunitId,SourceHash,SourceFile,Manifest,Result) VALUES (@sub,@h,@f,@m,@r)", k_cn, k_tx)
            k_cmd.Parameters.AddWithValue("@sub", k_subunitId)
            k_cmd.Parameters.AddWithValue("@h", k_source.FileHash)
            k_cmd.Parameters.AddWithValue("@f", Path.GetFileName(k_source.FilePath))
            k_cmd.Parameters.AddWithValue("@m", k_json)
            k_cmd.Parameters.AddWithValue("@r", k_json)
            RecordCommand(k_dump, "AD_Imports", k_cmd)
            k_cmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>Renders the actual parameter values through the shared SQL literal converter before execution.</summary>
    Private Shared Sub RecordCommand(k_dump As SqlDumpWriter, k_table As String, k_command As MySqlCommand)
        Dim k_sql = k_command.CommandText
        ' Replace placeholders in one pass: inserted text may itself contain another parameter name.
        Dim k_values = k_command.Parameters.Cast(Of MySqlParameter)().ToDictionary(Function(p) p.ParameterName, Function(p) ValueConverter.ToLiteral(p.Value))
        k_sql = System.Text.RegularExpressions.Regex.Replace(k_sql, "@[A-Za-z][A-Za-z0-9_]*", Function(m) k_values(m.Value))
        k_dump.WriteStatement(k_table, k_sql)
    End Sub

End Class
