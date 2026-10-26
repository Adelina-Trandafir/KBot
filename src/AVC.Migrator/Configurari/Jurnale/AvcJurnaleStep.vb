Imports System.Globalization
Imports System.Text.RegularExpressions
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>One journal of the Access <c>Jurnale</c> of one unit.</summary>
Friend NotInheritable Class AvcJurnalRow
    Public Property IdUnitate As Integer
    ''' <summary>The code as Access holds it (trimmed); the target writes it in capitals.</summary>
    Public Property Cod As String = String.Empty
    Public Property Denumire As String = String.Empty
    ''' <summary>The sector+source of the unit's subunit (set by the step; empty = the unit has no subunit).</summary>
    Public Property SursaSector As String = String.Empty

    Public ReadOnly Property CodTinta As String
        Get
            Return Cod.ToUpperInvariant()
        End Get
    End Property
End Class

''' <summary>Everything read from the Access <c>Jurnale</c> of the ticked units.</summary>
Friend NotInheritable Class AvcJurnaleSet
    Public ReadOnly Property Rows As New List(Of AvcJurnalRow)()
    Public ReadOnly Property UnitsWithoutTable As New List(Of Integer)()
    Public ReadOnly Property MissingColumns As New List(Of String)()
End Class

''' <summary>Reads the <c>Jurnale</c> of every ticked unit's file.</summary>
Friend NotInheritable Class AvcJurnaleReader

    Private Sub New()
    End Sub

    Public Const AccessTable As String = "Jurnale"

    ''' <summary>Access I/O: logs and rethrows.</summary>
    Public Shared Function Read(k_context As AvcContext) As AvcJurnaleSet
        Try
            Dim k_result As New AvcJurnaleSet()
            For Each k_unit In k_context.Units
                If Not k_unit.HasUnitFile Then Continue For
                Using k_cn = AccessProvider.Open(k_unit.UnitFilePath, k_context.AccessPassword)
                    Dim k_table = AccessSchema.ResolveTableName(k_cn, AccessTable)
                    If k_table Is Nothing Then
                        k_result.UnitsWithoutTable.Add(k_unit.IdUnitate)
                        Continue For
                    End If
                    Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
                        For Each k_column In {"CodJurnal", "DenumireJurnal"}
                            If Not k_reader.HasColumn(k_column) AndAlso Not k_result.MissingColumns.Contains(k_column) Then
                                k_result.MissingColumns.Add(k_column)
                            End If
                        Next
                        While k_reader.Read()
                            Dim k_cod As String = AvcSql.AsText(k_reader.ValueOrMissing("CodJurnal")).Trim()
                            If k_cod.Length = 0 Then Continue While
                            k_result.Rows.Add(New AvcJurnalRow() With {
                                .IdUnitate = k_unit.IdUnitate, .Cod = k_cod,
                                .Denumire = AvcSql.AsText(k_reader.ValueOrMissing("DenumireJurnal")).Trim()})
                        End While
                    End Using
                End Using
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcJurnaleReader.Read", ex)
            Throw
        End Try
    End Function

End Class

''' <summary>
''' Stage 1, step 4 (slice AV01-09): the Access <c>Jurnale</c> (code + name) becomes <c>TZ_Jurnale</c>, the journals of the unit database.
''' </summary>
''' <remarks>
''' <para>
''' The journals of a unit take the sector+source of ITS subunit (like its chart of accounts). The target is one list per unit database: the journals of the ticked
''' units are merged by sector+source and code (written in capitals). The same code at the same sector+source with different names in two files keeps the first name and is reported (ATENTIE). A code the target would refuse (1 to 10 of A-Z, 0-9, _)
''' blocks the run, because the window would never let the operator type it.
''' </para>
''' <para>
''' The write is an upsert that changes nothing on a repeat: a code that exists keeps the name the operator gave it in the window.
''' </para>
''' </remarks>
Friend NotInheritable Class AvcJurnaleStep
    Inherits AvcStep

    Private Shared ReadOnly CodRegex As New Regex("^[A-Z0-9_]{1,10}$", RegexOptions.CultureInvariant)
    Private Shared ReadOnly Columns As String() = {"SursaSector", "Cod", "Denumire"}

    Private _set As AvcJurnaleSet

    Public Overrides ReadOnly Property Key As String
        Get
            Return "Jurnale"
        End Get
    End Property

    Public Overrides ReadOnly Property Stage As String
        Get
            Return "Etapa 1 - Configurare"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "Jurnale"
        End Get
    End Property

    Public Overrides ReadOnly Property AccessTable As String
        Get
            Return AvcJurnaleReader.AccessTable
        End Get
    End Property

    Public Overrides ReadOnly Property TargetTables As IReadOnlyList(Of String)
        Get
            Return {"TZ_Jurnale"}
        End Get
    End Property

    Public Overrides ReadOnly Property DependsOn As IReadOnlyList(Of String)
        Get
            Return {"Subunitati"}
        End Get
    End Property

    Public Overrides Sub Reset()
        MyBase.Reset()
        _set = Nothing
    End Sub

    Public Overrides Sub ReadAccess(k_context As AvcContext)
        _set = AvcJurnaleReader.Read(k_context)
        AccessRows = _set.Rows.Count
        RowsToWrite = _set.Rows.Select(Function(k_r) k_r.CodTinta).Distinct(StringComparer.Ordinal).Count()
    End Sub

    Public Overrides Function Verify(k_context As AvcContext, k_cn As MySqlConnection) As List(Of Finding)
        Try
            Dim k_findings As New List(Of Finding)()
            If _set Is Nothing Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty,
                                           "«Jurnale» nu a fost citit din Access."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count = k_context.Units.Count Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty,
                                           "Fișierele Access ale unităților bifate nu au tabelul «Jurnale»."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count > 0 Then
                k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Informativ, AccessTable, String.Empty,
                                           "Fișierul Access nu are tabelul «Jurnale» la unitățile: " & String.Join(", ", _set.UnitsWithoutTable) & "."))
            End If
            If _set.MissingColumns.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.JURNAL_INVALID, FindingClass.Blocant, "TZ_Jurnale", String.Empty,
                                           "«Jurnale» din Access nu are coloanele: " & String.Join(", ", _set.MissingColumns) & "."))
                Return k_findings
            End If

            Dim k_db = k_context.TargetDatabase
            If Not AvcSql.TableExists(k_cn, k_db, "TZ_Jurnale") Then
                k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Blocant, "TZ_Jurnale", String.Empty,
                                           $"Tabelul «TZ_Jurnale» nu există în «{k_db}». Rulați sql/AVACONT/Configurari/Jurnale/AV01_09_jurnale.sql " &
                                           "pe AVACONT_SURSA și sincronizați schema."))
                Return k_findings
            End If
            For Each k_column In AvcSql.MissingColumns(k_cn, k_db, "TZ_Jurnale", Columns)
                k_findings.Add(New Finding(Finding.COLOANA_OBLIGATORIE, FindingClass.Blocant, "TZ_Jurnale", k_column,
                                           $"Coloana «{k_column}» nu există în «TZ_Jurnale» din «{k_db}»."))
            Next
            If k_findings.Any(Function(f) f.IsBlocking) Then Return k_findings
            If _set.Rows.Count = 0 Then
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_Jurnale", String.Empty,
                                           "«Jurnale» nu are niciun rând; nu se scrie nimic."))
                Return k_findings
            End If

            If Not ResolveSs(k_context, k_cn, Nothing, k_findings) Then Return k_findings

            Dim k_bad = _set.Rows.Where(Function(k_r) Not CodRegex.IsMatch(k_r.CodTinta)).
                Select(Function(k_r) $"unitatea {k_r.IdUnitate}: «{k_r.Cod}»").Distinct(StringComparer.Ordinal).ToList()
            If k_bad.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.JURNAL_INVALID, FindingClass.Blocant, "TZ_Jurnale", "Cod",
                                           "coduri de jurnal pe care fereastra nu le-ar primi (de la 1 la 10 caractere: litere, cifre, _)", k_bad.Count).
                    WithDetail(String.Join(vbLf, k_bad)))
            End If

            ' The same code in capitals with different names: one journal, the first name wins.
            Dim k_conflicts = _set.Rows.GroupBy(Function(k_r) k_r.SursaSector & "|" & k_r.CodTinta, StringComparer.Ordinal).
                Where(Function(k_g) k_g.Select(Function(k_r) k_r.Denumire).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1).
                Select(Function(k_g) $"{k_g.Key.Replace("|", " ")}: " & String.Join(" / ", k_g.Select(Function(k_r) $"«{k_r.Denumire}» (unitatea {k_r.IdUnitate})"))).ToList()
            If k_conflicts.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.JURNAL_INVALID, FindingClass.Atentie, "TZ_Jurnale", "Denumire",
                                           "același cod, la același sector și sursă, cu denumiri diferite în fișierele unităților; se păstrează prima denumire", k_conflicts.Count).
                    WithDetail(String.Join(vbLf, k_conflicts)))
            End If

            Dim k_codes = _set.Rows.Select(Function(k_r) k_r.SursaSector & "|" & k_r.CodTinta).Distinct(StringComparer.Ordinal).Count()
            k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_Jurnale", String.Empty,
                                       $"{k_codes} jurnale ▸ «TZ_Jurnale» (din {_set.Rows.Count} rânduri Access).", k_codes))

            Using k_cmd As New MySqlCommand("SELECT COUNT(*) FROM `TZ_Jurnale`", k_cn)
                Dim k_existing As Long = Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
                If k_existing > 0 Then
                    k_findings.Add(New Finding(Finding.RANDURI_EXISTENTE, FindingClass.Atentie, "TZ_Jurnale", "Cod",
                                               $"«TZ_Jurnale» are deja {k_existing} jurnale. Un cod existent rămâne cu denumirea lui (ce ați editat în fereastră nu se suprascrie); " &
                                               "se adaugă doar codurile care lipsesc.", CInt(Math.Min(k_existing, Integer.MaxValue))))
                End If
            End Using
            Return k_findings
        Catch ex As Exception
            GlobalErrorLog.Write("AvcJurnaleStep.Verify", ex)
            Throw
        End Try
    End Function

    ' Puts on every row the sector+source of its unit's subunit: the one in the target now, or the one the subunits step of this run will create.
    ' False (and a BLOCANT finding) when a unit that has journals has none. Server I/O: logs and rethrows.
    Private Function ResolveSs(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction, k_findings As List(Of Finding)) As Boolean
        Try
            Dim k_map = AvcSubunitStore.UnitMap(k_cn, k_transaction)
            Dim k_missing As New SortedSet(Of Integer)()
            For Each k_row In _set.Rows
                Dim k_sub As AvcSubunitRef = Nothing
                Dim k_planned As AvcSubunit = Nothing
                If k_map.TryGetValue(k_row.IdUnitate, k_sub) Then
                    k_row.SursaSector = k_sub.SursaSector
                ElseIf k_context.PlannedSubunits.TryGetValue(k_row.IdUnitate, k_planned) Then
                    k_row.SursaSector = k_planned.SursaSector
                Else
                    k_missing.Add(k_row.IdUnitate)
                End If
            Next
            If k_missing.Count > 0 AndAlso k_findings IsNot Nothing Then
                k_findings.Add(New Finding(AvcKinds.SUBUNITATE_LIPSA, FindingClass.Blocant, "TZ_Jurnale", "SursaSector",
                                           $"Jurnalele unităților {String.Join(", ", k_missing)} iau sectorul și sursa subunității lor, iar ele nu au încă subunitate. " &
                                           "Bifați pasul «Subunități» (le creează în aceeași migrare) și verificați din nou."))
            End If
            Return k_missing.Count = 0
        Catch ex As Exception
            GlobalErrorLog.Write("AvcJurnaleStep.ResolveSs", ex)
            Throw
        End Try
    End Function

    Public Overrides Function Write(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction,
                                    k_dump As SqlDumpWriter, k_progress As IProgress(Of String),
                                    k_cancel As CancellationToken) As List(Of AvcWritten)
        Try
            If _set Is Nothing Then Throw New InvalidOperationException("«Jurnale» nu a fost citit din Access.")
            If _set.Rows.Count = 0 Then
                k_progress.Report("«TZ_Jurnale»: «Jurnale» nu are rânduri, nimic de scris.")
                Return New List(Of AvcWritten) From {New AvcWritten("TZ_Jurnale", 0)}
            End If
            Dim k_unresolved As New List(Of Finding)()
            If Not ResolveSs(k_context, k_cn, k_transaction, k_unresolved) Then Throw New InvalidOperationException(k_unresolved(0).Message)
            k_dump.WriteComment("TZ_Jurnale", "Fostul «Jurnale»: sector+sursă (al subunității unității) + cod (litere mari) + denumire; la același cod se păstrează prima denumire.")

            Dim k_sql = $"INSERT INTO {AvcSql.Quote("TZ_Jurnale")} ({String.Join(", ", Columns.Select(AddressOf AvcSql.Quote))}) " &
                        $"VALUES ({String.Join(", ", Columns.Select(Function(c) "@" & c))}) " &
                        "ON DUPLICATE KEY UPDATE `IdJurnal` = LAST_INSERT_ID(`IdJurnal`)"
            Dim k_written As Integer = 0
            Using k_cmd As New MySqlCommand(k_sql, k_cn, k_transaction)
                For Each k_column In Columns
                    k_cmd.Parameters.Add(New MySqlParameter("@" & k_column, Nothing))
                Next
                For Each k_group In _set.Rows.GroupBy(Function(k_r) k_r.SursaSector & "|" & k_r.CodTinta, StringComparer.Ordinal).OrderBy(Function(k_g) k_g.Key, StringComparer.Ordinal)
                    k_cancel.ThrowIfCancellationRequested()
                    Dim k_values As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                        {"SursaSector", k_group.First().SursaSector},
                        {"Cod", k_group.First().CodTinta},
                        {"Denumire", k_group.First().Denumire}
                    }
                    For Each k_column In Columns
                        k_cmd.Parameters("@" & k_column).Value = k_values(k_column)
                    Next
                    k_dump.WriteStatement("TZ_Jurnale", AvcSql.RenderStatement(k_sql, k_values))
                    k_cmd.ExecuteNonQuery()
                    k_written += 1
                Next
            End Using
            k_dump.FlushAll()
            k_progress.Report($"«TZ_Jurnale»: {k_written} jurnale (confirmate la COMMIT).")
            Return New List(Of AvcWritten) From {New AvcWritten("TZ_Jurnale", k_written)}
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcJurnaleStep.Write", ex)
            Throw
        End Try
    End Function

End Class
