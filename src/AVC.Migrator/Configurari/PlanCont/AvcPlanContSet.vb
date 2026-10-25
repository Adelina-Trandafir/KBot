Imports System.Globalization
Imports System.Text.RegularExpressions
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

' Slice AV01-05: the Access chart of accounts (baza<year>.accdb, table PlanCont) becomes TZ_PlanCont (+ the opening
' balances in TZ_SoldInitial; the turnovers RPD / RPC are NOT migrated). One Access row is one full account; the target splits it the way the
' window does: base account + sector/source + [ClsfF] + [ClsfE] + [.analytic]. Read once; the verification (dry run) and
' the writer use the same set, so what was reported is what is written.

''' <summary>One row of the Access <c>PlanCont</c>, shaped for the target.</summary>
Friend NotInheritable Class AvcPlanContRow

    Public Property IdUnitate As Integer
    ''' <summary>The Access primary key, e.g. 117000001A.</summary>
    Public Property SimbolAccess As String = String.Empty
    ''' <summary>The base account (7 digits); Nothing when the Access cell holds nothing usable.</summary>
    Public Property Cont As Integer?
    Public Property ContText As String = String.Empty
    ''' <summary>Sector + source as 01A.</summary>
    Public Property SursaSector As String = String.Empty
    Public Property ClsfF As String = String.Empty
    Public Property ClsfE As String = String.Empty
    Public Property Analitic As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Fel As String = String.Empty
    Public Property Extra As Boolean
    Public Property SoldDebitor As Decimal
    Public Property SoldCreditor As Decimal

    ''' <summary>
    ''' True when the Access key of the account appears in <c>Debit</c>, <c>Credit</c> or <c>ParteneriSI</c> of the unit file. Also True
    ''' when that could not be read (a table without <c>SimbolCont</c>): an account is only called unused when that is proven.
    ''' </summary>
    Public Property UsedInAccess As Boolean = True

    ''' <summary>
    ''' Set by <see cref="AvcPlanContSet.Validate"/>: the account is unused, has no opening balance and holds a value the common nomenclators
    ''' refuse, so it is NOT written (the foreign key would refuse it) and only reported as a warning.
    ''' </summary>
    Public Property Skipped As Boolean

    ''' <summary>The symbol the window/server would compose for this account.</summary>
    Public Function ComposedSymbol() As String
        Dim k_text As String = $"{ContText}{SursaSector}{ClsfF}{ClsfE}"
        If Analitic.Length > 0 Then k_text &= "." & Analitic
        Return k_text
    End Function

    Public ReadOnly Property HasAmounts As Boolean
        Get
            Return SoldDebitor <> 0D OrElse SoldCreditor <> 0D
        End Get
    End Property

    Public ReadOnly Property HasNegativeAmount As Boolean
        Get
            Return SoldDebitor < 0D OrElse SoldCreditor < 0D
        End Get
    End Property

    ''' <summary>A, B or P: the Access one when valid, else the type of the base account (Access leaves it blank on some rows).</summary>
    Public Function EffectiveFel(k_lookups As AvcPlanContLookups) As String
        Dim k_fel As String = Fel.Trim().ToUpperInvariant()
        If k_fel = "A" OrElse k_fel = "B" OrElse k_fel = "P" Then Return k_fel
        Dim k_base As AvcPlanContBase = Nothing
        If Cont.HasValue AndAlso k_lookups.Conturi.TryGetValue(Cont.Value, k_base) Then Return k_base.Tip
        Return String.Empty
    End Function

    ''' <summary>The Access name, or the name of the base account when the Access cell is empty.</summary>
    Public Function EffectiveDenumire(k_lookups As AvcPlanContLookups) As String
        If Denumire.Length > 0 Then Return Denumire
        Dim k_base As AvcPlanContBase = Nothing
        If Cont.HasValue AndAlso k_lookups.Conturi.TryGetValue(Cont.Value, k_base) Then Return k_base.Denumire
        Return String.Empty
    End Function

End Class

''' <summary>What the common database says about a base account.</summary>
Friend NotInheritable Class AvcPlanContBase
    Public Property Denumire As String = String.Empty
    Public Property Tip As String = String.Empty
    Public Property CereClsfF As Boolean
    Public Property CereClsfE As Boolean
End Class

