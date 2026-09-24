Option Strict On
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Xfa

''' <summary>How one signing check ended (slice 0078).</summary>
Public Enum PdfSigningStatus
    ''' <summary>The server has the signed PDF and its roles.</summary>
    Uploaded = 0
    ''' <summary>409: somebody else stored a signed PDF first. Our copy is kept in PdfDeIncarcat.</summary>
    Conflict = 1
    ''' <summary>The upload failed (network, server). Our copy is kept in PdfDeIncarcat for a retry.</summary>
    Queued = 2
    ''' <summary>Adobe's «Save As» had to be cancelled -- the signature was NOT applied.</summary>
    SaveCancelled = 3
    ''' <summary>The saved file could not be read or checked.</summary>
    Failed = 4
End Enum

''' <summary>The result of one signing check. POCO -&gt; no Try/Catch.</summary>
Public NotInheritable Class PdfSigningOutcome
    Public Property Status As PdfSigningStatus
    ''' <summary>Roles now on the server ("A,Ordonator"), on <see cref="PdfSigningStatus.Uploaded"/>.</summary>
    Public Property Semnatura As String = String.Empty
    ''' <summary>The server's new sha, on <see cref="PdfSigningStatus.Uploaded"/>.</summary>
    Public Property NewSha As String = String.Empty
    ''' <summary>Romanian, for the operator.</summary>
    Public Property Message As String = String.Empty
End Class

