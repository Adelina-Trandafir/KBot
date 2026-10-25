Imports System.Globalization
Imports System.IO
Imports System.Security.Cryptography
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>
''' The three operations over the steps: read the Access file, verify against the unit database, write. Verify-then-write:
''' the writer only runs on a verification without a blocking finding, and re-checks that the Access file is the one that
''' was read.
''' </summary>
Friend NotInheritable Class AvcRunner

    Private Sub New()
    End Sub

    ''' <summary>SHA-256 of the file, lowercase hex. I/O: logs and rethrows.</summary>
    Public Shared Function HashOf(k_path As String) As String
        Try
            Using k_file As New FileStream(k_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Return Convert.ToHexString(SHA256.HashData(k_file)).ToLowerInvariant()
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AvcRunner.HashOf", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Reads the Access table of every step (ticked or not, so the grid shows all the counts) and records the file hash.
    ''' A step that cannot be read does not stop the others: its error comes back as a blocking finding.
    ''' </summary>
    Public Shared Function ReadAll(k_context As AvcContext, k_steps As IEnumerable(Of AvcStep)) As List(Of Finding)
        Try
            If k_context.Units.Count = 0 Then Throw New InvalidOperationException("Bifați cel puțin o unitate.")
            Dim k_errors As New List(Of Finding)()
            k_context.FileHashes.Clear()
            For Each k_unit In k_context.Units
                If Not k_unit.HasUnitFile Then
                    k_errors.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, String.Empty, String.Empty,
                                             $"Unitatea {k_unit.IdUnitate} ({k_unit.NumeUnitate}) nu are fișier de nomenclatoare pe acest disc: " &
                                             If(k_unit.UnitFilePath.Length = 0, "registrul nu numește niciunul.", k_unit.UnitFilePath)))
                End If
            Next
            If k_errors.Count > 0 Then
                For Each k_step In k_steps
                    k_step.Reset()
                Next
                Return k_errors
            End If
            For Each k_step In k_steps
                k_step.Reset()
                Try
                    k_step.ReadAccess(k_context)
                Catch ex As Exception
                    GlobalErrorLog.Write("AvcRunner.ReadAll." & k_step.Key, ex)
                    k_step.Reset()
                    k_errors.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, k_step.AccessTable, String.Empty,
                                             $"«{k_step.Title}» nu a putut fi citit din Access: {ex.Message}"))
                End Try
            Next
            ' Hashed AFTER the reads: the Access driver rewrites bytes of the file header when it opens and closes it, so a hash
            ' taken before the first open never matches again. From here on only an outside change alters the hash.
            For Each k_unit In k_context.Units
                k_context.FileHashes(k_unit.UnitFilePath) = HashOf(k_unit.UnitFilePath)
            Next
            Return k_errors
        Catch ex As Exception
            GlobalErrorLog.Write("AvcRunner.ReadAll", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Checks the ticked steps against the unit database. Read-only on the server. Server I/O: logs and rethrows
    ''' (a database that does not exist, a wrong password).
    ''' </summary>
    Public Shared Function Verify(k_context As AvcContext, k_steps As IReadOnlyList(Of AvcStep)) As VerificationReport
        Try
            RequireTarget(k_context)
            Dim k_report As New VerificationReport()
            Dim k_selected = k_steps.Where(Function(s) s.Selected).ToList()
            If k_selected.Count = 0 Then
                k_report.Add(AvcKinds.NICIUN_PAS, FindingClass.Blocant, String.Empty, String.Empty, "Nu este bifat niciun pas de migrat.")
                Return k_report
            End If

            Dim k_selectedKeys As New HashSet(Of String)(k_selected.Select(Function(s) s.Key), StringComparer.OrdinalIgnoreCase)
            For Each k_step In k_selected
                For Each k_needed In k_step.DependsOn
                    If k_selectedKeys.Contains(k_needed) Then Continue For
                    Dim k_other = k_steps.FirstOrDefault(Function(s) String.Equals(s.Key, k_needed, StringComparison.OrdinalIgnoreCase))
                    k_report.Add(AvcKinds.PAS_DEPENDENT, FindingClass.Atentie, String.Empty, String.Empty,
                                 $"«{k_step.Title}» are nevoie de «{If(k_other Is Nothing, k_needed, k_other.Title)}», care nu e bifat: " &
                                 "trebuie să fi fost migrat deja, altfel scrierea poate fi refuzată de cheile străine.")
                Next
            Next

            Using k_cn = k_context.Server.Open(k_context.TargetDatabase)
                For Each k_step In k_steps
                    k_step.ExistingRows.Clear()
                    For Each k_table In k_step.TargetTables
                        k_step.ExistingRows(k_table) = AvcSql.CountRows(k_cn, k_context.TargetDatabase, k_table)
                    Next
                Next

                For Each k_step In k_selected
                    If k_step.AccessRows < 0 Then
                        k_report.Add(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, k_step.AccessTable, String.Empty,
                                     $"«{k_step.Title}» nu a fost citit din Access. Apăsați «Citește» și rezolvați eroarea.")
                        Continue For
                    End If
                    Dim k_missing = k_step.TargetTables.Where(Function(t) k_step.ExistingRows(t) < 0).ToList()
                    For Each k_table In k_missing
                        k_report.Add(Finding.TABEL_LIPSA, FindingClass.Blocant, k_table, String.Empty,
                                     $"Tabelul «{k_table}» nu există în «{k_context.TargetDatabase}» (pasul «{k_step.Title}»). " &
                                     "Scriptul sql/AVACONT/... al pasului trebuie rulat pe AVACONT_SURSA, apoi «Sincronizare schemă».")
                    Next
                    If k_missing.Count > 0 Then Continue For
                    For Each k_finding In k_step.Verify(k_context, k_cn)
                        k_report.Add(k_finding)
                    Next
                Next
            End Using
            Return k_report
        Catch ex As Exception
            GlobalErrorLog.Write("AvcRunner.Verify", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Writes every ticked step inside ONE transaction. Returns what was written, or Nothing when the operator stopped it
    ''' before COMMIT (rollback confirmed). Server I/O: any failure rolls everything back and is rethrown.
    ''' </summary>
    Public Shared Function Run(k_context As AvcContext, k_steps As IReadOnlyList(Of AvcStep),
                               k_progress As IProgress(Of String), k_cancel As CancellationToken) As List(Of AvcWritten)
        Try
            RequireTarget(k_context)
            Dim k_selected = k_steps.Where(Function(s) s.Selected).ToList()
            If k_selected.Count = 0 Then Throw New InvalidOperationException("Nu este bifat niciun pas de migrat.")
            For Each k_entry In k_context.FileHashes
                If Not File.Exists(k_entry.Key) OrElse Not String.Equals(HashOf(k_entry.Key), k_entry.Value, StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidOperationException($"Fișierul Access «{k_entry.Key}» s-a modificat după citire. Citiți din nou și verificați.")
                End If
            Next
            If k_cancel.IsCancellationRequested Then Return Nothing

            Dim k_written As New List(Of AvcWritten)()
            Using k_dump As New SqlDumpWriter(k_context.JournalRoot, k_context.TargetDatabase, Sub(m) k_progress.Report(m))
                k_progress.Report("Jurnal SQL: " & k_dump.Folder)
                k_dump.WriteInfo({
                    "Aplicație: AVC.Migrator",
                    "Bază destinație: " & k_context.TargetDatabase,
                    "Cod fiscal: " & If(k_context.CodFiscal.Length = 0, "(necompletat)", k_context.CodFiscal),
                    "Server: " & k_context.Server.Describe(),
                    "Unități: " & String.Join(", ", k_context.Units.Select(Function(u) $"{u.IdUnitate} {u.NumeUnitate}")),
                    "Fișiere Access (SHA-256): " & String.Join("; ", k_context.FileHashes.Select(Function(h) $"{Path.GetFileName(h.Key)} {h.Value}")),
                    "An: " & k_context.Year.ToString(CultureInfo.InvariantCulture),
                    "Pași: " & String.Join(", ", k_selected.Select(Function(s) s.Key))})

                Using k_cn = k_context.Server.Open(k_context.TargetDatabase)
                    Using k_tx = k_cn.BeginTransaction()
                        Dim k_commitAttempted = False
                        Try
                            For Each k_step In k_selected
                                If RollbackIfStopped(k_tx, k_cancel, k_dump) Then Return Nothing
                                k_progress.Report($"Pasul «{k_step.Title}»...")
                                k_written.AddRange(k_step.Write(k_context, k_cn, k_tx, k_dump, k_progress, k_cancel))
                            Next
                            If RollbackIfStopped(k_tx, k_cancel, k_dump) Then Return Nothing
                            k_commitAttempted = True
                            k_tx.Commit()
                            k_dump.WriteFinal(True, k_written.Select(Function(w) $"{w.Table}: {w.Rows} rânduri"), Nothing)
                        Catch ex As OperationCanceledException
                            k_tx.Rollback()
                            k_dump.WriteFinal(False, {"Migrare oprită. Rollback confirmat."}, Nothing)
                            Return Nothing
                        Catch ex As Exception
                            Try
                                k_tx.Rollback()
                            Catch rollbackEx As Exception
                                k_dump.WriteComment("_outcome", "Rezultat necunoscut: rollback neconfirmat. Verificați serverul înainte de reluare.")
                                k_dump.FlushAll()
                                Throw New AggregateException("Rezultatul tranzacției nu poate fi confirmat. Verificați serverul înainte de reluare.", ex, rollbackEx)
                            End Try
                            ' A COMMIT that was sent but not acknowledged may have been applied.
                            If k_commitAttempted Then
                                k_dump.WriteComment("_outcome", "COMMIT neconfirmat. Verificați tabelele pe server înainte de reluare.")
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
            GlobalErrorLog.Write("AvcRunner.Run", ex)
            Throw
        End Try
    End Function

    Private Shared Sub RequireTarget(k_context As AvcContext)
        If k_context.Server Is Nothing OrElse String.IsNullOrWhiteSpace(k_context.TargetDatabase) Then
            Throw New InvalidOperationException("Serverul și baza unității trebuie completate.")
        End If
    End Sub

    ''' <summary>The normal stop path: confirm the rollback instead of throwing a cancellation into the debugger.</summary>
    Private Shared Function RollbackIfStopped(k_tx As MySqlTransaction, k_cancel As CancellationToken, k_dump As SqlDumpWriter) As Boolean
        If Not k_cancel.IsCancellationRequested Then Return False
        k_tx.Rollback()
        k_dump.WriteFinal(False, {"Migrare oprită. Rollback confirmat."}, Nothing)
        Return True
    End Function

End Class
