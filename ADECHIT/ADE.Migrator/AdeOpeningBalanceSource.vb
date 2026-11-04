Imports System.Data
Imports System.Globalization

''' <summary>A carry-forward value for one Access child present in both 2020 and 2021.</summary>
Public NotInheritable Class AdeOpeningBalance
    Public Property SourceId As Long
    Public Property Name As String
    Public Property Cnp As String
    Public Property Proposed As Integer
    Public Property OriginalInitial As Integer?
    Public Property HasProposed As Boolean
    Public Property Origin As String
    Public Property Problem As String = String.Empty
End Class

''' <summary>Read-only extraction of the balance before the retained 2021 history.</summary>
Public NotInheritable Class AdeOpeningBalanceSource
    Private Sub New()
    End Sub

    Public Shared Function Build(source As AdeSource) As List(Of AdeOpeningBalance)
        Dim months = source.Tables("LunaD").AsEnumerable().ToDictionary(Function(r) Id(r, "IDL"))
        Dim ids2020 = months.Where(Function(p) Number(p.Value, "Anul") = 2020).Select(Function(p) p.Key).ToHashSet()
        Dim ids2021 = months.Where(Function(p) Number(p.Value, "Anul") = 2021).Select(Function(p) p.Key).ToHashSet()
        If ids2020.Count = 0 OrElse ids2021.Count = 0 Then
            Throw New InvalidOperationException("MDB-ul complet trebuie să conțină lunile din 2020 și 2021.")
        End If
        Dim boundary = ids2021.Min()
        If months.Any(Function(p) (Number(p.Value, "Anul") < 2021) <> (p.Key < boundary)) Then
            Throw New InvalidOperationException("Ordinea lunilor din MDB nu corespunde anilor. Soldurile nu pot fi preluate automat.")
        End If
        Dim attendance = source.Tables("Prezenta").AsEnumerable().ToList()
        Dim snapshots = source.Tables("SS_Buget").AsEnumerable().ToList()
        Dim in2020 = attendance.Concat(snapshots).Where(Function(r) ids2020.Contains(Id(r, "IDL"))).Select(Function(r) Id(r, "IDP")).ToHashSet()
        Dim in2021 = attendance.Concat(snapshots).Where(Function(r) ids2021.Contains(Id(r, "IDL"))).Select(Function(r) Id(r, "IDP")).ToHashSet()
        Dim people = source.Tables("Platitori").AsEnumerable().ToDictionary(Function(r) Id(r, "IDP"))
        Dim attendanceMonths = attendance.ToDictionary(Function(r) Id(r, "IDZ"), Function(r) Id(r, "IDL"))
        Dim payments = source.Tables("Plati").AsEnumerable().ToList()
        Dim refunds = source.Tables("Retur").AsEnumerable().ToList()
        Dim result As New List(Of AdeOpeningBalance)()
        For Each child In in2020.Intersect(in2021).Where(Function(id) id > 0)
            Dim person As DataRow = Nothing
            people.TryGetValue(child, person)
            Dim saved = snapshots.Where(Function(r) Id(r, "IDP") = child AndAlso ids2021.Contains(Id(r, "IDL"))).OrderBy(Function(r) Id(r, "IDL")).ToList()
            Dim reference = If(person, saved.FirstOrDefault())
            Dim item As New AdeOpeningBalance With {.SourceId = child,
                .Name = If(reference Is Nothing, $"Copil Access {child}", Text(reference, "Nume")),
                .Cnp = If(reference Is Nothing, String.Empty, Text(reference, "CNP"))}
            result.Add(item)
            Try
                Dim childAttendance = attendance.Where(Function(r) Id(r, "IDP") = child).ToList()
                Dim childPayments = payments.Where(Function(r) Id(r, "IDP") = child AndAlso Active(r, "Anulata")).ToList()
                Dim childRefunds = refunds.Where(Function(r) Id(r, "IDP") = child AndAlso Active(r, "Anulat")).ToList()
                Dim refundMonth As Func(Of DataRow, Long) = Function(r)
                    If Not r.IsNull("IDL") Then Return Id(r, "IDL")
                    Dim linked As Long
                    Return If(attendanceMonths.TryGetValue(Id(r, "IDZ"), linked), linked, 0)
                End Function
                If childPayments.Any(Function(r) Id(r, "IDL") = 0 AndAlso (r.IsNull("Data") OrElse Before2021(r, "Data"))) OrElse
                   childRefunds.Any(Function(r) refundMonth(r) = 0 AndAlso (r.IsNull("Data") OrElse Before2021(r, "Data"))) Then
                    Throw New InvalidOperationException("Există mișcări vechi sau fără dată care nu au legătură cu luna; soldul nu poate fi stabilit complet.")
                End If
                Dim value As Double
                If person IsNot Nothing Then
                    item.OriginalInitial = Convert.ToInt32(Number(person, "SI"))
                    value = Number(person, "SI") + childAttendance.Where(Function(r) Id(r, "IDL") > 0 AndAlso Id(r, "IDL") < boundary).Sum(Function(r) Number(r, "ValoareContract")) -
                        childPayments.Where(Function(r) Id(r, "IDL") > 0 AndAlso Id(r, "IDL") < boundary).Sum(Function(r) Number(r, "Plata")) +
                        childRefunds.Where(Function(r) refundMonth(r) > 0 AndAlso refundMonth(r) < boundary).Sum(Function(r) Number(r, "Suma"))
                    item.Origin = "SI Access + contracte − plăți + restituiri, înainte de 2021"
                Else
                    If saved.Count = 0 OrElse saved.Where(Function(r) Id(r, "IDL") = Id(saved(0), "IDL")).Count() <> 1 Then
                        Throw New InvalidOperationException("Copilul nu mai există în Platitori și nu are o situație salvată unică în 2021.")
                    End If
                    If saved(0).IsNull("SID") AndAlso saved(0).IsNull("SIC") Then
                        Throw New InvalidOperationException("Prima situație salvată din 2021 nu conține soldul inițial.")
                    End If
                    Dim savedMonth = Id(saved(0), "IDL")
                    Dim retainedNet = childAttendance.Where(Function(r) Id(r, "IDL") >= boundary AndAlso Id(r, "IDL") < savedMonth).Sum(Function(r) Number(r, "ValoareContract")) -
                        childPayments.Where(Function(r) Id(r, "IDL") >= boundary AndAlso Id(r, "IDL") < savedMonth).Sum(Function(r) Number(r, "Plata")) +
                        childRefunds.Where(Function(r) refundMonth(r) >= boundary AndAlso refundMonth(r) < savedMonth).Sum(Function(r) Number(r, "Suma"))
                    value = Number(saved(0), "SID") - Number(saved(0), "SIC") - retainedNet
                    item.Origin = "SID − SIC din prima situație salvată în 2021, minus mișcările anterioare din 2021"
                End If
                ' Access Long uses midpoint-to-even conversion; do not add the carry to the destination twice.
                item.Proposed = Convert.ToInt32(value)
                item.HasProposed = True
                ' SLICE-AD19-04: saved situations preserve history, including later cancellation differences.
                ' They do not validate an opening derived from the current source movements.
            Catch ex As Exception When TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is OverflowException
                item.Problem = ex.Message
            End Try
        Next
        Return result.OrderBy(Function(r) r.Name, StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)).ToList()
    End Function

    Friend Shared Function Id(row As DataRow, column As String) As Long
        Return If(row.IsNull(column), 0L, Convert.ToInt64(row(column), CultureInfo.InvariantCulture))
    End Function

    Friend Shared Function Number(row As DataRow, column As String) As Double
        Return If(row.IsNull(column), 0.0, Convert.ToDouble(row(column), CultureInfo.InvariantCulture))
    End Function

    Friend Shared Function Text(row As DataRow, column As String) As String
        Return If(row.IsNull(column), String.Empty, Convert.ToString(row(column), CultureInfo.InvariantCulture))
    End Function

    Private Shared Function Active(row As DataRow, column As String) As Boolean
        Return Not row.IsNull(column) AndAlso Not Convert.ToBoolean(row(column), CultureInfo.InvariantCulture)
    End Function

    Private Shared Function Before2021(row As DataRow, column As String) As Boolean
        Return Not row.IsNull(column) AndAlso Convert.ToDateTime(row(column), CultureInfo.InvariantCulture).Year < 2021
    End Function
End Class
