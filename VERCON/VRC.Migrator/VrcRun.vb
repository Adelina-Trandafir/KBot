Imports System.Text

''' <summary>Where the run is (SLICE-VC05-04). COMMIT sent is never claimed as cancelled: an unanswered COMMIT is <see cref="ResultUnknown"/>.</summary>
Public Enum VrcRunState
    Unread = 0
    PlanRead = 1
    Blocked = 2
    Tested = 3
    InImport = 4
    StopRequested = 5
    RollbackConfirmed = 6
    CommitConfirmed = 7
    ResultUnknown = 8
    Reconciled = 9
End Enum

''' <summary>One finding of the transfer; written to VC_MigrationIssues and to the report. Original values stay readable next to the target value.</summary>
Public NotInheritable Class VrcIssue

    Public Property SourceFile As String = String.Empty
    ''' <summary>VC_ImportSources.SourceId of the plan source (a file can feed two: Alimente and Materiale); 0 = none.</summary>
    Public Property SourceId As Long
    Public Property TableName As String = String.Empty
    Public Property SourceKey As String = String.Empty
    Public Property FieldName As String = String.Empty
    ''' <summary>N01, N02, N00, X01 (price sign), ... or a conversion code.</summary>
    Public Property RuleCode As String = String.Empty
    ''' <summary>BLOCAJ, AVERTIZARE or CONVERSIE (the values VC_MigrationIssues accepts).</summary>
    Public Property Severity As String = String.Empty
    Public Property OriginalValue As String = String.Empty
    Public Property TargetValue As String = String.Empty
    Public Property Justification As String = String.Empty

End Class

''' <summary>Source rows against target rows of one table in one context, after the write and before COMMIT.</summary>
Public NotInheritable Class VrcCount

    Public Property Gestiune As String = String.Empty
    Public Property TableName As String = String.Empty
    ''' <summary>Exercise year, or 0 for a gestiune-level table.</summary>
    Public Property Year As Integer
    Public Property SourceRows As Long
    Public Property SkippedRows As Long
    Public Property TargetRows As Long
    ''' <summary>Value columns whose converted sum differs from the sum read back from MariaDB.</summary>
    Public Property SumDifferences As New List(Of String)()

    Public ReadOnly Property Matches As Boolean
        Get
            Return SourceRows - SkippedRows = TargetRows AndAlso SumDifferences.Count = 0
        End Get
    End Property

End Class

Public NotInheritable Class VrcRunResult

    Public Property State As VrcRunState = VrcRunState.InImport
    Public Property Message As String = String.Empty
    Public Property SubunitId As Long
    Public Property ImportId As Long
    Public Property Issues As New List(Of VrcIssue)()
    Public Property Counts As New List(Of VrcCount)()
    ''' <summary>One line per partner prepared from ANAF: added in the transfer transaction, or why not.</summary>
    Public Property NewPartnerLines As New List(Of String)()
    Public Property Seconds As Double

    ''' <summary>The outcome in one clear line: success, rolled back (nothing written), unknown or critical.</summary>
    Public ReadOnly Property Verdict As String
        Get
            Select Case State
                Case VrcRunState.CommitConfirmed, VrcRunState.Reconciled
                    Return "SUCCES — transferul este confirmat (COMMIT); datele sunt pe server."
                Case VrcRunState.RollbackConfirmed, VrcRunState.StopRequested
                    Return "ANULAT (rollback) — transferul NU a reușit; nimic nu a rămas pe server, nici partenerii din ANAF."
                Case VrcRunState.ResultUnknown
                    Return "EROARE CRITICĂ — COMMIT trimis fără răspuns; verificați cu «Diagnostic». Nu repetați transferul."
                Case Else
                    Return "EROARE — transferul nu a pornit sau nu s-a terminat."
            End Select
        End Get
    End Property

    Public ReadOnly Property Blockers As Integer
        Get
            Return Issues.Where(Function(k_i) k_i.Severity = "BLOCAJ").Count()
        End Get
    End Property

    ''' <summary>Readable report of the run: result, counts per table, findings.</summary>
    Public Function ToMarkdown(k_plan As VrcPlan) As String
        Dim k_sb As New StringBuilder()
        k_sb.AppendLine(Verdict)
        k_sb.AppendLine()
        k_sb.AppendLine("# VRC.Migrator — raport de migrare")
        k_sb.AppendLine()
        k_sb.AppendLine($"- DC `{k_plan.Dc}`, subunitate «{k_plan.SubunitName.Trim()}», plan `{k_plan.Hash()}`.")
        k_sb.AppendLine($"- Stare: **{State}**. {Message}")
        k_sb.AppendLine($"- Durată: {Seconds:N1} s. ImportId: {ImportId}, SubunitId: {SubunitId}.")
        k_sb.AppendLine()
        k_sb.AppendLine("## Numărători (sursă față de țintă, înainte de COMMIT)")
        k_sb.AppendLine()
        k_sb.AppendLine("| Gestiune | Tabel | An | Rânduri sursă | Netransferate | Rânduri țintă | Sume | Rezultat |")
        k_sb.AppendLine("|---|---|---:|---:|---:|---:|---|---|")
        For Each k_c In Counts
            k_sb.AppendLine($"| {k_c.Gestiune} | {k_c.TableName} | {If(k_c.Year = 0, "—", k_c.Year.ToString(Globalization.CultureInfo.InvariantCulture))} | {k_c.SourceRows:N0} | {k_c.SkippedRows:N0} | {k_c.TargetRows:N0} | {If(k_c.SumDifferences.Count = 0, "egale", String.Join("; ", k_c.SumDifferences))} | {If(k_c.Matches, "ok", "**DIFERENȚĂ**")} |")
        Next
        k_sb.AppendLine()
        If NewPartnerLines.Count > 0 Then
            k_sb.AppendLine("## Parteneri din ANAF (confirmați sau anulați odată cu transferul, aceeași tranzacție)")
            k_sb.AppendLine()
            For Each k_line In NewPartnerLines
                k_sb.AppendLine("- " & k_line)
            Next
            k_sb.AppendLine()
        End If
        k_sb.AppendLine("## Constatări")
        k_sb.AppendLine()
        If Issues.Count = 0 Then
            k_sb.AppendLine("Nicio constatare.")
        Else
            For Each k_group In Issues.GroupBy(Function(k_i) k_i.Severity & "|" & k_i.RuleCode & "|" & k_i.TableName).OrderBy(Function(k_g) k_g.Key)
                Dim k_first = k_group.First()
                k_sb.AppendLine($"- **{k_first.Severity}** {k_first.RuleCode} în {k_first.TableName}: {k_group.Count():N0} cazuri. {k_first.Justification}")
                For Each k_i In k_group.Take(5)
                    k_sb.AppendLine($"  - {k_i.SourceFile}, cheie {k_i.SourceKey}{If(k_i.FieldName.Length > 0, ", " & k_i.FieldName, "")}: {k_i.OriginalValue} → {k_i.TargetValue}")
                Next
            Next
        End If
        Return k_sb.ToString()
    End Function

End Class
