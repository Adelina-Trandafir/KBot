Imports System.Globalization
Imports System.Text

''' <summary>
''' Value conversions of the transfer (SLICE-VC05-03). Pure functions: no database, no file. A value with more than four decimals
''' is ROUNDED to four (decided by the operator, 18.10.2026: Access Double fields do not round correctly and show noise such as 0.1E+6);
''' a value that does not fit DECIMAL(20,4) is REFUSED.
''' </summary>
Friend NotInheritable Class VrcConvert

    Private Const IntMax As Decimal = 2147483647D
    Private Const IntMin As Decimal = -2147483648D
    ''' <summary>Access stores Double: 8497.0226 can come back as 8497.022600000001. Anything closer to a whole unit than this is whole.</summary>
    Private Const Noise As Decimal = 0.0000001D

    Private Sub New()
    End Sub

    ''' <summary>The value as an exact Decimal; Nothing for NULL/empty. Double is read through its 15-digit text form so float noise does not leak in.</summary>
    Friend Shared Function ToDecimal(k_value As Object) As Decimal?
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return Nothing
        If TypeOf k_value Is Decimal Then Return CType(k_value, Decimal)
        If TypeOf k_value Is Double Then
            Return Decimal.Parse(CType(k_value, Double).ToString("G15", CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture)
        End If
        If TypeOf k_value Is Single Then
            Return Decimal.Parse(CType(k_value, Single).ToString("G7", CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture)
        End If
        If TypeOf k_value Is Boolean Then Return If(CType(k_value, Boolean), -1D, 0D)
        Return Convert.ToDecimal(k_value, CultureInfo.InvariantCulture)
    End Function

    ''' <summary>The value for DECIMAL(20,4): rounded to four decimals (away from zero). False only when it does not fit the column.</summary>
    Friend Shared Function StoredDecimal(k_value As Decimal, ByRef k_result As Decimal, ByRef k_reason As String) As Boolean
        Dim k_rounded As Decimal = Math.Round(k_value, 4, MidpointRounding.AwayFromZero)
        If Math.Abs(k_rounded) >= 10000000000000000D Then
            k_reason = "nu încape în DECIMAL(20,4)"
            Return False
        End If
        k_result = k_rounded
        Return True
    End Function

    ''' <summary>A whole number that fits INT (counters, durations, keys). Fractions are refused.</summary>
    Friend Shared Function WholeInt(k_value As Decimal, ByRef k_result As Integer, ByRef k_reason As String) As Boolean
        Dim k_rounded As Decimal = Math.Round(k_value, 0, MidpointRounding.AwayFromZero)
        If Math.Abs(k_value - k_rounded) > Noise Then
            k_reason = "nu este număr întreg"
            Return False
        End If
        If k_rounded > IntMax OrElse k_rounded < IntMin Then
            k_reason = "nu încape în INT"
            Return False
        End If
        k_result = CInt(k_rounded)
        Return True
    End Function

    ''' <summary>Access Yes/No (-1/0, True/False) to 1/0; Nothing stays NULL.</summary>
    Friend Shared Function ToBit(k_value As Object) As Integer?
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return Nothing
        If TypeOf k_value Is Boolean Then Return If(CType(k_value, Boolean), 1, 0)
        Dim k_number As Decimal? = ToDecimal(k_value)
        If Not k_number.HasValue Then Return Nothing
        Return If(k_number.Value <> 0D, 1, 0)
    End Function

    ''' <summary>Tax code without spaces, dots, dashes and the RO prefix, upper case; empty when nothing is left.</summary>
    Friend Shared Function NormalizeTaxCode(k_text As String) As String
        If k_text Is Nothing Then Return String.Empty
        Dim k_sb As New StringBuilder()
        For Each k_c In k_text.Trim().ToUpperInvariant()
            If Char.IsLetterOrDigit(k_c) Then k_sb.Append(k_c)
        Next
        Dim k_result As String = k_sb.ToString()
        If k_result.StartsWith("RO", StringComparison.Ordinal) AndAlso k_result.Length > 2 AndAlso Char.IsDigit(k_result(2)) Then k_result = k_result.Substring(2)
        Return k_result
    End Function

    ''' <summary>True when the text contains «stoc» ignoring case and diacritics (an opening-balance document).</summary>
    Friend Shared Function IsOpeningStock(k_text As String) As Boolean
        If k_text Is Nothing OrElse k_text.Length = 0 Then Return False
        Dim k_sb As New StringBuilder()
        For Each k_c In k_text.Normalize(NormalizationForm.FormD)
            If CharUnicodeInfo.GetUnicodeCategory(k_c) <> UnicodeCategory.NonSpacingMark Then k_sb.Append(Char.ToLowerInvariant(k_c))
        Next
        Return k_sb.ToString().Contains("stoc")
    End Function

    ''' <summary>Largest whole number written in the text (digits only); Nothing when it has none. Used for the last real document number.</summary>
    Friend Shared Function LeadingNumber(k_text As String) As Long?
        If k_text Is Nothing Then Return Nothing
        Dim k_digits As New StringBuilder()
        For Each k_c In k_text.Trim()
            If Char.IsDigit(k_c) Then k_digits.Append(k_c) Else Exit For
        Next
        Dim k_number As Long
        If k_digits.Length > 0 AndAlso k_digits.Length <= 9 AndAlso Long.TryParse(k_digits.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, k_number) Then Return k_number
        Return Nothing
    End Function

    ''' <summary>Canonical text of a signed source key (stable, culture independent).</summary>
    Friend Shared Function KeyText(k_value As Object) As String
        Dim k_number As Decimal? = ToDecimal(k_value)
        If Not k_number.HasValue Then Return String.Empty
        Return Math.Round(k_number.Value, 0, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture)
    End Function

End Class
