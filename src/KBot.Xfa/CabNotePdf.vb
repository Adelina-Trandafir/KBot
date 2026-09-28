Option Strict On
Imports System.IO
Imports System.Text
Imports System.Xml
Imports iTextSharp.text.pdf
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' Slice 0088 -- makes the PDF of a «Nota contabila corectie CAB» (MF form F1135).
'''
''' <para><b>The template</b> is <c>Templates\F1135.pdf</c>, embedded in this assembly: the FIRST
''' revision of a real note Adobe saved («NOTA CAB 23.pdf», bytes 0..97098), i.e. the form exactly
''' as the ministry published it -- no data, no signature, with its Reader usage rights (UR3).
''' It is not downloaded: there is no public address K-BOT could take it from (the DDF / ORD
''' templates come from the MF guides page; F1135 is not on it).</para>
'''
''' <para><b>What it does is what Adobe did</b> on «VALIDARE SI GENERARE XML» in that note's second
''' revision: the XFA data (<see cref="CabCorrectionNoteRules.FormDataXml"/>) and the attachment
''' <c>f1135.xml</c> (<see cref="CabCorrectionNoteRules.ExportXml"/>, described «text/xml»), both
''' in APPEND mode, so the template's bytes and its usage rights stay untouched and Adobe Reader
''' still lets the operator sign.</para>
''' </summary>
Public NotInheritable Class CabNotePdf

    Private Sub New()
    End Sub

    ''' <summary>The resource name of the template (LogicalName in KBot.Xfa.vbproj).</summary>
    Public Const TemplateResource As String = "KBot.Xfa.Templates.F1135.pdf"

    ''' <summary>The attachment FOREXE reads.</summary>
    Public Const AttachmentName As String = "f1135.xml"

    ' iTextSharp: NUL = keep the template's PDF version (same sentinel as AdobeUtils).
    Private Const KeepPdfVersion As Char = ChrW(0)

    ''' <summary>The blank F1135 template.</summary>
    Public Shared Function TemplateBytes() As Byte()
        Try
            Using s As Stream = GetType(CabNotePdf).Assembly.GetManifestResourceStream(TemplateResource)
                If s Is Nothing Then Throw New InvalidOperationException($"The F1135 template resource «{TemplateResource}» is missing from KBot.Xfa.")
                Using ms As New MemoryStream()
                    s.CopyTo(ms)
                    Return ms.ToArray()
                End Using
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("CabNotePdf.TemplateBytes", ex)
            Throw
        End Try
    End Function

    ''' <summary>The note's PDF, unsigned. Throws on any failure (logged).</summary>
    Public Shared Function Build(note As CabCorrectionNote) As Byte()
        Try
            ArgumentNullException.ThrowIfNull(note)
            Dim formData As New XmlDocument()
            formData.LoadXml(CabCorrectionNoteRules.FormDataXml(note))
            Dim attachment As Byte() = New UTF8Encoding(False).GetBytes(CabCorrectionNoteRules.ExportXml(note))

            Dim reader As New PdfReader(TemplateBytes())
            Try
                Using output As New MemoryStream()
                    Dim stamper As New PdfStamper(reader, output, KeepPdfVersion, True)
                    Dim xfa As XfaForm = stamper.AcroFields.Xfa
                    If Not xfa.XfaPresent Then Throw New InvalidOperationException("The F1135 template has no XFA form.")
                    xfa.FillXfaForm(formData.DocumentElement)

                    Dim spec As PdfFileSpecification = PdfFileSpecification.FileEmbedded(
                        stamper.Writer, Nothing, AttachmentName, attachment, "text/plain", Nothing,
                        PdfStream.BEST_COMPRESSION)
                    stamper.AddFileAttachment("text/xml", spec)
                    stamper.Close()
                    Return output.ToArray()
                End Using
            Finally
                reader.Close()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("CabNotePdf.Build", ex)
            Throw
        End Try
    End Function

    ''' <summary>Writes the note's PDF to <paramref name="path"/> (the folder is created).</summary>
    Public Shared Sub Write(note As CabCorrectionNote, path As String)
        Try
            Dim bytes As Byte() = Build(note)
            Dim dir As String = IO.Path.GetDirectoryName(path)
            If Not String.IsNullOrEmpty(dir) Then Directory.CreateDirectory(dir)
            ' .part + move: a half-written file must never look like a valid note.
            File.WriteAllBytes(path & ".part", bytes)
            File.Move(path & ".part", path, overwrite:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("CabNotePdf.Write", ex)
            Throw
        End Try
    End Sub

End Class
