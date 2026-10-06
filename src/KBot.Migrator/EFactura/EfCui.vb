Imports System.Text

''' <summary>
''' The one way a tax code is brought to a comparable form: letters and digits only, upper case, without the
''' «RO» that precedes the digits of a VAT payer. «RO 5888716», «ro5888716» and «5888716» all become «5888716».
''' </summary>
''' <remarks>
''' The Python server writes <c>EF_Primite.CuiNormalizat</c> by the SAME rule (00EF-03+), and the view of
''' <c>KbotForm</c> compares it with <c>FX_DDF_Parteneri.CodFiscal</c> normalised the same way. Change it in all
''' places or in none.
''' </remarks>
Public NotInheritable Class EfCui

    Private Sub New()
    End Sub

    Public Shared Function Normalize(k_value As String) As String
        If String.IsNullOrWhiteSpace(k_value) Then Return String.Empty
        Dim kept As New StringBuilder(k_value.Length)
        For Each ch In k_value
            If Char.IsLetterOrDigit(ch) Then kept.Append(Char.ToUpperInvariant(ch))
        Next
        Dim cleaned = kept.ToString()
        If cleaned.Length > 2 AndAlso cleaned.StartsWith("RO", StringComparison.Ordinal) AndAlso Char.IsDigit(cleaned(2)) Then
            cleaned = cleaned.Substring(2)
        End If
        Return cleaned
    End Function

End Class
