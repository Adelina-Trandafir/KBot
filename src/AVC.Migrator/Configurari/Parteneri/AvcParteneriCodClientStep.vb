Imports System.Globalization
Imports System.Text
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>One partner of the Access <c>Parteneri</c> that carries a client code.</summary>
Friend NotInheritable Class AvcCodClientRow
    Public Property IdUnitate As Integer
    Public Property CodPartener As String = String.Empty
    Public Property CodFiscal As String = String.Empty
    Public Property CodClient As String = String.Empty
End Class

''' <summary>The client codes of ONE unit that belong together: the same normalised CodFiscal (or, when the Access row has no CodFiscal, the same CodPartener).</summary>
Friend NotInheritable Class AvcCodClientGroup
    Public Property IdUnitate As Integer
    ''' <summary><c>F|</c> + the normalised CodFiscal, or <c>P|</c> + the CodPartener in capitals.</summary>
    Public Property Key As String = String.Empty
    ''' <summary>The CodFiscal as written in Access (first one seen), for the report.</summary>
    Public Property CodFiscal As String = String.Empty
    Public Property CodPartener As String = String.Empty
    ''' <summary>The distinct codes, in capital-insensitive ordinal order (so a repeat gives the same text).</summary>
    Public ReadOnly Property Codes As New List(Of String)()

    ''' <summary>The codes joined with «;» (the operator's rule).</summary>
    Public ReadOnly Property Joined As String
        Get
            Return String.Join(";", Codes)
        End Get
    End Property
End Class

''' <summary>Everything read from the Access <c>Parteneri</c> of the ticked units: only the rows with a client code are kept.</summary>
Friend NotInheritable Class AvcCodClientSet
    Public ReadOnly Property Rows As New List(Of AvcCodClientRow)()
    ''' <summary>All the partner rows of the Access files, with or without a code.</summary>
    Public Property TotalRows As Integer
    Public ReadOnly Property UnitsWithoutTable As New List(Of Integer)()
    ''' <summary>Units whose <c>Parteneri</c> has no <c>CodClient</c> column at all.</summary>
    Public ReadOnly Property UnitsWithoutColumn As New List(Of Integer)()
End Class