''' <summary>The nomenclators of AVACONT_COMUN the chart is checked (and completed) against.</summary>
Friend NotInheritable Class AvcPlanContLookups

    Public ReadOnly Property Conturi As New Dictionary(Of Integer, AvcPlanContBase)()
    Public ReadOnly Property ClsfF As New HashSet(Of String)(StringComparer.Ordinal)
    Public ReadOnly Property ClsfE As New HashSet(Of String)(StringComparer.Ordinal)
    Public ReadOnly Property SursaSector As New HashSet(Of String)(StringComparer.Ordinal)

    ''' <summary>Reads the four nomenclators of the common database. Server I/O: logs and rethrows.</summary>
    Public Shared Function Load(k_cn As MySqlConnection, k_transaction As MySqlTransaction, k_commonDatabase As String) As AvcPlanContLookups
        Try
            Dim k_result As New AvcPlanContLookups()
            Dim k_db As String = AvcSql.Quote(k_commonDatabase)

            Using k_cmd As New MySqlCommand(
                $"SELECT `Simbol`, `DENUMIRE`, `TIP`, COALESCE(`CLSFF`, 0), COALESCE(`CLSFE`, 0) FROM {k_db}.`DefaCont`", k_cn, k_transaction)
                Using k_reader = k_cmd.ExecuteReader()
                    While k_reader.Read()
                        k_result.Conturi(k_reader.GetInt32(0)) = New AvcPlanContBase() With {
                            .Denumire = If(k_reader.IsDBNull(1), String.Empty, k_reader.GetString(1)),
                            .Tip = If(k_reader.IsDBNull(2), String.Empty, k_reader.GetString(2).Trim().ToUpperInvariant()),
                            .CereClsfF = k_reader.GetInt32(3) <> 0,
                            .CereClsfE = k_reader.GetInt32(4) <> 0}
                    End While
                End Using
            End Using

            ReadSet(k_cn, k_transaction, $"SELECT `ClsfF` FROM {k_db}.`DefaClsfF`", k_result.ClsfF)
            ReadSet(k_cn, k_transaction, $"SELECT `ClsfE` FROM {k_db}.`DefaClsfE`", k_result.ClsfE)
            ReadSet(k_cn, k_transaction, $"SELECT `SursaSector` FROM {k_db}.`DefaSursaSector`", k_result.SursaSector)
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcPlanContLookups.Load", ex)
            Throw
        End Try
    End Function

    Private Shared Sub ReadSet(k_cn As MySqlConnection, k_transaction As MySqlTransaction, k_sql As String, k_into As HashSet(Of String))
        Using k_cmd As New MySqlCommand(k_sql, k_cn, k_transaction)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    If Not k_reader.IsDBNull(0) Then k_into.Add(k_reader.GetString(0))
                End While
            End Using
        End Using
    End Sub

End Class

