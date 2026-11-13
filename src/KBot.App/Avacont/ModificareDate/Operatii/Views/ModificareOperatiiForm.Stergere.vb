Option Strict On
Imports System.Linq
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

' Slice AV02-05 - «Modificare operatii»: «Sterge operatia» and «Sterge documentul» (the Access buttons STO / STD). The server decides: the reason
' a row cannot go is already in the list (shown at once, no question asked); otherwise the server checks again, the operator answers its question
' (with the invoices a payment paid, which become unpaid), and the server deletes under lock, checking a third time.
Partial Public Class ModificareOperatiiForm

    Private Const TitluStergeOperatie As String = "Ștergere operație"
    Private Const TitluStergeDocument As String = "Ștergere document"

    ' UI boundary: started without await.
    Private Async Sub StergeOperatie()
        Try
            Dim k_op As OperatieContabila = SelectedOperatie()
            If _busy OrElse k_op Is Nothing Then Return
            If Not String.IsNullOrEmpty(k_op.Refuz) Then
                KBotMessage.Show(Me, k_op.Refuz, TitluStergeOperatie, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            SetBusy(True)
            Dim k_check As OperatieStergereVerificare = Await _gate.RunAsync(
                Function() _api.VerificaStergereOperatieAsync(k_op.IdOper, CancellationToken.None)).ConfigureAwait(True)
            SetBusy(False)
            If Not Confirm(k_check, TitluStergeOperatie) Then Return
            SetBusy(True)
            Await _gate.RunAsync(Function() _api.StergeOperatieAsync(k_op.IdOper, CancellationToken.None)).ConfigureAwait(True)
            SetBusy(False)
            LoadList()
        Catch ex As ApiException
            GlobalErrorLog.Write("ModificareOperatiiForm.StergeOperatie", ex)
            AfterFailedDelete(ex.Message, TitluStergeOperatie)
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.StergeOperatie", ex)
            AfterFailedDelete("Operația nu a putut fi ștearsă. Detalii în jurnalul de erori.", TitluStergeOperatie)
        End Try
    End Sub

    ' UI boundary: started without await.
    Private Async Sub StergeDocument()
        Try
            Dim k_op As OperatieContabila = SelectedOperatie()
            If _busy OrElse k_op Is Nothing Then Return
            ' Every operation of the document must be deletable; the list already knows the reasons of the ones on screen.
            Dim k_blocked As OperatieContabila = gridOperatii.Rows.Select(Function(k_r) TryCast(k_r.Tag, OperatieContabila)).
                FirstOrDefault(Function(k_o) k_o IsNot Nothing AndAlso k_o.IdDocument = k_op.IdDocument AndAlso Not String.IsNullOrEmpty(k_o.Refuz))
            If k_blocked IsNot Nothing Then
                KBotMessage.Show(Me, $"Operația «{k_blocked.Explicatie}» a documentului nu se poate șterge: {k_blocked.Refuz}",
                                 TitluStergeDocument, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            SetBusy(True)
            Dim k_check As OperatieStergereVerificare = Await _gate.RunAsync(
                Function() _api.VerificaStergereDocumentAsync(k_op.IdDocument, CancellationToken.None)).ConfigureAwait(True)
            SetBusy(False)
            If Not Confirm(k_check, TitluStergeDocument) Then Return
            SetBusy(True)
            Await _gate.RunAsync(Function() _api.StergeDocumentAsync(k_op.IdDocument, CancellationToken.None)).ConfigureAwait(True)
            SetBusy(False)
            LoadList()
        Catch ex As ApiException
            GlobalErrorLog.Write("ModificareOperatiiForm.StergeDocument", ex)
            AfterFailedDelete(ex.Message, TitluStergeDocument)
        Catch ex As Exception
            GlobalErrorLog.Write("ModificareOperatiiForm.StergeDocument", ex)
            AfterFailedDelete("Documentul nu a putut fi șters. Detalii în jurnalul de erori.", TitluStergeDocument)
        End Try
    End Sub

    ' True = go on. A refusal is shown and answers False; otherwise the operator answers the server's question («Nu» is the default button).
    Private Function Confirm(k_check As OperatieStergereVerificare, k_title As String) As Boolean
        If IsDisposed Then Return False
        If Not String.IsNullOrEmpty(k_check.Refuz) Then
            KBotMessage.Show(Me, k_check.Refuz, k_title, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        Return KBotMessage.Show(Me, k_check.Intrebare, k_title, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) = DialogResult.Yes
    End Function

    Private Sub AfterFailedDelete(k_text As String, k_title As String)
        If IsDisposed Then Return
        SetBusy(False)
        KBotMessage.Show(Me, k_text, k_title, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        LoadList()
    End Sub

End Class