''' <summary>Reads the Access <c>Parteneri</c> (CodPartener, CodFiscal, CodClient) of every ticked unit's file.</summary>
Friend NotInheritable Class AvcCodClientReader

    Private Sub New()
    End Sub

    Public Const AccessTable As String = "Parteneri"

    ''' <summary>Access I/O: logs and rethrows.</summary>
    Public Shared Function Read(k_context As AvcContext) As AvcCodClientSet
        Try
            Dim k_result As New AvcCodClientSet()
            For Each k_unit In k_context.Units
                If Not k_unit.HasUnitFile Then Continue For
                ReadUnit(k_context, k_unit, k_result)
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcCodClientReader.Read", ex)
            Throw
        End Try
    End Function

    Private Shared Sub ReadUnit(k_context As AvcContext, k_unit As CaiUnit, k_result As AvcCodClientSet)
        Using k_cn = AccessProvider.Open(k_unit.UnitFilePath, k_context.AccessPassword)
            Dim k_table = AccessSchema.ResolveTableName(k_cn, AccessTable)
            If k_table Is Nothing Then
                k_result.UnitsWithoutTable.Add(k_unit.IdUnitate)
                Return
            End If
            Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
                Dim k_hasCode As Boolean = k_reader.HasColumn("CodClient")
                If Not k_hasCode Then k_result.UnitsWithoutColumn.Add(k_unit.IdUnitate)
                While k_reader.Read()
                    k_result.TotalRows += 1
                    If Not k_hasCode Then Continue While
                    Dim k_code As String = AvcSql.AsText(k_reader.ValueOrMissing("CodClient"))
                    If k_code.Length = 0 Then Continue While
                    k_result.Rows.Add(New AvcCodClientRow() With {
                        .IdUnitate = k_unit.IdUnitate,
                        .CodPartener = AvcSql.AsText(k_reader.ValueOrMissing("CodPartener")),
                        .CodFiscal = AvcSql.AsText(k_reader.ValueOrMissing("CodFiscal")),
                        .CodClient = k_code})
                End While
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' CodFiscal compared the same way on both sides: capitals, letters and digits only, and a leading «RO» in front of digits dropped
    ''' (RO 123-456 = 123456). Empty when nothing is left.
    ''' </summary>
    Friend Shared Function NormalizeCodFiscal(k_value As String) As String
        If String.IsNullOrWhiteSpace(k_value) Then Return String.Empty
        Dim k_text As New StringBuilder()
        For Each k_char As Char In k_value.ToUpperInvariant()
            If Char.IsLetterOrDigit(k_char) Then k_text.Append(k_char)
        Next
        Dim k_clean As String = k_text.ToString()
        If k_clean.Length > 2 AndAlso k_clean.StartsWith("RO", StringComparison.Ordinal) AndAlso k_clean.Substring(2).All(AddressOf Char.IsDigit) Then
            k_clean = k_clean.Substring(2)
        End If
        Return k_clean
    End Function

    ''' <summary>The key of a partner inside its unit: the CodFiscal when there is one, else the CodPartener.</summary>
    Friend Shared Function KeyOf(k_codFiscal As String, k_codPartener As String) As String
        Dim k_fiscal As String = NormalizeCodFiscal(k_codFiscal)
        If k_fiscal.Length > 0 Then Return "F|" & k_fiscal
        Return "P|" & If(k_codPartener, String.Empty).Trim().ToUpperInvariant()
    End Function

    ''' <summary>The Access rows grouped per unit and key; the codes of a group are distinct and sorted. Pure: no I/O.</summary>
    Friend Shared Function Group(k_rows As IEnumerable(Of AvcCodClientRow)) As List(Of AvcCodClientGroup)
        Dim k_groups As New Dictionary(Of String, AvcCodClientGroup)(StringComparer.Ordinal)
        Dim k_order As New List(Of AvcCodClientGroup)()
        For Each k_row In k_rows
            Dim k_key As String = KeyOf(k_row.CodFiscal, k_row.CodPartener)
            If k_key.Length <= 2 Then Continue For
            Dim k_id As String = k_row.IdUnitate.ToString(CultureInfo.InvariantCulture) & "|" & k_key
            Dim k_group As AvcCodClientGroup = Nothing
            If Not k_groups.TryGetValue(k_id, k_group) Then
                k_group = New AvcCodClientGroup() With {.IdUnitate = k_row.IdUnitate, .Key = k_key, .CodFiscal = k_row.CodFiscal, .CodPartener = k_row.CodPartener}
                k_groups(k_id) = k_group
                k_order.Add(k_group)
            End If
            If Not k_group.Codes.Contains(k_row.CodClient, StringComparer.OrdinalIgnoreCase) Then k_group.Codes.Add(k_row.CodClient)
        Next
        For Each k_group In k_order
            k_group.Codes.Sort(StringComparer.OrdinalIgnoreCase)
        Next
        Return k_order
    End Function

End Class

