Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports iTextSharp.text.pdf
Imports iTextSharp.text.pdf.security
Imports KBot.Common

''' <summary>
''' One signed field as the PDF describes it (slice 0079): who signed it and when. POCO.
''' </summary>
Public NotInheritable Class PdfSignatureDetail
    Public Property FieldName As String = String.Empty
    ''' <summary>DDF: A / B / Ordonator; ORD: AB / CD / Ordonator; "" when no rule maps the field.</summary>
    Public Property Role As String = String.Empty
    ''' <summary>Signer name from the signature (the certificate's CN when the name is absent).</summary>
    Public Property Signer As String = String.Empty
    ''' <summary>The timestamp authority's time when there is one, else the signer's own clock.</summary>
    Public Property SignedAt As DateTimeOffset?
End Class

''' <summary>
''' What a PDF says about its signatures (slice 0078): the signed field names and the signer roles
''' they map to. Immutable.
''' </summary>
Public NotInheritable Class PdfSignatureInfo

    ''' <summary>Names of the signature fields that HOLD a signature, sorted (ordinal).</summary>
    Public ReadOnly Property FieldNames As IReadOnlyList(Of String)
    ''' <summary>Combination of <see cref="AdobeUtils.SIGNER_AB"/> / CD / ORDONATOR.</summary>
    Public ReadOnly Property Mask As Integer
    ''' <summary>"DDF" or "ORD".</summary>
    Public ReadOnly Property DocType As String
    ''' <summary>Signed fields no rule could map to a role (logged; they do not reach Semnatura).</summary>
    Public ReadOnly Property Unclassified As IReadOnlyList(Of String)
    ''' <summary>Slice 0079: signer + time of every signed field (same order as <see cref="FieldNames"/>).</summary>
    Public ReadOnly Property Details As IReadOnlyList(Of PdfSignatureDetail)

    Public Sub New(fieldNames As IEnumerable(Of String), mask As Integer, docType As String,
                   unclassified As IEnumerable(Of String),
                   Optional details As IEnumerable(Of PdfSignatureDetail) = Nothing)
        Me.FieldNames = If(fieldNames, Enumerable.Empty(Of String)()).
            OrderBy(Function(s) s, StringComparer.Ordinal).ToList()
        Me.Mask = mask
        Me.DocType = If(docType, "").ToUpperInvariant()
        Me.Unclassified = If(unclassified, Enumerable.Empty(Of String)()).ToList()
        Me.Details = If(details, Enumerable.Empty(Of PdfSignatureDetail)()).
            OrderBy(Function(d) d.FieldName, StringComparer.Ordinal).ToList()
    End Sub

    ''' <summary>The role name of one signer bit for a document type ("" for 0).</summary>
    Public Shared Function RoleName(bit As Integer, docType As String) As String
        Dim ddf As Boolean = String.Equals(docType, "DDF", StringComparison.OrdinalIgnoreCase)
        If (bit And AdobeUtils.SIGNER_AB) <> 0 Then Return If(ddf, "A", "AB")
        If (bit And AdobeUtils.SIGNER_CD) <> 0 Then Return If(ddf, "B", "CD")
        If (bit And AdobeUtils.SIGNER_ORDONATOR) <> 0 Then Return "Ordonator"
        Return String.Empty
    End Function

    Public ReadOnly Property IsSigned As Boolean
        Get
            Return FieldNames.Count > 0
        End Get
    End Property

    ''' <summary>
    ''' Role names in canonical order -- DDF: A, B, Ordonator; ORD: AB, CD, Ordonator. The same
    ''' names Access wrote into <c>Semnatura</c> (mdl_FX_Helpers.NumeSemnatar).
    ''' </summary>
    Public Function RoleNames() As IReadOnlyList(Of String)
        Dim ddf As Boolean = DocType = "DDF"
        Dim roles As New List(Of String)()
        If (Mask And AdobeUtils.SIGNER_AB) <> 0 Then roles.Add(If(ddf, "A", "AB"))
        If (Mask And AdobeUtils.SIGNER_CD) <> 0 Then roles.Add(If(ddf, "B", "CD"))
        If (Mask And AdobeUtils.SIGNER_ORDONATOR) <> 0 Then roles.Add("Ordonator")
        Return roles
    End Function

    ''' <summary>The value for <c>FX_DDF_REV.Semnatura</c> / <c>FX_ORD.Semnatura</c>: roles joined by ",".</summary>
    Public Function ToSemnatura() As String
        Return String.Join(",", RoleNames())
    End Function

    ''' <summary>A stable key of the signed field set -- equal keys = nothing new was signed.</summary>
    Public Function FieldKey() As String
        Return String.Join("|", FieldNames)
    End Function

