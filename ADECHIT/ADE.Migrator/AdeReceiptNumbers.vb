Imports System.Globalization

''' <summary>One receipt of the plan, with the number it has in Access.</summary>
Public NotInheritable Class AdeReceipt

    Public Sub New(k_id As Integer, k_paymentId As Integer?, k_issuedOn As DateTime?, k_series As String, k_number As Integer?, k_cancelled As Boolean)
        Id = k_id
        PaymentId = k_paymentId
        IssuedOn = k_issuedOn
        Series = k_series
        Number = k_number
        Cancelled = k_cancelled
    End Sub

    ''' <summary>IDC in Access.</summary>
    Public ReadOnly Property Id As Integer
    ''' <summary>IDPL in Access.</summary>
    Public ReadOnly Property PaymentId As Integer?
    Public ReadOnly Property IssuedOn As DateTime?
    Public ReadOnly Property Series As String
    ''' <summary>The number as Access has it (the operator's edits live apart, in a dictionary).</summary>
    Public ReadOnly Property Number As Integer?
    Public ReadOnly Property Cancelled As Boolean

End Class

''' <summary>
''' Receipt numbers that repeat inside one series. The target key <c>uq_Sub_Serie_Numar</c> allows each (series, number) once per
''' subunit, cancelled receipts and other years included, so every repeat has to get another number before the write.
''' The number column is a whole number: a suffix such as «1234.1» does not fit it.
''' </summary>
Public NotInheritable Class AdeReceiptNumbers

    Private Sub New()
    End Sub

    ''' <summary>The number a receipt will have: the operator's edit, else the one from Access.</summary>
    Public Shared Function Current(k_receipt As AdeReceipt, k_edits As IDictionary(Of Integer, Integer)) As Integer?
        Dim k_value As Integer
        If k_edits IsNot Nothing AndAlso k_edits.TryGetValue(k_receipt.Id, k_value) Then Return k_value
        Return k_receipt.Number
    End Function

    Public Shared Function GroupKey(k_series As String, k_number As Integer) As String
        Return k_series & "|" & k_number.ToString(CultureInfo.InvariantCulture)
    End Function

    ''' <summary>Receipts with a series and a number, grouped by (series, number) as they will be written.</summary>
    Public Shared Function Groups(k_receipts As IEnumerable(Of AdeReceipt), k_edits As IDictionary(Of Integer, Integer)) As Dictionary(Of String, List(Of AdeReceipt))
        Dim k_groups As New Dictionary(Of String, List(Of AdeReceipt))(StringComparer.Ordinal)
        For Each k_receipt In k_receipts
            Dim k_number = Current(k_receipt, k_edits)
            ' A SQL unique key permits repeats when any component is NULL.
            If k_receipt.Series Is Nothing OrElse Not k_number.HasValue Then Continue For
            Dim k_key = GroupKey(k_receipt.Series, k_number.Value)
            If Not k_groups.ContainsKey(k_key) Then k_groups(k_key) = New List(Of AdeReceipt)()
            k_groups(k_key).Add(k_receipt)
        Next
        Return k_groups
    End Function

    ''' <summary>The receipts that share their number in Access (no edits applied): the rows the correction window lists.</summary>
    Public Shared Function Duplicated(k_receipts As IEnumerable(Of AdeReceipt)) As List(Of AdeReceipt)
        Return Groups(k_receipts, Nothing).Where(Function(k_pair) k_pair.Value.Count > 1).
            SelectMany(Function(k_pair) k_pair.Value).
            OrderBy(Function(k_r) k_r.Series, StringComparer.Ordinal).ThenBy(Function(k_r) k_r.Number).
            ThenBy(Function(k_r) If(k_r.IssuedOn, DateTime.MinValue)).ThenBy(Function(k_r) k_r.Id).ToList()
    End Function

    ''' <summary>What the window says about a receipt after the edits so far.</summary>
    Public Shared Function Status(k_receipt As AdeReceipt, k_groups As Dictionary(Of String, List(Of AdeReceipt)), k_edits As IDictionary(Of Integer, Integer)) As String
        Dim k_number = Current(k_receipt, k_edits)
        If k_receipt.Series Is Nothing OrElse Not k_number.HasValue Then Return "Fără serie sau număr"
        Dim k_group = k_groups(GroupKey(k_receipt.Series, k_number.Value))
        If k_group.Count > 1 Then
            Return "Încă în dublu cu IDC " & String.Join(", ", k_group.Where(Function(k_r) k_r.Id <> k_receipt.Id).Select(Function(k_r) k_r.Id))
        End If
        Return If(k_edits IsNot Nothing AndAlso k_edits.ContainsKey(k_receipt.Id), "Schimbat", "Rezolvat (nu mai este dublu)")
    End Function

    ''' <summary>The thousands digit taken out: 1234 → 234, 19387 → 1387. Nothing below 1000 or when the result would be 0.</summary>
    Public Shared Function StripThousands(k_number As Integer) As Integer?
        If k_number < 1000 Then Return Nothing
        Dim k_result As Integer = (k_number \ 10000) * 1000 + (k_number Mod 1000)
        Return If(k_result > 0, CType(k_result, Integer?), Nothing)
    End Function

    ''' <summary>
    ''' In every set of repeated numbers, all but the newest receipt (by date, then IDC) lose their thousands digit, when the new
    ''' number is free in the series. A receipt that cannot be changed that way is counted and left as it is.
    ''' </summary>
    Public Shared Function ApplyOlderRule(k_receipts As IEnumerable(Of AdeReceipt), k_edits As IDictionary(Of Integer, Integer)) As (Changed As Integer, Skipped As Integer)
        Dim k_changed As Integer = 0
        Dim k_skipped As Integer = 0
        Dim k_groups = Groups(k_receipts, k_edits)
        Dim k_taken As New HashSet(Of String)(k_groups.Keys, StringComparer.Ordinal)
        For Each k_pair In k_groups.OrderBy(Function(k_p) k_p.Key, StringComparer.Ordinal)
            If k_pair.Value.Count < 2 Then Continue For
            Dim k_ordered = k_pair.Value.OrderBy(Function(k_r) If(k_r.IssuedOn, DateTime.MinValue)).ThenBy(Function(k_r) k_r.Id).ToList()
            For k_index As Integer = 0 To k_ordered.Count - 2
                Dim k_receipt = k_ordered(k_index)
                Dim k_new = StripThousands(Current(k_receipt, k_edits).Value)
                If Not k_new.HasValue OrElse k_taken.Contains(GroupKey(k_receipt.Series, k_new.Value)) Then
                    k_skipped += 1
                    Continue For
                End If
                k_edits(k_receipt.Id) = k_new.Value
                k_taken.Add(GroupKey(k_receipt.Series, k_new.Value))
                k_changed += 1
            Next
        Next
        Return (k_changed, k_skipped)
    End Function

End Class
