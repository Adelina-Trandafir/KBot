Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>
''' Stage 1, first step: every ticked unit that is not yet a subunit of the unit database becomes one (<c>TZ_Subunitati</c>), with the
''' denomination, chapter and salary decimals the operator saw and corrected in the unit grid. It runs before the chart of accounts, in the
''' same transaction, because each unit's chart goes into its own subunit. Units that are already subunits are left as they are.
''' </summary>
''' <remarks>
''' The Access source is the registry itself (<c>cale.accdb</c> ▸ <c>cai</c>), already read by the form; the drafts arrive in
''' <see cref="AvcContext.SubunitDrafts"/>. What the server holds is read again at verification and at the write, so a subunit added
''' in between is never added twice.
''' </remarks>
Friend NotInheritable Class AvcSubunitsStep
    Inherits AvcStep

    Public Overrides ReadOnly Property Key As String
        Get
            Return "Subunitati"
        End Get
    End Property

    Public Overrides ReadOnly Property Stage As String
        Get
            Return "Etapa 1 - Configurare"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "Subunități (câte una pe unitate)"
        End Get
    End Property

    Public Overrides ReadOnly Property AccessTable As String
        Get
            Return "cai (cale.accdb)"
        End Get
    End Property

    Public Overrides ReadOnly Property TargetTables As IReadOnlyList(Of String)
        Get
            Return {"TZ_Subunitati"}
        End Get
    End Property

    Public Overrides Sub ReadAccess(k_context As AvcContext)
        AccessRows = k_context.Units.Count
        RowsToWrite = k_context.Units.Count
    End Sub

    ''' <summary>The drafts of the ticked units the server does not hold as subunits yet. A ticked unit without a draft is a bug of the caller.</summary>
    Private Shared Function Pending(k_context As AvcContext, k_existing As IEnumerable(Of AvcSubunit)) As List(Of AvcSubunit)
        Dim k_ported As New HashSet(Of Integer)(k_existing.Where(Function(k_s) k_s.IdUnitateAccess > 0).Select(Function(k_s) k_s.IdUnitateAccess))
        Dim k_result As New List(Of AvcSubunit)()
        For Each k_unit In k_context.Units
            If k_ported.Contains(k_unit.IdUnitate) Then Continue For
            Dim k_unitId = k_unit.IdUnitate
            Dim k_draft = k_context.SubunitDrafts.FirstOrDefault(Function(k_d) k_d.IdUnitateAccess = k_unitId)
            If k_draft Is Nothing Then
                Throw New InvalidOperationException($"Unitatea {AvcSubunitRules.LabelOf(k_unit)} nu are datele subunității (denumire, capitol, zecimale).")
            End If
            k_result.Add(k_draft)
        Next
        Return k_result
    End Function

    Public Overrides Function Verify(k_context As AvcContext, k_cn As MySqlConnection) As List(Of Finding)
        Try
            Dim k_findings As New List(Of Finding)()
            k_context.PlannedSubunits.Clear()
            Dim k_db = k_context.TargetDatabase
            For Each k_column In AvcSql.MissingColumns(k_cn, k_db, "TZ_Subunitati", AvcSubunitStore.LinkColumns)
                k_findings.Add(New Finding(Finding.COLOANA_OBLIGATORIE, FindingClass.Blocant, "TZ_Subunitati", k_column,
                                           $"Coloana «{k_column}» nu există în «TZ_Subunitati» din «{k_db}». Rulați sql/AVACONT/Configurari/PlanCont/" &
                                           "AV01_04d_subunitati_unitate_parinte.sql pe AVACONT_SURSA, apoi «Sincronizare schemă» pe baza unității."))
            Next
            If k_findings.Count > 0 Then Return k_findings

            Dim k_existing = AvcSubunitStore.List(k_cn)
            Dim k_todo = Pending(k_context, k_existing)
            RowsToWrite = k_todo.Count
            If k_todo.Count = 0 Then
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_Subunitati", String.Empty,
                                           "Toate unitățile bifate sunt deja subunități; nu se adaugă nimic."))
                Return k_findings
            End If

            Dim k_unitOf = k_context.Units.ToDictionary(Function(k_u) k_u.IdUnitate)
            Dim k_sectors = AvcSubunitStore.SectorSources(k_cn, k_context.CommonDatabase)
            Dim k_reason = AvcSubunitRules.Check(k_todo, k_unitOf, k_context.DcUnits, k_existing, k_sectors, k_context.CommonDatabase)
            If k_reason Is Nothing AndAlso AvcSubunitRules.ParentsFirst(k_todo, k_existing) Is Nothing Then
                k_reason = "Legăturile de părinte dintre unitățile bifate se rotesc în cerc (o unitate ar fi propriul ei strămoș). Corectați-le în registru."
            End If
            If k_reason IsNot Nothing Then
                k_findings.Add(New Finding(AvcKinds.SUBUNITATE_INVALIDA, FindingClass.Blocant, "TZ_Subunitati", String.Empty,
                                           k_reason & " Corectați valoarea în tabelul de unități."))
                Return k_findings
            End If

            For Each k_draft In k_todo
                k_context.PlannedSubunits(k_draft.IdUnitateAccess) = k_draft
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_Subunitati", String.Empty,
                                           $"Unitatea {AvcSubunitRules.LabelOf(k_unitOf(k_draft.IdUnitateAccess))} devine subunitatea «{k_draft.Denumire}» " &
                                           $"(capitolul {k_draft.Capitol}, sursa {k_draft.SursaSector}, zecimale salarii {k_draft.Zecimale}) ▸ «TZ_Subunitati».", 1))
            Next
            Return k_findings
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSubunitsStep.Verify", ex)
            Throw
        End Try
    End Function

    Public Overrides Function Write(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction,
                                    k_dump As SqlDumpWriter, k_progress As IProgress(Of String),
                                    k_cancel As CancellationToken) As List(Of AvcWritten)
        Try
            k_cancel.ThrowIfCancellationRequested()
            Dim k_existing = AvcSubunitStore.List(k_cn, k_transaction)
            Dim k_todo = Pending(k_context, k_existing)
            If k_todo.Count = 0 Then
                k_progress.Report("«TZ_Subunitati»: toate unitățile bifate sunt deja subunități, nimic de scris.")
                Return New List(Of AvcWritten) From {New AvcWritten("TZ_Subunitati", 0)}
            End If

            Dim k_ordered = AvcSubunitRules.ParentsFirst(k_todo, k_existing)
            If k_ordered Is Nothing Then Throw New InvalidOperationException("Legăturile de părinte dintre unitățile bifate se rotesc în cerc.")
            Dim k_sectors As New HashSet(Of String)(AvcSubunitStore.SectorSources(k_cn, k_context.CommonDatabase, k_transaction), StringComparer.OrdinalIgnoreCase)
            k_dump.WriteComment("TZ_Subunitati", "Câte o subunitate pentru fiecare unitate bifată care nu era încă una; planul ei de conturi intră în ea.")
            Dim k_count = AvcSubunitStore.AddInTransaction(k_cn, k_transaction, k_context.CommonDatabase, k_ordered, k_sectors,
                                                           Sub(k_sql, k_values) k_dump.WriteStatement("TZ_Subunitati", AvcSql.RenderStatement(k_sql, k_values)))
            k_dump.FlushAll()
            k_progress.Report($"«TZ_Subunitati»: {k_count} subunități (confirmate la COMMIT).")
            Return New List(Of AvcWritten) From {New AvcWritten("TZ_Subunitati", k_count)}
        Catch ex As OperationCanceledException
            Throw
        Catch ex As InvalidOperationException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSubunitsStep.Write", ex)
            Throw
        End Try
    End Function

End Class
