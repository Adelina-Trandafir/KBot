Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' The preview pane of the DDF editor's «Fisiere» page. It is handed the attachment the operator
''' selected in the grid and shows it, or says plainly why it cannot.
'''
''' <para><b>Four kinds are shown, everything else is refused by name.</b> Images are drawn by
''' WinForms, Excel and Word documents are hosted through <see cref="OfficeDocumentHost"/>, PDFs
''' through the Adobe host the DDF view already uses. Anything else gets
''' «Tipul fișierului nu e suportat de K-BOT.» — a sentence, not an empty rectangle, because an
''' operator staring at a blank pane has no way to tell "unsupported" from "broken".</para>
'''
''' <para><b>The bytes come from the draft, so a temporary file has to exist.</b> Excel, Word and
''' Adobe all open PATHS, not arrays. Each preview writes one file under
''' <c>%TEMP%\KBOT\previzualizare\</c> and the previous one is deleted when the next is written.
''' Deletion is best-effort on purpose: Office can still be letting go of its handle, and a leftover
''' temporary file is a far smaller problem than an exception thrown at a preview.</para>
'''
''' <para><b>One panel, one host.</b> Office and Adobe both reparent a foreign top-level window into
''' <c>pnlGazda</c>. Whichever is not wanted now is released FIRST, so there is no way to leave a
''' stale Excel window sitting behind an Adobe one.</para>
'''
''' <para><b>Nothing here writes to the draft.</b> This pane only reads; adding, removing and saving
''' stay on the three buttons of the page.</para>
''' </summary>
Public Class DdfFisierPreview
    Implements IThemedControl

    ''' <summary>What the pane knows how to show. Never persisted, never sent anywhere.</summary>
    Private Enum TipPreviz
        Nesuportat = 0
        Imagine = 1
        Pdf = 2
        Excel = 3
        Word = 4
    End Enum

    ''' <summary>The sentence the operator gets for everything the pane cannot draw.</summary>
    Private Const MESAJ_NESUPORTAT As String = "Tipul fișierului nu e suportat de K-BOT."

    Private Shared ReadOnly EXT_IMAGINE As String() =
        {".bmp", ".jpg", ".jpeg", ".png", ".gif", ".ico", ".tif", ".tiff"}
    Private Shared ReadOnly EXT_EXCEL As String() =
        {".xls", ".xlsx", ".xlsm", ".xlsb", ".csv"}
    Private Shared ReadOnly EXT_WORD As String() =
        {".doc", ".docx", ".docm", ".rtf"}

    ''' <summary>Which of the two window hosts owns <c>pnlGazda</c> right now.</summary>
    Private Enum DdfContainer
        Niciunul = 0
        Office = 1
        Adobe = 2
    End Enum

    ' Both hosts are created LAZILY: an operator who never selects a PDF never loads the Adobe
    ' machinery, and one who never selects a spreadsheet never starts an Excel process.
    Private _office As OfficeDocumentHost
    Private _adobe As AdobeReaderHost
    ''' <summary>The host currently holding the panel. See <see cref="InchideContainerele"/>.</summary>
    Private _container As DdfContainer = DdfContainer.Niciunul

    ''' <summary>The folder holding the file currently being previewed. Deleted when the next one
    ''' is written and when the control is disposed.</summary>
    Private _folderTemp As String
    ''' <summary>Folders a viewer was still holding when we tried to delete them. Retried at every
    ''' preview — see <see cref="StergeTemporarele"/>.</summary>
    Private ReadOnly _temporareRamase As New List(Of String)()
    ''' <summary>The path last asked for — the stale-answer guard on the asynchronous Adobe path.</summary>
    Private _caleCeruta As String
    ''' <summary>The attachment key on screen, so a grid that re-raises its selection event for the
    ''' same row does not re-open the same workbook.</summary>
    Private _cheieCurenta As Integer = -1
    ''' <summary>Set while a preview is being opened. See <see cref="OcupatChanged"/>.</summary>
    Private _ocupat As Boolean

    ''' <summary>
    ''' Raised when the pane starts and finishes opening a document. The page turns the grid off
    ''' while this is True: opening Excel takes a second or two, and a row clicked during it would
    ''' otherwise queue up and be delivered the moment the pane is free again — one operator click
    ''' per second turning into a queue of Office instances.
    ''' </summary>
    Public Event OcupatChanged(ocupat As Boolean)

    Public Sub New()
        InitializeComponent()
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' What the page calls
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Shows one attachment. <paramref name="cheie"/> is the draft key
    ''' (<c>DdfDraftAtt.Cheie</c>): the same key twice in a row is a no-op, which is what keeps a
    ''' grid that re-raises its selection event from re-opening the document.
    ''' </summary>
    Public Sub ShowAttachment(cheie As Integer, numeFisier As String, continut As Byte())
        ' Belt and braces with the grid the page disables: the flag also covers whatever else could
        ' reach this method while a document is opening.
        If _ocupat Then Return
        If cheie = _cheieCurenta Then Return

        ' True once the asynchronous PDF path has taken over. It owns the busy flag from then on --
        ' releasing it here would let the next click in before Adobe has finished embedding.
        Dim preluatAsincron As Boolean = False
        SeteazaOcupat(True)
        Try
            _cheieCurenta = cheie
            lblTitlu.Text = If(numeFisier, String.Empty)

            Dim tip As TipPreviz = TipDupaExtensie(numeFisier)
            If continut Is Nothing OrElse continut.Length = 0 Then tip = TipPreviz.Nesuportat

            ' THE OLD CONTAINER GOES FIRST, and it is CLOSED, not dropped for the collector to find.
            ' Only the host the new file actually needs survives, and even that one has its current
            ' document closed before the next is opened.
            InchideContainerele(ContainerulPentru(tip))

            If continut Is Nothing OrElse continut.Length = 0 Then
                ' The same wording the «Salvează pe disc» button uses for this case: the bytes
                ' are fetched when the document opens, and a failure there was already reported.
                ArataMesaj("Conținutul fișierului nu este disponibil. Închide și redeschide documentul.")
                Return
            End If

            Select Case tip
                Case TipPreviz.Nesuportat
                    ArataMesaj(MESAJ_NESUPORTAT)
                Case TipPreviz.Imagine
                    ArataImaginea(continut)
                Case TipPreviz.Pdf
                    _caleCeruta = ScrieFisierulTemporar(numeFisier, continut)
                    preluatAsincron = True
                    ArataPdf(_caleCeruta)
                Case Else
                    _caleCeruta = ScrieFisierulTemporar(numeFisier, continut)
                    ArataOffice(_caleCeruta, If(tip = TipPreviz.Excel, OfficeDocumentKind.Excel, OfficeDocumentKind.Word))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFisierPreview.ShowAttachment", ex)
            ' Also in the working log, and spelled out: whoever is reading office_preview.log to
            ' find out why a preview failed should not have to open a second file for the reason.
            OfficeHostLog.Write($"Preview of «{numeFisier}» failed: {OfficeHostLog.Describe(ex)}")
            ArataMesaj("Fișierul nu a putut fi previzualizat. Detalii în jurnalul de erori.")
        Finally
            If Not preluatAsincron Then SeteazaOcupat(False)
        End Try
    End Sub

    ''' <summary>Empties the pane: nothing selected, or the selected row was removed.</summary>
    Public Sub Clear()
        Try
            _cheieCurenta = -1
            _caleCeruta = Nothing
            lblTitlu.Text = String.Empty
            InchideContainerele(DdfContainer.Niciunul)
            ArataMesaj("Selectează un fișier din listă.")
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFisierPreview.Clear", ex)
        End Try
    End Sub

    ''' <summary>Flips the busy flag and tells the page, so the grid follows it exactly.</summary>
    Private Sub SeteazaOcupat(valoare As Boolean)
        If _ocupat = valoare Then Return
        _ocupat = valoare
        RaiseEvent OcupatChanged(valoare)
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The three ways of showing something
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Draws an image. The bytes are copied into a bitmap of our own rather than handed to
    ''' <c>Image.FromStream</c> directly: that image keeps the stream alive for as long as it lives,
    ''' and this one has to outlive the array it came from.
    ''' </summary>
    Private Sub ArataImaginea(continut As Byte())
        Dim veche As Image = picImagine.Image
        Try
            Using ms As New MemoryStream(continut, writable:=False)
                Using original As Image = Image.FromStream(ms)
                    picImagine.Image = New Bitmap(original)
                End Using
            End Using
            ArataSuprafata(picImagine)
        Catch ex As ArgumentException
            ' What GDI+ throws for bytes that are not an image it recognises -- a .png that is
            ' really something else, or a truncated upload. Not an error worth a log entry beyond
            ' the sink: the operator gets the same answer as for any unsupported file.
            GlobalErrorLog.Write("DdfFisierPreview.ArataImaginea", ex)
            picImagine.Image = Nothing
            ArataMesaj(MESAJ_NESUPORTAT)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFisierPreview.ArataImaginea", ex)
            picImagine.Image = Nothing
            ArataMesaj("Imaginea nu a putut fi afișată. Detalii în jurnalul de erori.")
        Finally
            ' The previous bitmap is released only after the new one is in place, so a failure
            ' halfway cannot leave the box pointing at a disposed image.
            If veche IsNot Nothing AndAlso Not ReferenceEquals(veche, picImagine.Image) Then veche.Dispose()
        End Try
    End Sub

    ''' <summary>
    ''' Hosts an Excel or Word document. This BLOCKS while Office starts, so the waiting message is
    ''' painted before the call rather than merely assigned — an unpainted label is exactly the blank
    ''' pane this control exists to avoid.
    ''' </summary>
    Private Sub ArataOffice(cale As String, fel As OfficeDocumentKind)
        ArataMesaj("Se deschide documentul…")
        lblMesaj.Refresh()

        Dim gazda As OfficeDocumentHost = AsiguraOffice()
        If fel = OfficeDocumentKind.Excel Then gazda.ExcelRibbon = ExcelRibbonMode.HideDockWindow
        Dim rezultat As OfficeHostResult = gazda.ShowDocument(cale, fel)
        If rezultat.Succeeded Then
            ArataSuprafata(pnlGazda)
        Else
            ArataMesaj(rezultat.Message)
        End If
    End Sub

    ''' <summary>Hosts a PDF through the same Adobe host the DDF view uses.</summary>
    Private Sub ArataPdf(cale As String)
        ArataMesaj("Se deschide documentul…")
        lblMesaj.Refresh()
        ' Fire-and-forget deliberate: the method handles all of its own failures, and the caller is
        ' a synchronous handler with nobody to await it (the same shape as ReaderHostPreview).
        ArataPdfAsync(cale)
    End Sub

    ' Asynchronous UI boundary: logs and SWALLOWS. Every failure path ends in a Romanian sentence.
    ' It also OWNS the busy flag from the moment ShowAttachment hands over -- the Finally below is
    ' the only thing that lets the grid back on, so it must not be able to be skipped.
    Private Async Sub ArataPdfAsync(cale As String)
        Try
            Dim gazda As AdobeReaderHost = AsiguraAdobe()
            Dim rezultat As AdobeHostResult = Await gazda.ShowDocumentAsync(cale).ConfigureAwait(True)

            ' The operator picked another file in the meantime: this answer is stale.
            If Not String.Equals(_caleCeruta, cale, StringComparison.Ordinal) Then Return

            Select Case rezultat.Status
                Case AdobeHostStatus.Hosted
                    ArataSuprafata(pnlGazda)
                Case AdobeHostStatus.Superseded
                    ' A newer request took over; it owns the pane now.
                Case Else
                    ArataMesaj(rezultat.Message)
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFisierPreview.ArataPdfAsync", ex)
            ArataMesaj("Documentul nu a putut fi afișat. Detalii în jurnalul de erori.")
        Finally
            SeteazaOcupat(False)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Hosts and temporary files
    ' ══════════════════════════════════════════════════════════════════════════

    Private Function AsiguraOffice() As OfficeDocumentHost
        If _office Is Nothing Then _office = New OfficeDocumentHost(pnlGazda, AddressOf OfficeHostLog.Write)
        Return _office
    End Function

    ''' <summary>
    ''' The Adobe host, under the operator's own viewer settings — the same ones
    ''' <see cref="ReaderHostPreview"/> reads, so a profile that was tuned for the DDF view holds
    ''' here too. A broken setting falls back to «Automat» and says so in the working log.
    ''' </summary>
    Private Function AsiguraAdobe() As AdobeReaderHost
        If _adobe IsNot Nothing Then Return _adobe

        _adobe = New AdobeReaderHost(pnlGazda, AddressOf AdobeHostLog.Write) With {.PopupWatchEnabled = True}
        _adobe.Options.DetachMode = AdobeDetachMode.KillProcess

        Dim mod_ As AdobeSettingRead(Of AdobeViewerMode) = AdobeViewerSettings.CurrentMode()
        Dim instanta As AdobeSettingRead(Of AdobeNewInstanceMode) = AdobeViewerSettings.CurrentNewInstance()
        If mod_.HasWarning Then AdobeHostLog.Write("ATENȚIE: " & mod_.Warning)
        If instanta.HasWarning Then AdobeHostLog.Write("ATENȚIE: " & instanta.Warning)
        _adobe.Mode = mod_.Value
        _adobe.NewInstanceMode = instanta.Value
        Return _adobe
    End Function

    ''' <summary>Which host a file of that kind needs, if any.</summary>
    Private Shared Function ContainerulPentru(tip As TipPreviz) As DdfContainer
        Select Case tip
            Case TipPreviz.Pdf : Return DdfContainer.Adobe
            Case TipPreviz.Excel, TipPreviz.Word : Return DdfContainer.Office
            Case Else : Return DdfContainer.Niciunul
        End Select
    End Function

    ''' <summary>
    ''' Closes every container except <paramref name="pastreaza"/>, and closes the document inside
    ''' that one too. Called before EVERY preview and from <c>Dispose</c>.
    '''
    ''' <para><b>Closed, not dropped.</b> An unwanted host is disposed here and its field cleared —
    ''' nothing is left for the garbage collector to notice at some later point. That matters more
    ''' than tidiness: the hosted window is a child of <c>pnlGazda</c>, so an Office process still
    ''' holding one when the panel goes away is left running with nothing left to reach it by.</para>
    '''
    ''' <para><b>The kept host still lets go of its document.</b> Two spreadsheets in a row reuse the
    ''' same <see cref="OfficeDocumentHost"/>, but the first workbook is closed and its instance quit
    ''' before the second is opened — never two Excels at once.</para>
    ''' </summary>
    Private Sub InchideContainerele(pastreaza As DdfContainer)
        Try
            If _office IsNot Nothing Then
                If pastreaza = DdfContainer.Office Then
                    _office.Detach()
                Else
                    _office.Dispose()
                    _office = Nothing
                End If
            End If

            If _adobe IsNot Nothing Then
                If pastreaza = DdfContainer.Adobe Then
                    _adobe.Detach()
                Else
                    _adobe.Dispose()
                    _adobe = Nothing
                End If
            End If

            _container = pastreaza
            ' Only AFTER every host has let go: deleting a file Excel still has open cannot succeed.
            StergeTemporarele()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFisierPreview.InchideContainerele", ex)
        End Try
    End Sub

    ''' <summary>Everything goes. Called from <c>Dispose</c> (see the Designer).</summary>
    Friend Sub EliberezaGazdele()
        InchideContainerele(DdfContainer.Niciunul)
    End Sub

    ''' <summary>
    ''' Writes the bytes to a fresh folder under <c>%TEMP%\KBOT\previzualizare\</c> and removes the
    ''' previous one. The file keeps its own name -- Office and Adobe both read the extension, and
    ''' the operator sees the name in a window title.
    ''' </summary>
    Private Function ScrieFisierulTemporar(numeFisier As String, continut As Byte()) As String
        Try
            ' The previous folder is already gone: EliberezaGazdele ran before this, and it deletes
            ' it once both hosts have released their handles.
            _folderTemp = Path.Combine(Path.GetTempPath(), "KBOT", "previzualizare",
                                       Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(_folderTemp)

            Dim cale As String = Path.Combine(_folderTemp, NumeSigur(numeFisier))
            File.WriteAllBytes(cale, continut)
            Return cale
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFisierPreview.ScrieFisierulTemporar", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Deletes the folder of the previous preview, and retries the ones that would not go before.
    '''
    ''' <para>BEST EFFORT, and it has to be: a viewer that has just been told to quit can still hold
    ''' its file open for a moment, which is exactly what the working log showed for every PDF
    ''' («being used by another process»). Throwing at a preview because a temporary file is busy
    ''' would be absurd — but so is walking away from it, so the failures are kept and tried again
    ''' at the next preview, by which time the process is long gone.</para>
    ''' </summary>
    Private Sub StergeTemporarele()
        If Not String.IsNullOrEmpty(_folderTemp) Then
            _temporareRamase.Add(_folderTemp)
            _folderTemp = Nothing
        End If

        For i As Integer = _temporareRamase.Count - 1 To 0 Step -1
            If IncearcaStergerea(_temporareRamase(i)) Then _temporareRamase.RemoveAt(i)
        Next
    End Sub

    ''' <summary>One delete attempt. True when the folder is gone (or was never there).</summary>
    Private Shared Function IncearcaStergerea(folder As String) As Boolean
        Try
            If Not Directory.Exists(folder) Then Return True
            Directory.Delete(folder, recursive:=True)
            Return True
        Catch ex As IOException
            OfficeHostLog.Write($"Temporary folder still locked, will retry: {folder} ({ex.Message})")
            Return False
        Catch ex As UnauthorizedAccessException
            OfficeHostLog.Write($"Temporary folder still locked, will retry: {folder} ({ex.Message})")
            Return False
        End Try
    End Function

    ''' <summary>The file name with anything Windows refuses replaced by an underscore.</summary>
    Private Shared Function NumeSigur(numeFisier As String) As String
        Dim nume As String = Path.GetFileName(If(numeFisier, String.Empty)).Trim()
        If nume.Length = 0 Then Return "fisier"
        For Each c As Char In Path.GetInvalidFileNameChars()
            nume = nume.Replace(c, "_"c)
        Next
        Return nume
    End Function

    ''' <summary>Which of the four kinds an extension names. Unknown extensions are not guessed at:
    ''' the file's own bytes are never sniffed, because opening a mislabelled file in Excel is a
    ''' worse outcome than telling the operator the type is not supported.</summary>
    Private Shared Function TipDupaExtensie(numeFisier As String) As TipPreviz
        Dim ext As String = Path.GetExtension(If(numeFisier, String.Empty)).ToLowerInvariant()
        If ext.Length = 0 Then Return TipPreviz.Nesuportat
        If ext = ".pdf" Then Return TipPreviz.Pdf
        If Array.IndexOf(EXT_IMAGINE, ext) >= 0 Then Return TipPreviz.Imagine
        If Array.IndexOf(EXT_EXCEL, ext) >= 0 Then Return TipPreviz.Excel
        If Array.IndexOf(EXT_WORD, ext) >= 0 Then Return TipPreviz.Word
        Return TipPreviz.Nesuportat
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' The three states
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Shows one of the surfaces and hides the other two. Every state setter touches ALL
    ''' three, so a new state cannot leave an old surface visible underneath it.</summary>
    Private Sub ArataSuprafata(activa As Control)
        pnlGazda.Visible = ReferenceEquals(activa, pnlGazda)
        picImagine.Visible = ReferenceEquals(activa, picImagine)
        lblMesaj.Visible = ReferenceEquals(activa, lblMesaj)
        activa.BringToFront()
    End Sub

    Private Sub ArataMesaj(mesaj As String)
        lblMesaj.Text = mesaj
        ArataSuprafata(lblMesaj)
    End Sub

    ''' <summary>Only the host that actually owns the panel is re-laid out; the other one is either
    ''' gone or has nothing embedded.</summary>
    Private Sub pnlGazda_SizeChanged(sender As Object, e As EventArgs) Handles pnlGazda.SizeChanged
        Try
            Select Case _container
                Case DdfContainer.Office : _office?.Relayout()
                Case DdfContainer.Adobe : _adobe?.Relayout()
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFisierPreview.pnlGazda_SizeChanged", ex)
        End Try
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            lblTitlu.BackColor = p.SurfaceAltColor
            lblTitlu.ForeColor = p.TextColor
            lblMesaj.BackColor = p.SurfaceAltColor
            lblMesaj.ForeColor = p.TextDimColor
            ' The host panel keeps the plain surface colour: what covers it is a foreign window, and
            ' the thin border around it should read as the document's edge, not as our own panel.
            pnlGazda.BackColor = p.SurfaceColor
            picImagine.BackColor = p.SurfaceColor
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFisierPreview.ApplyTheme", ex)
        End Try
    End Sub

End Class