''' <summary>One kind of problem found in the chart, with the rows it concerns.</summary>
Friend NotInheritable Class AvcPlanContIssue
    Public Sub New(k_severity As FindingClass, k_column As String, k_message As String, k_count As Integer, k_detail As String)
        Severity = k_severity
        Column = k_column
        Message = k_message
        Count = k_count
        Detail = k_detail
    End Sub

    Public ReadOnly Property Severity As FindingClass
    Public ReadOnly Property Column As String
    Public ReadOnly Property Message As String
    Public ReadOnly Property Count As Integer
    Public ReadOnly Property Detail As String
End Class

''' <summary>Everything read from the Access <c>PlanCont</c> of the ticked units.</summary>
Friend NotInheritable Class AvcPlanContSet

    Private Shared ReadOnly AnaliticRegex As New Regex("^[0-9A-Z_]{1,10}$", RegexOptions.CultureInvariant)

    Public ReadOnly Property Rows As New List(Of AvcPlanContRow)()
    ''' <summary>Units whose file has no <c>PlanCont</c> table.</summary>
    Public ReadOnly Property UnitsWithoutTable As New List(Of Integer)()
    ''' <summary>Units whose <c>PlanCont</c> holds at least one row.</summary>
    Public ReadOnly Property UnitsWithRows As New HashSet(Of Integer)()
    ''' <summary>Access columns the reader needs and the table does not have.</summary>
    Public ReadOnly Property MissingColumns As New List(Of String)()
    ''' <summary>Units where the use of the accounts in <c>Debit</c> / <c>Credit</c> / <c>ParteneriSI</c> could not be read: none of their accounts is called unused.</summary>
    Public ReadOnly Property UsageUnknownUnits As New List(Of Integer)()

    ''' <summary>The rows that are written: every row except the ones <see cref="Validate"/> set aside (unused, no balance, refused by a nomenclator).</summary>
    Public Function ActiveRows() As List(Of AvcPlanContRow)
        Return Rows.Where(Function(r) Not r.Skipped).ToList()
    End Function

    ''' <summary>Accounts that carry an opening balance (the rows of <c>TZ_SoldInitial</c>). The turnovers are not migrated.</summary>
    Public Function WithAmountsCount() As Integer
        Return Rows.Where(Function(r) Not r.Skipped AndAlso r.HasAmounts).Count()
    End Function

    ''' <summary>
    ''' Checks the rows against the common nomenclators. BLOCANT = the insert would be refused (foreign key, check, width) or
    ''' two Access rows would collapse into one; ATENTIE = it is written, but the operator should look; INFORMATIV = a gap filled.
    ''' </summary>
    Public Function Validate(k_lookups As AvcPlanContLookups) As List(Of AvcPlanContIssue)
        Dim k_issues As New List(Of AvcPlanContIssue)()

        Dim k_skipped As New List(Of String)()
        Dim k_noBase As New List(Of String)()
        Dim k_noSs As New List(Of String)()
        Dim k_noClsfF As New List(Of String)()
        Dim k_noClsfE As New List(Of String)()
        Dim k_eWithoutF As New List(Of String)()
        Dim k_notAllowedF As New List(Of String)()
        Dim k_notAllowedE As New List(Of String)()
        Dim k_badFel As New List(Of String)()
        Dim k_blankFel As New List(Of String)()
        Dim k_blankName As New List(Of String)()
        Dim k_badAnalitic As New List(Of String)()
        Dim k_tooLong As New List(Of String)()
        Dim k_negative As New List(Of String)()
        Dim k_different As New List(Of String)()
        Dim k_seen As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Dim k_duplicates As New List(Of String)()

        For Each k_row In Rows
            Dim k_id As String = If(k_row.SimbolAccess.Length > 0, k_row.SimbolAccess, k_row.ComposedSymbol())

            ' What the common nomenclators refuse (the foreign key would refuse the row). An account that is ALSO unused in Access
            ' (not in Debit, Credit or ParteneriSI) and has no opening balance is set aside with a warning instead of blocking.
            Dim k_base As AvcPlanContBase = Nothing
            Dim k_noBaseText As String = Nothing
            Dim k_noSsText As String = Nothing
            Dim k_noClsfFText As String = Nothing
            Dim k_noClsfEText As String = Nothing
            Dim k_eWithoutFRow As Boolean = False
            If Not k_row.Cont.HasValue OrElse Not k_lookups.Conturi.TryGetValue(k_row.Cont.Value, k_base) Then
                k_noBaseText = $"Cont «{k_row.ContText}»"
            End If
            If k_row.SursaSector.Length <> 3 OrElse Not k_lookups.SursaSector.Contains(k_row.SursaSector) Then
                k_noSsText = $"sector+sursă «{k_row.SursaSector}»"
            End If
            If k_row.ClsfF.Length > 0 AndAlso Not k_lookups.ClsfF.Contains(k_row.ClsfF) Then k_noClsfFText = $"ClsfF «{k_row.ClsfF}»"
            If k_row.ClsfE.Length > 0 AndAlso Not k_lookups.ClsfE.Contains(k_row.ClsfE) Then k_noClsfEText = $"ClsfE «{k_row.ClsfE}»"
            If k_row.ClsfE.Length > 0 AndAlso k_row.ClsfF.Length = 0 Then k_eWithoutFRow = True

            k_row.Skipped = False
            Dim k_refused As Boolean = k_noBaseText IsNot Nothing OrElse k_noSsText IsNot Nothing OrElse k_noClsfFText IsNot Nothing OrElse
                                       k_noClsfEText IsNot Nothing OrElse k_eWithoutFRow
            If k_refused AndAlso Not k_row.UsedInAccess AndAlso Not k_row.HasAmounts Then
                k_row.Skipped = True
                Dim k_why As New List(Of String)()
                For Each k_text In {k_noBaseText, k_noSsText, k_noClsfFText, k_noClsfEText}
                    If k_text IsNot Nothing Then k_why.Add(k_text)
                Next
                If k_eWithoutFRow Then k_why.Add("ClsfE fără ClsfF")
                k_skipped.Add($"{k_id}  ({String.Join("; ", k_why)})")
                Continue For
            End If
            If k_noBaseText IsNot Nothing Then k_noBase.Add($"{k_id}  ({k_noBaseText})")
            If k_noSsText IsNot Nothing Then k_noSs.Add($"{k_id}  ({k_noSsText})")
            If k_noClsfFText IsNot Nothing Then k_noClsfF.Add($"{k_id}  ({k_noClsfFText})")
            If k_noClsfEText IsNot Nothing Then k_noClsfE.Add($"{k_id}  ({k_noClsfEText})")
            If k_eWithoutFRow Then k_eWithoutF.Add(k_id)
            If k_base IsNot Nothing Then
                If k_row.ClsfF.Length > 0 AndAlso Not k_base.CereClsfF Then k_notAllowedF.Add(k_id)
                If k_row.ClsfE.Length > 0 AndAlso Not k_base.CereClsfE Then k_notAllowedE.Add(k_id)
            End If

            Dim k_fel As String = k_row.Fel.Trim().ToUpperInvariant()
            If k_fel.Length = 0 Then
                k_blankFel.Add(k_id)
            ElseIf k_fel <> "A" AndAlso k_fel <> "B" AndAlso k_fel <> "P" Then
                k_badFel.Add($"{k_id}  («{k_row.Fel}»)")
            End If
            If k_row.Denumire.Length = 0 Then k_blankName.Add(k_id)
            If k_row.Analitic.Length > 0 AndAlso Not AnaliticRegex.IsMatch(k_row.Analitic) Then k_badAnalitic.Add($"{k_id}  (analitic «{k_row.Analitic}»)")

            Dim k_composed As String = k_row.ComposedSymbol()
            If k_composed.Length > 50 OrElse k_row.Denumire.Length > 255 OrElse k_row.Analitic.Length > 10 Then k_tooLong.Add(k_id)
            If k_row.HasNegativeAmount Then k_negative.Add(k_id)

            If k_row.SimbolAccess.Length > 0 AndAlso Not String.Equals(k_row.SimbolAccess, k_composed, StringComparison.OrdinalIgnoreCase) Then
                k_different.Add($"{k_row.SimbolAccess}  ▸  {k_composed}")
            End If

            Dim k_first As String = Nothing
            If k_seen.TryGetValue(k_composed, k_first) Then
                k_duplicates.Add($"{k_composed}  ({k_first} și {k_id})")
            Else
                k_seen(k_composed) = k_id
            End If
        Next

        AddIssue(k_issues, FindingClass.Blocant, "Cont", k_noBase,
                 "conturi a căror «Cont» nu există în nomenclatorul comun (DefaCont); cheia străină ar refuza rândul")
        AddIssue(k_issues, FindingClass.Blocant, "SursaSector", k_noSs,
                 "conturi al căror sector+sursă nu există în nomenclatorul comun (DefaSursaSector)")
        AddIssue(k_issues, FindingClass.Blocant, "ClsfF", k_noClsfF, "conturi cu o clasificație funcțională absentă din DefaClsfF")
        AddIssue(k_issues, FindingClass.Blocant, "ClsfE", k_noClsfE, "conturi cu o clasificație economică absentă din DefaClsfE")
        AddIssue(k_issues, FindingClass.Blocant, "ClsfE", k_eWithoutF, "conturi cu clasificație economică, dar fără cea funcțională")
        AddIssue(k_issues, FindingClass.Blocant, "SimbolCont", k_tooLong, "conturi cu un simbol, o denumire sau un analitic mai lung decât permite coloana")
        AddIssue(k_issues, FindingClass.Blocant, "SoldDebitor", k_negative, "conturi cu un sold negativ; coloanele acceptă doar zero sau mai mult")
        AddIssue(k_issues, FindingClass.Blocant, "SimbolCont", k_duplicates, "simboluri compuse identice: două rânduri Access ar deveni un singur cont")
        AddIssue(k_issues, FindingClass.Atentie, "Cont", k_skipped,
                 "conturi NEFOLOSITE (nici în Debit, Credit sau ParteneriSI) și fără sold inițial, cu valori care nu există în nomenclatoarele comune; NU se migrează")
        AddIssue(k_issues, FindingClass.Atentie, "ClsfF", k_notAllowedF, "conturi cu ClsfF, deși contul de bază nu se desparte pe clasificație funcțională (se scriu așa)")
        AddIssue(k_issues, FindingClass.Atentie, "ClsfE", k_notAllowedE, "conturi cu ClsfE, deși contul de bază nu se desparte pe clasificație economică (se scriu așa)")
        AddIssue(k_issues, FindingClass.Atentie, "FelCont", k_badFel, "conturi cu un fel diferit de A, B sau P; se ia felul contului de bază")
        AddIssue(k_issues, FindingClass.Atentie, "Analitic", k_badAnalitic, "conturi cu analitic în afara regulii (cifre, litere mari, «_»)")
        AddIssue(k_issues, FindingClass.Atentie, "SimbolCont", k_different,
                 "conturi al căror simbol compus (cont + sector + sursă + clasificații + analitic) diferă de cheia din Access; se scrie cel compus")
        AddIssue(k_issues, FindingClass.Informativ, "FelCont", k_blankFel, "conturi fără fel în Access; se ia felul contului de bază")
        AddIssue(k_issues, FindingClass.Informativ, "Denumire", k_blankName, "conturi fără denumire în Access; se ia denumirea contului de bază")
        Return k_issues
    End Function

    Private Shared Sub AddIssue(k_into As List(Of AvcPlanContIssue), k_severity As FindingClass, k_column As String,
                                k_rows As List(Of String), k_what As String)
        If k_rows.Count = 0 Then Return
        Dim k_shown As IEnumerable(Of String) = k_rows.Take(400)
        Dim k_more As String = If(k_rows.Count > 400, Environment.NewLine & $"… și încă {k_rows.Count - 400}", String.Empty)
        k_into.Add(New AvcPlanContIssue(k_severity, k_column, $"{k_rows.Count} {k_what}.", k_rows.Count,
                                        String.Join(Environment.NewLine, k_shown) & k_more))
    End Sub

