Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' The receptie actions of the shell (slice 0086 split out of KbotForm.vb): the link editor
''' (slice 0048-04) and the rebuild from FX_Istoric (slice 0062). Both live in the shell, not
''' in ReceptiiView, because they need the re-login net on their own response shapes and
''' <c>WithReauth</c> is private and generic here.
''' </summary>
Partial Public Class KbotForm

    ''' <summary>
    ''' Opens the receptie -> snapshot link editor (slice 0048-04), MODALLY.
    '''
    ''' <para>It lives here, not in the view, for one reason: the form needs the re-login net on
    ''' TWO response shapes, and <c>WithReauth</c> is private and generic in the shell. The view
    ''' only gets this action, so the re-login policy stays, as everywhere, in one place.</para>
    '''
    ''' <para>D-I: modal. After a save the receptii reload -- changed links move the headers
    ''' between receptii, so what stayed on screen is no longer true.</para>
    ''' </summary>
    Private Async Sub DeschideLegaturileReceptiilor(cod As String)
        Try
            If String.IsNullOrWhiteSpace(cod) Then Return
            Using f As New AsociereForm(_apiClient, cod,
                                        Function(op) WithReauth(Of AsociereStare)(op),
                                        Function(op) WithReauth(Of AsociereRezultat)(op))
                f.ShowDialog(Me)
                If f.SAuSalvatModificari Then
                    ' The tree, not only the view (operator, 10.09.2026): a moved link can turn
                    ' the node's Are* flags on or off, and `LoadTreeAsync` with the selection kept
                    ' pushes the new context into the open view by itself -- so `Reincarca()`
                    ' would be a second read of the same thing.
                    Await LoadTreeAsync(pastreazaSelectia:=True)
                End If
            End Using
        Catch ex As Exception
            ' UI boundary: logged and shown; a throw from here would land on the UI thread.
            GlobalErrorLog.Write("MainForm.DeschideLegaturileReceptiilor", ex)
            KBotMessage.Show(Me, "Editorul de legături nu a putut fi deschis. Detalii în jurnalul de erori.",
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Rebuilds, from FX_Istoric, the receptie snapshots (FX_Receptii_H) and lines
    ''' (FX_Receptii) that are missing for one angajament -- the footer-left icon of the
    ''' <c>ReceptiiView</c> tree (slice 0062).
    ''' </summary>
    ''' <remarks>
    ''' <para>Two calls to the same endpoint: a dry run first, so the operator confirms real
    ''' numbers, then the write. Nothing is written when the dry run finds nothing, or when
    ''' the operator says no. Lives here for the usual reason: the call needs the re-login net
    ''' on its own response shape, and <c>WithReauth</c> is private to the shell.</para>
    ''' <para>After a write the TREE reloads with the selection kept: rebuilt snapshots can
    ''' switch the node's Are* flags, and the reload pushes the fresh context into the open
    ''' view by itself. The rebuilt snapshots land UNPLACED (no receptie); the operator places
    ''' them from the link editor, which is why the closing message points there.</para>
    ''' </remarks>
    Private Async Sub RebuildReceptiiFromIstoric(cod As String)
        Try
            If String.IsNullOrWhiteSpace(cod) Then Return

            Dim dryRun As ReceptiiRebuildResult
            busyBar.Running = True
            Try
                dryRun = Await WithReauth(Of ReceptiiRebuildResult)(
                    Function() _apiClient.RebuildReceptiiAsync(cod, False, CancellationToken.None))
            Finally
                busyBar.Running = False
            End Try
            If dryRun Is Nothing Then
                AratEsecul("Refacerea recepțiilor")
                Return
            End If

            If dryRun.NothingToDo Then
                KBotMessage.Show(Me,
                    $"«{cod}»: toate instantaneele și liniile din istoric există deja în recepții. " &
                    "Nu este nimic de refăcut." & WarningsParagraph(dryRun),
                    "Refacerea recepțiilor", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim question As String =
                $"«{cod}»: față de istoric lipsesc {dryRun.HeadersMissing} instantanee și " &
                $"{dryRun.LinesMissing} linii de recepție; {dryRun.LinesOrphaned} linii există dar " &
                "nu stau pe niciun instantaneu și vor fi relegate." & vbCrLf & vbCrLf &
                "Instantaneele refăcute NU se așază pe nicio recepție: vor apărea în dosarul " &
                "«Instantanee neașezate» și se așază din editorul de legături." &
                WarningsParagraph(dryRun) & vbCrLf & vbCrLf & "Continuați?"
            If KBotMessage.Show(Me, question, "Refacerea recepțiilor",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            Dim result As ReceptiiRebuildResult
            busyBar.Running = True
            Try
                result = Await WithReauth(Of ReceptiiRebuildResult)(
                    Function() _apiClient.RebuildReceptiiAsync(cod, True, CancellationToken.None))
            Finally
                busyBar.Running = False
            End Try
            If result Is Nothing Then
                AratEsecul("Refacerea recepțiilor")
                Return
            End If

            KBotMessage.Show(Me,
                $"«{cod}»: s-au scris {result.HeadersWritten} instantanee și " &
                $"{result.LinesWritten} linii; {result.LinesRelinked} linii au fost relegate." &
                If(result.ReceptiiRecalculated.Count > 0,
                   vbCrLf & $"Diferențele s-au recalculat pe {result.ReceptiiRecalculated.Count} recepții.",
                   String.Empty) &
                WarningsParagraph(result),
                "Refacerea recepțiilor", MessageBoxButtons.OK, MessageBoxIcon.Information)

            Await LoadTreeAsync(pastreazaSelectia:=True)
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.RebuildReceptiiFromIstoric", ex)
            KBotMessage.Show(Me, ex.Message, "Refacerea recepțiilor",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            ' UI boundary: log and show; a throw from here would land on the UI thread.
            GlobalErrorLog.Write("MainForm.RebuildReceptiiFromIstoric", ex)
            KBotMessage.Show(Me, "Refacerea recepțiilor a eșuat: " & ex.Message,
                            "Refacerea recepțiilor", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>The server's warnings as a trailing paragraph, or nothing when there are none.</summary>
    Private Shared Function WarningsParagraph(r As ReceptiiRebuildResult) As String
        If r Is Nothing OrElse r.Warnings.Count = 0 Then Return String.Empty
        Return vbCrLf & vbCrLf & "Semnalări:" & vbCrLf & "  • " &
               String.Join(vbCrLf & "  • ", r.Warnings)
    End Function
End Class