''' <summary>
''' Stage 1, an intermediate step (operator's decision, 10.11.2026): the Access <c>Parteneri.CodClient</c> becomes <c>Parteneri.CodClient</c> in MariaDB
''' (the column added by <c>sql/AVACONT/IntroducereDate/Operatii/AV02_04_parteneri_cod_client.sql</c>, right after <c>CodFiscal</c>). For most users the
''' Access column is empty and the step writes nothing.
''' </summary>
''' <remarks>
''' <para>
''' The partners meet by CodFiscal inside a unit: when several Access rows share one CodFiscal and carry different client codes, the codes are joined
''' with «;» and every target partner of the unit with that CodFiscal gets the joined text. A row with no CodFiscal meets the target by CodPartener.
''' </para>
''' <para>
''' The write changes nothing that is already filled: only a target <c>CodClient</c> that is empty is set, so a repeat adds what is missing and never
''' overwrites. The partners themselves come from KBot.Migrator (run it first); a code whose partner is not in <c>Parteneri</c> is not written and is listed.
''' </para>
''' </remarks>
Friend NotInheritable Class AvcParteneriCodClientStep
    Inherits AvcStep

    Private Const MaxLength As Integer = 255

    Private _set As AvcCodClientSet

    Public Overrides ReadOnly Property Key As String
        Get
            Return "ParteneriCodClient"
        End Get
    End Property

    Public Overrides ReadOnly Property Stage As String
        Get
            Return "Etapa 1 - Configurare"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "Cod client pe parteneri"
        End Get
    End Property

    Public Overrides ReadOnly Property AccessTable As String
        Get
            Return AvcCodClientReader.AccessTable
        End Get
    End Property

    Public Overrides ReadOnly Property TargetTables As IReadOnlyList(Of String)
        Get
            Return {"Parteneri"}
        End Get
    End Property

    Public Overrides Sub Reset()
        MyBase.Reset()
        _set = Nothing
    End Sub

    Public Overrides Sub ReadAccess(k_context As AvcContext)
        _set = AvcCodClientReader.Read(k_context)
        AccessRows = _set.TotalRows
        RowsToWrite = AvcCodClientReader.Group(_set.Rows).Count
    End Sub

    ''' <summary>One target partner that gets a client code.</summary>
    Private NotInheritable Class PlanEntry
        Public Property IdPartener As Long
        Public Property Value As String = String.Empty
        Public Property IdUnitate As Integer
    End Class

    ''' <summary>What a run would do, worked out from the Access groups and the target <c>Parteneri</c>.</summary>
    Private NotInheritable Class Plan
        Public ReadOnly Property Entries As New List(Of PlanEntry)()
        ''' <summary>Groups with no partner in the target.</summary>
        Public ReadOnly Property Unmatched As New List(Of String)()
        ''' <summary>Groups whose joined text is longer than the column.</summary>
        Public ReadOnly Property TooLong As New List(Of String)()
        ''' <summary>Groups that carried more than one code (joined with «;»).</summary>
        Public ReadOnly Property Joined As New List(Of String)()
        ''' <summary>Target partners that already have another client code and keep it.</summary>
        Public ReadOnly Property Kept As New List(Of String)()
    End Class

    ' Server I/O: logs and rethrows. The transaction is Nothing during the verification.
    Private Function BuildPlan(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction) As Plan
        Try
            Dim k_plan As New Plan()
            Dim k_groups = AvcCodClientReader.Group(_set.Rows)
            If k_groups.Count = 0 Then Return k_plan

            ' The target partners of the units that have a code: unit -> key -> partners.
            Dim k_index As New Dictionary(Of Integer, Dictionary(Of String, List(Of (IdPartener As Long, CodClient As String))))()
            For Each k_unitId In k_groups.Select(Function(k_g) k_g.IdUnitate).Distinct()
                Dim k_byKey As New Dictionary(Of String, List(Of (IdPartener As Long, CodClient As String)))(StringComparer.Ordinal)
                Using k_cmd As New MySqlCommand("SELECT `IdPartener`, `CodPartener`, `CodFiscal`, `CodClient` FROM `Parteneri` WHERE `IdUnitate` = @unit", k_cn, k_transaction)
                    k_cmd.Parameters.AddWithValue("@unit", k_unitId)
                    Using k_reader = k_cmd.ExecuteReader()
                        While k_reader.Read()
                            Dim k_partner As (IdPartener As Long, CodClient As String) = (Convert.ToInt64(k_reader("IdPartener"), CultureInfo.InvariantCulture), AvcSql.AsText(k_reader("CodClient")))
                            Dim k_fiscal As String = AvcCodClientReader.NormalizeCodFiscal(AvcSql.AsText(k_reader("CodFiscal")))
                            If k_fiscal.Length > 0 Then AddTo(k_byKey, "F|" & k_fiscal, k_partner)
                            Dim k_cod As String = AvcSql.AsText(k_reader("CodPartener")).ToUpperInvariant()
                            If k_cod.Length > 0 Then AddTo(k_byKey, "P|" & k_cod, k_partner)
                        End While
                    End Using
                End Using
                k_index(k_unitId) = k_byKey
            Next

            For Each k_group In k_groups
                Dim k_label As String = $"unitatea {k_context.UnitLabel(k_group.IdUnitate)}: {If(k_group.CodFiscal.Length > 0, "CodFiscal «" & k_group.CodFiscal & "»", "partener «" & k_group.CodPartener & "»")} ▸ «{k_group.Joined}»"
                If k_group.Codes.Count > 1 Then k_plan.Joined.Add(k_label)
                If k_group.Joined.Length > MaxLength Then
                    k_plan.TooLong.Add(k_label)
                    Continue For
                End If
                Dim k_matches As List(Of (IdPartener As Long, CodClient As String)) = Nothing
                If Not k_index(k_group.IdUnitate).TryGetValue(k_group.Key, k_matches) Then
                    k_plan.Unmatched.Add(k_label)
                    Continue For
                End If
                For Each k_partner In k_matches
                    If k_partner.CodClient.Length = 0 Then
                        k_plan.Entries.Add(New PlanEntry() With {.IdPartener = k_partner.IdPartener, .Value = k_group.Joined, .IdUnitate = k_group.IdUnitate})
                    ElseIf Not String.Equals(k_partner.CodClient, k_group.Joined, StringComparison.Ordinal) Then
                        k_plan.Kept.Add($"{k_label} (în «Parteneri» rămâne «{k_partner.CodClient}»)")
                    End If
                Next
            Next
            ' A partner reached through both of its keys is written once.
            Dim k_seen As New HashSet(Of Long)()
            k_plan.Entries.RemoveAll(Function(k_e) Not k_seen.Add(k_e.IdPartener))
            Return k_plan
        Catch ex As Exception
            GlobalErrorLog.Write("AvcParteneriCodClientStep.BuildPlan", ex)
            Throw
        End Try
    End Function

    Private Shared Sub AddTo(k_map As Dictionary(Of String, List(Of (IdPartener As Long, CodClient As String))), k_key As String, k_partner As (IdPartener As Long, CodClient As String))
        Dim k_list As List(Of (IdPartener As Long, CodClient As String)) = Nothing
        If Not k_map.TryGetValue(k_key, k_list) Then
            k_list = New List(Of (IdPartener As Long, CodClient As String))()
            k_map(k_key) = k_list
        End If
        k_list.Add(k_partner)
    End Sub

    Public Overrides Function Verify(k_context As AvcContext, k_cn As MySqlConnection) As List(Of Finding)
        Try
            Dim k_findings As New List(Of Finding)()
            If _set Is Nothing Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty, "«Parteneri» nu a fost citit din Access."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count > 0 Then
                k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Informativ, AccessTable, String.Empty,
                                           "Fișierul Access nu are tabelul «Parteneri» la unitățile: " & String.Join(", ", _set.UnitsWithoutTable.Select(Function(k_uid) k_context.UnitLabel(k_uid))) & "."))
            End If
            If _set.UnitsWithoutColumn.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.COD_CLIENT, FindingClass.Informativ, AccessTable, "CodClient",
                                           "«Parteneri» din Access nu are coloana «CodClient» la unitățile: " & String.Join(", ", _set.UnitsWithoutColumn.Select(Function(k_uid) k_context.UnitLabel(k_uid))) & "; nu se scrie nimic pentru ele."))
            End If
            If _set.Rows.Count = 0 Then
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "Parteneri", "CodClient",
                                           "Niciun partener nu are cod client în Access; nu se scrie nimic."))
                RowsToWrite = 0
                Return k_findings
            End If

            Dim k_db = k_context.TargetDatabase
            For Each k_column In AvcSql.MissingColumns(k_cn, k_db, "Parteneri", {"IdPartener", "IdUnitate", "CodPartener", "CodFiscal", "CodClient"})
                k_findings.Add(New Finding(Finding.COLOANA_OBLIGATORIE, FindingClass.Blocant, "Parteneri", k_column,
                                           $"Coloana «{k_column}» nu există în «Parteneri» din «{k_db}». Rulați sql/AVACONT/IntroducereDate/Operatii/AV02_04_parteneri_cod_client.sql " &
                                           "pe AVACONT_SURSA, apoi «Sincronizare schemă» pe baza unității."))
            Next
            If k_findings.Any(Function(f) f.IsBlocking) Then Return k_findings

            Dim k_plan = BuildPlan(k_context, k_cn, Nothing)
            RowsToWrite = k_plan.Entries.Count
            For Each k_unitId In _set.Rows.Select(Function(k_r) k_r.IdUnitate).Distinct().OrderBy(Function(u) u)
                Dim k_count As Integer = k_plan.Entries.Where(Function(k_e) k_e.IdUnitate = k_unitId).Count()
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "Parteneri", "CodClient",
                                           $"Unitatea {k_context.UnitLabel(k_unitId)}: {k_count} parteneri primesc cod client ▸ «Parteneri.CodClient».", k_count))
            Next
            If k_plan.Joined.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.COD_CLIENT, FindingClass.Informativ, "Parteneri", "CodClient",
                                           "coduri fiscale cu mai multe coduri client; codurile se unesc cu «;»", k_plan.Joined.Count).WithDetail(String.Join(vbLf, k_plan.Joined)))
            End If
            If k_plan.Kept.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.COD_CLIENT, FindingClass.Informativ, "Parteneri", "CodClient",
                                           "parteneri care au deja un cod client diferit în «Parteneri»; rămân neschimbați", k_plan.Kept.Count).WithDetail(String.Join(vbLf, k_plan.Kept)))
            End If
            If k_plan.TooLong.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.COD_CLIENT, FindingClass.Atentie, "Parteneri", "CodClient",
                                           $"coduri client unite care depășesc {MaxLength} de caractere; NU se scriu", k_plan.TooLong.Count).WithDetail(String.Join(vbLf, k_plan.TooLong)))
            End If
            If k_plan.Unmatched.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.PARTENER_LIPSA, FindingClass.Atentie, "Parteneri", "CodClient",
                                           "coduri client al căror partener nu există în «Parteneri» (rulați întâi KBot.Migrator, pasul Parteneri); NU se scriu", k_plan.Unmatched.Count).WithDetail(String.Join(vbLf, k_plan.Unmatched)))
            End If
            Return k_findings
        Catch ex As Exception
            GlobalErrorLog.Write("AvcParteneriCodClientStep.Verify", ex)
            Throw
        End Try
    End Function

    Public Overrides Function Write(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction,
                                    k_dump As SqlDumpWriter, k_progress As IProgress(Of String),
                                    k_cancel As CancellationToken) As List(Of AvcWritten)
        Try
            If _set Is Nothing Then Throw New InvalidOperationException("«Parteneri» nu a fost citit din Access.")
            If _set.Rows.Count = 0 Then
                k_progress.Report("«Parteneri.CodClient»: niciun partener nu are cod client în Access, nimic de scris.")
                Return New List(Of AvcWritten) From {New AvcWritten("Parteneri", 0)}
            End If
            Dim k_plan = BuildPlan(k_context, k_cn, k_transaction)
            k_dump.WriteComment("Parteneri", "Cod client din Access «Parteneri.CodClient»; mai multe coduri pe același CodFiscal se unesc cu «;»; se completează doar ce e gol.")
            Dim k_sql As String = "UPDATE `Parteneri` SET `CodClient` = @CodClient WHERE `IdPartener` = @IdPartener AND (`CodClient` IS NULL OR `CodClient` = '')"
            Dim k_written As Integer = 0
            Using k_cmd As New MySqlCommand(k_sql, k_cn, k_transaction)
                k_cmd.Parameters.Add(New MySqlParameter("@CodClient", Nothing))
                k_cmd.Parameters.Add(New MySqlParameter("@IdPartener", Nothing))
                For Each k_entry In k_plan.Entries
                    k_cancel.ThrowIfCancellationRequested()
                    k_cmd.Parameters("@CodClient").Value = k_entry.Value
                    k_cmd.Parameters("@IdPartener").Value = k_entry.IdPartener
                    Dim k_values As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {{"CodClient", k_entry.Value}, {"IdPartener", k_entry.IdPartener}}
                    k_dump.WriteStatement("Parteneri", AvcSql.RenderStatement(k_sql, k_values))
                    k_written += k_cmd.ExecuteNonQuery()
                    k_context.Tick(k_written, k_plan.Entries.Count, "«Parteneri»: cod client")
                Next
            End Using
            k_dump.FlushAll()
            k_progress.Report($"«Parteneri.CodClient»: {k_written} parteneri au primit cod client (confirmat la COMMIT).")
            Return New List(Of AvcWritten) From {New AvcWritten("Parteneri", k_written)}
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcParteneriCodClientStep.Write", ex)
            Throw
        End Try
    End Function

End Class
