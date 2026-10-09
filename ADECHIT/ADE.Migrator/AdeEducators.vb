Imports System.Data
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>One educator text found in Access for a group, and the names the migration proposes to write for it.</summary>
Public NotInheritable Class AdeEducatorEntry

    Public Sub New(k_idg As Integer, k_group As String, k_raw As String, k_names As String)
        IdG = k_idg
        GroupName = k_group
        Raw = k_raw
        Names = k_names
    End Sub

    Public ReadOnly Property IdG As Integer
    Public ReadOnly Property GroupName As String
    ''' <summary>The text exactly as it is in Access.</summary>
    Public ReadOnly Property Raw As String
    ''' <summary>Proposed names, <c>Name | Name</c>; the operator may correct it before writing.</summary>
    Public ReadOnly Property Names As String

End Class

''' <summary>
''' Access keeps both educators of a group in one text («Zamfir Cristina  / Andrei Delia»). The migration writes one row
''' per educator, so the text is split on «|» and «/», runs of spaces become one, and «-» (no educator) is dropped.
''' </summary>
Public NotInheritable Class AdeEducators

    Private Shared ReadOnly Separators As Char() = {"|"c, "/"c}
    Private Shared ReadOnly Blanks As New Regex("\s+", RegexOptions.Compiled)

    Private Sub New()
    End Sub

    ''' <summary>The names inside one text, cleaned. Empty when the text names nobody.</summary>
    Public Shared Function Split(k_text As String) As List(Of String)
        Dim k_names As New List(Of String)()
        If String.IsNullOrWhiteSpace(k_text) Then Return k_names
        For Each k_part In k_text.Split(Separators)
            Dim k_clean = Blanks.Replace(k_part, " ").Trim()
            If k_clean.Length = 0 OrElse k_clean = "-" Then Continue For
            If Not k_names.Contains(k_clean, StringComparer.OrdinalIgnoreCase) Then k_names.Add(k_clean)
        Next
        Return k_names
    End Function

    Public Shared Function Join(k_names As IEnumerable(Of String)) As String
        Return String.Join(" | ", k_names)
    End Function

    ''' <summary>
    ''' Every distinct educator text per group: the current one (Grupe) and the ones of the saved months (SS_Buget), so a
    ''' name written two ways shows up as two lines the operator can make identical.
    ''' </summary>
    Public Shared Function Candidates(k_source As AdeSource) As List(Of AdeEducatorEntry)
        Dim k_found As New List(Of AdeEducatorEntry)()
        Dim k_seen As New HashSet(Of String)(StringComparer.Ordinal)
        Dim k_groups As New Dictionary(Of Integer, String)()
        For Each k_row As DataRow In k_source.Tables("Grupe").Rows
            Dim k_note As String = Nothing
            Dim k_id = AdeValue.ToInt(k_row("IDG"), k_note)
            If k_id Is Nothing Then Continue For
            Dim k_name = Convert.ToString(k_row("Grupa"), CultureInfo.InvariantCulture)
            k_groups(CInt(k_id)) = k_name
            Add(k_found, k_seen, CInt(k_id), k_name, Convert.ToString(k_row("Educator"), CultureInfo.InvariantCulture))
        Next
        Dim k_ss = k_source.Tables("SS_Buget")
        If k_ss.Columns.Contains("Educator") AndAlso k_ss.Columns.Contains("IDG") Then
            For Each k_row As DataRow In k_ss.Rows
                Dim k_note As String = Nothing
                Dim k_id = AdeValue.ToInt(k_row("IDG"), k_note)
                If k_id Is Nothing OrElse Not k_groups.ContainsKey(CInt(k_id)) Then Continue For
                Add(k_found, k_seen, CInt(k_id), k_groups(CInt(k_id)), Convert.ToString(k_row("Educator"), CultureInfo.InvariantCulture))
            Next
        End If
        k_found.Sort(Function(a, b)
                         Dim k_cmp = a.IdG.CompareTo(b.IdG)
                         Return If(k_cmp <> 0, k_cmp, String.CompareOrdinal(a.Raw, b.Raw))
                     End Function)
        Return k_found
    End Function

    Private Shared Sub Add(k_list As List(Of AdeEducatorEntry), k_seen As HashSet(Of String), k_idg As Integer,
                           k_group As String, k_raw As String)
        If k_raw Is Nothing Then Return
        If Not k_seen.Add($"{k_idg}|{k_raw}") Then Return
        k_list.Add(New AdeEducatorEntry(k_idg, k_group, k_raw, Join(Split(k_raw))))
    End Sub

End Class