End Class

''' <summary>
''' Reads the signatures of a PDF from its BYTES (slice 0078). Bytes, not a path: the file on screen
''' is held open by Adobe, so the caller copies it with FileShare.ReadWrite and hands the copy here.
''' Uses the same field -> role rules as the legacy SIGN flow (<see cref="AdobeUtils"/>).
''' </summary>
Public NotInheritable Class PdfSignatures

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Signed field names + role mask of <paramref name="pdfBytes"/>. Throws when the bytes are not
    ''' a readable PDF (a real error, NOT «unsigned» -- e.g. a file caught mid-write).
    ''' </summary>
    Public Shared Function Read(pdfBytes As Byte(), docType As String) As PdfSignatureInfo
        Dim reader As PdfReader = Nothing
        Try
            If pdfBytes Is Nothing OrElse pdfBytes.Length = 0 Then
                Throw New ArgumentException("Empty PDF content.", NameOf(pdfBytes))
            End If
            reader = New PdfReader(pdfBytes)
            Dim names As List(Of String) = reader.AcroFields.GetSignatureNames()
            If names Is Nothing Then names = New List(Of String)()
            Dim mask As Integer = 0
            Dim unclassified As New List(Of String)()
            Dim details As New List(Of PdfSignatureDetail)()
            For Each nm As String In names
                Dim bit As Integer = AdobeUtils.ClassifySigner(nm, docType)
                If bit = 0 Then unclassified.Add(nm)
                mask = mask Or bit
                details.Add(ReadDetail(reader, nm, PdfSignatureInfo.RoleName(bit, docType)))
            Next
            XfaLog.Log("INFO", $"PdfSignatures.Read ({docType}) -- signed fields: {names.Count} " &
                               $"[{String.Join(", ", names)}], mask {mask}" &
                               If(unclassified.Count > 0, $", UNCLASSIFIED [{String.Join(", ", unclassified)}]", ""))
            Return New PdfSignatureInfo(names, mask, docType, unclassified, details)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSignatures.Read", ex)
            Throw
        Finally
            Try
                reader?.Close()
            Catch ex As Exception
                GlobalErrorLog.Write("PdfSignatures.Read.Close", ex)
            End Try
        End Try
    End Function

    ' Signer + time of one field. A signature iText cannot parse still counts as signed (the field
    ' name is known) -- it just carries no name / time, and the failure is logged.
    Private Shared Function ReadDetail(reader As PdfReader, fieldName As String, role As String) As PdfSignatureDetail
        Dim detail As New PdfSignatureDetail With {.FieldName = fieldName, .Role = role}
        Try
            Dim pk As PdfPKCS7 = reader.AcroFields.VerifySignature(fieldName)
            If pk Is Nothing Then Return detail
            Dim name As String = pk.SignName
            If String.IsNullOrWhiteSpace(name) AndAlso pk.SigningCertificate IsNot Nothing Then
                name = CertificateInfo.GetSubjectFields(pk.SigningCertificate)?.GetField("CN")
            End If
            detail.Signer = If(name, String.Empty).Trim()
            Dim at As DateTime = If(pk.TimeStampDate <> DateTime.MaxValue, pk.TimeStampDate, pk.SignDate)
            If at <> DateTime.MinValue AndAlso at <> DateTime.MaxValue Then detail.SignedAt = New DateTimeOffset(at)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSignatures.ReadDetail", ex)
        End Try
        Return detail
    End Function

End Class
