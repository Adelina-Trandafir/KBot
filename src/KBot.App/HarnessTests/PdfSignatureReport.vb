#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports KBot.Common
Imports KBot.Xfa

''' <summary>
''' The signature lines both signing benches write (slices 0078 / 0078-04): signed fields, roles,
''' signer + time, and for each signature whether its bytes are intact and whether anything was
''' appended after it. Never throws: a PDF that cannot be read becomes one line saying so.
''' </summary>
Friend NotInheritable Class PdfSignatureReport

    Private Sub New()
    End Sub

    Public Shared Function Lines(label As String, bytes As Byte(), docType As String) As List(Of String)
        Dim result As New List(Of String)()
        Try
            Dim sha As String = ShortSha(PdfHash.Compute(bytes))
            Dim info As PdfSignatureInfo = PdfSignatures.Read(bytes, docType)
            If Not info.IsSigned Then
                result.Add($"{label}: nicio semnătură ({bytes.Length:N0} octeți, sha {sha}).")
                Return result
            End If
            result.Add($"{label}: {info.FieldNames.Count} câmpuri semnate [{String.Join(", ", info.FieldNames)}] " &
                       $"-> Semnatura «{info.ToSemnatura()}», {bytes.Length:N0} octeți, sha {sha}.")
            If info.Unclassified.Count > 0 Then
                result.Add($"{label}: câmpuri FĂRĂ rol recunoscut: {String.Join(", ", info.Unclassified)}.")
            End If
            ' Slice 0079: what the signature log receives for each field.
            For Each d As PdfSignatureDetail In info.Details
                result.Add($"{label}:   {d.FieldName} — rol «{d.Role}», semnatar «{d.Signer}», " &
                           If(d.SignedAt.HasValue, $"semnat la {d.SignedAt.Value:yyyy-MM-dd HH:mm:ss zzz}", "fără dată"))
            Next
            ' Slice 0078-04: is each signature still intact, and does anything come after it?
            For Each c As PdfSignatureCheck In XfaSignedDocument.CheckSignatures(bytes)
                If c.ErrorText.Length > 0 Then
                    result.Add($"{label}:   {c.FieldName} — verificare imposibilă: {c.ErrorText}")
                Else
                    result.Add($"{label}:   {c.FieldName} — revizia {c.Revision}/{c.TotalRevisions}, integritate " &
                               If(c.IntegrityOk, "OK", "RUPTĂ") & ", acoperă tot documentul: " &
                               If(c.CoversWholeDocument, "da", "nu (s-a adăugat ceva după ea)"))
                End If
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSignatureReport.Lines", ex)
            result.Add($"{label}: semnăturile nu au putut fi citite: {ex.Message}")
        End Try
        Return result
    End Function

    Public Shared Function ShortSha(sha As String) As String
        If String.IsNullOrEmpty(sha) Then Return "—"
        Return If(sha.Length > 12, sha.Substring(0, 12), sha)
    End Function

End Class
#End If
