Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 0088 -- the CAB correction notes.
''' <list type="bullet">
''' <item>After a FOREXE login that stored NEW «ERRRRRRRRRR» operations, <see cref="CabNoteForm"/>
''' opens straight away on exactly those (KbotForm.UncorrectedOperations.vb).</item>
''' <item>The menu entry «Operațiuni necorelate» opens it on EVERY stored ERR operation no note
''' covers yet. The entry is hidden when there is none; when there are, it and the «Meniu» button
''' carry a «(!)» mark.</item>
''' <item><see cref="UploadCabNoteAsync"/> is the one «upload into CAB» step, shared by the window
''' and the «Note corecție» view.</item>
''' </list>
''' </summary>
Partial Public Class KbotForm

    Private Const CabNoteCaption As String = "Nota de corecție CAB"
    Private Const UncorrelatedMenuKey As String = "operatiuni_necorelate"
    Private Const MenuButtonText As String = "  Meniu"
    Private Const MenuButtonMarkedText As String = "  Meniu  (!)"

    ' True while uncorrelated operations exist (the «(!)» mark); ApplyMenuButtonText reads it.
    Private _menuMarked As Boolean

    ' Re-entry guard: a login and a menu click must not open two windows.
    Private _uncorrelatedOpen As Boolean

    Private Sub OpenUncorrelatedFromMenu()
        ShowUncorrelatedFireAndForget()
    End Sub

    ' UI boundary: the menu handler is synchronous.
    Private Async Sub ShowUncorrelatedFireAndForget()
        Try
            Await ShowUncorrelatedAsync(onlyIds:=Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ShowUncorrelatedFireAndForget", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Reads the uncorrelated ERR operations from the database, refreshes the menu mark and opens
    ''' the note window on them -- on all of them (<paramref name="onlyIds"/> Nothing, the menu), or
    ''' only on the given FX_Operatiuni ids (the rows a login has just stored). Never throws.
    ''' </summary>
    ''' <summary>
    ''' Slice 0097: the «notecab» entry shows only when the angajament has notes. A note just saved
    ''' on the selected angajament raises the flag LOCALLY (as ORD / DDF do after their first
    ''' write) -- a tree reload would drop the selection.
    ''' </summary>
    Private Sub AprindePoartaNotelor(saved As IReadOnlyList(Of CabCorrectionNote))
        Try
            If _currentInfo Is Nothing OrElse _currentInfo.AreNoteCab OrElse saved Is Nothing Then Return
            Dim cod As String = _currentInfo.CodAngajament
            Dim atinge As Boolean = saved.Any(Function(n) n IsNot Nothing AndAlso n.Corrections.Any(
                Function(c) String.Equals(c.CommitmentCode, cod, StringComparison.OrdinalIgnoreCase)))
            If Not atinge Then Return
            _currentInfo.AreNoteCab = True
            ApplyViewGating(_currentInfo)
        Catch ex As Exception
            ' The notes are saved; a gate that stays shut until the next reload is not worth a throw.
            GlobalErrorLog.Write("MainForm.AprindePoartaNotelor", ex)
        End Try
    End Sub

    Private Async Function ShowUncorrelatedAsync(onlyIds As List(Of Integer)) As Task
        Try
            If _uncorrelatedOpen Then Return
            If onlyIds IsNot Nothing AndAlso onlyIds.Count = 0 Then
                Await RefreshUncorrelatedMarkAsync()
                Return
            End If
            Dim opsApi As IUncorrectedOperationsApi = TryCast(_apiClient, IUncorrectedOperationsApi)
            Dim notesApi As ICabNotesApi = TryCast(_apiClient, ICabNotesApi)
            If opsApi Is Nothing OrElse notesApi Is Nothing Then Throw New InvalidOperationException("The API client does not implement the slice 0088 interfaces.")

            Dim all As List(Of UncorrectedOperation) = Await WithReauth(
                Function() opsApi.GetUncorrelatedOperationsAsync(CancellationToken.None))
            MarkUncorrelated(all.Count)
            Dim shown As List(Of UncorrectedOperation) =
                If(onlyIds Is Nothing, all, all.Where(Function(o) onlyIds.Contains(o.IdFxp)).ToList())
            If shown.Count = 0 Then Return
            If IsDisposed OrElse Disposing Then Return

            Dim prep As CabNotePreparation = Await WithReauth(
                Function() notesApi.GetCabNotePreparationAsync(CancellationToken.None))

            _uncorrelatedOpen = True
            Try
                Using f As New CabNoteForm(shown, prep, notesApi, _session.NumeUnitate, _session.CF,
                                           AddressOf UploadCabNoteAsync)
                    f.ShowDialog(Me)
                    If f.SavedNotes.Count > 0 Then
                        TryCast(_activeView, NoteCabView)?.Reincarca()
                        AprindePoartaNotelor(f.SavedNotes)
                    End If
                End Using
            Finally
                _uncorrelatedOpen = False
            End Try
            Await RefreshUncorrelatedMarkAsync()
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.ShowUncorrelatedAsync", ex)
            KBotMessage.Show(Me, "Operațiunile necorelate nu pot fi citite: " & ex.Message,
                             CabNoteCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ShowUncorrelatedAsync", ex)
            KBotMessage.Show(Me, "Operațiunile necorelate nu pot fi citite. Detalii în jurnalul de erori.",
                             CabNoteCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Function

    ''' <summary>
    ''' Counts the uncorrelated ERR operations and shows / hides the menu entry and the «(!)» mark.
    ''' Never throws: a count that cannot be read leaves the mark as it was (logged).
    ''' </summary>
    Private Async Function RefreshUncorrelatedMarkAsync() As Task
        Try
            Dim opsApi As IUncorrectedOperationsApi = TryCast(_apiClient, IUncorrectedOperationsApi)
            If opsApi Is Nothing Then Return
            Dim all As List(Of UncorrectedOperation) = Await WithReauth(
                Function() opsApi.GetUncorrelatedOperationsAsync(CancellationToken.None))
            MarkUncorrelated(all.Count)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.RefreshUncorrelatedMarkAsync", ex)
        End Try
    End Function

    Private Sub MarkUncorrelated(count As Integer)
        If IsDisposed Then Return
        Dim item As KBotMenuItem = menuNou.Items.FirstOrDefault(Function(i) String.Equals(i.Key, UncorrelatedMenuKey, StringComparison.Ordinal))
        If item Is Nothing Then Throw New InvalidOperationException($"The menu has no «{UncorrelatedMenuKey}» entry (KbotForm.Designer.vb).")
        item.Visible = count > 0
        item.Text = $"<b>(!) Operațiuni necorelate ({count})</b>"
        Dim palette As ThemePalette = ThemeManager.Current?.Palette
        item.ForeColor = If(palette Is Nothing, Color.Empty, palette.ErrorColor)
        _menuMarked = count > 0
        ApplyMenuButtonText()
        tips.SetToolTipText(btnMeniu, If(count > 0,
            $"Există {count} operațiuni «ERRRRRRRRRR» din FOREXE necorelate cu un angajament: Meniu → «Operațiuni necorelate».",
            "Angajament nou, clasificațiile bugetare și partenerii."))
    End Sub

    ''' <summary>
    ''' «Vrei să încarci NOTA DE CORECȚIE în CAB?» -- on OK, uploads <paramref name="pdfPath"/> into
    ''' FOREXE («Transmitere documente electronice»), records it on the note and shows FOREXE's
    ''' answer. True when the upload went through. Never throws (UI step): every failure is told.
    ''' </summary>
    Friend Async Function UploadCabNoteAsync(owner As IWin32Window, note As CabCorrectionNote, pdfPath As String) As Task(Of Boolean)
        Try
            If note Is Nothing Then Return False
            If String.IsNullOrWhiteSpace(pdfPath) OrElse Not File.Exists(pdfPath) Then
                KBotMessage.Show(owner, "PDF-ul notei nu există pe acest calculator; deschideți pagina «Document» a notei ca să-l generați.",
                                 CabNoteCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            Dim question As String = "Vrei să încarci NOTA DE CORECȚIE în CAB?" & Environment.NewLine & Environment.NewLine &
                                     $"Nota nr. {note.NoteNumber} din {CabCorrectionNoteRules.FormDate(note.NoteDate)} — " &
                                     $"{note.Corrections.Count} operațiuni, {note.Total:N2} lei"
            If note.Sent Then question &= Environment.NewLine & Environment.NewLine & "Atenție: nota a mai fost trimisă o dată în FOREXE."
            If Not CabNoteFiles.IsSigned(pdfPath) Then
                question &= Environment.NewLine & Environment.NewLine & "Atenție: documentul NU este semnat; FOREXE îl poate refuza."
            End If
            If KBotMessage.Show(owner, question, CabNoteCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) <> DialogResult.OK Then
                Return False
            End If

            ' Slice 0088-04 (operator): success or failure is said in CabNoteReceiptForm, which also
            ' offers «Verifică recipisa».
            Dim answer As String
            Try
                answer = Await _robotQueue.RunWithResultAsync(Of String)(
                    Nothing, $"Încărcare notă CAB nr. {note.NoteNumber}",
                    Function() _controller.TrimiteDocumentAsync(pdfPath))
            Catch ex As Exception
                ' Already logged by the coordinator; the message is Romanian.
                OperatorLog.Write("MainForm.UploadCabNoteAsync", CabNoteCaption, ex.Message, KBotLogLevel.Error)
                ShowReceiptWindow(owner, note, $"Nota nr. {note.NoteNumber} NU a fost încărcată în CAB.",
                                  ex.Message, isError:=True, index:=note.RegistrationIndex)
                Return False
            End Try
            If answer Is Nothing Then
                Dim reason As String = If(String.IsNullOrEmpty(_controller.LastFailure), "Sesiunea FOREXE nu s-a deschis.", _controller.LastFailure)
                OperatorLog.Write("MainForm.UploadCabNoteAsync", CabNoteCaption, reason, KBotLogLevel.Warn)
                ShowReceiptWindow(owner, note, $"Nota nr. {note.NoteNumber} NU a fost încărcată în CAB.",
                                  reason, isError:=True, index:=note.RegistrationIndex)
                Return False
            End If

            Dim index As String = CabCorrectionNoteRules.RegistrationIndexFrom(answer)
            Dim headline As String = $"Nota nr. {note.NoteNumber} a fost trimisă în FOREXE."
            Dim details As String = answer
            Try
                Dim api As ICabNotesApi = TryCast(_apiClient, ICabNotesApi)
                If api Is Nothing Then Throw New InvalidOperationException("The API client does not implement ICabNotesApi.")
                Await WithReauth(Function() MarkSentAsync(api, note.IdNc, answer, index))
                note.Sent = True
                note.SentAt = DateTime.Now
                note.SentAnswer = answer
                If index.Length > 0 Then note.RegistrationIndex = index
            Catch ex As Exception
                GlobalErrorLog.Write("MainForm.UploadCabNoteAsync.Mark", ex)
                details = answer & Environment.NewLine & Environment.NewLine &
                          "⚠ Trimiterea NU a putut fi notată în K-BOT: " & ex.Message
            End Try
            OperatorLog.Write("MainForm.UploadCabNoteAsync", CabNoteCaption, headline & " " & details, KBotLogLevel.Info)
            ShowReceiptWindow(owner, note, headline, details, isError:=False, index:=index)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.UploadCabNoteAsync", ex)
            KBotMessage.Show(owner, "Încărcarea notei în CAB s-a oprit. Detalii în jurnalul de erori.",
                             CabNoteCaption, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ' WithReauth wants a result; the mark has none.
    Private Shared Async Function MarkSentAsync(api As ICabNotesApi, idNc As Integer, answer As String, index As String) As Task(Of Boolean)
        Await api.MarkCabNoteSentAsync(idNc, answer, index, CancellationToken.None)
        Return True
    End Function

    ' ── Receipt (slice 0088-04) ───────────────────────────────────────────────

    ''' <summary>
    ''' The «Recipisă» page's «Validează documentul»: the receipt window for a note that has none on
    ''' the server yet. True when a receipt was stored. Never throws (UI step).
    ''' </summary>
    Friend Function OpenReceiptWindow(owner As IWin32Window, note As CabCorrectionNote) As Boolean
        Try
            If note Is Nothing Then Return False
            Dim headline As String = If(note.Sent,
                $"Nota nr. {note.NoteNumber} a fost trimisă în CAB" &
                    If(note.SentAt.HasValue, $" pe {note.SentAt.Value:dd.MM.yyyy HH:mm}", String.Empty) & "; recipisa nu este încă în K-BOT.",
                $"Nota nr. {note.NoteNumber} nu apare ca trimisă în CAB din K-BOT.")
            Dim details As String = If(String.IsNullOrWhiteSpace(note.SentAnswer), String.Empty, note.SentAnswer)
            Dim index As String = If(String.IsNullOrWhiteSpace(note.RegistrationIndex),
                                     CabCorrectionNoteRules.RegistrationIndexFrom(note.SentAnswer), note.RegistrationIndex)
            Return ShowReceiptWindow(owner, note, headline, details, isError:=Not note.Sent, index:=index) IsNot Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.OpenReceiptWindow", ex)
            KBotMessage.Show(owner, "Fereastra recipisei nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             CabNoteCaption, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ' The window; returns the receipt it stored (Nothing = none). The caller wraps.
    Private Function ShowReceiptWindow(owner As IWin32Window, note As CabCorrectionNote, headline As String,
                                       details As String, isError As Boolean, index As String) As CabNoteReceipt
        Using f As New CabNoteReceiptForm(note, headline, details, isError, index,
                                          Function(i) VerifyReceiptAsync(note, i))
            f.ShowDialog(If(owner, Me))
            If f.SavedReceipt IsNot Nothing Then
                note.Receipt = f.SavedReceipt
                note.RegistrationIndex = f.SavedReceipt.RegistrationIndex
                TryCast(_activeView, NoteCabView)?.ReceiptSaved(note)
            End If
            Return f.SavedReceipt
        End Using
    End Function

    ''' <summary>
    ''' «Verifică recipisa»: FOREXE's SNM inbox -> the receipt of <paramref name="index"/> -> the
    ''' server (on the note's PDF) -> a copy on this computer. Never throws: every outcome is a line.
    ''' </summary>
    Private Async Function VerifyReceiptAsync(note As CabCorrectionNote, index As String) As Task(Of CabReceiptCheck)
        Try
            Dim found As ForexeReceipt
            Try
                found = Await _robotQueue.RunWithResultAsync(Of ForexeReceipt)(
                    Nothing, $"Căutare recipisă {index}",
                    Function() _controller.CautaRecipisaAsync(index))
            Catch ex As Exception
                ' Logged by the coordinator; the message is Romanian.
                Return New CabReceiptCheck With {.Message = ex.Message}
            End Try
            If found Is Nothing Then
                Return New CabReceiptCheck With {
                    .Message = "Recipisa nu a fost căutată: " &
                               If(String.IsNullOrEmpty(_controller.LastFailure), "sesiunea FOREXE nu s-a deschis.", _controller.LastFailure)}
            End If
            If Not found.Found Then
                Return New CabReceiptCheck With {
                    .Message = $"Recipisa pentru indexul {index} nu este încă în FOREXE ({found.MessagesRead} mesaje citite). " &
                               "Încercați din nou peste câteva minute."}
            End If

            Dim api As ICabNotesApi = TryCast(_apiClient, ICabNotesApi)
            If api Is Nothing Then Throw New InvalidOperationException("The API client does not implement ICabNotesApi.")
            Dim receipt As New CabNoteReceipt With {
                .RegistrationIndex = index,
                .RegistrationNumber = found.RegistrationNumber,
                .MessageId = found.MessageId,
                .MessageText = found.MessageText,
                .MessageDate = found.MessageDate,
                .FileName = If(String.IsNullOrWhiteSpace(found.FileName), $"RECIPISA_{index}.pdf", found.FileName)}

            ' The copy on this computer first: a server that refuses still leaves the operator the file.
            Dim local As String = CabNoteFiles.ReceiptPath(note, index, receipt.FileName)
            CabNoteFiles.WriteReceipt(local, found.Content)

            Dim saved As CabNoteReceipt
            Try
                saved = Await WithReauth(Function() api.SaveCabNoteReceiptAsync(note.IdNc, receipt, found.Content, CancellationToken.None))
            Catch ex As ApiException
                GlobalErrorLog.Write("MainForm.VerifyReceiptAsync.Save", ex)
                Return New CabReceiptCheck With {
                    .Message = $"Recipisa «{receipt.FileName}» a fost descărcată în «{local}», dar NU a putut fi salvată pe server: {ex.Message}",
                    .LocalPath = local}
            End Try
            Return New CabReceiptCheck With {
                .Found = True, .Receipt = saved, .LocalPath = local,
                .Message = $"Recipisa «{saved.FileName}» ({saved.RegistrationNumber}) a fost descărcată și salvată pe server."}
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.VerifyReceiptAsync", ex)
            Return New CabReceiptCheck With {.Message = "Recipisa nu a putut fi salvată. Detalii în jurnalul de erori."}
        End Try
    End Function

End Class
