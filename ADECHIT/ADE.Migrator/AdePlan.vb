Imports System.Data
Imports System.Globalization
Imports KBot.Common

''' <summary>One thing the check found. A blocking finding stops the migration.</summary>
Public NotInheritable Class AdeFinding

    Public Sub New(k_blocking As Boolean, k_text As String)
        Blocking = k_blocking
        Text = k_text
    End Sub

    Public ReadOnly Property Blocking As Boolean
    Public ReadOnly Property Text As String

End Class

''' <summary>The rows that will be written to one AD_ table.</summary>
Public NotInheritable Class AdePlanTable

    Public Sub New(k_table As AdeTable, k_sourceRows As Integer)
        Table = k_table
        SourceRows = k_sourceRows
    End Sub

    Public ReadOnly Property Table As AdeTable
    ''' <summary>Rows in Access; -1 for a table the migration builds itself.</summary>
    Public ReadOnly Property SourceRows As Integer
    Public ReadOnly Property Rows As New List(Of Dictionary(Of String, Object))()

End Class

''' <summary>
''' Everything the migration will do, worked out BEFORE anything is written: the converted rows of every table, the
''' rows it builds itself (educators in time, the history of each child), what it changed on the way, and what blocks it.
''' </summary>
Public NotInheritable Class AdePlan

    ''' <summary>Access tables that are deliberately not read.</summary>
    Public Const NotMigrated As String = "Delegati, Prezenta_sub, MutaCopil, Trimis, Facturi, Ver_DB, NUMERE, OPuri, COMP, Mail, BonuriF"

    Private Const SystemUser As String = "migrare"
    Private Const DepartedWord As String = "plecat"

    Public ReadOnly Property Tables As New List(Of AdePlanTable)()
    Public ReadOnly Property Findings As New List(Of AdeFinding)()
    ''' <summary>The conversions applied, one line each, for the operator to read.</summary>
    Public ReadOnly Property Notes As New List(Of String)()
    Public ReadOnly Property ReceiptConfig As AdeReceiptConfig

    Public ReadOnly Property HasBlocking As Boolean
        Get
            Return Findings.Any(Function(f) f.Blocking)
        End Get
    End Property

    Public Function Find(k_name As String) As AdePlanTable
        Return Tables.First(Function(t) String.Equals(t.Table.Name, k_name, StringComparison.OrdinalIgnoreCase))
    End Function

    ' ------------------------------------------------------------------------------------------------------------

    ''' <param name="k_educators">The names the operator settled on, per (group, text in Access). A text that is not in
    ''' the dictionary is split by the default rule.</param>
    Public Shared Function Build(k_source As AdeSource, k_educators As IDictionary(Of (Integer, String), String)) As AdePlan
        Try
            Dim k_plan As New AdePlan(k_source.ReceiptConfig)
            k_plan.ConvertTables(k_source)
            k_plan.FixLinks()
            k_plan.SetGroupTypes()
            k_plan.FillDerived(k_source)
            k_plan.BuildEducators(k_source, k_educators)
            k_plan.BuildHistory(k_source)
            Return k_plan
        Catch ex As Exception
            GlobalErrorLog.Write("AdePlan.Build", ex)
            Throw
        End Try
    End Function

    Private Sub New(k_receipt As AdeReceiptConfig)
        ReceiptConfig = k_receipt
    End Sub

    Private Sub Block(k_text As String)
        Findings.Add(New AdeFinding(True, k_text))
    End Sub

    ' ---- 1. column by column ---------------------------------------------------------------------------------------

    Private Sub ConvertTables(k_source As AdeSource)
        For Each k_table In AdeSchema.Tables
            Dim k_src = k_source.Tables(k_table.Name)
            Dim k_planTable As New AdePlanTable(k_table, k_src.Rows.Count)
            Tables.Add(k_planTable)

            Dim k_columns As New Dictionary(Of String, DataColumn)(StringComparer.OrdinalIgnoreCase)
            For Each k_dc As DataColumn In k_src.Columns
                k_columns(k_dc.ColumnName) = k_dc
            Next
            Dim k_ignored = k_columns.Keys.Where(Function(c) Not k_table.Columns.Any(Function(t) String.Equals(t.Name, c, StringComparison.OrdinalIgnoreCase))).OrderBy(Function(c) c).ToList()
            If k_ignored.Count > 0 Then Notes.Add($"{k_table.Name}: coloane din Access care nu se mai iau — {String.Join(", ", k_ignored)}.")

            Dim k_rounded As New Dictionary(Of String, (Count As Integer, Example As String))()
            Dim k_keys As New HashSet(Of Object)()
            For Each k_row As DataRow In k_src.Rows
                Dim k_out As New Dictionary(Of String, Object)(StringComparer.Ordinal)
                For Each k_col In k_table.Columns
                    Dim k_dc As DataColumn = Nothing
                    Dim k_cell As Object = If(k_columns.TryGetValue(k_col.Name, k_dc), k_row(k_dc), Nothing)
                    Dim k_note As String = Nothing
                    k_out(k_col.Name) = ConvertCell(k_table, k_col, k_cell, k_note)
                    If k_note IsNot Nothing Then
                        Dim k_id = $"{k_table.Name}.{k_col.Name}"
                        Dim k_old = If(k_rounded.ContainsKey(k_id), k_rounded(k_id), (Count:=0, Example:=k_note))
                        k_rounded(k_id) = (k_old.Count + 1, k_old.Example)
                    End If
                Next
                Dim k_key = k_out(k_table.Key)
                If k_key Is Nothing Then
                    Block($"{k_table.Name}: un rând fără cheie ({k_table.Key}).")
                ElseIf Not k_keys.Add(k_key) Then
                    Block($"{k_table.Name}: cheie dublă {k_table.Key} = {k_key}.")
                End If
                k_planTable.Rows.Add(k_out)
            Next
            For Each k_pair In k_rounded
                Notes.Add($"{k_pair.Key}: {k_pair.Value.Count} valori nu au intrat întregi (ex.: {k_pair.Value.Example}).")
            Next
        Next
    End Sub

    Private Function ConvertCell(k_table As AdeTable, k_col As AdeColumn, k_cell As Object, ByRef k_note As String) As Object
        Select Case k_col.Kind
            Case AdeKind.Int
                Return AdeValue.ToInt(k_cell, k_note)
            Case AdeKind.Dbl
                Return AdeValue.ToDbl(k_cell)
            Case AdeKind.Flag
                Return AdeValue.ToFlag(k_cell)
            Case AdeKind.Moment
                Return AdeValue.ToMoment(k_cell)
            Case Else
                Dim k_text = AdeValue.ToStr(k_cell)
                Dim k_string = TryCast(k_text, String)
                If k_string IsNot Nothing AndAlso k_col.MaxLen > 0 AndAlso k_string.Length > k_col.MaxLen Then
                    Block($"{k_table.Name}.{k_col.Name}: un text are {k_string.Length} caractere, iar coloana primește cel mult {k_col.MaxLen}.")
                End If
                Return k_text
        End Select
    End Function

    ' ---- 2. links that point nowhere -------------------------------------------------------------------------------

    Private Sub FixLinks()
        Dim k_keys As New Dictionary(Of String, HashSet(Of Object))(StringComparer.OrdinalIgnoreCase)
        For Each k_planTable In Tables
            k_keys(k_planTable.Table.Name) = New HashSet(Of Object)(k_planTable.Rows.Select(Function(r) r(k_planTable.Table.Key)).Where(Function(v) v IsNot Nothing))
        Next
        For Each k_rel In AdeSchema.Relations
            Dim k_allowed = AdeSchema.DanglingAllowed.Contains($"{k_rel.Child}.{k_rel.Col}", StringComparer.OrdinalIgnoreCase)
            Dim k_count = 0
            Dim k_example As Object = Nothing
            For Each k_row In Find(k_rel.Child).Rows
                Dim k_value = k_row(k_rel.Col)
                If k_value Is Nothing OrElse k_keys(k_rel.Parent).Contains(k_value) Then Continue For
                k_count += 1
                If k_example Is Nothing Then k_example = k_value
                If k_allowed Then k_row(k_rel.Col) = Nothing
            Next
            If k_count = 0 Then Continue For
            If k_allowed Then
                Notes.Add($"{k_rel.Child}.{k_rel.Col}: {k_count} legături către {k_rel.Parent} nu mai există (ex.: {k_example}); se scrie gol.")
            Else
                Block($"{k_rel.Child}.{k_rel.Col}: {k_count} rânduri arată către {k_rel.Parent} inexistent (ex.: {k_example}).")
            End If
        Next
    End Sub

    ' ---- 3. group type ---------------------------------------------------------------------------------------------

    Private Sub SetGroupTypes()
        Dim k_departed As New List(Of String)()
        For Each k_row In Find("Grupe").Rows
            Dim k_name = TryCast(k_row("Grupa"), String)
            Dim k_isDeparted = k_name IsNot Nothing AndAlso k_name.IndexOf(DepartedWord, StringComparison.OrdinalIgnoreCase) >= 0
            k_row("Tip") = If(k_isDeparted, "PLECATI", "NORMALA")
            If k_isDeparted Then k_departed.Add(k_name)
        Next
        If k_departed.Count = 0 Then
            Notes.Add("Nicio grupă nu are «plecat» în nume: nu există grupa specială pentru copiii plecați (Tip = PLECATI).")
        Else
            Notes.Add($"Grupa pentru copiii plecați (Tip = PLECATI), recunoscută după nume: {String.Join("; ", k_departed)}.")
        End If
    End Sub

    ' ---- 4. values that Access repeated on every row, and the origin of the movements ------------------------------

    Private Sub FillDerived(k_source As AdeSource)
        ' The child's address (Access Platitori.Adresa) goes on every payer of that child whose own address is empty or just
        ' a town (shorter than 10 characters).
        Dim k_addresses As New Dictionary(Of Integer, String)()
        For Each k_row As DataRow In k_source.Tables("Platitori").Rows
            If Not k_source.Tables("Platitori").Columns.Contains("Adresa") Then Exit For
            Dim k_text = Convert.ToString(k_row("Adresa"), CultureInfo.InvariantCulture)
            Dim k_id = AdeValue.ToInt(k_row("IDP"), Nothing)
            If k_id IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(k_text) Then k_addresses(CInt(k_id)) = k_text.Trim()
        Next
        Dim k_copied = 0
        For Each k_row In Find("Platitori_sub").Rows
            Dim k_idp = k_row("IDP")
            Dim k_own = TryCast(k_row("Adresa"), String)
            If k_idp IsNot Nothing AndAlso k_addresses.ContainsKey(CInt(k_idp)) AndAlso (k_own Is Nothing OrElse k_own.Trim().Length < 10) Then
                k_row("Adresa") = k_addresses(CInt(k_idp))
                k_copied += 1
            End If
        Next
        If k_copied > 0 Then Notes.Add($"Adresa copilului copiată pe {k_copied} plătitori (adresa lor era goală sau doar localitatea).")

        ' Working days of the month: Access kept them on every attendance row; the month keeps the most frequent value.
        Dim k_days As New Dictionary(Of Integer, Dictionary(Of Integer, Integer))()
        Dim k_prezenta = k_source.Tables("Prezenta")
        If k_prezenta.Columns.Contains("ZileLuna") Then
            For Each k_row As DataRow In k_prezenta.Rows
                Dim k_idl = AdeValue.ToInt(k_row("IDL"), Nothing)
                Dim k_zile = AdeValue.ToInt(k_row("ZileLuna"), Nothing)
                If k_idl Is Nothing OrElse k_zile Is Nothing Then Continue For
                If Not k_days.ContainsKey(CInt(k_idl)) Then k_days(CInt(k_idl)) = New Dictionary(Of Integer, Integer)()
                Dim k_bucket = k_days(CInt(k_idl))
                k_bucket(CInt(k_zile)) = If(k_bucket.ContainsKey(CInt(k_zile)), k_bucket(CInt(k_zile)), 0) + 1
            Next
        End If
        Dim k_months = 0
        Dim k_monthOf As New Dictionary(Of Integer, (Luna As Integer, Anul As Integer))()
        For Each k_row In Find("LunaD").Rows
            If k_row("IDL") Is Nothing Then Continue For
            Dim k_idl = CInt(k_row("IDL"))
            If k_row("Luna") IsNot Nothing AndAlso k_row("Anul") IsNot Nothing Then k_monthOf(k_idl) = (CInt(k_row("Luna")), CInt(k_row("Anul")))
            If k_row("ZileLuna") Is Nothing AndAlso k_days.ContainsKey(k_idl) Then
                k_row("ZileLuna") = k_days(k_idl).OrderByDescending(Function(p) p.Value).First().Key
                k_months += 1
            End If
        Next
        If k_months > 0 Then Notes.Add($"ZileLuna completat pe {k_months} luni (valoarea cea mai frecventă din prezență).")

        ' Month and year of origin of payments and refunds, so a re-opened month can take its movements back.
        For Each k_name In {"Plati", "Retur"}
            Dim k_set = 0
            For Each k_row In Find(k_name).Rows
                If k_row("IDL") Is Nothing OrElse Not k_monthOf.ContainsKey(CInt(k_row("IDL"))) Then Continue For
                Dim k_month = k_monthOf(CInt(k_row("IDL")))
                k_row("OriginMonth") = k_month.Luna
                k_row("OriginYear") = k_month.Anul
                k_set += 1
            Next
            Notes.Add($"{k_name}: luna și anul de proveniență (OriginMonth/OriginYear) puse pe {k_set} rânduri, din luna legată.")
        Next
    End Sub

    ' ---- 5. educators in time ---------------------------------------------------------------------------------------

    Private Function NamesOf(k_educators As IDictionary(Of (Integer, String), String), k_idg As Integer, k_raw As String) As List(Of String)
        If k_raw Is Nothing Then Return New List(Of String)()
        Dim k_text As String = Nothing
        If k_educators IsNot Nothing AndAlso k_educators.TryGetValue((k_idg, k_raw), k_text) Then Return AdeEducators.Split(k_text)
        Return AdeEducators.Split(k_raw)
    End Function

    Private Shared Function YearMonth(k_row As Dictionary(Of String, Object), k_months As Dictionary(Of Integer, Integer)) As Integer?
        If k_row("IDL") Is Nothing OrElse Not k_months.ContainsKey(CInt(k_row("IDL"))) Then Return Nothing
        Return k_months(CInt(k_row("IDL")))
    End Function

    Private Function MonthIndex() As Dictionary(Of Integer, Integer)
        Dim k_index As New Dictionary(Of Integer, Integer)()
        For Each k_row In Find("LunaD").Rows
            If k_row("IDL") IsNot Nothing AndAlso k_row("Luna") IsNot Nothing AndAlso k_row("Anul") IsNot Nothing Then
                k_index(CInt(k_row("IDL"))) = CInt(k_row("Anul")) * 12 + CInt(k_row("Luna")) - 1
            End If
        Next
        Return k_index
    End Function

    Private Shared Function StartOf(k_ym As Integer) As DateTime
        Return AdeValue.MonthStart(k_ym \ 12, k_ym Mod 12 + 1)
    End Function

    Private Shared Function EndOf(k_ym As Integer) As DateTime
        Return StartOf(k_ym).AddMonths(1).AddDays(-1)
    End Function

    Private Sub BuildEducators(k_source As AdeSource, k_educators As IDictionary(Of (Integer, String), String))
        Dim k_target As New AdePlanTable(AdeSchema.GrupeEducator, -1)
        Tables.Insert(Tables.IndexOf(Find("Grupe")) + 1, k_target)
        Dim k_monthIndex = MonthIndex()

        ' group -> month -> names found in the saved situation of that month
        Dim k_seen As New Dictionary(Of Integer, SortedDictionary(Of Integer, List(Of String)))()
        For Each k_row In Find("SS_Buget").Rows
            If k_row("IDG") Is Nothing Then Continue For
            Dim k_ym = YearMonth(k_row, k_monthIndex)
            If k_ym Is Nothing Then Continue For
            Dim k_idg = CInt(k_row("IDG"))
            If Not k_seen.ContainsKey(k_idg) Then k_seen(k_idg) = New SortedDictionary(Of Integer, List(Of String))()
            If Not k_seen(k_idg).ContainsKey(k_ym.Value) Then k_seen(k_idg)(k_ym.Value) = New List(Of String)()
            For Each k_name In NamesOf(k_educators, k_idg, TryCast(k_row("Educator"), String))
                If Not k_seen(k_idg)(k_ym.Value).Contains(k_name, StringComparer.OrdinalIgnoreCase) Then k_seen(k_idg)(k_ym.Value).Add(k_name)
            Next
        Next

        For Each k_group In Find("Grupe").Rows
            Dim k_idg = CInt(k_group("IDG"))
            Dim k_current = NamesOf(k_educators, k_idg, ReadCurrentEducator(k_source, k_idg))
            Dim k_months = If(k_seen.ContainsKey(k_idg), k_seen(k_idg), New SortedDictionary(Of Integer, List(Of String))())
            Dim k_order = k_months.Keys.ToList()
            Dim k_all As New List(Of String)()
            For Each k_list In k_months.Values
                For Each k_name In k_list
                    If Not k_all.Contains(k_name, StringComparer.OrdinalIgnoreCase) Then k_all.Add(k_name)
                Next
            Next
            For Each k_name In k_current
                If Not k_all.Contains(k_name, StringComparer.OrdinalIgnoreCase) Then k_all.Add(k_name)
            Next

            For Each k_name In k_all
                Dim k_runStart As Integer? = Nothing
                Dim k_previous As Integer = 0
                For k_i = 0 To k_order.Count - 1
                    Dim k_present = k_months(k_order(k_i)).Contains(k_name, StringComparer.OrdinalIgnoreCase)
                    Dim k_isLast = (k_i = k_order.Count - 1)
                    If k_present AndAlso k_runStart Is Nothing Then k_runStart = k_order(k_i)
                    If k_present Then k_previous = k_order(k_i)
                    If k_runStart IsNot Nothing AndAlso (Not k_present OrElse k_isLast) Then
                        Dim k_stillHere = k_isLast AndAlso k_present AndAlso k_current.Contains(k_name, StringComparer.OrdinalIgnoreCase)
                        AddEducatorRow(k_target, k_idg, k_name, StartOf(k_runStart.Value), If(k_stillHere, CType(Nothing, DateTime?), EndOf(k_previous)))
                        k_runStart = Nothing
                    End If
                Next
                ' A current educator not seen in the last saved month starts after it.
                Dim k_inLast = k_order.Count > 0 AndAlso k_months(k_order(k_order.Count - 1)).Contains(k_name, StringComparer.OrdinalIgnoreCase)
                If Not k_inLast AndAlso k_current.Contains(k_name, StringComparer.OrdinalIgnoreCase) Then
                    If k_order.Count = 0 Then
                        AddEducatorRow(k_target, k_idg, k_name, Nothing, Nothing)
                    Else
                        AddEducatorRow(k_target, k_idg, k_name, StartOf(k_order(k_order.Count - 1) + 1), Nothing)
                    End If
                End If
            Next
        Next
        Notes.Add($"Educatori: {k_target.Rows.Count} rânduri în Grupe_Educator, câte unul pe educator și perioadă (DeLa/PanaLa); un text cu doi educatori («A | B» sau «A / B») dă două rânduri.")
    End Sub

    Private Function ReadCurrentEducator(k_source As AdeSource, k_idg As Integer) As String
        For Each k_row As DataRow In k_source.Tables("Grupe").Rows
            Dim k_id = AdeValue.ToInt(k_row("IDG"), Nothing)
            If k_id IsNot Nothing AndAlso CInt(k_id) = k_idg Then Return Convert.ToString(k_row("Educator"), CultureInfo.InvariantCulture)
        Next
        Return Nothing
    End Function

    Private Sub AddEducatorRow(k_target As AdePlanTable, k_idg As Integer, k_name As String, k_from As DateTime?, k_until As DateTime?)
        If k_name.Length > 50 Then Block($"Grupe_Educator: numele «{k_name}» are peste 50 de caractere.")
        k_target.Rows.Add(New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
            {"IDG", k_idg}, {"Educator", k_name}, {"DeLa", If(k_from.HasValue, CType(k_from.Value, Object), Nothing)},
            {"PanaLa", If(k_until.HasValue, CType(k_until.Value, Object), Nothing)}})
    End Sub

    ' ---- 6. the history of each child -------------------------------------------------------------------------------

    Private Sub BuildHistory(k_source As AdeSource)
        Dim k_target As New AdePlanTable(AdeSchema.PlatitoriIstoric, -1)
        Tables.Add(k_target)
        Dim k_monthIndex = MonthIndex()
        Dim k_departed As New HashSet(Of Integer)(Find("Grupe").Rows.Where(Function(r) CStr(r("Tip")) = "PLECATI").Select(Function(r) CInt(r("IDG"))))

        ' child -> month -> group, from the saved situations
        Dim k_path As New Dictionary(Of Integer, SortedDictionary(Of Integer, Integer))()
        For Each k_row In Find("SS_Buget").Rows
            Dim k_ym = YearMonth(k_row, k_monthIndex)
            If k_row("IDP") Is Nothing OrElse k_row("IDG") Is Nothing OrElse k_ym Is Nothing Then Continue For
            Dim k_idp = CInt(k_row("IDP"))
            If Not k_path.ContainsKey(k_idp) Then k_path(k_idp) = New SortedDictionary(Of Integer, Integer)()
            k_path(k_idp)(k_ym.Value) = CInt(k_row("IDG"))
        Next

        Dim k_entries = 0, k_exits = 0, k_moves = 0, k_returns = 0
        For Each k_child In Find("Platitori").Rows
            Dim k_idp = CInt(k_child("IDP"))
            Dim k_steps = If(k_path.ContainsKey(k_idp), k_path(k_idp), New SortedDictionary(Of Integer, Integer)())
            Dim k_currentGroup = k_child("IDG")

            If k_child("DataIntrare") IsNot Nothing Then
                Dim k_first As Object = If(k_steps.Count > 0, CType(k_steps.First().Value, Object), k_currentGroup)
                AddHistory(k_target, k_idp, CType(k_child("DataIntrare"), DateTime), "INTRARE", Nothing, k_first, False, "din data intrării")
                k_entries += 1
            End If

            Dim k_deducedExit = False
            Dim k_before As Integer? = Nothing
            For Each k_step In k_steps
                If k_before.HasValue AndAlso k_before.Value <> k_step.Value Then
                    Dim k_kind = "MUTARE"
                    If k_departed.Contains(k_step.Value) AndAlso Not k_departed.Contains(k_before.Value) Then
                        k_kind = "PLECARE"
                        k_deducedExit = True
                        k_exits += 1
                    ElseIf k_departed.Contains(k_before.Value) AndAlso Not k_departed.Contains(k_step.Value) Then
                        k_kind = "REVENIRE"
                        k_returns += 1
                    Else
                        k_moves += 1
                    End If
                    AddHistory(k_target, k_idp, StartOf(k_step.Key), k_kind, k_before.Value, k_step.Value, True, "dedus din situațiile lunare (doar luna)")
                End If
                k_before = k_step.Value
            Next

            If Not k_deducedExit AndAlso k_child("Plecat") IsNot Nothing AndAlso CBool(k_child("Plecat")) AndAlso k_child("DataIesire") IsNot Nothing Then
                AddHistory(k_target, k_idp, CType(k_child("DataIesire"), DateTime), "PLECARE", Nothing, k_currentGroup, False, "din data ieșirii")
                k_exits += 1
            End If
        Next
        Notes.Add($"Istoric copii: {k_entries} intrări și {k_exits} plecări, {k_moves} mutări între grupe și {k_returns} reveniri; mutările sunt deduse din situațiile lunare (dată = prima zi a lunii, marcate «dedus»). MutaCopil din Access nu se citește.")
    End Sub

    Private Sub AddHistory(k_target As AdePlanTable, k_idp As Integer, k_when As DateTime, k_kind As String,
                           k_old As Object, k_new As Object, k_deduced As Boolean, k_note As String)
        k_target.Rows.Add(New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
            {"IDP", k_idp}, {"Data", k_when}, {"Tip", k_kind}, {"IDG_Vechi", k_old}, {"IDG_Nou", k_new},
            {"Dedus", k_deduced}, {"Utilizator", SystemUser}, {"Nota", k_note}})
    End Sub

End Class
