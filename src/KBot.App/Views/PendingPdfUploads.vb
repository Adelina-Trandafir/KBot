Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common

''' <summary>Which family a signed PDF belongs to (slice 0078).</summary>
Public Enum PdfDocKind
    Ddf = 0
    Ord = 1
End Enum

''' <summary>
''' One signed PDF that has not reached the server yet (slice 0078). The PDF lives next to its
''' <c>.json</c> in <see cref="PendingPdfUploads.Root"/>. POCO -&gt; no Try/Catch.
''' </summary>
Public NotInheritable Class PendingPdfUpload
    Public Property Kind As PdfDocKind
    ''' <summary>IDREV (DDF) or IDORDP (ORD).</summary>
    Public Property Id As Integer
    ''' <summary>The server sha the signing started from ("-" = no row on the server).</summary>
    Public Property ShaPrecedent As String = ApiClient.ShaFaraRand
    ''' <summary>Signer roles, e.g. "A,Ordonator".</summary>
    Public Property Semnatura As String = String.Empty
    ''' <summary>Where the signed copy must land in the local cache once uploaded.</summary>
    Public Property CachePath As String = String.Empty
    Public Property Created As DateTime
    ''' <summary>
    ''' The server refused it with 409 (somebody else signed first). Never retried automatically:
    ''' the operator decides, the file stays here until then.
    ''' </summary>
    Public Property Conflict As Boolean
    ''' <summary>
    ''' Slice 0079: the signatures this copy ADDED, for the server's signature log. Kept here
    ''' because the retry happens later, when nobody remembers what the file looked like before.
    ''' </summary>
    Public Property Semnaturi As List(Of PdfSignatureRecord) = New List(Of PdfSignatureRecord)()
    ''' <summary>Full path of the PDF copy (not serialised -- derived from the folder).</summary>
    <Text.Json.Serialization.JsonIgnore>
    Public Property PdfPath As String = String.Empty
End Class

