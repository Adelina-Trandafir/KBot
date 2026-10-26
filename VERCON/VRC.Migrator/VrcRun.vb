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
    ''' <summary>Full path of the source file (report only; VC_MigrationIssues keeps the name).</summary>
    Public Property SourceFullPath As String = String.Empty
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

''' <summary>One subunit created by the transfer: its name and the ids of the subunit and of its import marker.</summary>
Public NotInheritable Class VrcSubunitResult

    Public Property Name As String = String.Empty
    Public Property SubunitId As Long
    Public Property ImportId As Long

End Class

Public NotInheritable Class VrcRunResult

    Public Property State As VrcRunState = VrcRunState.InImport
    Public Property Message As String = String.Empty
    ''' <summary>Id of the FIRST subunit (the report lists all in <see cref="Subunits"/>).</summary>
    Public Property SubunitId As Long
    ''' <summary>Import marker of the first subunit.</summary>
    Public Property ImportId As Long
    ''' <summary>Every subunit of the transfer (one per name in the plan), each with its own marker.</summary>
    Public Property Subunits As New List(Of VrcSubunitResult)()
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

    ''' <summary>
    ''' Readable report of the run, written for the operator (18.10.2026): the result in one sentence, then each problem to fix in plain words
    ''' (where, what is wrong, what to do, the Access query), then what is only informative. No ids, no rule codes, no empty tables.
    ''' </summary>
    Public Function ToMarkdown(k_plan As VrcPlan) As String
        Dim k_sb As New StringBuilder()
        Dim k_committed As Boolean = State = VrcRunState.CommitConfirmed OrElse State = VrcRunState.Reconciled
        Dim k_rolledBack As Boolean = State = VrcRunState.RollbackConfirmed OrElse State = VrcRunState.StopRequested
        Dim k_blocking = Issues.Where(Function(k_i) k_i.Severity = "BLOCAJ").ToList()
        Dim k_groups = k_blocking.GroupBy(Function(k_i) k_i.RuleCode & "|" & k_i.TableName & "|" & k_i.FieldName, StringComparer.OrdinalIgnoreCase).
                                  OrderBy(Function(k_g) k_g.Key, StringComparer.OrdinalIgnoreCase).ToList()
        Dim k_unitNames = k_plan.SubunitNames()

        k_sb.AppendLine(Verdict)
        k_sb.AppendLine()
        k_sb.AppendLine("# Raport de migrare")
        k_sb.AppendLine()
        k_sb.AppendLine($"Baza `{k_plan.Dc}` · {k_unitNames.Count} {If(k_unitNames.Count = 1, "subunitate", "subunități")} · durată {Seconds:N1} s")
        k_sb.AppendLine()
        If Not k_committed Then
            k_sb.AppendLine("Subunități: " & String.Join(", ", k_unitNames.Select(Function(k_unitName) $"«{k_unitName}»")) & ".")
            k_sb.AppendLine()
        End If

        If k_groups.Count > 0 Then
            Dim k_fileCount As Integer = k_blocking.Select(Function(k_i) PathOf(k_i)).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            k_sb.AppendLine($"**{k_groups.Count} {If(k_groups.Count = 1, "problemă", "probleme")} de rezolvat** ({Cases(k_blocking.Count)} în {k_fileCount} {If(k_fileCount = 1, "fișier", "fișiere")}). " &
                            "Corectați datele în Access, apoi apăsați din nou «Citește bifatele» și «Migrează».")
            k_sb.AppendLine()
        ElseIf Message.Length > 0 Then
            k_sb.AppendLine(Message)
            k_sb.AppendLine()
        End If

        If k_committed Then
            k_sb.AppendLine("## Subunități create")
            k_sb.AppendLine()
            For Each k_unit In k_unitNames
                Dim k_made = Subunits.FirstOrDefault(Function(k_x) String.Equals(k_x.Name, k_unit, StringComparison.OrdinalIgnoreCase))
                k_sb.AppendLine($"- «{k_unit}», ZEC {k_plan.ZecOf(k_unit)}{If(k_made Is Nothing, "", $" (SubunitId {k_made.SubunitId}, ImportId {k_made.ImportId})")}")
            Next
            k_sb.AppendLine()
        End If

        If NewPartnerLines.Count > 0 Then
            If k_rolledBack Then
                k_sb.AppendLine($"Parteneri din ANAF: {NewPartnerLines.Count} erau pregătiți, dar nu au fost adăugați (transferul a fost anulat).")
                k_sb.AppendLine()
            Else
                k_sb.AppendLine("## Parteneri din ANAF (adăugați în aceeași tranzacție)")
                k_sb.AppendLine()
                For Each k_line In NewPartnerLines
                    k_sb.AppendLine("- " & k_line)
                Next
                k_sb.AppendLine()
            End If
        End If

        If k_groups.Count > 0 Then
            k_sb.AppendLine("## Ce trebuie rezolvat")
            k_sb.AppendLine()
            Dim k_number As Integer = 0
            For Each k_group In k_groups
                k_number += 1
                AppendProblem(k_sb, k_number, k_group.ToList())
            Next
        End If

        If Counts.Count > 0 Then
            k_sb.AppendLine("## Verificarea numărătorilor (sursă față de țintă, înainte de COMMIT)")
            k_sb.AppendLine()
            k_sb.AppendLine("| Gestiune | Tabel | An | Rânduri sursă | Netransferate | Rânduri țintă | Sume | Rezultat |")
            k_sb.AppendLine("|---|---|---:|---:|---:|---:|---|---|")
            For Each k_c In Counts
                k_sb.AppendLine($"| {k_c.Gestiune} | {k_c.TableName} | {If(k_c.Year = 0, "—", k_c.Year.ToString(Globalization.CultureInfo.InvariantCulture))} | {k_c.SourceRows:N0} | {k_c.SkippedRows:N0} | {k_c.TargetRows:N0} | {If(k_c.SumDifferences.Count = 0, "egale", String.Join("; ", k_c.SumDifferences))} | {If(k_c.Matches, "ok", "**DIFERENȚĂ**")} |")
            Next
            k_sb.AppendLine()
        End If

        AppendSignCheck(k_sb)
        AppendInformative(k_sb)
        If k_groups.Count = 0 AndAlso Issues.Count = 0 Then k_sb.AppendLine("Nicio constatare.")
        Return k_sb.ToString()
    End Function

    ''' <summary>One problem to fix: title, what is wrong, where (files, with a few example rows each), what to do, the Access query.</summary>
    Private Shared Sub AppendProblem(k_sb As StringBuilder, k_number As Integer, k_issues As List(Of VrcIssue))
        Dim k_first = k_issues(0)
        Dim k_text = Describe(k_issues)
        Dim k_pk As String = KeyNameOf(k_first.TableName)
        k_sb.AppendLine($"### {k_number}. {k_text.Title}")
        k_sb.AppendLine()
        k_sb.AppendLine(k_text.What)
        k_sb.AppendLine()
        k_sb.AppendLine("**Unde:**")
        k_sb.AppendLine()
        For Each k_file In k_issues.GroupBy(Function(k_i) PathOf(k_i), StringComparer.OrdinalIgnoreCase)
            k_sb.AppendLine($"- `{k_file.Key}` ({Cases(k_file.Count())}){FolderLink(k_file.Key)}")
            For Each k_i In k_file.Take(5)
                k_sb.AppendLine("    - " & ExampleOf(k_i, k_pk))
            Next
            If k_file.Count() > 5 Then k_sb.AppendLine($"    - … și încă {k_file.Count() - 5:N0}")
        Next
        k_sb.AppendLine()
        k_sb.AppendLine($"**Ce faceți:** {k_text.Todo}")
        k_sb.AppendLine()
        Dim k_sql As String = VrcAccessSql.ForField(k_issues)
        If k_sql.Length > 0 Then
            k_sb.AppendLine("**Interogare Access** ca să vedeți rândurile (în fișierul de mai sus: Creare > Proiectare interogare > Vizualizare SQL):")
            k_sb.AppendLine()
            k_sb.AppendLine("```sql")
            For Each k_line In k_sql.Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
                k_sb.AppendLine(k_line)
            Next
            k_sb.AppendLine("```")
            k_sb.AppendLine()
        End If
    End Sub

    ''' <summary>«ColumnKey = 4: Field = 2»: one affected row in words the operator can look up in Access.</summary>
    Private Shared Function ExampleOf(k_issue As VrcIssue, k_pk As String) As String
        Dim k_where As String = If(k_pk.Length > 0, $"{k_pk} = {k_issue.SourceKey}", $"rândul {k_issue.SourceKey}")
        ' Account / origin findings carry their own sentence per row (it names the position, product and bon): show it instead of «IDM = 2».
        If k_issue.RuleCode = "X16" Then Return k_issue.Justification
        If k_issue.RuleCode = "X15" OrElse k_issue.RuleCode = "X17" Then Return $"{k_where}: {k_issue.Justification}"
        If k_issue.FieldName.Length = 0 OrElse k_issue.OriginalValue.Length = 0 Then Return k_where
        Return $"{k_where}: {k_issue.FieldName} = {k_issue.OriginalValue}"
    End Function

    ''' <summary>The title, the explanation and the advice for one group of blocking findings (same rule, table and field), in plain Romanian.</summary>
    Private Shared Function Describe(k_issues As List(Of VrcIssue)) As (Title As String, What As String, Todo As String)
        Dim k_first = k_issues(0)
        Dim k_table As String = k_first.TableName
        Dim k_field As String = k_first.FieldName
        Dim k_rows As String = If(k_issues.Count = 1, "1 rând", $"{k_issues.Count:N0} rânduri")
        Dim k_column = ColumnOf(k_table, k_field)
        Select Case k_first.RuleCode
            Case "X05"
                If String.Equals(k_first.OriginalValue, "NULL", StringComparison.OrdinalIgnoreCase) Then
                    Return ($"{k_table}: {k_field} este gol, dar este obligatoriu",
                            $"{k_rows} din {k_table} au câmpul {k_field} gol, iar MariaDB nu acceptă gol în această coloană.",
                            "Completați valoarea în Access.")
                End If
                If k_column IsNot Nothing AndAlso k_column.Link = VrcLinkKind.Origin Then
                    Return ($"{k_table}: {k_field} nu arată spre niciun bon",
                            $"{k_rows} din {k_table} au {k_field} (bonul din care a plecat obiectul) cu un număr care nu se găsește nici în OI_Predare, nici în OI_Transfer.",
                            "Corectați valoarea sau adăugați bonul lipsă. Bonul poate fi într-un alt fișier al aceleiași gestiuni: atunci bifați și acel fișier.")
                End If
                Dim k_parent As String = If(k_column Is Nothing, String.Empty, k_column.Parent)
                Dim k_parentKey As String = If(k_column Is Nothing, String.Empty, k_column.ParentField)
                If k_parentKey.Length = 0 AndAlso k_parent.Length > 0 Then k_parentKey = KeyNameOf(k_parent)
                Dim k_in As String = If(k_parent.Length = 0, "în tabelul-părinte",
                                     If(String.Equals(k_parent, k_table, StringComparison.OrdinalIgnoreCase), $"tot în {k_table}", $"în {k_parent}")) &
                                     If(k_parentKey.Length > 0, $" (coloana {k_parentKey})", String.Empty)
                Return ($"{k_table}: {k_field} arată spre un rând care nu există",
                        $"{k_rows} din {k_table} au {k_field} completat cu un număr care nu se găsește {k_in}.",
                        $"Corectați {k_field} la rândurile de mai jos, sau goliți-l dacă legătura nu are rost. Rândul căutat poate fi într-un alt fișier al aceleiași gestiuni: atunci bifați și acel fișier.")
            Case "X10"
                Return ($"{k_table}: {k_field} are valori, dar nu se știe spre ce arată",
                        $"{k_rows} din {k_table} au {k_field} diferit de 0, iar migratorul nu are încă o regulă pentru această legătură, deci nu poate transfera valoarea.",
                        $"Nu se rezolvă corectând un singur rând: trebuie hotărât ce reprezintă {k_table}.{k_field}. Dacă valorile sunt rămășițe fără rost, le puteți pune pe 0 în Access.")
            Case "X14"
                Return ($"{k_table}: {k_field} poate însemna două lucruri",
                        $"{k_rows} din {k_table} au {k_field} cu un număr care există și ca bon de predare (OI_Predare), și ca bon de transfer (OI_Transfer); nu se știe din care a plecat obiectul.",
                        "Hotărâți dacă numărul înseamnă bon de predare sau bon de transfer, înainte de transfer.")
            Case "X15"
                Return ($"{k_table}: bonul de pe poziții lipsește sau este greșit",
                        $"{k_rows} din {k_table} au bonul curent sau bonul de origine lipsă ori completat greșit. Motivul este scris la fiecare rând de mai jos.",
                        "Corectați bonul la rândurile de mai jos, în Access.")
            Case "X16"
                Dim k_origins = If(String.Equals(k_field, "IDBO", StringComparison.OrdinalIgnoreCase),
                                   k_issues.Select(Function(k_i) k_i.OriginalValue).Where(Function(k_o) k_o.Length > 0).Distinct().
                                            OrderBy(Function(k_o) k_o.PadLeft(12, "0"c), StringComparer.Ordinal).ToList(),
                                   New List(Of String)())
                Dim k_originText As String = If(k_origins.Count = 0, String.Empty,
                    $" Bonuri de origine în cauză: {String.Join(", ", k_origins.Take(15))}{If(k_origins.Count > 15, ", …", String.Empty)}.")
                If Not String.Equals(k_field, "IDBO", StringComparison.OrdinalIgnoreCase) Then
                    Return ($"{k_table}: liniile nu se potrivesc cu pozițiile din registrul OI",
                            $"{k_rows} din {k_table} nu se potrivesc cu nicio poziție din OI_Produse (același bon, același produs, același preț), deci migratorul nu poate stabili în ce cont erau obiectele. Motivul exact este scris la fiecare rând de mai jos.",
                            "Deschideți OI_Produse și căutați pozițiile bonului și produsului de pe rândurile de mai jos; verificați produsul, prețul și semnul cantității.")
                End If
                Return ($"{k_table}: nu se poate afla contul obiectelor scoase",
                        $"{k_rows} din {k_table} sunt scoateri (transfer sau casare) la care migratorul nu a putut afla în ce cont se afla obiectul. Contul se ia de la primirea inițială (linia din bonul de predare cu același produs), urmărind înapoi bonurile de origine; aici lanțul se rupe.{k_originText} Motivul exact este scris la fiecare rând de mai jos.",
                        "Deschideți în Access tabelul OI_Predare și căutați bonul de mai sus (coloana IDBP = numărul bonului): trebuie să aibă o linie cu același produs ca poziția scoasă. Dacă produsul de pe poziția scoasă (OI_Produse, coloana IdProdus) sau cel de pe bon este greșit, corectați-l. Dacă bonul de origine este de fapt un transfer, căutați-l în OI_Transfer.")
            Case "X17"
                Return ($"{k_table}: bonul de origine șters nu poate fi înlocuit automat",
                        $"{k_rows} din {k_table} arată spre un bon de origine care nu mai există în Access. Migratorul a căutat singur primirea potrivită, dar nu a găsit exact una. Motivul este scris la fiecare rând de mai jos.",
                        "Alegeți voi bonul corect: la rândurile de mai jos completați în Access câmpul IDBO cu numărul bonului de predare (sau de transfer) din care a plecat obiectul.")
            Case "X11"
                Return ($"{k_table}: {k_field} are o valoare prea mare",
                        $"{k_rows} din {k_table} au {k_field} cu o valoare pe care MariaDB nu o poate păstra. {k_first.Justification}",
                        "Corectați valoarea în Access (probabil o greșeală de tastare).")
            Case "X12"
                Return ($"{k_table}: {k_field} trebuie să fie număr întreg",
                        $"{k_rows} din {k_table} au {k_field} cu zecimale sau în afara limitelor. {k_first.Justification}",
                        "Corectați valoarea în Access.")
            Case "X03"
                Return ($"{k_table}: număr de document cu prea multe zecimale",
                        $"{k_rows} din {k_table} au {k_field} cu mai mult de 2 zecimale; fracția nu încape în NumarSub.",
                        "Corectați numărul în Access (cel mult 2 zecimale, de exemplu 4.1).")
            Case "X13"
                Return ($"{k_table}: Tip nu este «Felul 1», «Felul 2», …",
                        $"{k_rows} din {k_table} au în Tip un text care nu poate deveni fel de mâncare. {k_first.Justification}",
                        "Corectați Tip în Access: doar «Felul 1», «Felul 2» etc.")
            Case "X06"
                Return ($"{k_table}: rânduri fără cheie primară",
                        $"{k_rows} din {k_table} nu au cheie primară și nu pot fi legate de nimic.",
                        "Verificați tabelul în Access.")
            Case Else
                Return ($"{k_table}{If(k_field.Length > 0, "." & k_field, String.Empty)}: {k_first.Justification}",
                        $"{k_rows}. {k_first.Justification}",
                        "Corectați datele în Access.")
        End Select
    End Function

    ''' <summary>
    ''' Write-offs saved with a positive quantity are corrected by the transfer (x -1). This section says which, and whether the correction
    ''' leaves any batch below zero (checked day by day). The heading decides the colour of the HTML report: success or warning.
    ''' </summary>
    Private Sub AppendSignCheck(k_sb As StringBuilder)
        Dim k_flips = Issues.Where(Function(k_i) k_i.RuleCode = "N18").ToList()
        If k_flips.Count = 0 Then Return
        Dim k_negative = Issues.Where(Function(k_i) k_i.RuleCode = "X18").ToList()
        k_sb.AppendLine(If(k_negative.Count = 0, "## Casări cu semn corectat — stocul rămâne în ordine",
                           "## AVERTIZĂRI — stoc negativ după corectarea semnului la casări"))
        k_sb.AppendLine()
        k_sb.AppendLine($"{Cases(k_flips.Count)} din OI_Produse ({k_flips.Select(Function(k_i) k_i.Justification.Split(","c)(0)).Distinct().Count()} bonuri de casare) aveau cantitatea scrisă cu plus, deși o casare scade stocul. " &
                        "Migratorul înmulțește cantitatea (și valoarea) cu -1, numai în transfer; fișierele Access rămân neatinse.")
        k_sb.AppendLine()
        If k_negative.Count = 0 Then
            k_sb.AppendLine("**Verificare stoc:** după schimbare, niciun lot de obiecte nu ajunge pe minus (fiecare lot afectat a fost urmărit zi cu zi, în ordinea datelor bonurilor).")
        Else
            k_sb.AppendLine($"**ATENȚIE — {k_negative.Count:N0} {If(k_negative.Count = 1, "lot ajunge", "loturi ajung")} pe minus după schimbarea semnului.** Transferul nu este blocat, dar casările de mai jos scot mai mult decât s-a primit:")
            k_sb.AppendLine()
            For Each k_issue In k_negative.Take(25)
                k_sb.AppendLine("- " & k_issue.Justification)
            Next
            If k_negative.Count > 25 Then k_sb.AppendLine($"- … și încă {k_negative.Count - 25:N0}")
            k_sb.AppendLine()
            k_sb.AppendLine("**Ce faceți:** verificați casările acestor loturi în Access. Fie cantitatea pozitivă era o intrare reală (atunci semnul nu trebuia schimbat), fie lipsește o primire sau este scrisă prea mică.")
        End If
        k_sb.AppendLine()
        k_sb.AppendLine("Pozițiile schimbate:")
        k_sb.AppendLine()
        For Each k_file In k_flips.GroupBy(Function(k_i) PathOf(k_i), StringComparer.OrdinalIgnoreCase)
            k_sb.AppendLine($"- `{k_file.Key}` ({Cases(k_file.Count())}){FolderLink(k_file.Key)}")
            For Each k_i In k_file.Take(8)
                k_sb.AppendLine("    - " & k_i.Justification)
            Next
            If k_file.Count() > 8 Then k_sb.AppendLine($"    - … și încă {k_file.Count() - 8:N0}")
        Next
        k_sb.AppendLine()
    End Sub

    ''' <summary>What the transfer does by itself or notes without blocking: short lines, one per kind. Nothing here asks the operator to act.</summary>
    Private Sub AppendInformative(k_sb As StringBuilder)
        Dim k_info = Issues.Where(Function(k_i) k_i.Severity <> "BLOCAJ" AndAlso k_i.RuleCode <> "N18" AndAlso k_i.RuleCode <> "X18").ToList()
        If k_info.Count = 0 Then Return
        k_sb.AppendLine("## Doar de informare (nu blochează și nu cer nimic)")
        k_sb.AppendLine()
        Dim k_dishes = k_info.Where(Function(k_i) k_i.RuleCode = "N11").ToList()
        If k_dishes.Count > 0 Then
            Dim k_labels = k_dishes.Select(Function(k_i) k_i.OriginalValue).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(Function(k_s) k_s, StringComparer.OrdinalIgnoreCase)
            Dim k_tables = k_dishes.Select(Function(k_i) k_i.TableName).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(Function(k_s) k_s, StringComparer.OrdinalIgnoreCase)
            k_sb.AppendLine($"- Feluri de mâncare create automat din coloana Tip: {String.Join(", ", k_labels)} (din {String.Join(", ", k_tables)}).")
        End If
        For Each k_group In k_info.Where(Function(k_i) k_i.RuleCode <> "N11").
                                   GroupBy(Function(k_i) k_i.RuleCode & "|" & k_i.TableName & "|" & k_i.FieldName, StringComparer.OrdinalIgnoreCase).
                                   OrderBy(Function(k_g) k_g.Key, StringComparer.OrdinalIgnoreCase)
            Dim k_first = k_group.First()
            If k_first.RuleCode = "N15" Then
                k_sb.AppendLine($"- IntrariStorno: {Cases(k_group.Count())} ignorate, fără NIR părinte bifat Storno. Nu blochează importul și nu sunt scrise în țintă.")
                For Each k_file In k_group.GroupBy(Function(k_i) PathOf(k_i), StringComparer.OrdinalIgnoreCase)
                    k_sb.AppendLine($"    - `{k_file.Key}`: " & String.Join(", ", k_file.Select(Function(k_i) $"ID {k_i.SourceKey} ({k_i.OriginalValue})")))
                Next
                Continue For
            End If
            If k_first.RuleCode = "N12" Then
                ' Orphan links kept as NULL: every row is listed, because the operator may want to fix them later.
                k_sb.AppendLine($"- {k_first.TableName}.{k_first.FieldName}: {Cases(k_group.Count())} cu legătura goală. {k_first.Justification}")
                For Each k_file In k_group.GroupBy(Function(k_i) PathOf(k_i), StringComparer.OrdinalIgnoreCase)
                    k_sb.AppendLine($"    - `{k_file.Key}`: " & String.Join(", ", k_file.Take(20).Select(Function(k_i) $"{KeyNameOf(k_i.TableName)} {k_i.SourceKey} (arăta spre {k_i.OriginalValue})")) & If(k_file.Count() > 20, $", … și încă {k_file.Count() - 20}", String.Empty))
                Next
                Continue For
            End If
            If k_first.RuleCode = "N14" Then
                k_sb.AppendLine($"- {Cases(k_group.Count())} în care bonul de origine nu mai exista în Access: s-a găsit o singură primire potrivită și poziția a fost legată de ea. Se schimbă doar în transfer; fișierele Access rămân neatinse.")
                For Each k_file In k_group.GroupBy(Function(k_i) PathOf(k_i), StringComparer.OrdinalIgnoreCase)
                    k_sb.AppendLine($"    - `{k_file.Key}`: " & String.Join(", ", k_file.Select(Function(k_i) $"poziția {k_i.SourceKey} (bonul {k_i.OriginalValue} lipsea; legată de {k_i.TargetValue})")))
                Next
                Continue For
            End If
            If k_first.RuleCode = "N16" Then
                k_sb.AppendLine($"- {k_first.TableName}: {Cases(k_group.Count())} de poziții de primire din registru nu aveau produs; produsul a fost luat din linia bonului de predare cu aceeași cantitate și același preț, acolo unde exista o singură variantă. Fișierele Access rămân neatinse.")
                Continue For
            End If
            If k_first.RuleCode = "N17" Then
                k_sb.AppendLine($"- {k_first.TableName}: {Cases(k_group.Count())} de poziții de primire rămân fără produs, pentru că bonul nu arată o singură variantă sigură. Le reglați din VERCON, cu butonul «Reglare registru OI» din paginile bonurilor.")
                Continue For
            End If
            If k_first.RuleCode = "X08" Then
                Dim k_what As String = If(String.Equals(k_first.FieldName, "Cant", StringComparison.OrdinalIgnoreCase), "cantitate negativă", "preț negativ")
                k_sb.AppendLine($"- {k_first.TableName}: {Cases(k_group.Count())} cu {k_what}. Se transferă exact cum sunt, cu minus; nu se corectează nimic.")
                Continue For
            End If
            If k_first.RuleCode = "N10" Then
                Dim k_parts = k_first.TargetValue.Split({" + NumarSub "}, StringSplitOptions.None)
                Dim k_sample As String = If(k_parts.Length = 2, $" De exemplu {k_first.OriginalValue} devine numărul {k_parts(0)}, cu subnumărul {k_parts(1)}.", String.Empty)
                k_sb.AppendLine($"- {k_first.TableName}: {Cases(k_group.Count())} cu număr de document cu zecimale (document introdus între două numere). Partea întreagă rămâne numărul documentului, zecimalele devin subnumărul; ordinea documentelor nu se schimbă.{k_sample}")
                Continue For
            End If
            If k_first.RuleCode = "X04" Then
                Dim k_codes = k_group.Select(Function(k_i) If(String.Equals(k_i.OriginalValue, "NULL", StringComparison.OrdinalIgnoreCase), "(fără cod fiscal)", k_i.OriginalValue)).
                                      Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                k_sb.AppendLine($"- {k_first.TableName}: {k_group.Count():N0} referințe de furnizor fără cod fiscal utilizabil ({String.Join(", ", k_codes.Take(10))}{If(k_codes.Count > 10, ", …", String.Empty)}). Documentele se transferă cu CodPartener NULL; asocierea la eFactura cere completarea explicită a codului fiscal.")
                Continue For
            End If
            If k_first.RuleCode = "N13" Then
                k_sb.AppendLine($"- {k_first.TableName}: {Cases(k_group.Count())} în care contul scris pe linie diferă de contul bonului de predare inițial. Se folosește contul de la predare.")
                Continue For
            End If
            Dim k_example As String = If(k_first.OriginalValue.Length > 0 AndAlso k_first.TargetValue.Length > 0, $" Exemplu: {k_first.OriginalValue} → {k_first.TargetValue}.", String.Empty)
            k_sb.AppendLine($"- {k_first.TableName}{If(k_first.FieldName.Length > 0, "." & k_first.FieldName, String.Empty)}: {Cases(k_group.Count())}. {k_first.Justification}{k_example}")
        Next
        k_sb.AppendLine()
    End Sub

    Private Shared Function Cases(k_count As Integer) As String
        Return If(k_count = 1, "1 caz", $"{k_count:N0} cazuri")
    End Function

    ''' <summary>The source key column of a table (from the import map); empty when unknown.</summary>
    Private Shared Function KeyNameOf(k_tableName As String) As String
        Dim k_schema = VrcImportMap.Find(k_tableName)
        Return If(k_schema Is Nothing, String.Empty, k_schema.SourceKey)
    End Function

    Private Shared Function ColumnOf(k_tableName As String, k_fieldName As String) As VrcImportColumn
        If k_fieldName.Length = 0 Then Return Nothing
        Dim k_schema = VrcImportMap.Find(k_tableName)
        If k_schema Is Nothing Then Return Nothing
        Return k_schema.Columns.FirstOrDefault(Function(k_c) String.Equals(k_c.Name, k_fieldName, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Shared Function PathOf(k_issue As VrcIssue) As String
        Return If(k_issue.SourceFullPath.Length > 0, k_issue.SourceFullPath, k_issue.SourceFile)
    End Function

    ''' <summary>« — [deschide folderul](file:///…)» for a real path; nothing for a bare name. The report view and the text box open it in Explorer.</summary>
    Private Shared Function FolderLink(k_path As String) As String
        If Not IO.Path.IsPathRooted(k_path) Then Return String.Empty
        Return $" — [deschide folderul]({New Uri(k_path).AbsoluteUri})"
    End Function

End Class
