Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>
''' Pure decisions of the print watch (slice 0099). No Win32 here -- <see cref="AdobePrintWatcher"/>
''' reads the print queue and <see cref="AdobeSaveTrap"/> reads the window titles, and both ask.
''' </summary>
Public NotInheritable Class AdobePrintJobFilter

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Titles of Adobe's own Print window, lower case and WITHOUT diacritics (the title read from
    ''' the window is folded the same way before the comparison): English, and the Romanian words an
    ''' Adobe or Windows translation may use. NOT MEASURED on a client PC -- the print queue, not
    ''' this list, decides what is counted; a title missing here only costs the faster checking.
    ''' </summary>
    Public Shared ReadOnly PrintDialogTitles As IReadOnlyList(Of String) =
        New String() {"print", "imprimare", "tiparire", "tipareste", "imprima"}

    ''' <summary>
    ''' True when <paramref name="title"/> is the title of a Print window: one of
    ''' <see cref="PrintDialogTitles"/>, alone or followed by a space / a colon / an ellipsis.
    ''' </summary>
    Public Shared Function IsPrintDialogTitle(title As String) As Boolean
        If String.IsNullOrWhiteSpace(title) Then Return False
        Dim t As String = Fold(title).Trim()
        For Each word As String In PrintDialogTitles
            If Not t.StartsWith(word, StringComparison.Ordinal) Then Continue For
            If t.Length = word.Length Then Return True
            Dim after As Char = t(word.Length)
            If after = " "c OrElse after = ":"c OrElse after = "."c Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' True when a print job named <paramref name="jobDocument"/> is a print of the file at
    ''' <paramref name="pdfPath"/>. Adobe names the job after the file («DDF_NR_12_REV_0_X.PDF»);
    ''' the ActiveX control has been seen to offer the whole path with the separators turned into
    ''' «_» (see <see cref="AdobeSaveDialogFilter.NameMatches"/>), so the file stem may sit anywhere
    ''' in the name -- but it must END there: the character after it is not a letter, a digit or
    ''' «_», so «ORD_NR_1_AB» never answers for «ORD_NR_1_ABC.PDF». Case-insensitive.
    ''' </summary>
    Public Shared Function JobMatches(jobDocument As String, pdfPath As String) As Boolean
        If String.IsNullOrWhiteSpace(jobDocument) OrElse String.IsNullOrWhiteSpace(pdfPath) Then Return False
        Dim stem As String = Path.GetFileNameWithoutExtension(pdfPath.Trim())
        If String.IsNullOrEmpty(stem) Then Return False
        Dim start As Integer = 0
        While start < jobDocument.Length
            Dim found As Integer = jobDocument.IndexOf(stem, start, StringComparison.OrdinalIgnoreCase)
            If found < 0 Then Return False
            Dim after As Integer = found + stem.Length
            If after >= jobDocument.Length Then Return True
            Dim c As Char = jobDocument(after)
            If Not Char.IsLetterOrDigit(c) AndAlso c <> "_"c Then Return True
            start = found + 1
        End While
        Return False
    End Function

    ''' <summary>
    ''' True when the job belongs to <paramref name="currentUser"/> (on a shared computer every
    ''' session sees every job). A job with no owner name is not refused.
    ''' </summary>
    Public Shared Function SameUser(jobUser As String, currentUser As String) As Boolean
        If String.IsNullOrWhiteSpace(jobUser) OrElse String.IsNullOrWhiteSpace(currentUser) Then Return True
        Return String.Equals(jobUser.Trim(), currentUser.Trim(), StringComparison.OrdinalIgnoreCase)
    End Function

    ' Lower case, diacritics removed.
    Private Shared Function Fold(text As String) As String
        Dim decomposed As String = text.Normalize(NormalizationForm.FormD)
        Dim sb As New StringBuilder(decomposed.Length)
        For Each c As Char In decomposed
            If CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark Then sb.Append(c)
        Next
        Return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant()
    End Function

End Class