''' <summary>
''' The local safety net for signed PDFs whose upload failed (slice 0078):
''' <c>&lt;AppDir&gt;\PdfDeIncarcat\&lt;kind&gt;_&lt;id&gt;.pdf</c> + <c>.json</c>.
'''
''' WHY IT MUST EXIST: the server copy is the single source of truth, so opening a document whose
''' local file differs from the server REPLACES the local file. Without this folder, a signature
''' applied while the network was down would be overwritten by the older server version on the next
''' open -- the operator's signature silently lost. While an entry exists for a document, the views
''' show THAT copy and retry the upload instead of downloading over it.
''' </summary>
Public NotInheritable Class PendingPdfUploads

    Private Sub New()
    End Sub

    ''' <summary>The folder name, next to the executable (the operator sees it).</summary>
    Public Const FolderName As String = "PdfDeIncarcat"

    Private Shared ReadOnly _json As New JsonSerializerOptions With {.WriteIndented = True}

    Public Shared ReadOnly Property Root As String
        Get
            Return Path.Combine(AppContext.BaseDirectory, FolderName)
        End Get
    End Property

    Private Shared Function BaseName(kind As PdfDocKind, id As Integer) As String
        Return $"{If(kind = PdfDocKind.Ddf, "DDF", "ORD")}_{id}"
    End Function

    ''' <summary>Keeps a signed copy for a later upload (overwrites an older entry of the same document).</summary>
    Public Shared Sub Save(entry As PendingPdfUpload, pdfBytes As Byte())
        Try
            ArgumentNullException.ThrowIfNull(entry)
            If pdfBytes Is Nothing OrElse pdfBytes.Length = 0 Then Throw New ArgumentException("Empty PDF.", NameOf(pdfBytes))
            Directory.CreateDirectory(Root)
            Dim baseName As String = PendingPdfUploads.BaseName(entry.Kind, entry.Id)
            Dim pdf As String = Path.Combine(Root, baseName & ".pdf")
            ' .part + move, as in PdfCache: a half-written copy must never look like a valid one.
            File.WriteAllBytes(pdf & ".part", pdfBytes)
            File.Move(pdf & ".part", pdf, overwrite:=True)
            File.WriteAllText(Path.Combine(Root, baseName & ".json"),
                              JsonSerializer.Serialize(entry, _json), New UTF8Encoding(False))
            entry.PdfPath = pdf
        Catch ex As Exception
            GlobalErrorLog.Write("PendingPdfUploads.Save", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The pending entry of one document, or Nothing.</summary>
    Public Shared Function TryGet(kind As PdfDocKind, id As Integer) As PendingPdfUpload
        Try
            Dim baseName As String = PendingPdfUploads.BaseName(kind, id)
            Dim meta As String = Path.Combine(Root, baseName & ".json")
            Dim pdf As String = Path.Combine(Root, baseName & ".pdf")
            If Not File.Exists(meta) OrElse Not File.Exists(pdf) Then Return Nothing
            Dim entry As PendingPdfUpload = JsonSerializer.Deserialize(Of PendingPdfUpload)(File.ReadAllText(meta), _json)
            If entry Is Nothing Then Return Nothing
            entry.PdfPath = pdf
            Return entry
        Catch ex As Exception
            GlobalErrorLog.Write("PendingPdfUploads.TryGet", ex)
            Throw
        End Try
    End Function

    ''' <summary>Every pending entry on this machine.</summary>
    Public Shared Function All() As List(Of PendingPdfUpload)
        Try
            Dim list As New List(Of PendingPdfUpload)()
            If Not Directory.Exists(Root) Then Return list
            For Each meta As String In Directory.GetFiles(Root, "*.json")
                Dim entry As PendingPdfUpload = JsonSerializer.Deserialize(Of PendingPdfUpload)(File.ReadAllText(meta), _json)
                If entry Is Nothing Then Continue For
                entry.PdfPath = Path.ChangeExtension(meta, ".pdf")
                If File.Exists(entry.PdfPath) Then list.Add(entry)
            Next
            Return list
        Catch ex As Exception
            GlobalErrorLog.Write("PendingPdfUploads.All", ex)
            Throw
        End Try
    End Function

    ''' <summary>Marks an entry as refused with 409 (kept, never retried automatically).</summary>
    Public Shared Sub MarkConflict(entry As PendingPdfUpload)
        Try
            entry.Conflict = True
            File.WriteAllText(Path.Combine(Root, BaseName(entry.Kind, entry.Id) & ".json"),
                              JsonSerializer.Serialize(entry, _json), New UTF8Encoding(False))
        Catch ex As Exception
            GlobalErrorLog.Write("PendingPdfUploads.MarkConflict", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Removes an entry once the server has it. The <c>.json</c> goes first -- without it the entry
    ''' no longer exists; the <c>.pdf</c> may still be open in Adobe (when the kept copy is the one on
    ''' screen), so failing to delete it is logged, not fatal. A lone <c>.pdf</c> is ignored.
    ''' </summary>
    Public Shared Sub Remove(kind As PdfDocKind, id As Integer)
        Try
            Dim baseName As String = PendingPdfUploads.BaseName(kind, id)
            File.Delete(Path.Combine(Root, baseName & ".json"))
            Try
                File.Delete(Path.Combine(Root, baseName & ".pdf"))
            Catch ex As IOException
                GlobalErrorLog.Write("PendingPdfUploads.Remove.Pdf", ex)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("PendingPdfUploads.Remove", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Tries to upload one entry. On success the copy goes to its cache path, the entry is removed
    ''' and the server's answer is returned. 409 marks it as a conflict and rethrows; any other
    ''' failure leaves the entry untouched and rethrows.
    ''' </summary>
    Public Shared Async Function UploadAsync(api As IApiClient, entry As PendingPdfUpload) As Task(Of PutPdfResponse)
        Try
            ArgumentNullException.ThrowIfNull(api)
            ArgumentNullException.ThrowIfNull(entry)
            Dim bytes As Byte() = File.ReadAllBytes(entry.PdfPath)
            Dim resp As PutPdfResponse
            Try
                If entry.Kind = PdfDocKind.Ddf Then
                    resp = Await api.UploadDdfPdfAsync(entry.Id, bytes, entry.ShaPrecedent, entry.Semnatura,
                                                       entry.Semnaturi, CancellationToken.None).ConfigureAwait(True)
                Else
                    resp = Await api.UploadOrdPdfAsync(entry.Id, bytes, entry.ShaPrecedent, entry.Semnatura,
                                                       entry.Semnaturi, CancellationToken.None).ConfigureAwait(True)
                End If
            Catch ex As ApiException When ex.StatusCode.GetValueOrDefault() = 409
                MarkConflict(entry)
                Throw
            End Try
            If Not String.IsNullOrWhiteSpace(entry.CachePath) Then SignedPdfFiles.WriteCache(entry.CachePath, bytes)
            Remove(entry.Kind, entry.Id)
            Return resp
        Catch ex As Exception
            GlobalErrorLog.Write("PendingPdfUploads.UploadAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Retries every non-conflict entry (after login). Returns one Romanian line per entry for the
    ''' operator; an empty list when there was nothing to do. Never throws -- each failure is a line.
    ''' </summary>
    Public Shared Async Function RetryAllAsync(api As IApiClient) As Task(Of List(Of String))
        Dim lines As New List(Of String)()
        Try
            For Each entry As PendingPdfUpload In All()
                Dim label As String = $"{If(entry.Kind = PdfDocKind.Ddf, "DDF revizia", "ORD")} {entry.Id}"
                If entry.Conflict Then
                    lines.Add($"{label}: în conflict cu versiunea de pe server — rămâne în «{Root}» pentru decizia dumneavoastră.")
                    Continue For
                End If
                Try
                    Await UploadAsync(api, entry).ConfigureAwait(True)
                    lines.Add($"{label}: documentul semnat a fost încărcat pe server.")
                Catch ex As ApiException When ex.StatusCode.GetValueOrDefault() = 409
                    lines.Add($"{label}: între timp altcineva a salvat alt document semnat. Copia dumneavoastră rămâne în «{Root}».")
                Catch ex As Exception
                    lines.Add($"{label}: încărcarea a eșuat din nou ({ex.Message}). Se reîncearcă la următoarea pornire.")
                End Try
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("PendingPdfUploads.RetryAllAsync", ex)
            lines.Add("Documentele semnate neîncărcate nu au putut fi verificate. Detalii în jurnalul de erori.")
        End Try
        Return lines
    End Function

End Class