''' <summary>
''' Watches ONE document on screen while the operator may sign it (slice 0078), and uploads it the
''' moment a new signature is saved.
'''
''' TWO SIGNALS: <see cref="NotifySaved"/> (the Save As trap reports Adobe closed the dialog) and a
''' FileSystemWatcher on the file itself (catches a save that did not go through a dialog). Both
''' only SCHEDULE a check; the check waits until the file stops changing, reads it (Adobe keeps it
''' open), and decides.
'''
''' WHAT COUNTS AS NEW: the set of SIGNED FIELD NAMES, not the role set -- a DDF role «A» has several
''' fields (SignatureField11..16), and a second A-field signature changes the PDF without changing
''' the roles. A save whose signed fields did not change (a plain form edit) is NOT uploaded; the
''' next open replaces it with the server copy, which is the rule of the slice.
'''
''' The session is created and used on the UI thread; FileSystemWatcher callbacks are posted back
''' to it through the captured SynchronizationContext.
''' </summary>
Public NotInheritable Class PdfSigningSession
    Implements IDisposable

    Private ReadOnly _api As IApiClient
    Private ReadOnly _ui As SynchronizationContext
    Private _watcher As FileSystemWatcher
    Private _baselineTask As Task(Of String)
    Private _baselineKey As String
    Private _checking As Boolean
    Private _again As Boolean
    Private _disposed As Boolean

    Public ReadOnly Property Kind As PdfDocKind
    ''' <summary>IDREV (DDF) or IDORDP (ORD).</summary>
    Public ReadOnly Property Id As Integer
    ''' <summary>The file Adobe shows -- the one the trap overwrites.</summary>
    Public ReadOnly Property DisplayedPath As String
    ''' <summary>Where the signed copy lives in the persistent cache (may equal DisplayedPath).</summary>
    Public ReadOnly Property CachePath As String
    ''' <summary>The sha the server has now ("" = no signed PDF on the server yet).</summary>
    Public Property ServerSha As String

    ''' <summary>Raised on the UI thread after every check that did something.</summary>
    Public Event Completed As Action(Of PdfSigningSession, PdfSigningOutcome)

    Public Sub New(kind As PdfDocKind, id As Integer, displayedPath As String, cachePath As String,
                   serverSha As String, api As IApiClient)
        ArgumentNullException.ThrowIfNull(api)
        If String.IsNullOrWhiteSpace(displayedPath) Then Throw New ArgumentException("Empty path.", NameOf(displayedPath))
        Me.Kind = kind
        Me.Id = id
        Me.DisplayedPath = displayedPath
        Me.CachePath = If(cachePath, String.Empty)
        Me.ServerSha = If(serverSha, String.Empty)
        _api = api
        _ui = If(SynchronizationContext.Current, New SynchronizationContext())
    End Sub

    Private ReadOnly Property DocType As String
        Get
            Return If(Kind = PdfDocKind.Ddf, "DDF", "ORD")
        End Get
    End Property

    ''' <summary>Same document, same file? Used by the views to keep a session across re-renders.</summary>
    Public Function Matches(kind As PdfDocKind, id As Integer, displayedPath As String) As Boolean
        Return Me.Kind = kind AndAlso Me.Id = id AndAlso
               String.Equals(Me.DisplayedPath, displayedPath, StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' Starts watching: reads the signed-field baseline of the file as it is NOW (in the
    ''' background), and arms the FileSystemWatcher.
    ''' </summary>
    Public Sub Begin()
        Try
            Dim path As String = DisplayedPath
            Dim docType As String = Me.DocType
            _baselineTask = Task.Run(Function() ReadFieldKey(path, docType))
            Dim dir As String = IO.Path.GetDirectoryName(path)
            If Directory.Exists(dir) Then
                _watcher = New FileSystemWatcher(dir, IO.Path.GetFileName(path)) With {
                    .NotifyFilter = NotifyFilters.LastWrite Or NotifyFilters.Size Or NotifyFilters.FileName}
                AddHandler _watcher.Changed, AddressOf OnFileEvent
                AddHandler _watcher.Created, AddressOf OnFileEvent
                AddHandler _watcher.Renamed, AddressOf OnFileEvent
                _watcher.EnableRaisingEvents = True
            End If
            AdobeHostLogLine($"Sesiune de semnare pornită: {DocType} {Id}, fișier {path}.")
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningSession.Begin", ex)
            Throw
        End Try
    End Sub

    ' The signed-field key of a file; "" when it does not exist, has no signature or cannot be read
    ' (a file that cannot be read now is treated as «nothing signed yet» -- the check after the next
    ' save reads it again and is the one that decides).
    Private Shared Function ReadFieldKey(path As String, docType As String) As String
        Try
            If Not File.Exists(path) Then Return ""
            Return PdfSignatures.Read(SignedPdfFiles.ReadShared(path), docType).FieldKey()
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningSession.ReadFieldKey", ex)
            Return ""
        End Try
    End Function

    ''' <summary>The Save As trap reports Adobe saved <paramref name="path"/>.</summary>
    Public Sub NotifySaved(path As String)
        Try
            If Not String.Equals(path, DisplayedPath, StringComparison.OrdinalIgnoreCase) Then Return
            ScheduleCheck()
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningSession.NotifySaved", ex)
        End Try
    End Sub

    ''' <summary>The Save As trap had to cancel: the signature was not applied.</summary>
    Public Sub NotifySaveCancelled(reason As String)
        Try
            RaiseCompleted(New PdfSigningOutcome With {
                .Status = PdfSigningStatus.SaveCancelled,
                .Message = "Semnătura NU a fost aplicată: salvarea documentului a fost oprită de K-BOT, " &
                           "ca fișierul să nu ajungă în alt loc." & Environment.NewLine & reason &
                           Environment.NewLine & "Încercați din nou semnarea."})
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningSession.NotifySaveCancelled", ex)
        End Try
    End Sub

    ' FileSystemWatcher thread -> UI thread.
    Private Sub OnFileEvent(sender As Object, e As FileSystemEventArgs)
        Try
            If _disposed Then Return
            _ui.Post(Sub(state) ScheduleCheck(), Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningSession.OnFileEvent", ex)
        End Try
    End Sub

    Private Sub ScheduleCheck()
        If _disposed Then Return
        If _checking Then
            _again = True
            Return
        End If
        CheckAsync()
    End Sub

    ' UI-thread async boundary: log and swallow. Loops while new save signals arrived meanwhile.
    Private Async Sub CheckAsync()
        _checking = True
        Try
            Do
                _again = False
                Await CheckOnceAsync().ConfigureAwait(True)
            Loop While _again AndAlso Not _disposed
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningSession.CheckAsync", ex)
        Finally
            _checking = False
        End Try
    End Sub

    Private Async Function CheckOnceAsync() As Task
        If _baselineKey Is Nothing AndAlso _baselineTask IsNot Nothing Then
            _baselineKey = Await _baselineTask.ConfigureAwait(True)
        End If
        If _baselineKey Is Nothing Then _baselineKey = ""

        Dim bytes As Byte() = Await SignedPdfFiles.ReadWhenSettledAsync(DisplayedPath).ConfigureAwait(True)
        If bytes Is Nothing OrElse _disposed Then Return

        ' Exactly the server copy: nothing to do (e.g. the watcher fired on our own cache write).
        Dim sha As String = PdfHash.Compute(bytes)
        If PdfHash.AreEqual(sha, ServerSha) Then Return

        Dim docType As String = Me.DocType
        Dim info As PdfSignatureInfo
        Try
            info = Await Task.Run(Function() PdfSignatures.Read(bytes, docType)).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningSession.CheckOnceAsync.Read", ex)
            RaiseCompleted(New PdfSigningOutcome With {
                .Status = PdfSigningStatus.Failed,
                .Message = "Documentul salvat de Adobe nu a putut fi citit, deci semnăturile nu au putut fi verificate. " &
                           "Detalii în jurnalul de erori."})
            Return
        End Try

        If Not info.IsSigned Then Return
        Dim key As String = info.FieldKey()
        If String.Equals(key, _baselineKey, StringComparison.Ordinal) Then Return

        If info.Unclassified.Count > 0 Then
            AdobeHostLogLine($"ATENȚIE: câmpuri semnate fără rol cunoscut: {String.Join(", ", info.Unclassified)}.")
        End If
        Dim semnatura As String = info.ToSemnatura()
        ' Slice 0079: only the fields signed since the baseline go to the signature log -- plus the
        ' ones a kept copy of this document added and never managed to report.
        Dim records As List(Of PdfSignatureRecord) = SignatureRecords.Added(info, _baselineKey)
        Try
            Dim kept As PendingPdfUpload = PendingPdfUploads.TryGet(Kind, Id)
            If kept IsNot Nothing Then records = SignatureRecords.Merge(records, kept.Semnaturi)
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningSession.CheckOnceAsync.Pending", ex)
        End Try
        Await UploadAsync(bytes, key, semnatura, records).ConfigureAwait(True)
    End Function

    Private Async Function UploadAsync(bytes As Byte(), key As String, semnatura As String,
                                       records As List(Of PdfSignatureRecord)) As Task
        Dim precedent As String = If(String.IsNullOrWhiteSpace(ServerSha), ApiClient.ShaFaraRand, ServerSha)
        Dim roles As String = If(String.IsNullOrEmpty(semnatura), Nothing, semnatura)
        Try
            Dim resp As PutPdfResponse
            If Kind = PdfDocKind.Ddf Then
                resp = Await _api.UploadDdfPdfAsync(Id, bytes, precedent, roles, records, CancellationToken.None).ConfigureAwait(True)
            Else
                resp = Await _api.UploadOrdPdfAsync(Id, bytes, precedent, roles, records, CancellationToken.None).ConfigureAwait(True)
            End If

            ServerSha = resp.sha256
            _baselineKey = key
            ' The persistent cache gets the signed copy when the document on screen is elsewhere
            ' (the unsigned TempPdf copy, or a PdfDeIncarcat copy). Not fatal if it fails: the next
            ' open downloads it from the server anyway.
            If Not String.IsNullOrWhiteSpace(CachePath) AndAlso
               Not String.Equals(CachePath, DisplayedPath, StringComparison.OrdinalIgnoreCase) Then
                Try
                    SignedPdfFiles.WriteCache(CachePath, bytes)
                Catch ex As Exception
                    GlobalErrorLog.Write("PdfSigningSession.UploadAsync.Cache", ex)
                End Try
            End If
            ' A kept copy of this document is now obsolete: the server has a newer signed version.
            Try
                If PendingPdfUploads.TryGet(Kind, Id) IsNot Nothing Then PendingPdfUploads.Remove(Kind, Id)
            Catch ex As Exception
                GlobalErrorLog.Write("PdfSigningSession.UploadAsync.Pending", ex)
            End Try

            Dim stored As String = If(String.IsNullOrEmpty(resp.semnatura), semnatura, resp.semnatura)
            AdobeHostLogLine($"Document semnat încărcat: {DocType} {Id}, roluri «{stored}», sha {Left8(resp.sha256)}.")
            RaiseCompleted(New PdfSigningOutcome With {
                .Status = PdfSigningStatus.Uploaded,
                .Semnatura = If(stored, String.Empty),
                .NewSha = resp.sha256,
                .Message = $"Semnătura a fost salvată pe server ({If(String.IsNullOrEmpty(stored), "fără rol recunoscut", stored.Replace(",", ", "))})."})
        Catch ex As ApiException When ex.StatusCode.GetValueOrDefault() = 409
            GlobalErrorLog.Write("PdfSigningSession.UploadAsync", ex)
            Keep(bytes, precedent, semnatura, records, conflict:=True)
            RaiseCompleted(New PdfSigningOutcome With {
                .Status = PdfSigningStatus.Conflict,
                .Message = "Între timp altcineva a salvat pe server o altă versiune semnată a acestui document. " &
                           $"Semnătura dumneavoastră NU a fost încărcată; copia ei se păstrează în «{PendingPdfUploads.Root}». " &
                           "Se afișează versiunea de pe server."})
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningSession.UploadAsync", ex)
            Keep(bytes, precedent, semnatura, records, conflict:=False)
            RaiseCompleted(New PdfSigningOutcome With {
                .Status = PdfSigningStatus.Queued,
                .Message = "Documentul semnat NU a putut fi încărcat pe server acum" &
                           If(TypeOf ex Is ApiException, $" ({ex.Message})", "") & ". " &
                           $"Copia semnată se păstrează în «{PendingPdfUploads.Root}» și se încarcă automat " &
                           "la următoarea deschidere a documentului sau la următoarea pornire."})
        End Try
    End Function

    Private Sub Keep(bytes As Byte(), precedent As String, semnatura As String,
                     records As List(Of PdfSignatureRecord), conflict As Boolean)
        Try
            Dim entry As New PendingPdfUpload With {
                .Kind = Kind, .Id = Id, .ShaPrecedent = precedent, .Semnatura = If(semnatura, String.Empty),
                .CachePath = CachePath, .Created = DateTime.Now, .Conflict = conflict,
                .Semnaturi = If(records, New List(Of PdfSignatureRecord)())}
            PendingPdfUploads.Save(entry, bytes)
        Catch ex As Exception
            ' Already logged by Save. The operator still gets the message; the file on screen keeps
            ' the signature until the next open.
            GlobalErrorLog.Write("PdfSigningSession.Keep", ex)
        End Try
    End Sub

    Private Sub RaiseCompleted(outcome As PdfSigningOutcome)
        If _disposed Then Return
        RaiseEvent Completed(Me, outcome)
    End Sub

    Private Shared Function Left8(s As String) As String
        If String.IsNullOrEmpty(s) Then Return ""
        Return If(s.Length > 8, s.Substring(0, 8), s)
    End Function

    Private Shared Sub AdobeHostLogLine(line As String)
        AdobeHostLog.Write(line)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            _disposed = True
            If _watcher IsNot Nothing Then
                _watcher.EnableRaisingEvents = False
                RemoveHandler _watcher.Changed, AddressOf OnFileEvent
                RemoveHandler _watcher.Created, AddressOf OnFileEvent
                RemoveHandler _watcher.Renamed, AddressOf OnFileEvent
                _watcher.Dispose()
                _watcher = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningSession.Dispose", ex)
        End Try
    End Sub

End Class
