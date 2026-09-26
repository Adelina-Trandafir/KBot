Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Forexe

''' <summary>
''' FOREXE downloads started from the angajamente tree (slice 0086 split out of KbotForm.vb):
''' the footer's right icon (the list) and a node's right icon (the whole angajament), plus the
''' "reuse what is already in memory?" questions for both. What happens to a downloaded
''' package afterwards is in KbotForm.Ingest.vb.
''' </summary>
Partial Public Class KbotForm

    ''' <summary>
    ''' The RIGHT icon of the tree footer (slice 0057) = refresh the angajamente list
    ''' from FOREXE.
    ''' </summary>
    ''' <remarks>
    ''' The rule the operator asked for: an angajament the server already has is LEFT
    ''' ALONE -- neither Descriere nor Stare is touched; one that is missing is added
    ''' empty (its header only, no indicatori / receptii / plati) and only then shows up
    ''' in the tree. Downloading it whole stays the right icon of the NODE.
    '''
    ''' The figures come from the server; they are not counted here. The tree shows one
    ''' period only (a year + an SS), so an angajament from another period would look new
    ''' to it.
    ''' </remarks>
    Private Async Sub Tree_FooterRightIconClicked(e As MouseEventArgs) Handles tree.FooterRightIconClicked
        Try
            Dim mapate As List(Of Angajament)
            busyBar.Running = True
            Try
                mapate = Await _controller.DownloadListaAsync()
            Finally
                busyBar.Running = False
            End Try

            If mapate Is Nothing Then
                ShowForexeFailure("Listă angajamente")
                Return
            End If

            ' With no DbName (no login -- possible only in the Debug harness) we cannot
            ' aim at the unit's database. The list was saved locally by the coordinator
            ' either way.
            If String.IsNullOrEmpty(_session.DbName) Then
                KBotMessage.Show(Me,
                    "Lista a fost descărcată și salvată local, dar nu poate fi trimisă pe server: " &
                    "sesiunea nu are baza unității (necesită login).",
                    "Listă angajamente", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim rezultat As AngajamenteAdaugate
            busyBar.Running = True
            Try
                rezultat = Await WithReauth(Of AngajamenteAdaugate)(
                    Function() _apiClient.AdaugaAngajamenteNoiAsync(_session.DbName, mapate,
                                                                    CancellationToken.None))
            Finally
                busyBar.Running = False
            End Try

            ' The tree is re-read ONLY when something was written: a reload clears the
            ' selection and drops the operator back on «sumar», which is pointless when no
            ' new angajament appeared.
            If rezultat.Inserate > 0 Then Await LoadTreeAsync()

            KBotMessage.Show(Me,
                $"Angajamente în FOREXE: {rezultat.Candidate}." & Environment.NewLine &
                $"Adăugate acum: {rezultat.Inserate} · deja existente (neatinse): {rezultat.Existente}.",
                "Listă angajamente", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            ' UI boundary (async Sub): cannot re-throw -- log it and say why.
            GlobalErrorLog.Write("MainForm.tree_FooterRightIconClicked", ex)
            KBotMessage.Show(Me, "Actualizarea listei de angajamente a eșuat: " & ex.Message,
                            "Listă angajamente", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Shows why a FOREXE intent came back empty -- but ONLY when it was a failure.
    ''' LastFailure stays empty when the operator cancelled it themselves (closed the
    ''' certificate dialog, pressed Cancel), and a box telling them that back would
    ''' be noise.
    ''' </summary>
    Private Sub ShowForexeFailure(titlu As String)
        Try
            Dim motiv As String = _controller.LastFailure
            If String.IsNullOrWhiteSpace(motiv) Then Return
            KBotMessage.Show(Me, motiv, titlu, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ShowForexeFailure", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The right icon of a NODE = download that whole angajament from FOREXE («Prelucrare
    ''' Completa», or the REVERSE variant when it already has local history) AND carry it all the
    ''' way into the tables, through the two phases of the ingest (slice 0055).
    '''
    ''' <para><b>A second press no longer downloads out of reflex</b> (operator, 09.09.2026).
    ''' When this angajament was already downloaded while the application stayed open and the
    ''' package is fit (it exists and has rows), the operator is asked whether to reuse it. See
    ''' <see cref="IntreabaDacaRefolosescPachetul"/> for why it is worth asking.</para>
    ''' </summary>
    Private Async Sub Tree_RightIconClicked(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            Dim cod As String = If(pNode Is Nothing, Nothing, TryCast(pNode.Tag, String))
            If String.IsNullOrEmpty(cod) Then Return

            Dim pachet As PrelucrareRezultat = IntreabaDacaRefolosescPachetul(cod)
            If pachet Is Nothing Then
                ' The question FIRST, then the robot (slice 0060): the receptii choice changes
                ' what the workflow runs, so it must be known before it starts. `Nothing` = the
                ' operator closed the form -- then nothing is downloaded, because giving up the
                ' question is giving up the download, not a default download.
                Dim sarite As List(Of Date) = Await AlegeReceptiileDeSaritAsync(cod)
                If sarite Is Nothing Then Return

                busyBar.Running = True
                Try
                    ' The LOCAL history decides forward/reverse (Access FX_Angajament_InfoComplete):
                    ' it is read through the same re-login net as the rest of the shell.
                    pachet = Await _controller.DownloadNodeAsync(
                        cod,
                        Function(c, ct) WithReauth(Of IstoricInfo)(Function() _apiClient.GetIstoricAsync(c, ct)),
                        sarite)
                Finally
                    busyBar.Running = False
                End Try
            End If

            ' Nothing = the robot did not start or failed; it already said why on the console.
            If pachet Is Nothing Then Return
            Await DuLaIngestieAsync(cod, pachet)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.tree_RightIconClicked", ex)
            KBotMessage.Show(Me, "Descărcarea angajamentului a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' THE ANGAJAMENTE LIST from memory, when the operator wants it. Nothing = download again.
    ''' The twin of <see cref="IntreabaDacaRefolosescPachetul"/>, on the synchronise button.
    ''' </summary>
    ''' <remarks>
    ''' "Downloaded correctly" means here: it reached the store (so the robot reported success)
    ''' and it holds at least one row. An empty list would have nothing to send to the upsert,
    ''' so it is not offered.
    ''' </remarks>
    Private Function IntreabaDacaRefolosescLista() As List(Of Angajament)
        Try
            Dim lista As IReadOnlyList(Of Angajament) = _controller.Rezultate.UltimaLista
            Dim moment As Date? = _controller.Rezultate.MomentLista
            If lista Is Nothing OrElse lista.Count = 0 OrElse Not moment.HasValue Then Return Nothing

            Dim raspuns As DialogResult = KBotMessage.Show(
                Me,
                $"Lista de angajamente a fost deja descărcată din FOREXE la " &
                $"{moment.Value:HH:mm:ss} ({moment.Value:dd.MM.yyyy}), cu {lista.Count} " &
                "angajamente, și este încă în memorie." & Environment.NewLine &
                Environment.NewLine &
                "Da = trimit pe server lista din memorie (imediat)." & Environment.NewLine &
                "Nu = descarc lista din nou din FOREXE.",
                "Sincronizare",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
            If raspuns <> DialogResult.Yes Then Return Nothing

            _controller.SpuneStare($"Se folosește lista descărcată la {moment.Value:HH:mm:ss} " &
                               "(din memorie, fără o descărcare nouă).")
            Return New List(Of Angajament)(lista)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.IntreabaDacaRefolosescLista", ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' THE IN-MEMORY PACKAGE, when the operator wants it. Nothing = download again.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Why it asks instead of deciding on its own</b> (operator, 09.09.2026). A FOREXE
    ''' download takes minutes: it opens the browser, authenticates on the token, walks the
    ''' whole flow. And a second press on the same icon is almost always a RETRY -- the placement
    ''' form was closed without saving, or something was placed wrong -- not a request for fresher
    ''' data. Downloading every time throws the operator's minutes away; reusing every time
    ''' would hide whatever changed on FOREXE meanwhile. Hence the question, with the moment of
    ''' the download written into it, because that is exactly the fact they can decide from.</para>
    ''' <para><b>Only what is fit to reuse is offered</b> -- see
    ''' <c>WorkflowResultStore.PachetBunDeRefolosit</c>: a package enters the store only after
    ''' the robot reported success, and one with no rows is not offered at all, because the
    ''' ingest would refuse it anyway.</para>
    ''' <para><b>Reuse is safe by construction.</b> The ingest has a two-phase contract over the
    ''' SAME payload (slice 0055): the proposal is asked for with the package, the save resends
    ''' it unchanged. A package held in memory is the very object a download would have handed
    ''' back, so both phases see exactly the same thing.</para>
    ''' </remarks>
    Private Function IntreabaDacaRefolosescPachetul(cod As String) As PrelucrareRezultat
        Try
            Dim pachet As PrelucrareRezultat = _controller.Rezultate.PachetBunDeRefolosit(cod)
            If pachet Is Nothing Then Return Nothing

            Dim randuri As Integer = WorkflowResultStore.NumaraRanduri(pachet)
            Dim raspuns As DialogResult = KBotMessage.Show(
                Me,
                $"Angajamentul «{cod}» a fost deja descărcat din FOREXE la " &
                $"{pachet.Moment:HH:mm:ss} ({pachet.Moment:dd.MM.yyyy}), cu {randuri} rânduri, " &
                "și datele sunt încă în memorie." & Environment.NewLine &
                Environment.NewLine &
                "Da = folosesc datele din memorie (imediat)." & Environment.NewLine &
                "Nu = descarc din nou din FOREXE (durează, dar aduce orice s-a schimbat între timp).",
                "FOREXE — descărcare",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
            If raspuns <> DialogResult.Yes Then Return Nothing

            _controller.SpuneStare($"«{cod}»: se folosesc datele descărcate la {pachet.Moment:HH:mm:ss} " &
                               "(din memorie, fără o descărcare nouă).")
            Return pachet
        Catch ex As Exception
            ' UI boundary: if the question itself fails, download again. Never the other way
            ' round -- a silent reuse after an error is precisely the decision the machine does
            ' not get to take.
            GlobalErrorLog.Write("MainForm.IntreabaDacaRefolosescPachetul", ex)
            Return Nothing
        End Try
    End Function
End Class
