Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe

''' <summary>
''' From a downloaded package to the tables (slice 0055), and the partial refreshes the
''' operator asks for (slice 0060) -- slice 0086 split out of KbotForm.vb.
''' </summary>
''' <remarks>
''' Three entry points, one road: the right icon of a NODE (the whole angajament, in
''' KbotForm.Download.vb), the footer icon of the RECEPTII tree and the one of the REZERVARI
''' tree. They all live in the shell, not in the views, for the same reason as
''' <c>DeschideLegaturileReceptiilor</c>: they need the re-login net, which is private and
''' generic in the shell, and the re-login policy stays in one place.
''' </remarks>
Partial Public Class KbotForm

    ''' <summary>
    ''' The road from the downloaded package to the tables (slice 0055), in two phases.
    '''
    ''' <para><b>PHASE ONE</b> -- <c>PrelucrareCoordinator.CerePropunereAsync</c>: the server
    ''' runs every step in one transaction and rolls it back UNCONDITIONALLY, then returns the
    ''' picture. Nothing is written. When a classification matches several units, the
    ''' coordinator opens the choice dialog by itself, as many times as needed.</para>
    '''
    ''' <para><b>PHASE TWO</b> -- the button in <c>AsociereForm</c>: it resends THE SAME package,
    ''' with the fingerprint and the operator's decisions, and the server commits. That is why
    ''' the package is kept alive between phases: <c>rand_istoric</c> in the decisions is the
    ''' row index in <c>TabelIstoric</c>, not a database key, so the second phase must see
    ''' exactly what the first one saw.</para>
    '''
    ''' <para>Closing the form without saving is NOT an error path: it is the operator's choice
    ''' not to write the download. The form warns them what they lose.</para>
    '''
    ''' <para>Slice 0081-04: returns True only when the package was saved on the server -- the
    ''' DDF send moves the revision forward only then.</para>
    ''' </summary>
    Private Async Function DuLaIngestieAsync(cod As String, pachet As PrelucrareRezultat) As Task(Of Boolean)
        Try
            ' EMPTY PACKAGE > THE SERVER IS NOT TOUCHED (operator, 08.09.2026).
            '
            ' A package without a single row has nothing to ask the server: the proposal would
            ' run every step over nothing and come back empty, and the operator would get either
            ' an association window with no lines or the server's error for a request it had no
            ' reason to receive. Say it plainly, here, and stop.
            '
            ' ROWS are counted, not tables: the workflow returns its five tables even when all
            ' of them are empty, so `Tabele.Count` would be 5 for a package with nothing in it.
            ' Same arithmetic as `total` in `ForexeController.DownloadNodeAsync`.
            Dim randuri As Integer = 0
            If pachet IsNot Nothing AndAlso pachet.Tabele IsNot Nothing Then
                randuri = pachet.Tabele.Values.Sum(Function(t) If(t Is Nothing, 0, t.Count))
            End If
            If randuri = 0 Then
                KBotMessage.Show(Me,
                    $"Pachetul descărcat pentru «{cod}» e gol: FOREXE n-a întors niciun rând." &
                    Environment.NewLine &
                    "Nu s-a trimis nimic pe server. Verificați dacă angajamentul chiar are date " &
                    "în FOREXE și reluați descărcarea.",
                    "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return False
            End If

            ' The unit choices are GATHERED here, not in the coordinator: the same list also
            ' goes into phase two, because the «do not ask me again» tick was rolled back with
            ' the proposal.
            Dim alegeri As New List(Of AlegereUnitate)()
            Dim coordonator As New PrelucrareCoordinator(_apiClient)

            Dim propunere As PrelucrarePropunere
            busyBar.Running = True
            Try
                ' CancellationToken.None, as on any call the operator starts from the shell. NOT
                ' `_cts.Token`: that field is born only in SincronizeazaAsync, so it is Nothing
                ' until the operator asks for a synchronisation -- and here it failed with a
                ' NullReference before any request left. The ingest has no cancel button; when
                ' it gets one, the source is made here, next to it.
                propunere = Await WithReauth(Of PrelucrarePropunere)(
                    Function() coordonator.CerePropunereAsync(pachet, alegeri, CancellationToken.None))
            Finally
                busyBar.Running = False
            End Try

            ' Nothing = the operator gave up a unit choice. Nothing was written.
            If propunere Is Nothing Then Return False

            ' -- EMPTY BASKET = NO QUESTION (operator, 10.09.2026) ---------------------
            '
            ' The placement form exists for one thing only: the snapshots the machine CANNOT
            ' place by itself (F9 -- the automatic pass places only the LAST snapshot of a
            ' chain). When the download leaves none -- and that happens often, especially on
            ' partial refreshes, which bring no history at all -- the window would open with an
            ' empty basket and the button already lit, i.e. it would ask the operator to press
            ' to confirm they have nothing to confirm. It is saved directly.
            '
            ' THE QUESTION IS THE SAME, asked through THE SAME function as in the form
            ' (`AsociereForm.NehotarateDin`), over THE SAME picture; two counts written
            ' separately would drift apart, and the drift would show as a silent save where
            ' the operator should have been asked.
            Dim stare As AsociereStare = AsociereStare.DinPropunere(propunere)
            Dim faraMutari As New Dictionary(Of Integer, Integer)()
            Dim faraIgnorate As New Dictionary(Of Integer, Boolean)()

            If AsociereForm.NehotarateDin(stare.Instantanee, faraMutari, faraIgnorate) = 0 Then
                If Not Await SalveazaFaraMachetaAsync(cod, stare, propunere, pachet, alegeri) Then Return False
            Else
                Using f As New AsociereForm(_apiClient, cod, propunere, pachet, alegeri,
                                            Function(op) WithReauth(Of PrelucrareRaspuns)(op))
                    f.ShowDialog(Me)
                    If Not f.SAuSalvatModificari Then Return False
                End Using
            End If

            ' Written: the tree has other Are* flags (istoric, receptii, plati appear now), and
            ' the open view shows old figures. Both are re-read -- BUT the node stays selected
            ' (operator, 10.09.2026): a reload that clears the selection throws the operator
            ' back to the top of the list right after finishing work on an angajament, and the
            ' open view closes with it.
            Await LoadTreeAsync(pastreazaSelectia:=True)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DuLaIngestieAsync", ex)
            KBotMessage.Show(Me, "Ingestia descărcării a eșuat: " & ex.Message & Environment.NewLine &
                            "Pachetul a rămas în «WorkflowResults».",
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' PHASE TWO without the operator (slice 0060): resends the package with the decisions that
    ''' read themselves off the proposal. Called ONLY when there is nothing left to place.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>It is not a shortcut around the operator.</b> The server asks for a decision on
    ''' EVERY snapshot to place (400 on incomplete coverage), precisely so that silence cannot
    ''' mean «ignore it». Here that set is empty, so the decision list holds exactly the rows the
    ''' server placed itself in phase one, with the receptie it put them on. No choice is
    ''' invented; what was not to be chosen is confirmed.</para>
    ''' <para><b>The same empty dictionaries as in the count.</b> «No move, no ignore, no delete
    ''' set by the operator» is exactly the state the form has the moment it opens, so
    ''' <c>DeciziiDin</c> produces what it would have produced had the operator pressed «Salveaza»
    ''' without touching anything (F18).</para>
    ''' <para>Returns False when nothing was written: either the operator gave up a unit choice,
    ''' or the save failed -- and then the tree is NOT reloaded, having nothing new to show.</para>
    ''' </remarks>
    Private Async Function SalveazaFaraMachetaAsync(cod As String,
                                                    stare As AsociereStare,
                                                    propunere As PrelucrarePropunere,
                                                    pachet As PrelucrareRezultat,
                                                    alegeri As List(Of AlegereUnitate)) As Task(Of Boolean)
        Try
            Dim faraMutari As New Dictionary(Of Integer, Integer)()
            Dim faraIgnorate As New Dictionary(Of Integer, Boolean)()
            Dim faraStergeri As New Dictionary(Of Integer, Boolean)()
            Dim decizii As List(Of DecizieAsociere) =
                AsociereForm.DeciziiDin(stare.Instantanee, stare.Receptii,
                                        faraMutari, faraIgnorate, faraStergeri)

            Dim coordonator As New PrelucrareCoordinator(_apiClient)
            Dim raspuns As PrelucrareRaspuns
            busyBar.Running = True
            Try
                raspuns = Await WithReauth(Of PrelucrareRaspuns)(
                    Function() coordonator.SalveazaAsync(pachet, propunere.Amprenta, decizii,
                                                         alegeri, CancellationToken.None))
            Finally
                busyBar.Running = False
            End Try

            If raspuns Is Nothing Then
                ' The operator gave up a unit choice: the server rolled back.
                KBotMessage.Show(Me,
                    $"Salvarea descărcării lui «{cod}» s-a oprit la o alegere de unitate — " &
                    "nu s-a scris nimic. Reluați descărcarea când doriți.",
                    "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            ' The SERVER's figures, not a retelling: the same sentence the form shows after a
            ' save, so the operator reads the same thing on both roads. `KBotMessage.Show` also
            ' writes it to Logs\mesaje_operator.log.
            KBotMessage.Show(Me, AsociereForm.TextDupaSalvare(raspuns),
                            $"K-BOT — Descărcarea lui «{cod}»", MessageBoxButtons.OK,
                            If(raspuns.Avertismente.Count > 0,
                               MessageBoxIcon.Warning, MessageBoxIcon.Information))
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.SalveazaFaraMachetaAsync", ex)
            KBotMessage.Show(Me, $"Descărcarea lui «{cod}» nu a putut fi salvată: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.SalveazaFaraMachetaAsync", ex)
            KBotMessage.Show(Me, $"Descărcarea lui «{cod}» nu a putut fi salvată: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Asks the operator WHICH RECEPTII to read again and returns the dates of those NOT ticked
    ''' (slice 0060). An empty list means «all»; <c>Nothing</c> means «gave up, download
    ''' nothing».
    ''' </summary>
    ''' <remarks>
    ''' <para><b>The list is read from the server, not from the open view.</b> The refresh can be
    ''' asked for from any node, not only the one whose view is on screen, and a list taken from
    ''' the view would be empty exactly then -- or, worse, belong to ANOTHER angajament.</para>
    ''' <para><b>A failed read does NOT stop the download.</b> Without a list nobody can be
    ''' asked, and the question saves time, it is not a precondition: everything is downloaded,
    ''' and the reason is said on the console. The same reasoning as the coordinator's
    ''' <c>UltimaDataExtras</c>.</para>
    ''' <para>An angajament with no local receptie opens nothing: it would have nothing to show,
    ''' and an empty window with a «Descarca» button is a question with no substance.</para>
    ''' </remarks>
    Private Async Function AlegeReceptiileDeSaritAsync(cod As String) As Task(Of List(Of Date))
        Try
            If String.IsNullOrWhiteSpace(cod) Then Return New List(Of Date)()

            Dim info As ReceptiiInfo
            busyBar.Running = True
            Try
                info = Await WithReauth(Of ReceptiiInfo)(
                    Function() _apiClient.GetReceptiiAsync(cod, CancellationToken.None))
            Finally
                busyBar.Running = False
            End Try

            Dim randuri As List(Of ReceptieRow) = info?.Receptii
            If randuri Is Nothing OrElse randuri.Count = 0 Then Return New List(Of Date)()

            Using dlg As New SelectieReceptiiForm(randuri, cod)
                If dlg.ShowDialog(Me) <> DialogResult.OK Then Return Nothing
                If dlg.DateDeSarit.Count > 0 Then
                    _controller.SpuneStare($"«{cod}»: {dlg.DateDeSarit.Count} zile de recepții sar " &
                                       "peste citirea detaliului (alegerea operatorului).")
                End If
                Return New List(Of Date)(dlg.DateDeSarit)
            End Using
        Catch ex As Exception
            ' UI boundary: without a list EVERYTHING is downloaded. Never the other way round --
            ' silently skipping receptii after a failed read would be exactly the decision the
            ' machine is not allowed to take.
            GlobalErrorLog.Write("MainForm.AlegeReceptiileDeSaritAsync", ex)
            _controller.SpuneStare($"Nu s-a putut citi lista de recepții a lui «{cod}» ({ex.Message}) — " &
                               "se descarcă toate.")
            Return New List(Of Date)()
        End Try
    End Function

    ''' <summary>
    ''' Refreshes ONLY the receptii of the given angajament -- the right footer icon of the
    ''' <c>ReceptiiView</c> tree (slice 0060).
    ''' </summary>
    ''' <remarks>
    ''' Goes through THE SAME two phases as downloading a whole node: the package goes to the
    ''' proposal, and the placement (or its absence) decides whether the form opens. A partial
    ''' flow may not bypass that -- its history is empty, so usually nothing is left to decide
    ''' and the save happens by itself, but the road is one.
    ''' </remarks>
    Private Async Sub ReimprospateazaReceptii(cod As String)
        Try
            If String.IsNullOrWhiteSpace(cod) Then Return

            Dim sarite As List(Of Date) = Await AlegeReceptiileDeSaritAsync(cod)
            If sarite Is Nothing Then Return   ' gave up

            Dim pachet As PrelucrareRezultat
            busyBar.Running = True
            Try
                pachet = Await _controller.DownloadReceptiiAsync(cod, sarite)
            Finally
                busyBar.Running = False
            End Try
            If pachet Is Nothing Then
                AratEsecul("Reîmprospătarea recepțiilor")
                Return
            End If

            Await DuLaIngestieAsync(cod, pachet)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ReimprospateazaReceptii", ex)
            KBotMessage.Show(Me, "Reîmprospătarea recepțiilor a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Refreshes ONLY the reservations of the given angajament -- the right footer icon of the
    ''' <c>RezervariView</c> tree (slice 0060).
    ''' </summary>
    ''' <remarks>
    ''' Brings the header, the indicators and the history, because <c>FX_Rezervari</c> is written
    ''' on the server FROM <c>FX_Istoric</c>. No question is asked about receptii: the package
    ''' holds none, so there would be nothing to answer. The fresh history, however, CAN bring
    ''' snapshots to place -- then the form opens, as after any download.
    ''' </remarks>
    Private Async Sub ReimprospateazaRezervari(cod As String)
        Try
            If String.IsNullOrWhiteSpace(cod) Then Return

            Dim pachet As PrelucrareRezultat
            busyBar.Running = True
            Try
                pachet = Await _controller.DownloadRezervariAsync(cod)
            Finally
                busyBar.Running = False
            End Try
            If pachet Is Nothing Then
                AratEsecul("Reîmprospătarea rezervărilor")
                Return
            End If

            Await DuLaIngestieAsync(cod, pachet)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ReimprospateazaRezervari", ex)
            KBotMessage.Show(Me, "Reîmprospătarea rezervărilor a eșuat: " & ex.Message,
                            "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Shows why the last thing asked of the robot came back empty -- and STAYS QUIET when the
    ''' operator is the one who gave up (slice 0057: a cancel leaves <c>LastFailure</c> empty,
    ''' deliberately).
    ''' </summary>
    Private Sub AratEsecul(cePorneam As String)
        Dim motiv As String = _controller.LastFailure
        If String.IsNullOrWhiteSpace(motiv) Then Return
        KBotMessage.Show(Me, cePorneam & " nu a adus nimic: " & motiv,
                        "FOREXE", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub
End Class