End Class

''' <summary>Reads the <c>PlanCont</c> of every ticked unit's file; each row keeps the unit it came from.</summary>
Friend NotInheritable Class AvcPlanContReader

    Private Sub New()
    End Sub

    Public Const AccessTable As String = "PlanCont"

    ''' <summary>Access I/O: logs and rethrows.</summary>
    Public Shared Function Read(k_context As AvcContext) As AvcPlanContSet
        Try
            Dim k_result As New AvcPlanContSet()
            For Each k_unit In k_context.Units
                If Not k_unit.HasUnitFile Then Continue For
                ReadUnit(k_context, k_unit, k_result)
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcPlanContReader.Read", ex)
            Throw
        End Try
    End Function

    Private Shared Sub ReadUnit(k_context As AvcContext, k_unit As CaiUnit, k_result As AvcPlanContSet)
        Using k_cn = AccessProvider.Open(k_unit.UnitFilePath, k_context.AccessPassword)
            Dim k_table = AccessSchema.ResolveTableName(k_cn, AccessTable)
            If k_table Is Nothing Then
                k_result.UnitsWithoutTable.Add(k_unit.IdUnitate)
                Return
            End If

            Dim k_firstRow As Integer = k_result.Rows.Count
            Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
                For Each k_column In {"SimbolCont", "Cont", "DenumireCont", "CodSector", "Sursa"}
                    If Not k_reader.HasColumn(k_column) AndAlso Not k_result.MissingColumns.Contains(k_column) Then
                        k_result.MissingColumns.Add(k_column)
                    End If
                Next

                While k_reader.Read()
                    Dim k_contText As String = DigitsOf(Text(k_reader, "Cont"))
                    If k_contText.Length > 7 Then k_contText = k_contText.Substring(0, 7)
                    Dim k_contValue As Integer
                    k_result.Rows.Add(New AvcPlanContRow() With {
                        .IdUnitate = k_unit.IdUnitate,
                        .SimbolAccess = Text(k_reader, "SimbolCont"),
                        .ContText = k_contText,
                        .Cont = If(k_contText.Length = 7 AndAlso Integer.TryParse(k_contText, NumberStyles.None, CultureInfo.InvariantCulture, k_contValue),
                                   CType(k_contValue, Integer?), Nothing),
                        .SursaSector = SectorOf(Text(k_reader, "CodSector")) & Text(k_reader, "Sursa").ToUpperInvariant(),
                        .ClsfF = Text(k_reader, "ClsfF"),
                        .ClsfE = Text(k_reader, "ClsfE"),
                        .Analitic = Text(k_reader, "Analitic"),
                        .Denumire = Text(k_reader, "DenumireCont"),
                        .Fel = Text(k_reader, "FelCont"),
                        .Extra = IsYes(k_reader.ValueOrMissing("Extra")),
                        .SoldDebitor = Amount(k_reader.ValueOrMissing("SID")),
                        .SoldCreditor = Amount(k_reader.ValueOrMissing("SIC"))})
                    k_result.UnitsWithRows.Add(k_unit.IdUnitate)
                End While
            End Using
            MarkUsage(k_cn, k_unit, k_result, k_firstRow)
        End Using
    End Sub

    ''' <summary>The Access tables whose <c>SimbolCont</c> says an account is in use (note lines and opening balances per partner).</summary>
    Private Shared ReadOnly UsageTables As String() = {"Debit", "Credit", "ParteneriSI"}

    ''' <summary>
    ''' Sets <see cref="AvcPlanContRow.UsedInAccess"/> on the rows this unit just added: an account is unused only when its key is in none of
    ''' <see cref="UsageTables"/>. A table the file does not have uses nothing; a table that cannot be read (no <c>SimbolCont</c>) leaves the
    ''' whole unit "used", so nothing of it is set aside on a guess.
    ''' </summary>
    Private Shared Sub MarkUsage(k_cn As System.Data.OleDb.OleDbConnection, k_unit As CaiUnit, k_result As AvcPlanContSet, k_firstRow As Integer)
        Dim k_used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim k_known As Boolean = True
        For Each k_name In UsageTables
            Dim k_table = AccessSchema.ResolveTableName(k_cn, k_name)
            If k_table Is Nothing Then Continue For
            Try
                Using k_reader = AccessSchema.OpenKeyReader(k_cn, k_table, {"SimbolCont"})
                    While k_reader.Read()
                        Dim k_symbol = AvcSql.AsText(k_reader.ValueOrMissing("SimbolCont"))
                        If k_symbol.Length > 0 Then k_used.Add(k_symbol)
                    End While
                End Using
            Catch ex As Exception
                GlobalErrorLog.Write("AvcPlanContReader.MarkUsage." & k_name, ex)
                k_known = False
            End Try
        Next
        If Not k_known Then
            k_result.UsageUnknownUnits.Add(k_unit.IdUnitate)
            Return
        End If
        For k_index = k_firstRow To k_result.Rows.Count - 1
            Dim k_row = k_result.Rows(k_index)
            k_row.UsedInAccess = k_row.SimbolAccess.Length = 0 OrElse k_used.Contains(k_row.SimbolAccess)
        Next
    End Sub

    Private Shared Function Text(k_reader As AccessTableReader, k_column As String) As String
        Return AvcSql.AsText(k_reader.ValueOrMissing(k_column))
    End Function

    Private Shared Function DigitsOf(k_text As String) As String
        Return New String(k_text.Where(Function(k_c) Char.IsDigit(k_c)).ToArray())
    End Function

    ' Access keeps the sector as «01», but a numeric cell would come out as «1».
    Private Shared Function SectorOf(k_text As String) As String
        Return If(k_text.Length = 1 AndAlso Char.IsDigit(k_text(0)), "0" & k_text, k_text)
    End Function

    ' Access Yes/No is a Boolean (-1 / 0); older copies keep «Da» / «Nu» as text.
    Private Shared Function IsYes(k_value As Object) As Boolean
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return False
        If TypeOf k_value Is Boolean Then Return CBool(k_value)
        Dim k_text As String = Convert.ToString(k_value, CultureInfo.InvariantCulture).Trim()
        Return String.Equals(k_text, "Da", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(k_text, "True", StringComparison.OrdinalIgnoreCase) OrElse k_text = "1" OrElse k_text = "-1"
    End Function

    ' Currency / Double to two decimals; empty = 0. An unreadable cell is 0 too, but a negative one stays negative so the
    ' verification can name the row.
    Private Shared Function Amount(k_value As Object) As Decimal
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return 0D
        Try
            Return Math.Round(Convert.ToDecimal(k_value, CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero)
        Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is InvalidCastException OrElse TypeOf ex Is OverflowException
            GlobalErrorLog.Write("AvcPlanContReader.Amount", ex)
            Return 0D
        End Try
    End Function

End Class
