Option Strict On
Imports System.IO
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Xfa

''' <summary>
''' Slice 0088 -- where a correction note's PDF lives on this computer, the same two places as the
''' DDF / ORD documents:
''' <list type="bullet">
''' <item>the WORK copy, in <see cref="TempPdfStore"/> (wiped at start): the one K-BOT generates
''' and shows while it is unsigned;</item>
''' <item>the CACHE copy, <c>...\PDF\NC\NOTA_CAB_{nr}_{an}.PDF</c> next to the ORD root: the copy
''' of what the server holds once it is signed.</item>
''' </list>
''' </summary>
Public NotInheritable Class CabNoteFiles

    Private Sub New()
    End Sub

    Public Const DocType As String = "NC"

    ''' <summary>The generated (unsigned) copy.</summary>
    Public Shared Function WorkPath(note As CabCorrectionNote) As String
        If note Is Nothing Then Return Nothing
        Return TempPdfStore.PathFor(note.FileName)
    End Function

    ''' <summary>The signed copy's cache: the folder «NC» beside the ORD root.</summary>
    Public Shared Function CachePath(note As CabCorrectionNote) As String
        If note Is Nothing Then Return Nothing
        Dim ordRoot As String = If(KBotPaths.Current.OrdPdfRoot, KBotPaths.DefaultOrdPdfRoot).TrimEnd("\"c, "/"c)
        Dim parent As String = Path.GetDirectoryName(ordRoot)
        If String.IsNullOrEmpty(parent) Then parent = ordRoot
        Return Path.Combine(parent, "NC", note.FileName)
    End Function

    ''' <summary>
    ''' Slice 0088-04: the receipt's copy on this computer, <c>...\PDF\NC\Recipise\RECIPISA_{index}{ext}</c>
    ''' -- the extension of the file FOREXE served (a PDF so far), «.pdf» when it has none.
    ''' </summary>
    Public Shared Function ReceiptPath(note As CabCorrectionNote, index As String, fileName As String) As String
        If note Is Nothing OrElse String.IsNullOrWhiteSpace(index) Then Return Nothing
        Dim ext As String = Path.GetExtension(If(fileName, String.Empty))
        If String.IsNullOrEmpty(ext) OrElse ext.Length > 6 Then ext = ".pdf"
        Return Path.Combine(Path.GetDirectoryName(CachePath(note)), "Recipise", $"RECIPISA_{index.Trim()}{ext.ToLowerInvariant()}")
    End Function

    ''' <summary>Writes a receipt to <paramref name="target"/> (.part + move, as the PDF cache).</summary>
    Public Shared Sub WriteReceipt(target As String, content As Byte())
        Try
            If String.IsNullOrWhiteSpace(target) Then Throw New ArgumentException("No receipt path.", NameOf(target))
            If content Is Nothing OrElse content.Length = 0 Then Throw New ArgumentException("Empty receipt.", NameOf(content))
            Directory.CreateDirectory(Path.GetDirectoryName(target))
            File.WriteAllBytes(target & ".part", content)
            File.Move(target & ".part", target, overwrite:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteFiles.WriteReceipt", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Generates the note's PDF into its work copy and returns the path.</summary>
    Public Shared Function Generate(note As CabCorrectionNote) As String
        Try
            TempPdfStore.EnsureRoot()
            Dim target As String = WorkPath(note)
            CabNotePdf.Write(note, target)
            Return target
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteFiles.Generate", ex)
            Throw
        End Try
    End Function

    ''' <summary>Does the file carry at least one signature? False when it cannot be read.</summary>
    Public Shared Function IsSigned(path As String) As Boolean
        Try
            If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then Return False
            Return PdfSignatures.Read(SignedPdfFiles.ReadShared(path), DocType).IsSigned
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteFiles.IsSigned", ex)
            Return False
        End Try
    End Function

End Class
