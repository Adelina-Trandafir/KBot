Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain

''' <summary>
''' THE DDF EDITOR (slice 0051) -- the command dispatch and the ways into the editor (slice 0086
''' split out of KbotForm.vb). The deletes and the refresh after a write are in
''' KbotForm.DdfDelete.vb; the send to forexecab and the Rezervari footer follow-ups are in
''' KbotForm.DdfSend.vb.
''' </summary>
''' <remarks>
''' The same shape as <c>ExecutaComandaOrd</c>: the view asks, the shell executes. Every call
''' goes through <c>WithReauth</c>, which is private and generic here, so the re-login policy
''' stays in one place.
''' </remarks>
Partial Public Class KbotForm

    ''' <summary>
    ''' Executes one write command of the fundamentation document.
    '''
    ''' <para><c>Adauga</c> and <c>AdaugaRevizieInitiala</c> are asked for by the "+" icon on
    ''' the RESERVATIONS tree (<c>RezervariView.Tree_RightIconClicked</c>) -- which is where
    ''' Access asked for them too: <c>fxRezervari_AdaugaRevizie</c> in <c>frmFX_MAIN</c>.
    ''' Which of the two is decided there, from the reservation's <c>EInitiala</c>, exactly as
    ''' <c>cNode.Value2</c> decided it in <c>frmFX_MAIN_REZ</c>. They had no caller when slice
    ''' 0051 shipped (decision D20); they have one now.</para>
    ''' </summary>
    Private Async Sub ExecutaComandaDdf(comanda As DdfComanda)
        Try
            If comanda Is Nothing OrElse String.IsNullOrWhiteSpace(comanda.Cod) Then Return

            Select Case comanda.Actiune
                Case DdfActiune.Adauga
                    Await AdaugaDdfAsync(comanda.Cod, rev0:=False).ConfigureAwait(True)
                Case DdfActiune.AdaugaRevizieInitiala
                    Await AdaugaDdfAsync(comanda.Cod, rev0:=True).ConfigureAwait(True)
                Case DdfActiune.Modifica
                    Await ModificaDdfAsync(comanda.Revizie).ConfigureAwait(True)
                Case DdfActiune.StergeRevizie
                    Await StergeRevizieDdfAsync(comanda.Revizie).ConfigureAwait(True)
                Case DdfActiune.Sterge
                    Await StergeDocumentDdfAsync(comanda.Revizie).ConfigureAwait(True)
                Case DdfActiune.StergeLuna
                    Await StergeLunaDdfAsync(comanda).ConfigureAwait(True)
                Case DdfActiune.StergeToate
                    Await StergeToateReviziileDdfAsync(comanda).ConfigureAwait(True)
                Case DdfActiune.Trimite
                    ' Slice 0098: a send drives the robot, so it waits its turn in the robot queue.
                    Await _robotQueue.RunAsync(Nothing, $"Trimitere DDF «{comanda.Cod}»",
                                               Function() TrimiteDdfAsync(comanda.Cod, comanda.Revizie)).ConfigureAwait(True)
                Case DdfActiune.FinalizeazaPdf
                    Await FinalizeazaPdfDupaSemnareBAsync(comanda.Cod, comanda.Revizie).ConfigureAwait(True)
                Case Else
                    ' No silent no-ops: an unknown action is a programming defect.
                    Throw New ArgumentException($"Acțiune DDF necunoscută: {comanda.Actiune}", NameOf(comanda))
            End Select
        Catch ex As RobotTaskDroppedException
            ' Taken out of the robot queue by the operator: the console already said it.
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.ExecutaComandaDdf", ex)
            KBotMessage.Show(Me, ex.Message, "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ExecutaComandaDdf", ex)
            KBotMessage.Show(Me, "Comanda nu a putut fi executată. Detalii în jurnalul de erori.",
                            "Document de fundamentare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' «Adauga» -- generates the graph on the server (NOTHING is written) and opens the
    ''' editor. The port of <c>FX_Adaugare_DDF</c>.
    '''
    ''' <para><paramref name="rev0"/> selects the HEADER treatment: the initial revision, which
    ''' also creates the document, or a subsequent one on a document that exists. It does NOT
    ''' select the line source -- the server takes that from the data, and refuses loudly when
    ''' neither source has rows.</para>
    '''
    ''' <para>Called from the "+" icon on the reservations tree; see
    ''' <see cref="ExecutaComandaDdf"/>.</para>
    ''' </summary>
    Private Async Function AdaugaDdfAsync(cod As String, rev0 As Boolean) As Task
        busyBar.Running = True
        Dim draft As DdfDraft
        Try
            draft = Await WithReauth(Of DdfDraft)(
                Function() _apiClient.GenereazaDdfAsync(cod, rev0, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        ' Operator, 26.09.2026: the program of a revision added from the Rezervari tree is the
        ' angajament's -- FX_Indicatori.SS -> DefaProgram -- not the one /genereaza copied (the
        ' last revision's, or the fixed default for revision 0). The SSs are the selected node's.
        Dim surse As String = String.Empty
        If _currentInfo IsNot Nothing AndAlso
           String.Equals(_currentInfo.CodAngajament, cod, StringComparison.OrdinalIgnoreCase) Then
            surse = If(_currentInfo.Surse, String.Empty)
        End If
        Await DeschideEditorulDdfAsync(draft, programDinIndicatori:=True, surseIndicatori:=surse)
    End Function

    ''' <summary>
    ''' «Modifica» -- loads the selected revision in the editor's shape and opens it.
    '''
    ''' <para>Read through <c>GET /api/forexe/ddf/draft/{iddf}/{idrev}</c>, NOT through the
    ''' view's call: that one is the read-only view's and chooses its columns deliberately --
    ''' it returns neither the section-A keys and classifications, nor the section-B rows, nor
    ''' the attachment rows, all of which editing needs.</para>
    ''' </summary>
    Private Async Function ModificaDdfAsync(revizie As RevizieRow) As Task
        If revizie Is Nothing OrElse revizie.Idrev <= 0 OrElse revizie.Iddf <= 0 Then
            KBotMessage.Show(Me, "Selectați o revizie din arbore.", "Document de fundamentare",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Slice 0081-02: only a revision not sent yet (S0 / S1) is edited (D4). Once forexecab has
        ' it, any change is a NEW revision -- «Adauga rezervare» in the Rezervari tree.
        Dim stare As DdfRevisionState = revizie.Stare
        If Not DdfRevisionStates.CanEdit(stare) Then
            KBotMessage.Show(Me, $"Revizia este «{DdfRevisionStates.Label(stare)}» și nu se mai modifică." &
                            vbCrLf & vbCrLf & "Orice schimbare de valori cere o revizie nouă " &
                            "(«Adaugă rezervare» din subsolul arborelui Rezervări).",
                            "Document de fundamentare", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        busyBar.Running = True
        Dim draft As DdfDraft
        Try
            Dim iddf As Integer = revizie.Iddf
            Dim idrev As Integer = revizie.Idrev
            draft = Await WithReauth(Of DdfDraft)(
                Function() _apiClient.GetDdfDraftAsync(iddf, idrev, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try

        Await DeschideEditorulDdfAsync(draft, stare)
    End Function

    ''' <summary>
    ''' Slice 0081-02 -- «Angajament nou»: a new, K-BOT-only angajament («!» code), revision 0,
    ''' section A empty. The port of Access's <c>FX_Adaugare_ANG</c>; forexecab hears of it only at
    ''' «Trimite in FOREXE». Slice 0087: reached from the header menu (<c>menuNou</c>), no longer
    ''' from its own button.
    ''' </summary>
    Private Async Sub DeschideAngajamentNou()
        Try
            If String.IsNullOrWhiteSpace(_session.DbName) Then
                KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                                "Angajament nou", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim draft As DdfDraft = DdfDraftFactory.ForNewAngajament(
                DdfDraftFactory.NewManualCode(), _session.DbName, _session.CodProgram, Date.Today)
            Await DeschideEditorulDdfAsync(draft, DdfRevisionState.Draft)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideAngajamentNou", ex)
            KBotMessage.Show(Me, "Documentul pentru angajamentul nou nu a putut fi deschis. Detalii în jurnalul de erori.",
                            "Angajament nou", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0081-02 -- the option picked in the Rezervari footer menu. UI boundary (Async Sub
    ''' handed to the view): every failure is logged and shown, never thrown.
    ''' </summary>
    Private Async Sub ExecutaMeniulRezervari(optiune As RezervariMenuOption, info As AngajamentTreeInfo)
        Try
            Select Case optiune
                Case RezervariMenuOption.AdaugaRezervare
                    Await AdaugaRezervareDdfAsync(info, cuIndicatori:=False).ConfigureAwait(True)
                Case RezervariMenuOption.AdaugaRezervareCuIndicatori
                    Await AdaugaRezervareDdfAsync(info, cuIndicatori:=True).ConfigureAwait(True)
                Case RezervariMenuOption.Definitiveaza
                    Await _robotQueue.RunAsync(Nothing, $"Definitivare «{info?.CodAngajament}»",
                                               Function() SchimbaStareaAngajamentuluiAsync(info, definitivare:=True)).ConfigureAwait(True)
                Case RezervariMenuOption.Deruleaza
                    Await _robotQueue.RunAsync(Nothing, $"Derulare «{info?.CodAngajament}»",
                                               Function() SchimbaStareaAngajamentuluiAsync(info, definitivare:=False)).ConfigureAwait(True)
                Case RezervariMenuOption.GenereazaPdfFinal
                    Await GenereazaPdfFinalDinMeniuAsync(info).ConfigureAwait(True)
                Case Else
                    ' No silent no-ops: an option with no handler is a programming defect.
                    Throw New ArgumentException($"Opțiune de meniu fără acțiune: {optiune}", NameOf(optiune))
            End Select
        Catch ex As RobotTaskDroppedException
            ' Taken out of the robot queue by the operator: the console already said it.
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.ExecutaMeniulRezervari", ex)
            KBotMessage.Show(Me, ex.Message, "Document de fundamentare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ExecutaMeniulRezervari", ex)
            KBotMessage.Show(Me, "Acțiunea nu a putut fi executată. Detalii în jurnalul de erori.",
                            "Document de fundamentare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0081-02 -- «Adauga rezervare» from the Rezervari footer menu: a NEW revision in
    ''' manual mode, section A empty. On an angajament with a document it continues that document
    ''' (header of the last revision); without one it opens its revision 0 (the carried-over case).
    ''' The menu offers it only when no revision is open (plan 0081-04); the check is repeated
    ''' here against fresh data, because the tree may be minutes old.
    ''' <para><paramref name="cuIndicatori"/> (operator, 26.09.2026, «2. Foloseste indicatorii
    ''' existenti»): section A starts with a line, value 0, for every indicator the angajament
    ''' already has; the editor builds them once the classification list is here.</para>
    ''' </summary>
    Private Async Function AdaugaRezervareDdfAsync(info As AngajamentTreeInfo, cuIndicatori As Boolean) As Task
        If info Is Nothing OrElse String.IsNullOrWhiteSpace(info.CodAngajament) Then Return
        Dim cod As String = info.CodAngajament

        busyBar.Running = True
        Dim draft As DdfDraft
        Try
            Dim ddf As DdfInfo = Await WithReauth(Of DdfInfo)(
                Function() _apiClient.GetDdfAsync(cod, CancellationToken.None))
            Dim revizii As List(Of RevizieRow) = If(ddf?.Revizii, New List(Of RevizieRow)())
            Dim deschisa As RevizieRow = revizii.FirstOrDefault(Function(r) DdfRevisionStates.IsOpen(r.Stare))
            If deschisa IsNot Nothing Then
                KBotMessage.Show(Me, $"Revizia {deschisa.NumarRev} este încă «{DdfRevisionStates.Label(deschisa.Stare)}»." &
                                vbCrLf & vbCrLf & "Se lucrează la o singură revizie odată: termin-o întâi din vederea DDF.",
                                "Adaugă rezervare", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            If revizii.Count = 0 Then
                draft = DdfDraftFactory.ForFirstRevisionOfExisting(
                    cod, _session.DbName, _session.CodProgram, info.Descriere, info.DataCreare, info.Stare, Date.Today)
            Else
                Dim ultima As RevizieRow = revizii.OrderByDescending(Function(r) r.NumarRev).First()
                Dim iddf As Integer = ultima.Iddf
                Dim idrev As Integer = ultima.Idrev
                Dim sursa As DdfDraft = Await WithReauth(Of DdfDraft)(
                    Function() _apiClient.GetDdfDraftAsync(iddf, idrev, CancellationToken.None))
                draft = DdfDraftFactory.ForAddedReservation(sursa, Date.Today)
            End If
        Finally
            busyBar.Running = False
        End Try

        ' Both entries (empty revision and prefilled one): the program comes from the indicators
        ' (FX_Indicatori.SS -> DefaProgram), never from the last revision or the session -- operator,
        ' 26.09.2026. The editor maps the node's sources once it has the DefaProgram map.
        Await DeschideEditorulDdfAsync(draft, DdfRevisionState.Draft, cuIndicatori, If(info.Surse, String.Empty),
                                       programDinIndicatori:=True)
    End Function

    ''' <summary>
    ''' Opens the editor as a page of the main window and, when it closes, on a save reloads the view onto what
    ''' was written. Returns when the editor has closed (what ShowDialog used to do).
    ''' </summary>
    Private Async Function DeschideEditorulDdfAsync(draft As DdfDraft, Optional stare As DdfRevisionState = DdfRevisionState.Draft,
                                                    Optional cuIndicatori As Boolean = False,
                                                    Optional surseIndicatori As String = "",
                                                    Optional programDinIndicatori As Boolean = False) As Task
        If draft Is Nothing Then Return
        ' Slice 0081-02: a brand-new K-BOT angajament is not in the tree yet; after its save the
        ' tree is reloaded ONTO it, so the operator lands on what they just created.
        Dim angajamentNou As Boolean = draft.Nou AndAlso draft.Manual

        ' Seven specialisations of the 401 net, named rather than positional: `WithReauth` is
        ' private and generic, so the form needs one closure per response shape, and seven
        ' identical-looking delegates in a row is an argument order nobody gets right twice.
        Dim reauth As New DdfEditReauth(
            Function(op) WithReauth(Of DdfSaveRezultat)(op),
            Function(op) WithReauth(Of PutDdfFisierResponse)(op),
            Function(op) WithReauth(Of PdfDownloadResult)(op),
            Function(op) WithReauth(Of DdfNumarLock)(op),
            Function(op) WithReauth(Of List(Of String))(op),
            Function(op) WithReauth(Of List(Of DdfPartener))(op),
            Function(op) WithReauth(Of List(Of DdfClasificatie))(op),
            Function(op) WithReauth(Of List(Of DdfSursaProgram))(op))

        ' Slice 0081-09: a section-A line picks its SS among those of the document's program
        ' (AVACONT_COMUN.DefaProgram, read by the editor); the SS chosen here is the one proposed.
        Using f As New DdfEditForm(_apiClient, draft, reauth, stare, TryCast(_apiClient, IDdfSendApi)) With {
                .SursaSectorSesiune = If(_session.SectorSursa, String.Empty),
                .PornesteCuIndicatorii = cuIndicatori,
                .SurseIndicatori = If(surseIndicatori, String.Empty),
                .ProgramDinIndicatori = programDinIndicatori}
            If Not Await ShowModalAsync(f, CodRef(MachetaDdfEdit, draft.CodAngajament)) Then Return
            If f.SAuSalvatModificari AndAlso angajamentNou Then
                ReincarcaArborelePe(draft.CodAngajament)
            ElseIf f.SAuSalvatModificari Then
                ' What is still on screen is no longer true: the lines changed, the reservations
                ' consumed changed, and a new revision was not there at all.
                ' `IddfSalvat` is the key the server returned, so it also answers "does this
                ' angajament have a document now?" -- which is what the «ddf» nav entry asks.
                DupaScriereaDdf(f.IdrevSalvat, draft.CodAngajament,
                                iddfDocument:=f.IddfSalvat)
            End If
        End Using
    End Function

    ''' <summary>
    ''' The Rezervari footer RIGHT icon, «Repară» (operator, 12.10.2026; it replaced the refresh from
    ''' FOREXE): the same dry-run / confirm / write flow as the footer menu's «Reanalizează rezervările».
    ''' UI boundary (Async Sub handed to the view): every failure is logged and shown, never thrown.
    ''' </summary>
    Private Async Sub ReparaRezervari(cod As String, greseli As IReadOnlyList(Of RezervareGresita))
        Try
            ' Slice 0114-26 (operator, 23.10.2026): the same button also frees the reservations linked to a revision
            ' that does not cover them. That goes FIRST: such a reservation counts as linked to a revision, and a
            ' chain repair refuses the months that have linked reservations.
            Dim k_info As AngajamentTreeInfo = Nothing
            _treeInfos.TryGetValue(cod, k_info)
            Dim k_areLegaturi As Boolean = k_info IsNot Nothing AndAlso k_info.LegaturiGresite.Count > 0
            If k_areLegaturi Then
                If Not Await ReparaLegaturiAsync(cod).ConfigureAwait(True) Then Return
            End If
            If Not k_areLegaturi OrElse (greseli IsNot Nothing AndAlso greseli.Count > 0) Then
                Await ReanalizeazaRezervariAsync(cod, greseli).ConfigureAwait(True)
            End If
        Catch ex As ApiException
            GlobalErrorLog.Write("MainForm.ReparaRezervari", ex)
            KBotMessage.Show(Me, ex.Message, "Reanalizarea rezervărilor", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ReparaRezervari", ex)
            KBotMessage.Show(Me, "Rezervările nu au putut fi reparate. Detalii în jurnalul de erori.",
                            "Reanalizarea rezervărilor", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0114-26: frees the reservations of the angajament that are linked to a DDF revision which does not cover
    ''' them, so each gets its own revision and the «+» of the Rezervari tree comes back on it. Dry run first (the
    ''' server lists what it would free), the operator confirms, then it writes and the tree reloads with the
    ''' selection kept. Returns False when the operator said no or the call failed (nothing else should follow), True
    ''' when it is done or there was nothing to do. Called from <see cref="ReparaRezervari"/>, which logs and shows
    ''' the exceptions.
    ''' </summary>
    Private Async Function ReparaLegaturiAsync(cod As String) As Task(Of Boolean)
        If String.IsNullOrWhiteSpace(cod) Then Return False
        Const k_titlu As String = "Rezervări legate greșit"
        Dim k_api As IRezervariLegaturiApi = TryCast(_apiClient, IRezervariLegaturiApi)
        If k_api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IRezervariLegaturiApi.")

        Dim k_proba As RezervariLegaturiResult
        busyBar.Running = True
        Try
            k_proba = Await WithReauth(Of RezervariLegaturiResult)(
                Function() k_api.RepairReservationLinksAsync(cod, False, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try
        If k_proba Is Nothing Then
            AratEsecul(k_titlu)
            Return False
        End If

        If k_proba.ToRepair = 0 Then
            ' The tree row said otherwise: what it held is out of date, so it is read again.
            KBotMessage.Show(Me, $"«{cod}»: nicio rezervare nu mai e legată greșit de o revizie. Nu este nimic de reparat.",
                             k_titlu, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Await LoadTreeAsync(pastreazaSelectia:=True)
            Return True
        End If

        Dim k_ro As New Globalization.CultureInfo("ro-RO")
        Dim k_linii As New List(Of String)()
        For Each k_d As RezervareLegataGresit In k_proba.Details.Take(10)
            Dim k_zi As String = If(k_d.DataRevizie.HasValue, $" din {k_d.DataRevizie.Value:dd.MM.yyyy}", String.Empty)
            k_linii.Add($"  • rezervarea din {k_d.DataRezervare:dd.MM.yyyy}, {k_d.CodIndicator}: " &
                        $"{k_d.Valoare.ToString("+#,##0.00;-#,##0.00;0.00", k_ro)}, legată de revizia {k_d.IdRevizie}{k_zi}")
        Next
        If k_proba.ToRepair > k_linii.Count Then k_linii.Add($"  • … și încă {k_proba.ToRepair - k_linii.Count}")
        Dim k_intrebare As String =
            $"«{cod}»: {k_proba.ToRepair} " & If(k_proba.ToRepair = 1, "rezervare e legată", "rezervări sunt legate") &
            " de o revizie care nu " & If(k_proba.ToRepair = 1, "o cuprinde", "le cuprinde") & ", deci nu " &
            If(k_proba.ToRepair = 1, "are", "au") & " «+» și nu " & If(k_proba.ToRepair = 1, "își poate", "își pot") &
            " primi revizia:" & vbCrLf & vbCrLf & String.Join(vbCrLf, k_linii) & vbCrLf & vbCrLf &
            "Repararea le scoate din acea revizie și le lasă fără revizie, ca să primească «+». " &
            "Sumele și reviziile nu se schimbă. Continuați?"
        If KBotMessage.Show(Me, k_intrebare, k_titlu, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then
            Return False
        End If

        Dim k_rezultat As RezervariLegaturiResult
        busyBar.Running = True
        Try
            k_rezultat = Await WithReauth(Of RezervariLegaturiResult)(
                Function() k_api.RepairReservationLinksAsync(cod, True, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try
        If k_rezultat Is Nothing Then
            AratEsecul(k_titlu)
            Return False
        End If

        KBotMessage.Show(Me, $"«{cod}»: {k_rezultat.Repaired} " & If(k_rezultat.Repaired = 1, "rezervare a rămas", "rezervări au rămas") &
                         " fără revizie și o pot primi acum (apăsați «+» în vederea «Rezervări»)." &
                         If(k_rezultat.PicturesRemoved > 0, $" S-au scos {k_rezultat.PicturesRemoved} capturi de pe revizia greșită.", String.Empty),
                         k_titlu, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Await LoadTreeAsync(pastreazaSelectia:=True)
        Return True
    End Function

    ''' <summary>
    ''' «Reanalizează rezervările» from the Rezervari footer menu: re-walks the FX_Istoric chain of
    ''' the angajament on the server (nothing is downloaded) and corrects Rez_Ord, TipRand,
    ''' Val_Rezervare_Ant/Dif and R_Anterioara/R_Valoare. Dry run first, so the operator sees the
    ''' real numbers before anything is written; the tree reloads with the selection kept.
    ''' </summary>
    Private Async Function ReanalizeazaRezervariAsync(cod As String, greseli As IReadOnlyList(Of RezervareGresita)) As Task
        If String.IsNullOrWhiteSpace(cod) Then Return
        Const titlu As String = "Reanalizarea rezervărilor"

        Dim proba As RezervariReanalizaResult
        busyBar.Running = True
        Try
            proba = Await WithReauth(Of RezervariReanalizaResult)(
                Function() _apiClient.ReanalyzeRezervariAsync(cod, False, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try
        If proba Is Nothing Then
            AratEsecul(titlu)
            Return
        End If

        If proba.NothingToDo Then
            KBotMessage.Show(Me, $"«{cod}»: rezervările sunt deja în acord cu istoricul. Nu este nimic de corectat." &
                             Semnalari(proba), titlu, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Slice 0114-25: a modal window shows what was detected and what the repair will write (it replaced a message
        ' box with eight examples); «Repară» = go on, «Renunță» = nothing is written.
        Dim raspuns As DialogResult
        Using dlg As New RezervariReparaForm(cod, greseli, proba)
            raspuns = dlg.ShowDialog(Me)
        End Using
        If raspuns <> DialogResult.OK Then Return

        Dim rezultat As RezervariReanalizaResult
        busyBar.Running = True
        Try
            rezultat = Await WithReauth(Of RezervariReanalizaResult)(
                Function() _apiClient.ReanalyzeRezervariAsync(cod, True, CancellationToken.None))
        Finally
            busyBar.Running = False
        End Try
        If rezultat Is Nothing Then
            AratEsecul(titlu)
            Return
        End If

        KBotMessage.Show(Me, $"«{cod}»: s-au corectat {rezultat.HistoryCorrected} rânduri de istoric și " &
                         $"{rezultat.ReservationsCorrected} rezervări.", titlu,
                         MessageBoxButtons.OK, MessageBoxIcon.Information)
        Await LoadTreeAsync(pastreazaSelectia:=True)
    End Function

    Private Shared Function Semnalari(r As RezervariReanalizaResult) As String
        If r Is Nothing OrElse r.Warnings.Count = 0 Then Return String.Empty
        Return vbCrLf & vbCrLf & "Semnalări:" & vbCrLf & "  • " & String.Join(vbCrLf & "  • ", r.Warnings)
    End Function
End Class
