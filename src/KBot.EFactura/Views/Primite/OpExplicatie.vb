Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text

''' <summary>
''' Slice 01EF-07 -- the text of an order of payment («Explicație O.P.», «Fel factură»). ANAF refuses an order of payment whose text holds a diacritic, and the bar code
''' of the order reads a comma as «next field»: so the text is ASCII only and has NO comma anywhere, whatever the operator typed or pasted. The official limit of the
''' explanation is <see cref="MaxLength"/> characters; an empty one is refused (<see cref="MinLength"/>).
''' </summary>
Friend NotInheritable Class OpExplicatie

    ''' <summary>The shortest explanation an order of payment accepts.</summary>
    Public Const MinLength As Integer = 1

    ''' <summary>The official limit of the explanation of an order of payment.</summary>
    Public Const MaxLength As Integer = 75

    ' Typographic characters that have a plain ASCII twin (the rest of the non-ASCII characters is dropped).
    Private Shared ReadOnly _twins As New Dictionary(Of Char, Char) From {
        {ChrW(&H2013), "-"c}, {ChrW(&H2014), "-"c}, {ChrW(&H2018), "'"c}, {ChrW(&H2019), "'"c}, {ChrW(&H201A), "'"c},
        {ChrW(&H201C), """"c}, {ChrW(&H201D), """"c}, {ChrW(&H201E), """"c}, {ChrW(&HAB), """"c}, {ChrW(&HBB), """"c}, {ChrW(&HA0), " "c}}

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The text as the operator types it: diacritics become their plain letter (ă â î ș ț ş ţ …), a comma becomes a space, anything else that is not ASCII is dropped.
    ''' Spaces are left alone (the caret must not jump while typing); <see cref="Fit"/> tidies them.
    ''' </summary>
    Public Shared Function Clean(k_text As String) As String
        If String.IsNullOrEmpty(k_text) Then Return String.Empty
        Dim k_result As New StringBuilder(k_text.Length)
        For Each k_char As Char In k_text.Normalize(NormalizationForm.FormD)
            If CharUnicodeInfo.GetUnicodeCategory(k_char) = UnicodeCategory.NonSpacingMark Then Continue For
            Dim k_twin As Char
            If _twins.TryGetValue(k_char, k_twin) Then
                k_result.Append(k_twin)
            ElseIf k_char = ","c OrElse k_char = ChrW(9) OrElse k_char = ChrW(10) OrElse k_char = ChrW(13) Then
                k_result.Append(" "c)
            ElseIf AscW(k_char) >= 32 AndAlso AscW(k_char) < 127 Then
                k_result.Append(k_char)
            End If
        Next
        Return k_result.ToString()
    End Function

    ''' <summary>The text as it is written on the order: cleaned, single spaces, no space at the ends, at most <see cref="MaxLength"/> characters.</summary>
    Public Shared Function Fit(k_text As String) As String
        Dim k_tidy As String = Tidy(k_text)
        Return If(k_tidy.Length > MaxLength, k_tidy.Substring(0, MaxLength).TrimEnd(), k_tidy)
    End Function

    ' Cleaned, single spaces, no space at the ends; the length is left as it is.
    Private Shared Function Tidy(k_text As String) As String
        Dim k_clean As String = Clean(k_text)
        Dim k_single As New StringBuilder(k_clean.Length)
        Dim k_space As Boolean
        For Each k_char As Char In k_clean.Trim()
            If k_char = " "c Then
                If Not k_space Then k_single.Append(k_char)
                k_space = True
            Else
                k_single.Append(k_char)
                k_space = False
            End If
        Next
        Return k_single.ToString()
    End Function

    ''' <summary>
    ''' The explanation the order starts with (Access: «F &lt;numbers of the invoices paid&gt; C &lt;client code&gt; &lt;kind of invoice&gt;»); the parts that are empty are left out.
    ''' <paramref name="k_cut"/> is True when the text had to be shortened to <see cref="MaxLength"/>.
    ''' </summary>
    Public Shared Function Compose(k_numbers As IEnumerable(Of String), k_clientCode As String, k_kind As String, ByRef k_cut As Boolean) As String
        Dim k_parts As New List(Of String)()
        Dim k_list As List(Of String) = k_numbers.Where(Function(k_n) Not String.IsNullOrWhiteSpace(k_n)).ToList()
        If k_list.Count > 0 Then k_parts.Add("F " & String.Join(" ", k_list))
        If Not String.IsNullOrWhiteSpace(k_clientCode) Then k_parts.Add("C " & k_clientCode.Trim())
        If Not String.IsNullOrWhiteSpace(k_kind) Then k_parts.Add(k_kind.Trim())
        Dim k_whole As String = String.Join(" ", k_parts)
        k_cut = Tidy(k_whole).Length > MaxLength
        Return Fit(k_whole)
    End Function

End Class
