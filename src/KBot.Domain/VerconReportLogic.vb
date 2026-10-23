Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text.RegularExpressions

' Slice 0123-02 -- the rules of the VERCON Rapoarte page that the site keeps in its JavaScript (static/js/vercon/report-result.js,
' report-selection.js, report-period.js), ported without change of behaviour: how the catalog is laid out as a tree, which periods the
' period list offers, which ticked rows become which filter of the request. Pure functions, no I/O.

''' <summary>A node of the report tree (a group, or a report whose <see cref="Id"/> is <c>doc:</c> + the report's id).</summary>
Public NotInheritable Class VerconTreeNode
    Public Property Id As String = String.Empty
    Public Property Label As String = String.Empty
    Public ReadOnly Property Children As New List(Of VerconTreeNode)()
End Class

''' <summary>The catalog as the tree the operator picks a report from, and the group (category) every report belongs to.</summary>
Public NotInheritable Class VerconReportTree

    Public Const ReportPrefix As String = "doc:"

    ' Reports the site leaves out of the tree: they are reached from another report or are not offered to the operator.
    Private Shared ReadOnly Hidden As String() = {
        "OI_Predare", "OI_Transfer", "OI_Casare", "FM1-u", "FM2", "Fisa_Magazie_BF", "NIR_SFP", "NIR_Storno",
        "Centralizator_Conturi_Furnizori", "BUConsum", "BUConsum_F", "BUConsum_F1", "BonRetur", "MF_Balanta_Subinv"}

    Private Shared ReadOnly MfDocuments As String() = {"MF_Bon_BP", "MF_Bon_BT", "MF_Bon_BS"}
    Private Shared ReadOnly CustodyFamilies As String() = {"bp", "bt", "bs"}

    Private Sub New()
    End Sub

    ''' <summary>The tree, labels in capitals like on the site.</summary>
    Public Shared Function Build(k_reports As IEnumerable(Of VerconReportSpec)) As List(Of VerconTreeNode)
        ArgumentNullException.ThrowIfNull(k_reports)
        Dim k_nodes As List(Of VerconTreeNode) = BuildNodes(k_reports.ToList())
        Upper(k_nodes)
        Return k_nodes
    End Function

    ''' <summary>The group each report sits in (the report id -> the id of the node two levels up), used to keep the selection tree between two reports of a group.</summary>
    Public Shared Function CategoriesOf(k_nodes As IEnumerable(Of VerconTreeNode)) As Dictionary(Of String, String)
        ArgumentNullException.ThrowIfNull(k_nodes)
        Dim k_result As New Dictionary(Of String, String)(StringComparer.Ordinal)
        Walk(k_nodes, Nothing, 0, k_result)
        Return k_result
    End Function

    Private Shared Sub Walk(k_nodes As IEnumerable(Of VerconTreeNode), k_category As String, k_depth As Integer, k_result As Dictionary(Of String, String))
        For Each k_node As VerconTreeNode In k_nodes
            If k_node.Id.StartsWith(ReportPrefix, StringComparison.Ordinal) Then
                k_result(k_node.Id.Substring(ReportPrefix.Length)) = k_category
            Else
                Walk(k_node.Children, If(k_depth <= 1, k_node.Id, k_category), k_depth + 1, k_result)
            End If
        Next
    End Sub

    Private Shared Sub Upper(k_nodes As IEnumerable(Of VerconTreeNode))
        For Each k_node As VerconTreeNode In k_nodes
            k_node.Label = k_node.Label.ToUpper(CultureInfo.GetCultureInfo("ro-RO"))
            Upper(k_node.Children)
        Next
    End Sub

    Private Shared Function Doc(k_report As VerconReportSpec) As VerconTreeNode
        Return New VerconTreeNode() With {.Id = ReportPrefix & k_report.Id, .Label = k_report.Label}
    End Function

    Private Shared Function BuildNodes(k_all As List(Of VerconReportSpec)) As List(Of VerconTreeNode)
        ' The server names the groups itself (inventory objects): the tree is those groups, in the order they first appear.
        If k_all.Any(Function(k_r) k_r.TreeGroup.Length > 0) Then
            Dim k_named As New List(Of VerconTreeNode)()
            For Each k_group As String In k_all.Select(Function(k_r) k_r.TreeGroup).Distinct()
                Dim k_node As New VerconTreeNode() With {.Id = "tree-group:" & k_named.Count.ToString(CultureInfo.InvariantCulture), .Label = k_group}
                Dim k_name As String = k_group
                For Each k_r As VerconReportSpec In k_all.Where(Function(k_x) k_x.TreeGroup = k_name)
                    k_node.Children.Add(Doc(k_r))
                Next
                k_named.Add(k_node)
            Next
            Return k_named
        End If
        Dim k_reports As List(Of VerconReportSpec) = k_all.Where(Function(k_r) Not Hidden.Contains(k_r.Id)).ToList()
        Dim k_mf As Boolean = k_reports.Any(Function(k_r) k_r.Types.Count = 1 AndAlso k_r.Types(0) = "MF")
        Dim k_custody As List(Of VerconReportSpec) = k_reports.Where(Function(k_r) k_r.Ledger = "custody" OrElse
                                                                         CustodyFamilies.Contains(k_r.Family) OrElse k_r.Kind = "depreciation").ToList()
        If k_custody.Count > 0 Then
            Dim k_rest As List(Of VerconReportSpec) = k_reports.Where(Function(k_r) Not k_custody.Contains(k_r)).ToList()
            Dim k_result As List(Of VerconTreeNode) = If(k_rest.Count > 0, BuildNodes(k_rest), New List(Of VerconTreeNode)())
            Dim k_holder As New VerconTreeNode() With {
                .Id = "oi-reports", .Label = If(k_mf, "Mijloace fixe", "Obiecte de inventar în folosință")}
            If k_mf Then
                For Each k_id As String In MfDocuments
                    For Each k_r As VerconReportSpec In k_custody.Where(Function(k_x) k_x.Id = k_id)
                        k_holder.Children.Add(Doc(k_r))
                    Next
                Next
                For Each k_r As VerconReportSpec In k_custody.Where(Function(k_x) Not MfDocuments.Contains(k_x.Id))
                    k_holder.Children.Add(Doc(k_r))
                Next
            Else
                Dim k_index As Integer = 0
                For Each k_group As String In k_custody.Select(Function(k_r) k_r.Group).Distinct()
                    Dim k_node As New VerconTreeNode() With {.Id = "oi-reports:" & k_index.ToString(CultureInfo.InvariantCulture), .Label = k_group}
                    Dim k_name As String = k_group
                    For Each k_r As VerconReportSpec In k_custody.Where(Function(k_x) k_x.Group = k_name)
                        k_node.Children.Add(Doc(k_r))
                    Next
                    k_holder.Children.Add(k_node)
                    k_index += 1
                Next
            End If
            k_result.Add(k_holder)
            Return k_result
        End If

        ' Magazie: Produse / Intrari / Iesiri / Liste de alimente. The key is ASCII (never read); the label is what the operator sees.
        Dim k_groups As New Dictionary(Of String, List(Of VerconTreeNode))(StringComparer.Ordinal)
        For Each k_r As VerconReportSpec In k_reports.Where(Function(k_x) Not k_x.Id.EndsWith("subreport", StringComparison.Ordinal) AndAlso k_x.Id <> "BC subreport 2013")
            Dim k_key As String = WarehouseGroup(k_r)
            If Not k_groups.ContainsKey(k_key) Then k_groups(k_key) = New List(Of VerconTreeNode)()
            k_groups(k_key).Add(Doc(k_r))
        Next
        Dim k_products As List(Of VerconTreeNode) = Nothing
        If k_groups.TryGetValue("produse", k_products) Then
            ' Fisa magazie first (a stable sort: the rest keeps its order).
            k_groups("produse") = k_products.OrderBy(Function(k_n) If(k_n.Id = ReportPrefix & "Fisa_Magazie", 0, 1)).ToList()
        End If
        Dim k_warehouse As New VerconTreeNode() With {.Id = "warehouse", .Label = "Magazie"}
        For Each k_pair As KeyValuePair(Of String, String) In New List(Of KeyValuePair(Of String, String)) From {
                New KeyValuePair(Of String, String)("produse", "Produse"), New KeyValuePair(Of String, String)("intrari", "Intrări"),
                New KeyValuePair(Of String, String)("iesiri", "Ieșiri"), New KeyValuePair(Of String, String)("alimente", "Liste de alimente")}
            If Not k_groups.ContainsKey(k_pair.Key) Then Continue For
            Dim k_node As New VerconTreeNode() With {.Id = "reports:" & k_pair.Key, .Label = k_pair.Value}
            k_node.Children.AddRange(k_groups(k_pair.Key))
            k_warehouse.Children.Add(k_node)
        Next
        Return New List(Of VerconTreeNode) From {k_warehouse}
    End Function

    Private Shared Function WarehouseGroup(k_r As VerconReportSpec) As String
        If k_r.Kind = "food" Then Return "alimente"
        If k_r.Family = "bc" OrElse k_r.Family = "br" OrElse k_r.Kind.StartsWith("consumption", StringComparison.Ordinal) OrElse k_r.Id = "Centralizator_BC" Then Return "iesiri"
        If k_r.Family = "nir" OrElse k_r.Family = "storno" OrElse k_r.Kind = "receipts_summary" OrElse k_r.Kind = "suppliers" OrElse
           (k_r.Kind = "purchases" AndAlso (k_r.Id = "Nota_Comanda" OrElse k_r.Id = "Referat_periodic_Intrari")) Then Return "intrari"
        Return "produse"
    End Function
