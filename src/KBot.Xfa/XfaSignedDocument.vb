Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Xml
Imports iTextSharp.text.pdf
Imports iTextSharp.text.pdf.security
Imports KBot.Common

''' <summary>
''' The state of one signature inside a PDF (slice 0078-04). POCO.
''' </summary>
Public NotInheritable Class PdfSignatureCheck
    Public Property FieldName As String = String.Empty
    ''' <summary>The revision the signature closes (1-based).</summary>
    Public Property Revision As Integer
    Public Property TotalRevisions As Integer
    ''' <summary>False once anything was appended after the signature (normal for a second signer).</summary>
    Public Property CoversWholeDocument As Boolean
    ''' <summary>The signed bytes still hash to what the signature says (iText PdfPKCS7.Verify).</summary>
    Public Property IntegrityOk As Boolean
    ''' <summary>Why the check could not run ("" when it ran).</summary>
    Public Property ErrorText As String = String.Empty
End Class

''' <summary>
''' Slice 0078-04: work on a PDF that is ALREADY signed, without touching the signed bytes.
'''
''' The DDF of a new angajament (not based on a rezervare) is signed on section A before forexecab
''' gives the Cod angajament and the Indicator that section B needs. Section B then has to be filled
''' INTO the A-signed file -- an incremental update (iText append mode), never a regeneration from
''' the template, which would drop A's signature. Whether Adobe then still accepts A and lets B be
''' signed is what the signing bench measures; this class only does the file work and reports the
''' signatures' state.
''' </summary>
Public NotInheritable Class XfaSignedDocument

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The XFA form data (the root under <c>xfa:data</c>, e.g. <c>form1</c>) as XML text, or ""
    ''' when the PDF has no XFA data. Risky boundary (iText, XML): logs and rethrows.
    ''' </summary>
    Public Shared Function ReadFormData(pdfBytes As Byte()) As String
        Dim reader As PdfReader = Nothing
        Try
            If pdfBytes Is Nothing OrElse pdfBytes.Length = 0 Then Throw New ArgumentException("Empty PDF content.", NameOf(pdfBytes))
            reader = New PdfReader(pdfBytes)
            Dim xfa As New XfaForm(reader)
            If Not xfa.XfaPresent OrElse xfa.DatasetsNode Is Nothing Then Return ""
            For Each child As XmlNode In xfa.DatasetsNode.ChildNodes
                If child.NodeType <> XmlNodeType.Element OrElse child.LocalName <> "data" Then Continue For
                For Each root As XmlNode In child.ChildNodes
                    If root.NodeType = XmlNodeType.Element Then Return root.OuterXml
                Next
            Next
            Return ""
        Catch ex As Exception
            GlobalErrorLog.Write("XfaSignedDocument.ReadFormData", ex)
            Throw
        Finally
            Try
                reader?.Close()
            Catch ex As Exception
                GlobalErrorLog.Write("XfaSignedDocument.ReadFormData.Close", ex)
            End Try
        End Try
    End Function

    ' iText's «keep the PDF version» sentinel (same as AdobeUtils).
    Private Const KeepPdfVersion As Char = ChrW(0)

    ''' <summary>
    ''' Applies <paramref name="dataXmlPath"/> (same shape and rules as the generation XML) to
    ''' <paramref name="inputPdfPath"/> in APPEND mode and writes the result to
    ''' <paramref name="outputPdfPath"/> (must differ from the input). The bytes of the input -- and
    ''' so every signature in it -- stay as they are; the change is a new revision after them.
    '''
    ''' <para>ONLY the <c>datasets</c> packet is rewritten (slice 0078-04, second pass). iText's own
    ''' «XFA changed» path (<c>XfaForm.SetXfa</c>) rewrites the <c>template</c> packet too, re-serialised
    ''' from the DOM; after a signature that is a change of the FORM, not a fill -- what Adobe does when
    ''' the operator types into a field is a datasets-only update. The first pass used SetXfa and broke
    ''' the embedded NOTAFD.xml. Throws when the XFA is not split into packets (no datasets stream
    ''' to update on its own). Risky boundary: logs and rethrows.</para>
    '''
    ''' <para>Slice 0078-05: when the document carries <c>NOTAFD.xml</c>, its section B is brought in
    ''' line with the new form data (<see cref="DdfNotafdSync"/>) in the SAME revision -- only that
    ''' attachment's stream is replaced, as «Valideaza» would. Returns one Romanian line saying what
    ''' happened to the attachment.</para>
    ''' </summary>
    Public Shared Function FillIncremental(inputPdfPath As String, outputPdfPath As String, dataXmlPath As String) As String
        Try
            If String.Equals(Path.GetFullPath(inputPdfPath), Path.GetFullPath(outputPdfPath), StringComparison.OrdinalIgnoreCase) Then
                Throw New ArgumentException("The output must be a different file than the input.", NameOf(outputPdfPath))
            End If
            XfaLog.Init("DDF")
            XfaLog.LogSection("FillIncremental (datasets only)")

            Dim config As New XmlDocument()
            config.Load(dataXmlPath)

            Using reader As New PdfReader(inputPdfPath)
                Dim xfa As New XfaForm(reader)
                If Not xfa.XfaPresent OrElse xfa.DatasetsNode Is Nothing Then
                    Throw New InvalidOperationException("The PDF has no XFA datasets.")
                End If
                Dim datasetsRef As PRIndirectReference = FindDatasetsRef(reader)

                AdobeUtils.ProcessXmlNodes(config.DocumentElement, xfa.DomDocument, Nothing)
                Dim bytes As Byte() = XfaForm.SerializeDoc(xfa.DatasetsNode)

                ' NOTAFD.xml, from the data just changed (Nothing = attachment absent or unchanged).
                Dim note As String = "NOTAFD.xml: documentul nu are atașamentul."
                Dim notafdRef As PRIndirectReference = FindEmbeddedFileRef(reader, DdfNotafdSync.AttachmentName)
                Dim notafdBytes As Byte() = Nothing
                If notafdRef IsNot Nothing Then
                    Dim oldXml As String = Text.Encoding.UTF8.GetString(
                        PdfReader.GetStreamBytes(CType(PdfReader.GetPdfObject(notafdRef), PRStream)))
                    Dim newXml As String = DdfNotafdSync.SyncSectionB(xfa.DatasetsNode, oldXml, note)
                    If newXml IsNot Nothing Then notafdBytes = New Text.UTF8Encoding(False).GetBytes(newXml)
                End If

                Using output As New FileStream(outputPdfPath, FileMode.Create, FileAccess.Write)
                    Dim stamper As New PdfStamper(reader, output, KeepPdfVersion, True)
                    ' The stream is fetched AFTER the stamper: in append mode Close() writes each marked
                    ' object with its own IndRef, which the reader fills in only once the stamper made it
                    ' appendable. Fetched before (the previous version), IndRef was Nothing and Close()
                    ' threw NullReferenceException (28.09.2026). Set explicitly as well, to not depend on it.
                    Dim datasets As PRStream = CType(PdfReader.GetPdfObject(datasetsRef), PRStream)
                    If datasets.IndRef Is Nothing Then datasets.IndRef = datasetsRef
                    datasets.SetData(bytes)
                    ' Append mode writes only objects marked by their INDIRECT REFERENCE. Marking the stream
                    ' object itself (first version of this pass) marked nothing: the output carried only
                    ' iText's Info + XMP update and the old data.
                    stamper.MarkUsed(datasetsRef)
                    If notafdBytes IsNot Nothing Then
                        ' Same rules as datasets: fetched after the stamper, marked by reference.
                        Dim attached As PRStream = CType(PdfReader.GetPdfObject(notafdRef), PRStream)
                        If attached.IndRef Is Nothing Then attached.IndRef = notafdRef
                        SetEmbeddedFileData(attached, notafdBytes)
                        stamper.MarkUsed(notafdRef)
                    End If
                    stamper.Close()
                End Using
                VerifyDatasets(outputPdfPath, bytes)
                If notafdBytes IsNot Nothing Then VerifyEmbeddedFile(outputPdfPath, DdfNotafdSync.AttachmentName, notafdBytes)
                XfaLog.Log("INFO", $"FillIncremental: datasets rewritten ({bytes.Length} bytes, object {datasetsRef.Number}), template untouched; {note}")
                Return note
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("XfaSignedDocument.FillIncremental", ex)
            Throw
        End Try
    End Function

    ' The reference of the embedded file stream of attachment «fileName»: the EmbeddedFiles name
    ' tree first, then any Filespec in the file (the form attaches through importDataObject, whose
    ' filespec may sit in an object stream). Nothing when the document has no such attachment.
    Private Shared Function FindEmbeddedFileRef(reader As PdfReader, fileName As String) As PRIndirectReference
        Dim names As PdfDictionary = reader.Catalog.GetAsDict(PdfName.NAMES)
        Dim tree As PdfDictionary = If(names Is Nothing, Nothing, names.GetAsDict(PdfName.EMBEDDEDFILES))
        If tree IsNot Nothing Then
            For Each kv As KeyValuePair(Of String, PdfObject) In PdfNameTree.ReadTree(tree)
                Dim found As PRIndirectReference = EmbeddedStreamRef(TryCast(PdfReader.GetPdfObject(kv.Value), PdfDictionary), fileName)
                If found IsNot Nothing Then Return found
            Next
        End If
        For i As Integer = 1 To reader.XrefSize - 1
            Dim found As PRIndirectReference = EmbeddedStreamRef(TryCast(reader.GetPdfObject(i), PdfDictionary), fileName)
            If found IsNot Nothing Then Return found
        Next
        Return Nothing
    End Function

    Private Shared Function EmbeddedStreamRef(spec As PdfDictionary, fileName As String) As PRIndirectReference
        If spec Is Nothing OrElse Not PdfName.FILESPEC.Equals(spec.GetAsName(PdfName.TYPE)) Then Return Nothing
        Dim uf As PdfString = spec.GetAsString(PdfName.UF)
        Dim f As PdfString = spec.GetAsString(PdfName.F)
        Dim specName As String = If(uf IsNot Nothing, uf.ToUnicodeString(), If(f IsNot Nothing, f.ToUnicodeString(), ""))
        If Not String.Equals(specName, fileName, StringComparison.OrdinalIgnoreCase) Then Return Nothing
        Dim ef As PdfDictionary = spec.GetAsDict(PdfName.EF)
        If ef Is Nothing Then Return Nothing
        Dim ref As PRIndirectReference = TryCast(ef.Get(PdfName.F), PRIndirectReference)
        If ref Is Nothing OrElse Not TypeOf PdfReader.GetPdfObject(ref) Is PRStream Then Return Nothing
        Return ref
    End Function

    ' New content for an embedded file stream, with the parameters a reader checks: decoded length,
    ' size, MD5 checksum and modification date.
    Private Shared Sub SetEmbeddedFileData(stream As PRStream, data As Byte())
        stream.SetData(data)
        stream.Put(New PdfName("DL"), New PdfNumber(data.Length))
        Dim params As PdfDictionary = stream.GetAsDict(PdfName.PARAMS)
        If params Is Nothing Then
            params = New PdfDictionary()
            stream.Put(PdfName.PARAMS, params)
        End If
        params.Put(PdfName.SIZE, New PdfNumber(data.Length))
        Using md5 As System.Security.Cryptography.MD5 = System.Security.Cryptography.MD5.Create()
            params.Put(PdfName.CHECKSUM, New PdfString(md5.ComputeHash(data)).SetHexWriting(True))
        End Using
        params.Put(PdfName.MODDATE, New PdfDate())
    End Sub

    ' Reads the written file back: the attachment must hold exactly the new content.
    Private Shared Sub VerifyEmbeddedFile(pdfPath As String, fileName As String, expected As Byte())
        Using check As New PdfReader(ReadShared(pdfPath))
            Dim ref As PRIndirectReference = FindEmbeddedFileRef(check, fileName)
            If ref Is Nothing Then Throw New InvalidOperationException($"The written PDF lost the attachment {fileName}.")
            Dim actual As Byte() = PdfReader.GetStreamBytes(CType(PdfReader.GetPdfObject(ref), PRStream))
            If Not actual.AsSpan().SequenceEqual(expected) Then
                Throw New InvalidOperationException(
                    $"The written PDF does not carry the new {fileName} (object {ref.Number}: {actual.Length} bytes, expected {expected.Length}).")
            End If
        End Using
    End Sub

    Private Shared Function ReadShared(pdfPath As String) As Byte()
        Using fs As New FileStream(pdfPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Using ms As New MemoryStream()
                fs.CopyTo(ms)
                Return ms.ToArray()
            End Using
        End Using
    End Function

    ' The reference of the «datasets» stream in the AcroForm XFA array (name, stream, name, stream, ...).
    Private Shared Function FindDatasetsRef(reader As PdfReader) As PRIndirectReference
        Dim acroForm As PdfDictionary = reader.Catalog.GetAsDict(PdfName.ACROFORM)
        If acroForm Is Nothing Then Throw New InvalidOperationException("The PDF has no AcroForm.")
        Dim xfaObj As PdfObject = PdfReader.GetPdfObject(acroForm.Get(PdfName.XFA))
        Dim arr As PdfArray = TryCast(xfaObj, PdfArray)
        If arr Is Nothing Then
            Throw New InvalidOperationException("The XFA is a single stream, not packets: datasets cannot be updated alone.")
        End If
        For i As Integer = 0 To arr.Size - 2 Step 2
            Dim name As PdfString = arr.GetAsString(i)
            If name IsNot Nothing AndAlso name.ToUnicodeString() = "datasets" Then
                Dim ref As PRIndirectReference = TryCast(arr.GetPdfObject(i + 1), PRIndirectReference)
                If ref IsNot Nothing AndAlso TypeOf PdfReader.GetPdfObject(ref) Is PRStream Then Return ref
            End If
        Next
        Throw New InvalidOperationException("The XFA has no «datasets» packet.")
    End Function

    ' Reads the written file back: its datasets must be exactly the ones just written. The first
    ' version of this pass «succeeded» while writing nothing -- never again silently.
    ' Read SHARED: PdfReader(path) opens with FileShare.Read and fails while Adobe holds the file
    ' (seen 28.09.2026 on a reused name still open in the Acrobat broker).
    Private Shared Sub VerifyDatasets(pdfPath As String, expected As Byte())
        Dim written As Byte()
        Using fs As New FileStream(pdfPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Using ms As New MemoryStream()
                fs.CopyTo(ms)
                written = ms.ToArray()
            End Using
        End Using
        Using check As New PdfReader(written)
            Dim ref As PRIndirectReference = FindDatasetsRef(check)
            Dim actual As Byte() = PdfReader.GetStreamBytes(CType(PdfReader.GetPdfObject(ref), PRStream))
            If Not actual.AsSpan().SequenceEqual(expected) Then
                Throw New InvalidOperationException(
                    $"The written PDF does not carry the new datasets (object {ref.Number}: {actual.Length} bytes, expected {expected.Length}).")
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Every signature of <paramref name="pdfBytes"/>: its revision, whether it covers the whole
    ''' file and whether its bytes are intact. A signature that cannot be checked is returned with
    ''' <see cref="PdfSignatureCheck.ErrorText"/>. Risky boundary: logs and rethrows when the PDF
    ''' itself cannot be read.
    ''' </summary>
    Public Shared Function CheckSignatures(pdfBytes As Byte()) As List(Of PdfSignatureCheck)
        Dim reader As PdfReader = Nothing
        Try
            If pdfBytes Is Nothing OrElse pdfBytes.Length = 0 Then Throw New ArgumentException("Empty PDF content.", NameOf(pdfBytes))
            reader = New PdfReader(pdfBytes)
            Dim fields As AcroFields = reader.AcroFields
            Dim result As New List(Of PdfSignatureCheck)()
            Dim names As List(Of String) = fields.GetSignatureNames()
            If names Is Nothing Then Return result
            For Each nm As String In names
                Dim check As New PdfSignatureCheck With {
                    .FieldName = nm, .Revision = fields.GetRevision(nm), .TotalRevisions = fields.TotalRevisions}
                Try
                    check.CoversWholeDocument = fields.SignatureCoversWholeDocument(nm)
                    Dim pk As PdfPKCS7 = fields.VerifySignature(nm)
                    check.IntegrityOk = pk IsNot Nothing AndAlso pk.Verify()
                Catch ex As Exception
                    GlobalErrorLog.Write("XfaSignedDocument.CheckSignatures.Field", ex)
                    check.ErrorText = ex.Message
                End Try
                result.Add(check)
            Next
            Return result
        Catch ex As Exception
            GlobalErrorLog.Write("XfaSignedDocument.CheckSignatures", ex)
            Throw
        Finally
            Try
                reader?.Close()
            Catch ex As Exception
                GlobalErrorLog.Write("XfaSignedDocument.CheckSignatures.Close", ex)
            End Try
        End Try
    End Function

End Class
