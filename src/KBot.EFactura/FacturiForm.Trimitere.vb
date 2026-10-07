Option Strict On
Imports System.Globalization
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' Slice 00EF-09 -- what the invoice menu does on the server: send a draft to ANAF, read the state of a sent invoice, cancel an accepted one
' by a storno. The rules are the server's (PYTHON/routes/efactura/trimitere.py, factura_routes.py): the window asks, waits and tells what
' happened; a refusal comes back as the server's own Romanian text (with the findings of its checks listed under it).
'
' THE FLOW OF A SEND, as the operator decided on 06.10.2026 (the VBA flow of Access): send -> the window waits 2-3 seconds -> reads the
' state once. Accepted / refused are shown at once; «in prelucrare» is left as it is, the menu then offers «Validează la ANAF» to read the
' state again later.
Partial Public Class FacturiForm

    ' The pause between the upload and the first state check: ANAF needs a moment before it knows the file (the VBA flow waited 2-3 s).
    Private Const AnafPauseMs As Integer = 2500

    ''' <summary>What a send-and-check left behind: whether the invoice may have changed on the server, and what to tell the operator.</summary>
    Private NotInheritable Class FlowResult
        Public Property Reload As Boolean
        Public Property Text As String
        Public Property Kind As NoticeKind = NoticeKind.Success
    End Class

    ' ── Send ────────────────────────────────────────────────────────────────────

    ''' <summary>The menu's «Trimite factura în ANAF» for the shown draft: asks, sends, waits, reads the state once.</summary>
    Private Async Function SendCurrentAsync() As Task
        Dim k_f As EFacturaFactura = _current
        If k_f Is Nothing OrElse k_f.Stare <> EFacturaStare.Ciorna OrElse _mode <> EditMode.Viewing Then Return
        If KBotMessage.Show(Me, $"Trimiteți factura {k_f.Eticheta} din {k_f.DataFactura:dd.MM.yyyy} ({k_f.ClientDenumire}) la ANAF?" & vbLf & vbLf &
                            "Factura se verifică întâi; dacă nu are erori, se trimite." & vbLf &
                            "După trimitere factura nu se mai modifică.",
                            "Trimitere la ANAF", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
        Dim k_result As FlowResult = Nothing
        SetBusy(True, $"Se trimite factura {k_f.Eticheta} la ANAF…")
        Try
            k_result = Await SendAndCheckAsync(k_f).ConfigureAwait(True)
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
        If IsDisposed Then Return
        If k_result.Reload Then Await ReloadAndShowAsync(k_f.IdFactura).ConfigureAwait(True)
        If Not IsDisposed AndAlso k_result.Text IsNot Nothing Then ntfMesaj.Show(k_result.Text, k_result.Kind)
    End Function

    ''' <summary>
    ''' Sends invoice <paramref name="k_f"/>, waits, reads the state once. Does not touch the busy state (the caller holds it) and does not
    ''' reload; every failure is shown here (a message box for a refused send, a notice text for a state that could not be read).
    ''' </summary>
    Private Async Function SendAndCheckAsync(k_f As EFacturaFactura) As Task(Of FlowResult)
        Dim k_result As New FlowResult()
        Try
            Dim k_sent As EFacturaTrimitere = Await _gate.RunAsync(
                Function() _api.SendFacturaAsync(k_f.IdFactura, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return k_result
            k_result.Reload = True
            SetStatus($"Factura {k_f.Eticheta} a fost primită de ANAF. Se așteaptă răspunsul…")
            Try
                Await Task.Delay(AnafPauseMs, _cts.Token).ConfigureAwait(True)
                Dim k_check As EFacturaVerificare = Await _gate.RunAsync(
                    Function() _api.VerifyFacturaAsync(k_f.IdFactura, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return k_result
                DescribeCheck(k_f.Eticheta, k_check, k_result)
                If k_sent.Avertismente.Count > 0 AndAlso k_result.Text IsNot Nothing Then
                    k_result.Text &= vbLf & "Avertismente la verificarea facturii:" & vbLf & "• " & String.Join(vbLf & "• ", k_sent.Avertismente)
                End If
            Catch ex As ApiException
                GlobalErrorLog.Write("FacturiForm.SendAndCheckAsync", ex)
                k_result.Text = $"Factura {k_f.Eticheta} a fost trimisă la ANAF, dar starea ei nu a putut fi citită acum: {ex.Message}" & vbLf &
                                "Citiți-o din meniul facturii, cu «Validează la ANAF»."
                k_result.Kind = NoticeKind.Warning
            End Try
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            ' The send was refused or did not reach ANAF: the server changed nothing.
            GlobalErrorLog.Write("FacturiForm.SendAndCheckAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, ex.Message & TokenHint(ex), "Trimitere la ANAF", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.SendAndCheckAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Trimiterea facturii nu a reușit. Detalii în jurnalul de erori.", "Trimitere la ANAF",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            k_result.Reload = True
        End Try
        Return k_result
    End Function

    ' When ANAF no longer takes the unit's token the way out is the «Token ANAF» button, not the menu.
    Private Shared Function TokenHint(k_ex As ApiException) As String
        If String.Equals(k_ex.Reason, "TOKEN_NECESAR", StringComparison.Ordinal) Then
            Return vbLf & vbLf & "Reînnoiți tokenul cu butonul «Token ANAF» din subsolul ferestrei."
        End If
        Return String.Empty
    End Function

    ''' <summary>What the operator reads after a state check.</summary>
    Private Shared Sub DescribeCheck(k_label As String, k_check As EFacturaVerificare, k_result As FlowResult)
        k_result.Reload = True
        Select Case k_check.Rezultat
            Case EFacturaRezultat.Acceptata
                k_result.Text = $"ANAF a acceptat factura {k_label}."
                k_result.Kind = NoticeKind.Success
            Case EFacturaRezultat.Refuzata
                k_result.Text = $"ANAF a refuzat factura {k_label}." & If(k_check.Mesaj.Length = 0, String.Empty, vbLf & k_check.Mesaj) & vbLf &
                                "Motivul se vede și în vederea «Eroare ANAF»."
                k_result.Kind = NoticeKind.Error
            Case EFacturaRezultat.InPrelucrare
                k_result.Text = $"Factura {k_label} este încă în prelucrare la ANAF. Peste câteva momente, din meniul ei, apăsați «Validează la ANAF»."
                k_result.Kind = NoticeKind.Warning
            Case Else
                k_result.Text = If(k_check.Mesaj.Length = 0, $"ANAF a dat o stare neașteptată pentru factura {k_label}.", k_check.Mesaj)
                k_result.Kind = NoticeKind.Warning
        End Select
    End Sub

    ' ── Check the state ─────────────────────────────────────────────────────────

    ''' <summary>The menu's «Validează la ANAF» for a sent invoice: reads its state at ANAF and stores it.</summary>
    Private Async Function VerifyCurrentAsync() As Task
        Dim k_f As EFacturaFactura = _current
        If k_f Is Nothing OrElse k_f.Stare <> EFacturaStare.Incarcata OrElse _mode <> EditMode.Viewing Then Return
        Dim k_result As New FlowResult()
        SetBusy(True, $"Se citește starea facturii {k_f.Eticheta} la ANAF…")
        Try
            Dim k_check As EFacturaVerificare = Await _gate.RunAsync(
                Function() _api.VerifyFacturaAsync(k_f.IdFactura, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            DescribeCheck(k_f.Eticheta, k_check, k_result)
        Catch ex As OperationCanceledException
            Return
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiForm.VerifyCurrentAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, ex.Message & TokenHint(ex), "Validare la ANAF", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
            Return
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.VerifyCurrentAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Starea facturii nu a putut fi citită. Detalii în jurnalul de erori.", "Validare la ANAF",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
        If IsDisposed Then Return
        Await ReloadAndShowAsync(k_f.IdFactura).ConfigureAwait(True)
        If Not IsDisposed Then ntfMesaj.Show(k_result.Text, k_result.Kind)
    End Function

    ' ── Storno ──────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' The menu's «Stornează factura în ANAF» for an accepted invoice: the server writes the storno (the same lines, signs changed) and the
    ''' replacement invoice, both drafts; the storno is sent at once and its state read, the replacement stays a draft to correct and send.
    ''' </summary>
    Private Async Function StornoCurrentAsync() As Task
        Dim k_f As EFacturaFactura = _current
        If k_f Is Nothing OrElse k_f.Stare <> EFacturaStare.Acceptata OrElse Not k_f.PoateStorna OrElse _mode <> EditMode.Viewing Then Return
        If KBotMessage.Show(Me, $"Stornați factura {k_f.Eticheta} din {k_f.DataFactura:dd.MM.yyyy} ({k_f.ClientDenumire})?" & vbLf & vbLf &
                            "K-BOT întocmește factura de stornare (aceleași linii, cu semnul schimbat) și o trimite la ANAF." & vbLf &
                            "În același timp pregătește o factură nouă, copie a celei stornate, pe care o corectați și o trimiteți." & vbLf & vbLf &
                            "Stornarea nu se poate anula.",
                            "Stornare factură", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
        Dim k_stornoId As Integer = 0
        Dim k_newLabel As String = String.Empty
        Dim k_stornoLabel As String = String.Empty
        Dim k_sendResult As FlowResult = Nothing
        SetBusy(True, "Se stornează factura…")
        Try
            ' The two new invoices take the next two numbers and the same date; it is today, never before the newest invoice.
            Dim k_next As EFacturaNumarUrmator = Await _gate.RunAsync(Function() _api.GetNextNumberAsync(_cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            Dim k_date As Date = Date.Today
            If k_next.DataMinima.HasValue AndAlso k_next.DataMinima.Value.Date > k_date Then k_date = k_next.DataMinima.Value.Date
            Dim k_copy As EFacturaFactura = CopyForStorno(k_f, k_date)
            Dim k_pair As EFacturaStornare = Await _gate.RunAsync(
                Function() _api.StornoFacturaAsync(k_f.IdFactura, k_copy, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            k_stornoId = k_pair.Storno.IdFactura
            k_stornoLabel = k_pair.Storno.Eticheta
            k_newLabel = k_pair.Factura.Eticheta
            SetStatus($"Factura de stornare {k_stornoLabel} a fost întocmită. Se trimite la ANAF…")
            k_sendResult = Await SendAndCheckAsync(k_pair.Storno).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            Return
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiForm.StornoCurrentAsync", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Stornare factură", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.StornoCurrentAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Factura nu a putut fi stornată. Detalii în jurnalul de erori.", "Stornare factură",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
        If IsDisposed Then Return
        If k_stornoId = 0 Then Return
        ' The storno was written (and maybe sent): show it, with the replacement next to it in the tree.
        Await ReloadAndShowAsync(k_stornoId).ConfigureAwait(True)
        If IsDisposed Then Return
        Dim k_text As String = If(k_sendResult IsNot Nothing AndAlso k_sendResult.Text IsNot Nothing, k_sendResult.Text & vbLf,
                                  $"Factura de stornare {k_stornoLabel} este întocmită; o trimiteți din meniul ei." & vbLf) &
                               $"Factura nouă {k_newLabel} (ciornă) este pregătită: corectați-o și trimiteți-o la ANAF."
        ntfMesaj.Show(k_text, If(k_sendResult IsNot Nothing AndAlso k_sendResult.Text IsNot Nothing, k_sendResult.Kind, NoticeKind.Warning))
    End Function

    ''' <summary>The replacement of a storno: the same customer, account, comments and lines as the cancelled invoice, dated <paramref name="k_date"/>.</summary>
    Private Shared Function CopyForStorno(k_f As EFacturaFactura, k_date As Date) As EFacturaFactura
        Dim k_copy As New EFacturaFactura() With {
            .IdClient = k_f.IdClient, .DataFactura = k_date, .Comentarii = k_f.Comentarii, .BT_13 = k_f.BT_13,
            .ContPlata = k_f.ContPlata, .AtasamentOriginal = k_f.AtasamentOriginal}
        For Each k_line As EFacturaLinie In k_f.Linii
            k_copy.Linii.Add(New EFacturaLinie() With {
                .NrCrt = k_line.NrCrt, .Continut = k_line.Continut, .Um = k_line.Um, .Cant = k_line.Cant, .PU = k_line.PU,
                .Valoare = k_line.Valoare, .Platit = k_line.Platit, .Grup = k_line.Grup})
        Next
        Return k_copy
    End Function

End Class
