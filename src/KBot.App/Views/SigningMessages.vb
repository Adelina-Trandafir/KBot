Option Strict On
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The operator messages of the signing flow (slice 0078), one wording for DDF and ORD. Every one
''' goes through <c>KBotMessage</c> (house rule), so it also lands in mesaje_operator.log.
''' </summary>
Public NotInheritable Class SigningMessages

    Private Sub New()
    End Sub

    Private Const Caption As String = "Semnare document"

    ''' <summary>The result of a signing check.</summary>
    Public Shared Sub ShowOutcome(owner As IWin32Window, outcome As PdfSigningOutcome)
        Try
            If outcome Is Nothing OrElse String.IsNullOrEmpty(outcome.Message) Then Return
            Dim icon As MessageBoxIcon = If(outcome.Status = PdfSigningStatus.Uploaded,
                                            MessageBoxIcon.Information, MessageBoxIcon.Warning)
            KBotMessage.Show(owner, outcome.Message, Caption, MessageBoxButtons.OK, icon)
        Catch ex As Exception
            GlobalErrorLog.Write("SigningMessages.ShowOutcome", ex)
        End Try
    End Sub

    ''' <summary>The local signed file was not the server's and has been replaced (item 4 of the slice).</summary>
    Public Shared Sub ShowLocalReplaced(owner As IWin32Window)
        Try
            KBotMessage.Show(owner,
                             "Fișierul semnat de pe acest calculator nu era identic cu originalul de pe server." &
                             Environment.NewLine &
                             "A fost înlocuit cu originalul descărcat de pe server.",
                             Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("SigningMessages.ShowLocalReplaced", ex)
        End Try
    End Sub

    ''' <summary>A kept signed copy reached the server after all.</summary>
    Public Shared Sub ShowPendingUploaded(owner As IWin32Window)
        Try
            KBotMessage.Show(owner,
                             "Documentul semnat păstrat pe acest calculator a fost încărcat acum pe server.",
                             Caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            GlobalErrorLog.Write("SigningMessages.ShowPendingUploaded", ex)
        End Try
    End Sub

    ''' <summary>A kept signed copy was refused (409): the server version is shown.</summary>
    Public Shared Sub ShowPendingConflict(owner As IWin32Window)
        Try
            KBotMessage.Show(owner,
                             "Pe server există între timp altă versiune semnată a acestui document." & Environment.NewLine &
                             $"Copia semnată de pe acest calculator NU a fost încărcată și rămâne în «{PendingPdfUploads.Root}». " &
                             "Se afișează versiunea de pe server.",
                             Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("SigningMessages.ShowPendingConflict", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Generating a document that already has a SIGNED copy on the server: the new one is unsigned
    ''' and would have to be signed again from scratch. True = go ahead.
    ''' </summary>
    Public Shared Function ConfirmRegenerateSigned(owner As IWin32Window) As Boolean
        Try
            Return KBotMessage.Show(owner,
                                    "Acest document are deja o versiune SEMNATĂ pe server." & Environment.NewLine &
                                    "Documentul generat acum va fi NESEMNAT și, dacă îl semnați, va înlocui versiunea de pe server." &
                                    Environment.NewLine & "Continuați?",
                                    Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button2) = DialogResult.Yes
        Catch ex As Exception
            GlobalErrorLog.Write("SigningMessages.ConfirmRegenerateSigned", ex)
            Return False
        End Try
    End Function

    ''' <summary>Several kept copies were retried after login -- one line each.</summary>
    Public Shared Sub ShowRetrySummary(owner As IWin32Window, lines As IEnumerable(Of String))
        Try
            If lines Is Nothing Then Return
            Dim text As String = String.Join(Environment.NewLine, lines)
            If text.Length = 0 Then Return
            KBotMessage.Show(owner, "Documente semnate păstrate pe acest calculator:" & Environment.NewLine & text,
                             Caption, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            GlobalErrorLog.Write("SigningMessages.ShowRetrySummary", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0088-04: the retry at start left signed copies that did not reach the server. Says what
    ''' happened to each and asks whether to delete the ones left: Yes deletes them from
    ''' <see cref="PendingPdfUploads.Root"/>, No keeps them for the next start.
    ''' </summary>
    Public Shared Sub AskDeletePending(owner As IWin32Window, result As PendingRetryResult)
        Try
            If result Is Nothing OrElse result.Left.Count = 0 Then Return
            Dim text As String =
                "Documente semnate păstrate pe acest calculator:" & Environment.NewLine &
                String.Join(Environment.NewLine, result.Lines) & Environment.NewLine & Environment.NewLine &
                $"{result.Left.Count} document(e) NU au ajuns pe server. Le ștergeți de pe acest calculator?" & Environment.NewLine &
                "Da = se șterg (semnăturile din ele se pierd); Nu = rămân și se reîncearcă la următoarea pornire."
            If KBotMessage.Show(owner, text, Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            Dim failed As New List(Of String)()
            For Each entry As PendingPdfUpload In result.Left
                Try
                    PendingPdfUploads.Remove(entry.Kind, entry.Id)
                Catch ex As Exception
                    ' Logged by Remove; said once below.
                    failed.Add(PendingPdfUploads.Label(entry))
                End Try
            Next
            If failed.Count > 0 Then
                KBotMessage.Show(owner, "Nu s-au putut șterge: " & String.Join(", ", failed) &
                                 $". Fișierele sunt în «{PendingPdfUploads.Root}».",
                                 Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SigningMessages.AskDeletePending", ex)
        End Try
    End Sub

End Class
