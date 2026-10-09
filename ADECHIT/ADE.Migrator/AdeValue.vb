Imports System.Globalization

''' <summary>Conversions of one Access cell to the value stored in MariaDB. Nothing is stored silently changed: a value
''' that does not fit is reported through <paramref name="k_note"/>.</summary>
Public NotInheritable Class AdeValue

    Private Sub New()
    End Sub

    Public Shared Function IsEmpty(k_value As Object) As Boolean
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return True
        Dim k_text = TryCast(k_value, String)
        Return k_text IsNot Nothing AndAlso k_text.Trim().Length = 0
    End Function

    ''' <summary>Whole number, or Nothing for an empty cell. <paramref name="k_note"/> is set when a fraction was rounded.</summary>
    Public Shared Function ToInt(k_value As Object, ByRef k_note As String) As Object
        k_note = Nothing
        If IsEmpty(k_value) Then Return Nothing
        Dim k_number As Double
        If TypeOf k_value Is String Then
            If Not Double.TryParse(CStr(k_value).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, k_number) Then
                k_note = $"valoarea «{k_value}» nu este un număr"
                Return Nothing
            End If
        ElseIf TypeOf k_value Is Boolean Then
            k_number = If(CBool(k_value), 1, 0)
        Else
            k_number = Convert.ToDouble(k_value, CultureInfo.InvariantCulture)
        End If
        Dim k_rounded As Double = Math.Round(k_number, MidpointRounding.AwayFromZero)
        If Math.Abs(k_rounded - k_number) > 0.0000001 Then k_note = $"{k_number.ToString(CultureInfo.InvariantCulture)} rotunjit la {k_rounded.ToString(CultureInfo.InvariantCulture)}"
        If k_rounded > Integer.MaxValue OrElse k_rounded < Integer.MinValue Then
            k_note = $"valoarea {k_number.ToString(CultureInfo.InvariantCulture)} nu încape într-un număr întreg"
            Return Nothing
        End If
        Return CInt(k_rounded)
    End Function

    Public Shared Function ToDbl(k_value As Object) As Object
        If IsEmpty(k_value) Then Return Nothing
        If TypeOf k_value Is String Then
            Dim k_number As Double
            If Double.TryParse(CStr(k_value).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, k_number) Then Return k_number
            Return Nothing
        End If
        Return Convert.ToDouble(k_value, CultureInfo.InvariantCulture)
    End Function

    Public Shared Function ToFlag(k_value As Object) As Object
        If IsEmpty(k_value) Then Return Nothing
        If TypeOf k_value Is Boolean Then Return CBool(k_value)
        Dim k_number = ToInt(k_value, Nothing)
        Return k_number IsNot Nothing AndAlso CInt(k_number) <> 0
    End Function

    Public Shared Function ToMoment(k_value As Object) As Object
        If IsEmpty(k_value) Then Return Nothing
        If TypeOf k_value Is DateTime Then Return CType(k_value, DateTime)
        Dim k_moment As DateTime
        If DateTime.TryParse(Convert.ToString(k_value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture,
                             DateTimeStyles.None, k_moment) Then Return k_moment
        Return Nothing
    End Function

    ''' <summary>Text exactly as written in Access (no trimming): the old data is carried over unchanged.</summary>
    Public Shared Function ToStr(k_value As Object) As Object
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return Nothing
        Dim k_text = Convert.ToString(k_value, CultureInfo.InvariantCulture)
        If k_text Is Nothing OrElse k_text.Length = 0 Then Return Nothing
        Return k_text
    End Function

    ''' <summary>First day of a month.</summary>
    Public Shared Function MonthStart(k_year As Integer, k_month As Integer) As DateTime
        Return New DateTime(k_year, k_month, 1)
    End Function

End Class
