Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports KBot.Api
Imports KBot.Xfa

''' <summary>
''' Which signatures an upload reports to the server's signature log (slice 0079).
'''
''' ONLY THE ADDED ONES: a field already signed when the file was opened was signed by somebody
''' else, earlier -- reporting it again would put THIS operator, computer and address next to a
''' signature they did not make. «Added» = signed now, absent from the baseline key the signing
''' session read when it started.
''' </summary>
Public NotInheritable Class SignatureRecords

    Private Sub New()
    End Sub

    ''' <summary>The records of the fields signed in <paramref name="info"/> and not in <paramref name="baselineKey"/>.</summary>
    Public Shared Function Added(info As PdfSignatureInfo, baselineKey As String) As List(Of PdfSignatureRecord)
        Dim before As New HashSet(Of String)(
            If(baselineKey, String.Empty).Split("|"c, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal)
        Dim result As New List(Of PdfSignatureRecord)()
        If info Is Nothing Then Return result
        For Each d As PdfSignatureDetail In info.Details
            If before.Contains(d.FieldName) Then Continue For
            result.Add(New PdfSignatureRecord With {
                .camp = d.FieldName,
                .rol = If(d.Role, String.Empty),
                .semnatar = If(d.Signer, String.Empty),
                .data = If(d.SignedAt.HasValue, d.SignedAt.Value.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture), String.Empty)})
        Next
        Return result
    End Function

    ''' <summary>
    ''' <paramref name="first"/> plus the records of <paramref name="second"/> it does not already have
    ''' (same field, same time) -- a kept copy's records must not be lost when a newer signature of
    ''' the same document goes up first.
    ''' </summary>
    Public Shared Function Merge(first As IEnumerable(Of PdfSignatureRecord),
                                 second As IEnumerable(Of PdfSignatureRecord)) As List(Of PdfSignatureRecord)
        Dim result As New List(Of PdfSignatureRecord)(If(first, Enumerable.Empty(Of PdfSignatureRecord)()))
        For Each r As PdfSignatureRecord In If(second, Enumerable.Empty(Of PdfSignatureRecord)())
            If Not result.Any(Function(x) x.camp = r.camp AndAlso x.data = r.data) Then result.Add(r)
        Next
        Return result
    End Function

End Class
