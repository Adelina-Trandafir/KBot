Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Note corecție» (slice 0088) -- the CAB correction notes (F1135) whose correction row went to the
''' selected angajament. Same shape as <see cref="OrdView"/>: a month -&gt; note tree on the left, and
''' two lazy pages on the right, «Vizualizare» (the note's rows) and «Document» (its PDF).
'''
''' <para><b>The document</b> (operator, 28.09.2026): a SIGNED note is downloaded from the server
''' (<see cref="PdfCache"/>, checked by sha) and never regenerated; an UNSIGNED one is generated here
''' from the stored note, into the work area. Whatever is on screen is watched by a
''' <see cref="PdfSigningSession"/> (family <see cref="PdfDocKind.Nc"/>): a signature saved in Adobe
''' goes to the server at once, replacing the unsigned copy stored when the note was made.</para>
'''
''' <para>The notes come in ONE call per angajament (GET /api/forexe/note-cab?cod=), through the
''' shell's 401 net; a click in the tree only filters. Right click on a note: «Încarcă în CAB»,
''' through the shell's <c>UploadCabNoteAsync</c> (the same step as the note window).</para>
''' </summary>
Public Class NoteCabView
    Implements IAngajamentView, IThemedControl

    Private Const PageView As String = "vizualizare"
    Private Const PageDocument As String = "document"
    Private Const PageReceipt As String = "recipisa"

    Private Const IconMonth As String = "month"
    Private Const IconUnsigned As String = "unsigned"
    Private Const IconSigned As String = "signed"
    Private Const IconSent As String = "sent"

    Private Const MenuUpload As String = "incarca"
    Private Const MenuRegenerate As String = "regenereaza"

    Private Shared ReadOnly RoCulture As New CultureInfo("ro-RO")

    Private ReadOnly _apiClient As IApiClient
    Private ReadOnly _withReauth As Func(Of Func(Of Task(Of List(Of CabCorrectionNote))), Task(Of List(Of CabCorrectionNote)))
    Private ReadOnly _uploadToCab As Func(Of IWin32Window, CabCorrectionNote, String, Task(Of Boolean))
    ' Slice 0088-04: the shell's receipt window («Validează documentul»); True = a receipt was stored.
    Private ReadOnly _openReceipt As Func(Of IWin32Window, CabCorrectionNote, Boolean)
    ' The note whose receipt is being downloaded now (0 = none).
    Private _receiptLoadingFor As Integer

    Private ReadOnly _pages As New Dictionary(Of String, ICabNotePage)(StringComparer.Ordinal)
    Private _activePage As ICabNotePage
    Private _currentCtx As CabNotePageContext

    Private _requestedCod As String
    Private _notes As List(Of CabCorrectionNote)
    Private _nodeNotes As List(Of CabCorrectionNote)
    Private _selected As CabCorrectionNote
    ' The document resolved for the selected note (downloaded signed copy, or generated work copy).
    Private _resolvedPath As String
    Private _resolvedFor As Integer
    Private _generating As Boolean
    Private _signing As PdfSigningSession
    Private ReadOnly _leaves As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()

    Private _splitterExpanded As Integer
    Private _panel1MinExpanded As Integer

    Public Sub New(apiClient As IApiClient,
                   withReauth As Func(Of Func(Of Task(Of List(Of CabCorrectionNote))), Task(Of List(Of CabCorrectionNote))),
                   Optional uploadToCab As Func(Of IWin32Window, CabCorrectionNote, String, Task(Of Boolean)) = Nothing,
                   Optional openReceipt As Func(Of IWin32Window, CabCorrectionNote, Boolean) = Nothing)
        ArgumentNullException.ThrowIfNull(apiClient)
        ArgumentNullException.ThrowIfNull(withReauth)
        InitializeComponent()
        _apiClient = apiClient
        _withReauth = withReauth
        _uploadToCab = uploadToCab
        _openReceipt = openReceipt
        AddHandler Disposed, Sub(s, e) EndSigning()
        navSub.SelectedKey = PageView
        ShowEmpty("Selectați un angajament din arbore.")
    End Sub

    Public ReadOnly Property ViewKey As String Implements IAngajamentView.ViewKey
        Get
            Return "notecab"
        End Get
    End Property

    Private ReadOnly Property NotesApi As ICabNotesApi
        Get
            Dim api As ICabNotesApi = TryCast(_apiClient, ICabNotesApi)
            If api Is Nothing Then Throw New InvalidOperationException("The API client does not implement ICabNotesApi.")
            Return api
        End Get
    End Property

    ''' <summary>Reloads the notes of the angajament on screen (after the note window saved some).</summary>
    Public Sub Reincarca()
        Try
            If String.IsNullOrWhiteSpace(_requestedCod) Then Return
            ShowEmpty("Se încarcă notele de corecție…")
            LoadAsync(_requestedCod)
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.Reincarca", ex)
        End Try
    End Sub

    ' ── Pages ──────────────────────────────────────────────────────────────────

    Private Sub NavSub_SelectionChanged(key As String) Handles navSub.SelectionChanged
        Try
            ActivatePage(key)
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.NavSub_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub ActivatePage(key As String)
        Try
            Dim page As ICabNotePage = Nothing
            If Not _pages.TryGetValue(key, page) Then
                Select Case key
                    Case PageView : page = New CabNoteVizualizarePage()
                    Case PageDocument : page = New CabNoteDocumentPage()
                    Case PageReceipt : page = New CabNoteReceiptPage()
                    Case Else
                        Throw New ArgumentException($"Unknown notes page: '{key}'.", NameOf(key))
                End Select
                If key = PageReceipt Then
                    AddHandler page.GenerateRequested, AddressOf OnValidateRequested
                Else
                    AddHandler page.GenerateRequested, AddressOf OnGenerateRequested
                End If
                Dim ctrl As Control = DirectCast(page, Control)
                ctrl.Dock = DockStyle.Fill
                ctrl.Visible = False
                pnlPages.Controls.Add(ctrl)
                ThemeManager.Apply(ctrl)
                _pages(key) = page
            End If
            Dim previous As ICabNotePage = _activePage
            _activePage = page
            DirectCast(page, Control).Visible = True
            If previous IsNot Nothing AndAlso Not ReferenceEquals(previous, page) Then DirectCast(previous, Control).Visible = False
            page.SetContext(_currentCtx)
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.ActivatePage", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' The context of the node selected now. The document of a note: the resolved one (downloaded
    ''' signed copy / generated work copy) when there is one, else -- for an unsigned note -- its work
    ''' copy path (missing = the «Generează» surface). A signed note shows nothing until its copy is
    ''' checked against the server.
    ''' </summary>
    Private Function BuildContext() As CabNotePageContext
        If String.IsNullOrWhiteSpace(_requestedCod) OrElse _notes Is Nothing Then Return Nothing
        Dim path As String = Nothing
        If _selected IsNot Nothing Then
            If Not String.IsNullOrEmpty(_resolvedPath) AndAlso _resolvedFor = _selected.IdNc Then
                path = _resolvedPath
            ElseIf Not _selected.IsSigned Then
                path = CabNoteFiles.WorkPath(_selected)
            End If
        End If
        Dim exists As Boolean = Not String.IsNullOrEmpty(path) AndAlso File.Exists(path)
        Dim ctx As New CabNotePageContext(_nodeNotes, _selected, path, exists)
        ctx.Signing = EnsureSigning(path, exists)
        Dim r As CabNoteReceipt = _selected?.Receipt
        If r IsNot Nothing Then
            ctx.ReceiptPath = CabNoteFiles.ReceiptPath(_selected, r.RegistrationIndex, r.FileName)
            ctx.ReceiptExists = Not String.IsNullOrEmpty(ctx.ReceiptPath) AndAlso File.Exists(ctx.ReceiptPath)
        End If
        Return ctx
    End Function

    Private Sub PushToActivePage()
        _currentCtx = BuildContext()
        _activePage?.SetContext(_currentCtx)
    End Sub

    ' ── Shell context ──────────────────────────────────────────────────────────

    Public Sub SetContext(info As AngajamentTreeInfo) Implements IAngajamentView.SetContext
        Try
            Dim cod As String = info?.CodAngajament
            If String.IsNullOrWhiteSpace(cod) Then
                ClearAll()
                ShowEmpty("Selectați un angajament din arbore.")
                Return
            End If
            _requestedCod = cod
            ShowEmpty("Se încarcă notele de corecție…")
            LoadAsync(cod)
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.SetContext", ex)
            Throw
        End Try
    End Sub

    ' UI boundary (started without await): log and show.
    Private Async Sub LoadAsync(cod As String)
        Try
            Dim api As ICabNotesApi = NotesApi
            Dim notes As List(Of CabCorrectionNote) = Await _withReauth(
                Function() api.GetCabNotesAsync(cod, CancellationToken.None)).ConfigureAwait(True)
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            If notes Is Nothing OrElse notes.Count = 0 Then
                ClearAll()
                ShowEmpty("Angajamentul nu are note de corecție CAB.")
                Return
            End If
            _notes = notes
            _nodeNotes = notes
            _selected = Nothing
            _resolvedPath = Nothing
            _resolvedFor = 0
            BuildTree()
            PushToActivePage()
            ShowContent()
        Catch ex As ApiException
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("NoteCabView.LoadAsync", ex)
            ClearAll()
            ShowEmpty(ex.Message)
        Catch ex As Exception
            If Not String.Equals(_requestedCod, cod, StringComparison.Ordinal) Then Return
            GlobalErrorLog.Write("NoteCabView.LoadAsync", ex)
            ClearAll()
            ShowEmpty("Notele de corecție nu au putut fi încărcate. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' ── Tree ──────────────────────────────────────────────────────────────────

    Private Sub BuildTree()
        Try
            tree.Clear()
            _leaves.Clear()
            If _notes Is Nothing Then Return
            Dim icoMonth As Image = tree.NodeImage(IconMonth)
            For Each mg In _notes.GroupBy(Function(n) n.NoteDate.Year * 100 + n.NoteDate.Month).OrderBy(Function(g) g.Key)
                Dim monthNotes As List(Of CabCorrectionNote) = mg.OrderBy(Function(n) n.NoteNumber).ToList()
                Dim total As Decimal = monthNotes.Sum(Function(n) n.Total)
                Dim root As AdvancedTreeControl.TreeItem = tree.AddItem(
                    $"NC_{mg.Key}", $"{MonthLabel(mg.Key Mod 100)} {mg.Key \ 100}~~~{Money(total)}",
                    pLeftIconClosed:=icoMonth, pLeftIconOpen:=icoMonth, pExpanded:=True)
                root.Tag = New CabNoteNodePayload(monthNotes, Nothing)
                root.Bold = True
                For Each n As CabCorrectionNote In monthNotes
                    Dim icon As Image = tree.NodeImage(If(n.Sent, IconSent, If(n.IsSigned, IconSigned, IconUnsigned)))
                    Dim leaf As AdvancedTreeControl.TreeItem = tree.AddItem(
                        $"NC_{n.IdNc}", $"Nota nr. {n.NoteNumber} · {n.NoteDate:dd.MM}~~~{Money(n.Total)}",
                        root, pLeftIconClosed:=icon, pLeftIconOpen:=icon)
                    leaf.Tag = New CabNoteNodePayload(New List(Of CabCorrectionNote) From {n}, n)
                    leaf.Tooltip = String.Join(vbLf, n.Corrections.Select(
                                       Function(c) $"{c.TreasuryReference} / nr. {c.DocumentNumber} → {c.CommitmentCode} / {c.IndicatorCode}")) &
                                   vbLf & If(n.IsSigned, "Semnată: " & n.Signature, "Nesemnată") &
                                   vbLf & If(n.Sent, "Trimisă în CAB", "Netrimisă în CAB") &
                                   vbLf & If(n.Receipt IsNot Nothing,
                                             "Recipisă: " & If(String.IsNullOrEmpty(n.Receipt.RegistrationNumber),
                                                               n.Receipt.RegistrationIndex, n.Receipt.RegistrationNumber),
                                             "Fără recipisă FOREXE")
                    _leaves(n.IdNc) = leaf
                Next
            Next
            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.BuildTree", ex)
            Throw
        End Try
    End Sub

    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing Then Return
            Dim payload As CabNoteNodePayload = TryCast(pNode.Tag, CabNoteNodePayload)
            If payload Is Nothing Then Return
            Dim changed As Boolean = Not ReferenceEquals(_selected, payload.Note)
            _nodeNotes = payload.Notes
            _selected = payload.Note
            If changed Then
                _resolvedPath = Nothing
                _resolvedFor = 0
            End If
            PushToActivePage()
            If e IsNot Nothing AndAlso e.Button = MouseButtons.Right AndAlso _selected IsNot Nothing Then
                ShowMenu(_selected)
                Return
            End If
            EnsureDocumentAsync(_selected)
            EnsureReceiptAsync(_selected)
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    Private Sub ShowMenu(note As CabCorrectionNote)
        Dim items As New List(Of CustomPopupItem) From {New CustomPopupItem(MenuUpload, "Încarcă în &CAB…")}
        If Not note.IsSigned Then items.Add(New CustomPopupItem(MenuRegenerate, "&Generează din nou documentul"))
        Dim menu As New CustomPopup(items)
        AddHandler menu.ItemClicked, Sub(s As Object, ev As CustomPopupItemEventArgs) OnMenu(ev.Item.Key, note)
        menu.ShowAtCursor(tree)
    End Sub

    Private Sub OnMenu(key As String, note As CabCorrectionNote)
        Try
            Select Case key
                Case MenuUpload : UploadAsync(note)
                Case MenuRegenerate : GenerateAsync(note)
                Case Else
                    Throw New ArgumentException($"Unknown menu command: {key}", NameOf(key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.OnMenu", ex)
        End Try
    End Sub

    ' ── Document ─────────────────────────────────────────────────────────────

    ''' <summary>
    ''' A signed note: its server copy into the cache (downloaded only when the local one differs).
    ''' A signed copy kept on this computer after a failed upload goes first, as in OrdView.
    ''' Unsigned: nothing to fetch -- the page shows the work copy or the «Generează» surface.
    ''' UI boundary (async Sub): log and swallow.
    ''' </summary>
    Private Async Sub EnsureDocumentAsync(note As CabCorrectionNote)
        Try
            If note Is Nothing Then Return
            Dim id As Integer = note.IdNc
            Dim cachePath As String = CabNoteFiles.CachePath(note)

            Dim pending As PendingPdfUpload = PendingPdfUploads.TryGet(PdfDocKind.Nc, id)
            If pending IsNot Nothing AndAlso Not pending.Conflict Then
                If String.IsNullOrWhiteSpace(pending.CachePath) Then pending.CachePath = cachePath
                Try
                    Dim resp As PutPdfResponse = Await PendingPdfUploads.UploadAsync(_apiClient, pending).ConfigureAwait(True)
                    note.PdfSha256 = resp.sha256
                    If Not String.IsNullOrEmpty(resp.semnatura) Then note.Signature = resp.semnatura
                    SigningMessages.ShowPendingUploaded(FindForm())
                Catch ex As ApiException When ex.StatusCode.GetValueOrDefault() = 409
                    SigningMessages.ShowPendingConflict(FindForm())
                Catch ex As Exception
                    GlobalErrorLog.Write("NoteCabView.EnsureDocumentAsync.Pending", ex)
                    If _selected Is Nothing OrElse _selected.IdNc <> id Then Return
                    _resolvedPath = pending.PdfPath
                    _resolvedFor = id
                    PushToActivePage()
                    Return
                End Try
            End If

            If Not note.IsSigned Then
                If _selected IsNot Nothing AndAlso _selected.IdNc = id Then PushToActivePage()
                Return
            End If

            Dim api As ICabNotesApi = NotesApi
            Dim result As PdfCacheResult = Await PdfCache.EnsureAsync(
                cachePath, note.PdfSha256,
                Function(localSha) api.DownloadCabNotePdfAsync(id, localSha, CancellationToken.None)).ConfigureAwait(True)
            If _selected Is Nothing OrElse _selected.IdNc <> id Then Return
            Select Case result.Status
                Case PdfCacheStatus.Gata
                    _resolvedPath = result.Cale
                    _resolvedFor = id
                    PushToActivePage()
                    If result.LocalReplaced Then SigningMessages.ShowLocalReplaced(FindForm())
                Case PdfCacheStatus.Eroare
                    ShowEmpty(result.Mesaj)
                Case Else
                    ' The server has no PDF (a race): nothing signed to show.
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.EnsureDocumentAsync", ex)
        End Try
    End Sub

    ' ── Receipt (slice 0088-04) ─────────────────────────────────────────────────

    ''' <summary>
    ''' The note's receipt, when the server has one and this computer does not: downloaded into
    ''' <see cref="CabNoteFiles.ReceiptPath"/>. The «Recipisă» page shows «Se descarcă…» meanwhile.
    ''' UI boundary (async Sub): log and swallow.
    ''' </summary>
    Private Async Sub EnsureReceiptAsync(note As CabCorrectionNote)
        Try
            Dim r As CabNoteReceipt = note?.Receipt
            If r Is Nothing OrElse _receiptLoadingFor = note.IdNc Then Return
            Dim target As String = CabNoteFiles.ReceiptPath(note, r.RegistrationIndex, r.FileName)
            If String.IsNullOrEmpty(target) OrElse File.Exists(target) Then Return
            _receiptLoadingFor = note.IdNc
            Try
                Dim api As ICabNotesApi = NotesApi
                Dim bytes As Byte() = Await api.DownloadCabNoteReceiptAsync(r.IdReceipt, CancellationToken.None).ConfigureAwait(True)
                CabNoteFiles.WriteReceipt(target, bytes)
            Finally
                _receiptLoadingFor = 0
            End Try
            If _selected IsNot Nothing AndAlso _selected.IdNc = note.IdNc Then PushToActivePage()
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.EnsureReceiptAsync", ex)
        End Try
    End Sub

    ' «Validează documentul» on the «Recipisă» page: the shell's receipt window.
    Private Sub OnValidateRequested(sender As Object, e As EventArgs)
        Try
            If _selected Is Nothing Then Return
            If _openReceipt Is Nothing Then
                KBotMessage.Show(FindForm(), "Verificarea recipisei nu este disponibilă în acest context.",
                                 "Nota de corecție CAB", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            ' A stored receipt comes back through ReceiptSaved (called by the shell).
            _openReceipt(FindForm(), _selected)
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.OnValidateRequested", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The shell stored a receipt for <paramref name="note"/> (after an upload or «Validează
    ''' documentul»): the note on screen takes it and the «Recipisă» page shows the file.
    ''' </summary>
    Public Sub ReceiptSaved(note As CabCorrectionNote)
        Try
            If note Is Nothing OrElse _notes Is Nothing Then Return
            Dim mine As CabCorrectionNote = _notes.FirstOrDefault(Function(x) x.IdNc = note.IdNc)
            If mine Is Nothing Then Return
            If Not ReferenceEquals(mine, note) Then
                mine.Receipt = note.Receipt
                mine.RegistrationIndex = note.RegistrationIndex
                mine.Sent = mine.Sent OrElse note.Sent
            End If
            BuildTreeKeepingSelection()
            EnsureReceiptAsync(mine)
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.ReceiptSaved", ex)
        End Try
    End Sub

    Private Sub OnGenerateRequested(sender As Object, e As EventArgs)
        Try
            GenerateAsync(_selected)
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.OnGenerateRequested", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The unsigned note's PDF, generated from the stored note into the work area. A signed note is
    ''' never generated (operator, 28.09.2026) -- its document is the server's. UI boundary.
    ''' </summary>
    Private Async Sub GenerateAsync(note As CabCorrectionNote)
        Try
            If _generating OrElse note Is Nothing Then Return
            If note.IsSigned Then
                KBotMessage.Show(FindForm(), "Nota este semnată: documentul ei se descarcă de pe server, nu se generează din nou.",
                                 "Nota de corecție CAB", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            _generating = True
            Try
                EndSigning()   ' the file on screen is about to be replaced
                Dim path As String = Await Task.Run(Function() CabNoteFiles.Generate(note)).ConfigureAwait(True)
                If _selected Is Nothing OrElse _selected.IdNc <> note.IdNc Then Return
                _resolvedPath = path
                _resolvedFor = note.IdNc
                PushToActivePage()
            Finally
                _generating = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.GenerateAsync", ex)
            KBotMessage.Show(FindForm(), "Documentul notei nu a putut fi generat: " & ex.Message,
                             "Nota de corecție CAB", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>«Încarcă în CAB»: the document on screen (the signed copy when there is one). UI boundary.</summary>
    Private Async Sub UploadAsync(note As CabCorrectionNote)
        Try
            If note Is Nothing Then Return
            If _uploadToCab Is Nothing Then
                KBotMessage.Show(FindForm(), "Încărcarea în CAB nu este disponibilă în acest context.",
                                 "Nota de corecție CAB", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim path As String = Nothing
            If _resolvedFor = note.IdNc AndAlso Not String.IsNullOrEmpty(_resolvedPath) AndAlso File.Exists(_resolvedPath) Then
                path = _resolvedPath
            ElseIf note.IsSigned Then
                Dim api As ICabNotesApi = NotesApi
                Dim result As PdfCacheResult = Await PdfCache.EnsureAsync(
                    CabNoteFiles.CachePath(note), note.PdfSha256,
                    Function(localSha) api.DownloadCabNotePdfAsync(note.IdNc, localSha, CancellationToken.None)).ConfigureAwait(True)
                If result.Status = PdfCacheStatus.Gata Then path = result.Cale
            Else
                Dim work As String = CabNoteFiles.WorkPath(note)
                If Not File.Exists(work) Then work = Await Task.Run(Function() CabNoteFiles.Generate(note)).ConfigureAwait(True)
                path = work
            End If
            If Await _uploadToCab(FindForm(), note, path).ConfigureAwait(True) Then BuildTreeKeepingSelection()
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.UploadAsync", ex)
        End Try
    End Sub

    Private Sub BuildTreeKeepingSelection()
        Dim keep As CabCorrectionNote = _selected
        BuildTree()
        Dim leaf As AdvancedTreeControl.TreeItem = Nothing
        If keep IsNot Nothing AndAlso _leaves.TryGetValue(keep.IdNc, leaf) Then tree.SelectAndReveal(leaf)
        PushToActivePage()
    End Sub

    ' ── Signing (slice 0078 machinery, family NC) ──────────────────────────────

    Private Function EnsureSigning(pdfPath As String, exists As Boolean) As PdfSigningSession
        Dim n As CabCorrectionNote = _selected
        If Not exists OrElse n Is Nothing Then
            EndSigning()
            Return Nothing
        End If
        Dim serverSha As String = n.PdfSha256
        Dim pending As PendingPdfUpload = PendingPdfUploads.TryGet(PdfDocKind.Nc, n.IdNc)
        If pending IsNot Nothing AndAlso String.Equals(pending.PdfPath, pdfPath, StringComparison.OrdinalIgnoreCase) Then
            serverSha = If(pending.ShaPrecedent = ApiClient.ShaFaraRand, String.Empty, pending.ShaPrecedent)
        End If
        ' Slice 0078-04: kept only while the server version is the one it started from.
        If _signing IsNot Nothing AndAlso _signing.Matches(PdfDocKind.Nc, n.IdNc, pdfPath, serverSha) Then Return _signing
        EndSigning()
        _signing = New PdfSigningSession(PdfDocKind.Nc, n.IdNc, pdfPath, CabNoteFiles.CachePath(n), serverSha, _apiClient)
        AddHandler _signing.Completed, AddressOf OnSigningCompleted
        _signing.Begin()
        Return _signing
    End Function

    Private Sub EndSigning()
        Try
            If _signing Is Nothing Then Return
            RemoveHandler _signing.Completed, AddressOf OnSigningCompleted
            _signing.Dispose()
            _signing = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.EndSigning", ex)
        End Try
    End Sub

    Private Sub OnSigningCompleted(session As PdfSigningSession, outcome As PdfSigningOutcome)
        Try
            If outcome Is Nothing Then Return
            Dim n As CabCorrectionNote = _notes?.FirstOrDefault(Function(x) x.IdNc = session.Id)
            If outcome.Status = PdfSigningStatus.Uploaded AndAlso n IsNot Nothing Then
                n.PdfSha256 = outcome.NewSha
                n.Signature = outcome.Semnatura
                BuildTreeKeepingSelection()
            End If
            SigningMessages.ShowOutcome(FindForm(), outcome)
            If outcome.Status = PdfSigningStatus.Conflict Then Reincarca()
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.OnSigningCompleted", ex)
        End Try
    End Sub

    ' ── Collapse, empty state, formatting, theme ───────────────────────────────

    Private Sub Tree_CollapsedChanged(collapsed As Boolean) Handles tree.CollapsedChanged
        Try
            Dim padLeft As Integer = split.Panel1.Padding.Left
            If collapsed Then
                _splitterExpanded = split.SplitterDistance
                _panel1MinExpanded = split.Panel1MinSize
                Dim target As Integer = tree.MinimumCollapsedWidth + padLeft
                split.Panel1MinSize = Math.Min(_panel1MinExpanded, target)
                split.SplitterDistance = ClampSplitter(target)
                split.IsSplitterFixed = True
            Else
                split.IsSplitterFixed = False
                If _panel1MinExpanded > 0 Then split.Panel1MinSize = _panel1MinExpanded
                split.SplitterDistance = ClampSplitter(If(_splitterExpanded > 0, _splitterExpanded, tree.ExpandedWidth + padLeft))
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.Tree_CollapsedChanged", ex)
        End Try
    End Sub

    Private Function ClampSplitter(wanted As Integer) As Integer
        Dim max As Integer = split.Width - split.Panel2MinSize - split.SplitterWidth
        If max < split.Panel1MinSize Then Return split.Panel1MinSize
        Return Math.Max(split.Panel1MinSize, Math.Min(wanted, max))
    End Function

    Private Sub ClearAll()
        _requestedCod = Nothing
        _notes = Nothing
        _nodeNotes = Nothing
        _selected = Nothing
        _resolvedPath = Nothing
        _resolvedFor = 0
        tree.Clear()
        _leaves.Clear()
        PushToActivePage()
    End Sub

    Private Sub ShowEmpty(message As String)
        lblEmpty.Text = message
        lblEmpty.Visible = True
        split.Visible = False
    End Sub

    Private Sub ShowContent()
        lblEmpty.Visible = False
        split.Visible = True
    End Sub

    Private Shared Function Money(value As Decimal) As String
        Return value.ToString("N2", RoCulture)
    End Function

    Private Shared Function MonthLabel(month As Integer) As String
        If month < 1 OrElse month > 12 Then Return CStr(month)
        Dim name As String = RoCulture.DateTimeFormat.GetMonthName(month)
        Return Char.ToUpper(name(0), RoCulture) & name.Substring(1)
    End Function

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            split.BackColor = p.SurfaceAltColor
            split.Panel1.BackColor = p.SurfaceAltColor
            split.Panel2.BackColor = p.SurfaceAltColor
            pnlPages.BackColor = p.SurfaceAltColor
            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor
            For Each page As ICabNotePage In _pages.Values
                TryCast(page, IThemedControl)?.ApplyTheme(scheme)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("NoteCabView.ApplyTheme", ex)
        End Try
    End Sub

End Class

''' <summary>What a node of the notes tree covers. POCO.</summary>
Friend NotInheritable Class CabNoteNodePayload
    Public ReadOnly Property Notes As List(Of CabCorrectionNote)
    ''' <summary>The note of a leaf; Nothing on a month.</summary>
    Public ReadOnly Property Note As CabCorrectionNote

    Public Sub New(notes As List(Of CabCorrectionNote), note As CabCorrectionNote)
        Me.Notes = If(notes, New List(Of CabCorrectionNote)())
        Me.Note = note
    End Sub
End Class