End Class

''' <summary>One row of the period list: a face text, the header (the year) and the row text under it, and the dates it stands for.</summary>
Public NotInheritable Class VerconPeriodItem
    Public Property Text As String = String.Empty

    ''' <summary>The year the row is listed under; empty for the rows above the years.</summary>
    Public Property Group As String = String.Empty
    Public Property RowText As String = String.Empty
    Public Property StartDate As Date
    Public Property EndDate As Date
End Class

''' <summary>The period list of the page («Anul curent pana azi», «TOTI anii», then each year with «Tot anul» and its months, newest first).</summary>
Public NotInheritable Class VerconPeriods

    Public Const CurrentToToday As String = "Anul curent până azi"
    Public Const AllYears As String = "TOTI anii"
    Public Const Custom As String = "Interval personalizat"

    Private Shared ReadOnly MonthNames As String() = {"Ianuarie", "Februarie", "Martie", "Aprilie", "Mai", "Iunie", "Iulie", "August",
                                                      "Septembrie", "Octombrie", "Noiembrie", "Decembrie"}

    Private Sub New()
    End Sub

    ''' <summary>The rows, in list order. The first one is the default when the server proposed «Anul curent pana azi».</summary>
    Public Shared Function Build(k_catalog As VerconCatalog) As List(Of VerconPeriodItem)
        ArgumentNullException.ThrowIfNull(k_catalog)
        Dim k_items As New List(Of VerconPeriodItem)()
        Dim k_start As Date
        Dim k_end As Date
        If TryIso(k_catalog.DefaultStart, k_start) AndAlso TryIso(k_catalog.DefaultEnd, k_end) Then
            k_items.Add(New VerconPeriodItem() With {.Text = CurrentToToday, .StartDate = k_start, .EndDate = k_end})
        End If
        If TryIso(k_catalog.AllYearsStart, k_start) AndAlso TryIso(k_catalog.AllYearsEnd, k_end) Then
            k_items.Add(New VerconPeriodItem() With {.Text = AllYears, .StartDate = k_start, .EndDate = k_end})
        End If
        For Each k_period As VerconPeriod In k_catalog.Periods.OrderByDescending(Function(k_p) k_p.Year)
            Dim k_year As Integer = k_period.Year
            Dim k_group As String = k_year.ToString(CultureInfo.InvariantCulture)
            Dim k_last As Integer = Math.Max(0, Math.Min(12, k_period.LastMovementMonth))
            k_items.Add(New VerconPeriodItem() With {
                .Text = "Tot anul / " & k_group, .Group = k_group, .RowText = "Tot anul",
                .StartDate = New Date(k_year, 1, 1), .EndDate = New Date(k_year, 12, 31)})
            For k_month As Integer = k_last To 1 Step -1
                k_items.Add(New VerconPeriodItem() With {
                    .Text = MonthNames(k_month - 1) & " / " & k_group, .Group = k_group, .RowText = MonthNames(k_month - 1),
                    .StartDate = New Date(k_year, k_month, 1), .EndDate = New Date(k_year, k_month, Date.DaysInMonth(k_year, k_month))})
            Next
        Next
        Return k_items
    End Function

    ''' <summary>The row whose dates are exactly these, or Nothing (then the face says «Interval personalizat»).</summary>
    Public Shared Function Match(k_items As IEnumerable(Of VerconPeriodItem), k_start As Date, k_end As Date) As VerconPeriodItem
        ArgumentNullException.ThrowIfNull(k_items)
        Return k_items.FirstOrDefault(Function(k_i) k_i.StartDate.Date = k_start.Date AndAlso k_i.EndDate.Date = k_end.Date)
    End Function

    Private Shared Function TryIso(k_text As String, ByRef k_value As Date) As Boolean
        Return Date.TryParseExact(If(k_text, String.Empty), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, k_value)
    End Function
End Class

''' <summary>Which ticked rows of the selection tree become which filter of the request, and when a report cannot be asked for yet.</summary>
Public NotInheritable Class VerconSelectionLogic

    Private Shared ReadOnly LeafPattern As New Regex(
        "^(variant|category-filter|receiver|document|receipt-product|receipt-document-product|outgoing-item|mf-item):", RegexOptions.Compiled)
    Private Shared ReadOnly BranchPattern As New Regex(
        "^(category|document-supplier|receipt-supplier|nir-supplier|out-account|out-receiver|out-document|mf-branch):", RegexOptions.Compiled)

    Private Sub New()
    End Sub

    ''' <summary>A row that is a choice in itself (the ticks sit on these).</summary>
    Public Shared Function IsLeaf(k_id As String) As Boolean
        Return k_id IsNot Nothing AndAlso LeafPattern.IsMatch(k_id)
    End Function

    ''' <summary>A branch whose box speaks for everything under it, even before its children are loaded.</summary>
    Public Shared Function IsBoxedBranch(k_id As String) As Boolean
        Return k_id IsNot Nothing AndAlso BranchPattern.IsMatch(k_id)
    End Function

    ''' <summary>The start of the ids of the data rows that sit under a root row (so a root that holds ticked rows can be read again when the tree is rebuilt).</summary>
    Public Shared Function PrefixOf(k_rootId As String) As String
        ArgumentNullException.ThrowIfNull(k_rootId)
        Dim k_parts As String() = k_rootId.Split(":"c)
        If k_rootId.StartsWith("mf-branch:", StringComparison.Ordinal) Then Return "mf-item:" & k_parts(1) & ":"
        If k_rootId.StartsWith("out-account:", StringComparison.Ordinal) Then Return "outgoing-item:account:" & k_parts(1) & ":"
        If k_rootId.StartsWith("out-receiver:", StringComparison.Ordinal) Then Return "outgoing-item:receiver:" & k_parts(1) & ":"
        If k_rootId.StartsWith("out-document:", StringComparison.Ordinal) Then Return "outgoing-item:" & k_rootId.Substring("out-document:".Length) & ":"
        If k_rootId.StartsWith("outgoing-item:", StringComparison.Ordinal) Then Return k_rootId
        If k_rootId.StartsWith("receipt-supplier:", StringComparison.Ordinal) Then Return "receipt-product:" & k_parts(1) & ":"
        If k_rootId.StartsWith("nir-supplier:", StringComparison.Ordinal) Then Return "receipt-document-product:" & k_parts(1) & ":"
        If k_rootId.StartsWith("document-supplier:", StringComparison.Ordinal) Then Return "document:"
        If k_rootId.StartsWith("category:", StringComparison.Ordinal) OrElse k_rootId = "selection:products" Then Return "variant:"
        If k_rootId = "selection:use-accounts" Then Return "category-filter:"
        Return If(k_rootId.StartsWith("selection:", StringComparison.Ordinal), k_rootId.Substring("selection:".Length), k_rootId) & ":"
    End Function

    ''' <summary>The filters of the request for the ticked leaves. <paramref name="k_allTicked"/>: every leaf of the whole tree is ticked (then the filters mean «everything» and are left out, unless the report requires them).</summary>
    Public Shared Function Filters(k_ticked As IEnumerable(Of String), k_spec As VerconReportSpec, k_allTicked As Boolean) As Dictionary(Of String, String)
        ArgumentNullException.ThrowIfNull(k_ticked)
        ArgumentNullException.ThrowIfNull(k_spec)
        Dim k_receipts As New List(Of String)()
        Dim k_outgoing As New List(Of String)()
        Dim k_fixedAssets As New List(Of String)()
        Dim k_collected As New Dictionary(Of String, List(Of String))(StringComparer.Ordinal)
        For Each k_id As String In k_ticked
            If k_id.StartsWith("supplier:", StringComparison.Ordinal) Then Continue For
            If k_id.StartsWith("mf-item:", StringComparison.Ordinal) Then
                If k_spec.MfSelection.Length > 0 AndAlso Not k_allTicked Then k_fixedAssets.Add(k_id)
                Continue For
            End If
            If k_id.StartsWith("receipt-product:", StringComparison.Ordinal) OrElse k_id.StartsWith("receipt-document-product:", StringComparison.Ordinal) Then
                If k_spec.IncomingSelection AndAlso Not k_allTicked Then k_receipts.Add(k_id)
                Continue For
            End If
            If k_id.StartsWith("outgoing-item:", StringComparison.Ordinal) Then
                If (k_spec.OutgoingSelection OrElse k_spec.OutgoingFilters) AndAlso Not k_allTicked Then k_outgoing.Add(k_id)
                Continue For
            End If
            Dim k_parts As String() = k_id.Split(":"c, 2)
            If k_parts.Length < 2 Then Continue For
            Dim k_key As String = If(k_parts(0) = "category-filter", "category", k_parts(0))
            If Not {"variant", "category", "receiver", "document"}.Contains(k_key) Then Continue For
            If Not k_spec.Filters.Contains(k_key) Then Continue For
            If k_allTicked AndAlso Not k_spec.Required(k_key) Then Continue For
            ' The value is what follows the first colon only up to the next one, as the site reads it.
            Dim k_value As String = k_id.Split(":"c)(1)
            If Not k_collected.ContainsKey(k_key) Then k_collected(k_key) = New List(Of String)()
            k_collected(k_key).Add(k_value)
        Next
        Dim k_result As New Dictionary(Of String, String)(StringComparer.Ordinal)
        If k_receipts.Count > 0 Then k_result("receipt_selection") = String.Join(",", k_receipts)
        If k_outgoing.Count > 0 Then k_result("outgoing_selection") = String.Join(",", k_outgoing)
        If k_fixedAssets.Count > 0 Then k_result("mf_selection") = String.Join(",", k_fixedAssets)
        For Each k_pair As KeyValuePair(Of String, List(Of String)) In k_collected
            If k_pair.Value.Count = 1 Then
                k_result(k_pair.Key) = k_pair.Value(0)
            Else
                k_result(k_pair.Key & "_ids") = String.Join(",", k_pair.Value)
            End If
        Next
        Return k_result
    End Function

    ''' <summary>The reason the report cannot be asked for yet, or an empty text.</summary>
    Public Shared Function SelectionError(k_spec As VerconReportSpec, k_ticked As IEnumerable(Of String)) As String
        If k_spec Is Nothing Then Return "Alegeți raportul."
        ArgumentNullException.ThrowIfNull(k_ticked)
        Dim k_list As List(Of String) = k_ticked.ToList()
        For Each k_pair As KeyValuePair(Of String, String) In New List(Of KeyValuePair(Of String, String)) From {
                New KeyValuePair(Of String, String)("variant", "produs / variantă"),
                New KeyValuePair(Of String, String)("supplier", "furnizor"),
                New KeyValuePair(Of String, String)("document", "document")}
            If k_spec.Required(k_pair.Key) AndAlso k_list.Where(Function(k_id) k_id.StartsWith(k_pair.Key & ":", StringComparison.Ordinal)).Count() <> 1 Then
                Return $"Alegeți un singur {k_pair.Value} pentru acest raport."
            End If
        Next
        Return String.Empty
    End Function
End Class
