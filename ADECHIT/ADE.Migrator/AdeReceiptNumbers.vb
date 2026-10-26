Imports System.Globalization

''' <summary>One receipt of the plan, with the number it has in Access.</summary>
Public NotInheritable Class AdeReceipt

    Public Sub New(k_id As Integer, k_issuedOn As DateTime?, k_series As String, k_number As Integer?, Optional k_subNumber As Integer = 0)
        Id = k_id
        IssuedOn = k_issuedOn
        Series = k_series
        Number = k_number
        SubNumber = k_subNumber
    End Sub

    ''' <summary>IDC in Access.</summary>
    Public ReadOnly Property Id As Integer
    Public ReadOnly Property IssuedOn As DateTime?
    Public ReadOnly Property Series As String
    ''' <summary>The number as Access has it; the migration never changes it.</summary>
    Public ReadOnly Property Number As Integer?
    ''' <summary>Access has no sub-number: 0 for a receipt read from Access.</summary>
    Public ReadOnly Property SubNumber As Integer

    ''' <summary>The school year the receipt belongs to (see <see cref="AdeReceiptNumbers.SchoolYearOf"/>); Nothing without a date.</summary>
    Public ReadOnly Property SchoolYear As Integer?
        Get
            Return AdeReceiptNumbers.SchoolYearOf(IssuedOn)
        End Get
    End Property

End Class

''' <summary>What <see cref="AdeReceiptNumbers.AssignSubNumbers"/> decided.</summary>
Public NotInheritable Class AdeSubNumbering

    ''' <summary>Sub-number in hundredths, like VC_NIR.NumarSub (10 prints as «,1»), per IDC in Access; receipts that keep no sub-number are absent.</summary>
    Public ReadOnly Property Subs As New Dictionary(Of Integer, Integer)()
    ''' <summary>How many (series, school year, number) were found more than once.</summary>
    Public Property RepeatedNumbers As Integer
    ''' <summary>Receipts that found no free sub-number (more than 100 receipts with one number in one school year): the migration is blocked for them.</summary>
    Public Property Skipped As Integer
    ''' <summary>A few repeated numbers, written for the operator.</summary>
    Public ReadOnly Property Examples As New List(Of String)()

End Class

''' <summary>
''' Receipt numbers that repeat inside one series AND one school year (September to August). The target key <c>uq_Sub_Serie_An_Numar</c>
''' (sql/AD_13) is (subunit, series, school year, number, sub-number), cancelled receipts included. The same number in another school
''' year is fine, because the numbering starts again every September. Inside one school year the repeats get a sub-number (751 and 751,1
''' are two numbers), the way NIR numbers do in VERCON; the number from Access is never changed.
''' </summary>
Public NotInheritable Class AdeReceiptNumbers

    Private Sub New()
    End Sub

    ''' <summary>The year in which the school year of a date began: September 2026 to August 2027 is 2026. Mirrors AD_Chitante.AnScolar.</summary>
    Public Shared Function SchoolYearOf(k_date As DateTime?) As Integer?
        If Not k_date.HasValue Then Return Nothing
        Return If(k_date.Value.Month < 9, k_date.Value.Year - 1, k_date.Value.Year)
    End Function

    ''' <summary>True when the target key covers the receipt: SQL keys ignore rows with a NULL part (series, school year or number).</summary>
    Public Shared Function InKey(k_receipt As AdeReceipt) As Boolean
        Return k_receipt.Series IsNot Nothing AndAlso k_receipt.SchoolYear.HasValue AndAlso k_receipt.Number.HasValue
    End Function

    ''' <summary>Receipts grouped by (series, school year, number, sub-number) as they will be written.</summary>
    Public Shared Function Groups(k_receipts As IEnumerable(Of AdeReceipt)) As Dictionary(Of String, List(Of AdeReceipt))
        Dim k_groups As New Dictionary(Of String, List(Of AdeReceipt))(StringComparer.Ordinal)
        For Each k_receipt In k_receipts
            If Not InKey(k_receipt) Then Continue For
            Dim k_key = k_receipt.Series & "|" & k_receipt.SchoolYear.Value.ToString(CultureInfo.InvariantCulture) & "|" &
                        k_receipt.Number.Value.ToString(CultureInfo.InvariantCulture) & "|" & k_receipt.SubNumber.ToString(CultureInfo.InvariantCulture)
            If Not k_groups.ContainsKey(k_key) Then k_groups(k_key) = New List(Of AdeReceipt)()
            k_groups(k_key).Add(k_receipt)
        Next
        Return k_groups
    End Function

    ''' <summary>The sub-numbers tried for a number, in order: ,1 ,2 … ,9 (10, 20 … 90 hundredths), then ,01 ,02 … ,99 without the tenths already tried.</summary>
    Private Shared Function SubCandidates() As List(Of Integer)
        Dim k_list As New List(Of Integer)()
        For k_tenth As Integer = 1 To 9
            k_list.Add(k_tenth * 10)
        Next
        For k_hundredth As Integer = 1 To 99
            If k_hundredth Mod 10 <> 0 Then k_list.Add(k_hundredth)
        Next
        Return k_list
    End Function

    ''' <summary>
    ''' In every set of receipts with the same series, school year and number: the NEWEST receipt (by date, then IDC) keeps the number
    ''' without a sub-number, the older ones keep the number and get ,1 ,2 … oldest first. Nothing is written here; the plan applies the result.
    ''' </summary>
    Public Shared Function AssignSubNumbers(k_receipts As IEnumerable(Of AdeReceipt)) As AdeSubNumbering
        Dim k_result As New AdeSubNumbering()
        Dim k_candidates = SubCandidates()
        For Each k_pair In Groups(k_receipts).OrderBy(Function(k_p) k_p.Key, StringComparer.Ordinal)
            If k_pair.Value.Count < 2 Then Continue For
            k_result.RepeatedNumbers += 1
            Dim k_ordered = k_pair.Value.OrderBy(Function(k_r) If(k_r.IssuedOn, DateTime.MinValue)).ThenBy(Function(k_r) k_r.Id).ToList()
            For k_index As Integer = 0 To k_ordered.Count - 2
                If k_index < k_candidates.Count Then
                    k_result.Subs(k_ordered(k_index).Id) = k_candidates(k_index)
                Else
                    k_result.Skipped += 1
                End If
            Next
            If k_result.Examples.Count < 5 Then
                Dim k_first = k_ordered(0)
                k_result.Examples.Add($"seria «{k_first.Series}», numărul {k_first.Number}, anul școlar {k_first.SchoolYear}/{k_first.SchoolYear + 1}, " &
                                      $"{k_ordered.Count} chitanțe (IDC {String.Join(", ", k_ordered.Take(4).Select(Function(k_r) k_r.Id))}{If(k_ordered.Count > 4, ", …", String.Empty)})")
            End If
        Next
        Return k_result
    End Function

End Class
