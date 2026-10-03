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
            ' Slice 0098: through the robot queue, in the order the operator asked.
            Await _robotQueue.RunAsync("lista", "Lista de angajamente", AddressOf DescarcaListaAsync)
        Catch ex As RobotTaskDroppedException
            ' Duplicate or taken out of the queue: the console already said it.
        Catch ex As Exception
            ' UI boundary (async Sub): cannot re-throw -- log it and say why.
            GlobalErrorLog.Write("MainForm.tree_FooterRightIconClicked", ex)
            KBotMessage.Show(Me, "Actualizarea listei de angajamente a eșuat: " & ex.Message,
                            "Listă angajamente", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>The list refresh itself, run as one robot queue task (slice 0098).</summary>
    Private Async Function DescarcaListaAsync() As Task
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

            ' Slice 0100: what the tree held before, to tell the angajamente that APPEAR in it.
            Dim dinainte As New HashSet(Of String)(_treeInfos.Keys, StringComparer.OrdinalIgnoreCase)

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
            Dim coduriNoi As List(Of String) =
                _treeInfos.Keys.Where(Function(c) Not dinainte.Contains(c)).ToList()

            ' A good download is SILENT (operator, 28.09.2026): the figures go to the operator
            ' log only, never into a box. Only errors are shown.
            OperatorLog.Write("MainForm.Tree_FooterRightIconClicked", "Listă angajamente",
                $"Angajamente în FOREXE: {rezultat.Candidate}. " &
                $"Adăugate acum: {rezultat.Inserate} · deja existente (neatinse): {rezultat.Existente}.")

            ' Slice 0100: multi-thread mode only -- the new ones can be downloaded right away.
            Await IntreabaDespreAngajamenteleNoiAsync(coduriNoi)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DescarcaListaAsync", ex)
            Throw
        End Try
    End Function

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
            Await PuneNodulInCoadaAsync(cod)
        Catch ex As Exception
            ' UI boundary (async Sub): PuneNodulInCoadaAsync already answers its own failures.
            GlobalErrorLog.Write("MainForm.tree_RightIconClicked", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0109: «Actualizeaza angajamente...» without multi-thread -- every ticked angajament is
    ''' queued exactly as if its own node icon had been pressed, one after the other in the order
    ''' given. The calls queue synchronously (each runs up to its first wait), so the first one is
    ''' asked about its receptii as usual and the rest, finding the queue busy, read them all.
    ''' </summary>
    Private Async Function PuneNodurileInCoadaAsync(coduri As IEnumerable(Of String)) As Task
        Try
            Dim lucrari As New List(Of Task)()
            For Each k_cod As String In coduri
                If String.IsNullOrWhiteSpace(k_cod) Then Continue For
                lucrari.Add(PuneNodulInCoadaAsync(k_cod))
            Next
            Await Task.WhenAll(lucrari)
        Catch ex As Exception
            ' UI boundary: each node already answers its own failure; this only guards the join.
            GlobalErrorLog.Write("MainForm.PuneNodurileInCoadaAsync", ex)
        End Try
    End Function

    ''' <summary>
    ''' One node's whole download as a robot queue task (slice 0098), shared by the node icon and the
    ''' «Actualizeaza angajamente...» window (slice 0109). A failure is shown here; a duplicate or a
    ''' task taken out of the queue is not (the console already said it).
    ''' </summary>
    Private Async Function PuneNodulInCoadaAsync(cod As String) As Task
        Try
            MarcheazaDescarcareaFaraIntrebare(cod)
            ' Slice 0098: through the robot queue -- several clicks in a row run one after the
            ' other, in order, and a second click on a node already queued is refused.
            Await _robotQueue.RunAsync("nod|" & cod, $"Descărcare completă «{cod}»",
                                       Function() DescarcaNodulAsync(cod))
        Catch ex As RobotTaskDroppedException
            ' Duplicate or taken out of the queue: the console already said it.
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PuneNodulInCoadaAsync", ex)
            KBotMessage.Show(Me, "Descărcarea angajamentului a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Function

    ''' <summary>
    ''' When another FOREXE action is already running or waiting, this one and the waiting
    ''' node / receptii ones skip the receptii selection window: all receptii are downloaded.
    ''' </summary>
    Private Sub MarcheazaDescarcareaFaraIntrebare(cod As String)
        If _robotQueue.ActionCount(_controller.IsBusy) = 0 Then
            _descarcareFaraIntrebare.Remove(cod)   ' a lone click is asked as usual
            Return
        End If
        _descarcareFaraIntrebare.Add(cod)
        For Each w As RobotQueue.RobotTask In _robotQueue.Waiting
            Dim key As String = If(w.Key, String.Empty)
            Dim i As Integer = key.IndexOf("|"c)
            If i > 0 AndAlso (key.StartsWith("nod|", StringComparison.OrdinalIgnoreCase) OrElse
                              key.StartsWith("receptii|", StringComparison.OrdinalIgnoreCase)) Then
                _descarcareFaraIntrebare.Add(key.Substring(i + 1))
            End If
        Next
    End Sub

    ''' <summary>A node's whole download + ingest, run as one robot queue task (slice 0098).</summary>
    Private Async Function DescarcaNodulAsync(cod As String) As Task
        Try
            ' Links that do not close would make the save refuse after the robot ran.
            If Not Await AsocierePermiteAsync(cod, "Descărcarea completă") Then Return
            Dim pachet As PrelucrareRezultat = IntreabaDacaRefolosescPachetul(cod)
            If pachet Is Nothing Then
                ' The question FIRST, then the robot (slice 0060): the receptii choice changes
                ' what the workflow runs, so it must be known before it starts. `Nothing` = the
                ' operator closed the form -- then nothing is downloaded, because giving up the
                ' question is giving up the download, not a default download.
                Dim sarite As ReceptiiSarite = Await AlegeReceptiileDeSaritAsync(cod)
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
            If pachet Is Nothing Then
                SpuneCapturiNetrimise(cod, "descărcarea din FOREXE nu a reușit")
                Return
            End If
            If Not Await DuLaIngestieAsync(cod, pachet) Then
                SpuneCapturiNetrimise(cod, "descărcarea nu s-a salvat în K-BOT")
                Return
            End If

            ' Pictures of the FOREXE page still waiting on disk for this angajament (operator,
            ' 28.09.2026): the whole node was just written, so the records they hang off may
            ' exist now. What the server still cannot place stays on disk, as on the browser road.
            Await TrimiteCapturileAsync(cod, CapturaStore.FelReceptie)
            Await TrimiteCapturileAsync(cod, CapturaStore.FelRezervare)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DescarcaNodulAsync", ex)
            Throw
        End Try
    End Function

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
