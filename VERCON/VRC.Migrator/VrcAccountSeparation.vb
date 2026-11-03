Imports System.Globalization
Imports System.IO
Imports System.Text

' SLICE-VC10-01: several independent destinations under the same existing subunit in the migration plan.
Public NotInheritable Class VrcAccountSeparation
    Private ReadOnly _accepted As HashSet(Of String)
    Private ReadOnly _holders As List(Of VrcOiHolderAssociation)
    Private _catalog As New List(Of VrcOiData)()
    Public ReadOnly Repeats As New List(Of VrcOiRepeat)()

    Public Sub New(k_accepted As HashSet(Of String), k_holders As List(Of VrcOiHolderAssociation))
        _accepted = k_accepted
        _holders = k_holders
    End Sub

    Public Function Build(k_plan As VrcPlan, k_choices As List(Of VrcOiChoice), Optional k_allData As IEnumerable(Of VrcOiData) = Nothing) As VrcOiConversion
        Dim k_result As New VrcOiConversion With {.BaseHash = k_plan.Hash()}
        Dim k_report As New StringBuilder()
        k_result.Blockers.AddRange(k_plan.Blockers)
        If k_choices.Count = 0 Then k_result.Blockers.Add("Bifați conturile de separat.")
        Dim k_available = If(k_allData, k_choices.Select(Function(k_c) k_c.Data)).Distinct().ToList()
        _catalog = k_available
        For Each k_choice In k_choices
            If Not {"OI", "MATERIALE"}.Contains(k_choice.TargetType) Then k_result.Blockers.Add($"{k_choice.Data.Source.Profile.Label}, «{k_choice.AccountName}»: alegeți tipul destinației (OI sau MATERIALE).")
            If String.IsNullOrWhiteSpace(k_choice.TargetName) OrElse k_choice.TargetName.Trim().Length > 100 Then k_result.Blockers.Add($"{k_choice.Data.Source.Profile.Label}, «{k_choice.AccountName}»: completați numele noii gestiuni (cel mult 100 de caractere).")
        Next
        For Each k_group In k_choices.GroupBy(Function(k_c) Origin(k_c.Data.Source) & "|" & k_c.AccountKey)
            If k_group.Select(Function(k_c) VrcOiData.Normal(k_c.TargetName) & "|" & k_c.TargetType).Distinct().Count() > 1 Then k_result.Blockers.Add($"«{k_group.First().AccountName}»: destinațiile diferă între ani. Folosiți același tip și același nume în toate bazele acestui cont.")
            Dim k_first = k_group.First()
            For Each k_data In k_available.Where(Function(k_d) Origin(k_d.Source) = Origin(k_first.Data.Source) AndAlso k_d.Rows("Masini").Any(Function(k_r) VrcOiData.Key(k_r, "IdMasina") = k_first.AccountKey))
                If Not k_group.Any(Function(k_c) String.Equals(k_c.Data.Source.Source.Path, k_data.Source.Source.Path, StringComparison.OrdinalIgnoreCase)) Then k_result.Blockers.Add($"Contul «{k_first.AccountName}» există și în {k_data.Source.Profile.Label}. Includeți și această bază în separare, ca să nu rămână același cont în gestiuni diferite între ani.")
            Next
        Next
        For Each k_duplicate In k_choices.Select(Function(k_c) k_c.Data).Distinct().GroupBy(Function(k_d) k_d.Source.Profile.HashBefore).Where(Function(k_g) k_g.Select(Function(k_d) k_d.Source.Source.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            k_result.Blockers.Add("Posibile copii ale aceleiași baze (conținut identic): " & String.Join("; ", k_duplicate.Select(Function(k_d) k_d.Source.Profile.Label)) & ". Lămuriți sursele înainte de separare.")
        Next
        For Each k_group In k_choices.Where(Function(k_c) {"OI", "MATERIALE"}.Contains(k_c.TargetType) AndAlso Not String.IsNullOrWhiteSpace(k_c.TargetName)).GroupBy(Function(k_c) VrcOiData.Normal(k_c.Data.Source.SubunitName) & "|" & VrcOiData.Normal(k_c.TargetName))
            Dim k_list = k_group.ToList()
            Dim k_first = k_list(0)
            If k_list.Select(Function(k_c) k_c.TargetType).Distinct().Count() > 1 Then
                k_result.Blockers.Add($"Gestiunea «{k_first.TargetName}» are tipuri diferite între conturile selectate.")
                Continue For
            End If
            Try
                Dim k_part As VrcOiConversion
                If k_first.TargetType = "OI" Then
                    Dim k_builder As New VrcOiBuilder(_accepted, _holders, _catalog)
                    k_part = k_builder.Build(k_plan, k_list, k_first.TargetName)
                    Repeats.AddRange(k_builder.Repeats)
                Else
                    k_part = Material(k_plan, k_list, k_first.TargetName)
                End If
                k_report.AppendLine(k_part.Report)
                k_result.Blockers.AddRange(k_part.Blockers)
                k_result.Generated.AddRange(k_part.Generated)
                For Each k_remaining In k_part.Residual
                    Dim k_existing As VrcOiData = Nothing
                    If Not k_result.Residual.TryGetValue(k_remaining.Key, k_existing) Then
                        k_result.Residual(k_remaining.Key) = k_remaining.Value
                    Else
                        For Each k_table In k_existing.Tables.Keys.ToList()
                            Dim k_allowed = k_remaining.Value.Rows(k_table).ToHashSet()
                            k_existing.Tables(k_table) = k_existing.Rows(k_table).Where(Function(k_r) k_allowed.Contains(k_r)).ToList()
                        Next
                    End If
                Next
            Catch ex As Exception When TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is KeyNotFoundException OrElse TypeOf ex Is FormatException OrElse TypeOf ex Is OverflowException OrElse TypeOf ex Is InvalidCastException
                k_result.Blockers.Add($"Gestiunea «{k_first.TargetName}»: {ex.Message}")
            End Try
        Next
        k_result.Blockers = k_result.Blockers.Distinct(StringComparer.Ordinal).ToList()
        If k_result.Blockers.Count > 0 Then
            k_report.AppendLine($"GENERARE BLOCATĂ — {k_result.Blockers.Count} probleme:")
            For Each k_problem In k_result.Blockers
                k_report.AppendLine("• " & k_problem)
            Next
        Else
            k_report.AppendLine($"Pregătite {k_result.Generated.Select(Function(k_s) VrcOiData.Normal(k_s.SubunitName) & "|" & VrcOiData.Normal(k_s.GestiuneName)).Distinct().Count()} gestiuni distincte. Materiale păstrează bazele anuale; OI folosește un exercițiu multianual.")
        End If
        k_result.Report = k_report.ToString()
        Return k_result
    End Function

    Private Function Material(k_plan As VrcPlan, k_choices As List(Of VrcOiChoice), k_name As String) As VrcOiConversion
        Dim k_result As New VrcOiConversion()
        Dim k_unit = k_choices(0).Data.Source.SubunitName
        If k_plan.Sources.Any(Function(k_s) VrcOiData.Normal(k_s.SubunitName) = VrcOiData.Normal(k_unit) AndAlso VrcOiData.Normal(k_s.GestiuneName) = VrcOiData.Normal(k_name)) Then k_result.Blockers.Add($"Denumirea «{k_name}» există deja în plan; alegeți un nume nou pentru gestiunea separată.")
        Dim k_report As New StringBuilder()
        Dim k_holderIds As New Dictionary(Of String, Long)(StringComparer.Ordinal)
        For Each k_group In k_choices.GroupBy(Function(k_c) k_c.Data).OrderBy(Function(k_g) k_g.Key.Source.Year)
            Dim k_data = k_group.Key
            Try
            Dim k_accounts = k_group.Select(Function(k_c) k_c.AccountKey).ToHashSet(StringComparer.Ordinal)
            Dim k_products = k_data.Rows("Produse").Where(Function(k_r) k_accounts.Contains(VrcOiData.Key(k_r, "IdMasina"))).Select(Function(k_r) VrcOiData.Key(k_r, "ID")).ToHashSet(StringComparer.Ordinal)
            Dim k_variants = k_data.Rows("ProduseP").Where(Function(k_r) k_products.Contains(VrcOiData.Key(k_r, "ID"))).Select(Function(k_r) VrcOiData.Key(k_r, "IdProdus")).ToHashSet(StringComparer.Ordinal)
            Dim k_nirs = MaterialDocuments(k_data, "NIR", "Intrari", "IdIntrare", k_accounts, k_variants, k_result.Blockers)
            Dim k_bons = MaterialDocuments(k_data, "Iesiri_H", "Iesiri", "IdBonConsum", k_accounts, k_variants, k_result.Blockers)
            Dim k_selected As New VrcOiData With {.Source = k_data.Source}
            Dim k_remaining As New VrcOiData With {.Source = k_data.Source}
            For Each k_table In k_data.Tables
                Dim k_nameTable = k_table.Key
                Dim k_spec = VrcImportMap.Find(k_nameTable)
                Dim k_filter As Func(Of Dictionary(Of String, Object), Boolean) = Nothing
                Select Case k_nameTable
                    Case "Masini" : k_filter = Function(k_r) k_accounts.Contains(VrcOiData.Key(k_r, "IdMasina"))
                    Case "Produse" : k_filter = Function(k_r) k_products.Contains(VrcOiData.Key(k_r, "ID"))
                    Case "ProduseP" : k_filter = Function(k_r) k_variants.Contains(VrcOiData.Key(k_r, "IdProdus"))
                    Case "NIR", "Intrari", "IntrariStorno" : k_filter = Function(k_r) k_nirs.Contains(VrcOiData.Key(k_r, "IdIntrare"))
                    Case "Iesiri_H", "Iesiri", "IesiriRetur", "LegBCLA" : k_filter = Function(k_r) k_bons.Contains(VrcOiData.Key(k_r, "IdBonConsum"))
                    Case "OI_Produse", "OI_Predare", "OI_Transfer", "OI_Casare" : k_filter = Function(k_r) k_variants.Contains(VrcOiData.Key(k_r, "IdProdus"))
                    Case Else
                        If k_spec IsNot Nothing AndAlso k_spec.IsExercise Then k_filter = Function(k_r) False
                End Select
                If k_filter Is Nothing Then
                    k_selected.Tables(k_nameTable) = k_table.Value.ToList()
                    k_remaining.Tables(k_nameTable) = k_table.Value.ToList()
                Else
                    k_selected.Tables(k_nameTable) = k_table.Value.Where(k_filter).ToList()
                    k_remaining.Tables(k_nameTable) = k_table.Value.Where(Function(k_r) Not k_filter(k_r)).ToList()
                End If
            Next
            Dim k_usedHolders = k_selected.Rows("Iesiri_H").Select(Function(k_r) VrcOiData.Key(k_r, "IDC")).ToHashSet(StringComparer.Ordinal)
            Dim k_map As New Dictionary(Of String, Long)(StringComparer.Ordinal)
            Dim k_clients As New List(Of Dictionary(Of String, Object))()
            Dim k_holderNotes As New List(Of String)()
            For Each k_old In k_usedHolders
                Dim k_alias = _holders.FirstOrDefault(Function(k_h) String.Equals(k_h.SourcePath, k_data.Source.Source.Path, StringComparison.OrdinalIgnoreCase) AndAlso k_h.SourceKey = k_old)
                Dim k_sourceName = VrcOiHolders.SourceName(k_data, k_old)
                Dim k_label = If(k_alias?.TargetName, VrcOiHolders.DefaultTarget(k_data, k_old))
                If String.IsNullOrWhiteSpace(k_label) Then
                    For Each k_bon In k_selected.Rows("Iesiri_H").Where(Function(k_r) VrcOiData.Key(k_r, "IDC") = k_old)
                        k_result.Blockers.Add($"{k_data.Source.Profile.Label}, {VrcOiHolders.DocumentLabel(k_bon)}: «{k_sourceName}». Alegeți deținătorul în grila de asocieri.")
                    Next
                    Continue For
                End If
                Dim k_copy As Dictionary(Of String, Object)
                Try
                    k_copy = VrcOiHolders.Resolve(k_data, k_old, k_label, _catalog)
                Catch ex As InvalidOperationException
                    For Each k_bon In k_selected.Rows("Iesiri_H").Where(Function(k_r) VrcOiData.Key(k_r, "IDC") = k_old)
                        k_result.Blockers.Add($"{k_data.Source.Profile.Label}, {VrcOiHolders.DocumentLabel(k_bon)}: {ex.Message}")
                    Next
                    Continue For
                End Try
                Dim k_identity = VrcOiData.Normal(k_label)
                If Not k_holderIds.ContainsKey(k_identity) Then k_holderIds(k_identity) = k_holderIds.Count + 1
                k_map(k_old) = k_holderIds(k_identity)
                k_copy("IDC") = k_holderIds(k_identity)
                If Not k_clients.Any(Function(k_r) CLng(k_r("IDC")) = k_holderIds(k_identity)) Then k_clients.Add(k_copy)
                k_holderNotes.Add($"Deținător: «{k_sourceName}» din {k_data.Source.Profile.Label} → «{k_label.Trim()}».")
            Next
            k_selected.Tables("Clienti") = k_clients
            k_selected.Tables("Iesiri_H") = k_selected.Rows("Iesiri_H").Select(Function(k_r) VrcOiData.CloneRow(k_r)).ToList()
            For Each k_bon In k_selected.Rows("Iesiri_H")
                Dim k_old = VrcOiData.Key(k_bon, "IDC")
                k_bon("IDC") = If(k_map.ContainsKey(k_old), CType(k_map(k_old), Object), Nothing)
            Next
            Dim k_note = $"{k_data.Source.Profile.Label}: {String.Join(", ", k_group.Select(Function(k_c) k_c.AccountName))} → «{k_name}» (MATERIALE); {k_nirs.Count} NIR și {k_bons.Count} bonuri păstrate, fără transformare în BP."
            k_report.AppendLine(k_note)
            k_result.Residual(k_data.Source.Source.Path) = k_remaining
            k_result.Generated.Add(New VrcPlanSource With {.Source = k_data.Source.Source, .Profile = k_data.Source.Profile, .Year = k_data.Source.Year,
                .GestiuneName = k_name.Trim(), .GestiuneType = "MATERIALE", .SubunitName = k_unit,
                .PreparedRows = k_selected.Tables, .PreparedHash = VrcOiData.Fingerprint(k_selected.Tables), .IsSeparatedSource = True,
                .PreparedNotes = New List(Of String)(k_holderNotes) From {k_note}})
            Catch ex As Exception When TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is KeyNotFoundException OrElse TypeOf ex Is FormatException OrElse TypeOf ex Is OverflowException OrElse TypeOf ex Is InvalidCastException
                k_result.Blockers.Add($"{k_data.Source.Profile.Label}, gestiune «{k_name}»: {ex.Message}")
            End Try
        Next
        k_result.Report = k_report.ToString()
        Return k_result
    End Function

    Private Shared Function MaterialDocuments(k_data As VrcOiData, k_header As String, k_lines As String, k_key As String, k_accounts As HashSet(Of String), k_variants As HashSet(Of String), k_problems As List(Of String)) As HashSet(Of String)
        Dim k_result As New HashSet(Of String)(StringComparer.Ordinal)
        For Each k_row In k_data.Rows(k_header)
            Dim k_id = VrcOiData.Key(k_row, k_key)
            Dim k_items = k_data.Rows(k_lines).Where(Function(k_r) VrcOiData.Key(k_r, k_key) = k_id).ToList()
            If Not k_accounts.Contains(VrcOiData.Key(k_row, "IdMasina")) AndAlso Not k_items.Any(Function(k_r) k_variants.Contains(VrcOiData.Key(k_r, "IdProdus"))) Then Continue For
            If k_items.Any(Function(k_r) Not k_variants.Contains(VrcOiData.Key(k_r, "IdProdus"))) Then
                k_problems.Add($"{k_data.Source.Profile.Label}: documentul {VrcOiData.Text(k_row, If(k_header = "NIR", "Numar", "NumarBC"))}, din {VrcOiData.Text(k_row, If(k_header = "NIR", "Data", "DataBC"))}, conține și produse din alte conturi. Separarea parțială cere o decizie explicită.")
            Else
                k_result.Add(k_id)
            End If
        Next
        Dim k_headers = k_data.Rows(k_header).Select(Function(k_r) VrcOiData.Key(k_r, k_key)).ToHashSet(StringComparer.Ordinal)
        For Each k_row In k_data.Rows(k_lines).Where(Function(k_r) k_variants.Contains(VrcOiData.Key(k_r, "IdProdus")) AndAlso Not k_headers.Contains(VrcOiData.Key(k_r, k_key)))
            Dim k_variant = k_data.Rows("ProduseP").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "IdProdus") = VrcOiData.Key(k_row, "IdProdus"))
            Dim k_product = If(k_variant Is Nothing, Nothing, k_data.Rows("Produse").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "ID") = VrcOiData.Key(k_variant, "ID")))
            k_problems.Add($"{k_data.Source.Profile.Label}: produsul «{If(k_product Is Nothing, "nume lipsă", VrcOiData.Text(k_product, "DenumireP"))}» are o linie fără document părinte (antet lipsă).")
        Next
        Return k_result
    End Function

    Private Shared Function Origin(k_source As VrcPlanSource) As String
        Return VrcOiData.Normal(k_source.SubunitName) & "|" & VrcOiData.Normal(k_source.GestiuneName) & "|" & VrcOiData.Normal(Path.GetDirectoryName(k_source.Source.Path))
    End Function
End Class
