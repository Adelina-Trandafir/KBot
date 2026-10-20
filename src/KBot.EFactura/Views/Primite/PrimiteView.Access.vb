Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

' Slice 00EF-43 -- «Preia din Access» and «Trimite in Access» of the received invoices, for the clients that also run the Access application (the
' ribbon shows the two buttons only to them). The Access file is read and written by KBot.Access through the bridge (the PC); the K-BOT database is
' reached only through the server. Rows are recognised by their natural key, never by an Access number (server: primite_access.py).
'   - Preia: what the Migrator's E-Factura tab does for the received invoices (messages, invoices, lines, notes, ANAF messages, booked operations, the
'     credit-note references, the CPV of the lines, the automatic link to receptions), without the operator choosing files or ticks: the file is
'     <registry folder>\EFACTURA\ef_<year>.accdb, the unit's messages are those whose cui_unit is the unit's tax code.
'   - Trimite: the messages of the year that K-BOT has and the file does not, into EF, EFT, EFS, EFT_C and EFT_M (not EFT_O, the accounting).
' One thing at a time with the update from ANAF (the same guard).
Partial Public Class PrimiteView

    Private Const AccessCaptionPreia As String = "Preia din Access"
    Private Const AccessCaptionTrimite As String = "Trimite în Access"
    Private Const AccessReadBatch As Integer = 10
    Private Const AccessBatchBytes As Integer = 5 * 1024 * 1024
    Private Const AccessSendBatch As Integer = 20
    Private Const AccessShownProblems As Integer = 8
    Private Const AccessShownNotRead As Integer = 20

    ''' <summary>The bridge when the Access features may be used now; Nothing (after telling the operator) when not.</summary>
    Private Function AccessBridgeOrTell(k_caption As String) As IAccessBridge
        Dim k_bridge As IAccessBridge = AccessBridge.Instance
        If k_bridge Is Nothing OrElse Not AccessFeature.Enabled Then
            KBotMessage.Show(FindForm(), "Funcțiile Access nu sunt disponibile pe acest calculator.", k_caption,
                             MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return Nothing
        End If
        If _year <= 0 Then
            KBotMessage.Show(FindForm(), "Alegeți mai întâi anul în K-BOT: magazinul Access de facturi este pe an.", k_caption,
                             MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return Nothing
        End If
        Return k_bridge
    End Function

    ''' <summary>
    ''' «Preia din Access»: brings the unit's received invoices of the year from the Access store into K-BOT, batch by batch, then links the new ones to the
    ''' receptions that already exist where the match is certain. What K-BOT already has is left as it is, so it can be run again. One at a time.
    ''' UI boundary (called from the ribbon): a failure is logged and told to the operator, nothing is thrown.
    ''' </summary>
    Public Async Function PreiaDinAccessAsync() As Task
        Try
            If _api Is Nothing OrElse _syncing Then Return
            Dim k_bridge As IAccessBridge = AccessBridgeOrTell(AccessCaptionPreia)
            If k_bridge Is Nothing Then Return
            _syncing = True
            Try
                SetSyncStatus("Se caută magazinul de facturi din Access…", "Se caută în Access…")
                Dim k_store As String = k_bridge.ResolveEFacturaStore(AppSettings.Current.AccessRegistryPath, _year)
                Dim k_cui As String = Await _gate.RunAsync(Function() _api.GetAccessCuiAsync(_cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                Dim k_ids As List(Of String) = Await Task.Run(Function() k_bridge.ListEFacturaUnitMessageIds(k_store, k_cui)).ConfigureAwait(True)
                If IsDisposed Then Return
                If k_ids.Count = 0 Then
                    SetSyncStatus("În Access nu sunt mesaje ale unității.", FooterIdleText)
                    KBotMessage.Show(FindForm(), $"În magazinul Access «{k_store}» nu există mesaje ale unității (cod fiscal {k_cui}).", AccessCaptionPreia,
                                     MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return
                End If
                If KBotMessage.Show(FindForm(),
                                    $"În Access sunt {k_ids.Count.ToString(CultureInfo.CurrentCulture)} mesaje ale unității (cod fiscal {k_cui}), anul {_year}." & vbLf &
                                    "Se preiau în K-BOT cele care lipsesc, cu facturile, liniile, notele și operațiunile lor; ce există deja în K-BOT rămâne neschimbat." & vbLf & vbLf &
                                    "Continuați?", AccessCaptionPreia, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                    SetSyncStatus(String.Empty, FooterIdleText)
                    Return
                End If

                Dim k_total As New EFacturaAccessPreluare()
                Dim k_added As New List(Of Integer)()
                Dim k_references As New List(Of AccessEfReference)()
                Dim k_done As Integer
                ' Slice 00EF-44: what this PC saw, sent with every batch and with the end call so the server's journal can explain a short run.
                Dim k_context As New AccessPreluareContext() With {
                    .An = _year, .Fisier = Path.GetFileName(k_store), .Cui = k_cui, .AccesTotal = k_ids.Count,
                    .Loturi = (k_ids.Count + AccessReadBatch - 1) \ AccessReadBatch}
                For k_start As Integer = 0 To k_ids.Count - 1 Step AccessReadBatch
                    Dim k_slice As List(Of String) = k_ids.Skip(k_start).Take(AccessReadBatch).ToList()
                    Dim k_messages As List(Of AccessEfMessage) = Await Task.Run(Function() k_bridge.ReadEFacturaMessages(k_store, k_slice)).ConfigureAwait(True)
                    If IsDisposed Then Return
                    NoteSlice(k_context, k_slice, k_messages, k_start \ AccessReadBatch + 1)
                    For Each k_part As List(Of AccessEfMessage) In SplitBySize(k_messages)
                        Dim k_answer As EFacturaAccessPreluare = Await _gate.RunAsync(Function() _api.PreiaAccessAsync(k_part, k_context, _cts.Token)).ConfigureAwait(True)
                        If IsDisposed Then Return
                        AddTo(k_total, k_answer)
                        k_added.AddRange(k_answer.IdPrimite)
                    Next
                    For Each k_m As AccessEfMessage In k_messages
                        For Each k_i As AccessEfInvoice In k_m.Invoices
                            If k_i.RefNrFact.Length > 0 Then
                                k_references.Add(New AccessEfReference() With {
                                    .IdSol = k_m.IdSol, .NrFact = k_i.NrFact, .RefIdSol = k_i.RefIdSol, .RefNrFact = k_i.RefNrFact})
                            End If
                        Next
                    Next
                    k_done += k_slice.Count
                    Dim k_of As String = $"{k_done.ToString(CultureInfo.CurrentCulture)} din {k_ids.Count.ToString(CultureInfo.CurrentCulture)}"
                    SetSyncStatus("Se preiau mesajele din Access… " & k_of, "Se preiau… " & k_of)
                Next

                SetSyncStatus("Se leagă facturile noi de recepții…", "Se leagă…")
                Dim k_final As EFacturaAccessFinal = Await _gate.RunAsync(Function() _api.FinalizeazaAccessAsync(k_added, k_references, k_context, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                If k_total.FacturiNoi > 0 OrElse k_total.Operatii > 0 Then Await LoadAsync(_idDdf).ConfigureAwait(True)
                SetSyncStatus(If(k_total.Erori.Count = 0, "Preluarea din Access s-a încheiat.", "Preluarea din Access s-a încheiat cu probleme."), FooterIdleText)
                ShowPreluareResult(k_total, k_final)
            Finally
                _syncing = False
                If Not IsDisposed Then tree.FooterCaption = FooterIdleText
            End Try
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As InvalidOperationException
            ' The bridge's refusals (no registry, no store file) carry their own Romanian text.
            GlobalErrorLog.Write("PrimiteView.PreiaDinAccessAsync", ex)
            If Not IsDisposed Then
                RaiseEvent SyncStatus("Preluarea din Access s-a oprit.")
                KBotMessage.Show(FindForm(), ex.Message, AccessCaptionPreia, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.PreiaDinAccessAsync", ex)
            If Not IsDisposed Then
                RaiseEvent SyncStatus("Preluarea din Access s-a oprit.")
                KBotMessage.Show(FindForm(), ex.Message, AccessCaptionPreia, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PreiaDinAccessAsync", ex)
            If Not IsDisposed Then
                RaiseEvent SyncStatus("Preluarea din Access s-a oprit.")
                KBotMessage.Show(FindForm(), "Preluarea din Access nu a putut fi dusă la capăt. Detalii în jurnalul de erori.", AccessCaptionPreia,
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Function

    ''' <summary>
    ''' «Trimite in Access»: adds to the Access store of the year the messages K-BOT has and the file does not, with their invoices, lines, notes and ANAF
    ''' messages (not the booked operations). A message the file already has is left as it is, so it can be run again. One at a time.
    ''' UI boundary (called from the ribbon): a failure is logged and told to the operator, nothing is thrown.
    ''' </summary>
    Public Async Function TrimiteInAccessAsync() As Task
        Try
            If _api Is Nothing OrElse _syncing Then Return
            Dim k_bridge As IAccessBridge = AccessBridgeOrTell(AccessCaptionTrimite)
            If k_bridge Is Nothing Then Return
            _syncing = True
            Try
                SetSyncStatus("Se caută magazinul de facturi din Access…", "Se caută în Access…")
                Dim k_store As String = k_bridge.ResolveEFacturaStore(AppSettings.Current.AccessRegistryPath, _year)
                Dim k_have As HashSet(Of String) = Await Task.Run(
                    Function() New HashSet(Of String)(k_bridge.ListEFacturaMessageIds(k_store), StringComparer.OrdinalIgnoreCase)).ConfigureAwait(True)
                If IsDisposed Then Return

                Dim k_sum As New AccessEfWriteResult()
                Dim k_missing As Integer = -1
                Dim k_sent As Integer
                Do
                    Dim k_answer As EFacturaAccessExport = Await _gate.RunAsync(
                        Function() _api.ExportaAccessAsync(_year, k_have, AccessSendBatch, _cts.Token)).ConfigureAwait(True)
                    If IsDisposed Then Return
                    If k_missing < 0 Then
                        k_missing = k_answer.TotalLipsa
                        If k_missing = 0 Then
                            SetSyncStatus("Access are deja toate mesajele.", FooterIdleText)
                            KBotMessage.Show(FindForm(), $"Magazinul Access are deja toate mesajele anului {_year} din K-BOT.", AccessCaptionTrimite,
                                             MessageBoxButtons.OK, MessageBoxIcon.Information)
                            Return
                        End If
                        If KBotMessage.Show(FindForm(),
                                            $"K-BOT are {k_missing.ToString(CultureInfo.CurrentCulture)} mesaje din {_year} care lipsesc din magazinul Access." & vbLf &
                                            "Se adaugă în Access, cu facturile, liniile, notele și mesajele lor; ce există deja în Access rămâne neschimbat." & vbLf & vbLf &
                                            "Continuați?", AccessCaptionTrimite, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                            SetSyncStatus(String.Empty, FooterIdleText)
                            Return
                        End If
                    End If
                    If k_answer.Mesaje.Count = 0 Then Exit Do
                    Dim k_batch As List(Of AccessEfMessage) = k_answer.Mesaje
                    Dim k_written As AccessEfWriteResult = Await Task.Run(Function() k_bridge.WriteEFacturaMessages(k_store, k_batch)).ConfigureAwait(True)
                    If IsDisposed Then Return
                    k_sum.MessagesAdded += k_written.MessagesAdded
                    k_sum.MessagesSkipped += k_written.MessagesSkipped
                    k_sum.InvoicesAdded += k_written.InvoicesAdded
                    k_sum.LinesAdded += k_written.LinesAdded
                    k_sum.NotesAdded += k_written.NotesAdded
                    k_sum.InvoiceMessagesAdded += k_written.InvoiceMessagesAdded
                    k_sum.TextsShortened += k_written.TextsShortened
                    For Each k_m As AccessEfMessage In k_batch
                        k_have.Add(k_m.IdSol)
                    Next
                    k_sent += k_batch.Count
                    Dim k_of As String = $"{Math.Min(k_sent, k_missing).ToString(CultureInfo.CurrentCulture)} din {k_missing.ToString(CultureInfo.CurrentCulture)}"
                    SetSyncStatus("Se trimit mesajele în Access… " & k_of, "Se trimit… " & k_of)
                    If k_answer.Ramase = 0 Then Exit Do
                Loop
                SetSyncStatus("Trimiterea în Access s-a încheiat.", FooterIdleText)
                ShowTrimitereResult(k_sum)
            Finally
                _syncing = False
                If Not IsDisposed Then tree.FooterCaption = FooterIdleText
            End Try
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As InvalidOperationException
            GlobalErrorLog.Write("PrimiteView.TrimiteInAccessAsync", ex)
            If Not IsDisposed Then
                RaiseEvent SyncStatus("Trimiterea în Access s-a oprit.")
                KBotMessage.Show(FindForm(), ex.Message, AccessCaptionTrimite, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.TrimiteInAccessAsync", ex)
            If Not IsDisposed Then
                RaiseEvent SyncStatus("Trimiterea în Access s-a oprit.")
                KBotMessage.Show(FindForm(), ex.Message, AccessCaptionTrimite, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.TrimiteInAccessAsync", ex)
            If Not IsDisposed Then
                RaiseEvent SyncStatus("Trimiterea în Access s-a oprit.")
                KBotMessage.Show(FindForm(), "Trimiterea în Access nu a putut fi dusă la capăt. Detalii în jurnalul de erori.", AccessCaptionTrimite,
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Function

    ' ---- helpers: reached only through the two wrapped boundaries above ----------------------------------------------------

    ''' <summary>Records for the journal what one slice of ids gave: how many were asked, how many messages came back, and which ids did not.</summary>
    Private Shared Sub NoteSlice(k_context As AccessPreluareContext, k_asked As List(Of String), k_read As List(Of AccessEfMessage), k_lot As Integer)
        Dim k_back As New HashSet(Of String)(k_read.Select(Function(k_m) k_m.IdSol), StringComparer.OrdinalIgnoreCase)
        For Each k_id As String In k_asked
            If k_back.Contains(k_id) Then Continue For
            k_context.NecititeTotal += 1
            If k_context.Necitite.Count < AccessShownNotRead Then k_context.Necitite.Add(k_id)
        Next
        k_context.Lot = k_lot
        k_context.Cerute = k_asked.Count
        k_context.CititeInLot = k_read.Count
        k_context.Citite += k_read.Count
    End Sub

    ''' <summary>The messages in groups whose archives together stay under the budget (a request is capped by the server); one message is never split.</summary>
    Private Shared Iterator Function SplitBySize(k_messages As List(Of AccessEfMessage)) As IEnumerable(Of List(Of AccessEfMessage))
        Dim k_group As New List(Of AccessEfMessage)()
        Dim k_bytes As Long
        For Each k_m As AccessEfMessage In k_messages
            Dim k_size As Long = If(k_m.Xml Is Nothing, 0L, k_m.Xml.LongLength)
            If k_group.Count > 0 AndAlso k_bytes + k_size > AccessBatchBytes Then
                Yield k_group
                k_group = New List(Of AccessEfMessage)()
                k_bytes = 0
            End If
            k_group.Add(k_m)
            k_bytes += k_size
        Next
        If k_group.Count > 0 Then Yield k_group
    End Function

    Private Shared Sub AddTo(k_total As EFacturaAccessPreluare, k_part As EFacturaAccessPreluare)
        k_total.MesajeNoi += k_part.MesajeNoi
        k_total.MesajeSarite += k_part.MesajeSarite
        k_total.FacturiNoi += k_part.FacturiNoi
        k_total.FacturiSarite += k_part.FacturiSarite
        k_total.Linii += k_part.Linii
        k_total.Note += k_part.Note
        k_total.MesajeFactura += k_part.MesajeFactura
        k_total.Operatii += k_part.Operatii
        k_total.OperatiiSarite += k_part.OperatiiSarite
        k_total.IdPrimite.AddRange(k_part.IdPrimite)
        k_total.Probleme.AddRange(k_part.Probleme)
        k_total.Erori.AddRange(k_part.Erori)
    End Sub

    Private Shared Function Num(k_value As Integer) As String
        Return k_value.ToString("N0", CultureInfo.CurrentCulture)
    End Function

    Private Sub ShowPreluareResult(k_total As EFacturaAccessPreluare, k_final As EFacturaAccessFinal)
        Dim k_lines As New List(Of String) From {
            $"Mesaje noi: {Num(k_total.MesajeNoi)} (deja în K-BOT: {Num(k_total.MesajeSarite)}).",
            $"Facturi noi: {Num(k_total.FacturiNoi)} (deja în K-BOT: {Num(k_total.FacturiSarite)}), cu {Num(k_total.Linii)} linii, " &
            $"{Num(k_total.Note)} note și {Num(k_total.MesajeFactura)} mesaje ANAF.",
            $"Operațiuni contabile preluate: {Num(k_total.Operatii)} (deja în K-BOT: {Num(k_total.OperatiiSarite)})."}
        If k_final.Omisa.Length > 0 Then
            k_lines.Add(k_final.Omisa)
        ElseIf k_final.Citite > 0 Then
            k_lines.Add($"Legate automat de o recepție existentă: {Num(k_final.Legate)} din {Num(k_final.Citite)} facturi noi; restul se leagă din K-BOT.")
        End If
        Dim k_icon As MessageBoxIcon = MessageBoxIcon.Information
        If k_total.Probleme.Count > 0 Then
            k_icon = MessageBoxIcon.Warning
            k_lines.Add(String.Empty)
            k_lines.Add("Nu s-au putut prelua:")
            k_lines.AddRange(k_total.Probleme.Take(AccessShownProblems))
            If k_total.Probleme.Count > AccessShownProblems Then k_lines.Add("…")
        End If
        If k_total.Erori.Count > 0 Then
            k_icon = MessageBoxIcon.Warning
            k_lines.Add(String.Empty)
            k_lines.Add($"{Num(k_total.Erori.Count)} mesaje nu au putut fi scrise:")
            k_lines.AddRange(k_total.Erori.Take(AccessShownProblems))
            If k_total.Erori.Count > AccessShownProblems Then k_lines.Add("…")
        End If
        KBotMessage.Show(FindForm(), String.Join(vbLf, k_lines), AccessCaptionPreia, MessageBoxButtons.OK, k_icon)
    End Sub

    Private Sub ShowTrimitereResult(k_sum As AccessEfWriteResult)
        Dim k_lines As New List(Of String) From {
            $"Mesaje adăugate în Access: {Num(k_sum.MessagesAdded)} (existau deja: {Num(k_sum.MessagesSkipped)}).",
            $"Facturi: {Num(k_sum.InvoicesAdded)}, cu {Num(k_sum.LinesAdded)} linii, {Num(k_sum.NotesAdded)} note și {Num(k_sum.InvoiceMessagesAdded)} mesaje ANAF."}
        Dim k_icon As MessageBoxIcon = MessageBoxIcon.Information
        If k_sum.TextsShortened > 0 Then
            k_icon = MessageBoxIcon.Warning
            k_lines.Add($"{Num(k_sum.TextsShortened)} texte au fost scurtate la 255 de caractere, cât primesc coloanele Access.")
        End If
        KBotMessage.Show(FindForm(), String.Join(vbLf, k_lines), AccessCaptionTrimite, MessageBoxButtons.OK, k_icon)
    End Sub

End Class
